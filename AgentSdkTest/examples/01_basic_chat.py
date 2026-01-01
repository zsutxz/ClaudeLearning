#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
示例 01: 基础对话示例

这是最简单的入门示例，展示如何使用 Claude Agent SDK 进行基础对话。

功能演示：
- 基本的问答对话
- query() 函数的使用
- 自定义选项（系统提示词、轮次限制）
- 消息流处理
- 成本显示
- 持久会话管理
"""

import sys
import anyio
from pathlib import Path

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))

from claude_agent_sdk import (
    query,
    ClaudeSDKClient,
    ClaudeAgentOptions,
    AssistantMessage,
    ResultMessage,
    TextBlock,
)

from lib.config import get_config
from lib.utils import print_example_header, print_cost


# ============================================================
# 示例 1-3: 使用 query() 函数的简单示例
# ============================================================

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
    """示例 3: 多轮对话（使用独立 query 调用）"""
    print("\n📝 示例 3: 多轮对话（独立 query 调用）")
    print("-" * 40)
    print("注意：这种方式每次调用都是独立的，不会保持上下文")

    questions = [
        "我叫小明，请记住我的名字。",
        "我叫什么名字？",
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


# ============================================================
# 示例 4: 使用 ClaudeSDKClient 的持久会话管理
# ============================================================

class ConversationSession:
    """
    持久对话会话管理类

    与 query() 函数不同，ClaudeSDKClient 维护一个持久的连接，
    所有对话都在同一个会话中进行，Claude 会记住之前的上下文。
    """

    def __init__(self, options: ClaudeAgentOptions = None):
        self.client = ClaudeSDKClient(options)
        self.turn_count = 0

    async def start(self):
        """启动交互式对话会话"""
        await self.client.connect()

        print("🎙️  持久会话模式启动")
        print("=" * 50)
        print("Claude 会记住之前的对话内容")
        print("\n可用命令:")
        print("  - 输入消息: 与 Claude 对话")
        print("  - 'exit' : 退出会话")
        print("  - 'new'  : 开始新会话（清除上下文）")
        print("  - 'demo' : 运行演示对话")
        print("=" * 50)

        while True:
            try:
                user_input = input(f"\n[轮次 {self.turn_count + 1}] 👤 你: ").strip()

                if not user_input:
                    continue

                # 处理命令
                if user_input.lower() == 'exit':
                    print("\n👋 会话结束")
                    break
                elif user_input.lower() == 'new':
                    await self._new_session()
                    continue
                elif user_input.lower() == 'demo':
                    await self._run_demo()
                    continue

                # 正常对话
                await self.client.query(user_input)
                self.turn_count += 1

                # 接收并显示响应
                print(f"[轮次 {self.turn_count}] 🤖 Claude: ", end="", flush=True)
                async for message in self.client.receive_response():
                    if isinstance(message, AssistantMessage):
                        for block in message.content:
                            if isinstance(block, TextBlock):
                                print(block.text, end="", flush=True)
                print()  # 换行

            except KeyboardInterrupt:
                print("\n\n⚠️  检测到中断信号")
                choice = input("是否退出会话？(y/n): ").strip().lower()
                if choice == 'y':
                    break
                else:
                    continue
            except Exception as e:
                print(f"\n❌ 错误: {e}")

        await self.client.disconnect()
        print(f"✅ 会话结束，共进行 {self.turn_count} 轮对话")

    async def _new_session(self):
        """开始新会话"""
        await self.client.disconnect()
        await self.client.connect()
        self.turn_count = 0
        print("\n🆕 新会话已启动（之前的上下文已清除）")

    async def _run_demo(self):
        """运行演示对话"""
        demo_conversations = [
            "你好，我叫小明，请记住我的名字。",
            "我叫什么名字？",
            "我喜欢 Python 编程，特别是 AI 和机器学习。",
            "根据我告诉你的信息，介绍一下我自己。",
        ]

        print("\n📋 演示对话（展示会话记忆功能）")
        print("-" * 50)

        for question in demo_conversations:
            await self.client.query(question)
            self.turn_count += 1

            print(f"\n[轮次 {self.turn_count}] 👤 你: {question}")
            print(f"[轮次 {self.turn_count}] 🤖 Claude: ", end="", flush=True)

            async for message in self.client.receive_response():
                if isinstance(message, AssistantMessage):
                    for block in message.content:
                        if isinstance(block, TextBlock):
                            print(block.text, end="", flush=True)
            print()

        print("-" * 50)
        print("✅ 演示完成")


async def session_management_example():
    """示例 4: 持久会话管理（自动演示模式）"""
    print("\n📝 示例 4: 持久会话管理")
    print("-" * 40)

    options = ClaudeAgentOptions(
        allowed_tools=["Read", "Write", "Bash"],
        permission_mode="default"
    )

    print("\n演示自动对话流程...")
    print("（提示：重新运行程序并选择交互模式可体验完整功能）\n")

    # 创建会话并运行演示
    session = ConversationSession(options)

    # 连接客户端
    await session.client.connect()

    # 演示对话序列
    demo_conversations = [
        ("创建文件", "请创建一个名为 hello.txt 的文件，内容是 'Hello, World!'"),
        ("查询内容", "查看 hello.txt 文件的内容"),
        ("修改文件", "在 hello.txt 文件中添加一行 'This is a demo'"),
        ("再次查询", "再次查看 hello.txt 的内容"),
    ]

    for i, (desc, prompt) in enumerate(demo_conversations, 1):
        print(f"\n[步骤 {i}] {desc}")
        print(f"👤 你: {prompt}")
        print(f"🤖 Claude: ", end="", flush=True)

        await session.client.query(prompt)
        session.turn_count += 1

        async for message in session.client.receive_response():
            if isinstance(message, AssistantMessage):
                for block in message.content:
                    if isinstance(block, TextBlock):
                        print(block.text, end="", flush=True)
        print()

    # 断开连接
    await session.client.disconnect()

    print("\n" + "-" * 40)
    print("✅ 持久会话演示完成")
    print("💡 提示：Claude 在整个会话中保持了上下文记忆")


# ============================================================
# 主函数
# ============================================================

async def main():
    """运行所有基础对话示例"""
    print_example_header(
        "Claude Agent SDK - 基础对话示例",
        "展示从简单到复杂的各种对话功能"
    )

    # 验证配置
    config = get_config()
    if not config.anthropic_api_key:
        print("❌ 错误: 未设置 ANTHROPIC_API_KEY")
        print("请在 .env 文件中配置 API 密钥")
        return

    try:
        # 运行示例
        await basic_question_example()
        await custom_options_example()
        await conversation_example()
        await session_management_example()

        print("\n" + "=" * 50)
        print("✅ 所有基础对话示例完成!")
        print("=" * 50)
        print("\n💡 提示:")
        print("  - 示例 1-2: 使用 query() 函数，适合简单场景")
        print("  - 示例 3: 独立 query 调用，不保持上下文")
        print("  - 示例 4: ClaudeSDKClient，保持持久会话")

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()


async def interactive_mode():
    """交互式会话模式"""
    print_example_header(
        "Claude Agent SDK - 交互式会话模式",
        "与 Claude 进行持续的对话交流"
    )

    config = get_config()
    if not config.anthropic_api_key:
        print("❌ 错误: 未设置 ANTHROPIC_API_KEY")
        print("请在 .env 文件中配置 API 密钥")
        return

    options = ClaudeAgentOptions(
        allowed_tools=["Read", "Write", "Bash", "Grep", "Glob"],
        permission_mode="default"
    )

    session = ConversationSession(options)
    await session.start()


if __name__ == "__main__":
    import argparse

    parser = argparse.ArgumentParser(
        description="Claude Agent SDK - 基础对话示例"
    )
    parser.add_argument(
        "-i", "--interactive",
        action="store_true",
        help="启动交互式会话模式"
    )

    args = parser.parse_args()

    if args.interactive:
        anyio.run(interactive_mode)
    else:
        anyio.run(main)
