"""OpenAI-compatible provider implementation."""

from __future__ import annotations

import json
from collections.abc import AsyncIterator

from openai import AsyncOpenAI

from heagent.providers.base import ProviderMetadata
from heagent.types import Message, ProviderResponse, Role, TokenUsage, ToolCall, ToolSchema


def _to_openai_messages(messages: list[Message]) -> list[dict[str, object]]:
    """Convert HeAgent Messages to OpenAI API message dicts."""
    result: list[dict[str, object]] = []
    for msg in messages:
        d: dict[str, object] = {"role": msg.role.value, "content": msg.content}
        if msg.tool_calls:
            d["tool_calls"] = [
                {
                    "id": tc.id,
                    "type": "function",
                    "function": {"name": tc.name, "arguments": json.dumps(tc.arguments)},
                }
                for tc in msg.tool_calls
            ]
        if msg.tool_call_id:
            d["tool_call_id"] = msg.tool_call_id
        if msg.name:
            d["name"] = msg.name
        result.append(d)
    return result


def _to_openai_tools(tools: list[ToolSchema]) -> list[dict[str, object]]:
    """Convert HeAgent ToolSchema to OpenAI tools format."""
    return [
        {
            "type": "function",
            "function": {"name": t.name, "description": t.description, "parameters": t.parameters},
        }
        for t in tools
    ]


def _parse_tool_calls(raw: list[object]) -> list[ToolCall]:
    """Parse OpenAI tool_calls to HeAgent ToolCall list."""
    result: list[ToolCall] = []
    for tc in raw:
        tc_id = getattr(tc, "id", "")
        fn = getattr(tc, "function", None)
        if fn is None:
            continue
        name = getattr(fn, "name", "")
        args_str = getattr(fn, "arguments", "{}")
        try:
            args = json.loads(args_str) if isinstance(args_str, str) else {}
        except json.JSONDecodeError:
            args = {}
        result.append(ToolCall(id=tc_id, name=name, arguments=args))
    return result


class OpenAIProvider:
    """Provider for OpenAI and compatible APIs."""

    def __init__(
        self,
        api_key: str,
        model: str = "gpt-4o",
        base_url: str | None = None,
    ) -> None:
        self._model = model
        self._client = AsyncOpenAI(api_key=api_key, base_url=base_url)

    async def send(
        self,
        messages: list[Message],
        *,
        tools: list[ToolSchema] | None = None,
    ) -> ProviderResponse:
        kwargs: dict[str, object] = {
            "model": self._model,
            "messages": _to_openai_messages(messages),
        }
        if tools:
            kwargs["tools"] = _to_openai_tools(tools)

        resp = await self._client.chat.completions.create(**kwargs)  # type: ignore[call-overload]

        choice = resp.choices[0]
        message = choice.message
        tool_calls = _parse_tool_calls(message.tool_calls) if message.tool_calls else []

        usage = TokenUsage(
            prompt_tokens=resp.usage.prompt_tokens if resp.usage else 0,
            completion_tokens=resp.usage.completion_tokens if resp.usage else 0,
            total_tokens=resp.usage.total_tokens if resp.usage else 0,
        )

        return ProviderResponse(
            content=message.content or "",
            tool_calls=tool_calls,
            usage=usage,
            model=resp.model,
            finish_reason=choice.finish_reason or "stop",
        )

    async def stream(
        self,
        messages: list[Message],
        *,
        tools: list[ToolSchema] | None = None,
    ) -> AsyncIterator[ProviderResponse]:
        kwargs: dict[str, object] = {
            "model": self._model,
            "messages": _to_openai_messages(messages),
            "stream": True,
        }
        if tools:
            kwargs["tools"] = _to_openai_tools(tools)

        stream = await self._client.chat.completions.create(**kwargs)  # type: ignore[call-overload]
        async for chunk in stream:
            if not chunk.choices:
                continue
            delta = chunk.choices[0].delta
            usage = TokenUsage(
                prompt_tokens=chunk.usage.prompt_tokens if chunk.usage else 0,
                completion_tokens=chunk.usage.completion_tokens if chunk.usage else 0,
                total_tokens=chunk.usage.total_tokens if chunk.usage else 0,
            )
            yield ProviderResponse(
                content=delta.content or "",
                tool_calls=[],
                usage=usage,
                model=chunk.model,
                finish_reason=chunk.choices[0].finish_reason or "",
            )

    def get_metadata(self) -> ProviderMetadata:
        return ProviderMetadata(
            name="openai",
            model=self._model,
            supports_streaming=True,
            supports_tools=True,
        )
