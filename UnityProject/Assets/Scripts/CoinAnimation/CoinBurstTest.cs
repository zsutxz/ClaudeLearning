using UnityEngine;

public class CoinBurstTest : MonoBehaviour
{
    public CoinAnimationSystem coinAnimationSystem;
    
    void Start()
    {
        // Example of how to trigger coin burst animations
        if (coinAnimationSystem != null)
        {
            // Start coin burst after 2 seconds
            Invoke("StartCoinBurst", 2f);
        }
    }
    
    void StartCoinBurst()
    {
        coinAnimationSystem.SpawnCoinBurst(
            Vector3.zero, 
            10, 
            3.0f
        );
    }
    
    // Example of how to trigger animations with a UI button
    public void OnBurstButtonClick()
    {
        if (coinAnimationSystem != null)
        {
            coinAnimationSystem.SpawnCoinBurst(
                Vector3.zero, 
                15, 
                2.0f
            );
        }
    }
}