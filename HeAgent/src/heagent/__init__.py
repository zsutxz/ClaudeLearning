"""HeAgent - A self-improving AI Agent core framework."""

__version__ = "0.1.0"

from heagent.agent.loop import AgentLoop as Agent
from heagent.config import Settings, get_settings
from heagent.providers.anthropic import AnthropicProvider
from heagent.providers.chain import ProviderChain
from heagent.providers.openai import OpenAIProvider
from heagent.tools.decorator import tool

__all__ = [
    "Agent",
    "AnthropicProvider",
    "OpenAIProvider",
    "ProviderChain",
    "Settings",
    "get_settings",
    "tool",
]
