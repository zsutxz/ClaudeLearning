# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 语言和Git规则
- **语言**：使用中文进行交流
- **Git规则**：不自动提交任何代码更改，手动提交前需要明确确认

## 🎯 Project Overview
这是一个**Claude Agent SDK 测试项目**，专注于演示和测试 Anthropic Claude Agent SDK 的各种功能，包括多模型支持、工具调用、MCP服务器集成、技能系统等高级特性。

## 🏗️ 项目架构

### 核心目录结构
```
E:\AI\ClaudeLearning\AgentSdkTest\
├── .claude/                    # Claude配置目录
│   └── settings.local.json     # Claude本地MCP服务器配置
├── Prompt/                     # 多模型测试目录
│   ├── TestPrompt.py          # 通用AI代理测试
│   ├── TestDeepseek.py        # DeepSeek API测试
│   └── .env                   # 环境变量配置
├── MultiAIAgent.py            # 核心多模型代理实现
├── TestAgentSdk.py            # SDK基础功能测试
├── TestMcp.py                 # MCP服务器集成测试
├── TestTool.py                # 自定义工具创建和调用
├── TestSkill.py               # 技能系统测试
├── TestSlash.py               # Slash命令系统测试
├── TestConversationSession.py # 持久化对话会话测试
├── TestHook.py                # Hook功能测试
├── TestTodos.py               # 待办事项系统测试
├── TestDeepseek.py            # DeepSeek模型测试
├── TestMultiLlm.py            # 多模型测试
├── TestPrompt.py              # Prompt测试
├── .mcp.json                  # MCP服务器配置
├── requirements.txt           # Python依赖
├── .env.example              # 环境变量模板
└── README.md                 # 项目说明文档
```

### 技术栈
- **编程语言**: Python 3.13+
- **核心SDK**: `claude-agent-sdk` (Anthropic官方，实际使用glm-4.6模型)
- **AI模型支持**:
  - GLM模型 (glm-4.6 - 主要使用模型)
  - Anthropic Claude (claude-4-haiku, claude-4-opus)
  - OpenAI GPT (gpt-3.5-turbo, gpt-4, gpt-4-turbo-preview)
  - DeepSeek (deepseek-chat)
  - 本地模型 (Ollama: llama2, mistral, codellama, phi)
  - Mock模型 (用于测试)

### 架构特点
- **统一多模型接口**: UniversalAIAgent提供一致的API接口
- **模块化测试**: 每个功能模块有独立的测试文件
- **MCP协议集成**: 支持Model Context Protocol服务器
- **自定义API端点**: 使用智谱AI的API端点

## ⚙️ 开发环境配置

### 依赖安装
```bash
# 安装核心依赖（仅4个包）
pip install -r requirements.txt

# 可选依赖
pip install python-dotenv         # .env文件支持
pip install mcp-server-filesystem # MCP文件服务器
```

### 环境变量配置
复制 `.env.example` 为 `.env` 并配置API密钥：
```bash
# 智谱AI API (主要使用)
ANTHROPIC_API_KEY=your_glm_api_key_here
ANTHROPIC_BASE_URL=https://open.bigmodel.cn/api/anthropic

# OpenAI API
OPENAI_API_KEY=your_openai_api_key_here
OPENAI_BASE_URL=https://api.openai.com/v1

# DeepSeek API
DEEPSEEK_API_KEY=your_deepseek_api_key_here
DEEPSEEK_BASE_URL=https://api.deepseek.com/v1
```

### MCP服务器配置
项目已配置文件系统MCP服务器：
- `.mcp.json`: 定义MCP服务器配置，限制文件访问权限
- `.claude/settings.local.json`: Claude本地配置，启用filesystem MCP服务器

## 🚀 常用命令

### 环境设置
```bash
# 1. 配置环境变量
cp .env.example .env
# 编辑.env文件，填入API密钥

# 2. 安装依赖（仅需4个核心包）
pip install -r requirements.txt

# 3. 验证环境
python TestAgentSdk.py
```

