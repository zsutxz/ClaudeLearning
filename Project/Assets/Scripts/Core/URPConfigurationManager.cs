// Disable URP features since URP package is not installed
#define URP_NOT_AVAILABLE

using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if !URP_NOT_AVAILABLE && URP_12_0_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

namespace CoinAnimation.Core
{
    /// <summary>
    /// URP Configuration Manager for managing different performance tiers
    /// Handles AC2: URP Installation and Configuration
    /// </summary>
    public class URPConfigurationManager : MonoBehaviour
    {
        #if !URP_NOT_AVAILABLE && URP_12_0_OR_NEWER
        [Header("URP Assets")]
        [SerializeField] private UniversalRenderPipelineAsset lowQualityAsset;
        [SerializeField] private UniversalRenderPipelineAsset mediumQualityAsset;
        [SerializeField] private UniversalRenderPipelineAsset highQualityAsset;
#else
        [Header("URP Assets")]
        [SerializeField] private UnityEngine.Object lowQualityAsset;
        [SerializeField] private UnityEngine.Object mediumQualityAsset;
        [SerializeField] private UnityEngine.Object highQualityAsset;
        [Header("URP Warning")]
        [SerializeField] private string urpNotAvailableWarning = "URP is not installed or version is too old";
#endif
        
        [Header("Quality Settings")]
        [SerializeField] private RenderQuality currentQuality = RenderQuality.Medium;
        [SerializeField] private bool autoAdjustQuality = true;
        [SerializeField] private float targetFrameRate = 60f;
        
        private PerformanceMonitor performanceMonitor;
        
        public enum RenderQuality
        {
            Low = 0,
            Medium = 1,
            High = 2
        }
        
        private void Awake()
        {
            performanceMonitor = GetComponent<PerformanceMonitor>();
            if (performanceMonitor == null)
            {
                performanceMonitor = gameObject.AddComponent<PerformanceMonitor>();
            }
            
            InitializeQualitySettings();
        }
        
        private void Start()
        {
            SetQualityLevel(currentQuality);
            
            if (autoAdjustQuality)
            {
                InvokeRepeating(nameof(AdjustQualityBasedOnPerformance), 5f, 5f);
            }
        }
        
        #if !URP_NOT_AVAILABLE && URP_12_0_OR_NEWER
        /// <summary>
        /// Initialize URP quality settings for different performance tiers
        /// </summary>
        private void InitializeQualitySettings()
        {
            // Load URP assets if not assigned
            if (lowQualityAsset == null)
                lowQualityAsset = LoadAsset<UniversalRenderPipelineAsset>("URPSettings_LowQuality");

            if (mediumQualityAsset == null)
                mediumQualityAsset = LoadAsset<UniversalRenderPipelineAsset>("URPSettings_MediumQuality");

            if (highQualityAsset == null)
                highQualityAsset = LoadAsset<UniversalRenderPipelineAsset>("URPSettings_HighQuality");

            // Validate that all assets are loaded
            if (lowQualityAsset == null || mediumQualityAsset == null || highQualityAsset == null)
            {
                Debug.LogWarning("Some URP assets could not be loaded. Performance adjustment may not work correctly.");
            }
        }
        #else
        /// <summary>
        /// Initialize quality settings (URP not available)
        /// </summary>
        private void InitializeQualitySettings()
        {
            // Load generic assets if not assigned
            if (lowQualityAsset == null)
                lowQualityAsset = LoadAsset<UnityEngine.Object>("URPSettings_LowQuality");

            if (mediumQualityAsset == null)
                mediumQualityAsset = LoadAsset<UnityEngine.Object>("URPSettings_MediumQuality");

            if (highQualityAsset == null)
                highQualityAsset = LoadAsset<UnityEngine.Object>("URPSettings_HighQuality");

            // Validate that all assets are loaded
            if (lowQualityAsset == null || mediumQualityAsset == null || highQualityAsset == null)
            {
                Debug.LogWarning("Some quality assets could not be loaded. Performance adjustment may not work correctly.");
            }
        }
        #endif
        
