# Story 1.2: 核心类型与异常层级

Status: done

## Story

As a 开发者,
I want 定义框架共享的数据类型和异常层级,
So that 所有模块使用统一的类型系统和错误处理。

## Acceptance Criteria

1. types.py 包含 Message, Role Pydantic model，Message 支持 user/assistant/system/tool 角色
2. types.py 包含 ProviderResponse Pydantic model（content, tool_calls, usage, model, finish_reason）
3. types.py 包含 ToolCall, ToolResult, TokenUsage Pydantic model
4. exceptions.py 包含 HeAgentError 基类和 ProviderError, ToolError, SafetyViolation, BudgetExceeded 子类
5. 所有类型有完整类型注解，mypy --strict 通过
6. 单元测试覆盖所有 model 实例化和异常层级

## Tasks / Subtasks

- [ ] Task 1: 实现 types.py (AC: #1, #2, #3, #5)
  - [ ] Role enum (USER, ASSISTANT, SYSTEM, TOOL)
  - [ ] Message model (role, content, name?, tool_call_id?, tool_calls?)
  - [ ] TokenUsage model (prompt_tokens, completion_tokens, total_tokens)
  - [ ] ToolCall model (id, name, arguments as dict)
  - [ ] ToolResult model (tool_call_id, content, is_error)
  - [ ] ProviderResponse model (content, tool_calls list, usage, model, finish_reason)
- [ ] Task 2: 实现 exceptions.py (AC: #4, #5)
  - [ ] HeAgentError 基类（含 message 属性）
  - [ ] ProviderError, ToolError, SafetyViolation, BudgetExceeded 子类
- [ ] Task 3: 编写单元测试 (AC: #6)
  - [ ] test_types.py — 所有 model 实例化和字段验证
  - [ ] test_exceptions.py — 异常层级继承和消息
- [ ] Task 4: 运行 mypy 和 pytest 验证 (AC: #5, #6)
  - [ ] mypy --strict 通过
  - [ ] pytest 全部通过

## Dev Notes

### Architecture Constraints
- 所有数据模型使用 Pydantic BaseModel，不用 dict 或 dataclass
- 异常层级：HeAgentError → ProviderError / ToolError / SafetyViolation / BudgetExceeded
- JSON 字段命名 snake_case
- types.py 和 exceptions.py 是叶子模块，无内部依赖

### Anti-Patterns to Avoid
- 不用 dataclass 或 TypedDict 替代 Pydantic BaseModel
- 不在 types.py 中 import 其他 heagent 模块
- 不添加 business logic 到 model 中

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
