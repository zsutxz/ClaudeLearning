"""
RAG系统核心实现（最终修复版）
支持多种向量数据库、嵌入模型和检索策略
"""

import os
import json
import logging
from typing import List, Dict, Any, Optional, Union
from dataclasses import dataclass
from pathlib import Path

# LangChain imports - 使用正确的导入路径
from langchain_community.document_loaders import (
    DirectoryLoader,
    TextLoader,
    PyPDFLoader,
    CSVLoader,
    UnstructuredMarkdownLoader,
    UnstructuredWordDocumentLoader
)
from langchain_text_splitters import (
    RecursiveCharacterTextSplitter,
    TokenTextSplitter
)
from langchain_community.embeddings import (
    OpenAIEmbeddings,
    HuggingFaceEmbeddings,
    HuggingFaceInstructEmbeddings
)
from langchain_community.vectorstores import (
    Chroma,
    FAISS
)
from langchain.chains import RetrievalQA, ConversationalRetrievalChain
from langchain_community.chat_models import ChatOpenAI
from langchain_community.llms import OpenAI
from langchain.memory import ConversationBufferMemory
from langchain.prompts import PromptTemplate

# 配置日志
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)


@dataclass
class RAGConfig:
    """RAG系统配置类"""
    # 数据路径
    data_path: str = "./data"
    persist_path: str = "./vector_store"

    # 向量数据库配置
    vector_store_type: str = "chroma"  # chroma, faiss
    collection_name: str = "rag_collection"

    # 嵌入模型配置
    embedding_model: str = "openai"  # openai, huggingface, instruct
    embedding_model_name: str = "text-embedding-ada-002"

    # 文档分割配置
    chunk_size: int = 1000
    chunk_overlap: int = 200
    chunk_strategy: str = "recursive"  # recursive, token

    # 检索配置
    retrieval_strategy: str = "semantic"  # semantic, simple
    top_k: int = 5
    rerank: bool = True

    # LLM配置
    llm_model: str = "gpt-3.5-turbo"
    llm_temperature: float = 0.1
    max_tokens: int = 1000

    # API密钥
    openai_api_key: Optional[str] = None


class DocumentProcessor:
    """文档处理器"""

    def __init__(self, config: RAGConfig):
        self.config = config
        self.text_splitter = self._get_text_splitter()

    def _get_text_splitter(self):
        """获取文本分割器"""
        if self.config.chunk_strategy == "recursive":
            return RecursiveCharacterTextSplitter(
                chunk_size=self.config.chunk_size,
                chunk_overlap=self.config.chunk_overlap,
                length_function=len,
                separators=["\n\n", "\n", " ", ""]
            )
        elif self.config.chunk_strategy == "token":
            return TokenTextSplitter(
                chunk_size=self.config.chunk_size,
                chunk_overlap=self.config.chunk_overlap
            )
        else:
            raise ValueError(f"不支持的分割策略: {self.config.chunk_strategy}")

    def _get_embeddings(self):
        """获取嵌入模型"""
        if self.config.embedding_model == "openai":
            return OpenAIEmbeddings(
                openai_api_key=self.config.openai_api_key,
                model=self.config.embedding_model_name
            )
        elif self.config.embedding_model == "huggingface":
            return HuggingFaceEmbeddings(
                model_name=self.config.embedding_model_name,
                model_kwargs={'device': 'cpu'}
            )
        elif self.config.embedding_model == "instruct":
            return HuggingFaceInstructEmbeddings(
                model_name=self.config.embedding_model_name,
                model_kwargs={'device': 'cpu'}
            )
        else:
            raise ValueError(f"不支持的嵌入模型: {self.config.embedding_model}")

    def load_documents(self, path: str, file_types: List[str] = None) -> List:
        """加载文档"""
        if file_types is None:
            file_types = ["**/*.txt", "**/*.md", "**/*.pdf", "**/*.docx", "**/*.csv"]

        documents = []
        for file_type in file_types:
            try:
                if file_type.endswith(".pdf"):
                    loader = DirectoryLoader(
                        path,
                        glob=file_type,
                        loader_cls=PyPDFLoader,
                        recursive=True
                    )
                elif file_type.endswith(".docx"):
                    loader = DirectoryLoader(
                        path,
                        glob=file_type,
                        loader_cls=UnstructuredWordDocumentLoader,
                        recursive=True
                    )
                elif file_type.endswith(".md"):
                    loader = DirectoryLoader(
                        path,
                        glob=file_type,
                        loader_cls=UnstructuredMarkdownLoader,
                        recursive=True
                    )
                elif file_type.endswith(".csv"):
                    loader = DirectoryLoader(
                        path,
                        glob=file_type,
                        loader_cls=CSVLoader,
                        recursive=True
                    )
                else:
                    loader = DirectoryLoader(
                        path,
                        glob=file_type,
                        loader_cls=TextLoader,
                        recursive=True
                    )

                docs = loader.load()
                documents.extend(docs)
                logger.info(f"从 {file_type} 加载了 {len(docs)} 个文档")

            except Exception as e:
                logger.error(f"加载 {file_type} 文件时出错: {str(e)}")
                continue

        return documents

    def process_documents(self, documents: List) -> List:
        """处理文档（分割和元数据增强）"""
        if not documents:
            raise ValueError("没有找到文档")

        # 分割文档
        chunks = self.text_splitter.split_documents(documents)

        # 增强元数据
        for i, chunk in enumerate(chunks):
            chunk.metadata.update({
                "chunk_id": i,
                "chunk_size": len(chunk.page_content),
                "source_file": Path(chunk.metadata.get("source", "")).name,
                "timestamp": str(Path(chunk.metadata.get("source", "")).stat().st_mtime)
                if chunk.metadata.get("source") else ""
            })

        logger.info(f"文档处理完成，共 {len(chunks)} 个块")
        return chunks


