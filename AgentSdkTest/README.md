# Claude Agent SDK 整合项目

这是一个整合了 **AgentSdkTest** 多模型接口和 **ClaudeAgentExample** 模块化架构的项目，提供统一的AI代理开发框架。

## 核心特性

- **多模型统一接口**: 支持 Claude、OpenAI、DeepSeek、Ollama、Mock 等多种AI提供商
- **模块化架构**: lib/ 核心库 + examples/ 示例代码的清晰结构
- **专业化代理**: 代码助手、任务代理、聊天代理等预设类型
- **工厂模式**: 灵活创建不同类型的代理
- **交互式菜单**: 快速开始向导，方便体验各种功能

## 项目结构

```
AgentSdkTest/
├── lib/                       # 核心库模块
│   ├── __init__.py
│   ├── multi_agent.py         # 多模型统一接口 (核心)
│   ├── agent_factory.py       # 代理工厂
│   ├── config.py              # 配置管理
│   └── utils.py               # 工具函数
├── examples/                  # 示例代码目录
│   ├── 01_basic_chat.py       # 基础对话示例
│   ├── 02_multi_model.py      # 多模型支持示例
│   ├── 03_tools_usage.py      # 工具使用示例
│   ├── 04_mcp_integration.py  # MCP 集成示例
│   ├── 05_session_management.py  # 会话管理示例
│   ├── 06_stream_response.py  # 流式响应示例
│   └── 07_advanced_agent.py   # 高级代理示例
├── config/                    # 配置文件目录
│   ├── .env.example           # 环境变量模板
│   └── mcp_config.json        # MCP 配置
├── quick_start.py             # 交互式快速开始菜单
├── run_all_examples.py        # 批量运行所有示例
├── requirements.txt           # 依赖包列表
├── README.md                  # 本文件
└── CLAUDE.md                  # Claude Code 项目配置
```

## 快速开始

### 1. 安装依赖

```bash
pip install -r requirements.txt
```

### 2. 配置 API 密钥

复制环境变量模板并填入你的 API 密钥：

```bash
cp config/.env.example .env
```

编辑 `.env` 文件，至少配置以下内容：

```bash
# 智谱AI API (主要使用)
ANTHROPIC_API_KEY=your_api_key_here
ANTHROPIC_BASE_URL=https://open.bigmodel.cn/api/anthropic
ANTHROPIC_MODEL=glm-4.7
```

获取密钥: https://open.bigmodel.cn/

### 3. 运行示例

#### 方式一：使用交互式菜单（推荐）

```bash
python quick_start.py
```

#### 方式二：直接运行示例

```bash
# 多模型支持示例（核心功能）
python examples/02_multi_model.py

# 其他示例
python examples/01_basic_chat.py
python examples/03_tools_usage.py
```

#### 方式三：批量运行所有示例

```bash
python run_all_examples.py
```

## 核心功能

### 1. 多模型统一接口

```python
from lib.multi_agent import UniversalAIAgent

# 创建代理（支持多种提供商）
agent = UniversalAIAgent(provider="claude", model="glm-4.7")

# 同步对话
response = agent.chat("你好，请介绍一下你自己")

# 流式响应
response = agent.chat("写一个故事", stream=True)
```

### 2. 专业化代理

```python
from lib.multi_agent import UniversalCodeAgent, UniversalTaskAgent

# 代码助手
code_agent = UniversalCodeAgent(provider="claude", language="Python")
code_agent.write_code("实现一个快速排序算法")

# 任务代理
task_agent = UniversalTaskAgent(provider="claude", task_description="帮助用户解决问题")
task_agent.solve_problem("如何解决 Python 的 IndentationError？")
```

### 3. 代理工厂模式

