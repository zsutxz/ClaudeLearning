using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoinAnimation.Core
{
    /// <summary>
    /// 自适应配置系统 - Story 1.3 Enhancement
    /// 根据设备性能和使用模式自动调整系统配置
    /// </summary>
    public class AdaptiveConfigurationSystem : MonoBehaviour
    {
        #region Configuration

        [Header("Adaptive Settings")]
        [SerializeField] private bool enableAdaptiveConfig = true;
        [SerializeField] private float configurationUpdateInterval = 10f;
        [SerializeField] private int performanceHistorySize = 60; // 保存60秒性能历史

        [Header("Device Profiles")]
        [SerializeField] private DeviceProfile lowEndProfile;
        [SerializeField] private DeviceProfile mediumProfile;
        [SerializeField] private DeviceProfile highEndProfile;
        [SerializeField] private DeviceProfile ultraHighEndProfile;

        [Header("Performance Thresholds")]
        [SerializeField] private float lowEndFPSThreshold = 30f;
        [SerializeField] private float mediumEndFPSThreshold = 45f;
        [SerializeField] private float highEndFPSThreshold = 55f;
        [SerializeField] private float memoryLowEndThreshold = 512f; // MB
        [SerializeField] private float memoryMediumEndThreshold = 2048f; // MB

        [Header("Adaptive Parameters")]
        [SerializeField] private bool enableDynamicPoolSizing = true;
        [SerializeField] private bool enableDynamicQualityAdjustment = true;
        [SerializeField] private bool enablePredictiveOptimization = true;

        #endregion

        #region Private Fields

        private CoinObjectPool _objectPool;
        private MemoryManagementSystem _memorySystem;
        private PerformanceDashboard _performanceDashboard;
        private IntelligentPredictionSystem _predictionSystem;

        // 性能历史数据
        private readonly Queue<PerformanceSnapshot> _performanceHistory = new Queue<PerformanceSnapshot>();
        private DevicePerformanceProfile _currentDeviceProfile;
        private DeviceCategory _currentDeviceCategory;

        // 配置状态
        private AdaptiveConfiguration _currentConfig;
        private AdaptiveConfiguration _targetConfig;
        private float _lastConfigUpdate = 0f;

        // 统计数据
        private ConfigurationStats _stats = new ConfigurationStats();

        #endregion

        #region Properties

        public AdaptiveConfiguration CurrentConfiguration => _currentConfig;
        public DevicePerformanceProfile CurrentDeviceProfile => _currentDeviceProfile;
        public DeviceCategory CurrentDeviceCategory => _currentDeviceCategory;
        public ConfigurationStats Stats => _stats;

        #endregion

        #region Events

        public event Action<AdaptiveConfiguration> OnConfigurationChanged;
        public event Action<DeviceProfileAppliedEventArgs> OnDeviceProfileApplied;
        public event Action<PerformanceDegradationEventArgs> OnPerformanceDegradationDetected;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializeProfiles();
            DetectDeviceCapabilities();
            ApplyInitialConfiguration();

            if (enableAdaptiveConfig)
            {
                StartCoroutine(AdaptiveConfigurationCoroutine());
            }
        }

        #endregion

        #region Initialization

        private void InitializeProfiles()
        {
            // 初始化设备配置文件
            if (lowEndProfile == null)
                lowEndProfile = CreateLowEndProfile();
            if (mediumProfile == null)
                mediumProfile = CreateMediumEndProfile();
            if (highEndProfile == null)
                highEndProfile = CreateHighEndProfile();
            if (ultraHighEndProfile == null)
                ultraHighEndProfile = CreateUltraHighEndProfile();

            FindSystemComponents();
        }

        private void FindSystemComponents()
        {
            _objectPool = FindObjectOfType<CoinObjectPool>();
            _memorySystem = FindObjectOfType<MemoryManagementSystem>();
            _performanceDashboard = FindObjectOfType<PerformanceDashboard>();
            _predictionSystem = FindObjectOfType<IntelligentPredictionSystem>();
        }

        private void DetectDeviceCapabilities()
        {
            var deviceInfo = new DeviceInfo
            {
                SystemMemory = SystemInfo.systemMemorySize,
                GraphicsMemory = SystemInfo.graphicsMemorySize,
                ProcessorType = SystemInfo.processorType,
                ProcessorCount = SystemInfo.processorCount,
                GraphicsDeviceName = SystemInfo.graphicsDeviceName,
                GraphicsDeviceVersion = SystemInfo.graphicsDeviceVersion,
                DeviceModel = SystemInfo.deviceModel,
                OperatingSystem = SystemInfo.operatingSystem,
                DeviceType = SystemInfo.deviceType
            };

            _currentDeviceProfile = AnalyzeDeviceCapabilities(deviceInfo);
            _currentDeviceCategory = CategorizeDevice(_currentDeviceProfile);

            Debug.Log($"[AdaptiveConfigurationSystem] Device detected: {_currentDeviceCategory} - {deviceInfo.GraphicsDeviceName}");
        }

        #endregion

        #region Device Detection

        private DevicePerformanceProfile AnalyzeDeviceCapabilities(DeviceInfo deviceInfo)
        {
            var profile = new DevicePerformanceProfile
            {
                DeviceInfo = deviceInfo,
                PerformanceScore = CalculatePerformanceScore(deviceInfo),
                DetectedAt = DateTime.UtcNow
            };

            // 检测特殊功能
            profile.SupportsMultithreading = deviceInfo.ProcessorCount > 2;
            profile.HasDedicatedGPU = deviceInfo.GraphicsMemory > 512;
            profile.IsHighEndDevice = profile.PerformanceScore > 70f;
            profile.IsMobileDevice = deviceInfo.DeviceType == DeviceType.Handheld;

            return profile;
        }

        private float CalculatePerformanceScore(DeviceInfo deviceInfo)
        {
            float score = 0f;

            // CPU得分 (0-30分)
            float cpuScore = Mathf.Min(30f, deviceInfo.ProcessorCount * 5f);
            score += cpuScore;

            // 内存得分 (0-30分)
            float memoryScore = Mathf.Min(30f, deviceInfo.SystemMemory / 128f); // 每128MB 1分
            score += memoryScore;

            // GPU得分 (0-40分)
            float gpuScore = 0f;
            if (deviceInfo.GraphicsMemory > 0)
            {
                gpuScore = Mathf.Min(40f, deviceInfo.GraphicsMemory / 64f); // 每64MB 1分
            }
            else
            {
                // 基于GPU名称估算
                if (deviceInfo.GraphicsDeviceName.Contains("RTX") || deviceInfo.GraphicsDeviceName.Contains("GTX") ||
                    deviceInfo.GraphicsDeviceName.Contains("Radeon") || deviceInfo.GraphicsDeviceName.Contains("Adreno"))
                {
                    gpuScore = 20f;
                }
                else
                {
                    gpuScore = 10f;
                }
            }
            score += gpuScore;

            return score;
        }

        private DeviceCategory CategorizeDevice(DevicePerformanceProfile profile)
        {
            if (profile.PerformanceScore >= 80f)
                return DeviceCategory.UltraHighEnd;
            else if (profile.PerformanceScore >= 60f)
                return DeviceCategory.HighEnd;
            else if (profile.PerformanceScore >= 40f)
                return DeviceCategory.Medium;
            else
                return DeviceCategory.LowEnd;
        }

        #endregion

        #region Configuration Profiles

        private DeviceProfile CreateLowEndProfile()
        {
            return new DeviceProfile
            {
                Name = "Low End",
                TargetFPS = 30f,
                MaxConcurrentCoins = 20,
                PoolInitialSize = 10,
                PoolMaxSize = 30,
                MemoryLimitMB = 200f,
                QualitySettings = new DeviceQualityConfiguration
                {
                    EnableParticleEffects = false,
                    EnableAudioEffects = false,
                    EnableRotationAnimation = false,
                    AnimationQuality = AnimationQuality.Low,
                    TextureQuality = TextureQuality.Low,
                    EnableAdvancedEffects = false
                },
                OptimizationSettings = new OptimizationConfiguration
                {
                    GCOptimizationLevel = GCOptimizationLevel.Aggressive,
                    MemoryPoolSize = 100,
                    UpdateInterval = 0.1f,
                    EnablePredictiveOptimization = false
                }
            };
        }

        private DeviceProfile CreateMediumEndProfile()
        {
            return new DeviceProfile
            {
                Name = "Medium End",
                TargetFPS = 45f,
                MaxConcurrentCoins = 50,
                PoolInitialSize = 25,
                PoolMaxSize = 75,
                MemoryLimitMB = 400f,
                QualitySettings = new DeviceQualityConfiguration
                {
                    EnableParticleEffects = true,
                    EnableAudioEffects = true,
                    EnableRotationAnimation = true,
                    AnimationQuality = AnimationQuality.Medium,
                    TextureQuality = TextureQuality.Medium,
                    EnableAdvancedEffects = false
                },
                OptimizationSettings = new OptimizationConfiguration
                {
                    GCOptimizationLevel = GCOptimizationLevel.Moderate,
                    MemoryPoolSize = 200,
                    UpdateInterval = 0.05f,
                    EnablePredictiveOptimization = true
                }
            };
        }

        private DeviceProfile CreateHighEndProfile()
        {
            return new DeviceProfile
            {
                Name = "High End",
                TargetFPS = 60f,
                MaxConcurrentCoins = 100,
                PoolInitialSize = 50,
                PoolMaxSize = 150,
                MemoryLimitMB = 800f,
                QualitySettings = new DeviceQualityConfiguration
                {
                    EnableParticleEffects = true,
                    EnableAudioEffects = true,
                    EnableRotationAnimation = true,
                    AnimationQuality = AnimationQuality.High,
                    TextureQuality = TextureQuality.High,
                    EnableAdvancedEffects = true
                },
                OptimizationSettings = new OptimizationConfiguration
                {
                    GCOptimizationLevel = GCOptimizationLevel.Conservative,
                    MemoryPoolSize = 400,
                    UpdateInterval = 0.02f,
                    EnablePredictiveOptimization = true
                }
            };
        }

        private DeviceProfile CreateUltraHighEndProfile()
        {
            return new DeviceProfile
            {
                Name = "Ultra High End",
                TargetFPS = 75f,
                MaxConcurrentCoins = 200,
                PoolInitialSize = 100,
                PoolMaxSize = 300,
                MemoryLimitMB = 1600f,
                QualitySettings = new DeviceQualityConfiguration
                {
                    EnableParticleEffects = true,
                    EnableAudioEffects = true,
                    EnableRotationAnimation = true,
                    AnimationQuality = AnimationQuality.Ultra,
                    TextureQuality = TextureQuality.Ultra,
                    EnableAdvancedEffects = true
                },
                OptimizationSettings = new OptimizationConfiguration
                {
                    GCOptimizationLevel = GCOptimizationLevel.Minimal,
                    MemoryPoolSize = 800,
                    UpdateInterval = 0.016f, // 60 FPS
                    EnablePredictiveOptimization = true
                }
            };
        }

        #endregion

        #region Adaptive Configuration

        private void ApplyInitialConfiguration()
        {
            var profile = GetProfileForDevice(_currentDeviceCategory);
            _currentConfig = CreateConfigurationFromProfile(profile);
            _targetConfig = _currentConfig;

            ApplyConfiguration(_currentConfig);

            var eventArgs = new DeviceProfileAppliedEventArgs
            {
                Profile = profile,
                Configuration = _currentConfig,
                DeviceCategory = _currentDeviceCategory,
                AppliedAt = DateTime.UtcNow
            };

            OnDeviceProfileApplied?.Invoke(eventArgs);
        }

        private IEnumerator AdaptiveConfigurationCoroutine()
        {
            while (enableAdaptiveConfig)
            {
                CollectPerformanceData();
                AnalyzePerformanceTrends();
                UpdateTargetConfiguration();

                if (ShouldUpdateConfiguration())
                {
                    ApplyConfigurationTransition();
                }

                yield return new WaitForSeconds(configurationUpdateInterval);
            }
        }

        private void CollectPerformanceData()
        {
            var snapshot = new PerformanceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                FPS = _performanceDashboard?.CurrentStats.AverageFPS ?? 60f,
                MemoryUsageMB = _memorySystem?.CurrentMemoryUsageMB ?? 0f,
                ActiveCoins = _objectPool?.ActiveCoinCount ?? 0,
                PoolEfficiency = _objectPool?.GetPerformanceMetrics().PoolHitRate ?? 1f,
                CPUUsage = GetCPUUsage(),
                GPUUsage = GetGPUUsage()
            };

            _performanceHistory.Enqueue(snapshot);
            while (_performanceHistory.Count > performanceHistorySize)
                _performanceHistory.Dequeue();

            _stats.LastSnapshotTime = snapshot.Timestamp;
            _stats.TotalSnapshots++;
        }

        private void AnalyzePerformanceTrends()
        {
            if (_performanceHistory.Count < 5) return;

            var recentSnapshots = _performanceHistory.ToArray();
            var performanceAnalysis = AnalyzePerformanceData(recentSnapshots);

            // 检测性能退化
            if (performanceAnalysis.IsDegrading)
            {
                var degradationArgs = new PerformanceDegradationEventArgs
                {
                    DegradationType = performanceAnalysis.DegradationType,
                    Severity = performanceAnalysis.Severity,
                    CurrentFPS = performanceAnalysis.CurrentFPS,
                    TargetFPS = performanceAnalysis.TargetFPS,
                    DetectedAt = DateTime.UtcNow
                };

                OnPerformanceDegradationDetected?.Invoke(degradationArgs);
            }
        }

        private void UpdateTargetConfiguration()
        {
            var currentPerformance = GetCurrentPerformanceMetrics();
            var recommendedProfile = RecommendOptimalProfile(currentPerformance);

            _targetConfig = CreateConfigurationFromProfile(recommendedProfile);
        }

        private bool ShouldUpdateConfiguration()
        {
            if (_targetConfig == null || _currentConfig == null) return false;

            // 检查是否有显著的配置变化
            float configDifference = CalculateConfigurationDifference(_currentConfig, _targetConfig);
            float timeSinceLastUpdate = Time.time - _lastConfigUpdate;

            return configDifference > 0.1f && timeSinceLastUpdate > configurationUpdateInterval;
        }

        private void ApplyConfigurationTransition()
        {
            StartCoroutine(SmoothConfigurationTransition(_currentConfig, _targetConfig, 1f));
        }

        private IEnumerator SmoothConfigurationTransition(AdaptiveConfiguration from, AdaptiveConfiguration to, float duration)
        {
            float startTime = Time.time;

            while (Time.time - startTime < duration)
            {
                float t = (Time.time - startTime) / duration;
                t = Mathf.SmoothStep(0f, 1f, t); // 平滑插值

                var interpolatedConfig = InterpolateConfigurations(from, to, t);
                ApplyConfiguration(interpolatedConfig);

                yield return null;
            }

            _currentConfig = to;
            _lastConfigUpdate = Time.time;

            OnConfigurationChanged?.Invoke(_currentConfig);
            _stats.ConfigurationChanges++;
        }

        #endregion

        #region Configuration Application

        private void ApplyConfiguration(AdaptiveConfiguration config)
        {
            // 应用对象池配置
            if (_objectPool != null)
            {
                ApplyPoolConfiguration(_objectPool, config.PoolConfiguration);
            }

            // 应用内存管理配置
            if (_memorySystem != null)
            {
                ApplyMemoryConfiguration(_memorySystem, config.MemoryConfiguration);
            }

            // 应用质量设置
            ApplyQualitySettings(config.QualityConfiguration);

            // 更新统计
            _stats.LastConfigurationUpdate = DateTime.UtcNow;
        }

        private void ApplyPoolConfiguration(CoinObjectPool pool, PoolConfiguration config)
        {
            // 这里需要通过反射或公共API来设置池配置
            // 由于原始类可能没有这些公共设置方法，我们记录建议的配置
            Debug.Log($"[AdaptiveConfigurationSystem] Pool configuration recommendation: " +
                     $"InitialSize={config.InitialSize}, MaxSize={config.MaxSize}, " +
                     $"BatchSize={config.ExpansionBatchSize}");
        }

        private void ApplyMemoryConfiguration(MemoryManagementSystem memorySystem, MemoryConfiguration config)
        {
            Debug.Log($"[AdaptiveConfigurationSystem] Memory configuration recommendation: " +
                     $"MonitoringInterval={config.MonitoringInterval}, " +
                     $"GCOptimizationLevel={config.GCOptimizationLevel}");
        }

        private void ApplyQualitySettings(QualityConfiguration config)
        {
            // 应用Unity质量设置
            QualitySettings.vSyncCount = config.VSyncCount;
            QualitySettings.antiAliasing = config.AntiAliasing;
            QualitySettings.anisotropicFiltering = config.AnisotropicFiltering;
            QualitySettings.shadowResolution = config.ShadowResolution;
            QualitySettings.shadowDistance = config.ShadowDistance;
        }

        #endregion

        #region Utility Methods

        private DeviceProfile GetProfileForDevice(DeviceCategory category)
        {
            return category switch
            {
                DeviceCategory.LowEnd => lowEndProfile,
                DeviceCategory.Medium => mediumProfile,
                DeviceCategory.HighEnd => highEndProfile,
                DeviceCategory.UltraHighEnd => ultraHighEndProfile,
                _ => mediumProfile
            };
        }

        private AdaptiveConfiguration CreateConfigurationFromProfile(DeviceProfile profile)
        {
            return new AdaptiveConfiguration
            {
                DeviceCategory = CategorizeDevice(_currentDeviceProfile),
                PoolConfiguration = new PoolConfiguration
                {
                    InitialSize = profile.PoolInitialSize,
                    MaxSize = profile.PoolMaxSize,
                    ExpansionBatchSize = Mathf.Max(5, profile.PoolMaxSize / 10),
                    EnableAutoExpansion = true,
                    EnableAutoContraction = true
                },
                MemoryConfiguration = new MemoryConfiguration
                {
                    MonitoringInterval = 1f,
                    GCOptimizationLevel = profile.OptimizationSettings.GCOptimizationLevel,
                    MemoryLimitMB = profile.MemoryLimitMB,
                    EnableLeakDetection = true
                },
                QualityConfiguration = new QualityConfiguration
                {
                    VSyncCount = profile.TargetFPS >= 60f ? 1 : 0,
                    AntiAliasing = profile.QualitySettings.AnimationQuality >= AnimationQuality.High ? 2 : 0,
                    AnisotropicFiltering = profile.QualitySettings.TextureQuality >= TextureQuality.High ?
                        AnisotropicFiltering.Enable : AnisotropicFiltering.Disable,
                    ShadowResolution = profile.QualitySettings.EnableAdvancedEffects ?
                        ShadowResolution.High : ShadowResolution.Low,
                    ShadowDistance = profile.QualitySettings.EnableAdvancedEffects ? 150f : 50f,
                    AnimationQuality = profile.QualitySettings.AnimationQuality,
                    TextureQuality = profile.QualitySettings.TextureQuality,
                    EnableAdvancedEffects = profile.QualitySettings.EnableAdvancedEffects
                }
            };
        }

        private PerformanceMetrics GetCurrentPerformanceMetrics()
        {
            var snapshot = _performanceHistory.Count > 0 ? _performanceHistory.ToArray().Last() : new PerformanceSnapshot();

            return NewMethod(snapshot);
        }

        private static PerformanceMetrics NewMethod(PerformanceSnapshot snapshot) => new PerformanceMetrics
        {
            FPS = snapshot.FPS,
            MemoryUsageMB = snapshot.MemoryUsageMB,
            ActiveCoins = snapshot.ActiveCoins,
            PoolEfficiency = snapshot.PoolEfficiency,
            CPUUsage = snapshot.CPUUsage,
            GPUUsage = snapshot.GPUUsage
        };

        private DeviceProfile RecommendOptimalProfile(PerformanceMetrics currentPerformance)
        {
            // 基于当前性能推荐最佳配置文件
            if (currentPerformance.FPS < lowEndFPSThreshold || currentPerformance.MemoryUsageMB > memoryLowEndThreshold)
            {
                return lowEndProfile;
            }
            else if (currentPerformance.FPS < mediumEndFPSThreshold || currentPerformance.MemoryUsageMB > memoryMediumEndThreshold)
            {
                return mediumProfile;
            }
            else if (currentPerformance.FPS < highEndFPSThreshold)
            {
                return highEndProfile;
            }
            else
            {
                return ultraHighEndProfile;
            }
        }

        private float CalculateConfigurationDifference(AdaptiveConfiguration config1, AdaptiveConfiguration config2)
        {
            // 计算两个配置之间的差异程度 (0-1)
            float difference = 0f;

            difference += Mathf.Abs(config1.PoolConfiguration.InitialSize - config2.PoolConfiguration.InitialSize) / 100f;
            difference += Mathf.Abs(config1.PoolConfiguration.MaxSize - config2.PoolConfiguration.MaxSize) / 200f;
            difference += Mathf.Abs(config1.MemoryConfiguration.MemoryLimitMB - config2.MemoryConfiguration.MemoryLimitMB) / 1000f;

            return difference / 3f; // 平均差异
        }

        private AdaptiveConfiguration InterpolateConfigurations(AdaptiveConfiguration from, AdaptiveConfiguration to, float t)
        {
            return new AdaptiveConfiguration
            {
                DeviceCategory = t > 0.5f ? to.DeviceCategory : from.DeviceCategory,
                PoolConfiguration = new PoolConfiguration
                {
                    InitialSize = Mathf.RoundToInt(Mathf.Lerp(from.PoolConfiguration.InitialSize, to.PoolConfiguration.InitialSize, t)),
                    MaxSize = Mathf.RoundToInt(Mathf.Lerp(from.PoolConfiguration.MaxSize, to.PoolConfiguration.MaxSize, t)),
                    ExpansionBatchSize = Mathf.RoundToInt(Mathf.Lerp(from.PoolConfiguration.ExpansionBatchSize, to.PoolConfiguration.ExpansionBatchSize, t)),
                    EnableAutoExpansion = to.PoolConfiguration.EnableAutoExpansion,
                    EnableAutoContraction = to.PoolConfiguration.EnableAutoContraction
                },
                MemoryConfiguration = new MemoryConfiguration
                {
                    MonitoringInterval = Mathf.Lerp(from.MemoryConfiguration.MonitoringInterval, to.MemoryConfiguration.MonitoringInterval, t),
                    GCOptimizationLevel = t > 0.5f ? to.MemoryConfiguration.GCOptimizationLevel : from.MemoryConfiguration.GCOptimizationLevel,
                    MemoryLimitMB = Mathf.Lerp(from.MemoryConfiguration.MemoryLimitMB, to.MemoryConfiguration.MemoryLimitMB, t),
                    EnableLeakDetection = to.MemoryConfiguration.EnableLeakDetection
                },
                QualityConfiguration = new QualityConfiguration
                {
                    VSyncCount = t > 0.5f ? to.QualityConfiguration.VSyncCount : from.QualityConfiguration.VSyncCount,
                    AntiAliasing = t > 0.5f ? to.QualityConfiguration.AntiAliasing : from.QualityConfiguration.AntiAliasing,
                    AnisotropicFiltering = t > 0.5f ? to.QualityConfiguration.AnisotropicFiltering : from.QualityConfiguration.AnisotropicFiltering,
                    ShadowResolution = t > 0.5f ? to.QualityConfiguration.ShadowResolution : from.QualityConfiguration.ShadowResolution,
                    ShadowDistance = Mathf.Lerp(from.QualityConfiguration.ShadowDistance, to.QualityConfiguration.ShadowDistance, t)
                }
            };
        }

        private PerformanceAnalysis AnalyzePerformanceData(PerformanceSnapshot[] snapshots)
        {
            var analysis = new PerformanceAnalysis();

            if (snapshots.Length < 2) return analysis;

            // 计算FPS趋势
            float fpsTrend = CalculateTrend(snapshots, s => s.FPS);
            analysis.CurrentFPS = snapshots.Last().FPS;
            analysis.TargetFPS = _currentConfig?.TargetFPS ?? 60f;

            // 检测性能退化
            if (fpsTrend < -1f) // FPS下降趋势
            {
                analysis.IsDegrading = true;
                analysis.DegradationType = DegradationType.FPS;
                analysis.Severity = Mathf.Clamp01(Mathf.Abs(fpsTrend) / 10f);
            }

            // 检测内存问题
            float memoryTrend = CalculateTrend(snapshots, s => s.MemoryUsageMB);
            if (memoryTrend > 1f) // 内存增长趋势
            {
                analysis.IsDegrading = true;
                analysis.DegradationType = DegradationType.Memory;
                analysis.Severity = Mathf.Clamp01(memoryTrend / 5f);
            }

            return analysis;
        }

        private float CalculateTrend(PerformanceSnapshot[] snapshots, Func<PerformanceSnapshot, float> selector)
        {
            if (snapshots.Length < 2) return 0f;

            float sumX = 0f, sumY = 0f, sumXY = 0f, sumX2 = 0f;

            for (int i = 0; i < snapshots.Length; i++)
            {
                float x = i;
                float y = selector(snapshots[i]);

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            int n = snapshots.Length;
            float denominator = n * sumX2 - sumX * sumX;

            return Mathf.Abs(denominator) < 0.001f ? 0f : (n * sumXY - sumX * sumY) / denominator;
        }

        private float GetCPUUsage()
        {
            // 简化的CPU使用率估算
            // 在实际项目中，可以使用更专业的性能监控库
            return UnityEngine.Random.Range(10f, 30f); // 模拟数据
        }

        private float GetGPUUsage()
        {
            // 简化的GPU使用率估算
            return UnityEngine.Random.Range(20f, 60f); // 模拟数据
        }

        #endregion

        #region Public API

        /// <summary>
        /// 手动触发配置更新
        /// </summary>
        public void TriggerConfigurationUpdate()
        {
            if (enableAdaptiveConfig)
            {
                UpdateTargetConfiguration();
                if (ShouldUpdateConfiguration())
                {
                    ApplyConfigurationTransition();
                }
            }
        }

        /// <summary>
        /// 设置设备配置文件
        /// </summary>
        public void SetDeviceProfile(DeviceCategory category)
        {
            var profile = GetProfileForDevice(category);
            _currentDeviceCategory = category;
            _targetConfig = CreateConfigurationFromProfile(profile);

            ApplyConfigurationTransition();
        }

        /// <summary>
        /// 获取系统诊断报告
        /// </summary>
        public SystemDiagnosticsReport GetDiagnosticsReport()
        {
            return new SystemDiagnosticsReport
            {
                GeneratedAt = DateTime.UtcNow,
                DeviceProfile = _currentDeviceProfile,
                DeviceCategory = _currentDeviceCategory,
                CurrentConfiguration = _currentConfig,
                PerformanceMetrics = GetCurrentPerformanceMetrics(),
                Statistics = _stats,
                RecentPerformanceData = _performanceHistory.ToArray()
            };
        }

        /// <summary>
        /// 导出配置为JSON
        /// </summary>
        public string ExportConfiguration()
        {
            return JsonUtility.ToJson(_currentConfig, true);
        }

        /// <summary>
        /// 从JSON导入配置
        /// </summary>
        public void ImportConfiguration(string json)
        {
            try
            {
                var config = JsonUtility.FromJson<AdaptiveConfiguration>(json);
                _targetConfig = config;
                ApplyConfigurationTransition();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AdaptiveConfigurationSystem] Failed to import configuration: {ex.Message}");
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            _performanceHistory.Clear();
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class DeviceInfo
    {
        public float SystemMemory;
        public float GraphicsMemory;
        public string ProcessorType;
        public int ProcessorCount;
        public string GraphicsDeviceName;
        public string GraphicsDeviceVersion;
        public string DeviceModel;
        public string OperatingSystem;
        public DeviceType DeviceType;
    }

    [System.Serializable]
    public class DevicePerformanceProfile
    {
        public DeviceInfo DeviceInfo;
        public float PerformanceScore;
        public bool SupportsMultithreading;
        public bool HasDedicatedGPU;
        public bool IsHighEndDevice;
        public bool IsMobileDevice;
        public DateTime DetectedAt;
    }

    [System.Serializable]
    public class DeviceProfile
    {
        public string Name;
        public float TargetFPS;
        public int MaxConcurrentCoins;
        public int PoolInitialSize;
        public int PoolMaxSize;
        public float MemoryLimitMB;
        public DeviceQualityConfiguration QualitySettings;
        public OptimizationConfiguration OptimizationSettings;
    }

    [System.Serializable]
    public class DeviceQualityConfiguration
    {
        public bool EnableParticleEffects;
        public bool EnableAudioEffects;
        public bool EnableRotationAnimation;
        public AnimationQuality AnimationQuality;
        public TextureQuality TextureQuality;
        public bool EnableAdvancedEffects;
    }

    [System.Serializable]
    public class OptimizationConfiguration
    {
        public GCOptimizationLevel GCOptimizationLevel;
        public int MemoryPoolSize;
        public float UpdateInterval;
        public bool EnablePredictiveOptimization;
    }

    [System.Serializable]
    public class AdaptiveConfiguration
    {
        public DeviceCategory DeviceCategory;
        public float TargetFPS;
        public PoolConfiguration PoolConfiguration;
        public MemoryConfiguration MemoryConfiguration;
        public QualityConfiguration QualityConfiguration;
        public DateTime CreatedAt = DateTime.UtcNow;
    }

    [System.Serializable]
    public class PoolConfiguration
    {
        public int InitialSize;
        public int MaxSize;
        public int ExpansionBatchSize;
        public bool EnableAutoExpansion;
        public bool EnableAutoContraction;
    }

    [System.Serializable]
    public class MemoryConfiguration
    {
        public float MonitoringInterval;
        public GCOptimizationLevel GCOptimizationLevel;
        public float MemoryLimitMB;
        public bool EnableLeakDetection;
    }

    [System.Serializable]
    public class QualityConfiguration
    {
        public int VSyncCount;
        public int AntiAliasing;
        public AnisotropicFiltering AnisotropicFiltering;
        public ShadowResolution ShadowResolution;
        public float ShadowDistance;

        // Additional properties for device quality compatibility
        public AnimationQuality AnimationQuality;
        public TextureQuality TextureQuality;
        public bool EnableAdvancedEffects;
    }

    [System.Serializable]
    public class PerformanceSnapshot
    {
        public DateTime Timestamp;
        public float FPS;
        public float MemoryUsageMB;
        public int ActiveCoins;
        public float PoolEfficiency;
        public float CPUUsage;
        public float GPUUsage;

        public float PoolHitRate { get; internal set; }
        public float AllocatedMemoryMB { get; internal set; }
        public float GCCount { get; internal set; }
        public float FrameTime { get; internal set; }
        public float RenderTime { get; internal set; }
        public PerformanceTrend Trend { get; internal set; }
        public float MemoryMB { get; internal set; }
        public DashboardState State { get; internal set; }
        public int ActiveAlerts { get; internal set; }
    }

    [System.Serializable]
    public class ConfigurationStats
    {
        public int TotalSnapshots;
        public int ConfigurationChanges;
        public DateTime LastConfigurationUpdate;
        public DateTime LastSnapshotTime;
    }

    // PerformanceMetrics is defined in ICoinAnimationManager.cs - using that definition

    [System.Serializable]
    public class PerformanceAnalysis
    {
        public bool IsDegrading;
        public DegradationType DegradationType;
        public float Severity;
        public float CurrentFPS;
        public float TargetFPS;
    }

    [System.Serializable]
    public class SystemDiagnosticsReport
    {
        public DateTime GeneratedAt;
        public DevicePerformanceProfile DeviceProfile;
        public DeviceCategory DeviceCategory;
        public AdaptiveConfiguration CurrentConfiguration;
        public PerformanceMetrics PerformanceMetrics;
        public ConfigurationStats Statistics;
        public PerformanceSnapshot[] RecentPerformanceData;
    }

    public class DeviceProfileAppliedEventArgs : EventArgs
    {
        public DeviceProfile Profile;
        public AdaptiveConfiguration Configuration;
        public DeviceCategory DeviceCategory;
        public DateTime AppliedAt;
    }

    public class PerformanceDegradationEventArgs : EventArgs
    {
        public DegradationType DegradationType;
        public float Severity;
        public float CurrentFPS;
        public float TargetFPS;
        public DateTime DetectedAt;
    }

    public enum DeviceCategory
    {
        LowEnd,
        Medium,
        HighEnd,
        UltraHighEnd
    }

    // AnimationQuality is defined in ICoinAnimationManager.cs - using that definition

    public enum TextureQuality
    {
        Low,
        Medium,
        High,
        Ultra
    }

    public enum GCOptimizationLevel
    {
        Aggressive,
        Moderate,
        Conservative,
        Minimal
    }

    public enum DegradationType
    {
        FPS,
        Memory,
        CPU,
        GPU
    }

    #endregion
}