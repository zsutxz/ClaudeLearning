#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
示例 01: 基础对话示例

这是最简单的入门示例，展示如何使用 Claude Agent SDK 进行基础对话。

功能演示：
- 基本的问答对话
- query() 函数的使用
- 消息流处理
- 成本显示
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
from lib.utils import print_example_header, print_cost


async def basic_question_example():
    """示例 1: 简单的数学问题"""
    print("\n📝 示例 1: 简单的数学问题")
    print("-" * 40)

    message_stream = query(prompt="2 + 3 等于多少？请用中文回答。")

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"🤖 Claude: {block.text}")
        elif isinstance(message, ResultMessage):
            if message.total_cost_usd > 0:
                print_cost(message.total_cost_usd)


async def custom_options_example():
    """示例 2: 使用自定义选项"""
    print("\n📝 示例 2: 使用自定义选项（系统提示词 + 单轮对话）")
    print("-" * 40)

    # 创建自定义选项
    options = ClaudeAgentOptions(
        system_prompt="你是一个简洁的助手，所有回答都用一句话完成。",
        max_turns=1,  # 限制为一轮对话
    )

    message_stream = query(
        prompt="请解释什么是 Python 编程语言？",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"🤖 Claude: {block.text}")
        elif isinstance(message, ResultMessage):
            if message.total_cost_usd > 0:
                print_cost(message.total_cost_usd)


async def conversation_example():
    """示例 3: 多轮对话"""
    print("\n📝 示例 3: 多轮对话")
    print("-" * 40)

    questions = [
        "我叫小明，请记住我的名字。",
        "我叫什么名字？",
        "我喜欢编程，特别是 Python。请记住这个。",
        "根据我告诉你的信息，介绍一下我。",
    ]

    options = ClaudeAgentOptions(
        max_turns=1,  # 每个问题单独一轮
    )

    for i, question in enumerate(questions, 1):
        print(f"\n👤 用户 (问题 {i}): {question}")

        message_stream = query(prompt=question, options=options)

        async for message in message_stream:
            if isinstance(message, AssistantMessage):
                for block in message.content:
                    if isinstance(block, TextBlock):
                        print(f"🤖 Claude: {block.text}")


async def main():
    """运行所有基础对话示例"""
    print_example_header(
        "Claude Agent SDK - 基础对话示例",
        "展示最简单的对话功能和使用方法"
    )

    # 验证配置
    config = get_config()
    if not config.anthropic_api_key:
        print("❌ 错误: 未设置 ANTHROPIC_API_KEY")
        print("请在 config/.env 文件中配置 API 密钥")
        return

    try:
        # 运行示例
        await basic_question_example()
        await custom_options_example()
        await conversation_example()

        print("\n" + "=" * 50)
        print("✅ 所有基础对话示例完成!")
        print("=" * 50)

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    anyio.run(main)
