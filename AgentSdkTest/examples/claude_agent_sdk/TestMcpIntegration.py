#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
示例 04: MCP 集成示例

展示如何使用 Model Context Protocol (MCP) 服务器。

功能演示：
- MCP 服务器配置
- 文件系统 MCP 服务器
- MCP 工具调用
"""

import sys
import anyio
from pathlib import Path

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent.parent.parent
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


async def mcp_filesystem_example():
    """示例: MCP 文件系统服务器"""
    print("\n📝 MCP 文件系统服务器示例")
    print("-" * 40)

    options = ClaudeAgentOptions(
        allowed_tools=["Read", "Write", "Grep", "Glob"],
        mcp_servers={
            "filesystem": {
                "command": "python",
                "args": ["-m", "mcp_server_filesystem"],
                "env": {"ALLOWED_PATHS": str(project_root)}
            }
        },
        max_turns=5,
    )

    message_stream = query(
        prompt="请列出 lib/ 目录下的所有 Python 文件",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}", end="", flush=True)
        elif isinstance(message, ResultMessage):
            print()


async def main():
    """运行 MCP 集成示例"""
    print_example_header(
        "MCP 集成示例",
        "展示 Model Context Protocol 服务器的使用"
    )

    # 验证配置
    config = get_config()
    if not config.anthropic_api_key:
        print("❌ 错误: 未设置 ANTHROPIC_API_KEY")
        print("请在 .env 文件中配置 API 密钥")
        return

    try:
        await mcp_filesystem_example()

        print("\n" + "=" * 50)
        print("✅ MCP 集成示例完成!")
        print("=" * 50)

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        print("提示: 需要安装 mcp-server-filesystem: pip install mcp-server-filesystem")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    anyio.run(main)
