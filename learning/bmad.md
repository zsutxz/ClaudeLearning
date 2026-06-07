# BMAD 框架：安装、架构与使用方法

> **BMAD** = Breakthrough Method for Agile AI-Driven Development
> 当前版本：v6.7.1 | 分析日期：2026-05-24

---

## 一、安装

### 1.1 前置条件

- **Node.js 20.12+**
- **Python 3.10+**（部分功能需要）
- 支持的 AI IDE：Claude Code、Cursor、Windsurf、Roo、Copilot 等 42 个平台

### 1.2 交互式安装（推荐）

```bash
npx bmad-method install
```

安装流程：
1. 选择目标 IDE（可多选）
2. 选择安装模块（BMM、BMB、CIS、WDS 等）
3. 回答配置问题：用户名、语言（Chinese）、输出目录、项目名等
4. 安装器生成 `_bmad/` 目录和 `.claude/skills/` 下的技能文件

### 1.3 非交互式安装（CI/Docker）

```bash
npx bmad-method install --yes --tools claude-code --set core.user_name=tan --set core.communication_language=Chinese
```

常用标志：

| 标志 | 说明 |
|------|------|
| `--yes` | 跳过确认 |
| `--tools <id>` | 指定目标 IDE（必选） |
| `--set <module>.<key>=<value>` | 设置配置项 |
| `--channel <stable\|next>` | 选择发布通道 |
| `--list-tools` | 查看支持的 IDE |
| `--list-options [module]` | 查看可配置项 |
| `--custom-source <git-url-or-path>` | 安装自定义模块 |

### 1.4 更新

```bash
npx bmad-method install --quick-update
```

- 保留 `_bmad/custom/` 下的用户定制
- 自动迁移配置格式变更

### 1.5 安装产物

```
项目根目录/
├── _bmad/                          # BMAD 框架核心目录
│   ├── config.toml                 # 安装器管理的配置
│   ├── config.user.toml            # 用户答案
│   ├── custom/                     # 用户持久化定制（升级不覆盖）
│   │   ├── config.toml
│   │   └── config.user.toml
│   ├── _config/                    # 代理清单、IDE 配置
│   ├── _memory/                    # 代理持久记忆
│   ├── core/                       # 核心模块
│   │   ├── config.yaml             # 运行时配置
│   │   └── bmad-init/              # 初始化脚本
│   ├── bmm/                        # BMM 软件开发生命周期模块
│   │   ├── config.yaml
│   │   ├── 1-analysis/             # Phase 1 工作流
│   │   ├── 2-plan-workflows/       # Phase 2 工作流
│   │   ├── 3-solutioning/          # Phase 3 工作流
│   │   └── 4-implementation/       # Phase 4 工作流
│   └── bmb/                        # BMB 构建器模块
├── .claude/
│   └── skills/                     # 51+ 个技能目录
│       ├── bmad-agent-analyst/
│       │   ├── SKILL.md            # 技能入口
│       │   └── bmad-skill-manifest.yaml
│       ├── bmad-agent-pm/
│       ├── bmad-create-prd/
│       └── ...
└── CLAUDE.md                       # 项目指令（自动更新）
```

---

## 二、架构

### 2.1 三层架构

```
┌─────────────────────────────────────────┐
│            用户交互层                     │
│   /bmad-agent-analyst  /bmad-create-prd  │
│   通过 IDE 斜杠命令或技能名调用           │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│            技能系统层                     │
│   SKILL.md（入口）+ Manifest（身份）      │
│   51+ 技能 → 代理 / 工作流 / 任务         │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│            框架核心层                     │
│   _bmad/ 配置 + 模块 + 工作流步骤文件     │
│   Step-File 系统 + Agent Memory          │
└─────────────────────────────────────────┘
```

### 2.2 四大模块

