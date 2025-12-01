#!/usr/bin/env python3
"""
Python项目专用架构分析器
专门分析Python项目的架构模式、代码质量、最佳实践
支持Web应用、数据科学、机器学习、CLI工具等项目类型
"""

import ast
import re
import json
import subprocess
from pathlib import Path
from typing import Dict, List, Tuple, Optional, Set
from dataclasses import dataclass
from enum import Enum
import configparser

class PythonProjectType(Enum):
    WEB_APP = "Web Application"
    DATA_SCIENCE = "Data Science"
    MACHINE_LEARNING = "Machine Learning"
    CLI_TOOL = "CLI Tool"
    LIBRARY = "Python Library"
    API_SERVICE = "API Service"
    MICROSERVICE = "Microservice"
    DESKTOP_APP = "Desktop Application"
    GAME = "Game Development"

class PythonFramework(Enum):
    DJANGO = "Django"
    FLASK = "Flask"
    FASTAPI = "FastAPI"
    STREAMLIT = "Streamlit"
    GRADIO = "Gradio"
    JUPYTER = "Jupyter Notebook"
    PANDAS = "Pandas"
    NUMPY = "NumPy"
    TENSORFLOW = "TensorFlow"
    PYTORCH = "PyTorch"
    SCIKIT_LEARN = "Scikit-learn"
    CLICK = "Click"
    ARGPARSE = "Argparse"
    TKINTER = "Tkinter"
    PYQT = "PyQt"
    KIVY = "Kivy"

class PythonArchitecturePattern(Enum):
    MVC = "Model-View-Controller"
    MVT = "Model-View-Template"
    REPOSITORY = "Repository Pattern"
    FACTORY = "Factory Pattern"
    SINGLETON = "Singleton Pattern"
    OBSERVER = "Observer Pattern"
    STRATEGY = "Strategy Pattern"
    ADAPTER = "Adapter Pattern"
    DECORATOR = "Decorator Pattern"
    DEPENDENCY_INJECTION = "Dependency Injection"
    COMMAND = "Command Pattern"
    STATE_MACHINE = "State Machine"
    PIPELINE = "Pipeline Pattern"
    PLUGIN = "Plugin Architecture"

class CodeQualityIssue(Enum):
    LONG_FUNCTIONS = "Long Functions (>50 lines)"
    LARGE_CLASSES = "Large Classes (>300 lines)"
    COMPLEXITY_HIGH = "High Cyclomatic Complexity"
    MISSING_DOCS = "Missing Documentation"
    TYPE_HINTS_MISSING = "Missing Type Hints"
    HARDCODED_VALUES = "Hardcoded Values"
    EXCEPTION_HANDLING = "Poor Exception Handling"
    DUPLICATE_CODE = "Code Duplication"
    NAMING_CONVENTION = "Naming Convention Issues"
    IMPORT_ISSUES = "Import Organization Issues"

class PythonBestPractice(Enum):
    VIRTUAL_ENV = "Virtual Environment Usage"
    DEPENDENCY_MANAGEMENT = "Dependency Management"
    TESTING = "Unit Testing Present"
    LOGGING = "Logging Implementation"
    CONFIG_MANAGEMENT = "Configuration Management"
    ASYNC_PROGRAMMING = "Async Programming"
    CONTEXT_MANAGERS = "Context Managers Usage"
    PROPERTY_DECORATORS = "Property Decorators"
    DATA_CLASSES = "Data Classes Usage"
    TYPE_HINTS = "Type Hints Usage"

@dataclass
class PythonModuleInfo:
    name: str
    file_path: str
    lines_of_code: int
    functions: List[str]
    classes: List[str]
    imports: List[str]
    complexity_score: float
    documentation_coverage: float
    type_hints_coverage: float
    dependencies: List[str]

@dataclass
class PythonArchitectureAnalysis:
    project_type: PythonProjectType
    frameworks: List[PythonFramework]
    patterns: List[PythonArchitecturePattern]
    quality_issues: List[CodeQualityIssue]
    best_practices: List[PythonBestPractice]
    modules: List[PythonModuleInfo]
    dependencies: Dict[str, str]
    project_structure: Dict[str, List[str]]
    test_coverage: Dict[str, float]
    recommendations: List[str]
    quality_score: float
    security_issues: List[str]

