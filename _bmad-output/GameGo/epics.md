---
stepsCompleted: [1, 2, 3]
inputDocuments:
  - _bmad-output/GameGo/prd.md
  - _bmad-output/GameGo/architecture-五子棋.md
---

# 五子棋 — AI 加强与功能完善 - Epic Breakdown

## Overview

本文档将 PRD 需求和架构决策分解为可实施的 Epic 和 Story。

## Requirements Inventory

### Functional Requirements

FR-1: 三档 AI 策略实现（Simple / Medium / Hard），通过 IAIPlayer 接口统一调度。Simple 保持现有 SimpleAI 行为；Medium 强度介于 Simple 和 Hard 之间；Hard 使用 MinimaxAI，搜索深度 ≥ 3。

FR-2: AI 难度 UI 选择。左上角显示"简单/中等/困难"三个按钮，当前选中项有 ● 标记。仅 PvAI 模式显示。点击任意按钮立即重启游戏并使用对应 AI。

FR-3: MinimaxAI 搜索深度从 2 提升到 3，Hard AI 落子时间 ≤ 2 秒，落子质量显著高于 depth=2。

FR-4: 悔棋操作。PvAI 模式撤回玩家落子 + AI 回应（共两步）；PvP 模式撤回最近一步。游戏结束时不可悔棋，棋盘为空时不可悔棋。可连续悔棋到空盘。悔棋后视觉正确更新。

FR-5: 悔棋 UI 按钮。左上角显示，游戏进行中可点击，空盘或游戏结束时置灰。点击立即生效，无确认弹框。

FR-6: GameManager 从 GameConfig 读取 defaultGameMode、aiFirst、defaultAIDifficulty。GameConfig 为 null 时使用硬编码默认值。SerializeField 仍可 Editor 覆盖。

FR-7: BoardView 从 GameConfig 读取 cellSize、pieceScale、颜色参数。GameConfig 为 null 时使用现有 SerializeField 默认值。

### NonFunctional Requirements

NFR-1: Hard AI（MinimaxAI depth=3）落子时间不超过 2 秒。

NFR-2: GameConfig 为 null 时优雅降级，各组件使用硬编码默认值，不报错。

NFR-3: 保持现有 MVC + 策略模式架构，不引入新的设计模式。

### Additional Requirements

- AI 切换使用策略模式（已有 IAIPlayer 接口），通过 AIDifficulty 枚举 + 工厂逻辑选择实例
- MinimaxAI 需支持构造函数参数配置搜索深度（从 MAX_DEPTH 常量改为实例字段）
- 事件驱动通信保持不变（OnPiecePlaced 等四个 UnityEvent）
- 悔棋需维护落子历史栈（Stack<(int, int, PieceType)>），Board 需已有 RemovePiece 方法（已存在）

### UX Design Requirements

无独立 UX 文档。UI 使用现有 OnGUI 即时模式渲染。

### FR Coverage Map

FR-1: Epic 1 — 三档 AI 策略
FR-2: Epic 1 — AI 难度 UI
FR-3: Epic 1 — MinimaxAI depth=3
FR-4: Epic 2 — 悔棋逻辑
FR-5: Epic 2 — 悔棋 UI
FR-6: Epic 2 — GameManager 接 GameConfig
FR-7: Epic 2 — BoardView 接 GameConfig

## Epic List

### Epic 1: AI 难度系统
玩家可选择简单/中等/困难三档 AI，获得递增的对弈挑战。
**FRs covered:** FR-1, FR-2, FR-3

### Epic 2: 悔棋与配置统一
玩家可悔棋撤回失误操作；开发者通过 GameConfig 统一管理游戏参数。
**FRs covered:** FR-4, FR-5, FR-6, FR-7

## Epic 1: AI 难度系统

### Story 1.1: AIDifficulty 枚举与 MinimaxAI 深度可配置

As a 玩家,
I want AI 有简单、中等、困难三种难度可选,
So that 我能找到适合自己水平的对手。

**Acceptance Criteria:**

**Given** PieceType.cs 中已定义 PieceType、GameState、GameMode 枚举
**When** 添加 AIDifficulty 枚举（Simple, Medium, Hard）
**Then** 项目编译通过，AIDifficulty 可被其他类引用

**Given** MinimaxAI 使用 `const int MAX_DEPTH = 2`
**When** 改为构造函数参数 `public MinimaxAI(int maxDepth = 3)`
**Then** `new MinimaxAI()` 默认深度为 3，`new MinimaxAI(1)` 深度为 1，项目编译通过

### Story 1.2: GameManager 三档 AI 工厂逻辑

