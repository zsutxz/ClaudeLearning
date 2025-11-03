# 开发故事工作流配置

这是一个关于自动化软件开发流程的配置文件，它定义了一个完整的故事开发工作流，让开发团队能够系统性地实现用户故事。

```yaml
name: dev-story
description: "通过实现任务/子任务、编写测试、验证和根据验收标准更新故事文件来执行故事"
author: "BMad"

# 来自配置的关键变量
config_source: "{project-root}/bmad/bmm/config.yaml"
output_folder: "{config_source}:output_folder"
user_name: "{config_source}:user_name"
communication_language: "{config_source}:communication_language"
date: 系统生成

# 工作流组件
installed_path: "{project-root}/bmad/bmm/workflows/dev-story"
instructions: "{installed_path}/instructions.md"
validation: "{installed_path}/checklist.md"

# 这是一个操作工作流（没有输出模板文档）
template: false

# 变量（可由调用者提供）
variables:
  story_path: "" # 故事文件路径
  run_tests_command: "auto" # 'auto' = 从仓库推断，或用显式命令覆盖
  strict: true # 如果为true，在验证失败时停止
  story_dir: "{config_source}:dev_story_location" # 包含故事markdown文件的目录
  story_selection_limit: 10 # 故事选择限制数量
  run_until_complete: true # 继续执行所有任务，除非遇到停止条件才暂停
  force_yolo: true # 提示执行器激活#yolo：跳过可选提示和引导

# 推荐输入
recommended_inputs:
  - story_markdown: "故事markdown文件的路径（包含任务/子任务、验收标准）"

# 所需工具（概念性；执行器应提供等效功能）
required_tools:
  - read_file # 文件读取工具
  - write_file # 文件写入工具
  - search_repo # 仓库搜索工具
  - run_tests # 测试运行工具
  - list_files # 文件列表工具
  - file_info # 文件信息工具

tags:
  - development # 开发
  - story-execution # 故事执行
  - tests # 测试
  - validation # 验证
  - bmad-v6 # BMad第六版

execution_hints:
  interactive: false # 最小化提示；旨在运行到完成
  autonomous: true # 除非被阻止，否则无需用户输入继续进行
  iterative: true # 迭代执行
```

## 工作流的故事化解读

想象一下，这个配置文件就像是软件开发团队的"智能助手说明书"。它告诉计算机如何像一个经验丰富的开发者一样，有条不紊地完成用户故事的开发工作。

### 核心功能解析

**自动发现和选择故事**：就像一个聪明的图书管理员，这个工作流能够自动找到最新的故事文件，并像展示菜单一样让你选择要开发哪个功能。

**7步标准化流程**：
1. **加载故事** - 就像厨师查看菜谱，了解要做什么菜
2. **规划实现** - 就像建筑师画蓝图，规划如何构建
3. **编写测试** - 就像质检员制定检测标准
4. **运行验证** - 就像产品出厂前的最终检查
5. **更新进度** - 就像项目经理更新任务状态
6. **完成确认** - 就像最终验收，确保一切就绪
7. **交接准备** - 就像打包完成的产品，准备交付

### 技术特色

**自动化程度高**：设置了 `interactive: false` 和 `autonomous: true`，意味着这个工作流可以像个不知疲倦的开发者一样，持续工作直到任务完成。

**智能配置**：通过 `run_until_complete: true`，它会像有责任心的员工一样，确保所有任务都完成后才下班。

**质量控制**：`strict: true` 设置确保任何测试失败都会立即停止，就像严格的质量检查员，绝不放过任何问题。

这个工作流配置展现了现代软件开发中"自动化、标准化、质量第一"的理念，让复杂的项目管理变得像执行程序一样可靠和高效。