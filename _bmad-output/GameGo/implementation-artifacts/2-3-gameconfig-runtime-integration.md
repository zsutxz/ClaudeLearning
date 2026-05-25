---
story_id: "2.3"
story_key: "2-3-gameconfig-runtime-integration"
title: "GameConfig 运行时接入"
status: review
created: 2026-05-21
updated: 2026-05-21
---

# Story 2.3: GameConfig 运行时接入

As a 开发者,
I want 通过 GameConfig ScriptableObject 统一管理游戏参数,
So that 不用改代码就能调整游戏配置。

## Acceptance Criteria

**AC1:** GameManager 拥有 GameConfig 引用且不为 null → Awake 从 GameConfig 读取
**AC2:** GameManager 的 GameConfig 为 null → 使用 SerializeField 默认值不报错
**AC3:** BoardView 拥有 GameConfig 引用且不为 null → Awake 从 GameConfig 读取 cellSize/pieceScale
**AC4:** BoardView 的 GameConfig 为 null → 使用 SerializeField 默认值不报错

## Tasks

- [x] GameManager 添加 gameConfig 字段和 ApplyGameConfig() 方法
- [x] BoardView 添加 gameConfig 字段和 ApplyGameConfig() 方法
- [x] GameConfig 添加 defaultAIDifficulty 字段

## Dev Agent Record

### Implementation Plan
- GameManager.cs: 添加 [SerializeField] GameConfig gameConfig，Awake 中调用 ApplyGameConfig
- BoardView.cs: 添加 [SerializeField] GameConfig gameConfig，Awake 中调用 ApplyGameConfig
- GameConfig.cs: 添加 public AIDifficulty defaultAIDifficulty = AIDifficulty.Simple

### Debug Log

### Completion Notes
- GameManager.cs: 添加 gameConfig 引用 + ApplyGameConfig() 读取 gameMode/aiFirst/aiDifficulty
- BoardView.cs: 添加 gameConfig 引用 + ApplyGameConfig() 读取 cellSize/pieceScale
- GameConfig.cs: 添加 defaultAIDifficulty 字段

## File List
- gamego/Assets/Scripts/Core/GameManager.cs (modified)
- gamego/Assets/Scripts/Core/BoardView.cs (modified)
- gamego/Assets/Scripts/Core/GameConfig.cs (modified)

## Change Log

- 2026-05-21: Story created from epics.md
