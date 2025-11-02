using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CoinAnimation.Core;
using CoinAnimation.Animation;
using CoinAnimation.Core.AdaptiveQuality;

namespace CoinAnimation.AdaptiveQuality
{
    /// <summary>
    /// 自适应质量管理器
    /// Story 2.1 Task 2.2 - 根据设备能力和性能指标动态调整动画质量
    /// </summary>
    public class AdaptiveQualityManager : MonoBehaviour
    {
        #region Configuration

        [Header("Quality Management Settings")]
        [SerializeField] private bool enableAdaptiveQuality = true;
        [SerializeField] private bool autoAdjustQuality = true;
        [SerializeField] private float adjustmentInterval = 2.0f;
        [SerializeField] private int performanceHistorySize = 10;

        [Header("Performance Thresholds")]
        [SerializeField] private float targetFPS = 60f;
        [SerializeField] private float fpsWarningThreshold = 55f;
        [SerializeField] private float fpsCriticalThreshold = 45f;
        [SerializeField] private float memoryWarningThresholdMB = 100f;
        [SerializeField] private float memoryCriticalThresholdMB = 150f;

        [Header("Quality Scaling Factors")]
        [SerializeField] private float coinCountScaleFactorLow = 0.6f;
        [SerializeField] private float coinCountScaleFactorMedium = 0.8f;
        [SerializeField] private float coinCountScaleFactorHigh = 1.0f;
        [SerializeField] private float effectIntensityScaleLow = 0.3f;
        [SerializeField] private float effectIntensityScaleMedium = 0.7f;
        [SerializeField] private float effectIntensityScaleHigh = 1.0f;

        [Header("Quality Presets")]
        [SerializeField] private QualityPreset minimumQuality;
        [SerializeField] private QualityPreset lowQuality;
        [SerializeField] private QualityPreset mediumQuality;
        [SerializeField] private QualityPreset highQuality;

        #endregion

        #region Private Fields

        // 组件引用
        private DeviceCapabilityDetector _deviceDetector;
        private PerformanceMonitor _performanceMonitor;
        private CoinAnimationManager _animationManager;
        private CoinObjectPool _objectPool;

        // 当前状态
        private QualityPreset _currentQuality;
        private DevicePerformanceTier _deviceTier;
        private Core.QualityLevel _currentQualityLevel = Core.QualityLevel.Medium;

        // 性能历史
        private readonly Queue<float> _fpsHistory = new Queue<float>();
        private readonly Queue<float> _memoryHistory = new Queue<float>();
        private readonly Queue<int> _activeCoinsHistory = new Queue<int>();

        // 调整状态
        private float _lastAdjustmentTime = 0f;
        private int _consecutivePoorPerformanceFrames = 0;
        private int _consecutiveGoodPerformanceFrames = 0;
        private bool _qualityAdjustmentInProgress = false;

        // 性能统计
        private QualityAdjustmentStats _adjustmentStats = new QualityAdjustmentStats();

        #endregion

        #region Properties

        public QualityPreset CurrentQuality => _currentQuality;
        public Core.QualityLevel CurrentQualityLevel => _currentQualityLevel;
        public DevicePerformanceTier DeviceTier => _deviceTier;
        public bool IsQualityAdjustmentInProgress => _qualityAdjustmentInProgress;
        public QualityAdjustmentStats AdjustmentStats => _adjustmentStats;

        #endregion

        #region Events

        public event Action<Core.QualityLevel, Core.QualityLevel> OnQualityLevelChanged;
        public event Action<QualityPreset> OnQualitySettingsUpdated;
        public event Action<string> OnQualityAdjustmentInitiated;
        public event Action<QualityAdjustmentResult> OnQualityAdjustmentCompleted;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeQualityPresets();
        }

        private void Start()
        {
            FindSystemComponents();
            StartCoroutine(InitializationWorkflow());
        }

        private void Update()
        {
            if (!enableAdaptiveQuality) return;

            UpdatePerformanceHistory();
            
            if (autoAdjustQuality && !_qualityAdjustmentInProgress)
            {
                CheckForQualityAdjustment();
            }
        }

        #endregion

        #region Initialization

        private void InitializeQualityPresets()
        {
            // 初始化质量预设
            minimumQuality = QualityPreset.GetMinimumPreset();
            lowQuality = QualityPreset.GetLowPreset();
            mediumQuality = QualityPreset.GetMediumPreset();
            highQuality = QualityPreset.GetHighPreset();

            _currentQuality = mediumQuality;
        }

