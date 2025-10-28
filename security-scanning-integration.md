# 主动安全扫描集成到工作流程指南

## 概述
将主动安全扫描集成到软件开发生命周期(SDLC)中，可以在早期发现和修复安全漏洞，降低安全风险和修复成本。

## 1. 当前工作流程分析

基于您的项目结构，我们识别了以下现状：
- 使用Git进行版本控制
- 配置了MCP服务器用于上下文管理
- 包含Claude Code技能和自定义命令
- 项目为技能仓库类型，包含多个独立功能模块

## 2. 安全扫描工具选择策略

### 推荐的安全工具组合：

#### 静态应用安全测试(SAST)
- **Semgrep** - 轻量级，支持多种语言
- **CodeQL** - GitHub提供，深度分析
- **SonarQube** - 企业级代码质量检查

#### 动态应用安全测试(DAST)
- **OWASP ZAP** - 开源Web应用安全扫描
- **Burp Suite** - 专业Web安全测试
- **Nuclei** - 快速漏洞扫描

#### 依赖项安全扫描
- **Snyk** - 开源依赖漏洞检测
- **OWASP Dependency-Check** - 免费依赖项检查
- **Trivy** - 容器和依赖项扫描

#### 容器安全
- **Trivy** - 容器镜像扫描
- **Clair** - 开源容器漏洞分析
- **Docker Scout** - Docker官方安全扫描

## 3. CI/CD流水线安全扫描集成点

### GitHub Actions 示例配置

```yaml
name: Security Scanning Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  # 代码提交阶段 - 快速扫描
  pre-commit-security:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Run Semgrep
        uses: returntocorp/semgrep-action@v1
        with:
          config: >-
            p/security-audit
            p/secrets
            p/owasp-top-ten

      - name: Scan for secrets
        uses: trufflesecurity/trufflehog@main
        with:
          path: ./
          base: main
          head: HEAD

  # 构建阶段 - 全面扫描
  build-security:
    runs-on: ubuntu-latest
    needs: pre-commit-security
    steps:
      - uses: actions/checkout@v3

      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'

      - name: Install dependencies
        run: npm ci

      - name: Run npm audit
        run: npm audit --audit-level=moderate

      - name: Run Snyk security scan
        uses: snyk/actions/node@master
        env:
          SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}

      - name: SonarQube Scan
        uses: sonarqube-quality-gate-action@master
        env:
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}

  # 部署前验证
  pre-deploy-security:
    runs-on: ubuntu-latest
    needs: build-security
    steps:
      - uses: actions/checkout@v3

      - name: Build application
        run: npm run build

      - name: OWASP ZAP Baseline Scan
        uses: zaproxy/action-baseline@v0.7.0
        with:
          target: 'http://localhost:3000'

      - name: Container security scan (if using Docker)
        if: contains(github.event.head_commit.message, '[docker]')
        uses: aquasecurity/trivy-action@master
        with:
          image-ref: 'your-app:latest'
          format: 'sarif'
          output: 'trivy-results.sarif'
```

## 4. 代码提交阶段的安全检查

### Pre-commit Hooks 配置

创建 `.pre-commit-config.yaml`:

```yaml
repos:
  - repo: https://github.com/pre-commit/pre-commit-hooks
    rev: v4.4.0
    hooks:
      - id: check-added-large-files
      - id: check-merge-conflict
      - id: check-yaml
      - id: end-of-file-fixer
      - id: trailing-whitespace

  - repo: https://github.com/Yelp/detect-secrets
    rev: v1.4.0
    hooks:
      - id: detect-secrets
        args: ['--baseline', '.secrets.baseline']

  - repo: https://github.com/returntocorp/semgrep
    rev: v1.45.0
    hooks:
      - id: semgrep
        args: ['--config=auto', '--severity=ERROR']
```

### Git Hooks 安装脚本

```bash
#!/bin/bash
# install-hooks.sh

# 安装 pre-commit
pip install pre-commit

# 安装 hooks
pre-commit install

# 初始化 secrets baseline
detect-secrets scan > .secrets.baseline

echo "Git hooks 安装完成！"
```

## 5. 构建阶段的安全扫描

### Dockerfile 安全最佳实践

