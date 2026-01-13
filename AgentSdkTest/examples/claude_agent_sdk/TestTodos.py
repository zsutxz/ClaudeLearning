#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""修复的Todo功能测试 - 简化版本"""

import asyncio
import sys
from pathlib import Path
from typing import List, Dict
from claude_agent_sdk import query, ClaudeAgentOptions
from claude_agent_sdk.types import AssistantMessage, ToolUseBlock
import os

# 设置控制台编码
if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.detach())
    sys.stderr = codecs.getwriter('utf-8')(sys.stderr.detach())

# 获取项目根目录
project_root = Path(__file__).parent.parent.parent.resolve()

# 加载环境变量
env_loaded = False
try:
    from dotenv import load_dotenv
    # 尝试从config目录加载.env文件
    env_file = project_root / "config" / ".env"
    if env_file.exists():
        load_dotenv(env_file)
        env_loaded = True
        print(f"✓ 已加载环境变量: {env_file}")
    else:
        # 尝试从项目根目录加载
        load_dotenv(project_root / ".env")
        env_loaded = True
        print(f"✓ 已加载环境变量: {project_root / '.env'}")
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
            env_loaded = True
            print(f"✓ 已加载环境变量: {env_file}")
            break

# 检查API密钥
api_key = os.getenv('ANTHROPIC_API_KEY')
if not api_key:
    print("❌ 错误: 请设置 ANTHROPIC_API_KEY 环境变量")
    print(f"   可以在以下位置创建 .env 文件:")
    print(f"   - {project_root / 'config' / '.env'}")
    print(f"   - {project_root / '.env'}")
    sys.exit(1)

# 显示当前配置
print(f"✓ API密钥已配置: {api_key[:10]}...{api_key[-4:]}")
model = os.getenv('ANTHROPIC_MODEL', 'glm-4.7')
print(f"✓ 使用模型: {model}")
base_url = os.getenv('ANTHROPIC_BASE_URL', 'https://open.bigmodel.cn/api/anthropic')
print(f"✓ API端点: {base_url}")

class TodoTracker:
    def __init__(self):
        self.todos: List[Dict] = []

    def display_progress(self):
        if not self.todos:
            return

        completed = len([t for t in self.todos if t["status"] == "completed"])
        in_progress = len([t for t in self.todos if t["status"] == "in_progress"])
        pending = len([t for t in self.todos if t["status"] == "pending"])
        total = len(self.todos)

        print(f"\n📊 任务统计：{completed}/{total} 已完成, {in_progress} 进行中, {pending} 待开始\n")

        for i, todo in enumerate(self.todos):
            status = todo["status"]
            if status == "completed":
                icon = "✅"
                text = todo["content"]
            elif status == "in_progress":
                icon = "🔧"
                text = todo.get("activeForm", todo["content"])
            else:  # pending
                icon = "❌"
                text = todo["content"]
            print(f"{i + 1:2d}. {icon} {text}")
        print()

    def process_message(self, message):
        """处理收到的消息"""
        if isinstance(message, AssistantMessage):
            for content_block in message.content:
                if isinstance(content_block, ToolUseBlock):
                    if content_block.name == "TodoWrite":
                        self.todos = content_block.input.get("todos", [])
                        print("🔄 待办事项状态更新：")
                        self.display_progress()

async def test_todo_functionality():
    """测试Todo功能"""
    print("\n🚀 开始测试Todo功能...")

    tracker = TodoTracker()

    try:
        # 配置代理选项
        options = ClaudeAgentOptions(
            max_turns=10,
            model=model
        )

        print("📝 提示Claude使用TodoWrite工具创建任务列表...")

        async for message in query(
            prompt="请使用TodoWrite工具创建一个学习游戏开发的完整任务列表，包含5个具体任务",
            options=options
        ):
            tracker.process_message(message)

            # 显示其他类型消息的简单信息
            if hasattr(message, 'content') and not isinstance(message, AssistantMessage):
                print(f"💬 收到消息: {type(message).__name__}")

    except Exception as e:
        print(f"❌ 错误: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    try:
        asyncio.run(test_todo_functionality())
    except KeyboardInterrupt:
        print("\n⚠️ 程序被用户中断")
    except Exception as e:
        print(f"❌ 程序错误: {e}")
        # 忽略一些已知的库清理问题
        error_msg = str(e)
        if any(keyword in error_msg for keyword in ["cancel scope", "Event loop is closed", "unclosed transport"]):
            print("ℹ️ 忽略已知的库清理问题")
        else:
            raise
