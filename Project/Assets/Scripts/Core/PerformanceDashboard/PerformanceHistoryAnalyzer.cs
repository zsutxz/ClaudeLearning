using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoinAnimation.Core
{
    /// <summary>
    /// 性能历史跟踪和趋势分析系统
    /// Story 2.1 Task 4.3 - 历史数据管理和趋势预测分析
    /// </summary>
    public class PerformanceHistoryAnalyzer : MonoBehaviour
    {
        #region Configuration

        [Header("History Settings")]
        [SerializeField] private bool enableHistoryTracking = true;
        [SerializeField] private int maxHistorySize = 10000; // 最大历史记录数量
        [SerializeField] private int historyRetentionDays = 30; // 历史数据保留天数
        [SerializeField] private float historyUpdateInterval = 1f; // 历史更新间隔

        [Header("Trend Analysis Settings")]
        [SerializeField] private bool enableTrendAnalysis = true;
        [SerializeField] private int trendAnalysisWindowSize = 60; // 趋势分析窗口大小
        [SerializeField] private float trendSignificanceThreshold = 0.1f; // 趋势显著性阈值
        [SerializeField] private int predictionHorizonMinutes = 10; // 预测时间范围（分钟）

        [Header("Data Aggregation")]
        [SerializeField] private bool enableDataAggregation = true;
        [SerializeField] private int aggregationLevel = 5; // 聚合级别（每N个数据点聚合）
        [SerializeField] private bool enableHourlySummary = true;
        [SerializeField] private bool enableDailySummary = true;

        [Header("Performance Baseline")]
        [SerializeField] private bool enableBaselineTracking = true;
        [SerializeField] private int baselineWindowSize = 100; // 基线窗口大小
        [SerializeField] private float baselineDeviationThreshold = 0.2f; // 基线偏差阈值

        [Header("Anomaly Detection")]
        [SerializeField] private bool enableAnomalyDetection = true;
        [SerializeField] private float anomalyThreshold = 2.0f; // 异常检测阈值（标准差倍数）
        [SerializeField] private int anomalyMinWindow = 30; // 异常检测最小窗口

        #endregion

        #region Private Fields

        // 历史数据存储
        private readonly Queue<PerformanceSnapshot> _rawHistory = new Queue<PerformanceSnapshot>();
        private readonly Queue<AggregatedDataPoint> _aggregatedHistory = new Queue<AggregatedDataPoint>();
        private readonly Dictionary<DateTime, HourlySummary> _hourlySummaries = new Dictionary<DateTime, HourlySummary>();
        private readonly Dictionary<DateTime, DailySummary> _dailySummaries = new Dictionary<DateTime, DailySummary>();

        // 趋势分析
        private readonly Dictionary<MetricType, TrendAnalysis> _trendAnalyses = new Dictionary<MetricType, TrendAnalysis>();
        private readonly Dictionary<MetricType, PerformanceBaseline> _baselines = new Dictionary<MetricType, PerformanceBaseline>();
        private readonly List<PerformanceAnomaly> _detectedAnomalies = new List<PerformanceAnomaly>();

        // 系统组件引用
        private PerformanceMonitor _performanceMonitor;
        private MemoryManagementSystem _memoryManager;
        private CoinObjectPool _objectPool;
        private AdvancedPerformanceDashboard _dashboard;

        // 分析状态
        private bool _historyTrackingActive = false;
        private DateTime _lastHistoryUpdate = DateTime.MinValue;
        private DateTime _lastTrendAnalysis = DateTime.MinValue;
        private DateTime _lastAnomalyCheck = DateTime.MinValue;

        // 统计数据
        private HistoryAnalysisStatistics _statistics = new HistoryAnalysisStatistics();

        #endregion

        #region Properties

        public bool IsHistoryTrackingActive => _historyTrackingActive;
        public int HistoryCount => _rawHistory.Count;
        public IReadOnlyList<PerformanceSnapshot> RawHistory => _rawHistory.ToList().AsReadOnly();
        public IReadOnlyList<PerformanceAnomaly> DetectedAnomalies => _detectedAnomalies.AsReadOnly();
        public HistoryAnalysisStatistics Statistics => _statistics;

        #endregion

        #region Events

        public event Action<PerformanceSnapshot> OnHistoryDataAdded;
        public event Action<TrendAnalysis> OnTrendAnalysisUpdated;
        public event Action<PerformanceAnomaly> OnAnomalyDetected;
        public event Action<BaselineDeviation> OnBaselineDeviationDetected;
        public event Action<PerformancePrediction> OnPredictionGenerated;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            FindSystemComponents();
            InitializeBaselines();
            StartCoroutine(HistoryTrackingCoroutine());
        }

        private void Update()
        {
            if (!enableHistoryTracking || !_historyTrackingActive) return;

            // 检查是否需要更新历史记录
            if (ShouldUpdateHistory())
            {
                UpdateHistory();
            }
        }

        private void OnDestroy()
        {
            CleanupHistoryData();
        }

        #endregion

        #region Initialization

        private void FindSystemComponents()
        {
            _performanceMonitor = FindObjectOfType<PerformanceMonitor>();
            _memoryManager = FindObjectOfType<MemoryManagementSystem>();
            _objectPool = FindObjectOfType<CoinObjectPool>();
            _dashboard = FindObjectOfType<AdvancedPerformanceDashboard>();

            Debug.Log($"[PerformanceHistoryAnalyzer] Components found: " +
                     $"PerfMonitor: {_performanceMonitor != null}, " +
                     $"MemoryMgr: {_memoryManager != null}, " +
                     $"ObjectPool: {_objectPool != null}");
        }

        private void InitializeBaselines()
        {
            foreach (MetricType metricType in Enum.GetValues(typeof(MetricType)))
            {
                _baselines[metricType] = new PerformanceBaseline
                {
                    MetricType = metricType,
                    CreatedAt = DateTime.UtcNow,
                    DataPoints = new List<float>(),
                    Mean = 0f,
                    StandardDeviation = 0f,
                    MinValue = float.MaxValue,
                    MaxValue = float.MinValue
                };

                _trendAnalyses[metricType] = new TrendAnalysis
                {
                    MetricType = metricType,
                    Trend = TrendDirection.Stable,
                    Confidence = 0f,
                    Slope = 0f,
                    LastUpdated = DateTime.UtcNow
                };
            }

            //Debug.Log($"[PerformanceHistoryAnalyzer] Initialized baselines for {Enum.GetValues(typeof(MetricType)).Length()} metrics");
        }

        #endregion

        #region History Tracking Coroutine

        private IEnumerator HistoryTrackingCoroutine()
        {
            _historyTrackingActive = true;

            while (enableHistoryTracking)
            {
                // 更新历史记录
                UpdateHistory();

                // 执行趋势分析
                if (enableTrendAnalysis)
                {
                    yield return StartCoroutine(PerformTrendAnalysis());
                }

                // 检测异常
                if (enableAnomalyDetection)
                {
                    CheckForAnomalies();
                }

                // 更新基准线
                if (enableBaselineTracking)
                {
                    UpdateBaselines();
                }

                // 清理旧数据
                CleanupOldData();

                yield return new WaitForSeconds(historyUpdateInterval);
            }
        }

        private void UpdateHistory()
        {
            var snapshot = CreatePerformanceSnapshot();
            
            // 添加到原始历史记录
            _rawHistory.Enqueue(snapshot);
            OnHistoryDataAdded?.Invoke(snapshot);

            // 保持历史记录大小
            while (_rawHistory.Count > maxHistorySize)
            {
                _rawHistory.Dequeue();
            }

            // 数据聚合
            if (enableDataAggregation)
            {
                ProcessDataAggregation(snapshot);
            }

            // 更新统计
            _statistics.TotalSnapshots++;
            _statistics.LastUpdate = DateTime.UtcNow;

            _lastHistoryUpdate = DateTime.UtcNow;
        }

        private PerformanceSnapshot CreatePerformanceSnapshot()
        {
            return new PerformanceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                FPS = GetFPS(),
                FrameTime = GetFrameTime(),
                MemoryUsageMB = GetMemoryUsage(),
                ActiveCoins = GetActiveCoinCount(),
                GCCount = GetGCCount(),
                AllocatedMemoryMB = GetAllocatedMemory(),
                PoolHitRate = GetPoolHitRate(),
                CPUUsage = GetCPUUsage(),
                GPUUsage = GetGPUUsage(),
                RenderTime = GetRenderTime()
            };
        }

        #endregion

        #region Trend Analysis

        private IEnumerator PerformTrendAnalysis()
        {
            if (_rawHistory.Count < trendAnalysisWindowSize)
            {
                yield break;
            }

            var analysisStartTime = DateTime.UtcNow;

            // 对每个指标进行趋势分析
            foreach (MetricType metricType in Enum.GetValues(typeof(MetricType)))
            {
                yield return StartCoroutine(AnalyzeMetricTrend(metricType));
            }

            var analysisDuration = DateTime.UtcNow - analysisStartTime;
            _statistics.LastTrendAnalysisDuration = analysisDuration;
            _statistics.TotalTrendAnalyses++;

            Debug.Log($"[PerformanceHistoryAnalyzer] Trend analysis completed in {analysisDuration.TotalMilliseconds:F2}ms");
        }

        private IEnumerator AnalyzeMetricTrend(MetricType metricType)
        {
            var data = ExtractMetricData(metricType, trendAnalysisWindowSize);
            if (data.Count < 10) yield break;

            // 计算线性回归
            var regression = CalculateLinearRegression(data);
            var slope = regression.Slope;
            var correlation = regression.Correlation;

            // 确定趋势方向
            var trend = DetermineTrendDirection(slope, correlation);

            // 计算置信度
            var confidence = CalculateTrendConfidence(correlation, data.Count);

            // 更新趋势分析
            var trendAnalysis = new TrendAnalysis
            {
                MetricType = metricType,
                Trend = trend,
                Slope = slope,
                Correlation = correlation,
                Confidence = confidence,
                DataPoints = data.Count,
                TimeWindow = TimeSpan.FromSeconds(data.Count),
                LastUpdated = DateTime.UtcNow,
                Prediction = GeneratePrediction(data, slope, confidence)
            };

            _trendAnalyses[metricType] = trendAnalysis;
            OnTrendAnalysisUpdated?.Invoke(trendAnalysis);

            yield return null;
        }

        private List<float> ExtractMetricData(MetricType metricType, int windowSize)
        {
            var data = new List<float>();
            var snapshots = _rawHistory.TakeLast(windowSize).ToList();

            foreach (var snapshot in snapshots)
            {
                var value = metricType switch
                {
                    MetricType.FPS => snapshot.FPS,
                    MetricType.FrameTime => snapshot.FrameTime,
                    MetricType.Memory => snapshot.MemoryUsageMB,
                    MetricType.ActiveCoins => (float)snapshot.ActiveCoins,
                    MetricType.GCCount => (float)snapshot.GCCount,
                    MetricType.AllocatedMemory => snapshot.AllocatedMemoryMB,
                    MetricType.PoolHitRate => snapshot.PoolHitRate,
                    MetricType.CPUUsage => snapshot.CPUUsage,
                    MetricType.GPUUsage => snapshot.GPUUsage,
                    MetricType.RenderTime => snapshot.RenderTime,
                    _ => 0f
                };

                data.Add(value);
            }

            return data;
        }

        private LinearRegressionResult CalculateLinearRegression(List<float> data)
        {
            if (data.Count < 2) return new LinearRegressionResult();

            var n = data.Count;
            var sumX = 0f;
            var sumY = 0f;
            var sumXY = 0f;
            var sumX2 = 0f;
            var sumY2 = 0f;

            for (int i = 0; i < n; i++)
            {
                var x = i;
                var y = data[i];
                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
                sumY2 += y * y;
            }

            var denominator = n * sumX2 - sumX * sumX;
            if (Mathf.Abs(denominator) < 0.001f)
            {
                return new LinearRegressionResult();
            }

            var slope = (n * sumXY - sumX * sumY) / denominator;
            var intercept = (sumY - slope * sumX) / n;

            // 计算相关系数
            var correlation = CalculateCorrelation(data, slope, intercept);

            return new LinearRegressionResult
            {
                Slope = slope,
                Intercept = intercept,
                Correlation = correlation,
                DataPoints = n
            };
        }

        private float CalculateCorrelation(List<float> data, float slope, float intercept)
        {
            var n = data.Count;
            if (n < 2) return 0f;

            var sumY = 0f;
            var sumY2 = 0f;
            var sumYPredicted = 0f;
            var sumYPredicted2 = 0f;
            var sumYYPredicted = 0f;

            for (int i = 0; i < n; i++)
            {
                var x = i;
                var y = data[i];
                var yPredicted = slope * x + intercept;

                sumY += y;
                sumY2 += y * y;
                sumYPredicted += yPredicted;
                sumYPredicted2 += yPredicted * yPredicted;
                sumYYPredicted += y * yPredicted;
            }

            var numerator = n * sumYYPredicted - sumY * sumYPredicted;
            var denominator = Mathf.Sqrt((n * sumY2 - sumY * sumY) * (n * sumYPredicted2 - sumYPredicted * sumYPredicted));

            return denominator > 0.001f ? numerator / denominator : 0f;
        }

        private TrendDirection DetermineTrendDirection(float slope, float correlation)
        {
            var significance = Mathf.Abs(correlation);
            
            if (significance < trendSignificanceThreshold)
                return TrendDirection.Stable;

            if (slope > 0.01f)
                return TrendDirection.Increasing;
            else if (slope < -0.01f)
                return TrendDirection.Decreasing;
            else
                return TrendDirection.Stable;
        }

        private float CalculateTrendConfidence(float correlation, int dataPoints)
        {
            var correlationConfidence = Mathf.Abs(correlation);
            var dataPointConfidence = Mathf.Min((float)dataPoints / 100f); // 100个数据点达到最高置信度
            
            return (correlationConfidence + dataPointConfidence) / 2f;
        }

        private PerformancePrediction GeneratePrediction(List<float> data, float slope, float confidence)
        {
            if (data.Count == 0 || confidence < 0.3f)
            {
                return new PerformancePrediction
                {
                    PredictedValue = data.LastOrDefault(),
                    Confidence = 0f,
                    TimeHorizon = TimeSpan.FromMinutes(predictionHorizonMinutes),
                    PredictionType = PredictionType.Unreliable
                };
            }

            var lastValue = data.Last();
            var predictedChange = slope * predictionHorizonMinutes * 60f / historyUpdateInterval;
            var predictedValue = lastValue + predictedChange;

            var predictionType = DeterminePredictionType(slope, confidence);

            return new PerformancePrediction
            {
                PredictedValue = predictedValue,
                Confidence = confidence,
                TimeHorizon = TimeSpan.FromMinutes(predictionHorizonMinutes),
                PredictionType = predictionType,
                UncertaintyRange = CalculateUncertaintyRange(data, slope, confidence)
            };
        }

        private PredictionType DeterminePredictionType(float slope, float confidence)
        {
            if (confidence < 0.3f)
                return PredictionType.Unreliable;
            else if (confidence < 0.6f)
                return PredictionType.LowConfidence;
            else if (Mathf.Abs(slope) < 0.01f)
                return PredictionType.Stable;
            else
                return PredictionType.HighConfidence;
        }

        private float CalculateUncertaintyRange(List<float> data, float slope, float confidence)
        {
            // 计算标准误差作为不确定性范围
            var mean = data.Average();
            var variance = data.Sum(x => Mathf.Pow(x - mean, 2)) / data.Count;
            var standardError = Mathf.Sqrt(variance / data.Count);

            return standardError * (1f - confidence);
        }

        #endregion

        #region Baseline Management

        private void UpdateBaselines()
        {
            if (_rawHistory.Count < baselineWindowSize) return;

            foreach (MetricType metricType in Enum.GetValues(typeof(MetricType)))
            {
                UpdateBaseline(metricType);
            }
        }

        private void UpdateBaseline(MetricType metricType)
        {
            var data = ExtractMetricData(metricType, baselineWindowSize);
            if (data.Count == 0) return;

            var baseline = _baselines[metricType];
            
            // 更新数据点
            foreach (var value in data)
            {
                baseline.DataPoints.Add(value);
            }

            // 保持窗口大小
            while (baseline.DataPoints.Count > baselineWindowSize)
            {
                baseline.DataPoints.RemoveAt(0);
            }

            // 重新计算统计信息
            baseline.Mean = baseline.DataPoints.Average();
            baseline.MinValue = baseline.DataPoints.Min();
            baseline.MaxValue = baseline.DataPoints.Max();
            
            // 计算标准差
            var variance = baseline.DataPoints.Sum(x => Mathf.Pow(x - baseline.Mean, 2)) / baseline.DataPoints.Count;
            baseline.StandardDeviation = Mathf.Sqrt(variance);

            baseline.LastUpdated = DateTime.UtcNow;

            // 检查基准线偏差
            CheckBaselineDeviation(metricType, baseline);
        }

        private void CheckBaselineDeviation(MetricType metricType, PerformanceBaseline baseline)
        {
            if (_rawHistory.Count == 0) return;

            var currentValue = GetMetricValue(metricType);
            var deviation = Mathf.Abs(currentValue - baseline.Mean);
            var deviationPercent = deviation / baseline.Mean;

            //if (deviationPercent > baselineDeviationThreshold)
            //{
            //    var deviation = new BaselineDeviation
            //    {
            //        MetricType = metricType,
            //        CurrentValue = currentValue,
            //        BaselineMean = baseline.Mean,
            //        DeviationPercent = deviationPercent,
            //        StandardDeviationsFromMean = deviation / baseline.StandardDeviation,
            //        Timestamp = DateTime.UtcNow,
            //        Severity = DetermineDeviationSeverity(deviationPercent)
            //    };

            //    OnBaselineDeviationDetected?.Invoke(deviation);

            //    Debug.LogWarning($"[PerformanceHistoryAnalyzer] Baseline deviation detected for {metricType}: " +
            //                   $"{currentValue:F2} vs baseline {baseline.Mean:F2} ({deviationPercent:P1})");
            //}
        }

        private DeviationSeverity DetermineDeviationSeverity(float deviationPercent)
        {
            if (deviationPercent > 0.5f)
                return DeviationSeverity.Critical;
            else if (deviationPercent > 0.3f)
                return DeviationSeverity.High;
            else if (deviationPercent > 0.2f)
                return DeviationSeverity.Medium;
            else
                return DeviationSeverity.Low;
        }

        #endregion

        #region Anomaly Detection

        private void CheckForAnomalies()
        {
            if (_rawHistory.Count < anomalyMinWindow) return;

            foreach (MetricType metricType in Enum.GetValues(typeof(MetricType)))
            {
                DetectAnomaliesForMetric(metricType);
            }
        }

        private void DetectAnomaliesForMetric(MetricType metricType)
        {
            var data = ExtractMetricData(metricType, anomalyMinWindow);
            if (data.Count < anomalyMinWindow) return;

            var baseline = _baselines[metricType];
            if (baseline.StandardDeviation < 0.001f) return;

            // 检测异常值
            for (int i = 0; i < data.Count; i++)
            {
                var value = data[i];
                var zScore = Mathf.Abs(value - baseline.Mean) / baseline.StandardDeviation;

                if (zScore > anomalyThreshold)
                {
                    var anomaly = new PerformanceAnomaly
                    {
                        Id = Guid.NewGuid().ToString(),
                        MetricType = metricType,
                        AnomalyValue = value,
                        ExpectedValue = baseline.Mean,
                        ZScore = zScore,
                        Severity = DetermineAnomalySeverity(zScore),
                        Timestamp = DateTime.UtcNow.AddSeconds(-(data.Count - i - 1) * historyUpdateInterval),
                        Context = CreateAnomalyContext(metricType, i, data),
                        Resolved = false
                    };

                    // 检查是否已存在相同异常
                    if (!IsDuplicateAnomaly(anomaly))
                    {
                        _detectedAnomalies.Add(anomaly);
                        OnAnomalyDetected?.Invoke(anomaly);

                        Debug.LogWarning($"[PerformanceHistoryAnalyzer] Anomaly detected for {metricType}: " +
                                       $"Value {value:F2} (Z-score: {zScore:F2})");
                    }
                }
            }

            // 清理旧的异常记录
            CleanupOldAnomalies();
        }

        private AnomalySeverity DetermineAnomalySeverity(float zScore)
        {
            if (zScore > 4f)
                return AnomalySeverity.Critical;
            else if (zScore > 3f)
                return AnomalySeverity.High;
            else if (zScore > 2f)
                return AnomalySeverity.Medium;
            else
                return AnomalySeverity.Low;
        }

        private Dictionary<string, object> CreateAnomalyContext(MetricType metricType, int index, List<float> data)
        {
            var context = new Dictionary<string, object>
            {
                ["MetricType"] = metricType.ToString(),
                ["DataIndex"] = index,
                ["WindowSize"] = data.Count,
                ["TimeWindow"] = TimeSpan.FromSeconds(data.Count * historyUpdateInterval)
            };

            // 添加周围数据点
            var surroundingData = new List<float>();
            for (int i = Mathf.Max(0, index - 3); i <= Mathf.Min(data.Count - 1, index + 3); i++)
            {
                surroundingData.Add(data[i]);
            }
            context["SurroundingData"] = surroundingData;

            return context;
        }

        private bool IsDuplicateAnomaly(PerformanceAnomaly anomaly)
        {
            return _detectedAnomalies.Any(a => 
                a.MetricType == anomaly.MetricType && 
                Mathf.Abs(a.AnomalyValue - anomaly.AnomalyValue) < 0.01f &&
                (a.Timestamp - anomaly.Timestamp).TotalSeconds < 60f);
        }

        private void CleanupOldAnomalies()
        {
            var cutoffTime = DateTime.UtcNow.AddDays(-7); // 保留7天的异常记录
            _detectedAnomalies.RemoveAll(a => a.Timestamp < cutoffTime);
        }

        #endregion

        #region Data Aggregation

        private void ProcessDataAggregation(PerformanceSnapshot snapshot)
        {
            // 处理小时聚合
            if (enableHourlySummary)
            {
                ProcessHourlyAggregation(snapshot);
            }

            // 处理日聚合
            if (enableDailySummary)
            {
                ProcessDailyAggregation(snapshot);
            }
        }

        private void ProcessHourlyAggregation(PerformanceSnapshot snapshot)
        {
            var hourKey = new DateTime(snapshot.Timestamp.Year, snapshot.Timestamp.Month, snapshot.Timestamp.Day, snapshot.Timestamp.Hour, 0, 0);

            if (!_hourlySummaries.ContainsKey(hourKey))
            {
                _hourlySummaries[hourKey] = new HourlySummary
                {
                    Hour = hourKey,
                    Timestamp = hourKey
                };
            }

            var summary = _hourlySummaries[hourKey];
            summary.AddSnapshot(snapshot);
        }

        private void ProcessDailyAggregation(PerformanceSnapshot snapshot)
        {
            var dayKey = new DateTime(snapshot.Timestamp.Year, snapshot.Timestamp.Month, snapshot.Timestamp.Day);

            if (!_dailySummaries.ContainsKey(dayKey))
            {
                _dailySummaries[dayKey] = new DailySummary
                {
                    Day = dayKey,
                    Timestamp = dayKey
                };
            }

            //var summary = _dailySummaries[dayKey];
            //summary.AddSnapshot(snapshot);
        }

        #endregion

        #region Utility Methods

        private bool ShouldUpdateHistory()
        {
            return (DateTime.UtcNow - _lastHistoryUpdate).TotalSeconds >= historyUpdateInterval;
        }

        private void CleanupOldData()
        {
            var cutoffTime = DateTime.UtcNow.AddDays(-historyRetentionDays);

            // 清理原始历史数据
            while (_rawHistory.Count > 0 && _rawHistory.Peek().Timestamp < cutoffTime)
            {
                _rawHistory.Dequeue();
            }

            // 清理聚合数据
            while (_aggregatedHistory.Count > 0 && _aggregatedHistory.Peek().Timestamp < cutoffTime)
            {
                _aggregatedHistory.Dequeue();
            }

            // 清理小时汇总
            var oldHourKeys = _hourlySummaries.Keys.Where(k => k < cutoffTime).ToList();
            foreach (var key in oldHourKeys)
            {
                _hourlySummaries.Remove(key);
            }

            // 清理日汇总
            var oldDayKeys = _dailySummaries.Keys.Where(k => k < cutoffTime).ToList();
            foreach (var key in oldDayKeys)
            {
                _dailySummaries.Remove(key);
            }
        }

        private void CleanupHistoryData()
        {
            _rawHistory.Clear();
            _aggregatedHistory.Clear();
            _hourlySummaries.Clear();
            _dailySummaries.Clear();
            _detectedAnomalies.Clear();
        }

        // 性能数据获取方法
        private float GetFPS()
        {
            if (_performanceMonitor != null)
            {
                return _performanceMonitor.GetCurrentMetrics().averageFrameRate;
            }
            return 1f / Time.unscaledDeltaTime;
        }

        private float GetFrameTime()
        {
            return Time.unscaledDeltaTime * 1000f; // 转换为毫秒
        }

        private float GetMemoryUsage()
        {
            if (_memoryManager != null)
            {
                return _memoryManager.CurrentMemoryUsageMB;
            }
            return GC.GetTotalMemory(false) / (1024f * 1024f);
        }

        private int GetActiveCoinCount()
        {
            if (_objectPool != null)
            {
                return _objectPool.ActiveCoinCount;
            }
            return 0;
        }

        private int GetGCCount()
        {
            return GC.CollectionCount(0);
        }

        private float GetAllocatedMemory()
        {
            return 0.2f;//UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(0) / (1024f * 1024f);
        }

        private float GetPoolHitRate()
        {
            if (_objectPool != null)
            {
                var totalRequests = _objectPool.TotalRequests;
                var poolHits = _objectPool.PoolHits;
                return totalRequests > 0 ? (float)poolHits / totalRequests * 100f : 100f;
            }
            return 100f;
        }

        private float GetCPUUsage()
        {
            // 简化的CPU使用率估算
            return 10f;// UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(0) > 0 ? 10f : 5f;
        }

        private float GetGPUUsage()
        {
            // 简化的GPU使用率估算
            return 8;// UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(0) > 0 ? 15f : 8f;
        }

        private float GetRenderTime()
        {
            return Time.deltaTime * 1000f; // 转换为毫秒
        }

        private float GetMetricValue(MetricType metricType)
        {
            return metricType switch
            {
                MetricType.FPS => GetFPS(),
                MetricType.FrameTime => GetFrameTime(),
                MetricType.Memory => GetMemoryUsage(),
                MetricType.ActiveCoins => (float)GetActiveCoinCount(),
                MetricType.GCCount => (float)GetGCCount(),
                MetricType.AllocatedMemory => GetAllocatedMemory(),
                MetricType.PoolHitRate => GetPoolHitRate(),
                MetricType.CPUUsage => GetCPUUsage(),
                MetricType.GPUUsage => GetGPUUsage(),
                MetricType.RenderTime => GetRenderTime(),
                _ => 0f
            };
        }

        #endregion

        #region Public API

        /// <summary>
        /// 获取历史报告
        /// </summary>
        public HistoryAnalysisReport GetHistoryReport()
        {
            return new HistoryAnalysisReport
            {
                GeneratedAt = DateTime.UtcNow,
                TotalSnapshots = _statistics.TotalSnapshots,
                HistoryDuration = _statistics.SystemUptime,
                ActiveAnomalies = _detectedAnomalies.Count,
                TrendAnalyses = new Dictionary<MetricType, TrendAnalysis>(_trendAnalyses),
                Baselines = new Dictionary<MetricType, PerformanceBaseline>(_baselines),
                HourlySummaries = new Dictionary<DateTime, HourlySummary>(_hourlySummaries),
                DailySummaries = new Dictionary<DateTime, DailySummary>(_dailySummaries),
                Statistics = _statistics
            };
        }

        /// <summary>
        /// 获取指定时间范围的历史数据
        /// </summary>
        public List<PerformanceSnapshot> GetHistoryData(TimeSpan timeRange)
        {
            var cutoff = DateTime.UtcNow - timeRange;
            return _rawHistory.Where(s => s.Timestamp >= cutoff).ToList();
        }

        /// <summary>
        /// 获取指定指标的预测
        /// </summary>
        public PerformancePrediction GetPrediction(MetricType metricType)
        {
            if (_trendAnalyses.TryGetValue(metricType, out var analysis))
            {
                return analysis.Prediction;
            }
            return new PerformancePrediction { PredictionType = PredictionType.Unreliable };
        }

        /// <summary>
        /// 设置历史保留天数
        /// </summary>
        public void SetRetentionDays(int days)
        {
            historyRetentionDays = Mathf.Max(1, days);
            Debug.Log($"[PerformanceHistoryAnalyzer] History retention set to {days} days");
        }

        /// <summary>
        /// 清除所有历史数据
        /// </summary>
        public void ClearAllHistory()
        {
            CleanupHistoryData();
            Debug.Log("[PerformanceHistoryAnalyzer] All history data cleared");
        }

        /// <summary>
        /// 启用/禁用历史跟踪
        /// </summary>
        public void SetHistoryTrackingEnabled(bool enabled)
        {
            enableHistoryTracking = enabled;
            Debug.Log($"[PerformanceHistoryAnalyzer] History tracking {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 导出历史数据
        /// </summary>
        public string ExportHistoryData(string format = "csv")
        {
            var data = _rawHistory.ToList();
            
            return format.ToLower() switch
            {
                "csv" => ExportToCSV(data),
                "json" => ExportToJSON(data),
                _ => ExportToCSV(data)
            };
        }

        private string ExportToCSV(List<PerformanceSnapshot> data)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Timestamp,FPS,FrameTime,MemoryMB,ActiveCoins,GCCount,AllocatedMemoryMB,PoolHitRate,CPUUsage,GPUUsage,RenderTime");

            foreach (var snapshot in data)
            {
                csv.AppendLine($"{snapshot.Timestamp:yyyy-MM-dd HH:mm:ss.fff}," +
                            $"{snapshot.FPS:F2}," +
                            $"{snapshot.FrameTime:F2}," +
                            $"{snapshot.MemoryUsageMB:F2}," +
                            $"{snapshot.ActiveCoins}," +
                            $"{snapshot.GCCount}," +
                            $"{snapshot.AllocatedMemoryMB:F2}," +
                            $"{snapshot.PoolHitRate:F2}," +
                            $"{snapshot.CPUUsage:F2}," +
                            $"{snapshot.GPUUsage:F2}," +
                            $"{snapshot.RenderTime:F2}");
            }

            return csv.ToString();
        }

        private string ExportToJSON(List<PerformanceSnapshot> data)
        {
            return JsonUtility.ToJson(new { data = data, exportedAt = DateTime.UtcNow }, true);
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class HistoryPerformanceSnapshot
    {
        public DateTime Timestamp;
        public float FPS;
        public float FrameTime;
        public float MemoryUsageMB;
        public int ActiveCoins;
        public int GCCount;
        public float AllocatedMemoryMB;
        public float PoolHitRate;
        public float CPUUsage;
        public float GPUUsage;
        public float RenderTime;
    }

    [System.Serializable]
    public class TrendAnalysis
    {
        public MetricType MetricType;
        public TrendDirection Trend;
        public float Slope;
        public float Correlation;
        public float Confidence;
        public int DataPoints;
        public TimeSpan TimeWindow;
        public DateTime LastUpdated;
        public PerformancePrediction Prediction;
    }

    [System.Serializable]
    public class PerformanceBaseline
    {
        public MetricType MetricType;
        public DateTime CreatedAt;
        public List<float> DataPoints;
        public float Mean;
        public float StandardDeviation;
        public float MinValue;
        public float MaxValue;
        public DateTime LastUpdated;
    }

    [System.Serializable]
    public class PerformancePrediction
    {
        public float PredictedValue;
        public float Confidence;
        public TimeSpan TimeHorizon;
        public PredictionType PredictionType;
        public float UncertaintyRange;
    }

    [System.Serializable]
    public class PerformanceAnomaly
    {
        public string Id;
        public MetricType MetricType;
        public float AnomalyValue;
        public float ExpectedValue;
        public float ZScore;
        public AnomalySeverity Severity;
        public DateTime Timestamp;
        public Dictionary<string, object> Context;
        public bool Resolved;
    }

    [System.Serializable]
    public class BaselineDeviation
    {
        public MetricType MetricType;
        public float CurrentValue;
        public float BaselineMean;
        public float DeviationPercent;
        public float StandardDeviationsFromMean;
        public DateTime Timestamp;
        public DeviationSeverity Severity;
    }

    [System.Serializable]
    public class HistoryAnalysisStatistics
    {
        public int TotalSnapshots;
        public DateTime SystemStartTime = DateTime.UtcNow;
        public TimeSpan SystemUptime;
        public DateTime LastUpdate;
        public TimeSpan LastTrendAnalysisDuration;
        public int TotalTrendAnalyses;
        public int TotalAnomaliesDetected;
    }

    [System.Serializable]
    public class HistoryAnalysisReport
    {
        public DateTime GeneratedAt;
        public int TotalSnapshots;
        public TimeSpan HistoryDuration;
        public int ActiveAnomalies;
        public Dictionary<MetricType, TrendAnalysis> TrendAnalyses;
        public Dictionary<MetricType, PerformanceBaseline> Baselines;
        public Dictionary<DateTime, HourlySummary> HourlySummaries;
        public Dictionary<DateTime, DailySummary> DailySummaries;
        public HistoryAnalysisStatistics Statistics;
    }

    [System.Serializable]
    public class LinearRegressionResult
    {
        public float Slope;
        public float Intercept;
        public float Correlation;
        public int DataPoints;
    }

    [System.Serializable]
    public class AggregatedDataPoint
    {
        public DateTime Timestamp;
        public float AverageFPS;
        public float AverageMemoryMB;
        public int TotalCoins;
        public int SampleCount;
    }

    [System.Serializable]
    public class HourlySummary
    {
        public DateTime Hour;
        public DateTime Timestamp;
        public List<PerformanceSnapshot> Snapshots = new List<PerformanceSnapshot>();
        public float AverageFPS;
        public float AverageMemoryMB;
        public int TotalCoins;
        public float MinFPS;
        public float MaxFPS;
        public float MinMemoryMB;
        public float MaxMemoryMB;

        public void AddSnapshot(PerformanceSnapshot snapshot)
        {
            Snapshots.Add(snapshot);
            CalculateStatistics();
        }

        private void CalculateStatistics()
        {
            if (Snapshots.Count == 0) return;

            AverageFPS = Snapshots.Average(s => s.FPS);
            AverageMemoryMB = Snapshots.Average(s => s.MemoryUsageMB);
            TotalCoins = Snapshots.Sum(s => s.ActiveCoins);
            MinFPS = Snapshots.Min(s => s.FPS);
            MaxFPS = Snapshots.Max(s => s.FPS);
            MinMemoryMB = Snapshots.Min(s => s.MemoryUsageMB);
            MaxMemoryMB = Snapshots.Max(s => s.MemoryUsageMB);
        }
    }

    [System.Serializable]
    public class DailySummary
    {
        public DateTime Day;
        public DateTime Timestamp;
        public List<HourlySummary> HourlySummaries = new List<HourlySummary>();
        public float AverageFPS;
        public float AverageMemoryMB;
        public int TotalCoins;
        public float MinFPS;
        public float MaxFPS;
        public float MinMemoryMB;
        public float MaxMemoryMB;

        public void AddHourlySummary(HourlySummary hourlySummary)
        {
            HourlySummaries.Add(hourlySummary);
            CalculateStatistics();
        }

        private void CalculateStatistics()
        {
            if (HourlySummaries.Count == 0) return;

            AverageFPS = HourlySummaries.Average(h => h.AverageFPS);
            AverageMemoryMB = HourlySummaries.Average(h => h.AverageMemoryMB);
            TotalCoins = HourlySummaries.Sum(h => h.TotalCoins);
            MinFPS = HourlySummaries.Min(h => h.MinFPS);
            MaxFPS = HourlySummaries.Max(h => h.MaxFPS);
            MinMemoryMB = HourlySummaries.Min(h => h.MinMemoryMB);
            MaxMemoryMB = HourlySummaries.Max(h => h.MaxMemoryMB);
        }
    }

    #endregion

    #region Enums

    public enum MetricType
    {
        FPS,
        FrameTime,
        Memory,
        ActiveCoins,
        GCCount,
        AllocatedMemory,
        PoolHitRate,
        CPUUsage,
        GPUUsage,
        RenderTime
    }

    public enum TrendDirection
    {
        Increasing,
        Decreasing,
        Stable
    }

    public enum PredictionType
    {
        HighConfidence,
        LowConfidence,
        Unreliable,
        Stable
    }

    public enum AnomalySeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum DeviationSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    #endregion
}