| 模块 | 代码 | 功能 |
|------|------|------|
| **Core** | `_bmad/core/` | 初始化、头脑风暴、Party Mode、文档工具、高级需求启发 |
| **BMM** | `_bmad/bmm/` | 完整软件开发生命周期（分析→规划→方案→实现） |
| **BMB** | `_bmad/bmb/` | 元工具：创建新代理、模块、工作流 |
| **CIS** | — | 创意智能套件：头脑风暴、故事讲述、设计思维 |

### 2.3 BMM 四阶段生命周期

```
Phase 1: 分析               Phase 2: 规划
┌──────────────────┐        ┌──────────────────┐
│ Mary  分析师      │        │ John   PM         │
│ Paige 技术文档    │        │ Sally  UX 设计师  │
│                  │        │                   │
│ • 市场调研        │ ────▶  │ • PRD 创建/编辑   │
│ • 竞品分析        │        │ • UX 设计规范     │
│ • 产品简报        │        │ • PRD 验证        │
│ • PRFAQ          │        │                   │
└──────────────────┘        └────────┬─────────┘
                                     │
Phase 4: 实现               Phase 3: 方案
┌──────────────────┐        ┌────────▼─────────┐
│ Amelia 开发       │        │ Winston 架构师     │
│                  │        │                   │
│ • Sprint 规划    │ ◀────  │ • 架构设计        │
│ • Story 开发     │        │ • Epic/Story 拆分  │
│ • 代码审查       │        │ • 实现就绪检查     │
│ • 回顾           │        │ • 项目上下文生成   │
│ • 路线纠正       │        │                   │
└──────────────────┘        └──────────────────┘
```

### 2.4 代理角色清单

v6.3.0 后合并为以下核心代理：

| 代理 | 代号 | 角色 | 模块 |
|------|------|------|------|
| Mary | bmad-agent-analyst | 业务分析师 | BMM |
| John | bmad-agent-pm | 产品经理 | BMM |
| Sally | bmad-agent-ux-designer | UX 设计师 | BMM |
| Winston | bmad-agent-architect | 架构师 | BMM |
| Amelia | bmad-agent-dev | 开发者（含 QA、SM、Quick Dev） | BMM |
| Paige | bmad-agent-tech-writer | 技术文档 | BMM |
| Bond | bmad-agent-builder | 代理构建专家 | BMB |
| Morgan | bmad-module-builder | 模块构建专家 | BMB |
| Wendy | bmad-workflow-builder | 工作流构建专家 | BMB |

### 2.5 技能目录结构

每个技能是一个独立目录：

```
.claude/skills/bmad-agent-analyst/
├── SKILL.md                       # 技能指令（LLM 读取并执行）
├── bmad-skill-manifest.yaml       # 身份声明（名称、角色、风格）
└── steps/                         # 工作流步骤文件（可选）
    ├── step-01-orient.md
    ├── step-02-discover.md
    └── step-03-execute.md
```

**SKILL.md** 是核心入口，包含：
- 代理角色和行为指令
- 配置加载流程
- 功能菜单定义
- 子技能路由逻辑

**Manifest** 声明代理身份：

```yaml
type: agent
name: bmad-agent-analyst
displayName: Mary
title: Business Analyst
icon: "📊"
role: Strategic Business Analyst + Requirements Expert
communicationStyle: "Speaks with the excitement of a treasure hunter..."
module: bmm
```

### 2.6 配置体系

四文件分层架构：

| 文件 | 谁管理 | 用途 |
|------|--------|------|
| `_bmad/config.toml` | 安装器 | 核心设置 + 代理清单 |
| `_bmad/config.user.toml` | 安装器 | 用户答案（用户名、语言等） |
| `_bmad/custom/config.toml` | 用户 | 持久化定制覆盖 |
| `_bmad/custom/config.user.toml` | 用户 | 用户级持久定制 |

每个模块也有自己的 `config.yaml`（运行时），由 `bmad_init.py` 从 TOML 生成。

### 2.7 初始化系统

`bmad_init.py`（位于 `.claude/skills/bmad-init/scripts/`）提供四个子命令：

