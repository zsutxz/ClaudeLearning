# The Odds API 集成

## 注册
- 官网: the-odds-api.com
- 免费套餐: 500次/月, 无需信用卡
- 获取: 注册后Dashboard显示API Key

## API Key
Key 通过环境变量提供（**切勿写入代码**，历史版本曾硬编码并已泄露）：
```
export ODDS_API_KEY="你的 key"   # the-odds-api.com 注册后从 Dashboard 获取
```
未设置时 `full_prediction.py` 会跳过实时赔率校准、使用默认评分。

## 端点

### 夺冠赔率
```
GET https://api.the-odds-api.com/v4/sports/soccer_fifa_world_cup_winner/odds/
  ?apiKey={KEY}&regions=uk&markets=outrights
```
返回Betfair/Bet365/William Hill等博彩公司的夺冠赔率。

### 赛事列表
```
GET https://api.the-odds-api.com/v4/sports/?apiKey={KEY}
```

## 数据优选
- 首选 **Betfair** (交易所, 超额赔付率最低, 最准确)
- 备用: Bet365, William Hill
- 使用归一化隐含概率: `(1/赔率) / Σ(1/所有赔率) × 100%`

## 队名映射
Betfair的队名与内部队名有差异:

| 内部名 | Betfair名 |
|:------|:---------|
| United States | USA |
| Bosnia and Herzegovina | Bosnia & Herzegovina |

映射在 `ODDS_NAME_MAP` 字典中。

## 故障排查
- PowerShell编码: `result.stdout.decode('utf-8', errors='replace')`
- 超时: 设置20秒timeout
- 备用: API失败时使用硬编码默认赔率
