"""Anthropic Claude provider implementation."""

from __future__ import annotations

from collections.abc import AsyncIterator

from anthropic import AsyncAnthropic

from heagent.providers.base import ProviderMetadata
from heagent.types import Message, ProviderResponse, Role, TokenUsage, ToolCall, ToolSchema


def _to_anthropic_messages(messages: list[Message]) -> list[dict[str, object]]:
    result: list[dict[str, object]] = []
    for msg in messages:
        if msg.role == Role.SYSTEM:
            continue
        d: dict[str, object] = {"role": msg.role.value, "content": msg.content}
        if msg.role == Role.TOOL:
            d["role"] = "user"
            d["content"] = f"[Tool result {msg.tool_call_id}]: {msg.content}"
        if msg.tool_calls:
            blocks: list[dict[str, object]] = []
            for tc in msg.tool_calls:
                blocks.append({"type": "tool_use", "id": tc.id, "name": tc.name, "input": tc.arguments})
            d["content"] = blocks
        result.append(d)
    return result


def _extract_system(messages: list[Message]) -> str:
    parts = [m.content for m in messages if m.role == Role.SYSTEM]
    return "\n".join(parts) if parts else ""


def _to_anthropic_tools(tools: list[ToolSchema]) -> list[dict[str, object]]:
    return [{"name": t.name, "description": t.description, "input_schema": t.parameters} for t in tools]


def _parse_tool_use_blocks(blocks: list[object]) -> list[ToolCall]:
    result: list[ToolCall] = []
    for block in blocks:
        if getattr(block, "type", None) != "tool_use":
            continue
        result.append(ToolCall(id=getattr(block, "id", ""), name=getattr(block, "name", ""), arguments=getattr(block, "input", {})))
    return result


class AnthropicProvider:
    """Provider for Anthropic Claude API."""

    def __init__(self, api_key: str, model: str = "claude-sonnet-4-6", max_tokens: int = 4096, base_url: str | None = None) -> None:
        self._model = model
        self._max_tokens = max_tokens
        self._client = AsyncAnthropic(api_key=api_key, base_url=base_url)

    async def send(self, messages: list[Message], *, tools: list[ToolSchema] | None = None) -> ProviderResponse:
        system = _extract_system(messages)
        api_msgs = _to_anthropic_messages(messages)
        kwargs: dict[str, object] = {"model": self._model, "max_tokens": self._max_tokens, "messages": api_msgs}
        if system:
            kwargs["system"] = system
        if tools:
            kwargs["tools"] = _to_anthropic_tools(tools)
        resp = await self._client.messages.create(**kwargs)  # type: ignore
        text_parts: list[str] = []
        for block in resp.content:
            if getattr(block, "type", None) == "text":
                text_parts.append(getattr(block, "text", ""))
        tool_calls = _parse_tool_use_blocks(resp.content)
        usage = TokenUsage(prompt_tokens=resp.usage.input_tokens, completion_tokens=resp.usage.output_tokens, total_tokens=resp.usage.input_tokens + resp.usage.output_tokens)
        return ProviderResponse(content="".join(text_parts), tool_calls=tool_calls, usage=usage, model=resp.model, finish_reason=resp.stop_reason or "end_turn")

    async def stream(self, messages: list[Message], *, tools: list[ToolSchema] | None = None) -> AsyncIterator[ProviderResponse]:
        system = _extract_system(messages)
        api_msgs = _to_anthropic_messages(messages)
        kwargs: dict[str, object] = {"model": self._model, "max_tokens": self._max_tokens, "messages": api_msgs}
        if system:
            kwargs["system"] = system
        if tools:
            kwargs["tools"] = _to_anthropic_tools(tools)
        async with self._client.messages.stream(**kwargs) as stream:  # type: ignore
            async for text in stream.text_stream:
                yield ProviderResponse(content=text, tool_calls=[], usage=TokenUsage(prompt_tokens=0, completion_tokens=0, total_tokens=0), model=self._model, finish_reason="")

    def get_metadata(self) -> ProviderMetadata:
        return ProviderMetadata(name="anthropic", model=self._model, supports_streaming=True, supports_tools=True)
