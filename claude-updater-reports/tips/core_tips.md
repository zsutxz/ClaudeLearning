# Claude Code 核心技巧

*搜索时间：2026-06-10*
*来源：[Builder.io 50 Tips](https://www.builder.io/blog/claude-code-tips-best-practices) | [Reddit Creator Tips](https://www.reddit.com/r/ClaudeAI/comments/1r2m8ma/12_claude_code_tips_from_creator_of_claude_code/)*

---

## 模型与 Effort

- `/effort xhigh` — 最强推理，用于架构决策和复杂 bug
- `/effort high` — 日常编码推荐（Opus 4.8 默认）
- `/effort medium` — 简单编辑，节省 token
- `/fast` — 快速输出模式，运行在 Opus 4.8 上不降级模型

## 权限与自动化

- **Auto mode** — 用分类器替代手动权限弹窗，安全操作自动执行，危险操作直接阻止
- **Hard deny 规则** — 在 settings.json 中配置无条件阻止的操作
- `claude --allowedTools` — 预授权工具列表，减少交互

## 会话管理

- `/resume` — 恢复上次会话，支持粘贴 PR URL 定位
- `claude agents` — 一屏查看所有活跃/阻塞/完成的后台会话
- `/goal <条件>` — 跨轮次持续执行直到条件满足
- **Rewind** — 在历史记录中选择"Summarize up to here"压缩上下文

## 工作流

- `/ultrareview` — 云端多代理代码审查，不消耗本地上下文
- `/code-review` — 内置代码审查，报告正确性 bug
- `/loop [间隔] <任务>` — 定时重复执行任务，省略间隔则自适应
- **Dynamic Workflows** — 用脚本编排大规模子代理集群

## 插件与扩展

- `--plugin-dir <zip路径>` — 从 zip 文件加载插件
- `--plugin-url <url>` — 从远程 URL 加载插件
- `/theme` — 自定义主题配色
- **Hooks** — PreToolUse/PostToolUse/Stop 三种钩子，支持条件 if

## Windows 特定

- PowerShell 原生支持，不再强制 Git Bash
- `claude project purge` — 清理项目本地状态
