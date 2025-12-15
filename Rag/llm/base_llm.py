#!/usr/bin/env python3
"""
基础LLM抽象类
"""

from abc import ABC, abstractmethod
from typing import List, Dict, Optional


class BaseLLM(ABC):
    """大语言模型基类"""

    def __init__(self, api_key: str, base_url: str = None, model_name: str = None):
        """
        初始化LLM

        Args:
            api_key: API密钥
            base_url: API基础URL
            model_name: 模型名称
        """
        self.api_key = api_key
        self.base_url = base_url
        self.model_name = model_name

    @abstractmethod
    def generate(self, prompt: str, max_tokens: int = 1000, temperature: float = 0.7) -> str:
        """
        生成文本

        Args:
            prompt: 输入提示
            max_tokens: 最大token数
            temperature: 温度参数，控制随机性

        Returns:
            生成的文本
        """
        pass

    @abstractmethod
    def chat(self, messages: List[Dict[str, str]], max_tokens: int = 1000, temperature: float = 0.7) -> str:
        """
        对话生成

        Args:
            messages: 消息列表，格式为 [{"role": "user", "content": "..."}]
            max_tokens: 最大token数
            temperature: 温度参数

        Returns:
            生成的回复
        """
        pass

    def create_rag_prompt(self, query: str, context: str) -> str:
        """
        创建RAG提示

        Args:
            query: 用户查询
            context: 检索到的上下文

        Returns:
            格式化的RAG提示
        """
        prompt = f"""基于以下信息回答问题。如果信息不足，请说明无法基于给定信息回答。

参考信息：
{context}

问题：{query}

请基于上述信息给出准确、详细的回答："""
        return prompt

    def generate_rag_response(self, query: str, context: str, max_tokens: int = 1000, temperature: float = 0.7) -> str:
        """
        生成RAG响应

        Args:
            query: 用户查询
            context: 检索到的上下文
            max_tokens: 最大token数
            temperature: 温度参数，控制随机性

        Returns:
            生成的答案
        """
        prompt = self.create_rag_prompt(query, context)
        return self.generate(prompt, max_tokens=max_tokens, temperature=temperature)