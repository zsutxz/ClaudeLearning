using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoinAnimation.Core;
using CoinAnimation.Animation;

namespace CoinAnimation.AdaptiveQuality
{
    /// <summary>
    /// 实时质量调整器
    /// Story 2.1 Task 2.3 - 基于实时性能指标进行动态质量调整
    /// </summary>
    public class RealTimeQualityAdjuster : MonoBehaviour
    {
        #region Configuration

        [Header("Real-Time Adjustment Settings")]
        [SerializeField] private bool enableRealTimeAdjustment = true;
        [SerializeField] private float monitoringInterval = 0.1f; // 100ms 监控间隔
        [SerializeField] private int smoothingWindowSize = 30; // 30帧平滑窗口
        [SerializeField] private bool enablePredictiveAdjustment = true;

        [Header("Performance Thresholds")]
        [SerializeField] private float criticalFPSThreshold = 40f;
        [SerializeField] private float warningFPSThreshold = 50f;
        [SerializeField] private float targetFPS = 60f;
        [SerializeField] private float criticalMemoryThresholdMB = 120f;
        [SerializeField] private float warningMemoryThresholdMB = 80f;
        [SerializeField] private float frameTimeSpikeThreshold = 50f; // ms

        [Header("Adjustment Sensitivity")]
        [SerializeField, Range(0.1f, 2f)] private float adjustmentSensitivity = 1.0f;
        [SerializeField] private int consecutiveFramesToTrigger = 5; // 连续5帧触发调整
        [SerializeField] private float minimumTimeBetweenAdjustments = 2.0f; // 最小调整间隔

        [Header("Dynamic Scaling Factors")]
        [SerializeField] private float coinCountReductionFactor = 0.8f;
        [SerializeField] private float effectIntensityReductionFactor = 0.6f;
        [SerializeField] private float animationQualityReductionFactor = 0.7f;

        #endregion

        #region Private Fields

        // 组件引用
        private AdaptiveQualityManager _qualityManager;
        private PerformanceMonitor _performanceMonitor;
        private CoinAnimationManager _animationManager;
        private CoinObjectPool _objectPool;

        // 性能监控数据
        private readonly Queue<float> _frameTimeHistory = new Queue<float>();
        private readonly Queue<float> _fpsHistory = new Queue<float>();
        private readonly Queue<float> _memoryHistory = new Queue<float>();
        private readonly Queue<int> _activeCoinsHistory = new Queue<int>();

        // 当前性能状态
        private RealTimePerformanceMetrics _currentMetrics;
        private PerformanceTrend _performanceTrend = PerformanceTrend.Stable;
        private QualityPressureLevel _currentPressureLevel = QualityPressureLevel.None;

        // 调整控制
        private float _lastAdjustmentTime = 0f;
        private int _consecutiveLowPerformanceFrames = 0;
        private int _consecutiveHighPerformanceFrames = 0;
        private bool _adjustmentInProgress = false;

        // 平滑和预测
        private float _smoothedFPS = 60f;
        private float _smoothedFrameTime = 16.67f; // 60fps = 16.67ms
        private float _smoothedMemoryMB = 0f;
        private PerformancePrediction _prediction = new PerformancePrediction();

        // 事件和统计
        private RealTimeAdjustmentStats _stats = new RealTimeAdjustmentStats();

        #endregion

        #region Properties

        public RealTimePerformanceMetrics CurrentMetrics => _currentMetrics;
        public PerformanceTrend PerformanceTrend => _performanceTrend;
        public QualityPressureLevel CurrentPressureLevel => _currentPressureLevel;
        public bool IsAdjustmentInProgress => _adjustmentInProgress;
        public RealTimeAdjustmentStats Stats => _stats;

        #endregion

        #region Events

        public event Action<RealTimePerformanceMetrics> OnPerformanceMetricsUpdated;
        public event Action<QualityPressureLevel, QualityPressureLevel> OnPressureLevelChanged;
        public event Action<RealTimeAdjustmentDecision> OnRealTimeAdjustment;
        public event Action<PerformanceTrend> OnPerformanceTrendChanged;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            FindSystemComponents();
            StartCoroutine(RealTimeMonitoringCoroutine());
        }

