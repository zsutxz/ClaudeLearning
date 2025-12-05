# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 语言和Git规则
- **语言**：使用中文进行交流
- **Git规则**：不自动提交任何代码更改，手动提交前需要明确确认

## 🎯 Project Overview
这是一个**多项目技术试验仓库**，专注于AI应用开发、智能代理系统和企业级开发框架的综合性技术平台。项目采用模块化设计，包含多个独立的子项目和工具。

## 🚀 快速开始

### AI开发环境
```bash
# 1. 进入AI测试项目
cd AgentSdkTest

# 2. 安装依赖（仅需4个核心包）
pip install -r requirements.txt

# 3. 配置API密钥（复制.env.example为.env并填入密钥）
cp .env.example .env

# 4. 运行基础测试
python AgentSdkStart.py
```

### BMAD框架环境
```bash
# 1. 安装BMAD框架（需要Node.js）
npx bmad-method@alpha install

# 2. 初始化BMAD项目
*workflow-init

# 3. 创建新工作流
*bmad:bmb:workflows:create-workflow
```

### 技能系统使用
```bash
# 安装技能插件
/plugin marketplace add anthropics/skills

# 使用特定技能
skill code-architecture-analyzer  # 代码架构分析
skill pdf                        # PDF处理
skill docx                       # Word文档处理
```

## 🏗️ 项目整体架构

### 核心项目结构

#### 1. BMAD框架 (`.bmad/`)
- **技术栈**: Node.js, YAML, Markdown
- **核心功能**: 业务模型架构化开发框架
- **主要组件**:
  - `bmb/` - BMad Builder Module：完整的创建、编辑、审计工作流
  - `bmm/` - BMad Model Module：核心代理和团队管理
  - `_cfg/` - 配置管理模块
  - `core/` - 框架核心组件
  - `docs/` - 完整的文档体系

#### 2. Claude Agent SDK测试 (`AgentSdkTest/`)
- **技术栈**: Python 3.13, Claude Agent SDK
- **核心功能**: 多模型支持和Claude SDK高级功能测试
- **主要组件**:
  - `Multi_LLm.py` - 统一多模型代理，支持Claude、OpenAI、DeepSeek、Ollama
  - `TestMcp.py` - MCP(Model Context Protocol)服务器集成测试
  - `TestTool.py` - 自定义工具创建和调用测试
  - `AgentSdkStart.py` - SDK快速开始示例
- **依赖**: 4个核心Python包

#### 4. Claude技能系统 (`.claude/skills/`)
- **功能**: 35个专业技能模块，扩展Claude Code能力
- **主要技能**:
  - `code-architecture-analyzer` - 智能代码架构解读和分析工具
  - `ai-news-aggregator` - AI行业新闻聚合和智能总结
  - `algorithmic-art` - 算法艺术生成器（p5.js）
  - `docx` - Word文档创建、编辑和分析工具
  - `pdf` - PDF文档处理、提取和转换工具
  - `artifacts-builder` - HTML构件构建器（React、Tailwind CSS）
  - `mcp-builder` - MCP服务器开发指南
  - `meeting-insights-analyzer` - 会议记录分析和行为洞察
  - `video-downloader` - 视频下载和处理工具
  - `unity-*` - Unity游戏开发相关技能套件

#### 5. 产品文档 (`docs/`)
- **内容**: 完整的产品简报和需求文档
- **主要文件**:
  - `product-brief.md` (41.8KB) - 产品简报和需求文档
  - `stories/` - 用户故事和需求文档
  - `sprint-artifacts/` - Sprint产出物

#### 6. 测试和示例 (`Test/`)
- **内容**: 各种测试Prompt和LLM交互示例
- **包含**: 科学模拟、技术演示、原型代码

### 目录结构
```
E:\AI\ClaudeLearning\
├── CLAUDE.md                    # 项目配置文件
├── CLAUDE.local.md              # 用户私有配置
├── .bmad/                       # BMAD框架（隐藏目录）
│   ├── bmb/                     # 构建工具模块
│   ├── bmm/                     # 核心模块
│   ├── _cfg/                    # 配置模块
│   ├── core/                    # 框架核心
│   └── docs/                    # BMAD文档
├── .claude/                     # Claude工具和配置
│   ├── settings.local.json      # Claude本地配置
│   └── skills/                  # 35个专业技能模块
├── AgentSdkTest/                # Claude Agent SDK测试
│   ├── Multi_LLm.py             # 多模型支持
│   ├── TestMcp.py               # MCP服务器测试
│   ├── TestTool.py              # 自定义工具测试
│   └── requirements.txt         # 4个核心依赖
├── docs/                        # 项目文档系统
│   ├── product-brief.md         # 产品简报
│   └── stories/                 # 用户故事
├── Test/                        # 测试和示例文件
└── Readme/                      # 阅读材料库
```

