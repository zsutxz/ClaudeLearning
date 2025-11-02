using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoinAnimation.Core;
using CoinAnimation.Animation;

namespace CoinAnimation.AdaptiveQuality
{
    /// <summary>
    /// 平滑质量过渡系统
    /// Story 2.1 Task 2.4 - 实现无缝质量过渡，避免视觉干扰和用户体验断裂
    /// </summary>
    public class SmoothQualityTransition : MonoBehaviour
    {
        #region Configuration

        [Header("Transition Settings")]
        [SerializeField] private bool enableSmoothTransitions = true;
        [SerializeField] private float defaultTransitionDuration = 1.5f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool prioritizeCriticalTransitions = true;

        [Header("Visual Smoothing")]
        [SerializeField] private bool enableVisualSmoothing = true;
        [SerializeField] private float fadeTransitionDuration = 0.8f;
        [SerializeField] private bool enableCameraEffectCompensation = true;
        [SerializeField] private bool enableParticleGradualAdjustment = true;

        [Header("Performance Smoothing")]
        [SerializeField] private bool enablePerformanceSmoothing = true;
        [SerializeField] private int transitionSteps = 10;
        [SerializeField] private float stepInterval = 0.1f;
        [SerializeField] private bool enableLoadBalancing = true;

        [Header("Safety and Fallback")]
        [SerializeField] private float maxTransitionTime = 5.0f;
        [SerializeField] private bool enableEmergencySkip = true;
        [SerializeField] private float emergencySkipThreshold = 3.0f;

        #endregion

        #region Private Fields

        // 组件引用
        private AdaptiveQualityManager _qualityManager;
        private RealTimeQualityAdjuster _realTimeAdjuster;
        //private CoinAnimationManager _animationManager;
        private CoinObjectPool _objectPool;
        private Camera _mainCamera;

        // 过渡状态
        private bool _transitionInProgress = false;
        private QualityTransition _currentTransition;
        private Queue<QualityTransition> _transitionQueue = new Queue<QualityTransition>();

        // 平滑参数
        private readonly Dictionary<string, float> _currentValues = new Dictionary<string, float>();
        private readonly Dictionary<string, float> _targetValues = new Dictionary<string, float>();
        private readonly Dictionary<string, Coroutine> _activeCoroutines = new Dictionary<string, Coroutine>();

        // 视觉效果
        private FadeEffectController _fadeController;
        private CameraEffectController _cameraEffectController;
        private ParticleTransitionController _particleController;

        // 统计和监控
        private TransitionStatistics _statistics = new TransitionStatistics();
        private PerformanceMonitor _performanceMonitor;

        #endregion

        #region Properties

        public bool IsTransitionInProgress => _transitionInProgress;
        public QualityTransition CurrentTransition => _currentTransition;
        public TransitionStatistics Statistics => _statistics;
        public int QueuedTransitions => _transitionQueue.Count;

        #endregion

        #region Events

        public event Action<QualityTransition> OnTransitionStarted;
        public event Action<QualityTransition, float> OnTransitionProgress;
        public event Action<QualityTransition, bool> OnTransitionCompleted;
        public event Action<string> OnTransitionSkipped;
        public event Action<TransitionStatistics> OnStatisticsUpdated;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeControllers();
        }

        private void Start()
        {
            FindSystemComponents();
            InitializeCurrentValues();
        }

        private void Update()
        {
            if (!enableSmoothTransitions) return;

            UpdateTransitions();
            MonitorTransitionPerformance();
        }

        #endregion

        #region Initialization

        private void InitializeControllers()
        {
            _fadeController = gameObject.AddComponent<FadeEffectController>();
            _cameraEffectController = gameObject.AddComponent<CameraEffectController>();
            _particleController = gameObject.AddComponent<ParticleTransitionController>();
        }

        private void FindSystemComponents()
        {
            _qualityManager = FindObjectOfType<AdaptiveQualityManager>();
            _realTimeAdjuster = FindObjectOfType<RealTimeQualityAdjuster>();
            _animationManager = FindObjectOfType<CoinAnimationManager>();
            _objectPool = FindObjectOfType<CoinObjectPool>();
            _performanceMonitor = FindObjectOfType<PerformanceMonitor>();

            _mainCamera = Camera.main;
            if (_mainCamera == null)
                _mainCamera = FindObjectOfType<Camera>();

            Debug.Log("[SmoothQualityTransition] Component discovery complete");
        }

