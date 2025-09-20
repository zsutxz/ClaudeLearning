using UnityEngine;
using UnityEngine.UI;

public class CoinBurstDemo : MonoBehaviour
{
    public CoinAnimationSystem coinAnimationSystem;
    public Button singleCoinButton;
    public Button coinBurstButton;
    public Button largeBurstButton;
    public Text infoText;
    
    void Start()
    {
        // Setup button listeners
        if (singleCoinButton != null)
            singleCoinButton.onClick.AddListener(SpawnSingleCoin);
            
        if (coinBurstButton != null)
            coinBurstButton.onClick.AddListener(SpawnCoinBurst);
            
        if (largeBurstButton != null)
            largeBurstButton.onClick.AddListener(SpawnLargeCoinBurst);
            
        UpdateInfoText();
    }
    
    void SpawnSingleCoin()
    {
        if (coinAnimationSystem != null)
        {
            // Spawn a single coin from a random position to the center
            Vector3 startPosition = new Vector3(Random.Range(-3f, 3f), Random.Range(-2f, -1f), 0);
            coinAnimationSystem.SpawnCoinAnimation(startPosition, Vector3.zero);
            
            UpdateInfoText();
        }
    }
    
    void SpawnCoinBurst()
    {
        if (coinAnimationSystem != null)
        {
            // Spawn a burst of 5 coins from the center
            coinAnimationSystem.SpawnCoinBurst(Vector3.zero, 5, 2.0f);
            
            UpdateInfoText();
        }
    }
    
    void SpawnLargeCoinBurst()
    {
        if (coinAnimationSystem != null)
        {
            // Spawn a large burst of 15 coins with callback
            coinAnimationSystem.SpawnCoinBurst(
                new Vector3(0, -1, 0), 
                15, 
                3.0f,
                () => {
                    Debug.Log("Large coin burst completed!");
                    if (infoText != null)
                        infoText.text += "\nLarge burst completed!";
                }
            );
            
            UpdateInfoText();
        }
    }
    
    void UpdateInfoText()
    {
        if (infoText != null && coinAnimationSystem != null)
        {
            int availableCoins, activeCoins, totalCoins;
            coinAnimationSystem.GetPoolStats(out availableCoins, out activeCoins, out totalCoins);
            infoText.text = $"Coins - Available: {availableCoins}, Active: {activeCoins}, Total: {totalCoins}";
        }
    }
    
    void Update()
    {
        // Update stats every 0.5 seconds
        if (Time.frameCount % 30 == 0) // Update every 30 frames (approx. 0.5 seconds at 60 FPS)
        {
            UpdateInfoText();
        }
    }
}