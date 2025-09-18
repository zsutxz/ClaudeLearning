using UnityEngine;
using System.Collections;

public class CoinAnimationSystem : MonoBehaviour
{
    [Header("System Settings")]
    public GameObject coinPrefab;
    public int initialPoolSize = 10;
    public int maxPoolSize = 50;
    
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