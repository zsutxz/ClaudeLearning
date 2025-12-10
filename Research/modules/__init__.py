"""
Research Agent 功能模块包

包含核心功能模块:
- literature_retriever: 文献检索模块
- data_processor: 数据处理模块
- report_generator: 报告生成模块
- quality_checker: 质量检查模块
"""

__version__ = "1.0.0"

# 直接导入类
from .literature_retriever.literature_retriever import LiteratureRetriever
from .data_processor import DataProcessor
from .report_generator import ReportGenerator
from .quality_checker import QualityChecker

# 延迟导入函数（避免循环依赖）
def get_literature_retriever():
    """获取文献检索模块"""
    return LiteratureRetriever

def get_data_processor():
    """获取数据处理模块"""
    return DataProcessor

def get_report_generator():
    """获取报告生成模块"""
    return ReportGenerator

def get_quality_checker():
    """获取质量检查模块"""
    return QualityChecker

__all__ = [
    'LiteratureRetriever',
    'DataProcessor',
    'ReportGenerator',
    'QualityChecker',
    'get_literature_retriever',
    'get_data_processor',
    'get_report_generator',
    'get_quality_checker'
]