class PythonArchitectureAnalyzer:
    def __init__(self, project_path: Path):
        self.project_path = project_path
        self.excluded_dirs = {
            '.git', '__pycache__', 'node_modules', '.pytest_cache',
            '.venv', 'venv', 'env', '.env', 'build', 'dist', 'egg-info'
        }

    def analyze(self) -> PythonArchitectureAnalysis:
        """执行Python项目的全面架构分析"""

        # 识别项目类型
        project_type = self._identify_project_type()

        # 检测框架
        frameworks = self._detect_frameworks()

        # 检测架构模式
        patterns = self._detect_patterns()

        # 分析代码质量问题
        quality_issues = self._analyze_quality_issues()

        # 评估最佳实践
        best_practices = self._evaluate_best_practices()

        # 分析模块
        modules = self._analyze_modules()

        # 分析依赖
        dependencies = self._analyze_dependencies()

        # 分析项目结构
        project_structure = self._analyze_project_structure()

        # 分析测试覆盖率
        test_coverage = self._analyze_test_coverage()

        # 安全检查
        security_issues = self._security_analysis()

        # 生成建议
        recommendations = self._generate_recommendations(
            project_type, frameworks, patterns, quality_issues, best_practices, security_issues
        )

        # 计算质量评分
        quality_score = self._calculate_quality_score(
            patterns, quality_issues, best_practices, test_coverage
        )

        return PythonArchitectureAnalysis(
            project_type=project_type,
            frameworks=frameworks,
            patterns=patterns,
            quality_issues=quality_issues,
            best_practices=best_practices,
            modules=modules,
            dependencies=dependencies,
            project_structure=project_structure,
            test_coverage=test_coverage,
            recommendations=recommendations,
            quality_score=quality_score,
            security_issues=security_issues
        )

    def _identify_project_type(self) -> PythonProjectType:
        """识别Python项目类型"""
        indicators = {
            PythonProjectType.WEB_APP: [
                'requirements.txt', 'manage.py', 'app.py', 'server.py',
                'wsgi.py', 'asgi.py', 'urls.py', 'views.py'
            ],
            PythonProjectType.DATA_SCIENCE: [
                'requirements.txt', 'Jupyterfile', 'jupyter/', 'notebooks/',
                '.ipynb', 'pandas', 'numpy', 'matplotlib'
            ],
            PythonProjectType.MACHINE_LEARNING: [
                'model.py', 'train.py', 'predict.py', 'requirements.txt',
                'tensorflow', 'torch', 'sklearn', 'keras', 'datasets/'
            ],
            PythonProjectType.CLI_TOOL: [
                'setup.py', 'Click', 'argparse', 'typer', 'main.py',
                '__main__.py', 'console_scripts'
            ],
            PythonProjectType.LIBRARY: [
                'setup.py', 'pyproject.toml', 'src/', 'tests/',
                'README.md', 'LICENSE'
            ],
            PythonProjectType.API_SERVICE: [
                'api/', 'main.py', 'requirements.txt', 'fastapi',
                'flask', 'django', 'endpoints/', 'routes/'
            ],
            PythonProjectType.DESKTOP_APP: [
                'tkinter', 'pyqt', 'kivy', 'pygame', 'gui/',
                'main.py', 'app.py'
            ]
        }

        # 检查文件和目录
        project_files = []
        for file_path in self.project_path.rglob("*"):
            if file_path.is_file() and not any(excluded in str(file_path) for excluded in self.excluded_dirs):
                project_files.append(str(file_path).lower())

        # 检查目录结构
        project_dirs = [d.name.lower() for d in self.project_path.iterdir() if d.is_dir()]

        # 计算每种项目类型的匹配度
        type_scores = {}
        for project_type, files in indicators.items():
            score = sum(1 for indicator in files if any(indicator in f for f in project_files + project_dirs))
            type_scores[project_type] = score

        # 返回得分最高的项目类型
        if type_scores:
            return max(type_scores, key=type_scores.get)
        return PythonProjectType.WEB_APP  # 默认值

    def _detect_frameworks(self) -> List[PythonFramework]:
        """检测使用的Python框架"""
        frameworks = []

        # 检查依赖文件
        dependency_files = [
            'requirements.txt', 'pyproject.toml', 'Pipfile',
            'setup.py', 'environment.yml'
        ]

        for dep_file in dependency_files:
            file_path = self.project_path / dep_file
            if file_path.exists():
                try:
                    content = file_path.read_text(encoding='utf-8').lower()
                    frameworks.extend(self._detect_frameworks_from_content(content))
                except:
                    continue

        # 检查Python文件中的导入
        for py_file in self.project_path.rglob("*.py"):
            if not any(excluded in str(py_file) for excluded in self.excluded_dirs):
                try:
                    content = py_file.read_text(encoding='utf-8')
                    frameworks.extend(self._detect_frameworks_from_content(content))
                except:
                    continue

        return list(set(frameworks))  # 去重

    def _detect_frameworks_from_content(self, content: str) -> List[PythonFramework]:
        """从内容中检测框架"""
        frameworks = []

        framework_indicators = {
            PythonFramework.DJANGO: ['django', 'manage.py', 'urls.py', 'views.py'],
            PythonFramework.FLASK: ['flask', 'app = flask(', 'flask.ext'],
            PythonFramework.FASTAPI: ['fastapi', 'from fastapi', 'app = fastapi'],
            PythonFramework.STREAMLIT: ['streamlit', 'st.', 'streamlit.'],
            PythonFramework.GRADIO: ['gradio', 'gr.', 'gradio.'],
            PythonFramework.JUPYTER: ['jupyter', 'ipython', 'notebook'],
            PythonFramework.PANDAS: ['pandas', 'pd = pandas', 'import pandas'],
            PythonFramework.NUMPY: ['numpy', 'np = numpy', 'import numpy'],
            PythonFramework.TENSORFLOW: ['tensorflow', 'tf = tensorflow', 'import tensorflow'],
            PythonFramework.PYTORCH: ['torch', 'pytorch', 'import torch'],
            PythonFramework.SCIKIT_LEARN: ['sklearn', 'scikit-learn', 'import sklearn'],
            PythonFramework.CLICK: ['click', '@click.', 'import click'],
            PythonFramework.ARGPARSE: ['argparse', 'ArgumentParser', 'import argparse'],
            PythonFramework.TKINTER: ['tkinter', 'Tk()', 'import tkinter'],
            PythonFramework.PYQT: ['pyqt', 'PyQt', 'from pyqt'],
            PythonFramework.KIVY: ['kivy', 'from kivy', 'import kivy']
        }

        for framework, indicators in framework_indicators.items():
            if any(indicator in content for indicator in indicators):
                frameworks.append(framework)

        return frameworks

    def _detect_patterns(self) -> List[PythonArchitecturePattern]:
        """检测架构模式"""
        patterns = []

        # 分析Python文件
        py_files = [
            f for f in self.project_path.rglob("*.py")
            if not any(excluded in str(f) for excluded in self.excluded_dirs)
        ]

        # 合并所有代码内容进行分析
        all_code = ""
        for py_file in py_files:
            try:
                all_code += py_file.read_text(encoding='utf-8') + "\n"
            except:
                continue

        # 检测各种模式
        if self._detect_mvc_pattern(py_files):
            patterns.append(PythonArchitecturePattern.MVC)

        if self._detect_mvt_pattern(py_files):
            patterns.append(PythonArchitecturePattern.MVT)

        if self._detect_repository_pattern(all_code):
            patterns.append(PythonArchitecturePattern.REPOSITORY)

        if self._detect_factory_pattern(all_code):
            patterns.append(PythonArchitecturePattern.FACTORY)

        if self._detect_singleton_pattern(all_code):
            patterns.append(PythonArchitecturePattern.SINGLETON)

        if self._detect_observer_pattern(all_code):
            patterns.append(PythonArchitecturePattern.OBSERVER)

        if self._detect_strategy_pattern(all_code):
            patterns.append(PythonArchitecturePattern.STRATEGY)

        if self._detect_adapter_pattern(all_code):
            patterns.append(PythonArchitecturePattern.ADAPTER)

        if self._detect_decorator_pattern(all_code):
            patterns.append(PythonArchitecturePattern.DECORATOR)

        if self._detect_dependency_injection_pattern(all_code):
            patterns.append(PythonArchitecturePattern.DEPENDENCY_INJECTION)

        if self._detect_command_pattern(all_code):
            patterns.append(PythonArchitecturePattern.COMMAND)

        if self._detect_state_machine_pattern(all_code):
            patterns.append(PythonArchitecturePattern.STATE_MACHINE)

        if self._detect_pipeline_pattern(all_code):
            patterns.append(PythonArchitecturePattern.PIPELINE)

        if self._detect_plugin_pattern(py_files):
            patterns.append(PythonArchitecturePattern.PLUGIN)

        return patterns

    def _analyze_quality_issues(self) -> List[CodeQualityIssue]:
        """分析代码质量问题"""
        issues = []

        py_files = [
            f for f in self.project_path.rglob("*.py")
            if not any(excluded in str(f) for excluded in self.excluded_dirs)
        ]

        for py_file in py_files:
            try:
                with open(py_file, 'r', encoding='utf-8') as f:
                    content = f.read()

                # 解析AST
                tree = ast.parse(content)

                # 检查长函数
                if self._has_long_functions(tree):
                    issues.append(CodeQualityIssue.LONG_FUNCTIONS)

                # 检查大类
                if self._has_large_classes(tree):
                    issues.append(CodeQualityIssue.LARGE_CLASSES)

                # 检查复杂度
                if self._has_high_complexity(tree):
                    issues.append(CodeQualityIssue.COMPLEXITY_HIGH)

                # 检查文档
                if self._missing_documentation(tree):
                    issues.append(CodeQualityIssue.MISSING_DOCS)

                # 检查类型提示
                if self._missing_type_hints(tree):
                    issues.append(CodeQualityIssue.TYPE_HINTS_MISSING)

                # 检查硬编码值
                if self._has_hardcoded_values(content):
                    issues.append(CodeQualityIssue.HARDCODED_VALUES)

                # 检查异常处理
                if self._poor_exception_handling(tree):
                    issues.append(CodeQualityIssue.EXCEPTION_HANDLING)

                # 检查命名规范
                if self._naming_convention_issues(tree):
                    issues.append(CodeQualityIssue.NAMING_CONVENTION)

            except:
                continue

        # 去重
        return list(set(issues))

    def _evaluate_best_practices(self) -> List[PythonBestPractice]:
        """评估最佳实践"""
        practices = []

        # 检查虚拟环境
        if self._has_virtual_environment():
            practices.append(PythonBestPractice.VIRTUAL_ENV)

        # 检查依赖管理
        if self._has_dependency_management():
            practices.append(PythonBestPractice.DEPENDENCY_MANAGEMENT)

        # 检查测试
        if self._has_unit_tests():
            practices.append(PythonBestPractice.TESTING)

        # 检查日志
        if self._has_logging_implementation():
            practices.append(PythonBestPractice.LOGGING)

        # 检查配置管理
        if self._has_configuration_management():
            practices.append(PythonBestPractice.CONFIG_MANAGEMENT)

        # 检查异步编程
        if self._has_async_programming():
            practices.append(PythonBestPractice.ASYNC_PROGRAMMING)

        # 检查上下文管理器
        if self._has_context_managers():
            practices.append(PythonBestPractice.CONTEXT_MANAGERS)

        # 检查属性装饰器
        if self._has_property_decorators():
            practices.append(PythonBestPractice.PROPERTY_DECORATORS)

        # 检查数据类
        if self._has_data_classes():
            practices.append(PythonBestPractice.DATA_CLASSES)

        # 检查类型提示
        if self._has_type_hints_usage():
            practices.append(PythonBestPractice.TYPE_HINTS)

        return practices

    def _analyze_modules(self) -> List[PythonModuleInfo]:
        """分析Python模块"""
        modules = []

        py_files = [
            f for f in self.project_path.rglob("*.py")
            if not any(excluded in str(f) for excluded in self.excluded_dirs)
        ]

        for py_file in py_files:
            try:
                with open(py_file, 'r', encoding='utf-8') as f:
                    content = f.read()

                # 解析AST
                tree = ast.parse(content)

                # 提取模块信息
                module_name = py_file.stem
                file_path = str(py_file.relative_to(self.project_path))
                lines_of_code = len([line for line in content.split('\n') if line.strip()])

                functions = [node.name for node in ast.walk(tree) if isinstance(node, ast.FunctionDef)]
                classes = [node.name for node in ast.walk(tree) if isinstance(node, ast.ClassDef)]
                imports = self._extract_imports(tree)

                complexity_score = self._calculate_complexity_score(tree)
                documentation_coverage = self._calculate_doc_coverage(tree)
                type_hints_coverage = self._calculate_type_hints_coverage(tree)
                dependencies = self._extract_module_dependencies(tree)

                modules.append(PythonModuleInfo(
                    name=module_name,
                    file_path=file_path,
                    lines_of_code=lines_of_code,
                    functions=functions,
                    classes=classes,
                    imports=imports,
                    complexity_score=complexity_score,
                    documentation_coverage=documentation_coverage,
                    type_hints_coverage=type_hints_coverage,
                    dependencies=dependencies
                ))

            except:
                continue

        return modules

    def _analyze_dependencies(self) -> Dict[str, str]:
        """分析项目依赖"""
        dependencies = {}

        # 分析requirements.txt
        req_file = self.project_path / 'requirements.txt'
        if req_file.exists():
            try:
                content = req_file.read_text(encoding='utf-8')
                for line in content.split('\n'):
                    line = line.strip()
                    if line and not line.startswith('#'):
                        if '==' in line:
                            pkg, version = line.split('==', 1)
                            dependencies[pkg] = version
                        else:
                            dependencies[line] = 'latest'
            except:
                pass

        # 分析pyproject.toml
        pyproject_file = self.project_path / 'pyproject.toml'
        if pyproject_file.exists():
            try:
                content = pyproject_file.read_text(encoding='utf-8')
                # 简化版TOML解析
                import re
                deps_match = re.search(r'dependencies\s*=\s*\[(.*?)\]', content, re.DOTALL)
                if deps_match:
                    deps_section = deps_match.group(1)
                    for dep in re.findall(r'["\']([^"\']+)["\']', deps_section):
                        if '>=' in dep or '==' in dep or '<=' in dep:
                            pkg, version = re.split(r'[<>=]+', dep, 1)
                            dependencies[pkg] = dep
                        else:
                            dependencies[dep] = 'latest'
            except:
                pass

        return dependencies

    def _analyze_project_structure(self) -> Dict[str, List[str]]:
        """分析项目结构"""
        structure = {
            "source_code": [],
            "tests": [],
            "configuration": [],
            "documentation": [],
            "scripts": [],
            "data": [],
            "models": []
        }

        for item in self.project_path.rglob("*"):
            if item.is_file() and not any(excluded in str(item) for excluded in self.excluded_dirs):
                rel_path = str(item.relative_to(self.project_path))

                if item.suffix == '.py':
                    if 'test' in item.name.lower() or 'tests' in str(item.parent).lower():
                        structure["tests"].append(rel_path)
                    else:
                        structure["source_code"].append(rel_path)

                elif item.suffix in ['.txt', '.yaml', '.yml', '.toml', '.ini', '.cfg', '.conf']:
                    structure["configuration"].append(rel_path)

                elif item.suffix in ['.md', '.rst', '.txt'] and ('readme' in item.name.lower() or 'doc' in str(item.parent).lower()):
                    structure["documentation"].append(rel_path)

                elif item.suffix in ['.sh', '.bat', '.ps1']:
                    structure["scripts"].append(rel_path)

                elif item.suffix in ['.csv', '.json', '.pkl', '.parquet', '.h5']:
                    structure["data"].append(rel_path)

                elif item.suffix in ['.pkl', '.pth', '.pt', '.h5', '.pb', '.onnx']:
                    structure["models"].append(rel_path)

        return structure

    def _analyze_test_coverage(self) -> Dict[str, float]:
        """分析测试覆盖率"""
        coverage = {}

        # 检查是否有pytest配置
        pytest_files = [
            'pytest.ini', 'pyproject.toml', 'setup.cfg', '.coveragerc'
        ]

        for config_file in pytest_files:
            file_path = self.project_path / config_file
            if file_path.exists():
                try:
                    content = file_path.read_text(encoding='utf-8')
                    # 尝试运行pytest获取覆盖率
                    if self._can_run_pytest():
                        result = subprocess.run(
                            ['pytest', '--cov=.', '--cov-report=term-missing'],
                            cwd=self.project_path,
                            capture_output=True,
                            text=True,
                            timeout=30
                        )
                        if result.returncode == 0:
                            coverage_output = result.stdout
                            # 解析覆盖率输出
                            coverage_match = re.search(r'TOTAL\s+\d+\s+\d+\s+(\d+)%', coverage_output)
                            if coverage_match:
                                coverage['overall'] = float(coverage_match.group(1))
                except:
                    pass
                break

        # 计算测试文件数量
        test_files = list(self.project_path.rglob("*test*.py"))
        source_files = [
            f for f in self.project_path.rglob("*.py")
            if not any(excluded in str(f) for excluded in self.excluded_dirs)
            and 'test' not in str(f).lower()
        ]

        if source_files:
            coverage['test_to_source_ratio'] = len(test_files) / len(source_files)
        else:
            coverage['test_to_source_ratio'] = 0.0

        return coverage

    def _security_analysis(self) -> List[str]:
        """安全分析"""
        security_issues = []

        py_files = [
            f for f in self.project_path.rglob("*.py")
            if not any(excluded in str(f) for excluded in self.excluded_dirs)
        ]

        for py_file in py_files:
            try:
                with open(py_file, 'r', encoding='utf-8') as f:
                    content = f.read()

                # 检查硬编码密钥
                if self._has_hardcoded_secrets(content):
                    security_issues.append(f"Hardcoded secrets found in {py_file.name}")

                # 检查SQL注入风险
                if self._has_sql_injection_risks(content):
                    security_issues.append(f"Potential SQL injection in {py_file.name}")

                # 检查eval/exec使用
                if self._has_dangerous_functions(content):
                    security_issues.append(f"Dangerous functions (eval/exec) in {py_file.name}")

                # 检查pickle不安全使用
                if self._has_unsafe_pickle(content):
                    security_issues.append(f"Unsafe pickle usage in {py_file.name}")

            except:
                continue

        return list(set(security_issues))

    def _generate_recommendations(self, project_type: PythonProjectType,
                                 frameworks: List[PythonFramework],
                                 patterns: List[PythonArchitecturePattern],
                                 quality_issues: List[CodeQualityIssue],
                                 best_practices: List[PythonBestPractice],
                                 security_issues: List[str]) -> List[str]:
        """生成改进建议"""
        recommendations = []

        # 基于项目类型的建议
        if project_type == PythonProjectType.WEB_APP:
            recommendations.extend([
                "🌐 **API设计**: 使用RESTful API设计原则和OpenAPI规范",
                "🔐 **安全实现**: 添加认证、授权、HTTPS、CSRF保护",
                "📊 **性能优化**: 实现缓存、数据库优化、异步处理",
                "🧪 **测试覆盖**: 添加单元测试、集成测试、API测试"
            ])

        elif project_type == PythonProjectType.MACHINE_LEARNING:
            recommendations.extend([
                "🤖 **模型管理**: 使用MLflow或DVC管理模型版本",
                "📈 **实验跟踪**: 实现实验日志记录和参数管理",
                "🚀 **部署准备**: 容器化模型服务，实现模型监控",
                "🔧 **数据处理**: 使用DVC或类似工具管理数据集版本"
            ])

        elif project_type == PythonProjectType.DATA_SCIENCE:
            recommendations.extend([
                "📊 **数据处理**: 使用Pandas、NumPy优化数据处理流程",
                "📈 **可视化**: 添加Matplotlib、Seaborn、Plotly可视化",
                "📝 **文档完善**: 添加数据字典、处理流程说明",
                "🔄 **自动化**: 使用Jupyter或自动化脚本处理重复任务"
            ])

        # 基于质量问题的建议
        if CodeQualityIssue.LONG_FUNCTIONS in quality_issues:
            recommendations.append("📏 **函数拆分**: 将长函数拆分为更小的功能单元")

        if CodeQualityIssue.MISSING_DOCS in quality_issues:
            recommendations.append("📝 **添加文档**: 为函数和类添加docstring文档")

        if CodeQualityIssue.TYPE_HINTS_MISSING in quality_issues:
            recommendations.append("💡 **类型提示**: 添加类型提示提高代码可读性和IDE支持")

        if CodeQualityIssue.HARDCODED_VALUES in quality_issues:
            recommendations.append("⚙️ **配置管理**: 将硬编码值移动到配置文件")

        # 基于最佳实践的建议
        if PythonBestPractice.TESTING not in best_practices:
            recommendations.append("🧪 **添加测试**: 实现单元测试和集成测试")

        if PythonBestPractice.LOGGING not in best_practices:
            recommendations.append("📋 **实现日志**: 添加结构化日志记录")

        if PythonBestPractice.VIRTUAL_ENV not in best_practices:
            recommendations.append("🐍 **虚拟环境**: 使用虚拟环境管理依赖")

        # 基于安全问题的建议
        if security_issues:
            recommendations.extend([
                "🔒 **安全审计**: 检查并修复硬编码密钥和敏感信息",
                "🛡️ **输入验证**: 实现输入验证和输出编码",
                "🔐 **依赖更新**: 定期更新依赖包修复安全漏洞"
            ])

        # 基于架构模式的建议
        if not patterns:
            recommendations.append("🏗️ **架构设计**: 考虑应用MVC、Repository或Factory模式")

        # 通用建议
        recommendations.extend([
            "📊 **代码质量**: 使用pylint、black、isort等工具提高代码质量",
            "🔄 **CI/CD**: 设置持续集成和自动化测试",
            "📚 **文档完善**: 添加README、API文档、使用示例",
            "🐛 **错误处理**: 实现完善的异常处理和错误日志",
            "⚡ **性能监控**: 添加性能监控和指标收集"
        ])

        return recommendations

    def _calculate_quality_score(self, patterns: List[PythonArchitecturePattern],
                                 quality_issues: List[CodeQualityIssue],
                                 best_practices: List[PythonBestPractice],
                                 test_coverage: Dict[str, float]) -> float:
        """计算项目质量评分"""
        base_score = 50.0

        # 架构模式加分 (每个模式+4分)
        pattern_score = len(patterns) * 4

        # 最佳实践加分 (每个实践+3分)
        practice_score = len(best_practices) * 3

        # 质量问题扣分 (每个问题-6分)
        issue_penalty = len(quality_issues) * 6

        # 测试覆盖率加分
        coverage_score = test_coverage.get('overall', 0) * 0.2

        # 测试文件比例加分
        test_ratio_score = test_coverage.get('test_to_source_ratio', 0) * 10

        final_score = base_score + pattern_score + practice_score - issue_penalty + coverage_score + test_ratio_score
        return max(0.0, min(100.0, final_score))

    # 模式检测方法
    def _detect_mvc_pattern(self, py_files: List[Path]) -> bool:
        """检测MVC模式"""
        has_model = any('model' in str(f).lower() for f in py_files)
        has_view = any('view' in str(f).lower() for f in py_files)
        has_controller = any('controller' in str(f).lower() for f in py_files)
        return has_model and has_view and has_controller

    def _detect_mvt_pattern(self, py_files: List[Path]) -> bool:
        """检测MVT模式 (Django特有)"""
        has_model = any('model' in str(f).lower() for f in py_files)
        has_view = any('view' in str(f).lower() for f in py_files)
        has_template = any('template' in str(f).lower() for f in py_files)
        return has_model and has_view and has_template

    def _detect_repository_pattern(self, code_content: str) -> bool:
        """检测Repository模式"""
        repo_indicators = [
            r'class.*Repository',
            r'def.*find_by',
            r'def.*save\(',
            r'def.*delete\(',
            r'def.*get_by'
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in repo_indicators)

    def _detect_factory_pattern(self, code_content: str) -> bool:
        """检测Factory模式"""
        factory_indicators = [
            r'class.*Factory',
            r'def.*create.*\(',
            r'def.*build.*\(',
            r'Factory\(\)'
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in factory_indicators)

    def _detect_singleton_pattern(self, code_content: str) -> bool:
        """检测Singleton模式"""
        singleton_indicators = [
            r'_instance\s*=',
            r'def.*__new__',
            r'def.*get_instance',
            r'@classmethod'
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in singleton_indicators)

    def _detect_observer_pattern(self, code_content: str) -> bool:
        """检测Observer模式"""
        observer_indicators = [
            r'@observer|@subscribe',
            r'notify\s*\(',
            r'attach\s*\(',
            r'detach\s*\(',
            r'subject\.'
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in observer_indicators)

    def _detect_strategy_pattern(self, code_content: str) -> bool:
        """检测Strategy模式"""
        strategy_indicators = [
            r'class.*Strategy',
            r'def.*execute.*\(',
            r'strategy\s*=',
            r'StrategyPattern'
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in strategy_indicators)

    def _detect_adapter_pattern(self, code_content: str) -> bool:
        """检测Adapter模式"""
        adapter_indicators = [
            r'class.*Adapter',
            r'def.*adapt.*\(',
            r'interface\s+.*Adapter'
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in adapter_indicators)

    def _detect_decorator_pattern(self, code_content: str) -> bool:
        """检测Decorator模式"""
        return '@' in code_content and 'def ' in code_content

    def _detect_dependency_injection_pattern(self, code_content: str) -> bool:
        """检测依赖注入模式"""
        di_indicators = [
            r'def.*__init__\([^)]*\*args[^)]*\*\*kwargs',
            r'inject\s*=',
            r'container\.'
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in di_indicators)

    def _detect_command_pattern(self, code_content: str) -> bool:
        """检测Command模式"""
        command_indicators = [
            r'class.*Command',
            r'def.*execute.*\(',
            r'def.*undo.*\(',
            r'Command.*execute'
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in command_indicators)

    def _detect_state_machine_pattern(self, code_content: str) -> bool:
        """检测状态机模式"""
        state_machine_indicators = [
            r'class.*State',
            r'current_state\s*=',
            r'def.*transition.*\(',
            r'switch.*state'
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in state_machine_indicators)

    def _detect_pipeline_pattern(self, code_content: str) -> bool:
        """检测Pipeline模式"""
        pipeline_indicators = [
            r'class.*Pipeline',
            r'pipeline\s*=',
            r'def.*fit_transform',
            r'def.*process.*\('
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in pipeline_indicators)

    def _detect_plugin_pattern(self, py_files: List[Path]) -> bool:
        """检测Plugin架构模式"""
        plugin_indicators = ['plugin', 'extension', 'module', 'addons']
        return any(any(indicator in str(f).lower() for indicator in plugin_indicators) for f in py_files)

    # 质量问题检测方法
    def _has_long_functions(self, tree: ast.AST) -> bool:
        """检测长函数"""
        for node in ast.walk(tree):
            if isinstance(node, ast.FunctionDef):
                if hasattr(node, 'end_lineno') and node.end_lineno:
                    lines = node.end_lineno - node.lineno + 1
                else:
                    lines = len(node.body)  # 简化计算
                if lines > 50:
                    return True
        return False

    def _has_large_classes(self, tree: ast.AST) -> bool:
        """检测大类"""
        for node in ast.walk(tree):
            if isinstance(node, ast.ClassDef):
                if hasattr(node, 'end_lineno') and node.end_lineno:
                    lines = node.end_lineno - node.lineno + 1
                else:
                    lines = len([n for n in ast.walk(node) if isinstance(n, (ast.FunctionDef, ast.ClassDef))])
                if lines > 300:
                    return True
        return False

    def _has_high_complexity(self, tree: ast.AST) -> bool:
        """检测高复杂度"""
        def calculate_complexity(node):
            complexity = 1
            for child in ast.walk(node):
                if isinstance(child, (ast.If, ast.While, ast.For, ast.AsyncFor)):
                    complexity += 1
                elif isinstance(child, ast.ExceptHandler):
                    complexity += 1
                elif isinstance(child, ast.BoolOp):
                    complexity += len(child.values) - 1
            return complexity

        for node in ast.walk(tree):
            if isinstance(node, ast.FunctionDef):
                if calculate_complexity(node) > 10:
                    return True
        return False

    def _missing_documentation(self, tree: ast.AST) -> bool:
        """检测缺失文档"""
        for node in ast.walk(tree):
            if isinstance(node, (ast.FunctionDef, ast.ClassDef)):
                if not ast.get_docstring(node):
                    return True
        return False

    def _missing_type_hints(self, tree: ast.AST) -> bool:
        """检测缺失类型提示"""
        for node in ast.walk(tree):
            if isinstance(node, ast.FunctionDef):
                if not node.returns:
                    return True
                if not all(arg.annotation for arg in node.args.args):
                    return True
        return False

    def _has_hardcoded_values(self, content: str) -> bool:
        """检测硬编码值"""
        # 检查URL、密钥、数据库连接字符串等
        hardcoded_patterns = [
            r'http[s]?://[^\s\']+',
            r'password\s*=\s*[\'\"][^\'\"]+[\'\"]',
            r'api_key\s*=\s*[\'\"][^\'\"]+[\'\"]',
            r'secret\s*=\s*[\'\"][^\'\"]+[\'\"]',
            r'localhost:\d+',
            r'\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}'
        ]
        return any(re.search(pattern, content, re.IGNORECASE) for pattern in hardcoded_patterns)

    def _poor_exception_handling(self, tree: ast.AST) -> bool:
        """检测异常处理问题"""
        for node in ast.walk(tree):
            if isinstance(node, ast.ExceptHandler):
                if node.type is None:  # except:
                    return True
                if isinstance(node.type, ast.Name) and node.type.id == 'Exception':
                    if not node.body or len(node.body) == 1:
                        # 只有pass或简单的print
                        if isinstance(node.body[0], ast.Pass) or \
                           (isinstance(node.body[0], ast.Expr) and
                            isinstance(node.body[0].value, ast.Call)):
                            return True
        return False

    def _naming_convention_issues(self, tree: ast.AST) -> bool:
        """检测命名规范问题"""
        for node in ast.walk(tree):
            if isinstance(node, ast.FunctionDef):
                if not node.name.islower() or '_' not in node.name:
                    if not node.name.startswith('_'):  # 允许私有方法
                        return True
            elif isinstance(node, ast.ClassDef):
                if not node.name[0].isupper():
                    return True
        return False

    # 最佳实践检测方法
    def _has_virtual_environment(self) -> bool:
        """检测虚拟环境"""
        venv_indicators = [
            'venv/', 'env/', '.venv/', 'virtualenv/',
            'conda.yaml', 'environment.yml', 'Pipfile'
        ]
        return any((self.project_path / indicator).exists() for indicator in venv_indicators)

    def _has_dependency_management(self) -> bool:
        """检测依赖管理"""
        dep_files = ['requirements.txt', 'pyproject.toml', 'Pipfile', 'setup.py']
        return any((self.project_path / f).exists() for f in dep_files)

    def _has_unit_tests(self) -> bool:
        """检测单元测试"""
        test_patterns = [
            'test_', '_test.py', 'tests/', 'conftest.py',
            'pytest.ini', 'tox.ini'
        ]
        return any(
            (self.project_path / pattern).exists() or
            any(pattern in str(f).lower() for f in self.project_path.rglob("*.py"))
            for pattern in test_patterns
        )

    def _has_logging_implementation(self) -> bool:
        """检测日志实现"""
        for py_file in self.project_path.rglob("*.py"):
            if not any(excluded in str(py_file) for excluded in self.excluded_dirs):
                try:
                    content = py_file.read_text(encoding='utf-8')
                    if 'import logging' in content or 'logging.getLogger' in content:
                        return True
                except:
                    continue
        return False

    def _has_configuration_management(self) -> bool:
        """检测配置管理"""
        config_patterns = [
            'config.py', 'settings.py', '.env', 'config.yaml',
            'config.yml', 'settings.ini', 'config.json'
        ]
        return any((self.project_path / pattern).exists() for pattern in config_patterns)

    def _has_async_programming(self) -> bool:
        """检测异步编程"""
        for py_file in self.project_path.rglob("*.py"):
            if not any(excluded in str(py_file) for excluded in self.excluded_dirs):
                try:
                    content = py_file.read_text(encoding='utf-8')
                    if 'async def' in content or 'await ' in content:
                        return True
                except:
                    continue
        return False

    def _has_context_managers(self) -> bool:
        """检测上下文管理器"""
        for py_file in self.project_path.rglob("*.py"):
            if not any(excluded in str(py_file) for excluded in self.excluded_dirs):
                try:
                    with open(py_file, 'r', encoding='utf-8') as f:
                        content = f.read()
                        tree = ast.parse(content)

                        for node in ast.walk(tree):
                            if isinstance(node, ast.With):
                                return True
                except:
                    continue
        return False

    def _has_property_decorators(self) -> bool:
        """检测属性装饰器"""
        for py_file in self.project_path.rglob("*.py"):
            if not any(excluded in str(py_file) for excluded in self.excluded_dirs):
                try:
                    content = py_file.read_text(encoding='utf-8')
                    if '@property' in content:
                        return True
                except:
                    continue
        return False

    def _has_data_classes(self) -> bool:
        """检测数据类"""
        for py_file in self.project_path.rglob("*.py"):
            if not any(excluded in str(py_file) for excluded in self.excluded_dirs):
                try:
                    content = py_file.read_text(encoding='utf-8')
                    if '@dataclass' in content or 'dataclasses' in content:
                        return True
                except:
                    continue
        return False

    def _has_type_hints_usage(self) -> bool:
        """检测类型提示使用"""
        for py_file in self.project_path.rglob("*.py"):
            if not any(excluded in str(py_file) for excluded in self.excluded_dirs):
                try:
                    with open(py_file, 'r', encoding='utf-8') as f:
                        content = f.read()
                        tree = ast.parse(content)

                        for node in ast.walk(tree):
                            if isinstance(node, ast.FunctionDef) and node.returns:
                                return True
                            elif isinstance(node, ast.AnnAssign):  # 带类型注解的赋值
                                return True
                except:
                    continue
        return False

    # 辅助方法
    def _can_run_pytest(self) -> bool:
        """检查是否可以运行pytest"""
        try:
            subprocess.run(['pytest', '--version'], capture_output=True, timeout=5)
            return True
        except:
            return False

    def _extract_imports(self, tree: ast.AST) -> List[str]:
        """提取导入"""
        imports = []
        for node in ast.walk(tree):
            if isinstance(node, ast.Import):
                imports.extend([alias.name for alias in node.names])
            elif isinstance(node, ast.ImportFrom):
                module = node.module or ''
                imports.extend([f"{module}.{alias.name}" for alias in node.names])
        return list(set(imports))

    def _extract_module_dependencies(self, tree: ast.AST) -> List[str]:
        """提取模块依赖"""
        dependencies = []
        for node in ast.walk(tree):
            if isinstance(node, ast.Import):
                dependencies.extend([alias.name.split('.')[0] for alias in node.names])
            elif isinstance(node, ast.ImportFrom):
                if node.module:
                    dependencies.append(node.module.split('.')[0])
        return list(set(dependencies))

    def _calculate_complexity_score(self, tree: ast.AST) -> float:
        """计算复杂度评分"""
        def calculate_complexity(node):
            complexity = 1
            for child in ast.walk(node):
                if isinstance(child, (ast.If, ast.While, ast.For, ast.AsyncFor)):
                    complexity += 1
                elif isinstance(child, ast.ExceptHandler):
                    complexity += 1
                elif isinstance(child, ast.BoolOp):
                    complexity += len(child.values) - 1
            return complexity

        total_complexity = 0
        function_count = 0

        for node in ast.walk(tree):
            if isinstance(node, ast.FunctionDef):
                total_complexity += calculate_complexity(node)
                function_count += 1

        return total_complexity / max(function_count, 1)

    def _calculate_doc_coverage(self, tree: ast.AST) -> float:
        """计算文档覆盖率"""
        documented = 0
        total = 0

        for node in ast.walk(tree):
            if isinstance(node, (ast.FunctionDef, ast.ClassDef)):
                total += 1
                if ast.get_docstring(node):
                    documented += 1

        return (documented / total * 100) if total > 0 else 0

    def _calculate_type_hints_coverage(self, tree: ast.AST) -> float:
        """计算类型提示覆盖率"""
        with_hints = 0
        total_functions = 0

        for node in ast.walk(tree):
            if isinstance(node, ast.FunctionDef):
                total_functions += 1
                if node.returns and all(arg.annotation for arg in node.args.args):
                    with_hints += 1

        return (with_hints / total_functions * 100) if total_functions > 0 else 0

    # 安全检测方法
    def _has_hardcoded_secrets(self, content: str) -> bool:
        """检测硬编码密钥"""
        secret_patterns = [
            r'api[_-]?key\s*=\s*[\'\"][^\'\"]{10,}[\'\"]',
            r'secret[_-]?key\s*=\s*[\'\"][^\'\"]{10,}[\'\"]',
            r'password\s*=\s*[\'\"][^\'\"]{8,}[\'\"]',
            r'token\s*=\s*[\'\"][^\'\"]{10,}[\'\"]'
        ]
        return any(re.search(pattern, content, re.IGNORECASE) for pattern in secret_patterns)

    def _has_sql_injection_risks(self, content: str) -> bool:
        """检测SQL注入风险"""
        injection_patterns = [
            r'execute\s*\(\s*[\'\"]\s*.*%\s*.*[\'\"]',
            r'format.*sql',
            r'f\'[\'"]\s*.*\{.*\}.*\s*.*[\'\"].*sql'
        ]
        return any(re.search(pattern, content, re.IGNORECASE) for pattern in injection_patterns)

    def _has_dangerous_functions(self, content: str) -> bool:
        """检测危险函数"""
        dangerous_functions = ['eval(', 'exec(', 'compile(', '__import__(']
        return any(func in content for func in dangerous_functions)

    def _has_unsafe_pickle(self, content: str) -> bool:
        """检测不安全的pickle使用"""
        return re.search(r'pickle\.load\s*\(', content, re.IGNORECASE) is not None