        private void FindSystemComponents()
        {
            _deviceDetector = FindObjectOfType<DeviceCapabilityDetector>();
            _performanceMonitor = FindObjectOfType<PerformanceMonitor>();
            _animationManager = FindObjectOfType<CoinAnimationManager>();
            _objectPool = FindObjectOfType<CoinObjectPool>();

            if (_deviceDetector != null)
            {
                _deviceDetector.OnDetectionComplete += OnDeviceDetectionComplete;
                _deviceDetector.OnBenchmarkComplete += OnDeviceBenchmarkComplete;
            }

            //if (_performanceMonitor != null)
            //{
            //    _performanceMonitor.OnPerformanceAlert += OnPerformanceAlert;
            //}
        }

        private IEnumerator InitializationWorkflow()
        {
            Debug.Log("[AdaptiveQualityManager] Starting initialization...");

            // 等待设备检测完成
            if (_deviceDetector != null && !_deviceDetector.DetectionComplete)
            {
                Debug.Log("[AdaptiveQualityManager] Waiting for device detection...");
                yield return new WaitUntil(() => _deviceDetector.DetectionComplete);
            }

            // 设置初始质量等级
            SetInitialQualityLevel();

            // 应用初始质量设置
            yield return StartCoroutine(ApplyQualitySettings(_currentQuality));

            Debug.Log($"[AdaptiveQualityManager] Initialization complete. Initial quality: {_currentQualityLevel}");
        }

        private void SetInitialQualityLevel()
        {
            if (_deviceDetector != null)
            {
                _deviceTier = _deviceDetector.PerformanceTier;
            }
            else
            {
                // 如果没有设备检测器，基于系统信息做简单判断
                _deviceTier = DetermineDeviceTierFromSystemInfo();
            }

            _currentQualityLevel = _deviceTier switch
            {
                DevicePerformanceTier.HighEnd => Core.QualityLevel.High,
                DevicePerformanceTier.MidRange => Core.QualityLevel.Medium,
                DevicePerformanceTier.LowEnd => Core.QualityLevel.Low,
                _ => Core.QualityLevel.Minimum
            };

            _currentQuality = GetQualityPresetForLevel(_currentQualityLevel);
        }

        private DevicePerformanceTier DetermineDeviceTierFromSystemInfo()
        {
            var memoryMB = SystemInfo.systemMemorySize;
            var processorCount = SystemInfo.processorCount;

            if (memoryMB >= 16384 && processorCount >= 8)
                return DevicePerformanceTier.HighEnd;
            else if (memoryMB >= 8192 && processorCount >= 4)
                return DevicePerformanceTier.MidRange;
            else if (memoryMB >= 4096 && processorCount >= 2)
                return DevicePerformanceTier.LowEnd;
            else
                return DevicePerformanceTier.Minimum;
        }

        #endregion

        #region Performance Monitoring

        private void UpdatePerformanceHistory()
        {
            float currentFPS = 60f; // 默认值
            float currentMemory = 0f;
            int activeCoins = 0;

            // 从性能监控器获取数据
            if (_performanceMonitor != null)
            {
                var metrics = _performanceMonitor.GetCurrentMetrics();
                currentFPS = metrics.FrameRate;
                currentMemory = metrics.MemoryUsageMB;
                activeCoins = metrics.ActiveCoinsCount;
            }
            else
            {
                // 如果没有性能监控器，使用简单计算
                currentFPS = 1f / Time.deltaTime;
                currentMemory = GC.GetTotalMemory(false) / (1024f * 1024f);
            }

            // 从对象池获取活跃金币数
            if (_objectPool != null)
            {
                activeCoins = _objectPool.ActiveCoinCount;
            }

            // 更新历史记录
            _fpsHistory.Enqueue(currentFPS);
            while (_fpsHistory.Count > performanceHistorySize)
                _fpsHistory.Dequeue();

            _memoryHistory.Enqueue(currentMemory);
            while (_memoryHistory.Count > performanceHistorySize)
                _memoryHistory.Dequeue();

            _activeCoinsHistory.Enqueue(activeCoins);
            while (_activeCoinsHistory.Count > performanceHistorySize)
                _activeCoinsHistory.Dequeue();

            // 更新性能计数器
            UpdatePerformanceCounters(currentFPS, currentMemory);
        }

