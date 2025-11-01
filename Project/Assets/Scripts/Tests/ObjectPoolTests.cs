using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using CoinAnimation.Core;
using CoinAnimation.Animation;
using Object = UnityEngine.Object;

namespace CoinAnimation.Tests
{
    /// <summary>
    /// Comprehensive test suite for CoinObjectPool
    /// Story 1.3 Task 4.1 - Object Pool Testing
    /// </summary>
    public class ObjectPoolTests
    {
        private GameObject _testGameObject;
        private CoinObjectPool _objectPool;
        private GameObject _testCoinPrefab;

        [SetUp]
        public void SetUp()
        {
            // Create test GameObject and pool
            _testGameObject = new GameObject("TestObject");
            _objectPool = _testGameObject.AddComponent<CoinObjectPool>();

            // Create test coin prefab
            _testCoinPrefab = new GameObject("TestCoin");
            _testCoinPrefab.AddComponent<Rigidbody>();
            _testCoinPrefab.AddComponent<Collider>();
            _testCoinPrefab.AddComponent<CoinAnimationController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_objectPool != null)
            {
                _objectPool.ClearPool();
            }
            
            if (_testCoinPrefab != null)
            {
                Object.DestroyImmediate(_testCoinPrefab);
            }
            
            if (_testGameObject != null)
            {
                Object.DestroyImmediate(_testGameObject);
            }
        }

        #region Basic Pool Operations

        [Test]
        public void ObjectPool_InitializesWithCorrectSize()
        {
            // Arrange & Act
            InitializePoolWithPrefab();

            // Assert
            Assert.IsTrue(_objectPool.IsPoolInitialized, "Pool should be initialized");
            Assert.Greater(_objectPool.CurrentPoolSize, 0, "Pool should have coins after initialization");
        }

        [Test]
        public void GetCoin_ReturnsValidCoin_WhenPoolHasAvailableCoins()
        {
            // Arrange
            InitializePoolWithPrefab();
            
            // Act
            GameObject coin = _objectPool.GetCoin();

            // Assert
            Assert.IsNotNull(coin, "Should return a valid coin");
            Assert.IsTrue(coin.activeInHierarchy, "Returned coin should be active");
            Assert.IsNotNull(coin.GetComponent<CoinAnimationController>(), "Coin should have animation controller");
        }

        [Test]
        public void GetCoin_ReturnsNull_WhenPoolIsEmptyAndAtMaxCapacity()
        {
            // Arrange
            InitializePoolWithPrefab();
            
            // Get all coins until pool is empty
            while (_objectPool.AvailableCoinCount > 0)
            {
                var coin = _objectPool.GetCoin();
                Assert.IsNotNull(coin, "Should be able to get coins while available");
            }

            // Try to get one more coin
            GameObject extraCoin = _objectPool.GetCoin();

            // Assert
            Assert.IsNull(extraCoin, "Should return null when pool is at max capacity");
        }

        [Test]
        public void ReturnCoin_SuccessfullyReturnsCoinToPool()
        {
            // Arrange
            InitializePoolWithPrefab();
            GameObject coin = _objectPool.GetCoin();
            int initialAvailableCount = _objectPool.AvailableCoinCount;

            // Act
            _objectPool.ReturnCoin(coin);

            // Assert
            Assert.AreEqual(initialAvailableCount + 1, _objectPool.AvailableCoinCount, 
                "Available coin count should increase after returning coin");
            Assert.IsFalse(coin.activeInHierarchy, "Returned coin should be inactive");
        }

        [Test]
        public void ReturnCoin_HandlesNullCoin_Gracefully()
        {
            // Arrange
            InitializePoolWithPrefab();
            int initialAvailableCount = _objectPool.AvailableCoinCount;

            // Act & Assert
            Assert.DoesNotThrow(() => _objectPool.ReturnCoin(null), 
                "Should not throw when returning null coin");
            Assert.AreEqual(initialAvailableCount, _objectPool.AvailableCoinCount,
                "Available count should not change when returning null coin");
        }

        #endregion

        #region Pool Expansion and Contraction

