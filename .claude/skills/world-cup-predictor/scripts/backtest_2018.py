#!/usr/bin/env python3
"""
2018世界杯回测 v2 — 防作弊版
=============================
- 用compute_ratings生成评分 (仅使用≤2018数据)
- 用v4.6权重和引擎参数
- 不含任何2018后信息
"""
import math, random, os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from compute_ratings import compute_rating

GROUPS = {
    "A":["Russia","Saudi Arabia","Egypt","Uruguay"],
    "B":["Portugal","Spain","Morocco","Iran"],
    "C":["France","Australia","Peru","Denmark"],
    "D":["Argentina","Iceland","Croatia","Nigeria"],
    "E":["Brazil","Switzerland","Costa Rica","Serbia"],
    "F":["Germany","Mexico","Sweden","South Korea"],
    "G":["Belgium","Panama","Tunisia","England"],
    "H":["Poland","Senegal","Colombia","Japan"],
}
ALL_T = [t for g in GROUPS.values() for t in g]

# 2018年6月FIFA排名 (世界杯前)
FIFA_RANK = {
    "Germany":1,"Brazil":2,"Belgium":3,"Portugal":4,"Argentina":5,
    "Switzerland":6,"France":7,"Spain":8,"Poland":10,"England":12,
    "Colombia":13,"Uruguay":14,"Croatia":16,"Denmark":18,"Mexico":19,
    "Senegal":21,"Sweden":24,"Russia":33,"Japan":34,"South Korea":41,
    "Saudi Arabia":44,"Egypt":46,"Australia":47,"Iran":50,
    "Morocco":55,"Nigeria":57,"Costa Rica":59,"Iceland":62,
    "Serbia":65,"Peru":68,"Panama":70,"Tunisia":72,
}

ACTUAL = {"France":7,"Croatia":6,"Belgium":5,"England":5,
    "Uruguay":4,"Brazil":4,"Sweden":4,"Russia":4,
    "Argentina":3,"Portugal":3,"Spain":3,"Denmark":3,
    "Switzerland":3,"Mexico":3,"Japan":3,"Colombia":3,
    "Germany":1,"Iceland":1,"Nigeria":1,"Costa Rica":1,
    "Serbia":1,"South Korea":1,"Senegal":1,"Poland":1,
    "Iran":1,"Morocco":1,"Peru":1,"Australia":1,
    "Saudi Arabia":1,"Egypt":1,"Panama":1,"Tunisia":1,
}
ACTUAL_CHAMP = "France"

RN = {7:"🏆冠军",6:"🥈亚军",5:"🥉四强",4:"八强",3:"16强",1:"小组"}

# ======== ENGINE (v4.6) ========
def frs(r):
    if r<=5: return 13+(6-r)*0.4
    elif r<=10: return 11+(10-r)*0.4
    elif r<=20: return 8+(20-r)*0.3
    elif r<=30: return 5+(30-r)*0.3
    elif r<=40: return 3+(40-r)*0.2
    else: return max(0,3-(r-40)*0.3)

def mk(n, d):
    rf,wh,cp,atk,defn,ada,tac,inj = d
    fr = frs(FIFA_RANK.get(n,50)); frr=fr/0.15
    bs = rf*0.26 + frr*0.12 + wh*0.05 + cp*0.12 + atk*0.08 + defn*0.20 + ada*0.02 + tac*0.14
    return {"f":round(bs+inj,2),"a":atk,"d":defn}

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
    return pois(random.gammavariate(r,(1-p)/p))

def xg(a,d,avg=1.70): return avg*(a/50)*((100-d)/50)
RA={"g":1.70,"r16":1.40,"qf":1.20,"sf":1.10,"f":1.05}

def pr(_,__,a1,d1,a2,d2,ko=False,seed=None,rt="g"):
    if seed: random.seed(seed)
    avg=RA.get(rt,1.35)
    g1=nb(max(0.15,xg(a1,d2,avg)),0.65)
    g2=nb(max(0.15,xg(a2,d1,avg)),0.65)
    if g1>g2: return "t1",g1,g2
    if g2>g1: return "t2",g1,g2
    if ko:
        random.seed((seed or 0)+999)
        if random.random()<0.50:
            adv=a1+d1-a2-d2
            et=0.50+min(0.12,adv*0.002)
            if random.random()<et: g1+=1
            else: g2+=1
        if g1==g2:
            random.seed((seed or 0)+1000)
            adv=a1+d1-a2-d2
            pk=0.50+min(0.04,adv*0.0005)
            if random.random()<pk: g1+=1
            else: g2+=1
        return ("t1" if g1>g2 else "t2"),g1,g2
    return "draw",g1,g2

