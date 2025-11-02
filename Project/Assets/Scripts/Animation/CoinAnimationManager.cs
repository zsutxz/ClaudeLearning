using System;
using System.Collections.Generic;
using UnityEngine;
using CoinAnimation.Core;

namespace CoinAnimation.Animation
{
    /// <summary>
    /// Enhanced coin animation manager with object pooling integration
    /// Story 1.3 - Object Pooling and Memory Management Integration
    /// </summary>
    public class CoinAnimationManager : MonoBehaviour, ICoinAnimationManager
    {
        #region Singleton

        private static CoinAnimationManager _instance;
        public static CoinAnimationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CoinAnimationManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CoinAnimationManager");
                        _instance = go.AddComponent<CoinAnimationManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Events

        public static event EventHandler<CoinAnimationEventArgs> OnCoinStateChanged;
        public static event EventHandler<CoinCollectionEventArgs> OnCoinCollectionComplete;

        // ICoinAnimationManager events
        public event Action<Guid> OnAnimationStarted;
        public event Action<Guid> OnAnimationCompleted;
        public event Action<PerformanceMetrics> OnPerformanceThresholdExceeded;

        #endregion

        #region Fields

        [Header("Pool Configuration")]
        [SerializeField] private ObjectPoolConfiguration poolConfiguration;
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private bool useObjectPooling = true;

        [Header("Legacy Settings")]
        [SerializeField] private int maxConcurrentCoins = 50;

        // Pool management
        private CoinObjectPool _objectPool;
        private readonly Dictionary<int, GameObject> _activeCoinObjects = new Dictionary<int, GameObject>();
        private readonly Dictionary<Guid, List<int>> _animationSessions = new Dictionary<Guid, List<int>>();
        private int _coinIdCounter = 0;

        // Performance tracking
        private PerformanceMetrics _currentMetrics;
        private float _lastMetricsUpdate = 0f;
        private float _metricsUpdateInterval = 1f;

        // ICoinAnimationManager compatibility
        private readonly Dictionary<Guid, CoinAnimationSession> _activeSessions = new Dictionary<Guid, CoinAnimationSession>();

        #endregion

        #region Properties

        public int ActiveCoinCount => _activeCoinObjects.Count;
        public bool IsAtCapacity => _activeCoinObjects.Count >= maxConcurrentCoins;

        // Pool properties
        public PoolPerformanceMetrics PoolMetrics => _objectPool?.GetPerformanceMetrics() ?? new PoolPerformanceMetrics();
        public bool IsPoolInitialized => _objectPool != null;

        #endregion

        #region Initialization

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeSystem();
        }

        private void InitializeSystem()
        {
            // Initialize pool configuration if not assigned
            if (poolConfiguration == null)
            {
                poolConfiguration = ObjectPoolConfiguration.CreateHighEndConfig();
                Debug.Log("[CoinAnimationManager] Using default high-end pool configuration");
            }
            
            poolConfiguration.ValidateAndFix();
            
            // Initialize object pool if enabled
            if (useObjectPooling)
            {
                InitializeObjectPool();
            }
            
            // Initialize performance metrics
            _currentMetrics = new PerformanceMetrics
            {
                timestamp = DateTime.UtcNow,
                currentFrameRate = 60f,
                activeAnimations = 0,
                memoryUsage = 0f,
                cpuUsage = 0f,
                isPerformanceOptimal = true
            };
            
            Debug.Log($"[CoinAnimationManager] System initialized with pooling: {useObjectPooling}");
        }

