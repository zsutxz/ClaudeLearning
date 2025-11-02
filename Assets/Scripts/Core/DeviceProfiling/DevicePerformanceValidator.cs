using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using CoinAnimation.Core;
using CoinAnimation.AdaptiveQuality;

namespace CoinAnimation.Core.DeviceProfiling
{
    /// <summary>
    /// 设备性能验证器
    /// Story 2.1 Task 5 - 跨设备性能验证系统
    /// </summary>
    public class DevicePerformanceValidator : MonoBehaviour
    {
        #region Configuration

        [Header("Validation Settings")]
        [SerializeField] private bool enableValidation = true;
        [SerializeField] private bool runValidationOnStart = false;
        [SerializeField] private float validationDuration = 30f; // 验证持续时间（秒）
        [SerializeField] private int stressTestCoinCount = 50; // 压力测试金币数量

        [Header("Performance Targets")]
        [SerializeField] private float targetFrameRate = 60f;
        [SerializeField] private float minimumAcceptableFPS = 45f;
        [SerializeField] private float memoryWarningThreshold = 200f; // MB
        [SerializeField] private float memoryCriticalThreshold = 400f; // MB

        [Header("Device Classifications")]
        [SerializeField] private DeviceClassificationConfig classificationConfig;

        [Header("Validation Scenarios")]
        [SerializeField] private List<ValidationScenario> validationScenarios;

        #endregion

        #region Private Fields

        private DeviceCapabilityDetector _deviceDetector;
        private AdaptiveQualityManager _qualityManager;
        private AdvancedPerformanceDashboard _dashboard;
        private PerformanceMonitor _performanceMonitor;

        private ValidationResults _currentValidation;
        private List<ValidationResults> _validationHistory;
        private bool _isValidationRunning = false;

        // 性能指标收集
        private readonly Queue<float> _fpsSamples = new Queue<float>();
        private readonly Queue<float> _memorySamples = new Queue<float>();
        private readonly Queue<float> _frameTimeSamples = new Queue<float>();
        private readonly int maxSampleCount = 1000;

        #endregion

        #region Events

        public event Action<ValidationResults> OnValidationStarted;
        public event Action<ValidationResults> OnValidationCompleted;
        public event Action<ValidationProgress> OnValidationProgress;
        public event Action<string> OnValidationError;

        #endregion

        #region Properties

        public bool IsValidationRunning => _isValidationRunning;
        public ValidationResults CurrentValidation => _currentValidation;
        public List<ValidationResults> ValidationHistory => _validationHistory ??= new List<ValidationResults>();
        public DevicePerformanceClass CurrentDeviceClass => ConvertTierToClass(_deviceDetector?.DetectedCapabilities?.PerformanceTier ?? DevicePerformanceTier.Unknown);

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            LoadDefaultConfiguration();
        }

        private void Start()
        {
            if (runValidationOnStart)
            {
                StartCoroutine(RunFullValidationCoroutine());
            }
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            _deviceDetector = FindObjectOfType<DeviceCapabilityDetector>();
            _qualityManager = FindObjectOfType<AdaptiveQualityManager>();
            _dashboard = FindObjectOfType<AdvancedPerformanceDashboard>();
            _performanceMonitor = FindObjectOfType<PerformanceMonitor>();

            if (_deviceDetector == null)
            {
                _deviceDetector = gameObject.AddComponent<DeviceCapabilityDetector>();
            }

            if (_performanceMonitor == null)
            {
                _performanceMonitor = gameObject.AddComponent<PerformanceMonitor>();
            }

            _validationHistory = new List<ValidationResults>();
        }

        private void LoadDefaultConfiguration()
        {
            if (classificationConfig == null)
            {
                classificationConfig = CreateDefaultClassificationConfig();
            }

            if (validationScenarios == null || validationScenarios.Count == 0)
            {
                validationScenarios = CreateDefaultValidationScenarios();
            }
        }

