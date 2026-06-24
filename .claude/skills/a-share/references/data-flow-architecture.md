# 数据流与模块架构

## 模块调用图

```
recommend.py（主入口）
   │
   ├── data_sources.fetch_index_overview()      ──→ push2  三大指数实时
   ├── data_sources.fetch_sector_board()        ──→ push2  行业板块涨幅
   ├── data_sources.fetch_market_snapshot()     ──→ push2  全市场快照（~5000 只）
   │
   ├── scoring.filter_candidates(snapshot)      ──→ 本地过滤 → 候选池
   ├── scoring.preliminary_rank(pool)           ──→ 本地粗排 → 取前 ~120 只
   │
   ├── data_sources.fetch_main_money_flow()     ──→ push2  主力资金流（{code: money}）
   │
   └── for each 候选（≤120）:
         ├── data_sources.fetch_stock_kline()   ──→ push2his 个股 60 日 K
         └── scoring.score_stock(stock, kline, money, sector_map)
                ├── indicators.sma / ema / macd / kdj / rsi / momentum ...
                └── → {score, factors{tech,money,sector,risk}, reasons[]}

   排序 Top 10 → build_report() → docs/a-share/YYYYMMDD.md
```

## 分层抓取（性能核心）

A 股约 5000 只，**禁止**对全市场逐只拉 K 线（5000 次 PowerShell 调用会超时）。采用三层漏斗：

| 层 | 动作 | 请求数 | 说明 |
|----|------|--------|------|
| 1 | 全市场快照 | 1 | 一次拿 5000 只的涨跌幅/换手/量/PE/市值 |
| 2 | 本地过滤 | 0 | 剔除 ST/涨跌停/次新/亏损/极端换手，剩 100–300 只 |
| 2.5 | 本地粗排 | 0 | 按快照数据排序，取前 ~120 只 |
| 3 | 候选 K 线 + 资金 | ≤120 | 仅对候选逐只拉 60 日 K 与资金流 |

资金流排行（`fetch_main_money_flow`）一次拿前 200 只主力净流入，按 code 合并到候选，无需逐只请求。

## 模块职责

- **`data_sources.py`** — 纯数据获取。`fetch_powershell()` 是唯一网络出口（PowerShell 桥接）。所有 `fetch_*` 失败返回空集合，不抛异常。
- **`indicators.py`** — 纯函数，无 IO。输入 K 线序列，输出指标值。可独立单测。
- **`scoring.py`** — 过滤 + 评分。`score_stock()` 是核心，输入一只股票的全部数据，输出总分 + 分项 + 理由。
- **`recommend.py`** — 编排 + 输出。串行调用上述模块，生成 markdown 报告写入 `docs/a-share/`。

## 输出位置

报告写到 `os.getcwd()/docs/a-share/YYYYMMDD.md`（用户工作目录，非插件目录；`docs/` 已 gitignore）。SKILL.md 以绝对路径 `${CLAUDE_PLUGIN_ROOT}` 调用 `recommend.py` 且**不 cd**，保证 `os.getcwd()` 是用户目录。

## 昨日对比

`load_previous_report()` 读取该目录下最近一份报告，解析其中的推荐清单，在今日报告里追加"昨日推荐今日表现"一栏（参考 `world-cup-predictor/scripts/full_prediction.py` 的 `load_previous_report()` 思路）。首次运行无历史则跳过。
