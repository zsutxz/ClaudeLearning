#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
示例 05: 会话管理示例

展示如何管理对话会话和上下文。

功能演示：
- 对话历史管理
- 上下文维护
- 会话持久化
"""

import sys
from pathlib import Path

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))

from lib.multi_agent import UniversalAIAgent
from lib.config import get_config
from lib.utils import print_example_header


def conversation_history_example():
    """示例 1: 对话历史管理"""
    print("\n📝 示例 1: 对话历史管理")
    print("-" * 40)

    config = get_config()
    provider = "claude" if config.anthropic_api_key else "mock"

    agent = UniversalAIAgent(provider=provider)

    # 多轮对话
    questions = [
        "我叫小明，请记住我的名字。",
        "我叫什么名字？",
        "我喜欢编程，特别是 Python。",
    ]

    for question in questions:
        print(f"\n👤 用户: {question}")
        response = agent.chat(question)
        print(f"🤖 AI: {response}")

    # 查看对话摘要
    print(f"\n📊 {agent.get_conversation_summary()}")


def clear_history_example():
    """示例 2: 清空对话历史"""
    print("\n📝 示例 2: 清空对话历史")
    print("-" * 40)

    config = get_config()
    provider = "claude" if config.anthropic_api_key else "mock"

    agent = UniversalAIAgent(provider=provider)

    # 添加系统提示词
    agent.add_system_prompt("你是一个友好的助手。")

    # 进行一些对话
    agent.chat("你好！")
    agent.chat("今天天气怎么样？")

    print(f"清空前: {agent.get_conversation_summary()}")

    # 清空历史（保留系统提示词）
    agent.clear_history()

    print(f"清空后: {agent.get_conversation_summary()}")


def system_prompt_example():
    """示例 3: 系统提示词影响"""
    print("\n📝 示例 3: 系统提示词影响")
    print("-" * 40)

    config = get_config()
    provider = "claude" if config.anthropic_api_key else "mock"

    # 创建两个不同角色的代理
    professional_agent = UniversalAIAgent(provider=provider)
    professional_agent.add_system_prompt("你是一位专业的技术顾问，用正式、准确的语言回答。")

    casual_agent = UniversalAIAgent(provider=provider)
    casual_agent.add_system_prompt("你是一位友好的朋友，用轻松、口语化的方式交流。")

    question = "什么是 Python？"

    print(f"\n👔 专业顾问:")
    response = professional_agent.chat(question)
    print(f"{response}")

    print(f"\n🤝 友好朋友:")
    response = casual_agent.chat(question)
    print(f"{response}")


def main():
    """运行所有会话管理示例"""
    print_example_header(
        "会话管理示例",
        "展示对话历史、上下文管理和会话持久化"
    )

    try:
        conversation_history_example()
        clear_history_example()
        system_prompt_example()

        print("\n" + "=" * 50)
        print("✅ 所会话管理示例完成!")
        print("=" * 50)

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    main()
