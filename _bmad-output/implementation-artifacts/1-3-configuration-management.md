# Story 1.3: 配置管理

Status: review

## Story

As a 开发者,
I want 通过 Pydantic Settings 管理 API Key 和框架参数,
So that 敏感信息从 .env 加载，非敏感参数有类型安全的默认值。

## Acceptance Criteria

1. `config.py` 中 `Settings` 类继承 `pydantic_settings.BaseSettings`，包含所有框架配置字段
2. API Key 字段（`openai_api_key`, `anthropic_api_key`）从 `.env` 文件/环境变量加载，缺失时类型为 `str | None = None`（不阻断启动，由 Provider 层在使用时校验）
3. 非敏感参数有合理默认值：`max_iterations=50`、`compression_threshold=0.8`、`default_model="gpt-4o"`、`shell_timeout=120`
4. 支持多 Key 配置：`openai_api_keys: str` 存储原始值，`openai_key_pool` 属性解析逗号分隔列表
5. `Settings()` 实例化完成类型校验，无效值（如 threshold > 1.0）触发 `ValidationError`
6. 提供 `get_settings()` 模块级函数，返回缓存的 Settings 单例
7. `mypy --strict` 通过，单元测试覆盖字段默认值、环境变量加载、校验失败

## Tasks / Subtasks

- [x] Task 1: 实现 Settings 类 (AC: #1, #2, #3, #4)
  - [x] 继承 `pydantic_settings.BaseSettings`，配置 `model_config = SettingsConfigDict(env_file='.env', env_file_encoding='utf-8', extra='ignore')`
  - [x] API Key 字段：`openai_api_key: str | None = None`、`anthropic_api_key: str | None = None`
  - [x] 多 Key 字段：`openai_api_keys: str = ""`，通过 `openai_key_pool` 属性解析为列表
  - [x] 框架参数字段：`max_iterations: int = 50`、`compression_threshold: float = 0.8`、`default_model: str = "gpt-4o"`、`shell_timeout: int = 120`
  - [x] 重试参数字段：`retry_max_attempts: int = 3`、`retry_base_delay: float = 1.0`、`retry_max_delay: float = 30.0`
- [x] Task 2: 实现 get_settings() 单例函数 (AC: #6)
  - [x] 模块级 `_settings: Settings | None = None` 缓存
  - [x] `get_settings()` 首次调用创建实例，后续返回缓存
  - [x] `reset_settings()` 用于测试重置
- [x] Task 3: 实现 Key 解析逻辑 (AC: #4)
  - [x] `_parse_comma_list()` 辅助函数处理逗号分隔字符串
  - [x] 空字符串返回空列表
  - [x] 通过 `@property` 提供 `openai_key_pool` / `anthropic_key_pool`
- [x] Task 4: 编写单元测试 (AC: #7)
  - [x] `test_config.py` — 24 个测试覆盖默认值、环境变量加载、校验失败、单例行为
  - [x] 使用 `monkeypatch.setenv()` 设置测试环境变量
  - [x] 测试 `compression_threshold` 范围校验（0.0~1.0）
  - [x] 测试多 Key 逗号分隔解析
- [x] Task 5: 运行验证 (AC: #7)
  - [x] `mypy --strict` 通过
  - [x] `pytest` 36/36 全部通过（含已有测试无回归）

## Dev Notes

### Architecture Constraints
- 使用 `pydantic_settings.BaseSettings`（来自 `pydantic-settings>=2.0` 包，已声明在 `pyproject.toml`）
- `model_config` 使用 `SettingsConfigDict`（pydantic-settings v2 API）
- `extra='ignore'` 确保 `.env` 中有额外变量时不报错
- API Key 不阻断启动 — 运行时由 Provider 层在使用时校验是否为 None
- 遵循架构决策：配置在程序入口一次性全量校验 [Source: architecture.md#Process Patterns]

### Implementation Decision
- 多 Key 字段使用 `str` 类型存储原始值，通过 `@property` 提供 `key_pool` 列表访问
- 原因：pydantic-settings 对 `list[str]` 类型环境变量尝试 JSON 解析，`"sk-1,sk-2"` 不是合法 JSON
- 字段约束使用 `Field(ge=1)` 等验证器，而非自定义 validator，更简洁

### Anti-Patterns to Avoid
- 不使用 `python-dotenv` 直接加载 — `pydantic-settings` 内置 `.env` 支持
- 不在 `config.py` 中添加业务逻辑 — 只做配置定义和加载
- 不将 API Key 设为必填 — 这会阻断无特定 Provider 的场景
- 不使用 `os.environ.get()` 手动读取 — 统一走 `BaseSettings` 机制

## Dev Agent Record

### Agent Model Used
Claude (GLM-5.1)

### Debug Log References
- 初始实现使用 `list[str]` + `@field_validator` 解析多 Key，但 pydantic-settings 在 env source 层先尝试 JSON 解析，导致 `SettingsError`
- 修复：改用 `str` 存储原始值 + `@property` 提供 `key_pool` 列表

### Completion Notes List
- Settings 类完整实现：11 个配置字段，含 Pydantic Field 约束
- get_settings() / reset_settings() 单例模式
- _parse_comma_list() 辅助函数处理逗号分隔
- .env.example 更新包含所有新字段
- 24 个新测试 + 12 个已有测试全部通过
- mypy --strict 无错误

### File List
- NEW: HeAgent/src/heagent/config.py
- NEW: HeAgent/tests/test_config.py
- MODIFIED: HeAgent/.env.example
