# Research Agent - 技术调研专业代理

## 语言和Git规则
- **语言**：使用中文进行交流
- **Git规则**：不自动提交任何代码更改，手动提交前需要明确确认

## 🎯 Project Overview
Research Agent是基于Claude Agent SDK构建的专业技术调研代理，专注于技术趋势分析、架构评估和工具选型。该项目继承自ClaudeLearning项目的多模型支持架构，提供全面的文献检索、数据收集和报告生成功能。

## 🚀 快速开始

### 环境配置
```bash
# 1. 进入Research目录
cd Research

# 2. 创建环境变量文件
cp .env.example .env

# 3. 编辑.env文件，添加必要的API密钥
# ANTHROPIC_API_KEY=your_anthropic_api_key
# GITHUB_TOKEN=your_github_token
# KAGGLE_USERNAME=your_kaggle_username
# KAGGLE_KEY=your_kaggle_key

# 4. 安装依赖（建议使用虚拟环境）
pip install -r requirements.txt
```

### 基础使用示例
```python
from research_agent import ResearchAgent, quick_research

# 方式1：使用ResearchAgent类
agent = ResearchAgent(
    research_domain="人工智能",
    provider="claude",
    model="glm-4.7"
)

result = await agent.conduct_research(
    query="大语言模型的最新发展趋势",
    max_sources=10,
    output_format="markdown"
)

# 方式2：使用快速研究函数
result = await quick_research(
    query="机器学习在医疗诊断中的应用",
    research_domain="医疗AI",
    max_sources=5
)

# 查看生成的报告
print(result.report)
```

## 🏗️ 项目架构

### 继承架构
```
ClaudeLearning主项目
└── AgentSdkTest/MultiAIAgent.py
    └── UniversalAIAgent (基础AI代理)
        └── UniversalTaskAgent (任务代理层)
            └── ResearchAgent (技术调研代理)
```

### 目录结构
```
Research/
├── research_agent.py              # ResearchAgent主类，继承UniversalTaskAgent
├── example_usage.py               # 详细使用示例和测试程序
├── modules/                       # 核心功能模块
│   ├── __init__.py               # 模块导入和工厂函数
│   ├── literature_retriever/     # 文献检索模块
│   │   ├── __init__.py
│   │   └── literature_retriever.py
│   ├── data_processor.py         # 数据处理模块
│   ├── report_generator.py       # 报告生成模块
│   └── quality_checker.py        # 质量检查模块
├── mcp_servers/                   # MCP服务器实现
│   ├── __init__.py
│   └── research_server.py        # 研究工具MCP服务器
├── config/                        # 配置文件
│   └── research_mcp.json         # MCP服务器配置
├── test/                          # 测试文件
│   ├── __init__.py
│   └── test_research_agent.py     # 核心功能测试
├── reports/                       # 生成的报告目录
├── templates/                     # 报告模板目录
├── requirements.txt               # 项目依赖
├── .env.example                   # 环境变量示例
└── README.md                      # 项目说明
```

## 🔧 核心功能

### 1. ResearchAgent主类
- **继承关系**: 继承自UniversalTaskAgent
- **核心方法**:
  - `conduct_research()`: 执行完整的技术调研流程
  - `_search_literature()`: 文献检索
  - `_process_data()`: 数据处理
  - `_generate_analysis()`: 分析生成
  - `_generate_report()`: 报告生成

### 2. 功能模块系统
- **LiteratureRetriever**: 文献检索模块
- **DataProcessor**: 数据处理模块
- **ReportGenerator**: 报告生成模块
- **QualityChecker**: 质量检查模块

### 3. MCP服务器集成
提供7个核心研究工具：
- `search_literature`: 文献检索
- `analyze_repository`: 仓库分析
- `fetch_paper`: 获取论文详情
- `process_data`: 数据处理
- `generate_report`: 报告生成
- `check_quality`: 质量检查
- `search_github`: GitHub搜索

## 📦 依赖管理

### 核心依赖（继承自AgentSdkTest）
```txt
anthropic>=0.3.0      # Claude API
openai>=1.0.0         # OpenAI API
requests>=2.28.0      # HTTP客户端
python-dotenv>=1.0.0  # 环境变量管理
```

### 研究专用依赖
```txt
# 数据处理
pandas>=2.0.0         # 数据分析
numpy>=1.24.0         # 数值计算
beautifulsoup4>=4.12.0 # 网页解析

# API集成
PyGithub>=1.59.0      # GitHub API
kaggle>=1.5.0         # Kaggle API
arxiv>=1.4.0          # arXiv API
scholarly>=1.7.0      # Google Scholar

# 报告生成
jinja2>=3.1.0         # 模板引擎
markdown>=3.5.0       # Markdown处理
reportlab>=4.0.0      # PDF生成
matplotlib>=3.7.0     # 数据可视化
```

## 🛠️ 常用命令

### 开发和测试
```bash
# 运行基础测试
python test/test_research_agent.py

# 运行使用示例
python example_usage.py

# 直接运行Research Agent
python research_agent.py
```

### 使用pytest
```bash
# 运行所有测试
pytest

# 运行特定测试类
pytest test/test_research_agent.py::TestResearchAgent

# 运行异步测试
pytest test/test_research_agent.py::TestAsyncFunctions -v
```

