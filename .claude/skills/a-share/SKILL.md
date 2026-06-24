---
name: a-share
description: |
  A股推荐、选股、盘后复盘、次日推荐清单、涨停、龙虎榜、主力资金、板块轮动、个股评分与持仓参考。当用户说"A股推荐 / 帮我选股 / 明天买什么 / 今天复盘 / 盘后复盘 / 次日推荐 / 选股打分 / 板块轮动"时自动激活。
version: 1.0.0
allowed-tools:
  - Bash
  - Read
  - Write
metadata:
  tags: [a-share, stock, china, recommendation, screening, finance]
---

# A股盘后推荐

面向沪深 A 股的盘后选股 skill。默认输出中文，尽量简洁，优先给结果，不展开模型细节。抓取真实行情 → 多因子打分 → 输出市场复盘 + 次日推荐 Top 10，报告落盘到 `docs/a-share/YYYYMMDD.md`。

## 触发场景

- 用户说"A股推荐 / 帮我选股 / 明天买什么 / 盘后复盘 / 次日推荐 / 选股打分"等。
- 用户想看今日市场复盘、领涨板块、涨停情况。
- 用户想要一个**可解释**的选股清单（带评分理由），而不是单一内幕消息。

不激活：询问美股/港股/加密货币、询问某只具体股票的基本面（直接回答即可，不必跑全市场流程）、纯粹查实时报价。

## 执行顺序

1. 在当前工作目录直接运行（**不 cd**），用绝对路径调用主脚本：

   ```bash
   python "${CLAUDE_PLUGIN_ROOT}/skills/a-share/scripts/recommend.py"
   ```

   （Windows 上 `python3` 常被 Microsoft Store 别名劫持，故用 `python`；Linux/Mac 若 `python` 指向 Python 2 则改用 `python3`。）
   可选参数：第一个为候选数（默认 120），第二个为输出 Top N（默认 10）。数据源为东方财富免费接口，无需 key、无需 pip 依赖。

2. 脚本依次：抓三大指数 + 板块 → 全市场快照 → 过滤候选池 → 逐只拉 K 线与资金流评分（约 1–2 分钟）。
3. 跑完后读取生成的 `docs/a-share/YYYYMMDD.md`，向用户复述核心结论。

## 输出规则

- 默认直接复述报告：市场复盘（指数 / 涨跌家数 / 涨停 / 领涨板块）+ 次日推荐 Top 10。
- 每只推荐都带综合评分与 1–3 条理由；用户问细节时再展开。
- **必须**保留风险免责声明，不预测具体涨跌目标价，只给参考。
- 脚本抓取失败（断网 / 接口异常）时，如实告知"数据获取失败，稍后重试"，**不编造数据**。

## 参考脚本

- `scripts/recommend.py` — 主入口（编排 + 报告生成 + 昨日对比）
- `scripts/data_sources.py` — 东方财富接口封装（PowerShell 桥接，全市场快照 / K 线 / 资金流 / 板块）
- `scripts/indicators.py` — 技术指标（MA / MACD / KDJ / RSI / 量比，纯标准库）
- `scripts/scoring.py` — 候选池过滤 + 多因子评分（技术动能 / 资金面 / 板块热度 / 风险）

## 参考资料

- `references/data-sources.md` — 东方财富接口字段 / URL 与降级策略
- `references/scoring-factors.md` — 评分因子定义与权重（可调）
- `references/data-flow-architecture.md` — 模块调用图与分层抓取
