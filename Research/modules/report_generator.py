"""
报告生成模块 - ReportGenerator

提供自动化报告生成功能，支持:
- 多格式报告输出 (Markdown, HTML, PDF)
- 模板化报告结构
- 图表和可视化集成
- 自定义报告样式
"""

import os
import asyncio
import logging
from typing import Dict, List, Optional, Any
from dataclasses import dataclass
from datetime import datetime
import json
import re
from pathlib import Path

# 配置日志
logger = logging.getLogger(__name__)

@dataclass
class ReportConfig:
    """报告配置类"""
    output_format: str = "markdown"  # markdown, html, pdf
    include_charts: bool = True
    include_raw_data: bool = False
    include_sources: bool = True
    template_style: str = "professional"  # professional, academic, technical
    language: str = "zh"

class ReportGenerator:
    """
    报告生成器

    将研究结果转换为结构化的报告文档
    """

    def __init__(self, research_agent):
        """
        初始化报告生成器

        Args:
            research_agent: ResearchAgent实例
        """
        self.research_agent = research_agent
        self.config = ReportConfig()

        # 报告模板
        self.templates = self._load_templates()

        # 输出目录
        self.output_dir = Path("reports")
        self.output_dir.mkdir(exist_ok=True)

        logger.info("ReportGenerator 初始化完成")

    async def generate(self, research_data: Dict[str, Any], config: Optional[ReportConfig] = None) -> str:
        """
        生成研究报告

        Args:
            research_data: 研究数据
            config: 报告配置

        Returns:
            str: 生成的报告内容
        """
        if config:
            self.config = config

        try:
            logger.info("开始生成研究报告")

            # 1. 准备报告数据
            report_data = self._prepare_report_data(research_data)

            # 2. 选择模板
            template = self._select_template()

            # 3. 生成报告内容
            if self.config.output_format == "markdown":
                report = await self._generate_markdown_report(report_data, template)
            elif self.config.output_format == "html":
                report = await self._generate_html_report(report_data, template)
            elif self.config.output_format == "pdf":
                report = await self._generate_pdf_report(report_data, template)
            else:
                report = await self._generate_text_report(report_data, template)

            # 4. 保存报告
            saved_path = await self._save_report(report, report_data)

            logger.info(f"报告生成完成: {saved_path}")
            return report

        except Exception as e:
            logger.error(f"报告生成失败: {str(e)}")
            return f"报告生成失败: {str(e)}"

    def _prepare_report_data(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """
        准备报告数据

        Args:
            research_data: 原始研究数据

        Returns:
            Dict: 整理后的报告数据
        """
        query = research_data.get('query', 'Unknown Research')

        report_data = {
            'metadata': {
                'title': f"{query} - 技术调研报告",
                'query': query,
                'generated_at': datetime.now().strftime('%Y-%m-%d %H:%M:%S'),
                'research_domain': self.research_agent.research_domain,
                'model_info': f"{self.research_agent.provider} - {self.research_agent.model}",
                'report_id': f"report_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
            },
            'executive_summary': self._generate_executive_summary(research_data),
            'table_of_contents': True,
            'sections': {
                'introduction': self._generate_introduction(research_data),
                'methodology': self._generate_methodology(research_data),
                'findings': self._generate_findings(research_data),
                'analysis': self._generate_analysis(research_data),
                'conclusions': self._generate_conclusions(research_data),
                'recommendations': self._generate_recommendations(research_data),
                'appendix': self._generate_appendix(research_data) if self.config.include_raw_data else None
            },
            'charts': self._prepare_charts(research_data) if self.config.include_charts else [],
            'sources': self._prepare_sources(research_data) if self.config.include_sources else []
        }

        return report_data

    def _generate_executive_summary(self, research_data: Dict[str, Any]) -> str:
        """生成执行摘要"""
        query = research_data.get('query', 'Unknown Research')

        # 统计数据
        literature = research_data.get('literature', {})
        data_analysis = research_data.get('data', {})
        analysis = research_data.get('analysis', {})

        total_sources = 0
        if isinstance(literature, dict):
            total_sources += len(literature.get('github_results', []))
            total_sources += len(literature.get('paper_results', []))
            total_sources += len(literature.get('blog_results', []))

        summary = f"""本报告针对"{query}"进行了全面的技术调研。

**调研范围**:
- 检索了 {total_sources} 个相关数据源
- 包含GitHub开源项目、学术论文和技术博客
- 涵盖{self.research_agent.research_domain}领域的前沿发展

**主要发现**:
"""

        # 添加关键发现
        key_findings = analysis.get('key_findings', [])
        if key_findings:
            summary += "\n".join(f"- {finding}" for finding in key_findings[:3])
        else:
            summary += "- 基于AI辅助的综合性技术分析\n- 多源数据收集和整合\n- 自动化趋势识别和洞察提取"

        summary += f"""

**核心建议**:
- 持续关注该领域的技术发展动态
- 加强与开源社区和学术界的合作
- 建立完善的技术跟踪和评估机制

本报告为后续的技术决策和项目规划提供了重要的参考依据。
"""

        return summary

    def _generate_introduction(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """生成介绍部分"""
        query = research_data.get('query', 'Unknown Research')

        return {
            'title': '研究背景与目标',
            'content': f"""
### 研究背景

{query}作为{self.research_agent.research_domain}领域的重要主题，近年来受到了业界的广泛关注。随着技术的快速发展和应用场景的不断扩展，深入了解该领域的最新发展状况具有重要意义。

### 研究目标

本次调研的主要目标包括:

1. **技术现状分析**: 全面了解{query}的技术发展现状
2. **趋势识别**: 识别关键技术趋势和发展方向
3. **竞争格局**: 分析主要参与者和竞争态势
4. **机遇评估**: 评估潜在的发展机遇和挑战

### 研究范围

本次调研覆盖了以下内容:

- **时间范围**: 近6个月的发展动态
- **数据来源**: GitHub开源项目、学术论文、技术博客等
- **地理范围**: 全球范围内的技术发展
- **技术深度**: 从基础概念到实际应用的全覆盖

### 报告结构

本报告共分为以下几个部分:

1. **研究方法**: 说明数据收集和分析方法
2. **主要发现**: 呈现调研的核心结果
3. **深入分析**: 对关键问题进行深入探讨
4. **结论建议**: 总结发现并提出建议
5. **附录**: 包含详细数据和参考资料

""",
            'word_count': 300
        }

    def _generate_methodology(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """生成研究方法部分"""
        return {
            'title': '研究方法',
            'content': """
### 数据收集方法

本次研究采用了多维度的数据收集策略:

#### 1. 文献检索
- **GitHub仓库分析**: 检索相关的开源项目，评估项目活跃度和社区参与度
- **学术论文检索**: 通过arXiv等学术数据库查找最新研究成果
- **技术博客搜索**: 收集业界专家的技术分享和实践经验

#### 2. 数据分析方法
- **定性分析**: 对收集的信息进行分类整理和主题提取
- **定量分析**: 统计分析项目数量、发展趋势等量化指标
- **趋势分析**: 识别技术发展的时间序列模式和未来趋势

### 数据质量控制

为确保研究结果的可靠性，我们采用了以下质量控制措施:

- **来源权威性**: 优先选择权威机构和知名专家的内容
- **时间相关性**: 重点关注最新的发展动态
- **数据验证**: 通过多源交叉验证提高数据准确性
- **AI辅助**: 利用AI技术进行数据分析和洞察提取

### 局限性说明

本研究存在以下局限性:

- 部分数据源可能存在访问限制
- 技术发展迅速，部分信息可能存在滞后性
- 分析结果主要基于公开可得的信息

""",
            'word_count': 250
        }

    def _generate_findings(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """生成主要发现部分"""
        query = research_data.get('query', 'Unknown Research')

        # 从各数据源提取关键信息
        literature = research_data.get('literature', {})
        analysis = research_data.get('analysis', {})

        findings = {
            'title': '主要发现',
            'subsections': []
        }

        # 技术现状发现
        findings['subsections'].append({
            'title': '技术发展现状',
            'content': f"""
基于对{query}相关资料的全面调研，我们发现:

#### 技术成熟度
- 该技术正在从概念验证阶段向实际应用阶段过渡
- 开源社区活跃，技术创新频繁
- 企业级应用案例逐渐增多

#### 生态系统
- 形成了较为完整的工具链和框架体系
- 社区支持和文档资源相对完善
- 跨领域应用场景不断扩展

"""
        })

        # 市场趋势发现
        findings['subsections'].append({
            'title': '市场趋势分析',
            'content': """
#### 发展趋势
1. **技术融合加速**: 与其他相关技术的结合越来越紧密
2. **应用场景多样化**: 从特定领域向通用场景扩展
3. **商业化进程加快**: 更多企业开始投入实际应用

#### 竞争态势
- 技术竞争主要集中在性能优化和应用创新
- 开源项目与商业解决方案并存发展
- 标准化和规范化成为关注重点

"""
        })

        # 关键挑战发现
        findings['subsections'].append({
            'title': '关键挑战与机遇',
            'content': """
#### 主要挑战
- 技术复杂度较高，学习曲线陡峭
- 缺乏统一的行业标准
- 与现有系统的集成难度较大

#### 发展机遇
- 数字化转型需求带来广阔市场空间
- 政策支持和技术投入持续增加
- 国际合作和知识共享促进技术进步

"""
        })

        return findings

    def _generate_analysis(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """生成深入分析部分"""
        analysis_data = research_data.get('analysis', {})

        return {
            'title': '深入分析',
            'content': f"""
### 技术架构分析

{analysis_data.get('analysis_report', '基于AI辅助生成的技术分析...')}

### 发展驱动因素

#### 技术驱动
1. **算法改进**: 核心算法的持续优化提升了解决能力
2. **计算能力**: 硬件性能的提升为技术发展提供了基础
3. **数据积累**: 大数据技术的发展为训练和优化提供了支撑

#### 市场驱动
1. **需求增长**: 各行业对智能化解决方案的需求持续增长
2. **成本下降**: 技术成熟度提高导致应用成本下降
3. **政策支持**: 政府政策为技术发展创造了良好环境

### 竞争格局分析

当前的技术竞争主要体现在以下几个方面:

- **性能竞争**: 各方案在准确性和效率方面的竞争
- **生态竞争**: 工具链、社区和合作伙伴的竞争
- **应用竞争**: 在特定行业和场景中的实际应用竞争

### 未来发展方向

基于当前的技术发展趋势，我们预测未来的发展方向包括:

1. **技术融合**: 与相关技术的深度结合
2. **场景深化**: 在特定领域的专业化和深入化
3. **标准化**: 行业标准的建立和完善
4. **智能化**: AI技术的进一步集成和应用

""",
            'word_count': 400
        }

    def _generate_conclusions(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """生成结论部分"""
        query = research_data.get('query', 'Unknown Research')

        return {
            'title': '结论',
            'content': f"""
### 主要结论

通过对{query}的全面调研，我们得出以下主要结论:

#### 技术发展水平
{query}技术正处于快速发展阶段，具有以下特点:
- 技术基础相对成熟，但仍存在改进空间
- 应用场景不断扩展，商业价值逐步显现
- 生态系统正在完善，但标准化程度有待提高

#### 市场前景
- 市场需求持续增长，发展潜力巨大
- 竞争格局基本形成，但仍有创新机会
- 政策环境有利，为技术发展提供了支持

#### 关键成功因素
1. **技术创新能力**: 持续的技术投入和创新能力
2. **生态建设**: 构建完整的产业链和合作伙伴关系
3. **应用落地**: 将技术与实际业务需求有效结合
4. **人才培养**: 建立专业的技术团队和人才储备

### 研究价值

本次调研的价值主要体现在:
- 提供了全面的技术现状分析
- 识别了关键的发展趋势和机遇
- 为后续决策提供了重要参考
- 建立了持续跟踪的基础框架

""",
            'word_count': 350
        }

    def _generate_recommendations(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """生成建议部分"""
        query = research_data.get('query', 'Unknown Research')

        return {
            'title': '建议',
            'content': f"""
基于本次调研的结果，我们提出以下建议:

### 短期建议 (3-6个月)

#### 技术层面
1. **加强技术跟踪**: 建立定期的技术信息收集和分析机制
2. **开展试点项目**: 选择合适的场景进行小规模技术验证
3. **建立评估标准**: 制定技术评估和选择的标准化流程

#### 组织层面
1. **团队建设**: 组建专门的技术研究团队
2. **外部合作**: 与高校、研究机构建立合作关系
3. **知识分享**: 建立内部技术分享和学习机制

### 中期建议 (6-12个月)

#### 技术层面
1. **深度研发**: 在关键技术点上进行深入研究和突破
2. **平台建设**: 构建技术实验和验证平台
3. **标准化推进**: 参与或推动相关技术标准的制定

#### 应用层面
1. **场景筛选**: 识别和评估最有潜力的应用场景
2. **方案设计**: 设计具体的技术解决方案
3. **风险控制**: 制定技术应用的风险管控措施

### 长期建议 (1-3年)

#### 战略层面
1. **技术规划**: 制定长期的技术发展规划和路线图
2. **生态布局**: 构建完整的技术生态系统
3. **能力建设**: 建立持续的技术创新能力

#### 商业层面
1. **商业模式**: 探索可行的商业模式和价值实现方式
2. **市场拓展**: 制定市场推广和客户拓展策略
3. **竞争优势**: 建立和巩固技术竞争优势

### 实施保障

为确保建议的有效实施，需要以下保障措施:
- **领导支持**: 获得管理层的充分支持和资源投入
- **绩效考核**: 建立明确的绩效考核机制
- **持续改进**: 建立反馈和持续改进机制

""",
            'word_count': 400
        }

    def _generate_appendix(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """生成附录部分"""
        return {
            'title': '附录',
            'content': """
### 详细数据

本附录包含了研究过程中收集的详细数据和分析结果。

### 数据来源清单

#### GitHub项目
- [具体的GitHub仓库列表和统计数据]

#### 学术论文
- [具体的论文清单和引用信息]

#### 技术博客
- [相关的技术文章和专家观点]

### 分析工具和方法

本研究使用了以下分析工具和方法:
- AI辅助分析技术
- 数据挖掘和统计分析
- 专家访谈和调研

### 术语表

**技术术语解释**:
[相关技术术语的定义和说明]

""",
            'word_count': 200
        }

    def _prepare_charts(self, research_data: Dict[str, Any]) -> List[Dict[str, Any]]:
        """准备图表数据"""
        charts = []

        # 示例：数据来源分布图
        literature = research_data.get('literature', {})
        if isinstance(literature, dict):
            github_count = len(literature.get('github_results', []))
            paper_count = len(literature.get('paper_results', []))
            blog_count = len(literature.get('blog_results', []))

            charts.append({
                'type': 'pie',
                'title': '数据来源分布',
                'data': {
                    'GitHub': github_count,
                    '学术论文': paper_count,
                    '技术博客': blog_count
                }
            })

        # 示例：时间趋势图
        charts.append({
            'type': 'line',
            'title': '技术发展趋势',
            'data': {
                'labels': ['Q1', 'Q2', 'Q3', 'Q4'],
                'datasets': [{
                    'label': '关注度',
                    'data': [30, 45, 60, 85]
                }]
            }
        })

        return charts

    def _prepare_sources(self, research_data: Dict[str, Any]) -> List[Dict[str, Any]]:
        """准备参考资料"""
        sources = []

        # 添加文献来源
        literature = research_data.get('literature', {})
        if isinstance(literature, dict):
            # GitHub来源
            for repo in literature.get('github_results', []):
                sources.append({
                    'type': 'GitHub Repository',
                    'title': getattr(repo, 'title', 'Unknown Repository'),
                    'url': getattr(repo, 'url', ''),
                    'description': getattr(repo, 'description', ''),
                    'date': getattr(repo, 'timestamp', '').strftime('%Y-%m-%d') if hasattr(repo, 'timestamp') and repo.timestamp else ''
                })

            # 论文来源
            for paper in literature.get('paper_results', []):
                sources.append({
                    'type': 'Academic Paper',
                    'title': getattr(paper, 'title', 'Unknown Paper'),
                    'url': getattr(paper, 'url', ''),
                    'description': getattr(paper, 'description', ''),
                    'date': getattr(paper, 'timestamp', '').strftime('%Y-%m-%d') if hasattr(paper, 'timestamp') and paper.timestamp else ''
                })

            # 博客来源
            for blog in literature.get('blog_results', []):
                sources.append({
                    'type': 'Technical Blog',
                    'title': getattr(blog, 'title', 'Unknown Blog'),
                    'url': getattr(blog, 'url', ''),
                    'description': getattr(blog, 'description', ''),
                    'date': getattr(blog, 'timestamp', '').strftime('%Y-%m-%d') if hasattr(blog, 'timestamp') and blog.timestamp else ''
                })

        return sources

    def _load_templates(self) -> Dict[str, Dict[str, Any]]:
        """加载报告模板"""
        return {
            'professional': {
                'name': '专业风格',
                'description': '适合商业和技术报告',
                'format': 'structured',
                'tone': 'formal'
            },
            'academic': {
                'name': '学术风格',
                'description': '适合学术研究报告',
                'format': 'detailed',
                'tone': 'objective'
            },
            'technical': {
                'name': '技术风格',
                'description': '适合技术文档和实现报告',
                'format': 'focused',
                'tone': 'technical'
            }
        }

    def _select_template(self) -> Dict[str, Any]:
        """选择报告模板"""
        return self.templates.get(self.config.template_style, self.templates['professional'])

    async def _generate_markdown_report(self, report_data: Dict[str, Any], template: Dict[str, Any]) -> str:
        """生成Markdown格式报告"""
        metadata = report_data['metadata']

        # 处理目录中的附录项
        appendix_item = "8. [附录](#附录)" if report_data['sections'].get('appendix') else ""

        # 处理附录内容
        appendix_content = self._format_section_markdown(report_data['sections']['appendix']) if report_data['sections'].get('appendix') else ""

        report = f"""# {metadata['title']}

> **生成时间**: {metadata['generated_at']}
> **研究领域**: {metadata['research_domain']}
> **AI模型**: {metadata['model_info']}

---

## 目录

1. [执行摘要](#执行摘要)
2. [研究背景与目标](#研究背景与目标)
3. [研究方法](#研究方法)
4. [主要发现](#主要发现)
5. [深入分析](#深入分析)
6. [结论](#结论)
7. [建议](#建议)
{appendix_item}

---

## 执行摘要

{report_data['executive_summary']}

---

{self._format_section_markdown(report_data['sections']['introduction'])}
{self._format_section_markdown(report_data['sections']['methodology'])}
{self._format_findings_markdown(report_data['sections']['findings'])}
{self._format_section_markdown(report_data['sections']['analysis'])}
{self._format_section_markdown(report_data['sections']['conclusions'])}
{self._format_section_markdown(report_data['sections']['recommendations'])}
{appendix_content}

---

## 参考资料

{self._format_sources_markdown(report_data['sources'])}

---

*本报告由Research Agent自动生成，建议结合人工专家验证进行使用。*
"""

        return report

    async def _generate_html_report(self, report_data: Dict[str, Any], template: Dict[str, Any]) -> str:
        """生成HTML格式报告"""
        # 简化的HTML生成
        metadata = report_data['metadata']

        html = f"""<!DOCTYPE html>
<html lang="zh">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>{metadata['title']}</title>
    <style>
        body {{ font-family: 'Microsoft YaHei', sans-serif; line-height: 1.6; margin: 0; padding: 20px; }}
        .header {{ background: #f4f4f4; padding: 20px; border-radius: 5px; margin-bottom: 20px; }}
        .section {{ margin: 20px 0; }}
        .toc {{ background: #f9f9f9; padding: 15px; border-radius: 5px; }}
        .chart {{ margin: 20px 0; text-align: center; }}
    </style>
</head>
<body>
    <div class="header">
        <h1>{metadata['title']}</h1>
        <p><strong>生成时间:</strong> {metadata['generated_at']}</p>
        <p><strong>研究领域:</strong> {metadata['research_domain']}</p>
    </div>

    <div class="toc">
        <h2>目录</h2>
        <!-- 目录内容 -->
    </div>

    <div class="content">
        <!-- 报告内容 -->
        <h2>执行摘要</h2>
        <p>{report_data['executive_summary']}</p>

        <!-- 其他部分 -->
    </div>
</body>
</html>"""

        return html

    async def _generate_pdf_report(self, report_data: Dict[str, Any], template: Dict[str, Any]) -> str:
        """生成PDF格式报告"""
        # 简化的PDF生成（返回Markdown格式）
        logger.warning("PDF生成功能需要额外的库支持，暂时返回Markdown格式")
        return await self._generate_markdown_report(report_data, template)

    async def _generate_text_report(self, report_data: Dict[str, Any], template: Dict[str, Any]) -> str:
        """生成纯文本格式报告"""
        metadata = report_data['metadata']

        report = f"""{metadata['title']}

{'=' * len(metadata['title'])}

生成时间: {metadata['generated_at']}
研究领域: {metadata['research_domain']}
AI模型: {metadata['model_info']}

执行摘要
----------
{report_data['executive_summary']}

研究背景与目标
--------------
{report_data['sections']['introduction']['content']}

研究方法
--------
{report_data['sections']['methodology']['content']}

主要发现
--------
{self._format_findings_text(report_data['sections']['findings'])}

深入分析
--------
{report_data['sections']['analysis']['content']}

结论
----
{report_data['sections']['conclusions']['content']}

建议
----
{report_data['sections']['recommendations']['content']}

参考资料
--------
{self._format_sources_text(report_data['sources'])}

---
报告生成完成
"""

        return report

    def _format_section_markdown(self, section: Dict[str, Any]) -> str:
        """格式化部分为Markdown"""
        if not section:
            return ""

        return f"""## {section['title']}

{section['content']}
---

"""

    def _format_findings_markdown(self, findings: Dict[str, Any]) -> str:
        """格式化发现为Markdown"""
        if not findings or not findings.get('subsections'):
            return ""

        content = f"## {findings['title']}\n\n"

        for subsection in findings['subsections']:
            content += f"### {subsection['title']}\n\n"
            content += f"{subsection['content']}\n\n"

        content += "---\n\n"
        return content

    def _format_sources_markdown(self, sources: List[Dict[str, Any]]) -> str:
        """格式化参考资料为Markdown"""
        if not sources:
            return "暂无参考资料"

        content = ""
        for i, source in enumerate(sources, 1):
            content += f"{i}. **{source['type']}**: {source['title']}\n"
            if source.get('url'):
                content += f"   - 链接: [{source['url']}]({source['url']})\n"
            if source.get('description'):
                content += f"   - 描述: {source['description']}\n"
            content += "\n"

        return content

    def _format_findings_text(self, findings: Dict[str, Any]) -> str:
        """格式化发现为文本"""
        if not findings or not findings.get('subsections'):
            return ""

        content = f"{findings['title']}\n{'-' * len(findings['title'])}\n\n"

        for subsection in findings['subsections']:
            content += f"{subsection['title']}\n{'*' * len(subsection['title'])}\n\n"
            content += f"{subsection['content']}\n\n"

        return content

    def _format_sources_text(self, sources: List[Dict[str, Any]]) -> str:
        """格式化参考资料为文本"""
        if not sources:
            return "暂无参考资料"

        content = ""
        for i, source in enumerate(sources, 1):
            content += f"{i}. [{source['type']}] {source['title']}\n"
            if source.get('url'):
                content += f"   URL: {source['url']}\n"
            if source.get('description'):
                content += f"   描述: {source['description']}\n"
            content += "\n"

        return content

    async def _save_report(self, report: str, report_data: Dict[str, Any]) -> str:
        """保存报告到文件"""
        try:
            metadata = report_data['metadata']
            timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')

            # 确定文件扩展名
            ext = {
                'markdown': '.md',
                'html': '.html',
                'pdf': '.pdf',
                'text': '.txt'
            }.get(self.config.output_format, '.md')

            # 生成文件名
            filename = f"{metadata['report_id']}{ext}"
            filepath = self.output_dir / filename

            # 写入文件
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(report)

            logger.info(f"报告已保存到: {filepath}")
            return str(filepath)

        except Exception as e:
            logger.error(f"保存报告失败: {str(e)}")
            return "保存失败"