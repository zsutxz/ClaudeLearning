using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR && UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEditor;
using UnityEngine.Rendering.Universal;
#endif

namespace CoinAnimation.Core.Compatibility
{
#if UNITY_EDITOR && UNITY_RENDER_PIPELINE_UNIVERSAL
    /// <summary>
    /// URP着色器兼容性检查器 - 检查着色器与URP的兼容性
    /// URP Shader Compatibility Checker - Checks shader compatibility with URP
    /// </summary>
    public class URPShaderCompatibilityChecker
    {
        [Header("Shader Check Configuration")]
        [SerializeField] private bool enableDetailedLogging = true;
        [SerializeField] private List<string> criticalShaders = new List<string>();
        [SerializeField] private List<string> optionalShaders = new List<string>();

        [Header("Check Results")]
        [SerializeField] private ShaderCompatibilityReport compatibilityReport;

        private Dictionary<string, ShaderTestResult> shaderTestResults = new Dictionary<string, ShaderTestResult>();

        // 着色器测试结果结构
        [System.Serializable]
        public class ShaderTestResult
        {
            public string shaderName;
            public string shaderPath;
            public bool isFound;
            public bool isCompatible;
            public bool compilesSuccessfully;
            public string supportLevel; // 完全支持/部分支持/不支持
            public List<string> supportedFeatures = new List<string>();
            public List<string> unsupportedFeatures = new List<string>();
            public List<string> compilationErrors = new List<string>();
            public List<string> warnings = new List<string>();
            public float compilationTime;
            public int variantCount;
            public string recommendedAction;
            public ShaderTestType testType;
        }

        // 着色器兼容性报告
        [System.Serializable]
        public class ShaderCompatibilityReport
        {
            public DateTime checkDate;
            public string urpVersion;
            public string unityVersion;
            public List<ShaderTestResult> testResults = new List<ShaderTestResult>();
            public List<string> criticalIssues = new List<string>();
            public List<string> warnings = new List<string>();
            public List<string> recommendations = new List<string>();

            // 统计信息
            public int totalShadersTested;
            public int compatibleShaders;
            public int partiallyCompatibleShaders;
            public int incompatibleShaders;
            public float overallCompatibilityPercentage;
            public bool isSystemReadyForProduction;

            // 特定统计
            public int builtInShadersCount;
            public int customShadersCount;
            public int coinAnimationShadersCount;
            public int uiShadersCount;

            // 性能信息
            public float totalCompilationTime;
            public int totalVariants;
        }

        // 着色器测试类型
        public enum ShaderTestType
        {
            BuiltIn,        // 内置着色器
            Custom,         // 自定义着色器
            CoinAnimation,  // 金币动画着色器
            UI,            // UI着色器
            PostProcess    // 后处理着色器
        }

        // 着色器类别定义
        public class ShaderCategory
        {
            public string name;
            public List<string> shaderPaths;
            public ShaderTestType type;
            public bool isCritical;
            public string description;
        }

        /// <summary>
        /// 初始化着色器兼容性检查器
        /// Initialize shader compatibility checker
        /// </summary>
        public void Initialize()
        {
            // 初始化着色器列表
            InitializeShaderLists();

            // 创建兼容性报告
            compatibilityReport = new ShaderCompatibilityReport
            {
                checkDate = DateTime.Now,
                urpVersion = GetURPVersion(),
                unityVersion = Application.unityVersion
            };

            LogInfo("URP着色器兼容性检查器初始化完成");
            LogInfo($"URP版本: {compatibilityReport.urpVersion}");
            LogInfo($"Unity版本: {compatibilityReport.unityVersion}");
        }

