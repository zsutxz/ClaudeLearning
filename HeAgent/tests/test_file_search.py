"""Tests for file and search tools."""

from __future__ import annotations

import pytest

from heagent.tools.builtins.file import file_read, file_write
from heagent.tools.builtins.search import content_search, file_search
from heagent.tools.registry import ToolRegistry


class TestFileToolsRegistration:
    def test_file_read_registered(self) -> None:
        assert "file_read" in ToolRegistry.get().list_names()

    def test_file_write_registered(self) -> None:
        assert "file_write" in ToolRegistry.get().list_names()

    def test_file_search_registered(self) -> None:
        assert "file_search" in ToolRegistry.get().list_names()

    def test_content_search_registered(self) -> None:
        assert "content_search" in ToolRegistry.get().list_names()


@pytest.mark.asyncio
class TestFileRead:
    async def test_read_existing_file(self, tmp_path: object) -> None:
        p = tmp_path / "hello.txt"  # type: ignore[operator]
        p.write_text("hello world", encoding="utf-8")
        result = await file_read(str(p))
        assert result == "hello world"

    async def test_read_missing_file(self, tmp_path: object) -> None:
        result = await file_read(str(tmp_path / "missing.txt"))  # type: ignore[operator]
        assert "Error: file not found" in result

    async def test_read_directory(self, tmp_path: object) -> None:
        result = await file_read(str(tmp_path))  # type: ignore[operator]
        assert "Error: path is a directory" in result


@pytest.mark.asyncio
class TestFileWrite:
    async def test_write_new_file(self, tmp_path: object) -> None:
        p = tmp_path / "output.txt"  # type: ignore[operator]
        result = await file_write(str(p), "test content")
        assert "OK" in result
        assert p.read_text(encoding="utf-8") == "test content"

    async def test_write_creates_parent_dirs(self, tmp_path: object) -> None:
        p = tmp_path / "sub" / "dir" / "file.txt"  # type: ignore[operator]
        result = await file_write(str(p), "nested")
        assert "OK" in result
        assert p.read_text(encoding="utf-8") == "nested"

    async def test_write_overwrites(self, tmp_path: object) -> None:
        p = tmp_path / "overwrite.txt"  # type: ignore[operator]
        p.write_text("old", encoding="utf-8")
        await file_write(str(p), "new")
        assert p.read_text(encoding="utf-8") == "new"


@pytest.mark.asyncio
class TestFileSearch:
    async def test_find_by_pattern(self, tmp_path: object) -> None:
        d = tmp_path  # type: ignore[operator]
        (d / "a.txt").write_text("", encoding="utf-8")
        (d / "b.py").write_text("", encoding="utf-8")
        (d / "c.txt").write_text("", encoding="utf-8")
        result = await file_search("*.txt", str(d))
        assert "a.txt" in result
        assert "c.txt" in result
        assert "b.py" not in result

    async def test_no_matches(self, tmp_path: object) -> None:
        result = await file_search("*.xyz", str(tmp_path))  # type: ignore[operator]
        assert "No files matching" in result

    async def test_max_results(self, tmp_path: object) -> None:
        d = tmp_path  # type: ignore[operator]
        for i in range(30):
            (d / f"f{i}.txt").write_text("", encoding="utf-8")
        result = await file_search("*.txt", str(d), max_results=5)
        assert len(result.splitlines()) == 5


@pytest.mark.asyncio
class TestContentSearch:
    async def test_find_content(self, tmp_path: object) -> None:
        d = tmp_path  # type: ignore[operator]
        (d / "a.txt").write_text("hello world\nfoo bar", encoding="utf-8")
        (d / "b.txt").write_text("no match here", encoding="utf-8")
        result = await content_search("hello", str(d), "*.txt")
        assert "a.txt" in result
        assert "hello" in result

    async def test_no_content_matches(self, tmp_path: object) -> None:
        d = tmp_path  # type: ignore[operator]
        (d / "a.txt").write_text("nothing relevant", encoding="utf-8")
        result = await content_search("missing", str(d), "*.txt")
        assert "No matches" in result

    async def test_invalid_regex(self, tmp_path: object) -> None:
        result = await content_search("[invalid", str(tmp_path), "*.txt")  # type: ignore[operator]
        assert "Error: invalid regex" in result
