#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
示例 07: 高级代理示例

综合运用所有功能的高级示例，展示完整的 Agent 应用场景。

功能演示：
- 综合使用工具、MCP、流式响应
- 复杂任务处理
- 多轮对话管理
- 自定义系统提示词
"""

# Windows 控制台编码修复 - 必须在其他导入之前
import sys
import os
if sys.platform == "win32":
    try:
        sys.stdout.reconfigure(encoding='utf-8')
        sys.stderr.reconfigure(encoding='utf-8')
    except:
        pass
import anyio
from pathlib import Path

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))

from claude_agent_sdk import (
    query,
    ClaudeAgentOptions,
    AssistantMessage,
    ResultMessage,
    TextBlock,
)

from lib.config import get_config
from lib.agent_factory import create_agent
from lib.utils import print_example_header, print_cost


async def code_review_agent_example():
    """示例 1: 代码审查代理"""
    print("\n📝 示例 1: 代码审查代理")
    print("-" * 40)
    print("这是一个专业的代码审查助手，可以：")
    print("  • 读取代码文件")
    print("  • 分析代码质量")
    print("  • 提供改进建议")
    print()

    # 创建代码审查代理
    factory = create_agent("code")

    options = ClaudeAgentOptions(
        system_prompt=f"""{factory.code_system_prompt}

作为代码审查专家，请检查：
1. 代码风格和规范
2. 潜在的安全问题
3. 性能优化建议
4. 错误处理完善性""",
        allowed_tools=["Read", "Grep", "Glob"],
        max_turns=5,
    )

    message_stream = query(
        prompt="请审查 ../lib/config.py 文件，分析其代码质量并提供改进建议。",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")
        elif isinstance(message, ResultMessage):
            print(f"\n✅ 审查完成")
            if message.total_cost_usd > 0:
                print_cost(message.total_cost_usd)


async def documentation_generator_example():
    """示例 2: 文档生成代理"""
    print("\n📝 示例 2: 文档生成代理")
    print("-" * 40)
    print("这是一个智能文档生成助手，可以：")
    print("  • 分析项目结构")
    print("  • 读取代码文件")
    print("  • 生成 API 文档")
    print()

    options = ClaudeAgentOptions(
        system_prompt="""你是一个专业的技术文档编写者。

你的任务：
1. 分析项目结构
2. 阅读关键代码文件
3. 生成清晰的 API 文档

文档格式要求：
- 使用 Markdown 格式
- 包含函数签名和说明
- 添加使用示例
- 标注参数和返回值""",
        allowed_tools=["Read", "Grep", "Glob", "Write"],
        max_turns=8,
    )

    message_stream = query(
        prompt="请为 ../lib/agent_factory.py 模块生成 API 文档，"
              "并保存到 ../docs/agent_factory_api.md 文件。",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")
        elif isinstance(message, ResultMessage):
            print(f"\n✅ 文档生成完成")


async def task_planning_agent_example():
    """示例 3: 任务规划代理"""
    print("\n📝 示例 3: 任务规划代理")
    print("-" * 40)
    print("这是一个智能任务规划助手，可以：")
    print("  • 理解复杂任务需求")
    print("  • 分解任务步骤")
    print("  • 协助执行任务")
    print()

    options = ClaudeAgentOptions(
        system_prompt="""你是一个专业的任务规划助手。

工作流程：
1. 仔细理解用户需求
2. 将复杂任务分解为步骤
3. 逐步执行每个步骤
4. 确认每步完成后再继续

原则：
- 不确定时主动询问
- 执行危险操作前请求确认
- 提供清晰的进度反馈""",
        allowed_tools=["Read", "Write", "Grep", "Glob", "Bash"],
        max_turns=10,
    )

    message_stream = query(
        prompt="请帮我完成以下任务：\n"
              "1. 创建一个名为 test_output 的目录\n"
              "2. 在该目录中创建一个 README.md 文件\n"
              "3. 写入项目简介和安装说明\n"
              "4. 验证文件创建成功",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")
        elif isinstance(message, ResultMessage):
            print(f"\n✅ 任务规划完成")


async def interactive_tutor_example():
    """示例 4: 交互式教学代理"""
    print("\n📝 示例 4: 交互式教学代理")
    print("-" * 40)
    print("这是一个智能教学助手，可以：")
    print("  • 循序渐进地讲解概念")
    print("  • 根据学生反应调整教学")
    print("  • 提供练习和反馈")
    print()

    options = ClaudeAgentOptions(
        system_prompt="""你是一位经验丰富的编程导师。

