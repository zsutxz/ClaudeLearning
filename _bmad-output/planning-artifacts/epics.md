---
stepsCompleted: [1, 2, 3, 4]
status: complete
completedAt: '2026-05-26'
inputDocuments:
  - HeAgent/docs/prd-HeAgent-2026-05-23/prd.md
  - HeAgent/docs/architecture-HeAgent-2026-05-23/architecture.md
---

# HeAgent - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for HeAgent, decomposing the requirements from the PRD and Architecture requirements into implementable stories.

## Requirements Inventory

### Functional Requirements

- FR-1: Provider 接口定义 — 统一 BaseProvider 协议，含 send/stream/parse/metadata
- FR-2: OpenAI Provider — 支持 OpenAI 兼容端点、SSE 流式、function calling
- FR-3: Anthropic Provider — 支持 Claude API、流式、tool_use、基础缓存
- FR-4: Provider 回退链 — 429/503 自动切换下一个 Provider
- FR-5: 凭证轮换 — 多 Key 池，单 Key 限速自动切换
- FR-6: 声明式工具注册 — @tool 装饰器，自动生成 schema
- FR-7: 工具并行执行 — ThreadPoolExecutor 并行，失败不互影响
- FR-8: 安全护栏 — 危险命令拦截 + 黑白名单
- FR-9: 内置工具集 — 终端执行、文件操作、网页搜索
- FR-10: 技能提炼 — 后台分析执行模式，生成 SKILL.md
- FR-11: 事实记忆 — MEMORY.md 跨会话记忆，关键词去重
- FR-12: 用户画像 — USER.md 用户偏好，影响回复风格
- FR-13: 自动上下文压缩 — 主模型总结中间对话，阈值 80%
- FR-14: 迭代预算 — 默认 50 次，超出强制输出
- FR-15: 会话持久化 — JSON 格式，session ID 恢复
- FR-16: 子 Agent 生成 — 独立上下文+预算，继承父工具集
- FR-17: 并行子 Agent — asyncio.gather 并行，单失败不阻塞
- FR-18: MCP 工具发现与调用（MVP 后延后）
- FR-19: 错误分类与差异化重试 — 429→轮换/回退，瞬态→退避，非瞬态→返回

### NonFunctional Requirements

- NFR-1: 代码可读性 — 核心循环 <1,500 行，单模块 <400 行，函数 <50 行
- NFR-2: 插件化零耦合 — 新增 Provider/工具不修改核心代码
- NFR-3: 可靠性 — Provider 回退 + 凭证轮换确保服务连续
- NFR-4: 安全性 — 终端命令安全护栏，用户可配黑白名单
- NFR-5: 资源控制 — 迭代预算防无限循环，上下文压缩防 token 超限
- NFR-6: 可测试性 — 所有模块可独立单元测试
- NFR-7: 依赖最小化 — 仅 pydantic, httpx, openai, anthropic, click

### Additional Requirements

- 无外部 starter template — 项目从零构建，项目初始化是 Epic 1 Story 1
- Python 3.11+ — 利用原生 async/await、match-case、类型提示增强
- Pydantic v2 BaseModel — 所有数据模型统一使用，不用 dict/dataclass
- src layout — src/heagent/ 目录布局
- ruff — linter + formatter
- mypy --strict — 类型检查
- pytest + pytest-asyncio — 测试框架
- click CLI — 异步桥接需 asyncio.run() 入口
- pyproject.toml [tool.heagent] — 框架配置段
- .heagent/ 目录 — 运行时数据（skills/memory/user/sessions）
- 自定义异常层级 — HeAgentError → ProviderError/ToolError/SafetyViolation/BudgetExceeded
- 中间件管道 — (Request, NextFn) -> Response 链式调用
- 指数退避+抖动 — 默认 base=1s, max_delay=30s, max_retries=3

### UX Design Requirements

不适用 — HeAgent 是 CLI/库项目，无 UI。

### FR Coverage Map

