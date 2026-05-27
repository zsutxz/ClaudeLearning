---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]
inputDocuments:
  - HeAgent/docs/brief-HeAgent-2026-05-23/brief.md
  - HeAgent/docs/prd-HeAgent-2026-05-23/prd.md
workflowType: 'architecture'
lastStep: 8
status: 'complete'
completedAt: '2026-05-25'
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

## Core Architectural Decisions

### Decision Priority Analysis

**Critical Decisions (Block Implementation):**

- 模块间通信模式 — 决定整体代码组织方式
- Provider 抽象层设计 — 影响 FR-1~5 全部功能
- 工具注册机制 — 影响 FR-6~9 全部功能

**Important Decisions (Shape Architecture):**

- 配置管理 — 影响所有模块的初始化和参数传递
- 记忆存储策略 — 影响 FR-10~12 的持久化方式
- 子 Agent 隔离模型 — 影响 FR-16~17 的并行实现

**Deferred Decisions (Post-MVP):**

- MCP 协议集成（FR-18）— 接口预留，MVP 后实现
- 语义相似度记忆去重 — V1 用关键词匹配
- 独立辅助模型上下文压缩 — V1 用主模型

### 模块间通信模式

**决策：直接调用 + 中间件管道混合**

- 核心调用链：模块间通过 `typing.Protocol` 定义接口，直接函数/方法调用。简洁、类型安全、IDE 友好
- 横切关注点：重试策略、安全护栏、日志通过中间件管道插入，不侵入核心逻辑
- 中间件接口：`Middleware` Protocol，`async def process(request, next) -> response` 链式调用
- **Rationale：** HeAgent 是单体框架，不需要事件总线级别的解耦。中间件处理横切关注点避免核心逻辑膨胀

### 工具注册机制

**决策：`@tool` 装饰器注册**

- 函数上加 `@tool` 装饰器，自动提取函数签名、类型注解、docstring 生成 JSON Schema
- 运行时通过 `ToolRegistry` 单例管理所有注册工具
- 添加新工具只需写函数加装饰器，零样板代码，不改核心循环
- 内置工具（终端、文件、搜索）和 MCP 工具统一走 `ToolRegistry`
- **Rationale：** 最 Pythonic 的方式，类型注解直接映射到 LLM 的 tool schema，零额外配置

### Provider 抽象层设计

**决策：单协议接口 + 外层 ProviderChain**

- `BaseProvider` Protocol：`async def send()`, `async def stream()`, `get_metadata()` 等方法
- 所有 Provider（OpenAI、Anthropic）实现 `BaseProvider`，只负责"与模型通信"
- `ProviderChain` 外层类：管理有序 Provider 列表，处理回退（429/503 触发切换）和凭证轮换（多 Key 池）
- **Rationale：** 职责单一——Provider 不知道回退逻辑，Chain 不知道 API 细节。新增 Provider 只实现 Protocol，接入 Chain 即可

### 配置管理

**决策：Pydantic BaseSettings + `.env` 文件**

- 使用 `pydantic-settings` 的 `BaseSettings` 加载配置
- 敏感配置（API Key）从 `.env` 文件/环境变量加载
- 非敏感配置（模型参数、阈值）可在 Settings 类中设置默认值
- 类型校验和验证规则通过 Pydantic 字段声明，启动时即失败
- **Rationale：** 与 Pydantic v2 统一生态，零额外学习成本，类型安全免费获得

### 记忆存储策略

**决策：项目级 `.heagent/` 目录**

- 存储结构：`.heagent/skills/`（SKILL.md）、`.heagent/memory/`（MEMORY.md）、`.heagent/user/`（USER.md）
- 所有记忆文件跟随项目，不跨项目共享
- 记忆去重 V1 用关键词匹配，后续可升级语义相似度
- **Rationale：** 简单直接，避免跨项目合并的复杂度。项目隔离更安全

### 子 Agent 隔离模型

