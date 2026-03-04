# 五子棋 (Gomoku) - Unity 项目

经典五子棋游戏的 Unity 实现。

## 🎯 功能特性

- ✅ 15x15 标准棋盘
- ✅ 本地双人对战 (PvP)
- ✅ 人机对战 (PvAI) - 简单 AI
- ✅ 五子连线胜负判定
- ✅ 落子动画效果
- ✅ 获胜连线高亮
- ✅ 音效系统
- ✅ 完整的单元测试

## 📁 项目结构

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── PieceType.cs       # 枚举定义
│   │   ├── Board.cs           # 棋盘逻辑
│   │   ├── WinChecker.cs      # 胜负判定
│   │   ├── GameManager.cs     # 游戏管理器
│   │   ├── BoardView.cs       # 棋盘视图
│   │   ├── CellView.cs        # 格子视图
│   │   ├── GameConfig.cs      # 游戏配置
│   │   ├── AudioManager.cs    # 音效管理
│   │   ├── PieceAnimation.cs  # 棋子动画
│   │   └── GomokuTests.cs     # 单元测试
│   ├── AI/
│   │   ├── IAIPlayer.cs       # AI 接口
│   │   ├── SimpleAI.cs        # 简单 AI
│   │   └── MinimaxAI.cs       # 高级 AI (Minimax)
│   ├── UI/
│   │   └── UIManager.cs       # UI 管理
│   ├── Editor/
│   │   └── GomokuSceneSetup.cs # 场景快速设置工具
│   └── Gomoku.asmdef          # Assembly Definition
├── Prefabs/                   # 预制体
├── Materials/                 # 材质
├── Scenes/                    # 场景
└── Audio/                     # 音效
```

## 🚀 快速开始

### 方法一：使用编辑器工具（推荐）

1. 在 Unity Hub 中打开 `gamego` 文件夹
2. 菜单：**Tools > Gomoku > Setup Scene** - 自动创建场景
3. 菜单：**Tools > Gomoku > Create Cell Prefab** - 创建格子预制体
4. 菜单：**Tools > Gomoku > Create Piece Prefab** - 创建棋子预制体
5. 菜单：**Tools > Gomoku > Create Materials** - 创建材质
6. 配置 `BoardView` 组件的引用（Cell Prefab、Piece Prefab、材质）
7. 运行游戏！

### 方法二：手动创建

详见下方"手动设置步骤"。

### 运行测试

1. 菜单：**Window > General > Test Runner**
2. 选择 **PlayMode** 标签
3. 点击 **Run All**

## 🤖 AI 策略

### SimpleAI (MVP 版本)
1. 🏆 获胜（五连）
2. 🛡️ 阻止对手获胜
3. ⚔️ 创建活四
4. 🛡️ 阻止对手活四
5. ⚔️ 创建活三
6. 🛡️ 阻止对手活三
7. 📍 在棋子附近落子
8. 🎲 随机选择

### MinimaxAI (进阶版本)
- 使用 Minimax + Alpha-Beta 剪枝
- 可调整搜索深度 (MAX_DEPTH)
- 更强的对弈能力

## 游戏模式

- **PvP**: 本地双人对战
- **PvAI**: 人机对战（简单 AI）

## AI 策略

当前 SimpleAI 实现：
1. 优先获胜（检测五连机会）
2. 阻止对手获胜
3. 创建活四/活三
4. 阻止对手活四/活三
5. 在棋子附近落子
6. 随机选择

## 后续开发计划

- [ ] 更智能的 AI (Minimax + Alpha-Beta 剪枝)
- [ ] 悔棋功能
- [ ] 多种棋盘皮肤
- [ ] 音效系统
- [ ] 教程系统
- [ ] 在线对战

## 技术规格

- **Unity 版本**: 2021.3 LTS 或更高
- **渲染管线**: Built-in / URP 均可
- **目标平台**: Windows / WebGL
