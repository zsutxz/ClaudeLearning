#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
东方财富行情数据源
==================
封装 A 股盘后推荐所需的全部行情抓取：
  - 全市场实时快照（clist/get，一次拿 ~5000 只）
  - 三大指数实时（stock/get）
  - 行业板块涨幅（clist/get, fs=m:90+t:2）
  - 主力资金流排行（clist/get, fid=f62）
  - 个股前复权日 K（kline/get）

网络出口统一走 fetch_powershell()（PowerShell System.Net.WebClient 桥接），
绕过 Windows Git Bash 的网络/证书问题。所有 fetch_* 失败时返回空集合，绝不抛异常。
依赖: Python 3.8+（标准库）。字段含义见 references/data-sources.md。
"""
from __future__ import annotations

import base64
import json
import subprocess
import time
import urllib.request

# Windows 控制台默认 GBK，直接运行本文件自测时强制 utf-8 输出
try:
    import sys
    sys.stdout.reconfigure(encoding="utf-8")
except Exception:
    pass


def fetch_powershell(url: str, retries: int = 2) -> str | None:
    """抓取 URL 返回 UTF-8 文本（双通道）。

    通道一：urllib 直连——快（约 0.2s/次）、无中文乱码，本环境（Windows 原生 Python）首选。
    通道二：PowerShell System.Net.WebClient 桥接——慢（约 7s/次）但可绕过部分网络限制，
            Base64 传输防 PowerShell console 用 GBK 输出导致的中文乱码。仅在 urllib 连续失败时启用。
    网络/超时/HTTP 错误返回 None，不抛异常。
    """
    # 通道一：urllib 直连（首选）
    for _ in range(retries + 1):
        try:
            req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0"})
            with urllib.request.urlopen(req, timeout=15) as r:
                return r.read().decode("utf-8", errors="replace")
        except Exception:
            continue
    # 通道二：PowerShell 兜底
    escaped = url.replace("'", "''")
    ps = (
        "$wc = New-Object System.Net.WebClient;"
        "$wc.Headers.Add('User-Agent','Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36');"
        "try { $b = $wc.DownloadData('" + escaped + "');"
        "      Write-Output ([System.Convert]::ToBase64String($b)) }"
        "catch { Write-Output '__FAIL__' }"
    )
    try:
        r = subprocess.run(
            ["powershell.exe", "-NoProfile", "-Command", ps],
            capture_output=True, text=True, timeout=25,
            encoding="utf-8", errors="replace",
        )
        out = (r.stdout or "").strip()
        if out and out != "__FAIL__":
            try:
                # PowerShell 长输出会在 console 宽度处自动换行，破坏 base64；
                # base64 字母表不含空白，去除所有空白（含换行）后再解码。
                return base64.b64decode("".join(out.split())).decode("utf-8", errors="replace")
            except Exception:
                return None
    except Exception:
        pass
    return None


def to_float(v) -> float | None:
    """东方财富数字字段可能是 '-'（停牌/无数据）/None/空串，安全转 float；无效返回 None。"""
    if v is None or v == "" or v == "-":
        return None
    try:
        return float(v)
    except (TypeError, ValueError):
        return None


def _scale100(v: float | None) -> float | None:
    """东方财富 stock/get 接口价格/涨跌幅为×100 定点整数，需 /100 还原（clist 的 fltt=2 不受影响）。"""
    return v / 100.0 if v is not None else None


def market_of(code: str) -> int:
    """返回东方财富 secid 市场前缀：6 开头（沪市主板/科创板）=1，其余（深市/创业板/北交所）=0。"""
    if code and code.startswith("6"):
        return 1
    return 0


# ==================== 全市场快照 ====================

_SNAPSHOT_FIELDS = "f2,f3,f4,f5,f6,f7,f8,f9,f12,f14,f15,f16,f17,f18,f20,f21,f23"


def fetch_market_snapshot() -> list[dict]:
    """全市场沪深 A 股实时快照。

    东方财富 clist 单页硬上限 100 只，自动翻页拿全部（A 股约 5800 只 ≈ 59 页）。
    返回 [{code,name,market,price,pct_chg,chg,volume(手),amount(元),
           amplitude(%),turnover(%),pe,high,low,open,prev_close,
           total_mv,circ_mv,pb}]；某页失败即停止并返回已拿到的部分（降级，不抛异常）。
    """
    out: list[dict] = []
    page_size = 100
    fail_streak = 0
    for pn in range(1, 81):                 # 安全上限 80 页 = 8000 只
        url = (
            "http://push2.eastmoney.com/api/qt/clist/get"
            f"?pn={pn}&pz={page_size}&po=1&np=1"
            "&ut=bd1d9ddb04089700cf9c27f6f7426751"
            "&fltt=2&invt=2&fid=f3"
            "&fs=m:0+t:6,m:0+t:80,m:1+t:2,m:1+t:23,m:0+t:81+s:2048"
            f"&fields={_SNAPSHOT_FIELDS}"
        )
        txt = fetch_powershell(url, retries=2)
        if not txt:
            fail_streak += 1
            if fail_streak >= 3:            # 连续 3 页失败才停，容忍偶发 502
                break
            time.sleep(0.3)                 # 失败后稍等，缓解东方财富限流
            continue                        # 单页失败：跳过继续下一页
        fail_streak = 0
        time.sleep(0.08)                    # 翻页节流，降低被限流概率
        try:
            diff = json.loads(txt).get("data", {}).get("diff") or []
        except (json.JSONDecodeError, AttributeError, TypeError):
            break
        if not diff:
            break
        for d in diff:
            code = str(d.get("f12", ""))
            if not code:
                continue
            out.append({
                "code": code,
                "name": str(d.get("f14", "")),
                "market": market_of(code),
                "price": to_float(d.get("f2")),
                "pct_chg": to_float(d.get("f3")),
                "chg": to_float(d.get("f4")),
                "volume": to_float(d.get("f5")),       # 手
                "amount": to_float(d.get("f6")),       # 元
                "amplitude": to_float(d.get("f7")),    # 振幅 %
                "turnover": to_float(d.get("f8")),     # 换手率 %
                "pe": to_float(d.get("f9")),
                "high": to_float(d.get("f15")),
                "low": to_float(d.get("f16")),
                "open": to_float(d.get("f17")),
                "prev_close": to_float(d.get("f18")),
                "total_mv": to_float(d.get("f20")),    # 元
                "circ_mv": to_float(d.get("f21")),     # 元
                "pb": to_float(d.get("f23")),
            })
        if len(diff) < page_size:          # 最后一页（不足 100）
            break
    return out


# ==================== 三大指数 ====================

_INDEX_SECIDS = {
    "上证指数": "1.000001",
    "深证成指": "0.399001",
    "创业板指": "0.399006",
}


def fetch_index_overview() -> dict:
    """三大指数实时快照。返回 {名称: {price, pct_chg, amount(元)}}，单只失败则该项缺省。"""
    out: dict = {}
    for name, secid in _INDEX_SECIDS.items():
        url = (
            "http://push2.eastmoney.com/api/qt/stock/get"
            f"?secid={secid}&ut=bd1d9ddb04089700cf9c27f6f7426751"
            "&fields=f43,f48,f58,f60,f170"
        )
        txt = fetch_powershell(url)
        if not txt:
            continue
        try:
            d = json.loads(txt).get("data") or {}
        except (json.JSONDecodeError, AttributeError, TypeError):
            continue
        if not d:
            continue
        out[name] = {
            "price": _scale100(to_float(d.get("f43"))),    # stock/get 价格为×100 定点
            "pct_chg": _scale100(to_float(d.get("f170"))),  # 涨跌幅同上
            "amount": _scale100(to_float(d.get("f48"))),    # 成交额（元）
        }
    return out


# ==================== 行业板块 ====================

def fetch_sector_board(top: int = 30) -> list[dict]:
    """行业板块涨幅排行（fs=m:90+t:2）。返回 [{name, pct_chg, leader}]，按涨幅降序；失败 []。"""
    url = (
        "http://push2.eastmoney.com/api/qt/clist/get"
        f"?pn=1&pz={top}&po=1&np=1"
        "&ut=bd1d9ddb04089700cf9c27f6f7426751"
        "&fltt=2&invt=2&fid=f3&fs=m:90+t:2"
        "&fields=f2,f3,f4,f12,f14,f104,f105,f128,f136"
    )
    txt = fetch_powershell(url)
    if not txt:
        return []
    try:
        diff = json.loads(txt).get("data", {}).get("diff") or []
    except (json.JSONDecodeError, AttributeError, TypeError):
        return []

    out: list[dict] = []
    for d in diff:
        out.append({
            "code": str(d.get("f12", "")),     # 板块代码，如 BK0447（用于拉成分股）
            "name": str(d.get("f14", "")),
            "pct_chg": to_float(d.get("f3")),
            "leader": str(d.get("f128", "")),
        })
    return out


def fetch_sector_constituents(board_code: str, limit: int = 500) -> list[str]:
    """行业板块成分股代码列表（fs=b:{板块代码}）。返回 [code,...]；失败 []。

    用于把个股归属到行业板块，从而计算板块热度因子。
    """
    url = (
        "http://push2.eastmoney.com/api/qt/clist/get"
        f"?pn=1&pz={limit}&po=1&np=1"
        "&ut=bd1d9ddb04089700cf9c27f6f7426751"
        f"&fltt=2&invt=2&fid=f3&fs=b:{board_code}+st:f:!50"
        "&fields=f12"
    )
    txt = fetch_powershell(url)
    if not txt:
        return []
    try:
        diff = json.loads(txt).get("data", {}).get("diff") or []
    except (json.JSONDecodeError, AttributeError, TypeError):
        return []
    return [str(d.get("f12", "")) for d in diff if d.get("f12")]


# ==================== 主力资金流 ====================

def fetch_main_money_flow(top: int = 500) -> dict:
    """主力资金流排行（fid=f62，按主力净流入降序取前 top）。

    返回 {code: {main_net(元), main_pct(%), huge_net(元)}}；失败返回 {}。
    未进入前 top 的股票（多为净流出）资金数据缺失，评分时按中性处理。
    """
    url = (
        "http://push2.eastmoney.com/api/qt/clist/get"
        f"?fid=f62&po=1&pz={top}&pn=1&np=1"
        "&ut=bd1d9ddb04089700cf9c27f6f7426751"
        "&fltt=2&invt=2"
        "&fs=m:0+t:6,m:0+t:80,m:1+t:2,m:1+t:23"
        "&fields=f12,f14,f62,f184,f66,f69,f72,f75,f78"
    )
    txt = fetch_powershell(url)
    if not txt:
        return {}
    try:
        diff = json.loads(txt).get("data", {}).get("diff") or []
    except (json.JSONDecodeError, AttributeError, TypeError):
        return {}

    out: dict = {}
    for d in diff:
        code = str(d.get("f12", ""))
        if not code:
            continue
        out[code] = {
            "main_net": to_float(d.get("f62")),    # 主力净流入额 元
            "main_pct": to_float(d.get("f184")),   # 主力净流入占比 %
            "huge_net": to_float(d.get("f66")),    # 超大单净流入额 元
        }
    return out


# ==================== 个股日 K（前复权） ====================

def fetch_stock_kline(code: str, market: int, days: int = 60) -> dict | None:
    """个股前复权日 K。返回 {dates, opens, closes, highs, lows, volumes(手), amounts(元)} 或 None。

    停牌等无效交易日（close 为空）会被跳过，保证 closes 全为有效 float。
    """
    secid = f"{market}.{code}"
    url = (
        "http://push2his.eastmoney.com/api/qt/stock/kline/get"
        f"?secid={secid}&ut=fa5fd1943c7b386f172d6893dbbd10d0"
        "&fields1=f1,f2,f3,f4,f5,f6"
        "&fields2=f51,f52,f53,f54,f55,f56,f57,f58,f59,f60"
        f"&klt=101&fqt=1&end=20500101&lmt={days}"
    )
    txt = fetch_powershell(url)
    if not txt:
        return None
    try:
        klines = json.loads(txt).get("data", {}).get("klines") or []
    except (json.JSONDecodeError, AttributeError, TypeError):
        return None
    if not klines:
        return None

    dates: list[str] = []
    opens: list[float] = []
    closes: list[float] = []
    highs: list[float] = []
    lows: list[float] = []
    vols: list[float] = []
    amts: list[float] = []
    for line in klines:
        parts = line.split(",")
        if len(parts) < 7:
            continue
        # 日期,开,收,高,低,量,额,振幅,涨跌幅,涨跌额,换手
        c_close = to_float(parts[2])
        if c_close is None:            # 停牌/无效日，跳过
            continue
        c_open = to_float(parts[1]) or c_close
        c_high = to_float(parts[3]) or c_close
        c_low = to_float(parts[4]) or c_close
        dates.append(parts[0])
        opens.append(c_open)
        closes.append(c_close)
        highs.append(c_high)
        lows.append(c_low)
        vols.append(to_float(parts[5]) or 0.0)
        amts.append(to_float(parts[6]) or 0.0)

    if not closes:
        return None
    return {
        "dates": dates, "opens": opens, "closes": closes,
        "highs": highs, "lows": lows, "volumes": vols, "amounts": amts,
    }


if __name__ == "__main__":
    # 自测：抓全市场快照 + 三大指数，打印概览
    print("[自测] 拉取全市场快照...")
    snap = fetch_market_snapshot()
    print(f"  快照股票数: {len(snap)}")
    if snap:
        s = snap[0]
        print(f"  示例: {s['code']} {s['name']} 现价{s['price']} 涨跌幅{s['pct_chg']}% 换手{s['turnover']}%")
    print("[自测] 三大指数:")
    for k, v in fetch_index_overview().items():
        print(f"  {k}: {v.get('price')}  {v.get('pct_chg')}%")
