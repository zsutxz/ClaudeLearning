# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 规则
- **语言**：使用中文交流
- **Git**：不自动提交，手动提交前需明确确认
- **Unity .meta 文件**：已在 .gitignore 中忽略，不要提交

## 项目概览

Unity 五子棋（Gomoku）游戏，15x15 标准棋盘，支持 PvP 和 PvAI。

- **Unity 版本**：2022.3.5f1c1
- **渲染管线**：Built-in / URP 均可
- **Assembly**：`Gomoku.asmdef`，所有脚本在 `Gomoku` 命名空间下
- **目标平台**：Windows / WebGL

## 架构

**MVC 分层**：
- **Model**：`Board`（纯 C# 类，棋盘数据）、`PieceType`/`GameState`/`GameMode` 枚举
- **View**：`BoardView`（棋盘渲染）、`CellView`（格子交互）、`PieceAnimation`（落子动画）、`UIManager`
- **Controller**：`GameManager`（MonoBehaviour，游戏流程控制）

**AI 系统**（工厂模式 `AIFactory`，统一接口 `IAIPlayer`）：

| AI | 策略 | 难度控制 |
|----|------|---------|
| `SimpleAI` | 规则优先（五连→堵→活四→活三→随机） | randomFactor: Easy=0.5, Medium=0.2, Hard=0 |
| `MinimaxAI` | Minimax + Alpha-Beta 剪枝 | 搜索深度: Easy=1, Medium=2, Hard=3 |
| `MctsAI` | 蒙特卡洛树搜索 | 模拟次数: Easy=500, Medium=1000, Hard=3000 |

**棋型评估**：`Evaluation/BoardEvaluator` + `PatternScores` + `PatternType`，供 MinimaxAI 和 MctsAI 共用。

**胜负判定**：`WinChecker.CheckWin(board, x, y)` — 从最后落子点向四个方向计数，>=5 即胜。

**事件通信**：`GameManager` 通过 `UnityEvent`（OnTurnChanged, OnGameEnded, OnPiecePlaced, OnGameReset）驱动 UI 和动画。

## 常用操作

- **场景设置**：Unity 菜单 Tools > Gomoku > Setup Scene（Editor 脚本 `GomokuSceneSetup.cs`）
- **运行测试**：Window > General > Test Runner > PlayMode > Run All（测试在 `GomokuTests.cs`）
- **配置 AI**：`GameManager` Inspector 中选择 GameMode、AIType、Difficulty

## 开发注意事项

- `Board` 是纯逻辑类，不依赖 MonoBehaviour，方便单元测试
- 新增 AI 策略只需实现 `IAIPlayer.GetMove(Board, PieceType)` 接口，并在 `AIFactory` 中注册
- MinimaxAI 搜索深度超过 3 会明显卡顿，候选人位置通过 `GetCandidateMoves` 限制在已有棋子附近
- 音效通过 `AudioManager` 单例管理，在 `GameManager.Awake` 中初始化
