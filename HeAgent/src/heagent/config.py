"""HeAgent configuration management via pydantic-settings."""

from __future__ import annotations

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict

_settings: Settings | None = None


def _parse_comma_list(v: str) -> list[str]:
    if not v:
        return []
    return [k.strip() for k in v.split(",") if k.strip()]


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        extra="ignore",
    )

    # API Keys — optional, validated at Provider usage time
    openai_api_key: str | None = None
    anthropic_api_key: str | None = None

    # Multi-key pools — comma-separated in env, stored as str for env parsing
    openai_api_keys: str = ""
    anthropic_api_keys: str = ""

    # Framework parameters
    default_model: str = "gpt-4o"
    max_iterations: int = Field(default=50, ge=1)
    compression_threshold: float = Field(default=0.8, ge=0.0, le=1.0)
    shell_timeout: int = Field(default=120, ge=1)

    # Retry parameters
    retry_max_attempts: int = Field(default=3, ge=1)
    retry_base_delay: float = Field(default=1.0, ge=0.0)
    retry_max_delay: float = Field(default=30.0, ge=0.0)

    @property
    def openai_key_pool(self) -> list[str]:
        return _parse_comma_list(self.openai_api_keys)

    @property
    def anthropic_key_pool(self) -> list[str]:
        return _parse_comma_list(self.anthropic_api_keys)


def get_settings() -> Settings:
    global _settings
    if _settings is None:
        _settings = Settings()
    return _settings


def reset_settings() -> None:
    global _settings
    _settings = None