| FR | Epic | 说明 |
|----|------|------|
| FR-1 | Epic 1 | BaseProvider Protocol |
| FR-2 | Epic 1 | OpenAI Provider |
| FR-3 | Epic 1 | Anthropic Provider |
| FR-4 | Epic 1 | Provider 回退链 |
| FR-5 | Epic 1 | 凭证轮换 |
| FR-6 | Epic 2 | 声明式工具注册 |
| FR-7 | Epic 2 | 工具并行执行 |
| FR-8 | Epic 2 | 安全护栏 |
| FR-9 | Epic 2 | 内置工具集 |
| FR-10 | Epic 4 | 技能提炼 |
| FR-11 | Epic 4 | 事实记忆 |
| FR-12 | Epic 4 | 用户画像 |
| FR-13 | Epic 3 | 上下文压缩 |
| FR-14 | Epic 2 | 迭代预算 |
| FR-15 | Epic 3 | 会话持久化 |
| FR-16 | Epic 5 | 子 Agent 生成 |
| FR-17 | Epic 5 | 并行编排 |
| FR-18 | 延后 | MCP（MVP 后） |
| FR-19 | Epic 1 | 错误分类与重试 |

## Epic List

### Epic 1: 项目基础设施与 LLM 通信
用户成果：可通过统一接口调用多个 LLM API，自动回退和错误恢复
**FRs covered:** FR-1, FR-2, FR-3, FR-4, FR-5, FR-19

### Epic 2: 自主 Agent 核心循环
用户成果：Agent 可自主使用工具完成任务，有迭代预算控制
**FRs covered:** FR-6, FR-7, FR-8, FR-9, FR-14

### Epic 3: 对话持久化与上下文管理
用户成果：Agent 可处理长对话，自动压缩和持久化会话
**FRs covered:** FR-13, FR-15

### Epic 4: 自学习记忆系统
用户成果：Agent 从经验中学习，跨会话记忆用户偏好
**FRs covered:** FR-10, FR-11, FR-12

### Epic 5: 多 Agent 并行编排
用户成果：Agent 可委派子任务并并行执行
**FRs covered:** FR-16, FR-17

## Epic 1: 项目基础设施与 LLM 通信

用户成果：可通过统一接口调用多个 LLM API，自动回退和错误恢复

### Story 1.1: 项目初始化与构建配置

As a 开发者,
I want 初始化 HeAgent Python 项目结构并配置构建工具,
So that 我有一个可运行的、符合 Python 最佳实践的包骨架。

**Acceptance Criteria:**

**Given** 空项目目录
**When** 执行项目初始化
**Then** 创建 `pyproject.toml`（含 python>=3.11, pydantic, httpx, openai, anthropic, click 依赖）
**And** 创建 `src/heagent/__init__.py` 入口
**And** 创建 `ruff.toml` 配置（PEP 8 强制）
**And** 创建 `.env.example` 环境变量模板
**And** `pip install -e .` 成功安装，`import heagent` 无报错

### Story 1.2: 核心类型与异常层级

As a 开发者,
I want 定义框架共享的数据类型和异常层级,
So that 所有模块使用统一的类型系统和错误处理。

**Acceptance Criteria:**

**Given** 项目结构已初始化
**When** 实现 `types.py` 和 `exceptions.py`
**Then** `types.py` 包含 Message, ProviderResponse, ToolCall, ToolResult, TokenUsage, MiddlewareProtocol 等 Pydantic model
**And** `exceptions.py` 包含 HeAgentError → ProviderError / ToolError / SafetyViolation / BudgetExceeded 异常层级
**And** 所有类型有完整类型注解，`mypy --strict` 通过

### Story 1.3: 配置管理

As a 开发者,
I want 通过 Pydantic Settings 管理 API Key 和框架参数,
So that 敏感信息从 .env 加载，非敏感参数有类型安全的默认值。

**Acceptance Criteria:**

