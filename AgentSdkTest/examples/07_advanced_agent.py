#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
示例 07: 高级代理示例

展示专业化代理的高级用法。

功能演示：
- 代码助手代理
- 任务执行代理
- 自定义代理创建
- 多模型切换
"""

import sys
from pathlib import Path

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))

from lib.multi_agent import UniversalAIAgent, UniversalCodeAgent, UniversalTaskAgent
from lib.agent_factory import AgentFactory, create_multi_agent
from lib.config import get_config
from lib.utils import print_example_header


def code_agent_example():
    """示例 1: 代码助手代理"""
    print("\n📝 示例 1: 代码助手代理")
    print("-" * 40)

    config = get_config()
    provider = "claude" if config.anthropic_api_key else "mock"

    agent = UniversalCodeAgent(provider=provider, language="Python")

    # 代码生成
    print("\n🔧 代码生成:")
    response = agent.write_code("实现一个快速排序算法")
    print(f"{response}")

    # 代码审查
    print("\n🔍 代码审查:")
    code = """
def quicksort(arr):
    if len(arr) <= 1:
        return arr
    pivot = arr[0]
    left = [x for x in arr[1:] if x < pivot]
    right = [x for x in arr[1:] if x >= pivot]
    return quicksort(left) + [pivot] + quicksort(right)
"""
    response = agent.review_code(code)
    print(f"{response}")


def task_agent_example():
    """示例 2: 任务执行代理"""
    print("\n📝 示例 2: 任务执行代理")
    print("-" * 40)

    config = get_config()
    provider = "claude" if config.anthropic_api_key else "mock"

    agent = UniversalTaskAgent(
        provider=provider,
        task_description="帮助用户解决编程问题，提供清晰的解决方案和代码示例"
    )

    problem = "我在使用 Python 列表时遇到了问题：如何高效地去重并保持顺序？"
    print(f"\n❓ 问题: {problem}")
    response = agent.solve_problem(problem)
    print(f"🤖 解决方案: {response}")


def factory_pattern_example():
    """示例 3: 代理工厂模式"""
    print("\n📝 示例 3: 代理工厂模式")
    print("-" * 40)

    config = get_config()

    # 使用工厂创建代理
    factory = AgentFactory(config)

    # 创建不同类型的代理
    providers = ["claude" if config.anthropic_api_key else "mock"]

    for provider in providers:
        print(f"\n🔹 使用 {provider.upper()} 创建代理:")

        # 通用代理
        chat_agent = factory.create_multi_model_agent(provider=provider)
        print(f"   ✅ 通用代理: {type(chat_agent).__name__}")

        # 代码代理
        code_agent = factory.create_code_agent_multi(provider=provider, language="Python")
        print(f"   ✅ 代码代理: {type(code_agent).__name__}")

        # 任务代理
        task_agent = factory.create_task_agent_multi(
            provider=provider,
            task_description="测试任务代理"
        )
        print(f"   ✅ 任务代理: {type(task_agent).__name__}")


def multi_provider_example():
    """示例 4: 多提供商切换"""
    print("\n📝 示例 4: 多提供商切换")
    print("-" * 40)

    config = get_config()

    # 测试不同的提供商
    providers_to_test = ["mock"]  # 默认使用 mock

    if config.anthropic_api_key:
        providers_to_test.insert(0, "claude")
    if config.openai_api_key:
        providers_to_test.append("openai")
    if config.deepseek_api_key:
        providers_to_test.append("deepseek")

    question = "请用一句话介绍你自己。"

    for provider in providers_to_test:
        print(f"\n🔹 {provider.upper()} 模型:")
        try:
            agent = create_multi_agent(agent_type="chat", provider=provider)
            response = agent.chat(question)
            print(f"🤖 {response[:100]}...")
        except Exception as e:
            print(f"⚠️ {provider} 不可用: {e}")


def main():
    """运行所有高级代理示例"""
    print_example_header(
        "高级代理示例",
        "展示专业化代理和高级功能的用法"
    )

    try:
        code_agent_example()
        task_agent_example()
        factory_pattern_example()
        multi_provider_example()

        print("\n" + "=" * 50)
        print("✅ 所高级代理示例完成!")
        print("=" * 50)

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    main()
