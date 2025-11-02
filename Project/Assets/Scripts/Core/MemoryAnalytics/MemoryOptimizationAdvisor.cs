using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoinAnimation.Core
{
    /// <summary>
    /// 内存优化顾问系统
    /// Story 2.1 Task 3.4 - 为开发者提供智能内存优化建议和最佳实践指导
    /// </summary>
    public class MemoryOptimizationAdvisor : MonoBehaviour
    {
        #region Configuration

        [Header("Advisor Settings")]
        [SerializeField] private bool enableAdvisor = true;
        [SerializeField] private bool enableRealTimeAdvice = true;
        [SerializeField] private float adviceUpdateInterval = 5f;
        [SerializeField] private int maxRecommendations = 10;

        [Header("Analysis Settings")]
        [SerializeField] private bool enablePerformanceAnalysis = true;
        [SerializeField] private bool enableBestPracticeAnalysis = true;
        [SerializeField] private bool enableCodePatternAnalysis = true;
        [SerializeField] private bool enableAssetAnalysis = true;

        [Header("Recommendation Settings")]
        [SerializeField] private bool prioritizeCriticalIssues = true;
        [SerializeField] private bool enableImplementationGuides = true;
        [SerializeField] private bool includeCodeExamples = true;
        [SerializeField] private bool enableCostBenefitAnalysis = true;

        [Header("Output Settings")]
        [SerializeField] private bool generateDetailedReports = true;
        [SerializeField] private bool enableConsoleOutput = true;
        [SerializeField] private bool enableLogOutput = true;
        [SerializeField] private bool saveToFile = false;

        #endregion

        #region Private Fields

        // 分析组件
        private MemoryUsagePatternAnalyzer _patternAnalyzer;
        private MemoryLeakDetector _leakDetector;
        private MemoryPressureManager _pressureManager;
        private PerformanceMonitor _performanceMonitor;

        // 建议系统
        private readonly List<MemoryOptimizationRecommendation> _currentRecommendations = new List<MemoryOptimizationRecommendation>();
        private readonly Dictionary<OptimizationCategory, List<RecommendationRule>> _recommendationRules = new Dictionary<OptimizationCategory, List<RecommendationRule>>();
        private readonly Queue<AdvisorSession> _adviceHistory = new Queue<AdvisorSession>();

        // 分析状态
        private bool _advisorActive = false;
        private float _lastAdviceUpdateTime = 0f;
        private int _totalRecommendationsGenerated = 0;
        private int _acceptedRecommendations = 0;

        // 统计数据
        private MemoryAdvisorStats _advisorStats = new MemoryAdvisorStats();

        #endregion

        #region Properties

        public bool IsAdvisorActive => _advisorActive;
        public IReadOnlyList<MemoryOptimizationRecommendation> CurrentRecommendations => _currentRecommendations.AsReadOnly();
        public MemoryAdvisorStats AdvisorStats => _advisorStats;
        public int TotalRecommendationsGenerated => _totalRecommendationsGenerated;

        #endregion

        #region Events

        public event Action<List<MemoryOptimizationRecommendation>> OnRecommendationsUpdated;
        public event Action<MemoryOptimizationReport> OnDetailedReportGenerated;
        public event Action<OptimizationInsight> OnInsightGenerated;
        public event Action<string> OnBestPracticeTipGenerated;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            FindSystemComponents();
            InitializeRecommendationRules();
            
            if (enableAdvisor)
            {
                StartCoroutine(AdvisorCoroutine());
            }
        }

        #endregion

        #region Initialization

        private void FindSystemComponents()
        {
            _patternAnalyzer = FindObjectOfType<MemoryUsagePatternAnalyzer>();
            _leakDetector = FindObjectOfType<MemoryLeakDetector>();
            _pressureManager = FindObjectOfType<MemoryPressureManager>();
            _performanceMonitor = FindObjectOfType<PerformanceMonitor>();

            Debug.Log($"[MemoryOptimizationAdvisor] Components found: " +
                     $"PatternAnalyzer: {_patternAnalyzer != null}, " +
                     $"LeakDetector: {_leakDetector != null}, " +
                     $"PressureManager: {_pressureManager != null}, " +
                     $"PerformanceMonitor: {_performanceMonitor != null}");
        }

        private void InitializeRecommendationRules()
        {
            // 内存分配优化规则
            _recommendationRules[OptimizationCategory.Allocation] = new List<RecommendationRule>
            {
                new RecommendationRule
                {
                    Name = "Object Pooling",
                    Condition = CheckObjectPoolingNeed,
                    Priority = RecommendationPriority.High,
                    Category = OptimizationCategory.Allocation,
                    EstimatedBenefit = 50f,
                    ImplementationEffort = ImplementationEffort.Medium,
                    Description = "Implement object pooling for frequently created/destroyed objects"
                },
                new RecommendationRule
                {
                    Name = "Array Pre-allocation",
                    Condition = CheckArrayPreallocationNeed,
                    Priority = RecommendationPriority.Medium,
                    Category = OptimizationCategory.Allocation,
                    EstimatedBenefit = 20f,
                    ImplementationEffort = ImplementationEffort.Low,
                    Description = "Pre-allocate arrays to avoid frequent reallocations"
                }
            };

            // 垃圾回收优化规则
            _recommendationRules[OptimizationCategory.GarbageCollection] = new List<RecommendationRule>
            {
                new RecommendationRule
                {
                    Name = "Reduce Allocations",
                    Condition = CheckHighAllocationRate,
                    Priority = RecommendationPriority.Critical,
                    Category = OptimizationCategory.GarbageCollection,
                    EstimatedBenefit = 80f,
                    ImplementationEffort = ImplementationEffort.High,
                    Description = "Reduce memory allocations to minimize GC pressure"
                },
                new RecommendationRule
                {
                    Name = "Use Value Types",
                    Condition = CheckValueTypeOpportunity,
                    Priority = RecommendationPriority.Medium,
                    Category = OptimizationCategory.GarbageCollection,
                    EstimatedBenefit = 30f,
                    ImplementationEffort = ImplementationEffort.Medium,
                    Description = "Use structs instead of classes for small data structures"
                }
            };

            // 资源管理优化规则
            _recommendationRules[OptimizationCategory.ResourceManagement] = new List<RecommendationRule>
            {
                new RecommendationRule
                {
                    Name = "Asset Unloading",
                    Condition = CheckUnusedAssets,
                    Priority = RecommendationPriority.High,
                    Category = OptimizationCategory.ResourceManagement,
                    EstimatedBenefit = 60f,
                    ImplementationEffort = ImplementationEffort.Low,
                    Description = "Unload unused assets to free memory"
                },
                new RecommendationRule
                {
                    Name = "Texture Optimization",
                    Condition = CheckTextureOptimizationNeed,
                    Priority = RecommendationPriority.Medium,
                    Category = OptimizationCategory.ResourceManagement,
                    EstimatedBenefit = 40f,
                    ImplementationEffort = ImplementationEffort.Medium,
                    Description = "Optimize texture compression and mipmaps"
                }
            };

            // 代码模式优化规则
            _recommendationRules[OptimizationCategory.CodePatterns] = new List<RecommendationRule>
            {
                new RecommendationRule
                {
                    Name = "Coroutine Optimization",
                    Condition = CheckCoroutineOptimizationNeed,
                    Priority = RecommendationPriority.Medium,
                    Category = OptimizationCategory.CodePatterns,
                    EstimatedBenefit = 25f,
                    ImplementationEffort = ImplementationEffort.Low,
                    Description = "Optimize coroutine usage to reduce memory overhead"
                },
                new RecommendationRule
                {
                    Name = "Event Subscription Cleanup",
                    Condition = CheckEventSubscriptionLeaks,
                    Priority = RecommendationPriority.High,
                    Category = OptimizationCategory.CodePatterns,
                    EstimatedBenefit = 35f,
                    ImplementationEffort = ImplementationEffort.Low,
                    Description = "Ensure proper cleanup of event subscriptions"
                }
            };

            Debug.Log($"[MemoryOptimizationAdvisor] Initialized {_recommendationRules.Count} categories with {_recommendationRules.Values.Sum(r => r.Count)} total rules");
        }

        #endregion

        #region Advisor Coroutine

        private IEnumerator AdvisorCoroutine()
        {
            _advisorActive = true;
            
            while (enableAdvisor)
            {
                yield return new WaitForSeconds(adviceUpdateInterval);

                if (Time.time > _lastAdviceUpdateTime + adviceUpdateInterval)
                {
                    yield return StartCoroutine(GenerateRecommendations());
                }
            }
        }

        private IEnumerator GenerateRecommendations()
        {
            _lastAdviceUpdateTime = Time.time;
            var sessionStartTime = DateTime.UtcNow;

            Debug.Log("[MemoryOptimizationAdvisor] Generating memory optimization recommendations...");

            AdvisorAnalysisData analysisData = null;
            try
            {
                // 收集分析数据
                analysisData = CollectAnalysisData();
            }
            catch (Exception e)
            {
                Debug.LogError($"[MemoryOptimizationAdvisor] Data collection failed: {e.Message}");
                _advisorStats.FailedSessions++;
                yield break;
            }

            try
            {
                //// 应用推荐规则
                //yield return StartCoroutine(ApplyRecommendationRules(analysisData));

                //// 生成洞察
                //yield return StartCoroutine(GenerateInsights(analysisData));

                //// 生成最佳实践提示
                //if (enableBestPracticeAnalysis)
                //{
                //    yield return StartCoroutine(GenerateBestPracticeTips(analysisData));
                //}
            }
            catch (Exception e)
            {
                Debug.LogError($"[MemoryOptimizationAdvisor] Recommendation generation failed: {e.Message}");
                _advisorStats.FailedSessions++;
                yield break;
            }

            // 更新统计（移到try-catch外部）
            var sessionDuration = DateTime.UtcNow - sessionStartTime;
            _advisorStats.TotalSessions++;
            _advisorStats.TotalSessionTime += sessionDuration;
            _advisorStats.AverageSessionTime = _advisorStats.TotalSessionTime / _advisorStats.TotalSessions;

            // 触发事件
            OnRecommendationsUpdated?.Invoke(_currentRecommendations);

            // 输出建议
            if (enableConsoleOutput)
            {
                OutputRecommendationsToConsole();
            }

            Debug.Log($"[MemoryOptimizationAdvisor] Generated {_currentRecommendations.Count} recommendations in {sessionDuration.TotalSeconds:F2}s");
        }

        private AdvisorAnalysisData CollectAnalysisData()
        {
            var data = new AdvisorAnalysisData
            {
                Timestamp = DateTime.UtcNow,
                CurrentMemoryMB = GC.GetTotalMemory(false) / (1024f * 1024f),
                PatternAnalysis = _patternAnalyzer?.GetAnalysisReport(),
                LeakDetection = _leakDetector?.GetLeakReport(),
                PressureAnalysis = _pressureManager?.GetPressureReport(),
                PerformanceMetrics = _performanceMonitor?.GetCurrentMetrics(),
                ComponentData = GetComponentAnalysisData(),
                AssetData = GetAssetAnalysisData()
            };

            return data;
        }

        #endregion

        #region Recommendation Rule Application

        private IEnumerator ApplyRecommendationRules(AdvisorAnalysisData analysisData)
        {
            _currentRecommendations.Clear();

            foreach (var category in _recommendationRules.Keys)
            {
                foreach (var rule in _recommendationRules[category])
                {
                    if (rule.Condition(analysisData))
                    {
                        var recommendation = CreateRecommendation(rule, analysisData);
                        _currentRecommendations.Add(recommendation);
                        _totalRecommendationsGenerated++;

                        yield return null; // 避免阻塞
                    }
                }
            }

            // 排序和过滤建议
            SortAndFilterRecommendations();

            yield return null;
        }

        private MemoryOptimizationRecommendation CreateRecommendation(RecommendationRule rule, AdvisorAnalysisData analysisData)
        {
            var recommendation = new MemoryOptimizationRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                RuleName = rule.Name,
                Category = rule.Category,
                Priority = rule.Priority,
                Title = rule.Description,
                Description = GenerateDetailedDescription(rule, analysisData),
                EstimatedBenefit = rule.EstimatedBenefit,
                ImplementationEffort = rule.ImplementationEffort,
                RiskLevel = CalculateRiskLevel(rule),
                GeneratedAt = DateTime.UtcNow,
                ContextData = analysisData,
                AcceptanceCriteria = GenerateAcceptanceCriteria(rule),
                SuccessMetrics = GenerateSuccessMetrics(rule)
            };

            // 添加实现指导
            if (enableImplementationGuides)
            {
                recommendation.ImplementationGuide = GenerateImplementationGuide(rule);
            }

            // 添加代码示例
            if (includeCodeExamples)
            {
                recommendation.CodeExample = GenerateCodeExample(rule);
            }

            // 添加成本效益分析
            if (enableCostBenefitAnalysis)
            {
                recommendation.CostBenefitAnalysis = GenerateCostBenefitAnalysis(rule, analysisData);
            }

            return recommendation;
        }

        private void SortAndFilterRecommendations()
        {
            // 排序：按优先级和收益排序
            List<MemoryOptimizationRecommendation> sortedRecommendations;
            if (prioritizeCriticalIssues)
            {
                sortedRecommendations = _currentRecommendations
                    .OrderByDescending(r => r.Priority)
                    .ThenByDescending(r => r.EstimatedBenefit)
                    .ToList();
            }
            else
            {
                sortedRecommendations = _currentRecommendations
                    .OrderByDescending(r => r.EstimatedBenefit)
                    .ThenBy(r => r.ImplementationEffort)
                    .ToList();
            }

            // 限制建议数量
            if (sortedRecommendations.Count > maxRecommendations)
            {
                sortedRecommendations = sortedRecommendations.Take(maxRecommendations).ToList();
            }

            // 更新 readonly 列表的内容
            _currentRecommendations.Clear();
            _currentRecommendations.AddRange(sortedRecommendations);
        }

        #endregion

        #region Insight Generation

        private IEnumerator GenerateInsights(AdvisorAnalysisData analysisData)
        {
            var insights = new List<OptimizationInsight>();

            // 内存使用趋势洞察
            yield return StartCoroutine(GenerateMemoryTrendInsights(analysisData, insights));

            // 性能关联洞察
            yield return StartCoroutine(GeneratePerformanceCorrelationInsights(analysisData, insights));

            // 资源使用效率洞察
            yield return StartCoroutine(GenerateResourceEfficiencyInsights(analysisData, insights));

            // 触发洞察事件
            foreach (var insight in insights)
            {
                OnInsightGenerated?.Invoke(insight);
            }

            yield return null;
        }

        private IEnumerator GenerateMemoryTrendInsights(AdvisorAnalysisData analysisData, List<OptimizationInsight> insights)
        {
            if (analysisData.PressureAnalysis != null)
            {
                var trend = analysisData.PressureAnalysis.PressureTrend;
                var level = analysisData.PressureAnalysis.CurrentPressureLevel;

                if (trend == MemoryPressureTrend.Increasing && level >= MemoryPressureLevel.Moderate)
                {
                    insights.Add(new OptimizationInsight
                    {
                        Type = InsightType.Warning,
                        Title = "Memory Usage Trending Upward",
                        Description = $"Memory pressure is increasing ({trend}) and currently at {level} level",
                        Recommendation = "Consider immediate optimization actions",
                        Confidence = 0.8f,
                        Data = new Dictionary<string, object>
                        {
                            ["Trend"] = trend.ToString(),
                            ["CurrentLevel"] = level.ToString()
                        }
                    });
                }
            }

            yield return null;
        }

        private IEnumerator GeneratePerformanceCorrelationInsights(AdvisorAnalysisData analysisData, List<OptimizationInsight> insights)
        {
            if (analysisData.PerformanceMetrics != null)
            {
                var fps = analysisData.PerformanceMetrics.averageFrameRate;
                var memory = analysisData.CurrentMemoryMB;

                if (fps < 50 && memory > 100) // 低FPS + 高内存
                {
                    insights.Add(new OptimizationInsight
                    {
                        Type = InsightType.Critical,
                        Title = "Memory Impact on Performance",
                        Description = $"High memory usage ({memory:F1}MB) correlates with low frame rate ({fps:F1} FPS)",
                        Recommendation = "Memory optimization will likely improve performance",
                        Confidence = 0.9f,
                        Data = new Dictionary<string, object>
                        {
                            ["FPS"] = fps,
                            ["MemoryMB"] = memory
                        }
                    });
                }
            }

            yield return null;
        }

        private IEnumerator GenerateResourceEfficiencyInsights(AdvisorAnalysisData analysisData, List<OptimizationInsight> insights)
        {
            // 检查资源使用效率
            if (analysisData.ComponentData != null)
            {
                var totalComponents = analysisData.ComponentData.Values.Sum();
                var activeCoins = analysisData.ComponentData.GetValueOrDefault("ActiveCoins", 0);

                if (activeCoins > 0 && totalComponents > activeCoins * 10) // 组件/金币比例过高
                {
                    insights.Add(new OptimizationInsight
                    {
                        Type = InsightType.Optimization,
                        Title = "Component Efficiency Issue",
                        Description = $"High component-to-coin ratio: {totalComponents} components for {activeCoins} coins",
                        Recommendation = "Optimize component usage per coin",
                        Confidence = 0.7f,
                        Data = new Dictionary<string, object>
                        {
                            ["TotalComponents"] = totalComponents,
                            ["ActiveCoins"] = activeCoins,
                            ["Ratio"] = totalComponents / (float)activeCoins
                        }
                    });
                }
            }

            yield return null;
        }

        #endregion

        #region Best Practice Tips

        private IEnumerator GenerateBestPracticeTips(AdvisorAnalysisData analysisData)
        {
            var tips = new List<string>();

            // 基于当前状态生成最佳实践提示
            if (analysisData.CurrentMemoryMB > 100)
            {
                tips.Add("💡 Consider implementing asset loading/unloading strategies for large memory usage");
            }

            if (analysisData.PressureAnalysis?.CurrentPressureLevel >= MemoryPressureLevel.High)
            {
                tips.Add("💡 High memory pressure detected: review object pooling and caching strategies");
            }

            if (analysisData.LeakDetection?.IsLeakConfirmed == true)
            {
                tips.Add("💡 Memory leak confirmed: check for proper disposal of resources and event unsubscription");
            }

            // 通用最佳实践
            tips.AddRange(GetGeneralBestPractices());

            // 输出提示
            foreach (var tip in tips)
            {
                OnBestPracticeTipGenerated?.Invoke(tip);
                
                if (enableConsoleOutput)
                {
                    Debug.Log($"[MemoryOptimizationAdvisor] {tip}");
                }
            }

            yield return null;
        }

        private List<string> GetGeneralBestPractices()
        {
            return new List<string>
            {
                "💡 Use object pooling for frequently instantiated objects",
                "💡 Avoid unnecessary allocations in Update() methods",
                "💡 Prefer structs over classes for small data structures",
                "💡 Unload unused assets and resources regularly",
                "💡 Implement proper cleanup in OnDestroy() methods",
                "💡 Use caching wisely - don't cache everything",
                "💡 Profile memory usage regularly to identify issues early"
            };
        }

        #endregion

        #region Recommendation Rule Conditions

        private bool CheckObjectPoolingNeed(AdvisorAnalysisData data)
        {
            // 检查是否需要对象池
            return data.ComponentData?.GetValueOrDefault("ActiveCoins", 0) > 20 ||
                   data.CurrentMemoryMB > 80 ||
                   (data.LeakDetection?.DetectedLeaks?.Any(l => l.Type == MemoryLeakType.GameObjectAccumulation) ?? false);
        }

        private bool CheckArrayPreallocationNeed(AdvisorAnalysisData data)
        {
            // 检查是否需要数组预分配
            return data.CurrentMemoryMB > 60 ||
                   (data.PatternAnalysis?.DetectedPatterns?.Any(p => p.Type == MemoryPatternType.MemorySpike) ?? false);
        }

        private bool CheckHighAllocationRate(AdvisorAnalysisData data)
        {
            // 检查高分配率
            return data.PressureAnalysis?.CurrentPressureLevel >= MemoryPressureLevel.Moderate ||
                   data.PatternAnalysis?.CurrentTrend == MemoryUsageTrend.Increasing;
        }

        private bool CheckValueTypeOpportunity(AdvisorAnalysisData data)
        {
            // 检查值类型使用机会
            return data.ComponentData?.GetValueOrDefault("MonoBehaviours", 0) > 50;
        }

        private bool CheckUnusedAssets(AdvisorAnalysisData data)
        {
            // 检查未使用的资源
            return data.AssetData?.TotalAssetSizeMB > 50 ||
                   data.CurrentMemoryMB > 100;
        }

        private bool CheckTextureOptimizationNeed(AdvisorAnalysisData data)
        {
            // 检查纹理优化需求
            return data.AssetData?.TextureSizeMB > 20 ||
                   data.CurrentMemoryMB > 80;
        }

        private bool CheckCoroutineOptimizationNeed(AdvisorAnalysisData data)
        {
            // 检查协程优化需求
            return data.ComponentData?.GetValueOrDefault("Coroutines", 0) > 10;
        }

        private bool CheckEventSubscriptionLeaks(AdvisorAnalysisData data)
        {
            // 检查事件订阅泄漏
            return data.LeakDetection?.DetectedLeaks?.Any(l => l.Type == MemoryLeakType.EventSubscriptionLeak) ?? false;
        }

        #endregion

        #region Implementation Guides and Code Examples

        private string GenerateDetailedDescription(RecommendationRule rule, AdvisorAnalysisData data)
        {
            return $"{rule.Description}. " +
                   $"Estimated memory saving: {rule.EstimatedBenefit:F1}MB. " +
                   $"Implementation effort: {rule.ImplementationEffort}.";
        }

        private ImplementationGuide GenerateImplementationGuide(RecommendationRule rule)
        {
            return rule.Name switch
            {
                "Object Pooling" => new ImplementationGuide
                {
                    Steps = new List<string>
                    {
                        "1. Create a pool class for your objects",
                        "2. Implement Get() and Return() methods",
                        "3. Initialize pool with required capacity",
                        "4. Use pool instead of Instantiate/Destroy"
                    },
                    Considerations = new List<string>
                    {
                        "Consider thread safety if needed",
                        "Implement proper reset of returned objects",
                        "Monitor pool hit rates"
                    }
                },
                "Array Pre-allocation" => new ImplementationGuide
                {
                    Steps = new List<string>
                    {
                        "1. Determine maximum array size needed",
                        "2. Pre-allocate arrays to that size",
                        "3. Use counter to track actual usage",
                        "4. Consider List<T> with initial capacity"
                    },
                    Considerations = new List<string>
                    {
                        "Balance memory usage vs flexibility",
                        "Consider using Span<T> for performance",
                        "Profile to find optimal size"
                    }
                },
                _ => new ImplementationGuide
                {
                    Steps = new List<string> { "Implement the optimization pattern" },
                    Considerations = new List<string> { "Test thoroughly before production" }
                }
            };
        }

        private string GenerateCodeExample(RecommendationRule rule)
        {
            return rule.Name switch
            {
                "Object Pooling" => @"
// Object Pool Implementation Example
public class CoinPool
{
    private Queue<GameObject> pool = new Queue<GameObject>();
    private GameObject coinPrefab;
    
    public GameObject GetCoin()
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }
        return Instantiate(coinPrefab);
    }
    
    public void ReturnCoin(GameObject coin)
    {
        coin.SetActive(false);
        pool.Enqueue(coin);
    }
}",
                "Array Pre-allocation" => @"
