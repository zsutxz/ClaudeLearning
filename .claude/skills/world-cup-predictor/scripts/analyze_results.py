#!/usr/bin/env python3
"""
2026世界杯 — 赛后分析与参数自适应修正
=========================================
每日运行：获取实际比赛结果 → 对比昨日预测 → 计算准确率 → 检测系统偏差 → 修正参数

Usage:
    python3 analyze_results.py                   # 分析+输出到报告
    python3 analyze_results.py --auto-adjust     # 分析+自动修正参数
    python3 analyze_results.py --date 20260611   # 指定日期分析

依赖: PowerShell (Windows WebClient), Python 3.8+
"""
import json, re, os, sys, math, subprocess
from datetime import date, timedelta

SKILL_DIR = os.path.dirname(os.path.abspath(__file__))
# 所有产物统一写入运行时当前目录的 docs/world-cup-predictor/（SKILL.md 以绝对路径调用本脚本、不 cd）
REPORTS_DIR = os.path.join(os.getcwd(), "docs", "world-cup-predictor")
os.makedirs(REPORTS_DIR, exist_ok=True)
ACCURACY_LOG = os.path.join(REPORTS_DIR, "accuracy_history.json")
PARAM_FILE = os.path.join(SKILL_DIR, "param_overrides.json")

# 中文→英文队名 (与 translations.py 保持一致)
TEAM_CN_TO_EN = {
    "阿根廷": "Argentina", "法国": "France", "西班牙": "Spain", "英格兰": "England",
    "巴西": "Brazil", "荷兰": "Netherlands", "葡萄牙": "Portugal", "比利时": "Belgium",
    "德国": "Germany", "克罗地亚": "Croatia", "乌拉圭": "Uruguay", "瑞士": "Switzerland",
    "哥伦比亚": "Colombia", "墨西哥": "Mexico", "日本": "Japan", "摩洛哥": "Morocco",
    "美国": "United States", "塞内加尔": "Senegal", "瑞典": "Sweden", "伊朗": "Iran",
    "韩国": "South Korea", "澳大利亚": "Australia", "奥地利": "Austria", "土耳其": "Turkey",
    "厄瓜多尔": "Ecuador", "埃及": "Egypt", "加纳": "Ghana", "挪威": "Norway",
    "巴拉圭": "Paraguay", "加拿大": "Canada", "苏格兰": "Scotland", "捷克": "Czech Republic",
    "南非": "South Africa", "佛得角": "Cape Verde", "沙特阿拉伯": "Saudi Arabia", "伊拉克": "Iraq",
    "阿尔及利亚": "Algeria", "突尼斯": "Tunisia", "科特迪瓦": "Ivory Coast",
    "刚果民主共和国": "DR Congo", "乌兹别克斯坦": "Uzbekistan", "波黑": "Bosnia and Herzegovina",
    "约旦": "Jordan", "新西兰": "New Zealand", "巴拿马": "Panama", "卡塔尔": "Qatar",
    "海地": "Haiti", "库拉索": "Curaçao",
}

# 英文别名 → 标准英文名
TEAM_ALIAS = {
    "USA": "United States", "Bosnia & Herzegovina": "Bosnia and Herzegovina",
    "Bosnia-Herzegovina": "Bosnia and Herzegovina",
    "Côte d'Ivoire": "Ivory Coast", "Korea Republic": "South Korea",
    "DR Congo": "DR Congo",
}

# 12组 → Wikipedia章节index (Group A=20, B=21, ... L=31)
GROUP_SECTIONS = {chr(65+i): 20+i for i in range(12)}  # A=20, B=21, ..., L=31


def ps_fetch(url):
    """通过PowerShell从Windows侧访问网络"""
    cmd = [
        "powershell.exe", "-Command", "&{",
        "$wc = New-Object System.Net.WebClient;",
        "$wc.Headers.Add('User-Agent', 'Mozilla/5.0 (compatible; HermesBot/1.0)');",
        f"$r = $wc.DownloadString('{url}');",
        "Write-Output $r;", "}"
    ]
    try:
        result = subprocess.run(cmd, capture_output=True, timeout=30)
        if result.returncode == 0 and len(result.stdout) > 0:
            text = result.stdout.decode('utf-8', errors='replace')
            return text
    except Exception as e:
        print(f"  [⚠️] PowerShell请求失败: {e}")
    return None


