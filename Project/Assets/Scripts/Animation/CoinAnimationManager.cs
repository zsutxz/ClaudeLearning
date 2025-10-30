using System;
using System.Collections.Generic;
using UnityEngine;
using CoinAnimation.Core;

namespace CoinAnimation.Animation
{
    /// <summary>
    /// 简化的金币动画管理器
    /// </summary>
    public class CoinAnimationManager : MonoBehaviour
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

        #endregion

        #region Fields

        [SerializeField] private int maxConcurrentCoins = 50;

        private readonly Dictionary<int, CoinAnimationController> _activeCoins = new Dictionary<int, CoinAnimationController>();
        private int _coinIdCounter = 0;

        #endregion

        #region Properties

        public int ActiveCoinCount => _activeCoins.Count;
        public bool IsAtCapacity => _activeCoins.Count >= maxConcurrentCoins;

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
        }

        #endregion

        #region Coin Management

        public int RegisterCoin(CoinAnimationController coinController)
        {
            if (coinController == null) return -1;

            if (_activeCoins.Count >= maxConcurrentCoins)
            {
                Debug.LogWarning($"[CoinAnimationManager] 达到最大容量 ({maxConcurrentCoins})");
                return -1;
            }

            int coinId = ++_coinIdCounter;
            _activeCoins.Add(coinId, coinController);

            return coinId;
        }

        public void UnregisterCoin(int coinId)
        {
            if (_activeCoins.ContainsKey(coinId))
            {
                _activeCoins.Remove(coinId);
            }
        }

        #endregion

        #region Event Triggers

        internal void TriggerCollectionComplete(int coinId, Vector3 collectionPoint)
        {
            var args = new CoinCollectionEventArgs(coinId, collectionPoint);
            OnCoinCollectionComplete?.Invoke(this, args);
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion
    }

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