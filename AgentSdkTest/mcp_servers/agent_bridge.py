#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Agent SDK 桥接 MCP 服务器

将 UniversalAIAgent 的功能暴露为 MCP 工具，
使 Claude Code 可以直接调用 Agent SDK 的多模型能力。
"""

import os
import sys
import asyncio
from pathlib import Path
from typing import Any, List, Optional

# 添加项目路径
project_root = Path(__file__).parent.parent.parent
sys.path.insert(0, str(project_root))
sys.path.insert(0, str(project_root / "AgentSdkTest"))

# 导入 MCP 协议
from mcp.server import Server
from mcp.server.stdio import stdio_server
from mcp.types import Tool, TextContent

# 导入官方 Claude Agent SDK
try:
    from claude_agent_sdk import ClaudeSDKClient, ClaudeAgentOptions
    HAS_OFFICIAL_SDK = True
except ImportError as e:
    print(f"警告: 无法导入官方 Claude Agent SDK: {e}")
    HAS_OFFICIAL_SDK = False

# 导入项目自定义 Agent SDK
try:
    from lib.multi_agent import (
        UniversalAIAgent,
        UniversalCodeAgent,
        UniversalTaskAgent,
    )
    HAS_AGENT_SDK = True
except ImportError as e:
    print(f"警告: 无法导入 Agent SDK: {e}")
    HAS_AGENT_SDK = False

# 尝试导入配置
try:
    from lib.config import get_config
    HAS_CONFIG = True
except ImportError:
    HAS_CONFIG = False

# 尝试导入工厂
try:
    from lib.agent_factory import AgentFactory, create_multi_agent
    HAS_FACTORY = True
except ImportError:
    HAS_FACTORY = False


# ============================================================
# 全局状态管理
# ============================================================

class AgentBridgeState:
    """桥接器状态管理"""

    def __init__(self):
        self.agents = {}  # 存储活跃的代理实例
        self.conversations = {}  # 存储对话历史
        self.config = None

        # 官方 SDK Agent 存储区
        self.official_agents = {}  # 存储官方 SDK Agent 实例

        # 尝试加载配置
        if HAS_CONFIG:
            try:
                self.config = get_config()
            except Exception as e:
                print(f"警告: 加载配置失败: {e}")

    def get_official_agent(self, agent_id: str):
        """获取官方 SDK Agent 实例"""
        return self.official_agents.get(agent_id)

    def create_agent_id(self, provider: str, agent_type: str) -> str:
        """创建唯一的代理 ID"""
        import time
        timestamp = int(time.time() * 1000)
        return f"{agent_type}_{provider}_{timestamp}"

    def store_official_agent(self, agent_id: str, agent):
        """存储官方 SDK Agent 实例"""
        self.official_agents[agent_id] = agent


# 全局状态实例
state = AgentBridgeState()


# ============================================================
# 工具处理函数
# ============================================================

async def handle_list_providers() -> List[TextContent]:
    """列出支持的模型提供商"""
    if not HAS_AGENT_SDK:
        return [TextContent(type="text", text="Agent SDK 未安装")]

    try:
        providers = UniversalAIAgent.SUPPORTED_PROVIDERS
        import json
        result = []
        for name, config in providers.items():
            result.append({
                "name": name,
                "description": config["description"],
                "models": config["models"],
                "env_key": config["env_key"]
            })

        return [
            TextContent(type="text", text="支持的模型提供商:"),
            TextContent(type="text", text=json.dumps(result, indent=2, ensure_ascii=False))
        ]
    except Exception as e:
        return [TextContent(type="text", text=f"错误: {str(e)}")]


# ============================================================
# 官方 Claude Agent SDK 工具处理函数
# ============================================================

async def handle_official_sdk_chat(
    message: str,
    system_prompt: Optional[str] = None,
    agent_id: Optional[str] = None,
    max_turns: int = 1
) -> List[TextContent]:
    """使用官方 Claude Agent SDK 进行对话"""
    if not HAS_OFFICIAL_SDK:
        return [TextContent(type="text", text="官方 Claude Agent SDK 未安装")]

    try:
        # 设置环境变量（如果配置存在）
        if state.config:
            if state.config.anthropic_api_key:
                os.environ['ANTHROPIC_API_KEY'] = state.config.anthropic_api_key
            if state.config.anthropic_base_url:
                os.environ['ANTHROPIC_BASE_URL'] = state.config.anthropic_base_url

        # 创建或获取 Agent
        if agent_id:
            client = state.get_official_agent(agent_id)
            if not client:
                return [TextContent(type="text", text=f"官方 SDK Agent 不存在: {agent_id}")]
        else:
            # 创建新的 Client
            options = ClaudeAgentOptions(
                system_prompt=system_prompt or "你是一个有用的 AI 助手。",
                max_turns=max_turns
            )

            client = ClaudeSDKClient(options=options)

        # 官方 SDK 使用异步流式接口
        # 1. 先连接客户端
        await client.connect()

        # 2. 发送查询
        await client.query(message)

        # 3. 接收响应
        response_text = ""
        async for msg in client.receive_response():
            # 处理 AssistantMessage (包含响应内容)
            if type(msg).__name__ == 'AssistantMessage':
                if hasattr(msg, 'content') and len(msg.content) > 0:
                    first_block = msg.content[0]
                    if hasattr(first_block, 'text'):
                        response_text += first_block.text

            # 处理 ResultMessage (包含最终结果)
            elif type(msg).__name__ == 'ResultMessage':
                if hasattr(msg, 'result') and msg.result:
                    # 如果之前没有输出，这里使用结果
                    if not response_text:
                        response_text = msg.result
                break

        # 断开连接
        await client.disconnect()

        return [
            TextContent(type="text", text="官方 Claude Agent SDK 回复:"),
            TextContent(type="text", text=response_text)
        ]

    except Exception as e:
        import traceback
        return [TextContent(type="text", text=f"官方 SDK 调用失败: {str(e)}\n类型: {type(e).__name__}\n详情:\n{traceback.format_exc()}")]


async def handle_official_sdk_create_agent(
    system_prompt: str,
    max_turns: int = 1
) -> List[TextContent]:
    """使用官方 Claude Agent SDK 创建 Agent"""
    if not HAS_OFFICIAL_SDK:
        return [TextContent(type="text", text="官方 Claude Agent SDK 未安装")]

    try:
        # 设置环境变量（如果配置存在）
        if state.config:
            if state.config.anthropic_api_key:
                os.environ['ANTHROPIC_API_KEY'] = state.config.anthropic_api_key
            if state.config.anthropic_base_url:
                os.environ['ANTHROPIC_BASE_URL'] = state.config.anthropic_base_url

        # 创建 Client
        options = ClaudeAgentOptions(
            system_prompt=system_prompt,
            max_turns=max_turns
        )

        client = ClaudeSDKClient(options=options)

        # 生成并存储 Agent ID
        agent_id = state.create_agent_id("official", "claude")
        state.store_official_agent(agent_id, client)

        import json
        return [
            TextContent(type="text", text=f"官方 SDK Agent 创建成功: {agent_id}"),
            TextContent(type="text", text=json.dumps({
                "agent_id": agent_id,
                "system_prompt": system_prompt,
                "max_turns": max_turns
            }, indent=2, ensure_ascii=False))
        ]

    except Exception as e:
        return [TextContent(type="text", text=f"创建官方 SDK Agent 失败: {str(e)}\n类型: {type(e).__name__}")]


# ============================================================
# MCP 服务器定义
# ============================================================

# 创建 MCP Server 实例
server = Server("agent-sdk-bridge")

# 定义工具列表
TOOLS = [
    Tool(
        name="list_providers",
        description="列出所有支持的模型提供商和模型列表",
        inputSchema={
            "type": "object",
            "properties": {},
            "required": []
        }
    ),
    Tool(
        name="official_sdk_chat",
        description="使用官方 Claude Agent SDK 进行对话",
        inputSchema={
            "type": "object",
            "properties": {
                "message": {
                    "type": "string",
                    "description": "要发送的消息"
                },
                "system_prompt": {
                    "type": "string",
                    "description": "系统提示词（可选）"
                },
                "agent_id": {
                    "type": "string",
                    "description": "官方 SDK Agent ID（如果使用已有 Agent）"
                },
                "max_turns": {
                    "type": "integer",
                    "description": "最大对话轮次",
                    "default": 1
                }
            },
            "required": ["message"]
        }
    ),
    Tool(
        name="official_sdk_create_agent",
        description="使用官方 Claude Agent SDK 创建 Agent",
        inputSchema={
            "type": "object",
            "properties": {
                "system_prompt": {
                    "type": "string",
                    "description": "系统提示词"
                },
                "max_turns": {
                    "type": "integer",
                    "description": "最大对话轮次",
                    "default": 1
                }
            },
            "required": ["system_prompt"]
        }
    ),
]


@server.list_tools()
async def list_tools() -> List[Tool]:
    """列出可用工具"""
    return TOOLS


@server.call_tool()
async def call_tool(name: str, arguments: Any) -> List[TextContent]:
    """调用工具"""
    handlers = {
        "list_providers": handle_list_providers,
        "official_sdk_chat": handle_official_sdk_chat,
        "official_sdk_create_agent": handle_official_sdk_create_agent,
    }

    handler = handlers.get(name)
    if handler:
        return await handler(**arguments)
    else:
        return [TextContent(type="text", text=f"未知工具: {name}")]


# ============================================================
# 主程序
# ============================================================

if __name__ == "__main__":
    # 检查命令行参数
    if len(sys.argv) > 1 and sys.argv[1] == "--test":
        # 测试模式
        print("=" * 60)
        print("Agent SDK 桥接 MCP 服务器 - 测试模式")
        print("=" * 60)
        print()

        print("SDK 状态:")
        print(f"  官方 Claude Agent SDK: {'OK' if HAS_OFFICIAL_SDK else 'MISSING'}")
        print(f"  项目自定义 Agent SDK: {'OK' if HAS_AGENT_SDK else 'MISSING'}")
        print(f"  配置模块: {'OK' if HAS_CONFIG else 'MISSING'}")
        print(f"  工厂模块: {'OK' if HAS_FACTORY else 'MISSING'}")
        print()

        if HAS_CONFIG and state.config:
            print("配置信息:")
            print(f"  模型: {state.config.anthropic_model}")
            print(f"  Base URL: {state.config.anthropic_base_url}")
            print(f"  API Key: {'已配置' if state.config.anthropic_api_key else '未配置'}")
        print()

        print("已创建 MCP 服务器: agent-sdk-bridge")
        print(f"支持的工具 ({len(TOOLS)} 个):")
        for tool in TOOLS:
            print(f"  - {tool.name}: {tool.description}")
        print()
    else:
        # MCP 模式：启动 stdio 服务器
        async def main():
            async with stdio_server() as (read_stream, write_stream):
                await server.run(
                    read_stream,
                    write_stream,
                    server.create_initialization_options()
                )

        asyncio.run(main())
