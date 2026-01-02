# 独立工作流构建器架构

本文档描述独立工作流构建器系统的架构——一种创建结构化工作流的纯Markdown方法。

## 核心架构原则

### 1. 微文件设计

每个工作流由多个专注、自包含的文件组成，由初始加载的 workflow.md 文件驱动：

```
workflow-folder/
├── workflow.md              # 主工作流配置
├── steps/                   # 步骤指令文件（专注、自包含）
│   ├── step-01-init.md
│   ├── step-02-profile.md
│   └── step-N-[name].md
├── templates/               # 内容模板
│   ├── profile-section.md
│   └── [other-sections].md
└── data/                    # 可选数据文件
    └── [data-files].csv/.json
```

### 2. 即时加载 (JIT)

- **内存中单文件**：仅加载当前步骤文件
- **无预览**：步骤文件不得引用未来步骤
- **顺序处理**：步骤严格按顺序执行
- **按需加载**：模板仅在需要时加载

### 3. 状态管理

- **Frontmatter跟踪**：工作流状态存储在输出文档的frontmatter中
- **进度数组**：`stepsCompleted` 跟踪已完成的步骤
- **最后步骤标记**：`lastStep` 指示恢复位置
- **仅追加构建**：文档通过追加内容增长

### 4. 执行模型

```
1. 加载 workflow.md → 读取配置
2. 执行 step-01-init.md → 初始化或检测继续
3. 对于每个步骤：
   a. 完全加载步骤文件
   b. 顺序执行指令
   c. 在菜单点等待用户输入
   d. 仅在选择 'C'（继续）时继续
   e. 更新文档/frontmatter
   f. 加载下一步骤
```

## 关键组件

### 工作流文件 (workflow.md)

- **目的**：入口点和配置
- **内容**：角色定义、目标、架构规则
- **操作**：指向 step-01-init.md

### 步骤文件 (step-NN-[name].md)

- **大小**：专注且简洁（通常 5-10KB）
- **结构**：Frontmatter + 顺序指令
- **特性**：自包含规则、菜单处理、状态更新

### Frontmatter 变量

步骤文件中的标准变量：

```yaml
workflow_path: '{project-root}/_bmad/bmb/reference/workflows/[workflow-name]'
thisStepFile: '{workflow_path}/steps/step-[N]-[name].md'
nextStepFile: '{workflow_path}/steps/step-[N+1]-[name].md'
workflowFile: '{workflow_path}/workflow.md'
outputFile: '{output_folder}/[output-name]-{project_name}.md'
```

## 执行流程

### 全新工作流

```
workflow.md
    ↓
step-01-init.md (创建文档)
    ↓
step-02-[name].md
    ↓
step-03-[name].md
    ↓
...
    ↓
step-N-[final].md (完成工作流)
```

### 继续工作流

```
workflow.md
    ↓
step-01-init.md (检测现有文档)
    ↓
step-01b-continue.md (分析状态)
    ↓
step-[appropriate-next].md
```

## 菜单系统

### 标准菜单模式

```
显示：**选择选项：** [A] [操作] [P] 派对模式 [C] 继续

#### 菜单处理逻辑：
- 如果 A：执行 {advancedElicitationTask}
- 如果 P：执行 {partyModeWorkflow}
- 如果 C：保存内容、更新frontmatter、加载下一步骤
```

### 菜单规则

- **必须暂停**：始终等待用户输入
- **仅继续**：仅在选择 'C' 时继续
- **状态持久化**：在加载下一步骤前保存
- **返回循环**：其他操作后返回菜单

## 协作对话模型

### 非命令-响应

- **促进者角色**：AI引导，用户决策
- **平等伙伴**：双方共同贡献
- **无假设**：不假设用户想要下一步
- **明确同意**：始终请求输入

### 示例模式

```
AI："告诉我您的饮食偏好。"
用户：[提供信息]
AI："谢谢。现在让我们讨论您的烹饪习惯。"
[继续对话]
AI：**菜单选项**
```

## CSV智能（可选）

### 数据驱动行为

- CSV文件中的配置
- 动态菜单选项
- 变量替换
- 条件逻辑

### 示例结构

```csv
variable,type,value,description
cooking_frequency,choice,"daily|weekly|occasionally","用户烹饪频率"
meal_type,multi,"breakfast|lunch|dinner|snacks","要规划的餐食类型"
```

## 最佳实践

### 文件大小限制

- **步骤文件**：保持专注且合理大小（通常5-10KB）
- **模板**：保持专注且可重用
- **工作流文件**：保持精简，无实现细节

### 顺序执行

- **编号步骤**：使用顺序编号（1、2、3...）
- **无跳过**：每个步骤必须完成
- **状态更新**：在frontmatter中标记完成

### 错误预防

- **路径变量**：使用frontmatter变量，绝不硬编码
- **完整加载**：始终在执行前读取整个文件
- **菜单暂停**：未选择 'C' 时绝不继续

## 从XML迁移

### 优势

- **无依赖**：纯markdown，无需XML解析
- **人类可读**：文件自文档化
- **Git友好**：清晰的差异和合并
- **灵活**：更易修改和扩展

### 关键差异

| XML工作流       | 独立工作流           |
| --------------- | -------------------- |
| 单个大文件      | 多个微文件           |
| 复杂结构        | 简单顺序步骤         |
| 需要解析器      | 任何markdown查看器   |
| 严格格式        | 灵活组织             |

## 实现说明

### 关键规则

- **绝不**加载多个步骤文件
- **始终**首先完整读取步骤文件
- **绝不**跳过步骤或优化
- **始终**在步骤完成时更新输出文件的frontmatter
- **绝不**未经用户同意继续

### 成功指标

- 正确创建文档
- 所有步骤按顺序完成
- 用户对协作过程满意
- 清晰、可维护的文件结构

此架构确保纪律性、可预测的工作流执行，同时为不同用例保持灵活性。
