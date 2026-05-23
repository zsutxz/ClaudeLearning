---
title: 五子棋 — AI 加强与功能完善
created: 2026-05-21
updated: 2026-05-22
status: draft
---

# PRD: 五子棋 — AI 加强与功能完善

## 0. 文档目的

面向开发者本人，定义迭代范围。v1 MVP 已完成（棋盘、落子、胜负判定、PvP/PvAI、SimpleAI、MinimaxAI）。v2（AI 三档难度 + 悔棋 + GameConfig）已实现。**v3 聚焦搜索算法优化和 AI 研究工具**——置换表提升 MinimaxAI 性能，AI 自对弈模式提供算法对比实验环境。

## 1. Vision

v2 让游戏"好玩"。v3 让 AI **可研究**——通过置换表和迭代加深，Hard AI 搜索深度从 3 提升到 5-6 而不超时；AI 自对弈模式让两种算法自动对局并记录结果，直观对比策略强度。核心目标：从"有 AI 对手"升级为"有 AI 实验平台"。

## 2. Target User

### 2.1 Primary Persona

**Tan** — 独立开发者。用这个项目学习博弈 AI 算法，通过实验对比验证算法改进效果。

### 2.2 Jobs To Be Done

- 观察不同 AI 算法对弈，直观感受策略差异
- 量化对比算法强度（胜率、思考时间、搜索节点数）
- 提升 Hard AI 强度而不牺牲响应速度

## 3. Glossary

- **Zobrist 哈希** — 将棋盘状态映射为 64 位哈希值的算法，O(1) 增量更新
- **置换表 (Transposition Table)** — 哈希表缓存已评估棋盘的搜索结果，避免重复计算
- **迭代加深 (Iterative Deepening)** — 从深度 1 逐步加深搜索，在时间限制内返回当前最佳结果
- **AI 自对弈 (AI vs AI)** — 两个 AI 策略自动对弈，无需人类操作
- **对弈记录 (GameRecord)** — 一局对弈的完整数据（双方策略、每步落子、耗时、结果）

## 4. Features

### 4.1 置换表优化

**Description:** 为 MinimaxAI 添加 Zobrist 哈希和置换表缓存。相同棋盘状态（不同落子顺序到达）不重复搜索。结合迭代加深，在时间预算内自动选择最优搜索深度。

**Functional Requirements:**

#### FR-8: Zobrist 哈希

Board 维护 64 位 Zobrist 哈希值，每次落子/移除时 O(1) 增量更新。

**Consequences:**
- Board 新增 `ulong ZobristKey` 属性
- PlacePiece 和 RemovePiece 时异或更新
- 起始空盘哈希值固定，可复现

#### FR-9: 置换表

MinimaxAI 使用 Dictionary 缓存已搜索的棋盘状态（哈希键 + 深度 + 分数 + 最佳走法）。

**Consequences:**
- 命中置换表时直接返回缓存结果，不进入递归
- 缓存条目包含：哈希键、搜索深度、评估分数、最佳走法、节点类型（exact/alpha/beta）
- 置换表大小可配置（默认 1M 条目）
- 每次新对弈清空置换表

#### FR-10: 迭代加深 + 时间控制

Hard AI 使用迭代加深：从深度 1 开始，逐层加深，在时间预算内返回当前最佳结果。

**Consequences:**
- 默认时间预算 2 秒
- 超时立即返回上一深度完成的最佳走法
- 上一深度的最佳走法作为下一深度搜索的第一个候选（提高剪枝效率）
- 搜索深度可达 5-6 层（得益于置换表缓存）

**Out of Scope:** 并行搜索、多线程

### 4.2 AI 自对弈模式

**Description:** 新增游戏模式，两个 AI 策略自动对弈。玩家可选择双方 AI 配置，观看对局或快速模拟，查看结果统计。

**Functional Requirements:**

#### FR-11: 自对弈模式入口

GameMode 枚举新增 AIvsAI。UI 新增自对弈配置界面：选择黑方/白方 AI 策略和难度。

**Consequences:**
- GameMode 新增 `AIvsAI` 值
- 自对弈配置 UI：两个下拉框分别选择黑方/白方 AI（SimpleAI / MinimaxAI-Medium / MinimaxAI-Hard）
- 点击开始后自动对弈，人类无需操作

#### FR-12: 自对弈执行

GameManager 支持自动交替调用双方 AI，每步落子后触发渲染和判定。

**Consequences:**
- AI 回合不再需要等待人类点击
- 每步之间可选延迟（可配置，默认 0.5 秒，设为 0 则瞬时完成）
- 支持中途暂停/继续

#### FR-13: 对弈统计

每局自对弈记录数据并显示统计信息。

**Consequences:**
- 显示：总步数、耗时、黑方/白方 AI 策略名称
- 游戏结束时弹框显示结果（黑胜/白胜/平局）+ 步数
- 可选：批量对弈模式（连续 N 局，汇总胜率统计）

**Out of Scope:** 对弈记录持久化保存、棋谱回放

## 5. Non-Goals (Explicit)

- 在线对战 / 网络功能
- 教程系统 / 策略提示
- 多种棋盘皮肤
- 棋局保存/加载文件
- 移动端适配
- 神经网络 / 强化学习（v4+）

## 6. MVP Scope

### 6.1 In Scope

- Board 添加 Zobrist 哈希
- MinimaxAI 置换表缓存
- 迭代加深 + 时间控制
- AIvsAI 游戏模式
- 自对弈 UI 配置和执行
- 对弈结果统计

### 6.2 Out of Scope for v3

- 批量对弈模式 — 单局自对弈已满足基本对比需求
- 搜索节点数/命中率实时显示 — 调试信息，非核心功能
- AudioManager 接入 — 非本迭代重点

## 7. Success Metrics

**Primary**
- **SM-4**: Hard AI 在 2 秒内搜索到深度 5+，落子质量显著高于 v2 的 depth=3。Validates FR-8, FR-9, FR-10.
- **SM-5**: SimpleAI vs MinimaxAI-Hard 自对弈 10 局，Hard 胜率 > 90%。Validates FR-11, FR-12.
- **SM-6**: 自对弈模式可完整观看一局 AI 对弈，统计正确显示。Validates FR-11, FR-12, FR-13.

**Counter-metrics**
- **SM-C2**: 置换表内存占用不超过 100MB — 不能为追求深度牺牲内存。Counterbalances SM-4.

## 8. Open Questions

1. 自对弈批量模式是否纳入 v3？单局已够用，批量可作为 Story 级别追加
2. 搜索节点数/置换表命中率是否需要在 UI 显示？可作为开发者调试选项

## 9. Assumptions Index

- `[ASSUMPTION]` Zobrist 哈希增量更新在 PlacePiece/RemovePiece 中正确维护 — §4.1 FR-8
- `[ASSUMPTION]` 置换表命中率在 15×15 棋盘中盘阶段可达 20%+ — §4.1 FR-9
- `[ASSUMPTION]` 迭代加深 + 置换表可让 Hard AI 在 2 秒内完成 depth=5 搜索 — §4.1 FR-10
- `[ASSUMPTION]` AIvsAI 模式可复用现有 GameManager 事件系统渲染棋盘 — §4.2 FR-12
