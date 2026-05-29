"""Sub-agent — isolated agent instance for delegated tasks."""

from __future__ import annotations

import asyncio
from dataclasses import dataclass, field

from heagent.agent.loop import AgentLoop
from heagent.providers.base import BaseProvider
from heagent.tools.registry import ToolRegistry
from heagent.tools.safety import SafetyGuard
from heagent.types import Message, Role


@dataclass
class SubAgentResult:
    task: str
    output: str
    success: bool
    iterations: int = 0


class SubAgent:
    """An isolated agent with its own context and budget."""

    def __init__(
        self,
        provider: BaseProvider,
        *,
        registry: ToolRegistry | None = None,
        guard: SafetyGuard | None = None,
        max_iterations: int = 20,
    ) -> None:
        self._provider = provider
        self._registry = registry or ToolRegistry.get()
        self._guard = guard or SafetyGuard()
        self._max_iterations = max_iterations

    async def run(self, task: str) -> SubAgentResult:
        loop = AgentLoop(
            self._provider,
            registry=self._registry,
            guard=self._guard,
            max_iterations=self._max_iterations,
        )
        try:
            output = await loop.run(task)
            return SubAgentResult(task=task, output=output, success=True)
        except Exception as e:
            return SubAgentResult(task=task, output=str(e), success=False)


async def run_parallel(agents: list[SubAgent], tasks: list[str]) -> list[SubAgentResult]:
    """Run multiple sub-agents in parallel via asyncio.gather."""
    coros = [a.run(t) for a, t in zip(agents, tasks)]
    return list(await asyncio.gather(*coros, return_exceptions=False))
