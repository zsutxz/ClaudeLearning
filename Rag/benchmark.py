#!/usr/bin/env python3
"""
RAG系统性能基准测试
测试不同配置下的性能表现
"""

import os
import sys
import time
import json
import statistics
from pathlib import Path
from datetime import datetime
from concurrent.futures import ThreadPoolExecutor
import matplotlib.pyplot as plt
import numpy as np

# 添加src目录到Python路径
sys.path.append(str(Path(__file__).parent / "src"))

# 颜色输出
class Colors:
    GREEN = '\033[92m'
    YELLOW = '\033[93m'
    RED = '\033[91m'
    BLUE = '\033[94m'
    RESET = '\033[0m'
    BOLD = '\033[1m'


def print_color(text, color):
    """打印彩色文本"""
    print(f"{color}{text}{Colors.RESET}")


class BenchmarkResult:
    """基准测试结果类"""

    def __init__(self, name):
        self.name = name
        self.response_times = []
        self.indexing_time = None
        self.memory_usage = []
        self.error_count = 0
        self.total_queries = 0

    def add_response_time(self, time):
        """添加响应时间"""
        self.response_times.append(time)

    def get_stats(self):
        """获取统计信息"""
        if not self.response_times:
            return None

        return {
            "avg": statistics.mean(self.response_times),
            "median": statistics.median(self.response_times),
            "min": min(self.response_times),
            "max": max(self.response_times),
            "std": statistics.stdev(self.response_times) if len(self.response_times) > 1 else 0,
            "p95": np.percentile(self.response_times, 95),
            "p99": np.percentile(self.response_times, 99)
        }


