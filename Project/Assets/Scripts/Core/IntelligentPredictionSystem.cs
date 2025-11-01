using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoinAnimation.Core
{
    /// <summary>
    /// 智能预测系统 - Story 1.3 Enhancement
    /// 使用机器学习算法预测对象池需求和内存使用模式
    /// </summary>
    public class IntelligentPredictionSystem : MonoBehaviour
    {
        #region Configuration

        [Header("Prediction Settings")]
        [SerializeField] private bool enablePrediction = true;
        [SerializeField] private int historySize = 300; // 5分钟历史数据 @ 1秒间隔
        [SerializeField] private float predictionInterval = 5f; // 每5秒更新一次预测
        [SerializeField] private int predictionWindow = 30; // 预测未来30秒

        [Header("Learning Parameters")]
        [SerializeField] private float learningRate = 0.1f;
        [SerializeField] private float predictionConfidenceThreshold = 0.7f;
        [SerializeField] private int minDataPointsForPrediction = 30;

        [Header("Adaptive Thresholds")]
        [SerializeField] private float expansionThresholdMultiplier = 1.2f;
        [SerializeField] private float contractionThresholdMultiplier = 0.6f;
        [SerializeField] private float memoryGrowthThreshold = 0.1f; // 10%增长率阈值

        #endregion

        #region Private Fields

        private CoinObjectPool _objectPool;
        private MemoryManagementSystem _memorySystem;

        // 历史数据
        private readonly Queue<PredictionDataPoint> _dataHistory = new Queue<PredictionDataPoint>();
        private readonly List<float> _coinDemandHistory = new List<float>();
        private readonly List<float> _memoryUsageHistory = new List<float>();

        // 预测模型
        private PredictionModel _coinDemandModel;
        private PredictionModel _memoryUsageModel;
        private PatternDetector _patternDetector;

        // 预测结果
        private PredictionResult _lastPrediction;
        private float _lastPredictionTime = 0f;

        // 性能统计
        private PredictionStats _stats = new PredictionStats();

        #endregion

        #region Properties

        public PredictionResult LastPrediction => _lastPrediction;
        public PredictionStats Stats => _stats;
        public bool IsEnabled => enablePrediction;

        #endregion

        #region Events

        public event Action<PredictionResult> OnPredictionUpdated;
        public event Action<AdaptiveRecommendation> OnRecommendationGenerated;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializePredictionModels();
            FindSystemComponents();

            if (enablePrediction)
            {
                StartCoroutine(PredictionCoroutine());
            }
        }

        #endregion

        #region Initialization

        private void InitializePredictionModels()
        {
            _coinDemandModel = new LinearRegressionModel(learningRate);
            _memoryUsageModel = new LinearRegressionModel(learningRate);
            _patternDetector = new PatternDetector(historySize);
        }

        private void FindSystemComponents()
        {
            _objectPool = FindObjectOfType<CoinObjectPool>();
            _memorySystem = FindObjectOfType<MemoryManagementSystem>();

            if (_objectPool == null)
                Debug.LogWarning("[IntelligentPredictionSystem] CoinObjectPool not found");
            if (_memorySystem == null)
                Debug.LogWarning("[IntelligentPredictionSystem] MemoryManagementSystem not found");
        }

        #endregion

        #region Prediction Coroutine

        private System.Collections.IEnumerator PredictionCoroutine()
        {
            while (enablePrediction)
            {
                CollectCurrentData();

                if (_dataHistory.Count >= minDataPointsForPrediction)
                {
                    UpdatePredictions();
                    GenerateRecommendations();
                }

                yield return new WaitForSeconds(predictionInterval);
            }
        }

        #endregion

        #region Data Collection

        private void CollectCurrentData()
        {
            var dataPoint = new PredictionDataPoint
            {
                Timestamp = DateTime.UtcNow,
                ActiveCoins = _objectPool?.ActiveCoinCount ?? 0,
                AvailableCoins = _objectPool?.AvailableCoinCount ?? 0,
                PoolSize = _objectPool?.CurrentPoolSize ?? 0,
                MemoryUsageMB = _memorySystem?.CurrentMemoryUsageMB ?? GC.GetTotalMemory(false) / (1024f * 1024f),
                RequestRate = CalculateRequestRate(),
                ReturnRate = CalculateReturnRate()
            };

            // 添加到历史数据
            _dataHistory.Enqueue(dataPoint);
            while (_dataHistory.Count > historySize)
                _dataHistory.Dequeue();

            // 更新数值历史
            _coinDemandHistory.Add(dataPoint.ActiveCoins);
            _memoryUsageHistory.Add(dataPoint.MemoryUsageMB);

            // 保持历史数据大小
            while (_coinDemandHistory.Count > historySize)
                _coinDemandHistory.RemoveAt(0);
            while (_memoryUsageHistory.Count > historySize)
                _memoryUsageHistory.RemoveAt(0);
        }

        private float CalculateRequestRate()
        {
            // 计算过去一秒内的请求率
            float now = Time.time;
            int requestCount = 0;

            lock (_dataHistory)
            {
                foreach (var point in _dataHistory.Reverse())
                {
                    if ((float)(now - point.Timestamp.TimeOfDay.TotalSeconds) <= 1f)
                    {
                        requestCount++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return requestCount;
        }

        private float CalculateReturnRate()
        {
            // 计算过去一秒内的返回率
            float now = Time.time;
            int returnCount = 0;

            lock (_dataHistory)
            {
                foreach (var point in _dataHistory.Reverse())
                {
                    if ((float)(now - point.Timestamp.TimeOfDay.TotalSeconds) <= 1f)
                    {
                        returnCount++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return returnCount;
        }

        #endregion

        #region Prediction Models

        private void UpdatePredictions()
        {
            if (_coinDemandHistory.Count < minDataPointsForPrediction) return;

            // 预测金币需求
            var coinPrediction = PredictCoinDemand();

            // 预测内存使用
            var memoryPrediction = PredictMemoryUsage();

            // 检测模式
            var patterns = _patternDetector.DetectPatterns(_dataHistory.ToList());

            _lastPrediction = new PredictionResult
            {
                Timestamp = DateTime.UtcNow,
                PredictedCoinDemand = coinPrediction,
                PredictedMemoryUsage = memoryPrediction,
                DetectedPatterns = patterns,
                Confidence = CalculatePredictionConfidence(coinPrediction, memoryPrediction),
                PredictionWindow = predictionWindow
            };

            UpdatePredictionStats();
            OnPredictionUpdated?.Invoke(_lastPrediction);
        }

        private CoinDemandPrediction PredictCoinDemand()
        {
            try
            {
                // 使用线性回归预测未来需求
                var features = ExtractCoinDemandFeatures();
                var predictedMaxDemand = _coinDemandModel.Predict(features);

                // 计算需求趋势
                float demandTrend = CalculateDemandTrend();

                // 预测峰值时间
                float peakTime = PredictPeakDemandTime();

                return new CoinDemandPrediction
                {
                    PredictedMaxDemand = Mathf.RoundToInt(predictedMaxDemand),
                    DemandTrend = demandTrend,
                    PeakDemandTime = peakTime,
                    RecommendedPoolSize = CalculateRecommendedPoolSize(predictedMaxDemand),
                    Confidence = _coinDemandModel.GetConfidence()
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IntelligentPredictionSystem] Coin demand prediction failed: {ex.Message}");
                return GetDefaultCoinDemandPrediction();
            }
        }

        private MemoryUsagePrediction PredictMemoryUsage()
        {
            try
            {
                var features = ExtractMemoryUsageFeatures();
                var predictedMemory = _memoryUsageModel.Predict(features);

                // 计算内存增长趋势
                float memoryTrend = CalculateMemoryTrend();

                // 预测内存压力时间
                float pressureTime = PredictMemoryPressureTime();

                return new MemoryUsagePrediction
                {
                    PredictedMaxMemoryMB = predictedMemory,
                    MemoryTrend = memoryTrend,
                    MemoryPressureTime = pressureTime,
                    RiskLevel = AssessMemoryRisk(predictedMemory),
                    Confidence = _memoryUsageModel.GetConfidence()
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IntelligentPredictionSystem] Memory usage prediction failed: {ex.Message}");
                return GetDefaultMemoryUsagePrediction();
            }
        }

        #endregion

        #region Feature Extraction

        private float[] ExtractCoinDemandFeatures()
        {
            if (_coinDemandHistory.Count < 5) return new float[] { 0 };

            var recentData = _coinDemandHistory.TakeLast(10).ToArray();

            return new float[]
            {
                recentData.Average(), // 平均需求
                recentData.Max(), // 峰值需求
                CalculateVariance(recentData), // 需求方差
                CalculateTrend(recentData), // 需求趋势
                (float)DateTime.Now.Hour / 24f, // 时间特征 (0-1)
                (float)DateTime.Now.DayOfWeek / 7f // 星期特征 (0-1)
            };
        }

        private float[] ExtractMemoryUsageFeatures()
        {
            if (_memoryUsageHistory.Count < 5) return new float[] { 0 };

            var recentData = _memoryUsageHistory.TakeLast(10).ToArray();

            return new float[]
            {
                recentData.Average(), // 平均内存使用
                recentData.Max(), // 峰值内存使用
                CalculateVariance(recentData), // 内存使用方差
                CalculateTrend(recentData), // 内存使用趋势
                _objectPool?.CurrentPoolSize ?? 0, // 对象池大小
                Time.realtimeSinceStartup / 3600f // 运行时间(小时)
            };
        }

        #endregion

        #region Trend Analysis

        private float CalculateDemandTrend()
        {
            if (_coinDemandHistory.Count < 2) return 0f;

            var recent = _coinDemandHistory.TakeLast(10).ToArray();
            return CalculateTrend(recent);
        }

        private float CalculateMemoryTrend()
        {
            if (_memoryUsageHistory.Count < 2) return 0f;

            var recent = _memoryUsageHistory.TakeLast(10).ToArray();
            return CalculateTrend(recent);
        }

        private float CalculateTrend(float[] data)
        {
            if (data.Length < 2) return 0f;

            float sumX = 0f, sumY = 0f, sumXY = 0f, sumX2 = 0f;

            for (int i = 0; i < data.Length; i++)
            {
                sumX += i;
                sumY += data[i];
                sumXY += i * data[i];
                sumX2 += i * i;
            }

            int n = data.Length;
            float denominator = n * sumX2 - sumX * sumX;

            return Mathf.Abs(denominator) < 0.001f ? 0f : (n * sumXY - sumX * sumY) / denominator;
        }

        private float CalculateVariance(float[] data)
        {
            if (data.Length < 2) return 0f;

            float mean = data.Average();
            float sum = 0f;

            foreach (float value in data)
            {
                float diff = value - mean;
                sum += diff * diff;
            }

            return sum / data.Length;
        }

        #endregion

        #region Peak Prediction

        private float PredictPeakDemandTime()
        {
            // 简化实现：基于历史峰值模式预测下一个峰值
            var recentPeaks = FindRecentDemandPeaks();

            if (recentPeaks.Count < 2)
            {
                return predictionWindow; // 默认预测在预测窗口末尾
            }

            // 计算峰值间隔的平均值
            float avgInterval = 0f;
            for (int i = 1; i < recentPeaks.Count; i++)
            {
                avgInterval += recentPeaks[i] - recentPeaks[i - 1];
            }
            avgInterval /= recentPeaks.Count - 1;

            // 预测下一个峰值时间
            float lastPeakTime = recentPeaks.Last();
            return Mathf.Min(lastPeakTime + avgInterval, predictionWindow);
        }

        private float PredictMemoryPressureTime()
        {
            // 预测何时会达到内存压力阈值
            if (_memoryUsageHistory.Count < 5) return predictionWindow;

            float currentMemory = _memoryUsageHistory.Last();
            float trend = CalculateMemoryTrend();

            if (trend <= 0)
            {
                return predictionWindow; // 内存使用在下降或稳定
            }

            float pressureThreshold = _memorySystem?.MemoryWarningThresholdMB ?? 80f;
            float timeToPressure = (pressureThreshold - currentMemory) / trend;

            return Mathf.Clamp(timeToPressure, 0f, predictionWindow);
        }

        private List<float> FindRecentDemandPeaks()
        {
            var peaks = new List<float>();

            if (_coinDemandHistory.Count < 3) return peaks;

            for (int i = 1; i < _coinDemandHistory.Count - 1; i++)
            {
                float prev = _coinDemandHistory[i - 1];
                float curr = _coinDemandHistory[i];
                float next = _coinDemandHistory[i + 1];

                // 简单的峰值检测：当前值比前后都高
                if (curr > prev && curr > next && curr > _coinDemandHistory.Average())
                {
                    peaks.Add(i);
                }
            }

            return peaks;
        }

        #endregion

        #region Recommendations

        private void GenerateRecommendations()
        {
            if (_lastPrediction == null || _lastPrediction.Confidence < predictionConfidenceThreshold)
                return;

            var recommendations = new List<AdaptiveRecommendation>();

            // 基于金币需求预测的推荐
            if (_lastPrediction.PredictedCoinDemand != null)
            {
                var demandRec = GeneratePoolSizeRecommendation(_lastPrediction.PredictedCoinDemand);
                if (demandRec != null)
                    recommendations.Add(demandRec);
            }

            // 基于内存使用预测的推荐
            if (_lastPrediction.PredictedMemoryUsage != null)
            {
                var memoryRec = GenerateMemoryRecommendation(_lastPrediction.PredictedMemoryUsage);
                if (memoryRec != null)
                    recommendations.Add(memoryRec);
            }

            // 发送推荐
            foreach (var rec in recommendations)
            {
                OnRecommendationGenerated?.Invoke(rec);
            }
        }

        private AdaptiveRecommendation GeneratePoolSizeRecommendation(CoinDemandPrediction demandPred)
        {
            if (_objectPool == null) return null;

            int currentPoolSize = _objectPool.CurrentPoolSize;
            int recommendedSize = demandPred.RecommendedPoolSize;

            if (recommendedSize > currentPoolSize * expansionThresholdMultiplier)
            {
                return new AdaptiveRecommendation
                {
                    Type = RecommendationType.ExpandPool,
                    Priority = demandPred.DemandTrend > 0 ? RecommendationPriority.High : RecommendationPriority.Medium,
                    Message = $"Predicted demand surge: expand pool from {currentPoolSize} to {recommendedSize}",
                    CurrentValue = currentPoolSize,
                    RecommendedValue = recommendedSize,
                    Confidence = demandPred.Confidence,
                    EstimatedTimeToImpact = demandPred.PeakDemandTime
                };
            }
            else if (recommendedSize < currentPoolSize * contractionThresholdMultiplier)
            {
                return new AdaptiveRecommendation
                {
                    Type = RecommendationType.ContractPool,
                    Priority = RecommendationPriority.Low,
                    Message = $"Predicted demand decrease: consider contracting pool from {currentPoolSize} to {recommendedSize}",
                    CurrentValue = currentPoolSize,
                    RecommendedValue = recommendedSize,
                    Confidence = demandPred.Confidence,
                    EstimatedTimeToImpact = demandPred.PeakDemandTime
                };
            }

            return null;
        }

        private AdaptiveRecommendation GenerateMemoryRecommendation(MemoryUsagePrediction memoryPred)
        {
            float currentMemory = _memorySystem?.CurrentMemoryUsageMB ?? 0f;

            if (memoryPred.RiskLevel == MemoryRisk.High)
            {
                return new AdaptiveRecommendation
                {
                    Type = RecommendationType.MemoryCleanup,
                    Priority = RecommendationPriority.High,
                    Message = $"High memory risk predicted: {memoryPred.PredictedMaxMemoryMB:F1}MB (current: {currentMemory:F1}MB)",
                    CurrentValue = currentMemory,
                    RecommendedValue = memoryPred.PredictedMaxMemoryMB * 0.8f, // 建议20%的缓冲
                    Confidence = memoryPred.Confidence,
                    EstimatedTimeToImpact = memoryPred.MemoryPressureTime
                };
            }
            else if (memoryPred.RiskLevel == MemoryRisk.Medium)
            {
                return new AdaptiveRecommendation
                {
                    Type = RecommendationType.MemoryOptimization,
                    Priority = RecommendationPriority.Medium,
                    Message = $"Medium memory pressure predicted: consider optimization",
                    CurrentValue = currentMemory,
                    RecommendedValue = memoryPred.PredictedMaxMemoryMB * 0.9f,
                    Confidence = memoryPred.Confidence,
                    EstimatedTimeToImpact = memoryPred.MemoryPressureTime
                };
            }

            return null;
        }

        #endregion

        #region Utility Methods

        private int CalculateRecommendedPoolSize(float predictedMaxDemand)
        {
            // 基于预测需求计算推荐的池大小
            // 添加安全缓冲区
            float safetyBuffer = 1.3f; // 30% 缓冲区
            return Mathf.CeilToInt(predictedMaxDemand * safetyBuffer);
        }

        private float CalculatePredictionConfidence(CoinDemandPrediction coinPred, MemoryUsagePrediction memoryPred)
        {
            // 综合置信度计算
            float coinConfidence = coinPred?.Confidence ?? 0f;
            float memoryConfidence = memoryPred?.Confidence ?? 0f;

            // 权重平均
            return (coinConfidence * 0.6f + memoryConfidence * 0.4f);
        }

        private MemoryRisk AssessMemoryRisk(float predictedMemory)
        {
            float threshold = _memorySystem?.MemoryWarningThresholdMB ?? 80f;

            if (predictedMemory > threshold * 1.5f)
                return MemoryRisk.High;
            else if (predictedMemory > threshold)
                return MemoryRisk.Medium;
            else
                return MemoryRisk.Low;
        }

        private CoinDemandPrediction GetDefaultCoinDemandPrediction()
        {
            return new CoinDemandPrediction
            {
                PredictedMaxDemand = _objectPool?.ActiveCoinCount ?? 10,
                DemandTrend = 0f,
                PeakDemandTime = predictionWindow,
                RecommendedPoolSize = _objectPool?.CurrentPoolSize ?? 20,
                Confidence = 0.1f
            };
        }

        private MemoryUsagePrediction GetDefaultMemoryUsagePrediction()
        {
            float currentMemory = _memorySystem?.CurrentMemoryUsageMB ?? 20f;

            return new MemoryUsagePrediction
            {
                PredictedMaxMemoryMB = currentMemory * 1.1f, // 简单预测10%增长
                MemoryTrend = 0.1f,
                MemoryPressureTime = predictionWindow,
                RiskLevel = MemoryRisk.Low,
                Confidence = 0.1f
            };
        }

        private void UpdatePredictionStats()
        {
            _stats.TotalPredictions++;
            _stats.LastPredictionTime = DateTime.UtcNow;

            if (_lastPrediction != null)
            {
                _stats.AverageConfidence = (_stats.AverageConfidence * (_stats.TotalPredictions - 1) + _lastPrediction.Confidence) / _stats.TotalPredictions;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// 手动触发预测更新
        /// </summary>
        public void TriggerPredictionUpdate()
        {
            if (_dataHistory.Count >= minDataPointsForPrediction)
            {
                UpdatePredictions();
                GenerateRecommendations();
            }
        }

        /// <summary>
        /// 获取预测准确性报告
        /// </summary>
        public PredictionAccuracyReport GetAccuracyReport()
        {
            // 这里可以实现预测准确性验证逻辑
            return new PredictionAccuracyReport
            {
                ReportTime = DateTime.UtcNow,
                TotalPredictions = _stats.TotalPredictions,
                AverageConfidence = _stats.AverageConfidence,
                DataPoints = _dataHistory.Count
            };
        }

        /// <summary>
        /// 启用/禁用预测系统
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            enablePrediction = enabled;

            if (enabled && !IsInvoking(nameof(PredictionCoroutine)))
            {
                StartCoroutine(PredictionCoroutine());
            }
            else if (!enabled && IsInvoking(nameof(PredictionCoroutine)))
            {
                StopCoroutine(nameof(PredictionCoroutine));
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            _dataHistory.Clear();
            _coinDemandHistory.Clear();
            _memoryUsageHistory.Clear();
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class PredictionDataPoint
    {
        public DateTime Timestamp;
        public int ActiveCoins;
        public int AvailableCoins;
        public int PoolSize;
        public float MemoryUsageMB;
        public float RequestRate;
        public float ReturnRate;
    }

    [System.Serializable]
    public class PredictionResult
    {
        public DateTime Timestamp;
        public CoinDemandPrediction PredictedCoinDemand;
        public MemoryUsagePrediction PredictedMemoryUsage;
        public List<string> DetectedPatterns;
        public float Confidence;
        public int PredictionWindow;
    }

    [System.Serializable]
    public class CoinDemandPrediction
    {
        public int PredictedMaxDemand;
        public float DemandTrend;
        public float PeakDemandTime;
        public int RecommendedPoolSize;
        public float Confidence;
    }

    [System.Serializable]
    public class MemoryUsagePrediction
    {
        public float PredictedMaxMemoryMB;
        public float MemoryTrend;
        public float MemoryPressureTime;
        public MemoryRisk RiskLevel;
        public float Confidence;
    }

    [System.Serializable]
    public class AdaptiveRecommendation
    {
        public RecommendationType Type;
        public RecommendationPriority Priority;
        public string Message;
        public float CurrentValue;
        public float RecommendedValue;
        public float Confidence;
        public float EstimatedTimeToImpact;
        public DateTime CreatedAt = DateTime.UtcNow;
    }

    [System.Serializable]
    public class PredictionStats
    {
        public int TotalPredictions;
        public float AverageConfidence;
        public DateTime LastPredictionTime;
    }

    [System.Serializable]
    public class PredictionAccuracyReport
    {
        public DateTime ReportTime;
        public int TotalPredictions;
        public float AverageConfidence;
        public int DataPoints;
    }

    public enum RecommendationType
    {
        ExpandPool,
        ContractPool,
        MemoryCleanup,
        MemoryOptimization,
        PerformanceTuning
    }

    public enum RecommendationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum MemoryRisk
    {
        Low,
        Medium,
        High,
        Critical
    }

    #endregion

    #region Prediction Models (Simplified Implementation)

    public interface PredictionModel
    {
        float Predict(float[] features);
        float GetConfidence();
        void Train(float[][] features, float[] targets);
    }

    public class LinearRegressionModel : PredictionModel
    {
        private float[] weights;
        private float bias;
        private float learningRate;
        private float confidence = 0.5f;

        public LinearRegressionModel(float learningRate)
        {
            this.learningRate = learningRate;
            // 初始化权重
            weights = new float[6]; // 假设最多6个特征
            bias = 0f;
        }

        public float Predict(float[] features)
        {
            if (features.Length != weights.Length)
                return 0f;

            float result = bias;
            for (int i = 0; i < features.Length; i++)
            {
                result += weights[i] * features[i];
            }

            return result;
        }

        public float GetConfidence()
        {
            return confidence;
        }

        public void Train(float[][] features, float[] targets)
        {
            // 简化的训练实现
            if (features.Length == 0 || features.Length != targets.Length) return;

            // 简单的梯度下降
            for (int epoch = 0; epoch < 10; epoch++)
            {
                for (int i = 0; i < features.Length; i++)
                {
                    float prediction = Predict(features[i]);
                    float error = targets[i] - prediction;

                    // 更新权重
                    for (int j = 0; j < weights.Length; j++)
                    {
                        weights[j] += learningRate * error * features[i][j];
                    }
                    bias += learningRate * error;
                }
            }

            // 基于误差计算置信度
            confidence = CalculateTrainingConfidence(features, targets);
        }

        private float CalculateTrainingConfidence(float[][] features, float[] targets)
        {
            float totalError = 0f;
            for (int i = 0; i < features.Length; i++)
            {
                float prediction = Predict(features[i]);
                float error = Mathf.Abs(targets[i] - prediction);
                totalError += error;
            }

            float averageError = totalError / features.Length;
            float averageTarget = targets.Average();

            // 置信度基于相对误差
            return Mathf.Clamp01(1f - (averageError / averageTarget));
        }
    }

    public class PatternDetector
    {
        private int historySize;

        public PatternDetector(int historySize)
        {
            this.historySize = historySize;
        }

        public List<string> DetectPatterns(List<PredictionDataPoint> data)
        {
            var patterns = new List<string>();

            if (data.Count < 10) return patterns;

            // 检测周期性模式
            if (DetectPeriodicPattern(data))
                patterns.Add("Periodic demand pattern detected");

            // 检测趋势模式
            if (DetectGrowthPattern(data))
                patterns.Add("Growth trend pattern detected");

            // 检测峰值模式
            if (DetectPeakPattern(data))
                patterns.Add("Peak demand pattern detected");

            return patterns;
        }

        private bool DetectPeriodicPattern(List<PredictionDataPoint> data)
        {
            // 简化的周期性检测
            var values = data.Select(d => d.ActiveCoins).ToArray();
            if (values.Length < 20) return false;

            // 检查是否存在重复模式
            for (int period = 5; period <= values.Length / 4; period++)
            {
                if (CheckPeriodicity(values, period))
                    return true;
            }

            return false;
        }

        private bool CheckPeriodicity(int[] values, int period)
        {
            // 检查指定周期性的相关性
            int matches = 0;
            for (int i = 0; i < values.Length - period; i++)
            {
                float diff = Mathf.Abs(values[i] - values[i + period]);
                if (diff < values.Average() * 0.1f) // 10% 容差
                    matches++;
            }

            return matches > (values.Length - period) * 0.7f; // 70% 匹配率
        }

        private bool DetectGrowthPattern(List<PredictionDataPoint> data)
        {
            var values = data.Select(d => d.ActiveCoins).ToArray();
            if (values.Length < 10) return false;

            // 计算趋势
            float trend = 0f;
            for (int i = 1; i < values.Length; i++)
            {
                trend += values[i] - values[i - 1];
            }
            trend /= values.Length - 1;

            return trend > values.Average() * 0.05f; // 5% 增长阈值
        }

        private bool DetectPeakPattern(List<PredictionDataPoint> data)
        {
            // 检测是否存在规律的峰值模式
            var values = data.Select(d => d.ActiveCoins).ToArray();
            if (values.Length < 20) return false;

            int peakCount = 0;
            float mean = (float)values.Average();

            for (int i = 1; i < values.Length - 1; i++)
            {
                if (values[i] > values[i - 1] && values[i] > values[i + 1] && values[i] > mean * 1.2f)
                {
                    peakCount++;
                }
            }

            return peakCount >= 3; // 至少3个峰值
        }
    }

    #endregion

    #region Extension Methods

    public static class EnumerableExtensions
    {
        public static float[] TakeLast(this List<float> list, int count)
        {
            if (list.Count <= count)
                return list.ToArray();

            return list.Skip(list.Count - count).ToArray();
        }
    }

    #endregion
}