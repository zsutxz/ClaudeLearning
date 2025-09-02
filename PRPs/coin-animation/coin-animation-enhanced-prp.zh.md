## 目标
创建一个增强版的 Unity 金币飞行物对象池动画系统，该系统使用 DOTween，使金币图标 `icon02.png` 从起始位置平滑地飞向结束位置，并带有流畅的缓动效果，最终打包成一个可供方便复用的预制件（Prefab）。

## 为什么
- 通过视觉上吸引人的金币收集动画来增强游戏的用户体验。
- 提供可轻松实例化的可复用预制件组件。
- 实现对象池，以高效管理多个金币动画，避免性能下降。
- 展示在 Unity 中结合对象池模式的高级 DOTween 实现方法。
- 创建可扩展的动画系统，用于处理大量并发的金币特效。

## 做什么
- 使用 DOTween 的 Unity 金币飞行物动画预制件。
- 用于高效管理金币生命周期的对象池系统。
- 包含移动、旋转和缩放效果的平滑飞行物动画。
- 可配置的动画时长、路径和效果参数。
- 兼容 URP（通用渲染管线）的实现。

### 成功标准
- [ ] 金币飞行物动画作为一个可实例化的预制件正常工作。
- [ ] 对象池系统高效地管理金币的生命周期。
- [ ] 动画包含移动、旋转和缩放效果。
- [ ] 参数可在 Unity Inspector 中配置。
- [ ] 系统能处理多个并发的金币动画。
- [ ] 在大量金币的情况下没有内存泄漏或性能下降。
- [ ] 代码遵循 Unity 最佳实践和 DOTween 约定。
- [ ] 实现与 URP 兼容。

## 所有需要的上下文

### 文档与参考资料
```yaml
# 必读 - 将这些包含在你的上下文窗口中
- url: https://dotween.demigiant.com/documentation.php
  why: DOTween 官方文档，用于查阅缓动方法和参数。

- url: https://docs.unity3d.com/Manual/URP.html
  why: 理解 URP 集成和渲染管线。

- url: https://docs.unity3d.com/Manual/ObjectPooling.html
  why: Unity 推荐的对象池实现方法。

- file: D:\work\AI\ClaudeTest\UnityProject\Assets\Resources\icon02.png
  why: 需要制作动画的金币图像资源。
```

### 当前代码库结构
```bash
.
├── UnityProject/
│   ├── Assets/
│   │   ├── Resources/
│   │   │   └── icon02.png          # 金币图像资源
│   │   ├── Scenes/
│   │   │   └── EnhancedRayTracingScene.unity
│   │   ├── Scripts/
│   │   │   ├── CoinAnimation/      # 金币动画脚本目录
│   │   │   │   ├── CoinAnimationController.cs
│   │   │   │   ├── CoinAnimationDemo.cs
│   │   │   │   ├── CoinPoolManager.cs (待创建)
│   │   │   │   └── Coin.prefab (待创建)
│   │   │   ├── PerformanceMonitor.cs
│   │   │   └── RayTracingController.cs
│   │   ├── Materials/
│   │   ├── Shaders/
│   │   └── ...
│   └── Packages/
│       └── manifest.json           # 项目依赖 (需要添加 DOTween)
```

### 期望的代码库结构（包含待添加文件及其职责）
```bash
UnityProject/
  - Assets/Scripts/CoinAnimation/Coin.cs:
    - 处理单个金币动画的金币组件。
    - 实现飞行效果的 DOTween 序列。
    - 动画完成时自我停用。

  - Assets/Scripts/CoinAnimation/CoinPoolManager.cs:
    - 金币预制件的对象池管理器。
    - 从池中获取/返还金币的方法。
    - 池的初始化和管理。

  - Assets/Scripts/CoinAnimation/CoinAnimationSystem.cs:
    - 用于生成和控制金币动画的高级系统。
    - 作为其他系统触发动画的集成点。

  - Assets/Prefabs/Coin.prefab:
    - 包含金币精灵和动画组件的预制件。
    - 可在场景中直接实例化。

PRPs/coin-animation/coin-animation-enhanced-prp.md:
  - 增强版金币动画实现的完整 PRP 规范。
  - 包含对象池的实现蓝图。
  - 验证关卡和最佳实践。
```

