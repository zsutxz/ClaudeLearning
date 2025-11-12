# Claude Agent SDK 快速开始示例

这个项目演示了如何使用Claude Agent SDK进行基本的AI对话和工具使用。

## 🚀 快速开始

### 1. 安装依赖

```bash
pip install claude-agent-sdk
# 可选：安装python-dotenv以支持.env文件
pip install python-dotenv
```

### 2. 配置API密钥

有两种方式配置API密钥：

#### 方法1：环境变量
```bash
export ANTHROPIC_API_KEY=your_api_key_here
```

#### 方法2：.env文件（推荐）
项目已包含`.env`文件，您只需要修改其中的API密钥：
```
ANTHROPIC_API_KEY=your_api_key_here
```

### 3. 运行示例

```bash
python AgentSdlquick_start.py
```

## 📝 示例说明

### 基础示例 (Basic Example)
向Claude提问简单的数学问题，展示基本的对话功能。

### 选项示例 (With Options Example)
演示如何使用自定义选项配置Claude的行为：
- 设置系统提示词
- 限制对话轮次
- 自定义模型行为

### 工具示例 (With Tools Example)
展示如何让Claude使用工具（文件读写功能）：
- 创建文件
- 读取文件内容
- 显示使用成本

## 🔧 配置选项

### ClaudeAgentOptions
```python
options = ClaudeAgentOptions(
    system_prompt="系统提示词",
    max_turns=1,  # 最大对话轮次
    allowed_tools=["Read", "Write"],  # 允许使用的工具
    model="claude-3-haiku-20240307"  # 使用的模型
)
```

## 📊 输出示例

```
=== Basic Example ===
Claude: 2 + 2 = 4

=== With Options Example ===
Claude: Python is a high-level programming language...

=== With Tools Example ===
Claude: I've created the file `hello.txt` with "Hello, World!" in it.
Cost: $0.0113
```

## 🛠️ 自定义开发

您可以基于这些示例构建更复杂的应用：

1. **聊天机器人**: 扩展对话功能，添加记忆和历史记录
2. **文件处理器**: 利用工具功能构建自动化文件处理系统
3. **数据分析**: 结合Python的数据处理库进行智能分析
4. **Web集成**: 将Claude集成到Web应用中

## 📋 故障排除

### API密钥错误
```
Claude: Invalid API key · Please run /login
```
**解决方案**: 检查.env文件或环境变量中的API密钥是否正确设置。

### 依赖包问题
```
ModuleNotFoundError: No module named 'claude_agent_sdk'
```
**解决方案**: 使用pip安装所需的依赖包。

### 权限问题
确保脚本有足够的权限读取.env文件和创建新文件。

## 📚 相关资源

- [Claude Agent SDK 官方文档](https://docs.anthropic.com/claude/docs/claude-sdk)
- [Anthropic API 文档](https://docs.anthropic.com/claude/reference)
- [Python 异步编程指南](https://docs.python.org/3/library/asyncio.html)

---

*最后更新: 2025-11-12*