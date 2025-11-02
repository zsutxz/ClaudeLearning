using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using CoinAnimation.Core.Compatibility;
using System;

namespace CoinAnimation.Tests.Compatibility
{
    /// <summary>
    /// 跨平台一致性验证器测试套件
    /// Cross-Platform Consistency Validator Test Suite
    /// </summary>
    [TestFixture]
    public class CrossPlatformConsistencyValidatorTests
    {
        private CrossPlatformConsistencyValidator validator;
        private string testReportPath;

        [SetUp]
        public void SetUp()
        {
            // 创建跨平台一致性验证器实例
            validator = new CrossPlatformConsistencyValidator();

            // 设置测试报告路径
            testReportPath = Path.Combine(Application.temporaryCachePath, "cross_platform_test_report.json");

            // 清理之前的测试文件
            if (File.Exists(testReportPath))
            {
                File.Delete(testReportPath);
            }
        }

        [TearDown]
        public void TearDown()
        {
            // 清理测试文件
            if (File.Exists(testReportPath))
            {
                File.Delete(testReportPath);
            }
        }

        [Test]
        public void Validator_Initialization_ShouldCreateValidInstance()
        {
            // Arrange & Act
            validator.Initialize();

            // Assert
            Assert.IsNotNull(validator, "跨平台一致性验证器应该成功创建");

            var report = validator.GetConsistencyReport();
            Assert.IsNotNull(report, "一致性报告应该存在");
            Assert.IsNotNull(report.functionalTests, "功能测试列表应该存在");
            Assert.IsNotNull(report.performanceTests, "性能测试列表应该存在");
            Assert.IsNotNull(report.visualTests, "视觉测试列表应该存在");
        }

        [Test]
        public void Initialization_ShouldSetCorrectPlatformAndVersion()
        {
            // Arrange & Act
            validator.Initialize();

            // Assert
            var report = validator.GetConsistencyReport();
            Assert.IsTrue(report.validationDate != default(DateTime), "验证日期应该被设置");
            Assert.IsFalse(string.IsNullOrEmpty(report.currentPlatform), "当前平台应该被检测到");
            Assert.IsFalse(string.IsNullOrEmpty(report.currentUnityVersion), "Unity版本应该被检测到");
        }

        [UnityTest]
        public IEnumerator CompleteConsistencyValidation_ShouldRunSuccessfully()
        {
            // Arrange
            validator.Initialize();
            var initialReport = validator.GetConsistencyReport();
            var initialFunctionalTests = initialReport.functionalTests.Count;
            var initialPerformanceTests = initialReport.performanceTests.Count;
            var initialVisualTests = initialReport.visualTests.Count;

            // Act
            yield return validator.RunCompleteConsistencyValidation();

            // Assert
            var finalReport = validator.GetConsistencyReport();
            Assert.IsTrue(finalReport.validationDate != default(DateTime), "验证日期应该被更新");
            Assert.IsTrue(finalReport.functionalTests.Count >= initialFunctionalTests,
                "应该运行功能一致性测试");
            Assert.IsTrue(finalReport.performanceTests.Count >= initialPerformanceTests,
                "应该运行性能基准测试");
            Assert.IsTrue(finalReport.visualTests.Count >= initialVisualTests,
                "应该运行视觉效果一致性测试");
            Assert.IsTrue(finalReport.overallConsistencyScore >= 0, "总体一致性分数应该有效");
        }

