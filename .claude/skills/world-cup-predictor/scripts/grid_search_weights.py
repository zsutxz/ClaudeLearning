#!/usr/bin/env python3
"""
v4.4 权重网格搜索
===============
自动测试不同8维权重组合对2022回测的影响.
每次测试500次MC, 按方向准确率+Brier Score+轮次误差评分.
"""
import math, random, os, sys, time
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from compute_ratings import compute_rating
from translations import TEAM_CN

# ======== 2022数据 ========
GROUPS_22 = {
    "A": ["Qatar","Ecuador","Senegal","Netherlands"],
    "B": ["England","Iran","USA","Wales"],
    "C": ["Argentina","Saudi Arabia","Mexico","Poland"],
    "D": ["France","Australia","Denmark","Tunisia"],
    "E": ["Spain","Costa Rica","Germany","Japan"],
    "F": ["Belgium","Canada","Morocco","Croatia"],
    "G": ["Brazil","Serbia","Switzerland","Cameroon"],
    "H": ["Portugal","Ghana","Uruguay","South Korea"],
}
ALL_T22 = [t for g in GROUPS_22 for t in GROUPS_22[g]]

FIFA_RANK_22 = {
    "Brazil":1,"Belgium":2,"Argentina":3,"France":4,"England":5,
    "Spain":7,"Netherlands":8,"Portugal":9,"Denmark":10,
    "Germany":11,"Croatia":12,"Mexico":13,"Uruguay":14,"Switzerland":15,
    "USA":16,"Senegal":18,"Wales":19,"Iran":20,"Japan":24,
    "Morocco":22,"Serbia":21,"Poland":26,"South Korea":28,
    "Tunisia":30,"Costa Rica":31,"Australia":38,"Canada":41,
    "Cameroon":43,"Ecuador":44,"Saudi Arabia":50,"Qatar":50,"Ghana":61,
}

# 实际晋级轮次: 7=冠军 6=亚军 5=四强 4=八强 3=16强 2=小组第3 1=小组第4
ACTUAL_22 = {
    "Argentina":7,"France":6,"Croatia":5,"Morocco":5,
    "Netherlands":4,"England":4,"Brazil":4,"Portugal":4,
    "USA":3,"Australia":3,"Japan":3,"South Korea":3,
    "Poland":3,"Senegal":3,"Switzerland":3,"Spain":3,
    "Qatar":1,"Ecuador":1,"Saudi Arabia":1,"Mexico":1,
    "Wales":1,"Iran":1,"Denmark":1,"Tunisia":1,
    "Costa Rica":1,"Germany":1,"Belgium":1,"Canada":1,
    "Cameroon":1,"Serbia":1,"Ghana":1,"Uruguay":1,
}
ACTUAL_CHAMP = "Argentina"

# 实际小组赛每场比分 (用于方向准确率)
ACTUAL_SCORES_22 = {
    ("A","Qatar","Ecuador"):(0,2),("A","Senegal","Netherlands"):(0,2),
    ("A","Qatar","Senegal"):(1,3),("A","Netherlands","Ecuador"):(1,1),
    ("A","Ecuador","Senegal"):(1,2),("A","Netherlands","Qatar"):(2,0),
    ("B","England","Iran"):(6,2),("B","USA","Wales"):(1,1),
    ("B","Wales","Iran"):(0,2),("B","England","USA"):(0,0),
    ("B","Iran","USA"):(0,1),("B","Wales","England"):(0,3),
    ("C","Argentina","Saudi Arabia"):(1,2),("C","Mexico","Poland"):(0,0),
    ("C","Poland","Saudi Arabia"):(2,0),("C","Argentina","Mexico"):(2,0),
    ("C","Poland","Argentina"):(0,2),("C","Saudi Arabia","Mexico"):(1,2),
    ("D","Denmark","Tunisia"):(0,0),("D","France","Australia"):(4,1),
    ("D","Tunisia","Australia"):(0,1),("D","France","Denmark"):(2,1),
    ("D","Australia","Denmark"):(1,0),("D","Tunisia","France"):(1,0),
    ("E","Germany","Japan"):(1,2),("E","Spain","Costa Rica"):(7,0),
    ("E","Japan","Costa Rica"):(0,1),("E","Spain","Germany"):(1,1),
    ("E","Japan","Spain"):(2,1),("E","Costa Rica","Germany"):(2,4),
    ("F","Morocco","Croatia"):(0,0),("F","Belgium","Canada"):(1,0),
    ("F","Belgium","Morocco"):(0,2),("F","Croatia","Canada"):(4,1),
    ("F","Croatia","Belgium"):(0,0),("F","Canada","Morocco"):(1,2),
    ("G","Switzerland","Cameroon"):(1,0),("G","Brazil","Serbia"):(2,0),
    ("G","Cameroon","Serbia"):(3,3),("G","Brazil","Switzerland"):(1,0),
    ("G","Serbia","Switzerland"):(2,3),("G","Cameroon","Brazil"):(1,0),
    ("H","Uruguay","South Korea"):(0,0),("H","Portugal","Ghana"):(3,2),
    ("H","South Korea","Ghana"):(2,3),("H","Portugal","Uruguay"):(2,0),
    ("H","Ghana","Uruguay"):(0,2),("H","South Korea","Portugal"):(2,1),
}

