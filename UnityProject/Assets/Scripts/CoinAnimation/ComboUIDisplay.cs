using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ComboUIDisplay : MonoBehaviour
{
    [Header("UI References")]
    public GameObject comboPanel;
    public Text comboCountText;
    public Text comboLevelText;
    public Image comboLevelIcon;
    public Slider comboProgressSlider;
    public Text waterfallEffectText;
    public Image waterfallIcon;

    [Header("Display Settings")]
    public float displayDuration = 3.0f;
    public float fadeDuration = 0.5f;

    [Header("Combo Level Colors")]
    public Color bronzeColor = new Color(0.6f, 0.4f, 0.2f);  // Brown
    public Color silverColor = new Color(0.75f, 0.75f, 0.75f); // Silver
    public Color goldColor = new Color(1.0f, 0.84f, 0.0f);    // Gold
    public Color platinumColor = new Color(0.9f, 0.9f, 1.0f); // Light blue

    private CanvasGroup canvasGroup;
    private Coroutine hideCoroutine;

    void Awake()
    {
        // Get or add CanvasGroup for fade effects
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Initially hide the combo panel
        HideComboPanel();
    }

    void Start()
    {
        // Subscribe to combo events
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.onComboIncreased.AddListener(OnComboIncreased);
            ComboManager.Instance.onComboLevelChanged.AddListener(OnComboLevelChanged);
            ComboManager.Instance.onComboReset.AddListener(OnComboReset);
        }
    }

    /// <summary>
    /// Called when combo is increased
    /// </summary>
    /// <param name="comboCount">Current combo count</param>
    private void OnComboIncreased(int comboCount)
    {
        if (comboCount > 1) // Only show for combos of 2 or more
        {
            UpdateComboDisplay(comboCount);
            ShowComboPanel();
            AnimateComboIncrease();
        }
    }

    /// <summary>
    /// Called when combo level changes
    /// </summary>
    /// <param name="comboLevel">New combo level</param>
    private void OnComboLevelChanged(ComboManager.ComboLevel comboLevel)
    {
        UpdateComboLevelDisplay(comboLevel);
        AnimateComboLevelUp(comboLevel);
    }

    /// <summary>
    /// Called when combo is reset
    /// </summary>
    private void OnComboReset()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        HideComboPanel();
    }

    /// <summary>
    /// Updates the combo display with current combo count
    /// </summary>
    /// <param name="comboCount">Current combo count</param>
    private void UpdateComboDisplay(int comboCount)
    {
        if (comboCountText != null)
        {
            comboCountText.text = "COMBO x" + comboCount;
        }

        // Update progress slider if available
        if (comboProgressSlider != null && ComboManager.Instance != null)
        {
            int nextLevelThreshold = GetNextLevelThreshold(ComboManager.Instance.GetComboLevel(comboCount));
            if (nextLevelThreshold > 0)
            {
                float progress = (float)comboCount / nextLevelThreshold;
                comboProgressSlider.value = Mathf.Clamp01(progress);
            }
        }
    }

    /// <summary>
    /// Updates the combo level display
    /// </summary>
    /// <param name="comboLevel">Current combo level</param>
    private void UpdateComboLevelDisplay(ComboManager.ComboLevel comboLevel)
    {
        if (comboLevelText != null)
        {
            comboLevelText.text = GetComboLevelName(comboLevel);
        }

        if (comboLevelIcon != null)
        {
            comboLevelIcon.color = GetComboLevelColor(comboLevel);
        }

        // Update waterfall effect text based on combo level
        if (waterfallEffectText != null)
        {
            waterfallEffectText.text = GetWaterfallEffectDescription(comboLevel);
        }

        // Update waterfall icon based on combo level
        if (waterfallIcon != null)
        {
            waterfallIcon.color = GetComboLevelColor(comboLevel);
        }

        // Update panel background color
        if (comboPanel != null)
        {
            Image panelImage = comboPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = GetComboLevelColor(comboLevel);
            }
        }
    }

    /// <summary>
    /// Shows the combo panel with fade-in animation
    /// </summary>
    private void ShowComboPanel()
    {
        if (comboPanel != null)
        {
            comboPanel.SetActive(true);
            canvasGroup.alpha = 1.0f;

            // Cancel any existing hide coroutine
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }

            // Start new hide coroutine
            hideCoroutine = StartCoroutine(HidePanelAfterDelay(displayDuration));
        }
    }

    /// <summary>
    /// Hides the combo panel with fade-out animation
    /// </summary>
    private void HideComboPanel()
    {
        if (comboPanel != null)
        {
            comboPanel.SetActive(false);
            canvasGroup.alpha = 0.0f;
        }
    }

    /// <summary>
    /// Animates the combo increase with scaling effect
    /// </summary>
    private void AnimateComboIncrease()
    {
        if (comboCountText != null)
        {
            // Scale up and then back to normal
            comboCountText.transform.DOScale(1.2f, 0.1f)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => comboCountText.transform.DOScale(1.0f, 0.1f));
        }
    }

    /// <summary>
    /// Animates the combo level up with special effects
    /// </summary>
    /// <param name="comboLevel">New combo level</param>
    private void AnimateComboLevelUp(ComboManager.ComboLevel comboLevel)
    {
        if (comboLevelText != null)
        {
            // Color flash effect
            Color originalColor = comboLevelText.color;
            comboLevelText.DOColor(GetComboLevelColor(comboLevel), 0.2f)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => comboLevelText.color = originalColor);

            // Scale effect for level up
            comboLevelText.transform.DOScale(1.3f, 0.2f)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => comboLevelText.transform.DOScale(1.0f, 0.1f));
        }
        
        // Animate waterfall effect text if available
        if (waterfallEffectText != null)
        {
            // Color flash effect
            Color originalColor = waterfallEffectText.color;
            waterfallEffectText.DOColor(GetComboLevelColor(comboLevel), 0.2f)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => waterfallEffectText.color = originalColor);
        }
        
        // Animate waterfall icon if available
        if (waterfallIcon != null)
        {
            // Pulse effect
            waterfallIcon.transform.DOScale(1.5f, 0.2f)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => waterfallIcon.transform.DOScale(1.0f, 0.1f));
        }
    }

    /// <summary>
    /// Coroutine to hide panel after delay
    /// </summary>
    /// <param name="delay">Delay in seconds</param>
    private IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Fade out
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0.0f, fadeDuration).OnComplete(() => {
                if (comboPanel != null)
                {
                    comboPanel.SetActive(false);
                }
            });
        }

        hideCoroutine = null;
    }

    /// <summary>
    /// Gets the name for a combo level
    /// </summary>
    /// <param name="level">Combo level</param>
    /// <returns>Name of the combo level</returns>
    private string GetComboLevelName(ComboManager.ComboLevel level)
    {
        switch (level)
        {
            case ComboManager.ComboLevel.Bronze:
                return "BRONZE";
            case ComboManager.ComboLevel.Silver:
                return "SILVER";
            case ComboManager.ComboLevel.Gold:
                return "GOLD";
            case ComboManager.ComboLevel.Platinum:
                return "PLATINUM";
            default:
                return "";
        }
    }

    /// <summary>
    /// Gets the color for a combo level
    /// </summary>
    /// <param name="level">Combo level</param>
    /// <returns>Color for the combo level</returns>
    private Color GetComboLevelColor(ComboManager.ComboLevel level)
    {
        switch (level)
        {
            case ComboManager.ComboLevel.Bronze:
                return bronzeColor;
            case ComboManager.ComboLevel.Silver:
                return silverColor;
            case ComboManager.ComboLevel.Gold:
                return goldColor;
            case ComboManager.ComboLevel.Platinum:
                return platinumColor;
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Gets the waterfall effect description for a combo level
    /// </summary>
    /// <param name="level">Combo level</param>
    /// <returns>Waterfall effect description</returns>
    private string GetWaterfallEffectDescription(ComboManager.ComboLevel level)
    {
        switch (level)
        {
            case ComboManager.ComboLevel.Bronze:
                return "Ripple Effect";
            case ComboManager.ComboLevel.Silver:
                return "Cascade Effect";
            case ComboManager.ComboLevel.Gold:
                return "Waterfall Effect";
            case ComboManager.ComboLevel.Platinum:
                return "Torrent Effect";
            default:
                return "";
        }
    }

    /// <summary>
    /// Gets the threshold for the next combo level
    /// </summary>
    /// <param name="currentLevel">Current combo level</param>
    /// <returns>Threshold for next level, or 0 if at highest level</returns>
    private int GetNextLevelThreshold(ComboManager.ComboLevel currentLevel)
    {
        if (ComboManager.Instance == null) return 0;

        switch (currentLevel)
        {
            case ComboManager.ComboLevel.None:
                return ComboManager.Instance.bronzeLevel;
            case ComboManager.ComboLevel.Bronze:
                return ComboManager.Instance.silverLevel;
            case ComboManager.ComboLevel.Silver:
                return ComboManager.Instance.goldLevel;
            case ComboManager.ComboLevel.Gold:
                return ComboManager.Instance.platinumLevel;
            case ComboManager.ComboLevel.Platinum:
                // For platinum level, we could return a higher value or 0
                return ComboManager.Instance.platinumLevel + 10;
            default:
                return 0;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.onComboIncreased.RemoveListener(OnComboIncreased);
            ComboManager.Instance.onComboLevelChanged.RemoveListener(OnComboLevelChanged);
            ComboManager.Instance.onComboReset.RemoveListener(OnComboReset);
        }
    }
}