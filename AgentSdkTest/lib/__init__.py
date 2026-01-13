"""
Claude Agent SDK 多模型支持库

提供统一的AI代理接口，支持多种模型提供商。
"""

from .multi_agent import UniversalAIAgent, UniversalTaskAgent, UniversalCodeAgent
from .factory import (
    AgentFactory,
    create_chat_agent,
    create_code_agent,
    create_task_agent,
    create_file_agent,
    create_agent,
    create_multi_agent,
)
from .config import Config, get_config, load_env_file

__all__ = [
    # 多模型代理
    "UniversalAIAgent",
    "UniversalTaskAgent",
    "UniversalCodeAgent",
    # 代理工厂
    "AgentFactory",
    "create_chat_agent",
    "create_code_agent",
    "create_task_agent",
    "create_file_agent",
    "create_agent",
    "create_multi_agent",
    # 配置
    "Config",
    "get_config",
    "load_env_file",
]

__version__ = "2.0.0"
