"""
Research Agent 使用示例

演示如何使用Research Agent进行技术调研
"""

import asyncio
import sys
import os

# 添加项目路径
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from research_agent import ResearchAgent, quick_research, ResearchConfig

async def basic_usage_example():
    """基础使用示例"""
    print("=== Research Agent 基础使用示例 ===\n")

    # 创建Research Agent
    agent = ResearchAgent(
        research_domain="人工智能",
        provider="mock",  # 使用模拟模式进行演示
        model="mock-model"
    )

    print(f"研究领域: {agent.research_domain}")
    print(f"AI提供商: {agent.provider}")
    print(f"模型: {agent.model}\n")

    # 执行简单调研
    print("开始执行技术调研...")
    result = await agent.conduct_research(
        query="大语言模型的最新发展趋势",
        max_sources=10,
        output_format="markdown"
    )

    print(f"\n调研查询: {result.query}")
    print(f"生成时间: {result.timestamp}")
    print(f"来源数量: {result.metadata.get('config', {}).get('max_sources', 'N/A')}")

    print("\n=== 生成的报告 ===")
    print(result.report)

    print("\n=== 示例完成 ===\n")

async def quick_research_example():
    """快速研究示例"""
    print("=== 快速研究示例 ===\n")

    # 使用便捷函数
    result = await quick_research(
        query="机器学习在医疗诊断中的应用",
        research_domain="医疗AI",
        provider="mock",
        max_sources=5,
        output_format="markdown"
    )

    print(f"快速查询: {result.query}")
    print(f"研究领域: {result.metadata.get('research_domain', 'N/A')}")

    print("\n=== 快速研究报告 ===")
    print(result.report[:500] + "..." if len(result.report) > 500 else result.report)

    print("\n=== 快速研究完成 ===\n")

async def advanced_usage_example():
    """高级使用示例"""
    print("=== 高级使用示例 ===\n")

    # 创建自定义配置
    config = ResearchConfig(
        research_domain="区块链技术",
        max_sources=15,
        output_format="markdown",
        include_github=True,
        include_papers=True,
        include_blogs=False,
        cache_results=True
    )

    # 创建Research Agent
    agent = ResearchAgent(
        research_domain=config.research_domain,
        provider="mock",
        model="mock-model"
    )

    print("使用自定义配置:")
    print(f"- 最大来源数: {config.max_sources}")
    print(f"- 输出格式: {config.output_format}")
    print(f"- 包含GitHub: {config.include_github}")
    print(f"- 包含论文: {config.include_papers}")
    print(f"- 包含博客: {config.include_blogs}")
    print(f"- 缓存结果: {config.cache_results}\n")

    # 执行调研
    result = await agent.conduct_research(
        query="DeFi智能合约安全最佳实践",
        max_sources=config.max_sources,
        output_format=config.output_format
    )

    print("=== 调研结果摘要 ===")
    print(f"查询: {result.query}")
    print(f"时间: {result.timestamp}")
    print(f"配置: {result.metadata.get('config', {})}")

    # 显示部分报告
    report_lines = result.report.split('\n')
    print("\n=== 报告前10行 ===")
    for i, line in enumerate(report_lines[:10]):
        print(f"{i+1:2d}: {line}")

    print("\n=== 高级示例完成 ===\n")

def configuration_example():
    """配置示例"""
    print("=== 配置示例 ===\n")

    # 创建不同类型的配置
    configs = {
        "基础配置": ResearchConfig(),
        "学术论文调研": ResearchConfig(
            research_domain="计算机科学",
            max_sources=20,
            include_papers=True,
            include_github=False,
            include_blogs=False
        ),
        "技术趋势分析": ResearchConfig(
            research_domain="技术趋势",
            max_sources=15,
            include_github=True,
            include_blogs=True,
            output_format="markdown"
        )
    }

    for name, config in configs.items():
        print(f"{name}:")
        print(f"  研究领域: {config.research_domain}")
        print(f"  最大来源: {config.max_sources}")
        print(f"  GitHub: {config.include_github}")
        print(f"  论文: {config.include_papers}")
        print(f"  博客: {config.include_blogs}")
        print(f"  格式: {config.output_format}")
        print()

async def interactive_example():
    """交互式示例"""
    print("=== 交互式研究示例 ===")
    print("输入研究查询，输入 'quit' 退出\n")

    agent = ResearchAgent(
        research_domain="通用技术研究",
        provider="mock",
        model="mock-model"
    )

    while True:
        try:
            query = input("请输入研究查询: ").strip()

            if query.lower() in ['quit', 'exit', 'q']:
                print("退出交互模式")
                break

            if not query:
                print("查询不能为空，请重新输入")
                continue

            print(f"\n正在调研: {query}")
            result = await agent.conduct_research(query, max_sources=5)

            print("\n=== 调研报告 ===")
            print(result.report)

            print("\n" + "="*50 + "\n")

        except KeyboardInterrupt:
            print("\n用户中断，退出...")
            break
        except Exception as e:
            print(f"错误: {e}")

async def performance_test():
    """性能测试示例"""
    print("=== 性能测试示例 ===\n")

    import time

    agent = ResearchAgent(provider="mock", model="mock-model")

    queries = [
        "机器学习算法比较",
        "云计算架构设计",
        "数据隐私保护技术",
        "微服务最佳实践",
        "DevOps工具链选择"
    ]

    print(f"执行 {len(queries)} 个查询的性能测试...\n")

    start_time = time.time()

    for i, query in enumerate(queries, 1):
        query_start = time.time()

        result = await agent.conduct_research(query, max_sources=3)

        query_time = time.time() - query_start
        print(f"查询 {i}: {query[:30]}... - {query_time:.2f}s")

    total_time = time.time() - start_time
    avg_time = total_time / len(queries)

    print(f"\n=== 性能测试结果 ===")
    print(f"总查询数: {len(queries)}")
    print(f"总时间: {total_time:.2f}s")
    print(f"平均时间: {avg_time:.2f}s")
    print(f"查询/秒: {len(queries) / total_time:.2f}")

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
    print("6. 交互式示例")
    print("0. 运行所有示例")

    try:
        choice = input("\n请选择要运行的示例 (0-6): ").strip()

        if choice == "0":
            # 运行所有示例
            for name, func in examples:
                print(f"\n{'='*60}")
                print(f"运行: {name}")
                print('='*60)
                await func()
        elif choice == "6":
            await interactive_example()
        elif choice.isdigit() and 1 <= int(choice) <= 5:
            index = int(choice) - 1
            name, func = examples[index]
            print(f"\n运行: {name}")
            await func()
        else:
            print("无效选择，运行基础示例...")
            await basic_usage_example()

    except KeyboardInterrupt:
        print("\n用户中断")
    except Exception as e:
        print(f"错误: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    print("Research Agent 使用示例程序")
    print("基于Claude Agent SDK构建的专业技术调研工具\n")

    # 检查环境
    try:
        from research_agent import ResearchAgent
        print("✓ ResearchAgent 导入成功")
    except ImportError as e:
        print(f"✗ 导入错误: {e}")
        print("请确保依赖已正确安装")
        sys.exit(1)

    # 运行示例
    asyncio.run(main())