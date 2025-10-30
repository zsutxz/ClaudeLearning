using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using CoinAnimation.Core;
using CoinAnimation.Animation;

namespace CoinAnimation.Physics
{
    /// <summary>
    /// Advanced spiral motion controller for dynamic coin trajectories
    /// Creates mesmerizing spiral patterns with configurable parameters
    /// </summary>
    public class SpiralMotionController : MonoBehaviour
    {
        #region Configuration
        
        [Header("Spiral Parameters")]
        [SerializeField, Tooltip("Base spiral radius")]
        private float baseRadius = 2f;
        
        [SerializeField, Tooltip("Radius compression over time")]
        private float radiusCompression = 0.8f;
        
        [SerializeField, Tooltip("Vertical movement component")]
        private float verticalAmplitude = 0.5f;
        
        [SerializeField, Tooltip("Spiral rotation speed")]
        private float rotationSpeed = 2f;

        [Header("Dynamic Intensity")]
        [SerializeField, Tooltip("Enable distance-based intensity")]
        private bool enableDistanceIntensity = true;
        
        [SerializeField, Tooltip("Intensity falloff start distance")]
        private float intensityStartDistance = 3f;
        
        [SerializeField, Tooltip("Intensity falloff end distance")]
        private float intensityEndDistance = 8f;

        [Header("Noise and Turbulence")]
        [SerializeField, Tooltip("Perlin noise scale for organic movement")]
        private float noiseScale = 0.1f;
        
        [SerializeField, Tooltip("Noise strength multiplier")]
        private float noiseStrength = 0.3f;
        
        [SerializeField, Tooltip("Turbulence frequency")]
        private float turbulenceFrequency = 5f;

        [Header("Performance")]
        [SerializeField, Tooltip("Maximum concurrent spiral animations")]
        private int maxConcurrentSpirals = 50;
        
        [SerializeField, Tooltip("Spiral update rate (Hz)")]
        private int updateRate = 60;

        #endregion

        #region Private Fields
        
        private readonly Dictionary<int, SpiralAnimation> _activeSpirals = new Dictionary<int, SpiralAnimation>();
        private readonly List<Vector3> _spiralPath = new List<Vector3>();
        private float _timeOffset = 0f;
        private bool _isInitialized = false;

        #endregion

        #region Properties
        
        /// <summary>
        /// Number of currently active spiral animations
        /// </summary>
        public int ActiveSpiralCount => _activeSpirals.Count;
        
        /// <summary>
        /// Is the controller at maximum capacity
        /// </summary>
        public bool IsAtCapacity => _activeSpirals.Count >= maxConcurrentSpirals;

        #endregion

        #region Initialization
        
        private void Awake()
        {
            InitializeController();
        }

        /// <summary>
        /// Initialize the spiral motion controller
        /// </summary>
        private void InitializeController()
        {
            if (_isInitialized) return;
            
            // Randomize time offset for variety
            _timeOffset = Random.Range(0f, 1000f);
            
            _isInitialized = true;
        }

        #endregion

        #region Spiral Animation Control
        
        /// <summary>
        /// Start spiral animation for a coin moving toward target
        /// </summary>
        /// <param name="coinController">Coin to animate</param>
        /// <param name="targetPosition">Target collection position</param>
        /// <param name="duration">Animation duration</param>
        /// <param name="spiralType">Type of spiral pattern</param>
        /// <returns>True if spiral was started successfully</returns>
        public bool StartSpiralAnimation(CoinAnimationController coinController, Vector3 targetPosition, 
            float duration, SpiralType spiralType = SpiralType.Helix)
        {
            if (coinController == null || IsAtCapacity) return false;
            
            int coinId = coinController.CoinId;
            
            // Remove existing spiral for this coin
            if (_activeSpirals.ContainsKey(coinId))
            {
                StopSpiralAnimation(coinId);
            }
            
            // Create new spiral animation
            var spiral = new SpiralAnimation
            {
                CoinId = coinId,
                CoinController = coinController,
                TargetPosition = targetPosition,
                StartTime = Time.time,
                Duration = duration,
                SpiralType = spiralType,
                StartPosition = coinController.transform.position,
                IsComplete = false
            };
            
            // Configure spiral parameters based on type
            ConfigureSpiralParameters(spiral);
            
            _activeSpirals[coinId] = spiral;
            
            // Start the spiral animation
            StartCoroutine(ExecuteSpiralAnimation(spiral));
            
            return true;
        }

        /// <summary>
        /// Stop spiral animation for specific coin
        /// </summary>
        /// <param name="coinId">Coin ID to stop spiral for</param>
        public void StopSpiralAnimation(int coinId)
        {
            if (_activeSpirals.ContainsKey(coinId))
            {
                var spiral = _activeSpirals[coinId];
                spiral.IsComplete = true;
                _activeSpirals.Remove(coinId);
            }
        }

        /// <summary>
        /// Stop all active spiral animations
        /// </summary>
        public void StopAllSpiralAnimations()
        {
            foreach (var spiral in _activeSpirals.Values)
            {
                spiral.IsComplete = true;
            }
            
            _activeSpirals.Clear();
        }