```dockerfile
# 使用官方基础镜像
FROM node:18-alpine AS builder

# 创建非root用户
RUN addgroup -g 1001 -S nodejs
RUN adduser -S nextjs -u 1001

# 设置工作目录
WORKDIR /app

# 复制package文件
COPY package*.json ./

# 安装依赖
RUN npm ci --only=production && npm cache clean --force

# 复制源代码
COPY --chown=nextjs:nodejs . .

# 构建应用
RUN npm run build

# 生产阶段
FROM node:18-alpine AS runner

# 安装安全更新
RUN apk update && apk upgrade

# 创建非root用户
RUN addgroup -g 1001 -S nodejs
RUN adduser -S nextjs -u 1001

# 设置工作目录
WORKDIR /app

# 复制构建产物
COPY --from=builder --chown=nextjs:nodejs /app/dist ./dist
COPY --from=builder --chown=nextjs:nodejs /app/node_modules ./node_modules
COPY --from=builder --chown=nextjs:nodejs /app/package.json ./package.json

# 切换到非root用户
USER nextjs

# 暴露端口
EXPOSE 3000

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:3000/health || exit 1

# 启动应用
CMD ["npm", "start"]
```

## 6. 部署前的安全验证

### 安全检查清单

```yaml
# security-checklist.yaml
security_checks:
  code_review:
    - 代码已通过同行评审
    - 所有安全发现已修复或接受风险

  sast_results:
    - 无高危漏洞
    - 中危漏洞已修复或记录

  dependency_scan:
    - 无已知漏洞的依赖项
    - 所有依赖项为最新版本

  container_security:
    - 镜像已通过漏洞扫描
    - 使用非root用户运行
    - 包含健康检查

  infrastructure_security:
    - 网络配置正确
    - 密钥和凭证已加密存储
    - 访问控制已配置
```

### 自动化验证脚本

```python
#!/usr/bin/env python3
# security-validation.py

import requests
import json
import sys
from typing import Dict, List

class SecurityValidator:
    def __init__(self):
        self.results = []

    def check_application_health(self, url: str) -> bool:
        """检查应用程序健康状态"""
        try:
            response = requests.get(f"{url}/health", timeout=10)
            return response.status_code == 200
        except:
            return False

    def check_security_headers(self, url: str) -> Dict[str, bool]:
        """检查安全头"""
        try:
            response = requests.get(url, timeout=10)
            headers = response.headers

            security_headers = {
                'X-Content-Type-Options': 'nosniff' in headers.get('X-Content-Type-Options', ''),
                'X-Frame-Options': 'DENY' in headers.get('X-Frame-Options', ''),
                'X-XSS-Protection': '1; mode=block' in headers.get('X-XSS-Protection', ''),
                'Strict-Transport-Security': 'max-age=' in headers.get('Strict-Transport-Security', ''),
                'Content-Security-Policy': 'default-src' in headers.get('Content-Security-Policy', '')
            }

            return security_headers
        except:
            return {}

    def validate_deployment(self, app_url: str) -> bool:
        """执行部署前验证"""
        print("🔍 开始部署前安全验证...")

        # 健康检查
        if not self.check_application_health(app_url):
            print("❌ 应用健康检查失败")
            return False
        print("✅ 应用健康检查通过")

        # 安全头检查
        headers = self.check_security_headers(app_url)
        missing_headers = [k for k, v in headers.items() if not v]

        if missing_headers:
            print(f"⚠️ 缺少安全头: {', '.join(missing_headers)}")

        print("✅ 安全验证完成")
        return True

if __name__ == "__main__":
    validator = SecurityValidator()
    app_url = sys.argv[1] if len(sys.argv) > 1 else "http://localhost:3000"

    success = validator.validate_deployment(app_url)
    sys.exit(0 if success else 1)
```

## 7. 运行时安全监控

### 监控配置示例

```yaml
# docker-compose.monitoring.yml
version: '3.8'

services:
  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml

  grafana:
    image: grafana/grafana:latest
    ports:
      - "3001:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin123
    volumes:
      - grafana-storage:/var/lib/grafana

  falco:
    image: falcosecurity/falco:latest
    privileged: true
    volumes:
      - /var/run/docker.sock:/host/var/run/docker.sock
      - /dev:/host/dev
      - /proc:/host/proc:ro
      - /boot:/host/boot:ro
      - /lib/modules:/host/lib/modules:ro
      - /usr:/host/usr:ro
      - /etc:/host/etc:ro
      - ./falco_rules.yaml:/etc/falco/falco_rules.local.yaml

volumes:
  grafana-storage:
```

