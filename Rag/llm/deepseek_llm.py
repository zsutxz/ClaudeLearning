#!/usr/bin/env python3
"""
DeepSeek LLM实现
"""

import os
import time
from typing import List, Dict, Optional
from openai import OpenAI

from .base_llm import BaseLLM


class DeepSeekLLM(BaseLLM):
    """DeepSeek大语言模型"""

    def __init__(
        self,
        api_key: str = None,
        base_url: str = "https://api.deepseek.com",
        model_name: str = "deepseek-chat"
    ):
        """
        初始化DeepSeek LLM

        Args:
            api_key: DeepSeek API密钥
            base_url: API基础URL
            model_name: 模型名称
        """
        # 从环境变量获取API密钥
        if api_key is None:
            api_key = os.getenv('DEEPSEEK_API_KEY')
            if not api_key:
                raise ValueError("请设置DEEPSEEK_API_KEY环境变量")

        super().__init__(api_key, base_url, model_name)

        # 初始化OpenAI客户端
        self.client = OpenAI(
            api_key=self.api_key,
            base_url=self.base_url
        )

    def generate(self, prompt: str, max_tokens: int = 1000, temperature: float = 0.7) -> str:
        """
        生成文本

        Args:
            prompt: 输入提示
            max_tokens: 最大token数
            temperature: 温度参数

        Returns:
            生成的文本
        """
        try:
            messages = [{"role": "user", "content": prompt}]
            return self.chat(messages, max_tokens, temperature)
        except Exception as e:
            print(f"生成文本时出错: {e}")
            return f"生成失败: {str(e)}"

    def chat(self, messages: List[Dict[str, str]], max_tokens: int = 1000, temperature: float = 0.7) -> str:
        """
        对话生成

        Args:
            messages: 消息列表
            max_tokens: 最大token数
            temperature: 温度参数

        Returns:
            生成的回复
        """
        try:
            start_time = time.time()

            response = self.client.chat.completions.create(
                model=self.model_name,
                messages=messages,
                max_tokens=max_tokens,
                temperature=temperature,
                stream=False
            )

            end_time = time.time()
            generation_time = end_time - start_time

            answer = response.choices[0].message.content

            # 打印统计信息
            print(f"[DeepSeek] 生成耗时: {generation_time:.2f}秒")
            if hasattr(response, 'usage') and response.usage:
                print(f"[DeepSeek] 输入tokens: {response.usage.prompt_tokens}")
                print(f"[DeepSeek] 输出tokens: {response.usage.completion_tokens}")
                print(f"[DeepSeek] 总计tokens: {response.usage.total_tokens}")

            return answer

        except Exception as e:
            print(f"DeepSeek API调用失败: {e}")
            return f"API调用失败: {str(e)}"

    def test_connection(self) -> bool:
        """
        测试API连接

        Returns:
            连接是否成功
        """
        try:
            print("测试DeepSeek API连接...")
            test_response = self.generate("请说'测试成功'", max_tokens=10)
            if "测试成功" in test_response or "成功" in test_response:
                print("[DeepSeek] API连接测试成功！")
                return True
            else:
                print(f"[DeepSeek] API响应异常: {test_response}")
                return False
        except Exception as e:
            print(f"[DeepSeek] API连接测试失败: {e}")
            return False

    def get_model_info(self) -> Dict[str, str]:
        """
        获取模型信息

        Returns:
            模型信息字典
        """
        return {
            "provider": "DeepSeek",
            "model_name": self.model_name,
            "base_url": self.base_url,
            "capabilities": "文本生成、对话、RAG"
        }