        /// <summary>
        /// 运行完整的着色器兼容性检查
        /// Run complete shader compatibility check
        /// </summary>
        public IEnumerator RunCompleteCompatibilityCheck()
        {
            LogInfo("🚀 开始URP着色器兼容性检查...");

            // 1. 检查内置URP着色器
            yield return StartCoroutine(CheckBuiltInURPShaders());

            // 2. 检查金币动画着色器
            yield return StartCoroutine(CheckCoinAnimationShaders());

            // 3. 检查UI着色器
            yield return StartCoroutine(CheckUIShaders());

            // 4. 检查后处理着色器
            yield return StartCoroutine(CheckPostProcessShaders());

            // 5. 检查自定义着色器
            yield return StartCoroutine(CheckCustomShaders());

            // 6. 验证着色器变体
            yield return StartCoroutine(VerifyShaderVariants());

            // 7. 性能测试
            yield return StartCoroutine(RunShaderPerformanceTests());

            // 8. 生成最终报告
            GenerateCompatibilityReport();

            LogInfo("✅ URP着色器兼容性检查完成");
            yield return null;
        }

        /// <summary>
        /// 检查内置URP着色器
        /// Check built-in URP shaders
        /// </summary>
        private IEnumerator CheckBuiltInURPShaders()
        {
            LogInfo("🔍 检查内置URP着色器...");

            var builtInShaderCategories = new List<ShaderCategory>
            {
                new ShaderCategory
                {
                    name = "2D渲染器着色器",
                    type = ShaderTestType.BuiltIn,
                    isCritical = true,
                    description = "URP 2D渲染器核心着色器",
                    shaderPaths = new List<string>
                    {
                        "Universal Render Pipeline/2D/Sprite-Lit-Default",
                        "Universal Render Pipeline/2D/Sprite-Unlit-Default",
                        "Universal Render Pipeline/2D/Sprite-Lit-Advanced"
                    }
                },
                new ShaderCategory
                {
                    name = "前向渲染器着色器",
                    type = ShaderTestType.BuiltIn,
                    isCritical = true,
                    description = "URP前向渲染器核心着色器",
                    shaderPaths = new List<string>
                    {
                        "Universal Render Pipeline/Lit",
                        "Universal Render Pipeline/Unlit",
                        "Universal Render Pipeline/SimpleLit",
                        "Universal Render Pipeline/BakedLit"
                    }
                },
                new ShaderCategory
                {
                    name = "粒子着色器",
                    type = ShaderTestType.BuiltIn,
                    isCritical = false,
                    description = "URP粒子系统着色器",
                    shaderPaths = new List<string>
                    {
                        "Universal Render Pipeline/Particles/Lit",
                        "Universal Render Pipeline/Particles/Unlit",
                        "Universal Render Pipeline/Particles/SimpleLit"
                    }
                },
                new ShaderCategory
                {
                    name = "特殊效果着色器",
                    type = ShaderTestType.BuiltIn,
                    isCritical = false,
                    description = "URP特殊效果着色器",
                    shaderPaths = new List<string>
                    {
                        "Hidden/Universal Render Pipeline/FallbackError",
                        "Hidden/Universal Render Pipeline/Blit",
                        "Hidden/Universal Render Pipeline/CopyDepth"
                    }
                }
            };

            foreach (var category in builtInShaderCategories)
            {
                yield return StartCoroutine(CheckShaderCategory(category));
                yield return null; // 避免帧率下降
            }

            LogInfo("✅ 内置URP着色器检查完成");
        }

        /// <summary>
        /// 检查金币动画着色器
        /// Check coin animation shaders
        /// </summary>
        private IEnumerator CheckCoinAnimationShaders()
        {
            LogInfo("🪙 检查金币动画着色器...");

            var coinShaderCategory = new ShaderCategory
            {
                name = "金币动画着色器",
                type = ShaderTestType.CoinAnimation,
                isCritical = true,
                description = "金币动画系统专用着色器",
                shaderPaths = new List<string>
                {
                    "CoinAnimation/URP/CoinShader",
                    "CoinAnimation/URP/UnlitCoin",
                    "CoinAnimation/URP/MetallicCoin",
                    "CoinAnimation/URP/GlowingCoin",
                    "CoinAnimation/URP/ParticleCoin"
                }
            };

            yield return StartCoroutine(CheckShaderCategory(coinShaderCategory));
            LogInfo("✅ 金币动画着色器检查完成");
        }

