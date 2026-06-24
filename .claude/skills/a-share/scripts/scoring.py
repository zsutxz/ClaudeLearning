#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
多因子评分引擎
==============
职责：
  1. filter_candidates()   —— 从全市场快照剔除 ST/涨跌停/亏损/极端换手/低流动性
  2. preliminary_rank()    —— 候选池粗排，取前 ~120 只进入第 3 层拉 K 线（避免全市场逐只抓）
  3. score_stock()         —— 单只多因子综合打分（技术动能+资金面+板块热度−风险）

总分公式与因子定义见 references/scoring-factors.md。依赖: Python 3.8+（标准库 + 本目录 indicators.py）。
"""
from __future__ import annotations

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import indicators as ind  # noqa: E402  (本地模块，需先把脚本目录加入 path)

# ==================== 候选池过滤阈值 ====================
MIN_AMOUNT = 5e7        # 成交额下限（元）= 5000 万，剔除流动性差的票
MIN_TURNOVER = 0.3      # 换手率下限（%），剔除僵死股
MAX_TURNOVER = 30.0     # 换手率上限（%），剔除纯投机

# ==================== 评分权重（可调） ====================
WEIGHTS = {"tech": 0.40, "money": 0.30, "sector": 0.20, "risk": 0.10}


# ==================== 涨跌停判断 ====================

def is_limit_up(stock: dict) -> bool:
    """是否涨停。主板 ~10%、创业板/科创板 ~20%、北交所 ~30%。"""
    pct = stock.get("pct_chg")
    code = stock.get("code", "")
    if pct is None:
        return False
    if code.startswith("30") or code.startswith("68"):   # 创业板 / 科创板 20%
        return pct >= 19.5
    if code.startswith("8") or code.startswith("4"):     # 北交所 30%
        return pct >= 29.5
    return pct >= 9.8                                     # 主板 10%


def is_limit_down(stock: dict) -> bool:
    """是否跌停（统一用 ≤ -9.8% 近似，双创 -20% 亦被覆盖）。"""
    pct = stock.get("pct_chg")
    return pct is not None and pct <= -9.8


def is_st(stock: dict) -> bool:
    """名称含 ST / *ST / 退 的视为 ST/退市。"""
    name = stock.get("name", "")
    return "ST" in name or "退" in name


# ==================== 候选池过滤 ====================

def filter_candidates(snapshot: list[dict]) -> list[dict]:
    """剔除 ST/退市、涨跌停、亏损、极端换手、流动性差、价格异常的股票，返回候选池。"""
    out: list[dict] = []
    for s in snapshot:
        if is_st(s):
            continue
        price = s.get("price")
        if not price or price <= 0:
            continue
        if is_limit_up(s) or is_limit_down(s):
            continue
        pe = s.get("pe")
        if pe is not None and pe < 0:                 # 动态市盈率为负 = 亏损
            continue
        turnover = s.get("turnover")
        if turnover is None:
            continue
        if turnover < MIN_TURNOVER or turnover > MAX_TURNOVER:
            continue
        amount = s.get("amount")
        if amount is None or amount < MIN_AMOUNT:
            continue
        out.append(s)
    return out


def preliminary_rank(pool: list[dict], limit: int = 120) -> list[dict]:
    """候选池粗排：综合活跃度（涨幅截顶 + 换手）降序，取前 limit 只进入第 3 层拉 K 线。

    涨幅封顶 8%——偏好「涨了但没涨停、有一定换手」的活跃票，避免高位票与死水股扎堆。
    """
    def key(s: dict) -> float:
        pct = min(s.get("pct_chg") or 0.0, 8.0)
        turn = min(s.get("turnover") or 0.0, 15.0)
        return pct + turn * 0.2

    return sorted(pool, key=key, reverse=True)[:limit]


# ==================== 多因子打分 ====================

def score_stock(stock: dict, kline: dict | None, money: dict | None,
                sector_pct: float | None) -> dict | None:
    """对单只股票多因子打分。

    参数:
      stock      —— 全市场快照里的该股 dict（含 price/pct_chg/turnover/amplitude 等）
      kline      —— fetch_stock_kline() 的结果或 None
      money      —— 主力资金流 dict {main_net, main_pct, huge_net} 或 None
      sector_pct —— 该股所属行业板块当日涨幅（%），无归属则 None

    返回 {score(0-100), factors{tech,money,sector,risk}, reasons[...]}。
    数据严重缺失（无快照关键字段）返回 None。
    """
    if not stock.get("price"):
        return None

    reasons: list[str] = []

    # ---------- 技术动能 ----------
    # 有 K 线则从 0 起累加各项（满分 100）；无 K 线/次新置中性偏低 40。
    if kline and len(kline["closes"]) >= 20:
        tech = 0.0
        closes = kline["closes"]
        ma5 = ind.sma(closes, 5)
        ma10 = ind.sma(closes, 10)
        ma20 = ind.sma(closes, 20)
        ma60 = ind.sma(closes, 60)
        price = closes[-1]

        if ma5 and ma10 and ma20 and ma5 > ma10 > ma20:
            tech += 25
            reasons.append("均线多头排列(MA5>MA10>MA20)")
        if ma20 and price > ma20:
            tech += 15
        if ma60 and price > ma60:
            tech += 10
        m = ind.macd(closes)
        if m:
            dif, dea, hist = m
            if dif > dea and hist > 0:
                tech += 20
                reasons.append("MACD多头(红柱)")
        mom20 = ind.momentum(closes, 20)
        if mom20 is not None and 5 <= mom20 <= 25:
            tech += 15
            reasons.append(f"近20日涨{mom20:.1f}%趋势健康")
        vr = ind.volume_ratio(kline["volumes"], 5)
        if vr and vr > 1.2:
            tech += 15
            reasons.append(f"放量(量比{vr:.1f})")
    else:
        tech = 40.0  # 无 K 线 / 次新，技术面未知，中性偏低
    tech = max(0, min(100, tech))

    # ---------- 资金面 ----------
    money_score = 40.0  # 默认中性（资金数据缺失）
    if money:
        main_net = money.get("main_net") or 0.0
        main_pct = money.get("main_pct") or 0.0
        huge_net = money.get("huge_net") or 0.0
        up = (stock.get("pct_chg") or 0) > 0
        if main_net > 0:
            money_score = 30.0
            reasons.append(f"主力净流入{main_net / 1e8:.2f}亿")
            if main_pct > 3:
                money_score += 25
            if huge_net > 0:
                money_score += 20
            if up:
                money_score += 25       # 量价齐升
        else:
            money_score = 20.0          # 主力净流出
    money_score = max(0, min(100, money_score))

    # ---------- 板块热度 ----------
    sector = 50.0  # 默认中性（未归属到热门板块）
    if sector_pct is not None:
        if sector_pct >= 3:
            sector = 90 + min(10, (sector_pct - 3) * 2)
            reasons.append(f"所属板块涨{sector_pct:.1f}%强势")
        elif sector_pct >= 1:
            sector = 60 + (sector_pct - 1) * 15
        elif sector_pct >= 0:
            sector = 40 + sector_pct * 20
        else:
            sector = max(0, 40 + sector_pct * 10)
    sector = max(0, min(100, sector))

    # ---------- 风险（减分项） ----------
    risk = 0.0
    if kline and len(kline["closes"]) >= 6:
        m5 = ind.momentum(kline["closes"], 5)
        if m5 is not None:
            if m5 > 30:
                risk += 40
                reasons.append(f"5日涨{m5:.0f}%偏高(追高风险)")
            elif m5 > 20:
                risk += 20
    amp = stock.get("amplitude")
    if amp and amp > 8:
        risk += 20
    if money and (money.get("main_net") or 0) < 0 and (stock.get("pct_chg") or 0) > 1:
        risk += 25
        reasons.append("量价背离(涨但主力流出)")
    turn = stock.get("turnover")
    if turn and turn > 25:
        risk += 15
    risk = min(100, risk)

    # ---------- 总分 ----------
    w = WEIGHTS
    total = w["tech"] * tech + w["money"] * money_score + w["sector"] * sector - w["risk"] * risk
    total = max(0, min(100, total))

    return {
        "score": round(total, 1),
        "factors": {
            "tech": round(tech, 1),
            "money": round(money_score, 1),
            "sector": round(sector, 1),
            "risk": round(risk, 1),
        },
        "reasons": reasons,
    }


if __name__ == "__main__":
    # 自测：构造一只均线多头的强势股评分
    fake_stock = {"code": "600519", "name": "贵州茅台", "price": 1700.0,
                  "pct_chg": 3.2, "turnover": 0.8, "amplitude": 4.0, "pe": 25.0}
    closes = [100 + i * 1.0 for i in range(60)]
    fake_kline = {"dates": ["d"] * 60, "opens": closes, "closes": closes,
                  "highs": [c + 2 for c in closes], "lows": [c - 2 for c in closes],
                  "volumes": [10000] * 59 + [15000], "amounts": [0] * 60}
    fake_money = {"main_net": 3e8, "main_pct": 5.0, "huge_net": 1e8}
    r = score_stock(fake_stock, fake_kline, fake_money, 3.5)
    import json as _j
    print(_j.dumps(r, ensure_ascii=False, indent=2))
