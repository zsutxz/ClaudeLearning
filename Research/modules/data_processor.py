"""
数据处理模块 - DataProcessor

提供研究数据的收集、清洗、分析和可视化功能，支持:
- 多源数据收集和集成
- 数据清洗和标准化
- 统计分析和趋势识别
- 数据可视化和报告
"""

import os
import asyncio
import aiohttp
import pandas as pd
import numpy as np
import logging
from typing import Dict, List, Optional, Any, Tuple
from dataclasses import dataclass
from datetime import datetime, timedelta
import json
import re
from collections import Counter, defaultdict

# 配置日志
logger = logging.getLogger(__name__)

@dataclass
class DataPoint:
    """数据点类"""
    timestamp: datetime
    source: str
    metric_name: str
    value: Any
    metadata: Dict[str, Any]

@dataclass
class ProcessingConfig:
    """数据处理配置"""
    enable_statistics: bool = True
    enable_trend_analysis: bool = True
    enable_sentiment_analysis: bool = False
    enable_categorization: bool = True
    time_window_days: int = 30
    min_data_points: int = 5

class DataProcessor:
    """
    数据处理器

    负责研究数据的收集、清洗、分析和处理
    """

    def __init__(self, research_agent):
        """
        初始化数据处理器

        Args:
            research_agent: ResearchAgent实例
        """
        self.research_agent = research_agent
        self.config = ProcessingConfig()

        # 数据存储
        self.raw_data = []
        self.processed_data = {}
        self.statistics = {}
        self.trends = {}

        # API配置
        self.apis = {
            'github': {
                'base_url': 'https://api.github.com',
                'token': os.getenv('GITHUB_TOKEN')
            },
            'stackoverflow': {
                'base_url': 'https://api.stackexchange.com/2.3',
                'key': os.getenv('STACKEXCHANGE_KEY')
            }
        }

        logger.info("DataProcessor 初始化完成")

    async def process(self, query: str, config: Optional[ProcessingConfig] = None) -> Dict[str, Any]:
        """
        执行完整的数据处理流程

        Args:
            query: 研究查询
            config: 处理配置

        Returns:
            Dict: 处理结果
        """
        if config:
            self.config = config

        logger.info(f"开始数据处理: {query}")

        try:
            # 1. 数据收集
            raw_data = await self._collect_data(query)

            # 2. 数据清洗
            cleaned_data = self._clean_data(raw_data)

            # 3. 数据分析
            analysis_results = self._analyze_data(cleaned_data, query)

            # 4. 趋势分析
            trend_results = self._analyze_trends(cleaned_data) if self.config.enable_trend_analysis else {}

            # 5. 生成报告
            report = self._generate_processing_report({
                'query': query,
                'raw_data_count': len(raw_data),
                'cleaned_data_count': len(cleaned_data),
                'analysis': analysis_results,
                'trends': trend_results
            })

            result = {
                'query': query,
                'processing_summary': {
                    'total_records': len(cleaned_data),
                    'processing_time': datetime.now().isoformat(),
                    'data_sources': list(set(d.get('source', 'unknown') for d in cleaned_data)),
                    'quality_metrics': self._calculate_quality_metrics(cleaned_data)
                },
                'statistics': analysis_results.get('statistics', {}),
                'trends': trend_results,
                'recommendations': analysis_results.get('recommendations', []),
                'raw_insights': self._extract_insights(cleaned_data),
                'report': report
            }

            logger.info(f"数据处理完成: {query} - 处理了 {len(cleaned_data)} 条记录")
            return result

        except Exception as e:
            logger.error(f"数据处理失败: {str(e)}")
            return {
                'query': query,
                'error': str(e),
                'processing_summary': {'status': 'failed'},
                'statistics': {},
                'trends': {},
                'recommendations': [],
                'report': f"数据处理失败: {str(e)}"
            }

    async def _collect_data(self, query: str) -> List[Dict[str, Any]]:
        """
        收集多源数据

        Args:
            query: 研究查询

        Returns:
            List[Dict]: 原始数据
        """
        data = []

        try:
            # 1. 基于AI的数据收集建议
            ai_suggestions = await self._get_ai_data_suggestions(query)
            data.extend(ai_suggestions)

            # 2. 公开API数据收集 (如果有API密钥)
            if self.apis['github']['token']:
                github_data = await self._collect_github_data(query)
                data.extend(github_data)

            # 3. 模拟数据收集 (用于演示)
            simulated_data = self._generate_simulated_data(query)
            data.extend(simulated_data)

            logger.info(f"收集到 {len(data)} 条数据记录")
            return data

        except Exception as e:
            logger.error(f"数据收集失败: {str(e)}")
            return []

    async def _get_ai_data_suggestions(self, query: str) -> List[Dict[str, Any]]:
        """获取AI建议的数据收集策略"""
        try:
            prompt = f"""对于研究主题"{query}"，请提供数据收集建议。

研究领域: {self.research_agent.research_domain}

请提供:
1. 推荐的数据源和API
2. 关键指标和测量维度
3. 数据收集方法和工具
4. 数据质量评估标准

以结构化的JSON格式返回建议。"""

            response = self.research_agent.chat(prompt)

            # 将AI响应转换为数据记录格式
            return [{
                'source': 'AI建议',
                'type': 'data_collection_strategy',
                'content': response,
                'timestamp': datetime.now().isoformat(),
                'metadata': {
                    'query': query,
                    'domain': self.research_agent.research_domain
                }
            }]

        except Exception as e:
            logger.error(f"获取AI数据建议失败: {str(e)}")
            return []

    async def _collect_github_data(self, query: str) -> List[Dict[str, Any]]:
        """收集GitHub相关数据"""
        try:
            # 这里可以实现真实的GitHub API调用
            # 目前返回模拟数据
            return [{
                'source': 'GitHub',
                'type': 'repository_stats',
                'content': f'GitHub repositories related to {query}',
                'timestamp': datetime.now().isoformat(),
                'metadata': {
                    'query': query,
                    'api_endpoint': 'search/repositories'
                }
            }]

        except Exception as e:
            logger.error(f"GitHub数据收集失败: {str(e)}")
            return []

    def _generate_simulated_data(self, query: str) -> List[Dict[str, Any]]:
        """生成模拟数据用于演示"""
        import random

        data_types = ['performance_metrics', 'usage_statistics', 'survey_results', 'market_analysis']
        sources = ['Industry Report', 'Academic Study', 'User Survey', 'Technical Blog']

        simulated_data = []
        for i in range(random.randint(5, 15)):
            data_type = random.choice(data_types)
            source = random.choice(sources)

            # 生成模拟指标
            metrics = {}
            if data_type == 'performance_metrics':
                metrics = {
                    'accuracy': random.uniform(0.7, 0.95),
                    'speed': random.uniform(0.5, 2.0),
                    'efficiency': random.uniform(60, 95)
                }
            elif data_type == 'usage_statistics':
                metrics = {
                    'active_users': random.randint(1000, 100000),
                    'growth_rate': random.uniform(-0.1, 0.3),
                    'retention': random.uniform(0.3, 0.8)
                }
            elif data_type == 'survey_results':
                metrics = {
                    'satisfaction': random.uniform(3.0, 5.0),
                    'recommendation_rate': random.uniform(0.6, 0.9),
                    'feature_adoption': random.uniform(0.2, 0.8)
                }
            else:  # market_analysis
                metrics = {
                    'market_size': random.randint(1000000, 100000000),
                    'competition_level': random.choice(['Low', 'Medium', 'High']),
                    'growth_potential': random.uniform(0.1, 0.5)
                }

            simulated_data.append({
                'source': source,
                'type': data_type,
                'content': f'Simulated {data_type} data for {query}',
                'timestamp': (datetime.now() - timedelta(days=random.randint(0, 30))).isoformat(),
                'metrics': metrics,
                'metadata': {
                    'query': query,
                    'simulation_id': f"sim_{i}",
                    'confidence': random.uniform(0.7, 0.9)
                }
            })

        return simulated_data

    def _clean_data(self, raw_data: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """
        清洗和标准化数据

        Args:
            raw_data: 原始数据

        Returns:
            List[Dict]: 清洗后的数据
        """
        cleaned_data = []

        for item in raw_data:
            try:
                # 基本数据验证
                if not item or not isinstance(item, dict):
                    continue

                # 必需字段检查
                if 'timestamp' not in item or 'source' not in item:
                    continue

                # 时间戳标准化
                if isinstance(item['timestamp'], str):
                    try:
                        timestamp = datetime.fromisoformat(item['timestamp'].replace('Z', '+00:00'))
                        item['timestamp'] = timestamp
                    except:
                        item['timestamp'] = datetime.now()

                # 数据类型验证
                if 'metrics' in item and not isinstance(item['metrics'], dict):
                    item['metrics'] = {}

                # 添加质量分数
                item['quality_score'] = self._calculate_record_quality(item)

                cleaned_data.append(item)

            except Exception as e:
                logger.warning(f"清洗数据记录失败: {str(e)}")
                continue

        # 去重
        cleaned_data = self._remove_duplicates(cleaned_data)

        # 排序
        cleaned_data.sort(key=lambda x: x['timestamp'], reverse=True)

        logger.info(f"数据清洗完成: {len(raw_data)} -> {len(cleaned_data)} 条记录")
        return cleaned_data

    def _calculate_record_quality(self, record: Dict[str, Any]) -> float:
        """计算数据记录质量分数"""
        score = 5.0  # 基础分数

        # 时间新鲜度
        if isinstance(record.get('timestamp'), datetime):
            days_old = (datetime.now() - record['timestamp']).days
            if days_old <= 7:
                score += 2.0
            elif days_old <= 30:
                score += 1.0
            elif days_old <= 90:
                score += 0.5

        # 数据完整性
        required_fields = ['source', 'type', 'content']
        missing_fields = sum(1 for field in required_fields if field not in record)
        score -= missing_fields * 0.5

        # 指标丰富度
        metrics = record.get('metrics', {})
        if metrics:
            score += min(len(metrics) * 0.3, 2.0)

        # 元数据完整性
        metadata = record.get('metadata', {})
        if metadata:
            score += min(len(metadata) * 0.1, 1.0)

        return max(0.0, min(10.0, score))

    def _remove_duplicates(self, data: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """去除重复数据"""
        seen = set()
        unique_data = []

        for item in data:
            # 创建唯一标识符
            identifier = (
                item.get('source', ''),
                item.get('type', ''),
                item.get('content', ''),
                item.get('timestamp').isoformat() if isinstance(item.get('timestamp'), datetime) else ''
            )

            if identifier not in seen:
                seen.add(identifier)
                unique_data.append(item)

        return unique_data

    def _analyze_data(self, data: List[Dict[str, Any]], query: str) -> Dict[str, Any]:
        """
        分析数据

        Args:
            data: 清洗后的数据
            query: 研究查询

        Returns:
            Dict: 分析结果
        """
        analysis = {
            'statistics': {},
            'patterns': [],
            'recommendations': [],
            'insights': []
        }

        if not data:
            return analysis

        try:
            # 基本统计
            analysis['statistics'] = self._calculate_basic_statistics(data)

            # 模式识别
            analysis['patterns'] = self._identify_patterns(data)

            # 生成建议
            analysis['recommendations'] = self._generate_recommendations(data, query)

            # 提取洞察
            analysis['insights'] = self._extract_key_insights(data, query)

            return analysis

        except Exception as e:
            logger.error(f"数据分析失败: {str(e)}")
            return analysis

    def _calculate_basic_statistics(self, data: List[Dict[str, Any]]) -> Dict[str, Any]:
        """计算基本统计数据"""
        stats = {
            'total_records': len(data),
            'date_range': {},
            'source_distribution': {},
            'type_distribution': {},
            'quality_metrics': {}
        }

        if not data:
            return stats

        # 时间范围
        timestamps = [item['timestamp'] for item in data if isinstance(item.get('timestamp'), datetime)]
        if timestamps:
            stats['date_range'] = {
                'earliest': min(timestamps).isoformat(),
                'latest': max(timestamps).isoformat(),
                'span_days': (max(timestamps) - min(timestamps)).days
            }

        # 来源分布
        sources = [item.get('source', 'unknown') for item in data]
        stats['source_distribution'] = dict(Counter(sources))

        # 类型分布
        types = [item.get('type', 'unknown') for item in data]
        stats['type_distribution'] = dict(Counter(types))

        # 质量指标
        quality_scores = [item.get('quality_score', 0) for item in data]
        if quality_scores:
            stats['quality_metrics'] = {
                'average_quality': np.mean(quality_scores),
                'min_quality': min(quality_scores),
                'max_quality': max(quality_scores),
                'high_quality_ratio': sum(1 for score in quality_scores if score >= 8) / len(quality_scores)
            }

        return stats

    def _identify_patterns(self, data: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """识别数据模式"""
        patterns = []

        try:
            # 时间趋势模式
            time_patterns = self._analyze_time_patterns(data)
            if time_patterns:
                patterns.extend(time_patterns)

            # 来源关联模式
            source_patterns = self._analyze_source_patterns(data)
            if source_patterns:
                patterns.extend(source_patterns)

            # 指标相关性模式
            metric_patterns = self._analyze_metric_patterns(data)
            if metric_patterns:
                patterns.extend(metric_patterns)

            return patterns

        except Exception as e:
            logger.error(f"模式识别失败: {str(e)}")
            return []

    def _analyze_time_patterns(self, data: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """分析时间模式"""
        patterns = []

        # 按时间分组统计
        time_groups = defaultdict(list)
        for item in data:
            if isinstance(item.get('timestamp'), datetime):
                date_key = item['timestamp'].date()
                time_groups[date_key].append(item)

        if len(time_groups) > 1:
            dates = sorted(time_groups.keys())
            counts = [len(time_groups[date]) for date in dates]

            # 简单的线性趋势检测
            if len(counts) >= 3:
                recent_counts = counts[-3:]
                if all(recent_counts[i] > recent_counts[i-1] for i in range(1, len(recent_counts))):
                    patterns.append({
                        'type': 'increasing_trend',
                        'description': '数据量呈现上升趋势',
                        'confidence': 0.7
                    })
                elif all(recent_counts[i] < recent_counts[i-1] for i in range(1, len(recent_counts))):
                    patterns.append({
                        'type': 'decreasing_trend',
                        'description': '数据量呈现下降趋势',
                        'confidence': 0.7
                    })

        return patterns

    def _analyze_source_patterns(self, data: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """分析来源模式"""
        patterns = []

        # 来源多样性分析
        sources = set(item.get('source', 'unknown') for item in data)
        if len(sources) > 3:
            patterns.append({
                'type': 'diverse_sources',
                'description': f'数据来源多样化，包含{len(sources)}个不同来源',
                'confidence': 0.8
            })

        # 主导来源识别
        source_counts = Counter(item.get('source', 'unknown') for item in data)
        if source_counts:
            dominant_source, dominant_count = source_counts.most_common(1)[0]
            if dominant_count / len(data) > 0.6:
                patterns.append({
                    'type': 'dominant_source',
                    'description': f'主要数据来源是{dominant_source}，占比{dominant_count/len(data):.1%}',
                    'confidence': 0.9
                })

        return patterns

    def _analyze_metric_patterns(self, data: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """分析指标模式"""
        patterns = []

        # 收集所有指标
        all_metrics = defaultdict(list)
        for item in data:
            metrics = item.get('metrics', {})
            for key, value in metrics.items():
                if isinstance(value, (int, float)):
                    all_metrics[key].append(value)

        # 分析指标分布
        for metric_name, values in all_metrics.items():
            if len(values) >= 3:
                mean_val = np.mean(values)
                std_val = np.std(values)

                # 识别异常值
                outliers = [v for v in values if abs(v - mean_val) > 2 * std_val]
                if outliers:
                    patterns.append({
                        'type': 'metric_outliers',
                        'description': f'指标{metric_name}存在{len(outliers)}个异常值',
                        'confidence': 0.8,
                        'metric': metric_name,
                        'outlier_ratio': len(outliers) / len(values)
                    })

        return patterns

    def _generate_recommendations(self, data: List[Dict[str, Any]], query: str) -> List[str]:
        """生成数据改进建议"""
        recommendations = []

        # 数据质量建议
        quality_scores = [item.get('quality_score', 0) for item in data]
        if quality_scores and np.mean(quality_scores) < 7.0:
            recommendations.append("建议提高数据收集标准，重点关注数据来源的权威性和时间新鲜度")

        # 数据多样性建议
        sources = set(item.get('source', 'unknown') for item in data)
        if len(sources) < 3:
            recommendations.append("建议扩展数据来源，增加数据多样性和代表性")

        # 时间覆盖建议
        timestamps = [item['timestamp'] for item in data if isinstance(item.get('timestamp'), datetime)]
        if timestamps:
            time_span = (max(timestamps) - min(timestamps)).days
            if time_span < 7:
                recommendations.append("建议收集更长时间跨度的数据，以识别长期趋势")

        # 指标完善建议
        metrics_count = sum(1 for item in data if item.get('metrics'))
        if metrics_count / len(data) < 0.5:
            recommendations.append("建议为更多数据记录添加量化指标，以支持深度分析")

        return recommendations

    def _extract_key_insights(self, data: List[Dict[str, Any]], query: str) -> List[str]:
        """提取关键洞察"""
        insights = []

        if not data:
            return insights

        try:
            # 基于数据特征的洞察
            insights.append(f"收集到{len(data)}条相关数据记录")

            # 时间洞察
            timestamps = [item['timestamp'] for item in data if isinstance(item.get('timestamp'), datetime)]
            if timestamps:
                time_span = (max(timestamps) - min(timestamps)).days
                insights.append(f"数据时间跨度为{time_span}天")

            # 来源洞察
            sources = Counter(item.get('source', 'unknown') for item in data)
            if sources:
                top_source = sources.most_common(1)[0]
                insights.append(f"主要数据来源是{top_source[0]}，占{top_source[1]/len(data):.1%}")

            # 质量洞察
            quality_scores = [item.get('quality_score', 0) for item in data if 'quality_score' in item]
            if quality_scores:
                avg_quality = np.mean(quality_scores)
                insights.append(f"数据质量平均分为{avg_quality:.1f}/10")

            return insights

        except Exception as e:
            logger.error(f"洞察提取失败: {str(e)}")
            return ["数据洞察提取失败"]

    def _analyze_trends(self, data: List[Dict[str, Any]]) -> Dict[str, Any]:
        """分析数据趋势"""
        trends = {
            'time_trends': {},
            'metric_trends': {},
            'source_trends': {}
        }

        try:
            # 时间趋势
            time_trends = self._calculate_time_trends(data)
            trends['time_trends'] = time_trends

            # 指标趋势
            metric_trends = self._calculate_metric_trends(data)
            trends['metric_trends'] = metric_trends

            return trends

        except Exception as e:
            logger.error(f"趋势分析失败: {str(e)}")
            return trends

    def _calculate_time_trends(self, data: List[Dict[str, Any]]) -> Dict[str, Any]:
        """计算时间趋势"""
        # 简化的时间趋势分析
        return {
            'data_volume_trend': 'stable',
            'confidence': 0.6
        }

    def _calculate_metric_trends(self, data: List[Dict[str, Any]]) -> Dict[str, Any]:
        """计算指标趋势"""
        # 简化的指标趋势分析
        return {
            'overall_trend': 'insufficient_data',
            'confidence': 0.5
        }

    def _calculate_quality_metrics(self, data: List[Dict[str, Any]]) -> Dict[str, Any]:
        """计算数据质量指标"""
        if not data:
            return {}

        quality_scores = [item.get('quality_score', 0) for item in data if 'quality_score' in item]

        if not quality_scores:
            return {}

        return {
            'average_quality': np.mean(quality_scores),
            'quality_distribution': {
                'high': sum(1 for score in quality_scores if score >= 8),
                'medium': sum(1 for score in quality_scores if 5 <= score < 8),
                'low': sum(1 for score in quality_scores if score < 5)
            }
        }

    def _extract_insights(self, data: List[Dict[str, Any]]) -> List[str]:
        """提取数据洞察"""
        return self._extract_key_insights(data, "")

    def _generate_processing_report(self, processing_data: Dict[str, Any]) -> str:
        """生成数据处理报告"""
        query = processing_data.get('query', 'Unknown Query')
        timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')

        report = f"""# 数据处理报告

## 概述
**查询**: {query}
**处理时间**: {timestamp}
**记录数量**: {processing_data.get('cleaned_data_count', 0)}

## 数据统计
- 原始记录数: {processing_data.get('raw_data_count', 0)}
- 清洗后记录数: {processing_data.get('cleaned_data_count', 0)}
- 数据保留率: {processing_data.get('cleaned_data_count', 0) / max(processing_data.get('raw_data_count', 1), 1):.1%}

## 分析结果
{self._format_analysis_results(processing_data.get('analysis', {}))}

## 趋势分析
{self._format_trend_results(processing_data.get('trends', {}))}

## 结论
数据处理完成，已生成统计分析和趋势报告。

---
*报告生成时间: {timestamp}*
"""
        return report

    def _format_analysis_results(self, analysis: Dict[str, Any]) -> str:
        """格式化分析结果"""
        if not analysis:
            return "暂无分析结果"

        formatted = []

        stats = analysis.get('statistics', {})
        if stats:
            formatted.append(f"- 总记录数: {stats.get('total_records', 0)}")

            source_dist = stats.get('source_distribution', {})
            if source_dist:
                formatted.append(f"- 数据来源: {', '.join(f'{k}:{v}' for k, v in source_dist.items())}")

        recommendations = analysis.get('recommendations', [])
        if recommendations:
            formatted.append("\n**建议**:")
            for rec in recommendations:
                formatted.append(f"- {rec}")

        return '\n'.join(formatted)

    def _format_trend_results(self, trends: Dict[str, Any]) -> str:
        """格式化趋势结果"""
        if not trends:
            return "暂无趋势分析"

        formatted = []

        for trend_type, trend_data in trends.items():
            if isinstance(trend_data, dict):
                formatted.append(f"**{trend_type}**:")
                for key, value in trend_data.items():
                    formatted.append(f"  - {key}: {value}")

        return '\n'.join(formatted) if formatted else "趋势数据处理完成"