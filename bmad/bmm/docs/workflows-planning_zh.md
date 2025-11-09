# BMM 规划工作流（阶段2）

**阅读时间：** 约10分钟

## 概览

阶段2（规划）工作流是**所有项目的必需**。它们使用**规模自适应系统**将战略愿景转化为可操作需求，该系统根据项目复杂性自动选择正确的规划深度。

**关键原则：** 一个统一入口点（`workflow-init`）智能路由到适当的规划方法论 - 从快速技术规范到综合PRD。

**何时使用：** 所有项目都需要规划。系统根据复杂性自动调整深度。

---

## 阶段2规划工作流图谱

```mermaid
%%{init: {'theme':'base', 'themeVariables': { 'primaryColor':'#fff','primaryTextColor':'#000','primaryBorderColor':'#000','lineColor':'#000','fontSize':'16px','fontFamily':'arial'}}}%%
graph TB
    Start["<b>开始：workflow-init</b><br/>发现 + 路由"]

    subgraph QuickFlow["<b>快速流程（简单规划）</b>"]
        direction TB
        TechSpec["<b>PM：tech-spec</b><br/>技术文档<br/>→ 故事或史诗+故事<br/>通常1-15个故事"]
    end

    subgraph BMadMethod["<b>BMAD方法（推荐）</b>"]
        direction TB
        PRD["<b>PM：prd</b><br/>战略PRD"]
        GDD["<b>游戏设计师：gdd</b><br/>游戏设计文档"]
        Narrative["<b>游戏设计师：narrative</b><br/>故事驱动设计"]

        Epics["<b>PM：create-epics-and-stories</b><br/>史诗+故事分解<br/>通常10-50+个故事"]

        UXDesign["<b>UX设计师：ux</b><br/>可选UX规范"]
    end

    subgraph Enterprise["<b>企业方法</b>"]
        direction TB
        EntNote["<b>使用BMad方法规划</b><br/>+<br/>扩展阶段3工作流<br/>（架构 + 安全 + DevOps）<br/>通常30+个故事"]
    end

    subgraph Updates["<b>流程中更新（任何时候）</b>"]
        direction LR
        CorrectCourse["<b>PM/SM：correct-course</b><br/>更新需求/故事"]
    end

    Start -->|错误修复、简单| QuickFlow
    Start -->|软件产品| PRD
    Start -->|游戏项目| GDD
    Start -->|故事驱动| Narrative
    Start -->|企业需求| Enterprise

    PRD --> Epics
    GDD --> Epics
    Narrative --> Epics
    Epics -.->|可选| UXDesign
    UXDesign -.->|可能更新| Epics

    QuickFlow --> Phase4["<b>阶段4：实施</b>"]
    Epics --> Phase3["<b>阶段3：解决方案设计</b>"]
    Enterprise -.->|使用BMad规划| Epics
    Enterprise --> Phase3Ext["<b>阶段3：扩展</b><br/>（架构 + 安全 + DevOps）"]
    Phase3 --> Phase4
    Phase3Ext --> Phase4

    Phase4 -.->|重大变更| CorrectCourse
    CorrectCourse -.->|更新| Epics

    style Start fill:#fff9c4,stroke:#f57f17,stroke-width:3px,color:#000
    style QuickFlow fill:#c5e1a5,stroke:#33691e,stroke-width:3px,color:#000
    style BMadMethod fill:#e1bee7,stroke:#6a1b9a,stroke-width:3px,color:#000
    style Enterprise fill:#ffcdd2,stroke:#c62828,stroke-width:3px,color:#000
    style Updates fill:#ffecb3,stroke:#ff6f00,stroke-width:3px,color:#000
    style Phase3 fill:#90caf9,stroke:#0d47a1,stroke-width:2px,color:#000
    style Phase4 fill:#ffcc80,stroke:#e65100,stroke-width:2px,color:#000

    style TechSpec fill:#aed581,stroke:#1b5e20,stroke-width:2px,color:#000
    style PRD fill:#ce93d8,stroke:#4a148c,stroke-width:2px,color:#000
    style GDD fill:#ce93d8,stroke:#4a148c,stroke-width:2px,color:#000
    style Narrative fill:#ce93d8,stroke:#4a148c,stroke-width:2px,color:#000
    style UXDesign fill:#ce93d8,stroke:#4a148c,stroke-width:2px,color:#000
    style Epics fill:#ba68c8,stroke:#6a1b9a,stroke-width:3px,color:#000
    style EntNote fill:#ef9a9a,stroke:#c62828,stroke-width:2px,color:#000
    style Phase3Ext fill:#ef5350,stroke:#c62828,stroke-width:2px,color:#000
    style CorrectCourse fill:#ffb74d,stroke:#ff6f00,stroke-width:2px,color:#000
```

