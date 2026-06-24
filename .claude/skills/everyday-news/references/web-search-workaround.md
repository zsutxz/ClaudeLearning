# Web Search for Cron Jobs

当前的 Claude Code 会话不一定提供 `WebSearch` 工具。若需要搜索网页，优先使用 skill 已声明的工具；如果当前上下文里没有可用的搜索工具，再走技能内约定的替代方案。

## 推荐方式：直接使用 `WebSearch`

```text
使用 WebSearch 搜索“query keywords”，返回 3-5 条结果
```

## 不推荐的方式

- 依赖不存在的 `web_search` 工具
- 直接把未经核验的网页内容当结果使用

## 原因

在没有原生搜索工具的场景下，搜索应当通过当前会话允许的工具链完成；结果必须经过核验，避免把推断内容写入日报。