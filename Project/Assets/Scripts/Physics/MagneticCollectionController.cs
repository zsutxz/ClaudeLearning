using System.Collections.Generic;
using UnityEngine;
using CoinAnimation.Core;
using CoinAnimation.Animation;

namespace CoinAnimation.Physics
{
    /// <summary>
    /// Controller for magnetic coin collection system
    /// Manages multiple magnetic fields and coin interactions
    /// </summary>
    public class MagneticCollectionController : MonoBehaviour
    {
        #region Configuration
        
        [Header("Magnetic Field Settings")]
        [SerializeField, Tooltip("Default magnetic field configuration")]
        private MagneticFieldData defaultFieldData;
        
        [SerializeField, Tooltip("Maximum number of active magnetic fields")]
        private int maxMagneticFields = 10;
        
        [SerializeField, Tooltip("Enable real-time physics calculations")]
        private bool enableRealtimePhysics = true;

        [Header("Performance Settings")]
        [SerializeField, Tooltip("Physics update rate (Hz)")]
        private int physicsUpdateRate = 60;
        
        [SerializeField, Tooltip("Maximum coins processed per frame")]
        private int maxCoinsPerFrame = 100;
        
        [SerializeField, Tooltip("Enable spatial optimization")]
        private bool enableSpatialOptimization = true;

        #endregion

        #region Private Fields
        
        private readonly List<MagneticField> _activeFields = new List<MagneticField>();
        private readonly Dictionary<int, CoinMagneticState> _coinStates = new Dictionary<int, CoinMagneticState>();
        private readonly List<CoinAnimationController> _nearbyCoins = new List<CoinAnimationController>();
        
        private float _lastUpdateTime = 0f;
        private float _physicsUpdateInterval = 0.0167f; // ~60 FPS
        private bool _isInitialized = false;

        #endregion

        #region Properties
        
        /// <summary>
        /// Number of currently active magnetic fields
        /// </summary>
        public int ActiveFieldCount => _activeFields.Count;
        
        /// <summary>
        /// Number of coins currently affected by magnetic fields
        /// </summary>
        public int AffectedCoinCount => _coinStates.Count;

        #endregion

        #region Initialization
        
        private void Awake()
        {
            InitializeController();
        }

        /// <summary>
        /// Initialize the magnetic collection system
        /// </summary>
        private void InitializeController()
        {
            if (_isInitialized) return;
            
            // Calculate physics update interval
            _physicsUpdateInterval = 1f / physicsUpdateRate;
            
            // Validate configuration
            if (defaultFieldData == null)
            {
                CreateDefaultFieldData();
            }
            
            _isInitialized = true;
            
            if (CoinAnimationManager.Instance != null)
            {
                CoinAnimationManager.Instance.OnCoinStateChanged += OnCoinStateChanged;
                CoinAnimationManager.Instance.OnCoinCollectionComplete += OnCoinCollectionComplete;
            }
        }

        /// <summary>
        /// Create default magnetic field data if none assigned
        /// </summary>
        private void CreateDefaultFieldData()
        {
            defaultFieldData = ScriptableObject.CreateInstance<MagneticFieldData>();
            defaultFieldData.ResetToDefaults();
            defaultFieldData.SetFalloffCurve(MagneticFieldType.Gaussian);
        }

        #endregion

        #region Magnetic Field Management
        
        /// <summary>
        /// Add a new magnetic field at specified position
        /// </summary>
        /// <param name="position">Field center position</param>
        /// <param name="fieldData">Field configuration (uses default if null)</param>
        /// <returns>ID of created field</returns>
        public int AddMagneticField(Vector3 position, MagneticFieldData fieldData = null)
        {
            if (_activeFields.Count >= maxMagneticFields)
            {
                Debug.LogWarning("[MagneticCollectionController] Maximum magnetic fields reached!");
                return -1;
            }
            
            fieldData = fieldData ?? defaultFieldData;
            if (fieldData == null)
            {
                Debug.LogError("[MagneticCollectionController] No valid field data available!");
                return -1;
            }
            
            // Create field instance
            var field = new MagneticField
            {
                Id = _activeFields.Count,
                Position = position,
                Data = Instantiate(fieldData),
                IsActive = true
            };
            
            // Update field data center position
            field.Data.FieldCenter = position;
            field.Data.ValidateParameters();
            
            _activeFields.Add(field);
            
            return field.Id;
        }

        /// <summary>
        /// Remove a magnetic field by ID
        /// </summary>
        /// <param name="fieldId">ID of field to remove</param>
        public void RemoveMagneticField(int fieldId)
        {
            for (int i = _activeFields.Count - 1; i >= 0; i--)
            {
                if (_activeFields[i].Id == fieldId)
                {
                    var field = _activeFields[i];
                    _activeFields.RemoveAt(i);
                    
                    // Clear affected coins
                    ClearCoinsFromField(field);
                    break;
                }
            }
        }

