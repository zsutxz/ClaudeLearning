# Agent SDK 桥接 MCP 服务器

将 Claude Agent SDK 的多模型能力暴露为 MCP 工具，使 Claude Code 可以直接调用。

## 📁 文件位置

```
AgentSdkTest/mcp_servers/
├── agent_bridge.py          # Agent SDK 桥接服务器
├── doc_processor_server.py  # 文档处理服务器
├── config.py                # 配置模块
└── __init__.py              # 包初始化
```

## 🔧 MCP 配置

已在 `.mcp.json` 中配置：

```json
{
  "agent-sdk-bridge": {
    "command": "python",
    "args": ["-m", "mcp_servers.agent_bridge"],
    "cwd": "D:\\work\\AI\\ClaudeLearning",
    "env": {
      "PYTHONPATH": "D:\\work\\AI\\ClaudeLearning;D:\\work\\AI\\ClaudeLearning\\AgentSdkTest"
    }
  }
}
```

## 🛠️ 可用工具

| 工具 | 描述 | 参数 |
|------|------|------|
| `list_providers` | 列出支持的模型提供商 | 无 |
| `create_agent` | 创建新的代理实例 | provider, agent_type, model |
| `chat` | 发送消息给代理 | message, provider, agent_id?, model? |
| `code_assistant` | 代码助手（解释/审查/调试/优化） | code, language, task, provider |
| `task_agent` | 任务执行代理 | task, provider |
| `list_agents` | 列出所有活跃代理 | 无 |
| `get_conversation` | 获取对话历史 | agent_id |
| `delete_agent` | 删除代理实例 | agent_id |
| `multi_model_compare` | 多模型对比 | message, providers[] |

## 📖 使用示例

### 1. 列出支持的提供商

```python
# 在 Claude Code 中调用
mcp__agent_sdk_bridge__list_providers()
```

### 2. 创建代理

```python
mcp__agent_sdk_bridge__create_agent(
    provider="claude",
    agent_type="code",
    model="glm-4.7"
)
# 返回: {"agent_id": "code_claude_1234567890", ...}
```

### 3. 聊天对话

```python
# 使用现有代理
mcp__agent_sdk_bridge__chat(
    message="解释一下 Python 的装饰器",
    agent_id="code_claude_1234567890"
)

# 或创建临时代理
mcp__agent_sdk_bridge__chat(
    message="你好",
    provider="claude"
)
```

### 4. 代码助手

```python
mcp__agent_sdk_bridge__code_assistant(
    code="def foo(): return 42",
    language="Python",
    task="explain"  # explain/review/debug/optimize
)
```

### 5. 多模型对比

```python
mcp__agent_sdk_bridge__multi_model_compare(
    message="什么是 AI？",
    providers=["claude", "mock"]
)
```

## 🏗️ 架构说明

### 状态管理

```python
class AgentBridgeState:
    agents: dict           # 存储活跃的代理实例
    conversations: dict    # 存储对话历史
    config: Config         # 全局配置
```

### 支持的代理类型

| 类型 | 类 | 用途 |
|------|-----|------|
| `chat` | UniversalAIAgent | 通用对话 |
| `code` | UniversalCodeAgent | 代码助手 |
| `task` | UniversalTaskAgent | 任务执行 |

### 支持的提供商

- **claude**: glm-4.7, glm-4.6 (智谱AI)
- **openai**: gpt-4o-mini, gpt-4
- **deepseek**: deepseek-chat, deepseek-coder
- **ollama**: llama2, mistral (本地)
- **mock**: mock-model (测试)

## 🧪 测试

```bash
# 测试模式
cd AgentSdkTest
python -m mcp_servers.agent_bridge --test

# 直接运行
python mcp_servers/agent_bridge.py
```

## ⚙️ 配置要求

### 环境变量

在 `AgentSdkTest/config/.env` 中配置：

```bash
# 智谱AI API
ANTHROPIC_API_KEY=your_glm_api_key
ANTHROPIC_BASE_URL=https://open.bigmodel.cn/api/anthropic
ANTHROPIC_MODEL=glm-4.7

# OpenAI (可选)
OPENAI_API_KEY=your_openai_key

# DeepSeek (可选)
DEEPSEEK_API_KEY=your_deepseek_key
```

### 依赖检查

服务器会自动检测以下依赖：

- ✅ Agent SDK (`lib.multi_agent`)
- ✅ 配置模块 (`lib.config`)
- ✅ 工厂模块 (`lib.agent_factory`)

## 🔍 故障排除

### 1. MCP 服务器未启动

```bash
# 检查 Python 路径
echo $PYTHONPATH

# 验证模块导入
python -c "from lib.multi_agent import UniversalAIAgent"
```

### 2. API 密钥错误

```bash
# 检查配置
cat AgentSdkTest/config/.env | grep API_KEY
```

### 3. 编码问题（Windows）

如果控制台显示乱码，这是正常的编码问题，不影响功能。

## 📝 与 Claude Code 集成

现在你可以直接在 Claude Code 中：

1. 调用 Agent SDK 的多模型能力
2. 创建和管理代理实例
3. 进行代码审查和优化
4. 对比不同模型的回答
5. 执行特定任务

**这实现了 Claude Code 和 Agent SDK 的无缝集成！**

---

*创建时间: 2025-01-02*
