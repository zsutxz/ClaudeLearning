"""
文献检索模块 - LiteratureRetriever
提供多源文献检索和分析功能。
"""

import os
import asyncio
import logging
from typing import Dict, List, Optional, Any
from dataclasses import dataclass
from datetime import datetime, timedelta

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
    """文献检索器 - 集成多种数据源"""

    def __init__(self, research_agent):
        self.research_agent = research_agent
        self.config = LiteratureConfig()
        self.github_token = os.getenv('GITHUB_TOKEN')
        self.github_api_base = 'https://api.github.com'
        self.arxiv_api_base = 'http://export.arxiv.org/api/query'
        self.search_history = []
        logger.info("LiteratureRetriever 初始化完成")

    async def search(self, query: str, config: Optional[LiteratureConfig] = None) -> Dict[str, Any]:
        """执行多源文献检索"""
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
            # 并行检索
            tasks = []
            if self.config.include_github:
                tasks.append(self._search_github(query))
            if self.config.include_papers:
                tasks.append(self._search_arxiv(query))
            if self.config.include_blogs:
                tasks.append(self._search_tech_blogs(query))

            if tasks:
                search_results = await asyncio.gather(*tasks, return_exceptions=True)

                idx = 0
                if self.config.include_github and idx < len(search_results):
                    if not isinstance(search_results[idx], Exception):
                        results['github_results'] = search_results[idx]
                    idx += 1

                if self.config.include_papers and idx < len(search_results):
                    if not isinstance(search_results[idx], Exception):
                        results['paper_results'] = search_results[idx]
                    idx += 1

                if self.config.include_blogs and idx < len(search_results):
                    if not isinstance(search_results[idx], Exception):
                        results['blog_results'] = search_results[idx]
                    idx += 1

            results['total_results'] = sum(len(results[k]) for k in ['github_results', 'paper_results', 'blog_results'])
            results['search_summary'] = self._generate_search_summary(results, query)

            self.search_history.append({
                'query': query, 'timestamp': datetime.now(),
                'total_results': results['total_results'], 'config': self.config.__dict__
            })

            logger.info(f"文献检索完成: {query} - 找到 {results['total_results']} 个结果")
            return results

        except Exception as e:
            logger.error(f"文献检索失败: {e}")
            return {'query': query, 'error': str(e), 'timestamp': datetime.now().isoformat(),
                   'github_results': [], 'paper_results': [], 'blog_results': [], 'total_results': 0}

    async def _search_github(self, query: str) -> List[SearchResult]:
        """搜索GitHub仓库"""
        try:
            if not self.github_token:
                logger.warning("未配置GitHub token")
                return []

            import aiohttp
            github_query = self._build_github_query(query)

            async with aiohttp.ClientSession() as session:
                headers = {'Authorization': f'token {self.github_token}', 'Accept': 'application/vnd.github.v3+json'}
                url = f"{self.github_api_base}/search/repositories"
                params = {'q': github_query, 'sort': 'stars', 'order': 'desc', 'per_page': min(self.config.max_github_results, 30)}

                async with session.get(url, headers=headers, params=params) as response:
                    if response.status == 200:
                        repos_data = await response.json()
                        return self._process_github_repos(repos_data.get('items', []), query)
                    else:
                        logger.error(f"GitHub API错误: {response.status}")
                        return []

        except Exception as e:
            logger.error(f"GitHub搜索失败: {e}")
            return []

    def _build_github_query(self, query: str) -> str:
        """构建GitHub搜索查询"""
        time_limit = (datetime.now() - timedelta(days=self.config.time_range_days)).strftime('%Y-%m-%d')
        github_query = f"{query} pushed:>={time_limit}"

        if self.research_agent.research_domain.lower() in ['人工智能', 'artificial intelligence', 'machine learning']:
            github_query += " language:python language:jupyter"

        return github_query

    def _process_github_repos(self, repos: List[Dict], query: str) -> List[SearchResult]:
        """处理GitHub仓库结果"""
        results = []

        for repo in repos[:self.config.max_github_results]:
            try:
                relevance_score = self._calculate_github_relevance(repo, query)
                results.append(SearchResult(
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
                        'license': repo.get('license', {}).get('name') if repo.get('license') else None
                    }
                ))
            except Exception as e:
                logger.error(f"处理GitHub仓库失败: {e}")
                continue

        return sorted(results, key=lambda x: x.relevance_score, reverse=True)

    def _calculate_github_relevance(self, repo: Dict, query: str) -> float:
        """计算GitHub仓库相关性分数"""
        score = 0.0
        query_lower = query.lower()

        if repo['name'] and query_lower in repo['name'].lower():
            score += 3.0

        if repo['description']:
            desc_lower = repo['description'].lower()
            query_words = query_lower.split()
            matches = sum(1 for word in query_words if word in desc_lower)
            score += matches * 1.5

        topics = repo.get('topics', [])
        topic_matches = sum(1 for topic in topics if query_lower in topic.lower())
        score += topic_matches * 2.0

        stars = repo.get('stargazers_count', 0)
        if stars > 1000:
            score += 2.0
        elif stars > 100:
            score += 1.0

        return min(score, 10.0)

    async def _search_arxiv(self, query: str) -> List[SearchResult]:
        """搜索arXiv学术论文"""
        try:
            import aiohttp
            arxiv_query = f'all:"{query}"'

            async with aiohttp.ClientSession() as session:
                params = {
                    'search_query': arxiv_query, 'start': 0,
                    'max_results': min(self.config.max_paper_results, 20),
                    'sortBy': 'relevance', 'sortOrder': 'descending'
                }

                async with session.get(self.arxiv_api_base, params=params) as response:
                    if response.status == 200:
                        xml_content = await response.text()
                        return self._parse_arxiv_xml(xml_content, query)
                    else:
                        logger.error(f"arXiv API错误: {response.status}")
                        return []

        except Exception as e:
            logger.error(f"arXiv搜索失败: {e}")
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
                    arxiv_id = entry.find('atom:id', namespace).text.split('/')[-1]
                    url = f"https://arxiv.org/abs/{arxiv_id}"
                    summary = entry.find('atom:summary', namespace).text.strip().replace('\s+', ' ')
                    published = entry.find('atom:published', namespace).text
                    timestamp = datetime.fromisoformat(published.replace('Z', '+00:00'))

                    authors = [author.find('atom:name', namespace).text for author in entry.findall('atom:author', namespace)][:5]

                    relevance_score = self._calculate_arxiv_relevance(title, summary, query)

                    results.append(SearchResult(
                        title=title,
                        url=url,
                        description=summary[:300] + "..." if len(summary) > 300 else summary,
                        source='arXiv',
                        relevance_score=relevance_score,
                        timestamp=timestamp,
                        metadata={'arxiv_id': arxiv_id, 'authors': authors}
                    ))
                except Exception as e:
                    logger.error(f"处理arXiv条目失败: {e}")
                    continue

            return results[:self.config.max_paper_results]

        except Exception as e:
            logger.error(f"解析arXiv XML失败: {e}")
            return []

    def _calculate_arxiv_relevance(self, title: str, summary: str, query: str) -> float:
        """计算arXiv论文相关性分数"""
        score = 0.0
        query_lower = query.lower()

        title_lower = title.lower()
        if query_lower in title_lower:
            score += 4.0

        query_words = query_lower.split()
        title_matches = sum(1 for word in query_words if word in title_lower)
        score += title_matches * 2.0

        summary_lower = summary.lower()
        summary_matches = sum(1 for word in query_words if word in summary_lower)
        score += summary_matches * 1.0

        return min(score, 10.0)

    async def _search_tech_blogs(self, query: str) -> List[SearchResult]:
        """搜索技术博客"""
        try:
            prompt = f"""为研究主题推荐相关技术博客和资源:

查询: {query}
领域: {self.research_agent.research_domain}

推荐: 技术博客、在线教程、开源项目、技术会议
格式: 标题、URL、描述、相关性(1-10)"""

            response = self.research_agent.chat(prompt)

            return [SearchResult(
                title="AI推荐的技术博客",
                url="https://example.com",
                description=response[:200] + "..." if len(response) > 200 else response,
                source="AI推荐",
                relevance_score=8.0,
                timestamp=datetime.now(),
                metadata={'type': 'ai_recommendation'}
            )]

        except Exception as e:
            logger.error(f"技术博客搜索失败: {e}")
            return []

    def _generate_search_summary(self, results: Dict[str, Any], query: str) -> Dict[str, Any]:
        """生成检索摘要"""
        source_counts = {
            'GitHub': len(results['github_results']),
            'arXiv': len(results['paper_results']),
            'Blogs': len(results['blog_results'])
        }

        all_results = results['github_results'] + results['paper_results'] + results['blog_results']
        all_results.sort(key=lambda x: x.relevance_score, reverse=True)

        return {
            'query': query,
            'total_sources': sum(source_counts.values()),
            'source_breakdown': source_counts,
            'top_results': [
                {'title': r.title, 'url': r.url, 'source': r.source, 'relevance_score': r.relevance_score}
                for r in all_results[:10]
            ],
            'search_time': datetime.now().isoformat()
        }

    async def get_search_history(self, limit: int = 10) -> List[Dict[str, Any]]:
        """获取搜索历史"""
        return self.search_history[-limit:] if self.search_history else []

    def clear_search_history(self):
        """清空搜索历史"""
        self.search_history.clear()
        logger.info("搜索历史已清空")
