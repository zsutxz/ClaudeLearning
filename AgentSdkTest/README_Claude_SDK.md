# 通用AI代理SDK - 多模型支持

这是一个功能完整的Python SDK，支持多种AI模型的通用代理框架，包括Claude、OpenAI、DeepSeek、Ollama等。项目提供了统一的接口来使用不同的AI服务，特别适合需要多模型支持的应用开发。

## 📁 文件结构

```
AgentSdkTest/
├── claude_agent_deepseek.py         # 通用AI代理核心实现（支持多模型）
├── test_deepseek.py                 # DeepSeek API快速测试工具
├── requirements.txt                 # 项目依赖列表
├── README_Claude_SDK.md             # 本说明文档
└── .gitignore                       # Git忽略配置
```

## 🚀 快速开始

### 1. 安装依赖

```bash
pip install -r requirements.txt
```

或手动安装：
```bash
pip install anthropic openai requests
```

### 2. 获取API密钥

根据需要获取相应服务的API密钥：

- **DeepSeek**: [https://platform.deepseek.com/](https://platform.deepseek.com/)
- **Claude**: [https://console.anthropic.com/](https://console.anthropic.com/)
- **OpenAI**: [https://platform.openai.com/](https://platform.openai.com/)

### 3. 配置API密钥

#### 方法1：代码中配置（推荐用于测试）
```python
# 在claude_agent_deepseek.py中已预配置DeepSeek API Key
agent = UniversalAIAgent(
    provider="deepseek",
    api_key="your-api-key-here",
    base_url="https://api.deepseek.com/v1"
)
```

#### 方法2：环境变量配置
```bash
# Windows
set DEEPSEEK_API_KEY="your-deepseek-api-key-here"
set ANTHROPIC_API_KEY="your-claude-api-key-here"
set OPENAI_API_KEY="your-openai-api-key-here"

# Linux/MacOS
export DEEPSEEK_API_KEY="your-deepseek-api-key-here"
export ANTHROPIC_API_KEY="your-claude-api-key-here"
export OPENAI_API_KEY="your-openai-api-key-here"
```

### 4. 运行示例

#### 快速测试（推荐）
```bash
# 测试DeepSeek API连接
python test_deepseek.py
```

#### 完整功能演示
```bash
# 运行通用AI代理完整示例
python claude_agent_deepseek.py
```

## 🏗️ 核心功能

### 1. UniversalAIAgent (通用AI代理)
支持多种模型的基础代理类：

```python
# DeepSeek示例
agent = UniversalAIAgent(provider="deepseek")
response = agent.chat("你好！", stream=True)

# Claude示例
claude_agent = UniversalAIAgent(provider="claude", model="claude-3-5-sonnet-20241022")
response = claude_agent.chat("分析一下这个代码")

# OpenAI示例
openai_agent = UniversalAIAgent(provider="openai", model="gpt-4")
response = openai_agent.chat("写一个Python函数")

# 模拟模式（无需API密钥）
mock_agent = UniversalAIAgent(provider="mock")
response = mock_agent.chat("测试功能")
```

### 2. UniversalTaskAgent (任务型代理)
专门处理特定任务的代理：

```python
# 创建学习规划助手
task_agent = UniversalTaskAgent(
    task_description="帮助用户制定学习计划和提供学习建议",
    provider="deepseek"
)
plan = task_agent.solve_problem("我想学习人工智能，应该从哪里开始？")

# 创建商业咨询助手
business_agent = UniversalTaskAgent(
    task_description="提供商业策略和营销建议",
    provider="claude"
)
strategy = business_agent.solve_problem("如何提升产品销量？")
```

### 3. UniversalCodeAgent (代码助手)
多语言编程专用代理：

```python
# Python代码助手
python_agent = UniversalCodeAgent(language="Python", provider="deepseek")
code = python_agent.write_code("实现一个快速排序算法")
review = python_agent.review_code("some_code_here")
debug_result = python_agent.debug_code("buggy_code", "错误信息")

# JavaScript代码助手
js_agent = UniversalCodeAgent(language="JavaScript", provider="claude")
js_code = js_agent.write_code("创建一个React组件")
```

## 🛠️ 高级功能

### 🔄 多模型支持
支持5种不同的AI服务提供商：

```python
# 支持的提供商和模型
providers = {
    "claude": ["claude-3-5-sonnet-20241022", "claude-3-haiku-20240307", "claude-3-opus-20240229"],
    "openai": ["gpt-3.5-turbo", "gpt-4", "gpt-4-turbo-preview"],
    "deepseek": ["deepseek-chat"],
    "ollama": ["llama2", "mistral", "codellama", "phi"],
    "mock": ["mock-model"]  # 模拟模式
}
```

### 📡 流式响应
实时显示AI回复过程，支持对话体验：

```python
# DeepSeek流式响应
agent = UniversalAIAgent(provider="deepseek")
response = agent.chat("请详细解释机器学习基础概念", stream=True)

# Claude流式响应
claude_agent = UniversalAIAgent(provider="claude")
response = claude_agent.chat("分析这个项目的架构", stream=True)
```

### 📚 对话历史管理
智能维护对话上下文：

```python
# 清空对话历史（保留系统提示词）
agent.clear_history()

# 获取对话统计摘要
summary = agent.get_conversation_summary()
print(summary)  # 输出：对话统计: 5 条用户消息, 5 条助手回复

# 自定义系统提示词
agent.add_system_prompt("你是一个专业的AI助手，请用简洁明了的语言回答问题。")
```

### 🔧 自定义API端点
支持自定义API配置：

```python
# 自定义DeepSeek端点
custom_agent = UniversalAIAgent(
    provider="deepseek",
    base_url="https://your-custom-endpoint.com/v1"
)

# 自定义OpenAI端点
custom_openai = UniversalAIAgent(
    provider="openai",
    base_url="https://api.your-service.com/v1"
)
```

## 📊 支持的模型

### Claude模型
- `claude-3-5-sonnet-20241022` - 最新高性能模型
- `claude-3-haiku-20240307` - 快速响应模型
- `claude-3-opus-20240229` - 高质量推理模型

### OpenAI模型
- `gpt-4` - 强大的通用模型
- `gpt-4-turbo-preview` - 快速预览版
- `gpt-3.5-turbo` - 经济实用模型

### DeepSeek模型
- `deepseek-chat` - 通用对话模型（推荐）

### 本地模型 (Ollama)
- `llama2` - 开源大语言模型
- `mistral` - 高效推理模型
- `codellama` - 代码专用模型
- `phi` - 轻量级模型

### 模拟模式
- `mock-model` - 无需API密钥的测试模式

## ⚠️ 注意事项

### 1. API限制
- 每个API请求有token限制
- 建议设置合理的`max_tokens`参数
- 监控API使用量和费用

### 2. 错误处理
代码包含完善的错误处理，但仍建议：
- 设置适当的超时时间
- 实现重试机制
- 记录错误日志

### 3. 性能优化
- 使用对象池管理代理实例
- 合理控制对话历史长度
- 在高并发场景下使用异步处理

## 🔧 自定义扩展

### 创建新的代理类型

```python
# Python
class CustomAgent(UniversalAIAgent):
    def __init__(self, custom_config, **kwargs):
        super().__init__(**kwargs)
        self.custom_config = custom_config
        self.add_system_prompt(f"自定义代理配置: {custom_config}")

    def custom_method(self, input_data):
        prompt = f"根据自定义配置处理: {input_data}"
        return self.chat(prompt)
```

## 🛠️ 最近修复的问题

### ✅ 已修复 (v2.0)

#### 1. DeepSeek API初始化错误
**问题**: `'UniversalTaskAgent' object has no attribute 'client'`

**原因**:
- 代码中存在重复的条件判断
- DeepSeek初始化逻辑不完整
- API密钥配置不正确

**修复内容**:
- 删除了重复的`elif self.provider == "openai":`代码块
- 添加了专门的DeepSeek客户端初始化逻辑
- 修正了API密钥配置，支持硬编码和环境变量
- 在流式响应中添加了DeepSeek支持

**修复代码示例**:
```python
# 修复前（错误）
elif self.provider == "openai":
    self.client = openai.OpenAI(api_key=self.api_key, base_url=base_url)
elif self.provider == "openai":  # 重复！
    self.client = openai.OpenAI(api_key=self.api_key, base_url=base_url)

# 修复后（正确）
elif self.provider == "openai":
    self.client = openai.OpenAI(api_key=self.api_key, base_url=base_url)
elif self.provider == "deepseek":
    deepseek_base_url = base_url or DEEPSEEK_BASE_URL
    self.client = openai.OpenAI(api_key=self.api_key, base_url=deepseek_base_url)
```

#### 2. 环境变量配置问题
**问题**: DeepSeek使用错误的环境变量名

**修复**:
- 更新配置文件中的环境变量名为`DEEPSEEK_API_KEY`
- 支持硬编码API Key作为备选方案
- 添加了provider特定的API Key处理逻辑

#### 3. 流式响应支持缺失
**问题**: DeepSeek不支持流式响应

**修复**: 在`_stream_response()`方法中添加了DeepSeek支持，复用OpenAI兼容接口

## 📚 更多资源

### 官方文档
- [DeepSeek官方文档](https://platform.deepseek.com/docs)
- [DeepSeek API参考](https://platform.deepseek.com/docs/api)
- [Claude API文档](https://docs.anthropic.com/claude/reference)
- [OpenAI API文档](https://platform.openai.com/docs/api-reference)
- [Ollama文档](https://github.com/ollama/ollama)

### Python SDK
- [OpenAI Python SDK](https://github.com/openai/openai-python)
- [Anthropic Python SDK](https://github.com/anthropics/anthropic-sdk-python)

### 最佳实践
- [DeepSeek最佳实践指南](https://platform.deepseek.com/docs/guides)
- [Claude使用指南](https://docs.anthropic.com/claude/docs)
- [OpenAI最佳实践](https://platform.openai.com/docs/guides)

## 🐛 故障排除

### 通用问题

**问题**: `'UniversalTaskAgent' object has no attribute 'client'`
```
✅ 已修复：这个错误在v2.0版本中已经解决
如果仍然遇到，请确保使用最新版本的代码
```

**问题**: API密钥错误或无效
```
解决方案:
1. 检查API密钥是否正确设置
2. 确认API密钥是否有效且有足够余额
3. 检查环境变量设置：echo $DEEPSEEK_API_KEY
```

**问题**: 网络连接超时
```
解决方案:
1. 检查网络连接，确认可以访问api.deepseek.com
2. 设置适当的超时时间
3. 考虑使用代理或VPN
```

**问题**: Token限制超出
```
解决方案:
1. 减少输入文本长度
2. 增加max_tokens参数（最大支持值）
3. 清理对话历史：agent.clear_history()
```

### 模型特定问题

**DeepSeek问题**:
- 依赖包安装失败: `pip install openai`
- API端点错误: 确认使用 https://api.deepseek.com/v1
- 模型名称错误: 使用 "deepseek-chat"

**Claude问题**:
- 依赖包安装失败: `pip install anthropic`
- API密钥格式: 确认使用正确的Anthropic API格式
- 消息格式: Claude对消息格式有特殊要求

**OpenAI问题**:
- API密钥权限: 确认密钥有权限访问指定模型
- 地区限制: 某些地区可能需要特殊网络配置

**Ollama问题**:
- 服务未启动: 运行 `ollama serve`
- 模型未下载: 使用 `ollama pull <model-name>`
- 端口冲突: 检查11434端口是否被占用

## 📄 许可证

MIT License - 可自由使用和修改。

## 💡 各AI服务对比

### 🚀 DeepSeek优势
- **成本效益** - 极具竞争力的价格
- **中文优化** - 原生中文理解和生成
- **快速响应** - 国内服务器，低延迟
- **OpenAI兼容** - 无需修改现有代码

### 🎯 Claude优势
- **推理能力强** - 复杂问题分析
- **安全性高** - 严格的AI安全准则
- **长文本处理** - 支持大篇幅对话
- **创意写作** - 优秀的文本生成能力

### ⚡ OpenAI优势
- **生态成熟** - 丰富的工具和集成
- **模型多样** - GPT-3.5到GPT-4全系列
- **API稳定** - 成熟稳定的服务
- **社区支持** - 庞大的开发者社区

### 🏠 Ollama优势
- **本地部署** - 数据隐私保护
- **免费使用** - 无API调用费用
- **离线工作** - 不依赖网络连接
- **自定义模型** - 支持微调和私有模型

### 📊 选择建议

| 使用场景 | 推荐模型 | 原因 |
|---------|---------|------|
| **中文对话** | DeepSeek | 中文优化，成本低 |
| **代码生成** | Claude/OpenAI | 代码能力强，生态成熟 |
| **创意写作** | Claude | 推理和创造力强 |
| **成本敏感** | DeepSeek/Ollama | 成本最低 |
| **数据隐私** | Ollama | 本地部署，完全私有 |
| **快速原型** | DeepSeek | 响应快，兼容性好 |
| **企业应用** | Claude/OpenAI | 稳定可靠，支持完善 |

## 🎯 项目特色

### 🔄 统一接口
- 一套代码，支持5种AI服务
- 无缝切换不同模型
- 降低学习成本和开发复杂度

### 🛡️ 可靠性
- 完善的错误处理机制
- 自动降级到模拟模式
- 详细的故障排除指南

### ⚡ 高性能
- 流式响应支持
- 对话历史智能管理
- 优化的API调用

### 🎁 易用性
- 预配置的API密钥
- 一键安装和运行
- 丰富的示例代码

---

*🎉 享受使用多模型AI代理SDK构建智能应用的乐趣！*

*如有问题或建议，欢迎提交Issue或Pull Request。* 🤖✨