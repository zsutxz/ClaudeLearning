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

# 导入 Agent SDK
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

        # 尝试加载配置
        if HAS_CONFIG:
            try:
                self.config = get_config()
            except Exception as e:
                print(f"警告: 加载配置失败: {e}")

    def get_agent(self, agent_id: str) -> Optional[UniversalAIAgent]:
        """获取代理实例"""
        return self.agents.get(agent_id)

    def create_agent_id(self, provider: str, agent_type: str) -> str:
        """创建唯一的代理 ID"""
        import time
        timestamp = int(time.time() * 1000)
        return f"{agent_type}_{provider}_{timestamp}"

    def store_agent(self, agent_id: str, agent: UniversalAIAgent):
        """存储代理实例"""
        self.agents[agent_id] = agent

    def get_conversation(self, agent_id: str) -> List[dict]:
        """获取对话历史"""
        return self.conversations.get(agent_id, [])

    def add_to_conversation(self, agent_id: str, role: str, content: str):
        """添加到对话历史"""
        if agent_id not in self.conversations:
            self.conversations[agent_id] = []
        self.conversations[agent_id].append({
            "role": role,
            "content": content
        })


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


async def handle_create_agent(
    provider: str,
    agent_type: str,
    model: Optional[str] = None
) -> List[TextContent]:
    """创建新的代理实例"""
    if not HAS_AGENT_SDK:
        return [TextContent(type="text", text="Agent SDK 未安装")]

    try:
        # 根据 agent_type 创建代理
        if agent_type == "code":
            agent = UniversalCodeAgent(provider=provider, model=model)
        elif agent_type == "task":
            agent = UniversalTaskAgent(provider=provider, model=model)
        else:
            agent = UniversalAIAgent(provider=provider, model=model)

        # 生成并存储代理 ID
        agent_id = state.create_agent_id(provider, agent_type)
        state.store_agent(agent_id, agent)

        import json
        return [
            TextContent(type="text", text=f"代理创建成功: {agent_id}"),
            TextContent(type="text", text=json.dumps({
                "agent_id": agent_id,
                "provider": provider,
                "type": agent_type,
                "model": agent.model
            }, indent=2, ensure_ascii=False))
        ]
    except Exception as e:
        return [TextContent(type="text", text=f"创建代理失败: {str(e)}")]


async def handle_chat(
    message: str,
    provider: str = "claude",
    agent_id: Optional[str] = None,
    model: Optional[str] = None
) -> List[TextContent]:
    """发送消息给代理"""
    if not HAS_AGENT_SDK:
        return [TextContent(type="text", text="Agent SDK 未安装")]

    try:
        # 获取或创建代理
        if agent_id:
            agent = state.get_agent(agent_id)
            if not agent:
                return [TextContent(type="text", text=f"代理不存在: {agent_id}")]
        else:
            # 创建临时代理
            agent = UniversalAIAgent(provider=provider, model=model)

        # 发送消息
        response = agent.chat(message)

        # 保存对话历史
        if agent_id:
            state.add_to_conversation(agent_id, "user", message)
            state.add_to_conversation(agent_id, "assistant", response)

        return [
            TextContent(type="text", text=f"回复 ({agent.provider}/{agent.model}):"),
            TextContent(type="text", text=response)
        ]
    except Exception as e:
        return [TextContent(type="text", text=f"对话失败: {str(e)}")]


