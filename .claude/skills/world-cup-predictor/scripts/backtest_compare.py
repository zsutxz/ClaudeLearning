#!/usr/bin/env python3
"""
数据驱动 vs 人工评分 — 2022回测对比

⚠️ 诚实性声明: 下方 MANUAL_TD（人工评分）为主观赛前估计，与 backtest_2022.py 的
   TEAM_DATA_2022 同源，数值无法证明完全客观。本脚本的「人工 vs 数据」对比结论
   仅供方法演示，不可作为模型质量证据。
"""
import math, random, os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from compute_ratings import compute_rating, WC_HISTORY

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

FIFA_2022 = {
    "Brazil":1,"Belgium":2,"Argentina":3,"France":4,"England":5,
    "Italy":6,"Spain":7,"Netherlands":8,"Portugal":9,"Denmark":10,
    "Germany":11,"Croatia":12,"Mexico":13,"Uruguay":14,"Switzerland":15,
    "USA":16,"Senegal":18,"Wales":19,"Iran":20,"Japan":24,
    "Morocco":22,"Serbia":21,"Poland":26,"South Korea":28,
    "Tunisia":30,"Costa Rica":31,"Australia":38,"Canada":41,
    "Cameroon":43,"Ecuador":44,"Saudi Arabia":50,"Qatar":50,"Ghana":61,
}

# 人工评分 (当前引擎在用)
MANUAL_TD = {
    "Argentina":(88,88,92,85,82,65,85,0),"France":(85,90,90,90,80,68,88,-0.5),
    "Brazil":(90,92,90,90,82,60,82,0),"England":(85,75,88,85,82,62,82,0),
    "Netherlands":(82,70,82,78,82,60,85,0),"Croatia":(76,78,78,70,78,60,82,0),
    "Portugal":(82,68,85,82,76,62,80,0),"Spain":(80,72,85,80,80,60,88,0),
    "Germany":(78,85,82,80,78,60,78,0),"Belgium":(82,72,85,82,74,60,75,0),
    "Denmark":(80,60,78,76,80,58,76,0),"Uruguay":(76,75,78,74,78,65,78,0),
    "Switzerland":(74,60,74,70,76,60,74,0),"USA":(74,45,74,72,72,65,72,0),
    "Senegal":(76,50,74,72,70,62,70,0),"Poland":(72,45,74,72,68,58,68,0),
    "Mexico":(72,65,74,70,70,75,72,0),"Japan":(76,55,74,72,68,55,78,0),
    "Morocco":(75,50,74,68,78,65,80,0),"South Korea":(72,55,72,72,66,55,72,0),
    "Serbia":(74,45,74,74,68,58,68,0),"Iran":(72,45,70,66,70,55,68,0),
    "Ecuador":(72,40,68,68,70,60,66,0),"Wales":(70,42,70,68,68,58,68,0),
    "Cameroon":(68,40,68,68,64,58,64,0),"Canada":(70,25,70,70,66,68,68,0),
    "Ghana":(68,45,68,68,64,58,64,0),"Costa Rica":(66,42,66,64,66,60,66,0),
    "Australia":(66,48,66,64,68,58,64,0),"Saudi Arabia":(64,40,64,64,62,55,64,0),
    "Tunisia":(66,40,64,64,66,58,64,0),"Qatar":(62,20,62,62,62,55,62,0),
}

ACTUAL = {
    "Argentina":7,"France":6,"Croatia":5,"Morocco":5,
    "Netherlands":4,"England":4,"Brazil":4,"Portugal":4,
    "USA":3,"Australia":3,"Japan":3,"South Korea":3,
    "Poland":3,"Senegal":3,"Switzerland":3,"Spain":3,
    **{t:1 for t in ["Qatar","Ecuador","Saudi Arabia","Mexico","Wales","Iran",
        "Denmark","Tunisia","Costa Rica","Germany","Belgium","Canada",
        "Cameroon","Serbia","Ghana","Uruguay"]}
}
RN = {7:"🏆冠军",6:"🥈亚军",5:"🥉四强",4:"八强",3:"16强",2:"32强",1:"小组"}

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
    fr = frs(FIFA_2022.get(name,50))
    frr = fr/0.15
    bs = rf*0.22 + frr*0.14 + wh*0.11 + cp*0.12 + atk*0.09 + defn*0.13 + ada*0.08 + tac*0.11
    return {"s":round(bs,2),"f":round(bs+inj,2),"a":atk,"d":defn}

def pois(l):
    if l<=0: return 0
    L=math.exp(-l);k=0;p=1.0
    while p>L: k+=1; p*=random.random()
    return max(0,k-1)

def nb(m,p=0.7):
    if m<=0: return 0
    if m<0.3: return pois(m)
    r=m*p/(1-p)
    if r<1: return pois(m)
    return pois(random.gammavariate(r,(1-p)/p))

def xg(a,d,avg=1.40): return avg*(a/50)*((120-d)/70)
RA={"g":1.80,"r16":1.40,"qf":1.30,"sf":1.20,"f":1.15}

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