class RAGBenchmark:
    """RAG系统基准测试器"""

    def __init__(self):
        self.results = {}
        self.test_queries = [
            "什么是RAG系统？",
            "机器学习有哪些类型？",
            "深度学习的应用有哪些？",
            "自然语言处理是什么？",
            "计算机视觉的应用场景？",
            "RAG系统的组成部分？",
            "如何评估RAG系统？",
            "向量数据库的选择？",
            "嵌入模型的比较？",
            "检索策略的优化？",
            "AI的发展历史？",
            "监督学习的特点？",
            "无监督学习的应用？",
            "强化学习的原理？",
            "神经网络的结构？"
        ]

    def benchmark_vector_stores(self):
        """测试不同向量数据库的性能"""
        print_color("\n=== 向量数据库性能对比 ===", Colors.BOLD + Colors.BLUE)

        vector_stores = ["chroma", "faiss"]

        for store_type in vector_stores:
            print_color(f"\n测试 {store_type.upper()}...", Colors.YELLOW)
            result = BenchmarkResult(f"vector_store_{store_type}")

            try:
                from rag_system import RAGSystem, RAGConfig

                # 创建配置
                config = RAGConfig(
                    data_path="./data/sample_documents",
                    vector_store_type=store_type,
                    embedding_model="openai",
                    chunk_size=1000,
                    top_k=5
                )

                # 创建RAG系统
                rag = RAGSystem(config)

                # 测试索引时间
                print("  索引文档...")
                start_time = time.time()
                rag.index_documents()
                result.indexing_time = time.time() - start_time
                print(f"  索引时间: {result.indexing_time:.2f}秒")

                # 执行查询测试
                print("  执行查询测试...")
                for query in self.test_queries[:5]:  # 使用前5个查询
                    start_time = time.time()
                    rag_result = rag.query(query)
                    response_time = time.time() - start_time

                    if rag_result["success"]:
                        result.add_response_time(response_time)
                        result.total_queries += 1
                    else:
                        result.error_count += 1

                self.results[result.name] = result
                print(f"  平均响应时间: {result.get_stats()['avg']:.2f}秒")

            except Exception as e:
                print_color(f"  错误: {str(e)}", Colors.RED)

    def benchmark_embeddings(self):
        """测试不同嵌入模型的性能"""
        print_color("\n=== 嵌入模型性能对比 ===", Colors.BOLD + Colors.BLUE)

        embeddings = [
            ("openai", "text-embedding-ada-002"),
            ("huggingface", "sentence-transformers/all-MiniLM-L6-v2")
        ]

        for embed_type, embed_name in embeddings:
            print_color(f"\n测试 {embed_name}...", Colors.YELLOW)
            result = BenchmarkResult(f"embedding_{embed_type}")

            try:
                from rag_system import RAGSystem, RAGConfig

                config = RAGConfig(
                    data_path="./data/sample_documents",
                    vector_store_type="chroma",
                    embedding_model=embed_type,
                    embedding_model_name=embed_name,
                    chunk_size=1000,
                    top_k=5
                )

                rag = RAGSystem(config)

                # 索引
                start_time = time.time()
                rag.index_documents()
                result.indexing_time = time.time() - start_time

                # 查询测试
                for query in self.test_queries[:5]:
                    start_time = time.time()
                    rag_result = rag.query(query)
                    response_time = time.time() - start_time

                    if rag_result["success"]:
                        result.add_response_time(response_time)
                        result.total_queries += 1
                    else:
                        result.error_count += 1

                self.results[result.name] = result
                print(f"  平均响应时间: {result.get_stats()['avg']:.2f}秒")

            except Exception as e:
                print_color(f"  错误: {str(e)}", Colors.RED)

    def benchmark_retrieval_strategies(self):
        """测试不同检索策略的性能"""
        print_color("\n=== 检索策略性能对比 ===", Colors.BOLD + Colors.BLUE)

        strategies = ["semantic", "multi_query", "contextual"]

        for strategy in strategies:
            print_color(f"\n测试 {strategy} 策略...", Colors.YELLOW)
            result = BenchmarkResult(f"strategy_{strategy}")

            try:
                from rag_system import RAGSystem, RAGConfig

                config = RAGConfig(
                    data_path="./data/sample_documents",
                    vector_store_type="chroma",
                    embedding_model="openai",
                    retrieval_strategy=strategy,
                    chunk_size=1000,
                    top_k=5
                )

                rag = RAGSystem(config)

                # 索引
                start_time = time.time()
                rag.index_documents()
                result.indexing_time = time.time() - start_time

                # 查询测试
                for query in self.test_queries[:5]:
                    start_time = time.time()
                    rag_result = rag.query(query)
                    response_time = time.time() - start_time

                    if rag_result["success"]:
                        result.add_response_time(response_time)
                        result.total_queries += 1
                    else:
                        result.error_count += 1

                self.results[result.name] = result
                print(f"  平均响应时间: {result.get_stats()['avg']:.2f}秒")

            except Exception as e:
                print_color(f"  错误: {str(e)}", Colors.RED)

    def benchmark_chunk_sizes(self):
        """测试不同文档块大小的性能"""
        print_color("\n=== 文档块大小性能对比 ===", Colors.BOLD + Colors.BLUE)

        chunk_sizes = [500, 1000, 1500, 2000]

        for size in chunk_sizes:
            print_color(f"\n测试块大小 {size}...", Colors.YELLOW)
            result = BenchmarkResult(f"chunk_size_{size}")

            try:
                from rag_system import RAGSystem, RAGConfig

                config = RAGConfig(
                    data_path="./data/sample_documents",
                    vector_store_type="chroma",
                    embedding_model="openai",
                    chunk_size=size,
                    chunk_overlap=50,
                    top_k=5
                )

                rag = RAGSystem(config)

                # 索引
                start_time = time.time()
                rag.index_documents()
                result.indexing_time = time.time() - start_time

                # 查询测试
                for query in self.test_queries[:5]:
                    start_time = time.time()
                    rag_result = rag.query(query)
                    response_time = time.time() - start_time

                    if rag_result["success"]:
                        result.add_response_time(response_time)
                        result.total_queries += 1
                    else:
                        result.error_count += 1

                self.results[result.name] = result
                print(f"  平均响应时间: {result.get_stats()['avg']:.2f}秒")

            except Exception as e:
                print_color(f"  错误: {str(e)}", Colors.RED)

    def benchmark_concurrent_queries(self):
        """测试并发查询性能"""
        print_color("\n=== 并发查询性能测试 ===", Colors.BOLD + Colors.BLUE)

        try:
            from rag_system import create_rag_system

            # 创建RAG系统
            rag = create_rag_system(
                data_path="./data/sample_documents",
                vector_store_type="chroma",
                embedding_model="openai"
            )

            # 测试不同的并发级别
            concurrency_levels = [1, 2, 5, 10]

            for level in concurrency_levels:
                print_color(f"\n测试并发级别 {level}...", Colors.YELLOW)
                result = BenchmarkResult(f"concurrent_{level}")

                def execute_query(query):
                    """执行单个查询"""
                    start_time = time.time()
                    rag_result = rag.query(query)
                    response_time = time.time() - start_time
                    return response_time, rag_result["success"]

                # 并发执行查询
                start_time = time.time()
                with ThreadPoolExecutor(max_workers=level) as executor:
                    futures = [
                        executor.submit(execute_query, query)
                        for query in self.test_queries[:10]  # 使用10个查询
                    ]

                    for future in futures:
                        response_time, success = future.result()
                        if success:
                            result.add_response_time(response_time)
                            result.total_queries += 1
                        else:
                            result.error_count += 1

                total_time = time.time() - start_time
                result.total_execution_time = total_time

                print(f"  总执行时间: {total_time:.2f}秒")
                print(f"  平均响应时间: {result.get_stats()['avg']:.2f}秒")
                print(f"  QPS: {result.total_queries / total_time:.2f}")

                self.results[result.name] = result

        except Exception as e:
            print_color(f"并发测试错误: {str(e)}", Colors.RED)

    def generate_report(self):
        """生成测试报告"""
        print_color("\n=== 生成测试报告 ===", Colors.BOLD + Colors.BLUE)

        # 文本报告
        report_lines = []
        report_lines.append("RAG系统性能基准测试报告")
        report_lines.append("="*50)
        report_lines.append(f"测试时间: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        report_lines.append("")

        # 统计各项测试结果
        for name, result in self.results.items():
            stats = result.get_stats()
            if stats:
                report_lines.append(f"{name}:")
                report_lines.append(f"  平均响应时间: {stats['avg']:.2f}秒")
                report_lines.append(f"  中位数: {stats['median']:.2f}秒")
                report_lines.append(f"  最小值: {stats['min']:.2f}秒")
                report_lines.append(f"  最大值: {stats['max']:.2f}秒")
                report_lines.append(f"  标准差: {stats['std']:.2f}秒")
                report_lines.append(f"  95%分位数: {stats['p95']:.2f}秒")
                if result.indexing_time:
                    report_lines.append(f"  索引时间: {result.indexing_time:.2f}秒")
                report_lines.append(f"  成功率: {(result.total_queries / (result.total_queries + result.error_count) * 100):.1f}%")
                report_lines.append("")

        # 保存报告
        report_file = f"benchmark_report_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"
        with open(report_file, 'w', encoding='utf-8') as f:
            f.write('\n'.join(report_lines))

        # JSON格式报告
        json_results = {}
        for name, result in self.results.items():
            json_results[name] = {
                "stats": result.get_stats(),
                "indexing_time": result.indexing_time,
                "total_queries": result.total_queries,
                "error_count": result.error_count
            }

        json_file = f"benchmark_results_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"
        with open(json_file, 'w', encoding='utf-8') as f:
            json.dump(json_results, f, ensure_ascii=False, indent=2)

        print_color(f"报告已保存:", Colors.GREEN)
        print(f"  文本报告: {report_file}")
        print(f"  JSON报告: {json_file}")

        # 生成可视化图表
        self.generate_plots()

    def generate_plots(self):
        """生成性能图表"""
        try:
            # 按测试类型分组结果
            vector_store_results = {}
            embedding_results = {}
            strategy_results = {}
            chunk_results = {}
            concurrent_results = {}

            for name, result in self.results.items():
                stats = result.get_stats()
                if not stats:
                    continue

                if name.startswith("vector_store_"):
                    vector_store_results[name.replace("vector_store_", "")] = stats['avg']
                elif name.startswith("embedding_"):
                    embedding_results[name.replace("embedding_", "")] = stats['avg']
                elif name.startswith("strategy_"):
                    strategy_results[name.replace("strategy_", "")] = stats['avg']
                elif name.startswith("chunk_size_"):
                    size = name.replace("chunk_size_", "")
                    chunk_results[int(size)] = stats['avg']
                elif name.startswith("concurrent_"):
                    concurrent_results[name.replace("concurrent_", "")] = stats['avg']

            # 创建图表
            if vector_store_results or embedding_results or strategy_results:
                fig, axes = plt.subplots(2, 2, figsize=(12, 10))
                fig.suptitle('RAG系统性能基准测试', fontsize=16)

                # 向量数据库对比
                if vector_store_results:
                    axes[0, 0].bar(vector_store_results.keys(), vector_store_results.values())
                    axes[0, 0].set_title('向量数据库响应时间对比')
                    axes[0, 0].set_ylabel('平均响应时间 (秒)')

                # 嵌入模型对比
                if embedding_results:
                    axes[0, 1].bar(embedding_results.keys(), embedding_results.values())
                    axes[0, 1].set_title('嵌入模型响应时间对比')
                    axes[0, 1].set_ylabel('平均响应时间 (秒)')

                # 检索策略对比
                if strategy_results:
                    axes[1, 0].bar(strategy_results.keys(), strategy_results.values())
                    axes[1, 0].set_title('检索策略响应时间对比')
                    axes[1, 0].set_ylabel('平均响应时间 (秒)')

                # 文档块大小对比
                if chunk_results:
                    sizes = sorted(chunk_results.keys())
                    times = [chunk_results[size] for size in sizes]
                    axes[1, 1].plot(sizes, times, marker='o')
                    axes[1, 1].set_title('文档块大小对响应时间的影响')
                    axes[1, 1].set_xlabel('块大小')
                    axes[1, 1].set_ylabel('平均响应时间 (秒)')

                plt.tight_layout()
                plot_file = f"benchmark_plots_{datetime.now().strftime('%Y%m%d_%H%M%S')}.png"
                plt.savefig(plot_file)
                print_color(f"性能图表已保存: {plot_file}", Colors.GREEN)

        except Exception as e:
            print_color(f"生成图表失败: {str(e)}", Colors.RED)

    def run_all_benchmarks(self):
        """运行所有基准测试"""
        print_color("\n🚀 开始RAG系统性能基准测试", Colors.BOLD + Colors.BLUE)
        print("="*60)

        # 检查API密钥
        if not os.getenv("OPENAI_API_KEY"):
            print_color("❌ 请设置OPENAI_API_KEY环境变量", Colors.RED)
            return

        start_time = time.time()

        # 运行各项测试
        self.benchmark_vector_stores()
        self.benchmark_embeddings()
        self.benchmark_retrieval_strategies()
        self.benchmark_chunk_sizes()
        self.benchmark_concurrent_queries()

        total_time = time.time() - start_time
        print_color(f"\n✅ 所有测试完成！总耗时: {total_time:.2f}秒", Colors.GREEN)

        # 生成报告
        self.generate_report()


def main():
    """主函数"""
    print_color("RAG系统性能基准测试工具", Colors.BOLD + Colors.CYAN)
    print("="*60)

    benchmark = RAGBenchmark()

    # 选择测试类型
    print("\n请选择要执行的测试:")
    options = [
        "运行所有测试",
        "向量数据库对比",
        "嵌入模型对比",
        "检索策略对比",
        "文档块大小对比",
        "并发性能测试"
    ]

    choice = input("\n请选择 (1-6): ").strip()

    if choice == "1":
        benchmark.run_all_benchmarks()
    elif choice == "2":
        benchmark.benchmark_vector_stores()
        benchmark.generate_report()
    elif choice == "3":
        benchmark.benchmark_embeddings()
        benchmark.generate_report()
    elif choice == "4":
        benchmark.benchmark_retrieval_strategies()
        benchmark.generate_report()
    elif choice == "5":
        benchmark.benchmark_chunk_sizes()
        benchmark.generate_report()
    elif choice == "6":
        benchmark.benchmark_concurrent_queries()
        benchmark.generate_report()
    else:
        print_color("无效选择", Colors.RED)
        return

    print_color("\n测试完成！请查看生成的报告文件。", Colors.GREEN)


if __name__ == "__main__":
    main()