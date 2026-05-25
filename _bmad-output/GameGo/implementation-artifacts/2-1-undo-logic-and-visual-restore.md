---
story_id: "2.1"
story_key: "2-1-undo-logic-and-visual-restore"
title: "悔棋逻辑与视觉还原"
status: review
created: 2026-05-21
updated: 2026-05-21
---

# Story 2.1: 悔棋逻辑与视觉还原

As a 玩家,
I want 悔棋撤回上一步操作,
So that 我能纠正失误继续探索策略。

## Acceptance Criteria

**AC1:** PvAI 模式撤两步（玩家+AI），轮回到玩家回合
**AC2:** PvP 模式撤一步，轮回到上一个玩家
**AC3:** 棋盘为空时 Undo() 返回 false
**AC4:** 游戏结束时 Undo() 返回 false
**AC5:** 悔棋后棋子移除、最后落子标记正确更新
**AC6:** 连续悔棋可撤回到空盘，回合恢复为黑棋

## Tasks

- [x] GameManager 添加 _moveHistory 栈和 CanUndo 属性
- [x] TryPlacePiece/MakeAIMove 中记录落子到历史栈
- [x] 实现 Undo() 方法（PvAI 撤两步、PvP 撤一步）
- [x] BoardView 添加 RemovePieceAt() 和 RefreshLastMoveMarker() 方法

## Dev Agent Record

### Implementation Plan
- GameManager.cs: 添加 Stack<(int x, int y, PieceType piece)> _moveHistory
- TryPlacePiece 和 MakeAIMove 中 _moveHistory.Push
- Undo() 方法：Pop 对应步数，Board.RemovePiece，通知 BoardView 刷新
- CanUndo 属性：_moveHistory.Count > 0 && _gameState == Playing
- BoardView.cs: 添加 RemovePieceAt(int x, int y) 和 RefreshLastMoveMarker((int x, int y) lastMove)

### Debug Log

### Completion Notes
- GameManager.cs: 添加 _moveHistory 栈、CanUndo 属性、Undo() 方法；TryPlacePiece/MakeAIMove 中记录落子
- BoardView.cs: 添加 RemovePieceAt() 和 RefreshLastMoveMarker() 方法

## File List
- gamego/Assets/Scripts/Core/GameManager.cs (modified)
- gamego/Assets/Scripts/Core/BoardView.cs (modified)

## Change Log

- 2026-05-21: Story created from epics.md
