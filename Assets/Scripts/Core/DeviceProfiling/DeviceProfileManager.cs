using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using CoinAnimation.Core;
using CoinAnimation.AdaptiveQuality;

namespace CoinAnimation.Core.DeviceProfiling
{
    /// <summary>
    /// 设备配置文件管理器
    /// Story 2.1 Task 5.4 - 创建设备特定性能配置文件和预设
    /// </summary>
    public class DeviceProfileManager : MonoBehaviour
    {
        #region Configuration

        [Header("Profile Management")]
        [SerializeField] private bool enableProfileManagement = true;
        [SerializeField] private bool autoLoadProfile = true;
        [SerializeField] private bool autoSaveProfile = true;
        [SerializeField] private string profileDirectoryName = "DeviceProfiles";

        [Header("Profile Creation")]
        [SerializeField] private bool enableAutoProfiling = true;
        [SerializeField] private bool profileValidation = true;
        [SerializeField] private float profileValidationDuration = 30f;

        [Header("Default Profiles")]
        [SerializeField] private List<DevicePerformanceProfile> defaultProfiles = new List<DevicePerformanceProfile>();

        [Header("Profile Presets")]
        [SerializeField] private DeviceProfilePresets profilePresets;

        #endregion

        #region Private Fields

        private DeviceCapabilityDetector _deviceDetector;
        private LowEndDeviceTester _lowEndTester;
        private MidRangeDeviceValidator _midRangeValidator;
        private HighEndDeviceValidator _highEndValidator;
        private AdaptiveQualityManager _qualityManager;

        private DevicePerformanceProfile _currentProfile;
        private Dictionary<string, DevicePerformanceProfile> _profileCache = new Dictionary<string, DevicePerformanceProfile>();
        private Dictionary<DevicePerformanceClass, DeviceProfileTemplate> _profileTemplates = new Dictionary<DevicePerformanceClass, DeviceProfileTemplate>();

        private string _profileDirectoryPath;
        private bool _isProfileLoading = false;
        private bool _isProfileCreating = false;

        #endregion

        #region Events

        public event Action<DevicePerformanceProfile> OnProfileLoaded;
        public event Action<DevicePerformanceProfile> OnProfileCreated;
        public event Action<DevicePerformanceProfile> OnProfileUpdated;
        public event Action<string> OnProfileError;
        public event Action<ProfileValidationResult> OnProfileValidated;

        #endregion

        #region Properties

        public DevicePerformanceProfile CurrentProfile => _currentProfile;
        public bool IsProfileLoading => _isProfileLoading;
        public bool IsProfileCreating => _isProfileCreating;
        public string ProfileDirectory => _profileDirectoryPath;
        public Dictionary<DevicePerformanceClass, DeviceProfileTemplate> ProfileTemplates => _profileTemplates;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            SetupProfileDirectory();
            InitializeProfileTemplates();
            LoadDefaultProfiles();
        }

        private void Start()
        {
            if (autoLoadProfile)
            {
                StartCoroutine(AutoLoadProfileCoroutine());
            }
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            _deviceDetector = FindObjectOfType<DeviceCapabilityDetector>();
            _lowEndTester = FindObjectOfType<LowEndDeviceTester>();
            _midRangeValidator = FindObjectOfType<MidRangeDeviceValidator>();
            _highEndValidator = FindObjectOfType<HighEndDeviceValidator>();
            _qualityManager = FindObjectOfType<AdaptiveQualityManager>();
        }

        private void SetupProfileDirectory()
        {
            _profileDirectoryPath = Path.Combine(Application.persistentDataPath, profileDirectoryName);

            if (!Directory.Exists(_profileDirectoryPath))
            {
                Directory.CreateDirectory(_profileDirectoryPath);
                UnityEngine.Debug.Log($"[DeviceProfileManager] Created profile directory: {_profileDirectoryPath}");
            }
        }

        private void InitializeProfileTemplates()
        {
            _profileTemplates[DevicePerformanceClass.LowEnd] = CreateLowEndTemplate();
            _profileTemplates[DevicePerformanceClass.MidRange] = CreateMidRangeTemplate();
            _profileTemplates[DevicePerformanceClass.HighEnd] = CreateHighEndTemplate();
            _profileTemplates[DevicePerformanceClass.Unknown] = CreateDefaultTemplate();
        }

        private void LoadDefaultProfiles()
        {
            if (defaultProfiles.Count == 0)
            {
                defaultProfiles = CreateDefaultDeviceProfiles();
            }

            foreach (var profile in defaultProfiles)
            {
                _profileCache[profile.deviceId] = profile;
            }
        }

        #endregion

        #region Profile Templates

        private DeviceProfileTemplate CreateLowEndTemplate()
        {
            return new DeviceProfileTemplate
            {
                deviceClass = DevicePerformanceClass.LowEnd,
                name = "Low-End Device Template",
                description = "Optimized settings for low-end devices",
                qualitySettings = new QualitySettingsTemplate
                {
                    qualityLevel = QualityLevel.Low,
                    targetFPS = 45,
                    maximumFPS = 60,
                    enableVSync = true,
                    antiAliasing = 0,
                    textureQuality = 0,
                    shadowQuality = ShadowQuality.HardOnly,
                    shadowDistance = 50f,
                    particleCount = 20,
                    enablePostProcessing = false
                },
                performanceSettings = new PerformanceSettingsTemplate
                {
                    maxConcurrentCoins = 20,
                    objectPoolSize = 25,
                    memoryLimitMB = 150,
                    gpuTimeLimit = 20f,
                    enableAggressiveCleanup = true,
                    cleanupInterval = 5f,
                    enableDynamicQuality = true,
                    qualityAdjustmentThreshold = 50f
                },
                optimizationSettings = new OptimizationSettingsTemplate
                {
                    enableFrameLimiting = true,
                    frameLimit = 60,
                    enableLOD = true,
                    lodBias = 0.5f,
                    enableOcclusion = true,
                    enableFrustumCulling = true,
                    enableInstancing = true,
                    enableBatching = true
                },
                resourceSettings = new ResourceSettingsTemplate
                {
                    maxTextureSize = 1024,
                    textureCompression = true,
                    audioQuality = 0.5f,
                    enableStreaming = false,
                    cacheSize = 50
                }
            };
        }

