"""Skill extraction — distill reusable operation patterns into SKILL.md files."""

from __future__ import annotations

from datetime import datetime
from pathlib import Path


class SkillStore:
    """Manage extracted skills as SKILL.md files."""

    def __init__(self, base_dir: str = ".heagent/skills") -> None:
        self._base = Path(base_dir)

    def save(self, name: str, description: str, pattern: str, steps: list[str]) -> str:
        self._base.mkdir(parents=True, exist_ok=True)
        safe = name.replace(" ", "_").replace("/", "-")
        path = self._base / f"{safe}.md"
        lines = [
            f"# {name}",
            "",
            f"- **Description:** {description}",
            f"- **Created:** {datetime.now().isoformat()}",
            "",
            "## Pattern",
            pattern,
            "",
            "## Steps",
        ]
        for i, step in enumerate(steps, 1):
            lines.append(f"{i}. {step}")
        path.write_text("\n".join(lines) + "\n", encoding="utf-8")
        return str(path)

    def load(self, name: str) -> str | None:
        safe = name.replace(" ", "_").replace("/", "-")
        path = self._base / f"{safe}.md"
        if path.exists():
            return path.read_text(encoding="utf-8")
        return None

    def list_skills(self) -> list[str]:
        if not self._base.exists():
            return []
        return sorted(p.stem for p in self._base.glob("*.md"))

    def delete(self, name: str) -> bool:
        safe = name.replace(" ", "_").replace("/", "-")
        path = self._base / f"{safe}.md"
        if path.exists():
            path.unlink()
            return True
        return False

    def all_skills_content(self) -> list[str]:
        if not self._base.exists():
            return []
        return [p.read_text(encoding="utf-8") for p in sorted(self._base.glob("*.md"))]
