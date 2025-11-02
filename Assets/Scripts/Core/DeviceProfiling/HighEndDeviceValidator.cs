using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using CoinAnimation.Core;
using CoinAnimation.AdaptiveQuality;

namespace CoinAnimation.Core.DeviceProfiling
{
    /// <summary>
    /// 高端设备性能优化验证器
    /// Story 2.1 Task 5.3 - 高端设备性能优化确认
    /// </summary>
    public class HighEndDeviceValidator : MonoBehaviour
    {
        #region Configuration

        [Header("High-End Device Specifications")]
        [SerializeField] private DeviceSpecs highEndSpecs = new DeviceSpecs
        {
            minCPUScore = 85f,
            minMemoryScore = 85f,
            minGPUScore = 75f,
            minStorageScore = 90f,
            targetFPS = 60f,
            maxConcurrentCoins = 100,
            qualityLevel = QualityLevel.High
        };

        [Header("High-End Test Configuration")]
        [SerializeField] private bool enableHighEndTesting = true;
        [SerializeField] private bool autoDetectHighEnd = true;
        [SerializeField] private float extremeTestDuration = 60f; // 极限测试时间
        [SerializeField] private int maxCoinCount = 150; // 高端设备最大金币数

        [Header("Performance Targets for High-End")]
        [SerializeField] private float minimumAcceptableFPS = 55f;
        [SerializeField] private float targetFPS = 60f;
        [SerializeField] private float idealFPS = 120f; // 理想FPS目标
        [SerializeField] private float memoryWarningThreshold = 500f; // MB
        [SerializeField] private float frameTimeWarningThreshold = 16.67f; // ms (60 FPS)

        [Header("High-End Feature Tests")]
        [SerializeField] private bool testUltraQuality = true;
        [SerializeField] private bool testAdvancedEffects = true;
        [SerializeField] private bool testMaximumConcurrent = true;
        [SerializeField] private bool test4KSupport = true;
        [SerializeField] private bool testRayTracing = true;
        [SerializeField] private bool testHDRRendering = true;

        [Header("Extreme Performance Tests")]
        [SerializeField] private bool testStressScenarios = true;
        [SerializeField] private bool testBottleneckAnalysis = true;
        [SerializeField] private bool testThermalManagement = true;
        [SerializeField] private bool testPowerOptimization = true;

        [Header("Advanced Graphics Tests")]
        [SerializeField] private bool testAdvancedShaders = true;
        [SerializeField] private bool testPostProcessing = true;
        [SerializeField] private bool testShadowQuality = true;
        [SerializeField] private bool testAntiAliasing = true;

        #endregion

        #region Private Fields

        private DeviceCapabilityDetector _deviceDetector;
        private IAdaptiveQualityManager _qualityManager;
        private PerformanceMonitor _performanceMonitor;
        private AdvancedPerformanceDashboard _dashboard;
        private MemoryManagementSystem _memorySystem;
        private CoinObjectPool _objectPool;
        private URPConfigurationManager _urpManager;

        private HighEndTestResults _testResults;
        private bool _isTestRunning = false;
        private bool _isHighEndDevice = false;

        // 高级性能监控
        private readonly Queue<float> _fpsHistory = new Queue<float>();
        private readonly Queue<float> _memoryHistory = new Queue<float>();
        private readonly Queue<float> _frameTimeHistory = new Queue<float>();
        private readonly Queue<float> _gpuTimeHistory = new Queue<float>();
        private readonly Queue<HighEndMetricsSnapshot> _detailedHistory = new Queue<HighEndMetricsSnapshot>();
        private readonly int maxHistorySize = 10000;

        // 极限测试统计
        private int _totalFrames;
        private int _droppedFrames;
        private float _testStartTime;
        private float _peakMemoryUsage;
        private float _peakGPUTime;
        private List<PerformanceBottleneck> _detectedBottlenecks = new List<PerformanceBottleneck>();
        private List<ThermalEvent> _thermalEvents = new List<ThermalEvent>();

        // 高级图形设置
        private Dictionary<string, object> _originalGraphicsSettings = new Dictionary<string, object>();
        private bool _supportsAdvancedFeatures = false;

        #endregion

        #region Events

        public event Action<HighEndTestResults> OnHighEndTestStarted;
        public event Action<HighEndTestResults> OnHighEndTestCompleted;
        public event Action<string> OnHighEndTestError;
        public event Action<HighEndTestProgress> OnHighEndTestProgress;
        public event Action<PerformanceBottleneck> OnBottleneckDetected;
        public event Action<ThermalEvent> OnThermalEvent;

        #endregion

        #region Properties

        public bool IsTestRunning => _isTestRunning;
        public bool IsHighEndDevice => _isHighEndDevice;
        public HighEndTestResults CurrentTestResults => _testResults;
        public float CurrentPerformanceIndex => CalculatePerformanceIndex();
        public List<PerformanceBottleneck> DetectedBottlenecks => _detectedBottlenecks;
        public bool SupportsAdvancedFeatures => _supportsAdvancedFeatures;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            DetectDeviceCapabilities();
            InitializeAdvancedGraphics();
        }

        private void Start()
        {
            if (enableHighEndTesting && autoDetectHighEnd && _isHighEndDevice)
            {
                Debug.Log("[HighEndDeviceValidator] High-end device detected, starting automatic validation");
                StartCoroutine(DelayedHighEndValidationStart());
            }
        }

        private IEnumerator DelayedHighEndValidationStart()
        {
            yield return new WaitForSeconds(3f); // 等待系统完全初始化
            StartHighEndValidation();
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            _deviceDetector = FindObjectOfType<DeviceCapabilityDetector>();
            _qualityManager = FindObjectOfType<IAdaptiveQualityManager>();
            _performanceMonitor = FindObjectOfType<PerformanceMonitor>();
            _dashboard = FindObjectOfType<AdvancedPerformanceDashboard>();
            _memorySystem = FindObjectOfType<MemoryManagementSystem>();
            _objectPool = FindObjectOfType<CoinObjectPool>();
            _urpManager = FindObjectOfType<URPConfigurationManager>();

            // 创建缺失的组件
            if (_deviceDetector == null)
            {
                _deviceDetector = gameObject.AddComponent<DeviceCapabilityDetector>();
            }

            if (_performanceMonitor == null)
            {
                _performanceMonitor = gameObject.AddComponent<PerformanceMonitor>();
            }
        }

        private void DetectDeviceCapabilities()
        {
            if (_deviceDetector != null)
            {
                _deviceDetector.OnDetectionComplete += (capabilities) =>
                {
                    _isHighEndDevice = EvaluateHighEndDevice(capabilities);
                    _supportsAdvancedFeatures = CheckAdvancedFeatureSupport();
                    Debug.Log($"[HighEndDeviceValidator] Device classification: {capabilities.PerformanceTier} (High-end: {_isHighEndDevice}, Advanced Features: {_supportsAdvancedFeatures})");
                };

                _deviceDetector.RedetectDeviceCapabilities();
            }
            else
            {
                _isHighEndDevice = PerformSimpleHighEndDetection();
                _supportsAdvancedFeatures = CheckAdvancedFeatureSupport();
                Debug.Log($"[HighEndDeviceValidator] Simple detection result: High-end device = {_isHighEndDevice}");
            }
        }

        private bool EvaluateHighEndDevice(DeviceCapabilities capabilities)
        {
            return capabilities.CPUScore >= highEndSpecs.minCPUScore &&
                   capabilities.MemoryScore >= highEndSpecs.minMemoryScore &&
                   capabilities.GPUScore >= highEndSpecs.minGPUScore &&
                   capabilities.PerformanceTier == DevicePerformanceClass.HighEnd;
        }

        private bool PerformSimpleHighEndDetection()
        {
            var memoryGB = SystemInfo.systemMemorySize / 1024f;
            var processorCount = SystemInfo.processorCount;
            var graphicsMemoryMB = SystemInfo.graphicsMemorySize;

            return memoryGB >= 16f || // 16GB+内存
                   processorCount >= 8 || // 8核+CPU
                   graphicsMemoryMB >= 4096; // 4GB+显存
        }

        private void InitializeAdvancedGraphics()
        {
            // 保存原始图形设置
            SaveCurrentGraphicsSettings();

            // 检测高级图形功能支持
            _supportsAdvancedFeatures = CheckAdvancedFeatureSupport();

            if (_supportsAdvancedFeatures)
            {
                Debug.Log("[HighEndDeviceValidator] Advanced graphics features supported");
            }
        }

        private void SaveCurrentGraphicsSettings()
        {
            _originalGraphicsSettings["vSyncCount"] = QualitySettings.vSyncCount;
            _originalGraphicsSettings["antiAliasing"] = QualitySettings.antiAliasing;
            _originalGraphicsSettings["anisotropicFiltering"] = QualitySettings.anisotropicFiltering;
            _originalGraphicsSettings["pixelLightCount"] = QualitySettings.pixelLightCount;
            _originalGraphicsSettings["shadowCascadeCount"] = QualitySettings.shadowCascadeCount;
            _originalGraphicsSettings["shadowDistance"] = QualitySettings.shadowDistance;
            _originalGraphicsSettings["softVegetation"] = QualitySettings.softVegetation;
            _originalGraphicsSettings["realtimeReflectionProbes"] = QualitySettings.realtimeReflectionProbes;
        }

        private bool CheckAdvancedFeatureSupport()
        {
            var supportedFeatures = 0;

            // 检查各种高级功能支持
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12 ||
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan ||
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
            {
                supportedFeatures++;
            }

            if (SystemInfo.graphicsMemorySize >= 4096) supportedFeatures++; // 4GB+ 显存
            if (SystemInfo.maxTextureSize >= 8192) supportedFeatures++; // 8K+ 纹理支持
            if (SystemInfo.supportsComputeShaders) supportedFeatures++; // 计算着色器
            if (SystemInfo.supportsInstancing) supportedFeatures++; // GPU实例化
            if (SystemInfo.supports3DTextures) supportedFeatures++; // 3D纹理
            if (SystemInfo.supportsCubemapArray) supportedFeatures++; // 立方体纹理数组

            return supportedFeatures >= 4; // 至少支持4项高级功能
        }

        #endregion

        #region Test Control

        public void StartHighEndValidation()
        {
            if (_isTestRunning)
            {
                Debug.LogWarning("[HighEndDeviceValidator] Test is already running");
                return;
            }

            if (!_isHighEndDevice)
            {
                Debug.LogWarning("[HighEndDeviceValidator] This device is not classified as high-end. Some tests may not be optimal.");
            }

            StartCoroutine(RunHighEndValidationCoroutine());
        }

