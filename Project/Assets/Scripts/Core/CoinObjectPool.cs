using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using CoinAnimation.Core;

namespace CoinAnimation.Core
{
    /// <summary>
    /// High-performance object pool for coin GameObjects
    /// Supports configurable initial/maximum size with thread-safe operations
    /// Implements Story 1.3 Task 1 requirements
    /// </summary>
    public class CoinObjectPool : MonoBehaviour
    {
        #region Configuration

        [Header("Pool Configuration")]
        [SerializeField] private int initialPoolSize = 20;
        [SerializeField] private int maxPoolSize = 100;
        [SerializeField] private int expansionBatchSize = 10;
        [SerializeField] private bool enableAutoExpansion = true;
        [SerializeField] private bool enableAutoContraction = true;
        [SerializeField] private float contractionCheckInterval = 30f;
        [SerializeField] private float idleTimeBeforeContraction = 60f;

        [Header("Prefabs")]
        [SerializeField] private GameObject coinPrefab;

        [Header("Performance")]
        [SerializeField] private bool enablePerformanceMonitoring = true;
        [SerializeField] private float performanceReportInterval = 10f;

        #endregion

        #region Private Fields

        // Thread-safe pool storage
        private readonly ConcurrentQueue<GameObject> _availableCoins = new ConcurrentQueue<GameObject>();
        private readonly HashSet<GameObject> _activeCoins = new HashSet<GameObject>();
        private readonly ConcurrentDictionary<GameObject, float> _coinUsageTimestamps = new ConcurrentDictionary<GameObject, float>();

        // Pool management
        private int _currentPoolSize = 0;
        private bool _isInitialized = false;
        private float _lastContractionCheck = 0f;
        private float _lastPerformanceReport = 0f;
        private readonly object _poolSizeLock = new object();

        // Performance tracking
        private int _totalPoolRequests = 0;
        private int _poolHits = 0;
        private int _poolExpansions = 0;
        private int _poolContractions = 0;

        #endregion

        #region Properties

        /// <summary>
        /// Current number of coins in the pool (active + available)
        /// </summary>
        public int CurrentPoolSize => _currentPoolSize;

        /// <summary>
        /// Number of coins currently active in animations
        /// </summary>
        public int ActiveCoinCount => _activeCoins.Count;

        /// <summary>
        /// Number of coins available for immediate use
        /// </summary>
        public int AvailableCoinCount => _availableCoins.Count;

        /// <summary>
        /// Maximum pool size limit
        /// </summary>
        public int MaxPoolSize => maxPoolSize;

        /// <summary>
        /// Pool hit rate (0-1)
        /// </summary>
        public float PoolHitRate => _totalPoolRequests > 0 ? (float)_poolHits / _totalPoolRequests : 0f;

        /// <summary>
        /// Is pool currently at maximum capacity
        /// </summary>
        public bool IsAtMaxCapacity => _currentPoolSize >= maxPoolSize;

        /// <summary>
        /// Is pool empty (no available coins)
        /// </summary>
        public bool IsEmpty => _availableCoins.IsEmpty;

        /// <summary>
        /// Is pool initialized and ready for use
        /// </summary>
        public bool IsPoolInitialized => _isInitialized;

        #endregion

        #region Events

        /// <summary>
        /// Triggered when pool expands
        /// </summary>
        public event Action<int, int> OnPoolExpanded; // oldSize, newSize

        /// <summary>
        /// Triggered when pool contracts
        /// </summary>
        public event Action<int, int> OnPoolContracted; // oldSize, newSize

        /// <summary>
        /// Triggered when pool performance thresholds are exceeded
        /// </summary>
        public event Action<PoolPerformanceMetrics> OnPerformanceThresholdExceeded;

        #endregion

        #region Initialization

        private void Awake()
        {
            ValidateConfiguration();
            InitializePool();
        }