### 已知的代码库陷阱和库的怪癖
```csharp
// 关键：DOTween 需要通过包管理器或手动安装导入。
// 关键：DOTween 序列需要被正确地终止/销毁以防止内存泄漏。
// 关键：对象池需要仔细管理对象状态。
// 关键：URP 材质可能需要特定的着色器设置才能正确渲染。
// 关键：没有正确设置的情况下，动画可能无法在编辑模式下工作。
// 关键：DOTween 在使用前需要初始化 (DOTween.Init())。
// 关键：为 DOTween 使用安全模式以防止已销毁对象出现问题。
// 关键：预制件需要正确维护对组件的引用。
```

## 实现蓝图

### 数据模型和结构
```csharp
// 金币动画配置参数
[System.Serializable]
public struct CoinAnimationParams
{
    public float duration;              // 动画时长
    public Vector3 startPosition;       // 金币起始位置
    public Vector3 endPosition;         // 金币结束位置
    public bool enableRotation;         // 是否启用旋转效果
    public float rotationSpeed;         // 旋转速度（如果启用）
    public bool enableScaling;          // 是否启用缩放效果
    public float startScale;            // 金币起始缩放
    public float endScale;              // 金币结束缩放
    public Ease easeType;               // 动画的缓动函数
};

// 单个金币组件
public class Coin : MonoBehaviour
{
    public CoinAnimationParams animationParams;  // 动画参数
    private Sequence animationSequence;          // DOTween 动画序列
    private Action<Coin> returnToPoolCallback;   // 将金币返还到池的回调
};

// 金币的对象池管理器
public class CoinPoolManager : MonoBehaviour
{
    public GameObject coinPrefab;                // 要池化的预制件
    public int initialPoolSize = 10;            // 初始创建的金币数量
    private Queue<Coin> coinPool;               // 可用金币的池
    private List<Coin> activeCoins;             // 当前活动中的金币
};

// 高级动画系统
public class CoinAnimationSystem : MonoBehaviour
{
    public CoinPoolManager poolManager;         // 对池管理器的引用
    public void SpawnCoinAnimation(CoinAnimationParams parameters);  // 生成金币动画
};
```

### 为完成 PRP 需要完成的任务列表（按顺序排列）
```yaml
任务 1:
在 Unity 项目中设置 DOTween:
  - 将 DOTween 包添加到项目（手动安装）
  - 验证 DOTween 的安装和初始化
  - 配置 DOTween 设置为安全模式

任务 2:
创建 Coin 组件:
  - 将现有的 CoinAnimationController 修改为 Coin 组件
  - 实现飞行物动画的 DOTween 序列
  - 添加动画完成时的自我停用功能
  - 将参数绑定到 Unity Inspector

任务 3:
创建金币预制件:
  - 从金币 GameObject 创建预制件
  - 配置组件和引用
  - 验证预制件在实例化时能正常工作

任务 4:
实现对象池管理器:
  - 创建 CoinPoolManager 脚本
  - 实现带有初始金币的池初始化
  - 添加从池中获取/返还金币的方法
  - 跟踪活动金币以防止泄漏

任务 5:
创建金币动画系统:
  - 开发 CoinAnimationSystem 以进行高级控制
  - 实现生成金币动画的方法
  - 连接池管理器以获取金币

任务 6:
实现带对象池的飞行物动画:
  - 修改动画序列以与池化金币协同工作
  - 确保金币在动画结束后返回池中
  - 处理边界情况和错误条件

任务 7:
测试和验证实现:
  - 验证预制件实例化能正常工作
  - 用多个金币测试对象池
  - 验证内存管理和性能
  - 确认 URP 兼容性
```

