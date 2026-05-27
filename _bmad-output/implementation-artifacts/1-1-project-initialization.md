# Story 1.1: 项目初始化与构建配置

Status: done

## Story

As a 开发者,
I want 初始化 HeAgent Python 项目结构并配置构建工具,
So that 我有一个可运行的、符合 Python 最佳实践的包骨架。

## Acceptance Criteria

1. `pyproject.toml` 包含 python>=3.11 和核心依赖（pydantic>=2.13, httpx>=0.28, openai>=2.37, anthropic>=0.104, click>=8.0, pydantic-settings>=2.0），且 `pip install -e .` 成功
2. `src/heagent/__init__.py` 入口文件存在，`import heagent` 无报错
3. `ruff.toml` 配置 PEP 8 强制规则（line-length=120, target python 3.11）
4. `.env.example` 包含 OPENAI_API_KEY, ANTHROPIC_API_KEY, DEFAULT_MODEL, MAX_ITERATIONS 模板
5. `.gitignore` 排除 `.env`, `__pycache__/`, `.heagent/`, `*.egg-info/`, `.mypy_cache/`, `.pytest_cache/`
6. `pytest` 和 `pytest-asyncio` 作为开发依赖安装，`pytest` 可运行（0 tests collected）

## Tasks / Subtasks

- [x] Task 1: 创建 pyproject.toml (AC: #1, #6)
  - [x] 配置 [project] 元数据：name="heagent", version="0.1.0", requires-python=">=3.11"
  - [x] 配置核心依赖：pydantic>=2.13, httpx>=0.28, openai>=2.37, anthropic>=0.104, click>=8.0, pydantic-settings>=2.0
  - [x] 配置开发依赖：pytest>=8.0, pytest-asyncio>=0.23, ruff, mypy
  - [x] 配置 [build-system] 使用 hatchling
  - [x] 配置 [tool.pytest.ini_options] asyncio_mode="auto"
- [x] Task 2: 创建 src layout 目录结构 (AC: #2)
  - [x] 创建 src/heagent/__init__.py（版本号常量 __version__）
  - [x] 创建 src/heagent/providers/__init__.py
  - [x] 创建 src/heagent/tools/__init__.py
  - [x] 创建 src/heagent/tools/builtins/__init__.py
  - [x] 创建 src/heagent/memory/__init__.py
  - [x] 创建 src/heagent/context/__init__.py
  - [x] 创建 src/heagent/agent/__init__.py
  - [x] 创建 tests/conftest.py
- [x] Task 3: 配置 ruff.toml (AC: #3)
  - [x] line-length=120, target-version="py311"
  - [x] 启用规则：E, F, I (isort), UP, B, SIM, TCH
  - [x] 配置 isort known-first-party = ["heagent"]
- [x] Task 4: 创建配置文件模板 (AC: #4, #5)
  - [x] 创建 .env.example
  - [x] 创建 .gitignore
- [x] Task 5: 验证安装和基础测试 (AC: #1, #2, #6)
  - [x] pip install -e . 成功
  - [x] python -c "import heagent; print(heagent.__version__)" 输出 0.1.0
  - [x] pytest 运行无错误（0 tests collected）

## Dev Notes

### Architecture Constraints
- **src layout**: 代码在 `src/heagent/` 下，不是顶层 `heagent/` [Source: architecture.md#Project Structure]
- **Python 3.11+**: 利用原生 async/await、match-case [Source: architecture.md#Technical Constraints]
- **依赖版本已锁定**: pydantic v2.13.4, httpx v0.28.1, openai v2.37.0, anthropic v0.104.0 [Source: architecture.md#依赖版本锁定]
- **构建工具**: uv 或 pip，不引入 poetry/pdm 等重型工具
- **测试框架**: pytest + pytest-asyncio，asyncio_mode="auto" 避免每处加 @pytest.mark.asyncio [Source: architecture.md#构建 & 开发]

### Project Structure Notes
- 目录结构严格遵循 architecture.md#Complete Project Directory Structure
- 每个 `__init__.py` 保持最小（空或版本号），后续 Story 填充导出
- tests/ 目录镜像 src/heagent/ 结构

### Anti-Patterns to Avoid
- 不用 `setup.py` / `setup.cfg`，统一用 `pyproject.toml`
- 不在 `__init__.py` 中做重度导入（保持轻量）
- 不添加 architecture.md 中未列出的依赖

### References
- [Source: HeAgent/docs/architecture-HeAgent-2026-05-23/architecture.md#Complete Project Directory Structure]
- [Source: HeAgent/docs/architecture-HeAgent-2026-05-23/architecture.md#依赖版本锁定]
- [Source: HeAgent/docs/prd-HeAgent-2026-05-23/prd.md#6. MVP 范围]

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List
- All 5 tasks completed, all 6 ACs verified
- pip install -e ".[dev]" successful
- import heagent outputs 0.1.0
- pytest runs with 0 tests collected (expected for project init)

### File List
- HeAgent/pyproject.toml (NEW)
- HeAgent/src/heagent/__init__.py (NEW)
- HeAgent/src/heagent/providers/__init__.py (NEW)
- HeAgent/src/heagent/tools/__init__.py (NEW)
- HeAgent/src/heagent/tools/builtins/__init__.py (NEW)
- HeAgent/src/heagent/memory/__init__.py (NEW)
- HeAgent/src/heagent/context/__init__.py (NEW)
- HeAgent/src/heagent/agent/__init__.py (NEW)
- HeAgent/tests/conftest.py (NEW)
- HeAgent/ruff.toml (NEW)
- HeAgent/.env.example (NEW)
- HeAgent/.gitignore (NEW)
