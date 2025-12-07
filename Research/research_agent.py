"""
Research Agent - 专业技术调研代理
基于Claude Agent SDK构建，专注于技术趋势分析、架构评估和工具选型

主要功能:
- 技术文献检索和分析
- 数据收集和处理
- 自动报告生成
- 多源数据集成

作者: Claude Code Assistant
版本: 1.0.0
"""

import os
import sys
import asyncio
import json
import logging
import re
from typing import Dict, List, Optional, Any
from datetime import datetime
from dataclasses import dataclass
from pathlib import Path

# 添加AgentSdkTest路径到系统路径
sys.path.append(os.path.join(os.path.dirname(__file__), '..', 'AgentSdkTest'))

# 导入基础代理类
try:
    from MultiAIAgent import UniversalTaskAgent
except ImportError:
    print("Error: 无法导入MultiAIAgent，请确保AgentSdkTest目录存在且包含MultiAIAgent.py")
    UniversalTaskAgent = object
    UniversalAIAgent = object

# 配置日志
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

@dataclass
class ResearchConfig:
    """研究配置类"""
    research_domain: str = "人工智能"
    max_sources: int = 20
    output_format: str = "markdown"
    include_github: bool = True
    include_papers: bool = True
    include_blogs: bool = True
    cache_results: bool = True
    save_to_file: bool = True  # 是否自动保存报告到文件
    reports_dir: str = "reports"  # 报告保存目录

@dataclass
class ResearchResult:
    """研究结果类"""
    query: str
    literature: Dict[str, Any]
    data: Dict[str, Any]
    analysis: Dict[str, Any]
    report: str
    metadata: Dict[str, Any]
    timestamp: datetime
    saved_file_path: Optional[str] = None  # 保存的文件路径

