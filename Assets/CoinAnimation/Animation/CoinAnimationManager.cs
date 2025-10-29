using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using CoinAnimation.Core;

namespace CoinAnimation.Animation
{
    /// <summary>
    /// Singleton manager for all coin animations
    /// Handles state management, performance optimization, and event coordination
    /// </summary>
    public class CoinAnimationManager : MonoBehaviour
    {
        #region Singleton Pattern
        
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
        
        /// <summary>
        /// Fired when any coin changes animation state
        /// </summary>
        public static event EventHandler<CoinAnimationEventArgs> OnCoinStateChanged;
        
        /// <summary>
        /// Fired when coin collection sequence completes
        /// </summary>
        public static event EventHandler<CoinCollectionEventArgs> OnCoinCollectionComplete;

        #endregion

        #region Configuration
        
        [Header("Performance Settings")]
        [SerializeField, Tooltip("Maximum concurrent animated coins")]
        private int maxConcurrentCoins = 100;
        
        [SerializeField, Tooltip("Target framerate for animation optimization")]
        private int targetFrameRate = 60;
        
        [SerializeField, Tooltip("Enable performance profiling")]
        private bool enableProfiling = false;

        [Header("DOTween Settings")]
        [SerializeField, Tooltip("Global DOTween time scale")]
        private float globalTimeScale = 1f;
        
        [SerializeField, Tooltip("DOTween capacity for tweens")]
        private int tweenCapacity = 200;

        #endregion

        #region Private Fields
        
        private readonly Dictionary<int, CoinAnimationController> _activeCoins = new Dictionary<int, CoinAnimationController>();
        private readonly Queue<CoinAnimationController> _coinPool = new Queue<CoinAnimationController>();
        private readonly List<Tween> _activeTweens = new List<Tween>();
        
        private int _coinIdCounter = 0;
        private bool _isInitialized = false;

        #endregion

        #region Properties
        
        /// <summary>
        /// Number of currently active animated coins
        /// </summary>
        public int ActiveCoinCount => _activeCoins.Count;
        
        /// <summary>
        /// Current performance status
        /// </summary>
        public bool IsAtCapacity => _activeCoins.Count >= maxConcurrentCoins;
        
        /// <summary>
        /// Global time scale for all coin animations
        /// </summary>
        public float GlobalTimeScale
        {
            get => globalTimeScale;
            set
            {
                globalTimeScale = Mathf.Clamp01(value);
                DOTween.globalTimeScale = globalTimeScale;
            }
        }

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
            
            InitializeDOTween();
        }

        /// <summary>
        /// Initialize DOTween settings and optimize for performance
        /// </summary>
        private void InitializeDOTween()
        {
            if (_isInitialized) return;
            
            // Configure DOTween for optimal performance
            DOTween.SetTweensCapacity(tweenCapacity, 50);
            DOTween.logBehaviour = LogBehaviour.ErrorsOnly;
            DOTween.defaultAutoPlay = AutoPlay.All;
            DOTween.globalTimeScale = globalTimeScale;
            
            // Set target framerate
            Application.targetFrameRate = targetFrameRate;
            
            _isInitialized = true;
            
            if (enableProfiling)
            {
                Debug.Log($"[CoinAnimationManager] Initialized - Capacity: {tweenCapacity}, Target FPS: {targetFrameRate}");
            }
        }

        #endregion

        #region Coin Management
        
        /// <summary>
        /// Register a new coin for animation management
        /// </summary>
        /// <param name="coinController">Coin controller to register</param>
        /// <returns>Unique ID for the registered coin</returns>
        public int RegisterCoin(CoinAnimationController coinController)
        {
            if (coinController == null)
            {
                Debug.LogError("[CoinAnimationManager] Cannot register null coin controller");
                return -1;
            }
            
            int coinId = ++_coinIdCounter;
            
            if (_activeCoins.Count >= maxConcurrentCoins)
            {
                Debug.LogWarning($"[CoinAnimationManager] Capacity reached ({maxConcurrentCoins}). Pooling oldest coin.");
                PoolOldestCoin();
            }
            
            _activeCoins.Add(coinId, coinController);
            
            if (enableProfiling)
            {
                Debug.Log($"[CoinAnimationManager] Registered coin {coinId}. Active: {_activeCoins.Count}/{maxConcurrentCoins}");
            }
            
            return coinId;
        }

