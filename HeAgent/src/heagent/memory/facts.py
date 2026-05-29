"""Fact memory — persistent MEMORY.md for cross-session recall."""

from __future__ import annotations

from pathlib import Path


class FactStore:
    """Append-only fact memory with keyword deduplication."""

    def __init__(self, path: str = ".heagent/memory/MEMORY.md") -> None:
        self._path = Path(path)

    def add(self, fact: str) -> bool:
        """Add a fact. Returns True if added, False if duplicate."""
        existing = self._load_facts()
        fact_words = set(fact.lower().split())
        for ef in existing:
            overlap = fact_words & set(ef.lower().split())
            if len(overlap) / max(len(fact_words), 1) > 0.7:
                return False
        self._path.parent.mkdir(parents=True, exist_ok=True)
        with open(self._path, "a", encoding="utf-8") as f:
            f.write(f"- {fact}\n")
        return True

    def load(self) -> list[str]:
        return self._load_facts()

    def clear(self) -> None:
        if self._path.exists():
            self._path.unlink()

    def _load_facts(self) -> list[str]:
        if not self._path.exists():
            return []
        lines = self._path.read_text(encoding="utf-8").strip().splitlines()
        return [l[2:] for l in lines if l.startswith("- ")]
