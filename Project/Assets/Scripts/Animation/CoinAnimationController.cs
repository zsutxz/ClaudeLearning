using System;
using System.Collections;
using UnityEngine;
using CoinAnimation.Core;

namespace CoinAnimation.Animation
{
    /// <summary>
    /// Enhanced coin animation controller with memory management integration
    /// Story 1.3 Task 3 - Performance Integration
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class CoinAnimationController : MonoBehaviour
    {
        #region Configuration

        [Header("Animation Settings")]
        [SerializeField] private float animationSpeed = 1f;
        [SerializeField] private float rotationSpeed = 360f;

        [Header("Effects")]
        [SerializeField] private ParticleSystem collectionEffect;
        [SerializeField] private AudioSource collectionAudio;

        [Header("Memory Optimization")]
        [SerializeField] private bool enableMemoryOptimization = true;
        [SerializeField] private bool enableGCPrevention = true;

        #endregion

        #region Private Fields

        private Rigidbody _rigidbody;
        private Collider _collider;
        private Coroutine _currentAnimationCoroutine;
        private CoinAnimationState _currentState = CoinAnimationState.Idle;
        private int _coinId = -1;

        // Memory management integration
        private MemoryManagementSystem _memorySystem;
        private CoinObjectPool _objectPool;
        private bool _isMemoryTracked = false;

        #endregion

        #region Properties

        public CoinAnimationState CurrentState => _currentState;
        public int CoinId => _coinId;
        public bool IsAnimating => _currentAnimationCoroutine != null;
        public bool IsMemoryOptimized => enableMemoryOptimization && _memorySystem != null;

        #endregion

        #region Events

        public event EventHandler<CoinAnimationEventArgs> OnStateChanged;

        #endregion

        #region Initialization

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            
            // Find memory and pool systems
            _memorySystem = FindObjectOfType<MemoryManagementSystem>();
            _objectPool = FindObjectOfType<CoinObjectPool>();
        }

        private void Start()
        {
            // Register with memory tracking system if available
            RegisterWithMemorySystem();
            
            // Legacy registration for backward compatibility
            if (CoinAnimationManager.Instance != null)
            {
                _coinId = CoinAnimationManager.Instance.RegisterCoin(this);
            }
        }

        private void RegisterWithMemorySystem()
        {
            if (enableMemoryOptimization && _memorySystem != null && !_isMemoryTracked)
            {
                _memorySystem.TrackObject(this, "CoinAnimationController");
                _isMemoryTracked = true;
            }
        }

        private void UnregisterFromMemorySystem()
        {
            if (enableMemoryOptimization && _memorySystem != null && _isMemoryTracked)
            {
                _memorySystem.UntrackObject(this, "CoinAnimationController");
                _isMemoryTracked = false;
            }
        }

        #endregion

        #region State Management

        public void SetState(CoinAnimationState newState)
        {
            if (_currentState == newState) return;

            var previousState = _currentState;
            _currentState = newState;
            
            var args = new CoinAnimationEventArgs(previousState, newState, 0f, true);
            OnStateChanged?.Invoke(this, args);

            // Handle memory-related state changes
            HandleMemoryStateChange(previousState, newState);
        }

        private void HandleMemoryStateChange(CoinAnimationState previousState, CoinAnimationState newState)
        {
            if (!enableMemoryOptimization) return;

            switch (newState)
            {
                case CoinAnimationState.Moving:
                    // Consider GC prevention for movement animations
                    if (enableGCPrevention && _memorySystem != null)
                    {
                        _memorySystem.EnableGCPrevention();
                    }
                    break;

                case CoinAnimationState.Collecting:
                    // Enable GC prevention for complex collection animations
                    if (enableGCPrevention && _memorySystem != null)
                    {
                        _memorySystem.EnableGCPrevention();
                    }
                    break;

                case CoinAnimationState.Pooled:
                    // Disable GC prevention when coin returns to pool
                    if (_memorySystem != null)
                    {
                        _memorySystem.DisableGCPrevention();
                    }
                    break;

                case CoinAnimationState.Idle:
                    // Allow normal GC when idle
                    if (_memorySystem != null)
                    {
                        _memorySystem.DisableGCPrevention();
                    }
                    break;
            }
        }

        #endregion

        #region Animation Methods

        /// <summary>
        /// 动画移动到目标位置
        /// </summary>
        public void AnimateToPosition(Vector3 targetPosition, float duration)
        {
            StopCurrentAnimation();
            SetState(CoinAnimationState.Moving);

            _currentAnimationCoroutine = StartCoroutine(MoveToPositionCoroutine(targetPosition, duration / animationSpeed));
        }

        /// <summary>
        /// 收集金币动画
        /// </summary>
        public void CollectCoin(Vector3 collectionPoint, float duration = 1f)
        {
            StopCurrentAnimation();
            SetState(CoinAnimationState.Collecting);

            // Enable GC prevention for smooth animation
            if (enableGCPrevention && _memorySystem != null)
            {
                _memorySystem.EnableGCPrevention();
            }

            _currentAnimationCoroutine = StartCoroutine(CollectCoinCoroutine(collectionPoint, duration / animationSpeed));
        }

        /// <summary>
        /// 移动协程
        /// </summary>
        private IEnumerator MoveToPositionCoroutine(Vector3 targetPosition, float duration)
        {
            Vector3 startPosition = transform.position;
            Vector3 startRotation = transform.eulerAngles;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 使用简单的缓动函数
                t = EaseOutQuad(t);

                // 位置插值
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);

