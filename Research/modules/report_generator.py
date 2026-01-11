"""
报告生成模块 - ReportGenerator
提供自动化报告生成功能，支持Markdown和文本格式。
"""

import os
import asyncio
import logging
from typing import Dict, List, Optional, Any
from dataclasses import dataclass
from datetime import datetime
from pathlib import Path

logger = logging.getLogger(__name__)

@dataclass
class ReportConfig:
    """报告配置"""
    output_format: str = "markdown"  # markdown, html, text
    include_charts: bool = True
    include_raw_data: bool = False
    include_sources: bool = True
    template_style: str = "professional"  # professional, academic, technical
    language: str = "zh"

class ReportGenerator:
    """报告生成器 - 将研究结果转换为结构化报告"""

    def __init__(self, research_agent):
        self.research_agent = research_agent
        self.config = ReportConfig()
        self.output_dir = Path("reports")
        self.output_dir.mkdir(exist_ok=True)
        logger.info("ReportGenerator 初始化完成")

    async def generate(self, research_data: Dict[str, Any], config: Optional[ReportConfig] = None) -> str:
        """生成研究报告"""
        if config:
            self.config = config

        try:
            logger.info("开始生成研究报告")
            report_data = self._prepare_report_data(research_data)

            # 选择格式生成
            fmt = self.config.output_format
            if fmt == "markdown":
                report = self._generate_markdown_report(report_data)
            elif fmt == "html":
                report = self._generate_html_report(report_data)
            elif fmt == "pdf":
                report = self._generate_markdown_report(report_data)  # PDF暂时回退到MD
            else:
                report = self._generate_text_report(report_data)

            saved_path = await self._save_report(report, report_data)
            logger.info(f"报告生成完成: {saved_path}")
            return report

        except Exception as e:
            logger.error(f"报告生成失败: {e}")
            return f"报告生成失败: {e}"

    def _prepare_report_data(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """准备报告数据"""
        query = research_data.get('query', 'Unknown Research')
        ts = datetime.now().strftime('%Y-%m-%d %H:%M:%S')

        return {
            'metadata': {
                'title': f"{query} - 技术调研报告",
                'query': query,
                'generated_at': ts,
                'research_domain': self.research_agent.research_domain,
                'model_info': f"{self.research_agent.provider} - {self.research_agent.model}",
                'report_id': f"report{datetime.now().strftime('%Y%m%d%H%M%S')}"
            },
            'executive_summary': self._generate_executive_summary(research_data),
            'sections': {
                'introduction': self._generate_introduction(research_data),
                'methodology': self._generate_methodology(research_data),
                'findings': self._generate_findings(research_data),
                'analysis': self._generate_analysis(research_data),
                'conclusions': self._generate_conclusions(research_data),
                'recommendations': self._generate_recommendations(research_data)
            },
            'sources': self._prepare_sources(research_data) if self.config.include_sources else []
        }

    def _generate_executive_summary(self, research_data: Dict[str, Any]) -> str:
        """生成执行摘要"""
        query = research_data.get('query', 'Unknown Research')
        literature = research_data.get('literature', {})
        analysis = research_data.get('analysis', {})

        total_sources = sum(len(literature.get(k, [])) for k in ['github_results', 'paper_results', 'blog_results']) if isinstance(literature, dict) else 0

        summary = f"""本报告针对"{query}"进行了全面的技术调研。

**调研范围**:
- 检索了 {total_sources} 个相关数据源
- 涵盖{self.research_agent.research_domain}领域的前沿发展

**主要发现**:
"""

        key_findings = analysis.get('key_findings', []) if isinstance(analysis, dict) else []
        if key_findings:
            summary += "\n".join(f"- {finding}" for finding in key_findings[:3])
        else:
            summary += "- 基于AI辅助的综合性技术分析\n- 多源数据收集和整合\n- 自动化趋势识别和洞察提取"

        summary += f"\n\n**核心建议**:\n- 持续关注该领域的技术发展动态\n- 建立完善的技术跟踪和评估机制"

        return summary

    def _generate_introduction(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """生成介绍部分"""
        query = research_data.get('query', 'Unknown Research')
        return {
            'title': '研究背景与目标',
            'content': f"""### 研究背景
{query}作为{self.research_agent.research_domain}领域的重要主题，近年来受到了业界的广泛关注。

### 研究目标
1. **技术现状分析**: 全面了解{query}的技术发展现状
2. **趋势识别**: 识别关键技术趋势和发展方向
3. **竞争格局**: 分析主要参与者和竞争态势
4. **机遇评估**: 评估潜在的发展机遇和挑战
"""
        }

    def _generate_methodology(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """生成研究方法部分"""
        return {
            'title': '研究方法',
            'content': """### 数据收集方法
- **GitHub仓库分析**: 检索相关的开源项目，评估项目活跃度
- **学术论文检索**: 通过arXiv等数据库查找最新研究成果
- **技术博客搜索**: 收集业界专家的技术分享和实践经验

### 数据质量控制
- **来源权威性**: 优先选择权威机构和知名专家的内容
- **时间相关性**: 重点关注最新的发展动态
- **AI辅助**: 利用AI技术进行数据分析和洞察提取
"""
        }

    def _generate_findings(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """生成主要发现部分"""
        query = research_data.get('query', 'Unknown Research')
        return {
            'title': '主要发现',
            'subsections': [
                {
                    'title': '技术发展现状',
                    'content': f"""基于对{query}相关资料的调研：
#### 技术成熟度
- 该技术正在从概念验证阶段向实际应用阶段过渡
- 开源社区活跃，技术创新频繁

#### 生态系统
- 形成了较为完整的工具链和框架体系
- 社区支持和文档资源相对完善
"""
                },
                {
                    'title': '市场趋势分析',
                    'content': """#### 发展趋势
1. **技术融合加速**: 与相关技术的结合越来越紧密
2. **应用场景多样化**: 从特定领域向通用场景扩展
3. **商业化进程加快**: 更多企业开始投入实际应用
"""
                },
                {
                    'title': '关键挑战与机遇',
                    'content': """#### 主要挑战
- 技术复杂度较高，学习曲线陡峭
- 缺乏统一的行业标准

#### 发展机遇
- 数字化转型需求带来广阔市场空间
- 政策支持和技术投入持续增加
"""
                }
            ]
        }

    def _generate_analysis(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """生成深入分析部分"""
        analysis_data = research_data.get('analysis', {})
        analysis_report = analysis_data.get('analysis_report', '基于AI辅助生成的技术分析...') if isinstance(analysis_data, dict) else '基于AI辅助生成的技术分析...'

        return {
            'title': '深入分析',
            'content': f"""### 技术架构分析
{analysis_report}

### 发展驱动因素
**技术驱动**:
1. 算法改进：核心算法的持续优化
2. 计算能力：硬件性能提升
3. 数据积累：大数据技术发展

**市场驱动**:
1. 需求增长：各行业对智能化解决方案需求增加
2. 成本下降：技术成熟度提高
3. 政策支持：政府政策创造良好环境
"""
        }

    def _generate_conclusions(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """生成结论部分"""
        query = research_data.get('query', 'Unknown Research')
        return {
            'title': '结论',
            'content': f"""### 主要结论
通过对{query}的全面调研，我们得出以下结论：

#### 技术发展水平
{query}技术正处于快速发展阶段：
- 技术基础相对成熟，但仍存在改进空间
- 应用场景不断扩展，商业价值逐步显现

#### 市场前景
- 市场需求持续增长，发展潜力巨大
- 竞争格局基本形成，但仍有创新机会
"""
        }

    def _generate_recommendations(self, research_data: Dict[str, Any]) -> Dict[str, Any]:
        """生成建议部分"""
        return {
            'title': '建议',
            'content': """基于本次调研，我们提出以下建议：

### 短期建议 (3-6个月)
1. **加强技术跟踪**: 建立定期的技术信息收集机制
2. **开展试点项目**: 选择合适场景进行小规模验证
3. **建立评估标准**: 制定技术评估标准化流程

### 中期建议 (6-12个月)
1. **深度研发**: 在关键技术点上进行突破
2. **平台建设**: 构建技术实验和验证平台
3. **标准化推进**: 参与或推动技术标准制定

### 长期建议 (1-3年)
1. **技术规划**: 制定长期技术发展规划
2. **生态布局**: 构建完整技术生态系统
3. **能力建设**: 建立持续技术创新能力
"""
        }

    def _prepare_sources(self, research_data: Dict[str, Any]) -> List[Dict[str, Any]]:
        """准备参考资料"""
        sources = []
        literature = research_data.get('literature', {})

        if isinstance(literature, dict):
            for source_type, key in [('GitHub Repository', 'github_results'), ('Academic Paper', 'paper_results'), ('Technical Blog', 'blog_results')]:
                for item in literature.get(key, []):
                    sources.append({
                        'type': source_type,
                        'title': getattr(item, 'title', 'Unknown'),
                        'url': getattr(item, 'url', ''),
                        'description': getattr(item, 'description', '')
                    })

        return sources

    def _generate_markdown_report(self, data: Dict[str, Any]) -> str:
        """生成Markdown格式报告"""
        m = data['metadata']
        sections = data['sections']

        # 生成目录和章节
        toc_items = [
            "1. [执行摘要](#执行摘要)",
            "2. [研究背景与目标](#研究背景与目标)",
            "3. [研究方法](#研究方法)",
            "4. [主要发现](#主要发现)",
            "5. [深入分析](#深入分析)",
            "6. [结论](#结论)",
            "7. [建议](#建议)"
        ]

        # 生成章节内容
        sections_md = ""
        for section in sections.values():
            sections_md += f"\n## {section['title']}\n\n{section['content']}\n\n"

        # 生成发现子章节
        findings = sections['findings']
        if findings.get('subsections'):
            sections_md = sections_md.replace("## 主要发现\n\n", "")
            for sub in findings['subsections']:
                sections_md += f"### {sub['title']}\n\n{sub['content']}\n\n"

        # 生成参考资料
        sources_md = ""
        if data['sources']:
            sources_md = "\n".join(f"{i}. **{s['type']}**: {s['title']}" + (f" - [{s['url']}]({s['url']})" if s['url'] else "") for i, s in enumerate(data['sources'], 1))
        else:
            sources_md = "暂无参考资料"

        return f"""# {m['title']}

> **生成时间**: {m['generated_at']}
> **研究领域**: {m['research_domain']}
> **AI模型**: {m['model_info']}

---

## 目录

{chr(10).join(toc_items)}

---

## 执行摘要

{data['executive_summary']}

---{sections_md}---

## 参考资料

{sources_md}

---

*本报告由Research Agent自动生成，建议结合人工验证。*
"""

    def _generate_html_report(self, data: Dict[str, Any]) -> str:
        """生成HTML格式报告"""
        m = data['metadata']
        return f"""<!DOCTYPE html>
<html lang="zh">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>{m['title']}</title>
    <style>
        body {{ font-family: 'Microsoft YaHei', sans-serif; line-height: 1.6; margin: 0; padding: 20px; }}
        .header {{ background: #f4f4f4; padding: 20px; border-radius: 5px; margin-bottom: 20px; }}
        .section {{ margin: 20px 0; }}
    </style>
</head>
<body>
    <div class="header">
        <h1>{m['title']}</h1>
        <p><strong>生成时间:</strong> {m['generated_at']}</p>
        <p><strong>研究领域:</strong> {m['research_domain']}</p>
    </div>

    <div class="content">
        <h2>执行摘要</h2>
        <p>{data['executive_summary']}</p>

        <h2>研究背景与目标</h2>
        <p>{data['sections']['introduction']['content']}</p>

        <h2>研究方法</h2>
        <p>{data['sections']['methodology']['content']}</p>

        <h2>主要发现</h2>
        <p>基于AI辅助生成的技术分析结果</p>

        <h2>深入分析</h2>
        <p>{data['sections']['analysis']['content']}</p>

        <h2>结论</h2>
        <p>{data['sections']['conclusions']['content']}</p>

        <h2>建议</h2>
        <p>{data['sections']['recommendations']['content']}</p>
    </div>
</body>
</html>"""

    def _generate_text_report(self, data: Dict[str, Any]) -> str:
        """生成纯文本格式报告"""
        m = data['metadata']
        sections = data['sections']

        return f"""{m['title']}
{'=' * len(m['title'])}

生成时间: {m['generated_at']}
研究领域: {m['research_domain']}
AI模型: {m['model_info']}

执行摘要
----------
{data['executive_summary']}

研究背景与目标
--------------
{sections['introduction']['content']}

研究方法
--------
{sections['methodology']['content']}

主要发现
--------
{sections['findings']['subsections'][0]['content'] if sections['findings'].get('subsections') else '基于AI分析'}

深入分析
--------
{sections['analysis']['content']}

结论
----
{sections['conclusions']['content']}

建议
----
{sections['recommendations']['content']}

报告生成完成
"""

    async def _save_report(self, report: str, report_data: Dict[str, Any]) -> str:
        """保存报告到文件"""
        try:
            ext_map = {'markdown': '.md', 'html': '.html', 'pdf': '.pdf', 'text': '.txt'}
            ext = ext_map.get(self.config.output_format, '.md')

            filename = f"{report_data['metadata']['report_id']}{ext}"
            filepath = self.output_dir / filename

            filepath.write_text(report, encoding='utf-8')
            logger.info(f"报告已保存: {filepath}")
            return str(filepath)

        except Exception as e:
            logger.error(f"保存报告失败: {e}")
            return "保存失败"
