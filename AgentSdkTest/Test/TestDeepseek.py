#!/usr/bin/env python3
"""
快速测试DeepSeek API
"""


import os
from openai import OpenAI

# 加载.env文件中的环境变量
try:
    from dotenv import load_dotenv
    load_dotenv()
except ImportError:
    # 如果没有python-dotenv，手动读取.env文件
    env_file = '.env'
    if os.path.exists(env_file):
        with open(env_file, 'r') as f:
            for line in f:
                if '=' in line and not line.strip().startswith('#'):
                    key, value = line.strip().split('=', 1)
                    os.environ[key] = value

# 确保API密钥存在
if not os.getenv('DEEPSEEK_API_KEY'):
    raise ValueError("请设置DEEPSEEK_API_KEY环境变量或在.env文件中配置")

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
            api_key=os.getenv('DEEPSEEK_API_KEY'),
            base_url=os.getenv('DEEPSEEK_BASE_URL', 'https://api.deepseek.com')
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
        
        print("-" * 40)
        reply = response.choices[0].message.content
        print("DeepSeek回复:", reply)

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