class ResearchAgent(UniversalTaskAgent):
    """
    专业技术调研代理

    继承UniversalTaskAgent，专注于技术调研任务，包括:
    - 文献检索和分析
    - 技术趋势分析
    - 数据收集和处理
    - 自动报告生成
    """

    def __init__(self, research_domain: str = "人工智能", **kwargs):
        """
        初始化Research Agent

        Args:
            research_domain: 研究领域
            **kwargs: 传递给父类的参数
        """
        super().__init__(
            task_description=f"专业{research_domain}领域技术调研助手",
            **kwargs
        )

        self.research_domain = research_domain
        self.config = ResearchConfig(research_domain=research_domain)

        # 添加专业的系统提示词
        system_prompt = f"""你是一个专业的{research_domain}技术调研助手。你的核心能力包括:

**文献检索分析**:
- 搜索和分析学术论文、技术文档
- 评估文献质量和相关性
- 提取关键信息和见解

**技术趋势分析**:
- 识别技术发展趋势
- 分析市场竞争格局
- 预测技术演进方向

**数据处理能力**:
- 收集和整理多源数据
- 数据清洗和标准化
- 统计分析和可视化

**报告生成**:
- 生成结构化研究报告
- 提供清晰的结论和建议
- 包含引用和参考资料

**工作原则**:
1. 确保信息的准确性和可靠性
2. 提供客观中立的分析
3. 引用权威来源和数据支撑
4. 保持报告的逻辑性和可读性
5. 及时澄清模糊的需求

当前研究领域: {research_domain}
"""

        self.add_system_prompt(system_prompt)

        # 初始化模块 (延迟导入，避免循环依赖)
        self.literature_retriever = None
        self.data_processor = None
        self.report_generator = None
        self.quality_checker = None

        # 确保报告目录存在
        self._ensure_reports_directory()

        logger.info(f"Research Agent 初始化完成 - 研究领域: {research_domain}")

    def _init_modules(self):
        """初始化功能模块"""
        try:
            from modules.literature_retriever import LiteratureRetriever
            from modules.data_processor import DataProcessor
            from modules.report_generator import ReportGenerator
            from modules.quality_checker import QualityChecker

            self.literature_retriever = LiteratureRetriever(self)
            self.data_processor = DataProcessor(self)
            self.report_generator = ReportGenerator(self)
            self.quality_checker = QualityChecker(self)

            logger.info("所有功能模块初始化成功")

        except ImportError as e:
            logger.warning(f"模块导入失败，将使用基础功能: {e}")
            self._init_basic_modules()

    def _init_basic_modules(self):
        """初始化基础功能模块"""
        # 提供基础的文献检索功能
        self.literature_retriever = self._basic_literature_search
        self.data_processor = self._basic_data_processing
        self.report_generator = self._basic_report_generation
        self.quality_checker = self._basic_quality_check

    async def conduct_research(self, query: str, **options) -> ResearchResult:
        """
        执行完整的技术调研流程

        Args:
            query: 研究查询
            **options: 研究选项

        Returns:
            ResearchResult: 完整的研究结果
        """
        logger.info(f"开始执行技术调研: {query}")

        # 过滤出 ResearchConfig 接受的参数
        config_kwargs = {k: v for k, v in options.items()
                        if k in ['research_domain', 'max_sources', 'output_format',
                                'include_github', 'include_papers', 'include_blogs', 'cache_results']}

        # 更新配置
        config = ResearchConfig(**config_kwargs)

        # 初始化模块
        if not self.literature_retriever:
            self._init_modules()

        try:
            # 1. 文献检索
            logger.info("步骤1: 执行文献检索...")
            literature_data = await self._search_literature(query, config)

            # 2. 数据收集处理
            logger.info("步骤2: 数据收集和处理...")
            processed_data = await self._process_data(query, config)

            # 3. 质量检查
            logger.info("步骤3: 执行质量检查...")
            quality_report = await self._check_quality({
                'literature': literature_data,
                'data': processed_data,
                'query': query
            })

            # 4. 生成分析报告
            logger.info("步骤4: 生成分析报告...")
            analysis_report = await self._generate_analysis({
                'literature': literature_data,
                'data': processed_data,
                'quality': quality_report
            })

            # 5. 生成最终报告
            logger.info("步骤5: 生成最终报告...")
            final_report = await self._generate_report({
                'query': query,
                'literature': literature_data,
                'data': processed_data,
                'analysis': analysis_report,
                'quality': quality_report,
                'config': config
            })

            # 构建结果
            result = ResearchResult(
                query=query,
                literature=literature_data,
                data=processed_data,
                analysis=analysis_report,
                report=final_report,
                metadata={
                    'research_domain': self.research_domain,
                    'config': config.__dict__,
                    'provider': self.provider,
                    'model': self.model,
                    'timestamp': datetime.now().isoformat()
                },
                timestamp=datetime.now()
            )

            logger.info(f"技术调研完成: {query}")
            return result

        except Exception as e:
            logger.error(f"技术调研执行失败: {str(e)}")
            # 返回错误结果
            return ResearchResult(
                query=query,
                literature={'error': str(e)},
                data={'error': str(e)},
                analysis={'error': str(e)},
                report=f"调研执行失败: {str(e)}",
                metadata={'error': str(e)},
                timestamp=datetime.now()
            )

    async def _search_literature(self, query: str, config: ResearchConfig) -> Dict[str, Any]:
        """执行文献检索"""
        try:
            if hasattr(self.literature_retriever, '__call__') and asyncio.iscoroutinefunction(self.literature_retriever):
                return await self.literature_retriever(query, config)
            elif hasattr(self.literature_retriever, '__call__'):
                return self.literature_retriever(query, config)
            else:
                # 使用AI进行基础文献搜索建议
                prompt = f"""请为"{query}"这个研究主题提供文献搜索建议。

研究领域: {self.research_domain}
最大文献数量: {config.max_sources}

请提供:
1. 推荐的搜索关键词
2. 相关的学术数据库和资源
3. 重要的研究方向和子主题
4. 可能的权威作者和机构

以结构化的方式返回建议。"""

                response = self.chat(prompt)
                return {
                    'search_suggestions': response,
                    'sources': ['AI建议的文献来源'],
                    'status': 'ai_suggestions'
                }
        except Exception as e:
            logger.error(f"文献检索失败: {str(e)}")
            return {'error': str(e), 'status': 'failed'}

    async def _process_data(self, query: str, config: ResearchConfig) -> Dict[str, Any]:
        """处理研究数据"""
        try:
            if hasattr(self.data_processor, '__call__') and asyncio.iscoroutinefunction(self.data_processor):
                return await self.data_processor(query, config)
            elif hasattr(self.data_processor, '__call__'):
                return self.data_processor(query, config)
            else:
                # 使用AI进行基础数据处理建议
                prompt = f"""对于"{query}"这个研究主题，请提供数据收集和处理建议。

研究领域: {self.research_domain}

请提供:
1. 推荐的数据源和API
2. 数据收集的方法和工具
3. 数据清洗和预处理步骤
4. 数据分析的技术和算法

请以实用的方式返回具体建议。"""

                response = self.chat(prompt)
                return {
                    'processing_suggestions': response,
                    'data_sources': ['AI建议的数据源'],
                    'status': 'ai_suggestions'
                }
        except Exception as e:
            logger.error(f"数据处理失败: {str(e)}")
            return {'error': str(e), 'status': 'failed'}

    async def _check_quality(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """执行质量检查"""
        try:
            if hasattr(self.quality_checker, '__call__') and asyncio.iscoroutinefunction(self.quality_checker):
                return await self.quality_checker(research_data)
            elif hasattr(self.quality_checker, '__call__'):
                return self.quality_checker(research_data)
            else:
                # 使用AI进行基础质量评估
                prompt = f"""请对以下研究数据进行质量评估:

查询: {research_data.get('query', 'N/A')}
文献数据: {len(str(research_data.get('literature', {})))} 字符
数据状态: {research_data.get('data', {}).get('status', 'unknown')}

请从以下维度进行评估:
1. 数据完整性和相关性
2. 来源可信度和权威性
3. 分析深度和洞察价值
4. 改进建议和后续步骤

返回具体的质量评估报告。"""

                response = self.chat(prompt)
                return {
                    'quality_assessment': response,
                    'overall_score': 8.0,  # 默认评分
                    'recommendations': ['建议添加更多数据源'],
                    'status': 'ai_assessment'
                }
        except Exception as e:
            logger.error(f"质量检查失败: {str(e)}")
            return {'error': str(e), 'status': 'failed'}

    async def _generate_analysis(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """生成分析报告"""
        try:
            prompt = f"""基于以下研究数据，请生成深入的技术分析:

文献数据: {str(research_data.get('literature', {}))[:500]}...
数据状态: {research_data.get('data', {}).get('status', 'unknown')}
质量评估: {research_data.get('quality', {}).get('status', 'unknown')}

研究领域: {self.research_domain}

请提供:
1. 技术趋势分析
2. 关键发现和洞察
3. 竞争格局分析
4. 发展机会和挑战
5. 结论和建议

以专业、客观的方式返回分析结果。"""

            response = self.chat(prompt)
            return {
                'analysis_report': response,
                'key_findings': ['基于AI生成的关键发现'],
                'trends': ['识别的技术趋势'],
                'status': 'completed'
            }
        except Exception as e:
            logger.error(f"分析生成失败: {str(e)}")
            return {'error': str(e), 'status': 'failed'}

    async def _generate_report(self, research_data: Dict[str, Any]) -> str:
        """生成最终报告"""
        try:
            query = research_data.get('query', 'Unknown Research')
            config = research_data.get('config')

            report_format = config.output_format if config else 'markdown'

            if report_format == 'markdown':
                return self._generate_markdown_report(research_data)
            else:
                return self._generate_text_report(research_data)

        except Exception as e:
            logger.error(f"报告生成失败: {str(e)}")
            return f"报告生成失败: {str(e)}"

    def _generate_markdown_report(self, research_data: Dict[str, Any]) -> str:
        """生成Markdown格式报告"""
        query = research_data.get('query', 'Unknown Research')
        timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')

        report = f"""# {query} - 技术调研报告

## 研究概述

**研究领域**: {self.research_domain}
**调研时间**: {timestamp}
**AI模型**: {self.provider} - {self.model}

---

## 执行摘要

本报告基于多源数据收集和分析，提供了关于"{query}"的全面技术调研结果。

---

## 文献检索结果

{research_data.get('literature', {}).get('search_suggestions', '暂无文献检索结果')}

---

## 数据收集分析

{research_data.get('data', {}).get('processing_suggestions', '暂无数据分析结果')}

---

## 质量评估

{research_data.get('quality', {}).get('quality_assessment', '暂无质量评估结果')}

---

## 技术分析

{research_data.get('analysis', {}).get('analysis_report', '暂无分析结果')}

---

## 关键发现

- 基于AI辅助生成的调研结果
- 多源数据收集和处理
- 自动化质量评估和分析

---

## 建议和后续步骤

1. 扩展数据源和检索范围
2. 深入分析特定技术细节
3. 跟踪最新发展趋势
4. 进行专家验证和反馈

---

## 报告元数据

- 生成工具: Research Agent v1.0.0
- 数据来源: AI辅助生成
- 报告格式: Markdown
- 更新时间: {timestamp}

---

*本报告由Research Agent自动生成，建议结合人工专家验证进行使用。*
"""
        return report

    def _generate_text_report(self, research_data: Dict[str, Any]) -> str:
        """生成纯文本格式报告"""
        query = research_data.get('query', 'Unknown Research')
        timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')

        report = f"""{query} - 技术调研报告

生成时间: {timestamp}
研究领域: {self.research_domain}

=== 执行摘要 ===
本报告基于AI辅助的技术调研，提供了关于{query}的分析结果。

=== 主要发现 ===
{research_data.get('analysis', {}).get('analysis_report', '分析结果生成中...')}

=== 建议 ===
1. 建议扩展更多数据源
2. 进行深度技术分析
3. 结合专家意见验证

报告生成完成。
"""
        return report

# 基础功能实现 (当模块不可用时的回退方案)
    def _basic_literature_search(self, query: str, config: ResearchConfig) -> Dict[str, Any]:
        """基础文献搜索"""
        return {'status': 'basic_search', 'query': query}

    def _basic_data_processing(self, query: str, config: ResearchConfig) -> Dict[str, Any]:
        """基础数据处理"""
        return {'status': 'basic_processing', 'query': query}

    def _basic_report_generation(self, research_data: Dict[str, Any]) -> str:
        """基础报告生成"""
        return "基础报告生成完成"

    def _basic_quality_check(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """基础质量检查"""
        return {'status': 'basic_check', 'score': 7.5}

# 便捷函数
async def quick_research(query: str, research_domain: str = "人工智能", **kwargs) -> ResearchResult:
    """
    快速技术调研函数

    Args:
        query: 研究查询
        research_domain: 研究领域
        **kwargs: 其他参数（仅用于conduct_research）

    Returns:
        ResearchResult: 调研结果
    """
    # 过滤掉ResearchAgent构造函数不接受的参数
    agent_kwargs = {k: v for k, v in kwargs.items() if k in ['provider', 'model', 'task_description']}
    agent = ResearchAgent(research_domain=research_domain, **agent_kwargs)
    return await agent.conduct_research(query, **kwargs)

if __name__ == "__main__":
    # 测试代码
    async def test_research_agent():
        """测试Research Agent"""
        print("=== Research Agent ===")

        # 创建研究代理
        agent = ResearchAgent(
            research_domain="人工智能",
            provider="claude",  # 使用模拟模式进行测试
            api_key=os.getenv('ANTHROPIC_API_KEY'),
            base_url=os.getenv('ANTHROPIC_BASE_URL', 'https://open.bigmodel.cn/api/anthropic')
        )

        # 执行研究
        result = await agent.conduct_research(
            query="使用llm rag 进行客服系统构建的最新方法",
            max_sources=10,
            output_format="markdown"
        )

        print(f"研究查询: {result.query}")
        print(f"生成时间: {result.timestamp}")
        print("\n=== 生成的报告 ===")
        print(result.report)

        print("\n=== 测试完成 ===")

    # 运行测试
    asyncio.run(test_research_agent())