        private DeviceClassificationConfig CreateDefaultClassificationConfig()
        {
            return new DeviceClassificationConfig
            {
                lowEndSpecs = new DeviceSpecs
                {
                    minCPUScore = 30f,
                    minMemoryScore = 30f,
                    minGPUScore = 25f,
                    minStorageScore = 40f,
                    targetFPS = 45f,
                    maxConcurrentCoins = 20,
                    qualityLevel = QualityLevel.Low
                },
                midRangeSpecs = new DeviceSpecs
                {
                    minCPUScore = 60f,
                    minMemoryScore = 60f,
                    minGPUScore = 50f,
                    minStorageScore = 70f,
                    targetFPS = 60f,
                    maxConcurrentCoins = 50,
                    qualityLevel = QualityLevel.Medium
                },
                highEndSpecs = new DeviceSpecs
                {
                    minCPUScore = 85f,
                    minMemoryScore = 85f,
                    minGPUScore = 75f,
                    minStorageScore = 90f,
                    targetFPS = 60f,
                    maxConcurrentCoins = 100,
                    qualityLevel = QualityLevel.High
                }
            };
        }

        private List<ValidationScenario> CreateDefaultValidationScenarios()
        {
            return new List<ValidationScenario>
            {
                new ValidationScenario
                {
                    scenarioName = "Idle Baseline",
                    description = "Baseline performance with minimal load",
                    coinCount = 0,
                    duration = 5f,
                    targetFPS = 60f,
                    stressLevel = StressLevel.Minimal
                },
                new ValidationScenario
                {
                    scenarioName = "Light Load",
                    description = "Light animation load with few coins",
                    coinCount = 10,
                    duration = 10f,
                    targetFPS = 60f,
                    stressLevel = StressLevel.Light
                },
                new ValidationScenario
                {
                    scenarioName = "Moderate Load",
                    description = "Moderate animation load",
                    coinCount = 30,
                    duration = 15f,
                    targetFPS = 60f,
                    stressLevel = StressLevel.Moderate
                },
                new ValidationScenario
                {
                    scenarioName = "Heavy Load",
                    description = "Heavy animation load stress test",
                    coinCount = 50,
                    duration = 20f,
                    targetFPS = 45f,
                    stressLevel = StressLevel.Heavy
                },
                new ValidationScenario
                {
                    scenarioName = "Peak Load",
                    description = "Maximum capacity stress test",
                    coinCount = 100,
                    duration = 10f,
                    targetFPS = 30f,
                    stressLevel = StressLevel.Peak
                }
            };
        }

        #endregion

        #region Validation Control

        public void StartValidation()
        {
            if (_isValidationRunning)
            {
                UnityEngine.Debug.LogWarning("[DevicePerformanceValidator] Validation is already running");
                return;
            }

            StartCoroutine(RunFullValidationCoroutine());
        }

        public void StopValidation()
        {
            if (!_isValidationRunning)
            {
                return;
            }

            _isValidationRunning = false;
            StopAllCoroutines();
            UnityEngine.Debug.Log("[DevicePerformanceValidator] Validation stopped by user");
        }

        public void StartValidationScenario(ValidationScenario scenario)
        {
            if (_isValidationRunning)
            {
                UnityEngine.Debug.LogWarning("[DevicePerformanceValidator] Cannot start scenario while validation is running");
                return;
            }

            StartCoroutine(RunScenarioValidationCoroutine(scenario));
        }

        #endregion

        #region Validation Coroutines

