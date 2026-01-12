"""
多智能体协作系统

实现智能体之间的通信、任务分发和协作机制。

核心组件:
- AgentMessage: 智能体间消息格式
- AgentCommunicationBus: 通信总线
- AgentCoordinator: 智能体协调器
- MultiAgentSystem: 多智能体系统高层接口
"""

import asyncio
import uuid
from datetime import datetime
from dataclasses import dataclass, field
from enum import Enum
from typing import Any, Callable, Dict, List, Optional

from lib.multi_agent import UniversalAIAgent
from lib.config import get_config


# ==================== 数据结构 ====================

class MessageType(Enum):
    """消息类型"""
    TASK_REQUEST = "task_request"      # 任务请求
    TASK_RESPONSE = "task_response"    # 任务响应
    AGENT_MESSAGE = "agent_message"    # 智能体消息
    BROADCAST = "broadcast"            # 广播消息
    STATUS_UPDATE = "status_update"    # 状态更新


class AgentStatus(Enum):
    """智能体状态"""
    IDLE = "idle"          # 空闲
    BUSY = "busy"          # 忙碌
    ERROR = "error"        # 错误
    OFFLINE = "offline"    # 离线


@dataclass
class AgentMessage:
    """智能体间消息"""
    id: str = field(default_factory=lambda: str(uuid.uuid4()))
    type: MessageType = MessageType.AGENT_MESSAGE
    sender: str = ""
    receiver: str = ""  # 空字符串表示广播
    content: Any = None
    timestamp: datetime = field(default_factory=datetime.now)
    metadata: Dict[str, Any] = field(default_factory=dict)

    def __repr__(self):
        return f"AgentMessage({self.sender} -> {self.receiver or 'ALL'}: {self.type.value})"


@dataclass
class AgentInfo:
    """智能体信息"""
    id: str
    agent: UniversalAIAgent
    status: AgentStatus = AgentStatus.IDLE
    capabilities: List[str] = field(default_factory=list)
    current_task: Optional[str] = None
    message_count: int = 0
    completed_tasks: int = 0


@dataclass
class TaskResult:
    """任务执行结果"""
    success: bool
    agent_id: str
    result: Any = None
    error: Optional[str] = None
    duration: float = 0.0


# ==================== 通信总线 ====================

class AgentCommunicationBus:
    """智能体通信总线 - 处理智能体间的消息传递"""

    def __init__(self):
        self._subscribers: Dict[str, List[Callable]] = {}
        self._message_history: List[AgentMessage] = []

    def subscribe(self, agent_id: str, callback: Callable[[AgentMessage], None]):
        """订阅消息"""
        if agent_id not in self._subscribers:
            self._subscribers[agent_id] = []
        self._subscribers[agent_id].append(callback)

    def unsubscribe(self, agent_id: str):
        """取消订阅"""
        if agent_id in self._subscribers:
            del self._subscribers[agent_id]

    async def publish(self, message: AgentMessage):
        """发布消息"""
        self._message_history.append(message)

        if message.receiver:
            # 发送给特定接收者
            if message.receiver in self._subscribers:
                for callback in self._subscribers[message.receiver]:
                    await self._safe_callback(callback, message)
        else:
            # 广播给所有订阅者
            for agent_id, callbacks in self._subscribers.items():
                if agent_id != message.sender:  # 不发送给自己
                    for callback in callbacks:
                        await self._safe_callback(callback, message)

    async def _safe_callback(self, callback: Callable[[AgentMessage], None], message: AgentMessage):
        """安全执行回调"""
        try:
            if asyncio.iscoroutinefunction(callback):
                await callback(message)
            else:
                callback(message)
        except Exception as e:
            print(f"❌ 消息回调执行失败: {e}")

    def get_message_history(self, limit: int = 100) -> List[AgentMessage]:
        """获取消息历史"""
        return self._message_history[-limit:]

    def clear_history(self):
        """清空消息历史"""
        self._message_history.clear()


# ==================== 协调器 ====================