        /// <summary>
        /// Update magnetic field position
        /// </summary>
        /// <param name="fieldId">Field ID to update</param>
        /// <param name="newPosition">New field position</param>
        public void UpdateFieldPosition(int fieldId, Vector3 newPosition)
        {
            foreach (var field in _activeFields)
            {
                if (field.Id == fieldId)
                {
                    field.Position = newPosition;
                    field.Data.FieldCenter = newPosition;
                    break;
                }
            }
        }

        /// <summary>
        /// Clear all magnetic fields
        /// </summary>
        public void ClearAllFields()
        {
            foreach (var field in _activeFields)
            {
                ClearCoinsFromField(field);
            }
            
            _activeFields.Clear();
        }

        #endregion

        #region Physics Update
        
        private void Update()
        {
            if (!enableRealtimePhysics || !Application.isPlaying) return;
            
            float currentTime = Time.time;
            if (currentTime - _lastUpdateTime < _physicsUpdateInterval) return;
            
            _lastUpdateTime = currentTime;
            UpdateMagneticPhysics();
        }

        /// <summary>
        /// Main physics update loop
        /// </summary>
        private void UpdateMagneticPhysics()
        {
            if (_activeFields.Count == 0) return;
            
            // Get nearby coins using spatial optimization
            FindNearbyCoins();
            
            // Process coins in batches for performance
            int processedCount = 0;
            foreach (var coin in _nearbyCoins)
            {
                if (processedCount >= maxCoinsPerFrame) break;
                
                ProcessCoinMagneticInteraction(coin);
                processedCount++;
            }
            
            // Update coin states
            UpdateCoinStates();
        }

