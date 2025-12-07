"""
简化的RAG系统实现
只使用基本功能，避免复杂的依赖
"""

import os
import json
import logging
from typing import List, Dict, Any, Optional
from dataclasses import dataclass
from pathlib import Path

# 基础导入
from langchain_community.document_loaders import (
    DirectoryLoader,
    TextLoader
)
from langchain_text_splitters import (
    RecursiveCharacterTextSplitter
)
from langchain_community.embeddings import (
    HuggingFaceEmbeddings
)
from langchain_community.vectorstores import Chroma

# 配置日志
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)


@dataclass
class SimpleRAGConfig:
    """简化的RAG配置"""
    data_path: str = "./data"
    persist_path: str = "./vector_store"
    embedding_model_name: str = "sentence-transformers/all-MiniLM-L6-v2"
    chunk_size: int = 1000
    chunk_overlap: int = 200
    top_k: int = 5


class SimpleRAG:
    """简化的RAG系统"""

    def __init__(self, config: SimpleRAGConfig):
        self.config = config
        self.embeddings = None
        self.vector_store = None

    def _create_embeddings(self):
        """创建嵌入模型"""
        self.embeddings = HuggingFaceEmbeddings(
            model_name=self.config.embedding_model_name,
            model_kwargs={'device': 'cpu'}
        )
        logger.info(f"嵌入模型创建完成: {self.config.embedding_model_name}")

    def _load_documents(self):
        """加载文档"""
        loader = DirectoryLoader(
            self.config.data_path,
            glob="**/*.txt",
            loader_cls=TextLoader,
            recursive=True
        )
        documents = loader.load()
        logger.info(f"加载了 {len(documents)} 个文档")
        return documents

    def _split_documents(self, documents):
        """分割文档"""
        text_splitter = RecursiveCharacterTextSplitter(
            chunk_size=self.config.chunk_size,
            chunk_overlap=self.config.chunk_overlap,
            length_function=len,
            separators=["\n\n", "\n", " ", ""]
        )
        chunks = text_splitter.split_documents(documents)
        logger.info(f"分割成 {len(chunks)} 个文档块")
        return chunks

    def index_documents(self, force_reindex: bool = False):
        """索引文档"""
        if not force_reindex and os.path.exists(self.config.persist_path):
            logger.info("加载已存在的向量存储")
            self.vector_store = Chroma(
                persist_directory=self.config.persist_path,
                embedding_function=self.embeddings
            )
        else:
            logger.info("创建新的向量存储")

            # 创建嵌入模型
            if self.embeddings is None:
                self._create_embeddings()

            # 加载和处理文档
            documents = self._load_documents()
            chunks = self._split_documents(documents)

            # 创建向量存储
            self.vector_store = Chroma.from_documents(
                documents=chunks,
                embedding=self.embeddings,
                persist_directory=self.config.persist_path
            )

            logger.info("向量存储创建完成")

    def search(self, query: str, k: int = None):
        """搜索相关文档"""
        if self.vector_store is None:
            self.index_documents()

        if k is None:
            k = self.config.top_k

        # 搜索
        docs = self.vector_store.similarity_search(query, k=k)

        # 格式化结果
        results = []
        for doc in docs:
            results.append({
                "content": doc.page_content,
                "metadata": doc.metadata
            })

        return results

    def get_stats(self):
        """获取统计信息"""
        stats = {
            "embedding_model": self.config.embedding_model_name,
            "chunk_size": self.config.chunk_size,
            "top_k": self.config.top_k
        }

        if self.vector_store:
            try:
                collection = self.vector_store.get()
                stats["indexed_documents"] = len(collection["ids"])
            except:
                stats["indexed_documents"] = "无法获取"

        return stats


def main():
    """测试简化RAG系统"""
    print("=" * 50)
    print("简化RAG系统测试")
    print("=" * 50)

    # 创建配置
    config = SimpleRAGConfig(
        data_path="./data/sample_documents",
        persist_path="./vector_store"
    )

    # 创建RAG系统
    rag = SimpleRAG(config)

    # 索引文档
    print("\n1. 索引文档...")
    rag.index_documents()

    # 获取统计信息
    print("\n2. 系统统计:")
    stats = rag.get_stats()
    for key, value in stats.items():
        print(f"   {key}: {value}")

    # 测试搜索
    queries = [
        "什么是RAG系统？",
        "机器学习有哪些类型？",
        "RAG系统的优势是什么？"
    ]

    print("\n3. 测试文档检索:")
    for i, query in enumerate(queries, 1):
        print(f"\n查询 {i}: {query}")
        results = rag.search(query)

        print(f"找到 {len(results)} 个相关文档:")
        for j, result in enumerate(results, 1):
            print(f"\n  文档 {j}:")
            print(f"    来源: {result['metadata'].get('source', '未知')}")
            print(f"    内容: {result['content'][:150]}...")

    print("\n" + "=" * 50)
    print("测试完成！")
    print("\n说明:")
    print("- 此版本仅演示文档检索功能")
    print("- 要获得完整的问答功能，需要配置OpenAI API密钥")
    print("- RAG系统已成功索引文档并能进行语义搜索")


if __name__ == "__main__":
    main()