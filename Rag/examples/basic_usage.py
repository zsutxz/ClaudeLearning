#!/usr/bin/env python3
"""
RAG系统基础使用示例
演示如何快速创建和使用RAG系统
"""

import os
import sys
from pathlib import Path

# 添加src目录到Python路径
sys.path.append(str(Path(__file__).parent.parent / "src"))

from rag_system import create_rag_system, RAGConfig

def main():
    # 配置API密钥（确保在.env文件中设置了OPENAI_API_KEY）
    if not os.getenv("OPENAI_API_KEY"):
        print("错误：请设置OPENAI_API_KEY环境变量")
        print("可以创建.env文件并添加：OPENAI_API_KEY=your_api_key_here")
        return

    print("=== RAG系统基础使用示例 ===\n")

    # 1. 创建RAG系统（使用默认配置）
    print("1. 创建RAG系统...")
    rag = create_rag_system(
        data_path="./data/sample_documents",
        vector_store_type="chroma",
        embedding_model="openai"
    )
    print("✓ RAG系统创建完成\n")

    # 2. 查看系统统计
    print("2. 系统统计信息：")
    stats = rag.get_stats()
    for key, value in stats.items():
        print(f"   {key}: {value}")
    print()

    # 3. 执行查询
    queries = [
        "什么是RAG系统？",
        "机器学习有哪些类型？",
        "RAG系统有什么优势？",
        "如何优化RAG系统的检索质量？"
    ]

    print("3. 执行查询：")
    for i, query in enumerate(queries, 1):
        print(f"\n查询 {i}: {query}")
        result = rag.query(query, return_source=True)

        print(f"答案: {result['answer']}")

        if result.get('sources'):
            print("\n相关来源：")
            for j, source in enumerate(result['sources'], 1):
                print(f"  来源 {j}: {source['metadata'].get('source', '未知')}")
                print(f"  内容: {source['content'][:100]}...")

        print("-" * 50)

    print("\n=== 示例完成 ===")

if __name__ == "__main__":
    main()