        [Test]
        public void Pool_Expands_WhenNoCoinsAvailable()
        {
            // Arrange
            InitializePoolWithPrefab();
            int initialPoolSize = _objectPool.CurrentPoolSize;
            
            // Get all available coins
            var coins = new System.Collections.Generic.List<GameObject>();
            while (_objectPool.AvailableCoinCount > 0 && !_objectPool.IsAtMaxCapacity)
            {
                var coin = _objectPool.GetCoin();
                if (coin != null) coins.Add(coin);
            }

            // Try to get one more coin to trigger expansion
            GameObject expansionCoin = _objectPool.GetCoin();

            // Assert
            Assert.IsTrue(_objectPool.CurrentPoolSize > initialPoolSize, 
                "Pool should expand when no coins available");
            Assert.IsNotNull(expansionCoin, "Should get a valid coin after expansion");
        }

        [Test]
        public void PreWarmPool_IncreasesPoolSizeToTarget()
        {
            // Arrange
            InitializePoolWithPrefab();
            int initialPoolSize = _objectPool.CurrentPoolSize;
            int targetSize = initialPoolSize + 10;

            // Act
            _objectPool.PreWarmPool(targetSize);

            // Assert
            Assert.GreaterOrEqual(_objectPool.CurrentPoolSize, targetSize,
                "Pool size should be at least target size after pre-warming");
        }

        [Test]
        public void ForceExpansion_IncreasesPoolSizeBySpecifiedAmount()
        {
            // Arrange
            InitializePoolWithPrefab();
            int initialPoolSize = _objectPool.CurrentPoolSize;
            int expansionAmount = 5;

            // Act
            int actualExpansion = _objectPool.ForceExpansion(expansionAmount);

            // Assert
            Assert.Greater(actualExpansion, 0, "Should return positive expansion amount");
            Assert.AreEqual(initialPoolSize + actualExpansion, _objectPool.CurrentPoolSize,
                "Pool size should increase by actual expansion amount");
        }

        #endregion

        #region Performance Monitoring

        [Test]
        public void PerformanceMetrics_AccuratelyTrackPoolUsage()
        {
            // Arrange
            InitializePoolWithPrefab();
            var metrics = _objectPool.GetPerformanceMetrics();

            // Act - Get and return some coins
            var coins = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < 5 && _objectPool.AvailableCoinCount > 0; i++)
            {
                var coin = _objectPool.GetCoin();
                if (coin != null) coins.Add(coin);
            }

            // Return half of the coins
            for (int i = 0; i < coins.Count / 2; i++)
            {
                _objectPool.ReturnCoin(coins[i]);
            }

            var updatedMetrics = _objectPool.GetPerformanceMetrics();

