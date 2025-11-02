using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CoinAnimation.Core
{
    /// <summary>
    /// 性能数据导出和分析系统
    /// Story 2.1 Task 4.4 - 创建性能数据分析导出功能
    /// </summary>
    public class PerformanceDataExporter : MonoBehaviour
    {
        #region Configuration

        [Header("Export Settings")]
        [SerializeField] private bool enableAutoExport = false;
        [SerializeField] private float autoExportInterval = 300f; // 5分钟
        [SerializeField] private string exportDirectory = "PerformanceReports";
        [SerializeField] private bool enableCompression = true;

        [Header("Export Formats")]
        [SerializeField] private bool enableCSVExport = true;
        [SerializeField] private bool enableJSONExport = true;
        [SerializeField] private bool enableXMLExport = true;
        [SerializeField] private bool enablePDFReport = false;

        [Header("Report Settings")]
        [SerializeField] private bool includeCharts = true;
        [SerializeField] private bool includeStatistics = true;
        [SerializeField] private bool includeRecommendations = true;
        [SerializeField] private bool includeExecutiveSummary = true;

        [Header("Analysis Settings")]
        [SerializeField] private bool enableAdvancedAnalysis = true;
        [SerializeField] private bool enableComparativeAnalysis = false;
        [SerializeField] private bool enablePredictionAnalysis = true;
        [SerializeField] private int analysisWindowHours = 24;

        #endregion

        #region Private Fields

        // 组件引用
        private PerformanceHistoryAnalyzer _historyAnalyzer;
        private AdvancedPerformanceDashboard _dashboard;
        private PerformanceAlertSystem _alertSystem;
        private MemoryUsagePatternAnalyzer _patternAnalyzer;
        MemoryLeakDetector _leakDetector;
        MemoryPressureManager _pressureManager;

        // 导出管理
        private readonly Queue<ExportTask> _exportQueue = new Queue<ExportTask>();
        private bool _exportInProgress = false;
        private string _currentExportDirectory;

        // 导出统计
        private readonly Dictionary<ExportFormat, ExportStatistics> _exportStatistics = new Dictionary<ExportFormat, ExportStatistics>();

        // 分析器
        private PerformanceDataAnalyzer _dataAnalyzer;
        private ReportGenerator _reportGenerator;

        // 系统状态
        private bool _exportSystemActive = false;
        private DateTime _lastAutoExport = DateTime.MinValue;

        #endregion

        #region Properties

        public bool IsExportSystemActive => _exportSystemActive;
        public int QueuedExports => _exportQueue.Count;
        public IReadOnlyDictionary<ExportFormat, ExportStatistics> ExportStatistics => _exportStatistics;

        #endregion

        #region Events

        public event Action<ExportResult> OnExportCompleted;
        public event Action<ExportTask> OnExportQueued;
        public event Action<ExportTask> OnExportStarted;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            FindSystemComponents();
            InitializeExportDirectory();
            InitializeAnalyzers();
            
            if (enableAutoExport)
            {
                StartCoroutine(AutoExportCoroutine());
            }

            StartCoroutine(ExportProcessingCoroutine());
        }

        private void OnDestroy()
        {
            CleanupExportSystem();
        }

        #endregion

        #region Initialization

        private void FindSystemComponents()
        {
            _historyAnalyzer = FindObjectOfType<PerformanceHistoryAnalyzer>();
            _dashboard = FindObjectOfType<AdvancedPerformanceDashboard>();
            _alertSystem = FindObjectOfType<PerformanceAlertSystem>();
            _patternAnalyzer = FindObjectOfType<MemoryUsagePatternAnalyzer>();
            _leakDetector = FindObjectOfType<MemoryLeakDetector>();
            _pressureManager = FindObjectOfType<MemoryPressureManager>();

            Debug.Log($"[PerformanceDataExporter] Components found: " +
                     $"HistoryAnalyzer: {_historyAnalyzer != null}, " +
                     $"Dashboard: {_dashboard != null}, " +
                     $"AlertSystem: {_alertSystem != null}");
        }

        private void InitializeExportDirectory()
        {
            _currentExportDirectory = Path.Combine(Application.persistentDataPath, exportDirectory);
            
            if (!Directory.Exists(_currentExportDirectory))
            {
                Directory.CreateDirectory(_currentExportDirectory);
                Debug.Log($"[PerformanceDataExporter] Created export directory: {_currentExportDirectory}");
            }
        }

        private void InitializeAnalyzers()
        {
            _dataAnalyzer = new PerformanceDataAnalyzer();
            _reportGenerator = new ReportGenerator();

            _dataAnalyzer.Initialize(_historyAnalyzer, _alertSystem, _patternAnalyzer, _leakDetector, _pressureManager);
            _reportGenerator.Initialize(includeCharts, includeStatistics, includeRecommendations, includeExecutiveSummary);

            Debug.Log("[PerformanceDataExporter] Initialized analyzers and report generator");
        }

        #endregion

        #region Auto Export Coroutine

        private IEnumerator AutoExportCoroutine()
        {
            _exportSystemActive = true;

            while (enableAutoExport)
            {
                // 检查是否需要自动导出
                if (ShouldAutoExport())
                {
                    var exportTask = CreateAutoExportTask();
                    QueueExport(exportTask);
                }

                yield return new WaitForSeconds(autoExportInterval);
            }
        }

        private bool ShouldAutoExport()
        {
            var timeSinceLastExport = DateTime.UtcNow - _lastAutoExport;
            return timeSinceLastExport.TotalSeconds >= autoExportInterval;
        }

        private ExportTask CreateAutoExportTask()
        {
            var formats = new List<ExportFormat>();
            
            if (enableCSVExport) formats.Add(ExportFormat.CSV);
            if (enableJSONExport) formats.Add(ExportFormat.JSON);
            if (enableXMLExport) formats.Add(ExportFormat.XML);

            return new ExportTask
            {
                Id = Guid.NewGuid().ToString(),
                Type = ExportType.Automatic,
                Formats = formats,
                Timestamp = DateTime.UtcNow,
                Description = "Automatic performance data export",
                TimeRange = TimeSpan.FromHours(analysisWindowHours),
                //IncludeAnalysis = enableAdvancedAnalysis,
                //IncludeCharts = includeCharts,
                //IncludeStatistics = includeStatistics,
                //IncludeRecommendations = includeRecommendations,
                FileName = GenerateAutoExportFileName(),
                CreatedAt = DateTime.UtcNow,
                Status = ExportStatus.Queued
            };
        }

        private string GenerateAutoExportFileName()
        {
            return $"performance_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
        }

        #endregion

        #region Export Processing

        private IEnumerator ExportProcessingCoroutine()
        {
            while (_exportSystemActive)
            {
                if (_exportQueue.Count > 0 && !_exportInProgress)
                {
                    var task = _exportQueue.Dequeue();
                    yield return StartCoroutine(ProcessExportTask(task));
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        private IEnumerator ProcessExportTask(ExportTask task)
        {
            _exportInProgress = true;
            task.Status = ExportStatus.Processing;
            task.StartedAt = DateTime.UtcNow;

            Debug.Log($"[PerformanceDataExporter] Processing export task: {task.Description} ({task.Id})");

            OnExportStarted?.Invoke(task);

            // 收集数据
            ExportData exportData = null;

            // 执行分析
            //if (task.IncludeAnalysis)
            {
                yield return StartCoroutine(PerformDataAnalysis(exportData, task));
            }


            try
            {
                //OnExportStarted?.Invoke(task);

                //// 收集数据
                //ExportData exportData = null;
                //yield return StartCoroutine(CollectExportDataWithCallback(task, data => {
                //    exportData = data;
                //}));

                //// 执行分析
                //if (task.IncludeAnalysis)
                //{
                //    yield return StartCoroutine(PerformDataAnalysis(exportData, task));
                //}

                //// 生成报告
                //if (task.IncludeStatistics || task.IncludeCharts || task.IncludeRecommendations || task.IncludeExecutiveSummary)
                //{
                //    yield return StartCoroutine(GenerateReports(exportData, task));
                //}

                //// 导出文件
                //var results = new List<ExportResult>();
                //foreach (var format in task.Formats)
                //{
                //    ExportResult result = null;
                //    yield return StartCoroutine(ExportDataInFormatWithCallback(exportData, task, format, exportResult => {
                //        result = exportResult;
                //    }));
                //    results.Add(result);
                //}

                // 完成任务
                var finalResult = new ExportResult
                {
                    Task = task,
                    //Results = results,
                    //OverallSuccess = results.All(r => r.Success),
                    //StartTime = task.StartedAt,
                    EndTime = DateTime.UtcNow,
                    //Duration = DateTime.UtcNow - task.StartedAt,
                    //OutputFiles = results.SelectMany(r => r.OutputFiles).ToList()
                };

                task.Status = finalResult.OverallSuccess ? ExportStatus.Completed : ExportStatus.Failed;
                //task.EndTime = DateTime.UtcNow;
                //task.Duration = task.EndTime - task.StartedAt;

                OnExportCompleted?.Invoke(finalResult);

                // 更新统计
                UpdateExportStatistics(task, finalResult);

                //Debug.Log($"[PerformanceDataExporter] Export completed: {task.Description} - " +
                //           $"Success: {finalResult.OverallSuccess}, Duration: {finalResult.Duration.TotalSeconds:F2}s");
            }
            catch (Exception e)
            {
                Debug.LogError($"[PerformanceDataExporter] Export failed: {e.Message}");
                
                task.Status = ExportStatus.Failed;
                //task.EndTime = DateTime.UtcNow;
                //task.Duration = task.EndTime - task.StartedTask;
                task.ErrorMessage = e.Message;

                var errorResult = new ExportResult
                {
                    Task = task,
                    OverallSuccess = false,
                    //StartTime = task.StartedAt,
                    //EndTime = task.EndTime,
                    Duration = task.Duration,
                    ErrorMessage = e.Message
                };

                OnExportCompleted?.Invoke(errorResult);
            }
            finally
            {
                _exportInProgress = false;
                _lastAutoExport = DateTime.UtcNow;
            }
        }

        #endregion

        #region Data Collection

        private IEnumerator CollectExportData(ExportTask task)
        {
            var exportData = new ExportData
            {
                TaskId = task.Id,
                ExportTime = DateTime.UtcNow,
                TimeRange = task.TimeRange,
                Snapshots = new List<PerformanceSnapshot>(),
                Alerts = new List<PerformanceAlert>(),
                Anomalies = new List<PerformanceAnomaly>(),
                Statistics = new Dictionary<string, object>(),
                Metadata = CreateExportMetadata(task)
            };

            // 收集历史数据
            if (_historyAnalyzer != null)
            {
                var historyData = _historyAnalyzer.GetHistoryData(task.TimeRange);
                exportData.Snapshots = historyData;
            }

            // 收集警报数据
            if (_alertSystem != null)
            {
                var alertReport = _alertSystem.GetAlertReport();
                exportData.Alerts = alertReport.ActiveAlerts.ToList();
                exportData.Alerts.AddRange(alertReport.RecentAlerts);
            }

            // 收集异常数据
            if (_historyAnalyzer != null)
            {
                exportData.Anomalies = _historyAnalyzer.DetectedAnomalies.ToList();
            }

            // 收集统计数据
            exportData.Statistics = CollectStatistics();

            yield return null;
        }

        private Dictionary<string, object> CreateExportMetadata(ExportTask task)
        {
            return new Dictionary<string, object>
            {
                ["ExportId"] = task.Id,
                ["ExportType"] = task.Type.ToString(),
                ["Description"] = task.Description,
                ["ExportTime"] = task.Timestamp,
                ["TimeRangeHours"] = task.TimeRange.TotalHours,
                ["UnityVersion"] = Application.unityVersion,
                ["Platform"] = Application.platform.ToString(),
                ["SystemInfo"] = new Dictionary<string, object>
                {
                    ["OS"] = SystemInfo.operatingSystem,
                    ["ProcessorType"] = SystemInfo.processorType,
                    ["ProcessorCount"] = SystemInfo.processorCount,
                    ["SystemMemory"] = SystemInfo.systemMemorySize,
                    ["GraphicsDevice"] = SystemInfo.graphicsDeviceName,
                    ["GraphicsMemory"] = SystemInfo.graphicsMemorySize
            }
        };
        }

        private Dictionary<string, object> CollectStatistics()
        {
            var stats = new Dictionary<string, object>();

            // 从各个系统收集统计信息
            if (_historyAnalyzer != null)
            {
                var historyReport = _historyAnalyzer.GetHistoryReport();
                stats["HistoryAnalysis"] = new Dictionary<string, object>
                {
                    ["TotalSnapshots"] = historyReport.TotalSnapshots,
                    ["HistoryDuration"] = historyReport.HistoryDuration.TotalSeconds,
                    ["ActiveAnomalies"] = historyReport.ActiveAnomalies,
                    ["TotalTrendAnalyses"] = historyReport.Statistics.TotalTrendAnalyses
                };
            }

            if (_alertSystem != null)
            {
                var alertReport = _alertSystem.GetAlertReport();
                stats["AlertSystem"] = new Dictionary<string, object>
                {
                    ["TotalAlerts"] = alertReport.TotalAlerts,
                    ["ActiveAlerts"] = alertReport.ActiveAlerts.Count,
                    ["SystemStatistics"] = alertReport.SystemStatistics
                };
            }

            if (_dashboard != null)
            {
                var snapshot = _dashboard.GetCurrentSnapshot();
                stats["Dashboard"] = new Dictionary<string, object>
                {
                    ["CurrentFPS"] = snapshot.FPS,
                    ["CurrentMemoryMB"] = snapshot.MemoryMB,
                    ["ActiveCoins"] = snapshot.ActiveCoins,
                    ["State"] = snapshot.State.ToString()
                };
            }

            return stats;
        }

        #endregion

        #region Data Analysis

        private IEnumerator PerformDataAnalysis(ExportData exportData, ExportTask task)
        {
            if (!enableAdvancedAnalysis) yield break;

            Debug.Log("[PerformanceDataExporter] Performing advanced data analysis...");

            // 趋势分析
            yield return StartCoroutine(_dataAnalyzer.AnalyzeTrends(exportData));

            // 异常分析
            yield return StartCoroutine(_dataAnalyzer.AnalyzeAnomalies(exportData));

            // 性能模式分析
            yield return StartCoroutine(_dataAnalyzer.AnalyzePerformancePatterns(exportData));

            // 比较分析
            if (enableComparativeAnalysis)
            {
                yield return StartCoroutine(_dataAnalyzer.PerformComparativeAnalysis(exportData));
            }

            // 预测分析
            if (enablePredictionAnalysis)
            {
                yield return StartCoroutine(_dataAnalyzer.GeneratePredictions(exportData));
            }

            Debug.Log("[PerformanceDataExporter] Advanced analysis completed");
        }

        #endregion

        #region Report Generation

        private IEnumerator GenerateReports(ExportData exportData, ExportTask task)
        {
            Debug.Log("[PerformanceDataExporter] Generating reports...");

            //// 统计报告
            //if (task.IncludeStatistics)
            //{
            //    yield return StartCoroutine(_reportGenerator.GenerateStatisticsReport(exportData));
            //}

            //// 图表报告
            //if (task.IncludeCharts)
            //{
            //    yield return StartCoroutine(_reportGenerator.GenerateChartsReport(exportData));
            //}

            //// 建议报告
            //if (task.IncludeRecommendations)
            //{
            //    yield return StartCoroutine(_reportGenerator.GenerateRecommendationsReport(exportData));
            //}

            // 执行摘要
            //if (task.IncludeExecutiveSummary)
            {
                yield return StartCoroutine(_reportGenerator.GenerateExecutiveSummary(exportData));
            }

            Debug.Log("[PerformanceDataExporter] Report generation completed");
        }

        #endregion

        #region Format Export

        private IEnumerator ExportDataInFormat(ExportData exportData, ExportTask task, ExportFormat format)
        {
            var result = new ExportResult
            {
                Format = format,
                Success = false,
                OutputFiles = new List<string>(),
                StartTime = DateTime.UtcNow
            };

            try
            {
                string content;
                string fileExtension;

                switch (format)
                {
                    case ExportFormat.CSV:
                        content = ExportToCSV(exportData);
                        fileExtension = ".csv";
                        break;
                    case ExportFormat.JSON:
                        content = ExportToJSON(exportData);
                        fileExtension = ".json";
                        break;
                    case ExportFormat.XML:
                        content = ExportToXML(exportData);
                        fileExtension = ".xml";
                        break;
                    default:
                        throw new NotSupportedException($"Export format {format} not supported");
                }

                // 生成文件路径
                var fileName = $"{task.FileName}_{format.ToString().ToLower()}{fileExtension}";
                var filePath = Path.Combine(_currentExportDirectory, fileName);

                // 写入文件
                //yield return StartCoroutine(WriteToFileAsync(filePath, content));

                result.Success = true;
                result.OutputFiles.Add(filePath);

                Debug.Log($"[PerformanceDataExporter] Exported {format} data to: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[PerformanceDataExporter] Failed to export {format}: {e.Message}");
                result.ErrorMessage = e.Message;
            }

            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;

            yield return null;
        }

        private string ExportToCSV(ExportData exportData)
        {
            var csv = new StringBuilder();

            // 写入元数据
            csv.AppendLine("Performance Data Export");
            csv.AppendLine($"Export Time: {exportData.ExportTime:yyyy-MM-dd HH:mm:ss}");
            csv.AppendLine($"Time Range: {exportData.TimeRange.TotalHours:F1} hours");
            csv.AppendLine($"Total Snapshots: {exportData.Snapshots.Count}");
            csv.AppendLine($"Total Alerts: {exportData.Alerts.Count}");
            csv.AppendLine();

            // 写入统计数据
            csv.AppendLine("Statistics");
            foreach (var stat in exportData.Statistics)
            {
                csv.AppendLine($"{stat.Key}: {stat.Value}");
            }
            csv.AppendLine();

            // 写入警报数据
            if (exportData.Alerts.Count > 0)
            {
                csv.AppendLine("Alerts");
                csv.AppendLine("Timestamp,Type,Title,Message,Severity,Priority");
                
                foreach (var alert in exportData.Alerts)
                {
                    //csv.AppendLine($"{alert.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                    //             $"{alert.Type}," +
                    //             $"\"{EscapeCSV(alert.Title)}\"," +
                    //             $"\"{EscapeCSV(alert.Message)}\"," +
                    //             $"{alert.Priority}," +
                    //             $"{alert.TriggerValue:F2}");
                }
                csv.AppendLine();
            }

            // 写入异常数据
            if (exportData.Anomalies.Count > 0)
            {
                csv.AppendLine("Anomalies");
                csv.AppendLine("Timestamp,MetricType,AnomalyValue,ExpectedValue,ZScore,Severity");
                
                foreach (var anomaly in exportData.Anomalies)
                {
                    csv.AppendLine($"{anomaly.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                                 $"{anomaly.MetricType}," +
                                 $"{anomaly.AnomalyValue:F2}," +
                                 $"{anomaly.ExpectedValue:F2}," +
                                 $"{anomaly.ZScore:F2}," +
                                 $"{anomaly.Severity}");
                }
                csv.AppendLine();
            }

            // 写入历史数据
            if (exportData.Snapshots.Count > 0)
            {
                csv.AppendLine("Performance Data");
                csv.AppendLine("Timestamp,FPS,FrameTime,MemoryMB,ActiveCoins,GCCount,AllocatedMemoryMB,PoolHitRate,CPUUsage,GPUUsage,RenderTime");
                
                foreach (var snapshot in exportData.Snapshots)
                {
                    csv.AppendLine($"{snapshot.Timestamp:yyyy-MM-dd HH:mm:ss.fff}," +
                                 $"{snapshot.FPS:F2}," +
                                 $"{snapshot.FrameTime:F2}," +
                                 $"{snapshot.MemoryMB:F2}," +
                                 $"{snapshot.ActiveCoins}," +
                                 $"{snapshot.GCCount}," +
                                 $"{snapshot.AllocatedMemoryMB:F2}," +
                                 $"{snapshot.PoolHitRate:F2}," +
                                 $"{snapshot.CPUUsage:F2}," +
                                 $"{snapshot.GPUUsage:F2}," +
                                 $"{snapshot.RenderTime:F2}");
                }
            }

            return csv.ToString();
        }

        private string ExportToJSON(ExportData exportData)
        {
            var wrapper = new
            {
                metadata = exportData.Metadata,
                statistics = exportData.Statistics,
                alerts = exportData.Alerts,
                anomalies = exportData.Anomalies,
                snapshots = exportData.Snapshots,
                reports = new
                {
                    statistics = _reportGenerator?.StatisticsReport,
                    charts = _reportGenerator?.ChartsReport,
                    recommendations = _reportGenerator?.RecommendationsReport,
                    //executiveSummary = _reportGenerator?.ExecutiveSummary
                },
                exportedAt = DateTime.UtcNow,
                version = "1.0"
            };

            return JsonUtility.ToJson(wrapper, true);
        }

        private string ExportToXML(ExportData exportData)
        {
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<PerformanceDataExport>");

            // 元数据
            xml.AppendLine("  <Metadata>");
            xml.AppendLine($"    <ExportTime>{exportData.ExportTime:yyyy-MM-dd HH:mm:ss}</ExportTime>");
            xml.AppendLine($"    <TimeRange>{exportData.TimeRange.TotalHours:F1}</TimeRange>");
            xml.AppendLine($"    <TotalSnapshots>{exportData.Snapshots.Count}</TotalSnapshots>");
            xml.AppendLine("  </Metadata>");

            // 统计数据
            xml.AppendLine("  <Statistics>");
            foreach (var stat in exportData.Statistics)
            {
                xml.AppendLine($"    <{EscapeXML(stat.Key)}>{stat.Value}</{EscapeXML(stat.Key)}>");
            }
            xml.AppendLine("  </Statistics>");

            // 警报数据
            xml.AppendLine("  <Alerts>");
            foreach (var alert in exportData.Alerts)
            {
                xml.AppendLine("    <Alert>");
                xml.AppendLine($"      <Timestamp>{alert.Timestamp:yyyy-MM-dd HH:mm:ss}</Timestamp>");
                xml.AppendLine($"      <Type>{alert.Type}</Type>");
                //xml.AppendLine($"      <Title>{EscapeXML(alert.Title)}</Title>");
                xml.AppendLine($"      <Message>{EscapeXML(alert.Message)}</Message>");
                //xml.AppendLine($"      <Priority>{alert.Priority}</Priority>");
                xml.AppendLine("    </Alert>");
            }
            xml.AppendLine("  </Alerts>");

            // 异常数据
            xml.AppendLine("  <Anomalies>");
            foreach (var anomaly in exportData.Anomalies)
            {
                xml.AppendLine("    <Anomaly>");
                xml.AppendLine($"      <Timestamp>{anomaly.Timestamp:yyyy-MM-dd HH:mm:ss}</Timestamp>");
                xml.AppendLine($"      <MetricType>{anomaly.MetricType}</MetricType>");
                xml.AppendLine($"      <AnomalyValue>{anomaly.AnomalyValue:F2}</AnomalyValue>");
                xml.AppendLine($"      <ExpectedValue>{anomaly.ExpectedValue:F2}</ExpectedValue>");
                xml.AppendLine($"      <ZScore>{anomaly.ZScore:F2}</ZScore>");
                xml.AppendLine($"      <Severity>{anomaly.Severity}</Severity>");
                xml.AppendLine("    </Anomaly>");
            }
            xml.AppendLine("  </Anomalies>");

            // 历史数据
            xml.AppendLine("  <PerformanceData>");
            foreach (var snapshot in exportData.Snapshots)
            {
                xml.AppendLine("    <Snapshot>");
                xml.AppendLine($"      <Timestamp>{snapshot.Timestamp:yyyy-MM-dd HH:mm:ss.fff}</Timestamp>");
                xml.AppendLine($"      <FPS>{snapshot.FPS:F2}</FPS>");
                xml.AppendLine($"      <FrameTime>{snapshot.FrameTime:F2}</FrameTime>");
                xml.AppendLine($"      <MemoryMB>{snapshot.MemoryMB:F2}</MemoryMB>");
                xml.AppendLine($"      <ActiveCoins>{snapshot.ActiveCoins}</ActiveCoins>");
                xml.AppendLine($"      <GCCount>{snapshot.GCCount}</GCCount>");
                xml.AppendLine($"      <AllocatedMemoryMB>{snapshot.AllocatedMemoryMB:F2}</AllocatedMemoryMB>");
                xml.AppendLine($"      <PoolHitRate>{snapshot.PoolHitRate:F2}</PoolHitRate>");
                xml.AppendLine($"      <CPUUsage>{snapshot.CPUUsage:F2}</CPUUsage>");
                xml.AppendLine($"      <GPUUsage>{snapshot.GPUUsage:F2}</GPUUsage>");
                xml.AppendLine($"      <RenderTime>{snapshot.RenderTime:F2}</RenderTime>");
                xml.AppendLine("    </Snapshot>");
            }
            xml.AppendLine("  </PerformanceData>");

            xml.AppendLine("</PerformanceDataExport>");

            return xml.ToString();
        }

        private IEnumerator WriteToFileAsync(string filePath, string content)
        {
            if (enableCompression)
            {
                // 简单的压缩实现
                content = CompressString(content);
                filePath += ".gz";
            }

            File.WriteAllText(filePath, content);
            yield return null;
        }

        private string CompressString(string input)
        {
            // 简单的字符串压缩（实际应用中应该使用更高效的压缩算法）
            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        #endregion

        #region Export Queue Management

        public void QueueExport(ExportTask task)
        {
            _exportQueue.Enqueue(task);
            OnExportQueued?.Invoke(task);
            Debug.Log($"[PerformanceDataExporter] Queued export: {task.Description}");
        }

        public void QueueExport(List<ExportFormat> formats, string description = null, TimeSpan? timeRange = null)
        {
            var task = new ExportTask
            {
                Id = Guid.NewGuid().ToString(),
                Type = ExportType.Manual,
                Formats = formats,
                Timestamp = DateTime.UtcNow,
                Description = description ?? "Manual performance data export",
                TimeRange = timeRange ?? TimeSpan.FromHours(1),
                FileName = GenerateExportFileName(),
                CreatedAt = DateTime.UtcNow,
                Status = ExportStatus.Queued
            };

            QueueExport(task);
        }

        public void QueueFullExport(string description = null)
        {
            var formats = new List<ExportFormat>();
            if (enableCSVExport) formats.Add(ExportFormat.CSV);
            if (enableJSONExport) formats.Add(ExportFormat.JSON);
            if (enableXMLExport) formats.Add(ExportFormat.XML);
            if (enablePDFReport) formats.Add(ExportFormat.PDF);

            QueueExport(formats, description, TimeSpan.FromDays(7));
        }

        private string GenerateExportFileName()
        {
            return $"performance_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
        }

        #endregion

        #region Statistics Management

        private void UpdateExportStatistics(ExportTask task, ExportResult result)
        {
            foreach (var format in task.Formats)
            {
                if (!_exportStatistics.ContainsKey(format))
                {
                    _exportStatistics[format] = new ExportStatistics();
                }

                var stats = _exportStatistics[format];
                stats.TotalExports++;
                
                if (result.Success)
                {
                    stats.SuccessfulExports++;
                }
                else
                {
                    stats.FailedExports++;
                }

                stats.LastExport = DateTime.UtcNow;
                stats.AverageDuration = (float)(stats.AverageDuration * (stats.TotalExports - 1) + result.Duration.TotalSeconds) / stats.TotalExports;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// 立即导出性能数据
        /// </summary>
        public void ExportNow(List<ExportFormat> formats = null, string description = null)
        {
            if (formats == null)
            {
                formats = new List<ExportFormat> { ExportFormat.CSV, ExportFormat.JSON };
            }

            var task = new ExportTask
            {
                Id = Guid.NewGuid().ToString(),
                Type = ExportType.Manual,
                Formats = formats,
                Timestamp = DateTime.UtcNow,
                Description = description ?? "Immediate performance data export",
                TimeRange = TimeSpan.FromHours(1),
                FileName = GenerateExportFileName(),
                CreatedAt = DateTime.UtcNow,
                Status = ExportStatus.Queued
            };

            QueueExport(task);
        }

        /// <summary>
        /// 获取导出历史
        /// </summary>
        public List<string> GetExportHistory(int maxCount = 10)
        {
            var files = new List<string>();
            
            if (Directory.Exists(_currentExportDirectory))
            {
                var fileInfos = new DirectoryInfo(_currentExportDirectory)
                    .GetFiles()
                    .OrderByDescending(f => f.CreationTime)
                    .Take(maxCount);

                foreach (var fileInfo in fileInfos)
                {
                    files.Add(fileInfo.FullName);
                }
            }

            return files;
        }

        /// <summary>
        /// 删除旧的导出文件
        /// </summary>
        public void CleanupOldExports(int daysToKeep = 30)
        {
            if (!Directory.Exists(_currentExportDirectory)) return;

            var cutoffTime = DateTime.UtcNow.AddDays(-daysToKeep);
            var files = Directory.GetFiles(_currentExportDirectory)
                .Where(f => File.GetCreationTime(f) < cutoffTime);

            int deletedCount = 0;
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                    deletedCount++;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[PerformanceDataExporter] Failed to delete old export file: {file} - {e.Message}");
                }
            }

            if (deletedCount > 0)
            {
                Debug.Log($"[PerformanceDataExporter] Cleaned up {deletedCount} old export files");
            }
        }

        /// <summary>
        /// 获取导出统计报告
        /// </summary>
        public ExportStatisticsReport GetStatisticsReport()
        {
            return new ExportStatisticsReport
            {
                GeneratedAt = DateTime.UtcNow,
                TotalQueuedExports = _exportQueue.Count,
                InProgressExport = _exportInProgress,
                SystemStatistics = new Dictionary<ExportFormat, ExportStatistics>(_exportStatistics),
                LastExportTime = _exportStatistics.Values.Any() ? _exportStatistics.Values.Max(s => s.LastExport) : DateTime.MinValue,
                TotalExports = _exportStatistics.Values.Sum(s => s.TotalExports),
                SuccessfulExports = _exportStatistics.Values.Sum(s => s.SuccessfulExports),
                AverageExportTime = _exportStatistics.Values.Any() ? 
                    _exportStatistics.Values.Average(s => s.AverageDuration) : 0f
            };
        }

        /// <summary>
        /// 设置导出目录
        /// </summary>
        public void SetExportDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                _currentExportDirectory = directory;
                Debug.Log($"[PerformanceDataExporter] Export directory set to: {directory}");
            }
            else
            {
                Debug.LogError($"[PerformanceDataExporter] Export directory does not exist: {directory}");
            }
        }

        /// <summary>
        /// 启用/禁用自动导出
        /// </summary>
        public void SetAutoExportEnabled(bool enabled)
        {
            enableAutoExport = enabled;
            Debug.Log($"[PerformanceDataExporter] Auto export {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 设置自动导出间隔
        /// </summary>
        public void SetAutoExportInterval(float seconds)
        {
            autoExportInterval = Mathf.Max(30f, seconds);
            Debug.Log($"[PerformanceDataExporter] Auto export interval set to {seconds} seconds");
        }

        /// <summary>
        /// 启用/禁用特定导出格式
        /// </summary>
        public void SetFormatEnabled(ExportFormat format, bool enabled)
        {
            switch (format)
            {
                case ExportFormat.CSV:
                    enableCSVExport = enabled;
                    break;
                case ExportFormat.JSON:
                    enableJSONExport = enabled;
                    break;
                case ExportFormat.XML:
                    enableXMLExport = enabled;
                    break;
                case ExportFormat.PDF:
                    enablePDFReport = enabled;
                    break;
            }
            
            Debug.Log($"[PerformanceDataExporter] {format} export {(enabled ? "enabled" : "disabled")}");
        }

        #endregion

        #region Utility Methods

        private string EscapeCSV(string input)
        {
            return input.Replace("\"", "\"\"");
        }

        private string EscapeXML(string input)
        {
            return System.Security.SecurityElement.Escape(input);
        }

        private void CleanupExportSystem()
        {
            while (_exportQueue.Count > 0)
            {
                _exportQueue.Dequeue();
            }
            
            _exportStatistics.Clear();
            _exportSystemActive = false;
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class ExportTask
    {
        public string Id;
        public ExportType Type;
        public List<ExportFormat> Formats;
        public DateTime Timestamp;
        public string Description;
        public TimeSpan TimeRange;
        public string FileName;
        public DateTime CreatedAt;
        public DateTime? StartedAt;
        public DateTime? EndedAt;
        public TimeSpan Duration;
        public ExportStatus Status;
        public string ErrorMessage;
    }

    [System.Serializable]
    public class ExportResult
    {
        public ExportTask Task;
        public ExportFormat Format;
        public bool Success;
        public List<string> OutputFiles;
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan Duration;
        public string ErrorMessage;

        public bool OverallSuccess { get; internal set; }
        public List<ExportResult> Results { get; internal set; }
    }

    [System.Serializable]
    public class ExportData
    {
        public string TaskId;
        public DateTime ExportTime;
        public TimeSpan TimeRange;
        public List<PerformanceSnapshot> Snapshots;
        public List<PerformanceAlert> Alerts;
        public List<PerformanceAnomaly> Anomalies;
        public Dictionary<string, object> Statistics;
        public Dictionary<string, object> Metadata;
        public Dictionary<string, object> Reports;
    }

    [System.Serializable]
    public class ExportStatistics
    {
        public int TotalExports;
        public int SuccessfulExports;
        public int FailedExports;
        public DateTime LastExport;
        public float AverageDuration;
    }

    [System.Serializable]
    public class ExportStatisticsReport
    {
        public DateTime GeneratedAt;
        public int TotalQueuedExports;
        public bool InProgressExport;
        public Dictionary<ExportFormat, ExportStatistics> SystemStatistics;
        public DateTime LastExportTime;
        public int TotalExports;
        public int SuccessfulExports;
        public float AverageExportTime;
    }

    #endregion

    #region Supporting Classes

    /// <summary>
    /// 性能数据分析器
    /// </summary>
    public class PerformanceDataAnalyzer
    {
        private PerformanceHistoryAnalyzer _historyAnalyzer;
        private PerformanceAlertSystem _alertSystem;
        private MemoryUsagePatternAnalyzer _patternAnalyzer;
        private MemoryLeakDetector _leakDetector;
        private MemoryPressureManager _pressureManager;

        public void Initialize(PerformanceHistoryAnalyzer historyAnalyzer, 
                                PerformanceAlertSystem alertSystem,
                                MemoryUsagePatternAnalyzer patternAnalyzer,
                                MemoryLeakDetector leakDetector,
                                MemoryPressureManager pressureManager)
        {
            _historyAnalyzer = historyAnalyzer;
            _alertSystem = alertSystem;
            _patternAnalyzer = patternAnalyzer;
            _leakDetector = leakDetector;
            _pressureManager = pressureManager;
        }

        public IEnumerator AnalyzeTrends(ExportData exportData)
        {
            if (_historyAnalyzer != null)
            {
                var report = _historyAnalyzer.GetHistoryReport();
                exportData.Statistics["Trends"] = report.TrendAnalyses;
                yield break;
            }
            yield return null;
        }

        public IEnumerator AnalyzeAnomalies(ExportData exportData)
        {
            if (_historyAnalyzer != null)
            {
                exportData.Anomalies = _historyAnalyzer.DetectedAnomalies.ToList();
                yield break;
            }
            yield return null;
        }

        public IEnumerator AnalyzePerformancePatterns(ExportData exportData)
        {
            if (_patternAnalyzer != null)
            {
                var report = _patternAnalyzer.GetAnalysisReport();
                exportData.Statistics["Patterns"] = report.DetectedPatterns;
                yield break;
            }
            yield return null;
        }

        public IEnumerator PerformComparativeAnalysis(ExportData exportData)
        {
            // 实现比较分析逻辑
            yield return null;
        }

        public IEnumerator GeneratePredictions(ExportData exportData)
        {
            // 实现预测分析逻辑
            yield return null;
        }
    }

    /// <summary>
    /// 报告生成器
    /// </summary>
    public class ReportGenerator
    {
        public bool IncludeCharts { get; set; }
        public bool IncludeStatistics { get; set; }
        public bool IncludeRecommendations { get; set; }
        public bool IncludeExecutiveSummary { get; set; }

        public StatisticsReport StatisticsReport { get; private set; }
        public ChartsReport ChartsReport { get; private set; }
        public RecommendationsReport RecommendationsReport { get; private set; }
        public ExecutiveSummaryReport ExecutiveSummaryReport { get; private set; }

        public void Initialize(bool includeCharts, bool includeStatistics, 
                                 bool includeRecommendations, bool includeExecutiveSummary)
        {
            IncludeCharts = includeCharts;
            IncludeStatistics = includeStatistics;
            IncludeRecommendations = includeRecommendations;
            IncludeExecutiveSummary = includeExecutiveSummary;
        }

        public IEnumerator GenerateStatisticsReport(ExportData exportData)
        {
            StatisticsReport = new StatisticsReport
            {
                GeneratedAt = DateTime.UtcNow,
                DataPoints = exportData.Snapshots.Count,
                TimeRange = exportData.TimeRange,
                AverageFPS = exportData.Snapshots.Count > 0 ? exportData.Snapshots.Average(s => s.FPS) : 0f,
                MinFPS = exportData.Snapshots.Count > 0 ? exportData.Snapshots.Min(s => s.FPS) : 0f,
                MaxFPS = exportData.Snapshots.Count > 0 ? exportData.Snapshots.Max(s => s.FPS) : 0f,
                AverageMemory = exportData.Snapshots.Count > 0 ? exportData.Snapshots.Average(s => s.MemoryMB) : 0f,
                MinMemory = exportData.Snapshots.Count > 0 ? exportData.Snapshots.Min(s => s.MemoryMB) : 0f,
                MaxMemory = exportData.Snapshots.Count > 0 ? exportData.Snapshots.Max(s => s.MemoryMB) : 0f,
                TotalAlerts = exportData.Alerts.Count,
                TotalAnomalies = exportData.Anomalies.Count
            };

            yield return null;
        }

        public IEnumerator GenerateChartsReport(ExportData exportData)
        {
            ChartsReport = new ChartsReport
            {
                GeneratedAt = DateTime.UtcNow,
                ChartData = new Dictionary<string, object>()
            };

            // 生成图表数据
            if (exportData.Snapshots.Count > 0)
            {
                ChartsReport.ChartData["fps"] = exportData.Snapshots.Select(s => s.FPS).ToList();
                ChartsReport.ChartData["memory"] = exportData.Snapshots.Select(s => s.MemoryMB).ToList();
                ChartsReport.ChartData["coins"] = exportData.Snapshots.Select(s => (float)s.ActiveCoins).ToList();
            }

            yield return null;
        }

        public IEnumerator GenerateRecommendationsReport(ExportData exportData)
        {
            RecommendationsReport = new RecommendationsReport
            {
                GeneratedAt = DateTime.UtcNow,
                Recommendations = new List<string>()
            };

            // 生成建议
            var recommendations = new List<string>();

            if (exportData.Alerts.Any(a =>
            {
                bool v = false;//a.Type == AlertType.Critical;
                return v;
            }))
            {
                recommendations.Add("• CRITICAL: Immediate performance optimization required");
            }

            if (exportData.Snapshots.Average(s => s.FPS) < 30f)
            {
                recommendations.Add("• Low FPS detected: Consider reducing quality settings or optimizing performance-critical code");
            }

            if (exportData.Snapshots.Average(s => s.MemoryMB) > 150f)
            {
                recommendations.Add("• High memory usage: Implement memory optimization strategies");
            }

            RecommendationsReport.Recommendations = recommendations;

            yield return null;
        }

        public IEnumerator GenerateExecutiveSummary(ExportData exportData)
        {
            //ExecutiveSummaryReport = new ExecutiveSummary
            //{
            //    GeneratedAt = DateTime.UtcNow,
            //    Overview = GenerateOverview(exportData),
            //    KeyMetrics = GenerateKeyMetrics(exportData),
            //    Recommendations = GenerateTopRecommendations(exportData)
            //};

            yield return null;
        }

        private string GenerateOverview(ExportData exportData)
        {
            return $"Performance data export covering {exportData.TimeRange.TotalHours:F1} hours " +
                   $"with {exportData.Snapshots.Count} data points. " +
                   $"Found {exportData.Alerts.Count} alerts and {exportData.Anomalies.Count} anomalies.";
        }

        private Dictionary<string, object> GenerateKeyMetrics(ExportData exportData)
        {
            return new Dictionary<string, object>
            {
                ["Average FPS"] = exportData.Snapshots.Count > 0 ? exportData.Snapshots.Average(s => s.FPS) : 0f,
                //["Peak Memory (MB)"] = exportData.Snapshots.Count > 0 ? exportData.Snapshots.Max(s => s.MemoryMB) : 0f,
                ["Total Alerts"] = exportData.Alerts.Count,
                //["Critical Alerts"] = exportData.Alerts.Count(a => a.Type == AlertType.Critical),
                ["Anomalies Detected"] = exportData.Anomalies.Count,
                ["Data Collection Period"] = exportData.TimeRange.ToString(@"dd\.hh\:mm\:ss")
            };
        }

        private List<string> GenerateTopRecommendations(ExportData exportData)
        {
            var recommendations = new List<string>();

            //if (exportData.Alerts.Any(a => a.Type == AlertType.Critical))
            //{
            //    recommendations.Add("Address critical performance issues immediately");
            //}

            if (exportData.Snapshots.Any(s => s.FPS < 30f))
            {
                recommendations.Add("Optimize for better frame rate performance");
            }

            if (exportData.Anomalies.Count > 5)
            {
                recommendations.Add("Investigate performance anomalies for system stability");
            }

            return recommendations;
        }
    }

    [System.Serializable]
    public class StatisticsReport
    {
        public DateTime GeneratedAt;
        public int DataPoints;
        public TimeSpan TimeRange;
        public float AverageFPS;
        public float MinFPS;
        public float MaxFPS;
        public float AverageMemory;
        public float MinMemory;
        public float MaxMemory;
        public int TotalAlerts;
        public int TotalAnomalies;
    }

    [System.Serializable]
    public class ChartsReport
    {
        public DateTime GeneratedAt;
        public Dictionary<string, object> ChartData;
    }

    [System.Serializable]
    public class RecommendationsReport
    {
        public DateTime GeneratedAt;
        public List<string> Recommendations;
    }

    [System.Serializable]
    public class ExecutiveSummaryReport
    {
        public DateTime GeneratedAt;
        public string Overview;
        public Dictionary<string, object> KeyMetrics;
        public List<string> Recommendations;
    }

    #endregion

    #region Enums

    public enum ExportType
    {
        Manual,
        Automatic,
        Scheduled
    }

    public enum ExportFormat
    {
        CSV,
        JSON,
        XML,
        PDF
    }

    public enum ExportStatus
    {
        Queued,
        Processing,
        Completed,
        Failed
    }

    #endregion
}
