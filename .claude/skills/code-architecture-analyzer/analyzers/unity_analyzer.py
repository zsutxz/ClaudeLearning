#!/usr/bin/env python3
"""
Unity项目专用架构分析器
专门分析Unity游戏的架构模式、性能问题、最佳实践
"""

import re
import json
import yaml
from pathlib import Path
from typing import Dict, List, Tuple, Optional
from dataclasses import dataclass
from enum import Enum

class UnityArchitecturePattern(Enum):
    MVC = "Model-View-Controller"
    SINGLETON = "Singleton Pattern"
    OBJECT_POOL = "Object Pool Pattern"
    OBSERVER = "Observer Pattern"
    STATE_MACHINE = "State Machine"
    FACTORY = "Factory Pattern"
    STRATEGY = "Strategy Pattern"
    COMMAND = "Command Pattern"
    PUBLISHER_SUBSCRIBER = "Publisher-Subscriber"
    DATA_LOCALITY = "Data Locality Pattern"
    ECS = "Entity Component System"

class UnityPerformanceIssue(Enum):
    GC_ALLOC = "GC Allocation Issues"
    EXPENSIVE_METHODS = "Expensive Method Calls"
    INEFFICIENT_UPDATE = "Inefficient Update Usage"
    MISSING_OBJECT_POOLING = "Missing Object Pooling"
    INEFFICIENT_COLLISION = "Inefficient Collision Detection"
    POOR_PHYSICS_SETUP = "Poor Physics Configuration"
    INEFFICIENT_UI = "Inefficient UI Implementation"
    RESOURCE_LOADING = "Inefficient Resource Loading"
    MEMORY_LEAKS = "Memory Leaks"

class UnityBestPractice(Enum):
    SCRIPTABLEOBJECTS = "ScriptableObject Usage"
    OBJECT_POOLING = "Object Pooling Implementation"
    EVENT_SYSTEM = "Event-Driven Architecture"
    ASSET_BUNDLES = "Asset Bundle Management"
    ADDRESSABLES = "Addressables System"
    COROUTINES = "Coroutine Optimization"
    LAYERING = "Proper Layering"
    SEPARATION_OF_CONCERNS = "Separation of Concerns"

@dataclass
class UnityComponentInfo:
    name: str
    type: str
    file_path: str
    lines_of_code: int
    dependencies: List[str]
    performance_score: float

@dataclass
class UnityArchitectureAnalysis:
    project_info: Dict[str, str]
    patterns: List[UnityArchitecturePattern]
    performance_issues: List[UnityPerformanceIssue]
    best_practices: List[UnityBestPractice]
    components: List[UnityComponentInfo]
    scene_structure: Dict[str, List[str]]
    asset_organisation: Dict[str, List[str]]
    build_configuration: Dict[str, str]
    recommendations: List[str]
    quality_score: float