        #if !URP_NOT_AVAILABLE && URP_12_0_OR_NEWER
        /// <summary>
        /// Set the render quality level
        /// </summary>
        /// <param name="quality">Target quality level</param>
        public void SetQualityLevel(RenderQuality quality)
        {
            UniversalRenderPipelineAsset targetAsset = null;

            switch (quality)
            {
                case RenderQuality.Low:
                    targetAsset = lowQualityAsset;
                    break;
                case RenderQuality.Medium:
                    targetAsset = mediumQualityAsset;
                    break;
                case RenderQuality.High:
                    targetAsset = highQualityAsset;
                    break;
            }

            if (targetAsset != null)
            {
                GraphicsSettings.renderPipelineAsset = targetAsset;
                QualitySettings.SetQualityLevel((int)quality, true);
                currentQuality = quality;

                Debug.Log($"URP Quality set to: {quality}");
            }
            else
            {
                Debug.LogError($"Failed to set quality level {quality}: Asset not found");
            }
        }
#else
        /// <summary>
        /// Set the render quality level (URP not available)
        /// </summary>
        /// <param name="quality">Target quality level</param>
        public void SetQualityLevel(RenderQuality quality)
        {
            currentQuality = quality;
            QualitySettings.SetQualityLevel((int)quality, true);
            Debug.LogWarning($"URP is not available. Using Built-in render pipeline with quality: {quality}");
        }
#endif
        
        /// <summary>
        /// Automatically adjust quality based on performance metrics
        /// </summary>
        private void AdjustQualityBasedOnPerformance()
        {
            if (performanceMonitor == null) return;
            
            var metrics = performanceMonitor.GetCurrentMetrics();
            
            // Adjust quality based on frame rate
            if (metrics.averageFrameRate < targetFrameRate * 0.8f) // Below 80% of target
            {
                if (currentQuality > RenderQuality.Low)
                {
                    SetQualityLevel(currentQuality - 1);
                    Debug.Log($"Decreased quality to {currentQuality} due to performance constraints");
                }
            }
            else if (metrics.averageFrameRate > targetFrameRate * 1.2f) // Above 120% of target
            {
                if (currentQuality < RenderQuality.High)
                {
                    SetQualityLevel(currentQuality + 1);
                    Debug.Log($"Increased quality to {currentQuality} due to good performance");
                }
            }
        }
        
        #if !URP_NOT_AVAILABLE && URP_12_0_OR_NEWER
        /// <summary>
        /// Get current URP configuration info
        /// </summary>
        /// <returns>Configuration information</returns>
        public URPConfigurationInfo GetCurrentConfiguration()
        {
            var currentAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

            return new URPConfigurationInfo
            {
                currentQuality = currentQuality,
                renderScale = currentAsset?.renderScale ?? 1f,
                msaaSampleCount = currentAsset?.msaaSampleCount ?? 1,
                shadowDistance = currentAsset?.shadowDistance ?? 50f,
                mainLightShadowResolution = currentAsset?.mainLightShadowmapResolution ?? 1024,
                supportsHDR = currentAsset?.supportsHDR ?? false,
                renderingPath = currentAsset != null ? "URP" : "Built-in"
            };
        }
#else
        /// <summary>
        /// Get current configuration info (URP not available)
        /// </summary>
        /// <returns>Configuration information</returns>
        public URPConfigurationInfo GetCurrentConfiguration()
        {
            return new URPConfigurationInfo
            {
                currentQuality = currentQuality,
                renderScale = 1f,
                msaaSampleCount = 1,
                shadowDistance = 50f,
                mainLightShadowResolution = 1024,
                supportsHDR = false,
                renderingPath = "Built-in"
            };
        }
#endif
        
