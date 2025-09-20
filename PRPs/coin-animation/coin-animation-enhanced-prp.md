## Goal
Create an enhanced Unity coin flying animation system using DOTween with object pooling that makes the coin icon02.png fly from a start position to an end position with smooth tweening effects, packaged as a prefab for easy reuse.

## Why
- Enhance user experience with visually appealing coin collection animations in games
- Provide reusable prefab components that can be easily instantiated
- Implement object pooling to efficiently manage multiple coin animations without performance degradation
- Demonstrate advanced DOTween implementation with pooling patterns in Unity
- Create scalable animation system for handling many simultaneous coin effects

## What
- Unity prefab for coin flying animation using DOTween
- Object pooling system to manage coin lifecycle efficiently
- Smooth flying animation with movement, rotation, and scaling effects
- Configurable parameters for animation duration, path, and effects
- URP (Universal Render Pipeline) compatible implementation

### Success Criteria
- [ ] Coin flying animation works as a prefab that can be instantiated
- [ ] Object pooling system manages coin lifecycle efficiently
- [ ] Animation includes movement, rotation, and scaling effects
- [ ] Parameters are configurable in the Unity Inspector
- [ ] System can handle multiple simultaneous coin animations
- [ ] No memory leaks or performance degradation with many coins
- [ ] Code follows Unity best practices and DOTween conventions
- [ ] Implementation works with URP

## All Needed Context

### Documentation & References
```yaml
# MUST READ - Include these in your context window
- url: https://dotween.demigiant.com/documentation.php
  why: Official DOTween documentation for tweening methods and parameters

- url: https://docs.unity3d.com/Manual/URP.html
  why: Understanding URP integration and rendering pipeline

- url: https://docs.unity3d.com/Manual/ObjectPooling.html
  why: Unity's recommended approach to object pooling

- file: D:\work\AI\ClaudeTest\UnityProject\Assets\Resources\icon02.png
  why: The coin image asset to be animated
```

### Current Codebase tree
```bash
.
├── UnityProject/
│   ├── Assets/
│   │   ├── Resources/
│   │   │   └── icon02.png          # Coin image asset
│   │   ├── Scenes/
│   │   │   └── EnhancedRayTracingScene.unity
│   │   ├── Scripts/
│   │   │   ├── CoinAnimation/      # Directory for coin animation scripts
│   │   │   │   ├── CoinAnimationController.cs
│   │   │   │   ├── CoinAnimationDemo.cs
│   │   │   │   ├── CoinPoolManager.cs (to be created)
│   │   │   │   └── Coin.prefab (to be created)
│   │   │   ├── PerformanceMonitor.cs
│   │   │   └── RayTracingController.cs
│   │   ├── Materials/
│   │   ├── Shaders/
│   │   └── ...
│   └── Packages/
│       └── manifest.json           # Project dependencies (DOTween needs to be added)
```

### Desired Codebase tree with files to be added and responsibility of file
```bash
UnityProject/
  - Assets/Scripts/CoinAnimation/Coin.cs:
    - Coin component that handles individual coin animation
    - DOTween sequence implementation for flying effect
    - Self-deactivation when animation completes

  - Assets/Scripts/CoinAnimation/CoinPoolManager.cs:
    - Object pool manager for coin prefabs
    - Methods to get/return coins from pool
    - Pool initialization and management

  - Assets/Scripts/CoinAnimation/CoinAnimationSystem.cs:
    - High-level system to spawn and control coin animations
    - Integration point for other systems to trigger animations

  - Assets/Prefabs/Coin.prefab:
    - Prefab containing coin sprite and animation components
    - Ready to be instantiated in scenes

PRPs/coin-animation/coin-animation-enhanced-prp.md:
  - Complete PRP specification for enhanced coin animation implementation
  - Implementation blueprint with object pooling
  - Validation gates and best practices
```

