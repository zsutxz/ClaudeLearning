"""
文献检索模块 - LiteratureRetriever

提供多源文献检索和分析功能，支持:
- GitHub仓库分析
- 学术论文检索
- 技术博客搜索
- 官方文档解析
"""

import os
import asyncio
import aiohttp
import logging
from typing import Dict, List, Optional, Any
from dataclasses import dataclass
from datetime import datetime, timedelta
import json
import re

# 配置日志
logger = logging.getLogger(__name__)

@dataclass
class SearchResult:
    """搜索结果类"""
    title: str
    url: str
    description: str
    source: str
    relevance_score: float
    timestamp: datetime
    metadata: Dict[str, Any]

@dataclass
class LiteratureConfig:
    """文献检索配置"""
    max_github_results: int = 10
    max_paper_results: int = 5
    max_blog_results: int = 5
    include_github: bool = True
    include_papers: bool = True
    include_blogs: bool = True
    time_range_days: int = 180

class LiteratureRetriever:
    """
    文献检索器

    集成多种数据源，提供统一的文献检索接口
    """

    def __init__(self, research_agent):
        """
        初始化文献检索器

        Args:
            research_agent: ResearchAgent实例
        """
        self.research_agent = research_agent
        self.config = LiteratureConfig()

        # API配置
        self.github_token = os.getenv('GITHUB_TOKEN')
        self.github_api_base = 'https://api.github.com'

        # arXiv API配置
        self.arxiv_api_base = 'http://export.arxiv.org/api/query'

        # 搜索历史
        self.search_history = []

        logger.info("LiteratureRetriever 初始化完成")

    async def search(self, query: str, config: Optional[LiteratureConfig] = None) -> Dict[str, Any]:
        """
        执行多源文献检索

        Args:
            query: 搜索查询
            config: 检索配置

        Returns:
            Dict: 检索结果汇总
        """
        if config:
            self.config = config

        logger.info(f"开始文献检索: {query}")

        results = {
            'query': query,
            'timestamp': datetime.now().isoformat(),
            'github_results': [],
            'paper_results': [],
            'blog_results': [],
            'total_results': 0,
            'search_summary': {}
        }

        try:
            # 并行执行多种检索
            tasks = []

            if self.config.include_github:
                tasks.append(self._search_github(query))

            if self.config.include_papers:
                tasks.append(self._search_arxiv(query))

            if self.config.include_blogs:
                tasks.append(self._search_tech_blogs(query))

            # 执行所有检索任务
            if tasks:
                search_results = await asyncio.gather(*tasks, return_exceptions=True)

                # 处理结果
                result_index = 0
                if self.config.include_github:
                    if result_index < len(search_results) and not isinstance(search_results[result_index], Exception):
                        results['github_results'] = search_results[result_index]
                    result_index += 1

                if self.config.include_papers:
                    if result_index < len(search_results) and not isinstance(search_results[result_index], Exception):
                        results['paper_results'] = search_results[result_index]
                    result_index += 1

                if self.config.include_blogs:
                    if result_index < len(search_results) and not isinstance(search_results[result_index], Exception):
                        results['blog_results'] = search_results[result_index]
                    result_index += 1

            # 计算总结果数
            results['total_results'] = (
                len(results['github_results']) +
                len(results['paper_results']) +
                len(results['blog_results'])
            )

            # 生成检索摘要
            results['search_summary'] = self._generate_search_summary(results, query)

            # 记录搜索历史
            self.search_history.append({
                'query': query,
                'timestamp': datetime.now(),
                'total_results': results['total_results'],
                'config': self.config.__dict__
            })

            logger.info(f"文献检索完成: {query} - 找到 {results['total_results']} 个结果")
            return results

        except Exception as e:
            logger.error(f"文献检索失败: {str(e)}")
            return {
                'query': query,
                'error': str(e),
                'timestamp': datetime.now().isoformat(),
                'github_results': [],
                'paper_results': [],
                'blog_results': [],
                'total_results': 0
            }

    async def _search_github(self, query: str) -> List[SearchResult]:
        """
        搜索GitHub仓库

        Args:
            query: 搜索查询

        Returns:
            List[SearchResult]: GitHub搜索结果
        """
        try:
            if not self.github_token:
                logger.warning("未配置GitHub token，跳过GitHub搜索")
                return []

            # 构建GitHub搜索查询
            github_query = self._build_github_query(query)

            async with aiohttp.ClientSession() as session:
                headers = {
                    'Authorization': f'token {self.github_token}',
                    'Accept': 'application/vnd.github.v3+json'
                }

                # 搜索仓库
                repos_url = f"{self.github_api_base}/search/repositories"
                repos_params = {
                    'q': github_query,
                    'sort': 'stars',
                    'order': 'desc',
                    'per_page': min(self.config.max_github_results, 30)
                }

                async with session.get(repos_url, headers=headers, params=repos_params) as response:
                    if response.status == 200:
                        repos_data = await response.json()
                        return self._process_github_repos(repos_data.get('items', []), query)
                    else:
                        logger.error(f"GitHub API错误: {response.status}")
                        return []

        except Exception as e:
            logger.error(f"GitHub搜索失败: {str(e)}")
            return []

    def _build_github_query(self, query: str) -> str:
        """构建GitHub搜索查询"""
        # 添加时间限制
        time_limit = (datetime.now() - timedelta(days=self.config.time_range_days)).strftime('%Y-%m-%d')

        # 基础查询
        github_query = f"{query} pushed:>={time_limit}"

        # 添加语言过滤 (如果有特定研究领域的偏好)
        if self.research_agent.research_domain.lower() in ['人工智能', 'artificial intelligence', 'machine learning']:
            github_query += " language:python language:jupyter"

        return github_query

    def _process_github_repos(self, repos: List[Dict], query: str) -> List[SearchResult]:
        """处理GitHub仓库结果"""
        results = []

        for repo in repos[:self.config.max_github_results]:
            try:
                # 计算相关性分数
                relevance_score = self._calculate_github_relevance(repo, query)

                result = SearchResult(
                    title=repo['full_name'],
                    url=repo['html_url'],
                    description=repo['description'] or '无描述',
                    source='GitHub',
                    relevance_score=relevance_score,
                    timestamp=datetime.fromisoformat(repo['updated_at'].replace('Z', '+00:00')),
                    metadata={
                        'stars': repo['stargazers_count'],
                        'forks': repo['forks_count'],
                        'language': repo['language'],
                        'topics': repo.get('topics', []),
                        'license': repo.get('license', {}).get('name') if repo.get('license') else None,
                        'created_at': repo['created_at']
                    }
                )
                results.append(result)

            except Exception as e:
                logger.error(f"处理GitHub仓库失败: {str(e)}")
                continue

        return sorted(results, key=lambda x: x.relevance_score, reverse=True)

    def _calculate_github_relevance(self, repo: Dict, query: str) -> float:
        """计算GitHub仓库相关性分数"""
        score = 0.0
        query_lower = query.lower()

        # 标题匹配
        if repo['name'] and query_lower in repo['name'].lower():
            score += 3.0

        # 描述匹配
        if repo['description']:
            desc_lower = repo['description'].lower()
            query_words = query_lower.split()
            matches = sum(1 for word in query_words if word in desc_lower)
            score += matches * 1.5

        # 主题匹配
        topics = repo.get('topics', [])
        topic_matches = sum(1 for topic in topics if query_lower in topic.lower())
        score += topic_matches * 2.0

        # 星级加权
        stars = repo.get('stargazers_count', 0)
        if stars > 1000:
            score += 2.0
        elif stars > 100:
            score += 1.0

        # 最近更新
        try:
            updated_at = datetime.fromisoformat(repo['updated_at'].replace('Z', '+00:00'))
            days_since_update = (datetime.now(updated_at.tzinfo) - updated_at).days

            if days_since_update < 30:
                score += 1.5
            elif days_since_update < 90:
                score += 1.0
        except:
            pass

        return min(score, 10.0)  # 限制最高分数

    async def _search_arxiv(self, query: str) -> List[SearchResult]:
        """搜索arXiv学术论文"""
        try:
            # 构建arXiv查询
            arxiv_query = f'all:"{query}"'

            async with aiohttp.ClientSession() as session:
                params = {
                    'search_query': arxiv_query,
                    'start': 0,
                    'max_results': min(self.config.max_paper_results, 20),
                    'sortBy': 'relevance',
                    'sortOrder': 'descending'
                }

                async with session.get(self.arxiv_api_base, params=params) as response:
                    if response.status == 200:
                        xml_content = await response.text()
                        return self._parse_arxiv_xml(xml_content, query)
                    else:
                        logger.error(f"arXiv API错误: {response.status}")
                        return []

        except Exception as e:
            logger.error(f"arXiv搜索失败: {str(e)}")
            return []

    def _parse_arxiv_xml(self, xml_content: str, query: str) -> List[SearchResult]:
        """解析arXiv XML响应"""
        try:
            import xml.etree.ElementTree as ET

            root = ET.fromstring(xml_content)
            namespace = {'atom': 'http://www.w3.org/2005/Atom'}

            results = []

            for entry in root.findall('atom:entry', namespace):
                try:
                    title = entry.find('atom:title', namespace).text.strip()

                    # 提取ID和URL
                    arxiv_id = entry.find('atom:id', namespace).text.split('/')[-1]
                    url = f"https://arxiv.org/abs/{arxiv_id}"

                    # 摘要
                    summary = entry.find('atom:summary', namespace).text.strip()
                    summary = re.sub(r'\s+', ' ', summary)  # 清理空白字符

                    # 作者
                    authors = []
                    for author in entry.findall('atom:author', namespace):
                        name = author.find('atom:name', namespace).text
                        authors.append(name)

                    # 发布日期
                    published = entry.find('atom:published', namespace).text
                    timestamp = datetime.fromisoformat(published.replace('Z', '+00:00'))

                    # 计算相关性
                    relevance_score = self._calculate_arxiv_relevance(title, summary, query)

                    result = SearchResult(
                        title=title,
                        url=url,
                        description=summary[:300] + "..." if len(summary) > 300 else summary,
                        source='arXiv',
                        relevance_score=relevance_score,
                        timestamp=timestamp,
                        metadata={
                            'arxiv_id': arxiv_id,
                            'authors': authors[:5],  # 限制作者数量
                            'primary_category': entry.find('atom:primary_category', namespace).get('term') if entry.find('atom:primary_category', namespace) is not None else None,
                            'categories': [cat.get('term') for cat in entry.findall('atom:category', namespace)]
                        }
                    )
                    results.append(result)

                except Exception as e:
                    logger.error(f"处理arXiv条目失败: {str(e)}")
                    continue

            return results[:self.config.max_paper_results]

        except Exception as e:
            logger.error(f"解析arXiv XML失败: {str(e)}")
            return []

    def _calculate_arxiv_relevance(self, title: str, summary: str, query: str) -> float:
        """计算arXiv论文相关性分数"""
        score = 0.0
        query_lower = query.lower()

        # 标题匹配
        title_lower = title.lower()
        if query_lower in title_lower:
            score += 4.0

        query_words = query_lower.split()
        title_matches = sum(1 for word in query_words if word in title_lower)
        score += title_matches * 2.0

        # 摘要匹配
        summary_lower = summary.lower()
        summary_matches = sum(1 for word in query_words if word in summary_lower)
        score += summary_matches * 1.0

        # 时间加权 (较新的论文权重更高)
        # 这里可以添加基于时间的计算逻辑

        return min(score, 10.0)

    async def _search_tech_blogs(self, query: str) -> List[SearchResult]:
        """搜索技术博客"""
        try:
            # 使用AI进行技术博客搜索建议
            prompt = f"""请为以下研究主题推荐相关的技术博客和在线资源:

研究查询: {query}
研究领域: {self.research_agent.research_domain}

请推荐:
1. 相关的技术博客和网站
2. 在线教程和文档
3. 开源项目介绍
4. 技术会议和演讲资料

对于每个推荐，请提供:
- 标题/名称
- URL链接
- 简短描述
- 相关性评分(1-10)

以JSON格式返回推荐结果。"""

            # 使用research agent的chat方法
            response = self.research_agent.chat(prompt)

            # 尝试解析AI的推荐
            try:
                # 这里可以添加更复杂的JSON解析逻辑
                # 暂时返回简单的结果
                return [
                    SearchResult(
                        title="AI推荐的技术博客",
                        url="https://example.com",
                        description=response[:200] + "..." if len(response) > 200 else response,
                        source="AI推荐",
                        relevance_score=8.0,
                        timestamp=datetime.now(),
                        metadata={'type': 'ai_recommendation'}
                    )
                ]
            except:
                return []

        except Exception as e:
            logger.error(f"技术博客搜索失败: {str(e)}")
            return []

    def _generate_search_summary(self, results: Dict[str, Any], query: str) -> Dict[str, Any]:
        """生成检索摘要"""
        summary = {
            'query': query,
            'total_sources': 0,
            'source_breakdown': {},
            'top_results': [],
            'search_time': datetime.now().isoformat()
        }

        # 统计各来源结果数
        source_counts = {
            'GitHub': len(results['github_results']),
            'arXiv': len(results['paper_results']),
            'Blogs': len(results['blog_results'])
        }

        summary['source_breakdown'] = source_counts
        summary['total_sources'] = sum(source_counts.values())

        # 获取top结果
        all_results = []
        all_results.extend(results['github_results'])
        all_results.extend(results['paper_results'])
        all_results.extend(results['blog_results'])

        # 按相关性排序
        all_results.sort(key=lambda x: x.relevance_score, reverse=True)

        # 转换为字典格式
        summary['top_results'] = [
            {
                'title': result.title,
                'url': result.url,
                'source': result.source,
                'relevance_score': result.relevance_score
            }
            for result in all_results[:10]
        ]

        return summary

    async def get_search_history(self, limit: int = 10) -> List[Dict[str, Any]]:
        """获取搜索历史"""
        return self.search_history[-limit:] if self.search_history else []

    def clear_search_history(self):
        """清空搜索历史"""
        self.search_history.clear()
        logger.info("搜索历史已清空")