# UGUI Coin Prefab Documentation

## Overview
This document describes the UGUI coin prefab created for the coin animation system. The prefab is designed to work with the existing coroutine-based animation system while maintaining the zero-dependency philosophy.

## Creation Process
The UGUI coin prefab is created using the `UGUICoinPrefabCreator` editor script, which can be accessed in the Unity Editor through the menu: `Coin Animation > Create UGUI Coin Prefab`.

### Script Functionality
1. **Canvas Setup**: Creates a Canvas with Screen Space - Overlay rendering mode if one doesn't exist
2. **Coin GameObject**: Creates a new GameObject named "UGUICoin" as a child of the Canvas
3. **Image Component**: Adds a UI.Image component for visual representation
4. **Sprite Assignment**:
   - First attempts to find a coin sprite in the project
   - If none is found, creates a default gold circle sprite
5. **Animation Controller**: Adds the CoinAnimationController component
6. **RectTransform**: Configures the RectTransform with appropriate size and positioning
7. **Prefab Creation**: Saves the configured GameObject as a prefab

## Prefab Structure
```
Canvas (GameObject)
├── UGUICoin (GameObject)
│   ├── RectTransform
│   ├── Image (UI.Image)
│   │   └── sprite: CoinSprite or DefaultCoinSprite
│   └── CoinAnimationController
│       ├── animationSpeed: 1f
│       └── rotationSpeed: 360f
```

## Component Details

### Canvas
- **Render Mode**: Screen Space - Overlay
- **CanvasScaler**: Configured for scale with screen size, reference resolution 1920x1080
- **GraphicRaycaster**: Added for UI interaction

### UGUICoin GameObject
- **Name**: UGUICoin
- **Components**:
  - RectTransform
  - Image
  - CoinAnimationController

### RectTransform
- **Anchored Position**: (0, 0)
- **Size Delta**: (100, 100)

### Image
- **Sprite**: Either a coin sprite from the project or a default gold circle sprite
- **Type**: Simple
- **Preserve Aspect**: True

### CoinAnimationController
- **Animation Speed**: 1f (normal speed)
- **Rotation Speed**: 360f (full rotation per second)
- **Collection Effect**: None (can be assigned in inspector)
- **Collection Audio**: None (can be assigned in inspector)

## Usage
To use the UGUI coin prefab:

```csharp
// Instantiate the prefab
GameObject coinPrefab = Resources.Load<GameObject>("Prefabs/UI/UGUICoin");
GameObject coinInstance = Instantiate(coinPrefab, canvasTransform);

// Get the controller
CoinAnimationController controller = coinInstance.GetComponent<CoinAnimationController>();

// Animate the coin
controller.AnimateToPosition(targetPosition, duration);
```

## Design Considerations
- **Zero Dependencies**: The prefab uses only built-in Unity UI components and the existing coin animation system
- **Performance**: Optimized for 60fps performance with minimal overhead
- **Scalability**: Designed to work with 50+ concurrent coins
- **Flexibility**: Can use custom coin sprites or fall back to a default

## Testing
The prefab should be tested by:
1. Creating a new scene
2. Running the `Create UGUI Coin Prefab` menu item
3. Verifying the prefab appears in the Project window
4. Dragging the prefab into the scene
5. Testing the animation methods via script or inspector

## Notes
In a real Unity environment, the prefab would be a binary or YAML file containing the serialized data of the GameObject and its components. This documentation file serves as a reference for the prefab's structure and intended behavior.