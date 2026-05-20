# API 参考

## 枚举 — PieceType.cs

### PieceType

```csharp
public enum PieceType { None, Black, White }
```

### GameState

```csharp
public enum GameState { Playing, BlackWin, WhiteWin, Draw }
```

### GameMode

```csharp
public enum GameMode { PvAI, PvP }
```

---

## Board — 棋盘数据模型

纯 C# 类，不继承 MonoBehaviour。

### 常量

| 名称 | 类型 | 值 | 说明 |
|------|------|----|------|
| `BOARD_SIZE` | `const int` | 15 | 棋盘边长 |

### 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| `PieceCount` | `int` | 已落子总数 |
| `Grid` | `PieceType[,]` | 棋盘网格（直接暴露内部数组，注意封装风险） |

### 方法

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `Reset()` | `void` | 清空棋盘所有棋子 |
| `GetPiece(int x, int y)` | `PieceType` | 获取指定位置棋子 |
| `PlacePiece(int x, int y, PieceType piece)` | `bool` | 落子。成功返回 true，位置越界或非空返回 false |
| `RemovePiece(int x, int y)` | `bool` | 移除棋子。供 MinimaxAI 回溯使用 |
| `IsValidPosition(int x, int y)` | `bool` | 坐标是否在棋盘范围内 |
| `IsEmpty(int x, int y)` | `bool` | 指定位置是否为空 |
| `IsFull()` | `bool` | 棋盘是否已满（225 格全占） |

---

## WinChecker — 胜负判定

静态工具类，无需实例化。

### 方法

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `CheckWin(Board board, int x, int y)` | `bool` | 以最后落子位置为中心，四方向扫描是否五连 |
| `CheckGameState(Board board, int lastX, int lastY)` | `GameState` | 综合判定：先查五连，再查平局 |
| `TryGetWinningLine(Board board, int lastX, int lastY, out List<(int,int)> winningPositions)` | `bool` | 获取获胜连线所有坐标，用于视觉高亮 |

---

## GameManager — 游戏流程控制

MonoBehaviour，场景中的核心组件。

### 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| `CurrentPlayer` | `PieceType` | 当前回合玩家 |
| `GameState` | `GameState` | 当前游戏状态 |
| `Board` | `Board` | 棋盘数据引用 |
| `GameMode` | `GameMode` | 当前游戏模式 |
| `aiFirst` | `bool` | AI 是否先手 |

### 事件

| 事件 | 签名 | 触发时机 |
|------|------|----------|
| `OnTurnChanged` | `UnityEvent<PieceType>` | 回合切换 |
| `OnGameEnded` | `UnityEvent<GameState>` | 游戏结束（胜/平） |
| `OnPiecePlaced` | `UnityEvent<int, int, PieceType>` | 成功落子后 |
| `OnGameReset` | `UnityEvent` | 游戏重置 |

### 方法

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `StartNewGame()` | `void` | 重置棋盘、初始化 AI、触发事件 |
| `TryPlacePiece(int x, int y)` | `bool` | 核心落子入口。检查状态→落子→判胜→切换回合 |
| `SetGameMode(GameMode mode)` | `void` | 运行时切换游戏模式 |
| `SetAIFirst(bool first)` | `void` | 设置 AI 先手 |

---

## BoardView — 棋盘视图

MonoBehaviour，管理 15×15 的 CellView 网格。

### 方法

| 方法 | 说明 |
|------|------|
| `InitializeBoard()` | 实例化 225 个 CellView，居中排列 |
| `OnCellClicked(int x, int y)` | 格子点击回调，转发至 GameManager |
| `ShowPiece(int x, int y, PieceType piece)` | 在指定格子实例化棋子 Sphere |
| `HighlightWinningLine(List<(int,int)> positions)` | 高亮获胜连线格子 |
| `ClearBoard()` | 清除所有棋子和高亮 |
| `AutoFindResources()` | 自动查找或创建 Prefab/材质 |

