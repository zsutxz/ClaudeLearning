using UnityEngine;
using UnityEngine.UI;

public class WaterfallDemo : MonoBehaviour
{
    [Header("Demo Settings")]
    public Transform demoOrigin;
    public Transform demoTarget;
    public float demoInterval = 2.0f;
    
    [Header("UI References")]
    public Button startDemoButton;
    public Button stopDemoButton;
    public Toggle autoQualityToggle;
    public Dropdown qualityDropdown;
    public Text statusText;
    
    private bool isDemoRunning = false;
    private float lastDemoTime = 0f;
    
    void Start()
    {
        // Setup UI event listeners
        if (startDemoButton != null)
            startDemoButton.onClick.AddListener(StartWaterfallDemo);
            
        if (stopDemoButton != null)
            stopDemoButton.onClick.AddListener(StopWaterfallDemo);
            
        if (autoQualityToggle != null)
            autoQualityToggle.onValueChanged.AddListener(OnAutoQualityToggleChanged);
            
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(OnQualityDropdownChanged);
            
        // Initialize quality dropdown options
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string> { "Low", "Medium", "High" });
            qualityDropdown.value = (int)WaterfallQualitySettings.Instance.currentQualityLevel;
        }
        
        UpdateStatusText("Ready to start demo");
    }
    
    void Update()
    {
        if (isDemoRunning && Time.time - lastDemoTime >= demoInterval)
        {
            TriggerWaterfallDemo();
            lastDemoTime = Time.time;
        }
    }
    
    /// <summary>
    /// Starts the waterfall demo
    /// </summary>
    public void StartWaterfallDemo()
    {
        isDemoRunning = true;
        lastDemoTime = Time.time - demoInterval; // Trigger immediately
        UpdateStatusText("Waterfall demo running...");
        
        // Auto-detect quality if enabled
        if (autoQualityToggle != null && autoQualityToggle.isOn)
        {
            WaterfallQualitySettings.Instance.AutoDetectQualityLevel();
            if (qualityDropdown != null)
                qualityDropdown.value = (int)WaterfallQualitySettings.Instance.currentQualityLevel;
        }
    }
    
    /// <summary>
    /// Stops the waterfall demo
    /// </summary>
    public void StopWaterfallDemo()
    {
        isDemoRunning = false;
        UpdateStatusText("Demo stopped");
    }
    
    /// <summary>
    /// Triggers a single waterfall effect in the demo
    /// </summary>
    private void TriggerWaterfallDemo()
    {
        if (CoinAnimationSystem.Instance != null && demoOrigin != null && demoTarget != null)
        {
            Vector3 origin = demoOrigin.position;
            Vector3 target = demoTarget.position;
            
            // Add some randomness to make it more interesting
            origin += new Vector3(Random.Range(-1f, 1f), Random.Range(-0.5f, 0.5f), 0);
            target += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1f, 2f), 0);
            
            // Register a coin collection to trigger combo
            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.RegisterCoinCollection();
            }
            
            // Trigger the waterfall cascade
            CoinAnimationSystem.Instance.TriggerWaterfallCascade(origin, target);
            
            UpdateStatusText($"Waterfall triggered! Combo: {ComboManager.Instance?.GetCurrentCombo() ?? 0}");
        }
    }
    
    /// <summary>
    /// Updates the status text display
    /// </summary>
    /// <param name="text">Text to display</param>
    private void UpdateStatusText(string text)
    {
        if (statusText != null)
        {
            statusText.text = text;
        }
    }
    
    /// <summary>
    /// Handles auto quality toggle changes
    /// </summary>
    /// <param name="isOn">Whether toggle is on</param>
    private void OnAutoQualityToggleChanged(bool isOn)
    {
        if (isOn)
        {
            WaterfallQualitySettings.Instance.AutoDetectQualityLevel();
            if (qualityDropdown != null)
                qualityDropdown.value = (int)WaterfallQualitySettings.Instance.currentQualityLevel;
            UpdateStatusText("Quality auto-detected");
        }
    }
    
    /// <summary>
    /// Handles quality dropdown changes
    /// </summary>
    /// <param name="value">Selected dropdown value</param>
    private void OnQualityDropdownChanged(int value)
    {
        WaterfallQualitySettings.QualityLevel qualityLevel = (WaterfallQualitySettings.QualityLevel)value;
        WaterfallQualitySettings.Instance.SetQualityLevel(qualityLevel);
        UpdateStatusText($"Quality set to {qualityLevel}");
    }
    
    void OnDestroy()
    {
        // Clean up event listeners
        if (startDemoButton != null)
            startDemoButton.onClick.RemoveListener(StartWaterfallDemo);
            
        if (stopDemoButton != null)
            stopDemoButton.onClick.RemoveListener(StopWaterfallDemo);
    }
}