        private DeviceProfileTemplate CreateMidRangeTemplate()
        {
            return new DeviceProfileTemplate
            {
                deviceClass = DevicePerformanceClass.MidRange,
                name = "Mid-Range Device Template",
                description = "Balanced settings for mid-range devices",
                qualitySettings = new QualitySettingsTemplate
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
                    enablePostProcessing = true
                },
                performanceSettings = new PerformanceSettingsTemplate
                {
                    maxConcurrentCoins = 50,
                    objectPoolSize = 75,
                    memoryLimitMB = 300,
                    gpuTimeLimit = 15f,
                    enableAggressiveCleanup = false,
                    cleanupInterval = 10f,
                    enableDynamicQuality = true,
                    qualityAdjustmentThreshold = 55f
                },
                optimizationSettings = new OptimizationSettingsTemplate
                {
                    enableFrameLimiting = true,
                    frameLimit = 120,
                    enableLOD = true,
                    lodBias = 0.75f,
                    enableOcclusion = true,
                    enableFrustumCulling = true,
                    enableInstancing = true,
                    enableBatching = true
                },
                resourceSettings = new ResourceSettingsTemplate
                {
                    maxTextureSize = 2048,
                    textureCompression = true,
                    audioQuality = 0.7f,
                    enableStreaming = false,
                    cacheSize = 100
                }
            };
        }

        private DeviceProfileTemplate CreateHighEndTemplate()
        {
            return new DeviceProfileTemplate
            {
                deviceClass = DevicePerformanceClass.HighEnd,
                name = "High-End Device Template",
                description = "Maximum quality settings for high-end devices",
                qualitySettings = new QualitySettingsTemplate
                {
                    qualityLevel = QualityLevel.High,
                    targetFPS = 60,
                    maximumFPS = 240,
                    enableVSync = false,
                    antiAliasing = 8,
                    textureQuality = 2,
                    shadowQuality = ShadowQuality.All,
                    shadowDistance = 200f,
                    particleCount = 150,
                    enablePostProcessing = true
                },
                performanceSettings = new PerformanceSettingsTemplate
                {
                    maxConcurrentCoins = 100,
                    objectPoolSize = 150,
                    memoryLimitMB = 800,
                    gpuTimeLimit = 10f,
                    enableAggressiveCleanup = false,
                    cleanupInterval = 15f,
                    enableDynamicQuality = false,
                    qualityAdjustmentThreshold = 120f
                },
                optimizationSettings = new OptimizationSettingsTemplate
                {
                    enableFrameLimiting = false,
                    frameLimit = 0,
                    enableLOD = false,
                    lodBias = 1.0f,
                    enableOcclusion = true,
                    enableFrustumCulling = true,
                    enableInstancing = true,
                    enableBatching = true
                },
                resourceSettings = new ResourceSettingsTemplate
                {
                    maxTextureSize = 4096,
                    textureCompression = false,
                    audioQuality = 1.0f,
                    enableStreaming = true,
                    cacheSize = 200
                }
            };
        }

        private DeviceProfileTemplate CreateDefaultTemplate()
        {
            return CreateMidRangeTemplate(); // 默认使用中端配置
        }

        #endregion

        #region Profile Creation and Management

        public IEnumerator CreateProfileCoroutine()
        {
            if (_isProfileCreating)
            {
                UnityEngine.Debug.LogWarning("[DeviceProfileManager] Profile creation already in progress");
                yield break;
            }

            _isProfileCreating = true;
            Exception caughtException = null;

            try
            {
                UnityEngine.Debug.Log("[DeviceProfileManager] Starting device profile creation");
            }
            catch (Exception e)
            {
                caughtException = e;
            }

            if (caughtException == null)
            {
                // 等待设备检测完成
                yield return WaitForDeviceDetection();

                try
                {
                    // 创建设备配置文件
                    DevicePerformanceProfile profile = null;
                    yield return StartCoroutine(CreateDeviceProfileWithCallback(result => {
                        profile = result;
                    }));

                    if (profile != null)
                    {
                        // 验证配置文件
                        if (profileValidation)
                        {
                            ProfileValidationResult validationResult = null;
                            yield return StartCoroutine(ValidateProfileWithCallback(profile, result => {
                                validationResult = result;
                            }));
                            OnProfileValidated?.Invoke(validationResult);

                            if (!validationResult.isValid)
                            {
                                UnityEngine.Debug.LogWarning($"[DeviceProfileManager] Profile validation failed: {validationResult.errorMessage}");
                                OnProfileError?.Invoke($"Profile validation failed: {validationResult.errorMessage}");
                                _isProfileCreating = false;
                                yield break;
                            }
                        }

                        // 应用配置文件
                        _currentProfile = profile;
                        yield return ApplyProfile(profile);

                        // 保存配置文件
                        if (autoSaveProfile)
                        {
                            yield return SaveProfile(profile);
                        }

                        OnProfileCreated?.Invoke(profile);
                        UnityEngine.Debug.Log($"[DeviceProfileManager] Profile created successfully: {profile.profileName}");
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("[DeviceProfileManager] Failed to create device profile");
                        OnProfileError?.Invoke("Failed to create device profile");
                    }
                }
                catch (Exception e)
                {
                    caughtException = e;
                }
            }

            if (caughtException != null)
            {
                UnityEngine.Debug.LogError($"[DeviceProfileManager] Profile creation error: {caughtException.Message}");
                OnProfileError?.Invoke($"Profile creation error: {caughtException.Message}");
            }

            _isProfileCreating = false;
        }

        public void CreateProfileAsync()
        {
            StartCoroutine(CreateProfileCoroutine());
        }

        private IEnumerator WaitForDeviceDetection()
        {
            var timeout = 10f;
            var elapsed = 0f;

            while (_deviceDetector == null || _deviceDetector.DetectedCapabilities == null && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.5f);
                elapsed += 0.5f;
            }

            if (_deviceDetector?.DetectedCapabilities == null)
            {
                UnityEngine.Debug.LogWarning("[DeviceProfileManager] Device detection timeout or failed");
            }
        }

        private IEnumerator CreateDeviceProfile()
        {
            var capabilities = _deviceDetector?.DetectedCapabilities;
            if (capabilities == null)
            {
                UnityEngine.Debug.LogError("[DeviceProfileManager] No device capabilities available");
                yield break;
            }

            var deviceClass = capabilities.PerformanceTier;
            var template = _profileTemplates.ContainsKey(deviceClass) ? _profileTemplates[deviceClass] : _profileTemplates[DevicePerformanceClass.Unknown];

            var profile = new DevicePerformanceProfile
            {
                deviceId = GenerateDeviceId(),
                deviceName = SystemInfo.deviceModel,
                deviceClass = deviceClass,
                profileName = $"{deviceClass} Profile - {SystemInfo.deviceModel}",
                description = $"Auto-generated profile for {SystemInfo.deviceModel}",
                createdAt = DateTime.UtcNow,
                lastUpdated = DateTime.UtcNow,
                version = "1.0.0",
                deviceCapabilities = capabilities,
                qualitySettings = CreateQualitySettingsFromTemplate(template.qualitySettings, capabilities),
                performanceSettings = CreatePerformanceSettingsFromTemplate(template.performanceSettings, capabilities),
                optimizationSettings = CreateOptimizationSettingsFromTemplate(template.optimizationSettings, capabilities),
                resourceSettings = CreateResourceSettingsFromTemplate(template.resourceSettings, capabilities)
            };

            // 收集实际性能数据
            yield return CollectPerformanceData(profile);

            // 根据实际数据调整配置
            AdjustProfileBasedOnPerformanceData(profile);

            yield return profile;
        }

        private string GenerateDeviceId()
        {
            var deviceInfo = $"{SystemInfo.deviceModel}_{SystemInfo.processorType}_{SystemInfo.graphicsDeviceName}";
            return System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(deviceInfo)).Take(8).ToArray().ToHexString();
        }

        private QualitySettingsData CreateQualitySettingsFromTemplate(QualitySettingsTemplate template, DeviceCapabilities capabilities)
        {
            return new QualitySettingsData
            {
                qualityLevel = template.qualityLevel,
                targetFPS = template.targetFPS,
                maximumFPS = template.maximumFPS,
                enableVSync = template.enableVSync,
                antiAliasing = template.antiAliasing,
                textureQuality = template.textureQuality,
                shadowQuality = template.shadowQuality,
                shadowDistance = template.shadowDistance,
                particleCount = template.particleCount,
                enablePostProcessing = template.enablePostProcessing,
                enableHDR = capabilities.GPUScore > 80f, // 高端GPU启用HDR
                enableRayTracing = capabilities.GPUScore > 90f && _supportsRayTracing() // 支持RT的高端GPU
            };
        }

        private PerformanceSettingsData CreatePerformanceSettingsFromTemplate(PerformanceSettingsTemplate template, DeviceCapabilities capabilities)
        {
            // 根据设备能力调整性能设置
            var adjustedMaxCoins = template.maxConcurrentCoins;
            var adjustedMemoryLimit = template.memoryLimitMB;

            // 高端设备可以处理更多金币
            if (capabilities.PerformanceTier == DevicePerformanceClass.HighEnd)
            {
                adjustedMaxCoins = Mathf.RoundToInt(template.maxConcurrentCoins * 1.5f);
                adjustedMemoryLimit = Mathf.RoundToInt(template.memoryLimitMB * 1.3f);
            }
            // 低端设备需要更保守的设置
            else if (capabilities.PerformanceTier == DevicePerformanceClass.LowEnd)
            {
                adjustedMaxCoins = Mathf.RoundToInt(template.maxConcurrentCoins * 0.8f);
                adjustedMemoryLimit = Mathf.RoundToInt(template.memoryLimitMB * 0.7f);
            }

            return new PerformanceSettingsData
            {
                maxConcurrentCoins = adjustedMaxCoins,
                objectPoolSize = template.objectPoolSize,
                memoryLimitMB = adjustedMemoryLimit,
                gpuTimeLimit = template.gpuTimeLimit,
                enableAggressiveCleanup = template.enableAggressiveCleanup,
                cleanupInterval = template.cleanupInterval,
                enableDynamicQuality = template.enableDynamicQuality,
                qualityAdjustmentThreshold = template.qualityAdjustmentThreshold,
                adaptiveQualityEnabled = capabilities.PerformanceTier != DevicePerformanceClass.HighEnd // 高端设备通常不需要动态质量调整
            };
        }

        private OptimizationSettingsData CreateOptimizationSettingsFromTemplate(OptimizationSettingsTemplate template, DeviceCapabilities capabilities)
        {
            return new OptimizationSettingsData
            {
                enableFrameLimiting = template.enableFrameLimiting,
                frameLimit = template.frameLimit,
                enableLOD = template.enableLOD,
                lodBias = template.lodBias,
                enableOcclusion = template.enableOcclusion,
                enableFrustumCulling = template.enableFrustumCulling,
                enableInstancing = template.enableInstancing,
                enableBatching = template.enableBatching,
                enableAsyncUpload = SystemInfo.graphicsMemorySize > 2048, // 2GB+显存启用异步上传
                enableGPUInstancing = SystemInfo.supportsInstancing
            };
        }

        private ResourceSettingsData CreateResourceSettingsFromTemplate(ResourceSettingsTemplate template, DeviceCapabilities capabilities)
        {
            var adjustedTextureSize = template.maxTextureSize;
            var adjustedCacheSize = template.cacheSize;

            // 根据显存大小调整
            if (SystemInfo.graphicsMemorySize > 4096) // 4GB+
            {
                adjustedTextureSize = Mathf.Min(8192, template.maxTextureSize * 2);
                adjustedCacheSize = Mathf.RoundToInt(template.cacheSize * 1.5f);
            }
            else if (SystemInfo.graphicsMemorySize < 1024) // 1GB以下
            {
                adjustedTextureSize = Mathf.Max(512, template.maxTextureSize / 2);
                adjustedCacheSize = Mathf.Max(25, template.cacheSize / 2);
            }

            return new ResourceSettingsData
            {
                maxTextureSize = adjustedTextureSize,
                textureCompression = template.textureCompression,
                audioQuality = template.audioQuality,
                enableStreaming = template.enableStreaming && SystemInfo.systemMemorySize > 8192, // 8GB+内存启用流式加载
                cacheSize = adjustedCacheSize,
                enableMipMaps = true,
                enableAnisotropicFiltering = SystemInfo.graphicsMemorySize > 2048
            };
        }

        private IEnumerator CollectPerformanceData(DevicePerformanceProfile profile)
        {
            UnityEngine.Debug.Log("[DeviceProfileManager] Collecting performance data for profile");

            var dataCollector = gameObject.AddComponent<ProfileDataCollector>();
            PerformanceData performanceData = null;
            yield return StartCoroutine(CollectDataWithCallback(dataCollector, profileValidationDuration, result => {
                performanceData = result;
            }));
            DestroyImmediate(dataCollector);

            if (performanceData != null)
            {
                profile.performanceData = performanceData;
                UnityEngine.Debug.Log($"[DeviceProfileManager] Performance data collected - Avg FPS: {performanceData.averageFPS:F1}, Memory: {performanceData.averageMemoryUsage:F1}MB");
            }
        }

        private void AdjustProfileBasedOnPerformanceData(DevicePerformanceProfile profile)
        {
            if (profile.performanceData == null) return;

            var data = profile.performanceData;

            // 根据实际FPS调整设置
            if (data.averageFPS < profile.qualitySettings.targetFPS * 0.8f)
            {
                // 性能低于目标，降低质量
                profile.qualitySettings.qualityLevel = (QualityLevel)Mathf.Max((int)profile.qualitySettings.qualityLevel - 1, 0);
                profile.performanceSettings.maxConcurrentCoins = Mathf.RoundToInt(profile.performanceSettings.maxConcurrentCoins * 0.8f);
                UnityEngine.Debug.Log("[DeviceProfileManager] Adjusted profile settings downward due to performance constraints");
            }
            else if (data.averageFPS > profile.qualitySettings.targetFPS * 1.2f)
            {
                // 性能充裕，可以提升质量
                if (profile.qualitySettings.qualityLevel < QualityLevel.High)
                {
                    profile.qualitySettings.qualityLevel = (QualityLevel)Mathf.Min((int)profile.qualitySettings.qualityLevel + 1, 2);
                    profile.performanceSettings.maxConcurrentCoins = Mathf.RoundToInt(profile.performanceSettings.maxConcurrentCoins * 1.1f);
                    UnityEngine.Debug.Log("[DeviceProfileManager] Adjusted profile settings upward due to good performance");
                }
            }

            // 根据内存使用调整设置
            if (data.averageMemoryUsage > profile.performanceSettings.memoryLimitMB * 0.9f)
            {
                profile.performanceSettings.enableAggressiveCleanup = true;
                profile.performanceSettings.cleanupInterval = Mathf.Min(profile.performanceSettings.cleanupInterval * 0.5f, 2f);
                UnityEngine.Debug.Log("[DeviceProfileManager] Enabled aggressive cleanup due to memory pressure");
            }
        }

        private bool _supportsRayTracing()
        {
            // 简化的光线追踪支持检测
            return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12 &&
                   SystemInfo.graphicsMemorySize >= 4096 &&
                   SystemInfo.graphicsShaderLevel >= 50;
        }

        #endregion

        #region Profile Validation

        private IEnumerator ValidateProfile(DevicePerformanceProfile profile)
        {
            UnityEngine.Debug.Log($"[DeviceProfileManager] Validating profile: {profile.profileName}");

            var result = new ProfileValidationResult
            {
                profileId = profile.deviceId,
                profileName = profile.profileName,
                isValid = true,
                warnings = new List<string>(),
                errors = new List<string>()
            };

            // 基本验证
            if (profile.deviceId == null || profile.deviceId.Length == 0)
            {
                result.errors.Add("Device ID is required");
                result.isValid = false;
            }

            if (profile.qualitySettings.targetFPS <= 0)
            {
                result.errors.Add("Target FPS must be greater than 0");
                result.isValid = false;
            }

            if (profile.performanceSettings.maxConcurrentCoins <= 0)
            {
                result.errors.Add("Max concurrent coins must be greater than 0");
                result.isValid = false;
            }

            // 性能验证
            yield return StartCoroutine(ValidateProfilePerformance(profile, result));

            // 兼容性验证
            ValidateProfileCompatibility(profile, result);

            OnProfileValidated?.Invoke(result);
            yield return result;
        }

        
        private IEnumerator ValidateProfilePerformance(DevicePerformanceProfile profile, ProfileValidationResult result)
        {
            // 应用配置文件进行测试
            var originalSettings = CaptureCurrentSettings();
            yield return ApplyProfile(profile);

            // 运行性能测试
            var testDuration = 10f;
            var startTime = Time.time;
            var fpsReadings = new List<float>();
            var memoryReadings = new List<float>();

            while (Time.time - startTime < testDuration)
            {
                var fps = 1f / Time.unscaledDeltaTime;
                var memory = GC.GetTotalMemory(false) / (1024f * 1024f);

                fpsReadings.Add(fps);
                memoryReadings.Add(memory);

                yield return null;
            }

            var averageFPS = fpsReadings.Average();
            var averageMemory = memoryReadings.Average();

            // 验证性能指标
            if (averageFPS < profile.qualitySettings.targetFPS * 0.7f)
            {
                result.warnings.Add($"Average FPS ({averageFPS:F1}) is significantly below target ({profile.qualitySettings.targetFPS})");
            }

            if (averageMemory > profile.performanceSettings.memoryLimitMB * 1.2f)
            {
                result.warnings.Add($"Average memory usage ({averageMemory:F1}MB) exceeds limit ({profile.performanceSettings.memoryLimitMB}MB)");
            }

            // 恢复原始设置
            yield return RestoreSettings(originalSettings);

            Debug.Log($"[DeviceProfileManager] Profile validation completed - FPS: {averageFPS:F1}, Memory: {averageMemory:F1}MB");
        }

        private void ValidateProfileCompatibility(DevicePerformanceProfile profile, ProfileValidationResult result)
        {
            // 验证设备兼容性
            if (profile.qualitySettings.enableRayTracing && !_supportsRayTracing())
            {
                result.warnings.Add("Ray tracing enabled but device may not support it");
            }

            if (profile.qualitySettings.enableHDR && SystemInfo.graphicsDeviceVersion.Contains("OpenGL"))
            {
                result.warnings.Add("HDR enabled but OpenGL may have limited HDR support");
            }

            if (profile.resourceSettings.maxTextureSize > SystemInfo.maxTextureSize)
            {
                result.warnings.Add($"Max texture size ({profile.resourceSettings.maxTextureSize}) exceeds device limit ({SystemInfo.maxTextureSize})");
            }

            if (profile.qualitySettings.antiAliasing > 8 && !SystemInfo.supportsMultisampledTextures)
            {
                result.warnings.Add("High anti-aliasing setting may not be supported");
            }
        }

        private SettingsSnapshot CaptureCurrentSettings()
        {
            return new SettingsSnapshot
            {
                qualityLevel = QualitySettings.GetQualityLevel(),
                vSyncCount = QualitySettings.vSyncCount,
                antiAliasing = QualitySettings.antiAliasing,
                shadowQuality = QualitySettings.shadows,
                shadowDistance = QualitySettings.shadowDistance,
                targetFrameRate = Application.targetFrameRate
            };
        }

        private IEnumerator RestoreSettings(SettingsSnapshot settings)
        {
            QualitySettings.SetQualityLevel(settings.qualityLevel, true);
            QualitySettings.vSyncCount = settings.vSyncCount;
            QualitySettings.antiAliasing = settings.antiAliasing;
            QualitySettings.shadows = settings.shadowQuality;
            QualitySettings.shadowDistance = settings.shadowDistance;
            Application.targetFrameRate = settings.targetFrameRate;

            yield return new WaitForSeconds(0.5f); // 等待设置生效
        }

        #endregion

        #region Profile Application

        private IEnumerator ApplyProfile(DevicePerformanceProfile profile)
        {
            Debug.Log($"[DeviceProfileManager] Applying profile: {profile.profileName}");

            // 应用质量设置
            yield return ApplyQualitySettings(profile.qualitySettings);

            // 应用性能设置
            yield return ApplyPerformanceSettings(profile.performanceSettings);

            // 应用优化设置
            yield return ApplyOptimizationSettings(profile.optimizationSettings);

            // 应用资源设置
            yield return ApplyResourceSettings(profile.resourceSettings);

            Debug.Log($"[DeviceProfileManager] Profile applied successfully: {profile.profileName}");
        }

        private IEnumerator ApplyQualitySettings(QualitySettingsData settings)
        {
            QualitySettings.SetQualityLevel((int)settings.qualityLevel, true);
            QualitySettings.vSyncCount = settings.enableVSync ? 1 : 0;
            QualitySettings.antiAliasing = settings.antiAliasing;
            QualitySettings.shadows = settings.shadowQuality;
            QualitySettings.shadowDistance = settings.shadowDistance;
            Application.targetFrameRate = settings.maximumFPS;

            yield return new WaitForSeconds(0.5f); // 等待设置生效
        }

        private IEnumerator ApplyPerformanceSettings(PerformanceSettingsData settings)
        {
            if (_qualityManager != null)
            {
                _qualityManager.enableAdaptiveQuality = settings.adaptiveQualityEnabled;
                _qualityManager.autoAdjustQuality = settings.enableDynamicQuality;
                _qualityManager.fpsCriticalThreshold = settings.qualityAdjustmentThreshold;
                _qualityManager.SetQualityLevel(settings.qualityLevel);
            }

            if (_objectPool != null)
            {
                _objectPool.SetPoolSize(settings.objectPoolSize);
            }

            if (_memorySystem != null)
            {
                _memorySystem.enableAutoCleanup = settings.enableAggressiveCleanup;
                _memorySystem.cleanupThreshold = 0.8f;
                _memorySystem.cleanupInterval = settings.cleanupInterval;
            }

            yield return null;
        }

        private IEnumerator ApplyOptimizationSettings(OptimizationSettingsData settings)
        {
            // 应用优化设置
            // 这里可以添加具体的优化设置应用逻辑

            if (settings.enableFrameLimiting && settings.frameLimit > 0)
            {
                Application.targetFrameRate = settings.frameLimit;
            }

            yield return null;
        }

        private IEnumerator ApplyResourceSettings(ResourceSettingsData settings)
        {
            // 应用资源设置
            // 这里可以添加具体的资源设置应用逻辑

            yield return null;
        }

        #endregion

        #region Profile Saving and Loading

        private IEnumerator AutoLoadProfileCoroutine()
        {
            if (_deviceDetector?.DetectedCapabilities == null)
            {
                yield return new WaitForSeconds(2f); // 等待设备检测
            }

            var deviceId = GenerateDeviceId();
            var profilePath = Path.Combine(_profileDirectoryPath, $"{deviceId}.json");

            if (File.Exists(profilePath))
            {
                yield return LoadProfile(deviceId);
            }
            else if (enableAutoProfiling)
            {
                Debug.Log("[DeviceProfileManager] No existing profile found, creating new profile");
                yield return CreateProfileCoroutine();
            }
        }

        private IEnumerator LoadProfile(string deviceId)
        {
            _isProfileLoading = true;

            try
            {
                var profilePath = Path.Combine(_profileDirectoryPath, $"{deviceId}.json");

                if (!File.Exists(profilePath))
                {
                    Debug.LogWarning($"[DeviceProfileManager] Profile file not found: {profilePath}");
                    yield break;
                }

                var json = File.ReadAllText(profilePath);
                var profile = JsonUtility.FromJson<DevicePerformanceProfile>(json);

                if (profile != null)
                {
                    _currentProfile = profile;
                    _profileCache[deviceId] = profile;

                    yield return ApplyProfile(profile);

                    OnProfileLoaded?.Invoke(profile);
                    Debug.Log($"[DeviceProfileManager] Profile loaded: {profile.profileName}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DeviceProfileManager] Error loading profile: {e.Message}");
                OnProfileError?.Invoke($"Error loading profile: {e.Message}");
            }
            finally
            {
                _isProfileLoading = false;
            }
        }

        private IEnumerator SaveProfile(DevicePerformanceProfile profile)
        {
            try
            {
                profile.lastUpdated = DateTime.UtcNow;

                var profilePath = Path.Combine(_profileDirectoryPath, $"{profile.deviceId}.json");
                var json = JsonUtility.ToJson(profile, true);

                File.WriteAllText(profilePath, json);

                // 同时保存备份
                var backupPath = Path.Combine(_profileDirectoryPath, $"{profile.deviceId}_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                File.WriteAllText(backupPath, json);

                Debug.Log($"[DeviceProfileManager] Profile saved: {profile.profileName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DeviceProfileManager] Error saving profile: {e.Message}");
                OnProfileError?.Invoke($"Error saving profile: {e.Message}");
            }

            yield return null;
        }

        #endregion

        #region Public API

        public void UpdateProfile()
        {
            if (_currentProfile == null)
            {
                Debug.LogWarning("[DeviceProfileManager] No profile to update");
                return;
            }

            StartCoroutine(UpdateProfileCoroutine());
        }

        private IEnumerator UpdateProfileCoroutine()
        {
            Debug.Log("[DeviceProfileManager] Updating current profile");

            // 收集最新的性能数据
            yield return CollectPerformanceData(_currentProfile);

            // 根据最新数据调整配置
            AdjustProfileBasedOnPerformanceData(_currentProfile);

            // 保存更新后的配置文件
            yield return SaveProfile(_currentProfile);

            OnProfileUpdated?.Invoke(_currentProfile);
            Debug.Log($"[DeviceProfileManager] Profile updated: {_currentProfile.profileName}");
        }

        public void ResetProfile()
        {
            if (_currentProfile == null) return;

            var deviceClass = _currentProfile.deviceClass;
            if (_profileTemplates.ContainsKey(deviceClass))
            {
                var template = _profileTemplates[deviceClass];

                // 重置为模板值
                _currentProfile.qualitySettings = CreateQualitySettingsFromTemplate(template.qualitySettings, _currentProfile.deviceCapabilities);
                _currentProfile.performanceSettings = CreatePerformanceSettingsFromTemplate(template.performanceSettings, _currentProfile.deviceCapabilities);
                _currentProfile.optimizationSettings = CreateOptimizationSettingsFromTemplate(template.optimizationSettings, _currentProfile.deviceCapabilities);
                _currentProfile.resourceSettings = CreateResourceSettingsFromTemplate(template.resourceSettings, _currentProfile.deviceCapabilities);
                _currentProfile.lastUpdated = DateTime.UtcNow;

                StartCoroutine(ApplyProfile(_currentProfile));
                StartCoroutine(SaveProfile(_currentProfile));

                Debug.Log($"[DeviceProfileManager] Profile reset to template: {_currentProfile.profileName}");
            }
        }

        public List<DevicePerformanceProfile> GetAllProfiles()
        {
            var profiles = new List<DevicePerformanceProfile>();

            if (Directory.Exists(_profileDirectoryPath))
            {
                var files = Directory.GetFiles(_profileDirectoryPath, "*.json");

                foreach (var file in files)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var profile = JsonUtility.FromJson<DevicePerformanceProfile>(json);
                        if (profile != null)
                        {
                            profiles.Add(profile);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[DeviceProfileManager] Failed to load profile from {file}: {e.Message}");
                    }
                }
            }

            return profiles;
        }

        public void DeleteProfile(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return;

            var profilePath = Path.Combine(_profileDirectoryPath, $"{deviceId}.json");

            if (File.Exists(profilePath))
            {
                File.Delete(profilePath);
                _profileCache.Remove(deviceId);
                Debug.Log($"[DeviceProfileManager] Profile deleted: {deviceId}");
            }
        }

        public DeviceProfileExportData ExportProfile(string deviceId)
        {
            if (_profileCache.ContainsKey(deviceId))
            {
                var profile = _profileCache[deviceId];
                return new DeviceProfileExportData
                {
                    profile = profile,
                    exportedAt = DateTime.UtcNow,
                    exportVersion = "1.0"
                };
            }
            return null;
        }

        public IEnumerator ImportProfile(DeviceProfileExportData exportData)
        {
            if (exportData?.profile == null)
            {
                Debug.LogWarning("[DeviceProfileManager] Invalid export data");
                yield break;
            }

            var profile = exportData.profile;

            // 验证导入的配置文件
            var validationResult = new ProfileValidationResult
            {
                profileId = profile.deviceId,
                profileName = profile.profileName,
                isValid = true,
                warnings = new List<string>(),
                errors = new List<string>()
            };

            // 基本验证
            if (profile.deviceId == null || profile.deviceId.Length == 0)
            {
                validationResult.errors.Add("Device ID is required");
                validationResult.isValid = false;
            }

            if (profile.qualitySettings.targetFPS <= 0)
            {
                validationResult.errors.Add("Target FPS must be greater than 0");
                validationResult.isValid = false;
            }

            if (profile.performanceSettings.maxConcurrentCoins <= 0)
            {
                validationResult.errors.Add("Max concurrent coins must be greater than 0");
                validationResult.isValid = false;
            }

            // 性能验证
            yield return StartCoroutine(ValidateProfilePerformance(profile, validationResult));

            // 兼容性验证
            ValidateProfileCompatibility(profile, validationResult);
            if (!validationResult.isValid)
            {
                Debug.LogWarning($"[DeviceProfileManager] Imported profile validation failed: {string.Join(", ", validationResult.errors)}");
                yield break;
            }

            // 生成新的设备ID以避免冲突
            profile.deviceId = GenerateDeviceId();
            profile.createdAt = DateTime.UtcNow;
            profile.lastUpdated = DateTime.UtcNow;

            _profileCache[profile.deviceId] = profile;
            yield return SaveProfile(profile);

            Debug.Log($"[DeviceProfileManager] Profile imported: {profile.profileName}");
        }

        #endregion

        #region Helper Methods

        private List<DevicePerformanceProfile> CreateDefaultDeviceProfiles()
        {
            return new List<DevicePerformanceProfile>
            {
                CreateProfileFromTemplate(_profileTemplates[DevicePerformanceClass.LowEnd]),
                CreateProfileFromTemplate(_profileTemplates[DevicePerformanceClass.MidRange]),
                CreateProfileFromTemplate(_profileTemplates[DevicePerformanceClass.HighEnd])
            };
        }

        private DevicePerformanceProfile CreateProfileFromTemplate(DeviceProfileTemplate template)
        {
            return new DevicePerformanceProfile
            {
                deviceId = template.deviceClass.ToString(),
                deviceName = template.deviceClass.ToString(),
                deviceClass = template.deviceClass,
                profileName = template.name,
                description = template.description,
                createdAt = DateTime.UtcNow,
                lastUpdated = DateTime.UtcNow,
                version = "1.0.0",
                qualitySettings = ConvertTemplateToQualitySettings(template.qualitySettings),
                performanceSettings = ConvertTemplateToPerformanceSettings(template.performanceSettings),
                optimizationSettings = ConvertTemplateToOptimizationSettings(template.optimizationSettings),
                resourceSettings = ConvertTemplateToResourceSettings(template.resourceSettings)
            };
        }

        private QualitySettingsData ConvertTemplateToQualitySettings(QualitySettingsTemplate template)
        {
            return new QualitySettingsData
            {
                qualityLevel = template.qualityLevel,
                targetFPS = template.targetFPS,
                maximumFPS = template.maximumFPS,
                enableVSync = template.enableVSync,
                antiAliasing = template.antiAliasing,
                textureQuality = template.textureQuality,
                shadowQuality = template.shadowQuality,
                shadowDistance = template.shadowDistance,
                particleCount = template.particleCount,
                enablePostProcessing = template.enablePostProcessing,
                enableHDR = false,
                enableRayTracing = false
            };
        }

        private PerformanceSettingsData ConvertTemplateToPerformanceSettings(PerformanceSettingsTemplate template)
        {
            return new PerformanceSettingsData
            {
                maxConcurrentCoins = template.maxConcurrentCoins,
                objectPoolSize = template.objectPoolSize,
                memoryLimitMB = template.memoryLimitMB,
                gpuTimeLimit = template.gpuTimeLimit,
                enableAggressiveCleanup = template.enableAggressiveCleanup,
                cleanupInterval = template.cleanupInterval,
                enableDynamicQuality = template.enableDynamicQuality,
                qualityAdjustmentThreshold = template.qualityAdjustmentThreshold,
                adaptiveQualityEnabled = template.enableDynamicQuality
            };
        }

        private OptimizationSettingsData ConvertTemplateToOptimizationSettings(OptimizationSettingsTemplate template)
        {
            return new OptimizationSettingsData
            {
                enableFrameLimiting = template.enableFrameLimiting,
                frameLimit = template.frameLimit,
                enableLOD = template.enableLOD,
                lodBias = template.lodBias,
                enableOcclusion = template.enableOcclusion,
                enableFrustumCulling = template.enableFrustumCulling,
                enableInstancing = template.enableInstancing,
                enableBatching = template.enableBatching,
                enableAsyncUpload = false,
                enableGPUInstancing = false
            };
        }

        private ResourceSettingsData ConvertTemplateToResourceSettings(ResourceSettingsTemplate template)
        {
            return new ResourceSettingsData
            {
                maxTextureSize = template.maxTextureSize,
                textureCompression = template.textureCompression,
                audioQuality = template.audioQuality,
                enableStreaming = template.enableStreaming,
                cacheSize = template.cacheSize,
                enableMipMaps = true,
                enableAnisotropicFiltering = false
            };
        }

        #endregion
    }

    #region Helper Components

    /// <summary>
    /// 配置文件数据收集器
    /// </summary>
    public class ProfileDataCollector : MonoBehaviour
    {
        public IEnumerator CollectDataCoroutine(float duration)
        {
            var data = new ProfilePerformanceData
            {
                collectionDuration = duration,
                samples = new List<PerformanceSample>()
            };

            var startTime = Time.time;
            var frameCount = 0;

            while (Time.time - startTime < duration)
            {
                var sample = new PerformanceSample
                {
                    timestamp = Time.time - startTime,
                    fps = 1f / Time.unscaledDeltaTime,
                    frameTime = Time.unscaledDeltaTime * 1000f,
                    memoryUsage = GC.GetTotalMemory(false) / (1024f * 1024f),
                    frameCount = frameCount++
                };

                data.samples.Add(sample);
                yield return null;
            }

            // 计算统计数据
            data.averageFPS = data.samples.Average(s => s.fps);
            data.minFPS = data.samples.Min(s => s.fps);
            data.maxFPS = data.samples.Max(s => s.fps);
            data.averageFrameTime = data.samples.Average(s => s.frameTime);
            data.averageMemoryUsage = data.samples.Average(s => s.memoryUsage);
            data.peakMemoryUsage = data.samples.Max(s => s.memoryUsage);
            data.totalFrames = data.samples.Count;

            yield return data;
        }
    }

    #endregion

    #region Data Structures

    [System.Serializable]
    public class DevicePerformanceProfile
    {
        public string deviceId;
        public string deviceName;
        public DevicePerformanceClass deviceClass;
        public string profileName;
        public string description;
        public DateTime createdAt;
        public DateTime lastUpdated;
        public string version;
        public DeviceCapabilities deviceCapabilities;
        public QualitySettingsData qualitySettings;
        public PerformanceSettingsData performanceSettings;
        public OptimizationSettingsData optimizationSettings;
        public ResourceSettingsData resourceSettings;
        public ProfilePerformanceData performanceData;
    }

    [System.Serializable]
    public class QualitySettingsData
    {
        public QualityLevel qualityLevel;
        public int targetFPS;
        public int maximumFPS;
        public bool enableVSync;
        public int antiAliasing;
        public int textureQuality;
        public ShadowQuality shadowQuality;
        public float shadowDistance;
        public int particleCount;
        public bool enablePostProcessing;
        public bool enableHDR;
        public bool enableRayTracing;
    }

    [System.Serializable]
    public class PerformanceSettingsData
    {
        public int maxConcurrentCoins;
        public int objectPoolSize;
        public float memoryLimitMB;
        public float gpuTimeLimit;
        public bool enableAggressiveCleanup;
        public float cleanupInterval;
        public bool enableDynamicQuality;
        public float qualityAdjustmentThreshold;
        public bool adaptiveQualityEnabled;
        public QualityLevel qualityLevel;
    }

    [System.Serializable]
    public class OptimizationSettingsData
    {
        public bool enableFrameLimiting;
        public int frameLimit;
        public bool enableLOD;
        public float lodBias;
        public bool enableOcclusion;
        public bool enableFrustumCulling;
        public bool enableInstancing;
        public bool enableBatching;
        public bool enableAsyncUpload;
        public bool enableGPUInstancing;
    }

    [System.Serializable]
    public class ResourceSettingsData
    {
        public int maxTextureSize;
        public bool textureCompression;
        public float audioQuality;
        public bool enableStreaming;
        public int cacheSize;
        public bool enableMipMaps;
        public bool enableAnisotropicFiltering;
    }

    [System.Serializable]
    public class ProfilePerformanceData
    {
        public float collectionDuration;
        public float averageFPS;
        public float minFPS;
        public float maxFPS;
        public float averageFrameTime;
        public float averageMemoryUsage;
        public float peakMemoryUsage;
        public int totalFrames;
        public List<PerformanceSample> samples;
    }

    [System.Serializable]
    public class PerformanceSample
    {
        public float timestamp;
        public float fps;
        public float frameTime;
        public float memoryUsage;
        public int frameCount;
    }

    [System.Serializable]
    public class DeviceProfileTemplate
    {
        public DevicePerformanceClass deviceClass;
        public string name;
        public string description;
        public QualitySettingsTemplate qualitySettings;
        public PerformanceSettingsTemplate performanceSettings;
        public OptimizationSettingsTemplate optimizationSettings;
        public ResourceSettingsTemplate resourceSettings;
    }

    [System.Serializable]
    public class QualitySettingsTemplate
    {
        public QualityLevel qualityLevel;
        public int targetFPS;
        public int maximumFPS;
        public bool enableVSync;
        public int antiAliasing;
        public int textureQuality;
        public ShadowQuality shadowQuality;
        public float shadowDistance;
        public int particleCount;
        public bool enablePostProcessing;
    }

    [System.Serializable]
    public class PerformanceSettingsTemplate
    {
        public int maxConcurrentCoins;
        public int objectPoolSize;
        public float memoryLimitMB;
        public float gpuTimeLimit;
        public bool enableAggressiveCleanup;
        public float cleanupInterval;
        public bool enableDynamicQuality;
        public float qualityAdjustmentThreshold;
    }

    [System.Serializable]
    public class OptimizationSettingsTemplate
    {
        public bool enableFrameLimiting;
        public int frameLimit;
        public bool enableLOD;
        public float lodBias;
        public bool enableOcclusion;
        public bool enableFrustumCulling;
        public bool enableInstancing;
        public bool enableBatching;
    }

    [System.Serializable]
    public class ResourceSettingsTemplate
    {
        public int maxTextureSize;
        public bool textureCompression;
        public float audioQuality;
        public bool enableStreaming;
        public int cacheSize;
    }

    [System.Serializable]
    public class DeviceProfilePresets
    {
        public List<DeviceProfileTemplate> presets = new List<DeviceProfileTemplate>();
    }

    [System.Serializable]
    public class ProfileValidationResult
    {
        public string profileId;
        public string profileName;
        public bool isValid;
        public List<string> warnings;
        public List<string> errors;
        public string errorMessage;
    }

    [System.Serializable]
    public class SettingsSnapshot
    {
        public int qualityLevel;
        public int vSyncCount;
        public int antiAliasing;
        public ShadowQuality shadowQuality;
        public float shadowDistance;
        public int targetFrameRate;
    }

    [System.Serializable]
    public class DeviceProfileExportData
    {
        public DevicePerformanceProfile profile;
        public DateTime exportedAt;
        public string exportVersion;
    }

    #endregion

    #region Extension Methods

    public static class StringExtensions
    {
        public static string ToHexString(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

    #endregion
}