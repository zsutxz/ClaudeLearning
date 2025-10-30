using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoinAnimation.Animation
{
    /// <summary>
    /// DOTween Manager for initializing and managing DOTween animations
    /// Handles AC3: DOTween animation framework integration and accessibility
    /// </summary>
    public class DOTweenManager : MonoBehaviour
    {
        [Header("DOTween Configuration")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private DOTweenSettings defaultSettings;
        
        [Header("Performance Settings")]
        [SerializeField] private int maxConcurrentTweens = 100;
        [SerializeField] private float tweenUpdateRate = 60f;
        
        [Header("Debug")]
        [SerializeField] private bool enableDOTweenLogging = false;
        
        public static DOTweenManager Instance { get; private set; }
        public DOTweenSettings DefaultSettings => defaultSettings;
        
        private int activeTweenCount = 0;
        
        public event Action<int> OnTweenCountChanged;
        public event Action<Tween> OnTweenStarted;
        public event Action<Tween> OnTweenCompleted;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                if (autoInitialize)
                {
                    InitializeDOTween();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Initialize DOTween with optimal settings for coin animation system
        /// </summary>
        public void InitializeDOTween()
        {
            // Configure DOTween global settings
            DOTween.SetTweensCapacity(maxConcurrentTweens, 50);
            
            // Set time scale to match Unity time
            DOTween.timeScale = Time.timeScale;
            
            // Configure update rate for performance
            DOTween.useSmoothDeltaTime = false;
            
            // Set up safe mode for debugging
            DOTween.safeMode = enableDOTweenLogging;
            
            // Initialize default settings if not set
            if (defaultSettings == null)
            {
                defaultSettings = CreateDefaultSettings();
            }
            
            // Set up global callbacks
            DOTween.OnTweenCreated += OnTweenCreatedCallback;
            DOTween.OnTweenComplete += OnTweenCompleteCallback;
            
            Debug.Log("DOTween initialized successfully for coin animation system");
        }
        
        /// <summary>
        /// Create a coin collection animation with magnetic effect
        /// </summary>
        /// <param name="coin">Coin transform to animate</param>
        /// <param name="target">Target position for collection</param>
        /// <param name="duration">Animation duration</param>
        /// <param name="onComplete">Completion callback</param>
        /// <returns>Animation sequence</returns>
        public Sequence CreateCoinCollectionAnimation(Transform coin, Transform target, float duration = 2f, Action onComplete = null)
        {
            var sequence = DOTween.Sequence();
            
            // Calculate spiral path for magnetic collection
            var spiralPath = GenerateSpiralPath(coin.position, target.position, duration);
            
            // Add spiral movement
            sequence.Append(coin.DOPath(spiralPath, duration * 0.8f, PathType.CatmullRom)
                .SetEase(Ease.OutSine)
                .SetLookAt(0.01f, target.position));
            
            // Add spin animation
            sequence.Join(coin.DOLocalRotate(new Vector3(0, 360, 0), duration * 0.6f, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutSine));
            
            // Add scale effect at the end
            sequence.Append(coin.DOScale(Vector3.zero, duration * 0.2f)
                .SetEase(Ease.InBack));
            
            // Callback
            if (onComplete != null)
            {
                sequence.OnComplete(onComplete);
            }
            
            return sequence;
        }
        
        /// <summary>
        /// Create coin spawn animation
        /// </summary>
        /// <param name="coin">Coin transform to animate</param>
        /// <param name="spawnPosition">Spawn position</param>
        /// <param name="onComplete">Completion callback</param>
        /// <returns>Animation tween</returns>
        public Tween CreateCoinSpawnAnimation(Transform coin, Vector3 spawnPosition, Action onComplete = null)
        {
            // Set initial state
            coin.position = spawnPosition;
            coin.localScale = Vector3.zero;
            coin.rotation = UnityEngine.Random.rotation;
            
            // Create spawn animation
            var tween = coin.DOScale(Vector3.one, 0.5f)
                .SetEase(Ease.OutBack)
                .From(Vector3.zero)
                .OnComplete(() => onComplete?.Invoke());
            
            return tween;
        }
        
        /// <summary>
        /// Create bounce animation for coin collection feedback
        /// </summary>
        /// <param name="target">Target transform to bounce</param>
        /// <param name="onComplete">Completion callback</param>
        /// <returns>Animation sequence</returns>
        public Sequence CreateBounceFeedbackAnimation(Transform target, Action onComplete = null)
        {
            var sequence = DOTween.Sequence();
            
            // Bounce up
            sequence.Append(target.DOScale(Vector3.one * 1.2f, 0.1f)
                .SetEase(Ease.OutQuad));
            
            // Bounce down
            sequence.Append(target.DOScale(Vector3.one * 0.9f, 0.1f)
                .SetEase(Ease.InQuad));
            
            // Return to normal
            sequence.Append(target.DOScale(Vector3.one, 0.1f)
                .SetEase(Ease.OutQuad));
            
            if (onComplete != null)
            {
                sequence.OnComplete(onComplete);
            }
            
            return sequence;
        }
        
        /// <summary>
        /// Create floating text animation for coin value display
        /// </summary>
        /// <param name="text">Text transform to animate</param>
        /// <param name="endPosition">End position for floating effect</param>
        /// <param name="onComplete">Completion callback</param>
        /// <returns>Animation sequence</returns>
        public Sequence CreateFloatingTextAnimation(Transform text, Vector3 endPosition, Action onComplete = null)
        {
            var sequence = DOTween.Sequence();
            
            // Fade in and move up
            sequence.Append(text.DOMove(endPosition, 1.5f)
                .SetEase(Ease.OutQuad));
            
            sequence.Join(text.DOFade(0f, 1.5f)
                .From(1f)
                .SetEase(Ease.InQuad));
            
            // Slight scale pulse
            sequence.Join(text.DOScale(Vector3.one * 1.1f, 0.3f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutSine));
            
            if (onComplete != null)
            {
                sequence.OnComplete(onComplete);
            }
            
            return sequence;
        }
        
        /// <summary>
        /// Generate spiral path for magnetic coin collection
        /// </summary>
        /// <param name="startPos">Starting position</param>
        /// <param name="targetPos">Target position</param>
        /// <param name="duration">Animation duration</param>
        /// <returns>Path points array</returns>
        private Vector3[] GenerateSpiralPath(Vector3 startPos, Vector3 targetPos, float duration)
        {
            var points = new Vector3[20];
            var distance = Vector3.Distance(startPos, targetPos);
            var direction = (targetPos - startPos).normalized;
            
            for (int i = 0; i < points.Length; i++)
            {
                var t = (float)i / (points.Length - 1);
                var basePos = Vector3.Lerp(startPos, targetPos, t);
                
                // Add spiral effect
                var spiralOffset = Vector3.Cross(direction, Vector3.up).normalized * 
                                 Mathf.Sin(t * Mathf.PI * 4) * distance * 0.3f * (1f - t);
                
                var verticalOffset = Vector3.up * Mathf.Sin(t * Mathf.PI * 2) * distance * 0.1f * (1f - t);
                
                points[i] = basePos + spiralOffset + verticalOffset;
            }
            
            return points;
        }
        
        /// <summary>
        /// Create default DOTween settings for coin animations
        /// </summary>
        /// <returns>Default settings</returns>
        private DOTweenSettings CreateDefaultSettings()
        {
            return new DOTweenSettings
            {
                defaultEaseType = Ease.OutQuad,
                defaultAutoKill = true,
                defaultUpdateType = UpdateType.Normal,
                defaultTimeScaleIndependent = false,
                useSmoothDeltaTime = false,
                maxConcurrentTweens = maxConcurrentTweens,
                defaultTweenDuration = 1f
            };
        }
        
        /// <summary>
        /// Stop all DOTween animations
        /// </summary>
        /// <param name="complete">Whether to complete animations immediately</param>
        public void StopAllAnimations(bool complete = false)
        {
            if (complete)
            {
                DOTween.CompleteAll();
            }
            else
            {
                DOTween.KillAll();
            }
            
            activeTweenCount = 0;
            OnTweenCountChanged?.Invoke(activeTweenCount);
        }
        
        /// <summary>
        /// Get current animation statistics
        /// </summary>
        /// <returns>Animation statistics</returns>
        public DOTweenStatistics GetStatistics()
        {
            return new DOTweenStatistics
            {
                activeTweens = activeTweenCount,
                maxConcurrentTweens = maxConcurrentTweens,
                memoryUsageEstimate = activeTweenCount * 1024, // Rough estimate
                updateRate = tweenUpdateRate,
                timestamp = DateTime.Now
            };
        }
        
        private void OnTweenCreatedCallback(Tween tween)
        {
            activeTweenCount++;
            OnTweenCountChanged?.Invoke(activeTweenCount);
            OnTweenStarted?.Invoke(tween);
        }
        
        private void OnTweenCompleteCallback(Tween tween)
        {
            activeTweenCount = Mathf.Max(0, activeTweenCount - 1);
            OnTweenCountChanged?.Invoke(activeTweenCount);
            OnTweenCompleted?.Invoke(tween);
        }
        
        private void OnDestroy()
        {
            DOTween.OnTweenCreated -= OnTweenCreatedCallback;
            DOTween.OnTweenComplete -= OnTweenCompleteCallback;
        }
    }
    
    /// <summary>
    /// DOTween configuration settings
    /// </summary>
    [Serializable]
    public class DOTweenSettings
    {
        [Header("Animation Settings")]
        public Ease defaultEaseType = Ease.OutQuad;
        public bool defaultAutoKill = true;
        public UpdateType defaultUpdateType = UpdateType.Normal;
        public bool defaultTimeScaleIndependent = false;
        
        [Header("Performance")]
        public bool useSmoothDeltaTime = false;
        public int maxConcurrentTweens = 100;
        public float defaultTweenDuration = 1f;
    }
    
    /// <summary>
    /// DOTween animation statistics
    /// </summary>
    [Serializable]
    public class DOTweenStatistics
    {
        public int activeTweens;
        public int maxConcurrentTweens;
        public float memoryUsageEstimate;
        public float updateRate;
        public DateTime timestamp;
    }
}