        [UnityTest]
        public IEnumerator FunctionalConsistencyTests_ShouldValidateBasicFunctionality()
        {
            // Arrange
            validator.Initialize();

            // Act
            yield return validator.RunCompleteConsistencyValidation();

            // Assert
            var report = validator.GetConsistencyReport();

            // 检查基础功能测试
            var basicFunctionalityTest = report.functionalTests.Find(t => t.testName.Contains("基础功能"));
            Assert.IsNotNull(basicFunctionalityTest, "应该包含基础功能测试");
            Assert.IsNotNull(basicFunctionalityTest.testSteps, "基础功能测试应该包含测试步骤");
            Assert.IsTrue(basicFunctionalityTest.testSteps.Count > 0, "基础功能测试应该有测试步骤");

            // 检查动画系统功能测试
            var animationTest = report.functionalTests.Find(t => t.testName.Contains("动画系统"));
            Assert.IsNotNull(animationTest, "应该包含动画系统功能测试");
            Assert.IsNotNull(animationTest.executionTime, "动画测试应该记录执行时间");
        }

        [UnityTest]
        public IEnumerator PerformanceBenchmarkTests_ShouldMeasurePerformanceMetrics()
        {
            // Arrange
            validator.Initialize();

            // Act
            yield return validator.RunCompleteConsistencyValidation();

            // Assert
            var report = validator.GetConsistencyReport();

            // 检查帧率基准测试
            var fpsBenchmark = report.performanceTests.Find(t => t.benchmarkName.Contains("帧率"));
            Assert.IsNotNull(fpsBenchmark, "应该包含帧率基准测试");
            Assert.IsTrue(fpsBenchmark.currentValue > 0, "帧率应该大于0");
            Assert.IsTrue(fpsBenchmark.baselineValue > 0, "基准帧率应该大于0");
            Assert.IsNotNull(fpsBenchmark.tolerance, "容差应该被设置");

            // 检查内存使用基准测试
            var memoryBenchmark = report.performanceTests.Find(t => t.benchmarkName.Contains("内存"));
            Assert.IsNotNull(memoryBenchmark, "应该包含内存使用基准测试");
            Assert.IsTrue(memoryBenchmark.currentValue >= 0, "内存使用应该非负");
        }

        [UnityTest]
        public IEnumerator VisualEffectConsistencyTests_ShouldValidateVisualProperties()
        {
            // Arrange
            validator.Initialize();

            // Act
            yield return validator.RunCompleteConsistencyValidation();

            // Assert
            var report = validator.GetConsistencyReport();

            // 检查位置一致性测试
            var positionTest = report.visualTests.Find(t => t.testType == "Position");
            Assert.IsNotNull(positionTest, "应该包含位置一致性测试");
            Assert.IsNotNull(positionTest.expectedPosition, "应该有预期位置");
            Assert.IsNotNull(positionTest.actualPosition, "应该有实际位置");
            Assert.IsTrue(positionTest.positionDifference >= 0, "位置差异应该非负");

            // 检查旋转一致性测试
            var rotationTest = report.visualTests.Find(t => t.testType == "Rotation");
            Assert.IsNotNull(rotationTest, "应该包含旋转一致性测试");
            Assert.IsNotNull(rotationTest.expectedRotation, "应该有预期旋转");
            Assert.IsNotNull(rotationTest.actualRotation, "应该有实际旋转");
            Assert.IsTrue(rotationTest.rotationDifference >= 0, "旋转差异应该非负");

            // 检查缩放一致性测试
            var scaleTest = report.visualTests.Find(t => t.testType == "Scale");
            Assert.IsNotNull(scaleTest, "应该包含缩放一致性测试");
            Assert.IsNotNull(scaleTest.expectedScale, "应该有预期缩放");
            Assert.IsNotNull(scaleTest.actualScale, "应该有实际缩放");
            Assert.IsTrue(scaleTest.scaleDifference >= 0, "缩放差异应该非负");
        }

        [UnityTest]
        public IEnumerator CrossPlatformResultsAnalysis_ShouldGenerateConsistencyScore()
        {
            // Arrange
            validator.Initialize();

            // Act
            yield return validator.RunCompleteConsistencyValidation();

            // Assert
            var report = validator.GetConsistencyReport();
            Assert.IsTrue(report.totalTestsRun > 0, "应该运行了测试");
            Assert.IsTrue(report.passedTests >= 0, "通过测试数量应该非负");
            Assert.IsTrue(report.failedTests >= 0, "失败测试数量应该非负");
            Assert.IsTrue(report.passRate >= 0 && report.passRate <= 100, "通过率应该在0-100%之间");
            Assert.IsTrue(report.overallConsistencyScore >= 0 && report.overallConsistencyScore <= 100,
                "总体一致性分数应该在0-100%之间");
        }

