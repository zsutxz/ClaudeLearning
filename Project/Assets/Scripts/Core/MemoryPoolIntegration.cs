using System;
using System.Collections;
using UnityEngine;
using CoinAnimation.Core;
using CoinAnimation.Animation;

namespace CoinAnimation.Core
{
    /// <summary>
    /// Integrates object pool with memory management system
    /// Story 1.3 Task 2 - Memory Management Integration
    /// </summary>
    public class MemoryPoolIntegration : MonoBehaviour
    {
        #region Configuration

        [Header("Integration Settings")]
        [SerializeField] private bool enableIntegration = true;
        [SerializeField] private bool enableMemoryOptimization = true;
        [SerializeField] private bool enableLeakTracking = true;

        [Header("Performance Thresholds")]
        [SerializeField] private float highMemoryThreshold = 80f; // MB
        [SerializeField] private float criticalMemoryThreshold = 120f; // MB
        [SerializeField] private float memoryGrowthRateThreshold = 5f; // MB per minute

        [Header("Automatic Responses")]
        [SerializeField] private bool enableAutoPoolContraction = true;
        [SerializeField] private bool enableAutoGCPrevention = true;
        [SerializeField] private bool enableEmergencyCleanup = true;

        #endregion

        #region Private Fields

        private CoinObjectPool _objectPool;
        private MemoryManagementSystem _memorySystem;
        private CoinAnimationManager _animationManager;

        // Performance monitoring
        private float _lastPerformanceCheck = 0f;
        private float _performanceCheckInterval = 2f;
        private MemoryStatistics _lastMemoryStats;

        // Auto-response tracking
        private bool _autoContractionActive = false;
        private bool _gcPreventionActive = false;
        private DateTime _lastEmergencyCleanup = DateTime.MinValue;

        #endregion

        #region Properties

        /// <summary>
        /// Is integration currently active
        /// </summary>
        public bool IsIntegrationActive => enableIntegration && _objectPool != null && _memorySystem != null;

        /// <summary>
        /// Current integrated performance metrics
        /// </summary>
        public IntegratedPerformanceMetrics CurrentMetrics => GetIntegratedMetrics();

        #endregion

        #region Events

        /// <summary>
        /// Triggered when automatic pool optimization occurs
        /// </summary>
        public event Action<IntegratedOptimizationEventArgs> OnIntegratedOptimization;

        /// <summary>
        /// Triggered when memory-pool performance correlation is detected
        /// </summary>
        public event Action<PerformanceCorrelationEventArgs> OnPerformanceCorrelation;

        #endregion

        #region Initialization

        private void Start()
        {
            InitializeIntegration();
        }

        private void InitializeIntegration()
        {
            // Find required components
            _objectPool = FindObjectOfType<CoinObjectPool>();
            _memorySystem = FindObjectOfType<MemoryManagementSystem>();
            _animationManager = CoinAnimationManager.Instance;

            // Validate integration requirements
            if (!ValidateIntegrationRequirements())
            {
                enableIntegration = false;
                return;
            }

            // Subscribe to memory system events
            SubscribeToMemoryEvents();

            // Subscribe to pool events
            SubscribeToPoolEvents();

            Debug.Log("[MemoryPoolIntegration] Integration initialized successfully");
        }

        private bool ValidateIntegrationRequirements()
        {
            if (_objectPool == null)
            {
                Debug.LogError("[MemoryPoolIntegration] CoinObjectPool not found!");
                return false;
            }

            if (_memorySystem == null)
            {
                Debug.LogError("[MemoryPoolIntegration] MemoryManagementSystem not found!");
                return false;
            }

            if (_animationManager == null)
            {
                Debug.LogError("[MemoryPoolIntegration] CoinAnimationManager not found!");
                return false;
            }

            return true;
        }

        private void SubscribeToMemoryEvents()
        {
            _memorySystem.OnMemoryWarning += OnMemoryWarningHandler;
            _memorySystem.OnMemoryCritical += OnMemoryCriticalHandler;
            _memorySystem.OnMemoryLeakDetected += OnMemoryLeakDetectedHandler;
            _memorySystem.OnMemoryCleanup += OnMemoryCleanupHandler;
        }

        private void SubscribeToPoolEvents()
        {
            // These events would need to be exposed in CoinObjectPool
            // For now, we'll monitor through polling
        }

        #endregion

        #region Update Loop

        private void Update()
        {
            if (!enableIntegration) return;

            // Periodic performance monitoring
            if (Time.time - _lastPerformanceCheck >= _performanceCheckInterval)
            {
                MonitorIntegratedPerformance();
                _lastPerformanceCheck = Time.time;
            }
        }

