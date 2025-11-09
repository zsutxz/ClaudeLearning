# workflow.xml 工作流执行引擎 - 中文技术翻译

**原文链接**: `d:\work\AI\ClaudeTest\bmad\core\tasks\workflow.xml`
**翻译时间**: 2025-11-09
**文章类型**: 核心技术引擎文档

---

```xml
<task id="bmad/core/tasks/workflow.xml" name="Execute Workflow">
  <objective>通过加载配置、遵循指令并生成输出来执行给定的工作流</objective>

  <llm critical="true">
    <mandate>始终读取完整文件 - 读取任何工作流相关文件时绝不使用偏移量/限制</mandate>
    <mandate>指令是强制性要求 - 可以是文件路径、步骤或YAML、XML、markdown中的嵌入列表</mandate>
    <mandate>严格按照精确顺序执行指令中的所有步骤</mandate>
    <mandate>在每个"template-output"标签后保存到模板输出文件</mandate>
    <mandate>绝不委托步骤 - 您负责每个步骤的执行</mandate>
  </llm>

  <WORKFLOW-RULES critical="true">
    <rule n="1">步骤按精确的数字顺序执行（1, 2, 3...）</rule>
    <rule n="2">可选步骤：询问用户，除非#yolo模式激活</rule>
    <rule n="3">template-output标签：保存内容 → 显示给用户 → 继续前获得批准</rule>
    <rule n="4">用户必须批准每个主要部分才能继续，除非#yolo模式激活</rule>
  </WORKFLOW-RULES>
```

## 🏗️ 工作流执行流程架构

### 阶段1：加载和初始化工作流

```xml
<step n="1" title="Load and Initialize Workflow">
  <substep n="1a" title="Load Configuration and Resolve Variables">
    <action>从提供路径读取workflow.yaml</action>
    <mandate>加载config_source（所有模块必需）</mandate>
    <phase n="1">从config_source路径加载外部配置</phase>
    <phase n="2">用配置值解析所有{config_source}: 引用</phase>
    <phase n="3">解析系统变量（date:系统生成）和路径（{project-root}, {installed_path}）</phase>
    <phase n="4">询问用户输入任何仍然未知的变量</phase>
  </substep>

  <substep n="1b" title="Load Required Components">
    <mandate>指令：从路径读取完整文件或嵌入列表（必需）</mandate>
    <check>如果是模板路径 → 读取完整模板文件</check>
    <check>如果是验证路径 → 记录路径供需要时加载</check>
    <check>如果template: false → 标记为action-workflow（否则为template-workflow）</check>
    <note>数据文件（csv, json）→ 仅存储路径，指令引用时按需加载</note>
  </substep>

  <substep n="1c" title="Initialize Output" if="template-workflow">
    <action>用所有变量和{{date}}解析default_output_file路径</action>
    <action>如果输出目录不存在则创建</action>
    <action>如果是template-workflow → 将模板与占位符写入输出文件</action>
    <action>如果是action-workflow → 跳过文件创建</action>
  </substep>
</step>
```

### 阶段2：处理每个指令步骤

```xml
<step n="2" title="Process Each Instruction Step">
  <iterate>对于指令中的每个步骤：</iterate>

  <substep n="2a" title="Handle Step Attributes">
    <check>如果optional="true"且非#yolo → 询问用户是否包含</check>
    <check>如果if="condition" → 评估条件</check>
    <check>如果for-each="item" → 为每个项目重复步骤</check>
    <check>如果repeat="n" → 重复步骤n次</check>
  </substep>

  <substep n="2b" title="Execute Step Content">
    <action>处理步骤指令（markdown或XML标签）</action>
    <action>将{{variables}}替换为值（如果未知则询问用户）</action>
    <execute-tags>
      <tag>action xml标签 → 执行动作</tag>
      <tag>check if="condition" xml标签 → 包装动作的条件块（需要结束标签&lt;/check&gt;）</tag>
      <tag>ask xml标签 → 提示用户并等待响应</tag>
      <tag>invoke-workflow xml标签 → 用给定输入执行另一个工作流</tag>
      <tag>invoke-task xml标签 → 执行指定任务</tag>
      <tag>goto step="x" → 跳转到指定步骤</tag>
    </execute-tags>
  </substep>

  <substep n="2c" title="Handle Special Output Tags">
    <if tag="template-output">
      <mandate>为此部分生成内容</mandate>
      <mandate>保存到文件（首次写入，后续编辑）</mandate>
      <action>显示检查点分隔符：━━━━━━━━━━━━━━━━━━━━━━━━</action>
      <action>显示生成的内容</action>
      <ask>继续[c]或编辑[e]？等待响应</ask>
    </if>
  </substep>

  <substep n="2d" title="Step Completion">
    <check>如果没有特殊标签且非#yolo：</check>
    <ask>继续下一步？（y/n/edit）</ask>
  </substep>
</step>
```

