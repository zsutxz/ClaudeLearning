"""
配置文件
"""
import os
from typing import Dict, Any
from dotenv import load_dotenv

load_dotenv()

class Settings:
    """应用配置类"""

    # API配置
    OPENAI_API_KEY: str = os.getenv("OPENAI_API_KEY", "")
    ANTHROPIC_API_KEY: str = os.getenv("ANTHROPIC_API_KEY", "")
    SERPER_API_KEY: str = os.getenv("SERPER_API_KEY", "")
    DEEPSEEK_API_KEY: str = os.getenv("DEEPSEEK_API_KEY", "")

    # 模型配置
    DEFAULT_MODEL: str = "gpt-4o-mini"
    TEMPERATURE: float = 0.1
    MAX_TOKENS: int = 4000

    # DeepSeek 配置
    DEEPSEEK_MODEL: str = "deepseek-chat"
    DEEPSEEK_BASE_URL: str = "https://api.deepseek.com/v1"
    USE_DEEPSEEK: bool = os.getenv("USE_DEEPSEEK", "False").lower() == "true"

    # 应用配置
    DEBUG: bool = os.getenv("DEBUG", "False").lower() == "true"
    MAX_RETRIES: int = int(os.getenv("MAX_RETRIES", "3"))
    TIMEOUT: int = int(os.getenv("TIMEOUT", "30"))

    # 搜索配置
    MAX_SEARCH_RESULTS: int = 10
    SEARCH_LANGUAGES: list = ["zh", "en"]

    # 学习方案配置
    MIN_COURSE_DURATION: int = 1  # 最小课程时长(小时)
    MAX_COURSE_DURATION: int = 100  # 最大课程时长(小时)
    DEFAULT_COURSE_DURATION: int = 20  # 默认课程时长(小时)

    @classmethod
    def get_llm_config(cls) -> Dict[str, Any]:
        """获取LLM配置"""
        if cls.USE_DEEPSEEK and cls.DEEPSEEK_API_KEY:
            # 使用 DeepSeek API
            return {
                "model": cls.DEEPSEEK_MODEL,
                "temperature": cls.TEMPERATURE,
                "max_tokens": cls.MAX_TOKENS,
                "openai_api_key": cls.DEEPSEEK_API_KEY,
                "openai_api_base": cls.DEEPSEEK_BASE_URL,
            }
        else:
            # 使用 OpenAI API
            return {
                "model": cls.DEFAULT_MODEL,
                "temperature": cls.TEMPERATURE,
                "max_tokens": cls.MAX_TOKENS,
                "api_key": cls.OPENAI_API_KEY,
            }

    @classmethod
    def validate_config(cls) -> bool:
        """验证配置"""
        # 如果使用 DeepSeek
        if cls.USE_DEEPSEEK:
            if not cls.DEEPSEEK_API_KEY or cls.DEEPSEEK_API_KEY in ["your_deepseek_api_key_here", ""]:
                print("警告: DEEPSEEK_API_KEY 未设置或使用默认值")
                print("请在 .env 文件中设置有效的 DeepSeek API 密钥")
                return False
            print(f"使用 DeepSeek API: {cls.DEEPSEEK_MODEL}")
            return True
        else:
            # 使用 OpenAI
            if not cls.OPENAI_API_KEY or cls.OPENAI_API_KEY in ["your_openai_api_key_here", ""]:
                print("警告: OPENAI_API_KEY 未设置或使用默认值")
                print("请在 .env 文件中设置有效的 OpenAI API 密钥")
                return False
            print(f"使用 OpenAI API: {cls.DEFAULT_MODEL}")
            return True

settings = Settings()