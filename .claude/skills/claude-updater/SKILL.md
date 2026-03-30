---
name: claude-updater
description: 统一管理 Claude Code 生态系统的所有组件更新，包括 CLI、BMAD 框架、插件市场等。当用户需要更新 Claude Code 相关组件、检查更新状态或查看更新报告时使用此 skill。
license: MIT
allowed-tools: [Bash, Read, Write, WebSearch]
metadata:
  version: "1.0.0"
  category: system
  tags: update,cli,plugin,bmad,maintenance
---

# Claude Code 生态更新器

## 概述
这个 skill 统一管理 Claude Code 生态系统中所有组件的更新流程，包括：
- Claude Code CLI 本身
- BMAD 框架（`_bmad/` 目录）
- Claude 官方技能插件（document-skills, example-skills）
- everything-claude-code 插件
- 其他已安装插件

**核心特性**：
- 一键检查所有组件的更新状态
- 批量更新或选择性更新
- 生成详细的更新报告
- 支持回滚和备份

## 使用时机
当用户提出以下需求时，使用此 skill：
- "检查更新" / "检查 Claude Code 更新"
- "更新全部" / "更新所有组件"
- "更新 CLI" / "更新 Claude CLI"
- "更新插件" / "更新技能"
- "更新 BMAD" / "更新 BMAD 框架"
- "查看更新报告" / "更新状态"
- "Claude Code 有什么更新"
- 类似的更新相关需求

## 组件清单

| 组件 | 位置 | 更新命令 | 检查方式 |
|------|------|----------|----------|
| Claude Code CLI | npm 全局 | `claude update` | `claude --version` |
| BMAD 框架 | `_bmad/` | `npx bmad-method install` | 检查 `_bmad/` 目录版本 |
| document-skills | 插件系统 | `claude plugin update` | `claude plugin list` |
| example-skills | 插件系统 | `claude plugin update` | `claude plugin list` |
| everything-claude-code | 插件系统 | `claude plugin update` | `claude plugin list` |
| code-simplifier | 插件系统 | `claude plugin update` | `claude plugin list` |
| context7 | 插件系统 | `claude plugin update` | `claude plugin list` |
| code-review | 插件系统 | `claude plugin update` | `claude plugin list` |

## 用户命令

| 命令 | 说明 |
|------|------|
| `检查更新` | 检查所有组件的更新状态，不执行更新 |
| `更新全部` | 更新所有组件（CLI + 插件 + BMAD） |
| `更新CLI` | 只更新 Claude Code CLI |
| `更新插件` | 更新所有已安装的插件 |
| `更新BMAD` | 只更新 BMAD 框架 |
| `更新报告` | 显示最近的更新报告 |
| `插件列表` | 显示所有已安装的插件 |

## 执行流程

### 步骤 1: 检查更新状态

**检查 Claude CLI 版本**：
```bash
claude --version
```

**检查已安装插件**：
```bash
claude plugin list
```

**检查 BMAD 框架版本**：
```bash
# 检查 _bmad 目录是否存在
ls _bmad/

# 查看当前 BMAD 版本（如果有版本文件）
cat _bmad/.bmad-version 2>/dev/null || echo "版本文件不存在"
```

**检查插件市场更新**：
```bash
claude plugin marketplace list
```

### 步骤 2: 执行更新

#### 更新 Claude CLI
```bash
# 首选方式
claude update

# 备选方式
npm update -g @anthropic-ai/claude-code
```

#### 更新插件
```bash
# 更新插件市场
claude plugin marketplace update

# 更新所有插件（批量）
# 注意：需要逐个更新
claude plugin update everything-claude-code@everything-claude-code
claude plugin update document-skills@anthropic-agent-skills
claude plugin update example-skills@anthropic-agent-skills
claude plugin update code-simplifier@claude-plugins-official
claude plugin update context7@claude-plugins-official
claude plugin update code-review@claude-plugins-official
```

#### 更新 BMAD 框架
```bash
# 使用官方安装器更新（交互式）
npx bmad-method install

# 注意：此命令是交互式的，需要用户手动选择安装选项
```

### 步骤 3: 生成更新报告

将更新结果保存到 `claude-updater-reports/` 目录。

## 数据存储

**报告存储位置**：项目根目录下的 `claude-updater-reports/` 目录

```
项目根目录/
├── claude-updater-reports/     # 更新报告目录
│   ├── index.json             # 报告索引
│   ├── latest.md              # 最新报告
│   └── history/               # 历史报告
│       └── YYYY-MM-DD_HHMMSS.md
└── .claude/
    └── skills/
        └── claude-updater/
            └── SKILL.md       # 本文件
```

## 报告格式

### 更新报告模板

```markdown
# Claude Code 生态更新报告

*生成时间：{datetime}*
*系统：{os_info}*

---

## 更新摘要

| 组件 | 更新前 | 更新后 | 状态 |
|------|--------|--------|------|
| Claude CLI | {old_version} | {new_version} | {status} |
| everything-claude-code | {old} | {new} | {status} |
| document-skills | - | - | {status} |
| example-skills | - | - | {status} |
| BMAD 框架 | {old_commit} | {new_commit} | {status} |

**状态说明**：✅ 成功 | ⏭️ 无更新 | ❌ 失败 | ⚠️ 警告

---

## 详细日志

### Claude CLI
```
{cli_update_log}
```

### 插件更新
```
{plugin_update_log}
```

### BMAD 框架
```
{bmad_update_log}
```

---

## 注意事项

{warnings}

---

*下次建议检查时间：{next_check_date}*
```