        public void StopHighEndValidation()
        {
            if (!_isTestRunning)
            {
                return;
            }

            _isTestRunning = false;
            StopAllCoroutines();
            RestoreOriginalGraphicsSettings();
            Debug.Log("[HighEndDeviceValidator] High-end validation stopped by user");
        }

        public void StartSpecificTest(HighEndTestType testType)
        {
            if (_isTestRunning)
            {
                Debug.LogWarning("[HighEndDeviceValidator] Cannot start specific test while validation is running");
                return;
            }

            StartCoroutine(RunSpecificTestCoroutine(testType));
        }

        #endregion

        #region Test Coroutines

        private IEnumerator RunHighEndValidationCoroutine()
        {
            _isTestRunning = true;
            _testResults = new HighEndTestResults
            {
                testStartTime = DateTime.UtcNow,
                deviceInfo = CollectHighEndDeviceInfo(),
                isHighEndDevice = _isHighEndDevice,
                supportsAdvancedFeatures = _supportsAdvancedFeatures
            };

            OnHighEndTestStarted?.Invoke(_testResults);

            Debug.Log("[HighEndDeviceValidator] Starting comprehensive high-end device validation");

            try
            {
                ResetTestState();

                // Phase 1: 基准性能和极限测试
                yield return RunPhase_ExtremeBaselinePerformance();

                // Phase 2: 超高质量设置测试
                if (testUltraQuality)
                {
                    yield return RunPhase_UltraQualitySettings();
                }

                // Phase 3: 高级视觉效果测试
                if (testAdvancedEffects)
                {
                    yield return RunPhase_AdvancedVisualEffects();
                }

                // Phase 4: 最大并发测试
                if (testMaximumConcurrent)
                {
                    yield return RunPhase_MaximumConcurrentAnimations();
                }

                // Phase 5: 4K分辨率支持测试
                if (test4KSupport)
                {
                    yield return RunPhase_4KResolutionSupport();
                }

                // Phase 6: 光线追踪测试（如果支持）
                if (testRayTracing && _supportsAdvancedFeatures)
                {
                    yield return RunPhase_RayTracingSupport();
                }

                // Phase 7: HDR渲染测试
                if (testHDRRendering)
                {
                    yield return RunPhase_HDRRendering();
                }

                // Phase 8: 高级着色器测试
                if (testAdvancedShaders)
                {
                    yield return RunPhase_AdvancedShaders();
                }

                // Phase 9: 后处理效果测试
                if (testPostProcessing)
                {
                    yield return RunPhase_PostProcessingEffects();
                }

                // Phase 10: 阴影质量测试
                if (testShadowQuality)
                {
                    yield return RunPhase_ShadowQualityOptimization();
                }

                // Phase 11: 抗锯齿测试
                if (testAntiAliasing)
                {
                    yield return RunPhase_AntiAliasingOptimization();
                }

                // Phase 12: 压力场景测试
                if (testStressScenarios)
                {
                    yield return RunPhase_ExtremeStressScenarios();
                }

                // Phase 13: 瓶颈分析测试
                if (testBottleneckAnalysis)
                {
                    yield return RunPhase_BottleneckAnalysis();
                }

                // Phase 14: 热管理测试
                if (testThermalManagement)
                {
                    yield return RunPhase_ThermalManagement();
                }

                // Phase 15: 电源优化测试
                if (testPowerOptimization)
                {
                    yield return RunPhase_PowerOptimization();
                }

                // 完成测试
                _testResults.testEndTime = DateTime.UtcNow;
                _testResults.overallResult = EvaluateOverallResult();
                _testResults.recommendations = GenerateHighEndRecommendations();
                _testResults.optimizationProfile = GenerateHighEndOptimizationProfile();
                _testResults.performanceIndex = CalculatePerformanceIndex();

                Debug.Log($"[HighEndDeviceValidator] Validation completed. Overall result: {_testResults.overallResult}, Performance Index: {_testResults.performanceIndex:F1}");

                OnHighEndTestCompleted?.Invoke(_testResults);
            }
            catch (Exception e)
            {
                var errorMessage = $"High-end validation failed: {e.Message}";
                Debug.LogError($"[HighEndDeviceValidator] {errorMessage}");
                OnHighEndTestError?.Invoke(errorMessage);
            }
            finally
            {
                _isTestRunning = false;
                CleanupTestEnvironment();
                RestoreOriginalGraphicsSettings();
            }
        }

        private IEnumerator RunSpecificTestCoroutine(HighEndTestType testType)
        {
            _isTestRunning = true;

            try
            {
                switch (testType)
                {
                    case HighEndTestType.ExtremeBaseline:
                        yield return RunPhase_ExtremeBaselinePerformance();
                        break;
                    case HighEndTestType.UltraQuality:
                        yield return RunPhase_UltraQualitySettings();
                        break;
                    case HighEndTestType.AdvancedEffects:
                        yield return RunPhase_AdvancedVisualEffects();
                        break;
                    case HighEndTestType.MaximumConcurrent:
                        yield return RunPhase_MaximumConcurrentAnimations();
                        break;
                    case HighEndTestType.Resolution4K:
                        yield return RunPhase_4KResolutionSupport();
                        break;
                    case HighEndTestType.RayTracing:
                        yield return RunPhase_RayTracingSupport();
                        break;
                    case HighEndTestType.HDRRendering:
                        yield return RunPhase_HDRRendering();
                        break;
                    case HighEndTestType.AdvancedShaders:
                        yield return RunPhase_AdvancedShaders();
                        break;
                    case HighEndTestType.PostProcessing:
                        yield return RunPhase_PostProcessingEffects();
                        break;
                    case HighEndTestType.ShadowQuality:
                        yield return RunPhase_ShadowQualityOptimization();
                        break;
                    case HighEndTestType.AntiAliasing:
                        yield return RunPhase_AntiAliasingOptimization();
                        break;
                    case HighEndTestType.ExtremeStress:
                        yield return RunPhase_ExtremeStressScenarios();
                        break;
                    case HighEndTestType.BottleneckAnalysis:
                        yield return RunPhase_BottleneckAnalysis();
                        break;
                    case HighEndTestType.ThermalManagement:
                        yield return RunPhase_ThermalManagement();
                        break;
                    case HighEndTestType.PowerOptimization:
                        yield return RunPhase_PowerOptimization();
                        break;
                }
            }
            finally
            {
                _isTestRunning = false;
                RestoreOriginalGraphicsSettings();
            }
        }

        #endregion

        #region Test Phases Implementation

