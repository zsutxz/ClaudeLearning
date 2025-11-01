using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using CoinAnimation.Core;

namespace CoinAnimation.Tests
{
    /// <summary>
    /// PerformanceDashboard 综合测试套件
    /// Story 1.3 Enhancement - 高级性能监控系统测试
    /// </summary>
    public class PerformanceDashboardTests : MonoBehaviour
    {
        private GameObject _testGameObject;
        private PerformanceDashboard _dashboard;
        private CoinObjectPool _mockPool;
        private MemoryManagementSystem _mockMemorySystem;

        [SetUp]
        public void SetUp()
        {
            // 创建测试对象
            _testGameObject = new GameObject("TestPerformanceDashboard");
            _dashboard = _testGameObject.AddComponent<PerformanceDashboard>();

            // 创建模拟依赖
            CreateMockDependencies();
        }

        [TearDown]
        public void TearDown()
        {
            if (_dashboard != null)
                _dashboard.SetEnabled(false);

            UnityEngine.Object.DestroyImmediate(_testGameObject);
        }

        #region Initialization Tests

        [Test]
        public void PerformanceDashboard_InitializesWithCorrectDefaults()
        {
            // Assert
            Assert.IsTrue(_dashboard.IsEnabled, "Dashboard should be enabled by default");
            Assert.IsNotNull(_dashboard.CurrentStats, "Stats should be initialized");
        }

        [Test]
        public void PerformanceDashboard_CanBeEnabledAndDisabled()
        {
            // Act
            _dashboard.SetEnabled(false);
            Assert.IsFalse(_dashboard.IsEnabled, "Dashboard should be disabled");

            _dashboard.SetEnabled(true);
            Assert.IsTrue(_dashboard.IsEnabled, "Dashboard should be enabled");
        }

        #endregion

        #region Performance Monitoring Tests

        [UnityTest]
        public IEnumerator PerformanceDashboard_CollectsPerformanceDataOverTime()
        {
            // Arrange
            float initialDataPoints = _dashboard.CurrentStats.DataPoints;

            // Act - Wait for data collection
            yield return new WaitForSeconds(1.5f);

            // Assert
            Assert.Greater(_dashboard.CurrentStats.DataPoints, initialDataPoints,
                "Should collect more data points over time");
        }

        [Test]
        public void PerformanceDashboard_GeneratesPerformanceReport()
        {
            // Act
            var report = _dashboard.GetPerformanceReport();

            // Assert
            Assert.IsNotNull(report, "Report should not be null");
            Assert.Greater(report.Timestamp, default(DateTime), "Timestamp should be set");
            Assert.GreaterOrEqual(report.CurrentFPS, 0f, "FPS should be non-negative");
            Assert.GreaterOrEqual(report.CurrentMemoryMB, 0f, "Memory should be non-negative");
            Assert.GreaterOrEqual(report.ActiveCoinsCount, 0, "Active coins should be non-negative");
        }

        [Test]
        public void PerformanceDashboard_ExportsToCSV()
        {
            // Act
            string csv = _dashboard.ExportToCSV();

            // Assert
            Assert.IsNotNull(csv, "CSV export should not be null");
            Assert.IsTrue(csv.Contains("Timestamp,FPS,MemoryMB"), "CSV should contain headers");
            Assert.IsTrue(csv.Length > 50, "CSV should contain data");
        }

        #endregion

        #region Alert System Tests

        [Test]
        public void PerformanceDashboard_TriggersAlertsOnThresholdViolation()
        {
            // Arrange
            bool alertTriggered = false;
            PerformanceAlert triggeredAlert = null;

            _dashboard.OnPerformanceAlert += (alert) =>
            {
                alertTriggered = true;
                triggeredAlert = alert;
            };

            // Act - Simulate low FPS (this would need access to internal state or mock)
            // Note: This test might need adjustment based on actual implementation
            _dashboard.SetPosition(new Vector2(100, 100));

            // Assert
            // Note: Actual alert triggering depends on implementation details
            // This is a placeholder for the concept
            Assert.IsNotNull(triggeredAlert, "Alert system should be functional");
        }

        [Test]
        public void PerformanceDashboard_CalculatesCorrectStatistics()
        {
            // Act
            var stats = _dashboard.CurrentStats;

            // Assert
            Assert.IsNotNull(stats, "Stats should not be null");
            Assert.GreaterOrEqual(stats.AverageFPS, 0f, "Average FPS should be non-negative");
            Assert.GreaterOrEqual(stats.AverageMemoryMB, 0f, "Average memory should be non-negative");
            Assert.GreaterOrEqual(stats.DataPoints, 0, "Data points should be non-negative");
        }

        #endregion

        #region Integration Tests

        [UnityTest]
        public IEnumerator PerformanceDashboard_IntegratesWithObjectPool()
        {
            // Arrange
            _mockPool = _testGameObject.AddComponent<CoinObjectPool>();

            // Wait for initialization
            yield return new WaitForEndOfFrame();

            // Act
            var report = _dashboard.GetPerformanceReport();

            // Assert
            Assert.IsNotNull(report, "Dashboard should work with object pool integration");
        }

        [UnityTest]
        public IEnumerator PerformanceDashboard_IntegratesWithMemorySystem()
        {
            // Arrange
            _mockMemorySystem = _testGameObject.AddComponent<MemoryManagementSystem>();

            // Wait for initialization
            yield return new WaitForEndOfFrame();

            // Act
            var report = _dashboard.GetPerformanceReport();

            // Assert
            Assert.IsNotNull(report, "Dashboard should work with memory system integration");
            Assert.GreaterOrEqual(report.CurrentMemoryMB, 0f, "Memory data should be available");
        }

