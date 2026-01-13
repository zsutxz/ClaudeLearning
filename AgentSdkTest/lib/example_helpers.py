"""
示例辅助工具模块

提供示例文件通用的辅助函数，减少重复代码。
"""

import sys
from pathlib import Path


def setup_project_root():
    """
    设置项目根目录到 Python 路径

    所有示例文件都需要这一步，将其提取为公共函数。
    """
    project_root = Path(__file__).parent.parent
    sys.path.insert(0, str(project_root))
    return project_root


def run_with_fallback(synchronous_fn, asynchronous_fn, *args, **kwargs):
    """
    运行函数，支持同步和异步回退

    Args:
        synchronous_fn: 同步函数
        asynchronous_fn: 异步函数
        *args: 位置参数
        **kwargs: 关键字参数

    Returns:
        函数执行结果
    """
    try:
        import anyio
        return anyio.run(asynchronous_fn(*args, **kwargs))
    except (ImportError, RuntimeError):
        return synchronous_fn(*args, **kwargs)


def select_provider(config) -> str:
    """
    根据配置自动选择可用的提供商

    Args:
        config: 配置对象

    Returns:
        str: 提供商名称
    """
    if config.anthropic_api_key:
        return "claude"
    if config.openai_api_key:
        return "openai"
    if config.deepseek_api_key:
        return "deepseek"
    return "mock"


def print_example_summary(example_name: str, success: bool, details: str = ""):
    """
    打印示例执行摘要

    Args:
        example_name: 示例名称
        success: 是否成功
        details: 详细信息
    """
    status = "✅ 成功" if success else "❌ 失败"
    print(f"\n{example_name}: {status}")
    if details:
        print(f"详情: {details}")


def create_agent_selector(config) -> str:
    """
    根据配置创建代理选择器

    这是 select_provider 的别名，保持向后兼容。

    Args:
        config: 配置对象

    Returns:
        str: 提供商名称
    """
    return select_provider(config)
