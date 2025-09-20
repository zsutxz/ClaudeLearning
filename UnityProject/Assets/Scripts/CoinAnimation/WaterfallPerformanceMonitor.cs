using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

public class WaterfallPerformanceMonitor : MonoBehaviour
{
    [Header("Performance Settings")]
    public bool enableMonitoring = true;
    public float updateInterval = 1.0f;
    
    [Header("UI References")]
    public Text performanceText;
    
    [Header("Performance Thresholds")]
    public float targetFrameRate = 60.0f;
    public int maxActiveCoins = 50;
    public int maxParticles = 1000;
    
    private float lastUpdateTime = 0f;
    private float frameRate = 0f;
    private int frames = 0;
    
    private StringBuilder performanceStringBuilder = new StringBuilder();
    
    void Update()
    {
        if (!enableMonitoring) return;
        
        // Calculate frame rate
        frames++;
        float timeSinceLastUpdate = Time.time - lastUpdateTime;
        
        if (timeSinceLastUpdate >= updateInterval)
        {
            frameRate = frames / timeSinceLastUpdate;
            frames = 0;
            lastUpdateTime = Time.time;
            
            UpdatePerformanceDisplay();
        }
    }
    
    /// <summary>
    /// Updates the performance display with current metrics
    /// </summary>
    private void UpdatePerformanceDisplay()
    {
        if (performanceText == null) return;
        
        performanceStringBuilder.Clear();
        
        // Add frame rate information
        performanceStringBuilder.AppendLine($"FPS: {frameRate:F1} ({(frameRate >= targetFrameRate ? "✓" : "✗")})");
        
        // Add coin pool statistics
        if (CoinPoolManager.Instance != null)
        {
            int availableCount, activeCount, totalCount;
            CoinPoolManager.Instance.GetPoolStats(out availableCount, out activeCount, out totalCount);
            performanceStringBuilder.AppendLine($"Coins: {activeCount}/{totalCount} ({(activeCount <= maxActiveCoins ? "✓" : "✗")})");
        }
        
        // Add particle system information
        if (WaterfallEffectsManager.Instance != null && 
            WaterfallEffectsManager.Instance.waterfallParticles != null)
        {
            var particles = new ParticleSystem.Particle[WaterfallEffectsManager.Instance.waterfallParticles.main.maxParticles];
            int particleCount = WaterfallEffectsManager.Instance.waterfallParticles.GetParticles(particles);
            performanceStringBuilder.AppendLine($"Particles: {particleCount} ({(particleCount <= maxParticles ? "✓" : "✗")})");
        }
        
        // Add memory information
        long monoUsedSize = Profiler.GetMonoUsedSizeLong();
        long totalReservedSize = Profiler.GetTotalReservedMemoryLong();
        performanceStringBuilder.AppendLine($"Memory: {FormatBytes(monoUsedSize)}/{FormatBytes(totalReservedSize)}");
        
        // Add performance warnings if needed
        if (frameRate < targetFrameRate * 0.8f)
        {
            performanceStringBuilder.AppendLine("<color=red>WARNING: Low frame rate detected!</color>");
        }
        
        if (CoinPoolManager.Instance != null)
        {
            int availableCount, activeCount, totalCount;
            CoinPoolManager.Instance.GetPoolStats(out availableCount, out activeCount, out totalCount);
            if (activeCount > maxActiveCoins * 0.8f)
            {
                performanceStringBuilder.AppendLine("<color=yellow>WARNING: High coin count!</color>");
            }
        }
        
        performanceText.text = performanceStringBuilder.ToString();
    }
    
    /// <summary>
    /// Formats bytes into a human-readable string
    /// </summary>
    /// <param name="bytes">Number of bytes</param>
    /// <returns>Formatted string</returns>
    private string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        float number = (float)bytes;
        while (Mathf.Round(number / 1024) >= 1)
        {
            number = number / 1024;
            counter++;
        }
        return string.Format("{0:N1}{1}", number, suffixes[counter]);
    }
}