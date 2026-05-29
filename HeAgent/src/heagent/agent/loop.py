"""Agent core loop — LLM ↔ tool cycle with iteration budget."""

from __future__ import annotations

import asyncio
import logging
from dataclasses import dataclass, field

from heagent.agent.middleware import MiddlewareFn, Request, compose
from heagent.config import get_settings
from heagent.exceptions import BudgetExceeded, SafetyViolation, ToolError
from heagent.providers.base import BaseProvider
from heagent.tools.registry import ToolRegistry
from heagent.tools.safety import SafetyGuard
from heagent.types import Message, ProviderResponse, Role, ToolCall, ToolResult

logger = logging.getLogger(__name__)


@dataclass
class AgentState:
    """Mutable state for a single agent run."""

    messages: list[Message] = field(default_factory=list)
    iteration: int = 0
    max_iterations: int = 50
    results: list[ToolResult] = field(default_factory=list)


class AgentLoop:
    """Orchestrates the LLM ↔ tool execution cycle."""

    def __init__(
        self,
        provider: BaseProvider,
        *,
        registry: ToolRegistry | None = None,
        guard: SafetyGuard | None = None,
        middlewares: list[MiddlewareFn] | None = None,
        max_iterations: int | None = None,
    ) -> None:
        self.provider = provider
        self.registry = registry or ToolRegistry.get()
        self.guard = guard or SafetyGuard()
        self.middlewares = middlewares or []
        settings = get_settings()
        self.max_iterations = max_iterations or settings.max_iterations

    async def run(self, prompt: str, *, system: str | None = None) -> str:
        """Run the agent loop until the LLM produces a final answer."""
        state = AgentState(max_iterations=self.max_iterations)
        if system:
            state.messages.append(Message(role=Role.SYSTEM, content=system))
        state.messages.append(Message(role=Role.USER, content=prompt))

        while True:
            state.iteration += 1
            if state.iteration > state.max_iterations:
                raise BudgetExceeded(
                    f"Exceeded {state.max_iterations} iterations without final answer"
                )

            response = await self._call_provider(state)
            state.messages.append(
                Message(role=Role.ASSISTANT, content=response.content, tool_calls=response.tool_calls or None)
            )

            if not response.tool_calls:
                return response.content

            tool_results = await self._execute_tools(response.tool_calls, state)
            for tr in tool_results:
                state.messages.append(
                    Message(role=Role.TOOL, content=tr.content, tool_call_id=tr.tool_call_id)
                )

    async def _call_provider(self, state: AgentState) -> ProviderResponse:
        tools = self.registry.enabled_schemas()

        async def handler(req: Request) -> ProviderResponse:
            return await self.provider.send(
                req.messages,
                tools=req.tools or None,
            )

        if self.middlewares:
            chain = compose(self.middlewares, handler)
            return await chain(Request(messages=state.messages, tools=tools))

        return await handler(Request(messages=state.messages, tools=tools))

    async def _execute_tools(
        self, calls: list[ToolCall], state: AgentState
    ) -> list[ToolResult]:
        tasks = [self._execute_one(call) for call in calls]
        results = await asyncio.gather(*tasks)
        state.results.extend(results)
        return list(results)

    async def _execute_one(self, call: ToolCall) -> ToolResult:
        try:
            self.guard.check(call)
        except SafetyViolation as e:
            return ToolResult(tool_call_id=call.id, content=str(e), is_error=True)

        handler = self.registry.get_handler(call.name)
        if handler is None:
            return ToolResult(tool_call_id=call.id, content=f"Unknown tool: {call.name}", is_error=True)

        try:
            result = await self._invoke(handler, call)
            content = str(result) if result is not None else ""
            return ToolResult(tool_call_id=call.id, content=content)
        except Exception as e:
            logger.exception("Tool %s failed", call.name)
            return ToolResult(tool_call_id=call.id, content=f"Tool error: {e}", is_error=True)

    async def _invoke(self, handler: object, call: ToolCall) -> object:
        """Invoke a tool handler with call arguments."""
        if asyncio.iscoroutinefunction(handler):
            return await handler(**call.arguments)
        return handler(**call.arguments)
