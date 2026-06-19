# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 语言和Git规则
- **语言**：使用中文进行交流
- **Git规则**：不自动提交任何代码更改，手动提交前需要明确确认

## Project Overview

多项目技术试验仓库，包含独立的子项目。每个子项目有自己的依赖和运行方式，**必须在各自目录下运行**（导入路径依赖）。

## 子项目

### AgentSdkTest — 多模型AI代理框架
Python 3.10+ 项目，提供统一的AI代理接口，支持 Claude(智谱AI)/OpenAI/DeepSeek/Ollama 多模型切换。

核心架构：`UniversalAIAgent`（基类）→ `UniversalTaskAgent` / `UniversalCodeAgent` / `UniversalTalkAgent`，通过 `provider` 参数切换模型。工厂模式在 `lib/factory.py`，基类与子类在 `lib/multi_agent.py`。

```bash
cd AgentSdkTest
pip install -r requirements.txt           # 安装依赖
python quick_start.py                      # 交互式菜单（主入口，推荐）
python run_all_examples.py                 # 运行所有示例
```

环境变量配置在 `config/.env`，加载优先级：`config/.env` > `.env`。API密钥格式为智谱AI（id.secret）。

**导入路径**：必须在 AgentSdkTest 目录下运行，使用 `from lib.multi_agent import UniversalAIAgent`。AgentSdkTest 有独立的 `.mcp.json`。

### Research — 技术调研代理
继承 `UniversalTaskAgent`，添加文献检索、数据处理、报告生成、质量检查模块（`modules/`：`literature_retriever`、`data_processor`、`report_generator`、`quality_checker`、`tool_manager`）。

```bash
cd Research
pip install -r requirements.txt
python example_usage.py                    # 运行示例
pytest test/                               # 运行测试（test_research_agent.py）
```

所有方法都是异步的，需要 `asyncio.run()` 调用。Research 自动将 AgentSdkTest 添加到 `sys.path`（`from lib.multi_agent import UniversalTaskAgent`）。`Research/reports/` 已在 `.gitignore` 排除。

### Prompt/ — 提示词模板集合
中文提示词模板（Gemini、翻译、排版、打分器等），纯 Markdown 文本。

### _bmad/ — BMAD 企业框架
BMAD 工作流通过 Claude Code 技能系统调用，模块在 `_bmad/bmb`（Builder）与 `_bmad/bmm`（Model），配置在 `_bmad/config.toml` / `config.user.toml`。技能名以 `bmad-*` 前缀调用（如 `bmad-create-prd`、`bmad-agent-pm`、`bmad-bmb-setup`）。BMAD 产物输出到 `_bmad-output/`（`bmb-creations`、`planning-artifacts`）。

## 全局架构关系

```
AgentSdkTest/lib/multi_agent.py  ←──  Research/research_agent.py (继承)
       UniversalAIAgent (基类)
            ├── UniversalTaskAgent → ResearchAgent (调研)
            ├── UniversalCodeAgent (代码)
            └── UniversalTalkAgent (对话，未实现)
```

根目录 `.mcp.json` 当前为空（`{"mcpServers": {}}`）；MCP 配置按子项目独立维护（AgentSdkTest 有自己的 `.mcp.json`）。

## 其他目录
- `learning/` — 学习笔记（BMAD 版本历史等）
- `claude-updater-reports/` — claude-updater 工具的报告输出（`latest.md`、`history/`、`tips/`）

## 关键注意事项
- **运行目录**：AgentSdkTest 和 Research 各自必须在自己的目录下运行（导入路径依赖）
- **环境变量**：`.env` 文件已在 `.gitignore` 排除，通过 `python-dotenv` 加载
- **已迁移**：gamego（Unity 五子棋）已迁移至独立仓库 `E:/AI/AILab_Gomoku`，本仓库不再包含

## 网页浏览

网页浏览与自动化优先使用 chrome-devtools MCP（ecc 插件提供，`mcp__plugin_ecc_chrome-devtools__*` 工具，如 `navigate_page`、`take_snapshot`、`click`）；若额外安装了 Playwright MCP，也可使用其 `browser_*` 工具。gstack 已于 2026-06 移除，其功能由已安装的 bmad / ecc / document-skills 插件覆盖。

## 技能路由

当用户的请求匹配到可用技能时，通过 Skill 工具调用。不确定时优先调用技能。

路由规则（技能来源：bmad-* / ecc:* / document-skills:* / 内置）：
- 产品创意/头脑风暴 → bmad-brainstorming
- 产品需求/PRD/战略 → bmad-prd 或 bmad-product-brief
- 架构设计 → bmad-create-architecture 或 ecc:plan
- 设计系统/前端方案 → ecc:design-system 或 document-skills:frontend-design
- Bug/报错/构建错误 → bmad-investigate 或 ecc:build-fix
- QA/测试网站 → ecc:browser-qa 或 document-skills:webapp-testing（浏览器自动化驱动）
- 代码审查/diff 检查 → ecc:code-review 或内置 code-review
- 视觉/前端优化 → document-skills:frontend-design 或 ecc:frontend-design-direction
- 发布/PR → ecc:pr 或 ecc:review-pr
- 安全审查 → ecc:security-scan 或内置 security-review
- 保存/恢复会话 → ecc:save-session / ecc:resume-session