---

## 快速参考

| 工作流                     | 代理         | 轨道       | 目的                                    | 典型故事数 |
| ---------------------------- | ------------- | ----------- | ------------------------------------------ | --------------- |
| **workflow-init**            | PM/分析师    | 全部         | 入口点：发现 + 路由           | N/A             |
| **tech-spec**                | PM            | 快速流程  | 技术文档 → 故事或史诗+故事 | 1-15            |
| **prd**                      | PM            | BMad方法 | 战略PRD                              | 10-50+          |
| **gdd**                      | 游戏设计师 | BMad方法 | 游戏设计文档                       | 10-50+          |
| **narrative**                | 游戏设计师 | BMad方法 | 故事驱动的游戏/体验设计        | 10-50+          |
| **create-epics-and-stories** | PM            | BMad方法 | 将PRD/GDD分解为史诗+故事            | N/A             |
| **ux**                       | UX设计师   | BMad方法 | 可选UX规范                  | N/A             |
| **correct-course**           | PM/SM         | 全部         | 流程中需求变更             | N/A             |

**注意：** 故事数量是基于典型使用的指导，不是严格定义。

---

## 规模自适应规划系统

BMM使用三个不同的规划轨道来适应项目复杂性：

### 轨道1：快速流程

**最适合：** 错误修复、简单功能、明确范围、增强功能

**规划：** 仅技术规范 → 实施

**时间：** 几小时到1天

**故事数量：** 通常1-15个（指导）

**文档：** tech-spec.md + 故事文件

**示例：** "修复身份验证错误"，"添加OAuth社交登录"

---

### 轨道2：BMad方法（推荐）

**最适合：** 产品、平台、复杂功能、多个史诗

**规划：** PRD + 架构 → 实施

**时间：** 1-3天

**故事数量：** 通常10-50+个（指导）

**文档：** PRD.md（或GDD.md）+ architecture.md + 史诗文件 + 故事文件

**绿地：** 产品简介（可选）→ PRD → UX（可选）→ 架构 → 实施

**棕地：** document-project → PRD → 架构（推荐）→ 实施

**示例：** "客户仪表板"，"电商平台"，"向现有应用添加搜索"

**为什么棕地需要架构？** 将庞大代码库上下文提炼为针对您特定项目的专注解决方案设计。

---

### 轨道3：企业方法

**最适合：** 企业需求、多租户、合规、安全敏感

**规划（阶段2）：** 使用BMad方法规划（PRD + 史诗+故事）

**解决方案设计（阶段3）：** 扩展工作流（架构 + 安全 + DevOps + SecOps作为可选添加）

**时间：** 总共3-7天（1-3天规划 + 2-4天扩展解决方案设计）

**故事数量：** 通常30+个（但由企业需求定义）

**阶段2文档：** PRD.md + 史诗 + 史诗文件 + 故事文件

**阶段3文档：** architecture.md + security-architecture.md（可选）+ devops-strategy.md（可选）+ secops-strategy.md（可选）

**示例：** "多租户SaaS"，"HIPAA合规门户"，"添加SOC2审计日志"

---

## 轨道选择如何工作

`workflow-init`通过教育性选择指导您：

1. **描述分析** - 分析项目描述的复杂性
2. **教育性呈现** - 显示所有三个轨道及其权衡
3. **推荐** - 基于关键词和上下文建议轨道
4. **用户选择** - 您选择合适的轨道

系统指导但从不强制。您可以覆盖推荐。

---

## 工作流描述

### workflow-init（入口点）