        private IEnumerator RunFullValidationCoroutine()
        {
            _isValidationRunning = true;
            _currentValidation = new ValidationResults
            {
                validationStartTime = DateTime.UtcNow,
                deviceInfo = CollectDeviceInfo(),
                scenarios = new List<ScenarioResults>()
            };

            OnValidationStarted?.Invoke(_currentValidation);

            UnityEngine.Debug.Log("[DevicePerformanceValidator] Starting full device validation");

            try
            {
                // 运行所有验证场景
                for (int i = 0; i < validationScenarios.Count; i++)
                {
                    var scenario = validationScenarios[i];
                    var progress = new ValidationProgress
                    {
                        currentScenario = i + 1,
                        totalScenarios = validationScenarios.Count,
                        scenarioName = scenario.scenarioName,
                        progressPercentage = (float)(i + 1) / validationScenarios.Count * 100f
                    };

                    OnValidationProgress?.Invoke(progress);

                    UnityEngine.Debug.Log($"[DevicePerformanceValidator] Running scenario: {scenario.scenarioName}");

                    ScenarioResults scenarioResults = null;
                    yield return StartCoroutine(RunScenarioWithCallback(scenario, result => {
                        scenarioResults = result;
                    }));
                    _currentValidation.scenarios.Add(scenarioResults);

                    // 场景间的恢复时间
                    yield return new WaitForSeconds(2f);
                }

                // 生成最终结果
                _currentValidation.validationEndTime = DateTime.UtcNow;
                _currentValidation.overallResult = EvaluateOverallResult(_currentValidation);
                _currentValidation.recommendations = GenerateRecommendations(_currentValidation);

                // 添加到历史记录
                _validationHistory.Add(_currentValidation);

                OnValidationCompleted?.Invoke(_currentValidation);

                UnityEngine.Debug.Log($"[DevicePerformanceValidator] Validation completed. Overall result: {_currentValidation.overallResult}");
            }
            catch (Exception e)
            {
                var errorMessage = $"Validation failed: {e.Message}";
                UnityEngine.Debug.LogError($"[DevicePerformanceValidator] {errorMessage}");
                OnValidationError?.Invoke(errorMessage);
            }
            finally
            {
                _isValidationRunning = false;
            }
        }

        private IEnumerator RunScenarioValidationCoroutine(ValidationScenario scenario)
        {
            _isValidationRunning = true;
            var scenarioResults = new ScenarioResults
            {
                scenario = scenario,
                startTime = DateTime.UtcNow
            };

            try
            {
                ClearSamples();
                SetupScenario(scenario);

                // 运行场景测试
                var elapsed = 0f;
                while (elapsed < scenario.duration)
                {
                    CollectPerformanceSamples();
                    elapsed += Time.deltaTime;

                    // 更新进度
                    var progress = new ValidationProgress
                    {
                        currentScenario = 1,
                        totalScenarios = 1,
                        scenarioName = scenario.scenarioName,
                        progressPercentage = (elapsed / scenario.duration) * 100f
                    };

                    OnValidationProgress?.Invoke(progress);

                    yield return null;
                }

                // 分析结果
                scenarioResults.endTime = DateTime.UtcNow;
                scenarioResults.metrics = AnalyzeCollectedSamples();
                scenarioResults.result = EvaluateScenarioResult(scenarioResults.metrics, scenario);

                UnityEngine.Debug.Log($"[DevicePerformanceValidator] Scenario '{scenario.scenarioName}' completed: {scenarioResults.result}");
            }
            catch (Exception e)
            {
                scenarioResults.error = e.Message;
                scenarioResults.result = ValidationResult.Error;
                UnityEngine.Debug.LogError($"[DevicePerformanceValidator] Scenario '{scenario.scenarioName}' failed: {e.Message}");
            }
            finally
            {
                CleanupScenario();
                _isValidationRunning = false;
            }
        }

        private IEnumerator RunScenarioWithCallback(ValidationScenario scenario, System.Action<ScenarioResults> onComplete)
        {
            var scenarioResults = new ScenarioResults
            {
                scenario = scenario,
                startTime = DateTime.UtcNow
            };

            ClearSamples();
            SetupScenario(scenario);

            var elapsed = 0f;
            while (elapsed < scenario.duration)
            {
                CollectPerformanceSamples();
                elapsed += Time.deltaTime;
                yield return null;
            }

            scenarioResults.endTime = DateTime.UtcNow;
            scenarioResults.metrics = AnalyzeCollectedSamples();
            scenarioResults.result = EvaluateScenarioResult(scenarioResults.metrics, scenario);

            CleanupScenario();

            onComplete?.Invoke(scenarioResults);
        }

