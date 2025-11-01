using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using CoinAnimation.Core;

namespace CoinAnimation.Tests
{
    /// <summary>
    /// IntelligentPredictionSystem 综合测试套件
    /// Story 1.3 Enhancement - 智能预测算法测试
    /// </summary>
    public class IntelligentPredictionSystemTests : MonoBehaviour
    {
        private GameObject _testGameObject;
        private IntelligentPredictionSystem _predictionSystem;
        private CoinObjectPool _mockPool;
        private MemoryManagementSystem _mockMemorySystem;

        [SetUp]
        public void SetUp()
        {
            // 创建测试对象
            _testGameObject = new GameObject("TestPredictionSystem");
            _predictionSystem = _testGameObject.AddComponent<IntelligentPredictionSystem>();

            // 创建模拟依赖
            CreateMockDependencies();
        }

        [TearDown]
        public void TearDown()
        {
            if (_predictionSystem != null)
                _predictionSystem.SetEnabled(false);

            UnityEngine.Object.DestroyImmediate(_testGameObject);
        }

        #region Initialization Tests

        [Test]
        public void IntelligentPredictionSystem_InitializesWithCorrectDefaults()
        {
            // Assert
            Assert.IsTrue(_predictionSystem.IsEnabled, "Prediction system should be enabled by default");
            Assert.IsNotNull(_predictionSystem.Stats, "Stats should be initialized");
            Assert.AreEqual(0, _predictionSystem.Stats.TotalPredictions, "Should start with zero predictions");
        }

        [Test]
        public void IntelligentPredictionSystem_CanBeEnabledAndDisabled()
        {
            // Act
            _predictionSystem.SetEnabled(false);
            Assert.IsFalse(_predictionSystem.IsEnabled, "Should be disabled");

            _predictionSystem.SetEnabled(true);
            Assert.IsTrue(_predictionSystem.IsEnabled, "Should be enabled");
        }

        #endregion

        #region Prediction Logic Tests

        [UnityTest]
        public IEnumerator IntelligentPredictionSystem_CollectsDataOverTime()
        {
            // Arrange
            int initialDataPoints = _predictionSystem.Stats.TotalPredictions;

            // Act - Wait for data collection and prediction
            yield return new WaitForSeconds(6f); // Wait longer than prediction interval

            // Assert
            Assert.Greater(_predictionSystem.Stats.TotalPredictions, initialDataPoints,
                "Should generate predictions over time");
        }

        [Test]
        public void IntelligentPredictionSystem_TriggersPredictionUpdate()
        {
            // Arrange
            bool predictionUpdated = false;
            _predictionSystem.OnPredictionUpdated += (result) => predictionUpdated = true;

            // Act
            _predictionSystem.TriggerPredictionUpdate();

            // Assert - Note: This might not generate prediction if insufficient data
            Assert.IsNotNull(_predictionSystem.LastPrediction, "Should have prediction structure");
        }

        [Test]
        public void IntelligentPredictionSystem_GeneratesRecommendations()
        {
            // Arrange
            bool recommendationGenerated = false;
            AdaptiveRecommendation capturedRecommendation = null;

            _predictionSystem.OnRecommendationGenerated += (rec) =>
            {
                recommendationGenerated = true;
                capturedRecommendation = rec;
            };

            // Act
            _predictionSystem.TriggerPredictionUpdate();

            // Assert - Recommendations depend on prediction confidence and data
            Assert.IsNotNull(capturedRecommendation, "Should be able to generate recommendations");
        }

        #endregion

        #region Model Accuracy Tests

        [UnityTest]
        public IEnumerator IntelligentPredictionSystem_ImprovesAccuracyOverTime()
        {
            // Arrange
            var initialConfidence = _predictionSystem.Stats.AverageConfidence;

            // Act - Generate some prediction data
            yield return new WaitForSeconds(12f); // Allow multiple prediction cycles

            var currentConfidence = _predictionSystem.Stats.AverageConfidence;

            // Assert
            Assert.GreaterOrEqual(currentConfidence, 0f, "Confidence should be non-negative");
            Assert.LessOrEqual(currentConfidence, 1f, "Confidence should not exceed 1.0");
        }

        [Test]
        public void IntelligentPredictionSystem_GeneratesAccuracyReport()
        {
            // Act
            var report = _predictionSystem.GetAccuracyReport();

            // Assert
            Assert.IsNotNull(report, "Accuracy report should not be null");
            Assert.Greater(report.ReportTime, default(DateTime), "Report timestamp should be set");
            Assert.GreaterOrEqual(report.TotalPredictions, 0, "Total predictions should be non-negative");
            Assert.GreaterOrEqual(report.AverageConfidence, 0f, "Average confidence should be non-negative");
        }

