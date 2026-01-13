#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
示例 06: 流式响应示例

展示如何使用流式响应功能。

功能演示：
- 同步响应
- 流式响应
- 实时输出处理
"""

import sys
from pathlib import Path

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent.parent.parent
sys.path.insert(0, str(project_root))

from lib.multi_agent import UniversalAIAgent
from lib.config import get_config
from lib.utils import print_example_header


def sync_response_example():
    """示例 1: 同步响应"""
    print("\n📝 示例 1: 同步响应")
    print("-" * 40)

    config = get_config()
    provider = "claude" if config.anthropic_api_key else "mock"

    agent = UniversalAIAgent(provider=provider)

    response = agent.chat("请用一句话介绍 Python 编程语言。", stream=False)
    print(f"🤖 完整回复: {response}")


def stream_response_example():
    """示例 2: 流式响应"""
    print("\n📝 示例 2: 流式响应（实时输出）")
    print("-" * 40)

    config = get_config()
    provider = "claude" if config.anthropic_api_key else "mock"

    agent = UniversalAIAgent(provider=provider)

    response = agent.chat("请写一个简短的故事，关于一只勇敢的小猫。", stream=True)


def long_content_example():
    """示例 3: 长内容流式输出"""
    print("\n📝 示例 3: 长内容流式输出")
    print("-" * 40)

    config = get_config()
    provider = "claude" if config.anthropic_api_key else "mock"

    agent = UniversalAIAgent(provider=provider)

    response = agent.chat(
        "请详细介绍人工智能的发展历史，包括机器学习、深度学习和大语言模型的演进。",
        stream=True
    )


def main():
    """运行所有流式响应示例"""
    print_example_header(
        "流式响应示例",
        "展示同步响应和流式响应的使用方法"
    )

    try:
        sync_response_example()
        stream_response_example()
        long_content_example()

        print("\n" + "=" * 50)
        print("✅ 所流式响应示例完成!")
        print("=" * 50)

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    main()
