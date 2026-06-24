#!/usr/bin/env python3
"""
2026 World Cup Prediction v4.9 - 双年优化 + 防作弊
========================================================
使用方法: python3 full_prediction.py
依赖: Python 3.8+ (标准库)

|v4.9 改进:
|  O. 9维权重 — state20%/league8%/def20%/tactics13%/fifa12% (v5.0: 防→20%, state→20%)
|  P. 防作弊修复 — compute_history_score移除当前年数据
|  Q. 独立测试 — 2018回测为真正out-of-sample验证
|  R. 赛后对比分析 — analyze_results.py自动对比实际结果(v5.0)
|  S. 负二项p=0.60 — 更多平局和冷门(v5.0)
|  A. xG防守曲线 — (100-def)/50, 拉大强弱差距
  B. 动态avg_goals — 小组1.70→32强1.40→...→决赛1.05
  C. 东道主加成 — 美/加/墨 场地适应分+3
  D. 赔率校准 — Betfair赔率反推, 拉升市场热门队
  E. 负二项分布 — p=0.65, 方差+54%, 冷门更真实
  F. 伤病→xG联动 — 直接影响atk/dfn评分
  G. 东道主xG+0.35 — 美/加/墨每场xG加成
  H. CONCACAF+0.10 — 海地/库拉索同大洲优势
  I. 权重优化 — 网格搜索: state26%/def16%/history8%
  J. 预期比分 — 平均比分替代模态值
  K. 历史对比修复 — 中文→英文队名映射
"""
import math, random, os, sys, json, re, html, subprocess
from datetime import date, timedelta

# ======== TOP SCORER DATA ========
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
try:
    from top_scorer_players import TEAM_SCORERS
    HAS_SCORER_DATA = True
except ImportError:
    TEAM_SCORERS = {}
    HAS_SCORER_DATA = False

# [NEW] 数据驱动评分 — 替代人工TEAM_DATA
from compute_ratings import compute_rating, WC_HISTORY

# 中英文翻译
from translations import tn, pn, TEAM_CN

# ======== REAL GROUPS FROM WIKIPEDIA ========
GROUPS = {
    "A": ["Mexico", "South Africa", "South Korea", "Czech Republic"],
    "B": ["Canada", "Bosnia and Herzegovina", "Qatar", "Switzerland"],
    "C": ["Brazil", "Morocco", "Haiti", "Scotland"],
    "D": ["United States", "Paraguay", "Australia", "Turkey"],
    "E": ["Germany", "Curaçao", "Ivory Coast", "Ecuador"],
    "F": ["Netherlands", "Japan", "Sweden", "Tunisia"],
    "G": ["Belgium", "Egypt", "Iran", "New Zealand"],
    "H": ["Spain", "Cape Verde", "Saudi Arabia", "Uruguay"],
    "I": ["France", "Senegal", "Iraq", "Norway"],
    "J": ["Argentina", "Algeria", "Austria", "Jordan"],
    "K": ["Portugal", "DR Congo", "Uzbekistan", "Colombia"],
    "L": ["England", "Croatia", "Ghana", "Panama"],
}

ALL_TEAMS = []
for g in GROUPS:
    ALL_TEAMS.extend(GROUPS[g])

# [新] 东道主和C O N C A C A F大洲优势 (2026北美洲世界杯)
HOSTS = {"United States", "Canada", "Mexico"}
CONCACAF = {"United States", "Canada", "Mexico", "Haiti", "Curaçao"}

FIFA_RANK = {
    "Argentina":1,"France":2,"Spain":3,"England":4,"Brazil":5,
    "Netherlands":6,"Portugal":7,"Belgium":8,"Germany":9,"Italy":10,
    "Croatia":11,"Uruguay":12,"Denmark":13,"Switzerland":14,"Morocco":15,
    "Colombia":16,"Mexico":17,"Japan":18,"Senegal":19,"United States":20,
    "Sweden":21,"Iran":22,"South Korea":23,"Australia":24,"Austria":25,
    "Turkey":26,"Ecuador":27,"Wales":28,"Egypt":29,"Nigeria":30,
    "Canada":31,"Paraguay":32,"Norway":33,"Scotland":34,"Czech Republic":35,
    "South Africa":36,"Cape Verde":37,"Qatar":38,"Saudi Arabia":39,
    "Iraq":40,"Ghana":41,"Panama":42,"Algeria":43,"Tunisia":44,
    "Ivory Coast":45,"Bosnia and Herzegovina":46,"Jordan":47,"Uzbekistan":48,
    "New Zealand":49,"DR Congo":50,"Haiti":51,"Curaçao":52
}

# Bet365赔率隐含概率 (2026.05.13)
# [C] 赔率校准: 用市场赔率反向拉升评分
# The Odds API Key — 必须通过环境变量提供，切勿硬编码（历史版本曾在此泄露真实密钥）
# 免费注册: the-odds-api.com ；设置示例: export ODDS_API_KEY="你的key"
ODDS_API_KEY = os.environ.get("ODDS_API_KEY", "")

# 初始赔率数据 (会被API实时数据覆盖)
BET365_IMPLIED = {}

# The Odds API 队名映射 (API队名 vs 我们用的队名)
ODDS_NAME_MAP = {
    "United States": "USA",
    "Bosnia and Herzegovina": "Bosnia & Herzegovina",
}

# ==================== [NEW] The Odds API 实时赔率 ====================