        private void UpdatePerformanceCounters(float fps, float memoryMB)
        {
            if (fps < fpsCriticalThreshold || memoryMB > memoryCriticalThresholdMB)
            {
                _consecutivePoorPerformanceFrames++;
                _consecutiveGoodPerformanceFrames = 0;
            }
            else if (fps > fpsWarningThreshold && memoryMB < memoryWarningThresholdMB)
            {
                _consecutiveGoodPerformanceFrames++;
                _consecutivePoorPerformanceFrames = 0;
            }
            else
            {
                _consecutivePoorPerformanceFrames = 0;
                _consecutiveGoodPerformanceFrames = 0;
            }
        }

        #endregion

        #region Quality Adjustment Logic

        private void CheckForQualityAdjustment()
        {
            if (Time.time < _lastAdjustmentTime + adjustmentInterval)
                return;

            var adjustmentDecision = EvaluateQualityAdjustmentNeed();
            
            if (adjustmentDecision.ShouldAdjust)
            {
                StartCoroutine(PerformQualityAdjustment(adjustmentDecision));
            }
        }

        private QualityAdjustmentDecision EvaluateQualityAdjustmentNeed()
        {
            var decision = new QualityAdjustmentDecision
            {
                ShouldAdjust = false,
                Reason = string.Empty,
                SuggestedQualityLevel = _currentQualityLevel
            };

            var avgFPS = CalculateAverageFPS();
            var avgMemory = CalculateAverageMemory();

            // 检查是否需要降低质量
            if (_consecutivePoorPerformanceFrames >= 10) // 连续10帧性能不佳
            {
                decision.ShouldAdjust = true;
                decision.SuggestedQualityLevel = GetLowerQualityLevel(_currentQualityLevel);
                decision.Reason = $"Poor performance detected: {avgFPS:F1} FPS, {avgMemory:F1}MB memory";
            }
            // 检查是否可以提升质量
            else if (_consecutiveGoodPerformanceFrames >= 30 && _currentQualityLevel < GetMaximumQualityLevelForDevice())
            {
                decision.ShouldAdjust = true;
                decision.SuggestedQualityLevel = GetHigherQualityLevel(_currentQualityLevel);
                decision.Reason = $"Good performance sustained: {avgFPS:F1} FPS, {avgMemory:F1}MB memory";
            }

            return decision;
        }

        private IEnumerator PerformQualityAdjustment(QualityAdjustmentDecision decision)
        {
            if (_qualityAdjustmentInProgress)
                yield break;

            _qualityAdjustmentInProgress = true;
            _lastAdjustmentTime = Time.time;

            Debug.Log($"[AdaptiveQualityManager] Starting quality adjustment: {_currentQualityLevel} -> {decision.SuggestedQualityLevel}");
            Debug.Log($"[AdaptiveQualityManager] Reason: {decision.Reason}");

            OnQualityAdjustmentInitiated?.Invoke(decision.Reason);

            var oldQualityLevel = _currentQualityLevel;
            var newQualityPreset = GetQualityPresetForLevel(decision.SuggestedQualityLevel);

            // 执行质量调整
            yield return StartCoroutine(ApplyQualitySettings(newQualityPreset));

            // 更新状态
            _currentQualityLevel = decision.SuggestedQualityLevel;
            _currentQuality = newQualityPreset;

            // 重置计数器
            _consecutivePoorPerformanceFrames = 0;
            _consecutiveGoodPerformanceFrames = 0;

            // 更新统计
            _adjustmentStats.TotalAdjustments++;
            if (decision.SuggestedQualityLevel < oldQualityLevel)
                _adjustmentStats.Downgrades++;
            else
                _adjustmentStats.Upgrades++;

            _adjustmentStats.LastAdjustmentTime = DateTime.UtcNow;

            // 触发事件
            OnQualityLevelChanged?.Invoke(oldQualityLevel, _currentQualityLevel);
            OnQualitySettingsUpdated?.Invoke(_currentQuality);

            var result = new QualityAdjustmentResult
            {
                OldQualityLevel = oldQualityLevel,
                NewQualityLevel = _currentQualityLevel,
                Reason = decision.Reason,
                AdjustmentTime = DateTime.UtcNow,
                Success = true
            };

            OnQualityAdjustmentCompleted?.Invoke(result);

            Debug.Log($"[AdaptiveQualityManager] Quality adjustment completed: {oldQualityLevel} -> {_currentQualityLevel}");

            _qualityAdjustmentInProgress = false;
        }

