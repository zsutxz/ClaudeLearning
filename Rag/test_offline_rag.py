#!/usr/bin/env python3
"""
离线RAG系统测试
不依赖网络连接或外部API
"""

import os
import sys
import json
import numpy as np
from pathlib import Path
from typing import List, Dict, Any

# Add src to path
sys.path.append(str(Path(__file__).parent / "src"))

class MockEmbedding:
    """模拟嵌入类，使用简单的TF-IDF向量"""

    def __init__(self, dimension=384):
        self.dimension = dimension
        self.vocab = {}
        self.idf = {}

    def _tokenize(self, text: str) -> List[str]:
        """简单的分词"""
        # 简单的中文分词
        import re
        # 移除标点符号并分割
        text = re.sub(r'[^\w\s]', '', text.lower())
        return text.split()

    def _build_vocab(self, documents: List[str]):
        """构建词汇表"""
        from collections import Counter
        doc_count = len(documents)
        word_docs = Counter()

        for doc in documents:
            words = set(self._tokenize(doc))
            word_docs.update(words)

        # 计算IDF
        self.vocab = list(word_docs.keys())
        for word, count in word_docs.items():
            self.idf[word] = np.log(doc_count / (count + 1))

    def embed_documents(self, texts: List[str]) -> List[List[float]]:
        """嵌入多个文档"""
        if not self.vocab:
            self._build_vocab(texts)

        embeddings = []
        for text in texts:
            words = self._tokenize(text)
            vector = np.zeros(self.dimension)

            # 简单的TF-IDF向量
            for i, word in enumerate(self.vocab[:self.dimension]):
                if word in words:
                    tf = words.count(word) / len(words)
                    vector[i] = tf * self.idf.get(word, 0)

            embeddings.append(vector.tolist())

        return embeddings

    def embed_query(self, text: str) -> List[float]:
        """嵌入查询"""
        return self.embed_documents([text])[0]

class SimpleVectorStore:
    """简单的向量存储"""

    def __init__(self):
        self.documents = []
        self.embeddings = []
        self.metadata = []

    def add_documents(self, docs: List[str], metadatas: List[Dict] = None):
        """添加文档"""
        self.documents.extend(docs)
        if metadatas:
            self.metadata.extend(metadatas)
        else:
            self.metadata.extend([{}] * len(docs))

        # 生成嵌入
        embedder = MockEmbedding()
        new_embeddings = embedder.embed_documents(docs)
        self.embeddings.extend(new_embeddings)

    def similarity_search(self, query: str, k: int = 5) -> List[Dict]:
        """相似度搜索"""
        if not self.documents:
            return []

        # 生成查询嵌入
        embedder = MockEmbedding()
        query_embedding = embedder.embed_query(query)

        # 计算相似度
        similarities = []
        for doc_embedding in self.embeddings:
            # 简单的余弦相似度
            similarity = np.dot(query_embedding, doc_embedding)
            similarities.append(similarity)

        # 获取top-k结果
        indices = np.argsort(similarities)[-k:][::-1]

        results = []
        for idx in indices:
            results.append({
                "content": self.documents[idx],
                "metadata": self.metadata[idx],
                "score": similarities[idx]
            })

        return results

