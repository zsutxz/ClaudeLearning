using System;
using UnityEngine;

namespace CoinAnimation.Physics
{
    /// <summary>
    /// Physics-based attraction calculations, spiral motion generation, magnetic field management
    /// Interface defined in Story Context XML for Epic 1 Story 2
    /// </summary>
    public interface IMagneticCollectionController
    {
        /// <summary>
        /// Initializes magnetic field with specified parameters
        /// </summary>
        /// <param name="centerPoint">Center point of magnetic field</param>
        /// <param name="fieldStrength">Strength of magnetic attraction</param>
        /// <param name="fieldRadius">Effective radius of magnetic field</param>
        void InitializeMagneticField(Vector3 centerPoint, float fieldStrength, float fieldRadius);
        
        /// <summary>
        /// Applies magnetic force to a coin object
        /// </summary>
        /// <param name="coin">Coin object to apply force to</param>
        /// <param name="targetPosition">Target position for collection</param>
        void ApplyMagneticForce(GameObject coin, Vector3 targetPosition);
        
        /// <summary>
        /// Generates spiral motion path for coin collection
        /// </summary>
        /// <param name="startPosition">Starting position of coin</param>
        /// <param name="targetPosition">Target collection position</param>
        /// <param name="duration">Duration of spiral animation</param>
        /// <returns>Spiral path points</returns>
        Vector3[] GenerateSpiralPath(Vector3 startPosition, Vector3 targetPosition, float duration);
        
        /// <summary>
        /// Updates magnetic field parameters in real-time
        /// </summary>
        /// <param name="fieldStrength">New field strength</param>
        /// <param name="fieldRadius">New field radius</param>
        void UpdateFieldParameters(float fieldStrength, float fieldRadius);
        
        /// <summary>
        /// Enables or disables magnetic collection for coins
        /// </summary>
        /// <param name="enabled">Whether magnetic collection is enabled</param>
        void SetMagneticCollectionEnabled(bool enabled);
        
        /// <summary>
        /// Gets current magnetic field configuration
        /// </summary>
        /// <returns>Field configuration</returns>
        MagneticFieldConfiguration GetFieldConfiguration();
        
        /// <summary>
        /// Event triggered when coin enters magnetic field
        /// </summary>
        event Action<GameObject> OnCoinEnteredField;
        
        /// <summary>
        /// Event triggered when coin is collected via magnetic force
        /// </summary>
        event Action<GameObject> OnCoinCollected;
    }
    
    /// <summary>
    /// Configuration for magnetic field behavior
    /// </summary>
    [Serializable]
    public class MagneticFieldConfiguration
    {
        [Header("Field Properties")]
        public Vector3 centerPoint;
        public float fieldStrength = 10f;
        public float fieldRadius = 5f;
        public bool isEnabled = true;
        
        [Header("Spiral Motion")]
        public float spiralFrequency = 2f;
        public float spiralAmplitude = 1f;
        public bool enableSpiralMotion = true;
        
        [Header("Physics Settings")]
        public float dragCoefficient = 0.1f;
        public float collectionThreshold = 0.1f;
        public LayerMask coinLayerMask = -1;
        
        [Header("Performance")]
        public int maxConcurrentCoins = 50;
        public float physicsUpdateRate = 60f;
        public bool enableOptimizedPhysics = true;
    }
}