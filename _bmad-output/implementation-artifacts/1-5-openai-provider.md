# Story 1.5: OpenAI Provider

Status: done

## Story

As a 开发者,
I want 实现 OpenAI 兼容 API 的 Provider,
So that 我可以通过统一接口调用 OpenAI 和兼容端点（LM Studio）。

## Acceptance Criteria

1. `providers/openai.py` 实现 `OpenAIProvider` 类，满足 `BaseProvider` Protocol
2. 支持 OpenAI ChatCompletion API 调用，返回 `ProviderResponse`
3. 支持 SSE 流式响应，`stream()` 返回 `AsyncIterator[ProviderResponse]`
4. 支持 function calling / tool_use 格式解析
5. 支持兼容端点（通过 `base_url` 配置）
6. 通过 BaseProvider Protocol 的 structural subtyping 测试验证
7. `mypy --strict` 通过，单元测试覆盖

## Tasks / Subtasks

- [ ] Task 1: 实现 OpenAIProvider 类 (AC: #1, #2, #5)
- [ ] Task 2: 实现 stream() (AC: #3)
- [ ] Task 3: 实现 tool_calls 解析 (AC: #4)
- [ ] Task 4: 实现 get_metadata() (AC: #1)
- [ ] Task 5: 编写单元测试 (AC: #6, #7)
- [ ] Task 6: 运行验证 (AC: #7)

## Dev Notes

### Architecture Constraints
- 实现 BaseProvider Protocol，不需要显式继承
- 使用 openai SDK v2.37+ AsyncOpenAI 客户端
- base_url 参数支持兼容端点
- arguments JSON 字符串 → dict 解析

### File Location
- 源码：`src/heagent/providers/openai.py`（NEW）
- 测试：`tests/providers/test_openai.py`（NEW）

### Anti-Patterns
- 不添加回退逻辑 — 属于 ProviderChain
- 不直接用 httpx — 用 openai SDK
- 不硬编码 API key — 构造函数注入

### References
- [Source: base.py] — BaseProvider Protocol
- [Source: config.py] — Settings

## Dev Agent Record

### Agent Model Used

### Completion Notes List

### File List