        /// <summary>
        /// 检查UI着色器
        /// Check UI shaders
        /// </summary>
        private IEnumerator CheckUIShaders()
        {
            LogInfo("🎨 检查UI着色器...");

            var uiShaderCategory = new ShaderCategory
            {
                name = "UI着色器",
                type = ShaderTestType.UI,
                isCritical = true,
                description = "UGUI和TextMeshPro着色器",
                shaderPaths = new List<string>
                {
                    "Universal Render Pipeline/2D/Sprite-Lit-Default",
                    "Universal Render Pipeline/2D/Sprite-Unlit-Default",
                    "TextMeshPro/Universal Render Pipeline/TextMeshPro",
                    "TextMeshPro/Universal Render Pipeline/TextMeshPro/Sprite",
                    "UI/Default"
                }
            };

            yield return StartCoroutine(CheckShaderCategory(uiShaderCategory));
            LogInfo("✅ UI着色器检查完成");
        }

        /// <summary>
        /// 检查后处理着色器
        /// Check post process shaders
        /// </summary>
        private IEnumerator CheckPostProcessShaders()
        {
            LogInfo("🎭 检查后处理着色器...");

            var postProcessShaderCategory = new ShaderCategory
            {
                name = "后处理着色器",
                type = ShaderTestType.PostProcess,
                isCritical = false,
                description = "URP后处理效果着色器",
                shaderPaths = new List<string>
                {
                    "Hidden/Universal Render Pipeline/PostProcessing/Bloom",
                    "Hidden/Universal Render Pipeline/PostProcessing/Tonemapping",
                    "Hidden/Universal Render Pipeline/PostProcessing/Vignette",
                    "Hidden/Universal Render Pipeline/PostProcessing/ColorAdjustments",
                    "Hidden/Universal Render Pipeline/PostProcessing/DepthOfField"
                }
            };

            yield return StartCoroutine(CheckShaderCategory(postProcessShaderCategory));
            LogInfo("✅ 后处理着色器检查完成");
        }

        /// <summary>
        /// 检查自定义着色器
        /// Check custom shaders
        /// </summary>
        private IEnumerator CheckCustomShaders()
        {
            LogInfo("🔧 检查自定义着色器...");

            // 检查项目中可能存在的自定义着色器
            var customShaders = FindCustomShaders();

            if (customShaders.Count > 0)
            {
                var customShaderCategory = new ShaderCategory
                {
                    name = "自定义着色器",
                    type = ShaderTestType.Custom,
                    isCritical = false,
                    description = "项目中的自定义着色器",
                    shaderPaths = customShaders
                };

                yield return StartCoroutine(CheckShaderCategory(customShaderCategory));
            }
            else
            {
                LogInfo("ℹ️ 未发现自定义着色器");
            }

            LogInfo("✅ 自定义着色器检查完成");
        }

        /// <summary>
        /// 检查着色器类别
        /// Check shader category
        /// </summary>
        private IEnumerator CheckShaderCategory(ShaderCategory category)
        {
            LogInfo($"🔍 检查 {category.name}...");

            foreach (var shaderPath in category.shaderPaths)
            {
                yield return StartCoroutine(CheckSingleShader(shaderPath, category.type, category.isCritical));
                yield return null; // 避免性能问题
            }

            LogInfo($"✅ {category.name} 检查完成");
        }

