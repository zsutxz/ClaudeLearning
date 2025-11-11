"""
内容分析工具
"""
import re
from typing import List, Dict, Any, Tuple
from datetime import datetime
import pandas as pd
from langchain_text_splitters import RecursiveCharacterTextSplitter
from langchain_openai import OpenAIEmbeddings
# 注释掉FAISS导入，如果未安装的话
# from langchain.vectorstores import FAISS
from langchain_core.documents import Document

from config.settings import settings


class ContentAnalyzer:
    """内容分析器"""

    def __init__(self):
        self.text_splitter = RecursiveCharacterTextSplitter(
            chunk_size=1000,
            chunk_overlap=200,
            length_function=len
        )
        self.embeddings = OpenAIEmbeddings(
            openai_api_key=settings.OPENAI_API_KEY
        )

    def extract_key_concepts(self, content: str, num_concepts: int = 10) -> List[str]:
        """提取关键概念"""
        # 简单的关键词提取算法
        # 移除标点符号和停用词
        content_clean = re.sub(r'[^\w\s]', '', content.lower())

        # 常见停用词
        stop_words = {
            'the', 'a', 'an', 'and', 'or', 'but', 'in', 'on', 'at', 'to', 'for',
            'of', 'with', 'by', 'is', 'are', 'was', 'were', 'be', 'been', 'being',
            'have', 'has', 'had', 'do', 'does', 'did', 'will', 'would', 'could',
            'should', 'may', 'might', 'must', 'can', 'this', 'that', 'these', 'those',
            'i', 'you', 'he', 'she', 'it', 'we', 'they', 'me', 'him', 'her', 'us', 'them',
            'my', 'your', 'his', 'its', 'our', 'their', 'mine', 'yours', 'hers', 'ours', 'theirs'
        }

        words = [word for word in content_clean.split() if word not in stop_words and len(word) > 2]

        # 计算词频
        word_freq = {}
        for word in words:
            word_freq[word] = word_freq.get(word, 0) + 1

        # 按频率排序并返回前N个
        sorted_words = sorted(word_freq.items(), key=lambda x: x[1], reverse=True)
        return [word for word, freq in sorted_words[:num_concepts]]

    def analyze_trends(self, articles: List[Dict[str, Any]]) -> Dict[str, Any]:
        """分析技术趋势"""
        if not articles:
            return {"trends": [], "growth_topics": [], "declining_topics": []}

        # 汇总所有内容
        all_content = " ".join([
            article.get("title", "") + " " +
            article.get("summary", "") + " " +
            article.get("snippet", "")
            for article in articles
        ])

        # 提取关键词
        keywords = self.extract_key_concepts(all_content, num_concepts=20)

        # 分析发布时间趋势
        dates = []
        for article in articles:
            if article.get("published"):
                try:
                    # 尝试解析日期
                    date_str = article["published"]
                    if isinstance(date_str, str):
                        # 简单的日期解析
                        date_patterns = [
                            r'(\d{4}-\d{2}-\d{2})',
                            r'(\d{1,2}/\d{1,2}/\d{4})',
                            r'(\d{4}/\d{2}/\d{2})'
                        ]

                        for pattern in date_patterns:
                            match = re.search(pattern, date_str)
                            if match:
                                dates.append(match.group(1))
                                break
                except:
                    continue

        # 按月份统计
        monthly_counts = {}
        for date_str in dates:
            month = date_str[:7]  # YYYY-MM
            monthly_counts[month] = monthly_counts.get(month, 0) + 1

        return {
            "trends": keywords[:10],
            "growth_topics": keywords[:5],  # 简化处理
            "declining_topics": [],  # 需要历史数据对比
            "monthly_activity": monthly_counts,
            "total_articles": len(articles)
        }

    def categorize_content(self, articles: List[Dict[str, Any]]) -> Dict[str, List[Dict[str, Any]]]:
        """内容分类"""
        categories = {
            "tutorials": [],
            "documentation": [],
            "research_papers": [],
            "news": [],
            "case_studies": [],
            "tools": []
        }

        for article in articles:
            title = article.get("title", "").lower()
            summary = article.get("summary", "").lower()
            snippet = article.get("snippet", "").lower()
            combined_text = f"{title} {summary} {snippet}"

            # 关键词匹配分类
            if any(word in combined_text for word in ["tutorial", "how to", "guide", "step by step", "学习", "教程"]):
                categories["tutorials"].append(article)
            elif any(word in combined_text for word in ["documentation", "docs", "api", "reference", "文档"]):
                categories["documentation"].append(article)
            elif any(word in combined_text for word in ["research", "paper", "study", "analysis", "研究", "论文"]):
                categories["research_papers"].append(article)
            elif any(word in combined_text for word in ["news", "announcement", "release", "新闻", "发布"]):
                categories["news"].append(article)
            elif any(word in combined_text for word in ["case study", "example", "implementation", "案例", "实现"]):
                categories["case_studies"].append(article)
            elif any(word in combined_text for word in ["tool", "framework", "library", "utility", "工具"]):
                categories["tools"].append(article)
            else:
                # 默认归类为文档
                categories["documentation"].append(article)

        return categories

    def assess_difficulty(self, articles: List[Dict[str, Any]]) -> Dict[str, Any]:
        """评估内容难度"""
        if not articles:
            return {"overall_difficulty": "unknown", "distribution": {}}

        # 简单的难度评估基于关键词
        beginner_keywords = [
            "introduction", "beginner", "basic", "getting started",
            "入门", "基础", "简介", "hello world"
        ]

        intermediate_keywords = [
            "intermediate", "practical", "implementation", "best practices",
            "实践", "实现", "最佳实践"
        ]

        advanced_keywords = [
            "advanced", "expert", "deep dive", "architecture", "optimization",
            "高级", "专家", "深入", "架构", "优化"
        ]

        difficulty_counts = {"beginner": 0, "intermediate": 0, "advanced": 0}

        for article in articles:
            title = article.get("title", "").lower()
            summary = article.get("summary", "").lower()
            snippet = article.get("snippet", "").lower()
            combined_text = f"{title} {summary} {snippet}"

            # 计算每个难度级别的匹配分数
            beginner_score = sum(1 for keyword in beginner_keywords if keyword in combined_text)
            intermediate_score = sum(1 for keyword in intermediate_keywords if keyword in combined_text)
            advanced_score = sum(1 for keyword in advanced_keywords if keyword in combined_text)

            # 确定难度级别
            if beginner_score > intermediate_score and beginner_score > advanced_score:
                difficulty_counts["beginner"] += 1
            elif advanced_score > intermediate_score and advanced_score > beginner_score:
                difficulty_counts["advanced"] += 1
            else:
                difficulty_counts["intermediate"] += 1

        # 确定整体难度
        total = sum(difficulty_counts.values())
        if total == 0:
            overall_difficulty = "intermediate"
        elif difficulty_counts["beginner"] / total > 0.6:
            overall_difficulty = "beginner"
        elif difficulty_counts["advanced"] / total > 0.6:
            overall_difficulty = "advanced"
        else:
            overall_difficulty = "intermediate"

        return {
            "overall_difficulty": overall_difficulty,
            "distribution": difficulty_counts,
            "total_articles": total
        }

    def generate_summary(self, articles: List[Dict[str, Any]]) -> Dict[str, Any]:
        """生成内容摘要"""
        if not articles:
            return {"summary": "未找到相关内容", "key_insights": []}

        # 提取标题和摘要
        titles = [article.get("title", "") for article in articles if article.get("title")]
        summaries = [article.get("summary", "") or article.get("snippet", "")
                    for article in articles]

        # 生成关键洞察
        key_insights = []

        # 最新发布的内容
        recent_articles = sorted(
            [a for a in articles if a.get("published")],
            key=lambda x: x["published"],
            reverse=True
        )[:3]

        if recent_articles:
            key_insights.append(f"最新内容: {recent_articles[0].get('title', '未知标题')}")

        # 热门主题
        all_content = " ".join(titles + summaries)
        trending_topics = self.extract_key_concepts(all_content, num_concepts=5)
        if trending_topics:
            key_insights.append(f"热门主题: {', '.join(trending_topics[:3])}")

        # 内容类型分布
        categories = self.categorize_content(articles)
        category_summary = [f"{cat}: {len(arts)}篇"
                           for cat, arts in categories.items() if arts]
        if category_summary:
            key_insights.append(f"内容分布: {', '.join(category_summary)}")

        return {
            "summary": f"找到 {len(articles)} 篇相关内容，涵盖多个技术主题和学习资源。",
            "key_insights": key_insights,
            "total_sources": len(set(article.get("source", "unknown") for article in articles)),
            "categories": {cat: len(arts) for cat, arts in categories.items() if arts}
        }