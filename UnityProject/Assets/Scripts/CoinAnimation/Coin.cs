using UnityEngine;
using DG.Tweening;
using System;

public class Coin : MonoBehaviour
{
    [Header("Animation Parameters")]
    public CoinAnimationParams animationParams;

    private Sequence animationSequence;
    private Action<Coin> returnToPoolCallback;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Load coin sprite from Assets if not already set
        if (spriteRenderer.sprite == null)
        {
            var coinSprite = Resources.Load<Sprite>("icon02");
            if (coinSprite == null)
            {
                // Try to load from Assets folder directly
                coinSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/icon02.png");
            }
            if (coinSprite != null) spriteRenderer.sprite = coinSprite;
        }
    }

    // Initialize coin with parameters and return callback
    public void Initialize(CoinAnimationParams parameters, Action<Coin> returnCallback)
    {
        animationParams = parameters;
        returnToPoolCallback = returnCallback;
        transform.position = parameters.startPosition;
        transform.localScale = Vector3.one * parameters.startScale;
    }

    // Start the flying animation
    public void StartFlying()
    {
        // Clean up any existing animation
        if (animationSequence != null)
            animationSequence.Kill();

        // Create DOTween sequence
        animationSequence = DOTween.Sequence();

        // If this is a cascade effect, add delay
        if (animationParams.isCascadeEffect && animationParams.cascadeDelay > 0)
        {
            animationSequence.AppendInterval(animationParams.cascadeDelay);
        }

        // Movement tween with curved path for waterfall effect
        if (animationParams.isCascadeEffect)
        {
            // Create a curved path for waterfall effect
            Vector3[] path = CreateWaterfallPath(animationParams.startPosition, animationParams.endPosition);
            animationSequence.Append(transform.DOPath(path, animationParams.duration, PathType.CatmullRom)
                .SetEase(animationParams.easeType));
        }
        else
        {
            // Straight path for regular collection
            animationSequence.Append(transform.DOMove(animationParams.endPosition, animationParams.duration)
                .SetEase(animationParams.easeType));
        }

        // Rotation tween if enabled
        if (animationParams.enableRotation)
        {
            animationSequence.Join(transform.DORotate(
                new Vector3(0, 0, 360), animationParams.duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental));
        }

        // Scaling tween if enabled
        if (animationParams.enableScaling)
        {
            animationSequence.Join(transform.DOScale(animationParams.endScale, animationParams.duration)
                .SetEase(animationParams.easeType));
        }

        // Return to pool when animation completes
        animationSequence.OnComplete(() => {
            ReturnToPool();
        });
    }

    // Create a curved path for waterfall effect
    private Vector3[] CreateWaterfallPath(Vector3 start, Vector3 end)
    {
        // Create a curved path that mimics waterfall flow
        Vector3 controlPoint1 = start + new Vector3((end.x - start.x) * 0.25f, Mathf.Abs(end.y - start.y) * 0.5f, 0);
        Vector3 controlPoint2 = start + new Vector3((end.x - start.x) * 0.75f, Mathf.Abs(end.y - start.y) * 0.3f, 0);

        return new Vector3[] { start, controlPoint1, controlPoint2, end };
    }

    // Return coin to pool
    public void ReturnToPool()
    {
        if (animationSequence != null)
        {
            animationSequence.Kill();
            animationSequence = null;
        }

        // Deactivate and return to pool
        gameObject.SetActive(false);
        returnToPoolCallback?.Invoke(this);
    }

    void OnDisable()
    {
        // Clean up any running tweens
        if (animationSequence != null)
        {
            animationSequence.Kill();
            animationSequence = null;
        }
    }

    // Trigger cascade effect on nearby coins
    public void TriggerCascadeEffect(Vector3 collectionPoint, CoinPoolManager poolManager)
    {
        // This method would be called when a coin is collected to trigger the cascade effect
        // In a full implementation, this would find nearby coins and trigger their animations
        // with delays to create the ripple effect
    }
}