def fetch_live_odds():
    """通过PowerShell调用The Odds API获取实时夺冠赔率.
    使用Betfair数据(超额赔付率最低, 最准确).
    返回 {球队名: 隐含概率%} 字典.
    """
    if not ODDS_API_KEY:
        print("  [赔率] 未设置 ODDS_API_KEY 环境变量，跳过实时赔率校准（使用默认评分）")
        return None, None
    url = (f"https://api.the-odds-api.com/v4/sports/soccer_fifa_world_cup_winner/odds/"
           f"?apiKey={ODDS_API_KEY}&regions=uk&markets=outrights")
    try:
        cmd = [
            "powershell.exe", "-Command",
            f"$wc = New-Object System.Net.WebClient;"
            f"$wc.Headers.Add('User-Agent', 'Mozilla/5.0');"
            f"$r = $wc.DownloadString('{url}');"
            f"Write-Output $r"
        ]
        result = subprocess.run(cmd, capture_output=True, text=True, encoding="utf-8", errors="replace", timeout=20)
        if result.returncode == 0 and result.stdout.strip():
            data = json.loads(result.stdout)
            odds = {}
            for item in data:
                for bm in item.get('bookmakers', []):
                    # 优选 Betfair (超额赔付最低)
                    if bm['key'] in ('betfair_ex_uk', 'bet365_uk', 'williamhill_uk'):
                        for mkt in bm.get('markets', []):
                            total_implied = sum(1/o['price'] for o in mkt['outcomes'])
                            for o in mkt['outcomes']:
                                implied = (1/o['price']) / total_implied * 100
                                odds[o['name']] = round(implied, 2)
                        if odds:
                            return odds, bm['title']
        return None, None
    except:
        return None, None

# [已移除] 人工TEAM_DATA — 改用 compute_ratings 自动生成
# 保留空字典用于伤病调整
TEAM_DATA = {}

def init_team_data(fifa_rank):
    """用数据驱动评分初始化所有球队的8维评分."""
    td = {}
    from compute_ratings import STYLE
    for team, rank in fifa_rank.items():
        # 阿根廷是卫冕冠军, 施加魔咒修正
        is_champion = (team == "Argentina")
        td[team] = compute_rating(team, rank, 2026, defending_champion=is_champion)
    return td

# 8维评分: (近期状态, 历史战绩, 核心球员, 进攻, 防守, 场地适应, 战术体系, 伤病修正)
# 自动从FIFA排名+历史数据生成

# [G] 伤病数据缓存 (从Wikipedia自动获取)
INJURY_CACHE = {}

# ==================== [G] 自动伤病更新 ====================

def fetch_injuries_powershell():
    """用PowerShell从Wikipedia爬伤病数据."""
    try:
        cmd = [
            "powershell.exe", "-Command",
            "$wc = New-Object System.Net.WebClient; "
            "$wc.Headers.Add('User-Agent', 'Mozilla/5.0'); "
            "$url = 'https://en.wikipedia.org/w/api.php?action=parse&page=2026_FIFA_World_Cup&prop=text&section=9&format=json'; "
            "$r = $wc.DownloadString($url); "
            "Write-Output $r"
        ]
        result = subprocess.run(cmd, capture_output=True, text=True, encoding="utf-8", errors="replace", timeout=15)
        if result.returncode == 0:
            return result.stdout
    except Exception:
        pass
    return None

def parse_injuries_from_wikipedia(raw_json):
    """解析Wikipedia parse API返回的伤病章节HTML表格.

    返回值三态(便于上层显性区分):
      None  — 抓取/JSON 解析失败（无法判断有无伤病，需告警）
      {}    — 解析成功但当前页面无伤病条目
      {...} — {球员: {"team":队, "status":状态}}
    """
    if not raw_json:
        return None
    try:
        data = json.loads(raw_json)
    except (ValueError, TypeError):
        return None
    html_text = data.get("parse", {}).get("text", {}).get("*", "")
    if not html_text:
        return None

    def _strip(cell):
        cell = re.sub(r'<[^>]+>', ' ', cell)   # 去标签
        cell = html.unescape(cell)
        return re.sub(r'\s+', ' ', cell).strip()

    injures = {}
    # Wikipedia parse API 返回的是渲染后的 HTML，按表格行 <tr>/<td> 提取
    # （旧代码误用 wikitext 的 || 语法去匹配 HTML，导致永远抓不到数据）
    for row in re.findall(r'<tr[^>]*>(.*?)</tr>', html_text, re.DOTALL):
        cells = re.findall(r'<t[dh][^>]*>(.*?)</t[dh]>', row, re.DOTALL)
        if len(cells) < 4:
            continue
        team = _strip(cells[0])
        player = _strip(cells[1])
        status = _strip(cells[3]).lower()
        if not player:
            continue
        if any(k in status for k in ("out", "缺席", "confirmed", "doubt", "injured", "absent", "存疑")):
            injures[player] = {"team": team, "status": status}
    return injures

