"""
Research Agent 使用示例
演示如何使用Research Agent进行技术调研
"""

import asyncio
import sys
import os

sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from research_agent import ResearchAgent, quick_research, ResearchConfig

async def basic_usage_example():
    """基础使用示例"""
    print("=== Research Agent 基础使用 ===\n")

    agent = ResearchAgent(
        research_domain="人工智能",
        provider="claude",
        api_key=os.getenv('ANTHROPIC_API_KEY'),
        base_url=os.getenv('ANTHROPIC_BASE_URL', 'https://open.bigmodel.cn/api/anthropic')
    )

    print(f"研究领域: {agent.research_domain}")
    print(f"AI提供商: {agent.provider}")
    print(f"模型: {agent.model}\n")

    result = await agent.conduct_research(
        query="大语言模型的最新发展趋势",
        max_sources=10,
        output_format="markdown"
    )

    print(f"查询: {result.query}")
    print(f"时间: {result.timestamp}")
    print(f"\n{result.report[:500]}...")
    print("\n=== 示例完成 ===\n")

async def quick_research_example():
    """快速研究示例"""
    print("=== 快速研究示例 ===\n")

    result = await quick_research(
        query="机器学习在医疗诊断中的应用",
        research_domain="医疗AI",
        provider="mock",
        max_sources=5,
        output_format="markdown"
    )

    print(f"查询: {result.query}")
    print(f"领域: {result.metadata.get('research_domain', 'N/A')}")
    print(f"\n{result.report[:300]}...\n")

async def advanced_usage_example():
    """高级使用示例"""
    print("=== 高级使用示例 ===\n")

    config = ResearchConfig(
        research_domain="区块链技术",
        max_sources=15,
        output_format="markdown",
        include_github=True,
        include_papers=True,
        include_blogs=False
    )

    agent = ResearchAgent(research_domain=config.research_domain, provider="mock")

    print(f"配置: 最大来源={config.max_sources}, GitHub={config.include_github}, 论文={config.include_papers}\n")

    result = await agent.conduct_research(query="DeFi智能合约安全最佳实践", max_sources=config.max_sources)

    print(f"查询: {result.query}")
    print(f"时间: {result.timestamp}")
    print(f"\n报告前10行:")
    for i, line in enumerate(result.report.split('\n')[:10]):
        print(f"{i+1:2d}: {line}")

    print("\n=== 高级示例完成 ===\n")

async def configuration_example():
    """配置示例"""
    print("=== 配置示例 ===\n")

    configs = {
        "基础配置": ResearchConfig(),
        "学术论文调研": ResearchConfig(research_domain="计算机科学", max_sources=20, include_papers=True, include_github=False, include_blogs=False),
        "技术趋势分析": ResearchConfig(research_domain="技术趋势", max_sources=15, include_github=True, include_blogs=True)
    }

    for name, config in configs.items():
        print(f"{name}:")
        print(f"  领域: {config.research_domain}, 最大: {config.max_sources}")
        print(f"  GitHub: {config.include_github}, 论文: {config.include_papers}, 博客: {config.include_blogs}\n")

async def performance_test():
    """性能测试示例"""
    print("=== 性能测试 ===\n")

    import time
    agent = ResearchAgent(provider="mock")

    queries = ["机器学习算法", "云计算架构", "数据隐私", "微服务实践", "DevOps工具"]
    results = []

    for query in queries:
        start = time.time()
        result = await agent.conduct_research(query, max_sources=3)
        elapsed = time.time() - start
        results.append(elapsed)
        print(f"{query[:20]}: {elapsed:.2f}s")

    total = sum(results)
    avg = total / len(results)

    print(f"\n总查询: {len(queries)}, 总时间: {total:.2f}s, 平均: {avg:.2f}s, 查询/秒: {len(queries)/total:.2f}\n")

async def main():
    """主函数"""
    print("Research Agent 使用示例\n")

    examples = [
        ("基础使用", basic_usage_example),
        ("快速研究", quick_research_example),
        ("高级配置", advanced_usage_example),
        ("配置演示", configuration_example),
        ("性能测试", performance_test)
    ]

    print("可用示例:")
    for i, (name, _) in enumerate(examples, 1):
        print(f"{i}. {name}")
    print("0. 运行所有示例")

    try:
        choice = input("\n选择示例 (0-5): ").strip()

        if choice == "0":
            for name, func in examples:
                print(f"\n{'='*50}\n运行: {name}\n{'='*50}")
                await func()
        elif choice.isdigit() and 1 <= int(choice) <= 5:
            name, func = examples[int(choice) - 1]
            await func()
        else:
            await basic_usage_example()

    except KeyboardInterrupt:
        print("\n用户中断")
    except Exception as e:
        print(f"错误: {e}")

if __name__ == "__main__":
    print("Research Agent 使用示例程序")
    print("基于Claude Agent SDK构建的专业技术调研工具\n")

    try:
        from research_agent import ResearchAgent
        print("[OK] ResearchAgent 导入成功")
    except ImportError as e:
        print(f"[ERROR] 导入错误: {e}")
        sys.exit(1)

    asyncio.run(main())
