# 游戏头脑风暴工作流程配置
name: "brainstorm-game"
description: "通过协调CIS头脑风暴工作流程与游戏特定上下文、指导和额外的游戏设计技巧来促进游戏头脑风暴会议。"
author: "BMad"

# 关键变量从config_source加载
config_source: "{project-root}/bmad/bmm/config.yaml"
output_folder: "{config_source}:output_folder"
user_name: "{config_source}:user_name"
date: 系统生成

# 模块路径和组件文件
installed_path: "{project-root}/bmad/bmm/workflows/1-analysis/brainstorm-game"
template: false
instructions: "{installed_path}/instructions_zh.md"

# 游戏头脑风暴的上下文和技巧
game_context: "{installed_path}/game-context_zh.md"
game_brain_methods: "{installed_path}/game-brain-methods.csv"

# 要调用的CIS头脑风暴工作流程
cis_brainstorming: "{project-root}/bmad/cis/workflows/brainstorming/workflow.yaml"