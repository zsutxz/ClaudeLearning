# .claude/commands/pr-ready.md
准备Pull Request，确保符合团队标准：

## 代码质量检查
1. 运行ESLint和Prettier格式化
2. 执行完整测试套件
3. 检查代码覆盖率（≥80%）
4. 运行TypeScript类型检查

## 文档更新
1. 更新相关API文档
2. 添加或修改README中的功能说明
3. 更新CHANGELOG.md

## PR描述生成
基于Git提交历史生成PR模板：
- 功能概述
- 技术变更说明
- 测试计划
- 影响分析

完成检查后，自动创建符合团队规范的PR。
