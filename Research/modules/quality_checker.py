"""
质量检查模块 - QualityChecker

提供研究数据的质量评估和验证功能，支持:
- 多维度质量评估
- 数据完整性检查
- 可信度评分
- 改进建议生成
"""

import os
import asyncio
import logging
from typing import Dict, List, Optional, Any, Tuple
from dataclasses import dataclass
from datetime import datetime, timedelta
import re
import json
from collections import defaultdict

# 配置日志
logger = logging.getLogger(__name__)

@dataclass
class QualityConfig:
    """质量检查配置"""
    min_sources_required: int = 3
    min_confidence_threshold: float = 0.6
    enable_source_validation: bool = True
    enable_content_validation: bool = True
    enable_consistency_check: bool = True
    enable_bias_detection: bool = True

@dataclass
class QualityScore:
    """质量分数类"""
    overall_score: float
    dimension_scores: Dict[str, float]
    issues: List[str]
    recommendations: List[str]
    confidence: float

class QualityChecker:
    """
    质量检查器

    评估研究数据的可靠性、完整性和准确性
    """

    def __init__(self, research_agent):
        """
        初始化质量检查器

        Args:
            research_agent: ResearchAgent实例
        """
        self.research_agent = research_agent
        self.config = QualityConfig()

        # 质量维度权重
        self.dimension_weights = {
            'completeness': 0.25,    # 完整性
            'reliability': 0.25,     # 可靠性
            'relevance': 0.20,       # 相关性
            'freshness': 0.15,       # 时效性
            'consistency': 0.15      # 一致性
        }

        # 质量检查历史
        self.check_history = []

        logger.info("QualityChecker 初始化完成")

    async def check(self, research_data: Dict[str, Any]) -> QualityScore:
        """
        执行全面的质量检查

        Args:
            research_data: 研究数据

        Returns:
            QualityScore: 质量评估结果
        """
        try:
            logger.info("开始执行质量检查")

            # 1. 基础数据验证
            basic_validation = self._basic_data_validation(research_data)

            # 2. 多维度质量评估
            dimension_scores = self._assess_dimensions(research_data)

            # 3. 问题识别
            issues = self._identify_issues(research_data, dimension_scores)

            # 4. 生成改进建议
            recommendations = self._generate_recommendations(issues, dimension_scores)

            # 5. 计算总体质量分数
            overall_score = self._calculate_overall_score(dimension_scores)

            # 6. 计算置信度
            confidence = self._calculate_confidence(research_data, overall_score)

            # 构建质量分数对象
            quality_score = QualityScore(
                overall_score=overall_score,
                dimension_scores=dimension_scores,
                issues=issues,
                recommendations=recommendations,
                confidence=confidence
            )

            # 记录检查历史
            self.check_history.append({
                'timestamp': datetime.now(),
                'overall_score': overall_score,
                'issues_count': len(issues),
                'confidence': confidence
            })

            logger.info(f"质量检查完成 - 总分: {overall_score:.2f}, 置信度: {confidence:.2f}")
            return quality_score

        except Exception as e:
            logger.error(f"质量检查失败: {str(e)}")
            return QualityScore(
                overall_score=0.0,
                dimension_scores={},
                issues=[f"质量检查失败: {str(e)}"],
                recommendations=["请检查数据格式和完整性"],
                confidence=0.0
            )

    def _basic_data_validation(self, research_data: Dict[str, Any]) -> bool:
        """
        基础数据验证

        Args:
            research_data: 研究数据

        Returns:
            bool: 验证是否通过
        """
        required_fields = ['query', 'literature', 'data', 'analysis']

        for field in required_fields:
            if field not in research_data:
                logger.warning(f"缺少必需字段: {field}")
                return False

        # 检查查询内容
        query = research_data.get('query', '').strip()
        if len(query) < 3:
            logger.warning("查询内容过短")
            return False

        return True

    def _assess_dimensions(self, research_data: Dict[str, Any]) -> Dict[str, float]:
        """
        多维度质量评估

        Args:
            research_data: 研究数据

        Returns:
            Dict[str, float]: 各维度的质量分数
        """
        scores = {}

        # 1. 完整性评估
        scores['completeness'] = self._assess_completeness(research_data)

        # 2. 可靠性评估
        scores['reliability'] = self._assess_reliability(research_data)

        # 3. 相关性评估
        scores['relevance'] = self._assess_relevance(research_data)

        # 4. 时效性评估
        scores['freshness'] = self._assess_freshness(research_data)

        # 5. 一致性评估
        scores['consistency'] = self._assess_consistency(research_data)

        return scores

    def _assess_completeness(self, research_data: Dict[str, Any]) -> float:
        """
        评估数据完整性

        Args:
            research_data: 研究数据

        Returns:
            float: 完整性分数 (0-10)
        """
        score = 5.0  # 基础分数

        # 检查文献数据完整性
        literature = research_data.get('literature', {})
        if isinstance(literature, dict):
            # 检查是否有多个数据源
            source_count = 0
            if literature.get('github_results'):
                source_count += 1
            if literature.get('paper_results'):
                source_count += 1
            if literature.get('blog_results'):
                source_count += 1

            if source_count >= 3:
                score += 2.0
            elif source_count >= 2:
                score += 1.0

            # 检查数据量
            total_results = (
                len(literature.get('github_results', [])) +
                len(literature.get('paper_results', [])) +
                len(literature.get('blog_results', []))
            )

            if total_results >= 20:
                score += 2.0
            elif total_results >= 10:
                score += 1.0
            elif total_results >= 5:
                score += 0.5

        # 检查分析数据完整性
        analysis = research_data.get('analysis', {})
        if isinstance(analysis, dict):
            if analysis.get('analysis_report'):
                score += 0.5
            if analysis.get('key_findings'):
                score += 0.5

        # 检查元数据
        metadata = research_data.get('metadata', {})
        if isinstance(metadata, dict):
            required_metadata = ['research_domain', 'provider', 'model']
            metadata_completeness = sum(1 for field in required_metadata if field in metadata)
            score += metadata_completeness * 0.2

        return min(max(score, 0.0), 10.0)

    def _assess_reliability(self, research_data: Dict[str, Any]) -> float:
        """
        评估数据可靠性

        Args:
            research_data: 研究数据

        Returns:
            float: 可靠性分数 (0-10)
        """
        score = 5.0  # 基础分数

        literature = research_data.get('literature', {})
        if isinstance(literature, dict):
            # GitHub项目可靠性
            github_results = literature.get('github_results', [])
            if github_results:
                # 评估项目质量
                high_quality_projects = 0
                for repo in github_results:
                    if hasattr(repo, 'metadata'):
                        meta = repo.metadata
                        stars = meta.get('stars', 0)
                        forks = meta.get('forks', 0)

                        # 根据星标和分支数评估质量
                        if stars > 1000:
                            high_quality_projects += 1
                        elif stars > 100:
                            high_quality_projects += 0.5

                if len(github_results) > 0:
                    github_quality_ratio = high_quality_projects / len(github_results)
                    score += github_quality_ratio * 2.0

            # 学术论文可靠性
            paper_results = literature.get('paper_results', [])
            if paper_results:
                # 学术论文通常具有较高的可靠性
                score += min(len(paper_results) * 0.5, 2.0)

        # AI模型可靠性
        provider = research_data.get('metadata', {}).get('provider', '').lower()
        if provider in ['claude', 'openai', 'anthropic']:
            score += 1.0
        elif provider:
            score += 0.5

        return min(max(score, 0.0), 10.0)

    def _assess_relevance(self, research_data: Dict[str, Any]) -> float:
        """
        评估数据相关性

        Args:
            research_data: 研究数据

        Returns:
            float: 相关性分数 (0-10)
        """
        score = 5.0  # 基础分数

        query = research_data.get('query', '').lower()
        if not query:
            return 0.0

        literature = research_data.get('literature', {})
        if isinstance(literature, dict):
            relevance_scores = []

            # 检查GitHub项目相关性
            for repo in literature.get('github_results', []):
                if hasattr(repo, 'title') and hasattr(repo, 'description'):
                    title_text = f"{repo.title} {repo.description}".lower()
                    relevance = self._calculate_text_relevance(query, title_text)
                    relevance_scores.append(relevance)

            # 检查论文相关性
            for paper in literature.get('paper_results', []):
                if hasattr(paper, 'title') and hasattr(paper, 'description'):
                    title_text = f"{paper.title} {paper.description}".lower()
                    relevance = self._calculate_text_relevance(query, title_text)
                    relevance_scores.append(relevance)

            # 检查博客相关性
            for blog in literature.get('blog_results', []):
                if hasattr(blog, 'title') and hasattr(blog, 'description'):
                    title_text = f"{blog.title} {blog.description}".lower()
                    relevance = self._calculate_text_relevance(query, title_text)
                    relevance_scores.append(relevance)

            if relevance_scores:
                avg_relevance = sum(relevance_scores) / len(relevance_scores)
                score += avg_relevance * 3.0

        # 分析内容相关性
        analysis = research_data.get('analysis', {})
        if isinstance(analysis, dict):
            analysis_text = analysis.get('analysis_report', '').lower()
            if analysis_text:
                analysis_relevance = self._calculate_text_relevance(query, analysis_text)
                score += analysis_relevance * 2.0

        return min(max(score, 0.0), 10.0)

    def _assess_freshness(self, research_data: Dict[str, Any]) -> float:
        """
        评估数据时效性

        Args:
            research_data: 研究数据

        Returns:
            float: 时效性分数 (0-10)
        """
        score = 5.0  # 基础分数

        literature = research_data.get('literature', {})
        if isinstance(literature, dict):
            timestamps = []

            # 收集所有时间戳
            for repo in literature.get('github_results', []):
                if hasattr(repo, 'timestamp'):
                    timestamps.append(repo.timestamp)

            for paper in literature.get('paper_results', []):
                if hasattr(paper, 'timestamp'):
                    timestamps.append(paper.timestamp)

            for blog in literature.get('blog_results', []):
                if hasattr(blog, 'timestamp'):
                    timestamps.append(blog.timestamp)

            if timestamps:
                # 计算平均时效性
                now = datetime.now()
                freshness_scores = []

                for timestamp in timestamps:
                    if isinstance(timestamp, datetime):
                        days_old = (now - timestamp).days
                        if days_old <= 7:
                            freshness_scores.append(1.0)
                        elif days_old <= 30:
                            freshness_scores.append(0.8)
                        elif days_old <= 90:
                            freshness_scores.append(0.6)
                        elif days_old <= 180:
                            freshness_scores.append(0.4)
                        else:
                            freshness_scores.append(0.2)

                if freshness_scores:
                    avg_freshness = sum(freshness_scores) / len(freshness_scores)
                    score += avg_freshness * 3.0

        return min(max(score, 0.0), 10.0)

    def _assess_consistency(self, research_data: Dict[str, Any]) -> float:
        """
        评估数据一致性

        Args:
            research_data: 研究数据

        Returns:
            float: 一致性分数 (0-10)
        """
        score = 7.0  # 基础分数，AI生成的一致性通常较好

        # 检查不同来源的一致性
        literature = research_data.get('literature', {})
        if isinstance(literature, dict):
            sources = []

            if literature.get('github_results'):
                sources.append('GitHub')
            if literature.get('paper_results'):
                sources.append('Academic')
            if literature.get('blog_results'):
                sources.append('Blogs')

            # 多个来源通常表示更好的一致性验证
            if len(sources) >= 3:
                score += 1.0
            elif len(sources) >= 2:
                score += 0.5

        # 检查分析一致性
        analysis = research_data.get('analysis', {})
        if isinstance(analysis, dict):
            # 检查是否有明显矛盾的分析结果
            if analysis.get('analysis_report'):
                score += 0.5

        return min(max(score, 0.0), 10.0)

    def _calculate_text_relevance(self, query: str, text: str) -> float:
        """
        计算文本相关性

        Args:
            query: 查询文本
            text: 目标文本

        Returns:
            float: 相关性分数 (0-1)
        """
        query_words = set(query.split())
        text_words = set(text.split())

        if not query_words:
            return 0.0

        # 计算词汇重叠度
        intersection = query_words.intersection(text_words)
        overlap_ratio = len(intersection) / len(query_words)

        # 考虑文本长度影响
        length_factor = min(len(text) / 1000, 1.0)  # 避免过短文本的影响

        return min(overlap_ratio * (1 + length_factor), 1.0)

    def _identify_issues(self, research_data: Dict[str, Any], dimension_scores: Dict[str, float]) -> List[str]:
        """
        识别质量问题

        Args:
            research_data: 研究数据
            dimension_scores: 各维度分数

        Returns:
            List[str]: 问题列表
        """
        issues = []

        # 基于维度分数识别问题
        for dimension, score in dimension_scores.items():
            if score < 5.0:
                if dimension == 'completeness':
                    issues.append("数据不完整，缺少关键信息或数据源")
                elif dimension == 'reliability':
                    issues.append("数据来源可靠性较低，建议增加权威来源")
                elif dimension == 'relevance':
                    issues.append("数据与研究主题相关性不够强")
                elif dimension == 'freshness':
                    issues.append("数据时效性较差，部分信息可能过时")
                elif dimension == 'consistency':
                    issues.append("不同来源数据存在不一致性")

        # 检查具体问题
        literature = research_data.get('literature', {})
        if isinstance(literature, dict):
            total_sources = (
                len(literature.get('github_results', [])) +
                len(literature.get('paper_results', [])) +
                len(literature.get('blog_results', []))
            )

            if total_sources < self.config.min_sources_required:
                issues.append(f"数据源数量不足，至少需要{self.config.min_sources_required}个来源")

        # 检查分析深度
        analysis = research_data.get('analysis', {})
        if isinstance(analysis, dict):
            if not analysis.get('analysis_report'):
                issues.append("缺少深入的分析报告")
            if not analysis.get('key_findings'):
                issues.append("缺少关键发现总结")

        return issues

    def _generate_recommendations(self, issues: List[str], dimension_scores: Dict[str, float]) -> List[str]:
        """
        生成改进建议

        Args:
            issues: 问题列表
            dimension_scores: 各维度分数

        Returns:
            List[str]: 建议列表
        """
        recommendations = []

        # 基于问题生成建议
        if "数据不完整" in str(issues):
            recommendations.extend([
                "扩展数据收集范围，增加更多权威来源",
                "补充关键指标和量化数据",
                "完善元数据信息"
            ])

        if "可靠性较低" in str(issues):
            recommendations.extend([
                "优先选择同行评议的学术论文",
                "增加知名企业和机构的报告",
                "验证开源项目的影响力和活跃度"
            ])

        if "相关性不够强" in str(issues):
            recommendations.extend([
                "优化搜索关键词和查询策略",
                "使用更专业的检索数据库",
                "增加领域专家的推荐和筛选"
            ])

        if "时效性较差" in str(issues):
            recommendations.extend([
                "优先收集最近6个月的数据",
                "设置时间过滤器，排除过时信息",
                "关注最新发布的研究报告和技术趋势"
            ])

        # 基于维度分数生成通用建议
        low_scores = [dim for dim, score in dimension_scores.items() if score < 6.0]
        if low_scores:
            recommendations.append(f"重点改进以下方面: {', '.join(low_scores)}")

        # 高分建议（保持优势）
        high_scores = [dim for dim, score in dimension_scores.items() if score >= 8.0]
        if high_scores:
            recommendations.append(f"继续保持以下方面的优势: {', '.join(high_scores)}")

        return list(set(recommendations))  # 去重

    def _calculate_overall_score(self, dimension_scores: Dict[str, float]) -> float:
        """
        计算总体质量分数

        Args:
            dimension_scores: 各维度分数

        Returns:
            float: 总体分数 (0-10)
        """
        if not dimension_scores:
            return 0.0

        weighted_sum = sum(
            score * self.dimension_weights.get(dimension, 0.2)
            for dimension, score in dimension_scores.items()
        )

        return min(max(weighted_sum, 0.0), 10.0)

    def _calculate_confidence(self, research_data: Dict[str, Any], overall_score: float) -> float:
        """
        计算评估置信度

        Args:
            research_data: 研究数据
            overall_score: 总体分数

        Returns:
            float: 置信度 (0-1)
        """
        confidence = 0.5  # 基础置信度

        # 基于数据量调整置信度
        literature = research_data.get('literature', {})
        if isinstance(literature, dict):
            total_sources = (
                len(literature.get('github_results', [])) +
                len(literature.get('paper_results', [])) +
                len(literature.get('blog_results', []))
            )

            if total_sources >= 20:
                confidence += 0.3
            elif total_sources >= 10:
                confidence += 0.2
            elif total_sources >= 5:
                confidence += 0.1

        # 基于分数一致性调整置信度
        dimension_scores = self._assess_dimensions(research_data)
        if dimension_scores:
            score_variance = max(dimension_scores.values()) - min(dimension_scores.values())
            if score_variance < 2.0:
                confidence += 0.2
            elif score_variance > 5.0:
                confidence -= 0.2

        # 基于总体分数调整
        if overall_score >= 8.0:
            confidence += 0.1
        elif overall_score < 4.0:
            confidence -= 0.2

        return min(max(confidence, 0.0), 1.0)

    def get_quality_summary(self, quality_score: QualityScore) -> str:
        """
        获取质量评估摘要

        Args:
            quality_score: 质量分数对象

        Returns:
            str: 质量摘要
        """
        summary = f"""## 质量评估摘要

**总体分数**: {quality_score.overall_score:.2f}/10.0
**评估置信度**: {quality_score.confidence:.2f}/1.0

### 各维度评分

"""

        for dimension, score in quality_score.dimension_scores.items():
            dimension_names = {
                'completeness': '完整性',
                'reliability': '可靠性',
                'relevance': '相关性',
                'freshness': '时效性',
                'consistency': '一致性'
            }
            display_name = dimension_names.get(dimension, dimension)
            summary += f"- **{display_name}**: {score:.2f}/10.0\n"

        if quality_score.issues:
            summary += "\n### 发现的问题\n\n"
            for issue in quality_score.issues:
                summary += f"- ⚠️ {issue}\n"

        if quality_score.recommendations:
            summary += "\n### 改进建议\n\n"
            for recommendation in quality_score.recommendations:
                summary += f"- 💡 {recommendation}\n"

        return summary

    async def get_check_history(self, limit: int = 10) -> List[Dict[str, Any]]:
        """
        获取质量检查历史

        Args:
            limit: 返回的记录数量

        Returns:
            List[Dict]: 检查历史记录
        """
        return self.check_history[-limit:] if self.check_history else []

    def clear_check_history(self):
        """清空质量检查历史"""
        self.check_history.clear()
        logger.info("质量检查历史已清空")