class OfflineRAG:
    """离线RAG系统"""

    def __init__(self, data_path: str = "./data/sample_documents"):
        self.data_path = data_path
        self.vector_store = SimpleVectorStore()
        self.loaded = False

    def _load_documents(self):
        """加载文档"""
        documents = []
        metadatas = []

        if os.path.exists(self.data_path):
            for file_path in Path(self.data_path).glob("*.txt"):
                try:
                    with open(file_path, 'r', encoding='utf-8') as f:
                        content = f.read().strip()
                        if content:
                            documents.append(content)
                            metadatas.append({
                                "source": str(file_path),
                                "filename": file_path.name
                            })
                except Exception as e:
                    print(f"Error loading {file_path}: {e}")

        # 如果没有找到文档，创建示例文档
        if not documents:
            print("No documents found, creating sample documents...")
            documents = [
                "RAG（Retrieval-Augmented Generation）是一种结合信息检索和文本生成的AI技术。",
                "RAG系统可以从知识库中检索相关信息，然后基于这些信息生成答案。",
                "RAG系统的主要优势是能够提供准确的、基于事实的答案，并支持答案来源追溯。",
                "机器学习是人工智能的一个分支，它使计算机能够在没有明确编程的情况下学习。",
                "机器学习分为监督学习、无监督学习和强化学习三种主要类型。"
            ]
            metadatas = [
                {"source": "sample1.txt", "filename": "sample1.txt"},
                {"source": "sample2.txt", "filename": "sample2.txt"},
                {"source": "sample3.txt", "filename": "sample3.txt"},
                {"source": "sample4.txt", "filename": "sample4.txt"},
                {"source": "sample5.txt", "filename": "sample5.txt"}
            ]

        return documents, metadatas

    def initialize(self):
        """初始化系统"""
        print("Loading documents...")
        documents, metadatas = self._load_documents()

        print(f"Loaded {len(documents)} documents")

        print("Creating embeddings and building vector store...")
        self.vector_store.add_documents(documents, metadatas)

        print("RAG system initialized successfully!")
        self.loaded = True

    def query(self, question: str, k: int = 3) -> Dict[str, Any]:
        """查询系统"""
        if not self.loaded:
            self.initialize()

        # 检索相关文档
        results = self.vector_store.similarity_search(question, k=k)

        # 简单的答案生成（基于检索到的文档）
        answer = self._generate_answer(question, results)

        return {
            "question": question,
            "answer": answer,
            "sources": results,
            "success": True
        }

    def _generate_answer(self, question: str, sources: List[Dict]) -> str:
        """生成简单的答案"""
        if not sources:
            return "抱歉，我没有找到相关信息来回答您的问题。"

        # 简单的关键词匹配来生成答案
        question_lower = question.lower()

        # 检查问题类型
        if "什么是" in question or "what is" in question_lower:
            # 定义问题
            for source in sources:
                content = source["content"]
                if "是一种" in content or "is a" in content.lower():
                    return content[:200] + "..." if len(content) > 200 else content

        elif "优势" in question or "advantage" in question_lower:
            # 优势问题
            for source in sources:
                content = source["content"]
                if "优势" in content or "advantage" in content.lower():
                    return content[:200] + "..." if len(content) > 200 else content

        elif "类型" in question or "type" in question_lower:
            # 类型问题
            for source in sources:
                content = source["content"]
                if "分为" in content or "types include" in content.lower():
                    return content[:200] + "..." if len(content) > 200 else content

        # 默认答案
        if sources:
            return f"根据搜索结果，{' '.join([s['content'][:100] for s in sources[:2]])}..."

        return "抱歉，我没有找到相关信息。"

    def get_stats(self):
        """获取统计信息"""
        return {
            "total_documents": len(self.vector_store.documents),
            "vector_dimension": 384,
            "system_type": "Offline Mock RAG"
        }

def main():
    """主函数"""
    print("=" * 60)
    print("离线RAG系统测试")
    print("=" * 60)

    # 创建RAG系统
    rag = OfflineRAG()

    # 初始化
    print("\n1. 初始化RAG系统...")
    rag.initialize()

    # 获取统计信息
    print("\n2. 系统统计:")
    stats = rag.get_stats()
    for key, value in stats.items():
        print(f"   {key}: {value}")

    # 测试查询
    queries = [
        "什么是RAG系统？",
        "RAG系统有什么优势？",
        "机器学习有哪些类型？",
        "AI是什么？"
    ]

    print("\n3. 测试查询:")
    for i, query in enumerate(queries, 1):
        print(f"\n查询 {i}: {query}")
        result = rag.query(query)

        print(f"答案: {result['answer']}")

        if result['sources']:
            print(f"\n找到 {len(result['sources'])} 个相关文档:")
            for j, source in enumerate(result['sources'], 1):
                print(f"\n  文档 {j}:")
                print(f"    来源: {source['metadata'].get('filename', '未知')}")
                print(f"    相似度: {source['score']:.3f}")
                print(f"    内容: {source['content'][:150]}...")

    print("\n" + "=" * 60)
    print("测试完成！")
    print("\n说明:")
    print("- 此版本使用简单的TF-IDF向量模拟嵌入")
    print("- 不需要网络连接或API密钥")
    print("- 适合测试RAG系统的基本流程")

if __name__ == "__main__":
    main()