# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

### Git规则
- **不自动提交** - Claude不会自动提交任何代码更改
- 手动提交前需要明确确认

## 🎯 Project Overview
这是一个**多项目技术试验仓库**，包含多个独立的子项目和工具，主要专注于AI应用开发、Unity游戏开发和各种技术创新测试。

## 🏗️ 项目整体架构

### 核心项目结构

#### 1. Unity 金币动画项目 (`Project/`)
- **技术栈**: Unity 2022.3.5f1, C#
- **核心功能**: 极简金币动画系统，仅4个核心文件
- **主要组件**:
  - `BasicCoinAnimation.cs` - 金币动画控制器
  - `SimpleCoinManager.cs` - 金币管理器
  - `BasicCoinDemo.cs` - 演示脚本

#### 2. LangGraph智能学习助手 (`langgraph-agent/`)
- **技术栈**: Python, LangGraph, LangChain, OpenAI/Anthropic API
- **核心功能**: 自动收集技术资料并生成个性化学习方案
- **主要组件**:
  - `main.py` - TechLearningAssistant主程序
  - `src/tech_learning_workflow.py` - LangGraph工作流引擎
  - `agents/` - 研究和学习智能体
  - `tools/` - 网络搜索和内容分析工具
- **依赖**: 15个核心Python包，包括异步处理和学术搜索

#### 3. Claude Agent SDK测试 (`AgentSdkTest/`)
- **技术栈**: Python 3.13, Claude Agent SDK
- **核心功能**: 多模型支持和Claude SDK高级功能测试
- **主要组件**:
  - `Multi_LLm.py` - 统一多模型代理
  - `TestMcp.py` - MCP服务器集成测试
  - `TestTool.py` - 自定义工具测试
  - 支持Claude、OpenAI、DeepSeek、Ollama等模型

#### 4. BMAD框架 (`bmad/`)
- **功能**: 业务模型架构化开发框架
- **结构**: 包含核心(bmm)、构建工具(bmb)、配置(_cfg)等模块
- **用途**: 系统化的业务应用开发框架

### 目录结构
```
E:\AI\ClaudeTest\
├── CLAUDE.md                    # 项目配置文件
├── CLAUDE.local.md              # 用户私有配置
├── Project/                     # Unity项目根目录
│   ├── Assets/
│   │   └── Scripts/
│   │       ├── Animation/
│   │       │   ├── BasicCoinAnimation.cs    # 核心动画控制器
│   │       │   └── SimpleCoinManager.cs     # 金币管理器
│   │       └── Examples/
│   │           └── BasicCoinDemo.cs         # 演示脚本
│   └── ProjectSettings/        # Unity项目设置
├── langgraph-agent/             # LangGraph智能学习助手
│   ├── main.py                  # 主程序入口
│   ├── src/tech_learning_workflow.py  # 工作流引擎
│   ├── agents/                  # 智能体模块
│   ├── tools/                   # 工具模块
│   └── requirements.txt         # 15个AI相关依赖
├── AgentSdkTest/                # Claude Agent SDK测试
│   ├── Multi_LLm.py             # 多模型支持
│   ├── TestMcp.py               # MCP服务器测试
│   ├── TestTool.py              # 自定义工具测试
│   └── requirements.txt         # Claude SDK依赖
├── bmad/                        # BMAD框架
│   ├── bmm/                     # 核心模块
│   ├── bmb/                     # 构建工具
│   └── _cfg/                    # 配置模块
├── Test/                        # 测试和示例文件
│   └── Prompt/                  # Prompt模板文件
│       ├── 单词卡片.md
│       ├── 知识卡片.md
│       └── 信达雅翻译.md
└── .claude/                     # Claude工具和配置
    └── skills/                  # 技能目录
```

## 🛠️ 技术栈概览

### 编程语言
- **C#** (Unity项目)
- **Python 3.13+** (AI和Agent项目)
- **JavaScript/Node.js** (MCP服务器)

### 核心框架和库
- **Unity 2022.3.5f1** - 游戏引擎
- **LangGraph/LangChain** - AI工作流框架
- **Claude Agent SDK** - Anthropic官方SDK
- **OpenAI API** - GPT模型集成
- **MCP (Model Context Protocol)** - 工具服务器协议

