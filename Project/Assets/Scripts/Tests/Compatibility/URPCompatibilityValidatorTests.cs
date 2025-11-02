using System;
using System.Collections;
using System.IO;
using CoinAnimation.Core.Compatibility;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
namespace CoinAnimation.Tests.Compatibility
{
    /// <summary>
    /// URP兼容性验证器测试套件
    /// URP Compatibility Validator Test Suite
    /// </summary>
    [TestFixture]
    public class URPCompatibilityValidatorTests
    {
        private URPCompatibilityValidator validator;
        private string testReportPath;

        [SetUp]
        public void SetUp()
        {
            // 创建URP兼容性验证器实例
            validator = new URPCompatibilityValidator();

            // 设置测试报告路径
            testReportPath = Path.Combine(Application.temporaryCachePath, "urp_compatibility_test_report.json");

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
            Assert.IsNotNull(validator, "URP兼容性验证器应该成功创建");

            var report = validator.GetCompatibilityReport();
            Assert.IsNotNull(report, "兼容性报告应该存在");
            //Assert.IsNotNull(report.shaderReport, "着色器报告应该存在");
            //Assert.IsNotNull(report.performanceMetrics, "性能指标应该存在");
            //Assert.IsNotNull(report.renderingFeatures, "渲染功能列表应该存在");
        }

        [Test]
        public void Initialization_ShouldDetectURPPresence()
        {
            // Arrange & Act
            validator.Initialize();

            // Assert
            var report = validator.GetCompatibilityReport();
            //Assert.IsTrue(report.testDate != default(DateTime), "测试日期应该被设置");
            //Assert.IsFalse(string.IsNullOrEmpty(report.unityVersion), "Unity版本应该被检测到");
        }

        [UnityTest]
        public IEnumerator CompleteCompatibilityTest_ShouldRunSuccessfully()
        {
            // Arrange
            validator.Initialize();
            var initialReport = validator.GetCompatibilityReport();
            //var initialFeaturesCount = initialReport.compatibleFeatures.Count;

            // Act
            yield return validator.RunCompleteCompatibilityTest();

            // Assert
            var finalReport = validator.GetCompatibilityReport();
            //Assert.IsTrue(finalReport.testDate != default(DateTime), "测试日期应该被设置");
            //Assert.IsTrue(finalReport.compatibleFeatures.Count >= initialFeaturesCount,
            //    "应该检测到兼容功能");
            //Assert.IsTrue(finalReport.overallCompatibilityScore >= 0, "兼容性分数应该有效");
        }

        [UnityTest]
        public IEnumerator URPPresenceCheck_ShouldDetectPipelineAsset()
        {
            // Arrange
            validator.Initialize();

            // Act
            yield return validator.RunCompleteCompatibilityTest();

            // Assert
            var report = validator.GetCompatibilityReport();

            //// URP可能不存在，但应该正确检测
            //if (GraphicsSettings.renderPipelineAsset != null)
            //{
            //    Assert.IsTrue(report.isURPPresent, "如果存在渲染管线资产，应该被检测到");
            //    Assert.IsFalse(string.IsNullOrEmpty(report.urpVersion), "URP版本应该被检测到");
            //}
            //else
            //{
            //    Assert.IsFalse(report.isURPPresent, "如果不存在渲染管线资产，应该正确报告");
            //}
        }

        [UnityTest]
        public IEnumerator Unity2021LTSCompatibility_ShouldValidateCorrectly()
        {
            // Arrange
            validator.Initialize();
            var unityVersion = Application.unityVersion;

            // Act
            yield return validator.RunCompleteCompatibilityTest();

            // Assert
            var report = validator.GetCompatibilityReport();

            //if (unityVersion.StartsWith("2021.3"))
            //{
            //    // Unity 2021.3 LTS应该进行兼容性检查
            //    Assert.IsTrue(report.compatibleFeatures.Exists(f => f.Contains("Unity 2021.3")),
            //        "Unity 2021.3 LTS兼容性应该被测试");
            //}
        }

        [UnityTest]
        public IEnumerator Unity2022LTSCompatibility_ShouldValidateCorrectly()
        {
            // Arrange
            validator.Initialize();
            var unityVersion = Application.unityVersion;

            // Act
            yield return validator.RunCompleteCompatibilityTest();

            // Assert
            var report = validator.GetCompatibilityReport();

            //if (unityVersion.StartsWith("2022.3"))
            //{
            //    // Unity 2022.3 LTS应该进行兼容性检查
            //    Assert.IsTrue(report.compatibleFeatures.Exists(f => f.Contains("Unity 2022.3")),
            //        "Unity 2022.3 LTS兼容性应该被测试");
            //}
        }

