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
3. **_bmad/** - BMAD企业级开发框架（隐藏目录）
4. **.claude/** - Claude技能系统和配置

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

# 运行交互式菜单
python quick_start.py

# 或直接运行示例
python examples/01_basic_chat.py
python examples/02_multi_model.py
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

### BMAD框架
```bash
# 安装BMAD（需要Node.js）
npx bmad-method@alpha install

# 初始化项目
*workflow-init

# 创建工作流
*bmad:bmb:workflows:create-workflow
```

## 核心架构

### 系统架构模式
```
Claude Code (主环境)
    ├── 技能生态系统 (.claude/skills/)
    ├── MCP服务器 (文件系统集成)
    └── 多模型支持 (Claude/OpenAI/DeepSeek)

BMAD框架 (企业开发)
    ├── BMB (Builder Module) - 创建/编辑/审计
    ├── BMM (Model Module) - 代理和团队管理
    └── BMGD (Game Development) - 游戏开发专用

AI代理系统
    ├── AgentSdkTest - 多模型统一接口
    └── Research - 技术调研专业代理
```

### 代理继承体系
```
UniversalAIAgent (基础AI代理)
    ├── UniversalTaskAgent (任务型)
    │   └── ResearchAgent (调研代理)
    ├── UniversalCodeAgent (代码助手)
    └── UniversalTalkAgent (对话型)
```

### AgentSdkTest 核心架构
```
AgentSdkTest/
├── lib/                           # 核心库模块
│   ├── multi_agent.py             # UniversalAIAgent - 多模型统一接口
│   ├── agent_factory.py           # AgentFactory - 工厂模式
│   ├── config.py                  # Config - 配置管理 (支持config/.env)
│   └── utils.py                   # 工具函数
├── examples/                      # 渐进式示例 (01-07)
├── Test/                          # 测试文件集合
├── config/
│   ├── .env                       # 环境变量配置 (优先加载)
│   └── .env.example               # 配置模板
├── quick_start.py                 # 交互式菜单
└── run_all_examples.py            # 批量运行
```

### Research 核心架构
```
Research/
├── research_agent.py              # ResearchAgent主类
├── modules/
│   ├── literature_retriever/      # 文献检索模块
│   ├── data_processor.py          # 数据处理
│   ├── report_generator.py        # 报告生成
│   └── quality_checker.py         # 质量检查
├── mcp_servers/                   # 研究工具MCP服务器
│   ├── search_literature
│   ├── analyze_repository
│   ├── fetch_paper
│   └── generate_report
└── test/                          # 测试用例
```

### BMAD框架结构
```
_bmad/
├── bmm/                           # Model Module
│   ├── agents/                    # 代理定义 (YAML)
│   ├── workflows/                 # 工作流定义
│   └── teams/                     # 团队配置
├── bmb/                           # Builder Module
│   └── workflows/                 # 创建/编辑/审计工作流
├── bmgd/                          # Game Development Module
│   ├── agents/                    # 游戏开发代理
│   └── gametest/                  # 游戏测试
└── core/                          # 框架核心组件
```

## 常用命令

### AgentSdkTest
```bash
# 快速开始
python quick_start.py

# 运行特定示例
python examples/01_basic_chat.py        # 基础对话
python examples/02_multi_model.py       # 多模型支持
python examples/03_tools_usage.py       # 工具使用
python examples/04_mcp_integration.py    # MCP集成
python examples/05_session_management.py # 会话管理
python examples/06_stream_response.py   # 流式响应
python examples/07_advanced_agent.py    # 高级代理

# 运行所有示例
python run_all_examples.py

# 测试文件
cd Test
python TestAgentSdk.py                 # SDK基础测试
python TestMultiLlm.py                  # 多模型测试
python TestMcp.py                       # MCP服务器测试
python TestTool.py                      # 自定义工具测试
```

### Research
```bash
# 运行示例
python example_usage.py

# 运行测试
pytest test/

# 执行调研
python -c "from research_agent import ResearchAgent; ..."
```

### BMAD框架
```bash
# 初始化
*workflow-init

# 创建工作流
*bmad:bmb:workflows:create-workflow
*bmad:bmm:workflows:create-agent
*bmad:bmb:workflows:audit-workflow

# 游戏开发
*bmad:bmgd:gametest:create-test
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
# 继承Claude配置
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
- **Mock**: mock-model (测试)

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
`lib/config.py` 按以下顺序查找 `.env`:
1. `AgentSdkTest/config/.env` (优先)
2. `AgentSdkTest/.env` (备用)

### Git工作流
1. **不自动提交**: 所有代码更改需手动确认后提交
2. **提交前确认**: 运行 `git status` 和 `git diff` 检查更改
3. **中文提交信息**: 使用清晰的中文描述更改

### 测试文件位置
- **AgentSdkTest/Test/** - 所有测试文件已移至此目录
- **examples/** - 渐进式学习示例 (01-07)

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
```

### 环境变量未加载
```bash
# 验证配置加载
python -c "from lib.config import get_config; c = get_config(); print(c.anthropic_api_key)"
```

### MCP服务器问题
```bash
# 检查MCP配置
cat .mcp.json
cat .claude/settings.local.json
```

---

*多项目技术试验仓库 - 专注AI应用开发、智能代理系统和企业级开发框架*