**Given** types.py 和 exceptions.py 已完成
**When** 实现 `config.py`
**Then** `Settings` 类继承 BaseSettings，包含 openai_api_key, anthropic_api_key, default_model, max_iterations 等字段
**And** API Key 从 `.env` 文件加载，缺失时启动报错（非静默失败）
**And** 非敏感参数有合理默认值（max_iterations=50, compression_threshold=0.8）
**And** `Settings()` 实例化完成类型校验

### Story 1.4: BaseProvider Protocol

As a 开发者,
I want 定义统一的 Provider 协议接口,
So that 新 Provider 只需实现 Protocol 即可接入框架，无需修改核心代码。

**Acceptance Criteria:**

**Given** types.py 和 config.py 已完成
**When** 实现 `providers/base.py`
**Then** BaseProvider Protocol 定义 `async def send()`, `async def stream()`, `get_metadata()` 方法
**And** 所有方法签名使用 types.py 中的统一类型（Message → ProviderResponse）
**And** 新 Provider 只需实现 Protocol 方法，核心循环无需任何修改（NFR-2 验证）

### Story 1.5: OpenAI Provider

As a 开发者,
I want 实现 OpenAI 兼容 API 的 Provider,
So that 我可以通过统一接口调用 OpenAI 和兼容端点（LM Studio）。

**Acceptance Criteria:**

**Given** BaseProvider Protocol 已定义
**When** 实现 `providers/openai.py`
**Then** 支持 OpenAI ChatCompletion API 调用，返回 ProviderResponse
**And** 支持 SSE 流式响应
**And** 支持 function calling / tool_use 格式解析
**And** 支持兼容端点（通过 base_url 配置）
**And** 通过 BaseProvider Protocol 的单元测试验证

### Story 1.6: Anthropic Provider

As a 开发者,
I want 实现 Anthropic Claude API 的 Provider,
So that 我可以通过统一接口调用 Claude API。

**Acceptance Criteria:**

**Given** BaseProvider Protocol 已定义
**When** 实现 `providers/anthropic.py`
**Then** 支持 Anthropic Messages API 调用，返回 ProviderResponse
**And** 支持 SSE 流式响应
**And** 支持 tool_use 格式解析
**And** 支持基础 cache_control 提示词缓存（V1）
**And** 通过 BaseProvider Protocol 的单元测试验证

### Story 1.7: Provider 回退链与凭证轮换

As a 开发者,
I want Provider 自动回退和 API Key 轮换,
So that 单个 Provider 或 Key 故障不中断服务。

**Acceptance Criteria:**

**Given** OpenAI 和 Anthropic Provider 已实现
**When** 实现 `providers/chain.py`
**Then** ProviderChain 管理有序 Provider 列表
**And** 主 Provider 返回 429/503 时，自动激活下一个 Provider（FR-4）
**And** 回退触发时记录日志（Provider 名称、错误类型、切换目标）
**And** 单个 Key 触发速率限制时，自动切换下一个 Key（FR-5）
**And** Key 池耗尽时，触发 Provider 级别回退
**And** 所有 Provider 均不可用时，返回 ProviderError

### Story 1.8: 错误分类与差异化重试

As a 开发者,
I want API 错误自动分类并差异化处理,
So that 瞬态错误自动恢复，非瞬态错误快速失败。

**Acceptance Criteria:**

**Given** ProviderChain 和异常层级已完成
**When** 实现错误分类和重试逻辑
**Then** 429 速率限制 → 触发凭证轮换或 Provider 回退
**And** 401 认证错误 → 跳过当前 Provider
**And** 瞬态错误（网络超时、503）→ 指数退避重试（base=1s, max=30s, 最多 3 次）
**And** 非瞬态错误 → 立即返回错误
**And** 重试日志记录每次尝试的错误类型和延迟

## Epic 2: 自主 Agent 核心循环

用户成果：Agent 可自主使用工具完成任务，有迭代预算控制

### Story 2.1: 声明式工具注册

As a 开发者,
I want 通过 @tool 装饰器声明工具并自动生成参数 schema,
So that 添加新工具零样板代码，LLM 自动获得工具描述。

**Acceptance Criteria:**

