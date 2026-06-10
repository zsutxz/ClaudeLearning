# Claude Code 新功能汇总

*搜索时间：2026-06-10*
*来源：[What's new](https://code.claude.com/docs/en/whats-new) | [Release Notes](https://docs.anthropic.com/en/release-notes/claude-code)*

---

## Week 22（v2.1.150–v2.1.157，2026-05-25~29）

- **Claude Opus 4.8** 成为新默认模型（Max/Team Premium/Enterprise/API），high effort 默认开启，`/effort xhigh` 用于最难题目
- **Dynamic Workflows** — 从脚本编排数十到数百个子代理
- **Security-guidance 插件** — 实时审查代码变更的安全漏洞
- **Fast mode** 运行在 Opus 4.8，$10/$50 per MTok

## Week 21（v2.1.143–v2.1.149，2026-05-18~22）

- **Auto mode 支持 Pro 计划** — 后台安全检查替代权限提示，支持 Sonnet 4.6
- **`/usage`** — 按技能/子代理/插件/MCP 细分用量
- **`/code-review`** — 新内置命令，报告正确性 bug
- **Background sessions** 出现在 `/resume` 中

## Week 20（v2.1.139–v2.1.142，2026-05-11~15）

- **Agent view** — `claude agents` 一屏查看所有会话状态
- **`/goal`** — 跨轮次持续工作直到完成条件满足
- **Rewind 菜单** — 支持"压缩到此为止"的上下文摘要
- **Fast mode** 默认运行 Opus 4.7

## Week 19（v2.1.128–v2.1.136，2026-05-04~08）

- **插件从 .zip 和 URL 加载** — `--plugin-dir` 支持 zip，`--plugin-url` 远程加载
- **`worktree.baseRef`** — 选择新 worktree 从远程默认分支还是本地 HEAD 分支
- **Auto mode hard deny 规则** — 无条件阻止特定操作
- **Hooks 可感知 effort level** — 通过 `effort.level` 和 `$CLAUDE_EFFORT`

## Week 18（v2.1.120–v2.1.126，2026-04-27~05-01）

- **Windows 不再需要 Git Bash** — 可直接使用 PowerShell
- **`claude ultrareview`** — 云端代码审查用于 CI 和脚本
- **`claude project purge`** — 清理项目本地状态
- **PR URL 粘贴到 `/resume`** 可找到创建该 PR 的会话

## Week 17（v2.1.114–v2.1.119，2026-04-20~24）

- **`/ultrareview`** 公开预览 — 云端 bug-hunting 代理舰队
- **Session recap** — 终端未聚焦时显示发生了什么
- **Custom themes** — 从 `/theme` 或插件构建配色方案
- **Claude Code Web** 重新设计，新会话侧边栏和拖拽布局

## Week 16（v2.1.105–v2.1.113，2026-04-13~17）

- **Claude Opus 4.7** 发布，新 `xhigh` effort 等级
- **Routines** — Web 版定时触发云端代理（定时/GitHub 事件/API 调用）
- **Mobile push notifications** — 长任务完成或需要你时推送手机
- **CLI 迁移到原生二进制**

## Week 15（v2.1.92–v2.1.101，2026-04-06~10）

- **Ultraplan** 早期预览 — CLI 起草计划 → Web 编辑 → 远程/本地执行
- **Monitor 工具** — 实时流式后台事件到对话中
- **`/loop` 自适应节奏** — 省略间隔时自动调整
- **`/team-onboarding`** — 打包配置为可回放指南
- **`/autofix-pr`** — 从终端开启 PR 自动修复

## Week 14（v2.1.86–v2.1.91，2026-03-30~04-03）

- **Computer use CLI 研究预览** — Claude 可操作原生应用和 GUI
- **`/powerup`** 交互式课程
- **MCP 结果大小覆盖** — 单工具最高 500K
- **插件可执行文件** 在 Bash 工具的 PATH 上

## Week 13（v2.1.83–v2.1.85，2026-03-23~27）

- **Auto mode 研究预览** — 分类器自动处理权限提示
- **Computer use Desktop** 应用中支持
- **PR auto-fix on Web**
- **Transcript search** — 用 `/` 搜索会话记录
- **Windows 原生 PowerShell 工具**
- **条件 `if` hooks**