async def handle_code_assistant(
    code: str,
    language: str = "Python",
    task: str = "explain",
    provider: str = "claude"
) -> List[TextContent]:
    """代码助手工具"""
    if not HAS_AGENT_SDK:
        return [TextContent(type="text", text="Agent SDK 未安装")]

    try:
        agent = UniversalCodeAgent(provider=provider)

        # 根据任务类型构建提示
        prompts = {
            "explain": f"请解释以下 {language} 代码:\n\n{code}",
            "review": f"请审查以下 {language} 代码，提供改进建议:\n\n{code}",
            "debug": f"请检查以下 {language} 代码中的潜在错误:\n\n{code}",
            "optimize": f"请优化以下 {language} 代码的性能:\n\n{code}",
        }

        prompt = prompts.get(task, prompts["explain"])
        response = agent.chat(prompt)

        return [
            TextContent(type="text", text=f"代码助手 - {task} ({language}):"),
            TextContent(type="text", text=response)
        ]
    except Exception as e:
        return [TextContent(type="text", text=f"代码助手失败: {str(e)}")]


async def handle_task_agent(
    task: str,
    provider: str = "claude"
) -> List[TextContent]:
    """任务执行代理"""
    if not HAS_AGENT_SDK:
        return [TextContent(type="text", text="Agent SDK 未安装")]

    try:
        agent = UniversalTaskAgent(provider=provider)
        response = agent.chat(task)

        return [
            TextContent(type="text", text=f"任务执行完成:"),
            TextContent(type="text", text=response)
        ]
    except Exception as e:
        return [TextContent(type="text", text=f"任务执行失败: {str(e)}")]


async def handle_list_agents() -> List[TextContent]:
    """列出所有活跃的代理"""
    import json

    agents_info = []
    for agent_id, agent in state.agents.items():
        agents_info.append({
            "agent_id": agent_id,
            "provider": agent.provider,
            "model": agent.model,
            "conversation_length": len(state.get_conversation(agent_id))
        })

    return [
        TextContent(type="text", text=f"活跃代理数量: {len(state.agents)}"),
        TextContent(type="text", text=json.dumps(agents_info, indent=2, ensure_ascii=False))
    ]


async def handle_get_conversation(agent_id: str) -> List[TextContent]:
    """获取代理的对话历史"""
    conversation = state.get_conversation(agent_id)

    if not conversation:
        return [TextContent(type="text", text=f"代理 {agent_id} 没有对话历史")]

    import json
    return [
        TextContent(type="text", text=f"对话历史 ({len(conversation)} 条):"),
        TextContent(type="text", text=json.dumps(conversation, indent=2, ensure_ascii=False))
    ]


async def handle_delete_agent(agent_id: str) -> List[TextContent]:
    """删除代理实例"""
    if agent_id in state.agents:
        del state.agents[agent_id]
        if agent_id in state.conversations:
            del state.conversations[agent_id]
        return [TextContent(type="text", text=f"代理已删除: {agent_id}")]
    else:
        return [TextContent(type="text", text=f"代理不存在: {agent_id}")]


