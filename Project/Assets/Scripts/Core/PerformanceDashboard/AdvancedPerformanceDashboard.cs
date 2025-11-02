
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CoinAnimation.Core
{
    /// <summary>
    /// 高级性能指标仪表板
    /// Story 2.1 Task 4.1 - 运行时性能可视化界面
    /// </summary>
    public class AdvancedPerformanceDashboard : MonoBehaviour
    {
        #region Configuration

        [Header("Dashboard Settings")]
        [SerializeField] private bool enableDashboard = true;
        [SerializeField] private bool showInRuntime = true;
        [SerializeField] private bool showInEditor = true;
        [SerializeField] private DashboardLayout layout = DashboardLayout.Compact;
        [SerializeField] private DashboardTheme theme = DashboardTheme.Dark;

        [Header("Update Settings")]
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private int historySize = 300; // 5分钟@10Hz
        [SerializeField] private bool smoothData = true;
        [SerializeField] private int smoothingWindow = 5;

        [Header("Visualization Settings")]
        [SerializeField] private bool showFPS = true;
        [SerializeField] private bool showMemory = true;
        [SerializeField] private bool showCoinPerformance = true;
        [SerializeField] private bool showTrends = true;
        [SerializeField] private bool showAlerts = true;

        [Header("Chart Settings")]
        [SerializeField] private int chartDataPoints = 60;
        [SerializeField] private Color normalColor = Color.green;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);

        #endregion

        #region Private Fields

        // 组件引用
        private Canvas _dashboardCanvas;
        private GameObject _dashboardPanel;
        private UnityEngine.UI.Text _titleText;
        private UnityEngine.UI.Text _fpsText;
        private UnityEngine.UI.Text _memoryText;
        private UnityEngine.UI.Text _coinText;
        private UnityEngine.UI.Text _statusText;
        private UnityEngine.UI.Image _trendIndicator;
        private RawImage _chartImage;
        private GameObject _alertPanel;

        // 数据管理
        private readonly Queue<PerformanceDataPoint> _performanceHistory = new Queue<PerformanceDataPoint>();
        private readonly Queue<float> _fpsHistory = new Queue<float>();
        private readonly Queue<float> _memoryHistory = new Queue<float>();
        private readonly Queue<int> _coinHistory = new Queue<int>();

        // 系统组件引用
        private PerformanceMonitor _performanceMonitor;
        private MemoryManagementSystem _memoryManager;
        private CoinObjectPool _objectPool;
        private MemoryUsagePatternAnalyzer _patternAnalyzer;
        private MemoryLeakDetector _leakDetector;
        private MemoryPressureManager _pressureManager;
        private IAdaptiveQualityManager _qualityManager;

        // 仪表板状态
        private DashboardState _currentState = DashboardState.Normal;
        private float _lastUpdateTime = 0f;
        private bool _isInitialized = false;
        private Texture2D _chartTexture;
        private Color[] _chartPixels;

        // 可视化数据
        private float _currentFPS = 60f;
        private float _currentMemoryMB = 0f;
        private int _currentActiveCoins = 0;
        private PerformanceTrend _currentTrend = PerformanceTrend.Stable;
        private int _activeAlerts = 0;

        // 性能统计
        private DashboardStatistics _statistics = new DashboardStatistics();

        #endregion

        #region Properties

        public DashboardState CurrentState => _currentState;
        public DashboardStatistics Statistics => _statistics;
        public bool IsVisible => _dashboardPanel?.activeInHierarchy ?? false;
        public bool IsEnabled => enableDashboard;

        #endregion

        #region Events

        public event Action<PerformanceAlert> OnPerformanceAlert;
        public event Action<DashboardState> OnStateChanged;
        public event Action<PerformanceSnapshot> OnDataUpdated;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeTheme();
            CreateChartTexture();
        }

        private void Start()
        {
            FindSystemComponents();
            InitializeDashboard();
            StartCoroutine(UpdateCoroutine());
        }

        private void Update()
        {
            if (!enableDashboard || !_isInitialized) return;

            HandleInput();
        }

        private void OnDestroy()
        {
            CleanupResources();
        }

        #endregion

        #region Initialization

        private void InitializeTheme()
        {
            switch (theme)
            {
                case DashboardTheme.Dark:
                    backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
                    normalColor = Color.green;
                    warningColor = Color.yellow;
                    criticalColor = Color.red;
                    break;
                case DashboardTheme.Light:
                    backgroundColor = new Color(0.9f, 0.9f, 0.9f, 0.9f);
                    normalColor = new Color(0, 0.5f, 0);
                    warningColor = new Color(1f, 0.5f, 0);
                    criticalColor = new Color(0.8f, 0, 0);
                    break;
            }
        }

        private void CreateChartTexture()
        {
            _chartTexture = new Texture2D(chartDataPoints, 64, TextureFormat.RGBA32, false);
            _chartPixels = new Color[chartDataPoints * 64];
            _chartTexture.filterMode = FilterMode.Point;
        }

        private void FindSystemComponents()
        {
            _performanceMonitor = FindObjectOfType<PerformanceMonitor>();
            _memoryManager = FindObjectOfType<MemoryManagementSystem>();
            _objectPool = FindObjectOfType<CoinObjectPool>();
            _patternAnalyzer = FindObjectOfType<MemoryUsagePatternAnalyzer>();
            _leakDetector = FindObjectOfType<MemoryLeakDetector>();
            _pressureManager = FindObjectOfType<MemoryPressureManager>();
            //_qualityManager = FindObjectOfType<IAdaptiveQualityManager>();

            Debug.Log($"[AdvancedPerformanceDashboard] Components found: " +
                     $"PerfMonitor: {_performanceMonitor != null}, " +
                     $"MemoryMgr: {_memoryManager != null}, " +
                     $"ObjectPool: {_objectPool != null}");
        }

        private void InitializeDashboard()
        {
            CreateDashboardCanvas();
            CreateDashboardPanel();
            CreateUIElements();
            
            _isInitialized = true;
            Debug.Log("[AdvancedPerformanceDashboard] Dashboard initialized successfully");
        }

        private void CreateDashboardCanvas()
        {
            // 创建Canvas
            var canvasGO = new GameObject("PerformanceDashboardCanvas");
            canvasGO.transform.SetParent(transform);

            _dashboardCanvas = canvasGO.AddComponent<Canvas>();
            _dashboardCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _dashboardCanvas.sortingOrder = 1000;

            var canvasScaler = canvasGO.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            canvasGO.AddComponent<GraphicRaycaster>();
        }

        private void CreateDashboardPanel()
        {
            // 创建主面板
            _dashboardPanel = new GameObject("DashboardPanel");
            _dashboardPanel.transform.SetParent(_dashboardCanvas.transform, false);

            var panelRect = _dashboardPanel.AddComponent<RectTransform>();
            
            switch (layout)
            {
                case DashboardLayout.Compact:
                    panelRect.anchorMin = new Vector2(0, 1);
                    panelRect.anchorMax = new Vector2(0, 1);
                    panelRect.pivot = new Vector2(0, 1);
                    panelRect.anchoredPosition = new Vector2(10, -10);
                    panelRect.sizeDelta = new Vector2(300, 200);
                    break;
                case DashboardLayout.Expanded:
                    panelRect.anchorMin = new Vector2(0, 0);
                    panelRect.anchorMax = new Vector2(1, 1);
                    panelRect.offsetMin = Vector2.zero;
                    panelRect.offsetMax = Vector2.zero;
                    break;
                case DashboardLayout.TopBar:
                    panelRect.anchorMin = new Vector2(0, 1);
                    panelRect.anchorMax = new Vector2(1, 1);
                    panelRect.pivot = new Vector2(0.5f, 1);
                    panelRect.anchoredPosition = new Vector2(0, 0);
                    panelRect.sizeDelta = new Vector2(0, 100);
                    break;
            }

            // 添加背景
            var background = _dashboardPanel.AddComponent<Image>();
            background.color = backgroundColor;

            // 添加阴影效果
            var shadow = _dashboardPanel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(2, -2);
        }

        private void CreateUIElements()
        {
            CreateTitle();
            CreateMetricsPanel();
            CreateChartPanel();
            CreateAlertPanel();
            CreateControlPanel();
        }

        private void CreateTitle()
        {
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(_dashboardPanel.transform, false);

            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.9f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(10, 0);
            titleRect.offsetMax = new Vector2(-10, -10);

            _titleText = titleGO.AddComponent<UnityEngine.UI.Text>();
            _titleText.text = "🚀 Performance Dashboard";
            _titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _titleText.fontSize = 16;
            _titleText.fontStyle = FontStyle.Bold;
            _titleText.color = Color.white;
            _titleText.alignment = TextAnchor.MiddleCenter;
        }

        private void CreateMetricsPanel()
        {
            var metricsGO = new GameObject("MetricsPanel");
            metricsGO.transform.SetParent(_dashboardPanel.transform, false);

            var metricsRect = metricsGO.AddComponent<RectTransform>();
            metricsRect.anchorMin = new Vector2(0, 0.5f);
            metricsRect.anchorMax = new Vector2(1, 0.85f);
            metricsRect.offsetMin = new Vector2(10, 0);
            metricsRect.offsetMax = new Vector2(-10, 0);

            // FPS 显示
            var fpsGO = CreateMetricItem(metricsGO, "FPS", new Vector2(0, 0.8f), new Vector2(1, 1));
            _fpsText = fpsGO.GetComponent<UnityEngine.UI.Text>();

            // 内存显示
            var memoryGO = CreateMetricItem(metricsGO, "Memory", new Vector2(0, 0.5f), new Vector2(1, 0.75f));
            _memoryText = memoryGO.GetComponent<UnityEngine.UI.Text>();

            // 金币性能显示
            var coinGO = CreateMetricItem(metricsGO, "Coins", new Vector2(0, 0.2f), new Vector2(1, 0.45f));
            _coinText = coinGO.GetComponent<UnityEngine.UI.Text>();

            // 状态显示
            var statusGO = CreateMetricItem(metricsGO, "Status", new Vector2(0, 0), new Vector2(1, 0.2f));
            _statusText = statusGO.GetComponent<UnityEngine.UI.Text>();
        }

        private GameObject CreateMetricItem(GameObject parent, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            var itemGO = new GameObject($"Metric_{label}");
            itemGO.transform.SetParent(parent.transform, false);

            var itemRect = itemGO.AddComponent<RectTransform>();
            itemRect.anchorMin = anchorMin;
            itemRect.anchorMax = anchorMax;
            itemRect.offsetMin = Vector2.zero;
            itemRect.offsetMax = Vector2.zero;

            var text = itemGO.AddComponent<UnityEngine.UI.Text>();
            text.text = $"{label}: --";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 12;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;

            return itemGO;
        }

        private void CreateChartPanel()
        {
            var chartGO = new GameObject("ChartPanel");
            chartGO.transform.SetParent(_dashboardPanel.transform, false);

            var chartRect = chartGO.AddComponent<RectTransform>();
            chartRect.anchorMin = new Vector2(0, 0.1f);
            chartRect.anchorMax = new Vector2(0.7f, 0.45f);
            chartRect.offsetMin = new Vector2(10, 0);
            chartRect.offsetMax = new Vector2(-10, 0);

            // 图表背景
            var chartBackground = chartGO.AddComponent<Image>();
            chartBackground.color = new Color(0, 0, 0, 0.3f);

            // 图表图像
            var chartImageGO = new GameObject("ChartImage");
            chartImageGO.transform.SetParent(chartGO.transform, false);

            var chartImageRect = chartImageGO.AddComponent<RectTransform>();
            chartImageRect.anchorMin = Vector2.zero;
            chartImageRect.anchorMax = Vector2.one;
            chartImageRect.offsetMin = new Vector2(5, 5);
            chartImageRect.offsetMax = new Vector2(-5, -5);

            _chartImage = chartImageGO.AddComponent<RawImage>();
            _chartImage.texture = _chartTexture;
            _chartImage.color = Color.white;

            // 趋势指示器
            var trendGO = new GameObject("TrendIndicator");
            trendGO.transform.SetParent(chartGO.transform, false);

            var trendRect = trendGO.AddComponent<RectTransform>();
            trendRect.anchorMin = new Vector2(0.8f, 0.3f);
            trendRect.anchorMax = new Vector2(1, 0.7f);
            trendRect.offsetMin = Vector2.zero;
            trendRect.offsetMax = Vector2.zero;

            _trendIndicator = trendGO.AddComponent<Image>();
            _trendIndicator.color = normalColor;
        }

        private void CreateAlertPanel()
        {
            var alertGO = new GameObject("AlertPanel");
            alertGO.transform.SetParent(_dashboardPanel.transform, false);

            var alertRect = alertGO.AddComponent<RectTransform>();
            alertRect.anchorMin = new Vector2(0.7f, 0.1f);
            alertRect.anchorMax = new Vector2(1, 0.45f);
            alertRect.offsetMin = new Vector2(5, 0);
            alertRect.offsetMax = new Vector2(-10, 0);

            // 警报背景
            var alertBackground = alertGO.AddComponent<Image>();
            alertBackground.color = new Color(0, 0, 0, 0.2f);

            // 警报列表
            _alertPanel = new GameObject("AlertList");
            _alertPanel.transform.SetParent(alertGO.transform, false);

            var alertListRect = _alertPanel.AddComponent<RectTransform>();
            alertListRect.anchorMin = Vector2.zero;
            alertListRect.anchorMax = Vector2.one;
            alertListRect.offsetMin = new Vector2(5, 5);
            alertListRect.offsetMax = new Vector2(-5, -5);

            // 初始时隐藏警报面板
            alertGO.SetActive(false);
        }

        private void CreateControlPanel()
        {
            var controlGO = new GameObject("ControlPanel");
            controlGO.transform.SetParent(_dashboardPanel.transform, false);

            var controlRect = controlGO.AddComponent<RectTransform>();
            controlRect.anchorMin = new Vector2(0.8f, 0);
            controlRect.anchorMax = new Vector2(1, 0.08f);
            controlRect.offsetMin = new Vector2(0, 5);
            controlRect.offsetMax = new Vector2(-10, 0);

            // 隐藏/显示按钮
            var toggleButtonGO = new GameObject("ToggleButton");
            toggleButtonGO.transform.SetParent(controlGO.transform, false);

            var buttonRect = toggleButtonGO.AddComponent<RectTransform>();
            buttonRect.anchorMin = Vector2.zero;
            buttonRect.anchorMax = Vector2.one;
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;

            var button = toggleButtonGO.AddComponent<Button>();
            button.onClick.AddListener(ToggleDashboard);

            var buttonText = toggleButtonGO.AddComponent<UnityEngine.UI.Text>();
            buttonText.text = "👁";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 14;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
        }

        #endregion

        #region Update Coroutine

        private IEnumerator UpdateCoroutine()
        {
            while (enableDashboard)
            {
                if (Time.time > _lastUpdateTime + updateInterval)
                {
                    CollectPerformanceData();
                    UpdateVisualization();
                    CheckForAlerts();
                    UpdateStatistics();
                }

                yield return new WaitForSeconds(0.1f); // 高频更新
            }
        }

        private void CollectPerformanceData()
        {
            // 收集性能数据
            _currentFPS = GetFPS();
            _currentMemoryMB = GetMemoryUsage();
            _currentActiveCoins = GetActiveCoinCount();
            _currentTrend = CalculateTrend();

            // 添加到历史记录
            _fpsHistory.Enqueue(_currentFPS);
            _memoryHistory.Enqueue(_currentMemoryMB);
            _coinHistory.Enqueue(_currentActiveCoins);

            // 保持历史记录大小
            while (_fpsHistory.Count > historySize)
            {
                _fpsHistory.Dequeue();
                _memoryHistory.Dequeue();
                _coinHistory.Dequeue();
            }

            // 创建数据点
            var dataPoint = new PerformanceDataPoint
            {
                Timestamp = DateTime.UtcNow,
                FPS = _currentFPS,
                MemoryMB = _currentMemoryMB,
                ActiveCoins = _currentActiveCoins,
                Trend = _currentTrend
            };

            _performanceHistory.Enqueue(dataPoint);
            while (_performanceHistory.Count > historySize)
                _performanceHistory.Dequeue();

            _lastUpdateTime = Time.time;

            // 触发事件
            //OnDataUpdated?.Invoke(dataPoint);
        }

        private void UpdateVisualization()
        {
            if (!_isInitialized) return;

            UpdateMetricsDisplay();
            UpdateChart();
            UpdateTrendIndicator();
            UpdateAlerts();
        }

        private void UpdateMetricsDisplay()
        {
            // 更新FPS显示
            if (showFPS && _fpsText != null)
            {
                var fpsColor = GetStatusColor(_currentFPS, 55f, 45f);
                _fpsText.text = $"FPS: {_currentFPS:F1}";
                _fpsText.color = fpsColor;
            }

            // 更新内存显示
            if (showMemory && _memoryText != null)
            {
                var memoryColor = GetStatusColor(_currentMemoryMB, 80f, 120f, true);
                _memoryText.text = $"Memory: {_currentMemoryMB:F1}MB";
                _memoryText.color = memoryColor;
            }

            // 更新金币显示
            if (showCoinPerformance && _coinText != null)
            {
                _coinText.text = $"Coins: {_currentActiveCoins} active";
                _coinText.color = Color.white;
            }

            // 更新状态显示
            if (_statusText != null)
            {
                _statusText.text = $"State: {_currentState} | Alerts: {_activeAlerts}";
                _statusText.color = GetStateColor(_currentState);
            }
        }

        private void UpdateChart()
        {
            if (_chartTexture == null || _chartImage == null) return;

            // 清除图表
            Array.Clear(_chartPixels, 0, _chartPixels.Length);

            // 绘制FPS图表
            DrawLineChart(_fpsHistory, normalColor, 0, 31);

            // 绘制内存图表
            DrawLineChart(_memoryHistory, warningColor, 32, 63);

            // 应用到纹理
            _chartTexture.SetPixels(_chartPixels);
            _chartTexture.Apply();
        }

        private void DrawLineChart(Queue<float> data, Color color, int yOffset, int yHeight)
        {
            if (data.Count < 2) return;

            var dataArray = data.ToArray();
            var minValue = dataArray.Min();
            var maxValue = dataArray.Max();
            var range = maxValue - minValue;
            if (range < 0.001f) range = 1f;

            for (int i = 1; i < dataArray.Length; i++)
            {
                var x1 = (i - 1) * chartDataPoints / dataArray.Length;
                var x2 = i * chartDataPoints / dataArray.Length;

                var y1 = yOffset + (dataArray[i - 1] - minValue) / range * yHeight;
                var y2 = yOffset + (dataArray[i] - minValue) / range * yHeight;

                DrawLine(x1, y1, x2, y2, color);
            }
        }

        private void DrawLine(float x1, float y1, float x2, float y2, Color color)
        {
            x1 = Mathf.Clamp(x1, 0, chartDataPoints - 1);
            x2 = Mathf.Clamp(x2, 0, chartDataPoints - 1);
            y1 = Mathf.Clamp(y1, 0, 63);
            y2 = Mathf.Clamp(y2, 0, 63);

            var dx = x2 - x1;
            var dy = y2 - y1;
            var steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));

            for (int i = 0; i <= steps; i++)
            {
                var t = i / steps;
                var x = Mathf.Round(x1 + dx * t);
                var y = Mathf.Round(y1 + dy * t);

                var index = (int)y * chartDataPoints + (int)x;
                if (index >= 0 && index < _chartPixels.Length)
                {
                    _chartPixels[index] = color;
                }
            }
        }

        private void UpdateTrendIndicator()
        {
            if (_trendIndicator == null) return;

            var trendColor = _currentTrend switch
            {
                PerformanceTrend.Improving => Color.green,
                PerformanceTrend.Degrading => Color.red,
                _ => Color.yellow
            };

            _trendIndicator.color = trendColor;
        }

        private void UpdateAlerts()
        {
            if (_alertPanel == null) return;

            // 更新警报显示
            _alertPanel.SetActive(_activeAlerts > 0);
        }

        #endregion

        #region Alert System

        private void CheckForAlerts()
        {
            _activeAlerts = 0;
            var alerts = new List<PerformanceAlert>();

            // FPS警报
            if (_currentFPS < 45f)
            {
                alerts.Add(new PerformanceAlert
                {
                    //Type = AlertType.Critical,
                    Message = $"Critical FPS: {_currentFPS:F1}",
                    Timestamp = DateTime.UtcNow
                });
                _activeAlerts++;
            }
            else if (_currentFPS < 55f)
            {
                alerts.Add(new PerformanceAlert
                {
                    //Type = AlertType.Warning,
                    Message = $"Low FPS: {_currentFPS:F1}",
                    Timestamp = DateTime.UtcNow
                });
                _activeAlerts++;
            }

            // 内存警报
            if (_currentMemoryMB > 120f)
            {
                alerts.Add(new PerformanceAlert
                {
                    //Type = AlertType.Critical,
                    Message = $"Critical Memory: {_currentMemoryMB:F1}MB",
                    Timestamp = DateTime.UtcNow
                });
                _activeAlerts++;
            }
            else if (_currentMemoryMB > 80f)
            {
                alerts.Add(new PerformanceAlert
                {
                    //Type = AlertType.Warning,
                    Message = $"High Memory: {_currentMemoryMB:F1}MB",
                    Timestamp = DateTime.UtcNow
                });
                _activeAlerts++;
            }

            // 触发警报事件
            foreach (var alert in alerts)
            {
                OnPerformanceAlert?.Invoke(alert);
            }

            // 更新状态
            UpdateDashboardState();
        }

        private void UpdateDashboardState()
        {
            var newState = _currentState;

            if (_activeAlerts >= 2)
                newState = DashboardState.Critical;
            else if (_activeAlerts >= 1)
                newState = DashboardState.Warning;
            else if (_currentTrend == PerformanceTrend.Degrading)
                newState = DashboardState.Monitoring;
            else
                newState = DashboardState.Normal;

            if (newState != _currentState)
            {
                _currentState = newState;
                OnStateChanged?.Invoke(_currentState);
            }
        }

        #endregion

        #region Data Collection Methods

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

        private int GetActiveCoinCount()
        {
            if (_objectPool != null)
            {
                return _objectPool.ActiveCoinCount;
            }
            return 0;
        }

        private PerformanceTrend CalculateTrend()
        {
            if (_fpsHistory.Count < 10) return PerformanceTrend.Stable;

            var recentAverage = _fpsHistory.TakeLast(5).Average();
            var olderAverage = _fpsHistory.Take(5).Average();

            var difference = recentAverage - olderAverage;
            var threshold = 2f;

            if (difference > threshold)
                return PerformanceTrend.Improving;
            else if (difference < -threshold)
                return PerformanceTrend.Degrading;
            else
                return PerformanceTrend.Stable;
        }

        #endregion

        #region Utility Methods

        private Color GetStatusColor(float value, float warningThreshold, float criticalThreshold, bool inverse = false)
        {
            if (inverse)
            {
                if (value > criticalThreshold) return criticalColor;
                if (value > warningThreshold) return warningColor;
                return normalColor;
            }
            else
            {
                if (value < criticalThreshold) return criticalColor;
                if (value < warningThreshold) return warningColor;
                return normalColor;
            }
        }

        private Color GetStateColor(DashboardState state)
        {
            return state switch
            {
                DashboardState.Critical => criticalColor,
                DashboardState.Warning => warningColor,
                DashboardState.Monitoring => Color.yellow,
                _ => normalColor
            };
        }

        private void HandleInput()
        {
            // 切换显示/隐藏
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ToggleDashboard();
            }
        }

        private void ToggleDashboard()
        {
            if (_dashboardPanel != null)
            {
                var isVisible = _dashboardPanel.activeInHierarchy;
                _dashboardPanel.SetActive(!isVisible);
            }
        }

        private void UpdateStatistics()
        {
            _statistics.TotalUpdates++;
            
            if (_fpsHistory.Count > 0)
            {
                _statistics.AverageFPS = _fpsHistory.Average();
                _statistics.MinFPS = _fpsHistory.Min();
                _statistics.MaxFPS = _fpsHistory.Max();
            }

            if (_memoryHistory.Count > 0)
            {
                _statistics.AverageMemoryMB = _memoryHistory.Average();
                _statistics.MinMemoryMB = _memoryHistory.Min();
                _statistics.MaxMemoryMB = _memoryHistory.Max();
            }

            _statistics.LastUpdateTime = DateTime.UtcNow;
        }

        private void CleanupResources()
        {
            if (_chartTexture != null)
            {
                Destroy(_chartTexture);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// 获取性能快照
        /// </summary>
        public PerformanceSnapshot GetCurrentSnapshot()
        {
            return new PerformanceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                FPS = _currentFPS,
                MemoryMB = _currentMemoryMB,
                ActiveCoins = _currentActiveCoins,
                Trend = _currentTrend,
                State = _currentState,
                ActiveAlerts = _activeAlerts
            };
        }

        /// <summary>
        /// 获取历史数据
        /// </summary>
        public List<PerformanceDataPoint> GetHistoryData(int maxPoints = -1)
        {
            var data = _performanceHistory.ToList();
            if (maxPoints > 0 && data.Count > maxPoints)
            {
                return data.TakeLast(maxPoints).ToList();
            }
            return data;
        }

        /// <summary>
        /// 设置仪表板布局
        /// </summary>
        public void SetLayout(DashboardLayout newLayout)
        {
            layout = newLayout;
            // 重新初始化UI布局
            if (_isInitialized)
            {
                // 销毁现有UI并重新创建
                if (_dashboardPanel != null)
                {
                    Destroy(_dashboardPanel);
                }
                InitializeDashboard();
            }
        }

        /// <summary>
        /// 设置主题
        /// </summary>
        public void SetTheme(DashboardTheme newTheme)
        {
            theme = newTheme;
            InitializeTheme();
        }

        /// <summary>
        /// 启用/禁用仪表板
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            enableDashboard = enabled;
            if (_dashboardPanel != null)
            {
                _dashboardPanel.SetActive(enabled);
            }
        }

        /// <summary>
        /// 导出性能数据
        /// </summary>
        public string ExportData(string format = "csv")
        {
            var data = _performanceHistory.ToList();
            
            return format.ToLower() switch
            {
                "csv" => ExportToCSV(data),
                "json" => ExportToJSON(data),
                _ => ExportToCSV(data)
            };
        }

        private string ExportToCSV(List<PerformanceDataPoint> data)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Timestamp,FPS,MemoryMB,ActiveCoins,Trend,State");

            //foreach (var point in data)
            //{
            //    csv.AppendLine($"{point.Timestamp:yyyy-MM-dd HH:mm:ss},{point.FPS:F2},{point.MemoryMB:F2},{point.ActiveCoins},{point.Trend},{point.State}");
            //}

            return csv.ToString();
        }

        private string ExportToJSON(List<PerformanceDataPoint> data)
        {
            return JsonUtility.ToJson(new { data = data, exportedAt = DateTime.UtcNow }, true);
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class PerformanceDataPoint
    {
        public DateTime Timestamp;
        public float FPS;
        public float MemoryMB;
        public int ActiveCoins;
        public PerformanceTrend Trend;
    }

    [System.Serializable]
    public class DashboardPerformanceSnapshot
    {
        public DateTime Timestamp;
        public float FPS;
        public float MemoryMB;
        public int ActiveCoins;
        public PerformanceTrend Trend;
        public DashboardState State;
        public int ActiveAlerts;
    }

    [System.Serializable]
    public class DashboardPerformanceAlert
    {
        public AlertType Type;
        public string Message;
        public DateTime Timestamp;
        public Dictionary<string, object> Data;
    }

    [System.Serializable]
    public class DashboardStatistics
    {
        public int TotalUpdates;
        public float AverageFPS;
        public float MinFPS;
        public float MaxFPS;
        public float AverageMemoryMB;
        public float MinMemoryMB;
        public float MaxMemoryMB;
        public DateTime LastUpdateTime;
        public TimeSpan TotalUptime;
    }

    #endregion

    #region Enums

    public enum DashboardLayout
    {
        Compact,
        Expanded,
        TopBar,
        SidePanel
    }

    public enum DashboardTheme
    {
        Dark,
        Light,
        Custom
    }

    public enum DashboardState
    {
        Normal,
        Monitoring,
        Warning,
        Critical
    }

    public enum DashboardPerformanceTrend
    {
        Improving,
        Stable,
        Degrading
    }

    public enum AlertType
    {
        Info,
        Warning,
        Critical
    }

    #endregion
}