#!/usr/bin/env python3
"""
前端项目专用架构分析器
专门分析React、Vue、Angular等前端项目的架构模式、代码质量、最佳实践
"""

import json
import re
import yaml
from pathlib import Path
from typing import Dict, List, Tuple, Optional, Set
from dataclasses import dataclass
from enum import Enum

class FrontendFramework(Enum):
    REACT = "React"
    VUE = "Vue.js"
    ANGULAR = "Angular"
    SVELTE = "Svelte"
    NEXTJS = "Next.js"
    Nuxt = "Nuxt.js"
    REACT_NATIVE = "React Native"
    VITE = "Vite"
    WEBPACK = "Webpack"
    PARCEL = "Parcel"

class FrontendArchitecturePattern(Enum):
    COMPONENT_BASED = "Component-Based Architecture"
    STATE_MANAGEMENT = "State Management Pattern"
    ROUTING_PATTERN = "Client-Side Routing"
    LAZY_LOADING = "Lazy Loading Pattern"
    CODE_SPLITING = "Code Splitting"
    HOC_PATTERN = "Higher-Order Component Pattern"
    RENDER_PROPS = "Render Props Pattern"
    COMPOSITION_API = "Composition API Pattern"
    OBSERVER_PATTERN = "Observer Pattern"
    PUB_SUB = "Publisher-Subscriber"
    MICRO_FRONTENDS = "Micro Frontends"
    SERVER_COMPONENTS = "Server Components"
    CONTEXT_API = "Context API Pattern"
    REDUX_PATTERN = "Redux/Flux Pattern"
    PROVIDER_PATTERN = "Provider Pattern"
    FACTORY_PATTERN = "Factory Pattern"
    STRATEGY_PATTERN = "Strategy Pattern"

class FrontendPerformanceIssue(Enum):
    UNNECESSARY_RERENDERS = "Unnecessary Rerenders"
    MISSING_KEYS = "Missing React Keys"
    INLINE_STYLES = "Inline Styles"
    LARGE_BUNDLES = "Large Bundle Sizes"
    MISSING_LAZY_LOADING = "Missing Lazy Loading"
    MEMORY_LEAKS = "Memory Leaks"
    INEFFICIENT_STATE = "Inefficient State Updates"
    MISSING_CODE_SPLITING = "Missing Code Splitting"
    PROP_DRILLING = "Prop Drilling"
    INEFFICIENT_EFFECTS = "Inefficient useEffect Usage"
    UNOPTIMIZED_IMAGES = "Unoptimized Images"
    MISSING_CACHING = "Missing Caching Strategy"

class FrontendBestPractice(Enum):
    COMPONENT_TESTS = "Component Testing"
    STORYBOOK = "Storybook Usage"
    TYPESCRIPT = "TypeScript Usage"
    ESLINT = "ESLint Configuration"
    PRETTIER = "Prettier Configuration"
    HUSKY = "Git Hooks (Husky)"
    CI_CD = "CI/CD Pipeline"
    PERFORMANCE_MONITORING = "Performance Monitoring"
    ERROR_BOUNDARIES = "Error Boundaries"
    ACCESSIBILITY = "Accessibility (A11y)"
    SEO_OPTIMIZATION = "SEO Optimization"
    PWA_FEATURES = "PWA Features"
    RESPONSIVE_DESIGN = "Responsive Design"
    TREE_SHAKING = "Tree Shaking"
    BUNDLE_ANALYSIS = "Bundle Analysis"

@dataclass
class FrontendComponentInfo:
    name: str
    type: str  # Functional, Class, Container, Presentational
    file_path: str
    lines_of_code: int
    props: List[str]
    hooks: List[str]  # React hooks or Vue composition API
    state_management: str
    dependencies: List[str]
    performance_score: float

@dataclass
class FrontendProjectInfo:
    framework: FrontendFramework
    version: str
    build_tool: str
    package_manager: str
    css_framework: str
    testing_framework: str
    state_management: str
    routing: str
    ui_library: str
    dev_server: str

@dataclass
class FrontendArchitectureAnalysis:
    project_info: FrontendProjectInfo
    patterns: List[FrontendArchitecturePattern]
    performance_issues: List[FrontendPerformanceIssue]
    best_practices: List[FrontendBestPractice]
    components: List[FrontendComponentInfo]
    bundle_analysis: Dict[str, any]
    project_structure: Dict[str, List[str]]
    dependencies: Dict[str, str]
    test_coverage: Dict[str, float]
    recommendations: List[str]
    quality_score: float
    accessibility_score: float
    performance_score: float

