---
stepsCompleted: [1, 2, 3]
inputDocuments:
  - HeAgent/docs/brief-HeAgent-2026-05-23/brief.md
  - HeAgent/docs/prd-HeAgent-2026-05-23/prd.md
workflowType: 'architecture'
project_name: 'HeAgent'
user_name: 'tan'
date: '2026-05-23'
---

# Architecture Decision Document — HeAgent

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each architectural decision together._

## Project Context Analysis

### Requirements Overview

**Functional Requirements:**

19 个 FR 分为 7 组：

| 模块 | FR 范围 | 核心职责 | 架构影响 |
|------|---------|----------|----------|
| Provider 抽象层 | FR-1~5 | 统一多模型 API 接口 + 回退链 + 凭证轮换 | 需要稳定的 `BaseProvider` 协议，Provider 作为独立插件加载 |
| 工具调用引擎 | FR-6~9 | 声明式注册 + 并行执行 + 安全护栏 + 内置工具 | 注册表模式，`ThreadPoolExecutor` 并行，安全检查中间件 |
| 自学习系统 | FR-10~12 | 技能提炼 + 事实记忆 + 用户画像 | 后台异步分析，Markdown 持久化，自动注入系统提示词 |
| 上下文管理 | FR-13~15 | 自动压缩 + 迭代预算 + 会话持久化 | Token 计数器，压缩策略接口，JSON 序列化 |
| 子 Agent 委派 | FR-16~17 | 子 Agent 生成 + 并行编排 | 线程隔离，独立上下文，结果回收 |
| MCP 协议集成 | FR-18 | 外部工具服务器发现与调用 | MVP 后延后，但接口预留 |
| 错误处理 | FR-19 | 错误分类 + 差异化重试 | 全局重试中间件，错误分类枚举 |

**Non-Functional Requirements:**

- **代码可读性**：核心循环 <1,500 行，单模块 <400 行，函数 <50 行
- **插件化零耦合**：新增 Provider/工具不修改核心代码
- **可靠性**：Provider 回退 + 凭证轮换确保服务连续
- **安全性**：终端命令安全护栏，用户可配黑白名单
- **资源控制**：迭代预算防无限循环，上下文压缩防 token 超限

**Scale & Complexity:**

- Primary domain: 后端 AI Agent 框架库
- Complexity level: 中等 — 模块多但边界清晰，无分布式/实时/合规需求
- Estimated architectural components: 10-12 个核心模块

### Technical Constraints & Dependencies

- **语言**: Python 3.11+
- **核心依赖**: Pydantic v2（数据模型）、httpx（异步 HTTP）、openai SDK、anthropic SDK
- **代码量约束**: 核心循环 <1,500 行，单模块 <400 行
- **无重型框架**: CLI 用 argparse/click，无 FastAPI/HTTP 服务
- **持久化格式**: Markdown（SKILL.md、MEMORY.md、USER.md）+ JSON（会话历史）

### Cross-Cutting Concerns Identified

1. **错误重试策略** — 影响 Provider、工具执行、子 Agent 所有外部调用
2. **Token/迭代预算** — 影响核心循环、上下文管理、子 Agent
3. **安全护栏** — 影响工具执行、子 Agent 工具继承
4. **日志** — 贯穿所有模块，需统一日志接口
5. **配置管理** — API Key、模型参数、阈值等需统一配置层

## Starter Template Evaluation

### Primary Technology Domain

**自定义 Python AI Agent 框架** — 非 Web/API/Mobile 项目，不适用标准 starter template。

### Starter Options Considered

**评估结论：不使用外部 starter template。**

原因：
1. 技术栈已由产品简报锁定（Python 3.11+, Pydantic v2, httpx, openai SDK, anthropic SDK）
2. HeAgent 是从零构建的自定义框架，没有匹配的模板
3. 项目结构设计是架构决策的一部分，将在后续步骤中完成

### 依赖版本锁定（2026-05-23 验证）

| 依赖 | 版本 | 用途 |
|------|------|------|
| python | >=3.11 | 运行时 |
| pydantic | v2.13.4 | 数据模型、配置验证 |
| httpx | v0.28.1 | 异步 HTTP 客户端 |
| openai | v2.37.0 | OpenAI API 适配 |
| anthropic | v0.104.0 | Anthropic API 适配 |
| click | >=8.0 | CLI 框架（选用 click 而非 argparse，支持命令组和装饰器） |

### 技术决策已确定

**语言 & 运行时：**
- Python 3.11+（利用原生 async/await、类型提示增强、match-case）
- 全异步架构（async/await 贯穿所有 I/O 操作）

**数据层：**
- Pydantic v2 用于所有数据模型（ProviderResponse、ToolSchema、MemoryEntry 等）
- 配置验证使用 Pydantic BaseSettings

**HTTP 通信：**
- httpx.AsyncClient 作为统一 HTTP 客户端
- 不直接使用 requests（同步）或 aiohttp（冗余）

**CLI：**
- click 框架（比 argparse 更声明式，支持命令组）

**构建 & 开发：**
- uv 或 pip 作为包管理
- pytest + pytest-asyncio 作为测试框架
- ruff 作为 linter + formatter

**Note:** 项目初始化（创建项目结构、配置 pyproject.toml）将是第一个实现 Story。