        /// <summary>
        /// 检查单个着色器
        /// Check single shader
        /// </summary>
        private IEnumerator CheckSingleShader(string shaderPath, ShaderTestType testType, bool isCritical)
        {
            var startTime = Time.realtimeSinceStartup;
            var result = new ShaderTestResult
            {
                shaderName = Path.GetFileNameWithoutExtension(shaderPath),
                shaderPath = shaderPath,
                testType = testType
            };

            try
            {
                // 1. 查找着色器
                var shader = Shader.Find(shaderPath);
                result.isFound = shader != null;

                if (!result.isFound)
                {
                    result.isCompatible = false;
                    result.supportLevel = "不支持";
                    result.recommendedAction = isCritical ?
                        "关键着色器缺失，需要安装或创建此着色器" :
                        "可选着色器缺失，不影响核心功能";

                    if (isCritical)
                    {
                        LogError($"❌ 关键着色器未找到: {shaderPath}");
                    }
                    else
                    {
                        LogWarning($"⚠️ 可选着色器未找到: {shaderPath}");
                    }
                }
                else
                {
                    // 2. 检查着色器兼容性
                    yield return StartCoroutine(AnalyzeShaderCompatibility(shader, result));

                    // 3. 测试着色器编译
                    yield return StartCoroutine(TestShaderCompilation(shader, result));

                    // 4. 分析着色器功能
                    yield return StartCoroutine(AnalyzeShaderFeatures(shader, result));
                }
            }
            catch (Exception ex)
            {
                result.compilationErrors.Add($"检查异常: {ex.Message}");
                result.isCompatible = false;
                LogError($"❌ 着色器检查异常 {shaderPath}: {ex.Message}");
            }

            result.compilationTime = Time.realtimeSinceStartup - startTime;
            shaderTestResults[shaderPath] = result;

            yield return null;
        }

        /// <summary>
        /// 分析着色器兼容性
        /// Analyze shader compatibility
        /// </summary>
        private IEnumerator AnalyzeShaderCompatibility(Shader shader, ShaderTestResult result)
        {
            // 检查着色器是否支持URP
            var shaderName = shader.name.ToLowerInvariant();
            var isURPShader = shaderName.Contains("universal render pipeline") ||
                             shaderName.Contains("urp") ||
                             shaderName.Contains("2d");

            if (isURPShader)
            {
                result.isCompatible = true;
                result.supportLevel = "完全支持";
                result.supportedFeatures.Add("URP原生支持");
            }
            else
            {
                // 检查是否是内置着色器
                if (shaderName.Contains("standard") || shaderName.Contains("builtin"))
                {
                    result.isCompatible = false;
                    result.supportLevel = "不支持";
                    result.unsupportedFeatures.Add("内置着色器不兼容URP");
                    result.recommendedAction = "替换为对应的URP着色器";
                }
                else
                {
                    result.isCompatible = true;
                    result.supportLevel = "部分支持";
                    result.warnings.Add("自定义着色器需要验证URP兼容性");
                    result.recommendedAction = "测试着色器在URP下的表现";
                }
            }

            yield return null;
        }

        /// <summary>
        /// 测试着色器编译
        /// Test shader compilation
        /// </summary>
        private IEnumerator TestShaderCompilation(Shader shader, ShaderTestResult result)
        {
            try
            {
                // 创建临时材质测试编译
                var testMaterial = new Material(shader);

                if (testMaterial != null)
                {
                    result.compilesSuccessfully = true;
                    result.supportedFeatures.Add("编译成功");

                    // 测试基本属性设置
                    if (shader.HasProperty("_BaseColor"))
                    {
                        testMaterial.SetColor("_BaseColor", Color.white);
                        result.supportedFeatures.Add("基础颜色属性");
                    }

                    if (shader.HasProperty("_MainTex"))
                    {
                        testMaterial.SetTexture("_MainTex", Texture2D.whiteTexture);
                        result.supportedFeatures.Add("主纹理属性");
                    }

                    if (shader.HasProperty("_Metallic"))
                    {
                        testMaterial.SetFloat("_Metallic", 0.5f);
                        result.supportedFeatures.Add("金属度属性");
                    }

                    if (shader.HasProperty("_Smoothness"))
                    {
                        testMaterial.SetFloat("_Smoothness", 0.5f);
                        result.supportedFeatures.Add("光滑度属性");
                    }

                    // 清理测试材质
                    UnityEngine.Object.DestroyImmediate(testMaterial);
                }
                else
                {
                    result.compilesSuccessfully = false;
                    result.compilationErrors.Add("无法创建材质");
                    result.isCompatible = false;
                }
            }
            catch (Exception ex)
            {
                result.compilesSuccessfully = false;
                result.compilationErrors.Add($"编译测试失败: {ex.Message}");
                result.isCompatible = false;
            }

            yield return null;
        }

