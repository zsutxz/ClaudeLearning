"""
文献检索模块

提供多种文献源的检索功能:
- GitHub仓库和代码分析
- 学术论文检索 (arXiv, IEEE等)
- 技术博客和文章
- 官方文档和API参考
"""

from .literature_retriever import LiteratureRetriever

__all__ = ['LiteratureRetriever']