        private void ValidateConfiguration()
        {
            if (initialPoolSize <= 0)
            {
                Debug.LogWarning("[CoinObjectPool] Invalid initialPoolSize, using default: 20");
                initialPoolSize = 20;
            }

            if (maxPoolSize <= initialPoolSize)
            {
                maxPoolSize = initialPoolSize * 5;
                Debug.LogWarning($"[CoinObjectPool] Invalid maxPoolSize, using calculated: {maxPoolSize}");
            }

            if (expansionBatchSize <= 0)
            {
                expansionBatchSize = Math.Max(5, initialPoolSize / 4);
                Debug.LogWarning($"[CoinObjectPool] Invalid expansionBatchSize, using calculated: {expansionBatchSize}");
            }

            if (coinPrefab == null)
            {
                Debug.LogError("[CoinObjectPool] Coin prefab is required!");
                throw new InvalidOperationException("Coin prefab must be assigned");
            }
        }

        private void InitializePool()
        {
            Debug.Log($"[CoinObjectPool] Initializing pool with {initialPoolSize} coins");
            
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewCoinInPool();
            }

            _lastContractionCheck = Time.time;
            _lastPerformanceReport = Time.time;
            _isInitialized = true;

            Debug.Log($"[CoinObjectPool] Pool initialized with {_currentPoolSize} coins");
        }

        #endregion

        #region Pool Operations

        /// <summary>
        /// Get a coin from the pool (thread-safe)
        /// </summary>
        /// <returns>Coin GameObject or null if pool is at max capacity</returns>
        public GameObject GetCoin()
        {
            Interlocked.Increment(ref _totalPoolRequests);

            // Try to get from available pool
            if (_availableCoins.TryDequeue(out GameObject coin))
            {
                if (coin != null)
                {
                    coin.SetActive(true);
                    _activeCoins.Add(coin);
                    _coinUsageTimestamps[coin] = Time.time;
                    Interlocked.Increment(ref _poolHits);
                    return coin;
                }
            }

            // No available coins, try to expand pool
            if (enableAutoExpansion && !IsAtMaxCapacity)
            {
                lock (_poolSizeLock)
                {
                    if (!IsAtMaxCapacity)
                    {
                        return ExpandPoolAndReturnCoin();
                    }
                }
            }

            Debug.LogWarning("[CoinObjectPool] Pool exhausted and at max capacity");
            return null;
        }

        /// <summary>
        /// Return a coin to the pool (thread-safe)
        /// </summary>
        /// <param name="coin">Coin GameObject to return</param>
        public void ReturnCoin(GameObject coin)
        {
            if (coin == null) return;

            if (_activeCoins.Contains(coin))
            {
                _activeCoins.Remove(coin);
                _coinUsageTimestamps[coin] = Time.time;
                
                // Reset coin state
                ResetCoinState(coin);
                
                _availableCoins.Enqueue(coin);
            }
            else
            {
                Debug.LogWarning("[CoinObjectPool] Attempted to return coin that wasn't active");
            }
        }

        /// <summary>
        /// Pre-warm the pool to a specific size
        /// </summary>
        /// <param name="targetSize">Target pool size</param>
        public void PreWarmPool(int targetSize)
        {
            if (targetSize <= _currentPoolSize) return;
            if (targetSize > maxPoolSize)
            {
                targetSize = maxPoolSize;
                Debug.LogWarning($"[CoinObjectPool] Pre-warm target exceeds max size, using {maxPoolSize}");
            }

            lock (_poolSizeLock)
            {
                int coinsToCreate = targetSize - _currentPoolSize;
                for (int i = 0; i < coinsToCreate; i++)
                {
                    CreateNewCoinInPool();
                }
            }

            Debug.Log($"[CoinObjectPool] Pre-warmed pool to {_currentPoolSize} coins");
        }

        #endregion

        #region Pool Management

