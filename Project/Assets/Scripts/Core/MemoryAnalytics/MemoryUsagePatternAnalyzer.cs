using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoinAnimation.Core
{
    /// <summary>
    /// 内存使用模式分析器
    /// Story 2.1 Task 3.1 - 分析内存使用模式并提供优化建议
    /// </summary>
    public class MemoryUsagePatternAnalyzer : MonoBehaviour
    {
        #region Configuration

        [Header("Pattern Analysis Settings")]
        [SerializeField] private bool enablePatternAnalysis = true;
        [SerializeField] private int analysisHistorySize = 300; // 5分钟@1Hz
        [SerializeField] private float analysisInterval = 1.0f; // 1秒分析间隔
        [SerializeField] private int patternDetectionWindow = 60; // 60秒窗口

        [Header("Memory Thresholds")]
        [SerializeField] private float normalMemoryThresholdMB = 50f;
        [SerializeField] private float warningMemoryThresholdMB = 80f;
        [SerializeField] private float criticalMemoryThresholdMB = 120f;
        [SerializeField] private float leakDetectionGrowthRate = 5f; // MB per minute

        [Header("Optimization Settings")]
        [SerializeField] private bool enableOptimizationSuggestions = true;
        [SerializeField] private bool enableAutomaticOptimizations = false;
        [SerializeField] private float optimizationSensitivity = 0.7f;

        #endregion

        #region Private Fields

        // 内存数据收集
        private readonly Queue<MemorySnapshot> _memoryHistory = new Queue<MemorySnapshot>();
        private readonly List<MemoryPattern> _detectedPatterns = new List<MemoryPattern>();
        private readonly Dictionary<MemoryPatternType, int> _patternFrequency = new Dictionary<MemoryPatternType, int>();

        // 分析状态
        private MemoryAnalysisState _currentAnalysisState = MemoryAnalysisState.Normal;
        private MemoryUsageTrend _currentTrend = MemoryUsageTrend.Stable;
        private float _lastAnalysisTime = 0f;
        private bool _analysisInProgress = false;

        // 组件引用
        private MemoryManagementSystem _memoryManager;
        private CoinObjectPool _objectPool;
        private PerformanceMonitor _performanceMonitor;

        // 性能指标
        private MemoryAnalysisMetrics _analysisMetrics = new MemoryAnalysisMetrics();

        #endregion

        #region Properties

        public MemoryAnalysisState CurrentAnalysisState => _currentAnalysisState;
        public MemoryUsageTrend CurrentTrend => _currentTrend;
        public MemoryAnalysisMetrics AnalysisMetrics => _analysisMetrics;
        public bool IsAnalysisInProgress => _analysisInProgress;
        public IReadOnlyList<MemoryPattern> DetectedPatterns => _detectedPatterns.AsReadOnly();

        #endregion

        #region Events

        public event Action<MemoryAnalysisState> OnAnalysisStateChanged;
        public event Action<MemoryUsageTrend> OnUsageTrendChanged;
        public event Action<MemoryPattern> OnPatternDetected;
        public event Action<List<MemoryOptimizationSuggestion>> OnOptimizationSuggestionsGenerated;
        public event Action<MemoryPressureLevel> OnMemoryPressureChanged;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            FindSystemComponents();
            StartCoroutine(AnalysisCoroutine());
        }

        private void Update()
        {
            if (!enablePatternAnalysis) return;

            CollectMemorySnapshot();
        }

        #endregion

        #region Component Discovery

        private void FindSystemComponents()
        {
            _memoryManager = FindObjectOfType<MemoryManagementSystem>();
            _objectPool = FindObjectOfType<CoinObjectPool>();
            _performanceMonitor = FindObjectOfType<PerformanceMonitor>();

            Debug.Log($"[MemoryUsagePatternAnalyzer] Components found: " +
                     $"MemoryManager: {_memoryManager != null}, " +
                     $"ObjectPool: {_objectPool != null}, " +
                     $"PerformanceMonitor: {_performanceMonitor != null}");
        }

        #endregion

        #region Data Collection

        private void CollectMemorySnapshot()
        {
            if (Time.time < _lastAnalysisTime + analysisInterval) return;

            var snapshot = CreateMemorySnapshot();
            _memoryHistory.Enqueue(snapshot);

            // 保持历史记录大小
            while (_memoryHistory.Count > analysisHistorySize)
                _memoryHistory.Dequeue();

            _lastAnalysisTime = Time.time;
        }

        private MemorySnapshot CreateMemorySnapshot()
        {
            var totalMemory = GC.GetTotalMemory(false) / (1024f * 1024f); // MB
            var managedMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory() / (1024f * 1024f); // MB
            var unmanagedMemory = UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemory() / (1024f * 1024f); // MB

            return new MemorySnapshot
            {
                Timestamp = DateTime.UtcNow,
                TotalMemoryMB = totalMemory,
                ManagedMemoryMB = managedMemory,
                UnmanagedMemoryMB = unmanagedMemory,
                ActiveCoinsCount = _objectPool?.ActiveCoinCount ?? 0,
                PooledObjectsCount = _objectPool?.CurrentPoolSize ?? 0,
                FrameRate = _performanceMonitor != null ? 
                    _performanceMonitor.GetCurrentMetrics().averageFrameRate : 60f,
                GCGeneration = GC.MaxGeneration,
                GCCollectionCount = new int[GC.MaxGeneration + 1]
            };
        }

        #endregion

        #region Pattern Analysis

        private IEnumerator AnalysisCoroutine()
        {
            while (enablePatternAnalysis)
            {
                if (!_analysisInProgress && _memoryHistory.Count >= patternDetectionWindow)
                {
                    yield return StartCoroutine(PerformMemoryAnalysis());
                }

                yield return new WaitForSeconds(analysisInterval * 10); // 每10秒进行一次完整分析
            }
        }

        private IEnumerator PerformMemoryAnalysis()
        {
            _analysisInProgress = true;
            var analysisStartTime = Time.realtimeSinceStartup;

            Debug.Log("[MemoryUsagePatternAnalyzer] Starting memory pattern analysis...");

            try
            {
                //// 1. 分析内存使用趋势
                //yield return StartCoroutine(AnalyzeMemoryTrends());

                //// 2. 检测内存模式
                //yield return StartCoroutine(DetectMemoryPatterns());

                //// 3. 生成优化建议
                //if (enableOptimizationSuggestions)
                //{
                //    yield return StartCoroutine(GenerateOptimizationSuggestions());
                //}

                // 4. 更新分析状态
                UpdateAnalysisState();

                var analysisDuration = Time.realtimeSinceStartup - analysisStartTime;
                _analysisMetrics.TotalAnalysisCount++;
                _analysisMetrics.AverageAnalysisTime = 
                    (_analysisMetrics.AverageAnalysisTime * (_analysisMetrics.TotalAnalysisCount - 1) + analysisDuration) 
                    / _analysisMetrics.TotalAnalysisCount;

                Debug.Log($"[MemoryUsagePatternAnalyzer] Analysis completed in {analysisDuration:F3}s");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MemoryUsagePatternAnalyzer] Analysis failed: {e.Message}");
                _analysisMetrics.FailedAnalysisCount++;
            }

            // 1. 分析内存使用趋势
            yield return StartCoroutine(AnalyzeMemoryTrends());

            _analysisInProgress = false;
        }

        private IEnumerator AnalyzeMemoryTrends()
        {
            if (_memoryHistory.Count < patternDetectionWindow) yield break;

            var recentSnapshots = _memoryHistory.TakeLast(patternDetectionWindow).ToList();
            var olderSnapshots = _memoryHistory.Take(patternDetectionWindow).ToList();

            // 计算内存增长率
            var recentAverage = recentSnapshots.Average(s => s.TotalMemoryMB);
            var olderAverage = olderSnapshots.Average(s => s.TotalMemoryMB);
            var growthRate = (recentAverage - olderAverage) / olderAverage;

            // 确定趋势
            var newTrend = DetermineMemoryTrend(growthRate);
            if (newTrend != _currentTrend)
            {
                _currentTrend = newTrend;
                OnUsageTrendChanged?.Invoke(_currentTrend);
                Debug.Log($"[MemoryUsagePatternAnalyzer] Memory trend changed to: {_currentTrend}");
            }

            // 检测内存压力
            var pressureLevel = DetermineMemoryPressureLevel(recentAverage);
            OnMemoryPressureChanged?.Invoke(pressureLevel);

            yield return null;
        }

        private MemoryUsageTrend DetermineMemoryTrend(float growthRate)
        {
            if (growthRate > 0.1f) // 10%以上增长
                return MemoryUsageTrend.Increasing;
            else if (growthRate < -0.05f) // 5%以上下降
                return MemoryUsageTrend.Decreasing;
            else
                return MemoryUsageTrend.Stable;
        }

        private MemoryPressureLevel DetermineMemoryPressureLevel(float memoryMB)
        {
            if (memoryMB >= criticalMemoryThresholdMB)
                return MemoryPressureLevel.Critical;
            //else if (memoryMB >= warningMemoryThresholdMB)
            //    return MemoryPressureLevel.Warning;
            else if (memoryMB >= normalMemoryThresholdMB)
                return MemoryPressureLevel.Moderate;
            else
                return MemoryPressureLevel.Normal;
        }

        #endregion

        #region Pattern Detection

        private IEnumerator DetectMemoryPatterns()
        {
            if (_memoryHistory.Count < patternDetectionWindow) yield break;

            var snapshots = _memoryHistory.ToList();
            _detectedPatterns.Clear();

            // 检测各种内存模式
            yield return StartCoroutine(DetectMemoryLeakPattern(snapshots));
            yield return StartCoroutine(DetectMemorySpikePattern(snapshots));
            yield return StartCoroutine(DetectFragmentationPattern(snapshots));
            yield return StartCoroutine(DetectGarbageCollectionPattern(snapshots));
            yield return StartCoroutine(DetectCyclicPattern(snapshots));
            yield return StartCoroutine(DetectCorrelationPattern(snapshots));

            // 更新模式频率统计
            UpdatePatternFrequency();

            Debug.Log($"[MemoryUsagePatternAnalyzer] Detected {_detectedPatterns.Count} memory patterns");
        }

        private IEnumerator DetectMemoryLeakPattern(List<MemorySnapshot> snapshots)
        {
            // 检测持续内存增长模式
            var timeSpan = (snapshots.Last().Timestamp - snapshots.First().Timestamp).TotalMinutes;
            var memoryGrowth = snapshots.Last().TotalMemoryMB - snapshots.First().TotalMemoryMB;
            var growthRate = memoryGrowth / timeSpan; // MB per minute

            if (growthRate > leakDetectionGrowthRate)
            {
                var pattern = new MemoryPattern
                {
                    Type = MemoryPatternType.MemoryLeak,
                    //Severity = CalculateSeverity(growthRate, leakDetectionGrowthRate * 2),
                    Description = $"Memory leak detected: {growthRate:F2} MB/min growth rate",
                    DetectedTime = DateTime.UtcNow,
                    AffectedComponents = IdentifyLeakSources(snapshots),
                    //Recommendations = GenerateLeakRecommendations(growthRate)
                };

                _detectedPatterns.Add(pattern);
                OnPatternDetected?.Invoke(pattern);
            }

            yield return null;
        }

        private IEnumerator DetectMemorySpikePattern(List<MemorySnapshot> snapshots)
        {
            // 检测内存尖峰模式
            var averageMemory = snapshots.Average(s => s.TotalMemoryMB);
            var threshold = averageMemory * 1.5f; // 150% of average

            var spikes = snapshots.Where(s => s.TotalMemoryMB > threshold).ToList();
            
            if (spikes.Count >= 3) // 至少3个尖峰
            {
                var pattern = new MemoryPattern
                {
                    Type = MemoryPatternType.MemorySpike,
                    Severity = CalculateSeverity(spikes.Count, 10),
                    Description = $"Memory spikes detected: {spikes.Count} spikes above {threshold:F1}MB",
                    DetectedTime = DateTime.UtcNow,
                    AffectedComponents = IdentifySpikeSources(spikes),
                    Recommendations = GenerateSpikeRecommendations(spikes)
                };

                _detectedPatterns.Add(pattern);
                OnPatternDetected?.Invoke(pattern);
            }

            yield return null;
        }

        private IEnumerator DetectFragmentationPattern(List<MemorySnapshot> snapshots)
        {
            // 检测内存碎片化模式
            var fragmentationRatio = CalculateFragmentationRatio(snapshots.Last());
            
            if (fragmentationRatio > 0.3f) // 30%以上碎片化
            {
                var pattern = new MemoryPattern
                {
                    Type = MemoryPatternType.MemoryFragmentation,
                    Severity = CalculateSeverity(fragmentationRatio, 0.5f),
                    Description = $"Memory fragmentation detected: {fragmentationRatio:P1} fragmentation ratio",
                    DetectedTime = DateTime.UtcNow,
                    AffectedComponents = new List<string> { "Managed Heap" },
                    Recommendations = GenerateFragmentationRecommendations(fragmentationRatio)
                };

                _detectedPatterns.Add(pattern);
                OnPatternDetected?.Invoke(pattern);
            }

            yield return null;
        }

        private IEnumerator DetectGarbageCollectionPattern(List<MemorySnapshot> snapshots)
        {
            // 检测GC频繁模式
            var gcFrequency = CalculateGCFrequency(snapshots);
            
            if (gcFrequency > 5) // 每分钟超过5次GC
            {
                var pattern = new MemoryPattern
                {
                    Type = MemoryPatternType.FrequentGC,
                    Severity = CalculateSeverity(gcFrequency, 15),
                    Description = $"Frequent garbage collection: {gcFrequency:F1} GC events per minute",
                    DetectedTime = DateTime.UtcNow,
                    AffectedComponents = IdentifyGCSources(snapshots),
                    Recommendations = GenerateGCRecommendations(gcFrequency)
                };

                _detectedPatterns.Add(pattern);
                OnPatternDetected?.Invoke(pattern);
            }

            yield return null;
        }

        private IEnumerator DetectCyclicPattern(List<MemorySnapshot> snapshots)
        {
            // 检测周期性内存使用模式
            var cyclicPattern = DetectCyclicMemoryUsage(snapshots);
            
            if (cyclicPattern != null)
            {
                var pattern = new MemoryPattern
                {
                    Type = MemoryPatternType.CyclicUsage,
                    Severity = PatternSeverity.Medium,
                    Description = $"Cyclic memory usage pattern detected: {cyclicPattern.PeriodMinutes:F1} minute cycle",
                    DetectedTime = DateTime.UtcNow,
                    AffectedComponents = cyclicPattern.RelatedComponents,
                    Recommendations = GenerateCyclicRecommendations(cyclicPattern)
                };

                _detectedPatterns.Add(pattern);
                OnPatternDetected?.Invoke(pattern);
            }

            yield return null;
        }

        private IEnumerator DetectCorrelationPattern(List<MemorySnapshot> snapshots)
        {
            // 检测内存与其他指标的关联模式
            var correlations = CalculateMemoryCorrelations(snapshots);
            
            foreach (var correlation in correlations)
            {
                if (Math.Abs(correlation.CorrelationValue) > 0.7f) // 强相关性
                {
                    var pattern = new MemoryPattern
                    {
                        Type = MemoryPatternType.Correlation,
                        Severity = PatternSeverity.Low,
                        Description = $"Strong correlation detected: Memory vs {correlation.MetricName} ({correlation.CorrelationValue:F2})",
                        DetectedTime = DateTime.UtcNow,
                        AffectedComponents = new List<string> { correlation.MetricName },
                        Recommendations = GenerateCorrelationRecommendations(correlation)
                    };

                    _detectedPatterns.Add(pattern);
                    OnPatternDetected?.Invoke(pattern);
                }
            }

            yield return null;
        }

        #endregion

        #region Optimization Suggestions

        private IEnumerator GenerateOptimizationSuggestions()
        {
            var suggestions = new List<MemoryOptimizationSuggestion>();

            // 基于检测到的模式生成建议
            foreach (var pattern in _detectedPatterns)
            {
                var patternSuggestions = GenerateSuggestionsForPattern(pattern);
                suggestions.AddRange(patternSuggestions);
            }

            // 基于当前状态生成通用建议
            var generalSuggestions = GenerateGeneralOptimizationSuggestions();
            suggestions.AddRange(generalSuggestions);

            // 去重和排序
            suggestions = suggestions.Distinct().OrderByDescending(s => s.Priority).ToList();

            OnOptimizationSuggestionsGenerated?.Invoke(suggestions);
            Debug.Log($"[MemoryUsagePatternAnalyzer] Generated {suggestions.Count} optimization suggestions");

            yield return null;
        }

        private List<MemoryOptimizationSuggestion> GenerateSuggestionsForPattern(MemoryPattern pattern)
        {
            var suggestions = new List<MemoryOptimizationSuggestion>();

            switch (pattern.Type)
            {
                case MemoryPatternType.MemoryLeak:
                    suggestions.Add(new MemoryOptimizationSuggestion
                    {
                        Type = OptimizationType.FixMemoryLeak,
                        Priority = OptimizationPriority.Critical,
                        Description = "Fix memory leak in identified components",
                        EstimatedMemorySaving = EstimateLeakMemorySaving(pattern),
                        ImplementationEffort = ImplementationEffort.High,
                        AffectedComponents = pattern.AffectedComponents
                    });
                    break;

                case MemoryPatternType.MemorySpike:
                    suggestions.Add(new MemoryOptimizationSuggestion
                    {
                        Type = OptimizationType.ReduceMemorySpikes,
                        Priority = OptimizationPriority.High,
                        Description = "Implement memory pooling to reduce allocation spikes",
                        EstimatedMemorySaving = EstimateSpikeMemorySaving(pattern),
                        ImplementationEffort = ImplementationEffort.Medium,
                        AffectedComponents = pattern.AffectedComponents
                    });
                    break;

                case MemoryPatternType.MemoryFragmentation:
                    suggestions.Add(new MemoryOptimizationSuggestion
                    {
                        Type = OptimizationType.ReduceFragmentation,
                        Priority = OptimizationPriority.Medium,
                        Description = "Implement object pooling and reduce small allocations",
                        EstimatedMemorySaving = EstimateFragmentationMemorySaving(pattern),
                        ImplementationEffort = ImplementationEffort.Medium,
                        AffectedComponents = pattern.AffectedComponents
                    });
                    break;

                case MemoryPatternType.FrequentGC:
                    suggestions.Add(new MemoryOptimizationSuggestion
                    {
                        Type = OptimizationType.ReduceGCPressure,
                        Priority = OptimizationPriority.High,
                        Description = "Reduce allocation frequency to minimize GC pressure",
                        EstimatedMemorySaving = EstimateGCMemorySaving(pattern),
                        ImplementationEffort = ImplementationEffort.Medium,
                        AffectedComponents = pattern.AffectedComponents
                    });
                    break;
            }

            return suggestions;
        }

        private List<MemoryOptimizationSuggestion> GenerateGeneralOptimizationSuggestions()
        {
            var suggestions = new List<MemoryOptimizationSuggestion>();

            var currentMemory = _memoryHistory.Count > 0 ? _memoryHistory.Last().TotalMemoryMB : 0f;

            if (currentMemory > normalMemoryThresholdMB)
            {
                suggestions.Add(new MemoryOptimizationSuggestion
                {
                    Type = OptimizationType.OptimizeTextures,
                    Priority = OptimizationPriority.Medium,
                    Description = "Compress textures and use appropriate mipmaps",
                    EstimatedMemorySaving = currentMemory * 0.15f, // 15% 估算节省
                    ImplementationEffort = ImplementationEffort.Low
                });
            }

            if (_objectPool != null && _objectPool.ActiveCoinCount < _objectPool.CurrentPoolSize * 0.5f)
            {
                suggestions.Add(new MemoryOptimizationSuggestion
                {
                    Type = OptimizationType.OptimizePoolSize,
                    Priority = OptimizationPriority.Low,
                    Description = "Reduce object pool size to match actual usage",
                    EstimatedMemorySaving = (_objectPool.CurrentPoolSize - _objectPool.ActiveCoinCount) * 0.1f,
                    ImplementationEffort = ImplementationEffort.Low
                });
            }

            return suggestions;
        }

        #endregion

        #region Utility Methods

        private void UpdateAnalysisState()
        {
            var newState = DetermineAnalysisState();
            
            if (newState != _currentAnalysisState)
            {
                _currentAnalysisState = newState;
                OnAnalysisStateChanged?.Invoke(_currentAnalysisState);
                Debug.Log($"[MemoryUsagePatternAnalyzer] Analysis state changed to: {_currentAnalysisState}");
            }
        }

        private MemoryAnalysisState DetermineAnalysisState()
        {
            var hasCriticalPatterns = _detectedPatterns.Any(p => p.Severity == PatternSeverity.Critical);
            var hasHighPatterns = _detectedPatterns.Any(p => p.Severity == PatternSeverity.High);
            var currentMemory = _memoryHistory.Count > 0 ? _memoryHistory.Last().TotalMemoryMB : 0f;

            if (hasCriticalPatterns || currentMemory >= criticalMemoryThresholdMB)
                return MemoryAnalysisState.Critical;
            else if (hasHighPatterns || currentMemory >= warningMemoryThresholdMB)
                return MemoryAnalysisState.Warning;
            else if (_detectedPatterns.Count > 0)
                return MemoryAnalysisState.Monitoring;
            else
                return MemoryAnalysisState.Normal;
        }

        private void UpdatePatternFrequency()
        {
            _patternFrequency.Clear();
            
            foreach (var pattern in _detectedPatterns)
            {
                if (_patternFrequency.ContainsKey(pattern.Type))
                    _patternFrequency[pattern.Type]++;
                else
                    _patternFrequency[pattern.Type] = 1;
            }
        }

        // 辅助方法实现
        private PatternSeverity CalculateSeverity(float value, float threshold)
        {
            if (value >= threshold * 2) return PatternSeverity.Critical;
            if (value >= threshold) return PatternSeverity.High;
            if (value >= threshold * 0.5) return PatternSeverity.Medium;
            return PatternSeverity.Low;
        }

        private float CalculateFragmentationRatio(MemorySnapshot snapshot)
        {
            // 简化的碎片化计算
            return snapshot.UnmanagedMemoryMB / (snapshot.TotalMemoryMB + 0.001f);
        }

        private float CalculateGCFrequency(List<MemorySnapshot> snapshots)
        {
            // 简化的GC频率计算
            var timeSpan = (snapshots.Last().Timestamp - snapshots.First().Timestamp).TotalMinutes;
            return 0f;//timeSpan > 0 ? snapshots.Count / timeSpan : 0f;
        }

        // 其他辅助方法的占位符实现...
        private List<string> IdentifyLeakSources(List<MemorySnapshot> snapshots) => new List<string> { "Unknown" };
        private List<string> GenerateLeakRecommendations(float growthRate) => new List<string> { "Investigate allocation patterns" };
        private List<string> IdentifySpikeSources(List<MemorySnapshot> spikes) => new List<string> { "High allocation events" };
        private List<string> GenerateSpikeRecommendations(List<MemorySnapshot> spikes) => new List<string> { "Implement pooling" };
        private List<string> GenerateFragmentationRecommendations(float ratio) => new List<string> { "Reduce small allocations" };
        private List<string> IdentifyGCSources(List<MemorySnapshot> snapshots) => new List<string> { "Frequent allocations" };
        private List<string> GenerateGCRecommendations(float frequency) => new List<string> { "Reduce allocation rate" };
        private CyclicMemoryPattern DetectCyclicMemoryUsage(List<MemorySnapshot> snapshots) => null;
        private List<MemoryCorrelation> CalculateMemoryCorrelations(List<MemorySnapshot> snapshots) => new List<MemoryCorrelation>();
        private List<string> GenerateCyclicRecommendations(CyclicMemoryPattern pattern) => new List<string> { "Optimize cyclic allocations" };
        private List<string> GenerateCorrelationRecommendations(MemoryCorrelation correlation) => new List<string> { "Investigate correlation" };
        private float EstimateLeakMemorySaving(MemoryPattern pattern) => 20f;
        private float EstimateSpikeMemorySaving(MemoryPattern pattern) => 15f;
        private float EstimateFragmentationMemorySaving(MemoryPattern pattern) => 10f;
        private float EstimateGCMemorySaving(MemoryPattern pattern) => 12f;

        #endregion

        #region Public API

        /// <summary>
        /// 获取内存分析报告
        /// </summary>
        public MemoryAnalysisReport GetAnalysisReport()
        {
            return new MemoryAnalysisReport
            {
                GeneratedAt = DateTime.UtcNow,
                CurrentState = _currentAnalysisState,
                CurrentTrend = _currentTrend,
                DetectedPatterns = new List<MemoryPattern>(_detectedPatterns),
                PatternFrequency = new Dictionary<MemoryPatternType, int>(_patternFrequency),
                CurrentMemoryMB = _memoryHistory.Count > 0 ? _memoryHistory.Last().TotalMemoryMB : 0f,
                AverageMemoryMB = _memoryHistory.Count > 0 ? _memoryHistory.Average(s => s.TotalMemoryMB) : 0f,
                PeakMemoryMB = _memoryHistory.Count > 0 ? _memoryHistory.Max(s => s.TotalMemoryMB) : 0f,
                AnalysisMetrics = _analysisMetrics
            };
        }

        /// <summary>
        /// 手动触发内存分析
        /// </summary>
        public void TriggerAnalysis()
        {
            if (!_analysisInProgress)
            {
                StartCoroutine(PerformMemoryAnalysis());
            }
        }

        /// <summary>
        /// 清除历史数据
        /// </summary>
        public void ClearHistory()
        {
            _memoryHistory.Clear();
            _detectedPatterns.Clear();
            _patternFrequency.Clear();
            Debug.Log("[MemoryUsagePatternAnalyzer] History cleared");
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class MemorySnapshot
    {
        public DateTime Timestamp;
        public float TotalMemoryMB;
        public float ManagedMemoryMB;
        public float UnmanagedMemoryMB;
        public int ActiveCoinsCount;
        public int PooledObjectsCount;
        public float FrameRate;
        public int GCGeneration;
        public int[] GCCollectionCount;
    }

    [System.Serializable]
    public class MemoryPattern
    {
        public MemoryPatternType Type;
        public PatternSeverity Severity;
        public string Description;
        public DateTime DetectedTime;
        public List<string> AffectedComponents;
        public List<string> Recommendations;
        public Dictionary<string, object> PatternData;
    }

    [System.Serializable]
    public class MemoryAnalysisMetrics
    {
        public int TotalAnalysisCount;
        public int FailedAnalysisCount;
        public float AverageAnalysisTime;
        public DateTime LastAnalysisTime;
        public TimeSpan TotalAnalysisTime;
    }

    [System.Serializable]
    public class MemoryOptimizationSuggestion
    {
        public OptimizationType Type;
        public OptimizationPriority Priority;
        public string Description;
        public float EstimatedMemorySaving;
        public ImplementationEffort ImplementationEffort;
        public List<string> AffectedComponents;
        public List<string> ImplementationSteps;
    }

    [System.Serializable]
    public class MemoryAnalysisReport
    {
        public DateTime GeneratedAt;
        public MemoryAnalysisState CurrentState;
        public MemoryUsageTrend CurrentTrend;
        public List<MemoryPattern> DetectedPatterns;
        public Dictionary<MemoryPatternType, int> PatternFrequency;
        public float CurrentMemoryMB;
        public float AverageMemoryMB;
        public float PeakMemoryMB;
        public MemoryAnalysisMetrics AnalysisMetrics;
    }

    [System.Serializable]
    public class CyclicMemoryPattern
    {
        public float PeriodMinutes;
        public float AmplitudeMB;
        public List<string> RelatedComponents;
    }

    [System.Serializable]
    public class MemoryCorrelation
    {
        public string MetricName;
        public float CorrelationValue;
        public CorrelationType Type;
    }

    #endregion

    #region Enums

    public enum MemoryAnalysisState
    {
        Normal,
        Monitoring,
        Warning,
        Critical
    }

    public enum MemoryUsageTrend
    {
        Decreasing,
        Stable,
        Increasing
    }

    public enum MemoryPatternType
    {
        MemoryLeak,
        MemorySpike,
        MemoryFragmentation,
        FrequentGC,
        CyclicUsage,
        Correlation
    }

    public enum PatternSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    //public enum MemoryPressureLevel
    //{
    //    Normal,
    //    Moderate,
    //    Warning,
    //    Critical
    //}

    public enum OptimizationType
    {
        FixMemoryLeak,
        ReduceMemorySpikes,
        ReduceFragmentation,
        ReduceGCPressure,
        OptimizeTextures,
        OptimizePoolSize,
        CompressAudio,
        OptimizeShaders
    }

    public enum OptimizationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum ImplementationEffort
    {
        Low,
        Medium,
        High
    }

    public enum CorrelationType
    {
        Positive,
        Negative,
        None
    }

    #endregion
}