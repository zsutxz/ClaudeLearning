---
name: claude-updater
description: 统一管理 Claude Code 生态系统的所有组件更新，包括 CLI、插件、BMAD 框架。当用户需要更新 Claude Code 相关组件、检查更新状态或查看更新报告时使用此 skill。
license: MIT
allowed-tools: [Bash, Read, Write, WebSearch]
metadata:
  version: "2.0.0"
  category: system
  tags: update,cli,plugin,bmad,maintenance
---

# Claude Code 生态更新器

## 概述

统一管理 Claude Code 生态系统中所有组件的更新流程：

- **Claude Code CLI** — 核心命令行工具
- **官方插件** — 通过 marketplace 分发的插件
- **BMAD 框架** — `_bmad/` 目录中的业务方法框架
- **自定义 Skills** — `.claude/skills/` 目录中的本地技能

## 使用时机

当用户提出以下需求时，使用此 skill：

- "检查更新" / "检查 Claude Code 更新"
- "更新全部" / "更新所有组件"
- "更新 CLI" / "更新 Claude CLI"
- "更新插件" / "更新技能"
- "更新 BMAD" / "更新 BMAD 框架"
- "查看更新报告" / "更新状态"
- "Claude Code 有什么更新"

## 组件清单

### 已安装插件

| 插件 | Marketplace | 说明 |
|------|-------------|------|
| code-review | claude-plugins-official | 代码审查 |
| code-simplifier | claude-plugins-official | 代码简化 |
| context7 | claude-plugins-official | 上下文管理 |
| document-skills | anthropic-agent-skills | 文档生成技能集 |
| security-guidance | claude-plugins-official | 安全指导 |
| understand-anything | understand-anything | 代码库知识图谱 |

### Marketplace 源

| 名称 | GitHub 仓库 |
|------|-------------|
| claude-plugins-official | anthropics/claude-plugins-official |
| everything-claude-code | affaan-m/everything-claude-code |
| anthropic-agent-skills | anthropics/skills |
| understand-anything | Lum1104/Understand-Anything |

## 命令参考

### CLI 更新

```bash
# 检查当前版本
claude --version

# 更新 CLI（推荐）
claude update

# 备选：通过 npm 更新
npm update -g @anthropic-ai/claude-code
```

### 插件管理

```bash
# 查看已安装插件
claude plugin list

# 查看所有 marketplace
claude plugin marketplace list

# 更新所有 marketplace 源
claude plugin marketplace update

# 更新指定 marketplace 源
claude plugin marketplace update claude-plugins-official

# 更新单个插件
claude plugin update code-review@claude-plugins-official
claude plugin update document-skills@anthropic-agent-skills
claude plugin update understand-anything@understand-anything

# 安装新插件
claude plugin install <plugin>@<marketplace>

# 安装时指定作用域
claude plugin install <plugin> --scope user    # 用户级（默认）
claude plugin install <plugin> --scope project  # 项目级
claude plugin install <plugin> --scope local    # 本地级
```

### BMAD 框架更新

```bash
# 使用官方安装器（交互式，会记住上次选择）
npx bmad-method install

# 检查当前 BMAD 配置
cat _bmad/config.toml
cat _bmad/config.user.toml 2>/dev/null
```

> **注意**：BMAD 安装器是交互式的，需要用户手动选择选项。之前的答案会被记住为默认值。自定义配置放在 `_bmad/custom/` 目录下，不会被安装器覆盖。

## 执行流程

### 场景 1: 检查更新

1. **记录当前状态**
   ```bash
   claude --version                                    # CLI 版本
   claude plugin list                                  # 已安装插件及版本
   claude plugin marketplace list                      # Marketplace 源状态
   ```

2. **检查可用更新**
   ```bash
   claude plugin marketplace update                    # 刷新 marketplace 源
   claude plugin list                                  # 再次查看是否有新版本
   ```

3. **汇总结果**
   - 对比各组件当前版本与最新版本
   - 标注有可用更新的组件
   - 返回检查报告

### 场景 2: 更新全部

1. **备份当前状态** — 记录所有组件当前版本号
2. **更新 CLI** — `claude update`
3. **更新 Marketplace 源** — `claude plugin marketplace update`
4. **逐个更新插件** — 对每个已安装插件执行 `claude plugin update <plugin>`
5. **更新 BMAD** — 提示用户运行 `npx bmad-method install`（交互式）
6. **生成更新报告** — 保存到 `claude-updater-reports/` 目录

### 场景 3: 选择性更新

根据用户指定的组件，只执行对应的更新步骤。

## 更新报告

### 报告存储

```
项目根目录/
├── claude-updater-reports/
│   ├── index.json             # 报告索引
│   ├── latest.md              # 最新报告
│   └── history/
│       └── YYYY-MM-DD_HHMMSS.md
```

### 报告模板

```markdown
# Claude Code 生态更新报告

*生成时间：{datetime}*
*系统：{os_info}*

---

## 更新摘要

| 组件 | 更新前 | 更新后 | 状态 |
|------|--------|--------|------|
| Claude CLI | {old_version} | {new_version} | ✅/⏭️/❌ |
| code-review | {old} | {new} | ✅/⏭️/❌ |
| document-skills | {old} | {new} | ✅/⏭️/❌ |
| understand-anything | {old} | {new} | ✅/⏭️/❌ |
| BMAD 框架 | {old_commit} | {new_commit} | ✅/⏭️/❌ |

**状态说明**：✅ 成功 | ⏭️ 无更新 | ❌ 失败 | ⚠️ 警告

---

## 详细日志

{各组件的更新输出}

---

## 注意事项

{warnings}
```

### 索引文件 (index.json)

```json
{
  "last_check": "2026-06-08T10:30:00",
  "last_update": "2026-06-08T10:35:00",
  "components": {
    "claude-cli": {
      "current_version": "2.1.168",
      "last_checked": "2026-06-08T10:30:00"
    },
    "plugins": {
      "code-review": { "version": "ed3ff7abb352" },
      "document-skills": { "version": "c30d329f5814" },
      "understand-anything": { "version": "2.7.6" }
    },
    "bmad": {
      "last_checked": "2026-06-08T10:30:00"
    }
  },
  "history": []
}
```

## 注意事项

### BMAD 更新
- 使用 `npx bmad-method install` 官方安装器
- **交互式命令**：需要用户手动选择安装选项
- 安装器会记住之前的答案作为默认值
- 自定义配置在 `_bmad/custom/` 下，不会被覆盖

### 插件更新
- 需要网络连接
- 中国大陆用户可能需要代理
- 更新后需重启 Claude Code 才能生效
- 使用 `--scope` 指定安装范围

### CLI 更新
- Windows 可能需要管理员权限
- 优先使用 `claude update` 而非 npm
- 更新后需重启 Claude Code

### 错误处理
- 单个组件更新失败不影响其他组件
- 记录所有错误信息到报告
- 提供失败组件的手动更新命令

## 错误排查

| 错误 | 原因 | 解决方案 |
|------|------|----------|
| 网络超时 | 网络不通 | 检查网络/代理设置 |
| 权限不足 | Windows 权限 | 以管理员身份运行 |
| npx 失败 | Node.js 问题 | 运行 `node --version` 检查 |
| 插件冲突 | 版本不兼容 | 尝试 `claude plugin update <name>` 单独更新 |