As a 玩家,
I want 选择不同 AI 难度后游戏使用对应的 AI 策略,
So that 对弈强度随难度递增。

**Acceptance Criteria:**

**Given** GameManager 中 aiDifficulty 字段为 AIDifficulty.Simple
**When** 调用 StartNewGame()
**Then** 创建 SimpleAI 实例

**Given** aiDifficulty 为 AIDifficulty.Medium
**When** 调用 StartNewGame()
**Then** 创建 MinimaxAI(1) 实例

**Given** aidifficulty 为 AIDifficulty.Hard
**When** 调用 StartNewGame()
**Then** 创建 MinimaxAI(3) 实例

**Given** 任意 AIDifficulty 值
**When** 调用 SetAIDifficulty(difficulty)
**Then** aiDifficulty 字段被更新

**Given** Inspector 面板
**When** 查看 GameManager 组件
**Then** 可见 AIDifficulty 下拉框，默认为 Simple

### Story 1.3: AI 难度 UI 选择按钮

As a 玩家,
I want 在游戏界面上直接切换 AI 难度,
So that 不用退出游戏就能调整对手强度。

**Acceptance Criteria:**

**Given** 游戏处于 PvAI 模式且未弹出胜负对话框
**When** 查看左上角
**Then** 显示"简单""中等""困难"三个按钮，当前选中项有 ● 标记

**Given** 当前选中"简单"
**When** 点击"中等"按钮
**Then** 棋盘清空，新一局开始，使用 MinimaxAI(1)，按钮标记更新为 ● 中等

**Given** 游戏处于 PvP 模式
**When** 查看左上角
**Then** 不显示 AI 难度按钮

**Given** 胜负弹框正在显示
**When** 查看界面
**Then** 难度按钮不可见

## Epic 2: 悔棋与配置统一

### Story 2.1: 悔棋逻辑与视觉还原

As a 玩家,
I want 悔棋撤回上一步操作,
So that 我能纠正失误继续探索策略。

**Acceptance Criteria:**

**Given** PvAI 模式下玩家已落子且 AI 已回应（棋盘上有 ≥ 2 步）
**When** 调用 Undo()
**Then** 撤回玩家落子和 AI 回应（共两步），轮回到玩家回合，棋盘显示正确

**Given** PvP 模式下棋盘上有 ≥ 1 步
**When** 调用 Undo()
**Then** 撤回最近一步，轮回到上一个玩家

**Given** 棋盘为空
**When** 调用 Undo()
**Then** 返回 false，不执行任何操作

**Given** 游戏已结束（胜负/平局弹框状态）
**When** 调用 Undo()
**Then** 返回 false，不执行任何操作

**Given** 悔棋后
**When** 查看棋盘
**Then** 被撤回的格子恢复为空，最后落子标记更新到新的最后一步

**Given** 连续多次调用 Undo()
**When** 直到棋盘为空
**Then** 所有棋子被移除，回合恢复为黑棋

### Story 2.2: 悔棋 UI 按钮

As a 玩家,
I want 看到一个悔棋按钮来撤回操作,
So that 操作直观方便。

**Acceptance Criteria:**

**Given** 游戏进行中且棋盘不为空
**When** 查看左上角
**Then** 显示可点击的"悔棋"按钮

**Given** 棋盘为空或游戏已结束
**When** 查看左上角
**Then** 悔棋按钮置灰且不可点击

**Given** 悔棋按钮可点击
**When** 点击悔棋按钮
**Then** 立即执行悔棋，无确认弹框

### Story 2.3: GameConfig 运行时接入

As a 开发者,
I want 通过 GameConfig ScriptableObject 统一管理游戏参数,
So that 不用改代码就能调整游戏配置。

**Acceptance Criteria:**

**Given** GameManager 拥有 GameConfig 引用且不为 null
**When** Awake() 执行
**Then** gameMode、aiFirst、aiDifficulty 从 GameConfig 读取

**Given** GameManager 的 GameConfig 引用为 null
**When** Awake() 执行
**Then** 使用 SerializeField 默认值，不报错

**Given** BoardView 拥有 GameConfig 引用且不为 null
**When** Awake() 执行
**Then** cellSize、pieceScale 从 GameConfig 读取，棋盘按配置大小渲染

**Given** BoardView 的 GameConfig 引用为 null
**When** Awake() 执行
**Then** 使用现有 SerializeField 默认值，不报错

**Given** 修改了 GameConfig asset 中的 cellSize 为 2.0
**When** 重新运行游戏
**Then** 棋盘格子间距变大
