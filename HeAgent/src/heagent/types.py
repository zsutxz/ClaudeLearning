"""HeAgent shared type definitions."""

from __future__ import annotations

from enum import Enum

from pydantic import BaseModel


class Role(str, Enum):
    """Message role in conversation."""

    USER = "user"
    ASSISTANT = "assistant"
    SYSTEM = "system"
    TOOL = "tool"


class TokenUsage(BaseModel):
    """Token usage statistics from a provider response."""

    prompt_tokens: int
    completion_tokens: int
    total_tokens: int


class ToolCall(BaseModel):
    """A tool call requested by the LLM."""

    id: str
    name: str
    arguments: dict[str, object]


class ToolResult(BaseModel):
    """Result returned from executing a tool."""

    tool_call_id: str
    content: str
    is_error: bool = False


class Message(BaseModel):
    """A message in the conversation history."""

    role: Role
    content: str
    name: str | None = None
    tool_call_id: str | None = None
    tool_calls: list[ToolCall] | None = None


class ProviderResponse(BaseModel):
    """Unified response from any provider."""

    content: str
    tool_calls: list[ToolCall] = []
    usage: TokenUsage
    model: str
    finish_reason: str
