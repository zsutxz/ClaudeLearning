"""
测试主程序 - 不需要真实API密钥
"""
import asyncio
import json

# 模拟的测试数据
MOCK_RESEARCH_RESULT = {
    "technology": "Python",
    "status": "completed",
    "search_results": {
        "technology": "Python",
        "total_results": 5,
        "results": [
            {
                "title": "Python 官方文档",
                "link": "https://docs.python.org",
                "snippet": "Python官方文档，包含完整的语言参考和标准库文档",
                "source": "google"
            },
            {
                "title": "Python Tutorial for Beginners",
                "link": "https://example.com/python-tutorial",
                "snippet": "适合初学者的Python教程，涵盖基础语法和概念",
                "source": "google"
            }
        ]
    },
    "analysis": {
        "trends": {
            "trends": ["web development", "data science", "machine learning", "automation"],
            "growth_topics": ["fastapi", "django", "pandas"],
            "declining_topics": [],
            "monthly_activity": {"2024-01": 15, "2024-02": 20, "2024-03": 18},
            "total_articles": 53
        },
        "categories": {
            "tutorials": [
                {"title": "Python Tutorial for Beginners", "summary": "基础教程"}
            ],
            "documentation": [
                {"title": "Python 官方文档", "summary": "官方文档"}
            ],
            "research_papers": [],
            "news": [],
            "case_studies": [],
            "tools": []
        },
        "difficulty": {
            "overall_difficulty": "beginner",
            "distribution": {"beginner": 3, "intermediate": 1, "advanced": 1},
            "total_articles": 5
        },
        "summary": {
            "summary": "Python是一门易学易用的编程语言，广泛应用于web开发、数据科学和机器学习领域。找到大量优质的初学者资源。",
            "key_insights": [
                "最新内容: Python 3.12新特性介绍",
                "热门主题: web development, data science, machine learning",
                "内容分布: tutorials: 1篇, documentation: 1篇"
            ],
            "total_sources": 2,
            "categories": {"tutorials": 1, "documentation": 1}
        }
    },
    "report": """
# Python技术研究报告

## 1. 技术概述
Python是一门高级编程语言，以其简洁的语法和强大的功能而闻名。它支持多种编程范式，包括面向对象、函数式和过程式编程。

## 2. 最新趋势
- **Web开发**: Django和Flask框架持续流行
- **数据科学**: Pandas、NumPy、Matplotlib生态系统成熟
- **机器学习**: TensorFlow、PyTorch支持度提升
- **自动化**: 脚本和DevOps工具广泛应用

## 3. 学习资源分类
- **教程资源**: 大量免费和付费教程
- **官方文档**: 完整且权威的参考文档
- **实践项目**: 开源项目和实战案例

## 4. 难度评估
Python被评为初学者友好的语言，学习曲线相对平缓。

## 5. 关键洞察
- 生态系统成熟，资源丰富
- 社区活跃，支持良好
- 应用领域广泛，就业机会多

## 6. 推荐下一步行动
- 从基础语法开始学习
- 选择感兴趣的应用领域深入
- 参与开源项目积累经验
    """,
    "timestamp": "2024-03-15T10:30:00"
}

MOCK_LEARNING_PLAN = {
    "technology": "Python",
    "experience_level": "beginner",
    "duration_hours": 30,
    "learning_plan": """
# Python初学者学习方案

## 学习目标
- 掌握Python基础语法和概念
- 能够编写简单的Python程序
- 理解面向对象编程基础
- 完成至少2个实践项目

## 阶段规划

### 第一阶段：基础入门 (预计 12 小时)
**学习重点:**
- Python语法基础
- 数据类型和变量
- 控制流程

**具体内容:**
- 变量、字符串、数字操作
- 列表、字典、元组、集合
- 条件语句和循环
- 函数定义和调用

**实践项目:**
- 简单计算器
- 待办事项列表

**预计时长:** 12小时 (2周)

**学习资源推荐:**
- Python官方教程
- "Python编程：从入门到实践"

### 第二阶段：进阶提升 (预计 9 小时)
**学习重点:**
- 面向对象编程
- 文件操作
- 异常处理

**具体内容:**
- 类和对象
- 继承和多态
- 文件读写
- 错误和异常处理

**实践项目:**
- 学生管理系统
- 文件处理工具

**预计时长:** 9小时 (1.5周)

### 第三阶段：高级应用 (预计 6 小时)
**学习重点:**
- 模块和包
- 标准库使用
- 简单的Web开发

**具体内容:**
- 模块导入和使用
- 常用标准库介绍
- Flask基础

**实践项目:**
- 个人博客系统

**预计时长:** 6小时 (1周)

### 第四阶段：专家精进 (预计 3 小时)
**学习重点:**
- 代码优化
- 测试基础
- 社区参与

**具体内容:**
- 代码重构技巧
- 单元测试基础
- 开源项目贡献

**实践项目:**
- 贡献小型开源项目

**预计时长:** 3小时 (0.5周)

## 学习建议
1. **学习方法**: 理论与实践相结合，多写代码
2. **学习工具**: VS Code + Python插件
3. **注意事项**: 坚持每天编码，循序渐进

## 里程碑检查点
- 能独立编写100行以上的程序
- 完成至少2个完整项目
- 能够阅读和理解他人代码

## 扩展资源
- **进阶方向**: Web开发、数据科学、机器学习
- **推荐书籍**: "流畅的Python"、"Python Cookbook"
- **在线课程**: Coursera、edX上的Python课程
    """,
    "resources": {
        "official_docs": ["Python官方文档", "Python语言参考"],
        "tutorials": ["Python官方教程", "W3Schools Python教程"],
        "books": ["Python编程：从入门到实践", "流畅的Python"],
        "courses": ["Coursera Python课程", "edX Python入门"],
        "tools": ["VS Code", "PyCharm", "Jupyter Notebook"],
        "communities": ["Python官方论坛", "Stack Overflow", "GitHub"]
    },
    "estimated_timeline": {
        "total_hours": 30,
        "beginner_phase": {"hours": 12, "weeks": 2},
        "intermediate_phase": {"hours": 9, "weeks": 2},
        "advanced_phase": {"hours": 6, "weeks": 1},
        "expert_phase": {"hours": 3, "weeks": 1}
    },
    "success_metrics": [
        "完成所有阶段的学习目标",
        "独立完成至少2个实践项目",
        "能够解决常见Python编程问题",
        "掌握Python基础概念和语法",
        "能够搭建Python开发环境",
        "完成入门级项目"
    ]
}