class UnityArchitectureAnalyzer:
    def __init__(self, project_path: Path):
        self.project_path = project_path
        self.assets_path = project_path / "Assets"
        self.project_settings_path = project_path / "ProjectSettings"
        self.packages_path = project_path / "Packages"

    def analyze(self) -> UnityArchitectureAnalysis:
        """执行Unity项目的全面架构分析"""

        # 获取项目基本信息
        project_info = self._get_project_info()

        # 分析架构模式
        patterns = self._detect_patterns()

        # 检测性能问题
        performance_issues = self._detect_performance_issues()

        # 评估最佳实践
        best_practices = self._evaluate_best_practices()

        # 分析组件
        components = self._analyze_components()

        # 分析场景结构
        scene_structure = self._analyze_scenes()

        # 分析资源组织
        asset_organisation = self._analyze_assets()

        # 分析构建设置
        build_configuration = self._analyze_build_settings()

        # 生成建议
        recommendations = self._generate_recommendations(patterns, performance_issues, best_practices)

        # 计算质量评分
        quality_score = self._calculate_quality_score(patterns, performance_issues, best_practices)

        return UnityArchitectureAnalysis(
            project_info=project_info,
            patterns=patterns,
            performance_issues=performance_issues,
            best_practices=best_practices,
            components=components,
            scene_structure=scene_structure,
            asset_organisation=asset_organisation,
            build_configuration=build_configuration,
            recommendations=recommendations,
            quality_score=quality_score
        )

    def _get_project_info(self) -> Dict[str, str]:
        """获取Unity项目基本信息"""
        info = {
            "unity_version": "Unknown",
            "target_platform": "Unknown",
            "scripting_backend": "Unknown",
            "api_compatibility": "Unknown"
        }

        # 分析ProjectSettings/ProjectVersion.txt
        version_file = self.project_settings_path / "ProjectVersion.txt"
        if version_file.exists():
            try:
                content = version_file.read_text()
                if "m_EditorVersion:" in content:
                    version = re.search(r"m_EditorVersion:\s*(.+)", content)
                    if version:
                        info["unity_version"] = version.group(1).strip()
            except:
                pass

        # 分析BuildSettings
        build_settings_file = self.project_settings_path / "BuildSettings.asset"
        if build_settings_file.exists():
            try:
                content = build_settings_file.read_text()
                # 简化版本的平台检测
                if "StandaloneWindows64" in content:
                    info["target_platform"] = "Windows"
                elif "Android" in content:
                    info["target_platform"] = "Android"
                elif "iOS" in content:
                    info["target_platform"] = "iOS"
            except:
                pass

        return info

    def _detect_patterns(self) -> List[UnityArchitecturePattern]:
        """检测Unity架构模式"""
        patterns = []

        if not self.assets_path.exists():
            return patterns

        # 获取所有C#文件
        cs_files = list(self.assets_path.rglob("*.cs"))

        # 合并所有代码内容进行分析
        all_code = ""
        for cs_file in cs_files:
            try:
                all_code += cs_file.read_text(encoding='utf-8') + "\n"
            except:
                continue

        # MVC模式检测
        if self._detect_mvc_pattern(cs_files):
            patterns.append(UnityArchitecturePattern.MVC)

        # Singleton模式检测
        if self._detect_singleton_pattern(all_code):
            patterns.append(UnityArchitecturePattern.SINGLETON)

        # Object Pool模式检测
        if self._detect_object_pool_pattern(all_code):
            patterns.append(UnityArchitecturePattern.OBJECT_POOL)

        # Observer模式检测
        if self._detect_observer_pattern(all_code):
            patterns.append(UnityArchitecturePattern.OBSERVER)

        # State Machine模式检测
        if self._detect_state_machine_pattern(all_code):
            patterns.append(UnityArchitecturePattern.STATE_MACHINE)

        # Factory模式检测
        if self._detect_factory_pattern(all_code):
            patterns.append(UnityArchitecturePattern.FACTORY)

        # Strategy模式检测
        if self._detect_strategy_pattern(all_code):
            patterns.append(UnityArchitecturePattern.STRATEGY)

        # Command模式检测
        if self._detect_command_pattern(all_code):
            patterns.append(UnityArchitecturePattern.COMMAND)

        # Publisher-Subscriber模式检测
        if self._detect_pubsub_pattern(all_code):
            patterns.append(UnityArchitecturePattern.PUBLISHER_SUBSCRIBER)

        # Data Locality/DOTS模式检测
        if self._detect_dots_pattern(all_code):
            patterns.append(UnityArchitecturePattern.DATA_LOCALITY)

        # ECS模式检测
        if self._detect_ecs_pattern(all_code):
            patterns.append(UnityArchitecturePattern.ECS)

        return patterns

    def _detect_performance_issues(self) -> List[UnityPerformanceIssue]:
        """检测性能问题"""
        issues = []

        if not self.assets_path.exists():
            return issues

        cs_files = list(self.assets_path.rglob("*.cs"))

        for cs_file in cs_files:
            try:
                content = cs_file.read_text(encoding='utf-8')

                # GC分配问题
                if self._has_gc_allocation_issues(content):
                    issues.append(UnityPerformanceIssue.GC_ALLOC)

                # 昂贵的方法调用
                if self._has_expensive_method_calls(content):
                    issues.append(UnityPerformanceIssue.EXPENSIVE_METHODS)

                # 低效的Update使用
                if self._has_inefficient_update(content):
                    issues.append(UnityPerformanceIssue.INEFFICIENT_UPDATE)

                # 缺少对象池
                if self._missing_object_pooling(content, cs_file):
                    issues.append(UnityPerformanceIssue.MISSING_OBJECT_POOLING)

                # 低效的碰撞检测
                if self._has_inefficient_collision(content):
                    issues.append(UnityPerformanceIssue.INEFFICIENT_COLLISION)

                # 内存泄漏风险
                if self._has_memory_leak_risks(content):
                    issues.append(UnityPerformanceIssue.MEMORY_LEAKS)

            except:
                continue

        # 去重
        return list(set(issues))

    def _evaluate_best_practices(self) -> List[UnityBestPractice]:
        """评估最佳实践"""
        practices = []

        if not self.assets_path.exists():
            return practices

        # 获取所有C#文件
        cs_files = list(self.assets_path.rglob("*.cs"))

        # 合并所有代码内容
        all_code = ""
        for cs_file in cs_files:
            try:
                all_code += cs_file.read_text(encoding='utf-8') + "\n"
            except:
                continue

        # ScriptableObject使用
        if self._uses_scriptable_objects(all_code):
            practices.append(UnityBestPractice.SCRIPTABLEOBJECTS)

        # 对象池实现
        if self._implements_object_pooling(all_code):
            practices.append(UnityBestPractice.OBJECT_POOLING)

        # 事件系统
        if self._uses_event_system(all_code):
            practices.append(UnityBestPractice.EVENT_SYSTEM)

        # Addressables系统
        if self._uses_addressables():
            practices.append(UnityBestPractice.ADDRESSABLES)

        # 协程优化
        if self._optimizes_coroutines(all_code):
            practices.append(UnityBestPractice.COROUTINES)

        # 合理分层
        if self._has_proper_layering(cs_files):
            practices.append(UnityBestPractice.LAYERING)

        # 关注点分离
        if self._has_separation_of_concerns(cs_files):
            practices.append(UnityBestPractice.SEPARATION_OF_CONCERNS)

        return practices

    def _analyze_components(self) -> List[UnityComponentInfo]:
        """分析Unity组件"""
        components = []

        if not self.assets_path.exists():
            return components

        cs_files = list(self.assets_path.rglob("*.cs"))

        for cs_file in cs_files:
            try:
                content = cs_file.read_text(encoding='utf-8')
                rel_path = cs_file.relative_to(self.assets_path)

                # 计算代码行数
                lines_of_code = len([line for line in content.split('\n') if line.strip()])

                # 检测组件类型
                component_type = self._detect_component_type(content)

                # 分析依赖
                dependencies = self._extract_dependencies(content)

                # 计算性能评分
                performance_score = self._calculate_performance_score(content)

                # 获取类名
                class_name = self._extract_class_name(content, cs_file.name)

                components.append(UnityComponentInfo(
                    name=class_name,
                    type=component_type,
                    file_path=str(rel_path),
                    lines_of_code=lines_of_code,
                    dependencies=dependencies,
                    performance_score=performance_score
                ))

            except:
                continue

        return components

    def _analyze_scenes(self) -> Dict[str, List[str]]:
        """分析场景结构"""
        structure = {
            "scenes": [],
            "prefabs": [],
            "scene_objects": []
        }

        if not self.assets_path.exists():
            return structure

        # 查找场景文件
        for scene_file in self.assets_path.rglob("*.unity"):
            structure["scenes"].append(str(scene_file.relative_to(self.assets_path)))

        # 查找预制体
        for prefab_file in self.assets_path.rglob("*.prefab"):
            structure["prefabs"].append(str(prefab_file.relative_to(self.assets_path)))

        return structure

    def _analyze_assets(self) -> Dict[str, List[str]]:
        """分析资源组织"""
        organisation = {
            "textures": [],
            "materials": [],
            "audio": [],
            "animations": [],
            "scripts": [],
            "prefabs": [],
            "scenes": []
        }

        if not self.assets_path.exists():
            return organisation

        for asset_path in self.assets_path.rglob("*"):
            if asset_path.is_file():
                ext = asset_path.suffix.lower()
                rel_path = str(asset_path.relative_to(self.assets_path))

                if ext in [".png", ".jpg", ".jpeg", ".tga", ".psd", ".tiff"]:
                    organisation["textures"].append(rel_path)
                elif ext == ".mat":
                    organisation["materials"].append(rel_path)
                elif ext in [".wav", ".mp3", ".ogg", ".aiff"]:
                    organisation["audio"].append(rel_path)
                elif ext in [".anim", ".controller"]:
                    organisation["animations"].append(rel_path)
                elif ext == ".cs":
                    organisation["scripts"].append(rel_path)
                elif ext == ".prefab":
                    organisation["prefabs"].append(rel_path)
                elif ext == ".unity":
                    organisation["scenes"].append(rel_path)

        return organisation

    def _analyze_build_settings(self) -> Dict[str, str]:
        """分析构建设置"""
        settings = {}

        if not self.project_settings_path.exists():
            return settings

        # 分析BuildSettings
        build_settings_file = self.project_settings_path / "BuildSettings.asset"
        if build_settings_file.exists():
            try:
                content = build_settings_file.read_text()
                settings["build_targets"] = self._extract_build_targets(content)
            except:
                settings["build_targets"] = "Unknown"

        return settings

    def _generate_recommendations(self, patterns: List[UnityArchitecturePattern],
                                 issues: List[UnityPerformanceIssue],
                                 practices: List[UnityBestPractice]) -> List[str]:
        """生成改进建议"""
        recommendations = []

        # 基于性能问题的建议
        if UnityPerformanceIssue.GC_ALLOC in issues:
            recommendations.append("🔄 **减少GC分配**: 使用对象池、StringBuilder、避免在Update中分配内存")

        if UnityPerformanceIssue.MISSING_OBJECT_POOLING in issues:
            recommendations.append("🏊 **实现对象池**: 为频繁创建销毁的对象（如子弹、特效）实现对象池")

        if UnityPerformanceIssue.INEFFICIENT_UPDATE in issues:
            recommendations.append("⚡ **优化Update**: 使用事件驱动、协程或状态机减少Update调用")

        if UnityPerformanceIssue.EXPENSIVE_METHODS in issues:
            recommendations.append("🔍 **缓存昂贵调用**: 缓存GetComponent、transform.position等昂贵方法的结果")

        # 基于架构模式的建议
        if UnityArchitecturePattern.SINGLETON not in patterns:
            recommendations.append("🏗️ **考虑单例模式**: 对全局管理器（GameManager、AudioManager）使用单例模式")

        if UnityArchitecturePattern.OBSERVER not in patterns:
            recommendations.append("📡 **使用事件系统**: 实现观察者模式减少组件间耦合")

        if UnityArchitecturePattern.STATE_MACHINE not in patterns:
            recommendations.append("🎮 **状态机模式**: 对复杂游戏逻辑（角色状态、UI状态）使用状态机")

        # 基于最佳实践的建议
        if UnityBestPractice.SCRIPTABLEOBJECTS not in practices:
            recommendations.append("📋 **使用ScriptableObject**: 用ScriptableObject管理配置数据、游戏参数")

        if UnityBestPractice.ADDRESSABLES not in practices:
            recommendations.append("📦 **Addressables系统**: 使用Addressables管理远程资源和热更新")

        # 通用建议
        recommendations.extend([
            "🧪 **添加测试**: 实现单元测试和集成测试确保代码质量",
            "📊 **性能分析**: 使用Unity Profiler定期分析性能瓶颈",
            "🎯 **分层架构**: 确保表现层、业务层、数据层的清晰分离",
            "🔧 **代码规范**: 统一命名规范和代码风格",
            "📝 **文档完善**: 为复杂组件添加注释和文档"
        ])

        return recommendations

    def _calculate_quality_score(self, patterns: List[UnityArchitecturePattern],
                               issues: List[UnityPerformanceIssue],
                               practices: List[UnityBestPractice]) -> float:
        """计算项目质量评分 (0-100)"""
        base_score = 50.0

        # 架构模式加分 (每个模式+5分)
        pattern_score = len(patterns) * 5

        # 最佳实践加分 (每个实践+3分)
        practice_score = len(practices) * 3

        # 性能问题扣分 (每个问题-8分)
        issue_penalty = len(issues) * 8

        final_score = base_score + pattern_score + practice_score - issue_penalty
        return max(0.0, min(100.0, final_score))

    # 模式检测方法
    def _detect_mvc_pattern(self, cs_files: List[Path]) -> bool:
        """检测MVC模式"""
        has_controller = any("controller" in str(f).lower() for f in cs_files)
        has_model = any("model" in str(f).lower() or "data" in str(f).lower() for f in cs_files)
        has_view = any("view" in str(f).lower() or "ui" in str(f).lower() for f in cs_files)
        return has_controller and has_model and has_view

    def _detect_singleton_pattern(self, code_content: str) -> bool:
        """检测单例模式"""
        patterns = [
            r"static.*\w+.*instance",
            r"private\s+.*\w+.*instance",
            r"public\s+static\s+\w+\s+Instance",
            r"void\s+Awake\s*\(\s*\).*\{.*instance"
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in patterns)

    def _detect_object_pool_pattern(self, code_content: str) -> bool:
        """检测对象池模式"""
        patterns = [
            r"Queue<.*>",
            r"Stack<.*>",
            r"List<.*>.*pool",
            r"GetFromPool|ReturnToPool",
            r"instantiate.*queue"
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in patterns)

    def _detect_observer_pattern(self, code_content: str) -> bool:
        """检测观察者模式"""
        patterns = [
            r"event\s+.*Action",
            r"UnityEvent",
            r"AddListener|RemoveListener",
            r"subscribe|unsubscribe",
            r"on[A-Z]\w*"
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in patterns)

    def _detect_state_machine_pattern(self, code_content: str) -> bool:
        """检测状态机模式"""
        patterns = [
            r"enum.*State",
            r"switch\s*\(\s*state",
            r"currentState",
            r"ChangeState",
            r"State\s*\{"
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in patterns)

    def _detect_factory_pattern(self, code_content: str) -> bool:
        """检测工厂模式"""
        patterns = [
            r"Factory",
            r"Create.*\(",
            r"Instantiate.*factory",
            r"Build.*\("
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in patterns)

    def _detect_strategy_pattern(self, code_content: str) -> bool:
        """检测策略模式"""
        patterns = [
            r"interface.*Strategy",
            r"abstract.*Strategy",
            r"SetStrategy|GetStrategy",
            r"Execute.*Strategy"
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in patterns)

    def _detect_command_pattern(self, code_content: str) -> bool:
        """检测命令模式"""
        patterns = [
            r"interface.*Command",
            r"Execute\s*\(\s*\)",
            r"Undo\s*\(\s*\)",
            r"ICommand"
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in patterns)

    def _detect_pubsub_pattern(self, code_content: str) -> bool:
        """检测发布订阅模式"""
        patterns = [
            r"Publish\s*\(",
            r"Subscribe\s*\(",
            r"Unsubscribe\s*\(",
            r"EventSystem",
            r"MessageBus"
        ]
        return any(re.search(pattern, code_content, re.IGNORECASE) for pattern in patterns)

    def _detect_dots_pattern(self, code_content: str) -> bool:
        """检测DOTS模式"""
        dots_keywords = ["IComponentData", "ISystem", "Entity", "EntityManager", "JobComponentSystem"]
        return any(keyword in code_content for keyword in dots_keywords)

    def _detect_ecs_pattern(self, code_content: str) -> bool:
        """检测ECS模式"""
        ecs_keywords = ["Component", "System", "Entity", "World", "IComponent"]
        return sum(keyword in code_content for keyword in ecs_keywords) >= 3

    # 性能问题检测方法
    def _has_gc_allocation_issues(self, content: str) -> bool:
        """检测GC分配问题"""
        patterns = [
            r"new\s+\w+\s*\[\s*\]",
            r"Instantiate\s*\(",
            r"ToString\s*\(\s*\)",
            r"string\s*\+",
            r"List<.*>\s*new"
        ]
        return any(re.search(pattern, content, re.IGNORECASE) for pattern in patterns)

    def _has_expensive_method_calls(self, content: str) -> bool:
        """检测昂贵的方法调用"""
        expensive_methods = [
            r"FindObjectOfType",
            r"FindObjectsOfType",
            r"GetComponent\s*\(",
            r"transform\.position",
            r"transform\.rotation",
            r"Physics\.Raycast"
        ]
        return any(re.search(pattern, content, re.IGNORECASE) for pattern in expensive_methods)

    def _has_inefficient_update(self, content: str) -> bool:
        """检测低效的Update使用"""
        # 检查是否有复杂的Update逻辑
        update_match = re.search(r"void\s+Update\s*\(\s*\)\s*\{([^}]+)\}", content, re.DOTALL)
        if update_match:
            update_body = update_match.group(1)
            # 如果Update中有复杂逻辑（多个循环、 Instantiate等）
            complexity = len(re.findall(r"(for|while|foreach|Instantiate|new\s+\w+)", update_body))
            return complexity > 2
        return False

    def _missing_object_pooling(self, content: str, cs_file: Path) -> bool:
        """检测缺少对象池的情况"""
        instantiate_count = len(re.findall(r"Instantiate\s*\(", content, re.IGNORECASE))
        has_pool = any(keyword in content.lower() for keyword in ["pool", "queue", "stack"])

        # 如果文件名包含常见需要对象池的类型且频繁实例化
        needs_pool_keywords = ["bullet", "projectile", "effect", "particle", "coin", "enemy"]
        file_needs_pool = any(keyword in str(cs_file).lower() for keyword in needs_pool_keywords)

        return file_needs_pool and instantiate_count > 1 and not has_pool

    def _has_inefficient_collision(self, content: str) -> bool:
        """检测低效的碰撞检测"""
        collision_patterns = [
            r"OnCollisionEnter.*\(.*Collision.*\)",
            r"OnTriggerEnter.*\(.*Collider.*\)",
            r"Physics\.OverlapSphere",
            r"Physics\.CheckSphere"
        ]
        return any(re.search(pattern, content, re.IGNORECASE) for pattern in collision_patterns)

    def _has_memory_leak_risks(self, content: str) -> bool:
        """检测内存泄漏风险"""
        leak_patterns = [
            r"static\s+.*List<",
            r"static\s+.*Dictionary<",
            r"\.Add\s*\(",
            r"\.Subscribe\s*\("
        ]

        # 检查是否有静态集合但没有清理机制
        has_static_collection = any(re.search(pattern, content, re.IGNORECASE) for pattern in leak_patterns[:2])
        has_add_operations = any(re.search(pattern, content, re.IGNORECASE) for pattern in leak_patterns[2:])
        has_clear_mechanism = any(keyword in content.lower() for keyword in ["clear", "remove", "unsubscribe"])

        return has_static_collection and has_add_operations and not has_clear_mechanism

    # 最佳实践评估方法
    def _uses_scriptable_objects(self, code_content: str) -> bool:
        """检测ScriptableObject使用"""
        return "ScriptableObject" in code_content

    def _implements_object_pooling(self, code_content: str) -> bool:
        """检测对象池实现"""
        return self._detect_object_pool_pattern(code_content)

    def _uses_event_system(self, code_content: str) -> bool:
        """检测事件系统使用"""
        return any(keyword in code_content for keyword in ["event", "Action", "UnityEvent", "AddListener"])

    def _uses_addressables(self) -> bool:
        """检测Addressables使用"""
        manifest_file = self.packages_path / "manifest.json"
        if manifest_file.exists():
            try:
                content = manifest_file.read_text()
                return "addressables" in content.lower()
            except:
                pass
        return False

    def _optimizes_coroutines(self, code_content: str) -> bool:
        """检测协程优化"""
        return "StartCoroutine" in code_content

    def _has_proper_layering(self, cs_files: List[Path]) -> bool:
        """检测合理分层"""
        layer_dirs = ["controllers", "models", "views", "managers", "services"]
        return any(any(layer_dir in str(f).lower() for f in cs_files) for layer_dir in layer_dirs)

    def _has_separation_of_concerns(self, cs_files: List[Path]) -> bool:
        """检测关注点分离"""
        # 简化版的关注点分离检测
        specialized_files = sum(1 for f in cs_files if any(keyword in str(f).lower()
                              for keyword in ["controller", "model", "view", "service", "manager", "utility"]))
        return specialized_files >= 3

    # 组件分析方法
    def _detect_component_type(self, content: str) -> str:
        """检测组件类型"""
        if "MonoBehaviour" in content:
            if "ScriptableObject" in content:
                return "ScriptableObject"
            else:
                return "MonoBehaviour"
        elif "ScriptableObject" in content:
            return "ScriptableObject"
        else:
            return "Utility"

    def _extract_dependencies(self, content: str) -> List[str]:
        """提取依赖"""
        dependencies = []

        # 提取using语句
        using_pattern = r"using\s+([\w\.]+);"
        for match in re.finditer(using_pattern, content):
            dependency = match.group(1)
            if dependency not in ["System", "UnityEngine", "UnityEditor"]:
                dependencies.append(dependency)

        return dependencies

    def _calculate_performance_score(self, content: str) -> float:
        """计算性能评分"""
        score = 100.0

        # GC分配扣分
        if self._has_gc_allocation_issues(content):
            score -= 20

        # 昂贵方法调用扣分
        if self._has_expensive_method_calls(content):
            score -= 15

        # 低效Update扣分
        if self._has_inefficient_update(content):
            score -= 25

        return max(0.0, score)

    def _extract_class_name(self, content: str, filename: str) -> str:
        """提取类名"""
        class_pattern = r"public\s+class\s+(\w+)"
        match = re.search(class_pattern, content)
        if match:
            return match.group(1)

        # 如果找不到public class，尝试其他模式
        class_pattern = r"class\s+(\w+)"
        match = re.search(class_pattern, content)
        if match:
            return match.group(1)

        # 使用文件名作为备选
        return Path(filename).stem

    def _extract_build_targets(self, content: str) -> str:
        """提取构建目标"""
        if "StandaloneWindows64" in content:
            return "Windows"
        elif "Android" in content:
            return "Android"
        elif "iOS" in content:
            return "iOS"
        elif "WebGL" in content:
            return "WebGL"
        else:
            return "Unknown"

