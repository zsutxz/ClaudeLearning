#!/usr/bin/env python3
"""
快速测试DeepSeek API
"""

# 在这里设置你的DeepSeek API密钥
DEEPSEEK_API_KEY = "sk-943df85"

import os
from openai import OpenAI

def test_deepseek_api():
    """测试DeepSeek API连接"""

    # # 检查是否设置了API密钥
    # if DEEPSEEK_API_KEY == "在这里输入你的DeepSeek API密钥":
    #     print("请先在代码中设置你的DeepSeek API密钥！")
    #     print("获取API密钥: https://platform.deepseek.com/")
    #     return

    try:
        # 创建客户端
        client = OpenAI(
            api_key=DEEPSEEK_API_KEY,
            base_url="https://api.deepseek.com"
        )

        print("正在测试DeepSeek API连接...")
        print("-" * 40)

        # 简单的对话测试
        response = client.chat.completions.create(
            model="deepseek-chat",
            messages=[
                {"role": "user", "content": "你好！请简单介绍一下你自己。"}
            ],
            max_tokens=200
        )

        print("API连接成功！")
        reply = response.choices[0].message.content
        print("DeepSeek回复:", reply)
        print("-" * 40)

        # 代码生成测试
        code_response = client.chat.completions.create(
            model="deepseek-chat",
            messages=[
                {"role": "system", "content": "你是一个专业的Python编程助手。"},
                {"role": "user", "content": "请写一个简单的Python函数来计算两个数的和。"}
            ],
            max_tokens=300
        )

        print("代码生成测试:")
        code_reply = code_response.choices[0].message.content
        print(code_reply)
        print("-" * 40)

        print("所有测试通过！DeepSeek API工作正常。")

    except Exception as e:
        print(f"API测试失败: {str(e)}")
        print("\n可能的问题:")
        print("1. API密钥无效")
        print("2. 网络连接问题")
        print("3. API服务不可用")
        print("4. 余额不足")

if __name__ == "__main__":
    test_deepseek_api()