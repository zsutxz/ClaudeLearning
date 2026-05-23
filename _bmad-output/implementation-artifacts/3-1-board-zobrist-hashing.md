---
story_id: "3.1"
story_key: "3-1-board-zobrist-hashing"
title: "Board 添加 Zobrist 哈希"
status: review
created: 2026-05-22
updated: 2026-05-22
---

# Story 3.1: Board 添加 Zobrist 哈希

As a 开发者,
I want Board 维护 64 位 Zobrist 哈希值,
So that MinimaxAI 可通过哈希快速判断棋盘状态是否重复。

## Acceptance Criteria

**AC1:** 空棋盘 ZobristKey 为固定初始值，每次运行一致
**AC2:** PlacePiece 后 ZobristKey 通过异或更新
**AC3:** RemovePiece 后 ZobristKey 通过异或恢复到落子前的值
**AC4:** 不同落子顺序到达相同状态，ZobristKey 相同

## Tasks

- [x] Board 类添加 Zobrist 随机表初始化（static ulong[,] 数组）
- [x] Board 类添加 ZobristKey 属性和初始哈希常量
- [x] PlacePiece 中异或更新 ZobristKey
- [x] RemovePiece 中异或恢复 ZobristKey
- [x] Reset 中重置 ZobristKey 为初始值

## Dev Agent Record

### Implementation Plan

- Board.cs: 添加 static ulong[,] _zobristTable 初始化（使用固定种子的 Random）
- Board.cs: 添加 public ulong ZobristKey 属性
- PlacePiece: 成功后 `_zobristKey ^= _zobristTable[x, y * 2 + (int)piece - 1]`
- RemovePiece: 成功后同样异或（异或两次恢复原值）
- Reset: `_zobristKey = INITIAL_KEY`

### Debug Log

### Completion Notes
- Board.cs: 添加 static ulong[,] _zobristTable（固定种子 Random(42) 初始化 225×2 随机数表）
- Board.cs: 添加 ulong _zobristKey 字段和 ZobristKey 属性
- PlacePiece: 成功后 _zobristKey ^= table[index, pieceIndex]
- RemovePiece: 先异或恢复再清空格子（保证读正确的棋子类型）
- Reset: _zobristKey = 0

## File List
- gamego/Assets/Scripts/Core/Board.cs (modified)

## Change Log

- 2026-05-22: Story created from epics.md
