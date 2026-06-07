# BMAD v6.0 → v6.7.1 版本演进

## v6.0.0（2025年9-12月，alpha.0 ~ alpha.22）

**从零开始的重写**，BMAD 历史上最大的架构变革。

### 核心变化

- **Step-File 系统**：将工作流拆成细粒度的步骤文件，长任务可以暂停、保存状态、继续执行
- **21+ 专业 AI 代理**：统一的协作矩阵，包括 PM、架构师、开发、QA 等
- **Spec-Driven Development (SDD)**：规范驱动开发方法论
- **`npx bmad-method install`**：原生 NPM 安装，支持 Claude Code / Cursor 等 IDE
- **模块化架构**：BMM（软件开发）、BMGD（游戏开发）、CIS（创意智能）、BMB（构建器）

### 关键 alpha 里程碑

- **alpha.0**（2025-09-28）：Lean Core + BMad Method (BMM) + BoMB + CIS + Game Development
- **alpha.4**：文档中心（18 指南，7000+ 行）、Paige 技术文档代理、Quick Spec Flow、意图驱动规划
- **alpha.9**：智能文件发现（3 策略）、3-Track 系统、Web Bundles 指南、统一输出结构
- **alpha.10**：Epics After Architecture（技术驱动的 Story 创建）、Frame Expert 代理
- **alpha.13**：Step-File 系统完成、Phase 4 转型、Playwright 集成、TTS 注入
- **alpha.15**：module.yaml 标准化、交互式自定义内容安装
- **alpha.16~17**：`.bmad` → `_bmad` 迁移（LLM 常忽略点号开头的隐藏文件夹）、代理记忆系统（`_bmad/_memory/`）、安装器全面重做
- **alpha.20**：路径分离（规划产物 vs 永久文档）、Windows 安装器修复
- **alpha.21**：统一 2 字母菜单代码系统、Chat/Party Mode 自动注入所有代理
- **alpha.22**：统一代理工作流（Create/Edit/Validate 三路径）、代理知识系统（13 个数据文件）、260+ 文件深度语言集成

### 代理角色

Mary（分析师）、John（PM）、Sally（UX）、Winston（架构师）、Amelia（开发）、Quinn（QA）、Bob（SM）、Paige（技术文档）、Barry（快速开发）

---

## v6.1.0 — 技能化架构革命

**"一切皆技能"（Everything is a Skill）**

- **SKILL.md 入口点**：所有工作流、代理、任务统一为技能目录格式，每个技能用 `SKILL.md` 作为入口
- **清除旧引擎**：移除 YAML/XML 工作流引擎管道，更精简
- **15 个平台迁移**：全部转为原生 Agent Skills 格式
- **Edge Case Hunter**：Phase 4 的并行代码审查层，捕获边界条件
- **Quick Dev 预览**：实验性快速开发工具
- **npm 包体积减少 91%**：6.2 MB → 555 KB
- **中文文档翻译**：完整 zh-CN 支持
- **WDS（Whiteport Design Studio）模块**：设计到代码的完整管线
- **75 commits | 61 PRs | 306 files | +10,472 / -6,065 lines**

---

## v6.2.0 — 原生技能转换

**所有 BMM 工作流转为原生技能包：**

- 转换了 20+ 个工作流：create-prd、edit-prd、validate-prd、create-architecture、create-ux-design、create-epics-and-stories、create-story、dev-story、code-review、sprint-planning、sprint-status、retrospective、quick-dev 等
- **技能验证器**：确定性验证工具用于 CI
- **Code Review 重写**：分片步骤文件架构，更结构化的审查流程
- **规范化技能调用语法**

---

## v6.2.1 — 质量提升

- Quick Dev 增加审查轨迹生成、可点击的 spec 链接
- Code Review 修复：不再陷入无限循环
- 技能验证器引入，清洗所有技能的验证问题
- 移除死代码（agent schema 验证、旧 manifest 逻辑）
- 安装器发现技能改为按 `SKILL.md` 而非 manifest YAML
- 法语、中文文档翻译同步

---

## v6.2.2 — 帮助系统现代化

- `module-help.csv` 升级为 13 列格式，用 `after`/`before` 依赖图替代序号
- `bmad-help` 从 8 步流程重写为结果导向设计（缩短约 50%）

---

## v6.3.0 — 大规模整合

### 破坏性变更

| 移除 | 原因 |
|------|------|
| Barry（快速开发代理） | 合并进 Amelia |
| Quinn（QA 代理） | 合并进 Amelia |
| Bob（Scrum Master） | 合并进 Amelia |
| `bmad-init` 技能 | 代理直接从 `config.yaml` 加载配置 |
| `spec-wip.md` 单例 | 改为 `spec-{slug}.md`，支持并行会话 |

### 新增功能

