#!/usr/bin/env python3
"""
代码架构分析工具
支持多语言项目的架构识别、设计模式分析、依赖关系梳理
"""

import os
import re
import json
import yaml
from pathlib import Path
from typing import Dict, List, Optional, Tuple
from dataclasses import dataclass
from enum import Enum

class ProjectType(Enum):
    UNITY = "unity"
    PYTHON = "python"
    JAVASCRIPT = "javascript"
    TYPESCRIPT = "typescript"
    CSHARP = "csharp"
    JAVA = "java"
    GO = "go"
    UNKNOWN = "unknown"

class ArchitecturePattern(Enum):
    MVC = "Model-View-Controller"
    MVP = "Model-View-Presenter"
    MVVM = "Model-View-ViewModel"
    LAYERED = "Layered Architecture"
    MICROSERVICES = "Microservices"
    MONOLITH = "Monolithic"
    EVENT_DRIVEN = "Event-Driven"
    PLUGIN = "Plugin Architecture"
    REPOSITORY = "Repository Pattern"
    FACTORY = "Factory Pattern"
    OBSERVER = "Observer Pattern"
    SINGLETON = "Singleton Pattern"

@dataclass
class ArchitectureInfo:
    project_type: ProjectType
    tech_stack: Dict[str, List[str]]
    patterns: List[ArchitecturePattern]
    dependencies: Dict[str, List[str]]
    structure: Dict[str, List[str]]
    issues: List[str]
    suggestions: List[str]

