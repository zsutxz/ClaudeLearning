## Goal
Create a complete PRP (Problem Resolution Protocol) for implementing a ray tracing rendering example in Unity using URP (Universal Render Pipeline) shaders. This PRP will serve as a comprehensive implementation guide for developers working on advanced rendering techniques in Unity.

## Why
- Demonstrate advanced rendering techniques using ray tracing in Unity
- Provide educational content for developers learning about real-time ray tracing
- Showcase the capabilities of Unity's HDRP/URP ray tracing features
- Create reusable shader templates for future ray tracing projects
- Establish best practices for performance optimization in ray traced scenes

## What
- Unity project with ray tracing implementation using URP shaders
- Basic scene with primitive objects to demonstrate ray tracing effects
- Custom URP shaders that utilize ray tracing capabilities
- Interactive controls to adjust ray tracing parameters
- Performance monitoring and optimization guidelines
- Documentation explaining the implementation approach

### Success Criteria
- [ ] Unity scene with ray tracing enabled renders correctly
- [ ] Custom URP shaders demonstrate ray tracing effects (reflections, shadows, etc.)
- [ ] Interactive controls allow real-time adjustment of ray tracing parameters
- [ ] Performance is optimized for real-time rendering
- [ ] Implementation follows Unity best practices for ray tracing
- [ ] Code is well-documented with explanations of key concepts
- [ ] Cross-platform compatibility verified

## All Needed Context

### Documentation & References
```yaml
# MUST READ - Include these in your context window
- url: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/ray-tracing/getting-started.html
  why: Official Unity documentation for getting started with ray tracing in URP

- url: https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Ray-Tracing-Getting-Started.html
  why: Reference for ray tracing concepts in Unity (HDRP, but many concepts apply to URP)

- url: https://docs.unity3d.com/Manual/urp-renderer-volume.html
  why: Understanding URP renderer features and ray tracing integration

- url: https://docs.unity3d.com/Manual/shader-writing.html
  why: Shader writing basics in Unity

- url: https://docs.unity3d.com/Manual/SL-ShaderPrograms.html
  why: Understanding shader programs and their structure

- url: https://github.com/Unity-Technologies/Graphics
  why: Reference for Unity's graphics examples and best practices
```

### Current Codebase tree
```bash
.
├── CLAUDE.md
├── INITIAL_RayTracing.md
├── PRPs/
│   ├── templates/
│   │   └── prp_base.md
│   └── ray-tracing-implementation.md (this file)
├── examples/
├── src/
│   ├── assets/
│   └── components/
└── tools/
```

### Desired Codebase tree with files to be added and responsibility of file
```bash
PRPs/ray-tracing-implementation.md:
  - Complete PRP specification for ray tracing implementation in Unity
  - Implementation blueprint and validation gates
  - Best practices documentation
  - Testing strategies

UnityProject/
  - Assets/Scenes/RayTracingScene.unity:
    - Main scene demonstrating ray tracing effects
    - Primitive objects (spheres, cubes) for ray tracing tests
    - Lighting setup with ray traced shadows and reflections

  - Assets/Shaders/RayTracingShader.shader:
    - Custom URP shader that utilizes ray tracing features
    - Implementation of ray traced reflections and shadows

  - Assets/Scripts/RayTracingController.cs:
    - Script to control ray tracing parameters in real-time
    - UI integration for parameter adjustments

  - Assets/Materials/RayTracingMaterial.mat:
    - Material using the custom ray tracing shader
    - Configurable properties for ray tracing effects
```

### Known Gotchas of our codebase & Library Quirks
```csharp
// CRITICAL: Unity's ray tracing requires compatible hardware (DXR support)
// CRITICAL: URP ray tracing has different capabilities than HDRP
// CRITICAL: Performance can degrade significantly with complex scenes
// CRITICAL: Shader compilation can fail silently - always check console
// CRITICAL: Ray tracing effects may not work in all build targets
// CRITICAL: Need to enable ray tracing in the URP asset settings
// CRITICAL: Some ray tracing features require specific Unity package versions
```

