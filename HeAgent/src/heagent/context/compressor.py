"""Context compressor — summarize old messages when token usage is high."""

from __future__ import annotations

import logging

from heagent.providers.base import BaseProvider
from heagent.types import Message, ProviderResponse, Role, TokenUsage

logger = logging.getLogger(__name__)

_DEFAULT_PROMPT = (
    "Summarize the following conversation so far in a concise paragraph. "
    "Preserve key facts, decisions, and any important context."
)


class ContextCompressor:
    """Compresses conversation history using the LLM when token usage exceeds a threshold."""

    def __init__(
        self,
        provider: BaseProvider,
        *,
        threshold: float = 0.8,
        keep_recent: int = 4,
    ) -> None:
        self.provider = provider
        self.threshold = threshold
        self.keep_recent = keep_recent

    async def compress(
        self,
        messages: list[Message],
        token_count: int,
        max_tokens: int,
    ) -> list[Message]:
        """Compress messages if token_count/max_tokens exceeds threshold."""
        if max_tokens == 0:
            return messages
        if token_count / max_tokens < self.threshold:
            return messages

        system_msgs: list[Message] = []
        conversation: list[Message] = []

        for m in messages:
            if m.role == Role.SYSTEM:
                system_msgs.append(m)
            else:
                conversation.append(m)

        if len(conversation) <= self.keep_recent:
            return messages

        old = conversation[: -self.keep_recent]
        recent = conversation[-self.keep_recent :]

        summary = await self._summarize(old)

        compressed = [
            *system_msgs,
            Message(role=Role.SYSTEM, content=f"[Conversation summary]\n{summary}"),
            *recent,
        ]
        logger.info(
            "Compressed %d messages into summary (threshold=%.0f%%)",
            len(old),
            self.threshold * 100,
        )
        return compressed

    async def _summarize(self, messages: list[Message]) -> str:
        prompt = "\n".join(f"{m.role.value}: {m.content}" for m in messages if m.content)
        resp: ProviderResponse = await self.provider.send(
            [Message(role=Role.USER, content=f"{_DEFAULT_PROMPT}\n\n{prompt}")]
        )
        return resp.content
