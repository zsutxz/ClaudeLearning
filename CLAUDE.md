# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 语言和Git规则
- **语言**：使用中文进行交流
- **Git规则**：不自动提交任何代码更改，手动提交前需要明确确认

## Project Overview
这是一个**多项目技术试验仓库**，专注于AI应用开发、智能代理系统和企业级开发框架的综合性技术平台。项目采用模块化设计，包含多个独立的子项目和工具。

### 核心项目
1. **AgentSdkTest/** - Claude Agent SDK整合项目，多模型统一接口
2. **Research/** - 技术调研专业代理系统
3. **_bmad/** - BMAD企业级框架（业务建模和开发工作流）
4. **.claude/** - Claude技能系统和配置（169+ 技能模块）

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

### BMAD 框架
```bash
# BMAD 工作流通过 Claude Code 技能系统调用
# 使用示例：
*workflow-init          # 初始化新项目
*prd                    # 创建/验证/编辑 PRD
*create-architecture    # 创建架构文档
*create-epics-and-stories  # 创建史诗和用户故事
```

## 核心架构

### 系统架构模式
```
Claude Code (主环境)
    ├── 技能生态系统 (.claude/skills/) - 169+ 技能
    ├── MCP服务器 (.mcp.json)
    │   ├── agentvibes - TTS语音输出
    │   └── agent-sdk-bridge - 自定义SDK桥接
    └── 多模型支持 (Claude/OpenAI/DeepSeek)

AI代理系统
    ├── AgentSdkTest - 多模型统一接口
    └── Research - 技术调研专业代理

BMAD企业框架
    ├── BMB - Builder Module (代理和工作流构建)
    ├── BMM - Model Module (业务建模)
    ├── BMGD - Game Development Module (游戏开发)
    └── Core - 框架核心功能
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

### BMAD 框架架构
```
_bmad/                            # BMAD 企业级框架（隐藏目录）
├── bmb/                           # Builder Module
│   └── workflows/
│       ├── agent/                 # 代理创建工作流
│       ├── module/                # 模块构建工作流
│       └── workflow/              # 工作流创建
├── bmm/                           # Model Module
│   └── workflows/
│       ├── prd/                   # 产品需求文档
│       ├── create-architecture/   # 架构设计
│       ├── create-epics-and-stories/  # 故事创建
│       └── testarch/              # 测试架构
├── bmgd/                          # Game Development Module
│   └── workflows/                 # 游戏开发工作流
├── core/                          # 框架核心
│   ├── resources/                 # 资源和模板
│   └── workflows/                 # 核心工作流
└── _cfg/                          # 配置管理
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

### BMAD 工作流
BMAD 工作流通过 Claude Code 技能系统调用，使用 `*workflow-name` 格式：

**项目初始化**：
- `*workflow-init` - 初始化新项目

**规划阶段**：
- `*prd` - 创建/验证/编辑 PRD（三模式：create/validate/edit）
- `*create-architecture` - 创建架构文档
- `*create-ux-design` - UX设计协作

**实施阶段**：
- `*create-epics-and-stories` - 创建史诗和用户故事
- `*create-story` - 创建下一个用户故事
- `*dev-story` - 执行故事开发
- `*sprint-planning` - Sprint 规划
- `*sprint-status` - Sprint 状态检查
- `*correct-course` - 航线修正（处理重大变更）

**测试阶段**：
- `*testarch-test-design` - 测试设计
- `*testarch-automate` - 测试自动化
- `*testarch-framework` - 测试框架初始化
- `*testarch-ci` - CI/CD 配置

### Claude技能系统
```bash
# 安装技能插件
/plugin marketplace add anthropics/skills

# 使用技能（通过 Skill 工具调用）
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

### 配置的MCP服务器
- **agentvibes** - TTS语音输出
- **agent-sdk-bridge** - 自定义Python模块，位于项目根目录的 `mcp_servers/agent_bridge.py`
  - 提供 Claude Agent SDK 与 MCP 协议的桥接
  - 支持官方 SDK 创建和对话
  - PYTHONPATH 配置: `D:\work\AI\ClaudeLearning;D:\work\AI\ClaudeLearning\AgentSdkTest`

### 其他可用MCP服务器
- **filesystem** - 文件系统访问
- **playwright** - 浏览器自动化
- **fetch** - 网页抓取
- **web_reader** - 网页内容提取和转换

## 技能系统

### 技能概览
项目集成 **169+ 个专业技能模块**，通过 `.claude/skills/` 目录管理。

### 主要技能分类

#### 代码开发
- `code-architecture-analyzer` - 代码架构分析
- `mcp-builder` - MCP服务器构建
- `artifacts-builder` - HTML工件构建
- `code-simplifier` - 代码简化和重构

#### 文档处理
- `pdf` - PDF处理（提取、合并、分割）
- `docx` - Word文档处理
- `pptx` - PowerPoint处理
- `xlsx` - Excel处理
- `translate-it-article` - IT文章翻译

#### AI/ML
- `ai-news-aggregator` - AI新闻聚合
- `llm-evaluation` - LLM评估
- `rag-implementation` - RAG系统实现
- `langchain-architecture` - LangChain架构
- `mlops-engineer` - MLOps工程
- `data-scientist` - 数据科学

#### Unity开发（10个技能）
- `unity-scene-optimizer` - 场景优化
- `unity-script-validator` - 脚本验证
- `unity-uitoolkit` - UI Toolkit开发
- `unity-template-generator` - 模板生成
- `unity-test-runner` - 测试运行
- `unity-ui-selector` - UI框架选择
- `unity-architect` - 架构设计
- `unity-performance` - 性能优化

#### BMAD框架
- `bmad:core:workflows:prd` - PRD工作流
- `bmad:bmm:workflows:create-architecture` - 架构创建
- `bmad:bmm:workflows:create-epics-and-stories` - 故事创建
- `bmad:bmm:workflows:dev-story` - 故事开发
- `bmad:core:agents:bmad-master` - BMAD主代理

#### 企业管理
- `internal-comms` - 内部沟通
- `invoice-organizer` - 发票整理
- `meeting-insights-analyzer` - 会议分析

#### 媒体处理
- `video-downloader` - 视频下载
- `image-enhancer` - 图像增强
- `algorithmic-art` - 算法艺术

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

### BMAD工作流问题
```bash
# 检查_bmad目录结构
ls _bmad/

# 确保使用正确的工作流名称（带*前缀）
*workflow-status  # 检查工作流状态
```

---

## 文档编写指南

### FORCold.md 文档要求

为每个项目编写详细的 FORCold.md 文件，用通俗易懂的语言解释整个项目。

**应包含的内容**：
- 技术架构
- 代码库结构及各部分如何连接
- 使用的技术
- 技术决策的原因
- 经验教训（包括遇到的bug及修复方法、潜在陷阱及避免方法）
- 新技术使用
- 优秀工程师的思维和工作方式
- 最佳实践

**写作风格**：
- 生动有趣，避免像枯燥的技术文档或教科书
- 适当使用类比和轶事，使内容更易于理解和记忆

---

*多项目技术试验仓库 - 专注AI应用开发、智能代理系统*

*最后更新: 2026-01-25*
