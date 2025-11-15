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
    
async def main():
    options = ClaudeAgentOptions(
        setting_sources=["user", "project"],  # Load Skills from filesystem
        allowed_tools=["Skill"]  # Restricted toolset
    )

    async for message in query(
         prompt="What Skills are available?",
        options=options
    ):
        print(message)
    
async def test_skill():
    options = ClaudeAgentOptions(
    cwd="../.claude/skills/",  # Set the working directory to where skills are located
    setting_sources=["user", "project"],  # Load Skills from filesystem
    allowed_tools=["Skill", "Read", "Bash"]
    )   

    async for message in query(
        prompt="Extract text from invoice.pdf",
        options=options
    ):
        print(message)
        
if __name__ == "__main__":
    try:
        asyncio.run(test_skill())
    except KeyboardInterrupt:
        print("Program interrupted by user")
    except Exception as e:
        print(f"Program error: {e}")
        # 忽略库清理时的问题
        if "cancel scope" in str(e) or "Event loop is closed" in str(e) or "unclosed transport" in str(e):
            print("Ignoring known library cleanup issues")