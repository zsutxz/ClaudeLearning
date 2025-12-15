#!/usr/bin/env python3
"""
RAG系统主程序
集成了检索（Retrieval）和生成（Generation）的完整RAG系统
"""

import sys
import os
from pathlib import Path

# 导入自定义模块
from embeddings.sentence_transformers_embeddings import SentenceTransformersEmbeddings
from core.document_loader import DocumentLoader
from core.vector_store import VectorStoreManager
from core.rag_system import CompleteRAGSystem, RAGConfig
from config.environment import setup_environment
from config.huggingface_mirror import setup_huggingface_mirror

def test_sentence_transformers_rag():
    """测试使用Sentence-Transformers的RAG系统"""
    print("="*60)
    print("RAG系统测试 (Sentence-Transformers本地嵌入版本)")
    print("="*60)

    try:
        # 设置环境
        print("\n1. 设置环境...")
        env_manager = setup_environment()

        # 设置 Hugging Face 镜像
        print("\n2. 设置 Hugging Face 国内镜像...")
        setup_huggingface_mirror()

        # 创建Sentence-Transformers嵌入实例
        print("\n3. 初始化本地嵌入模型...")
        embeddings = SentenceTransformersEmbeddings(
            model_name="paraphrase-multilingual-MiniLM-L12-v2"
        )

        # 测试单个文本嵌入
        test_text = "这是一个测试文本"
        print("\n4. 测试文本嵌入...")
        test_result = embeddings.test_embedding(test_text)
        print(f"[OK] 嵌入维度: {test_result['embedding_dimension']}")
        print(f"[OK] 嵌入耗时: {test_result['embedding_time']:.2f}秒")

        # 测试批量嵌入
        print("\n5. 测试批量嵌入...")
        batch_result = embeddings.test_batch_embedding()
        print(f"[OK] 批量大小: {batch_result['batch_size']}")
        print(f"[OK] 平均每个文本耗时: {batch_result['avg_time_per_text']:.3f}秒")

        # 加载文档
        print("\n6. 测试文档加载...")
        document_loader = DocumentLoader()
        documents = document_loader.load_text_documents()

        # 使用实际文档或测试文档
        if documents:
            docs = documents[:5]
            print(f"使用真实文档: {len(docs)} 个")
        else:
            docs = document_loader.create_test_documents()
            print(f"使用测试文档: {len(docs)} 个")

        # 创建向量存储
        print("\n7. 创建向量存储...")
        vector_store_manager = VectorStoreManager()
        vector_store = vector_store_manager.create_vector_store(
            documents=docs,
            embeddings=embeddings
        )

        # 测试相似度搜索
        print("\n8. 测试相似度搜索...")
        queries = [
            "什么是RAG技术？",
            "如何生成文本嵌入？",
            "RAG系统如何工作？"
        ]

        for query in queries:
            results = vector_store_manager.similarity_search(query, k=2)
            vector_store_manager.print_search_results(results)

        # 相似度分析
        print("\n9. 嵌入相似度分析...")
        from utils.similarity import SimilarityCalculator

        test_texts = [
            "人工智能正在改变世界",
            "AI技术影响我们的生活",
            "今天天气很好"
        ]

        # 生成嵌入
        embeddings_list = embeddings.embed_documents(test_texts)

        # 计算相似度
        sim12 = SimilarityCalculator.cosine_similarity(
            embeddings_list[0], embeddings_list[1]
        )
        sim13 = SimilarityCalculator.cosine_similarity(
            embeddings_list[0], embeddings_list[2]
        )

        print(f"\n文本1: {test_texts[0]}")
        print(f"文本2: {test_texts[1]}")
        print(f"文本3: {test_texts[2]}")
        print(f"\n文本1和文本2的相似度: {sim12:.3f} (应该较高)")
        print(f"文本1和文本3的相似度: {sim13:.3f} (应该较低)")

        print("\n" + "="*60)
        print("测试完成！Sentence-Transformers RAG系统工作正常")
        print("\n优势:")
        print("[+] 完全本地运行，无需API密钥")
        print("[+] 支持多种语言的预训练模型")
        print("[+] 可离线使用，保护数据隐私")
        print("[+] 批量处理效率高")
        print("[+] 支持自定义模型微调")

        print("\n推荐的中文优化模型:")
        print("- shibing624/text2vec-base-chinese")
        print("- sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2")
        print("- sentence-transformers/distiluse-base-multilingual-cased-v2")

        return True

    except Exception as e:
        print(f"\n错误: {str(e)}")
        import traceback
        traceback.print_exc()
        return False

