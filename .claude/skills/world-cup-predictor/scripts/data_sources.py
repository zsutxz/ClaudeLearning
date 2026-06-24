"""
外部数据源 — 从Wikipedia爬取实时球队数据
=========================================
通过PowerShell调用Wikipedia API获取:
1. 各队近期战绩 (近10场正式比赛)
2. 历史交锋记录
3. FIFA排名变动

用法: from data_sources import fetch_team_form, update_ratings_with_form
"""
import subprocess, json, re, time
from datetime import datetime, timedelta

CACHE = {}
CACHE_TTL = 3600  # 1小时缓存

def ps_fetch(url):
    """通过PowerShell从Windows侧访问网络"""
    cmd = [
        "powershell.exe", "-Command", "&{",
        "$wc = New-Object System.Net.WebClient;",
        "$wc.Headers.Add('User-Agent', 'Mozilla/5.0 (compatible; HermesBot/1.0)');",
        f"$r = $wc.DownloadString('{url}');",
        "Write-Output $r;", "}"
    ]
    try:
        result = subprocess.run(cmd, capture_output=True, timeout=20)
        if result.returncode == 0 and len(result.stdout) > 0:
            return result.stdout.decode('utf-8', errors='replace')
    except: pass
    return None

def search_wikipedia_page(team):
    """搜索球队在Wikipedia上的页面标题"""
    url = (f"https://en.wikipedia.org/w/api.php?action=query&list=search&srsearch="
           f"{team}+national+football+team&srlimit=1&format=json")
    r = ps_fetch(url)
    if r:
        data = json.loads(r)
        pages = data.get('query',{}).get('search',[])
        if pages:
            return pages[0]['title']
    return None

def parse_recent_results(html_text):
    """
    从球队页面的HTML中解析近期比赛结果。
    查找表格行中的比分和结果。
    返回: [(对手, 比分, W/D/L), ...]
    """
    results = []
    # 找比分模式: team A–team B 或 team A vs team B
    # Wikipedia比赛结果通常在表格中: | {{fb|TEAM}} || 比分 || {{fb|TEAM}}
    score_pattern = r'flagicon\|[^}]+\}\}\s*\|\|\s*(\d+)[–-](\d+)\s*\|\|'
    matches = re.findall(score_pattern, html_text)
    for g1, g2 in matches[:15]:
        results.append((int(g1), int(g2)))
    return results

def fetch_team_recent_form(team_name):
    """
    获取球队近期的比赛结果(近10场)。
    返回: (胜, 平, 负, 进球, 失球) 或 None
    """
    cache_key = f"form_{team_name}"
    if cache_key in CACHE:
        age = (datetime.now() - CACHE[cache_key]['time']).seconds
        if age < CACHE_TTL:
            return CACHE[cache_key]['data']
    
    page_title = search_wikipedia_page(team_name)
    if not page_title:
        return None
    
    # 获取页面的比赛结果章节
    url = (f"https://en.wikipedia.org/w/api.php?action=parse&page={page_title}"
           f"&prop=text&section=0&format=json")
    r = ps_fetch(url)
    if not r:
        return None
    
    try:
        data = json.loads(r)
        html = data.get('parse',{}).get('text',{}).get('*','')
    except:
        return None
    
    # 解析比分 (简单方法)
    scores = parse_recent_results(html)
    
    if not scores:
        # 尝试找 "Recent results" 或 "2025" 章节
        sections_url = (f"https://en.wikipedia.org/w/api.php?action=parse&page={page_title}"
                       f"&prop=sections&format=json")
        r2 = ps_fetch(sections_url)
        if r2:
            try:
                sections = json.loads(r2).get('parse',{}).get('sections',[])
                for sec in sections:
                    line = sec.get('line','')
                    if '2025' in line or '2026' in line or 'Recent' in line:
                        sec_url = (f"https://en.wikipedia.org/w/api.php?action=parse&page={page_title}"
                                  f"&prop=text&section={sec['index']}&format=json")
                        r3 = ps_fetch(sec_url)
                        if r3:
                            html3 = json.loads(r3).get('parse',{}).get('text',{}).get('*','')
                            scores = parse_recent_results(html3)
                        break
            except: pass
    
    if not scores:
        return None
    
    w = d = l = gf = ga = 0
    for g1, g2 in scores[-10:]:  # 最近10场
        if g1 > g2: w += 1
        elif g1 == g2: d += 1
        else: l += 1
        gf += g1
        ga += g2
    
    result = (w, d, l, gf, ga)
    CACHE[cache_key] = {'data': result, 'time': datetime.now()}
    return result

