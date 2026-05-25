---
story_id: "4.2"
story_key: "4-2-self-play-ui-config-and-execution"
title: "自对弈 UI 配置与执行"
status: review
created: 2026-05-22
updated: 2026-05-22
---

# Story 4.2: 自对弈 UI 配置与执行

As a 玩家,
I want 选择双方 AI 配置并观看自动对弈,
So that 我能直观对比不同 AI 策略的表现。

## Acceptance Criteria

**AC1:** AIvsAI 模式显示黑方/白方 AI 策略选择下拉框
**AC2:** 点击开始后自动对弈，每步 0.5 秒延迟
**AC3:** 支持暂停/继续
**AC4:** 支持重置

## Tasks

- [x] UIManager 添加 AIvsAI 配置 UI（黑方/白方 AI 难度选择）
- [x] UIManager 添加游戏模式切换按钮（PvAI / PvP / AIvsAI）
- [x] AIvsAI 模式显示暂停/继续/重置按钮
- [x] 选择 AI 配置后调用 GameManager setter 并重启

## Dev Agent Record

### Implementation Plan

- UIManager.cs OnGUI 中：
  - 添加游戏模式切换按钮行
  - AIvsAI 模式显示双方 AI 下拉框（用按钮模拟）
  - AIvsAI 模式显示暂停/继续/重置按钮
  - PvAI 模式保留原有 AI 难度按钮

### Debug Log

### Completion Notes
- UIManager OnGUI 新增：游戏模式切换按钮行（人机/双人/AI对弈）
- AIvsAI 模式：显示黑方/白方 AI 难度选择（● 标记当前选中）
- AIvsAI 模式：开始自对弈按钮
- 自对弈进行中：暂停/继续 + 重置按钮
- PvAI 模式保留原有 AI 难度按钮
- 静态 DifficultyNames/Difficulties 数组消除重复

## File List
- gamego/Assets/Scripts/UI/UIManager.cs (modified)

## Change Log

- 2026-05-22: Story created from epics.md
