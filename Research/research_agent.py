"""
Research Agent - 专业技术调研代理
基于Claude Agent SDK，提供技术调研、文献检索和报告生成功能。
"""

import os
import sys
import asyncio
import logging
from typing import Dict, List, Optional, Any
from datetime import datetime
from dataclasses import dataclass
from pathlib import Path

# 添加AgentSdkTest路径
sys.path.append(os.path.join(os.path.dirname(__file__), '..', 'AgentSdkTest'))

try:
    from lib.multi_agent import UniversalTaskAgent
except ImportError as e:
    print(f"Error: 无法导入UniversalTaskAgent - {e}")
    raise

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

@dataclass
class ResearchConfig:
    """研究配置"""
    research_domain: str = "人工智能"
    max_sources: int = 20
    output_format: str = "markdown"
    include_github: bool = True
    include_papers: bool = True
    include_blogs: bool = True
    cache_results: bool = True
    save_to_file: bool = True
    reports_dir: str = "reports"

@dataclass
class ResearchResult:
    """研究结果"""
    query: str
    literature: Dict[str, Any]
    data: Dict[str, Any]
    analysis: Dict[str, Any]
    report: str
    metadata: Dict[str, Any]
    timestamp: datetime
    saved_file_path: Optional[str] = None

class ResearchAgent(UniversalTaskAgent):
    """专业技术调研代理 - 提供文献检索、数据分析和报告生成功能"""

    def __init__(self, research_domain: str = "人工智能", **kwargs):
        super().__init__(task_description=f"专业{research_domain}领域技术调研助手", **kwargs)
        self.research_domain = research_domain
        self.config = ResearchConfig(research_domain=research_domain)

        system_prompt = f"""你是{research_domain}技术调研助手，专注于:
- 文献检索和分析
- 技术趋势识别
- 数据收集处理
- 结构化报告生成

工作原则: 准确、客观、引用权威来源、保持逻辑性。
当前研究领域: {research_domain}"""

        self.add_system_prompt(system_prompt)

        # 初始化模块（延迟加载）
        self.literature_retriever = None
        self.data_processor = None
        self.report_generator = None
        self.quality_checker = None

        self._ensure_reports_directory()
        logger.info(f"Research Agent 初始化完成 - 研究领域: {research_domain}")

    def _ensure_reports_directory(self):
        """确保reports目录存在"""
        try:
            Path(self.config.reports_dir).mkdir(exist_ok=True)
        except Exception as e:
            logger.error(f"创建reports目录失败: {e}")

    def _generate_filename(self, timestamp: datetime) -> str:
        """生成报告文件名"""
        time_str = timestamp.strftime('%Y%m%d%H%M%S')
        ext = 'md' if self.config.output_format == 'markdown' else self.config.output_format
        return f"report{time_str}.{ext}"

    def _save_report_to_file(self, report: str, filename: str) -> Optional[str]:
        """保存报告到文件"""
        try:
            filepath = Path(self.config.reports_dir) / filename
            filepath.write_text(report, encoding='utf-8')
            logger.info(f"报告已保存: {filepath.absolute()}")
            return str(filepath.absolute())
        except Exception as e:
            logger.error(f"保存报告失败: {e}")
            return None

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
            logger.info("功能模块初始化成功")
        except ImportError as e:
            logger.warning(f"模块导入失败: {e}")
            self._init_basic_modules()

    def _init_basic_modules(self):
        """初始化基础功能模块（回退方案）"""
        self.literature_retriever = self._basic_literature_search
        self.data_processor = self._basic_data_processing
        self.report_generator = self._basic_report_generation
        self.quality_checker = self._basic_quality_check

    async def conduct_research(self, query: str, **options) -> ResearchResult:
        """执行完整的技术调研流程"""
        logger.info(f"开始执行技术调研: {query}")

        # 过滤有效参数并创建配置
        valid_keys = {'research_domain', 'max_sources', 'output_format',
                      'include_github', 'include_papers', 'include_blogs',
                      'cache_results', 'save_to_file', 'reports_dir'}
        config = ResearchConfig(**{k: v for k, v in options.items() if k in valid_keys})

        # 初始化模块
        if not self.literature_retriever:
            self._init_modules()

        try:
            # 执行调研步骤
            literature_data = await self._search_literature(query, config)
            processed_data = await self._process_data(query, config)
            quality_report = await self._check_quality({'literature': literature_data, 'data': processed_data, 'query': query})
            analysis_report = await self._generate_analysis({'literature': literature_data, 'data': processed_data, 'quality': quality_report})
            final_report = await self._generate_report({'query': query, 'literature': literature_data, 'data': processed_data, 'analysis': analysis_report, 'quality': quality_report, 'config': config})

            # 保存报告
            saved_file_path = None
            if config.save_to_file:
                filename = self._generate_filename(datetime.now())
                saved_file_path = self._save_report_to_file(final_report, filename)

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
                    'timestamp': datetime.now().isoformat(),
                    'saved_file_path': saved_file_path
                },
                timestamp=datetime.now(),
                saved_file_path=saved_file_path
            )

            logger.info(f"技术调研完成: {query}")
            return result

        except Exception as e:
            logger.error(f"技术调研失败: {e}")
            return ResearchResult(
                query=query, literature={'error': str(e)}, data={'error': str(e)},
                analysis={'error': str(e)}, report=f"调研失败: {e}",
                metadata={'error': str(e)}, timestamp=datetime.now()
            )

    async def _call_module(self, module, *args, **kwargs) -> Any:
        """统一调用模块（同步/异步）"""
        if asyncio.iscoroutinefunction(module):
            return await module(*args, **kwargs)
        elif callable(module):
            return module(*args, **kwargs)
        return None

    async def _search_literature(self, query: str, config: ResearchConfig) -> Dict[str, Any]:
        """执行文献检索"""
        try:
            result = await self._call_module(self.literature_retriever, query, config)
            if result:
                return result
            # AI辅助回退
            response = self.chat(f"为'{query}'提供文献搜索建议（领域：{self.research_domain}，最多{config.max_sources}条）")
            return {'search_suggestions': response, 'sources': ['AI建议'], 'status': 'ai_suggestions'}
        except Exception as e:
            logger.error(f"文献检索失败: {e}")
            return {'error': str(e), 'status': 'failed'}

    async def _process_data(self, query: str, config: ResearchConfig) -> Dict[str, Any]:
        """处理研究数据"""
        try:
            result = await self._call_module(self.data_processor, query, config)
            if result:
                return result
            response = self.chat(f"为'{query}'提供数据处理建议（领域：{self.research_domain}）")
            return {'processing_suggestions': response, 'data_sources': ['AI建议'], 'status': 'ai_suggestions'}
        except Exception as e:
            logger.error(f"数据处理失败: {e}")
            return {'error': str(e), 'status': 'failed'}

    async def _check_quality(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """执行质量检查"""
        try:
            result = await self._call_module(self.quality_checker, research_data)
            if result:
                return result
            query = research_data.get('query', 'N/A')
            response = self.chat(f"评估研究数据质量（查询：{query}，文献大小：{len(str(research_data.get('literature', {})))}字符）")
            return {'quality_assessment': response, 'overall_score': 8.0, 'recommendations': ['建议添加更多数据源'], 'status': 'ai_assessment'}
        except Exception as e:
            logger.error(f"质量检查失败: {e}")
            return {'error': str(e), 'status': 'failed'}

    async def _generate_analysis(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """生成分析报告"""
        try:
            lit = str(research_data.get('literature', {}))[:500]
            data_status = research_data.get('data', {}).get('status', 'unknown')
            quality_status = research_data.get('quality', {}).get('status', 'unknown')
            prompt = f"基于以下数据生成技术分析（领域：{self.research_domain}）：文献：{lit}...，数据状态：{data_status}，质量：{quality_status}"
            response = self.chat(prompt)
            return {'analysis_report': response, 'key_findings': ['基于AI生成'], 'trends': ['技术趋势'], 'status': 'completed'}
        except Exception as e:
            logger.error(f"分析生成失败: {e}")
            return {'error': str(e), 'status': 'failed'}

    async def _generate_report(self, research_data: Dict[str, Any]) -> str:
        """生成最终报告"""
        try:
            fmt = research_data.get('config', ResearchConfig()).output_format
            if fmt == 'markdown':
                return self._generate_markdown_report(research_data)
            return self._generate_text_report(research_data)
        except Exception as e:
            logger.error(f"报告生成失败: {e}")
            return f"报告生成失败: {e}"

    def _generate_markdown_report(self, data: Dict[str, Any]) -> str:
        """生成Markdown格式报告"""
        query = data.get('query', 'Unknown Research')
        ts = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        lit = data.get('literature', {}).get('search_suggestions', '暂无')
        proc = data.get('data', {}).get('processing_suggestions', '暂无')
        qual = data.get('quality', {}).get('quality_assessment', '暂无')
        ana = data.get('analysis', {}).get('analysis_report', '暂无')

        return f"""# {query} - 技术调研报告

## 研究概述
- **研究领域**: {self.research_domain}
- **调研时间**: {ts}
- **AI模型**: {self.provider} - {self.model}

## 执行摘要
本报告基于多源数据收集和分析，提供了关于"{query}"的技术调研结果。

## 文献检索结果
{lit}

## 数据收集分析
{proc}

## 质量评估
{qual}

## 技术分析
{ana}

## 关键发现
- 基于AI辅助生成的调研结果
- 多源数据收集和处理
- 自动化质量评估和分析

## 建议
1. 扩展数据源和检索范围
2. 深入分析特定技术细节
3. 跟踪最新发展趋势
4. 进行专家验证和反馈

---
*本报告由Research Agent自动生成，建议结合人工验证。*
"""

    def _generate_text_report(self, data: Dict[str, Any]) -> str:
        """生成纯文本格式报告"""
        query = data.get('query', 'Unknown Research')
        ts = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        ana = data.get('analysis', {}).get('analysis_report', '分析中...')

        return f"""{query} - 技术调研报告

生成时间: {ts}
研究领域: {self.research_domain}

=== 执行摘要 ===
本报告基于AI辅助的技术调研。

=== 主要发现 ===
{ana}

=== 建议 ===
1. 扩展更多数据源
2. 进行深度技术分析
3. 结合专家意见验证
"""

    # 基础功能（回退方案）
    def _basic_literature_search(self, query: str, config: ResearchConfig) -> Dict[str, Any]:
        return {'status': 'basic_search', 'query': query}

    def _basic_data_processing(self, query: str, config: ResearchConfig) -> Dict[str, Any]:
        return {'status': 'basic_processing', 'query': query}

    def _basic_report_generation(self, research_data: Dict[str, Any]) -> str:
        return "基础报告生成完成"

    def _basic_quality_check(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        return {'status': 'basic_check', 'score': 7.5}

# 便捷函数
async def quick_research(query: str, research_domain: str = "人工智能", **kwargs) -> ResearchResult:
    """快速技术调研函数"""
    agent_kwargs = {k: v for k, v in kwargs.items() if k in {'provider', 'model', 'task_description'}}
    agent = ResearchAgent(research_domain=research_domain, **agent_kwargs)
    return await agent.conduct_research(query, **kwargs)

if __name__ == "__main__":
    async def test_research_agent():
        """测试Research Agent"""
        print("=== Research Agent 测试 ===")
        agent = ResearchAgent(
            research_domain="人工智能",
            provider="claude",
            api_key=os.getenv('ANTHROPIC_API_KEY'),
            base_url=os.getenv('ANTHROPIC_BASE_URL', 'https://open.bigmodel.cn/api/anthropic')
        )

        result = await agent.conduct_research(
            query="bmad使用方法",
            max_sources=10,
            output_format="markdown",
            save_to_file=True
        )

        print(f"查询: {result.query}")
        print(f"时间: {result.timestamp}")
        print(f"保存: {result.saved_file_path}")
        print(f"\n{result.report[:500]}...")

        if result.saved_file_path and os.path.exists(result.saved_file_path):
            print(f"\n报告已保存: {result.saved_file_path} ({os.path.getsize(result.saved_file_path)} 字节)")

    asyncio.run(test_research_agent())