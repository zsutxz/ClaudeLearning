#!/usr/bin/env python3
"""
文档加载模块
提供从各种来源加载文档的功能
"""

import os
from typing import List, Optional, Dict, Any
from pathlib import Path

try:
    from langchain_core.documents import Document
except ImportError:
    from langchain.schema import Document

from langchain_community.document_loaders import (
    DirectoryLoader,
    TextLoader,
    PyPDFLoader,
    CSVLoader,
    UnstructuredMarkdownLoader,
    UnstructuredWordDocumentLoader
)


class DocumentLoader:
    """文档加载器"""

    def __init__(self, data_directory: str = "./data/sample_documents"):
        """
        初始化文档加载器

        Args:
            data_directory: 默认文档目录
        """
        self.data_directory = data_directory

    def load_text_documents(
        self,
        directory: Optional[str] = None,
        glob_pattern: str = "**/*.txt",
        encoding: str = 'utf-8'
    ) -> List[Document]:
        """
        加载文本文档

        Args:
            directory: 文档目录路径
            glob_pattern: 文件匹配模式
            encoding: 文件编码

        Returns:
            文档列表
        """
        if directory is None:
            directory = self.data_directory

        if not os.path.exists(directory):
            print(f"文档目录不存在: {directory}")
            return []

        print(f"从 {directory} 加载文本文档...")

        loader = DirectoryLoader(
            directory,
            glob=glob_pattern,
            loader_cls=TextLoader,
            loader_kwargs={'encoding': encoding}
        )
        documents = loader.load()
        print(f"加载了 {len(documents)} 个文档")

        return documents

    def load_pdf_documents(
        self,
        directory: Optional[str] = None,
        glob_pattern: str = "**/*.pdf"
    ) -> List[Document]:
        """
        加载PDF文档

        Args:
            directory: 文档目录路径
            glob_pattern: 文件匹配模式

        Returns:
            文档列表
        """
        if directory is None:
            directory = self.data_directory

        if not os.path.exists(directory):
            print(f"文档目录不存在: {directory}")
            return []

        print(f"从 {directory} 加载PDF文档...")

        loader = DirectoryLoader(
            directory,
            glob=glob_pattern,
            loader_cls=PyPDFLoader
        )
        documents = loader.load()
        print(f"加载了 {len(documents)} 个文档")

        return documents

    def load_markdown_documents(
        self,
        directory: Optional[str] = None,
        glob_pattern: str = "**/*.md"
    ) -> List[Document]:
        """
        加载Markdown文档

        Args:
            directory: 文档目录路径
            glob_pattern: 文件匹配模式

        Returns:
            文档列表
        """
        if directory is None:
            directory = self.data_directory

        if not os.path.exists(directory):
            print(f"文档目录不存在: {directory}")
            return []

        print(f"从 {directory} 加载Markdown文档...")

        loader = DirectoryLoader(
            directory,
            glob=glob_pattern,
            loader_cls=UnstructuredMarkdownLoader
        )
        documents = loader.load()
        print(f"加载了 {len(documents)} 个文档")

        return documents

    def load_word_documents(
        self,
        directory: Optional[str] = None,
        glob_pattern: str = "**/*.docx"
    ) -> List[Document]:
        """
        加载Word文档

        Args:
            directory: 文档目录路径
            glob_pattern: 文件匹配模式

        Returns:
            文档列表
        """
        if directory is None:
            directory = self.data_directory

        if not os.path.exists(directory):
            print(f"文档目录不存在: {directory}")
            return []

        print(f"从 {directory} 加载Word文档...")

        loader = DirectoryLoader(
            directory,
            glob=glob_pattern,
            loader_cls=UnstructuredWordDocumentLoader
        )
        documents = loader.load()
        print(f"加载了 {len(documents)} 个文档")

        return documents

    def load_csv_documents(
        self,
        directory: Optional[str] = None,
        glob_pattern: str = "**/*.csv"
    ) -> List[Document]:
        """
        加载CSV文档

        Args:
            directory: 文档目录路径
            glob_pattern: 文件匹配模式

        Returns:
            文档列表
        """
        if directory is None:
            directory = self.data_directory

        if not os.path.exists(directory):
            print(f"文档目录不存在: {directory}")
            return []

        print(f"从 {directory} 加载CSV文档...")

        loader = DirectoryLoader(
            directory,
            glob=glob_pattern,
            loader_cls=CSVLoader
        )
        documents = loader.load()
        print(f"加载了 {len(documents)} 个文档")

        return documents

    def load_all_documents(self, directory: Optional[str] = None) -> List[Document]:
        """
        加载所有支持类型的文档

        Args:
            directory: 文档目录路径

        Returns:
            所有文档列表
        """
        all_documents = []

        # 支持的文件类型和对应的加载方法
        file_types = [
            ("*.txt", self.load_text_documents),
            ("*.pdf", self.load_pdf_documents),
            ("*.md", self.load_markdown_documents),
            ("*.docx", self.load_word_documents),
            ("*.csv", self.load_csv_documents)
        ]

        for glob_pattern, loader_func in file_types:
            try:
                docs = loader_func(directory, glob_pattern)
                all_documents.extend(docs)
            except Exception as e:
                print(f"加载 {glob_pattern} 文件时出错: {str(e)}")

        print(f"总共加载了 {len(all_documents)} 个文档")
        return all_documents

    def create_test_documents(self) -> List[Document]:
        """
        创建测试文档

        Returns:
            测试文档列表
        """
        print("创建测试文档...")

        test_docs = [
            Document(
                page_content="RAG（Retrieval-Augmented Generation）是一种结合信息检索和文本生成的AI技术。",
                metadata={"source": "test1.txt"}
            ),
            Document(
                page_content="RAG系统可以从知识库中检索相关信息，然后基于这些信息生成答案。",
                metadata={"source": "test2.txt"}
            ),
            Document(
                page_content="Sentence-Transformers是一个优秀的Python库，用于生成高质量的文本嵌入向量。",
                metadata={"source": "test3.txt"}
            ),
            Document(
                page_content="向量数据库是RAG系统的重要组成部分，用于存储和检索文档的嵌入向量。",
                metadata={"source": "test4.txt"}
            ),
            Document(
                page_content="本地嵌入模型可以在不依赖外部API的情况下生成文本向量，保护数据隐私。",
                metadata={"source": "test5.txt"}
            )
        ]

        return test_docs

    def get_document_stats(self, documents: List[Document]) -> Dict[str, Any]:
        """
        获取文档统计信息

        Args:
            documents: 文档列表

        Returns:
            统计信息字典
        """
        if not documents:
            return {"error": "文档列表为空"}

        total_chars = sum(len(doc.page_content) for doc in documents)
        total_tokens = sum(len(doc.page_content.split()) for doc in documents)

        sources = [doc.metadata.get('source', '未知') for doc in documents]
        source_counts = {}
        for source in sources:
            source_counts[source] = source_counts.get(source, 0) + 1

        return {
            "document_count": len(documents),
            "total_characters": total_chars,
            "total_words": total_tokens,
            "avg_characters_per_doc": total_chars / len(documents),
            "avg_words_per_doc": total_tokens / len(documents),
            "unique_sources": len(set(sources)),
            "source_distribution": source_counts
        }