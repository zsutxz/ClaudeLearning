"""
基于LangGraph的技术学习工作流
"""
from typing import Dict, Any, List, Optional
from langgraph.graph import StateGraph, END
from typing_extensions import Annotated, TypedDict

# 添加消息类型处理
try:
    from langgraph.graph.message import add_messages
except ImportError:
    # 如果导入失败，定义一个简单的消息合并函数
    def add_messages(left, right):
        if not left:
            return right if isinstance(right, list) else [right]
        if not right:
            return left if isinstance(left, list) else [left]

        left_list = left if isinstance(left, list) else [left]
        right_list = right if isinstance(right, list) else [right]
        return left_list + right_list

from agents.research_agent import ResearchAgent
from agents.learning_agent import LearningAgent
from config.settings import settings


class WorkflowState(TypedDict):
    """工作流状态"""
    messages: Annotated[list, add_messages]
    technology: str
    experience_level: str
    duration_hours: int
    preferences: Dict[str, Any]
    research_results: Optional[Dict[str, Any]]
    learning_plan: Optional[Dict[str, Any]]
    error: Optional[str]
    status: str


class TechLearningWorkflow:
    """技术学习工作流"""

    def __init__(self):
        self.research_agent = ResearchAgent()
        self.learning_agent = LearningAgent()
        self.workflow = self._create_workflow()

    def _create_workflow(self) -> StateGraph:
        """创建工作流图"""
        workflow = StateGraph(WorkflowState)

        # 添加节点
        workflow.add_node("validate_input", self._validate_input)
        workflow.add_node("research_technology", self._research_technology)
        workflow.add_node("generate_learning_plan", self._generate_learning_plan)
        workflow.add_node("customize_plan", self._customize_plan)
        workflow.add_node("generate_final_output", self._generate_final_output)
        workflow.add_node("handle_error", self._handle_error)

        # 设置入口点
        workflow.set_entry_point("validate_input")

        # 添加边
        workflow.add_edge("validate_input", "research_technology")
        workflow.add_edge("research_technology", "generate_learning_plan")
        workflow.add_conditional_edges(
            "generate_learning_plan",
            self._should_customize,
            {
                "customize": "customize_plan",
                "finalize": "generate_final_output"
            }
        )
        workflow.add_edge("customize_plan", "generate_final_output")
        workflow.add_edge("generate_final_output", END)
        workflow.add_edge("handle_error", END)

        return workflow.compile()

    async def _validate_input(self, state: WorkflowState) -> WorkflowState:
        """验证输入参数"""
        try:
            technology = state.get("technology", "").strip()
            if not technology:
                return {
                    **state,
                    "error": "请提供要学习的技术名称",
                    "status": "error"
                }

            experience_level = state.get("experience_level", "beginner")
            if experience_level not in ["beginner", "intermediate", "advanced"]:
                experience_level = "beginner"

            duration_hours = state.get("duration_hours", settings.DEFAULT_COURSE_DURATION)
            if not isinstance(duration_hours, int) or duration_hours < 1:
                duration_hours = settings.DEFAULT_COURSE_DURATION

            preferences = state.get("preferences", {})
            if not isinstance(preferences, dict):
                preferences = {}

            return {
                **state,
                "technology": technology,
                "experience_level": experience_level,
                "duration_hours": duration_hours,
                "preferences": preferences,
                "status": "validated"
            }

        except Exception as e:
            return {
                **state,
                "error": f"输入验证失败: {str(e)}",
                "status": "error"
            }

    async def _research_technology(self, state: WorkflowState) -> WorkflowState:
        """技术研究阶段"""
        try:
            technology = state["technology"]
            # 默认使用快速模式避免网络问题
            research_results = await self.research_agent.research_technology(technology, fast_mode=True)

            if research_results["status"] == "no_results":
                return {
                    **state,
                    "error": research_results["message"],
                    "status": "error"
                }

            return {
                **state,
                "research_results": research_results,
                "status": "researched"
            }

        except Exception as e:
            return {
                **state,
                "error": f"技术研究失败: {str(e)}",
                "status": "error"
            }

    async def _generate_learning_plan(self, state: WorkflowState) -> WorkflowState:
        """生成基础学习方案"""
        try:
            research_results = state.get("research_results")
            if not research_results:
                return {
                    **state,
                    "error": "缺少研究结果数据",
                    "status": "error"
                }

            analysis = research_results.get("analysis", {})
            if not analysis:
                # 创建默认分析数据
                analysis = {
                    "summary": "技术研究摘要",
                    "trends": {"trends": []},
                    "categories": {},
                    "difficulty": {"overall_difficulty": "intermediate"}
                }

            technology = state.get("technology", "Python")
            experience_level = state.get("experience_level", "beginner")
            duration_hours = state.get("duration_hours", 20)

            learning_plan = self.learning_agent.generate_learning_plan(
                technology=technology,
                analysis=analysis,
                duration_hours=duration_hours,
                experience_level=experience_level
            )

            return {
                **state,
                "learning_plan": learning_plan,
                "status": "planned"
            }

        except Exception as e:
            return {
                **state,
                "error": f"学习方案生成失败: {str(e)}",
                "status": "error"
            }

    async def _customize_plan(self, state: WorkflowState) -> WorkflowState:
        """定制学习方案"""
        try:
            base_plan = state["learning_plan"]
            preferences = state["preferences"]

            if preferences:
                customized_plan = self.learning_agent.customize_plan(base_plan, preferences)
                return {
                    **state,
                    "learning_plan": customized_plan,
                    "status": "customized"
                }

            return {
                **state,
                "status": "ready"
            }

        except Exception as e:
            return {
                **state,
                "error": f"方案定制失败: {str(e)}",
                "status": "error"
            }

    async def _generate_final_output(self, state: WorkflowState) -> WorkflowState:
        """生成最终输出"""
        try:
            research_results = state.get("research_results")
            learning_plan = state.get("learning_plan")

            # 安全检查，确保数据存在
            if not research_results:
                return {
                    **state,
                    "error": "缺少研究结果数据",
                    "status": "error"
                }

            if not learning_plan:
                return {
                    **state,
                    "error": "缺少学习方案数据",
                    "status": "error"
                }

            # 安全获取分析数据
            analysis = research_results.get("analysis", {})
            if not analysis:
                analysis = {
                    "summary": "技术研究摘要",
                    "trends": {"trends": []},
                    "categories": {},
                    "difficulty": {"overall_difficulty": "intermediate"}
                }

            # 安全获取学习方案数据
            if isinstance(learning_plan, dict):
                plan_content = learning_plan.get("learning_plan", "学习方案")
                resources = learning_plan.get("resources", {})
                timeline = learning_plan.get("estimated_timeline", {})
                success_metrics = learning_plan.get("success_metrics", [])
            else:
                plan_content = str(learning_plan) if learning_plan else "学习方案"
                resources = {}
                timeline = {}
                success_metrics = []

            final_output = {
                "technology": state.get("technology", "未知技术"),
                "experience_level": state.get("experience_level", "beginner"),
                "duration_hours": state.get("duration_hours", 20),
                "research_summary": analysis.get("summary", "技术研究摘要"),
                "research_report": research_results.get("report", "技术报告"),
                "learning_plan": plan_content,
                "resources": resources,
                "timeline": timeline,
                "success_metrics": success_metrics,
                "personalization_applied": state.get("status") == "customized",
                "timestamp": research_results.get("timestamp", "")
            }

            return {
                **state,
                "messages": [str(final_output)],
                "status": "completed"
            }

        except Exception as e:
            return {
                **state,
                "error": f"输出生成失败: {str(e)}",
                "status": "error"
            }

    async def _handle_error(self, state: WorkflowState) -> WorkflowState:
        """处理错误"""
        error_message = state.get("error", "未知错误")
        return {
            **state,
            "messages": [f"处理过程中发生错误: {error_message}"],
            "status": "failed"
        }

    def _should_customize(self, state: WorkflowState) -> str:
        """决定是否需要定制方案"""
        preferences = state.get("preferences", {})
        if preferences and len(preferences) > 0:
            return "customize"
        return "finalize"

    async def run(self, technology: str, experience_level: str = "beginner",
                  duration_hours: int = None, preferences: Dict[str, Any] = None) -> Dict[str, Any]:
        """
        运行技术学习工作流

        Args:
            technology: 技术名称
            experience_level: 经验水平
            duration_hours: 学习时长(小时)
            preferences: 用户偏好

        Returns:
            完整的学习方案
        """
        if duration_hours is None:
            duration_hours = settings.DEFAULT_COURSE_DURATION
        if preferences is None:
            preferences = {}

        initial_state = {
            "messages": [],
            "technology": technology,
            "experience_level": experience_level,
            "duration_hours": duration_hours,
            "preferences": preferences,
            "research_results": None,
            "learning_plan": None,
            "error": None,
            "status": "initialized"
        }

        try:
            # 运行工作流
            result = await self.workflow.ainvoke(initial_state)

            if result["status"] == "completed":
                # 解析最终输出 - 处理AIMessage对象
                if result["messages"]:
                    final_message = result["messages"][-1]
                    # 检查是否是AIMessage对象
                    if hasattr(final_message, 'content'):
                        # AIMessage对象
                        message_content = final_message.content
                    elif isinstance(final_message, tuple) and len(final_message) >= 2:
                        # 元组格式 (role, content)
                        message_content = final_message[1]
                    elif isinstance(final_message, dict):
                        # 字典格式
                        message_content = final_message.get('content', str(final_message))
                    else:
                        # 直接使用字符串
                        message_content = str(final_message)
                else:
                    return {"status": "error", "error": "没有找到输出消息"}

                if isinstance(message_content, str):
                    import json
                    import ast
                    try:
                        # 尝试解析JSON
                        if message_content.strip().startswith('{'):
                            # 先尝试用ast.literal_eval解析（支持单引号）
                            try:
                                parsed_data = ast.literal_eval(message_content)
                            except:
                                # 如果AST解析失败，尝试修复引号后用JSON解析
                                fixed_content = message_content.replace("'", '"')
                                parsed_data = json.loads(fixed_content)

                            # 确保数据是字典格式
                            if isinstance(parsed_data, dict):
                                return {"status": "completed", "data": parsed_data}
                            else:
                                return {"status": "completed", "data": {"result": parsed_data}}
                        else:
                            # 如果不是JSON格式，直接返回字符串
                            return {"status": "completed", "data": {"result": message_content}}
                    except (json.JSONDecodeError, ValueError, SyntaxError):
                        # 如果不是JSON格式，直接返回字符串
                        return {"status": "completed", "data": {"result": message_content}}
                else:
                    return {"status": "completed", "data": {"result": message_content}}
            else:
                return {
                    "status": result["status"],
                    "error": result.get("error", "未知错误")
                }

        except Exception as e:
            return {
                "status": "error",
                "error": f"工作流执行失败: {str(e)}"
            }