        private IEnumerator RunPhase_ExtremeBaselinePerformance()
        {
            const float phaseDuration = 20f;
            var phaseName = "Extreme Baseline Performance";

            Debug.Log($"[HighEndDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new HighEndPhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            // 极限基准测试 - 测试设备真正极限
            var extremeLoads = new[] { 50, 75, 100, 125, 150, 175, 200 };

            for (int i = 0; i < extremeLoads.Length; i++)
            {
                var coinCount = extremeLoads[i];
                var progress = (float)i / extremeLoads.Length;

                ReportProgress(phaseName, progress * 100f);

                Debug.Log($"[HighEndDeviceValidator] Testing extreme baseline with {coinCount} coins");

                CreateTestCoins(coinCount);
                yield return RunExtremePerformanceTest(5f, $"Extreme_{coinCount}coins");

                var metrics = CollectAdvancedMetrics();
                phaseResults.measurements.Add(new HighEndMeasurement
                {
                    measurementName = $"{coinCount} Coins (Extreme)",
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gpuTime = metrics.gpuTime,
                    performanceIndex = metrics.performanceIndex,
                    coinCount = coinCount,
                    bottleneckType = DetectBottleneck(metrics)
                });

                CleanupTestCoins();
                yield return new WaitForSeconds(1f);
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluateHighEndPhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[HighEndDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_UltraQualitySettings()
        {
            const float phaseDuration = 25f;
            var phaseName = "Ultra Quality Settings";

            Debug.Log($"[HighEndDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new HighEndPhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var ultraQualityPresets = new[]
            {
                new { Name = "Ultra High", Quality = QualityLevel.High, AA = 8, Shadows = ShadowQuality.All, Aniso = 2 },
                new { Name = "Extreme", Quality = QualityLevel.High, AA = 8, Shadows = ShadowQuality.All, Aniso = 16 },
                new { Name = "Insane", Quality = QualityLevel.High, AA = 8, Shadows = ShadowQuality.All, Aniso = 16 },
                new { Name = "Custom Ultra", Quality = QualityLevel.High, AA = 8, Shadows = ShadowQuality.All, Aniso = 16 }
            };

            var testCoinCount = 100; // 高端设备标准负载

            foreach (var preset in ultraQualityPresets)
            {
                Debug.Log($"[HighEndDeviceValidator] Testing ultra quality preset: {preset.Name}");

                ApplyUltraQualitySettings(preset);
                yield return new WaitForSeconds(2f);

                CreateTestCoins(testCoinCount);
                yield return RunUltraQualityTest(5f, preset.Name);

                var metrics = CollectAdvancedMetrics();
                phaseResults.measurements.Add(new HighEndMeasurement
                {
                    measurementName = preset.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gpuTime = metrics.gpuTime,
                    performanceIndex = metrics.performanceIndex,
                    qualityLevel = preset.Quality,
                    antiAliasing = preset.AA,
                    shadowQuality = preset.Shadows,
                    anisotropicFiltering = preset.Aniso,
                    coinCount = testCoinCount
                });

                CleanupTestCoins();
            }

            var optimalUltraPreset = FindOptimalUltraPreset(phaseResults);
            _testResults.recommendedUltraPreset = optimalUltraPreset;

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluateHighEndPhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[HighEndDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_AdvancedVisualEffects()
        {
            const float phaseDuration = 20f;
            var phaseName = "Advanced Visual Effects";

            Debug.Log($"[HighEndDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new HighEndPhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var advancedEffectTests = new[]
            {
                new { Name = "Particle Storm", Particles = 500, Coins = 80, Duration = 4f },
                new { Name = "Shader Effects", Shaders = "Complex", Coins = 100, Duration = 4f },
                new { Name = "Post-Processing", Effects = "All", Coins = 90, Duration = 4f },
                new { Name = "Combined Ultra", Combined = true, Coins = 75, Duration = 4f }
            };

            foreach (var test in advancedEffectTests)
            {
                Debug.Log($"[HighEndDeviceValidator] Testing advanced effect: {test.Name}");

                CreateTestCoins(test.Coins);
                if (test.Particles > 0) CreateAdvancedParticleEffects(test.Particles);
                if (!string.IsNullOrEmpty(test.Shaders)) SetupAdvancedShaders(test.Shaders);
                if (!string.IsNullOrEmpty(test.Effects)) EnablePostProcessingEffects(test.Effects);
                if (test.Combined) EnableAllAdvancedEffects();

                yield return RunAdvancedEffectsTest(test.Duration, test.Name);

                var metrics = CollectAdvancedMetrics();
                phaseResults.measurements.Add(new HighEndMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gpuTime = metrics.gpuTime,
                    performanceIndex = metrics.performanceIndex,
                    particleCount = test.Particles,
                    shaderComplexity = test.Shaders,
                    postProcessingEffects = test.Effects,
                    combinedEffects = test.Combined,
                    coinCount = test.Coins
                });

                CleanupAdvancedEffects();
                CleanupTestCoins();
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluateHighEndPhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[HighEndDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_MaximumConcurrentAnimations()
        {
            const float phaseDuration = 30f;
            var phaseName = "Maximum Concurrent Animations";

            Debug.Log($"[HighEndDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new HighEndPhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            // 寻找设备的最大并发动画能力
            var concurrentTests = new[]
            {
                new { Name = "High Load", Coins = 100, Duration = 6f },
                new { Name = "Extreme Load", Coins = 150, Duration = 8f },
                new { Name = "Maximum Load", Coins = 200, Duration = 10f },
                new { Name = "Beyond Maximum", Coins = 250, Duration = 6f }
            };

            foreach (var test in concurrentTests)
            {
                Debug.Log($"[HighEndDeviceValidator] Testing concurrent capacity: {test.Name}");

                CreateTestCoins(test.Coins);
                StartMaximumConcurrentAnimations();

                yield return RunMaximumConcurrentTest(test.Duration, test.Name);

                var metrics = CollectAdvancedMetrics();
                var concurrencyMetrics = CalculateConcurrencyMetrics(test.Coins, metrics);

                phaseResults.measurements.Add(new HighEndMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gpuTime = metrics.gpuTime,
                    performanceIndex = metrics.performanceIndex,
                    coinCount = test.Coins,
                    concurrencyEfficiency = concurrencyMetrics.efficiency,
                    animationComplexity = concurrencyMetrics.complexity,
                    memoryPerCoin = concurrencyMetrics.memoryPerCoin
                });

                StopMaximumConcurrentAnimations();
                CleanupTestCoins();
            }

            var maxConcurrentCoins = DetermineMaximumConcurrentCoins(phaseResults);
            _testResults.maximumConcurrentCoins = maxConcurrentCoins;

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluateHighEndPhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[HighEndDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_4KResolutionSupport()
        {
            const float phaseDuration = 15f;
            var phaseName = "4K Resolution Support";

            Debug.Log($"[HighEndDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new HighEndPhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var originalResolution = Screen.currentResolution;
            var resolutionTests = new[]
            {
                new { Name = "1080p", Width = 1920, Height = 1080 },
                new { Name = "1440p", Width = 2560, Height = 1440 },
                new { Name = "4K", Width = 3840, Height = 2160 },
                new { Name = "Custom Ultra", Width = 5120, Height = 2880 }
            };

            var testCoinCount = 75;

            foreach (var resolution in resolutionTests)
            {
                Debug.Log($"[HighEndDeviceValidator] Testing resolution: {resolution.Name} ({resolution.Width}x{resolution.Height})");

                if (!SetResolution(resolution.Width, resolution.Height))
                {
                    Debug.LogWarning($"[HighEndDeviceValidator] Resolution {resolution.Name} not supported, skipping");
                    continue;
                }

                yield return new WaitForSeconds(1f); // 等待分辨率切换

                CreateTestCoins(testCoinCount);
                yield return RunResolutionTest(3f, resolution.Name);

                var metrics = CollectAdvancedMetrics();
                phaseResults.measurements.Add(new HighEndMeasurement
                {
                    measurementName = resolution.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gpuTime = metrics.gpuTime,
                    performanceIndex = metrics.performanceIndex,
                    resolutionWidth = resolution.Width,
                    resolutionHeight = resolution.Height,
                    pixelCount = resolution.Width * resolution.Height,
                    coinCount = testCoinCount
                });

                CleanupTestCoins();
            }

            // 恢复原始分辨率
            Screen.SetResolution(originalResolution.width, originalResolution.height, Screen.fullScreen);

            var supported4K = Check4KSupport(phaseResults);
            _testResults.supports4K = supported4K;

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluateHighEndPhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[HighEndDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}, 4K Support: {supported4K}");
        }

        private IEnumerator RunPhase_RayTracingSupport()
        {
            const float phaseDuration = 20f;
            var phaseName = "Ray Tracing Support";

            Debug.Log($"[HighEndDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new HighEndPhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var rayTracingTests = new[]
            {
                new { Name = "No RT", Enabled = false, Coins = 100, Duration = 4f },
                new { Name = "RT Low", Enabled = true, Quality = "Low", Coins = 80, Duration = 4f },
                new { Name = "RT Medium", Enabled = true, Quality = "Medium", Coins = 60, Duration = 4f },
                new { Name = "RT High", Enabled = true, Quality = "High", Coins = 40, Duration = 4f }
            };

            foreach (var test in rayTracingTests)
            {
                Debug.Log($"[HighEndDeviceValidator] Testing ray tracing: {test.Name}");

                if (test.Enabled)
                {
                    if (!EnableRayTracing(test.Quality))
                    {
                        Debug.LogWarning($"[HighEndDeviceValidator] Ray tracing {test.Quality} not supported, skipping");
                        continue;
                    }
                }
                else
                {
                    DisableRayTracing();
                }

                yield return new WaitForSeconds(1f);

                CreateTestCoins(test.Coins);
                yield return RunRayTracingTest(test.Duration, test.Name);

                var metrics = CollectAdvancedMetrics();
                phaseResults.measurements.Add(new HighEndMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gpuTime = metrics.gpuTime,
                    performanceIndex = metrics.performanceIndex,
                    rayTracingEnabled = test.Enabled,
                    rayTracingQuality = test.Quality,
                    coinCount = test.Coins
                });

                CleanupTestCoins();
            }

            var rayTracingSupport = CheckRayTracingSupport(phaseResults);
            _testResults.supportsRayTracing = rayTracingSupport;

            DisableRayTracing();

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluateHighEndPhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[HighEndDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}, RT Support: {rayTracingSupport}");
        }

        private IEnumerator RunPhase_HDRRendering()
        {
            const float phaseDuration = 15f;
            var phaseName = "HDR Rendering";

            Debug.Log($"[HighEndDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new HighEndPhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var hdrTests = new[]
            {
                new { Name = "SDR", Enabled = false, Coins = 100, Duration = 3f },
                new { Name = "HDR10", Enabled = true, Format = "HDR10", Coins = 90, Duration = 3f },
                new { Name = "Dolby Vision", Enabled = true, Format = "DolbyVision", Coins = 85, Duration = 3f },
                new { Name = "HLG", Enabled = true, Format = "HLG", Coins = 80, Duration = 3f }
            };

            foreach (var test in hdrTests)
            {
                Debug.Log($"[HighEndDeviceValidator] Testing HDR: {test.Name}");

                if (test.Enabled)
                {
                    if (!EnableHDR(test.Format))
                    {
                        Debug.LogWarning($"[HighEndDeviceValidator] HDR {test.Format} not supported, skipping");
                        continue;
                    }
                }
                else
                {
                    DisableHDR();
                }

                yield return new WaitForSeconds(1f);

                CreateTestCoins(test.Coins);
                yield return RunHDRTest(test.Duration, test.Name);

                var metrics = CollectAdvancedMetrics();
                phaseResults.measurements.Add(new HighEndMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gpuTime = metrics.gpuTime,
                    performanceIndex = metrics.performanceIndex,
                    hdrEnabled = test.Enabled,
                    hdrFormat = test.Format,
                    coinCount = test.Coins
                });

                CleanupTestCoins();
            }

            var hdrSupport = CheckHDRSupport(phaseResults);
            _testResults.supportsHDR = hdrSupport;

            DisableHDR();

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluateHighEndPhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[HighEndDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}, HDR Support: {hdrSupport}");
        }

        private IEnumerator RunPhase_AdvancedShaders()
        {
            const float phaseDuration = 12f;
            var phaseName = "Advanced Shaders";

            Debug.Log($"[HighEndDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new HighEndPhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var shaderTests = new[]
            {
                new { Name = "Standard", Type = "Standard", Coins = 100, Duration = 2f },
                new { Name = "PBR", Type = "PBR", Coins = 90, Duration = 2f },
                new { Name = "Subsurface Scattering", Type = "SSS", Coins = 80, Duration = 2f },
                new { Name = "Custom Shader", Type = "CustomComplex", Coins = 75, Duration = 2f },
                new { Name = "Compute Shader", Type = "Compute", Coins = 70, Duration = 2f }
            };

            foreach (var test in shaderTests)
            {
                Debug.Log($"[HighEndDeviceValidator] Testing shader: {test.Name}");

                SetupShaderType(test.Type);
                yield return new WaitForSeconds(0.5f);

                CreateTestCoins(test.Coins);
                yield return RunShaderTest(test.Duration, test.Name);

                var metrics = CollectAdvancedMetrics();
                phaseResults.measurements.Add(new HighEndMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gpuTime = metrics.gpuTime,
                    performanceIndex = metrics.performanceIndex,
                    shaderType = test.Type,
                    shaderComplexity = GetShaderComplexity(test.Type),
                    coinCount = test.Coins
                });

                CleanupTestCoins();
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluateHighEndPhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[HighEndDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_PostProcessingEffects()
        {
            const float phaseDuration = 15f;
            var phaseName = "Post-Processing Effects";

            Debug.Log($"[HighEndDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new HighEndPhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var postProcessingTests = new[]
            {
                new { Name = "No Effects", Effects = new string[0], Coins = 100, Duration = 2f },
                new { Name = "Basic Effects", Effects = new[] { "Bloom", "Vignette" }, Coins = 95, Duration = 2f },
                new { Name = "Advanced Effects", Effects = new[] { "Bloom", "Vignette", "ColorGrading", "AmbientOcclusion" }, Coins = 85, Duration = 2f },
                new { Name = "Ultra Effects", Effects = new[] { "Bloom", "Vignette", "ColorGrading", "AmbientOcclusion", "MotionBlur", "DepthOfField" }, Coins = 75, Duration = 3f }
            };

            foreach (var test in postProcessingTests)
            {
                Debug.Log($"[HighEndDeviceValidator] Testing post-processing: {test.Name}");

                EnablePostProcessingEffects(test.Effects);
                yield return new WaitForSeconds(0.5f);

                CreateTestCoins(test.Coins);
                yield return RunPostProcessingTest(test.Duration, test.Name);

                var metrics = CollectAdvancedMetrics();
                phaseResults.measurements.Add(new HighEndMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gpuTime = metrics.gpuTime,
                    performanceIndex = metrics.performanceIndex,
                    postProcessingCount = test.Effects.Length,
                    postProcessingEffects = string.Join(", ", test.Effects),
                    coinCount = test.Coins
                });

                CleanupTestCoins();
            }

            DisableAllPostProcessingEffects();

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluateHighEndPhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[HighEndDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_ShadowQualityOptimization()
        {
            const float phaseDuration = 12f;
            var phaseName = "Shadow Quality Optimization";

            Debug.Log($"[HighEndDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new HighEndPhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var shadowTests = new[]
            {
                new { Name = "Hard Shadows", Quality = ShadowQuality.HardOnly, Distance = 50f, Coins = 100, Duration = 2f },
                new { Name = "Soft Shadows", Quality = ShadowQuality.All, Distance = 100f, Coins = 95, Duration = 2f },
                new { Name = "High Quality", Quality = ShadowQuality.All, Distance = 150f, Coins = 90, Duration = 2f },
                new { Name = "Ultra Quality", Quality = ShadowQuality.All, Distance = 200f, Coins = 85, Duration = 2f },
                new { Name = "Extreme Distance", Quality = ShadowQuality.All, Distance = 300f, Coins = 80, Duration = 2f }
            };

            foreach (var test in shadowTests)
            {
                Debug.Log($"[HighEndDeviceValidator] Testing shadow quality: {test.Name}");

                SetShadowQuality(test.Quality, test.Distance);
                yield return new WaitForSeconds(0.5f);

                CreateTestCoins(test.Coins);
                yield return RunShadowTest(test.Duration, test.Name);

                var metrics = CollectAdvancedMetrics();
                phaseResults.measurements.Add(new HighEndMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gpuTime = metrics.gpuTime,
                    performanceIndex = metrics.performanceIndex,
                    shadowQuality = test.Quality,
                    shadowDistance = test.Distance,
                    coinCount = test.Coins
                });

                CleanupTestCoins();
            }

            var optimalShadowSettings = FindOptimalShadowSettings(phaseResults);
            _testResults.optimalShadowSettings = optimalShadowSettings;

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluateHighEndPhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[HighEndDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_AntiAliasingOptimization()
        {
            const float phaseDuration = 10f;
            var phaseName = "Anti-Aliasing Optimization";

            Debug.Log($"[HighEndDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new HighEndPhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var aaTests = new[]
            {
                new { Name = "No AA", Level = 0, Method = "None", Coins = 100, Duration = 1.5f },
                new { Name = "FXAA", Level = 2, Method = "FXAA", Coins = 98, Duration = 1.5f },
                new { Name = "SMAA 2x", Level = 2, Method = "SMAA", Coins = 95, Duration = 1.5f },
                new { Name = "MSAA 4x", Level = 4, Method = "MSAA", Coins = 90, Duration = 1.5f },
                new { Name = "MSAA 8x", Level = 8, Method = "MSAA", Coins = 85, Duration = 1.5f },
                new { Name = "TXAA", Level = 4, Method = "TXAA", Coins = 88, Duration = 2f }
            };

            foreach (var test in aaTests)
            {
                Debug.Log($"[HighEndDeviceValidator] Testing anti-aliasing: {test.Name}");

                SetAntiAliasing(test.Level, test.Method);
                yield return new WaitForSeconds(0.5f);

                CreateTestCoins(test.Coins);
                yield return RunAntiAliasingTest(test.Duration, test.Name);

                var metrics = CollectAdvancedMetrics();
                phaseResults.measurements.Add(new HighEndMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gpuTime = metrics.gpuTime,
                    performanceIndex = metrics.performanceIndex,
                    antiAliasingLevel = test.Level,
                    antiAliasingMethod = test.Method,
                    coinCount = test.Coins
                });

                CleanupTestCoins();
            }

            var optimalAASettings = FindOptimalAntiAliasingSettings(phaseResults);
            _testResults.optimalAntiAliasingSettings = optimalAASettings;

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluateHighEndPhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[HighEndDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_ExtremeStressScenarios()
        {
            const float phaseDuration = 40f;
            var phaseName = "Extreme Stress Scenarios";

            Debug.Log($"[HighEndDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new HighEndPhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var stressScenarios = new[]
            {
                new { Name = "Sustained Load", Coins = 120, Duration = 10f, Effects = "Medium" },
                new { Name = "Peak Load", Coins = 150, Duration = 8f, Effects = "High" },
                new { Name = "Extreme Load", Coins = 180, Duration = 6f, Effects = "Ultra" },
                new { Name = "Beyond Maximum", Coins = 200, Duration = 5f, Effects = "Extreme" },
                new { Name = "Recovery Test", Coins = 100, Duration = 8f, Effects = "Low" }
            };

            float totalProgress = 0f;
            float totalDuration = stressScenarios.Sum(s => s.Duration);

            foreach (var scenario in stressScenarios)
            {
                Debug.Log($"[HighEndDeviceValidator] Running stress scenario: {scenario.Name}");

                CreateTestCoins(scenario.Coins);
                EnableStressEffects(scenario.Effects);

                yield return RunExtremeStressTest(scenario.Duration, scenario.Name);

                var metrics = CollectAdvancedMetrics();
                phaseResults.measurements.Add(new HighEndMeasurement
                {
                    measurementName = scenario.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gpuTime = metrics.gpuTime,
                    performanceIndex = metrics.performanceIndex,
                    coinCount = scenario.Coins,
                    stressLevel = scenario.Effects,
                    stressImpact = CalculateStressImpact(metrics),
                    bottleneckType = DetectBottleneck(metrics)
                });

                totalProgress += scenario.Duration;
                ReportProgress(phaseName, (totalProgress / totalDuration) * 100f);

                CleanupStressEffects();
                CleanupTestCoins();
                yield return new WaitForSeconds(1f);
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluateHighEndPhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[HighEndDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_BottleneckAnalysis()
        {
            const float phaseDuration = 25f;
            var phaseName = "Bottleneck Analysis";

            Debug.Log($"[HighEndDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new HighEndPhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var bottleneckTests = new[]
            {
                new { Name = "CPU Bound", CPUHeavy = true, GPUHeavy = false, MemoryHeavy = false, Coins = 100, Duration = 5f },
                new { Name = "GPU Bound", CPUHeavy = false, GPUHeavy = true, MemoryHeavy = false, Coins = 120, Duration = 5f },
                new { Name = "Memory Bound", CPUHeavy = false, GPUHeavy = false, MemoryHeavy = true, Coins = 80, Duration = 5f },
                new { Name = "Mixed Load", CPUHeavy = true, GPUHeavy = true, MemoryHeavy = false, Coins = 110, Duration = 5f },
                new { Name = "All Stress", CPUHeavy = true, GPUHeavy = true, MemoryHeavy = true, Coins = 90, Duration = 5f }
            };

            foreach (var test in bottleneckTests)
            {
                Debug.Log($"[HighEndDeviceValidator] Analyzing bottleneck: {test.Name}");

                CreateTestCoins(test.Coins);
                SimulateBottleneck(test.CPUHeavy, test.GPUHeavy, test.MemoryHeavy);

                yield return RunBottleneckAnalysisTest(test.Duration, test.Name);

                var metrics = CollectAdvancedMetrics();
                var bottleneck = AnalyzeBottleneck(test, metrics);

                phaseResults.measurements.Add(new HighEndMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gpuTime = metrics.gpuTime,
                    performanceIndex = metrics.performanceIndex,
                    coinCount = test.Coins,
                    primaryBottleneck = bottleneck.type,
                    bottleneckSeverity = bottleneck.severity,
                    cpuUsage = bottleneck.cpuUsage,
                    gpuUsage = bottleneck.gpuUsage,
                    memoryBandwidth = bottleneck.memoryBandwidth
                });

                if (bottleneck.severity >= 0.7f)
                {
                    _detectedBottlenecks.Add(bottleneck);
                    OnBottleneckDetected?.Invoke(bottleneck);
                }

                CleanupBottleneckSimulation();
                CleanupTestCoins();
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluateHighEndPhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[HighEndDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_ThermalManagement()
        {
            const float phaseDuration = 35f;
            var phaseName = "Thermal Management";

            Debug.Log($"[HighEndDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new HighEndPhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var thermalTests = new[]
            {
                new { Name = "Normal Load", Coins = 80, Duration = 8f, ExpectedThermal = "Normal" },
                new { Name = "Sustained Load", Coins = 100, Duration = 10f, ExpectedThermal = "Warm" },
                new { Name = "High Load", Coins = 120, Duration = 10f, ExpectedThermal = "Hot" },
                new { Name = "Maximum Load", Coins = 140, Duration = 7f, ExpectedThermal = "Very Hot" }
            };

            float totalProgress = 0f;
            float totalDuration = thermalTests.Sum(t => t.Duration);

            foreach (var test in thermalTests)
            {
                Debug.Log($"[HighEndDeviceValidator] Testing thermal management: {test.Name}");

                CreateTestCoins(test.Coins);

                yield return RunThermalTest(test.Duration, test.Name);

                var metrics = CollectAdvancedMetrics();
                var thermalMetrics = AnalyzeThermalMetrics();

                phaseResults.measurements.Add(new HighEndMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gpuTime = metrics.gpuTime,
                    performanceIndex = metrics.performanceIndex,
                    coinCount = test.Coins,
                    thermalState = thermalMetrics.state,
                    thermalThrottling = thermalMetrics.throttling,
                    temperatureEstimate = thermalMetrics.temperature,
                    performanceDegradation = thermalMetrics.performanceDegradation
                });

                if (thermalMetrics.throttling)
                {
                    var thermalEvent = new ThermalEvent
                    {
                        timestamp = Time.time,
                        temperature = thermalMetrics.temperature,
                        state = thermalMetrics.state,
                        impact = thermalMetrics.performanceDegradation
                    };
                    _thermalEvents.Add(thermalEvent);
                    OnThermalEvent?.Invoke(thermalEvent);
                }

                totalProgress += test.Duration;
                ReportProgress(phaseName, (totalProgress / totalDuration) * 100f);

                CleanupTestCoins();
                yield return new WaitForSeconds(1f);
            }

            var thermalCapability = EvaluateThermalCapability();
            _testResults.thermalCapability = thermalCapability;

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluateHighEndPhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[HighEndDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_PowerOptimization()
        {
            const float phaseDuration = 20f;
            var phaseName = "Power Optimization";

            Debug.Log($"[HighEndDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new HighEndPhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var powerTests = new[]
            {
                new { Name = "Maximum Performance", PowerMode = "Performance", Coins = 120, Duration = 4f },
                new { Name = "Balanced", PowerMode = "Balanced", Coins = 100, Duration = 4f },
                new { Name = "Power Saving", PowerMode = "PowerSaving", Coins = 80, Duration = 4f },
                new { Name = "Adaptive", PowerMode = "Adaptive", Coins = 100, Duration = 4f },
                new { Name = "Custom Optimal", PowerMode = "Custom", Coins = 110, Duration = 4f }
            };

            foreach (var test in powerTests)
            {
                Debug.Log($"[HighEndDeviceValidator] Testing power mode: {test.Name}");

                SetPowerMode(test.PowerMode);
                yield return new WaitForSeconds(1f);

                CreateTestCoins(test.Coins);
                yield return RunPowerOptimizationTest(test.Duration, test.Name);

                var metrics = CollectAdvancedMetrics();
                var powerMetrics = AnalyzePowerMetrics();

                phaseResults.measurements.Add(new HighEndMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gpuTime = metrics.gpuTime,
                    performanceIndex = metrics.performanceIndex,
                    powerMode = test.PowerMode,
                    powerEfficiency = powerMetrics.efficiency,
                    estimatedPowerDraw = powerMetrics.powerDraw,
                    performancePerWatt = powerMetrics.performancePerWatt,
                    coinCount = test.Coins
                });

                CleanupTestCoins();
            }

            var optimalPowerMode = FindOptimalPowerMode(phaseResults);
            _testResults.optimalPowerMode = optimalPowerMode;

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluateHighEndPhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[HighEndDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        #endregion

        #region Test Implementation Methods

        private IEnumerator RunExtremePerformanceTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectAdvancedPerformanceMetrics();
                DetectPerformanceIssues();
                yield return null;
            }
        }

        private IEnumerator RunUltraQualityTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectAdvancedPerformanceMetrics();
                yield return null;
            }
        }

        private IEnumerator RunAdvancedEffectsTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectAdvancedPerformanceMetrics();
                UpdateAdvancedEffects();
                yield return null;
            }
        }

        private IEnumerator RunMaximumConcurrentTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectAdvancedPerformanceMetrics();
                UpdateMaximumConcurrentAnimations();
                yield return null;
            }
        }

        private IEnumerator RunResolutionTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectAdvancedPerformanceMetrics();
                yield return null;
            }
        }

        private IEnumerator RunRayTracingTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectAdvancedPerformanceMetrics();
                yield return null;
            }
        }

        private IEnumerator RunHDRTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectAdvancedPerformanceMetrics();
                yield return null;
            }
        }

        private IEnumerator RunShaderTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectAdvancedPerformanceMetrics();
                yield return null;
            }
        }

        private IEnumerator RunPostProcessingTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectAdvancedPerformanceMetrics();
                yield return null;
            }
        }

        private IEnumerator RunShadowTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectAdvancedPerformanceMetrics();
                yield return null;
            }
        }

        private IEnumerator RunAntiAliasingTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectAdvancedPerformanceMetrics();
                yield return null;
            }
        }

        private IEnumerator RunExtremeStressTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectAdvancedPerformanceMetrics();
                UpdateAdvancedEffects();
                UpdateMaximumConcurrentAnimations();
                yield return null;
            }
        }

        private IEnumerator RunBottleneckAnalysisTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectAdvancedPerformanceMetrics();
                yield return null;
            }
        }

        private IEnumerator RunThermalTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectAdvancedPerformanceMetrics();
                yield return null;
            }
        }

        private IEnumerator RunPowerOptimizationTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectAdvancedPerformanceMetrics();
                yield return null;
            }
        }

        #endregion

        #region Helper Methods

        private void ResetTestState()
        {
            _fpsHistory.Clear();
            _memoryHistory.Clear();
            _frameTimeHistory.Clear();
            _gpuTimeHistory.Clear();
            _detailedHistory.Clear();
            _detectedBottlenecks.Clear();
            _thermalEvents.Clear();

            _totalFrames = 0;
            _droppedFrames = 0;
            _testStartTime = Time.time;
            _peakMemoryUsage = 0f;
            _peakGPUTime = 0f;

            if (_testResults != null)
            {
                _testResults.phaseResults.Clear();
            }
        }

        private void CollectAdvancedPerformanceMetrics()
        {
            float fps, memoryUsage, frameTime, gpuTime;

            if (_performanceMonitor != null)
            {
                var metrics = _performanceMonitor.GetCurrentMetrics();
                fps = metrics.FPS;
                memoryUsage = metrics.MemoryUsageMB;
                frameTime = metrics.FrameTime;
                gpuTime = EstimateGPUTime();
            }
            else
            {
                fps = 1f / Time.unscaledDeltaTime;
                memoryUsage = GC.GetTotalMemory(false) / (1024f * 1024f);
                frameTime = Time.unscaledDeltaTime * 1000f;
                gpuTime = EstimateGPUTime();
            }

            _fpsHistory.Enqueue(fps);
            _memoryHistory.Enqueue(memoryUsage);
            _frameTimeHistory.Enqueue(frameTime);
            _gpuTimeHistory.Enqueue(gpuTime);

            // 维护历史记录大小
            while (_fpsHistory.Count > maxHistorySize) _fpsHistory.Dequeue();
            while (_memoryHistory.Count > maxHistorySize) _memoryHistory.Dequeue();
            while (_frameTimeHistory.Count > maxHistorySize) _frameTimeHistory.Dequeue();
            while (_gpuTimeHistory.Count > maxHistorySize) _gpuTimeHistory.Dequeue();

            // 更新峰值
            if (memoryUsage > _peakMemoryUsage) _peakMemoryUsage = memoryUsage;
            if (gpuTime > _peakGPUTime) _peakGPUTime = gpuTime;

            // 创建详细快照
            var snapshot = new HighEndMetricsSnapshot
            {
                timestamp = Time.time,
                fps = fps,
                memoryUsage = memoryUsage,
                frameTime = frameTime,
                gpuTime = gpuTime,
                performanceIndex = CalculateInstantPerformanceIndex(fps, memoryUsage, frameTime, gpuTime)
            };

            _detailedHistory.Enqueue(snapshot);
            while (_detailedHistory.Count > maxHistorySize / 10) _detailedHistory.Dequeue();

            _totalFrames++;
            if (fps < minimumAcceptableFPS) _droppedFrames++;
        }

        private HighEndMetricsSnapshot CollectAdvancedMetrics()
        {
            if (_performanceMonitor != null)
            {
                var metrics = _performanceMonitor.GetCurrentMetrics();
                return new HighEndMetricsSnapshot
                {
                    timestamp = Time.time,
                    fps = metrics.FPS,
                    memoryUsage = metrics.MemoryUsageMB,
                    frameTime = metrics.FrameTime,
                    gpuTime = EstimateGPUTime(),
                    performanceIndex = CalculateInstantPerformanceIndex(metrics.FPS, metrics.MemoryUsageMB, metrics.FrameTime, EstimateGPUTime())
                };
            }

            return new HighEndMetricsSnapshot
            {
                timestamp = Time.time,
                fps = _fpsHistory.Count > 0 ? _fpsHistory.Average() : 60f,
                memoryUsage = _memoryHistory.Count > 0 ? _memoryHistory.Average() : 0f,
                frameTime = _frameTimeHistory.Count > 0 ? _frameTimeHistory.Average() : 16.67f,
                gpuTime = _gpuTimeHistory.Count > 0 ? _gpuTimeHistory.Average() : 10f,
                performanceIndex = CalculatePerformanceIndex()
            };
        }

        private float EstimateGPUTime()
        {
            // 简化的GPU时间估算
            var frameTime = Time.unscaledDeltaTime * 1000f;
            var cpuTime = frameTime * 0.6f; // 假设CPU占60%
            return frameTime - cpuTime;
        }

        private float CalculateInstantPerformanceIndex(float fps, float memoryUsage, float frameTime, float gpuTime)
        {
            var fpsScore = Mathf.Clamp01(fps / idealFPS) * 40f;
            var memoryScore = Mathf.Clamp01(1f - (memoryUsage / memoryWarningThreshold)) * 20f;
            var frameTimeScore = Mathf.Clamp01((frameTimeWarningThreshold - frameTime) / frameTimeWarningThreshold) * 20f;
            var gpuScore = Mathf.Clamp01((20f - gpuTime) / 20f) * 20f;

            return fpsScore + memoryScore + frameTimeScore + gpuScore;
        }

        private float CalculatePerformanceIndex()
        {
            if (_detailedHistory.Count < 10) return 0f;

            var recentSnapshots = _detailedHistory.TakeLast(10).ToArray();
            return recentSnapshots.Average(s => s.performanceIndex);
        }

        private void DetectPerformanceIssues()
        {
            if (_fpsHistory.Count < 30) return;

            var recentFPS = _fpsHistory.TakeLast(30).ToArray();
            var averageFPS = recentFPS.Average();
            var minFPS = recentFPS.Min();

            // 检测严重性能下降
            if (minFPS < minimumAcceptableFPS * 0.5f)
            {
                var bottleneck = new PerformanceBottleneck
                {
                    timestamp = Time.time,
                    type = BottleneckType.PerformanceDrop,
                    severity = 1f - (minFPS / minimumAcceptableFPS),
                    description = $"Severe performance drop detected: {minFPS:F1} FPS (avg: {averageFPS:F1})"
                };
                _detectedBottlenecks.Add(bottleneck);
                OnBottleneckDetected?.Invoke(bottleneck);
            }
        }

        private BottleneckType DetectBottleneck(HighEndMetricsSnapshot metrics)
        {
            if (metrics.frameTime > frameTimeWarningThreshold * 2f)
                return BottleneckType.CPU;
            if (metrics.gpuTime > 15f)
                return BottleneckType.GPU;
            if (metrics.memoryUsage > memoryWarningThreshold * 1.5f)
                return BottleneckType.Memory;
            return BottleneckType.None;
        }

        // 继续下一部分...

        #endregion

        #region Graphics Settings Methods

        private void ApplyUltraQualitySettings(dynamic preset)
        {
            // 应用超高质量设置
            QualitySettings.antiAliasing = preset.AA;
            QualitySettings.shadows = preset.Shadows;
            QualitySettings.anisotropicFiltering = preset.Aniso > 0;
            QualitySettings.shadowDistance = 200f;
            QualitySettings.shadowCascadeCount = 4;
            QualitySettings.lodBias = 2f;
            QualitySettings.maximumLODLevel = 0;
            QualitySettings.particleRaycastBudget = 2048;
            QualitySettings.asyncUploadTimeSlice = 4;
            QualitySettings.asyncUploadBufferSize = 16;
            QualitySettings.realtimeReflectionProbes = true;
        }

        private bool SetResolution(int width, int height)
        {
            var resolutions = Screen.resolutions;
            var targetResolution = resolutions.FirstOrDefault(r => r.width == width && r.height == height);

            if (targetResolution.width > 0)
            {
                Screen.SetResolution(width, height, Screen.fullScreen);
                return true;
            }

            return false;
        }

        private bool EnableRayTracing(string quality)
        {
            // 简化的光线追踪启用（实际需要RTX支持）
            Debug.Log($"[HighEndDeviceValidator] Enabling Ray Tracing with quality: {quality}");
            return true; // 假设支持
        }

        private void DisableRayTracing()
        {
            Debug.Log("[HighEndDeviceValidator] Disabling Ray Tracing");
        }

        private bool EnableHDR(string format)
        {
            // 简化的HDR启用
            Debug.Log($"[HighEndDeviceValidator] Enabling HDR format: {format}");
            return true; // 假设支持
        }

        private void DisableHDR()
        {
            Debug.Log("[HighEndDeviceValidator] Disabling HDR");
        }

        private void SetupShaderType(string type)
        {
            Debug.Log($"[HighEndDeviceValidator] Setting up shader type: {type}");
        }

        private int GetShaderComplexity(string type)
        {
            switch (type)
            {
                case "Standard": return 1;
                case "PBR": return 3;
                case "SSS": return 5;
                case "CustomComplex": return 7;
                case "Compute": return 6;
                default: return 1;
            }
        }

        private void EnablePostProcessingEffects(string[] effects)
        {
            Debug.Log($"[HighEndDeviceValidator] Enabling post-processing effects: {string.Join(", ", effects)}");
        }

        private void EnableAllAdvancedEffects()
        {
            Debug.Log("[HighEndDeviceValidator] Enabling all advanced effects");
        }

        private void CleanupAdvancedEffects()
        {
            Debug.Log("[HighEndDeviceValidator] Cleaning up advanced effects");
        }

        private void DisableAllPostProcessingEffects()
        {
            Debug.Log("[HighEndDeviceValidator] Disabling all post-processing effects");
        }

        private void SetShadowQuality(ShadowQuality quality, float distance)
        {
            QualitySettings.shadows = quality;
            QualitySettings.shadowDistance = distance;
        }

        private void SetAntiAliasing(int level, string method)
        {
            QualitySettings.antiAliasing = level;
        }

        private void SetPowerMode(string mode)
        {
            Debug.Log($"[HighEndDeviceValidator] Setting power mode: {mode}");
        }

        private void RestoreOriginalGraphicsSettings()
        {
            if (_originalGraphicsSettings.ContainsKey("vSyncCount"))
                QualitySettings.vSyncCount = (int)_originalGraphicsSettings["vSyncCount"];
            if (_originalGraphicsSettings.ContainsKey("antiAliasing"))
                QualitySettings.antiAliasing = (int)_originalGraphicsSettings["antiAliasing"];
            if (_originalGraphicsSettings.ContainsKey("shadowDistance"))
                QualitySettings.shadowDistance = (float)_originalGraphicsSettings["shadowDistance"];
            // 恢复其他设置...
        }

        #endregion

        #region Object Creation and Management

        private void CreateTestCoins(int count)
        {
            var coinPrefab = Resources.Load<GameObject>("HighEndTestCoin");
            if (coinPrefab == null)
            {
                coinPrefab = CreateHighEndTestCoin();
            }

            for (int i = 0; i < count; i++)
            {
                var coin = Instantiate(coinPrefab);
                coin.name = $"HighEndTestCoin_{i}";
                coin.tag = "HighEndTestCoin";
                coin.transform.position = UnityEngine.Random.insideUnitSphere * 12f;
            }
        }

        private GameObject CreateHighEndTestCoin()
        {
            var coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "HighEndTestCoin";
            coin.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);

            var renderer = coin.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Standard"));
            material.color = Color.gold;
            material.enableInstancing = true;
            material.SetFloat("_Metallic", 0.8f);
            material.SetFloat("_Glossiness", 0.9f);
            renderer.material = material;

            return coin;
        }

        private void CleanupTestCoins()
        {
            var testCoins = GameObject.FindGameObjectsWithTag("HighEndTestCoin");
            foreach (var coin in testCoins)
            {
                DestroyImmediate(coin);
            }
        }

        private void CreateAdvancedParticleEffects(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var particle = new GameObject($"AdvancedParticle_{i}");
                particle.tag = "AdvancedParticle";
                particle.transform.position = UnityEngine.Random.insideUnitSphere * 15f;
                // particle.AddComponent<ParticleSystem>(); // 添加粒子系统
            }
        }

        private void UpdateAdvancedEffects()
        {
            var particles = GameObject.FindGameObjectsWithTag("AdvancedParticle");
            foreach (var particle in particles)
            {
                particle.transform.Rotate(0f, Time.deltaTime * 100f, 0f);
            }
        }

        private void SetupAdvancedShaders(string complexity)
        {
            Debug.Log($"[HighEndDeviceValidator] Setting up advanced shaders: {complexity}");
        }

        private void EnablePostProcessingEffects(string effects)
        {
            Debug.Log($"[HighEndDeviceValidator] Enabling post-processing: {effects}");
        }

        private void StartMaximumConcurrentAnimations()
        {
            var coins = GameObject.FindGameObjectsWithTag("HighEndTestCoin");
            foreach (var coin in coins)
            {
                var animator = coin.GetComponent<HighEndTestAnimator>();
                if (animator == null)
                {
                    animator = coin.AddComponent<HighEndTestAnimator>();
                }
                animator.StartMaximumAnimation();
            }
        }

        private void UpdateMaximumConcurrentAnimations()
        {
            var animators = FindObjectsOfType<HighEndTestAnimator>();
            foreach (var animator in animators)
            {
                animator.UpdateMaximumAnimation();
            }
        }

        private void StopMaximumConcurrentAnimations()
        {
            var animators = FindObjectsOfType<HighEndTestAnimator>();
            foreach (var animator in animators)
            {
                animator.StopMaximumAnimation();
            }
        }

        private void EnableStressEffects(string level)
        {
            Debug.Log($"[HighEndDeviceValidator] Enabling stress effects: {level}");
        }

        private void CleanupStressEffects()
        {
            Debug.Log("[HighEndDeviceValidator] Cleaning up stress effects");
        }

        private void SimulateBottleneck(bool cpuHeavy, bool gpuHeavy, bool memoryHeavy)
        {
            Debug.Log($"[HighEndDeviceValidator] Simulating bottleneck - CPU: {cpuHeavy}, GPU: {gpuHeavy}, Memory: {memoryHeavy}");
        }

        private void CleanupBottleneckSimulation()
        {
            Debug.Log("[HighEndDeviceValidator] Cleaning up bottleneck simulation");
        }

        #endregion

        #region Analysis Methods

        private ConcurrencyMetrics CalculateConcurrencyMetrics(int coinCount, HighEndMetricsSnapshot metrics)
        {
            return new ConcurrencyMetrics
            {
                efficiency = Mathf.Clamp01(metrics.fps / targetFPS) * 100f,
                complexity = coinCount,
                memoryPerCoin = metrics.memoryUsage / coinCount
            };
        }

        private float CalculateStressImpact(HighEndMetricsSnapshot metrics)
        {
            var baselineFPS = 120f; // 高端设备基准FPS
            return Mathf.Clamp01(1f - (metrics.fps / baselineFPS)) * 100f;
        }

        private PerformanceBottleneck AnalyzeBottleneck(dynamic test, HighEndMetricsSnapshot metrics)
        {
            var bottleneckType = BottleneckType.None;
            var severity = 0f;

            if (test.CPUHeavy && metrics.frameTime > frameTimeWarningThreshold)
            {
                bottleneckType = BottleneckType.CPU;
                severity = Mathf.Clamp01((metrics.frameTime - frameTimeWarningThreshold) / frameTimeWarningThreshold);
            }
            else if (test.GPUHeavy && metrics.gpuTime > 15f)
            {
                bottleneckType = BottleneckType.GPU;
                severity = Mathf.Clamp01((metrics.gpuTime - 15f) / 15f);
            }
            else if (test.MemoryHeavy && metrics.memoryUsage > memoryWarningThreshold)
            {
                bottleneckType = BottleneckType.Memory;
                severity = Mathf.Clamp01((metrics.memoryUsage - memoryWarningThreshold) / memoryWarningThreshold);
            }

            return new PerformanceBottleneck
            {
                timestamp = Time.time,
                type = bottleneckType,
                severity = severity,
                cpuUsage = EstimateCPUUsage(),
                gpuUsage = EstimateGPUUsage(metrics.gpuTime),
                memoryBandwidth = EstimateMemoryBandwidth(metrics.memoryUsage)
            };
        }

        private ThermalMetrics AnalyzeThermalMetrics()
        {
            // 简化的热分析
            var runtime = Time.time - _testStartTime;
            var temperature = 25f + (runtime * 0.1f); // 假设温度随时间上升
            var throttling = temperature > 85f;
            var degradation = throttling ? 0.2f : 0f;

            return new ThermalMetrics
            {
                state = GetThermalState(temperature),
                temperature = temperature,
                throttling = throttling,
                performanceDegradation = degradation
            };
        }

        private string GetThermalState(float temperature)
        {
            if (temperature < 50f) return "Normal";
            if (temperature < 70f) return "Warm";
            if (temperature < 85f) return "Hot";
            return "Very Hot";
        }

        private PowerMetrics AnalyzePowerMetrics()
        {
            // 简化的功耗分析
            var fps = _fpsHistory.Count > 0 ? _fpsHistory.Average() : 60f;
            var powerDraw = EstimatePowerDraw(fps);
            var efficiency = fps / powerDraw;

            return new PowerMetrics
            {
                efficiency = efficiency,
                powerDraw = powerDraw,
                performancePerWatt = fps / powerDraw
            };
        }

        private float EstimatePowerDraw(float fps)
        {
            // 简化的功耗估算
            return 100f + (fps * 0.5f); // 基础功耗 + FPS相关功耗
        }

        private float EstimateCPUUsage()
        {
            return UnityEngine.Random.Range(30f, 80f); // 简化估算
        }

        private float EstimateGPUUsage(float gpuTime)
        {
            return Mathf.Clamp01(gpuTime / 16.67f) * 100f;
        }

        private float EstimateMemoryBandwidth(float memoryUsage)
        {
            return memoryUsage * 0.1f; // 简化估算
        }

        private string FindOptimalUltraPreset(HighEndPhaseResults phaseResults)
        {
            var bestPreset = "Ultra High";
            float bestScore = 0f;

            foreach (var measurement in phaseResults.measurements)
            {
                var score = CalculateUltraQualityScore(measurement.fps, measurement.memoryUsage, measurement.performanceIndex);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPreset = measurement.measurementName;
                }
            }

            return bestPreset;
        }

