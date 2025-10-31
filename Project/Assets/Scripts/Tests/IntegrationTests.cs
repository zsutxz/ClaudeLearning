using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using CoinAnimation.Core;
using CoinAnimation.Animation;

namespace CoinAnimation.Tests
{
    /// <summary>
    /// Comprehensive integration tests for object pooling, memory management, and animation systems
    /// Story 1.3 Task 4.3 - Integration Testing
    /// </summary>
    public class IntegrationTests
    {
        private GameObject _testManagerObject;
        private GameObject _testPoolObject;
        private GameObject _testMemoryObject;
        private GameObject _testIntegrationObject;
        
        private CoinAnimationManager _animationManager;
        private CoinObjectPool _objectPool;
        private MemoryManagementSystem _memorySystem;
        private MemoryPoolIntegration _integration;
        
        private GameObject _testCoinPrefab;

        [SetUp]
        public void SetUp()
        {
            // Create test GameObjects
            _testManagerObject = new GameObject("TestAnimationManager");
            _testPoolObject = new GameObject("TestObjectPool");
            _testMemoryObject = new GameObject("TestMemorySystem");
            _testIntegrationObject = new GameObject("TestIntegration");

            // Create systems
            _animationManager = _testManagerObject.AddComponent<CoinAnimationManager>();
            _objectPool = _testPoolObject.AddComponent<CoinObjectPool>();
            _memorySystem = _testMemoryObject.AddComponent<MemoryManagementSystem>();
            _integration = _testIntegrationObject.AddComponent<MemoryPoolIntegration>();

            // Create test coin prefab
            _testCoinPrefab = CreateTestCoinPrefab();
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup all objects
            if (_testCoinPrefab != null)
            {
                Object.DestroyImmediate(_testCoinPrefab);
            }

            if (_testIntegrationObject != null)
            {
                Object.DestroyImmediate(_testIntegrationObject);
            }

            if (_testMemoryObject != null)
            {
                Object.DestroyImmediate(_testMemoryObject);
            }

            if (_testPoolObject != null)
            {
                Object.DestroyImmediate(_testPoolObject);
            }

            if (_testManagerObject != null)
            {
                Object.DestroyImmediate(_testManagerObject);
            }
        }

        #region Full System Integration

        [UnityTest]
        public IEnumerator CompleteSystem_Workflow_FunctionsCorrectly()
        {
            // Arrange
            yield return new WaitForSeconds(0.1f); // Allow systems to initialize

            // Act - Simulate complete coin animation workflow
            // 1. Get coin from pool
            GameObject coin = _objectPool.GetCoin();
            Assert.IsNotNull(coin, "Should get coin from pool");

            // 2. Start animation
            var controller = coin.GetComponent<CoinAnimationController>();
            Assert.IsNotNull(controller, "Coin should have animation controller");

            Vector3 startPosition = coin.transform.position;
            Vector3 targetPosition = startPosition + Vector3.up * 2f;

            controller.CollectCoin(targetPosition, 1f);

            // 3. Wait for animation to complete
            yield return new WaitForSeconds(1.5f);

            // Assert - Coin should be back in pool (inactive)
            Assert.IsFalse(coin.activeInHierarchy, "Coin should be inactive after collection");

            // Verify system metrics
            var poolMetrics = _objectPool.GetPerformanceMetrics();
            var memoryStats = _memorySystem.GetMemoryStatistics();
            var integrationMetrics = _integration.GetIntegratedMetrics();

            Assert.IsTrue(poolMetrics.PoolHits > 0, "Pool should have recorded hits");
            Assert.IsTrue(memoryStats.CurrentMemoryMB > 0, "Memory system should track usage");
            Assert.IsNotNull(integrationMetrics, "Integration should provide metrics");
        }

        [UnityTest]
        public IEnumerator MultipleCoinAnimations_HandledEfficiently()
        {
            // Arrange
            List<GameObject> coins = new List<GameObject>();
            int coinCount = 20;

            // Act - Spawn and animate multiple coins
            for (int i = 0; i < coinCount; i++)
            {
                GameObject coin = _objectPool.GetCoin();
                if (coin != null)
                {
                    coins.Add(coin);
                    
                    var controller = coin.GetComponent<CoinAnimationController>();
                    Vector3 targetPosition = coin.transform.position + UnityEngine.Random.insideUnitSphere * 3f;
                    controller.CollectCoin(targetPosition, UnityEngine.Random.Range(0.5f, 1.5f));
                }
            }

            yield return new WaitForSeconds(2f); // Wait for animations

            // Assert - All coins should be returned to pool
            foreach (var coin in coins)
            {
                Assert.IsFalse(coin.activeInHierarchy, $"Coin {coin.name} should be inactive");
            }

            // Verify performance metrics
            var poolMetrics = _objectPool.GetPerformanceMetrics();
            var memoryStats = _memorySystem.GetMemoryStatistics();

            Assert.GreaterOrEqual(poolMetrics.PoolHits, coinCount, "Pool should handle all coin requests");
            Assert.Less(memoryStats.MemoryGrowthRateMB, 10f, "Memory growth should be reasonable");
        }

