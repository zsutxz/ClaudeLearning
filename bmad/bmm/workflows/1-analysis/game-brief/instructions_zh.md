# 游戏简报 - 交互式工作流程说明

<critical>工作流程执行引擎受以下文件约束：{project-root}/bmad/core/tasks/workflow.md</critical>
<critical>您必须已经加载并处理了：{installed_path}/workflow.yaml</critical>

<workflow>

<step n="0" goal="初始化游戏简报会话">
<action>欢迎用户参与游戏简报创建流程</action>
<action>这是一个协作过程，用于定义您的游戏愿景</action>
<ask>您游戏的工作标题是什么？</ask>
<template-output>game_name</template-output>
</step>

<step n="1" goal="收集可用输入和背景信息">
<action>检查用户有哪些可用输入：</action>
<ask>您是否有以下任何文档来帮助提供简报信息？

1. 市场研究或玩家数据
2. 头脑风暴结果或游戏马拉松原型
3. 竞争对手游戏分析
4. 初始游戏想法或设计笔记
5. 参考游戏列表
6. 没有 - 让我们从头开始

请分享您拥有的任何文档或选择选项6。</ask>

<action>加载并分析任何提供的文档</action>
<action>从输入文档中提取关键见解和主题</action>

<ask>基于您分享的内容（或从头开始），告诉我：

- 您想创造什么样的核心游戏体验？
- 玩家应该有什么样的情感或感受？
- 是什么激发了这款游戏的想法？</ask>

<template-output>initial_context</template-output>
</step>

<step n="2" goal="选择协作模式">
<ask>您希望如何完成简报？

**1. 交互模式** - 我们将逐步完成每个部分，边讨论边完善
**2. YOLO模式** - 我将根据我们的对话生成完整的草稿，然后我们一起完善

哪种方法最适合您？</ask>

<action>存储用户的模式偏好</action>
<template-output>collaboration_mode</template-output>
</step>

<step n="3" goal="定义游戏愿景" if="collaboration_mode == 'interactive'">
<ask>让我们捕捉您的游戏愿景。

**核心概念** - 用一句话描述您的游戏是什么？
示例："在神秘塔楼中攀登的Roguelike卡牌构筑游戏"

**电梯演讲** - 用2-3句话向发行商或玩家介绍您的游戏。
示例："Slay the Spire将卡牌游戏和Roguelike融合在一起。构筑独特的卡组，遇见奇异的生物，发现强大力量的遗物，击败塔楼。"

**愿景声明** - 这款游戏的宏伟目标是什么？您想创造什么样的体验？
示例："创造一个深度可重玩的战术卡牌游戏，奖励战略思维同时保持随机性的刺激。每次运行都应该感觉独特但公平。"

您的回答：</ask>

<action>帮助完善核心概念，使其清晰且有吸引力</action>
<action>确保电梯演讲简洁但能吸引注意力</action>
<action>指导愿景声明具有启发性但可实现</action>

<template-output>core_concept</template-output>
<template-output>elevator_pitch</template-output>
<template-output>vision_statement</template-output>
</step>

<step n="4" goal="识别目标市场" if="collaboration_mode == 'interactive'">
<ask>谁会玩您的游戏？

**主要受众：**

- 年龄范围
- 游戏经验水平（休闲、核心、硬核）
- 偏好类型
- 平台偏好
- 典型游戏会话长度
- 为什么这款游戏会吸引他们？

**次要受众**（如适用）：

- 还有谁可能喜欢这款游戏？
- 他们的需求可能有何不同？

**市场背景：**

- 市场机会是什么？
- 是否有类似的成功游戏？
- 竞争格局如何？
- 为什么现在是这款游戏的合适时机？</ask>

<action>推动超越"喜欢有趣游戏的人"的具体性</action>
<action>帮助识别现实且可触及的受众</action>
<action>记录市场证据或假设</action>

<template-output>primary_audience</template-output>
<template-output>secondary_audience</template-output>
<template-output>market_context</template-output>
</step>

<step n="5" goal="定义游戏基础" if="collaboration_mode == 'interactive'">
<ask>让我们定义您的核心游戏玩法。