        /// <summary>
        /// Unregister a coin from animation management
        /// </summary>
        /// <param name="coinId">ID of coin to unregister</param>
        public void UnregisterCoin(int coinId)
        {
            if (_activeCoins.ContainsKey(coinId))
            {
                var controller = _activeCoins[coinId];
                _activeCoins.Remove(coinId);
                
                // Kill any active tweens for this coin
                controller.KillAllTweens();
                
                if (enableProfiling)
                {
                    Debug.Log($"[CoinAnimationManager] Unregistered coin {coinId}. Active: {_activeCoins.Count}");
                }
            }
        }

        /// <summary>
        /// Move oldest coin to pool for performance
        /// </summary>
        private void PoolOldestCoin()
        {
            if (_activeCoins.Count == 0) return;
            
            var enumerator = _activeCoins.GetEnumerator();
            if (enumerator.MoveNext())
            {
                int oldestId = enumerator.Current.Key;
                var oldestController = enumerator.Current.Value;
                
                _activeCoins.Remove(oldestId);
                _coinPool.Enqueue(oldestController);
                
                // Trigger state change to pooled
                TriggerStateChange(oldestController, CoinAnimationState.Pooled);
            }
        }

        #endregion

        #region Animation Control
        
        /// <summary>
        /// Start animation sequence for a coin
        /// </summary>
        /// <param name="coinId">Coin to animate</param>
        /// <param name="targetPosition">Target position</param>
        /// <param name="duration">Animation duration</param>
        /// <param name="context">Animation context for easing selection</param>
        public void AnimateCoinToPosition(int coinId, Vector3 targetPosition, float duration, EasingContext context = EasingContext.Normal)
        {
            if (!_activeCoins.ContainsKey(coinId))
            {
                Debug.LogError($"[CoinAnimationManager] Coin {coinId} not found in active coins");
                return;
            }
            
            var coin = _activeCoins[coinId];
            var easing = CoinAnimationEasing.GetContextualEasing(
                Vector3.Distance(coin.transform.position, targetPosition), 
                context
            );
            
            coin.AnimateToPosition(targetPosition, duration, easing);
        }

        #endregion

        #region Event System
        
        /// <summary>
        /// Trigger coin state change event
        /// </summary>
        /// <param name="controller">Coin controller</param>
        /// <param name="newState">New animation state</param>
        internal void TriggerStateChange(CoinAnimationController controller, CoinAnimationState newState)
        {
            var previousState = controller.CurrentState;
            controller.SetState(newState);
            
            var args = new CoinAnimationEventArgs(
                previousState, 
                newState, 
                0f, // Duration not needed for state changes
                true
            );
            
            OnCoinStateChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Trigger coin collection complete event
        /// </summary>
        /// <param name="coinId">Collected coin ID</param>
        /// <param name="collectionPoint">Collection point position</param>
        internal void TriggerCollectionComplete(int coinId, Vector3 collectionPoint)
        {
            var args = new CoinCollectionEventArgs(coinId, collectionPoint);
            OnCoinCollectionComplete?.Invoke(this, args);
        }

        #endregion

        #region Performance Management
        
        /// <summary>
        /// Optimize performance by cleaning up completed tweens
        /// </summary>
        public void OptimizePerformance()
        {
            // Remove completed tweens
            for (int i = _activeTweens.Count - 1; i >= 0; i--)
            {
                if (!_activeTweens[i].active)
                {
                    _activeTweens.RemoveAt(i);
                }
            }
            
            // DOTween automatically cleans up, but we can help
            DOTween.CompleteAll(false);
        }

        /// <summary>
        /// Clear all active animations (useful for scene transitions)
        /// </summary>
        public void ClearAllAnimations()
        {
            foreach (var coin in _activeCoins.Values)
            {
                coin.KillAllTweens();
            }
            
            DOTween.KillAll(false);
            _activeTweens.Clear();
        }

        #endregion

        #region Cleanup
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                ClearAllAnimations();
                _instance = null;
            }
        }

        #endregion
    }

    /// <summary>
    /// Event arguments for coin collection completion
    /// </summary>
    public class CoinCollectionEventArgs : EventArgs
    {
        public int CoinId { get; }
        public Vector3 CollectionPoint { get; }

        public CoinCollectionEventArgs(int coinId, Vector3 collectionPoint)
        {
            CoinId = coinId;
            CollectionPoint = collectionPoint;
        }
    }
}