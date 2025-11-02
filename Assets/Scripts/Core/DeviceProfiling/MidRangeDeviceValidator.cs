using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CoinAnimation.Core;
using CoinAnimation.AdaptiveQuality;

namespace CoinAnimation.Core.DeviceProfiling
{
    /// <summary>
    /// 中端设备性能验证器
    /// Story 2.1 Task 5.2 - 中端设备性能验证
    /// </summary>
    public class MidRangeDeviceValidator : MonoBehaviour
    {
        #region Configuration

        [Header("Mid-Range Device Specifications")]
        [SerializeField] private DeviceSpecs midRangeSpecs = new DeviceSpecs
        {
            minCPUScore = 60f,
            minMemoryScore = 60f,
            minGPUScore = 50f,
            minStorageScore = 70f,
            targetFPS = 60f,
            maxConcurrentCoins = 50,
            qualityLevel = QualityLevel.Medium
        };

        [Header("Test Configuration")]
        [SerializeField] private bool enableMidRangeTesting = true;
        [SerializeField] private bool autoDetectMidRange = true;
        [SerializeField] private float standardTestDuration = 45f;
        [SerializeField] private int maxCoinCount = 60; // 中端设备最大金币数

        [Header("Performance Targets")]
        [SerializeField] private float minimumAcceptableFPS = 50f;
        [SerializeField] private float targetFPS = 60f;
        [SerializeField] private float memoryWarningThreshold = 300f; // MB
        [SerializeField] private float frameTimeWarningThreshold = 20f; // ms (50 FPS)

        [Header("Mid-Range Feature Tests")]
        [SerializeField] private bool testAdvancedAnimations = true;
        [SerializeField] private bool testParticleEffects = true;
        [SerializeField] private bool testDynamicQualityScaling = true;
        [SerializeField] private bool testMultiScenePerformance = true;

        [Header("Balanced Optimization Tests")]
        [SerializeField] private bool testQualityBalance = true;
        [SerializeField] private bool testPerformanceConsistency = true;
        [SerializeField] private bool testResourceManagement = true;

        #endregion

        #region Private Fields

        private DeviceCapabilityDetector _deviceDetector;
        private IAdaptiveQualityManager _qualityManager;
        private PerformanceMonitor _performanceMonitor;
        private AdvancedPerformanceDashboard _dashboard;
        private MemoryManagementSystem _memorySystem;
        private CoinObjectPool _objectPool;

        private MidRangeTestResults _testResults;
        private bool _isTestRunning = false;
        private bool _isMidRangeDevice = false;

        // 性能监控
        private readonly Queue<float> _fpsHistory = new Queue<float>();
        private readonly Queue<float> _memoryHistory = new Queue<float>();
        private readonly Queue<float> _frameTimeHistory = new Queue<float>();
        private readonly Queue<MidRangeMetricsSnapshot> _detailedHistory = new Queue<MidRangeMetricsSnapshot>();
        private readonly int maxHistorySize = 5000;

        // 测试统计
        private int _totalFrames;
        private int _droppedFrames;
        private float _testStartTime;
        private float _peakMemoryUsage;
        private List<float> _fpsSpikeTimes = new List<float>();
        private List<float> _memorySpikeTimes = new List<float>();

        #endregion

        #region Events

        public event Action<MidRangeTestResults> OnMidRangeTestStarted;
        public event Action<MidRangeTestResults> OnMidRangeTestCompleted;
        public event Action<string> OnMidRangeTestError;
        public event Action<MidRangeTestProgress> OnMidRangeTestProgress;

        #endregion

        #region Properties

        public bool IsTestRunning => _isTestRunning;
        public bool IsMidRangeDevice => _isMidRangeDevice;
        public MidRangeTestResults CurrentTestResults => _testResults;
        public float CurrentStabilityIndex => CalculateStabilityIndex();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            DetectDeviceCapabilities();
        }

        private void Start()
        {
            if (enableMidRangeTesting && autoDetectMidRange && _isMidRangeDevice)
            {
                Debug.Log("[MidRangeDeviceValidator] Mid-range device detected, starting automatic validation");
                StartCoroutine(DelayedValidationStart());
            }
        }

        private IEnumerator DelayedValidationStart()
        {
            yield return new WaitForSeconds(2f); // 等待系统完全初始化
            StartMidRangeValidation();
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
                    _isMidRangeDevice = EvaluateMidRangeDevice(capabilities);
                    Debug.Log($"[MidRangeDeviceValidator] Device classification: {capabilities.PerformanceTier} (Mid-range: {_isMidRangeDevice})");
                };

