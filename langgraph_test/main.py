"""
主程序入口
"""
import asyncio
import json
import sys
from typing import Dict, Any, Optional

from src.tech_learning_workflow import TechLearningWorkflow
from config.settings import settings


class TechLearningAssistant:
    """技术学习助手"""

    def __init__(self):
        self.workflow = TechLearningWorkflow()

    async def create_learning_plan(self, technology: str,
                                 experience_level: str = "beginner",
                                 duration_hours: int = None,
                                 preferences: Dict[str, Any] = None) -> Dict[str, Any]:
        """
        创建学习方案

        Args:
            technology: 技术名称
            experience_level: 经验水平 (beginner/intermediate/advanced)
            duration_hours: 学习时长(小时)
            preferences: 用户偏好

        Returns:
            学习方案结果
        """
        print(f"开始为技术 '{technology}' 生成学习方案...")
        print(f"经验水平: {experience_level}")
        print(f"学习时长: {duration_hours or settings.DEFAULT_COURSE_DURATION} 小时")

        if preferences:
            print(f"个性化偏好: {preferences}")

        print("-" * 50)

        try:
            result = await self.workflow.run(
                technology=technology,
                experience_level=experience_level,
                duration_hours=duration_hours,
                preferences=preferences
            )

            if result["status"] == "completed":
                # 确保data是字典格式
                data = result.get("data", {})
                if isinstance(data, str):
                    # 如果data是字符串，尝试解析
                    try:
                        import ast
                        data = ast.literal_eval(data)
                    except:
                        data = {"result": data}
                elif not isinstance(data, dict):
                    data = {"result": data}

                self._print_success_result(data)
                return result
            else:
                self._print_error_result(result)
                return result

        except Exception as e:
            error_result = {
                "status": "error",
                "error": f"执行失败: {str(e)}"
            }
            self._print_error_result(error_result)
            return error_result

    def _print_success_result(self, data: Dict[str, Any]):
        """打印成功结果"""
        print("学习方案生成成功!")
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
            if isinstance(research_summary, dict):
                print(f"   {research_summary.get('summary', '无摘要')}")
                key_insights = research_summary.get('key_insights', [])
                if key_insights:
                    print("   关键洞察:")
                    for insight in key_insights:
                        print(f"   - {insight}")
            else:
                # 如果是字符串，直接显示
                print(f"   {research_summary}")

        # 学习方案
        learning_plan = data.get('learning_plan', '')
        if learning_plan:
            print(f"\n学习方案:\n{learning_plan}")

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
            phases = ['beginner_phase', 'intermediate_phase', 'advanced_phase', 'expert_phase']
            phase_names = ['入门阶段', '进阶阶段', '高级阶段', '专家阶段']
            for phase, name in zip(phases, phase_names):
                phase_data = timeline.get(phase, {})
                if phase_data:
                    print(f"   {name}: {phase_data.get('hours', 0)} 小时 ({phase_data.get('weeks', 0)} 周)")

        # 成功指标
        success_metrics = data.get('success_metrics', [])
        if success_metrics:
            print("\n成功衡量指标:")
            for metric in success_metrics:
                print(f"   {metric}")

        if data.get('personalization_applied'):
            print("\n已应用个性化定制")

        print("=" * 60)

    def _print_error_result(self, result: Dict[str, Any]):
        """打印错误结果"""
        print("生成学习方案失败!")
        print(f"错误信息: {result.get('error', '未知错误')}")
        print("\n可能的解决方案:")
        print("1. 检查技术名称是否正确")
        print("2. 确认网络连接正常")
        print("3. 检查API配置是否正确")
        print("4. 尝试使用更通用的技术名称")

    def save_result(self, result: Dict[str, Any], filename: str = None):
        """
        保存结果到文件

        Args:
            result: 结果数据
            filename: 文件名(可选)
        """
        if filename is None:
            technology = result.get("data", {}).get("technology", "unknown")
            timestamp = result.get("data", {}).get("timestamp", "").replace(":", "-")
            filename = f"learning_plan_{technology}_{timestamp}.json"

        try:
            with open(filename, 'w', encoding='utf-8') as f:
                json.dump(result, f, ensure_ascii=False, indent=2)
            print(f"结果已保存到: {filename}")
        except Exception as e:
            print(f"保存文件失败: {e}")


async def interactive_mode():
    """交互模式"""
    assistant = TechLearningAssistant()

    print("技术学习助手 - 交互模式")
    print("=" * 40)

    while True:
        try:
            print("\n请输入要学习的技术名称 (输入 'quit' 退出):")
            technology = input("> ").strip()

            if technology.lower() in ['quit', 'exit', 'q']:
                print("再见!")
                break

            if not technology:
                print("请输入有效的技术名称")
                continue

            print("请选择您的经验水平:")
            print("1. beginner (初学者)")
            print("2. intermediate (中级)")
            print("3. advanced (高级)")

            level_choice = input("> ").strip()
            experience_levels = {"1": "beginner", "2": "intermediate", "3": "advanced"}
            experience_level = experience_levels.get(level_choice, "beginner")

            print("请输入计划学习时长(小时，直接回车使用默认值):")
            duration_input = input("> ").strip()
            duration_hours = int(duration_input) if duration_input.isdigit() else None

            print("是否有特殊学习偏好? (可选，直接回车跳过)")
            print("例如: {\"learning_style\": \"visual\", \"preferred_time\": \"evening\"}")
            preferences_input = input("> ").strip()
            preferences = json.loads(preferences_input) if preferences_input else {}

            print("\n开始生成学习方案...")
            result = await assistant.create_learning_plan(
                technology=technology,
                experience_level=experience_level,
                duration_hours=duration_hours,
                preferences=preferences
            )

            if result["status"] == "completed":
                save_choice = input("\n是否保存结果到文件? (y/n): ").strip().lower()
                if save_choice in ['y', 'yes']:
                    assistant.save_result(result)

        except KeyboardInterrupt:
            print("\n\n用户中断，退出程序")
            break
        except Exception as e:
            print(f"发生错误: {e}")


async def cli_mode():
    """命令行模式"""
    import argparse

    parser = argparse.ArgumentParser(description="技术学习助手")
    parser.add_argument("technology", help="要学习的技术名称")
    parser.add_argument("--level", choices=["beginner", "intermediate", "advanced"],
                       default="beginner", help="经验水平")
    parser.add_argument("--hours", type=int, help="学习时长(小时)")
    parser.add_argument("--preferences", help="JSON格式的学习偏好")
    parser.add_argument("--output", help="输出文件名")
    parser.add_argument("--interactive", action="store_true", help="交互模式")

    args = parser.parse_args()

    if args.interactive:
        await interactive_mode()
        return

    assistant = TechLearningAssistant()

    preferences = json.loads(args.preferences) if args.preferences else {}

    result = await assistant.create_learning_plan(
        technology=args.technology,
        experience_level=args.level,
        duration_hours=args.hours,
        preferences=preferences
    )

    if args.output and result["status"] == "completed":
        assistant.save_result(result, args.output)


def main():
    """主函数"""
    if not settings.validate_config():
        print("配置验证失败，请检查 .env 文件")
        print("注意: 需要在 .env 文件中设置有效的 OPENAI_API_KEY")
        sys.exit(1)

    try:
        asyncio.run(cli_mode())
    except KeyboardInterrupt:
        print("\n用户中断，退出程序")
    except Exception as e:
        print(f"程序执行失败: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()