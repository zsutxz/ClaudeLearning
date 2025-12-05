#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Research Agent 基础测试脚本
避免Unicode字符，确保在Windows GBK环境下正常运行
"""

import asyncio
import sys
import os

# 添加项目路径
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

def test_imports():
    """测试导入功能"""
    print("=" * 50)
    print("Test 1: Import Functionality")
    print("=" * 50)

    try:
        from research_agent import ResearchAgent, ResearchConfig, ResearchResult, quick_research
        print("[OK] Successfully imported ResearchAgent, ResearchConfig, ResearchResult, quick_research")
        return True
    except ImportError as e:
        print(f"[ERROR] Import failed: {e}")
        return False

def test_config_creation():
    """测试配置创建"""
    print("\n" + "=" * 50)
    print("Test 2: Configuration Creation")
    print("=" * 50)

    try:
        from research_agent import ResearchConfig

        # 测试默认配置
        config1 = ResearchConfig()
        print(f"[OK] Default config: domain={config1.research_domain}, max_sources={config1.max_sources}")

        # 测试自定义配置
        config2 = ResearchConfig(
            research_domain="Machine Learning",
            max_sources=15,
            output_format="markdown"
        )
        print(f"[OK] Custom config: domain={config2.research_domain}, max_sources={config2.max_sources}")

        return True
    except Exception as e:
        print(f"[ERROR] Config creation failed: {e}")
        return False

def test_agent_creation():
    """测试代理创建"""
    print("\n" + "=" * 50)
    print("Test 3: Agent Creation")
    print("=" * 50)

    try:
        from research_agent import ResearchAgent

        # 测试基础代理创建
        agent = ResearchAgent(
            research_domain="Artificial Intelligence",
            provider="mock",  # 使用mock模式
            model="mock-model"
        )
        print(f"[OK] Agent created: domain={agent.research_domain}, provider={agent.provider}")
        print(f"[OK] Agent type: {type(agent).__name__}")
        print(f"[OK] Task description: {agent.task_description[:100]}...")

        return True
    except Exception as e:
        print(f"[ERROR] Agent creation failed: {e}")
        return False

async def test_basic_research():
    """测试基础研究功能"""
    print("\n" + "=" * 50)
    print("Test 4: Basic Research (Async)")
    print("=" * 50)

    try:
        from research_agent import ResearchAgent

        agent = ResearchAgent(
            research_domain="Artificial Intelligence",
            provider="mock",
            model="mock-model"
        )

        print("Starting basic research...")
        result = await agent.conduct_research(
            query="Latest trends in large language models",
            max_sources=5,
            output_format="markdown"
        )

        print(f"[OK] Research completed: query='{result.query}'")
        print(f"[OK] Generated at: {result.timestamp}")
        print(f"[OK] Report length: {len(result.report)} characters")
        print(f"[OK] Report preview: {result.report[:200]}...")

        return True
    except Exception as e:
        print(f"[ERROR] Research failed: {e}")
        return False

async def test_quick_research():
    """测试快速研究功能"""
    print("\n" + "=" * 50)
    print("Test 5: Quick Research (Async)")
    print("=" * 50)

    try:
        from research_agent import quick_research

        print("Starting quick research...")
        result = await quick_research(
            query="Machine learning applications in healthcare",
            research_domain="Healthcare AI",
            provider="mock",
            max_sources=3,
            output_format="markdown"
        )

        print(f"[OK] Quick research completed: query='{result.query}'")
        print(f"[OK] Research domain: {result.metadata.get('research_domain', 'N/A')}")
        print(f"[OK] Report length: {len(result.report)} characters")

        return True
    except Exception as e:
        print(f"[ERROR] Quick research failed: {e}")
        return False

def test_report_generation():
    """测试报告生成功能"""
    print("\n" + "=" * 50)
    print("Test 6: Report Generation")
    print("=" * 50)

    try:
        from research_agent import ResearchAgent

        agent = ResearchAgent(provider="mock", model="mock-model")

        # 创建模拟研究数据
        research_data = {
            'query': 'Test Query',
            'literature': {'search_suggestions': 'Test literature suggestions'},
            'data': {'processing_suggestions': 'Test data processing suggestions'},
            'quality': {'quality_assessment': 'Test quality assessment'},
            'config': agent.config
        }

        # 测试Markdown报告
        markdown_report = agent._generate_markdown_report(research_data)
        print(f"[OK] Markdown report generated: {len(markdown_report)} characters")
        print(f"[OK] Contains title: {'#' in markdown_report}")

        # 测试文本报告
        text_report = agent._generate_text_report(research_data)
        print(f"[OK] Text report generated: {len(text_report)} characters")

        return True
    except Exception as e:
        print(f"[ERROR] Report generation failed: {e}")
        return False

async def main():
    """主测试函数"""
    print("Research Agent Core Functionality Test")
    print("Test time:", asyncio.get_event_loop().time())

    test_results = []

    # 同步测试
    test_results.append(("Import Functionality", test_imports()))
    test_results.append(("Configuration Creation", test_config_creation()))
    test_results.append(("Agent Creation", test_agent_creation()))
    test_results.append(("Report Generation", test_report_generation()))

    # 异步测试
    print("\nStarting async tests...")
    test_results.append(("Basic Research", await test_basic_research()))
    test_results.append(("Quick Research", await test_quick_research()))

    # 汇总结果
    print("\n" + "=" * 50)
    print("Test Results Summary")
    print("=" * 50)

    passed = 0
    total = len(test_results)

    for test_name, result in test_results:
        status = "PASS" if result else "FAIL"
        print(f"{test_name:.<40} {status}")
        if result:
            passed += 1

    print(f"\nTotal: {passed}/{total} tests passed")

    if passed == total:
        print("\nAll tests passed! Research Agent basic functionality is working correctly")
        return True
    else:
        print("\nSome tests failed, please check the related functionality")
        return False

if __name__ == "__main__":
    try:
        success = asyncio.run(main())
        sys.exit(0 if success else 1)
    except KeyboardInterrupt:
        print("\nTest interrupted by user")
        sys.exit(1)
    except Exception as e:
        print(f"\nTest execution error: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)