def fetch_head_to_head(team1, team2):
    """
    获取两队历史交锋记录。
    返回: (team1胜, 平, team2胜) 或 None
    """
    cache_key = f"h2h_{team1}_{team2}"
    if cache_key in CACHE:
        age = (datetime.now() - CACHE[cache_key]['time']).seconds
        if age < CACHE_TTL:
            return CACHE[cache_key]['data']
    
    search_term = f"{team1}–{team2} association football"
    url = (f"https://en.wikipedia.org/w/api.php?action=query&list=search&srsearch={search_term}"
           f"&srlimit=3&format=json")
    r = ps_fetch(url)
    if not r:
        return None
    
    try:
        data = json.loads(r)
        pages = data.get('query',{}).get('search',[])
        for page in pages:
            title = page['title']
            if team1.split()[-1] in title and team2.split()[-1] in title:
                # 找到h2h页面, 解析
                page_url = (f"https://en.wikipedia.org/w/api.php?action=parse&page={title}"
                           f"&prop=text&section=0&format=json")
                r2 = ps_fetch(page_url)
                if r2:
                    html = json.loads(r2).get('parse',{}).get('text',{}).get('*','')
                    text = re.sub(r'<[^>]+>', ' ', html)
                    # 找比分: Team1 2–1 Team2
                    scores = re.findall(r'(\d+)[–-](\d+)', text)
                    t1_w = t2_w = d = 0
                    for s1, s2 in scores[-5:]:
                        if int(s1) > int(s2): t1_w += 1
                        elif int(s1) == int(s2): d += 1
                        else: t2_w += 1
                    if t1_w + t2_w + d > 0:
                        result = (t1_w, d, t2_w)
                        CACHE[cache_key] = {'data': result, 'time': datetime.now()}
                        return result
    except: pass
    return None

def update_ratings_with_form(team_data, fifa_rank_dict):
    """
    用真实近期战绩更新评分中的"近期状态"维度。
    返回更新后的 team_data.
    """
    updated = {}
    for team, rating_tuple in team_data.items():
        rf, wh, cp, atk, defn, ada, tac, inj = rating_tuple
        
        form_data = fetch_team_recent_form(team)
        if form_data:
            w, d, l, gf, ga = form_data
            total = w + d + l
            if total > 0:
                # 胜率 → 状态分 (0-100)
                win_rate = w / total
                # 净胜球调整
                gd = gf - ga
                gd_factor = max(-20, min(20, gd / total * 5))
                # 新状态分 = 基础排名分 × 胜率修正
                new_form = min(98, max(30, int(50 + win_rate * 40 + gd_factor)))
                if abs(new_form - rf) > 5:  # 变化大于5分才更新
                    print(f"  [数据] {team}: 状态 {rf}→{new_form} (近{total}场:{w}胜{d}平{l}负, {gf}:{ga})")
                    rf = new_form
        
        updated[team] = (rf, wh, cp, atk, defn, ada, tac, inj)
    
    return updated

def fetch_fifa_ranking_trend(team):
    """
    获取FIFA排名变动趋势.
    返回: (当前排名, 变动) 或 None
    Wikipedia FIFA排名页面: https://en.wikipedia.org/wiki/FIFA_World_Rankings
    """
    # 简化: 使用ODDS_NAME_MAP类似的映射
    url = "https://en.wikipedia.org/w/api.php?action=query&titles=FIFA_World_Rankings&prop=extracts&format=json"
    r = ps_fetch(url)
    if r:
        try:
            data = json.loads(r)
            pages = data.get('query',{}).get('pages',{})
            for pid, page in pages.items():
                extract = page.get('extract','')
                # 找排名表格
                text = re.sub(r'<[^>]+>', ' ', extract)
                text = re.sub(r'\s+', ' ', text)
                # 找球队名+排名
                if team in text:
                    idx = text.index(team)
                    snippet = text[max(0,idx-50):idx+50]
                    return snippet
        except: pass
    return None

if __name__ == "__main__":
    # 测试
    import sys
    test_team = sys.argv[1] if len(sys.argv) > 1 else "France"
    print(f"正在获取 {test_team} 近期战绩...")
    form = fetch_team_recent_form(test_team)
    if form:
        w, d, l, gf, ga = form
        print(f"  近{ w+d+l}场: {w}胜 {d}平 {l}负, 进球{gf} 失球{ga}")
    else:
        print(f"  未能获取 {test_team} 的数据")
