# 开发故事

## 目的

端到端执行单个用户故事：选择下一个未完成的任务，按照存储库标准实现它，编写测试，运行验证，并更新故事文件 — 全部在v6操作工作流中完成。

## 主要特性

- 从配置 `dev_story_location` 自动发现最近的故事
- 呈现可选择的最新故事列表
- 逐个任务迭代，直到故事完成
- 强制执行验收标准和测试覆盖率
- 限制对故事文件批准部分的编辑

## 如何调用

- 通过工作流名称（如果你的运行器支持）：
  - `workflow dev-story`
- 通过路径：
  - `workflow {project-root}/bmad/bmm/workflows/4-implementation/dev-story/workflow.yaml`

## 输入和变量

- `story_path`（可选）：故事markdown文件的显式路径。如果省略，工作流将自动发现故事。
- `run_tests_command`（可选，默认：`auto`）：用于运行测试的命令。当为`auto`时，运行器应该推断（例如，`npm test`、`pnpm test`、`yarn test`、`pytest`、`go test`等）。
- `strict`（默认：`true`）：如果为`true`，在验证或测试失败时停止。
- `story_dir`（来自配置）：从`{project-root}/bmad/bmm/config.yaml`键`dev_story_location`解析。
- `story_selection_limit`（默认：`10`）：选择时显示的最近故事数量。

## 配置

确保你的BMM配置定义了故事目录：

```yaml
# bmad/bmm/config.yaml
output_folder: ./outputs
user_name: Your Name
communication_language: en
# 故事markdown文件所在的目录
dev_story_location: ./docs/stories
```

## 工作流概要

1. 加载故事并选择下一个任务
   - 如果提供了`story_path`则使用；否则从`dev_story_location`列出最近的故事
   - 解析故事、验收标准、任务/子任务、开发注释、状态
   - 选择第一个未完成的任务
2. 规划和实现
   - 在开发代理记录 → 调试日志中记录简要计划
   - 实现任务和子任务，处理边界情况
3. 编写测试
   - 添加单元、集成和E2E测试（如适用）
4. 运行验证和测试
   - 运行现有测试进行回归 + 新测试
   - 如果配置了，进行代码检查/质量检查；确保满足验收标准
5. 标记任务完成并更新故事
   - 在任务上勾选[x]，更新文件列表，添加完成注释和更改日志
   - 如果还有任务，从步骤1重复
6. 完成序列
   - 验证所有任务完成，运行完整回归测试套件，更新状态 → "准备好审查"
7. 验证和交接（可选）
   - 可选地运行验证并完成注释

## 允许的故事文件修改

此工作流只能更改这些部分：

- 任务/子任务复选框
- 开发代理记录（调试日志、完成注释）
- 文件列表
- 更改日志
- 状态

## 此工作流中的文件

- `workflow.yaml` — 配置和变量
- `instructions.md` — 执行逻辑和步骤
- `checklist.md` — 完成的验证检查清单

## 相关工作流

- `story-context` — 为单个故事构建开发上下文
- `story-context-batch` — 处理多个故事并更新状态