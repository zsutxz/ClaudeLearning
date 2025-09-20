# Coin Animation System Documentation

## Overview
This system implements a cascading coin collection effect using Unity and DOTween with object pooling for optimal performance. When a coin is collected, it triggers a waterfall-like cascade effect that pulls nearby coins toward the collection point.

## Components

### 1. Coin.cs
The main component that handles individual coin animation.

**Key Features:**
- DOTween-based animation with movement, rotation, and scaling
- Curved path animation for waterfall effect
- Object pooling integration
- Automatic return to pool after animation completion

### 2. CoinPoolManager.cs
Manages the object pool of coins for efficient memory usage.

**Key Features:**
- Pre-allocation of coins
- Get/Return coin methods
- Active vs. available coin tracking

### 3. CoinAnimationSystem.cs
High-level system for spawning and controlling coin animations.

**Key Features:**
- Spawn coin animations with various parameters
- Cascade effect implementation
- Coin burst functionality
- Performance monitoring integration

### 4. CoinAnimationParams.cs
Data structure for configuring coin animations.

**Parameters:**
- Duration, start/end positions
- Rotation and scaling options
- Easing functions
- Cascade effect settings

## Cascade Effect Implementation

The cascade effect is implemented as follows:

1. When a coin is collected, it triggers the `SpawnCoinWithCascade` method
2. The initial coin animates normally toward the collection point
3. Nearby coins are identified (simulated in this implementation)
4. Each nearby coin is animated with a delay to create a ripple effect
5. Coins follow curved paths to simulate waterfall flow

## Performance Considerations

- Object pooling prevents instantiation overhead
- DOTween sequences are properly cleaned up
- Curved paths use Catmull-Rom splines for smooth animation
- Performance monitoring tracks FPS and pool status

## Usage

### Basic Coin Animation
```csharp
coinAnimationSystem.SpawnCoinAnimation(startPosition, endPosition);
```

### Coin with Cascade Effect
```csharp
coinAnimationSystem.SpawnCoinWithCascade(startPosition, endPosition);
```

### Coin Burst
```csharp
coinAnimationSystem.SpawnCoinBurst(centerPosition, coinCount, radius);
```

## Setup Instructions

1. Install DOTween package (see README.md)
2. Create a GameObject with CoinAnimationSystem component
3. Assign a coin prefab to the CoinPoolManager
4. Configure collection point and other parameters
5. Use the provided demo script or implement your own controller