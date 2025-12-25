# 🤖 Claude Agent SDK 示例项目

一个完整的 Claude Agent SDK 示例项目，展示从基础到高级的各种功能和用法。

## 📋 项目简介

本项目提供了 **7 个渐进式示例**，帮助你全面了解 Claude Agent SDK 的功能：

- ✅ 基础对话和问答
- ✅ 多模型支持和参数配置
- ✅ 工具使用（文件读写、代码搜索等）
- ✅ MCP 协议集成
- ✅ 会话管理和上下文维护
- ✅ 流式响应处理
- ✅ 高级代理应用

## 🚀 快速开始

### 1. 环境要求

- Python 3.10+
- 稳定的网络连接

### 2. 安装依赖

```bash
pip install -r requirements.txt
```

### 3. 配置 API 密钥

复制配置模板并填入你的 API 密钥：

```bash
cp config/.env.example config/.env
```

编辑 `config/.env` 文件：

```bash
# 使用智谱 AI API
ANTHROPIC_API_KEY=your_api_key_here
```

获取 API 密钥: https://open.bigmodel.cn/

### 4. 运行示例

**方式一：使用快速开始菜单**

```bash
python quick_start.py
```

**方式二：直接运行单个示例**

```bash
# 基础对话示例
python examples/01_basic_chat.py

# 多模型支持示例
python examples/02_multi_model.py

# 工具使用示例
python examples/03_tools_usage.py
```

**方式三：批量运行所有示例**

```bash
python run_all_examples.py
```

## 📁 项目结构

```
ClaudeAgentExample/
├── examples/                   # 示例代码目录
│   ├── 01_basic_chat.py       # 基础对话示例
│   ├── 02_multi_model.py      # 多模型支持示例
│   ├── 03_tools_usage.py      # 工具使用示例
│   ├── 04_mcp_integration.py  # MCP 集成示例
│   ├── 05_session_management.py # 会话管理示例
│   ├── 06_stream_response.py  # 流式响应示例
│   └── 07_advanced_agent.py   # 高级代理示例
├── lib/                       # 核心库模块
│   ├── __init__.py
│   ├── config.py              # 配置管理
│   ├── agent_factory.py       # 代理工厂
│   └── utils.py               # 工具函数
├── config/                    # 配置文件目录
│   ├── .env.example          # 环境变量模板
│   └── mcp_config.json       # MCP 服务器配置
├── quick_start.py            # 快速开始入口
├── run_all_examples.py       # 批量运行脚本
├── requirements.txt          # Python 依赖
└── README.md                 # 本文件
```

## 📚 示例说明

### 01. 基础对话示例

**文件**: `examples/01_basic_chat.py`

**功能**:
- 简单的问答对话
- `query()` 函数的基本使用
- 自定义选项（系统提示词、对话轮次）
- 多轮对话演示

**适合**: 初次接触 SDK 的开发者

### 02. 多模型支持示例

**文件**: `examples/02_multi_model.py`

**功能**:
- 不同模型的选择和使用
- 温度参数对比测试
- Token 限制测试
- 系统提示词影响演示

**适合**: 需要调优模型参数的开发者

### 03. 工具使用示例

**文件**: `examples/03_tools_usage.py`

**功能**:
- 文件读写工具 (Read, Write)
- 代码搜索工具 (Grep, Glob)
- 命令执行工具 (Bash)
- 综合文件操作

**适合**: 需要让 AI 操作文件系统的开发者

### 04. MCP 集成示例

**文件**: `examples/04_mcp_integration.py`

**功能**:
- MCP 协议概念介绍
- 文件系统 MCP 服务器配置
- MCP 与工具结合使用

**适合**: 需要扩展 AI 能力的开发者

### 05. 会话管理示例

**文件**: `examples/05_session_management.py`

**功能**:
- 对话历史保存
- 上下文记忆维护
- 会话持久化和恢复
- 自定义会话管理器

**适合**: 需要维护长期对话的开发者

### 06. 流式响应示例

**文件**: `examples/06_stream_response.py`