**Given** 项目结构和类型系统已完成
**When** 实现 `tools/decorator.py` 和 `tools/registry.py`
**Then** `@tool` 装饰器自动提取函数签名、类型注解、docstring 生成 JSON Schema
**And** ToolRegistry 单例管理所有注册工具
**And** 注册的工具自动出现在 LLM 可用工具列表中
**And** 添加新工具不需要修改任何其他代码（NFR-2 验证）

### Story 2.2: 终端执行工具

As a 开发者,
I want Agent 能执行 shell 命令并返回结果,
So that Agent 可以与系统终端交互完成任务。

**Acceptance Criteria:**

**Given** @tool 注册机制已完成
**When** 实现 `tools/builtins/shell.py`
**Then** 通过 @tool 注册，自动出现在可用工具列表
**And** 执行命令返回 stdout, stderr, exit_code
**And** 有超时控制（默认 120 秒）
**And** 可独立启用/禁用

### Story 2.3: 文件操作工具

As a 开发者,
I want Agent 能读写和搜索文件,
So that Agent 可以处理文件系统任务。

**Acceptance Criteria:**

**Given** @tool 注册机制已完成
**When** 实现 `tools/builtins/file.py` 和 `tools/builtins/search.py`
**Then** 文件读写工具支持 read/write 操作
**And** 搜索工具支持文件名搜索和内容搜索
**And** 所有内置工具通过 @tool 注册，可独立启用/禁用

### Story 2.4: 安全护栏

As a 开发者,
I want 工具执行前进行安全检查,
So that 危险操作被拦截，防止 Agent 执行破坏性命令。

**Acceptance Criteria:**

**Given** 工具注册和内置工具已完成
**When** 实现 `tools/safety.py`
**Then** 终端工具检测到危险命令（rm -rf, format, dd 等）时暂停执行并请求确认
**And** 用户可配置白名单模式（仅允许列表内命令）或黑名单模式（阻止列表内命令）
**And** 拦截事件记录日志
**And** 安全检查在工具执行前自动触发，不需手动调用

### Story 2.5: Agent 核心循环与迭代预算

As a 开发者,
I want Agent 能自主循环调用 LLM 和工具直到任务完成,
So that 复杂任务可以通过多轮推理和工具调用自动完成。

**Acceptance Criteria:**

**Given** Provider 层和工具系统已完成
**When** 实现 `agent/loop.py` 和 `agent/middleware.py`
**Then** Agent 循环：发送消息给 LLM → 解析 tool_calls → 执行工具 → 将结果回传 LLM → 重复
**And** 中间件管道 `(Request, NextFn) -> Response` 可插入重试、日志等横切逻辑
**And** 每次工具调用消耗一次迭代，达到上限（默认 50）时强制输出最终响应（FR-14）
**And** LLM 无 tool_calls 时，循环终止并返回最终响应
**And** 核心循环模块 <1,500 行（NFR-1 验证）

### Story 2.6: 工具并行执行

As a 开发者,
I want 无依赖的工具调用并行执行,
So that 多个工具调用高效完成，缩短响应时间。

**Acceptance Criteria:**

**Given** Agent 核心循环已完成
**When** LLM 返回多个 tool_calls
**Then** 无依赖关系的工具调用并行执行
**And** 所有结果收集完毕后才提交给 LLM 进入下一轮
**And** 单个工具执行失败不影响其他工具的并行执行
**And** 失败的工具返回 ToolError 结果，不中断循环

## Epic 3: 对话持久化与上下文管理

用户成果：Agent 可处理长对话，自动压缩和持久化会话

### Story 3.1: 会话持久化

As a 开发者,
I want 对话历史可保存和恢复,
So that 我可以跨会话继续之前的对话。

**Acceptance Criteria:**

**Given** Agent 核心循环已完成
**When** 实现 `context/session.py`
**Then** 对话历史以 JSON 格式保存到 `.heagent/sessions/` 目录
**And** 可通过 session ID 恢复之前的对话上下文
**And** 会话文件包含完整消息历史和工具调用记录
**And** 会话加载后 Agent 可无缝继续对话

