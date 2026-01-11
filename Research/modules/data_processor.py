"""
数据处理模块 - DataProcessor
提供数据收集、清洗、分析和可视化功能。
"""

import os
import asyncio
import pandas as pd
import numpy as np
import logging
from typing import Dict, List, Optional, Any
from dataclasses import dataclass
from datetime import datetime, timedelta
from collections import Counter, defaultdict
import random

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
    """数据处理器 - 负责数据收集、清洗和分析"""

    def __init__(self, research_agent):
        self.research_agent = research_agent
        self.config = ProcessingConfig()
        self.apis = {
            'github': {'base_url': 'https://api.github.com', 'token': os.getenv('GITHUB_TOKEN')},
            'stackoverflow': {'base_url': 'https://api.stackexchange.com/2.3', 'key': os.getenv('STACKEXCHANGE_KEY')}
        }
        logger.info("DataProcessor 初始化完成")

    async def process(self, query: str, config: Optional[ProcessingConfig] = None) -> Dict[str, Any]:
        """执行完整的数据处理流程"""
        if config:
            self.config = config

        logger.info(f"开始数据处理: {query}")

        try:
            raw_data = await self._collect_data(query)
            cleaned_data = self._clean_data(raw_data)
            analysis_results = self._analyze_data(cleaned_data, query)
            trend_results = self._analyze_trends(cleaned_data) if self.config.enable_trend_analysis else {}

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
                'report': self._generate_processing_report(query, raw_data, cleaned_data, analysis_results, trend_results)
            }

            logger.info(f"数据处理完成: {query} - 处理了 {len(cleaned_data)} 条记录")
            return result

        except Exception as e:
            logger.error(f"数据处理失败: {e}")
            return {
                'query': query, 'error': str(e), 'processing_summary': {'status': 'failed'},
                'statistics': {}, 'trends': {}, 'recommendations': [], 'report': f"数据处理失败: {e}"
            }

    async def _collect_data(self, query: str) -> List[Dict[str, Any]]:
        """收集多源数据"""
        try:
            data = []

            # AI建议
            try:
                response = self.research_agent.chat(f"为'{query}'提供数据收集建议（领域：{self.research_agent.research_domain}）")
                data.append({
                    'source': 'AI建议', 'type': 'data_collection_strategy', 'content': response,
                    'timestamp': datetime.now().isoformat(), 'metadata': {'query': query, 'domain': self.research_agent.research_domain}
                })
            except Exception as e:
                logger.warning(f"AI建议获取失败: {e}")

            # GitHub数据
            if self.apis['github']['token']:
                data.append({
                    'source': 'GitHub', 'type': 'repository_stats', 'content': f'GitHub repositories for {query}',
                    'timestamp': datetime.now().isoformat(), 'metadata': {'query': query, 'api_endpoint': 'search/repositories'}
                })

            # 模拟数据
            data.extend(self._generate_simulated_data(query))

            logger.info(f"收集到 {len(data)} 条数据记录")
            return data

        except Exception as e:
            logger.error(f"数据收集失败: {e}")
            return []

    def _generate_simulated_data(self, query: str) -> List[Dict[str, Any]]:
        """生成模拟数据用于演示"""
        data_types = ['performance_metrics', 'usage_statistics', 'survey_results', 'market_analysis']
        sources = ['Industry Report', 'Academic Study', 'User Survey', 'Technical Blog']

        metrics_templates = {
            'performance_metrics': {'accuracy': (0.7, 0.95), 'speed': (0.5, 2.0), 'efficiency': (60, 95)},
            'usage_statistics': {'active_users': (1000, 100000), 'growth_rate': (-0.1, 0.3), 'retention': (0.3, 0.8)},
            'survey_results': {'satisfaction': (3.0, 5.0), 'recommendation_rate': (0.6, 0.9), 'feature_adoption': (0.2, 0.8)},
            'market_analysis': {'market_size': (1000000, 100000000), 'growth_potential': (0.1, 0.5)}
        }

        data = []
        for i in range(random.randint(5, 15)):
            data_type = random.choice(data_types)
            source = random.choice(sources)

            metrics = {}
            if data_type in metrics_templates:
                for key, (min_val, max_val) in metrics_templates[data_type].items():
                    if key == 'market_size':
                        metrics[key] = random.randint(int(min_val), int(max_val))
                    elif key == 'competition_level':
                        metrics[key] = random.choice(['Low', 'Medium', 'High'])
                    else:
                        metrics[key] = random.uniform(min_val, max_val)

            data.append({
                'source': source, 'type': data_type, 'content': f'{data_type} for {query}',
                'timestamp': (datetime.now() - timedelta(days=random.randint(0, 30))).isoformat(),
                'metrics': metrics,
                'metadata': {'query': query, 'simulation_id': f"sim_{i}", 'confidence': random.uniform(0.7, 0.9)}
            })

        return data

    def _clean_data(self, raw_data: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """清洗和标准化数据"""
        cleaned_data = []

        for item in raw_data:
            try:
                if not item or not isinstance(item, dict):
                    continue

                if 'timestamp' not in item or 'source' not in item:
                    continue

                # 时间戳标准化
                if isinstance(item['timestamp'], str):
                    try:
                        item['timestamp'] = datetime.fromisoformat(item['timestamp'].replace('Z', '+00:00'))
                    except:
                        item['timestamp'] = datetime.now()

                if 'metrics' in item and not isinstance(item['metrics'], dict):
                    item['metrics'] = {}

                item['quality_score'] = self._calculate_record_quality(item)
                cleaned_data.append(item)

            except Exception as e:
                logger.warning(f"清洗数据记录失败: {e}")
                continue

        # 去重和排序
        cleaned_data = self._remove_duplicates(cleaned_data)
        cleaned_data.sort(key=lambda x: x['timestamp'], reverse=True)

        logger.info(f"数据清洗完成: {len(raw_data)} -> {len(cleaned_data)} 条记录")
        return cleaned_data

    def _calculate_record_quality(self, record: Dict[str, Any]) -> float:
        """计算数据记录质量分数"""
        score = 5.0

        if isinstance(record.get('timestamp'), datetime):
            days_old = (datetime.now() - record['timestamp']).days
            if days_old <= 7:
                score += 2.0
            elif days_old <= 30:
                score += 1.0
            elif days_old <= 90:
                score += 0.5

        for field in ['source', 'type', 'content']:
            if field not in record:
                score -= 0.5

        metrics = record.get('metrics', {})
        if metrics:
            score += min(len(metrics) * 0.3, 2.0)

        metadata = record.get('metadata', {})
        if metadata:
            score += min(len(metadata) * 0.1, 1.0)

        return max(0.0, min(10.0, score))

    def _remove_duplicates(self, data: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """去除重复数据"""
        seen = set()
        unique_data = []

        for item in data:
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
        """分析数据"""
        analysis = {'statistics': {}, 'patterns': [], 'recommendations': [], 'insights': []}

        if not data:
            return analysis

        try:
            analysis['statistics'] = self._calculate_basic_statistics(data)
            analysis['patterns'] = self._identify_patterns(data)
            analysis['recommendations'] = self._generate_recommendations(data, query)
            analysis['insights'] = self._extract_key_insights(data, query)
            return analysis
        except Exception as e:
            logger.error(f"数据分析失败: {e}")
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

        timestamps = [item['timestamp'] for item in data if isinstance(item.get('timestamp'), datetime)]
        if timestamps:
            stats['date_range'] = {
                'earliest': min(timestamps).isoformat(),
                'latest': max(timestamps).isoformat(),
                'span_days': (max(timestamps) - min(timestamps)).days
            }

        stats['source_distribution'] = dict(Counter(item.get('source', 'unknown') for item in data))
        stats['type_distribution'] = dict(Counter(item.get('type', 'unknown') for item in data))

        quality_scores = [item.get('quality_score', 0) for item in data]
        if quality_scores:
            stats['quality_metrics'] = {
                'average_quality': np.mean(quality_scores),
                'min_quality': min(quality_scores),
                'max_quality': max(quality_scores),
                'high_quality_ratio': sum(1 for s in quality_scores if s >= 8) / len(quality_scores)
            }

        return stats

    def _identify_patterns(self, data: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """识别数据模式"""
        patterns = []

        try:
            # 时间趋势模式
            time_groups = defaultdict(list)
            for item in data:
                if isinstance(item.get('timestamp'), datetime):
                    time_groups[item['timestamp'].date()].append(item)

            if len(time_groups) > 1:
                dates = sorted(time_groups.keys())
                counts = [len(time_groups[date]) for date in dates]

                if len(counts) >= 3:
                    recent_counts = counts[-3:]
                    if all(recent_counts[i] > recent_counts[i-1] for i in range(1, len(recent_counts))):
                        patterns.append({'type': 'increasing_trend', 'description': '数据量呈现上升趋势', 'confidence': 0.7})
                    elif all(recent_counts[i] < recent_counts[i-1] for i in range(1, len(recent_counts))):
                        patterns.append({'type': 'decreasing_trend', 'description': '数据量呈现下降趋势', 'confidence': 0.7})

            # 来源多样性
            sources = set(item.get('source', 'unknown') for item in data)
            if len(sources) > 3:
                patterns.append({'type': 'diverse_sources', 'description': f'数据来源多样化（{len(sources)}个来源）', 'confidence': 0.8})

            return patterns

        except Exception as e:
            logger.error(f"模式识别失败: {e}")
            return []

    def _generate_recommendations(self, data: List[Dict[str, Any]], query: str) -> List[str]:
        """生成数据改进建议"""
        recommendations = []

        quality_scores = [item.get('quality_score', 0) for item in data]
        if quality_scores and np.mean(quality_scores) < 7.0:
            recommendations.append("建议提高数据收集标准，重点关注数据来源的权威性和时间新鲜度")

        sources = set(item.get('source', 'unknown') for item in data)
        if len(sources) < 3:
            recommendations.append("建议扩展数据来源，增加数据多样性和代表性")

        timestamps = [item['timestamp'] for item in data if isinstance(item.get('timestamp'), datetime)]
        if timestamps and (max(timestamps) - min(timestamps)).days < 7:
            recommendations.append("建议收集更长时间跨度的数据，以识别长期趋势")

        metrics_count = sum(1 for item in data if item.get('metrics'))
        if metrics_count / len(data) < 0.5:
            recommendations.append("建议为更多数据记录添加量化指标，以支持深度分析")

        return recommendations

    def _extract_key_insights(self, data: List[Dict[str, Any]], query: str) -> List[str]:
        """提取关键洞察"""
        if not data:
            return []

        insights = [f"收集到{len(data)}条相关数据记录"]

        timestamps = [item['timestamp'] for item in data if isinstance(item.get('timestamp'), datetime)]
        if timestamps:
            time_span = (max(timestamps) - min(timestamps)).days
            insights.append(f"数据时间跨度为{time_span}天")

        sources = Counter(item.get('source', 'unknown') for item in data)
        if sources:
            top_source = sources.most_common(1)[0]
            insights.append(f"主要数据来源是{top_source[0]}，占{top_source[1]/len(data):.1%}")

        quality_scores = [item.get('quality_score', 0) for item in data if 'quality_score' in item]
        if quality_scores:
            insights.append(f"数据质量平均分为{np.mean(quality_scores):.1f}/10")

        return insights

    def _analyze_trends(self, data: List[Dict[str, Any]]) -> Dict[str, Any]:
        """分析数据趋势"""
        return {
            'time_trends': {'data_volume_trend': 'stable', 'confidence': 0.6},
            'metric_trends': {'overall_trend': 'insufficient_data', 'confidence': 0.5}
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
                'high': sum(1 for s in quality_scores if s >= 8),
                'medium': sum(1 for s in quality_scores if 5 <= s < 8),
                'low': sum(1 for s in quality_scores if s < 5)
            }
        }

    def _extract_insights(self, data: List[Dict[str, Any]]) -> List[str]:
        """提取数据洞察"""
        return self._extract_key_insights(data, "")

    def _generate_processing_report(self, query: str, raw_count: int, cleaned_count: int,
                                   analysis: Dict[str, Any], trends: Dict[str, Any]) -> str:
        """生成数据处理报告"""
        ts = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        stats = analysis.get('statistics', {})
        recs = analysis.get('recommendations', [])

        return f"""# 数据处理报告

## 概述
- **查询**: {query}
- **处理时间**: {ts}
- **记录数量**: {cleaned_count}

## 数据统计
- 原始记录数: {raw_count}
- 清洗后记录数: {cleaned_count}
- 数据保留率: {cleaned_count/max(raw_count, 1):.1%}

## 分析结果
- 总记录数: {stats.get('total_records', 0)}
- 数据来源: {', '.join(f'{k}:{v}' for k, v in stats.get('source_distribution', {}).items())}

## 建议
{chr(10).join(f'- {r}' for r in recs) if recs else '无具体建议'}

---
*报告生成时间: {ts}*
"""
