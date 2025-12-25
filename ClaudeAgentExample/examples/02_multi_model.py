#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
示例 02: 多模型支持示例

展示如何使用不同的 AI 模型和自定义 API 端点。

功能演示：
- 不同模型的选择和使用
- 自定义 base_url 配置
- 模型参数调优
- 模型能力对比
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


async def test_default_model():
    """测试默认模型 (glm-4.7)"""
    print("\n📝 测试 1: 默认模型 (glm-4.7)")
    print("-" * 40)

    message_stream = query(
        prompt="请用一句话介绍你自己。",
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"🤖 回复: {block.text}")
        elif isinstance(message, ResultMessage):
            if message.total_cost_usd > 0:
                print_cost(message.total_cost_usd)


async def test_custom_temperature():
    """测试不同的温度参数"""
    print("\n📝 测试 2: 温度参数对比")
    print("-" * 40)

    prompt = "请写一个简短的故事开头，关于一只猫的冒险。"

    # 低温度 - 更确定性的输出
    print("\n🌡️  低温度 (0.2) - 输出更确定:")
    options_low = ClaudeAgentOptions(
        model="glm-4.7",
        temperature=0.2,
        max_tokens=200,
    )

    message_stream = query(prompt=prompt, options=options_low)
    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")

    # 高温度 - 更创造性的输出
    print("\n🌡️  高温度 (0.9) - 输出更有创造性:")
    options_high = ClaudeAgentOptions(
        model="glm-4.7",
        temperature=0.9,
        max_tokens=200,
    )

    message_stream = query(prompt=prompt, options=options_high)
    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")


async def test_token_limits():
    """测试不同的 token 限制"""
    print("\n📝 测试 3: Token 限制")
    print("-" * 40)

    prompt = "请详细介绍人工智能的发展历史，从图灵测试到现代大语言模型。"

    # 短回复
    print("\n📏 短回复 (100 tokens):")
    options_short = ClaudeAgentOptions(
        model="glm-4.7",
        max_tokens=100,
    )

    message_stream = query(prompt=prompt, options=options_short)
    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")


async def test_system_prompts():
    """测试不同的系统提示词"""
    print("\n📝 测试 4: 系统提示词影响")
    print("-" * 40)

    prompt = "什么是 Python？"

    # 专业助手
    print("\n👔 专业助手角色:")
    options_professional = ClaudeAgentOptions(
        model="glm-4.7",
        system_prompt="你是一位专业的技术文档编写者，用正式、准确的语言回答问题。",
        max_turns=1,
    )

    message_stream = query(prompt=prompt, options=options_professional)
    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")

    # 幽默助手
    print("\n🤡 幽默助手角色:")
    options_funny = ClaudeAgentOptions(
        model="glm-4.7",
        system_prompt="你是一位幽默风趣的老师，用轻松有趣的方式解释复杂概念。",
        max_turns=1,
    )

    message_stream = query(prompt=prompt, options=options_funny)
    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")


async def main():
    """运行所有多模型示例"""
    print_example_header(
        "Claude Agent SDK - 多模型支持示例",
        "展示不同模型和参数的使用方法"
    )

    # 验证配置
    config = get_config()
    if not config.anthropic_api_key:
        print("❌ 错误: 未设置 ANTHROPIC_API_KEY")
        print("请在 config/.env 文件中配置 API 密钥")
        return

    try:
        # 运行示例
        await test_default_model()
        await test_custom_temperature()
        await test_token_limits()
        await test_system_prompts()

        print("\n" + "=" * 50)
        print("✅ 所有多模型示例完成!")
        print("=" * 50)

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    anyio.run(main)
