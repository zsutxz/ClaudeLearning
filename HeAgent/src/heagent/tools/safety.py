"""Safety guardrails — inspect tool calls before execution."""

from __future__ import annotations

import re
from enum import Enum

from heagent.exceptions import SafetyViolation
from heagent.types import ToolCall


class SafetyMode(str, Enum):
    BLACKLIST = "blacklist"
    WHITELIST = "whitelist"


_DANGEROUS_PATTERNS: list[re.Pattern[str]] = [
    re.compile(p, re.IGNORECASE)
    for p in [
        r"\brm\s+(-[a-zA-Z]*f[a-zA-Z]*\s+|.*-rf\b)",
        r"\bformat\s+[a-zA-Z]:",
        r"\bdd\s+if=",
        r"\bmkfs\b",
        r"\bshutdown\b",
        r"\breboot\b",
        r"\bdel\s+/[sS]",
        r"\brmdir\s+/[sS]",
        r":\(\)\{.*;\}",  # fork bomb
        r">\s*/dev/sd",
        r"\bchmod\s+(-R\s+)?000\b",
        r"\bchown\s+(-R\s+)?root\b",
    ]
]


class SafetyGuard:
    """Pre-execution safety checker for tool calls."""

    def __init__(
        self,
        mode: SafetyMode = SafetyMode.BLACKLIST,
        blocked_commands: list[str] | None = None,
        allowed_commands: list[str] | None = None,
    ) -> None:
        self.mode = mode
        self._blocked: list[str] = blocked_commands or []
        self._allowed: list[str] = allowed_commands or []
        self._blocked_compiled = [re.compile(p, re.IGNORECASE) for p in self._blocked]
        self._allowed_compiled = [re.compile(p, re.IGNORECASE) for p in self._allowed]
        self._violation_log: list[str] = []

    def check(self, call: ToolCall) -> None:
        """Raise SafetyViolation if the tool call is unsafe."""
        if call.name != "shell":
            return
        command = call.arguments.get("command", "")
        if not isinstance(command, str):
            return

        for pat in _DANGEROUS_PATTERNS:
            if pat.search(command):
                msg = f"Blocked dangerous command: {command}"
                self._violation_log.append(msg)
                raise SafetyViolation(msg)

        if self.mode == SafetyMode.BLACKLIST:
            for pat in self._blocked_compiled:
                if pat.search(command):
                    msg = f"Blocked by blacklist: {command}"
                    self._violation_log.append(msg)
                    raise SafetyViolation(msg)
        elif self.mode == SafetyMode.WHITELIST:
            if self._allowed_compiled:
                allowed = any(pat.search(command) for pat in self._allowed_compiled)
                if not allowed:
                    msg = f"Blocked (not in whitelist): {command}"
                    self._violation_log.append(msg)
                    raise SafetyViolation(msg)

    @property
    def violations(self) -> list[str]:
        return list(self._violation_log)