### Known Gotchas of our codebase & Library Quirks
```csharp
// CRITICAL: DOTween requires importing via Package Manager or manual installation
// CRITICAL: DOTween sequences need to be properly killed/destroyed to prevent memory leaks
// CRITICAL: Object pooling requires careful management of object states
// CRITICAL: URP materials might require specific shader settings for proper rendering
// CRITICAL: Animation might not work in edit mode without proper setup
// CRITICAL: DOTween needs to be initialized before use (DOTween.Init())
// CRITICAL: Use safe mode for DOTween to prevent issues with destroyed objects
// CRITICAL: Prefabs need to maintain references to components properly
```

## Implementation Blueprint

### Data models and structure
```csharp
// Coin animation configuration parameters
[System.Serializable]
public struct CoinAnimationParams
{
    public float duration;              // Duration of the animation
    public Vector3 startPosition;       // Starting position of the coin
    public Vector3 endPosition;         // Ending position of the coin
    public bool enableRotation;         // Whether to enable rotation effect
    public float rotationSpeed;         // Speed of rotation if enabled
    public bool enableScaling;          // Whether to enable scaling effect
    public float startScale;            // Starting scale of the coin
    public float endScale;              // Ending scale of the coin
    public Ease easeType;               // Easing function for the animation
};

// Individual coin component
public class Coin : MonoBehaviour
{
    public CoinAnimationParams animationParams;  // Animation parameters
    private Sequence animationSequence;          // DOTween animation sequence
    private Action<Coin> returnToPoolCallback;   // Callback to return coin to pool
};

// Object pool manager for coins
public class CoinPoolManager : MonoBehaviour
{
    public GameObject coinPrefab;                // Prefab to pool
    public int initialPoolSize = 10;            // Initial number of coins to create
    private Queue<Coin> coinPool;               // Pool of available coins
    private List<Coin> activeCoins;             // Currently active coins
};

// High-level animation system
public class CoinAnimationSystem : MonoBehaviour
{
    public CoinPoolManager poolManager;         // Reference to pool manager
    public void SpawnCoinAnimation(CoinAnimationParams parameters);  // Spawn coin animation
};
```

### list of tasks to be completed to fulfill the PRP in the order they should be completed
```yaml
Task 1:
SETUP DOTween in Unity project:
  - ADD DOTween package to project (manual installation)
  - VERIFY DOTween installation and initialization
  - CONFIGURE DOTween settings for safe mode

Task 2:
CREATE Coin component:
  - MODIFY existing CoinAnimationController to be Coin component
  - IMPLEMENT DOTween sequence for flying animation
  - ADD self-deactivation when animation completes
  - BIND parameters to Unity Inspector

Task 3:
CREATE coin prefab:
  - CREATE prefab from coin GameObject
  - CONFIGURE components and references
  - VERIFY prefab works when instantiated

Task 4:
IMPLEMENT object pool manager:
  - CREATE CoinPoolManager script
  - IMPLEMENT pool initialization with initial coins
  - ADD methods to get/return coins from pool
  - TRACK active coins to prevent leaks

Task 5:
CREATE coin animation system:
  - DEVELOP CoinAnimationSystem for high-level control
  - IMPLEMENT methods to spawn coin animations
  - IMPLEMENT coin burst functionality for multiple coins
  - CONNECT with pool manager for coin retrieval

Task 6:
IMPLEMENT flying animation with pooling:
  - MODIFY animation sequence to work with pooled coins
  - ENSURE coins return to pool after animation
  - HANDLE edge cases and error conditions

Task 7:
TEST and VALIDATE implementation:
  - VERIFY prefab instantiation works correctly
  - TEST object pooling with multiple coins
  - VALIDATE memory management and performance
  - CONFIRM URP compatibility
```

