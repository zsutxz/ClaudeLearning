name: "Unity URP Ray Tracing Example Implementation"
description: |
  ## Purpose
  Create a complete Unity URP ray tracing example with shader and controller script following best practices and Unity's official documentation.

  ## Core Principles
  1. **Context is King**: Include ALL necessary documentation, examples, and caveats for Unity ray tracing
  2. **Validation Loops**: Provide executable tests/lints the AI can run and fix
  3. **Information Dense**: Use keywords and patterns from Unity's ray tracing implementation
  4. **Progressive Success**: Start simple, validate, then enhance
  5. **Global rules**: Be sure to follow all rules in CLAUDE.md

---
## Goal
Implement a complete Unity URP ray tracing example that demonstrates basic ray tracing functionality in Unity's Universal Render Pipeline, including a custom shader and controller script.

## Why
- Provide developers with a working example of Unity URP ray tracing implementation
- Demonstrate proper setup and configuration of ray tracing in Unity
- Serve as a reference for more complex ray tracing features
- Help developers understand performance considerations and hardware requirements

## What
A Unity package containing:
1. A ray tracing shader that demonstrates basic ray tracing functionality
2. A C# controller script to manage ray tracing effects
3. Documentation explaining setup and usage
4. Example scene showcasing the ray tracing effect

### Success Criteria
- [ ] Ray tracing effect renders correctly in a Unity scene
- [ ] Code follows Unity's best practices for URP ray tracing
- [ ] All hardware and software requirements are documented
- [ ] Example runs on supported hardware (NVIDIA RTX or equivalent AMD GPU)

## All Needed Context

### Documentation & References (list all context needed to implement the feature)
```yaml
# MUST READ - Include these in your context window
- url: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/ray-tracing/getting-started.html
  why: Official Unity URP ray tracing documentation and setup guide

- url: https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@14.0/manual/raytracing.html
  why: Core RP ray tracing documentation with technical details

- file: INITIAL.md
  why: Contains the original feature request and requirements
```

### Current Codebase tree (run `tree` in the root of the project) to get an overview of the codebase
```bash
.
├── CLAUDE.md
├── INITIAL.md
├── PRPs/
│   └── templates/
│       └── prp_base.md
├── examples/
├── tools/
└── unity_examples/
    ├── RayTracing/
    │   ├── Shaders/
    │   └── Scripts/
    └── Documentation/
```

### Desired Codebase tree with files to be added and responsibility of file
```bash
unity_examples/
├── RayTracing/
│   ├── Shaders/
│   │   └── RayTracingExample.shader  # Ray tracing HLSL shader implementation
│   └── Scripts/
│       └── RayTracingController.cs    # C# controller for ray tracing effects
└── Documentation/
    └── raytracing-setup.md           # Setup guide and requirements
```

### Known Gotchas of our codebase & Library Quirks
```csharp
// CRITICAL: Unity's RayTracingShader requires specific setup in URP
// Example: Must be assigned to a RayTracingAccelerationStructure component
// Example: Requires DirectX 12 or Vulkan with RT support
// Example: Performance degrades significantly with complex scenes
```

## Implementation Blueprint

### Data models and structure

Create the core Unity components for ray tracing:
- RayTracingShader asset (HLSL)
- C# script for controlling ray tracing effects
- Documentation for setup and requirements

### list of tasks to be completed to fullfill the PRP in the order they should be completed

```yaml
Task 1:
CREATE unity_examples/RayTracing/Shaders/RayTracingExample.shader:
  - FOLLOW pattern from: Unity official ray tracing documentation
  - IMPLEMENT basic ray generation, hit and miss shaders
  - INCLUDE comments explaining each section

Task 2:
CREATE unity_examples/RayTracing/Scripts/RayTracingController.cs:
  - FOLLOW pattern from: Unity component design patterns
  - IMPLEMENT ray tracing initialization and management
  - INCLUDE proper error handling for unsupported hardware

Task 3:
CREATE unity_examples/Documentation/raytracing-setup.md:
  - DOCUMENT all hardware and software requirements
  - INCLUDE step-by-step setup instructions
  - PROVIDE troubleshooting tips

Task 4:
VALIDATE implementation:
  - TEST on supported hardware
  - VERIFY performance considerations are addressed
  - CONFIRM documentation is accurate and complete
```

