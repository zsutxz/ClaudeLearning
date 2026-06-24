#!/usr/bin/env python3
"""
2022世界杯 — 小组赛预测验证
"""
import math, random, os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

# ======== 2022数据 ========
GROUPS = {
    "A":["Qatar","Ecuador","Senegal","Netherlands"],
    "B":["England","Iran","USA","Wales"],
    "C":["Argentina","Saudi Arabia","Mexico","Poland"],
    "D":["France","Australia","Denmark","Tunisia"],
    "E":["Spain","Costa Rica","Germany","Japan"],
    "F":["Belgium","Canada","Morocco","Croatia"],
    "G":["Brazil","Serbia","Switzerland","Cameroon"],
    "H":["Portugal","Ghana","Uruguay","South Korea"],
}
ALL_T = [t for g in GROUPS for t in GROUPS[g]]

FIFA_RANK = {
    "Brazil":1,"Belgium":2,"Argentina":3,"France":4,"England":5,
    "Spain":7,"Netherlands":8,"Portugal":9,"Denmark":10,
    "Germany":11,"Croatia":12,"Mexico":13,"Uruguay":14,"Switzerland":15,
    "USA":16,"Senegal":18,"Wales":19,"Iran":20,"Japan":24,
    "Morocco":22,"Serbia":21,"Poland":26,"South Korea":28,
    "Tunisia":30,"Costa Rica":31,"Australia":38,"Canada":41,
    "Cameroon":43,"Ecuador":44,"Saudi Arabia":50,"Qatar":50,"Ghana":61,
}

from compute_ratings import compute_rating, WC_HISTORY