        [Test]
        public void ReportExport_ShouldSaveToFile()
        {
            // Arrange
            validator.Initialize();

            // Act
            validator.ExportReportToFile(testReportPath);

            // Assert
            Assert.IsTrue(File.Exists(testReportPath), "报告文件应该被创建");

            var fileContent = File.ReadAllText(testReportPath);
            Assert.IsFalse(string.IsNullOrEmpty(fileContent), "报告文件应该有内容");
            Assert.IsTrue(fileContent.Contains("validationDate"), "报告应该包含验证日期信息");
        }

        [Test]
        public void ConsistencyReport_ShouldHaveValidStructure()
        {
            // Arrange
            validator.Initialize();

            // Act
            var report = validator.GetConsistencyReport();

            // Assert
            Assert.IsNotNull(report.functionalTests, "功能测试列表应该存在");
            Assert.IsNotNull(report.performanceTests, "性能测试列表应该存在");
            Assert.IsNotNull(report.visualTests, "视觉测试列表应该存在");
            Assert.IsNotNull(report.consistencyIssues, "一致性问题列表应该存在");
            Assert.IsNotNull(report.recommendations, "建议列表应该存在");
            Assert.IsNotNull(report.platformResults, "平台结果字典应该存在");
        }

        [UnityTest]
        public IEnumerator MultipleValidationRuns_ShouldProduceConsistentResults()
        {
            // Arrange
            validator.Initialize();

            // Act - 运行两次验证
            yield return validator.RunCompleteConsistencyValidation();
            var firstReport = validator.GetConsistencyReport();

            // 重置并再次运行
            validator.Initialize();
            yield return validator.RunCompleteConsistencyValidation();
            var secondReport = validator.GetConsistencyReport();

            // Assert
            Assert.AreEqual(firstReport.currentPlatform, secondReport.currentPlatform,
                "平台信息应该一致");
            Assert.AreEqual(firstReport.currentUnityVersion, secondReport.currentUnityVersion,
                "Unity版本应该一致");

            // 测试结果数量应该大致相同
            var functionalDiff = Mathf.Abs(firstReport.functionalTests.Count - secondReport.functionalTests.Count);
            var performanceDiff = Mathf.Abs(firstReport.performanceTests.Count - secondReport.performanceTests.Count);
            var visualDiff = Mathf.Abs(firstReport.visualTests.Count - secondReport.visualTests.Count);

            Assert.IsTrue(functionalDiff <= 1, "功能测试数量应该一致");
            Assert.IsTrue(performanceDiff <= 1, "性能测试数量应该一致");
            Assert.IsTrue(visualDiff <= 1, "视觉测试数量应该一致");
        }

        [UnityTest]
        public IEnumerator ErrorHandling_ShouldGracefullyHandleExceptions()
        {
            // Arrange
            validator.Initialize();

            // Act - 测试应该能优雅地处理异常情况
            yield return validator.RunCompleteConsistencyValidation();

            // Assert
            var report = validator.GetConsistencyReport();

            // 即使某些测试失败，报告也应该有效
            Assert.IsNotNull(report, "即使有异常，报告也应该存在");
            Assert.IsTrue(report.totalTestsRun >= 0, "总测试数应该有效");
            Assert.IsTrue(report.passedTests >= 0, "通过测试数应该有效");
            Assert.IsTrue(report.failedTests >= 0, "失败测试数应该有效");
        }