教学风格：
1. 苏格拉底式教学 - 引导学生思考
2. 循序渐进 - 从基础到进阶
3. 实例驱动 - 用代码演示概念
4. 鼓励提问 - 营造轻松氛围

对话策略：
- 先了解学生基础
- 用简单例子引入概念
- 鼓励学生动手实践
- 及时给予正向反馈""",
        max_turns=5,
    )

    # 模拟多轮教学对话
    tutorial_prompts = [
        "我想学习 Python 的装饰器，但我不太理解。你能从最简单的开始教我吗？",
        "这个例子我明白了！那装饰器有什么实际用途呢？",
        "太有趣了！能给我一个练习题吗？",
    ]

    for prompt in tutorial_prompts:
        print(f"\n👤 学生: {prompt}")
        print("🤖 导师: ", end="", flush=True)

        message_stream = query(prompt=prompt, options=options)

        async for message in message_stream:
            if isinstance(message, AssistantMessage):
                for block in message.content:
                    if isinstance(block, TextBlock):
                        print(block.text, end="", flush=True)

        print()  # 换行


async def debugging_assistant_example():
    """示例 5: 调试助手代理"""
    print("\n📝 示例 5: 调试助手代理")
    print("-" * 40)
    print("这是一个智能调试助手，可以：")
    print("  • 分析错误信息")
    print("  • 定位问题代码")
    print("  • 提供修复方案")
    print()

    # 创建一个有错误的示例代码
    buggy_code = '''
def calculate_average(numbers):
    total = 0
    for num in numbers:
        total += num
    average = total / len(numbers)
    return average

# 测试
result = calculate_average([1, 2, 3, 4, 5])
print(f"平均值: {result}")

# 边界情况测试
empty_result = calculate_average([])
print(f"空列表平均值: {empty_result}")
'''

    # 保存错误代码
    test_file = Path(__file__).parent / "buggy_example.py"
    test_file.write_text(buggy_code, encoding="utf-8")

    options = ClaudeAgentOptions(
        system_prompt="""你是一个专业的代码调试助手。

调试流程：
1. 仔细阅读错误信息
2. 分析可能的原因
3. 定位问题代码
4. 提供修复方案
5. 解释防止类似问题的建议

输出格式：
- 🔍 问题诊断
- 💡 解决方案
- ✅ 修复后的代码
- 📚 预防建议""",
        allowed_tools=["Read", "Write", "Bash"],
        max_turns=5,
    )

    message_stream = query(
        prompt=f"请帮我调试这个文件中的代码: {test_file}\n"
              "代码在处理空列表时会出现错误，请找出问题并提供修复方案。",
        options=options,
    )

    async for message in message_stream:
        if isinstance(message, AssistantMessage):
            for block in message.content:
                if isinstance(block, TextBlock):
                    print(f"{block.text}")
        elif isinstance(message, ResultMessage):
            print(f"\n✅ 调试分析完成")


async def main():
    """运行所有高级代理示例"""
    print_example_header(
        "Claude Agent SDK - 高级代理示例",
        "综合运用所有功能的完整 Agent 应用场景"
    )

    # 验证配置
    config = get_config()
    if not config.anthropic_api_key:
        print("❌ 错误: 未设置 ANTHROPIC_API_KEY")
        print("请在 config/.env 文件中配置 API 密钥")
        return

    try:
        # 运行示例
        await code_review_agent_example()
        await documentation_generator_example()
        await task_planning_agent_example()
        await interactive_tutor_example()
        await debugging_assistant_example()

        print("\n" + "=" * 50)
        print("✅ 所有高级代理示例完成!")
        print("=" * 50)

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    anyio.run(main)