class MockTechLearningAssistant:
    """模拟的技术学习助手"""

    def __init__(self):
        pass

    async def create_learning_plan(self, technology: str,
                                 experience_level: str = "beginner",
                                 duration_hours: int = None,
                                 preferences: dict = None):
        """模拟创建学习方案"""
        print(f"开始为技术 '{technology}' 生成学习方案...")
        print(f"经验水平: {experience_level}")
        print(f"学习时长: {duration_hours or 30} 小时")

        if preferences:
            print(f"个性化偏好: {preferences}")

        print("-" * 50)
        print("学习方案生成成功!")

        # 返回模拟结果
        result = {
            "status": "completed",
            "data": {
                **MOCK_LEARNING_PLAN,
                "technology": technology,
                "experience_level": experience_level,
                "duration_hours": duration_hours or 30,
                "research_summary": MOCK_RESEARCH_RESULT["analysis"]["summary"],
                "research_report": MOCK_RESEARCH_RESULT["report"],
                "personalization_applied": bool(preferences),
                "timestamp": MOCK_RESEARCH_RESULT["timestamp"]
            }
        }

        self._print_success_result(result["data"])
        return result

    def _print_success_result(self, data):
        """打印成功结果"""
        print("=" * 60)
        print(f"技术: {data.get('technology', '未知')}")
        print(f"经验水平: {data.get('experience_level', '未知')}")
        print(f"计划时长: {data.get('duration_hours', 0)} 小时")
        print(f"生成时间: {data.get('timestamp', '未知')}")
        print("=" * 60)

        # 研究摘要
        research_summary = data.get('research_summary', {})
        if research_summary:
            print("\n研究摘要:")
            print(f"   {research_summary.get('summary', '无摘要')}")
            key_insights = research_summary.get('key_insights', [])
            if key_insights:
                print("   关键洞察:")
                for insight in key_insights:
                    print(f"   - {insight}")

        # 资源推荐
        resources = data.get('resources', {})
        if resources:
            print("\n学习资源:")
            for category, items in resources.items():
                if items:
                    print(f"   {category}:")
                    for item in items:
                        print(f"   - {item}")

        # 时间线
        timeline = data.get('timeline', {})
        if timeline:
            print(f"\n学习时间线:")
            print(f"   总时长: {timeline.get('total_hours', 0)} 小时")

        print("=" * 60)

    def save_result(self, result, filename=None):
        """保存结果"""
        if filename is None:
            technology = result.get("data", {}).get("technology", "unknown")
            filename = f"test_learning_plan_{technology}.json"

        try:
            with open(filename, 'w', encoding='utf-8') as f:
                json.dump(result, f, ensure_ascii=False, indent=2)
            print(f"结果已保存到: {filename}")
        except Exception as e:
            print(f"保存文件失败: {e}")


async def main():
    """测试主函数"""
    print("技术学习助手 - 测试模式")
    print("=" * 40)

    assistant = MockTechLearningAssistant()

    # 测试用例1: 基础Python学习
    print("\n=== 测试1: Python基础学习 ===")
    result1 = await assistant.create_learning_plan(
        technology="Python",
        experience_level="beginner",
        duration_hours=30
    )

    if result1["status"] == "completed":
        assistant.save_result(result1, "test_python_plan.json")

    print("\n=== 测试2: React进阶学习 ===")
    result2 = await assistant.create_learning_plan(
        technology="React",
        experience_level="intermediate",
        duration_hours=40,
        preferences={
            "learning_style": "hands-on",
            "preferred_time": "evening",
            "focus": "practical_projects"
        }
    )

    if result2["status"] == "completed":
        assistant.save_result(result2, "test_react_plan.json")

    print("\n所有测试完成!")


if __name__ == "__main__":
    asyncio.run(main())