"""Tests for heagent.config — Settings, get_settings, reset_settings."""

from __future__ import annotations

from typing import Any

import pytest
from pydantic import ValidationError

from heagent.config import Settings, get_settings, reset_settings


@pytest.fixture(autouse=True)
def _clean_settings() -> Any:
    """Reset settings singleton before and after each test."""
    reset_settings()
    yield
    reset_settings()


# --- Default values ---


class TestDefaults:
    def test_default_model(self) -> None:
        s = Settings()
        assert s.default_model == "gpt-4o"

    def test_default_max_iterations(self) -> None:
        s = Settings()
        assert s.max_iterations == 50

    def test_default_compression_threshold(self) -> None:
        s = Settings()
        assert s.compression_threshold == 0.8

    def test_default_shell_timeout(self) -> None:
        s = Settings()
        assert s.shell_timeout == 120

    def test_default_retry_params(self) -> None:
        s = Settings()
        assert s.retry_max_attempts == 3
        assert s.retry_base_delay == 1.0
        assert s.retry_max_delay == 30.0

    def test_default_single_api_keys_none(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.delenv("OPENAI_API_KEY", raising=False)
        monkeypatch.delenv("ANTHROPIC_API_KEY", raising=False)
        s = Settings()
        assert s.openai_api_key is None
        assert s.anthropic_api_key is None

    def test_default_api_keys_empty_str(self) -> None:
        s = Settings()
        assert s.openai_api_keys == ""
        assert s.anthropic_api_keys == ""

    def test_default_key_pools_empty(self) -> None:
        s = Settings()
        assert s.openai_key_pool == []
        assert s.anthropic_key_pool == []


# --- Environment variable loading ---


class TestEnvLoading:
    def test_openai_key_from_env(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("OPENAI_API_KEY", "sk-test-123")
        s = Settings()
        assert s.openai_api_key == "sk-test-123"

    def test_anthropic_key_from_env(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("ANTHROPIC_API_KEY", "ant-key-456")
        s = Settings()
        assert s.anthropic_api_key == "ant-key-456"

    def test_max_iterations_from_env(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("MAX_ITERATIONS", "100")
        s = Settings()
        assert s.max_iterations == 100

    def test_default_model_from_env(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("DEFAULT_MODEL", "claude-sonnet-4-6")
        s = Settings()
        assert s.default_model == "claude-sonnet-4-6"


# --- Multi-key parsing ---


class TestMultiKeyParsing:
    def test_comma_separated_openai_keys(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("OPENAI_API_KEYS", "sk-1,sk-2,sk-3")
        s = Settings()
        assert s.openai_key_pool == ["sk-1", "sk-2", "sk-3"]

    def test_comma_separated_anthropic_keys(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("ANTHROPIC_API_KEYS", "key-a,key-b")
        s = Settings()
        assert s.anthropic_key_pool == ["key-a", "key-b"]

    def test_empty_string_gives_empty_list(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("OPENAI_API_KEYS", "")
        s = Settings()
        assert s.openai_key_pool == []

    def test_single_key_no_comma(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("OPENAI_API_KEYS", "sk-single")
        s = Settings()
        assert s.openai_key_pool == ["sk-single"]


# --- Validation ---


class TestValidation:
    def test_threshold_above_1_fails(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("COMPRESSION_THRESHOLD", "1.5")
        with pytest.raises(ValidationError):
            Settings()

    def test_threshold_zero_ok(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("COMPRESSION_THRESHOLD", "0.0")
        s = Settings()
        assert s.compression_threshold == 0.0

    def test_threshold_one_ok(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("COMPRESSION_THRESHOLD", "1.0")
        s = Settings()
        assert s.compression_threshold == 1.0

    def test_negative_max_iterations_fails(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("MAX_ITERATIONS", "-1")
        with pytest.raises(ValidationError):
            Settings()

    def test_non_numeric_max_iterations_fails(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("MAX_ITERATIONS", "abc")
        with pytest.raises(ValidationError):
            Settings()

    def test_max_iterations_zero_fails(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("MAX_ITERATIONS", "0")
        with pytest.raises(ValidationError):
            Settings()

    def test_shell_timeout_zero_fails(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("SHELL_TIMEOUT", "0")
        with pytest.raises(ValidationError):
            Settings()

    def test_retry_max_attempts_zero_fails(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("RETRY_MAX_ATTEMPTS", "0")
        with pytest.raises(ValidationError):
            Settings()

    def test_retry_base_delay_negative_fails(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("RETRY_BASE_DELAY", "-1.0")
        with pytest.raises(ValidationError):
            Settings()

    def test_retry_max_delay_negative_fails(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("RETRY_MAX_DELAY", "-5.0")
        with pytest.raises(ValidationError):
            Settings()


# --- Singleton ---


class TestSingleton:
    def test_get_settings_returns_instance(self) -> None:
        s = get_settings()
        assert isinstance(s, Settings)

    def test_get_settings_returns_same_instance(self) -> None:
        s1 = get_settings()
        s2 = get_settings()
        assert s1 is s2

    def test_reset_clears_singleton(self) -> None:
        s1 = get_settings()
        reset_settings()
        s2 = get_settings()
        assert s1 is not s2

    def test_singleton_reads_env(self, monkeypatch: pytest.MonkeyPatch) -> None:
        monkeypatch.setenv("MAX_ITERATIONS", "75")
        s = get_settings()
        assert s.max_iterations == 75
