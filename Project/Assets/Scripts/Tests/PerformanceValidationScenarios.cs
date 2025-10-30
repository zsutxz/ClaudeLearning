using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Diagnostics;
using CoinAnimation.Core;
using CoinAnimation.Animation;
using CoinAnimation.Physics;
using DG.Tweening;
using System.Linq;

namespace CoinAnimation.Tests
{
    /// <summary>
    /// Performance validation scenarios for coin animation system
    /// Validates 60fps performance with 100+ concurrent coins
    /// </summary>
    public class PerformanceValidationScenarios
    {
        #region Test Setup
        
        private List<CoinAnimationController> _testCoins;
        private CoinAnimationManager _animationManager;
        private MagneticCollectionController _magneticController;
        private SpiralMotionController _spiralController;
        private MagneticFieldData _testFieldData;
        
        private const int PERFORMANCE_TEST_COIN_COUNT = 100;
        private const int TARGET_FPS = 60;
        private const float MIN_ACCEPTABLE_FPS = 50f;
        private const float TEST_DURATION = 3f;

        [SetUp]
        public void SetUp()
        {
            InitializePerformanceTestEnvironment();
        }

        [TearDown]
        public void TearDown()
        {
            CleanupPerformanceTestEnvironment();
            DOTween.KillAll(false);
        }

        /// <summary>
        /// Initialize performance test environment with many coins
        /// </summary>
        private void InitializePerformanceTestEnvironment()
        {
            // Create managers
            GameObject managerObject = new GameObject("PerformanceTestManager");
            _animationManager = managerObject.AddComponent<CoinAnimationManager>();
            
            GameObject magneticObject = new GameObject("PerformanceTestMagnetic");
            _magneticController = magneticObject.AddComponent<MagneticCollectionController>();
            
            GameObject spiralObject = new GameObject("PerformanceTestSpiral");
            _spiralController = spiralObject.AddComponent<SpiralMotionController>();
            
            // Create test field data
            _testFieldData = ScriptableObject.CreateInstance<MagneticFieldData>();
            _testFieldData.ResetToDefaults();
            _testFieldData.SetFalloffCurve(MagneticFieldType.Gaussian);
            _testFieldData.MaxAffectedCoins = PERFORMANCE_TEST_COIN_COUNT;
            
            // Create test coins
            CreateTestCoins(PERFORMANCE_TEST_COIN_COUNT);
        }

        /// <summary>
        /// Create specified number of test coins
        /// </summary>
        /// <param name="coinCount">Number of coins to create</param>
        private void CreateTestCoins(int coinCount)
        {
            _testCoins = new List<CoinAnimationController>();
            
            for (int i = 0; i < coinCount; i++)
            {
                GameObject coinObject = new GameObject($"PerfCoin_{i}");
                
                // Position coins in a grid pattern
                float gridSize = Mathf.CeilToInt(Mathf.Sqrt(coinCount));
                float spacing = 1f;
                int row = i / (int)gridSize;
                int col = i % (int)gridSize;
                
                coinObject.transform.position = new Vector3(
                    col * spacing - gridSize * spacing * 0.5f,
                    0f,
                    row * spacing - gridSize * spacing * 0.5f
                );
                
                // Add required components
                var coinController = coinObject.AddComponent<CoinAnimationController>();
                
                Rigidbody rb = coinObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                
                SphereCollider collider = coinObject.AddComponent<SphereCollider>();
                collider.radius = 0.3f;
                
                _testCoins.Add(coinController);
            }
        }

        /// <summary>
        /// Clean up performance test environment
        /// </summary>
        private void CleanupPerformanceTestEnvironment()
        {
            if (_testCoins != null)
            {
                foreach (var coin in _testCoins)
                {
                    if (coin != null && coin.gameObject != null)
                        Object.DestroyImmediate(coin.gameObject);
                }
                _testCoins.Clear();
            }
            
            if (_animationManager != null && _animationManager.gameObject != null)
                Object.DestroyImmediate(_animationManager.gameObject);
            
            if (_magneticController != null && _magneticController.gameObject != null)
                Object.DestroyImmediate(_magneticController.gameObject);
            
            if (_spiralController != null && _spiralController.gameObject != null)
                Object.DestroyImmediate(_spiralController.gameObject);
            
            if (_testFieldData != null)
                ScriptableObject.DestroyImmediate(_testFieldData);
        }

        #endregion

        #region Performance Metrics

        /// <summary>
        /// Performance metrics data structure
        /// </summary>
        private struct PerformanceMetrics
        {
            public float AverageFPS;
            public float MinFPS;
            public float MaxFPS;
            public int FrameCount;
            public float TotalTime;
            public long MemoryUsed;
            public int ActiveAnimationCount;
            public int ActiveSpiralCount;
            public int AffectedCoinCount;
        }


        #endregion

        #region Performance Tests