| 命令 | 用途 |
|------|------|
| `check` | 检测配置状态，返回待回答问题 |
| `load` | 快速加载已有配置（JSON） |
| `resolve-defaults` | 展开模板占位符 |
| `write` | 将用户答案写入配置文件 |

流程：技能激活 → 调用 `bmad_init.py load` → 有配置则直接用 → 没有则引导用户填写 → 写入配置。

### 2.8 Step-File 工作流

长工作流拆分为独立步骤文件，支持暂停和恢复：

```
steps-c/              # Create 路径
  step-01-orient.md   # 了解上下文
  step-02-discover.md # 收集信息
  step-03-design.md   # 设计方案
  ...
  step-09-finalize.md # 完成
steps-e/              # Edit 路径
steps-v/              # Validate 路径
```

关键规则：
- 永不同时加载多个步骤文件
- 步骤必须按顺序完成
- 通过 frontmatter 中 `stepsCompleted` 追踪状态
- 在菜单处暂停等待用户输入

### 2.9 工作流执行引擎

```
步骤 1: 加载和初始化
  ├─ 1a: 加载配置并解析变量
  ├─ 1b: 加载所需组件
  └─ 1c: 初始化输出

步骤 2: 按顺序处理每个步骤
  ├─ 2a: 处理步骤属性
  ├─ 2b: 执行步骤内容
  ├─ 2c: 处理模板输出
  └─ 2d: 步骤完成

步骤 3: 完成
  └─ 确认文档保存并报告完成
```

---

## 三、使用方法

### 3.1 启动代理

在 Claude Code 中输入斜杠命令或技能名：

```
/bmad-agent-analyst      # 启动 Mary（分析师）
/bmad-agent-pm           # 启动 John（PM）
/bmad-agent-architect    # 启动 Winston（架构师）
/bmad-agent-dev          # 启动 Amelia（开发）
/bmad-agent-ux-designer  # 启动 Sally（UX）
/bmad-agent-tech-writer  # 启动 Paige（技术文档）
```

代理启动后会：
1. 加载配置和项目上下文
2. 用配置的语言（中文）打招呼
3. 展示功能菜单
4. 等待用户选择

### 3.2 常用工作流技能

#### Phase 1 — 分析

| 技能 | 功能 |
|------|------|
| `bmad-market-research` | 市场调研 |
| `bmad-domain-research` | 领域调研 |
| `bmad-technical-research` | 技术调研 |
| `bmad-product-brief` | 产品简报（Create/Update/Validate） |
| `bmad-prfaq` | 亚马逊逆向工作法 |
| `bmad-document-project` | 现有项目文档化 |

#### Phase 2 — 规划

| 技能 | 功能 |
|------|------|
| `bmad-create-prd` / `bmad-prd` | 创建 PRD |
| `bmad-edit-prd` | 编辑 PRD |
| `bmad-validate-prd` | 验证 PRD 质量 |
| `bmad-create-ux-design` | UX 设计规范 |

#### Phase 3 — 方案

| 技能 | 功能 |
|------|------|
| `bmad-create-architecture` | 架构设计 |
| `bmad-create-epics-and-stories` | Epic/Story 拆分 |
| `bmad-check-implementation-readiness` | 实现就绪检查 |
| `bmad-generate-project-context` | 项目上下文生成 |

#### Phase 4 — 实现

| 技能 | 功能 |
|------|------|
| `bmad-sprint-planning` | Sprint 规划 |
| `bmad-sprint-status` | Sprint 状态跟踪 |
| `bmad-create-story` | 创建 Story |
| `bmad-dev-story` | 开发 Story（TDD） |
| `bmad-code-review` | 代码审查 |
| `bmad-quick-dev` | 快速开发（小功能/技术规范） |
| `bmad-retrospective` | 回顾 |
| `bmad-correct-course` | 路线纠正 |
| `bmad-qa-generate-e2e-tests` | E2E 测试生成 |

