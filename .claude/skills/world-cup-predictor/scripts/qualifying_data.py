"""
预选赛真实比赛数据 (2023-2025)
=================================
从Wikipedia API爬取6个大洲预选赛818场比赛结果。
每队的场均进球(GF/M)和场均失球(GA/M)来自实际比赛数据。

v2: 新增 get_qualifying_bonus() — 对比实际表现vs排名预期, 返回加成值

用法:
  from qualifying_data import get_qualifying_bonus, GF_GA_DATA, CONF_MAP
  bonus = get_qualifying_bonus("Brazil")  # 返回 -3 到 +3 的加成值
"""

# 大洲归一化系数 (基于跨大洲比赛历史表现)
# UEFA=1.0为基准, 其他大洲按相对强度折扣
CONF_FACTOR = {
    "uefa": 1.00,
    "conmebol": 0.95,
    "caf": 0.82,
    "afc": 0.78,
    "concacaf": 0.73,
    "ofc": 0.38,
}

# 每队所属大洲
CONF_MAP = {
    "Argentina": "conmebol", "Brazil": "conmebol", "Uruguay": "conmebol",
    "Colombia": "conmebol", "Ecuador": "conmebol", "Paraguay": "conmebol",
    "France": "uefa", "Spain": "uefa", "England": "uefa",
    "Netherlands": "uefa", "Portugal": "uefa", "Belgium": "uefa",
    "Germany": "uefa", "Croatia": "uefa", "Switzerland": "uefa",
    "Sweden": "uefa", "Austria": "uefa", "Turkey": "uefa",
    "Scotland": "uefa", "Norway": "uefa", "Czech Republic": "uefa",
    "Bosnia and Herzegovina": "uefa",
    "Morocco": "caf", "Senegal": "caf", "Egypt": "caf",
    "Tunisia": "caf", "Algeria": "caf", "Ivory Coast": "caf",
    "Ghana": "caf", "Cape Verde": "caf", "DR Congo": "caf",
    "South Africa": "caf",
    "Japan": "afc", "South Korea": "afc", "Australia": "afc",
    "Iran": "afc", "Saudi Arabia": "afc", "Iraq": "afc",
    "Uzbekistan": "afc", "Jordan": "afc", "Qatar": "afc",
    "United States": "concacaf", "Mexico": "concacaf", "Canada": "concacaf",
    "Panama": "concacaf", "Haiti": "concacaf", "Curaçao": "concacaf",
    "New Zealand": "ofc",
}

