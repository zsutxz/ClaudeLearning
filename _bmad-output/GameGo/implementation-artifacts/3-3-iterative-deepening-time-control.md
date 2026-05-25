---
story_id: "3.3"
story_key: "3-3-iterative-deepening-time-control"
title: "迭代加深与时间控制"
status: review
created: 2026-05-22
updated: 2026-05-22
---

# Story 3.3: 迭代加深与时间控制

As a 开发者,
I want Hard AI 使用迭代加深搜索在时间预算内返回最佳走法,
So that 搜索深度可达 5-6 层而不超时。

## Acceptance Criteria

**AC1:** Hard AI 从深度 1 开始逐层加深搜索
**AC2:** 超过时间预算（默认 2 秒）立即返回上一深度最佳结果
**AC3:** 上一深度最佳走法作为下一深度首选候选
**AC4:** 标准开局场景搜索深度达到 5+ 且不超过 2 秒

## Tasks

- [x] MinimaxAI 添加时间控制字段（_timeLimit, _searchStartTime, _isTimeout）
- [x] Minimax 方法中检查超时，超时时立即返回当前评估
- [x] GetMove 改为迭代加深循环：depth 1 → 2 → ... → maxDepth 或超时
- [x] Hard AI 在 GameManager 中使用迭代加深模式
- [x] GameManager 中 Hard AI 使用 MinimaxAI(6)（最大深度上限）

## Dev Agent Record

### Implementation Plan

- MinimaxAI.cs: 添加 _useIterativeDeepening 构造参数
- GetMove 中：如果启用迭代加深，从 depth=1 循环到 _maxDepth
- 每次迭代前检查是否超时，超时返回上一深度最佳结果
- Minimax 递归中周期性检查超时
- GameManager: Hard 改为 MinimaxAI(6, true)

### Debug Log

### Completion Notes
- MinimaxAI 构造函数新增 useIterativeDeepening 和 timeLimitMs 参数
- 使用 Stopwatch 高精度计时，每 4096 个节点检查一次超时
- GetMove 迭代加深：depth 1 → maxDepth，超时返回上一深度结果
- 上一深度最佳走法通过 ReorderCandidates 提到首位
- GameManager: Hard 改为 MinimaxAI(6, true, 2000)，最大搜索 6 层，2 秒时限

## File List
- gamego/Assets/Scripts/AI/MinimaxAI.cs (modified)
- gamego/Assets/Scripts/Core/GameManager.cs (modified)

## Change Log

- 2026-05-22: Story created from epics.md