**核心游戏支柱**（2-4个基本要素）：
这些是定义您游戏的支柱。一切都应该支持这些支柱。
示例：

- "精确控制 + 挑战性战斗 + 奖励探索"（Hollow Knight）
- "涌现故事 + 生存紧张感 + 创造性问题解决"（RimWorld）
- "战略深度 + 快速会话 + 大规模可重玩性"（Into the Breach）

**主要机制：**
玩家实际上做什么？

- 核心动作（跳跃、射击、建造、管理等）
- 关键系统（战斗、资源管理、进度等）
- 交互模式（实时、回合制等）

**玩家体验目标：**
您在为什么样的情感和体验进行设计？
示例：紧张感和释放感、掌控感和成长感、创造力和表达感、发现感和惊喜感

您的游戏基础：</ask>

<action>确保支柱具体且可衡量</action>
<action>专注于玩家动作，而非实现细节</action>
<action>将机制与情感体验联系起来</action>

<template-output>core_gameplay_pillars</template-output>
<template-output>primary_mechanics</template-output>
<template-output>player_experience_goals</template-output>
</step>

<step n="6" goal="定义范围和约束" if="collaboration_mode == 'interactive'">
<ask>让我们建立现实的约束条件。

**目标平台：**

- PC（Steam、itch.io、Epic）？
- 主机（哪些）？
- 移动设备（iOS、Android）？
- 网页浏览器？
- 如果有多个，优先顺序是什么？

**开发时间线：**

- 目标发布日期或时间范围？
- 是否有固定截止日期（游戏马拉松、资金里程碑）？
- 分阶段发布（抢先体验、测试版）？

**预算考虑：**

- 自筹资金、资助资金、发行商支持？
- 资产创建预算（艺术、音频、配音）？
- 营销预算？
- 工具和软件成本？

**团队资源：**

- 团队规模和角色？
- 全职还是兼职？
- 可用技能vs所需技能？
- 外包计划？

**技术约束：**

- 引擎偏好或要求？
- 性能目标（帧率、加载时间）？
- 文件大小限制？
- 可访问性要求？</ask>

<action>帮助用户对范围保持现实</action>
<action>早期识别潜在阻塞因素</action>
<action>记录关于资源的假设</action>

<template-output>target_platforms</template-output>
<template-output>development_timeline</template-output>
<template-output>budget_considerations</template-output>
<template-output>team_resources</template-output>
<template-output>technical_constraints</template-output>
</step>

<step n="7" goal="建立参考框架" if="collaboration_mode == 'interactive'">
<ask>让我们识别您的参考游戏和定位。

**灵感游戏：**
列出3-5款激发此项目灵感的游戏。对于每款：

- 游戏名称
- 您从中获得什么（机制、感觉、艺术风格等）
- 您不从中获得什么

**竞争分析：**
哪些游戏与您的游戏最相似？

- 直接竞争对手（非常相似的游戏）
- 间接竞争对手（以不同方式解决相同玩家需求）
- 他们做得好的地方
- 他们做得不好的地方
- 您的游戏将有什么不同

**关键差异化点：**
是什么让您的游戏独一无二？

- 您的亮点是什么？
- 为什么玩家会选择您的游戏而不是其他选择？
- 您能做什么而其他人不能或不会做的？</ask>

<action>帮助识别真正的差异化vs"只是更好"</action>
<action>寻找具体的、明确的差异</action>
<action>验证差异化点对玩家来说确实有价值</action>

<template-output>inspiration_games</template-output>
<template-output>competitive_analysis</template-output>
<template-output>key_differentiators</template-output>
</step>

<step n="8" goal="定义内容框架" if="collaboration_mode == 'interactive'">
<ask>让我们范围化您的内容需求。

**世界与设定：**

- 您的游戏在何时何地发生？
- 需要多少世界构建？
- 叙事是否重要（关键、支持、最小）？
- 现实世界还是奇幻/科幻？

**叙事方法：**

- 故事驱动、故事轻松还是无故事？
- 线性、分支还是涌现叙事？
- 过场动画、对话、环境叙事？
- 需要多少写作？

**内容量：**
估算范围：