def generate_unity_report(analysis: UnityArchitectureAnalysis) -> str:
    """生成Unity架构分析报告"""
    report = "# Unity项目架构分析报告\n\n"

    # 项目概览
    report += "## 📊 项目概览\n"
    report += f"- **Unity版本**: {analysis.project_info.get('unity_version', 'Unknown')}\n"
    report += f"- **目标平台**: {analysis.project_info.get('target_platform', 'Unknown')}\n"
    report += f"- **质量评分**: {analysis.quality_score:.1f}/100\n"
    report += f"- **检测到的架构模式**: {len(analysis.patterns)} 个\n"
    report += f"- **性能问题**: {len(analysis.performance_issues)} 个\n"
    report += f"- **最佳实践**: {len(analysis.best_practices)} 个\n\n"

    # 架构模式分析
    report += "## 🏗️ 架构模式识别\n"
    if analysis.patterns:
        for pattern in analysis.patterns:
            report += f"- ✅ **{pattern.value}**: 检测到该架构模式\n"
    else:
        report += "- ⚠️ 未检测到明确的架构模式\n"
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
        report += "- 💡 建议改进代码实践\n"
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

        report += "\n### 高性能组件:\n"
        for comp in sorted_components[-3:]:  # 显示最后3个性能较高的
            report += f"- **{comp.name}** ({comp.type}): {comp.performance_score:.1f}分\n"
    report += "\n"

    # 资源组织
    report += "## 📁 资源组织分析\n"
    for category, files in analysis.asset_organisation.items():
        if files:
            report += f"- **{category.title()}**: {len(files)} 个文件\n"
    report += "\n"

    # 场景结构
    report += "## 🎬 场景结构\n"
    report += f"- **场景文件**: {len(analysis.scene_structure['scenes'])} 个\n"
    report += f"- **预制体**: {len(analysis.scene_structure['prefabs'])} 个\n\n"

    # 改进建议
    report += "## 💡 改进建议\n"
    for i, suggestion in enumerate(analysis.recommendations, 1):
        report += f"{i}. {suggestion}\n"
    report += "\n"

    # 质量评估详情
    report += "## 📈 质量评估详情\n"
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
        print("用法: python unity_analyzer.py <Unity项目路径>")
        sys.exit(1)

    project_path = Path(sys.argv[1])
    analyzer = UnityArchitectureAnalyzer(project_path)
    analysis = analyzer.analyze()
    report = generate_unity_report(analysis)

    print(report)

    # 保存报告
    output_file = project_path / "unity_architecture_analysis.md"
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(report)

    print(f"\n📄 报告已保存到: {output_file}")