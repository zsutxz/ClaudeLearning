using UnityEngine;
using System.Collections.Generic;

public class CoinAnimationSystem : MonoBehaviour
{
    public CoinPoolManager poolManager;
    public float cascadeRadius = 3.0f;  // Radius to search for nearby coins
    public float cascadeDelay = 0.1f;   // Delay between cascade activations
    public int maxCascadeCoins = 10;    // Maximum number of coins in cascade

    // Spawn a coin animation with parameters
    public void SpawnCoinAnimation(CoinAnimationParams parameters)
    {
        if (poolManager == null) return;

        Coin coin = poolManager.GetCoin();
        coin.Initialize(parameters, poolManager.ReturnCoin);
        coin.StartFlying();
    }

    // Convenience method with positions
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
            easeType = Ease.OutQuad,
            isCascadeEffect = false,
            cascadeDelay = 0f
        };

        SpawnCoinAnimation(parameters);
    }

    // Spawn a coin with cascade effect
    public void SpawnCoinWithCascade(Vector3 start, Vector3 end)
    {
        // Spawn the initial coin with regular animation
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
            easeType = Ease.OutQuad,
            isCascadeEffect = false,
            cascadeDelay = 0f
        };

        SpawnCoinAnimation(parameters);

        // Trigger cascade effect on nearby coins
        TriggerCascadeEffect(end);
    }

    // Trigger cascade effect on nearby coins
    private void TriggerCascadeEffect(Vector3 collectionPoint)
    {
        // In a full implementation, this would find nearby coins and trigger their animations
        // with delays to create the ripple effect
        // For now, we'll simulate this with a simple approach

        // Find nearby coins (in a real implementation, you would have a collection of coins in the scene)
        List<Vector3> nearbyCoinPositions = GenerateNearbyCoinPositions(collectionPoint, cascadeRadius, maxCascadeCoins);

        // Trigger cascade animations with delays
        for (int i = 0; i < nearbyCoinPositions.Count; i++)
        {
            Vector3 nearbyCoinPos = nearbyCoinPositions[i];
            float delay = (i + 1) * cascadeDelay;

            // Schedule cascade coin animation
            ScheduleCascadeCoinAnimation(nearbyCoinPos, collectionPoint, delay);
        }
    }

    // Generate positions for nearby coins (simulation)
    private List<Vector3> GenerateNearbyCoinPositions(Vector3 center, float radius, int count)
    {
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < count; i++)
        {
            // Generate random position within radius
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 position = center + new Vector3(randomCircle.x, randomCircle.y, 0);

            // Add some variation in height to simulate waterfall effect
            position.y += Random.Range(-radius * 0.5f, radius * 0.5f);

            positions.Add(position);
        }

        return positions;
    }

    // Schedule a cascade coin animation with delay
    private void ScheduleCascadeCoinAnimation(Vector3 start, Vector3 end, float delay)
    {
        // Create parameters for cascade effect
        CoinAnimationParams parameters = new CoinAnimationParams
        {
            duration = 0.8f,  // Slightly faster for cascade effect
            startPosition = start,
            endPosition = end,
            enableRotation = true,
            rotationSpeed = 360f,
            enableScaling = true,
            startScale = 0.8f,
            endScale = 0.3f,
            easeType = Ease.OutQuart,
            isCascadeEffect = true,
            cascadeDelay = delay
        };

        // Use a coroutine to delay the animation start
        StartCoroutine(SpawnDelayedCoinAnimation(parameters, delay));
    }

    // Coroutine to spawn delayed coin animation
    private System.Collections.IEnumerator SpawnDelayedCoinAnimation(CoinAnimationParams parameters, float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnCoinAnimation(parameters);
    }

    // Spawn a burst of coins from a center position
    public void SpawnCoinBurst(Vector3 centerPosition, int count, float radius = 2.0f, System.Action onComplete = null)
    {
        if (poolManager == null) return;

        int completedCount = 0;

        for (int i = 0; i < count; i++)
        {
            // Calculate random start position within the radius
            Vector3 randomOffset = Random.insideUnitCircle * radius;
            Vector3 startPosition = centerPosition + new Vector3(randomOffset.x, randomOffset.y, 0);

            // Calculate random end position within a smaller radius around the center
            Vector3 endOffset = Random.insideUnitCircle * (radius * 0.5f);
            Vector3 endPosition = centerPosition + new Vector3(endOffset.x, endOffset.y, 0);

            // Add some upward movement to make it more natural
            endPosition.y += Random.Range(0.5f, 1.5f);

            SpawnCoinAnimation(startPosition, endPosition);
        }
    }
}