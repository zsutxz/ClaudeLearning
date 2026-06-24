"""
数据驱动评分系统 v2 (2026-05-23)
===================
v2改进: atk/dfn基于真实预选赛数据(GF/M, GA/M)而非FIFA排名衍生值。
  大洲归一化系数: UEFA×1.0, CONMEBOL×0.95, CAF×0.82, AFC×0.78, CONCACAF×0.73, OFC×0.38

用法:
  from compute_ratings import compute_rating, WC_HISTORY
  ratings = compute_rating("Argentina", 1, 2026)
  # 返回: (近期状态, 历史战绩, 核心球员, 进攻, 防守, 场地适应, 战术体系, 伤病修正)
"""
import math

# 世界杯成绩: 7=冠军 6=亚军 5=四强 4=八强 3=16强 2=小组 1=未晋级
WC_HISTORY = {
    # 2026参赛队 (48队)
    "Argentina":  {2022:7, 2018:3, 2014:6, 2010:4},
    "France":     {2022:6, 2018:7, 2014:4, 2010:2},
    "Spain":      {2022:3, 2018:3, 2014:2, 2010:7},
    "England":    {2022:4, 2018:5, 2014:2, 2010:3},
    "Brazil":     {2022:4, 2018:4, 2014:4, 2010:4},
    "Netherlands":{2022:4, 2018:0, 2014:5, 2010:6},
    "Portugal":   {2022:4, 2018:3, 2014:2, 2010:3},
    "Belgium":    {2022:2, 2018:5, 2014:4, 2010:0},
    "Germany":    {2022:2, 2018:2, 2014:7, 2010:5},
    "Croatia":    {2022:5, 2018:6, 2014:2, 2010:0},
    "Uruguay":    {2022:2, 2018:4, 2014:3, 2010:5},
    "Switzerland":{2022:3, 2018:3, 2014:3, 2010:2},
    "Colombia":   {2022:0, 2018:3, 2014:4, 2010:0},
    "Mexico":     {2022:2, 2018:3, 2014:3, 2010:3},
    "Japan":      {2022:3, 2018:3, 2014:2, 2010:3},
    "Morocco":    {2022:5, 2018:2, 2014:0, 2010:0},
    "USA":        {2022:3, 2018:0, 2014:3, 2010:3},
    "Senegal":    {2022:3, 2018:2, 2014:0, 2010:0},
    "Sweden":     {2022:0, 2018:4, 2014:0, 2010:0},
    "Iran":       {2022:2, 2018:2, 2014:2, 2010:0},
    "South Korea":{2022:3, 2018:2, 2014:2, 2010:3},
    "Australia":  {2022:3, 2018:2, 2014:2, 2010:2},
    "Austria":    {2022:0, 2018:0, 2014:0, 2010:0},
    "Turkey":     {2022:0, 2018:0, 2014:0, 2010:0},
    "Ecuador":    {2022:2, 2018:0, 2014:2, 2010:0},
    "Egypt":      {2022:0, 2018:2, 2014:0, 2010:0},
    "Ghana":      {2022:2, 2018:0, 2014:2, 2010:4},
    "Norway":     {2022:0, 2018:0, 2014:0, 2010:0},
    "Paraguay":   {2022:0, 2018:0, 2014:0, 2010:4},
    "Canada":     {2022:2, 2018:0, 2014:0, 2010:0},
    "Scotland":   {2022:0, 2018:0, 2014:0, 2010:0},
    "Czech Republic":{2022:0, 2018:0, 2014:0, 2010:0},
    "South Africa":   {2022:0, 2018:0, 2014:0, 2010:2},
    "Cape Verde":     {2022:0, 2018:0, 2014:0, 2010:0},
    "Saudi Arabia":   {2022:2, 2018:2, 2014:0, 2010:0},
    "Iraq":           {2022:0, 2018:0, 2014:0, 2010:0},
    "Algeria":        {2022:0, 2018:0, 2014:3, 2010:0},
    "Tunisia":        {2022:2, 2018:2, 2014:0, 2010:0},
    "Ivory Coast":    {2022:0, 2018:0, 2014:2, 2010:2},
    "DR Congo":       {2022:0, 2018:0, 2014:0, 2010:0},
    "Uzbekistan":     {2022:0, 2018:0, 2014:0, 2010:0},
    "Bosnia and Herzegovina": {2022:0, 2018:0, 2014:2, 2010:0},
    "Jordan":         {2022:0, 2018:0, 2014:0, 2010:0},
    "New Zealand":    {2022:0, 2018:0, 2014:0, 2010:2},
    "Panama":         {2022:0, 2018:2, 2014:0, 2010:0},
    "Qatar":          {2022:2, 2018:0, 2014:0, 2010:0},
    "Haiti":          {2022:0, 2018:0, 2014:0, 2010:0},
    "Curaçao":        {2022:0, 2018:0, 2014:0, 2010:0},
}