### 代码质量检查
```bash
# 代码格式化
black *.py modules/**/*.py mcp_servers/**/*.py

# 代码风格检查
flake8 --max-line-length=100

# 类型检查
mypy research_agent.py
```

## ⚙️ 配置说明

### 环境变量配置（.env）
```bash
# Claude API配置
ANTHROPIC_API_KEY=your_anthropic_api_key
ANTHROPIC_BASE_URL=https://open.bigmodel.cn/api/anthropic  # 使用glm-4.6

# GitHub API（仓库分析）
GITHUB_TOKEN=your_github_personal_access_token

# Kaggle API（数据集获取）
KAGGLE_USERNAME=your_kaggle_username
KAGGLE_KEY=your_kaggle_api_key

# 数据库配置（可选）
DATABASE_URL=sqlite:///research_data.db
REDIS_URL=redis://localhost:6379/0
```

### ResearchConfig配置类
```python
@dataclass
class ResearchConfig:
    research_domain: str = "人工智能"  # 研究领域
    max_sources: int = 20            # 最大文献数量
    output_format: str = "markdown"   # 输出格式
    include_github: bool = True       # 是否包含GitHub
    include_papers: bool = True       # 是否包含论文
    include_blogs: bool = True        # 是否包含博客
    cache_results: bool = True        # 是否缓存结果
```

### MCP服务器配置（config/research_mcp.json）
定义了4个MCP服务器：
- **research-tools**: 主要研究工具集成
- **github-server**: GitHub API专用服务器
- **arxiv-server**: 学术论文检索服务器
- **kaggle-server**: 数据集获取服务器

## 🔄 开发工作流程

### 1. 新功能开发流程
```bash
# 1. 创建功能分支
git checkout -b feature/new-feature

# 2. 开发功能
# - 在modules/中添加新模块
# - 或在research_agent.py中添加新方法

# 3. 添加测试
# 在test/test_research_agent.py中添加相应测试

# 4. 运行测试
pytest

# 5. 提交代码（手动确认）
git add .
git commit -m "feat: 添加新功能描述"
```

### 2. 模块开发指南
```python
# 新模块应遵循以下模式：
class NewModule:
    def __init__(self, agent):
        self.agent = agent

    async def process(self, data: Dict[str, Any]) -> Dict[str, Any]:
        """处理数据的异步方法"""
        # 实现具体功能
        pass
```

### 3. MCP工具开发指南
```python
# 在mcp_servers/research_server.py中添加新工具
async def new_tool(self, args: Dict[str, Any]) -> ToolResult:
    """新工具实现"""
    try:
        # 获取参数
        param = args.get("param", "")

        # 实现功能
        result = {"status": "success", "data": "..."}

        return ToolResult(
            content=[
                {"type": "text", "text": "工具执行成功"},
                {"type": "json", "json": result}
            ]
        )
    except Exception as e:
        return ToolResult(
            content=[{"type": "text", "text": f"错误: {str(e)}"}],
            is_error=True
        )
```

## 🔍 调试和故障排除

### 常见问题

**问题1: ImportError: No module named 'MultiAIAgent'**
- 确保在正确的目录运行（Research目录）
- 检查上级目录是否包含AgentSdkTest/MultiAIAgent.py

**问题2: API密钥错误**
- 检查.env文件是否正确配置
- 确保API密钥有效且有足够权限

**问题3: 异步函数执行错误**
- 使用`await`关键字调用异步方法
- 在测试中使用`run_async_test`辅助函数

### 调试技巧
```python
# 启用详细日志
import logging
logging.basicConfig(level=logging.DEBUG)

# 使用Mock模式测试
agent = ResearchAgent(provider="mock", model="mock-model")

# 查看配置
print(f"研究领域: {agent.research_domain}")
print(f"AI提供商: {agent.provider}")
```

## 📊 性能优化

### 缓存策略
- 使用Redis缓存API响应
- 本地SQLite数据库存储历史记录
- 配置CACHE_TTL控制缓存时间

### 异步处理
- 所有API调用使用异步模式
- 并发处理多个数据源
- 使用asyncio.gather()提高效率

### 请求限制
- MAX_CONCURRENT_REQUESTS控制并发数
- REQUEST_TIMEOUT设置超时时间
- 使用令牌桶算法限制API调用频率

## 🧪 测试策略

### 测试覆盖范围
- ResearchAgent核心功能测试
- 各模块单元测试
- 异步函数测试
- 错误处理测试
- 集成测试

### 测试数据
- 使用Mock模式避免真实API调用
- 提供测试用的示例数据
- 模拟各种错误场景

## 📈 扩展开发

### 支持新的数据源
1. 在modules/中创建新的检索器
2. 在MCP服务器中添加相应工具
3. 更新ResearchConfig支持新选项

### 添加报告格式
1. 在ReportGenerator中添加新格式支持
2. 创建相应的模板文件
3. 更新_generate_report方法

### 集成新的AI模型
1. 在AgentSdkTest的MultiAIAgent中添加支持
2. 在ResearchAgent中配置模型特定参数
3. 添加模型测试用例

## 🔗 相关文档

- [ClaudeLearning主项目](https://github.com/zsutxz/ClaudeLearning)
- [Claude Agent SDK文档](https://docs.anthropic.com/claude/docs)
- [MCP协议规范](https://modelcontextprotocol.io/)
- [BMAD开发框架](../.bmad/)

---

*更新时间: 2024-01-15*
*版本: 1.0.0*