using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using CoinAnimation.Core;

namespace CoinAnimation.Animation
{
    /// <summary>
    /// Individual coin animation controller
    /// Handles per-coin animation state and DOTween sequences
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class CoinAnimationController : MonoBehaviour
    {
        #region Configuration
        
        [Header("Animation Settings")]
        [SerializeField, Tooltip("Base animation speed multiplier")]
        private float animationSpeed = 1f;
        
        [SerializeField, Tooltip("Rotation animation speed")]
        private float rotationSpeed = 360f;
        
        [SerializeField, Tooltip("Scale animation amount on collection")]
        private Vector3 collectionScale = new Vector3(1.5f, 1.5f, 1.5f);
        
        [SerializeField, Tooltip("Enable physics-based movement")]
        private bool enablePhysics = true;

        [Header("Visual Effects")]
        [SerializeField, Tooltip("Particle system for collection effect")]
        private ParticleSystem collectionEffect;
        
        [SerializeField, Tooltip("Audio source for collection sound")]
        private AudioSource collectionAudio;

        #endregion

        #region Private Fields
        
        private Rigidbody _rigidbody;
        private Collider _collider;
        private List<Tween> _activeTweens = new List<Tween>();
        
        private CoinAnimationState _currentState = CoinAnimationState.Idle;
        private int _coinId = -1;
        private bool _isInitialized = false;

        #endregion

        #region Properties
        
        /// <summary>
        /// Current animation state
        /// </summary>
        public CoinAnimationState CurrentState => _currentState;
        
        /// <summary>
        /// Unique identifier for this coin
        /// </summary>
        public int CoinId => _coinId;
        
        /// <summary>
        /// Is the coin currently being animated
        /// </summary>
        public bool IsAnimating => _activeTweens.Count > 0;

        #endregion

        #region Events
        
        /// <summary>
        /// Fired when this coin's animation state changes
        /// </summary>
        public event EventHandler<CoinAnimationEventArgs> OnStateChanged;

        #endregion

        #region Initialization
        
        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            RegisterWithManager();
        }

        /// <summary>
        /// Initialize required components
        /// </summary>
        private void InitializeComponents()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            
            // Configure physics
            if (_rigidbody != null)
            {
                _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                _rigidbody.useGravity = enablePhysics;
            }
            
