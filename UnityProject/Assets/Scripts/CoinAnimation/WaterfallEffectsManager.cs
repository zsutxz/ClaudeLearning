using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class WaterfallEffectsManager : MonoBehaviour
{
    [Header("Waterfall Effects Settings")]
    public float baseCascadeDelay = 0.05f;
    public float cascadeDelayVariation = 0.02f;
    public float baseCurveHeight = 1.0f;
    public float curveHeightVariation = 0.5f;
    
    [Header("Tier Intensity Settings")]
    public WaterfallTierSettings bronzeSettings;
    public WaterfallTierSettings silverSettings;
    public WaterfallTierSettings goldSettings;
    public WaterfallTierSettings platinumSettings;
    
    [Header("Effect Components")]
    public ParticleSystem waterfallParticles;
    public Camera mainCamera;
    
    [Header("Events")]
    public UnityEvent<ComboManager.ComboLevel> onTierEffectTriggered;
    
    [System.Serializable]
    public class WaterfallTierSettings
    {
        public float intensityMultiplier = 1.0f;
        public float particleDensity = 1.0f;
        public float screenShakeIntensity = 0.1f;
        public Color tierColor = Color.white;
        public float audioVolume = 1.0f;
    }
    
    private static WaterfallEffectsManager instance;
    
    public static WaterfallEffectsManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject waterfallManagerObject = new GameObject("WaterfallEffectsManager");
                instance = waterfallManagerObject.AddComponent<WaterfallEffectsManager>();
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
            
            // Initialize default tier settings
            InitializeDefaultTierSettings();
            
            // Try to find main camera if not set
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initializes default tier settings with appropriate values
    /// </summary>
    private void InitializeDefaultTierSettings()
    {
        if (bronzeSettings == null) bronzeSettings = new WaterfallTierSettings();
        if (silverSettings == null) silverSettings = new WaterfallTierSettings();
        if (goldSettings == null) goldSettings = new WaterfallTierSettings();
        if (platinumSettings == null) platinumSettings = new WaterfallTierSettings();
        
        // Bronze settings (lowest intensity)
        bronzeSettings.intensityMultiplier = 1.0f;
        bronzeSettings.particleDensity = 1.0f;
        bronzeSettings.screenShakeIntensity = 0.1f;
        bronzeSettings.tierColor = new Color(0.8f, 0.6f, 0.4f); // Bronze color
        bronzeSettings.audioVolume = 0.5f;
        
        // Silver settings (medium-low intensity)
        silverSettings.intensityMultiplier = 1.5f;
        silverSettings.particleDensity = 1.5f;
        silverSettings.screenShakeIntensity = 0.2f;
        silverSettings.tierColor = new Color(0.8f, 0.8f, 0.8f); // Silver color
        silverSettings.audioVolume = 0.7f;
        
        // Gold settings (medium-high intensity)
        goldSettings.intensityMultiplier = 2.0f;
        goldSettings.particleDensity = 2.0f;
        goldSettings.screenShakeIntensity = 0.3f;
        goldSettings.tierColor = new Color(1.0f, 0.8f, 0.2f); // Gold color
        goldSettings.audioVolume = 0.9f;
        
        // Platinum settings (highest intensity)
        platinumSettings.intensityMultiplier = 3.0f;
        platinumSettings.particleDensity = 3.0f;
        platinumSettings.screenShakeIntensity = 0.5f;
        platinumSettings.tierColor = new Color(0.7f, 0.7f, 1.0f); // Platinum color
        platinumSettings.audioVolume = 1.0f;
    }
    
    /// <summary>
    /// Gets the settings for a specific combo tier
    /// </summary>
    /// <param name="tier">The combo tier to get settings for</param>
    /// <returns>Tier settings</returns>
    public WaterfallTierSettings GetTierSettings(ComboManager.ComboLevel tier)
    {
        switch (tier)
        {
            case ComboManager.ComboLevel.Bronze:
                return bronzeSettings;
            case ComboManager.ComboLevel.Silver:
                return silverSettings;
            case ComboManager.ComboLevel.Gold:
                return goldSettings;
            case ComboManager.ComboLevel.Platinum:
                return platinumSettings;
            default:
                return bronzeSettings; // Default to bronze settings
        }
    }
    
    /// <summary>
    /// Triggers waterfall effects for a specific tier
    /// </summary>
    /// <param name="tier">The combo tier to trigger effects for</param>
    public void TriggerWaterfallEffect(ComboManager.ComboLevel tier)
    {
        // Get tier settings
        WaterfallTierSettings settings = GetTierSettings(tier);
        
        // Apply visual effects based on tier
        ApplyVisualEffects(settings);
        
        // Apply audio effects based on tier
        ApplyAudioEffects(settings);
        
        // Apply screen shake based on tier
        ApplyScreenShake(settings);
        
        // Notify listeners
        onTierEffectTriggered?.Invoke(tier);
    }
    
    /// <summary>
    /// Applies visual effects for the current tier
    /// </summary>
    /// <param name="settings">Tier settings to apply</param>
    private void ApplyVisualEffects(WaterfallTierSettings settings)
    {
        // Trigger particle effects if available
        if (waterfallParticles != null)
        {
            var particleSettings = waterfallParticles.main;
            particleSettings.maxParticles = Mathf.RoundToInt(100 * settings.particleDensity);
            
            var emission = waterfallParticles.emission;
            emission.rateOverTime = 50 * settings.particleDensity;
            
            var colorModule = waterfallParticles.colorOverLifetime;
            colorModule.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(settings.tierColor, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorModule.color = gradient;
            
            waterfallParticles.Play();
            
            // Stop particles after a short duration
            DOVirtual.DelayedCall(2.0f, () => {
                if (waterfallParticles != null)
                {
                    waterfallParticles.Stop();
                }
            });
        }
        
        // In a full implementation, this would also trigger
        // lighting changes and other visual enhancements
        Debug.Log($"Waterfall visual effects triggered with intensity {settings.intensityMultiplier}");
    }
    
    /// <summary>
    /// Applies audio effects for the current tier
    /// </summary>
    /// <param name="settings">Tier settings to apply</param>
    private void ApplyAudioEffects(WaterfallTierSettings settings)
    {
        // In a full implementation, this would trigger audio clips
        // with volume and pitch based on tier settings
        // For now, we'll just log the effect
        Debug.Log($"Waterfall audio effects triggered with volume {settings.audioVolume}");
    }
    
    /// <summary>
    /// Applies screen shake effect for the current tier
    /// </summary>
    /// <param name="settings">Tier settings to apply</param>
    private void ApplyScreenShake(WaterfallTierSettings settings)
    {
        // Trigger camera shake if main camera is available
        if (mainCamera != null)
        {
            // Store original camera position
            Vector3 originalPosition = mainCamera.transform.localPosition;
            
            // Create a shake sequence
            float shakeDuration = 0.5f;
            float shakeMagnitude = settings.screenShakeIntensity;
            
            // Use DOTween to create the shake effect
            mainCamera.transform.DOShakePosition(shakeDuration, shakeMagnitude, 10, 90, false, true)
                .OnComplete(() => {
                    // Reset camera position after shake
                    mainCamera.transform.localPosition = originalPosition;
                });
        }
        
        Debug.Log($"Screen shake triggered with intensity {settings.screenShakeIntensity}");
    }
    
    /// <summary>
    /// Calculates cascade delay with variation for natural timing
    /// </summary>
    /// <param name="index">Index of the coin in the cascade</param>
    /// <returns>Cascade delay with variation</returns>
    public float GetCascadeDelay(int index)
    {
        float variation = Random.Range(-cascadeDelayVariation, cascadeDelayVariation);
        return baseCascadeDelay + (variation * index * 0.1f);
    }
    
    /// <summary>
    /// Calculates curve height with variation for natural paths
    /// </summary>
    /// <param name="index">Index of the coin in the cascade</param>
    /// <returns>Curve height with variation</returns>
    public float GetCurveHeight(int index)
    {
        float variation = Random.Range(-curveHeightVariation, curveHeightVariation);
        return baseCurveHeight + (variation * index * 0.1f);
    }
    
    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}