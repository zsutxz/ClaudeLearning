# 使用指南

## 快速开始

### 1. 演示模式（无需API密钥）

```bash
# 运行演示版本
python demo.py
```

演示版本包含3个预设技术（Python、React、Docker），展示了完整的功能：
- 技术研究摘要
- 个性化学习方案
- 资源推荐
- 学习时间线

### 2. 完整模式（需要API密钥）

#### 配置API密钥
```bash
# 复制环境变量模板
cp .env.example .env

# 编辑 .env 文件，添加你的OpenAI API密钥
OPENAI_API_KEY=your_openai_api_key_here
```

#### 使用方法

**命令行模式：**
```bash
# 基础用法
python main.py "Python" --level beginner --hours 30

# 带个性化偏好
python main.py "React" --level intermediate --hours 40 --preferences '{"learning_style": "hands-on"}'

# 保存结果到文件
python main.py "Docker" --level advanced --hours 50 --output docker_plan.json
```

**交互模式：**
```bash
python main.py --interactive
```

## 功能特性

### 1. 智能技术研究
- 自动搜索最新技术资料
- 分析技术趋势和发展方向
- 评估学习难度和内容分类

### 2. 个性化学习方案
- 根据经验水平定制内容
- 支持多种学习风格偏好
- 提供分阶段学习路径

### 3. 资源推荐
- 官方文档和教程
- 推荐书籍和课程
- 开发工具和社区资源

### 4. 学习规划
- 详细的阶段划分
- 时间估算和进度跟踪
- 成功衡量指标

## 支持的经验水平

- **beginner**: 初学者，适合没有或很少经验的用户
- **intermediate**: 中级水平，适合有基础经验的用户
- **advanced**: 高级水平，适合有深入经验的用户

## 个性化偏好选项

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

## 输出格式

程序会生成JSON格式的学习方案，包含：
- 技术研究摘要
- 详细学习计划
- 推荐资源列表
- 学习时间线
- 成功衡量指标

## 示例输出

```json
{
  "technology": "Python",
  "experience_level": "beginner",
  "duration_hours": 30,
  "research_summary": {
    "summary": "Python是...",
    "key_insights": ["..."]
  },
  "learning_plan": "详细的学习计划...",
  "resources": {
    "official_docs": ["..."],
    "tutorials": ["..."]
  },
  "timeline": {
    "total_hours": 30,
    "phases": [...]
  },
  "success_metrics": ["..."]
}
```

## 故障排除

### 常见问题

1. **配置验证失败**
   - 检查 `.env` 文件是否存在
   - 确认 `OPENAI_API_KEY` 已正确设置

2. **搜索结果为空**
   - 确认网络连接正常
   - 尝试使用更通用的技术名称
   - 检查API密钥余额

3. **程序运行缓慢**
   - 网络连接可能较慢
   - 可以减少搜索结果数量

### 调试模式

在 `.env` 文件中设置：
```
DEBUG=True
```

## 扩展使用

### 批量处理

可以使用Python脚本批量生成多个技术的学习方案：

```python
import asyncio
from main import TechLearningAssistant

async def batch_generate():
    assistant = TechLearningAssistant()
    technologies = ["Python", "JavaScript", "Docker", "React"]

    for tech in technologies:
        result = await assistant.create_learning_plan(
            technology=tech,
            experience_level="beginner",
            duration_hours=20
        )
        assistant.save_result(result, f"{tech}_plan.json")

asyncio.run(batch_generate())
```

### 集成到其他项目

可以将这个工具集成到：
- 学习管理系统
- 技术培训平台
- 个人知识管理工具

## 注意事项

1. **API使用**: 需要有效的OpenAI API密钥
2. **网络依赖**: 需要稳定的网络连接进行搜索
3. **资源消耗**: 大量使用可能产生API费用
4. **结果质量**: 结果质量依赖于搜索数据和LLM能力

## 联系支持

如有问题或建议，请：
1. 查看错误信息和故障排除指南
2. 检查配置和网络连接
3. 提交Issue或反馈

---

**注意**: 本工具仅供学习和研究使用，请遵守相关API使用条款。