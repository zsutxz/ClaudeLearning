#!/usr/bin/env python3
"""
双年联合权重搜索 (2018+2022)
============================
同时优化两个独立赛事的权重, 避免过拟合单一赛事.
"""
import math, random, os, sys, time
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from compute_ratings import compute_rating

# ======== 2018数据 ========
G18 = {"A":["Russia","Saudi Arabia","Egypt","Uruguay"],"B":["Portugal","Spain","Morocco","Iran"],
       "C":["France","Australia","Peru","Denmark"],"D":["Argentina","Iceland","Croatia","Nigeria"],
       "E":["Brazil","Switzerland","Costa Rica","Serbia"],"F":["Germany","Mexico","Sweden","South Korea"],
       "G":["Belgium","Panama","Tunisia","England"],"H":["Poland","Senegal","Colombia","Japan"]}
ALL18 = [t for g in G18 for t in G18[g]]
FR18 = {"Germany":1,"Brazil":2,"Belgium":3,"Portugal":4,"Argentina":5,"Switzerland":6,"France":7,"Spain":8,
        "Poland":10,"England":12,"Colombia":13,"Uruguay":14,"Croatia":16,"Denmark":18,"Mexico":19,
        "Senegal":21,"Sweden":24,"Russia":33,"Japan":34,"South Korea":41,"Saudi Arabia":44,"Egypt":46,
        "Australia":47,"Iran":50,"Morocco":55,"Nigeria":57,"Costa Rica":59,"Iceland":62,"Serbia":65,
        "Peru":68,"Panama":70,"Tunisia":72}
ACT18 = {"France":7,"Croatia":6,"Belgium":5,"England":5,"Uruguay":4,"Brazil":4,"Sweden":4,"Russia":4,
         "Argentina":3,"Portugal":3,"Spain":3,"Denmark":3,"Switzerland":3,"Mexico":3,"Japan":3,"Colombia":3,
         "Germany":1,"Iceland":1,"Nigeria":1,"Costa Rica":1,"Serbia":1,"South Korea":1,"Senegal":1,
         "Poland":1,"Iran":1,"Morocco":1,"Peru":1,"Australia":1,"Saudi Arabia":1,"Egypt":1,"Panama":1,"Tunisia":1}

# ======== 2022数据 ========
G22 = {"A":["Qatar","Ecuador","Senegal","Netherlands"],"B":["England","Iran","USA","Wales"],
       "C":["Argentina","Saudi Arabia","Mexico","Poland"],"D":["France","Australia","Denmark","Tunisia"],
       "E":["Spain","Costa Rica","Germany","Japan"],"F":["Belgium","Canada","Morocco","Croatia"],
       "G":["Brazil","Serbia","Switzerland","Cameroon"],"H":["Portugal","Ghana","Uruguay","South Korea"]}
ALL22 = [t for g in G22 for t in G22[g]]
FR22 = {"Brazil":1,"Belgium":2,"Argentina":3,"France":4,"England":5,"Spain":7,"Netherlands":8,
        "Portugal":9,"Denmark":10,"Germany":11,"Croatia":12,"Mexico":13,"Uruguay":14,"Switzerland":15,
        "USA":16,"Senegal":18,"Wales":19,"Iran":20,"Japan":24,"Morocco":22,"Serbia":21,"Poland":26,
        "South Korea":28,"Tunisia":30,"Costa Rica":31,"Australia":38,"Canada":41,"Cameroon":43,
        "Ecuador":44,"Saudi Arabia":50,"Qatar":50,"Ghana":61}
ACT22 = {"Argentina":7,"France":6,"Croatia":5,"Morocco":5,"Netherlands":4,"England":4,"Brazil":4,
         "Portugal":4,"USA":3,"Australia":3,"Japan":3,"South Korea":3,"Poland":3,"Senegal":3,
         "Switzerland":3,"Spain":3,"Qatar":1,"Ecuador":1,"Saudi Arabia":1,"Mexico":1,"Wales":1,
         "Iran":1,"Denmark":1,"Tunisia":1,"Costa Rica":1,"Germany":1,"Belgium":1,"Canada":1,
         "Cameroon":1,"Serbia":1,"Ghana":1,"Uruguay":1}

BASE = {"state":0.30,"fifa":0.13,"history":0.05,"core":0.12,"atk":0.08,"def":0.18,"venue":0.02,"tactics":0.12}

# ======== ENGINE ========
def frs(r):
    if r<=5: return 13+(6-r)*0.4
    elif r<=10: return 11+(10-r)*0.4
    elif r<=20: return 8+(20-r)*0.3
    elif r<=30: return 5+(30-r)*0.3
    elif r<=40: return 3+(40-r)*0.2
    else: return max(0,3-(r-40)*0.3)

def mk(n, d, w, fr_dict):
    rf,wh,cp,atk,defn,ada,tac,inj = d
    fr = frs(fr_dict.get(n,50)); frr=fr/0.15
    bs = rf*w["state"] + frr*w["fifa"] + wh*w["history"] + cp*w["core"] + atk*w["atk"] + defn*w["def"] + ada*w["venue"] + tac*w["tactics"]
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
            adv=a1+d1-a2-d2;et=0.50+min(0.12,adv*0.002)
            if random.random()<et:g1+=1
            else:g2+=1
        if g1==g2:
            random.seed((seed or 0)+1000)
            adv=a1+d1-a2-d2;pk=0.50+min(0.04,adv*0.0005)
            if random.random()<pk:g1+=1
            else:g2+=1
        return ("t1" if g1>g2 else "t2"),g1,g2
    return "draw",g1,g2