# ======== 当前权重(基线) ========
BASE = {
    "state": 0.26, "fifa": 0.13, "history": 0.08, "core": 0.12,
    "atk": 0.08, "def": 0.16, "venue": 0.05, "tactics": 0.12,
}

# ======== 引擎 ========
def fifa_rank_score(rank):
    if rank <= 5: return 13 + (6-rank)*0.4
    elif rank <= 10: return 11 + (10-rank)*0.4
    elif rank <= 20: return 8 + (20-rank)*0.3
    elif rank <= 30: return 5 + (30-rank)*0.3
    elif rank <= 40: return 3 + (40-rank)*0.2
    else: return max(0, 3 - (rank-40)*0.3)

def make_team(name, data, weight):
    rf,wh,cp,atk,defn,ada,tac,inj = data
    fr = fifa_rank_score(FIFA_RANK_22.get(name,50))
    fr_raw = fr / 0.15
    bs = (rf * weight["state"] + fr_raw * weight["fifa"] + wh * weight["history"]
          + cp * weight["core"] + atk * weight["atk"] + defn * weight["def"]
          + ada * weight["venue"] + tac * weight["tactics"])
    return {"f":round(bs+inj,2),"a":atk,"d":defn}

def poisson(lmbda):
    if lmbda <= 0: return 0
    L = math.exp(-lmbda); k=0; p=1.0
    while p > L: k+=1; p *= random.random()
    return max(0, k-1)

def neg_binom(mean, p=0.65):
    if mean <= 0: return 0
    if mean < 0.3: return poisson(mean)
    r = mean * p / (1 - p)
    if r < 1: return poisson(mean)
    rate = random.gammavariate(r, (1-p)/p)
    return poisson(rate)

def xg(atk, deff, avg=1.70):
    return avg * (atk/50) * ((100-deff)/50)

ROUND_AVG = {"group":1.70,"r32":1.40,"r16":1.30,"qf":1.20,"sf":1.10,"final":1.05}

def predict(t1n,t2n,a1,d1,a2,d2,ko=False,seed=None,rt="group"):
    if seed is not None: random.seed(seed)
    avg = ROUND_AVG.get(rt, 1.35)
    g1 = max(0.15, xg(a1,d2,avg))
    g2 = max(0.15, xg(a2,d1,avg))
    g1 = neg_binom(g1, 0.65)
    g2 = neg_binom(g2, 0.65)
    if g1 > g2: return "t1",g1,g2
    if g2 > g1: return "t2",g1,g2
    if ko:
        random.seed((seed or 0)+999)
        if random.random() < 0.55: g1 += 1
        else: g2 += 1
        if g1 == g2:
            random.seed((seed or 0)+1000)
            if random.random() < 0.52: g1 += 1
            else: g2 += 1
        return ("t1" if g1>g2 else "t2"), g1, g2
    return "draw",g1,g2

