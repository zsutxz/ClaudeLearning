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
 
async def test_skill():
    # 获取项目根目录中的skills目录
    skills_dir = Path(__file__).parent.parent.parent / ".claude" / "skills"

    options = ClaudeAgentOptions(
        cwd=str(skills_dir),  # Set the working directory to where skills are located
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