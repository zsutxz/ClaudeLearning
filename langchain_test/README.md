# 基于LangGraph的智能技术学习助手

一个使用LangGraph构建的智能体，能够自动收集指定IT技术的最新资料，进行分析总结，并生成个性化学习方案。

## 🌟 功能特性

- **智能资料收集**: 自动搜索最新的技术文档、教程、博客和学术论文
- **内容分析总结**: 提取关键概念、分析趋势、评估难度
- **个性化学习方案**: 根据用户经验水平和偏好生成定制化学习路径
- **多阶段学习规划**: 从入门到专家的完整学习路线
- **资源推荐**: 提供官方文档、教程、工具和社区资源
- **工作流自动化**: 基于LangGraph的智能化处理流程

## 🚀 快速开始

### 1. 环境准备

```bash
# 克隆项目
git clone <repository-url>
cd langgraph-agent

# 创建虚拟环境
python -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate

# 安装依赖
pip install -r requirements.txt
```

### 2. 配置API密钥

```bash
# 复制环境变量模板
cp .env.example .env

# 编辑 .env 文件，添加你的API密钥
OPENAI_API_KEY=your_openai_api_key_here
SERPER_API_KEY=your_serper_api_key_here  # 可选，用于Google搜索
```

### 3. 运行示例

```bash
# 基础使用示例
python examples/basic_usage.py

# 命令行模式
python main.py "Python" --level beginner --hours 30

# 交互模式
python main.py --interactive
```

## 📖 使用方法

### 命令行接口

```bash
# 基础用法
python main.py <技术名称> [选项]

# 选项说明
--level {beginner,intermediate,advanced}  # 经验水平 (默认: beginner)
--hours <数字>                            # 学习时长(小时)
--preferences '<JSON字符串>'              # 学习偏好
--output <文件名>                         # 输出文件名
--interactive                             # 交互模式

# 示例
python main.py "React" --level intermediate --hours 40
python main.py "Machine Learning" --level advanced --hours 60 --preferences '{"learning_style": "hands-on"}'
python main.py --interactive
```

### 编程接口

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

## 🏗️ 项目架构

```
langgraph-agent/
├── config/                    # 配置文件
│   └── settings.py           # 应用配置
├── tools/                     # 工具模块
│   ├── web_searcher.py       # 网络搜索工具
│   └── content_analyzer.py   # 内容分析工具
├── agents/                    # 智能体模块
│   ├── research_agent.py     # 研究智能体
│   └── learning_agent.py     # 学习方案生成智能体
├── src/                       # 核心模块
│   └── tech_learning_workflow.py  # 工作流定义
├── examples/                  # 使用示例
│   └── basic_usage.py        # 基础使用示例
├── main.py                    # 主程序入口
├── requirements.txt           # 依赖列表
├── .env.example              # 环境变量模板
└── README.md                 # 项目文档
```

## 🔧 核心组件

### 1. WebSearcher (网络搜索工具)
- Google搜索集成
- ArXiv学术论文搜索
- 技术博客RSS订阅
- 网页内容提取

### 2. ContentAnalyzer (内容分析器)
- 关键概念提取
- 技术趋势分析
- 内容分类评估
- 难度等级判断

### 3. ResearchAgent (研究智能体)
- 技术资料收集
- 内容分析处理
- 研究报告生成

### 4. LearningAgent (学习智能体)
- 个性化方案生成
- 学习路径规划
- 资源推荐匹配
- 进度跟踪建议

### 5. TechLearningWorkflow (工作流)
- 基于LangGraph的状态管理
- 自动化处理流程
- 错误处理机制
- 结果整合输出

## 📊 输出格式

```json
{
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
    "intermediate_phase": {"hours": 9, "weeks": 1}
  },
  "success_metrics": ["成功指标1", "成功指标2"],
  "timestamp": "2024-xx-xx"
}
```

## 🎯 使用场景

### 1. 个人学习规划
- 选择新技术进行学习
- 制定系统的学习计划
- 获取最新学习资源

### 2. 团队技术培训
- 为团队成员制定学习路径
- 统一技术栈学习方案
- 跟踪学习进度

### 3. 技术选型研究
- 评估新技术适用性
- 了解技术发展趋势
- 制定技术迁移方案

### 4. 教育机构
- 课程设计参考
- 教学资源整理
- 学习路径优化

## ⚙️ 配置选项

### 环境变量
- `OPENAI_API_KEY`: OpenAI API密钥 (必需)
- `SERPER_API_KEY`: Google搜索API密钥 (可选)
- `DEBUG`: 调试模式开关
- `MAX_RETRIES`: 最大重试次数
- `TIMEOUT`: 请求超时时间

### 学习偏好配置
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

## 🔍 故障排除

### 常见问题

**Q: 搜索结果为空或很少**
A:
- 检查网络连接
- 验证API密钥配置
- 尝试使用更通用的技术名称
- 检查SERPER_API_KEY是否配置

**Q: 学习方案生成失败**
A:
- 确认OPENAI_API_KEY已正确设置
- 检查API余额是否充足
- 尝试减少请求的token数量

**Q: 程序运行缓慢**
A:
- 调整并发请求数量
- 增加超时时间设置
- 减少搜索结果数量

### 调试模式

```bash
# 启用调试模式
export DEBUG=True
python main.py "Python" --level beginner
```

## 🤝 贡献指南

1. Fork 项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 📝 更新日志

### v1.0.0
- 初始版本发布
- 基础搜索和分析功能
- 学习方案生成
- LangGraph工作流集成

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情

## 🙏 致谢

- [LangGraph](https://github.com/langchain-ai/langgraph) - 工作流框架
- [LangChain](https://github.com/langchain-ai/langchain) - LLM应用框架
- [OpenAI](https://openai.com/) - 语言模型API
- [Serper](https://serper.dev/) - 搜索API服务

## 📞 联系方式

如有问题或建议，请通过以下方式联系:
- 提交 Issue
- 发送邮件
- 参与讨论

---

**注意**: 本项目仅供学习和研究使用，请遵守相关API的使用条款和限制。