async def handle_multi_model_compare(
    message: str,
    providers: Optional[List[str]] = None
) -> List[TextContent]:
    """多模型对比"""
    if not HAS_AGENT_SDK:
        return [TextContent(type="text", text="Agent SDK 未安装")]

    if providers is None:
        providers = ["claude", "mock"]  # 默认对比

    results = []
    for provider in providers:
        try:
            agent = UniversalAIAgent(provider=provider)
            response = agent.chat(message)
            results.append({
                "provider": provider,
                "model": agent.model,
                "response": response[:200] + "..." if len(response) > 200 else response
            })
        except Exception as e:
            results.append({
                "provider": provider,
                "error": str(e)
            })

    import json
    return [
        TextContent(type="text", text=f"多模型对比结果:"),
        TextContent(type="text", text=json.dumps(results, indent=2, ensure_ascii=False))
    ]


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
        name="create_agent",
        description="创建新的代理实例",
        inputSchema={
            "type": "object",
            "properties": {
                "provider": {
                    "type": "string",
                    "description": "模型提供商 (claude, openai, deepseek, ollama, mock)",
                    "default": "claude"
                },
                "agent_type": {
                    "type": "string",
                    "description": "代理类型 (chat, code, task)",
                    "default": "chat"
                },
                "model": {
                    "type": "string",
                    "description": "模型名称（可选）"
                }
            },
            "required": []
        }
    ),
    Tool(
        name="chat",
        description="发送消息给代理并获取回复",
        inputSchema={
            "type": "object",
            "properties": {
                "message": {
                    "type": "string",
                    "description": "要发送的消息"
                },
                "provider": {
                    "type": "string",
                    "description": "模型提供商",
                    "default": "claude"
                },
                "agent_id": {
                    "type": "string",
                    "description": "代理ID（如果使用已有代理）"
                },
                "model": {
                    "type": "string",
                    "description": "模型名称（可选）"
                }
            },
            "required": ["message"]
        }
    ),
    Tool(
        name="code_assistant",
        description="代码助手 - 解释、审查、调试、优化代码",
        inputSchema={
            "type": "object",
            "properties": {
                "code": {
                    "type": "string",
                    "description": "代码内容"
                },
                "language": {
                    "type": "string",
                    "description": "编程语言",
                    "default": "Python"
                },
                "task": {
                    "type": "string",
                    "description": "任务类型 (explain, review, debug, optimize)",
                    "default": "explain",
                    "enum": ["explain", "review", "debug", "optimize"]
                },
                "provider": {
                    "type": "string",
                    "description": "模型提供商",
                    "default": "claude"
                }
            },
            "required": ["code"]
        }
    ),
    Tool(
        name="task_agent",
        description="任务执行代理 - 专注于完成特定任务",
        inputSchema={
            "type": "object",
            "properties": {
                "task": {
                    "type": "string",
                    "description": "任务描述"
                },
                "provider": {
                    "type": "string",
                    "description": "模型提供商",
                    "default": "claude"
                }
            },
            "required": ["task"]
        }
    ),
    Tool(
        name="list_agents",
        description="列出所有活跃的代理实例",
        inputSchema={
            "type": "object",
            "properties": {},
            "required": []
        }
    ),
    Tool(
        name="get_conversation",
        description="获取指定代理的对话历史",
        inputSchema={
            "type": "object",
            "properties": {
                "agent_id": {
                    "type": "string",
                    "description": "代理ID"
                }
            },
            "required": ["agent_id"]
        }
    ),
    Tool(
        name="delete_agent",
        description="删除指定的代理实例",
        inputSchema={
            "type": "object",
            "properties": {
                "agent_id": {
                    "type": "string",
                    "description": "代理ID"
                }
            },
            "required": ["agent_id"]
        }
    ),
    Tool(
        name="multi_model_compare",
        description="使用多个模型对比同一问题的回答",
        inputSchema={
            "type": "object",
            "properties": {
                "message": {
                    "type": "string",
                    "description": "要发送的消息"
                },
                "providers": {
                    "type": "array",
                    "items": {"type": "string"},
                    "description": "要对比的提供商列表",
                    "default": ["claude", "mock"]
                }
            },
            "required": ["message"]
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
        "create_agent": handle_create_agent,
        "chat": handle_chat,
        "code_assistant": handle_code_assistant,
        "task_agent": handle_task_agent,
        "list_agents": handle_list_agents,
        "get_conversation": handle_get_conversation,
        "delete_agent": handle_delete_agent,
        "multi_model_compare": handle_multi_model_compare,
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

        print(f"Agent SDK: {'OK' if HAS_AGENT_SDK else 'MISSING'}")
        print(f"配置模块: {'OK' if HAS_CONFIG else 'MISSING'}")
        print(f"工厂模块: {'OK' if HAS_FACTORY else 'MISSING'}")
        print()

        if HAS_CONFIG and state.config:
            print("配置信息:")
            print(f"  模型: {state.config.anthropic_model}")
            print(f"  Base URL: {state.config.anthropic_base_url}")
            print(f"  API Key: {'已配置' if state.config.anthropic_api_key else '未配置'}")
        print()

        print("已创建 MCP 服务器: agent-sdk-bridge")
        print("支持的工具:")
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
