using CoinAnimation.AdaptiveQuality;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CoinAnimation.Core.DeviceProfiling
{
    /// <summary>
    /// 设备性能等级分类
    /// </summary>
    public enum DevicePerformanceClass
    {
        Unknown,
        LowEnd,
        MidRange,
        HighEnd,
        UltraHighEnd
    }

    /// <summary>
    /// 设备配置文件预设管理器
    /// Story 2.1 Task 5.4 - 设备特定性能配置文件和预设
    /// </summary>
    public class DeviceProfilePresetsManager : MonoBehaviour
    {
        #region Configuration

        [Header("Presets Management")]
        [SerializeField] private bool enablePresetsManagement = true;
        [SerializeField] private bool autoLoadPresets = true;
        [SerializeField] private string presetsDirectoryName = "ProfilePresets";

        [Header("Built-in Presets")]
        [SerializeField] private List<DevicePreset> builtInPresets = new List<DevicePreset>();

        [Header("Preset Categories")]
        [SerializeField] private PresetCategories presetCategories;

        #endregion

        #region Private Fields

        private string _presetsDirectoryPath;
        private Dictionary<string, DevicePreset> _presetsCache = new Dictionary<string, DevicePreset>();
        private Dictionary<DevicePerformanceClass, List<DevicePreset>> _categorizedPresets = new Dictionary<DevicePerformanceClass, List<DevicePreset>>();

        #endregion

        #region Events

        public event Action<DevicePreset> OnPresetLoaded;
        public event Action<DevicePreset> OnPresetCreated;
        public event Action<string> OnPresetError;

        #endregion

        #region Properties

        public Dictionary<string, DevicePreset> PresetsCache => _presetsCache;
        public Dictionary<DevicePerformanceClass, List<DevicePreset>> CategorizedPresets => _categorizedPresets;
        public string PresetsDirectory => _presetsDirectoryPath;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            SetupPresetsDirectory();
            InitializeBuiltInPresets();
            InitializePresetCategories();

            if (autoLoadPresets)
            {
                LoadAllPresets();
            }
        }

        #endregion

        #region Initialization

        private void SetupPresetsDirectory()
        {
            _presetsDirectoryPath = Path.Combine(Application.persistentDataPath, presetsDirectoryName);

            if (!Directory.Exists(_presetsDirectoryPath))
            {
                Directory.CreateDirectory(_presetsDirectoryPath);
                Debug.Log($"[DeviceProfilePresetsManager] Created presets directory: {_presetsDirectoryPath}");
            }
        }

        private void InitializeBuiltInPresets()
        {
            if (builtInPresets.Count == 0)
            {
                builtInPresets = CreateBuiltInPresets();
            }

            foreach (var preset in builtInPresets)
            {
                _presetsCache[preset.id] = preset;
            }
        }

        private void InitializePresetCategories()
        {
            if (presetCategories == null)
            {
                presetCategories = CreateDefaultPresetCategories();
            }
        }

        #endregion

        #region Built-in Presets Creation

        private List<DevicePreset> CreateBuiltInPresets()
        {
            return new List<DevicePreset>
            {
                // 低端设备预设
                CreateLowEndMinimalPreset(),
                CreateLowEndBalancedPreset(),
                CreateLowEndPerformancePreset(),

                // 中端设备预设
                CreateMidRangeBalancedPreset(),
                CreateMidRangeQualityPreset(),
                CreateMidRangePerformancePreset(),
                CreateMidRangeOptimalPreset(),

                // 高端设备预设
                CreateHighEndBalancedPreset(),
                CreateHighEndQualityPreset(),
                CreateHighEndPerformancePreset(),
                CreateHighEndUltimatePreset(),
                CreateHighEndMaximumPreset(),

                // 特殊用途预设
                CreateStreamingPreset(),
                CreateBatterySaverPreset(),
                CreateShowcasePreset(),
                CreateTestingPreset()
            };
        }

        private DevicePreset CreateLowEndMinimalPreset()
        {
            return new DevicePreset
            {
                id = "low_end_minimal",
                name = "Low-End Minimal",
                description = "Maximum performance for very low-end devices",
                targetClass = DevicePerformanceClass.LowEnd,
                category = PresetCategory.Performance,
                priority = 10,
                isBuiltIn = true,
                version = "1.0.0",
                createdAt = DateTime.UtcNow,
                settings = new DevicePresetSettings
                {
                    qualitySettings = new QualitySettingsData
                    {
                        qualityLevel = QualityLevel.Minimum,
                        targetFPS = 30,
                        maximumFPS = 60,
                        enableVSync = true,
                        antiAliasing = 0,
                        textureQuality = 0,
                        shadowQuality = ShadowQuality.Disable,
                        shadowDistance = 25f,
                        particleCount = 5,
                        enablePostProcessing = false,
                        enableHDR = false,
                        enableRayTracing = false
                    },
                    performanceSettings = new PerformanceSettingsData
                    {
                        maxConcurrentCoins = 10,
                        objectPoolSize = 15,
                        memoryLimitMB = 100,
                        gpuTimeLimit = 30f,
                        enableAggressiveCleanup = true,
                        cleanupInterval = 3f,
                        enableDynamicQuality = true,
                        qualityAdjustmentThreshold = 25f,
                        adaptiveQualityEnabled = true,
                        qualityLevel = QualityLevel.Minimum
                    },
                    optimizationSettings = new OptimizationSettingsData
                    {
                        enableFrameLimiting = true,
                        frameLimit = 60,
                        enableLOD = true,
                        lodBias = 0.3f,
                        enableOcclusion = true,
                        enableFrustumCulling = true,
                        enableInstancing = true,
                        enableBatching = true,
                        enableAsyncUpload = false,
                        enableGPUInstancing = false
                    },
                    resourceSettings = new ResourceSettingsData
                    {
                        maxTextureSize = 512,
                        textureCompression = true,
                        audioQuality = 0.3f,
                        enableStreaming = false,
                        cacheSize = 25,
                        enableMipMaps = true,
                        enableAnisotropicFiltering = false
                    }
                }
            };
        }

        private DevicePreset CreateLowEndBalancedPreset()
        {
            return new DevicePreset
            {
                id = "low_end_balanced",
                name = "Low-End Balanced",
                description = "Balanced performance and quality for low-end devices",
                targetClass = DevicePerformanceClass.LowEnd,
                category = PresetCategory.Balanced,
                priority = 20,
                isBuiltIn = true,
                version = "1.0.0",
                createdAt = DateTime.UtcNow,
                settings = new DevicePresetSettings
                {
                    qualitySettings = new QualitySettingsData
                    {
                        qualityLevel = QualityLevel.Low,
                        targetFPS = 45,
                        maximumFPS = 60,
                        enableVSync = true,
                        antiAliasing = 0,
                        textureQuality = 0,
                        shadowQuality = ShadowQuality.HardOnly,
                        shadowDistance = 50f,
                        particleCount = 15,
                        enablePostProcessing = false,
                        enableHDR = false,
                        enableRayTracing = false
                    },
                    performanceSettings = new PerformanceSettingsData
                    {
                        maxConcurrentCoins = 20,
                        objectPoolSize = 25,
                        memoryLimitMB = 150,
                        gpuTimeLimit = 25f,
                        enableAggressiveCleanup = true,
                        cleanupInterval = 5f,
                        enableDynamicQuality = true,
                        qualityAdjustmentThreshold = 40f,
                        adaptiveQualityEnabled = true,
                        qualityLevel = QualityLevel.Low
                    },
                    optimizationSettings = new OptimizationSettingsData
                    {
                        enableFrameLimiting = true,
                        frameLimit = 60,
                        enableLOD = true,
                        lodBias = 0.5f,
                        enableOcclusion = true,
                        enableFrustumCulling = true,
                        enableInstancing = true,
                        enableBatching = true,
                        enableAsyncUpload = false,
                        enableGPUInstancing = false
                    },
                    resourceSettings = new ResourceSettingsData
                    {
                        maxTextureSize = 1024,
                        textureCompression = true,
                        audioQuality = 0.5f,
                        enableStreaming = false,
                        cacheSize = 40,
                        enableMipMaps = true,
                        enableAnisotropicFiltering = false
                    }
                }
            };
        }

        private DevicePreset CreateLowEndPerformancePreset()
        {
            return CreatePresetVariant("low_end_performance", "Low-End Performance",
                "Maximum FPS for competitive gameplay on low-end devices",
                DevicePerformanceClass.LowEnd, PresetCategory.Performance, 30);
        }

        private DevicePreset CreateMidRangeBalancedPreset()
        {
            return new DevicePreset
            {
                id = "mid_range_balanced",
                name = "Mid-Range Balanced",
                description = "Optimal balance between quality and performance",
                targetClass = DevicePerformanceClass.MidRange,
                category = PresetCategory.Balanced,
                priority = 50,
                isBuiltIn = true,
                version = "1.0.0",
                createdAt = DateTime.UtcNow,
                settings = new DevicePresetSettings
                {
                    qualitySettings = new QualitySettingsData
                    {
                        qualityLevel = QualityLevel.Medium,
                        targetFPS = 60,
                        maximumFPS = 120,
                        enableVSync = true,
                        antiAliasing = 2,
                        textureQuality = 1,
                        shadowQuality = ShadowQuality.All,
                        shadowDistance = 100f,
                        particleCount = 50,
                        enablePostProcessing = true,
                        enableHDR = false,
                        enableRayTracing = false
                    },
                    performanceSettings = new PerformanceSettingsData
                    {
                        maxConcurrentCoins = 50,
                        objectPoolSize = 75,
                        memoryLimitMB = 300,
                        gpuTimeLimit = 18f,
                        enableAggressiveCleanup = false,
                        cleanupInterval = 10f,
                        enableDynamicQuality = true,
                        qualityAdjustmentThreshold = 55f,
                        adaptiveQualityEnabled = true,
                        qualityLevel = QualityLevel.Medium
                    },
                    optimizationSettings = new OptimizationSettingsData
                    {
                        enableFrameLimiting = true,
                        frameLimit = 120,
                        enableLOD = true,
                        lodBias = 0.75f,
                        enableOcclusion = true,
                        enableFrustumCulling = true,
                        enableInstancing = true,
                        enableBatching = true,
                        enableAsyncUpload = true,
                        enableGPUInstancing = true
                    },
                    resourceSettings = new ResourceSettingsData
                    {
                        maxTextureSize = 2048,
                        textureCompression = true,
                        audioQuality = 0.7f,
                        enableStreaming = false,
                        cacheSize = 100,
                        enableMipMaps = true,
                        enableAnisotropicFiltering = true
                    }
                }
            };
        }

        private DevicePreset CreateMidRangeQualityPreset()
        {
            return CreatePresetVariant("mid_range_quality", "Mid-Range Quality",
                "Enhanced visual quality for mid-range devices",
                DevicePerformanceClass.MidRange, PresetCategory.Quality, 60);
        }

        private DevicePreset CreateMidRangePerformancePreset()
        {
            return CreatePresetVariant("mid_range_performance", "Mid-Range Performance",
                "Maximum performance for competitive mid-range devices",
                DevicePerformanceClass.MidRange, PresetCategory.Performance, 55);
        }

        private DevicePreset CreateMidRangeOptimalPreset()
        {
            return CreatePresetVariant("mid_range_optimal", "Mid-Range Optimal",
                "Smart optimization based on device capabilities",
                DevicePerformanceClass.MidRange, PresetCategory.Optimal, 70);
        }

        private DevicePreset CreateHighEndBalancedPreset()
        {
            return new DevicePreset
            {
                id = "high_end_balanced",
                name = "High-End Balanced",
                description = "Premium balanced experience for high-end devices",
                targetClass = DevicePerformanceClass.HighEnd,
                category = PresetCategory.Balanced,
                priority = 80,
                isBuiltIn = true,
                version = "1.0.0",
                createdAt = DateTime.UtcNow,
                settings = new DevicePresetSettings
                {
                    qualitySettings = new QualitySettingsData
                    {
                        qualityLevel = QualityLevel.High,
                        targetFPS = 60,
                        maximumFPS = 144,
                        enableVSync = false,
                        antiAliasing = 4,
                        textureQuality = 2,
                        shadowQuality = ShadowQuality.All,
                        shadowDistance = 200f,
                        particleCount = 100,
                        enablePostProcessing = true,
                        enableHDR = true,
                        enableRayTracing = false
                    },
                    performanceSettings = new PerformanceSettingsData
                    {
                        maxConcurrentCoins = 100,
                        objectPoolSize = 150,
                        memoryLimitMB = 600,
                        gpuTimeLimit = 12f,
                        enableAggressiveCleanup = false,
                        cleanupInterval = 15f,
                        enableDynamicQuality = false,
                        qualityAdjustmentThreshold = 120f,
                        adaptiveQualityEnabled = false,
                        qualityLevel = QualityLevel.High
                    },
                    optimizationSettings = new OptimizationSettingsData
                    {
                        enableFrameLimiting = false,
                        frameLimit = 0,
                        enableLOD = false,
                        lodBias = 1.0f,
                        enableOcclusion = true,
                        enableFrustumCulling = true,
                        enableInstancing = true,
                        enableBatching = true,
                        enableAsyncUpload = true,
                        enableGPUInstancing = true
                    },
                    resourceSettings = new ResourceSettingsData
                    {
                        maxTextureSize = 4096,
                        textureCompression = false,
                        audioQuality = 1.0f,
                        enableStreaming = true,
                        cacheSize = 200,
                        enableMipMaps = true,
                        enableAnisotropicFiltering = true
                    }
                }
            };
        }

        private DevicePreset CreateHighEndQualityPreset()
        {
            return CreatePresetVariant("high_end_quality", "High-End Quality",
                "Maximum visual quality for high-end devices",
                DevicePerformanceClass.HighEnd, PresetCategory.Quality, 90);
        }

        private DevicePreset CreateHighEndPerformancePreset()
        {
            return CreatePresetVariant("high_end_performance", "High-End Performance",
                "Maximum performance for competitive high-end devices",
                DevicePerformanceClass.HighEnd, PresetCategory.Performance, 85);
        }

        private DevicePreset CreateHighEndUltimatePreset()
        {
            return CreatePresetVariant("high_end_ultimate", "High-End Ultimate",
                "Ultimate experience with all features enabled",
                DevicePerformanceClass.HighEnd, PresetCategory.Ultimate, 95);
        }

        private DevicePreset CreateHighEndMaximumPreset()
        {
            return CreatePresetVariant("high_end_maximum", "High-End Maximum",
                "Maximum settings for demonstration and testing",
                DevicePerformanceClass.HighEnd, PresetCategory.Maximum, 100);
        }

        private DevicePreset CreateStreamingPreset()
        {
            return CreatePresetVariant("streaming_optimized", "Streaming Optimized",
                "Optimized for live streaming and recording",
                DevicePerformanceClass.Unknown, PresetCategory.Streaming, 60);
        }

        private DevicePreset CreateBatterySaverPreset()
        {
            return CreatePresetVariant("battery_saver", "Battery Saver",
                "Extended battery life with reduced power consumption",
                DevicePerformanceClass.Unknown, PresetCategory.Battery, 40);
        }

        private DevicePreset CreateShowcasePreset()
        {
            return CreatePresetVariant("showcase", "Showcase",
                "Best visual settings for product demonstration",
                DevicePerformanceClass.Unknown, PresetCategory.Showcase, 95);
        }

        private DevicePreset CreateTestingPreset()
        {
            return CreatePresetVariant("testing", "Testing",
                "Comprehensive testing settings for developers",
                DevicePerformanceClass.Unknown, PresetCategory.Testing, 50);
        }

        private DevicePreset CreatePresetVariant(string id, string name, string description,
            DevicePerformanceClass targetClass, PresetCategory category, int priority)
        {
            var baseSettings = GetBaseSettingsForClass(targetClass, category);

            return new DevicePreset
            {
                id = id,
                name = name,
                description = description,
                targetClass = targetClass,
                category = category,
                priority = priority,
                isBuiltIn = true,
                version = "1.0.0",
                createdAt = DateTime.UtcNow,
                settings = AdjustSettingsForCategory(baseSettings, category)
            };
        }

        private DevicePresetSettings GetBaseSettingsForClass(DevicePerformanceClass deviceClass, PresetCategory category)
        {
            switch (deviceClass)
            {
                case DevicePerformanceClass.LowEnd:
                    return CreateLowEndMinimalPreset().settings;
                case DevicePerformanceClass.MidRange:
                    return CreateMidRangeBalancedPreset().settings;
                case DevicePerformanceClass.HighEnd:
                    return CreateHighEndBalancedPreset().settings;
                default:
                    return CreateMidRangeBalancedPreset().settings;
            }
        }

        private DevicePresetSettings AdjustSettingsForCategory(DevicePresetSettings baseSettings, PresetCategory category)
        {
            var settings = baseSettings.Clone();

            switch (category)
            {
                case PresetCategory.Performance:
                    settings.qualitySettings.targetFPS = Mathf.RoundToInt(settings.qualitySettings.targetFPS * 1.2f);
                    settings.qualitySettings.enableVSync = false;
                    settings.performanceSettings.maxConcurrentCoins = Mathf.RoundToInt(settings.performanceSettings.maxConcurrentCoins * 1.3f);
                    break;

                case PresetCategory.Quality:
                    settings.qualitySettings.antiAliasing = Mathf.Min(settings.qualitySettings.antiAliasing + 2, 8);
                    settings.qualitySettings.shadowDistance = settings.qualitySettings.shadowDistance * 1.5f;
                    settings.qualitySettings.particleCount = Mathf.RoundToInt(settings.qualitySettings.particleCount * 1.5f);
                    break;

                case PresetCategory.Battery:
                    settings.qualitySettings.targetFPS = 30;
                    settings.qualitySettings.enableVSync = true;
                    settings.qualitySettings.antiAliasing = 0;
                    settings.qualitySettings.shadowQuality = ShadowQuality.Disable;
                    settings.optimizationSettings.enableFrameLimiting = true;
                    settings.optimizationSettings.frameLimit = 30;
                    break;

                case PresetCategory.Streaming:
                    settings.qualitySettings.targetFPS = 60;
                    settings.qualitySettings.enableVSync = true;
                    settings.qualitySettings.antiAliasing = 2;
                    settings.optimizationSettings.enableFrameLimiting = true;
                    settings.optimizationSettings.frameLimit = 60;
                    break;

                case PresetCategory.Showcase:
                    settings.qualitySettings.qualityLevel = QualityLevel.High;
                    settings.qualitySettings.enableHDR = true;
                    settings.qualitySettings.antiAliasing = 8;
                    settings.qualitySettings.shadowDistance = 300f;
                    settings.qualitySettings.particleCount = 200;
                    break;

                case PresetCategory.Testing:
                    settings.performanceSettings.enableDynamicQuality = false;
                    settings.performanceSettings.enableAggressiveCleanup = false;
                    break;
            }

            return settings;
        }

        #endregion

        #region Preset Categories

        private PresetCategories CreateDefaultPresetCategories()
        {
            return new PresetCategories
            {
                categories = new List<PresetCategoryDefinition>
                {
                    new PresetCategoryDefinition
                    {
                        id = PresetCategory.Balanced,
                        name = "Balanced",
                        description = "Optimal balance between quality and performance",
                        icon = "balance",
                        color = "#4CAF50"
                    },
                    new PresetCategoryDefinition
                    {
                        id = PresetCategory.Performance,
                        name = "Performance",
                        description = "Maximum FPS for competitive gaming",
                        icon = "speed",
                        color = "#FF9800"
                    },
                    new PresetCategoryDefinition
                    {
                        id = PresetCategory.Quality,
                        name = "Quality",
                        description = "Best visual quality and effects",
                        icon = "hd",
                        color = "#2196F3"
                    },
                    new PresetCategoryDefinition
                    {
                        id = PresetCategory.Battery,
                        name = "Battery Saver",
                        description = "Extended battery life optimization",
                        icon = "battery",
                        color = "#8BC34A"
                    },
                    new PresetCategoryDefinition
                    {
                        id = PresetCategory.Streaming,
                        name = "Streaming",
                        description = "Optimized for streaming and recording",
                        icon = "stream",
                        color = "#9C27B0"
                    },
                    new PresetCategoryDefinition
                    {
                        id = PresetCategory.Showcase,
                        name = "Showcase",
                        description = "Best settings for demonstration",
                        icon = "star",
                        color = "#E91E63"
                    },
                    new PresetCategoryDefinition
                    {
                        id = PresetCategory.Testing,
                        name = "Testing",
                        description = "Developer testing and debugging",
                        icon = "bug",
                        color = "#607D8B"
                    },
                    new PresetCategoryDefinition
                    {
                        id = PresetCategory.Optimal,
                        name = "Optimal",
                        description = "Smart optimization based on device",
                        icon = "auto",
                        color = "#00BCD4"
                    },
                    new PresetCategoryDefinition
                    {
                        id = PresetCategory.Ultimate,
                        name = "Ultimate",
                        description = "Maximum possible experience",
                        icon = "crown",
                        color = "#FFD700"
                    },
                    new PresetCategoryDefinition
                    {
                        id = PresetCategory.Maximum,
                        name = "Maximum",
                        description = "All settings at maximum values",
                        icon = "max",
                        color = "#FF5722"
                    }
                }
            };
        }

        #endregion

        #region Preset Management

        public void LoadAllPresets()
        {
            _presetsCache.Clear();
            _categorizedPresets.Clear();

            // 加载内置预设
            foreach (var preset in builtInPresets)
            {
                _presetsCache[preset.id] = preset;
                AddToCategorizedPresets(preset);
            }

            // 加载外部预设
            LoadExternalPresets();

            Debug.Log($"[DeviceProfilePresetsManager] Loaded {_presetsCache.Count} presets");
        }

        private void LoadExternalPresets()
        {
            if (!Directory.Exists(_presetsDirectoryPath)) return;

            var presetFiles = Directory.GetFiles(_presetsDirectoryPath, "*.json");

            foreach (var file in presetFiles)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var preset = JsonUtility.FromJson<DevicePreset>(json);

                    if (preset != null && !_presetsCache.ContainsKey(preset.id))
                    {
                        preset.isBuiltIn = false;
                        _presetsCache[preset.id] = preset;
                        AddToCategorizedPresets(preset);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[DeviceProfilePresetsManager] Failed to load preset from {file}: {e.Message}");
                }
            }
        }

        private void AddToCategorizedPresets(DevicePreset preset)
        {
            if (!_categorizedPresets.ContainsKey(preset.targetClass))
            {
                _categorizedPresets[preset.targetClass] = new List<DevicePreset>();
            }

            _categorizedPresets[preset.targetClass].Add(preset);
            _categorizedPresets[preset.targetClass].Sort((a, b) => a.priority.CompareTo(b.priority));
        }

        public List<DevicePreset> GetPresetsForDeviceClass(DevicePerformanceClass deviceClass)
        {
            if (_categorizedPresets.ContainsKey(deviceClass))
            {
                return _categorizedPresets[deviceClass];
            }

            return new List<DevicePreset>();
        }

        public List<DevicePreset> GetPresetsByCategory(PresetCategory category)
        {
            return _presetsCache.Values.Where(p => p.category == category).OrderBy(p => p.priority).ToList();
        }

        public DevicePreset GetPresetById(string presetId)
        {
            return _presetsCache.ContainsKey(presetId) ? _presetsCache[presetId] : null;
        }

        public DevicePreset GetRecommendedPreset(DevicePerformanceClass deviceClass, PresetCategory preferredCategory = PresetCategory.Balanced)
        {
            var presets = GetPresetsForDeviceClass(deviceClass);

            // 优先选择推荐类别的预设
            var preferredPresets = presets.Where(p => p.category == preferredCategory).ToList();
            if (preferredPresets.Count > 0)
            {
                return preferredPresets.OrderByDescending(p => p.priority).First();
            }

            // 如果没有找到推荐类别的预设，返回平衡类别
            var balancedPresets = presets.Where(p => p.category == PresetCategory.Balanced).ToList();
            if (balancedPresets.Count > 0)
            {
                return balancedPresets.OrderByDescending(p => p.priority).First();
            }

            // 最后返回优先级最高的预设
            return presets.OrderByDescending(p => p.priority).FirstOrDefault();
        }

        public IEnumerator CreateCustomPreset(string name, string description, DevicePerformanceClass targetClass,
            PresetCategory category, DevicePresetSettings settings)
        {
            var preset = new DevicePreset
            {
                id = GeneratePresetId(),
                name = name,
                description = description,
                targetClass = targetClass,
                category = category,
                priority = 50,
                isBuiltIn = false,
                version = "1.0.0",
                createdAt = DateTime.UtcNow,
                settings = settings
            };

            _presetsCache[preset.id] = preset;
            AddToCategorizedPresets(preset);

            yield return SavePreset(preset);

            OnPresetCreated?.Invoke(preset);
            Debug.Log($"[DeviceProfilePresetsManager] Custom preset created: {preset.name}");
        }

        public IEnumerator SavePreset(DevicePreset preset)
        {
            try
            {
                var presetPath = Path.Combine(_presetsDirectoryPath, $"{preset.id}.json");
                var json = JsonUtility.ToJson(preset, true);

                File.WriteAllText(presetPath, json);
                Debug.Log($"[DeviceProfilePresetsManager] Preset saved: {preset.name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DeviceProfilePresetsManager] Error saving preset: {e.Message}");
                OnPresetError?.Invoke($"Error saving preset: {e.Message}");
            }

            yield return null;
        }

        public void DeletePreset(string presetId)
        {
            if (_presetsCache.ContainsKey(presetId))
            {
                var preset = _presetsCache[presetId];

                if (!preset.isBuiltIn)
                {
                    var presetPath = Path.Combine(_presetsDirectoryPath, $"{presetId}.json");
                    if (File.Exists(presetPath))
                    {
                        File.Delete(presetPath);
                    }
                }

                _presetsCache.Remove(presetId);

                // 从分类中移除
                foreach (var categoryList in _categorizedPresets.Values)
                {
                    categoryList.RemoveAll(p => p.id == presetId);
                }

                Debug.Log($"[DeviceProfilePresetsManager] Preset deleted: {preset.name}");
            }
        }

        public IEnumerator ExportPreset(string presetId, string exportPath)
        {
            var preset = GetPresetById(presetId);
            if (preset == null)
            {
                Debug.LogWarning($"[DeviceProfilePresetsManager] Preset not found: {presetId}");
                yield break;
            }

            try
            {
                var exportData = new PresetExportData
                {
                    preset = preset,
                    exportedAt = DateTime.UtcNow,
                    exportVersion = "1.0",
                    metadata = new PresetExportMetadata
                    {
                        sourceDevice = SystemInfo.deviceModel,
                        exportReason = "Manual Export",
                        compatibleClasses = new List<DevicePerformanceClass> { preset.targetClass }
                    }
                };

                var json = JsonUtility.ToJson(exportData, true);
                File.WriteAllText(exportPath, json);

                Debug.Log($"[DeviceProfilePresetsManager] Preset exported to: {exportPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DeviceProfilePresetsManager] Error exporting preset: {e.Message}");
                OnPresetError?.Invoke($"Error exporting preset: {e.Message}");
            }

            yield return null;
        }

        public IEnumerator ImportPreset(string importPath)
        {
            try
            {
                if (!File.Exists(importPath))
                {
                    Debug.LogWarning($"[DeviceProfilePresetsManager] Import file not found: {importPath}");
                    yield break;
                }

                var json = File.ReadAllText(importPath);
                var exportData = JsonUtility.FromJson<PresetExportData>(json);

                if (exportData?.preset == null)
                {
                    Debug.LogWarning($"[DeviceProfilePresetsManager] Invalid preset export file: {importPath}");
                    yield break;
                }

                var preset = exportData.preset;
                preset.id = GeneratePresetId(); // 生成新ID避免冲突
                preset.isBuiltIn = false;
                preset.createdAt = DateTime.UtcNow;

                _presetsCache[preset.id] = preset;
                AddToCategorizedPresets(preset);

                yield return SavePreset(preset);

                Debug.Log($"[DeviceProfilePresetsManager] Preset imported: {preset.name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DeviceProfilePresetsManager] Error importing preset: {e.Message}");
                OnPresetError?.Invoke($"Error importing preset: {e.Message}");
            }

            yield return null;
        }

        private string GeneratePresetId()
        {
            return $"custom_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }

        #endregion

        #region Utility Methods

        public List<PresetCategoryDefinition> GetAvailableCategories()
        {
            return presetCategories.categories;
        }

        public PresetCompatibility CheckPresetCompatibility(DevicePreset preset, DeviceCapabilities deviceCapabilities)
        {
            var compatibility = new PresetCompatibility
            {
                presetId = preset.id,
                presetName = preset.name,
                isCompatible = true,
                compatibilityScore = 100f,
                warnings = new List<string>(),
                recommendations = new List<string>()
            };

            // 检查设备类别兼容性
            if (preset.targetClass != DevicePerformanceClass.Unknown &&
                preset.targetClass != deviceCapabilities.PerformanceTier)
            {
                compatibility.warnings.Add($"Preset is designed for {preset.targetClass} but device is {deviceCapabilities.PerformanceTier}");
                compatibility.compatibilityScore -= 20f;
            }

            // 检查硬件要求
            if (preset.settings.qualitySettings.enableRayTracing && deviceCapabilities.GPUScore < 90f)
            {
                compatibility.warnings.Add("Ray tracing may not be supported by this device");
                compatibility.compatibilityScore -= 15f;
            }

            if (preset.settings.qualitySettings.enableHDR && deviceCapabilities.GPUScore < 70f)
            {
                compatibility.warnings.Add("HDR may not be fully supported by this device");
                compatibility.compatibilityScore -= 10f;
            }

            if (preset.settings.qualitySettings.targetFPS > 60 && deviceCapabilities.CPUScore < 60f)
            {
                compatibility.warnings.Add("Target FPS may be difficult to maintain with this CPU");
                compatibility.compatibilityScore -= 10f;
            }

            if (preset.settings.performanceSettings.maxConcurrentCoins > 100 && deviceCapabilities.MemoryScore < 80f)
            {
                compatibility.warnings.Add("High coin count may cause memory issues");
                compatibility.compatibilityScore -= 10f;
            }

            // 检查资源要求
            if (preset.settings.resourceSettings.maxTextureSize > SystemInfo.maxTextureSize)
            {
                compatibility.warnings.Add($"Texture size ({preset.settings.resourceSettings.maxTextureSize}) exceeds device limit ({SystemInfo.maxTextureSize})");
                compatibility.compatibilityScore -= 20f;
            }

            // 生成建议
            if (compatibility.compatibilityScore < 80f)
            {
                compatibility.recommendations.Add("Consider using a preset designed for your device class");
                compatibility.recommendations.Add("Enable dynamic quality adjustment for better performance");
            }

            if (compatibility.compatibilityScore >= 80f && compatibility.compatibilityScore < 100f)
            {
                compatibility.recommendations.Add("Preset should work well with minor adjustments");
                compatibility.recommendations.Add("Monitor performance after applying preset");
            }

            return compatibility;
        }

        public List<DevicePreset> GetCompatiblePresets(DeviceCapabilities deviceCapabilities)
        {
            var compatiblePresets = new List<DevicePreset>();

            foreach (var preset in _presetsCache.Values)
            {
                var compatibility = CheckPresetCompatibility(preset, deviceCapabilities);
                if (compatibility.isCompatible && compatibility.compatibilityScore >= 70f)
                {
                    compatiblePresets.Add(preset);
                }
            }

            return compatiblePresets.OrderByDescending(p =>
                CheckPresetCompatibility(p, deviceCapabilities).compatibilityScore).ToList();
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class DevicePreset
    {
        public string id;
        public string name;
        public string description;
        public DevicePerformanceClass targetClass;
        public PresetCategory category;
        public int priority;
        public bool isBuiltIn;
        public string version;
        public DateTime createdAt;
        public DevicePresetSettings settings;
    }

    [System.Serializable]
    public class DevicePresetSettings
    {
        public QualitySettingsData qualitySettings;
        public PerformanceSettingsData performanceSettings;
        public OptimizationSettingsData optimizationSettings;
        public ResourceSettingsData resourceSettings;

        public DevicePresetSettings Clone()
        {
            return JsonUtility.FromJson<DevicePresetSettings>(JsonUtility.ToJson(this));
        }
    }

    [System.Serializable]
    public class PresetCategories
    {
        public List<PresetCategoryDefinition> categories = new List<PresetCategoryDefinition>();
    }

    [System.Serializable]
    public class PresetCategoryDefinition
    {
        public PresetCategory id;
        public string name;
        public string description;
        public string icon;
        public string color;
    }

    [System.Serializable]
    public class PresetExportData
    {
        public DevicePreset preset;
        public DateTime exportedAt;
        public string exportVersion;
        public PresetExportMetadata metadata;
    }

    [System.Serializable]
    public class PresetExportMetadata
    {
        public string sourceDevice;
        public string exportReason;
        public List<DevicePerformanceClass> compatibleClasses;
    }

    [System.Serializable]
    public class PresetCompatibility
    {
        public string presetId;
        public string presetName;
        public bool isCompatible;
        public float compatibilityScore;
        public List<string> warnings;
        public List<string> recommendations;
    }

    public enum PresetCategory
    {
        Balanced,
        Performance,
        Quality,
        Battery,
        Streaming,
        Showcase,
        Testing,
        Optimal,
        Ultimate,
        Maximum
    }

    #endregion
}