
import os

from MultiAIAgent import UniversalAIAgent,UniversalTaskAgent,UniversalCodeAgent


def main():
    """主函数 - 演示通用AI代理的使用"""
    print("通用AI代理SDK Python示例")
    print("=" * 50)

    try:
        # 1. 使用模拟模式（无需API密钥）
        print("\n模拟模式示例（无需API密钥）:")
        mock_agent = UniversalAIAgent(provider="mock")

        response = mock_agent.chat("你好！请简单介绍一下你自己。", stream=False)
        print("回复:", response)
        print("-" * 40)

        # 2. 任务型代理(claude)
        print("\n任务型代理示例（模拟模式）:")
        task_agent = UniversalTaskAgent(
            "帮助用户制定学习计划和提供学习建议",
            provider="claude",
            api_key=os.getenv('ANTHROPIC_API_KEY'),
            base_url=os.getenv('CLAUDE_BASE_URL', 'https://open.bigmodel.cn/api/anthropic')
        )

        plan = task_agent.solve_problem("我想学习人工智能，应该从哪里开始？")
        print("学习建议:", plan)
        print("-" * 40)
        
        # 3. 代码助手代理（模拟模式）
        print("\n代码助手代理示例（模拟模式）:")
        code_agent = UniversalCodeAgent(language="Python",       
                                        provider="deepseek",
                                        api_key=os.getenv('DEEPSEEK_API_KEY'),
                                        base_url=os.getenv('DEEPSEEK_BASE_URL', 'https://api.deepseek.com'))

        code_solution = code_agent.write_code("编写一个函数来计算斐波那契数列")
        print("代码解决方案:")
        print(code_solution)
        print("-" * 40)
    

        # 4. 代码审查示例（模拟模式）
        print("\n代码审查示例（模拟模式）:")
        buggy_code = """
def calculate_average(numbers):
    total = 0
    for num in numbers:
        total += num
    return total / len(numbers)
"""

        review = code_agent.review_code(buggy_code)
        print("代码审查结果:")
        print(review)
        print("-" * 40)

        # 5. 流式响应演示（模拟模式）
        print("\n流式响应演示（模拟模式）:")
        stream_response = code_agent.chat("请简单介绍深度学习的基本概念", stream=True)
        print("-" * 40)

        # 6. 演示如何使用其他提供商（如果有API密钥）
        print("\n其他提供商使用方法:")
        print("Claude API:")
        print("  claude_agent = UniversalAIAgent(provider='claude', model='claude-3-5-sonnet-20241022')")
        print("OpenAI API:")
        print("  openai_agent = UniversalAIAgent(provider='openai', model='gpt-3.5-turbo')")
        print("Ollama本地模型:")
        print("  ollama_agent = UniversalAIAgent(provider='ollama', model='llama2')")
        print("-" * 40)

        # 显示支持的模型
        print("\n支持的模型:")
        for provider, config in UniversalAIAgent.SUPPORTED_PROVIDERS.items():
            print(f"  {provider}: {', '.join(config['models'][:3])}{'...' if len(config['models']) > 3 else ''}")

        # 显示对话统计
        print(f"\n{mock_agent.get_conversation_summary()}")
        print(f"{task_agent.get_conversation_summary()}")
        print(f"{code_agent.get_conversation_summary()}")

    except Exception as e:
        print(f"运行示例时出错: {str(e)}")
        print("\n使用说明:")
        print("1. 模拟模式无需任何API密钥，可直接运行")
        print("2. 要使用真实API，请安装依赖并设置环境变量:")
        print("   pip install anthropic openai requests")
        print("   export ANTHROPIC_API_KEY='your-claude-key'")

        print("\n3. 获取API密钥:")
        print("   Claude: https://console.anthropic.com/")
        print("   OpenAI: https://platform.openai.com/")
        print("\n4. 本地模型（Ollama）:")
        print("   下载安装: https://ollama.ai/")
        print("   运行服务: ollama serve")


if __name__ == "__main__":
    main()