## 8. 安全扫描报告和通知

### 报告生成脚本

```python
#!/usr/bin/env python3
# security-report.py

import json
import smtplib
from email.mime.text import MimeText
from email.mime.multipart import MimeMultipart
from datetime import datetime

class SecurityReporter:
    def __init__(self):
        self.scan_results = {}

    def collect_scan_results(self):
        """收集各种扫描结果"""
        # 从文件中读取扫描结果
        with open('semgrep-results.json', 'r') as f:
            self.scan_results['semgrep'] = json.load(f)

        with open('trivy-results.json', 'r') as f:
            self.scan_results['trivy'] = json.load(f)

        with open('zap-results.json', 'r') as f:
            self.scan_results['zap'] = json.load(f)

    def generate_report(self) -> str:
        """生成安全报告"""
        report = f"""
# 安全扫描报告
生成时间: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}

## 扫描结果摘要

### SAST 扫描 (Semgrep)
- 发现问题数: {len(self.scan_results['semgrep'].get('results', []))}
- 高危问题: {len([r for r in self.scan_results['semgrep'].get('results', []) if r.get('metadata', {}).get('severity') == 'ERROR'])}

### 容器扫描 (Trivy)
- 漏洞总数: {len(self.scan_results['trivy'].get('Results', [{}])[0].get('Vulnerabilities', []))}
- 高危漏洞: {len([v for v in self.scan_results['trivy'].get('Results', [{}])[0].get('Vulnerabilities', []) if v.get('Severity') in ['HIGH', 'CRITICAL']])}

### DAST 扫描 (OWASP ZAP)
- 发现问题数: {len(self.scan_results['zap'].get('site', [{}])[0].get('alerts', []))}
- 高危问题: {len([a for a in self.scan_results['zap'].get('site', [{}])[0].get('alerts', []) if a.get('risk') == 'High'])}

## 建议措施
1. 立即修复所有高危漏洞
2. 制定中危漏洞修复计划
3. 建立定期安全扫描机制
4. 加强开发者安全培训
        """
        return report

    def send_notification(self, report: str, recipients: List[str]):
        """发送安全报告通知"""
        msg = MimeMultipart()
        msg['From'] = 'security@company.com'
        msg['To'] = ', '.join(recipients)
        msg['Subject'] = f'安全扫描报告 - {datetime.now().strftime("%Y-%m-%d")}'

        msg.attach(MimeText(report, 'plain', 'utf-8'))

        # 发送邮件配置
        smtp = smtplib.SMTP('smtp.company.com', 587)
        smtp.starttls()
        smtp.login('security@company.com', 'password')
        smtp.send_message(msg)
        smtp.quit()

if __name__ == "__main__":
    reporter = SecurityReporter()
    reporter.collect_scan_results()
    report = reporter.generate_report()

    # 保存报告
    with open(f'security-report-{datetime.now().strftime("%Y%m%d")}.md', 'w') as f:
        f.write(report)

    # 发送通知
    reporter.send_notification(report, ['dev-team@company.com', 'security-team@company.com'])
```

## 9. 实施路线图

### 阶段1：基础设置 (1-2周)
- 配置pre-commit hooks
- 设置GitHub Actions基础扫描
- 选择并配置SAST工具

### 阶段2：深度集成 (2-3周)
- 添加依赖项扫描
- 配置容器安全扫描
- 实施DAST扫描

### 阶段3：监控和报告 (1-2周)
- 设置运行时监控
- 配置安全报告
- 建立通知机制

### 阶段4：优化和自动化 (持续)
- 优化扫描性能
- 减少误报
- 建立安全度量指标

## 10. 最佳实践

1. **快速失败**：在开发阶段尽早发现问题
2. **渐进式实施**：从简单开始，逐步增加复杂度
3. **团队培训**：确保所有开发者了解安全要求
4. **持续改进**：定期审查和优化安全流程
5. **文档维护**：保持安全配置和流程文档更新

通过系统性地集成这些安全扫描工具和流程，您可以建立一个强大的安全防护体系，在软件开发生命周期的每个阶段都能主动发现和修复安全问题。