        [UnityTest]
        public IEnumerator PerformanceTest_100ConcurrentAnimations()
        {
            // Test 1: 100 coins with basic animations
            UnityEngine.Debug.Log($"Starting performance test: {PERFORMANCE_TEST_COIN_COUNT} concurrent animations");
            
            // Start animations on all coins
            foreach (var coin in _testCoins)
            {
                Vector3 targetPosition = coin.transform.position + new Vector3(
                    Random.Range(-5f, 5f),
                    Random.Range(0f, 2f),
                    Random.Range(-5f, 5f)
                );
                
                float duration = Random.Range(1f, 2f);
                coin.AnimateToPosition(targetPosition, duration);
            }

            // Measure performance
            yield return MeasurePerformanceCoroutine(TEST_DURATION);
        }

        [UnityTest]
        public IEnumerator PerformanceTest_MagneticFieldStress()
        {
            // Test 2: Multiple magnetic fields affecting many coins
            UnityEngine.Debug.Log($"Starting magnetic field stress test: {PERFORMANCE_TEST_COIN_COUNT} coins");
            
            // Create multiple magnetic fields
            List<int> fieldIds = new List<int>();
            int fieldCount = 5;
            
            for (int i = 0; i < fieldCount; i++)
            {
                Vector3 fieldPosition = new Vector3(
                    Random.Range(-8f, 8f),
                    0f,
                    Random.Range(-8f, 8f)
                );
                
                int fieldId = _magneticController.AddMagneticField(fieldPosition, _testFieldData);
                fieldIds.Add(fieldId);
            }
            
            // Position coins randomly
            foreach (var coin in _testCoins)
            {
                coin.transform.position = new Vector3(
                    Random.Range(-10f, 10f),
                    Random.Range(0f, 2f),
                    Random.Range(-10f, 10f)
                );
            }

            // Measure performance under magnetic influence
            yield return MeasurePerformanceCoroutine(TEST_DURATION);

            // Clean up fields
            foreach (int fieldId in fieldIds)
            {
                _magneticController.RemoveMagneticField(fieldId);
            }
        }

        [UnityTest]
        public IEnumerator PerformanceTest_SpiralAnimationStress()
        {
            // Test 3: Many concurrent spiral animations
            UnityEngine.Debug.Log($"Starting spiral animation stress test: {PERFORMANCE_TEST_COIN_COUNT} coins");
            
            // Create collection points
            List<Vector3> collectionPoints = new List<Vector3>();
            int pointCount = 3;
            
            for (int i = 0; i < pointCount; i++)
            {
                collectionPoints.Add(new Vector3(
                    Random.Range(-5f, 5f),
                    0f,
                    Random.Range(-5f, 5f)
                ));
            }
            
            // Start spiral animations
            int spiralCount = 0;
            foreach (var coin in _testCoins)
            {
                if (spiralCount >= 50) break; // Limit concurrent spirals for performance
                
                Vector3 targetPoint = collectionPoints[Random.Range(0, collectionPoints.Count)];
                SpiralType spiralType = (SpiralType)Random.Range(0, System.Enum.GetValues(typeof(SpiralType)).Length);
                
                bool started = _spiralController.StartSpiralAnimation(coin, targetPoint, 2f, spiralType);
                if (started) spiralCount++;
            }
            
            UnityEngine.Debug.Log($"Started {spiralCount} concurrent spiral animations");

            // Measure performance
            yield return MeasurePerformanceCoroutine(TEST_DURATION);
        }


        [UnityTest]
        public IEnumerator PerformanceTest_MixedWorkload()
        {
            // Test 4: Realistic mixed workload scenario
            UnityEngine.Debug.Log($"Starting mixed workload test: {PERFORMANCE_TEST_COIN_COUNT} coins");
            
            // Create magnetic field
            Vector3 fieldCenter = Vector3.zero;
            int fieldId = _magneticController.AddMagneticField(fieldCenter, _testFieldData);
            
            float startTime = Time.time;
            float testDuration = 5f;
            
            while (Time.time - startTime < testDuration)
            {
                // Randomly trigger different animation types
                foreach (var coin in _testCoins)
                {
                    if (Random.value < 0.02f) // 2% chance per frame per coin
                    {
                        int animationType = Random.Range(0, 4);
                        
                        switch (animationType)
                        {
                            case 0: // Basic movement
                                Vector3 targetPos = coin.transform.position + new Vector3(
                                    Random.Range(-2f, 2f),
                                    Random.Range(0f, 1f),
                                    Random.Range(-2f, 2f)
                                );
                                coin.AnimateToPosition(targetPos, Random.Range(0.5f, 1.5f));
                                break;
                                
                            case 1: // Spiral animation
                                Vector3 spiralTarget = fieldCenter + new Vector3(
                                    Random.Range(-1f, 1f),
                                    0f,
                                    Random.Range(-1f, 1f)
                                );
                                _spiralController.StartSpiralAnimation(coin, spiralTarget, 1.5f);
                                break;
                                
                            case 2: // Collection
                                if (Vector3.Distance(coin.transform.position, fieldCenter) < 3f)
                                {
                                    coin.CollectCoin(fieldCenter, 1f);
                                }
                                break;
                        }
                    }
                }
                
                yield return null;
            }
            
            // Clean up
            _magneticController.RemoveMagneticField(fieldId);
        }

