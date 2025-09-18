using UnityEngine;
using System.Collections.Generic;

public class CoinPoolManager : MonoBehaviour
{
    [Header("Pool Settings")]
    public GameObject coinPrefab;
    public int initialPoolSize = 10;
    public int maxPoolSize = 50;
    
    private Queue<Coin> availableCoins;
    private List<Coin> activeCoins;
    private static CoinPoolManager instance;
    
    public static CoinPoolManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject poolManagerObject = new GameObject("CoinPoolManager");
                instance = poolManagerObject.AddComponent<CoinPoolManager>();
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
            InitializePool();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initializes the coin pool with the specified number of coins
    /// </summary>
    private void InitializePool()
    {
        availableCoins = new Queue<Coin>();
        activeCoins = new List<Coin>();
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewCoin();
        }
    }
    
    /// <summary>
    /// Creates a new coin and adds it to the available pool
    /// </summary>
    /// <returns>The created coin</returns>
    private Coin CreateNewCoin()
    {
        if (coinPrefab == null)
        {
            Debug.LogError("Coin prefab is not assigned in CoinPoolManager");
            return null;
        }
        
        GameObject coinObject = Instantiate(coinPrefab);
        Coin coin = coinObject.GetComponent<Coin>();
        if (coin == null)
        {
            coin = coinObject.AddComponent<Coin>();
        }
        
        coinObject.SetActive(false);
        availableCoins.Enqueue(coin);
        return coin;
    }
    
    /// <summary>
    /// Gets a coin from the pool
    /// </summary>
    /// <returns>A coin from the pool, or null if pool is at maximum capacity</returns>
    public Coin GetCoin()
    {
        Coin coin;
        
        if (availableCoins.Count > 0)
        {
            coin = availableCoins.Dequeue();
        }
        else if (activeCoins.Count + availableCoins.Count < maxPoolSize)
        {
            coin = CreateNewCoin();
        }
        else
        {
            Debug.LogWarning("Coin pool is at maximum capacity. Consider increasing maxPoolSize.");
            return null;
        }
        
        coin.gameObject.SetActive(true);
        activeCoins.Add(coin);
        return coin;
    }
    
    /// <summary>
    /// Returns a coin to the pool
    /// </summary>
    /// <param name="coin">The coin to return</param>
    public void ReturnCoin(Coin coin)
    {
        if (coin == null) return;
        
        if (activeCoins.Remove(coin))
        {
            coin.ResetCoin();
            availableCoins.Enqueue(coin);
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
        availableCount = availableCoins.Count;
        activeCount = activeCoins.Count;
        totalCount = availableCount + activeCount;
    }
    
    /// <summary>
    /// Clears the pool and destroys all coins
    /// </summary>
    public void ClearPool()
    {
        foreach (Coin coin in availableCoins)
        {
            if (coin != null && coin.gameObject != null)
            {
                Destroy(coin.gameObject);
            }
        }
        
        foreach (Coin coin in activeCoins)
        {
            if (coin != null && coin.gameObject != null)
            {
                Destroy(coin.gameObject);
            }
        }
        
        availableCoins.Clear();
        activeCoins.Clear();
    }
    
    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}