#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""高级Slash命令测试 - 解决执行结果为空的问题"""

import asyncio
import sys
import json
from pathlib import Path
from claude_agent_sdk import query, ClaudeAgentOptions
import os

# 设置控制台编码
if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.detach())
    sys.stderr = codecs.getwriter('utf-8')(sys.stderr.detach())

# 获取项目根目录
project_root = Path(__file__).parent.parent.resolve()

# 加载.env文件中的环境变量
try:
    from dotenv import load_dotenv
    # 尝试从config目录加载.env文件
    env_file = project_root / "config" / ".env"
    if env_file.exists():
        load_dotenv(env_file)
    else:
        load_dotenv()
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
            break

if not os.getenv('ANTHROPIC_API_KEY'):
    raise ValueError(f"请设置ANTHROPIC_API_KEY环境变量或在以下位置配置.env文件:\n{project_root / 'config' / '.env'}")

async def test_slash_in_conversation():
    """在对话上下文中测试slash命令"""
    print("=== 在对话上下文中测试Slash命令 ===\n")

    options = ClaudeAgentOptions(
        max_turns=3,  # 增加轮次以支持对话
        system_prompt="你是一个有用的助手。当用户使用slash命令时，请正确执行并显示结果。"
    )

    try:
        print("1. 建立对话上下文...")

        # 先进行正常对话建立上下文
        async for message in query(
            prompt="你好，我想测试一些slash命令功能。请告诉我你支持哪些功能？",
            options=options
        ):
            message_type = type(message).__name__

            if message_type == "AssistantMessage":
                for block in message.content:
                    if hasattr(block, 'text'):
                        print(f"助手: {block.text}")
            elif message_type == "SystemMessage":
                data = getattr(message, 'data', {})
                slash_commands = data.get('slash_commands', [])
                if slash_commands:
                    print(f"[系统] 可用slash命令: {slash_commands}")

        print("\n2. 在同一会话中执行 /help 命令...")

        # 在同一会话中执行slash命令
        async for message in query(
            prompt="/help",
            options=options
        ):
            message_type = type(message).__name__

            if message_type == "AssistantMessage":
                for block in message.content:
                    if hasattr(block, 'text'):
                        print(f"Help回复: {block.text}")
            elif message_type == "ResultMessage":
                print(f"Help结果: {message.result}")
            elif message_type == "SystemMessage":
                print(f"系统消息: {getattr(message, 'data', str(message))}")

    except Exception as e:
        print(f"对话上下文测试出错: {e}")


async def test_slash_with_context():
    """测试带参数的slash命令"""
    print("\n=== 测试带参数的Slash命令 ===\n")

    options = ClaudeAgentOptions(
        max_turns=2
    )

    # 测试 /cost 命令
    try:
        print("测试 /cost 命令...")
        async for message in query(
            prompt="/cost --detailed",  # 尝试带参数
            options=options
        ):
            await process_message(message, "cost")

    except Exception as e:
        print(f"带参数slash命令测试出错: {e}")


async def test_slash_variations():
    """测试不同的slash命令调用方式"""
    print("\n=== 测试不同的Slash命令调用方式 ===\n")

    options = ClaudeAgentOptions(
        max_turns=1
    )

    test_cases = [
        ("普通调用", "/todos"),
        ("带空格", " /todos"),
        ("完整格式", "Please run /todos"),
        ("嵌入式", "Can you show me /todos?"),
        ("明确指令", "Execute the command: /todos")
    ]

    for name, prompt in test_cases:
        print(f"\n--- {name}: '{prompt}' ---")
        try:
            async for message in query(
                prompt=prompt,
                options=options
            ):
                await process_message(message, name)
        except Exception as e:
            print(f"{name} 测试出错: {e}")


async def process_message(message, context=""):
    """处理消息的通用函数"""
    message_type = type(message).__name__

    if message_type == "AssistantMessage":
        for block in message.content:
            if hasattr(block, 'text'):
                text = block.text.strip()
                if text:
                    print(f"[{context}] 回复: {text}")
                else:
                    print(f"[{context}] 回复: (空内容)")
    elif message_type == "ResultMessage":
        result = getattr(message, 'result', None)
        if result:
            print(f"[{context}] 结果: {result}")
        else:
            print(f"[{context}] 结果: (空结果)")
    elif message_type == "SystemMessage":
        data = getattr(message, 'data', {})
        if isinstance(data, dict):
            # 检查是否有有用的信息
            useful_info = {k: v for k, v in data.items()
                          if k not in ['session_id', 'uuid', 'tools'] and v is not None}
            if useful_info:
                print(f"[{context}] 系统信息: {useful_info}")
            else:
                print(f"[{context}] 系统消息: (标准初始化消息)")
        else:
            print(f"[{context}] 系统: {str(data)[:100]}...")
    else:
        print(f"[{context}] 其他类型消息: {message_type}")


async def test_debug_mode():
    """调试模式 - 详细显示所有信息"""
    print("\n=== 调试模式 - 详细信息 ===\n")

    options = ClaudeAgentOptions(
        max_turns=1
    )

    try:
        print("执行 /todos 命令并显示所有详细信息...")

        message_count = 0
        async for message in query(
            prompt="/todos",
            options=options
        ):
            message_count += 1
            print(f"\n--- 消息 {message_count} ---")
            print(f"类型: {type(message).__name__}")
            print(f"完整对象: {message}")

            # 显示所有属性
            if hasattr(message, '__dict__'):
                print("属性:")
                for attr, value in message.__dict__.items():
                    print(f"  {attr}: {value}")

    except Exception as e:
        print(f"调试模式出错: {e}")
        import traceback
        traceback.print_exc()


async def main():
    """主测试函数"""
    try:
        print("开始高级Slash命令测试...\n")

        # 1. 在对话上下文中测试
        await test_slash_in_conversation()

        # 2. 测试带参数的命令
        await test_slash_with_context()

        # 3. 测试不同的调用方式
        await test_slash_variations()

        # 4. 调试模式
        await test_debug_mode()

        print("\n" + "="*50)
        print("测试完成!")

    except Exception as e:
        print(f"主测试函数出错: {e}")
        if "cancel scope" in str(e) or "Event loop is closed" in str(e):
            print("忽略已知的库清理问题")
        else:
            import traceback
            traceback.print_exc()


if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("\n程序被用户中断")
    except Exception as e:
        print(f"程序错误: {e}")
        if "cancel scope" in str(e) or "Event loop is closed" in str(e) or "unclosed transport" in str(e):
            print("忽略已知的库清理问题")
        else:
            raise