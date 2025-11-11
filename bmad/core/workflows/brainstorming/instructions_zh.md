# 头脑风暴会话指令

## 工作流

<workflow>
<critical>工作流执行引擎由以下文件管理：{project_root}/bmad/core/tasks/workflow.xml</critical>
<critical>您必须已经加载并处理过：{project_root}/bmad/core/workflows/brainstorming/workflow.yaml</critical>

<step n="1" goal="会话设置">

<action>检查工作流调用时是否提供了上下文数据</action>

<check if="data attribute was passed to this workflow">
  <action>从数据文件路径加载上下文文档</action>
  <action>研究领域知识和会话焦点</action>
  <action>使用提供的上下文来引导会话</action>
  <action>确认专注的头脑风暴目标</action>
  <ask response="session_refinement">我看到我们要围绕上下文中概述的特定领域进行头脑风暴。您想探索哪个特定方面？</ask>
</check>

<check if="no context data provided">
  <action>继续进行通用上下文收集</action>
  <ask response="session_topic">1. 我们要针对什么进行头脑风暴？</ask>
  <ask response="stated_goals">2. 有什么约束或参数需要我们记住吗？</ask>
  <ask>3. 目标是广泛探索还是专注于特定方面的创意构思？</ask>

<critical>在继续之前等待用户回应。这个上下文将塑造整个会话。</critical>
</check>

<template-output>session_topic, stated_goals</template-output>

</step>

<step n="2" goal="呈现方法选项">

基于步骤1的上下文，呈现这四种方法选项：

<ask response="selection">
1. **用户选择技术** - 浏览并从我们的库中选择特定技术
2. **AI推荐技术** - 让我根据您的上下文推荐技术
3. **随机技术选择** - 用意外的创意方法给自己惊喜
4. **渐进式技术流程** - 从广泛开始，然后系统化缩小范围

您更喜欢哪种方法？（输入1-4）
</ask>

  <step n="2a" title="用户选择技术" if="selection==1">
    <action>从 {brain_techniques} CSV文件加载技术</action>
    <action>解析：类别、技术名称、描述、引导提示</action>

    <check if="strong context from Step 1 (specific problem/goal)">
      <action>基于stated_goals识别2-3个最相关的类别</action>
      <action>首先呈现这些类别，每个类别3-5个技术</action>
      <action>提供"显示所有类别"选项</action>
    </check>

    <check if="open exploration">
      <action>显示所有7个类别及有帮助的描述</action>
    </check>

    用于引导选择的类别描述：
    - **结构化**：彻底探索的系统化框架
    - **创意**：突破性思维的创新方法
    - **协作**：团队动态和团队创意方法
    - **深度**：根本原因和洞察的分析方法
    - **戏剧性**：激进视角的趣味探索
    - **狂野**：突破边界的极端思维
    - **内省之乐**：内在智慧和真实探索

    对每个类别，显示3-5个代表性技术及简要描述。

    用您自己的声音询问："哪些技术让您感兴趣？您可以通过名称、数字选择，或者告诉我您被什么吸引。"

  </step>

  <step n="2b" title="AI推荐技术" if="selection==2">
    <action>回顾 {brain_techniques} 并选择最适合上下文的3-5个技术</action>

    分析框架：

    1. **目标分析：**
       - 创新/新想法 → 创意、狂野类别
       - 问题解决 → 深度、结构化类别
       - 团队建设 → 协作类别
       - 个人洞察 → 内省之乐类别
       - 战略规划 → 结构化、深度类别

    2. **复杂度匹配：**
       - 复杂/抽象主题 → 深度、结构化技术
       - 熟悉/具体主题 → 创意、狂野技术
       - 情感/个人主题 → 内省之乐技术

    3. **能量/语调评估：**
       - 用户语言正式 → 结构化、分析技术
       - 用户语言有趣 → 创意、戏剧性、狂野技术
       - 用户语言反思 → 内省之乐、深度技术

    4. **可用时间：**
       - <30分钟 → 1-2个专注技术
       - 30-60分钟 → 2-3个互补技术
       - >60分钟 → 考虑渐进流程（3-5个技术）

    用您自己的声音呈现推荐，包括：
    - 技术名称（类别）
    - 为什么适合他们的上下文（具体）
    - 他们将发现什么（结果）
    - 预估时间

    示例结构：
    "基于您的目标[X]，我推荐：

    1. **[技术名称]**（类别）- X分钟
       原因：[基于他们上下文的具体原因]
       结果：[他们将生成/发现什么]

    2. **[技术名称]**（类别）- X分钟
       原因：[具体原因]
       结果：[预期结果]

    准备开始吗？[c] 还是您想要不同的技术？[r]"

  </step>

  <step n="2c" title="单一随机技术选择" if="selection==3">
    <action>从 {brain_techniques} CSV加载所有技术</action>
    <action>使用真随机选择技术</action>
    <action>为意外选择建立兴奋感</action>
    <format>
      让我们加点变化！宇宙选择了：
      **{{technique_name}}** - {{description}}
    </format>
  </step>

  <step n="2d" title="渐进式流程" if="selection==4">
    <action>基于会话上下文设计通过 {brain_techniques} 的渐进之旅</action>
    <action>分析步骤1中的stated_goals和session_topic</action>
    <action>确定会话长度（如果未说明则询问）</action>
    <action>选择3-4个互补且相互构建的技术</action>

    旅程设计原则：
    - 从发散探索开始（广泛、生成性）
    - 通过专注深入（分析性或创意性）
    - 以汇聚综合结束（整合、优先排序）

    按目标的常见模式：
    - **问题解决**：思维导图 → 五个为什么 → 假设逆转
    - **创新**：如果情景 → 类比思维 → 强制关联
    - **战略**：第一性原理 → SCAMPER → 六顶思考帽
    - **团队建设**：脑力写作 → 是的，而且构建 → 角色扮演

    用以下内容呈现您推荐的旅程：
    - 技术名称和简要原因
    - 每个的预估时间（10-20分钟）
    - 总会话持续时间
    - 序列的理由

    用您自己的声音询问："这个流程听起来怎么样？我们可以在进行中调整。"

  </step>

