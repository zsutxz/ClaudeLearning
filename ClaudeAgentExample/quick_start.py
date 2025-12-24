#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
快速开始脚本 - Claude Agent SDK 示例项目

提供交互式菜单，方便用户快速体验各种示例功能。
"""

import sys
import asyncio
from pathlib import Path

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent
sys.path.insert(0, str(project_root))

from lib.config import get_config
from lib.utils import print_separator, validate_api_key


def print_banner():
    """打印欢迎横幅"""
    banner = """
╔══════════════════════════════════════════════════════════════╗
║                                                                ║
║          🤖 Claude Agent SDK 示例项目 🤖                       ║
║                                                                ║
║                  快速开始向导 v1.0.0                           ║
║                                                                ║
╚══════════════════════════════════════════════════════════════╝
"""
    print(banner)


def print_menu():
    """打印主菜单"""
    menu = """
请选择要运行的示例：

═══════════════════════════════════════════════════════════════

📚 基础示例
  1. 基础对话示例           - 最简单的入门示例
  2. 多模型支持示例         - 不同模型和参数的使用
  3. 工具使用示例           - 文件读写、代码搜索等
  4. MCP 集成示例           - MCP 服务器集成

🔧 高级示例
  5. 会话管理示例           - 对话历史和上下文管理
  6. 流式响应示例           - 实时流式输出处理
  7. 高级代理示例           - 综合运用所有功能

═══════════════════════════════════════════════════════════════

🎯 其他选项
  0. 退出程序
  a. 运行所有示例
  c. 检查环境配置
  h. 查看帮助信息

═══════════════════════════════════════════════════════════════
"""
    print(menu)


def check_environment():
    """检查环境配置"""
    print("\n🔍 环境配置检查")
    print("=" * 50)

    config = get_config()

    # 检查 API 密钥
    print("\n📋 API 密钥状态:")
    claude_ok = validate_api_key(config.anthropic_api_key, "Claude API")
    validate_api_key(config.openai_api_key, "OpenAI API")
    validate_api_key(config.deepseek_api_key, "DeepSeek API")

    if not claude_ok:
        print("\n❌ Claude API 密钥未配置！")
        print("请在 config/.env 文件中设置 ANTHROPIC_API_KEY")
        return False

    # 检查依赖
    print("\n📦 依赖包检查:")
    try:
        import claude_agent_sdk
        print("   ✅ claude-agent-sdk")
    except ImportError:
        print("   ❌ claude-agent-sdk (未安装)")
        print("\n请运行: pip install -r requirements.txt")
        return False

    try:
        import anyio
        print("   ✅ anyio")
    except ImportError:
        print("   ❌ anyio (未安装)")
        return False

    # 检查可选依赖
    try:
        import mcp_server_filesystem
        print("   ✅ mcp-server-filesystem (可选)")
    except ImportError:
        print("   ⚠️  mcp-server-filesystem (未安装，MCP功能受限)")

    print("\n✅ 环境检查完成！")
    return True


def print_help():
    """打印帮助信息"""
    help_text = """
╔══════════════════════════════════════════════════════════════╗
║                       📖 帮助信息                             ║
╚══════════════════════════════════════════════════════════════╝

🚀 快速开始步骤：

1. 配置 API 密钥
   - 复制 config/.env.example 为 config/.env
   - 编辑 .env 文件，填入你的 API 密钥
   - 获取密钥: https://open.bigmodel.cn/

2. 安装依赖
   pip install -r requirements.txt

3. 运行示例
   - 使用本脚本选择运行
   - 或直接运行: python examples/01_basic_chat.py

📁 项目结构：

ClaudeAgentExample/
├── examples/           # 示例代码目录
├── lib/               # 核心库模块
├── config/            # 配置文件目录
├── quick_start.py     # 快速开始脚本（本文件）
└── README.md          # 详细文档

💡 使用技巧：

- 按数字键选择示例，按回车确认
- 输入 'h' 查看帮助信息
- 输入 'c' 检查环境配置
- 输入 'a' 运行所有示例
- 输入 '0' 退出程序

📚 更多信息：

- 查看完整文档: README.md
- 官方文档: https://docs.anthropic.com/claude/docs/claude-sdk
- 问题反馈: https://github.com/anthropics/claude-sdk/issues

"""
    print(help_text)


def run_example(example_number: int):
    """运行指定的示例"""
    examples = {
        1: ("examples/01_basic_chat.py", "基础对话示例"),
        2: ("examples/02_multi_model.py", "多模型支持示例"),
        3: ("examples/03_tools_usage.py", "工具使用示例"),
        4: ("examples/04_mcp_integration.py", "MCP 集成示例"),
        5: ("examples/05_session_management.py", "会话管理示例"),
        6: ("examples/06_stream_response.py", "流式响应示例"),
        7: ("examples/07_advanced_agent.py", "高级代理示例"),
    }

    if example_number not in examples:
        print(f"\n❌ 无效的示例编号: {example_number}")
        return

    script_path, script_name = examples[example_number]
    full_path = project_root / script_path

    if not full_path.exists():
        print(f"\n❌ 示例文件不存在: {script_path}")
        return

    print(f"\n🚀 正在运行: {script_name}")
    print("=" * 60)

    # 使用 subprocess 运行示例
    import subprocess
    try:
        result = subprocess.run(
            [sys.executable, str(full_path)],
            cwd=str(project_root),
            check=True,
            encoding='utf-8',
            errors='replace',
        )
        print(f"\n✅ {script_name} 运行完成")
    except subprocess.CalledProcessError as e:
        print(f"\n❌ 运行出错: {e}")
    except KeyboardInterrupt:
        print(f"\n⚠️  用户中断")


def run_all_examples():
    """运行所有示例"""
    print("\n🚀 运行所有示例")
    print("=" * 60)
    print("⚠️  注意: 这将需要较长时间和 API 调用")
    response = input("\n是否继续？(y/n): ").strip().lower()

    if response != 'y':
        print("已取消")
        return

    for i in range(1, 8):
        run_example(i)
        print("\n" + "=" * 60)
        input("按回车继续下一个示例...")

    print("\n✅ 所有示例运行完成！")


def main():
    """主函数"""
    print_banner()

    # 首次运行时检查环境
    if not check_environment():
        print("\n⚠️  环境配置有问题，请先解决上述问题")
        return

    while True:
        print_menu()
        choice = input("请输入选项: ").strip().lower()

        if choice == '0':
            print("\n👋 再见！")
            break
        elif choice == 'a':
            run_all_examples()
        elif choice == 'c':
            check_environment()
        elif choice == 'h':
            print_help()
        elif choice.isdigit() and 1 <= int(choice) <= 7:
            run_example(int(choice))
        else:
            print(f"\n❌ 无效的选项: {choice}")

        input("\n按回车继续...")


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n👋 程序已中断")
    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()