        private void Update()
        {
            if (!enableRealTimeAdjustment) return;

            UpdatePerformanceMetrics();
            AnalyzePerformanceTrend();
            EvaluateQualityPressure();
        }

        #endregion

        #region Component Discovery

        private void FindSystemComponents()
        {
            _qualityManager = FindObjectOfType<AdaptiveQualityManager>();
            _performanceMonitor = FindObjectOfType<PerformanceMonitor>();
            _animationManager = FindObjectOfType<CoinAnimationManager>();
            _objectPool = FindObjectOfType<CoinObjectPool>();

            Debug.Log("[RealTimeQualityAdjuster] Component discovery complete:");
            Debug.Log($"  Quality Manager: {_qualityManager != null}");
            Debug.Log($"  Performance Monitor: {_performanceMonitor != null}");
            Debug.Log($"  Animation Manager: {_animationManager != null}");
            Debug.Log($"  Object Pool: {_objectPool != null}");
        }

        #endregion

        #region Real-Time Monitoring

        private IEnumerator RealTimeMonitoringCoroutine()
        {
            while (enableRealTimeAdjustment)
            {
                CollectDetailedMetrics();
                UpdateSmoothedMetrics();
                
                if (enablePredictiveAdjustment)
                {
                    UpdatePerformancePrediction();
                }

                CheckForImmediateAdjustment();
                
                yield return new WaitForSeconds(monitoringInterval);
            }
        }

        private void CollectDetailedMetrics()
        {
            // 收集帧时间
            float frameTime = Time.unscaledDeltaTime * 1000f; // 转换为毫秒
            _frameTimeHistory.Enqueue(frameTime);
            while (_frameTimeHistory.Count > smoothingWindowSize)
                _frameTimeHistory.Dequeue();

            // 收集FPS
            float fps = 1f / Time.unscaledDeltaTime;
            _fpsHistory.Enqueue(fps);
            while (_fpsHistory.Count > smoothingWindowSize)
                _fpsHistory.Dequeue();

            // 收集内存使用
            float memoryMB = GC.GetTotalMemory(false) / (1024f * 1024f);
            _memoryHistory.Enqueue(memoryMB);
            while (_memoryHistory.Count > smoothingWindowSize)
                _memoryHistory.Dequeue();

            // 收集活跃金币数
            int activeCoins = 0;
            if (_objectPool != null)
            {
                activeCoins = _objectPool.ActiveCoinCount;
            }
            _activeCoinsHistory.Enqueue(activeCoins);
            while (_activeCoinsHistory.Count > smoothingWindowSize)
                _activeCoinsHistory.Dequeue();

            // 更新当前指标
            _currentMetrics = new RealTimePerformanceMetrics
            {
                FrameTime = frameTime,
                FPS = fps,
                MemoryUsageMB = memoryMB,
                ActiveCoinsCount = activeCoins,
                Timestamp = DateTime.UtcNow,
                FrameTimeSpike = DetectFrameTimeSpike(),
                MemoryPressure = CalculateMemoryPressure()
            };
        }

        private void UpdateSmoothedMetrics()
        {
            _smoothedFrameTime = CalculateAverage(_frameTimeHistory);
            _smoothedFPS = CalculateAverage(_fpsHistory);
            _smoothedMemoryMB = CalculateAverage(_memoryHistory);
        }

        private void UpdatePerformanceMetrics()
        {
            // 检测帧时间尖峰
            if (_currentMetrics.FrameTimeSpike)
            {
                _stats.FrameTimeSpikeCount++;
                _consecutiveLowPerformanceFrames++;
                _consecutiveHighPerformanceFrames = 0;
            }
            else if (_currentMetrics.FPS > warningFPSThreshold && _currentMetrics.MemoryUsageMB < warningMemoryThresholdMB)
            {
                _consecutiveHighPerformanceFrames++;
                _consecutiveLowPerformanceFrames = 0;
            }
            else
            {
                _consecutiveLowPerformanceFrames = 0;
                _consecutiveHighPerformanceFrames = 0;
            }

            OnPerformanceMetricsUpdated?.Invoke(_currentMetrics);
        }

        #endregion

        #region Performance Analysis

