"""Session persistence — save/restore conversation history as JSON."""

from __future__ import annotations

import json
import time
from pathlib import Path

from heagent.types import Message, Role


class SessionStore:
    """Manages saving and loading conversation sessions."""

    def __init__(self, base_dir: str = ".heagent/sessions") -> None:
        self._base = Path(base_dir)

    def save(self, session_id: str, messages: list[Message]) -> str:
        self._base.mkdir(parents=True, exist_ok=True)
        path = self._base / f"{session_id}.json"
        data = {
            "session_id": session_id,
            "timestamp": time.time(),
            "messages": [m.model_dump() for m in messages],
        }
        path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
        return str(path)

    def load(self, session_id: str) -> list[Message]:
        path = self._base / f"{session_id}.json"
        if not path.exists():
            return []
        data = json.loads(path.read_text(encoding="utf-8"))
        return [Message(**m) for m in data.get("messages", [])]

    def list_sessions(self) -> list[str]:
        if not self._base.exists():
            return []
        return sorted(
            p.stem for p in self._base.glob("*.json")
        )

    def delete(self, session_id: str) -> bool:
        path = self._base / f"{session_id}.json"
        if path.exists():
            path.unlink()
            return True
        return False
