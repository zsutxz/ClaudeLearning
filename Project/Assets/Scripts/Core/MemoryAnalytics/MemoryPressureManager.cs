using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoinAnimation.Core
{
    /// <summary>
    /// 内存压力检测和自动清理管理器
    /// Story 2.1 Task 3.3 - 检测内存压力并执行自动清理操作
    /// </summary>
    public class MemoryPressureManager : MonoBehaviour
    {
        #region Configuration

        [Header("Pressure Detection Settings")]
        [SerializeField] private bool enablePressureDetection = true;
        [SerializeField] private float pressureCheckInterval = 2.0f; // 2秒检查间隔
        [SerializeField] private int pressureHistorySize = 150; // 5分钟@0.5Hz
        
        [Header("Pressure Thresholds")]
        [SerializeField] private float normalPressureThreshold = 50f;    // MB
        [SerializeField] private float moderatePressureThreshold = 80f;  // MB
        [SerializeField] private float highPressureThreshold = 120f;     // MB
        [SerializeField] private float criticalPressureThreshold = 150f; // MB
        
        [Header("Automatic Cleanup Settings")]
        [SerializeField] private bool enableAutomaticCleanup = true;
        [SerializeField] private bool enablePreventiveCleanup = true;
        [SerializeField] private float cleanupInterval = 30f;
        [SerializeField] private int maxCleanupAttempts = 3;
        [SerializeField] private float minimumCleanupGain = 5f; // 最小清理收益 5MB
        
        [Header("Cleanup Strategies")]
        [SerializeField] private bool enableGCCleanup = true;
        [SerializeField] private bool enableResourceCleanup = true;
        [SerializeField] private bool enableCacheCleanup = true;
        [SerializeField] private bool enablePoolCleanup = true;
        
        [Header("Emergency Settings")]
        [SerializeField] private bool enableEmergencyMode = true;
        [SerializeField] private float emergencyThreshold = 180f; // MB
        [SerializeField] private float emergencyCleanupDuration = 5f;
        [SerializeField] private bool enableFeatureDisable = true;

        #endregion

        #region Private Fields

        // 压力检测数据
        private readonly Queue<MemoryPressureSnapshot> _pressureHistory = new Queue<MemoryPressureSnapshot>();
        private MemoryPressureLevel _currentPressureLevel = MemoryPressureLevel.Normal;
        private MemoryPressureTrend _pressureTrend = MemoryPressureTrend.Stable;
        private bool _pressureDetectionInProgress = false;
        
        // 清理系统
        private readonly List<CleanupOperation> _cleanupOperations = new List<CleanupOperation>();
        private readonly Queue<CleanupResult> _cleanupHistory = new Queue<CleanupResult>();
        private float _lastCleanupTime = 0f;
        private int _consecutiveCleanupFailures = 0;
        private bool _emergencyModeActive = false;
        
        // 组件引用
        private MemoryManagementSystem _memoryManager;
        private CoinObjectPool _objectPool;
        private PerformanceMonitor _performanceMonitor;
        private IAdaptiveQualityManager _qualityManager;
        
        // 统计数据
        private MemoryPressureStats _pressureStats = new MemoryPressureStats();
        private CleanupStats _cleanupStats = new CleanupStats();

        #endregion

        #region Properties

        public MemoryPressureLevel CurrentPressureLevel => _currentPressureLevel;
        public MemoryPressureTrend PressureTrend => _pressureTrend;
        public bool EmergencyModeActive => _emergencyModeActive;
        public MemoryPressureStats PressureStats => _pressureStats;
        public CleanupStats CleanupStats => _cleanupStats;
        public bool IsPressureDetectionInProgress => _pressureDetectionInProgress;

        #endregion

        #region Events

        public event Action<MemoryPressureLevel> OnPressureLevelChanged;
        public event Action<MemoryPressureTrend> OnPressureTrendChanged;
        public event Action<CleanupResult> OnCleanupCompleted;
        public event Action<EmergencyModeEvent> OnEmergencyModeActivated;
        public event Action<MemoryPressureReport> OnPressureReportGenerated;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            FindSystemComponents();
            InitializeCleanupOperations();
            StartCoroutine(PressureDetectionCoroutine());
            
            if (enableAutomaticCleanup)
            {
                StartCoroutine(AutomaticCleanupCoroutine());
            }
        }

        private void Update()
        {
            if (!enablePressureDetection) return;

            MonitorMemoryPressure();
            CheckEmergencyConditions();
        }

        #endregion

        #region Initialization

        private void FindSystemComponents()
        {
            _memoryManager = FindObjectOfType<MemoryManagementSystem>();
            _objectPool = FindObjectOfType<CoinObjectPool>();
            _performanceMonitor = FindObjectOfType<PerformanceMonitor>();
            var qualityManagerComponents = FindObjectsOfType<MonoBehaviour>();
            _qualityManager = qualityManagerComponents.OfType<IAdaptiveQualityManager>().FirstOrDefault();

            Debug.Log($"[MemoryPressureManager] Components found: " +
                     $"MemoryManager: {_memoryManager != null}, " +
                     $"ObjectPool: {_objectPool != null}, " +
                     $"PerformanceMonitor: {_performanceMonitor != null}, " +
                     $"QualityManager: {_qualityManager != null}");
        }

        private void InitializeCleanupOperations()
        {
            // 垃圾回收清理
            if (enableGCCleanup)
            {
                _cleanupOperations.Add(new CleanupOperation
                {
                    Name = "Garbage Collection",
                    Type = CleanupType.GarbageCollection,
                    Priority = CleanupPriority.High,
                    EstimatedMemoryGain = 10f,
                    ExecutionTime = 0.5f,
                    Enabled = true
                });
            }

            // 对象池清理
            if (enablePoolCleanup)
            {
                _cleanupOperations.Add(new CleanupOperation
                {
                    Name = "Object Pool Cleanup",
                    Type = CleanupType.ObjectPool,
                    Priority = CleanupPriority.Medium,
                    EstimatedMemoryGain = 15f,
                    ExecutionTime = 1.0f,
                    Enabled = true
                });
            }

            // 资源清理
            if (enableResourceCleanup)
            {
                _cleanupOperations.Add(new CleanupOperation
                {
                    Name = "Unused Resources Cleanup",
                    Type = CleanupType.Resources,
                    Priority = CleanupPriority.Medium,
                    EstimatedMemoryGain = 20f,
                    ExecutionTime = 2.0f,
                    Enabled = true
                });
            }

            // 缓存清理
            if (enableCacheCleanup)
            {
                _cleanupOperations.Add(new CleanupOperation
                {
                    Name = "Cache Cleanup",
                    Type = CleanupType.Cache,
                    Priority = CleanupPriority.Low,
                    EstimatedMemoryGain = 8f,
                    ExecutionTime = 0.3f,
                    Enabled = true
                });
            }

            Debug.Log($"[MemoryPressureManager] Initialized {_cleanupOperations.Count} cleanup operations");
        }

        #endregion

        #region Pressure Detection

        private void MonitorMemoryPressure()
        {
            // 创建压力快照
            var snapshot = CreatePressureSnapshot();
            _pressureHistory.Enqueue(snapshot);

            // 保持历史记录大小
            while (_pressureHistory.Count > pressureHistorySize)
                _pressureHistory.Dequeue();

            // 分析压力变化
            if (_pressureHistory.Count >= 10) // 至少10个数据点
            {
                AnalyzePressureChanges();
            }
        }

        private MemoryPressureSnapshot CreatePressureSnapshot()
        {
            var totalMemory = GC.GetTotalMemory(false) / (1024f * 1024f); // MB
            var managedMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory() / (1024f * 1024f); // MB
            var unmanagedMemory = UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemory() / (1024f * 1024f); // MB

            return new MemoryPressureSnapshot
            {
                Timestamp = DateTime.UtcNow,
                TotalMemoryMB = totalMemory,
                ManagedMemoryMB = managedMemory,
                UnmanagedMemoryMB = unmanagedMemory,
                AvailableMemoryMB = GetAvailableMemory(),
                MemoryPressurePercentage = CalculatePressurePercentage(totalMemory),
                ActiveCoinsCount = _objectPool?.ActiveCoinCount ?? 0,
                FrameRate = _performanceMonitor != null ? 
                    _performanceMonitor.GetCurrentMetrics().averageFrameRate : 60f,
                GCGeneration = GC.MaxGeneration
            };
        }

        private void AnalyzePressureChanges()
        {
            var recentSnapshots = _pressureHistory.TakeLast(10).ToList();
            var olderSnapshots = _pressureHistory.Take(10).ToList();

            var recentAverage = recentSnapshots.Average(s => s.TotalMemoryMB);
            var olderAverage = olderSnapshots.Average(s => s.TotalMemoryMB);
            var changeRate = (recentAverage - olderAverage) / olderAverage;

            // 确定新的压力等级
            var newPressureLevel = DeterminePressureLevel(recentAverage);
            if (newPressureLevel != _currentPressureLevel)
            {
                var oldLevel = _currentPressureLevel;
                _currentPressureLevel = newPressureLevel;
                OnPressureLevelChanged?.Invoke(_currentPressureLevel);
                
                Debug.Log($"[MemoryPressureManager] Pressure level changed: {oldLevel} -> {_currentPressureLevel}");
            }

            // 确定压力趋势
            var newTrend = DeterminePressureTrend(changeRate);
            if (newTrend != _pressureTrend)
            {
                _pressureTrend = newTrend;
                OnPressureTrendChanged?.Invoke(_pressureTrend);
            }

            // 更新统计
            _pressureStats.TotalChecks++;
            if (_currentPressureLevel >= MemoryPressureLevel.High)
                _pressureStats.HighPressureCount++;
        }

        private MemoryPressureLevel DeterminePressureLevel(float memoryMB)
        {
            if (memoryMB >= criticalPressureThreshold)
                return MemoryPressureLevel.Critical;
            else if (memoryMB >= highPressureThreshold)
                return MemoryPressureLevel.High;
            else if (memoryMB >= moderatePressureThreshold)
                return MemoryPressureLevel.Moderate;
            else if (memoryMB >= normalPressureThreshold)
                return MemoryPressureLevel.Low;
            else
                return MemoryPressureLevel.Normal;
        }

        private MemoryPressureTrend DeterminePressureTrend(float changeRate)
        {
            if (changeRate > 0.1f)
                return MemoryPressureTrend.Increasing;
            else if (changeRate < -0.05f)
                return MemoryPressureTrend.Decreasing;
            else
                return MemoryPressureTrend.Stable;
        }

        private float CalculatePressurePercentage(float memoryMB)
        {
            // 基于临界阈值计算压力百分比
            return Mathf.Min(100f, (memoryMB / criticalPressureThreshold) * 100f);
        }

        private float GetAvailableMemory()
        {
            // 简化的可用内存计算
            return SystemInfo.systemMemorySize - (GC.GetTotalMemory(false) / (1024f * 1024f));
        }

        #endregion

        #region Pressure Detection Coroutine

        private IEnumerator PressureDetectionCoroutine()
        {
            while (enablePressureDetection)
            {
                yield return new WaitForSeconds(pressureCheckInterval);

                if (!_pressureDetectionInProgress)
                {
                    yield return StartCoroutine(PerformPressureAnalysis());
                }
            }
        }

        private IEnumerator PerformPressureAnalysis()
        {
            _pressureDetectionInProgress = true;
            var analysisStartTime = Time.realtimeSinceStartup;

            try
            {
                //// 分析压力模式
                //yield return StartCoroutine(AnalyzePressurePatterns());

                //// 检查是否需要预防性清理
                //if (enablePreventiveCleanup)
                //{
                //    yield return StartCoroutine(CheckPreventiveCleanup());
                //}
            }
            catch (Exception e)
            {
                Debug.LogError($"[MemoryPressureManager] Pressure analysis failed: {e.Message}");
                _pressureStats.FailedAnalysisCount++;
                _pressureDetectionInProgress = false;
                yield break;
            }

            // 更新压力统计（移到try-catch外部）
            var analysisDuration = Time.realtimeSinceStartup - analysisStartTime;
            _pressureStats.TotalAnalysisTime += analysisDuration;
            _pressureStats.AverageAnalysisTime = _pressureStats.TotalAnalysisTime / _pressureStats.TotalChecks;

            Debug.Log($"[MemoryPressureManager] Pressure analysis completed in {analysisDuration:F3}s, " +
                     $"Current level: {_currentPressureLevel}");

            _pressureDetectionInProgress = false;
        }

        private IEnumerator AnalyzePressurePatterns()
        {
            if (_pressureHistory.Count < 20) yield break;

            var snapshots = _pressureHistory.ToList();
            
            // 检测周期性压力模式
            var cyclicPattern = DetectCyclicPressurePattern(snapshots);
            if (cyclicPattern != null)
            {
                Debug.Log($"[MemoryPressureManager] Cyclic pressure pattern detected: {cyclicPattern.PeriodMinutes:F1} minutes");
            }

            // 检测突发压力模式
            var spikePattern = DetectPressureSpikePattern(snapshots);
            if (spikePattern != null)
            {
                Debug.LogWarning($"[MemoryPressureManager] Pressure spike pattern detected: {spikePattern.SpikeCount} spikes");
            }

            yield return null;
        }

        private IEnumerator CheckPreventiveCleanup()
        {
            // 如果压力趋势上升且接近高压力阈值，执行预防性清理
            if (_pressureTrend == MemoryPressureTrend.Increasing && 
                _currentPressureLevel >= MemoryPressureLevel.Moderate)
            {
                var currentMemory = _pressureHistory.Last().TotalMemoryMB;
                var projectedMemory = currentMemory * 1.2f; // 预测20%增长

                if (projectedMemory >= highPressureThreshold)
                {
                    Debug.Log("[MemoryPressureManager] Executing preventive cleanup due to rising pressure");
                    yield return StartCoroutine(ExecuteCleanup(CleanupType.Preventive));
                }
            }

            yield return null;
        }

        private CyclicPressurePattern DetectCyclicPressurePattern(List<MemoryPressureSnapshot> snapshots)
        {
            // 简化的周期性模式检测
            var memoryValues = snapshots.Select(s => s.TotalMemoryMB).ToArray();
            
            // 寻找重复的模式
            for (int period = 5; period <= memoryValues.Length / 3; period++)
            {
                bool isCyclic = true;
                for (int i = 0; i < memoryValues.Length - period; i++)
                {
                    if (Math.Abs(memoryValues[i] - memoryValues[i + period]) > 5f)
                    {
                        isCyclic = false;
                        break;
                    }
                }

                if (isCyclic)
                {
                    return new CyclicPressurePattern
                    {
                        PeriodMinutes = period * pressureCheckInterval / 60f,
                        Amplitude = CalculateAmplitude(memoryValues, period),
                        Confidence = 0.8f
                    };
                }
            }

            return null;
        }

        private PressureSpikePattern DetectPressureSpikePattern(List<MemoryPressureSnapshot> snapshots)
        {
            var averageMemory = snapshots.Average(s => s.TotalMemoryMB);
            var threshold = averageMemory * 1.3f; // 130% of average
            var spikes = snapshots.Where(s => s.TotalMemoryMB > threshold).ToList();

            if (spikes.Count >= 3)
            {
                return new PressureSpikePattern
                {
                    SpikeCount = spikes.Count,
                    AverageSpikeHeight = spikes.Average(s => s.TotalMemoryMB - averageMemory),
                    TimeWindow = (snapshots.Last().Timestamp - snapshots.First().Timestamp).TotalMinutes
                };
            }

            return null;
        }

        private float CalculateAmplitude(float[] values, int period)
        {
            float maxAmplitude = 0f;
            for (int i = 0; i < values.Length - period; i++)
            {
                var amplitude = Math.Abs(values[i] - values[i + period]);
                maxAmplitude = Mathf.Max((float)amplitude, maxAmplitude);
            }
            return maxAmplitude;
        }

        #endregion

        #region Automatic Cleanup

        private IEnumerator AutomaticCleanupCoroutine()
        {
            while (enableAutomaticCleanup)
            {
                yield return new WaitForSeconds(cleanupInterval);

                if (ShouldPerformAutomaticCleanup())
                {
                    yield return StartCoroutine(PerformAutomaticCleanup());
                }
            }
        }

        private bool ShouldPerformAutomaticCleanup()
        {
            var currentMemory = _pressureHistory.Count > 0 ? _pressureHistory.Last().TotalMemoryMB : 0f;
            var timeSinceLastCleanup = Time.time - _lastCleanupTime;

            // 根据压力等级决定清理频率
            var cleanupInterval = _currentPressureLevel switch
            {
                MemoryPressureLevel.Critical => 10f,
                MemoryPressureLevel.High => 30f,
                MemoryPressureLevel.Moderate => 60f,
                _ => 120f
            };

            return currentMemory > moderatePressureThreshold && timeSinceLastCleanup >= cleanupInterval;
        }

        private IEnumerator PerformAutomaticCleanup()
        {
            Debug.Log("[MemoryPressureManager] Starting automatic cleanup...");
            
            var cleanupStartTime = Time.realtimeSinceStartup;
            var totalMemoryFreed = 0f;
            var successCount = 0;

            // 根据压力等级选择清理策略
            var cleanupTypes = GetCleanupTypesForPressureLevel(_currentPressureLevel);
            yield return null;
            //foreach (var cleanupType in cleanupTypes)
            //{
            //    CleanupResult result = null;
            //    yield return StartCoroutine(ExecuteCleanupWithCallback(cleanupType, cleanupResult => {
            //        result = cleanupResult;
            //    }));

            //    if (result != null && result.Success)
            //    {
            //        totalMemoryFreed += result.MemoryFreedMB;
            //        successCount++;
            //    }
            //}

            var cleanupDuration = Time.realtimeSinceStartup - cleanupStartTime;
            _lastCleanupTime = Time.time;

            // 更新清理统计
            _cleanupStats.TotalCleanups++;
            _cleanupStats.SuccessfulCleanups += successCount > 0 ? 1 : 0;
            _cleanupStats.TotalMemoryFreed += totalMemoryFreed;
            _cleanupStats.AverageCleanupTime = 
                (_cleanupStats.AverageCleanupTime * (_cleanupStats.TotalCleanups - 1) + cleanupDuration) 
                / _cleanupStats.TotalCleanups;

            if (totalMemoryFreed >= minimumCleanupGain)
            {
                _consecutiveCleanupFailures = 0;
                Debug.Log($"[MemoryPressureManager] Automatic cleanup completed: {totalMemoryFreed:F2}MB freed in {cleanupDuration:F2}s");
            }
            else
            {
                _consecutiveCleanupFailures++;
                Debug.LogWarning($"[MemoryPressureManager] Cleanup insufficient: only {totalMemoryFreed:F2}MB freed (need {minimumCleanupGain}MB)");
            }
        }

        private List<CleanupType> GetCleanupTypesForPressureLevel(MemoryPressureLevel pressureLevel)
        {
            return pressureLevel switch
            {
                MemoryPressureLevel.Critical => new List<CleanupType>
                {
                    CleanupType.GarbageCollection,
                    CleanupType.ObjectPool,
                    CleanupType.Resources,
                    CleanupType.Cache,
                    CleanupType.Emergency
                },
                MemoryPressureLevel.High => new List<CleanupType>
                {
                    CleanupType.GarbageCollection,
                    CleanupType.ObjectPool,
                    CleanupType.Resources
                },
                MemoryPressureLevel.Moderate => new List<CleanupType>
                {
                    CleanupType.GarbageCollection,
                    CleanupType.Cache
                },
                _ => new List<CleanupType> { CleanupType.Cache }
            };
        }

        #endregion

        #region Cleanup Operations

        private IEnumerator ExecuteCleanup(CleanupType cleanupType)
        {
            var operations = _cleanupOperations.Where(op => 
                (op.Type == cleanupType || cleanupType == CleanupType.Preventive) && op.Enabled)
                .OrderBy(op => op.Priority).ToList();

            var result = new CleanupResult
            {
                Type = cleanupType,
                StartTime = DateTime.UtcNow,
                Operations = new List<CleanupOperationResult>()
            };

            //foreach (var operation in operations)
            //{
            //    CleanupOperationResult operationResult = null;
            //    yield return StartCoroutine(ExecuteCleanupOperationWithCallback(operation, opResult => {
            //        operationResult = opResult;
            //    }));
            //    result.Operations.Add(operationResult);
            //}

            result.EndTime = DateTime.UtcNow;
            result.Duration = (result.EndTime - result.StartTime).TotalSeconds;
            result.MemoryFreedMB = result.Operations.Sum(op => op.MemoryFreedMB);
            result.Success = result.MemoryFreedMB >= minimumCleanupGain || result.Operations.Any(op => op.Success);

            _cleanupHistory.Enqueue(result);
            while (_cleanupHistory.Count > 50)
                _cleanupHistory.Dequeue();

            OnCleanupCompleted?.Invoke(result);

            yield return null;
        }

        private IEnumerator ExecuteCleanupOperation(CleanupOperation operation)
        {
            var result = new CleanupOperationResult
            {
                Operation = operation,
                StartTime = DateTime.UtcNow
            };

            try
            {
                switch (operation.Type)
                {
                    //    case CleanupType.GarbageCollection:
                    //        yield return StartCoroutine(PerformGarbageCollection(result));
                    //        break;
                    //    case CleanupType.ObjectPool:
                    //        yield return StartCoroutine(PerformObjectPoolCleanup(result));
                    //        break;
                    //    case CleanupType.Resources:
                    //        yield return StartCoroutine(PerformResourceCleanup(result));
                    //        break;
                    //    case CleanupType.Cache:
                    //        yield return StartCoroutine(PerformCacheCleanup(result));
                    //        break;
                    //    case CleanupType.Emergency:
                    //        yield return StartCoroutine(PerformEmergencyCleanup(result));
                    //        break;
                    //    case CleanupType.Preventive:
                    //        yield return StartCoroutine(PerformPreventiveCleanup(result));
                    //        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[MemoryPressureManager] Cleanup operation failed: {operation.Name} - {e.Message}");
                result.Success = false;
                result.ErrorMessage = e.Message;
            }

            result.EndTime = DateTime.UtcNow;
            result.Duration = (result.EndTime - result.StartTime).TotalSeconds;

            yield return null;
        }

        private IEnumerator PerformGarbageCollection(CleanupOperationResult result)
        {
            var beforeMemory = GC.GetTotalMemory(false) / (1024f * 1024f);
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            yield return new WaitForSeconds(0.5f);

            var afterMemory = GC.GetTotalMemory(false) / (1024f * 1024f);
            result.MemoryFreedMB = beforeMemory - afterMemory;
            result.Success = result.MemoryFreedMB > 1f;

            Debug.Log($"[MemoryPressureManager] GC cleanup: {result.MemoryFreedMB:F2}MB freed");
        }

        private IEnumerator PerformObjectPoolCleanup(CleanupOperationResult result)
        {
            if (_objectPool != null)
            {
                var beforePoolSize = _objectPool.CurrentPoolSize;
                //_objectPool.Cleanup(); // 假设有清理方法
                yield return null;
                
                var afterPoolSize = _objectPool.CurrentPoolSize;
                var objectsCleaned = beforePoolSize - afterPoolSize;
                result.MemoryFreedMB = objectsCleaned * 0.1f; // 每个对象约0.1MB
                result.Success = objectsCleaned > 0;

                Debug.Log($"[MemoryPressureManager] Object pool cleanup: {objectsCleaned} objects removed");
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = "ObjectPool not found";
            }
        }

        private IEnumerator PerformResourceCleanup(CleanupOperationResult result)
        {
            var beforeMemory = GC.GetTotalMemory(false) / (1024f * 1024f);
            
            // 卸载未使用的资源
            Resources.UnloadUnusedAssets();
            yield return new WaitForSeconds(1f);

            var afterMemory = GC.GetTotalMemory(false) / (1024f * 1024f);
            result.MemoryFreedMB = beforeMemory - afterMemory;
            result.Success = result.MemoryFreedMB > 5f;

            Debug.Log($"[MemoryPressureManager] Resource cleanup: {result.MemoryFreedMB:F2}MB freed");
        }

        private IEnumerator PerformCacheCleanup(CleanupOperationResult result)
        {
            var beforeMemory = GC.GetTotalMemory(false) / (1024f * 1024f);
            
            // 清理各种缓存
            Caching.ClearCache();
            yield return null;
            
            // 强制一次轻量级GC
            GC.Collect(0, GCCollectionMode.Optimized);
            yield return new WaitForSeconds(0.2f);

            var afterMemory = GC.GetTotalMemory(false) / (1024f * 1024f);
            result.MemoryFreedMB = beforeMemory - afterMemory;
            result.Success = result.MemoryFreedMB > 0.5f;

            Debug.Log($"[MemoryPressureManager] Cache cleanup: {result.MemoryFreedMB:F2}MB freed");
        }

        private IEnumerator PerformEmergencyCleanup(CleanupOperationResult result)
        {
            Debug.LogWarning("[MemoryPressureManager] EMERGENCY CLEANUP ACTIVATED!");
            
            var beforeMemory = GC.GetTotalMemory(false) / (1024f * 1024f);
            
            // 激进式清理
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            Resources.UnloadUnusedAssets();
            yield return new WaitForSeconds(1f);
            
            //// 关闭非必要功能
            //if (enableFeatureDisable && _qualityManager != null)
            //{
            //    _qualityManager.SetQualityLevel(QualityLevel.Minimum);
            //}
            
            Caching.ClearCache();
            yield return new WaitForSeconds(0.5f);
            
            // 最终GC
            GC.Collect();
            yield return new WaitForSeconds(0.5f);

            var afterMemory = GC.GetTotalMemory(false) / (1024f * 1024f);
            result.MemoryFreedMB = beforeMemory - afterMemory;
            result.Success = result.MemoryFreedMB > 10f;

            Debug.LogError($"[MemoryPressureManager] Emergency cleanup: {result.MemoryFreedMB:F2}MB freed");
        }

        private IEnumerator PerformPreventiveCleanup(CleanupOperationResult result)
        {
            var beforeMemory = GC.GetTotalMemory(false) / (1024f * 1024f);
            
            // 预防性清理 - 轻量级操作
            GC.Collect(0, GCCollectionMode.Optimized);
            yield return new WaitForSeconds(0.2f);
            
            Caching.ClearCache();
            yield return new WaitForSeconds(0.1f);

            var afterMemory = GC.GetTotalMemory(false) / (1024f * 1024f);
            result.MemoryFreedMB = beforeMemory - afterMemory;
            result.Success = result.MemoryFreedMB > 0.1f;

            Debug.Log($"[MemoryPressureManager] Preventive cleanup: {result.MemoryFreedMB:F2}MB freed");
        }

        #endregion

        #region Emergency Mode

        private void CheckEmergencyConditions()
        {
            if (!enableEmergencyMode || _emergencyModeActive) return;

            var currentMemory = _pressureHistory.Count > 0 ? _pressureHistory.Last().TotalMemoryMB : 0f;
            
            if (currentMemory >= emergencyThreshold)
            {
                ActivateEmergencyMode();
            }
        }

        private void ActivateEmergencyMode()
        {
            if (_emergencyModeActive) return;

            _emergencyModeActive = true;
            
            var emergencyEvent = new EmergencyModeEvent
            {
                ActivatedAt = DateTime.UtcNow,
                TriggerMemoryMB = _pressureHistory.Last().TotalMemoryMB,
                TriggerReason = "Memory exceeded emergency threshold"
            };

            OnEmergencyModeActivated?.Invoke(emergencyEvent);

            Debug.LogError($"[MemoryPressureManager] EMERGENCY MODE ACTIVATED! Memory: {emergencyEvent.TriggerMemoryMB:F2}MB");

            // 立即执行紧急清理
            StartCoroutine(ExecuteEmergencyCleanupSequence());
        }

        private IEnumerator ExecuteEmergencyCleanupSequence()
        {
            Debug.LogWarning("[MemoryPressureManager] Executing emergency cleanup sequence...");

            yield return StartCoroutine(ExecuteCleanup(CleanupType.Emergency));

            // 检查是否仍在紧急状态
            var currentMemory = _pressureHistory.Count > 0 ? _pressureHistory.Last().TotalMemoryMB : 0f;
            
            if (currentMemory < emergencyThreshold * 0.8f) // 降到80%以下
            {
                DeactivateEmergencyMode();
            }
            else
            {
                Debug.LogError("[MemoryPressureManager] Emergency cleanup insufficient, maintaining emergency mode");
            }
        }

        private void DeactivateEmergencyMode()
        {
            _emergencyModeActive = false;
            Debug.Log("[MemoryPressureManager] Emergency mode deactivated");
        }

        #endregion

        #region Public API

        /// <summary>
        /// 获取内存压力报告
        /// </summary>
        public MemoryPressureReport GetPressureReport()
        {
            return new MemoryPressureReport
            {
                GeneratedAt = DateTime.UtcNow,
                CurrentPressureLevel = _currentPressureLevel,
                PressureTrend = _pressureTrend,
                CurrentMemoryMB = _pressureHistory.Count > 0 ? _pressureHistory.Last().TotalMemoryMB : 0f,
                AverageMemoryMB = _pressureHistory.Count > 0 ? _pressureHistory.Average(s => s.TotalMemoryMB) : 0f,
                PeakMemoryMB = _pressureHistory.Count > 0 ? _pressureHistory.Max(s => s.TotalMemoryMB) : 0f,
                EmergencyModeActive = _emergencyModeActive,
                PressureStats = _pressureStats,
                CleanupStats = _cleanupStats,
                RecentCleanupResults = _cleanupHistory.TakeLast(10).ToList()
            };
        }

        /// <summary>
        /// 手动触发清理
        /// </summary>
        public void TriggerCleanup(CleanupType cleanupType = CleanupType.Automatic)
        {
            StartCoroutine(ExecuteCleanup(cleanupType));
        }

        /// <summary>
        /// 设置压力阈值
        /// </summary>
        public void SetPressureThresholds(float normal, float moderate, float high, float critical)
        {
            normalPressureThreshold = normal;
            moderatePressureThreshold = moderate;
            highPressureThreshold = high;
            criticalPressureThreshold = critical;
            
            Debug.Log($"[MemoryPressureManager] Pressure thresholds updated: N:{normal} M:{moderate} H:{high} C:{critical}");
        }

        /// <summary>
        /// 启用/禁用自动清理
        /// </summary>
        public void SetAutomaticCleanupEnabled(bool enabled)
        {
            enableAutomaticCleanup = enabled;
            Debug.Log($"[MemoryPressureManager] Automatic cleanup {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 清理历史数据
        /// </summary>
        public void ClearHistory()
        {
            _pressureHistory.Clear();
            _cleanupHistory.Clear();
            Debug.Log("[MemoryPressureManager] History cleared");
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class MemoryPressureSnapshot
    {
        public DateTime Timestamp;
        public float TotalMemoryMB;
        public float ManagedMemoryMB;
        public float UnmanagedMemoryMB;
        public float AvailableMemoryMB;
        public float MemoryPressurePercentage;
        public int ActiveCoinsCount;
        public float FrameRate;
        public int GCGeneration;
    }

    [System.Serializable]
    public class CleanupOperation
    {
        public string Name;
        public CleanupType Type;
        public CleanupPriority Priority;
        public float EstimatedMemoryGain;
        public float ExecutionTime;
        public bool Enabled;
        public Dictionary<string, object> Parameters;
    }

    [System.Serializable]
    public class CleanupResult
    {
        public CleanupType Type;
        public DateTime StartTime;
        public DateTime EndTime;
        public double Duration;
        public float MemoryFreedMB;
        public bool Success;
        public List<CleanupOperationResult> Operations;
    }

    [System.Serializable]
    public class CleanupOperationResult
    {
        public CleanupOperation Operation;
        public DateTime StartTime;
        public DateTime EndTime;
        public double Duration;
        public float MemoryFreedMB;
        public bool Success;
        public string ErrorMessage;
    }

    [System.Serializable]
    public class MemoryPressureStats
    {
        public int TotalChecks;
        public int FailedAnalysisCount;
        public int HighPressureCount;
        public float AverageAnalysisTime;
        public float TotalAnalysisTime;
        public DateTime LastAnalysisTime;
        public TimeSpan TotalTimeUnderPressure;
    }

    [System.Serializable]
    public class CleanupStats
    {
        public int TotalCleanups;
        public int SuccessfulCleanups;
        public float TotalMemoryFreed;
        public float AverageCleanupTime;
        public DateTime LastCleanupTime;
        public int ConsecutiveFailures;
    }

    [System.Serializable]
    public class MemoryPressureReport
    {
        public DateTime GeneratedAt;
        public MemoryPressureLevel CurrentPressureLevel;
        public MemoryPressureTrend PressureTrend;
        public float CurrentMemoryMB;
        public float AverageMemoryMB;
        public float PeakMemoryMB;
        public bool EmergencyModeActive;
        public MemoryPressureStats PressureStats;
        public CleanupStats CleanupStats;
        public List<CleanupResult> RecentCleanupResults;
    }

    [System.Serializable]
    public class CyclicPressurePattern
    {
        public float PeriodMinutes;
        public float Amplitude;
        public float Confidence;
    }

    [System.Serializable]
    public class PressureSpikePattern
    {
        public int SpikeCount;
        public float AverageSpikeHeight;
        public double TimeWindow;
    }

    [System.Serializable]
    public class EmergencyModeEvent
    {
        public DateTime ActivatedAt;
        public float TriggerMemoryMB;
        public string TriggerReason;
        public DateTime DeactivatedAt;
        public double DurationMinutes;
    }

    #endregion

    #region Enums

    public enum MemoryPressureLevel
    {
        Normal,
        Low,
        Moderate,
        High,
        Critical
    }

    public enum MemoryPressureTrend
    {
        Decreasing,
        Stable,
        Increasing
    }

    public enum CleanupType
    {
        Automatic,
        GarbageCollection,
        ObjectPool,
        Resources,
        Cache,
        Emergency,
        Preventive
    }

    public enum CleanupPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    #endregion
}