### 根据需要为每个任务添加的伪代码
```csharp
// 任务 1: 在 Unity 项目中设置 DOTween
// 需要手动安装 - 请参阅 README.md 获取说明
// 在脚本初始化中:
void Awake()
{
    // 以安全模式初始化 DOTween
    DOTween.Init(true, true, LogBehaviour.ErrorsOnly);
    DOTween.defaultEaseType = Ease.OutQuad;
}

// 任务 2: 创建 Coin 组件
using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
    [Header("动画参数")]
    public CoinAnimationParams animationParams;
    
    private Sequence animationSequence;
    private Action<Coin> returnToPoolCallback;
    private SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // 如果尚未设置，则从 Resources 加载金币精灵
        if (spriteRenderer.sprite == null)
        {
            var coinSprite = Resources.Load<Sprite>("icon02");
            if (coinSprite != null) spriteRenderer.sprite = coinSprite;
        }
    }
    
    // 用参数和返回回调初始化金币
    public void Initialize(CoinAnimationParams parameters, Action<Coin> returnCallback)
    {
        animationParams = parameters;
        returnToPoolCallback = returnCallback;
        transform.position = parameters.startPosition;
        transform.localScale = Vector3.one * parameters.startScale;
    }
    
    // 开始飞行物动画
    public void StartFlying()
    {
        // 清理任何现有的动画
        if (animationSequence != null)
            animationSequence.Kill();
            
        // 创建 DOTween 序列
        animationSequence = DOTween.Sequence();
        
        // 移动缓动
        animationSequence.Append(transform.DOMove(animationParams.endPosition, animationParams.duration)
            .SetEase(animationParams.easeType));
        
        // 如果启用，则添加旋转缓动
        if (animationParams.enableRotation)
        {
            animationSequence.Join(transform.DORotate(
                new Vector3(0, 0, 360), animationParams.duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental));
        }
        
        // 如果启用，则添加缩放缓动
        if (animationParams.enableScaling)
        {
            animationSequence.Join(transform.DOScale(animationParams.endScale, animationParams.duration)
                .SetEase(animationParams.easeType));
        }
        
        // 动画完成时返回到池
        animationSequence.OnComplete(() => {
            ReturnToPool();
        });
    }
    
    // 将金币返回到池
    public void ReturnToPool()
    {
        if (animationSequence != null)
        {
            animationSequence.Kill();
            animationSequence = null;
        }
        
        // 停用并返回到池
        gameObject.SetActive(false);
        returnToPoolCallback?.Invoke(this);
    }
    
    void OnDisable()
    {
        // 清理任何正在运行的缓动
        if (animationSequence != null)
        {
            animationSequence.Kill();
            animationSequence = null;
        }
    }
}

// 任务 3: 创建金币预制件
// 在 Unity 编辑器中:
// 1. 创建带有 SpriteRenderer 组件的 GameObject
// 2. 附加 Coin.cs 脚本
// 3. 在 Inspector 中配置默认参数
// 4. 拖到 Project 窗口以创建预制件
// 5. 从场景中删除原始 GameObject

// 任务 4: 实现对象池管理器
public class CoinPoolManager : MonoBehaviour
{
    [Header("池设置")]
    public GameObject coinPrefab;
    public int initialPoolSize = 10;
    
    private Queue<Coin> coinPool = new Queue<Coin>();
    private List<Coin> activeCoins = new List<Coin>();
    
    void Awake()
    {
        InitializePool();
    }
    
    // 用金币初始化池
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewCoin();
        }
    }
    
    // 创建一个新金币并添加到池中
    private Coin CreateNewCoin()
    {
        GameObject coinObj = Instantiate(coinPrefab, transform);
        Coin coin = coinObj.GetComponent<Coin>();
        coinObj.SetActive(false);
        coinPool.Enqueue(coin);
        return coin;
    }
    
    // 从池中获取一个金币
    public Coin GetCoin()
    {
        Coin coin;
        if (coinPool.Count > 0)
        {
            coin = coinPool.Dequeue();
        }
        else
        {
            // 如果池为空，则创建新金币
            coin = CreateNewCoin();
        }
        
        coin.gameObject.SetActive(true);
        activeCoins.Add(coin);
        return coin;
    }
    
    // 将金币返回到池
    public void ReturnCoin(Coin coin)
    {
        if (activeCoins.Contains(coin))
        {
            activeCoins.Remove(coin);
            coinPool.Enqueue(coin);
        }
    }
}

// 任务 5: 创建金币动画系统
public class CoinAnimationSystem : MonoBehaviour
{
    public CoinPoolManager poolManager;
    
    // 用参数生成一个金币动画
    public void SpawnCoinAnimation(CoinAnimationParams parameters)
    {
        if (poolManager == null) return; 
        
        Coin coin = poolManager.GetCoin();
        coin.Initialize(parameters, poolManager.ReturnCoin);
        coin.StartFlying();
    }
    
    // 带位置的便捷方法
    public void SpawnCoinAnimation(Vector3 start, Vector3 end)
    {
        CoinAnimationParams parameters = new CoinAnimationParams
        {
            duration = 1.0f,
            startPosition = start,
            endPosition = end,
            enableRotation = true,
            rotationSpeed = 360f,
            enableScaling = true,
            startScale = 1.0f,
            endScale = 0.5f,
            easeType = Ease.OutQuad
        };
        
        SpawnCoinAnimation(parameters);
    }
}

// 任务 6: 实现带对象池的飞行物动画
// 已在上面的 Coin.cs 中实现:
// - Initialize 方法用于设置金币参数
// - StartFlying 方法用于开始动画
// - ReturnToPool 方法用于将金币返回到池
// - 在 OnDisable 中进行适当的清理
```