        private IEnumerator RunScenario(ValidationScenario scenario)
        {
            var scenarioResults = new ScenarioResults
            {
                scenario = scenario,
                startTime = DateTime.UtcNow
            };

            ClearSamples();
            SetupScenario(scenario);

            var elapsed = 0f;
            while (elapsed < scenario.duration)
            {
                CollectPerformanceSamples();
                elapsed += Time.deltaTime;
                yield return null;
            }

            scenarioResults.endTime = DateTime.UtcNow;
            scenarioResults.metrics = AnalyzeCollectedSamples();
            scenarioResults.result = EvaluateScenarioResult(scenarioResults.metrics, scenario);

            CleanupScenario();

            return scenarioResults;
        }

        #endregion

        #region Scenario Management

        private void SetupScenario(ValidationScenario scenario)
        {
            //Debug.Log($"[DevicePerformanceValidator] Setting up scenario: {scenario.scenarioName} with {scenario.coinCount} coins");

            // 设置质量级别
            if (_qualityManager != null)
            {
                var targetQuality = GetQualityForScenario(scenario);
                _qualityManager.SetQualityLevel(targetQuality);
            }

            // 创建测试金币
            if (scenario.coinCount > 0)
            {
                CreateTestCoins(scenario.coinCount);
            }

            // 预热系统
            yield return new WaitForSeconds(1f);
        }

        private void CleanupScenario()
        {
            // 清理测试金币
            var testCoins = GameObject.FindGameObjectsWithTag("TestCoin");
            foreach (var coin in testCoins)
            {
                DestroyImmediate(coin);
            }

            // 重置系统状态
            if (_qualityManager != null)
            {
                _qualityManager.SetQualityLevel(QualityLevel.Medium);
            }

            ClearSamples();
        }

        private QualityLevel GetQualityForScenario(ValidationScenario scenario)
        {
            switch (scenario.stressLevel)
            {
                case StressLevel.Minimal:
                    return QualityLevel.High;
                case StressLevel.Light:
                    return QualityLevel.Medium;
                case StressLevel.Moderate:
                    return QualityLevel.Medium;
                case StressLevel.Heavy:
                    return QualityLevel.Low;
                case StressLevel.Peak:
                    return QualityLevel.Minimum;
                default:
                    return QualityLevel.Medium;
            }
        }

        #endregion

        #region Performance Collection

        private void ClearSamples()
        {
            _fpsSamples.Clear();
            _memorySamples.Clear();
            _frameTimeSamples.Clear();
        }

        private void CollectPerformanceSamples()
        {
            if (_performanceMonitor != null)
            {
                var metrics = _performanceMonitor.GetCurrentMetrics();
                _fpsSamples.Enqueue(metrics.FPS);
                _memorySamples.Enqueue(metrics.MemoryUsageMB);
                _frameTimeSamples.Enqueue(metrics.FrameTime);
            }
            else
            {
                // 使用Unity内置指标作为后备
                _fpsSamples.Enqueue(1f / Time.unscaledDeltaTime);
                _memorySamples.Enqueue(GC.GetTotalMemory(false) / (1024f * 1024f));
                _frameTimeSamples.Enqueue(Time.unscaledDeltaTime * 1000f);
            }

            // 保持样本数量在限制内
            while (_fpsSamples.Count > maxSampleCount) _fpsSamples.Dequeue();
            while (_memorySamples.Count > maxSampleCount) _memorySamples.Dequeue();
            while (_frameTimeSamples.Count > maxSampleCount) _frameTimeSamples.Dequeue();
        }