        private void InitializeCurrentValues()
        {
            // 初始化当前值字典
            _currentValues["MaxCoinCount"] = _objectPool?.MaxPoolSize ?? 50f;
            _currentValues["EffectIntensity"] = 1.0f;
            _currentValues["AnimationSpeed"] = 1.0f;
            _currentValues["RenderQuality"] = QualitySettings.GetQualityLevel();
            _currentValues["ShadowQuality"] = (float)QualitySettings.shadows;
            _currentValues["AntiAliasing"] = QualitySettings.antiAliasing;

            // 复制到目标值
            foreach (var kvp in _currentValues)
            {
                _targetValues[kvp.Key] = kvp.Value;
            }
        }

        #endregion

        #region Public Transition API

        /// <summary>
        /// 开始质量过渡
        /// </summary>
        public void StartTransition(QualityLevel fromLevel, QualityLevel toLevel, TransitionPriority priority = TransitionPriority.Normal)
        {
            var transition = new QualityTransition
            {
                FromLevel = fromLevel,
                ToLevel = toLevel,
                Priority = priority,
                StartTime = DateTime.UtcNow,
                Duration = CalculateTransitionDuration(fromLevel, toLevel, priority),
                TransitionType = DetermineTransitionType(fromLevel, toLevel)
            };

            StartTransition(transition);
        }

        /// <summary>
        /// 开始自定义质量过渡
        /// </summary>
        public void StartTransition(QualityTransition transition)
        {
            if (!enableSmoothTransitions)
            {
                Debug.LogWarning("[SmoothQualityTransition] Smooth transitions are disabled");
                return;
            }

            if (_transitionInProgress && transition.Priority <= TransitionPriority.Normal)
            {
                // 普通优先级过渡需要排队
                _transitionQueue.Enqueue(transition);
                Debug.Log($"[SmoothQualityTransition] Transition queued: {transition.FromLevel} -> {transition.ToLevel}");
            }
            else
            {
                // 高优先级或没有进行中的过渡，立即开始
                if (_transitionInProgress && transition.Priority > TransitionPriority.Normal)
                {
                    Debug.Log($"[SmoothQualityTransition] Interrupting current transition for high priority one");
                    InterruptCurrentTransition();
                }

                StartCoroutine(ExecuteTransition(transition));
            }
        }

        /// <summary>
        /// 立即跳转到目标质量（无过渡）
        /// </summary>
        public void SkipTransition(QualityLevel targetLevel)
        {
            if (_transitionInProgress)
            {
                InterruptCurrentTransition();
            }

            Debug.Log($"[SmoothQualityTransition] Skipping transition to {targetLevel}");
            ApplyQualityLevelImmediately(targetLevel);
            OnTransitionSkipped?.Invoke($"Skipped to {targetLevel}");
        }

        /// <summary>
        /// 暂停当前过渡
        /// </summary>
        public void PauseTransition()
        {
            if (_transitionInProgress && _currentTransition != null)
            {
                _currentTransition.IsPaused = true;
                _currentTransition.PauseTime = DateTime.UtcNow;
                Debug.Log("[SmoothQualityTransition] Transition paused");
            }
        }

        /// <summary>
        /// 恢复暂停的过渡
        /// </summary>
        public void ResumeTransition()
        {
            if (_transitionInProgress && _currentTransition != null && _currentTransition.IsPaused)
            {
                _currentTransition.IsPaused = false;
                var pauseDuration = (float)(DateTime.UtcNow - _currentTransition.PauseTime.Value).TotalSeconds;
                _currentTransition.StartTime = _currentTransition.StartTime.AddSeconds(pauseDuration);
                Debug.Log("[SmoothQualityTransition] Transition resumed");
            }
        }

        #endregion

        #region Transition Execution

