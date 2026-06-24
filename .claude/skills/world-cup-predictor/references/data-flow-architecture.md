# 数据流架构

## 模块调用链

```
full_prediction.py (主入口)
  │
  ├─ compute_ratings.py ──→ 8维评分元组 (form, hist, core, atk, dfn, venue, tac, inj)
  │     ↑                        ↑ 指数 0     1      2    3    4     5     6    7
  │     └── FIFA_RANK dict       │
  │                              │
  ├─ apply_injury_updates() ───→ 修改元组: td[3](atk), td[4](dfn) 按球员权重
  │     ↑                       直接在元组上修改, 后续make_team()自动继承修改
  │     └── Wikipedia API
  │
  ├─ make_team() ──────────────→ TEAMS[n] = {"score", "final", "atk", "defn", "inj"}
  │     ↑                       base_score = Σ(分维度 × 权重)
  │     └── BET365_IMPLIED       finale = base_score + inj (=卫冕冠军魔咒)
  │
  └─ predict() ────────────────→ (result, g1, g2)
        ↑                       xG = avg_goals × (atk/50) × (100-def/50)
        └── TEAMS[n]["atk"]     使用负二项分布抽样
             TEAMS[n]["defn"]
             + HOSTS/CONCACAF
```

## 关键: atk/dfn 是 xG 的唯一输入

```
compute_ratings() → 元组
  │                      ┌─ make_team() → TEAMS[n]["atk"] ──→ predict() → xG
  │                      │                                    ↕  (进球期望)
  └── td[3]=atk ────────┤        TEAMS[n]["defn"] ──→ predict() → xG
  td[4]=dfn              │
  td[7]=inj ────────────└─ make_team() → TEAMS[n]["final"] ──→ build_knockout_bracket()
                                                                  (排位, 不影响xG)
```

## 过去Bug: 伤病→xG断链 (v4.3前)

旧代码在 `apply_injury_updates()` 中:
```python
td[7] -= 1.5  # 只改了inj字段(指数7)
```
但 `predict()` 只用 atk/dfn (指数3和4) 计算 xG。 `inj` 虽然被加到 `final` 中影响了淘汰赛排位，但**进球期望完全不受影响**。

修复后 (v4.3):
```python
td[3] -= atk_penalty  # 直接降进攻评分 → xG降低
td[4] -= def_penalty  # 直接降防守评分 → 失球增加
```
卫冕冠军魔咒(-2.0) 仍保留在 `inj` 字段，这是一个心理效应，不应影响xG。

## 权重如何影响结果

权重只改变 `make_team()` 中的 `base_score` 计算公式:
```python
base_score = rf*0.26 + fr_raw*0.13 + wh*0.08 + cp*0.12
           + atk*0.08 + defn*0.16 + ada*0.05 + tac*0.12
```
权重改变不直接影响 atk/defn 评分 — 只改变 `final` (用于淘汰赛排位)。因此网格搜索只能优化**淘汰赛排位准确性** (Brier Score/平均轮次误差), 不能直接提高**小组赛方向准确率**。

## 蒙特卡洛模拟数据流

```
每届MC循环(N_SIM=2000):
  │
  ├─ 小组赛: predict() → stats (t1_w, d, t2_w, goals{分数:次数})
  │   ↓                   ↓
  │   每组6场           match_stats[(g,t1,t2)]
  │
  ├─ 小组排名: 按pts→GD→GF排序 → 头名+第二名晋级
  │
  ├─ 第三名: 前8个最佳第三名晋级
  │
  ├─ 淘汰赛: build_knockout_bracket()
  │   ↓
  │   5轮: R32→R16→QF→SF→Final
  │   predict(ko=True, round_type)
  │
  └─ 统计: champ_count[t]++ (决赛胜者)
           adv_count[t]++   (晋级淘汰赛)
           scorer_goals[(team, player)] += goals
```
