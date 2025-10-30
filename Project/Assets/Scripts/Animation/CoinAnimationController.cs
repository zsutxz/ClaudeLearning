using System;
using System.Collections;
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
        [SerializeField] private float animationSpeed = 1f;
        [SerializeField] private float rotationSpeed = 360f;

        [Header("Effects")]
        [SerializeField] private ParticleSystem collectionEffect;
        [SerializeField] private AudioSource collectionAudio;

        #endregion

        #region Private Fields

        private Rigidbody _rigidbody;
        private Collider _collider;
        private CoinAnimationState _currentState = CoinAnimationState.Idle;
        private int _coinId = -1;

        #endregion

        #region Properties

        public CoinAnimationState CurrentState => _currentState;
        public int CoinId => _coinId;
        public bool IsAnimating { get; private set; }

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
        /// Animate coin to target position
        /// </summary>
        public void AnimateToPosition(Vector3 targetPosition, float duration)
        {
            SetState(CoinAnimationState.Moving);
            IsAnimating = true;

            transform.DOMove(targetPosition, duration / animationSpeed)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    SetState(CoinAnimationState.Idle);
                    IsAnimating = false;
                });

            transform.DORotate(Vector3.up * rotationSpeed * duration, duration / animationSpeed, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear);
        }

        /// <summary>
        /// Animate coin collection with simple spiral effect
        /// </summary>
        public void CollectCoin(Vector3 collectionPoint, float duration = 1f)
        {
            SetState(CoinAnimationState.Collecting);
            IsAnimating = true;

            Sequence sequence = DOTween.Sequence();

            // Scale up and move
            sequence.Append(transform.DOScale(Vector3.one * 1.5f, duration * 0.3f).SetEase(Ease.OutBack));
            sequence.Join(transform.DOMove(collectionPoint, duration * 0.7f).SetEase(Ease.InSine));

            // Scale down and disappear
            sequence.Append(transform.DOScale(Vector3.zero, duration * 0.2f).SetEase(Ease.InBack))
                .OnComplete(() => {
                    PlayCollectionEffects();
                    SetState(CoinAnimationState.Pooled);
                    IsAnimating = false;

                    if (CoinAnimationManager.Instance != null)
                    {
                        CoinAnimationManager.Instance.TriggerCollectionComplete(_coinId, collectionPoint);
                    }
                });
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

        #region Cleanup

        private void OnDestroy()
        {
            transform.DOKill();

            if (_coinId != -1 && CoinAnimationManager.Instance != null)
            {
                CoinAnimationManager.Instance.UnregisterCoin(_coinId);
            }
        }

        #endregion
    }
}