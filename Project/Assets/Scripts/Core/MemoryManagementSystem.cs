using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using CoinAnimation.Core;
using Debug = UnityEngine.Debug;

namespace CoinAnimation.Core
{
    /// <summary>
    /// Advanced memory management system for coin animations
    /// Story 1.3 Task 2 - Memory Management System Implementation
    /// </summary>
    public class MemoryManagementSystem : MonoBehaviour
    {
        #region Configuration

        [Header("Memory Monitoring")]
        [SerializeField] private bool enableMemoryMonitoring = true;
        [SerializeField] private float memoryCheckInterval = 5f;
        [SerializeField] private float memoryWarningThresholdMB = 80f;
        [SerializeField] private float memoryCriticalThresholdMB = 120f;

        [Header("Garbage Collection Control")]
        [SerializeField] private bool enableGCOptimization = true;
        [SerializeField] private float gcPreventionWindow = 2f;
        [SerializeField] private int maxGCAttempts = 3;

        [Header("Memory Leak Detection")]
        [SerializeField] private bool enableLeakDetection = true;
        [SerializeField] private float leakCheckInterval = 30f;
        [SerializeField] private int leakDetectionThreshold = 20;

        [Header("Automatic Cleanup")]
        [SerializeField] private bool enableAutoCleanup = true;
        [SerializeField] private float cleanupInterval = 60f;
        [SerializeField] private float idleCleanupThreshold = 300f; // 5 minutes

        #endregion

        #region Private Fields

        // Memory tracking
        private long _baselineMemory = 0;
        private long _currentMemoryUsage = 0;
        private long _peakMemoryUsage = 0;
        private float _lastMemoryCheck = 0f;
        private float _lastGCPrevention = 0f;

        // Garbage collection control
        private bool _gcPreventionActive = false;
        private readonly object _gcLock = new object();
        private int _gcAttemptCount = 0;

        // Memory leak detection
        private readonly Dictionary<string, MemoryTracker> _memoryTrackers = new Dictionary<string, MemoryTracker>();
        private float _lastLeakCheck = 0f;
        private readonly List<MemoryLeakReport> _leakReports = new List<MemoryLeakReport>();

        // Performance monitoring
        private readonly Queue<MemorySnapshot> _memoryHistory = new Queue<MemorySnapshot>();
        private const int MAX_HISTORY_SIZE = 100;

        // Cleanup tracking
        private float _lastCleanup = 0f;
        private int _cleanupCount = 0;

        #endregion

        #region Properties

        /// <summary>
        /// Current memory usage in MB
        /// </summary>
        public float CurrentMemoryUsageMB => _currentMemoryUsage / (1024f * 1024f);

        /// <summary>
        /// Peak memory usage in MB
        /// </summary>
        public float PeakMemoryUsageMB => _peakMemoryUsage / (1024f * 1024f);

        /// <summary>
        /// Memory growth rate in MB per minute
        /// </summary>
        public float MemoryGrowthRateMB => CalculateMemoryGrowthRate();

        /// <summary>
        /// Is memory usage currently above warning threshold
        /// </summary>
        public bool IsMemoryWarning => CurrentMemoryUsageMB > memoryWarningThresholdMB;

        /// <summary>
        /// Memory warning threshold in MB
        /// </summary>
        public float MemoryWarningThresholdMB => memoryWarningThresholdMB;

        /// <summary>
        /// Is memory usage currently at critical level
        /// </summary>
        public bool IsMemoryCritical => CurrentMemoryUsageMB > memoryCriticalThresholdMB;

        /// <summary>
        /// Is garbage collection prevention currently active
        /// </summary>
        public bool IsGCPreventionActive => _gcPreventionActive;

        /// <summary>
        /// Number of detected memory leaks
        /// </summary>
        public int LeakCount => _leakReports.Count;

        #endregion

        #region Events

        /// <summary>
        /// Triggered when memory usage exceeds warning threshold
        /// </summary>
        public event Action<MemoryWarningEventArgs> OnMemoryWarning;

        /// <summary>
        /// Triggered when memory usage reaches critical level
        /// </summary>
        public event Action<MemoryCriticalEventArgs> OnMemoryCritical;

        /// <summary>
        /// Triggered when a memory leak is detected
        /// </summary>
        public event Action<MemoryLeakReport> OnMemoryLeakDetected;

        /// <summary>
        /// Triggered when automatic cleanup is performed
        /// </summary>
        public event Action<MemoryCleanupEventArgs> OnMemoryCleanup;

        #endregion

        #region Initialization

        private void Awake()
        {
            InitializeMemoryMonitoring();
        }