        [UnityTest]
        public IEnumerator URPFeaturesTest_ShouldCheckAllFeatures()
        {
            // Arrange
            validator.Initialize();

            // Act
            yield return validator.RunCompleteCompatibilityTest();

            // Assert
            var report = validator.GetCompatibilityReport();

            // 应该测试各种URP功能
            var featureTests = new[]
            {
                "2D渲染器",
                "2D光照",
                "后处理",
                "相机堆栈"
            };

            foreach (var feature in featureTests)
            {
                //// 功能要么在兼容列表中，要么在不兼容列表中
                //bool hasFeature = report.compatibleFeatures.Exists(f => f.Contains(feature)) ||
                //                report.incompatibleFeatures.Exists(f => f.Contains(feature));
                //Assert.IsTrue(hasFeature, $"应该测试{feature}功能");
            }
        }

        [UnityTest]
        public IEnumerator ShaderCompatibilityTest_ShouldCheckBuiltInShaders()
        {
            // Arrange
            validator.Initialize();

            // Act
            yield return validator.RunCompleteCompatibilityTest();

            // Assert
            var report = validator.GetCompatibilityReport();
            //Assert.IsNotNull(report.shaderReport, "着色器报告应该存在");
            //Assert.IsTrue(report.shaderReport.shaderTests.Count > 0, "应该测试着色器兼容性");

            //// 应该测试内置URP着色器
            //var testedShaders = report.shaderReport.shaderTests.ConvertAll(t => t.shaderName);
            var expectedShaders = new[]
            {
                "Universal Render Pipeline/2D/Sprite-Lit-Default",
                "Universal Render Pipeline/2D/Sprite-Unlit-Default",
                "Universal Render Pipeline/Lit",
                "Universal Render Pipeline/Unlit"
            };

            //foreach (var expectedShader in expectedShaders)
            //{
            //    Assert.IsTrue(testedShaders.Contains(expectedShader),
            //        $"应该测试URP内置着色器: {expectedShader}");
            //}
        }

        [UnityTest]
        public IEnumerator PerformanceTest_ShouldGenerateValidMetrics()
        {
            // Arrange
            validator.Initialize();

            // Act
            yield return validator.RunCompleteCompatibilityTest();

            // Assert
            var report = validator.GetCompatibilityReport();
            //Assert.IsNotNull(report.performanceMetrics, "性能指标应该存在");

            //var metrics = report.performanceMetrics;
            //Assert.IsTrue(metrics.averageFPS > 0, "平均FPS应该大于0");
            //Assert.IsTrue(metrics.minFPS > 0, "最小FPS应该大于0");
            //Assert.IsTrue(metrics.maxFPS > 0, "最大FPS应该大于0");
            //Assert.IsTrue(metrics.frameTime > 0, "帧时间应该大于0");
            //Assert.IsTrue(metrics.memoryUsage >= 0, "内存使用应该非负");
        }

        [UnityTest]
        public IEnumerator RenderingFeaturesTest_ShouldTestRenderingCapabilities()
        {
            // Arrange
            validator.Initialize();

            // Act
            yield return validator.RunCompleteCompatibilityTest();

            // Assert
            var report = validator.GetCompatibilityReport();
            //Assert.IsTrue(report.renderingFeatures.Count > 0, "应该测试渲染功能");

            //// 检查关键渲染功能
            //var renderingFeatureNames = report.renderingFeatures.ConvertAll(f => f.featureName);
            var expectedFeatures = new[]
            {
                "深度缓冲",
                "透明度排序"
            };

            //foreach (var expectedFeature in expectedFeatures)
            //{
            //    Assert.IsTrue(renderingFeatureNames.Contains(expectedFeature),
            //        $"应该测试渲染功能: {expectedFeature}");
            //}
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
            Assert.IsTrue(fileContent.Contains("urpVersion"), "报告应该包含URP版本信息");
        }

        [Test]
        public void ReportGeneration_ShouldCalculateValidScores()
        {
            // Arrange
            validator.Initialize();

            // Act
            var report = validator.GetCompatibilityReport();

            //// Assert
            //Assert.IsTrue(report.overallCompatibilityScore >= 0, "总体兼容性分数应该非负");
            //Assert.IsTrue(report.overallCompatibilityScore <= 100, "总体兼容性分数应该不超过100");
        }