**目的：** 所有规划的唯一统一入口点。发现项目需求并智能路由到适当的轨道。

**代理：** PM（根据需要协调其他代理）

**始终使用：** 这是您的规划起点。除非跳过发现，否则不要直接调用prd/gdd/tech-spec。

**流程：**

1. 发现（理解上下文、评估复杂性、识别关注点）
2. 路由决策（确定轨道、解释理由、确认）
3. 执行目标工作流（调用规划工作流、传递上下文）
4. 移交（记录决策、推荐下一阶段）

---

### tech-spec（快速流程）

**目的：** 用于简单更改的轻量级技术规范（快速流程轨道）。生成技术文档和故事或史诗+故事结构。

**代理：** PM

**何时使用：**

- 错误修复
- 单个API端点添加
- 配置更改
- 小型UI组件添加
- 独立验证规则

**关键输出：**

- **tech-spec.md** - 包含以下内容的技术文档：
  - 问题描述和解决方案
  - 源代码树更改
  - 实施细节
  - 测试策略
  - 验收标准
- **故事文件** - 单个故事或史诗+故事结构（通常1-15个故事）

**跳转到阶段：** 4（实施） - 不需要阶段3架构

**示例：** "当用户没有个人资料图像时修复空指针" → 单文件更改、空检查、单元测试、无数据库迁移。

---

### prd（产品需求文档）

**目的：** 带有史诗分解的软件产品战略PRD（BMad方法轨道）。

**代理：** PM（有架构师和分析师支持）

**何时使用：**

- 中到大型功能集
- 多屏幕用户体验
- 复杂业务逻辑
- 多系统集成
- 需要分阶段交付

**规模自适应结构：**

- **轻量级：** 单个史诗、5-10个故事、简化分析（10-15页）
- **标准：** 2-4个史诗、15-30个故事、综合分析（20-30页）
- **全面：** 5+个史诗、30-50+个故事、多阶段、广泛利益相关者分析（30-50+页）

**关键输出：**

- PRD.md（完整需求）
- epics.md（史诗分解）
- 史诗文件（epic-1-_.md、epic-2-_.md等）

**集成：** 输入到架构（阶段3）

**示例：** 电商结账 → 3个史诗（访客结账、支付处理、订单管理）、21个故事、4-6周交付。

---

### gdd（游戏设计文档）

**目的：** 游戏项目的完整游戏设计文档（BMad方法轨道）。

**代理：** 游戏设计师

**何时使用：**

- 设计任何游戏（任何类型）
- 需要综合设计文档
- 团队需要共享愿景
- 出版商/利益相关者沟通

**BMM GDD vs 传统：**

- 规模自适应细节（不是瀑布式）
- 敏捷史诗结构
- 直接移交给实施
- 与测试工作流集成

**关键输出：**

- GDD.md（完整游戏设计）
- 史诗分解（核心循环、内容、进度、打磨）

**集成：** 输入到架构（阶段3）

**示例：** Roguelike卡牌游戏 → 核心概念（杀戮尖塔 meets 哈迪斯）、3个角色、120张卡、50个敌人、带有26个故事的史诗分解。

---

### narrative（叙事设计）

**目的：** 用于叙事是核心的游戏/体验的故事驱动设计工作流（BMad方法轨道）。

**代理：** 游戏设计师（叙事设计师角色）+ 创意问题解决器（CIS）

**何时使用：**

- 故事是体验的核心
- 带有玩家选择的分支叙事
- 角色驱动游戏
- 视觉小说、冒险游戏、RPG

**与GDD结合：**

1. 首先运行`narrative`（故事结构）
2. 然后运行`gdd`（将故事与游戏玩法整合）

**关键输出：**

- narrative-design.md（完整叙事规范）
- 故事结构（幕、节拍、分支）
- 角色（简介、弧线、关系）
- 对话系统设计
- 实施指南

**集成：** 与GDD结合，然后输入到架构（阶段3）

**示例：** 选择驱动RPG → 3幕、12章、5个选择点、3个结局、60K字、40个叙事场景。

---

### ux（UX优先设计）

**目的：** 用于用户体验是主要差异化因素的项目的UX规范（BMad方法轨道）。

