"""Error classification and differentiated retry logic."""

from __future__ import annotations

import asyncio
import random
from collections.abc import Awaitable, Callable
from enum import Enum

from heagent.exceptions import ProviderError


class ErrorCategory(str, Enum):
    RATE_LIMITED = "rate_limited"
    AUTH_FAILED = "auth_failed"
    TRANSIENT = "transient"
    NON_TRANSIENT = "non_transient"


def classify_error(error: ProviderError) -> ErrorCategory:
    msg = error.message.lower()
    status = getattr(error, "status_code", None)
    if status == 429 or "rate" in msg or "429" in msg:
        return ErrorCategory.RATE_LIMITED
    if status == 401 or "auth" in msg or "401" in msg:
        return ErrorCategory.AUTH_FAILED
    if status in (503, 502, 500) or "timeout" in msg or "503" in msg or "overload" in msg:
        return ErrorCategory.TRANSIENT
    return ErrorCategory.NON_TRANSIENT


async def retry_with_backoff(
    fn: Callable[[], Awaitable[object]],
    *,
    max_attempts: int = 3,
    base_delay: float = 1.0,
    max_delay: float = 30.0,
) -> object:
    last_error: Exception | None = None
    for attempt in range(max_attempts):
        try:
            return await fn()
        except ProviderError as e:
            category = classify_error(e)
            if category != ErrorCategory.TRANSIENT:
                raise
            last_error = e
            if attempt < max_attempts - 1:
                delay = min(base_delay * (2 ** attempt) + random.uniform(0, 1), max_delay)
                await asyncio.sleep(delay)
    raise last_error or ProviderError("Retry exhausted")