def normalize_team(name):
    """统一队名格式"""
    name = name.strip()
    if name in TEAM_CN_TO_EN:
        return TEAM_CN_TO_EN[name]
    if name in TEAM_ALIAS:
        return TEAM_ALIAS[name]
    # 尝试子串匹配
    return name


def fetch_all_match_results():
    """
    从Wikipedia 2026世界杯页面获取所有已完成的比赛结果。
    遍历12个组(Groups A-L)，每个组的章节抓取比赛表格。
    
    返回: [(group, team1, score1, score2, team2), ...]
    """
    print("  [分析] 正在从Wikipedia获取所有比赛结果...")
    
    matches = []
    
    for group_letter in "ABCDEFGHIJKL":
        section_idx = GROUP_SECTIONS[group_letter]
        url = (f"https://en.wikipedia.org/w/api.php?action=parse"
               f"&page=2026_FIFA_World_Cup&prop=text&section={section_idx}&format=json")
        raw = ps_fetch(url)
        if not raw:
            print(f"  [⚠️] 无法获取{group_letter}组数据(raw为空)")
            continue
        
        # Debug: 检查原始数据
        if not raw.startswith('{"parse"'):
            print(f"  [⚠️] {group_letter}组数据格式异常: {raw[:50]}")
            continue
        
        try:
            data = json.loads(raw)
            html = data.get("parse", {}).get("text", {}).get("*", "")
        except (json.JSONDecodeError, KeyError, TypeError) as e:
            print(f"  [⚠️] {group_letter}组JSON解析失败: {e}, 跳过")
            continue
        
        # 解析比赛行: `TeamA � N–M � TeamB`
        # Wikipedia HTML: team name → score(数字–数字) → team name
        # 未进行的比赛: `TeamA � Match XX � TeamB` (没有数字比分)
        
        # 提取所有 <tr> 行
        rows = re.findall(r'<tr[^>]*>(.*?)</tr>', html, re.DOTALL)
        
        for row in rows:
            # 清理HTML标签
            text = re.sub(r'<[^>]+>', ' ', row)
            text = re.sub(r'&[^;]+;', ' ', text)
            text = re.sub(r'\s+', ' ', text).strip()
            
            # 找比分模式: 数字–数字 (即比赛已进行)
            score_match = re.search(r'(\d+)\s*[–\-]\s*(\d+)', text)
            if not score_match:
                continue
            
            s1, s2 = int(score_match.group(1)), int(score_match.group(2))
            
            # 提取队名: 在比分前后的单词
            # 格式: Team1 score1–score2 Team2
            parts = re.split(r'\s*\d+\s*[–\-]\s*\d+\s*', text)
            if len(parts) >= 2:
                t1_raw = parts[0].strip()
                t2_raw = parts[-1].strip()
                
                # 清理(主队标记)等
                t1_raw = re.sub(r'\s*\([^)]*\)', '', t1_raw).strip()
                t2_raw = re.sub(r'\s*\([^)]*\)', '', t2_raw).strip()
                
                # 跳过非队名行（如只包含数字的行、排名行）
                if t1_raw and t2_raw and not t1_raw.isdigit() and not t2_raw.isdigit():
                    t1 = normalize_team(t1_raw)
                    t2 = normalize_team(t2_raw)
                    if t1 != t1_raw or t2 != t2_raw:
                        matches.append((group_letter, t1, s1, s2, t2))
                    else:
                        matches.append((group_letter, t1_raw, s1, s2, t2_raw))
    
    # 去重
    seen = set()
    unique_matches = []
    for m in matches:
        key = (m[0], m[1], m[3], m[2], m[4])  # 同时检查正反向
        key2 = (m[0], m[4], m[3], m[2], m[1])
        if key not in seen and key2 not in seen:
            seen.add(key)
            unique_matches.append(m)
    
    print(f"  [分析] 找到 {len(unique_matches)} 场已完成比赛")
    return unique_matches


