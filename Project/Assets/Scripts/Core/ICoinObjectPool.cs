using System;
using UnityEngine;

namespace CoinAnimation.Core
{
    /// <summary>
    /// Memory-efficient coin instantiation, lifecycle management, garbage collection prevention
    /// Interface defined in Story Context XML for Epic 1 Story 2
    /// </summary>
    public interface ICoinObjectPool
    {
        /// <summary>
        /// Gets a coin instance from the pool or creates new one if unavailable
        /// </summary>
        /// <param name="position">Initial position for the coin</param>
        /// <param name="rotation">Initial rotation for the coin</param>
        /// <returns>Coin instance</returns>
        GameObject GetCoin(Vector3 position, Quaternion rotation);
        
        /// <summary>
        /// Returns a coin instance back to the pool for reuse
        /// </summary>
        /// <param name="coin">Coin instance to return</param>
        void ReturnCoin(GameObject coin);
        
        /// <summary>
        /// Pre-warms the pool with specified number of coin instances
        /// </summary>
        /// <param name="count">Number of coins to pre-create</param>
        void PreWarmPool(int count);
        
        /// <summary>
        /// Gets current pool statistics for monitoring
        /// </summary>
        /// <returns>Pool statistics</returns>
        PoolStatistics GetPoolStatistics();
        
        /// <summary>
        /// Clears all inactive coin instances from the pool
        /// </summary>
        void ClearPool();
        
        /// <summary>
        /// Sets the maximum pool size to prevent memory bloat
        /// </summary>
        /// <param name="maxSize">Maximum number of coins in pool</param>
        void SetMaxPoolSize(int maxSize);
        
        /// <summary>
        /// Event triggered when pool creates new coin instance
        /// </summary>
        event Action<GameObject> OnCoinCreated;
        
        /// <summary>
        /// Event triggered when pool reaches maximum capacity
        /// </summary>
        event Action<int> OnPoolCapacityReached;
    }
    
    /// <summary>
    /// Pool statistics for monitoring object pool performance
    /// </summary>
    [Serializable]
    public class PoolStatistics
    {
        public int totalCoinsCreated;
        public int activeCoins;
        public int inactiveCoins;
        public int maxPoolSize;
        public float memoryUsageEstimate;
        public DateTime lastAccessTime;
        public float averageAccessTime;
    }
}