# 简化金币动画系统 - 使用说明

## 概述
本项目经过大幅简化，去除了复杂的DOTweenManager和过度抽象的组件，提供简单直接的金币动画功能。

## 核心组件

### 1. CoinAnimationController
- **功能**: 单个金币的动画控制
- **主要方法**:
  - `AnimateToPosition(targetPosition, duration)` - 移动到目标位置
  - `CollectCoin(collectionPoint, duration)` - 收集金币

### 2. CoinAnimationManager
- **功能**: 全局金币管理器（单例）
- **主要功能**: 注册金币、管理最大数量

### 3. MagneticCollectionController
- **功能**: 简化的磁性收集系统
- **主要功能**: 自动吸引附近金币到收集点

## 使用方法

### 创建金币预制体
1. 创建3D物体（如Sphere）
2. 添加 `Rigidbody` 组件
3. 添加 `SphereCollider` 组件
4. 添加 `CoinAnimationController` 脚本
5. 保存为预制体

### 设置磁性收集
1. 创建空物体
2. 添加 `MagneticCollectionController` 脚本
3. 设置 `CollectionPoint` 为收集目标位置

### 运行演示
1. 创建空物体
2. 添加 `SimpleCoinDemo` 脚本
3. 设置 `coinPrefab`、`spawnPoint`、`collectionPoint`
4. 运行场景

## 简化特性

- ✅ 移除了复杂的DOTweenManager单例
- ✅ 简化了动画控制逻辑
- ✅ 精简了磁性收集系统
- ✅ 减少了测试代码复杂度
- ✅ 保持了核心动画功能

## 性能优化
- 默认最大金币数：50个
- DOTween容量：100个tween
- 磁性半径：5米（可配置）

## 注意事项
- 确保项目已安装DOTween插件
- 金币需要有Rigidbody和Collider组件
- 收集点需要在磁性控制器的范围内