**决策：`asyncio.create_task` 同进程异步任务**

- 子 Agent 通过 `asyncio.create_task()` 创建，共享事件循环
- 并行编排用 `asyncio.gather()` 收集所有子 Agent 结果
- 子 Agent 有独立的对话上下文和迭代预算，但不做进程级隔离
- 单个子 Agent 失败不阻塞其他，异常捕获后返回错误结果
- **Rationale：** HeAgent 是单 Agent 单会话场景，子 Agent 是短期任务委派，不需要进程隔离的开销

### 日志策略

**决策：Python stdlib `logging`**

- 使用标准库 `logging` 模块，零额外依赖
- 通过 `logging.config.dictConfig` 统一配置格式和层级
- 各模块通过 `logging.getLogger(__name__)` 获取 logger
- **Rationale：** Agent 框架日志需求不复杂，不引入 loguru/structlog 等第三方依赖

### Decision Impact Analysis

**Implementation Sequence:**

1. 项目结构初始化 + 配置管理（Settings）
2. Provider 抽象层（BaseProvider Protocol + OpenAI/Anthropic 实现 + ProviderChain）
3. 工具调用引擎（ToolRegistry + `@tool` 装饰器 + 内置工具）
4. Agent 核心循环（中间件管道 + 迭代预算）
5. 自学习系统（技能提炼 + 记忆管理 + 用户画像）
6. 上下文管理（自动压缩 + 会话持久化）
7. 子 Agent 委派（asyncio task 并行编排）

**Cross-Component Dependencies:**

- Provider 抽象层是核心循环的依赖（步骤 2 → 4）
- 工具注册是核心循环的依赖（步骤 3 → 4）
- 自学习系统依赖核心循环（步骤 4 → 5）
- 上下文管理依赖 Provider（步骤 2 → 6）
- 子 Agent 委派依赖核心循环 + 工具注册（步骤 4+3 → 7）

## Implementation Patterns & Consistency Rules

### Pattern Categories Defined

**Critical Conflict Points Identified:**
18 areas where AI agents could make different choices — covering naming, structure, format, communication, and process.

### Naming Patterns

**File Naming:**
- `snake_case.py` — PEP 8 标准
- 例：`tool_registry.py`, `base_provider.py`, `agent_loop.py`

**Class Naming:**
- `PascalCase` — PEP 8 标准
- 例：`ToolRegistry`, `ProviderChain`, `AgentLoop`

**Protocol Naming:**
- 无 `Protocol`/`Interface` 后缀 — Protocol 是类型提示不是类，后缀是噪音
- 例：`BaseProvider(TypedProtocol)` 而非 `BaseProviderProtocol`
- 通过 docstring 标注 Protocol 用途

**Async Method Naming:**
- 不加 `_async` 后缀 — 全异步架构中后缀冗余
- 例：`async def send()` 而非 `async def send_async()`

**Constants:**
- `UPPER_SNAKE_CASE` — PEP 8 模块级常量
- 例：`MAX_ITERATIONS`, `DEFAULT_MODEL`, `RETRY_BACKOFF_FACTOR`

**Functions & Variables:**
- `snake_case` — PEP 8 标准
- 例：`register_tool()`, `compress_context()`, `tool_registry`

### Structure Patterns

**Project Organization:**

```
heagent/
├── providers/        # Provider 抽象层 (FR-1~5)
├── tools/            # 工具调用引擎 (FR-6~9)
│   ├── registry.py   # ToolRegistry 单例
│   ├── decorator.py  # @tool 装饰器
│   ├── builtins/     # 内置工具 (shell, file, search)
│   └── safety.py     # 安全护栏
├── memory/           # 自学习系统 (FR-10~12)
├── context/          # 上下文管理 (FR-13~15)
├── agent/            # 核心循环 + 子 Agent (FR-16~17)
├── config.py         # Pydantic Settings
└── cli.py            # click CLI 入口
```