## Implementation Blueprint

### Data models and structure
```csharp
// Ray tracing configuration parameters
public struct RayTracingParams
{
    public float reflectionIntensity;  // Intensity of ray traced reflections
    public float shadowSoftness;       // Softness of ray traced shadows
    public int rayDepth;              // Maximum ray depth for recursive tracing
    public float rayLength;           // Maximum distance for ray casts
};

// Controller for managing ray tracing settings
public class RayTracingController : MonoBehaviour
{
    public RayTracingParams params;   // Current ray tracing parameters
    public Material rayTracingMaterial; // Material with ray tracing shader
    public Slider reflectionSlider;   // UI slider for reflection intensity
    public Slider shadowSlider;       // UI slider for shadow softness
    // ... other UI controls
};
```

### list of tasks to be completed to fulfill the PRP in the order they should be completed
```yaml
Task 1:
SETUP Unity project with URP:
  - CREATE new Unity project with URP template
  - CONFIGURE URP asset to enable ray tracing features
  - VERIFY hardware compatibility and ray tracing support

Task 2:
CREATE basic scene with primitive objects:
  - ADD several primitive objects (spheres, cubes) to the scene
  - CONFIGURE lighting with directional light
  - SET UP camera with appropriate positioning

Task 3:
IMPLEMENT ray tracing shader:
  - CREATE custom URP shader that supports ray tracing
  - ADD ray traced reflection and shadow calculations
  - IMPLEMENT ray-scene intersection functions

Task 4:
CREATE ray tracing controller script:
  - DEVELOP script to manage ray tracing parameters
  - ADD UI controls for adjusting ray tracing settings
  - BIND UI controls to shader parameters

Task 5:
ADD performance monitoring and optimization:
  - IMPLEMENT frame rate monitoring
  - ADD performance optimization techniques
  - INCLUDE quality settings for different hardware tiers

Task 6:
TEST and VALIDATE implementation:
  - VERIFY ray tracing effects render correctly
  - TEST cross-platform compatibility
  - VALIDATE performance meets requirements
```

