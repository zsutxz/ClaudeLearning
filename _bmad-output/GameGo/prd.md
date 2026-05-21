---
title: 五子棋 — AI 加强与功能完善
created: 2026-05-21
updated: 2026-05-21
status: draft
---

# PRD: 五子棋 — AI 加强与功能完善

## 0. 文档目的

面向开发者本人，定义 v2 迭代的范围和需求。v1 MVP 已完成（棋盘、落子、胜负判定、PvP/PvAI、SimpleAI、MinimaxAI）。本迭代聚焦 AI 难度分级、GameConfig 运行时接入、悔棋功能。输入来源：`game-brief.md`、源码分析。

## 1. Vision

v1 已实现可玩的五子棋。v2 要让它**值得反复对弈**——三档 AI 难度让新手到进阶都有挑战，悔棋降低挫败感，GameConfig 统一配置让后续迭代更快。核心目标：从"能玩"升级为"好玩"。

## 2. Target User

### 2.1 Primary Persona

**Tan** — 独立开发者，兼职开发。既是构建者也是主要玩家。用这个项目学习 Unity 和 AI 算法，同时享受策略对弈的乐趣。

### 2.2 Jobs To Be Done

- 对弈时找到合适难度的 AI 对手（不太弱也不碾压）
- 下错一步时能撤回，继续探索策略
- 快速调整游戏参数而不改代码

## 3. Glossary

- **棋盘 (Board)** — 15×15 的 PieceType 二维数组，纯数据模型
- **落子 (PlacePiece)** — 在空位放置黑棋或白棋
- **悔棋 (Undo)** — 撤回最近一步落子，恢复棋盘和回合状态
- **AI 难度 (AIDifficulty)** — Simple / Medium / Hard 三档
- **GameConfig** — ScriptableObject，集中管理棋盘、视觉、游戏、音频、动画参数
- **SimpleAI** — 规则优先级链 AI，贪心策略
- **MediumAI** — 新增中等难度 AI（待设计）
- **MinimaxAI** — Minimax + Alpha-Beta 剪枝 AI，搜索深度可调
- **GameManager** — 游戏流程控制器，持有 Board、IAIPlayer、状态
- **BoardView** — 棋盘视图，管理 CellView 网格

## 4. Features

### 4.1 AI 三档难度

**Description:** 游戏开始前和结束后，玩家可在 UI 上选择 AI 难度：简单、中等、困难。选择后立即生效并开始新一局。难度通过 `AIDifficulty` 枚举和 `IAIPlayer` 策略模式切换。Realizes UJ-1.

**Functional Requirements:**

#### FR-1: 三档 AI 策略实现

系统提供 Simple、Medium、Hard 三种 AI 策略，通过 `IAIPlayer` 接口统一调度。

**Consequences:**
- Simple 策略与当前 SimpleAI 行为一致
- Medium 策略对弈强度介于 Simple 和 Hard 之间
- Hard 策略使用 MinimaxAI，搜索深度 ≥ 3

**Out of Scope:** 在线 AI、机器学习 AI

#### FR-2: AI 难度 UI 选择

玩家可在游戏左上角通过按钮切换 AI 难度，当前选中项有明确标记。仅 PvAI 模式显示。切换后自动开始新一局。

**Consequences:**
- PvAI 模式下左上角显示"简单/中等/困难"三个按钮
- 当前选中项有 ● 标记
- 点击任意按钮立即重启游戏并使用对应 AI
- PvP 模式下不显示难度按钮

#### FR-3: MinimaxAI 搜索深度提升

Hard 难度的 MinimaxAI 搜索深度从 2 提升到 3。

**Consequences:**
- Hard AI 在搜索深度 3 下仍能在 2 秒内返回落子
- 落子质量显著高于 depth=2 的表现

### 4.2 悔棋功能

**Description:** PvAI 模式下，玩家可撤回最近一步（同时撤回 AI 的回应，共两步）。PvP 模式下撤回最近一步。悔棋恢复棋盘状态、回合和视觉。Realizes UJ-2.

**Functional Requirements:**

#### FR-4: 悔棋操作