## 🚀 常用命令

### Unity项目
```bash
# 打开Unity项目
Unity.exe -projectPath "E:\AI\ClaudeTest\Project"

# 命令行构建
Unity.exe -quit -batchmode -projectPath "E:\AI\ClaudeTest\Project" -buildTarget StandaloneWindows
```

### LangGraph Agent
```bash
cd langgraph-agent
python main.py "Python" --level beginner --hours 30
python examples/basic_usage.py
```

### Claude SDK测试
```bash
cd AgentSdkTest
python AgentSdkStart.py
python Multi_LLm.py
python TestMcp.py
```

### BMAD框架
```bash
# 安装BMAD (需要Node.js环境)
npx bmad-method@alpha install

# 初始化项目
*workflow-init
```

### 开发工作流
1. **启动Unity** → 打开Project目录
2. **创建场景** → 添加SimpleCoinManager组件
3. **创建金币预制体** → 添加BasicCoinAnimation组件
4. **测试功能** → 运行BasicDemo进行验证
5. **AI开发** → 在langgraph-agent或AgentSdkTest中测试AI功能
6. **提交代码** → 手动确认后提交

## 🏗️ Unity金币动画项目详情

### 核心文件结构 (仅4个文件)

1. **BasicCoinAnimation.cs** - 金币动画控制器
   - `MoveTo()` - 移动到目标位置（直线轨迹）
   - `FlyTo()` - 飞行到目标位置（抛物线轨迹）
   - `Collect()` - 收集金币动画
   - `StopAnimation()` - 停止动画
   - `Reset()` - 重置金币状态

2. **SimpleCoinManager.cs** - 金币管理器
   - `CreateCoinAnimation()` - 创建移动动画
   - `CreateFlyAnimation()` - 创建飞行动画
   - `CreateCollectionAnimation()` - 创建收集动画
   - `ClearAllCoins()` - 清理所有金币
   - 内置对象池管理

3. **BasicCoinDemo.cs** - 演示脚本
   - 按键控制：M-移动，F-飞行，C-收集，X-清理
   - GUI界面操作
   - 简单的使用示例

4. **README.md** - 使用文档
   - 详细的使用说明
   - 安装和配置指南

## ⚙️ 开发环境配置

### Unity配置
- **Unity版本**: 2022.3.5f1
- **目标平台**: Windows
- **脚本后端**: Mono
- **API兼容性**: .NET Standard 2.1

### Python环境配置
```bash
# LangGraph项目依赖
cd langgraph-agent
pip install -r requirements.txt  # 15个AI相关包

# Claude SDK测试项目依赖
cd AgentSdkTest
pip install -r requirements.txt  # Claude SDK和多模型支持

# 环境变量配置
# 在各项目目录下创建.env文件，添加对应的API密钥
```

### API配置文件
- `langgraph-agent/.env` - LangGraph项目环境变量
- `AgentSdkTest/.env` - SDK测试环境变量
- `.claude/settings.json` - Claude Code配置，包含MCP服务器

### 项目设置
```csharp
// 推荐的Unity项目设置
- Quality Settings: Balanced
- Scripting Runtime Version: .NET Standard 2.1
- Api Compatibility Level: .NET Standard 2.1
- Allow 'unsafe' Code: Disabled

// Python项目要求
- Python 3.13+ (推荐使用虚拟环境)
- 异步支持: asyncio
- 网络库: aiohttp, requests
```

## 📋 开发指南

### 新项目添加流程
1. 在根目录创建新文件夹
2. 添加相应的`.gitignore`规则
3. 创建项目特定的README和配置
4. 更新根目录CLAUDE.md（如需要）

### Git工作流
```bash
# 查看状态
git status

# 添加文件
git add .

# 提交更改（需手动确认）
git commit -m "commit message"

# 查看更改
git diff
```

### 多项目管理技巧
1. **环境隔离**: 每个项目使用独立的Python虚拟环境
2. **API密钥管理**: 通过.env文件管理，避免硬编码
3. **配置同步**: 保持各子项目文档与代码同步
4. **依赖管理**: 定期更新Python依赖，特别是AI相关包

