# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 🎯 Project Overview
这是一个基于LangGraph构建的智能技术学习助手，能够自动收集指定IT技术的最新资料，进行分析总结，并生成个性化学习方案。使用LangChain作为LLM框架，支持多种API配置。

## 🏗️ 项目架构

### 核心组件架构
```
langgraph-agent/
├── main.py                    # 主程序入口 - TechLearningAssistant类，CLI和交互接口
├── config/settings.py         # 应用配置 - API密钥、模型设置、DeepSeek支持
├── src/tech_learning_workflow.py  # 工作流引擎 - LangGraph状态管理和流程编排
├── agents/                    # 智能体模块
│   ├── research_agent.py      # 研究智能体 - 技术资料收集和分析
│   └── learning_agent.py      # 学习智能体 - 个性化学习方案生成
├── tools/                     # 工具模块
│   ├── web_searcher.py        # 网络搜索 - Google搜索、Arxiv论文、RSS订阅
│   └── content_analyzer.py    # 内容分析 - 关键概念提取、趋势分析
├── examples/basic_usage.py    # 使用示例 - 基础、高级、批量、个性化示例
├── testresearch.py            # 搜索功能测试
├── testdeepseek.py            # DeepSeek配置测试
└── requirements.txt           # Python依赖列表
```

### LangGraph工作流设计
工作流采用状态机模式，包含以下节点：
- **validate_input**: 输入参数验证和标准化
- **research_technology**: 技术资料收集和分析
- **generate_learning_plan**: 基础学习方案生成
- **customize_plan**: 个性化定制（可选）
- **generate_final_output**: 最终结果整合
- **handle_error**: 错误处理

### 智能体协作模式
- **ResearchAgent**: 使用WebSearcher和ContentAnalyzer收集技术资料
- **LearningAgent**: 基于研究结果生成个性化学习方案
- **状态传递**: 通过WorkflowState在智能体间传递数据

## ⚙️ 开发环境配置

### 1. 环境变量设置
```bash
# 复制配置模板
cp .env.example .env

# 编辑 .env 文件，添加你的API密钥
OPENAI_API_KEY=your_openai_api_key_here                    # 必需
SERPER_API_KEY=your_serper_api_key_here                  # 可选，用于Google搜索

# 可选的其他LLM API
ANTHROPIC_API_KEY=your_anthropic_api_key_here
```

### 2. 依赖安装
```bash
# 创建虚拟环境
python -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate

# 安装依赖
pip install -r requirements.txt
```

## 🚀 常用命令

### 基础运行
```bash
# 命令行模式 - 基础用法
python main.py "Python" --level beginner --hours 30

# 命令行模式 - 个性化偏好
python main.py "Machine Learning" --level advanced --hours 60 --preferences '{"learning_style": "hands-on"}'

# 交互模式
python main.py --interactive

# 保存结果到文件
python main.py "React" --level intermediate --output react_plan.json
```

### 开发和测试
```bash
# 运行完整示例（包含所有用法示例）
python examples/basic_usage.py

# 测试搜索功能
python testresearch.py

# 测试特定功能
python testdeepseek.py
```

### 配置管理
```bash
# 验证配置
python -c "from config.settings import settings; print(settings.validate_config())"
```

## 📊 核心模块使用

### 1. TechLearningAssistant (main.py:13)
主要用户接口类，提供完整的学习方案生成功能。

```python
from main import TechLearningAssistant
import asyncio

async def create_plan():
    assistant = TechLearningAssistant()
    result = await assistant.create_learning_plan(
        technology="Python",
        experience_level="beginner",
        duration_hours=30,
        preferences={
            "learning_style": "visual",
            "preferred_time": "evening"
        }
    )

    if result["status"] == "completed":
        assistant.save_result(result)
    return result

# 运行
result = asyncio.run(create_plan())
```

### 2. TechLearningWorkflow (src/tech_learning_workflow.py:41)
LangGraph工作流引擎，管理整个学习方案生成流程。

```python
from src.tech_learning_workflow import TechLearningWorkflow

workflow = TechLearningWorkflow()
result = await workflow.run(
    technology="React",
    experience_level="intermediate",
    duration_hours=40,
    preferences={"learning_style": "hands-on"}
)
```

### 3. WorkflowState (src/tech_learning_workflow.py:28)
LangGraph状态定义，包含完整的工作流状态管理。

```python
class WorkflowState(TypedDict):
    messages: Annotated[list, add_messages]
    technology: str
    experience_level: str
    duration_hours: int
    preferences: Dict[str, Any]
    research_results: Optional[Dict[str, Any]]
    learning_plan: Optional[Dict[str, Any]]
    error: Optional[str]
    status: str
```

### 4. ResearchAgent (agents/research_agent.py:14)
研究智能体，负责技术资料收集和初步分析。

```python
from agents.research_agent import ResearchAgent

agent = ResearchAgent()
research_results = await agent.search_technology_info("Docker")
```

### 5. WebSearcher (tools/web_searcher.py:16)
网络搜索工具，支持多种搜索源和内容提取。

```python
from tools.web_searcher import WebSearcher

async with WebSearcher() as searcher:
    google_results = await searcher.search_google("Python tutorial")
    arxiv_papers = await searcher.search_arxiv("machine learning")
```

