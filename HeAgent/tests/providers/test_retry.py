"""Tests for error classification and retry logic."""

from __future__ import annotations

import pytest

from heagent.exceptions import ProviderError
from heagent.providers.retry import ErrorCategory, classify_error, retry_with_backoff


class TestClassifyError:
    def test_rate_limited_429(self) -> None:
        e = ProviderError("Rate limited")
        e.status_code = 429
        assert classify_error(e) == ErrorCategory.RATE_LIMITED

    def test_rate_limited_message(self) -> None:
        assert classify_error(ProviderError("rate limit exceeded")) == ErrorCategory.RATE_LIMITED

    def test_auth_failed_401(self) -> None:
        e = ProviderError("Unauthorized")
        e.status_code = 401
        assert classify_error(e) == ErrorCategory.AUTH_FAILED

    def test_transient_503(self) -> None:
        e = ProviderError("Service unavailable")
        e.status_code = 503
        assert classify_error(e) == ErrorCategory.TRANSIENT

    def test_transient_timeout(self) -> None:
        assert classify_error(ProviderError("connection timeout")) == ErrorCategory.TRANSIENT

    def test_non_transient(self) -> None:
        e = ProviderError("Bad request")
        e.status_code = 400
        assert classify_error(e) == ErrorCategory.NON_TRANSIENT


class TestRetryWithBackoff:
    async def test_success_no_retry(self) -> None:
        result = await retry_with_backoff(lambda: _async_ok("done"))
        assert result == "done"

    async def test_transient_retries_then_succeeds(self) -> None:
        call_count = 0

        async def flaky():
            nonlocal call_count
            call_count += 1
            if call_count < 3:
                e = ProviderError("timeout")
                e.status_code = 503
                raise e
            return "ok"

        result = await retry_with_backoff(flaky, base_delay=0.01, max_delay=0.02)
        assert result == "ok"
        assert call_count == 3

    async def test_non_transient_fails_immediately(self) -> None:
        call_count = 0

        async def fail():
            nonlocal call_count
            call_count += 1
            e = ProviderError("Bad request")
            e.status_code = 400
            raise e

        with pytest.raises(ProviderError):
            await retry_with_backoff(fail, base_delay=0.01)
        assert call_count == 1

    async def test_rate_limited_no_retry(self) -> None:
        call_count = 0

        async def fail():
            nonlocal call_count
            call_count += 1
            e = ProviderError("Rate limited")
            e.status_code = 429
            raise e

        with pytest.raises(ProviderError):
            await retry_with_backoff(fail, base_delay=0.01)
        assert call_count == 1

    async def test_exhausted_raises(self) -> None:
        async def always_fail():
            e = ProviderError("timeout")
            e.status_code = 503
            raise e

        with pytest.raises(ProviderError, match="timeout"):
            await retry_with_backoff(always_fail, max_attempts=2, base_delay=0.01)


async def _async_ok(val: str) -> str:
    return val