        private void InitializeObjectPool()
        {
            // Try to find coin prefab if not assigned
            if (coinPrefab == null)
            {
                coinPrefab = FindDefaultCoinPrefab();
                if (coinPrefab == null)
                {
                    Debug.LogWarning("[CoinAnimationManager] Coin prefab not found, object pooling disabled. Please assign a coin prefab in the inspector.");
                    useObjectPooling = false;
                    return;
                }
                else
                {
                    Debug.Log($"[CoinAnimationManager] Auto-detected coin prefab: {coinPrefab.name}");
                }
            }
            
            GameObject poolObject = new GameObject("CoinObjectPool");
            poolObject.transform.SetParent(transform);
            _objectPool = poolObject.AddComponent<CoinObjectPool>();

            // Configure the pool with the detected prefab
            _objectPool.SetCoinPrefab(coinPrefab);
            _objectPool.enabled = true;

            Debug.Log($"[CoinAnimationManager] Object pool initialized with config: {poolConfiguration.GetConfigurationSummary()}");
            Debug.Log($"[CoinAnimationManager] Pool configured with prefab: {coinPrefab.name}");
        }

        #endregion

        #region Coin Management

        /// <summary>
        /// Legacy registration method for backward compatibility
        /// </summary>
        [Obsolete("Use GetCoinFromPool instead for pooled coin management")]
        public int RegisterCoin(MonoBehaviour coinController)
        {
            if (coinController == null) return -1;

            if (_activeCoinObjects.Count >= maxConcurrentCoins)
            {
                Debug.LogWarning($"[CoinAnimationManager] 达到最大容量 ({maxConcurrentCoins})");
                return -1;
            }

            int coinId = ++_coinIdCounter;
            var coinObject = coinController.gameObject;
            _activeCoinObjects.Add(coinId, coinObject);

            return coinId;
        }

