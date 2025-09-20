using UnityEngine;
using System.Collections;

public class CoinAnimationSystem : MonoBehaviour
{
    [Header("System Settings")]
    public GameObject coinPrefab;
    public int initialPoolSize = 10;
    public int maxPoolSize = 50;
    
    [Header("Waterfall Cascade Settings")]
    public float cascadeRadius = 3.0f;
    public LayerMask coinLayerMask = 1 << 0; // Default layer
    public int maxCascadeCoins = 10;
    
    private CoinPoolManager coinPoolManager;
    private static CoinAnimationSystem instance;
    
    public static CoinAnimationSystem Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject systemObject = new GameObject("CoinAnimationSystem");
                instance = systemObject.AddComponent<CoinAnimationSystem>();
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSystem();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initializes the coin animation system
    /// </summary>
    private void InitializeSystem()
    {
        // Create or get the coin pool manager
        GameObject poolManagerObject = new GameObject("CoinPoolManager");
        coinPoolManager = poolManagerObject.AddComponent<CoinPoolManager>();
        coinPoolManager.coinPrefab = coinPrefab;
        coinPoolManager.initialPoolSize = initialPoolSize;
        coinPoolManager.maxPoolSize = maxPoolSize;
    }
    
    /// <summary>
    /// Spawns a coin animation from start to end position
    /// </summary>
    /// <param name="startPosition">Start position of the animation</param>
    /// <param name="endPosition">End position of the animation</param>
    /// <param name="onComplete">Callback when animation completes</param>
    public void SpawnCoinAnimation(Vector3 startPosition, Vector3 endPosition, System.Action onComplete = null)
    {
        if (coinPoolManager == null)
        {
            Debug.LogError("CoinPoolManager is not initialized");
            return;
        }
        
        Coin coin = coinPoolManager.GetCoin();
        if (coin != null)
        {
            coin.Initialize(startPosition, endPosition);
            coin.StartFlying((returnedCoin) => {
                coinPoolManager.ReturnCoin(returnedCoin);
                onComplete?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("Could not get coin from pool");
            onComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Spawns multiple coin animations in sequence
    /// </summary>
    /// <param name="startPosition">Start position of the animations</param>
    /// <param name="endPosition">End position of the animations</param>
    /// <param name="count">Number of coins to spawn</param>
    /// <param name="delay">Delay between each coin spawn</param>
    public void SpawnCoinSequence(Vector3 startPosition, Vector3 endPosition, int count, float delay = 0.1f)
    {
        StartCoroutine(SpawnCoinSequenceCoroutine(startPosition, endPosition, count, delay));
    }
    
    private IEnumerator SpawnCoinSequenceCoroutine(Vector3 startPosition, Vector3 endPosition, int count, float delay)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnCoinAnimation(startPosition, endPosition);
            if (i < count - 1) // Don't wait after the last coin
            {
                yield return new WaitForSeconds(delay);
            }
        }
    }
    
    /// <summary>
    /// Spawns a burst of coins from a center position
    /// </summary>
    /// <param name="centerPosition">Center position to spawn coins from</param>
    /// <param name="count">Number of coins to spawn</param>
    /// <param name="radius">Radius of the spawn area</param>
    /// <param name="onComplete">Callback when all animations complete</param>
    public void SpawnCoinBurst(Vector3 centerPosition, int count, float radius = 2.0f, System.Action onComplete = null)
    {
        if (coinPoolManager == null)
        {
            Debug.LogError("CoinPoolManager is not initialized");
            return;
        }
        
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
            
            SpawnCoinAnimation(startPosition, endPosition, () => {
                completedCount++;
                if (completedCount >= count)
                {
                    onComplete?.Invoke();
                }
            });
        }
    }
    
    /// <summary>
    /// Triggers a waterfall cascade effect from a collection point
    /// </summary>
    /// <param name="collectionPoint">Point where the initial coin was collected</param>
    /// <param name="targetPosition">Target position for collected coins</param>
    public void TriggerWaterfallCascade(Vector3 collectionPoint, Vector3 targetPosition)
    {
        if (coinPoolManager == null)
        {
            Debug.LogError("CoinPoolManager is not initialized");
            return;
        }
        
        // Get the current combo tier for intensity
        ComboManager.ComboLevel tier = ComboManager.ComboLevel.None;
        if (ComboManager.Instance != null)
        {
            int comboCount = ComboManager.Instance.GetCurrentCombo();
            tier = ComboManager.Instance.GetComboLevel(comboCount);
        }
        
        // Trigger waterfall effects based on tier
        if (WaterfallEffectsManager.Instance != null)
        {
            WaterfallEffectsManager.Instance.TriggerWaterfallEffect(tier);
        }
        
        // Find nearby coins to cascade
        Collider2D[] nearbyCoins = Physics2D.OverlapCircleAll(collectionPoint, cascadeRadius, coinLayerMask);
        int cascadeCount = 0;
        
        foreach (Collider2D coinCollider in nearbyCoins)
        {
            // Limit the number of cascading coins
            if (cascadeCount >= maxCascadeCoins)
                break;
            
            // Get the coin component
            Coin nearbyCoin = coinCollider.GetComponent<Coin>();
            if (nearbyCoin != null && nearbyCoin.gameObject.activeInHierarchy)
            {
                // Skip if this is the original collected coin
                if (nearbyCoin.transform.position == collectionPoint)
                    continue;
                
                // Calculate curve height and delay based on distance and tier
                float distance = Vector3.Distance(nearbyCoin.transform.position, collectionPoint);
                float curveHeight = WaterfallEffectsManager.Instance != null ? 
                    WaterfallEffectsManager.Instance.GetCurveHeight(cascadeCount) : 1.0f;
                float delay = WaterfallEffectsManager.Instance != null ? 
                    WaterfallEffectsManager.Instance.GetCascadeDelay(cascadeCount) : 0.05f;
                
                // Get a coin from the pool for the cascade effect
                Coin cascadeCoin = coinPoolManager.GetCoin();
                if (cascadeCoin != null)
                {
                    // Initialize and start the waterfall animation
                    cascadeCoin.Initialize(nearbyCoin.transform.position, targetPosition);
                    cascadeCoin.StartWaterfallFlying((returnedCoin) => {
                        coinPoolManager.ReturnCoin(returnedCoin);
                    }, curveHeight, delay);
                    
                    cascadeCount++;
                }
            }
        }
    }
    
    /// <summary>
    /// Gets the current pool statistics
    /// </summary>
    /// <param name="availableCount">Number of available coins</param>
    /// <param name="activeCount">Number of active coins</param>
    /// <param name="totalCount">Total number of coins in pool</param>
    public void GetPoolStats(out int availableCount, out int activeCount, out int totalCount)
    {
        if (coinPoolManager != null)
        {
            coinPoolManager.GetPoolStats(out availableCount, out activeCount, out totalCount);
        }
        else
        {
            availableCount = activeCount = totalCount = 0;
        }
    }
    
    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}