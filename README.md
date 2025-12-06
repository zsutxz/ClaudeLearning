# ClaudeLearning - 多项目技术试验仓库

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Python 3.13+](https://img.shields.io/badge/python-3.13+-blue.svg)](https://www.python.org/downloads/)
[![Node.js](https://img.shields.io/badge/node.js-18+-green.svg)](https://nodejs.org/)
[![BMAD Framework](https://img.shields.io/badge/BMAD-Alpha-purple.svg)](https://bmad-method.com/)

> 🚀 专注AI应用开发、智能代理系统和企业级开发框架的综合性技术平台

## 🎯 项目简介

ClaudeLearning是一个**多项目技术试验仓库**，采用模块化设计，集成了AI应用开发、智能代理系统、企业级开发框架和30+专业技能模块。项目提供了从基础SDK测试到企业级应用开发的完整技术栈支持。

### ✨ 核心特色

- 🤖 **多模型AI支持** - 统一接口支持Claude、OpenAI、DeepSeek、Ollama等主流AI模型
- 🏗️ **企业级BMAD框架** - 完整的业务建模和开发工作流管理
- 🛠️ **30+专业技能模块** - 覆盖开发、文档、创意、Unity游戏等多个领域
- 🔌 **MCP协议集成** - Model Context Protocol提供安全的工具执行环境
- 📚 **技术调研代理** - 专业的文献检索、数据分析、报告生成系统

## 🚀 快速开始

### 环境要求

- Python 3.13+
- Node.js 18+
- Git

### 1. AI开发环境

```bash
# 克隆仓库
git clone https://github.com/zsutxz/ClaudeLearning.git
cd ClaudeLearning

# 进入AI测试项目
cd AgentSdkTest

# 安装依赖（仅需4个核心包）
pip install -r requirements.txt

# 配置API密钥
cp .env.example .env
# 编辑.env文件，添加您的API密钥

# 运行基础测试
python AgentSdkStart.py
```

### 2. 技术调研系统

```bash
# 进入Research目录
cd Research

# 安装研究专用依赖（86个专业包）
pip install -r requirements.txt

# 配置研究环境
cp .env.example .env
# 添加GitHub、Kaggle等API密钥

# 运行研究示例
python example_usage.py
```

### 3. BMAD企业框架

```bash
# 安装BMAD框架（需要Node.js环境）
npx bmad-method@alpha install

# 初始化BMAD项目
*workflow-init

# 创建新工作流
*bmad:bmb:workflows:create-workflow
```

### 4. Claude技能系统

```bash
# 安装技能插件（在Claude Code中使用）
/plugin marketplace add anthropics/skills

# 使用特定技能
skill code-architecture-analyzer  # 代码架构分析
skill pdf                        # PDF处理
skill unity-scene-optimizer      # Unity场景优化
```

## 📁 项目结构

```
ClaudeLearning/
├── 📄 README.md                  # 项目说明文档
├── 📄 CLAUDE.md                  # Claude Code配置指南
├── 📄 LICENSE                    # MIT许可证
│
├── 🤖 AgentSdkTest/              # Claude Agent SDK测试
│   ├── 🐍 MultiAIAgent.py         # 统一多模型代理
│   ├── 🧪 Test*.py               # 20+个功能测试文件
│   ├── ⚙️ .mcp.json              # MCP服务器配置
│   └── 📋 requirements.txt       # 4个核心依赖
│
├── 🔬 Research/                   # 技术调研专业代理
│   ├── 🐍 research_agent.py       # ResearchAgent主类
│   ├── 📦 modules/                # 核心功能模块
│   │   ├── literature_retriever/  # 文献检索
│   │   ├── data_processor.py      # 数据处理
│   │   ├── report_generator.py    # 报告生成
│   │   └── quality_checker.py     # 质量检查
│   ├── 🔌 mcp_servers/            # 研究工具MCP服务器
│   └── 📋 requirements.txt        # 86个专业依赖
│
├── 🏗️ .bmad/                     # BMAD企业级框架（隐藏目录）
│   ├── 📦 bmb/                    # Builder Module
│   ├── 🎯 bmm/                    # Model Module
│   ├── ⚙️ _cfg/                   # 配置管理
│   ├── 🔧 core/                   # 框架核心
│   └── 📚 docs/                   # BMAD文档
│
├── 🛠️ .claude/                    # Claude工具和配置
│   ├── ⚙️ settings.local.json     # Claude本地配置
│   └── 🎯 skills/                 # 30+专业技能模块
│       ├── 🏗️ code-architecture-analyzer
│       ├── 📄 pdf, docx, pptx
│       ├── 🎨 algorithmic-art
│       ├── 🎮 unity-*/            # 10个Unity开发技能
│       └── 🤖 ai-*/               # AI/ML相关技能
│
├── 📚 docs/                       # 项目文档系统
│   ├── 📋 product-brief.md        # 产品简报(41.8KB)
│   └── 📖 stories/                # 用户故事
│
├── 🧪 Test/                       # 测试和示例
│   └── 🎬 science-*.py            # 科学模拟演示
│
└── 📖 Readme/                     # 阅读材料库
    └── 📄 *.md                    # 技术文档和教程
```

## 🛠️ 核心功能

### 1. 多模型AI代理系统

```python
from AgentSdkTest.MultiAIAgent import UniversalAIAgent

# 创建统一代理
agent = UniversalAIAgent(
    provider="claude",  # 支持 claude, openai, deepseek, ollama
    model="glm-4.6"
)

# 执行任务
response = await agent.send_message("分析这个项目的架构")
```

### 2. 技术调研代理

```python
from Research.research_agent import ResearchAgent

# 创建研究代理
researcher = ResearchAgent(
    research_domain="人工智能",
    provider="claude"
)

# 执行研究
result = await researcher.conduct_research(
    query="大语言模型的最新发展趋势",
    max_sources=10
)

# 生成报告
print(result.report)
```

### 3. BMAD企业级开发

```yaml
# agent.yaml示例
name: my-agent
description: 自定义代理
version: 1.0
type: standalone
persona:
  role: 技术专家
  expertise: [AI, 架构设计]
tools: [analysis, documentation]
```

### 4. 专业技能模块

```bash
# 代码架构分析
skill code-architecture-analyzer

# PDF文档处理
skill pdf

# Unity性能优化
skill unity-scene-optimizer

# AI新闻聚合
skill ai-news-aggregator
```

## 📊 技术栈

### 核心技术

| 类别 | 技术 | 版本 | 用途 |
|------|------|------|------|
| **编程语言** | Python | 3.13+ | AI和代理开发 |
| **编程语言** | JavaScript/Node.js | 18+ | BMAD框架 |
| **配置格式** | YAML | - | 配置和文档 |
| **API协议** | MCP | 1.0 | 工具集成 |

### 主要框架

- **Claude Agent SDK** - Anthropic官方AI代理开发框架
- **BMAD Method** - 企业级业务建模和开发框架
- **MCP Protocol** - Model Context Protocol工具集成标准

### 依赖统计

- **AgentSdkTest**: 4个核心依赖
- **Research**: 86个专业依赖
- **技能模块**: 30+个独立技能
- **总计**: 120+个专业包

## 🎯 使用场景

### 1. AI应用开发
- 快速原型开发
- 多模型对比测试
- API集成和调试

### 2. 技术调研
- 文献检索和分析
- 技术趋势研究
- 竞品分析

### 3. 企业级开发
- 业务建模
- 工作流设计
- 团队协作

### 4. 游戏开发
- Unity性能优化
- 场景分析和调试
- 脚本验证

### 5. 文档处理
- PDF/Word处理
- 自动报告生成
- 知识管理

## 🤝 贡献指南

我们欢迎所有形式的贡献！请查看 [CONTRIBUTING.md](CONTRIBUTING.md) 了解详细信息。

### 开发流程

1. Fork 项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

### 代码规范

- Python代码遵循 PEP 8
- 使用有意义的变量和函数名
- 添加适当的注释和文档字符串
- 确保所有测试通过

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🔗 相关链接

- [Claude Code 官方文档](https://claude.ai/code)
- [Claude Agent SDK](https://docs.anthropic.com/claude/docs)
- [BMAD Method 框架](https://bmad-method.com/)
- [MCP 协议规范](https://modelcontextprotocol.io/)

## 📈 项目状态

![GitHub stars](https://img.shields.io/github/stars/zsutxz/ClaudeLearning?style=social)
![GitHub forks](https://img.shields.io/github/forks/zsutxz/ClaudeLearning?style=social)
![GitHub issues](https://img.shields.io/github/issues/zsutxz/ClaudeLearning)
![GitHub pull requests](https://img.shields.io/github/issues-pr/zsutxz/ClaudeLearning)

## 🏆 致谢

感谢以下开源项目和社区：

- [Anthropic Claude](https://anthropic.com/) - 提供强大的AI模型支持
- [BMAD Method](https://bmad-method.com/) - 企业级开发框架
- [MCP Community](https://modelcontextprotocol.io/) - 工具集成标准

---

<div align="center">
  <p>用 ❤️ 和 ☕ 制作</p>
  <p>© 2024 ClaudeLearning 项目</p>
</div>