---
story_id: "4.3"
story_key: "4-3-game-result-statistics"
title: "对弈结果统计"
status: review
created: 2026-05-22
updated: 2026-05-22
---

# Story 4.3: 对弈结果统计

As a 玩家,
I want 看到自对弈的统计数据,
So that 我能量化对比不同 AI 策略的强度。

## Acceptance Criteria

**AC1:** 自对弈结束弹框显示胜负结果+总步数+双方 AI 名称
**AC2:** 统计区域显示本局耗时
**AC3:** 结果弹框"再来一局"以相同配置开始新的自对弈

## Tasks

- [x] GameManager 添加对弈统计字段（_gameStartTime, _moveCount）
- [x] GameManager OnGameEnded 时计算并暴露统计数据
- [x] UIManager 胜负弹框显示统计信息（步数、耗时、AI 名称）
- [x] AIvsAI 结果弹框显示双方 AI 名称

## Dev Agent Record

### Implementation Plan

- GameManager: 添加 _gameStartTime (Stopwatch), _totalMoveCount
- StartNewGame 记录开始时间，重置计数
- 每次 PlacePiece 递增计数
- 新增 GameStats 属性暴露统计数据
- UIManager: AIvsAI 模式弹框显示统计

### Debug Log

### Completion Notes
- GameManager: 新增 GameStats 结构体（TotalMoves, ElapsedSeconds, BlackAIName, WhiteAIName）
- GameManager: 新增 _gameStopwatch (Stopwatch) 和 _totalMoveCount 字段
- StartNewGame 启动计时器，每次落子递增计数，游戏结束时停止计时
- GetGameStats() 方法返回完整统计数据
- DifficultyName() 将 AIDifficulty 转为可读名称
- UIManager: AIvsAI 弹框高度增加到 260px，显示双方 AI 名称 + 步数 + 耗时

## File List
- gamego/Assets/Scripts/Core/GameManager.cs (modified)
- gamego/Assets/Scripts/UI/UIManager.cs (modified)

## Change Log

- 2026-05-22: Story created from epics.md
