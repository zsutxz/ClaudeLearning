# 网络/代理故障排查记录

> 此文件记录了在2026年5月12日会话中排查外网访问问题时的方法和结论，
> 供未来会话参考，避免重复排查。

## 环境

- WSL2 (Windows Subsystem for Linux)
- Windows主机IP: 172.31.64.1
- 代理: 172.31.64.1:51081 (HTTP代理)
- `no_proxy`: localhost,127.0.0.1,::1,api.deepseek.com
- 模型API: DeepSeek (api.deepseek.com，通过直连，不经过代理)

## 测试过的方案及结果

| 方案 | 命令/方法 | 结果 |
|------|-----------|------|
| Wikipedia API (HTTPS, 绕过代理) | `curl --noproxy '*' ...` | 超时(timeout) - WSL无直连外网 |
| Wikipedia API (通过代理HTTP) | `curl http://en.wikipedia.org/...` | 空回复(Empty reply) |
| Wikipedia API (通过代理HTTPS/ConNECT) | `curl -x http://proxy CONNECT` | Proxy CONNECT aborted |
| DuckDuckGo API (HTTP) | `curl http://api.duckduckgo.com/...` | 空回复 |
| DuckDuckGo API (HTTPS, 绕过代理) | `curl --noproxy '*' https://...` | 网络不可达 |
| DuckDuckGo Lite | `curl http://lite.duckduckgo.com/...` | 超时 |
| Python urllib + ProxyHandler | `opener.open(url)` | Connection reset by peer |
| Python raw socket + proxy | `s.sendall(b"GET ...")` | 空回复 |
| netcat to proxy | `echo "GET ..." \| nc proxy 51081` | 空回复 |
| 通用HTTP (httpbin.org) | 通过代理 | 空回复 |
| delegate_task + web | 子智能体 web_search | 返回幻数据(不可靠) |

## 结论

**当前环境无外网访问能力。** 代理只放行DeepSeek API流量，其他一切外网请求被阻止。

## 可行替代方案（按优先级）

### 1. ✅ PowerShell从WSL调用Windows网络（推荐，2026-05-13发现）
```bash
powershell.exe -Command '
$wc = New-Object System.Net.WebClient
$wc.Headers.Add("User-Agent", "Mozilla/5.0")
$result = $wc.DownloadString("https://en.wikipedia.org/api/rest_v1/page/summary/2026_FIFA_World_Cup")
Write-Output $result
' 2>/dev/null | python3 -c "import sys, json; print(json.loads(sys.stdin.read())['extract'][:200])"
```
- 不需要任何配置，只要Windows有网
- **数据直接返回，无子智能体幻觉**
- `2>/dev/null` 过滤PowerShell进度输出
- 通过管道传给Python解析

### 2. 用户直接提供数据 — 最可靠

### 3. delegate_task + "web" toolset — 子智能体可以调用web_search
   - ⚠️ 子智能体经常产生幻觉（编造分组、球队等）
   - ⚠️ 每个子智能体通常只做1-2次搜索就停止
   - ✅ 但仍可获得部分真实数据（如FIFA排名表）
   - 建议：交叉验证、拆分成单次查询、限定搜索词

### 4. local files — 如果用户有保存离线数据文件（HTML/JSON/CSV），可以用read_file读取

## 对世界杯预测的影响

- 分组数据必须由用户提供或从子智能体数据中严格交叉验证
- 如果子智能体返回了可疑球队（如"朝鲜"、"津巴布韦"等极少参赛的队伍），数据不可信
- FIFA排名类结构化数据相对可靠，分组名单类自由文本数据不可靠
