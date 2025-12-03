#!/usr/bin/env python3
"""
Claude Agent SDK - 多模型支持版本
支持Anthropic Claude、OpenAI、本地模型等多种API

安装依赖:
pip install anthropic openai

配置环境变量:
export ANTHROPIC_API_KEY="your-anthropic-api-key-here"
export OPENAI_API_KEY="your-openai-api-key-here"
"""

import os
from typing import List, Dict, Optional

# 可选导入 - 处理依赖缺失的情况
try:
    import anthropic
except ImportError:
    anthropic = None

try:
    import openai
except ImportError:
    openai = None

# DeepSeek API密钥 - 从环境变量获取
DEEPSEEK_API_KEY = os.getenv("DEEPSEEK_API_KEY")
# DeepSeek API 基础 URL
DEEPSEEK_BASE_URL = "https://api.deepseek.com/v1"

# llm = ChatOpenAI(
#     model="deepseek-chat", # 使用 DeepSeek 的模型名称
#     openai_api_base=DEEPSEEK_BASE_URL, # 指定 DeepSeek 的 URL
#     temperature=0.7
# )

class UniversalAIAgent:
    """通用AI代理类 - 支持多种模型"""

    SUPPORTED_PROVIDERS = {
        "claude": {
            "models": ["glm-4.6", "claude-4-haiku", "claude-4-opus"],
            "env_key": "ANTHROPIC_API_KEY",
            "client_class": anthropic.Anthropic if anthropic else None
        },
        "openai": {
            "models": ["gpt-3.5-turbo", "gpt-4", "gpt-4-turbo-preview"],
            "env_key": "OPENAI_API_KEY",
            "client_class": openai.OpenAI if openai else None
        },
        "deepseek": {
            "models": ["deepseek-chat"],
            "env_key": "DEEPSEEK_API_KEY",
            "client_class": openai.OpenAI if openai else None
        },
        "ollama": {
            "models": ["llama2", "mistral", "codellama", "phi"],
            "env_key": None,  # 本地模型不需要API密钥
            "client_class": None  # 使用自定义实现
        },
        "mock": {
            "models": ["mock-model"],
            "env_key": None,  # 模拟模型不需要API密钥
            "client_class": None
        }
    }

    def __init__(self, provider: str = "mock", model: str = None, api_key: Optional[str] = None, base_url: Optional[str] = None):
        """
        初始化通用AI代理

        Args:
            provider: 模型提供商 (claude, openai, deeoseed, mock)
            model: 模型名称
            api_key: API密钥
            base_url: 自定义API端点
        """
        self.provider = provider.lower()

        if self.provider not in self.SUPPORTED_PROVIDERS:
            raise ValueError(f"不支持的提供商: {provider}。支持的提供商: {list(self.SUPPORTED_PROVIDERS.keys())}")

        provider_config = self.SUPPORTED_PROVIDERS[self.provider]

        # 设置默认模型
        if model is None:
            model = provider_config["models"][0]

        self.model = model
        self.conversation_history: List[Dict[str, str]] = []

        # 初始化客户端
        if self.provider == "mock":
            self.client = None
            print(f"[Mock] 使用模拟模型: {model} (无需API密钥)")
        elif self.provider == "ollama":
            self.client = None
            self.base_url = base_url or "http://localhost:11434"
            print(f"[Ollama] 使用本地模型: {model} (端点: {self.base_url})")
        else:
            # 设置API密钥
            if self.provider == "deepseek":
                # DeepSeek使用硬编码的API Key
                self.api_key = api_key or DEEPSEEK_API_KEY
            else:
                self.api_key = api_key or os.getenv(provider_config["env_key"])

            if not self.api_key:
                print(f"[Warning] 未设置{provider_config['env_key']}环境变量，将使用模拟模式")
                self.provider = "mock"
                self.client = None
                return

            # 初始化客户端
            if self.provider == "claude":
                self.client = anthropic.Anthropic(api_key=self.api_key,base_url=base_url)
                print(f"[Claude] 使用Claude模型: {model}")
            elif self.provider == "openai":
                self.client = openai.OpenAI(api_key=self.api_key, base_url=base_url)
                print(f"[OpenAI] 使用OpenAI模型: {model}")
            elif self.provider == "deepseek":
                # 使用硬编码的DeepSeek配置或自定义base_url
                deepseek_base_url = base_url or DEEPSEEK_BASE_URL
                self.client = openai.OpenAI(api_key=self.api_key, base_url=deepseek_base_url)
                print(f"[DeepSeek] 使用DeepSeek模型: {model} (端点: {deepseek_base_url})")

    def add_system_prompt(self, prompt: str):
        """添加系统提示词"""
        self.conversation_history.insert(0, {"role": "system", "content": prompt})

    def chat(self, message: str, stream: bool = False) -> str:
        """
        与AI进行对话

        Args:
            message: 用户消息
            stream: 是否使用流式响应

        Returns:
            AI的回复内容
        """
        # 添加用户消息到历史记录
        self.conversation_history.append({"role": "user", "content": message})

        try:
            if stream:
                return self._stream_response()
            else:
                return self._get_sync_response()
        except Exception as e:
            error_msg = f"调用{self.provider} API时出错: {str(e)}"
            print(error_msg)
            return error_msg

    def _get_sync_response(self) -> str:
        """获取同步响应"""
        if self.provider == "mock":
            return self._mock_response()
        elif self.provider == "ollama":
            return self._ollama_response()
        elif self.provider == "claude":
            return self._claude_response()
        elif self.provider == "openai":
            return self._openai_response()
        elif self.provider == "deepseek":
            return self._openai_response()
        else:
            raise ValueError(f"不支持的提供商: {self.provider}")

    def _mock_response(self) -> str:
        """模拟响应 - 用于测试"""
        user_message = self.conversation_history[-1]["content"]

        # 简单的模拟回复逻辑
        if "你好" in user_message or "hi" in user_message.lower():
            response = "你好！我是一个模拟AI助手。虽然我不能提供真实的智能回复，但可以帮你测试代码结构。"
        elif "代码" in user_message or "code" in user_message.lower():
            response = "我可以帮你编写代码。这是一个模拟回复，展示了代码助手的功能。"
        elif "问题" in user_message or "problem" in user_message.lower():
            response = "我可以帮你解决问题。请描述你的具体需求，我会提供模拟的解决方案。"
        else:
            response = f"这是一个模拟回复。你的问题是: {user_message}\n在实际使用中，这里会是真实AI模型的回复。"

        self.conversation_history.append({"role": "assistant", "content": response})
        return response

    def _ollama_response(self) -> str:
        """Ollama本地模型响应"""
        import requests

        messages = [msg for msg in self.conversation_history if msg["role"] != "system"]
        system_prompt = next((msg["content"] for msg in self.conversation_history if msg["role"] == "system"), None)

        payload = {
            "model": self.model,
            "messages": messages,
            "stream": False
        }

        if system_prompt:
            payload["system"] = system_prompt

        try:
            response = requests.post(f"{self.base_url}/api/chat", json=payload, timeout=30)
            response.raise_for_status()
            result = response.json()
            assistant_message = result["message"]["content"]

            self.conversation_history.append({"role": "assistant", "content": assistant_message})
            return assistant_message
        except Exception as e:
            return f"Ollama API调用失败: {str(e)}。请确保Ollama服务正在运行。"

    def _claude_response(self) -> str:
        """Claude API响应"""
        messages = [msg for msg in self.conversation_history if msg["role"] != "system"]
        system_prompt = next((msg["content"] for msg in self.conversation_history if msg["role"] == "system"), None)

        response = self.client.messages.create(
            model=self.model,
            max_tokens=4000,
            temperature=0.7,
            system=system_prompt,
            messages=messages
        )

        assistant_message = response.content[0].text
        self.conversation_history.append({"role": "assistant", "content": assistant_message})
        return assistant_message

    def _openai_response(self) -> str:
        """OpenAI API响应"""
        messages = self.conversation_history.copy()

        response = self.client.chat.completions.create(
            model=self.model,
            messages=messages,
            max_tokens=4000,
            temperature=0.7
        )

        assistant_message = response.choices[0].message.content
        self.conversation_history.append({"role": "assistant", "content": assistant_message})
        return assistant_message

    def _stream_response(self) -> str:
        """获取流式响应"""
        if self.provider == "mock":
            return self._mock_stream_response()
        elif self.provider == "claude":
            return self._claude_stream_response()
        elif self.provider == "openai":
            return self._openai_stream_response()
        elif self.provider == "deepseek":
            return self._openai_stream_response()  # DeepSeek使用OpenAI兼容的流式响应
        else:
            # 其他提供商暂不支持流式响应，回退到同步响应
            print(f"[Warning] {self.provider} 暂不支持流式响应，使用同步响应")
            return self._get_sync_response()

    def _mock_stream_response(self) -> str:
        """模拟流式响应"""
        user_message = self.conversation_history[-1]["content"]
        response = self._mock_response()

        print(f"{self.provider}: ", end="", flush=True)
        for char in response:
            print(char, end="", flush=True)
            import time
            time.sleep(0.01)  # 模拟流式输出
        print()

        return response

    def _claude_stream_response(self) -> str:
        """Claude流式响应"""
        messages = [msg for msg in self.conversation_history if msg["role"] != "system"]
        system_prompt = next((msg["content"] for msg in self.conversation_history if msg["role"] == "system"), None)

        full_response = ""
        print("Claude: ", end="", flush=True)

        stream = self.client.messages.create(
            model=self.model,
            max_tokens=4000,
            temperature=0.7,
            system=system_prompt,
            messages=messages,
            stream=True
        )

        for chunk in stream:
            if chunk.type == "content_block_delta" and chunk.delta.type == "text_delta":
                content = chunk.delta.text
                print(content, end="", flush=True)
                full_response += content

        print()
        self.conversation_history.append({"role": "assistant", "content": full_response})
        return full_response

    def _openai_stream_response(self) -> str:
        """OpenAI流式响应"""
        messages = self.conversation_history.copy()

        full_response = ""
        print("OpenAI: ", end="", flush=True)

        stream = self.client.chat.completions.create(
            model=self.model,
            messages=messages,
            max_tokens=4000,
            temperature=0.7,
            stream=True
        )

        for chunk in stream:
            if chunk.choices[0].delta.content is not None:
                content = chunk.choices[0].delta.content
                print(content, end="", flush=True)
                full_response += content

        print()
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


