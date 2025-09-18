using UnityEngine;

public class CoinAnimationTest : MonoBehaviour
{
    [Header("Test Settings")]
    public Vector3 startPosition = new Vector3(-2, 0, 0);
    public Vector3 endPosition = new Vector3(2, 3, 0);
    public int coinCount = 5;
    public float delayBetweenCoins = 0.2f;
    
    void Start()
    {
        // Initialize the coin animation system
        CoinAnimationSystem.Instance.InitializeSystem();
    }
    
    void Update()
    {
        // Press Space to spawn a single coin
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CoinAnimationSystem.Instance.SpawnCoinAnimation(startPosition, endPosition, () => {
                Debug.Log("Single coin animation completed!");
            });
        }
        
        // Press Enter to spawn a sequence of coins
        if (Input.GetKeyDown(KeyCode.Return))
        {
            CoinAnimationSystem.Instance.SpawnCoinSequence(startPosition, endPosition, coinCount, delayBetweenCoins);
            Debug.Log($"Spawned sequence of {coinCount} coins!");
        }
        
        // Press P to show pool statistics
        if (Input.GetKeyDown(KeyCode.P))
        {
            CoinAnimationSystem.Instance.GetPoolStats(out int available, out int active, out int total);
            Debug.Log($"Coin Pool - Available: {available}, Active: {active}, Total: {total}");
        }
    }
}