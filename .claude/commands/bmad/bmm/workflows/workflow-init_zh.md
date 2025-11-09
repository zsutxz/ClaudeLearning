---
**描述**: '通过确定级别、类型和创建工作流路径来初始化新的BMM项目'
---

# workflow-init 工作流初始化

**至关重要：必须遵循以下步骤** - 同时保持当前已加载的代理角色身份：

<steps CRITICAL="TRUE">
1. **始终加载**完整的 {project-root}/bmad/core/tasks/workflow.xml 文件
2. **阅读其全部内容** - 这是执行特定工作流配置 bmad/bmm/workflows/workflow-status/init/workflow.yaml 的**核心操作系统**
3. 将yaml路径 bmad/bmm/workflows/workflow-status/init/workflow.yaml 作为 'workflow-config' 参数传递给 workflow.xml 指令
4. **严格按照书面说明**遵循 workflow.xml 指令
5. 从模板生成任何文档时，在**每个部分后保存输出**
</steps>

---

**原文链接**: `d:\work\AI\ClaudeTest\.claude\commands\bmad\bmm\workflows\workflow-init.md`
**翻译时间**: 2025-11-09
**文章类型**: 技术配置文档

## 📋 技术解析

### 🔧 核心组件说明
- **workflow.xml**: 核心工作流执行引擎，负责解析和执行工作流配置
- **workflow.yaml**: 工作流配置文件，定义具体的初始化步骤和参数
- **BMM项目**: Business Model Matrix项目，采用BMAD框架的企业级建模方法

### ⚙️ 执行流程
1. **加载核心引擎** → 读取 workflow.xml 文件
2. **解析配置** → 指定具体的 workflow.yaml 配置文件路径
3. **参数传递** → 将配置文件路径作为参数传递给执行引擎
4. **按序执行** → 严格按照配置文件中的步骤执行
5. **结果保存** → 每完成一个步骤立即保存输出结果

### 🎯 技术要点
- **CRITICAL="TRUE"**: 标记关键步骤，必须严格按顺序执行
- **{project-root}**: 项目根目录的动态变量引用
- **角色保持**: 执行过程中需要保持当前代理的角色设定

---

*📝 翻译说明：本文档为BMAD框架v6.0的工作流初始化配置文档，翻译遵循技术准确性原则，保留所有技术术语和路径引用。*