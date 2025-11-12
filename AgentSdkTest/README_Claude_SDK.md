# Claude Agent SDK 示例 - DeepSeek版本

这个项目展示了如何使用DeepSeek API构建各种类型的智能代理，专注于Python实现。

## 📁 文件结构

```
AgentSdkTest/
├── claude_agent_deepseek.py         # DeepSeek API版本完整示例
├── test_deepseek.py                 # 快速API测试工具
└── README_Claude_SDK.md             # 本说明文档
```

## 🚀 快速开始

### 1. 获取DeepSeek API密钥

访问 [DeepSeek Platform](https://platform.deepseek.com/) 注册账户并获取API密钥。

### 2. 安装依赖

```bash
pip install openai
```

### 3. 设置环境变量

```bash
# Windows
set DEEPSEEK_API_KEY="your-deepseek-api-key-here"

# Linux/MacOS
export DEEPSEEK_API_KEY="your-deepseek-api-key-here"
```

### 4. 运行示例

#### 快速测试（推荐）
```bash
# 编辑test_deepseek.py，设置API密钥后运行
python test_deepseek.py
```

#### 完整示例
```bash
python claude_agent_deepseek.py
```

## 🏗️ 代理类型

### 1. DeepSeekAgent (基础代理)
最基础的对话代理，支持：
- 同步和流式响应
- 对话历史管理
- 系统提示词设置

```python
# Python
agent = DeepSeekAgent()
response = agent.chat("你好！", stream=True)
```

### 2. DeepSeekTaskAgent (任务型代理)
专门处理特定任务的代理：
- 自定义任务描述
- 问题解决方案生成
- 专业领域助手

```python
# Python
task_agent = DeepSeekTaskAgent("帮助用户制定学习计划")
plan = task_agent.solve_problem("我想学习AI，应该从哪里开始？")
```

### 3. DeepSeekCodeAgent (代码助手)
编程专用代理：
- 代码生成和优化
- 代码审查
- 调试辅助

```python
# Python
code_agent = DeepSeekCodeAgent(language="Python")
code = code_agent.write_code("实现斐波那契数列")
review = code_agent.review_code(code)
```

## 🛠️ 高级功能

### 流式响应
实时显示DeepSeek的回复过程：

```python
# Python
response = agent.chat("请详细解释机器学习", stream=True)
```

### 对话历史管理
自动维护对话上下文：

```python
# Python
agent.clear_history()  # 清空历史
summary = agent.get_conversation_summary()  # 获取摘要
```

### 自定义系统提示词
为代理设置特定的行为模式：

```python
# Python
agent.add_system_prompt("你是一个专业的AI助手，请用简洁明了的语言回答问题。")
```

## 📊 支持的DeepSeek模型

- `deepseek-chat` (推荐，通用对话模型)
- `deepseek-coder` (代码专用模型)

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
class CustomAgent(DeepSeekAgent):
    def __init__(self, custom_config, **kwargs):
        super().__init__(**kwargs)
        self.custom_config = custom_config
        self.add_system_prompt(f"自定义代理配置: {custom_config}")

    def custom_method(self, input_data):
        prompt = f"根据自定义配置处理: {input_data}"
        return self.chat(prompt)
```

## 📚 更多资源

- [DeepSeek官方文档](https://platform.deepseek.com/docs)
- [DeepSeek API参考](https://platform.deepseek.com/docs/api)
- [OpenAI Python SDK文档](https://github.com/openai/openai-python)
- [DeepSeek最佳实践](https://platform.deepseek.com/docs/guides)

## 🐛 故障排除

### 常见问题

**问题**: API密钥错误
```
解决方案: 检查DEEPSEEK_API_KEY环境变量设置，确保密钥正确且有效
```

**问题**: 网络连接超时
```
解决方案: 检查网络连接，设置适当的超时时间
```

**问题**: Token限制超出
```
解决方案: 减少输入长度或增加max_tokens参数
```

**问题**: 依赖包安装失败
```
解决方案: 检查Python版本，清理pip缓存: pip cache purge
```

**问题**: API调用频率限制
```
解决方案: 控制请求频率，考虑添加重试机制
```

## 📄 许可证

MIT License - 可自由使用和修改。

## 💡 DeepSeek vs 其他API的优势

### 🚀 性能优势
- **更快的响应速度** - 国内服务器，延迟更低
- **更有竞争力的价格** - 成本相对较低
- **优秀的中文支持** - 原生中文优化

### 🛠️ 技术特点
- **OpenAI格式兼容** - 无缝切换现有代码
- **强大的代码能力** - 支持多种编程语言
- **丰富的模型选择** - 通用和专用模型

### 📈 适用场景
- **个人开发者** - 低成本快速原型开发
- **企业应用** - 高性价比的AI解决方案
- **中文项目** - 原生优化的中文理解能力

---

*享受使用DeepSeek SDK构建智能代理的乐趣！* 🤖✨