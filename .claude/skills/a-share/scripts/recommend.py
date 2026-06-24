#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
A 股盘后推荐 主入口
==================
盘后跑一次：抓行情 → 过滤候选池 → 多因子评分 → 输出市场复盘 + 次日推荐 Top 10。
报告写到 {当前工作目录}/docs/a-share/YYYYMMDD.md（不 cd，不污染插件仓）。

用法:
    python3 recommend.py [候选数(默认120)] [Top N(默认10)]
依赖: Python 3.8+（标准库 + 本目录 data_sources / indicators / scoring）
"""
from __future__ import annotations

import os
import sys
from datetime import date

# Windows 控制台默认 GBK，print 中文/数字会 UnicodeEncodeError，强制 utf-8 输出
try:
    sys.stdout.reconfigure(encoding="utf-8")
except Exception:
    pass

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import data_sources as ds  # noqa: E402
import scoring as sc       # noqa: E402

# 所有产物统一写入运行时当前目录的 docs/a-share/
# （SKILL.md 以绝对路径调用本脚本、不 cd，保证 getcwd 为用户目录）
REPORT_DIR = os.path.join(os.getcwd(), "docs", "a-share")

TOP_SECTORS_FOR_HEAT = 12   # 取涨幅前 N 板块拉成分股，构建个股→板块涨幅映射
CANDIDATE_DEFAULT = 120     # 第 3 层拉 K 线的候选数（粗排后）
TOP_DEFAULT = 10            # 输出推荐数


def build_sector_map() -> dict:
    """热门板块 → 成分股，构建 {code: 板块涨幅}。非热门板块的股票不在映射中（sector 因子中性）。"""
    sectors = ds.fetch_sector_board(30)
    mapping: dict = {}
    for sec in sectors[:TOP_SECTORS_FOR_HEAT]:
        pct = sec.get("pct_chg")
        bk = sec.get("code")
        if pct is None or not bk:
            continue
        for code in ds.fetch_sector_constituents(bk):
            if code not in mapping:          # 同股多板块，取最热门板块优先
                mapping[code] = pct
    print(f"  [板块] 热门板块 {min(len(sectors), TOP_SECTORS_FOR_HEAT)} 个 → 覆盖 {len(mapping)} 只个股")
    return mapping


def count_limit(snapshot: list[dict]) -> tuple[int, int]:
    """统计涨停/跌停家数。"""
    up = sum(1 for s in snapshot if sc.is_limit_up(s))
    down = sum(1 for s in snapshot if sc.is_limit_down(s))
    return up, down


def load_previous_report() -> dict | None:
    """读取最近一份历史报告中的推荐清单 {code: name}，用于昨日对比。无则 None。"""
    if not os.path.isdir(REPORT_DIR):
        return None
    today = date.today().strftime("%Y%m%d")
    files = sorted(
        [f for f in os.listdir(REPORT_DIR) if f.endswith(".md")],
        reverse=True,
    )
    for f in files:
        if f.startswith(today):          # 跳过今天的
            continue
        try:
            content = open(os.path.join(REPORT_DIR, f), "r", encoding="utf-8").read()
        except Exception:
            continue
        prev: dict = {}
        for line in content.splitlines():
            parts = [p.strip() for p in line.strip("|").split("|")]
            if len(parts) >= 2 and len(parts[0]) == 6 and parts[0].isdigit():
                prev[parts[0]] = parts[1]
        if prev:
            return prev
    return None


def build_report(today: str, indices: dict, sectors: list[dict],
                 snapshot: list[dict], n_up: int, n_down: int, n_flat: int,
                 limit_up: int, limit_down: int, pool_size: int,
                 candidate_n: int, top: list, prev: dict | None) -> str:
    L: list[str] = []
    L.append(f"# A股盘后推荐 · {today}")
    L.append("")
    L.append("> 本推荐仅为数据模型测算，不构成投资建议。股市有风险，投资需谨慎。")
    L.append("")

    # 一、市场复盘
    L.append("## 一、市场复盘")
    L.append("")
    L.append("| 指数 | 现价 | 涨跌幅 |")
    L.append("|------|------|--------|")
    for name, v in indices.items():
        price = v.get("price")
        pct = v.get("pct_chg")
        L.append(f"| {name} | {f'{price:.2f}' if price else '—'} | "
                 f"{f'{pct:+.2f}%' if pct is not None else '—'} |")
    L.append("")
    L.append(f"全市场 **上涨 {n_up}** / 下跌 {n_down} / 平盘 {n_flat}；"
             f"**涨停 {limit_up}** / 跌停 {limit_down}。")
    L.append("")
    hot = "、".join(f"{s['name']}({s['pct_chg']:+.1f}%)"
                    for s in sectors if s.get("pct_chg") is not None)
    L.append(f"**领涨板块：** {hot[:200]}")
    L.append("")

    # 二、次日推荐
    L.append(f"## 二、次日推荐 Top {len(top)}")
    L.append("")
    if not top:
        L.append("（本期无符合条件的推荐，市场可能整体偏弱或数据异常。）")
    else:
        L.append("| 代码 | 名称 | 综合 | 技术 | 资金 | 板块 | 风险 | 现价 | 涨跌幅 | 推荐理由 |")
        L.append("|------|------|------|------|------|------|------|------|--------|----------|")
        for s, r in top:
            f = r["factors"]
            price = s.get("price")
            pct = s.get("pct_chg")
            reasons = "；".join(r["reasons"][:3]) if r["reasons"] else "—"
            L.append(
                f"| {s['code']} | {s['name']} | **{r['score']}** | "
                f"{f['tech']} | {f['money']} | {f['sector']} | {f['risk']} | "
                f"{f'{price:.2f}' if price else '—'} | "
                f"{f'{pct:+.2f}%' if pct is not None else '—'} | {reasons} |"
            )
    L.append("")

    # 三、候选池统计
    L.append("## 三、候选池统计")
    L.append("")
    L.append(f"- 全市场快照：{len(snapshot)} 只")
    L.append(f"- 过滤后候选池：{pool_size} 只（剔除 ST/涨跌停/亏损/极端换手/低流动性）")
    L.append(f"- 进入评分：前 {candidate_n} 只（粗排后）")
    L.append("")

    # 四、昨日对比
    if prev:
        L.append("## 四、昨日推荐今日表现")
        L.append("")
        hit = [(s, r) for s, r in top if s["code"] in prev]
        if hit:
            for s, r in hit:
                pct = s.get("pct_chg")
                L.append(f"- {s['code']} {s['name']}：今日 "
                         f"{f'{pct:+.2f}%' if pct is not None else '—'}（连续推荐）")
        else:
            L.append("- 昨日推荐今日均未进入本期 Top，注意趋势变化。")
        L.append("")

    # 五、评分说明
    L.append("## 评分说明")
    L.append("")
    L.append("- 综合 = 0.40×技术 + 0.30×资金 + 0.20×板块 − 0.10×风险（0–100，越高越优先）")
    L.append("- 技术：均线/MACD/动能/量比（近 60 日 K 线）；资金：主力净流入；板块：所属行业当日涨幅；风险：追高/振幅/量价背离")
    L.append("- 北向资金实时自 2024-08-19 起停披，资金面以主力净流入代理")
    L.append("")
    L.append("---")
    L.append("*免责声明：以上内容基于公开行情数据的量化测算，不构成任何投资建议。据此操作，风险自负。*")
    return "\n".join(L) + "\n"


def main() -> None:
    today = date.today().strftime("%Y%m%d")
    candidate_n = int(sys.argv[1]) if len(sys.argv) > 1 else CANDIDATE_DEFAULT
    top_n = int(sys.argv[2]) if len(sys.argv) > 2 else TOP_DEFAULT

    print("=" * 56)
    print(f"  A股盘后推荐 · {today}")
    print("=" * 56)

    # 1) 指数 + 板块 + 个股板块归属
    print("\n[1/5] 拉取指数与板块...")
    indices = ds.fetch_index_overview()
    sectors = ds.fetch_sector_board(8)
    sector_map = build_sector_map()

    # 2) 全市场快照 + 涨跌停统计
    print("[2/5] 拉取全市场快照...")
    snapshot = ds.fetch_market_snapshot()
    if not snapshot:
        print("  ⚠ 全市场快照抓取失败（网络/接口异常），终止。请稍后重试。")
        return
    n_up = sum(1 for s in snapshot if (s.get("pct_chg") or 0) > 0)
    n_down = sum(1 for s in snapshot if (s.get("pct_chg") or 0) < 0)
    n_flat = len(snapshot) - n_up - n_down
    limit_up, limit_down = count_limit(snapshot)
    print(f"  快照 {len(snapshot)} 只；上涨{n_up} 下跌{n_down} 平{n_flat}；"
          f"涨停{limit_up} 跌停{limit_down}")

    # 3) 过滤候选池 + 粗排
    print("[3/5] 过滤候选池...")
    pool = sc.filter_candidates(snapshot)
    ranked = sc.preliminary_rank(pool, candidate_n)
    print(f"  候选池 {len(pool)} 只 → 粗排取前 {len(ranked)} 只进入评分")

    # 4) 资金流 + 逐只拉 K 线评分
    print("[4/5] 拉取资金流与 K 线，逐只评分（约 1–2 分钟）...")
    money_map = ds.fetch_main_money_flow(500)
    scored: list = []
    for i, s in enumerate(ranked, 1):
        if i % 20 == 0:
            print(f"  进度 {i}/{len(ranked)}...")
        kline = ds.fetch_stock_kline(s["code"], s["market"], 60)
        money = money_map.get(s["code"])
        sec_pct = sector_map.get(s["code"])
        result = sc.score_stock(s, kline, money, sec_pct)
        if result and result["score"] > 0:
            scored.append((s, result))
    scored.sort(key=lambda x: x[1]["score"], reverse=True)
    top = scored[:top_n]
    print(f"  评分完成，有效 {len(scored)} 只，输出 Top {len(top)}")

    # 5) 昨日对比 + 生成报告
    prev = load_previous_report()
    report = build_report(today, indices, sectors, snapshot,
                          n_up, n_down, n_flat, limit_up, limit_down,
                          len(pool), candidate_n, top, prev)
    os.makedirs(REPORT_DIR, exist_ok=True)
    out_path = os.path.join(REPORT_DIR, f"{today}.md")
    with open(out_path, "w", encoding="utf-8") as f:
        f.write(report)
    print(f"\n[5/5] ✅ 报告已生成: {out_path}")
    print("-" * 56)
    print(report)


if __name__ == "__main__":
    main()