**功能**:
- 实时流式输出
- 打字机效果
- 流式数据统计
- 流式 vs 非流式对比

**适合**: 需要改善用户体验的开发者

### 07. 高级代理示例

**文件**: `examples/07_advanced_agent.py`

**功能**:
- 代码审查代理
- 文档生成代理
- 任务规划代理
- 交互式教学代理
- 调试助手代理

**适合**: 需要构建复杂 AI 应用的开发者

## 🔧 核心库模块

### Config - 配置管理

```python
from lib.config import get_config

config = get_config()
print(f"API Key: {config.anthropic_api_key}")
print(f"Model: {config.anthropic_model}")
```

### AgentFactory - 代理工厂

```python
from lib.agent_factory import create_agent

# 创建聊天代理
agent = create_agent("chat")

# 创建代码助手
code_agent = create_agent("code")
```

### Utils - 工具函数

```python
from lib.utils import (
    print_message,
    print_cost,
    print_example_header,
)

print_example_header("我的示例", "这是一个演示")
print_cost(0.00123)
```

## ⚙️ 配置说明

### 环境变量

| 变量名 | 说明 | 默认值 |
|--------|------|--------|
| `ANTHROPIC_API_KEY` | Claude API 密钥 | 必填 |
| `ANTHROPIC_BASE_URL` | API 端点 | `https://open.bigmodel.cn/api/anthropic` |
| `ANTHROPIC_MODEL` | 默认模型 | `glm-4.7` |
| `MAX_TOKENS` | 最大 token 数 | `4096` |
| `TEMPERATURE` | 温度参数 | `0.7` |

### ClaudeAgentOptions

```python
from claude_agent_sdk import ClaudeAgentOptions

options = ClaudeAgentOptions(
    system_prompt="系统提示词",
    max_turns=5,              # 最大对话轮次
    allowed_tools=["Read"],   # 允许的工具
    model="glm-4.7",          # 模型名称
)
```

## 🐛 故障排除

### 问题 1: API 密钥错误

```
❌ 错误: 未设置 ANTHROPIC_API_KEY
```

**解决方案**:
1. 确认 `config/.env` 文件存在
2. 检查 API 密钥是否正确填入
3. 确认文件格式正确（KEY=VALUE，无空格）

### 问题 2: 模块导入失败

```
ModuleNotFoundError: No module named 'claude_agent_sdk'
```

**解决方案**:
```bash
pip install -r requirements.txt
```

### 问题 3: MCP 服务器连接失败

```
⚠️ 未安装 mcp-server-filesystem
```

**解决方案**:
```bash
pip install mcp-server-filesystem
```

### 问题 4: 编码错误（Windows）

**解决方案**:
```bash
chcp 65001
set PYTHONIOENCODING=utf-8
```

## 📖 进阶话题

### 创建自定义代理

```python
from lib.agent_factory import AgentFactory
from claude_agent_sdk import ClaudeAgentOptions

class MyCustomAgent(AgentFactory):
    def __init__(self):
        super().__init__()
        self.system_prompt = "你是我的自定义助手..."

    async def process(self, user_input: str) -> str:
        options = ClaudeAgentOptions(
            system_prompt=self.system_prompt,
            allowed_tools=["Read", "Write"],
        )

        return await self.chat_async(
            prompt=user_input,
            options=options,
        )
```

### 扩展工具支持

在 `ClaudeAgentOptions` 中添加自定义工具：

```python
options = ClaudeAgentOptions(
    allowed_tools=[
        "Read", "Write", "Grep",
        "Bash", "Glob",
        # 添加你的自定义工具
    ],
)
```

## 📚 相关资源

- [Claude Agent SDK 官方文档](https://docs.anthropic.com/claude/docs/claude-sdk)
- [智谱 AI 开放平台](https://open.bigmodel.cn/)
- [MCP 协议规范](https://modelcontextprotocol.io/)
- [项目问题反馈](https://github.com/anthropics/claude-sdk/issues)

## 📝 许可证

本项目仅供学习和参考使用。

## 🤝 贡献

欢迎提交问题和改进建议！

---

**Happy Coding! 🎉**
