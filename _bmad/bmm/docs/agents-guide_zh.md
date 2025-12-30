# BMM 代理参考

基于每个代理的可用命令的快速参考。

---

## 分析师 (Mary) | `/bmad:bmm:agents:analyst`

业务分析和研究。

**能力：**

- `*workflow-status` - 获取工作流状态或初始化跟踪
- `*brainstorm-project` - 引导式头脑风暴会议
- `*research` - 市场、领域、竞争或技术研究
- `*product-brief` - 创建产品简报（PRD 的输入）
- `*document-project` - 记录现有项目
- 派对模式和高级启发

---

## PM (John) | `/bmad:bmm:agents:pm`

产品需求和规划。

**能力：**

- `*workflow-status` - 获取工作流状态或初始化跟踪
- `*create-prd` - 创建产品需求文档
- `*create-epics-and-stories` - 将 PRD 分解为史诗和用户故事（架构后）
- `*implementation-readiness` - 验证 PRD、UX、架构、史诗一致性
- `*correct-course` - 实施期间的课程纠正
- 派对模式和高级启发

---

## 架构师 (Winston) | `/bmad:bmm:agents:architect`

系统架构和技术设计。

**能力：**

- `*workflow-status` - 获取工作流状态或初始化跟踪
- `*create-architecture` - 创建架构文档以指导开发
- `*implementation-readiness` - 验证 PRD、UX、架构、史诗一致性
- `*create-excalidraw-diagram` - 系统架构或技术图
- `*create-excalidraw-dataflow` - 数据流图
- 派对模式和高级启发

---

## SM (Bob) | `/bmad:bmm:agents:sm`

冲刺规划和故事准备。

**能力：**

- `*sprint-planning` - 从史诗文件生成 sprint-status.yaml
- `*create-story` - 从史诗创建故事（准备开发）
- `*validate-create-story` - 验证故事质量
- `*epic-retrospective` - 史诗完成后的团队回顾
- `*correct-course` - 实施期间的课程纠正
- 派对模式和高级启发

---

## DEV (Amelia) | `/bmad:bmm:agents:dev`

故事实施和代码审查。

**能力：**

- `*dev-story` - 执行故事工作流（带测试的实施）
- `*code-review` - 彻底的代码审查

---

## 快速流程独立开发者 (Barry) | `/bmad:bmm:agents:quick-flow-solo-dev`

快速独立开发，无需交接。

**能力：**

- `*create-tech-spec` - 架构技术规格及实施就绪的故事
- `*quick-dev` - 独立端到端实施技术规格
- `*code-review` - 审查和改进代码

---

## TEA (Murat) | `/bmad:bmm:agents:tea`

测试架构和质量策略。

**能力：**

- `*framework` - 初始化生产就绪测试框架
- `*atdd` - 首先生成 E2E 测试（实施前）
- `*automate` - 综合测试自动化
- `*test-design` - 创建综合测试场景
- `*trace` - 将需求映射到测试、质量门决策
- `*nfr-assess` - 验证非功能需求
- `*ci` - 脚手架 CI/CD 质量流水线
- `*test-review` - 审查测试质量

---

## UX 设计师 (Sally) | `/bmad:bmm:agents:ux-designer`

用户体验和 UI 设计。

**能力：**

- `*create-ux-design` - 从 PRD 生成 UX 设计和 UI 计划
- `*validate-design` - 验证 UX 规格和设计产品
- `*create-excalidraw-wireframe` - 创建网站或应用线框图

---

## 技术文档编写者 (Paige) | `/bmad:bmm:agents:tech-writer`

技术文档和图表。

**能力：**

- `*document-project` - 综合项目文档（项目分析）
- `*generate-mermaid` - 生成 Mermaid 图表（架构、序列、流程、ER、类、状态）
- `*create-excalidraw-flowchart` - 流程和逻辑流可视化
- `*create-excalidraw-diagram` - 系统架构或技术图
- `*create-excalidraw-dataflow` - 数据流可视化
- `*validate-doc` - 根据标准审查文档
- `*improve-readme` - 审查和改进 README 文件
- `*explain-concept` - 创建清晰的技术解释和示例
- `*standards-guide` - 显示 BMAD 文档标准参考

---

## 通用命令

所有代理可用：

- `*menu` - 重新显示菜单选项
- `*dismiss` - 解除代理

派对模式可用于大多数代理以进行多代理协作。
