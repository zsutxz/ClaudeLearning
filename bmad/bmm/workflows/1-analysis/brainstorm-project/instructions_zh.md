# 项目头脑风暴 - 工作流指令

```xml
<critical>工作流执行引擎由以下文件管理：{project_root}/bmad/core/tasks/workflow.xml</critical>
<critical>您必须已经加载并处理过：{installed_path}/workflow.yaml</critical>
<critical>所有响应均使用 {communication_language} 进行交流</critical>
<critical>这是一个元工作流，用于编排带有项目特定上下文的CIS头脑风暴工作流</critical>

<workflow>

  <step n="1" goal="验证工作流准备状态" tag="workflow-status">
    <action>检查 {output_folder}/bmm-workflow-status.yaml 是否存在</action>

    <check if="status file not found">
      <output>未找到工作流状态文件。头脑风暴是可选的 - 您可以在没有状态跟踪的情况下继续。</output>
      <action>设置 standalone_mode = true</action>
    </check>

    <check if="status file found">
      <action>加载完整文件：{output_folder}/bmm-workflow-status.yaml</action>
      <action>解析 workflow_status 部分</action>
      <action>检查"brainstorm-project"工作流的状态</action>
      <action>从YAML元数据中获取 project_level</action>
      <action>找到第一个未完成的工作流（下一个预期的工作流）</action>

      <check if="brainstorm-project status is file path (already completed)">
        <output>⚠️ 头脑风暴会话已完成：{{brainstorm-project status}}</output>
        <ask>重新运行将创建新会话。继续吗？(y/n)</ask>
        <check if="n">
          <output>正在退出。使用 workflow-status 查看您的下一步。</output>
          <action>退出工作流</action>
        </check>
      </check>

      <check if="brainstorm-project is not the next expected workflow (anything after brainstorm-project is completed already)">
        <output>⚠️ 下一个预期工作流：{{next_workflow}}。头脑风暴顺序不当。</output>
        <ask>无论如何都要继续头脑风暴吗？(y/n)</ask>
        <check if="n">
          <output>正在退出。改为运行 {{next_workflow}}。</output>
          <action>退出工作流</action>
        </check>
      </check>

      <action>设置 standalone_mode = false</action>
    </check>
  </step>

  <step n="2" goal="加载项目头脑风暴上下文">
    <action>从以下位置读取项目上下文文档：{project_context}</action>
    <action>此上下文提供项目特定的指导，包括：
      - 项目创意的重点领域
      - 软件/产品项目的关键考虑因素
      - 项目头脑风暴的推荐技术
      - 输出结构指导
    </action>
  </step>

  <step n="3" goal="使用项目上下文调用核心头脑风暴">
    <action>执行带有项目上下文的CIS头脑风暴工作流</action>
    <invoke-workflow path="{core_brainstorming}" data="{project_context}">
      CIS头脑风暴工作流将：
      - 呈现交互式头脑风暴技术菜单
      - 引导用户完成选定的创意方法
      - 生成并捕获头脑风暴会话结果
      - 保存输出到：{output_folder}/brainstorming-session-results-{{date}}.md
    </invoke-workflow>
  </step>

  <step n="4" goal="更新状态并完成" tag="workflow-status">
    <check if="standalone_mode != true">
      <action>加载完整文件：{output_folder}/bmm-workflow-status.yaml</action>
      <action>找到工作流状态键"brainstorm-project"</action>
      <critical>只写入文件路径作为状态值 - 不包含其他文本、注释或元数据</critical>
      <action>更新 workflow_status["brainstorm-project"] = "{output_folder}/bmm-brainstorming-session-{{date}}.md"</action>
      <action>保存文件，保留所有注释和结构，包括状态定义</action>

      <action>在 workflow_status 中找到第一个未完成的工作流（下一个要做的工作流）</action>
      <action>根据下一个工作流从路径文件中确定下一个代理</action>
    </check>

    <output>**✅ 头脑风暴会话已完成，{user_name}！**

**会话结果：**

- 头脑风暴结果已保存到：{output_folder}/bmm-brainstorming-session-{{date}}.md

{{#if standalone_mode != true}}
**状态已更新：**

- 进度跟踪已更新

**下一步：**

- **下一步必需：** {{next_workflow}} ({{next_agent}} 代理)
- **可选：** 在继续之前，您可以运行其他分析工作流（研究、产品简介）

随时使用以下命令检查状态：`workflow-status`
{{else}}
**下一步：**

由于没有工作流正在进行中：

- 如果不确定下一步该做什么，请参考BMM工作流指南
- 或者运行 `workflow-init` 创建工作流路径并获得指导的下一步
{{/if}}
    </output>
  </step>

</workflow>
```

---

**翻译说明：**
- 保持原始的技术术语准确性和完整性
- 翻译为通俗易懂的简体中文，便于理解
- 完整保留原始的XML格式和结构
- 对关键概念保持技术准确性，如"workflow"、"agent"等专业术语