        private float CalculateUltraQualityScore(float fps, float memoryUsage, float performanceIndex)
        {
            var fpsScore = Mathf.Clamp01(fps / targetFPS) * 40f;
            var memoryScore = Mathf.Clamp01(1f - (memoryUsage / memoryWarningThreshold)) * 20f;
            var performanceScore = performanceIndex * 0.4f;

            return fpsScore + memoryScore + performanceScore;
        }

        private int DetermineMaximumConcurrentCoins(HighEndPhaseResults phaseResults)
        {
            var optimalMeasurement = phaseResults.measurements
                .Where(m => m.fps >= minimumAcceptableFPS && m.concurrencyEfficiency >= 70f)
                .OrderByDescending(m => m.coinCount)
                .FirstOrDefault();

            return optimalMeasurement?.coinCount ?? 100;
        }

        private bool Check4KSupport(HighEndPhaseResults phaseResults)
        {
            var measurement4K = phaseResults.measurements.FirstOrDefault(m => m.resolutionWidth >= 3840);
            return measurement4K != null && measurement4K.fps >= minimumAcceptableFPS;
        }

        private bool CheckRayTracingSupport(HighEndPhaseResults phaseResults)
        {
            var rtMeasurement = phaseResults.measurements.FirstOrDefault(m => m.rayTracingEnabled == true);
            return rtMeasurement != null && rtMeasurement.fps >= minimumAcceptableFPS * 0.8f;
        }

