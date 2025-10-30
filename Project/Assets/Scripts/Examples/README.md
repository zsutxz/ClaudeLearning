# 超级极简金币动画系统 - 使用说明

## 概述
本项目经过终极简化，完全移除了DOTween和Physics系统，使用纯Unity协程实现所有动画功能。

## 核心组件

### 1. CoinAnimationController
- **功能**: 单个金币的动画控制，使用协程实现
- **主要方法**:
  - `AnimateToPosition(targetPosition, duration)` - 移动到目标位置（带缓动）
  - `CollectCoin(collectionPoint, duration)` - 收集金币（放大+移动+缩小效果）
  - `StopCurrentAnimation()` - 停止当前动画
- **动画特性**:
  - 内置缓动函数（EaseOutQuad, EaseOutBack, EaseInSine等）
  - 平滑的旋转动画
  - 多阶段收集动画

### 2. CoinAnimationManager
- **功能**: 全局金币管理器（单例模式）
- **主要功能**: 注册金币、管理最大数量、事件分发

## 使用方法

### 创建金币预制体
1. 创建3D物体（如Sphere）
2. 添加 `Rigidbody` 组件
3. 添加 `SphereCollider` 组件
4. 添加 `CoinAnimationController` 脚本
5. 保存为预制体

### 运行演示
1. 创建空物体
2. 添加 `SimpleCoinDemo` 脚本
3. 设置 `coinPrefab`、`spawnPoint`、`collectionPoint`
4. 运行场景

### 手动控制动画
```csharp
// 获取金币控制器
var coinController = coinObject.GetComponent<CoinAnimationController>();

// 移动动画
coinController.AnimateToPosition(targetPosition, 1.0f);

// 收集动画
coinController.CollectCoin(collectionPoint, 1.5f);

// 停止动画
coinController.StopCurrentAnimation();
```

## 终极简化特性

- ✅ 完全移除DOTween依赖
- ✅ 完全移除Physics系统
- ✅ 使用纯Unity协程实现
- ✅ 内置缓动函数
- ✅ 零外部依赖
- ✅ 极致的性能优化

## 系统架构

```
Assets/Scripts/
├── Core/
│   └── CoinAnimationState.cs      # 状态枚举
├── Animation/
│   ├── CoinAnimationController.cs # 金币控制器（协程版）
│   └── CoinAnimationManager.cs    # 全局管理器
└── Examples/
    ├── SimpleCoinDemo.cs          # 演示脚本
    └── README.md                  # 本文档
```

## 动画效果详解

### 移动动画
- **位置插值**: 使用Vector3.Lerp实现平滑移动
- **缓动效果**: 内置EaseOutQuad函数
- **旋转动画**: 持续旋转增加视觉效果

### 收集动画
1. **放大阶段** (30%时间): 使用EaseOutBack缓动，放大到1.5倍
2. **移动阶段** (70%时间): 使用EaseInSine缓动，移动到收集点
3. **缩小阶段** (20%时间): 使用EaseInBack缓动，缩小到0
4. **特效播放**: 粒子效果和音效

### 内置缓动函数
- `EaseOutQuad`: 二次方缓出，适合移动动画
- `EaseOutBack`: 弹性缓出，适合放大效果
- `EaseInSine`: 正弦缓入，适合吸引效果
- `EaseInBack`: 弹性缓入，适合缩小效果

## 性能优化
- **协程驱动**: 轻量级协程，无GC压力
- **默认最大金币数**: 50个
- **无物理计算**: 纯数学插值
- **内存占用**: 极低

## 注意事项
- 金币需要有Rigidbody和Collider组件
- 动画使用协程，确保GameObject处于Active状态
- 可以通过StopCurrentAnimation()随时停止动画
- 收集完成后金币会自动SetActive(false)

## 状态管理
完整的动画状态流程：
1. **Idle** - 空闲状态
2. **Moving** - 移动动画中
3. **Collecting** - 收集动画中
4. **Pooled** - 已收集，对象池状态

## 优势总结
- **零依赖**: 不需要任何第三方插件
- **高性能**: 纯Unity原生实现
- **易维护**: 代码简洁清晰
- **易扩展**: 缓动函数可自定义
- **跨平台**: 兼容所有Unity支持的平台