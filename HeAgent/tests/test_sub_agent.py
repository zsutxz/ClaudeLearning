"""Tests for sub-agent and parallel orchestration."""

from __future__ import annotations

import asyncio

import pytest

from heagent.agent.sub import SubAgent, SubAgentResult, run_parallel
from heagent.providers.base import ProviderMetadata
from heagent.tools.registry import ToolRegistry
from heagent.types import Message, ProviderResponse, Role, TokenUsage


class StubProvider:
    def __init__(self, answer: str = "done") -> None:
        self._answer = answer

    async def send(self, messages: list[Message], **kw: object) -> ProviderResponse:
        return ProviderResponse(
            content=self._answer,
            usage=TokenUsage(prompt_tokens=1, completion_tokens=1, total_tokens=2),
            model="stub",
            finish_reason="stop",
        )

    async def stream(self, messages: list[Message], **kw: object) -> object:
        yield await self.send(messages)

    def get_metadata(self) -> ProviderMetadata:
        return ProviderMetadata(name="stub", model="stub")


@pytest.mark.asyncio
class TestSubAgent:
    async def test_run_success(self) -> None:
        r = await SubAgent(StubProvider("result A"), max_iterations=5).run("task A")
        assert r.success
        assert r.output == "result A"

    async def test_run_failure(self) -> None:
        class Fail(StubProvider):
            async def send(self, messages: list[Message], **kw: object) -> ProviderResponse:
                raise RuntimeError("API down")
        r = await SubAgent(Fail(), max_iterations=2).run("fail")
        assert not r.success
        assert "API down" in r.output

    async def test_separate_context(self) -> None:
        r1, r2 = await asyncio.gather(
            SubAgent(StubProvider("a1"), max_iterations=3).run("t1"),
            SubAgent(StubProvider("a2"), max_iterations=3).run("t2"),
        )
        assert r1.output == "a1"
        assert r2.output == "a2"


@pytest.mark.asyncio
class TestParallel:
    async def test_parallel_all_succeed(self) -> None:
        agents = [SubAgent(StubProvider(f"r{i}"), max_iterations=5) for i in range(3)]
        results = await run_parallel(agents, [f"t{i}" for i in range(3)])
        assert len(results) == 3
        assert all(r.success for r in results)

    async def test_parallel_one_fails(self) -> None:
        class Fail(StubProvider):
            async def send(self, messages: list[Message], **kw: object) -> ProviderResponse:
                raise ValueError("boom")
        results = await run_parallel(
            [SubAgent(StubProvider("ok"), max_iterations=5),
             SubAgent(Fail(), max_iterations=5)],
            ["good", "bad"],
        )
        assert results[0].success
        assert not results[1].success

    async def test_empty(self) -> None:
        assert await run_parallel([], []) == []
