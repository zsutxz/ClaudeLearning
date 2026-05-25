---
story_id: "4.1"
story_key: "4-1-aivsai-game-mode-and-gamemanager-support"
title: "AIvsAI 游戏模式与 GameManager 支持"
status: review
created: 2026-05-22
updated: 2026-05-22
---

# Story 4.1: AIvsAI 游戏模式与 GameManager 支持

As a 开发者,
I want GameManager 支持 AI vs AI 自动对弈,
So that 两个 AI 策略可以自动交替落子。

## Acceptance Criteria

**AC1:** GameMode 枚举新增 AIvsAI 值，编译通过
**AC2:** AIvsAI 模式下 StartNewGame 为黑方和白方分别创建 AI 实例
**AC3:** AI 回合自动落子并触发下一方 AI，无需人类操作
**AC4:** 支持暂停/继续自对弈

## Tasks

- [x] PieceType.cs GameMode 枚举新增 AIvsAI
- [x] GameManager 添加 _blackAI 和 _whiteAI 字段
- [x] GameManager 添加自对弈配置字段（blackAIDifficulty, whiteAIDifficulty）
- [x] StartNewGame AIvsAI 分支：创建双方 AI
- [x] AIvsAI 模式下使用协程自动交替落子
- [x] 添加暂停/继续控制

## Dev Agent Record

### Implementation Plan

- PieceType.cs: GameMode 新增 AIvsAI
- GameManager.cs:
  - 新增 [SerializeField] AIDifficulty blackAIDifficulty, whiteAIDifficulty
  - 新增 IAIPlayer _blackAI, _whiteAI
  - StartNewGame 中 AIvsAI 分支创建双方 AI
  - 用协程 RunAIvsAI 自动交替落子，每步 0.5 秒延迟
  - 新增 IsSelfPlaying, IsSelfPlayPaused 属性
  - 新增 PauseSelfPlay(), ResumeSelfPlay() 方法

### Debug Log

### Completion Notes
- PieceType.cs: GameMode 新增 AIvsAI 枚举值
- GameManager: 新增 blackAIDifficulty/whiteAIDifficulty/selfPlayStepDelay 字段
- GameManager: 新增 _blackAI/_whiteAI/_selfPlayCoroutine/_selfPlayPaused 字段
- StartNewGame: AIvsAI 分支创建双方 AI，启动 RunAIvsAI 协程
- RunAIvsAI 协程：每步延迟 selfPlayStepDelay 秒，支持暂停
- 新增 PauseSelfPlay/ResumeSelfPlay/SetBlackAIDifficulty/SetWhiteAIDifficulty
- CreateAI 工厂方法统一创建逻辑
- AIvsAI 模式下 TryPlacePiece 返回 false（禁止人类落子），Undo 返回 false

## File List
- gamego/Assets/Scripts/Core/PieceType.cs (modified)
- gamego/Assets/Scripts/Core/GameManager.cs (modified)

## Change Log

- 2026-05-22: Story created from epics.md
