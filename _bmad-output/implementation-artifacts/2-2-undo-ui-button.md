---
story_id: "2.2"
story_key: "2-2-undo-ui-button"
title: "悔棋 UI 按钮"
status: review
created: 2026-05-21
updated: 2026-05-21
---

# Story 2.2: 悔棋 UI 按钮

As a 玩家,
I want 看到一个悔棋按钮来撤回操作,
So that 操作直观方便。

## Acceptance Criteria

**AC1:** 游戏进行中且棋盘不为空 → 显示可点击的"悔棋"按钮
**AC2:** 棋盘为空或游戏已结束 → 按钮置灰不可点击
**AC3:** 点击后立即执行悔棋，无确认弹框

## Tasks

- [x] UIManager OnGUI 中添加悔棋按钮（DrawUndoButton）

## Dev Agent Record

### Implementation Plan
- UIManager.cs: 在 OnGUI 的 !_showDialog 分支中添加悔棋按钮
- 使用 GUI.enabled = canUndo 控制置灰
- 调用 gameManager.Undo()

### Debug Log

### Completion Notes
- UIManager.cs: 添加 DrawUndoButton() 方法，使用 GUI.enabled 控制置灰，调用 gameManager.Undo()

## File List
- gamego/Assets/Scripts/UI/UIManager.cs (modified)

## Change Log

- 2026-05-21: Story created from epics.md