        private IEnumerator ExecuteTransition(QualityTransition transition)
        {
            if (_transitionInProgress)
                yield break;

            _transitionInProgress = true;
            _currentTransition = transition;

            Debug.Log($"[SmoothQualityTransition] Starting transition: {transition.FromLevel} -> {transition.ToLevel} ({transition.Duration}s)");
            OnTransitionStarted?.Invoke(transition);

            var startTime = Time.time;
            var actualDuration = transition.Duration;
            var transitionCompleted = false;

            // 使用单独的方法执行过渡阶段，避免 yield return 在 try-catch 中
            yield return StartCoroutine(ExecuteTransitionPhases(transition, startTime, actualDuration,
                success => transitionCompleted = success));

            // 完成处理
            var totalDuration = Time.time - startTime;
            FinishTransition(transition, transitionCompleted, totalDuration);
        }

        private IEnumerator ExecuteTransitionPhases(QualityTransition transition, float startTime, float actualDuration, Action<bool> onComplete)
        {
            bool success = false;

            try
            {
                // 预处理阶段
                yield return StartCoroutine(PreTransitionPhase(transition));

                // 主要过渡阶段
                yield return StartCoroutine(MainTransitionPhase(transition, startTime, actualDuration));

                // 后处理阶段
                yield return StartCoroutine(PostTransitionPhase(transition));

                success = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SmoothQualityTransition] Transition failed: {e.Message}");
                transition.HasError = true;
                transition.ErrorMessage = e.Message;
                success = false;
            }

