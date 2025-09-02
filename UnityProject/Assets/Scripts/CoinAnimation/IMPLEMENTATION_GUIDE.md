# Coin Animation Feature Implementation Guide

## Overview

This document provides instructions for implementing a coin flying animation using DOTween in Unity. The feature makes a coin (icon02.png) fly from a start position to an end position with smooth animation effects.

## Prerequisites

1. Unity project with URP (Universal Render Pipeline)
2. DOTween plugin installed (see installation instructions)
3. Coin image asset (icon02.png) in the Resources folder

## Implementation Steps

### 1. Install DOTween

Follow the instructions in `Assets/Scripts/CoinAnimation/README.md` to install DOTween in your project.

### 2. Add CoinAnimationController to Scene

1. Create an empty GameObject in your scene
2. Attach the `CoinAnimationController.cs` script to it
3. Configure the animation parameters in the Inspector:
   - Duration: Time in seconds for the animation
   - Start Position: Where the coin animation begins
   - End Position: Where the coin animation ends
   - Enable Rotation: Whether the coin should rotate during animation
   - Rotation Speed: Speed of rotation if enabled
   - Enable Scaling: Whether the coin should scale during animation
   - Start Scale: Initial size of the coin
   - End Scale: Final size of the coin
   - Ease Type: Easing function for the animation

### 3. Trigger the Animation

You can trigger the animation in several ways:

#### Method 1: Programmatically
```csharp
public CoinAnimationController coinAnimationController;

void Start()
{
    // Start animation after 2 seconds
    Invoke("StartAnimation", 2f);
}

void StartAnimation()
{
    coinAnimationController.StartCoinAnimation();
}
```

#### Method 2: Through UI Button
Create a UI button and attach the `CoinAnimationDemo.cs` script to a GameObject. Connect the button's OnClick event to the `OnUIButtonClick()` method.

### 4. Test the Animation

1. Enter Play Mode in Unity
2. Observe the coin flying from start to end position
3. Verify that rotation and scaling effects work if enabled
4. Check that the coin is properly destroyed after animation completion

## Customization

You can customize the animation by modifying the parameters in the Inspector:
- Change animation duration for faster/slower movement
- Adjust start and end positions for different flight paths
- Enable/disable rotation and scaling effects
- Select different easing functions for various motion styles

## Troubleshooting

### Common Issues

1. **Coin doesn't appear**: Check that icon02.png exists in the Resources folder
2. **Animation doesn't play**: Verify DOTween is properly installed and initialized
3. **Memory leaks**: Ensure animations are properly stopped and cleaned up
4. **Performance issues**: Limit the number of simultaneous animations

### Error Messages

- "Could not load coin sprite": The icon02.png file is missing from Resources
- DOTween errors: DOTween is not properly installed or initialized

## Best Practices

1. Always initialize DOTween before use
2. Properly kill tweens to prevent memory leaks
3. Make animation parameters configurable in the Inspector
4. Test animations with different parameter combinations
5. Clean up GameObjects after animation completion
6. Use appropriate easing functions for natural motion

## Extending the Feature

You can extend this feature by:
1. Adding more complex flight paths (curves, multiple waypoints)
2. Including sound effects when the coin reaches its destination
3. Adding particle effects for visual enhancement
4. Creating different animation presets for various coin types
5. Implementing pooling for better performance with many coins

## Files Created

- `Assets/Scripts/CoinAnimation/CoinAnimationController.cs`: Main animation controller
- `Assets/Scripts/CoinAnimation/CoinAnimationDemo.cs`: Example usage script
- `Assets/Scripts/CoinAnimation/README.md`: DOTween installation instructions
- `PRPs/coin-animation/coin-animation-prp.md`: Complete implementation specification

## Validation

To validate the implementation:
1. Verify all files are correctly placed
2. Confirm DOTween is properly installed
3. Test animation in Play Mode
4. Check Inspector parameters affect the animation
5. Verify proper cleanup after animation completion