        private void AnalyzePerformanceTrend()
        {
            var newTrend = DeterminePerformanceTrend();
            
            if (newTrend != _performanceTrend)
            {
                _performanceTrend = newTrend;
                _stats.TrendChanges++;
                OnPerformanceTrendChanged?.Invoke(_performanceTrend);
            }
        }

        private PerformanceTrend DeterminePerformanceTrend()
        {
            if (_fpsHistory.Count < smoothingWindowSize / 2)
                return PerformanceTrend.Stable;

            // 计算最近的性能趋势
            float recentAverage = CalculateAverage(_fpsHistory);
            float olderAverage = CalculateAverage(_fpsHistory, smoothingWindowSize / 2, smoothingWindowSize);
            
            float difference = recentAverage - olderAverage;
            float threshold = targetFPS * 0.1f; // 10% 变化阈值

            if (difference > threshold)
                return PerformanceTrend.Improving;
            else if (difference < -threshold)
                return PerformanceTrend.Degrading;
            else
                return PerformanceTrend.Stable;
        }

        private void EvaluateQualityPressure()
        {
            var newPressureLevel = CalculateQualityPressure();
            
            if (newPressureLevel != _currentPressureLevel)
            {
                var oldPressureLevel = _currentPressureLevel;
                _currentPressureLevel = newPressureLevel;
                
                Debug.Log($"[RealTimeQualityAdjuster] Quality pressure changed: {oldPressureLevel} -> {newPressureLevel}");
                OnPressureLevelChanged?.Invoke(oldPressureLevel, newPressureLevel);
            }
        }

        private QualityPressureLevel CalculateQualityPressure()
        {
            // 基于多个因素计算质量压力
            int pressureScore = 0;

            // FPS压力
            if (_smoothedFPS < criticalFPSThreshold)
                pressureScore += 3;
            else if (_smoothedFPS < warningFPSThreshold)
                pressureScore += 2;
            else if (_smoothedFPS < targetFPS * 0.9f)
                pressureScore += 1;

            // 内存压力
            if (_smoothedMemoryMB > criticalMemoryThresholdMB)
                pressureScore += 3;
            else if (_smoothedMemoryMB > warningMemoryThresholdMB)
                pressureScore += 2;
            else if (_smoothedMemoryMB > warningMemoryThresholdMB * 0.8f)
                pressureScore += 1;

            // 帧时间压力
            if (_smoothedFrameTime > frameTimeSpikeThreshold)
                pressureScore += 2;
            else if (_smoothedFrameTime > frameTimeSpikeThreshold * 0.7f)
                pressureScore += 1;

            // 性能趋势压力
            if (_performanceTrend == PerformanceTrend.Degrading)
                pressureScore += 1;
            else if (_performanceTrend == PerformanceTrend.Improving)
                pressureScore -= 1;

            return pressureScore switch
            {
                >= 6 => QualityPressureLevel.Critical,
                >= 4 => QualityPressureLevel.High,
                >= 2 => QualityPressureLevel.Medium,
                >= 1 => QualityPressureLevel.Low,
                _ => QualityPressureLevel.None
            };
        }

        #endregion

        #region Prediction System

        private void UpdatePerformancePrediction()
        {
            // 基于历史数据和当前趋势预测未来性能
            float predictedFPS = _smoothedFPS;
            float predictedMemory = _smoothedMemoryMB;

            // 应用趋势调整
            switch (_performanceTrend)
            {
                case PerformanceTrend.Degrading:
                    predictedFPS *= 0.9f; // 预测下降10%
                    predictedMemory *= 1.1f; // 预测上升10%
                    break;
                case PerformanceTrend.Improving:
                    predictedFPS *= 1.05f; // 预测提升5%
                    predictedMemory *= 0.95f; // 预测下降5%
                    break;
            }

            _prediction = new PerformancePrediction
            {
                PredictedFPS = predictedFPS,
                PredictedMemoryMB = predictedMemory,
                PredictedTrend = _performanceTrend,
                Confidence = CalculatePredictionConfidence(),
                PredictionTime = DateTime.UtcNow
            };
        }