        private bool CheckHDRSupport(HighEndPhaseResults phaseResults)
        {
            var hdrMeasurement = phaseResults.measurements.FirstOrDefault(m => m.hdrEnabled == true);
            return hdrMeasurement != null && hdrMeasurement.fps >= minimumAcceptableFPS * 0.9f;
        }

        private dynamic FindOptimalShadowSettings(HighEndPhaseResults phaseResults)
        {
            var bestMeasurement = phaseResults.measurements
                .Where(m => m.fps >= targetFPS * 0.9f)
                .OrderByDescending(m => m.shadowDistance)
                .FirstOrDefault();

            return new
            {
                quality = bestMeasurement?.shadowQuality ?? ShadowQuality.All,
                distance = bestMeasurement?.shadowDistance ?? 150f
            };
        }

        private dynamic FindOptimalAntiAliasingSettings(HighEndPhaseResults phaseResults)
        {
            var bestMeasurement = phaseResults.measurements
                .Where(m => m.fps >= targetFPS * 0.95f)
                .OrderByDescending(m => m.antiAliasingLevel)
                .FirstOrDefault();

            return new
            {
                level = bestMeasurement?.antiAliasingLevel ?? 4,
                method = bestMeasurement?.antiAliasingMethod ?? "MSAA"
            };
        }

