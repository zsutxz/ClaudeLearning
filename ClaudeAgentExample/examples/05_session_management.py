#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
示例 05: 会话管理示例

展示如何管理对话会话，包括历史记录、上下文维护等。

功能演示：
- 对话历史保存
- 上下文管理
- 会话持久化
- 会话恢复
"""

import sys
import anyio
import json
from pathlib import Path
from datetime import datetime
from typing import List, Dict

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


class SessionManager:
    """简单的会话管理器"""

    def __init__(self, session_id: str):
        self.session_id = session_id
        self.history: List[Dict[str, str]] = []
        self.created_at = datetime.now()

    def add_message(self, role: str, content: str) -> None:
        """添加消息到历史记录"""
        self.history.append({
            "role": role,
            "content": content,
            "timestamp": datetime.now().isoformat()
        })

    def get_history(self) -> List[Dict[str, str]]:
        """获取历史记录"""
        return self.history

    def get_context_summary(self) -> str:
        """获取上下文摘要"""
        if not self.history:
            return "空会话"

        user_msgs = sum(1 for m in self.history if m["role"] == "user")
        asst_msgs = sum(1 for m in self.history if m["role"] == "assistant")

        return f"会话包含 {user_msgs} 条用户消息, {asst_msgs} 条助手回复"

    def save(self, filepath: str) -> None:
        """保存会话到文件"""
        session_data = {
            "session_id": self.session_id,
            "created_at": self.created_at.isoformat(),
            "history": self.history
        }

        with open(filepath, "w", encoding="utf-8") as f:
            json.dump(session_data, f, ensure_ascii=False, indent=2)

    @classmethod
    def load(cls, filepath: str) -> "SessionManager":
        """从文件加载会话"""
        with open(filepath, "r", encoding="utf-8") as f:
            data = json.load(f)

        session = cls(data["session_id"])
        session.history = data["history"]
        session.created_at = datetime.fromisoformat(data["created_at"])

        return session


async def context_memory_example():
    """示例 1: 上下文记忆"""
    print("\n📝 示例 1: 上下文记忆")
    print("-" * 40)

    options = ClaudeAgentOptions(
        system_prompt="你是一个有记忆的助手，请记住对话中的关键信息。",
        max_turns=1,
    )

    conversation = [
        "我叫张三，是一名软件工程师。",
        "我今年28岁。",
        "我住在上海。",
        "请根据我刚才告诉你的信息，介绍一下我自己。",
    ]

    for i, prompt in enumerate(conversation, 1):
        print(f"\n👤 用户 (第 {i} 轮): {prompt}")

        message_stream = query(prompt=prompt, options=options)

        response = ""
        async for message in message_stream:
            if isinstance(message, AssistantMessage):
                for block in message.content:
                    if isinstance(block, TextBlock):
                        response += block.text

        print(f"🤖 Claude: {response}")


async def session_tracking_example():
    """示例 2: 会话跟踪"""
    print("\n📝 示例 2: 会话跟踪")
    print("-" * 40)

    session = SessionManager("demo-session-001")

    options = ClaudeAgentOptions(
        system_prompt="你是一个专业的面试官，正在进行技术面试。",
        max_turns=1,
    )

    questions = [
        "请做一下自我介绍。",
        "你熟悉哪些编程语言？",
        "介绍一下你最自豪的项目。",
    ]

    for question in questions:
        # 记录用户问题
        session.add_message("user", question)

        print(f"\n👤 面试官: {question}")

        message_stream = query(prompt=question, options=options)

        response = ""
        async for message in message_stream:
            if isinstance(message, AssistantMessage):
                for block in message.content:
                    if isinstance(block, TextBlock):
                        response += block.text

        print(f"🤖 候选人: {response}")

        # 记录助手回复
        session.add_message("assistant", response)

    print(f"\n📊 会话统计: {session.get_context_summary()}")


async def session_persistence_example():
    """示例 3: 会话持久化"""
    print("\n📝 示例 3: 会话持久化")
    print("-" * 40)

    # 创建并保存会话
    session_file = Path(__file__).parent / "session_data.json"

    if session_file.exists():
        print("📂 发现已保存的会话，正在加载...")
        session = SessionManager.load(str(session_file))
        print(f"✅ 会话已加载: {session.session_id}")
        print(f"📊 {session.get_context_summary()}")
        print(f"⏰ 创建时间: {session.created_at.strftime('%Y-%m-%d %H:%M:%S')}")
    else:
        print("🆕 创建新会话...")
        session = SessionManager("persistent-session-001")

        options = ClaudeAgentOptions(
            system_prompt="你是一个学习助手，帮助用户学习新知识。",
            max_turns=1,
        )

        prompts = [
            "什么是递归？",
            "递归有什么优缺点？",
        ]

        for prompt in prompts:
            session.add_message("user", prompt)

            message_stream = query(prompt=prompt, options=options)

            response = ""
            async for message in message_stream:
                if isinstance(message, AssistantMessage):
                    for block in message.content:
                        if isinstance(block, TextBlock):
                            response += block.text

            session.add_message("assistant", response)

        # 保存会话
        session.save(str(session_file))
        print(f"💾 会话已保存到: {session_file}")

    # 显示会话历史
    print("\n📜 会话历史:")
    for i, msg in enumerate(session.history, 1):
        role_icon = "👤" if msg["role"] == "user" else "🤖"
        print(f"{i}. {role_icon} {msg['role']}: {msg['content'][:50]}...")


async def multi_turn_conversation_example():
    """示例 4: 多轮对话管理"""
    print("\n📝 示例 4: 多轮对话管理")
    print("-" * 40)

    options = ClaudeAgentOptions(
        system_prompt="""你是一个代码导师，正在教授 Python 编程。
        在对话中要：
        1. 循序渐进地讲解概念
        2. 鼓励学生提问
        3. 根据学生水平调整讲解深度""",
        max_turns=3,  # 允许多轮对话
    )

    message_stream = query(
        prompt="我想学习 Python 的装饰器，请从基础开始讲解。",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"🤖 导师: {block.text}")
        elif isinstance(message, ResultMessage):
            print(f"\n✅ 对话结束")


async def main():
    """运行所有会话管理示例"""
    print_example_header(
        "Claude Agent SDK - 会话管理示例",
        "展示如何管理对话会话和历史记录"
    )

    # 验证配置
    config = get_config()
    if not config.anthropic_api_key:
        print("❌ 错误: 未设置 ANTHROPIC_API_KEY")
        print("请在 config/.env 文件中配置 API 密钥")
        return

    try:
        # 运行示例
        await context_memory_example()
        await session_tracking_example()
        await session_persistence_example()
        await multi_turn_conversation_example()

        print("\n" + "=" * 50)
        print("✅ 所会话管理示例完成!")
        print("=" * 50)

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    anyio.run(main)
