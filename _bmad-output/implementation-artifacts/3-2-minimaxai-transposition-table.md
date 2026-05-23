---
story_id: "3.2"
story_key: "3-2-minimaxai-transposition-table"
title: "MinimaxAI 置换表缓存"
status: review
created: 2026-05-22
updated: 2026-05-22
---

# Story 3.2: MinimaxAI 置换表缓存

As a 开发者,
I want MinimaxAI 使用置换表缓存已搜索的棋盘状态,
So that 相同棋盘不重复搜索，提升性能。

## Acceptance Criteria

**AC1:** 搜索遇到已缓存棋盘（哈希匹配 + 缓存深度 ≥ 当前深度）时直接返回缓存结果
**AC2:** 缓存条目包含节点类型（exact/alpha/beta），命中时根据类型正确处理边界
**AC3:** 置换表 1M 条目，内存不超过 100MB
**AC4:** 每次新对弈（GetMove 调用首层）可清空置换表

## Tasks

- [x] 定义 TTEntry 结构体（key, depth, score, bestMove, nodeType）
- [x] 定义 NodeType 枚举（Exact, Alpha, Beta）
- [x] MinimaxAI 添加 Dictionary<ulong, TTEntry> 置换表
- [x] Minimax 方法开头查询置换表
- [x] Minimax 方法结尾存储结果到置换表
- [x] GetMove 方法开头清空置换表（可选，或保留跨对弈缓存）

## Dev Agent Record

### Implementation Plan

- MinimaxAI.cs: 添加 NodeType 枚举和 TTEntry 结构体
- MinimaxAI.cs: 添加 Dictionary<ulong, TTEntry> _transpositionTable
- Minimax 中：
  1. 查表：如果命中且 depth >= 当前深度，根据 nodeType 返回或调整边界
  2. 搜索完成后存表
- GetMove: 每次调用不清空，保留缓存跨回合复用

### Debug Log

### Completion Notes
- 新增 TTNodeType 枚举（Exact/Alpha/Beta）和 TTEntry 结构体
- MinimaxAI 添加 Dictionary<ulong, TTEntry> 置换表，跨回合保留缓存
- Minimax 搜索中：查表（根据 nodeType 处理边界）→ 搜索 → 存表
- 置换表最佳走法作为下一搜索首选候选（提高剪枝效率）
- 新增 ClearTranspositionTable() 公共方法供外部调用
- TTEntry 结构体 ~40 bytes，1M 条目 ~40MB，远低于 100MB 上限

## File List
- gamego/Assets/Scripts/AI/MinimaxAI.cs (modified)

## Change Log

- 2026-05-22: Story created from epics.md
