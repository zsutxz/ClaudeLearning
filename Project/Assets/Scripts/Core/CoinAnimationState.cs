using System;

namespace CoinAnimation.Core
{
    /// <summary>
    /// Animation states for coin behavior management
    /// Optimized for 60fps performance with state machine pattern
    /// </summary>
    public enum CoinAnimationState
    {
        /// <summary>
        /// Coin is idle and waiting for activation
        /// </summary>
        Idle = 0,
        
        /// <summary>
        /// Coin is moving toward target with physics-based animation
        /// </summary>
        Moving = 1,
        
        /// <summary>
        /// Coin is being collected with spiral motion effects
        /// </summary>
        Collecting = 2,
        
        /// <summary>
        /// Coin animation is paused (for debugging or special effects)
        /// </summary>
        Paused = 3,
        
        /// <summary>
        /// Coin is disabled and pooled for performance
        /// </summary>
        Pooled = 4
    }

    /// <summary>
    /// Animation completion event arguments
    /// Provides context for animation state transitions
    /// </summary>
    public class CoinAnimationEventArgs : EventArgs
    {
        public CoinAnimationState PreviousState { get; }
        public CoinAnimationState CurrentState { get; }
        public float AnimationDuration { get; }
        public bool WasSuccessful { get; }

        public CoinAnimationEventArgs(
            CoinAnimationState previousState, 
            CoinAnimationState currentState, 
            float duration, 
            bool successful)
        {
            PreviousState = previousState;
            CurrentState = currentState;
            AnimationDuration = duration;
            WasSuccessful = successful;
        }
    }
}