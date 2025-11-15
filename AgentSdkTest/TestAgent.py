import asyncio
from claude_agent_sdk import query, ClaudeAgentOptions
import os
from typing import Any

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
if not os.getenv('ANTHROPIC_API_KEY'):
    raise ValueError("请设置ANTHROPIC_API_KEY环境变量或在.env文件中配置")

def create_security_agent(security_level: str) -> dict:
    """创建安全代理配置"""
    return {
        'description': '安全代码审查员',
        'prompt': f"您是一位{'严格' if security_level == 'strict' else '平衡'}的安全审查员...",
        'tools': ['Read', 'Grep', 'Glob'],
        # 'model': 'opus' if security_level == 'strict' else 'sonnet'
    }


async def test_agents():
    """测试多代理功能 - 简化版本，使用工具而非代理"""
    try:
        # 使用简单的工具配置来测试代理功能
        options = ClaudeAgentOptions(
            system_prompt="您是一位代码审查专家，具有安全、性能和最佳实践方面的专业知识。审查代码时：识别安全漏洞、检查性能问题、验证编码标准的遵守情况、建议具体改进。",
            allowed_tools=["Read", "Grep", "Glob", "Bash"],
            max_turns=2
        )

        async for message in query(
            prompt="审查当前目录中的Python文件，识别安全问题和性能问题",
            options=options
        ):
            message_type = type(message).__name__
            print(f"Message type: {message_type}")

            if message_type == "ResultMessage":
                print(f"Result: {message.result}")
            elif message_type == "AssistantMessage":
                for block in message.content:
                    if hasattr(block, 'text'):
                        print(f"Assistant: {block.text}")
                    else:
                        print(f"Assistant: {block}")
            elif message_type == "SystemMessage":
                print(f"System: {getattr(message, 'content', str(message))}")
            elif message_type == "UserMessage":
                print(f"User: {getattr(message, 'content', str(message))}")
            else:
                print(f"Other: {str(message)}")

    except Exception as e:
        print(f"Error during agent query: {e}")
        if "cancel scope" in str(e) or "Event loop is closed" in str(e):
            print("Ignoring known library cleanup issues")
        else:
            raise


if __name__ == "__main__":
    import sys

    print("运行代理测试...")
    asyncio.run(test_agents())