## 🛠️ 技术栈概览

### 编程语言
- **Python 3.13+** (AI和Agent项目)
- **JavaScript/Node.js** (BMAD框架和MCP服务器)
- **YAML** (配置和文档格式)

### 核心框架和库
- **Claude Agent SDK** - Anthropic官方SDK，用于AI代理开发
- **BMAD Method** - 企业级开发框架

## 🚀 常用命令

### Claude SDK测试
```bash
cd AgentSdkTest
# 安装依赖
pip install -r requirements.txt

# 基础功能测试
python AgentSdkStart.py          # SDK快速开始
python Multi_LLm.py              # 多模型支持测试
python TestAgent.py              # 代理功能测试

# 高级功能测试
python TestMcp.py                # MCP服务器集成
python TestTool.py               # 自定义工具测试
python TestSkill.py              # 技能系统测试
python TestSlash.py              # Slash命令测试
python TestConversationSession.py # 持久化对话会话
python TestHook.py               # Hook功能测试
python TestTodos.py              # 待办事项系统

# 多模型测试
cd Prompt
python TestPrompt.py             # 通用AI代理测试
python TestDeepseek.py           # DeepSeek模型测试
```

### BMAD框架
```bash
# 安装BMAD (需要Node.js环境)
npx bmad-method@alpha install

# 初始化项目
*workflow-init

# 创建工作流
*bmad:bmb:workflows:create-workflow

# 创建代理
*bmad:bmb:workflows:create-agent

# 审计工作流
*bmad:bmb:workflows:audit-workflow

# 转换遗留项目
*bmad:bmb:workflows:convert-legacy
```

### Claude技能系统
```bash
# 安装技能插件
/plugin marketplace add anthropics/skills

# 安装文档技能
/plugin install document-skills@anthropic-agent-skills

# 安装示例技能
/plugin install example-skills@anthropic-agent-skills

# 使用技能（示例）
skill pdf                        # 使用PDF技能
skill docx                       # 使用Word文档技能
skill code-architecture-analyzer # 使用代码架构分析
```

### 开发工作流
1. **AI开发** → 在AgentSdkTest中测试Claude SDK功能
2. **框架开发** → 使用BMAD框架创建企业级应用
3. **技能开发** → 在.claude/skills中创建自定义技能
4. **文档编写** → 在docs中维护产品文档
5. **提交代码** → 手动确认后提交

## 🏗️ 核心架构洞察

### 系统架构模式
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Claude Code   │    │   BMAD框架      │    │   AI代理系统    │
│   (主环境)      │◄──►│   (企业开发)    │◄──►│   (AgentSdkTest)│
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   技能生态系统   │    │   MCP服务器     │    │   多模型支持     │
│ (.claude/skills)│◄──►│ (文件系统集成)  │◄──►│ (Claude/OpenAI) │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### 关键集成点
- **MCP服务器**：连接Claude Code与文件系统，提供安全的工具执行环境
- **技能系统**：扩展Claude Code能力，支持专业化任务处理
- **多模型架构**：通过UniversalAIAgent统一不同AI模型的接口
- **BMAD工作流**：企业级开发框架，支持agent、workflow、module的完整生命周期

### 数据流和交互模式
1. **开发请求** → Claude Code → 技能系统/BMAD框架
2. **AI代理调用** → UniversalAIAgent → 多模型API
3. **文件操作** → MCP服务器 → 安全的文件系统访问
4. **企业开发** → BMAD工作流 → 结构化的开发流程

## 🔧 关键开发模式