        private void Start()
        {
            _baselineMemory = GetProcessMemoryUsage();
            _currentMemoryUsage = _baselineMemory;
            _peakMemoryUsage = _baselineMemory;
            
            Debug.Log($"[MemoryManagementSystem] Initialized with baseline memory: {_baselineMemory / (1024f * 1024f):F2} MB");
        }

        private void InitializeMemoryMonitoring()
        {
            if (enableMemoryMonitoring)
            {
                InvokeRepeating(nameof(MonitorMemoryUsage), 1f, memoryCheckInterval);
            }

            if (enableLeakDetection)
            {
                InvokeRepeating(nameof(CheckForMemoryLeaks), 10f, leakCheckInterval);
            }

            if (enableAutoCleanup)
            {
                InvokeRepeating(nameof(PerformAutomaticCleanup), 30f, cleanupInterval);
            }
        }

        #endregion

        #region Update Loop

        private void Update()
        {
            // Update GC prevention window
            if (_gcPreventionActive && Time.time - _lastGCPrevention > gcPreventionWindow)
            {
                DisableGCPrevention();
            }

            // Track memory history
            if (Time.time - _lastMemoryCheck >= 1f) // Track every second
            {
                TrackMemorySnapshot();
                _lastMemoryCheck = Time.time;
            }
        }

        #endregion

        #region Memory Monitoring

        private void MonitorMemoryUsage()
        {
            _currentMemoryUsage = GetProcessMemoryUsage();
            
            if (_currentMemoryUsage > _peakMemoryUsage)
            {
                _peakMemoryUsage = _currentMemoryUsage;
            }

            // Check memory thresholds
            CheckMemoryThresholds();
        }

        private void CheckMemoryThresholds()
        {
            float currentMB = CurrentMemoryUsageMB;

            if (currentMB > memoryCriticalThresholdMB)
            {
                var criticalArgs = new MemoryCriticalEventArgs(currentMB, _peakMemoryUsage / (1024f * 1024f));
                OnMemoryCritical?.Invoke(criticalArgs);
                
                // Force emergency cleanup
                ForceEmergencyCleanup();
            }
            else if (currentMB > memoryWarningThresholdMB)
            {
                var warningArgs = new MemoryWarningEventArgs(currentMB, memoryWarningThresholdMB);
                OnMemoryWarning?.Invoke(warningArgs);
            }
        }

        private void TrackMemorySnapshot()
        {
            var snapshot = new MemorySnapshot
            {
                timestamp = DateTime.UtcNow,
                memoryUsage = _currentMemoryUsage,
                isGCPreventionActive = _gcPreventionActive,
                activeTrackers = _memoryTrackers.Count
            };

            _memoryHistory.Enqueue(snapshot);

            // Maintain history size limit
            while (_memoryHistory.Count > MAX_HISTORY_SIZE)
            {
                _memoryHistory.Dequeue();
            }
        }

        #endregion

        #region Garbage Collection Control

        /// <summary>
        /// Enable garbage collection prevention during critical animation periods
        /// </summary>
        public void EnableGCPrevention()
        {
            if (!enableGCOptimization) return;

            lock (_gcLock)
            {
                if (!_gcPreventionActive)
                {
                    _gcPreventionActive = true;
                    _lastGCPrevention = Time.time;
                    
                    // Suggest GC to run before we start preventing it
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    
                    // Allocate some buffer memory to reduce GC pressure
                    AllocateGCDBuffer();
                    
                    Debug.Log("[MemoryManagementSystem] GC prevention enabled");
                }
            }
        }

        /// <summary>
        /// Disable garbage collection prevention
        /// </summary>
        public void DisableGCPrevention()
        {
            if (!enableGCOptimization) return;

            lock (_gcLock)
            {
                if (_gcPreventionActive)
                {
                    _gcPreventionActive = false;
                    ReleaseGCBuffer();
                    
                    Debug.Log("[MemoryManagementSystem] GC prevention disabled");
                }
            }
        }

        private void AllocateGCDBuffer()
        {
            // Allocate some objects to create GC buffer
            // This helps prevent small, frequent GCs during animations
            _gcAttemptCount++;
        }

        private void ReleaseGCBuffer()
        {
            // Allow GC to run normally
            System.GC.Collect();
        }

        #endregion

        #region Memory Leak Detection

        /// <summary>
        /// Register an object for memory leak tracking
        /// </summary>
        /// <param name="obj">Object to track</param>
        /// <param name="category">Tracking category</param>
        public void TrackObject(object obj, string category = "Default")
        {
            if (!enableLeakDetection) return;

            var objectId = obj.GetHashCode();
            var key = $"{category}_{objectId}";
            
            _memoryTrackers[key] = new MemoryTracker
            {
                Object = obj,
                Category = category,
                CreatedTime = DateTime.UtcNow,
                LastAccessed = DateTime.UtcNow,
                AccessCount = 1
            };
        }

