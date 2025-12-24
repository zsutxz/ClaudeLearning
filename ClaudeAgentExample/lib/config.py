"""
配置管理模块

负责加载和管理项目配置，包括环境变量、API密钥、模型配置等。
"""

import os
from typing import Optional, Dict, Any
from pathlib import Path
from dataclasses import dataclass, field


@dataclass
class Config:
    """项目配置类"""

    # Claude API 配置 (智谱AI)
    anthropic_api_key: Optional[str] = None
    anthropic_base_url: str = "https://open.bigmodel.cn/api/anthropic"
    anthropic_model: str = "glm-4.7"

    # OpenAI 配置
    openai_api_key: Optional[str] = None
    openai_base_url: str = "https://api.openai.com/v1"
    openai_model: str = "gpt-4o-mini"

    # DeepSeek 配置
    deepseek_api_key: Optional[str] = None
    deepseek_base_url: str = "https://api.deepseek.com/v1"
    deepseek_model: str = "deepseek-chat"

    # 通用配置
    max_tokens: int = 4096
    temperature: float = 0.7
    max_turns: int = 5

    # 日志配置
    log_level: str = "INFO"

    # MCP 配置
    mcp_servers: Dict[str, Any] = field(default_factory=dict)

    def __post_init__(self):
        """初始化后处理，从环境变量加载配置"""
        # 加载 Claude API 配置
        if not self.anthropic_api_key:
            self.anthropic_api_key = os.getenv("ANTHROPIC_API_KEY")
        if base_url := os.getenv("ANTHROPIC_BASE_URL"):
            self.anthropic_base_url = base_url
        if model := os.getenv("ANTHROPIC_MODEL"):
            self.anthropic_model = model

        # 加载 OpenAI 配置
        if not self.openai_api_key:
            self.openai_api_key = os.getenv("OPENAI_API_KEY")
        if base_url := os.getenv("OPENAI_BASE_URL"):
            self.openai_base_url = base_url
        if model := os.getenv("OPENAI_MODEL"):
            self.openai_model = model

        # 加载 DeepSeek 配置
        if not self.deepseek_api_key:
            self.deepseek_api_key = os.getenv("DEEPSEEK_API_KEY")
        if base_url := os.getenv("DEEPSEEK_BASE_URL"):
            self.deepseek_base_url = base_url
        if model := os.getenv("DEEPSEEK_MODEL"):
            self.deepseek_model = model

    def validate(self) -> tuple[bool, list[str]]:
        """
        验证配置是否有效

        Returns:
            (is_valid, errors): 配置是否有效和错误列表
        """
        errors = []

        if not self.anthropic_api_key:
            errors.append("未设置 ANTHROPIC_API_KEY")

        return len(errors) == 0, errors

    def get_claude_config(self) -> Dict[str, Any]:
        """获取 Claude API 配置字典"""
        return {
            "api_key": self.anthropic_api_key,
            "base_url": self.anthropic_base_url,
            "model": self.anthropic_model,
            "max_tokens": self.max_tokens,
            "temperature": self.temperature,
        }


# 全局配置实例
_config: Optional[Config] = None


def get_config(reload: bool = False) -> Config:
    """
    获取全局配置实例

    Args:
        reload: 是否重新加载配置

    Returns:
        Config: 配置实例
    """
    global _config

    if _config is None or reload:
        _config = Config()

    return _config


def load_env_file(env_path: Optional[str] = None) -> None:
    """
    从 .env 文件加载环境变量

    Args:
        env_path: .env 文件路径，默认为项目根目录下的 .env
    """
    if env_path is None:
        # 查找 .env 文件
        project_root = Path(__file__).parent.parent
        env_path = project_root / "config" / ".env"

    env_file = Path(env_path)
    if not env_file.exists():
        return

    try:
        with open(env_file, "r", encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if not line or line.startswith("#"):
                    continue
                if "=" in line:
                    key, value = line.split("=", 1)
                    os.environ[key.strip()] = value.strip()
    except Exception as e:
        print(f"警告: 加载 .env 文件失败: {e}")


# 模块加载时自动加载 .env 文件
load_env_file()