class FrontendArchitectureAnalyzer:
    def __init__(self, project_path: Path):
        self.project_path = project_path
        self.excluded_dirs = {
            'node_modules', '.git', 'dist', 'build', 'coverage',
            '.nyc_output', '.next', '.nuxt', '.cache'
        }

    def analyze(self) -> FrontendArchitectureAnalysis:
        """执行前端项目的全面架构分析"""

        # 识别项目信息
        project_info = self._identify_project_info()

        # 检测架构模式
        patterns = self._detect_patterns()

        # 检测性能问题
        performance_issues = self._detect_performance_issues()

        # 评估最佳实践
        best_practices = self._evaluate_best_practices()

        # 分析组件
        components = self._analyze_components()

        # 分析打包文件
        bundle_analysis = self._analyze_bundles()

        # 分析项目结构
        project_structure = self._analyze_project_structure()

        # 分析依赖
        dependencies = self._analyze_dependencies()

        # 分析测试覆盖率
        test_coverage = self._analyze_test_coverage()

        # 计算可访问性评分
        accessibility_score = self._calculate_accessibility_score()

        # 计算性能评分
        performance_score = self._calculate_performance_score(
            performance_issues, bundle_analysis
        )

        # 生成建议
        recommendations = self._generate_recommendations(
            project_info, patterns, performance_issues, best_practices,
            bundle_analysis, accessibility_score
        )

        # 计算整体质量评分
        quality_score = self._calculate_quality_score(
            patterns, performance_issues, best_practices, test_coverage,
            accessibility_score, performance_score
        )

        return FrontendArchitectureAnalysis(
            project_info=project_info,
            patterns=patterns,
            performance_issues=performance_issues,
            best_practices=best_practices,
            components=components,
            bundle_analysis=bundle_analysis,
            project_structure=project_structure,
            dependencies=dependencies,
            test_coverage=test_coverage,
            recommendations=recommendations,
            quality_score=quality_score,
            accessibility_score=accessibility_score,
            performance_score=performance_score
        )

    def _identify_project_info(self) -> FrontendProjectInfo:
        """识别前端项目信息"""
        # 检查package.json
        package_file = self.project_path / 'package.json'
        if not package_file.exists():
            return FrontendProjectInfo(
                framework=FrontendFramework.REACT,  # 默认值
                version="Unknown",
                build_tool="Unknown",
                package_manager="Unknown",
                css_framework="Unknown",
                testing_framework="Unknown",
                state_management="Unknown",
                routing="Unknown",
                ui_library="Unknown",
                dev_server="Unknown"
            )

        try:
            with open(package_file, 'r', encoding='utf-8') as f:
                package_data = json.load(f)

            # 识别框架
            framework = self._detect_framework(package_data)
            version = self._extract_framework_version(package_data, framework)

            # 识别构建工具
            build_tool = self._detect_build_tool(package_data)

            # 识别包管理器
            package_manager = self._detect_package_manager()

            # 识别CSS框架
            css_framework = self._detect_css_framework(package_data)

            # 识别测试框架
            testing_framework = self._detect_testing_framework(package_data)

            # 识别状态管理
            state_management = self._detect_state_management(package_data)

            # 识别路由
            routing = self._detect_routing(package_data)

            # 识别UI库
            ui_library = self._detect_ui_library(package_data)

            # 识别开发服务器
            dev_server = self._detect_dev_server(package_data)

            return FrontendProjectInfo(
                framework=framework,
                version=version,
                build_tool=build_tool,
                package_manager=package_manager,
                css_framework=css_framework,
                testing_framework=testing_framework,
                state_management=state_management,
                routing=routing,
                ui_library=ui_library,
                dev_server=dev_server
            )

        except Exception:
            return FrontendProjectInfo(
                framework=FrontendFramework.REACT,  # 默认值
                version="Unknown",
                build_tool="Unknown",
                package_manager="Unknown",
                css_framework="Unknown",
                testing_framework="Unknown",
                state_management="Unknown",
                routing="Unknown",
                ui_library="Unknown",
                dev_server="Unknown"
            )

    def _detect_framework(self, package_data: dict) -> FrontendFramework:
        """检测前端框架"""
        dependencies = {**package_data.get('dependencies', {}), **package_data.get('devDependencies', {})}

        # React生态系统
        if 'react' in dependencies:
            if 'next' in dependencies:
                return FrontendFramework.NEXTJS
            elif 'react-native' in dependencies:
                return FrontendFramework.REACT_NATIVE
            else:
                return FrontendFramework.REACT

        # Vue生态系统
        elif 'vue' in dependencies:
            if 'nuxt' in dependencies:
                return FrontendFramework.Nuxt
            else:
                return FrontendFramework.VUE

        # Angular
        elif '@angular/core' in dependencies:
            return FrontendFramework.ANGULAR

        # Svelte
        elif 'svelte' in dependencies:
            return FrontendFramework.SVELTE

        # 构建工具作为备选
        elif 'vite' in dependencies:
            return FrontendFramework.VITE
        elif 'webpack' in dependencies:
            return FrontendFramework.WEBPACK
        elif 'parcel' in dependencies:
            return FrontendFramework.PARCEL

        return FrontendFramework.REACT  # 默认值

    def _extract_framework_version(self, package_data: dict, framework: FrontendFramework) -> str:
        """提取框架版本"""
        dependencies = {**package_data.get('dependencies', {}), **package_data.get('devDependencies', {})}

        framework_keys = {
            FrontendFramework.REACT: 'react',
            FrontendFramework.NEXTJS: 'next',
            FrontendFramework.VUE: 'vue',
            FrontendFramework.Nuxt: 'nuxt',
            FrontendFramework.ANGULAR: '@angular/core',
            FrontendFramework.SVELTE: 'svelte',
            FrontendFramework.VITE: 'vite'
        }

        key = framework_keys.get(framework)
        if key and key in dependencies:
            return dependencies[key]

        return "Unknown"

    def _detect_build_tool(self, package_data: dict) -> str:
        """检测构建工具"""
        dependencies = {**package_data.get('dependencies', {}), **package_data.get('devDependencies', {})}

        if 'vite' in dependencies:
            return "Vite"
        elif 'webpack' in dependencies:
            return "Webpack"
        elif 'parcel' in dependencies:
            return "Parcel"
        elif 'rollup' in dependencies:
            return "Rollup"
        elif 'esbuild' in dependencies:
            return "esbuild"
        elif 'turbo' in dependencies:
            return "Turbopack"

        # 检查配置文件
        config_files = ['vite.config.', 'webpack.config.', 'rollup.config.', 'parcel.config.']
        for config_file in config_files:
            if any(self.project_path.glob(f"{config_file}*")):
                return config_file.split('.')[0].title()

        return "Unknown"

    def _detect_package_manager(self) -> str:
        """检测包管理器"""
        if (self.project_path / 'pnpm-lock.yaml').exists():
            return "pnpm"
        elif (self.project_path / 'yarn.lock').exists():
            return "yarn"
        elif (self.project_path / 'package-lock.json').exists():
            return "npm"
        elif (self.project_path / 'bun.lockb').exists():
            return "bun"

        return "Unknown"

    def _detect_css_framework(self, package_data: dict) -> str:
        """检测CSS框架"""
        dependencies = {**package_data.get('dependencies', {}), **package_data.get('devDependencies', {})}

        if 'tailwindcss' in dependencies:
            return "Tailwind CSS"
        elif 'bootstrap' in dependencies:
            return "Bootstrap"
        elif '@mui/material' in dependencies or '@mui/core' in dependencies:
            return "Material-UI"
        elif '@chakra-ui/react' in dependencies:
            return "Chakra UI"
        elif 'antd' in dependencies:
            return "Ant Design"
        elif '@headlessui/react' in dependencies:
            return "Headless UI"
        elif '@emotion/react' in dependencies:
            return "Emotion"
        elif 'styled-components' in dependencies:
            return "Styled Components"
        elif 'bulma' in dependencies:
            return "Bulma"

        return "Unknown"

    def _detect_testing_framework(self, package_data: dict) -> str:
        """检测测试框架"""
        dependencies = {**package_data.get('dependencies', {}), **package_data.get('devDependencies', {})}

        if 'jest' in dependencies:
            return "Jest"
        elif 'vitest' in dependencies:
            return "Vitest"
        elif '@testing-library/react' in dependencies:
            return "React Testing Library"
        elif '@testing-library/vue' in dependencies:
            return "Vue Testing Library"
        elif 'cypress' in dependencies:
            return "Cypress"
        elif 'playwright' in dependencies:
            return "Playwright"
        elif 'storybook' in dependencies:
            return "Storybook"

        return "Unknown"

    def _detect_state_management(self, package_data: dict) -> str:
        """检测状态管理方案"""
        dependencies = {**package_data.get('dependencies', {}), **package_data.get('devDependencies', {})}

        if 'redux' in dependencies or '@reduxjs/toolkit' in dependencies:
            return "Redux"
        elif 'mobx' in dependencies:
            return "MobX"
        elif 'zustand' in dependencies:
            return "Zustand"
        elif 'recoil' in dependencies:
            return "Recoil"
        elif 'jotai' in dependencies:
            return "Jotai"
        elif 'pinia' in dependencies:
            return "Pinia"
        elif 'vuex' in dependencies:
            return "Vuex"
        elif 'valtio' in dependencies:
            return "Valtio"

        # 内置方案
        if 'react' in dependencies:
            return "React Context/useState"
        elif 'vue' in dependencies:
            return "Vue Reactivity System"

        return "Unknown"

    def _detect_routing(self, package_data: dict) -> str:
        """检测路由方案"""
        dependencies = {**package_data.get('dependencies', {}), **package_data.get('devDependencies', {})}

        if 'react-router-dom' in dependencies:
            return "React Router"
        elif 'vue-router' in dependencies:
            return "Vue Router"
        elif '@angular/router' in dependencies:
            return "Angular Router"
        elif '@reach/router' in dependencies:
            return "Reach Router"
        elif 'next' in dependencies:  # Next.js内置路由
            return "Next.js App Router"
        elif 'nuxt' in dependencies:  # Nuxt.js内置路由
            return "Nuxt.js File-based Routing"

        return "Unknown"

    def _detect_ui_library(self, package_data: dict) -> str:
        """检测UI组件库"""
        dependencies = {**package_data.get('dependencies', {}), **package_data.get('devDependencies', {})}

        if '@mui/material' in dependencies or '@mui/core' in dependencies:
            return "Material-UI"
        elif 'antd' in dependencies:
            return "Ant Design"
        elif '@chakra-ui/react' in dependencies:
            return "Chakra UI"
        elif 'react-bootstrap' in dependencies:
            return "React Bootstrap"
        elif 'element-plus' in dependencies:
            return "Element Plus"
        elif 'primevue' in dependencies:
            return "PrimeVue"
        elif '@headlessui/react' in dependencies:
            return "Headless UI"
        elif 'radix-ui' in dependencies:
            return "Radix UI"

        return "Unknown"

    def _detect_dev_server(self, package_data: dict) -> str:
        """检测开发服务器"""
        scripts = package_data.get('scripts', {})

        for script_name, script_command in scripts.items():
            if 'vite' in script_command:
                return "Vite Dev Server"
            elif 'webpack' in script_command:
                return "Webpack Dev Server"
            elif 'next dev' in script_command:
                return "Next.js Dev Server"
            elif 'nuxt dev' in script_command:
                return "Nuxt.js Dev Server"
            elif 'ng serve' in script_command:
                return "Angular CLI Dev Server"
            elif 'parcel' in script_command:
                return "Parcel Dev Server"

        # 基于构建工具推断
        build_tool = self._detect_build_tool(package_data)
        if build_tool in ["Vite", "Webpack", "Parcel"]:
            return f"{build_tool} Dev Server"

        return "Unknown"

    def _detect_patterns(self) -> List[FrontendArchitecturePattern]:
        """检测前端架构模式"""
        patterns = []

        # 获取所有源代码文件
        source_files = self._get_source_files()

        # 分析所有文件内容
        all_content = ""
        for file_path in source_files:
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    all_content += f.read() + "\n"
            except:
                continue

        # 检测各种模式
        if self._has_component_based_architecture(source_files):
            patterns.append(FrontendArchitecturePattern.COMPONENT_BASED)

        if self._has_state_management_pattern(all_content):
            patterns.append(FrontendArchitecturePattern.STATE_MANAGEMENT)

        if self._has_routing_pattern(all_content):
            patterns.append(FrontendArchitecturePattern.ROUTING_PATTERN)

        if self._has_lazy_loading_pattern(all_content):
            patterns.append(FrontendArchitecturePattern.LAZY_LOADING)

        if self._has_code_splitting_pattern(all_content):
            patterns.append(FrontendArchitecturePattern.CODE_SPLITING)

        if self._has_hoc_pattern(all_content):
            patterns.append(FrontendArchitecturePattern.HOC_PATTERN)

        if self._has_render_props_pattern(all_content):
            patterns.append(FrontendArchitecturePattern.RENDER_PROPS)

        if self._has_composition_api_pattern(all_content):
            patterns.append(FrontendArchitecturePattern.COMPOSITION_API)

        if self._has_observer_pattern(all_content):
            patterns.append(FrontendArchitecturePattern.OBSERVER_PATTERN)

        if self._has_pub_sub_pattern(all_content):
            patterns.append(FrontendArchitecturePattern.PUB_SUB)

        if self._has_micro_frontends_pattern():
            patterns.append(FrontendArchitecturePattern.MICRO_FRONTENDS)

        if self._has_server_components_pattern(all_content):
            patterns.append(FrontendArchitecturePattern.SERVER_COMPONENTS)

        if self._has_context_api_pattern(all_content):
            patterns.append(FrontendArchitecturePattern.CONTEXT_API)

        if self._has_redux_pattern(all_content):
            patterns.append(FrontendArchitecturePattern.REDUX_PATTERN)

        if self._has_provider_pattern(all_content):
            patterns.append(FrontendArchitecturePattern.PROVIDER_PATTERN)

        if self._has_factory_pattern(all_content):
            patterns.append(FrontendArchitecturePattern.FACTORY_PATTERN)

        if self._has_strategy_pattern(all_content):
            patterns.append(FrontendArchitecturePattern.STRATEGY_PATTERN)

        return patterns

    def _detect_performance_issues(self) -> List[FrontendPerformanceIssue]:
        """检测前端性能问题"""
        issues = []

        source_files = self._get_source_files()

        for file_path in source_files:
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()

                # 检测各种性能问题
                if self._has_unnecessary_rerenders(content):
                    issues.append(FrontendPerformanceIssue.UNNECESSARY_RERENDERS)

                if self._has_missing_react_keys(content):
                    issues.append(FrontendPerformanceIssue.MISSING_KEYS)

                if self._has_inline_styles(content):
                    issues.append(FrontendPerformanceIssue.INLINE_STYLES)

                if self._has_memory_leak_risks(content):
                    issues.append(FrontendPerformanceIssue.MEMORY_LEAKS)

                if self._has_inefficient_state_updates(content):
                    issues.append(FrontendPerformanceIssue.INEFFICIENT_STATE)

                if self._has_prop_drilling(content):
                    issues.append(FrontendPerformanceIssue.PROP_DRILLING)

                if self._has_inefficient_effects(content):
                    issues.append(FrontendPerformanceIssue.INEFFICIENT_EFFECTS)

            except:
                continue

        # 检测打包相关的性能问题
        if self._has_large_bundles():
            issues.append(FrontendPerformanceIssue.LARGE_BUNDLES)

        if self._has_missing_lazy_loading():
            issues.append(FrontendPerformanceIssue.MISSING_LAZY_LOADING)

        if self._has_missing_code_splitting():
            issues.append(FrontendPerformanceIssue.MISSING_CODE_SPLITING)

        if self._has_unoptimized_images():
            issues.append(FrontendPerformanceIssue.UNOPTIMIZED_IMAGES)

        if self._has_missing_caching_strategy():
            issues.append(FrontendPerformanceIssue.MISSING_CACHING)

        return list(set(issues))  # 去重

    def _evaluate_best_practices(self) -> List[FrontendBestPractice]:
        """评估前端最佳实践"""
        practices = []

        if self._has_component_tests():
            practices.append(FrontendBestPractice.COMPONENT_TESTS)

        if self._has_storybook():
            practices.append(FrontendBestPractice.STORYBOOK)

        if self._has_typescript():
            practices.append(FrontendBestPractice.TYPESCRIPT)

        if self._has_eslint():
            practices.append(FrontendBestPractice.ESLINT)

        if self._has_prettier():
            practices.append(FrontendBestPractice.PRETTIER)

        if self._has_husky():
            practices.append(FrontendBestPractice.HUSKY)

        if self._has_ci_cd():
            practices.append(FrontendBestPractice.CI_CD)

        if self._has_performance_monitoring():
            practices.append(FrontendBestPractice.PERFORMANCE_MONITORING)

        if self._has_error_boundaries():
            practices.append(FrontendBestPractice.ERROR_BOUNDARIES)

        if self._has_accessibility_features():
            practices.append(FrontendBestPractice.ACCESSIBILITY)

        if self._has_seo_optimization():
            practices.append(FrontendBestPractice.SEO_OPTIMIZATION)

        if self._has_pwa_features():
            practices.append(FrontendBestPractice.PWA_FEATURES)

        if self._has_responsive_design():
            practices.append(FrontendBestPractice.RESPONSIVE_DESIGN)

        if self._has_tree_shaking():
            practices.append(FrontendBestPractice.TREE_SHAKING)

        if self._has_bundle_analysis():
            practices.append(FrontendBestPractice.BUNDLE_ANALYSIS)

        return practices

    def _analyze_components(self) -> List[FrontendComponentInfo]:
        """分析前端组件"""
        components = []

        source_files = self._get_source_files()

        for file_path in source_files:
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()

                # 提取组件信息
                component_name = self._extract_component_name(content, file_path.name)
                component_type = self._detect_component_type(content)
                lines_of_code = len([line for line in content.split('\n') if line.strip()])
                props = self._extract_component_props(content)
                hooks = self._extract_component_hooks(content)
                state_management = self._detect_component_state_management(content)
                dependencies = self._extract_component_dependencies(content)
                performance_score = self._calculate_component_performance_score(content)

                components.append(FrontendComponentInfo(
                    name=component_name,
                    type=component_type,
                    file_path=str(file_path.relative_to(self.project_path)),
                    lines_of_code=lines_of_code,
                    props=props,
                    hooks=hooks,
                    state_management=state_management,
                    dependencies=dependencies,
                    performance_score=performance_score
                ))

            except:
                continue

        return components

    def _analyze_bundles(self) -> Dict[str, any]:
        """分析打包文件"""
        bundle_analysis = {
            "has_bundle_analysis": False,
            "bundle_sizes": {},
            "compression_ratio": {},
            "chunk_analysis": {}
        }

        # 检查是否有bundle分析文件
        bundle_report_files = [
            'bundle-analyzer-report.html',
            'stats.json',
            'webpack-bundle-analyzer-plugin.json'
        ]

        build_dirs = ['dist', 'build', '.next', '.nuxt']
        for build_dir in build_dirs:
            build_path = self.project_path / build_dir
            if build_path.exists():
                for report_file in bundle_report_files:
                    report_path = build_path / report_file
                    if report_path.exists():
                        bundle_analysis["has_bundle_analysis"] = True
                        break

        # 检查打包文件大小
        for build_dir in build_dirs:
            build_path = self.project_path / build_dir
            if build_path.exists():
                bundle_analysis["bundle_sizes"][build_dir] = self._calculate_bundle_sizes(build_path)

        return bundle_analysis

    def _analyze_project_structure(self) -> Dict[str, List[str]]:
        """分析项目结构"""
        structure = {
            "components": [],
            "pages": [],
            "hooks": [],
            "utils": [],
            "styles": [],
            "assets": [],
            "tests": [],
            "config": [],
            "types": []
        }

        for item in self.project_path.rglob("*"):
            if item.is_file() and not any(excluded in str(item) for excluded in self.excluded_dirs):
                rel_path = str(item.relative_to(self.project_path))
                parent_dir = item.parent.name.lower()
                file_ext = item.suffix.lower()

                if self._is_component_file(item):
                    structure["components"].append(rel_path)
                elif parent_dir in ["pages", "views", "screens"]:
                    structure["pages"].append(rel_path)
                elif parent_dir in ["hooks", "composables"]:
                    structure["hooks"].append(rel_path)
                elif parent_dir in ["utils", "helpers", "lib"]:
                    structure["utils"].append(rel_path)
                elif file_ext in [".css", ".scss", ".sass", ".less"]:
                    structure["styles"].append(rel_path)
                elif file_ext in [".jpg", ".png", ".svg", ".gif", ".webp"]:
                    structure["assets"].append(rel_path)
                elif "test" in parent_dir or file_ext in [".test.js", ".test.ts", ".spec.js", ".spec.ts"]:
                    structure["tests"].append(rel_path)
                elif file_ext in [".config.js", ".config.ts", ".json", ".yml", ".yaml"]:
                    structure["config"].append(rel_path)
                elif file_ext in [".d.ts"]:
                    structure["types"].append(rel_path)

        return structure

    def _analyze_dependencies(self) -> Dict[str, str]:
        """分析项目依赖"""
        dependencies = {}

        package_file = self.project_path / 'package.json'
        if package_file.exists():
            try:
                with open(package_file, 'r', encoding='utf-8') as f:
                    package_data = json.load(f)

                all_deps = {**package_data.get('dependencies', {}), **package_data.get('devDependencies', {})}
                dependencies.update(all_deps)

            except:
                pass

        return dependencies

    def _analyze_test_coverage(self) -> Dict[str, float]:
        """分析测试覆盖率"""
        coverage = {}

        # 检查是否有测试覆盖率报告
        coverage_files = [
            'coverage/lcov-report/index.html',
            'coverage/coverage-summary.json',
            'coverage/clover.xml'
        ]

        for coverage_file in coverage_files:
            coverage_path = self.project_path / coverage_file
            if coverage_path.exists():
                try:
                    if coverage_file.endswith('.json'):
                        with open(coverage_path, 'r', encoding='utf-8') as f:
                            coverage_data = json.load(f)
                            coverage["overall"] = coverage_data.get("total", {}).get("lines", {}).get("pct", 0)
                            break
                except:
                    continue

        # 计算测试文件比例
        test_files = [f for f in self._get_source_files() if any(keyword in str(f).lower() for keyword in ["test", "spec"])]
        source_files = [f for f in self._get_source_files() if not any(keyword in str(f).lower() for keyword in ["test", "spec"])]

        if source_files:
            coverage["test_to_source_ratio"] = len(test_files) / len(source_files) * 100
        else:
            coverage["test_to_source_ratio"] = 0

        return coverage

    def _calculate_accessibility_score(self) -> float:
        """计算可访问性评分"""
        score = 50.0  # 基础分数

        source_files = self._get_source_files()

        for file_path in source_files:
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()

                # 检查可访问性特性
                if self._has_aria_labels(content):
                    score += 5
                if self._has_alt_attributes(content):
                    score += 5
                if self._has_semantic_html(content):
                    score += 5
                if self._has_keyboard_navigation(content):
                    score += 5
                if self._has_screen_reader_support(content):
                    score += 5

            except:
                continue

        return min(100.0, score)

    def _calculate_performance_score(self, performance_issues: List[FrontendPerformanceIssue], bundle_analysis: Dict[str, any]) -> float:
        """计算性能评分"""
        score = 100.0

        # 性能问题扣分
        issue_penalties = {
            FrontendPerformanceIssue.UNNECESSARY_RERENDERS: 15,
            FrontendPerformanceIssue.MISSING_KEYS: 10,
            FrontendPerformanceIssue.INLINE_STYLES: 5,
            FrontendPerformanceIssue.LARGE_BUNDLES: 20,
            FrontendPerformanceIssue.MISSING_LAZY_LOADING: 10,
            FrontendPerformanceIssue.MEMORY_LEAKS: 20,
            FrontendPerformanceIssue.INEFFICIENT_STATE: 10,
            FrontendPerformanceIssue.MISSING_CODE_SPLITING: 10,
            FrontendPerformanceIssue.PROP_DRILLING: 5,
            FrontendPerformanceIssue.INEFFICIENT_EFFECTS: 10,
            FrontendPerformanceIssue.UNOPTIMIZED_IMAGES: 10,
            FrontendPerformanceIssue.MISSING_CACHING: 8
        }

        for issue in performance_issues:
            score -= issue_penalties.get(issue, 5)

        # 打包分析加分/扣分
        if bundle_analysis.get("bundle_sizes"):
            for build_dir, sizes in bundle_analysis["bundle_sizes"].items():
                total_size = sum(sizes.values())
                if total_size < 1024 * 1024:  # < 1MB
                    score += 5
                elif total_size > 5 * 1024 * 1024:  # > 5MB
                    score -= 10

        return max(0.0, score)

    def _generate_recommendations(self, project_info: FrontendProjectInfo,
                                 patterns: List[FrontendArchitecturePattern],
                                 performance_issues: List[FrontendPerformanceIssue],
                                 best_practices: List[FrontendBestPractice],
                                 bundle_analysis: Dict[str, any],
                                 accessibility_score: float) -> List[str]:
        """生成改进建议"""
        recommendations = []

        # 基于框架的建议
        if project_info.framework == FrontendFramework.REACT:
            recommendations.extend([
                "🔄 **使用React.memo**: 对性能敏感组件使用memo减少重渲染",
                "🎣 **自定义Hooks**: 提取重复逻辑到自定义Hooks中",
                "📦 **代码分割**: 使用React.lazy()和Suspense实现代码分割"
            ])

        elif project_info.framework == FrontendFramework.VUE:
            recommendations.extend([
                "🔧 **Composition API**: 使用Vue 3 Composition API提高代码复用",
                "🏭 **Teleport**: 使用Teleport处理模态框和弹出层",
                "📦 **异步组件**: 使用defineAsyncComponent实现组件懒加载"
            ])

        # 基于性能问题的建议
        if FrontendPerformanceIssue.UNNECESSARY_RERENDERS in performance_issues:
            recommendations.append("⚡ **减少重渲染**: 使用React.memo、Vue.memo或shouldComponentUpdate")

        if FrontendPerformanceIssue.MISSING_KEYS in performance_issues:
            recommendations.append("🔑 **添加Keys**: 为列表项添加稳定的key属性")

        if FrontendPerformanceIssue.LARGE_BUNDLES in performance_issues:
            recommendations.append("📦 **优化打包**: 实现代码分割和tree-shaking")

        if FrontendPerformanceIssue.MISSING_LAZY_LOADING in performance_issues:
            recommendations.append("🐌 **懒加载**: 对图片、路由和组件实现懒加载")

        # 基于最佳实践的建议
        if FrontendBestPractice.COMPONENT_TESTS not in best_practices:
            recommendations.append("🧪 **组件测试**: 添加单元测试和集成测试")

        if FrontendBestPractice.TYPESCRIPT not in best_practices:
            recommendations.append("📘 **TypeScript**: 迁移到TypeScript提高类型安全")

        if FrontendBestPractice.ESLINT not in best_practices:
            recommendations.append("📝 **代码规范**: 配置ESLint和Prettier保证代码质量")

        if FrontendBestPractice.PERFORMANCE_MONITORING not in best_practices:
            recommendations.append("📊 **性能监控**: 集成性能监控工具实时跟踪用户体验")

        # 基于可访问性的建议
        if accessibility_score < 70:
            recommendations.extend([
                "♿ **ARIA标签**: 为交互元素添加适当的ARIA标签",
                "🖼️ **图片替代**: 为图片添加alt属性",
                "⌨️ **键盘导航**: 确保所有功能可通过键盘访问"
            ])

        # 基于架构模式的建议
        if not patterns:
            recommendations.append("🏗️ **架构模式**: 考虑应用设计模式提高代码质量")

        # 通用建议
        recommendations.extend([
            "🎨 **CSS优化**: 使用CSS-in-JS或CSS模块避免样式冲突",
            "🔍 **SEO优化**: 添加meta标签和结构化数据",
            "📱 **响应式设计**: 确保在各种设备上的良好体验",
            "🔐 **安全实践**: 实施CSP策略和输入验证",
            "🚀 **PWA功能**: 考虑添加Service Worker和离线支持"
        ])

        return recommendations

    def _calculate_quality_score(self, patterns: List[FrontendArchitecturePattern],
                                 performance_issues: List[FrontendPerformanceIssue],
                                 best_practices: List[FrontendBestPractice],
                                 test_coverage: Dict[str, float],
                                 accessibility_score: float,
                                 performance_score: float) -> float:
        """计算整体质量评分"""
        base_score = 30.0

        # 架构模式加分 (每个模式+4分)
        pattern_score = len(patterns) * 4

        # 最佳实践加分 (每个实践+3分)
        practice_score = len(best_practices) * 3

        # 性能问题扣分 (每个问题-6分)
        issue_penalty = len(performance_issues) * 6

        # 测试覆盖率加分
        test_score = test_coverage.get("overall", 0) * 0.2

        # 可访问性评分
        a11y_score = accessibility_score * 0.2

        # 性能评分
        perf_score = performance_score * 0.3

        final_score = base_score + pattern_score + practice_score + test_score + a11y_score + perf_score - issue_penalty
        return max(0.0, min(100.0, final_score))

    # 辅助方法
    def _get_source_files(self) -> List[Path]:
        """获取所有源代码文件"""
        extensions = ['.js', '.jsx', '.ts', '.tsx', '.vue', '.svelte']
        source_files = []

        for ext in extensions:
            source_files.extend(self.project_path.rglob(f"*{ext}"))

        # 过滤排除目录
        return [
            f for f in source_files
            if not any(excluded in str(f) for excluded in self.excluded_dirs)
        ]

    def _is_component_file(self, file_path: Path) -> bool:
        """判断是否为组件文件"""
        file_name = file_path.name.lower()
        parent_dir = file_path.parent.name.lower()
        file_ext = file_path.suffix.lower()

        return (
            file_name.startswith('index.') or
            parent_dir in ['components', 'pages', 'views'] or
            any(keyword in file_name for keyword in ['button', 'input', 'modal', 'dialog', 'card', 'header', 'footer'])
        )

    # 架构模式检测方法
    def _has_component_based_architecture(self, source_files: List[Path]) -> bool:
        """检测组件化架构"""
        return len(source_files) > 0 and any(
            'export' in f.read_text() or 'export default' in f.read_text()
            for f in source_files[:10]  # 检查前10个文件
            if f.is_file()
        )

    def _has_state_management_pattern(self, content: str) -> bool:
        """检测状态管理模式"""
        state_indicators = [
            'useState', 'useReducer', 'useContext', 'createContext',
            'createStore', 'dispatch', 'subscribe', 'getState',
            'ref', 'reactive', 'computed', 'watch'
        ]
        return any(indicator in content for indicator in state_indicators)

    def _has_routing_pattern(self, content: str) -> bool:
        """检测路由模式"""
        routing_indicators = [
            'useRouter', 'useHistory', 'useLocation', 'navigate',
            'Link', 'Route', 'Switch', 'Redirect',
            'router-link', 'router-view', 'vue-router'
        ]
        return any(indicator in content for indicator in routing_indicators)

    def _has_lazy_loading_pattern(self, content: str) -> bool:
        """检测懒加载模式"""
        lazy_indicators = [
            'React.lazy', 'lazy', 'Suspense',
            'defineAsyncComponent', 'import(',
            'loadable', 'dynamic import'
        ]
        return any(indicator in content for indicator in lazy_indicators)

    def _has_code_splitting_pattern(self, content: str) -> bool:
        """检测代码分割模式"""
        split_indicators = [
            'import(', 'webpackChunkName', 'splitChunks',
            'dynamic import', 'lazy loading'
        ]
        return any(indicator in content for indicator in split_indicators)

    def _has_hoc_pattern(self, content: str) -> bool:
        """检测高阶组件模式"""
        hoc_indicators = [
            'withRouter', 'withStyles', 'connect',
            'export default compose(', 'export default hoc('
        ]
        return any(indicator in content for indicator in hoc_indicators)

    def _has_render_props_pattern(self, content: str) -> bool:
        """检测render props模式"""
        render_props_indicators = [
            'render={', 'children={', 'this.props.render',
            'this.props.children('
        ]
        return any(indicator in content for indicator in render_props_indicators)

    def _has_composition_api_pattern(self, content: str) -> bool:
        """检测Composition API模式"""
        composition_indicators = [
            'setup(', 'ref(', 'reactive(', 'computed(',
            'watch(', 'onMounted(', 'defineComponent'
        ]
        return any(indicator in content for indicator in composition_indicators)

    def _has_observer_pattern(self, content: str) -> bool:
        """检测观察者模式"""
        observer_indicators = [
            'useEffect', 'addEventListener', 'removeEventListener',
            'subscribe(', 'unsubscribe(', 'watch('
        ]
        return any(indicator in content for indicator in observer_indicators)

    def _has_pub_sub_pattern(self, content: str) -> bool:
        """检测发布订阅模式"""
        pub_sub_indicators = [
            'EventEmitter', 'EventBus', 'dispatch(',
            'on(', 'off(', 'emit('
        ]
        return any(indicator in content for indicator in pub_sub_indicators)

    def _has_micro_frontends_pattern(self) -> bool:
        """检测微前端模式"""
        # 检查是否有多个package.json或微前端配置
        package_files = list(self.project_path.rglob('package.json'))
        return len(package_files) > 1

    def _has_server_components_pattern(self, content: str) -> bool:
        """检测服务端组件模式"""
        server_indicators = [
            'use server', 'Server Component',
            'getServerSideProps', 'getStaticProps'
        ]
        return any(indicator in content for indicator in server_indicators)

    def _has_context_api_pattern(self, content: str) -> bool:
        """检测Context API模式"""
        context_indicators = [
            'createContext', 'useContext', 'Context.Provider',
            'Context.Consumer'
        ]
        return any(indicator in content for indicator in context_indicators)

    def _has_redux_pattern(self, content: str) -> bool:
        """检测Redux模式"""
        redux_indicators = [
            'createStore', 'useSelector', 'useDispatch',
            'configureStore', 'createSlice', 'Toolkit'
        ]
        return any(indicator in content for indicator in redux_indicators)

    def _has_provider_pattern(self, content: str) -> bool:
        """检测Provider模式"""
        provider_indicators = [
            '.Provider', 'useProvider', 'Provider value',
            'Context Provider'
        ]
        return any(indicator in content for indicator in provider_indicators)

    def _has_factory_pattern(self, content: str) -> bool:
        """检测工厂模式"""
        factory_indicators = [
            'createFactory', 'Factory', 'build(',
            'create('
        ]
        return any(indicator in content for indicator in factory_indicators)

    def _has_strategy_pattern(self, content: str) -> bool:
        """检测策略模式"""
        strategy_indicators = [
            'Strategy', 'execute(', 'setStrategy',
            'switch strategy'
        ]
        return any(indicator in content for indicator in strategy_indicators)

    # 性能问题检测方法
    def _has_unnecessary_rerenders(self, content: str) -> bool:
        """检测不必要的重渲染"""
        # 检查在渲染函数中创建对象或函数
        render_patterns = [
            'return {\s*style: {\s*',
            'onClick={()',
            'const style = {\s*',
            'return <div style={'
        ]
        return any(re.search(pattern, content) for pattern in render_patterns)

    def _has_missing_react_keys(self, content: str) -> bool:
        """检测缺失React key"""
        # 检查map渲染但没有key
        return re.search(r'\.map\(.*=>\s*<[^>]*(?!key\s*=)', content) is not None

    def _has_inline_styles(self, content: str) -> bool:
        """检测内联样式"""
        inline_style_patterns = [
            r'style=\{\{[^}]+\}\}',
            r'style="[^"]+"',
            r'style=\'[^\']+\''
        ]
        return any(re.search(pattern, content) for pattern in inline_style_patterns)

    def _has_memory_leak_risks(self, content: str) -> bool:
        """检测内存泄漏风险"""
        leak_patterns = [
            r'addEventListener\s*\([^)]*\)\s*[^}]*}',
            r'setInterval\s*\([^)]*\)',
            r'useEffect\s*\([^,]*\)\s*[^}]*return\s*[^;]*'
        ]
        return any(re.search(pattern, content) for pattern in leak_patterns)

    def _has_inefficient_state_updates(self, content: str) -> bool:
        """检测低效状态更新"""
        inefficient_patterns = [
            r'setState\(.*prevState.*=>',
            r'useState\s*\([^)]*\).*useState',
            r'setState\s*\([^,]*,\s*[^)]*\)'
        ]
        return any(re.search(pattern, content) for pattern in inefficient_patterns)

    def _has_prop_drilling(self, content: str) -> bool:
        """检测属性钻取"""
        # 简化检测：超过3层prop传递
        prop_depth = len(re.findall(r'props\.\w+\.\w+\.\w+', content))
        return prop_depth > 0

    def _has_inefficient_effects(self, content: str) -> bool:
        """检测低效useEffect"""
        inefficient_effect_patterns = [
            r'useEffect\s*\(\s*\(\s*\)\s*=>',
            r'useEffect\s*\([^,]*,\s*\[\s*\])'
        ]
        return any(re.search(pattern, content) for pattern in inefficient_effect_patterns)

    def _has_large_bundles(self) -> bool:
        """检测大打包文件"""
        build_dirs = ['dist', 'build']
        for build_dir in build_dirs:
            build_path = self.project_path / build_dir
            if build_path.exists():
                total_size = sum(f.stat().st_size for f in build_path.rglob('*') if f.is_file())
                # 如果总大小超过5MB，认为有bundle大小问题
                if total_size > 5 * 1024 * 1024:
                    return True
        return False

    def _has_missing_lazy_loading(self) -> bool:
        """检测缺失懒加载"""
        # 检查是否有路由懒加载配置
        router_files = list(self.project_path.rglob('*router*'))
        for router_file in router_files:
            try:
                content = router_file.read_text(encoding='utf-8')
                if 'import(' not in content and 'lazy' not in content:
                    return True
            except:
                continue
        return False

    def _has_missing_code_splitting(self) -> bool:
        """检测缺失代码分割"""
        # 检查webpack配置是否启用代码分割
        webpack_configs = ['webpack.config.js', 'webpack.config.ts', 'vite.config.js', 'vite.config.ts']
        for config_file in webpack_configs:
            config_path = self.project_path / config_file
            if config_path.exists():
                try:
                    content = config_path.read_text(encoding='utf-8')
                    if 'splitChunks' not in content and 'manualChunks' not in content:
                        return True
                except:
                    continue
        return False

    def _has_unoptimized_images(self) -> bool:
        """检测未优化的图片"""
        image_extensions = ['.jpg', '.jpeg', '.png', '.bmp', '.gif']
        asset_dirs = ['assets', 'images', 'img', 'public']

        for asset_dir in asset_dirs:
            dir_path = self.project_path / asset_dir
            if dir_path.exists():
                for img_file in dir_path.rglob('*'):
                    if img_file.is_file() and img_file.suffix.lower() in image_extensions:
                        size = img_file.stat().st_size
                        # 如果图片超过1MB且非webp格式，认为未优化
                        if size > 1024 * 1024 and img_file.suffix.lower() != '.webp':
                            return True
        return False

    def _has_missing_caching_strategy(self) -> bool:
        """检测缺失缓存策略"""
        caching_indicators = ['service-worker', 'Cache-Control', 'sw.js', 'workbox']
        for indicator in caching_indicators:
            if any(indicator in str(f) for f in self.project_path.rglob('*')):
                return False
        return True

    # 最佳实践检测方法
    def _has_component_tests(self) -> bool:
        """检测组件测试"""
        test_patterns = ['*.test.js', '*.test.ts', '*.spec.js', '*.spec.ts']
        for pattern in test_patterns:
            if list(self.project_path.rglob(pattern)):
                return True
        return False

    def _has_storybook(self) -> bool:
        """检测Storybook"""
        storybook_indicators = [
            '.storybook', 'stories.js', 'stories.ts',
            '@storybook/react', '@storybook/vue'
        ]
        return any(
            (self.project_path / indicator).exists() or
            any(indicator in str(f) for f in self.project_path.rglob('package.json'))
            for indicator in storybook_indicators
        )

    def _has_typescript(self) -> bool:
        """检测TypeScript使用"""
        ts_files = list(self.project_path.rglob('*.ts')) + list(self.project_path.rglob('*.tsx'))
        return len(ts_files) > 0

    def _has_eslint(self) -> bool:
        """检测ESLint配置"""
        eslint_files = ['.eslintrc.js', '.eslintrc.json', '.eslintrc.yml', 'eslint.config.js']
        return any((self.project_path / f).exists() for f in eslint_files)

    def _has_prettier(self) -> bool:
        """检测Prettier配置"""
        prettier_files = ['.prettierrc', '.prettierrc.json', '.prettierrc.yml', 'prettier.config.js']
        return any((self.project_path / f).exists() for f in prettier_files)

    def _has_husky(self) -> bool:
        """检测Git hooks (Husky)"""
        return (self.project_path / '.husky').exists() or (self.project_path / 'husky.config.js').exists()

    def _has_ci_cd(self) -> bool:
        """检测CI/CD配置"""
        ci_files = ['.github/workflows', '.gitlab-ci.yml', 'Jenkinsfile', 'azure-pipelines.yml']
        return any((self.project_path / f).exists() for f in ci_files)

    def _has_performance_monitoring(self) -> bool:
        """检测性能监控"""
        monitoring_libs = [
            'web-vitals', 'sentry', 'logrocket', 'datadog',
            'new-relic', 'bugsnag', 'rollbar'
        ]
        package_file = self.project_path / 'package.json'
        if package_file.exists():
            try:
                content = package_file.read_text(encoding='utf-8')
                return any(lib in content for lib in monitoring_libs)
            except:
                pass
        return False

    def _has_error_boundaries(self) -> bool:
        """检测错误边界"""
        error_boundary_indicators = ['ErrorBoundary', 'componentDidCatch', 'getDerivedStateFromError']
        source_files = self._get_source_files()

        for file_path in source_files[:20]:  # 检查前20个文件
            try:
                content = file_path.read_text(encoding='utf-8')
                if any(indicator in content for indicator in error_boundary_indicators):
                    return True
            except:
                continue
        return False

    def _has_accessibility_features(self) -> bool:
        """检测可访问性功能"""
        a11y_indicators = ['@testing-library/jest-dom', 'axe-core', 'eslint-plugin-jsx-a11y']
        package_file = self.project_path / 'package.json'
        if package_file.exists():
            try:
                content = package_file.read_text(encoding='utf-8')
                return any(lib in content for lib in a11y_indicators)
            except:
                pass
        return False

    def _has_seo_optimization(self) -> bool:
        """检测SEO优化"""
        seo_indicators = ['next-seo', 'react-helmet', 'vue-meta']
        package_file = self.project_path / 'package.json'
        if package_file.exists():
            try:
                content = package_file.read_text(encoding='utf-8')
                return any(lib in content for lib in seo_indicators)
            except:
                pass
        return False

    def _has_pwa_features(self) -> bool:
        """检测PWA功能"""
        pwa_files = ['manifest.json', 'service-worker.js', 'sw.js', 'workbox-']
        return any(
            (self.project_path / f).exists() or
            any(pattern in str(f) for f in self.project_path.rglob('*'))
            for pattern in pwa_files
        )

    def _has_responsive_design(self) -> bool:
        """检测响应式设计"""
        responsive_indicators = ['@media', 'breakpoints:', 'grid-template', 'flexbox', 'mobile:', 'tablet:']
        style_files = list(self.project_path.rglob('*.css')) + list(self.project_path.rglob('*.scss'))

        for style_file in style_files[:10]:  # 检查前10个样式文件
            try:
                content = style_file.read_text(encoding='utf-8')
                if any(indicator in content for indicator in responsive_indicators):
                    return True
            except:
                continue

        # 检查JS/CSS文件中的响应式设计
        source_files = self._get_source_files()
        for file_path in source_files[:10]:  # 检查前10个源文件
            try:
                content = file_path.read_text(encoding='utf-8')
                if any(indicator in content for indicator in responsive_indicators):
                    return True
            except:
                continue

        return False

    def _has_tree_shaking(self) -> bool:
        """检测Tree Shaking"""
        # 检查是否有ES6模块导入和构建工具配置
        source_files = self._get_source_files()
        has_es6_imports = False

        for file_path in source_files[:20]:  # 检查前20个文件
            try:
                content = file_path.read_text(encoding='utf-8')
                if 'import {' in content or 'export {' in content:
                    has_es6_imports = True
                    break
            except:
                continue

        build_configs = ['webpack.config.js', 'vite.config.js', 'rollup.config.js']
        has_optimized_config = any((self.project_path / config).exists() for config in build_configs)

        return has_es6_imports and has_optimized_config

    def _has_bundle_analysis(self) -> bool:
        """检测打包分析"""
        analysis_files = ['bundle-analyzer-report.html', 'stats.json', 'webpack-bundle-analyzer.json']
        build_dirs = ['dist', 'build', '.next', '.nuxt']

        for build_dir in build_dirs:
            build_path = self.project_path / build_dir
            if build_path.exists():
                for analysis_file in analysis_files:
                    if (build_path / analysis_file).exists():
                        return True
        return False

    # 可访问性检测方法
    def _has_aria_labels(self, content: str) -> bool:
        """检测ARIA标签"""
        aria_patterns = [r'aria-\w+', r'role="\w+"', r'aria-labelledby', r'aria-describedby']
        return any(re.search(pattern, content, re.IGNORECASE) for pattern in aria_patterns)

    def _has_alt_attributes(self, content: str) -> bool:
        """检测alt属性"""
        return 'alt=' in content or 'alt="' in content

    def _has_semantic_html(self, content: str) -> bool:
        """检测语义化HTML"""
        semantic_tags = ['<nav', '<main', '<article', '<section', '<aside', '<header>', '<footer>']
        return any(tag in content for tag in semantic_tags)

    def _has_keyboard_navigation(self, content: str) -> bool:
        """检测键盘导航"""
        keyboard_indicators = ['onKeyDown', 'onKeyPress', 'tabIndex', 'tabIndex']
        return any(indicator in content for indicator in keyboard_indicators)

    def _has_screen_reader_support(self, content: str) -> bool:
        """检测屏幕阅读器支持"""
        screen_reader_patterns = ['sr-only', 'screen-reader', 'aria-live', 'role="alert"']
        return any(pattern in content for pattern in screen_reader_patterns)

    # 组件分析方法
    def _extract_component_name(self, content: str, file_name: str) -> str:
        """提取组件名称"""
        # React组件
        function_match = re.search(r'function\s+([A-Z]\w+)', content)
        if function_match:
            return function_match.group(1)

        const_match = re.search(r'const\s+([A-Z]\w+)\s*=', content)
        if const_match:
            return const_match.group(1)

        class_match = re.search(r'class\s+([A-Z]\w+)', content)
        if class_match:
            return class_match.group(1)

        export_match = re.search(r'export\s+(?:default\s+)?([A-Z]\w+)', content)
        if export_match:
            return export_match.group(1)

        # Vue组件
        vue_match = re.search(r'name:\s*[\'"]([^\'"]+)[\'"]', content)
        if vue_match:
            return vue_match.group(1)

        # 使用文件名作为备选
        return Path(file_name).stem

    def _detect_component_type(self, content: str) -> str:
        """检测组件类型"""
        if 'class ' in content and 'extends Component' in content:
            return "Class Component"
        elif 'useState' in content or 'useRef' in content or 'useEffect' in content:
            return "Functional Component"
        elif 'setup(' in content:
            return "Vue Composition API"
        elif 'export default' in content and 'data():' in content:
            return "Vue Options API"
        elif 'connect(' in content or 'useSelector' in content:
            return "Connected Component"
        elif 'ErrorBoundary' in content or 'componentDidCatch' in content:
            return "Error Boundary"
        else:
            return "Component"

    def _extract_component_props(self, content: str) -> List[str]:
        """提取组件props"""
        props = []

        # React props
        destructuring_match = re.search(r'\(\s*\{\s*([^}]+)\s*\}\s*\)', content)
        if destructuring_match:
            props.extend([prop.strip() for prop in destructuring_match.group(1).split(',')])

        # Vue props
        vue_props_match = re.findall(r'props:\s*\{\s*([^}]+)\s*\}', content)
        for match in vue_props_match:
            props.extend([prop.strip() for prop in match.split(',') if prop.strip()])

        # TypeScript interface
        interface_match = re.search(r'interface\s+\w+Props\s*\{([^}]+)\}', content)
        if interface_match:
            ts_props = re.findall(r'(\w+)\s*:', interface_match.group(1))
            props.extend(ts_props)

        return list(set(props))

    def _extract_component_hooks(self, content: str) -> List[str]:
        """提取组件Hooks"""
        hooks = []

        # React hooks
        hook_matches = re.findall(r'use[A-Z]\w*', content)
        hooks.extend(hook_matches)

        # Vue composables
        composable_matches = re.findall(r'use[A-Z]\w*\(', content)
        hooks.extend(composable_matches)

        return list(set(hooks))

    def _detect_component_state_management(self, content: str) -> bool:
        """检测组件状态管理"""
        state_indicators = ['useState', 'useReducer', 'useContext', 'data()', 'reactive(']
        return any(indicator in content for indicator in state_indicators)

    def _extract_component_dependencies(self, content: str) -> List[str]:
        """提取组件依赖"""
        dependencies = []

        # React hooks依赖
        effect_deps_match = re.search(r'useEffect\([^,]*,\s*\[([^]]+)\]\)', content)
        if effect_deps_match:
            dependencies.extend([dep.strip() for dep in effect_deps_match.group(1).split(',')])

        # Vue watch依赖
        watch_deps_match = re.search(r'watch\([^,]*,\s*\[([^]]+)\]\)', content)
        if watch_deps_match:
            dependencies.extend([dep.strip() for dep in watch_deps_match.group(1).split(',')])

        # Computed依赖
        computed_deps_match = re.search(r'computed\([^,]*,\s*\[([^]]+)\]\)', content)
        if computed_deps_match:
            dependencies.extend([dep.strip() for dep in computed_deps_match.group(1).split(',')])

        return list(set(dependencies))

    def _calculate_component_performance_score(self, content: str) -> float:
        """计算组件性能评分"""
        score = 100.0

        # 重渲染风险
        if self._has_unnecessary_rerenders(content):
            score -= 15

        # 内存泄漏风险
        if self._has_memory_leak_risks(content):
            score -= 20

        # 低效状态更新
        if self._has_inefficient_state_updates(content):
            score -= 10

        # 低效effects
        if self._has_inefficient_effects(content):
            score -= 10

        # 内联样式
        if self._has_inline_styles(content):
            score -= 5

        return max(0.0, score)

    def _calculate_bundle_sizes(self, build_path: Path) -> Dict[str, int]:
        """计算打包文件大小"""
        sizes = {}

        for file_path in build_path.rglob('*'):
            if file_path.is_file():
                sizes[str(file_path.relative_to(build_path))] = file_path.stat().st_size

        return sizes