### 基础功能测试
```bash
# SDK基础功能测试
python TestAgentSdk.py

# 多模型支持测试
python MultiAIAgent.py

# 通用AI代理测试
python TestPrompt.py
```

### 专项功能测试
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
# 多模型通用测试
python TestMultiLlm.py

# DeepSeek模型测试
python TestDeepseek.py
```

### 完整功能测试
```bash
# 运行所有测试（推荐）
for file in Test*.py; do echo "=== $file ==="; python "$file"; echo; done
```

## 📝 核心功能模块

### 1. 多模型支持 (MultiAIAgent.py)
- **UniversalAIAgent**: 统一的AI代理接口，支持多种AI模型
- 支持GLM、Claude、OpenAI、DeepSeek、Ollama、Mock模型
- 流式和同步响应支持
- 对话历史管理和上下文维护
- 自适应API端点配置

### 2. 专业化代理架构
- **UniversalTaskAgent**: 任务型代理，专注于特定任务执行
- **UniversalCodeAgent**: 代码助手代理，支持代码生成和分析
- **UniversalTalkAgent**: 对话型代理，优化交互体验

### 3. 工具系统 (TestTool.py)
- 自定义工具创建和注册机制
- 工具调用和参数验证处理
- 与MCP服务器的深度集成
- 工具执行结果处理

### 4. MCP协议集成 (TestMcp.py)
- Model Context Protocol服务器配置和管理
- 文件系统服务器集成，提供安全的文件访问
- 进程隔离的工具执行环境
- MCP客户端-服务器通信测试

### 5. 技能系统 (TestSkill.py)
- 技能定义和动态执行框架
- 技能参数管理和验证
- 技能链式调用和组合
- 可扩展的技能生态

### 6. Slash命令系统 (TestSlash.py)
- 自定义Slash命令定义和解析
- 命令参数验证和处理
- 交互式命令执行界面
- 命令结果格式化输出

### 7. 会话管理 (TestConversationSession.py)
- 持久化对话会话支持
- 会话状态管理
- 上下文历史维护
- 会话恢复和清理

### 8. Hook系统 (TestHook.py)
- 事件驱动的Hook机制
- 自定义Hook注册和执行
- Hook链式处理
- 系统事件拦截和处理

### 9. 待办事项系统 (TestTodos.py)
- 任务创建和管理
- 任务状态跟踪
- 优先级和分类管理
- 任务完成状态通知

## 🔧 核心配置选项

### UniversalAIAgent 配置
```python
from MultiAIAgent import UniversalAIAgent