### Per task pseudocode as needed added to each task
```csharp
// Task 1: Setup Unity project with URP
// In Unity Editor:
// 1. Create new 3D URP project
// 2. In Project Settings > Graphics:
//    - Set Scriptable Render Pipeline Settings to URP asset
// 3. In URP Asset settings:
//    - Enable Ray Tracing (if available)
//    - Configure ray tracing quality settings

// Task 2: Create basic scene
// Create scene hierarchy:
// - Main Camera
// - Directional Light
// - GameObjects (Sphere, Cube, etc.)
// - Empty GameObject for RayTracingController

// Task 3: Ray tracing shader implementation
Shader "Custom/RayTracingShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ReflectionIntensity ("Reflection Intensity", Range(0, 1)) = 0.5
        _ShadowSoftness ("Shadow Softness", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // For ray tracing support
            #pragma require raytracing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ReflectionIntensity;
            float _ShadowSoftness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.normal = TransformObjectToWorldNormal(v.normal);
                return o;
            }

            // Ray tracing functions would go here
            // For URP, ray tracing is typically done through the renderer
            // This is a simplified example - actual implementation would be more complex

            half4 frag (v2f i) : SV_Target
            {
                // Sample base texture
                half4 col = tex2D(_MainTex, i.uv);
                
                // Add ray traced effects (simplified)
                // In a real implementation, this would involve:
                // 1. Ray-scene intersection calculations
                // 2. Reflection ray tracing
                // 3. Shadow ray tracing
                // 4. Global illumination calculations
                
                return col;
            }
            ENDHLSL
        }
    }
}

// Task 4: Ray tracing controller script
using UnityEngine;
using UnityEngine.UI;

public class RayTracingController : MonoBehaviour
{
    [Header("Ray Tracing Parameters")]
    public float reflectionIntensity = 0.5f;
    public float shadowSoftness = 0.5f;
    public int rayDepth = 3;
    
    [Header("UI Elements")]
    public Slider reflectionSlider;
    public Slider shadowSlider;
    public Material rayTracingMaterial;
    
    void Start()
    {
        // Initialize UI controls
        if (reflectionSlider != null)
        {
            reflectionSlider.onValueChanged.AddListener(OnReflectionIntensityChanged);
            reflectionSlider.value = reflectionIntensity;
        }
        
        if (shadowSlider != null)
        {
            shadowSlider.onValueChanged.AddListener(OnShadowSoftnessChanged);
            shadowSlider.value = shadowSoftness;
        }
    }
    
    void OnReflectionIntensityChanged(float value)
    {
        reflectionIntensity = value;
        if (rayTracingMaterial != null)
        {
            rayTracingMaterial.SetFloat("_ReflectionIntensity", value);
        }
    }
    
    void OnShadowSoftnessChanged(float value)
    {
        shadowSoftness = value;
        if (rayTracingMaterial != null)
        {
            rayTracingMaterial.SetFloat("_ShadowSoftness", value);
        }
    }
    
    // Additional methods for controlling ray tracing parameters
}

// Task 5: Performance monitoring
using UnityEngine;

public class PerformanceMonitor : MonoBehaviour
{
    [Header("Performance Settings")]
    public bool enablePerformanceMonitoring = true;
    public int targetFrameRate = 60;
    
    private float frameRate;
    private float deltaTime;
    
    void Update()
    {
        if (enablePerformanceMonitoring)
        {
            // Calculate frame rate
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            frameRate = 1.0f / deltaTime;
            
            // Adjust quality settings based on performance
            AdjustQualitySettings();
        }
    }
    
    void AdjustQualitySettings()
    {
        // If frame rate is low, reduce ray tracing quality
        if (frameRate < targetFrameRate * 0.8f)
        {
            // Reduce ray depth or other expensive parameters
            // This would communicate with the ray tracing controller
        }
    }
    
    void OnGUI()
    {
        if (enablePerformanceMonitoring)
        {
            GUI.Label(new Rect(10, 10, 200, 20), $"FPS: {frameRate:F2}");
        }
    }
}
```

### Integration Points
```yaml
UNITY_URP:
  - integration: "URP asset configuration for ray tracing support"
  - settings: "Quality settings and ray tracing configuration"
  
SHADER_INTEGRATION:
  - material: "Custom materials using ray tracing shaders"
  - properties: "Exposed shader properties for runtime control"
  
UI_CONTROLS:
  - sliders: "Real-time parameter adjustment through UI sliders"
  - binding: "Binding between UI controls and shader parameters"
```

## Validation Loop

### Level 1: Code Quality and Structure
```bash
# Manual code review checklist:
# 1. Shader code follows Unity's HLSL coding standards
# 2. C# scripts have clear separation of concerns
# 3. Proper error handling for unsupported hardware
# 4. Comments explain complex ray tracing algorithms
# 5. Consistent naming conventions across files
# 6. No hardcoded values that should be configurable
# 7. Appropriate use of Unity's ray tracing APIs
```

### Level 2: Functional Testing
```csharp
// Test cases to verify implementation:
// 1. Ray tracing functionality:
//    - Reflections render correctly on reflective surfaces
//    - Shadows are accurately calculated with ray tracing
//    - Global illumination effects are visible
//    - Ray traced effects respond to scene changes

// 2. UI controls:
//    - All sliders adjust their respective parameters
//    - Shader properties update in real-time
//    - UI responds to user input without errors

// 3. Performance:
//    - Frame rate remains acceptable on supported hardware
//    - Quality settings adjust automatically based on performance
//    - Memory usage is stable over time
//    - No crashes or rendering artifacts

// 4. Compatibility:
//    - Scene renders correctly on ray tracing supported hardware
//    - Fallback rendering works on unsupported hardware
//    - No errors in Unity console
```

