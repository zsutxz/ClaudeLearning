using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoinAnimation.Core
{
    /// <summary>
    /// 性能警报和警告系统
    /// Story 2.1 Task 4.2 - 实现智能性能警报和多层次警告系统
    /// </summary>
    public class PerformanceAlertSystem : MonoBehaviour
    {
        #region Configuration

        [Header("Alert System Settings")]
        [SerializeField] private bool enableAlerts = true;
        [SerializeField] private bool enableSoundAlerts = true;
        [SerializeField] private bool enableVisualAlerts = true;
        [SerializeField] private bool enableLogAlerts = true;

        [Header("Alert Thresholds")]
        [Header("FPS Thresholds")]
        [SerializeField] private float fpsNormalThreshold = 55f;
        [SerializeField] private float fpsWarningThreshold = 45f;
        [SerializeField] private float fpsCriticalThreshold = 35f;

        [Header("Memory Thresholds")]
        [SerializeField] private float memoryNormalThreshold = 80f;
        [SerializeField] private float memoryWarningThreshold = 120f;
        [SerializeField] private float memoryCriticalThreshold = 180f;

        [Header("Performance Thresholds")]
        [SerializeField] private float frameTimeWarningThreshold = 25f; // ms
        [SerializeField] private float frameTimeCriticalThreshold = 40f; // ms
        [SerializeField] private int gcFrequencyWarningThreshold = 5; // per minute

        [Header("Alert Management")]
        [SerializeField] private float alertCooldown = 5f;
        [SerializeField] private int maxAlertsPerMinute = 10;
        [SerializeField] private bool enableEscalation = true;
        [SerializeField] private int consecutiveAlertsToEscalate = 3;

        [Header("Notification Settings")]
        [SerializeField] private bool enableDesktopNotifications = false;
        [SerializeField] private bool enableEmailAlerts = false;
        [SerializeField] private string alertEmailAddress = "";

        #endregion

        #region Private Fields

        // 警报状态
        private bool _alertSystemActive = false;
        private DateTime _lastAlertTime = DateTime.MinValue;
        private int _alertsThisMinute = 0;
        private int _consecutiveAlerts = 0;
        private readonly Queue<DateTime> _alertTimestamps = new Queue<DateTime>();

        // 警报历史
        private readonly List<PerformanceAlert> _activeAlerts = new List<PerformanceAlert>();
        private readonly Queue<PerformanceAlert> _alertHistory = new Queue<PerformanceAlert>();
        private readonly Dictionary<AlertType, AlertStatistics> _alertStatistics = new Dictionary<AlertType, AlertStatistics>();

        // 警报规则
        private readonly List<AlertRule> _alertRules = new List<AlertRule>();
        private readonly Dictionary<string, AlertRule> _customRules = new Dictionary<string, AlertRule>();

        // 系统组件引用
        private PerformanceMonitor _performanceMonitor;
        private MemoryManagementSystem _memoryManager;
        private CoinObjectPool _objectPool;
        private AdvancedPerformanceDashboard _dashboard;

        // 当前性能状态
        private float _currentFPS = 60f;
        private float _currentMemoryMB = 0f;
        private float _currentFrameTime = 16.67f;
        private int _currentActiveCoins = 0;
        private float _lastGCTime = 0f;
        private int _gcCount = 0;

        // 通知系统
        private AlertNotificationManager _notificationManager;

        // 统计数据
        private AlertSystemStatistics _systemStatistics = new AlertSystemStatistics();

        #endregion

        #region Properties

        public bool IsAlertSystemActive => _alertSystemActive;
        public int ActiveAlertsCount => _activeAlerts.Count;
        public IReadOnlyList<PerformanceAlert> ActiveAlerts => _activeAlerts.AsReadOnly();
        public AlertSystemStatistics SystemStatistics => _systemStatistics;

        #endregion

        #region Events

        public event Action<PerformanceAlert> OnAlertTriggered;
        public event Action<PerformanceAlert> OnAlertResolved;
        public event Action<AlertEscalation> OnAlertEscalated;
        public event Action<AlertBatch> OnBatchAlertsTriggered;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            FindSystemComponents();
            InitializeAlertRules();
            InitializeNotificationManager();
            StartCoroutine(AlertMonitoringCoroutine());
        }

        private void Update()
        {
            if (!enableAlerts) return;

            UpdatePerformanceMetrics();
            CheckAlertCooldown();
        }

        private void OnDestroy()
        {
            CleanupNotificationManager();
        }

        #endregion

        #region Initialization

        private void FindSystemComponents()
        {
            _performanceMonitor = FindObjectOfType<PerformanceMonitor>();
            _memoryManager = FindObjectOfType<MemoryManagementSystem>();
            _objectPool = FindObjectOfType<CoinObjectPool>();
            _dashboard = FindObjectOfType<AdvancedPerformanceDashboard>();

            Debug.Log($"[PerformanceAlertSystem] Components found: " +
                     $"PerfMonitor: {_performanceMonitor != null}, " +
                     $"MemoryMgr: {_memoryManager != null}, " +
                     $"ObjectPool: {_objectPool != null}, " +
                     $"Dashboard: {_dashboard != null}");
        }

        private void InitializeAlertRules()
        {
            // FPS警报规则
            _alertRules.Add(new AlertRule
            {
                Id = "fps_warning",
                Name = "Low FPS Warning",
                Type = AlertType.Warning,
                Category = AlertCategory.Performance,
                Condition = data => data.FPS < fpsWarningThreshold && data.FPS >= fpsCriticalThreshold,
                Message = "FPS is below optimal level",
                Cooldown = 10f,
                EscalationThreshold = 3,
                Priority = AlertPriority.High,
                Enabled = true
            });

            _alertRules.Add(new AlertRule
            {
                Id = "fps_critical",
                Name = "Critical FPS",
                Type = AlertType.Critical,
                Category = AlertCategory.Performance,
                Condition = data => data.FPS < fpsCriticalThreshold,
                Message = "FPS is at critical level",
                Cooldown = 5f,
                EscalationThreshold = 1,
                Priority = AlertPriority.Critical,
                Enabled = true
            });

            // 内存警报规则
            _alertRules.Add(new AlertRule
            {
                Id = "memory_warning",
                Name = "High Memory Usage",
                Type = AlertType.Warning,
                Category = AlertCategory.Memory,
                Condition = data => data.MemoryMB > memoryWarningThreshold && data.MemoryMB < memoryCriticalThreshold,
                Message = "Memory usage is high",
                Cooldown = 15f,
                EscalationThreshold = 4,
                Priority = AlertPriority.Medium,
                Enabled = true
            });

            _alertRules.Add(new AlertRule
            {
                Id = "memory_critical",
                Name = "Critical Memory Usage",
                Type = AlertType.Critical,
                Category = AlertCategory.Memory,
                Condition = data => data.MemoryMB > memoryCriticalThreshold,
                Message = "Memory usage is at critical level",
                Cooldown = 10f,
                EscalationThreshold = 2,
                Priority = AlertPriority.Critical,
                Enabled = true
            });

            // 帧时间警报规则
            _alertRules.Add(new AlertRule
            {
                Id = "frame_time_warning",
                Name = "High Frame Time",
                Type = AlertType.Warning,
                Category = AlertCategory.Performance,
                Condition = data => data.FrameTime > frameTimeWarningThreshold && data.FrameTime < frameTimeCriticalThreshold,
                Message = "Frame time is elevated",
                Cooldown = 8f,
                EscalationThreshold = 3,
                Priority = AlertPriority.Medium,
                Enabled = true
            });

            // 垃圾回收警报规则
            _alertRules.Add(new AlertRule
            {
                Id = "gc_frequent",
                Name = "Frequent Garbage Collection",
                Type = AlertType.Warning,
                Category = AlertCategory.Memory,
                Condition = data => CalculateGCFrequency(data.Timestamp) > gcFrequencyWarningThreshold,
                Message = "Garbage collection is occurring frequently",
                Cooldown = 30f,
                EscalationThreshold = 5,
                Priority = AlertPriority.Medium,
                Enabled = true
            });

            // 金币性能警报规则
            _alertRules.Add(new AlertRule
            {
                Id = "coin_performance",
                Name = "Coin Performance Issue",
                Type = AlertType.Warning,
                Category = AlertCategory.Application,
                Condition = data => data.ActiveCoins > 80, // 假设80个是上限
                Message = "High number of active coins may impact performance",
                Cooldown = 20f,
                EscalationThreshold = 3,
                Priority = AlertPriority.Low,
                Enabled = true
            });

            Debug.Log($"[PerformanceAlertSystem] Initialized {_alertRules.Count} alert rules");
        }

        private void InitializeNotificationManager()
        {
            _notificationManager = new AlertNotificationManager();
            _notificationManager.Initialize(enableSoundAlerts, enableVisualAlerts, enableDesktopNotifications);
        }

        #endregion

        #region Alert Monitoring

        private IEnumerator AlertMonitoringCoroutine()
        {
            _alertSystemActive = true;

            while (enableAlerts)
            {
                // 检查所有警报规则
                CheckAllAlertRules();

                // 更新警报统计
                UpdateAlertStatistics();

                // 清理已解决的警报
                CleanupResolvedAlerts();

                yield return new WaitForSeconds(1f); // 每秒检查一次
            }
        }

        private void CheckAllAlertRules()
        {
            var currentData = CreateAlertData();
            var triggeredAlerts = new List<PerformanceAlert>();

            foreach (var rule in _alertRules)
            {
                if (!rule.Enabled) continue;

                // 检查冷却时间
                if (IsRuleInCooldown(rule)) continue;

                // 检查条件
                if (rule.Condition(currentData))
                {
                    var alert = CreateAlertFromRule(rule, currentData);
                    if (ShouldTriggerAlert(alert))
                    {
                        triggeredAlerts.Add(alert);
                        rule.LastTriggered = DateTime.UtcNow;
                    }
                }
            }

            // 批量处理警报
            if (triggeredAlerts.Count > 0)
            {
                ProcessAlertBatch(triggeredAlerts);
            }
        }

        private void UpdatePerformanceMetrics()
        {
            // 更新当前性能指标
            _currentFPS = GetFPS();
            _currentMemoryMB = GetMemoryUsage();
            _currentFrameTime = GetFrameTime();
            _currentActiveCoins = GetActiveCoinCount();

            // 跟踪垃圾回收
            var currentTime = Time.realtimeSinceStartup;
            if (GC.GetTotalMemory(false) < _memoryManager?.CurrentMemoryUsageMB * 1024 * 1024)
            {
                _gcCount++;
                _lastGCTime = currentTime;
            }
        }

        private AlertData CreateAlertData()
        {
            return new AlertData
            {
                Timestamp = DateTime.UtcNow,
                FPS = _currentFPS,
                MemoryMB = _currentMemoryMB,
                FrameTime = _currentFrameTime,
                ActiveCoins = _currentActiveCoins,
                GCCount = _gcCount,
                LastGCTime = _lastGCTime
            };
        }

        private PerformanceAlert CreateAlertFromRule(AlertRule rule, AlertData data)
        {
            return new PerformanceAlert
            {
                //Id = Guid.NewGuid().ToString(),
                //RuleId = rule.Id,
                //Type = rule.Type,
                //Category = rule.Category,
                //Priority = rule.Priority,
                //Title = rule.Name,
                Message = rule.Message,
                Timestamp = DateTime.UtcNow,
                //TriggerValue = GetTriggerValue(rule, data),
                //ThresholdValue = GetThresholdValue(rule),
                //Severity = CalculateSeverity(rule, data),
                //Data = new Dictionary<string, object>
                //{
                //    ["FPS"] = data.FPS,
                //    ["MemoryMB"] = data.MemoryMB,
                //    ["FrameTime"] = data.FrameTime,
                //    ["ActiveCoins"] = data.ActiveCoins
                //},
                //Resolved = false,
                //TriggerCount = 1,
                //FirstTriggered = DateTime.UtcNow,
                //LastTriggered = DateTime.UtcNow
            };
        }

        private bool ShouldTriggerAlert(PerformanceAlert alert)
        {
            // 检查全局冷却时间
            if (DateTime.UtcNow - _lastAlertTime < TimeSpan.FromSeconds(alertCooldown))
                return false;

            // 检查每分钟警报数量限制
            if (_alertsThisMinute >= maxAlertsPerMinute)
                return false;

            // 检查是否已存在相同类型的活跃警报
            //var existingAlert = _activeAlerts.FirstOrDefault(a => a.RuleId == alert.RuleId);
            //if (existingAlert != null)
            //{
            //    // 更新现有警报而不是创建新警报
            //    existingAlert.TriggerCount++;
            //    existingAlert.LastTriggered = DateTime.UtcNow;
            //    //existingAlert.Severity = Mathf.Max(existingAlert.Severity, alert.Severity);
            //    return false;
            //}

            return true;
        }

        #endregion

        #region Alert Processing

        private void ProcessAlertBatch(List<PerformanceAlert> alerts)
        {
            // 添加到活跃警报列表
            foreach (var alert in alerts)
            {
                _activeAlerts.Add(alert);
                _alertHistory.Enqueue(alert);
                
                // 更新统计
                //UpdateAlertStatistics(alert.Type);
                
                // 触发事件
                OnAlertTriggered?.Invoke(alert);
            }

            // 保持历史记录大小
            while (_alertHistory.Count > 1000)
                _alertHistory.Dequeue();

            // 发送通知
            SendNotifications(alerts);

            // 检查升级
            CheckForEscalation(alerts);

            // 更新警报计数
            _alertsThisMinute += alerts.Count;
            _lastAlertTime = DateTime.UtcNow;
            _consecutiveAlerts++;

            // 记录警报时间戳
            foreach (var alert in alerts)
            {
                _alertTimestamps.Enqueue(alert.Timestamp);
            }

            // 清理旧的警报时间戳
            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
            while (_alertTimestamps.Count > 0 && _alertTimestamps.Peek() < oneMinuteAgo)
            {
                _alertTimestamps.Dequeue();
            }

            // 触发批量警报事件
            if (alerts.Count > 1)
            {
                var batchAlert = new AlertBatch
                {
                    Alerts = alerts,
                    BatchTimestamp = DateTime.UtcNow,
                    TotalAlerts = alerts.Count
                };
                OnBatchAlertsTriggered?.Invoke(batchAlert);
            }

            //Debug.LogWarning($"[PerformanceAlertSystem] Triggered {alerts.Count} alerts: " +
            //               string.Join(", ", alerts.Select(a => a.Title)));
        }

        private void SendNotifications(List<PerformanceAlert> alerts)
        {
            if (_notificationManager == null) return;

            //foreach (var alert in alerts)
            //{
            //    // 发送声音警报
            //    if (enableSoundAlerts)
            //    {
            //        _notificationManager.PlaySoundAlert(alert.Type);
            //    }

            //    // 发送视觉警报
            //    if (enableVisualAlerts)
            //    {
            //        _notificationManager.ShowVisualAlert(alert);
            //    }

            //    // 发送桌面通知
            //    if (enableDesktopNotifications)
            //    {
            //        _notificationManager.SendDesktopNotification(alert);
            //    }

            //    // 写入日志
            //    if (enableLogAlerts)
            //    {
            //        LogAlert(alert);
            //    }
            //}
        }

        private void CheckForEscalation(List<PerformanceAlert> alerts)
        {
            if (!enableEscalation) return;

            foreach (var alert in alerts)
            {
                //if (alert.TriggerCount >= consecutiveAlertsToEscalate)
                //{
                //    EscalateAlert(alert);
                //}
            }
        }

        private void EscalateAlert(PerformanceAlert alert)
        {
            //// 升级警报类型
            //var escalatedType = alert.Type switch
            //{
            //    AlertType.Info => AlertType.Warning,
            //    AlertType.Warning => AlertType.Critical,
            //    _ => AlertType.Critical
            //};

            //if (escalatedType != alert.Type)
            //{
            //    var escalation = new AlertEscalation
            //    {
            //        OriginalAlert = alert,
            //        EscalatedType = escalatedType,
            //        EscalationTime = DateTime.UtcNow,
            //        Reason = $"Alert triggered {alert.TriggerCount} times consecutively"
            //    };

            //    alert.Type = escalatedType;
            //    alert.Severity = Mathf.Min(alert.Severity + 1, 5);

            //    OnAlertEscalated?.Invoke(escalation);

            //    Debug.LogError($"[PerformanceAlertSystem] Alert escalated: {alert.Title} -> {escalatedType}");
            //}
        }

        private void CleanupResolvedAlerts()
        {
            var currentData = CreateAlertData();
            var resolvedAlerts = new List<PerformanceAlert>();

            //foreach (var alert in _activeAlerts.ToList())
            //{
            //    var rule = _alertRules.FirstOrDefault(r => r.Id == alert.RuleId);
            //    if (rule != null && !rule.Condition(currentData))
            //    {
            //        // 警报已解决
            //        alert.Resolved = true;
            //        alert.ResolvedAt = DateTime.UtcNow;
            //        alert.Duration = alert.ResolvedAt.Value - alert.FirstTriggered;

            //        _activeAlerts.Remove(alert);
            //        resolvedAlerts.Add(alert);

            //        OnAlertResolved?.Invoke(alert);
            //    }
            //}

            //if (resolvedAlerts.Count > 0)
            //{
            //    _consecutiveAlerts = 0; // 重置连续警报计数
            //    Debug.Log($"[PerformanceAlertSystem] Resolved {resolvedAlerts.Count} alerts");
            //}
        }

        #endregion

        #region Utility Methods

        private void UpdateAlertStatistics(AlertType alertType)
        {
            if (!_alertStatistics.ContainsKey(alertType))
            {
                _alertStatistics[alertType] = new AlertStatistics();
            }

            var stats = _alertStatistics[alertType];
            stats.TotalCount++;
            stats.LastTriggered = DateTime.UtcNow;
        }

        private void UpdateAlertStatistics()
        {
            _systemStatistics.TotalAlerts = _alertHistory.Count;
            _systemStatistics.ActiveAlerts = _activeAlerts.Count;
            _systemStatistics.SystemUptime = DateTime.UtcNow - _systemStatistics.SystemStartTime;
            _systemStatistics.LastUpdate = DateTime.UtcNow;
        }

        private bool IsRuleInCooldown(AlertRule rule)
        {
            if (rule.LastTriggered == DateTime.MinValue) return false;
            
            var timeSinceLastTrigger = DateTime.UtcNow - rule.LastTriggered;
            return timeSinceLastTrigger < TimeSpan.FromSeconds(rule.Cooldown);
        }

        private float GetTriggerValue(AlertRule rule, AlertData data)
        {
            return rule.Id switch
            {
                "fps_warning" => data.FPS,
                "fps_critical" => data.FPS,
                "memory_warning" => data.MemoryMB,
                "memory_critical" => data.MemoryMB,
                "frame_time_warning" => data.FrameTime,
                "gc_frequent" => CalculateGCFrequency(data.Timestamp),
                "coin_performance" => data.ActiveCoins,
                _ => 0f
            };
        }

        private float GetThresholdValue(AlertRule rule)
        {
            return rule.Id switch
            {
                "fps_warning" => fpsWarningThreshold,
                "fps_critical" => fpsCriticalThreshold,
                "memory_warning" => memoryWarningThreshold,
                "memory_critical" => memoryCriticalThreshold,
                "frame_time_warning" => frameTimeWarningThreshold,
                "gc_frequent" => gcFrequencyWarningThreshold,
                "coin_performance" => 80f,
                _ => 0f
            };
        }

        private float CalculateSeverity(AlertRule rule, AlertData data)
        {
            var triggerValue = GetTriggerValue(rule, data);
            var thresholdValue = GetThresholdValue(rule);

            return rule.Type switch
            {
                AlertType.Info => 1f,
                AlertType.Warning => 2f + Mathf.Abs(triggerValue - thresholdValue) / thresholdValue,
                AlertType.Critical => 4f + Mathf.Abs(triggerValue - thresholdValue) / thresholdValue,
                _ => 1f
            };
        }

        private float CalculateGCFrequency(DateTime currentTime)
        {
            if (_lastGCTime == 0) return 0f;

            var timeDiff = (currentTime - DateTime.UtcNow.AddMinutes(-1)).TotalMinutes;
            if (timeDiff <= 0) return _gcCount;

            return _gcCount / (float)timeDiff;
        }

        private void CheckAlertCooldown()
        {
            // 每分钟重置警报计数
            if (_alertTimestamps.Count == 0) return;

            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
            if (_alertTimestamps.Peek() < oneMinuteAgo)
            {
                _alertsThisMinute = _alertTimestamps.Count;
            }
        }

        private void LogAlert(PerformanceAlert alert)
        {
            var logLevel = alert.Type switch
            {
                //AlertType.Info => "INFO",
                //AlertType.Warning => "WARNING",
                //AlertType.Critical => "ERROR",
                _ => "INFO"
            };

            //Debug.Log($"[PerformanceAlertSystem] [{logLevel}] {alert.Title}: {alert.Message} " +
            //            $"(Value: {alert.TriggerValue:F2}, Threshold: {alert.ThresholdValue:F2})");
        }

        // 性能数据获取方法
        private float GetFPS()
        {
            if (_performanceMonitor != null)
            {
                return _performanceMonitor.GetCurrentMetrics().averageFrameRate;
            }
            return 1f / Time.unscaledDeltaTime;
        }

        private float GetMemoryUsage()
        {
            if (_memoryManager != null)
            {
                return _memoryManager.CurrentMemoryUsageMB;
            }
            return GC.GetTotalMemory(false) / (1024f * 1024f);
        }

        private float GetFrameTime()
        {
            return Time.unscaledDeltaTime * 1000f; // 转换为毫秒
        }

        private int GetActiveCoinCount()
        {
            if (_objectPool != null)
            {
                return _objectPool.ActiveCoinCount;
            }
            return 0;
        }

        private void CleanupNotificationManager()
        {
            _notificationManager?.Cleanup();
        }

        #endregion

        #region Public API

        /// <summary>
        /// 手动触发警报
        /// </summary>
        public void TriggerAlert(string title, string message, AlertType type = AlertType.Warning)
        {
            var alert = new PerformanceAlert
            {
                //Id = Guid.NewGuid().ToString(),
                //Title = title,
                Message = message,
                //Type = type,
                Timestamp = DateTime.UtcNow,
                //TriggerCount = 1,
                //FirstTriggered = DateTime.UtcNow,
                //LastTriggered = DateTime.UtcNow
            };

            ProcessAlertBatch(new List<PerformanceAlert> { alert });
        }

        /// <summary>
        /// 添加自定义警报规则
        /// </summary>
        public void AddCustomAlertRule(AlertRule rule)
        {
            _customRules[rule.Id] = rule;
            _alertRules.Add(rule);
            Debug.Log($"[PerformanceAlertSystem] Added custom alert rule: {rule.Name}");
        }

        /// <summary>
        /// 移除警报规则
        /// </summary>
        public bool RemoveAlertRule(string ruleId)
        {
            var rule = _alertRules.FirstOrDefault(r => r.Id == ruleId);
            if (rule != null)
            {
                _alertRules.Remove(rule);
                _customRules.Remove(ruleId);
                Debug.Log($"[PerformanceAlertSystem] Removed alert rule: {rule.Name}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清除所有活跃警报
        /// </summary>
        public void ClearAllAlerts()
        {
            foreach (var alert in _activeAlerts)
            {
                //alert.Resolved = true;
                //alert.ResolvedAt = DateTime.UtcNow;
                OnAlertResolved?.Invoke(alert);
            }

            _activeAlerts.Clear();
            _consecutiveAlerts = 0;
            Debug.Log("[PerformanceAlertSystem] Cleared all active alerts");
        }

        /// <summary>
        /// 获取警报报告
        /// </summary>
        public AlertReport GetAlertReport()
        {
            return new AlertReport
            {
                GeneratedAt = DateTime.UtcNow,
                ActiveAlerts = new List<PerformanceAlert>(_activeAlerts),
                TotalAlerts = _alertHistory.Count,
                AlertStatistics = new Dictionary<AlertType, AlertStatistics>(_alertStatistics),
                SystemStatistics = _systemStatistics,
                RecentAlerts = _alertHistory.TakeLast(50).ToList()
            };
        }

        /// <summary>
        /// 启用/禁用警报系统
        /// </summary>
        public void SetAlertsEnabled(bool enabled)
        {
            enableAlerts = enabled;
            Debug.Log($"[PerformanceAlertSystem] Alerts {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 设置警报阈值
        /// </summary>
        public void SetThresholds(float fpsWarning, float fpsCritical, float memoryWarning, float memoryCritical)
        {
            fpsWarningThreshold = fpsWarning;
            fpsCriticalThreshold = fpsCritical;
            memoryWarningThreshold = memoryWarning;
            memoryCriticalThreshold = memoryCritical;
            
            Debug.Log($"[PerformanceAlertSystem] Thresholds updated - FPS: W:{fpsWarning} C:{fpsCritical}, Memory: W:{memoryWarning} C:{memoryCritical}");
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class SystemPerformanceAlert
    {
        public string Id;
        public string RuleId;
        public AlertType Type;
        public AlertCategory Category;
        public AlertPriority Priority;
        public string Title;
        public string Message;
        public DateTime Timestamp;
        public float TriggerValue;
        public float ThresholdValue;
        public float Severity;
        public Dictionary<string, object> Data;
        public bool Resolved;
        public DateTime? ResolvedAt;
        public TimeSpan Duration;
        public int TriggerCount;
        public DateTime FirstTriggered;
        public DateTime LastTriggered;
    }

    [System.Serializable]
    public class AlertRule
    {
        public string Id;
        public string Name;
        public AlertType Type;
        public AlertCategory Category;
        public AlertPriority Priority;
        public Func<AlertData, bool> Condition;
        public string Message;
        public float Cooldown;
        public int EscalationThreshold;
        public DateTime LastTriggered = DateTime.MinValue;
        public bool Enabled = true;
    }

    [System.Serializable]
    public class AlertData
    {
        public DateTime Timestamp;
        public float FPS;
        public float MemoryMB;
        public float FrameTime;
        public int ActiveCoins;
        public int GCCount;
        public float LastGCTime;
    }

    [System.Serializable]
    public class AlertStatistics
    {
        public int TotalCount;
        public DateTime LastTriggered;
        public TimeSpan AverageInterval;
        public float AverageSeverity;
    }

    [System.Serializable]
    public class AlertSystemStatistics
    {
        public int TotalAlerts;
        public int ActiveAlerts;
        public DateTime SystemStartTime = DateTime.UtcNow;
        public TimeSpan SystemUptime;
        public DateTime LastUpdate;
    }

    [System.Serializable]
    public class AlertEscalation
    {
        public PerformanceAlert OriginalAlert;
        public AlertType EscalatedType;
        public DateTime EscalationTime;
        public string Reason;
    }

    [System.Serializable]
    public class AlertBatch
    {
        public List<PerformanceAlert> Alerts;
        public DateTime BatchTimestamp;
        public int TotalAlerts;
    }

    [System.Serializable]
    public class AlertReport
    {
        public DateTime GeneratedAt;
        public List<PerformanceAlert> ActiveAlerts;
        public int TotalAlerts;
        public Dictionary<AlertType, AlertStatistics> AlertStatistics;
        public AlertSystemStatistics SystemStatistics;
        public List<PerformanceAlert> RecentAlerts;
    }

    #endregion

    #region Enums


    public enum AlertCategory
    {
        Performance,
        Memory,
        Application,
        System
    }

    public enum AlertPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    #endregion

    #region Notification Manager

    /// <summary>
    /// 警报通知管理器
    /// </summary>
    public class AlertNotificationManager
    {
        private bool _soundEnabled;
        private bool _visualEnabled;
        private bool _desktopEnabled;

        public void Initialize(bool soundEnabled, bool visualEnabled, bool desktopEnabled)
        {
            _soundEnabled = soundEnabled;
            _visualEnabled = visualEnabled;
            _desktopEnabled = desktopEnabled;
        }

        //public void PlaySoundAlert(AlertType alertType)
        //{
        //    if (!_soundEnabled) return;

        //    // 这里可以播放不同的声音文件
        //    Debug.Log($"[AlertNotificationManager] Playing sound for {alertType} alert");
        //}

        //public void ShowVisualAlert(PerformanceAlert alert)
        //{
        //    if (!_visualEnabled) return;

        //    // 这里可以显示视觉警报（屏幕闪烁、颜色变化等）
        //    Debug.Log($"[AlertNotificationManager] Showing visual alert for {alert.Title}");
        //}

        //public void SendDesktopNotification(PerformanceAlert alert)
        //{
        //    if (!_desktopEnabled) return;

        //    // 这里可以发送桌面通知
        //    //Debug.Log($"[AlertNotificationManager] Sending desktop notification: {alert.Title}");
        //}

        public void Cleanup()
        {
            // 清理资源
        }
    }

    #endregion
}