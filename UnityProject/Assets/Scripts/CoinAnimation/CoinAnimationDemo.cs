using UnityEngine;

public class CoinAnimationDemo : MonoBehaviour
{
    public CoinAnimationController coinAnimationController;
    
    void Start()
    {
        // Example of how to trigger the coin animation
        if (coinAnimationController != null)
        {
            // Start the animation after 2 seconds
            Invoke("StartCoinAnimation", 2f);
        }
    }
    
    void StartCoinAnimation()
    {
        coinAnimationController.StartCoinAnimation();
    }
    
    // Example of how to trigger the animation with a UI button
    public void OnUIButtonClick()
    {
        if (coinAnimationController != null)
        {
            coinAnimationController.StartCoinAnimation();
        }
    }
}