        [UnityTest]
        public IEnumerator PlatformSpecificValidation_ShouldDetectCurrentPlatform()
        {
            // Arrange
            validator.Initialize();

            // Act
            yield return validator.RunCompleteConsistencyValidation();

            // Assert
            var report = validator.GetConsistencyReport();
            var currentPlatform = Application.platform.ToString();

            Assert.AreEqual(currentPlatform, report.currentPlatform,
                "应该正确检测当前平台");

            // 检查环境信息是否包含平台信息
            var platformTest = report.functionalTests.Find(t => t.environmentInfo.Contains(currentPlatform));
            Assert.IsNotNull(platformTest, "测试应该包含当前平台的环境信息");
        }

        [UnityTest]
        public IEnumerator PerformanceMetrics_ShouldBeRealistic()
        {
            // Arrange
            validator.Initialize();

            // Act
            yield return validator.RunCompleteConsistencyValidation();

            // Assert
            var report = validator.GetConsistencyReport();

            foreach (var perfTest in report.performanceTests)
            {
                // 性能指标应该在合理范围内
                if (perfTest.metricType == "FPS")
                {
                    Assert.IsTrue(perfTest.currentValue >= 0 && perfTest.currentValue <= 200,
                        $"FPS应该在合理范围内 (0-200): {perfTest.currentValue}");
                }
                else if (perfTest.metricType == "MB")
                {
                    Assert.IsTrue(perfTest.currentValue >= 0 && perfTest.currentValue <= 500,
                        $"内存使用应该在合理范围内 (0-500MB): {perfTest.currentValue}");
                }
                else if (perfTest.metricType == "Count")
                {
                    Assert.IsTrue(perfTest.currentValue >= 0,
                        $"Draw Call数量应该非负: {perfTest.currentValue}");
                }
            }
        }

        [UnityTest]
        public IEnumerator VisualPrecisionTests_ShouldMeetToleranceRequirements()
        {
            // Arrange
            validator.Initialize();

            // Act
            yield return validator.RunCompleteConsistencyValidation();

            // Assert
            var report = validator.GetConsistencyReport();

            foreach (var visualTest in report.visualTests)
            {
                // 检查测试是否在容差范围内
                switch (visualTest.testType)
                {
                    case "Position":
                        Assert.IsTrue(visualTest.positionDifference <= 0.01f,
                            $"位置差异应该在容差范围内: {visualTest.positionDifference}");
                        break;
                    case "Rotation":
                        Assert.IsTrue(visualTest.rotationDifference <= 1f,
                            $"旋转差异应该在容差范围内: {visualTest.rotationDifference}");
                        break;
                    case "Scale":
                        Assert.IsTrue(visualTest.scaleDifference <= 0.01f,
                            $"缩放差异应该在容差范围内: {visualTest.scaleDifference}");
                        break;
                    case "Color":
                        Assert.IsTrue(visualTest.colorDifference <= 0.02f,
                            $"颜色差异应该在容差范围内: {visualTest.colorDifference}");
                        break;
                }
            }
        }

        [Test]
        public void ReportGeneration_ShouldIncludeAllRequiredSections()
        {
            // Arrange
            validator.Initialize();

            // Act
            var report = validator.GetConsistencyReport();

            // Assert
            // 检查报告是否包含所有必需的部分
            Assert.IsTrue(report.validationDate != default(DateTime), "应该包含验证日期");
            Assert.IsFalse(string.IsNullOrEmpty(report.currentPlatform), "应该包含当前平台");
            Assert.IsFalse(string.IsNullOrEmpty(report.currentUnityVersion), "应该包含Unity版本");
            Assert.IsTrue(report.functionalTests.Count > 0, "应该包含功能测试结果");
            Assert.IsTrue(report.performanceTests.Count > 0, "应该包含性能测试结果");
            Assert.IsTrue(report.visualTests.Count > 0, "应该包含视觉测试结果");
            Assert.IsTrue(report.totalTestsRun > 0, "应该有总测试数统计");
            Assert.IsTrue(report.overallConsistencyScore >= 0, "应该有总体一致性分数");
        }
    }
}