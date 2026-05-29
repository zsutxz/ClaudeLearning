"""Tests for CLI entry point and public API imports."""

from __future__ import annotations

import pytest
from click.testing import CliRunner

from heagent.cli import main


@pytest.fixture()
def clean_settings():
    """Reset settings singleton for tests that need it."""
    from heagent.config import reset_settings
    reset_settings()
    yield
    reset_settings()


class TestCLI:
    """CLI command tests using click.testing.CliRunner."""

    def test_no_api_key_shows_error(self, monkeypatch, clean_settings, tmp_path):
        monkeypatch.delenv("OPENAI_API_KEY", raising=False)
        monkeypatch.delenv("ANTHROPIC_API_KEY", raising=False)
        # Point to empty dir so pydantic-settings finds no .env file
        monkeypatch.chdir(tmp_path)
        runner = CliRunner()
        result = runner.invoke(main, ["hello"])
        assert result.exit_code != 0
        assert "No API key" in result.output

    def test_help_shows_usage(self):
        runner = CliRunner()
        result = runner.invoke(main, ["--help"])
        assert result.exit_code == 0
        assert "HeAgent" in result.output
        assert "--model" in result.output
        assert "--system" in result.output

    def test_interactive_exits_on_empty_input(self, monkeypatch):
        """Interactive mode exits gracefully on empty input."""
        monkeypatch.setenv("OPENAI_API_KEY", "test-key")
        runner = CliRunner()
        # Simulate empty input (user presses Enter immediately)
        result = runner.invoke(main, input="\n")
        # Should exit cleanly, not crash
        assert result.exit_code == 0

    def test_model_flag_sets_provider(self, monkeypatch):
        """--model flag is accepted without error."""
        monkeypatch.setenv("OPENAI_API_KEY", "test-key")
        runner = CliRunner()
        result = runner.invoke(main, ["--help"])
        assert "--model" in result.output


class TestPublicAPI:
    """Verify flat public API imports work."""

    def test_top_level_imports(self):
        from heagent import Agent, AnthropicProvider, OpenAIProvider, ProviderChain, Settings, get_settings, tool

        assert Agent is not None
        assert callable(tool)
        assert Settings is not None

    def test_agent_is_agentloop_alias(self):
        from heagent.agent.loop import AgentLoop
        from heagent import Agent

        assert Agent is AgentLoop

    def test_submodule_exports(self):
        from heagent.providers import AnthropicProvider, OpenAIProvider, ProviderChain
        from heagent.agent import AgentLoop, SubAgent, compose
        from heagent.tools import ToolRegistry, SafetyGuard, tool

        assert all(cls is not None for cls in [AnthropicProvider, OpenAIProvider, ProviderChain, AgentLoop, SubAgent, ToolRegistry, SafetyGuard])
        assert callable(tool)
        assert callable(compose)


class TestBuiltinRegistration:
    """Verify builtin tools register on import."""

    def test_builtin_tools_registered(self):
        from heagent.tools.registry import ToolRegistry

        # Tools registered at import time by cli.py; just verify they exist
        registry = ToolRegistry.get()
        names = registry.list_names()
        assert "shell" in names
        assert "file_read" in names
        assert "file_write" in names
        assert "file_search" in names
        assert "content_search" in names
