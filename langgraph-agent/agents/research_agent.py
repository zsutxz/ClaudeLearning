"""
研究智能体
"""
from typing import Dict, Any, List
from langchain_core.tools import tool
from langchain_openai import ChatOpenAI
from langchain_core.messages import HumanMessage, AIMessage

from tools.web_searcher import WebSearcher
from tools.content_analyzer import ContentAnalyzer
from config.settings import settings


class ResearchAgent:
    """技术研究智能体"""

    def __init__(self):
        self.llm = ChatOpenAI(**settings.get_llm_config())
        self.web_searcher = WebSearcher()
        self.content_analyzer = ContentAnalyzer()

    async def search_technology_info(self, technology: str) -> Dict[str, Any]:
        """
        搜索指定技术的最新信息

        Args:
            technology: 技术名称

        Returns:
            包含搜索结果的字典
        """
        search_queries = [
            f"{technology} tutorial guide 2024",
            f"{technology} best practices examples",
            f"{technology} documentation API",
            f"{technology} latest updates news"
        ]

        all_results = []
        async with self.web_searcher:
            for query in search_queries:
                results = await self.web_searcher.comprehensive_search(query)
                all_results.extend(results.get("google_results", []))
                all_results.extend(results.get("arxiv_results", []))
                all_results.extend(results.get("blog_results", []))

        # 去重并限制结果数量
        seen_links = set()
        unique_results = []
        for result in all_results:
            if result.get("link") not in seen_links:
                seen_links.add(result.get("link"))
                unique_results.append(result)
                if len(unique_results) >= 20:  # 限制总结果数
                    break

        return {
            "technology": technology,
            "total_results": len(unique_results),
            "results": unique_results
        }

    def analyze_content(self, articles: List[Dict[str, Any]]) -> Dict[str, Any]:
        """
        分析收集到的内容

        Args:
            articles: 文章列表

        Returns:
            分析结果
        """
        # 趋势分析
        trends = self.content_analyzer.analyze_trends(articles)

        # 内容分类
        categories = self.content_analyzer.categorize_content(articles)

        # 难度评估
        difficulty = self.content_analyzer.assess_difficulty(articles)

        # 生成摘要
        summary = self.content_analyzer.generate_summary(articles)

        return {
            "trends": trends,
            "categories": categories,
            "difficulty": difficulty,
            "summary": summary
        }

    def generate_technology_report(self, technology: str, analysis: Dict[str, Any]) -> str:
        """
        生成技术研究报告

        Args:
            technology: 技术名称
            analysis: 分析结果

        Returns:
            技术研究报告
        """
        prompt = f"""
        请基于以下分析结果，为技术 "{technology}" 生成一份详细的研究报告。

        分析数据：
        - 趋势分析: {analysis.get('trends', {})}
        - 内容分类: {analysis.get('categories', {})}
        - 难度评估: {analysis.get('difficulty', {})}
        - 内容摘要: {analysis.get('summary', {})}

        请按以下结构生成报告：
        1. 技术概述
        2. 最新趋势
        3. 学习资源分类
        4. 难度评估
        5. 关键洞察
        6. 推荐下一步行动

        请用中文回答，保持专业性和实用性。
        """

        messages = [HumanMessage(content=prompt)]
        response = self.llm.invoke(messages)

        return response.content

    async def research_technology(self, technology: str, fast_mode: bool = False) -> Dict[str, Any]:
        """
        完整的技术研究流程

        Args:
            technology: 技术名称
            fast_mode: 是否使用快速模式（跳过网络搜索）

        Returns:
            完整的研究结果
        """
        print(f"开始研究技术: {technology}")

        if fast_mode:
            print("使用快速模式（跳过网络搜索）...")
            return self._get_mock_research_results(technology)

        # 1. 搜索信息
        print("搜索相关资料...")
        search_results = await self.search_technology_info(technology)
        articles = search_results["results"]

        if not articles:
            return {
                "technology": technology,
                "status": "no_results",
                "message": f"未找到关于 {technology} 的相关资料"
            }

        print(f"找到 {len(articles)} 篇相关资料")

        # 2. 分析内容
        print("分析资料内容...")
        analysis = self.analyze_content(articles)

        # 3. 生成报告
        print("生成研究报告...")
        report = self.generate_technology_report(technology, analysis)

        return {
            "technology": technology,
            "status": "completed",
            "search_results": search_results,
            "analysis": analysis,
            "report": report,
            "timestamp": self._get_timestamp()
        }

    def _get_mock_research_results(self, technology: str) -> Dict[str, Any]:
        """获取模拟研究结果用于快速模式"""
        # 基于技术名称生成基本的分析结果
        mock_analysis = {
            "summary": f"{technology}是一种重要的技术，适合学习者掌握",
            "trends": {
                "trends": [
                    f"{technology}在业界应用广泛",
                    f"{technology}生态系统持续发展",
                    f"{technology}学习资源丰富"
                ]
            },
            "categories": {
                "基础概念": 30,
                "实践应用": 40,
                "高级特性": 20,
                "最佳实践": 10
            },
            "difficulty": {"overall_difficulty": "beginner"}
        }

        mock_report = f"""
# {technology}技术研究报告

## 1. 技术概述
{technology}是当前重要的技术之一，具有广泛的应用前景。

## 2. 最新趋势
- {technology}在业界应用日益广泛
- 学习资源和社区支持持续增长
- 技术生态不断成熟

## 3. 学习资源分类
- 官方文档和教程
- 在线课程和视频
- 实战项目和案例

## 4. 难度评估
{technology}适合初学者入门，同时也支持高级应用开发。

## 5. 推荐学习路径
建议从基础概念开始，逐步深入到实际应用。
"""

        return {
            "technology": technology,
            "status": "completed",
            "search_results": {"results": []},  # 空的搜索结果
            "analysis": mock_analysis,
            "report": mock_report,
            "timestamp": self._get_timestamp()
        }

    def _get_timestamp(self) -> str:
        """获取当前时间戳"""
        from datetime import datetime
        return datetime.now().isoformat()