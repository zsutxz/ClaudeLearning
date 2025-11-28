#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
调试主程序问题
"""
import asyncio
from agents.research_agent import ResearchAgent


async def debug_research():
    """调试研究功能"""
    print("=== 调试研究功能 ===")

    try:
        research_agent = ResearchAgent()
        print("[OK] ResearchAgent 初始化成功")

        # 测试搜索功能
        print("\n开始技术研究...")
        result = await research_agent.research_technology("Python")

        print(f"研究状态: {result.get('status')}")
        print(f"研究消息: {result.get('message', 'N/A')}")

        if result.get("status") == "completed":
            print(f"找到文章数量: {len(result.get('search_results', {}).get('results', []))}")
            print("[OK] 研究功能正常")
        else:
            print("[ERROR] 研究功能异常")

    except Exception as e:
        print(f"[ERROR] 研究功能出错: {str(e)}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    asyncio.run(debug_research())