#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
测试 DeepSeek API 配置
"""
import asyncio
import os
import sys
from config.settings import settings
from langchain_openai import ChatOpenAI
from langchain_core.messages import HumanMessage

# 设置输出编码
if sys.platform == "win32":
    import codecs
    sys.stdout = codecs.getwriter("utf-8")(sys.stdout.detach())


async def test_deepseek_config():
    """测试 DeepSeek 配置"""
    print("=== DeepSeek API 配置测试 ===\n")

    # 1. 验证配置
    print("1. 验证环境配置...")
    if not settings.validate_config():
        print("[ERROR] 配置验证失败")
        return False

    # 2. 获取 LLM 配置
    print("\n2. LLM 配置信息...")
    llm_config = settings.get_llm_config()
    print(f"模型: {llm_config.get('model', 'Unknown')}")
    print(f"API Base: {llm_config.get('openai_api_base', 'Default OpenAI')}")
    print(f"Temperature: {llm_config.get('temperature', 'Unknown')}")
    print(f"Max Tokens: {llm_config.get('max_tokens', 'Unknown')}")

    # 3. 测试 API 连接
    print("\n3. 测试 API 连接...")
    try:
        llm = ChatOpenAI(**llm_config)

        # 发送简单的测试消息
        test_message = HumanMessage(content="你好，请用一句话介绍一下你自己。")
        response = await llm.ainvoke([test_message])

        print(f"[SUCCESS] API 连接成功!")
        print(f"模型回复: {response.content}")
        return True

    except Exception as e:
        print(f"[ERROR] API 连接失败: {str(e)}")
        return False


async def test_learning():
    print("=== 测试 DeepSeek Learning ===")

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

        return True

    except Exception as e:
        print(f"\n✗ DeepSeek Learning 测试失败: {str(e)}")
        return False

async def main():
    """主函数"""
    success = await test_deepseek_config()
    success = await test_learning()

    print(f"\n=== 测试结果 ===")
    if success:
        print("[SUCCESS] DeepSeek API 配置正常，可以使用!")
    else:
        print("[ERROR] DeepSeek API 配置存在问题，请检查配置")

        print("\n[HELP] 故障排除建议:")
        print("1. 检查 .env 文件中的 DEEPSEEK_API_KEY 是否正确")
        print("2. 确认 USE_DEEPSEEK=True")
        print("3. 检查网络连接")
        print("4. 验证 DeepSeek API 密钥是否有效")


if __name__ == "__main__":
    asyncio.run(main())
    