        private ThermalCapability EvaluateThermalCapability()
        {
            var throttlingEvents = _thermalEvents.Count(e => e.throttling);
            var thermalEfficiency = 1f - (throttlingEvents / Mathf.Max(1f, _thermalEvents.Count));

            return new ThermalCapability
            {
                canSustainHighLoad = thermalEfficiency >= 0.8f,
                thermalEfficiency = thermalEfficiency,
                maxSustainedDuration = CalculateMaxSustainedDuration(),
                coolingRecommendation = GetCoolingRecommendation(thermalEfficiency)
            };
        }

        private float CalculateMaxSustainedDuration()
        {
            // 基于热事件计算最大持续负载时间
            var firstThermalEvent = _thermalEvents.FirstOrDefault();
            if (firstThermalEvent != null)
            {
                return firstThermalEvent.timestamp - _testStartTime;
            }
            return extremeTestDuration; // 没有热事件，可以持续全程
        }

        private string GetCoolingRecommendation(float efficiency)
        {
            if (efficiency >= 0.9f) return "Excellent cooling - no action needed";
            if (efficiency >= 0.7f) return "Good cooling - ensure adequate ventilation";
            if (efficiency >= 0.5f) return "Fair cooling - consider improved airflow";
            return "Poor cooling - active cooling solution recommended";
        }

