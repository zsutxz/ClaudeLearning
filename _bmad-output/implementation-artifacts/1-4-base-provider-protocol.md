# Story 1.4: BaseProvider Protocol

Status: done

## Story

As a 开发者,
I want 定义统一的 Provider 协议接口,
So that 新 Provider 只需实现 Protocol 即可接入框架，无需修改核心代码。

## Acceptance Criteria

1. `providers/base.py` 定义 `BaseProvider` Protocol，包含 `async def send()`、`async def stream()`、`get_metadata()` 方法
2. `send()` 接受 `list[Message]` + 可选 `tools` 参数，返回 `ProviderResponse`
3. `stream()` 接受相同参数，返回 `AsyncIterator[ProviderResponse]`
4. `get_metadata()` 返回 `ProviderMetadata`（name, model, supports_streaming, supports_tools）
5. 所有方法签名使用 `types.py` 中的统一类型
6. 新 Provider 只需实现 Protocol 方法 — 通过 structural subtyping 测试验证
7. `providers/__init__.py` 导出 `BaseProvider` 和 `ProviderMetadata`
8. `mypy --strict` 通过，49 个测试全部通过

## Tasks / Subtasks

- [x] Task 1: 实现 ProviderMetadata model (AC: #4)
- [x] Task 2: 实现 BaseProvider Protocol (AC: #1, #2, #3, #5)
- [x] Task 3: 定义 ToolSchema 类型 (AC: #2) — 放在 types.py
- [x] Task 4: 更新 providers/__init__.py 导出 (AC: #7)
- [x] Task 5: 编写单元测试 (AC: #6, #8) — 7 个测试
- [x] Task 6: 运行验证 (AC: #8) — 49/49 通过，mypy strict 无错误

## Dev Notes

### Implementation Decisions
- 使用 `typing.Protocol` + `@runtime_checkable` 而非 ABC — 支持 structural subtyping
- ProviderMetadata 放在 `base.py` 而非 `types.py` — 它是 Provider 层特有概念
- ToolSchema 放在 `types.py` — 工具系统和 Provider 都需要引用

## Dev Agent Record

### Agent Model Used
Claude (GLM-5.1)

### Completion Notes List
- BaseProvider Protocol 定义 3 个方法：send, stream, get_metadata
- ProviderMetadata Pydantic model 含 4 字段（2 有默认值）
- ToolSchema 新增到 types.py
- providers/__init__.py 导出更新
- 7 个新测试覆盖 metadata、tool schema、structural subtyping、send/stream 交互

### File List
- NEW: HeAgent/src/heagent/providers/base.py
- NEW: HeAgent/tests/providers/test_base.py
- MODIFIED: HeAgent/src/heagent/types.py (新增 ToolSchema)
- MODIFIED: HeAgent/src/heagent/providers/__init__.py (导出更新)
