"""
演示版本 - 不需要真实API密钥
"""
import asyncio
import json
from typing import Dict, Any

# 模拟数据
DEMO_DATA = {
    "Python": {
        "summary": "Python是一门简洁易学的编程语言，广泛应用于web开发、数据科学、机器学习等领域。",
        "trends": ["web开发", "数据科学", "机器学习", "自动化"],
        "resources": {
            "official_docs": ["Python官方文档", "PEP文档"],
            "tutorials": ["Python官方教程", "Real Python", "W3Schools"],
            "books": ["Python编程：从入门到实践", "流畅的Python"],
            "tools": ["VS Code", "PyCharm", "Jupyter Notebook"]
        }
    },
    "React": {
        "summary": "React是Facebook开发的用于构建用户界面的JavaScript库，组件化思想深入人心。",
        "trends": ["Hooks", "Next.js", "TypeScript", "服务端渲染"],
        "resources": {
            "official_docs": ["React官方文档", "React Hooks文档"],
            "tutorials": ["React官方教程", "React Patterns"],
            "books": ["React深入技术手册", "React设计模式"],
            "tools": ["Create React App", "Next.js", "React DevTools"]
        }
    },
    "Docker": {
        "summary": "Docker是容器化技术的事实标准，简化了应用的部署和管理。",
        "trends": ["Kubernetes", "微服务", "DevOps", "云原生"],
        "resources": {
            "official_docs": ["Docker官方文档", "Docker Compose文档"],
            "tutorials": ["Docker入门教程", "Dockerfile最佳实践"],
            "books": ["Docker深入浅出", "Docker实战"],
            "tools": ["Docker Desktop", "Docker Hub", "Portainer"]
        }
    }
}

def generate_learning_plan(technology: str, experience_level: str, duration_hours: int, preferences: dict = None) -> Dict[str, Any]:
    """生成学习方案"""

    # 获取技术数据
    tech_data = DEMO_DATA.get(technology, DEMO_DATA["Python"])

    # 根据经验水平调整内容
    if experience_level == "beginner":
        phases = [
            {"name": "基础入门", "hours": duration_hours * 0.4, "weeks": 2, "content": "基础概念、环境搭建、语法入门"},
            {"name": "进阶提升", "hours": duration_hours * 0.3, "weeks": 1, "content": "核心特性、最佳实践"},
            {"name": "实践应用", "hours": duration_hours * 0.2, "weeks": 1, "content": "项目实战、问题解决"},
            {"name": "深入理解", "hours": duration_hours * 0.1, "weeks": 1, "content": "原理理解、扩展学习"}
        ]
    elif experience_level == "intermediate":
        phases = [
            {"name": "巩固基础", "hours": duration_hours * 0.2, "weeks": 1, "content": "基础回顾、查漏补缺"},
            {"name": "深入学习", "hours": duration_hours * 0.4, "weeks": 2, "content": "高级特性、设计模式"},
            {"name": "综合实践", "hours": duration_hours * 0.3, "weeks": 1, "content": "复杂项目、架构设计"},
            {"name": "专家水平", "hours": duration_hours * 0.1, "weeks": 1, "content": "源码分析、社区贡献"}
        ]
    else:  # advanced
        phases = [
            {"name": "专家回顾", "hours": duration_hours * 0.1, "weeks": 1, "content": "高级特性回顾"},
            {"name": "深度研究", "hours": duration_hours * 0.5, "weeks": 2, "content": "源码分析、性能优化"},
            {"name": "架构设计", "hours": duration_hours * 0.3, "weeks": 1, "content": "系统架构、最佳实践"},
            {"name": "技术领导", "hours": duration_hours * 0.1, "weeks": 1, "content": "技术分享、团队指导"}
        ]

    # 生成学习计划文本
    plan_text = f"# {technology}学习方案 ({experience_level})\n\n"
    plan_text += "## 学习目标\n"
    if experience_level == "beginner":
        plan_text += "- 掌握基础概念和语法\n- 能够完成简单项目\n- 理解核心工作原理\n"
    elif experience_level == "intermediate":
        plan_text += "- 深入理解高级特性\n- 能够设计中等复杂度项目\n- 掌握最佳实践\n"
    else:
        plan_text += "- 精通高级特性\n- 能够进行架构设计\n- 具备技术指导能力\n"

    plan_text += "\n## 学习阶段\n"
    for i, phase in enumerate(phases, 1):
        plan_text += f"\n### 第{i}阶段：{phase['name']} ({phase['hours']}小时, {phase['weeks']}周)\n"
        plan_text += f"**学习重点**: {phase['content']}\n"
        plan_text += f"**实践项目**: 根据阶段难度选择合适的项目\n"

    # 添加个性化建议
    if preferences:
        plan_text += "\n## 个性化建议\n"
        if preferences.get("learning_style") == "hands-on":
            plan_text += "- 重视实践项目，多写代码\n"
        elif preferences.get("learning_style") == "visual":
            plan_text += "- 多观看视频教程和图表\n"
        elif preferences.get("learning_style") == "theoretical":
            plan_text += "- 深入理解原理，阅读文档\n"

    return {
        "technology": technology,
        "experience_level": experience_level,
        "duration_hours": duration_hours,
        "research_summary": {
            "summary": tech_data["summary"],
            "key_insights": [
                f"技术趋势: {', '.join(tech_data['trends'][:3])}",
                f"学习资源丰富度: {len(tech_data['resources'])}个类别"
            ]
        },
        "learning_plan": plan_text,
        "resources": tech_data["resources"],
        "timeline": {
            "total_hours": duration_hours,
            "phases": phases
        },
        "success_metrics": [
            f"完成{len(phases)}个学习阶段",
            "独立完成实践项目",
            "能够解决常见问题"
        ],
        "personalization_applied": bool(preferences),
        "timestamp": "2024-03-15T10:30:00"
    }

