"""Tests for OpenAIProvider — mocked SDK calls."""

from __future__ import annotations

import json
from collections.abc import AsyncIterator
from types import SimpleNamespace
from unittest.mock import AsyncMock, MagicMock, patch

import pytest

from heagent.providers.base import BaseProvider
from heagent.providers.openai import OpenAIProvider, _parse_tool_calls, _to_openai_messages, _to_openai_tools
from heagent.types import Message, Role, ToolSchema


def _mock_usage(p: int = 10, c: int = 5, t: int = 15) -> SimpleNamespace:
    return SimpleNamespace(prompt_tokens=p, completion_tokens=c, total_tokens=t)


def _mock_choice(content: str = "hello", finish_reason: str = "stop", tool_calls: list[object] | None = None) -> SimpleNamespace:
    return SimpleNamespace(message=SimpleNamespace(content=content, tool_calls=tool_calls), finish_reason=finish_reason)


def _mock_response(content: str = "hello", model: str = "gpt-4o", finish_reason: str = "stop", tool_calls: list[object] | None = None, usage: object | None = None) -> SimpleNamespace:
    return SimpleNamespace(choices=[_mock_choice(content, finish_reason, tool_calls)], model=model, usage=usage or _mock_usage())


def _mock_chunk(content: str = "hi", model: str = "gpt-4o", finish_reason: str = "", has_choices: bool = True, usage: object | None = None) -> SimpleNamespace:
    if not has_choices:
        return SimpleNamespace(choices=[], model=model, usage=usage)
    return SimpleNamespace(choices=[SimpleNamespace(delta=SimpleNamespace(content=content), finish_reason=finish_reason)], model=model, usage=usage)


def _mock_tool_call(tc_id: str = "call_1", name: str = "run", arguments: str = '{"cmd":"ls"}') -> SimpleNamespace:
    return SimpleNamespace(id=tc_id, function=SimpleNamespace(name=name, arguments=arguments))


class TestHelpers:
    def test_to_openai_messages(self) -> None:
        msgs = [Message(role=Role.USER, content="hi")]
        result = _to_openai_messages(msgs)
        assert result == [{"role": "user", "content": "hi"}]

    def test_to_openai_messages_with_tool_result(self) -> None:
        msgs = [Message(role=Role.TOOL, content="output", tool_call_id="tc1")]
        result = _to_openai_messages(msgs)
        assert result[0]["tool_call_id"] == "tc1"

    def test_to_openai_tools(self) -> None:
        tools = [ToolSchema(name="run", description="run cmd", parameters={"type": "object"})]
        result = _to_openai_tools(tools)
        assert result[0]["type"] == "function"

    def test_parse_tool_calls(self) -> None:
        raw = [_mock_tool_call()]
        result = _parse_tool_calls(raw)
        assert len(result) == 1
        assert result[0].name == "run"
        assert result[0].arguments == {"cmd": "ls"}

    def test_parse_tool_calls_bad_json(self) -> None:
        raw = [_mock_tool_call(arguments="not-json")]
        result = _parse_tool_calls(raw)
        assert result[0].arguments == {}


class TestProtocol:
    def test_satisfies_base_provider(self) -> None:
        p = OpenAIProvider(api_key="sk-test")
        assert isinstance(p, BaseProvider)


class TestSend:
    @patch("heagent.providers.openai.AsyncOpenAI")
    async def test_send_basic(self, mock_cls: MagicMock) -> None:
        mock_client = AsyncMock()
        mock_cls.return_value = mock_client
        mock_client.chat.completions.create = AsyncMock(return_value=_mock_response("Hello!"))

        p = OpenAIProvider(api_key="sk-test")
        resp = await p.send([Message(role=Role.USER, content="hi")])
        assert resp.content == "Hello!"
        assert resp.model == "gpt-4o"

    @patch("heagent.providers.openai.AsyncOpenAI")
    async def test_send_with_tools(self, mock_cls: MagicMock) -> None:
        mock_client = AsyncMock()
        mock_cls.return_value = mock_client
        mock_client.chat.completions.create = AsyncMock(
            return_value=_mock_response("", finish_reason="tool_calls", tool_calls=[_mock_tool_call()])
        )

        p = OpenAIProvider(api_key="sk-test")
        tools = [ToolSchema(name="run", description="run", parameters={"type": "object"})]
        resp = await p.send([Message(role=Role.USER, content="run ls")], tools=tools)
        assert resp.finish_reason == "tool_calls"
        assert len(resp.tool_calls) == 1

    @patch("heagent.providers.openai.AsyncOpenAI")
    async def test_send_custom_model(self, mock_cls: MagicMock) -> None:
        mock_client = AsyncMock()
        mock_cls.return_value = mock_client
        mock_client.chat.completions.create = AsyncMock(return_value=_mock_response("ok", model="gpt-4o-mini"))

        p = OpenAIProvider(api_key="sk-test", model="gpt-4o-mini")
        resp = await p.send([Message(role=Role.USER, content="hi")])
        assert resp.model == "gpt-4o-mini"


class TestStream:
    @patch("heagent.providers.openai.AsyncOpenAI")
    async def test_stream_yields_chunks(self, mock_cls: MagicMock) -> None:
        mock_client = AsyncMock()
        mock_cls.return_value = mock_client

        async def fake_stream(**kwargs: object) -> AsyncIterator[object]:
            for c in ["Hello", " World"]:
                yield _mock_chunk(content=c)

        mock_client.chat.completions.create = AsyncMock(side_effect=fake_stream)

        p = OpenAIProvider(api_key="sk-test")
        chunks = [c async for c in p.stream([Message(role=Role.USER, content="hi")])]
        assert len(chunks) == 2
        assert chunks[0].content == "Hello"


class TestMetadata:
    def test_metadata(self) -> None:
        p = OpenAIProvider(api_key="sk-test", model="gpt-4o")
        meta = p.get_metadata()
        assert meta.name == "openai"
        assert meta.supports_streaming is True

    @patch("heagent.providers.openai.AsyncOpenAI")
    def test_custom_base_url(self, mock_cls: MagicMock) -> None:
        OpenAIProvider(api_key="sk-test", base_url="http://localhost:1234/v1")
        mock_cls.assert_called_once_with(api_key="sk-test", base_url="http://localhost:1234/v1")
