#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
示例 03: 工具使用示例

展示如何让 Claude 使用各种工具来完成任务。

功能演示：
- 配置 allowed_tools
- 文件读写工具 (Read, Write)
- 代码搜索工具 (Grep)
- 文件查找工具 (Glob)
- 命令执行工具 (Bash)
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


async def file_write_example():
    """示例 1: 创建文件"""
    print("\n📝 示例 1: 创建文件")
    print("-" * 40)

    options = ClaudeAgentOptions(
        allowed_tools=["Write"],
        system_prompt="你是一个文件创建助手，根据用户要求创建文件。",
    )

    message_stream = query(
        prompt="请在当前目录创建一个名为 greeting.txt 的文件，内容是 '你好，Claude Agent SDK！'",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")
        elif isinstance(message, ResultMessage):
            print(f"\n✅ 操作结果: {message.result}")
            if message.total_cost_usd > 0:
                print(f"💰 成本: ${message.total_cost_usd:.4f}")


async def file_read_example():
    """示例 2: 读取文件"""
    print("\n📝 示例 2: 读取文件")
    print("-" * 40)

    # 先创建一个示例文件
    sample_file = Path(__file__).parent / "sample.txt"
    sample_file.write_text("这是一个示例文件。\n包含多行文本。\n用于演示文件读取功能。", encoding="utf-8")

    options = ClaudeAgentOptions(
        allowed_tools=["Read"],
        system_prompt="你是一个文件读取助手，帮助用户查看文件内容。",
    )

    message_stream = query(
        prompt=f"请读取 {sample_file} 文件的内容并告诉我文件里写了什么。",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")
        elif isinstance(message, ResultMessage):
            print(f"\n✅ 操作完成")


async def file_search_example():
    """示例 3: 搜索文件内容"""
    print("\n📝 示例 3: 搜索文件内容")
    print("-" * 40)

    options = ClaudeAgentOptions(
        allowed_tools=["Grep", "Glob"],
        system_prompt="你是一个代码搜索助手，帮助用户在项目中查找内容。",
        max_turns=3,
    )

    message_stream = query(
        prompt="请在当前目录的 Python 文件中搜索包含 'import' 关键字的行。",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")
        elif isinstance(message, ResultMessage):
            print(f"\n✅ 搜索完成")


async def code_analysis_example():
    """示例 4: 代码分析"""
    print("\n📝 示例 4: 代码分析")
    print("-" * 40)

    options = ClaudeAgentOptions(
        allowed_tools=["Read", "Grep", "Glob"],
        system_prompt="你是一个代码分析专家，可以阅读和分析代码。",
        max_turns=5,
    )

    message_stream = query(
        prompt="请分析 ../lib 目录下的 Python 代码，告诉我有哪些模块和它们的功能。",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")
        elif isinstance(message, ResultMessage):
            print(f"\n✅ 分析完成")


async def file_operations_example():
    """示例 5: 综合文件操作"""
    print("\n📝 示例 5: 综合文件操作")
    print("-" * 40)

    options = ClaudeAgentOptions(
        allowed_tools=["Read", "Write", "Grep", "Glob", "Bash"],
        system_prompt="你是一个文件操作助手，可以帮助用户完成各种文件相关任务。",
        max_turns=5,
    )

    message_stream = query(
        prompt="请执行以下操作：\n"
              "1. 创建一个名为 todo.txt 的文件\n"
              "2. 写入三条待办事项\n"
              "3. 读取文件确认内容",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")
        elif isinstance(message, ResultMessage):
            print(f"\n✅ 操作完成")


async def main():
    """运行所有工具使用示例"""
    print_example_header(
        "Claude Agent SDK - 工具使用示例",
        "展示如何让 Claude 使用各种工具完成任务"
    )

    # 验证配置
    config = get_config()
    if not config.anthropic_api_key:
        print("❌ 错误: 未设置 ANTHROPIC_API_KEY")
        print("请在 config/.env 文件中配置 API 密钥")
        return

    try:
        # 运行示例
        await file_write_example()
        await file_read_example()
        await file_search_example()
        await code_analysis_example()
        await file_operations_example()

        print("\n" + "=" * 50)
        print("✅ 所有工具使用示例完成!")
        print("=" * 50)

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    anyio.run(main)
