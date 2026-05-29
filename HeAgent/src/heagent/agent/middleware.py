"""Middleware pipeline for Agent request/response processing."""

from __future__ import annotations

from typing import Any, Callable, Protocol

from heagent.types import Message, ProviderResponse

MiddlewareFn = Callable[["Request", "NextFn"], Any]


class Request:
    """Wraps the data passed through the middleware pipeline."""

    __slots__ = ("messages", "tools", "metadata")

    def __init__(
        self,
        messages: list[Message],
        tools: list[object] | None = None,
        metadata: dict[str, object] | None = None,
    ) -> None:
        self.messages = messages
        self.tools = tools or []
        self.metadata = metadata or {}


class NextFn(Protocol):
    def __call__(self, request: Request) -> Any: ...


def compose(middlewares: list[MiddlewareFn], handler: Callable[[Request], Any]) -> NextFn:
    """Compose middlewares into a chain, with *handler* as the innermost function."""

    def build(idx: int) -> NextFn:
        if idx >= len(middlewares):
            return lambda req: handler(req)

        mw = middlewares[idx]

        def next_fn(req: Request) -> Any:
            return mw(req, build(idx + 1))

        return next_fn

    return build(0)
