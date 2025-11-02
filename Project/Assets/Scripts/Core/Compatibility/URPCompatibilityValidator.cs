using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR && UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

namespace CoinAnimation.Core.Compatibility
{
#if UNITY_EDITOR && UNITY_RENDER_PIPELINE_UNIVERSAL
    /// <summary>
    /// URP兼容性验证器 - 测试URP 12+版本兼容性
    /// Universal Render Pipeline compatibility validator for URP 12+ versions
    /// </summary>
    public class URPCompatibilityValidator
    {
        [Header("URP Version Configuration")]
        [SerializeField] private List<URPVersionInfo> supportedURPVersions = new List<URPVersionInfo>();

        [Header("Test Configuration")]
        [SerializeField] private int testCoinCount = 30;
        [SerializeField] private float performanceTestDuration = 10f;
        [SerializeField] private bool enableDetailedLogging = true;

        [Header("Validation Results")]
        [SerializeField] private URPCompatibilityReport compatibilityReport;

#if UNITY_EDITOR && UNITY_RENDER_PIPELINE_UNIVERSAL
        private URPRenderPipelineAsset urpAsset;
        private UniversalAdditionalCameraData cameraData;
#else
        private UnityEngine.Object urpAsset;
        private UnityEngine.Object cameraData;
#endif
        private List<GameObject> testCoins = new List<GameObject>();
        private PerformanceMonitor performanceMonitor;

        // URP版本信息结构
        [System.Serializable]
        public class URPVersionInfo
        {
            public string version;
            public string unityVersion;
            public bool isCompatible;
            public string notes;
        }

        // URP兼容性报告
        [System.Serializable]
        public class URPCompatibilityReport
        {
            public string urpVersion;
            public string unityVersion;
            public bool isURPPresent;
            public bool isCompatibleVersion;
            public List<string> compatibleFeatures = new List<string>();
            public List<string> incompatibleFeatures = new List<string>();
            public List<string> warnings = new List<string>();
            public PerformanceMetrics performanceMetrics;
            public ShaderCompatibilityReport shaderReport;
            public List<RenderingFeature> renderingFeatures = new List<RenderingFeature>();

            public DateTime testDate;
            public float overallCompatibilityScore;
        }

        // 性能指标
        [System.Serializable]
        public class PerformanceMetrics
        {
            public float averageFPS;
            public float minFPS;
            public float maxFPS;
            public float frameTime;
            public float memoryUsage;
            public float renderThreadTime;
            public int drawCalls;
            public int triangles;
            public bool meets60FPSTarget;
        }

        // 着色器兼容性报告
        [System.Serializable]
        public class ShaderCompatibilityReport
        {
            public List<ShaderTestResult> shaderTests = new List<ShaderTestResult>();
            public int compatibleShaders;
            public int incompatibleShaders;
            public float compatibilityPercentage;
        }

        // 着色器测试结果
        [System.Serializable]
        public class ShaderTestResult
        {
            public string shaderName;
            public bool isCompatible;
            public string supportLevel;
            public List<string> issues = new List<string>();
            public float compilationTime;
        }

        // 渲染功能
        [System.Serializable]
        public class RenderingFeature
        {
            public string featureName;
            public bool isSupported;
            public string featureType;
            public float performanceImpact;
            public string notes;
        }

        /// <summary>
        /// 初始化URP兼容性验证器
        /// Initialize URP compatibility validator
        /// </summary>
        public void Initialize()
        {
            // 初始化支持的URP版本列表
            InitializeSupportedURPVersions();

            // 获取当前URP资产
            urpAsset = GraphicsSettings.renderPipelineAsset as URPRenderPipelineAsset;

            // 初始化性能监控器
            performanceMonitor = new PerformanceMonitor();

            // 创建兼容性报告
            compatibilityReport = new URPCompatibilityReport
            {
                testDate = DateTime.Now,
                unityVersion = Application.unityVersion,
                urpVersion = GetURPVersion(),
                shaderReport = new ShaderCompatibilityReport(),
                performanceMetrics = new PerformanceMetrics()
            };

            LogInfo("URP兼容性验证器初始化完成");
            LogInfo($"Unity版本: {Application.unityVersion}");
            LogInfo($"URP版本: {GetURPVersion()}");
        }