        /// <summary>
        /// Find coins within range of active magnetic fields
        /// </summary>
        private void FindNearbyCoins()
        {
            _nearbyCoins.Clear();
            
            if (CoinAnimationManager.Instance == null) return;
            
            // Simple spatial optimization - check field boundaries first
            foreach (var field in _activeFields)
            {
                if (!field.IsActive) continue;
                
                // Find coins within field radius
                Collider[] hitColliders = Physics.OverlapSphere(field.Position, field.Data.FieldRadius);
                foreach (var collider in hitColliders)
                {
                    var coinController = collider.GetComponent<CoinAnimationController>();
                    if (coinController != null && coinController.CurrentState != CoinAnimationState.Pooled)
                    {
                        if (!_nearbyCoins.Contains(coinController))
                        {
                            _nearbyCoins.Add(coinController);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Process magnetic interaction for a single coin
        /// </summary>
        /// <param name="coin">Coin to process</param>
        private void ProcessCoinMagneticInteraction(CoinAnimationController coin)
        {
            Vector3 totalForce = Vector3.zero;
            Vector3 totalSpiralOffset = Vector3.zero;
            bool isInAnyField = false;
            
            // Calculate combined forces from all fields
            foreach (var field in _activeFields)
            {
                if (!field.IsActive) continue;
                
                if (field.Data.IsWithinField(coin.transform.position))
                {
                    Vector3 fieldForce = field.Data.CalculateMagneticForce(coin.transform.position);
                    Vector3 spiralOffset = field.Data.CalculateSpiralOffset(coin.transform.position, Time.time);
                    
                    totalForce += fieldForce;
                    totalSpiralOffset += spiralOffset;
                    isInAnyField = true;
                }
            }
            
            // Update coin state
            if (isInAnyField)
            {
                if (!_coinStates.ContainsKey(coin.CoinId))
                {
                    _coinStates[coin.CoinId] = new CoinMagneticState
                    {
                        CoinController = coin,
                        EnterTime = Time.time,
                        LastFieldStrength = 0f
                    };
                }
                
                // Apply magnetic forces
                ApplyMagneticForces(coin, totalForce, totalSpiralOffset);
                
                // Update state information
                var state = _coinStates[coin.CoinId];
                state.LastForce = totalForce;
                state.LastSpiralOffset = totalSpiralOffset;
                state.LastFieldStrength = GetMaximumFieldStrength(coin.transform.position);
            }
            else
            {
                // Remove from affected coins if outside all fields
                if (_coinStates.ContainsKey(coin.CoinId))
                {
                    _coinStates.Remove(coin.CoinId);
                }
            }
        }

        /// <summary>
        /// Apply calculated magnetic forces to coin
        /// </summary>
        /// <param name="coin">Target coin</param>
        /// <param name="force">Magnetic force to apply</param>
        /// <param name="spiralOffset">Spiral motion offset</param>
        private void ApplyMagneticForces(CoinAnimationController coin, Vector3 force, Vector3 spiralOffset)
        {
            // Check if coin should be collected
            float fieldStrength = GetMaximumFieldStrength(coin.transform.position);
            if (fieldStrength > 0.8f) // High field strength triggers collection
            {
                Vector3 nearestFieldCenter = GetNearestFieldCenter(coin.transform.position);
                coin.CollectCoin(nearestFieldCenter, 0.5f);
                return;
            }
            
            // Apply continuous magnetic animation using DOTween
            if (coin.CurrentState != CoinAnimationState.Collecting)
            {
                Vector3 targetPosition = coin.transform.position + force * Time.deltaTime + spiralOffset;
                float duration = 0.016f; // ~60 FPS update rate
                
                coin.AnimateToPosition(targetPosition, duration, CoinAnimationEasing.MagneticAttraction);
            }
        }

        #endregion

        #region Utility Methods
        
        /// <summary>
        /// Get maximum field strength at position from all active fields
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>Maximum field strength (0-1)</returns>
        private float GetMaximumFieldStrength(Vector3 position)
        {
            float maxStrength = 0f;
            
            foreach (var field in _activeFields)
            {
                if (!field.IsActive) continue;
                
                float strength = field.Data.GetFieldStrength(position);
                maxStrength = Mathf.Max(maxStrength, strength);
            }
            
            return maxStrength;
        }

        /// <summary>
        /// Get center of nearest active magnetic field
        /// </summary>
        /// <param name="position">Reference position</param>
        /// <returns>Nearest field center position</returns>
        private Vector3 GetNearestFieldCenter(Vector3 position)
        {
            Vector3 nearestCenter = Vector3.zero;
            float nearestDistance = float.MaxValue;
            
            foreach (var field in _activeFields)
            {
                if (!field.IsActive) continue;
                
                float distance = Vector3.Distance(position, field.Position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCenter = field.Position;
                }
            }
            
            return nearestCenter;
        }

        /// <summary>
        /// Clear coins from specific magnetic field
        /// </summary>
        /// <param name="field">Field to clear coins from</param>
        private void ClearCoinsFromField(MagneticField field)
        {
            var coinsToRemove = new List<int>();
            
            foreach (var kvp in _coinStates)
            {
                if (field.Data.IsWithinField(kvp.Value.CoinController.transform.position))
                {
                    coinsToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var coinId in coinsToRemove)
            {
                _coinStates.Remove(coinId);
            }
        }

        /// <summary>
        /// Update coin states and remove stale entries
        /// </summary>
        private void UpdateCoinStates()
        {
            var coinsToRemove = new List<int>();
            
            foreach (var kvp in _coinStates)
            {
                var coin = kvp.Value.CoinController;
                
                // Remove coins that are collected or destroyed
                if (coin == null || coin.CurrentState == CoinAnimationState.Pooled)
                {
                    coinsToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var coinId in coinsToRemove)
            {
                _coinStates.Remove(coinId);
            }
        }

        #endregion

        #region Event Handlers
        
        private void OnCoinStateChanged(object sender, CoinAnimationEventArgs e)
        {
            // Handle coin state changes if needed
            if (e.CurrentState == CoinAnimationState.Pooled && _coinStates.ContainsKey(e.NewState.GetHashCode()))
            {
                _coinStates.Remove(e.NewState.GetHashCode());
            }
        }

        private void OnCoinCollectionComplete(object sender, CoinCollectionEventArgs e)
        {
            // Remove collected coin from magnetic processing
            if (_coinStates.ContainsKey(e.CoinId))
            {
                _coinStates.Remove(e.CoinId);
            }
        }

        #endregion

        #region Cleanup
        
        private void OnDestroy()
        {
            if (CoinAnimationManager.Instance != null)
            {
                CoinAnimationManager.Instance.OnCoinStateChanged -= OnCoinStateChanged;
                CoinAnimationManager.Instance.OnCoinCollectionComplete -= OnCoinCollectionComplete;
            }
            
            ClearAllFields();
        }

        #endregion
    }

    #region Supporting Classes
    
    /// <summary>
    /// Represents an active magnetic field
    /// </summary>
    internal class MagneticField
    {
        public int Id { get; set; }
        public Vector3 Position { get; set; }
        public MagneticFieldData Data { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Tracks magnetic state for affected coins
    /// </summary>
    internal class CoinMagneticState
    {
        public CoinAnimationController CoinController { get; set; }
        public float EnterTime { get; set; }
        public Vector3 LastForce { get; set; }
        public Vector3 LastSpiralOffset { get; set; }
        public float LastFieldStrength { get; set; }
    }

    #endregion
}