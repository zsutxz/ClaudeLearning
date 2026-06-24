#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
技术指标（纯标准库，无 IO）
============================
输入为 K 线序列（list[float]），输出单值或最新指标值。所有函数数据不足时返回 None。
被 scoring.score_stock() 调用。依赖: Python 3.8+（标准库）。
"""
from __future__ import annotations


def sma(values: list[float], n: int) -> float | None:
    """简单移动平均，返回最近 n 期的均值；数据不足或 n<=0 返回 None。"""
    if n <= 0 or len(values) < n:
        return None
    return sum(values[-n:]) / n


def ema_series(values: list[float], n: int) -> list[float]:
    """指数移动平均序列（与 values 等长）。用首值作种子递推，长序列下误差可忽略。"""
    if not values:
        return []
    k = 2 / (n + 1)
    out = [values[0]]
    for i in range(1, len(values)):
        out.append(values[i] * k + out[-1] * (1 - k))
    return out


def macd(closes: list[float], fast: int = 12, slow: int = 26, signal: int = 9
         ) -> tuple[float, float, float] | None:
    """MACD，返回最近一个 (dif, dea, hist)；数据不足返回 None。

    hist = 2*(DIF-DEA) 为通达信红绿柱写法。hist>0 且 dif>dea 视为多头。
    """
    if len(closes) < slow + signal:
        return None
    ema_fast = ema_series(closes, fast)
    ema_slow = ema_series(closes, slow)
    dif = [f - s for f, s in zip(ema_fast, ema_slow)]
    dea = ema_series(dif, signal)
    d, e = dif[-1], dea[-1]
    hist = 2 * (d - e)
    return d, e, hist


def kdj(highs: list[float], lows: list[float], closes: list[float],
        n: int = 9, m1: int = 3, m2: int = 3) -> tuple[float, float, float] | None:
    """KDJ，返回最近 (k, d, j)；数据不足返回 None。采用通达信 SMA 递推，初值 50。"""
    length = min(len(highs), len(lows), len(closes))
    if length < n:
        return None
    rsvs: list[float] = []
    for i in range(n - 1, length):
        hh = max(highs[i - n + 1:i + 1])
        ll = min(lows[i - n + 1:i + 1])
        c = closes[i]
        rsvs.append(50.0 if hh == ll else (c - ll) / (hh - ll) * 100)
    k = d = 50.0
    for rsv in rsvs:
        k = (m1 - 1) / m1 * k + rsv / m1
        d = (m2 - 1) / m2 * d + k / m2
    j = 3 * k - 2 * d
    return k, d, j


def rsi(closes: list[float], n: int = 6) -> float | None:
    """RSI(n)，返回 0–100；数据不足返回 None。简单 n 期平均法。"""
    if len(closes) < n + 1:
        return None
    gains = losses = 0.0
    for i in range(-n, 0):
        diff = closes[i] - closes[i - 1]
        if diff > 0:
            gains += diff
        else:
            losses += -diff
    avg_loss = losses / n
    if avg_loss == 0:
        return 100.0
    rs = (gains / n) / avg_loss
    return 100 - 100 / (1 + rs)


def momentum(closes: list[float], n: int) -> float | None:
    """n 个交易日前的涨幅 %（现价相对 n 日前收盘）。数据不足返回 None。"""
    if len(closes) < n + 1:
        return None
    base = closes[-1 - n]
    if not base:
        return None
    return (closes[-1] - base) / base * 100


def volatility(closes: list[float], n: int) -> float | None:
    """近 n 日日收益率标准差（%），衡量波动；数据不足返回 None。"""
    if len(closes) < n + 1:
        return None
    rets: list[float] = []
    for i in range(-n, 0):
        prev = closes[i - 1]
        if prev:
            rets.append((closes[i] - prev) / prev)
    if len(rets) < 2:
        return None
    mean = sum(rets) / len(rets)
    var = sum((r - mean) ** 2 for r in rets) / len(rets)
    return var ** 0.5 * 100


def volume_ratio(volumes: list[float], n: int = 5) -> float | None:
    """量比 = 最近一日成交量 / 前 n 日平均成交量。数据不足返回 None。"""
    if len(volumes) < n + 1:
        return None
    today = volumes[-1]
    avg = sum(volumes[-1 - n:-1]) / n
    if not avg:
        return None
    return today / avg


if __name__ == "__main__":
    # 自测：构造一段上升序列，验证指标
    demo = [10.0 + i * 0.2 for i in range(60)]
    print("sma5:", sma(demo, 5))
    print("macd:", macd(demo))
    print("kdj :", kdj([x + 0.5 for x in demo], [x - 0.5 for x in demo], demo))
    print("rsi :", rsi(demo))
    print("mom20:", momentum(demo, 20))
    print("vol :", volatility(demo, 20))
