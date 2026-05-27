"""Tests for BaseProvider Protocol and ProviderMetadata."""

from __future__ import annotations

from collections.abc import AsyncIterator

import pytest

from heagent.providers.base import BaseProvider, ProviderMetadata
from heagent.types import Message, ProviderResponse, Role, TokenUsage, ToolSchema


# --- ProviderMetadata ---


class TestProviderMetadata:
    def test_create_metadata(self) -> None:
        m = ProviderMetadata(
            name="test-provider",
            model="test-model",
            supports_streaming=True,
            supports_tools=True,
        )
        assert m.name == "test-provider"
        assert m.model == "test-model"
        assert m.supports_streaming is True
        assert m.supports_tools is True

    def test_metadata_defaults(self) -> None:
        m = ProviderMetadata(name="p", model="m")
        assert m.supports_streaming is False
        assert m.supports_tools is False


# --- ToolSchema ---


class TestToolSchema:
    def test_create_tool_schema(self) -> None:
        ts = ToolSchema(
            name="run_shell",
            description="Execute a shell command",
            parameters={"type": "object", "properties": {"command": {"type": "string"}}},
        )
        assert ts.name == "run_shell"
        assert "command" in ts.parameters["properties"]


# --- Protocol structural subtyping ---


class TestProtocolStructuralTyping:
    def test_concrete_impl_satisfies_protocol(self) -> None:
        class FakeProvider:
            async def send(
                self,
                messages: list[Message],
                *,
                tools: list[ToolSchema] | None = None,
            ) -> ProviderResponse:
                return ProviderResponse(
                    content="hi",
                    usage=TokenUsage(prompt_tokens=1, completion_tokens=1, total_tokens=2),
                    model="fake",
                    finish_reason="stop",
                )

            async def stream(
                self,
                messages: list[Message],
                *,
                tools: list[ToolSchema] | None = None,
            ) -> AsyncIterator[ProviderResponse]:
                yield ProviderResponse(
                    content="hi",
                    usage=TokenUsage(prompt_tokens=1, completion_tokens=1, total_tokens=2),
                    model="fake",
                    finish_reason="stop",
                )

            def get_metadata(self) -> ProviderMetadata:
                return ProviderMetadata(name="fake", model="fake")

        provider: BaseProvider = FakeProvider()
        assert isinstance(provider, BaseProvider)

    async def test_send_returns_response(self) -> None:
        class FakeProvider:
            async def send(
                self,
                messages: list[Message],
                *,
                tools: list[ToolSchema] | None = None,
            ) -> ProviderResponse:
                return ProviderResponse(
                    content="ok",
                    usage=TokenUsage(prompt_tokens=1, completion_tokens=1, total_tokens=2),
                    model="test",
                    finish_reason="stop",
                )

            async def stream(
                self,
                messages: list[Message],
                *,
                tools: list[ToolSchema] | None = None,
            ) -> AsyncIterator[ProviderResponse]:
                yield ProviderResponse(
                    content="",
                    usage=TokenUsage(prompt_tokens=0, completion_tokens=0, total_tokens=0),
                    model="test",
                    finish_reason="stop",
                )

            def get_metadata(self) -> ProviderMetadata:
                return ProviderMetadata(name="test", model="test")

        p: BaseProvider = FakeProvider()
        msgs = [Message(role=Role.USER, content="hello")]
        resp = await p.send(msgs)
        assert resp.content == "ok"

    async def test_stream_yields_chunks(self) -> None:
        class FakeProvider:
            async def send(
                self,
                messages: list[Message],
                *,
                tools: list[ToolSchema] | None = None,
            ) -> ProviderResponse:
                return ProviderResponse(
                    content="",
                    usage=TokenUsage(prompt_tokens=0, completion_tokens=0, total_tokens=0),
                    model="test",
                    finish_reason="stop",
                )

            async def stream(
                self,
                messages: list[Message],
                *,
                tools: list[ToolSchema] | None = None,
            ) -> AsyncIterator[ProviderResponse]:
                yield ProviderResponse(
                    content="chunk1",
                    usage=TokenUsage(prompt_tokens=1, completion_tokens=1, total_tokens=2),
                    model="test",
                    finish_reason="stop",
                )
                yield ProviderResponse(
                    content="chunk2",
                    usage=TokenUsage(prompt_tokens=1, completion_tokens=1, total_tokens=2),
                    model="test",
                    finish_reason="stop",
                )

            def get_metadata(self) -> ProviderMetadata:
                return ProviderMetadata(name="test", model="test")

        p: BaseProvider = FakeProvider()
        msgs = [Message(role=Role.USER, content="hello")]
        chunks = [c async for c in p.stream(msgs)]
        assert len(chunks) == 2
        assert chunks[0].content == "chunk1"

    def test_get_metadata(self) -> None:
        class FakeProvider:
            async def send(
                self,
                messages: list[Message],
                *,
                tools: list[ToolSchema] | None = None,
            ) -> ProviderResponse:
                return ProviderResponse(
                    content="",
                    usage=TokenUsage(prompt_tokens=0, completion_tokens=0, total_tokens=0),
                    model="m",
                    finish_reason="stop",
                )

            async def stream(
                self,
                messages: list[Message],
                *,
                tools: list[ToolSchema] | None = None,
            ) -> AsyncIterator[ProviderResponse]:
                yield ProviderResponse(
                    content="",
                    usage=TokenUsage(prompt_tokens=0, completion_tokens=0, total_tokens=0),
                    model="m",
                    finish_reason="stop",
                )

            def get_metadata(self) -> ProviderMetadata:
                return ProviderMetadata(
                    name="fake",
                    model="gpt-4",
                    supports_streaming=True,
                    supports_tools=True,
                )

        p: BaseProvider = FakeProvider()
        meta = p.get_metadata()
        assert meta.name == "fake"
        assert meta.supports_streaming is True
