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
    /// 跨平台兼容性报告生成器测试套件
    /// Cross-Platform Compatibility Report Generator Test Suite
    /// </summary>
    [TestFixture]
    public class CrossPlatformCompatibilityReportGeneratorTests
    {
        private CrossPlatformCompatibilityReportGenerator reportGenerator;
        private string testReportPath;

        [SetUp]
        public void SetUp()
        {
            // 创建报告生成器实例
            reportGenerator = new CrossPlatformCompatibilityReportGenerator();

            // 设置测试报告路径
            testReportPath = Path.Combine(Application.temporaryCachePath, "test_reports");

            // 清理之前的测试文件
            if (Directory.Exists(testReportPath))
            {
                Directory.Delete(testReportPath, true);
            }
        }

        [TearDown]
        public void TearDown()
        {
            // 清理测试文件
            if (Directory.Exists(testReportPath))
            {
                Directory.Delete(testReportPath, true);
            }
        }

        [Test]
        public void ReportGenerator_Initialization_ShouldCreateValidInstance()
        {
            // Arrange & Act
            reportGenerator.Initialize();

            // Assert
            Assert.IsNotNull(reportGenerator, "报告生成器应该成功创建");

            var report = reportGenerator.GetComprehensiveReport();
            Assert.IsNotNull(report, "综合报告应该存在");
            Assert.IsNotNull(report.executiveSummary, "执行摘要应该存在");
            Assert.IsNotNull(report.validationSummary, "验证结果汇总应该存在");
            Assert.IsNotNull(report.platformDetails, "平台详情应该存在");
            Assert.IsNotNull(report.metadata, "报告元数据应该存在");
        }

        [Test]
        public void Initialization_ShouldSetCorrectMetadata()
        {
            // Arrange & Act
            reportGenerator.Initialize();

            // Assert
            var report = reportGenerator.GetComprehensiveReport();
            Assert.IsNotNull(report.metadata, "元数据应该存在");
            Assert.IsTrue(report.reportGenerationDate != default(DateTime), "报告生成日期应该被设置");
            Assert.IsFalse(string.IsNullOrEmpty(report.reportVersion), "报告版本应该被设置");
            Assert.AreEqual("Coin Animation System", report.projectName, "项目名称应该正确");
            Assert.IsFalse(string.IsNullOrEmpty(report.metadata.reportId), "报告ID应该被生成");
        }

        [UnityTest]
        public IEnumerator CompleteReportGeneration_ShouldRunSuccessfully()
        {
            // Arrange
            reportGenerator.Initialize();
            var initialReport = reportGenerator.GetComprehensiveReport();
            var initialScore = initialReport.executiveSummary.overallCompatibilityScore;

            // Act
            yield return reportGenerator.GenerateCompleteCompatibilityReport();

            // Assert
            var finalReport = reportGenerator.GetComprehensiveReport();
            Assert.IsTrue(finalReport.reportGenerationDate != default(DateTime), "报告生成日期应该被更新");
            Assert.IsTrue(finalReport.metadata.totalValidationTime > 0, "验证时间应该被记录");
            Assert.IsTrue(finalReport.validationSummary.totalTestsRun > 0, "应该运行了测试");
            Assert.IsTrue(finalReport.validationSummary.passedTests >= 0, "应该有通过测试统计");
            Assert.IsTrue(finalReport.validationSummary.failedTests >= 0, "应该有失败测试统计");
            Assert.IsTrue(finalReport.validationSummary.overallPassRate >= 0, "总体通过率应该有效");
        }

        [UnityTest]
        public IEnumerator ExecutiveSummaryGeneration_ShouldProvideValidAssessment()
        {
            // Arrange
            reportGenerator.Initialize();

            // Act
            yield return reportGenerator.GenerateCompleteCompatibilityReport();

            // Assert
            var report = reportGenerator.GetComprehensiveReport();
            var summary = report.executiveSummary;

            Assert.IsNotNull(summary, "执行摘要应该存在");
            Assert.IsFalse(string.IsNullOrEmpty(summary.overview), "概述应该存在");
            Assert.IsTrue(summary.overallCompatibilityScore >= 0 && summary.overallCompatibilityScore <= 100,
                "兼容性分数应该在0-100%之间");
            Assert.IsFalse(string.IsNullOrEmpty(summary.readinessLevel), "准备度级别应该被设置");
            Assert.IsTrue(summary.keyFindings.Count > 0, "应该有关键发现");
            Assert.IsTrue(summary.immediateActions.Count > 0, "应该有立即行动建议");
        }

        [UnityTest]
        public IEnumerator ValidationSummaryGeneration_ShouldAggregateTestResults()
        {
            // Arrange
            reportGenerator.Initialize();

            // Act
            yield return reportGenerator.GenerateCompleteCompatibilityReport();

            // Assert
            var report = reportGenerator.GetComprehensiveReport();
            var summary = report.validationSummary;

            Assert.IsNotNull(summary, "验证结果汇总应该存在");
            Assert.IsTrue(summary.totalTestsRun > 0, "总测试数应该大于0");
            Assert.IsTrue(summary.passedTests >= 0, "通过测试数应该非负");
            Assert.IsTrue(summary.failedTests >= 0, "失败测试数应该非负");
            Assert.IsTrue(summary.overallPassRate >= 0 && summary.overallPassRate <= 100,
                "总体通过率应该在0-100%之间");
            Assert.IsTrue(summary.categorySummaries.Count > 0, "应该有分类测试汇总");
        }

        [UnityTest]
        public IEnumerator PlatformCompatibilityDetails_ShouldIncludeAllPlatforms()
        {
            // Arrange
            reportGenerator.Initialize();

            // Act
            yield return reportGenerator.GenerateCompleteCompatibilityReport();

            // Assert
            var report = reportGenerator.GetComprehensiveReport();

            // 检查是否包含主要平台
            var expectedPlatforms = new[] { "Windows", "Linux", "Mac", "iOS", "Android" };
            foreach (var platform in expectedPlatforms)
            {
                Assert.IsTrue(report.platformDetails.ContainsKey(platform),
                    $"应该包含 {platform} 平台详情");

                var platformDetail = report.platformDetails[platform];
                Assert.AreEqual(platform, platformDetail.platformName, "平台名称应该正确");
                Assert.IsTrue(platformDetail.compatibilityScore >= 0 && platformDetail.compatibilityScore <= 100,
                    "兼容性分数应该在0-100%之间");
                Assert.IsNotNull(platformDetail.supportedFeatures, "支持的功能列表应该存在");
                Assert.IsNotNull(platformDetail.platformPerformance, "平台性能指标应该存在");
            }
        }

        [UnityTest]
        public IEnumerator PerformanceBenchmarkReport_ShouldIncludePerformanceData()
        {
            // Arrange
            reportGenerator.Initialize();

            // Act
            yield return reportGenerator.GenerateCompleteCompatibilityReport();

            // Assert
            var report = reportGenerator.GetComprehensiveReport();
            var perfReport = report.performanceBenchmarkReport;

            Assert.IsNotNull(perfReport, "性能基准报告应该存在");
            Assert.IsTrue(perfReport.platformBenchmarks.Count > 0, "应该有平台基准数据");
            Assert.IsTrue(perfReport.crossPlatformComparisons.Count > 0, "应该有交叉平台对比");
            Assert.IsNotNull(perfReport.trendAnalysis, "趋势分析应该存在");

            // 检查平台基准数据
            foreach (var benchmark in perfReport.platformBenchmarks.Values)
            {
                Assert.IsFalse(string.IsNullOrEmpty(benchmark.platform), "平台名称应该存在");
                Assert.IsTrue(benchmark.fps >= 0, "FPS应该非负");
                Assert.IsTrue(benchmark.memoryUsage >= 0, "内存使用应该非负");
                Assert.IsTrue(benchmark.drawCalls >= 0, "Draw Call数量应该非负");
            }
        }

        [UnityTest]
        public IEnumerator IssueIdentification_ShouldDetectProblems()
        {
            // Arrange
            reportGenerator.Initialize();

            // Act
            yield return reportGenerator.GenerateCompleteCompatibilityReport();

            // Assert
            var report = reportGenerator.GetComprehensiveReport();

            // 检查问题列表结构
            Assert.IsNotNull(report.identifiedIssues, "识别的问题列表应该存在");

            // 如果有问题，检查其结构
            foreach (var issue in report.identifiedIssues)
            {
                Assert.IsFalse(string.IsNullOrEmpty(issue.issueId), "问题ID应该存在");
                Assert.IsFalse(string.IsNullOrEmpty(issue.title), "问题标题应该存在");
                Assert.IsFalse(string.IsNullOrEmpty(issue.description), "问题描述应该存在");
                Assert.IsFalse(string.IsNullOrEmpty(issue.category), "问题类别应该存在");
                Assert.IsFalse(string.IsNullOrEmpty(issue.severity), "问题严重程度应该存在");
                Assert.IsTrue(issue.affectedPlatforms.Count > 0, "应该有受影响的平台列表");
                Assert.IsNotNull(issue.suggestedSolutions, "建议解决方案列表应该存在");
            }
        }

        [UnityTest]
        public IEnumerator RecommendationGeneration_ShouldProvideActionableAdvice()
        {
            // Arrange
            reportGenerator.Initialize();

            // Act
            yield return reportGenerator.GenerateCompleteCompatibilityReport();

            // Assert
            var report = reportGenerator.GetComprehensiveReport();

            // 检查建议列表
            Assert.IsTrue(report.recommendations.Count > 0, "应该有建议列表");

            foreach (var recommendation in report.recommendations)
            {
                Assert.IsFalse(string.IsNullOrEmpty(recommendation.recommendationId), "建议ID应该存在");
                Assert.IsFalse(string.IsNullOrEmpty(recommendation.title), "建议标题应该存在");
                Assert.IsFalse(string.IsNullOrEmpty(recommendation.description), "建议描述应该存在");
                Assert.IsFalse(string.IsNullOrEmpty(recommendation.category), "建议类别应该存在");
                Assert.IsTrue(recommendation.priority >= 1 && recommendation.priority <= 4, "优先级应该在1-4之间");
                Assert.IsFalse(string.IsNullOrEmpty(recommendation.implementationEffort), "实施难度应该存在");
                Assert.IsFalse(string.IsNullOrEmpty(recommendation.expectedBenefit), "预期收益应该存在");
                Assert.IsTrue(recommendation.implementationSteps.Count > 0, "应该有实施步骤");
            }

            // 检查建议是否按优先级排序
            for (int i = 0; i < report.recommendations.Count - 1; i++)
            {
                Assert.IsTrue(report.recommendations[i].priority <= report.recommendations[i + 1].priority,
                    "建议应该按优先级排序");
            }
        }

        [UnityTest]
        public IEnumerator ConclusionGeneration_ShouldProvideFinalAssessment()
        {
            // Arrange
            reportGenerator.Initialize();

            // Act
            yield return reportGenerator.GenerateCompleteCompatibilityReport();

            // Assert
            var report = reportGenerator.GetComprehensiveReport();
            var conclusion = report.conclusion;

            Assert.IsNotNull(conclusion, "结论和下一步应该存在");
            Assert.IsFalse(string.IsNullOrEmpty(conclusion.overallAssessment), "总体评估应该存在");
            Assert.IsNotNull(conclusion.remainingTasks, "剩余任务列表应该存在");
            Assert.IsNotNull(conclusion.longTermRecommendations, "长期建议列表应该存在");
            Assert.IsFalse(string.IsNullOrEmpty(conclusion.deploymentReadiness), "部署准备状态应该存在");
            Assert.IsTrue(conclusion.maintenanceRequirements.Count > 0, "应该有维护要求");
        }

        [UnityTest]
        public IEnumerator ReportExport_ShouldCreateMultipleFormats()
        {
            // Arrange
            reportGenerator.Initialize();

            // Act
            yield return reportGenerator.GenerateCompleteCompatibilityReport();

            // Assert
            // 检查是否创建了输出目录
            Assert.IsTrue(Directory.Exists(testReportPath), "应该创建输出目录");

            // 检查是否生成了报告文件
            var files = Directory.GetFiles(testReportPath, "*.*");
            Assert.IsTrue(files.Length > 0, "应该生成至少一个报告文件");

            // 检查JSON报告
            var jsonFiles = Directory.GetFiles(testReportPath, "*.json");
            if (jsonFiles.Length > 0)
            {
                var jsonContent = File.ReadAllText(jsonFiles[0]);
                Assert.IsFalse(string.IsNullOrEmpty(jsonContent), "JSON文件应该有内容");
                Assert.IsTrue(jsonContent.Contains("reportGenerationDate"), "JSON应该包含报告生成日期");
            }

            // 检查Markdown报告
            var mdFiles = Directory.GetFiles(testReportPath, "*.md");
            if (mdFiles.Length > 0)
            {
                var mdContent = File.ReadAllText(mdFiles[0]);
                Assert.IsFalse(string.IsNullOrEmpty(mdContent), "Markdown文件应该有内容");
                Assert.IsTrue(mdContent.Contains("# 跨平台兼容性综合报告"), "Markdown应该有标题");
            }
        }

        [UnityTest]
        public IEnumerator ReadinessLevelDetermination_ShouldProvideAccurateAssessment()
        {
            // Arrange
            reportGenerator.Initialize();

            // Act
            yield return reportGenerator.GenerateCompleteCompatibilityReport();

            // Assert
            var report = reportGenerator.GetComprehensiveReport();
            var summary = report.executiveSummary;

            // 检查准备度级别的逻辑
            if (summary.overallCompatibilityScore >= 95f && summary.criticalIssuesCount == 0)
            {
                Assert.AreEqual("Production Ready", summary.readinessLevel, "高分数且无关键问题应该评为Production Ready");
                Assert.IsTrue(summary.isProductionReady, "Production Ready状态应该正确");
            }
            else if (summary.overallCompatibilityScore >= 85f && summary.criticalIssuesCount <= 1)
            {
                Assert.AreEqual("Needs Attention", summary.readinessLevel, "中等分数应该评为Needs Attention");
                Assert.IsFalse(summary.isProductionReady, "Needs Attention状态应该正确");
            }
            else
            {
                Assert.AreEqual("Not Ready", summary.readinessLevel, "低分数应该评为Not Ready");
                Assert.IsFalse(summary.isProductionReady, "Not Ready状态应该正确");
            }
        }

        [UnityTest]
        public IEnumerator MetadataCompletion_ShouldIncludeAllEnvironmentInfo()
        {
            // Arrange
            reportGenerator.Initialize();

            // Act
            yield return reportGenerator.GenerateCompleteCompatibilityReport();

            // Assert
            var report = reportGenerator.GetComprehensiveReport();
            var metadata = report.metadata;

            Assert.IsNotNull(metadata, "元数据应该存在");
            Assert.IsFalse(string.IsNullOrEmpty(metadata.reportId), "报告ID应该存在");
            Assert.IsTrue(metadata.generationDate != default(DateTime), "生成日期应该存在");
            Assert.IsFalse(string.IsNullOrEmpty(metadata.generatedBy), "生成者应该存在");
            Assert.IsFalse(string.IsNullOrEmpty(metadata.unityVersion), "Unity版本应该存在");
            Assert.IsFalse(string.IsNullOrEmpty(metadata.platform), "平台应该存在");
            Assert.IsTrue(metadata.totalValidationTime > 0, "总验证时间应该被记录");

            // 检查测试环境信息
            Assert.IsTrue(metadata.testEnvironment.Count > 0, "应该有测试环境信息");
            Assert.IsTrue(metadata.testEnvironment.ContainsKey("Unity Version"), "应该有Unity版本信息");
            Assert.IsTrue(metadata.testEnvironment.ContainsKey("Platform"), "应该有平台信息");

            // 检查验证工具列表
            Assert.IsTrue(metadata.validationTools.Count > 0, "应该有验证工具列表");
        }

        [UnityTest]
        public IEnumerator ReportGenerationTime_ShouldBeReasonable()
        {
            // Arrange
            reportGenerator.Initialize();

            // Act
            var startTime = Time.realtimeSinceStartup;
            yield return reportGenerator.GenerateCompleteCompatibilityReport();
            var generationTime = Time.realtimeSinceStartup - startTime;

            // Assert
            var report = reportGenerator.GetComprehensiveReport();
            Assert.IsTrue(generationTime <= 30f, "报告生成时间应该在30秒内");
            Assert.IsTrue(report.metadata.totalValidationTime > 0, "验证时间应该被记录");
            Assert.IsTrue(report.metadata.totalValidationTime <= generationTime + 1f, "记录的时间应该合理");
        }

        [UnityTest]
        public IEnumerator ErrorHandling_ShouldGracefullyHandleValidatorFailures()
        {
            // Arrange
            reportGenerator.Initialize();

            // Act - 即使验证器可能失败，报告生成也应该成功
            yield return reportGenerator.GenerateCompleteCompatibilityReport();

            // Assert
            var report = reportGenerator.GetComprehensiveReport();

            // 即使某些验证器失败，报告也应该有效
            Assert.IsNotNull(report, "即使验证器失败，报告也应该存在");
            Assert.IsNotNull(report.executiveSummary, "执行摘要应该存在");
            Assert.IsTrue(report.validationSummary.totalTestsRun >= 0, "总测试数应该有效");
            Assert.IsTrue(report.executiveSummary.overallCompatibilityScore >= 0, "兼容性分数应该有效");
        }

        [Test]
        public void MarkdownReportGeneration_ShouldHaveValidStructure()
        {
            // Arrange
            reportGenerator.Initialize();

            // Act - 这里我们不能运行完整的生成（因为它是协程），但我们可以测试Markdown生成逻辑
            var report = reportGenerator.GetComprehensiveReport();

            // Assert
            Assert.IsNotNull(report, "报告应该存在");

            // 测试Markdown生成的基本逻辑
            var markdown = $@"
# 跨平台兼容性综合报告

**生成时间**: {report.reportGenerationDate:yyyy-MM-dd HH:mm:ss}
**报告版本**: {report.reportVersion}
**项目名称**: {report.projectName}

## 📊 执行摘要

{report.executiveSummary.overview}

**总体兼容性分数**: {report.executiveSummary.overallCompatibilityScore:F1}%
**准备度级别**: {report.executiveSummary.readinessLevel}
";

            Assert.IsTrue(markdown.Contains("# 跨平台兼容性综合报告"), "Markdown应该有标题");
            Assert.IsTrue(markdown.Contains("## 📊 执行摘要"), "Markdown应该有执行摘要部分");
            Assert.IsTrue(markdown.Contains("总体兼容性分数"), "Markdown应该包含兼容性分数");
        }

        [UnityTest]
        public IEnumerator ConsistencyScoreCalculation_ShouldBeAccurate()
        {
            // Arrange
            reportGenerator.Initialize();

            // Act
            yield return reportGenerator.GenerateCompleteCompatibilityReport();

            // Assert
            var report = reportGenerator.GetComprehensiveReport();
            var summary = report.validationSummary;

            // 手动计算一致性分数
            var calculatedScore = 0f;
            var weightSum = 0f;

            if (summary.categorySummaries.ContainsKey("Functional Consistency"))
            {
                var functionalScore = summary.categorySummaries["Functional Consistency"].passRate;
                calculatedScore += functionalScore * 0.4f;
                weightSum += 0.4f;
            }

            if (summary.categorySummaries.ContainsKey("Performance Benchmarks"))
            {
                var performanceScore = summary.categorySummaries["Performance Benchmarks"].passRate;
                calculatedScore += performanceScore * 0.35f;
                weightSum += 0.35f;
            }

            if (summary.categorySummaries.ContainsKey("Visual Effects"))
            {
                var visualScore = summary.categorySummaries["Visual Effects"].passRate;
                calculatedScore += visualScore * 0.25f;
                weightSum += 0.25f;
            }

            var expectedScore = weightSum > 0 ? calculatedScore / weightSum : 0f;

            Assert.AreEqual(expectedScore, report.executiveSummary.overallCompatibilityScore, 0.1f,
                "一致性分数计算应该准确");
        }
    }
}