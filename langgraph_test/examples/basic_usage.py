"""
基础使用示例
"""
import asyncio
from main import TechLearningAssistant


async def example_basic_usage():
    """基础使用示例"""
    assistant = TechLearningAssistant()

    # 示例1: 学习Python基础
    print("=== 示例1: Python基础学习 ===")
    result1 = await assistant.create_learning_plan(
        technology="Python",
        experience_level="beginner",
        duration_hours=30
    )

    # 示例2: 学习React进阶
    print("\n=== 示例2: React进阶学习 ===")
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

    return result1, result2


async def example_advanced_usage():
    """高级使用示例"""
    assistant = TechLearningAssistant()

    # 示例3: 机器学习高级学习
    print("=== 示例3: 机器学习高级学习 ===")
    result3 = await assistant.create_learning_plan(
        technology="Machine Learning",
        experience_level="advanced",
        duration_hours=60,
        preferences={
            "learning_style": "theory_plus_practice",
            "preferred_time": "flexible",
            "focus": ["deep_learning", "nlp", "computer_vision"],
            "tools": ["tensorflow", "pytorch", "scikit-learn"],
            "project_type": "research",
            "background": "software_engineering"
        }
    )

    # 保存结果
    if result3["status"] == "completed":
        assistant.save_result(result3, "ml_learning_plan.json")

    return result3


async def example_batch_processing():
    """批量处理示例"""
    assistant = TechLearningAssistant()

    # 要学习的技术列表
    technologies = [
        {"name": "Docker", "level": "beginner", "hours": 20},
        {"name": "Kubernetes", "level": "intermediate", "hours": 40},
        {"name": "Vue.js", "level": "beginner", "hours": 25},
        {"name": "TensorFlow", "level": "advanced", "hours": 50}
    ]

    print("=== 批量生成学习方案 ===")
    results = []

    for tech in technologies:
        print(f"\n正在处理: {tech['name']} ({tech['level']})")
        result = await assistant.create_learning_plan(
            technology=tech["name"],
            experience_level=tech["level"],
            duration_hours=tech["hours"]
        )
        results.append(result)

        # 保存每个结果
        if result["status"] == "completed":
            filename = f"learning_plan_{tech['name'].lower()}.json"
            assistant.save_result(result, filename)

    # 统计结果
    successful = sum(1 for r in results if r["status"] == "completed")
    print(f"\n批量处理完成: {successful}/{len(results)} 成功")

    return results


async def example_customization():
    """个性化定制示例"""
    assistant = TechLearningAssistant()

    # 不同类型的学习者偏好
    learner_profiles = [
        {
            "name": "视觉学习者",
            "preferences": {
                "learning_style": "visual",
                "preferred_content": ["video_tutorials", "infographics", "diagrams"],
                "study_method": "watch_and_practice",
                "time_preference": "morning"
            }
        },
        {
            "name": "实践派",
            "preferences": {
                "learning_style": "hands-on",
                "preferred_content": ["coding_exercises", "projects", "workshops"],
                "study_method": "learn_by_doing",
                "time_preference": "flexible",
                "focus": "practical_application"
            }
        },
        {
            "name": "理论派",
            "preferences": {
                "learning_style": "theoretical",
                "preferred_content": ["documentation", "books", "research_papers"],
                "study_method": "deep_understanding",
                "time_preference": "evening",
                "focus": ["concepts", "principles", "architecture"]
            }
        }
    ]

    technology = "TypeScript"
    print(f"=== 个性化学习方案示例: {technology} ===")

    for profile in learner_profiles:
        print(f"\n--- {profile['name']} ---")
        result = await assistant.create_learning_plan(
            technology=technology,
            experience_level="intermediate",
            duration_hours=30,
            preferences=profile["preferences"]
        )

        # 保存个性化方案
        if result["status"] == "completed":
            filename = f"typescript_{profile['name'].lower()}_plan.json"
            assistant.save_result(result, filename)


async def main():
    """运行所有示例"""
    print("🚀 技术学习助手 - 使用示例")
    print("=" * 50)

    try:
        # 基础使用示例
        await example_basic_usage()

        print("\n" + "=" * 50)
        # 高级使用示例
        await example_advanced_usage()

        print("\n" + "=" * 50)
        # 批量处理示例
        await example_batch_processing()

        print("\n" + "=" * 50)
        # 个性化定制示例
        await example_customization()

        print("\n✅ 所有示例运行完成!")

    except Exception as e:
        print(f"❌ 示例运行失败: {e}")


if __name__ == "__main__":
    asyncio.run(main())