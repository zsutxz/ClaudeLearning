using UnityEngine;

public class CoinAnimationSystem : MonoBehaviour
{
    public CoinPoolManager poolManager;
    
    /// <summary>
    /// Spawn a coin animation with parameters
    /// </summary>
    /// <param name="parameters">Animation parameters</param>
    public void SpawnCoinAnimation(CoinAnimationParams parameters)
    {
        if (poolManager == null) 
        {
            Debug.LogError("CoinPoolManager is not assigned!");
            return;
        }
        
        Coin coin = poolManager.GetCoin();
        coin.Initialize(parameters, poolManager.ReturnCoin);
        coin.StartFlying();
    }
    
    /// <summary>
    /// Convenience method to spawn coin animation with start and end positions
    /// </summary>
    /// <param name="start">Start position</param>
    /// <param name="end">End position</param>
    public void SpawnCoinAnimation(Vector3 start, Vector3 end)
    {
        CoinAnimationParams parameters = new CoinAnimationParams
        {
            duration = 1.0f,
            startPosition = start,
            endPosition = end,
            enableRotation = true,
            rotationSpeed = 360f,
            enableScaling = true,
            startScale = 1.0f,
            endScale = 0.5f,
            easeType = Ease.OutQuad
        };
        
        SpawnCoinAnimation(parameters);
    }
    
    /// <summary>
    /// Spawn multiple coins in a burst pattern
    /// </summary>
    /// <param name="start">Start position</param>
    /// <param name="count">Number of coins</param>
    /// <param name="radius">Radius of burst</param>
    public void SpawnCoinBurst(Vector3 start, int count, float radius = 2.0f)
    {
        for (int i = 0; i < count; i++)
        {
            // Random end position in a circle around start
            Vector3 end = start + new Vector3(
                Mathf.Cos((float)i / count * Mathf.PI * 2) * radius,
                Mathf.Sin((float)i / count * Mathf.PI * 2) * radius,
                0
            );
            
            SpawnCoinAnimation(start, end);
        }
    }
}