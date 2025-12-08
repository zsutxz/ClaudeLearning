#!/usr/bin/env python3
"""
简单的本地嵌入测试 - 不需要下载大模型
使用scikit-learn的TF-IDF作为基准嵌入方法
"""

import os
import sys
from pathlib import Path
import numpy as np
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity

class LocalTfidfEmbeddings:
    """使用TF-IDF作为文本嵌入的简单实现"""

    def __init__(self):
        self.vectorizer = TfidfVectorizer(
            max_features=1000,  # 限制特征数量
            ngram_range=(1, 2),  # 使用unigram和bigram
            stop_words=None  # 中文不需要停用词
        )
        self.fitted = False
        self.embeddings = None

    def fit(self, documents):
        """训练TF-IDF向量化器"""
        texts = [doc.page_content if hasattr(doc, 'page_content') else str(doc)
                for doc in documents]
        self.embeddings = self.vectorizer.fit_transform(texts)
        self.fitted = True
        return self.embeddings

    def embed_query(self, text):
        """将查询文本转换为嵌入向量"""
        if not self.fitted:
            # 如果没有训练过，使用默认的词汇表
            self.vectorizer.fit([text])
            self.fitted = True
        return self.vectorizer.transform([text]).toarray()[0]

    def embed_documents(self, texts):
        """批量转换文档"""
        if not self.fitted:
            self.embeddings = self.vectorizer.fit_transform(texts)
            self.fitted = True
            return self.embeddings.toarray()
        return self.vectorizer.transform(texts).toarray()

    def search(self, query_text, documents, k=3):
        """搜索最相似的文档"""
        # 首先进行训练，确保有词汇表
        if not self.fitted:
            self.fit(documents)

        query_emb = self.embed_query(query_text)

        # 获取所有文档的嵌入（重新训练以确保一致性）
        doc_texts = [doc.page_content if hasattr(doc, 'page_content') else str(doc)
                     for doc in documents]
        doc_embeddings = self.vectorizer.transform(doc_texts).toarray()

        # 计算相似度
        similarities = cosine_similarity([query_emb], doc_embeddings)[0]

        # 获取Top-K结果（即使相似度为0也要返回）
        top_indices = np.argsort(similarities)[::-1][:k]

        results = []
        for idx in top_indices:
            results.append({
                'document': documents[idx],
                'similarity': similarities[idx]
            })

        return results

def test_local_tfidf_rag():
    """测试基于TF-IDF的本地RAG系统"""
    print("="*60)
    print("RAG系统测试 (TF-IDF本地嵌入版本)")
    print("="*60)

    try:
        # 准备测试文档
        print("\n1. 准备测试文档...")

        try:
            from langchain_core.documents import Document
        except ImportError:
            from langchain.schema import Document

        documents = [
            Document(
                page_content="RAG（Retrieval-Augmented Generation）是一种结合信息检索和文本生成的AI技术。RAG系统首先从知识库中检索相关信息，然后基于这些信息生成答案，提高了回答的准确性和可靠性。",
                metadata={"source": "ai_knowledge.txt", "category": "技术概念"}
            ),
            Document(
                page_content="Transformer是Google在2017年提出的深度学习模型架构，完全基于注意力机制，抛弃了传统的循环神经网络(RNN)和卷积神经网络(CNN)。Transformer在自然语言处理领域取得了巨大成功。",
                metadata={"source": "deep_learning.txt", "category": "深度学习"}
            ),
            Document(
                page_content="BERT（Bidirectional Encoder Representations from Transformers）是Google开发的语言表示模型。BERT通过预训练和微调的方式，在多项自然语言处理任务上取得了最好的效果。",
                metadata={"source": "nlp_models.txt", "category": "NLP模型"}
            ),
            Document(
                page_content="向量数据库专门用于存储和查询高维向量数据。它们支持高效的相似度搜索，是RAG系统中的关键组件，用于快速找到与查询最相关的文档。",
                metadata={"source": "vector_db.txt", "category": "数据库"}
            ),
            Document(
                page_content="微调(Fine-tuning)是在预训练模型的基础上，使用特定任务的数据进行进一步训练的过程。微调可以让模型适应特定的应用场景，提高任务性能。",
                metadata={"source": "model_training.txt", "category": "模型训练"}
            )
        ]

        print(f"准备了 {len(documents)} 个文档")

        # 创建嵌入系统
        print("\n2. 初始化TF-IDF嵌入系统...")
        embeddings = LocalTfidfEmbeddings()

        # 训练嵌入模型
        print("3. 训练嵌入模型...")
        embeddings.fit(documents)
        print("TF-IDF模型训练完成！")

        # 测试查询
        print("\n4. 测试语义搜索...")
        test_queries = [
            "什么是RAG技术？",
            "Transformer模型有什么特点？",
            "如何训练语言模型？",
            "向量数据库的作用是什么？"
        ]

        for query in test_queries:
            print(f"\n查询: {query}")
            print("-" * 40)

            results = embeddings.search(query, documents, k=2)

            if results:
                for i, result in enumerate(results, 1):
                    doc = result['document']
                    similarity = result['similarity']
                    print(f"\n结果 {i} (相似度: {similarity:.3f}):")
                    print(f"  类别: {doc.metadata.get('category', '未知')}")
                    print(f"  来源: {doc.metadata.get('source', '未知')}")
                    print(f"  内容: {doc.page_content[:100]}...")
            else:
                print("未找到相关文档")

        # 性能测试
        print("\n5. 性能测试...")
        import time

        # 测试批量处理速度
        batch_texts = [f"这是测试文本{i}" for i in range(100)]

        start_time = time.time()
        embeddings.embed_documents(batch_texts)
        end_time = time.time()

        print(f"批量处理100个文本耗时: {end_time - start_time:.3f}秒")
        print(f"平均每个文本: {(end_time - start_time)/100*1000:.1f}毫秒")

        # 显示一些统计信息
        print("\n6. 系统信息...")
        print(f"特征维度: {len(embeddings.vectorizer.vocabulary_)}")
        print(f"文档数量: {len(documents)}")
        print(f"词汇表大小: {len(embeddings.vectorizer.get_feature_names_out())}")

        print("\n" + "="*60)
        print("测试完成！TF-IDF RAG系统工作正常")

        print("\n当前系统特点:")
        print("[√] 完全本地运行，无需网络连接")
        print("[√] 无需下载大型模型文件")
        print("[√] 处理速度快")
        print("[√] 适合作为RAG系统的入门方案")

        print("\n升级建议:")
        print("1. 对于更好的语义理解，建议使用Sentence-Transformers")
        print("2. 可以结合词向量（如Word2Vec）改进TF-IDF")
        print("3. 考虑使用BM25算法进行检索")

        return True

    except Exception as e:
        print(f"\n错误: {str(e)}")
        import traceback
        traceback.print_exc()
        return False

if __name__ == "__main__":
    success = test_local_tfidf_rag()
    sys.exit(0 if success else 1)