        /// <summary>
        /// Get a coin from the object pool (new pooling method)
        /// </summary>
        /// <returns>Coin GameObject or null if pool is unavailable</returns>
        public GameObject GetCoinFromPool()
        {
            if (!useObjectPooling || _objectPool == null)
            {
                Debug.LogWarning("[CoinAnimationManager] Object pooling is not enabled or initialized");
                return null;
            }

            GameObject coin = _objectPool.GetCoin();
            if (coin != null)
            {
                int coinId = ++_coinIdCounter;
                _activeCoinObjects.Add(coinId, coin);
                
                // Set up the coin controller (support both UGUI and 3D controllers)
                var controller = GetCoinAnimationController(coin);
                if (controller != null)
                {
                    // The controller will automatically register itself in Start()
                    // but we need to ensure it has the correct coin ID
                    var controllerType = controller.GetType();
                    var idField = controllerType.GetField("_coinId",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (idField != null)
                    {
                        idField.SetValue(controller, coinId);
                    }
                }
                
                return coin;
            }

            return null;
        }

        /// <summary>
        /// Return a coin to the object pool
        /// </summary>
        /// <param name="coinId">ID of the coin to return</param>
        public void ReturnCoinToPool(int coinId)
        {
            if (_activeCoinObjects.ContainsKey(coinId))
            {
                GameObject coin = _activeCoinObjects[coinId];
                _activeCoinObjects.Remove(coinId);
                
                if (useObjectPooling && _objectPool != null && coin != null)
                {
                    _objectPool.ReturnCoin(coin);
                }
            }
        }

        /// <summary>
        /// Return a coin GameObject to the pool
        /// </summary>
        /// <param name="coin">Coin GameObject to return</param>
        public void ReturnCoinToPool(GameObject coin)
        {
            if (coin == null) return;
            
            // Find the coin ID and remove from active tracking
            int coinIdToRemove = -1;
            foreach (var kvp in _activeCoinObjects)
            {
                if (kvp.Value == coin)
                {
                    coinIdToRemove = kvp.Key;
                    break;
                }
            }
            
            if (coinIdToRemove != -1)
            {
                _activeCoinObjects.Remove(coinIdToRemove);
            }
            
            if (useObjectPooling && _objectPool != null)
            {
                _objectPool.ReturnCoin(coin);
            }
        }

        /// <summary>
        /// Legacy unregistration method for backward compatibility
        /// </summary>
        [Obsolete("Use ReturnCoinToPool instead for pooled coin management")]
        public void UnregisterCoin(int coinId)
        {
            if (_activeCoinObjects.ContainsKey(coinId))
            {
                _activeCoinObjects.Remove(coinId);
            }
        }

        #endregion

        #region Event Triggers

        internal void TriggerCollectionComplete(int coinId, Vector2 collectionPoint)
        {
            var args = new CoinCollectionEventArgs(coinId, collectionPoint);
            OnCoinCollectionComplete?.Invoke(this, args);
            
            // Automatically return coin to pool when collection is complete
            if (useObjectPooling)
            {
                ReturnCoinToPool(coinId);
            }
        }

        #endregion

        #region ICoinAnimationManager Implementation

        public void Initialize(CoinAnimationConfiguration configuration)
        {
            // Apply configuration if provided
            if (configuration != null)
            {
                maxConcurrentCoins = configuration.maxConcurrentAnimations;
                // Apply other configuration settings as needed
            }
            
            Debug.Log("[CoinAnimationManager] ICoinAnimationManager initialized");
        }

        public Guid StartCoinAnimation(Transform target, int coinCount = 1)
        {
            Guid sessionId = Guid.NewGuid();
            var session = new CoinAnimationSession
            {
                SessionId = sessionId,
                Target = target,
                CoinCount = coinCount,
                StartTime = DateTime.UtcNow,
                CoinIds = new List<int>()
            };
            
            _activeSessions[sessionId] = session;
            
            // Spawn coins for this animation session
            for (int i = 0; i < coinCount; i++)
            {
                if (useObjectPooling)
                {
                    GameObject coin = GetCoinFromPool();
                    if (coin != null)
                    {
                        // Position coin at target and start animation
                        coin.transform.position = target.position;

                        var controller = GetCoinAnimationController(coin);
                        if (controller != null)
                        {
                            // Try to call CollectCoin using reflection
                            var collectMethod = controller.GetType().GetMethod("CollectCoin", new[] { typeof(Vector3), typeof(float) });
                            collectMethod?.Invoke(controller, new object[] { target.position, 2f });
                        }
                    }
                }
            }
            
            OnAnimationStarted?.Invoke(sessionId);
            
            // Auto-complete session after delay (simplified)
            StartCoroutine(CompleteAnimationSession(sessionId, 3f));
            
            return sessionId;
        }

        public void StopCoinAnimation(Guid sessionId)
        {
            if (_activeSessions.ContainsKey(sessionId))
            {
                // Stop all coins in this session
                var session = _activeSessions[sessionId];
                foreach (int coinId in session.CoinIds)
                {
                    if (_activeCoinObjects.ContainsKey(coinId))
                    {
                        GameObject coin = _activeCoinObjects[coinId];
                        var controller = GetCoinAnimationController(coin);

                        // Try to call StopCurrentAnimation using reflection
                        if (controller != null)
                        {
                            var stopMethod = controller.GetType().GetMethod("StopCurrentAnimation");
                            stopMethod?.Invoke(controller, null);
                        }
                        
                        // Return coin to pool
                        ReturnCoinToPool(coinId);
                    }
                }
                
                _activeSessions.Remove(sessionId);
                OnAnimationCompleted?.Invoke(sessionId);
            }
        }

        public PerformanceMetrics GetPerformanceMetrics()
        {
            UpdatePerformanceMetrics();
            return _currentMetrics;
        }

        public void Cleanup()
        {
            // Return all active coins to pool
            var activeCoinsCopy = new List<int>(_activeCoinObjects.Keys);
            foreach (int coinId in activeCoinsCopy)
            {
                ReturnCoinToPool(coinId);
            }
            
            // Clear active sessions
            _activeSessions.Clear();
            
            Debug.Log("[CoinAnimationManager] System cleanup completed");
        }

        #endregion

        #region Prefab Detection

        private GameObject FindDefaultCoinPrefab()
        {
            // Try common prefab paths
            string[] prefabPaths = {
                "Assets/Res/Prefabs/UI/UGUICoin.prefab",
                "Assets/Prefabs/Coin.prefab",
                "Assets/Res/Prefabs/Coin.prefab"
            };

            foreach (string path in prefabPaths)
            {
                GameObject prefab = Resources.Load<GameObject>(path.Replace("Assets/", "").Replace(".prefab", ""));
                if (prefab != null)
                {
                    return prefab;
                }
            }

            // Try to find in project using AssetDatabase (editor only)
            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab UGUICoin");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            // Fallback: try any prefab with coin animation controller
            guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && HasCoinAnimationController(prefab))
                {
                    Debug.Log($"[CoinAnimationManager] Found coin controller in prefab: {path}");
                    return prefab;
                }
            }
            #endif