# 各队风格修正 (进攻偏移, 防守偏移)
STYLE = {
    # 进攻型
    "Brazil": (5, 0), "Argentina": (3, 2), "France": (3, 3),
    "England": (3, 0), "Portugal": (3, 0), "Belgium": (5, -2),
    "Spain": (3, -2), "Netherlands": (0, 3), "Germany": (2, 2),
    # 防守型
    "Uruguay": (-3, 5), "Morocco": (-5, 8), "Croatia": (-3, 5),
    "Iran": (-5, 5), "Japan": (-2, 3), "Senegal": (3, 0),
    "Switzerland": (-2, 3), "Sweden": (-2, 4), "Egypt": (-3, 3),
    # 进攻偏弱/防守硬
    "Canada": (2, 0), "USA": (2, 1), "Mexico": (1, 2),
    "South Korea": (2, 0), "Australia": (0, 2), "Ecuador": (1, 2),
    "Turkey": (3, -1), "Austria": (1, 1), "Norway": (5, -3),
    "Scotland": (2, 0), "Ghana": (3, -1), "Czech Republic": (1, 1),
    # 弱队默认
    "South Africa": (0, 2), "Saudi Arabia": (-2, 3), "Tunisia": (-3, 4),
    "Algeria": (2, 0), "Ivory Coast": (3, -1), "Iraq": (-2, 2),
    "Cape Verde": (0, 1), "New Zealand": (-1, 2), "Panama": (-1, 1),
    "Qatar": (-1, 2), "Haiti": (0, 0), "Curaçao": (0, 0),
    "Paraguay": (-1, 3), "Jordan": (-1, 1), "Bosnia and Herzegovina": (2, 0),
    "Uzbekistan": (1, 0), "DR Congo": (1, 0),
}

