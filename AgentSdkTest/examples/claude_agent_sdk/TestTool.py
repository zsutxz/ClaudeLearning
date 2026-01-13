#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
自定义工具示例

展示如何创建和使用自定义工具扩展 Claude Agent SDK 功能。

功能演示：
- 使用 @tool 装饰器创建自定义工具
- 创建 MCP 服务器并注册自定义工具
- 使用 ClaudeSDKClient 进行交互式对话
- 多工具协同使用
"""

import sys
import anyio
from pathlib import Path
from typing import Any

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent.parent.parent
sys.path.insert(0, str(project_root))

from claude_agent_sdk import (
    ClaudeSDKClient,
    ClaudeAgentOptions,
    query,
    tool,
    create_sdk_mcp_server,
    AssistantMessage,
    ResultMessage,    
    TextBlock,
)

from lib.config import get_config
from lib.utils import print_example_header


async def read_file_example():
    """示例 1: 读取文件"""
    print("\n📝 示例 1: 读取文件内容")
    print("-" * 40)

    options = ClaudeAgentOptions(
        allowed_tools=["Read"],
        max_turns=5,
    )

    message_stream = query(
        prompt="请读取 examples/01_basic_chat.py 文件的内容",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}", end="", flush=True)
        elif isinstance(message, ResultMessage):
            print()


async def write_file_example():
    """示例 2: 写入文件"""
    print("\n📝 示例 2: 创建新文件")
    print("-" * 40)

    options = ClaudeAgentOptions(
        allowed_tools=["Read", "Write"],
        max_turns=5,
    )

    message_stream = query(
        prompt="请在当前目录创建一个名为 test_output.txt 的文件，写入 'Hello from Claude Agent SDK!'",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}", end="", flush=True)
        elif isinstance(message, ResultMessage):
            print()


async def search_code_example():
    """示例 3: 搜索代码"""
    print("\n📝 示例 3: 搜索代码中的特定模式")
    print("-" * 40)

    options = ClaudeAgentOptions(
        allowed_tools=["Grep"],
        max_turns=5,
    )

    message_stream = query(
        prompt="请在 lib/ 目录中搜索所有包含 'UniversalAIAgent' 的文件",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}", end="", flush=True)
        elif isinstance(message, ResultMessage):
            print()
            
            

# ============================================================
# 自定义工具定义
# ============================================================

@tool(
    name="calculate",
    description="执行数学计算，支持基本运算符（+, -, *, /, **, % 等）",
    input_schema={
        "expression": str
    }
)
async def calculate(args: dict[str, Any]) -> dict[str, Any]:
    """
    数学计算工具

    Args:
        args: 包含 expression 字段的字典，值为数学表达式字符串

    Returns:
        包含计算结果的字典
    """
    try:
        # 安全地评估数学表达式
        expression = args["expression"]
        result = eval(expression, {"__builtins__": {}}, {})

        return {
            "content": [{
                "type": "text",
                "text": f"计算结果: {result}"
            }]
        }
    except ZeroDivisionError:
        return {
            "content": [{
                "type": "text",
                "text": "错误: 除数不能为零"
            }],
            "is_error": True
        }
    except Exception as e:
        return {
            "content": [{
                "type": "text",
                "text": f"计算错误: {str(e)}"
            }],
            "is_error": True
        }


@tool(
    name="get_time",
    description="获取当前日期和时间",
    input_schema={}
)
async def get_time(args: dict[str, Any]) -> dict[str, Any]:
    """
    获取当前时间工具

    Returns:
        包含格式化当前时间的字典
    """
    from datetime import datetime

    current_time = datetime.now()
    formatted_time = current_time.strftime("%Y-%m-%d %H:%M:%S")
    weekday = current_time.strftime("%A")

    return {
        "content": [{
            "type": "text",
            "text": f"当前时间: {formatted_time} ({weekday})"
        }]
    }


@tool(
    name="string_operations",
    description="执行字符串操作（转换大小写、反转、统计长度等）",
    input_schema={
        "text": str,
        "operation": str
    }
)
async def string_operations(args: dict[str, Any]) -> dict[str, Any]:
    """
    字符串操作工具

    Args:
        args: 包含 text 和 operation 字段的字典
            - text: 要操作的文本
            - operation: 操作类型（upper, lower, reverse, length）

    Returns:
        包含操作结果的字典
    """
    text = args["text"]
    operation = args["operation"].lower()

    operations = {
        "upper": lambda t: t.upper(),
        "lower": lambda t: t.lower(),
        "reverse": lambda t: t[::-1],
        "length": lambda t: str(len(t)),
        "title": lambda t: t.title(),
    }

    if operation not in operations:
        return {
            "content": [{
                "type": "text",
                "text": f"错误: 不支持的操作 '{operation}'。支持的操作: {', '.join(operations.keys())}"
            }],
            "is_error": True
        }

    result = operations[operation](text)
    return {
        "content": [{
            "type": "text",
            "text": f"操作结果 ({operation}): {result}"
        }]
    }


# ============================================================
# 示例函数
# ============================================================

async def basic_calculations_example():
    """示例 1: 基本数学计算"""
    print("\n📝 示例 1: 数学计算工具")
    print("-" * 40)

    # 创建自定义工具服务器
    calc_server = create_sdk_mcp_server(
        name="math_utils",
        version="1.0.0",
        tools=[calculate, get_time]
    )

    options = ClaudeAgentOptions(
        mcp_servers={"math": calc_server},
        allowed_tools=[
            "mcp__math__calculate",
            "mcp__math__get_time"
        ]
    )

    async with ClaudeSDKClient(options=options) as client:
        # 测试计算功能
        print("计算: 123 * 456")
        await client.query("请计算 123 乘以 456")

        async for message in client.receive_response():
            if isinstance(message, AssistantMessage):
                for block in message.content:
                    if isinstance(block, TextBlock):
                        print(f"  {block.text}")

        # 测试时间功能
        print("\n获取当前时间")
        await client.query("现在几点了？")

        async for message in client.receive_response():
            if isinstance(message, AssistantMessage):
                for block in message.content:
                    if isinstance(block, TextBlock):
                        print(f"  {block.text}")


async def advanced_tools_example():
    """示例 2: 高级工具组合"""
    print("\n📝 示例 2: 高级工具组合")
    print("-" * 40)

    # 创建包含所有工具的服务器
    full_server = create_sdk_mcp_server(
        name="custom_tools",
        version="1.0.0",
        tools=[calculate, get_time, string_operations]
    )

    options = ClaudeAgentOptions(
        mcp_servers={"custom": full_server},
        allowed_tools=[
            "mcp__custom__calculate",
            "mcp__custom__get_time",
            "mcp__custom__string_operations"
        ]
    )

    async with ClaudeSDKClient(options=options) as client:
        # 组合使用多个工具
        print("执行复杂任务: 计算平方根后对结果字符串进行操作")
        await client.query("请计算 16 的平方根，然后把结果转换成中文的大写形式")

        async for message in client.receive_response():
            if isinstance(message, AssistantMessage):
                for block in message.content:
                    if isinstance(block, TextBlock):
                        print(f"{block.text}", end="", flush=True)
        print()


async def error_handling_example():
    """示例 3: 错误处理"""
    print("\n📝 示例 3: 错误处理")
    print("-" * 40)

    calc_server = create_sdk_mcp_server(
        name="math_utils",
        version="1.0.0",
        tools=[calculate]
    )

    options = ClaudeAgentOptions(
        mcp_servers={"math": calc_server},
        allowed_tools=["mcp__math__calculate"]
    )

    async with ClaudeSDKClient(options=options) as client:
        # 测试除零错误
        print("测试除零错误")
        await client.query("请计算 10 除以 0")

        async for message in client.receive_response():
            if isinstance(message, AssistantMessage):
                for block in message.content:
                    if isinstance(block, TextBlock):
                        print(f"  {block.text}")


# ============================================================
# 主函数
# ============================================================

async def main():
    """运行所有自定义工具示例"""
    print_example_header(
        "自定义工具示例",
        "展示如何创建和使用自定义工具扩展 Claude Agent SDK"
    )

    # 验证配置
    config = get_config()
    if not config.anthropic_api_key:
        print("❌ 错误: 未设置 ANTHROPIC_API_KEY")
        print("请在 .env 文件中配置 API 密钥")
        return

    try:
        await read_file_example()
                
        # 运行示例
        await basic_calculations_example()
        await advanced_tools_example()
        await error_handling_example()

        print("\n" + "=" * 50)
        print("✅ 所有自定义工具示例完成!")
        print("=" * 50)

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    anyio.run(main)