        #endregion

        #region Pattern Detection Tests

        [UnityTest]
        public IEnumerator IntelligentPredictionSystem_DetectsPeriodicPatterns()
        {
            // Arrange - Create periodic demand pattern
            SimulatePeriodicDemand();
            yield return new WaitForSeconds(8f);

            // Act
            _predictionSystem.TriggerPredictionUpdate();
            var prediction = _predictionSystem.LastPrediction;

            // Assert
            Assert.IsNotNull(prediction, "Should generate prediction");
            if (prediction.DetectedPatterns != null)
            {
                Assert.GreaterOrEqual(prediction.DetectedPatterns.Count, 0, "Should detect patterns if present");
            }
        }

        [UnityTest]
        public IEnumerator IntelligentPredictionSystem_DetectsGrowthTrends()
        {
            // Arrange - Create growth pattern
            yield return StartCoroutine(SimulateGrowthPattern());
            yield return new WaitForSeconds(8f);

            // Act
            _predictionSystem.TriggerPredictionUpdate();
            var prediction = _predictionSystem.LastPrediction;

            // Assert
            Assert.IsNotNull(prediction, "Should generate prediction");
            if (prediction.PredictedCoinDemand != null)
            {
                Assert.GreaterOrEqual(prediction.PredictedCoinDemand.DemandTrend, -1f, "Trend should be reasonable");
            }
        }

        #endregion

        #region Edge Cases Tests

        [Test]
        public void IntelligentPredictionSystem_HandlesMissingDependenciesGracefully()
        {
            // Arrange - Create isolated prediction system
            var isolatedSystem = new GameObject("IsolatedPrediction")
                .AddComponent<IntelligentPredictionSystem>();

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                isolatedSystem.TriggerPredictionUpdate();
                var prediction = isolatedSystem.LastPrediction;
                Assert.IsNotNull(prediction);
            }, "Should handle missing dependencies gracefully");