# 创建代理实例
agent = UniversalAIAgent(
    provider="claude",  # 选择模型提供商
    model="glm-4.6",    # 指定模型
    api_key="your_api_key"
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
    model="glm-4.6",  # 使用的模型
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

### 新功能开发模式
1. **创建测试脚本**: 基于`Test*.py`模板创建新测试文件
2. **配置环境变量**: 在`.env`中添加所需配置项
3. **实现核心功能**: 参考现有模块架构实现新功能
4. **编写测试用例**: 创建对应的测试验证功能
5. **集成测试**: 运行完整测试套件验证集成

### 多模型集成流程
1. **扩展SUPPORTED_PROVIDERS**: 在`MultiAIAgent.py`中添加新模型配置
2. **配置API密钥**: 在`.env`中添加对应环境变量
3. **测试模型兼容性**: 使用`TestMultiLlm.py`验证新模型
4. **文档更新**: 更新CLAUDE.md中的模型支持说明

### 自定义工具开发
1. **定义工具函数**: 使用适当的装饰器或接口创建工具
2. **参数验证**: 实现输入参数的类型和安全检查
3. **注册工具**: 在代理配置中添加工具到允许列表
4. **测试工具**: 使用`TestTool.py`验证工具功能和安全性
5. **错误处理**: 实现完善的异常处理和错误报告

### MCP服务器扩展
1. **定义MCP服务**: 在`.mcp.json`中配置新的MCP服务器
2. **权限配置**: 设置适当的文件访问和执行权限
3. **客户端集成**: 在代理代码中集成新的MCP客户端
4. **通信测试**: 使用`TestMcp.py`验证MCP通信功能

## ⚠️ 重要注意事项

### 安全性配置
- **API密钥管理**: 所有API密钥通过`.env`文件管理，已添加到`.gitignore`
- **文件访问限制**: MCP服务器配置了`ALLOWED_PATHS`，限制文件系统访问范围
- **依赖安全**: 使用最小依赖原则，仅包含4个核心Python包
- **代码安全**: 避免硬编码敏感信息，使用环境变量管理配置

### 系统兼容性
- **Python版本**: 支持Python 3.13+
- **跨平台**: 兼容Windows/Linux/macOS
- **依赖管理**: 使用`requirements.txt`管理核心依赖版本
- **可选依赖**: 通过try-except处理可选依赖，确保系统鲁棒性

### 性能优化策略
- **流式响应**: 支持流式输出处理，优化长对话体验
- **进程隔离**: MCP服务器提供安全的进程隔离工具执行环境
- **上下文管理**: 对话历史管理支持上下文长度限制
- **资源清理**: 实现适当的资源清理和内存管理

## 📋 故障排除指南

### 常见问题及解决方案

#### 1. API连接问题
**症状**: API密钥错误、连接超时、模型不可用
**解决方案**:
```bash
# 检查API密钥配置
cat .env | grep API_KEY

# 验证网络连接
curl -I https://open.bigmodel.cn/api/anthropic

# 测试基础功能
python TestAgentSdk.py
```

#### 2. 依赖包问题
**症状**: ModuleNotFoundError、版本冲突
**解决方案**:
```bash
# 重新安装依赖
pip uninstall -y anthropic openai requests
pip install -r requirements.txt

# 检查包版本
pip list | grep -E "(anthropic|openai|requests)"
```

#### 3. MCP服务器问题
**症状**: MCP连接失败、文件访问错误
**解决方案**:
```bash
# 验证MCP服务器配置
python -c "import json; print(json.load(open('.mcp.json')))"

# 检查文件系统权限
ls -la .claude/settings.local.json

# 测试MCP功能
python TestMcp.py
```

#### 4. 模型兼容性问题
**症状**: 模型名称错误、提供商不支持
**解决方案**:
```bash
# 查看支持的模型
python -c "from MultiAIAgent import UniversalAIAgent; print(UniversalAIAgent.SUPPORTED_PROVIDERS)"

# 测试特定模型
python TestMultiLlm.py
```

### 调试技巧和最佳实践

#### 调试模式
```bash
# 启用详细日志
export PYTHONPATH=.
python -v TestAgentSdk.py

# 单步调试
python -m pdb TestAgentSdk.py
```

#### 环境验证
```bash
# 检查Python环境
python --version
python -c "import sys; print(sys.path)"

# 验证关键模块
python -c "import anthropic, openai; print('Dependencies OK')"
```

#### 配置文件验证
```bash
# 验证JSON配置
python -m json.tool .mcp.json
python -m json.tool .claude/settings.local.json

# 检查环境变量
python -c "import os; print({k:v for k,v in os.environ.items() if 'API' in k or 'BASE_URL' in k})"
```

## 📚 相关资源

### 官方文档
- [Claude Agent SDK 官方文档](https://docs.anthropic.com/claude/docs/claude-sdk)
- [Model Context Protocol 规范](https://modelcontextprotocol.io/)
- [智谱AI API 文档](https://open.bigmodel.cn/dev/api)

### 项目参考
- [MultiAIAgent 源码](./MultiAIAgent.py) - 多模型统一接口实现
- [测试用例集合](./Test*.py) - 完整的功能测试示例
- [配置文件示例](./.env.example) - 环境变量配置模板

### 社区资源
- [GitHub Issues](https://github.com/anthropics/claude-sdk/issues) - 问题反馈和讨论
- [Stack Overflow](https://stackoverflow.com/questions/tagged/claude-api) - 技术问答

---

*Claude Agent SDK 测试项目 - 专注于多模型集成和高级功能演示*

*最后更新: 2025-01-05*