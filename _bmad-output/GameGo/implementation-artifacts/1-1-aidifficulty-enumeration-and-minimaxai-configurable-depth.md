---
story_id: "1.1"
story_key: "1-1-aidifficulty-enumeration-and-minimaxai-configurable-depth"
title: "AIDifficulty 枚举与 MinimaxAI 深度可配置"
status: review
created: 2026-05-21
updated: 2026-05-21
---

# Story 1.1: AIDifficulty 枚举与 MinimaxAI 深度可配置

As a 玩家,
I want AI 有简单、中等、困难三种难度可选,
So that 我能找到适合自己水平的对手。

## Acceptance Criteria

**AC1:** Given PieceType.cs 中已定义 PieceType、GameState、GameMode 枚举
When 添加 AIDifficulty 枚举（Simple, Medium, Hard）
Then 项目编译通过，AIDifficulty 可被其他类引用

**AC2:** Given MinimaxAI 使用 `const int MAX_DEPTH = 2`
When 改为构造函数参数 `public MinimaxAI(int maxDepth = 3)`
Then `new MinimaxAI()` 默认深度为 3，`new MinimaxAI(1)` 深度为 1，项目编译通过

## Tasks

- [x] 在 PieceType.cs 中添加 AIDifficulty 枚举（Simple, Medium, Hard）
- [x] 将 MinimaxAI.cs 的 MAX_DEPTH 常量改为构造函数参数

## Dev Agent Record

### Implementation Plan
- PieceType.cs: 在 GameMode 枚举后添加 AIDifficulty 枚举
- MinimaxAI.cs: 将 `const int MAX_DEPTH = 2` 改为 `readonly int _maxDepth`，添加构造函数

### Debug Log

### Completion Notes

- PieceType.cs: 添加 AIDifficulty 枚举（Simple, Medium, Hard）
- MinimaxAI.cs: MAX_DEPTH 常量改为 _maxDepth 实例字段，构造函数接受 maxDepth 参数，默认值 3

## File List

- gamego/Assets/Scripts/Core/PieceType.cs (modified)
- gamego/Assets/Scripts/AI/MinimaxAI.cs (modified)

## Change Log

- 2026-05-21: Story created from epics.md
