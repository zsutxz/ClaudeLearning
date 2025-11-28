"""
学习方案生成智能体
"""
from typing import Dict, Any, List
from langchain_openai import ChatOpenAI
from langchain_core.messages import HumanMessage, AIMessage
import json

from config.settings import settings


class LearningAgent:
    """学习方案生成智能体"""

    def __init__(self):
        self.llm = ChatOpenAI(**settings.get_llm_config())

    def _generate_learning_path_template(self) -> str:
        """学习路径模板"""
        return """
        学习路径规划模板：

        ## 第一阶段：基础入门 (预计 {beginner_hours} 小时)
        - 核心概念理解
        - 环境搭建
        - 基础语法/操作
        - 简单实践项目

        ## 第二阶段：进阶提升 (预计 {intermediate_hours} 小时)
        - 深入理解原理
        - 最佳实践学习
        - 中等复杂度项目
        - 性能优化基础

        ## 第三阶段：高级应用 (预计 {advanced_hours} 小时)
        - 高级特性掌握
        - 架构设计
        - 大型项目实践
        - 源码分析

        ## 第四阶段：专家精进 (预计 {expert_hours} 小时)
        - 深度定制
        - 贡献开源社区
        - 技术分享
        - 持续学习
        """

    def generate_learning_plan(self, technology: str, analysis: Dict[str, Any],
                             duration_hours: int = None, experience_level: str = "beginner") -> Dict[str, Any]:
        """
        生成个性化学习方案

        Args:
            technology: 技术名称
            analysis: 技术分析结果
            duration_hours: 学习时长(小时)
            experience_level: 经验水平 (beginner/intermediate/advanced)

        Returns:
            学习方案
        """
        if duration_hours is None:
            duration_hours = settings.DEFAULT_COURSE_DURATION

        # 根据难度分析和经验水平调整学习重点
        difficulty = analysis.get("difficulty", {}).get("overall_difficulty", "intermediate")
        categories = analysis.get("categories", {})
        trends = analysis.get("trends", {}).get("trends", [])

        # 构建学习方案生成提示
        prompt = f"""
        请为技术 "{technology}" 生成一个详细的学习方案。

        用户信息：
        - 经验水平: {experience_level}
        - 计划学习时长: {duration_hours} 小时
        - 技术难度: {difficulty}

        技术分析数据：
        - 内容分类: {categories}
        - 趋势主题: {trends[:5] if trends else []}

        请按以下要求生成学习方案：

        1. **学习目标** (3-5个明确目标)
        2. **阶段规划** (根据总时长合理分配4个阶段)
        3. **每个阶段包含**:
           - 学习重点
           - 具体内容
           - 实践项目
           - 预计时长
           - 学习资源推荐
        4. **学习建议** (学习方法、工具、注意事项)
        5. **里程碑检查点** (如何检验学习效果)
        6. **扩展资源** (进阶学习方向)

        请确保方案：
- 具有可操作性
- 理论与实践结合
- 循序渐进
- 包含实际项目
- 考虑到用户的经验水平

        请用中文回答，使用结构化格式，提供具体可行的建议。
        """

        messages = [HumanMessage(content=prompt)]
        response = self.llm.invoke(messages)

        # 生成学习资源推荐
        resources = self._generate_resource_recommendations(technology, categories, analysis)

        return {
            "technology": technology,
            "experience_level": experience_level,
            "duration_hours": duration_hours,
            "learning_plan": response.content,
            "resources": resources,
            "estimated_timeline": self._generate_timeline(duration_hours, experience_level),
            "success_metrics": self._generate_success_metrics(technology, experience_level)
        }

    def _generate_resource_recommendations(self, technology: str, categories: Dict[str, List],
                                         analysis: Dict[str, Any]) -> Dict[str, List[str]]:
        """生成学习资源推荐"""
        resources = {
            "official_docs": [],
            "tutorials": [],
            "books": [],
            "courses": [],
            "tools": [],
            "communities": []
        }

        # 根据技术名称推荐通用资源
        resources["official_docs"] = [
            f"{technology} 官方文档",
            f"{technology} API 参考手册"
        ]

        # 根据内容分类推荐资源
        if categories.get("tutorials"):
            resources["tutorials"].extend([
                f"{technology} 入门教程",
                f"{technology} 实战指南"
            ])

        if categories.get("documentation"):
            resources["official_docs"].extend([
                f"{technology} 深度文档",
                f"{technology} 开发指南"
            ])

        # 通用资源推荐
        resources["books"] = [
            f"{technology} 权威指南",
            f"{technology} 最佳实践"
        ]

        resources["courses"] = [
            f"{technology} 在线课程",
            f"{technology} 视频教程"
        ]

        resources["tools"] = [
            f"{technology} 开发工具",
            f"{technology} 调试工具"
        ]

        resources["communities"] = [
            f"{technology} 开发者社区",
            f"{technology} 技术论坛"
        ]

        return resources

    def _generate_timeline(self, total_hours: int, experience_level: str) -> Dict[str, Any]:
        """生成学习时间线"""
        if experience_level == "beginner":
            beginner_ratio = 0.4
            intermediate_ratio = 0.3
            advanced_ratio = 0.2
            expert_ratio = 0.1
        elif experience_level == "intermediate":
            beginner_ratio = 0.2
            intermediate_ratio = 0.4
            advanced_ratio = 0.3
            expert_ratio = 0.1
        else:  # advanced
            beginner_ratio = 0.1
            intermediate_ratio = 0.2
            advanced_ratio = 0.5
            expert_ratio = 0.2

        return {
            "total_hours": total_hours,
            "beginner_phase": {
                "hours": int(total_hours * beginner_ratio),
                "weeks": max(1, int(total_hours * beginner_ratio / 10))
            },
            "intermediate_phase": {
                "hours": int(total_hours * intermediate_ratio),
                "weeks": max(1, int(total_hours * intermediate_ratio / 10))
            },
            "advanced_phase": {
                "hours": int(total_hours * advanced_ratio),
                "weeks": max(1, int(total_hours * advanced_ratio / 10))
            },
            "expert_phase": {
                "hours": int(total_hours * expert_ratio),
                "weeks": max(1, int(total_hours * expert_ratio / 10))
            }
        }

    def _generate_success_metrics(self, technology: str, experience_level: str) -> List[str]:
        """生成成功衡量指标"""
        base_metrics = [
            "完成所有阶段的学习目标",
            "独立完成至少2个实践项目",
            "能够解决常见技术问题"
        ]

        if experience_level == "beginner":
            base_metrics.extend([
                f"掌握{technology}基础概念和语法",
                "能够搭建开发环境",
                "完成入门级项目"
            ])
        elif experience_level == "intermediate":
            base_metrics.extend([
                f"理解{technology}核心原理",
                "能够进行中等复杂度开发",
                "掌握性能优化基础"
            ])
        else:  # advanced
            base_metrics.extend([
                f"精通{technology}高级特性",
                "能够进行架构设计",
                "具备指导和培训他人的能力"
            ])

        return base_metrics

    def customize_plan(self, base_plan: Dict[str, Any], preferences: Dict[str, Any]) -> Dict[str, Any]:
        """
        根据用户偏好定制学习方案

        Args:
            base_plan: 基础学习方案
            preferences: 用户偏好

        Returns:
            定制后的学习方案
        """
        prompt = f"""
        请根据用户偏好，对以下学习方案进行个性化调整：

        原学习方案：
        {base_plan.get('learning_plan', '')}

        用户偏好：
        {json.dumps(preferences, ensure_ascii=False, indent=2)}

        请保持方案的完整性和可行性，重点关注：
        1. 根据学习风格调整内容呈现方式
        2. 根据时间安排调整学习节奏
        3. 根据具体目标调整学习重点
        4. 添加个性化建议和资源

        请用中文回答，保持结构化格式。
        """

        messages = [HumanMessage(content=prompt)]
        response = self.llm.invoke(messages)

        # 创建定制化的方案
        customized_plan = base_plan.copy()
        customized_plan["learning_plan"] = response.content
        customized_plan["personalization"] = {
            "applied_preferences": preferences,
            "customization_notes": "已根据用户偏好调整学习方案"
        }

        return customized_plan