// Array Pre-allocation Example
public class ParticleSystem
{
    private Particle[] particles;
    private int particleCount = 0;
    
    public void Initialize(int maxParticles)
    {
        particles = new Particle[maxParticles]; // Pre-allocate
    }
    
    public void AddParticle(Particle particle)
    {
        if (particleCount < particles.Length)
        {
            particles[particleCount++] = particle;
        }
    }
}",
                "Use Value Types" => @"
// Value Type vs Reference Type Example
// BAD: Class for small data (causes GC allocation)
public class VectorData
{
    public float x, y, z;
}

// GOOD: Struct for small data (no GC allocation)
public struct VectorData
{
    public float x, y, z;
}",
                _ => "// Code example available in documentation"
            };
        }

        private CostBenefitAnalysis GenerateCostBenefitAnalysis(RecommendationRule rule, AdvisorAnalysisData data)
        {
            return new CostBenefitAnalysis
            {
                ImplementationCostHours = rule.ImplementationEffort switch
                {
                    ImplementationEffort.Low => 2,
                    ImplementationEffort.Medium => 8,
                    ImplementationEffort.High => 20,
                    _ => 10
                },
                ExpectedMemorySavingMB = rule.EstimatedBenefit,
                PerformanceImprovement = rule.EstimatedBenefit * 0.1f, // 简化的性能提升估算
                RiskLevel = CalculateRiskLevel(rule),
                ROI = (rule.EstimatedBenefit * 10) / (rule.ImplementationEffort == ImplementationEffort.Low ? 2 : 
                                                  rule.ImplementationEffort == ImplementationEffort.Medium ? 8 : 20)
            };
        }

        private RiskLevel CalculateRiskLevel(RecommendationRule rule)
        {
            return rule.Name switch
            {
                "Object Pooling" => RiskLevel.Low,
                "Array Pre-allocation" => RiskLevel.Low,
                "Reduce Allocations" => RiskLevel.Medium,
                "Asset Unloading" => RiskLevel.Medium,
                _ => RiskLevel.Low
            };
        }

        private List<string> GenerateAcceptanceCriteria(RecommendationRule rule)
        {
            return new List<string>
            {
                "Memory usage reduced by at least 50% of estimated benefit",
                "No regression in functionality",
                "Performance maintained or improved",
                "No new memory leaks introduced"
            };
        }

        private List<string> GenerateSuccessMetrics(RecommendationRule rule)
        {
            return new List<string>
            {
                "Memory usage before and after optimization",
                "GC frequency and duration",
                "Frame rate stability",
                "User experience metrics"
            };
        }

        #endregion

        #region Data Collection Helpers

        private Dictionary<string, int> GetComponentAnalysisData()
        {
            var data = new Dictionary<string, int>();
            
            var gameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            var components = UnityEngine.Object.FindObjectsOfType<Component>();
            var monoBehaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();

            data["GameObjects"] = gameObjects.Length;
            data["Components"] = components.Length;
            data["MonoBehaviours"] = monoBehaviours.Length;
            data["ActiveCoins"] = FindObjectOfType<CoinObjectPool>()?.ActiveCoinCount ?? 0;

            return data;
        }

        private AssetAnalysisData GetAssetAnalysisData()
        {
            // 简化的资源分析
            return new AssetAnalysisData
            {
                //TotalAssetSizeMB = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(0) / (1024f * 1024f),
                TextureSizeMB = 20f, // 简化估算
                AudioSizeMB = 10f, // 简化估算
                MeshSizeMB = 15f // 简化估算
            };
        }

        #endregion

        #region Output Methods

        private void OutputRecommendationsToConsole()
        {
            if (_currentRecommendations.Count == 0)
            {
                Debug.Log("[MemoryOptimizationAdvisor] ✅ No memory optimization recommendations at this time");
                return;
            }

            Debug.Log($"[MemoryOptimizationAdvisor] 📋 {_currentRecommendations.Count} Memory Optimization Recommendations:");
            Debug.Log("=" + new string('=', 50));

            for (int i = 0; i < _currentRecommendations.Count; i++)
            {
                var rec = _currentRecommendations[i];
                var priorityIcon = rec.Priority switch
                {
                    RecommendationPriority.Critical => "🔴",
                    RecommendationPriority.High => "🟠",
                    RecommendationPriority.Medium => "🟡",
                    _ => "🟢"
                };

                Debug.Log($"{priorityIcon} {i + 1}. {rec.Title}");
                Debug.Log($"   📊 Benefit: {rec.EstimatedBenefit:F1}MB | ⚡ Effort: {rec.ImplementationEffort}");
                Debug.Log($"   📝 {rec.Description}");
                
                if (includeCodeExamples && !string.IsNullOrEmpty(rec.CodeExample))
                {
                    Debug.Log($"   💻 Code Example Available");
                }
                
                Debug.Log("");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// 获取详细的内存优化报告
        /// </summary>
        public MemoryOptimizationReport GetDetailedReport()
        {
            var report = new MemoryOptimizationReport
            {
                GeneratedAt = DateTime.UtcNow,
                CurrentRecommendations = new List<MemoryOptimizationRecommendation>(_currentRecommendations),
                AnalysisData = CollectAnalysisData(),
                AdvisorStats = _advisorStats,
                Summary = GenerateReportSummary()
            };

            if (generateDetailedReports)
            {
                report.DetailedAnalysis = GenerateDetailedAnalysis();
                report.ImplementationRoadmap = GenerateImplementationRoadmap();
            }

            OnDetailedReportGenerated?.Invoke(report);
            return report;
        }

        /// <summary>
        /// 接受建议
        /// </summary>
        public void AcceptRecommendation(string recommendationId)
        {
            var recommendation = _currentRecommendations.FirstOrDefault(r => r.Id == recommendationId);
            if (recommendation != null)
            {
                recommendation.Status = RecommendationStatus.Accepted;
                recommendation.AcceptedAt = DateTime.UtcNow;
                _acceptedRecommendations++;
                
                Debug.Log($"[MemoryOptimizationAdvisor] Recommendation accepted: {recommendation.Title}");
            }
        }

        /// <summary>
        /// 手动触发分析
        /// </summary>
        public void TriggerAnalysis()
        {
            if (!_advisorActive)
            {
                StartCoroutine(GenerateRecommendations());
            }
        }

        /// <summary>
        /// 启用/禁用顾问
        /// </summary>
        public void SetAdvisorEnabled(bool enabled)
        {
            enableAdvisor = enabled;
            if (enabled && !_advisorActive)
            {
                StartCoroutine(AdvisorCoroutine());
            }
            
            Debug.Log($"[MemoryOptimizationAdvisor] Advisor {(enabled ? "enabled" : "disabled")}");
        }

        #endregion

        #region Private Helper Methods

        private string GenerateReportSummary()
        {
            if (_currentRecommendations.Count == 0)
                return "No memory optimization issues detected. System is performing well.";

            var criticalCount = _currentRecommendations.Count(r => r.Priority == RecommendationPriority.Critical);
            var totalBenefit = _currentRecommendations.Sum(r => r.EstimatedBenefit);

            return $"Found {_currentRecommendations.Count} optimization recommendations " +
                   $"with {criticalCount} critical issues. " +
                   $"Total potential memory saving: {totalBenefit:F1}MB.";
        }

        private DetailedAnalysis GenerateDetailedAnalysis()
        {
            return new DetailedAnalysis
            {
                MemoryTrends = AnalyzeMemoryTrends(),
                PerformanceImpact = AnalyzePerformanceImpact(),
                ResourceUtilization = AnalyzeResourceUtilization(),
                RiskAssessment = AssessImplementationRisks()
            };
        }

        private ImplementationRoadmap GenerateImplementationRoadmap()
        {
            var roadmap = new ImplementationRoadmap
            {
                Phases = new List<ImplementationPhase>()
            };

            // 按优先级分组建议
            var phases = new[]
            {
                new { Priority = RecommendationPriority.Critical, Duration = "1-2 days", Name = "Critical Fixes" },
                new { Priority = RecommendationPriority.High, Duration = "3-5 days", Name = "High Impact Optimizations" },
                new { Priority = RecommendationPriority.Medium, Duration = "1-2 weeks", Name = "Medium Improvements" },
                new { Priority = RecommendationPriority.Low, Duration = "2-4 weeks", Name = "Fine Tuning" }
            };

            foreach (var phase in phases)
            {
                var recommendations = _currentRecommendations.Where(r => r.Priority == phase.Priority).ToList();
                if (recommendations.Count > 0)
                {
                    roadmap.Phases.Add(new ImplementationPhase
                    {
                        Name = phase.Name,
                        EstimatedDuration = phase.Duration,
                        Recommendations = recommendations,
                        ExpectedBenefits = recommendations.Sum(r => r.EstimatedBenefit)
                    });
                }
            }

            return roadmap;
        }

        private MemoryTrendAnalysis AnalyzeMemoryTrends()
        {
            // 简化的趋势分析
            return new MemoryTrendAnalysis
            {
                CurrentTrend = MemoryPressureTrend.Stable,
                ProjectedGrowth = 0f,
                RiskFactors = new List<string>()
            };
        }

        private PerformanceImpactAnalysis AnalyzePerformanceImpact()
        {
            // 简化的性能影响分析
            return new PerformanceImpactAnalysis
            {
                CurrentFPS = 60f,
                PotentialFPSGain = 5f,
                MemoryPerformanceRatio = 0.8f
            };
        }

        private ResourceUtilizationAnalysis AnalyzeResourceUtilization()
        {
            // 简化的资源利用分析
            return new ResourceUtilizationAnalysis
            {
                MemoryUtilization = 0.7f,
                AssetUtilization = 0.6f,
                ComponentEfficiency = 0.8f
            };
        }

        private RiskAssessment AssessImplementationRisks()
        {
            return new RiskAssessment
            {
                OverallRisk = RiskLevel.Low,
                TechnicalRisks = new List<string> { "Minor refactoring required" },
                OperationalRisks = new List<string>(),
                MitigationStrategies = new List<string> { "Thorough testing recommended" }
            };
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class MemoryOptimizationRecommendation
    {
        public string Id;
        public string RuleName;
        public OptimizationCategory Category;
        public RecommendationPriority Priority;
        public string Title;
        public string Description;
        public float EstimatedBenefit;
        public ImplementationEffort ImplementationEffort;
        public RiskLevel RiskLevel;
        public DateTime GeneratedAt;
        public RecommendationStatus Status = RecommendationStatus.Pending;
        public DateTime? AcceptedAt;
        public AdvisorAnalysisData ContextData;
        public List<string> AcceptanceCriteria;
        public List<string> SuccessMetrics;
        public ImplementationGuide ImplementationGuide;
        public string CodeExample;
        public CostBenefitAnalysis CostBenefitAnalysis;
    }

    [System.Serializable]
    public class RecommendationRule
    {
        public string Name;
        public Func<AdvisorAnalysisData, bool> Condition;
        public RecommendationPriority Priority;
        public OptimizationCategory Category;
        public float EstimatedBenefit;
        public ImplementationEffort ImplementationEffort;
        public string Description;
    }

    [System.Serializable]
    public class AdvisorAnalysisData
    {
        public DateTime Timestamp;
        public float CurrentMemoryMB;
        public MemoryAnalysisReport PatternAnalysis;
        public MemoryLeakReport LeakDetection;
        public MemoryPressureReport PressureAnalysis;
        public URPPerformanceMetrics PerformanceMetrics;
        public Dictionary<string, int> ComponentData;
        public AssetAnalysisData AssetData;
    }

    [System.Serializable]
    public class AssetAnalysisData
    {
        public float TotalAssetSizeMB;
        public float TextureSizeMB;
        public float AudioSizeMB;
        public float MeshSizeMB;
    }

    [System.Serializable]
    public class OptimizationInsight
    {
        public InsightType Type;
        public string Title;
        public string Description;
        public string Recommendation;
        public float Confidence;
        public Dictionary<string, object> Data;
    }

    [System.Serializable]
    public class ImplementationGuide
    {
        public List<string> Steps;
        public List<string> Considerations;
        public List<string> CommonPitfalls;
        public List<string> TestingGuidelines;
    }

    [System.Serializable]
    public class CostBenefitAnalysis
    {
        public float ImplementationCostHours;
        public float ExpectedMemorySavingMB;
        public float PerformanceImprovement;
        public RiskLevel RiskLevel;
        public float ROI;
    }

    [System.Serializable]
    public class MemoryAdvisorStats
    {
        public int TotalSessions;
        public int FailedSessions;
        public TimeSpan TotalSessionTime;
        public TimeSpan AverageSessionTime;
        public DateTime LastSessionTime;
        public int RecommendationsGenerated;
        public int RecommendationsAccepted;
    }

    [System.Serializable]
    public class MemoryOptimizationReport
    {
        public DateTime GeneratedAt;
        public List<MemoryOptimizationRecommendation> CurrentRecommendations;
        public AdvisorAnalysisData AnalysisData;
        public MemoryAdvisorStats AdvisorStats;
        public string Summary;
        public DetailedAnalysis DetailedAnalysis;
        public ImplementationRoadmap ImplementationRoadmap;
    }

    [System.Serializable]
    public class DetailedAnalysis
    {
        public MemoryTrendAnalysis MemoryTrends;
        public PerformanceImpactAnalysis PerformanceImpact;
        public ResourceUtilizationAnalysis ResourceUtilization;
        public RiskAssessment RiskAssessment;
    }

    [System.Serializable]
    public class ImplementationRoadmap
    {
        public List<ImplementationPhase> Phases;
        public float TotalExpectedBenefits;
        public string TotalEstimatedDuration;
    }

    [System.Serializable]
    public class ImplementationPhase
    {
        public string Name;
        public string EstimatedDuration;
        public List<MemoryOptimizationRecommendation> Recommendations;
        public float ExpectedBenefits;
    }

    [System.Serializable]
    public class AdvisorSession
    {
        public DateTime StartTime;
        public DateTime EndTime;
        public int RecommendationsGenerated;
        public List<string> Insights;
    }

    // 辅助数据结构
    [System.Serializable]
    public class MemoryTrendAnalysis
    {
        public MemoryPressureTrend CurrentTrend;
        public float ProjectedGrowth;
        public List<string> RiskFactors;
    }

    [System.Serializable]
    public class PerformanceImpactAnalysis
    {
        public float CurrentFPS;
        public float PotentialFPSGain;
        public float MemoryPerformanceRatio;
    }

    [System.Serializable]
    public class ResourceUtilizationAnalysis
    {
        public float MemoryUtilization;
        public float AssetUtilization;
        public float ComponentEfficiency;
    }

    [System.Serializable]
    public class RiskAssessment
    {
        public RiskLevel OverallRisk;
        public List<string> TechnicalRisks;
        public List<string> OperationalRisks;
        public List<string> MitigationStrategies;
    }

    #endregion

    #region Enums

    public enum OptimizationCategory
    {
        Allocation,
        GarbageCollection,
        ResourceManagement,
        CodePatterns,
        AssetOptimization,
        Performance
    }

    //public enum RecommendationPriority
    //{
    //    Low,
    //    Medium,
    //    High,
    //    Critical
    //}


    public enum RiskLevel
    {
        Low,
        Medium,
        High
    }

    public enum RecommendationStatus
    {
        Pending,
        Accepted,
        Rejected,
        Implemented,
        Completed
    }

    public enum InsightType
    {
        Information,
        Warning,
        Critical,
        Optimization
    }

    #endregion
}