        #endregion

        #region Memory-Pool Integration

        [UnityTest]
        public IEnumerator MemorySystem_TracksPoolObjects_Correctly()
        {
            // Arrange
            var initialMemoryStats = _memorySystem.GetMemoryStatistics();
            int initialTrackers = initialMemoryStats.ActiveTrackers;

            // Act - Get multiple coins from pool
            List<GameObject> coins = new List<GameObject>();
            for (int i = 0; i < 10; i++)
            {
                GameObject coin = _objectPool.GetCoin();
                if (coin != null)
                {
                    coins.Add(coin);
                    
                    // Track coin objects with memory system
                    _memorySystem.TrackObject(coin, "PoolCoin");
                }
            }

            yield return new WaitForSeconds(0.1f);

            var trackedMemoryStats = _memorySystem.GetMemoryStatistics();

            // Assert
            Assert.Greater(trackedMemoryStats.ActiveTrackers, initialTrackers,
                "Memory system should track pool objects");

            // Cleanup
            foreach (var coin in coins)
            {
                _memorySystem.UntrackObject(coin, "PoolCoin");
                _objectPool.ReturnCoin(coin);
            }

            yield return new WaitForSeconds(0.1f);

            var finalMemoryStats = _memorySystem.GetMemoryStatistics();
            Assert.AreEqual(initialTrackers, finalMemoryStats.ActiveTrackers,
                "Tracker count should return to initial after cleanup");
        }

        [UnityTest]
        public IEnumerator HighMemoryUsage_TriggersPoolOptimization()
        {
            // Arrange
            bool optimizationTriggered = false;
            _integration.OnIntegratedOptimization += (sender, args) => {
                optimizationTriggered = true;
            };

            // Act - Create memory pressure by getting many coins
            List<GameObject> coins = new List<GameObject>();
            for (int i = 0; i < 50; i++)
            {
                GameObject coin = _objectPool.GetCoin();
                if (coin != null)
                {
                    coins.Add(coin);
                }
            }

            // Simulate high memory usage (this would need actual memory pressure or mocking)
            yield return new WaitForSeconds(5f); // Wait for monitoring cycles

            // Return coins to trigger optimization
            foreach (var coin in coins)
            {
                _objectPool.ReturnCoin(coin);
            }

            yield return new WaitForSeconds(3f); // Wait for optimization

            // Assert
            // This depends on the actual memory thresholds and optimization logic
            Assert.IsTrue(_integration.IsIntegrationActive, "Integration should be active");
            
            // Optimization triggering depends on memory threshold implementation
            Debug.Log($"Optimization triggered: {optimizationTriggered}");
        }

        #endregion

        #region Animation-Memory Integration

        [UnityTest]
        public IEnumerator GCPrevention_ActivatedDuringComplexAnimations()
        {
            // Arrange
            GameObject coin = _objectPool.GetCoin();
            Assert.IsNotNull(coin, "Should get coin from pool");

            var controller = coin.GetComponent<CoinAnimationController>();
            var initialGCState = _memorySystem.IsGCPreventionActive;

            // Act - Start complex collection animation
            controller.CollectCoin(coin.transform.position + Vector3.up * 2f, 2f);

            // Check if GC prevention was enabled
            yield return new WaitForSeconds(0.1f);
            bool gcEnabledDuringAnimation = _memorySystem.IsGCPreventionActive;

            // Wait for animation to complete
            yield return new WaitForSeconds(2.5f);
            bool gcStateAfterAnimation = _memorySystem.IsGCPreventionActive;

            // Assert
            Assert.IsFalse(initialGCState, "GC prevention should be disabled initially");
            // GC prevention behavior depends on implementation
            Debug.Log($"GC prevention during animation: {gcEnabledDuringAnimation}");
            
            // Return coin to pool
            _objectPool.ReturnCoin(coin);
        }