```python
from lib.agent_factory import AgentFactory, create_multi_agent

# 使用工厂创建代理
factory = AgentFactory()

# 创建多模型代理
agent = factory.create_multi_model_agent(provider="claude")

# 创建代码代理
code_agent = factory.create_code_agent_multi(provider="claude", language="Python")

# 便捷函数
agent = create_multi_agent(agent_type="code", provider="claude")
```

### 4. 支持的模型提供商

| 提供商 | 模型 | 说明 |
|--------|------|------|
| Claude | glm-4.7, glm-4.6 | 智谱AI（主要使用） |
| OpenAI | gpt-4o-mini, gpt-4 | OpenAI GPT 系列 |
| DeepSeek | deepseek-chat | DeepSeek AI |
| Ollama | llama2, mistral | 本地模型 |
| Mock | mock-model | 测试用（无需API密钥） |

## 示例说明

### 基础示例 (01_basic_chat.py)
展示 Claude Agent SDK 的基础对话功能

### 多模型支持 (02_multi_model.py)
展示统一的多模型接口，这是整合后的核心功能

### 工具使用 (03_tools_usage.py)
展示文件读写、代码搜索等工具功能

### MCP 集成 (04_mcp_integration.py)
展示 Model Context Protocol 服务器集成

### 会话管理 (05_session_management.py)
展示对话历史和上下文管理

### 流式响应 (06_stream_response.py)
展示实时流式输出处理

### 高级代理 (07_advanced_agent.py)
展示专业化代理和工厂模式的高级用法

## 配置选项

### ClaudeAgentOptions

```python
from claude_agent_sdk import ClaudeAgentOptions

options = ClaudeAgentOptions(
    system_prompt="系统提示词",
    max_turns=5,                      # 最大对话轮次
    allowed_tools=["Read", "Write"],  # 允许使用的工具
    model="glm-4.7",                  # 使用的模型
)
```

### 多模型配置

```python
# 配置不同的提供商
agent = UniversalAIAgent(
    provider="claude",         # 提供商
    model="glm-4.7",           # 模型名称
    api_key="your_api_key",    # API密钥（可选，默认从环境变量读取）
    base_url="custom_url"      # 自定义API端点（可选）
)
```

## 开发指南

### 创建自定义代理

```python
from lib.multi_agent import UniversalAIAgent

class CustomAgent(UniversalAIAgent):
    def __init__(self, **kwargs):
        super().__init__(**kwargs)
        self.add_system_prompt("你是一个自定义助手...")
```

### 使用配置管理

```python
from lib.config import get_config

config = get_config()

# 访问配置
print(config.anthropic_api_key)
print(config.anthropic_model)

# 获取提供商配置
claude_config = config.get_provider_config("claude")
```

## 故障排除

### API 密钥错误

**错误**: `Invalid API key`

**解决方案**:
1. 检查 `.env` 文件是否存在
2. 确认 API 密钥格式正确
3. 验证环境变量是否正确加载

### 依赖包问题

**错误**: `ModuleNotFoundError: No module named 'claude_agent_sdk'`

**解决方案**:
```bash
pip install -r requirements.txt
```

### MCP 服务器问题

**错误**: MCP 连接失败

**解决方案**:
```bash
# 安装 MCP 文件系统服务器
pip install mcp-server-filesystem
```

## 相关资源

- [Claude Agent SDK 官方文档](https://docs.anthropic.com/claude/docs/claude-sdk)
- [智谱AI API 文档](https://open.bigmodel.cn/dev/api)
- [OpenAI API 文档](https://platform.openai.com/docs)
- [项目整合说明](CLAUDE.md)

## 版本历史

### v2.0.0 (当前版本)
- 整合 AgentSdkTest 多模型接口
- 采用 ClaudeAgentExample 模块化架构
- 新增交互式快速开始菜单
- 支持 5+ 种 AI 提供商
- 7 个渐进式示例

### v1.0.0 (AgentSdkTest)
- 基础多模型支持
- 扁平化测试文件结构

---

*最后更新: 2025-01-05*
