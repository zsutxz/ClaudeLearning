"""BaseProvider protocol and ProviderMetadata for LLM provider abstraction."""

from __future__ import annotations

from collections.abc import AsyncIterator
from typing import Protocol, runtime_checkable

from pydantic import BaseModel

from heagent.types import Message, ProviderResponse, ToolSchema


class ProviderMetadata(BaseModel):
    """Metadata describing a provider's capabilities."""

    name: str
    model: str
    supports_streaming: bool = False
    supports_tools: bool = False


@runtime_checkable
class BaseProvider(Protocol):
    """Unified protocol for LLM providers.

    Any provider (OpenAI, Anthropic, etc.) must implement these methods
    to integrate with the HeAgent framework. No inheritance needed —
    structural subtyping via Protocol.
    """

    async def send(
        self,
        messages: list[Message],
        *,
        tools: list[ToolSchema] | None = None,
    ) -> ProviderResponse: ...

    async def stream(
        self,
        messages: list[Message],
        *,
        tools: list[ToolSchema] | None = None,
    ) -> AsyncIterator[ProviderResponse]: ...

    def get_metadata(self) -> ProviderMetadata: ...