### 1. AI代理开发模式
```python
# 基础代理创建
from claude_agent_sdk import ClaudeAgent, ClaudeAgentOptions

options = ClaudeAgentOptions(
    model="claude-3-5-sonnet-20241022",
    allowed_tools=["Read", "Write", "Grep", "Bash"],
    mcp_servers={
        "filesystem": {
            "command": "python",
            "args": ["-m", "mcp_server_filesystem"],
            "env": {"ALLOWED_PATHS": "./"}
        }
    }
)

agent = ClaudeAgent(options)
response = agent.send_message("分析这个项目的架构")
```

### 2. 技能开发模式
```markdown
---
name: my-custom-skill
description: 自定义技能描述
allowed-tools: [Read, Write, Grep]
metadata:
  version: "1.0.0"
  category: "development"
---

# 技能说明
## 功能描述
详细说明技能的功能和使用方法
```

### 3. BMAD工作流模式
```yaml
# agent.yaml示例
name: my-agent
description: 自定义代理
version: 1.0
type: standalone
persona:
  role: 专家角色
  expertise: [领域1, 领域2]
tools: [tool1, tool2]
```

## 🔧 API配置和环境变量

### Claude SDK测试项目
在 `AgentSdkTest/.env` 中配置：
```bash
ANTHROPIC_API_KEY=your_anthropic_api_key_here
OPENAI_API_KEY=your_openai_api_key_here
DEEPSEEK_API_KEY=your_deepseek_api_key_here
```

### LangGraph项目
在 `langgraph-agent/.env` 中配置：
```bash
ANTHROPIC_API_KEY=your_anthropic_api_key_here
OPENAI_API_KEY=your_openai_api_key_here
```

### MCP服务器配置
- `.claude/settings.local.json` - Claude本地配置，启用文件系统MCP服务器
- `AgentSdkTest/.mcp.json` - MCP文件系统服务器配置

### 实际使用的模型配置
当前环境主要通过glm-4.6模型（智谱AI）提供服务：
```bash
# 主要使用模型
GLM_MODEL=glm-4.6

# 支持的备用模型
ANTHROPIC_API_KEY=your_anthropic_api_key_here  # Claude模型
OPENAI_API_KEY=your_openai_api_key_here         # OpenAI模型
DEEPSEEK_API_KEY=your_deepseek_api_key_here     # DeepSeek模型
```

## 📝 开发指南

### 新项目添加流程
1. 在根目录创建新文件夹
2. 添加相应的`.gitignore`规则
3. 创建项目特定的README和配置
4. 更新根目录CLAUDE.md（如需要）

### Git工作流
```bash
# 查看状态
git status

# 添加文件
git add .

# 提交更改（需手动确认）
git commit -m "commit message"

# 查看更改
git diff
```

### 多项目管理技巧
1. **环境隔离**: 每个项目使用独立的Python虚拟环境
2. **API密钥管理**: 通过.env文件管理，避免硬编码
3. **配置同步**: 保持各子项目文档与代码同步
4. **依赖管理**: 定期更新Python依赖，特别是AI相关包

## 💡 核心功能详解

### 1. Claude Agent SDK测试项目

#### 多模型支持架构
- **UniversalAIAgent**: 统一的AI代理接口
- 支持Claude、OpenAI、DeepSeek、Ollama、Mock模型
- 流式和同步响应支持
- 对话历史管理

#### 专业化代理类型
- **UniversalTaskAgent**: 任务型代理
- **UniversalCodeAgent**: 代码助手代理
- **UniversalTalkAgent**: 对话型代理

#### MCP集成
- Model Context Protocol服务器配置
- 文件系统服务器集成
- 进程隔离的工具执行

### 2. BMAD企业级框架

#### 核心模块
- **BMB (Builder Module)**: 创建和编辑BMAD组件
- **BMM (Model Module)**: 扩展开发能力
- **Core Framework**: 基础概念和约定

#### 支持的代理架构
- **Full Module Agent**: 完整的persona和角色定义
- **Hybrid Agent**: 共享核心能力，模块特定扩展
- **Standalone Agent**: 独立操作，最少依赖

#### 工作流类型
- **创建工作流**: agent, workflow, module
- **编辑工作流**: agent, workflow, module
- **维护工作流**: audit, convert-legacy, redoc

### 3. Claude技能生态系统

