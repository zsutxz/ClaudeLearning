# 五子棋 (Gomoku) - Unity 项目

经典五子棋游戏的 Unity 实现。

## 功能特性

- 15x15 标准棋盘
- 本地双人对战 (PvP)
- 人机对战 (PvAI) — SimpleAI（规则优先级）/ MinimaxAI（Alpha-Beta 剪枝）
- 五子连线胜负判定
- 落子动画与获胜连线高亮
- 编辑器一键场景搭建工具

## 快速开始

### 使用编辑器工具（推荐）

1. 在 Unity Hub 中打开 `gamego` 文件夹
2. 菜单：**Tools > Gomoku > Setup Scene** — 自动创建场景
3. 菜单：**Tools > Gomoku > Create Cell Prefab** — 创建格子预制体
4. 菜单：**Tools > Gomoku > Create Piece Prefab** — 创建棋子预制体
5. 菜单：**Tools > Gomoku > Create Materials** — 创建材质
6. 配置 `BoardView` 组件的引用（Cell Prefab、Piece Prefab、材质）
7. 运行游戏

### 运行测试

1. 菜单：**Window > General > Test Runner**
2. 选择 **PlayMode** 标签
3. 点击 **Run All**

## 技术规格

- **Unity 版本**: 2022.3.5f1c1
- **渲染管线**: Built-in（兼容 URP）
- **目标平台**: Windows / WebGL

## 文档

详细文档位于 [`docs/`](docs/) 目录：

| 文档 | 说明 |
|------|------|
| [架构文档](docs/architecture.md) | MVC 架构、模块划分、依赖关系、数据流、技术债 |
| [AI 策略](docs/ai-strategies.md) | SimpleAI 和 MinimaxAI 的实现细节与评估函数 |
| [API 参考](docs/api-reference.md) | 所有公共类、方法、事件的完整参考 |

## 后续开发计划

- [ ] AI 策略选择 UI（切换 SimpleAI / MinimaxAI）
- [ ] GameConfig 接入运行时配置
- [ ] AudioManager 接入游戏流程
- [ ] 悔棋功能
- [ ] 多种棋盘皮肤
- [ ] 教程系统
- [ ] 在线对战
