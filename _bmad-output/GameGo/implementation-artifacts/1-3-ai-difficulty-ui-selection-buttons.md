---
story_id: "1.3"
story_key: "1-3-ai-difficulty-ui-selection-buttons"
title: "AI 难度 UI 选择按钮"
status: review
created: 2026-05-21
updated: 2026-05-21
---

# Story 1.3: AI 难度 UI 选择按钮

As a 玩家,
I want 在游戏界面上直接切换 AI 难度,
So that 不用退出游戏就能调整对手强度。

## Acceptance Criteria

**AC1:** PvAI 模式下左上角显示"简单""中等""困难"三个按钮，当前选中项有 ● 标记
**AC2:** 点击任意按钮后棋盘清空、新一局开始、使用对应 AI、标记更新
**AC3:** PvP 模式下不显示难度按钮
**AC4:** 胜负弹框显示时难度按钮不可见

## Tasks

- [x] UIManager 添加 _currentAIDifficulty 状态字段
- [x] OnGUI 中添加 DrawAIDifficultyButtons() 方法（三按钮 + ● 标记）
- [x] 添加 SwitchAIDifficulty() 方法调用 GameManager

## Dev Agent Record

### Implementation Plan
- UIManager.cs: 添加 _currentAIDifficulty 字段
- OnGUI 中 PvAI 分支后调用 DrawAIDifficultyButtons()
- DrawAIDifficultyButtons: 三个按钮，当前项显示 ●，点击调用 SwitchAIDifficulty
- SwitchAIDifficulty: 更新本地状态 + gameManager.SetAIDifficulty + StartNewGame

### Debug Log

### Completion Notes
- UIManager.cs: 添加 _currentAIDifficulty 字段，OnGUI 中 PvAI 模式显示三按钮，DrawAIDifficultyButtons + 内联 SwitchAIDifficulty 逻辑

## File List
- gamego/Assets/Scripts/UI/UIManager.cs (modified)

## Change Log

- 2026-05-21: Story created from epics.md