def main():
    TEAMS = {}
    for n in ALL_T:
        is_champ = (n == "Germany")  # 2014冠军, 魔咒生效
        td = compute_rating(n, FIFA_RANK.get(n,50), 2018, defending_champion=is_champ)
        TEAMS[n] = mk(n, td)

    N=2000; cc={t:0 for t in TEAMS}; ac={t:0 for t in TEAMS}

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
        for t in adv: ac[t]=ac.get(t,0)+1

        seeded=sorted(adv[:16],key=lambda t:TEAMS[t]["f"],reverse=True)
        cur=[(seeded[i],seeded[15-i]) for i in range(8)]
        for rd in range(4):
            nxt=[]; rd_n=["r16","qf","sf","final"]
            for mi,(t1,t2) in enumerate(cur):
                td1,td2=TEAMS[t1],TEAMS[t2]
                r,g1,g2=pr(0,0,td1["a"],td1["d"],td2["a"],td2["d"],ko=True,
                          seed=bs+10000+rd*1000+mi*13,rt=rd_n[rd])
                w=t1 if r=="t1" else t2; nxt.append(w)
                if rd==3: cc[w]=cc.get(w,0)+1
            if rd<3: cur=[(nxt[i],nxt[i+1]) for i in range(0,len(nxt),2)]

    cs=sorted(cc.items(),key=lambda x:x[1],reverse=True)

    print("="*65)
    print("  2018世界杯回测 v2 (防作弊版)")
    print("  引擎: v4.6 | 评分: compute_ratings (仅≤2018数据)")
    print("="*65)
    print(f"\n{'#':<3} {'球队':<16} {'夺冠率':<8} {'实际':<8} {'晋级率':<6}")
    print("-"*48)
    for i,(t,c) in enumerate(cs[:16],1):
        print(f"{i:<3} {t:<16} {c/N*100:<6.2f}% {RN.get(ACTUAL.get(t,1),'?'):<8} {ac.get(t,0)/N*100:<5.0f}%")

    print(f"\n{'='*65}")
    print("  📈 核心指标")
    print(f"{'='*65}")
    champ_pred = cs[0][0]
    print(f"\n  冠军预测: {champ_pred} {'✅' if champ_pred==ACTUAL_CHAMP else '❌'} (实际: {ACTUAL_CHAMP})")
    print(f"  冠军在前3: {', '.join(t for t,_ in cs[:3])} {'✅' if ACTUAL_CHAMP in [t for t,_ in cs[:3]] else '❌'}")

    for label, n, actual_list in [
        ("四强",4,["France","Croatia","Belgium","England"]),
        ("八强",8,["France","Uruguay","Brazil","Belgium","Russia","Croatia","Sweden","England"]),
        ("16强",16,["France","Argentina","Uruguay","Portugal","Spain","Russia","Croatia","Denmark",
                    "Brazil","Switzerland","Mexico","Sweden","Belgium","Colombia","England","Japan"]),
    ]:
        pred=[t for t,_ in cs[:n]]
        hits=sum(1 for t in pred if t in actual_list)
        print(f"  {label}命中: {hits}/{n} ({hits/n*100:.0f}%)")

    # Brier Score
    brier = sum((cc.get(t,0)/N - (1 if t==ACTUAL_CHAMP else 0))**2 for t in TEAMS) / len(TEAMS)
    print(f"\n  Brier Score: {brier:.6f}")

    # 轮次误差
    round_err=0
    for t in TEAMS:
        cp=cc.get(t,0)/N;ap=ac.get(t,0)/N
        if ap<0.3: prd=1
        elif ap<0.5: prd=2
        elif cp>0.2: prd=7
        elif cp>0.1: prd=6
        elif cp>0.05: prd=5
        elif cp>0.025: prd=4
        else: prd=3
        round_err+=abs(prd-ACTUAL.get(t,1))
    print(f"  平均轮次误差: {round_err/len(TEAMS):.2f}")

    # 偏差
    print(f"\n{'='*65}")
    print("  偏 差 分 析")
    print(f"{'='*65}")
    print(f"  {'球队':<16} {'预测':<8} {'实际':<8}")
    over,under=[],[]
    for t in sorted(TEAMS.keys()):
        cp=cc.get(t,0)/N;ap=ac.get(t,0)/N
        prd=1 if ap<0.3 else 2 if ap<0.5 else 3 if cp<0.025 else 4 if cp<0.05 else 5 if cp<0.1 else 6 if cp<0.2 else 7
        err=prd-ACTUAL.get(t,1)
        if err>=2: over.append((t,err))
        if err<=-2: under.append((t,err))
        if abs(err)>=1:
            print(f"  {t:<16} {RN.get(prd,'R'+str(prd)):<8} {RN.get(ACTUAL.get(t,1),'?'):<8} {'📈' if err>0 else '📉'}{abs(err):+d}")
    if over:
        print(f"\n  📈 最被高估:")
        for t,e in sorted(over,key=lambda x:-x[1])[:5]: print(f"     {t}: +{e}轮")
    if under:
        print(f"\n  📉 最被低估:")
        for t,e in sorted(under,key=lambda x:x[1])[:5]: print(f"     {t}: {e}轮")

    print(f"\n{'='*65}")
    print(f"  结论: 2018是独立测试集(权重在2022上优化)")
    print(f"  真实泛化能力: Brier={brier:.4f} 轮差={round_err/len(TEAMS):.2f}")
    print(f"{'='*65}")

if __name__=="__main__":
    main()