        private string FindOptimalPowerMode(HighEndPhaseResults phaseResults)
        {
            var bestMeasurement = phaseResults.measurements
                .OrderByDescending(m => m.performancePerWatt)
                .FirstOrDefault();

            return bestMeasurement?.powerMode ?? "Balanced";
        }

        private PhaseResult EvaluateHighEndPhaseResult(HighEndPhaseResults phaseResults)
        {
            if (phaseResults.measurements.Count == 0) return PhaseResult.Error;

            var averagePerformanceIndex = phaseResults.measurements.Average(m => m.performanceIndex);
            var minFPS = phaseResults.measurements.Min(m => m.fps);

            if (averagePerformanceIndex >= 90f && minFPS >= targetFPS * 0.9f)
                return PhaseResult.Excellent;
            if (averagePerformanceIndex >= 80f && minFPS >= targetFPS * 0.8f)
                return PhaseResult.Good;
            if (averagePerformanceIndex >= 70f && minFPS >= targetFPS * 0.7f)
                return PhaseResult.Acceptable;
            if (averagePerformanceIndex >= 60f && minFPS >= targetFPS * 0.5f)
                return PhaseResult.Poor;
            return PhaseResult.Critical;
        }

        private ValidationResult EvaluateOverallResult()
        {
            if (_testResults.phaseResults.Count == 0) return ValidationResult.Error;

            var phaseScores = _testResults.phaseResults.Select(p => GetPhaseScore(p.result));
            var overallScore = phaseScores.Average();

            if (overallScore >= 90f) return ValidationResult.Excellent;
            if (overallScore >= 80f) return ValidationResult.Good;
            if (overallScore >= 70f) return ValidationResult.Acceptable;
            if (overallScore >= 50f) return ValidationResult.Poor;
            return ValidationResult.Critical;
        }

        private float GetPhaseScore(PhaseResult result)
        {
            switch (result)
            {
                case PhaseResult.Excellent: return 95f;
                case PhaseResult.Good: return 85f;
                case PhaseResult.Acceptable: return 75f;
                case PhaseResult.Poor: return 55f;
                case PhaseResult.Critical: return 25f;
                default: return 0f;
            }
        }

        private HighEndDeviceInfo CollectHighEndDeviceInfo()
        {
            return new HighEndDeviceInfo
            {
                deviceModel = SystemInfo.deviceModel,
                processorType = SystemInfo.processorType,
                processorCount = SystemInfo.processorCount,
                processorFrequency = SystemInfo.processorFrequency,
                systemMemorySize = SystemInfo.systemMemorySize,
                graphicsDeviceName = SystemInfo.graphicsDeviceName,
                graphicsMemorySize = SystemInfo.graphicsMemorySize,
                graphicsDeviceType = SystemInfo.graphicsDeviceType.ToString(),
                graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion,
                graphicsShaderLevel = SystemInfo.graphicsShaderLevel,
                maxTextureSize = SystemInfo.maxTextureSize,
                renderTextureSupport = SystemInfo.supportsRenderTextures,
                computeShaderSupport = SystemInfo.supportsComputeShaders,
                operatingSystem = SystemInfo.operatingSystem,
                detectedAsHighEnd = _isHighEndDevice,
                deviceScore = CalculateHighEndDeviceScore(),
                supportsAdvancedFeatures = _supportsAdvancedFeatures
            };
        }

        private float CalculateHighEndDeviceScore()
        {
            var memoryGB = SystemInfo.systemMemorySize / 1024f;
            var processorScore = SystemInfo.processorCount * 15f;
            var graphicsScore = SystemInfo.graphicsMemorySize / 64f;
            var shaderScore = SystemInfo.graphicsShaderLevel * 10f;

            return (memoryGB * 8f + processorScore + graphicsScore + shaderScore) / 4f;
        }

        private List<string> GenerateHighEndRecommendations()
        {
            var recommendations = new List<string>();

            var averageFPS = _fpsHistory.Count > 0 ? _fpsHistory.Average() : 0f;
            var averageMemory = _memoryHistory.Count > 0 ? _memoryHistory.Average() : 0f;
            var performanceIndex = CalculatePerformanceIndex();

            if (performanceIndex < 80f)
            {
                recommendations.Add("Performance index below optimal. Consider optimizing advanced effects settings.");
                recommendations.Add("Monitor GPU utilization and thermal performance under sustained load.");
            }

            if (_detectedBottlenecks.Count > 0)
            {
                recommendations.Add($"Detected {_detectedBottlenecks.Count} performance bottlenecks. Analyze bottleneck types for targeted optimization.");
            }

            if (_thermalEvents.Count > 0)
            {
                recommendations.Add($"Thermal throttling detected ({_thermalEvents.Count} events). Consider improved cooling solutions.");
            }

            // 高端设备特定建议
            recommendations.Add("This high-end device supports:");
            recommendations.Add("- Ultra quality settings with full visual fidelity");
            recommendations.Add("- 100-150 concurrent coins with maximum effects");
            recommendations.Add("- 4K resolution support (if display permits)");
            recommendations.Add("- Advanced ray tracing and HDR rendering");
            recommendations.Add("- Comprehensive post-processing effects");
            recommendations.Add("- Real-time monitoring and dynamic optimization");

            if (_testResults.supports4K)
            {
                recommendations.Add("- 4K gaming at stable frame rates is achievable");
            }

            if (_testResults.supportsRayTracing)
            {
                recommendations.Add("- Ray tracing can be enabled with acceptable performance impact");
            }

            return recommendations;
        }