        #endregion

        #region Event Handlers

        private void OnMemoryWarningHandler(object sender, MemoryWarningEventArgs args)
        {
            Debug.LogWarning($"[MemoryPoolIntegration] Memory warning: {args.CurrentMemoryMB:F2} MB " +
                           $"(threshold: {args.WarningThresholdMB:F2} MB)");

            if (enableAutoPoolContraction)
            {
                StartCoroutine(OptimizePoolForMemory("MemoryWarning"));
            }

            if (enableAutoGCPrevention && !_gcPreventionActive)
            {
                EnableGCPreventionForAnimation();
            }
        }

        private void OnMemoryCriticalHandler(object sender, MemoryCriticalEventArgs args)
        {
            Debug.LogError($"[MemoryPoolIntegration] Memory critical: {args.CurrentMemoryMB:F2} MB " +
                          $"(peak: {args.PeakMemoryMB:F2} MB)");

            if (enableEmergencyCleanup)
            {
                PerformEmergencyMemoryResponse();
            }
        }

        private void OnMemoryLeakDetectedHandler(object sender, MemoryLeakReport report)
        {
            if (enableLeakTracking)
            {
                HandleMemoryLeak(report);
            }
        }

        private void OnMemoryCleanupHandler(object sender, MemoryCleanupEventArgs args)
        {
            Debug.Log($"[MemoryPoolIntegration] Memory cleanup freed {args.MemoryFreedMB:F2} MB " +
                     $"in {args.Duration.TotalSeconds:F2}s");
        }

        #endregion

        #region Integrated Performance Monitoring

        private void MonitorIntegratedPerformance()
        {
            var currentMetrics = GetIntegratedMetrics();
            var memoryStats = _memorySystem.GetMemoryStatistics();

            // Check for performance correlations
            CheckPerformanceCorrelations(currentMetrics, memoryStats);

            // Update memory statistics tracking
            _lastMemoryStats = memoryStats;

            // Trigger automatic optimizations if needed
            TriggerAutomaticOptimizations(currentMetrics, memoryStats);
        }

        private void CheckPerformanceCorrelations(IntegratedPerformanceMetrics metrics, MemoryStatistics memoryStats)
        {
            // Correlate pool performance with memory usage
            if (metrics.PoolHitRate < 0.8f && memoryStats.MemoryGrowthRateMB > memoryGrowthRateThreshold)
            {
                var correlationArgs = new PerformanceCorrelationEventArgs
                {
                    PoolHitRate = metrics.PoolHitRate,
                    MemoryGrowthRate = memoryStats.MemoryGrowthRateMB,
                    CorrelationType = "LowPoolHitRate_HighMemoryGrowth",
                    Severity = CorrelationSeverity.High,
                    RecommendedAction = "Reduce pool size or optimize memory usage"
                };

                OnPerformanceCorrelation?.Invoke(this, correlationArgs);
                Debug.LogWarning($"[MemoryPoolIntegration] Performance correlation detected: {correlationArgs.CorrelationType}");
            }

            // Check for memory leaks affecting pool performance
            if (memoryStats.LeakCount > 10 && metrics.ActiveCoins > 50)
            {
                var correlationArgs = new PerformanceCorrelationEventArgs
                {
                    PoolHitRate = metrics.PoolHitRate,
                    MemoryGrowthRate = memoryStats.MemoryGrowthRateMB,
                    CorrelationType = "MemoryLeaks_PoolPerformance",
                    Severity = CorrelationSeverity.Critical,
                    RecommendedAction = "Investigate memory leaks in coin objects"
                };

                OnPerformanceCorrelation?.Invoke(this, correlationArgs);
                Debug.LogError($"[MemoryPoolIntegration] Critical correlation: {correlationArgs.CorrelationType}");
            }
        }

        private void TriggerAutomaticOptimizations(IntegratedPerformanceMetrics metrics, MemoryStatistics memoryStats)
        {
            // Automatic pool contraction for high memory usage
            if (enableAutoPoolContraction && memoryStats.IsMemoryWarning && !_autoContractionActive)
            {
                StartCoroutine(OptimizePoolForMemory("AutoOptimization"));
            }

            // Automatic GC prevention for heavy animation periods
            if (enableAutoGCPrevention && metrics.ActiveCoins > 30 && !_gcPreventionActive)
            {
                EnableGCPreventionForAnimation();
            }
        }

        #endregion

        #region Automatic Optimization Routines

