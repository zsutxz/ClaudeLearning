"""Provider chain with fallback."""

from __future__ import annotations

import logging
from collections.abc import AsyncIterator

from heagent.providers.base import BaseProvider, ProviderMetadata
from heagent.types import Message, ProviderResponse, ToolSchema

logger = logging.getLogger(__name__)


class ProviderChain:
    """Manages ordered providers with automatic fallback on failure."""

    def __init__(self, providers: list[BaseProvider]) -> None:
        if not providers:
            raise ValueError("ProviderChain requires at least one provider")
        self._providers = list(providers)
        self._current_index = 0

    @property
    def current(self) -> BaseProvider:
        return self._providers[self._current_index]

    @property
    def providers(self) -> list[BaseProvider]:
        return list(self._providers)

    def _advance(self) -> bool:
        if self._current_index < len(self._providers) - 1:
            old = self._current_index
            self._current_index += 1
            logger.info("Fallback: %s -> %s", self._providers[old].get_metadata().name, self.current.get_metadata().name)
            return True
        return False

    def reset(self) -> None:
        self._current_index = 0

    async def send(self, messages: list[Message], *, tools: list[ToolSchema] | None = None) -> ProviderResponse:
        last_error: Exception | None = None
        start = self._current_index
        for _ in range(len(self._providers)):
            try:
                return await self.current.send(messages, tools=tools)
            except Exception as e:
                last_error = e
                logger.warning("Provider %s failed: %s", self.current.get_metadata().name, e)
                if not self._advance():
                    break
        self._current_index = start
        raise last_error or RuntimeError("All providers failed")

    async def stream(self, messages: list[Message], *, tools: list[ToolSchema] | None = None) -> AsyncIterator[ProviderResponse]:
        start = self._current_index
        for _ in range(len(self._providers)):
            try:
                async for chunk in self.current.stream(messages, tools=tools):  # type: ignore[attr-defined]
                    yield chunk
                return
            except Exception as e:
                logger.warning("Provider %s stream failed: %s", self.current.get_metadata().name, e)
                if not self._advance():
                    break
        self._current_index = start
        raise RuntimeError("All providers failed for stream")

    def get_metadata(self) -> ProviderMetadata:
        return self.current.get_metadata()
