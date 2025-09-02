using UnityEngine;
using DG.Tweening;

[System.Serializable]
public struct CoinAnimationParams
{
    public float duration;              // Duration of the animation
    public Vector3 startPosition;       // Starting position of the coin
    public Vector3 endPosition;         // Ending position of the coin
    public bool enableRotation;         // Whether to enable rotation effect
    public float rotationSpeed;         // Speed of rotation if enabled
    public bool enableScaling;          // Whether to enable scaling effect
    public float startScale;            // Starting scale of the coin
    public float endScale;              // Ending scale of the coin
    public Ease easeType;               // Easing function for the animation
}

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
        // Load coin sprite from Resources if not already set
        if (spriteRenderer.sprite == null)
        {
            var coinSprite = Resources.Load<Sprite>("icon02");
            if (coinSprite != null) 
                spriteRenderer.sprite = coinSprite;
            else
                Debug.LogWarning("Could not load coin sprite from Resources/icon02");
        }
    }
    
    /// <summary>
    /// Initialize coin with parameters and return callback
    /// </summary>
    /// <param name="parameters">Animation parameters</param>
    /// <param name="returnCallback">Callback to return coin to pool</param>
    public void Initialize(CoinAnimationParams parameters, Action<Coin> returnCallback)
    {
        animationParams = parameters;
        returnToPoolCallback = returnCallback;
        transform.position = parameters.startPosition;
        transform.localScale = Vector3.one * parameters.startScale;
    }
    
    /// <summary>
    /// Start the flying animation
    /// </summary>
    public void StartFlying()
    {
        // Clean up any existing animation
        if (animationSequence != null)
            animationSequence.Kill();
            
        
        // Create DOTween sequence
        animationSequence = DOTween.Sequence();
        
        // Movement tween
        animationSequence.Append(transform.DOMove(animationParams.endPosition, animationParams.duration)
            .SetEase(animationParams.easeType));
        
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
    
    /// <summary>
    /// Return coin to pool
    /// </summary>
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
}