class VectorStoreManager:
    """向量存储管理器"""

    def __init__(self, config: RAGConfig):
        self.config = config
        self.embeddings = self._get_embeddings()
        self.vector_store = None

    def _get_embeddings(self):
        """获取嵌入模型"""
        if self.config.embedding_model == "openai":
            return OpenAIEmbeddings(
                openai_api_key=self.config.openai_api_key,
                model=self.config.embedding_model_name
            )
        elif self.config.embedding_model == "huggingface":
            return HuggingFaceEmbeddings(
                model_name=self.config.embedding_model_name,
                model_kwargs={'device': 'cpu'}
            )
        elif self.config.embedding_model == "instruct":
            return HuggingFaceInstructEmbeddings(
                model_name=self.config.embedding_model_name,
                model_kwargs={'device': 'cpu'}
            )
        else:
            raise ValueError(f"不支持的嵌入模型: {self.config.embedding_model}")

    def create_vector_store(self, chunks: List) -> Any:
        """创建向量存储"""
        if self.config.vector_store_type == "chroma":
            self.vector_store = Chroma.from_documents(
                documents=chunks,
                embedding=self.embeddings,
                persist_directory=self.config.persist_path,
                collection_name=self.config.collection_name
            )
        elif self.config.vector_store_type == "faiss":
            self.vector_store = FAISS.from_documents(
                documents=chunks,
                embedding=self.embeddings
            )
            # 保存FAISS索引
            self.vector_store.save_local(self.config.persist_path)
        else:
            raise ValueError(f"不支持的向量存储类型: {self.config.vector_store_type}")

        logger.info(f"向量存储创建完成，类型: {self.config.vector_store_type}")
        return self.vector_store

    def load_vector_store(self) -> Any:
        """加载已存在的向量存储"""
        if self.config.vector_store_type == "chroma":
            self.vector_store = Chroma(
                persist_directory=self.config.persist_path,
                embedding_function=self.embeddings,
                collection_name=self.config.collection_name
            )
        elif self.config.vector_store_type == "faiss":
            self.vector_store = FAISS.load_local(
                self.config.persist_path,
                self.embeddings
            )
        else:
            raise ValueError(f"向量存储类型 {self.config.vector_store_type} 不支持从磁盘加载")

        logger.info(f"向量存储加载完成")
        return self.vector_store