def run_backtest(groups, all_teams, fr_dict, actual, champ, weight, year, N=500):
    """跑一个赛事的回测, 返回 (方向准确率, Brier, 轮次误差, 冠军Top3)."""
    TEAMS = {}
    for n in all_teams:
        dc = (n == "Germany" and year == 2018)  # 德国2018卫冕冠军
        td = compute_rating(n, fr_dict.get(n,50), year, defending_champion=dc)
        TEAMS[n] = mk(n, td, weight, fr_dict)

    cc={t:0 for t in TEAMS};ac={t:0 for t in TEAMS}
    for sim in range(N):
        bs=100+sim*37;random.seed(bs);adv=[]
        for gi,g in enumerate("ABCDEFGH"):
            gt=groups[g];tbl={t:{"p":0,"gd":0,"gf":0} for t in gt}
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
        for t in adv:ac[t]=ac.get(t,0)+1
        seeded=sorted(adv[:16],key=lambda t:TEAMS[t]["f"],reverse=True)
        cur=[(seeded[i],seeded[15-i]) for i in range(8)]
        for rd in range(4):
            nxt=[];rd_n=["r16","qf","sf","final"]
            for mi,(t1,t2) in enumerate(cur):
                td1,td2=TEAMS[t1],TEAMS[t2]
                r,g1,g2=pr(0,0,td1["a"],td1["d"],td2["a"],td2["d"],ko=True,seed=bs+10000+rd*1000+mi*13,rt=rd_n[rd])
                w=t1 if r=="t1" else t2;nxt.append(w)
                if rd==3:cc[w]=cc.get(w,0)+1
            if rd<3:cur=[(nxt[i],nxt[i+1]) for i in range(0,len(nxt),2)]
    
    cs=sorted(cc.items(),key=lambda x:x[1],reverse=True)
    brier=sum((cc.get(t,0)/N - (1 if t==champ else 0))**2 for t in TEAMS)/len(TEAMS)
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
        round_err+=abs(prd-actual.get(t,1))
    avg_err=round_err/len(TEAMS)
    top3=[t for t,_ in cs[:3]]
    champ_top3=1 if champ in top3 else 0
    qf_pred=[t for t,_ in cs[:8]]
    qf_actual=[t for t in actual if actual[t]>=4]
    qf_hits=sum(1 for t in qf_pred if t in qf_actual)
    return brier,avg_err,champ_top3,qf_hits

def main():
    candidates=[]
    base=BASE.copy()
    candidates.append(("v4.6基线",dict(base)))
    
    # 状态±
    for d in [0.02,-0.02]:
        w=base.copy();w["state"]+=d;w["tactics"]-=d;candidates.append((f"state{w['state']:.0%}",w))
    # 防守±
    for d in [0.02,-0.02]:
        w=base.copy();w["def"]+=d;w["fifa"]-=d;candidates.append((f"def{w['def']:.0%}",w))
    # 历史±
    for d in [0.02,-0.02]:
        w=base.copy();w["history"]+=d;w["state"]-=d;candidates.append((f"hist{w['history']:.0%}",w))
    # 激进
    w=base.copy();w["state"]=0.28;w["def"]=0.20;w["history"]=0.03;w["fifa"]=0.12;w["venue"]=0.01
    candidates.append(("激进:state28%def20%hist3%",w))
    w=base.copy();w["state"]=0.26;w["def"]=0.20;w["tactics"]=0.14;w["fifa"]=0.12
    candidates.append(("防守:def20%tac14%",w))
    w=base.copy();w["state"]=0.28;w["history"]=0.08;w["def"]=0.16;w["fifa"]=0.12;w["tactics"]=0.10
    candidates.append(("平衡:state28%hist8%def16%",w))
    
    results=[]
    t0=time.time()
    for i,(name,w) in enumerate(candidates,1):
        b18,re18,ct18,qf18=run_backtest(G18,ALL18,FR18,ACT18,"France",w,2018,500)
        b22,re22,ct22,qf22=run_backtest(G22,ALL22,FR22,ACT22,"Argentina",w,2022,500)
        # 联合评分: 两届Brier平均 + 冠军Top3奖励 - 轮差惩罚
        score = -(b18+b22)*50 - (re18+re22)*0.3 + (ct18+ct22)*3 + (qf18+qf22)*0.5
        results.append((score,b18,b22,re18,re22,ct18,ct22,qf18,qf22,name,w))
        print(f"  [{i:>2}] {name:<22} 2018:B={b18:.4f} R={re18:.2f} 2022:B={b22:.4f} R={re22:.2f} QF:{qf18}/{qf22}")
    
    results.sort(key=lambda x:x[0],reverse=True)
    total=time.time()-t0
    
    print(f"\n{'='*60}")
    print(f"  🏆 双年联合最优 (耗时{total:.0f}s)")
    print(f"{'='*60}")
    print(f"  {'#':<3} {'名称':<22} {'2018Brier':<10} {'2022Brier':<10} {'2018轮差':<8} {'2022轮差':<8}")
    for i,r in enumerate(results[:5],1):
        print(f"  {i:<3} {r[9]:<22} {r[1]:<9.4f} {r[2]:<9.4f} {r[3]:<7.2f} {r[4]:<7.2f}")
    
    best=results[0]
    print(f"\n  最优权重: {best[9]}")
    for k in ["state","fifa","history","core","atk","def","venue","tactics"]:
        print(f"    {k:>8} = {best[10][k]:.0%}")
    print(f"\n  可直接复制到full_prediction.py的权重行:")
    b=best[10]
    print(f"  rf*{b['state']:.2f} + fr_raw*{b['fifa']:.2f} + wh*{b['history']:.2f}")
    print(f"  + cp*{b['core']:.2f} + atk*{b['atk']:.2f} + defn*{b['def']:.2f}")
    print(f"  + ada*{b['venue']:.2f} + tac*{b['tactics']:.2f}")

if __name__=="__main__":
    main()
