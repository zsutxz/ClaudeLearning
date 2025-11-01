using System;
using UnityEngine;

namespace CoinAnimation.Core
{
    /// <summary>
    /// Configuration settings for coin object pooling
    /// Story 1.3 Task 1 - Object Pool Configuration System
    /// </summary>
    [CreateAssetMenu(fileName = "ObjectPoolConfiguration", menuName = "Coin Animation/Object Pool Configuration")]
    [Serializable]
    public class ObjectPoolConfiguration : ScriptableObject
    {
        #region Pool Configuration

        [Header("Pool Size")]
        [SerializeField] private int _initialPoolSize = 20;
        [SerializeField] private int _maxPoolSize = 100;
        [SerializeField] private int _expansionBatchSize = 10;

        [Header("Pool Management")]
        [SerializeField] private bool _enableAutoExpansion = true;
        [SerializeField] private bool _enableAutoContraction = true;
        [SerializeField] private float _contractionCheckInterval = 30f;
        [SerializeField] private float _idleTimeBeforeContraction = 60f;

        [Header("Performance")]
        [SerializeField] private bool _enablePerformanceMonitoring = true;
        [SerializeField] private float _performanceReportInterval = 10f;

        #endregion

        #region Properties

        public int initialPoolSize
        {
            get => _initialPoolSize;
            set => _initialPoolSize = value;
        }

        public int maxPoolSize
        {
            get => _maxPoolSize;
            set => _maxPoolSize = value;
        }

        public int expansionBatchSize
        {
            get => _expansionBatchSize;
            set => _expansionBatchSize = value;
        }

        public bool enableAutoExpansion
        {
            get => _enableAutoExpansion;
            set => _enableAutoExpansion = value;
        }

        public bool enableAutoContraction
        {
            get => _enableAutoContraction;
            set => _enableAutoContraction = value;
        }

        public float contractionCheckInterval
        {
            get => _contractionCheckInterval;
            set => _contractionCheckInterval = value;
        }

        public float idleTimeBeforeContraction
        {
            get => _idleTimeBeforeContraction;
            set => _idleTimeBeforeContraction = value;
        }

        public bool enablePerformanceMonitoring
        {
            get => _enablePerformanceMonitoring;
            set => _enablePerformanceMonitoring = value;
        }

        public float performanceReportInterval
        {
            get => _performanceReportInterval;
            set => _performanceReportInterval = value;
        }

        // Backward compatibility properties
        public int InitialPoolSize => _initialPoolSize;
        public int MaxPoolSize => _maxPoolSize;
        public int ExpansionBatchSize => _expansionBatchSize;
        public bool EnableAutoExpansion => _enableAutoExpansion;
        public bool EnableAutoContraction => _enableAutoContraction;
        public float ContractionCheckInterval => _contractionCheckInterval;
        public float IdleTimeBeforeContraction => _idleTimeBeforeContraction;
        public bool EnablePerformanceMonitoring => _enablePerformanceMonitoring;
        public float PerformanceReportInterval => _performanceReportInterval;

        #endregion

        #region Public Methods

        /// <summary>
        /// Create a high-end configuration for powerful devices
        /// </summary>
        public static ObjectPoolConfiguration CreateHighEndConfig()
        {
            var config = CreateInstance<ObjectPoolConfiguration>();
            config._initialPoolSize = 50;
            config._maxPoolSize = 200;
            config._expansionBatchSize = 20;
            config._enableAutoExpansion = true;
            config._enableAutoContraction = true;
            config._contractionCheckInterval = 30f;
            config._idleTimeBeforeContraction = 60f;
            config._enablePerformanceMonitoring = true;
            config._performanceReportInterval = 5f;

            return config;
        }

        /// <summary>
        /// Create a mid-range configuration for average devices
        /// </summary>
        public static ObjectPoolConfiguration CreateMidRangeConfig()
        {
            var config = CreateInstance<ObjectPoolConfiguration>();
            config._initialPoolSize = 20;
            config._maxPoolSize = 100;
            config._expansionBatchSize = 10;
            config._enableAutoExpansion = true;
            config._enableAutoContraction = true;
            config._contractionCheckInterval = 45f;
            config._idleTimeBeforeContraction = 90f;
            config._enablePerformanceMonitoring = true;
            config._performanceReportInterval = 10f;

            return config;
        }