### Level 3: Platform Compatibility
```bash
# Test on multiple platforms:
# 1. Windows with DirectX 12 and ray tracing support
# 2. Platforms without ray tracing support (fallback rendering)
# 3. Different quality settings (low, medium, high)
#
# Verify consistent behavior:
# - Ray tracing effects render correctly on supported platforms
# - Fallback rendering works properly on unsupported platforms
# - Performance is acceptable across different hardware configurations
# - UI controls function properly on all platforms
```

## Final validation Checklist
- [ ] Requirements analysis complete with technical approach documented
- [ ] Unity project configured correctly with URP and ray tracing support
- [ ] Basic scene with primitive objects created for testing
- [ ] Custom URP shader implemented with ray tracing features
- [ ] Controller script developed for parameter adjustment
- [ ] Performance monitoring and optimization implemented
- [ ] Comprehensive testing strategy defined
- [ ] Best practices and implementation patterns documented
- [ ] Cross-platform compatibility verified
- [ ] Code quality meets established standards
- [ ] All success criteria from initial requirements met

---
## Anti-Patterns to Avoid
- ❌ Don't assume all hardware supports ray tracing
- ❌ Don't hardcode ray tracing parameters without UI controls
- ❌ Don't ignore performance implications of ray tracing
- ❌ Don't use complex ray tracing effects without fallbacks
- ❌ Don't skip testing on hardware without ray tracing support
- ❌ Don't create shaders that are incompatible with URP
- ❌ Don't neglect error handling for unsupported features
- ❌ Don't use ray tracing for effects that can be achieved with simpler techniques

## Implementation Patterns
- ✅ Use Unity's built-in ray tracing APIs when available
- ✅ Implement fallback rendering for unsupported hardware
- ✅ Separate ray tracing logic from basic rendering
- ✅ Use property drawers for shader parameter control
- ✅ Follow Unity's shader coding standards
- ✅ Implement performance monitoring and adaptive quality
- ✅ Use descriptive variable names that convey intent
- ✅ Comment complex mathematical calculations
- ✅ Implement proper error handling and validation
- ✅ Design for extensibility and future enhancements

## Performance Optimization Techniques
- ✅ Limit ray depth to prevent exponential ray growth
- ✅ Use spatial data structures for efficient ray-scene intersection
- ✅ Implement adaptive sampling based on scene complexity
- ✅ Use denoising techniques for ray traced effects
- ✅ Cache ray tracing results when possible
- ✅ Adjust ray count based on performance budget
- ✅ Use level of detail (LOD) for complex scenes
- ✅ Implement occlusion culling to reduce unnecessary rays
- ✅ Optimize shader code to reduce computation per ray
- ✅ Use texture streaming for ray traced textures

## Hardware Compatibility Considerations
- ✅ Check for ray tracing hardware support at runtime
- ✅ Provide visual feedback for unsupported features
- ✅ Implement graceful degradation for older hardware
- ✅ Test on various GPU architectures (NVIDIA, AMD, Intel)
- ✅ Verify compatibility with different DirectX/Vulkan versions
- ✅ Handle cases where drivers don't support required features
- ✅ Provide alternative rendering paths for unsupported systems
- ✅ Document minimum system requirements clearly

## Security Considerations
- ✅ Validate all user inputs to prevent shader injection
- ✅ Sanitize shader parameters to prevent out-of-bounds access
- ✅ Use secure coding practices in C# scripts
- ✅ Avoid exposing internal rendering data to users
- ✅ Implement proper error handling to prevent crashes
- ✅ Follow Unity's security best practices for plugin development

## Maintenance Best Practices
- ✅ Document complex ray tracing algorithms and calculations
- ✅ Use consistent naming conventions across shaders and scripts
- ✅ Add version information and change logs
- ✅ Implement proper error logging and debugging tools
- ✅ Create backup and recovery procedures for shader development
- ✅ Plan for future enhancements and scalability
- ✅ Follow established Unity development standards
- ✅ Keep up to date with Unity's ray tracing API changes

This PRP provides a comprehensive guide for implementing ray tracing in Unity using URP shaders with all required features while maintaining high code quality, performance standards, security, and compatibility.