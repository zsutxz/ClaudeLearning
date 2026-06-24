# Verified Claude Code & Codex Tip Sources

当日报流程里的搜索结果不可验证时，优先回退到这些已核验来源：

## 官方来源：Anthropic Engineering Blog

URL: `https://www.anthropic.com/engineering`

### 提取文章标题和日期

```bash
curl -sf -o /tmp/anthropic_blog.html https://www.anthropic.com/engineering
python3 -c "
import re
with open('/tmp/anthropic_blog.html') as f:
    content = f.read()
articles = re.findall(r'<h[1-3][^>]*>([^<]+)</h[1-3]><div[^>]*class=[^>]*date[^>]*>([^<]+)</div>', content)
for title, date in articles:
    print(f'{title.strip()} --- {date.strip()}')
"
```

### 备选：从页面属性里提取标题

```bash
python3 -c "
import re
with open('/tmp/anthropic_blog.html') as f:
    content = f.read()
titles = re.findall(r'alt=([^ ]+)', content)
for t in titles:
    if any(w in t.lower() for w in ['claude', 'agent', 'code', 'eval', 'harness', 'mcp']):
        print(t.strip())
"
```

## 官方来源：Claude Code GitHub Releases

当博客不可用时，直接从 GitHub Releases API 提取已验证的功能。

| Release | Date | Verified Features |
|---------|------|-------------------|
| v2.1.177 | Jun 13 | 无 release notes（hotfix，仅有 SHA 签名及二进制文件） |
| v2.1.176 | Jun 12 | 会话标题自动跟随对话语言；页脚自定义链接；凭证缓存改进；Remote Control 修复 |
| v2.1.175 | Jun 12 | 管理设置支持强制模型白名单 |
| v2.1.174 | Jun 11 | 滚动加速设置；`/model` 选择器修复；用量归因面板；背景会话环境变量修复 |
| v2.1.178 | Jun 15 | 权限规则参数匹配；嵌套 skills 目录；自动模式子代理评估；`/bug` 描述必填；多项 Remote Control 修复 |

### 提取方法

```bash
curl -sf -o /tmp/claude_releases.json 'https://api.github.com/repos/anthropics/claude-code/releases?per_page=5'
grep -oP '"tag_name":"[^"]*"' /tmp/claude_releases.json
```

## 可靠性说明

- GitHub Releases API 最可靠，优先级最高
- Anthropic Engineering Blog 偶尔会因前端渲染或网络环境返回空内容
- 外部搜索结果必须可核验，不能直接写入日报

## 已用过的技巧

下面这些技巧已经在过去几天的日报里用过了，避免重复：

1. 自愈编辑模式
2. 补丁预览边对边对比
3. 带约束的跨文件重构
4. 自定义指令配置文件
5. 自动提交信息生成
6. 多文件并行编辑
7. --plan 分步执行
8. .claudeignore 排除规则
9. /fix 与 /review 斜杠命令
10. 深度调查模式
11. Shell 命令实时预览
12. 跨文件编辑差异视图
13. /explain 命令集成调用栈
14. 自定义 Lint 规则
15. Claude Code 自动模式
16. Agent 技能扩展系统
17. 托管 Agent 架构
18. 跨产品容器化安全
19. Claude Code 质量报告更新
20. 长运行应用 Harness 设计
21. footerLinksRegexes 页脚定制
22. enforceAvailableModels 模型白名单
23. VSCode 用量归因面板
24. wheelScrollAcceleration 设置
25. 权限规则参数匹配
26. 嵌套技能目录加载
27. 自动模式子代理安全升级
28. /bug 命令增强
