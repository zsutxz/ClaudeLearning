import asyncio
import os
from pathlib import Path
from typing import Any
from claude_agent_sdk import query, ClaudeAgentOptions

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

async def main():
    # 获取当前项目目录
    project_dir = Path(__file__).parent.parent.resolve()

    options = ClaudeAgentOptions(
        mcp_servers={
            "filesystem": {
                "command": "python",
                "args": ["-m", "mcp_server_filesystem"],
                "env": {
                    "ALLOWED_PATHS": str(project_dir)
                }
            }
        },
        allowed_tools=["mcp__filesystem__list_directory"],
        cwd=str(project_dir)
    )

    try:
        async for message in query(
            prompt="List files in my project",
            options=options
        ):
            # 处理不同类型的消息
            message_type = type(message).__name__
            print(f"Message type: {message_type}")

            if message_type == "ResultMessage":
                print(f"Result: {message.result}")
            elif message_type == "AssistantMessage":
                print(f"Assistant: {message.content}")
            elif message_type == "SystemMessage":
                print(f"System: {getattr(message, 'content', str(message))}")
            elif message_type == "UserMessage":
                print(f"User: {getattr(message, 'content', str(message))}")
            else:
                print(f"Other: {str(message)}")

    except Exception as e:
        print(f"Error during query: {e}")
        # 忽略取消作用域相关的错误，这些是库的内部问题
        if "cancel scope" in str(e) or "Event loop is closed" in str(e):
            print("Ignoring known library cleanup issues")
        else:
            raise

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("Program interrupted by user")
    except Exception as e:
        print(f"Program error: {e}")
        # 忽略库清理时的问题
        if "cancel scope" in str(e) or "Event loop is closed" in str(e) or "unclosed transport" in str(e):
            print("Ignoring known library cleanup issues")