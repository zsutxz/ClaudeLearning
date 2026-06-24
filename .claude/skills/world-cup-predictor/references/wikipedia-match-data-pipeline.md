# Wikipedia 预选赛数据抓取管道

2026-05-23 验证完成。从6个大洲Wikipedia页面累计抓取818场预选赛比赛数据。

## 管道架构

```
PowerShell → HttpClient → Wikipedia API (action=parse)
    ↓
各页面wikitext/HTML → 正则解析 → 队名映射(FIFA_CODE dict)
    ↓
  每队GF/M + GA/M (大洲归一化)
    ↓
  get_qualifying_rating(team) → (atk, dfn) 用于base_score
```

## 各洲数据格式

| 大洲 | 方法 | 页面 | 解析模式 |
|:----|:-----|:-----|:---------|
| CONMEBOL | `prop=wikitext` | `2026 FIFA World Cup qualification (CONMEBOL)` | `\|team1\s*=\s*\{\{fb-rt\|([^}]+)\}\}\s*\n.*?\|score\s*=\s*(\d+)[–-](\d+)\s*\n.*?\|team2\s*=\s*\{\{fb\|([^}]+)\}\}` |
| UEFA | `prop=wikitext` | `Template:2026 FIFA World Cup qualification – UEFA group tables` | `\|match_([A-Z]+)_([A-Z]+)\s*=\s*\[\[[^\]]*\|(\d+)[–-](\d+)\]\]` |
| CAF | `prop=wikitext` | `Template:2026 FIFA World Cup qualification - CAF group tables` (连字符!非en-dash) | 同上UEFA格式 |
| AFC | `prop=text` | `2026 FIFA World Cup qualification – AFC second/third/fourth round` | `href="#([A-Z]+)_v_([A-Z]+)"[^>]*>\s*(\d+)[–-](\d+)\s*</a>` |
| CONCACAF | `prop=text` | `2026 FIFA World Cup qualification – CONCACAF second/third round` | 同上AFC格式 |
| OFC | `prop=wikitext` | `2026 FIFA World Cup qualification (OFC)` | 同CONMEBOL格式 |

## 队名映射

使用 `FIFA_CODE` 字典将Wikipedia 3字母代码(ARG, BRA, GER等)映射到标准队名(Argentina, Brazil, Germany等)。

包含约200+国家/地区代码。关键: 老牌球队的代码是标准的FIFA 3-letter codes。

## 已知陷阱

- **CAF模板名用连字符**: `Template:2026 FIFA World Cup qualification - CAF group tables` (U+002D `-`) 而非其他大洲使用的 en-dash (U+2013 `–`)
- **UEFA 12组数据全在单一Template页**: 不需分别拉12个子页面！`Template:2026 FIFA World Cup qualification – UEFA group tables` 包含所有组
- **CONCACAF东道主(USA/CAN/MEX)无预选赛数据**: 三东道主直接晋级，无第三轮比赛记录。需从CONCACAF Nations League或友谊赛补充
- **HTTP 429限流**: Wikipedia API限制约10req/分钟。批量拉取需在页面间加延迟或使用单一模板页策略
- **`$HOME` 是PowerShell自动变量**: 在PowerShell循环中勿使用 `$home` 作为变量名 — 它指向C:\Users\username

## 大洲归一化系数

比赛数据来自不同强度的大洲，直接比较场均进球会产生偏差（新西兰6.33GF/M vs 对库克群岛）。

```
UEFA:       ×1.00 (基准)
CONMEBOL:   ×0.95
CAF:        ×0.82
AFC:        ×0.78
CONCACAF:   ×0.73
OFC:        ×0.38
```

这些系数基于历史跨大洲比赛数据（世界杯+联合会杯交叉表现）。
