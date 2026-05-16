# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 语言和Git规则
- **语言**：使用中文进行交流
- **Git规则**：不自动提交任何代码更改，手动提交前需要明确确认

## Project Overview

多项目技术试验仓库，包含独立的子项目。每个子项目有自己的依赖和运行方式。

## 子项目

### AgentSdkTest — 多模型AI代理框架
Python 3.10+ 项目，提供统一的AI代理接口，支持 Claude(智谱AI)/OpenAI/DeepSeek/Ollama 多模型切换。

核心架构：`UniversalAIAgent`（基类）→ `UniversalTaskAgent` / `UniversalCodeAgent` / `UniversalTalkAgent`，通过 `provider` 参数切换模型。工厂模式在 `lib/factory.py`。

```bash
cd AgentSdkTest
pip install -r requirements.txt           # 安装依赖
python quick_start.py                      # 交互式菜单（推荐）
python examples/02_multi_model.py          # 多模型示例
python run_all_examples.py                 # 运行所有示例
```

环境变量配置在 `config/.env`，加载优先级：`config/.env` > `.env`。API密钥格式为智谱AI（id.secret）。

**导入路径**：必须在 AgentSdkTest 目录下运行，使用 `from lib.multi_agent import UniversalAIAgent`。

### Research — 技术调研代理
继承 `UniversalTaskAgent`，添加文献检索、数据处理、报告生成、质量检查模块。

```bash
cd Research
pip install -r requirements.txt
python example_usage.py                    # 运行示例
pytest test/                               # 运行测试
```

所有方法都是异步的，需要 `asyncio.run()` 调用。Research 自动将 AgentSdkTest 添加到 `sys.path`。

### gamego — 五子棋 (Unity)
Unity 2021.3 LTS 五子棋游戏，支持 PvP/PvAI。AI 策略有 SimpleAI（规则优先级）和 MinimaxAI（Alpha-Beta 剪枝）。通过 Unity Editor 菜单 Tools > Gomoku 设置场景。测试在 Test Runner PlayMode 标签运行。

### Prompt/ — 提示词模板集合
中文提示词模板文件（Gemini、翻译、排版、分析等），纯 Markdown 文本。

### _bmad/ — BMAD 企业框架
BMAD 工作流通过 Claude Code 技能系统调用，使用 `bmad-bmm-*` / `bmad-bmgd-*` / `bmad-bmb-*` 前缀的技能名。

## 全局架构关系

```
AgentSdkTest/lib/multi_agent.py  ←──  Research/research_agent.py (继承)
       UniversalAIAgent (基类)
            ├── UniversalTaskAgent → ResearchAgent (调研)
            ├── UniversalCodeAgent (代码)
            └── UniversalTalkAgent (对话，未实现)
```

MCP 服务器配置在根目录 `.mcp.json`（agent-sdk-bridge 桥接 MCP 协议）。AgentSdkTest 有独立的 `.mcp.json`。

## 关键注意事项

- **运行目录**：AgentSdkTest 和 Research 各自必须在自己的目录下运行（导入路径依赖）
- **环境变量**：`.env` 文件已在 `.gitignore` 排除，通过 `python-dotenv` 加载
- **文档目录**：`docs/` 包含 BMAD 生成的 PRD、架构文档、故事文件和 sprint 状态
- **scripts/**：包含 codex 等辅助脚本
