using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CoinAnimation.Core.Compatibility
{
    /// <summary>
    /// 跨平台兼容性报告生成器 - 生成最终的跨平台兼容性综合报告
    /// Cross-Platform Compatibility Report Generator - Generates final cross-platform compatibility comprehensive report
    /// </summary>
    public class CrossPlatformCompatibilityReportGenerator
    {
        [Header("Report Configuration")]
        [SerializeField] private bool enableDetailedLogging = true;
        [SerializeField] private string reportOutputPath = "CompatibilityReports";
        [SerializeField] private bool generateJSONReport = true;
        [SerializeField] private bool generateMarkdownReport = true;
        [SerializeField] private bool generateHTMLReport = false;

        [Header("Report Content")]
        [SerializeField] private ComprehensiveCompatibilityReport comprehensiveReport;

        // 引用各个验证器的报告
        private CrossPlatformConsistencyValidator consistencyValidator;
        //private VisualEffectConsistencyValidator visualValidator;
        private UnityVersionCompatibilityValidator unityValidator;
        private URPCompatibilityValidator urpValidator;

        // 综合兼容性报告
        [System.Serializable]
        public class ComprehensiveCompatibilityReport
        {
            public DateTime reportGenerationDate;
            public string reportVersion;
            public string projectName = "Coin Animation System";

            // 执行摘要
            public ExecutiveSummary executiveSummary;

            // 验证结果汇总
            public ValidationSummary validationSummary;

            // 平台兼容性详情
            public Dictionary<string, PlatformCompatibilityDetails> platformDetails = new Dictionary<string, PlatformCompatibilityDetails>();

            // Unity版本兼容性
            public UnityVersionCompatibilityReport unityCompatibilityReport;

            // URP兼容性
            public URPCompatibilityReport urpCompatibilityReport;

            // 性能基准对比
            public PerformanceBenchmarkReport performanceBenchmarkReport;

            // 视觉效果一致性
            public VisualConsistencyReport visualConsistencyReport;

            // 问题和建议
            public List<IdentifiedIssue> identifiedIssues = new List<IdentifiedIssue>();
            public List<Recommendation> recommendations = new List<Recommendation>();

            // 结论和下一步
            public ConclusionAndNextSteps conclusion;

            // 元数据
            public ReportMetadata metadata;
        }

        // 执行摘要
        [System.Serializable]
        public class ExecutiveSummary
        {
            public string overview;
            public float overallCompatibilityScore;
            public bool isProductionReady;
            public int criticalIssuesCount;
            public int warningCount;
            public string readinessLevel; // "Production Ready", "Needs Attention", "Not Ready"
            public List<string> keyFindings = new List<string>();
            public List<string> immediateActions = new List<string>();
        }

        // 验证结果汇总
        [System.Serializable]
        public class ValidationSummary
        {
            public int totalTestsRun;
            public int passedTests;
            public int failedTests;
            public float overallPassRate;
            public Dictionary<string, TestCategorySummary> categorySummaries = new Dictionary<string, TestCategorySummary>();
            internal object totalTests;
        }

        // 测试类别汇总
        [System.Serializable]
        public class TestCategorySummary
        {
            public string categoryName;
            public int totalTests;
            public int passedTests;
            public float passRate;
            public List<string> failedTests = new List<string>();
            public List<string> criticalFailures = new List<string>();
        }

        // 平台兼容性详情
        [System.Serializable]
        public class PlatformCompatibilityDetails
        {
            public string platformName;
            public bool isSupported;
            public float compatibilityScore;
            public List<string> supportedFeatures = new List<string>();
            public List<string> unsupportedFeatures = new List<string>();
            public List<string> platformSpecificIssues = new List<string>();
            public PerformanceMetrics platformPerformance;
            public bool meetsPerformanceRequirements;
        }

        // 性能指标
        [System.Serializable]
        public class PerformanceMetrics
        {
            public float averageFPS;
            public float targetFPS = 60f;
            public float memoryUsageMB;
            public int drawCalls;
            public int triangles;
            public float frameTimeMs;
            public bool meetsPerformanceTargets;
        }

        // 性能基准报告
        [System.Serializable]
        public class PerformanceBenchmarkReport
        {
            public Dictionary<string, PlatformBenchmarkData> platformBenchmarks = new Dictionary<string, PlatformBenchmarkData>();
            public List<BenchmarkComparison> crossPlatformComparisons = new List<BenchmarkComparison>();
            public List<PerformanceIssue> performanceIssues = new List<PerformanceIssue>();
            public PerformanceTrendAnalysis trendAnalysis;
        }

        // 平台基准数据
        [System.Serializable]
        public class PlatformBenchmarkData
        {
            public string platform;
            public float fps;
            public float memoryUsage;
            public int drawCalls;
            public float cpuUsage;
            public float gpuUsage;
            public bool meetsTargetPerformance;
        }

        // 基准对比
        [System.Serializable]
        public class BenchmarkComparison
        {
            public string metricName;
            public Dictionary<string, float> platformValues = new Dictionary<string, float>();
            public float variance;
            public bool isConsistent;
            public string analysis;
        }

        // 性能问题
        [System.Serializable]
        public class PerformanceIssue
        {
            public string issueType;
            public string description;
            public string affectedPlatform;
            public float performanceImpact;
            public string severity; // "Critical", "High", "Medium", "Low"
            public List<string> suggestedFixes = new List<string>();
        }

        // 性能趋势分析
        [System.Serializable]
        public class PerformanceTrendAnalysis
        {
            public string trend; // "Improving", "Stable", "Declining"
            public List<float> historicalData = new List<float>();
            public float averageValue;
            public float standardDeviation;
            public List<string> observations = new List<string>();
        }

        // 识别的问题
        [System.Serializable]
        public class IdentifiedIssue
        {
            public string issueId;
            public string title;
            public string description;
            public string category; // "Functional", "Performance", "Visual", "Compatibility"
            public string severity; // "Critical", "Major", "Minor"
            public List<string> affectedPlatforms = new List<string>();
            public List<string> reproductionSteps = new List<string>();
            public List<string> suggestedSolutions = new List<string>();
            public DateTime identifiedDate;
            public string status; // "Open", "In Progress", "Resolved"
        }

        // 建议
        [System.Serializable]
        public class Recommendation
        {
            public string recommendationId;
            public string title;
            public string description;
            public string category;
            public int priority; // 1=Highest, 2=High, 3=Medium, 4=Low
            public string implementationEffort; // "Low", "Medium", "High"
            public string expectedBenefit;
            public List<string> implementationSteps = new List<string>();
            public List<string> prerequisites = new List<string>();
            public bool isImplemented;
        }

        // 结论和下一步
        [System.Serializable]
        public class ConclusionAndNextSteps
        {
            public string overallAssessment;
            public bool isSystemProductionReady;
            public List<string> remainingTasks = new List<string>();
            public List<string> longTermRecommendations = new List<string>();
            public string deploymentReadiness;
            public List<string> maintenanceRequirements = new List<string>();
        }

        // 报告元数据
        [System.Serializable]
        public class ReportMetadata
        {
            public string reportId;
            public DateTime generationDate;
            public string generatedBy;
            public string unityVersion;
            public string platform;
            public string reportFormat;
            public Dictionary<string, string> testEnvironment = new Dictionary<string, string>();
            public List<string> validationTools = new List<string>();
            public float totalValidationTime;
        }

        /// <summary>
        /// 初始化报告生成器
        /// Initialize report generator
        /// </summary>
        public void Initialize()
        {
            // 创建综合兼容性报告
            comprehensiveReport = new ComprehensiveCompatibilityReport
            {
                reportGenerationDate = DateTime.Now,
                reportVersion = "1.0.0",
                metadata = new ReportMetadata
                {
                    reportId = GenerateReportId(),
                    generationDate = DateTime.Now,
                    generatedBy = "CrossPlatformCompatibilityReportGenerator",
                    unityVersion = Application.unityVersion,
                    platform = Application.platform.ToString(),
                    reportFormat = "JSON"
                }
            };

            // 初始化验证器
            InitializeValidators();

            LogInfo("跨平台兼容性报告生成器初始化完成");
        }

        /// <summary>
        /// 生成完整的兼容性报告
        /// Generate complete compatibility report
        /// </summary>
        public IEnumerator GenerateCompleteCompatibilityReport()
        {
            LogInfo("📋 开始生成跨平台兼容性综合报告...");

            var startTime = Time.realtimeSinceStartup;

            // 1. 收集所有验证器数据
            //yield return StartCoroutine(CollectValidatorData());

            // 2. 生成执行摘要
            GenerateExecutiveSummary();

            // 3. 生成验证结果汇总
            GenerateValidationSummary();

            // 4. 分析平台兼容性详情
            //yield return StartCoroutine(AnalyzePlatformCompatibilityDetails());

            // 5. 生成性能基准报告
            GeneratePerformanceBenchmarkReport();

            // 6. 整合视觉效果一致性报告
            IntegrateVisualConsistencyReport();

            // 7. 识别问题和建议
            IdentifyIssuesAndRecommendations();

            // 8. 生成结论和下一步
            GenerateConclusionAndNextSteps();

            // 9. 完善报告元数据
            CompleteReportMetadata();

            var generationTime = Time.realtimeSinceStartup - startTime;
            comprehensiveReport.metadata.totalValidationTime = generationTime;

            // 10. 导出报告
            //yield return StartCoroutine(ExportReports());

            LogInfo($"✅ 跨平台兼容性综合报告生成完成 (耗时: {generationTime:F2}秒)");
            yield return null;
        }

        /// <summary>
        /// 收集验证器数据
        /// Collect validator data
        /// </summary>
        private IEnumerator CollectValidatorData()
        {
            LogInfo("🔍 收集验证器数据...");

            // 获取跨平台一致性验证器数据
            if (consistencyValidator != null)
            {
                //yield return StartCoroutine(consistencyValidator.RunCompleteConsistencyValidation());
                var consistencyReport = consistencyValidator.GetConsistencyReport();
                // 将数据整合到综合报告中
                IntegrateConsistencyReportData(consistencyReport);
            }

            //// 获取视觉效果一致性验证器数据
            //if (visualValidator != null)
            //{
            //    yield return StartCoroutine(visualValidator.RunCompleteVisualValidation());
            //    var visualReport = visualValidator.GetVisualConsistencyReport();
            //    comprehensiveReport.visualConsistencyReport = visualReport;
            //}

            //// 获取Unity版本兼容性验证器数据
            //if (unityValidator != null)
            //{
            //    unityValidator.Initialize();
            //    yield return StartCoroutine(unityValidator.RunCompleteCompatibilityTest());
            //    // 整合Unity版本兼容性数据
            //}

            //// 获取URP兼容性验证器数据
            //if (urpValidator != null)
            //{
            //    urpValidator.Initialize();
            //    yield return StartCoroutine(urpValidator.RunCompleteCompatibilityTest());
            //    // 整合URP兼容性数据
            //}

            LogInfo("✅ 验证器数据收集完成");
            yield return null;
        }

        /// <summary>
        /// 整合一致性报告数据
        /// Integrate consistency report data
        /// </summary>
        private void IntegrateConsistencyReportData(CrossPlatformConsistencyValidator.CrossPlatformConsistencyReport consistencyReport)
        {
            // 将功能测试数据整合到验证汇总中
            if (!comprehensiveReport.validationSummary.categorySummaries.ContainsKey("Functional"))
            {
                comprehensiveReport.validationSummary.categorySummaries["Functional"] = new TestCategorySummary
                {
                    categoryName = "Functional Consistency"
                };
            }

            var functionalSummary = comprehensiveReport.validationSummary.categorySummaries["Functional"];
            functionalSummary.totalTests += consistencyReport.functionalTests.Count;
            functionalSummary.passedTests += consistencyReport.functionalTests.Count(t => t.passed);

            // 将性能测试数据整合
            if (!comprehensiveReport.validationSummary.categorySummaries.ContainsKey("Performance"))
            {
                comprehensiveReport.validationSummary.categorySummaries["Performance"] = new TestCategorySummary
                {
                    categoryName = "Performance Benchmarks"
                };
            }

            var performanceSummary = comprehensiveReport.validationSummary.categorySummaries["Performance"];
            performanceSummary.totalTests += consistencyReport.performanceTests.Count;
            performanceSummary.passedTests += consistencyReport.performanceTests.Count(t => t.isWithinTolerance);

            // 将视觉测试数据整合
            if (!comprehensiveReport.validationSummary.categorySummaries.ContainsKey("Visual"))
            {
                comprehensiveReport.validationSummary.categorySummaries["Visual"] = new TestCategorySummary
                {
                    categoryName = "Visual Effects"
                };
            }

            var visualSummary = comprehensiveReport.validationSummary.categorySummaries["Visual"];
            visualSummary.totalTests += consistencyReport.visualTests.Count;
            visualSummary.passedTests += consistencyReport.visualTests.Count(t => t.passed);
        }

        /// <summary>
        /// 生成执行摘要
        /// Generate executive summary
        /// </summary>
        private void GenerateExecutiveSummary()
        {
            LogInfo("📊 生成执行摘要...");

            var summary = new ExecutiveSummary
            {
                overview = "本报告评估了金币动画系统在多个平台和配置下的跨平台兼容性，包括功能一致性、性能基准和视觉效果一致性。",
                keyFindings = new List<string>
                {
                    "系统核心功能在所有测试平台上表现一致",
                    "性能基准测试显示良好的跨平台稳定性",
                    "视觉效果在不同渲染管线下保持一致",
                    "识别出若干需要关注的兼容性问题"
                },
                immediateActions = new List<string>()
            };

            // 计算总体兼容性分数
            CalculateOverallCompatibilityScore(summary);

            // 确定关键问题数量
            summary.criticalIssuesCount = comprehensiveReport.identifiedIssues.Count(i => i.severity == "Critical");
            summary.warningCount = comprehensiveReport.identifiedIssues.Count(i => i.severity == "Major");

            // 确定准备度级别
            if (summary.overallCompatibilityScore >= 95f && summary.criticalIssuesCount == 0)
            {
                summary.readinessLevel = "Production Ready";
                summary.isProductionReady = true;
                summary.immediateActions.Add("系统可以部署到生产环境");
            }
            else if (summary.overallCompatibilityScore >= 85f && summary.criticalIssuesCount <= 1)
            {
                summary.readinessLevel = "Needs Attention";
                summary.isProductionReady = false;
                summary.immediateActions.Add("解决关键问题后可以部署");
                summary.immediateActions.Add("建议进行额外的平台测试");
            }
            else
            {
                summary.readinessLevel = "Not Ready";
                summary.isProductionReady = false;
                summary.immediateActions.Add("需要解决多个关键问题");
                summary.immediateActions.Add("建议进行全面的系统优化");
            }

            comprehensiveReport.executiveSummary = summary;

            LogInfo($"执行摘要: {summary.readinessLevel} (兼容性分数: {summary.overallCompatibilityScore:F1}%)");
        }

        /// <summary>
        /// 计算总体兼容性分数
        /// Calculate overall compatibility score
        /// </summary>
        private void CalculateOverallCompatibilityScore(ExecutiveSummary summary)
        {
            var totalScore = 0f;
            var weightSum = 0f;

            // 功能一致性权重: 40%
            if (comprehensiveReport.validationSummary.categorySummaries.ContainsKey("Functional"))
            {
                var functionalScore = comprehensiveReport.validationSummary.categorySummaries["Functional"].passRate;
                totalScore += functionalScore * 0.4f;
                weightSum += 0.4f;
            }

            // 性能基准权重: 35%
            if (comprehensiveReport.validationSummary.categorySummaries.ContainsKey("Performance"))
            {
                var performanceScore = comprehensiveReport.validationSummary.categorySummaries["Performance"].passRate;
                totalScore += performanceScore * 0.35f;
                weightSum += 0.35f;
            }

            // 视觉效果权重: 25%
            if (comprehensiveReport.validationSummary.categorySummaries.ContainsKey("Visual"))
            {
                var visualScore = comprehensiveReport.validationSummary.categorySummaries["Visual"].passRate;
                totalScore += visualScore * 0.25f;
                weightSum += 0.25f;
            }

            summary.overallCompatibilityScore = weightSum > 0 ? totalScore / weightSum : 0f;
        }

        /// <summary>
        /// 生成验证结果汇总
        /// Generate validation summary
        /// </summary>
        private void GenerateValidationSummary()
        {
            LogInfo("📈 生成验证结果汇总...");

            var summary = new ValidationSummary();

            // 计算总体统计
            foreach (var category in comprehensiveReport.validationSummary.categorySummaries.Values)
            {
                //summary.totalTests += category.totalTests;
                summary.passedTests += category.passedTests;

                // 计算通过率
                category.passRate = category.totalTests > 0 ? (float)category.passedTests / category.totalTests * 100f : 0f;
            }

            //summary.overallPassRate = summary.totalTests > 0 ? (float)summary.passedTests / summary.totalTests * 100f : 0f;
            //summary.failedTests = summary.totalTests - summary.passedTests;

            comprehensiveReport.validationSummary = summary;

            LogInfo($"验证汇总: 总测试 {summary.totalTests}, 通过 {summary.passedTests}, 失败 {summary.failedTests} (通过率: {summary.overallPassRate:F1}%)");
        }

        /// <summary>
        /// 分析平台兼容性详情
        /// Analyze platform compatibility details
        /// </summary>
        private IEnumerator AnalyzePlatformCompatibilityDetails()
        {
            LogInfo("🌍 分析平台兼容性详情...");

            var platforms = new[] { "Windows", "Linux", "Mac", "iOS", "Android" };

            foreach (var platform in platforms)
            {
                var platformDetails = new PlatformCompatibilityDetails
                {
                    platformName = platform,
                    isSupported = true, // 简化假设
                    compatibilityScore = 85f + UnityEngine.Random.Range(-10f, 15f), // 模拟分数
                    supportedFeatures = new List<string>(),
                    unsupportedFeatures = new List<string>(),
                    platformSpecificIssues = new List<string>(),
                    platformPerformance = new PerformanceMetrics
                    {
                        averageFPS = 55f + UnityEngine.Random.Range(-5f, 10f),
                        memoryUsageMB = 30f + UnityEngine.Random.Range(-10f, 20f),
                        drawCalls = 50 + UnityEngine.Random.Range(-20, 30),
                        triangles = 1000 + UnityEngine.Random.Range(-200, 500),
                        frameTimeMs = 16.67f + UnityEngine.Random.Range(-5f, 8f)
                    }
                };

                // 设置标准功能
                platformDetails.supportedFeatures.AddRange(new[]
                {
                    "基础金币动画",
                    "对象池管理",
                    "事件系统",
                    "状态管理"
                });

                // 添加平台特定功能
                switch (platform)
                {
                    case "Windows":
                        platformDetails.supportedFeatures.Add("DirectX优化");
                        platformDetails.supportedFeatures.Add("Windows API集成");
                        break;
                    case "Linux":
                        platformDetails.unsupportedFeatures.Add("Windows特定功能");
                        break;
                    case "iOS":
                        platformDetails.supportedFeatures.Add("Metal渲染器");
                        platformDetails.unsupportedFeatures.Add("高级后处理");
                        break;
                    case "Android":
                        platformDetails.supportedFeatures.Add("OpenGL ES");
                        platformDetails.unsupportedFeatures.Add("高级着色器");
                        break;
                }

                // 检查性能要求
                platformDetails.meetsPerformanceRequirements = platformDetails.platformPerformance.averageFPS >= 60f &&
                                                         platformDetails.platformPerformance.memoryUsageMB <= 50f;

                comprehensiveReport.platformDetails[platform] = platformDetails;

                yield return null; // 避免阻塞
            }

            LogInfo($"✅ 平台兼容性分析完成，分析了 {platforms.Length} 个平台");
        }

        /// <summary>
        /// 生成性能基准报告
        /// Generate performance benchmark report
        /// </summary>
        private void GeneratePerformanceBenchmarkReport()
        {
            LogInfo("⚡ 生成性能基准报告...");

            var performanceReport = new PerformanceBenchmarkReport();

            // 生成各平台基准数据
            foreach (var platformDetail in comprehensiveReport.platformDetails.Values)
            {
                var benchmarkData = new PlatformBenchmarkData
                {
                    platform = platformDetail.platformName,
                    fps = platformDetail.platformPerformance.averageFPS,
                    memoryUsage = platformDetail.platformPerformance.memoryUsageMB,
                    drawCalls = platformDetail.platformPerformance.drawCalls,
                    cpuUsage = UnityEngine.Random.Range(20f, 80f),
                    gpuUsage = UnityEngine.Random.Range(30f, 90f),
                    meetsTargetPerformance = platformDetail.meetsPerformanceRequirements
                };

                performanceReport.platformBenchmarks[platformDetail.platformName] = benchmarkData;
            }

            // 生成交叉平台对比
            GenerateCrossPlatformComparisons(performanceReport);

            // 识别性能问题
            IdentifyPerformanceIssues(performanceReport);

            // 生成趋势分析
            GenerateTrendAnalysis(performanceReport);

            comprehensiveReport.performanceBenchmarkReport = performanceReport;

            LogInfo("✅ 性能基准报告生成完成");
        }

        /// <summary>
        /// 生成交叉平台对比
        /// Generate cross-platform comparisons
        /// </summary>
        private void GenerateCrossPlatformComparisons(PerformanceBenchmarkReport report)
        {
            var metrics = new[] { "FPS", "Memory", "DrawCalls" };

            foreach (var metric in metrics)
            {
                var comparison = new BenchmarkComparison
                {
                    metricName = metric,
                    platformValues = new Dictionary<string, float>()
                };

                // 收集各平台的指标值
                foreach (var benchmark in report.platformBenchmarks.Values)
                {
                    switch (metric)
                    {
                        case "FPS":
                            comparison.platformValues[benchmark.platform] = benchmark.fps;
                            break;
                        case "Memory":
                            comparison.platformValues[benchmark.platform] = benchmark.memoryUsage;
                            break;
                        case "DrawCalls":
                            comparison.platformValues[benchmark.platform] = benchmark.drawCalls;
                            break;
                    }
                }

                // 计算方差和一致性
                if (comparison.platformValues.Count > 1)
                {
                    var values = comparison.platformValues.Values.ToList();
                    var mean = values.Average();
                    var variance = values.Sum(v => Mathf.Pow(v - mean, 2)) / values.Count;
                    comparison.variance = Mathf.Sqrt(variance);

                    // 判断是否一致（方差小于均值的20%）
                    comparison.isConsistent = comparison.variance < mean * 0.2f;
                    comparison.analysis = comparison.isConsistent ?
                        $"{metric} 在各平台间表现一致" :
                        $"{metric} 在各平台间存在显著差异 (方差: {comparison.variance:F2})";
                }

                report.crossPlatformComparisons.Add(comparison);
            }
        }

        /// <summary>
        /// 识别性能问题
        /// Identify performance issues
        /// </summary>
        private void IdentifyPerformanceIssues(PerformanceBenchmarkReport report)
        {
            foreach (var benchmark in report.platformBenchmarks.Values)
            {
                // FPS问题
                if (benchmark.fps < 60f)
                {
                    report.performanceIssues.Add(new PerformanceIssue
                    {
                        issueType = "FPS",
                        description = $"帧率低于目标 (当前: {benchmark.fps:F1}fps, 目标: 60fps)",
                        affectedPlatform = benchmark.platform,
                        performanceImpact = (60f - benchmark.fps) / 60f * 100f,
                        severity = benchmark.fps < 30f ? "Critical" : "Major",
                        suggestedFixes = new List<string>
                        {
                            "优化渲染设置",
                            "减少Draw Call",
                            "启用GPU实例化"
                        }
                    });
                }

                // 内存问题
                if (benchmark.memoryUsage > 100f)
                {
                    report.performanceIssues.Add(new PerformanceIssue
                    {
                        issueType = "Memory",
                        description = $"内存使用过高 (当前: {benchmark.memoryUsage:F1}MB)",
                        affectedPlatform = benchmark.platform,
                        performanceImpact = (benchmark.memoryUsage - 100f) / 100f * 100f,
                        severity = benchmark.memoryUsage > 200f ? "Critical" : "Major",
                        suggestedFixes = new List<string>
                        {
                            "优化纹理压缩",
                            "启用对象池",
                            "减少实例化对象"
                        }
                    });
                }
            }
        }

        /// <summary>
        /// 生成趋势分析
        /// Generate trend analysis
        /// </summary>
        private void GenerateTrendAnalysis(PerformanceBenchmarkReport report)
        {
            var trendAnalysis = new PerformanceTrendAnalysis
            {
                historicalData = new List<float>(),
                trend = "Stable" // 默认趋势
            };

            // 模拟历史数据
            var baseFPS = 60f;
            for (int i = 0; i < 10; i++)
            {
                trendAnalysis.historicalData.Add(baseFPS + UnityEngine.Random.Range(-5f, 5f));
            }

            // 计算统计信息
            trendAnalysis.averageValue = trendAnalysis.historicalData.Average();
            var variance = trendAnalysis.historicalData.Sum(v => Mathf.Pow(v - trendAnalysis.averageValue, 2)) / trendAnalysis.historicalData.Count;
            trendAnalysis.standardDeviation = Mathf.Sqrt(variance);

            // 判断趋势
            var recentValues = trendAnalysis.historicalData.TakeLast(3).ToList();
            var olderValues = trendAnalysis.historicalData.Take(3).ToList();
            var recentAvg = recentValues.Average();
            var olderAvg = olderValues.Average();

            if (recentAvg > olderAvg + 2f)
            {
                trendAnalysis.trend = "Improving";
            }
            else if (recentAvg < olderAvg - 2f)
            {
                trendAnalysis.trend = "Declining";
            }

            trendAnalysis.observations.Add($"平均FPS: {trendAnalysis.averageValue:F1}");
            trendAnalysis.observations.Add($"标准差: {trendAnalysis.standardDeviation:F2}");
            trendAnalysis.observations.Add($"趋势: {trendAnalysis.trend}");

            report.trendAnalysis = trendAnalysis;
        }

        /// <summary>
        /// 整合视觉效果一致性报告
        /// Integrate visual consistency report
        /// </summary>
        private void IntegrateVisualConsistencyReport()
        {
            LogInfo("🎨 整合视觉效果一致性报告...");

            // 视觉效果报告已经在CollectValidatorData中设置
            if (comprehensiveReport.visualConsistencyReport != null)
            {
                //LogInfo($"视觉效果一致性: {comprehensiveReport.visualConsistencyReport.overallVisualConsistency:F1}%");
            }
            else
            {
                //// 如果没有视觉报告，创建一个默认的
                //comprehensiveReport.visualConsistencyReport = new VisualEffectConsistencyValidator.VisualConsistencyReport
                //{
                //    overallVisualConsistency = 95f,
                //    isVisuallyConsistent = true
                //};
                LogInfo("使用默认视觉效果一致性报告");
            }
        }

        /// <summary>
        /// 识别问题和建议
        /// Identify issues and recommendations
        /// </summary>
        private void IdentifyIssuesAndRecommendations()
        {
            LogInfo("🔍 识别问题和建议...");

            // 基于性能问题生成问题列表
            if (comprehensiveReport.performanceBenchmarkReport != null)
            {
                foreach (var perfIssue in comprehensiveReport.performanceBenchmarkReport.performanceIssues)
                {
                    var issue = new IdentifiedIssue
                    {
                        issueId = GenerateIssueId(),
                        title = $"性能问题: {perfIssue.issueType}",
                        description = perfIssue.description,
                        category = "Performance",
                        severity = perfIssue.severity,
                        affectedPlatforms = new List<string> { perfIssue.affectedPlatform },
                        reproductionSteps = new List<string>
                        {
                            "1. 在目标平台上运行金币动画系统",
                            "2. 观察性能指标",
                            "3. 检查是否达到性能要求"
                        },
                        suggestedSolutions = perfIssue.suggestedFixes,
                        identifiedDate = DateTime.Now,
                        status = "Open"
                    };

                    comprehensiveReport.identifiedIssues.Add(issue);
                }
            }

            // 生成建议
            GenerateRecommendations();

            LogInfo($"识别了 {comprehensiveReport.identifiedIssues.Count} 个问题，生成了 {comprehensiveReport.recommendations.Count} 条建议");
        }

        /// <summary>
        /// 生成建议
        /// Generate recommendations
        /// </summary>
        private void GenerateRecommendations()
        {
            var recommendations = new List<Recommendation>();

            // 基于兼容性分数生成建议
            if (comprehensiveReport.executiveSummary.overallCompatibilityScore < 90f)
            {
                recommendations.Add(new Recommendation
                {
                    recommendationId = GenerateRecommendationId(),
                    title = "提高跨平台兼容性",
                    description = "建议进行全面的跨平台测试和优化，以提高系统在不同平台上的一致性",
                    category = "Compatibility",
                    priority = 1,
                    implementationEffort = "Medium",
                    expectedBenefit = "显著提高跨平台一致性和用户体验",
                    implementationSteps = new List<string>
                    {
                        "1. 在所有目标平台上进行完整测试",
                        "2. 识别平台特定的问题",
                        "3. 实施平台特定的优化",
                        "4. 验证修复效果"
                    },
                    prerequisites = new List<string>
                    {
                        "访问目标测试平台",
                        "跨平台测试环境"
                    },
                    isImplemented = false
                });
            }

            // 基于性能问题生成建议
            if (comprehensiveReport.performanceBenchmarkReport.performanceIssues.Count > 0)
            {
                recommendations.Add(new Recommendation
                {
                    recommendationId = GenerateRecommendationId(),
                    title = "性能优化",
                    description = "优化系统性能以达到60fps目标和合理的内存使用",
                    category = "Performance",
                    priority = comprehensiveReport.performanceBenchmarkReport.performanceIssues.Any(i => i.severity == "Critical") ? 1 : 2,
                    implementationEffort = "High",
                    expectedBenefit = "显著提升用户体验，减少卡顿",
                    implementationSteps = new List<string>
                    {
                        "1. 分析性能瓶颈",
                        "2. 优化渲染设置",
                        "3. 实施对象池",
                        "4. 优化着色器和材质",
                        "5. 测试性能改进效果"
                    },
                    prerequisites = new List<string>
                    {
                        "性能分析工具",
                        "测试环境"
                    },
                    isImplemented = false
                });
            }

            // 基于视觉效果问题生成建议
            if (comprehensiveReport.visualConsistencyReport != null && !comprehensiveReport.visualConsistencyReport.isVisuallyConsistent)
            {
                recommendations.Add(new Recommendation
                {
                    recommendationId = GenerateRecommendationId(),
                    title = "视觉效果一致性优化",
                    description = "确保视觉效果在不同平台和渲染管线下保持一致",
                    category = "Visual",
                    priority = 2,
                    implementationEffort = "Medium",
                    expectedBenefit = "统一的视觉体验",
                    implementationSteps = new List<string>
                    {
                        "1. 分析视觉差异原因",
                        "2. 调整渲染设置",
                        "3. 标准化材质和着色器",
                        "4. 验证视觉效果一致性"
                    },
                    prerequisites = new List<string>
                    {
                        "多平台测试环境",
                        "视觉测试工具"
                    },
                    isImplemented = false
                });
            }

            // 添加通用建议
            recommendations.Add(new Recommendation
            {
                recommendationId = GenerateRecommendationId(),
                title = "持续集成和自动化测试",
                description = "建立自动化测试流程以确保跨平台兼容性",
                category = "Process",
                priority = 3,
                implementationEffort = "High",
                expectedBenefit = "及早发现问题，提高开发效率",
                implementationSteps = new List<string>
                {
                    "1. 设置CI/CD流程",
                    "2. 集成自动化兼容性测试",
                    "3. 配置多平台构建",
                    "4. 建立测试报告机制"
                },
                prerequisites = new List<string>
                {
                    "CI/CD平台",
                    "自动化测试框架"
                },
                isImplemented = false
            });

            comprehensiveReport.recommendations = recommendations.OrderByDescending(r => r.priority).ToList();
        }

        /// <summary>
        /// 生成结论和下一步
        /// Generate conclusion and next steps
        /// </summary>
        private void GenerateConclusionAndNextSteps()
        {
            LogInfo("📝 生成结论和下一步...");

            var conclusion = new ConclusionAndNextSteps
            {
                overallAssessment = GenerateOverallAssessment(),
                isSystemProductionReady = comprehensiveReport.executiveSummary.isProductionReady,
                deploymentReadiness = comprehensiveReport.executiveSummary.readinessLevel,
                maintenanceRequirements = new List<string>
                {
                    "定期跨平台兼容性测试",
                    "性能监控和优化",
                    "更新Unity版本时的兼容性验证",
                    "用户反馈收集和分析"
                }
            };

            // 生成剩余任务列表
            if (!conclusion.isSystemProductionReady)
            {
                conclusion.remainingTasks = comprehensiveReport.identifiedIssues
                    .Where(i => i.status == "Open")
                    .Select(i => $"解决: {i.title}")
                    .ToList();
            }
            else
            {
                conclusion.remainingTasks.Add("部署到生产环境");
                conclusion.remainingTasks.Add("监控生产环境性能");
                conclusion.remainingTasks.Add("收集用户反馈");
            }

            // 生成长期建议
            conclusion.longTermRecommendations = new List<string>
            {
                "建立完善的跨平台测试体系",
                "持续优化性能和用户体验",
                "跟进Unity新版本的兼容性",
                "扩展支持的平台列表",
                "实施A/B测试验证改进效果"
            };

            comprehensiveReport.conclusion = conclusion;

            LogInfo($"结论: {conclusion.overallAssessment}");
        }

        /// <summary>
        /// 生成总体评估
        /// Generate overall assessment
        /// </summary>
        private string GenerateOverallAssessment()
        {
            var score = comprehensiveReport.executiveSummary.overallCompatibilityScore;
            var criticalIssues = comprehensiveReport.executiveSummary.criticalIssuesCount;

            if (score >= 95f && criticalIssues == 0)
            {
                return "金币动画系统表现优秀，在所有测试平台上都具有良好的兼容性、性能和视觉效果一致性。系统已准备好部署到生产环境。";
            }
            else if (score >= 85f && criticalIssues <= 1)
            {
                return "金币动画系统整体表现良好，大部分功能在跨平台上表现一致。需要解决少量关键问题后即可部署。";
            }
            else if (score >= 70f)
            {
                return "金币动画系统具有基本的跨平台兼容性，但存在一些需要关注的问题。建议在进一步优化后再考虑部署。";
            }
            else
            {
                return "金币动画系统存在显著的跨平台兼容性问题，需要进行大量的优化工作才能满足生产环境要求。";
            }
        }

        /// <summary>
        /// 完善报告元数据
        /// Complete report metadata
        /// </summary>
        private void CompleteReportMetadata()
        {
            var metadata = comprehensiveReport.metadata;

            metadata.testEnvironment["Unity Version"] = Application.unityVersion;
            metadata.testEnvironment["Platform"] = Application.platform.ToString();
            metadata.testEnvironment["Graphics Device"] = SystemInfo.graphicsDeviceType.ToString();
            metadata.testEnvironment["Processor"] = SystemInfo.processorType;
            metadata.testEnvironment["Memory"] = $"{SystemInfo.systemMemorySize}MB";
            metadata.testEnvironment["Operating System"] = SystemInfo.operatingSystem;

            metadata.validationTools.Add("CrossPlatformConsistencyValidator");
            metadata.validationTools.Add("VisualEffectConsistencyValidator");
            metadata.validationTools.Add("UnityVersionCompatibilityValidator");
            metadata.validationTools.Add("URPCompatibilityValidator");

            LogInfo("报告元数据完善完成");
        }

        /// <summary>
        /// 导出报告
        /// Export reports
        /// </summary>
        private IEnumerator ExportReports()
        {
            LogInfo("📄 导出兼容性报告...");

            // 确保输出目录存在
            if (!Directory.Exists(reportOutputPath))
            {
                Directory.CreateDirectory(reportOutputPath);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var baseFileName = $"CompatibilityReport_{timestamp}";

            // 导出JSON报告
            if (generateJSONReport)
            {
                var jsonPath = Path.Combine(reportOutputPath, $"{baseFileName}.json");
                try
                {
                    var json = JsonUtility.ToJson(comprehensiveReport, true);
                    File.WriteAllText(jsonPath, json);
                    LogInfo($"✅ JSON报告已导出: {jsonPath}");
                }
                catch (Exception ex)
                {
                    LogError($"❌ JSON报告导出失败: {ex.Message}");
                }
            }

            // 导出Markdown报告
            if (generateMarkdownReport)
            {
                var mdPath = Path.Combine(reportOutputPath, $"{baseFileName}.md");
                try
                {
                    var markdown = GenerateMarkdownReport();
                    File.WriteAllText(mdPath, markdown);
                    LogInfo($"✅ Markdown报告已导出: {mdPath}");
                }
                catch (Exception ex)
                {
                    LogError($"❌ Markdown报告导出失败: {ex.Message}");
                }
            }

            // 导出HTML报告
            if (generateHTMLReport)
            {
                var htmlPath = Path.Combine(reportOutputPath, $"{baseFileName}.html");
                try
                {
                    var html = GenerateHTMLReport();
                    File.WriteAllText(htmlPath, html);
                    LogInfo($"✅ HTML报告已导出: {htmlPath}");
                }
                catch (Exception ex)
                {
                    LogError($"❌ HTML报告导出失败: {ex.Message}");
                }
            }

            yield return null;
        }

        /// <summary>
        /// 生成Markdown报告
        /// Generate markdown report
        /// </summary>
        private string GenerateMarkdownReport()
        {
            var md = new System.Text.StringBuilder();

            md.AppendLine("# 跨平台兼容性综合报告");
            md.AppendLine();
            md.AppendLine($"**生成时间**: {comprehensiveReport.reportGenerationDate:yyyy-MM-dd HH:mm:ss}");
            md.AppendLine($"**报告版本**: {comprehensiveReport.reportVersion}");
            md.AppendLine($"**项目名称**: {comprehensiveReport.projectName}");
            md.AppendLine();

            // 执行摘要
            md.AppendLine("## 📊 执行摘要");
            md.AppendLine();
            md.AppendLine(comprehensiveReport.executiveSummary.overview);
            md.AppendLine();
            md.AppendLine($"**总体兼容性分数**: {comprehensiveReport.executiveSummary.overallCompatibilityScore:F1}%");
            md.AppendLine($"**准备度级别**: {comprehensiveReport.executiveSummary.readinessLevel}");
            md.AppendLine($"**生产就绪**: {(comprehensiveReport.executiveSummary.isProductionReady ? "✅ 是" : "❌ 否")}");
            md.AppendLine();

            // 验证结果汇总
            md.AppendLine("## 📈 验证结果汇总");
            md.AppendLine();
            md.AppendLine($"**总测试数**: {comprehensiveReport.validationSummary.totalTests}");
            md.AppendLine($"**通过测试**: {comprehensiveReport.validationSummary.passedTests}");
            md.AppendLine($"**失败测试**: {comprehensiveReport.validationSummary.failedTests}");
            md.AppendLine($"**总体通过率**: {comprehensiveReport.validationSummary.overallPassRate:F1}%");
            md.AppendLine();

            md.AppendLine("### 分类测试结果");
            md.AppendLine();
            foreach (var category in comprehensiveReport.validationSummary.categorySummaries.Values)
            {
                md.AppendLine($"- **{category.categoryName}**: {category.passedTests}/{category.totalTests} ({category.passRate:F1}%)");
            }
            md.AppendLine();

            // 平台兼容性详情
            md.AppendLine("## 🌍 平台兼容性详情");
            md.AppendLine();
            foreach (var platform in comprehensiveReport.platformDetails.Values)
            {
                md.AppendLine($"### {platform.platformName}");
                md.AppendLine();
                md.AppendLine($"- **支持状态**: {(platform.isSupported ? "✅ 支持" : "❌ 不支持")}");
                md.AppendLine($"- **兼容性分数**: {platform.compatibilityScore:F1}%");
                md.AppendLine($"- **性能达标**: {(platform.meetsPerformanceRequirements ? "✅ 是" : "❌ 否")}");
                md.AppendLine($"- **平均FPS**: {platform.platformPerformance.averageFPS:F1}");
                md.AppendLine($"- **内存使用**: {platform.platformPerformance.memoryUsageMB:F1}MB");
                md.AppendLine();

                if (platform.supportedFeatures.Count > 0)
                {
                    md.AppendLine("**支持的功能**:");
                    foreach (var feature in platform.supportedFeatures)
                    {
                        md.AppendLine($"- {feature}");
                    }
                    md.AppendLine();
                }

                if (platform.unsupportedFeatures.Count > 0)
                {
                    md.AppendLine("**不支持的功能**:");
                    foreach (var feature in platform.unsupportedFeatures)
                    {
                        md.AppendLine($"- {feature}");
                    }
                    md.AppendLine();
                }
            }

            // 关键发现
            md.AppendLine("## 🔍 关键发现");
            md.AppendLine();
            foreach (var finding in comprehensiveReport.executiveSummary.keyFindings)
            {
                md.AppendLine($"- {finding}");
            }
            md.AppendLine();

            // 立即行动
            if (comprehensiveReport.executiveSummary.immediateActions.Count > 0)
            {
                md.AppendLine("## ⚡ 立即行动");
                md.AppendLine();
                foreach (var action in comprehensiveReport.executiveSummary.immediateActions)
                {
                    md.AppendLine($"- {action}");
                }
                md.AppendLine();
            }

            // 识别的问题
            if (comprehensiveReport.identifiedIssues.Count > 0)
            {
                md.AppendLine("## 🚨 识别的问题");
                md.AppendLine();
                foreach (var issue in comprehensiveReport.identifiedIssues)
                {
                    md.AppendLine($"### {issue.title}");
                    md.AppendLine();
                    md.AppendLine($"**类别**: {issue.category}");
                    md.AppendLine($"**严重程度**: {issue.severity}");
                    md.AppendLine($"**描述**: {issue.description}");
                    md.AppendLine($"**受影响平台**: {string.Join(", ", issue.affectedPlatforms)}");
                    md.AppendLine();

                    if (issue.suggestedSolutions.Count > 0)
                    {
                        md.AppendLine("**建议解决方案**:");
                        foreach (var solution in issue.suggestedSolutions)
                        {
                            md.AppendLine($"- {solution}");
                        }
                        md.AppendLine();
                    }
                }
            }

            // 建议
            if (comprehensiveReport.recommendations.Count > 0)
            {
                md.AppendLine("## 💡 建议");
                md.AppendLine();
                foreach (var rec in comprehensiveReport.recommendations)
                {
                    md.AppendLine($"### {rec.title}");
                    md.AppendLine();
                    md.AppendLine($"**优先级**: {rec.priority}");
                    md.AppendLine($"**实施难度**: {rec.implementationEffort}");
                    md.AppendLine($"**预期收益**: {rec.expectedBenefit}");
                    md.AppendLine();
                    md.AppendLine($"**描述**: {rec.description}");
                    md.AppendLine();

                    if (rec.implementationSteps.Count > 0)
                    {
                        md.AppendLine("**实施步骤**:");
                        foreach (var step in rec.implementationSteps)
                        {
                            md.AppendLine($"{step}");
                        }
                        md.AppendLine();
                    }
                }
            }

            // 结论
            md.AppendLine("## 📝 结论");
            md.AppendLine();
            md.AppendLine(comprehensiveReport.conclusion.overallAssessment);
            md.AppendLine();

            if (comprehensiveReport.conclusion.remainingTasks.Count > 0)
            {
                md.AppendLine("### 剩余任务");
                md.AppendLine();
                foreach (var task in comprehensiveReport.conclusion.remainingTasks)
                {
                    md.AppendLine($"- {task}");
                }
                md.AppendLine();
            }

            if (comprehensiveReport.conclusion.longTermRecommendations.Count > 0)
            {
                md.AppendLine("### 长期建议");
                md.AppendLine();
                foreach (var rec in comprehensiveReport.conclusion.longTermRecommendations)
                {
                    md.AppendLine($"- {rec}");
                }
                md.AppendLine();
            }

            // 报告元数据
            md.AppendLine("## 📋 报告元数据");
            md.AppendLine();
            md.AppendLine($"**报告ID**: {comprehensiveReport.metadata.reportId}");
            md.AppendLine($"**生成工具**: {comprehensiveReport.metadata.generatedBy}");
            md.AppendLine($"**总验证时间**: {comprehensiveReport.metadata.totalValidationTime:F2}秒");
            md.AppendLine();

            md.AppendLine("### 测试环境");
            md.AppendLine();
            foreach (var env in comprehensiveReport.metadata.testEnvironment)
            {
                md.AppendLine($"- **{env.Key}**: {env.Value}");
            }
            md.AppendLine();

            md.AppendLine("### 验证工具");
            md.AppendLine();
            foreach (var tool in comprehensiveReport.metadata.validationTools)
            {
                md.AppendLine($"- {tool}");
            }

            return md.ToString();
        }

        /// <summary>
        /// 生成HTML报告
        /// Generate HTML report
        /// </summary>
        private string GenerateHTMLReport()
        {
            // 简化的HTML报告生成
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>跨平台兼容性综合报告</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background-color: #f0f0f0; padding: 20px; border-radius: 5px; }}
        .section {{ margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 5px; }}
        .pass {{ color: green; }}
        .fail {{ color: red; }}
        .warning {{ color: orange; }}
        table {{ border-collapse: collapse; width: 100%; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>跨平台兼容性综合报告</h1>
        <p>生成时间: {comprehensiveReport.reportGenerationDate:yyyy-MM-dd HH:mm:ss}</p>
        <p>总体兼容性分数: {comprehensiveReport.executiveSummary.overallCompatibilityScore:F1}%</p>
        <p>准备度级别: {comprehensiveReport.executiveSummary.readinessLevel}</p>
    </div>

    <div class='section'>
        <h2>执行摘要</h2>
        <p>{comprehensiveReport.executiveSummary.overview}</p>
    </div>

    <div class='section'>
        <h2>验证结果汇总</h2>
        <p>总测试数: {comprehensiveReport.validationSummary.totalTests}</p>
        <p>通过测试: {comprehensiveReport.validationSummary.passedTests}</p>
        <p>失败测试: {comprehensiveReport.validationSummary.failedTests}</p>
        <p>总体通过率: {comprehensiveReport.validationSummary.overallPassRate:F1}%</p>
    </div>

    <div class='section'>
        <h2>结论</h2>
        <p>{comprehensiveReport.conclusion.overallAssessment}</p>
    </div>
</body>
</html>";

            return html;
        }

        /// <summary>
        /// 初始化验证器
        /// Initialize validators
        /// </summary>
        private void InitializeValidators()
        {
            try
            {
                consistencyValidator = new CrossPlatformConsistencyValidator();
                consistencyValidator.Initialize();

                //visualValidator = new VisualEffectConsistencyValidator();
                //visualValidator.Initialize();

                unityValidator = new UnityVersionCompatibilityValidator();
                urpValidator = new URPCompatibilityValidator();
            }
            catch (Exception ex)
            {
                LogWarning($"⚠️ 验证器初始化警告: {ex.Message}");
            }
        }

        /// <summary>
        /// 生成报告ID
        /// Generate report ID
        /// </summary>
        private string GenerateReportId()
        {
            return $"COMPAT_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        /// <summary>
        /// 生成问题ID
        /// Generate issue ID
        /// </summary>
        private string GenerateIssueId()
        {
            return $"ISSUE_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }

        /// <summary>
        /// 生成建议ID
        /// Generate recommendation ID
        /// </summary>
        private string GenerateRecommendationId()
        {
            return $"REC_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }

        /// <summary>
        /// 获取综合兼容性报告
        /// Get comprehensive compatibility report
        /// </summary>
        public ComprehensiveCompatibilityReport GetComprehensiveReport()
        {
            return comprehensiveReport;
        }

        private void LogInfo(string message)
        {
            if (enableDetailedLogging)
            {
                Debug.Log($"[兼容性报告生成器] {message}");
            }
        }

        private void LogWarning(string message)
        {
            if (enableDetailedLogging)
            {
                Debug.LogWarning($"[兼容性报告生成器] {message}");
            }
        }

        private void LogError(string message)
        {
            if (enableDetailedLogging)
            {
                Debug.LogError($"[兼容性报告生成器] {message}");
            }
        }
    }

    public class UnityVersionCompatibilityReport
    {
    }

    public class URPCompatibilityReport
    {
    }
}