class AgentCoordinator:
    """智能体协调器 - 管理多智能体协作"""

    def __init__(self):
        self.agents: Dict[str, AgentInfo] = {}
        self.bus = AgentCommunicationBus()

    def register_agent(
        self,
        agent_id: str,
        agent: UniversalAIAgent,
        capabilities: Optional[List[str]] = None
    ) -> AgentInfo:
        """注册智能体"""
        info = AgentInfo(
            id=agent_id,
            agent=agent,
            capabilities=capabilities or []
        )
        self.agents[agent_id] = info

        # 订阅消息
        self.bus.subscribe(agent_id, self._handle_message)

        return info

    def unregister_agent(self, agent_id: str):
        """注销智能体"""
        if agent_id in self.agents:
            self.bus.unsubscribe(agent_id)
            del self.agents[agent_id]

    def _handle_message(self, message: AgentMessage):
        """处理接收到的消息"""
        # 更新消息计数
        if message.receiver in self.agents:
            self.agents[message.receiver].message_count += 1

    async def send_message(
        self,
        sender_id: str,
        receiver_id: str,
        content: Any,
        message_type: MessageType = MessageType.AGENT_MESSAGE
    ):
        """发送消息到指定智能体"""
        message = AgentMessage(
            sender=sender_id,
            receiver=receiver_id,
            content=content,
            type=message_type
        )
        await self.bus.publish(message)

    async def broadcast(self, sender_id: str, content: Any):
        """广播消息给所有智能体"""
        message = AgentMessage(
            sender=sender_id,
            receiver="",  # 空字符串表示广播
            content=content,
            type=MessageType.BROADCAST
        )
        await self.bus.publish(message)

    def get_idle_agent(self, capability: Optional[str] = None) -> Optional[str]:
        """获取空闲智能体"""
        # 优先找已完成任务多的
        idle_agents = [
            (agent_id, info.completed_tasks)
            for agent_id, info in self.agents.items()
            if info.status == AgentStatus.IDLE
            and (capability is None or capability in info.capabilities)
        ]

        if not idle_agents:
            return None

        # 按完成任务数排序，选择经验最丰富的
        idle_agents.sort(key=lambda x: x[1], reverse=True)
        return idle_agents[0][0]

    def get_agent_status(self) -> Dict[str, Dict[str, Any]]:
        """获取所有智能体状态"""
        return {
            agent_id: {
                "status": info.status.value,
                "capabilities": info.capabilities,
                "current_task": info.current_task,
                "message_count": info.message_count,
                "completed_tasks": info.completed_tasks
            }
            for agent_id, info in self.agents.items()
        }

    async def distribute_task(
        self,
        task_description: str,
        required_capability: Optional[str] = None,
        input_data: Optional[str] = None
    ) -> Optional[TaskResult]:
        """分发任务到合适的智能体"""
        import time

        agent_id = self.get_idle_agent(required_capability)

        if agent_id is None:
            print(f"⚠️ 没有可用的智能体 (需要能力: {required_capability or '通用'})")
            return None

        # 更新智能体状态
        self.agents[agent_id].status = AgentStatus.BUSY
        self.agents[agent_id].current_task = task_description

        print(f"📋 任务分配给 {agent_id}: {task_description[:50]}...")

        # 执行任务并计时
        start_time = time.time()
        result = await self._execute_task(agent_id, task_description, input_data)
        duration = time.time() - start_time

        # 恢复空闲状态
        self.agents[agent_id].status = AgentStatus.IDLE
        self.agents[agent_id].current_task = None

        if result.success:
            self.agents[agent_id].completed_tasks += 1
            print(f"✅ {agent_id} 完成 (耗时: {duration:.2f}s)")
        else:
            self.agents[agent_id].status = AgentStatus.ERROR
            print(f"❌ {agent_id} 失败: {result.error}")

        return result

    async def _execute_task(
        self,
        agent_id: str,
        task_description: str,
        input_data: Optional[str]
    ) -> TaskResult:
        """执行任务"""
        agent_info = self.agents[agent_id]
        agent = agent_info.agent

        try:
            if input_data:
                prompt = f"{task_description}\n\n输入数据:\n{input_data}"
            else:
                prompt = task_description

            response = agent.chat(prompt)
            return TaskResult(
                success=True,
                agent_id=agent_id,
                result=response
            )

        except Exception as e:
            return TaskResult(
                success=False,
                agent_id=agent_id,
                error=str(e)
            )

    async def parallel_execute(
        self,
        tasks: List[Dict[str, Any]]
    ) -> List[TaskResult]:
        """并行执行多个任务"""
        async def execute_single(task):
            return await self.distribute_task(
                task_description=task["description"],
                required_capability=task.get("capability"),
                input_data=task.get("input_data")
            )

        results = await asyncio.gather(
            *[execute_single(task) for task in tasks],
            return_exceptions=True
        )

        # 过滤异常结果
        return [r for r in results if isinstance(r, TaskResult)]


# ==================== 多智能体系统 ====================