# 实际比分: (group, t1, t2) -> (g1, g2)
ACTUAL_SCORES = {
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

# 30%容忍度: 预测比分与实际最多差30%算"接近"
def score_close(pred, actual, tolerance=0.3):
    """判断预测比分是否接近实际"""
    if pred == actual: return "exact"
    g1p, g2p = pred
    g1a, g2a = actual
    # 胜负方向一致
    pred_winner = "t1" if g1p>g2p else "t2" if g2p>g1p else "d"
    actual_winner = "t1" if g1a>g2a else "t2" if g2a>g1a else "d"
    if pred_winner != actual_winner: return "wrong"
    # 比分差在2球以内
    if abs(g1p-g1a) <= 1 and abs(g2p-g2a) <= 1: return "close"
    return "direction"

# ==================== ENGINE ====================
def frs(r):
    if r<=5: return 13+(6-r)*0.4
    elif r<=10: return 11+(10-r)*0.4
    elif r<=20: return 8+(20-r)*0.3
    elif r<=30: return 5+(30-r)*0.3
    elif r<=40: return 3+(40-r)*0.2
    else: return max(0,3-(r-40)*0.3)

def mk(name, d):
    rf,wh,cp,atk,defn,ada,tac,inj = d
    fr = frs(FIFA_RANK.get(name,50))
    frr = fr/0.15
    bs = rf*0.26 + frr*0.12 + wh*0.05 + cp*0.12 + atk*0.08 + defn*0.20 + ada*0.02 + tac*0.14
    return {"s":round(bs,2),"f":round(bs+inj,2),"a":atk,"d":defn}

def pois(l):
    if l<=0: return 0
    L=math.exp(-l);k=0;p=1.0
    while p>L: k+=1; p*=random.random()
    return max(0,k-1)
def nb(m,p=0.65):
    if m<=0: return 0
    if m<0.3: return pois(m)
    r=m*p/(1-p)
    if r<1: return pois(m)
    rate=random.gammavariate(r,(1-p)/p)
    return pois(rate)

def xg(a,d,avg=1.70): return avg*(a/50)*((100-d)/50)

RA={"g":1.70,"r16":1.40,"qf":1.20,"sf":1.10,"f":1.05}
def pr(_,__,a1,d1,a2,d2,ko=False,seed=None,rt="g"):
    if seed: random.seed(seed)
    avg=RA.get(rt,1.35)
    g1=nb(max(0.15,xg(a1,d2,avg)),0.7)
    g2=nb(max(0.15,xg(a2,d1,avg)),0.7)
    if g1>g2: return "t1",g1,g2
    if g2>g1: return "t2",g1,g2
    if ko:
        random.seed((seed or 0)+999)
        if random.random()<0.55: g1+=1
        else: g2+=1
        if g1==g2:
            random.seed((seed or 0)+1000)
            if random.random()<0.52: g1+=1
            else: g2+=1
        return ("t1" if g1>g2 else "t2"),g1,g2
    return "draw",g1,g2

def main():
    # 自动生成评分
    TEAMS = {}
    for n in ALL_T:
        td = compute_rating(n, FIFA_RANK.get(n,50), 2022)
        TEAMS[n] = mk(n, td)

    N=2000; match_stats = {}
    
    for sim in range(N):
        bs=100+sim*37; random.seed(bs)
        for gi,g in enumerate("ABCDEFGH"):
            gt=GROUPS[g]; tbl={t:{"p":0,"gd":0,"gf":0} for t in gt}
            for mi,(t1,t2) in enumerate([(gt[i],gt[j]) for i in range(4) for j in range(i+1,4)]):
                td1,td2=TEAMS[t1],TEAMS[t2]
                r,g1,g2=pr(0,0,td1["a"],td1["d"],td2["a"],td2["d"],seed=bs+gi*100+mi*7,rt="g")
                key=(g,t1,t2)
                if key not in match_stats:
                    match_stats[key]={"t1_w":0,"d":0,"t2_w":0,"goals":{}}
                if r=="t1": match_stats[key]["t1_w"]+=1
                elif r=="t2": match_stats[key]["t2_w"]+=1
                else: match_stats[key]["d"]+=1
                sk=f"{g1}:{g2}"
                match_stats[key]["goals"][sk]=match_stats[key]["goals"].get(sk,0)+1
    
    # ===== 输出验证 =====
    print("="*70)
    print("  2022世界杯 — 小组赛预测 vs 实际结果")
    print("="*70)
    
    total = 0
    exact = 0
    close = 0
    direction_correct = 0
    wrong_dir = 0
    
    for g in "ABCDEFGH":
        gt = GROUPS[g]
        matches = [(gt[i],gt[j]) for i in range(4) for j in range(i+1,4)]
        
        # 计算预期积分
        exp_pts = {t:0 for t in gt}
        for t1, t2 in matches:
            key = (g, t1, t2)
            st = match_stats.get(key, {"t1_w":0,"d":0,"t2_w":0,"goals":{}})
            t1_wp = st["t1_w"]/N
            dp = st["d"]/N
            t2_wp = st["t2_w"]/N
            exp_pts[t1] += t1_wp*3 + dp*1
            exp_pts[t2] += t2_wp*3 + dp*1
        
        ranked = sorted(gt, key=lambda t: -exp_pts[t])
        pred_adv = ranked[:2]  # 预测出线
        # 实际出线
        real_adv = {"A":["Netherlands","Senegal"],"B":["England","USA"],"C":["Argentina","Poland"],
                    "D":["France","Australia"],"E":["Japan","Spain"],"F":["Morocco","Croatia"],
                    "G":["Brazil","Switzerland"],"H":["Portugal","South Korea"]}
        act_adv = real_adv[g]
        
        print(f"\n  ┌─ {g}组 ─────────────────────────────┐")
        
        # 对比预测出线 vs 实际出线
        hits = sum(1 for t in pred_adv if t in act_adv)
        print(f"  预测出线: {', '.join(pred_adv)}")
        print(f"  实际出线: {', '.join(act_adv)}")
        print(f"  出线命中: {hits}/2 {'✅' if hits==2 else '🟡' if hits==1 else '❌'}")
        
        print(f"  {'主队':<16} {'客队':<16} {'预测':<10} {'实际':<10} {'判定':<8}")
        print(f"  {'-'*54}")
        
        for t1, t2 in matches:
            key = (g, t1, t2)
            st = match_stats.get(key, {"t1_w":0,"d":0,"t2_w":0,"goals":{"0:0":2000}})
            best = max(st["goals"].items(), key=lambda x: x[1])
            pred = tuple(map(int, best[0].split(":")))
            actual = ACTUAL_SCORES.get((g,t1,t2), (0,0))
            
            verdict = score_close(pred, actual)
            total += 1
            
            markers = {"exact":"✅ 精确","close":"🟡 接近","direction":"⬆️ 方向","wrong":"❌ 错误"}
            mark = markers.get(verdict, "?")
            
            if verdict == "exact": exact += 1
            elif verdict == "close": close += 1
            elif verdict == "direction": direction_correct += 1
            elif verdict == "wrong": wrong_dir += 1
            
            print(f"  {t1:<16} {t2:<16} {best[0]:<10} {actual[0]}:{actual[1]:<7} {mark}")
        
        # 总结统计
        print()
    
    # 汇总
    print(f"\n{'='*70}")
    print("  📊 小组赛预测统计")
    print(f"{'='*70}")
    
    # 统计出线准确率
    adv_total = 0
    adv_correct = 0
    adv_half = 0
    adv_wrong = 0
    for g in "ABCDEFGH":
        gt = GROUPS[g]
        matches = [(gt[i],gt[j]) for i in range(4) for j in range(i+1,4)]
        exp_pts = {t:0 for t in gt}
        for t1, t2 in matches:
            key = (g, t1, t2)
            st = match_stats.get(key, {"t1_w":0,"d":0,"t2_w":0,"goals":{}})
            exp_pts[t1] += st["t1_w"]/N*3 + st["d"]/N*1
            exp_pts[t2] += st["t2_w"]/N*3 + st["d"]/N*1
        ranked = sorted(gt, key=lambda t: -exp_pts[t])
        pred_adv = ranked[:2]
        real_adv = {"A":["Netherlands","Senegal"],"B":["England","USA"],"C":["Argentina","Poland"],
                    "D":["France","Australia"],"E":["Japan","Spain"],"F":["Morocco","Croatia"],
                    "G":["Brazil","Switzerland"],"H":["Portugal","South Korea"]}
        hits = sum(1 for t in pred_adv if t in real_adv[g])
        if hits == 2: adv_correct += 2
        elif hits == 1: adv_correct += 1; adv_half += 1
        else: adv_wrong += 1
    
    print(f"  📋 出线预测: {adv_correct}/16 队正确 ({adv_correct/16*100:.0f}%)")
    print(f"     全部命中: {adv_correct//2}组 | 命中1队: {adv_half}组 | 全错: {adv_wrong}组")
    print()
    print(f"  {'指标':<30} {'数量':<8} {'占比':<8}")
    print(f"  {'-'*48}")
    print(f"  总场次:                          {total:<8}")
    print(f"  ✅ 比分精确命中:                 {exact:<8} {exact/total*100:<7.1f}%")
    print(f"  🟡 比分接近命中(胜负对±1球):    {close:<8} {close/total*100:<7.1f}%")
    print(f"  ⬆️ 方向正确(胜负方向对):        {direction_correct:<8} {direction_correct/total*100:<7.1f}%")
    print(f"  ❌ 方向错误(胜负方向反):        {wrong_dir:<8} {wrong_dir/total*100:<7.1f}%")
    correct_dir = exact + close + direction_correct
    print(f"  {'─'*48}")
    print(f"  🏆 胜负方向准确率:              {correct_dir:<8} {correct_dir/total*100:<7.1f}%")
    
    # 概率校准: 预测赢家概率 vs 实际
    print(f"\n{'='*70}")
    print("  🎯 胜负预测信心校准")
    print(f"{'='*70}")
    print(f"  {'预测胜率区间':<16} {'场次':<6} {'实际胜场':<10}")
    print(f"  {'-'*36}")
    for lo, hi in [(30,45),(45,55),(55,70),(70,100)]:
        matches_in_range = 0
        actual_wins = 0
        for key, st in match_stats.items():
            g, t1, t2 = key
            wp = max(st["t1_w"], st["t2_w"]) / N * 100
            if lo <= wp < hi:
                matches_in_range += 1
                pred_w = t1 if st["t1_w"] > st["t2_w"] else t2
                actual_s = ACTUAL_SCORES.get((g,t1,t2), (0,0))
                actual_w = t1 if actual_s[0] > actual_s[1] else t2 if actual_s[1] > actual_s[0] else None
                if actual_w == pred_w:
                    actual_wins += 1
        if matches_in_range:
            pct = actual_wins/matches_in_range*100
            bar = "█"*int(pct/5)
            print(f"  {lo:>3}%~{hi:<3}%   {matches_in_range:<6} {actual_wins:<8} ({pct:.0f}%) {bar}")
    
    print(f"\n  结论: 小组赛胜负方向准确率 {correct_dir/total*100:.0f}%")
    print(f"       48队晋级预测基于2000次MC, 八强6/8 (见主报告)")

if __name__ == "__main__":
    main()