# ======== 运行一次回测, 返回评分 ========
def run_backtest(weight, N_SIM=500):
    """用指定权重跑2022回测, 返回 (方向准确率, Brier, 平均轮次误差)"""
    TEAMS = {}
    for n in ALL_T22:
        td = compute_rating(n, FIFA_RANK_22.get(n,50), 2022)
        TEAMS[n] = make_team(n, td, weight)

    champ_count = {t:0 for t in TEAMS}
    adv_count = {t:0 for t in TEAMS}
    match_stats = {}  # (g,t1,t2) -> stats

    for sim in range(N_SIM):
        bs = 100 + sim * 37
        random.seed(bs)
        advancing = []
        all_third = []

        for gi, g in enumerate("ABCDEFGH"):
            gteams = GROUPS_22[g]
            tbl = {t:{"pts":0,"gd":0,"gf":0} for t in gteams}
            matches = [(gteams[i],gteams[j]) for i in range(4) for j in range(i+1,4)]

            for mi, (t1,t2) in enumerate(matches):
                td1,td2 = TEAMS[t1],TEAMS[t2]
                r,g1,g2 = predict(t1,t2,td1["a"],td1["d"],td2["a"],td2["d"],
                                  seed=bs+gi*100+mi*7,rt="group")
                tbl[t1]["gf"]+=g1;tbl[t2]["gf"]+=g2
                tbl[t1]["gd"]+=g1-g2;tbl[t2]["gd"]+=g2-g1
                if r=="t1":tbl[t1]["pts"]+=3
                elif r=="t2":tbl[t2]["pts"]+=3
                else:tbl[t1]["pts"]+=1;tbl[t2]["pts"]+=1

                key = (g,t1,t2)
                if key not in match_stats:
                    match_stats[key] = {"t1_w":0,"d":0,"t2_w":0,"goals":{}}
                if r=="t1":match_stats[key]["t1_w"]+=1
                elif r=="t2":match_stats[key]["t2_w"]+=1
                else:match_stats[key]["d"]+=1
                sk=f"{g1}:{g2}"
                match_stats[key]["goals"][sk]=match_stats[key]["goals"].get(sk,0)+1

            st = sorted(tbl.items(),key=lambda x:(x[1]["pts"],x[1]["gd"],x[1]["gf"]),reverse=True)
            advancing.append(st[0][0])
            advancing.append(st[1][0])
            all_third.append((st[2][0],st[2][1]["pts"],st[2][1]["gd"]))

        all_third.sort(key=lambda x:(x[1],x[2]),reverse=True)
        for t,_,_ in all_third[:8]:
            advancing.append(t)

        for t in advancing:
            adv_count[t]=adv_count.get(t,0)+1

        # 简化淘汰赛: 前16按评分配对, 4轮单淘 (2022: R16→QF→SF→Final)
        seeded = sorted(advancing[:16], key=lambda t: TEAMS[t]["f"], reverse=True)
        cur = [(seeded[i],seeded[15-i]) for i in range(8)]
        rd_names_22 = ["r16","qf","sf","final"]
        for rd in range(4):
            nxt = []
            for mi,(t1,t2) in enumerate(cur):
                td1,td2=TEAMS[t1],TEAMS[t2]
                r,g1,g2=predict(t1,t2,td1["a"],td1["d"],td2["a"],td2["d"],
                                ko=True,seed=bs+10000+rd*1000+mi*13,
                                rt=rd_names_22[rd])
                w=t1 if r=="t1" else t2
                nxt.append(w)
                if rd==3:  # Final
                    champ_count[w]=champ_count.get(w,0)+1
            if rd<3:  # 还有下一轮
                cur=[(nxt[i],nxt[i+1]) for i in range(0,len(nxt),2)]

    # ===== 计算评分 =====
    # 1. 方向准确率: 48场小组赛
    dir_correct = 0
    total_matches = 0
    for g in "ABCDEFGH":
        gteams = GROUPS_22[g]
        matches = [(gteams[i],gteams[j]) for i in range(4) for j in range(i+1,4)]
        for t1,t2 in matches:
            key = (g,t1,t2)
            st = match_stats.get(key, {"t1_w":0,"d":0,"t2_w":0,"goals":{"0:0":N_SIM}})
            pred_w = "t1" if st["t1_w"] > st["t2_w"] else "t2" if st["t2_w"] > st["t1_w"] else "d"
            actual_s = ACTUAL_SCORES_22.get((g,t1,t2), (0,0))
            actual_w = "t1" if actual_s[0]>actual_s[1] else "t2" if actual_s[1]>actual_s[0] else "d"
            if pred_w == actual_w:
                dir_correct += 1
            total_matches += 1

    dir_acc = dir_correct / total_matches * 100

    # 2. Brier Score (冠军)
    brier = sum((champ_count.get(t,0)/N_SIM - (1 if t==ACTUAL_CHAMP else 0))**2 for t in TEAMS) / len(TEAMS)

    # 3. 平均轮次误差
    round_err = 0
    for team in TEAMS:
        cp = champ_count.get(team,0)/N_SIM
        ap = adv_count.get(team,0)/N_SIM
        if ap < 0.3: pred_r = 1
        elif ap < 0.5: pred_r = 2
        elif cp > 0.2: pred_r = 7
        elif cp > 0.1: pred_r = 6
        elif cp > 0.05: pred_r = 5
        elif cp > 0.025: pred_r = 4
        else: pred_r = 3
        round_err += abs(pred_r - ACTUAL_22.get(team, 1))
    avg_round_err = round_err / len(TEAMS)

    # 4. 冠军在前3?
    top3 = [t for t,_ in sorted(champ_count.items(),key=lambda x:x[1],reverse=True)[:3]]
    champ_in_top3 = 1 if ACTUAL_CHAMP in top3 else 0

    # 5. 八强命中
    qf_actual = ["Argentina","Netherlands","England","France","Brazil","Croatia","Morocco","Portugal"]
    top8_pred = [t for t,_ in sorted(champ_count.items(),key=lambda x:x[1],reverse=True)[:8]]
    qf_hits = sum(1 for t in top8_pred if t in qf_actual)

    return dir_acc, brier, avg_round_err, champ_in_top3, qf_hits