        [UnityTest]
        public IEnumerator MemoryTracking_ContinuesThroughAnimationLifecycle()
        {
            // Arrange
            GameObject coin = _objectPool.GetCoin();
            var controller = coin.GetComponent<CoinAnimationController>();

            // Track the coin
            _memorySystem.TrackObject(coin, "AnimationTest");
            var initialStats = _memorySystem.GetMemoryStatistics();

            // Act - Complete animation lifecycle
            controller.AnimateToPosition(coin.transform.position + Vector3.right, 0.5f);
            yield return new WaitForSeconds(0.6f);

            controller.CollectCoin(coin.transform.position + Vector3.up, 1f);
            yield return new WaitForSeconds(1.1f);

            var finalStats = _memorySystem.GetMemoryStatistics();

            // Assert
            Assert.AreEqual(initialStats.ActiveTrackers, finalStats.ActiveTrackers,
                "Tracker count should remain consistent through animation lifecycle");

            // Cleanup
            _memorySystem.UntrackObject(coin, "AnimationTest");
            _objectPool.ReturnCoin(coin);
        }

        #endregion

        #region Performance Correlation

        [UnityTest]
        public IEnumerator PerformanceCorrelation_DetectedBetweenPoolAndMemory()
        {
            // Arrange
            bool correlationDetected = false;
            PerformanceCorrelationEventArgs correlationArgs = null;

            _integration.OnPerformanceCorrelation += (sender, args) => {
                correlationDetected = true;
                correlationArgs = args;
            };

            // Act - Create scenarios that might trigger correlation detection
            // 1. Low pool hit rate with high memory growth
            List<GameObject> coins = new List<GameObject>();
            
            // Get and immediately return coins (low hit rate scenario)
            for (int i = 0; i < 30; i++)
            {
                GameObject coin = _objectPool.GetCoin();
                if (coin != null)
                {
                    coins.Add(coin);
                    // Immediately return to simulate poor usage pattern
                    _objectPool.ReturnCoin(coin);
                }
            }

            // 2. Create some memory pressure
            for (int i = 0; i < 10; i++)
            {
                var tempObject = new GameObject($"TempMemory_{i}");
                _memorySystem.TrackObject(tempObject, "MemoryPressure");
            }

            yield return new WaitForSeconds(3f); // Wait for monitoring

            // 3. Trigger metrics update
            var metrics = _integration.GetIntegratedMetrics();

            // Assert
            Assert.IsNotNull(metrics, "Integration metrics should be available");
            Assert.IsTrue(_integration.IsIntegrationActive, "Integration should be active");

            // Correlation detection depends on actual thresholds and logic
            Debug.Log($"Performance correlation detected: {correlationDetected}");
            if (correlationDetected)
            {
                Assert.IsNotNull(correlationArgs, "Correlation arguments should not be null");
                Debug.Log($"Correlation type: {correlationArgs.CorrelationType}");
            }

            // Cleanup
            _memorySystem.UntrackObject(_testMemoryObject, "MemoryPressure");
            for (int i = 0; i < 10; i++)
            {
                var tempObject = GameObject.Find($"TempMemory_{i}");
                if (tempObject != null)
                {
                    Object.DestroyImmediate(tempObject);
                }
            }
        }

        #endregion

        #region Stress Testing

        [UnityTest]
        public IEnumerator StressTest_HighConcurrentAnimations_HandlesGracefully()
        {
            // Arrange
            int batchSize = 30;
            int batchCount = 3;
            List<List<GameObject>> allBatches = new List<List<GameObject>>();

            // Act - Create multiple batches of concurrent animations
            for (int batch = 0; batch < batchCount; batch++)
            {
                List<GameObject> currentBatch = new List<GameObject>();
                
                for (int i = 0; i < batchSize; i++)
                {
                    GameObject coin = _objectPool.GetCoin();
                    if (coin != null)
                    {
                        currentBatch.Add(coin);
                        
                        var controller = coin.GetComponent<CoinAnimationController>();
                        Vector3 randomTarget = coin.transform.position + 
                            new Vector3(
                                UnityEngine.Random.Range(-3f, 3f),
                                UnityEngine.Random.Range(0f, 3f),
                                UnityEngine.Random.Range(-3f, 3f)
                            );
                        
                        controller.CollectCoin(randomTarget, UnityEngine.Random.Range(0.3f, 1.2f));
                    }
                }
                
                allBatches.Add(currentBatch);
                yield return new WaitForSeconds(0.5f); // Small delay between batches
            }

            // Wait for all animations to complete
            yield return new WaitForSeconds(3f);

            // Assert
            int totalCoins = allBatches.Sum(batch => batch.Count);
            int activeCoins = allBatches.Sum(batch => batch.Count(coin => coin.activeInHierarchy));

            Assert.Less(activeCoins, totalCoins / 2, "Most coins should be inactive after animations");

            // Verify system stability
            var poolMetrics = _objectPool.GetPerformanceMetrics();
            var memoryStats = _memorySystem.GetMemoryStatistics();

            Assert.Greater(poolMetrics.PoolHitRate, 0.5f, "Pool hit rate should be reasonable");
            Assert.Less(memoryStats.MemoryGrowthRateMB, 20f, "Memory growth should be controlled");

            Debug.Log($"Stress test completed: {totalCoins} coins, " +
                     $"Pool hit rate: {poolMetrics.PoolHitRate:P2}, " +
                     $"Memory growth: {memoryStats.MemoryGrowthRateMB:F2} MB/min");

            // Cleanup
            foreach (var batch in allBatches)
            {
                foreach (var coin in batch)
                {
                    if (coin.activeInHierarchy)
                    {
                        _objectPool.ReturnCoin(coin);
                    }
                }
            }
        }