- **bmad-prfaq**：亚马逊"逆向工作法"（Working Backwards），5 阶段引导工作流
- **bmad-checkpoint-preview**：引导式 commit/branch/PR 审查
- **社区模块浏览器**：三层选择（官方、社区、自定义 URL）
- **Epic 上下文编译**：Quick Dev 自动编译规划文档为缓存上下文
- **Party Mode 重构**：从多文件工作流合并为单一 SKILL.md，使用真实子代理
- **Junie（JetBrains AI）平台支持**
- **Universal Source 支持**：5 策略 PluginResolver（GitHub/GitLab/Bitbucket/自托管/本地路径）

---

## v6.4.0 — 全定制化框架

### 两大核心特性

**1. TOML 定制化**
- 每个代理和工作流都可通过 `_bmad/custom/` 下的 TOML 覆盖定制
- 四文件配置架构：`config.toml`、`config.user.toml`、`custom/config.toml`、`custom/config.user.toml`
- `bmad-customize` 技能：引导式 TOML 覆盖编写
- 23 个工作流终端步骤支持 `on_complete` 钩子

**2. 发布通道系统**
- 每个模块可选 `stable` / `next` / `pinned` 通道
- CLI 标志：`--channel`、`--all-stable`、`--all-next`、`--pin CODE=TAG`

### 其他

- PRD 工作流不再静默删减用户需求
- 移除 1,683 行死代码
- 多语言文档同步（法语、捷克语、越南语、中文）
- Kimi Code CLI 支持

---

## v6.5.0 — 平台大扩展

- **新增 18 个代理平台**：AdaL、Sourcegraph Amp、IBM Bob、Command Code、Snowflake Cortex Code、Factory Droid、Firebender、Block Goose、Kode、Mistral Vibe、Mux、Neovate、OpenClaw、OpenHands、Pochi、Replit Agent、Warp、Zencoder
- **总支持平台达到 42 个**
- 所有支持 `.agents/skills/` 标准的平台统一使用该标准

---

## v6.6.0 — CI/CD 与健壮性

### 破坏性变更

- `--tools none` 不再接受，新安装必须指定 `--tools <id>`
- `project_name` 从 `[modules.bmm]` 移到 `[core]` 配置

### 新功能

- **非交互式配置**：`--set <module>.<key>=<value>` 用于 CI/Docker 场景
- **Brownfield Epic 作用域**：检测 Epic 间文件重叠，减少不必要的文件变动
- **自定义模块安装修复**：Azure DevOps URL、HTTP Git URL、本地路径等

---

## v6.7.0 — PRD 重建与调查技能

### 核心特性

**1. bmad-prd / bmad-brief 重建**
- 从脚本式工作流重构为精简的结果导向引导器
- 三个一等意图：Create / Update / Validate
- 支持 Express 和 Guided 两种模式
- 驱动需求启发而非 LLM 自动填充
- 新 PRD 验证管线：质量评分标准 → 综合分析 → HTML + Markdown 报告

**2. bmad-investigate（新技能）**
- 法医式调查，证据分级（Confirmed / Deduced / Hypothesized）
- 支持 Bug 分类、事件根因分析（RCA）、陌生代码探索
- 委托式纪律：大型代码库自动委派给子代理

**3. 决策日志模式**
- `.decision-log` 跟踪工作流中所有决策，便于后续继续或修改

**4. 其他新功能**
- WDS 模块加入官方选择器
- OpenCode 和 GitHub Copilot 指针文件支持
- BMad Automator 模块注册

### 破坏性变更

- 安装器移除社区模块选择器
- 远程市场注册表完全退役，零网络请求

---

## v6.7.1 — 安装器修复

- 修复已安装模块源丢失时的错误：安装器现在检测到无法解析的模块会保留不动，继续更新
- BMad Automator 模块从 `baut` 重命名为 `automator` 的兼容性处理

---

## 版本演进总结

```
v6.0   重写基础：Step-File、21代理、模块化、_bmad迁移
  ↓
v6.1   架构革命：一切皆技能、SKILL.md、91%包体积缩减
  ↓
v6.2   原生化：20+工作流转换、技能验证器
  ↓
v6.3   精简整合：3代理合并→Amelia、移除bmad-init、Party Mode重构
  ↓
v6.4   深度定制：TOML覆盖、发布通道、23工作流钩子
  ↓
v6.5   生态扩展：18新平台、总计42平台支持
  ↓
v6.6   CI友好：非交互配置、Brownfield优化
  ↓
v6.7   旗舰重建：PRD/Brief重构、bmad-investigate、决策日志
```

---

> **BMAD** = Breakthrough Method for Agile AI-Driven Development
> 当前安装版本：v6.7.1
> 来源：[CHANGELOG.md](https://github.com/bmad-code-org/BMAD-METHOD/blob/main/CHANGELOG.md) | [Releases](https://github.com/bmad-code-org/BMAD-METHOD/releases)