        private IEnumerator ApplyQualitySettings(QualityPreset qualityPreset)
        {
            Debug.Log($"[AdaptiveQualityManager] Applying quality settings for {qualityPreset.Name}");

            // 调整对象池大小
            if (_objectPool != null)
            {
                var newPoolSize = Mathf.RoundToInt(qualityPreset.MaxCoinCount * 1.2f); // 20%缓冲
                if (_objectPool.MaxPoolSize != newPoolSize)
                {
                    Debug.Log($"[AdaptiveQualityManager] Adjusting pool size: {_objectPool.MaxPoolSize} -> {newPoolSize}");
                    //_objectPool.SetMaxPoolSize(newPoolSize);
                    yield return null; // 等待一帧
                }
            }

            // 调整动画参数
            if (_animationManager != null)
            {
                // 这里需要根据实际的 CoinAnimationManager API 调整
                // 例如：设置最大并发动画数、动画质量等
                yield return null;
            }

            // 调整特效设置
            ApplyEffectSettings(qualityPreset);

            // 调整渲染设置
            ApplyRenderSettings(qualityPreset);

            yield return null;
        }

        private void ApplyEffectSettings(QualityPreset qualityPreset)
        {
            // 粒子系统设置
            var particleSystems = FindObjectsOfType<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                if (ps.gameObject.name.Contains("Coin"))
                {
                    var main = ps.main;
                    main.maxParticles = Mathf.RoundToInt(main.maxParticles * qualityPreset.EffectIntensityMultiplier);
                }
            }