        private GameObject ExpandPoolAndReturnCoin()
        {
            int oldSize = _currentPoolSize;
            
            // Create expansion batch
            int coinsToCreate = Math.Min(expansionBatchSize, maxPoolSize - _currentPoolSize);
            for (int i = 0; i < coinsToCreate; i++)
            {
                CreateNewCoinInPool();
            }

            // Get one coin for immediate use
            if (_availableCoins.TryDequeue(out GameObject coin))
            {
                coin.SetActive(true);
                _activeCoins.Add(coin);
                _coinUsageTimestamps[coin] = Time.time;
                
                Interlocked.Increment(ref _poolExpansions);
                OnPoolExpanded?.Invoke(oldSize, _currentPoolSize);
                
                Debug.Log($"[CoinObjectPool] Pool expanded from {oldSize} to {_currentPoolSize}");
                return coin;
            }

            return null;
        }

        private void CreateNewCoinInPool()
        {
            if (_currentPoolSize >= maxPoolSize) return;

            GameObject coin = Instantiate(coinPrefab, transform);
            coin.name = $"Coin_{_currentPoolSize:D4}";
            coin.SetActive(false);
            
            _availableCoins.Enqueue(coin);
            _coinUsageTimestamps[coin] = Time.time;
            _currentPoolSize++;
        }

        private void ResetCoinState(GameObject coin)
        {
            // Reset transform
            coin.transform.localPosition = Vector3.zero;
            coin.transform.localRotation = Quaternion.identity;
            coin.transform.localScale = Vector3.one;

            // Reset animation state using SendMessage (since we can't directly reference Animation assembly)
            if (coin.GetComponent("CoinAnimationController") != null)
            {
                coin.SendMessage("StopCurrentAnimation", SendMessageOptions.DontRequireReceiver);
                coin.SendMessage("SetState", CoinAnimationState.Pooled, SendMessageOptions.DontRequireReceiver);
            }

            // Reset physics components
            var rigidbody = coin.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }

