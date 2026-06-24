# 预选赛数据架构 (v4.8双轨制)

## 问题

xG公式 `avg × (atk/50) × ((100-def)/50)` 需要 atk/def 在 ~70-95 范围才能产生合理预测值。
但真实预选赛数据给出的 atk 范围是 50-80 (经大洲归一化后)，直接代入会破坏 xG 校准。

## 解决方案: 攻防评分双轨制

将 atk/defn 的两种用途解耦：

```
compute_ratings.py → (form, hist, core, atk, defn, venue, tac, inj)
                                          │     │
                    ┌─────────────────────┘     └──────────────┐
                    ▼                                         ▼
            xG公式: avg × (atk/50) × ((100-def)/50)    base_score: rf·26% + qatk·8% + qdfn·20%
                                                                    ↑           ↑
              使用原始FIFA排名衍生值 ← compute_ratings        真实预选赛数据 ← qualifying_data.py
```

## 数据流

```
1. compute_rating("Brazil", 5, 2026) → (90, 65, 88, 90, 82, 69, 87, 0)
                                                    │    │
                                                    │    └──→ make_team() → TEAMS["Brazil"]["atk"] = 90 (xG用)
                                                    │
                                                    └────→ get_qualifying_rating("Brazil") → (63, 62) → base_score 用

2. make_team("Brazil", ...):
   - qatk, qdfn = get_qualifying_rating("Brazil")  # 真实数据
   - base_score = rf·0.26 + fr_raw·0.12 + wh·0.05 + cp·0.12 
                + qatk·0.08 + qdfn·0.20 + ada·0.02 + tac·0.14
   - TEAMS["Brazil"] = {"score": base_score, "atk": 90, "defn": 82}  # xG用旧值
```

## 为何不用重缩放

尝试过 `atk_scaled = atk_real * 1.6 - 15` 等线性重缩放让真实数据进入xG有效范围，
但产生大量 95 上限截断(capping)，去掉了大洲间差异。

双轨制是最干净的方案：base_score 的 28% 权重受益于真实数据，
xG 公式保持完全不变。

## 关键代码

qualifying_data.py:
```python
def get_qualifying_rating(team):
    """返回(atk, dfn)用于base_score, 不影响xG."""
    adj_gf = gf_m * CONF_FACTOR[conf]
    adj_ga = ga_m * CONF_FACTOR[conf]
    qatk = 40 + (adj_gf / 3.0) * 55
    qdfn = 95 - (adj_ga / 1.5) * 55
    return cap(qatk), cap(qdfn)
```

full_prediction.py (make_team):
```python
from qualifying_data import get_qualifying_rating
qatk, qdfn = get_qualifying_rating(name)
base_score = rf*0.26 + ... + qatk*0.08 + qdfn*0.20 + ...
```

## 影响范围

28% 的 base_score 权重现在由真实数据驱动（atk 8% + dfn 20%）。

| 变动幅度 | 球队 | 原因 |
|:--------|:-----|:------|
| +10.6 | 科特迪瓦 | 2.50GF/M 0.00GA/M (CAF) |
| +9.8 | 新西兰 | 6.33GF/M (但OFC×0.38归一化后合理) |
| +9.6 | 突尼斯 | 2.11GF/M 0.00GA/M (CAF) |
| -6.2 | 巴西 | 1.33GF/M 0.94GA/M (CONMEBOL实际平庸) |
| -6.1 | 瑞典 | 0.67GF/M 2.00GA/M (UEFA表现很差) |
