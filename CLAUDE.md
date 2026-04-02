# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 规则
- **语言**：使用中文交流
- **Git**：不自动提交，手动提交前需明确确认
- **文档**：每个项目编写 FORCold.md，用通俗生动的语言解释项目（技术架构、设计决策、经验教训、最佳实践）

## 项目概览

多项目技术试验仓库，核心子项目：

| 项目 | 说明 | 技术栈 |
|------|------|--------|
| **AgentSdkTest/** | 多模型AI代理统一接口 | Python, Claude/OpenAI/DeepSeek/Ollama/Mock |
| **Research/** | 技术调研代理（继承UniversalTaskAgent） | Python, 文献检索/数据处理/报告生成 |
| **_bmad/** | BMAD企业级开发框架 | YAML工作流, Builder/Model/GameDev模块 |
| **.claude/** | Claude技能系统（169+技能） | Skills, MCP, Agents |

## 核心架构

**代理继承体系**：`UniversalAIAgent` → `UniversalTaskAgent` → `ResearchAgent` / `UniversalCodeAgent`
所有代理通过 `provider` 参数切换模型，支持同步和异步调用。

**模型选择**：GLM-4.7（默认，智谱AI）、GPT-4o-mini、DeepSeek-Coder、Ollama（本地）、Mock（测试）

**关键入口**：
- AgentSdkTest 核心：`lib/multi_agent.py`（UniversalAIAgent）、`lib/config.py`（配置）
- Research 核心：`research_agent.py` → `modules/`（literature_retriever, data_processor, report_generator）
- BMAD 工作流：`*workflow-init`、`*prd`、`*create-architecture`、`*dev-story`、`*sprint-planning`

## 运行命令

```bash
# AgentSdkTest（需在该目录下运行）
cd AgentSdkTest && python quick_start.py          # 交互式菜单
cd AgentSdkTest && python examples/02_multi_model.py  # 多模型示例
cd AgentSdkTest && python run_all_examples.py     # 运行所有示例

# Research（需在该目录下运行，依赖AgentSdkTest）
cd Research && python example_usage.py
cd Research && pytest test/

# 环境变量：AgentSdkTest/config/.env（优先）→ AgentSdkTest/.env（备用）
```

## 导入规则
- **AgentSdkTest**：`from lib.multi_agent import UniversalAIAgent`（必须在 AgentSdkTest/ 目录下）
- **Research**：`from research_agent import ResearchAgent`（必须在 Research/ 目录下，会自动 append AgentSdkTest 路径）

## MCP配置
- `.mcp.json` - 根目录配置（agent-sdk-bridge 自定义桥接）
- PYTHONPATH: 项目根目录 + AgentSdkTest

## 常见陷阱
- Research Agent 是异步的，必须用 `asyncio.run()` 调用
- 导入 `lib.*` 模块必须在 AgentSdkTest/ 目录下执行
- API密钥格式（智谱AI）：`id.secret`
- `.env` 文件已在 `.gitignore` 中，不会提交
