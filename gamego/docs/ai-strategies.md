# AI 策略文档

项目通过 `IAIPlayer` 接口实现策略模式，当前有两个实现。

## IAIPlayer 接口

```csharp
public interface IAIPlayer
{
    (int x, int y) GetMove(Board board, PieceType myPiece);
}
```

接收当前棋盘状态和己方颜色，返回落子坐标。

---

## SimpleAI — 规则优先级（默认启用）

### 策略原理

按固定优先级顺序逐条检查，找到第一个满足条件的位置立即返回：

| 优先级 | 动作 | 目标连子数 | 说明 |
|--------|------|-----------|------|
| 1 | 进攻获胜 | 5 | 自己能五连则直接落子 |
| 2 | 防守堵截 | 5 | 对手四连则必须堵 |
| 3 | 进攻 | 4 | 自己活四 |
| 4 | 防守 | 4 | 对手活三（提前堵） |
| 5 | 进攻 | 3 | 自己活三 |
| 6 | 防守 | 3 | 对手活二 |
| 7 | 附近空位 | — | 已有棋子周围 2 格内随机 |
| 8 | 完全随机 | — | 优先中心 (7,7) |

### 评估方法

`EvaluatePosition(board, x, y, piece, targetCount)`：

1. 在空位 (x, y) 假设落子
2. 沿四个方向（水平、垂直、主对角线、反对角线）双向统计连续同色棋子数
3. 若连子数 >= targetCount 则返回分数

### 特点

- **优点**: 速度快，逻辑清晰，能处理基本攻防
- **缺点**: 无前瞻搜索，不考虑对手后续反应，容易被有策略的人类击败
- **复杂度**: O(n²) 其中 n=15，每次落子约遍历 225 个位置 × 4 方向

---

## MinimaxAI — Minimax + Alpha-Beta（已实现，未启用）

### 算法参数

- **搜索深度**: `MAX_DEPTH = 2`
- **候选位置**: 已有棋子周围 2 格范围内的空位
- **剪枝**: 标准 Alpha-Beta 剪枝

### 搜索流程

```
GetMove()
  ├─ GetCandidateMoves()         收集候选位置
  ├─ 对每个候选位置:
  │   ├─ Board.PlacePiece()      模拟落子
  │   ├─ Minimax(depth, alpha, beta, isMaximizing)
  │   │   ├─ 即时胜负检测 → ±100000
  │   │   ├─ depth == 0 → EvaluateBoard()
  │   │   ├─ 递归搜索子节点
  │   │   └─ Alpha-Beta 剪枝
  │   └─ Board.RemovePiece()     回溯
  └─ 返回最高分位置
```

### 评估函数

`EvaluateBoard()` 对棋盘全局评估，扫描双方所有行/列/对角线段：

| 棋型 | 活型分（两端开放） | 死型分（一端被堵） |
|------|----------------|----------------|
| 五连 | 100,000 | 100,000 |
| 四连 | 10,000 | 1,000 |
| 三连 | 1,000 | 100 |
| 二连 | 100 | 10 |

最终分数 = 己方评估分 - 对手评估分。

### 特点

- **优点**: 有前瞻搜索，能发现 SimpleAI 无法识别的组合威胁
- **缺点**: 深度 2 仍然较浅；候选位置多时（开局空盘）性能堪忧；评估函数未考虑棋型组合
- **复杂度**: O(b^d × n²)，b 为平均分支因子，d 为深度

---

## 启用 MinimaxAI

当前 `GameManager.StartNewGame()` 硬编码创建 `SimpleAI`。切换方式：

```csharp
// GameManager.cs, StartNewGame() 方法中
// 将:
_aiPlayer = new SimpleAI();
// 改为:
_aiPlayer = new MinimaxAI();
```

建议后续通过 `GameConfig` 或 UI 下拉框让玩家选择 AI 难度。

## 扩展新 AI

实现 `IAIPlayer` 接口即可：

```csharp
public class CustomAI : IAIPlayer
{
    public (int x, int y) GetMove(Board board, PieceType myPiece)
    {
        // 自定义策略
    }
}
```