class CodeArchitectureAnalyzer:
    def __init__(self, project_path: str):
        self.project_path = Path(project_path)
        self.analysis_result = ArchitectureInfo(
            project_type=ProjectType.UNKNOWN,
            tech_stack={},
            patterns=[],
            dependencies={},
            structure={},
            issues=[],
            suggestions=[]
        )

    def analyze(self) -> ArchitectureInfo:
        """执行完整的架构分析"""
        self._identify_project_type()
        self._analyze_tech_stack()
        self._detect_architecture_patterns()
        self._analyze_dependencies()
        self._analyze_structure()
        self._identify_issues()
        self._generate_suggestions()
        return self.analysis_result

    def _identify_project_type(self):
        """识别项目类型"""
        indicators = {
            ProjectType.UNITY: ['Assets/', 'ProjectSettings/', 'Packages/', '.unity'],
            ProjectType.PYTHON: ['requirements.txt', 'setup.py', 'pyproject.toml', 'Pipfile'],
            ProjectType.JAVASCRIPT: ['package.json', 'node_modules/', '.js'],
            ProjectType.TYPESCRIPT: ['package.json', 'tsconfig.json', '.ts'],
            ProjectType.CSHARP: ['.csproj', '.sln', 'packages.config'],
            ProjectType.JAVA: ['pom.xml', 'build.gradle', '.java'],
            ProjectType.GO: ['go.mod', 'go.sum', '.go']
        }

        for project_type, files in indicators.items():
            for file in files:
                if any(self.project_path.rglob(file)) or (self.project_path / file).exists():
                    self.analysis_result.project_type = project_type
                    return

    def _analyze_tech_stack(self):
        """分析技术栈"""
        tech_stack = {
            'languages': [],
            'frameworks': [],
            'databases': [],
            'tools': []
        }

        # 分析语言
        if self.analysis_result.project_type == ProjectType.UNITY:
            tech_stack['languages'].extend(['C#', 'ShaderLab'])
            tech_stack['frameworks'].append('Unity')

        elif self.analysis_result.project_type == ProjectType.PYTHON:
            tech_stack['languages'].append('Python')
            # 分析requirements.txt
            req_file = self.project_path / 'requirements.txt'
            if req_file.exists():
                content = req_file.read_text()
                if 'django' in content.lower():
                    tech_stack['frameworks'].append('Django')
                if 'flask' in content.lower():
                    tech_stack['frameworks'].append('Flask')
                if 'fastapi' in content.lower():
                    tech_stack['frameworks'].append('FastAPI')
                if 'tensorflow' in content.lower():
                    tech_stack['frameworks'].append('TensorFlow')
                if 'pytorch' in content.lower():
                    tech_stack['frameworks'].append('PyTorch')

        elif self.analysis_result.project_type in [ProjectType.JAVASCRIPT, ProjectType.TYPESCRIPT]:
            lang = 'TypeScript' if self.analysis_result.project_type == ProjectType.TYPESCRIPT else 'JavaScript'
            tech_stack['languages'].append(lang)

            # 分析package.json
            package_file = self.project_path / 'package.json'
            if package_file.exists():
                try:
                    content = json.loads(package_file.read_text())
                    deps = {**content.get('dependencies', {}), **content.get('devDependencies', {})}

                    frameworks = {
                        'react': 'React',
                        'vue': 'Vue.js',
                        'angular': 'Angular',
                        'express': 'Express.js',
                        'next': 'Next.js',
                        'nuxt': 'Nuxt.js'
                    }

                    for dep, framework in frameworks.items():
                        if dep in deps:
                            tech_stack['frameworks'].append(framework)
                except:
                    pass

        self.analysis_result.tech_stack = tech_stack

    def _detect_architecture_patterns(self):
        """检测架构模式"""
        patterns = []

        if self.analysis_result.project_type == ProjectType.UNITY:
            # Unity特有模式检测
            if self._has_mono_behaviour_components():
                patterns.append(ArchitecturePattern.MVC)
            if self._has_object_pooling():
                patterns.extend([ArchitecturePattern.OBSERVER, ArchitecturePattern.FACTORY])

        # 通用模式检测
        if self._has_layered_structure():
            patterns.append(ArchitecturePattern.LAYERED)

        if self._has_mvc_structure():
            patterns.append(ArchitecturePattern.MVC)

        if self._has_microservices_structure():
            patterns.append(ArchitecturePattern.MICROSERVICES)

        self.analysis_result.patterns = patterns

    def _analyze_dependencies(self):
        """分析依赖关系"""
        dependencies = {
            'internal': [],
            'external': [],
            'configuration': []
        }

        # 根据项目类型分析依赖
        if self.analysis_result.project_type == ProjectType.PYTHON:
            self._analyze_python_dependencies(dependencies)
        elif self.analysis_result.project_type in [ProjectType.JAVASCRIPT, ProjectType.TYPESCRIPT]:
            self._analyze_node_dependencies(dependencies)
        elif self.analysis_result.project_type == ProjectType.UNITY:
            self._analyze_unity_dependencies(dependencies)

        self.analysis_result.dependencies = dependencies

    def _analyze_structure(self):
        """分析项目结构"""
        structure = {
            'source': [],
            'configuration': [],
            'documentation': [],
            'tests': [],
            'build': []
        }

        for path in self.project_path.rglob('*'):
            if path.is_file() and not any(skip in str(path) for skip in ['.git', 'node_modules', '__pycache__']):
                rel_path = path.relative_to(self.project_path)

                if self._is_source_file(rel_path):
                    structure['source'].append(str(rel_path))
                elif self._is_config_file(rel_path):
                    structure['configuration'].append(str(rel_path))
                elif self._is_doc_file(rel_path):
                    structure['documentation'].append(str(rel_path))
                elif self._is_test_file(rel_path):
                    structure['tests'].append(str(rel_path))
                elif self._is_build_file(rel_path):
                    structure['build'].append(str(rel_path))

        self.analysis_result.structure = structure

    def _identify_issues(self):
        """识别架构问题"""
        issues = []

        # 检查常见问题
        if self._has_circular_dependencies():
            issues.append("检测到潜在的循环依赖")

        if self._has_mixed_concerns():
            issues.append("发现关注点混合，建议分离业务逻辑")

        if self._has_large_files():
            issues.append("存在过大的文件，建议拆分")

        if not self._has_documentation():
            issues.append("缺少文档，建议添加项目说明")

        self.analysis_result.issues = issues

    def _generate_suggestions(self):
        """生成改进建议"""
        suggestions = []

        if self.analysis_result.project_type == ProjectType.UNITY:
            suggestions.extend([
                "考虑使用ScriptableObject管理配置数据",
                "实现对象池以优化内存使用",
                "使用事件系统解耦组件间通信",
                "添加单元测试和集成测试"
            ])

        elif self.analysis_result.project_type == ProjectType.PYTHON:
            suggestions.extend([
                "使用虚拟环境隔离依赖",
                "添加类型注解提高代码可读性",
                "实现日志系统便于调试",
                "使用pytest进行测试"
            ])

        # 通用建议
        suggestions.extend([
            "添加代码格式化工具配置",
            "建立代码审查流程",
            "完善错误处理机制",
            "添加性能监控"
        ])

        self.analysis_result.suggestions = suggestions

    # 辅助方法
    def _has_mono_behaviour_components(self) -> bool:
        """检查是否有MonoBehaviour组件"""
        return any(self.project_path.rglob('*.cs')) and any(
            'MonoBehaviour' in f.read_text()
            for f in self.project_path.rglob('*.cs')
            if f.is_file()
        )

    def _has_object_pooling(self) -> bool:
        """检查是否有对象池模式"""
        pool_keywords = ['Pool', 'ObjectPool', 'pool', 'Queue', 'Stack']
        return any(
            any(keyword in f.read_text() for keyword in pool_keywords)
            for f in self.project_path.rglob('*.cs')
            if f.is_file()
        )

    def _has_layered_structure(self) -> bool:
        """检查是否有分层结构"""
        layer_dirs = ['models', 'views', 'controllers', 'services', 'repositories']
        return any(
            self.project_path.joinpath(layer).exists()
            for layer in layer_dirs
        )

    def _has_mvc_structure(self) -> bool:
        """检查MVC结构"""
        return self._has_layered_structure() and any(
            self.project_path.joinpath(dir_name).exists()
            for dir_name in ['models', 'views', 'controllers']
        )

    def _has_microservices_structure(self) -> bool:
        """检查微服务结构"""
        return len([d for d in self.project_path.iterdir()
                   if d.is_dir() and (d / 'Dockerfile').exists()]) > 1

    def _is_source_file(self, path: Path) -> bool:
        """判断是否为源代码文件"""
        source_extensions = ['.cs', '.py', '.js', '.ts', '.java', '.go', '.cpp', '.c']
        return any(str(path).endswith(ext) for ext in source_extensions)

    def _is_config_file(self, path: Path) -> bool:
        """判断是否为配置文件"""
        config_patterns = ['package.json', 'requirements.txt', '.csproj', '.sln',
                          'pom.xml', 'build.gradle', 'tsconfig.json', '.env']
        return any(str(path).endswith(pattern) for pattern in config_patterns)

    def _is_doc_file(self, path: Path) -> bool:
        """判断是否为文档文件"""
        doc_extensions = ['.md', '.rst', '.txt', '.pdf', '.doc', '.docx']
        return any(str(path).endswith(ext) for ext in doc_extensions)

    def _is_test_file(self, path: Path) -> bool:
        """判断是否为测试文件"""
        test_patterns = ['test_', '_test.', 'spec.', '.test.', 'Tests/', 'test/']
        return any(pattern in str(path) for pattern in test_patterns)

    def _is_build_file(self, path: Path) -> bool:
        """判断是否为构建文件"""
        build_patterns = ['Makefile', 'CMakeLists.txt', 'Dockerfile', 'build.gradle', 'pom.xml']
        return any(str(path).endswith(pattern) for pattern in build_patterns)

    def _has_circular_dependencies(self) -> bool:
        """检查循环依赖（简化版）"""
        # 这里应该实现更复杂的依赖分析
        return False

    def _has_mixed_concerns(self) -> bool:
        """检查关注点混合"""
        # 这里应该分析代码的职责分离
        return False

    def _has_large_files(self) -> bool:
        """检查大文件"""
        large_file_threshold = 1000  # 行数
        for source_file in self.project_path.rglob('*'):
            if self._is_source_file(source_file) and source_file.is_file():
                try:
                    with open(source_file, 'r', encoding='utf-8') as f:
                        if sum(1 for _ in f) > large_file_threshold:
                            return True
                except:
                    continue
        return False

    def _has_documentation(self) -> bool:
        """检查是否有文档"""
        doc_files = list(self.project_path.rglob('*.md')) + list(self.project_path.rglob('README*'))
        return len(doc_files) > 0

    def _analyze_python_dependencies(self, dependencies: dict):
        """分析Python依赖"""
        req_file = self.project_path / 'requirements.txt'
        if req_file.exists():
            with open(req_file, 'r') as f:
                dependencies['external'] = [
                    line.strip().split('==')[0].split('>=')[0].split('<=')[0]
                    for line in f.readlines()
                    if line.strip() and not line.startswith('#')
                ]

    def _analyze_node_dependencies(self, dependencies: dict):
        """分析Node.js依赖"""
        package_file = self.project_path / 'package.json'
        if package_file.exists():
            try:
                content = json.loads(package_file.read_text())
                deps = {**content.get('dependencies', {}), **content.get('devDependencies', {})}
                dependencies['external'] = list(deps.keys())
            except:
                pass

    def _analyze_unity_dependencies(self, dependencies: dict):
        """分析Unity依赖"""
        manifest_file = self.project_path / 'Packages' / 'manifest.json'
        if manifest_file.exists():
            try:
                content = json.loads(manifest_file.read_text())
                dependencies['external'] = [
                    name for name in content.get('dependencies', {}).keys()
                    if name != 'com.unity.modules.ui'
                ]
            except:
                pass