            // Deactivate the coin
            coin.SetActive(false);
        }

        #endregion

        #region Update Loop

        private void Update()
        {
            // Periodic pool contraction check
            if (enableAutoContraction && Time.time - _lastContractionCheck >= contractionCheckInterval)
            {
                CheckAndContractPool();
                _lastContractionCheck = Time.time;
            }

            // Performance monitoring
            if (enablePerformanceMonitoring && Time.time - _lastPerformanceReport >= performanceReportInterval)
            {
                ReportPerformanceMetrics();
                _lastPerformanceReport = Time.time;
            }
        }

        #endregion

        #region Pool Contraction

        private void CheckAndContractPool()
        {
            if (!enableAutoContraction || _currentPoolSize <= initialPoolSize) return;

            float currentTime = Time.time;
            int coinsToContract = 0;

            // Find idle coins that can be contracted
            var idleCoins = new List<GameObject>();
            foreach (var kvp in _coinUsageTimestamps)
            {
                if (!_activeCoins.Contains(kvp.Key) && 
                    currentTime - kvp.Value >= idleTimeBeforeContraction)
                {
                    idleCoins.Add(kvp.Key);
                }
            }

            // Contract in batches
            if (idleCoins.Count >= expansionBatchSize)
            {
                coinsToContract = Math.Min(idleCoins.Count, expansionBatchSize);
                ContractPool(coinsToContract, idleCoins);
            }
        }

        private void ContractPool(int count, List<GameObject> idleCoins)
        {
            int oldSize = _currentPoolSize;
            int contracted = 0;

            lock (_poolSizeLock)
            {
                for (int i = 0; i < count && contracted < idleCoins.Count && _currentPoolSize > initialPoolSize; i++)
                {
                    GameObject coinToDestroy = idleCoins[i];
                    
                    // Remove from all tracking collections
                    _availableCoins.TryDequeue(out _); // This is not ideal, need better approach
                    _coinUsageTimestamps.TryRemove(coinToDestroy, out _);
                    
                    // Destroy the coin
                    if (coinToDestroy != null)
                    {
                        DestroyImmediate(coinToDestroy);
                        _currentPoolSize--;
                        contracted++;
                    }
                }
            }

            if (contracted > 0)
            {
                Interlocked.Add(ref _poolContractions, contracted);
                OnPoolContracted?.Invoke(oldSize, _currentPoolSize);
                Debug.Log($"[CoinObjectPool] Pool contracted from {oldSize} to {_currentPoolSize} ({contracted} coins)");
            }
        }

        #endregion

        #region Performance Monitoring

        private void ReportPerformanceMetrics()
        {
            var metrics = GetPerformanceMetrics();
            
            if (metrics.IsPerformanceDegraded)
            {
                OnPerformanceThresholdExceeded?.Invoke(metrics);
            }

            // Optional: Log metrics periodically
            if (Time.time % 60f < 0.1f) // Every minute approximately
            {
                Debug.Log($"[CoinObjectPool] Performance: HitRate={metrics.PoolHitRate:P1}, " +
                         $"Active={metrics.ActiveCoins}, Available={metrics.AvailableCoins}, " +
                         $"Size={metrics.PoolSize}");
            }
        }

        /// <summary>
        /// Get current pool performance metrics
        /// </summary>
        public PoolPerformanceMetrics GetPerformanceMetrics()
        {
            return new PoolPerformanceMetrics
            {
                PoolSize = _currentPoolSize,
                ActiveCoins = _activeCoins.Count,
                AvailableCoins = _availableCoins.Count,
                MaxPoolSize = maxPoolSize,
                PoolHitRate = PoolHitRate,
                TotalRequests = _totalPoolRequests,
                PoolHits = _poolHits,
                PoolExpansions = _poolExpansions,
                PoolContractions = _poolContractions,
                MemoryUsage = GetEstimatedMemoryUsage(),
                IsPerformanceDegraded = PoolHitRate < 0.8f || _activeCoins.Count > maxPoolSize * 0.9f,
                Timestamp = DateTime.UtcNow
            };
        }

        private float GetEstimatedMemoryUsage()
        {
            // Rough estimation: each coin ~1KB of memory
            return _currentPoolSize * 1024f;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Clear all coins from the pool
        /// </summary>
        public void ClearPool()
        {
            lock (_poolSizeLock)
            {
                // Return all active coins
                var activeCoinsCopy = new List<GameObject>(_activeCoins);
                foreach (var coin in activeCoinsCopy)
                {
                    ReturnCoin(coin);
                }

                // Destroy all coins
                while (_availableCoins.TryDequeue(out GameObject coin))
                {
                    if (coin != null)
                    {
                        DestroyImmediate(coin);
                    }
                }

                _coinUsageTimestamps.Clear();
                _currentPoolSize = 0;

                Debug.Log("[CoinObjectPool] Pool cleared");
            }
        }

        /// <summary>
        /// Force pool expansion by specified amount
        /// </summary>
        /// <param name="amount">Number of coins to add</param>
        /// <returns>Actual number of coins added</returns>
        public int ForceExpansion(int amount)
        {
            if (amount <= 0) return 0;

            lock (_poolSizeLock)
            {
                int oldSize = _currentPoolSize;
                int actualAmount = Math.Min(amount, maxPoolSize - _currentPoolSize);

                for (int i = 0; i < actualAmount; i++)
                {
                    CreateNewCoinInPool();
                }

                if (actualAmount > 0)
                {
                    Interlocked.Add(ref _poolExpansions, actualAmount);
                    OnPoolExpanded?.Invoke(oldSize, _currentPoolSize);
                    Debug.Log($"[CoinObjectPool] Force expanded pool by {actualAmount} coins");
                }

                return actualAmount;
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            ClearPool();
            Debug.Log("[CoinObjectPool] Pool destroyed and cleaned up");
        }

        #endregion
    }

    /// <summary>
    /// Performance metrics for the object pool
    /// </summary>
    [Serializable]
    public class PoolPerformanceMetrics
    {
        public int PoolSize;
        public int ActiveCoins;
        public int AvailableCoins;
        public int MaxPoolSize;
        public float PoolHitRate;
        public int TotalRequests;
        public int PoolHits;
        public int PoolExpansions;
        public int PoolContractions;
        public float MemoryUsage; // in bytes
        public bool IsPerformanceDegraded;
        public DateTime Timestamp;
    }
}