玩家点击悔棋按钮后，棋盘恢复到上一步的状态。

**Consequences:**
- PvAI 模式：撤回玩家落子 + AI 回应（共两步），轮回到玩家回合
- PvP 模式：撤回最近一步，轮回到上一个玩家
- 游戏已结束时不可悔棋（弹框状态下按钮不可用）
- 棋盘为空时不可悔棋
- 悔棋后棋子动画正常、最后落子标记正确更新
- 连续悔棋可撤回整局（直到棋盘为空）

**Out of Scope:** 选择性悔棋（撤回特定步骤）、悔棋次数限制

#### FR-5: 悔棋 UI

左上角显示悔棋按钮，游戏进行中可点击。

**Consequences:**
- 游戏进行中按钮可点击
- 棋盘为空或游戏结束时按钮置灰/不可点击
- 点击后立即生效，无确认弹框

### 4.3 GameConfig 运行时接入

**Description:** 已有的 GameConfig ScriptableObject 从"仅 Editor 预览"升级为运行时各组件的配置来源。一次配置，全局生效。Realizes UJ-3.

**Functional Requirements:**

#### FR-6: GameManager 从 GameConfig 读取设置

GameManager 启动时从 GameConfig 读取 defaultGameMode、aiFirst、defaultAIDifficulty。

**Consequences:**
- GameManager 的 gameMode、aiFirst、aiDifficulty 初始值来自 GameConfig
- Inspector 上的 SerializeField 仍可覆盖（Editor 调试用）
- GameConfig 为 null 时使用硬编码默认值，不报错

#### FR-7: BoardView 从 GameConfig 读取视觉参数

BoardView 初始化时从 GameConfig 读取 cellSize、pieceScale、boardColor、hoverColor、lastMoveColor、winningColor。

**Consequences:**
- 棋盘格子大小、棋子缩放、颜色均由 GameConfig 控制
- GameConfig 为 null 时使用现有 SerializeField 默认值

## 5. Non-Goals (Explicit)

- 在线对战 / 网络功能
- 教程系统 / 策略提示
- 多种棋盘皮肤
- 棋局保存/加载
- AudioManager 接入（下个迭代）
- 移动端适配

## 6. MVP Scope

### 6.1 In Scope

- 三档 AI（Simple / Medium / Hard）
- AI 难度 UI 切换
- MinimaxAI depth=3
- 悔棋（PvAI 撤两步，PvP 撤一步）
- 悔棋 UI 按钮
- GameManager 从 GameConfig 读取设置
- BoardView 从 GameConfig 读取视觉参数

### 6.2 Out of Scope for MVP

- AudioManager 接入 — 方法已定义但无人调用，需单独迭代
- Board.Grid 封装 — 影响面广，非功能性需求
- 测试启用 — GomokuTests 当前全部注释，需安装 Test Framework

## 7. Success Metrics

**Primary**
- **SM-1**: 能与三档 AI 分别完成一局对弈，难度体感递增。Validates FR-1, FR-2.
- **SM-2**: 悔棋后棋盘状态与视觉完全一致，可连续悔棋到空盘。Validates FR-4, FR-5.
- **SM-3**: 修改 GameConfig asset 后运行游戏，参数生效。Validates FR-6, FR-7.

**Counter-metrics**
- **SM-C1**: Hard AI 落子时间不超过 2 秒 — 不能为追求强度牺牲响应速度。Counterbalances SM-1.

## 8. Open Questions

1. MediumAI 具体算法选择？可选方案：SimpleAI + 部分防守规则增强、MinimaxAI depth=1、或混合策略。
2. 悔棋是否需要快捷键支持？（当前仅有 UI 按钮）

## 9. Assumptions Index

- `[ASSUMPTION]` MediumAI 可通过 SimpleAI 增强规则或浅层 Minimax 实现，不需要全新算法 — §4.1 FR-1
- `[ASSUMPTION]` MinimaxAI depth=3 在 15×15 棋盘上候选位置有限时可在 2 秒内完成 — §4.1 FR-3
- `[ASSUMPTION]` GameConfig 运行时接入不需要热重载（改配置需重启游戏）— §4.3 FR-6, FR-7
