# 世界杯实时比赛结果抓取 (analyze_results.py)

2026-06-15 验证完成。从2026世界杯主页面抓取已完成的比赛结果用于赛后对比分析。

## 数据源

- **页面**: `2026 FIFA World Cup` (Wikipedia)
- **API**: `action=parse&prop=text&section=N` (每组分章节拉取)
- **方法**: PowerShell → Windows .NET WebClient → Wikipedia REST API

## 章节索引 (2026年6月验证)

| 组 | 章节 | 已比赛 |
|:---|:----:|:------:|
| A | 20 | ✅ |
| B | 21 | ✅ |
| C | 22 | ✅ |
| D | 23 | ✅ |
| E | 24 | ✅ |
| F | 25 | ✅ |
| G | 26 | 待赛 |
| H | 27 | 待赛 |
| I | 28 | 待赛 |
| J | 29 | 待赛 |
| K | 30 | 待赛 |
| L | 31 | 待赛 |

## HTML表格解析

Wikipedia的Group章节渲染为简单的HTML表格行 (`<tr>`)。核心解析逻辑：

### 比赛行格式 (已完成比赛)

```
<tr>
  <td>Mexico</td>
  <td>2–0</td>
  <td>South Africa</td>
</tr>
```

去掉HTML标签后：
```
Mexico   2–0   South Africa
```

### 比赛行格式 (未进行)

```
Czech Republic   Match 25   South Africa
```
比分位置是"Match N"而非数字，跳过。

### 解析策略

```python
# 1. 提取所有 <tr> 行
rows = re.findall(r'<tr[^>]*>(.*?)</tr>', html, re.DOTALL)

# 2. 清理HTML标签和实体
text = re.sub(r'<[^>]+>', ' ', row)
text = re.sub(r'&[^;]+;', ' ', text)

# 3. 找比分: 数字–数字 (en-dash或连字符)
score_match = re.search(r'(\d+)\s*[–\-]\s*(\d+)', text)

# 4. 比分前后就是队名
parts = re.split(r'\s*\d+\s*[–\-]\s*\d+\s*', text)
t1 = parts[0].strip()
t2 = parts[-1].strip()
```

### 队名映射

Wikipedia HTML中的队名是英文全称（如 "Mexico", "South Africa"），需映射到预测报告中使用的中文名。使用 `TEAM_CN_TO_EN` 字典。

**陷阱**: 部分队名有别名（如 "USA" → "United States", "Bosnia & Herzegovina" → "Bosnia and Herzegovina"）。

## API注意事项

- 如该组无比赛，返回的HTML中可能无`<tr>`行 → 自动跳过
- Wikipedia API无速率限制警告但仍有隐式限流：每请求约0.5-1秒，12组约6-12秒完成
- PowerShell `Write-Output` 对大字符串可能出现断行 → 用 `decode('utf-8', errors='replace')` 处理
- `json.loads` 前检查 `raw.startswith('{"parse"')` 防止编码损坏导致解析失败

## 准确率指标

| 指标 | 公式 | 说明 |
|:----|:-----|:-----|
| 方向正确率 | `correct_winner / n` | 预测胜负方向是否准确（胜率>45%判定） |
| 精确比分 | `exact_score / n` | 四舍五入后比分完全一致 |
| 1球误差 | `within_1_goal / n` | 两队预测进球数与实际的偏差之和≤1 |
| 平均进球误差 | `Σ|pred_g - actual_g| / n` | 每场总进球偏差平均值 |
| Brier Score | `Σ(prob - outcome)² / n` | 概率预测的均方误差（0=完美, 1=最差） |

## 数据流

```
Wikipedia API → 12 Group sections → table rows with scores
    ↓
匹配预测报告 (按组+队名) → comparisons
    ↓
compute_metrics() → accuracy_history.json
    ↓
detect_bias() → bias report
    ↓
compute_weight_adjustments() → weight_suggestions.json
```