def test_complete_rag_system():
    """测试完整的RAG系统（检索+生成）"""
    print("="*60)
    print("完整RAG系统测试 (DeepSeek LLM集成)")
    print("="*60)

    try:
        # 检查环境变量
        deepseek_api_key = os.getenv('DEEPSEEK_API_KEY')
        if not deepseek_api_key:
            print("\n[警告] 未设置DEEPSEEK_API_KEY环境变量")
            print("将只进行检索测试，不生成答案")
            print("\n要使用完整功能，请:")
            print("1. 设置环境变量: export DEEPSEEK_API_KEY=your_key")
            print("2. 或创建.env文件并添加DEEPSEEK_API_KEY=your_key")

        # 设置环境
        print("\n1. 设置环境...")
        env_manager = setup_environment()

        # 设置 Hugging Face 镜像
        print("\n2. 设置 Hugging Face 国内镜像...")
        setup_huggingface_mirror()

        # 创建RAG配置
        print("\n3. 创建RAG系统配置...")
        config = RAGConfig(
            embedding_model_name="paraphrase-multilingual-MiniLM-L12-v2",
            vector_store_type="chroma",
            llm_provider="deepseek",
            llm_model_name="deepseek-chat",
            retrieval_k=3,
            max_tokens=1000,
            temperature=0.7
        )

        # 初始化完整RAG系统
        print("\n4. 初始化完整RAG系统...")
        rag_system = CompleteRAGSystem(config)

        # 显示系统信息
        stats = rag_system.get_stats()
        print(f"\n[系统信息]")
        for key, value in stats.items():
            print(f"  - {key}: {value}")

        # 测试查询
        print("\n5. 执行测试查询...")
        test_questions = [
            "什么是RAG技术？",
            "如何优化RAG系统的性能？",
            "RAG系统有哪些应用场景？"
        ]

        # 执行单个查询测试
        print("\n[单个查询测试]")
        question = test_questions[0]
        result = rag_system.query(question)
        rag_system.print_result(result)

        # 批量查询测试
        print("\n[批量查询测试]")
        batch_results = rag_system.batch_query(test_questions)

        # 打印总结
        print("\n" + "="*60)
        print("测试完成！完整RAG系统工作正常")
        print("\n系统特性:")
        print("[+] 本地嵌入模型，支持离线检索")
        print("[+] DeepSeek LLM集成，智能生成答案")
        print("[+] 模块化设计，易于扩展")
        print("[+] 支持多种向量数据库")
        print("[+] 完整的RAG流程：检索→生成")

        if deepseek_api_key:
            print("\n[✓] 完整RAG功能已启用")
        else:
            print("\n[!] 仅检索功能可用（需配置DEEPSEEK_API_KEY以启用生成）")

        return True

    except Exception as e:
        print(f"\n错误: {str(e)}")
        import traceback
        traceback.print_exc()
        return False


def main():
    """主函数"""
    import argparse

    parser = argparse.ArgumentParser(description="RAG系统测试程序")
    parser.add_argument(
        "--mode",
        choices=["retrieval", "complete", "both"],
        default="complete",
        help="运行模式: retrieval(仅检索), complete(完整RAG), both(两者)"
    )

    args = parser.parse_args()

    success = True

    if args.mode in ["retrieval", "both"]:
        print("\n" + "="*60)
        print("运行检索系统测试...")
        print("="*60)
        success_retrieval = test_sentence_transformers_rag()
        success = success and success_retrieval

    if args.mode in ["complete", "both"]:
        print("\n" + "="*60)
        print("运行完整RAG系统测试...")
        print("="*60)
        success_complete = test_complete_rag_system()
        success = success and success_complete

    return success


if __name__ == "__main__":
    success = main()
    sys.exit(0 if success else 1)