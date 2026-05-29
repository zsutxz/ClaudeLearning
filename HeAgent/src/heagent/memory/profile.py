"""User profile — USER.md for personalized interaction."""

from __future__ import annotations

from pathlib import Path


class ProfileStore:
    """Manage user profile as USER.md."""

    def __init__(self, path: str = ".heagent/user/USER.md") -> None:
        self._path = Path(path)

    def load(self) -> str:
        if not self._path.exists():
            return ""
        return self._path.read_text(encoding="utf-8").strip()

    def save(self, content: str) -> None:
        self._path.parent.mkdir(parents=True, exist_ok=True)
        self._path.write_text(content, encoding="utf-8")

    def update_section(self, section: str, value: str) -> None:
        current = self.load()
        header = f"## {section}"
        lines = current.splitlines() if current else []
        new_lines: list[str] = []
        replaced = False
        i = 0
        while i < len(lines):
            if lines[i].strip() == header:
                new_lines.append(header)
                new_lines.append(value)
                replaced = True
                i += 1
                while i < len(lines) and not lines[i].startswith("## "):
                    i += 1
            else:
                new_lines.append(lines[i])
                i += 1
        if not replaced:
            if new_lines:
                new_lines.append("")
            new_lines.extend([header, value])
        self.save("\n".join(new_lines) + "\n")

    def clear(self) -> None:
        if self._path.exists():
            self._path.unlink()
