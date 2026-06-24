# 2026 World Cup Scoring Engine
# Python implementation of the 8-dimension scoring + Monte Carlo simulation
# Copy this as starter code and modify team scores per session

import math
import random

# === FIXED: FIFA RANK → SCORE MAPPING ===
# 根据FIFA排名自动计算 ranking 维度的得分（满分15分）
def fifa_rank_score(rank):
    """Convert FIFA rank (1-48+) to score (0-15)"""
    if rank <= 5: return 13 + (6 - rank) * 0.4  # 1→15, 5→13.4
    elif rank <= 10: return 11 + (10 - rank) * 0.4  # 6→12.6, 10→11
    elif rank <= 20: return 8 + (20 - rank) * 0.3  # 11→10.7, 20→8
    elif rank <= 30: return 5 + (30 - rank) * 0.3  # 21→7.7, 30→5
    elif rank <= 40: return 3 + (40 - rank) * 0.2  # 31→4.8, 40→3
    else: return max(0, 3 - (rank - 40) * 0.3)  # 41→2.7, 50→0

# === TEAM DATA STRUCTURE ===
# Each team rated on 8 dimensions (0-100), then weighted:

def init_team(name, rf, fr, wh, cp, atk, defn, ada, tac, inj=0, notes=""):
    """8-Dimension scoring:
    rf = recent_form (近期状态), fr = fifa_rank (FIFA排名)
    wh = wc_history (历史战绩), cp = core_players (核心球员)
    atk = attack (进攻), defn = defense (防守)
    ada = adaptation (场地适应), tac = tactics (战术体系)
    inj = injury adjustment (伤病修正, negative)
    """
    WEIGHTS = {
        "rf": 0.20, "fr": 0.15, "wh": 0.15, "cp": 0.12,
        "atk": 0.10, "defn": 0.10, "ada": 0.10, "tac": 0.08
    }
    raw = rf*0.20 + fr*0.15 + wh*0.15 + cp*0.12 + atk*0.10 + defn*0.10 + ada*0.10 + tac*0.08
    return {"rf": rf, "fr": fr, "wh": wh, "cp": cp,
            "atk": atk, "defn": defn, "ada": ada, "tac": tac,
            "raw": round(raw, 2), "inj": inj,
            "final": round(raw + inj, 2), "notes": notes}

# === MATCH PREDICTOR (泊松分布模型) ===
# 足球比分接近泊松分布，比线性公式更真实
def poisson_goals(lambda_val):
    """Generate goals using Poisson distribution approximation"""
    # Use Knuth's algorithm for Poisson
    L = math.exp(-lambda_val)
    k = 0
    p = 1.0
    while p > L:
        k += 1
        p *= random.random()
    return max(0, k - 1)

def expected_goals(attack_strength, defense_strength, avg_goals=1.2):
    """Expected goals = league average * attack/avg_attack * defense/avg_defense"""
    # attack_strength (0-100), defense_strength (0-100, higher=better defense means fewer goals)
    atk_factor = attack_strength / 50  # normalize: 50 = average
    def_factor = (100 - defense_strength) / 50  # invert: 50 defense = average
    return avg_goals * atk_factor * def_factor

def predict_match(t1_score, t2_score, t1_atk, t1_def, t2_atk, t2_def, 
                  is_knockout=False, seed=None):
    """Returns (winner, team1_goals, team2_goals)
    Uses Elo-style win probability + Poisson goal distribution.
    is_knockout=True reduces draw probability (extra time/penalties).
    """
    if seed is not None:
        random.seed(seed)
    
    # Poisson-based goal generation
    xg1 = expected_goals(t1_atk, t2_def)
    xg2 = expected_goals(t2_atk, t1_def)
    
    g1 = poisson_goals(max(0.2, xg1))
    g2 = poisson_goals(max(0.2, xg2))
    
    if g1 > g2:
        return "t1", g1, g2
    elif g2 > g1:
        return "t2", g1, g2
    else:
        # 平局：小组赛直接判平，淘汰赛才进加时/点球
        if not is_knockout:
            return "draw", g1, g2
        # 淘汰赛加时：按实力差追加 0-1 球
        diff_elo = (t1_score - t2_score) / 50
        extra_seed = seed + 999 if seed else None
        random.seed(extra_seed)
        extra_prob = 1 / (1 + pow(10, -diff_elo / 10))
        if random.random() < extra_prob:
            g1 += 1
        else:
            g2 += 1
        if g1 == g2:  # 仍平 → 点球，强队略占优
            random.seed(extra_seed + 1)
            if random.random() < 0.52:
                g1 += 1
            else:
                g2 += 1
        return ("t1" if g1 > g2 else "t2"), g1, g2

