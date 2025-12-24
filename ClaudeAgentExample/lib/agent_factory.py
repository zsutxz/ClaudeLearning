"""
代理工厂模块

封装 Claude Agent SDK 的代理创建逻辑，提供多种预设代理类型。
"""

import asyncio
import anyio
from typing import Optional, List, Callable, Any
from dataclasses import field

from claude_agent_sdk import (
    ClaudeAgentOptions,
    query,
    AssistantMessage,
    ResultMessage,
    TextBlock,
)

from .config import get_config, Config


class AgentFactory:
    """
    代理工厂类

    提供多种预设的代理类型和统一的创建接口。
    """

    def __init__(self, config: Optional[Config] = None):
        """
        初始化代理工厂

        Args:
            config: 配置对象，如果为 None 则使用全局配置
        """
        self.config = config or get_config()

    def create_options(
        self,
        system_prompt: str = "",
        max_turns: int = 5,
        allowed_tools: Optional[List[str]] = None,
        model: Optional[str] = None,
    ) -> ClaudeAgentOptions:
        """
        创建 Claude 代理选项

        Args:
            system_prompt: 系统提示词
            max_turns: 最大对话轮次
            allowed_tools: 允许使用的工具列表
            model: 模型名称

        Returns:
            ClaudeAgentOptions: 代理选项对象
        """
        if model is None:
            model = self.config.anthropic_model

        return ClaudeAgentOptions(
            system_prompt=system_prompt,
            max_turns=max_turns,
            allowed_tools=allowed_tools or [],
            model=model,
        )

    async def chat_async(
        self,
        prompt: str,
        system_prompt: str = "",
        tools: Optional[List[str]] = None,
        model: Optional[str] = None,
        max_turns: int = 5,
        message_handler: Optional[Callable] = None,
    ) -> str:
        """
        异步聊天方法

        Args:
            prompt: 用户提示
            system_prompt: 系统提示词
            tools: 允许使用的工具列表
            model: 模型名称
            max_turns: 最大对话轮次
            message_handler: 消息处理函数

        Returns:
            str: 助手的回复内容
        """
        options = self.create_options(
            system_prompt=system_prompt,
            max_turns=max_turns,
            allowed_tools=tools or [],
            model=model,
        )

        message_stream = query(prompt=prompt, options=options)

        full_response = ""
        async for message in message_stream:
            if message_handler:
                await message_handler(message)
            else:
                # 默认消息处理
                if isinstance(message, AssistantMessage):
                    for block in message.content:
                        if isinstance(block, TextBlock):
                            full_response += block.text
                            print(block.text, end="", flush=True)
                elif isinstance(message, ResultMessage):
                    print()  # 换行
                    if message.total_cost_usd > 0:
                        print(f"\n💰 成本: ${message.total_cost_usd:.4f}")

        return full_response

    def chat(
        self,
        prompt: str,
        system_prompt: str = "",
        tools: Optional[List[str]] = None,
        model: Optional[str] = None,
        max_turns: int = 5,
    ) -> str:
        """
        同步聊天方法（包装异步方法）

        Args:
            prompt: 用户提示
            system_prompt: 系统提示词
            tools: 允许使用的工具列表
            model: 模型名称
            max_turns: 最大对话轮次

        Returns:
            str: 助手的回复内容
        """
        return anyio.run(self.chat_async(
            prompt=prompt,
            system_prompt=system_prompt,
            tools=tools,
            model=model,
            max_turns=max_turns,
        ))


# 预设代理类型

def create_chat_agent(config: Optional[Config] = None) -> AgentFactory:
    """
    创建聊天代理

    适合日常对话和问答场景。

    Args:
        config: 配置对象

    Returns:
        AgentFactory: 代理工厂实例
    """
    return AgentFactory(config)


def create_code_agent(config: Optional[Config] = None) -> AgentFactory:
    """
    创建代码助手代理

    专门用于代码编写、审查和调试。

    Args:
        config: 配置对象

    Returns:
        AgentFactory: 代理工厂实例
    """
    factory = AgentFactory(config)
    factory.code_system_prompt = """你是一个专业的编程助手，精通多种编程语言。

你的能力包括：
- 编写高质量的代码，遵循最佳实践
- 代码审查和优化建议
- 调试和错误修复
- 解释复杂的概念和算法
- 提供架构设计建议

请确保代码：
1. 语法正确，符合语言规范
2. 逻辑清晰，易于理解
3. 包含必要的注释
4. 遵循安全原则"""
    return factory


def create_task_agent(config: Optional[Config] = None) -> AgentFactory:
    """
    创建任务执行代理

    专注于完成特定任务，支持工具调用。

    Args:
        config: 配置对象

    Returns:
        AgentFactory: 代理工厂实例
    """
    factory = AgentFactory(config)
    factory.task_system_prompt = """你是一个专业的任务执行助手。

你的职责：
1. 准确理解用户需求
2. 使用可用工具完成任务
3. 提供清晰的结果说明
4. 在必要时请求更多信息

请遵循以下原则：
- 优先使用工具而非猜测
- 确认操作后再执行
- 提供详细的执行结果"""
    return factory


def create_file_agent(config: Optional[Config] = None) -> AgentFactory:
    """
    创建文件操作代理

    专门用于文件系统操作，支持读写、搜索等。

    Args:
        config: 配置对象

    Returns:
        AgentFactory: 代理工厂实例
    """
    factory = AgentFactory(config)
    factory.file_system_prompt = """你是一个专业的文件操作助手。

你可以使用的工具：
- Read: 读取文件内容
- Write: 写入文件
- Grep: 搜索文件内容
- Glob: 查找文件
- Bash: 执行命令

操作原则：
1. 读取文件前确认文件存在
2. 写入文件前确认目录存在
3. 危险操作前请求用户确认
4. 提供清晰的操作结果说明"""
    return factory


# 便捷函数

def create_agent(agent_type: str = "chat", config: Optional[Config] = None) -> AgentFactory:
    """
    创建指定类型的代理

    Args:
        agent_type: 代理类型 (chat, code, task, file)
        config: 配置对象

    Returns:
        AgentFactory: 代理工厂实例
    """
    agents = {
        "chat": create_chat_agent,
        "code": create_code_agent,
        "task": create_task_agent,
        "file": create_file_agent,
    }

    if agent_type not in agents:
        raise ValueError(f"不支持的代理类型: {agent_type}。支持的类型: {list(agents.keys())}")

    return agents[agent_type](config)