            onComplete?.Invoke(success);
        }

        private IEnumerator PreTransitionPhase(QualityTransition transition)
        {
            Debug.Log("[SmoothQualityTransition] Pre-transition phase");

            // 准备目标值
            CalculateTargetValues(transition);

            // 准备视觉效果
            if (enableVisualSmoothing)
            {
                yield return StartCoroutine(_fadeController.PrepareFade(fadeTransitionDuration));
            }

            // 准备相机效果
            if (enableCameraEffectCompensation && _cameraEffectController != null)
            {
                _cameraEffectController.PrepareCompensation(transition);
            }

            yield return null;
        }

        private IEnumerator MainTransitionPhase(QualityTransition transition, float startTime, float duration)
        {
            Debug.Log($"[SmoothQualityTransition] Main transition phase ({duration}s)");

            var emergencyStartTime = Time.time;
            var lastStepTime = Time.time;

            while (Time.time - startTime < duration)
            {
                if (transition.IsPaused)
                {
                    yield return null;
                    continue;
                }

                // 检查紧急跳过条件
                if (enableEmergencySkip && (Time.time - emergencyStartTime) > emergencySkipThreshold)
                {
                    Debug.LogWarning("[SmoothQualityTransition] Emergency skip triggered - transition taking too long");
                    break;
                }

                // 更新过渡进度
                var progress = Mathf.Clamp01((Time.time - startTime) / duration);
                var curvedProgress = transitionCurve.Evaluate(progress);

                // 应用过渡步骤
                if (Time.time - lastStepTime >= stepInterval)
                {
                    ApplyTransitionStep(curvedProgress);
                    lastStepTime = Time.time;

                    // 触发进度事件
                    OnTransitionProgress?.Invoke(transition, progress);
                }

                yield return null;
            }

            // 确保最终值被应用
            ApplyTransitionStep(1.0f);
        }

        private IEnumerator PostTransitionPhase(QualityTransition transition)
        {
            Debug.Log("[SmoothQualityTransition] Post-transition phase");

            // 确保所有最终设置被正确应用
            ApplyFinalQualitySettings(transition);

            // 清理视觉效果
            if (enableVisualSmoothing)
            {
                yield return StartCoroutine(_fadeController.CompleteFade(fadeTransitionDuration));
            }

            // 清理相机效果
            if (enableCameraEffectCompensation && _cameraEffectController != null)
            {
                _cameraEffectController.CleanupCompensation();
            }

            yield return null;
        }

        #endregion

        #region Transition Logic

        private void CalculateTargetValues(QualityTransition transition)
        {
            var fromPreset = GetQualityPreset(transition.FromLevel);
            var toPreset = GetQualityPreset(transition.ToLevel);

            _targetValues["MaxCoinCount"] = toPreset.MaxCoinCount;
            _targetValues["EffectIntensity"] = toPreset.EffectIntensityMultiplier;
            _targetValues["AnimationSpeed"] = toPreset.AnimationQuality;
            _targetValues["RenderQuality"] = toPreset.UnityQualityLevel;
            _targetValues["ShadowQuality"] = (float)toPreset.ShadowQuality;
            _targetValues["AntiAliasing"] = toPreset.AntiAliasing;

            // 存储转换信息
            transition.FromValues = new Dictionary<string, float>(_currentValues);
            transition.ToValues = new Dictionary<string, float>(_targetValues);
        }

        private void ApplyTransitionStep(float progress)
        {
            foreach (var key in _currentValues.Keys)
            {
                if (_targetValues.ContainsKey(key))
                {
                    var currentValue = _currentValues[key];
                    var targetValue = _targetValues[key];
                    var newValue = Mathf.Lerp(currentValue, targetValue, progress);

                    // 应用平滑步进
                    ApplySmoothedValue(key, newValue, progress);
                }
            }
        }

        private void ApplySmoothedValue(string key, float value, float progress)
        {
            switch (key)
            {
                case "MaxCoinCount":
                    if (_objectPool != null)
                    {
                        var newMaxCoins = Mathf.RoundToInt(value);
                        _objectPool.SetMaxPoolSize(newMaxCoins);
                    }
                    break;

                case "EffectIntensity":
                    ApplyEffectIntensity(value, progress);
                    break;

                case "AnimationSpeed":
                    ApplyAnimationSpeed(value, progress);
                    break;

                case "RenderQuality":
                    if (progress >= 1.0f) // 只在最后一步应用渲染质量
                    {
                        QualitySettings.SetQualityLevel(Mathf.RoundToInt(value), true);
                    }
                    break;

                case "ShadowQuality":
                    if (progress >= 1.0f) // 只在最后一步应用阴影质量
                    {
                        QualitySettings.shadows = (ShadowQuality)Mathf.RoundToInt(value);
                    }
                    break;

                case "AntiAliasing":
                    if (progress >= 1.0f) // 只在最后一步应用抗锯齿
                    {
                        QualitySettings.antiAliasing = Mathf.RoundToInt(value);
                    }
                    break;
            }

            _currentValues[key] = value;
        }

        private void ApplyEffectIntensity(float intensity, float progress)
        {
            if (!enableParticleGradualAdjustment) return;

            // 调整粒子系统
            var particleSystems = FindObjectsOfType<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                if (ps.gameObject.name.Contains("Coin") || ps.gameObject.name.Contains("Effect"))
                {
                    var main = ps.main;
                    var baseIntensity = _currentValues.ContainsKey("EffectIntensity") ? _currentValues["EffectIntensity"] : 1.0f;
                    var newIntensity = Mathf.Lerp(baseIntensity, intensity, progress);
                    
                    // 逐步调整粒子数量
                    var currentMax = main.maxParticles;
                    var targetMax = Mathf.RoundToInt(currentMax * newIntensity);
                    main.maxParticles = Mathf.Max(targetMax, 1);
                }
            }
        }

        private void ApplyAnimationSpeed(float speed, float progress)
        {
            // 调整动画器速度
            var animators = FindObjectsOfType<Animator>();
            foreach (var animator in animators)
            {
                if (animator.gameObject.name.Contains("Coin"))
                {
                    var baseSpeed = _currentValues.ContainsKey("AnimationSpeed") ? _currentValues["AnimationSpeed"] : 1.0f;
                    var newSpeed = Mathf.Lerp(baseSpeed, speed, progress);
                    animator.speed = Mathf.Max(0.1f, newSpeed);
                }
            }
        }

        private void ApplyFinalQualitySettings(QualityTransition transition)
        {
            // 确保所有最终设置被正确应用
            var toPreset = GetQualityPreset(transition.ToLevel);

            if (_objectPool != null)
            {
                _objectPool.SetMaxPoolSize(toPreset.MaxCoinCount);
            }

            QualitySettings.SetQualityLevel(toPreset.UnityQualityLevel, true);
            QualitySettings.shadows = toPreset.ShadowQuality;
            QualitySettings.antiAliasing = toPreset.AntiAliasing;

            // 更新当前值
            foreach (var kvp in _targetValues)
            {
                _currentValues[kvp.Key] = kvp.Value;
            }
        }

        #endregion

        #region Utility Methods

        private float CalculateTransitionDuration(QualityLevel from, QualityLevel to, TransitionPriority priority)
        {
            var baseDuration = defaultTransitionDuration;

            // 根据质量等级差异调整持续时间
            var levelDifference = Mathf.Abs((int)from - (int)to);
            baseDuration *= (1f + levelDifference * 0.3f);

            // 根据优先级调整
            switch (priority)
            {
                case TransitionPriority.Critical:
                    baseDuration *= 0.3f; // 快速过渡
                    break;
                case TransitionPriority.High:
                    baseDuration *= 0.6f;
                    break;
                case TransitionPriority.Low:
                    baseDuration *= 1.5f; // 缓慢过渡
                    break;
            }

            return Mathf.Min(baseDuration, maxTransitionTime);
        }

        private TransitionType DetermineTransitionType(QualityLevel from, QualityLevel to)
        {
            if (to > from)
                return TransitionType.Upgrade;
            else if (to < from)
                return TransitionType.Downgrade;
            else
                return TransitionType.Maintenance;
        }

        private QualityPreset GetQualityPreset(QualityLevel level)
        {
            return level switch
            {
                QualityLevel.Minimum => QualityPreset.GetMinimumPreset(),
                QualityLevel.Low => QualityPreset.GetLowPreset(),
                QualityLevel.Medium => QualityPreset.GetMediumPreset(),
                QualityLevel.High => QualityPreset.GetHighPreset(),
                _ => QualityPreset.GetMediumPreset()
            };
        }

        private void InterruptCurrentTransition()
        {
            if (_currentTransition != null)
            {
                _currentTransition.IsInterrupted = true;
                _currentTransition.InterruptTime = DateTime.UtcNow;
                Debug.Log("[SmoothQualityTransition] Current transition interrupted");
            }

            // 停止所有活跃的协程
            foreach (var coroutine in _activeCoroutines.Values)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }
            _activeCoroutines.Clear();

            _transitionInProgress = false;
        }

        private void ApplyQualityLevelImmediately(QualityLevel targetLevel)
        {
            var preset = GetQualityPreset(targetLevel);
            
            if (_objectPool != null)
            {
                _objectPool.SetMaxPoolSize(preset.MaxCoinCount);
            }

            QualitySettings.SetQualityLevel(preset.UnityQualityLevel, true);
            QualitySettings.shadows = preset.ShadowQuality;
            QualitySettings.antiAliasing = preset.AntiAliasing;

            // 更新当前值
            _currentValues["MaxCoinCount"] = preset.MaxCoinCount;
            _currentValues["EffectIntensity"] = preset.EffectIntensityMultiplier;
            _currentValues["AnimationSpeed"] = preset.AnimationQuality;
            _currentValues["RenderQuality"] = preset.UnityQualityLevel;
            _currentValues["ShadowQuality"] = (float)preset.ShadowQuality;
            _currentValues["AntiAliasing"] = preset.AntiAliasing;
        }

        #endregion

        #region Monitoring and Statistics

        private void UpdateTransitions()
        {
            // 处理过渡队列
            if (!_transitionInProgress && _transitionQueue.Count > 0)
            {
                var nextTransition = _transitionQueue.Dequeue();
                StartCoroutine(ExecuteTransition(nextTransition));
            }
        }

        private void MonitorTransitionPerformance()
        {
            if (!_transitionInProgress || _currentTransition == null) return;

            var elapsed = (float)(DateTime.UtcNow - _currentTransition.StartTime).TotalSeconds;
            
            // 检查是否超时
            if (elapsed > maxTransitionTime)
            {
                Debug.LogWarning($"[SmoothQualityTransition] Transition timeout detected ({elapsed:F2}s)");
                if (enableEmergencySkip)
                {
                    InterruptCurrentTransition();
                }
            }
        }

        private void FinishTransition(QualityTransition transition, bool success, float actualDuration)
        {
            transition.EndTime = DateTime.UtcNow;
            transition.ActualDuration = actualDuration;
            transition.IsSuccess = success;

            // 更新统计
            _statistics.TotalTransitions++;
            if (success)
                _statistics.SuccessfulTransitions++;
            else
                _statistics.FailedTransitions++;

            _statistics.AverageTransitionDuration = 
                (_statistics.AverageTransitionDuration * (_statistics.TotalTransitions - 1) + actualDuration) / _statistics.TotalTransitions;

            Debug.Log($"[SmoothQualityTransition] Transition completed: {transition.FromLevel} -> {transition.ToLevel} ({actualDuration:F2}s) Success: {success}");

            OnTransitionCompleted?.Invoke(transition, success);
            OnStatisticsUpdated?.Invoke(_statistics);

            // 重置状态
            _transitionInProgress = false;
            _currentTransition = null;

            // 处理队列中的下一个过渡
            if (_transitionQueue.Count > 0)
            {
                var nextTransition = _transitionQueue.Dequeue();
                StartCoroutine(ExecuteTransition(nextTransition));
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// 获取当前过渡状态
        /// </summary>
        public TransitionStatus GetTransitionStatus()
        {
            return new TransitionStatus
            {
                IsTransitionInProgress = _transitionInProgress,
                CurrentTransition = _currentTransition,
                QueuedTransitionsCount = _transitionQueue.Count,
                CurrentValues = new Dictionary<string, float>(_currentValues),
                TargetValues = new Dictionary<string, float>(_targetValues),
                Statistics = _statistics
            };
        }

        /// <summary>
        /// 清空过渡队列
        /// </summary>
        public void ClearTransitionQueue()
        {
            _transitionQueue.Clear();
            Debug.Log("[SmoothQualityTransition] Transition queue cleared");
        }

        /// <summary>
        /// 启用/禁用平滑过渡
        /// </summary>
        public void SetSmoothTransitionsEnabled(bool enabled)
        {
            enableSmoothTransitions = enabled;
            Debug.Log($"[SmoothQualityTransition] Smooth transitions {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 重置统计信息
        /// </summary>
        public void ResetStatistics()
        {
            _statistics = new TransitionStatistics();
            Debug.Log("[SmoothQualityTransition] Statistics reset");
        }

        #endregion
    }

    #region Supporting Controllers

    /// <summary>
    /// 淡入淡出效果控制器
    /// </summary>
    public class FadeEffectController : MonoBehaviour
    {
        public IEnumerator PrepareFade(float duration)
        {
            // 准备淡入淡出效果的逻辑
            yield return new WaitForSeconds(0.1f); // 占位实现
        }

        public IEnumerator CompleteFade(float duration)
        {
            // 完成淡入淡出效果的逻辑
            yield return new WaitForSeconds(0.1f); // 占位实现
        }
    }

    /// <summary>
    /// 相机效果补偿控制器
    /// </summary>
    public class CameraEffectController : MonoBehaviour
    {
        public void PrepareCompensation(QualityTransition transition)
        {
            // 准备相机效果补偿的逻辑
        }

        public void CleanupCompensation()
        {
            // 清理相机效果补偿的逻辑
        }
    }

    /// <summary>
    /// 粒子过渡控制器
    /// </summary>
    public class ParticleTransitionController : MonoBehaviour
    {
        // 粒子系统过渡控制的逻辑
    }

    #endregion

    #region Data Structures

    [System.Serializable]
    public class QualityTransition
    {
        public QualityLevel FromLevel;
        public QualityLevel ToLevel;
        public TransitionPriority Priority;
        public TransitionType TransitionType;
        public DateTime StartTime;
        public DateTime? EndTime;
        public DateTime? PauseTime;
        public DateTime? InterruptTime;
        public float Duration;
        public float ActualDuration;
        public bool IsSuccess;
        public bool HasError;
        public string ErrorMessage;
        public bool IsPaused;
        public bool IsInterrupted;
        public Dictionary<string, float> FromValues;
        public Dictionary<string, float> ToValues;
    }

    [System.Serializable]
    public class TransitionStatistics
    {
        public int TotalTransitions;
        public int SuccessfulTransitions;
        public int FailedTransitions;
        public int InterruptedTransitions;
        public float AverageTransitionDuration;
        public float TotalTransitionTime;
        public DateTime LastTransitionTime;
    }

    [System.Serializable]
    public class TransitionStatus
    {
        public bool IsTransitionInProgress;
        public QualityTransition CurrentTransition;
        public int QueuedTransitionsCount;
        public Dictionary<string, float> CurrentValues;
        public Dictionary<string, float> TargetValues;
        public TransitionStatistics Statistics;
    }

    public enum TransitionPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    public enum TransitionType
    {
        Maintenance,
        Upgrade,
        Downgrade
    }

    #endregion
}