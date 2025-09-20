# Waterfall Coin Collection Effects Documentation

## Overview

This document explains how to use and configure the waterfall coin collection effects in your Unity game. The system extends the existing coin animation system with cascading effects that pull nearby coins toward the player when collecting coins in quick succession.

## Features

1. **Cascading Coin Collection**: Collecting one coin triggers a waterfall effect that pulls nearby coins toward the collection point
2. **Tiered Intensity System**: Four combo levels (Bronze, Silver, Gold, Platinum) with increasing visual effects
3. **Curved Paths**: Natural acceleration patterns that mimic waterfall physics
4. **Particle Effects**: Visual effects including motion trails and splash effects
5. **Screen Shake**: Camera shake effects that intensify with combo progression
6. **Quality Settings**: Configurable quality settings for different device capabilities

## Components

### WaterfallEffectsManager
Handles all waterfall visual and audio effects based on combo tiers.

### Coin (Enhanced)
Updated coin class with curved paths and natural acceleration patterns.

### CoinAnimationSystem (Extended)
Extended with cascade functionality to trigger waterfall effects.

### ComboManager
Existing combo system that tracks combo streaks and levels.

### WaterfallQualitySettings
Manages quality settings for different device capabilities.

### WaterfallPerformanceMonitor
Monitors performance metrics and displays them in the UI.

## Setup Instructions

1. **Add Required Components**:
   - Add `WaterfallEffectsManager` to your scene
   - Add `WaterfallQualitySettings` to your scene
   - Ensure `CoinAnimationSystem` and `ComboManager` are in your scene

2. **Configure WaterfallEffectsManager**:
   - Assign a ParticleSystem to the `waterfallParticles` field
   - Set the `mainCamera` reference
   - Adjust tier settings as needed

3. **Configure CoinAnimationSystem**:
   - Set the `cascadeRadius` to define how far the cascade effect reaches
   - Adjust `maxCascadeCoins` to limit the number of cascading coins

4. **Configure UI**:
   - Update your combo UI to show waterfall effect information
   - Add performance monitor UI if desired

## Usage

### Basic Usage
To trigger a waterfall effect, simply collect coins in quick succession. The system automatically:
1. Tracks combo streaks with the ComboManager
2. Triggers waterfall effects when combos are active
3. Applies tiered intensity based on combo level

### Programmatic Usage
You can also trigger waterfall effects programmatically:

```csharp
// Trigger a waterfall cascade from a specific point
if (CoinAnimationSystem.Instance != null)
{
    CoinAnimationSystem.Instance.TriggerWaterfallCascade(collectionPoint, targetPosition);
}

// Register a coin collection with the combo system
if (ComboManager.Instance != null)
{
    ComboManager.Instance.RegisterCoinCollection();
}
```

## Configuration

### Quality Settings
Adjust quality settings for different device capabilities:

```csharp
// Set quality level
WaterfallQualitySettings.Instance.SetQualityLevel(WaterfallQualitySettings.QualityLevel.High);

// Auto-detect quality based on device
WaterfallQualitySettings.Instance.AutoDetectQualityLevel();
```

### Tier Settings
Customize the appearance and behavior of each combo tier in the WaterfallEffectsManager:

- **Bronze**: Basic ripple effect
- **Silver**: Enhanced cascade effect
- **Gold**: Full waterfall effect
- **Platinum**: Maximum intensity torrent effect

## Performance Optimization

1. **Object Pooling**: The system uses object pooling for coins to minimize garbage collection
2. **Quality Settings**: Use lower quality settings on mobile devices
3. **Cascade Limits**: Limit the number of cascading coins with `maxCascadeCoins`
4. **Performance Monitor**: Use the WaterfallPerformanceMonitor to track performance metrics

## Troubleshooting

### No Waterfall Effects
- Ensure ComboManager is properly initialized
- Check that coins are within the cascade radius
- Verify that the CoinAnimationSystem is correctly configured

### Performance Issues
- Reduce maxCascadeCoins in CoinAnimationSystem
- Lower quality settings using WaterfallQualitySettings
- Disable particle effects if not needed

### Missing Visual Effects
- Ensure a ParticleSystem is assigned to WaterfallEffectsManager
- Check that the main camera is correctly referenced
- Verify that UI elements are properly configured

## Extending the System

You can extend the waterfall effects by:
1. Adding custom particle effects for each tier
2. Implementing additional audio effects
3. Creating custom screen shake patterns
4. Adding new combo tiers or modifying existing ones