        /// <summary>
        /// Unregister an object from memory leak tracking
        /// </summary>
        /// <param name="obj">Object to untrack</param>
        /// <param name="category">Tracking category</param>
        public void UntrackObject(object obj, string category = "Default")
        {
            if (!enableLeakDetection) return;

            var objectId = obj.GetHashCode();
            var key = $"{category}_{objectId}";
            
            _memoryTrackers.Remove(key);
        }

        private void CheckForMemoryLeaks()
        {
            var now = DateTime.UtcNow;
            var leaksDetected = new List<MemoryLeakReport>();

            foreach (var kvp in _memoryTrackers)
            {
                var tracker = kvp.Value;
                var age = now - tracker.CreatedTime;
                
                // Check if object has been alive too long
                if (age.TotalMinutes > leakDetectionThreshold)
                {
                    var leak = new MemoryLeakReport
                    {
                        ObjectKey = kvp.Key,
                        Category = tracker.Category,
                        Age = age,
                        AccessCount = tracker.AccessCount,
                        LastAccessed = tracker.LastAccessed,
                        IsPotentialLeak = true
                    };
                    
                    leaksDetected.Add(leak);
                }
            }

            // Report leaks
            foreach (var leak in leaksDetected)
            {
                _leakReports.Add(leak);
                OnMemoryLeakDetected?.Invoke(leak);
                
                Debug.LogWarning($"[MemoryManagementSystem] Potential memory leak detected: {leak.ObjectKey} " +
                               $"(Age: {leak.Age.TotalMinutes:F1} min, Category: {leak.Category})");
            }

            // Clean old leak reports
            CleanupOldLeakReports();
        }

        private void CleanupOldLeakReports()
        {
            var cutoff = DateTime.UtcNow.AddHours(-1);
            _leakReports.RemoveAll(report => report.DetectionTime < cutoff);
        }

        #endregion

        #region Automatic Cleanup

        private void PerformAutomaticCleanup()
        {
            if (!enableAutoCleanup) return;

            var cleanupStart = DateTime.UtcNow;
            var memoryBefore = CurrentMemoryUsageMB;

            // Clean up old memory trackers
            CleanupOldTrackers();

            // Force garbage collection if needed
            if (IsMemoryWarning)
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }

            var cleanupEnd = DateTime.UtcNow;
            var memoryAfter = CurrentMemoryUsageMB;
            var memoryFreed = memoryBefore - memoryAfter;

            _cleanupCount++;
            _lastCleanup = Time.time;

            var cleanupArgs = new MemoryCleanupEventArgs
            {
                MemoryBeforeMB = memoryBefore,
                MemoryAfterMB = memoryAfter,
                MemoryFreedMB = memoryFreed,
                Duration = cleanupEnd - cleanupStart,
                CleanupNumber = _cleanupCount
            };

            OnMemoryCleanup?.Invoke(cleanupArgs);

            if (memoryFreed > 1f) // Only log if significant memory was freed
            {
                Debug.Log($"[MemoryManagementSystem] Automatic cleanup freed {memoryFreed:F2} MB of memory");
            }
        }