def apply_injury_updates(team_data):
    """[G] 根据爬到的伤病数据调整球队评分. 返回(修改球队数, 伤病总数).

    三种返回路径显性区分(不再静默吞失败):
      - 连接失败      → 告警 + (0,0)
      - 抓到页面但解析失败 → 警告 + (0,0)
      - 解析成功无伤病  → 提示 + (0,0)
    """
    modified = 0
    raw = fetch_injuries_powershell()
    if not raw:
        print("  [伤病] Wikipedia连接失败，使用本地伤病数据")
        return 0, 0
    injures = parse_injuries_from_wikipedia(raw)
    if injures is None:
        print("  [伤病][警告] 已抓取页面但无法解析伤病表格（章节结构可能已变 section=9），跳过伤病修正")
        return 0, 0
    if not injures:
        print("  [伤病] Wikipedia当前无伤病条目")
        return 0, 0
    print(f"  [伤病] 从Wikipedia获取到 {len(injures)} 条伤病信息")
    for player, info in injures.items():
        team = info["team"]
        status = info["status"]
        if team in team_data and team in TEAM_SCORERS:
            for pname, weight in TEAM_SCORERS.get(team, []):
                if pname.lower() in player.lower() or player.lower() in pname.lower():
                    # 判断伤病严重程度
                    if "out" in status or "absent" in status or "缺席" in status:
                        severity = 1.0
                        sev_label = "确认缺席"
                    elif "doubt" in status or "存疑" in status:
                        severity = 0.5
                        sev_label = "出战成疑"
                    else:
                        continue

                    # ⭐ 核心修复: 按球员权重(进球占比)决定对攻防评分的直接影响
                    # 权重≥0.20=核心射手, 0.10~0.19=中场/边锋, <0.10=防守球员
                    if weight >= 0.20:
                        atk_penalty, def_penalty = 12, 4   # 核心前锋/射手
                    elif weight >= 0.10:
                        atk_penalty, def_penalty = 7, 5    # 中场/边锋/二前锋
                    else:
                        atk_penalty, def_penalty = 3, 8    # 防守球员

                    atk_penalty *= severity
                    def_penalty *= severity

                    print(f"  [伤病] ⚠️ {player}({team}) {sev_label} → 进攻-{atk_penalty:.0f} 防守-{def_penalty:.0f} (权重{weight:.2f})")
                    td = list(team_data[team])
                    td[3] -= atk_penalty  # 进攻评分(index 3) — 直接影响xG
                    td[4] -= def_penalty  # 防守评分(index 4) — 直接影响失球
                    team_data[team] = tuple(td)
                    modified += 1
                    break
    return modified, len(injures)

# ==================== [NEW] 历史对比 ====================

# 中文队名 → 英文队名反向映射 (用于解析历史报告)
REVERSE_TEAM_CN = {v: k for k, v in TEAM_CN.items()}

# 所有产物统一写入运行时当前目录的 docs/world-cup-predictor/（SKILL.md 以绝对路径调用本脚本、不 cd）
# 注意：原代码引用了未定义的 SKILL_DIR（全文仅此一处），改用 getcwd 既统一输出目录也顺带修复了 NameError
REPORTS_DIR = os.path.join(os.getcwd(), "docs", "world-cup-predictor")
os.makedirs(REPORTS_DIR, exist_ok=True)

def load_previous_report():
    """扫描历史报告, 返回最新一份的夺冠率和金靴数据."""
    today = date.today().strftime("%Y%m%d")
    if not os.path.isdir(REPORTS_DIR):
        return None, None, None
    
    files = [f for f in os.listdir(REPORTS_DIR) if f.endswith(".md") and f[:-3] < today]
    if not files:
        return None, None, None
    
    files.sort(reverse=True)
    latest = os.path.join(REPORTS_DIR, files[0])
    
    prev_champ = {}   # team -> pct  (用英文队名)
    prev_scorer = {}  # player -> avg_goals
    prev_version = None
    
    with open(latest, "r", encoding="utf-8") as f:
        for line in f:
            # 读取版本号
            if "预测报告" in line and "v" in line:
                m = re.search(r'v(\d+\.\d+)', line)
                if m: prev_version = m.group(1)
            
            # 夺冠概率表: | 1 | Argentina | 10.2% | 92% |
            m = re.match(r'^\|\s*\d+\s*\|\s*(.+?)\s*\|\s*([\d.]+)%', line)
            if m:
                team_raw = m.group(1).strip()
                # ⭐ 中文名 → 英文名 (历史报告用tn()存储中文名)
                team = REVERSE_TEAM_CN.get(team_raw, team_raw)
                pct = float(m.group(2))
                prev_champ[team] = pct
            
            # 金靴表: | 1 | Kylian Mbappé | France | 3.66 | 25.6球 |
            m = re.match(r'^\|\s*\d+\s*\|\s*(.+?)\s*\|\s*(.+?)\s*\|\s*([\d.]+)\s*\|', line)
            if m:
                name = m.group(1).strip()
                avg = float(m.group(3))
                prev_scorer[name] = avg
    
    return prev_champ, prev_scorer, prev_version if prev_champ else (None, None, None)

# ==================== ENGINE ====================

def fifa_rank_score(rank):
    if rank <= 5: return 13 + (6-rank)*0.4
    elif rank <= 10: return 11 + (10-rank)*0.4
    elif rank <= 20: return 8 + (20-rank)*0.3
    elif rank <= 30: return 5 + (30-rank)*0.3
    elif rank <= 40: return 3 + (40-rank)*0.2
    else: return max(0, 3 - (rank-40)*0.3)

def make_team(name, data):
    rf,wh,cp,atk,defn,ada,tac,inj,league = data
    fr = fifa_rank_score(FIFA_RANK.get(name,50))
    fr_raw = fr / 0.15
    # [v2] 攻防评分分离: xG用FIFA排名值, base_score用真实预选赛数据
    from qualifying_data import get_qualifying_rating
    qatk, qdfn = get_qualifying_rating(name)
    # 9维加权: 近期状态20% + FIFA排名12% + 历史5% + 核心12% + 真实进攻8% + 真实防守20% + 场地2% + 战术13% + 联赛8%
    base_score = rf*0.20 + fr_raw*0.12 + wh*0.05 + cp*0.12 + qatk*0.08 + qdfn*0.20 + ada*0.02 + tac*0.13 + league*0.08
    
    # [E] 赔率校准: Betfair实时赔率微调 (权重约2-3%)
    implied = BET365_IMPLIED.get(name, BET365_IMPLIED.get(ODDS_NAME_MAP.get(name, ""), 0.05))
    # 隐含概率×0.08: Spain 16.4%→+1.31分, 丹麦0.1%→+0.008分
    odds_boost = implied * 0.08
    base_score += odds_boost
    
    return {"score":round(base_score,2),"final":round(base_score+inj,2),
            "atk":atk,"defn":defn,"inj":inj}

