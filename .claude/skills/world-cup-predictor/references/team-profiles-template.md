# 球队数据采集模板

## 每支球队需要采集的数据

### 基本信息
| 字段 | 说明 | 数据来源 |
|------|------|---------|
| 队名（中文） | 常见中文译名 | - |
| 队名（英文） | 官方英文名 | FIFA官网 |
| FIFA排名 | 最新世界排名及排名变化 | FIFA官网 |
| 所在大洲 | CONMEBOL/UEFA/AFC/CAF/CONCACAF/OFC | - |
| 小组 | 2026世界杯分组 | FIFA官网 |
| 主教练 | 姓名、国籍、执教年限 | 转会市场/维基 |

### 近期战绩
| 字段 | 说明 |
|------|------|
| 近6场结果 | W/D/L 及比分 |
| 场均进球 | 近6场总进球/6 |
| 场均失球 | 近6场总失球/6 |
| 预选赛排名 | 预选赛最终排名及积分 |
| 友谊赛对手质量 | 对手FIFA排名加权 |

### 球队阵容
| 字段 | 说明 |
|------|------|
| 核心球员（前5） | 姓名、俱乐部、位置、当前状态 |
| 主要伤病 | 伤病球员、预计恢复时间 |
| 旅欧球员数量 | 在五大联赛效力的球员数 |
| 平均年龄 | 全队平均年龄 |
| 大赛经验 | 参加过世界杯的人数 |

### 历史战绩
| 字段 | 说明 |
|------|------|
| 最佳成绩 | 历史最佳世界杯成绩 |
| 近4届成绩 | 2010/2014/2018/2022年成绩 |
| 对主要对手战绩 | 同组/同大洲劲敌的历史交锋 |

---

## 搜索策略

### 搜索关键词模板

```
# 排名和参赛信息
"{队名} FIFA ranking 2026"
"{队名} World Cup 2026 qualified"

# 近期状态
"{队名} 2026 recent results"
"{队名} friendly matches 2026"

# 阵容和伤病
"{队名} 2026 World Cup squad predicted"
"{队名} injury news 2026"
"{核心球员名} injury 2026"

# 赔率
"2026 World Cup odds {队名}"

# 中文搜索（互补）
"2026世界杯 {队名} 阵容"
"{队名} 最新状态 2026"
```

### 优先级顺序

1. **FIFA官网** → 官方排名和分组
2. **ESPN/Transfermarkt** → 阵容和转会数据
3. **当地体育媒体** → 最新动态
4. **维基百科** → 历史数据汇总
5. **中文体育媒体** → 中文球迷视角（新浪/腾讯/虎扑）

---

## 评分录入模板

```yaml
{队名}:
  fifa_rank: N
  recent_form:
    last_6: "W-W-D-W-L-W"
    goals_scored: N
    goals_conceded: N
  squad:
    star_players: 
      - name: "球员名"
        level: S/A/B/C
        injury_status: 健康/带伤/存疑/缺席
    avg_age: N
    overseas_count: N
  world_cup_history:
    best_result: "冠军/亚军/.../未晋级"
    last_4_editions: ["2010: 成绩", "2014: 成绩", "2018: 成绩", "2022: 成绩"]
  coach:
    name: "教练名"
    tenure_years: N
  scores:
    recent_form: 0-20
    fifa_rank: 0-15
    world_cup_history: 0-15
    core_players: 0-12
    attack: 0-10
    defense: 0-10
    adaptation: 0-10
    tactics: 0-8
  total_score: 0-100
  injury_adjustment: N
  final_score: 0-100
```

---

## 数据验证规则

### 交叉验证
- 同一数据至少查询2个独立来源
- FIFA排名以FIFA官网为准
- 伤病信息以官方公告为准（不采用小道消息）

### 数据新鲜度
- FIFA排名：使用最新公布的排名
- 近期战绩：不超过3个月前的比赛
- 伤病信息：不超过1周前的信息

### 标记规则
- 数据可信：✓
- 数据待验证：⚠ TBD
- 无数据：❌ 未知
