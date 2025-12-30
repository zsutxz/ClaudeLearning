---
name: code-architecture-analyzer
description: 智能代码架构分析工具 - 识别技术栈、检测设计模式、分析依赖关系、生成架构文档
allowed-tools:
  - Read
  - Glob
  - Grep
  - Bash
  - Write
  - Edit
metadata:
  version: "2.0.0"
  author: "Claude Code"
  category: "development"
  tags: ["architecture", "analysis", "code-review", "documentation", "patterns", "tech-stack"]
examples:
  - "分析这个项目的整体架构"
  - "生成详细的架构文档，包括设计模式和依赖关系"
  - "识别Unity项目中的性能瓶颈和优化机会"
  - "评估Python项目的代码质量和可维护性"
languages:
  - Python
  - JavaScript/TypeScript
  - C#/.NET
  - Java
  - Go
frameworks:
  - Django/Flask/FastAPI
  - React/Vue/Angular
  - Unity
  - ASP.NET
  - Spring Boot
---

# 代码架构分析师 v2.0

## 核心能力

### 🏗️ 架构识别
- 自动识别项目类型（Unity/Python/JS/TS/C#/Java/Go）
- 技术栈分析（语言、框架、数据库、中间件）
- 架构模式检测（MVC/MVVM/微服务/分层架构等）
- 设计模式识别（单例/工厂/观察者/策略等23种模式）

### 📊 代码质量
- 代码结构评估（职责分离、复杂度、耦合度）
- 命名规范检查
- 最佳实践建议
- 技术债务识别

### 🔗 依赖分析
- 模块依赖关系图
- 外部依赖版本/兼容性分析
- 循环依赖检测
- 架构层次分析

### 📝 文档生成
- 执行摘要
- 详细架构分析
- 改进建议（P0/P1/P2优先级）
- Markdown格式报告

## 分析流程

```
1️⃣ 项目扫描
   ├─ 识别项目类型
   ├─ 检测技术栈
   └─ 确定架构风格

2️⃣ 深度分析
   ├─ 核心模块分析
   ├─ 设计模式检测
   └─ 代码质量评估

3️⃣ 报告生成
   ├─ 架构概览
   ├─ 模块说明
   ├─ 依赖关系图
   └─ 改进建议
```

## 支持的项目类型

| 类型 | 识别特征 | 专项分析 |
|------|----------|----------|
| **Unity** | Assets/, .unity | 组件架构、性能优化、资源管理 |
| **Python** | requirements.txt, setup.py | AI/ML、Web框架、数据处理流 |
| **JavaScript/TypeScript** | package.json, .ts/.js | 前端框架、状态管理、路由架构 |
| **C#/.NET** | .csproj, .sln | ASP.NET、WPF、架构模式 |
| **Java** | pom.xml, build.gradle | Spring Boot、Android |
| **Go** | go.mod, go.sum | 微服务、并发模式 |

## 使用示例

### 快速分析
```
分析这个项目的架构
```

### 完整分析
```
生成详细的架构分析报告，包括：
1. 技术栈和架构模式
2. 设计模式识别
3. 模块依赖关系
4. 代码质量评估
5. 改进建议
```

### 专项分析
```
分析Unity项目的性能优化机会：
- 内存分配和GC压力
- 渲染优化
- 组件通信效率
```

## 输出格式

生成的报告包含：
- 执行摘要（项目类型、技术栈、关键发现）
- 项目结构分析
- 技术栈详解
- 架构模式识别
- 代码质量评估
- 依赖关系分析
- 改进建议（按优先级）

## 配置文件

技能包含完整的架构模式配置：
- `config/patterns.yaml` - 23种设计模式定义
- 支持SOLID原则检测
- 识别反模式（God Object、Spaghetti Code等）
- 质量指标基准

---

*详见 prompt.md 获取完整使用说明*
