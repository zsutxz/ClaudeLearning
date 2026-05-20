# 五子棋架构文档

## 项目概览

Unity 2022.3 五子棋游戏，采用 MVC 变体架构。15x15 标准棋盘，支持 PvP 和 PvAI 模式。

- **Unity 版本**: 2022.3.5f1c1
- **渲染管线**: Built-in（兼容 URP）
- **目标平台**: Windows / WebGL

## 模块划分

```
Assets/Scripts/
├── Core/           # 核心逻辑与视图
│   ├── PieceType.cs      枚举定义（棋子类型、游戏状态、游戏模式）
│   ├── Board.cs          棋盘数据模型（纯 C#，不依赖 MonoBehaviour）
│   ├── WinChecker.cs     胜负判定（静态工具类）
│   ├── GameManager.cs    游戏流程控制中心
│   ├── BoardView.cs      棋盘渲染与交互
│   ├── CellView.cs       单格视图与鼠标事件
│   ├── GameConfig.cs     配置 ScriptableObject（已定义，未接入）
│   ├── AudioManager.cs   音效单例（已定义，未接入）
│   ├── PieceAnimation.cs 棋子下落/获胜动画
│   └── GomokuTests.cs    单元测试（已注释）
├── AI/             # AI 策略
│   ├── IAIPlayer.cs      AI 接口契约
│   ├── SimpleAI.cs       规则优先级 AI（默认启用）
│   └── MinimaxAI.cs      Minimax + Alpha-Beta AI（已实现，未启用）
├── UI/
│   └── UIManager.cs      状态显示与模式切换
└── Editor/
    └── GomokuSceneSetup.cs  Tools > Gomoku 场景搭建工具
```

## 架构模式

### MVC 变体

| 角色 | 类 | 职责 |
|------|-----|------|
| **Model** | `Board` | 棋盘数据，纯逻辑，无 Unity 依赖 |
| **View** | `BoardView`, `CellView`, `PieceAnimation`, `UIManager` | 渲染与用户交互 |
| **Controller** | `GameManager` | 流程控制、状态管理、事件分发 |

### 事件驱动解耦

`GameManager` 通过四个 `UnityEvent` 与视图层通信：

```
OnPiecePlaced(int x, int y, PieceType)  →  BoardView.ShowPiece()
OnTurnChanged(PieceType)                →  UIManager 更新状态文字
OnGameEnded(GameState)                  →  BoardView 高亮 + UIManager 弹框
OnGameReset()                           →  BoardView.ClearBoard()
```

### 策略模式（AI）

`IAIPlayer` 接口统一 AI 契约，`SimpleAI` 和 `MinimaxAI` 为可互换实现。当前 `GameManager` 硬编码使用 `SimpleAI`。

## 依赖关系图

```
                PieceType (枚举，零依赖)
                     │
          ┌──────────┼──────────┐
          ▼          ▼          ▼
        Board    GameMode    GameState
          │                     │
          ▼                     ▼
     WinChecker ◄─────── GameManager ─────► IAIPlayer
          │                     │               │
          │                     ▼               ├── SimpleAI
          │               BoardView ◄───────    └── MinimaxAI
          │                │    │
          │                ▼    ▼
          │           CellView  PieceAnimation
          │
          └──────────► UIManager

    AudioManager (独立单例，未接入)
    GameConfig   (ScriptableObject，未接入)
```

## 核心数据流

### 用户落子

```
CellView.OnMouseDown()
  → BoardView.OnCellClicked(x, y)
    → GameManager.TryPlacePiece(x, y)
      ├─ Board.PlacePiece(x, y, piece)          # 数据层
      ├─ OnPiecePlaced → BoardView.ShowPiece()   # 渲染
      ├─ WinChecker.CheckGameState()              # 判定
      │   ├─ 游戏结束 → OnGameEnded → 高亮 + 弹框
      │   └─ 游戏继续 → SwitchPlayer
      │       └─ AI 回合 → MakeAIMove()
      └─
```

### AI 落子

```
GameManager.MakeAIMove()
  → IAIPlayer.GetMove(board, aiPiece)   # 策略计算
    → Board.PlacePiece(x, y, piece)      # 与用户落子共用后续流程
```

## 已知技术债

| 项目 | 现状 | 建议 |
|------|------|------|
| AI 策略选择 | `GameManager` 硬编码 `SimpleAI` | 通过 `GameConfig` 或 UI 选择切换 |
| GameConfig | 已定义但运行时未消费 | 各组件应从 GameConfig 读取参数 |
| AudioManager | 播放方法无人调用 | 在 `TryPlacePiece` / `OnGameEnded` 中接入 |
| Board.Grid | 直接暴露可变数组 | 改为只读接口或提供 Copy |
| GomokuTests | 测试全部注释 | 安装 Test Framework 后启用 |