def load_prediction(date_str):
    """
    加载指定日期的预测报告。
    返回预测的比赛字典: {match_key: {...}}
    """
    report_path = os.path.join(REPORTS_DIR, f"{date_str}.md")
    if not os.path.exists(report_path):
        print(f"  [分析] 未找到 {date_str} 的预测报告")
        return {}
    
    print(f"  [分析] 加载预测报告: {report_path}")
    
    predictions = {}
    current_group = None
    
    with open(report_path, "r", encoding="utf-8") as f:
        for line in f:
            # 检测组名: ### A组
            gm = re.match(r'^###\s*([A-Z])组', line)
            if gm:
                current_group = gm.group(1)
            
            # 比赛行: | 墨西哥 | 南非 | 2.3:1.2 | 60% | 18% | 22% |
            mm = re.match(r'^\|\s*(.+?)\s*\|\s*(.+?)\s*\|\s*([\d.]+):([\d.]+)\s*\|\s*(\d+)%\s*\|\s*(\d+)%\s*\|\s*(\d+)%', line)
            if mm and current_group:
                t1_cn = mm.group(1).strip()
                t2_cn = mm.group(2).strip()
                pred_g1 = float(mm.group(3))
                pred_g2 = float(mm.group(4))
                t1_wp = float(mm.group(5))
                dp = float(mm.group(6))
                t2_wp = float(mm.group(7))
                
                t1_en = TEAM_CN_TO_EN.get(t1_cn, t1_cn)
                t2_en = TEAM_CN_TO_EN.get(t2_cn, t2_cn)
                
                key = (current_group, t1_en, t2_en)
                predictions[key] = {
                    "t1_cn": t1_cn, "t2_cn": t2_cn,
                    "t1_en": t1_en, "t2_en": t2_en,
                    "pred_g1": pred_g1, "pred_g2": pred_g2,
                    "t1_wp": t1_wp, "dp": dp, "t2_wp": t2_wp,
                }
    
    print(f"  [分析] 找到 {len(predictions)} 场预测")
    return predictions


def match_results_to_comparisons(all_matches, predictions):
    """
    将实际比赛结果与预测相匹配。
    
    返回: [(t1_cn, t2_cn, pred_g1, pred_g2, act_g1, act_g2, t1_wp, dp, t2_wp), ...]
    """
    comparisons = []
    matched_keys = set()
    
    for grp, t1, s1, s2, t2 in all_matches:
        # 在预测中查找匹配
        for key, pred in predictions.items():
            if key in matched_keys:
                continue
            p_grp, p_t1, p_t2 = key
            p_t1_en, p_t2_en = pred["t1_en"], pred["t2_en"]
            
            # 检查正向和反向匹配
            if (t1 == p_t1_en and t2 == p_t2_en):
                comparisons.append((pred["t1_cn"], pred["t2_cn"],
                                   pred["pred_g1"], pred["pred_g2"],
                                   s1, s2, pred["t1_wp"], pred["dp"], pred["t2_wp"]))
                matched_keys.add(key)
                break
            elif (t1 == p_t2_en and t2 == p_t1_en):
                # 队名反了，反转分数
                comparisons.append((pred["t2_cn"], pred["t1_cn"],
                                   pred["pred_g2"], pred["pred_g1"],
                                   s1, s2, pred["t2_wp"], pred["dp"], pred["t1_wp"]))
                matched_keys.add(key)
                break
    
    # 报告未匹配的预测
    unmatched = set(predictions.keys()) - matched_keys
    if unmatched:
        # 这些预测的比赛尚未进行(或未找到数据)
        pass
    
    print(f"  [分析] 成功匹配 {len(comparisons)} 场")
    return comparisons