### Per task pseudocode as needed added to each task
```csharp
// Task 1: RayTracingExample.shader
// Pseudocode with CRITICAL details
Shader "RayTracing/RayTracingExample" {
    // PATTERN: Ray tracing shaders require specific structure
    HLSLINCLUDE
        // CRITICAL: Include required ray tracing headers
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/RayTracing/RayTracingHelpers.hlsl"
        
        // GOTCHA: Ray payloads must be defined at shader level
        struct RayPayload {
            float3 color : SV_Target0;
        };
        
        // CRITICAL: Ray generation shader - entry point for rays
        [shader("raygeneration")]
        void RayGeneration() {
            // PATTERN: Get ray from camera
            // Implementation details...
        }
        
        // PATTERN: Closest hit shader - called when ray hits geometry
        [shader("closesthit")]
        void ClosestHit(inout RayPayload payload, AttributeData attrib) {
            // Implementation details...
        }
        
        // PATTERN: Miss shader - called when ray doesn't hit anything
        [shader("miss")]
        void Miss(inout RayPayload payload) {
            // Implementation details...
        }
    ENDHLSL
    
    SubShader {
        Pass {
            Name "RayTracing"
            // CRITICAL: Tags required for ray tracing
            Tags { "LightMode" = "RayTracing" }
            HLSLPROGRAM
            // Entry points for ray tracing shaders
            #pragma raytracing surface
            ENDHLSL
        }
    }
}

// Task 2: RayTracingController.cs
// Pseudocode with CRITICAL details
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class RayTracingController : MonoBehaviour {
    // PATTERN: SerializeField for inspector exposure
    [SerializeField] private RayTracingShader rayTracingShader;
    [SerializeField] private Light directionalLight;
    
    // GOTCHA: Ray tracing requires specific components
    private RayTracingAccelerationStructure accelerationStructure;
    private Camera mainCamera;
    
    void Start() {
        // PATTERN: Always check for hardware support first
        if (!SystemInfo.supportsRayTracing) {
            Debug.LogError("Ray tracing not supported on this hardware");
            enabled = false;
            return;
        }
        
        // CRITICAL: Initialize acceleration structure
        accelerationStructure = new RayTracingAccelerationStructure();
        accelerationStructure.Build();
        
        mainCamera = Camera.main;
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        // GOTCHA: Ray tracing must be executed in specific render events
        if (rayTracingShader != null && accelerationStructure != null) {
            // PATTERN: Set shader parameters before execution
            rayTracingShader.SetAccelerationStructure("AccelerationStructure", accelerationStructure);
            // Execute ray tracing...
        }
    }
    
    void OnDestroy() {
        // PATTERN: Always dispose of resources
        if (accelerationStructure != null) {
            accelerationStructure.Dispose();
        }
    }
}
```

### Integration Points
```yaml
UNITY_PACKAGE:
  - dependency: "com.unity.render-pipelines.universal@14.0"
  - dependency: "com.unity.shadergraph@14.0"
  
COMPONENTS:
  - add to: GameObject with Camera component
  - requirement: RayTracingController component
  - requirement: RayTracingShader asset assigned
  
RENDER_PIPELINE:
  - configuration: UniversalRenderPipelineAsset with ray tracing enabled
  - setting: "Enable Ray Tracing" must be checked
```

## Validation Loop

### Level 1: Syntax & Style
```bash
# Run these FIRST - fix any errors before proceeding
# For Unity C# scripts:
# Use Unity's built-in code analysis or external tools like ReSharper

# For HLSL shaders:
# Validate shader compilation in Unity Editor
# Check for syntax errors in the console
```

### Level 2: Unit Tests each new feature/file/function use existing test patterns
```csharp
// CREATE RayTracingTests.cs with these test cases:
public class RayTracingTests {
    [Test]
    public void TestRayTracingControllerInitialization() {
        """RayTracingController initializes correctly on supported hardware"""
        // Mock SystemInfo.supportsRayTracing to return true
        // Create GameObject with RayTracingController
        // Assert that component is properly initialized
        Assert.IsNotNull(controller.accelerationStructure);
    }
    
    [Test]
    public void TestRayTracingControllerHardwareSupport() {
        """RayTracingController handles unsupported hardware gracefully"""
        // Mock SystemInfo.supportsRayTracing to return false
        // Create GameObject with RayTracingController
        // Assert that component is disabled and error is logged
        Assert.IsFalse(controller.enabled);
    }
    
    [Test]
    public void TestRayTracingShaderAssignment() {
        """RayTracingController properly handles shader assignment"""
        // Create controller with null shader
        // Assert appropriate behavior
        // Assign valid shader
        // Assert shader is properly used
    }
}
```

```bash
# Run and iterate until passing:
# Execute tests in Unity Test Framework
# If failing: Read error, understand root cause, fix code, re-run
```

### Level 3: Integration Test
```bash
# Manual testing steps:
# 1. Create new Unity project with URP 14.0+
# 2. Import the ray tracing example package
# 3. Add RayTracingController to main camera
# 4. Assign RayTracingExample shader to the controller
# 5. Ensure directional light exists in scene
# 6. Enter play mode and verify ray tracing effect renders

# Expected: Ray tracing effect visible in Game view
# If error: Check Unity console for error messages
```

## Final validation Checklist
- [ ] Ray tracing shader compiles without errors
- [ ] RayTracingController initializes correctly
- [ ] Example scene demonstrates ray tracing effect
- [ ] Documentation covers all requirements and setup steps
- [ ] Code handles unsupported hardware gracefully
- [ ] All resources are properly disposed
- [ ] Performance considerations are addressed

---
## Anti-Patterns to Avoid
- ❌ Don't skip hardware support checks
- ❌ Don't create overly complex shaders that hurt performance
- ❌ Don't forget to dispose of acceleration structures
- ❌ Don't ignore error handling for unsupported features
- ❌ Don't hardcode values that should be configurable
- ❌ Don't forget to include required dependencies