        /// <summary>
        /// Create a low-end configuration for less powerful devices
        /// </summary>
        public static ObjectPoolConfiguration CreateLowEndConfig()
        {
            var config = CreateInstance<ObjectPoolConfiguration>();
            config._initialPoolSize = 10;
            config._maxPoolSize = 50;
            config._expansionBatchSize = 5;
            config._enableAutoExpansion = true;
            config._enableAutoContraction = false;
            config._contractionCheckInterval = 60f;
            config._idleTimeBeforeContraction = 120f;
            config._enablePerformanceMonitoring = false;
            config._performanceReportInterval = 15f;

            return config;
        }

        /// <summary>
        /// Validate configuration settings and fix any invalid values
        /// </summary>
        public void ValidateAndFix()
        {
            // Ensure size constraints are valid
            if (_initialPoolSize <= 0)
                _initialPoolSize = 10;

            if (_maxPoolSize <= _initialPoolSize)
                _maxPoolSize = _initialPoolSize * 2;

            if (_expansionBatchSize <= 0)
                _expansionBatchSize = 5;

            // Ensure time intervals are reasonable
            if (_contractionCheckInterval <= 0)
                _contractionCheckInterval = 30f;

            if (_idleTimeBeforeContraction <= 0)
                _idleTimeBeforeContraction = 60f;

            if (_performanceReportInterval <= 0)
                _performanceReportInterval = 10f;

            // Log warnings for significant adjustments
            if (_maxPoolSize > 500)
            {
                Debug.LogWarning($"[ObjectPoolConfiguration] Large max pool size ({_maxPoolSize}) may cause memory issues");
            }

            if (_initialPoolSize > _maxPoolSize)
            {
                Debug.LogWarning($"[ObjectPoolConfiguration] Initial pool size ({_initialPoolSize}) was larger than max pool size. Adjusting to {_maxPoolSize / 2}");
                _initialPoolSize = _maxPoolSize / 2;
            }
        }

        /// <summary>
        /// Check if the current configuration is valid
        /// </summary>
        public bool IsValid()
        {
            return _initialPoolSize > 0 &&
                   _maxPoolSize >= _initialPoolSize &&
                   _expansionBatchSize > 0 &&
                   _contractionCheckInterval > 0 &&
                   _idleTimeBeforeContraction > 0 &&
                   _performanceReportInterval > 0;
        }

        /// <summary>
        /// Get a summary string of the current configuration
        /// </summary>
        public string GetConfigurationSummary()
        {
            return $"Pool Config: Size({initialPoolSize}-{maxPoolSize}), " +
                   $"Batch({expansionBatchSize}), " +
                   $"AutoExp({enableAutoExpansion}), " +
                   $"AutoCon({enableAutoContraction}), " +
                   $"PerfMon({enablePerformanceMonitoring})";
        }

        /// <summary>
        /// Apply configuration to a CoinObjectPool component
        /// </summary>
        public void ApplyToPool(CoinObjectPool pool)
        {
            if (pool == null)
            {
                Debug.LogError("[ObjectPoolConfiguration] Cannot apply configuration to null pool");
                return;
            }

            // Use reflection or direct property setting if available
            // For now, this will be used by the pool to read configuration values
            Debug.Log($"[ObjectPoolConfiguration] Applied configuration to pool: {GetConfigurationSummary()}");
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Clone this configuration
        /// </summary>
        public ObjectPoolConfiguration Clone()
        {
            var clone = CreateInstance<ObjectPoolConfiguration>();
            clone._initialPoolSize = this._initialPoolSize;
            clone._maxPoolSize = this._maxPoolSize;
            clone._expansionBatchSize = this._expansionBatchSize;
            clone._enableAutoExpansion = this._enableAutoExpansion;
            clone._enableAutoContraction = this._enableAutoContraction;
            clone._contractionCheckInterval = this._contractionCheckInterval;
            clone._idleTimeBeforeContraction = this._idleTimeBeforeContraction;
            clone._enablePerformanceMonitoring = this._enablePerformanceMonitoring;
            clone._performanceReportInterval = this._performanceReportInterval;

            return clone;
        }

        /// <summary>
        /// Check if this configuration is suitable for the target device specifications
        /// </summary>
        public bool IsSuitableForDevice(int targetCoins, float availableMemoryMB)
        {
            // Simple heuristic: if we want to handle many coins and have enough memory, use this config
            if (targetCoins > maxPoolSize)
                return false;

            // Estimate memory usage: assume each coin takes ~1MB including animations
            float estimatedMemory = maxPoolSize * 1.0f; // 1MB per coin estimate
            if (estimatedMemory > availableMemoryMB * 0.8f) // Don't use more than 80% of available memory
                return false;

            return true;
        }

        #endregion
    }
}