def poisson(lmbda):
    if lmbda <= 0: return 0
    L = math.exp(-lmbda); k=0; p=1.0
    while p > L: k+=1; p*=random.random()
    return max(0,k-1)

def neg_binom(mean, p=0.60):
    """[F] 负二项分布: 比泊松更多方差(冷门更多).
    方差 = mean/p, 当p=0.60时方差增大67% (v5.0: 从0.65调至0.60增加平局).
    使用Gamma-Poisson混合实现.
    """
    if mean <= 0: return 0
    if mean < 0.3: return poisson(mean)  # 小均值直接用泊松
    r = mean * p / (1 - p)  # shape parameter
    if r < 1:
        # 小r用简单方法
        return poisson(mean)
    # Gamma(r, scale=(1-p)/p) → Poisson
    rate = random.gammavariate(r, (1-p)/p)
    return poisson(rate)

def xg(atk, deff, avg=1.60):
    """[A] xG公式: 防守系数(100-def)/50 加大强弱队差距"""
    return avg * (atk/50) * ((100-deff)/50)

# [B] 动态avg_goals
ROUND_AVG = {
    "group": 1.65,   # v5.0: 从1.70调至1.65 (减少进球高估)
    "r32": 1.40,
    "r16": 1.30,
    "qf": 1.20,
    "sf": 1.10,
    "final": 1.05,
}

def predict(t1_name, t2_name, atk1, def1, atk2, def2, ko=False, seed=None, round_type="group"):
    if seed is not None: random.seed(seed)
    avg = ROUND_AVG.get(round_type, 1.35)
    xg1 = max(0.15, xg(atk1, def2, avg))
    xg2 = max(0.15, xg(atk2, def1, avg))
    
    # [H] 东道主及大洲优势 (历史数据: 东道主场均+0.38球, 同大洲+0.15球)
    if t1_name in HOSTS: xg1 += 0.35
    if t2_name in HOSTS: xg2 += 0.35
    if t1_name in CONCACAF and t1_name not in HOSTS: xg1 += 0.10
    if t2_name in CONCACAF and t2_name not in HOSTS: xg2 += 0.10
    
    # [F] 负二项分布: p=0.60, 方差+67%, 更多平局和冷门(v5.0校准)
    # 方差 = mean/p, 当p=0.60时方差增大67%.
    g1 = neg_binom(xg1, 0.60)
    g2 = neg_binom(xg2, 0.60)
    
    if g1 > g2: return "t1",g1,g2
    if g2 > g1: return "t2",g1,g2
    if ko:
        # [N] 加时/点球校准 (基于历史数据)
        random.seed((seed or 0)+999)
        if random.random() < 0.50:
            # 加时赛决出胜负: 基于攻防综合评分
            atk_adv = atk1 + def1 - atk2 - def2
            t1_et_win = 0.50 + min(0.12, atk_adv * 0.002)
            if random.random() < t1_et_win: g1 += 1
            else: g2 += 1
        if g1 == g2:
            # 点球: 几乎均衡, 球星微优势
            random.seed((seed or 0)+1000)
            star_adv = (atk1 + def1) - (atk2 + def2)
            t1_pk_win = 0.50 + min(0.04, star_adv * 0.0005)
            if random.random() < t1_pk_win: g1 += 1
            else: g2 += 1
        return ("t1" if g1>g2 else "t2"),g1,g2
    return "draw",g1,g2

def distribute_goals(team, goals, scorer_goals):
    if team not in TEAM_SCORERS or goals == 0:
        return
    players = TEAM_SCORERS[team]
    names = [p[0] for p in players]
    weights = [p[1] for p in players]
    for _ in range(goals):
        scorer = random.choices(names, weights=weights)[0]
        if scorer != "other":
            key = (team, scorer)
            scorer_goals[key] = scorer_goals.get(key, 0) + 1

def build_knockout_bracket(advancing, TEAMS):
    """[v4.5] FIFA 2026真实淘汰赛配对: 4路径×3组/路径
    
    advancing: [gA_head, gA_runner, gB_head, gB_runner, ..., gL_head, gL_runner, 8第三名]
    返回: [(team1, team2), ...] 共16对, 按路径连续排列
    """
    # 分离头名 and 第二名 (按A-L顺序)
    winners = [advancing[i] for i in range(0, 24, 2)]   # 12支: A-L头名
    runners = [advancing[i] for i in range(1, 24, 2)]   # 12支: A-L第二
    thirds = advancing[24:32]  # 8支最佳第三名
    
    # 4路径: [A,B,C], [D,E,F], [G,H,I], [J,K,L]
    # 每个路径 3头名 + 3第二 + 2第三名 = 8队 → 4场R32
    path_ranges = [(0, 3), (3, 6), (6, 9), (9, 12)]
    
    # 第三名分配到4个路径 (按评分蛇形: 最强给最强路径等)
    thirds_sorted = sorted(thirds, key=lambda t: TEAMS[t]["final"], reverse=True)
    path_thirds = [
        [thirds_sorted[0], thirds_sorted[7]],  # Path 1 (A-C): 最强+最弱第三
        [thirds_sorted[1], thirds_sorted[6]],  # Path 2 (D-F): 次强+次弱
        [thirds_sorted[2], thirds_sorted[5]],  # Path 3 (G-I)
        [thirds_sorted[3], thirds_sorted[4]],  # Path 4 (J-L): 最弱+次强
    ]
    
    bracket = []
    
    for pi, (start, end) in enumerate(path_ranges):
        pw = sorted(winners[start:end], key=lambda t: TEAMS[t]["final"], reverse=True)
        pr = sorted(runners[start:end], key=lambda t: TEAMS[t]["final"], reverse=True)
        pt = sorted(path_thirds[pi], key=lambda t: TEAMS[t]["final"], reverse=True)
        
        # 4场R32配对 (路径内):
        # ① 最强头名 vs 较弱第三名 (路径最强对最弱)
        # ② 中间头名 vs 较强第三名
        # ③ 最弱头名 vs 最弱第二名
        # ④ 最强第二名 vs 中间第二名
        bracket.append((pw[0], pt[1]))   # Match 1: 强胜 vs 弱三
        bracket.append((pw[1], pt[0]))   # Match 2: 中胜 vs 强三
        bracket.append((pw[2], pr[2]))   # Match 3: 弱胜 vs 弱二
        bracket.append((pr[0], pr[1]))   # Match 4: 强二 vs 中二
    
    # 淘汰赛树:
    # Path 1: bracket[0:4] → R16: [0vs1], [2vs3] → QF: [R16winner1 vs R16winner2]
    # Path 2: bracket[4:8]  → R16: [4vs5], [6vs7]  → QF: ...
    # 半决赛: Path1-QFwinner vs Path2-QFwinner
    #         Path3-QFwinner vs Path4-QFwinner
    # 决赛: SF1-winner vs SF2-winner
    return bracket

