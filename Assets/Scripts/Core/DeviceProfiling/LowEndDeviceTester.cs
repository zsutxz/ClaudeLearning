using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using CoinAnimation.Core;
using CoinAnimation.AdaptiveQuality;

namespace CoinAnimation.Core.DeviceProfiling
{
    /// <summary>
    /// 低端设备专用性能测试器
    /// Story 2.1 Task 5.1 - 低端设备测试（最低规格）
    /// </summary>
    public class LowEndDeviceTester : MonoBehaviour
    {
        #region Configuration

        [Header("Low-End Device Specifications")]
        [SerializeField] private DeviceSpecs lowEndSpecs = new DeviceSpecs
        {
            minCPUScore = 30f,
            minMemoryScore = 30f,
            minGPUScore = 25f,
            minStorageScore = 40f,
            targetFPS = 45f,
            maxConcurrentCoins = 20,
            qualityLevel = QualityLevel.Low
        };

        [Header("Test Configuration")]
        [SerializeField] private bool enableLowEndTesting = true;
        [SerializeField] private bool autoDetectLowEnd = true;
        [SerializeField] private float testDuration = 60f; // 延长测试时间以充分验证
        [SerializeField] private int maxCoinCount = 25; // 低端设备最大金币数

        [Header("Performance Thresholds")]
        [SerializeField] private float minimumAcceptableFPS = 30f;
        [SerializeField] private float targetFPS = 45f;
        [SerializeField] private float memoryWarningThreshold = 150f; // MB - 更严格的内存限制
        [SerializeField] private float frameTimeWarningThreshold = 33.33f; // ms (30 FPS)

        [Header("Low-End Optimization Tests")]
        [SerializeField] private bool testQualitySettings = true;
        [SerializeField] private bool testObjectPooling = true;
        [SerializeField] private bool testMemoryManagement = true;
        [SerializeField] private bool testRenderingOptimization = true;

        #endregion

        #region Private Fields

        private DeviceCapabilityDetector _deviceDetector;
        private IAdaptiveQualityManager _qualityManager;
        private PerformanceMonitor _performanceMonitor;
        private MemoryManagementSystem _memorySystem;
        private CoinObjectPool _objectPool;

        private LowEndTestResults _testResults;
        private bool _isTestRunning = false;
        private bool _isLowEndDevice = false;

        // 性能监控
        private readonly Queue<float> _fpsHistory = new Queue<float>();
        private readonly Queue<float> _memoryHistory = new Queue<float>();
        private readonly Queue<float> _frameTimeHistory = new Queue<float>();
        private readonly int maxHistorySize = 3000; // 更长的历史记录

        // 测试统计
        private int _totalFrames;
        private int _droppedFrames;
        private float _testStartTime;
        private float _peakMemoryUsage;

        #endregion

        #region Events

        public event Action<LowEndTestResults> OnLowEndTestStarted;
        public event Action<LowEndTestResults> OnLowEndTestCompleted;
        public event Action<string> OnLowEndTestError;
        public event Action<LowEndTestProgress> OnLowEndTestProgress;

        #endregion

        #region Properties

        public bool IsTestRunning => _isTestRunning;
        public bool IsLowEndDevice => _isLowEndDevice;
        public LowEndTestResults CurrentTestResults => _testResults;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            DetectDeviceCapabilities();
        }

        private void Start()
        {
            if (enableLowEndTesting && autoDetectLowEnd && _isLowEndDevice)
            {
                Debug.Log("[LowEndDeviceTester] Low-end device detected, starting automatic validation");
                StartLowEndValidation();
            }
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            _deviceDetector = FindObjectOfType<DeviceCapabilityDetector>();
            _qualityManager = FindObjectOfType<IAdaptiveQualityManager>();
            _performanceMonitor = FindObjectOfType<PerformanceMonitor>();
            _memorySystem = FindObjectOfType<MemoryManagementSystem>();
            _objectPool = FindObjectOfType<CoinObjectPool>();

            // 如果需要，创建缺失的组件
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
                    _isLowEndDevice = EvaluateLowEndDevice(capabilities);
                    Debug.Log($"[LowEndDeviceTester] Device classification: {capabilities.PerformanceTier} (Low-end: {_isLowEndDevice})");
                };

