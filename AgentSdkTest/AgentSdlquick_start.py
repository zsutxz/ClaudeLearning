#!/usr/bin/env python3
"""Improved Quick start example for Claude Code SDK."""

import anyio
import logging
import os
from typing import Optional

from claude_agent_sdk import (
    AssistantMessage,
    ClaudeAgentOptions,
    ResultMessage,
    TextBlock,
    query,
)

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)


async def process_messages(message_stream, show_cost: bool = False) -> None:
    """Process and display messages from Claude."""
    try:
        async for message in message_stream:
            if isinstance(message, AssistantMessage):
                for block in message.content:
                    if isinstance(block, TextBlock):
                        print(f"Claude: {block.text}")
            elif isinstance(message, ResultMessage) and show_cost and message.total_cost_usd > 0:
                print(f"\nCost: ${message.total_cost_usd:.4f}")
    except Exception as e:
        logger.error(f"Error processing messages: {e}")
        raise


async def basic_example():
    """Basic example - simple question."""
    print("=== Basic Example ===")

    try:
        message_stream = query(prompt="What is 2 + 2?")
        await process_messages(message_stream)
    except Exception as e:
        logger.error(f"Basic example failed: {e}")
        print(f"Error: {e}")
    print()


async def with_options_example():
    """Example with custom options."""
    print("=== With Options Example ===")

    try:
        options = ClaudeAgentOptions(
            system_prompt="You are a helpful assistant that explains things simply.",
            max_turns=1,
        )

        message_stream = query(
            prompt="Explain what Python is in one sentence.",
            options=options
        )
        await process_messages(message_stream)
    except Exception as e:
        logger.error(f"Options example failed: {e}")
        print(f"Error: {e}")
    print()


async def with_tools_example():
    """Example using tools."""
    print("=== With Tools Example ===")

    try:
        options = ClaudeAgentOptions(
            allowed_tools=["Read", "Write"],
            system_prompt="You are a helpful file assistant.",
        )

        message_stream = query(
            prompt="Create a file called hello.txt with 'Hello, World!' in it",
            options=options,
        )
        await process_messages(message_stream, show_cost=True)
    except Exception as e:
        logger.error(f"Tools example failed: {e}")
        print(f"Error: {e}")
    print()


async def main():
    """Run all examples."""
    logger.info("Starting Claude Agent SDK examples")

    try:
        await basic_example()
        await with_options_example()
        await with_tools_example()
        logger.info("All examples completed successfully")
    except Exception as e:
        logger.error(f"Application failed: {e}")
        print(f"Application error: {e}")


if __name__ == "__main__":
    anyio.run(main)