def generate_frontend_report(analysis: FrontendArchitectureAnalysis) -> str:
    """生成前端项目架构分析报告"""
    report = "# 前端项目架构分析报告\n\n"

    # 项目概览
    report += "## 📊 项目概览\n"
    report += f"- **框架**: {analysis.project_info.framework.value}\n"
    report += f"- **版本**: {analysis.project_info.version}\n"
    report += f"- **构建工具**: {analysis.project_info.build_tool}\n"
    report += f"- **包管理器**: {analysis.project_info.package_manager}\n"
    report += f"- **CSS框架**: {analysis.project_info.css_framework}\n"
    report += f"- **质量评分**: {analysis.quality_score:.1f}/100\n"
    report += f"- **性能评分**: {analysis.performance_score:.1f}/100\n"
    report += f"- **可访问性评分**: {analysis.accessibility_score:.1f}/100\n"
    report += f"- **架构模式**: {len(analysis.patterns)} 个\n"
    report += f"- **性能问题**: {len(analysis.performance_issues)} 个\n"
    report += f"- **最佳实践**: {len(analysis.best_practices)} 个\n\n"

    # 技术栈详情
    report += "## 🛠️ 技术栈详情\n"
    report += f"- **状态管理**: {analysis.project_info.state_management}\n"
    report += f"- **路由方案**: {analysis.project_info.routing}\n"
    report += f"- **UI库**: {analysis.project_info.ui_library}\n"
    report += f"- **测试框架**: {analysis.project_info.testing_framework}\n"
    report += f"- **开发服务器**: {analysis.project_info.dev_server}\n\n"

    # 架构模式分析
    report += "## 🏗️ 架构模式识别\n"
    if analysis.patterns:
        for pattern in analysis.patterns:
            report += f"- ✅ **{pattern.value}**: 检测到该架构模式\n"
    else:
        report += "- 💡 建议应用设计模式提高代码质量\n"
    report += "\n"

    # 性能问题分析
    report += "## ⚡ 性能问题分析\n"
    if analysis.performance_issues:
        for issue in analysis.performance_issues:
            report += f"- 🚨 **{issue.value}**: 需要优化\n"
    else:
        report += "- ✅ 未检测到明显的性能问题\n"
    report += "\n"

    # 最佳实践评估
    report += "## 🎯 最佳实践评估\n"
    if analysis.best_practices:
        for practice in analysis.best_practices:
            report += f"- ✨ **{practice.value}**: 良好的实践\n"
    else:
        report += "- 💡 建议改进开发实践\n"
    report += "\n"

    # 组件分析
    report += "## 🔧 组件分析\n"
    if analysis.components:
        # 按性能评分排序
        sorted_components = sorted(analysis.components, key=lambda x: x.performance_score)
        report += f"共分析了 {len(analysis.components)} 个组件\n\n"

        report += "### 性能较低组件 (需要关注):\n"
        for comp in sorted_components[:5]:  # 显示前5个性能较低的
            report += f"- **{comp.name}** ({comp.type}): {comp.performance_score:.1f}分, {comp.lines_of_code}行\n"
            report += f"  📁 `{comp.file_path}`\n"
            if comp.hooks:
                report += f"  🎣 Hooks: {', '.join(comp.hooks[:5])}\n"
            if comp.props:
                report += f"  🔌 Props: {', '.join(comp.props[:5])}\n"

        report += "\n### 高性能组件:\n"
        for comp in sorted_components[-3:]:  # 显示最后3个性能较高的
            report += f"- **{comp.name}** ({comp.type}): {comp.performance_score:.1f}分\n"
    report += "\n"

    # 打包分析
    report += "## 📦 打包分析\n"
    if analysis.bundle_analysis.get("bundle_sizes"):
        for build_dir, sizes in analysis.bundle_analysis["bundle_sizes"].items():
            total_size = sum(sizes.values()) / (1024 * 1024)  # MB
            report += f"### {build_dir}\n"
            report += f"- **总大小**: {total_size:.2f} MB\n"

            # 显示最大的文件
            if sizes:
                sorted_files = sorted(sizes.items(), key=lambda x: x[1], reverse=True)
                for file_path, size in sorted_files[:5]:  # 显示前5个最大的文件
                    size_mb = size / (1024 * 1024)
                    report += f"- **{file_path}**: {size_mb:.2f} MB\n"

        if not analysis.bundle_analysis.get("has_bundle_analysis"):
            report += "- ⚠️ 建议添加打包分析工具\n"
    else:
        report += "- ⚠️ 未找到打包文件\n"
    report += "\n"

    # 项目结构
    report += "## 📁 项目结构分析\n"
    for category, files in analysis.project_structure.items():
        if files:
            report += f"- **{category.title()}**: {len(files)} 个文件\n"
    report += "\n"

    # 测试覆盖率
    report += "## 🧪 测试覆盖率\n"
    if analysis.test_coverage:
        if "overall" in analysis.test_coverage:
            report += f"- **整体覆盖率**: {analysis.test_coverage['overall']:.1f}%\n"
        if "test_to_source_ratio" in analysis.test_coverage:
            report += f"- **测试/源码比例**: {analysis.test_coverage['test_to_source_ratio']:.1f}%\n"
    else:
        report += "- ⚠️ 无法获取测试覆盖率信息\n"
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

    report += f"\n### 评分详情\n"
    report += f"- **架构设计**: {len(analysis.patterns) * 4:.0f} 分\n"
    report += f"- **最佳实践**: {len(analysis.best_practices) * 3:.0f} 分\n"
    report += f"- **性能表现**: {analysis.performance_score * 0.3:.0f} 分\n"
    report += f"- **可访问性**: {analysis.accessibility_score * 0.2:.0f} 分\n"
    report += f"- **测试覆盖**: {analysis.test_coverage.get('overall', 0) * 0.2:.0f} 分\n"

    return report

if __name__ == "__main__":
    import sys
    if len(sys.argv) != 2:
        print("用法: python frontend_analyzer.py <前端项目路径>")
        sys.exit(1)

    project_path = Path(sys.argv[1])
    analyzer = FrontendArchitectureAnalyzer(project_path)
    analysis = analyzer.analyze()
    report = generate_frontend_report(analysis)

    print(report)

    # 保存报告
    output_file = project_path / "frontend_architecture_analysis.md"
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(report)

    print(f"\n📄 报告已保存到: {output_file}")