# 为了向后兼容，保留原有类名的别名
ClaudeAgent = UniversalAIAgent

class UniversalTaskAgent(UniversalAIAgent):
    """通用任务型代理"""

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


class UniversalCodeAgent(UniversalAIAgent):
    """通用代码助手代理"""

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


# 向后兼容的类别名
ClaudeTaskAgent = UniversalTaskAgent
ClaudeCodeAgent = UniversalCodeAgent


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
            api_key=os.getenv('CLAUDE_API_KEY'),
            base_url=os.getenv('CLAUDE_BASE_URL', 'https://open.bigmodel.cn/api/anthropic')
        )

        plan = task_agent.solve_problem("我想学习人工智能，应该从哪里开始？")
        print("学习建议:", plan)
        print("-" * 40)
        
        # 3. 代码助手代理（模拟模式）
        print("\n代码助手代理示例（模拟模式）:")
        code_agent = UniversalCodeAgent(language="Python",       
                                        provider="deepseek",
                                        api_key=DEEPSEEK_API_KEY,
                                        base_url=DEEPSEEK_BASE_URL)

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
        print("   export OPENAI_API_KEY='your-openai-key'")
        print("\n3. 获取API密钥:")
        print("   Claude: https://console.anthropic.com/")
        print("   OpenAI: https://platform.openai.com/")
        print("\n4. 本地模型（Ollama）:")
        print("   下载安装: https://ollama.ai/")
        print("   运行服务: ollama serve")


if __name__ == "__main__":
    main()