### Story 3.2: 自动上下文压缩

As a 开发者,
I want 对话 token 接近模型窗口阈值时自动压缩历史,
So that 长对话不会因 token 超限而失败。

**Acceptance Criteria:**

**Given** Provider 层和 Agent 循环已完成
**When** 实现 `context/compressor.py`
**Then** token 使用量达到 80% 阈值时自动触发压缩
**And** 压缩使用主模型总结中间对话轮次（V1）
**And** 最近 N 轮对话不压缩（保留活跃上下文）
**And** 压缩后 token 使用量显著下降，对话可继续
**And** 压缩阈值可配置

## Epic 4: 自学习记忆系统

用户成果：Agent 从经验中学习，跨会话记忆用户偏好

### Story 4.1: 技能提炼

As a 开发者,
I want Agent 从任务执行中提炼可复用操作模式,
So that 成功的操作经验可以在后续对话中复用。

**Acceptance Criteria:**

**Given** Agent 核心循环已完成
**When** 实现 `memory/skills.py`
**Then** 每轮对话结束后，系统自动评估是否产生可提炼的操作模式
**And** 提炼的 Skill 以 SKILL.md 格式保存到 `.heagent/skills/` 目录
**And** 后续对话中，匹配的 Skill 自动注入系统提示词
**And** Skill 文件包含名称、描述、适用场景、操作步骤

### Story 4.2: 事实记忆管理

As a 开发者,
I want Agent 跨会话记住用户偏好和项目事实,
So that Agent 可以基于历史记忆提供更精准的回复。

**Acceptance Criteria:**

**Given** Agent 核心循环已完成
**When** 实现 `memory/facts.py`
**Then** Agent 可在对话中主动保存事实到 `.heagent/memory/MEMORY.md`
**And** 新会话启动时自动加载 MEMORY.md 内容到上下文
**And** 记忆条目通过关键词匹配去重，避免重复积累（V1）
**And** 记忆文件格式为 Markdown，人类可读可编辑

### Story 4.3: 用户画像

As a 开发者,
I want Agent 了解用户的技术背景和偏好,
So that 回复风格和技术深度可个性化适配。

**Acceptance Criteria:**

**Given** 事实记忆系统已完成
**When** 实现 `memory/profile.py`
**Then** Agent 可在对话中更新 `.heagent/user/USER.md` 的画像条目
**And** 新会话启动时自动加载 USER.md 到系统提示词
**And** 用户画像包含技术背景、偏好风格、交互习惯等维度
**And** 画像信息影响 Agent 的回复风格和技术深度

## Epic 5: 多 Agent 并行编排

用户成果：Agent 可委派子任务并并行执行

### Story 5.1: 子 Agent 生成与执行

As a 开发者,
I want Agent 可委派任务给隔离的子 Agent 实例,
So that 子任务有独立上下文和预算，不影响父 Agent 状态。

**Acceptance Criteria:**

**Given** Agent 核心循环、工具系统和上下文管理已完成
**When** 实现 `agent/sub.py`
**Then** 子 Agent 有独立的对话上下文和迭代预算
**And** 子 Agent 默认继承父 Agent 的全部工具集
**And** 子 Agent 执行完毕后返回结果给父 Agent
**And** 子 Agent 通过 `asyncio.create_task()` 创建

### Story 5.2: 并行子 Agent 编排

As a 开发者,
I want 多个子 Agent 可并行执行并汇总结果,
So that 复杂任务可以拆分为并行子任务加速完成。

**Acceptance Criteria:**

**Given** 子 Agent 生成已完成
**When** 实现并行编排逻辑
**Then** 多个子 Agent 通过 `asyncio.gather()` 并行运行
**And** 所有子 Agent 结果收集完毕后汇总给父 Agent
**And** 单个子 Agent 失败不阻塞其他子 Agent
**And** 失败的子 Agent 返回错误结果，父 Agent 收到完整的成功/失败汇总
