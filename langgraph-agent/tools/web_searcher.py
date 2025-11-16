"""
网络搜索工具
"""
import asyncio
import aiohttp
import json
from typing import List, Dict, Any
from bs4 import BeautifulSoup
from urllib.parse import urlencode
import feedparser
import arxiv

from config.settings import settings


class WebSearcher:
    """网络搜索工具类"""

    def __init__(self):
        self.session = None
        self.headers = {
            "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
        }

    async def __aenter__(self):
        self.session = aiohttp.ClientSession(
            headers=self.headers,
            timeout=aiohttp.ClientTimeout(total=settings.TIMEOUT)
        )
        return self

    async def __aexit__(self, exc_type, exc_val, exc_tb):
        if self.session:
            await self.session.close()

    async def search_google(self, query: str, num_results: int = 5) -> List[Dict[str, Any]]:
        """使用Google搜索"""
        if not settings.SERPER_API_KEY:
            return await self._fallback_search(query, num_results)

        url = "https://google.serper.dev/search"
        payload = json.dumps({
            "q": query,
            "num": num_results,
            "hl": "zh-cn"
        })

        headers = {
            "X-API-KEY": settings.SERPER_API_KEY,
            "Content-Type": "application/json"
        }

        try:
            async with self.session.post(url, headers=headers, data=payload) as response:
                result = await response.json()
                return self._parse_google_results(result)
        except Exception as e:
            print(f"Google搜索失败: {e}")
            return await self._fallback_search(query, num_results)

    def _parse_google_results(self, data: Dict) -> List[Dict[str, Any]]:
        """解析Google搜索结果"""
        results = []
        for item in data.get("organic", []):
            results.append({
                "title": item.get("title", ""),
                "link": item.get("link", ""),
                "snippet": item.get("snippet", ""),
                "source": "google"
            })
        return results

    async def _fallback_search(self, query: str, num_results: int) -> List[Dict[str, Any]]:
        """备用搜索方法 - 返回模拟数据用于测试"""
        print(f"[INFO] 使用模拟搜索数据 (未配置 SERPER_API_KEY)")

        # 根据查询关键词返回一些模拟结果
        mock_data = {
            "python": [
                {
                    "title": "Python 官方文档",
                    "link": "https://docs.python.org/3/",
                    "snippet": "Python官方文档，包含完整的语言参考和标准库文档",
                    "source": "mock"
                },
                {
                    "title": "Python Tutorial - W3Schools",
                    "link": "https://www.w3schools.com/python/",
                    "snippet": "Python基础教程，适合初学者入门学习",
                    "source": "mock"
                },
                {
                    "title": "Real Python",
                    "link": "https://realpython.com/",
                    "snippet": "高质量的Python教程和实战项目指南",
                    "source": "mock"
                }
            ],
            "javascript": [
                {
                    "title": "JavaScript MDN 文档",
                    "link": "https://developer.mozilla.org/zh-CN/docs/Web/JavaScript",
                    "snippet": "Mozilla开发者网络提供的JavaScript完整文档",
                    "source": "mock"
                },
                {
                    "title": "JavaScript.info",
                    "link": "https://javascript.info/",
                    "snippet": "现代JavaScript教程，从基础到高级",
                    "source": "mock"
                }
            ]
        }

        # 查找匹配的模拟数据
        query_lower = query.lower()
        for key, data in mock_data.items():
            if key in query_lower:
                return data[:num_results]

        # 如果没有匹配的数据，返回通用的学习资源
        return [
            {
                "title": f"{query} 学习资源汇总",
                "link": "https://example.com/learning",
                "snippet": f"关于{query}的学习资源和教程汇总",
                "source": "mock"
            }
        ]

    async def fetch_page_content(self, url: str) -> str:
        """获取网页内容"""
        try:
            async with self.session.get(url) as response:
                if response.status == 200:
                    html = await response.text()
                    soup = BeautifulSoup(html, 'html.parser')
                    # 移除脚本和样式
                    for script in soup(["script", "style"]):
                        script.decompose()
                    return soup.get_text(strip=True)
        except Exception as e:
            print(f"获取页面内容失败 {url}: {e}")
        return ""

    async def search_arxiv(self, query: str, num_results: int = 5) -> List[Dict[str, Any]]:
        """搜索学术论文"""
        try:
            # 使用asyncio.wait_for添加超时保护
            def _search_arxiv():
                search = arxiv.Search(
                    query=query,
                    max_results=num_results,
                    sort_by=arxiv.SortCriterion.SubmittedDate
                )

                results = []
                for paper in search.results():
                    results.append({
                        "title": paper.title,
                        "authors": [author.name for author in paper.authors],
                        "summary": paper.summary[:200] + "..." if len(paper.summary) > 200 else paper.summary,
                        "published": paper.published.date().isoformat(),
                        "pdf_url": paper.pdf_url,
                        "source": "arxiv"
                    })
                return results

            # 设置10秒超时
            results = await asyncio.wait_for(
                asyncio.get_event_loop().run_in_executor(None, _search_arxiv),
                timeout=10.0
            )
            return results
        except asyncio.TimeoutError:
            print(f"ArXiv搜索超时")
            return []
        except Exception as e:
            print(f"ArXiv搜索失败: {e}")
            return []

    async def search_tech_blogs(self, query: str) -> List[Dict[str, Any]]:
        """搜索技术博客"""
        blog_feeds = [
            "https://feeds.feedburner.com/oreilly/radar",
            "https://martinfowler.com/feed.atom",
            "https://danielabaron.me/feed/",
            # 可以添加更多RSS源
        ]

        async def _fetch_feed(feed_url):
            try:
                def _parse_feed():
                    feed = feedparser.parse(feed_url)
                    results = []
                    for entry in feed.entries:
                        if query.lower() in entry.title.lower() or query.lower() in entry.summary.lower():
                            results.append({
                                "title": entry.title,
                                "link": entry.link,
                                "summary": entry.summary[:200] + "..." if len(entry.summary) > 200 else entry.summary,
                                "published": entry.get("published", ""),
                                "source": "blog"
                            })
                    return results

                # 设置5秒超时
                return await asyncio.wait_for(
                    asyncio.get_event_loop().run_in_executor(None, _parse_feed),
                    timeout=5.0
                )
            except asyncio.TimeoutError:
                print(f"RSS源超时 {feed_url}")
                return []
            except Exception as e:
                print(f"RSS源解析失败 {feed_url}: {e}")
                return []

        # 并行获取所有RSS源
        tasks = [_fetch_feed(feed_url) for feed_url in blog_feeds]
        feed_results = await asyncio.gather(*tasks, return_exceptions=True)

        results = []
        for feed_result in feed_results:
            if isinstance(feed_result, list):
                results.extend(feed_result)

        return results[:5]  # 限制结果数量

    async def comprehensive_search(self, query: str) -> Dict[str, Any]:
        """综合搜索"""
        tasks = [
            self.search_google(query),
            self.search_arxiv(query),
            self.search_tech_blogs(query)
        ]

        google_results, arxiv_results, blog_results = await asyncio.gather(*tasks)

        return {
            "query": query,
            "google_results": google_results,
            "arxiv_results": arxiv_results,
            "blog_results": blog_results,
            "total_results": len(google_results) + len(arxiv_results) + len(blog_results)
        }