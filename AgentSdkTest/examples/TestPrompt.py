
import os
import sys
from pathlib import Path

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))

# 加载.env文件中的环境变量
try:
    from dotenv import load_dotenv
    # 尝试从config目录加载.env文件
    env_file = project_root / "config" / ".env"
    if env_file.exists():
        load_dotenv(env_file)
    else:
        load_dotenv()
except ImportError:
    # 如果没有python-dotenv，手动读取.env文件
    env_paths = [
        project_root / "config" / ".env",
        project_root / ".env",
    ]
    for env_file in env_paths:
        if env_file.exists():
            with open(env_file, 'r') as f:
                for line in f:
                    if '=' in line and not line.strip().startswith('#'):
                        key, value = line.strip().split('=', 1)
                        os.environ[key] = value
            break

from lib.multi_agent import UniversalAIAgent, UniversalTaskAgent, UniversalCodeAgent

def main():
    """主函数 - 演示通用AI代理的使用"""
    print("通用AI代理SDK Python示例")
    print("=" * 50)

    try:

        # 1. 显示支持的模型
        print("\n支持的模型:")
        for provider, config in UniversalAIAgent.SUPPORTED_PROVIDERS.items():
            print(f"  {provider}: {', '.join(config['models'][:3])}{'...' if len(config['models']) > 3 else ''}")


        # 2. 任务型代理(claude)
        print("\n任务型代理示例（模拟模式）:")
        task_agent = UniversalTaskAgent(
            "帮助用户制定学习计划和提供学习建议",
            provider="claude",
            api_key=os.getenv('ANTHROPIC_API_KEY'),
            base_url=os.getenv('ANTHROPIC_BASE_URL', 'https://open.bigmodel.cn/api/anthropic')
        )


        plan = task_agent.solve_problem("我想学习人工智能，应该从哪里开始？")
        print("学习建议:", plan)
        print("-" * 40)

        # 显示对话统计
        print(f"{task_agent.get_conversation_summary()}")

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