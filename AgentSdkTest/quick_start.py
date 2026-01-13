#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
快速开始脚本 - Claude Agent SDK 整合项目

提供交互式菜单，方便用户快速体验各种示例功能。
整合了 Claude Agent SDK 和 Universal Agent 的所有示例。
"""

import sys
import subprocess
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
║          🤖 Claude Agent SDK 整合项目 🤖                       ║
║                                                                ║
║              多模型支持 + 模块化架构 v2.0.0                    ║
║                                                                ║
╚══════════════════════════════════════════════════════════════╝
"""
    print(banner)


def print_menu():
    """打印主菜单"""
    menu = """
请选择要运行的示例：

═══════════════════════════════════════════════════════════════

📘 Claude Agent SDK 示例 (官方 SDK)
  1. 基础对话示例           - query() 函数和基本对话
  2. SDK 综合测试           - 完整的 SDK 功能测试
  3. MCP 集成示例           - MCP 服务器集成
  4. 工具使用示例           - 文件读写、代码搜索等
  5. Hook 功能测试          - 钩子机制和事件拦截
  6. Slash 命令测试         - 自定义斜杠命令
  7. Skill 功能测试         - 技能插件系统
  8. Todos 功能测试         - 任务管理功能

📗 Universal Agent 示例 (多模型统一接口)
  9. DeepSeek 模型测试      - DeepSeek API 测试
 10. 多模型支持示例         - Claude/OpenAI/DeepSeek/Ollama
 11. 会话管理示例           - 对话历史和上下文管理
 12. 流式响应示例           - 实时流式输出处理
 13. 高级代理示例           - 专业化代理和工厂模式
 14. 多智能体系统           - 智能体协作和任务调度

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
    claude_ok = validate_api_key(config.anthropic_api_key, "Claude API (GLM)")
    validate_api_key(config.openai_api_key, "OpenAI API")
    validate_api_key(config.deepseek_api_key, "DeepSeek API")

    if not claude_ok:
        print("\n❌ Claude API 密钥未配置！")
        print("请在 config/.env 文件中设置 ANTHROPIC_API_KEY")
        print("获取密钥: https://open.bigmodel.cn/")
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

    # 检查可选依赖 - MCP 服务器 (通过 npm 安装)
    try:
        result = subprocess.run(
            "npm list -g @modelcontextprotocol/server-filesystem",
            shell=True,
            capture_output=True,
            text=True,
            encoding='utf-8',
            errors='ignore',
            timeout=5
        )
        if result.returncode == 0:
            print("   ✅ mcp-server-filesystem (可选, npm)")
        else:
            print("   ⚠️  mcp-server-filesystem (未安装, MCP功能受限)")
    except (FileNotFoundError, subprocess.TimeoutExpired):
        print("   ⚠️  mcp-server-filesystem (npm未找到或未安装)")

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
   - 在 config/.env 文件中设置 ANTHROPIC_API_KEY
   - 获取密钥: https://open.bigmodel.cn/

2. 安装依赖
   pip install -r requirements.txt

3. 运行示例
   - 使用本脚本选择运行
   - 或直接运行: python examples/claude_agent_sdk/TestBasicChat.py

📁 项目结构：

AgentSdkTest/
├── lib/                      # 核心库模块
│   ├── multi_agent.py        # 多模型统一接口
│   ├── factory.py            # 代理工厂
│   ├── multi_agent_system.py # 多智能体协作系统
│   ├── config.py             # 配置管理
│   └── utils.py              # 工具函数
├── examples/
│   ├── claude_agent_sdk/     # Claude SDK 官方示例
│   │   ├── TestBasicChat.py      # 基础对话
│   │   ├── TestAgentSdk.py       # SDK 综合测试
│   │   ├── TestMcpIntegration.py # MCP 集成
│   │   ├── TestTool.py           # 工具使用
│   │   ├── TestHook.py           # Hook 功能
│   │   ├── TestSlash.py          # Slash 命令
│   │   ├── TestSkill.py          # Skill 功能
│   │   └── TestTodos.py          # Todos 功能
│   └── universal_agent/      # 通用多模型代理示例
│       ├── 01_test_deepseek.py       # DeepSeek 测试
│       ├── 02_multi_model.py         # 多模型支持
│       ├── 03_session_management.py  # 会话管理
│       ├── 04_stream_response.py     # 流式响应
│       ├── 05_advanced_agent.py      # 高级代理
│       └── 06_multi_agent_system.py  # 多智能体系统
├── config/                   # 配置文件目录
├── quick_start.py           # 快速开始脚本（本文件）
└── README.md                # 详细文档

💡 核心特性：

Claude Agent SDK:
- 官方 SDK 接口，query() 函数
- 持久会话管理
- Hook 和 Slash 命令支持

Universal Agent:
- 多模型统一接口：Claude、OpenAI、DeepSeek、Ollama
- 专业化代理：代码助手、任务代理
- 多智能体协作系统

🎮 使用技巧：

- 按数字键选择示例，按回车确认
- 输入 'h' 查看帮助信息
- 输入 'c' 检查环境配置
- 输入 'a' 运行所有示例
- 输入 '0' 退出程序

📚 更多信息：

- 查看完整文档: README.md
- 官方文档: https://docs.anthropic.com/claude/docs/claude-sdk

"""
    print(help_text)


def run_example(example_number: int):
    """运行指定的示例"""
    examples = {
        # Claude Agent SDK 示例 (1-8)
        1: ("examples/claude_agent_sdk/TestBasicChat.py", "基础对话示例"),
        2: ("examples/claude_agent_sdk/TestAgentSdk.py", "SDK 综合测试"),
        3: ("examples/claude_agent_sdk/TestMcpIntegration.py", "MCP 集成示例"),
        4: ("examples/claude_agent_sdk/TestTool.py", "工具使用示例"),
        5: ("examples/claude_agent_sdk/TestHook.py", "Hook 功能测试"),
        6: ("examples/claude_agent_sdk/TestSlash.py", "Slash 命令测试"),
        7: ("examples/claude_agent_sdk/TestSkill.py", "Skill 功能测试"),
        8: ("examples/claude_agent_sdk/TestTodos.py", "Todos 功能测试"),

        # Universal Agent 示例 (9-14)
        9: ("examples/universal_agent/01_test_deepseek.py", "DeepSeek 模型测试"),
        10: ("examples/universal_agent/02_multi_model.py", "多模型支持示例"),
        11: ("examples/universal_agent/03_session_management.py", "会话管理示例"),
        12: ("examples/universal_agent/04_stream_response.py", "流式响应示例"),
        13: ("examples/universal_agent/05_advanced_agent.py", "高级代理示例"),
        14: ("examples/universal_agent/06_multi_agent_system.py", "多智能体系统"),
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

    for i in range(1, 15):
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
        print("提示: 即使没有 API 密钥，也可以使用 Mock 模型运行示例")

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
        elif choice.isdigit() and 1 <= int(choice) <= 14:
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