        private float CalculatePredictionConfidence()
        {
            // 基于数据量和一致性计算预测置信度
            float dataConfidence = Mathf.Min((float)_fpsHistory.Count / smoothingWindowSize);
            float trendConfidence = _performanceTrend == PerformanceTrend.Stable ? 0.8f : 0.6f;
            
            return (dataConfidence + trendConfidence) / 2f;
        }

        #endregion

        #region Real-Time Adjustment Logic

        private void CheckForImmediateAdjustment()
        {
            if (_adjustmentInProgress || 
                Time.time < _lastAdjustmentTime + minimumTimeBetweenAdjustments)
                return;

            // 检查是否需要立即调整
            bool needsAdjustment = false;
            string adjustmentReason = string.Empty;

            // 严重性能问题需要立即调整
            if (_consecutiveLowPerformanceFrames >= consecutiveFramesToTrigger)
            {
                needsAdjustment = true;
                adjustmentReason = $"Consecutive low performance: {_consecutiveLowPerformanceFrames} frames";
            }
            // 预测性调整
            else if (enablePredictiveAdjustment && _prediction.PredictedFPS < warningFPSThreshold)
            {
                needsAdjustment = true;
                adjustmentReason = $"Predictive adjustment: FPS predicted to drop to {_prediction.PredictedFPS:F1}";
            }
            // 质量压力调整
            else if (_currentPressureLevel >= QualityPressureLevel.High)
            {
                needsAdjustment = true;
                adjustmentReason = $"High quality pressure: {_currentPressureLevel}";
            }

            if (needsAdjustment)
            {
                StartCoroutine(PerformRealTimeAdjustment(adjustmentReason));
            }
        }

        private IEnumerator PerformRealTimeAdjustment(string reason)
        {
            if (_adjustmentInProgress) yield break;

            _adjustmentInProgress = true;
            _lastAdjustmentTime = Time.time;

            Debug.Log($"[RealTimeQualityAdjuster] Starting real-time adjustment: {reason}");

            var startTime = Time.realtimeSinceStartup;
            var adjustmentDecision = CreateAdjustmentDecision(reason);

            OnRealTimeAdjustment?.Invoke(adjustmentDecision);

            // 执行调整
            bool success = false;
            if (_qualityManager != null)
            {
                // 通过 AdaptiveQualityManager 执行调整
                _qualityManager.SetQualityLevel(adjustmentDecision.TargetQualityLevel);
                success = true;
            }
            else
            {
                // 直接调整
                yield return StartCoroutine(DirectQualityAdjustment(adjustmentDecision));
                success = true;
            }

            var duration = Time.realtimeSinceStartup - startTime;

            // 更新统计
            _stats.TotalAdjustments++;
            if (success)
                _stats.SuccessfulAdjustments++;
            _stats.LastAdjustmentTime = DateTime.UtcNow;
            _stats.AverageAdjustmentTime = (_stats.AverageAdjustmentTime * (_stats.TotalAdjustments - 1) + duration) / _stats.TotalAdjustments;

            Debug.Log($"[RealTimeQualityAdjuster] Adjustment completed in {duration:F3}s. Success: {success}");

            _adjustmentInProgress = false;
        }

        private RealTimeAdjustmentDecision CreateAdjustmentDecision(string reason)
        {
            var decision = new RealTimeAdjustmentDecision
            {
                Reason = reason,
                TriggerPressureLevel = _currentPressureLevel,
                CurrentMetrics = _currentMetrics,
                Prediction = _prediction,
                Timestamp = DateTime.UtcNow
            };

            // 根据当前压力和性能确定目标质量等级
            if (_currentPressureLevel >= QualityPressureLevel.Critical || _smoothedFPS < criticalFPSThreshold)
            {
                decision.AdjustmentType = AdjustmentType.EmergencyDowngrade;
                decision.TargetQualityLevel = Core.QualityLevel.Minimum;
                decision.Intensity = 1.0f;
            }
            else if (_currentPressureLevel >= QualityPressureLevel.High)
            {
                decision.AdjustmentType = AdjustmentType.Downgrade;
                decision.TargetQualityLevel = Core.QualityLevel.Low;
                decision.Intensity = 0.8f;
            }
            else if (_currentPressureLevel >= QualityPressureLevel.Medium)
            {
                decision.AdjustmentType = AdjustmentType.ModerateDowngrade;
                decision.TargetQualityLevel = Core.QualityLevel.Low;
                decision.Intensity = 0.6f;
            }
            else if (_performanceTrend == PerformanceTrend.Improving && _smoothedFPS > targetFPS * 1.1f)
            {
                decision.AdjustmentType = AdjustmentType.Upgrade;
                decision.TargetQualityLevel = Core.QualityLevel.Medium;
                decision.Intensity = 0.4f;
            }
            else
            {
                decision.AdjustmentType = AdjustmentType.Monitor;
                decision.TargetQualityLevel = Core.QualityLevel.Medium;
                decision.Intensity = 0.0f;
            }

            return decision;
        }