def run_backtest(name, team_data):
    """跑一次完整回测, 返回(夺冠排名列表, 评估指标)."""
    TEAMS = {}
    for n in ALL_T:
        if n in team_data: TEAMS[n] = mk(n, team_data[n])
    
    N=2000; cc={t:0 for t in TEAMS}
    for sim in range(N):
        bs=100+sim*37; random.seed(bs); adv=[]
        for gi,g in enumerate("ABCDEFGH"):
            gt=GROUPS[g]; tbl={t:{"p":0,"gd":0,"gf":0} for t in gt}
            for mi,(t1,t2) in enumerate([(gt[i],gt[j]) for i in range(4) for j in range(i+1,4)]):
                td1,td2=TEAMS[t1],TEAMS[t2]
                r,g1,g2=pr(0,0,td1["a"],td1["d"],td2["a"],td2["d"],seed=bs+gi*100+mi*7,rt="g")
                tbl[t1]["gf"]+=g1;tbl[t2]["gf"]+=g2
                tbl[t1]["gd"]+=g1-g2;tbl[t2]["gd"]+=g2-g1
                if r=="t1":tbl[t1]["p"]+=3
                elif r=="t2":tbl[t2]["p"]+=3
                else:tbl[t1]["p"]+=1;tbl[t2]["p"]+=1
            st=sorted(tbl.items(),key=lambda x:(x[1]["p"],x[1]["gd"],x[1]["gf"]),reverse=True)
            adv.append(st[0][0]);adv.append(st[1][0])
        seeded=sorted(adv[:16],key=lambda t:TEAMS[t]["f"],reverse=True)
        cur=[(seeded[i],seeded[15-i]) for i in range(8)]
        for rd in range(4):
            nxt=[]
            for mi,(t1,t2) in enumerate(cur):
                td1,td2=TEAMS[t1],TEAMS[t2]
                r,g1,g2=pr(0,0,td1["a"],td1["d"],td2["a"],td2["d"],ko=True,
                          seed=bs+10000+rd*1000+mi*13,rt=["r16","qf","sf","f"][rd])
                w=t1 if r=="t1" else t2; nxt.append(w)
                if rd==3: cc[w]=cc.get(w,0)+1
            if rd<3: cur=[(nxt[i],nxt[i+1]) for i in range(0,len(nxt),2)]
    return sorted(cc.items(),key=lambda x:x[1],reverse=True)

def evaluate(cs, label):
    """评估并输出."""
    qf_actual = ["Argentina","Netherlands","England","France","Brazil","Croatia","Morocco","Portugal"]
    sf_actual = ["Argentina","France","Croatia","Morocco"]
    champ_actual = "Argentina"
    
    pred_top8 = [t for t,_ in cs[:8]]
    pred_top4 = [t for t,_ in cs[:4]]
    champ_pred = cs[0][0]
    
    qf_hits = sum(1 for t in pred_top8 if t in qf_actual)
    sf_hits = sum(1 for t in pred_top4 if t in sf_actual)
    champ_top3 = champ_actual in [t for t,_ in cs[:3]]
    
    avg_rank = 0
    for i,(t,c) in enumerate(cs,1):
        if t == champ_actual: avg_rank = i; break
    
    # 排名误差
    total_err = 0
    actual_ranked = sorted(ACTUAL.keys(), key=lambda t: ACTUAL[t], reverse=True)
    for team in ALL_T:
        pred_rank = next(i+1 for i,(t,_) in enumerate(cs) if t==team)
        act_rank = next(i+1 for i,t in enumerate(actual_ranked) if t==team)
        total_err += abs(pred_rank - act_rank)
    
    print(f"\n{label}:")
    print(f"  🏆 冠军排名: #{avg_rank} {'✅' if avg_rank<=3 else '❌'}")
    print(f"  八强命中: {qf_hits}/8 ({qf_hits/8*100:.0f}%)")
    print(f"  四强命中: {sf_hits}/4 ({sf_hits/4*100:.0f}%)")
    print(f"  平均排名偏差: {total_err/len(ALL_T):.1f}")
    
    # Top 5
    print(f"  Top 5: {' → '.join(t for t,_ in cs[:5])}")
    return {"qf":qf_hits, "sf":sf_hits, "champ_rank":avg_rank, "rank_err":total_err/len(ALL_T)}

# ==================== 主流程 ====================
print("="*65)
print("  数据驱动 vs 人工评分 — 2022回测对决")
print("="*65)

# 1. 人工评分
print("\n📋 生成人工评分...")
manual_results = run_backtest("manual", MANUAL_TD)
m = evaluate(manual_results, "人工评分")

# 2. 数据驱动评分
print("\n🤖 生成数据驱动评分...")
auto_TD = {}
for team in ALL_T:
    if team in FIFA_2022:
        rank = FIFA_2022[team]
        auto_TD[team] = compute_rating(team, rank, 2022)

# 显示自动评分
print(f"\n{'球队':<16} {'状态':<5} {'历史':<5} {'核心':<5} {'进攻':<5} {'防守':<5} {'场地':<5} {'战术':<5}")
print("-"*50)
for team, rank in sorted(FIFA_2022.items(), key=lambda x: x[1]):
    if team in auto_TD and rank <= 16:
        print(f"{team:<16} {' '.join(f'{x:<5}' for x in auto_TD[team][:7])}")

auto_results = run_backtest("auto", auto_TD)
a = evaluate(auto_results, "数据驱动评分")

# 3. 对比
print(f"\n{'='*65}")
print("  ⚔️ 对决结果")
print(f"{'='*65}")
print(f"{'指标':<20} {'人工评分':<12} {'数据驱动':<12} {'胜者':<8}")
print("-"*52)
for metric, key in [("八强命中","qf"), ("四强命中","sf"), ("冠军排名","champ_rank"), ("排名偏差","rank_err")]:
    mv = m[key]
    av = a[key]
    if key == "champ_rank" or key == "rank_err":
        better = "✅人工" if mv <= av else "🤖数据"
        print(f"{metric:<20} {mv:<12} {av:<12} {better:<8}")
    else:
        better = "✅人工" if mv >= av else "🤖数据"
        print(f"{metric:<20} {mv:<12} {av:<12} {better:<8}")