#### 实际技能分类（35个技能）
- **代码与开发**: code-architecture-analyzer, artifacts-builder, mcp-builder, unity-*
- **文档处理**: docx, pdf, pptx, xlsx, translate-it-article
- **创意设计**: algorithmic-art, canvas-design, image-enhancer
- **商业分析**: ai-news-aggregator, lead-research-assistant, bmm-*
- **数据科学**: data-scientist, bmm-data-analyst, llm-evaluation
- **企业管理**: internal-comms, invoice-organizer, meeting-insights-analyzer
- **媒体处理**: video-downloader, theme-factory
- **元技能**: skill-creator, template-skill, prompt-engineering-patterns

#### 技能使用模式
```bash
# 直接调用技能
skill pdf
skill docx

# 在Claude.ai中使用
"Use the PDF skill to extract text from document.pdf"

# 通过API使用
# 参考Claude API Skills文档
```

## ⚙️ 开发环境配置

### Python环境要求
```bash
# Python版本要求
Python 3.13+ (推荐使用虚拟环境)

# AgentSdkTest项目依赖
pip install -r requirements.txt  # 仅4个核心包

# 可选依赖
pip install python-dotenv         # .env文件支持
pip install mcp-server-filesystem # MCP文件服务器
```

### Node.js环境（BMAD框架）
```bash
# BMAD框架需要Node.js环境
npx bmad-method@alpha install

# 全局安装BMAD CLI
npm install -g bmad-method
```

### API配置安全
- 所有API密钥通过环境变量管理
- .env文件已在.gitignore中排除
- MCP服务器配置了文件访问权限限制

## 🔧 故障排除

### AI项目常见问题

**问题**: Claude SDK连接错误
- 检查ANTHROPIC_API_KEY是否正确设置
- 验证Claude Agent SDK版本兼容性
- 确认网络代理设置（如需要）
- 当前环境使用glm-4.6模型，确认智谱AI访问正常

**问题**: 多模型测试失败
- 验证对应模型的API密钥配置
- 检查.env文件格式和权限
- 确认模型名称在SUPPORTED_PROVIDERS中

**问题**: MCP服务器连接失败
- 检查.mcp.json配置
- 验证Python环境和依赖安装
- 确认文件系统权限设置

### BMAD框架问题

**问题**: BMAD命令无法识别
- 确认Node.js环境正确安装
- 检查npx权限和网络连接
- 验证BMAD包版本兼容性

**问题**: 工作流执行失败
- 检查YAML文件格式
- 验证agent配置结构
- 确认必需的依赖文件存在

### 通用问题

**问题**: 依赖包冲突
- 使用Python虚拟环境隔离项目依赖
- 检查requirements.txt版本兼容性
- 清理pip缓存并重新安装

**问题**: Git提交问题
- 遵循"不自动提交"规则
- 手动确认代码更改后再提交
- 检查.gitignore规则是否正确

## 📚 扩展开发

### Claude SDK扩展
```python
# 在Multi_LLm.py中添加新模型支持
SUPPORTED_PROVIDERS["new_model"] = {
    "models": ["new-model-latest"],
    "api_key_env": "NEW_MODEL_API_KEY",
    "client_class": NewModelClient
}
```

### 技能开发
1. **创建技能目录**: `.claude/skills/your-skill/`
2. **创建SKILL.md**: 包含YAML前置元数据和指令
3. **测试技能**: 使用`skill your-skill-name`
4. **发布技能**: 通过插件市场分享

### BMAD工作流开发
```yaml
# 创建自定义工作流
name: my-custom-workflow
description: 自定义工作流描述
version: 1.0
workflows:
  - name: my-workflow
    description: 工作流描述
    agents: [custom-agent]
```

## 🏆 项目特色

### 1. **多技术栈融合**
- AI应用开发 (Python LangGraph)
- 智能代理集成 (Claude SDK)
- 企业级开发 (BMAD框架)
- 专业化技能系统 (35个技能)

### 2. **完善的开发生态**
- 统一的MCP服务器集成
- 35个专业扩展技能
- 企业级BMAD开发框架
- 完整的文档和规范

### 3. **高度模块化设计**
- 每个子项目完全独立
- 清晰的接口定义
- 可重用组件设计
- 统一的配置管理

### 4. **企业级特性**
- BMAD框架提供完整的业务建模能力
- 多模型AI代理支持
- 可扩展的技能系统
- 专业的文档规范

---

*多项目技术试验仓库 - 专注AI应用开发、智能代理系统和企业级开发框架*