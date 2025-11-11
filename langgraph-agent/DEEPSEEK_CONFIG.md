# DeepSeek API 配置说明

## ✅ 配置完成状态

DeepSeek API 已成功集成到 langgraph-agent 项目中，可以正常使用！

## 🔧 配置内容

### 1. 环境变量配置 (.env 文件)

```bash
# DeepSeek API配置
DEEPSEEK_API_KEY=sk-943df854319e423ca178e68e4668ca5a

# 选择使用的API (True=DeepSeek, False=OpenAI)
USE_DEEPSEEK=True

# OpenAI API配置 (备用)
OPENAI_API_KEY=your_openai_api_key_here

# 搜索API配置 (可选)
SERPER_API_KEY=your_serper_api_key_here
```

### 2. 代码配置修改

#### settings.py 更新

- 添加了 `DEEPSEEK_API_KEY`、`DEEPSEEK_MODEL`、`DEEPSEEK_BASE_URL` 配置
- 新增 `USE_DEEPSEEK` 开关控制
- 修改 `get_llm_config()` 方法支持 DeepSeek
- 更新 `validate_config()` 方法验证 DeepSeek 配置

#### 主要配置参数

```python
# DeepSeek 配置
DEEPSEEK_MODEL: str = "deepseek-chat"
DEEPSEEK_BASE_URL: str = "https://api.deepseek.com/v1"
USE_DEEPSEEK: bool = os.getenv("USE_DEEPSEEK", "False").lower() == "true"
```

## 🚀 使用方法

### 基本使用

程序会自动根据 `USE_DEEPSEEK` 环境变量选择使用哪个API：

```python
# 当 USE_DEEPSEEK=True 时，使用 DeepSeek API
# 当 USE_DEEPSEEK=False 时，使用 OpenAI API
```

### 切换API提供商

1. **使用 DeepSeek**：
   ```bash
   # .env 文件中设置
   USE_DEEPSEEK=True
   DEEPSEEK_API_KEY=your_deepseek_key
   ```

2. **使用 OpenAI**：
   ```bash
   # .env 文件中设置
   USE_DEEPSEEK=False
   OPENAI_API_KEY=your_openai_key
   ```

## 🧪 测试结果

### API 连接测试

✅ **DeepSeek API 连接成功**
- 模型: deepseek-chat
- API Base: https://api.deepseek.com/v1
- 响应速度: 正常
- 中文支持: 优秀

### 实际测试案例

测试请求：为Python初学者制定2小时学习计划

✅ **响应质量评估**：
- 内容完整性: 优秀
- 实用性: 高
- 结构化程度: 很好
- 代码示例: 提供完整可运行的代码

## 📋 验证步骤

1. **检查配置**：
   ```bash
   cd D:\work\AI\ClaudeTest\langgraph-agent
   python -c "from config.settings import settings; print('配置验证:', settings.validate_config())"
   ```

2. **测试API连接**：
   ```bash
   python test_deepseek.py
   ```

3. **运行主程序**：
   ```bash
   python main.py "Python" --level beginner --hours 2
   ```

## 💡 使用建议

### 优势对比

**DeepSeek vs OpenAI**：

| 特性 | DeepSeek | OpenAI |
|------|----------|--------|
| 成本 | 更低 | 较高 |
| 中文支持 | 优秀 | 良好 |
| 响应速度 | 快 | 快 |
| 代码能力 | 强 | 强 |
| 推理能力 | 良好 | 优秀 |

### 推荐场景

- **使用 DeepSeek**：
  - 中文学习内容生成
  - 成本敏感的应用
  - 需要快速响应的场景

- **使用 OpenAI**：
  - 复杂逻辑推理
  - 英文内容为主
  - 需要最先进模型能力

## 🔍 故障排除

### 常见问题

1. **API密钥错误**
   ```
   解决方案：检查 .env 文件中的 DEEPSEEK_API_KEY 是否正确
   ```

2. **网络连接问题**
   ```
   解决方案：检查网络连接，确认能访问 api.deepseek.com
   ```

3. **配置未生效**
   ```
   解决方案：确认 USE_DEEPSEEK=True 且 DEEPSEEK_API_KEY 已设置
   ```

### 调试命令

```bash
# 检查当前配置
python -c "from config.settings import settings; print(settings.get_llm_config())"

# 测试API连接
python test_simple.py

# 运行完整流程
python main.py "test" --level beginner --hours 1
```

## 📝 更新日志

- **2025-11-11**: 完成 DeepSeek API 集成
  - 添加配置支持
  - 实现API切换功能
  - 通过测试验证
  - 添加中文文档

---

**配置状态**: ✅ 完成
**测试状态**: ✅ 通过
**可用性**: ✅ 正常