                _deviceDetector.RedetectDeviceCapabilities();
            }
            else
            {
                // 使用简单的系统信息检测
                _isLowEndDevice = PerformSimpleLowEndDetection();
                Debug.Log($"[LowEndDeviceTester] Simple detection result: Low-end device = {_isLowEndDevice}");
            }
        }

        private bool EvaluateLowEndDevice(DeviceCapabilities capabilities)
        {
            return capabilities.CPUScore <= lowEndSpecs.minCPUScore ||
                   capabilities.MemoryScore <= lowEndSpecs.minMemoryScore ||
                   capabilities.GPUScore <= lowEndSpecs.minGPUScore ||
                   capabilities.PerformanceTier == DevicePerformanceClass.LowEnd;
        }

        private bool PerformSimpleLowEndDetection()
        {
            // 简单的低端设备检测标准
            var memoryGB = SystemInfo.systemMemorySize / 1024f;
            var processorCount = SystemInfo.processorCount;
            var graphicsMemoryMB = SystemInfo.graphicsMemorySize;

            return memoryGB <= 4f || // 4GB或更少内存
                   processorCount <= 2 || // 2核或更少CPU
                   graphicsMemoryMB <= 512; // 512MB或更少显存
        }

        #endregion

        #region Test Control

        public void StartLowEndValidation()
        {
            if (_isTestRunning)
            {
                Debug.LogWarning("[LowEndDeviceTester] Test is already running");
                return;
            }

            if (!_isLowEndDevice)
            {
                Debug.LogWarning("[LowEndDeviceTester] This device is not classified as low-end. Test may not be relevant.");
            }

            StartCoroutine(RunLowEndValidationCoroutine());
        }

        public void StopLowEndValidation()
        {
            if (!_isTestRunning)
            {
                return;
            }

            _isTestRunning = false;
            StopAllCoroutines();
            Debug.Log("[LowEndDeviceTester] Low-end validation stopped by user");
        }

        public void StartSpecificTest(LowEndTestType testType)
        {
            if (_isTestRunning)
            {
                Debug.LogWarning("[LowEndDeviceTester] Cannot start specific test while validation is running");
                return;
            }

            StartCoroutine(RunSpecificTestCoroutine(testType));
        }

        #endregion

        #region Test Coroutines

        private IEnumerator RunLowEndValidationCoroutine()
        {
            _isTestRunning = true;
            _testResults = new LowEndTestResults
            {
                testStartTime = DateTime.UtcNow,
                deviceInfo = CollectLowEndDeviceInfo(),
                isLowEndDevice = _isLowEndDevice
            };

            OnLowEndTestStarted?.Invoke(_testResults);

            Debug.Log("[LowEndDeviceTester] Starting comprehensive low-end device validation");

            try
            {
                // 清理初始状态
                ResetTestState();

                // Phase 1: 基准性能测试
                yield return RunPhase_BaselinePerformance();

                // Phase 2: 质量设置优化测试
                if (testQualitySettings)
                {
                    yield return RunPhase_QualitySettingsOptimization();
                }

                // Phase 3: 对象池效率测试
                if (testObjectPooling)
                {
                    yield return RunPhase_ObjectPoolingEfficiency();
                }

                // Phase 4: 内存管理测试
                if (testMemoryManagement)
                {
                    yield return RunPhase_MemoryManagementOptimization();
                }

                // Phase 5: 渲染优化测试
                if (testRenderingOptimization)
                {
                    yield return RunPhase_RenderingOptimization();
                }

                // Phase 6: 压力测试
                yield return RunPhase_StressTest();

                // 完成测试
                _testResults.testEndTime = DateTime.UtcNow;
                _testResults.overallResult = EvaluateOverallResult();
                _testResults.recommendations = GenerateLowEndRecommendations();

                Debug.Log($"[LowEndDeviceTester] Validation completed. Overall result: {_testResults.overallResult}");

                OnLowEndTestCompleted?.Invoke(_testResults);
            }
            catch (Exception e)
            {
                var errorMessage = $"Low-end validation failed: {e.Message}";
                Debug.LogError($"[LowEndDeviceTester] {errorMessage}");
                OnLowEndTestError?.Invoke(errorMessage);
            }
            finally
            {
                _isTestRunning = false;
                CleanupTestEnvironment();
            }
        }

        private IEnumerator RunSpecificTestCoroutine(LowEndTestType testType)
        {
            _isTestRunning = true;

            try
            {
                switch (testType)
                {
                    case LowEndTestType.BaselinePerformance:
                        yield return RunPhase_BaselinePerformance();
                        break;
                    case LowEndTestType.QualityOptimization:
                        yield return RunPhase_QualitySettingsOptimization();
                        break;
                    case LowEndTestType.ObjectPoolingTest:
                        yield return RunPhase_ObjectPoolingEfficiency();
                        break;
                    case LowEndTestType.MemoryManagementTest:
                        yield return RunPhase_MemoryManagementOptimization();
                        break;
                    case LowEndTestType.RenderingOptimizationTest:
                        yield return RunPhase_RenderingOptimization();
                        break;
                    case LowEndTestType.StressTest:
                        yield return RunPhase_StressTest();
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

        private IEnumerator RunPhase_BaselinePerformance()
        {
            const float phaseDuration = 10f;
            var phaseName = "Baseline Performance Test";

            Debug.Log($"[LowEndDeviceTester] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new PhaseTestResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            // 测试不同负载下的基准性能
            var testLoads = new int[] { 0, 5, 10, 15, 20, 25 };

            for (int i = 0; i < testLoads.Length; i++)
            {
                var coinCount = testLoads[i];
                var progress = (float)i / testLoads.Length;

                ReportProgress(phaseName, progress * 100f);

                Debug.Log($"[LowEndDeviceTester] Testing baseline with {coinCount} coins");

                // 创建测试金币
                CreateTestCoins(coinCount);

                // 运行测试
                yield return RunPerformanceTest(5f, $"Baseline_{coinCount}coins");

                // 记录结果
                var metrics = CollectCurrentMetrics();
                phaseResults.measurements.Add(new PhaseMeasurement
                {
                    measurementName = $"{coinCount} Coins",
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    coinCount = coinCount
                });

                // 清理金币
                CleanupTestCoins();

                yield return new WaitForSeconds(1f); // 恢复时间
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluatePhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[LowEndDeviceTester] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_QualitySettingsOptimization()
        {
            const float phaseDuration = 15f;
            var phaseName = "Quality Settings Optimization";

            Debug.Log($"[LowEndDeviceTester] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new PhaseTestResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            var qualityLevels = new QualityLevel[] { QualityLevel.High, QualityLevel.Medium, QualityLevel.Low, QualityLevel.Minimum };
            var testCoinCount = 15; // 中等负载

            for (int i = 0; i < qualityLevels.Length; i++)
            {
                var quality = qualityLevels[i];
                var progress = (float)i / qualityLevels.Length;

                ReportProgress(phaseName, progress * 100f);

                Debug.Log($"[LowEndDeviceTester] Testing quality level: {quality}");

                // 设置质量级别
                if (_qualityManager != null)
                {
                    _qualityManager.SetQualityLevel(quality);
                    yield return new WaitForSeconds(2f); // 等待质量设置生效
                }

                // 创建测试金币
                CreateTestCoins(testCoinCount);

                // 运行测试
                yield return RunPerformanceTest(3f, $"Quality_{quality}");

                // 记录结果
                var metrics = CollectCurrentMetrics();
                phaseResults.measurements.Add(new PhaseMeasurement
                {
                    measurementName = quality.ToString(),
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    qualityLevel = quality
                });

                // 清理金币
                CleanupTestCoins();
            }

            // 恢复最佳质量设置
            if (_qualityManager != null)
            {
                var optimalQuality = FindOptimalQuality(phaseResults);
                _qualityManager.SetQualityLevel(optimalQuality);
                _testResults.recommendedQuality = optimalQuality;
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluatePhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[LowEndDeviceTester] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_ObjectPoolingEfficiency()
        {
            const float phaseDuration = 20f;
            var phaseName = "Object Pooling Efficiency";

            Debug.Log($"[LowEndDeviceTester] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new PhaseTestResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            // 测试对象池效率
            var testCycles = 3;
            var coinsPerCycle = 20;

            for (int cycle = 0; cycle < testCycles; cycle++)
            {
                var progress = (float)cycle / testCycles;
                ReportProgress(phaseName, progress * 100f);

                Debug.Log($"[LowEndDeviceTester] Pool efficiency test cycle {cycle + 1}/{testCycles}");

                // 记录开始时的内存
                var startMemory = GC.GetTotalMemory(false);

                // 多次创建和销毁金币以测试对象池
                for (int i = 0; i < 10; i++)
                {
                    CreateTestCoins(coinsPerCycle);
                    yield return new WaitForSeconds(0.5f);
                    CleanupTestCoins();
                    yield return new WaitForSeconds(0.5f);
                }

                // 记录结束时的内存
                var endMemory = GC.GetTotalMemory(false);
                var memoryIncrease = (endMemory - startMemory) / (1024f * 1024f);

                var metrics = CollectCurrentMetrics();
                phaseResults.measurements.Add(new PhaseMeasurement
                {
                    measurementName = $"Pool Cycle {cycle + 1}",
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    memoryIncrease = memoryIncrease
                });

                // 强制垃圾回收
                GC.Collect();
                yield return new WaitForSeconds(2f);
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluatePhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[LowEndDeviceTester] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_MemoryManagementOptimization()
        {
            const float phaseDuration = 15f;
            var phaseName = "Memory Management Optimization";

            Debug.Log($"[LowEndDeviceTester] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new PhaseTestResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            // 测试内存管理策略
            var strategies = new[] { "Normal", "Aggressive Cleanup", "Conservative" };

            for (int i = 0; i < strategies.Length; i++)
            {
                var strategy = strategies[i];
                var progress = (float)i / strategies.Length;

                ReportProgress(phaseName, progress * 100f);

                Debug.Log($"[LowEndDeviceTester] Testing memory strategy: {strategy}");

                // 应用内存管理策略
                ApplyMemoryStrategy(strategy);

                // 创建内存压力
                CreateTestCoins(20);

                // 运行测试
                yield return RunPerformanceTest(4f, $"Memory_{strategy}");

                // 记录结果
                var metrics = CollectCurrentMetrics();
                phaseResults.measurements.Add(new PhaseMeasurement
                {
                    measurementName = strategy,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    gcCollections = GetGCCollectionCount()
                });

                // 清理
                CleanupTestCoins();
                GC.Collect();
                yield return new WaitForSeconds(1f);
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluatePhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[LowEndDeviceTester] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_RenderingOptimization()
        {
            const float phaseDuration = 10f;
            var phaseName = "Rendering Optimization";

            Debug.Log($"[LowEndDeviceTester] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new PhaseTestResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            // 测试渲染优化设置
            var renderingSettings = new[]
            {
                new { Name = "Default", VSync = 1, TargetFPS = 60 },
                new { Name = "VSync Off", VSync = 0, TargetFPS = 60 },
                new { Name = "30 FPS Cap", VSync = 1, TargetFPS = 30 },
                new { Name = "45 FPS Cap", VSync = 1, TargetFPS = 45 }
            };

            for (int i = 0; i < renderingSettings.Length; i++)
            {
                var setting = renderingSettings[i];
                var progress = (float)i / renderingSettings.Length;

                ReportProgress(phaseName, progress * 100f);

                Debug.Log($"[LowEndDeviceTester] Testing rendering setting: {setting.Name}");

                // 应用渲染设置
                QualitySettings.vSyncCount = setting.VSync;
                Application.targetFrameRate = setting.TargetFPS;

                yield return new WaitForSeconds(1f); // 等待设置生效

                // 创建测试金币
                CreateTestCoins(15);

                // 运行测试
                yield return RunPerformanceTest(3f, $"Render_{setting.Name}");

                // 记录结果
                var metrics = CollectCurrentMetrics();
                phaseResults.measurements.Add(new PhaseMeasurement
                {
                    measurementName = setting.Name,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    targetFPS = setting.TargetFPS
                });

                // 清理
                CleanupTestCoins();
            }

            // 恢复最佳渲染设置
            var optimalRendering = FindOptimalRenderingSettings(phaseResults);
            ApplyRenderingSettings(optimalRendering);
            _testResults.recommendedRenderingSettings = optimalRendering;

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluatePhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[LowEndDeviceTester] Phase completed: {phaseName} - {phaseResults.result}");
        }

        private IEnumerator RunPhase_StressTest()
        {
            const float phaseDuration = 30f; // 延长压力测试时间
            var phaseName = "Extended Stress Test";

            Debug.Log($"[LowEndDeviceTester] Starting phase: {phaseName}");
            ReportProgress(phaseName, 0f);

            var phaseResults = new PhaseTestResults
            {
                phaseName = phaseName,
                startTime = DateTime.UtcNow
            };

            // 逐步增加负载的压力测试
            var stressLevels = new[]
            {
                new { Level = "Light", Coins = 10, Duration = 5f },
                new { Level = "Moderate", Coins = 15, Duration = 8f },
                new { Level = "Heavy", Coins = 20, Duration = 10f },
                new { Level = "Peak", Coins = 25, Duration = 7f }
            };

            float totalProgress = 0f;
            float totalDuration = stressLevels.Sum(s => s.Duration);

            foreach (var stressLevel in stressLevels)
            {
                Debug.Log($"[LowEndDeviceTester] Stress level: {stressLevel.Level} with {stressLevel.Coins} coins");

                // 创建压力负载
                CreateTestCoins(stressLevel.Coins);

                // 运行压力测试
                yield return RunPerformanceTest(stressLevel.Duration, $"Stress_{stressLevel.Level}");

                // 记录结果
                var metrics = CollectCurrentMetrics();
                phaseResults.measurements.Add(new PhaseMeasurement
                {
                    measurementName = stressLevel.Level,
                    fps = metrics.fps,
                    memoryUsage = metrics.memoryUsage,
                    frameTime = metrics.frameTime,
                    coinCount = stressLevel.Coins,
                    duration = stressLevel.Duration
                });

                totalProgress += stressLevel.Duration;
                ReportProgress(phaseName, (totalProgress / totalDuration) * 100f);

                // 清理
                CleanupTestCoins();
                yield return new WaitForSeconds(2f); // 恢复时间
            }

            phaseResults.endTime = DateTime.UtcNow;
            phaseResults.result = EvaluatePhaseResult(phaseResults);

            _testResults.phaseResults.Add(phaseResults);
            Debug.Log($"[LowEndDeviceTester] Phase completed: {phaseName} - {phaseResults.result}");
        }

        #endregion

        #region Helper Methods

        private void ResetTestState()
        {
            _fpsHistory.Clear();
            _memoryHistory.Clear();
            _frameTimeHistory.Clear();
            _totalFrames = 0;
            _droppedFrames = 0;
            _testStartTime = Time.time;
            _peakMemoryUsage = 0f;

            if (_testResults != null)
            {
                _testResults.phaseResults.Clear();
            }
        }

        private void CreateTestCoins(int count)
        {
            var coinPrefab = Resources.Load<GameObject>("LowEndTestCoin");
            if (coinPrefab == null)
            {
                coinPrefab = CreateLowEndTestCoin();
            }

            for (int i = 0; i < count; i++)
            {
                var coin = Instantiate(coinPrefab);
                coin.name = $"LowEndTestCoin_{i}";
                coin.tag = "LowEndTestCoin";
                coin.transform.position = UnityEngine.Random.insideUnitSphere * 5f;
            }
        }

        private void CleanupTestCoins()
        {
            var testCoins = GameObject.FindGameObjectsWithTag("LowEndTestCoin");
            foreach (var coin in testCoins)
            {
                DestroyImmediate(coin);
            }
        }

        private GameObject CreateLowEndTestCoin()
        {
            var coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "LowEndTestCoin";
            coin.transform.localScale = new Vector3(0.3f, 0.05f, 0.3f); // 更小的金币以减轻负载

            var renderer = coin.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Diffuse")); // 使用更简单的着色器
            material.color = Color.yellow;
            renderer.material = material;

            return coin;
        }

        private IEnumerator RunPerformanceTest(float duration, string testName)
        {
            var testStartTime = Time.time;
            var testEndTime = testStartTime + duration;

            while (Time.time < testEndTime)
            {
                CollectPerformanceMetrics();
                _totalFrames++;

                // 检测掉帧
                var currentFPS = 1f / Time.unscaledDeltaTime;
                if (currentFPS < minimumAcceptableFPS)
                {
                    _droppedFrames++;
                }

                yield return null;
            }

            Debug.Log($"[LowEndDeviceTester] Performance test '{testName}' completed. Duration: {duration}s");
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

            // 保持历史记录大小
            while (_fpsHistory.Count > maxHistorySize) _fpsHistory.Dequeue();
            while (_memoryHistory.Count > maxHistorySize) _memoryHistory.Dequeue();
            while (_frameTimeHistory.Count > maxHistorySize) _frameTimeHistory.Dequeue();

            // 更新峰值内存使用
            if (memoryUsage > _peakMemoryUsage)
            {
                _peakMemoryUsage = memoryUsage;
            }
        }

        private PerformanceMetrics CollectCurrentMetrics()
        {
            if (_performanceMonitor != null)
            {
                return _performanceMonitor.GetCurrentMetrics();
            }

            return new PerformanceMetrics
            {
                FPS = _fpsHistory.Count > 0 ? _fpsHistory.Average() : 60f,
                MemoryUsageMB = _memoryHistory.Count > 0 ? _memoryHistory.Average() : 0f,
                FrameTime = _frameTimeHistory.Count > 0 ? _frameTimeHistory.Average() : 16.67f,
                Timestamp = DateTime.UtcNow
            };
        }

        private LowEndDeviceInfo CollectLowEndDeviceInfo()
        {
            return new LowEndDeviceInfo
            {
                deviceModel = SystemInfo.deviceModel,
                processorType = SystemInfo.processorType,
                processorCount = SystemInfo.processorCount,
                systemMemorySize = SystemInfo.systemMemorySize,
                graphicsDeviceName = SystemInfo.graphicsDeviceName,
                graphicsMemorySize = SystemInfo.graphicsMemorySize,
                operatingSystem = SystemInfo.operatingSystem,
                graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion,
                detectedAsLowEnd = _isLowEndDevice,
                deviceScore = CalculateDeviceScore()
            };
        }

        private float CalculateDeviceScore()
        {
            var memoryGB = SystemInfo.systemMemorySize / 1024f;
            var processorScore = SystemInfo.processorCount * 10f;
            var graphicsScore = SystemInfo.graphicsMemorySize / 64f; // 每64MB显存得1分

            return (memoryGB * 15f + processorScore + graphicsScore) / 3f;
        }

        private void ApplyMemoryStrategy(string strategy)
        {
            switch (strategy)
            {
                case "Aggressive Cleanup":
                    if (_memorySystem != null)
                    {
                        _memorySystem.enableAutoCleanup = true;
                        _memorySystem.cleanupThreshold = 0.7f;
                        _memorySystem.cleanupInterval = 5f;
                    }
                    break;
                case "Conservative":
                    if (_memorySystem != null)
                    {
                        _memorySystem.enableAutoCleanup = false;
                    }
                    break;
                default: // Normal
                    if (_memorySystem != null)
                    {
                        _memorySystem.enableAutoCleanup = true;
                        _memorySystem.cleanupThreshold = 0.8f;
                        _memorySystem.cleanupInterval = 10f;
                    }
                    break;
            }
        }

        private int GetGCCollectionCount()
        {
            // 简化的GC计数器
            return GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
        }

        private QualityLevel FindOptimalQuality(PhaseTestResults phaseResults)
        {
            var bestQuality = QualityLevel.Low;
            float bestScore = 0f;

            foreach (var measurement in phaseResults.measurements)
            {
                var score = CalculateQualityScore(measurement.fps, measurement.memoryUsage);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestQuality = measurement.qualityLevel ?? QualityLevel.Low;
                }
            }

            return bestQuality;
        }

        private float CalculateQualityScore(float fps, float memoryUsage)
        {
            var fpsScore = Mathf.Clamp01(fps / targetFPS) * 60f;
            var memoryScore = Mathf.Clamp01(1f - (memoryUsage / memoryWarningThreshold)) * 40f;
            return fpsScore + memoryScore;
        }

        private RenderingSettings FindOptimalRenderingSettings(PhaseTestResults phaseResults)
        {
            var bestSetting = new RenderingSettings { VSync = 1, TargetFPS = 45 };
            float bestScore = 0f;

            foreach (var measurement in phaseResults.measurements)
            {
                var score = CalculateRenderingScore(measurement.fps, measurement.frameTime);
                if (score > bestScore)
                {
                    bestScore = score;
                    // 根据测量名称确定设置
                    if (measurement.measurementName.Contains("30 FPS"))
                    {
                        bestSetting = new RenderingSettings { VSync = 1, TargetFPS = 30 };
                    }
                    else if (measurement.measurementName.Contains("45 FPS"))
                    {
                        bestSetting = new RenderingSettings { VSync = 1, TargetFPS = 45 };
                    }
                    else if (measurement.measurementName.Contains("VSync Off"))
                    {
                        bestSetting = new RenderingSettings { VSync = 0, TargetFPS = 60 };
                    }
                }
            }

            return bestSetting;
        }

        private float CalculateRenderingScore(float fps, float frameTime)
        {
            var fpsScore = Mathf.Clamp01(fps / targetFPS) * 50f;
            var frameTimeScore = Mathf.Clamp01((frameTimeWarningThreshold - frameTime) / frameTimeWarningThreshold) * 50f;
            return fpsScore + frameTimeScore;
        }

        private void ApplyRenderingSettings(RenderingSettings settings)
        {
            QualitySettings.vSyncCount = settings.VSync;
            Application.targetFrameRate = settings.TargetFPS;
        }

        private PhaseResult EvaluatePhaseResult(PhaseTestResults phaseResults)
        {
            if (phaseResults.measurements.Count == 0) return PhaseResult.Error;

            var averageFPS = phaseResults.measurements.Average(m => m.fps);
            var averageMemory = phaseResults.measurements.Average(m => m.memoryUsage);

            if (averageFPS >= targetFPS && averageMemory <= memoryWarningThreshold)
                return PhaseResult.Excellent;
            if (averageFPS >= minimumAcceptableFPS && averageMemory <= memoryWarningThreshold * 1.2f)
                return PhaseResult.Good;
            if (averageFPS >= minimumAcceptableFPS * 0.8f)
                return PhaseResult.Acceptable;
            if (averageFPS >= minimumAcceptableFPS * 0.6f)
                return PhaseResult.Poor;
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

        private List<string> GenerateLowEndRecommendations()
        {
            var recommendations = new List<string>();

            // 基于测试结果生成建议
            var averageFPS = _fpsHistory.Count > 0 ? _fpsHistory.Average() : 0f;
            var averageMemory = _memoryHistory.Count > 0 ? _memoryHistory.Average() : 0f;

            if (averageFPS < minimumAcceptableFPS)
            {
                recommendations.Add("Performance is below acceptable threshold. Consider reducing coin count to 15 or fewer.");
                recommendations.Add("Enable minimum quality settings for optimal performance.");
            }

            if (averageMemory > memoryWarningThreshold)
            {
                recommendations.Add("Memory usage is high. Enable aggressive memory cleanup.");
                recommendations.Add("Consider reducing object pool sizes.");
            }

            if (_droppedFrames > _totalFrames * 0.1f) // 超过10%的掉帧
            {
                recommendations.Add("High frame drop detected. Consider capping frame rate to 30 FPS.");
                recommendations.Add("Enable VSync to improve frame consistency.");
            }

            // 设备特定建议
            recommendations.Add("This low-end device benefits from:");
            recommendations.Add("- Simplified coin models with lower polygon counts");
            recommendations.Add("- Reduced particle effects and animations");
            recommendations.Add("- Aggressive object pooling with frequent cleanup");
            recommendations.Add("- Quality settings preset to Minimum or Low");

            return recommendations;
        }

        private void ReportProgress(string phaseName, float percentage)
        {
            var progress = new LowEndTestProgress
            {
                currentPhase = phaseName,
                progressPercentage = percentage,
                estimatedTimeRemaining = 0f // 可以添加更精确的时间估算
            };

            OnLowEndTestProgress?.Invoke(progress);
        }

        private void CleanupTestEnvironment()
        {
            CleanupTestCoins();

            // 恢复默认设置
            if (_qualityManager != null)
            {
                _qualityManager.SetQualityLevel(QualityLevel.Low);
            }

            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 60;

            GC.Collect();
        }

        #endregion

        #region Public API

        public LowEndTestReport GenerateLowEndReport()
        {
            if (_testResults == null)
            {
                return new LowEndTestReport
                {
                    isValid = false,
                    errorMessage = "No test data available"
                };
            }

            return new LowEndTestReport
            {
                isValid = _testResults.overallResult != ValidationResult.Error,
                testResults = _testResults,
                deviceInfo = _testResults.deviceInfo,
                overallScore = GetPhaseScore(_testResults.overallResult),
                recommendations = _testResults.recommendations,
                generatedAt = DateTime.UtcNow
            };
        }

        public LowEndOptimizationProfile GetOptimizationProfile()
        {
            return new LowEndOptimizationProfile
            {
                deviceInfo = CollectLowEndDeviceInfo(),
                recommendedQuality = _testResults?.recommendedQuality ?? QualityLevel.Low,
                recommendedRenderingSettings = _testResults?.recommendedRenderingSettings ?? new RenderingSettings { VSync = 1, TargetFPS = 45 },
                maxRecommendedCoins = _testResults?.phaseResults?.FirstOrDefault(m => m.measurements.Any(m => m.fps >= minimumAcceptableFPS))?.measurements?.FirstOrDefault()?.coinCount ?? 15,
                optimizationLevel = DetermineOptimizationLevel(),
                specificRecommendations = GenerateLowEndRecommendations()
            };
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

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class LowEndTestResults
    {
        public DateTime testStartTime;
        public DateTime testEndTime;
        public LowEndDeviceInfo deviceInfo;
        public bool isLowEndDevice;
        public List<PhaseTestResults> phaseResults = new List<PhaseTestResults>();
        public ValidationResult overallResult;
        public List<string> recommendations = new List<string>();
        public QualityLevel? recommendedQuality;
        public RenderingSettings? recommendedRenderingSettings;
    }

    [System.Serializable]
    public class LowEndDeviceInfo
    {
        public string deviceModel;
        public string processorType;
        public int processorCount;
        public int systemMemorySize;
        public string graphicsDeviceName;
        public int graphicsMemorySize;
        public string operatingSystem;
        public string graphicsDeviceVersion;
        public bool detectedAsLowEnd;
        public float deviceScore;
    }

    [System.Serializable]
    public class PhaseTestResults
    {
        public string phaseName;
        public DateTime startTime;
        public DateTime endTime;
        public PhaseResult result;
        public List<PhaseMeasurement> measurements = new List<PhaseMeasurement>();
    }

    [System.Serializable]
    public class PhaseMeasurement
    {
        public string measurementName;
        public float fps;
        public float memoryUsage;
        public float frameTime;
        public int coinCount;
        public float memoryIncrease;
        public int gcCollections;
        public QualityLevel? qualityLevel;
        public int targetFPS;
        public float duration;
    }

    public enum PhaseResult
    {
        Excellent,
        Good,
        Acceptable,
        Poor,
        Critical,
        Error
    }

    [System.Serializable]
    public class LowEndTestProgress
    {
        public string currentPhase;
        public float progressPercentage;
        public float estimatedTimeRemaining;
    }

    [System.Serializable]
    public class LowEndTestReport
    {
        public bool isValid;
        public string errorMessage;
        public LowEndTestResults testResults;
        public LowEndDeviceInfo deviceInfo;
        public float overallScore;
        public List<string> recommendations;
        public DateTime generatedAt;
    }

    [System.Serializable]
    public class LowEndOptimizationProfile
    {
        public LowEndDeviceInfo deviceInfo;
        public QualityLevel recommendedQuality;
        public RenderingSettings recommendedRenderingSettings;
        public int maxRecommendedCoins;
        public OptimizationLevel optimizationLevel;
        public List<string> specificRecommendations;
    }

    [System.Serializable]
    public class RenderingSettings
    {
        public int VSync;
        public int TargetFPS;
    }

    public enum LowEndTestType
    {
        BaselinePerformance,
        QualityOptimization,
        ObjectPoolingTest,
        MemoryManagementTest,
        RenderingOptimizationTest,
        StressTest
    }

    public enum OptimizationLevel
    {
        Unknown,
        Light,
        Moderate,
        Aggressive
    }

    #endregion
}