        /// <summary>
        /// Validate URP installation and configuration
        /// </summary>
        /// <returns>Validation result</returns>
        public URPValidationResult ValidateURPInstallation()
        {
            var result = new URPValidationResult();
            
            // Check if URP is installed
            var urpAsset = GraphicsSettings.renderPipelineAsset;
            result.isURPInstalled = urpAsset != null && urpAsset.GetType().Name.Contains("UniversalRenderPipelineAsset");
            
            // Check URP version
            if (result.isURPInstalled)
            {
                var urpVersion = urpAsset.GetType().Assembly.GetName().Version;
                result.urpVersion = urpVersion.ToString();
                result.meetsMinimumVersion = urpVersion.Major >= 12; // URP 12.0+ required
            }
            
            // Check quality assets
            result.hasLowQualityAsset = lowQualityAsset != null;
            result.hasMediumQualityAsset = mediumQualityAsset != null;
            result.hasHighQualityAsset = highQualityAsset != null;
            
            // Check configuration
            result.graphicsSettingsConfigured = GraphicsSettings.renderPipelineAsset != null;
            result.qualitySettingsConfigured = QualitySettings.GetQualityLevel() >= 0;
            
            result.isFullyConfigured = result.isURPInstalled && 
                                      result.meetsMinimumVersion && 
                                      result.hasLowQualityAsset && 
                                      result.hasMediumQualityAsset && 
                                      result.hasHighQualityAsset &&
                                      result.graphicsSettingsConfigured &&
                                      result.qualitySettingsConfigured;
            
            return result;
        }
        
        private T LoadAsset<T>(string assetName) where T : Object
        {
#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets(assetName);
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }
#endif
            return null;
        }
    }
    
    /// <summary>
    /// Simple performance monitor for URP quality adjustment
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        private float frameRateSum;
        private int frameRateCount;
        private float lastUpdateTime;
        
        public URPPerformanceMetrics GetCurrentMetrics()
        {
            var currentTime = Time.time;
            var deltaTime = currentTime - lastUpdateTime;
            
            if (deltaTime >= 1f)
            {
                var averageFrameRate = frameRateCount / deltaTime;
                
                var metrics = new URPPerformanceMetrics
                {
                    averageFrameRate = averageFrameRate,
                    currentFrameRate = 1f / Time.unscaledDeltaTime,
                    memoryUsage = System.GC.GetTotalMemory(false) / (1024f * 1024f), // MB
                    timestamp = System.DateTime.Now
                };
                
                frameRateSum = 0;
                frameRateCount = 0;
                lastUpdateTime = currentTime;
                
                return metrics;
            }
            
            frameRateSum += 1f / Time.unscaledDeltaTime;
            frameRateCount++;
            
            return new URPPerformanceMetrics
            {
                averageFrameRate = frameRateCount > 0 ? frameRateSum / frameRateCount : 60f,
                currentFrameRate = 1f / Time.unscaledDeltaTime,
                memoryUsage = System.GC.GetTotalMemory(false) / (1024f * 1024f),
                timestamp = System.DateTime.Now
            };
        }
    }

    /// <summary>
    /// URP Performance metrics structure
    /// </summary>
    [System.Serializable]
    public class URPPerformanceMetrics
    {
        public float averageFrameRate;
        public float currentFrameRate;
        public float memoryUsage;
        public System.DateTime timestamp;

        public float MemoryUsageMB { get; internal set; }
        public float FPS { get; internal set; }
        public float FrameTime { get; internal set; }
        public int ActiveCoinsCount { get; set; }
        public float FrameRate { get; set; }
    }

    /// <summary>
    /// URP Configuration information structure
    /// </summary>
    [System.Serializable]
    public class URPConfigurationInfo
    {
        public URPConfigurationManager.RenderQuality currentQuality;
        public float renderScale;
        public int msaaSampleCount;
        public float shadowDistance;
        public int mainLightShadowResolution;
        public bool supportsHDR;
        public string renderingPath;
    }
    
    /// <summary>
    /// URP validation result structure
    /// </summary>
    [System.Serializable]
    public class URPValidationResult
    {
        public bool isURPInstalled;
        public string urpVersion = "Unknown";
        public bool meetsMinimumVersion;
        public bool hasLowQualityAsset;
        public bool hasMediumQualityAsset;
        public bool hasHighQualityAsset;
        public bool graphicsSettingsConfigured;
        public bool qualitySettingsConfigured;
        public bool isFullyConfigured;
    }
}