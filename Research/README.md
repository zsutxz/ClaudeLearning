# Research Agent - 技术调研专业代理

## 项目概述

Research Agent是基于Claude Agent SDK构建的专业技术调研代理，专注于技术趋势分析、架构评估和工具选型。本项目充分利用ClaudeLearning项目现有的多模型支持、MCP集成和技能生态系统。

## 核心功能

### 🔍 文献检索分析
- **GitHub仓库分析**: 开源项目趋势、README分析、Issue讨论
- **学术论文检索**: arXiv、IEEE Xplore、ACM Digital Library
- **技术博客追踪**: Medium、Dev.to、个人技术博客
- **官方文档分析**: API文档、技术白皮书、架构文档

### 📊 数据收集处理
- **Kaggle数据集**: 机器学习数据集信息和分析
- **API数据集成**: REST API、GraphQL接口数据获取
- **网页数据抓取**: 结构化数据提取和解析
- **数据质量评估**: 来源可信度、内容相关性验证

### 📝 报告生成
- **多格式输出**: Markdown、HTML、PDF格式报告
- **智能模板**: 技术趋势、架构对比、工具评估模板
- **可视化图表**: 数据可视化和技术趋势图表
- **自动引用**: 智能引用管理和参考文献生成

## 项目结构

```
Research/
├── research_agent.py              # ResearchAgent主类
├── modules/                       # 核心功能模块
│   ├── literature_retriever/      # 文献检索模块
│   ├── data_processor/            # 数据处理模块
│   ├── report_generator/          # 报告生成模块
│   ├── quality_checker/           # 质量检查模块
│   └── tool_manager/              # 工具管理模块
├── mcp_servers/                   # MCP服务器
│   ├── github_server.py           # GitHub API集成
│   ├── arxiv_server.py            # 学术论文服务器
│   └── kaggle_server.py           # Kaggle数据服务器
├── config/                        # 配置文件
├── templates/                     # 报告模板
├── test/                          # 测试用例
├── requirements.txt               # 依赖管理
├── .env.example                   # 环境变量示例
└── README.md                      # 项目说明
```

## 技术架构

### 继承架构
```
UniversalAIAgent (ClaudeLearning基础)
    └── UniversalTaskAgent (任务代理层)
        └── ResearchAgent (技术调研代理)
```

### 技术栈
- **AI模型**: Claude (glm-4.6)、OpenAI、DeepSeek、Ollama
- **Python版本**: 3.13+
- **核心框架**: Claude Agent SDK、MCP (Model Context Protocol)
- **数据处理**: pandas、beautifulsoup4、PyGithub
- **报告生成**: jinja2、markdown、pdfkit

## 快速开始

### 1. 环境配置
```bash
# 复制环境变量模板
cp .env.example .env

# 编辑.env文件，添加API密钥
# ANTHROPIC_API_KEY=your_anthropic_key
# GITHUB_TOKEN=your_github_token
# KAGGLE_USERNAME=your_kaggle_username
# KAGGLE_KEY=your_kaggle_key
```

### 2. 安装依赖
```bash
pip install -r requirements.txt
```

### 3. 基础使用
```python
from research.research_agent import ResearchAgent

# 创建研究代理
agent = ResearchAgent(
    research_domain="人工智能",
    provider="claude",
    model="glm-4.6"
)

# 执行技术调研
result = await agent.conduct_research(
    query="大语言模型的最新发展趋势",
    report_type="tech_trends"
)

print(result['report'])
```

## 使用示例

### GitHub技术趋势分析
```python
# 分析GitHub上的技术趋势
result = await agent.analyze_github_trends(
    topic="machine learning",
    time_range="6months",
    min_stars=100
)
```

### 学术论文调研
```python
# 检索相关学术论文
papers = await agent.search_academic_papers(
    query="transformer architecture",
    max_results=20,
    sort_by="relevance"
)
```

### 生成技术报告
```python
# 生成完整的技术调研报告
report = await agent.generate_tech_report(
    topic="微服务架构最佳实践",
    include_github=True,
    include_papers=True,
    output_format="markdown"
)
```

## MCP服务器集成

本项目扩展了ClaudeLearning的MCP配置，新增以下服务器：

- **research-tools**: 主要研究工具集成
- **github-server**: GitHub API专用服务器
- **arxiv-server**: 学术论文检索服务器
- **kaggle-server**: 数据集获取服务器

## 技能集成

充分利用ClaudeLearning现有的28个专业技能，重点使用：

- **content-research-writer**: 研究写作和引用管理
- **code-architecture-analyzer**: 技术架构分析
- **ai-news-aggregator**: 最新技术动态
- **pdf/docx技能**: 文档解析和处理

## 开发状态

- [x] 第一阶段: 核心基础 (MVP版本)
  - [x] ResearchAgent基础类
  - [x] GitHub集成
  - [x] 基础报告生成
  - [ ] MCP服务器配置

- [ ] 第二阶段: 数据集成
  - [ ] arXiv论文检索
  - [ ] Kaggle数据集成
  - [ ] 数据质量评估

- [ ] 第三阶段: 高级功能
  - [ ] 技术趋势分析
  - [ ] 智能报告模板
  - [ ] 多格式输出

## 贡献指南

1. Fork本项目
2. 创建功能分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m 'Add amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 创建Pull Request

## 许可证

本项目遵循MIT许可证 - 详见[LICENSE](LICENSE)文件

## 联系方式

- 项目主页: [ClaudeLearning](https://github.com/zsutxz/ClaudeLearning)
- 问题反馈: [Issues](https://github.com/zsutxz/ClaudeLearning/issues)
- 技术讨论: [Discussions](https://github.com/zsutxz/ClaudeLearning/discussions)

---

*构建智能化的技术调研工具，让研究更高效、更专业。*