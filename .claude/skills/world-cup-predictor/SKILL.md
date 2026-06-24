---
name: world-cup-predictor
description: |
  世界杯预测、夺冠概率、金靴奖、小组赛比分、赛后分析、明天比赛、球队评分和历史回测。当用户说“世界杯预测 / 预测冠军 / 金靴奖 / 小组赛预测 / 赛后分析 / 明天比赛 / 中国队 / 赛后对比 / 回测”时自动激活。
version: 6.0.0
allowed-tools:
  - Bash
  - Read
  - Write
metadata:
  tags: [world-cup, fifa, football, prediction, 2026, soccer]
---

# 世界杯预测器

面向 2026 FIFA 世界杯的预测 skill。默认输出中文，尽量简洁，优先给结果，不展开模型细节。

## 触发场景

- 用户问世界杯冠军、夺冠概率、金靴奖
- 用户问某组、某队、某场比赛的预测
- 用户问“明天比赛”“赛后分析”“赛后对比”
- 用户问历史回测、模型表现或球队评分

## 执行顺序

1. 运行主引擎：在当前工作目录直接运行（**不 cd**）`python3 "${CLAUDE_PLUGIN_ROOT}/skills/world-cup-predictor/scripts/full_prediction.py"`
2. 读取当天报告：`docs/world-cup-predictor/YYYYMMDD.md`
3. 按问题类型只输出对应内容
4. 需要赛后对比时，再运行 `analyze_results.py`

## 输出规则

- 全部使用简体中文
- 用户问冠军，只给夺冠前 10
- 用户问金靴，只给前 10
- 用户问赛后分析，只比较胜 / 平 / 负方向
- 用户问明天比赛，只输出对阵、比分和方向
- 不解释模型原理，不展开长篇分析

## 参考脚本

- `scripts/full_prediction.py`：主预测流程
- `scripts/analyze_results.py`：赛后对比与准确率统计
- `scripts/compute_ratings.py`：球队评分
- `scripts/translations.py`：中英文映射
- `scripts/backtest_2022.py` / `scripts/backtest_2018.py`：历史回测

## 参考资料

- `references/historical-world-cup-data.md`
- `references/scoring-engine.py`
- `references/scoring-weights.md`
- `references/data-flow-architecture.md`
- `references/backtest-2022.md`
- `references/backtest-2018.md`
- `references/expected-score-display.md`
- `references/odds-api-integration.md`
- `references/qualifying-data-architecture.md`
- `references/live-match-results-parsing.md`
- `references/wikipedia-match-data-pipeline.md`
