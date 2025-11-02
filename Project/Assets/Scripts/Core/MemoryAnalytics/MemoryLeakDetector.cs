using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoinAnimation.Core
{
    /// <summary>
    /// 内存泄漏检测和预防警报系统
    /// Story 2.1 Task 3.2 - 实现智能内存泄漏检测和预防性警报
    /// </summary>
    public class MemoryLeakDetector : MonoBehaviour
    {
        #region Configuration

        [Header("Leak Detection Settings")]
        [SerializeField] private bool enableLeakDetection = true;
        [SerializeField] private float leakDetectionInterval = 30f; // 30秒检查间隔
        [SerializeField] private int leakHistorySize = 100; // 保存100个历史数据点
        [SerializeField] private float memoryGrowthThreshold = 10f; // MB per minute
        [SerializeField] private float leakConfirmationThreshold = 3f; // 连续3次检测确认泄漏

        [Header("Prevention Settings")]
        [SerializeField] private bool enablePrevention = true;
        [SerializeField] private bool enableAutomaticCleanup = true;
        [SerializeField] private float preventionCheckInterval = 10f;
        [SerializeField] private int maxConsecutiveLeaks = 5;

        [Header("Alert Settings")]
        [SerializeField] private bool enableAlerts = true;
        [SerializeField] private bool enableCriticalAlerts = true;
        [SerializeField] private float alertCooldown = 60f; // 警报冷却时间
        [SerializeField] private int maxAlertsPerMinute = 3;

        [Header("Component Tracking")]
        [SerializeField] private bool trackUnityObjects = true;
        [SerializeField] private bool trackMonoBehaviours = true;
        [SerializeField] private bool trackGameObjects = true;
        [SerializeField] private bool trackAssets = false; // 资源跟踪较消耗性能

        #endregion

        #region Private Fields

        // 泄漏检测数据
        private readonly Queue<MemoryLeakSnapshot> _leakHistory = new Queue<MemoryLeakSnapshot>();
        private readonly List<MemoryLeak> _detectedLeaks = new List<MemoryLeak>();
        private readonly Dictionary<string, ComponentTracker> _componentTrackers = new Dictionary<string, ComponentTracker>();
        
        // 检测状态
        private bool _leakDetectionInProgress = false;
        private float _lastLeakCheckTime = 0f;
        private int _consecutiveLeakDetections = 0;
        private bool _leakConfirmed = false;

        // 预防系统
        private readonly List<LeakPreventionRule> _preventionRules = new List<LeakPreventionRule>();
        private float _lastPreventionCheck = 0f;
        private int _preventionActionsTaken = 0;

        // 警报系统
        private float _lastAlertTime = 0f;
        private int _alertsThisMinute = 0;
        private readonly Queue<DateTime> _alertHistory = new Queue<DateTime>();

        // 组件计数器
        private int _lastGameObjectCount = 0;
        private int _lastComponentCount = 0;
        private int _lastMonoBehaviourCount = 0;

        // 统计数据
        private MemoryLeakDetectionStats _detectionStats = new MemoryLeakDetectionStats();

        #endregion

        #region Properties

        public bool LeakDetectionEnabled => enableLeakDetection;
        public bool LeakConfirmed => _leakConfirmed;
        public IReadOnlyList<MemoryLeak> DetectedLeaks => _detectedLeaks.AsReadOnly();
        public MemoryLeakDetectionStats DetectionStats => _detectionStats;
        public bool IsDetectionInProgress => _leakDetectionInProgress;

        #endregion

        #region Events

        public event Action<MemoryLeak> OnMemoryLeakDetected;
        public event Action<MemoryLeakAlert> OnLeakAlertIssued;
        public event Action<LeakPreventionAction> OnPreventionActionTaken;
        public event Action<MemoryLeakReport> OnLeakReportGenerated;
        public event Action<bool> OnLeakConfirmationChanged;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializeComponentTracking();
            InitializePreventionRules();
            StartCoroutine(LeakDetectionCoroutine());
            StartCoroutine(PreventionCoroutine());
        }

        private void Update()
        {
            if (!enableLeakDetection) return;

            TrackUnityComponents();
            UpdateAlertCooldown();
        }

        private void OnDestroy()
        {
            CleanupTracking();
        }

        #endregion

        #region Initialization

        private void InitializeComponentTracking()
        {
            _lastGameObjectCount = UnityEngine.Object.FindObjectsOfType<GameObject>().Length;
            _lastComponentCount = UnityEngine.Object.FindObjectsOfType<Component>().Length;
            _lastMonoBehaviourCount = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().Length;

            Debug.Log($"[MemoryLeakDetector] Initial component counts - GameObjects: {_lastGameObjectCount}, " +
                     $"Components: {_lastComponentCount}, MonoBehaviours: {_lastMonoBehaviourCount}");
        }

        private void InitializePreventionRules()
        {
            // 添加默认预防规则
            _preventionRules.Add(new LeakPreventionRule
            {
                Name = "GameObject Growth Limit",
                Type = PreventionType.GameObjectLimit,
                Threshold = 1000, // 最大1000个GameObject
                Action = PreventionAction.ForceCleanup,
                Priority = PreventionPriority.High
            });

            _preventionRules.Add(new LeakPreventionRule
            {
                Name = "Memory Growth Limit",
                Type = PreventionType.MemoryGrowth,
                Threshold = memoryGrowthThreshold * 2, // 2倍正常阈值
                Action = PreventionAction.TriggerGC,
                Priority = PreventionPriority.Critical
            });

            _preventionRules.Add(new LeakPreventionRule
            {
                Name = "Component Accumulation",
                Type = PreventionType.ComponentAccumulation,
                Threshold = 500, // 最大500个新增组件
                Action = PreventionAction.LogWarning,
                Priority = PreventionPriority.Medium
            });

            Debug.Log($"[MemoryLeakDetector] Initialized {_preventionRules.Count} prevention rules");
        }

        #endregion

        #region Leak Detection

        private IEnumerator LeakDetectionCoroutine()
        {
            while (enableLeakDetection)
            {
                yield return new WaitForSeconds(leakDetectionInterval);

                if (!_leakDetectionInProgress)
                {
                    yield return StartCoroutine(PerformLeakDetection());
                }
            }
        }

        private IEnumerator PerformLeakDetection()
        {
            _leakDetectionInProgress = true;
            var detectionStartTime = Time.realtimeSinceStartup;
            Exception caughtException = null;

            try
            {
                Debug.Log("[MemoryLeakDetector] Starting leak detection...");
            }
            catch (Exception e)
            {
                caughtException = e;
                Debug.LogError($"[MemoryLeakDetector] Leak detection initialization failed: {e.Message}");
                _detectionStats.FailedDetections++;
            }

            if (caughtException == null)
            {
                // 1. 创建内存快照
                var snapshot = CreateLeakSnapshot();
                _leakHistory.Enqueue(snapshot);

                // 保持历史记录大小
                while (_leakHistory.Count > leakHistorySize)
                    _leakHistory.Dequeue();

                // 2. 分析内存增长模式
                yield return StartCoroutine(AnalyzeMemoryGrowth(snapshot));

                // 3. 检测组件泄漏
                yield return StartCoroutine(DetectComponentLeaks(snapshot));

                // 4. 分析特定组件类型
                yield return StartCoroutine(AnalyzeSpecificLeaks(snapshot));

                // 5. 确认泄漏
                ConfirmMemoryLeak();

                // 6. 更新统计
                var detectionDuration = Time.realtimeSinceStartup - detectionStartTime;
                _detectionStats.TotalDetections++;
                _detectionStats.AverageDetectionTime =
                    (_detectionStats.AverageDetectionTime * (_detectionStats.TotalDetections - 1) + detectionDuration)
                    / _detectionStats.TotalDetections;

                Debug.Log($"[MemoryLeakDetector] Leak detection completed in {detectionDuration:F3}s");
            }

            _leakDetectionInProgress = false;
        }

        private MemoryLeakSnapshot CreateLeakSnapshot()
        {
            var currentGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            var currentComponents = UnityEngine.Object.FindObjectsOfType<Component>();
            var currentMonoBehaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();

            return new MemoryLeakSnapshot
            {
                Timestamp = DateTime.UtcNow,
                TotalMemoryMB = GC.GetTotalMemory(false) / (1024f * 1024f),
                ManagedMemoryMB = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory() / (1024f * 1024f),
                GameObjectCount = currentGameObjects.Length,
                ComponentCount = currentComponents.Length,
                MonoBehaviourCount = currentMonoBehaviours.Length,
                ActiveCoinsCount = FindObjectOfType<CoinObjectPool>()?.ActiveCoinCount ?? 0,
                SceneName = SceneManager.GetActiveScene().name,
                GCGeneration = GC.MaxGeneration,
                ComponentTypes = GetComponentTypeDistribution(currentComponents),
                suspiciousObjects = IdentifySuspiciousObjects(currentGameObjects)
            };
        }

        private IEnumerator AnalyzeMemoryGrowth(MemoryLeakSnapshot currentSnapshot)
        {
            if (_leakHistory.Count < 2) yield break;

            var previousSnapshot = _leakHistory.ToArray()[_leakHistory.Count - 2];
            var timeDiff = (currentSnapshot.Timestamp - previousSnapshot.Timestamp).TotalMinutes;
            
            if (timeDiff <= 0) yield break;

            // 计算内存增长率
            var memoryGrowth = currentSnapshot.TotalMemoryMB - previousSnapshot.TotalMemoryMB;
            var memoryGrowthRate = memoryGrowth / (float)timeDiff; // MB per minute

            // 检测是否超过阈值
            if (memoryGrowthRate > memoryGrowthThreshold)
            {
                var leak = new MemoryLeak
                {
                    Type = MemoryLeakType.MemoryGrowth,
                    Severity = CalculateLeakSeverity(memoryGrowthRate, memoryGrowthThreshold),
                    DetectedTime = DateTime.UtcNow,
                    GrowthRate = memoryGrowthRate,
                    Description = $"Memory growing at {memoryGrowthRate:F2} MB/min (threshold: {memoryGrowthThreshold} MB/min)",
                    AffectedComponents = new List<string> { "Managed Heap" },
                    EstimatedLeakSize = EstimateLeakSize(memoryGrowthRate),
                    Confidence = CalculateConfidence(_leakHistory.Count)
                };

                _detectedLeaks.Add(leak);
                OnMemoryLeakDetected?.Invoke(leak);

                Debug.LogWarning($"[MemoryLeakDetector] Memory leak detected: {leak.Description}");
            }

            yield return null;
        }

        private IEnumerator DetectComponentLeaks(MemoryLeakSnapshot snapshot)
        {
            // 检测GameObject泄漏
            var gameObjectGrowth = snapshot.GameObjectCount - _lastGameObjectCount;
            if (gameObjectGrowth > 50) // 新增超过50个GameObject
            {
                var leak = new MemoryLeak
                {
                    Type = MemoryLeakType.GameObjectAccumulation,
                    Severity = CalculateLeakSeverity(gameObjectGrowth, 100),
                    DetectedTime = DateTime.UtcNow,
                    Description = $"GameObject accumulation: +{gameObjectGrowth} objects since last check",
                    AffectedComponents = new List<string> { "GameObjects" },
                    EstimatedLeakSize = gameObjectGrowth * 0.1f, // 每个GameObject约0.1MB
                    Confidence = 0.8f
                };

                _detectedLeaks.Add(leak);
                OnMemoryLeakDetected?.Invoke(leak);
            }

            // 检测Component泄漏
            var componentGrowth = snapshot.ComponentCount - _lastComponentCount;
            if (componentGrowth > 100) // 新增超过100个Component
            {
                var leak = new MemoryLeak
                {
                    Type = MemoryLeakType.ComponentAccumulation,
                    Severity = CalculateLeakSeverity(componentGrowth, 200),
                    DetectedTime = DateTime.UtcNow,
                    Description = $"Component accumulation: +{componentGrowth} components since last check",
                    AffectedComponents = new List<string> { "Components" },
                    EstimatedLeakSize = componentGrowth * 0.05f, // 每个Component约0.05MB
                    Confidence = 0.7f
                };

                _detectedLeaks.Add(leak);
                OnMemoryLeakDetected?.Invoke(leak);
            }

            // 更新计数器
            _lastGameObjectCount = snapshot.GameObjectCount;
            _lastComponentCount = snapshot.ComponentCount;
            _lastMonoBehaviourCount = snapshot.MonoBehaviourCount;

            yield return null;
        }

        private IEnumerator AnalyzeSpecificLeaks(MemoryLeakSnapshot snapshot)
        {
            // 分析特定类型的组件泄漏
            if (snapshot.ComponentTypes != null)
            {
                foreach (var componentType in snapshot.ComponentTypes)
                {
                    if (componentType.Value > 100 && ShouldTrackComponentType(componentType.Key))
                    {
                        var leak = new MemoryLeak
                        {
                            Type = MemoryLeakType.SpecificComponentLeak,
                            Severity = PatternSeverity.Medium,
                            DetectedTime = DateTime.UtcNow,
                            Description = $"High count of {componentType.Key}: {componentType.Value} instances",
                            AffectedComponents = new List<string> { componentType.Key },
                            EstimatedLeakSize = componentType.Value * GetComponentSize(componentType.Key),
                            Confidence = 0.6f
                        };

                        _detectedLeaks.Add(leak);
                        OnMemoryLeakDetected?.Invoke(leak);
                    }
                }
            }

            // 分析可疑对象
            if (snapshot.suspiciousObjects != null && snapshot.suspiciousObjects.Count > 0)
            {
                var leak = new MemoryLeak
                {
                    Type = MemoryLeakType.SuspiciousObject,
                    Severity = PatternSeverity.High,
                    DetectedTime = DateTime.UtcNow,
                    Description = $"Found {snapshot.suspiciousObjects.Count} suspicious objects",
                    AffectedComponents = snapshot.suspiciousObjects.Select(obj => obj.name).ToList(),
                    EstimatedLeakSize = snapshot.suspiciousObjects.Count * 0.2f,
                    Confidence = 0.9f
                };

                _detectedLeaks.Add(leak);
                OnMemoryLeakDetected?.Invoke(leak);
            }

            yield return null;
        }

        #endregion

        #region Leak Confirmation

        private void ConfirmMemoryLeak()
        {
            var recentLeaks = _detectedLeaks.Where(l => 
                (DateTime.UtcNow - l.DetectedTime).TotalMinutes < leakDetectionInterval).ToList();

            bool wasConfirmed = _leakConfirmed;
            _leakConfirmed = recentLeaks.Count >= leakConfirmationThreshold;

            if (_leakConfirmed && !wasConfirmed)
            {
                _consecutiveLeakDetections++;
                Debug.LogWarning($"[MemoryLeakDetector] Memory leak CONFIRMED! {_consecutiveLeakDetections} consecutive detections");
                
                if (enableAlerts)
                {
                    IssueLeakAlert(recentLeaks);
                }

                OnLeakConfirmationChanged?.Invoke(true);
            }
            else if (!_leakConfirmed && wasConfirmed)
            {
                Debug.Log("[MemoryLeakDetector] Memory leak confirmation cleared");
                OnLeakConfirmationChanged?.Invoke(false);
            }
        }

        private void IssueLeakAlert(List<MemoryLeak> leaks)
        {
            if (!enableAlerts || !CanIssueAlert()) return;

            var alert = new MemoryLeakAlert
            {
                AlertType = _consecutiveLeakDetections >= maxConsecutiveLeaks ? 
                    LeakAlertType.Critical : LeakAlertType.Warning,
                Message = $"Memory leak confirmed! {_consecutiveLeakDetections} consecutive detections",
                DetectedLeaks = leaks,
                IssuedAt = DateTime.UtcNow,
                RecommendedActions = GenerateRecommendedActions(leaks),
                EstimatedImpact = CalculateEstimatedImpact(leaks)
            };

            OnLeakAlertIssued?.Invoke(alert);
            _lastAlertTime = Time.time;
            _alertsThisMinute++;
            _alertHistory.Enqueue(DateTime.UtcNow);

            // 保持警报历史
            while (_alertHistory.Count > 10)
                _alertHistory.Dequeue();

            Debug.LogError($"[MemoryLeakDetector] LEAK ALERT: {alert.Message}");
        }

        #endregion

        #region Prevention System

        private IEnumerator PreventionCoroutine()
        {
            while (enablePrevention)
            {
                yield return new WaitForSeconds(preventionCheckInterval);

                if (Time.time > _lastPreventionCheck + preventionCheckInterval)
                {
                    yield return StartCoroutine(PerformPreventionCheck());
                }
            }
        }

        private IEnumerator PerformPreventionCheck()
        {
            _lastPreventionCheck = Time.time;

            foreach (var rule in _preventionRules)
            {
                if (ShouldApplyRule(rule))
                {
                    yield return StartCoroutine(ApplyPreventionRule(rule));
                }
            }

            yield return null;
        }

        private bool ShouldApplyRule(LeakPreventionRule rule)
        {
            var currentSnapshot = _leakHistory.Count > 0 ? _leakHistory.Last() : null;
            if (currentSnapshot == null) return false;

            return rule.Type switch
            {
                PreventionType.MemoryGrowth => currentSnapshot.TotalMemoryMB > rule.Threshold,
                PreventionType.GameObjectLimit => currentSnapshot.GameObjectCount > rule.Threshold,
                PreventionType.ComponentAccumulation => currentSnapshot.ComponentCount > rule.Threshold,
                _ => false
            };
        }

        private IEnumerator ApplyPreventionRule(LeakPreventionRule rule)
        {
            var action = new LeakPreventionAction
            {
                Rule = rule,
                AppliedAt = DateTime.UtcNow,
                Success = false
            };

            try
            {
                switch (rule.Action)
                {
                    //case PreventionAction.TriggerGC:
                    //    yield return StartCoroutine(TriggerGarbageCollection(action));
                    //    break;
                    //case PreventionAction.ForceCleanup:
                    //    yield return StartCoroutine(ForceCleanup(action));
                    //    break;
                    case PreventionAction.LogWarning:
                        LogPreventionWarning(rule);
                        action.Success = true;
                        break;
                    //case PreventionAction.DisableFeatures:
                    //    yield return StartCoroutine(DisableMemoryIntensiveFeatures(action));
                    //    break;
                }

                _preventionActionsTaken++;
                OnPreventionActionTaken?.Invoke(action);

                Debug.Log($"[MemoryLeakDetector] Prevention action applied: {rule.Name} -> {rule.Action}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MemoryLeakDetector] Prevention action failed: {e.Message}");
            }

            yield return null;
        }

        private IEnumerator TriggerGarbageCollection(LeakPreventionAction action)
        {
            var beforeMemory = GC.GetTotalMemory(false) / (1024f * 1024f);
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            yield return new WaitForSeconds(0.5f); // 等待GC完成

            var afterMemory = GC.GetTotalMemory(false) / (1024f * 1024f);
            var memoryFreed = beforeMemory - afterMemory;

            action.ResultData = new Dictionary<string, object>
            {
                ["MemoryFreedMB"] = memoryFreed,
                ["BeforeMemoryMB"] = beforeMemory,
                ["AfterMemoryMB"] = afterMemory
            };

            action.Success = memoryFreed > 1f; // 至少释放1MB

            Debug.Log($"[MemoryLeakDetector] GC prevention freed {memoryFreed:F2} MB");
        }

        private IEnumerator ForceCleanup(LeakPreventionAction action)
        {
            // 强制清理未使用的对象
            var coinPool = FindObjectOfType<CoinObjectPool>();
            if (coinPool != null)
            {
                var beforePoolSize = coinPool.CurrentPoolSize;
                //coinPool.Cleanup(); // 假设有清理方法
                yield return null;
                
                var afterPoolSize = coinPool.CurrentPoolSize;
                var objectsCleaned = beforePoolSize - afterPoolSize;

                action.ResultData = new Dictionary<string, object>
                {
                    ["ObjectsCleaned"] = objectsCleaned,
                    ["BeforePoolSize"] = beforePoolSize,
                    ["AfterPoolSize"] = afterPoolSize
                };

                action.Success = objectsCleaned > 0;

                Debug.Log($"[MemoryLeakDetector] Force cleanup removed {objectsCleaned} objects");
            }
        }

        private void LogPreventionWarning(LeakPreventionRule rule)
        {
            Debug.LogWarning($"[MemoryLeakDetector] Prevention warning: {rule.Name} threshold exceeded");
        }

        private IEnumerator DisableMemoryIntensiveFeatures(LeakPreventionAction action)
        {
            //// 禁用内存密集型功能
            //var qualityManager = FindObjectOfType<AdaptiveQualityManager>();
            //if (qualityManager != null)
            //{
            //    var previousQuality = qualityManager.CurrentQualityLevel;
            //    qualityManager.SetQualityLevel(QualityLevel.Minimum);
                
            //    action.ResultData = new Dictionary<string, object>
            //    {
            //        ["PreviousQuality"] = previousQuality,
            //        ["NewQuality"] = QualityLevel.Minimum
            //    };

            //    action.Success = true;

            //    Debug.Log($"[MemoryLeakDetector] Disabled memory-intensive features, quality: {previousQuality} -> Minimum");
            //}

            yield return null;
        }

        #endregion

        #region Component Tracking

        private void TrackUnityComponents()
        {
            if (!trackUnityObjects) return;

            // 更新组件跟踪器
            var currentComponents = UnityEngine.Object.FindObjectsOfType<Component>();
            
            foreach (var component in currentComponents)
            {
                var typeName = component.GetType().Name;
                
                if (!_componentTrackers.ContainsKey(typeName))
                {
                    _componentTrackers[typeName] = new ComponentTracker
                    {
                        TypeName = typeName,
                        FirstSeen = DateTime.UtcNow,
                        PeakCount = 1,
                        CurrentCount = 1
                    };
                }
                else
                {
                    var tracker = _componentTrackers[typeName];
                    tracker.CurrentCount++;
                    tracker.PeakCount = Mathf.Max(tracker.PeakCount, tracker.CurrentCount);
                    tracker.LastSeen = DateTime.UtcNow;
                }
            }
        }

        private Dictionary<string, int> GetComponentTypeDistribution(Component[] components)
        {
            var distribution = new Dictionary<string, int>();
            
            foreach (var component in components)
            {
                var typeName = component.GetType().Name;
                if (distribution.ContainsKey(typeName))
                    distribution[typeName]++;
                else
                    distribution[typeName] = 1;
            }

            return distribution;
        }

        private List<GameObject> IdentifySuspiciousObjects(GameObject[] gameObjects)
        {
            var suspicious = new List<GameObject>();

            foreach (var obj in gameObjects)
            {
                if (IsSuspiciousObject(obj))
                {
                    suspicious.Add(obj);
                }
            }

            return suspicious;
        }

        private bool IsSuspiciousObject(GameObject obj)
        {
            // 检测可疑对象的规则
            if (obj.name.Contains("Clone") && obj.name.Length > 20) // 长的克隆名称
                return true;

            if (obj.transform.childCount > 50) // 过多的子对象
                return true;

            if (obj.GetComponents<Component>().Length > 20) // 过多的组件
                return true;

            return false;
        }

        private bool ShouldTrackComponentType(string typeName)
        {
            // 跟踪可能导致泄漏的组件类型
            var trackedTypes = new[]
            {
                "CoinAnimationController",
                "ParticleSystem",
                "AudioSource",
                "MeshRenderer",
                "SkinnedMeshRenderer"
            };

            return trackedTypes.Contains(typeName) || typeName.Contains("Coin");
        }

        private float GetComponentSize(string typeName)
        {
            // 估算组件大小（MB）
            return typeName switch
            {
                "CoinAnimationController" => 0.1f,
                "ParticleSystem" => 0.5f,
                "AudioSource" => 0.2f,
                "MeshRenderer" => 0.3f,
                "SkinnedMeshRenderer" => 1.0f,
                _ => 0.05f
            };
        }

        #endregion

        #region Utility Methods

        private void UpdateAlertCooldown()
        {
            // 更新警报冷却
            var now = DateTime.UtcNow;
            while (_alertHistory.Count > 0 && (now - _alertHistory.Peek()).TotalSeconds > 60)
            {
                _alertHistory.Dequeue();
            }
            _alertsThisMinute = _alertHistory.Count;
        }

        private bool CanIssueAlert()
        {
            return Time.time > _lastAlertTime + alertCooldown && 
                   _alertsThisMinute < maxAlertsPerMinute;
        }

        private PatternSeverity CalculateLeakSeverity(float value, float threshold)
        {
            if (value >= threshold * 3) return PatternSeverity.Critical;
            if (value >= threshold * 2) return PatternSeverity.High;
            if (value >= threshold) return PatternSeverity.Medium;
            return PatternSeverity.Low;
        }

        private float EstimateLeakSize(float growthRate)
        {
            // 估算泄漏大小（MB）
            return growthRate * 10f; // 假设10分钟内的泄漏量
        }

        private float CalculateConfidence(int dataPoints)
        {
            return Mathf.Min(1f, dataPoints / 10f);
        }

        private List<string> GenerateRecommendedActions(List<MemoryLeak> leaks)
        {
            var actions = new List<string>();

            if (leaks.Any(l => l.Type == MemoryLeakType.MemoryGrowth))
                actions.Add("Investigate memory allocation patterns");

            if (leaks.Any(l => l.Type == MemoryLeakType.GameObjectAccumulation))
                actions.Add("Check for un-destroyed GameObjects");

            if (leaks.Any(l => l.Type == MemoryLeakType.ComponentAccumulation))
                actions.Add("Review component addition/removal logic");

            actions.Add("Consider implementing object pooling");

            return actions;
        }

        private float CalculateEstimatedImpact(List<MemoryLeak> leaks)
        {
            return leaks.Sum(l => l.EstimatedLeakSize);
        }

        private void CleanupTracking()
        {
            _componentTrackers.Clear();
            _leakHistory.Clear();
            _detectedLeaks.Clear();
        }

        #endregion

        #region Public API

        /// <summary>
        /// 获取内存泄漏检测报告
        /// </summary>
        public MemoryLeakReport GetLeakReport()
        {
            return new MemoryLeakReport
            {
                GeneratedAt = DateTime.UtcNow,
                IsLeakConfirmed = _leakConfirmed,
                ConsecutiveDetections = _consecutiveLeakDetections,
                DetectedLeaks = new List<MemoryLeak>(_detectedLeaks),
                CurrentMemoryMB = _leakHistory.Count > 0 ? _leakHistory.Last().TotalMemoryMB : 0f,
                ComponentTrackers = new Dictionary<string, ComponentTracker>(_componentTrackers),
                PreventionActionsTaken = _preventionActionsTaken,
                DetectionStats = _detectionStats
            };
        }

        /// <summary>
        /// 手动触发泄漏检测
        /// </summary>
        public void TriggerLeakDetection()
        {
            if (!_leakDetectionInProgress)
            {
                StartCoroutine(PerformLeakDetection());
            }
        }

        /// <summary>
        /// 清除检测历史
        /// </summary>
        public void ClearDetectionHistory()
        {
            _leakHistory.Clear();
            _detectedLeaks.Clear();
            _componentTrackers.Clear();
            _consecutiveLeakDetections = 0;
            _leakConfirmed = false;
            Debug.Log("[MemoryLeakDetector] Detection history cleared");
        }

        /// <summary>
        /// 启用/禁用泄漏检测
        /// </summary>
        public void SetLeakDetectionEnabled(bool enabled)
        {
            enableLeakDetection = enabled;
            Debug.Log($"[MemoryLeakDetector] Leak detection {(enabled ? "enabled" : "disabled")}");
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class MemoryLeakSnapshot
    {
        public DateTime Timestamp;
        public float TotalMemoryMB;
        public float ManagedMemoryMB;
        public int GameObjectCount;
        public int ComponentCount;
        public int MonoBehaviourCount;
        public int ActiveCoinsCount;
        public string SceneName;
        public int GCGeneration;
        public Dictionary<string, int> ComponentTypes;
        public List<GameObject> suspiciousObjects;
    }

    [System.Serializable]
    public class MemoryLeak
    {
        public MemoryLeakType Type;
        public PatternSeverity Severity;
        public DateTime DetectedTime;
        public string Description;
        public List<string> AffectedComponents;
        public float EstimatedLeakSize;
        public float GrowthRate;
        public float Confidence;
        public Dictionary<string, object> LeakData;
    }

    [System.Serializable]
    public class MemoryLeakAlert
    {
        public LeakAlertType AlertType;
        public string Message;
        public List<MemoryLeak> DetectedLeaks;
        public DateTime IssuedAt;
        public List<string> RecommendedActions;
        public float EstimatedImpact;
    }

    [System.Serializable]
    public class LeakPreventionRule
    {
        public string Name;
        public PreventionType Type;
        public float Threshold;
        public PreventionAction Action;
        public PreventionPriority Priority;
        public bool Enabled = true;
    }

    [System.Serializable]
    public class LeakPreventionAction
    {
        public LeakPreventionRule Rule;
        public DateTime AppliedAt;
        public bool Success;
        public Dictionary<string, object> ResultData;
    }

    [System.Serializable]
    public class ComponentTracker
    {
        public string TypeName;
        public DateTime FirstSeen;
        public DateTime LastSeen;
        public int CurrentCount;
        public int PeakCount;
        public int TotalCreated;
        public int TotalDestroyed;
    }

    [System.Serializable]
    public class MemoryLeakDetectionStats
    {
        public int TotalDetections;
        public int FailedDetections;
        public float AverageDetectionTime;
        public DateTime LastDetectionTime;
        public TimeSpan TotalDetectionTime;
        public int LeaksDetected;
        public int FalsePositives;
    }

    [System.Serializable]
    public class MemoryLeakReport
    {
        public DateTime GeneratedAt;
        public bool IsLeakConfirmed;
        public int ConsecutiveDetections;
        public List<MemoryLeak> DetectedLeaks;
        public float CurrentMemoryMB;
        public Dictionary<string, ComponentTracker> ComponentTrackers;
        public int PreventionActionsTaken;
        public MemoryLeakDetectionStats DetectionStats;

        public string Category { get; internal set; }
        public string ObjectKey { get; internal set; }
        public TimeSpan Age { get; internal set; }
        public int AccessCount { get; internal set; }
        public DateTime LastAccessed { get; internal set; }
        public bool IsPotentialLeak { get; internal set; }
        public DateTime DetectionTime { get; internal set; }
    }

    #endregion

    #region Enums

    public enum MemoryLeakType
    {
        MemoryGrowth,
        GameObjectAccumulation,
        ComponentAccumulation,
        SpecificComponentLeak,
        SuspiciousObject,
        EventSubscriptionLeak,
        CoroutineLeak
    }

    public enum LeakAlertType
    {
        Info,
        Warning,
        Critical
    }

    public enum PreventionType
    {
        MemoryGrowth,
        GameObjectLimit,
        ComponentAccumulation,
        SceneLoadLimit
    }

    public enum PreventionAction
    {
        LogWarning,
        TriggerGC,
        ForceCleanup,
        DisableFeatures,
        UnloadAssets
    }

    public enum PreventionPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    #endregion
}