class MultiAgentSystem:
    """多智能体系统 - 高层接口"""

    def __init__(self):
        self.coordinator = AgentCoordinator()

    def create_agent(
        self,
        agent_id: str,
        provider: str = "claude",
        model: Optional[str] = None,
        capabilities: Optional[List[str]] = None,
        system_prompt: Optional[str] = None,
        **kwargs
    ) -> AgentInfo:
        """创建并注册智能体"""
        config = get_config()

        agent = UniversalAIAgent(
            provider=provider,
            model=model or config.anthropic_model,
            api_key=kwargs.get('api_key') or config.anthropic_api_key,
            base_url=kwargs.get('base_url') or config.anthropic_base_url
        )

        # 添加系统提示词
        if system_prompt:
            agent.add_system_prompt(system_prompt)

        info = self.coordinator.register_agent(agent_id, agent, capabilities)
        print(f"✅ 智能体已创建: {agent_id} (能力: {', '.join(capabilities or ['通用'])})")
        return info

    async def collaborative_workflow(
        self,
        workflow: List[Dict[str, Any]]
    ) -> Dict[str, TaskResult]:
        """
        协作工作流执行

        Args:
            workflow: 工作流定义
                [
                    {"agent": "coder", "task": "编写代码", "capability": "编程"},
                    {"agent": "reviewer", "task": "审查代码", "use_previous": true},
                    {"agent": "tester", "task": "编写测试", "use_previous": true}
                ]

        Returns:
            每个步骤的执行结果
        """
        results = {}
        previous_result = None

        for i, step in enumerate(workflow):
            agent_id = step.get("agent")
            task = step["task"]
            capability = step.get("capability")
            use_previous = step.get("use_previous", False)

            # 构建输入数据
            input_data = None
            if use_previous and previous_result:
                input_data = previous_result.result if previous_result.success else None

            # 执行任务
            result = await self.coordinator.distribute_task(
                task_description=f"[{agent_id}] {task}",
                required_capability=capability,
                input_data=input_data
            )

            step_key = f"step_{i+1}_{agent_id}"
            results[step_key] = result
            previous_result = result

        return results

    async def debate(
        self,
        topic: str,
        participants: List[str],
        rounds: int = 2
    ) -> Dict[str, List[str]]:
        """
        智能体辩论

        Args:
            topic: 辩论主题
            participants: 参与的智能体ID列表
            rounds: 辩论轮数

        Returns:
            每个智能体的发言记录
        """
        debate_history = {agent_id: [] for agent_id in participants}

        for round_num in range(1, rounds + 1):
            print(f"\n🔥 第 {round_num} 轮辩论")

            for agent_id in participants:
                # 获取其他智能体的观点
                others_views = []
                for other_id in participants:
                    if other_id != agent_id and debate_history[other_id]:
                        others_views.append(f"{other_id}: {debate_history[other_id][-1]}")

                # 构建提示词
                prompt = f"辩论主题: {topic}\n\n"
                if others_views:
                    prompt += f"其他观点:\n" + "\n".join(others_views) + "\n\n"
                prompt += f"请给出你的观点 (第{round_num}轮):"

                # 执行辩论
                result = await self.coordinator.distribute_task(
                    task_description=prompt,
                    input_data=None
                )

                if result and result.success:
                    debate_history[agent_id].append(result.result)
                    print(f"  🗣️ {agent_id}: {result.result[:100]}...")

        return debate_history

    def get_system_status(self) -> Dict[str, Any]:
        """获取系统状态"""
        agent_status = self.coordinator.get_agent_status()
        message_history = self.coordinator.bus.get_message_history()

        return {
            "agents": agent_status,
            "message_count": len(message_history),
            "registered_agents": list(self.coordinator.agents.keys()),
            "total_completed_tasks": sum(
                info.completed_tasks for info in self.coordinator.agents.values()
            )
        }


# ==================== 便捷函数 ====================

def create_multi_agent_system() -> MultiAgentSystem:
    """创建多智能体系统"""
    return MultiAgentSystem()


async def run_simple_collaboration():
    """运行简单的协作示例"""
    print("=== 多智能体协作示例 ===\n")

    system = MultiAgentSystem()

    # 创建专业化智能体
    system.create_agent(
        "developer",
        capabilities=["编程", "开发"],
        system_prompt="你是一个专业的程序员，擅长编写高质量代码。"
    )

    system.create_agent(
        "reviewer",
        capabilities=["审查", "质量保证"],
        system_prompt="你是一个代码审查专家，专注于代码质量、安全性和最佳实践。"
    )

    system.create_agent(
        "tester",
        capabilities=["测试", "验证"],
        system_prompt="你是一个测试工程师，擅长编写全面的测试用例。"
    )

    # 执行协作工作流
    workflow = [
        {
            "agent": "developer",
            "task": "用 Python 实现一个二分查找函数",
            "capability": "编程"
        },
        {
            "agent": "reviewer",
            "task": "审查上述代码的质量和安全性",
            "capability": "审查",
            "use_previous": True
        },
        {
            "agent": "tester",
            "task": "为上述代码编写单元测试",
            "capability": "测试",
            "use_previous": True
        }
    ]

    print("\n🚀 开始协作工作流...\n")
    results = await system.collaborative_workflow(workflow)

    # 显示结果
    print("\n📊 工作流结果:")
    for step, result in results.items():
        if result:
            print(f"\n{step}:")
            if result.success:
                print(f"{result.result[:200]}..." if len(result.result) > 200 else result.result)

    # 显示系统状态
    print("\n📊 系统状态:")
    status = system.get_system_status()
    for agent_id, agent_status in status["agents"].items():
        print(f"  {agent_id}: {agent_status['status']}, 完成任务: {agent_status['completed_tasks']}")


if __name__ == "__main__":
    asyncio.run(run_simple_collaboration())
