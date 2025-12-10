#!/usr/bin/env python3
"""
环境配置管理模块
处理环境变量加载和配置管理
"""

import os
import sys
from pathlib import Path
from typing import Dict, Optional, Any


class EnvironmentManager:
    """环境管理器"""

    def __init__(self, env_file: str = ".env", project_root: Optional[str] = None):
        """
        初始化环境管理器

        Args:
            env_file: 环境变量文件路径
            project_root: 项目根目录路径
        """
        self.env_file = env_file
        self.project_root = project_root or str(Path(__file__).parent.parent.parent)
        self._load_environment()

    def _load_environment(self):
        """加载环境变量"""
        # 首先尝试使用python-dotenv
        try:
            from dotenv import load_dotenv
            env_path = Path(self.project_root) / self.env_file
            if env_path.exists():
                load_dotenv(dotenv_path=env_path)
                print(f"已加载环境变量文件: {env_path}")
        except ImportError:
            # 如果没有dotenv，手动加载.env文件
            self._load_env_manually()

    def _load_env_manually(self):
        """手动加载.env文件"""
        env_path = Path(self.project_root) / self.env_file
        if env_path.exists():
            print(f"手动加载环境变量文件: {env_path}")
            with open(env_path, 'r', encoding='utf-8') as f:
                for line in f:
                    line = line.strip()
                    if line and '=' in line and not line.startswith('#'):
                        key, value = line.split('=', 1)
                        # 移除引号
                        if value.startswith('"') and value.endswith('"'):
                            value = value[1:-1]
                        elif value.startswith("'") and value.endswith("'"):
                            value = value[1:-1]
                        os.environ[key] = value

    def add_src_to_path(self):
        """将src目录添加到Python路径"""
        src_path = Path(self.project_root) / "src"
        if src_path.exists():
            sys.path.append(str(src_path))
            print(f"已添加src到Python路径: {src_path}")

    def create_directories(self, directories: list):
        """
        创建必要的目录

        Args:
            directories: 目录路径列表
        """
        for directory in directories:
            dir_path = Path(directory)
            if not dir_path.exists():
                dir_path.mkdir(parents=True, exist_ok=True)
                print(f"已创建目录: {dir_path}")

    def get_env_var(self, key: str, default: Any = None) -> str:
        """
        获取环境变量

        Args:
            key: 环境变量名
            default: 默认值

        Returns:
            环境变量值
        """
        return os.getenv(key, default)

    def set_env_var(self, key: str, value: str):
        """
        设置环境变量

        Args:
            key: 环境变量名
            value: 环境变量值
        """
        os.environ[key] = value

    def get_required_env_vars(self) -> Dict[str, bool]:
        """
        检查必需的环境变量

        Returns:
            环境变量存在性字典
        """
        required_vars = [
            "ANTHROPIC_API_KEY",
            "OPENAI_API_KEY",
            "DEEPSEEK_API_KEY"
        ]

        return {var: bool(self.get_env_var(var)) for var in required_vars}

    def print_environment_info(self):
        """打印环境信息"""
        print("\n=== 环境配置信息 ===")
        print(f"项目根目录: {self.project_root}")
        print(f"环境变量文件: {self.env_file}")

        # 检查必需的环境变量
        required_vars = self.get_required_env_vars()
        print("\n必需环境变量状态:")
        for var, exists in required_vars.items():
            status = "[OK] 已设置" if exists else "[X] 未设置"
            print(f"  {var}: {status}")

        # 打印Python版本信息
        print(f"\nPython版本: {sys.version}")
        print(f"Python路径: {sys.executable}")

    @staticmethod
    def check_dependencies():
        """
        检查项目依赖

        Returns:
            依赖检查结果字典
        """
        dependencies = {
            "sentence-transformers": ("sentence_transformers", False),
            "langchain": ("langchain", False),
            "langchain-community": ("langchain_community", False),
            "chromadb": ("chromadb", False),
            "numpy": ("numpy", False),
            "python-dotenv": ("dotenv", False)
        }

        for dep_name, (import_name, _) in dependencies.items():
            try:
                __import__(import_name)
                dependencies[dep_name] = (import_name, True)
            except ImportError:
                pass

        # 返回简化的结果字典
        return {dep: installed for dep, (_, installed) in dependencies.items()}

    @staticmethod
    def print_dependency_info(dependencies: Dict[str, bool]):
        """
        打印依赖信息

        Args:
            dependencies: 依赖状态字典
        """
        print("\n=== 依赖检查 ===")
        all_installed = True
        for dep, installed in dependencies.items():
            status = "[OK] 已安装" if installed else "[X] 未安装"
            print(f"  {dep}: {status}")
            if not installed:
                all_installed = False

        if not all_installed:
            print("\n缺少依赖！请运行:")
            print("pip install sentence-transformers langchain langchain-community chromadb numpy python-dotenv")

    @staticmethod
    def get_system_info() -> Dict[str, Any]:
        """
        获取系统信息

        Returns:
            系统信息字典
        """
        import platform
        import psutil

        return {
            "platform": platform.platform(),
            "python_version": platform.python_version(),
            "cpu_count": psutil.cpu_count(),
            "memory_total": f"{psutil.virtual_memory().total / (1024**3):.1f} GB",
            "memory_available": f"{psutil.virtual_memory().available / (1024**3):.1f} GB"
        }


def setup_environment(env_file: str = ".env") -> EnvironmentManager:
    """
    设置项目环境

    Args:
        env_file: 环境变量文件路径

    Returns:
        环境管理器实例
    """
    env_manager = EnvironmentManager(env_file=env_file)

    # 添加src到Python路径
    env_manager.add_src_to_path()

    # 创建必要的目录
    directories = [
        "./data/sample_documents",
        "./test_sentence_transformers_store",
        "./logs"
    ]
    env_manager.create_directories(directories)

    # 打印环境信息
    env_manager.print_environment_info()

    # 检查依赖
    dependencies = EnvironmentManager.check_dependencies()
    EnvironmentManager.print_dependency_info(dependencies)

    return env_manager