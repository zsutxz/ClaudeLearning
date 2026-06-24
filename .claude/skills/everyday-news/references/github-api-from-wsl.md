# GitHub API from WSL via PowerShell WebClient

A reusable technique for querying GitHub API from WSL, bypassing WSL's restrictive proxy.

## Why

WSL's proxy (172.31.64.1:51081) blocks most external HTTP(S) from curl/Python urllib. PowerShell's native `System.Net.WebClient` uses Windows' own networking stack and has full internet access.

## Pattern

```python
import subprocess, urllib.request, json

def fetch_powershell(url):
    escaped = url.replace("'", "''")
    ps = f"""
$wc = New-Object System.Net.WebClient;
$wc.Encoding = [System.Text.Encoding]::UTF8;
$wc.Headers.Add("User-Agent","Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
try {{$b=$wc.DownloadData('{escaped}');Write-Output ([System.Text.Encoding]::UTF8.GetString($b))}}
catch {{Write-Output "__FAIL__"}}"""
    try:
        r = subprocess.run(["powershell.exe","-NoProfile","-Command",ps],
            capture_output=True, text=True, timeout=20, encoding='utf-8', errors='replace')
        return None if r.stdout.strip()=="__FAIL__" else r.stdout.strip()
    except:
        return None

def search_github(query, per_page=5):
    """Search GitHub repos, sorted by stars."""
    encoded = urllib.request.quote(query)
    url = f"https://api.github.com/search/repositories?q={encoded}&sort=stars&order=desc&per_page={per_page}"
    data = fetch_powershell(url)
    if not data:
        return []
    try:
        return json.loads(data).get("items", [])
    except (json.JSONDecodeError, KeyError):
        return []
```

## Usage Examples

Search trending AI coding tools:
```python
repos = search_github("claude-code", 5)
for r in repos:
    print(f"{r['full_name']} — ⭐ {r['stargazers_count']} — {r.get('language', 'N/A')}")
    print(f"  {r.get('description', '')}")
    print(f"  {r['html_url']}")
```

## Rate Limits

- **Unauthenticated**: 60 requests/hour (per IP) — enough for daily cron (3-6 req/run)
- **Authenticated** (add `Authorization: token YOUR_TOKEN` header): 5000 requests/hour

To add auth header in the PowerShell script:
```powershell
$wc.Headers.Add("Authorization", "token ghp_xxxxxxxxxxxx");
```

## Pitfalls

- URL quoting: use `urllib.request.quote(query)` for search terms with spaces/spcial chars
- Timeout: GitHub API responds fast (<2s), but set timeout=20 for safety
- Decoding: `utf-8, errors='replace'` handles encoding issues from PowerShell pipe
- GitHub API returns `items` array; check both `json.loads()` success and `items` existence