def compute_metrics(comparisons):
    """计算预测准确率指标"""
    n = len(comparisons)
    if n == 0:
        return {"n": 0}
    
    correct_winner = 0
    correct_score = 0
    correct_1goal = 0
    total_goal_error = 0
    total_score_error = 0
    brier_sum = 0
    
    for t1, t2, pg1, pg2, ag1, ag2, t1_wp, dp, t2_wp in comparisons:
        # 实际结果
        if ag1 > ag2: actual_winner = "t1"
        elif ag2 > ag1: actual_winner = "t2"
        else: actual_winner = "draw"
        
        # 预测结果 (基于胜率 > 45% 判断)
        if t1_wp > 45: pred_winner = "t1"
        elif t2_wp > 45: pred_winner = "t2"
        else: pred_winner = "draw"
        
        if pred_winner == actual_winner:
            correct_winner += 1
        
        # 精确比分
        if round(pg1) == ag1 and round(pg2) == ag2:
            correct_score += 1
        
        # 1球误差内
        g_error = abs(round(pg1) - ag1) + abs(round(pg2) - ag2)
        if g_error <= 1:
            correct_1goal += 1
        
        total_goal_error += abs(pg1 - ag1) + abs(pg2 - ag2)
        total_score_error += abs((pg1 - pg2) - (ag1 - ag2))
        
        # Brier Score
        if actual_winner == "t1":
            brier = (t1_wp/100 - 1)**2 + (dp/100 - 0)**2 + (t2_wp/100 - 0)**2
        elif actual_winner == "t2":
            brier = (t1_wp/100 - 0)**2 + (dp/100 - 0)**2 + (t2_wp/100 - 1)**2
        else:
            brier = (t1_wp/100 - 0)**2 + (dp/100 - 1)**2 + (t2_wp/100 - 0)**2
        brier_sum += brier
    
    return {
        "n": n,
        "correct_winner": correct_winner,
        "correct_winner_pct": correct_winner / n * 100,
        "correct_score": correct_score,
        "correct_score_pct": correct_score / n * 100,
        "correct_1goal": correct_1goal,
        "correct_1goal_pct": correct_1goal / n * 100,
        "avg_goal_error": total_goal_error / n,
        "avg_score_error": total_score_error / n,
        "avg_brier": brier_sum / n,
    }


def detect_bias(comparisons):
    """检测系统性偏差"""
    if not comparisons:
        return {"has_data": False}
    
    biases = {"has_data": True}
    
    total_pred_goals = 0
    total_actual_goals = 0
    team_stats = {}
    
    for t1, t2, pg1, pg2, ag1, ag2, t1_wp, dp, t2_wp in comparisons:
        total_pred_goals += pg1 + pg2
        total_actual_goals += ag1 + ag2
        
        for team, pred_g, actual_g in [(t1, pg1, ag1), (t2, pg2, ag2)]:
            if team not in team_stats:
                team_stats[team] = {"pred": 0, "actual": 0, "n": 0}
            team_stats[team]["pred"] += pred_g
            team_stats[team]["actual"] += actual_g
            team_stats[team]["n"] += 1
    
    biases["goal_bias"] = total_pred_goals - total_actual_goals
    biases["goal_over_pct"] = (total_pred_goals / max(1, total_actual_goals) - 1) * 100
    
    # 各队偏差
    team_biases = []
    for team, data in team_stats.items():
        if data["n"] > 0:
            bias = data["pred"] / data["n"] - data["actual"] / data["n"]
            team_biases.append((team, bias, data))
    
    team_biases.sort(key=lambda x: x[1], reverse=True)
    biases["worst_overrated"] = team_biases[:3]
    biases["worst_underrated"] = team_biases[-3:] if team_biases else []
    
    # 热门/冷门准确率
    correct_fav = 0
    total_fav = 0
    for t1, t2, pg1, pg2, ag1, ag2, t1_wp, dp, t2_wp in comparisons:
        if t1_wp > 50:
            total_fav += 1
            if ag1 > ag2: correct_fav += 1
        elif t2_wp > 50:
            total_fav += 1
            if ag2 > ag1: correct_fav += 1
        elif dp > 40:
            total_fav += 1
            if ag1 == ag2: correct_fav += 1
    
    biases["fav_accuracy"] = correct_fav / max(1, total_fav) * 100
    biases["total_fav_games"] = total_fav
    
    return biases