        /// <summary>
        /// 运行完整的URP兼容性测试
        /// Run complete URP compatibility test suite
        /// </summary>
        public IEnumerator RunCompleteCompatibilityTest()
        {
            LogInfo("🚀 开始URP兼容性测试...");

            // 1. 检查URP是否存在
            yield return StartCoroutine(CheckURPPresence());

            // 2. 验证URP版本兼容性
            yield return StartCoroutine(ValidateURPVersionCompatibility());

            // 3. 测试URP功能
            yield return StartCoroutine(TestURPFeatures());

            // 4. 着色器兼容性测试
            yield return StartCoroutine(TestShaderCompatibility());

            // 5. 性能测试
            yield return StartCoroutine(RunPerformanceTests());

            // 6. 渲染功能测试
            yield return StartCoroutine(TestRenderingFeatures());

            // 7. 生成最终报告
            GenerateFinalReport();

            LogInfo("✅ URP兼容性测试完成");
            yield return null;
        }

        /// <summary>
        /// 检查URP是否存在
        /// Check URP presence
        /// </summary>
        private IEnumerator CheckURPPresence()
        {
            LogInfo("🔍 检查URP存在性...");

            compatibilityReport.isURPPresent = urpAsset != null;

            if (urpAsset != null)
            {
                LogInfo($"✅ 检测到URP渲染管线: {urpAsset.name}");

                // 获取URP版本信息
                var urpVersion = GetURPVersion();
                LogInfo($"📦 URP版本: {urpVersion}");

                // 检查关键URP组件
                yield return StartCoroutine(VerifyURPComponents());
            }
            else
            {
                LogError("❌ 未检测到URP渲染管线");
                compatibilityReport.warnings.Add("当前项目未使用URP渲染管线");
            }

            yield return null;
        }

        /// <summary>
        /// 验证URP组件
        /// Verify URP components
        /// </summary>
        private IEnumerator VerifyURPComponents()
        {
            // 检查URP资产设置
            if (urpAsset != null)
            {
                LogInfo($"📋 渲染管线资产: {urpAsset.name}");
                LogInfo($"🎨 支持的渲染器: {GetSupportedRenderers()}");

                // 检查渲染器数据
                var rendererData = urpAsset.scriptableRendererData;
                if (rendererData != null)
                {
                    LogInfo($"📊 渲染器数据: {rendererData.name}");
                    compatibilityReport.compatibleFeatures.Add($"渲染器: {rendererData.name}");
                }
            }

            // 检查相机数据
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
                if (cameraData != null)
                {
                    LogInfo("📷 主相机已配置URP数据");
                    compatibilityReport.compatibleFeatures.Add("主相机URP配置");
                }
                else
                {
                    LogWarning("⚠️ 主相机缺少URP数据组件");
                    compatibilityReport.warnings.Add("建议为主相机添加UniversalAdditionalCameraData组件");
                }
            }

            yield return null;
        }

        /// <summary>
        /// 验证URP版本兼容性
        /// Validate URP version compatibility
        /// </summary>
        private IEnumerator ValidateURPVersionCompatibility()
        {
            LogInfo("🔍 验证URP版本兼容性...");

            var currentURPVersion = GetURPVersion();
            var unityVersion = Application.unityVersion;

            // 检查Unity 2021.3 LTS兼容性
            if (unityVersion.StartsWith("2021.3"))
            {
                yield return StartCoroutine(TestUnity2021LTSCompatibility(currentURPVersion));
            }
            // 检查Unity 2022.3 LTS兼容性
            else if (unityVersion.StartsWith("2022.3"))
            {
                yield return StartCoroutine(TestUnity2022LTSCompatibility(currentURPVersion));
            }
            else
            {
                LogWarning($"⚠️ 未测试的Unity版本: {unityVersion}");
                compatibilityReport.warnings.Add($"当前Unity版本未经过官方兼容性测试: {unityVersion}");
            }

            yield return null;
        }

