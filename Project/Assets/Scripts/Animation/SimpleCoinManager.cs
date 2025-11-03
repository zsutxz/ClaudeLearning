using UnityEngine;
using System.Collections.Generic;

namespace CoinAnimation.Animation
{
    /// <summary>
    /// 简单的金币管理器
    /// 只负责创建和管理金币
    /// </summary>
    public class SimpleCoinManager : MonoBehaviour
    {
        [Header("金币设置")]
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private int maxCoins = 50;
        [SerializeField] private Transform coinParent;

        private Queue<GameObject> _coinPool = new Queue<GameObject>();
        private List<GameObject> _activeCoins = new List<GameObject>();

        private void Awake()
        {
            InitializePool();
        }

        /// <summary>
        /// 初始化金币池
        /// </summary>
        private void InitializePool()
        {
            for (int i = 0; i < maxCoins; i++)
            {
                GameObject coin = Instantiate(coinPrefab, coinParent);
                coin.SetActive(false);
                _coinPool.Enqueue(coin);
            }
        }

        /// <summary>
        /// 获取金币
        /// </summary>
        public GameObject GetCoin()
        {
            GameObject coin = null;

            if (_coinPool.Count > 0)
            {
                coin = _coinPool.Dequeue();
            }
            else
            {
                // 池为空，创建新金币
                coin = Instantiate(coinPrefab, coinParent);
            }

            if (coin != null)
            {
                coin.SetActive(true);
                coin.GetComponent<BasicCoinAnimation>().Reset();
                _activeCoins.Add(coin);
            }

            return coin;
        }

        /// <summary>
        /// 返还金币到池中
        /// </summary>
        public void ReturnCoin(GameObject coin)
        {
            if (coin != null && _activeCoins.Contains(coin))
            {
                _activeCoins.Remove(coin);
                coin.SetActive(false);
                coin.GetComponent<BasicCoinAnimation>().Reset();
                _coinPool.Enqueue(coin);
            }
        }

        /// <summary>
        /// 创建金币动画到目标位置
        /// </summary>
        public void CreateCoinAnimation(Vector3 startPosition, Vector3 targetPosition)
        {
            GameObject coin = GetCoin();
            if (coin != null)
            {
                coin.transform.position = startPosition;
                BasicCoinAnimation animation = coin.GetComponent<BasicCoinAnimation>();
                animation.MoveTo(targetPosition);
            }
        }

        /// <summary>
        /// 创建收集动画
        /// </summary>
        public void CreateCollectionAnimation(Vector3 startPosition, Vector3 collectPoint)
        {
            GameObject coin = GetCoin();
            if (coin != null)
            {
                coin.transform.position = startPosition;
                BasicCoinAnimation animation = coin.GetComponent<BasicCoinAnimation>();
                animation.Collect(collectPoint);
            }
        }

        /// <summary>
        /// 创建飞行动画（带抛物线轨迹）
        /// </summary>
        public void CreateFlyAnimation(Vector3 startPosition, Vector3 targetPosition)
        {
            GameObject coin = GetCoin();
            if (coin != null)
            {
                coin.transform.position = startPosition;
                BasicCoinAnimation animation = coin.GetComponent<BasicCoinAnimation>();
                animation.FlyTo(targetPosition);
            }
        }

        /// <summary>
        /// 清理所有金币
        /// </summary>
        public void ClearAllCoins()
        {
            foreach (var coin in _activeCoins)
            {
                if (coin != null)
                {
                    coin.SetActive(false);
                    _coinPool.Enqueue(coin);
                }
            }
            _activeCoins.Clear();
        }
    }
}