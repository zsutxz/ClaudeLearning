#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
示例 08: 多智能体协作系统

展示多智能体协作的完整功能：
- 智能体注册和管理
- 任务分发和调度
- 协作工作流
- 智能体辩论
- 并行任务执行
"""

import sys
import asyncio
from pathlib import Path
from typing import Dict, List, Optional

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent.parent.parent
sys.path.insert(0, str(project_root))

from lib.multi_agent_system import (
    MultiAgentSystem,
    create_multi_agent_system,
    AgentStatus,
    MessageType
)
from lib.config import get_config
from lib.utils import print_example_header


# ==================== 辅助函数 ====================

def get_provider():
    """获取可用的模型提供商"""
    config = get_config()
    return "claude" if config.anthropic_api_key else "mock"


def create_team(
    system: MultiAgentSystem,
    team_members: Dict[str, Dict[str, str]],
    provider: str
) -> None:
    """
    创建团队

    Args:
        system: 多智能体系统实例
        team_members: 团队成员配置
        provider: 模型提供商
    """
    for agent_id, info in team_members.items():
        system.create_agent(
            agent_id,
            provider=provider,
            capabilities=[info["capability"]],
            system_prompt=info["prompt"]
        )


def print_result_preview(result, max_length: int = 150) -> None:
    """
    打印结果预览

    Args:
        result: 任务结果
        max_length: 最大显示长度
    """
    if result and result.success:
        preview = result.result[:max_length] + "..." if len(result.result) > max_length else result.result
        print(f"{preview}")


def print_team_status(system: MultiAgentSystem, team_members: Optional[Dict[str, Dict[str, str]]] = None) -> None:
    """
    打印团队状态

    Args:
        system: 多智能体系统实例
        team_members: 团队成员配置（可选）
    """
    status = system.get_system_status()
    for agent_id, agent_info in status["agents"].items():
        role = f" ({team_members[agent_id]['capability']})" if team_members and agent_id in team_members else ""
        print(f"  {agent_id}{role}: 完成任务 {agent_info['completed_tasks']} 个")


# ==================== 示例 1: 基础多智能体 ====================

async def example_basic_multi_agent():
    """示例 1: 基础多智能体系统"""
    print("\n📝 示例 1: 基础多智能体系统")
    print("-" * 50)

    provider = get_provider()
    system = MultiAgentSystem()

    # 定义团队成员
    team_members = {
        "coder": {
            "capability": "编程",
            "prompt": "你是一个专业的程序员，擅长编写高质量代码。"
        },
        "analyst": {
            "capability": "分析",
            "prompt": "你是一个数据分析师，擅长分析问题和数据。"
        },
        "writer": {
            "capability": "写作",
            "prompt": "你是一个技术文档撰写专家。"
        }
    }

    # 创建团队
    create_team(system, team_members, provider)

    # 显示系统状态
    print("\n📊 系统状态:")
    status = system.get_system_status()
    for agent_id in status["registered_agents"]:
        agent_info = status["agents"][agent_id]
        print(f"  {agent_id}:")
        print(f"    状态: {agent_info['status']}")
        print(f"    能力: {', '.join(agent_info['capabilities'])}")

    # 分发任务
    print("\n🚀 开始分发任务:")

    result1 = await system.coordinator.distribute_task(
        task_description="用 Python 实现一个快速排序函数",
        required_capability="编程"
    )

    if result1 and result1.success:
        print(f"\n✅ 代码生成结果:\n{result1.result[:300]}...")

    result2 = await system.coordinator.distribute_task(
        task_description="分析以下情况: 销售额增长 20%，但利润下降 5%",
        required_capability="分析"
    )

    if result2 and result2.success:
        print(f"\n✅ 分析结果:\n{result2.result[:300]}...")


# ==================== 示例 2: 协作工作流 ====================

async def example_collaborative_workflow():
    """示例 2: 协作工作流"""
    print("\n\n📝 示例 2: 协作工作流")
    print("-" * 50)

    provider = get_provider()
    system = MultiAgentSystem()

    # 定义专业化团队
    team_members = {
        "developer": {
            "capability": "开发",
            "prompt": "你是软件开发专家，专注于编写高质量代码。"
        },
        "reviewer": {
            "capability": "审查",
            "prompt": "你是代码审查专家，专注于代码质量、安全性和最佳实践。"
        },
        "qa_engineer": {
            "capability": "测试",
            "prompt": "你是QA工程师，专注于编写全面的测试用例。"
        }
    }

    create_team(system, team_members, provider)

    # 定义协作工作流
    workflow = [
        {
            "agent": "developer",
            "task": "实现一个计算器类，支持加减乘除运算",
            "capability": "开发"
        },
        {
            "agent": "reviewer",
            "task": "审查上述代码，指出问题和改进建议",
            "capability": "审查",
            "use_previous": True
        },
        {
            "agent": "qa_engineer",
            "task": "为上述代码编写全面的单元测试",
            "capability": "测试",
            "use_previous": True
        }
    ]

    print("\n🔄 执行协作工作流...")
    print("工作流: 开发 -> 审查 -> 测试\n")

    results = await system.collaborative_workflow(workflow)

    # 显示结果
    print("\n📋 工作流执行结果:")
    for step_key, result in results.items():
        if result:
            status_icon = "✅" if result.success else "❌"
            print(f"\n{status_icon} {step_key}:")
            print_result_preview(result)

    # 最终统计
    print(f"\n📊 任务完成统计:")
    print_team_status(system)


# ==================== 示例 3: 智能体辩论 ====================

async def example_agent_debate():
    """示例 3: 智能体辩论"""
    print("\n\n📝 示例 3: 智能体辩论")
    print("-" * 50)

    provider = get_provider()
    system = MultiAgentSystem()

    # 定义具有不同观点的智能体
    debaters = {
        "optimist": {
            "capability": "辩论",
            "prompt": "你是一个乐观主义者，总是看到事物的积极面和机会。"
        },
        "realist": {
            "capability": "辩论",
            "prompt": "你是一个现实主义者，注重事实和实际情况。"
        },
        "pessimist": {
            "capability": "辩论",
            "prompt": "你是一个谨慎主义者，关注风险和潜在问题。"
        }
    }

    create_team(system, debaters, provider)

    # 辩论主题
    topic = "人工智能对未来工作的影响"

    print(f"\n🎤 辩论主题: {topic}")
    print(f"👥 参与者: {', '.join(debaters.keys())}")
    print(f"🔄 辩论轮数: 2\n")

    debate_history = await system.debate(
        topic=topic,
        participants=list(debaters.keys()),
        rounds=2
    )

    # 显示辩论总结
    print("\n📝 辩论总结:")
    for agent_id, statements in debate_history.items():
        print(f"\n{agent_id} 的观点:")
        for i, statement in enumerate(statements, 1):
            preview = statement[:100] + "..." if len(statement) > 100 else statement
            print(f"  第{i}轮: {preview}")


# ==================== 示例 4: 并行任务执行 ====================

async def example_parallel_execution():
    """示例 4: 并行任务执行"""
    print("\n\n📝 示例 4: 并行任务执行")
    print("-" * 50)

    provider = get_provider()
    system = MultiAgentSystem()

    # 创建多个工作智能体
    workers = {
        f"worker_{i+1}": {
            "capability": "处理",
            "prompt": "你是一个高效的任务处理助手。"
        }
        for i in range(3)
    }

    create_team(system, workers, provider)

    # 定义并行任务
    parallel_tasks = [
        {
            "description": "解释什么是递归",
            "capability": "处理"
        },
        {
            "description": "解释什么是动态规划",
            "capability": "处理"
        },
        {
            "description": "解释什么是贪心算法",
            "capability": "处理"
        }
    ]

    print("\n⚡ 并行执行多个任务...")
    print(f"任务数量: {len(parallel_tasks)}")
    print(f"可用智能体: {len(system.coordinator.agents)}\n")

    import time
    start_time = time.time()

    results = await system.coordinator.parallel_execute(parallel_tasks)

    duration = time.time() - start_time

    # 显示结果
    print(f"\n✅ 并行执行完成 (总耗时: {duration:.2f}s)")
    for i, result in enumerate(results, 1):
        if result and result.success:
            print(f"\n任务 {i} (由 {result.agent_id} 执行):")
            print_result_preview(result, max_length=100)


# ==================== 示例 5: 软件开发团队模拟 ====================

async def example_dev_team_simulation():
    """示例 5: 软件开发团队模拟"""
    print("\n\n📝 示例 5: 软件开发团队模拟")
    print("-" * 50)

    provider = get_provider()
    system = MultiAgentSystem()

    # 定义完整的开发团队
    team_members = {
        "product_manager": {
            "capability": "产品经理",
            "prompt": "你是产品经理，负责需求分析和项目规划。"
        },
        "architect": {
            "capability": "架构师",
            "prompt": "你是技术架构师，负责系统设计和技术选型。"
        },
        "developer": {
            "capability": "开发工程师",
            "prompt": "你是全栈开发工程师，负责代码实现。"
        },
        "tester": {
            "capability": "测试工程师",
            "prompt": "你是QA工程师，负责质量保证和测试。"
        }
    }

    create_team(system, team_members, provider)

    print("\n👥 开发团队成员:")
    for agent_id, info in team_members.items():
        print(f"  {agent_id}: {info['capability']}")

    # 模拟开发流程
    project = "开发一个待办事项管理应用"

    workflow = [
        {
            "agent": "product_manager",
            "task": f"为项目编写需求文档: {project}",
            "capability": "产品经理"
        },
        {
            "agent": "architect",
            "task": "基于需求设计系统架构",
            "capability": "架构师",
            "use_previous": True
        },
        {
            "agent": "developer",
            "task": "根据架构实现核心功能",
            "capability": "开发工程师",
            "use_previous": True
        },
        {
            "agent": "tester",
            "task": "制定测试计划并编写测试用例",
            "capability": "测试工程师",
            "use_previous": True
        }
    ]

    print(f"\n🚀 项目: {project}")
    print("📋 开发流程: 需求 -> 架构 -> 开发 -> 测试\n")

    await system.collaborative_workflow(workflow)

    # 项目总结
    print("\n📊 项目完成总结:")
    final_status = system.get_system_status()
    print(f"  总完成任务数: {final_status['total_completed_tasks']}")
    print_team_status(system, team_members)


# ==================== 主函数 ====================

async def main():
    """运行所有多智能体系统示例"""
    print_example_header(
        "多智能体协作系统示例",
        "展示多智能体协作的完整功能"
    )

    try:
        # 运行示例
        await example_basic_multi_agent()
        await example_collaborative_workflow()
        await example_agent_debate()
        await example_parallel_execution()
        await example_dev_team_simulation()

        print("\n" + "=" * 50)
        print("✅ 所有多智能体系统示例完成!")
        print("=" * 50)

    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    asyncio.run(main())
