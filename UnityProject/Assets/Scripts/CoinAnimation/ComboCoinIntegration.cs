using UnityEngine;

public class ComboCoinIntegration : MonoBehaviour
{
    [Header("Coin Collection Settings")]
    public float coinCollectionRadius = 1.0f;
    public LayerMask coinLayerMask = 1 << 0; // Default layer

    [Header("Combo Integration")]
    public bool enableComboSystem = true;

    void Update()
    {
        // Example of how to integrate combo system with coin collection
        // This would typically be called when a player collects a coin
        if (Input.GetKeyDown(KeyCode.C))
        {
            CollectCoinAtPosition(transform.position);
        }
    }

    /// <summary>
    /// Collects coins at a specific position and registers with combo system
    /// </summary>
    /// <param name="position">Position to collect coins at</param>
    public void CollectCoinAtPosition(Vector3 position)
    {
        if (enableComboSystem && ComboManager.Instance != null)
        {
            // Register the coin collection with the combo manager
            ComboManager.Instance.RegisterCoinCollection();
        }

        // Collect coins in the area (this would integrate with your existing coin system)
        Collider2D[] coins = Physics2D.OverlapCircleAll(position, coinCollectionRadius, coinLayerMask);
        foreach (Collider2D coinCollider in coins)
        {
            // Trigger coin collection animation
            Coin coin = coinCollider.GetComponent<Coin>();
            if (coin != null)
            {
                // Start the coin animation toward the player/UI
                // In a real implementation, you would animate the coin toward a target
                // For now, we'll just trigger the flying animation to a fixed position
                Vector3 targetPosition = position + Vector3.up * 2;
                coin.Initialize(coinCollider.transform.position, targetPosition);
                coin.StartFlying();
            }
        }
    }

    /// <summary>
    /// Integrates with existing coin collection system
    /// </summary>
    /// <param name="coin">The coin that was collected</param>
    public void OnCoinCollected(Coin coin)
    {
        if (enableComboSystem && ComboManager.Instance != null)
        {
            // Register the coin collection with the combo manager
            ComboManager.Instance.RegisterCoinCollection();
        }

        // Trigger visual and audio effects based on current combo
        if (ComboManager.Instance != null && ComboManager.Instance.GetCurrentCombo() > 1)
        {
            // Combo is active, trigger combo effects
            int comboCount = ComboManager.Instance.GetCurrentCombo();
            // The ComboEffectsManager and ComboAudioManager will automatically
            // handle the visual and audio effects based on the combo count
        }
    }

    /// <summary>
    /// Resets the combo system
    /// </summary>
    public void ResetCombo()
    {
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.ResetCombo();
        }
    }

    /// <summary>
    /// Gets the current combo count
    /// </summary>
    /// <returns>Current combo count</returns>
    public int GetCurrentCombo()
    {
        if (ComboManager.Instance != null)
        {
            return ComboManager.Instance.GetCurrentCombo();
        }
        return 0;
    }

    /// <summary>
    /// Gets the current combo level
    /// </summary>
    /// <returns>Current combo level</returns>
    public ComboManager.ComboLevel GetCurrentComboLevel()
    {
        if (ComboManager.Instance != null)
        {
            return ComboManager.Instance.GetComboLevel(ComboManager.Instance.GetCurrentCombo());
        }
        return ComboManager.ComboLevel.None;
    }

    void OnDrawGizmosSelected()
    {
        // Draw a wire sphere to visualize the coin collection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, coinCollectionRadius);
    }
}