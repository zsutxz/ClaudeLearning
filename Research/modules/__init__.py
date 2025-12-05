"""
Research Agent 功能模块包

包含核心功能模块:
- literature_retriever: 文献检索模块
- data_processor: 数据处理模块
- report_generator: 报告生成模块
- quality_checker: 质量检查模块
- tool_manager: 工具管理模块
"""

__version__ = "1.0.0"

# 模块导入 (延迟导入，避免循环依赖)
def get_literature_retriever():
    """获取文献检索模块"""
    from .literature_retriever import LiteratureRetriever
    return LiteratureRetriever

def get_data_processor():
    """获取数据处理模块"""
    from .data_processor import DataProcessor
    return DataProcessor

def get_report_generator():
    """获取报告生成模块"""
    from .report_generator import ReportGenerator
    return ReportGenerator

def get_quality_checker():
    """获取质量检查模块"""
    from .quality_checker import QualityChecker
    return QualityChecker

def get_tool_manager():
    """获取工具管理模块"""
    from .tool_manager import ToolManager
    return ToolManager

__all__ = [
    'get_literature_retriever',
    'get_data_processor',
    'get_report_generator',
    'get_quality_checker',
    'get_tool_manager'
]