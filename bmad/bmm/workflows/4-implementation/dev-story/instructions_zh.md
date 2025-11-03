# 开发故事 - 工作流指令

```xml
<critical>工作流执行引擎受以下文件管理：{project_root}/bmad/core/tasks/workflow.md</critical>
<critical>你必须已经加载并处理了：{installed_path}/workflow.yaml</critical>
<critical>只允许在这些区域修改故事文件：任务/子任务复选框、开发代理记录（调试日志、完成注释）、文件列表、更改日志和状态</critical>
<critical>严格按照顺序执行所有步骤；不要跳过步骤</critical>
<critical>如果 {{run_until_complete}} == true，则以非交互方式运行：除非遇到停止条件或为未经批准的依赖项需要明确的用户批准，否则不要在步骤之间暂停。</critical>
<critical>绝对不要因为"里程碑"、"重大进展"或"会话边界"而停止。在单个执行中继续，直到故事完成（所有AC都已满足且所有任务/子任务都已选中）或触发停止条件。</critical>
<critical>不要安排"下次会话"或请求审查暂停，除非适用停止条件。只有步骤6决定完成。</critical>

<workflow>

  <step n="1" goal="加载故事并选择下一个任务">
    <action>如果明确提供了 {{story_path}} 且有效 → 使用它。否则，尝试自动发现。</action>
    <action>自动发现：从配置中读取 {{story_dir}}（dev_story_location）。如果无效/缺失或不包含 .md 文件，要求用户提供以下任一选项：(a) 故事文件路径，或 (b) 要扫描的目录。</action>
    <action>如果提供了目录，则在该目录下递归列出匹配模式"story-*.md"的故事markdown文件。</action>
    <action>按最后修改时间排序（最新的在前）并取前 {{story_selection_limit}} 项。</action>
    <ask>呈现包含索引、文件名和修改时间的列表。询问："选择一个故事（1-{{story_selection_limit}}）或输入路径："</ask>
    <action>将选定项解析为 {{story_path}}</action>
    <action>从 {{story_path}} 读取完整的故事文件</action>
    <action>解析部分：故事、验收标准、任务/子任务（包括子任务）、开发注释、开发代理记录、文件列表、更改日志、状态</action>
    <action>在任务/子任务中识别第一个未完成的任务（未选中的 [ ]）；如果存在子任务，则将所有子任务视为所选任务范围的一部分</action>
    <check>如果没有找到未完成的任务 → "所有任务已完成 - 进入完成序列"并 <goto step="6">继续</goto></check>
    <check>如果故事文件无法访问 → 停止："无法访问故事文件，无法开发故事"</check>
    <check>如果任务要求模糊 → 要求用户澄清；如果未解决，停止："在实现之前必须明确任务要求"</check>
  </step>

  <step n="2" goal="规划并实现任务">
    <action>审查所选任务的验收标准和开发注释</action>
    <action>规划实现步骤和边界情况；在开发代理记录 → 调试日志中写下简要计划</action>
    <action>完全实现任务，包括所有子任务，遵循此存储库中的架构模式和编码标准</action>
    <action>适当处理错误条件和边界情况</action>
    <check>如果需要未经批准的依赖项 → 在添加之前请求用户批准</check>
    <check>如果发生3次连续实现失败 → 停止并请求指导</check>
    <check>如果缺少必需配置 → 停止："没有必要的配置文件无法继续"</check>
    <check>如果 {{run_until_complete}} == true → 不要在部分进展后停止；继续迭代任务，直到满足所有AC或触发停止条件</check>
    <check>在满足步骤6门控之前，不要提议暂停进行审查、站会或验证</check>
  </step>

  <step n="3" goal="编写全面测试">
    <action>为任务引入/更改的业务逻辑和核心功能创建单元测试</action>
    <action>在适用的地方为组件交互添加集成测试</action>
    <action>如果适用，为关键用户流程包括端到端测试</action>
    <action>涵盖计划中注明的边界情况和错误处理场景</action>
  </step>

  <step n="4" goal="运行验证和测试">
    <action>确定如何为此存储库运行测试（推断或使用提供的 {{run_tests_command}}）</action>
    <action>运行所有现有测试以确保没有回归</action>
    <action>运行新测试以验证实现的正确性</action>
    <action>如果配置了，运行代码检查和代码质量检查</action>
    <action>验证实现满足所有故事验收标准；如果AC包括定量阈值（例如，测试通过率），确保在标记完成之前达到它们</action>
    <check>如果回归测试失败 → 在继续之前停止并修复</check>
    <check>如果新测试失败 → 在继续之前停止并修复</check>
  </step>

  <step n="5" goal="标记任务完成并更新故事">
    <action>只有当所有测试通过且验证成功时，才用 [x] 标记任务（和子任务）复选框</action>
    <action>使用任何新、修改或删除的文件更新文件列表部分（相对于存储库根目录的路径）</action>
    <action>如果进行了重大更改，向开发代理记录添加完成注释（总结意图、方法和任何后续工作）</action>
    <action>向更改日志追加描述更改的简要条目</action>
    <action>保存故事文件</action>
    <check>确定是否还有更多未完成的任务</check>
    <check>如果还有更多任务 → <goto step="1">下一个任务</goto></check>
    <check>如果没有更多任务 → <goto step="6">完成</goto></check>
  </step>

  <step n="6" goal="故事完成序列">
    <action>验证所有任务和子任务都标记为 [x]（现在重新扫描故事文档）</action>
    <action>运行完整的回归测试套件（不要跳过）</action>
    <action>确认文件列表包含每个更改的文件</action>
    <action>执行故事完成定义检查清单，如果故事包含的话</action>
    <action>将故事状态更新为：准备好审查</action>
    <check>如果任何任务未完成 → 返回步骤1完成剩余工作（不要以部分进展完成）</check>
    <check>如果存在回归失败 → 在完成之前停止并解决</check>
    <check>如果文件列表不完整 → 在完成之前更新它</check>
  </step>

  <step n="7" goal="验证和交接" optional="true">
    <action>可选地使用 {project-root}/bmad/core/tasks/validate-workflow.md 对故事运行工作流验证任务</action>
    <action>在开发代理记录 → 完成注释中准备简洁摘要</action>
    <action>传达故事已准备好审查</action>
  </step>

</workflow>
```