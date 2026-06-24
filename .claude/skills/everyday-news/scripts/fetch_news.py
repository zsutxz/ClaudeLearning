#!/usr/bin/env python3
"""Fetch daily news via PowerShell, save structured data.
Chinese translation done by agent on delivery.

Usage:
    python3 ~/.hermes/skills/everyday-news/scripts/fetch_news.py

Output:
    - ./docs/everyday-news/YYYY-MM-DD.md  (English raw)
    - ./docs/everyday-news/YYYY-MM-DD.json (structured data)
"""
import subprocess, html, re, datetime, os, json, urllib.request, sys

# Windows 控制台默认 GBK，print 含 emoji 的汇总会抛 UnicodeEncodeError，强制 utf-8 输出
try:
    sys.stdout.reconfigure(encoding="utf-8")
except Exception:
    pass

# 所有产物统一写入运行时当前目录的 docs/everyday-news/（SKILL.md 以绝对路径调用本脚本、不 cd，保证 getcwd 为用户目录）
NEWS_DIR = os.path.join(os.getcwd(), "docs", "everyday-news")
os.makedirs(NEWS_DIR, exist_ok=True)

SOURCES = {
    "💰 财经": [
        ("CNBC", "https://search.cnbc.com/rs/search/combinedcms/view.xml?partnerId=wrss01&id=100003114"),
        ("MarketWatch", "https://feeds.marketwatch.com/marketwatch/topstories"),
    ],
    "🏛️ 政治": [
        ("NPR Politics", "https://feeds.npr.org/1014/rss.xml"),
        ("CNN Politics", "http://rss.cnn.com/rss/cnn_topstories.rss"),
    ],
    "⚽ 世界杯": [
        ("ESPN", "https://www.espn.com/espn/rss/news"),
        ("Sky Sports", "https://www.skysports.com/rss/12040"),
    ],
    "💻 科技/AI": [
        ("TechCrunch AI", "https://techcrunch.com/category/artificial-intelligence/feed/"),
        ("Ars Technica AI", "https://arstechnica.com/tag/ai/feed/"),
        ("The Verge", "https://www.theverge.com/rss/index.xml"),
    ],
}

# GitHub 热门：AI 编程工具（合并到 科技/AI，取 Top 3）
GITHUB_SEARCH_QUERIES = [
    ("claude-code", "GitHub"),
    ("codex-cli", "GitHub"),
    ("ai-coding-agent", "GitHub"),
]

# 世界杯关键词
WORLD_CUP_KEYWORDS = [
    "world cup", "worldcup", "fifa", "2026 world cup", "wc 2026", "qualif",
    "梅西", "姆巴佩", "巴西", "阿根廷", "法国", "世界杯", "淘汰赛",
    "messi", "mbappe", "ney", "ronaldo", "international break",
    "friendly", "copa", "concacaf", "conmebol", "uefa nations",
    "soccer", "football",
]


def fetch_powershell(url):
    """Fetch data via PowerShell WebClient (bypasses WSL network restrictions)."""
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


def fetch_github_trending(query, label, per_page=5):
    """Search GitHub for trending repos via PowerShell."""
    encoded = urllib.parse.quote(query)
    url = f"https://api.github.com/search/repositories?q={encoded}&sort=stars&order=desc&per_page={per_page}"
    data = fetch_powershell(url)
    if not data:
        return []
    try:
        parsed = json.loads(data)
        items = parsed.get("items", [])
        results = []
        for item in items:
            results.append({
                "name": item["full_name"],
                "url": item["html_url"],
                "stars": item["stargazers_count"],
                "lang": item.get("language") or "",
                "desc": item.get("description") or "",
                "query_label": label,
            })
        return results
    except (json.JSONDecodeError, KeyError):
        return []


def parse_feed(xml, max_items=10):
    """Extract (title, link) pairs from RSS 2.0 or Atom XML."""
    if not xml:
        return []
    items = re.findall(r'<item>(.*?)</item>', xml, re.DOTALL)
    results = []
    if items:
        for item in items:
            title_m = re.search(r'<title(?:[^>]*)>(?:<!\[CDATA\[)?\s*(.*?)\s*(?:\]\]>)?\s*</title>', item, re.DOTALL)
            link_m = re.search(r'<link>(?:<!\[CDATA\[)?\s*(.*?)\s*(?:\]\]>)?\s*</link>', item, re.DOTALL)
            if not link_m:
                link_m = re.search(r'<guid[^>]*>(?:<!\[CDATA\[)?\s*(.*?)\s*(?:\]\]>)?\s*</guid>', item, re.DOTALL)
            if title_m:
                title = html.unescape(title_m.group(1).strip().replace("\n"," ").replace("\r",""))
                while "  " in title: title = title.replace("  ", " ")
                link = html.unescape(link_m.group(1).strip()) if link_m else ""
                if title and len(title) > 5:
                    results.append((title, link))
            if len(results) >= max_items:
                break
        return results
    # Atom
    entries = re.findall(r'<entry>(.*?)</entry>', xml, re.DOTALL)
    if entries:
        for entry in entries:
            title_m = re.search(r'<title(?:[^>]*)>(?:<!\[CDATA\[)?\s*(.*?)\s*(?:\]\]>)?\s*</title>', entry, re.DOTALL)
            link_m = re.search(r'<link[^>]*href=["\']([^"\']+)["\']', entry)
            if title_m:
                title = html.unescape(title_m.group(1).strip().replace("\n"," ").replace("\r",""))
                while "  " in title: title = title.replace("  ", " ")
                link = html.unescape(link_m.group(1)) if link_m else ""
                if title and len(title) > 5:
                    results.append((title, link))
            if len(results) >= max_items:
                break
        return results
    return []


