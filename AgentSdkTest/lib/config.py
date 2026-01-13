"""
配置管理模块

负责加载和管理项目配置，包括环境变量、API密钥、模型配置等。
支持多种AI提供商的配置管理。
"""

import os
from typing import Optional, Dict, Any
from pathlib import Path
from dataclasses import dataclass, field


@dataclass
class Config:
    """项目配置类 - 支持多模型配置"""

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

    # Ollama 配置
    ollama_base_url: str = "http://localhost:11434"
    ollama_model: str = "llama2"

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
        providers = [
            ("anthropic", "ANTHROPIC", True),
            ("openai", "OPENAI", True),
            ("deepseek", "DEEPSEEK", True),
            ("ollama", "OLLAMA", False),
        ]

        for attr_name, env_prefix, has_api_key in providers:
            # API密钥
            if has_api_key:
                api_key = getattr(self, f"{attr_name}_api_key")
                if not api_key:
                    setattr(self, f"{attr_name}_api_key", os.getenv(f"{env_prefix}_API_KEY"))

            # Base URL
            if base_url := os.getenv(f"{env_prefix}_BASE_URL"):
                setattr(self, f"{attr_name}_base_url", base_url)

            # Model
            if model := os.getenv(f"{env_prefix}_MODEL"):
                setattr(self, f"{attr_name}_model", model)

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

    def get_provider_config(self, provider: str) -> Dict[str, Any]:
        """
        获取指定提供商的配置

        Args:
            provider: 提供商名称 (claude, openai, deepseek, ollama)

        Returns:
            配置字典
        """
        configs = {
            "claude": {
                "api_key": self.anthropic_api_key,
                "base_url": self.anthropic_base_url,
                "model": self.anthropic_model,
            },
            "openai": {
                "api_key": self.openai_api_key,
                "base_url": self.openai_base_url,
                "model": self.openai_model,
            },
            "deepseek": {
                "api_key": self.deepseek_api_key,
                "base_url": self.deepseek_base_url,
                "model": self.deepseek_model,
            },
            "ollama": {
                "base_url": self.ollama_base_url,
                "model": self.ollama_model,
            },
        }

        return configs.get(provider, {})


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
        env_path: .env 文件路径，默认按优先级查找 config/.env -> .env
    """
    if env_path is None:
        project_root = Path(__file__).parent.parent
        for path in [project_root / "config" / ".env", project_root / ".env"]:
            if path.exists():
                env_path = str(path)
                break

    if env_path is None:
        return

    try:
        with open(env_path, "r", encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith("#") and "=" in line:
                    key, value = line.split("=", 1)
                    os.environ[key.strip()] = value.strip()
    except Exception as e:
        print(f"警告: 加载 .env 文件失败: {e}")


# 模块加载时自动加载 .env 文件
try:
    import dotenv
    project_root = Path(__file__).parent.parent
    dotenv.load_dotenv(project_root / "config" / ".env")
    dotenv.load_dotenv()
except ImportError:
    load_env_file()
