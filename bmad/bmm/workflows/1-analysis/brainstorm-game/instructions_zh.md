# 头脑风暴游戏 - 工作流程说明

```xml
<critical>工作流程执行引擎由以下文件管理：{project_root}/bmad/core/tasks/workflow.md</critical>
<critical>你必须已经加载并处理：{installed_path}/workflow.yaml</critical>
<critical>这是一个元工作流程，通过游戏特定上下文和额外的游戏设计技巧来协调CIS头脑风暴工作流程</critical>

<workflow>

  <step n="1" goal="加载游戏头脑风暴上下文和技巧">
    <action>从以下位置读取游戏上下文文档：{game_context}</action>
    <action>此上下文提供游戏特定指导，包括：
      - 游戏创意的重点领域（机制、叙事、体验等）
      - 游戏设计的关键考虑因素
      - 游戏头脑风暴的推荐技巧
      - 输出结构指导
    </action>
    <action>从以下位置加载游戏特定大脑技巧：{game_brain_methods}</action>
    <action>这些额外的技巧补充了标准CIS头脑风暴方法，增加了游戏设计重点的方法，如：
      - MDA框架探索
      - 核心循环头脑风暴
      - 玩家幻想挖掘
      - 类型混搭
      - 以及其他游戏特定的创意方法
    </action>
  </step>

  <step n="2" goal="使用游戏上下文调用CIS头脑风暴">
    <action>使用游戏上下文和额外技巧执行CIS头脑风暴工作流程</action>
    <invoke-workflow path="{cis_brainstorming}" data="{game_context}" techniques="{game_brain_methods}">
      CIS头脑风暴工作流程将：
      - 将游戏特定技巧与标准技巧合并
      - 呈现交互式头脑风暴技巧菜单
      - 引导用户完成选定的创意方法
      - 生成并捕获头脑风暴会议结果
      - 将输出保存到：{output_folder}/brainstorming-session-results-{{date}}.md
    </invoke-workflow>
  </step>

  <step n="3" goal="完成">
    <action>确认头脑风暴会议成功完成</action>
    <action>头脑风暴结果由CIS工作流程保存</action>
    <action>报告工作流程完成</action>
  </step>

</workflow>
```