## 📝 代码规范

### C#代码风格
```csharp
// 1. 命名规范
public class SimpleCoinManager : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;
    private readonly List<GameObject> activeCoins = new List<GameObject>();

    // 2. 方法命名：PascalCase
    public void CreateCoinAnimation(Vector3 startPos, Vector3 targetPos)
    {
        // 3. 变量命名：camelCase
        GameObject coin = GetCoinFromPool();
        BasicCoinAnimation animation = coin.GetComponent<BasicCoinAnimation>();

        // 4. 注释规范
        animation.MoveTo(targetPos, 1.0f);
    }
}

// 5. 接口命名以I开头
public interface ICoinAnimation
{
    void MoveTo(Vector3 target, float duration);
}
```

### 代码组织原则
- **单一职责** - 每个类只负责一个功能
- **极简设计** - 避免过度抽象
- **性能优先** - 使用对象池，避免频繁GC
- **清晰命名** - 代码即文档

### 性能规范
```csharp
// ✅ 推荐：使用对象池
private GameObject GetCoinFromPool()
{
    if (coinPool.Count > 0)
    {
        GameObject coin = coinPool.Dequeue();
        coin.SetActive(true);
        return coin;
    }
    return Instantiate(coinPrefab);
}

// ❌ 避免：频繁Instantiate/Destroy
// GameObject coin = Instantiate(coinPrefab);
// Destroy(coin, 2.0f);
```

## ⚡ 使用方法

### 基础用法
```csharp
// 1. 在场景中添加 SimpleCoinManager 组件
// 2. 设置金币预制体（只需要 BasicCoinAnimation 组件）

// 创建移动动画
coinManager.CreateCoinAnimation(startPos, targetPos);

// 创建飞行动画（带抛物线轨迹）
coinManager.CreateFlyAnimation(startPos, targetPos);

// 创建收集动画
coinManager.CreateCollectionAnimation(startPos, collectPoint);

// 清理所有金币
coinManager.ClearAllCoins();
```

### 直接使用动画组件
```csharp
// 获取金币动画组件
BasicCoinAnimation coin = coinObject.GetComponent<BasicCoinAnimation>();

// 移动金币（直线轨迹）
coin.MoveTo(targetPosition, 1f);

// 飞行金币（抛物线轨迹）
coin.FlyTo(targetPosition, 1.5f);

// 收集金币
coin.Collect(collectionPoint, 0.5f);

// 停止动画
coin.StopAnimation();
```

## 🎮 演示控制

### 按键操作
- **M** - 演示移动动画（直线轨迹）
- **F** - 演示飞行动画（抛物线轨迹）
- **C** - 演示收集动画
- **X** - 清理所有金币

### GUI操作
- 点击界面按钮执行对应操作

## ✨ 项目特性

- **极简设计** - 只有4个核心文件，代码简洁
- **零依赖** - 不需要任何外部插件或包
- **高性能** - 基于Unity协程，支持50+并发金币
- **易使用** - 简单的API，一行代码创建动画
- **对象池** - 内置高效的对象池管理
- **跨平台** - 支持所有Unity平台

## 🚀 快速开始

### 1. 创建金币预制体
```csharp
// 在Unity编辑器中：
1. 创建3D物体（如Sphere）
2. 添加 BasicCoinAnimation 组件
3. 调整大小和材质
4. 保存为预制体
```

### 2. 设置场景
```csharp
// 在Unity编辑器中：
1. 在场景中创建空物体
2. 添加 SimpleCoinManager 组件
3. 将金币预制体拖入 Coin Prefab 字段
4. 设置Max Coins参数（建议50-100）
```

### 3. 运行演示
```csharp
// 在Unity编辑器中：
1. 添加 BasicCoinDemo 组件到场景
2. 设置生成点和目标点
3. 运行场景，使用按键或GUI操作
```

## 📋 开发历史

### 简化历程