# 预选赛真实数据: (场均进球GF/M, 场均失球GA/M, 比赛场次)
# 来源: 2023-2025 FIFA世界杯预选赛 + CONCACAF Nations League
# 空白队(东道主)使用最近友谊赛+CONCACAF数据估算
GF_GA_DATA = {
    # === CONMEBOL (18场/队, 双循环) ===
    "Argentina":   (1.72, 0.56, 18),
    "Brazil":      (1.33, 0.94, 18),
    "Uruguay":     (1.22, 0.67, 18),
    "Colombia":    (1.56, 1.00, 18),
    "Ecuador":     (0.78, 0.28, 18),
    "Paraguay":    (0.78, 0.56, 18),
    
    # === UEFA (6-8场/队, 小组赛) ===
    "France":      (2.67, 0.67, 6),
    "Spain":       (3.50, 0.33, 6),
    "England":     (2.75, 0.00, 8),
    "Netherlands": (3.38, 0.50, 8),
    "Portugal":    (3.33, 1.17, 6),
    "Belgium":     (3.62, 0.88, 8),
    "Germany":     (2.67, 0.50, 6),
    "Croatia":     (3.25, 0.50, 8),
    "Switzerland": (2.33, 0.33, 6),
    "Sweden":      (0.67, 2.00, 6),
    "Austria":     (2.75, 0.50, 8),
    "Turkey":      (2.83, 2.00, 6),
    "Scotland":    (2.17, 1.17, 6),
    "Norway":      (4.62, 0.62, 8),
    "Czech Republic": (2.25, 1.00, 8),
    "Bosnia and Herzegovina": (2.12, 0.88, 8),
    
    # === CAF (9-10场/队) ===
    "Morocco":     (2.75, 0.25, 8),
    "Senegal":     (2.20, 0.30, 10),
    "Egypt":       (2.00, 0.20, 10),
    "Tunisia":     (2.11, 0.00, 9),
    "Algeria":     (2.40, 0.80, 10),
    "Ivory Coast": (2.50, 0.00, 10),
    "Ghana":       (2.30, 0.60, 10),
    "Cape Verde":  (1.60, 0.80, 10),
    "DR Congo":    (1.50, 0.60, 10),
    "South Africa":(1.50, 0.90, 10),
    
    # === AFC (16-18场/队, 多轮) ===
    "Japan":       (3.38, 0.19, 16),
    "Iran":        (2.19, 0.75, 16),
    "South Korea": (2.00, 0.57, 14),
    "Australia":   (2.38, 0.44, 16),
    "Saudi Arabia":(1.22, 0.72, 18),
    "Iraq":        (1.50, 0.61, 18),
    "Uzbekistan":  (1.69, 0.69, 16),
    "Jordan":      (2.00, 0.75, 16),
    "Qatar":       (2.06, 1.56, 18),
    
    # === CONCACAF (6场/队, 第三轮) ===
    "Panama":      (2.33, 0.17, 6),
    "Haiti":       (2.11, 0.89, 9),
    # 东道主(未打预选赛, 使用CONCACAF Nations League/友谊赛估算)
    "United States": (1.80, 0.70, 10),
    "Mexico":      (1.60, 0.80, 10),
    "Canada":      (1.50, 0.90, 10),
    "Curaçao":     (0.80, 1.80, 8),
    
    # === OFC ===
    "New Zealand": (6.33, 0.33, 3),
}


def get_qualifying_bonus(team, fifa_rank=None):
    """
    基于预选赛真实表现 vs FIFA排名预期 的差额.
    返回 -3 到 +3 的加成分, 正值 = 排名低估了该队.
    ...
    """
    if team not in GF_GA_DATA:
        return 0

    FIFA_RANK = {
        "Argentina":1,"France":2,"Spain":3,"England":4,"Brazil":5,
        "Netherlands":6,"Portugal":7,"Belgium":8,"Germany":9,
        "Croatia":11,"Uruguay":12,"Switzerland":14,"Morocco":15,
        "Colombia":16,"Mexico":17,"Japan":18,"Senegal":19,"United States":20,
        "Sweden":21,"Iran":22,"South Korea":23,"Australia":24,"Austria":25,
        "Turkey":26,"Ecuador":27,"Egypt":29,"Ghana":41,"Norway":33,
        "Paraguay":32,"Canada":31,"Scotland":34,"Czech Republic":35,
        "South Africa":36,"Cape Verde":37,"Qatar":38,"Saudi Arabia":39,
        "Iraq":40,"Algeria":43,"Tunisia":44,"Ivory Coast":45,
        "DR Congo":50,"Uzbekistan":48,"Bosnia and Herzegovina":46,
        "Jordan":47,"New Zealand":49,"Panama":42,"Haiti":51,"Curaçao":52,
    }
    rank = fifa_rank or FIFA_RANK.get(team, 50)
    if rank <= 5: exp_gf = 2.00; exp_ga = 0.50
    elif rank <= 10: exp_gf = 1.70; exp_ga = 0.60
    elif rank <= 20: exp_gf = 1.40; exp_ga = 0.80
    elif rank <= 30: exp_gf = 1.10; exp_ga = 1.00
    elif rank <= 40: exp_gf = 0.90; exp_ga = 1.20
    else: exp_gf = 0.70; exp_ga = 1.40
    gf_m, ga_m, mp = GF_GA_DATA[team]
    conf = CONF_MAP.get(team, "uefa")
    conf_f = CONF_FACTOR.get(conf, 1.0)
    adj_gf = gf_m * conf_f
    adj_ga = ga_m * conf_f
    gf_diff = (adj_gf - exp_gf) * 4
    ga_diff = (exp_ga - adj_ga) * 4
    bonus = gf_diff + ga_diff
    return round(max(-3, min(3, bonus)))


