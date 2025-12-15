#!/usr/bin/env python3
"""
向量存储操作模块
封装Chroma向量数据库的创建和查询功能
"""

import time
from typing import List, Dict, Any, Optional
from pathlib import Path

try:
    from langchain_core.documents import Document
except ImportError:
    from langchain.schema import Document

from langchain_community.vectorstores import Chroma


class VectorStoreManager:
    """向量存储管理器"""

    def __init__(self, persist_directory: str = "./test_sentence_transformers_store"):
        """
        初始化向量存储管理器

        Args:
            persist_directory: 向量数据库持久化目录
        """
        self.persist_directory = persist_directory
        self.vector_store = None

    def create_vector_store(
        self,
        documents: List[Document],
        embeddings,
        persist_directory: Optional[str] = None,
        vector_store_type: str = "chroma"
    ):
        """
        创建向量存储

        Args:
            documents: 文档列表
            embeddings: 嵌入模型实例
            persist_directory: 可选的持久化目录
            vector_store_type: 向量存储类型（目前支持chroma）

        Returns:
            向量存储实例
        """
        if persist_directory:
            self.persist_directory = persist_directory

        print(f"创建向量存储 (类型: {vector_store_type})...")

        start_time = time.time()

        # 根据类型创建不同的向量存储
        if vector_store_type.lower() == "chroma":
            self.vector_store = Chroma.from_documents(
                documents=documents,
                embedding=embeddings,
                persist_directory=self.persist_directory
            )
        else:
            print(f"[警告] 不支持的向量存储类型: {vector_store_type}，使用默认的chroma")
            self.vector_store = Chroma.from_documents(
                documents=documents,
                embedding=embeddings,
                persist_directory=self.persist_directory
            )

        end_time = time.time()

        print(f"向量存储创建成功！耗时: {end_time - start_time:.2f}秒")
        return self.vector_store

    def load_existing_store(self, embeddings) -> Chroma:
        """
        加载已存在的向量存储

        Args:
            embeddings: 嵌入模型实例

        Returns:
            Chroma向量存储实例
        """
        if not Path(self.persist_directory).exists():
            raise FileNotFoundError(f"向量存储目录不存在: {self.persist_directory}")

        self.vector_store = Chroma(
            persist_directory=self.persist_directory,
            embedding_function=embeddings
        )
        print(f"已加载向量存储: {self.persist_directory}")
        return self.vector_store

    def similarity_search(
        self,
        query: str,
        k: int = 2,
        embeddings=None
    ) -> List[Document]:
        """
        相似度搜索

        Args:
            query: 查询文本
            k: 返回的文档数量
            embeddings: 嵌入模型实例（如果需要创建新的向量存储）

        Returns:
            相关文档列表
        """
        if self.vector_store is None:
            raise ValueError("向量存储未初始化，请先创建或加载向量存储")

        print(f"查询: {query}")

        start_time = time.time()
        results = self.vector_store.similarity_search(query, k=k)
        end_time = time.time()

        print(f"搜索耗时: {end_time - start_time:.3f}秒")
        print(f"找到 {len(results)} 个相关文档")

        return results

    def search_with_scores(
        self,
        query: str,
        k: int = 2
    ) -> List[tuple]:
        """
        带相似度分数的搜索

        Args:
            query: 查询文本
            k: 返回的文档数量

        Returns:
            (文档, 相似度分数) 元组列表
        """
        if self.vector_store is None:
            raise ValueError("向量存储未初始化")

        return self.vector_store.similarity_search_with_score(query, k=k)

    def print_search_results(self, results: List[Document], query: str = ""):
        """
        打印搜索结果

        Args:
            results: 搜索结果列表
            query: 查询文本（可选）
        """
        if query:
            print(f"\n查询: {query}")

        for i, doc in enumerate(results, 1):
            print(f"\n文档 {i}:")
            print(f"  来源: {doc.metadata.get('source', '未知')}")
            print(f"  内容: {doc.page_content[:100]}...")

    def get_store_info(self) -> Dict[str, Any]:
        """
        获取向量存储信息

        Returns:
            包含存储信息的字典
        """
        if self.vector_store is None:
            return {"error": "向量存储未初始化"}

        try:
            # 获取集合信息
            collection = self.vector_store._collection
            count = collection.count()

            return {
                "persist_directory": self.persist_directory,
                "document_count": count,
                "collection_name": collection.name
            }
        except Exception as e:
            return {"error": str(e)}

    def clear_store(self):
        """清除向量存储"""
        if self.vector_store is not None:
            self.vector_store.delete_all()
            print("向量存储已清空")

    def delete_store(self):
        """删除向量存储目录"""
        import shutil
        if Path(self.persist_directory).exists():
            shutil.rmtree(self.persist_directory)
            print(f"已删除向量存储目录: {self.persist_directory}")
        self.vector_store = None