---
story_id: "1.2"
story_key: "1-2-gamemanager-three-tier-ai-factory-logic"
title: "GameManager 三档 AI 工厂逻辑"
status: review
created: 2026-05-21
updated: 2026-05-21
---

# Story 1.2: GameManager 三档 AI 工厂逻辑

As a 玩家,
I want 选择不同 AI 难度后游戏使用对应的 AI 策略,
So that 对弈强度随难度递增。

## Acceptance Criteria

**AC1:** Given aiDifficulty 为 Simple → StartNewGame() 创建 SimpleAI
**AC2:** Given aiDifficulty 为 Medium → StartNewGame() 创建 MinimaxAI(1)
**AC3:** Given aiDifficulty 为 Hard → StartNewGame() 创建 MinimaxAI(3)
**AC4:** SetAIDifficulty(difficulty) 更新 aiDifficulty 字段
**AC5:** Inspector 可见 AIDifficulty 下拉框，默认 Simple

## Tasks

- [x] GameManager 添加 aiDifficulty SerializeField 和 AIDifficulty 属性
- [x] StartNewGame() 中用 switch 工厂替换硬编码 SimpleAI
- [x] 添加 SetAIDifficulty() 方法

## Dev Agent Record

### Implementation Plan
- GameManager.cs: 添加 [SerializeField] AIDifficulty aiDifficulty = AIDifficulty.Simple
- StartNewGame() 中用 aiDifficulty switch 替换 new SimpleAI()
- 添加 public AIDifficulty AIDifficulty => aiDifficulty 属性
- 添加 public void SetAIDifficulty(AIDifficulty difficulty) 方法

### Debug Log

### Completion Notes
- GameManager.cs: 添加 aiDifficulty SerializeField + AIDifficulty 属性，StartNewGame 中 switch 工厂逻辑，SetAIDifficulty 方法

## File List
- gamego/Assets/Scripts/Core/GameManager.cs (modified)

## Change Log

- 2026-05-21: Story created from epics.md