---

## CellView — 格子视图

MonoBehaviour，单个格子的交互与视觉。

### 视觉状态颜色

| 字段 | 默认值 | 说明 |
|------|--------|------|
| `normalColor` | 木色 | 正常状态 |
| `hoverColor` | — | 鼠标悬停 |
| `lastMoveColor` | — | 最后落子标记 |
| `winningColor` | — | 获胜高亮 |

### 方法

| 方法 | 说明 |
|------|------|
| `Initialize(int x, int y, BoardView boardView)` | 绑定坐标和父视图 |
| `SetPiece(PieceType piece, GameObject pieceObj)` | 绑定棋子 |
| `ClearPiece()` | 清除棋子 |
| `SetLastMove(bool isLast)` | 标记最后落子 |
| `SetWinning(bool isWinning)` | 标记获胜格子 |

### Unity 回调

- `OnMouseEnter()` → 悬停高亮
- `OnMouseExit()` → 恢复正常
- `OnMouseDown()` → 触发 `BoardView.OnCellClicked()`

---

## IAIPlayer — AI 接口

```csharp
public interface IAIPlayer
{
    (int x, int y) GetMove(Board board, PieceType myPiece);
}
```

---

## UIManager — UI 管理

MonoBehaviour，监听 GameManager 事件，使用 `OnGUI` 绘制。

### 事件

| 事件 | 说明 |
|------|------|
| `OnStatusChanged` | 状态文字变更 |
| `OnGameEnded` | 游戏结束（UI 层） |

### 方法

| 方法 | 说明 |
|------|------|
| `RestartGame()` | 重新开始游戏 |
| `SetPvPMode()` | 切换双人模式 |
| `SetPvAIMode()` | 切换人机模式 |

---

## AudioManager — 音效管理

单例 MonoBehaviour，`DontDestroyOnLoad`。

### 静态访问

```csharp
AudioManager.Instance.PlayPlacePiece();
```

### 方法

| 方法 | 说明 |
|------|------|
| `PlayPlacePiece()` | 落子音效 |
| `PlayWin()` | 胜利音效 |
| `PlayLose()` | 失败音效 |
| `PlayDraw()` | 平局音效 |
| `PlayButtonClick()` | 按钮点击音效 |
| `SetVolume(float volume)` | 设置音量 |
| `Mute(bool muted)` | 静音切换 |

> **注意**: 以上方法已定义但尚未在游戏流程中调用。

---

## GameConfig — 配置 ScriptableObject

| 分类 | 字段 | 类型 | 说明 |
|------|------|------|------|
| 棋盘 | `boardSize` | `int` | 棋盘尺寸 |
| 棋盘 | `cellSize` | `float` | 格子大小 |
| 棋盘 | `pieceScale` | `float` | 棋子缩放 |
| 视觉 | `boardColor` | `Color` | 棋盘颜色 |
| 视觉 | `hoverColor` | `Color` | 悬停颜色 |
| 视觉 | `lastMoveColor` | `Color` | 最后落子颜色 |
| 视觉 | `winningColor` | `Color` | 获胜高亮颜色 |
| 游戏 | `defaultGameMode` | `GameMode` | 默认模式 |
| 游戏 | `aiFirst` | `bool` | AI 先手 |
| 音频 | `enableAudio` | `bool` | 启用音效 |
| 音频 | `sfxVolume` | `float` | 音效音量 |
| 动画 | `placePieceDuration` | `float` | 落子动画时长 |
| 动画 | `winAnimationDuration` | `float` | 获胜动画时长 |

> **注意**: GameConfig 已定义但运行时组件未从中读取配置。

---

## PieceAnimation — 棋子动画

MonoBehaviour，挂载在棋子预制体上。

| 方法 | 说明 |
|------|------|
| `PlayDropAnimation()` | 下落 + 弹跳缩放（Start 自动调用） |
| `PlayWinAnimation()` | 黄色闪烁 3 次 |