### Per task pseudocode as needed added to each task
```csharp
// Task 1: Setup DOTween in Unity project
// Manual installation required - see README.md for instructions
// In script initialization:
void Awake()
{
    // Initialize DOTween with safe mode
    DOTween.Init(true, true, LogBehaviour.ErrorsOnly);
    DOTween.defaultEaseType = Ease.OutQuad;
}

// Task 2: Create Coin component
using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
    [Header("Animation Parameters")]
    public CoinAnimationParams animationParams;
    
    private Sequence animationSequence;
    private Action<Coin> returnToPoolCallback;
    private SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Load coin sprite from Resources if not already set
        if (spriteRenderer.sprite == null)
        {
            var coinSprite = Resources.Load<Sprite>("icon02");
            if (coinSprite != null) spriteRenderer.sprite = coinSprite;
        }
    }
    
    // Initialize coin with parameters and return callback
    public void Initialize(CoinAnimationParams parameters, Action<Coin> returnCallback)
    {
        animationParams = parameters;
        returnToPoolCallback = returnCallback;
        transform.position = parameters.startPosition;
        transform.localScale = Vector3.one * parameters.startScale;
    }
    
    // Start the flying animation
    public void StartFlying()
    {
        // Clean up any existing animation
        if (animationSequence != null)
            animationSequence.Kill();
            
        // Create DOTween sequence
        animationSequence = DOTween.Sequence();
        
        // Movement tween
        animationSequence.Append(transform.DOMove(animationParams.endPosition, animationParams.duration)
            .SetEase(animationParams.easeType));
        
        // Rotation tween if enabled
        if (animationParams.enableRotation)
        {
            animationSequence.Join(transform.DORotate(
                new Vector3(0, 0, 360), animationParams.duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental));
        }
        
        // Scaling tween if enabled
        if (animationParams.enableScaling)
        {
            animationSequence.Join(transform.DOScale(animationParams.endScale, animationParams.duration)
                .SetEase(animationParams.easeType));
        }
        
        // Return to pool when animation completes
        animationSequence.OnComplete(() => {
            ReturnToPool();
        });
    }
    
    // Return coin to pool
    public void ReturnToPool()
    {
        if (animationSequence != null)
        {
            animationSequence.Kill();
            animationSequence = null;
        }
        
        // Deactivate and return to pool
        gameObject.SetActive(false);
        returnToPoolCallback?.Invoke(this);
    }
    
    void OnDisable()
    {
        // Clean up any running tweens
        if (animationSequence != null)
        {
            animationSequence.Kill();
            animationSequence = null;
        }
    }
}

// Task 3: Create coin prefab
// In Unity Editor:
// 1. Create GameObject with SpriteRenderer component
// 2. Attach Coin.cs script
// 3. Configure default parameters in Inspector
// 4. Drag to Project window to create prefab
// 5. Delete original GameObject from scene

// Task 4: Implement object pool manager
public class CoinPoolManager : MonoBehaviour
{
    [Header("Pool Settings")]
    public GameObject coinPrefab;
    public int initialPoolSize = 10;
    
    private Queue<Coin> coinPool = new Queue<Coin>();
    private List<Coin> activeCoins = new List<Coin>();
    
    void Awake()
    {
        InitializePool();
    }
    
    // Initialize pool with coins
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewCoin();
        }
    }
    
    // Create a new coin and add to pool
    private Coin CreateNewCoin()
    {
        GameObject coinObj = Instantiate(coinPrefab, transform);
        Coin coin = coinObj.GetComponent<Coin>();
        coinObj.SetActive(false);
        coinPool.Enqueue(coin);
        return coin;
    }
    
    // Get a coin from the pool
    public Coin GetCoin()
    {
        Coin coin;
        if (coinPool.Count > 0)
        {
            coin = coinPool.Dequeue();
        }
        else
        {
            // Create new coin if pool is empty
            coin = CreateNewCoin();
        }
        
        coin.gameObject.SetActive(true);
        activeCoins.Add(coin);
        return coin;
    }
    
    // Return coin to pool
    public void ReturnCoin(Coin coin)
    {
        if (activeCoins.Contains(coin))
        {
            activeCoins.Remove(coin);
            coinPool.Enqueue(coin);
        }
    }
}

// Task 5: Create coin animation system
public class CoinAnimationSystem : MonoBehaviour
{
    public CoinPoolManager poolManager;
    
    // Spawn a coin animation with parameters
    public void SpawnCoinAnimation(CoinAnimationParams parameters)
    {
        if (poolManager == null) return;
        
        Coin coin = poolManager.GetCoin();
        coin.Initialize(parameters, poolManager.ReturnCoin);
        coin.StartFlying();
    }
    
    // Convenience method with positions
    public void SpawnCoinAnimation(Vector3 start, Vector3 end)
    {
        CoinAnimationParams parameters = new CoinAnimationParams
        {
            duration = 1.0f,
            startPosition = start,
            endPosition = end,
            enableRotation = true,
            rotationSpeed = 360f,
            enableScaling = true,
            startScale = 1.0f,
            endScale = 0.5f,
            easeType = Ease.OutQuad
        };
        
        SpawnCoinAnimation(parameters);
    }
    
    // Spawn a burst of coins from a center position
    public void SpawnCoinBurst(Vector3 centerPosition, int count, float radius = 2.0f, System.Action onComplete = null)
    {
        if (poolManager == null) return;
        
        int completedCount = 0;
        
        for (int i = 0; i < count; i++)
        {
            // Calculate random start position within the radius
            Vector3 randomOffset = Random.insideUnitCircle * radius;
            Vector3 startPosition = centerPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
            
            // Calculate random end position within a smaller radius around the center
            Vector3 endOffset = Random.insideUnitCircle * (radius * 0.5f);
            Vector3 endPosition = centerPosition + new Vector3(endOffset.x, endOffset.y, 0);
            
            // Add some upward movement to make it more natural
            endPosition.y += Random.Range(0.5f, 1.5f);
            
            SpawnCoinAnimation(startPosition, endPosition, () => {
                completedCount++;
                if (completedCount >= count)
                {
                    onComplete?.Invoke();
                }
            });
        }
    }
}

// Task 6: Implement flying animation with pooling
// Already implemented in Coin.cs above with:
// - Initialize method to set up coin with parameters
// - StartFlying method to begin animation
// - ReturnToPool method to return coin to pool
// - Proper cleanup in OnDisable
```

