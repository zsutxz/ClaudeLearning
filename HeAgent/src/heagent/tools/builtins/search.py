"""File search tools — find files by name or content."""

from __future__ import annotations

import re
from pathlib import Path

from heagent.tools.decorator import tool


@tool
async def file_search(
    pattern: str,
    directory: str = ".",
    max_results: int = 20,
) -> str:
    """Search for files by name pattern (glob) under a directory."""
    root = Path(directory)
    if not root.exists():
        return f"Error: directory not found: {directory}"
    matches: list[str] = []
    for p in root.rglob(pattern):
        matches.append(str(p))
        if len(matches) >= max_results:
            break
    if not matches:
        return f"No files matching '{pattern}' found in {directory}"
    return "\n".join(matches)


@tool
async def content_search(
    query: str,
    directory: str = ".",
    file_pattern: str = "*.txt",
    max_results: int = 20,
) -> str:
    """Search file contents for a regex pattern."""
    root = Path(directory)
    if not root.exists():
        return f"Error: directory not found: {directory}"
    try:
        regex = re.compile(query, re.IGNORECASE)
    except re.error as e:
        return f"Error: invalid regex: {e}"
    results: list[str] = []
    for p in root.rglob(file_pattern):
        if not p.is_file():
            continue
        try:
            text = p.read_text(encoding="utf-8", errors="replace")
        except Exception:
            continue
        for i, line in enumerate(text.splitlines(), 1):
            if regex.search(line):
                results.append(f"{p}:{i}: {line.strip()}")
                if len(results) >= max_results:
                    break
        if len(results) >= max_results:
            break
    if not results:
        return f"No matches for '{query}' in {file_pattern} files under {directory}"
    return "\n".join(results)