        #endregion

        #region Spiral Configuration
        
        /// <summary>
        /// Configure spiral parameters based on spiral type
        /// </summary>
        /// <param name="spiral">Spiral animation to configure</param>
        private void ConfigureSpiralParameters(SpiralAnimation spiral)
        {
            float distance = Vector3.Distance(spiral.StartPosition, spiral.TargetPosition);
            
            switch (spiral.SpiralType)
            {
                case SpiralType.Helix:
                    spiral.Radius = baseRadius;
                    spiral.RotationSpeed = rotationSpeed;
                    spiral.VerticalAmplitude = verticalAmplitude;
                    spiral.RadiusCompression = radiusCompression;
                    break;
                    
                case SpiralType.Vortex:
                    spiral.Radius = baseRadius * 1.5f;
                    spiral.RotationSpeed = rotationSpeed * 1.5f;
                    spiral.VerticalAmplitude = verticalAmplitude * 0.5f;
                    spiral.RadiusCompression = radiusCompression * 1.2f;
                    break;
                    
                case SpiralType.DoubleHelix:
                    spiral.Radius = baseRadius * 0.8f;
                    spiral.RotationSpeed = rotationSpeed * 2f;
                    spiral.VerticalAmplitude = verticalAmplitude * 1.2f;
                    spiral.RadiusCompression = radiusCompression * 0.8f;
                    spiral.DoubleHelix = true;
                    break;
                    
                case SpiralType.Corkscrew:
                    spiral.Radius = baseRadius * 0.6f;
                    spiral.RotationSpeed = rotationSpeed * 0.7f;
                    spiral.VerticalAmplitude = verticalAmplitude * 2f;
                    spiral.RadiusCompression = radiusCompression * 0.5f;
                    break;
            }
            
            // Apply distance-based intensity scaling
            if (enableDistanceIntensity)
            {
                float intensity = CalculateDistanceIntensity(distance);
                spiral.Radius *= intensity;
                spiral.NoiseStrength *= intensity;
            }
        }

        /// <summary>
        /// Calculate intensity based on distance to target
        /// </summary>
        /// <param name="distance">Distance to target</param>
        /// <returns>Intensity multiplier (0-1)</returns>
        private float CalculateDistanceIntensity(float distance)
        {
            if (distance <= intensityStartDistance) return 1f;
            if (distance >= intensityEndDistance) return 0.1f;
            
            float normalizedDistance = (distance - intensityStartDistance) / 
                                     (intensityEndDistance - intensityStartDistance);
            return Mathf.Lerp(1f, 0.1f, normalizedDistance);
        }

        #endregion

        #region Spiral Animation Execution
        