**Test Location:**
- `tests/` 目录，镜像源码目录结构
- 测试文件：`test_<module>.py`
- 共享 fixtures 在 `tests/conftest.py`

**Built-in Tools:**
- 放在 `tools/builtins/` 子目录，与第三方工具隔离
- 每个内置工具一个文件

**Runtime Config:**
- 项目根 `.heagent/` 目录存放运行时数据（skills, memory, user）
- 框架配置用 `pyproject.toml` 的 `[tool.heagent]` 段

### Format Patterns

**Data Models:**
- 所有数据结构使用 Pydantic BaseModel，不用 dict 或 dataclass
- 例：

```python
class ProviderResponse(BaseModel):
    content: str
    tool_calls: list[ToolCall] = []
    usage: TokenUsage
    model: str
    finish_reason: str
```

**Error Structure:**
- 自定义异常层级，不混用 dict 和 Exception

```python
class HeAgentError(Exception): ...
class ProviderError(HeAgentError): ...
class ToolError(HeAgentError): ...
class SafetyViolation(HeAgentError): ...
class BudgetExceeded(HeAgentError): ...
```

**JSON Field Naming:**
- `snake_case` — Python 生态标准，Pydantic 默认
- 外部 API 可通过 `alias_generator` 配置 `camelCase`

### Communication Patterns

**Middleware Signature:**
- `(request, next) -> response` 链式调用

```python
class Middleware(TypedProtocol):
    async def __call__(self, request: Request, next: NextFn) -> Response: ...
```

**Logging Format:**
- 可配置，默认纯文本：`%(levelname)s [%(name)s] %(message)s`
- 通过 `dictConfig` 可切换为 JSON 结构化
- 各模块用 `logging.getLogger(__name__)`

**Callback/Hook Naming:**
- `on_<event>` 前缀
- 例：`on_provider_error`, `on_tool_call`, `on_turn_complete`

### Process Patterns

**Retry Strategy:**
- 指数退避 + 抖动：`min(base * 2^attempt + jitter, max_delay)`
- 默认：`base=1s, max_delay=30s, max_retries=3`
- 仅重试可重试错误（429, 503, 网络超时），不重试 4xx

**Error Recovery:**
- Provider 层：自动重试 → 回退到下一个 Provider
- 工具执行层：返回 `ToolError` 结果，不中断 Agent 循环
- 安全护栏层：直接拒绝，不重试
- 预算超限：优雅停止，返回已收集的上下文

**Validation Timing:**
- `Settings` 在程序入口一次性全量校验
- Provider 连接首次使用时惰性验证
- 工具注册在装饰器执行时验证签名完整性

### Enforcement Guidelines

**All AI Agents MUST:**
- 遵循 PEP 8 命名约定，不引入其他风格
- 每个 PR 只修改一个功能域目录下的文件
- 新增 Provider/工具必须在对应目录下，实现对应 Protocol
- 所有异常继承 `HeAgentError` 层级，不引入裸 Exception

**Pattern Verification:**
- `ruff check` 强制命名和格式规范
- `mypy --strict` 验证类型注解完整性
- PR review 检查目录归属和 Protocol 实现

### Pattern Examples

**Good:**

```python
# providers/anthropic.py
class AnthropicProvider:
    """Provider for Anthropic Claude API."""

    async def send(self, messages: list[Message]) -> ProviderResponse:
        ...
```

**Anti-Pattern:**

```python
# providers/AnthropicProvider.js  ← 错误：PascalCase 文件名
class anthropic_provider:  # ← 错误：snake_case 类名
    async def send_async(self, msgs):  # ← 错误：_async 后缀 + 无类型注解
        ...
```

## Project Structure & Boundaries

### Complete Project Directory Structure

