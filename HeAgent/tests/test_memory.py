"""Tests for memory: skills, facts, profile."""

from __future__ import annotations

import pytest

from heagent.memory.facts import FactStore
from heagent.memory.profile import ProfileStore
from heagent.memory.skills import SkillStore


class TestSkillStore:
    def test_save_and_load(self, tmp_path: object) -> None:
        s = SkillStore(base_dir=str(tmp_path / "sk"))  # type: ignore[operator]
        s.save("deploy", "Deploy app", "git push && deploy", ["push", "deploy"])
        content = s.load("deploy")
        assert content is not None
        assert "1. push" in content

    def test_list(self, tmp_path: object) -> None:
        s = SkillStore(base_dir=str(tmp_path / "sk"))  # type: ignore[operator]
        s.save("a", "a", "p", ["x"])
        s.save("b", "b", "q", ["y"])
        assert s.list_skills() == ["a", "b"]

    def test_delete(self, tmp_path: object) -> None:
        s = SkillStore(base_dir=str(tmp_path / "sk"))  # type: ignore[operator]
        s.save("rm_me", "x", "p", [])
        assert s.delete("rm_me") is True
        assert s.load("rm_me") is None

    def test_all_skills_content(self, tmp_path: object) -> None:
        s = SkillStore(base_dir=str(tmp_path / "sk"))  # type: ignore[operator]
        s.save("a", "desc a", "p", [])
        assert len(s.all_skills_content()) == 1

    def test_name_sanitization(self, tmp_path: object) -> None:
        s = SkillStore(base_dir=str(tmp_path / "sk"))  # type: ignore[operator]
        s.save("my skill/here", "d", "p", [])
        assert s.load("my skill/here") is not None


class TestFactStore:
    def test_add_and_load(self, tmp_path: object) -> None:
        f = FactStore(path=str(tmp_path / "mem.md"))  # type: ignore[operator]
        assert f.add("Python is great") is True
        assert f.add("Rust is fast") is True
        assert f.load() == ["Python is great", "Rust is fast"]

    def test_dedup(self, tmp_path: object) -> None:
        f = FactStore(path=str(tmp_path / "mem.md"))  # type: ignore[operator]
        f.add("the user prefers dark mode")
        assert f.add("user prefers dark mode always") is False
        assert len(f.load()) == 1

    def test_clear(self, tmp_path: object) -> None:
        f = FactStore(path=str(tmp_path / "mem.md"))  # type: ignore[operator]
        f.add("x")
        f.clear()
        assert f.load() == []

    def test_load_empty(self, tmp_path: object) -> None:
        f = FactStore(path=str(tmp_path / "mem.md"))  # type: ignore[operator]
        assert f.load() == []


class TestProfileStore:
    def test_save_and_load(self, tmp_path: object) -> None:
        p = ProfileStore(path=str(tmp_path / "u.md"))  # type: ignore[operator]
        p.save("## Background\nSenior dev")
        assert "Senior dev" in p.load()

    def test_update_section_new(self, tmp_path: object) -> None:
        p = ProfileStore(path=str(tmp_path / "u.md"))  # type: ignore[operator]
        p.update_section("Style", "concise")
        p.update_section("Level", "advanced")
        assert "concise" in p.load()
        assert "advanced" in p.load()

    def test_update_section_replace(self, tmp_path: object) -> None:
        p = ProfileStore(path=str(tmp_path / "u.md"))  # type: ignore[operator]
        p.update_section("Style", "verbose")
        p.update_section("Style", "concise")
        assert "concise" in p.load()
        assert "verbose" not in p.load()

    def test_load_empty(self, tmp_path: object) -> None:
        assert ProfileStore(path=str(tmp_path / "u.md")).load() == ""  # type: ignore[operator]

    def test_clear(self, tmp_path: object) -> None:
        p = ProfileStore(path=str(tmp_path / "u.md"))  # type: ignore[operator]
        p.save("data")
        p.clear()
        assert p.load() == ""