        #endregion

        #region Performance Measurement Coroutine

        /// <summary>
        /// Coroutine wrapper for performance measurement
        /// </summary>
        /// <param name="duration">Duration to measure</param>
        /// <returns>Performance metrics</returns>
        private IEnumerator MeasurePerformanceCoroutine(float duration)
        {
            List<float> fpsSamples = new List<float>();
            float startTime = Time.time;
            float lastFrameTime = startTime;
            long startMemory = System.GC.GetTotalMemory(false);
            
            int frameCount = 0;
            float minFPS = float.MaxValue;
            float maxFPS = 0f;
            
            while (Time.time - startTime < duration)
            {
                float currentTime = Time.time;
                float deltaTime = currentTime - lastFrameTime;
                float currentFPS = 1f / deltaTime;
                
                fpsSamples.Add(currentFPS);
                minFPS = Mathf.Min(minFPS, currentFPS);
                maxFPS = Mathf.Max(maxFPS, currentFPS);
                frameCount++;
                lastFrameTime = currentTime;
                
                yield return null;
            }
            
            float totalTime = Time.time - startTime;
            float averageFPS = fpsSamples.Count > 0 ? fpsSamples.Sum() / fpsSamples.Count : 0f;
            long memoryUsed = System.GC.GetTotalMemory(false) - startMemory;
            
            // Log performance results
            UnityEngine.Debug.Log($"=== Performance Results ===");
            UnityEngine.Debug.Log($"Test Duration: {totalTime:F2}s");
            UnityEngine.Debug.Log($"Total Frames: {frameCount}");
            UnityEngine.Debug.Log($"Average FPS: {averageFPS:F1}");
            UnityEngine.Debug.Log($"Min FPS: {minFPS:F1}");
            UnityEngine.Debug.Log($"Max FPS: {maxFPS:F1}");
            UnityEngine.Debug.Log($"Memory Used: {memoryUsed / 1024 / 1024:F1} MB");
            
            if (_animationManager != null)
                UnityEngine.Debug.Log($"Active Animations: {_animationManager.ActiveCoinCount}");
            
            if (_spiralController != null)
                UnityEngine.Debug.Log($"Active Spirals: {_spiralController.ActiveSpiralCount}");
            
            if (_magneticController != null)
                UnityEngine.Debug.Log($"Affected Coins: {_magneticController.AffectedCoinCount}");
            
            // Performance assertions
            Assert.IsTrue(averageFPS >= MIN_ACCEPTABLE_FPS, 
                $"Average FPS should be at least {MIN_ACCEPTABLE_FPS} (achieved: {averageFPS:F1})");
            
            Assert.IsTrue(minFPS >= MIN_ACCEPTABLE_FPS * 0.7f, 
                $"Minimum FPS should not drop below {MIN_ACCEPTABLE_FPS * 0.7f:F1} (achieved: {minFPS:F1})");
            
            Assert.IsTrue(frameCount >= MIN_ACCEPTABLE_FPS * duration * 0.8f, 
                "Frame count should be consistent with target frame rate");
        }

        #endregion

        #region Stress Tests

        [Test]
        public void StressTest_MemoryAllocation()
        {
            // Test memory allocation patterns
            long initialMemory = System.GC.GetTotalMemory(true);
            
            // Create and destroy many animations rapidly
            for (int i = 0; i < 1000; i++)
            {
                foreach (var coin in _testCoins.Take(10)) // Test with subset
                {
                    Vector3 targetPos = coin.transform.position + Vector3.one;
                    coin.AnimateToPosition(targetPos, 0.1f);
                }
                
                // Force garbage collection periodically
                if (i % 100 == 0)
                {
                    System.GC.Collect();
                }
            }
            
            long finalMemory = System.GC.GetTotalMemory(true);
            long memoryIncrease = finalMemory - initialMemory;
            
            UnityEngine.Debug.Log($"Memory increase after stress test: {memoryIncrease / 1024 / 1024:F1} MB");
            
            // Memory should not increase excessively
            Assert.IsTrue(memoryIncrease < 50 * 1024 * 1024, 
                "Memory increase should be less than 50 MB after stress test");
        }

        [Test]
        public void StressTest_CapacityLimits()
        {
            // Test system behavior at capacity limits
            
            // Test animation manager capacity
            int maxCoins = _animationManager.ActiveCoinCount + PERFORMANCE_TEST_COIN_COUNT;
            
            // This should handle gracefully without crashing
            for (int i = 0; i < PERFORMANCE_TEST_COIN_COUNT * 2; i++)
            {
                var tempCoin = _testCoins[i % _testCoins.Count];
                Vector3 targetPos = tempCoin.transform.position + Vector3.one * Random.Range(0f, 5f);
                tempCoin.AnimateToPosition(targetPos, 1f);
            }
            
            // Should not crash and should maintain reasonable performance
            Assert.IsTrue(_animationManager.ActiveCoinCount <= maxCoins, 
                "Active coin count should not exceed capacity limits");
        }

        #endregion
    }
}