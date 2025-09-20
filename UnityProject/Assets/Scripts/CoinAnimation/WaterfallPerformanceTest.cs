using UnityEngine;
using System.Collections;

public class WaterfallPerformanceTest : MonoBehaviour
{
    [Header("Test Settings")]
    public int testCoinCount = 20;
    public float testDuration = 10.0f;
    public bool runPerformanceTest = false;
    
    [Header("Test References")]
    public Transform testOrigin;
    public Transform testTarget;
    
    private bool isTestRunning = false;
    private float testStartTime = 0f;
    
    void Update()
    {
        if (runPerformanceTest && !isTestRunning)
        {
            StartCoroutine(RunPerformanceTest());
            runPerformanceTest = false;
        }
    }
    
    /// <summary>
    /// Runs a performance test with multiple waterfall effects
    /// </summary>
    private IEnumerator RunPerformanceTest()
    {
        isTestRunning = true;
        testStartTime = Time.time;
        
        Debug.Log($"Starting waterfall performance test with {testCoinCount} coins for {testDuration} seconds");
        
        // Run the test for the specified duration
        while (Time.time - testStartTime < testDuration)
        {
            // Trigger multiple waterfall cascades
            for (int i = 0; i < 3; i++)
            {
                Vector3 randomOrigin = testOrigin.position + new Vector3(Random.Range(-2f, 2f), Random.Range(-1f, 1f), 0);
                Vector3 randomTarget = testTarget.position + new Vector3(Random.Range(-1f, 1f), Random.Range(1f, 3f), 0);
                
                if (CoinAnimationSystem.Instance != null)
                {
                    CoinAnimationSystem.Instance.TriggerWaterfallCascade(randomOrigin, randomTarget);
                }
                
                yield return new WaitForSeconds(0.1f);
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("Waterfall performance test completed");
        isTestRunning = false;
    }
}