        /// <summary>
        /// 分析着色器功能
        /// Analyze shader features
        /// </summary>
        private IEnumerator AnalyzeShaderFeatures(Shader shader, ShaderTestResult result)
        {
            // 获取着色器关键字
            var keywordCount = shader.keywordSpace.keywordCount;
            result.variantCount = keywordCount;

            if (keywordCount > 0)
            {
                result.supportedFeatures.Add($"支持 {keywordCount} 个着色器关键字");
            }

            // 检查渲染队列
            var renderQueue = shader.defaultQueue;
            if (renderQueue >= 2500) // 透明队列
            {
                result.supportedFeatures.Add("透明渲染支持");
            }
            else if (renderQueue >= 2000) // 几何队列
            {
                result.supportedFeatures.Add("不透明渲染支持");
            }

            // 检查着色器通道
            #if UNITY_EDITOR
            var subshaderCount = shader.subshaderCount;
            if (subshaderCount > 1)
            {
                result.supportedFeatures.Add($"支持 {subshaderCount} 个子着色器");
            }
            #endif

            yield return null;
        }

        /// <summary>
        /// 验证着色器变体
        /// Verify shader variants
        /// </summary>
        private IEnumerator VerifyShaderVariants()
        {
            LogInfo("🔧 验证着色器变体...");

            foreach (var kvp in shaderTestResults)
            {
                var result = kvp.Value;
                if (result.isFound && result.isCompatible)
                {
                    yield return StartCoroutine(VerifyShaderVariants(kvp.Key, result));
                }
            }

            LogInfo("✅ 着色器变体验证完成");
        }

        /// <summary>
        /// 验证单个着色器的变体
        /// Verify shader variants for single shader
        /// </summary>
        private IEnumerator VerifyShaderVariants(string shaderPath, ShaderTestResult result)
        {
            try
            {
                var shader = Shader.Find(shaderPath);
                if (shader != null)
                {
                    // 简单检查着色器变体数量
                    #if UNITY_EDITOR
                    var variantCount = ShaderUtil.GetVariantCount(shader);
                    if (variantCount > 0)
                    {
                        result.supportedFeatures.Add($"包含 {variantCount} 个着色器变体");

                        if (variantCount > 1000)
                        {
                            result.warnings.Add("着色器变体数量较多，可能影响构建大小");
                        }
                    }
                    #endif
                }
            }
            catch (Exception ex)
            {
                result.warnings.Add($"变体验证异常: {ex.Message}");
            }

            yield return null;
        }

        /// <summary>
        /// 运行着色器性能测试
        /// Run shader performance tests
        /// </summary>
        private IEnumerator RunShaderPerformanceTests()
        {
            LogInfo("⚡ 运行着色器性能测试...");

            // 创建测试场景
            var testCoins = new List<GameObject>();

            try
            {
                // 为每个兼容的着色器创建测试对象
                foreach (var kvp in shaderTestResults)
                {
                    var result = kvp.Value;
                    if (result.isCompatible && result.compilesSuccessfully)
                    {
                        var testCoin = CreateTestCoin(result.shaderPath);
                        if (testCoin != null)
                        {
                            testCoins.Add(testCoin);
                        }
                    }
                }

                // 运行性能基准测试
                if (testCoins.Count > 0)
                {
                    yield return StartCoroutine(RunPerformanceBenchmark(testCoins));
                }
            }
            finally
            {
                // 清理测试对象
                foreach (var coin in testCoins)
                {
                    if (coin != null)
                    {
                        UnityEngine.Object.DestroyImmediate(coin);
                    }
                }
            }

            LogInfo("✅ 着色器性能测试完成");
        }