        private ScenarioMetrics AnalyzeCollectedSamples()
        {
            return new ScenarioMetrics
            {
                fpsAverage = CalculateAverage(_fpsSamples),
                fpsMin = CalculateMin(_fpsSamples),
                fpsMax = CalculateMax(_fpsSamples),
                fpsStandardDeviation = CalculateStandardDeviation(_fpsSamples),
                frameTimeAverage = CalculateAverage(_frameTimeSamples),
                frameTimeMax = CalculateMax(_frameTimeSamples),
                memoryAverage = CalculateAverage(_memorySamples),
                memoryPeak = CalculateMax(_memorySamples),
                sampleCount = _fpsSamples.Count
            };
        }

        #endregion

        #region Result Evaluation

        private ValidationResult EvaluateScenarioResult(ScenarioMetrics metrics, ValidationScenario scenario)
        {
            var fpsScore = CalculateFPSScore(metrics.fpsAverage, scenario.targetFPS);
            var stabilityScore = CalculateStabilityScore(metrics.fpsStandardDeviation);
            var memoryScore = CalculateMemoryScore(metrics.memoryPeak);

            var overallScore = (fpsScore + stabilityScore + memoryScore) / 3f;

            if (overallScore >= 90f) return ValidationResult.Excellent;
            if (overallScore >= 75f) return ValidationResult.Good;
            if (overallScore >= 60f) return ValidationResult.Acceptable;
            if (overallScore >= 40f) return ValidationResult.Poor;
            return ValidationResult.Critical;
        }

        private ValidationResult EvaluateOverallResult(ValidationResults results)
        {
            if (results.scenarios.Count == 0) return ValidationResult.Error;

            var scores = new List<float>();
            foreach (var scenario in results.scenarios)
            {
                scores.Add(GetResultScore(scenario.result));
            }

            var averageScore = scores.Average();

            if (averageScore >= 85f) return ValidationResult.Excellent;
            if (averageScore >= 70f) return ValidationResult.Good;
            if (averageScore >= 55f) return ValidationResult.Acceptable;
            if (averageScore >= 35f) return ValidationResult.Poor;
            return ValidationResult.Critical;
        }

        private List<string> GenerateRecommendations(ValidationResults results)
        {
            var recommendations = new List<string>();

            // 分析各场景表现
            foreach (var scenario in results.scenarios)
            {
                if (scenario.result == ValidationResult.Poor || scenario.result == ValidationResult.Critical)
                {
                    recommendations.Add($"Consider reducing coin count for '{scenario.scenario.scenarioName}' scenario");
                }

                if (scenario.metrics.fpsAverage < scenario.scenario.targetFPS * 0.8f)
                {
                    recommendations.Add($"FPS below target in '{scenario.scenario.scenarioName}' - optimize animations");
                }

                if (scenario.metrics.memoryPeak > memoryWarningThreshold)
                {
                    recommendations.Add($"High memory usage in '{scenario.scenario.scenarioName}' - implement better pooling");
                }
            }

            // 设备特定建议
            var deviceClass = CurrentDeviceClass;
            switch (deviceClass)
            {
                case DevicePerformanceClass.LowEnd:
                    recommendations.Add("Low-end device detected: Use minimum quality settings");
                    recommendations.Add("Consider limiting concurrent coins to 15-20");
                    break;
                case DevicePerformanceClass.MidRange:
                    recommendations.Add("Mid-range device: Use medium quality settings");
                    recommendations.Add("Optimal coin count: 30-50");
                    break;
                case DevicePerformanceClass.HighEnd:
                    recommendations.Add("High-end device: Can use maximum quality settings");
                    recommendations.Add("Can handle 50+ concurrent coins");
                    break;
            }

            return recommendations;
        }

        #endregion

        #region Helper Methods

