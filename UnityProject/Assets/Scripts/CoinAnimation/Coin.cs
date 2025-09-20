using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class Coin : MonoBehaviour
{
    [Header("Animation Parameters")]
    public float duration = 1.0f;
    public Vector3 startPosition = Vector3.zero;
    public Vector3 endPosition = new Vector3(0, 2, 0);
    public bool enableRotation = true;
    public float rotationSpeed = 360f;
    public bool enableScaling = true;
    public float startScale = 1.0f;
    public float endScale = 0.5f;
    public Ease easeType = Ease.OutQuad;
    
    [Header("Waterfall Effects Parameters")]
    public bool useCurvedPath = false;
    public float curveHeight = 1.0f;
    public bool useNaturalAcceleration = false;
    public Ease naturalAccelerationCurve = Ease.InOutQuad;
    
    private Sequence animationSequence;
    private System.Action<Coin> onCompletionCallback;
    private bool isAnimating = false;
    
    void Awake()
    {
        // Initialize DOTween settings
        DOTween.Init(true, true, LogBehaviour.ErrorsOnly);
        DOTween.defaultEaseType = Ease.OutQuad;
    }
    
    /// <summary>
    /// Initializes the coin with start and end positions
    /// </summary>
    /// <param name="start">Start position</param>
    /// <param name="end">End position</param>
    public void Initialize(Vector3 start, Vector3 end)
    {
        startPosition = start;
        endPosition = end;
        transform.position = startPosition;
        transform.localScale = Vector3.one * startScale;
    }
    
    /// <summary>
    /// Starts the coin flying animation
    /// </summary>
    /// <param name="onComplete">Callback when animation completes</param>
    public void StartFlying(System.Action<Coin> onComplete = null)
    {
        if (isAnimating)
        {
            StopFlying();
        }
        
        onCompletionCallback = onComplete;
        isAnimating = true;
        
        // Reset position and scale
        transform.position = startPosition;
        transform.localScale = Vector3.one * startScale;
        
        // Create DOTween sequence
        animationSequence = DOTween.Sequence();
        
        // Movement tween with curved path if enabled
        if (useCurvedPath)
        {
            // Create a curved path using a control point
            Vector3 controlPoint = Vector3.Lerp(startPosition, endPosition, 0.5f);
            controlPoint.y += curveHeight; // Add height to create curve
            
            // Create the curved path
            Vector3[] path = {
                startPosition,
                controlPoint,
                endPosition
            };
            
            // Apply movement with curved path
            if (useNaturalAcceleration)
            {
                animationSequence.Append(transform.DOPath(path, duration, PathType.CatmullRom).SetEase(naturalAccelerationCurve));
            }
            else
            {
                animationSequence.Append(transform.DOPath(path, duration, PathType.CatmullRom).SetEase(easeType));
            }
        }
        else
        {
            // Straight path movement
            if (useNaturalAcceleration)
            {
                animationSequence.Append(transform.DOMove(endPosition, duration).SetEase(naturalAccelerationCurve));
            }
            else
            {
                animationSequence.Append(transform.DOMove(endPosition, duration).SetEase(easeType));
            }
        }
        
        // Rotation tween if enabled
        if (enableRotation)
        {
            animationSequence.Join(transform.DORotate(
                new Vector3(0, 0, 360), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental));
        }
        
        // Scaling tween if enabled
        if (enableScaling)
        {
            animationSequence.Join(transform.DOScale(endScale, duration).SetEase(easeType));
        }
        
        // Cleanup when animation completes
        animationSequence.OnComplete(() => {
            isAnimating = false;
            onCompletionCallback?.Invoke(this);
        });
    }
    
    /// <summary>
    /// Stops the current coin animation
    /// </summary>
    public void StopFlying()
    {
        if (animationSequence != null)
        {
            animationSequence.Kill();
            animationSequence = null;
        }
        
        isAnimating = false;
    }
    
    /// <summary>
    /// Starts a waterfall-style coin animation with curved path and natural acceleration
    /// </summary>
    /// <param name="onComplete">Callback when animation completes</param>
    /// <param name="curveHeight">Height of the curve for this animation</param>
    /// <param name="delay">Delay before animation starts</param>
    public void StartWaterfallFlying(System.Action<Coin> onComplete = null, float curveHeight = 1.0f, float delay = 0f)
    {
        // Set waterfall-specific parameters
        useCurvedPath = true;
        this.curveHeight = curveHeight;
        useNaturalAcceleration = true;
        naturalAccelerationCurve = Ease.InOutQuad;
        
        // If there's a delay, start the animation after the delay
        if (delay > 0)
        {
            DOVirtual.DelayedCall(delay, () => {
                StartFlying(onComplete);
            });
        }
        else
        {
            StartFlying(onComplete);
        }
    }
    
    /// <summary>
    /// Resets the coin to its initial state
    /// </summary>
    public void ResetCoin()
    {
        StopFlying();
        transform.position = startPosition;
        transform.localScale = Vector3.one * startScale;
        transform.rotation = Quaternion.identity;
        gameObject.SetActive(false);
    }
    
    void OnDisable()
    {
        StopFlying();
    }
    
    void OnDestroy()
    {
        StopFlying();
    }
}