class DemoTechLearningAssistant:
    """演示版技术学习助手"""

    async def create_learning_plan(self, technology: str,
                                 experience_level: str = "beginner",
                                 duration_hours: int = None,
                                 preferences: dict = None):
        """创建学习方案"""
        if duration_hours is None:
            duration_hours = 20

        print(f"开始为技术 '{technology}' 生成学习方案...")
        print(f"经验水平: {experience_level}")
        print(f"学习时长: {duration_hours} 小时")

        if preferences:
            print(f"个性化偏好: {preferences}")

        print("-" * 50)

        try:
            # 生成学习方案
            result_data = generate_learning_plan(
                technology, experience_level, duration_hours, preferences
            )

            result = {
                "status": "completed",
                "data": result_data
            }

            self._print_result(result_data)
            return result

        except Exception as e:
            error_result = {
                "status": "error",
                "error": f"生成失败: {str(e)}"
            }
            print(f"生成学习方案失败: {e}")
            return error_result

    def _print_result(self, data: Dict[str, Any]):
        """打印结果"""
        print("学习方案生成成功!")
        print("=" * 60)
        print(f"技术: {data['technology']}")
        print(f"经验水平: {data['experience_level']}")
        print(f"计划时长: {data['duration_hours']} 小时")
        print("=" * 60)

        # 研究摘要
        summary = data['research_summary']
        print(f"\n研究摘要:")
        print(f"  {summary['summary']}")
        for insight in summary['key_insights']:
            print(f"  - {insight}")

        # 学习资源
        print(f"\n学习资源:")
        for category, items in data['resources'].items():
            print(f"  {category}: {', '.join(items[:2])}")

        # 时间线
        print(f"\n学习时间线:")
        total_hours = data['timeline']['total_hours']
        print(f"  总时长: {total_hours} 小时")
        for phase in data['timeline']['phases']:
            print(f"  {phase['name']}: {phase['hours']} 小时 ({phase['weeks']} 周)")

        # 成功指标
        print(f"\n成功衡量指标:")
        for metric in data['success_metrics']:
            print(f"  {metric}")

        if data['personalization_applied']:
            print("\n已应用个性化定制")

        print("=" * 60)

    def save_result(self, result: Dict[str, Any], filename: str = None):
        """保存结果"""
        if filename is None:
            technology = result.get("data", {}).get("technology", "unknown")
            filename = f"demo_learning_plan_{technology}.json"

        try:
            with open(filename, 'w', encoding='utf-8') as f:
                json.dump(result, f, ensure_ascii=False, indent=2)
            print(f"结果已保存到: {filename}")
        except Exception as e:
            print(f"保存文件失败: {e}")


async def main():
    """演示主函数"""
    print("技术学习助手 - 演示模式")
    print("=" * 40)

    assistant = DemoTechLearningAssistant()

    # 演示用例
    test_cases = [
        {"tech": "Python", "level": "beginner", "hours": 20},
        {"tech": "React", "level": "intermediate", "hours": 30, "preferences": {"learning_style": "hands-on"}},
        {"tech": "Docker", "level": "advanced", "hours": 25}
    ]

    for i, case in enumerate(test_cases, 1):
        print(f"\n=== 演示 {i}: {case['tech']} ({case['level']}) ===")

        result = await assistant.create_learning_plan(
            technology=case["tech"],
            experience_level=case["level"],
            duration_hours=case["hours"],
            preferences=case.get("preferences")
        )

        if result["status"] == "completed":
            filename = f"demo_{case['tech'].lower()}_plan.json"
            assistant.save_result(result, filename)

    print("\n演示完成!")
    print("\n使用方法:")
    print("1. 设置真实的 OPENAI_API_KEY 到 .env 文件")
    print("2. 运行: python main.py '技术名称' --level beginner --hours 30")
    print("3. 或使用交互模式: python main.py --interactive")


if __name__ == "__main__":
    asyncio.run(main())