        /// <summary>
        /// Execute spiral animation coroutine
        /// </summary>
        /// <param name="spiral">Spiral animation to execute</param>
        /// <returns>IEnumerator for coroutine</returns>
        private System.Collections.IEnumerator ExecuteSpiralAnimation(SpiralAnimation spiral)
        {
            float elapsedTime = 0f;
            float updateInterval = 1f / updateRate;
            float lastUpdateTime = 0f;
            
            Vector3[] pathPoints = CalculateSpiralPath(spiral);
            int currentPathIndex = 0;
            
            while (elapsedTime < spiral.Duration && !spiral.IsComplete)
            {
                float currentTime = Time.time;
                
                // Update at specified rate for performance
                if (currentTime - lastUpdateTime >= updateInterval)
                {
                    // Calculate position along spiral path
                    float pathProgress = elapsedTime / spiral.Duration;
                    int targetIndex = Mathf.FloorToInt(pathProgress * (pathPoints.Length - 1));
                    
                    if (targetIndex != currentPathIndex && targetIndex < pathPoints.Length)
                    {
                        currentPathIndex = targetIndex;
                        Vector3 targetPosition = pathPoints[currentPathIndex];
                        
                        // Apply noise for organic movement
                        targetPosition += CalculateNoiseOffset(spiral, elapsedTime);
                        
                        // Animate coin to new position
                        float moveDuration = updateInterval * 1.5f; // Slight overlap for smoothness
                        spiral.CoinController.AnimateToPosition(targetPosition, moveDuration);
                    }
                    
                    lastUpdateTime = currentTime;
                }
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Ensure coin reaches final target
            if (!spiral.IsComplete && spiral.CoinController != null)
            {
                spiral.CoinController.AnimateToPosition(spiral.TargetPosition, 0.2f);
            }
            
            // Clean up
            spiral.IsComplete = true;
            _activeSpirals.Remove(spiral.CoinId);
        }

        /// <summary>
        /// Calculate complete spiral path for animation
        /// </summary>
        /// <param name="spiral">Spiral animation configuration</param>
        /// <returns>Array of path points</returns>
        private Vector3[] CalculateSpiralPath(SpiralAnimation spiral)
        {
            int pointCount = Mathf.RoundToInt(spiral.Duration * updateRate);
            Vector3[] path = new Vector3[pointCount];
            
            Vector3 toTarget = spiral.TargetPosition - spiral.StartPosition;
            Vector3 direction = toTarget.normalized;
            float distance = toTarget.magnitude;
            
            // Create perpendicular vectors for spiral plane
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(direction, up).normalized;
            Vector3 forward = Vector3.Cross(right, direction).normalized;
            
            for (int i = 0; i < pointCount; i++)
            {
                float t = (float)i / (pointCount - 1);
                float time = t * spiral.Duration;
                
                // Calculate spiral parameters
                float currentRadius = spiral.Radius * Mathf.Pow(spiral.RadiusCompression, t * 10f);
                float angle = time * spiral.RotationSpeed + _timeOffset;
                
                // Base spiral position
                Vector3 spiralOffset = right * Mathf.Cos(angle) * currentRadius +
                                    forward * Mathf.Sin(angle) * currentRadius;
                
                // Add vertical component if enabled
                if (spiral.VerticalAmplitude > 0f)
                {
                    float verticalOffset = Mathf.Sin(angle * 2f) * spiral.VerticalAmplitude * (1f - t);
                    spiralOffset += up * verticalOffset;
                }
                
                // Double helix variant
                if (spiral.DoubleHelix)
                {
                    float doubleAngle = angle * 2f;
                    spiralOffset = right * (Mathf.Cos(angle) + Mathf.Cos(doubleAngle) * 0.5f) * currentRadius +
                                  forward * (Mathf.Sin(angle) + Mathf.Sin(doubleAngle) * 0.5f) * currentRadius;
                }
                
                // Interpolate toward target
                Vector3 basePosition = Vector3.Lerp(spiral.StartPosition, spiral.TargetPosition, t);
                path[i] = basePosition + spiralOffset;
            }
            
            return path;
        }

        /// <summary>
        /// Calculate Perlin noise offset for organic movement
        /// </summary>
        /// <param name="spiral">Spiral animation</param>
        /// <param name="time">Current animation time</param>
        /// <returns>Noise offset vector</returns>
        private Vector3 CalculateNoiseOffset(SpiralAnimation spiral, float time)
        {
            if (noiseStrength <= 0f) return Vector3.zero;
            
            float noiseX = Mathf.PerlinNoise(
                spiral.StartPosition.x * noiseScale + time * turbulenceFrequency,
                spiral.StartPosition.y * noiseScale + _timeOffset
            ) - 0.5f;
            
            float noiseY = Mathf.PerlinNoise(
                spiral.StartPosition.y * noiseScale + time * turbulenceFrequency,
                spiral.StartPosition.z * noiseScale + _timeOffset
            ) - 0.5f;
            
            float noiseZ = Mathf.PerlinNoise(
                spiral.StartPosition.z * noiseScale + time * turbulenceFrequency,
                spiral.StartPosition.x * noiseScale + _timeOffset
            ) - 0.5f;
            
            return new Vector3(noiseX, noiseY, noiseZ) * noiseStrength * spiral.NoiseStrength;
        }

        #endregion

        #region Utility Methods
        
        /// <summary>
        /// Get spiral animation for specific coin
        /// </summary>
        /// <param name="coinId">Coin ID</param>
        /// <returns>Spiral animation or null if not found</returns>
        public SpiralAnimation GetSpiralAnimation(int coinId)
        {
            return _activeSpirals.ContainsKey(coinId) ? _activeSpirals[coinId] : null;
        }

        /// <summary>
        /// Check if coin has active spiral animation
        /// </summary>
        /// <param name="coinId">Coin ID to check</param>
        /// <returns>True if coin has active spiral</returns>
        public bool HasActiveSpiral(int coinId)
        {
            return _activeSpirals.ContainsKey(coinId);
        }

        /// <summary>
        /// Get visual debug information for active spirals
        /// </summary>
        /// <returns>Debug information string</returns>
        public string GetDebugInfo()
        {
            return $"Active Spirals: {_activeSpirals.Count}/{maxConcurrentSpirals}, " +
                   $"Update Rate: {updateRate}Hz, " +
                   $"Base Radius: {baseRadius:F2}";
        }

        #endregion

        #region Cleanup
        
        private void OnDestroy()
        {
            StopAllSpiralAnimations();
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Spiral animation data structure
    /// </summary>
    public class SpiralAnimation
    {
        public int CoinId { get; set; }
        public CoinAnimationController CoinController { get; set; }
        public Vector3 TargetPosition { get; set; }
        public Vector3 StartPosition { get; set; }
        public float StartTime { get; set; }
        public float Duration { get; set; }
        public SpiralType SpiralType { get; set; }
        
        // Spiral parameters
        public float Radius { get; set; }
        public float RotationSpeed { get; set; }
        public float VerticalAmplitude { get; set; }
        public float RadiusCompression { get; set; }
        public float NoiseStrength { get; set; } = 1f;
        public bool DoubleHelix { get; set; } = false;
        
        public bool IsComplete { get; set; }
    }

    /// <summary>
    /// Types of spiral motion patterns
    /// </summary>
    public enum SpiralType
    {
        Helix,        // Standard helix spiral
        Vortex,       // Fast rotating vortex
        DoubleHelix,  // Double helix pattern
        Corkscrew     // Vertical corkscrew motion
    }

    #endregion
}