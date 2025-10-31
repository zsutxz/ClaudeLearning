<!-- BMAD 方法 v6 工作流执行任务（简化版）-->

# 工作流

```xml
<task id="bmad/core/tasks/workflow.md" name="执行工作流">
  <objective>通过加载配置、遵循指令并产生输出来执行给定的工作流</objective>

  <llm critical="true">
    <mandate>始终读取完整文件 - 读取任何工作流相关文件时绝不使用 offset/limit</mandate>
    <mandate>指令是强制性的 - 无论是作为文件路径、步骤还是嵌入在 YAML、XML 或 markdown 中的列表</mandate>
    <mandate>按照精确顺序执行指令中的所有步骤</mandate>
    <mandate>在每个 "template-output" 标签后保存到模板输出文件</mandate>
    <mandate>绝不委托步骤 - 你要对每个步骤的执行负责</mandate>
  </llm>

  <WORKFLOW-RULES critical="true">
    <rule n="1">步骤按精确的数字顺序执行（1, 2, 3...）</rule>
    <rule n="2">可选步骤：除非激活 #yolo 模式，否则询问用户</rule>
    <rule n="3">模板输出标签：保存内容 → 显示给用户 → 在继续前获得批准</rule>
    <rule n="4">启发标签：立即执行，除非是 #yolo 模式（跳过所有启发）</rule>
    <rule n="5">在继续之前，用户必须批准每个主要部分，除非激活 #yolo 模式</rule>
  </WORKFLOW-RULES>

  <flow>
    <step n="1" title="加载并初始化工作流">
      <substep n="1a" title="加载配置并解析变量">
        <action>从提供的路径读取 workflow.yaml</action>
        <mandate>加载 config_source（所有模块必需）</mandate>
        <phase n="1">从 config_source 路径加载外部配置</phase>
        <phase n="2">用配置中的值解析所有 {config_source}: 引用</phase>
        <phase n="3">解析系统变量（date：系统生成）和路径（{project-root}，{installed_path}）</phase>
        <phase n="4">询问用户输入任何仍然未知的变量</phase>
      </substep>

      <substep n="1b" title="加载所需组件">
        <mandate>指令：从路径读取完整文件或嵌入列表（必需）</mandate>
        <check>如果是模板路径 → 读取完整模板文件</check>
        <check>如果是验证路径 → 记录路径供需要时加载</check>
        <check>如果 template: false → 标记为 action-workflow（否则为 template-workflow）</check>
        <note>数据文件（csv, json）→ 仅存储路径，在指令引用时按需加载</note>
      </substep>

      <substep n="1c" title="初始化输出" if="template-workflow">
        <action>用所有变量和 {{date}} 解析 default_output_file 路径</action>
        <action>如果输出目录不存在则创建</action>
        <action>如果是模板工作流 → 用占位符将模板写入输出文件</action>
        <action>如果是动作工作流 → 跳过文件创建</action>
      </substep>
    </step>

    <step n="2" title="处理每个指令步骤">
      <iterate>对于指令中的每个步骤：</iterate>

      <substep n="2a" title="处理步骤属性">
        <check>如果 optional="true" 且不是 #yolo → 询问用户是否包含</check>
        <check>如果 if="condition" → 评估条件</check>
        <check>如果 for-each="item" → 为每个项目重复步骤</check>
        <check>如果 repeat="n" → 重复步骤 n 次</check>
      </substep>

      <substep n="2b" title="执行步骤内容">
        <action>处理步骤指令（markdown 或 XML 标签）</action>
        <action>用值替换 {{variables}}（如果未知则询问用户）</action>
        <execute-tags>
          <tag><action> → 执行动作</tag>
          <tag><check> → 评估条件</tag>
          <tag><ask> → 提示用户并等待响应</tag>
          <tag><invoke-workflow> → 用给定输入执行另一个工作流</tag>
          <tag><invoke-task> → 执行指定任务</tag>
          <tag><goto step="x"> → 跳转到指定步骤</tag>
        </execute-tags>
      </substep>

      <substep n="2c" title="处理特殊输出标签">
        <if tag="template-output">
          <mandate>为此部分生成内容</mandate>
          <mandate>保存到文件（首次写入，后续编辑）</mandate>
          <action>显示检查点分隔符：━━━━━━━━━━━━━━━━━━━━━━━━</action>
          <action>显示生成的内容</action>
          <ask>继续 [c] 或编辑 [e]？等待响应</ask>
        </if>

        <if tag="elicit-required">
          <mandate critical="true">在呈现任何启发菜单之前，你必须使用 Read 工具读取 {project-root}/bmad/core/tasks/adv-elicit.md 文件</mandate>
          <action>用当前上下文加载并运行任务 {project-root}/bmad/core/tasks/adv-elicit.md</action>
          <action>显示启发菜单 5 个相关选项（列出 1-5 个选项，继续 [c] 或重新洗牌 [r]）</action>
          <mandate>暂停并等待用户选择</mandate>
        </if>
      </substep>

      <substep n="2d" title="步骤完成">
        <check>如果没有特殊标签且不是 #yolo：</check>
        <ask>继续下一步？（y/n/edit）</ask>
      </substep>
    </step>

    <step n="3" title="完成">
      <check>如果检查清单存在 → 运行验证</check>
      <check>如果 template: false → 确认动作完成</check>
      <check>否则 → 确认文档保存到输出路径</check>
      <action>报告工作流完成</action>
    </step>
  </flow>

  <execution-modes>
    <mode name="normal">在所有决策点完全用户交互</mode>
    <mode name="#yolo">跳过可选部分，跳过所有启发，最小化提示</mode>
  </execution-modes>

  <supported-tags desc="指令可以使用这些标签">
    <structural>
      <tag>step n="X" goal="..." - 用数字和目标定义步骤</tag>
      <tag>optional="true" - 步骤可以跳过</tag>
      <tag>if="condition" - 条件执行</tag>
      <tag>for-each="collection" - 迭代项目</tag>
      <tag>repeat="n" - 重复 n 次</tag>
    </structural>
    <execution>
      <tag>action - 要执行的必需动作</tag>
      <tag>check - 要评估的条件</tag>
      <tag>ask - 获取用户输入（等待响应）</tag>
      <tag>goto - 跳转到另一个步骤</tag>
      <tag>invoke-workflow - 调用另一个工作流</tag>
      <tag>invoke-task - 调用一个任务</tag>
    </execution>
    <output>
      <tag>template-output - 保存内容检查点</tag>
      <tag>elicit-required - 触发增强</tag>
      <tag>critical - 不能跳过</tag>
      <tag>example - 显示示例输出</tag>
    </output>
  </supported-tags>

  <llm final="true">
    <mandate>这是完整的工作流执行引擎</mandate>
    <mandate>你必须完全按照书面说明遵循指令并在步骤之间保持对话上下文</mandate>
    <mandate>如果困惑，重新阅读此任务、工作流 yaml 和任何 yaml 指示的文件</mandate>
  </llm>
</task>
```

**（译者注：本文档描述了 BMAD 方法的工作流执行引擎。这是一个专门用于执行结构化任务流程的系统，通过严格的步骤控制和规则约束来确保任务执行的一致性和可靠性。文档使用了大量的 XML 标签来定义各种执行逻辑和控制流，适合技术人员理解和实现。）**