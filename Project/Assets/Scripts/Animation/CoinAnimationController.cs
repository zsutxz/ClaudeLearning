using System;
using System.Collections;
using UnityEngine;
using CoinAnimation.Core;

namespace CoinAnimation.Animation
{
    /// <summary>
    /// 金币动画控制器 - 使用协程实现
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

        #endregion

        #region Private Fields

        private Rigidbody _rigidbody;
        private Collider _collider;
        private Coroutine _currentAnimationCoroutine;
        private CoinAnimationState _currentState = CoinAnimationState.Idle;
        private int _coinId = -1;

        #endregion

        #region Properties

        public CoinAnimationState CurrentState => _currentState;
        public int CoinId => _coinId;
        public bool IsAnimating => _currentAnimationCoroutine != null;

        #endregion

        #region Events

        public event EventHandler<CoinAnimationEventArgs> OnStateChanged;

        #endregion

        #region Initialization

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
        }

        private void Start()
        {
            if (CoinAnimationManager.Instance != null)
            {
                _coinId = CoinAnimationManager.Instance.RegisterCoin(this);
            }
        }

        #endregion

        #region State Management

        public void SetState(CoinAnimationState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;
            OnStateChanged?.Invoke(this, new CoinAnimationEventArgs(_currentState, newState, 0f, true));
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

        #region Cleanup

        private void OnDestroy()
        {
            StopCurrentAnimation();

            if (_coinId != -1 && CoinAnimationManager.Instance != null)
            {
                CoinAnimationManager.Instance.UnregisterCoin(_coinId);
            }
        }

        #endregion
    }
}