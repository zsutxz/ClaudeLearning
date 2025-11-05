# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

### Git规则
- **不自动提交** - Claude不会自动提交任何代码更改
- 手动提交前需要明确确认

## 🎯 Project Overview
这是一个 **Unity 2022.3.5f1** 项目，实现了**极简**金币动画系统，使用纯Unity协程。**删除所有复杂功能**，只保留最核心的金币移动和收集动画功能。总共只有 **4个核心文件**。

## 🏗️ 项目架构

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

### 目录结构
```
D:\work\AI\ClaudeTest\
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
├── Test/                       # 测试和示例文件
│   └── Prompt/                 # Prompt模板文件
│       ├── 单词卡片.md
│       ├── 知识卡片.md
│       └── 信达雅翻译.md
└── .claude/                    # Claude工具和配置
    └── skills/                 # 技能目录
```

## ⚙️ 开发环境配置

### Unity配置
- **Unity版本**: 2022.3.5f1
- **目标平台**: Windows
- **脚本后端**: Mono
- **API兼容性**: .NET Standard 2.1

### 项目设置
```csharp
// 推荐的Unity项目设置
- Quality Settings: Balanced
- Scripting Runtime Version: .NET Standard 2.1
- Api Compatibility Level: .NET Standard 2.1
- Allow 'unsafe' Code: Disabled
```

## 🚀 常用命令

### Unity编辑器操作
```bash
# 打开项目
Unity.exe -projectPath "D:\work\AI\ClaudeTest\Project"

# 构建项目
Unity.exe -quit -batchmode -projectPath "D:\work\AI\ClaudeTest\Project" -buildTarget StandaloneWindows -executeMethod BuildCommand.Build
```

### Git操作
```bash
# 初始化仓库
git init

# 添加文件
git add .

# 提交更改
git commit -m "commit message"

# 查看状态
git status

# 查看更改
git diff
```

### 开发工作流
1. **启动Unity** → 打开Project目录
2. **创建场景** → 添加SimpleCoinManager组件
3. **创建金币预制体** → 添加BasicCoinAnimation组件
4. **测试功能** → 运行BasicDemo进行验证
5. **提交代码** → 手动确认后提交

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

## 💡 最佳实践

### 开发建议
1. **金币预制体**: 只需要 `BasicCoinAnimation` 组件
2. **对象池**: 让 `SimpleCoinManager` 自动管理
3. **性能**: 避免同时创建过多金币（建议 < 100个）
4. **动画**: 使用内置的缓动效果，无需自定义

### 常见陷阱
```csharp
// ❌ 错误：忘记设置预制体引用
// SimpleCoinManager的Coin Prefab字段未赋值

// ❌ 错误：同时创建过多金币
for(int i = 0; i < 1000; i++) {
    coinManager.CreateCoinAnimation(start, target);
}

// ✅ 正确：合理控制金币数量
for(int i = 0; i < 50; i++) {
    coinManager.CreateCoinAnimation(start, target);
}
```

## 🔧 故障排除

### 常见问题

**问题**: 金币不显示
- 检查预制体是否正确设置
- 确认 SimpleCoinManager 的 Coin Prefab 字段已赋值
- 检查场景摄像机位置

**问题**: 动画不流畅
- 减少同时活动的金币数量
- 检查目标位置是否合理
- 确认Unity Quality Settings设置合适

**问题**: 收集动画无效果
- 确认收集点位置设置正确
- 检查动画时长参数
- 验证目标点是否在合理范围内

**问题**: 对象池异常
- 检查金币预制体是否包含BasicCoinAnimation组件
- 确认Max Coins设置合理
- 检查是否有脚本执行错误

## 📚 扩展开发

### 添加新动画类型
```csharp
// 在BasicCoinAnimation.cs中添加新方法
public void SpiralTo(Vector3 target, float duration)
{
    StartCoroutine(SpiralAnimation(target, duration));
}

private IEnumerator SpiralAnimation(Vector3 target, float duration)
{
    // 实现螺旋动画逻辑
}
```

### 自定义缓动函数
```csharp
// 添加自定义缓动效果
private float CustomEase(float t)
{
    return t * t * (3.0f - 2.0f * t); // SmoothStep
}
```

## 🏆 项目质量保证

### 代码审查清单
- [ ] 代码符合命名规范
- [ ] 方法职责单一
- [ ] 性能优化合理
- [ ] 注释清晰准确
- [ ] 错误处理完善

### 测试要求
- [ ] 基础功能测试
- [ ] 性能压力测试
- [ ] 边界条件测试
- [ ] 用户场景测试

---
*极简金币动画系统 - 专注核心功能，拒绝过度工程化*