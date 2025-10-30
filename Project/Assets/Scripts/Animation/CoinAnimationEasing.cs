using UnityEngine;
using DG.Tweening;

namespace CoinAnimation.Animation
{
    /// <summary>
    /// Custom easing functions for natural coin movement
    /// Optimized for satisfying physics-based animations
    /// </summary>
    public static class CoinAnimationEasing
    {
        /// <summary>
        /// Natural coin deceleration with gentle settle
        /// Perfect for coins coming to rest
        /// </summary>
        public static Ease CoinDecelerate => Ease.OutQuad;

        /// <summary>
        /// Quick initial burst followed by smooth coast
        /// Great for coin pickup initial movement
        /// </summary>
        public static Ease CoinBurst => Ease.OutCirc;

        /// <summary>
        /// Bouncy settle for satisfying collection feel
        /// Adds personality to coin collection
        /// </summary>
        public static Ease CoinBounce => Ease.OutBounce;

        /// <summary>
        /// Smooth magnetic attraction curve
        /// Used when coins are pulled toward collection points
        /// </summary>
        public static Ease MagneticAttraction => Ease.InOutSine;

        /// <summary>
        /// Elastic spiral entry for dramatic effect
        /// Used when coins enter magnetic field
        /// </summary>
        public static Ease SpiralEntry => Ease.OutBack;

        /// <summary>
        /// Quick snap for responsive feel
        /// Used for final collection moment
        /// </summary>
        public static Ease QuickSnap => Ease.InBack;

        /// <summary>
        /// Get custom easing curve based on distance and context
        /// Dynamic easing selection for optimal feel
        /// </summary>
        /// <param name="distance">Distance to target</param>
        /// <param name="context">Animation context (normal, magnetic, collection)</param>
        /// <returns>Appropriate easing function</returns>
        public static Ease GetContextualEasing(float distance, EasingContext context = EasingContext.Normal)
        {
            switch (context)
            {
                case EasingContext.Normal:
                    return distance < 2f ? CoinBurst : CoinDecelerate;
                    
                case EasingContext.Magnetic:
                    return distance < 1f ? SpiralEntry : MagneticAttraction;
                    
                case EasingContext.Collection:
                    return QuickSnap;
                    
                case EasingContext.Bounce:
                    return CoinBounce;
                    
                default:
                    return CoinDecelerate;
            }
        }

        /// <summary>
        /// Create custom animation curve for specific use cases
        /// </summary>
        /// <param name="curveType">Type of curve to generate</param>
        /// <returns>Animation curve instance</returns>
        public static AnimationCurve CreateCustomCurve(CustomCurveType curveType)
        {
            AnimationCurve curve = new AnimationCurve();
            
            switch (curveType)
            {
                case CustomCurveType.CoinSettle:
                    // Natural coin settling curve
                    curve.AddKey(0f, 0f);
                    curve.AddKey(0.2f, 0.8f);
                    curve.AddKey(0.5f, 1.1f);
                    curve.AddKey(0.8f, 0.98f);
                    curve.AddKey(1f, 1f);
                    break;
                    
                case CustomCurveType.MagneticPull:
                    // Increasing magnetic force curve
                    curve.AddKey(0f, 0f);
                    curve.AddKey(0.3f, 0.3f);
                    curve.AddKey(0.7f, 0.8f);
                    curve.AddKey(1f, 1f);
                    break;
                    
                case CustomCurveType.SpiralIntensity:
                    // Spiral intensity over distance
                    curve.AddKey(0f, 1f);
                    curve.AddKey(0.5f, 0.7f);
                    curve.AddKey(1f, 0.1f);
                    break;
            }
            
            return curve;
        }
    }

    /// <summary>
    /// Context for easing function selection
    /// </summary>
    public enum EasingContext
    {
        Normal,
        Magnetic,
        Collection,
        Bounce
    }

    /// <summary>
    /// Types of custom curves available
    /// </summary>
    public enum CustomCurveType
    {
        CoinSettle,
        MagneticPull,
        SpiralIntensity
    }
}