---
name: everyday-news
description: |
  每日新闻、财经早报、政治新闻、世界杯新闻、科技 AI 新闻、GitHub 热门仓库、
  Claude Code 技巧与 Codex 新功能。当用户说“每日新闻 / 新闻早报 / 财经新闻 / 政治新闻 / 
  世界杯新闻 / 科技 AI 新闻 / Claude Code 技巧 / Codex 新功能”时自动激活。
version: 2.7.0
author: Hermes
allowed-tools:
  - Bash
  - Read
  - Write
  - WebSearch
  - WebFetch
metadata:
  hermes:
    tags: [news, daily, finance, politics, sports, ai, technology, rss, github, claude-code, codex]
prerequisites:
  commands: [curl, python3]
---

# 📰 Everyday News — 每日新闻

每天早上 8:40 自动生成日报。每个栏目保留 **5 条**，中文输出、去重（对比前 3 天）、不显示网址。

## 触发时机

- 用户提到“每日新闻 / 新闻早报 / 新闻快报”
- 用户提到财经、政治、世界杯、科技 AI 的最新动态
- 用户提到 Claude Code 技巧、Codex 新功能、最新 release notes

## 执行流程

### 1. 运行抓取脚本

```bash
python3 "${CLAUDE_PLUGIN_ROOT}/skills/everyday-news/scripts/fetch_news.py"
```

> 在当前工作目录直接运行（**不 cd**），脚本把原始英文数据写入当前目录的 `docs/everyday-news/`（`docs/everyday-news/YYYY-MM-DD.json` + `docs/everyday-news/YYYY-MM-DD.md`），后续再做翻译、去重和排版。所有最终产物也统一写入 `docs/everyday-news/`。

### 2. 读取结果

- 优先读取 `docs/everyday-news/YYYY-MM-DD.json`
- 如需查看原始标题或历史对比，再读同目录下的 Markdown

### 3. 搜索 Claude Code & Codex 技巧

- 使用 `WebSearch` 搜索官方文档、release notes、已验证的技术博客
- 必要时使用 `WebFetch` 打开来源核验标题、日期和正文
- 只保留可验证、可复述的技巧；来源不清楚就丢弃

### 4. 验证 GitHub 仓库数据

- 用 `Bash` + `curl` 先下载 GitHub API 到临时文件
- 再用 `grep` 或 `Read` 检查仓库名、stars、语言和描述
- 发现不存在、数据对不上或内容可疑时直接重试搜索

### 5. 翻译与保存

- 统一翻译成简体中文，不显示任何 URL
- 按栏目输出：财经 / 政治 / 世界杯 / 科技 AI / Claude Code & Codex
- 最终日报写入 `docs/everyday-news/YYYY-MM-DD.md`

## 核心规则

- 中文标题优先，避免英文原文混排
- 同一栏目保持来源多样化，不要被单一来源填满
- 去重时对比前 3 天，完全重复或近似重复的新闻跳过
- 世界杯栏目只保留真正相关的新闻
- GitHub 栏目只保留 stars 高且描述清晰的仓库
- Claude Code & Codex 技巧必须有可验证来源

## 输出模板

```markdown
📰 每日新闻 ｜ YYYY-MM-DD

💰 财经
1. 中文标题 — CNBC
2. 中文标题 — MarketWatch

🏛️ 政治
1. 中文标题 — NPR
2. 中文标题 — CNN

⚽ 世界杯
1. 中文标题 — ESPN

💻 科技/AI
1. 中文标题 — TechCrunch
2. 中文标题 — Ars Technica
3. repo 名称 — ⭐ stars — 语言

🔧 Claude Code & Codex
- **技巧名** — 一句话说明（来源）
```

## 参考文件

- `references/round-robin-merge.md` — 跨源轮询合并算法说明
- `references/cron-delivery-setup.md` — 日报生成与保存流程
- `references/cron-mode-verification.md` — 数据核验与安全工具使用
- `references/github-api-from-wsl.md` — GitHub API 调用说明
- `references/web-search-workaround.md` — 搜索来源与核验策略
- `references/claude-code-tip-sources.md` — 已验证的 Claude Code/Codex 技巧来源