class RAGSystem:
    """RAG系统主类"""

    def __init__(self, config: RAGConfig):
        self.config = config
        self.document_processor = DocumentProcessor(config)
        self.vector_store_manager = VectorStoreManager(config)
        self.qa_chain = None
        self.memory = ConversationBufferMemory(
            memory_key="chat_history",
            return_messages=True
        )

    def index_documents(self, data_path: str = None, force_reindex: bool = False):
        """索引文档"""
        if data_path is None:
            data_path = self.config.data_path

        # 检查是否已存在索引
        if not force_reindex and os.path.exists(self.config.persist_path):
            logger.info("加载已存在的向量存储")
            self.vector_store_manager.load_vector_store()
        else:
            logger.info("创建新的向量存储")
            # 加载和处理文档
            documents = self.document_processor.load_documents(data_path)
            chunks = self.document_processor.process_documents(documents)

            # 创建向量存储
            self.vector_store_manager.create_vector_store(chunks)

        # 创建QA链
        self._create_qa_chain()

    def _create_qa_chain(self):
        """创建QA链"""
        # 只有在有OpenAI API密钥时才创建
        if self.config.openai_api_key:
            # 定义提示模板
            template = """使用以下上下文来回答问题。如果无法基于上下文回答，请说"我没有足够的信息来回答这个问题"。

上下文：
{context}

问题：{question}

答案："""

            QA_PROMPT = PromptTemplate(
                template=template,
                input_variables=["context", "question"]
            )

            # 创建LLM
            llm = ChatOpenAI(
                model_name=self.config.llm_model,
                temperature=self.config.llm_temperature,
                openai_api_key=self.config.openai_api_key
            )

            # 创建检索器
            retriever = self.vector_store_manager.vector_store.as_retriever(
                search_kwargs={"k": self.config.top_k}
            )

            # 创建QA链
            self.qa_chain = RetrievalQA.from_chain_type(
                llm=llm,
                chain_type="stuff",
                retriever=retriever,
                chain_type_kwargs={"prompt": QA_PROMPT},
                return_source_documents=True
            )
            logger.info("QA链创建完成")
        else:
            logger.info("未设置API密钥，仅支持文档检索")

    def query(self, question: str, return_source: bool = True) -> Dict[str, Any]:
        """查询RAG系统"""
        if self.qa_chain is None:
            # 如果没有QA链，只返回检索到的文档
            retriever = self.vector_store_manager.vector_store.as_retriever(
                search_kwargs={"k": self.config.top_k}
            )
            docs = retriever.get_relevant_documents(question)

            response = {
                "answer": "未设置API密钥，无法生成答案。但已检索到相关文档。",
                "question": question,
                "success": True,
                "sources": [
                    {
                        "content": doc.page_content[:200] + "...",
                        "metadata": doc.metadata
                    }
                    for doc in docs
                ] if return_source else []
            }
            return response

        try:
            result = self.qa_chain({"query": question})

            response = {
                "answer": result.get("result", ""),
                "question": question,
                "success": True
            }

            if return_source and "source_documents" in result:
                response["sources"] = [
                    {
                        "content": doc.page_content[:200] + "...",
                        "metadata": doc.metadata
                    }
                    for doc in result["source_documents"]
                ]

            return response

        except Exception as e:
            logger.error(f"查询时出错: {str(e)}")
            return {
                "answer": f"查询出错: {str(e)}",
                "question": question,
                "success": False
            }

    def get_stats(self) -> Dict[str, Any]:
        """获取系统统计信息"""
        stats = {
            "config": {
                "vector_store_type": self.config.vector_store_type,
                "embedding_model": self.config.embedding_model,
                "chunk_size": self.config.chunk_size,
                "retrieval_strategy": self.config.retrieval_strategy,
                "top_k": self.config.top_k
            }
        }

        # 如果向量存储存在，获取文档数量
        if self.vector_store_manager.vector_store:
            try:
                if self.config.vector_store_type == "chroma":
                    stats["indexed_documents"] = len(
                        self.vector_store_manager.vector_store.get()["metadatas"]
                    )
                else:
                    stats["indexed_documents"] = "未知（需要特定实现）"
            except:
                stats["indexed_documents"] = "无法获取"

        return stats


# 便捷函数
def create_rag_system(
    data_path: str,
    vector_store_type: str = "chroma",
    embedding_model: str = "openai",
    openai_api_key: str = None,
    **kwargs
) -> RAGSystem:
    """创建RAG系统的便捷函数"""
    config = RAGConfig(
        data_path=data_path,
        vector_store_type=vector_store_type,
        embedding_model=embedding_model,
        openai_api_key=openai_api_key or os.getenv("OPENAI_API_KEY"),
        **kwargs
    )

    rag = RAGSystem(config)
    rag.index_documents()

    return rag