#### 核心工具

| 技能 | 功能 |
|------|------|
| `bmad-brainstorming` | 头脑风暴（60+ 技术，10 大类） |
| `bmad-party-mode` | 多代理圆桌讨论 |
| `bmad-help` | 上下文感知帮助 |
| `bmad-customize` | 引导式 TOML 定制 |
| `bmad-distillator` | 文档压缩 |
| `bmad-shard-doc` | 大文档拆分 |
| `bmad-index-docs` | 文档索引生成 |
| `bmad-investigate` | 法医式 Bug/代码调查（证据分级） |
| `bmad-checkpoint-preview` | commit/PR 审查 |

### 3.3 典型开发流程

```
1. /bmad-agent-analyst → 市场调研、产品简报
2. /bmad-agent-pm → 创建 PRD
3. /bmad-agent-ux-designer → UX 设计
4. /bmad-agent-architect → 架构设计
5. /bmad-create-epics-and-stories → 拆分 Epic/Story
6. /bmad-sprint-planning → Sprint 规划
7. /bmad-dev-story → 逐个 Story 开发
8. /bmad-code-review → 代码审查
9. /bmad-sprint-status → 跟踪进度
10. /bmad-retrospective → Sprint 回顾
```

### 3.4 红绿重构开发流程（dev-story）

| 阶段 | 操作 |
|------|------|
| **RED** | 为任务/子任务功能编写失败的测试 |
| **GREEN** | 实现使测试通过的最小代码 |
| **REFACTOR** | 在保持测试绿色的同时改进代码结构 |

关键规则：
- 永不实现任何未映射到 Story 文件中特定任务的内容
- 连续执行直到所有任务完成或遇到明确的停止条件

### 3.5 头脑风暴系统

10 大类 60+ 技术：

| 类别 | 示例 |
|------|------|
| 协作类 | Yes And Building, Brain Writing Round Robin |
| 创意类 | What If Scenarios, Analogical Thinking |
| 深度类 | Five Whys, Morphological Analysis |
| 结构化类 | SCAMPER Method, Six Thinking Hats |
| 戏剧类 | Time Travel Talk Show, Alien Anthropologist |
| 仿生类 | Nature's Solutions, Ecosystem Thinking |

反偏差协议：每 10 个想法转换创意领域，目标 100+ 想法。

### 3.6 Party Mode（多代理协作）

- 协调所有已安装 BMAD 代理之间的讨论
- 智能代理选择：分析用户消息 → 匹配角色能力 → 选 2-3 个最相关代理
- 维护每个代理的独特个性和专业视角

### 3.7 定制化

通过 `_bmad/custom/` 下的 TOML 文件覆盖任意代理或工作流：

```bash
/bmad-customize        # 引导式定制工具
```

或手动编辑：

```toml
# _bmad/custom/config.user.toml

[agents.bmad-agent-analyst]
communication_style = "concise"

[workflow.bmad-create-prd]
default_mode = "express"
```

支持定制的维度：
- 代理人格、沟通风格、菜单项
- 工作流步骤、输出格式、默认值
- 工具配置、版本控制策略
- `on_complete` 钩子（23 个工作流终端）

### 3.8 注意事项

- **运行目录**：必须在项目根目录（`_bmad/` 所在目录）使用
- **语言配置**：安装时选择 Chinese，所有代理将用中文交互
- **持久记忆**：代理学习内容存储在 `_bmad/_memory/`，跨会话保持
- **升级安全**：`_bmad/custom/` 下的定制在升级时不会被覆盖
- **决策日志**：v6.7.0+ 使用 `.decision-log` 跟踪工作流中的决策

---

> 来源：[BMAD-METHOD GitHub](https://github.com/bmad-code-org/BMAD-METHOD) | [CHANGELOG.md](https://github.com/bmad-code-org/BMAD-METHOD/blob/main/CHANGELOG.md)
