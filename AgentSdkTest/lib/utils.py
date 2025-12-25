"""
工具函数模块

提供消息处理、成本计算、日志记录等辅助功能。
"""

import logging
import sys
import os
from typing import Optional, Any
from datetime import datetime
from pathlib import Path

# 修复 Windows 控制台编码问题
if sys.platform == "win32":
    os.environ['PYTHONIOENCODING'] = 'utf-8'
    try:
        # 尝试设置控制台代码页为 UTF-8
        import locale
        import codecs
        if sys.stdout.encoding != 'utf-8':
            sys.stdout.reconfigure(encoding='utf-8')
        if sys.stderr.encoding != 'utf-8':
            sys.stderr.reconfigure(encoding='utf-8')
    except:
        pass  # 如果设置失败，忽略错误


def setup_logging(
    level: str = "INFO",
    format_string: Optional[str] = None,
    log_file: Optional[str] = None,
) -> logging.Logger:
    """
    设置日志记录

    Args:
        level: 日志级别 (DEBUG, INFO, WARNING, ERROR, CRITICAL)
        format_string: 日志格式字符串
        log_file: 日志文件路径（可选）

    Returns:
        logging.Logger: 配置好的日志记录器
    """
    if format_string is None:
        format_string = "%(asctime)s - %(name)s - %(levelname)s - %(message)s"

    # 配置根日志记录器
    logging.basicConfig(
        level=getattr(logging, level.upper()),
        format=format_string,
        handlers=[],
    )

    logger = logging.getLogger("AgentSdkTest")

    # 控制台处理器
    console_handler = logging.StreamHandler(sys.stdout)
    console_handler.setFormatter(logging.Formatter(format_string))
    logger.addHandler(console_handler)

    # 文件处理器（可选）
    if log_file:
        file_handler = logging.FileHandler(log_file, encoding="utf-8")
        file_handler.setFormatter(logging.Formatter(format_string))
        logger.addHandler(file_handler)

    return logger


def print_message(role: str, content: str, timestamp: bool = True) -> None:
    """
    打印格式化的消息

    Args:
        role: 消息角色 (user, assistant, system)
        content: 消息内容
        timestamp: 是否显示时间戳
    """
    icons = {
        "user": "👤",
        "assistant": "🤖",
        "system": "⚙️",
        "tool": "🔧",
        "error": "❌",
        "warning": "⚠️",
        "info": "ℹ️",
        "success": "✅",
    }

    icon = icons.get(role.lower(), "💬")

    if timestamp:
        time_str = datetime.now().strftime("%H:%M:%S")
        print(f"[{time_str}] {icon} {role.capitalize()}: {content}")
    else:
        print(f"{icon} {role.capitalize()}: {content}")


def print_cost(cost_usd: float, tokens_used: Optional[int] = None) -> None:
    """
    打印成本信息

    Args:
        cost_usd: 成本（美元）
        tokens_used: 使用的 token 数量
    """
    print(f"\n{'='*50}")
    print(f"💰 成本: ${cost_usd:.6f}")
    if tokens_used:
        print(f"📊 Token 使用: {tokens_used:,}")
    print(f"{'='*50}\n")


def print_separator(title: str = "", width: int = 60) -> None:
    """
    打印分隔线

    Args:
        title: 分隔线标题
        width: 分隔线宽度
    """
    if title:
        padding = (width - len(title) - 2) // 2
        print(f"{'='*padding} {title} {'='*padding}")
    else:
        print(f"{'='*width}")


def print_example_header(example_name: str, description: str = "") -> None:
    """
    打印示例标题

    Args:
        example_name: 示例名称
        description: 示例描述
    """
    print_separator()
    print(f"📚 {example_name}")
    if description:
        print(f"   {description}")
    print_separator()


def validate_api_key(api_key: Optional[str], provider: str = "API") -> bool:
    """
    验证 API 密钥

    Args:
        api_key: API 密钥
        provider: 提供商名称

    Returns:
        bool: 密钥是否有效
    """
    if not api_key:
        print(f"   ❌ {provider} 密钥未设置")
        return False

    if len(api_key) < 10:
        print(f"   ⚠️ {provider} 密钥格式可能不正确")
        return False

    print(f"   ✅ {provider} 已配置")
    return True


def ensure_directory(path: str) -> Path:
    """
    确保目录存在，不存在则创建

    Args:
        path: 目录路径

    Returns:
        Path: 目录路径对象
    """
    dir_path = Path(path)
    dir_path.mkdir(parents=True, exist_ok=True)
    return dir_path


def format_duration(seconds: float) -> str:
    """
    格式化时间长度

    Args:
        seconds: 秒数

    Returns:
        str: 格式化的时间字符串
    """
    if seconds < 1:
        return f"{seconds*1000:.0f}ms"
    elif seconds < 60:
        return f"{seconds:.1f}s"
    else:
        minutes = int(seconds // 60)
        secs = seconds % 60
        return f"{minutes}m {secs:.0f}s"


def truncate_text(text: str, max_length: int = 100, suffix: str = "...") -> str:
    """
    截断文本

    Args:
        text: 原始文本
        max_length: 最大长度
        suffix: 截断后缀

    Returns:
        str: 截断后的文本
    """
    if len(text) <= max_length:
        return text
    return text[:max_length - len(suffix)] + suffix


class ProgressTracker:
    """进度追踪器"""

    def __init__(self, total: int, description: str = "处理中"):
        """
        初始化进度追踪器

        Args:
            total: 总任务数
            description: 任务描述
        """
        self.total = total
        self.current = 0
        self.description = description
        self.start_time = datetime.now()

    def update(self, increment: int = 1) -> None:
        """
        更新进度

        Args:
            increment: 增量
        """
        self.current += increment
        percentage = (self.current / self.total) * 100
        print(f"\r{self.description}: {self.current}/{self.total} ({percentage:.1f}%)", end="")

        if self.current >= self.total:
            elapsed = (datetime.now() - self.start_time).total_seconds()
            print(f" - 完成! 耗时: {format_duration(elapsed)}")

    def finish(self) -> None:
        """标记完成"""
        if self.current < self.total:
            self.current = self.total
            self.update(0)
