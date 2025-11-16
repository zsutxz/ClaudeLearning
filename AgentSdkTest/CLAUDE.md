# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Git规则
- **不自动提交** - Claude不会自动提交任何代码更改
- 手动提交前需要明确确认

## 🎯 Project Overview
这是一个 **Claude Agent SDK 测试项目**，专注于演示和测试 Anthropic Claude Agent SDK 的各种功能，包括多模型支持、工具调用、MCP服务器集成、技能系统等高级特性。

## 🏗️ 项目架构

### 核心目录结构
```
D:\work\AI\ClaudeTest\AgentSdkTest\
├── .claude/                    # Claude配置目录
│   └── settings.local.json     # Claude本地MCP服务器配置
├── Prompt/                     # 多模型测试目录
│   ├── TestPrompt.py          # 通用AI代理测试
│   ├── TestDeepseek.py        # DeepSeek API测试
│   └── .env                   # 环境变量配置
├── AgentSdkStart.py           # SDK快速开始示例
├── Multi_LLm.py               # 多模型支持实现
├── TestMcp.py                 # MCP服务器集成测试
├── TestTool.py                # 自定义工具创建和调用
├── TestSkill.py               # 技能系统测试
├── TestSlash.py               # Slash命令系统测试
├── TestAgent.py               # 代理功能测试
├── TestConversationSession.py # 持久化对话会话测试
├── TestHook.py                # Hook功能测试
├── TestTodos.py               # 待办事项系统测试
├── .mcp.json                  # MCP服务器配置
├── requirements.txt           # Python依赖
└── .env                       # 环境变量配置
```

### 技术栈
- **编程语言**: Python 3.13
- **核心SDK**: `claude-agent-sdk` (Anthropic官方)
- **AI模型支持**:
  - Anthropic Claude (claude-3-5-sonnet, claude-3-haiku, claude-3-opus)
  - OpenAI GPT (gpt-3.5-turbo, gpt-4, gpt-4-turbo-preview)
  - DeepSeek (deepseek-chat)
  - 本地模型 (Ollama: llama2, mistral, codellama, phi)
  - Mock模型 (用于测试)

## ⚙️ 开发环境配置

### 依赖安装
```bash
# 安装核心依赖
pip install -r requirements.txt

# 可选：安装python-dotenv以支持.env文件
pip install python-dotenv

# 可选：安装MCP文件系统服务器
pip install mcp-server-filesystem
```

### API密钥配置
在 `.env` 文件中配置所需的API密钥：
```bash
ANTHROPIC_API_KEY=your_anthropic_api_key_here
OPENAI_API_KEY=your_openai_api_key_here
# DEEPSEEK_API_KEY=your_deepseek_api_key_here  # 注意：当前硬编码在Multi_LLm.py中
```

### MCP服务器配置
项目已配置文件系统MCP服务器：
- `.mcp.json`: 定义MCP服务器配置
- `.claude/settings.local.json`: Claude本地配置，启用文件系统MCP服务器

## 🚀 常用命令

### 基础功能测试
```bash
# 快速开始示例
python AgentSdkStart.py

# 多模型支持测试
python Multi_LLm.py

# 基础代理功能测试
python TestAgent.py
```

### 高级功能测试
```bash
# MCP服务器集成测试
python TestMcp.py

# 自定义工具测试
python TestTool.py

# 技能系统测试
python TestSkill.py

# Slash命令测试
python TestSlash.py

# 持久化对话会话测试
python TestConversationSession.py

# Hook功能测试
python TestHook.py

# 待办事项系统测试
python TestTodos.py
```

### 多模型测试
```bash
# 通用AI代理测试
cd Prompt && python TestPrompt.py

# DeepSeek模型测试
cd Prompt && python TestDeepseek.py
```

## 📝 核心功能模块

### 1. 多模型支持 (Multi_LLm.py)
- **UniversalAIAgent**: 统一的AI代理接口
- 支持Claude、OpenAI、DeepSeek、Ollama、Mock模型
- 流式和同步响应支持
- 对话历史管理

### 2. 专业化代理
- **UniversalTaskAgent**: 任务型代理
- **UniversalCodeAgent**: 代码助手代理
- **UniversalTalkAgent**: 对话型代理

### 3. 工具系统 (TestTool.py)
- 自定义工具创建和注册
- 工具调用和参数处理
- 与MCP服务器的集成

### 4. MCP集成 (TestMcp.py)
- Model Context Protocol服务器配置
- 文件系统服务器集成
- 进程隔离的工具执行

### 5. 技能系统 (TestSkill.py)
- 技能定义和执行
- 技能参数管理
- 技能链式调用

### 6. Slash命令 (TestSlash.py)
- 自定义Slash命令定义
- 命令参数解析
- 交互式命令执行

## 🔧 配置选项

### ClaudeAgentOptions
```python
options = ClaudeAgentOptions(
    system_prompt="系统提示词",
    max_turns=1,  # 最大对话轮次
    allowed_tools=["Read", "Write"],  # 允许使用的工具
    model="claude-3-haiku-20240307",  # 使用的模型
    mcp_servers={  # MCP服务器配置
        "filesystem": {
            "command": "python",
            "args": ["-m", "mcp_server_filesystem"],
            "env": {"ALLOWED_PATHS": "./"}
        }
    }
)
```

## 🎮 开发工作流

### 新功能开发
1. **创建测试脚本**: 基于现有Test*.py模板创建新测试
2. **配置环境变量**: 在.env中添加所需配置
3. **实现功能**: 参考现有模块实现新功能
4. **测试验证**: 运行对应测试脚本验证功能

### 多模型集成
1. **扩展UniversalAIAgent**: 在SUPPORTED_PROVIDERS中添加新模型
2. **配置API密钥**: 在.env中添加对应环境变量
3. **测试兼容性**: 使用Prompt/TestPrompt.py验证新模型

### 自定义工具开发
1. **定义工具函数**: 使用@tool装饰器创建工具
2. **注册工具**: 在ClaudeAgentOptions中添加工具
3. **测试工具**: 使用TestTool.py验证工具功能

## ⚠️ 注意事项

### 安全性
- API密钥通过.env文件管理，已在.gitignore中排除
- DeepSeek API密钥当前硬编码在Multi_LLm.py中，需要移到环境变量
- MCP服务器配置了文件访问权限限制

### 兼容性
- 支持Python 3.13+
- 兼容Windows/Linux/macOS
- 依赖包版本要求见requirements.txt

### 性能优化
- 使用流式响应处理长对话
- MCP服务器提供进程隔离的工具执行
- 对话历史管理支持上下文限制

## 📋 故障排除

### 常见问题
1. **API密钥错误**: 检查.env文件或环境变量配置
2. **依赖包缺失**: 运行`pip install -r requirements.txt`
3. **MCP服务器连接失败**: 检查.mcp.json配置和python环境
4. **模型不支持**: 确认模型名称在SUPPORTED_PROVIDERS中

### 调试技巧
- 查看测试脚本输出的详细错误信息
- 检查.env文件格式和权限
- 验证MCP服务器配置和依赖安装

---
*Claude Agent SDK 测试项目 - 专注于多模型集成和高级功能演示*