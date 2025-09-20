using UnityEngine;
using UnityEngine.UI;

public class PerformanceMonitor : MonoBehaviour
{
    [Header("Performance Settings")]
    public CoinPoolManager coinPoolManager;
    public Text fpsText;
    public Text poolStatusText;

    private float deltaTime = 0.0f;
    private int frameCount = 0;
    private float frameRate = 0.0f;

    void Update()
    {
        // Calculate FPS
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        frameCount++;

        if (frameCount >= 30) // Update every 30 frames
        {
            frameRate = 1.0f / deltaTime;
            frameCount = 0;

            // Update FPS display
            if (fpsText != null)
            {
                fpsText.text = $"FPS: {Mathf.Clamp(frameRate, 0, 999):F1}";
            }

            // Update pool status display
            if (coinPoolManager != null && poolStatusText != null)
            {
                poolStatusText.text = $"Pool - Available: {coinPoolManager.AvailableCoinsCount()}, Active: {coinPoolManager.ActiveCoinsCount()}";
            }
        }
    }

    // Method to log performance events
    public void LogPerformanceEvent(string eventName)
    {
        Debug.Log($"[Performance] {eventName} at {Time.time:F2}s, FPS: {frameRate:F1}");
    }

    // Method to check if performance is acceptable
    public bool IsPerformanceAcceptable()
    {
        return frameRate >= 30.0f; // Minimum 30 FPS
    }

    // Method to get current FPS
    public float GetCurrentFPS()
    {
        return frameRate;
    }
}