def save_analysis(pred_date, comparisons, metrics, biases):
    """保存分析结果到报告文件"""
    today = date.today().strftime("%Y%m%d")
    analysis_path = os.path.join(REPORTS_DIR, f"{today}.md")
    
    analysis_section = "\n\n---\n"
    analysis_section += f"## 📊 赛后对比分析\n"
    analysis_section += f"> 对比 {pred_date} 预测 vs 实际结果 | 分析时间: {date.today()}\n\n"
    
    if metrics.get("n", 0) == 0:
        analysis_section += "今日无比赛可对比分析。\n"
    else:
        analysis_section += f"### 📈 准确率指标 (共 {metrics['n']} 场)\n\n"
        analysis_section += f"| 指标 | 数值 |\n|:----|:----:|\n"
        analysis_section += f"| ✅ 方向正确 | {metrics['correct_winner']}/{metrics['n']} ({metrics['correct_winner_pct']:.1f}%) |\n"
        analysis_section += f"| 🎯 精确比分 | {metrics['correct_score']}/{metrics['n']} ({metrics['correct_score_pct']:.1f}%) |\n"
        analysis_section += f"| 📏 1球误差内 | {metrics['correct_1goal']}/{metrics['n']} ({metrics['correct_1goal_pct']:.1f}%) |\n"
        analysis_section += f"| 📊 平均进球误差 | {metrics['avg_goal_error']:.2f} 球/场 |\n"
        analysis_section += f"| 📐 平均分差误差 | {metrics['avg_score_error']:.2f} |\n"
        analysis_section += f"| 🧮 Brier Score | {metrics['avg_brier']:.4f} (越低越好) |\n\n"
        
        # 逐场对比表
        analysis_section += "### ⚔️ 逐场对比\n\n"
        analysis_section += "| 主队 | 客队 | 预测 | 实际 | 方向 |\n|:----|:----|:----:|:----:|:----:|\n"
        
        for t1, t2, pg1, pg2, ag1, ag2, t1_wp, dp, t2_wp in comparisons:
            if ag1 > ag2: actual_winner = "t1"
            elif ag2 > ag1: actual_winner = "t2"
            else: actual_winner = "draw"
            if t1_wp > 45: pred_winner = "t1"
            elif t2_wp > 45: pred_winner = "t2"
            else: pred_winner = "draw"
            correct_mark = "✅" if pred_winner == actual_winner else "❌"
            analysis_section += f"| {t1} | {t2} | {pg1:.1f}:{pg2:.1f} | {ag1}:{ag2} | {correct_mark} |\n"
        
        analysis_section += "\n"
        
        # 偏差分析
        if biases.get("has_data"):
            analysis_section += "### 🔍 系统偏差分析\n\n"
            goal_bias = biases.get("goal_bias", 0)
            if goal_bias > 0:
                analysis_section += f"- 📈 预测总进球比实际多 **{goal_bias:.1f}球** ({biases.get('goal_over_pct', 0):.1f}%高估)\n"
            elif goal_bias < 0:
                analysis_section += f"- 📉 预测总进球比实际少 **{abs(goal_bias):.1f}球** ({abs(biases.get('goal_over_pct', 0)):.1f}%低估)\n"
            else:
                analysis_section += f"- ➡️ 预测总进球与实际持平\n"
            
            analysis_section += f"- 🎯 热门队胜率准确率: {biases.get('fav_accuracy', 0):.1f}% ({biases.get('total_fav_games', 0)}场)\n"
            
            worst_over = biases.get("worst_overrated", [])
            if worst_over:
                analysis_section += "- 📛 最被高估球队:\n"
                for team, bias, data in worst_over:
                    analysis_section += f"  - {team}: 预测场均{data['pred']/data['n']:.2f}球 vs 实际{data['actual']/data['n']:.2f}球 (高估{bias:+.2f})\n"
            
            worst_under = biases.get("worst_underrated", [])
            if worst_under:
                analysis_section += "- ✅ 最被低估球队:\n"
                for team, bias, data in reversed(worst_under):
                    analysis_section += f"  - {team}: 预测场均{data['pred']/data['n']:.2f}球 vs 实际{data['actual']/data['n']:.2f}球 (低估{abs(bias):.2f})\n"
    
    # 追加到今日预测报告
    if os.path.exists(analysis_path):
        with open(analysis_path, "a", encoding="utf-8") as f:
            f.write(analysis_section)
        print(f"  [分析] ✅ 分析已追加到 {analysis_path}")
    else:
        analysis_path = os.path.join(REPORTS_DIR, f"{today}_analysis.md")
        with open(analysis_path, "w", encoding="utf-8") as f:
            f.write(f"# 赛后对比分析\n> 对比 {pred_date} 预测 vs 实际结果\n")
            f.write(analysis_section)
        print(f"  [分析] ✅ 分析已保存到 {analysis_path}")
    
    # 保存准确率历史
    save_accuracy_history(pred_date, metrics, biases)


