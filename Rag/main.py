#!/usr/bin/env python3
"""
RAG系统主程序
使用Sentence-Transformers进行本地文本嵌入测试
"""

import sys
from pathlib import Path

# 导入自定义模块
from embeddings.sentence_transformers_embeddings import SentenceTransformersEmbeddings
from core.document_loader import DocumentLoader
from core.vector_store import VectorStoreManager
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

if __name__ == "__main__":
    success = test_sentence_transformers_rag()
    sys.exit(0 if success else 1)