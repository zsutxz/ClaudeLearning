## FEATURE:
- 在Unity中，创建一个极简的金币飞行动画系统
- 使用URP管线进行渲染
- 金币飞行动画做成预制体，易于复用
- 使用对象池技术，高效管理多个金币的生命周期

## IMPLEMENTATION APPROACH:
- 纯Unity协程实现，无外部依赖
- 零第三方插件，开箱即用
- 内置数学缓动函数，动画流畅自然
- 极简架构，代码少于600行

## OTHER CONSIDERATIONS:
- 金币图标使用icon02.png
- 专注性能优化，支持50+并发金币
- 跨平台兼容，支持所有Unity支持的设备
- 易于集成，即插即用

## TECHNICAL GOALS:
- 60fps性能表现
- 内存占用小于20MB
- 零外部依赖
- 简洁的API接口