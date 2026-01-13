#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
示例 02: 多模型支持示例

展示如何使用多模型接口，支持多种AI提供商。
这是整合后的核心功能，保留 AgentSdkTest 的多模型接口。

功能演示：
- 多模型统一接口 (Claude, OpenAI, DeepSeek, Ollama, Mock)
- 不同模型的选择和使用
- 流式响应
- 模型参数调优
"""

import sys
from pathlib import Path

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent.parent.parent
sys.path.insert(0, str(project_root))

from lib.example_helpers import create_agent_selector

from lib.multi_agent import (
    UniversalAIAgent,
    UniversalTaskAgent,
    UniversalCodeAgent,
)
from lib.config import get_config
from lib.utils import print_example_header


def test_mock_model():
    """测试 Mock 模型（无需API密钥）"""
    print("\n📝 测试 1: Mock 模型（无需API密钥）")
    print("-" * 40)

    agent = UniversalAIAgent(provider="mock", model="mock-model")
    response = agent.chat("你好，请介绍一下你自己。")
    print(f"🤖 Mock 回复: {response}")


def test_claude_model():
    """测试 Claude 模型（需要API密钥）"""
    print("\n📝 测试 2: Claude 模型 (glm-4.7)")
    print("-" * 40)

    config = get_config()
    if not config.anthropic_api_key:
        print("⚠️ 未配置 ANTHROPIC_API_KEY，跳过 Claude 测试")
        return

    agent = UniversalAIAgent(provider="claude", model="glm-4.7")
    response = agent.chat("请用一句话介绍你自己。")
    print(f"🤖 Claude 回复: {response}")


def test_stream_response():
    """测试流式响应"""
    print("\n📝 测试 3: 流式响应")
    print("-" * 40)

    config = get_config()
    if not config.anthropic_api_key:
        print("⚠️ 未配置 API 密钥，使用 Mock 模型演示流式响应")
        agent = UniversalAIAgent(provider="mock", model="mock-model")
    else:
        agent = UniversalAIAgent(provider="claude", model="glm-4.7")

    response = agent.chat("请写一个简短的故事开头，关于一只猫的冒险。", stream=True)


def test_code_agent():
    """测试代码助手代理"""
    print("\n📝 测试 4: 代码助手代理")
    print("-" * 40)

    config = get_config()
    provider = create_agent_selector(config)

    agent = UniversalCodeAgent(provider=provider, language="Python")
    response = agent.write_code("实现一个计算斐波那契数列的函数")
    print(f"🤖 代码助手:\n{response}")


def test_task_agent():
    """测试任务代理"""
    print("\n📝 测试 5: 任务代理")
    print("-" * 40)

    config = get_config()
    provider = create_agent_selector(config)

    agent = UniversalTaskAgent(
        provider=provider,
        task_description="帮助用户解决编程问题"
    )
    response = agent.solve_problem("我在使用 Python 时遇到了 IndentationError，该怎么解决？")
    print(f"🤖 任务助手: {response}")


def list_supported_providers():
    """列出所有支持的提供商"""
    print("\n📝 支持的模型提供商:")
    print("-" * 40)

    providers = UniversalAIAgent.list_providers()
    for name, config in providers.items():
        print(f"\n🔹 {name.upper()}")
        print(f"   描述: {config['description']}")
        print(f"   支持的模型: {', '.join(config['models'])}")
        if config['env_key']:
            print(f"   环境变量: {config['env_key']}")


def main():
    """运行所有多模型示例"""
    print_example_header(
        "多模型支持示例",
        "展示统一的多模型接口，支持 Claude、OpenAI、DeepSeek、Ollama 等"
    )

    try:
        # 列出支持的提供商
        list_supported_providers()

        # 运行测试
        test_mock_model()
        test_claude_model()
        test_stream_response()
        test_code_agent()
        test_task_agent()

        print("\n" + "=" * 50)
        print("✅ 所有多模型示例完成!")
        print("=" * 50)

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    main()
