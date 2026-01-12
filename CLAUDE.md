# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with in this repository.

## 语言和Git规则
- **语言**：使用中文进行交流
- **Git规则**：不自动提交任何代码更改，手动提交前需要明确确认

## Project Overview
这是一个**多项目技术试验仓库**，专注于AI应用开发、智能代理系统和企业级开发框架的综合性技术平台。项目采用模块化设计，包含多个独立的子项目和工具。

### 核心项目
1. **AgentSdkTest/** - Claude Agent SDK整合项目，多模型统一接口
2. **Research/** - 技术调研专业代理系统
3. **.claude/** - Claude技能系统和配置

## 快速开始

### AgentSdkTest (Claude Agent SDK)
```bash
cd AgentSdkTest

# 安装依赖（仅4个核心包）
pip install -r requirements.txt

# 配置API密钥在 config/.env
# ANTHROPIC_API_KEY=your_key
# ANTHROPIC_BASE_URL=https://open.bigmodel.cn/api/anthropic
# ANTHROPIC_MODEL=glm-4.7

# 或使用 python-dotenv 加载环境变量

# 运行交互式菜单
python quick_start.py

# 或直接运行示例
python examples/01_basic_chat.py
python examples/02_multi_model.py

# 运行测试文件
python examples/TestAgentSdk.py
```

### Research (技术调研代理)
```bash
cd Research

# 安装研究专用依赖（86个包）
pip install -r requirements.txt

# 配置环境变量
# GITHUB_TOKEN, KAGGLE_USERNAME, etc.

# 运行示例
python example_usage.py
```

## 核心架构

### 系统架构模式
```
Claude Code (主环境)
    ├── 技能生态系统 (.claude/skills/)
    ├── MCP服务器 (文件系统集成)
    └── 多模型支持 (Claude/OpenAI/DeepSeek)

AI代理系统
    ├── AgentSdkTest - 多模型统一接口
    └── Research - 技术调研专业代理
```

### 代理继承体系
```
UniversalAIAgent (基础AI代理) - lib/multi_agent.py
    ├── UniversalTaskAgent (任务型)
    │   └── ResearchAgent (调研代理) - Research/research_agent.py
    ├── UniversalCodeAgent (代码助手)
    └── UniversalTalkAgent (对话型，未实现)
```

**关键设计**：所有代理类都支持同步和异步调用，通过 `provider` 参数切换不同的AI模型。

### AgentSdkTest 核心架构
```
AgentSdkTest/
├── lib/                           # 核心库模块
│   ├── multi_agent.py             # UniversalAIAgent - 多模型统一接口
│   ├── agent_factory.py           # AgentFactory - 工厂模式
│   ├── config.py                  # Config - 配置管理 (支持config/.env)
│   └── utils.py                   # 工具函数
├── examples/                      # 示例和测试文件
│   ├── 01_basic_chat.py           # 基础对话
│   ├── 02_multi_model.py          # 多模型支持
│   ├── 04_mcp_integration.py      # MCP集成
│   ├── 05_session_management.py   # 会话管理
│   ├── 06_stream_response.py      # 流式响应
│   ├── 07_advanced_agent.py       # 高级代理
│   └── Test*.py                   # 各类测试文件
├── config/
│   └── .env                       # 环境变量配置 (优先加载，通过python-dotenv)
├── quick_start.py                 # 交互式菜单
└── run_all_examples.py            # 批量运行所有示例
```

### Research 核心架构
```
Research/
├── research_agent.py              # ResearchAgent主类（继承UniversalTaskAgent）
├── example_usage.py               # 使用示例
├── modules/                       # 核心功能模块
│   ├── literature_retriever/      # 文献检索模块
│   │   ├── __init__.py
│   │   └── literature_retriever.py
│   ├── data_processor.py          # 数据处理
│   ├── report_generator.py        # 报告生成
│   └── quality_checker.py         # 质量检查
├── test/                          # 测试用例
│   ├── __init__.py
│   └── test_research_agent.py
├── reports/                       # 生成的报告目录
└── requirements.txt               # 86个专业依赖包
```

## 常用命令

### AgentSdkTest
```bash
# 快速开始（交互式菜单）
python quick_start.py

# 运行特定示例
python examples/01_basic_chat.py        # 基础对话
python examples/02_multi_model.py       # 多模型支持
python examples/04_mcp_integration.py    # MCP集成
python examples/05_session_management.py # 会话管理
python examples/06_stream_response.py   # 流式响应
python examples/07_advanced_agent.py    # 高级代理

# 运行所有示例
python run_all_examples.py

# 测试文件（在examples/目录下）
python examples/TestAgentSdk.py         # SDK基础测试
python examples/TestDeepseek.py         # DeepSeek测试
python examples/TestMcp.py              # MCP服务器测试
python examples/TestTool.py             # 自定义工具测试
```

### Research
```bash
# 运行示例
python example_usage.py

# 运行测试
pytest test/

# 直接运行Research Agent（内置测试）
python research_agent.py
```

**注意**：Research Agent 使用异步函数，需要使用 `await` 或 `asyncio.run()` 调用：
```python
import asyncio
from research_agent import ResearchAgent

async def main():
    agent = ResearchAgent(research_domain="人工智能")
    result = await agent.conduct_research("查询主题")

asyncio.run(main())
```

### Claude技能系统
```bash
# 安装技能插件
/plugin marketplace add anthropics/skills

# 使用技能
skill code-architecture-analyzer
skill pdf
skill unity-scene-optimizer
skill ai-news-aggregator
```

## 环境变量配置

### AgentSdkTest (config/.env)
```bash
# 智谱AI API (主要使用)
ANTHROPIC_API_KEY=your_glm_api_key
ANTHROPIC_BASE_URL=https://open.bigmodel.cn/api/anthropic
ANTHROPIC_MODEL=glm-4.7

# OpenAI (可选)
OPENAI_API_KEY=your_openai_key
OPENAI_BASE_URL=https://api.openai.com/v1

# DeepSeek (可选)
DEEPSEEK_API_KEY=your_deepseek_key
DEEPSEEK_BASE_URL=https://api.deepseek.com/v1
```

### Research (.env)
```bash
# Research 依赖 AgentSdkTest 的配置
# 需要先配置 AgentSdkTest/config/.env 中的 Claude API

# 额外的研究专用配置
GITHUB_TOKEN=your_github_token
KAGGLE_USERNAME=your_kaggle_username
KAGGLE_KEY=your_kaggle_key
DATABASE_URL=sqlite:///research_data.db
```

## 多模型支持

### 支持的模型提供商
- **Claude**: glm-4.7, glm-4.6 (智谱AI)
- **OpenAI**: gpt-4o-mini, gpt-4
- **DeepSeek**: deepseek-chat, deepseek-coder
- **Ollama**: llama2, mistral, codellama (本地)
- **Mock**: mock-model (测试，无需API密钥)

### UniversalAIAgent 使用
```python
from lib.multi_agent import UniversalAIAgent

agent = UniversalAIAgent(
    provider="claude",
    model="glm-4.7",
    api_key=os.getenv('ANTHROPIC_API_KEY'),
    base_url=os.getenv('ANTHROPIC_BASE_URL')
)

response = agent.chat("你好")

# 支持流式响应
response = agent.chat("写一个故事", stream=True)
```

## MCP服务器集成

### 项目MCP配置
- `.mcp.json` - 根目录MCP服务器配置
- `.claude/settings.local.json` - Claude本地配置

### 常用MCP服务器
- **agentvibes** - TTS语音输出
- **filesystem** - 文件系统访问
- **playwright** - 浏览器自动化
- **fetch** - 网页抓取
- **web_reader** - 网页内容提取和转换

注意：MCP服务器配置文件位于根目录 `.mcp.json` 和 `.claude/settings.local.json`

## 技能系统

### 主要技能分类
- **代码开发**: code-architecture-analyzer, mcp-builder, artifacts-builder
- **文档处理**: pdf, docx, pptx, xlsx, translate-it-article
- **AI/ML**: ai-news-aggregator, llm-evaluation, rag-implementation, langchain-architecture
- **Unity开发**: unity-scene-optimizer, unity-script-validator, unity-uitoolkit (10个技能)
- **企业管理**: internal-comms, invoice-organizer, meeting-insights-analyzer
- **媒体处理**: video-downloader, image-enhancer, algorithmic-art

## 重要注意事项

### 环境变量加载优先级
`lib/config.py` 使用 `python-dotenv` 按以下顺序加载 `.env`:
1. `AgentSdkTest/config/.env` (优先)
2. `AgentSdkTest/.env` (备用)

如果 `python-dotenv` 未安装，则使用内置的 `load_env_file()` 函数按相同顺序查找。

### Git工作流
1. **不自动提交**: 所有代码更改需手动确认后提交
2. **提交前确认**: 运行 `git status` 和 `git diff` 检查更改
3. **中文提交信息**: 使用清晰的中文描述更改

### 测试文件位置
- **AgentSdkTest/examples/** - 所有测试文件和示例都在此目录
  - 渐进式学习示例 (01-07)
  - Test*.py 测试文件

### 导入路径说明
- **AgentSdkTest**: 使用 `from lib.multi_agent import UniversalAIAgent`（需要在 AgentSdkTest 目录下运行）
- **Research**: 使用 `from research_agent import ResearchAgent`（需要在 Research 目录下运行）
- Research Agent 会自动添加 AgentSdkTest 到路径：`sys.path.append(os.path.join(os.path.dirname(__file__), '..', 'AgentSdkTest'))`

### 安全性
- 所有 `.env` 文件已在 `.gitignore` 中排除
- API密钥通过环境变量管理
- MCP服务器配置了文件访问权限限制

## 故障排除

### API连接错误 (403 Forbidden)
```bash
# 检查API密钥配置
cat AgentSdkTest/config/.env | grep ANTHROPIC_API_KEY

# 智谱AI密钥格式: id.secret
# 例如: 406db45f77c9cf.a3b2c1d0e9f8a7b6c5d4e3f2a1b0c9d8

# 验证环境变量是否加载
python -c "from lib.config import get_config; c = get_config(); print(c.anthropic_api_key)"
```

### Research导入错误
```bash
# 如果出现 ImportError: No module named 'lib.multi_agent'
# 确保在正确的目录运行

cd Research  # 必须在Research目录下运行
python example_usage.py
```

### MCP服务器问题
```bash
# 检查MCP配置
cat .mcp.json
cat .claude/settings.local.json

# 重启Claude Code以重新加载MCP配置
```

---

*多项目技术试验仓库 - 专注AI应用开发、智能代理系统*

*最后更新: 2025-01-12*