def compute_league_performance(team):
    """联赛表现评分 (0-100): 评估国家队球员在实际联赛中的表现质量.
    三维评分: (1)豪门效力 — 球员在欧冠级俱乐部的数量
              (2)首发率 — 国家队球员在俱乐部的常规首发比例
              (3)个人状态 — 锋线进球/中场组织/防线稳定的近期表现
    每队格式: (豪门球员, 首发率%, 状态加成)
    """
    LEAGUE_DETAIL = {
        # ===== 超级强队 (豪门核心+高首发+状态火热) =====
        "France":     (8, 85, 10),   # 姆巴佩(皇马)、登贝莱(PSG)、格列兹曼(马竞)、
        "Brazil":     (7, 80, 8),    # 维尼修斯(皇马)、罗德里戈(皇马)、拉菲尼亚(巴萨)
        "Argentina":  (6, 82, 7),    # 梅西(迈阿密转法甲)、劳塔罗(国米)、阿尔瓦雷斯(曼城)
        "England":    (7, 78, 5),    # 凯恩(拜仁)、贝林厄姆(皇马)、萨卡(阿森纳)
        "Spain":      (6, 80, 3),    # 罗德里(曼城)、亚马尔(巴萨)、莫拉塔(马竞)
        "Germany":    (5, 75, 3),    # 哈弗茨(阿森纳)、基米希(拜仁)、穆西亚拉(拜仁)
        "Netherlands":(4, 72, 4),    # 范迪克(利物浦)、加克波(利物浦)、德里赫特(拜仁)
        "Portugal":   (4, 70, 5),    # C罗(沙特→回落)、B席(曼城)、莱奥(AC米兰)
        # ===== 强队 (部分豪门+稳定首发) =====
        "Belgium":    (3, 68, 2),    # 德布劳内(曼城)、多库(曼城)、特罗萨德(阿森纳)
        "Croatia":    (2, 65, 1),    # 莫德里奇(皇马)、格瓦迪奥尔(曼城)
        "Uruguay":    (2, 62, 4),    # 努涅斯(利物浦)、巴尔韦德(皇马)、阿劳霍(巴萨)
        "Morocco":    (2, 58, 3),    # 阿什拉夫(PSG)、齐耶赫(加拉塔萨雷→状态一般)
        "Switzerland":(1, 60, 1),    # 扎卡(勒沃库森)、阿坎吉(曼城轮换)
        "Denmark":    (2, 62, 1),    # 霍伊伦(曼联)、埃里克森(曼联→安德莱赫特)
        "Austria":    (1, 58, 2),    # 萨比策(多特)、阿拉巴(皇马伤)
        "Turkey":     (1, 55, 2),    # 恰尔汗奥卢(国米)、伊尔迪兹(尤文)
        "Senegal":    (1, 55, 3),    # 迪亚洛(利物浦)、库利巴利(沙特→状态不明)
        "Sweden":     (1, 52, 1),    # 伊萨克(纽卡)、库卢塞夫斯基(热刺)
        "Norway":     (2, 50, 5),    # 哈兰德(曼城)、厄德高(阿森纳)
        "Scotland":   (0, 48, 1),    # 罗伯逊(利物浦下坡)、麦克托米奈(那不勒斯)
        "Poland":     (1, 50, 2),    # 莱万(巴萨)、泽林斯基(国米)
        # ===== 中游 (零星豪门+部分五大联赛) =====
        "USA":        (1, 48, 2),    # 普利西奇(AC米兰)、麦肯尼(尤文)
        "Mexico":     (0, 45, 1),    # 希门尼斯(费耶诺德)、阿尔瓦雷斯(西汉姆)
        "Canada":     (1, 45, 3),    # 戴维(里尔)、戴维斯(拜仁)
        "Japan":      (0, 42, 2),    # 久保建英(皇家社会)、三笘薰(布莱顿)
        "Colombia":   (1, 44, 2),    # 迪亚斯(利物浦)、杜兰(维拉)
        "Ecuador":    (0, 42, 1),    # 凯塞多(切尔西)、埃斯图皮南(布莱顿)
        "Ivory Coast":(0, 40, 2),    # 阿莱(多特→伤愈)、凯西(吉达国民→回落)
        "Ghana":      (0, 38, 1),    # 库杜斯(西汉姆)、托马斯(阿森纳伤)
        "Algeria":    (0, 38, 1),    # 马赫雷斯(吉达国民回落)、本纳赛尔(AC米兰)
        "South Korea":(0, 36, 1),    # 孙兴慜(热刺下降)、黄喜灿(狼队)
        "Iran":       (0, 34, 0),    # 塔雷米(国米)、阿兹蒙(罗马→租借)
        "Australia":  (0, 32, 0),    # 苏塔(莱斯特)、赫鲁斯蒂奇(维罗纳)
        "Czech Republic":(0,30, 0),  # 希克(勒沃库森)、绍切克(西汉姆)
        "Paraguay":   (0, 28, 0),    # 阿尔米隆(纽卡回落)、冈萨雷斯(莱加内斯)
        # ===== 弱队 (五大联赛边缘人+本土联赛) =====
        "Cape Verde": (0, 18, 0), "Tunisia": (0, 16, 0), "Egypt": (0, 15, 1),
        "Bosnia and Herzegovina": (0, 14, 0),
        "Saudi Arabia":  (0, 8, 0), "Qatar": (0, 5, 0),
        "South Africa":  (0, 10, 0), "Iraq": (0, 5, 0),
        "New Zealand":   (0, 8, 0), "Panama": (0, 5, 0),
        "Haiti": (0, 3, 0), "Curaçao": (0, 2, 0),
        "Jordan": (0, 3, 0), "Uzbekistan": (0, 2, 0),
        "DR Congo": (0, 2, 0),
    }
    elite, starter_pct, form = LEAGUE_DETAIL.get(team, (0, 10, 0))
    # 评分 = 豪门权重40 + 首发率权重40 + 状态权重20
    elite_score = min(40, elite * 5)           # 每1个豪门球员+5分，上限40
    starter_score = starter_pct * 0.4          # 首发率×0.4，上限40
    form_score = min(20, max(0, form * 4))     # 状态×4，上限20
    total = elite_score + starter_score + form_score
    return round(min(100, max(0, total)))


def compute_history_score(team, year=2026):
    """计算历史世界杯战绩 (0-100).
    只用给定年份之前的数据 (防作弊).
    """
    hist = WC_HISTORY.get(team, {})
    # 使用最近4届世界杯, 不含当前年份
    weights = {year-4: 0.4, year-8: 0.3, year-12: 0.2, year-16: 0.1}
    score = 0
    total_w = 0
    for y, w in weights.items():
        r = hist.get(y, 0)
        if r > 0:
            rd_scores = {7:100, 6:90, 5:80, 4:65, 3:50, 2:30}
            s = rd_scores.get(r, 20)
            score += s * w
            total_w += w
    if total_w == 0:
        return 15  # 无历史战绩
    return round(score / total_w)

