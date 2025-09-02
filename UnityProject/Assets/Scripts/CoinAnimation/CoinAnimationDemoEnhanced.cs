using UnityEngine;

public class CoinAnimationDemoEnhanced : MonoBehaviour
{
    public CoinAnimationSystem coinAnimationSystem;
    
    void Start()
    {
        // Example of how to trigger coin animations
        if (coinAnimationSystem != null)
        {
            // Start single coin animation after 2 seconds
            Invoke("StartSingleCoin", 2f);
            
            // Start coin burst after 4 seconds
            Invoke("StartCoinBurst", 4f);
        }
    }
    
    void StartSingleCoin()
    {
        
        coinAnimationSystem.SpawnCoinAnimation(
            new Vector3(-2, 0, 0), 
            new Vector3(2, 2, 0)
        );
    }
    
    void StartCoinBurst()
    {
        coinAnimationSystem.SpawnCoinBurst(
            Vector3.zero, 
            5, 
            3.0f
        );
    }
    
    // Example of how to trigger animations with a UI button
    public void OnSingleCoinButtonClick()
    {
        if (coinAnimationSystem != null)
        {
            coinAnimationSystem.SpawnCoinAnimation(
                new Vector3(-2, 0, 0), 
                new Vector3(2, 2, 0)
            );
        }
    }
    
    public void OnBurstButtonClick()
    {
        if (coinAnimationSystem != null)
        {
            coinAnimationSystem.SpawnCoinBurst(
                Vector3.zero, 
                8, 
                2.0f
            );
        }
    }
}