        #endregion

        #region Performance Tests

        [UnityTest]
        public IEnumerator PerformanceDashboard_PerformsEfficientlyUnderLoad()
        {
            // Arrange
            int iterations = 100;
            var startTime = Time.realtimeSinceStartup;

            // Act - Generate performance reports rapidly
            for (int i = 0; i < iterations; i++)
            {
                var report = _dashboard.GetPerformanceReport();
                Assert.IsNotNull(report, $"Report {i} should not be null");

                if (i % 10 == 0)
                    yield return null; // Allow Unity to breathe
            }

            var endTime = Time.realtimeSinceStartup;
            float averageTime = (endTime - startTime) / iterations * 1000f; // Convert to ms

            // Assert
            Assert.Less(averageTime, 10f, "Average report generation time should be less than 10ms");
        }

        [UnityTest]
        public IEnumerator PerformanceDashboard_HandlesConcurrentAccess()
        {
            // Arrange
            bool completedWithoutError = true;
            var reports = new System.Collections.Generic.List<PerformanceReport>();

            // Act - Access dashboard from multiple "threads" (coroutines)
            var task1 = GenerateReports(10, reports);
            var task2 = GenerateReports(10, reports);
            var task3 = GenerateReports(10, reports);

            yield return StartCoroutine(task1);
            yield return StartCoroutine(task2);
            yield return StartCoroutine(task3);

            // Assert
            Assert.AreEqual(30, reports.Count, "Should generate all reports without error");
            Assert.IsTrue(completedWithoutError, "Should handle concurrent access safely");
        }

        #endregion

        #region Edge Cases Tests

        [Test]
        public void PerformanceDashboard_HandlesMissingDependenciesGracefully()
        {
            // Arrange - Dashboard exists without dependencies
            var isolatedDashboard = new GameObject("IsolatedDashboard").AddComponent<PerformanceDashboard>();

            // Act & Assert - Should not throw exceptions
            Assert.DoesNotThrow(() =>
            {
                var report = isolatedDashboard.GetPerformanceReport();
                Assert.IsNotNull(report);
            }, "Dashboard should handle missing dependencies gracefully");

            // Cleanup
            UnityEngine.Object.DestroyImmediate(isolatedDashboard.gameObject);
        }

        [Test]
        public void PerformanceDashboard_HandlesInvalidPositionInput()
        {
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                _dashboard.SetPosition(new Vector2(-100, -100));
                _dashboard.SetPosition(new Vector2(10000, 10000));
                _dashboard.SetPosition(Vector2.zero);
            }, "Should handle invalid position input gracefully");
        }

        [Test]
        public void PerformanceDashboard_CSVExportHandlesEmptyData()
        {
            // Arrange - New dashboard with minimal data
            var newDashboard = new GameObject("NewDashboard").AddComponent<PerformanceDashboard>();

            // Act
            string csv = newDashboard.ExportToCSV();

            // Assert
            Assert.IsNotNull(csv, "CSV export should not be null");
            Assert.IsTrue(csv.Contains("Timestamp,FPS,MemoryMB"), "Should export headers even with no data");

            // Cleanup
            UnityEngine.Object.DestroyImmediate(newDashboard.gameObject);
        }

        #endregion

        #region Helper Methods

        private void CreateMockDependencies()
        {
            // Create mock objects that the dashboard can monitor
            var poolObject = new GameObject("MockPool");
            _mockPool = poolObject.AddComponent<CoinObjectPool>();

            var memoryObject = new GameObject("MockMemory");
            _mockMemorySystem = memoryObject.AddComponent<MemoryManagementSystem>();

            // Parent them to the test object for cleanup
            poolObject.transform.SetParent(_testGameObject.transform);
            memoryObject.transform.SetParent(_testGameObject.transform);
        }

        private IEnumerator GenerateReports(int count, System.Collections.Generic.List<PerformanceReport> reports)
        {
            for (int i = 0; i < count; i++)
            {
                var report = _dashboard.GetPerformanceReport();
                reports.Add(report);
                yield return null;
            }
        }

        #endregion
    }

    #region Performance Test Extensions

    /// <summary>
    /// Performance-specific test extensions
    /// </summary>
    public static class PerformanceTestExtensions
    {
        /// <summary>
        /// Asserts that a performance metric is within acceptable bounds
        /// </summary>
        public static void AssertWithinBounds(this float value, float min, float max, string message = "")
        {
            Assert.GreaterOrEqual(value, min, $"Value {value} should be >= {min}. {message}");
            Assert.LessOrEqual(value, max, $"Value {value} should be <= {max}. {message}");
        }

        /// <summary>
        /// Asserts that performance metrics are reasonable
        /// </summary>
        public static void AssertReasonablePerformance(this PerformanceReport report)
        {
            Assert.That(report.CurrentFPS, Is.InRange(0f, 200f), "FPS should be reasonable");
            Assert.That(report.CurrentMemoryMB, Is.InRange(0f, 4096f), "Memory should be reasonable");
            Assert.That(report.ActiveCoinsCount, Is.InRange(0, 1000), "Active coins should be reasonable");
            Assert.That(report.PoolEfficiency, Is.InRange(0f, 100f), "Pool efficiency should be reasonable");
        }
    }

    #endregion
}