                _deviceDetector.RedetectDeviceCapabilities();
            }
            else
            {
                _isMidRangeDevice = PerformSimpleMidRangeDetection();
                Debug.Log($"[MidRangeDeviceValidator] Simple detection result: Mid-range device = {_isMidRangeDevice}");
            }
        }

        private bool EvaluateMidRangeDevice(DeviceCapabilities capabilities)
        {
            return capabilities.CPUScore >= midRangeSpecs.minCPUScore &&
                   capabilities.MemoryScore >= midRangeSpecs.minMemoryScore &&
                   capabilities.GPUScore >= midRangeSpecs.minGPUScore &&
                   capabilities.PerformanceTier == DevicePerformanceClass.MidRange;
        }

        private bool PerformSimpleMidRangeDetection()
        {
            var memoryGB = SystemInfo.systemMemorySize / 1024f;
            var processorCount = SystemInfo.processorCount;
            var graphicsMemoryMB = SystemInfo.graphicsMemorySize;

            return (memoryGB >= 6f && memoryGB <= 16f) || // 6-16GB内存
                   (processorCount >= 4 && processorCount <= 8) || // 4-8核CPU
                   (graphicsMemoryMB >= 1024 && graphicsMemoryMB <= 4096); // 1-4GB显存
        }

        #endregion

        #region Test Control

        public void StartMidRangeValidation()
        {
            if (_isTestRunning)
            {
                Debug.LogWarning("[MidRangeDeviceValidator] Test is already running");
                return;
            }

            if (!_isMidRangeDevice)
            {
                Debug.LogWarning("[MidRangeDeviceValidator] This device is not classified as mid-range. Test may not be optimal.");
            }

            StartCoroutine(RunMidRangeValidationCoroutine());
        }

        public void StopMidRangeValidation()
        {
            if (!_isTestRunning)
            {
                return;
            }

            _isTestRunning = false;
            StopAllCoroutines();
            Debug.Log("[MidRangeDeviceValidator] Mid-range validation stopped by user");
        }

        public void StartSpecificTest(MidRangeTestType testType)
        {
            if (_isTestRunning)
            {
                Debug.LogWarning("[MidRangeDeviceValidator] Cannot start specific test while validation is running");
                return;
            }

            StartCoroutine(RunSpecificTestCoroutine(testType));
        }

        #endregion

        #region Test Coroutines

        private IEnumerator RunMidRangeValidationCoroutine()
        {
            _isTestRunning = true;
            _testResults = new MidRangeTestResults
            {
                testStartTime = DateTime.UtcNow,
                deviceInfo = CollectMidRangeDeviceInfo(),
                isMidRangeDevice = _isMidRangeDevice
            };

            OnMidRangeTestStarted?.Invoke(_testResults);

            Debug.Log("[MidRangeDeviceValidator] Starting comprehensive mid-range device validation");

            try
            {
                ResetTestState();

                // Phase 1: 基准性能和稳定性测试
                yield return RunPhase_BaselineAndStability();

                // Phase 2: 平衡质量设置测试
                if (testQualityBalance)
                {
                    yield return RunPhase_BalancedQualitySettings();
                }

                // Phase 3: 高级动画效果测试
                if (testAdvancedAnimations)
                {
                    yield return RunPhase_AdvancedAnimations();
                }

                // Phase 4: 粒子效果性能测试
                if (testParticleEffects)
                {
                    yield return RunPhase_ParticleEffects();
                }

                // Phase 5: 动态质量缩放测试
                if (testDynamicQualityScaling)
                {
                    yield return RunPhase_DynamicQualityScaling();
                }

                // Phase 6: 性能一致性测试
                if (testPerformanceConsistency)
                {
                    yield return RunPhase_PerformanceConsistency();
                }

                // Phase 7: 资源管理测试
                if (testResourceManagement)
                {
                    yield return RunPhase_ResourceManagement();
                }

                // Phase 8: 多场景性能测试
                if (testMultiScenePerformance)
                {
                    yield return RunPhase_MultiScenePerformance();
                }

                // Phase 9: 综合压力测试
                yield return RunPhase_ComprehensiveStressTest();

                // 完成测试
                _testResults.testEndTime = DateTime.UtcNow;
                _testResults.overallResult = EvaluateOverallResult();
                _testResults.recommendations = GenerateMidRangeRecommendations();
                _testResults.optimizationProfile = GenerateOptimizationProfile();

                Debug.Log($"[MidRangeDeviceValidator] Validation completed. Overall result: {_testResults.overallResult}");

                OnMidRangeTestCompleted?.Invoke(_testResults);
            }
            catch (Exception e)
            {
                var errorMessage = $"Mid-range validation failed: {e.Message}";
                Debug.LogError($"[MidRangeDeviceValidator] {errorMessage}");
                OnMidRangeTestError?.Invoke(errorMessage);
            }
            finally
            {
                _isTestRunning = false;
                CleanupTestEnvironment();
            }
        }

        private IEnumerator RunSpecificTestCoroutine(MidRangeTestType testType)
        {
            _isTestRunning = true;

            try
            {
                switch (testType)
                {
                    case MidRangeTestType.BaselineStability:
                        yield return RunPhase_BaselineAndStability();
                        break;
                    case MidRangeTestType.BalancedQuality:
                        yield return RunPhase_BalancedQualitySettings();
                        break;
                    case MidRangeTestType.AdvancedAnimations:
                        yield return RunPhase_AdvancedAnimations();
                        break;
                    case MidRangeTestType.ParticleEffects:
                        yield return RunPhase_ParticleEffects();
                        break;
                    case MidRangeTestType.DynamicScaling:
                        yield return RunPhase_DynamicQualityScaling();
                        break;
                    case MidRangeTestType.PerformanceConsistency:
                        yield return RunPhase_PerformanceConsistency();
                        break;
                    case MidRangeTestType.ResourceManagement:
                        yield return RunPhase_ResourceManagement();
                        break;
                    case MidRangeTestType.MultiScene:
                        yield return RunPhase_MultiScenePerformance();
                        break;
                    case MidRangeTestType.ComprehensiveStress:
                        yield return RunPhase_ComprehensiveStressTest();
                        break;
                }
            }
            finally
            {
                _isTestRunning = false;
            }
        }

        #endregion

        #region Test Phases

        private IEnumerator RunPhase_BaselineAndStability()
        {
            const float phaseDuration = 15f;
            var phaseName = "Baseline Performance & Stability";

            Debug.Log($"[MidRangeDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new MidRangePhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            // 测试不同负载下的基准性能
            var testLoads = new[] { 10, 20, 30, 40, 50, 60 };

            for (int i = 0; i < testLoads.Length; i++)
            {
                var coinCount = testLoads[i];
                var progress = (float)i / testLoads.Length;

                ReportProgress(phaseName, progress * 100f);

                Debug.Log($"[MidRangeDeviceValidator] Testing baseline with {coinCount} coins");

                CreateTestCoins(coinCount);
                yield return RunStabilityTest(5f, $"Baseline_{coinCount}coins");

                var metrics = CollectDetailedMetrics();
                phaseResults.measurements.Add(new MidRangeMeasurement
                {
                    measurementName = $"{coinCount} Coins",
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    stabilityIndex = metrics.stabilityIndex,
                    coinCount = coinCount
                });

                CleanupTestCoins();
                yield return new WaitForSeconds(1f);
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluatePhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[MidRangeDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_BalancedQualitySettings()
        {
            const float phaseDuration = 20f;
            var phaseName = "Balanced Quality Settings";

            Debug.Log($"[MidRangeDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new MidRangePhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var qualityPresets = new[]
            {
                new { Name = "Balanced", Quality = QualityLevel.Medium, Effects = true },
                new { Name = "Performance", Quality = QualityLevel.Low, Effects = false },
                new { Name = "Quality", Quality = QualityLevel.High, Effects = true },
                new { Name = "Custom Balanced", Quality = QualityLevel.Medium, Effects = true }
            };

            var testCoinCount = 35; // 中等负载

            foreach (var preset in qualityPresets)
            {
                Debug.Log($"[MidRangeDeviceValidator] Testing quality preset: {preset.Name}");

                // 应用预设
                if (_qualityManager != null)
                {
                    _qualityManager.SetQualityLevel(preset.Quality);
                    yield return new WaitForSeconds(2f);
                }

                CreateTestCoins(testCoinCount);
                yield return RunBalancedTest(4f, preset.Name);

                var metrics = CollectDetailedMetrics();
                phaseResults.measurements.Add(new MidRangeMeasurement
                {
                    measurementName = preset.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    stabilityIndex = metrics.stabilityIndex,
                    qualityLevel = preset.Quality,
                    effectsEnabled = preset.Effects
                });

                CleanupTestCoins();
            }

            // 确定最佳平衡设置
            var optimalPreset = FindOptimalBalancedPreset(phaseResults);
            _testResults.recommendedQualityPreset = optimalPreset;

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluatePhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[MidRangeDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_AdvancedAnimations()
        {
            const float phaseDuration = 15f;
            var phaseName = "Advanced Animations Performance";

            Debug.Log($"[MidRangeDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new MidRangePhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var animationTests = new[]
            {
                new { Name = "Simple Movement", Type = "Simple", CoinCount = 40, Duration = 3f },
                new { Name = "Complex Movement", Type = "Complex", CoinCount = 30, Duration = 3f },
                new { Name = "Rotation + Scale", Type = "Complex", CoinCount = 35, Duration = 3f },
                new { Name = "Path Following", Type = "Complex", CoinCount = 25, Duration = 3f }
            };

            foreach (var test in animationTests)
            {
                Debug.Log($"[MidRangeDeviceValidator] Testing animation: {test.Name}");

                CreateTestCoins(test.CoinCount);
                yield return RunAnimationTest(test.Duration, test.Type, test.Name);

                var metrics = CollectDetailedMetrics();
                phaseResults.measurements.Add(new MidRangeMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    stabilityIndex = metrics.stabilityIndex,
                    animationType = test.Type,
                    coinCount = test.CoinCount
                });

                CleanupTestCoins();
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluatePhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[MidRangeDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_ParticleEffects()
        {
            const float phaseDuration = 12f;
            var phaseName = "Particle Effects Performance";

            Debug.Log($"[MidRangeDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new MidRangePhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var particleTests = new[]
            {
                new { Name = "No Particles", Particles = false, CoinCount = 40 },
                new { Name = "Light Particles", Particles = true, ParticleCount = 50, CoinCount = 35 },
                new { Name = "Medium Particles", Particles = true, ParticleCount = 100, CoinCount = 30 },
                new { Name = "Heavy Particles", Particles = true, ParticleCount = 200, CoinCount = 25 }
            };

            foreach (var test in particleTests)
            {
                Debug.Log($"[MidRangeDeviceValidator] Testing particle config: {test.Name}");

                CreateTestCoins(test.CoinCount);
                if (test.Particles)
                {
                    CreateParticleEffects(test.ParticleCount);
                }

                yield return RunParticleTest(3f, test.Name);

                var metrics = CollectDetailedMetrics();
                phaseResults.measurements.Add(new MidRangeMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    stabilityIndex = metrics.stabilityIndex,
                    particlesEnabled = test.Particles,
                    particleCount = test.Particles ? test.ParticleCount : 0,
                    coinCount = test.CoinCount
                });

                CleanupTestCoins();
                CleanupParticleEffects();
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluatePhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[MidRangeDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_DynamicQualityScaling()
        {
            const float phaseDuration = 25f;
            var phaseName = "Dynamic Quality Scaling";

            Debug.Log($"[MidRangeDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new MidRangePhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            // 模拟负载变化，测试动态质量缩放
            var loadScenarios = new[]
            {
                new { Name = "Light Load", Coins = 20, Duration = 5f, ExpectedQuality = QualityLevel.High },
                new { Name = "Medium Load", Coins = 40, Duration = 8f, ExpectedQuality = QualityLevel.Medium },
                new { Name = "Heavy Load", Coins = 60, Duration = 7f, ExpectedQuality = QualityLevel.Low },
                new { Name = "Recovery", Coins = 30, Duration = 5f, ExpectedQuality = QualityLevel.Medium }
            };

            float totalProgress = 0f;
            float totalDuration = loadScenarios.Sum(s => s.Duration);

            foreach (var scenario in loadScenarios)
            {
                Debug.Log($"[MidRangeDeviceValidator] Testing dynamic scaling: {scenario.Name}");

                CreateTestCoins(scenario.Coins);

                // 启用动态质量缩放
                if (_qualityManager != null)
                {
                    _qualityManager.enableAdaptiveQuality = true;
                    _qualityManager.autoAdjustQuality = true;
                }

                yield return RunDynamicScalingTest(scenario.Duration, scenario.Name);

                var metrics = CollectDetailedMetrics();
                var currentQuality = _qualityManager?.CurrentQualityLevel ?? QualityLevel.Medium;

                phaseResults.measurements.Add(new MidRangeMeasurement
                {
                    measurementName = scenario.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    stabilityIndex = metrics.stabilityIndex,
                    coinCount = scenario.Coins,
                    qualityLevel = currentQuality,
                    expectedQuality = scenario.ExpectedQuality,
                    scalingEfficiency = CalculateScalingEfficiency(currentQuality, scenario.ExpectedQuality, metrics.fps)
                });

                totalProgress += scenario.Duration;
                ReportProgress(phaseName, (totalProgress / totalDuration) * 100f);

                CleanupTestCoins();
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluatePhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[MidRangeDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_PerformanceConsistency()
        {
            const float phaseDuration = 20f;
            var phaseName = "Performance Consistency";

            Debug.Log($"[MidRangeDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new MidRangePhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            // 长时间运行测试以检查性能一致性
            var consistencyTests = new[]
            {
                new { Name = "Short Burst", Duration = 5f, CoinCount = 45 },
                new { Name = "Medium Run", Duration = 10f, CoinCount = 40 },
                new { Name = "Extended Run", Duration = 15f, CoinCount = 35 }
            };

            foreach (var test in consistencyTests)
            {
                Debug.Log($"[MidRangeDeviceValidator] Testing consistency: {test.Name}");

                CreateTestCoins(test.CoinCount);
                yield return RunConsistencyTest(test.Duration, test.Name);

                var metrics = CollectDetailedMetrics();
                var consistencyMetrics = CalculateConsistencyMetrics();

                phaseResults.measurements.Add(new MidRangeMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    stabilityIndex = metrics.stabilityIndex,
                    consistencyIndex = consistencyMetrics.consistencyIndex,
                    fpsVariance = consistencyMetrics.fpsVariance,
                    memoryVariance = consistencyMetrics.memoryVariance,
                    coinCount = test.CoinCount
                });

                CleanupTestCoins();
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluatePhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[MidRangeDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_ResourceManagement()
        {
            const float phaseDuration = 15f;
            var phaseName = "Resource Management";

            Debug.Log($"[MidRangeDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new MidRangePhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var resourceTests = new[]
            {
                new { Name = "Normal Management", Strategy = "Normal", CoinCount = 40 },
                new { Name = "Optimized Pooling", Strategy = "Optimized", CoinCount = 45 },
                new { Name = "Aggressive Cleanup", Strategy = "Aggressive", CoinCount = 50 }
            };

            foreach (var test in resourceTests)
            {
                Debug.Log($"[MidRangeDeviceValidator] Testing resource strategy: {test.Name}");

                ApplyResourceStrategy(test.Strategy);
                CreateTestCoins(test.CoinCount);

                var startMemory = GC.GetTotalMemory(false);
                yield return RunResourceTest(4f, test.Name);
                var endMemory = GC.GetTotalMemory(false);
                var memoryDelta = (endMemory - startMemory) / (1024f * 1024f);

                var metrics = CollectDetailedMetrics();
                phaseResults.measurements.Add(new MidRangeMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    stabilityIndex = metrics.stabilityIndex,
                    resourceStrategy = test.Strategy,
                    memoryDelta = memoryDelta,
                    coinCount = test.CoinCount
                });

                CleanupTestCoins();
                GC.Collect();
                yield return new WaitForSeconds(1f);
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluatePhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[MidRangeDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_MultiScenePerformance()
        {
            const float phaseDuration = 18f;
            var phaseName = "Multi-Scene Performance";

            Debug.Log($"[MidRangeDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new MidRangePhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            // 模拟多场景环境
            var sceneTests = new[]
            {
                new { Name = "Simple Scene", Complexity = "Simple", CoinCount = 45 },
                new { Name = "Complex Scene", Complexity = "Complex", CoinCount = 35 },
                new { Name = "Mixed Load", Complexity = "Mixed", CoinCount = 40 }
            };

            foreach (var test in sceneTests)
            {
                Debug.Log($"[MidRangeDeviceValidator] Testing scene: {test.Name}");

                SetupSceneEnvironment(test.Complexity);
                CreateTestCoins(test.CoinCount);

                yield return RunMultiSceneTest(5f, test.Name);

                var metrics = CollectDetailedMetrics();
                phaseResults.measurements.Add(new MidRangeMeasurement
                {
                    measurementName = test.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    stabilityIndex = metrics.stabilityIndex,
                    sceneComplexity = test.Complexity,
                    coinCount = test.CoinCount
                });

                CleanupTestCoins();
                CleanupSceneEnvironment();
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluatePhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[MidRangeDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_ComprehensiveStressTest()
        {
            const float phaseDuration = 30f;
            var phaseName = "Comprehensive Stress Test";

            Debug.Log($"[MidRangeDeviceValidator] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new MidRangePhaseResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            // 综合压力测试 - 结合所有功能
            var stressPhases = new[]
            {
                new { Name = "Warm-up", Coins = 20, Effects = false, Duration = 5f },
                new { Name = "Moderate Load", Coins = 35, Effects = true, Duration = 8f },
                new { Name = "Heavy Load", Coins = 50, Effects = true, Duration = 10f },
                new { Name = "Peak Load", Coins = 60, Effects = true, Duration = 7f }
            };

            float totalProgress = 0f;
            float totalDuration = stressPhases.Sum(s => s.Duration);

            foreach (var phase in stressPhases)
            {
                Debug.Log($"[MidRangeDeviceValidator] Stress phase: {phase.Name}");

                CreateTestCoins(phase.Coins);
                if (phase.Effects)
                {
                    CreateParticleEffects(150);
                }

                // 启用动态质量缩放
                if (_qualityManager != null)
                {
                    _qualityManager.enableAdaptiveQuality = true;
                }

                yield return RunComprehensiveStressTest(phase.Duration, phase.Name);

                var metrics = CollectDetailedMetrics();
                phaseResults.measurements.Add(new MidRangeMeasurement
                {
                    measurementName = phase.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    stabilityIndex = metrics.stabilityIndex,
                    coinCount = phase.Coins,
                    effectsEnabled = phase.Effects,
                    stressLevel = DetermineStressLevel(phase.Coins, phase.Effects)
                });

                totalProgress += phase.Duration;
                ReportProgress(phaseName, (totalProgress / totalDuration) * 100f);

                CleanupTestCoins();
                CleanupParticleEffects();
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluatePhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[MidRangeDeviceValidator] Phase completed: {phaseName} - {phaseResults.result}");
        }

        #endregion

        #region Test Implementation Methods

        private IEnumerator RunStabilityTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectPerformanceMetrics();
                DetectPerformanceSpikes();
                yield return null;
            }
        }

        private IEnumerator RunBalancedTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectPerformanceMetrics();
                yield return null;
            }
        }

        private IEnumerator RunAnimationTest(float duration, string animationType, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            // 启动动画测试
            StartAnimationTest(animationType);

            while (Time.time < endTime)
            {
                CollectPerformanceMetrics();
                UpdateAnimations();
                yield return null;
            }

            StopAnimationTest();
        }

        private IEnumerator RunParticleTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectPerformanceMetrics();
                UpdateParticles();
                yield return null;
            }
        }

        private IEnumerator RunDynamicScalingTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectPerformanceMetrics();
                yield return null;
            }
        }

        private IEnumerator RunConsistencyTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectPerformanceMetrics();
                yield return null;
            }
        }

        private IEnumerator RunResourceTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectPerformanceMetrics();
                yield return null;
            }
        }

        private IEnumerator RunMultiSceneTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectPerformanceMetrics();
                yield return null;
            }
        }

        private IEnumerator RunComprehensiveStressTest(float duration, string testName)
        {
            var startTime = Time.time;
            var endTime = startTime + duration;

            while (Time.time < endTime)
            {
                CollectPerformanceMetrics();
                UpdateAnimations();
                UpdateParticles();
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
            _detailedHistory.Clear();
            _fpsSpikeTimes.Clear();
            _memorySpikeTimes.Clear();

            _totalFrames = 0;
            _droppedFrames = 0;
            _testStartTime = Time.time;
            _peakMemoryUsage = 0f;

            if (_testResults != null)
            {
                _testResults.phaseResults.Clear();
            }
        }

        private void CollectPerformanceMetrics()
        {
            float fps, memoryUsage, frameTime;

            if (_performanceMonitor != null)
            {
                var metrics = _performanceMonitor.GetCurrentMetrics();
                fps = metrics.FPS;
                memoryUsage = metrics.MemoryUsageMB;
                frameTime = metrics.FrameTime;
            }
            else
            {
                fps = 1f / Time.unscaledDeltaTime;
                memoryUsage = GC.GetTotalMemory(false) / (1024f * 1024f);
                frameTime = Time.unscaledDeltaTime * 1000f;
            }

            _fpsHistory.Enqueue(fps);
            _memoryHistory.Enqueue(memoryUsage);
            _frameTimeHistory.Enqueue(frameTime);

            // 维护历史记录大小
            while (_fpsHistory.Count > maxHistorySize) _fpsHistory.Dequeue();
            while (_memoryHistory.Count > maxHistorySize) _memoryHistory.Dequeue();
            while (_frameTimeHistory.Count > maxHistorySize) _frameTimeHistory.Dequeue();

            // 更新峰值
            if (memoryUsage > _peakMemoryUsage)
            {
                _peakMemoryUsage = memoryUsage;
            }

            // 创建详细快照
            var snapshot = new MidRangeMetricsSnapshot
            {
                timestamp = Time.time,
                fps = fps,
                memoryUsage = memoryUsage,
                frameTime = frameTime
            };

            _detailedHistory.Enqueue(snapshot);
            while (_detailedHistory.Count > maxHistorySize / 10) _detailedHistory.Dequeue();

            _totalFrames++;
            if (fps < minimumAcceptableFPS)
            {
                _droppedFrames++;
            }
        }

        private void DetectPerformanceSpikes()
        {
            if (_fpsHistory.Count < 10) return;

            var recentFPS = _fpsHistory.TakeLast(10).Average();
            var overallAverage = _fpsHistory.Average();

            // 检测FPS尖峰（下降）
            if (recentFPS < overallAverage * 0.7f)
            {
                _fpsSpikeTimes.Add(Time.time);
            }

            // 检测内存尖峰
            if (_memoryHistory.Count >= 10)
            {
                var recentMemory = _memoryHistory.TakeLast(10).Average();
                var overallMemoryAverage = _memoryHistory.Average();

                if (recentMemory > overallMemoryAverage * 1.3f)
                {
                    _memorySpikeTimes.Add(Time.time);
                }
            }
        }

        private MidRangeMetricsSnapshot CollectDetailedMetrics()
        {
            if (_performanceMonitor != null)
            {
                var metrics = _performanceMonitor.GetCurrentMetrics();
                return new MidRangeMetricsSnapshot
                {
                    timestamp = Time.time,
                    fps = metrics.FPS,
                    memoryUsage = metrics.MemoryUsageMB,
                    frameTime = metrics.FrameTime,
                    stabilityIndex = CalculateStabilityIndex()
                };
            }

            return new MidRangeMetricsSnapshot
            {
                timestamp = Time.time,
                fps = _fpsHistory.Count > 0 ? _fpsHistory.Average() : 60f,
                memoryUsage = _memoryHistory.Count > 0 ? _memoryHistory.Average() : 0f,
                frameTime = _frameTimeHistory.Count > 0 ? _frameTimeHistory.Average() : 16.67f,
                stabilityIndex = CalculateStabilityIndex()
            };
        }

        private float CalculateStabilityIndex()
        {
            if (_fpsHistory.Count < 30) return 100f;

            var recentFPS = _fpsHistory.TakeLast(30).ToArray();
            var average = recentFPS.Average();
            var variance = 0f;

            foreach (var fps in recentFPS)
            {
                variance += Mathf.Pow(fps - average, 2);
            }

            variance /= recentFPS.Length;
            var standardDeviation = Mathf.Sqrt(variance);

            // 稳定性指数：标准差越小越稳定
            var stabilityIndex = Mathf.Clamp01(1f - (standardDeviation / average)) * 100f;
            return stabilityIndex;
        }

        private ConsistencyMetrics CalculateConsistencyMetrics()
        {
            if (_detailedHistory.Count < 10)
            {
                return new ConsistencyMetrics
                {
                    consistencyIndex = 100f,
                    fpsVariance = 0f,
                    memoryVariance = 0f
                };
            }

            var snapshots = _detailedHistory.ToArray();
            var fpsValues = snapshots.Select(s => s.fps).ToArray();
            var memoryValues = snapshots.Select(s => s.memoryUsage).ToArray();

            var fpsVariance = CalculateVariance(fpsValues);
            var memoryVariance = CalculateVariance(memoryValues);
            var consistencyIndex = Mathf.Clamp01(1f - (fpsVariance / (targetFPS * targetFPS))) * 100f;

            return new ConsistencyMetrics
            {
                consistencyIndex = consistencyIndex,
                fpsVariance = fpsVariance,
                memoryVariance = memoryVariance
            };
        }

        private float CalculateVariance(float[] values)
        {
            if (values.Length == 0) return 0f;

            var average = values.Average();
            var variance = 0f;

            foreach (var value in values)
            {
                variance += Mathf.Pow(value - average, 2);
            }

            return variance / values.Length;
        }

        private float CalculateScalingEfficiency(QualityLevel actual, QualityLevel expected, float fps)
        {
            var expectedScore = (int)expected * 25f;
            var actualScore = (int)actual * 25f;
            var fpsScore = Mathf.Clamp01(fps / targetFPS) * 50f;

            return (actualScore + fpsScore) / 2f;
        }

        private StressLevel DetermineStressLevel(int coinCount, bool effectsEnabled)
        {
            if (coinCount <= 20 && !effectsEnabled) return StressLevel.Light;
            if (coinCount <= 35 && effectsEnabled) return StressLevel.Moderate;
            if (coinCount <= 50) return StressLevel.Heavy;
            return StressLevel.Peak;
        }

        private void CreateTestCoins(int count)
        {
            var coinPrefab = Resources.Load<GameObject>("MidRangeTestCoin");
            if (coinPrefab == null)
            {
                coinPrefab = CreateMidRangeTestCoin();
            }

            for (int i = 0; i < count; i++)
            {
                var coin = Instantiate(coinPrefab);
                coin.name = $"MidRangeTestCoin_{i}";
                coin.tag = "MidRangeTestCoin";
                coin.transform.position = UnityEngine.Random.insideUnitSphere * 8f;
            }
        }

        private GameObject CreateMidRangeTestCoin()
        {
            var coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "MidRangeTestCoin";
            coin.transform.localScale = new Vector3(0.4f, 0.08f, 0.4f);

            var renderer = coin.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Standard"));
            material.color = Color.gold;
            material.enableInstancing = true; // 启用实例化以提升性能
            renderer.material = material;

            return coin;
        }

        private void CleanupTestCoins()
        {
            var testCoins = GameObject.FindGameObjectsWithTag("MidRangeTestCoin");
            foreach (var coin in testCoins)
            {
                DestroyImmediate(coin);
            }
        }

        private void CreateParticleEffects(int count)
        {
            // 简化的粒子效果创建
            for (int i = 0; i < count; i++)
            {
                var particle = new GameObject($"TestParticle_{i}");
                particle.tag = "TestParticle";
                particle.transform.position = UnityEngine.Random.insideUnitSphere * 10f;

                // 添加简单的粒子系统组件（如果需要）
                // particle.AddComponent<ParticleSystem>();
            }
        }

        private void UpdateParticles()
        {
            // 更新粒子效果
            var particles = GameObject.FindGameObjectsWithTag("TestParticle");
            foreach (var particle in particles)
            {
                particle.transform.Rotate(0f, Time.deltaTime * 50f, 0f);
            }
        }

        private void CleanupParticleEffects()
        {
            var particles = GameObject.FindGameObjectsWithTag("TestParticle");
            foreach (var particle in particles)
            {
                DestroyImmediate(particle);
            }
        }

        private void StartAnimationTest(string animationType)
        {
            // 启动特定类型的动画测试
            var coins = GameObject.FindGameObjectsWithTag("MidRangeTestCoin");
            foreach (var coin in coins)
            {
                var animator = coin.GetComponent<MidRangeTestAnimator>();
                if (animator == null)
                {
                    animator = coin.AddComponent<MidRangeTestAnimator>();
                }
                animator.StartAnimation(animationType);
            }
        }

        private void UpdateAnimations()
        {
            // 更新动画
            var animators = FindObjectsOfType<MidRangeTestAnimator>();
            foreach (var animator in animators)
            {
                animator.UpdateAnimation();
            }
        }

        private void StopAnimationTest()
        {
            var animators = FindObjectsOfType<MidRangeTestAnimator>();
            foreach (var animator in animators)
            {
                animator.StopAnimation();
            }
        }

        private void ApplyResourceStrategy(string strategy)
        {
            switch (strategy)
            {
                case "Optimized":
                    if (_objectPool != null)
                    {
                        _objectPool.SetPoolSize(75);
                        _objectPool.enablePreallocation = true;
                    }
                    if (_memorySystem != null)
                    {
                        _memorySystem.enableAutoCleanup = true;
                        _memorySystem.cleanupThreshold = 0.75f;
                    }
                    break;
                case "Aggressive":
                    if (_objectPool != null)
                    {
                        _objectPool.SetPoolSize(50);
                        _objectPool.enablePreallocation = false;
                    }
                    if (_memorySystem != null)
                    {
                        _memorySystem.enableAutoCleanup = true;
                        _memorySystem.cleanupThreshold = 0.6f;
                        _memorySystem.cleanupInterval = 3f;
                    }
                    break;
                default: // Normal
                    if (_objectPool != null)
                    {
                        _objectPool.SetPoolSize(60);
                    }
                    if (_memorySystem != null)
                    {
                        _memorySystem.enableAutoCleanup = true;
                        _memorySystem.cleanupThreshold = 0.8f;
                    }
                    break;
            }
        }

        private void SetupSceneEnvironment(string complexity)
        {
            // 根据复杂度设置场景环境
            switch (complexity)
            {
                case "Simple":
                    // 简单场景设置
                    RenderSettings.ambientIntensity = 0.5f;
                    break;
                case "Complex":
                    // 复杂场景设置
                    RenderSettings.ambientIntensity = 1.0f;
                    break;
                case "Mixed":
                    // 混合场景设置
                    RenderSettings.ambientIntensity = 0.75f;
                    break;
            }
        }

        private void CleanupSceneEnvironment()
        {
            // 清理场景环境
            RenderSettings.ambientIntensity = 0.5f;
        }

        private string FindOptimalBalancedPreset(MidRangePhaseResults phaseResults)
        {
            var bestPreset = "Balanced";
            float bestScore = 0f;

            foreach (var measurement in phaseResults.measurements)
            {
                var score = CalculateBalancedScore(measurement.fps, measurement.memoryUsage, measurement.stabilityIndex);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPreset = measurement.measurementName;
                }
            }

            return bestPreset;
        }

        private float CalculateBalancedScore(float fps, float memoryUsage, float stabilityIndex)
        {
            var fpsScore = Mathf.Clamp01(fps / targetFPS) * 40f;
            var memoryScore = Mathf.Clamp01(1f - (memoryUsage / memoryWarningThreshold)) * 30f;
            var stabilityScore = stabilityIndex * 0.3f; // 稳定性占30%

            return fpsScore + memoryScore + stabilityScore;
        }

        private PhaseResult EvaluatePhaseResult(MidRangePhaseResults phaseResults)
        {
            if (phaseResults.measurements.Count == 0) return PhaseResult.Error;

            var averageFPS = phaseResults.measurements.Average(m => m.fps);
            var averageStability = phaseResults.measurements.Average(m => m.stabilityIndex);
            var averageMemory = phaseResults.measurements.Average(m => m.memoryUsage);

            var overallScore = (Mathf.Clamp01(averageFPS / targetFPS) * 50f +
                               (averageStability / 100f) * 30f +
                               Mathf.Clamp01(1f - (averageMemory / memoryWarningThreshold)) * 20f);

            if (overallScore >= 85f) return PhaseResult.Excellent;
            if (overallScore >= 70f) return PhaseResult.Good;
            if (overallScore >= 55f) return PhaseResult.Acceptable;
            if (overallScore >= 35f) return PhaseResult.Poor;
            return PhaseResult.Critical;
        }

        private ValidationResult EvaluateOverallResult()
        {
            if (_testResults.phaseResults.Count == 0) return ValidationResult.Error;

            var phaseScores = _testResults.phaseResults.Select(p => GetPhaseScore(p.result));
            var overallScore = phaseScores.Average();

            if (overallScore >= 85f) return ValidationResult.Excellent;
            if (overallScore >= 70f) return ValidationResult.Good;
            if (overallScore >= 55f) return ValidationResult.Acceptable;
            if (overallScore >= 35f) return ValidationResult.Poor;
            return ValidationResult.Critical;
        }

        private float GetPhaseScore(PhaseResult result)
        {
            switch (result)
            {
                case PhaseResult.Excellent: return 95f;
                case PhaseResult.Good: return 80f;
                case PhaseResult.Acceptable: return 65f;
                case PhaseResult.Poor: return 40f;
                case PhaseResult.Critical: return 15f;
                default: return 0f;
            }
        }

        private MidRangeDeviceInfo CollectMidRangeDeviceInfo()
        {
            return new MidRangeDeviceInfo
            {
                deviceModel = SystemInfo.deviceModel,
                processorType = SystemInfo.processorType,
                processorCount = SystemInfo.processorCount,
                processorFrequency = SystemInfo.processorFrequency,
                systemMemorySize = SystemInfo.systemMemorySize,
                graphicsDeviceName = SystemInfo.graphicsDeviceName,
                graphicsMemorySize = SystemInfo.graphicsMemorySize,
                graphicsDeviceType = SystemInfo.graphicsDeviceType.ToString(),
                operatingSystem = SystemInfo.operatingSystem,
                detectedAsMidRange = _isMidRangeDevice,
                deviceScore = CalculateDeviceScore()
            };
        }

        private float CalculateDeviceScore()
        {
            var memoryGB = SystemInfo.systemMemorySize / 1024f;
            var processorScore = SystemInfo.processorCount * 12f;
            var graphicsScore = SystemInfo.graphicsMemorySize / 32f;

            return (memoryGB * 10f + processorScore + graphicsScore) / 3f;
        }

        private List<string> GenerateMidRangeRecommendations()
        {
            var recommendations = new List<string>();

            var averageFPS = _fpsHistory.Count > 0 ? _fpsHistory.Average() : 0f;
            var averageMemory = _memoryHistory.Count > 0 ? _memoryHistory.Average() : 0f;
            var stabilityIndex = CalculateStabilityIndex();

            if (averageFPS < minimumAcceptableFPS)
            {
                recommendations.Add("Performance is below target. Consider enabling medium quality settings.");
                recommendations.Add("Reduce maximum concurrent coins to 40-45 for optimal performance.");
            }

            if (stabilityIndex < 70f)
            {
                recommendations.Add("Performance stability is low. Enable frame rate limiting to 60 FPS.");
                recommendations.Add("Consider enabling VSync to improve frame consistency.");
            }

            if (averageMemory > memoryWarningThreshold)
            {
                recommendations.Add("Memory usage is elevated. Enable optimized object pooling with 75-100 pool size.");
                recommendations.Add("Consider periodic memory cleanup every 10-15 seconds.");
            }

            // 中端设备特定建议
            recommendations.Add("This mid-range device is optimized for:");
            recommendations.Add("- Balanced quality settings (Medium/High mix)");
            recommendations.Add("- 40-50 concurrent coins with enabled effects");
            recommendations.Add("- Dynamic quality scaling enabled");
            recommendations.Add("- Moderate particle effects (50-100 particles)");
            recommendations.Add("- Frame rate target of 60 FPS with VSync enabled");

            return recommendations;
        }

        private MidRangeOptimizationProfile GenerateOptimizationProfile()
        {
            return new MidRangeOptimizationProfile
            {
                deviceInfo = CollectMidRangeDeviceInfo(),
                recommendedQualityPreset = _testResults?.recommendedQualityPreset ?? "Balanced",
                recommendedCoinCount = DetermineOptimalCoinCount(),
                qualityLevel = QualityLevel.Medium,
                enableDynamicScaling = true,
                enableParticleEffects = true,
                maxParticleCount = 100,
                targetFrameRate = 60,
                enableVSync = true,
                optimizationLevel = DetermineOptimizationLevel(),
                performanceProfile = _testResults?.overallResult ?? ValidationResult.Acceptable
            };
        }

        private int DetermineOptimalCoinCount()
        {
            // 基于测试结果确定最佳金币数量
            var optimalMeasurements = _testResults?.phaseResults
                .SelectMany(p => p.measurements)
                .Where(m => m.fps >= minimumAcceptableFPS && m.stabilityIndex >= 70f)
                .OrderByDescending(m => m.coinCount)
                .FirstOrDefault();

            return optimalMeasurements?.coinCount ?? 40;
        }

        private OptimizationLevel DetermineOptimizationLevel()
        {
            if (_testResults == null) return OptimizationLevel.Unknown;

            switch (_testResults.overallResult)
            {
                case ValidationResult.Excellent:
                case ValidationResult.Good:
                    return OptimizationLevel.Light;
                case ValidationResult.Acceptable:
                    return OptimizationLevel.Moderate;
                case ValidationResult.Poor:
                case ValidationResult.Critical:
                    return OptimizationLevel.Aggressive;
                default:
                    return OptimizationLevel.Unknown;
            }
        }

        private void ReportProgress(string phaseName, float percentage)
        {
            var progress = new MidRangeTestProgress
            {
                currentPhase = phaseName,
                progressPercentage = percentage,
                estimatedTimeRemaining = 0f
            };

            OnMidRangeTestProgress?.Invoke(progress);
        }

        private void CleanupTestEnvironment()
        {
            CleanupTestCoins();
            CleanupParticleEffects();

            // 恢复默认设置
            if (_qualityManager != null)
            {
                _qualityManager.SetQualityLevel(QualityLevel.Medium);
                _qualityManager.enableAdaptiveQuality = false;
            }

            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 60;

            GC.Collect();
        }

        #endregion

        #region Public API

        public MidRangeTestReport GenerateMidRangeReport()
        {
            if (_testResults == null)
            {
                return new MidRangeTestReport
                {
                    isValid = false,
                    errorMessage = "No test data available"
                };
            }

            return new MidRangeTestReport
            {
                isValid = _testResults.overallResult != ValidationResult.Error,
                testResults = _testResults,
                deviceInfo = _testResults.deviceInfo,
                overallScore = GetPhaseScore(_testResults.overallResult),
                recommendations = _testResults.recommendations,
                optimizationProfile = _testResults.optimizationProfile,
                generatedAt = DateTime.UtcNow
            };
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class MidRangeTestResults
    {
        public DateTime testStartTime;
        public DateTime testEndTime;
        public MidRangeDeviceInfo deviceInfo;
        public bool isMidRangeDevice;
        public List<MidRangePhaseResults> phaseResults = new List<MidRangePhaseResults>();
        public ValidationResult overallResult;
        public List<string> recommendations = new List<string>();
        public string recommendedQualityPreset;
        public MidRangeOptimizationProfile optimizationProfile;
    }

    [System.Serializable]
    public class MidRangeDeviceInfo
    {
        public string deviceModel;
        public string processorType;
        public int processorCount;
        public float processorFrequency;
        public int systemMemorySize;
        public string graphicsDeviceName;
        public int graphicsMemorySize;
        public string graphicsDeviceType;
        public string operatingSystem;
        public bool detectedAsMidRange;
        public float deviceScore;
    }

    [System.Serializable]
    public class MidRangePhaseResults
    {
        public string phaseName;
        public DateTime startTime;
        public DateTime endTime;
        public PhaseResult result;
        public List<MidRangeMeasurement> measurements = new List<MidRangeMeasurement>();
    }

    [System.Serializable]
    public class MidRangeMeasurement
    {
        public string measurementName;
        public float fps;
        public float memoryUsage;
        public float frameTime;
        public float stabilityIndex;
        public int coinCount;
        public QualityLevel? qualityLevel;
        public QualityLevel? expectedQuality;
        public bool? effectsEnabled;
        public int? particleCount;
        public float? scalingEfficiency;
        public string animationType;
        public string resourceStrategy;
        public float? memoryDelta;
        public string sceneComplexity;
        public float? consistencyIndex;
        public float? fpsVariance;
        public float? memoryVariance;
        public StressLevel stressLevel;
    }

    [System.Serializable]
    public class MidRangeMetricsSnapshot
    {
        public float timestamp;
        public float fps;
        public float memoryUsage;
        public float frameTime;
        public float stabilityIndex;
    }

    [System.Serializable]
    public class ConsistencyMetrics
    {
        public float consistencyIndex;
        public float fpsVariance;
        public float memoryVariance;
    }

    [System.Serializable]
    public class MidRangeTestProgress
    {
        public string currentPhase;
        public float progressPercentage;
        public float estimatedTimeRemaining;
    }

    [System.Serializable]
    public class MidRangeTestReport
    {
        public bool isValid;
        public string errorMessage;
        public MidRangeTestResults testResults;
        public MidRangeDeviceInfo deviceInfo;
        public float overallScore;
        public List<string> recommendations;
        public MidRangeOptimizationProfile optimizationProfile;
        public DateTime generatedAt;
    }

    [System.Serializable]
    public class MidRangeOptimizationProfile
    {
        public MidRangeDeviceInfo deviceInfo;
        public string recommendedQualityPreset;
        public int recommendedCoinCount;
        public QualityLevel qualityLevel;
        public bool enableDynamicScaling;
        public bool enableParticleEffects;
        public int maxParticleCount;
        public int targetFrameRate;
        public bool enableVSync;
        public OptimizationLevel optimizationLevel;
        public ValidationResult performanceProfile;
    }

    public enum MidRangeTestType
    {
        BaselineStability,
        BalancedQuality,
        AdvancedAnimations,
        ParticleEffects,
        DynamicScaling,
        PerformanceConsistency,
        ResourceManagement,
        MultiScene,
        ComprehensiveStress
    }

    #endregion

    #region Helper Components

    /// <summary>
    /// 中端设备测试动画器
    /// </summary>
    public class MidRangeTestAnimator : MonoBehaviour
    {
        private string _animationType;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private float _animationTime;

        public void StartAnimation(string type)
        {
            _animationType = type;
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _animationTime = 0f;
        }

        public void UpdateAnimation()
        {
            _animationTime += Time.deltaTime;

            switch (_animationType)
            {
                case "Simple":
                    AnimateSimple();
                    break;
                case "Complex":
                    AnimateComplex();
                    break;
            }
        }

        private void AnimateSimple()
        {
            // 简单的上下浮动
            var yOffset = Mathf.Sin(_animationTime * 2f) * 0.5f;
            transform.position = _startPosition + Vector3.up * yOffset;
        }

        private void AnimateComplex()
        {
            // 复杂的运动：旋转 + 缩放 + 位置变化
            transform.rotation = _startRotation * Quaternion.Euler(0f, _animationTime * 90f, 0f);
            var scale = 1f + Mathf.Sin(_animationTime * 3f) * 0.2f;
            transform.localScale = Vector3.one * scale;

            var posOffset = new Vector3(
                Mathf.Sin(_animationTime * 1.5f) * 2f,
                Mathf.Cos(_animationTime * 2f) * 0.5f,
                Mathf.Sin(_animationTime * 1f) * 1f
            );
            transform.position = _startPosition + posOffset;
        }

        public void StopAnimation()
        {
            transform.position = _startPosition;
            transform.rotation = _startRotation;
            transform.localScale = Vector3.one;
        }
    }

    #endregion
}