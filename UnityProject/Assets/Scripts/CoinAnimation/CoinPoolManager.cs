using UnityEngine;
using System.Collections.Generic;

public class CoinPoolManager : MonoBehaviour
{
    [Header("Pool Settings")]
    public GameObject coinPrefab;
    public int initialPoolSize = 10;
    
    private Queue<Coin> coinPool = new Queue<Coin>();
    private List<Coin> activeCoins = new List<Coin>();
    
    void Awake()
    {
        InitializePool();
    }
    
    /// <summary>
    /// Initialize pool with coins
    /// </summary>
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewCoin();
        }
    }
    
    /// <summary>
    /// Create a new coin and add to pool
    /// </summary>
    /// <returns>The created coin</returns>
    private Coin CreateNewCoin()
    {
        GameObject coinObj = Instantiate(coinPrefab, transform);
        Coin coin = coinObj.GetComponent<Coin>();
        coinObj.SetActive(false);
        coinPool.Enqueue(coin);
        return coin;
    }
    
    /// <summary>
    /// Get a coin from the pool
    /// </summary>
    /// <returns>Available coin from pool</returns>
    public Coin GetCoin()
    {
        Coin coin;
        if (coinPool.Count > 0)
        {
            coin = coinPool.Dequeue();
        }
        else
        {
            // Create new coin if pool is empty
            coin = CreateNewCoin();
        }
        
        coin.gameObject.SetActive(true);
        activeCoins.Add(coin);
        return coin;
    }
    
    /// <summary>
    /// Return coin to pool
    /// </summary>
    /// <param name="coin">Coin to return</param>
    public void ReturnCoin(Coin coin)
    {
        if (activeCoins.Contains(coin))
        {
            activeCoins.Remove(coin);
            coinPool.Enqueue(coin);
        }
    }
    
    /// <summary>
    /// Get current pool statistics
    /// </summary>
    /// <returns>String with pool information</returns>
    public string GetPoolStats()
    {
        return $"Pool: {coinPool.Count} | Active: {activeCoins.Count}";
    }
}