        private DeviceInfo CollectDeviceInfo()
        {
            return new DeviceInfo
            {
                deviceModel = SystemInfo.deviceModel,
                deviceName = SystemInfo.deviceName,
                deviceType = SystemInfo.deviceType.ToString(),
                deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier,
                operatingSystem = SystemInfo.operatingSystem,
                processorType = SystemInfo.processorType,
                processorCount = SystemInfo.processorCount,
                processorFrequency = SystemInfo.processorFrequency,
                graphicsDeviceName = SystemInfo.graphicsDeviceName,
                graphicsDeviceType = SystemInfo.graphicsDeviceType.ToString(),
                graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion,
                graphicsMemorySize = SystemInfo.graphicsMemorySize,
                systemMemorySize = SystemInfo.systemMemorySize,
                deviceClass = CurrentDeviceClass.ToString()
            };
        }

        private void CreateTestCoins(int count)
        {
            var coinPrefab = Resources.Load<GameObject>("TestCoin");
            if (coinPrefab == null)
            {
                // 创建简单的测试金币
                coinPrefab = CreateSimpleTestCoin();
            }

            for (int i = 0; i < count; i++)
            {
                var coin = Instantiate(coinPrefab);
                coin.name = $"TestCoin_{i}";
                coin.tag = "TestCoin";
                coin.transform.position = UnityEngine.Random.insideUnitSphere * 10f;
            }
        }

        private GameObject CreateSimpleTestCoin()
        {
            var coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "TestCoin";
            coin.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);

            var renderer = coin.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Standard"));
            material.color = Color.yellow;
            renderer.material = material;

