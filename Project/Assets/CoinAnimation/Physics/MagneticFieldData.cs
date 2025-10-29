using UnityEngine;

namespace CoinAnimation.Physics
{
    /// <summary>
    /// Data structure for magnetic field configuration
    /// Optimized for performance with configurable parameters
    /// </summary>
    [CreateAssetMenu(fileName = "MagneticFieldData", menuName = "Coin Animation/Magnetic Field Data")]
    [System.Serializable]
    public class MagneticFieldData : ScriptableObject
    {
        [Header("Field Configuration")]
        [SerializeField, Tooltip("Center point of magnetic field")]
        private Vector3 fieldCenter = Vector3.zero;
        
        [SerializeField, Tooltip("Maximum effective range of magnetic field")]
        private float fieldRadius = 5f;
        
        [SerializeField, Tooltip("Strength of magnetic attraction at center")]
        private float maxMagneticStrength = 10f;
        
        [SerializeField, Tooltip("Falloff curve for field strength over distance")]
        private AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        [Header("Advanced Settings")]
        [SerializeField, Tooltip("Enable spiral motion within field")]
        private bool enableSpiralMotion = true;
        
        [SerializeField, Tooltip("Spiral intensity multiplier")]
        private float spiralIntensity = 1f;
        
        [SerializeField, Tooltip("Turbulence factor for realistic movement")]
        private float turbulence = 0.1f;
        
        [SerializeField, Tooltip("Angular velocity for spiral motion")]
        private float angularVelocity = 2f;

        [Header("Performance")]
        [SerializeField, Tooltip("Update rate for field calculations (Hz)")]
        private int updateRate = 60;
        
        [SerializeField, Tooltip("Maximum affected coins per frame")]
        private int maxAffectedCoins = 50;

        #region Properties
        
        /// <summary>
        /// Center position of the magnetic field
        /// </summary>
        public Vector3 FieldCenter 
        { 
            get => fieldCenter; 
            set => fieldCenter = value; 
        }
        
        /// <summary>
        /// Maximum effective range of the magnetic field
        /// </summary>
        public float FieldRadius 
        { 
            get => fieldRadius; 
            set => fieldRadius = Mathf.Max(0.1f, value); 
        }
        
        /// <summary>
        /// Maximum magnetic strength at field center
        /// </summary>
        public float MaxMagneticStrength 
        { 
            get => maxMagneticStrength; 
            set => maxMagneticStrength = Mathf.Max(0f, value); 
        }
        
        /// <summary>
        /// Whether spiral motion is enabled
        /// </summary>
        public bool EnableSpiralMotion 
        { 
            get => enableSpiralMotion; 
            set => enableSpiralMotion = value; 
        }
        
        /// <summary>
        /// Spiral motion intensity multiplier
        /// </summary>
        public float SpiralIntensity 
        { 
            get => spiralIntensity; 
            set => spiralIntensity = Mathf.Max(0f, value); 
        }
        
        /// <summary>
        /// Movement turbulence factor
        /// </summary>
        public float Turbulence 
        { 
            get => turbulence; 
            set => turbulence = Mathf.Clamp01(value); 
        }
        
        /// <summary>
        /// Angular velocity for spiral motion
        /// </summary>
        public float AngularVelocity 
        { 
            get => angularVelocity; 
            set => angularVelocity = Mathf.Max(0f, value); 
        }
        
        /// <summary>
        /// Field update rate in Hz
        /// </summary>
        public int UpdateRate 
        { 
            get => updateRate; 
            set => updateRate = Mathf.Clamp(1, 120, value); 
        }
        
        /// <summary>
        /// Maximum coins that can be affected per frame
        /// </summary>
        public int MaxAffectedCoins 
        { 
            get => maxAffectedCoins; 
            set => maxAffectedCoins = Mathf.Max(1, value); 
        }

        #endregion

        #region Physics Calculations
        
        /// <summary>
        /// Calculate magnetic force at given position
        /// </summary>
        /// <param name="position">Position to calculate force at</param>
        /// <returns>Magnetic force vector</returns>
        public Vector3 CalculateMagneticForce(Vector3 position)
        {
            Vector3 toCenter = fieldCenter - position;
            float distance = toCenter.magnitude;
            
            // Outside field range
            if (distance > fieldRadius || distance < 0.01f)
            {
                return Vector3.zero;
            }
            
            // Calculate base force strength using falloff curve
            float normalizedDistance = distance / fieldRadius;
            float forceStrength = falloffCurve.Evaluate(normalizedDistance) * maxMagneticStrength;
            
            // Apply force direction toward center
            Vector3 forceDirection = toCenter.normalized;
            Vector3 magneticForce = forceDirection * forceStrength;
            
            // Add turbulence for realistic movement
            if (turbulence > 0f)
            {
                Vector3 randomOffset = new Vector3(
                    Random.Range(-turbulence, turbulence),
                    Random.Range(-turbulence, turbulence),
                    Random.Range(-turbulence, turbulence)
                );
                magneticForce += randomOffset * forceStrength;
            }
            
            return magneticForce;
        }

