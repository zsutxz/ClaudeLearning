# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 语言和Git规则
- **语言**：使用中文进行交流
- **Git规则**：不自动提交任何代码更改，手动提交前需要明确确认

## 🎯 Project Overview
这是一个**整合项目**，结合了 AgentSdkTest 的多模型接口和 ClaudeAgentExample 的模块化架构，提供统一的AI代理开发框架。

## 🏗️ 项目架构

### 核心目录结构
```
AgentSdkTest/
├── lib/                       # 核心库模块
│   ├── __init__.py
│   ├── multi_agent.py         # 多模型统一接口 (核心功能)
│   ├── factory.py             # 代理工厂 (AgentFactory)
│   ├── multi_agent_system.py  # 多智能体协作系统
│   ├── config.py              # 配置管理
│   ├── example_helpers.py     # 示例公共辅助函数
│   └── utils.py               # 工具函数
├── examples/                  # 示例代码目录
│   ├── claude_agent_sdk/      # Claude Agent SDK 功能测试
│   │   ├── TestBasicChat.py       # 基础对话
│   │   ├── TestAgentSdk.py        # SDK 综合测试
│   │   ├── TestMcpIntegration.py  # MCP 集成
│   │   ├── TestTool.py            # 工具使用
│   │   ├── TestHook.py            # Hook 功能
│   │   ├── TestSlash.py           # Slash 命令
│   │   ├── TestSkill.py           # Skill 功能
│   │   └── TestTodos.py           # Todos 功能
│   └── universal_agent/       # UniversalAIAgent 渐进示例
│       ├── 01_test_deepseek.py        # DeepSeek 测试
│       ├── 02_multi_model.py          # 多模型支持示例 (核心)
│       ├── 03_session_management.py   # 会话管理示例
│       ├── 04_stream_response.py      # 流式响应示例
│       ├── 05_advanced_agent.py       # 高级代理示例
│       └── 06_multi_agent_system.py   # 多智能体协作示例
├── mcp_servers/               # 自带 MCP 服务器（agent_bridge 等）
├── config/                    # 配置文件目录
│   ├── .env.example           # 环境变量模板
│   └── mcp_config.json        # MCP 配置
├── quick_start.py             # 交互式快速开始菜单
├── run_all_examples.py        # 批量运行所有示例
├── requirements.txt           # 依赖包列表
├── README.md                  # 项目说明文档
└── CLAUDE.md                  # 本文件
```

### 技术栈
- **编程语言**: Python 3.10+
- **核心SDK**: `claude-agent-sdk` (Anthropic官方)
- **主要模型**: GLM-4.7 (智谱AI)
- **多模型支持**:
  - Claude (glm-4.7, glm-4.6 - 智谱AI)
  - OpenAI (gpt-4o-mini, gpt-4)
  - DeepSeek (deepseek-chat, deepseek-coder)
  - Ollama (llama2, mistral - 本地模型)
  - Mock (测试用)

### 架构特点
- **多模型统一接口**: UniversalAIAgent 提供一致的API接口
- **模块化设计**: lib/ 核心库 + examples/ 示例代码
- **专业化代理**: UniversalCodeAgent、UniversalTaskAgent 等
- **工厂模式**: AgentFactory 提供灵活的代理创建方式
- **多智能体协作**: multi_agent_system.py 提供通信总线与协调器
- **MCP协议集成**: 支持 Model Context Protocol 服务器

## ⚙️ 开发环境配置

### 依赖安装
```bash
# 安装核心依赖
pip install -r requirements.txt

# MCP 文件系统服务器（可选，无需 pip 安装）
# 由 Claude Code 通过 .mcp.json 配置，npx 自动拉取 npm 包
# @modelcontextprotocol/server-filesystem（官方维护，非 pip 包）
```

### 环境变量配置
复制 `config/.env.example` 为 `.env` 并配置API密钥：
```bash
# 智谱AI API (主要使用)
ANTHROPIC_API_KEY=your_glm_api_key_here
ANTHROPIC_BASE_URL=https://open.bigmodel.cn/api/anthropic
ANTHROPIC_MODEL=glm-4.7

# OpenAI API (可选)
OPENAI_API_KEY=your_openai_api_key_here
OPENAI_BASE_URL=https://api.openai.com/v1

# DeepSeek API (可选)
DEEPSEEK_API_KEY=your_deepseek_api_key_here
DEEPSEEK_BASE_URL=https://api.deepseek.com/v1
```

### MCP服务器配置
项目支持 MCP 文件系统服务器：
- `config/mcp_config.json`: MCP 服务器配置

## 🚀 常用命令

### 快速开始
```bash
# 使用交互式菜单 (推荐)
python quick_start.py

# 直接运行示例
python examples/universal_agent/02_multi_model.py  # 多模型支持示例

# 批量运行所有示例
python run_all_examples.py
```

### 核心功能测试
```bash
# 多模型接口测试
python examples/universal_agent/02_multi_model.py

# 代码/高级代理测试
python examples/universal_agent/05_advanced_agent.py

# 会话管理测试
python examples/universal_agent/03_session_management.py

# Claude Agent SDK 基础对话
python examples/claude_agent_sdk/TestBasicChat.py
```

## 📝 核心功能模块

### 1. 多模型统一接口 (lib/multi_agent.py)
- **UniversalAIAgent**: 统一的AI代理接口，支持多种模型
- **UniversalTaskAgent**: 任务型代理，专注于特定任务执行
- **UniversalCodeAgent**: 代码助手代理，支持代码生成和分析
- 支持同步和流式响应
- 对话历史管理和上下文维护