def generate_python_report(analysis: PythonArchitectureAnalysis) -> str:
    """生成Python项目架构分析报告"""
    report = "# Python项目架构分析报告\n\n"

    # 项目概览
    report += "## 📊 项目概览\n"
    report += f"- **项目类型**: {analysis.project_type.value}\n"
    report += f"- **使用框架**: {', '.join([f.value for f in analysis.frameworks]) if analysis.frameworks else '未检测到'}\n"
    report += f"- **质量评分**: {analysis.quality_score:.1f}/100\n"
    report += f"- **架构模式**: {len(analysis.patterns)} 个\n"
    report += f"- **质量问题**: {len(analysis.quality_issues)} 个\n"
    report += f"- **最佳实践**: {len(analysis.best_practices)} 个\n\n"

    # 框架分析
    report += "## 🔧 框架分析\n"
    if analysis.frameworks:
        for framework in analysis.frameworks:
            report += f"- ✅ **{framework.value}**: 检测到该框架\n"
    else:
        report += "- ⚠️ 未检测到明确的框架\n"
    report += "\n"

    # 架构模式
    report += "## 🏗️ 架构模式识别\n"
    if analysis.patterns:
        for pattern in analysis.patterns:
            report += f"- ✅ **{pattern.value}**: 检测到该架构模式\n"
    else:
        report += "- 💡 建议考虑应用设计模式提高代码质量\n"
    report += "\n"

    # 质量问题
    report += "## ⚠️ 代码质量问题\n"
    if analysis.quality_issues:
        for issue in analysis.quality_issues:
            report += f"- 🚨 **{issue.value}**: 需要改进\n"
    else:
        report += "- ✅ 未检测到明显的代码质量问题\n"
    report += "\n"

    # 最佳实践
    report += "## 🎯 最佳实践评估\n"
    if analysis.best_practices:
        for practice in analysis.best_practices:
            report += f"- ✨ **{practice.value}**: 良好的实践\n"
    else:
        report += "- 💡 建议改进开发实践\n"
    report += "\n"

    # 模块分析
    report += "## 📦 模块分析\n"
    if analysis.modules:
        # 按复杂度排序
        sorted_modules = sorted(analysis.modules, key=lambda x: x.complexity_score, reverse=True)
        report += f"共分析了 {len(analysis.modules)} 个模块\n\n"

        report += "### 复杂度较高的模块:\n"
        for module in sorted_modules[:5]:  # 显示前5个复杂度较高的
            report += f"- **{module.name}**: 复杂度 {module.complexity_score:.1f}, {module.lines_of_code}行\n"
            report += f"  📁 `{module.file_path}`\n"
            report += f"  📋 函数: {len(module.functions)}, 类: {len(module.classes)}\n"

        report += "\n### 高质量模块:\n"
        for module in sorted_modules[-3:]:  # 显示最后3个质量较高的
            report += f"- **{module.name}**: 文档覆盖率 {module.documentation_coverage:.1f}%, 类型提示覆盖率 {module.type_hints_coverage:.1f}%\n"
    report += "\n"

    # 依赖分析
    report += "## 📋 依赖分析\n"
    if analysis.dependencies:
        report += f"共发现 {len(analysis.dependencies)} 个依赖包\n\n"
        for dep, version in list(analysis.dependencies.items())[:10]:  # 显示前10个
            report += f"- **{dep}**: {version}\n"
        if len(analysis.dependencies) > 10:
            report += f"- ... 还有 {len(analysis.dependencies) - 10} 个依赖\n"
    else:
        report += "- ⚠️ 未找到依赖配置文件\n"
    report += "\n"

    # 测试覆盖率
    report += "## 🧪 测试覆盖率\n"
    if analysis.test_coverage:
        if 'overall' in analysis.test_coverage:
            report += f"- **整体覆盖率**: {analysis.test_coverage['overall']:.1f}%\n"
        if 'test_to_source_ratio' in analysis.test_coverage:
            report += f"- **测试/源码比例**: {analysis.test_coverage['test_to_source_ratio']:.2f}\n"
    else:
        report += "- ⚠️ 无法获取测试覆盖率信息\n"
    report += "\n"

    # 安全问题
    report += "## 🔒 安全分析\n"
    if analysis.security_issues:
        for issue in analysis.security_issues:
            report += f"- 🚨 {issue}\n"
    else:
        report += "- ✅ 未检测到明显的安全问题\n"
    report += "\n"

    # 项目结构
    report += "## 📁 项目结构分析\n"
    for category, files in analysis.project_structure.items():
        if files:
            report += f"- **{category}**: {len(files)} 个文件\n"
    report += "\n"

    # 改进建议
    report += "## 💡 改进建议\n"
    for i, suggestion in enumerate(analysis.recommendations, 1):
        report += f"{i}. {suggestion}\n"
    report += "\n"

    # 质量评估
    report += "## 📈 质量评估\n"
    if analysis.quality_score >= 80:
        report += "🟢 **优秀**: 项目架构和代码质量很高\n"
    elif analysis.quality_score >= 60:
        report += "🟡 **良好**: 项目整体质量不错，有改进空间\n"
    elif analysis.quality_score >= 40:
        report += "🟠 **一般**: 存在一些问题，需要重点关注\n"
    else:
        report += "🔴 **需要改进**: 存在较多问题，建议进行重构\n"
    report += "\n"

    return report

if __name__ == "__main__":
    import sys
    if len(sys.argv) != 2:
        print("用法: python python_analyzer.py <Python项目路径>")
        sys.exit(1)

    project_path = Path(sys.argv[1])
    analyzer = PythonArchitectureAnalyzer(project_path)
    analysis = analyzer.analyze()
    report = generate_python_report(analysis)

    print(report)

    # 保存报告
    output_file = project_path / "python_architecture_analysis.md"
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(report)

    print(f"\n📄 报告已保存到: {output_file}")