### 集成点
```yaml
DOTOOL_INTEGRATION:
  - initialization: "在脚本的 Awake 方法中调用 DOTween.Init()"
  - sequences: "使用 DOTween 序列实现复杂动画"
  - cleanup: "正确处理缓动以防止内存泄漏" 
  
OBJECT_POOLING:
  - pool_manager: "CoinPoolManager 处理金币生命周期"
  - prefab: "金币预制件包含所有必要的组件"
  - callbacks: "返回到池的回调系统" 
  
UNITY_URP:
  - rendering: "SpriteRenderer 与 URP 的兼容性"
  - materials: "默认材质与 URP 管线兼容" 
  
RESOURCES_LOADING:
  - sprite: "从 Resources 文件夹加载 icon02.png"
  - path: "Resources.Load<Sprite>(\"icon02\")"
```

## 验证循环

### 级别 1: 代码质量和结构
```bash
# 手动代码审查清单:
# 1. 脚本遵循 Unity 的 C# 编码标准
# 2. 正确使用 DOTween 序列和缓动
# 3. 正确实现对象池模式
# 4. 对缺失资源进行错误处理
# 5. 注释解释复杂的动画和池化逻辑
# 6. 一致的命名约定
# 7. 没有应可配置的硬编码值
# 8. 适当的内存管理和清理
```

### 级别 2: 功能测试
```csharp
// 验证实现的测试用例:
// 1. 预制件功能:
//    - 金币预制件能正确实例化
//    - 组件已正确附加
//    - 默认参数能正常工作

// 2. 对象池:
//    - 池以正确的金币数量初始化
//    - 可以从池中获取金币
//    - 使用后金币能返回到池中
//    - 池为空时能创建新金币

// 3. 动画系统:
//    - CoinAnimationSystem 能正确生成金币
//    - 动画参数影响金币移动
//    - 多个金币可以同时进行动画

// 4. 内存管理:
//    - 动画后没有内存泄漏
//    - 对象被正确停用/重新激活
//    - 池大小保持稳定

// 5. 边界情况:
//    - 动画可以在完成前停止
//    - 多次快速生成不会导致问题
//    - 并发动画之间没有冲突
```

