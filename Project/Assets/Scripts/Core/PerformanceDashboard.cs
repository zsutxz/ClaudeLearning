using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoinAnimation.Core;

namespace CoinAnimation.Core
{
    /// <summary>
    /// 高级性能监控仪表板
    /// Story 1.3 Enhancement - 实时性能可视化系统
    /// </summary>
    public class PerformanceDashboard : MonoBehaviour
    {
        #region Configuration

        [Header("Dashboard Settings")]
        [SerializeField] private bool enableDashboard = true;
        [SerializeField] private bool showInGame = true;
        [SerializeField] private Vector2 dashboardPosition = new Vector2(10, 10);
        [SerializeField] private Vector2 dashboardSize = new Vector2(300, 200);

        [Header("Monitoring Intervals")]
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private int historySize = 60; // 保存60秒的历史数据

        [Header("Performance Thresholds")]
        [SerializeField] private float fpsWarningThreshold = 55f;
        [SerializeField] private float fpsCriticalThreshold = 45f;
        [SerializeField] private float memoryWarningThreshold = 80f;
        [SerializeField] private float memoryCriticalThreshold = 120f;
        [SerializeField] private float poolEfficiencyWarningThreshold = 80f;

        [Header("Colors")]
        [SerializeField] private Color goodColor = Color.green;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);

        #endregion

        #region Private Fields

        private CoinObjectPool _objectPool;
        private MemoryManagementSystem _memorySystem;

        // 性能数据
        private readonly Queue<float> _fpsHistory = new Queue<float>();
        private readonly Queue<float> _memoryHistory = new Queue<float>();
        private readonly Queue<float> _poolEfficiencyHistory = new Queue<float>();
        private readonly Queue<int> _activeCoinsHistory = new Queue<int>();

        // 当前性能指标
        private float _currentFPS = 60f;
        private float _currentMemoryMB = 0f;
        private float _poolEfficiency = 100f;
        private int _activeCoinsCount = 0;
        private float _poolHitRate = 0f;

        // 更新时间
        private float _lastUpdateTime = 0f;
        private float _lastFPSUpdateTime = 0f;
        private int _frameCount = 0;

        // GUI样式
        private GUIStyle _labelStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _warningStyle;
        private Texture2D _backgroundTexture;

        // 性能统计
        private PerformanceStats _stats = new PerformanceStats();

        #endregion

        #region Properties

        public PerformanceStats CurrentStats => _stats;
        public bool IsEnabled => enableDashboard;

        #endregion

        #region Events

