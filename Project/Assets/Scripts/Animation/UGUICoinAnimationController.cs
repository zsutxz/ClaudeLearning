using System;
using System.Collections;
using UnityEngine;
using CoinAnimation.Core;

namespace CoinAnimation.Animation
{
    /// <summary>
    /// UGUI金币动画控制器 - 基于RectTransform的协程实现
    /// 专为UGUI设计，使用RectTransform而不是transform
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class UGUICoinAnimationController : MonoBehaviour
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

        private RectTransform _rectTransform;
        private UnityEngine.UI.Image _image;
        private Coroutine _currentAnimationCoroutine;
        private CoinAnimationState _currentState = CoinAnimationState.Idle;
        private int _coinId = -1;
        private Vector3 _originalScale;

        #endregion

        #region Properties

        public CoinAnimationState CurrentState => _currentState;
        public int CoinId => _coinId;
        public bool IsAnimating => _currentAnimationCoroutine != null;
        public RectTransform RectTransform => _rectTransform;

        #endregion

        #region Events

        public event EventHandler<CoinAnimationEventArgs> OnStateChanged;

        #endregion

        #region Initialization

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<UnityEngine.UI.Image>();
            _originalScale = _rectTransform.localScale;
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

            var oldState = _currentState;
            _currentState = newState;
            OnStateChanged?.Invoke(this, new CoinAnimationEventArgs(oldState, newState, 0f, true));
        }

        #endregion

        #region Animation Methods

        /// <summary>
        /// 动画移动到目标位置（使用anchoredPosition）
        /// </summary>
        public void AnimateToPosition(Vector2 targetAnchoredPosition, float duration)
        {
            StopCurrentAnimation();
            SetState(CoinAnimationState.Moving);

            _currentAnimationCoroutine = StartCoroutine(MoveToPositionCoroutine(targetAnchoredPosition, duration / animationSpeed));
        }

        /// <summary>
        /// 动画移动到目标世界位置
        /// </summary>
        public void AnimateToWorldPosition(Vector3 targetWorldPosition, float duration)
        {
            // 转换世界坐标到屏幕坐标，再转换到anchoredPosition
            Camera camera = Camera.main;
            if (camera == null) camera = Camera.current;
            if (camera == null)
            {
                Debug.LogError("No camera found for world position conversion");
                return;
            }

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, targetWorldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform.parent as RectTransform,
                screenPoint,
                camera,
                out Vector2 localPoint
            );

            AnimateToPosition(localPoint, duration);
        }

        /// <summary>
        /// 收集金币动画
        /// </summary>
        public void CollectCoin(Vector2 collectionAnchoredPosition, float duration = 1f)
        {
            StopCurrentAnimation();
            SetState(CoinAnimationState.Collecting);

            _currentAnimationCoroutine = StartCoroutine(CollectCoinCoroutine(collectionAnchoredPosition, duration / animationSpeed));
        }

        /// <summary>
        /// 收集金币动画（世界坐标版本）
        /// </summary>
        public void CollectCoinWorld(Vector3 collectionWorldPosition, float duration = 1f)
        {
            // 转换世界坐标到屏幕坐标，再转换到anchoredPosition
            Camera camera = Camera.main;
            if (camera == null) camera = Camera.current;
            if (camera == null)
            {
                Debug.LogError("No camera found for world position conversion");
                return;
            }

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, collectionWorldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform.parent as RectTransform,
                screenPoint,
                camera,
                out Vector2 localPoint
            );

            CollectCoin(localPoint, duration);
        }

        /// <summary>
        /// 移动协程
        /// </summary>
        private IEnumerator MoveToPositionCoroutine(Vector2 targetAnchoredPosition, float duration)
        {
            Vector2 startPosition = _rectTransform.anchoredPosition;
            Vector3 startRotation = _rectTransform.eulerAngles;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 使用缓动函数
                t = EaseOutQuad(t);

                // 位置插值
                _rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetAnchoredPosition, t);

                // 旋转动画
                float rotationAmount = rotationSpeed * duration;
                _rectTransform.eulerAngles = startRotation + Vector3.forward * (rotationAmount * t);

                yield return null;
            }

            _rectTransform.anchoredPosition = targetAnchoredPosition;
            SetState(CoinAnimationState.Idle);
            _currentAnimationCoroutine = null;
        }

        /// <summary>
        /// 收集金币协程
        /// </summary>
        private IEnumerator CollectCoinCoroutine(Vector2 collectionAnchoredPosition, float duration)
        {
            Vector2 startPosition = _rectTransform.anchoredPosition;
            Vector3 startScale = _rectTransform.localScale;
            float elapsed = 0f;

            // 阶段1: 放大 + 移动到目标点 (70%的时间)
            float scaleUpDuration = duration * 0.3f;
            float moveDuration = duration * 0.7f;

            // 放大阶段
            elapsed = 0f;
            while (elapsed < scaleUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / scaleUpDuration;
                t = EaseOutBack(t);

                _rectTransform.localScale = Vector3.Lerp(startScale, startScale * 1.5f, t);
                yield return null;
            }

            // 移动到收集点
            Vector2 moveStartPosition = _rectTransform.anchoredPosition;
            elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / moveDuration;
                t = EaseInSine(t);

                _rectTransform.anchoredPosition = Vector2.Lerp(moveStartPosition, collectionAnchoredPosition, t);
                yield return null;
            }

            // 阶段2: 缩小消失 (20%的时间)
            float scaleDownDuration = duration * 0.2f;
            elapsed = 0f;
            Vector3 scaleDownStart = _rectTransform.localScale;

            while (elapsed < scaleDownDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / scaleDownDuration;
                t = EaseInBack(t);

                _rectTransform.localScale = Vector3.Lerp(scaleDownStart, Vector3.zero, t);
                yield return null;
            }

            // 播放收集效果
            PlayCollectionEffects();

            // 通知收集完成
            if (CoinAnimationManager.Instance != null)
            {
                CoinAnimationManager.Instance.TriggerCollectionComplete(_coinId, collectionAnchoredPosition);
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

        /// <summary>
        /// 重置金币到原始状态
        /// </summary>
        public void ResetCoin()
        {
            StopCurrentAnimation();
            _rectTransform.localScale = _originalScale;
            SetState(CoinAnimationState.Idle);
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