## 🛠️ 开发指南

### 扩展搜索源
在 `tools/web_searcher.py` 中添加新的搜索方法：

```python
async def search_new_source(self, query: str) -> List[Dict[str, Any]]:
    """添加新的搜索源"""
    # 实现新搜索源的逻辑
    pass
```

### 修改学习方案生成逻辑
在 `agents/learning_agent.py` 中自定义prompt和模板：

```python
def generate_learning_plan(self, technology: str, analysis: Dict[str, Any],
                         duration_hours: int = None, experience_level: str = "beginner"):
    """自定义学习方案生成逻辑"""
    # 修改prompt模板和生成逻辑
    pass
```

### 扩展工作流
在 `src/tech_learning_workflow.py` 中添加新的处理节点：

```python
def _create_workflow(self) -> StateGraph:
    """扩展工作流"""
    workflow = StateGraph(WorkflowState)

    # 添加新节点
    workflow.add_node("new_processing_step", self._new_processing_step)

    # 添加边连接
    workflow.add_edge("research_technology", "new_processing_step")
    workflow.add_edge("new_processing_step", "generate_learning_plan")

    return workflow.compile()
```

## 🔧 配置选项详解

### LLM配置 (config/settings.py)
- **OpenAI API**: 默认使用gpt-4o-mini模型
- **Anthropic API**: 可选的Claude模型支持
- **模型参数**: temperature=0.1, max_tokens=4000

### 搜索配置
- **Google搜索**: 通过SERPER_API_KEY实现，返回最新网络内容
- **Arxiv搜索**: 自动检索相关学术论文，免费使用
- **RSS订阅**: 内置技术博客源，可扩展添加更多源

### 应用参数
- **DEBUG**: 调试模式开关 (False)
- **MAX_RETRIES**: API失败重试次数 (3)
- **TIMEOUT**: 网络请求超时时间 (30秒)
- **MAX_SEARCH_RESULTS**: 搜索结果数量限制 (10)
- **DEFAULT_COURSE_DURATION**: 默认课程时长 (20小时)

## 📈 输出格式规范

### 标准返回结构
```json
{
  "status": "completed|error",
  "data": {
    "technology": "Python",
    "experience_level": "beginner",
    "duration_hours": 30,
    "research_summary": {
      "summary": "技术分析摘要",
      "key_insights": ["关键洞察1", "关键洞察2"]
    },
    "research_report": "详细研究报告",
    "learning_plan": "完整学习方案",
    "resources": {
      "official_docs": ["官方文档1", "官方文档2"],
      "tutorials": ["教程1", "教程2"],
      "books": ["书籍1", "书籍2"]
    },
    "timeline": {
      "total_hours": 30,
      "beginner_phase": {"hours": 12, "weeks": 2},
      "intermediate_phase": {"hours": 9, "weeks": 1},
      "advanced_phase": {"hours": 6, "weeks": 1},
      "expert_phase": {"hours": 3, "weeks": 0.5}
    },
    "success_metrics": ["成功指标1", "成功指标2"],
    "timestamp": "2024-xx-xx",
    "personalization_applied": true
  },
  "error": "错误信息 (仅在status=error时)"
}
```

### 学习偏好配置格式
```json
{
  "learning_style": "visual|hands-on|theoretical",
  "preferred_time": "morning|evening|flexible",
  "focus": ["specific_topics"],
  "tools": ["preferred_tools"],
  "project_type": "personal|professional|research",
  "background": "user_background"
}
```

## 🚨 错误处理

### 常见问题诊断
1. **API密钥错误**: 检查.env文件中的OPENAI_API_KEY配置
2. **搜索结果为空**:
   - 检查网络连接
   - 验证SERPER_API_KEY是否配置
   - 尝试使用更通用的技术名称
3. **学习方案生成失败**:
   - 确认OPENAI_API_KEY已正确设置
   - 检查API余额是否充足
   - 尝试减少请求的token数量
4. **程序运行缓慢**:
   - 调整MAX_RETRIES和TIMEOUT参数
   - 减少MAX_SEARCH_RESULTS数量

### 调试模式
```bash
# 启用调试模式
export DEBUG=True
python main.py "Python" --level beginner
```

### 配置验证
```bash
# 验证所有必需配置
python -c "from config.settings import settings; exit(0 if settings.validate_config() else 1)"
```

## 🔄 Git配置
根据上级目录的CLAUDE.md规则：
- **不自动提交** - Claude不会自动提交任何代码更改
- 手动提交前需要明确确认

## 📝 项目特性

### 核心功能
- **智能资料收集**: 自动搜索技术文档、教程、博客和学术论文
- **内容分析总结**: 提取关键概念、分析趋势、评估难度
- **个性化学习方案**: 根据用户经验水平和偏好生成定制化学习路径
- **多阶段学习规划**: 从入门到专家的完整学习路线
- **资源推荐**: 提供官方文档、教程、工具和社区资源
- **工作流自动化**: 基于LangGraph的智能化处理流程

### 技术特点
- **LangGraph工作流**: 状态机模式管理复杂流程
- **异步处理**: 全面使用asyncio提升性能
- **模块化设计**: 独立的智能体和工具组件
- **多API支持**: OpenAI、Anthropic等多种LLM后端
- **错误恢复**: 完善的错误处理和重试机制