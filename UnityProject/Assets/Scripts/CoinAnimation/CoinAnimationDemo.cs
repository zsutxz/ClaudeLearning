using UnityEngine;

public class CoinAnimationDemo : MonoBehaviour
{
    public CoinAnimationSystem coinAnimationSystem;
    public Transform collectionPoint;  // The point where coins are collected

    void Start()
    {
        if (coinAnimationSystem == null)
        {
            coinAnimationSystem = FindObjectOfType<CoinAnimationSystem>();
        }

        if (collectionPoint == null)
        {
            collectionPoint = transform;
        }
    }

    void Update()
    {
        // Press Space to spawn a coin with cascade effect
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 randomStart = new Vector3(
                Random.Range(-5f, 5f),
                Random.Range(5f, 10f),
                0
            );

            coinAnimationSystem.SpawnCoinWithCascade(randomStart, collectionPoint.position);
        }

        // Press C to spawn a coin burst
        if (Input.GetKeyDown(KeyCode.C))
        {
            coinAnimationSystem.SpawnCoinBurst(collectionPoint.position, 5, 3.0f);
        }

        // Press R to spawn a regular coin
        if (Input.GetKeyDown(KeyCode.R))
        {
            Vector3 randomStart = new Vector3(
                Random.Range(-5f, 5f),
                Random.Range(5f, 10f),
                0
            );

            coinAnimationSystem.SpawnCoinAnimation(randomStart, collectionPoint.position);
        }
    }
}