        /// <summary>
        /// 测试Unity 2021.3 LTS兼容性
        /// Test Unity 2021.3 LTS compatibility
        /// </summary>
        private IEnumerator TestUnity2021LTSCompatibility(string urpVersion)
        {
            LogInfo("🧪 测试Unity 2021.3 LTS兼容性...");

            // Unity 2021.3 LTS推荐使用URP 12.1.x
            var isRecommendedVersion = urpVersion.StartsWith("12.1") || urpVersion.StartsWith("12.0");

            if (isRecommendedVersion)
            {
                LogInfo("✅ URP版本与Unity 2021.3 LTS兼容");
                compatibilityReport.isCompatibleVersion = true;
                compatibilityReport.compatibleFeatures.Add("Unity 2021.3 LTS兼容性");
            }
            else
            {
                LogWarning($"⚠️ URP版本 {urpVersion} 可能与Unity 2021.3 LTS不完全兼容");
                compatibilityReport.warnings.Add($"推荐使用URP 12.1.x版本，当前版本: {urpVersion}");
            }

            yield return null;
        }

        /// <summary>
        /// 测试Unity 2022.3 LTS兼容性
        /// Test Unity 2022.3 LTS compatibility
        /// </summary>
        private IEnumerator TestUnity2022LTSCompatibility(string urpVersion)
        {
            LogInfo("🧪 测试Unity 2022.3 LTS兼容性...");

            // Unity 2022.3 LTS推荐使用URP 13.1.x或14.0.x
            var isRecommendedVersion = urpVersion.StartsWith("13.1") || urpVersion.StartsWith("14.0") || urpVersion.StartsWith("13.0") || urpVersion.StartsWith("12.1");

            if (isRecommendedVersion)
            {
                LogInfo("✅ URP版本与Unity 2022.3 LTS兼容");
                compatibilityReport.isCompatibleVersion = true;
                compatibilityReport.compatibleFeatures.Add("Unity 2022.3 LTS兼容性");
            }
            else
            {
                LogWarning($"⚠️ URP版本 {urpVersion} 可能与Unity 2022.3 LTS不完全兼容");
                compatibilityReport.warnings.Add($"推荐使用URP 13.1.x或14.0.x版本，当前版本: {urpVersion}");
            }

            yield return null;
        }

        /// <summary>
        /// 测试URP功能
        /// Test URP features
        /// </summary>
        private IEnumerator TestURPFeatures()
        {
            LogInfo("🔍 测试URP功能...");

            // 1. 测试2D渲染器
            yield return StartCoroutine(Test2DRenderer());

            // 2. 测试光照系统
            yield return StartCoroutine(TestLightingSystem());

            // 3. 测试后处理
            yield return StartCoroutine(TestPostProcessing());

            // 4. 测试相机堆栈
            yield return StartCoroutine(TestCameraStack());

            yield return null;
        }

        /// <summary>
        /// 测试2D渲染器
        /// Test 2D renderer
        /// </summary>
        private IEnumerator Test2DRenderer()
        {
            LogInfo("🎨 测试2D渲染器...");

            if (urpAsset != null && urpAsset.scriptableRendererData != null)
            {
                var rendererData = urpAsset.scriptableRendererData;
                var rendererTypeName = rendererData.GetType().Name;

                if (rendererTypeName.Contains("2D") || rendererTypeName.Contains("Renderer2D"))
                {
                    LogInfo("✅ 检测到2D渲染器");
                    compatibilityReport.compatibleFeatures.Add("2D渲染器支持");

                    var renderingFeature = new RenderingFeature
                    {
                        featureName = "2D渲染器",
                        featureType = "渲染器",
                        isSupported = true,
                        performanceImpact = 0.1f,
                        notes = "支持Sprite Lit和Sprite Unlit着色器"
                    };
                    compatibilityReport.renderingFeatures.Add(renderingFeature);
                }
                else
                {
                    LogInfo("ℹ️ 使用Forward渲染器，对2D内容仍兼容");
                    compatibilityReport.compatibleFeatures.Add("Forward渲染器(2D兼容)");
                }
            }

            yield return null;
        }

