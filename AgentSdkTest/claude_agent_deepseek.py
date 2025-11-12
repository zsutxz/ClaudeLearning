#!/usr/bin/env python3
"""
Claude Agent SDK - DeepSeek API版本
使用DeepSeek API替代Anthropic API

安装依赖:
pip install openai

配置环境变量:
export DEEPSEEK_API_KEY="your-deepseek-api-key-here"
"""

import os
from typing import List, Dict, Optional
from openai import OpenAI


class DeepSeekAgent:
    """基于DeepSeek API的智能代理类"""

    def __init__(self, api_key: Optional[str] = None, model: str = "deepseek-chat"):
        """
        初始化DeepSeek代理

        Args:
            api_key: DeepSeek API密钥，如未提供则从环境变量读取
            model: 使用的DeepSeek模型版本
        """
        self.api_key = api_key or os.getenv("DEEPSEEK_API_KEY")
        if not self.api_key:
            raise ValueError("请设置DEEPSEEK_API_KEY环境变量或传入api_key参数")

        # DeepSeek API兼容OpenAI格式
        self.client = OpenAI(
            api_key=self.api_key,
            base_url="https://api.deepseek.com"
        )
        self.model = model
        self.conversation_history: List[Dict[str, str]] = []

    def add_system_prompt(self, prompt: str):
        """添加系统提示词"""
        self.conversation_history.insert(0, {"role": "system", "content": prompt})

    def chat(self, message: str, stream: bool = False) -> str:
        """
        与DeepSeek进行对话

        Args:
            message: 用户消息
            stream: 是否使用流式响应

        Returns:
            DeepSeek的回复内容
        """
        # 添加用户消息到历史记录
        self.conversation_history.append({"role": "user", "content": message})

        try:
            if stream:
                return self._stream_response()
            else:
                return self._get_sync_response()
        except Exception as e:
            error_msg = f"调用DeepSeek API时出错: {str(e)}"
            print(error_msg)
            return error_msg

    def _get_sync_response(self) -> str:
        """获取同步响应"""
        response = self.client.chat.completions.create(
            model=self.model,
            messages=self.conversation_history,
            max_tokens=4000,
            temperature=0.7
        )

        # 添加助手回复到历史记录
        assistant_message = response.choices[0].message.content
        self.conversation_history.append({"role": "assistant", "content": assistant_message})

        return assistant_message

    def _stream_response(self) -> str:
        """获取流式响应"""
        full_response = ""
        print("DeepSeek: ", end="", flush=True)

        stream = self.client.chat.completions.create(
            model=self.model,
            messages=self.conversation_history,
            max_tokens=4000,
            temperature=0.7,
            stream=True
        )

        for chunk in stream:
            if chunk.choices[0].delta.content is not None:
                content = chunk.choices[0].delta.content
                print(content, end="", flush=True)
                full_response += content

        print()  # 换行
        self.conversation_history.append({"role": "assistant", "content": full_response})
        return full_response

    def clear_history(self):
        """清空对话历史"""
        # 保留系统提示词
        system_messages = [msg for msg in self.conversation_history if msg["role"] == "system"]
        self.conversation_history = system_messages

    def get_conversation_summary(self) -> str:
        """获取对话摘要"""
        if not self.conversation_history:
            return "暂无对话记录"

        user_messages = len([msg for msg in self.conversation_history if msg["role"] == "user"])
        assistant_messages = len([msg for msg in self.conversation_history if msg["role"] == "assistant"])

        return f"对话统计: {user_messages} 条用户消息, {assistant_messages} 条助手回复"


class DeepSeekTaskAgent(DeepSeekAgent):
    """DeepSeek任务型代理"""

    def __init__(self, task_description: str, **kwargs):
        super().__init__(**kwargs)
        self.task_description = task_description
        self.add_system_prompt(f"""你是一个专业的任务助手。你的主要职责是: {task_description}

请遵循以下原则:
1. 准确理解用户需求
2. 提供具体可行的解决方案
3. 如有问题及时澄清
4. 保持专业和友好的态度
5. 在必要时请求更多信息""")

    def solve_problem(self, problem: str) -> str:
        """解决特定问题"""
        prompt = f"请帮我解决这个问题: {problem}\n\n请提供详细的解决方案。"
        return self.chat(prompt)


