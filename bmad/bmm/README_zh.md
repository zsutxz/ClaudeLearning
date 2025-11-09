# BMM - BMad 方法模块 (BMad Method Module)

**AI驱动的敏捷开发核心编排系统**，通过专业代理和工作流提供全面的生命周期管理。

---

## 📚 完整文档中心

👉 **[BMM 文档中心](./docs/README.md)** - 完整指南、教程和参考资料的起点

**快速链接：**

- **[快速入门指南](./docs/quick-start.md)** - BMM新手？从这里开始（15分钟）
- **[代理指南](./docs/agents-guide.md)** - 了解您的12个专业AI代理（45分钟）
- **[规模自适应系统](./docs/scale-adaptive-system.md)** - BMM如何适应项目规模（42分钟）
- **[常见问题](./docs/faq.md)** - 常见问题的快速解答
- **[术语表](./docs/glossary.md)** - 关键术语参考

---

## 🏗️ 模块结构

本模块包含：

```
bmm/
├── agents/          # 12个专业AI代理（PM、架构师、SM、DEV、TEA等）
├── workflows/       # 4个阶段+测试共34个工作流
├── teams/           # 预配置的代理组
├── tasks/           # 原子工作单元
├── testarch/        # 全面的测试基础设施
└── docs/            # 完整的用户文档
```

### 代理阵容

**核心开发团队：** PM、分析师、架构师、SM、DEV、TEA、UX设计师、技术文档工程师
**游戏开发团队：** 游戏设计师、游戏开发者、游戏架构师
**编排团队：** BMad 主控器（来自核心模块）

👉 **[完整代理指南](./docs/agents-guide.md)** - 角色、工作流以及每个代理的使用时机

### 工作流阶段

**阶段0：** 文档化（仅限棕地项目）
**阶段1：** 分析（可选）- 5个工作流
**阶段2：** 规划（必需）- 6个工作流
**阶段3：** 解决方案设计（Level 3-4）- 2个工作流
**阶段4：** 实施（迭代）- 10个工作流
**测试：** 质量保证（并行）- 9个工作流

👉 **[工作流指南](./docs/README.md#-workflow-guides)** - 每个阶段的详细文档

---

## 🚀 快速开始

**新项目：**

```bash
# 安装 BMM
npx bmad-method@alpha install

# 在IDE中加载分析师代理，然后：
*workflow-init
```

**现有项目（棕地项目）：**

```bash
# 首先文档化您的代码库
*document-project

# 然后初始化
*workflow-init
```

👉 **[快速入门指南](./docs/quick-start.md)** - 完整设置和首个项目演练

---

## 🎯 核心概念

### 规模自适应设计

BMM自动调整项目复杂度（级别0-4）：

- **级别0-1：** 针对错误修复和小功能的快速规格流程
- **级别2：** 带可选架构的PRD（产品需求文档）
- **级别3-4：** 完整PRD + 综合架构设计

👉 **[规模自适应系统](./docs/scale-adaptive-system.md)** - 完整级别分解

### 以故事为中心的实施

故事在定义的生命周期中流转：`backlog → drafted → ready → in-progress → review → done`

即时史诗上下文和故事上下文在需要时提供精确的专业能力。

👉 **[实施工作流](./docs/workflows-implementation.md)** - 完整故事生命周期指南

### 多代理协作

使用派对模式 (party mode) 让所有19+个代理（来自BMM、CIS、BMB、自定义模块）参与小组讨论，用于战略决策、创意头脑风暴和复杂问题解决。

👉 **[派对模式指南](./docs/party-mode.md)** - 如何编排多代理协作

---

## 📖 附加资源

- **[棕地项目指南](./docs/brownfield-guide.md)** - 处理现有代码库
- **[快速规格流程](./docs/quick-spec-flow.md)** - Level 0-1项目的快速通道
- **[企业级代理开发](./docs/enterprise-agentic-development.md)** - 团队协作模式
- **[故障排除](./docs/troubleshooting.md)** - 常见问题和解决方案
- **[IDE设置指南](../../../docs/ide-info/)** - 配置Claude Code、Cursor、Windsurf等

---

## 🤝 社区

- **[Discord](https://discord.gg/gk8jAdXWmj)** - 获取帮助、分享反馈（#general-dev, #bugs-issues）
- **[GitHub Issues](https://github.com/bmad-code-org/BMAD-METHOD/issues)** - 报告错误或请求功能
- **[YouTube](https://www.youtube.com/@BMadCode)** - 视频教程和演练

---

**准备开始构建？** → [从快速入门指南开始](./docs/quick-start.md)

---

**原文链接**: `d:\work\AI\ClaudeTest\bmad\bmm\README.md`
**翻译时间**: 2025-11-09
**文章类型**: 框架文档

## 📊 技术架构解析

### 🏛️ 系统设计理念
- **智能编排**: 通过AI代理实现开发流程的自动化编排
- **生命周期管理**: 覆盖从需求分析到代码部署的完整软件开发生命周期
- **规模适应性**: 根据项目复杂度自动调整开发流程和资源分配

### 🔧 核心技术特性
1. **多代理协作系统**: 12个专业AI代理协同工作
2. **阶段化工作流**: 4个主要开发阶段 + 测试阶段
3. **故事驱动开发**: 以用户故事为中心的开发模式
4. **即时上下文**: Just-in-time的专业知识提供

### 🎯 适用场景
- **新项目开发**: 从零开始的软件开发项目
- **现有项目维护**: 棕地项目的现代化和重构
- **企业级应用**: 大型团队的协作开发
- **游戏开发**: 专门的游戏开发代理团队

---

*📝 翻译说明：本文档为BMAD框架v6.0的核心模块说明文档，翻译遵循技术准确性原则，保留所有技术术语和链接引用，为中文开发者提供完整的技术框架理解。*