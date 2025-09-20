using UnityEngine;
using DG.Tweening;

[System.Serializable]
public struct CoinAnimationParams
{
    [Header("Basic Animation")]
    public float duration;              // Duration of the animation
    public Vector3 startPosition;       // Starting position of the coin
    public Vector3 endPosition;         // Ending position of the coin

    [Header("Rotation Effect")]
    public bool enableRotation;         // Whether to enable rotation effect
    public float rotationSpeed;         // Speed of rotation if enabled

    [Header("Scaling Effect")]
    public bool enableScaling;          // Whether to enable scaling effect
    public float startScale;            // Starting scale of the coin
    public float endScale;              // Ending scale of the coin

    [Header("Easing")]
    public Ease easeType;               // Easing function for the animation

    [Header("Cascade Effect")]
    public bool isCascadeEffect;        // Whether this is part of a cascade effect
    public float cascadeDelay;          // Delay before starting cascade effect
}