# ======== 搜索策略 ========
def generate_weight_candidates():
    """生成待测试的权重组合.
    策略: 从基线出发, 每个关键维度变化±0.02, 保持总和=1.0.
    """
    candidates = []
    base = dict(BASE)

    # 1. 基线
    candidates.append(("基线(v4.3)", dict(base)))

    # 2. 状态vs历史: state↑ history↓ / state↓ history↑
    for delta in [0.02, 0.04, -0.02, -0.04]:
        w = dict(base)
        w["state"] += delta
        w["history"] -= delta
        candidates.append((f"状态{delta:+.0%}/历史{delta:+.0%}", w))

    # 3. 防守vs战术: def↑ tactics↓ / def↓ tactics↑
    for delta in [0.02, 0.04, -0.02, -0.04]:
        w = dict(base)
        w["def"] += delta
        w["tactics"] -= delta
        candidates.append((f"防守{delta:+.0%}/战术{delta:+.0%}", w))

    # 4. 排名vs进攻: fifa↑ atk↓ / fifa↓ atk↑
    for delta in [0.02, -0.02]:
        w = dict(base)
        w["fifa"] += delta
        w["atk"] -= delta
        candidates.append((f"排名{delta:+.0%}/进攻{delta:+.0%}", w))

    # 5. 核心vs场地: core↑ venue↓ / core↓ venue↑
    for delta in [0.02, -0.02]:
        w = dict(base)
        w["core"] += delta
        w["venue"] -= delta
        candidates.append((f"核心{delta:+.0%}/场地{delta:+.0%}", w))

    # 6. 联合调整: 防守↑+战术↑ / 状态↓+历史↓ (从排名/进攻/场地挪)
    w = dict(base)
    w["def"] += 0.02; w["tactics"] += 0.02
    w["fifa"] -= 0.02; w["atk"] -= 0.02
    candidates.append(("防守+战术↑↑/排名+进攻↓↓", w))

    w = dict(base)
    w["state"] += 0.02; w["def"] += 0.02
    w["history"] -= 0.02; w["venue"] -= 0.02
    candidates.append(("状态+防守↑↑/历史+场地↓↓", w))

    w = dict(base)
    w["history"] += 0.02; w["tactics"] += 0.02
    w["state"] -= 0.02; w["core"] -= 0.02
    candidates.append(("历史+战术↑↑/状态+核心↓↓", w))

    # 7. 激进防守 (攻防倾斜)
    w = dict(base)
    w["def"] += 0.04; w["tactics"] += 0.02
    w["atk"] -= 0.03; w["fifa"] -= 0.03
    candidates.append(("防守+战术(def=20% tac=14%)", w))

    w = dict(base)
    w["def"] += 0.04; w["state"] += 0.02
    w["history"] -= 0.04; w["fifa"] -= 0.02
    candidates.append(("激进防守: def=20% history=4%", w))

    w = dict(base)
    w["history"] -= 0.03; w["def"] += 0.02; w["tactics"] += 0.02
    w["fifa"] -= 0.01
    candidates.append(("去历史化: hist=5% def=18% tac=14%", w))

    w = dict(base)
    w["state"] += 0.02; w["def"] += 0.02; w["tactics"] += 0.02
    w["history"] -= 0.04; w["venue"] -= 0.02
    candidates.append(("状态+防守: state=28% def=18% hist=4%", w))

    w = dict(base)
    w["def"] += 0.04; w["tactics"] += 0.02
    w["history"] -= 0.04; w["core"] -= 0.02
    candidates.append(("极限防守: def=20% tac=14% hist=4%", w))

    # 8. 激进状态
    w = dict(base)
    w["state"] += 0.04; w["def"] += 0.02
    w["history"] -= 0.03; w["venue"] -= 0.03
    candidates.append(("状态至上(state=26%)", w))

    # 9. 平衡型
    w = dict(base)
    for k in w: w[k] = 0.125
    candidates.append(("均匀(各12.5%)", w))

    # 10. 防守+排名 (减少历史声望噪音)
    w = dict(base)
    w["def"] += 0.02; w["fifa"] += 0.02
    w["history"] -= 0.02; w["core"] -= 0.02
    candidates.append(("防守+排名↑/历史+核心↓", w))

    return candidates

