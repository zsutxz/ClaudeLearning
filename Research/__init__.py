"""
Research Agent - 专业技术调研代理包

基于Claude Agent SDK构建，专注于技术趋势分析、架构评估和工具选型

主要模块:
- research_agent: 核心ResearchAgent类
- modules: 功能模块集合
- mcp_servers: MCP服务器集成
- config: 配置管理
- templates: 报告模板

作者: Claude Code Assistant
版本: 1.0.0
"""

__version__ = "1.0.0"
__author__ = "Claude Code Assistant"

# 导入核心类
from .research_agent import ResearchAgent, ResearchConfig, ResearchResult, quick_research

# 导出主要接口
__all__ = [
    'ResearchAgent',
    'ResearchConfig',
    'ResearchResult',
    'quick_research'
]

# 包信息
PACKAGE_INFO = {
    'name': 'research-agent',
    'version': __version__,
    'description': '专业技术调研代理',
    'author': __author__,
    'dependencies': [
        'anthropic>=0.3.0',
        'openai>=1.0.0',
        'requests>=2.28.0'
    ]
}

def get_version():
    """获取包版本"""
    return __version__

def get_package_info():
    """获取包信息"""
    return PACKAGE_INFO