            return null;
        }

        private bool HasCoinAnimationController(GameObject prefab)
        {
            if (prefab == null) return false;

            // Check for any coin animation controller type
            var controllers = prefab.GetComponentsInChildren<MonoBehaviour>();
            foreach (var controller in controllers)
            {
                if (controller != null)
                {
                    string typeName = controller.GetType().Name;
                    if (typeName.Contains("CoinAnimation") && typeName.Contains("Controller"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private MonoBehaviour GetCoinAnimationController(GameObject coin)
        {
            if (coin == null) return null;

            // Try UGUI coin animation controller first
            var uguiController = coin.GetComponent<UGUICoinAnimationController>();
            if (uguiController != null) return uguiController;

            // Try standard coin animation controller
            var standardController = coin.GetComponent<CoinAnimationController>();
            if (standardController != null) return standardController;

            // Fallback: try any coin animation controller type
            var controllers = coin.GetComponentsInChildren<MonoBehaviour>();
            foreach (var controller in controllers)
            {
                if (controller != null)
                {
                    string typeName = controller.GetType().Name;
                    if (typeName.Contains("CoinAnimation") && typeName.Contains("Controller"))
                    {
                        return controller;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Private Methods

        private System.Collections.IEnumerator CompleteAnimationSession(Guid sessionId, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (_activeSessions.ContainsKey(sessionId))
            {
                _activeSessions.Remove(sessionId);
                OnAnimationCompleted?.Invoke(sessionId);
            }
        }

        private void UpdatePerformanceMetrics()
        {
            if (Time.time - _lastMetricsUpdate < _metricsUpdateInterval) return;
            
            _currentMetrics.timestamp = DateTime.UtcNow;
            _currentMetrics.activeAnimations = _activeCoinObjects.Count;
            _currentMetrics.currentFrameRate = 1f / Time.deltaTime;
            
            // Estimate memory usage
            if (useObjectPooling && _objectPool != null)
            {
                var poolMetrics = _objectPool.GetPerformanceMetrics();
                _currentMetrics.memoryUsage = poolMetrics.MemoryUsage / (1024f * 1024f); // Convert to MB
            }
            
            // Determine if performance is optimal
            _currentMetrics.isPerformanceOptimal = 
                _currentMetrics.currentFrameRate >= 30f && 
                _currentMetrics.memoryUsage < 100f && // Less than 100MB
                _activeCoinObjects.Count < maxConcurrentCoins * 0.9f;
            
            // Trigger performance warning if needed
            if (!_currentMetrics.isPerformanceOptimal)
            {
                OnPerformanceThresholdExceeded?.Invoke(_currentMetrics);
            }
            
            _lastMetricsUpdate = Time.time;
        }

        #endregion

        #region Update Loop

        private void Update()
        {
            UpdatePerformanceMetrics();
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (_instance == this)
            {
                Cleanup();
                _instance = null;
            }
        }

        #endregion
    }

    #region Supporting Classes

    public class CoinCollectionEventArgs : EventArgs
    {
        public int CoinId { get; }
        public Vector2 CollectionPoint { get; }

        public CoinCollectionEventArgs(int coinId, Vector2 collectionPoint)
        {
            CoinId = coinId;
            CollectionPoint = collectionPoint;
        }
    }

    /// <summary>
    /// Animation session data for ICoinAnimationManager compatibility
    /// </summary>
    [Serializable]
    public class CoinAnimationSession
    {
        public Guid SessionId;
        public Transform Target;
        public int CoinCount;
        public DateTime StartTime;
        public List<int> CoinIds;
        public bool IsCompleted;
    }

    #endregion
}