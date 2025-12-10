#!/usr/bin/env python3
"""
Sentence-Transformers 嵌入模型类
提供本地文本嵌入功能，支持多种预训练模型
"""

import os
import time
from typing import List, Union

# 设置 Hugging Face 镜像
os.environ['HF_ENDPOINT'] = 'https://hf-mirror.com'


class SentenceTransformersEmbeddings:
    """使用Sentence-Transformers的本地嵌入类"""

    def __init__(self, model_name: str = "paraphrase-multilingual-MiniLM-L12-v2"):
        """
        初始化嵌入模型

        推荐的多语言模型：
        - paraphrase-multilingual-MiniLM-L12-v2: 轻量级多语言模型
        - paraphrase-multilingual-mpnet-base-v2: 更好的质量但更大
        - shibing624/text2vec-base-chinese: 专门优化中文
        """
        self.model_name = model_name
        self.model = None
        self._load_model()

    def _load_model(self):
        """加载模型"""
        print(f"正在加载Sentence-Transformers模型: {self.model_name}")
        print("首次运行时会自动下载模型，请耐心等待...")

        from sentence_transformers import SentenceTransformer
        self.model = SentenceTransformer(self.model_name)
        print("模型加载完成！")

    def embed_query(self, text: str) -> List[float]:
        """
        生成单个文本的嵌入向量

        Args:
            text: 输入文本

        Returns:
            嵌入向量列表
        """
        embedding = self.model.encode(text, convert_to_tensor=False)
        # 确保返回的是列表格式，Chroma期望这个格式
        return embedding.tolist() if hasattr(embedding, 'tolist') else embedding

    def embed_documents(self, texts: List[str]) -> List[List[float]]:
        """
        批量生成文本的嵌入向量（更高效）

        Args:
            texts: 输入文本列表

        Returns:
            嵌入向量的列表
        """
        embeddings = self.model.encode(texts, convert_to_tensor=False, batch_size=32)
        # 确保返回的是列表的列表格式
        return [emb.tolist() if hasattr(emb, 'tolist') else emb for emb in embeddings]

    def __call__(self, text: Union[str, List[str]]) -> Union[List[float], List[List[float]]]:
        """
        兼容LangChain的调用方式

        Args:
            text: 输入文本或文本列表

        Returns:
            嵌入向量或嵌入向量列表
        """
        if isinstance(text, str):
            return self.embed_query(text)
        elif isinstance(text, list):
            return self.embed_documents(text)
        else:
            raise ValueError("输入必须是字符串或字符串列表")

    def test_embedding(self, test_text: str = "这是一个测试文本") -> dict:
        """
        测试嵌入功能

        Args:
            test_text: 测试文本

        Returns:
            包含测试结果的字典
        """
        print(f"测试文本: {test_text}")

        start_time = time.time()
        embedding = self.embed_query(test_text)
        end_time = time.time()

        return {
            "embedding_dimension": len(embedding),
            "embedding_time": end_time - start_time,
            "success": True
        }

    def test_batch_embedding(self, test_texts: List[str] = None) -> dict:
        """
        测试批量嵌入功能

        Args:
            test_texts: 测试文本列表

        Returns:
            包含测试结果的字典
        """
        if test_texts is None:
            test_texts = [
                "人工智能是计算机科学的一个分支",
                "机器学习是人工智能的子领域",
                "深度学习使用神经网络来模拟人脑"
            ]

        print(f"批量处理{len(test_texts)}个文本")

        start_time = time.time()
        batch_embeddings = self.embed_documents(test_texts)
        end_time = time.time()

        return {
            "batch_size": len(test_texts),
            "total_time": end_time - start_time,
            "avg_time_per_text": (end_time - start_time) / len(test_texts),
            "success": True
        }