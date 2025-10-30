# 极简金币动画系统 - 使用说明

## 概述
本项目经过极简化处理，完全移除了Physics系统，提供最直接的金币动画功能。

## 核心组件

### 1. CoinAnimationController
- **功能**: 单个金币的动画控制
- **主要方法**:
  - `AnimateToPosition(targetPosition, duration)` - 移动到目标位置
  - `CollectCoin(collectionPoint, duration)` - 收集金币（带缩放效果）

### 2. CoinAnimationManager
- **功能**: 全局金币管理器（单例）
- **主要功能**: 注册金币、管理最大数量

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

## 极简化特性

- ✅ 完全移除了Physics系统
- ✅ 移除了复杂的DOTweenManager单例
- ✅ 简化了动画控制逻辑
- ✅ 精简了测试代码
- ✅ 保留了核心动画功能
- ✅ 极低的维护成本

## 系统架构

```
Assets/Scripts/
├── Core/
│   └── CoinAnimationState.cs      # 状态枚举
├── Animation/
│   ├── CoinAnimationController.cs # 金币控制器
│   └── CoinAnimationManager.cs    # 全局管理器
└── Examples/
    ├── SimpleCoinDemo.cs          # 演示脚本
    └── README.md                  # 本文档
```

## 性能优化
- 默认最大金币数：50个
- DOTween容量：100个tween
- 无物理计算开销
- 极低内存占用

## 注意事项
- 确保项目已安装DOTween插件
- 金币需要有Rigidbody和Collider组件
- 无磁性收集，需要手动调用CollectCoin或使用演示脚本

## 动画效果
- **移动动画**: 平滑的位置移动 + 旋转
- **收集动画**: 放大 + 移动到目标点 + 缩小消失
- **状态管理**: Idle → Moving → Collecting → Pooled