### 索引文件格式 (index.json)

```json
{
  "last_check": "2026-03-05T10:30:00",
  "last_update": "2026-03-05T10:35:00",
  "components": {
    "claude-cli": {
      "current_version": "2.1.63",
      "last_checked": "2026-03-05T10:30:00"
    },
    "bmad": {
      "current_commit": "abc1234",
      "last_checked": "2026-03-05T10:30:00"
    }
  },
  "history": [
    {
      "date": "2026-03-05T10:35:00",
      "report_file": "history/2026-03-05_103500.md",
      "updated_components": ["claude-cli", "bmad"]
    }
  ]
}
```

## 执行步骤（完整流程）

### 场景 1: 检查更新

1. **记录当前状态**
   ```
   - 运行 `claude --version` 获取 CLI 版本
   - 运行 `claude plugin list` 获取插件列表
   - 运行 `cd _bmad && git status` 获取 BMAD 状态
   ```

2. **检查可用更新**
   ```
   - 运行 `claude plugin marketplace list` 检查市场更新
   - 运行 `npx bmad-method --version` 检查 BMAD 安装器版本
   ```

3. **生成检查报告**
   ```
   - 汇总各组件状态
   - 标注有可用更新的组件
   - 返回给用户
   ```

### 场景 2: 更新全部

1. **备份当前状态**
   ```
   - 记录所有组件的当前版本
   - 更新 index.json
   ```

2. **更新 Claude CLI**
   ```
   - 运行 `claude update`
   - 记录更新结果
   ```

3. **更新插件**
   ```
   - 运行 `claude plugin marketplace update`
   - 逐个更新各插件
   - 记录每个插件的更新结果
   ```

4. **更新 BMAD**
   ```
   - 运行 `npx bmad-method install`
   - 注意：这是交互式命令，需要用户手动选择选项
   - 记录更新结果
   ```

5. **生成更新报告**
   ```
   - 创建报告文件
   - 更新索引
   - 返回摘要给用户
   ```

### 场景 3: 选择性更新

根据用户指定的组件，只执行对应的更新步骤。

## 注意事项

### BMAD 更新
- 使用官方安装器：`npx bmad-method install`
- **交互式命令**：需要用户手动选择安装选项
- 需要 Node.js 环境和 npx
- 更新时会显示 BMAD 版本和新功能信息

### 插件更新
- 需要网络连接
- 可能需要代理（中国大陆用户）
- 某些插件可能需要特定权限

### CLI 更新
- Windows 可能需要管理员权限
- 建议使用 `claude update` 而非 npm
- 更新后可能需要重启 Claude Code

### 错误处理
- 单个组件更新失败不影响其他组件
- 记录所有错误信息到报告
- 提供失败组件的手动更新命令

## 错误处理

### 网络错误
```
如果网络请求失败：
1. 提示用户检查网络连接
2. 建议使用代理
3. 提供离线模式（仅显示上次缓存的状态）
```

### 权限错误
```
如果权限不足：
1. 提示用户以管理员身份运行
2. Windows: 右键 -> 以管理员身份运行
3. Linux/macOS: 使用 sudo
```

### BMAD 安装错误
```
如果 npx bmad-method install 失败：
1. 检查 Node.js 是否安装：node --version
2. 检查网络连接
3. 尝试清除 npx 缓存：npx clear-npx-cache
4. 手动访问 https://docs.bmad-method.org 获取帮助
```

## 示例对话

**用户**: "检查更新"

**执行流程**：
1. 运行 `claude --version` - 当前版本 2.1.63
2. 运行 `claude plugin list` - 获取已安装插件
3. 检查 `_bmad/` 目录状态
4. 汇总结果，返回检查报告

**返回示例**：
```markdown
# 更新检查报告

*检查时间：2026-03-05 10:30*

## 组件状态

| 组件 | 当前版本 | 可用更新 |
|------|----------|----------|
| Claude CLI | 2.1.63 | ✅ 2.1.64 可用 |
| everything-claude-code | 1.2.0 | ⏭️ 已是最新 |
| BMAD 框架 | abc1234 | ✅ 3 个新提交 |

## 建议操作

运行 `更新全部` 来更新所有有可用更新的组件。
```

---

**用户**: "更新全部"

**执行流程**：
1. 更新 Claude CLI
2. 更新所有插件
3. 更新 BMAD 框架
4. 生成更新报告
5. 返回摘要

**返回示例**：
```markdown
# 更新完成

*更新时间：2026-03-05 10:35*

## 更新结果

| 组件 | 状态 | 说明 |
|------|------|------|
| Claude CLI | ✅ 成功 | 2.1.63 → 2.1.64 |
| everything-claude-code | ⏭️ 跳过 | 已是最新版本 |
| BMAD 框架 | ✅ 成功 | 使用 npx bmad-method install 更新 |

详细报告已保存到：`claude-updater-reports/history/2026-03-05_103500.md`

建议重启 Claude Code 以应用所有更新。
```

---

**用户**: "只更新插件"

**执行流程**：
1. 运行 `claude plugin marketplace update`
2. 逐个更新各插件
3. 记录结果
4. 返回摘要

---

**用户**: "更新报告"

**执行流程**：
1. 读取 `claude-updater-reports/latest.md`
2. 如果不存在，提示暂无报告
3. 返回报告内容

---

现在您可以开始管理 Claude Code 生态系统的更新了！
