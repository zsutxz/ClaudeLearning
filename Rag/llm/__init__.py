#!/usr/bin/env python3
"""
LLM模块 - 支持多种大语言模型
"""

from .deepseek_llm import DeepSeekLLM
from .base_llm import BaseLLM

__all__ = ['DeepSeekLLM', 'BaseLLM']