</step>

<step n="3" goal="交互式执行技术">

<critical>
记住：您是精通头脑风暴的创意引导者：作为引导者引导用户通过问题、提示和示例生成他们自己的想法。除非他们明确要求，否则不要为他们进行头脑风暴。
</critical>

<facilitation-principles>
  - 提问，而非告知 - 用问题引出想法
  - 构建，而非评判 - 使用"是的，而且..."永远不要"不，但是..."
  - 数量优于质量 - 目标是60分钟100个想法
  - 延迟判断 - 评估在生成之后进行
  - 保持好奇 - 对他们的想法表现出真诚兴趣
</facilitation-principles>

对于每个技术：

1. **介绍技术** - 使用CSV中的描述解释它如何工作
2. **提供第一个提示** - 使用CSV中的引导提示（管道分隔的提示）
   - 解析引导提示字段并选择合适的提示
   - 这些是您的对话开场白和后续问题
3. **等待他们的回应** - 让他们生成想法
4. **基于他们的想法构建** - 使用"是的，而且..."或"这让我想起..."或"如果我们也..."
5. **提出后续问题** - "告诉我更多关于..."，"那将如何工作？"，"还有什么？"
6. **监控能量** - 检查："您对这个{会话/技术/进展}感觉如何？"
   - 如果能量高 → 继续推动当前技术
   - 如果能量低 → "我们应该尝试不同的角度还是快速休息一下？"
7. **保持动力** - 庆祝："太好了！到目前为止您已经生成了[X]个想法！"
8. **记录一切** - 捕获所有想法用于最终报告

<example>
任何技术的引导流程示例：

1. 介绍："让我们试试[技术名称]。[根据他们的上下文调整CSV中的描述]。"

2. 第一个提示：从{brain_techniques}中提取第一个引导提示并适应他们的主题
   - CSV："如果我们有无限资源会怎样？"
   - 适应："如果您对[他们的主题]有无限资源会怎样？"

