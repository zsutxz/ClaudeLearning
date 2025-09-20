using UnityEngine;

public class WaterfallQualitySettings : MonoBehaviour
{
    public enum QualityLevel
    {
        Low,
        Medium,
        High
    }
    
    [Header("Quality Settings")]
    public QualityLevel currentQualityLevel = QualityLevel.High;
    
    [Header("Low Quality Settings")]
    public WaterfallSettings lowSettings;
    
    [Header("Medium Quality Settings")]
    public WaterfallSettings mediumSettings;
    
    [Header("High Quality Settings")]
    public WaterfallSettings highSettings;
    
    [System.Serializable]
    public class WaterfallSettings
    {
        public int maxCascadeCoins = 5;
        public float particleDensity = 0.5f;
        public float screenShakeIntensity = 0.1f;
        public int maxParticles = 100;
        public float curveHeightVariation = 0.2f;
    }
    
    private static WaterfallQualitySettings instance;
    
    public static WaterfallQualitySettings Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject qualitySettingsObject = new GameObject("WaterfallQualitySettings");
                instance = qualitySettingsObject.AddComponent<WaterfallQualitySettings>();
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDefaultSettings();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initializes default quality settings
    /// </summary>
    private void InitializeDefaultSettings()
    {
        // Low quality settings
        lowSettings.maxCascadeCoins = 3;
        lowSettings.particleDensity = 0.3f;
        lowSettings.screenShakeIntensity = 0.05f;
        lowSettings.maxParticles = 50;
        lowSettings.curveHeightVariation = 0.1f;
        
        // Medium quality settings
        mediumSettings.maxCascadeCoins = 5;
        mediumSettings.particleDensity = 0.6f;
        mediumSettings.screenShakeIntensity = 0.15f;
        mediumSettings.maxParticles = 150;
        mediumSettings.curveHeightVariation = 0.3f;
        
        // High quality settings
        highSettings.maxCascadeCoins = 10;
        highSettings.particleDensity = 1.0f;
        highSettings.screenShakeIntensity = 0.3f;
        highSettings.maxParticles = 300;
        highSettings.curveHeightVariation = 0.5f;
    }
    
    /// <summary>
    /// Gets the settings for the current quality level
    /// </summary>
    /// <returns>Waterfall settings for current quality level</returns>
    public WaterfallSettings GetCurrentSettings()
    {
        switch (currentQualityLevel)
        {
            case QualityLevel.Low:
                return lowSettings;
            case QualityLevel.Medium:
                return mediumSettings;
            case QualityLevel.High:
                return highSettings;
            default:
                return highSettings;
        }
    }
    
    /// <summary>
    /// Sets the quality level and applies the settings
    /// </summary>
    /// <param name="qualityLevel">Quality level to set</param>
    public void SetQualityLevel(QualityLevel qualityLevel)
    {
        currentQualityLevel = qualityLevel;
        ApplyQualitySettings();
    }
    
    /// <summary>
    /// Applies the current quality settings to the waterfall effects
    /// </summary>
    private void ApplyQualitySettings()
    {
        WaterfallSettings settings = GetCurrentSettings();
        
        // Apply settings to CoinAnimationSystem
        if (CoinAnimationSystem.Instance != null)
        {
            CoinAnimationSystem.Instance.maxCascadeCoins = settings.maxCascadeCoins;
        }
        
        // Apply settings to WaterfallEffectsManager
        if (WaterfallEffectsManager.Instance != null)
        {
            WaterfallEffectsManager.Instance.curveHeightVariation = settings.curveHeightVariation;
            
            // Update tier settings with new particle density
            if (WaterfallEffectsManager.Instance.bronzeSettings != null)
                WaterfallEffectsManager.Instance.bronzeSettings.particleDensity *= settings.particleDensity;
            if (WaterfallEffectsManager.Instance.silverSettings != null)
                WaterfallEffectsManager.Instance.silverSettings.particleDensity *= settings.particleDensity;
            if (WaterfallEffectsManager.Instance.goldSettings != null)
                WaterfallEffectsManager.Instance.goldSettings.particleDensity *= settings.particleDensity;
            if (WaterfallEffectsManager.Instance.platinumSettings != null)
                WaterfallEffectsManager.Instance.platinumSettings.particleDensity *= settings.particleDensity;
                
            // Update particle system if available
            if (WaterfallEffectsManager.Instance.waterfallParticles != null)
            {
                var main = WaterfallEffectsManager.Instance.waterfallParticles.main;
                main.maxParticles = settings.maxParticles;
            }
        }
        
        Debug.Log($"Waterfall quality settings applied: {currentQualityLevel}");
    }
    
    /// <summary>
    /// Detects the appropriate quality level based on device capabilities
    /// </summary>
    public void AutoDetectQualityLevel()
    {
        // Simple detection based on device type
        if (SystemInfo.systemMemorySize < 2048)
        {
            SetQualityLevel(QualityLevel.Low);
        }
        else if (SystemInfo.systemMemorySize < 4096)
        {
            SetQualityLevel(QualityLevel.Medium);
        }
        else
        {
            SetQualityLevel(QualityLevel.High);
        }
    }
    
    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}