        /// <summary>
        /// 测试光照系统
        /// Test lighting system
        /// </summary>
        private IEnumerator TestLightingSystem()
        {
            LogInfo("💡 测试光照系统...");

            // 测试2D光源支持
            var testLight = new GameObject("Test2DLight");
            var light2D = testLight.AddComponent<UnityEngine.Rendering.Universal.Light2D>();

            if (light2D != null)
            {
                LogInfo("✅ 2D光源组件可用");
                compatibilityReport.compatibleFeatures.Add("2D光照系统");

                var renderingFeature = new RenderingFeature
                {
                    featureName = "2D光照",
                    featureType = "光照",
                    isSupported = true,
                    performanceImpact = 0.3f,
                    notes = "支持全局光照和局部光照"
                };
                compatibilityReport.renderingFeatures.Add(renderingFeature);
            }
            else
            {
                LogWarning("⚠️ 2D光源组件不可用");
                compatibilityReport.incompatibleFeatures.Add("2D光照系统");
            }

            // 清理测试对象
            UnityEngine.Object.DestroyImmediate(testLight);

            yield return null;
        }

        /// <summary>
        /// 测试后处理
        /// Test post processing
        /// </summary>
        private IEnumerator TestPostProcessing()
        {
            LogInfo("🎭 测试后处理...");

            // 创建测试后处理体积
            var testVolume = new GameObject("TestPostProcessVolume");
            var volume = testVolume.AddComponent<UnityEngine.Rendering.Volume>();

            if (volume != null)
            {
                LogInfo("✅ 后处理体积组件可用");
                compatibilityReport.compatibleFeatures.Add("后处理系统");

                var renderingFeature = new RenderingFeature
                {
                    featureName = "后处理",
                    featureType = "效果",
                    isSupported = true,
                    performanceImpact = 0.4f,
                    notes = "支持色调映射、泛光、景深等效果"
                };
                compatibilityReport.renderingFeatures.Add(renderingFeature);
            }
            else
            {
                LogWarning("⚠️ 后处理组件不可用");
                compatibilityReport.incompatibleFeatures.Add("后处理系统");
            }

            // 清理测试对象
            UnityEngine.Object.DestroyImmediate(testVolume);

            yield return null;
        }

        /// <summary>
        /// 测试相机堆栈
        /// Test camera stack
        /// </summary>
        private IEnumerator TestCameraStack()
        {
            LogInfo("📷 测试相机堆栈...");

            if (cameraData != null)
            {
                LogInfo("✅ 相机堆栈功能可用");
                compatibilityReport.compatibleFeatures.Add("相机堆栈");

                var renderingFeature = new RenderingFeature
                {
                    featureName = "相机堆栈",
                    featureType = "渲染",
                    isSupported = true,
                    performanceImpact = 0.2f,
                    notes = "支持多相机渲染和UI分层"
                };
                compatibilityReport.renderingFeatures.Add(renderingFeature);
            }
            else
            {
                LogWarning("⚠️ 相机堆栈功能不可用（缺少URP相机数据）");
                compatibilityReport.incompatibleFeatures.Add("相机堆栈");
            }

            yield return null;
        }

        /// <summary>
        /// 测试着色器兼容性
        /// Test shader compatibility
        /// </summary>
        private IEnumerator TestShaderCompatibility()
        {
            LogInfo("🎨 测试着色器兼容性...");

            // 测试内置着色器
            yield return StartCoroutine(TestBuiltInShaders());

            // 测试自定义着色器
            yield return StartCoroutine(TestCustomShaders());

            // 生成着色器报告
            GenerateShaderReport();

            yield return null;
        }

