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

    // Initialize pool with coins
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewCoin();
        }
    }

    // Create a new coin and add to pool
    private Coin CreateNewCoin()
    {
        GameObject coinObj = Instantiate(coinPrefab, transform);
        Coin coin = coinObj.GetComponent<Coin>();
        coinObj.SetActive(false);
        coinPool.Enqueue(coin);
        return coin;
    }

    // Get a coin from the pool
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

    // Return coin to pool
    public void ReturnCoin(Coin coin)
    {
        if (activeCoins.Contains(coin))
        {
            activeCoins.Remove(coin);
            coinPool.Enqueue(coin);
        }
    }

    // Get count of available coins in pool
    public int AvailableCoinsCount()
    {
        return coinPool.Count;
    }

    // Get count of active coins
    public int ActiveCoinsCount()
    {
        return activeCoins.Count;
    }
}