class DeepSeekCodeAgent(DeepSeekAgent):
    """DeepSeek代码助手代理"""

    def __init__(self, language: str = "Python", **kwargs):
        super().__init__(**kwargs)
        self.language = language
        self.add_system_prompt(f"""你是一个专业的{language}编程助手。

你的能力包括:
- 编写高质量的{language}代码
- 代码审查和优化建议
- 调试和错误修复
- 解释复杂的概念和算法
- 提供最佳实践建议

请确保代码:
1. 语法正确，符合语言规范
2. 逻辑清晰，易于理解
3. 包含必要的注释
4. 遵循最佳实践和安全原则""")

    def write_code(self, requirement: str) -> str:
        """根据需求编写代码"""
        prompt = f"请编写{self.language}代码来实现以下功能: {requirement}"
        return self.chat(prompt)

    def review_code(self, code: str) -> str:
        """代码审查"""
        prompt = f"请审查以下{self.language}代码，提供改进建议:\n\n```{self.language.lower()}\n{code}\n```"
        return self.chat(prompt)

    def debug_code(self, code: str, error_message: str) -> str:
        """调试代码"""
        prompt = f"""以下{self.language}代码出现了错误，请帮助调试:

代码:
```{self.language.lower()}
{code}
```

错误信息:
{error_message}

请分析问题原因并提供修复方案。"""
        return self.chat(prompt)


def main():
    """主函数 - 演示DeepSeek代理的使用"""
    print("DeepSeek Agent SDK Python示例")
    print("=" * 50)

    try:
        # 1. 基础对话代理
        print("\n基础对话代理示例:")
        agent = DeepSeekAgent()

        response = agent.chat("你好！请简单介绍一下你自己。", stream=False)
        print("DeepSeek回复:", response)
        print("-" * 40)

        # 2. 任务型代理
        print("\n任务型代理示例:")
        task_agent = DeepSeekTaskAgent(
            "帮助用户制定学习计划和提供学习建议"
        )

        plan = task_agent.solve_problem("我想学习人工智能，应该从哪里开始？")
        print("学习建议:", plan[:300] + "...")
        print("-" * 40)

        # 3. 代码助手代理
        print("\n代码助手代理示例:")
        code_agent = DeepSeekCodeAgent(language="Python")

        code_solution = code_agent.write_code("编写一个函数来计算斐波那契数列")
        print("代码解决方案:")
        print(code_solution[:400] + "...")
        print("-" * 40)

        # 4. 代码审查示例
        print("\n代码审查示例:")
        buggy_code = """
def calculate_average(numbers):
    total = 0
    for num in numbers:
        total += num
    return total / len(numbers)
"""

        review = code_agent.review_code(buggy_code)
        print("代码审查结果:")
        print(review[:400] + "...")
        print("-" * 40)

        # 5. 流式响应演示
        print("\n流式响应演示:")
        stream_response = agent.chat("请简单介绍深度学习的基本概念", stream=True)
        print("-" * 40)

        # 显示对话统计
        print(f"\n{agent.get_conversation_summary()}")
        print(f"{task_agent.get_conversation_summary()}")
        print(f"{code_agent.get_conversation_summary()}")

    except Exception as e:
        print(f"运行示例时出错: {str(e)}")
        print("\n请确保:")
        print("1. 已安装 openai 包: pip install openai")
        print("2. 已设置 DEEPSEEK_API_KEY 环境变量")
        print("3. API密钥有效且有足够的额度")
        print("\n获取DeepSeek API密钥:")
        print("- 访问: https://platform.deepseek.com/")
        print("- 注册账户并获取API密钥")


if __name__ == "__main__":
    main()