def save_accuracy_history(pred_date, metrics, biases):
    """保存准确率历史"""
    history = {"entries": [], "version": "1.0"}
    if os.path.exists(ACCURACY_LOG):
        try:
            with open(ACCURACY_LOG, "r") as f:
                history = json.load(f)
        except:
            pass
    
    entry = {
        "date": pred_date,
        "analysis_date": date.today().strftime("%Y%m%d"),
        "n": metrics.get("n", 0),
        "correct_winner_pct": round(metrics.get("correct_winner_pct", 0), 1),
        "correct_score_pct": round(metrics.get("correct_score_pct", 0), 1),
        "avg_goal_error": round(metrics.get("avg_goal_error", 0), 2),
        "avg_brier": round(metrics.get("avg_brier", 0), 4),
        "goal_bias": round(biases.get("goal_bias", 0), 1),
        "fav_accuracy": round(biases.get("fav_accuracy", 0), 1),
    }
    
    # 检查是否已有同日期记录, 有则覆盖
    for i, e in enumerate(history["entries"]):
        if e["date"] == pred_date:
            history["entries"][i] = entry
            break
    else:
        history["entries"].append(entry)
    
    with open(ACCURACY_LOG, "w") as f:
        json.dump(history, f, indent=2, ensure_ascii=False)
    print(f"  [历史] ✅ 准确率已保存 ({len(history['entries'])}天)")


def compute_weight_adjustments(history):
    """基于历史准确率数据计算权重调整建议"""
    if not history or len(history) < 3:
        return {"adjust": False, "reason": "数据不足(需≥3天)"}
    
    entries = [e for e in history["entries"] if e["n"] > 0]
    if len(entries) < 3:
        return {"adjust": False, "reason": f"仅有{len(entries)}天有效数据(需≥3天)"}
    
    recent = entries[-5:]
    
    avg_winner = sum(e["correct_winner_pct"] for e in recent) / len(recent)
    avg_goal_error = sum(e["avg_goal_error"] for e in recent) / len(recent)
    avg_fav = sum(e["fav_accuracy"] for e in recent if e.get("total_fav_games", 0) > 0) / max(1, sum(1 for e in recent if e.get("total_fav_games", 0) > 0))
    
    adjustments = {
        "adjust": True,
        "accuracy_trend": {
            "avg_winner_acc": round(avg_winner, 1),
            "avg_goal_error": round(avg_goal_error, 2),
            "avg_fav_accuracy": round(avg_fav, 1),
        },
        "suggestions": [],
    }
    
    if avg_winner < 50:
        adjustments["suggestions"].append({
            "type": "weight_shift",
            "reason": f"方向准确率{avg_winner:.0f}%偏低，建议调整权重"
        })
    
    total_bias = sum(e.get("goal_bias", 0) for e in recent)
    if total_bias > 3:
        adjustments["suggestions"].append({
            "type": "goal_adjustment",
            "target": "lower_avg_goals",
            "reason": f"近{len(recent)}天进球高估{total_bias:.0f}球，建议降低avg_goals"
        })
    elif total_bias < -3:
        adjustments["suggestions"].append({
            "type": "goal_adjustment",
            "target": "raise_avg_goals",
            "reason": f"近{len(recent)}天进球低估{abs(total_bias):.0f}球，建议提高avg_goals"
        })
    
    suggestions_path = os.path.join(REPORTS_DIR, "weight_suggestions.json")
    with open(suggestions_path, "w") as f:
        json.dump(adjustments, f, indent=2, ensure_ascii=False)
    
    return adjustments


