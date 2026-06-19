# ClaudeLearning

> 个人多项目技术试验仓库：AI 代理 SDK、技术调研代理、提示词模板与 BMAD 方法框架的实践场。

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![Python 3.10+](https://img.shields.io/badge/python-3.10+-blue.svg)](https://www.python.org/downloads/)

## 仓库内容

| 目录 | 说明 | 技术栈 | 入口 |
|------|------|--------|------|
| [`AgentSdkTest/`](./AgentSdkTest) | 多模型 AI 代理框架，统一接口切换 Claude(智谱)/OpenAI/DeepSeek/Ollama | Python 3.10+ | `quick_start.py` |
| [`Research/`](./Research) | 技术调研代理，继承 `UniversalTaskAgent`，含文献检索/数据处理/报告生成/质量检查 | Python 3.10+ | `example_usage.py` |
| [`Prompt/`](./Prompt) | 中文提示词模板集合（Gemini、翻译、排版、打分器等） | Markdown | — |
| [`_bmad/`](./_bmad) | BMAD 方法框架（Builder `bmb` + Model `bmm`），通过 `bmad-*` 技能调用 | TOML | — |
| [`learning/`](./learning) | 学习笔记（BMAD 版本历史等） | Markdown | — |

每个子项目各自独立，**必须在各自目录下运行**（导入路径依赖）。详见 [`CLAUDE.md`](./CLAUDE.md)。

## 快速开始

### AgentSdkTest — 多模型代理

```bash
cd AgentSdkTest
pip install -r requirements.txt          # claude-agent-sdk / anthropic / openai / python-dotenv 等
cp config/.env.example config/.env       # 填入 API 密钥（智谱AI 格式 id.secret）
python quick_start.py                    # 交互式菜单（推荐）
```

最小示例（**同步**调用）：

```python
# 须在 AgentSdkTest/ 目录下运行
from lib.multi_agent import UniversalAIAgent

agent = UniversalAIAgent(provider="deepseek")   # claude / openai / deepseek / ollama / mock
print(agent.chat("用一句话介绍你自己"))
```

### Research — 技术调研

```bash
cd Research
pip install -r requirements.txt          # 86 行依赖（含数据处理 / 检索库）
python example_usage.py
pytest test/                             # 运行 test_research_agent.py
```

Research 的方法均为**异步**，需用 `asyncio.run()` 调用：

```python
# 须在 Research/ 目录下运行（脚本会自动把 AgentSdkTest 加入 sys.path）
import asyncio
from research_agent import ResearchAgent

async def main():
    researcher = ResearchAgent(research_domain="人工智能", provider="deepseek")
    result = await researcher.conduct_research(query="大语言模型的最新趋势")
    print(result)   # ResearchResult：含报告与来源

asyncio.run(main())
```

## 架构关系

```
AgentSdkTest/lib/multi_agent.py        ←──  Research/research_agent.py（继承）
       UniversalAIAgent（基类）
            ├── UniversalTaskAgent  →  ResearchAgent（调研）
            ├── UniversalCodeAgent（代码：write_code / review_code / debug_code）
            └── UniversalTalkAgent（对话，未实现）
```

`UniversalAIAgent` 通过 `provider` 参数切换模型；工厂在 `lib/factory.py`，类层级在 `lib/multi_agent.py`。

## BMAD 方法框架

BMAD 通过 Claude Code 技能系统调用，模块位于 `_bmad/`（`bmb` Builder、`bmm` Model），配置在 `_bmad/config.toml`。常用技能以 `bmad-` 前缀调用：

- `bmad-agent-pm` — 产品经理（PRD）
- `bmad-agent-architect` — 架构设计
- `bmad-agent-dev` — 故事实现
- `bmad-create-prd` / `bmad-create-architecture` / `bmad-quick-dev`

产物输出到 `_bmad-output/`（`planning-artifacts`、`bmb-creations`）。当前版本 v6.8.0（bmb 1.8.0）。

## 项目结构

```
ClaudeLearning/
├── AgentSdkTest/            多模型 AI 代理框架（lib/ 核心、examples/、quick_start.py）
├── Research/                技术调研代理（modules/、example_usage.py、test/）
├── Prompt/                  中文提示词模板（含 Test/ Out/ 子目录）
├── _bmad/                   BMAD 框架（bmb/ bmm/ core/ custom/ config.toml）
├── _bmad-output/            BMAD 产物输出
├── learning/                学习笔记
├── claude-updater-reports/  claude-updater 工具报告（latest.md / history/ / tips/）
├── docs/                    预留
├── CLAUDE.md                Claude Code 项目指南
├── LICENSE                  MIT
└── README.md                本文件
```

## 技术栈

- **Python 3.10+** — AgentSdkTest / Research
- **claude-agent-sdk**（Anthropic 官方）+ `anthropic` / `openai` — 多模型统一接口
- **智谱 AI GLM-4.x** 作为 Claude 渠道的默认模型（API 密钥格式 `id.secret`）
- **MCP**（Model Context Protocol）— 按子项目独立配置 `.mcp.json`
- **BMAD Method v6.8.0** — 企业级方法框架

## 环境变量

`.env` 已在 `.gitignore` 排除，通过 `python-dotenv` 加载。加载优先级：`AgentSdkTest/config/.env` > `.env`。

## 相关链接

- [Claude Code](https://claude.ai/code)
- [Claude Agent SDK](https://docs.anthropic.com/claude/docs)
- [BMAD Method](https://bmad-method.com/)
- [Model Context Protocol](https://modelcontextprotocol.io/)

## 许可证

[MIT](./LICENSE)