```
heagent/
├── pyproject.toml              # 项目元数据、依赖、[tool.heagent] 配置
├── README.md
├── LICENSE
├── .env.example                # 环境变量模板
├── .gitignore
├── ruff.toml                   # ruff linter/formatter 配置
├── src/
│   └── heagent/
│       ├── __init__.py         # 公开 API 导出
│       ├── cli.py              # click CLI 入口
│       ├── config.py           # Pydantic BaseSettings
│       ├── exceptions.py       # HeAgentError 异常层级
│       ├── types.py            # 共享类型定义 (Message, ToolCall, TokenUsage 等)
│       ├── providers/
│       │   ├── __init__.py     # 导出 BaseProvider, ProviderChain
│       │   ├── base.py         # BaseProvider Protocol
│       │   ├── openai.py       # OpenAI/兼容 API 实现
│       │   ├── anthropic.py    # Anthropic Claude 实现
│       │   ├── chain.py        # ProviderChain (回退+凭证轮换)
│       │   └── mcp.py          # MCP 协议集成 (MVP 后)
│       ├── tools/
│       │   ├── __init__.py     # 导出 @tool, ToolRegistry
│       │   ├── registry.py     # ToolRegistry 单例
│       │   ├── decorator.py    # @tool 装饰器 + schema 生成
│       │   ├── safety.py       # 安全护栏 (黑白名单)
│       │   └── builtins/
│       │       ├── __init__.py # 注册内置工具
│       │       ├── shell.py    # 终端命令执行
│       │       ├── file.py     # 文件读写
│       │       └── search.py   # 文件/内容搜索
│       ├── memory/
│       │   ├── __init__.py
│       │   ├── skills.py       # 技能提炼 (SKILL.md)
│       │   ├── facts.py        # 事实记忆 (MEMORY.md)
│       │   └── profile.py      # 用户画像 (USER.md)
│       ├── context/
│       │   ├── __init__.py
│       │   ├── compressor.py   # 上下文压缩
│       │   └── session.py      # 会话持久化 (JSON)
│       └── agent/
│           ├── __init__.py     # 导出 Agent, Middleware
│           ├── loop.py         # Agent 核心循环 + 迭代预算
│           ├── middleware.py   # Middleware Protocol + 管道
│           └── sub.py          # 子 Agent 委派 (asyncio.gather)
├── tests/
│   ├── conftest.py             # 共享 fixtures
│   ├── test_config.py
│   ├── test_exceptions.py
│   ├── providers/
│   │   ├── test_base.py
│   │   ├── test_openai.py
│   │   ├── test_anthropic.py
│   │   └── test_chain.py
│   ├── tools/
│   │   ├── test_registry.py
│   │   ├── test_decorator.py
│   │   ├── test_safety.py
│   │   └── builtins/
│   │       ├── test_shell.py
│   │       ├── test_file.py
│   │       └── test_search.py
│   ├── memory/
│   │   ├── test_skills.py
│   │   ├── test_facts.py
│   │   └── test_profile.py
│   ├── context/
│   │   ├── test_compressor.py
│   │   └── test_session.py
│   └── agent/
│       ├── test_loop.py
│       ├── test_middleware.py
│       └── test_sub.py
├── examples/
│   ├── basic_chat.py
│   ├── tool_usage.py
│   └── multi_provider.py
└── docs/
    └── ...
```

### Architectural Boundaries

**Module Dependency Direction (acyclic):**

```
exceptions ←── types ←── config
    ↑              ↑          ↑
    └── providers ──┴── tools ─┴── context ── agent
                            ↑              ↑
                        memory ─────────────┘
```

**Module Communication Rules:**
- `exceptions` 和 `types` 是叶子模块，无内部依赖
- `config` 仅依赖 `pydantic-settings`
- `providers` 和 `tools` 互相不知道对方
- `agent` 是顶层编排者，依赖 providers、tools、context、memory
- `memory` 仅被 `agent` 调用，不主动访问其他模块

**Internal Communication:**
- Provider ↔ Agent：通过 `ProviderResponse` Pydantic model
- Tools ↔ Agent：通过 `ToolCall` / `ToolResult` model
- Memory ↔ Agent：通过 `MemoryEntry` model + Markdown 文件
- Middleware：`(Request, NextFn) -> Response` 链式管道