            // Assert
            Assert.Greater(updatedMetrics.PoolHits, metrics.PoolHits,
                "Pool hits should increase after getting coins");
            Assert.Greater(updatedMetrics.TotalRequests, metrics.TotalRequests,
                "Total requests should increase after pool operations");
            Assert.IsTrue(updatedMetrics.PoolHitRate > 0, "Pool hit rate should be positive");
        }

        [Test]
        public void PoolHitRate_CalculatedCorrectly()
        {
            // Arrange
            InitializePoolWithPrefab();
            
            // Act - Make multiple requests
            var coins = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < 10; i++)
            {
                var coin = _objectPool.GetCoin();
                if (coin != null) coins.Add(coin);
            }

            var metrics = _objectPool.GetPerformanceMetrics();

            // Assert
            Assert.IsTrue(metrics.PoolHitRate >= 0 && metrics.PoolHitRate <= 1,
                "Pool hit rate should be between 0 and 1");
            Assert.AreEqual(coins.Count / (float)metrics.TotalRequests, metrics.PoolHitRate, 0.01f,
                "Pool hit rate should match expected calculation");
        }

        #endregion

        #region Thread Safety Tests

        [UnityTest]
        public IEnumerator Concurrent_GetAndReturn_Operations_DoNotCauseExceptions()
        {
            // Arrange
            InitializePoolWithPrefab();
            int operationCount = 100;
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<System.Exception>();

            // Act - Simulate concurrent operations
            var tasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>();
            
            for (int i = 0; i < operationCount; i++)
            {
                int index = i;
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        var coin = _objectPool.GetCoin();
                        System.Threading.Thread.Sleep(1); // Simulate work
                        if (coin != null)
                        {
                            _objectPool.ReturnCoin(coin);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            // Wait for all tasks to complete
            foreach (var task in tasks)
            {
                yield return new WaitUntil(() => task.IsCompleted);
            }

            // Assert
            Assert.IsEmpty(exceptions, "No exceptions should occur during concurrent operations");
            Assert.AreEqual(operationCount, _objectPool.GetPerformanceMetrics().TotalRequests,
                "All operations should be recorded");
        }

        #endregion

        #region Edge Cases

        [Test]
        public void ClearPool_RemovesAllCoinsAndResetsPool()
        {
            // Arrange
            InitializePoolWithPrefab();
            
            // Get some coins to make pool active
            var coins = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < 5 && _objectPool.AvailableCoinCount > 0; i++)
            {
                var coin = _objectPool.GetCoin();
                if (coin != null) coins.Add(coin);
            }

            Assert.Greater(_objectPool.ActiveCoinCount, 0, "Should have active coins before clear");

            // Act
            _objectPool.ClearPool();

            // Assert
            Assert.AreEqual(0, _objectPool.CurrentPoolSize, "Pool size should be 0 after clear");
            Assert.AreEqual(0, _objectPool.ActiveCoinCount, "Active coin count should be 0 after clear");
            Assert.AreEqual(0, _objectPool.AvailableCoinCount, "Available coin count should be 0 after clear");
        }

        [Test]
        public void GetFromUninitializedPool_ReturnsNull()
        {
            // Arrange - Don't initialize pool with prefab
            
            // Act
            GameObject coin = _objectPool.GetCoin();

            // Assert
            Assert.IsNull(coin, "Should return null from uninitialized pool");
            Assert.IsFalse(_objectPool.IsPoolInitialized, "Pool should not be initialized");
        }

        [Test]
        public void Pool_AtMaxCapacity_BehavesCorrectly()
        {
            // Arrange
            InitializePoolWithPrefab();
            
            // Fill pool to capacity
            var coins = new System.Collections.Generic.List<GameObject>();
            while (!_objectPool.IsAtMaxCapacity && _objectPool.AvailableCoinCount > 0)
            {
                var coin = _objectPool.GetCoin();
                if (coin != null) coins.Add(coin);
            }

            Assert.IsTrue(_objectPool.IsAtMaxCapacity || _objectPool.IsEmpty, 
                "Pool should be at max capacity or empty");

            // Act - Try to get more coins
            var additionalCoins = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < 5; i++)
            {
                var coin = _objectPool.GetCoin();
                if (coin != null) additionalCoins.Add(coin);
            }

            // Assert
            Assert.IsEmpty(additionalCoins, "Should not get additional coins when at max capacity");
        }

        #endregion

        #region Event Tests

        [Test]
        public void PoolExpanded_Event_TriggeredCorrectly()
        {
            // Arrange
            InitializePoolWithPrefab();
            bool eventTriggered = false;
            int oldSize = 0;
            int newSize = 0;
            
            _objectPool.OnPoolExpanded += (old, newSize) => {
                eventTriggered = true;
                oldSize = old;
                newSize = newSize;
            };

            // Act - Force expansion
            _objectPool.ForceExpansion(5);

            // Assert
            Assert.IsTrue(eventTriggered, "Pool expanded event should be triggered");
            Assert.Greater(newSize, oldSize, "New size should be greater than old size");
        }

        [Test]
        public void PerformanceThresholdExceeded_Event_TriggeredWhenNeeded()
        {
            // Arrange
            InitializePoolWithPrefab();
            bool eventTriggered = false;
            PoolPerformanceMetrics eventMetrics = null;
            
            _objectPool.OnPerformanceThresholdExceeded += (metrics) => {
                eventTriggered = true;
                eventMetrics = metrics;
            };

            // Act - Create performance degradation by getting many coins
            var coins = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < 50; i++)
            {
                var coin = _objectPool.GetCoin();
                if (coin != null) coins.Add(coin);
            }

            // Force metrics update
            var metrics = _objectPool.GetPerformanceMetrics();

            // Assert
            if (metrics.IsPerformanceDegraded)
            {
                Assert.IsTrue(eventTriggered, "Performance threshold event should be triggered when degraded");
                Assert.IsNotNull(eventMetrics, "Event metrics should not be null");
            }
        }

        #endregion

        #region Helper Methods

        private void InitializePoolWithPrefab()
        {
            // Use reflection or create a test prefab setup
            // For now, we'll create a simple test setup
            
            // Create a simple test prefab setup
            if (_objectPool != null)
            {
                // This would need the actual prefab assignment in the inspector
                // For automated tests, we'd need to create the setup programmatically
                Debug.Log("ObjectPool test initialized - note: requires manual prefab assignment for full functionality");
            }
        }

        #endregion
    }
}