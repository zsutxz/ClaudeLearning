"""Tests for session persistence."""

from __future__ import annotations

import json

import pytest

from heagent.context.session import SessionStore
from heagent.types import Message, Role, ToolCall


def _msgs(*contents: str) -> list[Message]:
    roles = [Role.USER, Role.ASSISTANT, Role.SYSTEM, Role.TOOL]
    return [Message(role=roles[i % len(roles)], content=c) for i, c in enumerate(contents)]


class TestSessionStore:
    def test_save_and_load(self, tmp_path: object) -> None:
        store = SessionStore(base_dir=str(tmp_path / "sessions"))  # type: ignore[operator]
        msgs = _msgs("hello", "world")
        store.save("s1", msgs)

        loaded = store.load("s1")
        assert len(loaded) == 2
        assert loaded[0].content == "hello"
        assert loaded[1].content == "world"

    def test_load_nonexistent(self, tmp_path: object) -> None:
        store = SessionStore(base_dir=str(tmp_path / "sessions"))  # type: ignore[operator]
        assert store.load("ghost") == []

    def test_json_format(self, tmp_path: object) -> None:
        store = SessionStore(base_dir=str(tmp_path / "sessions"))  # type: ignore[operator]
        msgs = [Message(role=Role.USER, content="test")]
        path = store.save("fmt", msgs)

        with open(path, encoding="utf-8") as f:
            data = json.load(f)
        assert "session_id" in data
        assert "timestamp" in data
        assert data["session_id"] == "fmt"
        assert len(data["messages"]) == 1

    def test_roundtrip_with_tool_calls(self, tmp_path: object) -> None:
        store = SessionStore(base_dir=str(tmp_path / "sessions"))  # type: ignore[operator]
        msgs = [
            Message(
                role=Role.ASSISTANT,
                content="",
                tool_calls=[ToolCall(id="c1", name="shell", arguments={"command": "ls"})],
            ),
        ]
        store.save("tools", msgs)
        loaded = store.load("tools")
        assert len(loaded) == 1
        assert loaded[0].tool_calls is not None
        assert loaded[0].tool_calls[0].name == "shell"

    def test_list_sessions(self, tmp_path: object) -> None:
        store = SessionStore(base_dir=str(tmp_path / "sessions"))  # type: ignore[operator]
        store.save("b", _msgs("b"))
        store.save("a", _msgs("a"))
        assert store.list_sessions() == ["a", "b"]

    def test_delete(self, tmp_path: object) -> None:
        store = SessionStore(base_dir=str(tmp_path / "sessions"))  # type: ignore[operator]
        store.save("del", _msgs("x"))
        assert store.delete("del") is True
        assert store.load("del") == []
        assert store.delete("del") is False

    def test_overwrite(self, tmp_path: object) -> None:
        store = SessionStore(base_dir=str(tmp_path / "sessions"))  # type: ignore[operator]
        store.save("ow", _msgs("v1"))
        store.save("ow", _msgs("v2"))
        loaded = store.load("ow")
        assert len(loaded) == 1
        assert loaded[0].content == "v2"