            // 其他特效设置...
        }

        private void ApplyRenderSettings(QualityPreset qualityPreset)
        {
            //// Unity质量设置
            //QualitySettings.SetQualityLevel(qualityPreset.UnityQualityLevel, true);

            //// 抗锯齿设置
            //QualitySettings.antiAliasing = qualityPreset.AntiAliasing;

            //// 阴影设置
            //QualitySettings.shadows = qualityPreset.ShadowQuality;
            //QualitySettings.shadowResolution = qualityPreset.ShadowResolution;
            //QualitySettings.shadowDistance = qualityPreset.ShadowDistance;
        }

        #endregion

        #region Quality Level Management

        private Core.QualityLevel GetLowerQualityLevel(Core.QualityLevel currentLevel)
        {
            return currentLevel switch
            {
                Core.QualityLevel.High => Core.QualityLevel.Medium,
                Core.QualityLevel.Medium => Core.QualityLevel.Low,
                Core.QualityLevel.Low => Core.QualityLevel.Minimum,
                _ => Core.QualityLevel.Minimum
            };
        }

        private Core.QualityLevel GetHigherQualityLevel(Core.QualityLevel currentLevel)
        {
            return currentLevel switch
            {
                Core.QualityLevel.Minimum => Core.QualityLevel.Low,
                Core.QualityLevel.Low => Core.QualityLevel.Medium,
                Core.QualityLevel.Medium => Core.QualityLevel.High,
                _ => Core.QualityLevel.High
            };
        }

        private Core.QualityLevel GetMaximumQualityLevelForDevice()
        {
            return _deviceTier switch
            {
                DevicePerformanceTier.HighEnd => Core.QualityLevel.High,
                DevicePerformanceTier.MidRange => Core.QualityLevel.Medium,
                DevicePerformanceTier.LowEnd => Core.QualityLevel.Low,
                _ => Core.QualityLevel.Minimum
            };
        }

        private QualityPreset GetQualityPresetForLevel(Core.QualityLevel level)
        {
            return level switch
            {
                Core.QualityLevel.Minimum => minimumQuality,
                Core.QualityLevel.Low => lowQuality,
                Core.QualityLevel.Medium => mediumQuality,
                Core.QualityLevel.High => highQuality,
                _ => mediumQuality
            };
        }

        #endregion

        #region Performance Calculations

        private float CalculateAverageFPS()
        {
            if (_fpsHistory.Count == 0) return targetFPS;

            float sum = 0f;
            foreach (float fps in _fpsHistory)
                sum += fps;

            return sum / _fpsHistory.Count;
        }

        private float CalculateAverageMemory()
        {
            if (_memoryHistory.Count == 0) return 0f;

            float sum = 0f;
            foreach (float memory in _memoryHistory)
                sum += memory;

            return sum / _memoryHistory.Count;
        }

        #endregion

        #region Event Handlers

        private void OnDeviceDetectionComplete(DeviceCapabilities capabilities)
        {
            Debug.Log("[AdaptiveQualityManager] Device detection completed");
            _deviceTier = capabilities.PerformanceTier;
        }

        private void OnDeviceBenchmarkComplete(BenchmarkResults results)
        {
            Debug.Log($"[AdaptiveQualityManager] Device benchmark completed with score: {results.OverallScore:F1}");
            
            // 根据基准测试结果调整质量等级
            var suggestedQualityLevel = results.OverallScore switch
            {
                >= 80f => Core.QualityLevel.High,
                >= 60f => Core.QualityLevel.Medium,
                >= 40f => Core.QualityLevel.Low,
                _ => Core.QualityLevel.Minimum
            };

            if (suggestedQualityLevel != _currentQualityLevel)
            {
                Debug.Log($"[AdaptiveQualityManager] Benchmark suggests quality change: {_currentQualityLevel} -> {suggestedQualityLevel}");
                StartCoroutine(PerformQualityAdjustment(new QualityAdjustmentDecision
                {
                    ShouldAdjust = true,
                    SuggestedQualityLevel = suggestedQualityLevel,
                    Reason = "Based on benchmark results"
                }));
            }
        }

        private void OnPerformanceAlert(PerformanceAlert alert)
        {
            Debug.LogWarning($"[AdaptiveQualityManager] Performance alert received: {alert.Type} - {alert.Message}");

            if (alert.Type == PerformanceAlertType.Critical)
            {
                // 立即降低质量
                var lowerLevel = GetLowerQualityLevel(_currentQualityLevel);
                if (lowerLevel != _currentQualityLevel)
                {
                    StartCoroutine(PerformQualityAdjustment(new QualityAdjustmentDecision
                    {
                        ShouldAdjust = true,
                        SuggestedQualityLevel = lowerLevel,
                        Reason = $"Critical performance alert: {alert.Message}"
                    }));
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// 手动设置质量等级
        /// </summary>
        public void SetQualityLevel(Core.QualityLevel qualityLevel)
        {
            if (_qualityAdjustmentInProgress) return;

            var newPreset = GetQualityPresetForLevel(qualityLevel);
            StartCoroutine(PerformQualityAdjustment(new QualityAdjustmentDecision
            {
                ShouldAdjust = true,
                SuggestedQualityLevel = qualityLevel,
                Reason = "Manual quality adjustment"
            }));
        }

        /// <summary>
        /// 启用/禁用自适应质量
        /// </summary>
        public void SetAdaptiveQualityEnabled(bool enabled)
        {
            enableAdaptiveQuality = enabled;
            Debug.Log($"[AdaptiveQualityManager] Adaptive quality {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 启用/禁用自动调整
        /// </summary>
        public void SetAutoAdjustEnabled(bool enabled)
        {
            autoAdjustQuality = enabled;
            Debug.Log($"[AdaptiveQualityManager] Auto adjust {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 获取当前性能报告
        /// </summary>
        public QualityPerformanceReport GetPerformanceReport()
        {
            return new QualityPerformanceReport
            {
                CurrentQualityLevel = _currentQualityLevel,
                DeviceTier = _deviceTier,
                AverageFPS = CalculateAverageFPS(),
                AverageMemoryMB = CalculateAverageMemory(),
                AverageActiveCoins = _activeCoinsHistory.Count > 0 ? 
                    (float)_activeCoinsHistory.ToArray().Sum() / _activeCoinsHistory.Count : 0f,
                AdjustmentStats = _adjustmentStats,
                IsAutoAdjustEnabled = autoAdjustQuality,
                LastAdjustmentTime = _adjustmentStats.LastAdjustmentTime
            };
        }

        /// <summary>
        /// 重置调整统计
        /// </summary>
        public void ResetAdjustmentStats()
        {
            _adjustmentStats = new QualityAdjustmentStats();
            Debug.Log("[AdaptiveQualityManager] Adjustment statistics reset");
        }

        #endregion

        #region IAdaptiveQualityManager Implementation

        //void IAdaptiveQualityManager.SetQualityLevel(int qualityLevel)
        //{
        //    SetQualityLevel((Core.QualityLevel)qualityLevel);
        //}

        //void IAdaptiveQualityManager.SetAdaptiveQualityEnabled(bool enabled)
        //{
        //    SetAdaptiveQualityEnabled(enabled);
        //}

        //object IAdaptiveQualityManager.GetPerformanceReport()
        //{
        //    return GetPerformanceReport();
        //}

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class QualityPreset
    {
        public string Name;
        public Core.QualityLevel Level;
        public int MaxCoinCount;
        public float EffectIntensityMultiplier;
        public int UnityQualityLevel;
        public int AntiAliasing;
        public ShadowQuality ShadowQuality;
        public int ShadowResolution;
        public float ShadowDistance;
        public bool EnableVSync;
        public int TargetFrameRate;

        public static QualityPreset GetMinimumPreset()
        {
            return new QualityPreset
            {
                Name = "Minimum",
                Level =     Core.QualityLevel.Minimum,
                MaxCoinCount = 15,
                EffectIntensityMultiplier = 0.2f,
                UnityQualityLevel = 0,
                AntiAliasing = 0,
                //ShadowQuality = UnityEngine.QualitySettings.ShadowQuality.Disable,
                ShadowResolution = 0,
                ShadowDistance = 10f,
                EnableVSync = false,
                TargetFrameRate = 30
            };
        }

        public static QualityPreset GetLowPreset()
        {
            return new QualityPreset
            {
                Name = "Low",
                Level = Core.QualityLevel.Low,
                MaxCoinCount = 25,
                EffectIntensityMultiplier = 0.4f,
                UnityQualityLevel = 1,
                AntiAliasing = 0,
                //ShadowQuality = UnityEngine.QualitySettings.ShadowQuality.HardOnly,
                ShadowResolution = 1,
                ShadowDistance = 20f,
                EnableVSync = false,
                TargetFrameRate = 45
            };
        }

        public static QualityPreset GetMediumPreset()
        {
            return new QualityPreset
            {
                Name = "Medium",
                Level = Core.QualityLevel.Medium,
                MaxCoinCount = 50,
                EffectIntensityMultiplier = 0.7f,
                UnityQualityLevel = 2,
                AntiAliasing = 2,
                //ShadowQuality = UnityEngine.QualitySettings.ShadowQuality.All,
                ShadowResolution = 2,
                ShadowDistance = 40f,
                EnableVSync = true,
                TargetFrameRate = 60
            };
        }

        public static QualityPreset GetHighPreset()
        {
            return new QualityPreset
            {
                Name = "High",
                Level = Core.QualityLevel.High,
                MaxCoinCount = 100,
                EffectIntensityMultiplier = 1.0f,
                UnityQualityLevel = 3,
                AntiAliasing = 4,
                //ShadowQuality = UnityEngine.QualitySettings.ShadowQuality.All,
                //ShadowResolution = 3,
                ShadowDistance = 80f,
                EnableVSync = true,
                TargetFrameRate = 60
            };
        }
    }

    [System.Serializable]
    public class QualityAdjustmentStats
    {
        public int TotalAdjustments;
        public int Upgrades;
        public int Downgrades;
        public DateTime LastAdjustmentTime;
        public TimeSpan TotalAdjustmentTime;
    }

    [System.Serializable]
    public class QualityAdjustmentDecision
    {
        public bool ShouldAdjust;
        public Core.QualityLevel SuggestedQualityLevel;
        public string Reason;
        public float Confidence;
    }

    [System.Serializable]
    public class QualityAdjustmentResult
    {
        public Core.QualityLevel OldQualityLevel;
        public Core.QualityLevel NewQualityLevel;
        public string Reason;
        public DateTime AdjustmentTime;
        public bool Success;
        public float AdjustmentDuration;
    }

    [System.Serializable]
    public class QualityPerformanceReport
    {
        public Core.QualityLevel CurrentQualityLevel;
        public DevicePerformanceTier DeviceTier;
        public float AverageFPS;
        public float AverageMemoryMB;
        public float AverageActiveCoins;
        public QualityAdjustmentStats AdjustmentStats;
        public bool IsAutoAdjustEnabled;
        public DateTime LastAdjustmentTime;
    }

  
    #endregion
}