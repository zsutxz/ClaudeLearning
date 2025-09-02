# Enhanced Coin Animation System with Object Pooling

## Overview

This document provides instructions for implementing an enhanced coin flying animation system using DOTween with object pooling in Unity. The feature makes coins (icon02.png) fly from a start position to an end position with smooth animation effects, packaged as a prefab for easy reuse.

## Prerequisites

1. Unity project with URP (Universal Render Pipeline)
2. DOTween plugin installed (see installation instructions)
3. Coin image asset (icon02.png) in the Resources folder

## New Features in Enhanced Version

1. **Prefab Implementation**: Coin animation is packaged as a reusable prefab
2. **Object Pooling**: Efficient management of coin lifecycle to prevent performance issues
3. **Enhanced API**: High-level system for easy coin spawning
4. **Multiple Animation Patterns**: Support for single coins and coin bursts

## Implementation Steps

### 1. Install DOTween

Follow the instructions in `Assets/Scripts/CoinAnimation/README.md` to install DOTween in your project.

### 2. Set Up the Coin Prefab

1. Create a new GameObject in your scene
2. Add a SpriteRenderer component
3. Attach the `Coin.cs` script
4. Configure default parameters in the Inspector
5. Drag the GameObject to the Project window to create a prefab
6. Delete the original GameObject from the scene
7. Place the prefab in `Assets/Prefabs/Coin.prefab`

### 3. Set Up the Coin Animation System

1. Create an empty GameObject in your scene and name it "CoinAnimationSystem"
2. Attach the `CoinAnimationSystem.cs` script
3. Attach the `CoinPoolManager.cs` script to a child GameObject named "CoinPoolManager"
4. Assign the Coin prefab to the CoinPoolManager's Coin Prefab field
5. Assign the CoinPoolManager to the CoinAnimationSystem's Pool Manager field

### 4. Trigger Animations

You can trigger animations in several ways:

#### Method 1: Programmatically
```csharp
public CoinAnimationSystem coinAnimationSystem;

void Start()
{
    // Start single coin animation
    coinAnimationSystem.SpawnCoinAnimation(
        new Vector3(-2, 0, 0), 
        new Vector3(2, 2, 0)
    );
    
    // Start coin burst
    coinAnimationSystem.SpawnCoinBurst(
        Vector3.zero, 
        5, 
        3.0f
    );
}
```

#### Method 2: Through UI Buttons
Create UI buttons and attach the `CoinAnimationDemoEnhanced.cs` script to a GameObject. Connect the buttons' OnClick events to the appropriate methods.

### 5. Test the Animations

1. Enter Play Mode in Unity
2. Observe coins flying from start to end positions
3. Verify that rotation and scaling effects work if enabled
4. Check that coins are properly returned to the pool after animation completion
5. Test with multiple simultaneous animations to verify performance

## Customization

You can customize the animations by:

1. **Modifying Prefab Parameters**: Adjust default values in the Coin prefab
2. **Changing Pool Size**: Modify the Initial Pool Size in CoinPoolManager
3. **Adjusting Animation Parameters**: Pass custom CoinAnimationParams to SpawnCoinAnimation
4. **Creating New Animation Patterns**: Extend CoinAnimationSystem with new methods

## Troubleshooting

### Common Issues

1. **Coin doesn't appear**: Check that icon02.png exists in the Resources folder
2. **Animation doesn't play**: Verify DOTween is properly installed and initialized
3. **Memory leaks**: Ensure animations are properly stopped and cleaned up
4. **Performance issues**: Check pool size and number of simultaneous animations
5. **Prefabs not working**: Verify all components are properly attached

### Error Messages

- "Could not load coin sprite": The icon02.png file is missing from Resources
- "CoinPoolManager is not assigned!": Assign the pool manager to the animation system
- DOTween errors: DOTween is not properly installed or initialized

## Best Practices

1. Always initialize DOTween before use
2. Properly kill tweens to prevent memory leaks
3. Use object pooling for frequently instantiated objects
4. Make animation parameters configurable in the Inspector
5. Test animations with different parameter combinations
6. Monitor pool statistics to optimize performance
7. Clean up GameObjects after animation completion
8. Use appropriate easing functions for natural motion

## Extending the Feature

You can extend this feature by:
1. Adding more complex flight paths (curves, multiple waypoints)
2. Including sound effects when coins reach their destination
3. Adding particle effects for visual enhancement
4. Creating different animation presets for various coin types
5. Implementing different pooling strategies
6. Adding animation chaining capabilities

## Files Created

- `Assets/Scripts/CoinAnimation/Coin.cs`: Individual coin component
- `Assets/Scripts/CoinAnimation/CoinPoolManager.cs`: Object pool manager
- `Assets/Scripts/CoinAnimation/CoinAnimationSystem.cs`: High-level animation system
- `Assets/Scripts/CoinAnimation/CoinAnimationDemoEnhanced.cs`: Example usage script
- `Assets/Prefabs/Coin.prefab`: Reusable coin prefab
- `Assets/Scripts/CoinAnimation/README.md`: DOTween installation instructions
- `PRPs/coin-animation/coin-animation-enhanced-prp.md`: Complete implementation specification

## Validation

To validate the implementation:
1. Verify all files are correctly placed
2. Confirm DOTween is properly installed
3. Test prefab instantiation
4. Test object pooling with multiple coins
5. Verify proper cleanup after animation completion
6. Check performance with many simultaneous animations