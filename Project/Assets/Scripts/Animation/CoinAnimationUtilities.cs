using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace CoinAnimation.Animation
{
    /// <summary>
    /// Utility functions for coin animations
    /// Provides reusable animation patterns for coin behaviors
    /// </summary>
    public static class CoinAnimationUtilities
    {
        private static readonly Dictionary<Transform, Sequence> activeSequences = new Dictionary<Transform, Sequence>();
        
        /// <summary>
        /// Create coin spin animation
        /// </summary>
        /// <param name="coin">Coin transform</param>
        /// <param name="duration">Animation duration</param>
        /// <param name="loops">Number of loops (-1 for infinite)</param>
        /// <returns>Animation tween</returns>
        public static Tween CreateCoinSpin(Transform coin, float duration = 2f, int loops = -1)
        {
            return coin.DOLocalRotate(Vector3.up * 360f, duration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(loops, LoopType.Incremental);
        }
        
        /// <summary>
        /// Create coin bounce animation
        /// </summary>
        /// <param name="coin">Coin transform</param>
        /// <param name="height">Bounce height</param>
        /// <param name="duration">Animation duration</param>
        /// <returns>Animation sequence</returns>
        public static Sequence CreateCoinBounce(Transform coin, float height = 1f, float duration = 1f)
        {
            var sequence = DOTween.Sequence();
            
            // Bounce up
            sequence.Append(coin.DOLocalMoveY(height, duration * 0.4f)
                .SetEase(Ease.OutQuad));
            
            // Bounce down
            sequence.Append(coin.DOLocalMoveY(0, duration * 0.4f)
                .SetEase(Ease.InQuad));
            
            // Small second bounce
            sequence.Append(coin.DOLocalMoveY(height * 0.3f, duration * 0.1f)
                .SetEase(Ease.OutQuad));
            sequence.Append(coin.DOLocalMoveY(0, duration * 0.1f)
                .SetEase(Ease.InQuad));
            
            return sequence;
        }
        
        /// <summary>
        /// Create coin wobble animation
        /// </summary>
        /// <param name="coin">Coin transform</param>
        /// <param name="intensity">Wobble intensity</param>
        /// <param name="duration">Animation duration</param>
        /// <returns>Animation tween</returns>
        public static Tween CreateCoinWobble(Transform coin, float intensity = 15f, float duration = 0.5f)
        {
            return coin.DOLocalRotate(Vector3.forward * intensity, duration * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(2, LoopType.Yoyo);
        }
        
        /// <summary>
        /// Create coin flip animation
        /// </summary>
        /// <param name="coin">Coin transform</param>
        /// <param name="duration">Animation duration</param>
        /// <param name="onComplete">Completion callback</param>
        /// <returns>Animation sequence</returns>
        public static Sequence CreateCoinFlip(Transform coin, float duration = 1f, Action onComplete = null)
        {
            var sequence = DOTween.Sequence();
            
            // Flip forward
            sequence.Append(coin.DOLocalRotate(Vector3.right * 180f, duration * 0.5f)
                .SetEase(Ease.InOutSine));
            
            // Flip back
            sequence.Append(coin.DOLocalRotate(Vector3.right * 360f, duration * 0.5f)
                .SetEase(Ease.InOutSine));
            
            // Ensure correct final rotation
            sequence.Append(coin.DOLocalRotate(Vector3.zero, 0.1f)
                .SetEase(Ease.Linear));
            
            if (onComplete != null)
            {
                sequence.OnComplete(onComplete);
            }
            
            return sequence;
        }
        
        /// <summary>
        /// Create coin glow/pulse animation
        /// </summary>
        /// <param name="coinRenderer">Coin mesh renderer</param>
        /// <param name="glowIntensity">Maximum glow intensity</param>
        /// <param name="duration">Animation duration</param>
        /// <returns>Animation tween</returns>
        public static Tween CreateCoinGlow(Renderer coinRenderer, float glowIntensity = 2f, float duration = 1f)
        {
            var material = coinRenderer.material;
            var originalEmission = material.GetColor("_EmissionColor");
            
            // Create glow animation
            return ShortcutExtensions.To<Color>(() => material.GetColor("_EmissionColor"),
                color => material.SetColor("_EmissionColor", color),
                originalEmission * glowIntensity, duration * 0.5f)
                .SetEase(Ease.OutQuad)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => material.SetColor("_EmissionColor", originalEmission));
        }
        
        /// <summary>
        /// Create coin trail effect animation
        /// </summary>
        /// <param name="coin">Coin transform</param>
        /// <param name="target">Target position</param>
        /// <param name="trailPositions">Number of trail positions</param>
        /// <param name="duration">Animation duration</param>
        /// <returns>Animation sequence</returns>
        public static Sequence CreateCoinTrail(Transform coin, Vector3 target, int trailPositions = 5, float duration = 1f)
        {
            var sequence = DOTween.Sequence();
            var trail = new List<Transform>();
            
            // Create trail objects
            for (int i = 0; i < trailPositions; i++)
            {
                var trailObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                trailObj.transform.localScale = Vector3.one * 0.1f;
                trailObj.transform.position = coin.position;
                
                var trailRenderer = trailObj.GetComponent<Renderer>();
                trailRenderer.material = new Material(Shader.Find("Standard"));
                trailRenderer.material.color = new Color(1f, 0.8f, 0f, 0.5f);
                
                trail.Add(trailObj.transform);
            }
            
            // Animate trail following the coin
            for (int i = 0; i < trailPositions; i++)
            {
                var delay = (float)i / trailPositions * duration * 0.8f;
                var trailObj = trail[i];
                
                sequence.Insert(delay, trailObj.DOMove(target, duration * 0.8f)
                    .SetEase(Ease.InQuad));
                
                sequence.Insert(delay, trailObj.DOScale(Vector3.zero, duration * 0.8f)
                    .SetEase(Ease.InQuad));
                
                sequence.Insert(delay + duration * 0.8f, ShortcutExtensions.To<Color>(() => trailObj.GetComponent<Renderer>().material.color,
                    color => trailObj.GetComponent<Renderer>().material.color = color,
                    new Color(1f, 0.8f, 0f, 0f), 0.2f));
            }
            
            // Clean up trail objects
            sequence.OnComplete(() => {
                foreach (var trailObj in trail)
                {
                    if (trailObj != null)
                    {
                        UnityEngine.Object.DestroyImmediate(trailObj.gameObject);
                    }
                }
            });
            
            return sequence;
        }
        
        /// <summary>
        /// Stop all animations for a specific coin
        /// </summary>
        /// <param name="coin">Coin transform</param>
        /// <param name="complete">Whether to complete animations</param>
        public static void StopCoinAnimations(Transform coin, bool complete = false)
        {
            coin.DOKill(complete);
            
            if (activeSequences.TryGetValue(coin, out var sequence))
            {
                if (complete)
                {
                    sequence.Complete();
                }
                else
                {
                    sequence.Kill();
                }
                activeSequences.Remove(coin);
            }
        }
        
        /// <summary>
        /// Check if coin has active animations
        /// </summary>
        /// <param name="coin">Coin transform</param>
        /// <returns>True if animations are active</returns>
        public static bool HasActiveAnimations(Transform coin)
        {
            return coin != null && (DOTween.IsTweening(coin) || 
                   (activeSequences.ContainsKey(coin) && activeSequences[coin].IsActive()));
        }
        
        /// <summary>
        /// Set animation time scale for slow-motion effects
        /// </summary>
        /// <param name="timeScale">Time scale multiplier</param>
        public static void SetAnimationTimeScale(float timeScale)
        {
            DOTween.timeScale = timeScale;
        }
        
        /// <summary>
        /// Reset animation time scale to normal
        /// </summary>
        public static void ResetAnimationTimeScale()
        {
            DOTween.timeScale = 1f;
        }
        
        /// <summary>
        /// Get active animation count for a coin
        /// </summary>
        /// <param name="coin">Coin transform</param>
        /// <returns>Number of active animations</returns>
        public static int GetActiveAnimationCount(Transform coin)
        {
            int count = 0;
            if (DOTween.IsTweening(coin))
                count++;
            if (activeSequences.ContainsKey(coin) && activeSequences[coin].IsActive())
                count++;
            return count;
        }
        
        /// <summary>
        /// Pause all coin animations
        /// </summary>
        public static void PauseAllAnimations()
        {
            DOTween.PauseAll();
        }
        
        /// <summary>
        /// Resume all coin animations
        /// </summary>
        public static void ResumeAllAnimations()
        {
            DOTween.PlayAll();
        }
        
        /// <summary>
        /// Create delayed animation
        /// </summary>
        /// <param name="delay">Delay in seconds</param>
        /// <param name="callback">Action to execute after delay</param>
        /// <returns>Animation tween</returns>
        public static Tween CreateDelayedAnimation(float delay, Action callback)
        {
            return DOTween.To(() => 0f, x => { }, 1f, delay)
                .OnComplete(callback);
        }
        
        /// <summary>
        /// Shake coin animation
        /// </summary>
        /// <param name="coin">Coin transform</param>
        /// <param name="strength">Shake strength</param>
        /// <param name="duration">Animation duration</param>
        /// <returns>Animation tween</returns>
        public static Tween CreateCoinShake(Transform coin, float strength = 0.1f, float duration = 0.5f)
        {
            return coin.DOShakePosition(duration, new Vector3(strength, strength, 0), 20, 90f, false, true)
                .SetEase(Ease.OutQuad);
        }
    }
}