**代理：** UX设计师

**何时使用：**

- UX是主要竞争优势
- 需要设计思维的用户工作流
- 创新交互模式
- 设计系统创建
- 可访问性关键体验

**协作方法：**

1. 视觉探索（生成多个选项）
2. 信息充分决策（与用户需求评估）
3. 协作设计（迭代优化）
4. 活文档（随项目演进）

**关键输出：**

- ux-spec.md（完整UX规范）
- 用户旅程
- 线框图和模型
- 交互规范
- 设计系统（组件、模式、令牌）
- 史诗分解（UX故事）

**集成：** 输入PRD或更新史诗，然后架构（阶段3）

**示例：** 仪表板重新设计 → 带有分屏切换的卡片布局、5个卡片组件、12个颜色令牌、响应式网格、3个史诗（布局、可视化、可访问性）。

---

### create-epics-and-stories

**目的：** 将PRD/GDD需求分解为史诗中组织的小型故事（BMad方法轨道）。

**代理：** PM

**何时使用：**

- PRD/GDD完成后（通常自动运行）
- 也可以稍后独立运行以重新生成史诗/故事
- 在主PRD工作流外规划故事分解时

**关键输出：**

- epics.md（所有史诗及其故事分解）
- 史诗文件（epic-1-*.md等）

**注意：** PRD工作流通常自动创建史诗。如果稍后需要，此工作流可以独立运行。

---

### correct-course

**目的：** 处理实施期间的重大需求变更（所有轨道）。

**代理：** PM、架构师或SM

**何时使用：**

- 项目中途优先级变更
- 新需求出现
- 需要范围调整
- 技术障碍需要重新规划

**流程：**

1. 分析变更影响
2. 提出解决方案（继续、转型、暂停）
3. 更新受影响文档（PRD、史诗、故事）
4. 重新路由到实施

**集成：** 更新规划工件，可能触发架构审查

---

## 决策指南

### 使用哪个规划工作流？

**使用`workflow-init`（推荐）：** 让系统发现需求并适当路由。

**直接选择（高级）：**

- **错误修复或单个更改** → `tech-spec`（快速流程）
- **软件产品** → `prd`（BMad方法）
- **游戏（游戏玩法优先）** → `gdd`（BMad方法）
- **游戏（故事优先）** → `narrative` + `gdd`（BMad方法）
- **UX创新项目** → `ux` + `prd`（BMad方法）
- **带合规的企业** → 在`workflow-init`中选择轨道 → 企业方法

---

## 与阶段3（解决方案设计）的集成

规划输出输入到解决方案设计：

| 规划输出     | 解决方案设计输入                    | 轨道决策               |
| ------------------- | ------------------------------------ | ---------------------------- |
| tech-spec.md        | 跳过阶段3 → 直接阶段4      | 快速流程（无架构） |
| PRD.md              | **architecture**（级别3-4）         | BMad方法（推荐）    |
| GDD.md              | **architecture**（游戏技术）         | BMad方法（推荐）    |
| narrative-design.md | **architecture**（叙事系统） | BMad方法                  |
| ux-spec.md          | **architecture**（前端设计）   | BMad方法                  |
| 企业文档     | **architecture** + 安全/运维      | 企业方法（必需） |

**关键决策点：**

- **快速流程：** 完全跳过阶段3 → 阶段4（实施）
- **BMad方法：** 可选阶段3（简单）、必需阶段3（复杂）
- **企业：** 必需阶段3（架构 + 扩展规划）

参见：[workflows-solutioning_zh.md](./workflows-solutioning_zh.md)

---

## 最佳实践

### 1. 始终从workflow-init开始

让入口点指导您。它可以防止过度规划简单功能或规划不足复杂倡议。

### 2. 信任推荐

如果`workflow-init`建议BMad方法，很可能存在您没有考虑的复杂性。在覆盖之前仔细审查。

### 3. 迭代需求

规划文档是活的。在解决方案设计和实施期间学习时优化PRD/GDD。

### 4. 早期涉及利益相关者

在解决方案设计之前与利益相关者审查PRD/GDD。及早发现不一致。

### 5. 关注"什么"而不是"如何"

