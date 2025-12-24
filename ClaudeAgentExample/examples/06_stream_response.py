#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
示例 06: 流式响应示例

展示如何使用流式响应来实时显示 Claude 的输出。

功能演示：
- 流式输出处理
- 实时响应显示
- 打字机效果
- 流式成本统计
"""

import sys
import anyio
import asyncio
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


async def basic_stream_example():
    """示例 1: 基础流式输出"""
    print("\n📝 示例 1: 基础流式输出")
    print("-" * 40)

    message_stream = query(
        prompt="请用100字左右介绍一下人工智能的历史。"
    )

    full_response = ""

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    # 实时打印每个文本块
                    print(block.text, end="", flush=True)
                    full_response += block.text
        elif isinstance(message, ResultMessage):
            print()  # 换行
            if message.total_cost_usd > 0:
                print_cost(message.total_cost_usd)

    print(f"\n📊 总字符数: {len(full_response)}")


async def typewriter_effect_example():
    """示例 2: 打字机效果"""
    print("\n📝 示例 2: 打字机效果")
    print("-" * 40)

    message_stream = query(
        prompt="请用一句诗描述月亮。"
    )

    print("🤖 Claude: ", end="", flush=True)

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    # 逐字符打印，模拟打字机效果
                    for char in block.text:
                        print(char, end="", flush=True)
                        await asyncio.sleep(0.02)  # 20ms 延迟
        elif isinstance(message, ResultMessage):
            print()  # 换行


async def stream_with_progress_example():
    """示例 3: 带进度的流式输出"""
    print("\n📝 示例 3: 带进度的流式输出")
    print("-" * 40)

    message_stream = query(
        prompt="请列出5个Python编程的最佳实践。"
    )

    char_count = 0
    print("🤖 Claude: ", end="", flush=True)

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    for char in block.text:
                        print(char, end="", flush=True)
                        char_count += 1
                        # 每50个字符显示一次进度
                        if char_count % 50 == 0:
                            print(f" [{char_count}]", end="", flush=True)
        elif isinstance(message, ResultMessage):
            print()  # 换行
            print(f"\n📊 总字符数: {char_count}")


async def stream_with_callback_example():
    """示例 4: 使用回调函数处理流式数据"""
    print("\n📝 示例 4: 流式数据处理")
    print("-" * 40)

    # 数据收集回调
    collected_data = {
        "text_chunks": [],
        "total_chars": 0,
    }

    async def collect_text(message):
        """收集文本数据的回调函数"""
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    collected_data["text_chunks"].append(block.text)
                    collected_data["total_chars"] += len(block.text)
        elif isinstance(message, ResultMessage):
            collected_data["cost"] = message.total_cost_usd

    # 执行查询
    message_stream = query(
        prompt="什么是面向对象编程？请简要解释。"
    )

    print("🤖 Claude: ", end="", flush=True)

    async for message in message_stream:
        # 同时显示和收集
        await collect_text(message)

        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(block.text, end="", flush=True)

    print()  # 换行

    # 显示收集的统计信息
    print(f"\n📊 流式数据统计:")
    print(f"   - 文本块数: {len(collected_data['text_chunks'])}")
    print(f"   - 总字符数: {collected_data['total_chars']}")
    if "cost" in collected_data and collected_data["cost"] > 0:
        print(f"   - 成本: ${collected_data['cost']:.6f}")


async def stream_comparison_example():
    """示例 5: 流式 vs 非流式对比"""
    print("\n📝 示例 5: 流式 vs 非流式对比")
    print("-" * 40)

    import time

    prompt = "请解释什么是递归。"

    # 流式响应
    print("\n⚡ 流式响应:")
    start_time = time.time()

    message_stream = query(prompt=prompt)

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(block.text, end="", flush=True)

    stream_time = time.time() - start_time
    print(f"\n⏱️  流式响应耗时: {stream_time:.2f}秒")

    # 非流式响应（通过收集全部流式数据模拟）
    print("\n📦 非流式响应 (收集全部后显示):")

    start_time = time.time()

    message_stream = query(prompt=prompt)

    full_text = ""
    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    full_text += block.text

    non_stream_time = time.time() - start_time

    print(full_text)
    print(f"\n⏱️  非流式响应耗时: {non_stream_time:.2f}秒")

    print(f"\n💡 结论: 流式响应让用户更早看到内容，体验更好！")


async def main():
    """运行所有流式响应示例"""
    print_example_header(
        "Claude Agent SDK - 流式响应示例",
        "展示如何使用流式响应实时显示输出"
    )

    # 验证配置
    config = get_config()
    if not config.anthropic_api_key:
        print("❌ 错误: 未设置 ANTHROPIC_API_KEY")
        print("请在 config/.env 文件中配置 API 密钥")
        return

    try:
        # 运行示例
        await basic_stream_example()
        await typewriter_effect_example()
        await stream_with_progress_example()
        await stream_with_callback_example()
        await stream_comparison_example()

        print("\n" + "=" * 50)
        print("✅ 所有流式响应示例完成!")
        print("=" * 50)

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    anyio.run(main)