- 典型游戏时长多长？
- 有多少关卡/阶段/区域？
- 重玩方法（程序化、解锁、多重路径）？
- 资产量（角色、敌人、物品、环境）？</ask>

<action>帮助现实地估算内容</action>
<action>识别稍后是否需要叙事工作流</action>
<action>标记需要规划的内容密集区域</action>

<template-output>world_setting</template-output>
<template-output>narrative_approach</template-output>
<template-output>content_volume</template-output>
</step>

<step n="9" goal="定义艺术和音频方向" if="collaboration_mode == 'interactive'">
<ask>您的游戏应该看起来和听起来像什么？

**视觉风格：**

- 艺术风格（像素艺术、低多边形、手绘、写实等）
- 色彩调色板和情绪
- 具有相似美学的参考图像或游戏
- 2D还是3D？
- 动画要求

**音频风格：**

- 音乐类型和情绪
- 音效方法（写实、风格化、复古）
- 配音需求（完整、部分、无）？
- 音频对游戏玩法的重要性（关键还是支持）

**制作方法：**

- 内部创建资产还是外包？
- 资产商店使用？
- 生成式/AI工具？
- 风格复杂度vs团队能力？</ask>

<action>确保艺术/音频愿景与预算和团队技能保持一致</action>
<action>识别潜在的制作瓶颈</action>
<action>注意是否需要风格指南</action>

<template-output>visual_style</template-output>
<template-output>audio_style</template-output>
<template-output>production_approach</template-output>
</step>

<step n="10" goal="评估风险" if="collaboration_mode == 'interactive'">
<ask>让我们诚实地识别潜在风险。

**关键风险：**

- 什么可能阻止这款游戏完成？
- 什么可能让它不好玩？
- 您在做什么假设可能是错误的？

**技术挑战：**

- 是否有任何未经证实的技术元素？
- 性能担忧？
- 平台特定挑战？
- 中间件或工具依赖？

**市场风险：**

- 市场是否饱和？
- 您是否依赖趋势或平台？
- 竞争担忧？
- 可发现性挑战？

**缓解策略：**
对于每个主要风险，您的计划是什么？

- 您将如何验证假设？
- 备用计划是什么？
- 您能否早期原型化风险元素？</ask>

<action>鼓励诚实的风险评估</action>
<action>专注于可操作的缓解，而不仅仅是担忧</action>
<action>按影响和可能性对风险进行优先级排序</action>

<template-output>key_risks</template-output>
<template-output>technical_challenges</template-output>
<template-output>market_risks</template-output>
<template-output>mitigation_strategies</template-output>
</step>

<step n="11" goal="定义成功标准" if="collaboration_mode == 'interactive'">
<ask>成功看起来像什么？

**MVP定义：**
绝对最小的可玩版本是什么？

- 核心循环必须有趣且完整
- 仅包含基本内容
- 以后可以添加什么？
- 您什么时候知道MVP"完成"了？

**成功指标：**
您将如何衡量成功？

- 获得的玩家数量
- 留存率（日留存、周留存）
- 会话时长
- 完成率
- 评测分数
- 收入目标（如果是商业游戏）
- 社区参与度

**发布目标：**
您发布的具体目标是什么？

- 首月销售/下载量？
- 评测分数目标？
- 主播/媒体报道目标？
- 社区规模目标？</ask>

<action>推动具体的、可衡量的目标</action>
<action>区分MVP和完整版本</action>
<action>确保目标在给定资源下是现实的</action>

<template-output>mvp_definition</template-output>
<template-output>success_metrics</template-output>
<template-output>launch_goals</template-output>
</step>

<step n="12" goal="识别立即下一步行动" if="collaboration_mode == 'interactive'">
<ask>接下来需要发生什么？

**立即行动：**
在此简报之后您应该立即做什么？

- 原型化一个核心机制？
- 创建艺术风格测试？
- 验证技术可行性？
- 构建垂直切片？
- 与目标受众进行游戏测试？

**研究需求：**
您还需要学习什么？

- 市场验证？
- 技术概念验证？
- 玩家兴趣测试？
- 竞争深入分析？

**开放问题：**
您还不确定什么？