# === GROUP STAGE ===
def simulate_group(teams, team_data, seed=42):
    """Simulate a group of 4 teams, return sorted table + match details"""
    random.seed(seed)
    table = {t: {"pts": 0, "gd": 0, "gf": 0, "ga": 0,
                  "w": 0, "d": 0, "l": 0} for t in teams}
    
    matches = [(teams[i], teams[j]) for i in range(4) for j in range(i+1, 4)]
    match_details = []
    
    for idx, (t1, t2) in enumerate(matches):
        td1, td2 = team_data[t1], team_data[t2]
        result, g1, g2 = predict_match(td1["final"], td2["final"],
                                       td1["atk"], td1["defn"],
                                       td2["atk"], td2["defn"],
                                       seed=seed + idx)
        
        table[t1]["gf"] += g1; table[t1]["ga"] += g2
        table[t2]["gf"] += g2; table[t2]["ga"] += g1
        table[t1]["gd"] += g1 - g2; table[t2]["gd"] += g2 - g1
        match_details.append((t1, t2, g1, g2))
        
        if result == "draw":
            table[t1]["pts"] += 1; table[t2]["pts"] += 1
            table[t1]["d"] += 1; table[t2]["d"] += 1
        elif result == "t1":
            table[t1]["pts"] += 3; table[t1]["w"] += 1; table[t2]["l"] += 1
        else:
            table[t2]["pts"] += 3; table[t2]["w"] += 1; table[t1]["l"] += 1
    
    sorted_table = sorted(table.items(),
                          key=lambda x: (x[1]["pts"], x[1]["gd"], x[1]["gf"]),
                          reverse=True)
    return sorted_table, match_details

# === KNOCKOUT (SIMPLE SEEDED BRACKET) ===
def seeded_bracket(advancing_teams, team_data, seed=42):
    """
    Rank 32 advancing teams by score, pair 1v32, 16v17, 8v25, etc.
    Returns full bracket results.
    """
    seeded = sorted(advancing_teams, key=lambda t: team_data[t]["final"], reverse=True)
    r16 = [(seeded[i], seeded[31-i]) for i in range(16)]
    
    # Play rounds
    random.seed(seed)
    round_results = []
    for round_idx, pairs in enumerate([r16, None, None, None]):
        results = []
        for idx, (t1, t2) in enumerate(pairs):
            td1, td2 = team_data[t1], team_data[t2]
            result, g1, g2 = predict_match(td1["final"], td2["final"],
                                          td1["atk"], td1["defn"],
                                          td2["atk"], td2["defn"],
                                          seed=seed + round_idx * 100 + idx)
            winner = t1 if result == "t1" else t2 if result == "t2" else t1
            results.append((winner, t1, t2, g1, g2))
        round_results.append(results)
        if round_idx < 3:
            pairs = [(results[i][0], results[i+1][0]) for i in range(0, len(results), 2)]
    return round_results

# === MONTE CARLO VERIFICATION ===
def monte_carlo(groups, team_data, n_simulations=500):
    """Run group stage N times to get advancement probabilities."""
    advance_count = {t: 0 for t in team_data}
    winner_count = {t: 0 for t in team_data}
    
    for sim in range(n_simulations):
        base_seed = 42 + sim
        for g_letter, g_teams in groups.items():
            st, _ = simulate_group(g_teams, team_data, seed=base_seed * 10)
            advance_count[st[0][0]] += 1
            advance_count[st[1][0]] += 1
            winner_count[st[0][0]] += 1
    
    return {t: {"advance_pct": advance_count[t]/n_simulations*100,
                "win_pct": winner_count[t]/n_simulations*100}
            for t in team_data}