**External Integration Points:**
- Provider → OpenAI / Anthropic API (httpx)
- Tools.builtins.shell → 系统终端 (subprocess)
- Tools.builtins.file → 本地文件系统
- Memory → `.heagent/` 目录的 Markdown/JSON 文件
- MCP (MVP 后) → 外部 MCP 工具服务器

### Requirements to Structure Mapping

| FR | File | Description |
|----|------|-------------|
| FR-1 | `providers/base.py` | BaseProvider Protocol |
| FR-2 | `providers/openai.py`, `providers/anthropic.py` | Model implementations |
| FR-3 | `providers/anthropic.py` | Anthropic caching (V1 basic) |
| FR-4 | `providers/chain.py` | Provider fallback chain |
| FR-5 | `providers/chain.py` | Credential rotation |
| FR-6 | `tools/decorator.py`, `tools/registry.py` | @tool registration |
| FR-7 | `tools/registry.py` | Parallel execution |
| FR-8 | `tools/safety.py` | Safety guardrails |
| FR-9 | `tools/builtins/*.py` | Built-in tools |
| FR-10 | `memory/skills.py` | Skill extraction |
| FR-11 | `memory/facts.py` | Fact memory |
| FR-12 | `memory/profile.py` | User profile |
| FR-13 | `context/compressor.py` | Context compression |
| FR-14 | `agent/loop.py` | Iteration budget |
| FR-15 | `context/session.py` | Session persistence |
| FR-16 | `agent/sub.py` | Sub-agent creation |
| FR-17 | `agent/sub.py` | Parallel orchestration |
| FR-18 | `providers/mcp.py` | MCP integration (post-MVP) |
| FR-19 | `exceptions.py` | Error classification + retry middleware |

### Integration Points

**Data Flow:**
1. User input → CLI (`cli.py`) → Agent loop (`agent/loop.py`)
2. Agent loop → Provider (`providers/chain.py`) → API call → `ProviderResponse`
3. Agent loop → Tool execution (`tools/registry.py`) → `ToolResult`
4. Agent loop → Context management (`context/compressor.py`) → compressed messages
5. Agent loop → Memory update (`memory/*.py`) → `.heagent/` files
6. Agent loop → Sub-agent (`agent/sub.py`) → parallel tasks → aggregated results

**Runtime Data Locations:**
- `.heagent/skills/*.md` — Skill files
- `.heagent/memory/*.md` — Memory files
- `.heagent/user/*.md` — User profile files
- `.heagent/sessions/*.json` — Session history

## Architecture Validation Results

### Coherence Validation ✅

**Decision Compatibility:**
- Python 3.11+ / Pydantic v2 / httpx / click — 全部兼容，无版本冲突
- Protocol 接口 + 直接调用 — `typing.Protocol` 是 Python 3.8+ 特性，3.11 完全支持
- `asyncio.create_task` + 全异步架构 — 统一 async/await，无同步/异步混用
- `@tool` 装饰器 + 运行时注册 — 函数签名 + 类型注解生成 schema，Python 原生支持
- click CLI + Agent 异步循环 — click 是同步框架，需在入口处 `asyncio.run()` 桥接（成熟模式）

**Pattern Consistency:**
- 命名全部遵循 PEP 8，无混用风格
- 结构按功能域组织，与模块决策对齐
- 通信通过 Pydantic model，无裸 dict 传递
- 中间件签名统一 `(Request, NextFn) -> Response`

**Structure Alignment:**
- 项目结构完整映射所有模块决策
- 依赖方向无环（DAG 已验证）
- 集成点（外部 API、文件系统、MCP）均有明确入口

### Requirements Coverage Validation ✅