def get_qualifying_rating(team):
    """
    基于预选赛真实数据返回(atk, dfn)评分 (40-95范围).
    用于base_score的攻防维度权重 (28%总权重).
    注意: 这个评分仅用于排位, 不影响xG公式.
    xG公式使用compute_ratings返回的旧atk/dfn值.
    """
    if team not in GF_GA_DATA:
        # 无数据时返回数据驱动评分
        conf = CONF_MAP.get(team, "uefa")
        if conf == "concacaf" and team in ["United States", "Mexico", "Canada"]:
            return 70, 75  # 东道主估算
        return 55, 60

    gf_m, ga_m, mp = GF_GA_DATA[team]
    conf = CONF_MAP.get(team, "uefa")
    conf_f = CONF_FACTOR.get(conf, 1.0)

    # 大洲归一化
    adj_gf = gf_m * conf_f
    adj_ga = ga_m * conf_f

    # 映射到40-95评分范围
    # qatk: 3.0 GF/M → 95, 0.5 GF/M → 48
    # qdfn: 0.0 GA/M → 95, 1.5 GA/M → 40
    qatk = 40 + (adj_gf / 3.0) * 55
    qdfn = 95 - (adj_ga / 1.5) * 55

    qatk = max(40, min(95, round(qatk)))
    qdfn = max(40, min(95, round(qdfn)))
    return qatk, qdfn


def print_all_ratings():
    """打印所有48队的真实攻防评分"""
    FIFA_2026 = {
        "Argentina":1,"France":2,"Spain":3,"England":4,"Brazil":5,
        "Netherlands":6,"Portugal":7,"Belgium":8,"Germany":9,
        "Croatia":11,"Uruguay":12,"Switzerland":14,"Morocco":15,
        "Colombia":16,"Mexico":17,"Japan":18,"Senegal":19,"United States":20,
        "Sweden":21,"Iran":22,"South Korea":23,"Australia":24,"Austria":25,
        "Turkey":26,"Ecuador":27,"Egypt":29,"Ghana":41,"Norway":33,
        "Paraguay":32,"Canada":31,"Scotland":34,"Czech Republic":35,
        "South Africa":36,"Cape Verde":37,"Qatar":38,"Saudi Arabia":39,
        "Iraq":40,"Algeria":43,"Tunisia":44,"Ivory Coast":45,
        "DR Congo":50,"Uzbekistan":48,"Bosnia and Herzegovina":46,
        "Jordan":47,"New Zealand":49,"Panama":42,"Haiti":51,"Curaçao":52,
    }
    print(f"{'Team':25s} {'GF/M':>6s} {'GA/M':>6s} {'Conf':>10s} {'AdjGF':>6s} {'AdjGA':>6s} {'NEWatk':>6s} {'NEWdfn':>6s}")
    print("-"*75)
    for team, rank in sorted(FIFA_2026.items(), key=lambda x: x[1]):
        atk, dfn = get_qualifying_rating(team)
        if team in GF_GA_DATA:
            gf_m, ga_m, mp = GF_GA_DATA[team]
            conf = CONF_MAP.get(team, "?")
            cf = CONF_FACTOR.get(conf, 1.0)
            adj_gf = gf_m * cf
            adj_ga = ga_m * cf
            print(f"{team:25s} {gf_m:6.2f} {ga_m:6.2f} {conf:>10s} {adj_gf:6.2f} {adj_ga:6.2f} {atk:6d} {dfn:6d}")
        else:
            print(f"{team:25s} {'N/A':>6s} {'N/A':>6s} {'?':>10s} {'N/A':>6s} {'N/A':>6s} {atk:6d} {dfn:6d}")

if __name__ == "__main__":
    print_all_ratings()