        private IEnumerator DirectQualityAdjustment(RealTimeAdjustmentDecision decision)
        {
            Debug.Log($"[RealTimeQualityAdjuster] Performing direct adjustment: {decision.AdjustmentType}");

            // 直接调整组件参数
            if (_objectPool != null)
            {
                var currentMaxCoins = _objectPool.MaxPoolSize;
                var newMaxCoins = Mathf.RoundToInt(currentMaxCoins * (1f - decision.Intensity * coinCountReductionFactor));
                newMaxCoins = Mathf.Max(newMaxCoins, 10); // 最小10个金币

                if (newMaxCoins != currentMaxCoins)
                {
                    //_objectPool.SetMaxPoolSize(newMaxCoins);
                    Debug.Log($"[RealTimeQualityAdjuster] Adjusted max coins: {currentMaxCoins} -> {newMaxCoins}");
                }
            }

            if (_animationManager != null)
            {
                // 这里需要根据实际的 AnimationManager API 调整
                // 例如：减少同时动画的数量、降低动画质量等
            }

            // 应用特效调整
            ApplyRealTimeEffectAdjustments(decision.Intensity);

            yield return null;
        }

        private void ApplyRealTimeEffectAdjustments(float intensity)
        {
            // 调整粒子系统
            var particleSystems = FindObjectsOfType<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                if (ps.gameObject.name.Contains("Coin") || ps.gameObject.name.Contains("Effect"))
                {
                    var main = ps.main;
                    var currentMaxParticles = main.maxParticles;
                    var newMaxParticles = Mathf.RoundToInt(currentMaxParticles * (1f - intensity * effectIntensityReductionFactor));
                    main.maxParticles = Mathf.Max(newMaxParticles, 10);
                }
            }