        /// <summary>
        /// Calculate spiral offset for position within field
        /// </summary>
        /// <param name="position">Position to calculate spiral for</param>
        /// <param name="time">Current time for animation</param>
        /// <returns>Spiral offset vector</returns>
        public Vector3 CalculateSpiralOffset(Vector3 position, float time)
        {
            if (!enableSpiralMotion)
            {
                return Vector3.zero;
            }
            
            Vector3 toCenter = fieldCenter - position;
            float distance = toCenter.magnitude;
            
            if (distance > fieldRadius || distance < 0.01f)
            {
                return Vector3.zero;
            }
            
            // Calculate spiral parameters
            float normalizedDistance = distance / fieldRadius;
            float spiralRadius = (1f - normalizedDistance) * spiralIntensity;
            float spiralAngle = time * angularVelocity + distance * 0.5f;
            
            // Create perpendicular spiral vectors
            Vector3 up = Vector3.up;
            Vector3 perpendicular1 = Vector3.Cross(toCenter.normalized, up).normalized;
            Vector3 perpendicular2 = Vector3.Cross(perpendicular1, toCenter.normalized).normalized;
            
            // Calculate spiral offset
            Vector3 spiralOffset = perpendicular1 * Mathf.Cos(spiralAngle) * spiralRadius +
                                 perpendicular2 * Mathf.Sin(spiralAngle) * spiralRadius;
            
            return spiralOffset;
        }

        /// <summary>
        /// Check if position is within magnetic field range
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>True if position is within field</returns>
        public bool IsWithinField(Vector3 position)
        {
            float distance = Vector3.Distance(position, fieldCenter);
            return distance <= fieldRadius;
        }

        /// <summary>
        /// Get normalized field strength at position (0-1)
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>Normalized field strength</returns>
        public float GetFieldStrength(Vector3 position)
        {
            Vector3 toCenter = fieldCenter - position;
            float distance = toCenter.magnitude;
            
            if (distance > fieldRadius || distance < 0.01f)
            {
                return 0f;
            }
            
            float normalizedDistance = distance / fieldRadius;
            return falloffCurve.Evaluate(normalizedDistance);
        }

        #endregion

        #region Utility Methods
        
        /// <summary>
        /// Reset field to default values
        /// </summary>
        public void ResetToDefaults()
        {
            fieldCenter = Vector3.zero;
            fieldRadius = 5f;
            maxMagneticStrength = 10f;
            falloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            enableSpiralMotion = true;
            spiralIntensity = 1f;
            turbulence = 0.1f;
            angularVelocity = 2f;
            updateRate = 60;
            maxAffectedCoins = 50;
        }

        /// <summary>
        /// Create falloff curve based on field type
        /// </summary>
        /// <param name="fieldType">Type of magnetic field</param>
        public void SetFalloffCurve(MagneticFieldType fieldType)
        {
            falloffCurve = new AnimationCurve();
            
            switch (fieldType)
            {
                case MagneticFieldType.Linear:
                    falloffCurve.AddKey(0f, 1f);
                    falloffCurve.AddKey(1f, 0f);
                    break;
                    
                case MagneticFieldType.Exponential:
                    falloffCurve.AddKey(0f, 1f);
                    falloffCurve.AddKey(0.3f, 0.8f);
                    falloffCurve.AddKey(0.7f, 0.3f);
                    falloffCurve.AddKey(1f, 0f);
                    break;
                    
                case MagneticFieldType.Gaussian:
                    falloffCurve.AddKey(0f, 1f);
                    falloffCurve.AddKey(0.2f, 0.95f);
                    falloffCurve.AddKey(0.5f, 0.6f);
                    falloffCurve.AddKey(0.8f, 0.2f);
                    falloffCurve.AddKey(1f, 0f);
                    break;
                    
                case MagneticFieldType.Plateau:
                    falloffCurve.AddKey(0f, 1f);
                    falloffCurve.AddKey(0.4f, 1f);
                    falloffCurve.AddKey(0.7f, 0.5f);
                    falloffCurve.AddKey(1f, 0f);
                    break;
            }
        }

        #endregion

        #region Validation
        
        /// <summary>
        /// Validate field parameters and auto-correct invalid values
        /// </summary>
        public void ValidateParameters()
        {
            fieldRadius = Mathf.Max(0.1f, fieldRadius);
            maxMagneticStrength = Mathf.Max(0f, maxMagneticStrength);
            spiralIntensity = Mathf.Max(0f, spiralIntensity);
            turbulence = Mathf.Clamp01(turbulence);
            angularVelocity = Mathf.Max(0f, angularVelocity);
            updateRate = Mathf.Clamp(1, 120, updateRate);
            maxAffectedCoins = Mathf.Max(1, maxAffectedCoins);
        }

        #endregion
    }

    /// <summary>
    /// Types of magnetic field falloff patterns
    /// </summary>
    public enum MagneticFieldType
    {
        Linear,
        Exponential,
        Gaussian,
        Plateau
    }
}