def print_summary(metrics, biases, adjustments=None):
    """打印分析摘要"""
    if metrics.get("n", 0) == 0:
        print("\n  📊 分析结果: 无比赛可分析")
        return
    
    print(f"\n  {'='*50}")
    print(f"  📊 赛后分析摘要 ({metrics['n']}场)")
    print(f"  {'='*50}")
    print(f"  方向准确率: {metrics['correct_winner_pct']:.1f}% ({metrics['correct_winner']}/{metrics['n']})")
    print(f"  精确比分:   {metrics['correct_score_pct']:.1f}%")
    print(f"  1球误差内:  {metrics['correct_1goal_pct']:.1f}%")
    print(f"  平均进球误差: {metrics['avg_goal_error']:.2f} 球/场")
    print(f"  Brier Score: {metrics['avg_brier']:.4f}")
    
    goal_bias = biases.get("goal_bias", 0)
    if goal_bias > 0:
        print(f"  📈 进球高估: +{goal_bias:.1f}球")
    elif goal_bias < 0:
        print(f"  📉 进球低估: {goal_bias:.1f}球")
    
    if adjustments and adjustments.get("adjust"):
        if adjustments.get("suggestions"):
            print(f"\n  🔧 参数调整建议:")
            for s in adjustments["suggestions"]:
                print(f"     • {s['reason']}")


def main():
    # 确定分析目标日期 (= 昨天的预测)
    if "--date" in sys.argv:
        idx = sys.argv.index("--date") + 1
        if idx < len(sys.argv):
            target_date = sys.argv[idx]
        else:
            print("错误: --date 需要参数 YYYYMMDD")
            sys.exit(1)
    else:
        yesterday = date.today() - timedelta(days=1)
        target_date = yesterday.strftime("%Y%m%d")
    
    auto_adjust = "--auto-adjust" in sys.argv
    
    print(f"\n  🔍 2026世界杯赛后分析 v1.0")
    print(f"  {'='*50}")
    print(f"  对比预测日期: {target_date}")
    
    # 步骤1: 获取所有已完成比赛结果
    all_matches = fetch_all_match_results()
    if not all_matches:
        print(f"\n  [分析] ❌ 未获取到比赛结果")
        return
    
    # 步骤2: 加载预测报告
    predictions = load_prediction(target_date)
    if not predictions:
        print(f"  [分析] ❌ 未加载到预测数据")
        return
    
    # 步骤3: 匹配实际结果与预测
    comparisons = match_results_to_comparisons(all_matches, predictions)
    if not comparisons:
        print(f"  [分析] 预测中的比赛尚未完成")
        return
    
    # 步骤4: 计算准确率
    metrics = compute_metrics(comparisons)
    
    # 步骤5: 检测偏差
    biases = detect_bias(comparisons)
    
    # 步骤6: 保存分析
    save_analysis(target_date, comparisons, metrics, biases)
    
    # 步骤7: 读取历史并计算调整建议
    history = None
    if os.path.exists(ACCURACY_LOG):
        try:
            with open(ACCURACY_LOG, "r") as f:
                history = json.load(f)
        except:
            pass
    
    adjustments = compute_weight_adjustments(history)
    
    # 步骤8: 输出摘要
    print_summary(metrics, biases, adjustments)
    
    print(f"\n  [分析] ✅ 完成")


if __name__ == "__main__":
    main()