### Integration Points
```yaml
DOTWEEN_INTEGRATION:
  - initialization: "DOTween.Init() in script Awake method"
  - sequences: "DOTween sequences for complex animations"
  - cleanup: "Proper disposal of tweens to prevent memory leaks"
  
OBJECT_POOLING:
  - pool_manager: "CoinPoolManager handles coin lifecycle"
  - prefab: "Coin prefab contains all necessary components"
  - callbacks: "Return to pool callback system"
  
UNITY_URP:
  - rendering: "SpriteRenderer compatibility with URP"
  - materials: "Default materials work with URP pipeline"
  
RESOURCES_LOADING:
  - sprite: "Loading icon02.png from Resources folder"
  - path: "Resources.Load<Sprite>(\"icon02\")"
```

## Validation Loop

### Level 1: Code Quality and Structure
```bash
# Manual code review checklist:
# 1. Scripts follow Unity's C# coding standards
# 2. Proper use of DOTween sequences and tweens
# 3. Correct implementation of object pooling pattern
# 4. Error handling for missing resources
# 5. Comments explain complex animation and pooling logic
# 6. Consistent naming conventions
# 7. No hardcoded values that should be configurable
# 8. Proper memory management and cleanup
```

### Level 2: Functional Testing
```csharp
// Test cases to verify implementation:
// 1. Prefab functionality:
//    - Coin prefab instantiates correctly
//    - Components are properly attached
//    - Default parameters work

// 2. Object pooling:
//    - Pool initializes with correct number of coins
//    - Coins can be retrieved from pool
//    - Coins return to pool after use
//    - New coins created when pool is empty

// 3. Animation system:
//    - CoinAnimationSystem spawns coins correctly
//    - Animation parameters affect coin movement
//    - Multiple coins can animate simultaneously
//    - Coin burst functionality works with random positions
//    - Coin burst callback fires when all coins complete

// 4. Memory management:
//    - No memory leaks after animations
//    - Objects properly deactivated/reactivated
//    - Pool size remains stable

// 5. Edge cases:
//    - Animation can be stopped before completion
//    - Multiple rapid spawns don't cause issues
//    - No conflicts between simultaneous animations
//    - Coin burst with zero or negative count handled properly
```

