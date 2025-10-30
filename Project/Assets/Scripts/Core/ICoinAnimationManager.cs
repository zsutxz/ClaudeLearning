using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoinAnimation.Core
{
    /// <summary>
    /// Central coordination of all coin animations, performance monitoring, and system lifecycle management
    /// Interface defined in Story Context XML for Epic 1 Story 2
    /// </summary>
    public interface ICoinAnimationManager
    {
        /// <summary>
        /// Initializes the coin animation system with specified configuration
        /// </summary>
        /// <param name="configuration">System configuration parameters</param>
        void Initialize(CoinAnimationConfiguration configuration);
        
        /// <summary>
        /// Starts coin animation sequence for specified target
        /// </summary>
        /// <param name="target">Transform target for coin animation</param>
        /// <param name="coinCount">Number of coins to animate</param>
        /// <returns>Animation session ID</returns>
        Guid StartCoinAnimation(Transform target, int coinCount = 1);
        
        /// <summary>
        /// Stops ongoing coin animation session
        /// </summary>
        /// <param name="sessionId">Animation session identifier</param>
        void StopCoinAnimation(Guid sessionId);
        
        /// <summary>
        /// Gets current performance metrics for the animation system
        /// </summary>
        /// <returns>Performance metrics data</returns>
        PerformanceMetrics GetPerformanceMetrics();
        
        /// <summary>
        /// System lifecycle management - cleans up resources
        /// </summary>
        void Cleanup();
        
        /// <summary>
        /// Event triggered when animation session starts
        /// </summary>
        event Action<Guid> OnAnimationStarted;
        
        /// <summary>
        /// Event triggered when animation session completes
        /// </summary>
        event Action<Guid> OnAnimationCompleted;
        
        /// <summary>
        /// Event triggered when system performance threshold is exceeded
        /// </summary>
        event Action<PerformanceMetrics> OnPerformanceThresholdExceeded;
    }
    
    /// <summary>
    /// Configuration structure for coin animation system
    /// </summary>
    [Serializable]
    public class CoinAnimationConfiguration
    {
        [Header("Performance Settings")]
        public int maxConcurrentAnimations = 10;
        public float targetFrameRate = 60f;
        public bool enablePerformanceMonitoring = true;
        
        [Header("Animation Settings")]
        public float defaultAnimationDuration = 2f;
        public AnimationEasingType defaultEasing = AnimationEasingType.EaseOut;
        public bool enableMagneticCollection = true;
        
        [Header("Quality Settings")]
        public AnimationQuality quality = AnimationQuality.High;
        public bool enableLOD = true;
        public float lodDistanceThreshold = 10f;
    }
    
    /// <summary>
    /// Performance metrics for monitoring system performance
    /// </summary>
    [Serializable]
    public class PerformanceMetrics
    {
        public float currentFrameRate;
        public int activeAnimations;
        public float memoryUsage;
        public float cpuUsage;
        public bool isPerformanceOptimal;
        public DateTime timestamp;
    }
    
    /// <summary>
    /// Animation quality levels
    /// </summary>
    public enum AnimationQuality
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Ultra = 3
    }
    
    /// <summary>
    /// Animation easing types
    /// </summary>
    public enum AnimationEasingType
    {
        Linear = 0,
        EaseIn = 1,
        EaseOut = 2,
        EaseInOut = 3,
        Bounce = 4,
        Elastic = 5
    }
}