#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
示例 04: MCP 集成示例

展示如何使用 Model Context Protocol (MCP) 服务器扩展 Claude 的能力。

功能演示：
- MCP 服务器配置
- 文件系统 MCP 服务器
- 自定义 MCP 服务器集成
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


async def filesystem_mcp_example():
    """示例 1: 使用文件系统 MCP 服务器"""
    print("\n📝 示例 1: 文件系统 MCP 服务器")
    print("-" * 40)

    options = ClaudeAgentOptions(
        allowed_tools=["Read", "Write", "Glob"],
        mcp_servers={
            "filesystem": {
                "command": "python",
                "args": ["-m", "mcp_server_filesystem"],
                "env": {
                    "ALLOWED_PATHS": str(Path(__file__).parent.parent)
                }
            }
        },
        system_prompt="你是一个文件系统助手，可以帮助用户操作文件。",
        max_turns=3,
    )

    message_stream = query(
        prompt="请列出当前目录的所有 Python 文件。",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")
        elif isinstance(message, ResultMessage):
            print(f"\n✅ 操作完成")


async def mcp_with_tools_example():
    """示例 2: MCP 与工具结合使用"""
    print("\n📝 示例 2: MCP 与工具结合")
    print("-" * 40)

    options = ClaudeAgentOptions(
        allowed_tools=["Read", "Write", "Grep", "Bash"],
        mcp_servers={
            "filesystem": {
                "command": "python",
                "args": ["-m", "mcp_server_filesystem"],
                "env": {
                    "ALLOWED_PATHS": str(Path(__file__).parent.parent)
                }
            }
        },
        system_prompt="你是一个智能文件管理助手，可以使用各种工具和 MCP 服务。",
        max_turns=5,
    )

    message_stream = query(
        prompt="请执行以下任务：\n"
              "1. 查找所有包含 'config' 的文件\n"
              "2. 读取找到的第一个文件\n"
              "3. 总结文件的内容",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")
        elif isinstance(message, ResultMessage):
            print(f"\n✅ 操作完成")


async def explain_mcp_example():
    """示例 3: 解释 MCP 的概念"""
    print("\n📝 示例 3: MCP 概念说明")
    print("-" * 40)

    print("""
📘 Model Context Protocol (MCP) 简介:

MCP 是一种开放标准，允许 AI 应用与外部数据源和工具进行安全、标准化的连接。

🔌 主要特点：
  • 标准化接口: 统一的协议连接各种服务
  • 安全隔离: 进程隔离的工具执行环境
  • 可扩展: 轻松添加新的 MCP 服务器
  • 类型安全: 强类型的工具定义

📦 常见 MCP 服务器：
  • filesystem: 文件系统操作
  • database: 数据库查询
  • api: REST API 调用
  • custom: 自定义业务逻辑

💡 使用场景：
  • 文件读写和管理
  • 数据库查询和操作
  • 外部 API 集成
  • 自定义工具扩展
    """)


async def main():
    """运行所有 MCP 集成示例"""
    print_example_header(
        "Claude Agent SDK - MCP 集成示例",
        "展示如何使用 MCP 协议扩展 Claude 的能力"
    )

    # 验证配置
    config = get_config()
    if not config.anthropic_api_key:
        print("❌ 错误: 未设置 ANTHROPIC_API_KEY")
        print("请在 config/.env 文件中配置 API 密钥")
        return

    try:
        # 先解释 MCP 概念
        await explain_mcp_example()

        # 检查是否安装了 mcp-server-filesystem
        try:
            import mcp_server_filesystem
            print("\n✅ 检测到 mcp-server-filesystem 已安装")
            print("\n⚠️  注意: MCP 服务器需要在运行时可用")
            print("如果遇到连接错误，请确保已安装 mcp-server-filesystem:\n")
            print("   pip install mcp-server-filesystem\n")

            # 运行示例
            # await filesystem_mcp_example()
            # await mcp_with_tools_example()

            print("💡 提示: 由于 MCP 服务器需要特定环境配置，")
            print("   实际运行示例前请确保 MCP 服务器正确配置。")

        except ImportError:
            print("\n⚠️  未安装 mcp-server-filesystem")
            print("请运行以下命令安装:")
            print("   pip install mcp-server-filesystem")

        print("\n" + "=" * 50)
        print("✅ MCP 集成示例说明完成!")
        print("=" * 50)

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    anyio.run(main)
