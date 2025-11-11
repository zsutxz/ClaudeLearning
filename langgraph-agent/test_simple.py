#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
简单测试 DeepSeek API 是否正常工作
"""
import asyncio
from config.settings import settings
from langchain_openai import ChatOpenAI
from langchain_core.messages import HumanMessage


async def test_simple():
    """简单测试 LLM 是否工作"""
    print("=== 简单测试 DeepSeek LLM ===")

    try:
        # 获取配置
        llm_config = settings.get_llm_config()
        print(f"使用模型: {llm_config.get('model')}")
        print(f"API Base: {llm_config.get('openai_api_base', 'Default')}")

        # 初始化 LLM
        llm = ChatOpenAI(**llm_config)

        # 发送测试消息
        test_prompt = """
        请为Python初学者制定一个2小时的学习计划，包含以下内容：
        1. 学习目标（3个）
        2. 具体内容安排
        3. 实践练习

        请用中文回答，保持简洁实用。
        """

        messages = [HumanMessage(content=test_prompt)]
        print("\n正在调用 DeepSeek API...")
        response = await llm.ainvoke(messages)

        print("\n=== DeepSeek 回复 ===")
        print(response.content)
        print("\n✓ DeepSeek API 测试成功!")

        return True

    except Exception as e:
        print(f"\n✗ DeepSeek API 测试失败: {str(e)}")
        return False


if __name__ == "__main__":
    success = asyncio.run(test_simple())
    if success:
        print("\n🎉 DeepSeek API 配置完成且工作正常!")
    else:
        print("\n❌ DeepSeek API 配置存在问题")