        private IEnumerator OptimizePoolForMemory(string reason)
        {
            if (_autoContractionActive) yield break;

            _autoContractionActive = true;
            Debug.Log($"[MemoryPoolIntegration] Starting pool optimization for: {reason}");

            var metricsBefore = GetIntegratedMetrics();
            var memoryBefore = _memorySystem.CurrentMemoryUsageMB;

            // Step 1: Force return idle coins to pool
            if (_animationManager != null)
            {
                // This would need to be implemented in CoinAnimationManager
                // For now, we'll rely on the pool's automatic contraction
            }

            // Step 2: Trigger pool contraction
            yield return new WaitForSeconds(0.5f);

            // Step 3: Force garbage collection
            _memorySystem.DisableGCPrevention();
            yield return new WaitForSeconds(0.1f);
            System.GC.Collect();
            yield return new WaitForSeconds(0.5f);

            var metricsAfter = GetIntegratedMetrics();
            var memoryAfter = _memorySystem.CurrentMemoryUsageMB;

            var optimizationArgs = new IntegratedOptimizationEventArgs
            {
                Reason = reason,
                MemoryBeforeMB = memoryBefore,
                MemoryAfterMB = memoryAfter,
                MemoryFreedMB = memoryBefore - memoryAfter,
                PoolSizeBefore = metricsAfter.PoolSize,
                PoolSizeAfter = metricsAfter.PoolSize,
                Duration = DateTime.UtcNow.Subtract(DateTime.UtcNow.AddSeconds(-2f)), // Simplified
                Success = memoryAfter < memoryBefore
            };

            OnIntegratedOptimization?.Invoke(this, optimizationArgs);

            _autoContractionActive = false;

            Debug.Log($"[MemoryPoolIntegration] Pool optimization completed: " +
                     $"Memory freed: {optimizationArgs.MemoryFreedMB:F2} MB, " +
                     $"Success: {optimizationArgs.Success}");
        }

        private void EnableGCPreventionForAnimation()
        {
            if (_gcPreventionActive) return;

            _gcPreventionActive = true;
            _memorySystem.EnableGCPrevention();

            // Schedule automatic disable
            StartCoroutine(DisableGCPreventionAfterDelay(5f));

            Debug.Log("[MemoryPoolIntegration] GC prevention enabled for animation period");
        }

        private IEnumerator DisableGCPreventionAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (_gcPreventionActive)
            {
                _memorySystem.DisableGCPrevention();
                _gcPreventionActive = false;
                Debug.Log("[MemoryPoolIntegration] GC prevention automatically disabled");
            }
        }

        private void PerformEmergencyMemoryResponse()
        {
            if (DateTime.UtcNow.Subtract(_lastEmergencyCleanup).TotalMinutes < 1f)
            {
                Debug.LogWarning("[MemoryPoolIntegration] Emergency cleanup cooldown active");
                return;
            }

            _lastEmergencyCleanup = DateTime.UtcNow;
            Debug.LogError("[MemoryPoolIntegration] Performing emergency memory response");

            // Emergency steps:
            // 1. Force all coins back to pool
            // 2. Reduce pool size to minimum
            // 3. Force multiple garbage collections
            // 4. Reset memory statistics

            StartCoroutine(EmergencyMemoryCleanupCoroutine());
        }

        private IEnumerator EmergencyMemoryCleanupCoroutine()
        {
            var memoryBefore = _memorySystem.CurrentMemoryUsageMB;

            // Step 1: Disable GC prevention
            _memorySystem.DisableGCPrevention();
            _gcPreventionActive = false;

            // Step 2: Force cleanup
            _animationManager?.Cleanup();

            // Step 3: Multiple garbage collections
            for (int i = 0; i < 3; i++)
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                yield return new WaitForSeconds(0.5f);
            }

            // Step 4: Reset memory statistics
            _memorySystem.ResetStatistics();

            var memoryAfter = _memorySystem.CurrentMemoryUsageMB;
            var memoryFreed = memoryBefore - memoryAfter;