### 2. 代理工厂 (lib/factory.py)
- **AgentFactory**: 工厂类，封装代理创建逻辑
- **create_multi_agent**: 便捷函数，创建多模型代理
- **create_chat_agent**: 创建聊天代理
- **create_code_agent**: 创建代码代理
- **create_task_agent**: 创建任务代理
- **create_file_agent**: 创建文件操作代理

### 3. 多智能体协作 (lib/multi_agent_system.py)
- **AgentMessage**: 智能体间消息格式
- **AgentCommunicationBus**: 通信总线
- **AgentCoordinator**: 智能体协调器，负责任务分发
- **MultiAgentSystem**: 多智能体系统高层接口

### 4. 配置管理 (lib/config.py)
- **Config**: 配置数据类
- **get_config**: 获取全局配置实例
- **load_env_file**: 从.env文件加载环境变量
- 支持多种AI提供商的配置管理

### 5. 示例代码 (examples/)
- `examples/universal_agent/`: 6 个 UniversalAIAgent 渐进示例（DeepSeek 测试 → 多模型 → 会话 → 流式 → 高级代理 → 多智能体）
- `examples/claude_agent_sdk/`: 8 个 Claude Agent SDK 功能测试（基础对话、SDK 综合、MCP、工具、Hook、Slash、Skill、Todos）
- 可独立运行，也可通过 `quick_start.py` 统一菜单运行

## 🔧 核心配置选项

### UniversalAIAgent 配置
```python
from lib.multi_agent import UniversalAIAgent

# 创建代理实例
agent = UniversalAIAgent(
    provider="claude",  # 选择模型提供商
    model="glm-4.7",    # 指定模型
    api_key="your_api_key",  # API密钥（可选）
    base_url="custom_url"    # 自定义端点（可选）
)

# 发送消息
response = agent.chat("你好，请介绍一下自己")
```

### ClaudeAgentOptions 配置
```python
from claude_agent_sdk import ClaudeAgentOptions

options = ClaudeAgentOptions(
    system_prompt="系统提示词",
    max_turns=1,  # 最大对话轮次
    allowed_tools=["Read", "Write", "Grep"],  # 允许使用的工具
    model="glm-4.7",  # 使用的模型
)
```

## 🎮 开发工作流

### 新功能开发模式
1. **创建示例文件**: 在 `examples/universal_agent/` 或 `examples/claude_agent_sdk/` 创建新示例
2. **使用核心库**: 从 `lib/` 导入所需模块
3. **测试验证**: 使用 `quick_start.py` 或直接运行
4. **文档更新**: 更新 README.md 和 CLAUDE.md

### 多模型使用流程
1. **选择提供商**: 从支持的提供商中选择（claude, openai, deepseek, ollama, mock）
2. **配置API密钥**: 在 `.env` 中配置对应的环境变量
3. **创建代理**: 使用 UniversalAIAgent 或工厂函数创建
4. **调用API**: 使用 chat() 方法进行对话

### 自定义代理开发
1. **继承基类**: 继承 UniversalAIAgent
2. **添加系统提示词**: 在 __init__ 中调用 add_system_prompt()
3. **实现专用方法**: 添加领域特定的方法
4. **测试验证**: 创建测试用例验证功能

## ⚠️ 重要注意事项

### 安全性配置
- **API密钥管理**: 所有API密钥通过 `.env` 文件管理，已添加到 `.gitignore`
- **文件访问限制**: MCP服务器配置了访问权限限制
- **依赖安全**: 使用最小依赖原则
- **代码安全**: 避免硬编码敏感信息

### 系统兼容性
- **Python版本**: 支持 Python 3.10+
- **跨平台**: 兼容 Windows/Linux/macOS
- **依赖管理**: 使用 `requirements.txt` 管理依赖版本
- **可选依赖**: 通过 try-except 处理可选依赖

### 性能优化策略
- **流式响应**: 支持流式输出处理
- **进程隔离**: MCP服务器提供安全的进程隔离
- **上下文管理**: 对话历史管理支持长度限制
- **资源清理**: 实现适当的资源清理

## 📋 故障排除指南

### 常见问题及解决方案

#### 1. API连接问题
**症状**: API密钥错误、连接超时
**解决方案**:
```bash
# 检查API密钥配置
cat .env | grep API_KEY

# 测试基础功能
python examples/universal_agent/02_multi_model.py
```

#### 2. 依赖包问题
**症状**: ModuleNotFoundError、版本冲突
**解决方案**:
```bash
# 重新安装依赖
pip install -r requirements.txt
```

#### 3. MCP服务器问题
**症状**: MCP连接失败、文件访问错误
**解决方案**:
- 检查 `.mcp.json` 是否配置了 `@modelcontextprotocol/server-filesystem`（通过 `npx -y` 运行，无需 pip）
- 确认本机已安装 Node.js / npx（filesystem server 是 npm 包，不是 pip 包）
- 清理 npx 缓存重试：`npx clear-npx-cache`

## 📚 相关资源

### 官方文档
- [Claude Agent SDK 官方文档](https://docs.anthropic.com/claude/docs/claude-sdk)
- [Model Context Protocol 规范](https://modelcontextprotocol.io/)
- [智谱AI API 文档](https://open.bigmodel.cn/dev/api)

### 项目参考
- [lib/multi_agent.py](./lib/multi_agent.py) - 多模型统一接口
- [lib/factory.py](./lib/factory.py) - 代理工厂
- [lib/multi_agent_system.py](./lib/multi_agent_system.py) - 多智能体协作系统
- [examples/universal_agent/02_multi_model.py](./examples/universal_agent/02_multi_model.py) - 多模型示例
- [README.md](./README.md) - 项目说明文档

---

*Claude Agent SDK 整合项目 - 多模型支持 + 模块化架构*
