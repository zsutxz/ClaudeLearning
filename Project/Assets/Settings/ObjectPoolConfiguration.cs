using System;
using UnityEngine;

namespace CoinAnimation.Core
{
    /// <summary>
    /// Configuration asset for object pool settings
    /// Story 1.3 Task 1 - Configurable pool parameters
    /// </summary>
    [CreateAssetMenu(fileName = "ObjectPoolConfiguration", menuName = "Coin Animation/Object Pool Configuration")]
    [Serializable]
    public class ObjectPoolConfiguration : ScriptableObject
    {
        [Header("Pool Size Configuration")]
        [Tooltip("Initial number of coins to create when pool initializes")]
        [Range(5, 100)]
        public int initialPoolSize = 20;

        [Tooltip("Maximum number of coins the pool can contain")]
        [Range(50, 500)]
        public int maxPoolSize = 100;

        [Tooltip("Number of coins to create when pool expands")]
        [Range(5, 50)]
        public int expansionBatchSize = 10;

        [Header("Pool Behavior")]
        [Tooltip("Automatically expand pool when no coins available")]
        public bool enableAutoExpansion = true;

        [Tooltip("Automatically contract pool when coins are idle")]
        public bool enableAutoContraction = true;

        [Tooltip("Seconds between pool contraction checks")]
        [Range(10f, 300f)]
        public float contractionCheckInterval = 30f;

        [Tooltip("Seconds a coin must be idle before being eligible for contraction")]
        [Range(30f, 300f)]
        public float idleTimeBeforeContraction = 60f;

        [Header("Performance Monitoring")]
        [Tooltip("Enable detailed performance monitoring and reporting")]
        public bool enablePerformanceMonitoring = true;

        [Tooltip("Seconds between performance reports")]
        [Range(5f, 60f)]
        public float performanceReportInterval = 10f;

        [Header("Memory Management")]
        [Tooltip("Target memory usage for the pool in MB")]
        [Range(10f, 200f)]
        public float targetMemoryUsageMB = 50f;

        [Tooltip("Memory usage threshold for performance warnings")]
        [Range(0.5f, 2f)]
        public float memoryWarningThreshold = 1.5f;

        [Header("Advanced Settings")]
        [Tooltip("Minimum pool size (cannot contract below this)")]
        [Range(5, 50)]
        public int minimumPoolSize = 10;

        [Tooltip("Enable thread-safe operations (recommended for multiplayer)")]
        public bool enableThreadSafety = true;

        [Tooltip("Pool warm-up strategy on initialization")]
        public PoolWarmUpStrategy warmUpStrategy = PoolWarmUpStrategy.PreWarmToInitial;

        [Tooltip("Enable pool statistics and analytics")]
        public bool enablePoolStatistics = true;

        #region Validation

        /// <summary>
        /// Validate configuration values and fix invalid ones
        /// </summary>
        public void ValidateAndFix()
        {
            // Ensure logical relationships between values
            initialPoolSize = Mathf.Clamp(initialPoolSize, 5, maxPoolSize);
            maxPoolSize = Mathf.Max(maxPoolSize, initialPoolSize);
            expansionBatchSize = Mathf.Clamp(expansionBatchSize, 1, maxPoolSize / 2);
            minimumPoolSize = Mathf.Clamp(minimumPoolSize, 5, initialPoolSize);

            // Ensure positive intervals
            contractionCheckInterval = Mathf.Max(contractionCheckInterval, 1f);
            idleTimeBeforeContraction = Mathf.Max(idleTimeBeforeContraction, contractionCheckInterval);
            performanceReportInterval = Mathf.Max(performanceReportInterval, 1f);

            // Ensure memory targets are reasonable
            targetMemoryUsageMB = Mathf.Max(targetMemoryUsageMB, 10f);
            memoryWarningThreshold = Mathf.Max(memoryWarningThreshold, 1.1f);
        }

        /// <summary>
        /// Check if configuration is valid
        /// </summary>
        public bool IsValid()
        {
            return initialPoolSize > 0 &&
                   maxPoolSize >= initialPoolSize &&
                   expansionBatchSize > 0 &&
                   contractionCheckInterval > 0 &&
                   idleTimeBeforeContraction > 0 &&
                   performanceReportInterval > 0 &&
                   targetMemoryUsageMB > 0 &&
                   memoryWarningThreshold > 1f &&
                   minimumPoolSize > 0 &&
                   minimumPoolSize <= initialPoolSize;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Create default configuration for low-end devices
        /// </summary>
        public static ObjectPoolConfiguration CreateLowEndConfig()
        {
            var config = CreateInstance<ObjectPoolConfiguration>();
            config.initialPoolSize = 10;
            config.maxPoolSize = 50;
            config.expansionBatchSize = 5;
            config.targetMemoryUsageMB = 25f;
            config.enableAutoContraction = false; // Keep pool stable on low-end devices
            config.contractionCheckInterval = 60f;
            config.performanceReportInterval = 30f;
            config.warmUpStrategy = PoolWarmUpStrategy.Lazy;
            return config;
        }

        /// <summary>
        /// Create default configuration for high-end devices
        /// </summary>
        public static ObjectPoolConfiguration CreateHighEndConfig()
        {
            var config = CreateInstance<ObjectPoolConfiguration>();
            config.initialPoolSize = 50;
            config.maxPoolSize = 200;
            config.expansionBatchSize = 25;
            config.targetMemoryUsageMB = 100f;
            config.enableAutoExpansion = true;
            config.enableAutoContraction = true;
            config.contractionCheckInterval = 15f;
            config.idleTimeBeforeContraction = 30f;
            config.enablePerformanceMonitoring = true;
            config.warmUpStrategy = PoolWarmUpStrategy.PreWarmToHalfMax;
            return config;
        }

        /// <summary>
        /// Get configuration summary string
        /// </summary>
        public string GetConfigurationSummary()
        {
            return $"Pool Config: Size({initialPoolSize}-{maxPoolSize}), " +
                   $"Batch({expansionBatchSize}), " +
                   $"Auto-Exp({enableAutoExpansion}), " +
                   $"Auto-Con({enableAutoContraction}), " +
                   $"Target({targetMemoryUsageMB}MB)";
        }

        #endregion
    }

    /// <summary>
    /// Strategies for pool warm-up on initialization
    /// </summary>
    public enum PoolWarmUpStrategy
    {
        /// <summary>
        /// Don't pre-warm, create coins on demand
        /// </summary>
        Lazy,

        /// <summary>
        /// Pre-warm to initial pool size
        /// </summary>
        PreWarmToInitial,

        /// <summary>
        /// Pre-warm to half of maximum pool size
        /// </summary>
        PreWarmToHalfMax,

        /// <summary>
        /// Pre-warm to full maximum pool size
        /// </summary>
        PreWarmToMax
    }
}