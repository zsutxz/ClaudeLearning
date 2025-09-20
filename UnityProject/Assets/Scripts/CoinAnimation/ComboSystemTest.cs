using UnityEngine;
using UnityEngine.UI;

public class ComboSystemTest : MonoBehaviour
{
    [Header("UI References")]
    public Button collectCoinButton;
    public Text statusText;
    public Text comboCountText;
    public Text comboLevelText;

    void Start()
    {
        // Set up button click listener
        if (collectCoinButton != null)
        {
            collectCoinButton.onClick.AddListener(OnCollectCoinClicked);
        }

        // Subscribe to combo events for testing
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.onComboIncreased.AddListener(OnComboIncreased);
            ComboManager.Instance.onComboLevelChanged.AddListener(OnComboLevelChanged);
            ComboManager.Instance.onComboReset.AddListener(OnComboReset);
        }

        UpdateStatusText("Ready to test combo system. Click 'Collect Coin' to begin.");
    }

    /// <summary>
    /// Called when the collect coin button is clicked
    /// </summary>
    private void OnCollectCoinClicked()
    {
        // Register a coin collection with the combo manager
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.RegisterCoinCollection();
            UpdateStatusText("Coin collected! Combo: " + ComboManager.Instance.GetCurrentCombo());
        }
    }

    /// <summary>
    /// Called when combo is increased
    /// </summary>
    /// <param name="comboCount">Current combo count</param>
    private void OnComboIncreased(int comboCount)
    {
        if (comboCountText != null)
        {
            comboCountText.text = "Combo Count: " + comboCount;
        }

        UpdateStatusText("Combo increased to " + comboCount);
    }

    /// <summary>
    /// Called when combo level changes
    /// </summary>
    /// <param name="comboLevel">New combo level</param>
    private void OnComboLevelChanged(ComboManager.ComboLevel comboLevel)
    {
        if (comboLevelText != null)
        {
            comboLevelText.text = "Combo Level: " + comboLevel.ToString();
        }

        UpdateStatusText("Combo level changed to " + comboLevel.ToString());
    }

    /// <summary>
    /// Called when combo is reset
    /// </summary>
    private void OnComboReset()
    {
        if (comboCountText != null)
        {
            comboCountText.text = "Combo Count: 0";
        }

        if (comboLevelText != null)
        {
            comboLevelText.text = "Combo Level: None";
        }

        UpdateStatusText("Combo reset!");
    }

    /// <summary>
    /// Updates the status text display
    /// </summary>
    /// <param name="message">Status message to display</param>
    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    /// <summary>
    /// Manually reset the combo for testing
    /// </summary>
    public void ResetCombo()
    {
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.ResetCombo();
        }
    }

    /// <summary>
    /// Simulate rapid coin collections for testing combo behavior
    /// </summary>
    public void SimulateRapidCollections(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.RegisterCoinCollection();
            }
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