### 阶段3：完成

```xml
<step n="3" title="Completion">
  <check>如果检查列表存在 → 运行验证</check>
  <check>如果template: false → 确认动作已完成</check>
  <check>否则 → 确认文档已保存到输出路径</check>
  <action>报告工作流完成</action>
</step>
```

## ⚙️ 执行模式

```xml
<execution-modes>
  <mode name="normal">在所有决策点完全用户交互</mode>
  <mode name="#yolo">跳过可选部分，跳过所有启发式询问，最小化提示</mode>
</execution-modes>
```

## 🏷️ 支持的标签系统

### 结构性标签
```xml
<structural>
  <tag>step n="X" goal="..." - 用数字和目标定义步骤</tag>
  <tag>optional="true" - 步骤可以跳过</tag>
  <tag>if="condition" - 条件执行</tag>
  <tag>for-each="collection" - 迭代项目</tag>
  <tag>repeat="n" - 重复n次</tag>
</structural>
```

### 执行性标签
```xml
<execution>
  <tag>action - 要执行的必需动作</tag>
  <tag>action if="condition" - 单个条件动作（内联，无需结束标签）</tag>
  <tag>check if="condition">...</check> - 包装多个项目的条件块（需要结束标签）</tag>
  <tag>ask - 获取用户输入（等待响应）</tag>
  <tag>goto - 跳转到另一个步骤</tag>
  <tag>invoke-workflow - 调用另一个工作流</tag>
  <tag>invoke-task - 调用一个任务</tag>
</execution>
```

### 输出性标签
```xml
<output>
  <tag>template-output - 保存内容检查点</tag>
  <tag>critical - 不能跳过</tag>
  <tag>example - 显示示例输出</tag>
</output>
```

## 🔀 条件执行模式详解

### 单动作模式
```xml
<pattern type="single-action">
  <use-case>一个动作带一个条件</use-case>
  <syntax><action if="condition">执行某操作</action></syntax>
  <example><action if="file exists">加载文件</action></example>
  <rationale>对于单个项目更清晰简洁</rationale>
</pattern>
```

### 多动作块模式
```xml
<pattern type="multi-action-block">
  <use-case>同一条件下多个动作/标签</use-case>
  <syntax><check if="condition">
  <action>第一个动作</action>
  <action>第二个动作</action>
</check></syntax>
  <example><check if="validation fails">
  <action>记录错误</action>
  <goto step="1">重试</goto>
</check></example>
  <rationale>明确的范围边界防止歧义</rationale>
</pattern>
```

### 嵌套条件模式
```xml
<pattern type="nested-conditions">
  <use-case>Else/替代分支</use-case>
  <syntax><check if="condition A">...</check>
<check if="else">...</check></syntax>
  <rationale>用明确块实现清晰的分支逻辑</rationale>
</pattern>
```

## 🎯 核心引擎指令

```xml
<llm final="true">
  <mandate>这是完整的工作流执行引擎</mandate>
  <mandate>您必须完全按照书面说明遵循指令并在步骤间保持对话上下文</mandate>
  <mandate>如果困惑，重新读取此任务、workflow yaml和任何yaml指示的文件</mandate>
</llm>
```

---

## 📊 技术架构解析

### 🏛️ 核心设计原则
- **原子性执行**: 每个步骤都是原子操作，要么完全成功，要么完全失败
- **状态隔离**: 步骤之间通过明确的检查点机制保持状态一致性
- **条件控制**: 支持复杂的条件分支逻辑，确保工作流的灵活性
- **用户交互**: 在关键决策点提供用户确认机制

### 🔧 关键技术特性
1. **变量解析系统**: 支持{config_source}、{project-root}等动态变量
2. **模板引擎**: 支持template-output的增量内容生成
3. **条件执行**: 三种条件模式满足不同复杂度需求
4. **错误处理**: 通过检查点和用户确认提供容错机制

### 🚀 性能优化
- **延迟加载**: 数据文件仅在需要时加载
- **增量保存**: template-output支持增量内容更新
- **并行友好**: 支持for-each迭代模式

---

*📝 翻译说明：本文档为BMAD框架的核心工作流引擎规范，翻译严格遵循技术文档标准，保留所有XML标签和代码结构，确保技术实现的准确性。*