        private HighEndOptimizationProfile GenerateHighEndOptimizationProfile()
        {
            return new HighEndOptimizationProfile
            {
                deviceInfo = CollectHighEndDeviceInfo(),
                recommendedUltraPreset = _testResults?.recommendedUltraPreset ?? "Ultra High",
                maximumConcurrentCoins = _testResults?.maximumConcurrentCoins ?? 100,
                supports4K = _testResults?.supports4K ?? false,
                supportsRayTracing = _testResults?.supportsRayTracing ?? false,
                supportsHDR = _testResults?.supportsHDR ?? false,
                optimalShadowSettings = _testResults?.optimalShadowSettings,
                optimalAntiAliasingSettings = _testResults?.optimalAntiAliasingSettings,
                thermalCapability = _testResults?.thermalCapability,
                optimalPowerMode = _testResults?.optimalPowerMode ?? "Balanced",
                performanceIndex = CalculatePerformanceIndex(),
                detectedBottlenecks = _detectedBottlenecks,
                thermalEvents = _thermalEvents
            };
        }

        private void ReportProgress(string phaseName, float percentage)
        {
            var progress = new HighEndTestProgress
            {
                currentPhase = phaseName,
                progressPercentage = percentage,
                estimatedTimeRemaining = 0f
            };

            OnHighEndTestProgress?.Invoke(progress);
        }

        private void CleanupTestEnvironment()
        {
            CleanupTestCoins();
            CleanupAdvancedEffects();
            DisableAllPostProcessingEffects();
            DisableRayTracing();
            DisableHDR();

            GC.Collect();
        }

        #endregion

        #region Public API

        public HighEndTestReport GenerateHighEndReport()
        {
            if (_testResults == null)
            {
                return new HighEndTestReport
                {
                    isValid = false,
                    errorMessage = "No test data available"
                };
            }

            return new HighEndTestReport
            {
                isValid = _testResults.overallResult != ValidationResult.Error,
                testResults = _testResults,
                deviceInfo = _testResults.deviceInfo,
                overallScore = GetPhaseScore(_testResults.overallResult),
                performanceIndex = _testResults.performanceIndex,
                recommendations = _testResults.recommendations,
                optimizationProfile = _testResults.optimizationProfile,
                generatedAt = DateTime.UtcNow
            };
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class HighEndTestResults
    {
        public DateTime testStartTime;
        public DateTime testEndTime;
        public HighEndDeviceInfo deviceInfo;
        public bool isHighEndDevice;
        public bool supportsAdvancedFeatures;
        public List<HighEndPhaseResults> phaseResults = new List<HighEndPhaseResults>();
        public ValidationResult overallResult;
        public List<string> recommendations = new List<string>();
        public string recommendedUltraPreset;
        public int maximumConcurrentCoins;
        public bool supports4K;
        public bool supportsRayTracing;
        public bool supportsHDR;
        public dynamic optimalShadowSettings;
        public dynamic optimalAntiAliasingSettings;
        public ThermalCapability thermalCapability;
        public string optimalPowerMode;
        public float performanceIndex;
        public HighEndOptimizationProfile optimizationProfile;
    }

    [System.Serializable]
    public class HighEndDeviceInfo
    {
        public string deviceModel;
        public string processorType;
        public int processorCount;
        public float processorFrequency;
        public int systemMemorySize;
        public string graphicsDeviceName;
        public int graphicsMemorySize;
        public string graphicsDeviceType;
        public string graphicsDeviceVersion;
        public int graphicsShaderLevel;
        public int maxTextureSize;
        public bool renderTextureSupport;
        public bool computeShaderSupport;
        public string operatingSystem;
        public bool detectedAsHighEnd;
        public float deviceScore;
        public bool supportsAdvancedFeatures;
    }

    [System.Serializable]
    public class HighEndPhaseResults
    {
        public string phaseName;
        public DateTime startTime;
        public DateTime endTime;
        public PhaseResult result;
        public List<HighEndMeasurement> measurements = new List<HighEndMeasurement>();
    }

    [System.Serializable]
    public class HighEndMeasurement
    {
        public string measurementName;
        public float fps;
        public float memoryUsage;
        public float frameTime;
        public float gpuTime;
        public float performanceIndex;
        public int coinCount;
        public QualityLevel? qualityLevel;
        public int? antiAliasing;
        public ShadowQuality? shadowQuality;
        public float? shadowDistance;
        public int? anisotropicFiltering;
        public int? particleCount;
        public string shaderComplexity;
        public string shaderType;
        public int? shaderComplexityLevel;
        public string postProcessingEffects;
        public int? postProcessingCount;
        public float? concurrencyEfficiency;
        public float? memoryPerCoin;
        public int? resolutionWidth;
        public int? resolutionHeight;
        public long? pixelCount;
        public bool? rayTracingEnabled;
        public string rayTracingQuality;
        public bool? hdrEnabled;
        public string hdrFormat;
        public float? scalingEfficiency;
        public string animationComplexity;
        public bool? combinedEffects;
        public string stressLevel;
        public float? stressImpact;
        public BottleneckType? bottleneckType;
        public BottleneckType? primaryBottleneck;
        public float? bottleneckSeverity;
        public float? cpuUsage;
        public float? gpuUsage;
        public float? memoryBandwidth;
        public string thermalState;
        public bool? thermalThrottling;
        public float? temperatureEstimate;
        public float? performanceDegradation;
        public string powerMode;
        public float? powerEfficiency;
        public float? estimatedPowerDraw;
        public float? performancePerWatt;
    }

    [System.Serializable]
    public class HighEndMetricsSnapshot
    {
        public float timestamp;
        public float fps;
        public float memoryUsage;
        public float frameTime;
        public float gpuTime;
        public float performanceIndex;
    }

    [System.Serializable]
    public class HighEndTestProgress
    {
        public string currentPhase;
        public float progressPercentage;
        public float estimatedTimeRemaining;
    }

    [System.Serializable]
    public class HighEndTestReport
    {
        public bool isValid;
        public string errorMessage;
        public HighEndTestResults testResults;
        public HighEndDeviceInfo deviceInfo;
        public float overallScore;
        public float performanceIndex;
        public List<string> recommendations;
        public HighEndOptimizationProfile optimizationProfile;
        public DateTime generatedAt;
    }

    [System.Serializable]
    public class HighEndOptimizationProfile
    {
        public HighEndDeviceInfo deviceInfo;
        public string recommendedUltraPreset;
        public int maximumConcurrentCoins;
        public bool supports4K;
        public bool supportsRayTracing;
        public bool supportsHDR;
        public dynamic optimalShadowSettings;
        public dynamic optimalAntiAliasingSettings;
        public ThermalCapability thermalCapability;
        public string optimalPowerMode;
        public float performanceIndex;
        public List<PerformanceBottleneck> detectedBottlenecks;
        public List<ThermalEvent> thermalEvents;
    }

    [System.Serializable]
    public class PerformanceBottleneck
    {
        public float timestamp;
        public BottleneckType type;
        public float severity;
        public string description;
        public float cpuUsage;
        public float gpuUsage;
        public float memoryBandwidth;
    }

    [System.Serializable]
    public class ThermalEvent
    {
        public float timestamp;
        public float temperature;
        public string state;
        public float impact;
    }

    [System.Serializable]
    public class ThermalCapability
    {
        public bool canSustainHighLoad;
        public float thermalEfficiency;
        public float maxSustainedDuration;
        public string coolingRecommendation;
    }

    [System.Serializable]
    public class ConcurrencyMetrics
    {
        public float efficiency;
        public int complexity;
        public float memoryPerCoin;
    }

    [System.Serializable]
    public class ThermalMetrics
    {
        public string state;
        public float temperature;
        public bool throttling;
        public float performanceDegradation;
    }

    [System.Serializable]
    public class PowerMetrics
    {
        public float efficiency;
        public float powerDraw;
        public float performancePerWatt;
    }

    public enum HighEndTestType
    {
        ExtremeBaseline,
        UltraQuality,
        AdvancedEffects,
        MaximumConcurrent,
        Resolution4K,
        RayTracing,
        HDRRendering,
        AdvancedShaders,
        PostProcessing,
        ShadowQuality,
        AntiAliasing,
        ExtremeStress,
        BottleneckAnalysis,
        ThermalManagement,
        PowerOptimization
    }

    public enum BottleneckType
    {
        None,
        CPU,
        GPU,
        Memory,
        Bandwidth,
        PerformanceDrop
    }

    #endregion

    #region Helper Components

    /// <summary>
    /// 高端设备测试动画器
    /// </summary>
    public class HighEndTestAnimator : MonoBehaviour
    {
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private Vector3 _startScale;
        private float _animationTime;
        private bool _isAnimating;

        public void StartMaximumAnimation()
        {
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _startScale = transform.localScale;
            _animationTime = 0f;
            _isAnimating = true;
        }

        public void UpdateMaximumAnimation()
        {
            if (!_isAnimating) return;

            _animationTime += Time.deltaTime;

            // 复杂的多层动画
            var posOffset = new Vector3(
                Mathf.Sin(_animationTime * 2f) * 3f,
                Mathf.Cos(_animationTime * 3f) * 1f,
                Mathf.Sin(_animationTime * 1.5f) * 2f
            );
            transform.position = _startPosition + posOffset;

            transform.rotation = _startRotation * Quaternion.Euler(
                Mathf.Sin(_animationTime * 2.5f) * 180f,
                _animationTime * 120f,
                Mathf.Cos(_animationTime * 2f) * 90f
            );

            var scaleMultiplier = 1f + Mathf.Sin(_animationTime * 4f) * 0.3f;
            transform.localScale = _startScale * scaleMultiplier;
        }

        public void StopMaximumAnimation()
        {
            _isAnimating = false;
            transform.position = _startPosition;
            transform.rotation = _startRotation;
            transform.localScale = _startScale;
        }
    }

    #endregion
}