        /// <summary>
        /// 创建测试金币
        /// Create test coin
        /// </summary>
        private GameObject CreateTestCoin(string shaderPath)
        {
            try
            {
                var coin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                coin.name = $"TestCoin_{Path.GetFileNameWithoutExtension(shaderPath)}";

                var renderer = coin.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var shader = Shader.Find(shaderPath);
                    if (shader != null)
                    {
                        var material = new Material(shader);

                        // 设置基本属性
                        if (shader.HasProperty("_BaseColor"))
                        {
                            material.SetColor("_BaseColor", Color.yellow);
                        }
                        else if (shader.HasProperty("_Color"))
                        {
                            material.SetColor("_Color", Color.yellow);
                        }

                        renderer.material = material;
                        return coin;
                    }
                }

                UnityEngine.Object.DestroyImmediate(coin);
                return null;
            }
            catch (Exception ex)
            {
                LogError($"❌ 创建测试金币失败 {shaderPath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 运行性能基准测试
        /// Run performance benchmark
        /// </summary>
        private IEnumerator RunPerformanceBenchmark(List<GameObject> testCoins)
        {
            LogInfo($"⏱️ 运行 {testCoins.Count} 个着色器的性能基准测试...");

            var testDuration = 5f;
            var startTime = Time.time;
            var frameCount = 0;

            while (Time.time - startTime < testDuration)
            {
                // 简单动画测试
                foreach (var coin in testCoins)
                {
                    if (coin != null)
                    {
                        coin.transform.Rotate(0f, 90f * Time.deltaTime, 0f);
                    }
                }

                frameCount++;
                yield return null;
            }

            var averageFPS = frameCount / testDuration;
            LogInfo($"📊 着色器性能测试结果: 平均FPS {averageFPS:F1}");

            yield return null;
        }

        /// <summary>
        /// 生成兼容性报告
        /// Generate compatibility report
        /// </summary>
        private void GenerateCompatibilityReport()
        {
            LogInfo("📋 生成着色器兼容性报告...");

            // 转换结果到报告格式
            compatibilityReport.testResults.Clear();
            compatibilityReport.criticalIssues.Clear();
            compatibilityReport.warnings.Clear();
            compatibilityReport.recommendations.Clear();

            int compatibleCount = 0;
            int partiallyCompatibleCount = 0;
            int incompatibleCount = 0;
            int builtInCount = 0;
            int customCount = 0;
            int coinAnimationCount = 0;
            int uiCount = 0;

            float totalCompilationTime = 0f;
            int totalVariants = 0;

            foreach (var kvp in shaderTestResults)
            {
                var result = kvp.Value;
                compatibilityReport.testResults.Add(result);

                // 统计兼容性
                if (result.isCompatible && result.compilesSuccessfully)
                {
                    if (result.supportLevel == "完全支持")
                    {
                        compatibleCount++;
                    }
                    else
                    {
                        partiallyCompatibleCount++;
                    }
                }
                else
                {
                    incompatibleCount++;

                    if (result.isFound && !result.compilesSuccessfully)
                    {
                        compatibilityReport.criticalIssues.Add($"{result.shaderName}: 编译失败");
                    }
                }

                // 统计类型
                switch (result.testType)
                {
                    case ShaderTestType.BuiltIn:
                        builtInCount++;
                        break;
                    case ShaderTestType.Custom:
                        customCount++;
                        break;
                    case ShaderTestType.CoinAnimation:
                        coinAnimationCount++;
                        break;
                    case ShaderTestType.UI:
                        uiCount++;
                        break;
                }

                // 收集统计信息
                totalCompilationTime += result.compilationTime;
                totalVariants += result.variantCount;

                // 收集问题和建议
                compatibilityReport.warnings.AddRange(result.warnings);
                if (!string.IsNullOrEmpty(result.recommendedAction))
                {
                    compatibilityReport.recommendations.Add(result.recommendedAction);
                }
            }

            // 设置报告统计
            compatibilityReport.totalShadersTested = shaderTestResults.Count;
            compatibilityReport.compatibleShaders = compatibleCount;
            compatibilityReport.partiallyCompatibleShaders = partiallyCompatibleCount;
            compatibilityReport.incompatibleShaders = incompatibleCount;
            compatibilityReport.builtInShadersCount = builtInCount;
            compatibilityReport.customShadersCount = customCount;
            compatibilityReport.coinAnimationShadersCount = coinAnimationCount;
            compatibilityReport.uiShadersCount = uiCount;
            compatibilityReport.totalCompilationTime = totalCompilationTime;
            compatibilityReport.totalVariants = totalVariants;

            // 计算兼容性百分比
            if (compatibilityReport.totalShadersTested > 0)
            {
                var totalCompatible = compatibleCount + partiallyCompatibleCount;
                compatibilityReport.overallCompatibilityPercentage =
                    (float)totalCompatible / compatibilityReport.totalShadersTested * 100f;
            }

            // 判断系统是否准备好用于生产
            compatibilityReport.isSystemReadyForProduction =
                compatibilityReport.overallCompatibilityPercentage >= 80f &&
                compatibilityReport.criticalIssues.Count == 0 &&
                compatibilityReport.coinAnimationShadersCount > 0;

            // 输出报告摘要
            LogReportSummary();
        }

        /// <summary>
        /// 输出报告摘要
        /// Log report summary
        /// </summary>
        private void LogReportSummary()
        {
            LogInfo("=" * 60);
            LogInfo("📊 URP着色器兼容性检查报告摘要");
            LogInfo("=" * 60);
            LogInfo($"检查日期: {compatibilityReport.checkDate}");
            LogInfo($"URP版本: {compatibilityReport.urpVersion}");
            LogInfo($"Unity版本: {compatibilityReport.unityVersion}");
            LogInfo($"总着色器数: {compatibilityReport.totalShadersTested}");
            LogInfo($"完全兼容: {compatibilityReport.compatibleShaders}");
            LogInfo($"部分兼容: {compatibilityReport.partiallyCompatibleShaders}");
            LogInfo($"不兼容: {compatibilityReport.incompatibleShaders}");
            LogInfo($"兼容性百分比: {compatibilityReport.overallCompatibilityPercentage:F1}%");

            LogInfo($"\n📈 着色器类型统计:");
            LogInfo($"   内置着色器: {compatibilityReport.builtInShadersCount}");
            LogInfo($"   自定义着色器: {compatibilityReport.customShadersCount}");
            LogInfo($"   金币动画着色器: {compatibilityReport.coinAnimationShadersCount}");
            LogInfo($"   UI着色器: {compatibilityReport.uiShadersCount}");

            LogInfo($"\n⚡ 性能信息:");
            LogInfo($"   总编译时间: {compatibilityReport.totalCompilationTime:F3}秒");
            LogInfo($"   总着色器变体: {compatibilityReport.totalVariants}");

            LogInfo($"\n🎯 生产就绪: {(compatibilityReport.isSystemReadyForProduction ? "✅" : "❌")}");

            if (compatibilityReport.criticalIssues.Count > 0)
            {
                LogInfo("\n🚨 关键问题:");
                foreach (var issue in compatibilityReport.criticalIssues)
                {
                    LogInfo($"   • {issue}");
                }
            }

            if (compatibilityReport.warnings.Count > 0)
            {
                LogInfo("\n⚠️ 警告:");
                foreach (var warning in compatibilityReport.warnings.Take(5)) // 只显示前5个
                {
                    LogInfo($"   • {warning}");
                }
                if (compatibilityReport.warnings.Count > 5)
                {
                    LogInfo($"   • ... 还有 {compatibilityReport.warnings.Count - 5} 个警告");
                }
            }

            if (compatibilityReport.recommendations.Count > 0)
            {
                LogInfo("\n💡 建议:");
                foreach (var recommendation in compatibilityReport.recommendations.Take(5)) // 只显示前5个
                {
                    LogInfo($"   • {recommendation}");
                }
                if (compatibilityReport.recommendations.Count > 5)
                {
                    LogInfo($"   • ... 还有 {compatibilityReport.recommendations.Count - 5} 个建议");
                }
            }

            LogInfo("=" * 60);
        }

        /// <summary>
        /// 查找自定义着色器
        /// Find custom shaders
        /// </summary>
        private List<string> FindCustomShaders()
        {
            var customShaders = new List<string>();

            #if UNITY_EDITOR
            // 在项目中查找着色器文件
            var shaderGUIDs = AssetDatabase.FindAssets("t:Shader");
            foreach (var guid in shaderGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                // 过滤掉内置着色器
                if (!path.Contains("Resources/unity_builtin_extra") &&
                    !path.Contains("Packages/com.unity.render-pipelines.universal"))
                {
                    var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                    if (shader != null)
                    {
                        customShaders.Add(shader.name);
                    }
                }
            }
            #endif

            return customShaders;
        }

        /// <summary>
        /// 获取URP版本
        /// Get URP version
        /// </summary>
        private string GetURPVersion()
        {
            #if UNITY_EDITOR
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForPackageName("com.unity.render-pipelines.universal");
            if (packageInfo != null)
            {
                return packageInfo.version;
            }
            #endif

            return "未知版本";
        }

        /// <summary>
        /// 初始化着色器列表
        /// Initialize shader lists
        /// </summary>
        private void InitializeShaderLists()
        {
            // 关键着色器
            criticalShaders.AddRange(new[]
            {
                "Universal Render Pipeline/Lit",
                "Universal Render Pipeline/Unlit",
                "Universal Render Pipeline/2D/Sprite-Lit-Default",
                "Universal Render Pipeline/2D/Sprite-Unlit-Default"
            });

            // 可选着色器
            optionalShaders.AddRange(new[]
            {
                "Universal Render Pipeline/SimpleLit",
                "Universal Render Pipeline/BakedLit",
                "Universal Render Pipeline/Particles/Lit",
                "Universal Render Pipeline/Particles/Unlit"
            });
        }

        /// <summary>
        /// 获取兼容性报告
        /// Get compatibility report
        /// </summary>
        public ShaderCompatibilityReport GetCompatibilityReport()
        {
            return compatibilityReport;
        }

        /// <summary>
        /// 导出兼容性报告到文件
        /// Export compatibility report to file
        /// </summary>
        public void ExportReportToFile(string filePath)
        {
            try
            {
                var json = JsonUtility.ToJson(compatibilityReport, true);
                File.WriteAllText(filePath, json);
                LogInfo($"📄 着色器兼容性报告已导出到: {filePath}");
            }
            catch (Exception ex)
            {
                LogError($"❌ 导出着色器兼容性报告失败: {ex.Message}");
            }
        }

        private void LogInfo(string message)
        {
            if (enableDetailedLogging)
            {
                Debug.Log($"[URP着色器兼容性] {message}");
            }
        }

        private void LogWarning(string message)
        {
            if (enableDetailedLogging)
            {
                Debug.LogWarning($"[URP着色器兼容性] {message}");
            }
        }

        private void LogError(string message)
        {
            if (enableDetailedLogging)
            {
                Debug.LogError($"[URP着色器兼容性] {message}");
            }
        }
    }
}
#endif

#if !UNITY_EDITOR || !UNITY_RENDER_PIPELINE_UNIVERSAL
    /// <summary>
    /// URP着色器兼容性检查器占位符 - URP未安装
    /// URP Shader Compatibility Checker Placeholder - URP not installed
    /// </summary>
    public class URPShaderCompatibilityChecker
    {
        public void Initialize() => UnityEngine.Debug.Log("URP未安装，跳过URP着色器兼容性检查");
        public System.Collections.IEnumerator RunCompleteCompatibilityCheck() => null;
        public object GetCompatibilityReport() => null;
        public void ExportReportToFile(string filePath) => UnityEngine.Debug.Log("URP未安装，无法导出报告");
    }
#endif
}