def is_world_cup_related(title):
    t = title.lower()
    return any(kw.lower() in t for kw in WORLD_CUP_KEYWORDS)


def merge_and_limit(entries_list, limit=5):
    """Round-robin merge: 1 from each source, cycle until limit reached.
    This ensures source diversity within each category."""
    seen = set()
    merged = []
    sources = []
    for name, items in entries_list:
        sources.append((name, list(items)))
    while len(merged) < limit:
        any_added = False
        for i, (name, items) in enumerate(sources):
            if len(merged) >= limit:
                break
            while items:
                title, link = items.pop(0)
                key = title[:40].lower()
                if key not in seen:
                    seen.add(key)
                    merged.append((title, link, name))
                    any_added = True
                    break
            if len(merged) >= limit:
                break
        if not any_added:
            break
    return merged[:limit]


def main():
    today = datetime.date.today().strftime("%Y-%m-%d")
    now = datetime.datetime.now().strftime("%H:%M")

    results = {}
    meta = {"ok": 0, "fail": 0}

    # --- RSS 新闻（轮询分配，每栏目5条） ---
    for cat, feeds in SOURCES.items():
        all_entries = []
        for name, url in feeds:
            xml = fetch_powershell(url)
            items = parse_feed(xml, 10)
            if items:
                meta["ok"] += 1
            else:
                meta["fail"] += 1
            all_entries.append((name, items))
        if cat == "⚽ 世界杯":
            filtered = []
            for name, items in all_entries:
                wc_items = [(t, l) for t, l in items if is_world_cup_related(t)]
                filtered.append((name, wc_items))
            merged = merge_and_limit(filtered, 5)
        else:
            merged = merge_and_limit(all_entries, 5)
        results[cat] = merged

    # --- GitHub 热门（合并到 科技/AI，2条RSS + 3条GitHub = 5条） ---
    github_seen = set()
    github_repos = []
    for query, label in GITHUB_SEARCH_QUERIES:
        repos = fetch_github_trending(query, label)
        for repo in repos:
            key = repo["name"].lower()
            if key not in github_seen:
                github_seen.add(key)
                github_repos.append(repo)
    github_repos.sort(key=lambda r: r["stars"], reverse=True)
    existing_tech = results.get("💻 科技/AI", [])
    tech_combined = list(existing_tech[:2])
    for r in github_repos[:3]:
        tech_combined.append({
            "title": r["name"],
            "link": r["url"],
            "source": "GitHub",
            "stars": r["stars"],
            "lang": r["lang"],
            "desc": r["desc"],
            "is_github": True,
        })
    results["💻 科技/AI"] = tech_combined

    # --- 保存 .md（英文原始数据，供代理翻译） ---
    lines = [f"📰 每日新闻 ｜ {today}", ""]
    for cat, entries in results.items():
        lines.append(f"─── {cat} ───\n")
        if entries:
            for i, item in enumerate(entries, 1):
                if isinstance(item, dict):
                    if item.get("is_github"):
                        lang_part = f" — {item['lang']}" if item['lang'] else ""
                        lines.append(f"  {i}. {item['title']} — ⭐ {item['stars']}{lang_part}")
                        lines.append(f"     {item['desc']}")
                    else:
                        lines.append(f"  {i}. [{item.get('source', '')}] {item.get('title', '')}")
                else:
                    title, link, src = item
                    lines.append(f"  {i}. [{src}] {title}")
        else:
            lines.append(f"  (暂无数据)\n")
        lines.append("")

    lines.append(f"📌 来源: {meta['ok']}成功 / {meta['fail']}失败 | {now} 自动抓取")

    md_path = os.path.join(NEWS_DIR, f"{today}.md")
    with open(md_path, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))

    # --- 保存 JSON（统一 dict 格式，方便 agent 翻译） ---
    json_path = os.path.join(NEWS_DIR, f"{today}.json")
    # Normalize all items to dict format for agent
    json_results = {}
    for cat, entries in results.items():
        json_list = []
        for item in entries:
            if isinstance(item, dict):
                json_list.append(item)
            else:
                title, link, source = item
                json_list.append({"title": title, "link": link, "source": source})
        json_results[cat] = json_list
    with open(json_path, "w", encoding="utf-8") as f:
        json.dump({"date": today, "results": json_results, "meta": meta}, f, ensure_ascii=False, indent=2)

    print("\n".join(lines))
    print(f"\n✅ 已保存: {md_path}")
    print(f"📋 JSON: {json_path}")


if __name__ == "__main__":
    main()