3. 基于回应构建：使用"是的，而且..."或"这让我想起..."或"基于此构建..."

4. 下一个提示：准备好推进时提取下一个引导提示

5. 监控能量：10-15分钟后，检查他们是否想继续或切换

CSV提供提示 - 您的角色是用您独特的声音自然引导。
</example>

继续参与技术，直到用户表示他们想要：

- 切换到不同的技术（"准备好尝试不同的方法了吗？"）
- 将当前想法应用于新技术
- 移动到汇聚阶段
- 结束会话

<energy-checkpoint>
  使用技术15-20分钟后，检查："我们应该继续这个技术还是尝试新的？"
</energy-checkpoint>

<template-output>technique_sessions</template-output>

</step>

<step n="4" goal="汇聚阶段 - 组织想法">

<transition-check>
  "我们已经生成了很多很棒的想法！您准备好开始整理它们，还是想继续探索？"
</transition-check>

准备整合时：

引导用户分类他们的想法：

1. **回顾所有生成的想法** - 显示到目前为止捕获的所有内容
2. **识别模式** - "我注意到几个关于X的想法...还有关于Y的..."
3. **分组到类别中** - 与用户合作在技术和跨技术中组织想法

询问："查看所有这些想法，哪些感觉像是：

- <ask response="immediate_opportunities">我们可以立即实施的快速胜利？</ask>
- <ask response="future_innovations">需要更多发展的有前景概念？</ask>
- <ask response="moonshots">值得长期追求的大胆宏伟目标？</ask>

<template-output>immediate_opportunities, future_innovations, moonshots</template-output>

</step>

<step n="5" goal="提取洞察和主题">

分析会话以识别更深层次的模式：

1. **识别重复主题** - 哪些概念出现在多个技术中？-> key_themes
2. **浮现关键洞察** - 过程中出现了什么认识？-> insights_learnings
3. **注意意外连接** - 发现了什么意外关系？-> insights_learnings

<invoke-task halt="true">{project_root}/bmad/core/tasks/adv-elicit.xml</invoke-task>

<template-output>key_themes, insights_learnings</template-output>

</step>

<step n="6" goal="行动计划">

<energy-check>
  "到目前为止工作很棒！您对最终规划阶段的能量如何？"
</energy-check>

与用户合作确定优先级并规划下一步：

<ask>在我们生成的所有想法中，哪3个感觉最重要去追求？</ask>

对于每个优先级：

1. 询问为什么这是优先级
2. 识别具体的下一步
3. 确定资源需求
4. 设定现实的时间表

<template-output>priority_1_name, priority_1_rationale, priority_1_steps, priority_1_resources, priority_1_timeline</template-output>
<template-output>priority_2_name, priority_2_rationale, priority_2_steps, priority_2_resources, priority_2_timeline</template-output>
<template-output>priority_3_name, priority_3_rationale, priority_3_steps, priority_3_resources, priority_3_timeline</template-output>

</step>

<step n="7" goal="会话反思">

以会话的元分析结束：

1. **哪些运作良好** - 哪些技术或时刻最富有成效？
2. **需要进一步探索的领域** - 哪些主题值得更深入调查？
3. **推荐的后续技术** - 什么方法会帮助继续这项工作？
4. **出现的问题** - 出现了什么我们应该解决的新问题？
5. **下次会话规划** - 何时以及接下来我们应该头脑风暴什么？

<template-output>what_worked, areas_exploration, recommended_techniques, questions_emerged</template-output>
<template-output>followup_topics, timeframe, preparation</template-output>

</step>

<step n="8" goal="生成最终报告">

将所有捕获的内容编译到结构化报告模板中：

1. 计算所有技术生成的想法总数
2. 列出所有使用的技术及持续时间估算
3. 根据模板结构格式化所有内容
4. 确保所有占位符都填充了实际内容

<template-output>agent_role, agent_name, user_name, techniques_list, total_ideas</template-output>

</step>

</workflow>