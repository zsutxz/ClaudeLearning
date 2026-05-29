"""HeAgent CLI — interactive and single-shot agent execution."""

from __future__ import annotations

import asyncio
import logging
import sys

import click

import heagent.tools.builtins  # noqa: F401 — trigger @tool registration
from heagent.agent.loop import AgentLoop
from heagent.config import get_settings, reset_settings
from heagent.exceptions import BudgetExceeded, HeAgentError
from heagent.providers.anthropic import AnthropicProvider
from heagent.providers.chain import ProviderChain
from heagent.providers.openai import OpenAIProvider
from heagent.providers.base import BaseProvider

logger = logging.getLogger(__name__)


def _build_provider(settings, model: str) -> BaseProvider:
    """Auto-detect provider from available API keys."""
    providers: list[BaseProvider] = []

    if settings.openai_api_key:
        providers.append(
            OpenAIProvider(api_key=settings.openai_api_key, model=model, base_url=settings.openai_base_url)
        )

    if settings.anthropic_api_key:
        providers.append(
            AnthropicProvider(api_key=settings.anthropic_api_key, model=model, base_url=settings.anthropic_base_url)
        )

    if not providers:
        click.echo(
            "Error: No API key configured. "
            "Set OPENAI_API_KEY or ANTHROPIC_API_KEY in .env or environment.",
            err=True,
        )
        raise SystemExit(1)

    if len(providers) == 1:
        return providers[0]

    return ProviderChain(providers)


async def _run_single(
    prompt: str,
    provider: BaseProvider,
    system: str | None,
    max_iterations: int,
) -> None:
    """Execute a single prompt and print the result."""
    loop = AgentLoop(provider, max_iterations=max_iterations)
    result = await loop.run(prompt, system=system)
    click.echo(result)


async def _run_chat(
    provider: BaseProvider,
    system: str | None,
    max_iterations: int,
) -> None:
    """Run an interactive chat loop."""
    loop = AgentLoop(provider, max_iterations=max_iterations)
    click.echo("HeAgent interactive mode. Type your message, or press Enter to exit.")

    while True:
        try:
            user_input = input("> ")
        except (KeyboardInterrupt, EOFError):
            click.echo("\nBye!")
            break

        if not user_input.strip():
            break

        try:
            result = await loop.run(user_input, system=system)
            click.echo(f"\n{result}\n")
        except BudgetExceeded as e:
            click.echo(f"[budget exceeded] {e.message}", err=True)
        except HeAgentError as e:
            click.echo(f"[error] {e.message}", err=True)


@click.command()
@click.argument("prompt", required=False)
@click.option("--model", default=None, help="Model name (default: from settings or gpt-4o)")
@click.option("--system", default=None, help="System prompt")
@click.option("--max-iterations", type=int, default=None, help="Max agent loop iterations")
def main(prompt: str | None, model: str | None, system: str | None, max_iterations: int | None) -> None:
    """HeAgent — self-improving AI agent.

    Run with a PROMPT for single-shot mode, or without for interactive chat.
    """
    # Fix Windows console encoding for CJK output
    if sys.stdout and hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")

    logging.basicConfig(
        level=logging.INFO,
        format="%(levelname)s [%(name)s] %(message)s",
        stream=sys.stderr,
    )

    settings = get_settings()
    resolved_model = model or settings.default_model
    resolved_iterations = max_iterations or settings.max_iterations

    provider = _build_provider(settings, resolved_model)

    if prompt:
        asyncio.run(_run_single(prompt, provider, system, resolved_iterations))
    else:
        asyncio.run(_run_chat(provider, system, resolved_iterations))
