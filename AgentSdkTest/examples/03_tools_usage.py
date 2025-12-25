#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
示例 03: 工具使用示例

展示如何使用 Claude Agent SDK 的工具功能。

功能演示：
- 基础工具使用（Read, Write, Grep, Bash）
- 文件操作
- 代码搜索
- 命令执行
"""

import sys
import anyio
from pathlib import Path

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))

from claude_agent_sdk import (
    query,
    ClaudeAgentOptions,
    AssistantMessage,
    ResultMessage,
    TextBlock,
)

from lib.config import get_config
from lib.utils import print_example_header


async def read_file_example():
    """示例 1: 读取文件"""
    print("\n📝 示例 1: 读取文件内容")
    print("-" * 40)

    options = ClaudeAgentOptions(
        allowed_tools=["Read"],
        max_turns=5,
    )

    message_stream = query(
        prompt="请读取 examples/01_basic_chat.py 文件的内容",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}", end="", flush=True)
        elif isinstance(message, ResultMessage):
            print()


async def write_file_example():
    """示例 2: 写入文件"""
    print("\n📝 示例 2: 创建新文件")
    print("-" * 40)

    options = ClaudeAgentOptions(
        allowed_tools=["Read", "Write"],
        max_turns=5,
    )

    message_stream = query(
        prompt="请在当前目录创建一个名为 test_output.txt 的文件，写入 'Hello from Claude Agent SDK!'",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}", end="", flush=True)
        elif isinstance(message, ResultMessage):
            print()


async def search_code_example():
    """示例 3: 搜索代码"""
    print("\n📝 示例 3: 搜索代码中的特定模式")
    print("-" * 40)

    options = ClaudeAgentOptions(
        allowed_tools=["Grep"],
        max_turns=5,
    )

    message_stream = query(
        prompt="请在 lib/ 目录中搜索所有包含 'UniversalAIAgent' 的文件",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}", end="", flush=True)
        elif isinstance(message, ResultMessage):
            print()


async def main():
    """运行所有工具使用示例"""
    print_example_header(
        "工具使用示例",
        "展示文件读写、代码搜索、命令执行等工具功能"
    )

    # 验证配置
    config = get_config()
    if not config.anthropic_api_key:
        print("❌ 错误: 未设置 ANTHROPIC_API_KEY")
        print("请在 .env 文件中配置 API 密钥")
        return

    try:
        # 运行示例
        await read_file_example()
        await write_file_example()
        await search_code_example()

        print("\n" + "=" * 50)
        print("✅ 所有工具使用示例完成!")
        print("=" * 50)

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    anyio.run(main)