# ==================== MAIN ====================

def main():
    # 加载历史对比数据
    prev_champ, prev_scorer, prev_ver = load_previous_report()
    
    # [H] 实时赔率: 从The Odds API获取
    data_sources = []
    print("  [正在获取实时赔率...]")
    live_odds, odds_source = fetch_live_odds()
    if live_odds:
        global BET365_IMPLIED
        BET365_IMPLIED = live_odds
        data_sources.append(f"Betfair实时赔率({odds_source})")
        print(f"  ✅ 从 {odds_source} 获取到 {len(live_odds)} 支球队的实时赔率")
        top3 = sorted(live_odds.items(), key=lambda x: x[1], reverse=True)[:3]
        for t, p in top3:
            print(f"     {t}: {p:.1f}%")
    else:
        print("  ⚠️ API获取失败, 使用默认赔率数据")
        # 备用硬编码数据
        BET365_IMPLIED = {"France":22.2,"Argentina":20.0,"Brazil":18.2,"England":16.7,
            "Germany":11.1,"Spain":10.0,"Portugal":7.7,"Netherlands":7.0,
            "Belgium":5.5,"Croatia":4.5,"USA":4.8,"Mexico":3.8,"Canada":1.2}
    
    print("="*60)
    print("  2026 FIFA世界杯预测 v5.0")
    print(f"  分组: Wikipedia (抽签2025.12.5)")
    print("  [O] 9维权重: state20% league8% def20% tac13% fifa12% (v5.0)")
    print("  [P] 防作弊: history只读≤当前年数据")
    print("  [M]真实路径 [N]加时点球 [A]xG [B]动态avg [C]东道主 [D]赔率 [E]负二项 [F]伤病 [G]xG+0.35 [H]CONCACAF [I]预期比分")
    if data_sources:
        print(f"  📡 外部数据: {' | '.join(data_sources)}")
    print("="*60)
    
    # 用数据驱动评分初始化 (替代人工TEAM_DATA)
    team_data_dict = init_team_data(FIFA_RANK)
    
    # [G] 自动伤病更新 (修改评分中的伤病字段)
    print("\n  [正在获取伤病数据...]")
    injury_count, infected_teams = apply_injury_updates(team_data_dict)
    injured_teams = infected_teams  # 兼容命名
    infected_desc = "无"
    
    TEAMS = {}
    for n in ALL_TEAMS:
        if n in team_data_dict:
            TEAMS[n] = make_team(n, team_data_dict[n])
    
    N_SIM = 2000
    champ_count = {t:0 for t in TEAMS}
    adv_count = {t:0 for t in TEAMS}
    scorer_goals = {}
    
    # 小组赛比分追踪
    match_stats = {}  # (group, t1, t2) -> {"t1_w":0, "d":0, "t2_w":0, "goals":{}}
    
    for sim in range(N_SIM):
        bs = 100 + sim * 37
        random.seed(bs)
        advancing = []       # 按顺序: 头名(12) + 第二名(12) + 第三名(8)
        all_third = []

        for gi, g in enumerate("ABCDEFGHIJKL"):
            gteams = GROUPS[g]
            tbl = {t:{"pts":0,"gd":0,"gf":0} for t in gteams}
            matches = [(gteams[i],gteams[j]) for i in range(4) for j in range(i+1,4)]

            for mi,(t1,t2) in enumerate(matches):
                td1,td2 = TEAMS[t1],TEAMS[t2]
                r,g1,g2 = predict(t1, t2, td1["atk"], td1["defn"],
                                  td2["atk"], td2["defn"],seed=bs+gi*100+mi*7,
                                  round_type="group")
                tbl[t1]["gf"]+=g1;tbl[t2]["gf"]+=g2
                tbl[t1]["gd"]+=g1-g2;tbl[t2]["gd"]+=g2-g1
                if r=="t1":tbl[t1]["pts"]+=3
                elif r=="t2":tbl[t2]["pts"]+=3
                else:tbl[t1]["pts"]+=1;tbl[t2]["pts"]+=1
                distribute_goals(t1, g1, scorer_goals)
                distribute_goals(t2, g2, scorer_goals)
                # 追踪比分
                key = (g, t1, t2)
                if key not in match_stats:
                    match_stats[key] = {"t1_w":0,"d":0,"t2_w":0,"goals":{},"t1_g":0,"t2_g":0}
                if r=="t1": match_stats[key]["t1_w"]+=1
                elif r=="t2": match_stats[key]["t2_w"]+=1
                else: match_stats[key]["d"]+=1
                match_stats[key]["t1_g"] += g1
                match_stats[key]["t2_g"] += g2
                score_key = f"{g1}:{g2}"
                match_stats[key]["goals"][score_key] = match_stats[key]["goals"].get(score_key,0)+1

            st = sorted(tbl.items(),key=lambda x:(x[1]["pts"],x[1]["gd"],x[1]["gf"]),reverse=True)
            advancing.append(st[0][0])   # 头名
            advancing.append(st[1][0])   # 第二名
            all_third.append((st[2][0],st[2][1]["pts"],st[2][1]["gd"]))

        # 前8名第三名晋级
        all_third.sort(key=lambda x:(x[1],x[2]),reverse=True)
        for t,_,_ in all_third[:8]:
            advancing.append(t)
        
        for t in advancing:
            adv_count[t]=adv_count.get(t,0)+1

        # [D] 真实淘汰赛配对
        cur = build_knockout_bracket(advancing, TEAMS)

        # 淘汰赛
        rd_names = ["r32", "r16", "qf", "sf", "final"]
        for rd in range(5):
            nxt = []
            for mi,(t1,t2) in enumerate(cur):
                td1,td2 = TEAMS[t1],TEAMS[t2]
                r,g1,g2 = predict(t1, t2, td1["atk"], td1["defn"],
                                  td2["atk"], td2["defn"],ko=True,
                                  seed=bs+10000+rd*1000+mi*13,
                                  round_type=rd_names[rd])
                winner = t1 if r=="t1" else t2
                nxt.append(winner)
                distribute_goals(t1, g1, scorer_goals)
                distribute_goals(t2, g2, scorer_goals)
                if rd==4:
                    champ_count[winner]=champ_count.get(winner,0)+1
            if rd<4:
                cur = [(nxt[i],nxt[i+1]) for i in range(0,len(nxt),2)]

        if (sim+1)%500==0:
            print(f"  已完成 {sim+1}/{N_SIM}...")

    # ===== 结果输出 =====
    cs = sorted(champ_count.items(),key=lambda x:x[1],reverse=True)
    
    # 计算每队预期比赛场次
    team_avg_matches = {}
    for team in TEAMS:
        adv_pct = adv_count.get(team, 0) / N_SIM
        # 3场小组赛 + 淘汰赛场次(晋级越远越多)
        team_avg_matches[team] = 3.0 + adv_pct * 3.5  # 晋级率越高, 预期场次越多
    
    # 金靴输出 (按场均进球排序)
    player_team = {}
    player_per_match = {}  # 场均进球
    player_total = {}      # 赛事总进球
    
    for (team, pname), goals in scorer_goals.items():
        total_avg = goals / N_SIM  # 每届赛事平均总进球
        matches = team_avg_matches.get(team, 3.0)
        per_match = total_avg / matches  # 场均进球
        player_per_match[pname] = player_per_match.get(pname, 0) + per_match
        player_total[pname] = player_total.get(pname, 0) + total_avg
        if pname not in player_team:
            player_team[pname] = team
    
    top_scorers = sorted(player_total.items(), key=lambda x: x[1], reverse=True)
    top_scorers_per_match = sorted(player_per_match.items(), key=lambda x: x[1], reverse=True)

    print(f"\n{'='*60}")
    print("  🏆 夺冠概率 Top 20")
    print(f"{'='*60}")
    print(f"{'#':<3} {'球队':<22} {'夺冠率':<10} {'32强率':<8}")
    print("-"*48)
    for i,(team,c) in enumerate(cs[:20],1):
        pct = c/N_SIM*100
        r32 = adv_count.get(team,0)/N_SIM*100
        bar = "█"*int(pct/1.5)+"░"*max(0,10-int(pct/1.5))
        print(f"{i:<3} {tn(team):<22} {pct:<7.2f}%  {r32:<6.1f}%  {bar}")

    print(f"\n{'='*60}")
    print("  ⚽ 金靴奖预测 (总进球/场均)")
    print(f"{'='*60}")
    print(f"{'#':<3} {'球员':<24} {'国家':<16} {'总进球':<8} {'场均':<8}")
    print("-"*58)
    for i,(pname, total) in enumerate(top_scorers[:20],1):
        team = player_team.get(pname, "?")
        per_match = player_per_match.get(pname, 0)
        bar = "⚽"*min(10,int(total))+"·"*max(0,10-int(total))
        print(f"{i:<3} {pn(pname):<24} {tn(team):<16} {total:<7.2f} {per_match:<7.2f}  {bar}")

    # ===== 小组赛预测 =====
    print(f"\n{'='*60}")
    print("  📋 小组赛预测 (各场最可能比分)")
    print(f"{'='*60}")
    
    for g in "ABCDEFGHIJKL":
        gteams = GROUPS[g]
        matches = [(gteams[i],gteams[j]) for i in range(4) for j in range(i+1,4)]
        
        print(f"\n  ┌─ {g}组 ───────────────────┐")
        # 计算每队预期积分
        expected_pts = {t:0 for t in gteams}
        expected_gd = {t:0 for t in gteams}
        expected_gf = {t:0 for t in gteams}
        
        for t1, t2 in matches:
            key = (g, t1, t2)
            st = match_stats.get(key, {"t1_w":0,"d":0,"t2_w":0,"goals":{"0:0":2000},"t1_g":0,"t2_g":0})
            t1_wp = st["t1_w"]/N_SIM*100
            dp = st["d"]/N_SIM*100
            t2_wp = st["t2_w"]/N_SIM*100
            # [v4.4] 预期平均比分 (2000次MC平均, 比模态值更多样)
            avg_g1 = st["t1_g"] / N_SIM
            avg_g2 = st["t2_g"] / N_SIM
            score_str = f"预期{avg_g1:.1f}:{avg_g2:.1f}"
            
            expected_pts[t1] += t1_wp/100*3 + dp/100*1
            expected_pts[t2] += t2_wp/100*3 + dp/100*1
            # 用预期平均进球算GD
            avg_g1 = st["t1_g"] / N_SIM
            avg_g2 = st["t2_g"] / N_SIM
            expected_gf[t1] += avg_g1
            expected_gf[t2] += avg_g2
            expected_gd[t1] += avg_g1 - avg_g2
            expected_gd[t2] += avg_g2 - avg_g1
            
            outcome = "✅" if t1_wp > 45 else "🤝" if dp > max(t1_wp,t2_wp) else "❌" if t2_wp > 45 else "⚡"
            print(f"  {tn(t1):<20} vs {tn(t2):<20}")
            print(f"  {score_str}  {outcome}  {t1_wp:.0f}%/{dp:.0f}%/{t2_wp:.0f}%")
        
        # 预期排名
        print(f"  ├─ 预期排名 ─────────────┤")
        ranked = sorted(gteams, key=lambda t: (-expected_pts[t], -expected_gd[t], -expected_gf[t]))
        for i, t in enumerate(ranked, 1):
            adv_pct = adv_count.get(t,0)/N_SIM*100
            print(f"  {i}. {tn(t):<20} {expected_pts[t]:.1f}分 GD:{expected_gd[t]:+.1f}  晋级:{adv_pct:.0f}%")
    
    # ===== 历史对比 =====
    # max_diff_teams / change_explanations 提前初始化：首次运行（无历史报告）时 prev_champ 为 None，
    # 下方整个对比段会被 `if prev_champ:` 跳过，但报告写入段仍会引用这两个变量，必须先定义以防 NameError
    max_diff_teams = []
    change_explanations = []
    if prev_champ:
        print(f"\n{'='*60}")
        print(f"  📊 与上次预测对比 (v{prev_ver})")
        print(f"{'='*60}")
        print(f"  {'球队':<20} {'上次%':<8} {'本次%':<8} {'变化':<8}")
        print("-"*50)
        # 取前10 + 历史前10中有变动的
        all_teams_for_compare = set()
        for team, _ in cs[:12]:
            all_teams_for_compare.add(team)
        for team in prev_champ:
            if prev_champ[team] > 1.0:
                all_teams_for_compare.add(team)
        # 按本次概率排序
        team_pct_new = {team: c/N_SIM*100 for team, c in cs}
        sorted_teams = sorted(all_teams_for_compare, key=lambda t: team_pct_new.get(t, 0), reverse=True)
        for team in sorted_teams[:15]:
            pct_new = team_pct_new.get(team, 0)
            pct_old = prev_champ.get(team, 0)
            diff = pct_new - pct_old
            arr = "↑" if diff > 0.3 else "↓" if diff < -0.3 else "→"
            bar = "📈" if diff > 0.3 else "📉" if diff < -0.3 else "➡️"
            if abs(diff) > 0.5:
                max_diff_teams.append((team, diff))
            print(f"  {tn(team):<20} {pct_old:<7.1f}%  {pct_new:<7.1f}%  {bar} {arr}{abs(diff):.1f}%")

        # 变化原因分析
        print(f"\n  📋 变化原因分析:")

        # 1. 版本变更
        if prev_ver and prev_ver != "4.9":
            change_explanations.append(f"• 引擎版本 v{prev_ver} → v4.9: 9维权重(state22%/league8%/def18%/tactics13%/fifa12%) + 新增联赛表现")
        elif prev_ver and prev_ver == "4.9":
            change_explanations.append(f"• 版本未变更, 仅随机种子差异 (负二项分布特性)")

        # 2. 伤病变更
        if injury_count > 0:
            change_explanations.append(f"• 伤病更新: Wikipedia获取到 {injury_count} 条伤病信息, {injured_teams} 支队评分调整 ({infected_desc})")

        # 3. 数据未变更
        if not change_explanations and prev_ver:
            change_explanations.append("• 数据未更新 (无新伤病/版本无变更), 结果稳定")
            if max_diff_teams:
                change_explanations.append(f"• 但部分球队有>0.5%波动 (负二项分布随机性导致)")

        for exp in change_explanations[:5]:
            print(f"  {exp}")

        # 对变化最大的球队给出原因
        if max_diff_teams and change_explanations:
            print(f"  📈 显著变化球队:")
            for team, diff in sorted(max_diff_teams, key=lambda x: abs(x[1]), reverse=True)[:3]:
                if abs(diff) >= 1.0:
                    if "伤病" in str(change_explanations):
                        print(f"    {team}: {diff:+.1f}% (伤病/引擎升级综合影响)")
                    else:
                        print(f"    {team}: {diff:+.1f}% (引擎升级导致权重调整)")

    # Save report
    rp = os.path.join(REPORTS_DIR, date.today().strftime("%Y%m%d") + ".md")
    os.makedirs(os.path.dirname(rp),exist_ok=True)
    with open(rp,"w",encoding="utf-8") as f:
        f.write(f"# 2026 FIFA世界杯预测报告 v5.0\n")
        f.write(f"> 生成: {date.today()} | 分组: Wikipedia (抽签2025.12.5)\n")
        f.write(f"> 模拟: {N_SIM}次蒙特卡洛 (含金靴奖) | v5.0: 9维权重(state20%/def20%/p=0.60)\n\n")
        
        f.write("## 🏆 夺冠概率\n\n")
        f.write("| # | 球队 | 夺冠率 | 32强率 |\n|:-:|:----|:----:|:----:|\n")
        for i,(t,c) in enumerate(cs[:20],1):
            f.write(f"| {i} | {tn(t)} | {c/N_SIM*100:.1f}% | {adv_count.get(t,0)/N_SIM*100:.0f}% |\n")
        
        f.write(f"\n## ⚽ 金靴奖预测\n\n")
        f.write("| # | 球员 | 国家 | 预计总进球 | 场均进球 |\n|:-:|:----|:----|:----:|:----:|\n")
        for i,(pname, total) in enumerate(top_scorers[:15],1):
            team = player_team.get(pname, "?")
            per_match = player_per_match.get(pname, 0)
            f.write(f"| {i} | {pn(pname)} | {tn(team)} | {total:.1f} | {per_match:.2f} |\n")
        
        # 小组赛预测
        f.write(f"\n## 📋 小组赛预测\n\n")
        for g in "ABCDEFGHIJKL":
            gteams = GROUPS[g]
            matches = [(gteams[i],gteams[j]) for i in range(4) for j in range(i+1,4)]
            f.write(f"### {g}组\n\n")
            f.write("| 主队 | 客队 | 预期比分 | 主胜% | 平% | 客胜% |\n|:----|:----|:-------:|:----:|:---:|:----:|\n")
            for t1, t2 in matches:
                key = (g, t1, t2)
                st = match_stats.get(key, {"t1_w":0,"d":0,"t2_w":0,"goals":{"0:0":2000},"t1_g":0,"t2_g":0})
                avg_g1 = st["t1_g"] / N_SIM
                avg_g2 = st["t2_g"] / N_SIM
                top_scores = f"{avg_g1:.1f}:{avg_g2:.1f}"
                t1_wp = st["t1_w"]/N_SIM*100
                dp = st["d"]/N_SIM*100
                t2_wp = st["t2_w"]/N_SIM*100
                f.write(f"| {tn(t1)} | {tn(t2)} | {top_scores} | {t1_wp:.0f}% | {dp:.0f}% | {t2_wp:.0f}% |\n")
            f.write("\n")
        
        f.write(f"\n## 📊 与上次预测对比\n\n")
        if prev_champ:
            f.write("| 球队 | 上次 | 本次 | 变化 |\n|:----|:---:|:---:|:---:|\n")
            team_pct_new = {team: c/N_SIM*100 for team, c in cs}
            all_teams = set(list(team_pct_new.keys())[:12])
            for t, p in prev_champ.items():
                if p > 1.0: all_teams.add(t)
            sorted_t = sorted(all_teams, key=lambda t: team_pct_new.get(t,0), reverse=True)
            for team in sorted_t[:15]:
                pn2 = team_pct_new.get(team, 0)
                po = prev_champ.get(team, 0)
                d2 = pn2 - po
                arr = "📈 +" if d2 > 0 else "📉 " if d2 < 0 else "➡️ "
                f.write(f"| {tn(team)} | {po:.1f}% | {pn2:.1f}% | {arr}{abs(d2):.1f}% |\n")
        # 变化原因
        f.write("\n**变化原因**:\n")
        if change_explanations:
            for exp in change_explanations[:5]:
                f.write(f"{exp}\n")
        else:
            f.write("首次报告，无历史对比\n")
        f.write("\n")
        
        f.write(f"\n## v5.0 升级内容\n\n")
        f.write("| 改进 | 描述 |\n|------|------|\n")
        f.write("| O. 9维权重 | state20%+league8%+def20%+tactics13%+fifa12% (防守+2%,状态-2%) |\n")
        f.write("| R. 赛后对比 | analyze_results.py 自动对比实际比赛结果 |\n")
        f.write("| S. 负二项p=0.60 | 方差+67%, 更多平局和冷门 |\n")
        f.write("| B. avg_goals | 小组1.70→1.65, 减少进球高估 |\n")
        
        f.write(f"\n## 方法论\n")
        f.write(f"8维评分+赔率校准+负二项分布+{N_SIM}次蒙特卡洛\n")
        if data_sources:
            f.write(f"\n**外部数据源**: {' | '.join(data_sources)}\n")
        f.write(f"> 数据: Wikipedia, Betfair实时赔率, FIFA排名 | v4.9 9维权重+联赛表现\n")

    print(f"\n📄 完整报告: {rp}")
    
    # ===== 赛后分析 =====
    # 检查是否有实际比赛结果可对比
    yesterday = (date.today() - timedelta(days=1)).strftime("%Y%m%d")
    print(f"\n  [分析] 正在检查 {yesterday} 的比赛结果...")
    try:
        import subprocess, sys
        analysis_cmd = [sys.executable or "python3", 
                       os.path.join(os.path.dirname(__file__), "analyze_results.py"),
                       "--date", yesterday]
        analysis_result = subprocess.run(analysis_cmd, capture_output=True, text=True, encoding="utf-8", errors="replace", timeout=60)
        if analysis_result.returncode == 0:
            for line in analysis_result.stdout.split('\n'):
                if line.strip():
                    print(f"  {line}")
            if analysis_result.stderr:
                print(f"  [分析] stderr: {analysis_result.stderr[:200]}")
        else:
            print(f"  [分析] 分析脚本返回码 {analysis_result.returncode}")
    except Exception as e:
        print(f"  [分析] 跳过: {e}")


if __name__ == "__main__":
    main()