def generate_architecture_report(analysis: ArchitectureInfo) -> str:
    """生成架构分析报告"""
    report = f"""
# 项目架构分析报告

## 执行摘要
- **项目类型**: {analysis.project_type.value}
- **检测到的架构模式**: {', '.join([p.value for p in analysis.patterns])}
- **主要发现**: {len(analysis.issues)} 个潜在问题

## 技术栈分析
"""

    for category, items in analysis.tech_stack.items():
        if items:
            report += f"### {category.title()}\n"
            for item in items:
                report += f"- {item}\n"

    report += f"""
## 架构模式识别
"""
    for pattern in analysis.patterns:
        report += f"- **{pattern.value}**: 基于项目结构检测到的设计模式\n"

    report += f"""
## 项目结构分析
"""
    for category, files in analysis.structure.items():
        if files:
            report += f"### {category.title()} ({len(files)} 个文件)\n"
            for file in files[:10]:  # 只显示前10个
                report += f"- {file}\n"
            if len(files) > 10:
                report += f"- ... 还有 {len(files) - 10} 个文件\n"

    if analysis.issues:
        report += f"""
## 识别的问题
"""
        for issue in analysis.issues:
            report += f"- ⚠️ {issue}\n"

    report += f"""
## 改进建议
"""
    for suggestion in analysis.suggestions:
        report += f"- 💡 {suggestion}\n"

    return report

if __name__ == "__main__":
    import sys
    if len(sys.argv) != 2:
        print("用法: python analyze-project.py <项目路径>")
        sys.exit(1)

    project_path = sys.argv[1]
    analyzer = CodeArchitectureAnalyzer(project_path)
    analysis = analyzer.analyze()
    report = generate_architecture_report(analysis)

    print(report)

    # 保存报告到文件
    output_file = Path(project_path) / "architecture_analysis.md"
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(report)

    print(f"\n报告已保存到: {output_file}")