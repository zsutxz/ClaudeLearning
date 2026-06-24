# Cron Mode: Data Verification & Safe Tool Usage

Cron 模式下没有用户在场审批安全扫描，因此可执行的操作范围更窄，某些带解释器的管道会被拦截。

## Cron 模式中的受限操作

| 模式 | 是否受限 | 原因 |
|------|----------|------|
| `curl | python3` | ❌ | 下载内容被直接执行 |
| `python3 -c "..."` | ❌ | 直接执行脚本片段 |
| 通过子进程执行带管道的解释器 | ❌ | 同样属于未审查执行 |
| `curl -sf -o file URL` | ✅ | 仅下载，不执行 |
| 读取已保存文件 | ✅ | 只读 |
| `grep -o` 提取字段 | ✅ | 文本抽取，不执行 |
| `write_file(path)` | ✅ | 仅写入 |

## 安全验证模式

当需要验证外部数据（例如 GitHub 仓库信息）时，使用“三步文件化”流程：

### 1. 下载到文件
```bash
curl -sf -o /tmp/gh_verify.json 'https://api.github.com/repos/OWNER/REPO'
```

### 2. 确认文件存在
```bash
ls -la /tmp/gh_verify.json
wc -c /tmp/gh_verify.json
```

### 3. 提取字段
```bash
grep -o '"full_name":"[^"]*"' /tmp/gh_verify.json
grep -o '"stargazers_count":[[:space:]]*[0-9]*' /tmp/gh_verify.json
grep -o '"language":"[^"]*"' /tmp/gh_verify.json
grep -o '"description":"[^"]*"' /tmp/gh_verify.json
grep -o '"message":"[^"]*"' /tmp/gh_verify.json
```

## 常见坑

- `language` 可能是 `null`，这时 `grep` 会匹配不到
- API 结果的空格和换行会影响正则
- 如果 `grep` 不稳定，就直接读文件内容人工核验

## GitHub API 搜索

搜索查询应先下载到临时文件，再读取和核验，避免把结果直接交给解释器处理。