# ======== 主函数 ========
def main():
    print("=" * 70)
    print("  v4.4 权重网格搜索 (Grid Search)")
    print("=" * 70)
    print(f"\n  基线权重:")
    for k, v in BASE.items():
        print(f"    {k:>8} = {v:.0%}")
    print(f"\n  总候选组合: ? 组")
    print(f"  每次迭代: 500 次MC")
    print()

    candidates = generate_weight_candidates()
    print(f"  实际候选: {len(candidates)} 组")
    print()

    results = []
    start_time = time.time()

    for i, (name, weight) in enumerate(candidates, 1):
        # 校验总和
        total = sum(weight.values())
        if abs(total - 1.0) > 0.001:
            print(f"  ⚠️ [{i}/{len(candidates)}] {name} — 权重和={total:.3f}!=1.0, 跳过")
            continue

        t0 = time.time()
        dir_acc, brier, round_err, champ_top3, qf_hits = run_backtest(weight, 500)
        elapsed = time.time() - t0

        score = dir_acc - brier * 100 - round_err * 0.5 + champ_top3 * 2 + qf_hits * 0.5

        results.append((score, dir_acc, brier, round_err, champ_top3, qf_hits, name, weight))
        
        bar = "█" * int(dir_acc / 3) + "░" * max(0, 20 - int(dir_acc / 3))
        print(f"  [{i:>2}/{len(candidates)}] {name:<26} "
              f"方向:{dir_acc:>5.1f}% Brier:{brier:.4f} 轮差:{round_err:.2f} "
              f"八强:{qf_hits}/8 {bar}")
        print(f"      权重: {', '.join(f'{k}={v:.0%}' for k,v in sorted(weight.items()))}")
        print(f"      耗时: {elapsed:.1f}s")

    total_time = time.time() - start_time

    # ===== 结果排序 =====
    results.sort(key=lambda x: x[0], reverse=True)

    print(f"\n{'='*70}")
    print(f"  🏆 Top 5 最优权重组合 (总分={results[0][0]:.2f})")
    print(f"{'='*70}")
    print(f"  {'#':<3} {'名称':<26} {'方向':<7} {'Brier':<8} {'轮差':<6} {'八强':<6} {'冠军Top3':<9}")
    print(f"  {'-'*66}")
    for i, (score, da, br, re, ct3, qf, name, w) in enumerate(results[:5], 1):
        print(f"  {i:<3} {name:<26} {da:<6.1f}% {br:<7.4f} {re:<5.2f} {qf}/8   {'✅' if ct3 else '❌'}")

    print(f"\n  📊 基线对比:")
    base_result = [r for r in results if r[6] == "基线(v4.3)"]
    if base_result:
        _, da_b, br_b, re_b, ct3_b, qf_b, _, _ = base_result[0]
        best = results[0]
        print(f"     基线: 方向{da_b:.1f}% Brier:{br_b:.4f} 轮差{re_b:.2f}")
        print(f"     最优: 方向{best[1]:.1f}% Brier:{best[2]:.4f} 轮差{best[3]:.2f}")
        print(f"     改进: 方向{best[1]-da_b:+.1f}% Brier{best[2]-br_b:+.4f} 轮差{best[3]-re_b:+.2f}")

    print(f"\n  总耗时: {total_time:.0f}s")

    # ===== 输出最佳权重 =====
    if results:
        _, _, _, _, _, _, best_name, best_w = results[0]
        print(f"\n{'='*70}")
        print(f"  最佳: {best_name}")
        print(f"{'='*70}")
        # 输出可直接复制到full_prediction.py的格式
        print(f"\n  # make_team() 中的权重行:")
        keys = ["state","fifa","history","core","atk","def","venue","tactics"]
        parts = [f"{best_w[k]*100:.0f}%" for k in keys]
        print(f"  rf*{best_w['state']:.2f} + fr_raw*{best_w['fifa']:.2f} + wh*{best_w['history']:.2f}")
        print(f"  + cp*{best_w['core']:.2f} + atk*{best_w['atk']:.2f} + defn*{best_w['def']:.2f}")
        print(f"  + ada*{best_w['venue']:.2f} + tac*{best_w['tactics']:.2f}")
        print(f"  # 权重: [{', '.join(parts)}]")

if __name__ == "__main__":
    main()