### 级别 3: 性能测试
```bash
# 在 Unity 编辑器中:
# 1. 创建带有 CoinAnimationSystem 的测试场景
# 2. 同时生成 50 个金币
# 3. 监控帧率和内存使用情况
# 4. 使用 Unity Profiler 进行性能分析
#
# 预期: 性能稳定，没有掉帧
# 如果有问题: 检查对象池实现和 DOTween 清理
```

## 最终验证清单
- [ ] DOTween 包已正确安装和配置
- [ ] Coin 组件已创建并正确实现动画
- [ ] 金币预制件已创建并正确配置
- [ ] 对象池管理器高效地处理金币生命周期
- [ ] 金币动画系统提供高级控制
- [ ] 飞行物动画包含移动、旋转和缩放效果
- [ ] 系统能处理多个并发的金币动画
- [ ] 内存管理得当，没有泄漏
- [ ] 实现遵循 Unity 和 DOTween 的最佳实践
- [ ] 代码有良好的文档和解释
- [ ] 满足初始需求中的所有成功标准

--- 
## 要避免的反模式
- ❌ 不要在使用前忘记初始化 DOTween
- ❌ 不要因为不终止缓动而造成内存泄漏
- ❌ 不要在没有 Inspector 控件的情况下硬编码动画值
- ❌ 不要忽略 URP 兼容性要求
- ❌ 不要使用已弃用的 DOTween 方法
- ❌ 不要创建没有适当错误处理的脚本
- ❌ 不要跳过测试不同的参数组合
- ❌ 不要使用没有池而直接实例化预制件
- ❌ 不要在池化对象活动时修改它们

## 实现模式
- ✅ 使用 DOTween 序列实现复杂动画
- ✅ 实现适当的缓动清理和处理
- ✅ 使所有动画参数可配置
- ✅ 遵循 Unity 的基于组件的架构
- ✅ 使用 Resources.Load 加载资源
- ✅ 实现对缺失资源的错误处理
- ✅ 对频繁实例化的对象使用对象池
- ✅ 实现对象返回池的回调系统
- ✅ 使用能传达意图的描述性变量名
- ✅ 注释复杂的动画和池化逻辑
- ✅ 为可扩展性和未来的增强进行设计

## 性能优化技术
- ✅ 正确终止缓动以防止内存泄漏
- ✅ 使用对象池减少实例化开销
- ✅ 限制并发动画以防止性能问题
- ✅ 使用适当的缓动函数以获得平滑的性能
- ✅ 缓存组件引用以避免重复的 GetComponent 调用
- ✅ 使用局部变量而不是重复的属性访问
- ✅ 优化动画时长以提高响应性
- ✅ 为缓动使用适当的更新模式
- ✅ 预分配池以避免运行时实例化
- ✅ 监控活动对象与池化对象的数量

## 安全考虑
- ✅ 验证所有输入参数以防止意外行为
- ✅ 清理资源路径以防止目录遍历
- ✅ 在 C# 脚本中使用安全编码实践
- ✅ 避免向用户暴露内部动画数据
- ✅ 实现适当的错误处理以防止崩溃
- ✅ 遵循 Unity 的脚本开发安全最佳实践

## 维护最佳实践
- ✅ 记录复杂的动画算法和池化逻辑
- ✅ 在脚本中使用一致的命名约定
- ✅ 添加版本信息和变更日志
- ✅ 实现适当的错误日志和调试工具
- ✅ 为动画开发创建备份和恢复程序
- ✅ 为未来的增强和可扩展性进行规划
- ✅ 遵循已建立的 Unity 开发标准
- ✅ 及时了解 DOTween API 的变化

此 PRP 为在 Unity 中使用 DOTween 和对象池实现增强版金币飞行物动画系统提供了全面的指南，该系统打包为可方便复用的预制件，具备所有必需功能，同时保持高质量的代码、性能标准、安全性和兼容性。
