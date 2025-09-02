using UnityEngine;
using UnityEngine.UI;

public class CoinAnimationValidation : MonoBehaviour
{
    public CoinAnimationSystem coinAnimationSystem;
    public Text poolStatsText;
    public Button singleCoinButton;
    public Button burstCoinButton;
    public int burstCount = 5;
    
    private int coinSpawnCount = 0;
    
    void Start()
    {
        if (singleCoinButton != null)
            singleCoinButton.onClick.AddListener(SpawnSingleCoin);
            
        if (burstCoinButton != null)
            burstCoinButton.onClick.AddListener(SpawnCoinBurst);
            
        UpdatePoolStats();
    }
    
    void SpawnSingleCoin()
    {
        if (coinAnimationSystem != null && coinAnimationSystem.poolManager != null)
        {
            Vector3 start = new Vector3(-3 + (coinSpawnCount % 7), -2 + (coinSpawnCount % 5), 0);
            Vector3 end = new Vector3(3 - (coinSpawnCount % 7), 2 - (coinSpawnCount % 5), 0);
            
            coinAnimationSystem.SpawnCoinAnimation(start, end);
            coinSpawnCount++;
            
            UpdatePoolStats();
        }
    }
    
    void SpawnCoinBurst()
    {
        if (coinAnimationSystem != null && coinAnimationSystem.poolManager != null)
        {
            coinAnimationSystem.SpawnCoinBurst(Vector3.zero, burstCount, 2.0f);
            coinSpawnCount += burstCount;
            
            UpdatePoolStats();
        }
    }
    
    void UpdatePoolStats()
    {
        if (coinAnimationSystem != null && coinAnimationSystem.poolManager != null && poolStatsText != null)
        {
            poolStatsText.text = coinAnimationSystem.poolManager.GetPoolStats();
        }
    }
    
    void Update()
    {
        // Update stats every frame for real-time monitoring
        if (Time.frameCount % 30 == 0) // Update every 30 frames
        {
            UpdatePoolStats();
        }
    }
}