"""File read/write tools."""

from __future__ import annotations

from pathlib import Path

from heagent.tools.decorator import tool


@tool
async def file_read(path: str) -> str:
    """Read the contents of a file."""
    p = Path(path)
    if not p.exists():
        return f"Error: file not found: {path}"
    if p.is_dir():
        return f"Error: path is a directory: {path}"
    try:
        return p.read_text(encoding="utf-8")
    except Exception as e:
        return f"Error reading file: {e}"


@tool
async def file_write(path: str, content: str) -> str:
    """Write content to a file, creating parent directories as needed."""
    p = Path(path)
    try:
        p.parent.mkdir(parents=True, exist_ok=True)
        p.write_text(content, encoding="utf-8")
        return f"OK: wrote {len(content)} chars to {path}"
    except Exception as e:
        return f"Error writing file: {e}"