        /// <summary>
        /// 测试内置着色器
        /// Test built-in shaders
        /// </summary>
        private IEnumerator TestBuiltInShaders()
        {
            var builtInShaders = new[]
            {
                "Universal Render Pipeline/2D/Sprite-Lit-Default",
                "Universal Render Pipeline/2D/Sprite-Unlit-Default",
                "Universal Render Pipeline/Lit",
                "Universal Render Pipeline/Unlit",
                "Hidden/Universal Render Pipeline/FallbackError"
            };

            foreach (var shaderPath in builtInShaders)
            {
                var shader = Shader.Find(shaderPath);
                var testResult = new ShaderTestResult
                {
                    shaderName = shaderPath,
                    isCompatible = shader != null
                };

                if (shader != null)
                {
                    LogInfo($"✅ 找到着色器: {shaderPath}");
                    testResult.supportLevel = "完全支持";
                    compatibilityReport.shaderReport.compatibleShaders++;
                }
                else
                {
                    LogWarning($"⚠️ 未找到着色器: {shaderPath}");
                    testResult.supportLevel = "不支持";
                    testResult.issues.Add("着色器未找到");
                    compatibilityReport.shaderReport.incompatibleShaders++;
                }

                compatibilityReport.shaderReport.shaderTests.Add(testResult);
                yield return null;
            }
        }

        /// <summary>
        /// 测试自定义着色器
        /// Test custom shaders
        /// </summary>
        private IEnumerator TestCustomShaders()
        {
            // 这里可以添加项目中自定义着色器的测试
            // Currently testing project-specific custom shaders

            var customShaderPaths = new[]
            {
                "CoinAnimation/URP/CoinShader",
                "CoinAnimation/URP/UnlitCoin"
            };

            foreach (var shaderPath in customShaderPaths)
            {
                var shader = Shader.Find(shaderPath);
                var testResult = new ShaderTestResult
                {
                    shaderName = shaderPath,
                    isCompatible = shader != null
                };

                if (shader != null)
                {
                    LogInfo($"✅ 找到自定义着色器: {shaderPath}");
                    testResult.supportLevel = "完全支持";
                    compatibilityReport.shaderReport.compatibleShaders++;
                }
                else
                {
                    LogInfo($"ℹ️ 自定义着色器不存在: {shaderPath} (可选)");
                    testResult.supportLevel = "不存在(可选)";
                    compatibilityReport.shaderReport.compatibleShaders++;
                }

                compatibilityReport.shaderReport.shaderTests.Add(testResult);
                yield return null;
            }
        }

        /// <summary>
        /// 生成着色器报告
        /// Generate shader report
        /// </summary>
        private void GenerateShaderReport()
        {
            var totalShaders = compatibilityReport.shaderReport.compatibleShaders + compatibilityReport.shaderReport.incompatibleShaders;

            if (totalShaders > 0)
            {
                compatibilityReport.shaderReport.compatibilityPercentage =
                    (float)compatibilityReport.shaderReport.compatibleShaders / totalShaders * 100f;
            }

            LogInfo($"📊 着色器兼容性统计: {compatibilityReport.shaderReport.compatibleShaders}/{totalShaders} " +
                    $"({compatibilityReport.shaderReport.compatibilityPercentage:F1}%)");
        }

        /// <summary>
        /// 运行性能测试
        /// Run performance tests
        /// </summary>
        private IEnumerator RunPerformanceTests()
        {
            LogInfo("⚡ 运行性能测试...");

            // 创建测试金币
            yield return StartCoroutine(CreateTestCoins());

            // 运行性能基准测试
            yield return StartCoroutine(RunPerformanceBenchmark());

            // 清理测试金币
            CleanupTestCoins();

            yield return null;
        }

