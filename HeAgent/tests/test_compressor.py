"""Tests for context compressor."""

from __future__ import annotations

import pytest

from heagent.context.compressor import ContextCompressor
from heagent.providers.base import ProviderMetadata
from heagent.types import Message, ProviderResponse, Role, TokenUsage


class StubProvider:
    def __init__(self, summary: str = "summary") -> None:
        self._summary = summary

    async def send(self, messages: list[Message], **kw: object) -> ProviderResponse:
        return ProviderResponse(
            content=self._summary,
            usage=TokenUsage(prompt_tokens=5, completion_tokens=5, total_tokens=10),
            model="stub",
            finish_reason="stop",
        )

    async def stream(self, messages: list[Message], **kw: object) -> object:
        yield await self.send(messages)

    def get_metadata(self) -> ProviderMetadata:
        return ProviderMetadata(name="stub", model="stub")


def _msg(role: Role, content: str) -> Message:
    return Message(role=role, content=content)


@pytest.mark.asyncio
class TestContextCompressor:
    async def test_no_compress_below_threshold(self) -> None:
        comp = ContextCompressor(StubProvider(), threshold=0.8)
        msgs = [_msg(Role.USER, "hi"), _msg(Role.ASSISTANT, "hello")]
        result = await comp.compress(msgs, token_count=10, max_tokens=100)
        assert result is msgs

    async def test_compress_above_threshold(self) -> None:
        comp = ContextCompressor(StubProvider(), threshold=0.5, keep_recent=2)
        msgs = [
            _msg(Role.SYSTEM, "sys"),
            _msg(Role.USER, "q1"),
            _msg(Role.ASSISTANT, "a1"),
            _msg(Role.USER, "q2"),
            _msg(Role.ASSISTANT, "a2"),
            _msg(Role.USER, "q3"),
            _msg(Role.ASSISTANT, "a3"),
        ]
        result = await comp.compress(msgs, token_count=90, max_tokens=100)
        assert len(result) < len(msgs)
        assert result[0].content == "sys"
        assert "[Conversation summary]" in result[1].content
        assert result[-2].content == "q3"
        assert result[-1].content == "a3"

    async def test_keep_recent_too_few_messages(self) -> None:
        comp = ContextCompressor(StubProvider(), threshold=0.1, keep_recent=10)
        msgs = [_msg(Role.USER, "q"), _msg(Role.ASSISTANT, "a")]
        result = await comp.compress(msgs, token_count=90, max_tokens=100)
        assert result is msgs

    async def test_zero_max_tokens(self) -> None:
        comp = ContextCompressor(StubProvider())
        msgs = [_msg(Role.USER, "x")]
        result = await comp.compress(msgs, token_count=50, max_tokens=0)
        assert result is msgs

    async def test_summary_content_used(self) -> None:
        comp = ContextCompressor(StubProvider(summary="key fact: answer is 42"), threshold=0.1, keep_recent=2)
        msgs = [
            _msg(Role.USER, "q1"),
            _msg(Role.ASSISTANT, "a1"),
            _msg(Role.USER, "q2"),
            _msg(Role.ASSISTANT, "a2"),
        ]
        result = await comp.compress(msgs, token_count=90, max_tokens=100)
        assert "42" in result[0].content
