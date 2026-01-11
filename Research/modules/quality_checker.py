"""
质量检查模块 - QualityChecker
提供研究数据的质量评估和验证功能。
"""

import os
import asyncio
import logging
from typing import Dict, List, Optional, Any, Tuple
from dataclasses import dataclass
from datetime import datetime
from collections import defaultdict

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
    """质量检查器 - 评估研究数据的可靠性和完整性"""

    def __init__(self, research_agent):
        self.research_agent = research_agent
        self.config = QualityConfig()

        # 质量维度权重
        self.dimension_weights = {
            'completeness': 0.25,
            'reliability': 0.25,
            'relevance': 0.20,
            'freshness': 0.15,
            'consistency': 0.15
        }

        self.check_history = []
        logger.info("QualityChecker 初始化完成")

    async def check(self, research_data: Dict[str, Any]) -> QualityScore:
        """执行全面的质量检查"""
        try:
            logger.info("开始执行质量检查")

            # 执行各项检查
            dimension_scores = self._assess_dimensions(research_data)
            issues = self._identify_issues(research_data, dimension_scores)
            recommendations = self._generate_recommendations(issues, dimension_scores)
            overall_score = self._calculate_overall_score(dimension_scores)
            confidence = self._calculate_confidence(research_data, overall_score)

            quality_score = QualityScore(
                overall_score=overall_score,
                dimension_scores=dimension_scores,
                issues=issues,
                recommendations=recommendations,
                confidence=confidence
            )

            self.check_history.append({
                'timestamp': datetime.now(),
                'overall_score': overall_score,
                'issues_count': len(issues),
                'confidence': confidence
            })

            logger.info(f"质量检查完成 - 总分: {overall_score:.2f}, 置信度: {confidence:.2f}")
            return quality_score

        except Exception as e:
            logger.error(f"质量检查失败: {e}")
            return QualityScore(
                overall_score=0.0, dimension_scores={}, issues=[f"质量检查失败: {e}"],
                recommendations=["请检查数据格式和完整性"], confidence=0.0
            )

    def _assess_dimensions(self, research_data: Dict[str, Any]) -> Dict[str, float]:
        """多维度质量评估"""
        return {
            'completeness': self._assess_completeness(research_data),
            'reliability': self._assess_reliability(research_data),
            'relevance': self._assess_relevance(research_data),
            'freshness': self._assess_freshness(research_data),
            'consistency': self._assess_consistency(research_data)
        }

    def _assess_completeness(self, research_data: Dict[str, Any]) -> float:
        """评估数据完整性"""
        score = 5.0
        literature = research_data.get('literature', {})

        if isinstance(literature, dict):
            source_count = sum(1 for k in ['github_results', 'paper_results', 'blog_results'] if literature.get(k))
            if source_count >= 3:
                score += 2.0
            elif source_count >= 2:
                score += 1.0

            total_results = sum(len(literature.get(k, [])) for k in ['github_results', 'paper_results', 'blog_results'])
            if total_results >= 20:
                score += 2.0
            elif total_results >= 10:
                score += 1.0
            elif total_results >= 5:
                score += 0.5

        analysis = research_data.get('analysis', {})
        if isinstance(analysis, dict):
            if analysis.get('analysis_report'):
                score += 0.5
            if analysis.get('key_findings'):
                score += 0.5

        return min(max(score, 0.0), 10.0)

    def _assess_reliability(self, research_data: Dict[str, Any]) -> float:
        """评估数据可靠性"""
        score = 5.0
        literature = research_data.get('literature', {})

        if isinstance(literature, dict):
            github_results = literature.get('github_results', [])
            if github_results:
                high_quality = sum(1 for r in github_results if hasattr(r, 'metadata') and r.metadata.get('stars', 0) > 100)
                if github_results:
                    score += (high_quality / len(github_results)) * 2.0

            paper_results = literature.get('paper_results', [])
            if paper_results:
                score += min(len(paper_results) * 0.5, 2.0)

        provider = research_data.get('metadata', {}).get('provider', '').lower()
        if provider in ['claude', 'openai', 'anthropic']:
            score += 1.0
        elif provider:
            score += 0.5

        return min(max(score, 0.0), 10.0)

    def _assess_relevance(self, research_data: Dict[str, Any]) -> float:
        """评估数据相关性"""
        score = 5.0
        query = research_data.get('query', '').lower()
        if not query:
            return 0.0

        literature = research_data.get('literature', {})
        if isinstance(literature, dict):
            relevance_scores = []

            for result_type in ['github_results', 'paper_results', 'blog_results']:
                for item in literature.get(result_type, []):
                    if hasattr(item, 'title') and hasattr(item, 'description'):
                        text = f"{item.title} {item.description}".lower()
                        relevance = self._calculate_text_relevance(query, text)
                        relevance_scores.append(relevance)

            if relevance_scores:
                score += (sum(relevance_scores) / len(relevance_scores)) * 3.0

        analysis = research_data.get('analysis', {})
        if isinstance(analysis, dict):
            analysis_text = analysis.get('analysis_report', '').lower()
            if analysis_text:
                score += self._calculate_text_relevance(query, analysis_text) * 2.0

        return min(max(score, 0.0), 10.0)

    def _assess_freshness(self, research_data: Dict[str, Any]) -> float:
        """评估数据时效性"""
        score = 5.0
        literature = research_data.get('literature', {})

        if isinstance(literature, dict):
            timestamps = []
            for result_type in ['github_results', 'paper_results', 'blog_results']:
                for item in literature.get(result_type, []):
                    if hasattr(item, 'timestamp') and isinstance(item.timestamp, datetime):
                        timestamps.append(item.timestamp)

            if timestamps:
                now = datetime.now()
                freshness_scores = []
                for ts in timestamps:
                    days_old = (now - ts).days
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
                    score += (sum(freshness_scores) / len(freshness_scores)) * 3.0

        return min(max(score, 0.0), 10.0)

    def _assess_consistency(self, research_data: Dict[str, Any]) -> float:
        """评估数据一致性"""
        score = 7.0

        literature = research_data.get('literature', {})
        if isinstance(literature, dict):
            sources = [k for k in ['github_results', 'paper_results', 'blog_results'] if literature.get(k)]
            if len(sources) >= 3:
                score += 1.0
            elif len(sources) >= 2:
                score += 0.5

        analysis = research_data.get('analysis', {})
        if isinstance(analysis, dict) and analysis.get('analysis_report'):
            score += 0.5

        return min(max(score, 0.0), 10.0)

    def _calculate_text_relevance(self, query: str, text: str) -> float:
        """计算文本相关性"""
        query_words = set(query.split())
        text_words = set(text.split())

        if not query_words:
            return 0.0

        intersection = query_words.intersection(text_words)
        overlap_ratio = len(intersection) / len(query_words)
        length_factor = min(len(text) / 1000, 1.0)

        return min(overlap_ratio * (1 + length_factor), 1.0)

    def _identify_issues(self, research_data: Dict[str, Any], dimension_scores: Dict[str, float]) -> List[str]:
        """识别质量问题"""
        issues = []

        dimension_names = {
            'completeness': '数据不完整，缺少关键信息或数据源',
            'reliability': '数据来源可靠性较低，建议增加权威来源',
            'relevance': '数据与研究主题相关性不够强',
            'freshness': '数据时效性较差，部分信息可能过时',
            'consistency': '不同来源数据存在不一致性'
        }

        for dimension, score in dimension_scores.items():
            if score < 5.0:
                issues.append(dimension_names.get(dimension, f"{dimension}分数较低"))

        # 检查数据源数量
        literature = research_data.get('literature', {})
        if isinstance(literature, dict):
            total_sources = sum(len(literature.get(k, [])) for k in ['github_results', 'paper_results', 'blog_results'])
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
        """生成改进建议"""
        recommendations = []

        issue_recommendations = {
            "数据不完整": ["扩展数据收集范围", "补充关键指标", "完善元数据"],
            "可靠性较低": ["选择同行评议论文", "增加企业报告", "验证项目影响力"],
            "相关性不够强": ["优化搜索关键词", "使用专业数据库", "增加专家推荐"],
            "时效性较差": ["收集最近6个月数据", "设置时间过滤器", "关注最新动态"]
        }

        for issue in issues:
            for keyword, recs in issue_recommendations.items():
                if keyword in issue:
                    recommendations.extend(recs)

        # 基于维度分数生成建议
        low_scores = [dim for dim, score in dimension_scores.items() if score < 6.0]
        if low_scores:
            recommendations.append(f"重点改进: {', '.join(low_scores)}")

        high_scores = [dim for dim, score in dimension_scores.items() if score >= 8.0]
        if high_scores:
            recommendations.append(f"保持优势: {', '.join(high_scores)}")

        return list(set(recommendations))

    def _calculate_overall_score(self, dimension_scores: Dict[str, float]) -> float:
        """计算总体质量分数"""
        if not dimension_scores:
            return 0.0

        weighted_sum = sum(
            score * self.dimension_weights.get(dimension, 0.2)
            for dimension, score in dimension_scores.items()
        )

        return min(max(weighted_sum, 0.0), 10.0)

    def _calculate_confidence(self, research_data: Dict[str, Any], overall_score: float) -> float:
        """计算评估置信度"""
        confidence = 0.5

        literature = research_data.get('literature', {})
        if isinstance(literature, dict):
            total_sources = sum(len(literature.get(k, [])) for k in ['github_results', 'paper_results', 'blog_results'])

            if total_sources >= 20:
                confidence += 0.3
            elif total_sources >= 10:
                confidence += 0.2
            elif total_sources >= 5:
                confidence += 0.1

        if overall_score >= 8.0:
            confidence += 0.1
        elif overall_score < 4.0:
            confidence -= 0.2

        return min(max(confidence, 0.0), 1.0)

    def get_quality_summary(self, quality_score: QualityScore) -> str:
        """获取质量评估摘要"""
        dimension_names = {
            'completeness': '完整性', 'reliability': '可靠性', 'relevance': '相关性',
            'freshness': '时效性', 'consistency': '一致性'
        }

        summary = f"## 质量评估摘要\n\n**总体分数**: {quality_score.overall_score:.2f}/10.0\n**评估置信度**: {quality_score.confidence:.2f}/1.0\n\n### 各维度评分\n\n"

        for dimension, score in quality_score.dimension_scores.items():
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
        """获取质量检查历史"""
        return self.check_history[-limit:] if self.check_history else []

    def clear_check_history(self):
        """清空质量检查历史"""
        self.check_history.clear()
        logger.info("质量检查历史已清空")