**Functional Requirements Coverage:**
- 19/19 FR 全部有架构支撑
- FR-18 MCP 集成已预留接口文件，延后是产品决策（PRD 已确认 V1 不含）
- 3 项 V1 简化决策已记录（FR-3 基础缓存、FR-11 关键词去重、FR-13 主模型压缩）

**Non-Functional Requirements Coverage:**
- 代码可读性（<1500 行核心循环）→ 模块拆分支撑
- 插件化零耦合 → Protocol + @tool 注册机制
- 可靠性 → ProviderChain 回退 + 凭证轮换
- 安全性 → safety.py 安全护栏
- 资源控制 → 迭代预算 + 上下文压缩

### Implementation Readiness Validation ✅

**Decision Completeness:**
- 所有关键决策有版本号（Step 2 已锁定）
- 实现模式有 6 类命名规则 + 正反例
- 一致性规则可执行（ruff + mypy + PR review）

**Structure Completeness:**
- 目录结构到文件级完整
- FR → 文件映射 19/19
- 测试结构镜像源码

**Pattern Completeness:**
- 命名、结构、格式、通信、流程 5 类模式全覆盖
- 异常层级 5 级
- 重试策略参数明确

### Gap Analysis Results

**Critical Gaps: 无**

**Important Gaps:**
1. `types.py` 核心类型字段级定义未展开 — 实现时第一个 story 定义
2. `__init__.py` 公开 API 导出策略 — 建议扁平导出 `from heagent import Agent, tool`

**Nice-to-Have:**
1. `pyproject.toml` 完整配置（构建系统、依赖组、entry points）
2. `ruff.toml` 规则配置
3. CI/CD pipeline 定义（pytest + ruff + mypy）

### Validation Issues Addressed

**Click 异步桥接：** CLI 入口需显式 `asyncio.run()` 调用。建议在 `cli.py` 中统一处理：

```python
@click.command()
def chat():
    asyncio.run(Agent(settings).run())
```

**公开 API 导出：** 建议 `__init__.py` 扁平导出：

```python
from heagent.agent import Agent
from heagent.tools import tool
from heagent.config import Settings
```

### Architecture Completeness Checklist

**Requirements Analysis**
- [x] Project context thoroughly analyzed
- [x] Scale and complexity assessed
- [x] Technical constraints identified
- [x] Cross-cutting concerns mapped

**Architectural Decisions**
- [x] Critical decisions documented with versions
- [x] Technology stack fully specified
- [x] Integration patterns defined
- [x] Performance considerations addressed

**Implementation Patterns**
- [x] Naming conventions established
- [x] Structure patterns defined
- [x] Communication patterns specified
- [x] Process patterns documented

**Project Structure**
- [x] Complete directory structure defined
- [x] Component boundaries established
- [x] Integration points mapped
- [x] Requirements to structure mapping complete

### Architecture Readiness Assessment

**Overall Status:** READY FOR IMPLEMENTATION

**Confidence Level:** High

**Key Strengths:**
- 19/19 FR 完整覆盖，无遗漏
- 模块依赖无环，边界清晰
- 全异步架构统一，无同步/异步混用
- V1 简化决策明确记录，避免过度设计

**Areas for Future Enhancement:**
- MCP 协议集成（FR-18）
- 语义相似度记忆去重（替代关键词匹配）
- 独立辅助模型上下文压缩
- CI/CD pipeline 完善

### Implementation Handoff

**AI Agent Guidelines:**
- 遵循所有架构决策，不引入替代方案
- 使用实现模式保持一致性
- 尊重项目结构和模块边界
- 异常必须继承 HeAgentError 层级
- 新增 Provider/工具必须在对应目录下实现 Protocol

**First Implementation Priority:**
1. 项目初始化：创建 `pyproject.toml`、`src/heagent/` 目录结构、配置文件
2. `exceptions.py` + `types.py` — 基础类型和异常（无外部依赖）
3. `config.py` — Pydantic Settings 配置
4. `providers/` — Provider 抽象层（第一个可运行模块）
