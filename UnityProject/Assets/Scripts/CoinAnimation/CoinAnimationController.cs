using UnityEngine;
using DG.Tweening;

public class CoinAnimationController : MonoBehaviour
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
    
    private GameObject coinInstance;
    private Sequence animationSequence;
    
    void Awake()
    {
        // Initialize DOTween with safe mode
        DOTween.Init(true, true, LogBehaviour.ErrorsOnly);
        DOTween.defaultEaseType = Ease.OutQuad;
    }
    
    /// <summary>
    /// Starts the coin flying animation
    /// </summary>
    public void StartCoinAnimation()
    {
        // Clean up any existing animation
        if (animationSequence != null)
            animationSequence.Kill();
            
        // Create coin instance at start position
        coinInstance = new GameObject("FlyingCoin");
        var renderer = coinInstance.AddComponent<SpriteRenderer>();
        
        // Load coin sprite from Resources
        var coinSprite = Resources.Load<Sprite>("icon02");
        if (coinSprite != null)
        {
            renderer.sprite = coinSprite;
        }
        else
        {
            Debug.LogError("Could not load coin sprite from Resources/icon02");
        }
        
        coinInstance.transform.position = startPosition;
        coinInstance.transform.localScale = Vector3.one * startScale;
        
        // Create DOTween sequence
        animationSequence = DOTween.Sequence();
        
        // Movement tween
        animationSequence.Append(coinInstance.transform.DOMove(endPosition, duration).SetEase(easeType));
        
        // Rotation tween if enabled
        if (enableRotation)
        {
            animationSequence.Join(coinInstance.transform.DORotate(
                new Vector3(0, 0, 360), duration/RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental));
        }
        
        // Scaling tween if enabled
        if (enableScaling)
        {
            animationSequence.Join(coinInstance.transform.DOScale(endScale, duration).SetEase(easeType));
        }
        
        // Cleanup when animation completes
        animationSequence.OnComplete(() => {
            if (coinInstance != null)
                Destroy(coinInstance);
        });
    }
    
    /// <summary>
    /// Stops the current coin animation
    /// </summary>
    public void StopCoinAnimation()
    {
        if (animationSequence != null)
        {
            animationSequence.Kill();
            animationSequence = null;
        }
        
        if (coinInstance != null)
        {
            Destroy(coinInstance);
            coinInstance = null;
        }
    }
    
    void OnDestroy()
    {
        StopCoinAnimation();
    }
}