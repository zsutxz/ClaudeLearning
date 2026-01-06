#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""合并的Claude Agent SDK测试 - 包含基础示例和代理测试功能"""

import anyio
import asyncio
import logging
import os
import sys
from pathlib import Path
from typing import Optional

# 设置UTF-8编码
if sys.platform == "win32":
    import locale
    try:
        # 尝试设置UTF-8编码
        os.system('chcp 65001 >nul')
    except:
        pass

import logging
logging.getLogger("claude_agent_sdk").setLevel(logging.WARNING)
  
# 获取项目根目录
project_root = Path(__file__).parent.parent.resolve()

# 加载.env文件中的环境变量
try:
    from dotenv import load_dotenv
    # 尝试从config目录加载.env文件
    env_file = project_root / "config" / ".env"
    if env_file.exists():
        load_dotenv(env_file)
    else:
        load_dotenv()
except ImportError:
    # 如果没有python-dotenv，手动读取.env文件
    env_paths = [
        project_root / "config" / ".env",
        project_root / ".env",
    ]
    for env_file in env_paths:
        if env_file.exists():
            with open(env_file, 'r') as f:
                for line in f:
                    if '=' in line and not line.strip().startswith('#'):
                        key, value = line.strip().split('=', 1)
                        os.environ[key] = value
            break

# 确保API密钥存在
if not os.getenv('ANTHROPIC_API_KEY'):
    raise ValueError(f"请设置ANTHROPIC_API_KEY环境变量或在以下位置配置.env文件:\n{project_root / 'config' / '.env'}")

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
        message_stream = query(prompt="What is 2 + 5?")
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
            prompt="Create a file called hello.txt in the current directory(./) with 'Hello, World!' in it.",
            options=options,
        )
        await process_messages(message_stream, show_cost=True)
    except Exception as e:
        logger.error(f"Tools example failed: {e}")
        print(f"Error: {e}")
    print()


async def agent_test_example():
    """代理测试示例 - 代码审查功能"""
    print("=== Agent Test Example ===")

    try:
        # 使用代理配置进行代码审查测试，配置glm-4.7模型
        options = ClaudeAgentOptions(
            system_prompt="您是一位代码审查专家，具有安全、性能和最佳实践方面的专业知识。审查代码时：识别安全漏洞、检查性能问题、验证编码标准的遵守情况、建议具体改进。",
            allowed_tools=["Read", "Grep", "Glob", "Bash"],
            max_turns=2,
            model="glm-4.7"
        )

        message_stream = query(
            prompt="审查当前目录中的Python文件，识别安全问题和性能问题",
            options=options
        )

        # 使用详细的消息处理逻辑
        async for message in message_stream:
            message_type = type(message).__name__
            print(f"Message type: {message_type}")

            if message_type == "ResultMessage":
                print(f"Result: {message.result}")
                if hasattr(message, 'total_cost_usd') and message.total_cost_usd > 0:
                    print(f"Cost: ${message.total_cost_usd:.4f}")
            elif message_type == "AssistantMessage":
                for block in message.content:
                    if hasattr(block, 'text'):
                        print(f"Assistant: {block.text}")
                    else:
                        print(f"Assistant: {block}")
            elif message_type == "SystemMessage":
                print(f"System: {getattr(message, 'content', str(message))}")
            elif message_type == "UserMessage":
                print(f"User: {getattr(message, 'content', str(message))}")
            else:
                print(f"Other: {str(message)}")

    except Exception as e:
        logger.error(f"Agent test failed: {e}")
        print(f"Error: {e}")
        if "cancel scope" in str(e) or "Event loop is closed" in str(e):
            print("Ignoring known library cleanup issues")
        else:
            raise
    print()


async def main():
    """运行所有示例"""
    logger.info("Starting Claude Agent SDK test suite")

    try:
        
        # 基础示例
        print("--- 基础功能测试 ---")
        await basic_example()
        await with_options_example()
        await with_tools_example()

        # 代理测试示例
        print("--- 代理功能测试 ---")
        await agent_test_example()

        logger.info("All tests completed successfully")
        print("=== 所有测试完成 ===")

    except Exception as e:
        logger.error(f"Test suite failed: {e}")
        print(f"测试失败: {e}")


if __name__ == "__main__":
    anyio.run(main)