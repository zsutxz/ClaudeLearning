#!/usr/bin/env python3
"""
RAG系统高级使用示例
演示不同的配置选项和高级功能
"""

import os
import sys
from pathlib import Path

# 添加src目录到Python路径
sys.path.append(str(Path(__file__).parent.parent / "src"))

from rag_system import RAGSystem, RAGConfig

def demo_different_retrieval_strategies():
    """演示不同的检索策略"""
    print("\n=== 不同检索策略对比 ===\n")

    strategies = ["semantic", "multi_query", "contextual"]
    query = "RAG系统的主要挑战是什么？"

    for strategy in strategies:
        print(f"策略: {strategy}")
        config = RAGConfig(
            data_path="./data/sample_documents",
            vector_store_type="chroma",
            embedding_model="openai",
            retrieval_strategy=strategy,
            top_k=3
        )

        rag = RAGSystem(config)
        rag.index_documents()

        result = rag.query(query)
        print(f"答案: {result['answer'][:200]}...")
        print("-" * 50)

def demo_different_chunk_strategies():
    """演示不同的文档分割策略"""
    print("\n=== 不同文档分割策略对比 ===\n")

    strategies = [
        ("recursive", "递归字符分割"),
        ("token", "Token分割"),
        ("semantic", "语义分割")
    ]

    query = "深度学习的关键概念有哪些？"

    for strategy, name in strategies:
        print(f"\n分割策略: {name}")
        config = RAGConfig(
            data_path="./data/sample_documents",
            vector_store_type="chroma",
            embedding_model="openai",
            chunk_strategy=strategy,
            chunk_size=500,
            chunk_overlap=50
        )

        rag = RAGSystem(config)
        rag.index_documents()

        result = rag.query(query)
        print(f"答案: {result['answer'][:200]}...")

def demo_conversational_mode():
    """演示对话模式"""
    print("\n=== 对话模式演示 ===\n")

    config = RAGConfig(
        data_path="./data/sample_documents",
        vector_store_type="chroma",
        embedding_model="openai",
        retrieval_strategy="contextual"
    )

    rag = RAGSystem(config)
    rag.index_documents()

    # 对话流程
    conversations = [
        "什么是机器学习？",
        "监督学习和无监督学习有什么区别？",
        "能给我举个监督学习的例子吗？"
    ]

    for message in conversations:
        print(f"用户: {message}")
        response = rag.chat(message)
        print(f"助手: {response}")
        print()

def demo_custom_prompt_template():
    """演示自定义提示模板"""
    print("\n=== 自定义提示模板演示 ===\n")

    config = RAGConfig(
        data_path="./data/sample_documents",
        vector_store_type="chroma",
        embedding_model="openai"
    )

    rag = RAGSystem(config)
    rag.index_documents()

    # 修改提示模板
    template = """你是一个专业的AI助手。请基于提供的上下文回答问题，
    如果信息不足，请明确指出。回答时请使用中文，并保持专业和准确。

    上下文：
    {context}

    问题：{question}

    专业回答："""

    # 这里需要手动修改QA链的提示模板
    # 实际实现中可以在RAGConfig中添加prompt_template参数

    query = "自然语言处理有哪些应用？"
    result = rag.query(query)
    print(f"查询: {query}")
    print(f"回答: {result['answer']}")

def demo_metadata_filtering():
    """演示元数据过滤（概念性示例）"""
    print("\n=== 元数据过滤演示 ===\n")

    print("在实际应用中，可以基于文档元数据进行过滤：")
    print("- 按文档类型过滤（如只搜索PDF文档）")
    print("- 按时间范围过滤（如只搜索最近一年的文档）")
    print("- 按作者过滤")
    print("- 按分类过滤")
    print("\n这需要在文档加载时添加相应的元数据，并在检索时应用过滤器。")

def main():
    # 检查API密钥
    if not os.getenv("OPENAI_API_KEY"):
        print("错误：请设置OPENAI_API_KEY环境变量")
        return

    print("=== RAG系统高级使用示例 ===")

    # 运行各个演示
    demo_different_retrieval_strategies()
    demo_different_chunk_strategies()
    demo_conversational_mode()
    demo_custom_prompt_template()
    demo_metadata_filtering()

    print("\n=== 高级示例完成 ===")

if __name__ == "__main__":
    main()