            return coin;
        }

        private float CalculateAverage(Queue<float> values)
        {
            if (values.Count == 0) return 0f;
            float sum = 0f;
            foreach (var value in values) sum += value;
            return sum / values.Count;
        }

        private float CalculateMin(Queue<float> values)
        {
            if (values.Count == 0) return 0f;
            float min = float.MaxValue;
            foreach (var value in values) if (value < min) min = value;
            return min;
        }

        private float CalculateMax(Queue<float> values)
        {
            if (values.Count == 0) return 0f;
            float max = float.MinValue;
            foreach (var value in values) if (value > max) max = value;
            return max;
        }

        private float CalculateStandardDeviation(Queue<float> values)
        {
            if (values.Count < 2) return 0f;

            var average = CalculateAverage(values);
            float sumOfSquares = 0f;

            foreach (var value in values)
            {
                var diff = value - average;
                sumOfSquares += diff * diff;
            }

            return Mathf.Sqrt(sumOfSquares / values.Count);
        }

        private float CalculateFPSScore(float actualFPS, float targetFPS)
        {
            if (targetFPS <= 0) return 0f;
            var ratio = actualFPS / targetFPS;
            return Mathf.Clamp01(ratio) * 100f;
        }

        private float CalculateStabilityScore(float standardDeviation)
        {
            // 标准差越小，稳定性越好
            if (standardDeviation <= 2f) return 100f;
            if (standardDeviation <= 5f) return 80f;
            if (standardDeviation <= 10f) return 60f;
            if (standardDeviation <= 15f) return 40f;
            return 20f;
        }

        private float CalculateMemoryScore(float memoryUsage)
        {
            if (memoryUsage <= 100f) return 100f;
            if (memoryUsage <= 200f) return 80f;
            if (memoryUsage <= 400f) return 60f;
            if (memoryUsage <= 800f) return 40f;
            return 20f;
        }

        private float GetResultScore(ValidationResult result)
        {
            switch (result)
            {
                case ValidationResult.Excellent: return 95f;
                case ValidationResult.Good: return 80f;
                case ValidationResult.Acceptable: return 65f;
                case ValidationResult.Poor: return 40f;
                case ValidationResult.Critical: return 15f;
                default: return 0f;
            }
        }

        #endregion

        #region Public API

        public ValidationReport GenerateValidationReport()
        {
            if (_currentValidation == null)
            {
                return new ValidationReport
                {
                    isValid = false,
                    errorMessage = "No validation data available"
                };
            }

            return new ValidationReport
            {
                isValid = _currentValidation.overallResult != ValidationResult.Error,
                validationResults = _currentValidation,
                deviceInfo = _currentValidation.deviceInfo,
                overallScore = GetResultScore(_currentValidation.overallResult),
                recommendations = _currentValidation.recommendations,
                generatedAt = DateTime.UtcNow
            };
        }

        public DevicePerformanceProfile GetDeviceProfile()
        {
            var capabilities = _deviceDetector?.DetectedCapabilities;
            if (capabilities == null)
            {
                return new DevicePerformanceProfile
                {
                    deviceClass = DevicePerformanceClass.Unknown,
                    profileStatus = "Device detection not completed"
                };
            }

            var profile = new DevicePerformanceProfile
            {
                deviceInfo = CollectDeviceInfo(),
                deviceCapabilities = capabilities,
                deviceClass = capabilities.PerformanceTier,
                recommendedSettings = _deviceDetector.GetRecommendedQualitySettings(),
                validationHistory = _validationHistory
            };

            // 添加最新的验证结果
            if (_currentValidation != null)
            {
                profile.latestValidation = _currentValidation;
                profile.profileStatus = _currentValidation.overallResult.ToString();
            }

            return profile;
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class DeviceClassificationConfig
    {
        public DeviceSpecs lowEndSpecs;
        public DeviceSpecs midRangeSpecs;
        public DeviceSpecs highEndSpecs;
    }

    [System.Serializable]
    public class DeviceSpecs
    {
        public float minCPUScore;
        public float minMemoryScore;
        public float minGPUScore;
        public float minStorageScore;
        public float targetFPS;
        public int maxConcurrentCoins;
        public QualityLevel qualityLevel;
    }

    [System.Serializable]
    public class ValidationScenario
    {
        public string scenarioName;
        public string description;
        public int coinCount;
        public float duration;
        public float targetFPS;
        public StressLevel stressLevel;
    }

    public enum StressLevel
    {
        Minimal,
        Light,
        Moderate,
        Heavy,
        Peak
    }

    [System.Serializable]
    public class ValidationProgress
    {
        public int currentScenario;
        public int totalScenarios;
        public string scenarioName;
        public float progressPercentage;
    }

    [System.Serializable]
    public class ValidationResults
    {
        public DateTime validationStartTime;
        public DateTime validationEndTime;
        public DeviceInfo deviceInfo;
        public List<ScenarioResults> scenarios;
        public ValidationResult overallResult;
        public List<string> recommendations;
    }

    [System.Serializable]
    public class ScenarioResults
    {
        public ValidationScenario scenario;
        public DateTime startTime;
        public DateTime endTime;
        public ScenarioMetrics metrics;
        public ValidationResult result;
        public string error;
    }

    [System.Serializable]
    public class ScenarioMetrics
    {
        public float fpsAverage;
        public float fpsMin;
        public float fpsMax;
        public float fpsStandardDeviation;
        public float frameTimeAverage;
        public float frameTimeMax;
        public float memoryAverage;
        public float memoryPeak;
        public int sampleCount;
    }

    [System.Serializable]
    public class DeviceInfo
    {
        public string deviceModel;
        public string deviceName;
        public string deviceType;
        public string deviceUniqueIdentifier;
        public string operatingSystem;
        public string processorType;
        public int processorCount;
        public float processorFrequency;
        public string graphicsDeviceName;
        public string graphicsDeviceType;
        public string graphicsDeviceVersion;
        public int graphicsMemorySize;
        public int systemMemorySize;
        public string deviceClass;
    }

    public enum ValidationResult
    {
        Excellent,
        Good,
        Acceptable,
        Poor,
        Critical,
        Error
    }

    [System.Serializable]
    public class ValidationReport
    {
        public bool isValid;
        public string errorMessage;
        public ValidationResults validationResults;
        public DeviceInfo deviceInfo;
        public float overallScore;
        public List<string> recommendations;
        public DateTime generatedAt;
    }

    #endregion
}