            _isInitialized = true;
        }

        /// <summary>
        /// Register with the global animation manager
        /// </summary>
        private void RegisterWithManager()
        {
            if (CoinAnimationManager.Instance != null)
            {
                _coinId = CoinAnimationManager.Instance.RegisterCoin(this);
            }
            else
            {
                Debug.LogError("[CoinAnimationController] CoinAnimationManager not found!");
            }
        }

        #endregion

        #region State Management
        
        /// <summary>
        /// Set the animation state (internal use by manager)
        /// </summary>
        /// <param name="newState">New state to set</param>
        internal void SetState(CoinAnimationState newState)
        {
            if (_currentState == newState) return;
            
            var previousState = _currentState;
            _currentState = newState;
            
            OnStateChanged?.Invoke(this, new CoinAnimationEventArgs(previousState, newState, 0f, true));
            
            // Handle state-specific logic
            switch (newState)
            {
                case CoinAnimationState.Idle:
                    HandleIdleState();
                    break;
                    
                case CoinAnimationState.Moving:
                    HandleMovingState();
                    break;
                    
                case CoinAnimationState.Collecting:
                    HandleCollectingState();
                    break;
                    
                case CoinAnimationState.Paused:
                    HandlePausedState();
                    break;
                    
                case CoinAnimationState.Pooled:
                    HandlePooledState();
                    break;
            }
        }

        #endregion

        #region Animation Methods
        
        /// <summary>
        /// Animate coin to target position with specified easing
        /// </summary>
        /// <param name="targetPosition">Target position</param>
        /// <param name="duration">Animation duration</param>
        /// <param name="ease">DOTween easing function</param>
        public void AnimateToPosition(Vector3 targetPosition, float duration, Ease ease = Ease.OutQuad)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[CoinAnimationController] Not initialized!");
                return;
            }
            
            KillAllTweens();
            
            // Create main movement animation
            Tween moveTween = transform.DOMove(targetPosition, duration / animationSpeed)
                .SetEase(ease)
                .SetUpdate(UpdateType.Fixed)
                .OnStart(() => {
                    SetState(CoinAnimationState.Moving);
                })
                .OnComplete(() => {
                    OnMovementComplete();
                });
            
            _activeTweens.Add(moveTween);
            
            // Add rotation animation for visual appeal
            Tween rotationTween = transform.DORotate(Vector3.up * rotationSpeed * duration, duration / animationSpeed, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetUpdate(UpdateType.Fixed);
            
            _activeTweens.Add(rotationTween);
        }

        /// <summary>
        /// Animate coin collection with spiral effect
        /// </summary>
        /// <param name="collectionPoint">Collection point position</param>
        /// <param name="duration">Collection animation duration</param>
        public void CollectCoin(Vector3 collectionPoint, float duration = 1f)
        {
            if (!_isInitialized) return;
            
            KillAllTweens();
            SetState(CoinAnimationState.Collecting);
            
            // Create spiral collection animation
            Sequence collectionSequence = DOTween.Sequence();
            
            // Scale up effect
            Tween scaleUp = transform.DOScale(collectionScale, duration * 0.3f)
                .SetEase(CoinAnimationEasing.CoinBurst);
            
            // Spiral movement to collection point
            Vector3[] path = CalculateSpiralPath(transform.position, collectionPoint, duration);
            Tween spiralMove = transform.DOPath(path, duration * 0.7f, PathType.CatmullRom)
                .SetEase(CoinAnimationEasing.SpiralEntry)
                .SetLookAt(0.01f);
            
            // Scale down at the end
            Tween scaleDown = transform.DOScale(Vector3.zero, duration * 0.2f)
                .SetEase(CoinAnimationEasing.QuickSnap);
            
            // Build sequence
            collectionSequence.Append(scaleUp);
            collectionSequence.Join(spiralMove);
            collectionSequence.Append(scaleDown);
            
            collectionSequence.SetUpdate(UpdateType.Fixed)
                .OnComplete(() => {
                    OnCollectionComplete(collectionPoint);
                });
            
            _activeTweens.Add(collectionSequence);
            
            // Play collection effects
            PlayCollectionEffects();
        }

        /// <summary>
        /// Calculate spiral path for collection animation
        /// </summary>
        /// <param name="startPos">Starting position</param>
        /// <param name="endPos">Ending position</param>
        /// <param name="duration">Animation duration</param>
        /// <returns>Array of path points</returns>
        private Vector3[] CalculateSpiralPath(Vector3 startPos, Vector3 endPos, float duration)
        {
            int pointCount = Mathf.RoundToInt(duration * 30); // 30 points per second
            Vector3[] path = new Vector3[pointCount];
            
            float distance = Vector3.Distance(startPos, endPos);
            Vector3 direction = (endPos - startPos).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
            
            for (int i = 0; i < pointCount; i++)
            {
                float t = (float)i / (pointCount - 1);
                float spiralRadius = Mathf.Lerp(distance * 0.5f, 0.1f, t);
                float spiralAngle = t * Mathf.PI * 4; // 2 full rotations
                
                Vector3 spiralOffset = perpendicular * Mathf.Cos(spiralAngle) * spiralRadius + 
                                     Vector3.up * Mathf.Sin(spiralAngle) * spiralRadius * 0.3f;
                
                path[i] = Vector3.Lerp(startPos, endPos, t) + spiralOffset;
            }
            
            return path;
        }

        #endregion

        #region State Handlers
        
        private void HandleIdleState()
        {
            // Enable physics when idle
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
            }
        }

        private void HandleMovingState()
        {
            // Disable physics during animated movement
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
            }
        }

        private void HandleCollectingState()
        {
            // Disable physics and collider during collection
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
            }
            if (_collider != null)
            {
                _collider.enabled = false;
            }
        }

        private void HandlePausedState()
        {
            // Pause all active tweens
            foreach (var tween in _activeTweens)
            {
                tween.Pause();
            }
        }

        private void HandlePooledState()
        {
            // Reset coin state for pooling
            KillAllTweens();
            transform.localScale = Vector3.one;
            gameObject.SetActive(false);
            
            if (_collider != null)
            {
                _collider.enabled = true;
            }
        }

        #endregion

        #region Effects
        
        /// <summary>
        /// Play collection visual and audio effects
        /// </summary>
        private void PlayCollectionEffects()
        {
            // Play particle effect
            if (collectionEffect != null)
            {
                collectionEffect.Play();
            }
            
            // Play collection sound
            if (collectionAudio != null && collectionAudio.clip != null)
            {
                collectionAudio.Play();
            }
        }

        #endregion

        #region Event Handlers
        
        private void OnMovementComplete()
        {
            SetState(CoinAnimationState.Idle);
        }

        private void OnCollectionComplete(Vector3 collectionPoint)
        {
            // Notify manager of collection completion
            if (CoinAnimationManager.Instance != null)
            {
                CoinAnimationManager.Instance.TriggerCollectionComplete(_coinId, collectionPoint);
            }
            
            // Pool this coin
            SetState(CoinAnimationState.Pooled);
        }

        #endregion

        #region Utility Methods
        
        /// <pause>
        /// Kill all active tweens for this coin
        /// </summary>
        public void KillAllTweens()
        {
            foreach (var tween in _activeTweens)
            {
                if (tween != null && tween.active)
                {
                    tween.Kill(false);
                }
            }
            _activeTweens.Clear();
        }

        /// <summary>
        /// Pause all animations
        /// </summary>
        public void PauseAnimations()
        {
            SetState(CoinAnimationState.Paused);
        }

        /// <summary>
        /// Resume all animations
        /// </summary>
        public void ResumeAnimations()
        {
            SetState(CoinAnimationState.Moving);
            foreach (var tween in _activeTweens)
            {
                if (tween != null)
                {
                    tween.Play();
                }
            }
        }

        #endregion

        #region Cleanup
        
        private void OnDestroy()
        {
            KillAllTweens();
            
            // Unregister from manager
            if (_coinId != -1 && CoinAnimationManager.Instance != null)
            {
                CoinAnimationManager.Instance.UnregisterCoin(_coinId);
            }
        }

        #endregion
    }
}