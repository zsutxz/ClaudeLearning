---
stepsCompleted: [1, 2, 3, 4]
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

FR-8: Board 维护 64 位 Zobrist 哈希值，每次落子/移除时 O(1) 增量更新。起始空盘哈希值固定可复现。

FR-9: MinimaxAI 使用 Dictionary 缓存已搜索的棋盘状态（哈希键 + 深度 + 分数 + 最佳走法）。命中时直接返回不进入递归。置换表大小可配置（默认 1M 条目），每次新对弈清空。

FR-10: Hard AI 使用迭代加深：从深度 1 逐层加深，在时间预算内（默认 2 秒）返回当前最佳结果。上一深度最佳走法作为下一深度首选候选。搜索深度可达 5-6 层。

FR-11: GameMode 枚举新增 AIvsAI。自对弈配置 UI：两个下拉框分别选择黑方/白方 AI 策略和难度。点击开始后自动对弈。

FR-12: GameManager 支持自动交替调用双方 AI，每步落子后触发渲染和判定。每步之间可选延迟（默认 0.5 秒，可配 0 瞬时）。支持中途暂停/继续。

FR-13: 每局自对弈记录并显示统计：总步数、耗时、双方 AI 策略名称。游戏结束时弹框显示结果+步数。

### NonFunctional Requirements

NFR-1: Hard AI（MinimaxAI depth=3）落子时间不超过 2 秒。

NFR-2: GameConfig 为 null 时优雅降级，各组件使用硬编码默认值，不报错。

NFR-3: 保持现有 MVC + 策略模式架构，不引入新的设计模式。

NFR-4: 置换表内存占用不超过 100MB。

NFR-5: Hard AI 在 2 秒内搜索到深度 5+。

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
FR-8: Epic 3 — Zobrist 哈希
FR-9: Epic 3 — 置换表
FR-10: Epic 3 — 迭代加深 + 时间控制
FR-11: Epic 4 — AIvsAI 模式入口
FR-12: Epic 4 — 自对弈执行
FR-13: Epic 4 — 对弈统计

## Epic List

### Epic 1: AI 难度系统
玩家可选择简单/中等/困难三档 AI，获得递增的对弈挑战。
**FRs covered:** FR-1, FR-2, FR-3

### Epic 2: 悔棋与配置统一
玩家可悔棋撤回失误操作；开发者通过 GameConfig 统一管理游戏参数。
**FRs covered:** FR-4, FR-5, FR-6, FR-7

### Epic 3: 搜索算法优化
通过 Zobrist 哈希、置换表和迭代加深，将 Hard AI 搜索深度从 3 提升到 5-6 层。
**FRs covered:** FR-8, FR-9, FR-10

### Epic 4: AI 自对弈模式
新增 AI vs AI 模式，两个 AI 策略自动对弈，提供算法对比实验环境。
**FRs covered:** FR-11, FR-12, FR-13

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

## Epic 3: 搜索算法优化

### Story 3.1: Board 添加 Zobrist 哈希

As a 开发者,
I want Board 维护 64 位 Zobrist 哈希值,
So that MinimaxAI 可通过哈希快速判断棋盘状态是否重复。

**Acceptance Criteria:**

**Given** Board 类中已初始化 Zobrist 哈希随机表
**When** 创建空棋盘
**Then** ZobristKey 为固定的初始值，每次运行一致

**Given** 空棋盘
**When** 调用 PlacePiece(7, 7, PieceType.Black)
**Then** ZobristKey 通过异或更新为新的哈希值

**Given** 棋盘上有棋子
**When** 调用 RemovePiece(7, 7)
**Then** ZobristKey 通过异或恢复到落子前的值

**Given** 两个棋盘经历不同的落子顺序到达相同状态
**When** 比较它们的 ZobristKey
**Then** 哈希值相同

### Story 3.2: MinimaxAI 置换表缓存

As a 开发者,
I want MinimaxAI 使用置换表缓存已搜索的棋盘状态,
So that 相同棋盘不重复搜索，提升性能。

**Acceptance Criteria:**

**Given** MinimaxAI 持有置换表 Dictionary
**When** 搜索遇到已缓存的棋盘状态（哈希匹配 + 缓存深度 ≥ 当前搜索深度）
**Then** 直接返回缓存结果，不进入递归

**Given** 缓存条目包含节点类型（exact/alpha/beta）
**When** 命中缓存
**Then** 根据节点类型正确处理 alpha/beta 边界

**Given** 置换表配置为 1M 条目
**When** 内存占用测量
**Then** 不超过 100MB（NFR-4）

**Given** 新对弈开始
**When** 调用 StartNewGame 或重新创建 MinimaxAI
**Then** 置换表被清空

### Story 3.3: 迭代加深与时间控制

As a 开发者,
I want Hard AI 使用迭代加深搜索在时间预算内返回最佳走法,
So that 搜索深度可达 5-6 层而不超时。

**Acceptance Criteria:**

**Given** Hard AI 配置时间预算 2 秒
**When** 开始搜索
**Then** 从深度 1 开始，逐层加深

**Given** 搜索进行中
**When** 已用时间超过 2 秒
**Then** 立即返回上一深度完成的最佳走法

**Given** 上一深度搜索完成
**When** 开始下一深度搜索
**Then** 上一深度最佳走法作为第一个候选（提高剪枝效率）

**Given** 标准开局场景
**When** Hard AI 落子
**Then** 搜索深度达到 5+ 且耗时不超过 2 秒（NFR-5）

## Epic 4: AI 自对弈模式

### Story 4.1: AIvsAI 游戏模式与 GameManager 支持

As a 开发者,
I want GameManager 支持 AI vs AI 自动对弈,
So that 两个 AI 策略可以自动交替落子。

**Acceptance Criteria:**

**Given** PieceType.cs 中 GameMode 枚举
**When** 添加 AIvsAI 值
**Then** 编译通过，UI 可选择自对弈模式

**Given** GameManager 设置为 AIvsAI 模式
**When** StartNewGame() 执行
**Then** 为黑方和白方分别创建 AI 实例

**Given** AIvsAI 模式进行中
**When** 当前 AI 返回落子位置
**Then** 自动执行落子并触发下一方 AI 思考，无需人类操作

### Story 4.2: 自对弈 UI 配置与执行

As a 玩家,
I want 选择双方 AI 配置并观看自动对弈,
So that 我能直观对比不同 AI 策略的表现。

**Acceptance Criteria:**

**Given** 游戏模式选择区域
**When** 选择 AIvsAI 模式
**Then** 显示黑方/白方 AI 策略下拉框（Simple / Minimax-Medium / Minimax-Hard）

**Given** AIvsAI 配置完成
**When** 点击开始
**Then** 自动对弈开始，每步之间有 0.5 秒延迟

**Given** 自对弈进行中
**When** 点击暂停按钮
**Then** 对弈暂停，可点击继续恢复

**Given** 自对弈进行中
**When** 点击重置按钮
**Then** 对弈终止，棋盘清空

### Story 4.3: 对弈结果统计

As a 玩家,
I want 看到自对弈的统计数据,
So that 我能量化对比不同 AI 策略的强度。

**Acceptance Criteria:**

**Given** 自对弈对局结束
**When** 查看结果弹框
**Then** 显示胜负结果（黑胜/白胜/平局）、总步数、双方 AI 名称

**Given** 自对弈对局结束
**When** 查看统计区域
**Then** 显示本局耗时

**Given** 结果弹框
**When** 点击"再来一局"
**Then** 以相同配置开始新的自对弈
