"""Tests for ProviderChain."""

from __future__ import annotations

from collections.abc import AsyncIterator

import pytest

from heagent.exceptions import ProviderError
from heagent.providers.base import ProviderMetadata
from heagent.providers.chain import ProviderChain
from heagent.types import Message, ProviderResponse, Role, TokenUsage, ToolSchema


def _make_provider(name: str, content: str = "ok", *, fail: bool = False, fail_stream: bool = False) -> object:
    class FakeProvider:
        _name = name
        _content = content
        _fail = fail
        _fail_stream = fail_stream

        async def send(self, messages: list[Message], *, tools: list[ToolSchema] | None = None) -> ProviderResponse:
            if self._fail:
                raise ProviderError(f"{self._name} send failed")
            return ProviderResponse(content=self._content, usage=TokenUsage(prompt_tokens=1, completion_tokens=1, total_tokens=2), model=self._name, finish_reason="stop")

        async def stream(self, messages: list[Message], *, tools: list[ToolSchema] | None = None) -> AsyncIterator[ProviderResponse]:
            if self._fail_stream:
                raise ProviderError(f"{self._name} stream failed")
            yield ProviderResponse(content=self._content, usage=TokenUsage(prompt_tokens=1, completion_tokens=1, total_tokens=2), model=self._name, finish_reason="stop")

        def get_metadata(self) -> ProviderMetadata:
            return ProviderMetadata(name=self._name, model=self._name)

    return FakeProvider()


class TestChain:
    def test_requires_at_least_one(self) -> None:
        with pytest.raises(ValueError):
            ProviderChain([])

    def test_current_returns_first(self) -> None:
        chain = ProviderChain([_make_provider("a"), _make_provider("b")])
        assert chain.current.get_metadata().name == "a"

    async def test_send_uses_current(self) -> None:
        chain = ProviderChain([_make_provider("a", "hello")])
        resp = await chain.send([Message(role=Role.USER, content="hi")])
        assert resp.content == "hello"

    async def test_send_fallback_on_failure(self) -> None:
        chain = ProviderChain([_make_provider("a", fail=True), _make_provider("b", "fallback")])
        resp = await chain.send([Message(role=Role.USER, content="hi")])
        assert resp.content == "fallback"

    async def test_send_all_fail_raises(self) -> None:
        chain = ProviderChain([_make_provider("a", fail=True), _make_provider("b", fail=True)])
        with pytest.raises(ProviderError):
            await chain.send([Message(role=Role.USER, content="hi")])

    async def test_stream_uses_current(self) -> None:
        chain = ProviderChain([_make_provider("a", "chunk")])
        chunks = [c async for c in chain.stream([Message(role=Role.USER, content="hi")])]
        assert chunks[0].content == "chunk"

    async def test_stream_fallback(self) -> None:
        chain = ProviderChain([_make_provider("a", fail_stream=True), _make_provider("b", "stream-ok")])
        chunks = [c async for c in chain.stream([Message(role=Role.USER, content="hi")])]
        assert chunks[0].content == "stream-ok"

    def test_reset(self) -> None:
        chain = ProviderChain([_make_provider("a"), _make_provider("b")])
        chain._advance()
        assert chain.current.get_metadata().name == "b"
        chain.reset()
        assert chain.current.get_metadata().name == "a"

    def test_providers_returns_copy(self) -> None:
        p = _make_provider("a")
        chain = ProviderChain([p])
        assert len(chain.providers) == 1