### Level 3: Performance Test
```bash
# In Unity Editor:
# 1. Create test scene with CoinAnimationSystem
# 2. Spawn 50 coins simultaneously
# 3. Monitor frame rate and memory usage
# 4. Profile with Unity Profiler
#
# Expected: Stable performance with no frame drops
# If issues: Check object pooling implementation and DOTween cleanup
```

## Final validation Checklist
- [ ] DOTween package properly installed and configured
- [ ] Coin component created with proper animation implementation
- [ ] Coin prefab created and configured correctly
- [ ] Object pool manager handles coin lifecycle efficiently
- [ ] Coin animation system provides high-level control
- [ ] Flying animation works with movement, rotation, and scaling
- [ ] System can handle multiple simultaneous coin animations
- [ ] Coin burst functionality spawns multiple coins with random positions
- [ ] Coin burst callback works correctly when all coins complete
- [ ] Memory is properly managed with no leaks
- [ ] Implementation follows Unity and DOTween best practices
- [ ] Code is well-documented with explanations
- [ ] All success criteria from initial requirements met

---
## Anti-Patterns to Avoid
- ❌ Don't forget to initialize DOTween before use
- ❌ Don't create memory leaks by not killing tweens
- ❌ Don't hardcode animation values without inspector controls
- ❌ Don't ignore URP compatibility requirements
- ❌ Don't use deprecated DOTween methods
- ❌ Don't create scripts without proper error handling
- ❌ Don't skip testing different parameter combinations
- ❌ Don't instantiate prefabs directly without using pool
- ❌ Don't modify pooled objects while they're active

## Implementation Patterns
- ✅ Use DOTween sequences for complex animations
- ✅ Implement proper tween cleanup and disposal
- ✅ Make all animation parameters configurable
- ✅ Follow Unity's component-based architecture
- ✅ Use Resources.Load for asset loading
- ✅ Implement error handling for missing assets
- ✅ Use object pooling for frequently instantiated objects
- ✅ Implement callback system for object return to pool
- ✅ Use descriptive variable names that convey intent
- ✅ Comment complex animation and pooling logic
- ✅ Design for extensibility and future enhancements

## Performance Optimization Techniques
- ✅ Kill tweens properly to prevent memory leaks
- ✅ Use object pooling to reduce instantiation overhead
- ✅ Limit simultaneous animations to prevent performance issues
- ✅ Use appropriate ease functions for smooth performance
- ✅ Cache component references to avoid repeated GetComponent calls
- ✅ Use local variables instead of repeated property access
- ✅ Optimize animation duration for responsiveness
- ✅ Use appropriate update modes for tweens
- ✅ Pre-allocate pool to avoid runtime instantiation
- ✅ Monitor active vs pooled object counts

## Security Considerations
- ✅ Validate all input parameters to prevent unexpected behavior
- ✅ Sanitize resource paths to prevent directory traversal
- ✅ Use secure coding practices in C# scripts
- ✅ Avoid exposing internal animation data to users
- ✅ Implement proper error handling to prevent crashes
- ✅ Follow Unity's security best practices for script development

## Maintenance Best Practices
- ✅ Document complex animation algorithms and pooling logic
- ✅ Use consistent naming conventions across scripts
- ✅ Add version information and change logs
- ✅ Implement proper error logging and debugging tools
- ✅ Create backup and recovery procedures for animation development
- ✅ Plan for future enhancements and scalability
- ✅ Follow established Unity development standards
- ✅ Keep up to date with DOTween API changes

This PRP provides a comprehensive guide for implementing an enhanced coin flying animation system in Unity using DOTween with object pooling, packaged as a prefab for easy reuse, with all required features while maintaining high code quality, performance standards, security, and compatibility.