                // 旋转动画
                float rotationAmount = rotationSpeed * duration;
                transform.eulerAngles = startRotation + Vector3.up * (rotationAmount * t);

                yield return null;
            }

            transform.position = targetPosition;
            SetState(CoinAnimationState.Idle);
            _currentAnimationCoroutine = null;
        }

        /// <summary>
        /// 收集金币协程
        /// </summary>
        private IEnumerator CollectCoinCoroutine(Vector3 collectionPoint, float duration)
        {
            Vector3 startPosition = transform.position;
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            // 阶段1: 放大 + 移动到目标点 (80%的时间)
            float scaleUpDuration = duration * 0.3f;
            float moveDuration = duration * 0.7f;

            // 放大阶段
            elapsed = 0f;
            while (elapsed < scaleUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / scaleUpDuration;
                t = EaseOutBack(t);

                transform.localScale = Vector3.Lerp(startScale, startScale * 1.5f, t);
                yield return null;
            }

            // 移动到收集点
            Vector3 moveStartPosition = transform.position;
            elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / moveDuration;
                t = EaseInSine(t);

                transform.position = Vector3.Lerp(moveStartPosition, collectionPoint, t);
                yield return null;
            }

            // 阶段2: 缩小消失 (20%的时间)
            float scaleDownDuration = duration * 0.2f;
            elapsed = 0f;
            Vector3 scaleDownStart = transform.localScale;

            while (elapsed < scaleDownDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / scaleDownDuration;
                t = EaseInBack(t);

                transform.localScale = Vector3.Lerp(scaleDownStart, Vector3.zero, t);
                yield return null;
            }

            // 播放收集效果
            PlayCollectionEffects();

            // 通知收集完成
            if (CoinAnimationManager.Instance != null)
            {
                CoinAnimationManager.Instance.TriggerCollectionComplete(_coinId, collectionPoint);
            }

            // Disable GC prevention after animation completes
            if (_memorySystem != null)
            {
                _memorySystem.DisableGCPrevention();
            }

            SetState(CoinAnimationState.Pooled);
            gameObject.SetActive(false);
            _currentAnimationCoroutine = null;
        }

        #endregion

        #region Effects

        private void PlayCollectionEffects()
        {
            if (collectionEffect != null)
            {
                collectionEffect.Play();
            }

            if (collectionAudio != null && collectionAudio.clip != null)
            {
                collectionAudio.Play();
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 停止当前动画
        /// </summary>
        public void StopCurrentAnimation()
        {
            if (_currentAnimationCoroutine != null)
            {
                StopCoroutine(_currentAnimationCoroutine);
                _currentAnimationCoroutine = null;
            }

            // Disable GC prevention when animation stops
            if (_memorySystem != null)
            {
                _memorySystem.DisableGCPrevention();
            }
        }

        /// <summary>
        /// 暂停动画
        /// </summary>
        public void PauseAnimations()
        {
            SetState(CoinAnimationState.Paused);
        }

        /// <summary>
        /// 恢复动画
        /// </summary>
        public void ResumeAnimations()
        {
            SetState(CoinAnimationState.Moving);
        }

        /// <summary>
        /// Return this coin to the pool manually
        /// </summary>
        public void ReturnToPool()
        {
            if (_objectPool != null)
            {
                _objectPool.ReturnCoin(gameObject);
            }
            else
            {
                // Fallback: disable and let manager handle
                gameObject.SetActive(false);
                SetState(CoinAnimationState.Pooled);
            }
        }

        #endregion

        #region Easing Functions

        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private float EaseInSine(float t)
        {
            return 1f - Mathf.Cos((t * Mathf.PI) / 2f);
        }

        private float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t;
        }

        #endregion

        #region Memory Optimization

        /// <summary>
        /// Optimize memory usage for this coin
        /// </summary>
        public void OptimizeMemoryUsage()
        {
            if (!enableMemoryOptimization) return;

            // Stop any ongoing animations
            StopCurrentAnimation();

            // Return to idle state
            SetState(CoinAnimationState.Idle);

            // Force cleanup
            if (collectionEffect != null && collectionEffect.isPlaying)
            {
                collectionEffect.Stop();
            }

            if (collectionAudio != null && collectionAudio.isPlaying)
            {
                collectionAudio.Stop();
            }

            // Reset transform to default
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            // Reset physics
            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Get memory usage information for this coin
        /// </summary>
        public CoinMemoryInfo GetMemoryInfo()
        {
            return new CoinMemoryInfo
            {
                CoinId = _coinId,
                CurrentState = _currentState,
                IsAnimating = IsAnimating,
                IsMemoryOptimized = IsMemoryOptimized,
                IsMemoryTracked = _isMemoryTracked,
                HasActiveEffects = (collectionEffect != null && collectionEffect.isPlaying) ||
                                 (collectionAudio != null && collectionAudio.isPlaying),
                Timestamp = DateTime.UtcNow
            };
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            StopCurrentAnimation();

            // Unregister from memory tracking system
            UnregisterFromMemorySystem();

            if (_coinId != -1 && CoinAnimationManager.Instance != null)
            {
                CoinAnimationManager.Instance.UnregisterCoin(_coinId);
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Memory information for individual coin
    /// </summary>
    [Serializable]
    public class CoinMemoryInfo
    {
        public int CoinId;
        public CoinAnimationState CurrentState;
        public bool IsAnimating;
        public bool IsMemoryOptimized;
        public bool IsMemoryTracked;
        public bool HasActiveEffects;
        public DateTime Timestamp;
    }

    #endregion
}