**原项目 (70+ 文件)**:
- 复杂的状态机和事件系统
- 性能监控和内存管理
- 多平台兼容性验证
- 自适应质量调整
- 大量测试文件和编辑器工具

**极简版 (4 文件)**:
- 只保留核心动画功能
- 移除所有复杂特性
- 代码量减少 95%
- 维护成本大幅降低

## 🎯 项目特色

### 1. **多技术栈融合**
- 游戏开发 (Unity C#)
- AI应用开发 (Python LangGraph)
- 智能代理集成 (Claude SDK)

### 2. **完善的配置管理**
- 环境变量分离
- MCP服务器集成
- Git钩子系统

### 3. **丰富的测试覆盖**
- 单元测试
- 集成测试
- 性能测试

### 4. **模块化设计**
- 每个子项目独立
- 清晰的接口定义
- 可重用组件

## 💡 最佳实践

### Unity开发建议
1. **金币预制体**: 只需要 `BasicCoinAnimation` 组件
2. **对象池**: 让 `SimpleCoinManager` 自动管理
3. **性能**: 避免同时创建过多金币（建议 < 100个）
4. **动画**: 使用内置的缓动效果，无需自定义

### AI项目开发建议
1. **环境隔离**: 每个项目使用独立的Python虚拟环境
2. **API密钥安全**: 确保所有API密钥通过环境变量管理
3. **异步编程**: 充分利用asyncio提升性能
4. **错误处理**: 实现完善的重试和错误恢复机制

### 多项目管理技巧
1. **统一配置**: 在根目录维护统一的开发规范
2. **文档同步**: 保持各子项目文档与代码同步
3. **依赖管理**: 定期更新Python依赖，特别是AI相关包
4. **版本控制**: 使用Git子模块或独立仓库管理大型依赖

## 🔧 故障排除

### Unity项目常见问题

**问题**: 金币不显示
- 检查预制体是否正确设置
- 确认 SimpleCoinManager 的 Coin Prefab 字段已赋值
- 检查场景摄像机位置

**问题**: 动画不流畅
- 减少同时活动的金币数量
- 检查目标位置是否合理
- 确认Unity Quality Settings设置合适

### AI项目常见问题

**问题**: LangGraph Agent运行失败
- 检查.env文件中的API密钥配置
- 验证网络连接和防火墙设置
- 确认Python虚拟环境正确激活

**问题**: Claude SDK连接错误
- 验证ANTHROPIC_API_KEY是否正确设置
- 检查Claude Agent SDK版本兼容性
- 确认网络代理设置（如需要）

### 通用问题

**问题**: 依赖包冲突
- 使用Python虚拟环境隔离项目依赖
- 检查requirements.txt版本兼容性
- 清理pip缓存并重新安装

**问题**: Git提交问题
- 遵循"不自动提交"规则
- 手动确认代码更改后再提交
- 检查.gitignore规则是否正确

## 📚 扩展开发

### Unity扩展
```csharp
// 在BasicCoinAnimation.cs中添加新动画类型
public void SpiralTo(Vector3 target, float duration)
{
    StartCoroutine(SpiralAnimation(target, duration));
}
```

### AI项目扩展
```python
# 在langgraph-agent中添加新的搜索源
async def search_new_source(self, query: str) -> List[Dict[str, Any]]:
    # 实现新搜索源逻辑
    pass
```

### MCP服务器扩展
```json
// 在.mcp.json中添加新的服务器配置
{
  "new_server": {
    "command": "python",
    "args": ["-m", "new_mcp_server"]
  }
}
```

## 🏆 项目质量保证

### 代码审查清单
- [ ] 代码符合命名规范
- [ ] 方法职责单一
- [ ] 性能优化合理
- [ ] 注释清晰准确
- [ ] 错误处理完善
- [ ] API密钥安全隔离
- [ ] 依赖版本兼容

### 测试要求
- [ ] 基础功能测试
- [ ] 性能压力测试
- [ ] 边界条件测试
- [ ] 用户场景测试
- [ ] API集成测试
- [ ] 多模块协作测试

---

*多项目技术试验仓库 - 专注技术创新，涵盖Unity游戏开发、AI应用和智能代理系统*