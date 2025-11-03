# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

### git规则
   - 不自动提交


### 🎯 Project Overview
This is a **Unity 2022.3.5f1** project implementing a **极简** coin animation system using pure Unity coroutines. **删除所有复杂功能**，只保留最核心的金币移动和收集动画功能。总共只有 **4个核心文件**。

### 🏗️ 极简架构

#### 核心文件结构 (仅4个文件)

1. **BasicCoinAnimation.cs** - 金币动画控制器
   - `MoveTo()` - 移动到目标位置
   - `Collect()` - 收集金币动画
   - `StopAnimation()` - 停止动画
   - `Reset()` - 重置金币状态

2. **SimpleCoinManager.cs** - 金币管理器
   - `CreateCoinAnimation()` - 创建移动动画
   - `CreateCollectionAnimation()` - 创建收集动画
   - `ClearAllCoins()` - 清理所有金币
   - 内置对象池管理

3. **BasicCoinDemo.cs** - 演示脚本
   - 按键控制：M-移动，C-收集，X-清理
   - GUI界面操作
   - 简单的使用示例

4. **README.md** - 使用文档
   - 详细的使用说明
   - 安装和配置指南

### ⚙️ 使用方法

#### 基础用法
```csharp
// 1. 在场景中添加 SimpleCoinManager 组件
// 2. 设置金币预制体（只需要 BasicCoinAnimation 组件）

// 创建移动动画
coinManager.CreateCoinAnimation(startPos, targetPos);

// 创建收集动画
coinManager.CreateCollectionAnimation(startPos, collectPoint);

// 清理所有金币
coinManager.ClearAllCoins();
```

#### 直接使用动画组件
```csharp
// 获取金币动画组件
BasicCoinAnimation coin = coinObject.GetComponent<BasicCoinAnimation>();

// 移动金币
coin.MoveTo(targetPosition, 1f);

// 收集金币
coin.Collect(collectionPoint, 0.5f);

// 停止动画
coin.StopAnimation();
```

### 🎮 演示控制

**按键操作:**
- **M** - 演示移动动画
- **C** - 演示收集动画
- **X** - 清理所有金币

**GUI操作:**
- 点击界面按钮执行对应操作

### 📁 项目结构
```
Project/Assets/Scripts/
├── Animation/
│   ├── BasicCoinAnimation.cs    # 核心动画控制器
│   └── SimpleCoinManager.cs     # 金币管理器
└── Examples/
    └── BasicCoinDemo.cs         # 演示脚本
```

### ✨ 特性

- **极简设计** - 只有4个核心文件，代码简洁
- **零依赖** - 不需要任何外部插件或包
- **高性能** - 基于Unity协程，支持50+并发金币
- **易使用** - 简单的API，一行代码创建动画
- **对象池** - 内置高效的对象池管理
- **跨平台** - 支持所有Unity平台

### 🚀 快速开始

1. **创建金币预制体**
   - 创建3D物体（如Sphere）
   - 添加 `BasicCoinAnimation` 组件
   - 保存为预制体

2. **设置场景**
   - 在场景中创建空物体
   - 添加 `SimpleCoinManager` 组件
   - 将金币预制体拖入 Coin Prefab 字段

3. **运行演示**
   - 添加 `BasicCoinDemo` 组件到场景
   - 设置生成点和目标点
   - 运行场景，使用按键或GUI操作

### 📋 简化历程

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

### 💡 最佳实践

1. **金币预制体**: 只需要 `BasicCoinAnimation` 组件
2. **对象池**: 让 `SimpleCoinManager` 自动管理
3. **性能**: 避免同时创建过多金币（建议 < 100个）
4. **动画**: 使用内置的缓动效果，无需自定义

### 🔧 故障排除

**问题**: 金币不显示
- 检查预制体是否正确设置
- 确认 SimpleCoinManager 的 Coin Prefab 字段已赋值

**问题**: 动画不流畅
- 减少同时活动的金币数量
- 检查目标位置是否合理

**问题**: 收集动画无效果
- 确认收集点位置设置正确
- 检查动画时长参数

---
*极简金币动画系统 - 专注核心功能，拒绝过度工程化*