- 需要解决的设计问题
- 技术未知数
- 市场验证需求
- 资源/预算问题</ask>

<action>创建可操作的下一步行动</action>
<action>按重要性和依赖性进行优先级排序</action>
<action>识别需要解决的阻塞因素</action>

<template-output>immediate_actions</template-output>
<template-output>research_needs</template-output>
<template-output>open_questions</template-output>
</step>

<!-- YOLO模式 - 生成所有内容然后完善 -->
<step n="3" goal="生成完整简报草稿" if="collaboration_mode == 'yolo'">
<action>基于初始上下文和任何提供的文档，生成涵盖所有部分的完整游戏简报</action>
<action>在信息缺失的地方做出合理假设</action>
<action>用[需要确认]标签标记需要用户验证的区域</action>

<template-output>core_concept</template-output>
<template-output>elevator_pitch</template-output>
<template-output>vision_statement</template-output>
<template-output>primary_audience</template-output>
<template-output>secondary_audience</template-output>
<template-output>market_context</template-output>
<template-output>core_gameplay_pillars</template-output>
<template-output>primary_mechanics</template-output>
<template-output>player_experience_goals</template-output>
<template-output>target_platforms</template-output>
<template-output>development_timeline</template-output>
<template-output>budget_considerations</template-output>
<template-output>team_resources</template-output>
<template-output>technical_constraints</template-output>
<template-output>inspiration_games</template-output>
<template-output>competitive_analysis</template-output>
<template-output>key_differentiators</template-output>
<template-output>world_setting</template-output>
<template-output>narrative_approach</template-output>
<template-output>content_volume</template-output>
<template-output>visual_style</template-output>
<template-output>audio_style</template-output>
<template-output>production_approach</template-output>
<template-output>key_risks</template-output>
<template-output>technical_challenges</template-output>
<template-output>market_risks</template-output>
<template-output>mitigation_strategies</template-output>
<template-output>mvp_definition</template-output>
<template-output>success_metrics</template-output>
<template-output>launch_goals</template-output>
<template-output>immediate_actions</template-output>
<template-output>research_needs</template-output>
<template-output>open_questions</template-output>

<action>向用户呈现完整草稿</action>
<ask>这是完整的游戏简报草稿。您想要调整或完善什么？</ask>
</step>

<step n="4" goal="完善简报部分" repeat="until-approved" if="collaboration_mode == 'yolo'">
<ask>您想要完善哪个部分？

1. 游戏愿景
2. 目标市场
3. 游戏基础
4. 范围和约束
5. 参考框架
6. 内容框架
7. 艺术与音频方向
8. 风险评估
9. 成功标准
10. 下一步行动
11. 保存并继续</ask>

<action>与用户合作完善所选部分</action>
<action>更新相关模板输出</action>
</step>

<!-- 两种模式的最终步骤 -->
<step n="13" goal="创建执行摘要">
<action>将所有部分综合成引人注目的执行摘要</action>
<action>包括：
- 1-2句话的游戏概念
- 目标受众和市场
- 核心游戏支柱
- 关键差异化点
- 成功愿景</action>

<template-output>executive_summary</template-output>
</step>

<step n="14" goal="汇编支持材料">
<action>如果提供了研究文档，创建关键发现摘要</action>
<action>记录在过程中收到的任何利益相关者输入</action>
<action>汇编参考游戏和资源列表</action>

<template-output>research_summary</template-output>
<template-output>stakeholder_input</template-output>
<template-output>references</template-output>
</step>

<step n="15" goal="最终审查和交接">
<action>生成完整的游戏简报文档</action>
<action>审查所有部分的完整性和一致性</action>
<action>用[设计待办]标签标记需要注意设计的区域</action>

<ask>游戏简报已完成！您想要：

1. 审查整个文档
2. 进行最终调整
3. 保存并准备GDD创建

此简报将作为创建游戏设计文档（GDD）的主要输入。

**推荐的下一步行动：**

- 创建核心机制原型
- 继续进行GDD工作流：`workflow gdd`
- 与目标玩家验证假设</ask>

<template-output>final_brief</template-output>
</step>

</workflow>