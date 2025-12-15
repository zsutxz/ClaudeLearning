#!/usr/bin/env python3
"""
完整的RAG系统实现 - 集成检索和生成
"""

import time
from typing import List, Dict, Optional, Union
from dataclasses import dataclass

from embeddings.sentence_transformers_embeddings import SentenceTransformersEmbeddings
from core.document_loader import DocumentLoader
from core.vector_store import VectorStoreManager
from llm.deepseek_llm import DeepSeekLLM


@dataclass
class RAGConfig:
    """RAG系统配置"""
    # 嵌入模型配置
    embedding_model_name: str = "paraphrase-multilingual-MiniLM-L12-v2"

    # 向量存储配置
    vector_store_type: str = "chroma"  # chroma, faiss

    # LLM配置
    llm_provider: str = "deepseek"
    llm_model_name: str = "deepseek-chat"

    # 检索配置
    retrieval_k: int = 3  # 检索的文档数量
    similarity_threshold: float = 0.5  # 相似度阈值

    # 生成配置
    max_tokens: int = 1000
    temperature: float = 0.7


@dataclass
class RAGResult:
    """RAG查询结果"""
    query: str
    answer: str
    sources: List[Dict]
    retrieval_time: float
    generation_time: float
    total_time: float
    used_context: bool


class CompleteRAGSystem:
    """完整的RAG系统"""

    def __init__(self, config: RAGConfig = None):
        """
        初始化RAG系统

        Args:
            config: RAG配置
        """
        self.config = config or RAGConfig()

        # 初始化组件
        self.embeddings = None
        self.vector_store_manager = None
        self.vector_store = None
        self.llm = None
        self.document_loader = None

        self._initialize_components()

    def _initialize_components(self):
        """初始化各个组件"""
        print("="*60)
        print("初始化RAG系统组件...")
        print("="*60)

        # 1. 初始化嵌入模型
        print("\n1. 初始化嵌入模型...")
        self.embeddings = SentenceTransformersEmbeddings(
            model_name=self.config.embedding_model_name
        )

        # 2. 初始化文档加载器
        print("\n2. 初始化文档加载器...")
        self.document_loader = DocumentLoader()

        # 3. 初始化向量存储管理器
        print("\n3. 初始化向量存储...")
        self.vector_store_manager = VectorStoreManager()

        # 4. 初始化LLM
        print("\n4. 初始化LLM...")
        if self.config.llm_provider.lower() == "deepseek":
            self.llm = DeepSeekLLM(model_name=self.config.llm_model_name)
            # 测试连接
            if not self.llm.test_connection():
                print("[警告] LLM连接测试失败，将只进行检索")
                self.llm = None
        else:
            print(f"[警告] 不支持的LLM提供商: {self.config.llm_provider}")
            self.llm = None

        # 5. 加载文档并创建向量存储
        self._load_documents_and_create_store()

        print("\n[完成] RAG系统初始化完成！")

    def _load_documents_and_create_store(self):
        """加载文档并创建向量存储"""
        print("\n5. 加载文档并创建向量存储...")

        # 加载文档
        documents = self.document_loader.load_text_documents()

        if not documents:
            print("[信息] 未找到文档，创建测试文档...")
            documents = self.document_loader.create_test_documents()

        # 创建向量存储
        self.vector_store = self.vector_store_manager.create_vector_store(
            documents=documents,
            embeddings=self.embeddings,
            vector_store_type=self.config.vector_store_type
        )

        print(f"[完成] 已加载 {len(documents)} 个文档")

    def query(self, question: str, k: int = None) -> RAGResult:
        """
        执行RAG查询

        Args:
            question: 用户问题
            k: 检索的文档数量

        Returns:
            RAG查询结果
        """
        if k is None:
            k = self.config.retrieval_k

        start_time = time.time()

        print(f"\n{'='*60}")
        print(f"RAG查询: {question}")
        print(f"{'='*60}")

        # 1. 检索相关文档
        print("\n[步骤1] 检索相关文档...")
        retrieval_start = time.time()

        # 使用带相似度分数的搜索
        retrieved_docs_with_scores = self.vector_store_manager.search_with_scores(
            question, k=k
        )

        retrieval_end = time.time()
        retrieval_time = retrieval_end - retrieval_start

        print(f"[完成] 检索到 {len(retrieved_docs_with_scores)} 个相关文档 (耗时: {retrieval_time:.2f}秒)")

        # 2. 生成答案
        answer = "LLM未配置，无法生成答案"
        generation_time = 0
        used_context = False

        if self.llm and retrieved_docs_with_scores:
            print("\n[步骤2] 生成答案...")
            generation_start = time.time()

            # 构建上下文
            context = "\n\n".join([
                f"文档片段{i+1}:\n{doc.page_content}"
                for i, (doc, score) in enumerate(retrieved_docs_with_scores)
            ])

            # 生成答案
            answer = self.llm.generate_rag_response(
                question, context,
                max_tokens=self.config.max_tokens,
                temperature=self.config.temperature
            )

            generation_end = time.time()
            generation_time = generation_end - generation_start
            used_context = True

            print(f"[完成] 答案生成完成 (耗时: {generation_time:.2f}秒)")
        elif not self.llm:
            print("[跳过] LLM未配置，仅返回检索结果")
        else:
            print("[跳过] 未检索到相关文档")

        # 3. 计算总时间
        total_time = time.time() - start_time

        # 4. 创建结果对象
        result = RAGResult(
            query=question,
            answer=answer,
            sources=[{
                'content': doc.page_content,
                'similarity': score,
                'metadata': doc.metadata
            } for doc, score in retrieved_docs_with_scores],
            retrieval_time=retrieval_time,
            generation_time=generation_time,
            total_time=total_time,
            used_context=used_context
        )

        return result

    def print_result(self, result: RAGResult):
        """
        打印RAG查询结果

        Args:
            result: RAG查询结果
        """
        print(f"\n{'='*60}")
        print("查询结果")
        print(f"{'='*60}")

        print(f"\n📝 问题: {result.query}")

        if result.used_context:
            print(f"\n💬 答案: {result.answer}")

        print(f"\n📚 参考文档 (共{len(result.sources)}个):")
        for i, source in enumerate(result.sources, 1):
            print(f"\n  [文档{i}] 相似度: {source['similarity']:.3f}")
            print(f"  内容: {source['content'][:100]}...")

        print(f"\n⏱️ 性能统计:")
        print(f"  - 检索时间: {result.retrieval_time:.2f}秒")
        print(f"  - 生成时间: {result.generation_time:.2f}秒")
        print(f"  - 总时间: {result.total_time:.2f}秒")
        print(f"{'='*60}")

    def batch_query(self, questions: List[str]) -> List[RAGResult]:
        """
        批量查询

        Args:
            questions: 问题列表

        Returns:
            RAG查询结果列表
        """
        print(f"\n{'='*60}")
        print(f"批量RAG查询 ({len(questions)}个问题)")
        print(f"{'='*60}")

        results = []
        for i, question in enumerate(questions, 1):
            print(f"\n处理第{i}/{len(questions)}个问题...")
            result = self.query(question)
            results.append(result)

        return results

    def get_stats(self) -> Dict[str, Union[int, str]]:
        """
        获取系统统计信息

        Returns:
            统计信息字典
        """
        stats = {
            "embedding_model": self.config.embedding_model_name,
            "vector_store_type": self.config.vector_store_type,
            "llm_provider": self.config.llm_provider if self.llm else "未配置",
            "llm_model": self.config.llm_model_name if self.llm else "未配置",
            "retrieval_k": self.config.retrieval_k
        }

        return stats