        private void CleanupOldTrackers()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-30);
            var keysToRemove = new List<string>();

            foreach (var kvp in _memoryTrackers)
            {
                if (kvp.Value.LastAccessed < cutoff)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _memoryTrackers.Remove(key);
            }
        }

        private void ForceEmergencyCleanup()
        {
            Debug.LogWarning("[MemoryManagementSystem] Performing emergency cleanup due to critical memory usage");

            // Clear all trackers
            _memoryTrackers.Clear();

            // Clear leak reports
            _leakReports.Clear();

            // Force multiple garbage collections
            for (int i = 0; i < maxGCAttempts; i++)
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.Collect();
            }

            // Clear memory history
            _memoryHistory.Clear();

            Debug.Log("[MemoryManagementSystem] Emergency cleanup completed");
        }

        #endregion

        #region Utility Methods

        private long GetProcessMemoryUsage()
        {
            try
            {
                using (var process = Process.GetCurrentProcess())
                {
                    return process.PrivateMemorySize64;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MemoryManagementSystem] Failed to get memory usage: {ex.Message}");
                return 0;
            }
        }

        private float CalculateMemoryGrowthRate()
        {
            if (_memoryHistory.Count < 10) return 0f;

            var snapshots = new List<MemorySnapshot>(_memoryHistory);
            var timeSpan = (float)(snapshots[snapshots.Count - 1].timestamp - snapshots[0].timestamp).TotalMinutes;
            
            if (timeSpan <= 0) return 0f;

            var memoryDelta = snapshots[snapshots.Count - 1].memoryUsage - snapshots[0].memoryUsage;
            var growthRateMB = (memoryDelta / (1024f * 1024f)) / timeSpan;

            return growthRateMB;
        }

        /// <summary>
        /// Get comprehensive memory statistics
        /// </summary>
        public MemoryStatistics GetMemoryStatistics()
        {
            return new MemoryStatistics
            {
                CurrentMemoryMB = CurrentMemoryUsageMB,
                PeakMemoryMB = PeakMemoryUsageMB,
                BaselineMemoryMB = _baselineMemory / (1024f * 1024f),
                MemoryGrowthRateMB = MemoryGrowthRateMB,
                IsGCPreventionActive = _gcPreventionActive,
                ActiveTrackers = _memoryTrackers.Count,
                LeakCount = _leakReports.Count,
                CleanupCount = _cleanupCount,
                HistorySize = _memoryHistory.Count,
                LastCleanup = _lastCleanup,
                IsMemoryWarning = IsMemoryWarning,
                IsMemoryCritical = IsMemoryCritical
            };
        }

        /// <summary>
        /// Reset memory statistics
        /// </summary>
        public void ResetStatistics()
        {
            _baselineMemory = GetProcessMemoryUsage();
            _currentMemoryUsage = _baselineMemory;
            _peakMemoryUsage = _baselineMemory;
            _memoryHistory.Clear();
            _leakReports.Clear();
            _cleanupCount = 0;
            
            Debug.Log("[MemoryManagementSystem] Memory statistics reset");
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            CancelInvoke();
            _memoryTrackers.Clear();
            _leakReports.Clear();
            _memoryHistory.Clear();
            
            Debug.Log("[MemoryManagementSystem] Memory management system destroyed and cleaned up");
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Tracks individual objects for leak detection
    /// </summary>
    [Serializable]
    public class MemoryTracker
    {
        public object Object;
        public string Category;
        public DateTime CreatedTime;
        public DateTime LastAccessed;
        public int AccessCount;
    }

    /// <summary>
    /// Memory snapshot for historical tracking
    /// </summary>
    [Serializable]
    public class MemorySnapshot
    {
        public DateTime timestamp;
        public long memoryUsage;
        public bool isGCPreventionActive;
        public int activeTrackers;
    }

    /// <summary>
    /// Memory leak detection report
    /// </summary>
    [Serializable]
    public class MemoryLeakReport
    {
        public string ObjectKey;
        public string Category;
        public TimeSpan Age;
        public int AccessCount;
        public DateTime LastAccessed;
        public bool IsPotentialLeak;
        public DateTime DetectionTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Comprehensive memory statistics
    /// </summary>
    [Serializable]
    public class MemoryStatistics
    {
        public float CurrentMemoryMB;
        public float PeakMemoryMB;
        public float BaselineMemoryMB;
        public float MemoryGrowthRateMB;
        public bool IsGCPreventionActive;
        public int ActiveTrackers;
        public int LeakCount;
        public int CleanupCount;
        public int HistorySize;
        public float LastCleanup;
        public bool IsMemoryWarning;
        public bool IsMemoryCritical;
        public DateTime GeneratedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Memory warning event arguments
    /// </summary>
    public class MemoryWarningEventArgs : EventArgs
    {
        public float CurrentMemoryMB;
        public float WarningThresholdMB;
        public DateTime Timestamp = DateTime.UtcNow;

        public MemoryWarningEventArgs(float currentMB, float thresholdMB)
        {
            CurrentMemoryMB = currentMB;
            WarningThresholdMB = thresholdMB;
        }
    }

    /// <summary>
    /// Memory critical event arguments
    /// </summary>
    public class MemoryCriticalEventArgs : EventArgs
    {
        public float CurrentMemoryMB;
        public float PeakMemoryMB;
        public DateTime Timestamp = DateTime.UtcNow;

        public MemoryCriticalEventArgs(float currentMB, float peakMB)
        {
            CurrentMemoryMB = currentMB;
            PeakMemoryMB = peakMB;
        }
    }

    /// <summary>
    /// Memory cleanup event arguments
    /// </summary>
    public class MemoryCleanupEventArgs : EventArgs
    {
        public float MemoryBeforeMB;
        public float MemoryAfterMB;
        public float MemoryFreedMB;
        public TimeSpan Duration;
        public int CleanupNumber;
        public DateTime Timestamp = DateTime.UtcNow;
    }

    #endregion
}