规划定义**构建什么**和**为什么**。将**如何**（技术设计）留给阶段3（解决方案设计）。

### 6. 棕地项目首先执行Document-Project

在规划棕地项目之前始终运行`document-project`。AI代理需要现有代码库上下文。

---

## 常见模式

### 绿地软件（BMad方法）

```
1. （可选）分析：product-brief、research
2. workflow-init → 路由到prd
3. PM：prd工作流
4. （可选）UX设计师：ux工作流
5. PM：create-epics-and-stories（可能自动）
6. → 阶段3：architecture
```

### 棕地软件（BMad方法）

```
1. 技术作者或分析师：document-project
2. workflow-init → 路由到prd
3. PM：prd工作流
4. PM：create-epics-and-stories
5. → 阶段3：architecture（推荐专注解决方案设计）
```

### 错误修复（快速流程）

```
1. workflow-init → 路由到tech-spec
2. 架构师：tech-spec工作流
3. → 阶段4：实施（跳过阶段3）
```

### 游戏项目（BMad方法）

```
1. （可选）分析：game-brief、research
2. workflow-init → 路由到gdd
3. 游戏设计师：gdd工作流（或narrative + gdd如果故事优先）
4. 游戏设计师创建史诗分解
5. → 阶段3：architecture（游戏系统）
```

### 企业项目（企业方法）

```
1. （推荐）分析：research（合规、安全）
2. workflow-init → 路由到企业方法
3. PM：prd工作流
4. （可选）UX设计师：ux工作流
5. PM：create-epics-and-stories
6. → 阶段3：architecture + security + devops + test strategy
```

---

## 常见反模式

### ❌ 跳过规划

"我们直接开始编码，边做边想。"
**结果：** 范围蔓延、返工、遗漏需求

### ❌ 过度规划简单更改

"让我为这个按钮颜色更改写一个20页的PRD。"
**结果：** 浪费时间、分析瘫痪

### ❌ 没有发现的规划

"我已经知道我想要什么，跳过问题。"
**结果：** 解决错误问题、错失机会

### ❌ 将PRD视为不可变

"PRD已锁定，不允许变更。"
**结果：** 忽略新信息、刚性规划

### ✅ 正确方法

- 使用规模自适应规划（为复杂性选择正确深度）
- 让利益相关者参与审查
- 随学习迭代
- 保持规划文档活着和更新
- 对重大变更使用`correct-course`

---

## 相关文档

- [阶段1：分析工作流](./workflows-analysis_zh.md) - 可选发现阶段
- [阶段3：解决方案设计工作流](./workflows-solutioning_zh.md) - 下一阶段
- [阶段4：实施工作流](./workflows-implementation_zh.md)
- [规模自适应系统](./scale-adaptive-system_zh.md) - 理解三个轨道
- [快速规范流程](./quick-spec-flow_zh.md) - 快速流程轨道详情
- [代理指南](./agents-guide_zh.md) - 完整代理参考

---

## 故障排除

**问：我应该首先运行哪个工作流？**
答：运行`workflow-init`。它分析您的项目并路由到正确的规划工作流。

**问：我总是需要PRD吗？**
答：不。简单更改使用`tech-spec`（快速流程）。只有BMad方法和企业轨道创建PRD。

**问：我可以跳过阶段3（解决方案设计）吗？**
答：快速流程可以。BMad方法（简单项目）可选。BMad方法（复杂项目）和企业必需。

**问：我怎么知道选择哪个轨道？**
答：使用`workflow-init` - 它根据您的描述推荐。故事数量是指导，不是定义。

**问：如果需求在项目中途变更怎么办？**
答：运行`correct-course`工作流。它分析影响并更新规划工件。

**问：棕地项目需要架构吗？**
答：推荐！架构将庞大代码库提炼为针对您特定项目的专注解决方案设计。

**问：我什么时候运行create-epics-and-stories？**
答：通常在PRD/GDD期间自动。也可以稍后独立运行以重新生成史诗。

**问：我应该在PRD之前使用product-brief吗？**
答：可选但绿地推荐。帮助战略思考。`workflow-init`根据上下文提供它。

---

_阶段2规划 - 为每个项目提供规模自适应需求。_