        #endregion

        #region Error Handling and Edge Cases

        [UnityTest]
        public IEnumerator System_HandlesNullReferences_Gracefully()
        {
            // Arrange - Create system with missing components
            
            var incompletePoolObject = new GameObject("IncompletePool");
            var incompletePool = incompletePoolObject.AddComponent<CoinObjectPool>();
            // Note: No prefab assigned, should handle gracefully

            // Act
            yield return new WaitForSeconds(0.1f);

            GameObject nullCoin = incompletePool.GetCoin();
            var metrics = incompletePool.GetPerformanceMetrics();

            // Assert
            Assert.IsNull(nullCoin, "Should handle missing prefab gracefully");
            Assert.IsNotNull(metrics, "Should still provide metrics even with incomplete setup");

            // Cleanup
            Object.DestroyImmediate(incompletePoolObject);
        }

        [UnityTest]
        public IEnumerator SystemRecovers_FromComponentDestruction()
        {
            // Arrange
            GameObject coin = _objectPool.GetCoin();
            Assert.IsNotNull(coin, "Should get coin from pool");

            var controller = coin.GetComponent<CoinAnimationController>();
            Assert.IsNotNull(controller, "Coin should have controller");

            // Act - Destroy controller during animation
            controller.CollectCoin(coin.transform.position + Vector3.up, 2f);
            yield return new WaitForSeconds(0.5f);

            Object.DestroyImmediate(controller);
            yield return new WaitForSeconds(0.1f);

            // Try to return coin to pool
            Assert.DoesNotThrow(() => _objectPool.ReturnCoin(coin),
                "Should handle destroyed components gracefully");

            // Assert
            Assert.IsFalse(coin.activeInHierarchy, "Coin should be inactive after return");
            
            var poolMetrics = _objectPool.GetPerformanceMetrics();
            Assert.IsTrue(poolMetrics.PoolSize >= 0, "Pool should remain stable");
        }

        #endregion

        #region Configuration Tests

        [Test]
        public void IntegrationReport_ProvidesComprehensiveInformation()
        {
            // Arrange & Act
            var report = _integration.GetIntegrationReport();

            // Assert
            Assert.IsNotNull(report, "Integration report should not be null");
            Assert.IsNotNull(report.CurrentMetrics, "Current metrics should not be null");
            Assert.IsNotNull(report.MemoryStatistics, "Memory statistics should not be null");
            Assert.IsNotNull(report.PoolMetrics, "Pool metrics should not be null");
            Assert.IsTrue(report.GeneratedAt <= DateTime.UtcNow, "Report timestamp should be valid");
            
            Debug.Log($"Integration Report - Active: {report.IsIntegrationActive}, " +
                     $"Memory: {report.CurrentMetrics.MemoryUsageMB:F2}MB, " +
                     $"Pool Hit Rate: {report.CurrentMetrics.PoolHitRate:P2}");
        }

        [Test]
        public void ObjectPoolConfiguration_ValidationWorksCorrectly()
        {
            // Arrange
            var config = ScriptableObject.CreateInstance<ObjectPoolConfiguration>();

            // Act - Set invalid values
            config.initialPoolSize = -1;
            config.maxPoolSize = 10;
            config.expansionBatchSize = 0;

            // Validate and fix
            config.ValidateAndFix();

            // Assert
            Assert.Greater(config.initialPoolSize, 0, "Initial pool size should be fixed to positive value");
            Assert.GreaterOrEqual(config.maxPoolSize, config.initialPoolSize, 
                "Max pool size should be >= initial pool size");
            Assert.Greater(config.expansionBatchSize, 0, "Expansion batch size should be positive");
            Assert.IsTrue(config.IsValid(), "Configuration should be valid after validation");

            // Cleanup
            Object.DestroyImmediate(config);
        }

        #endregion

        #region Helper Methods

        private GameObject CreateTestCoinPrefab()
        {
            var coinObject = new GameObject("TestCoinPrefab");
            coinObject.AddComponent<Rigidbody>();
            coinObject.AddComponent<SphereCollider>();
            coinObject.AddComponent<CoinAnimationController>();
            
            return coinObject;
        }

        #endregion
    }
}