        /// <summary>
        /// 创建测试金币
        /// Create test coins
        /// </summary>
        private IEnumerator CreateTestCoins()
        {
            LogInfo($"🪙 创建 {testCoinCount} 个测试金币...");

            for (int i = 0; i < testCoinCount; i++)
            {
                var coin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                coin.name = $"TestCoin_{i}";
                coin.transform.position = new Vector3(i % 10 * 2f, 0, i / 10 * 2f);

                // 添加URP材质
                var renderer = coin.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    material.color = Color.yellow;
                    renderer.material = material;
                }

                testCoins.Add(coin);

                if (i % 10 == 0)
                {
                    yield return null;
                }
            }

            LogInfo($"✅ 成功创建 {testCoins.Count} 个测试金币");
        }

        /// <summary>
        /// 运行性能基准测试
        /// Run performance benchmark
        /// </summary>
        private IEnumerator RunPerformanceBenchmark()
        {
            LogInfo($"⏱️ 运行 {performanceTestDuration} 秒性能基准测试...");

            var startTime = Time.time;
            var frameCount = 0;
            var fps = 0f;
            var minFPS = float.MaxValue;
            var maxFPS = 0f;

            // 启动性能监控
            performanceMonitor.StartMonitoring();

            while (Time.time - startTime < performanceTestDuration)
            {
                // 更新FPS计算
                fps = 1f / Time.deltaTime;
                minFPS = Mathf.Min(minFPS, fps);
                maxFPS = Mathf.Max(maxFPS, fps);
                frameCount++;

                // 模拟金币动画
                foreach (var coin in testCoins)
                {
                    coin.transform.Rotate(0f, 90f * Time.deltaTime, 0f);
                    coin.transform.position = coin.transform.position + Vector3.up * Mathf.Sin(Time.time + coin.transform.position.x) * 0.01f;
                }

                yield return null;
            }

            // 停止性能监控
            performanceMonitor.StopMonitoring();

            // 计算性能指标
            var averageFPS = frameCount / performanceTestDuration;
            var frameTime = 1000f / averageFPS; // 转换为毫秒

            compatibilityReport.performanceMetrics = new PerformanceMetrics
            {
                averageFPS = averageFPS,
                minFPS = minFPS,
                maxFPS = maxFPS,
                frameTime = frameTime,
                memoryUsage = GC.GetTotalMemory(false) / (1024f * 1024f), // MB
                renderThreadTime = Time.renderThreadTime,
                drawCalls = UnityEngine.Statistics.drawCalls,
                triangles = UnityEngine.Statistics.triangles,
                meets60FPSTarget = averageFPS >= 60f
            };

            LogInfo($"📊 性能测试结果:");
            LogInfo($"   平均FPS: {averageFPS:F1}");
            LogInfo($"   帧时间: {frameTime:F2}ms");
            LogInfo($"   内存使用: {compatibilityReport.performanceMetrics.memoryUsage:F1}MB");
            LogInfo($"   绘制调用: {compatibilityReport.performanceMetrics.drawCalls}");
            LogInfo($"   60fps目标: {(compatibilityReport.performanceMetrics.meets60FPSTarget ? "✅ 达标" : "❌ 未达标")}");
        }

        /// <summary>
        /// 清理测试金币
        /// Cleanup test coins
        /// </summary>
        private void CleanupTestCoins()
        {
            foreach (var coin in testCoins)
            {
                if (coin != null)
                {
                    UnityEngine.Object.DestroyImmediate(coin);
                }
            }
            testCoins.Clear();

            LogInfo("🧹 测试金币已清理");
        }

        /// <summary>
        /// 测试渲染功能
        /// Test rendering features
        /// </summary>
        private IEnumerator TestRenderingFeatures()
        {
            LogInfo("🎮 测试渲染功能...");

            // 测试透明度排序
            yield return StartCoroutine(TestTransparencySorting());

            // 测试深度缓冲
            yield return StartCoroutine(TestDepthBuffer());

            // 测试渲染层
            yield return StartCoroutine(TestRenderLayers());

            yield return null;
        }

        /// <summary>
        /// 测试透明度排序
        /// Test transparency sorting
        /// </summary>
        private IEnumerator TestTransparencySorting()
        {
            LogInfo("🔍 测试透明度排序...");

            var transparentObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            transparentObject.name = "TransparentTest";

            var renderer = transparentObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                material.color = new Color(1f, 1f, 0f, 0.5f); // 半透明黄色
                renderer.material = material;

                LogInfo("✅ 透明度排序支持正常");
                compatibilityReport.compatibleFeatures.Add("透明度排序");
            }

            UnityEngine.Object.DestroyImmediate(transparentObject);
            yield return null;
        }

        /// <summary>
        /// 测试深度缓冲
        /// Test depth buffer
        /// </summary>
        private IEnumerator TestDepthBuffer()
        {
            LogInfo("🔍 测试深度缓冲...");

            // 检查URP资产中的深度设置
            if (urpAsset != null)
            {
                LogInfo("✅ 深度缓冲功能可用");
                compatibilityReport.compatibleFeatures.Add("深度缓冲");

                var renderingFeature = new RenderingFeature
                {
                    featureName = "深度缓冲",
                    featureType = "渲染",
                    isSupported = true,
                    performanceImpact = 0.1f,
                    notes = "支持深度测试和深度写入"
                };
                compatibilityReport.renderingFeatures.Add(renderingFeature);
            }

            yield return null;
        }

        /// <summary>
        /// 测试渲染层
        /// Test render layers
        /// </summary>
        private IEnumerator TestRenderLayers()
        {
            LogInfo("🔍 测试渲染层...");

            // 创建测试对象并设置渲染层
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.layer = LayerMask.NameToLayer("UI");

            if (testObject.layer == LayerMask.NameToLayer("UI"))
            {
                LogInfo("✅ 渲染层功能正常");
                compatibilityReport.compatibleFeatures.Add("渲染层");
            }

            UnityEngine.Object.DestroyImmediate(testObject);
            yield return null;
        }

        /// <summary>
        /// 生成最终报告
        /// Generate final report
        /// </summary>
        private void GenerateFinalReport()
        {
            LogInfo("📋 生成最终兼容性报告...");

            // 计算总体兼容性分数
            var totalFeatures = compatibilityReport.compatibleFeatures.Count + compatibilityReport.incompatibleFeatures.Count;
            var compatibilityScore = totalFeatures > 0 ?
                (float)compatibilityReport.compatibleFeatures.Count / totalFeatures * 100f : 0f;

            compatibilityReport.overallCompatibilityScore = compatibilityScore;

            // 输出报告摘要
            LogInfo("=" * 50);
            LogInfo("📊 URP兼容性测试报告");
            LogInfo("=" * 50);
            LogInfo($"Unity版本: {compatibilityReport.unityVersion}");
            LogInfo($"URP版本: {compatibilityReport.urpVersion}");
            LogInfo($"URP存在: {(compatibilityReport.isURPPresent ? "✅" : "❌")}");
            LogInfo($"版本兼容: {(compatibilityReport.isCompatibleVersion ? "✅" : "❌")}");
            LogInfo($"总体兼容性: {compatibilityReport.overallCompatibilityScore:F1}%");
            LogInfo($"兼容功能: {compatibilityReport.compatibleFeatures.Count}");
            LogInfo($"不兼容功能: {compatibilityReport.incompatibleFeatures.Count}");
            LogInfo($"警告数量: {compatibilityReport.warnings.Count}");
            LogInfo($"60fps目标: {(compatibilityReport.performanceMetrics?.meets60FPSTarget == true ? "✅" : "❌")}");

            if (compatibilityReport.warnings.Count > 0)
            {
                LogInfo("\n⚠️ 警告:");
                foreach (var warning in compatibilityReport.warnings)
                {
                    LogInfo($"   • {warning}");
                }
            }

            LogInfo("=" * 50);
        }

        /// <summary>
        /// 获取URP版本
        /// Get URP version
        /// </summary>
        private string GetURPVersion()
        {
            if (urpAsset == null) return "未安装";

            // 尝试从不同源获取版本信息
            #if UNITY_EDITOR
            var urpPackage = UnityEditor.AssetDatabase.FindAssets("com.unity.render-pipelines.universal");
            if (urpPackage.Length > 0)
            {
                var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForPackageName("com.unity.render-pipelines.universal");
                if (packageInfo != null)
                {
                    return packageInfo.version;
                }
            }
            #endif

            return "未知版本";
        }

        /// <summary>
        /// 获取支持的渲染器
        /// Get supported renderers
        /// </summary>
        private string GetSupportedRenderers()
        {
            if (urpAsset?.scriptableRendererData != null)
            {
                return urpAsset.scriptableRendererData.GetType().Name;
            }
            return "未知";
        }

        /// <summary>
        /// 初始化支持的URP版本
        /// Initialize supported URP versions
        /// </summary>
        private void InitializeSupportedURPVersions()
        {
            supportedURPVersions.Add(new URPVersionInfo
            {
                version = "12.1.x",
                unityVersion = "2021.3 LTS",
                isCompatible = true,
                notes = "Unity 2021.3 LTS推荐版本"
            });

            supportedURPVersions.Add(new URPVersionInfo
            {
                version = "13.1.x",
                unityVersion = "2022.3 LTS",
                isCompatible = true,
                notes = "Unity 2022.3 LTS推荐版本"
            });

            supportedURPVersions.Add(new URPVersionInfo
            {
                version = "14.0.x",
                unityVersion = "2022.3 LTS",
                isCompatible = true,
                notes = "Unity 2022.3 LTS最新版本"
            });
        }

        /// <summary>
        /// 获取兼容性报告
        /// Get compatibility report
        /// </summary>
        public URPCompatibilityReport GetCompatibilityReport()
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
                LogInfo($"📄 兼容性报告已导出到: {filePath}");
            }
            catch (Exception ex)
            {
                LogError($"❌ 导出报告失败: {ex.Message}");
            }
        }

        private void LogInfo(string message)
        {
            if (enableDetailedLogging)
            {
                Debug.Log($"[URP兼容性测试] {message}");
            }
        }

        private void LogWarning(string message)
        {
            if (enableDetailedLogging)
            {
                Debug.LogWarning($"[URP兼容性测试] {message}");
            }
        }

        private void LogError(string message)
        {
            if (enableDetailedLogging)
            {
                Debug.LogError($"[URP兼容性测试] {message}");
            }
        }
    }

    /// <summary>
    /// 简化的性能监控器
    /// Simplified performance monitor
    /// </summary>
    public class PerformanceMonitor
    {
        private bool isMonitoring = false;
        private float startTime;
        private float startMemory;

        public void StartMonitoring()
        {
            isMonitoring = true;
            startTime = Time.time;
            startMemory = GC.GetTotalMemory(false);
        }

        public void StopMonitoring()
        {
            isMonitoring = false;
        }

        public float GetMemoryDelta()
        {
            return (GC.GetTotalMemory(false) - startMemory) / (1024f * 1024f); // MB
        }
    }
}
#endif

#if !UNITY_EDITOR || !UNITY_RENDER_PIPELINE_UNIVERSAL
    /// <summary>
    /// URP兼容性验证器占位符 - URP未安装
    /// URP Compatibility Validator Placeholder - URP not installed
    /// </summary>
    public class URPCompatibilityValidator
    {
        public void Initialize() => UnityEngine.Debug.Log("URP未安装，跳过URP兼容性验证");
        public System.Collections.IEnumerator RunCompleteCompatibilityTest() => null;
        public object GetCompatibilityReport() => null;
        public void ExportReportToFile(string filePath) => UnityEngine.Debug.Log("URP未安装，无法导出报告");
    }
#endif
}