"""Tests for HeAgent shared types."""

from heagent.types import (
    Message,
    ProviderResponse,
    Role,
    TokenUsage,
    ToolCall,
    ToolResult,
)


def test_role_enum() -> None:
    assert Role.USER == "user"
    assert Role.ASSISTANT == "assistant"
    assert Role.SYSTEM == "system"
    assert Role.TOOL == "tool"


def test_message_user() -> None:
    msg = Message(role=Role.USER, content="Hello")
    assert msg.role is Role.USER
    assert msg.content == "Hello"
    assert msg.tool_calls is None


def test_message_assistant_with_tool_calls() -> None:
    tc = ToolCall(id="call_1", name="shell", arguments={"command": "ls"})
    msg = Message(role=Role.ASSISTANT, content="", tool_calls=[tc])
    assert msg.tool_calls is not None
    assert len(msg.tool_calls) == 1
    assert msg.tool_calls[0].name == "shell"


def test_message_tool_result() -> None:
    msg = Message(role=Role.TOOL, content="output", tool_call_id="call_1")
    assert msg.tool_call_id == "call_1"


def test_token_usage() -> None:
    usage = TokenUsage(prompt_tokens=10, completion_tokens=20, total_tokens=30)
    assert usage.total_tokens == 30


def test_tool_result() -> None:
    result = ToolResult(tool_call_id="call_1", content="ok")
    assert not result.is_error

    err = ToolResult(tool_call_id="call_2", content="failed", is_error=True)
    assert err.is_error


def test_provider_response() -> None:
    resp = ProviderResponse(
        content="Hi",
        usage=TokenUsage(prompt_tokens=5, completion_tokens=1, total_tokens=6),
        model="gpt-4o",
        finish_reason="stop",
    )
    assert resp.model == "gpt-4o"
    assert resp.tool_calls == []
    assert resp.finish_reason == "stop"