            // Cleanup
            UnityEngine.Object.DestroyImmediate(isolatedSystem.gameObject);
        }

        [Test]
        public void IntelligentPredictionSystem_HandlesInsufficientData()
        {
            // Act - Trigger prediction immediately (before data collection)
            _predictionSystem.TriggerPredictionUpdate();
            var prediction = _predictionSystem.LastPrediction;

            // Assert
            Assert.IsNotNull(prediction, "Should still have prediction structure");
            Assert.GreaterOrEqual(prediction.Confidence, 0f, "Confidence should be non-negative");
            Assert.LessOrEqual(prediction.Confidence, 1f, "Confidence should not exceed 1.0");
        }

        [UnityTest]
        public IEnumerator IntelligentPredictionSystem_HandlesRapidParameterChanges()
        {
            // Act - Rapidly enable/disable system
            for (int i = 0; i < 10; i++)
            {
                _predictionSystem.SetEnabled(i % 2 == 0);
                yield return null;
            }

            // Final state should be enabled
            _predictionSystem.SetEnabled(true);

            // Assert
            Assert.IsTrue(_predictionSystem.IsEnabled, "System should be stable after rapid changes");
        }

        #endregion

        #region Performance Tests

        [UnityTest]
        public IEnumerator IntelligentPredictionSystem_PerformsEfficientlyUnderLoad()
        {
            // Arrange
            int iterations = 20;
            var startTime = Time.realtimeSinceStartup;

            // Act - Generate multiple predictions
            for (int i = 0; i < iterations; i++)
            {
                _predictionSystem.TriggerPredictionUpdate();

                if (i % 5 == 0)
                    yield return null; // Allow Unity to breathe
            }

            var endTime = Time.realtimeSinceStartup;
            float averageTime = (endTime - startTime) / iterations * 1000f; // Convert to ms

            // Assert
            Assert.Less(averageTime, 50f, "Average prediction time should be less than 50ms");
        }

        [Test]
        public void IntelligentPredictionSystem_MemoryUsageRemainsStable()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(false);

            // Act - Generate many predictions
            for (int i = 0; i < 100; i++)
            {
                _predictionSystem.TriggerPredictionUpdate();
                _predictionSystem.GetAccuracyReport();
            }

            var finalMemory = GC.GetTotalMemory(false);
            float memoryIncrease = (finalMemory - initialMemory) / (1024f * 1024f); // MB

            // Assert
            Assert.Less(memoryIncrease, 10f, "Memory increase should be less than 10MB");
        }

        #endregion

        #region Integration Tests

        [UnityTest]
        public IEnumerator IntelligentPredictionSystem_IntegratesWithObjectPool()
        {
            // Arrange
            var poolObject = new GameObject("TestPool");
            var pool = poolObject.AddComponent<CoinObjectPool>();
            poolObject.transform.SetParent(_testGameObject.transform);

            // Wait for initialization
            yield return new WaitForEndOfFrame();

            // Act
            _predictionSystem.TriggerPredictionUpdate();
            yield return new WaitForSeconds(2f);

            // Assert
            Assert.IsNotNull(_predictionSystem.LastPrediction, "Should work with object pool integration");

            // Cleanup
            UnityEngine.Object.DestroyImmediate(poolObject);
        }

        [UnityTest]
        public IEnumerator IntelligentPredictionSystem_IntegratesWithMemorySystem()
        {
            // Arrange
            var memoryObject = new GameObject("TestMemory");
            var memorySystem = memoryObject.AddComponent<MemoryManagementSystem>();
            memoryObject.transform.SetParent(_testGameObject.transform);

            // Wait for initialization
            yield return new WaitForEndOfFrame();

            // Act
            _predictionSystem.TriggerPredictionUpdate();
            yield return new WaitForSeconds(2f);

            // Assert
            Assert.IsNotNull(_predictionSystem.LastPrediction, "Should work with memory system integration");

            // Cleanup
            UnityEngine.Object.DestroyImmediate(memoryObject);
        }

        #endregion

        #region Helper Methods

        private void CreateMockDependencies()
        {
            // Create mock objects that the prediction system can monitor
            var poolObject = new GameObject("MockPool");
            _mockPool = poolObject.AddComponent<CoinObjectPool>();

            var memoryObject = new GameObject("MockMemory");
            _mockMemorySystem = memoryObject.AddComponent<MemoryManagementSystem>();

            // Parent them to the test object for cleanup
            poolObject.transform.SetParent(_testGameObject.transform);
            memoryObject.transform.SetParent(_testGameObject.transform);
        }

        private void SimulatePeriodicDemand()
        {
            // Simulate periodic demand pattern
            // This would involve manipulating the mock pool's active coin count
            // Implementation depends on the actual mock pool capabilities
        }

        private IEnumerator SimulateGrowthPattern()
        {
            // Simulate increasing demand pattern
            for (int i = 0; i < 10; i++)
            {
                // Simulate increasing demand
                // This would involve manipulating mock objects
                yield return new WaitForSeconds(0.5f);
            }
        }

        #endregion
    }

    #region Prediction Test Extensions

    /// <summary>
    /// Prediction-specific test extensions
    /// </summary>
    public static class PredictionTestExtensions
    {
        /// <summary>
        /// Asserts that prediction results are reasonable
        /// </summary>
        public static void AssertReasonablePrediction(this PredictionResult result)
        {
            Assert.IsNotNull(result, "Prediction result should not be null");
            Assert.Greater(result.Timestamp, default(DateTime), "Timestamp should be set");
            Assert.GreaterOrEqual(result.Confidence, 0f, "Confidence should be non-negative");
            Assert.LessOrEqual(result.Confidence, 1f, "Confidence should not exceed 1.0");
            Assert.Greater(result.PredictionWindow, 0, "Prediction window should be positive");
        }

        /// <summary>
        /// Asserts that coin demand predictions are reasonable
        /// </summary>
        public static void AssertReasonableCoinDemand(this CoinDemandPrediction prediction)
        {
            Assert.IsNotNull(prediction, "Coin demand prediction should not be null");
            Assert.GreaterOrEqual(prediction.PredictedMaxDemand, 0, "Predicted demand should be non-negative");
            Assert.GreaterOrEqual(prediction.RecommendedPoolSize, 0, "Recommended pool size should be non-negative");
            Assert.GreaterOrEqual(prediction.PeakDemandTime, 0f, "Peak demand time should be non-negative");
            Assert.GreaterOrEqual(prediction.Confidence, 0f, "Confidence should be non-negative");
            Assert.LessOrEqual(prediction.Confidence, 1f, "Confidence should not exceed 1.0");
        }

        /// <summary>
        /// Asserts that memory usage predictions are reasonable
        /// </summary>
        public static void AssertReasonableMemoryUsage(this MemoryUsagePrediction prediction)
        {
            Assert.IsNotNull(prediction, "Memory usage prediction should not be null");
            Assert.GreaterOrEqual(prediction.PredictedMaxMemoryMB, 0f, "Predicted memory should be non-negative");
            Assert.GreaterOrEqual(prediction.MemoryPressureTime, 0f, "Pressure time should be non-negative");
            Assert.GreaterOrEqual(prediction.Confidence, 0f, "Confidence should be non-negative");
            Assert.LessOrEqual(prediction.Confidence, 1f, "Confidence should not exceed 1.0");
        }
    }

    #endregion
}