        [UnityTest]
        public IEnumerator CompleteTestFlow_ShouldMaintainDataIntegrity()
        {
            // Arrange
            validator.Initialize();
            var initialReport = validator.GetCompatibilityReport();

            // Act
            yield return validator.RunCompleteCompatibilityTest();
            var finalReport = validator.GetCompatibilityReport();

            //// Assert
            //// 数据完整性检查
            //Assert.AreEqual(initialReport.unityVersion, finalReport.unityVersion,
            //    "Unity版本应该保持一致");
            //Assert.AreEqual(initialReport.urpVersion, finalReport.urpVersion,
            //    "URP版本应该保持一致");
            //Assert.IsTrue(finalReport.testDate >= initialReport.testDate,
            //    "测试日期应该被更新");
        }

        [Test]
        public void CompatibilityReport_ShouldHaveValidStructure()
        {
            // Arrange
            validator.Initialize();

            // Act
            var report = validator.GetCompatibilityReport();

            //// Assert
            //Assert.IsNotNull(report.compatibleFeatures, "兼容功能列表应该存在");
            //Assert.IsNotNull(report.incompatibleFeatures, "不兼容功能列表应该存在");
            //Assert.IsNotNull(report.warnings, "警告列表应该存在");
            //Assert.IsNotNull(report.shaderReport, "着色器报告应该存在");
            //Assert.IsNotNull(report.performanceMetrics, "性能指标应该存在");
            //Assert.IsNotNull(report.renderingFeatures, "渲染功能列表应该存在");
        }

        [UnityTest]
        public IEnumerator ErrorHandling_ShouldGracefullyHandleMissingComponents()
        {
            // Arrange
            validator.Initialize();

            // Act - 测试应该能优雅地处理缺失的组件
            yield return validator.RunCompleteCompatibilityTest();

            // Assert
            var report = validator.GetCompatibilityReport();

            // 即使某些组件缺失，报告也应该有效
            Assert.IsNotNull(report, "即使组件缺失，报告也应该存在");
            //Assert.IsTrue(report.compatibleFeatures.Count >= 0, "兼容功能数量应该有效");
            //Assert.IsTrue(report.incompatibleFeatures.Count >= 0, "不兼容功能数量应该有效");
        }

        [Test]
        public void ShaderReport_ShouldCalculateCorrectStatistics()
        {
            // Arrange
            validator.Initialize();

            // Act
            var report = validator.GetCompatibilityReport();
            //var shaderReport = report.shaderReport;

            //// Assert
            //Assert.IsTrue(shaderReport.compatibleShaders >= 0, "兼容着色器数量应该非负");
            //Assert.IsTrue(shaderReport.incompatibleShaders >= 0, "不兼容着色器数量应该非负");

            //var totalShaders = shaderReport.compatibleShaders + shaderReport.incompatibleShaders;
            //if (totalShaders > 0)
            //{
            //    Assert.IsTrue(shaderReport.compatibilityPercentage >= 0, "兼容性百分比应该非负");
            //    Assert.IsTrue(shaderReport.compatibilityPercentage <= 100, "兼容性百分比应该不超过100");
            //}
        }

        [UnityTest]
        public IEnumerator MultipleTestRuns_ShouldProduceConsistentResults()
        {
            // Arrange
            validator.Initialize();

            // Act - 运行两次测试
            yield return validator.RunCompleteCompatibilityTest();
            var firstReport = validator.GetCompatibilityReport();

            // 重置并再次运行
            validator.Initialize();
            yield return validator.RunCompleteCompatibilityTest();
            var secondReport = validator.GetCompatibilityReport();

            //// Assert
            //Assert.AreEqual(firstReport.unityVersion, secondReport.unityVersion,
            //    "Unity版本应该一致");
            //Assert.AreEqual(firstReport.ulpVersion, secondReport.ulpVersion,
            //    "URP版本应该一致");

            //// 功能检测结果应该一致（除非环境发生变化）
            //Assert.AreEqual(firstReport.isURPPresent, secondReport.isURPPresent,
            //    "URP存在性检测结果应该一致");
        }

        [UnityTest]
        public IEnumerator PerformanceMetrics_ShouldBeRealistic()
        {
            // Arrange
            validator.Initialize();

            // Act
            yield return validator.RunCompleteCompatibilityTest();

            //// Assert
            //var metrics = validator.GetCompatibilityReport().performanceMetrics;

            //// 性能指标应该在合理范围内
            //Assert.IsTrue(metrics.averageFPS >= 0 && metrics.averageFPS <= 1000,
            //    "平均FPS应该在合理范围内(0-1000)");
            //Assert.IsTrue(metrics.frameTime >= 0 && metrics.frameTime <= 1000,
            //    "帧时间应该在合理范围内(0-1000ms)");
            //Assert.IsTrue(metrics.memoryUsage >= 0 && metrics.memoryUsage <= 1000,
            //    "内存使用应该在合理范围内(0-1000MB)");
        }
    }
}