            Debug.Log($"[MemoryPoolIntegration] Emergency cleanup freed {memoryFreed:F2} MB of memory");
        }

        #endregion

        #region Memory Leak Handling

        private void HandleMemoryLeak(MemoryLeakReport report)
        {
            Debug.LogWarning($"[MemoryPoolIntegration] Handling memory leak: {report.ObjectKey} " +
                           $"(Category: {report.Category}, Age: {report.Age.TotalMinutes:F1} min)");

            // If leak is related to coin objects, take specific action
            if (report.Category.Contains("Coin") || report.Category.Contains("Animation"))
            {
                HandleCoinRelatedMemoryLeak(report);
            }
        }

        private void HandleCoinRelatedMemoryLeak(MemoryLeakReport report)
        {
            // Force cleanup of coin-related objects
            if (_animationManager != null)
            {
                // This would need specific implementation based on leak details
                StartCoroutine(CleanupCoinRelatedLeak(report));
            }
        }

        private IEnumerator CleanupCoinRelatedLeak(MemoryLeakReport report)
        {
            Debug.Log($"[MemoryPoolIntegration] Cleaning up coin-related leak: {report.ObjectKey}");

            // Force return of all active coins
            _animationManager.Cleanup();

            // Force garbage collection
            System.GC.Collect();
            yield return new WaitForSeconds(1f);

            Debug.Log("[MemoryPoolIntegration] Coin-related leak cleanup completed");
        }

        #endregion

        #region Utility Methods

        private IntegratedPerformanceMetrics GetIntegratedMetrics()
        {
            var poolMetrics = _objectPool?.GetPerformanceMetrics() ?? new PoolPerformanceMetrics();
            var memoryStats = _memorySystem?.GetMemoryStatistics() ?? new MemoryStatistics();

            return new IntegratedPerformanceMetrics
            {
                PoolSize = poolMetrics.PoolSize,
                ActiveCoins = poolMetrics.ActiveCoins,
                AvailableCoins = poolMetrics.AvailableCoins,
                PoolHitRate = poolMetrics.PoolHitRate,
                MemoryUsageMB = memoryStats.CurrentMemoryMB,
                MemoryGrowthRateMB = memoryStats.MemoryGrowthRateMB,
                IsGCPreventionActive = memoryStats.IsGCPreventionActive,
                LeakCount = memoryStats.LeakCount,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Get comprehensive integration report
        /// </summary>
        public IntegrationReport GetIntegrationReport()
        {
            return new IntegrationReport
            {
                IsIntegrationActive = IsIntegrationActive,
                CurrentMetrics = GetIntegratedMetrics(),
                MemoryStatistics = _memorySystem?.GetMemoryStatistics() ?? new MemoryStatistics(),
                PoolMetrics = _objectPool?.GetPerformanceMetrics() ?? new PoolPerformanceMetrics(),
                AutoContractionActive = _autoContractionActive,
                GCPreventionActive = _gcPreventionActive,
                LastEmergencyCleanup = _lastEmergencyCleanup,
                GeneratedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_memorySystem != null)
            {
                _memorySystem.OnMemoryWarning -= OnMemoryWarningHandler;
                _memorySystem.OnMemoryCritical -= OnMemoryCriticalHandler;
                _memorySystem.OnMemoryLeakDetected -= OnMemoryLeakDetectedHandler;
                _memorySystem.OnMemoryCleanup -= OnMemoryCleanupHandler;
            }

            StopAllCoroutines();

            Debug.Log("[MemoryPoolIntegration] Integration system destroyed and cleaned up");
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Integrated performance metrics combining pool and memory data
    /// </summary>
    [Serializable]
    public class IntegratedPerformanceMetrics
    {
        public int PoolSize;
        public int ActiveCoins;
        public int AvailableCoins;
        public float PoolHitRate;
        public float MemoryUsageMB;
        public float MemoryGrowthRateMB;
        public bool IsGCPreventionActive;
        public int LeakCount;
        public DateTime Timestamp;
    }

    /// <summary>
    /// Integration optimization event arguments
    /// </summary>
    public class IntegratedOptimizationEventArgs : EventArgs
    {
        public string Reason;
        public float MemoryBeforeMB;
        public float MemoryAfterMB;
        public float MemoryFreedMB;
        public int PoolSizeBefore;
        public int PoolSizeAfter;
        public TimeSpan Duration;
        public bool Success;
        public DateTime Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Performance correlation event arguments
    /// </summary>
    public class PerformanceCorrelationEventArgs : EventArgs
    {
        public float PoolHitRate;
        public float MemoryGrowthRate;
        public string CorrelationType;
        public CorrelationSeverity Severity;
        public string RecommendedAction;
        public DateTime Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Comprehensive integration report
    /// </summary>
    [Serializable]
    public class IntegrationReport
    {
        public bool IsIntegrationActive;
        public IntegratedPerformanceMetrics CurrentMetrics;
        public MemoryStatistics MemoryStatistics;
        public PoolPerformanceMetrics PoolMetrics;
        public bool AutoContractionActive;
        public bool GCPreventionActive;
        public DateTime LastEmergencyCleanup;
        public DateTime GeneratedAt;
    }

    /// <summary>
    /// Correlation severity levels
    /// </summary>
    public enum CorrelationSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    #endregion
}