        public event Action<PerformanceAlert> OnPerformanceAlert;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeStyles();
            CreateBackgroundTexture();
        }

        private void Start()
        {
            FindSystemComponents();
            StartCoroutine(MonitoringCoroutine());
        }

        private void Update()
        {
            if (!enableDashboard) return;

            UpdateFPSCounter();
        }

        private void OnGUI()
        {
            if (!enableDashboard || !showInGame) return;

            DrawDashboard();
        }

        #endregion

        #region Initialization

        private void FindSystemComponents()
        {
            _objectPool = FindObjectOfType<CoinObjectPool>();
            _memorySystem = FindObjectOfType<MemoryManagementSystem>();

            if (_objectPool == null)
                Debug.LogWarning("[PerformanceDashboard] CoinObjectPool not found");
            if (_memorySystem == null)
                Debug.LogWarning("[PerformanceDashboard] MemoryManagementSystem not found");
        }

        private void InitializeStyles()
        {
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = Color.white },
                wordWrap = false
            };

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                wordWrap = false
            };

            _warningStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                wordWrap = false
            };
        }

        private void CreateBackgroundTexture()
        {
            _backgroundTexture = new Texture2D(1, 1);
            _backgroundTexture.SetPixel(0, 0, backgroundColor);
            _backgroundTexture.Apply();
        }

        #endregion

        #region Performance Monitoring

        private IEnumerator MonitoringCoroutine()
        {
            while (enableDashboard)
            {
                UpdatePerformanceMetrics();
                CheckPerformanceThresholds();
                UpdateStatistics();

                yield return new WaitForSeconds(updateInterval);
            }
        }

        private void UpdatePerformanceMetrics()
        {
            // 更新内存使用
            if (_memorySystem != null)
            {
                _currentMemoryMB = _memorySystem.CurrentMemoryUsageMB;
            }
            else
            {
                _currentMemoryMB = GC.GetTotalMemory(false) / (1024f * 1024f);
            }

            // 更新对象池效率
            if (_objectPool != null)
            {
                _activeCoinsCount = _objectPool.ActiveCoinCount;
                _poolEfficiency = CalculatePoolEfficiency();
                _poolHitRate = CalculatePoolHitRate();
            }

            // 添加到历史记录
            AddToHistory();
        }

        private void UpdateFPSCounter()
        {
            _frameCount++;
            float currentTime = Time.realtimeSinceStartup;

            if (currentTime >= _lastFPSUpdateTime + 1f)
            {
                _currentFPS = _frameCount / (currentTime - _lastFPSUpdateTime);
                _frameCount = 0;
                _lastFPSUpdateTime = currentTime;
            }
        }

        private float CalculatePoolEfficiency()
        {
            if (_objectPool == null) return 100f;

            int totalRequests = _objectPool.TotalRequests;
            int poolHits = _objectPool.PoolHits;

            return totalRequests > 0 ? (float)poolHits / totalRequests * 100f : 100f;
        }

        private float CalculatePoolHitRate()
        {
            if (_objectPool == null) return 0f;

            int available = _objectPool.AvailableCoinCount;
            int total = _objectPool.CurrentPoolSize;

            return total > 0 ? (float)available / total * 100f : 0f;
        }

        private void AddToHistory()
        {
            // 添加FPS历史
            _fpsHistory.Enqueue(_currentFPS);
            while (_fpsHistory.Count > historySize)
                _fpsHistory.Dequeue();

            // 添加内存历史
            _memoryHistory.Enqueue(_currentMemoryMB);
            while (_memoryHistory.Count > historySize)
                _memoryHistory.Dequeue();

            // 添加池效率历史
            _poolEfficiencyHistory.Enqueue(_poolEfficiency);
            while (_poolEfficiencyHistory.Count > historySize)
                _poolEfficiencyHistory.Dequeue();

            // 添加活跃金币历史
            _activeCoinsHistory.Enqueue(_activeCoinsCount);
            while (_activeCoinsHistory.Count > historySize)
                _activeCoinsHistory.Dequeue();
        }

        #endregion

        #region Performance Thresholds

        private void CheckPerformanceThresholds()
        {
            // 检查FPS阈值
            if (_currentFPS < fpsCriticalThreshold)
            {
                TriggerAlert(PerformanceAlertType.Critical, "FPS", $" critically low: {_currentFPS:F1}");
            }
            else if (_currentFPS < fpsWarningThreshold)
            {
                TriggerAlert(PerformanceAlertType.Warning, "FPS", $" low: {_currentFPS:F1}");
            }

            // 检查内存阈值
            if (_currentMemoryMB > memoryCriticalThreshold)
            {
                TriggerAlert(PerformanceAlertType.Critical, "Memory", $" critically high: {_currentMemoryMB:F1}MB");
            }
            else if (_currentMemoryMB > memoryWarningThreshold)
            {
                TriggerAlert(PerformanceAlertType.Warning, "Memory", $" high: {_currentMemoryMB:F1}MB");
            }

            // 检查池效率阈值
            if (_poolEfficiency < poolEfficiencyWarningThreshold)
            {
                TriggerAlert(PerformanceAlertType.Warning, "Pool Efficiency", $" low: {_poolEfficiency:F1}%");
            }
        }

        private void TriggerAlert(PerformanceAlertType type, string source, string message)
        {
            var alert = new PerformanceAlert
            {
                Type = type,
                Source = source,
                Message = message,
                Timestamp = DateTime.UtcNow,
                FPS = _currentFPS,
                MemoryMB = _currentMemoryMB,
                PoolEfficiency = _poolEfficiency
            };

            OnPerformanceAlert?.Invoke(alert);
            Debug.LogWarning($"[PerformanceDashboard] {type} Alert: {source} {message}");
        }

        #endregion

        #region Statistics

        private void UpdateStatistics()
        {
            if (_fpsHistory.Count > 0)
            {
                _stats.AverageFPS = CalculateAverage(_fpsHistory);
                _stats.MinFPS = CalculateMin(_fpsHistory);
                _stats.MaxFPS = CalculateMax(_fpsHistory);
            }

            if (_memoryHistory.Count > 0)
            {
                _stats.AverageMemoryMB = CalculateAverage(_memoryHistory);
                _stats.MinMemoryMB = CalculateMin(_memoryHistory);
                _stats.MaxMemoryMB = CalculateMax(_memoryHistory);
            }

            if (_poolEfficiencyHistory.Count > 0)
            {
                _stats.AveragePoolEfficiency = CalculateAverage(_poolEfficiencyHistory);
            }

            _stats.DataPoints = _fpsHistory.Count;
            _stats.LastUpdated = DateTime.UtcNow;
        }

        private float CalculateAverage(Queue<float> queue)
        {
            if (queue.Count == 0) return 0f;

            float sum = 0f;
            foreach (float value in queue)
                sum += value;

            return sum / queue.Count;
        }

        private float CalculateMin(Queue<float> queue)
        {
            if (queue.Count == 0) return 0f;

            float min = float.MaxValue;
            foreach (float value in queue)
                if (value < min) min = value;

            return min;
        }

        private float CalculateMax(Queue<float> queue)
        {
            if (queue.Count == 0) return 0f;

            float max = float.MinValue;
            foreach (float value in queue)
                if (value > max) max = value;

            return max;
        }

        #endregion

        #region GUI Rendering

        private void DrawDashboard()
        {
            GUI.DrawTexture(new Rect(dashboardPosition, dashboardSize), _backgroundTexture);

            GUILayout.BeginArea(new Rect(dashboardPosition.x + 5, dashboardPosition.y + 5, dashboardSize.x - 10, dashboardSize.y - 10));

            DrawHeader();
            DrawPerformanceMetrics();
            DrawPoolStatus();
            DrawAlerts();

            GUILayout.EndArea();
        }

        private void DrawHeader()
        {
            GUILayout.Label("🚀 Performance Dashboard", _headerStyle);
            GUILayout.Space(5);
        }

        private void DrawPerformanceMetrics()
        {
            GUILayout.BeginVertical();

            // FPS
            Color fpsColor = GetStatusColor(_currentFPS, fpsWarningThreshold, fpsCriticalThreshold);
            _labelStyle.normal.textColor = fpsColor;
            GUILayout.Label($"FPS: {_currentFPS:F1} (Avg: {_stats.AverageFPS:F1})", _labelStyle);

            // Memory
            Color memColor = GetStatusColor(_currentMemoryMB, memoryWarningThreshold, memoryCriticalThreshold, true);
            _labelStyle.normal.textColor = memColor;
            GUILayout.Label($"Memory: {_currentMemoryMB:F1}MB (Avg: {_stats.AverageMemoryMB:F1}MB)", _labelStyle);

            // Reset color
            _labelStyle.normal.textColor = Color.white;

            GUILayout.Space(5);
            GUILayout.EndVertical();
        }

        private void DrawPoolStatus()
        {
            if (_objectPool == null) return;

            GUILayout.BeginVertical();
            GUILayout.Label("🏊 Object Pool Status:", _headerStyle);

            GUILayout.Label($"Active Coins: {_activeCoinsCount}");
            GUILayout.Label($"Available: {_objectPool.AvailableCoinCount}");
            GUILayout.Label($"Pool Size: {_objectPool.CurrentPoolSize}/{_objectPool.MaxPoolSize}");
            GUILayout.Label($"Efficiency: {_poolEfficiency:F1}%");
            GUILayout.Label($"Hit Rate: {_poolHitRate:F1}%");

            GUILayout.EndVertical();
        }

        private void DrawAlerts()
        {
            // 这里可以添加警告显示逻辑
            if (_currentFPS < fpsWarningThreshold || _currentMemoryMB > memoryWarningThreshold)
            {
                _warningStyle.normal.textColor = Color.yellow;
                GUILayout.Label("⚠️ Performance Issues Detected", _warningStyle);
            }
        }

        private Color GetStatusColor(float value, float warningThreshold, float criticalThreshold, bool inverse = false)
        {
            if (inverse)
            {
                if (value > criticalThreshold) return criticalColor;
                if (value > warningThreshold) return warningColor;
                return goodColor;
            }
            else
            {
                if (value < criticalThreshold) return criticalColor;
                if (value < warningThreshold) return warningColor;
                return goodColor;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// 启用/禁用仪表板
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            enableDashboard = enabled;
        }

        /// <summary>
        /// 设置仪表板位置
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            dashboardPosition = position;
        }

        /// <summary>
        /// 获取当前性能报告
        /// </summary>
        public PerformanceReport GetPerformanceReport()
        {
            return new PerformanceReport
            {
                Timestamp = DateTime.UtcNow,
                CurrentFPS = _currentFPS,
                CurrentMemoryMB = _currentMemoryMB,
                ActiveCoinsCount = _activeCoinsCount,
                PoolEfficiency = _poolEfficiency,
                Statistics = _stats,
                HistoryDataPoints = _fpsHistory.Count
            };
        }

        /// <summary>
        /// 导出性能数据为CSV格式
        /// </summary>
        public string ExportToCSV()
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Timestamp,FPS,MemoryMB,PoolEfficiency,ActiveCoins");

            // 这里简化处理，实际应用中需要更完整的时间戳记录
            for (int i = 0; i < _fpsHistory.Count; i++)
            {
                csv.AppendLine($"{i},{_fpsHistory.ToArray()[i]},{_memoryHistory.ToArray()[i]},{_poolEfficiencyHistory.ToArray()[i]},{_activeCoinsHistory.ToArray()[i]}");
            }

            return csv.ToString();
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (_backgroundTexture != null)
                Destroy(_backgroundTexture);
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class PerformanceStats
    {
        public float AverageFPS;
        public float MinFPS;
        public float MaxFPS;
        public float AverageMemoryMB;
        public float MinMemoryMB;
        public float MaxMemoryMB;
        public float AveragePoolEfficiency;
        public int DataPoints;
        public DateTime LastUpdated;
    }

    [System.Serializable]
    public class PerformanceAlert
    {
        public PerformanceAlertType Type;
        public string Source;
        public string Message;
        public DateTime Timestamp;
        public float FPS;
        public float MemoryMB;
        public float PoolEfficiency;
    }

    public enum PerformanceAlertType
    {
        Info,
        Warning,
        Critical
    }

    [System.Serializable]
    public class PerformanceReport
    {
        public DateTime Timestamp;
        public float CurrentFPS;
        public float CurrentMemoryMB;
        public int ActiveCoinsCount;
        public float PoolEfficiency;
        public PerformanceStats Statistics;
        public int HistoryDataPoints;
    }

    #endregion
}