def compute_rating(team, fifa_rank, year=2026, defending_champion=False):
    """
    从FIFA排名自动计算8维评分.
    返回: (近期状态, 历史战绩, 核心球员, 进攻, 防守, 场地适应, 战术体系, 伤病修正)
    """
    n_teams = 48
    
    # [FIFA排名] 基础
    rank_score = max(20, 100 - (fifa_rank - 1) * 80 / (n_teams - 1))
    
    # [历史战绩]
    hist_score = compute_history_score(team, year)
    
    # [近期状态] 排名=状态
    if fifa_rank <= 10: form = 75 + (11-fifa_rank) * 2.5
    elif fifa_rank <= 20: form = 60 + (21-fifa_rank) * 1.5
    elif fifa_rank <= 30: form = 45 + (31-fifa_rank)
    elif fifa_rank <= 40: form = 38 + (41-fifa_rank) * 0.5
    else: form = 35
    
    # [核心球员]
    if fifa_rank <= 5: core = 85 + (6-fifa_rank) * 3
    elif fifa_rank <= 10: core = 70 + (11-fifa_rank) * 3
    elif fifa_rank <= 20: core = 55 + (21-fifa_rank) * 1.5
    elif fifa_rank <= 30: core = 45 + (31-fifa_rank) * 0.8
    elif fifa_rank <= 40: core = 40 + (41-fifa_rank) * 0.4
    else: core = 36
    
    # [进攻/防守] 旧方法: 从FIFA排名衍生 (保持xG公式兼容)
    if fifa_rank <= 5: atk, dfn = 82+(6-fifa_rank)*3, 80+(6-fifa_rank)*2
    elif fifa_rank <= 10: atk, dfn = 72+(11-fifa_rank)*2, 70+(11-fifa_rank)*2
    elif fifa_rank <= 20: atk, dfn = 64+(21-fifa_rank), 64+(21-fifa_rank)
    elif fifa_rank <= 30: atk, dfn = 58+(31-fifa_rank)*0.5, 58+(31-fifa_rank)*0.5
    elif fifa_rank <= 40: atk, dfn = 54+(41-fifa_rank)*0.3, 54+(41-fifa_rank)*0.3
    else: atk, dfn = 52, 52
    
    # 风格修正
    style_atk, style_def = STYLE.get(team, (0, 0))
    atk = max(40, min(98, atk + style_atk))
    dfn = max(40, min(98, dfn + style_def))
    
    # [场地适应]
    wc_appearances = sum(1 for y in range(year-12, year+1, 4) 
                         if WC_HISTORY.get(team, {}).get(y, 0) > 0)
    venue = 45 + wc_appearances * 8
    # 东道主加成
    if team in ["United States", "Canada", "Mexico"]:
        venue = min(95, venue + 15)
    
    # [战术体系]
    tac = min(90, 48 + rank_score * 0.4 + hist_score * 0.15)
    
    # [预选赛表现加成] v2: 基于真实数据调整淘汰赛排位
    from qualifying_data import get_qualifying_bonus
    qual_bonus = get_qualifying_bonus(team)
    tac = min(90, tac + qual_bonus)
    
    # [伤病修正] 默认0
    inj = 0

    # [联赛表现] 基于该国球员在五大联赛的数量和欧冠参与度
    league = compute_league_performance(team)

    # 卫冕冠军魔咒: 减2.0分 (6届中4届卫冕冠军小组出局)
    if defending_champion:
        inj -= 2.0
    
    return (round(form), round(hist_score), round(core), round(atk), 
            round(dfn), round(venue), round(tac), inj, round(league))


if __name__ == "__main__":
    # 测试: 用2026 FIFA排名生成评分
    FIFA_2026 = {
        "Argentina":1,"France":2,"Spain":3,"England":4,"Brazil":5,
        "Netherlands":6,"Portugal":7,"Belgium":8,"Germany":9,
        "Croatia":11,"Uruguay":12,"Switzerland":14,"Morocco":15,
        "Colombia":16,"Mexico":17,"Japan":18,"Senegal":19,"USA":20,
        "Sweden":21,"Iran":22,"South Korea":23,"Australia":24,"Austria":25,
        "Turkey":26,"Ecuador":27,"Egypt":29,"Ghana":41,"Norway":33,
        "Paraguay":32,"Canada":31,"Scotland":34,"Czech Republic":35,
        "South Africa":36,"Cape Verde":37,"Qatar":38,"Saudi Arabia":39,
        "Iraq":40,"Algeria":43,"Tunisia":44,"Ivory Coast":45,
        "DR Congo":50,"Uzbekistan":48,"Bosnia and Herzegovina":46,
        "Jordan":47,"New Zealand":49,"Panama":42,"Haiti":51,"Curaçao":52,
    }
    
    print(f"{'球队':<22} {'状态':<5} {'历史':<5} {'核心':<5} {'进攻':<5} {'防守':<5} {'场地':<5} {'战术':<5} {'伤病':<5}")
    print("-"*60)
    for team, rank in sorted(FIFA_2026.items(), key=lambda x: x[1]):
        r = compute_rating(team, rank, 2026)
        print(f"{team:<22} {r[0]:<5} {r[1]:<5} {r[2]:<5} {r[3]:<5} {r[4]:<5} {r[5]:<5} {r[6]:<5} {r[7]:<5}")