            // 调整动画速度
            var animators = FindObjectsOfType<Animator>();
            foreach (var animator in animators)
            {
                if (animator.gameObject.name.Contains("Coin"))
                {
                    animator.speed = Mathf.Max(0.5f, 1f - intensity * 0.3f);
                }
            }
        }

        #endregion

        #region Utility Methods

        private float CalculateAverage(Queue<float> queue)
        {
            if (queue.Count == 0) return 0f;

            float sum = 0f;
            foreach (float value in queue)
                sum += value;

            return sum / queue.Count;
        }

        private float CalculateAverage(Queue<float> queue, int start, int end)
        {
            if (queue.Count == 0) return 0f;
            if (start >= queue.Count) return 0f;
            if (end > queue.Count) end = queue.Count;

            var array = queue.ToArray();
            float sum = 0f;
            int count = 0;

            for (int i = start; i < end; i++)
            {
                sum += array[i];
                count++;
            }

            return count > 0 ? sum / count : 0f;
        }

        private bool DetectFrameTimeSpike()
        {
            if (_frameTimeHistory.Count < 3) return false;

            var array = _frameTimeHistory.ToArray();
            float currentFrameTime = array[array.Length - 1];
            float recentAverage = 0f;

            // 计算最近几帧的平均值（不包括当前帧）
            for (int i = 1; i < Mathf.Min(4, array.Length); i++)
            {
                recentAverage += array[array.Length - 1 - i];
            }
            recentAverage /= (Mathf.Min(3, array.Length - 1));

            // 如果当前帧时间比最近平均值高出很多，认为是尖峰
            return currentFrameTime > recentAverage * 2f && currentFrameTime > frameTimeSpikeThreshold;
        }

        private float CalculateMemoryPressure()
        {
            if (_memoryHistory.Count < 2) return 0f;

            var array = _memoryHistory.ToArray();
            float current = array[array.Length - 1];
            float previous = array[array.Length - 2];

            return Mathf.Max(0f, current - previous); // 内存增长量
        }

        #endregion

        #region Public API

        /// <summary>
        /// 手动触发性能检查和调整
        /// </summary>
        public void TriggerPerformanceCheck()
        {
            if (!_adjustmentInProgress)
            {
                StartCoroutine(PerformRealTimeAdjustment("Manual trigger"));
            }
        }

        /// <summary>
        /// 启用/禁用实时调整
        /// </summary>
        public void SetRealTimeAdjustmentEnabled(bool enabled)
        {
            enableRealTimeAdjustment = enabled;
            Debug.Log($"[RealTimeQualityAdjuster] Real-time adjustment {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 获取详细性能报告
        /// </summary>
        public RealTimePerformanceReport GetDetailedReport()
        {
            return new RealTimePerformanceReport
            {
                CurrentMetrics = _currentMetrics,
                SmoothedMetrics = new RealTimePerformanceMetrics
                {
                    FrameTime = _smoothedFrameTime,
                    FPS = _smoothedFPS,
                    MemoryUsageMB = _smoothedMemoryMB,
                    Timestamp = DateTime.UtcNow
                },
                PerformanceTrend = _performanceTrend,
                CurrentPressureLevel = _currentPressureLevel,
                Prediction = _prediction,
                AdjustmentStats = _stats,
                IsRealTimeAdjustmentEnabled = enableRealTimeAdjustment,
                IsPredictiveAdjustmentEnabled = enablePredictiveAdjustment,
                MonitoringInterval = monitoringInterval
            };
        }

        /// <summary>
        /// 重置统计数据
        /// </summary>
        public void ResetStatistics()
        {
            _stats = new RealTimeAdjustmentStats();
            Debug.Log("[RealTimeQualityAdjuster] Statistics reset");
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class RealTimePerformanceMetrics
    {
        public float FrameTime;           // 当前帧时间 (ms)
        public float FPS;                 // 当前帧率
        public float MemoryUsageMB;       // 内存使用量 (MB)
        public int ActiveCoinsCount;      // 活跃金币数量
        public DateTime Timestamp;        // 时间戳
        public bool FrameTimeSpike;       // 是否检测到帧时间尖峰
        public float MemoryPressure;      // 内存压力 (增长量)
    }

    [System.Serializable]
    public class PerformancePrediction
    {
        public float PredictedFPS;        // 预测FPS
        public float PredictedMemoryMB;   // 预测内存使用
        public PerformanceTrend PredictedTrend; // 预测趋势
        public float Confidence;          // 预测置信度 (0-1)
        public DateTime PredictionTime;   // 预测时间
    }

    [System.Serializable]
    public class RealTimeAdjustmentStats
    {
        public int TotalAdjustments;
        public int SuccessfulAdjustments;
        public int FrameTimeSpikeCount;
        public int TrendChanges;
        public DateTime LastAdjustmentTime;
        public float AverageAdjustmentTime;
    }

    [System.Serializable]
    public class RealTimeAdjustmentDecision
    {
        public string Reason;                     // 调整原因
        public AdjustmentType AdjustmentType;     // 调整类型
        public Core.QualityLevel TargetQualityLevel;  // 目标质量等级
        public float Intensity;                   // 调整强度 (0-1)
        public QualityPressureLevel TriggerPressureLevel; // 触发压力等级
        public RealTimePerformanceMetrics CurrentMetrics; // 当前性能指标
        public PerformancePrediction Prediction;  // 性能预测
        public DateTime Timestamp;                // 决策时间
    }

    [System.Serializable]
    public class RealTimePerformanceReport
    {
        public RealTimePerformanceMetrics CurrentMetrics;
        public RealTimePerformanceMetrics SmoothedMetrics;
        public PerformanceTrend PerformanceTrend;
        public QualityPressureLevel CurrentPressureLevel;
        public PerformancePrediction Prediction;
        public RealTimeAdjustmentStats AdjustmentStats;
        public bool IsRealTimeAdjustmentEnabled;
        public bool IsPredictiveAdjustmentEnabled;
        public float MonitoringInterval;
    }

    
    #endregion
}