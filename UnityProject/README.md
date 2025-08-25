# Ray Tracing Implementation in Unity

This project demonstrates ray tracing implementation in Unity using URP (Universal Render Pipeline) shaders.

## Project Structure

```
UnityProject/
  Assets/
    Scenes/
      RayTracingScene.unity        # Main scene with ray tracing objects
    Shaders/
      RayTracingShader.shader       # Custom URP shader with ray tracing features
    Scripts/
      RayTracingController.cs      # Controller for ray tracing parameters
      PerformanceMonitor.cs        # Performance monitoring and optimization
    Materials/
      RayTracingMaterial.mat       # Material using the ray tracing shader
```

## Features

1. **Ray Traced Reflections**: Realistic reflections using ray tracing techniques
2. **Ray Traced Shadows**: Accurate soft shadows with ray tracing
3. **Interactive Controls**: UI sliders to adjust ray tracing parameters in real-time
4. **Performance Monitoring**: Frame rate monitoring and adaptive quality settings

## Implementation Details

### Ray Tracing Shader
The `RayTracingShader.shader` implements basic ray tracing features within URP constraints. Note that full ray tracing in URP requires specific hardware support and Unity package versions.

### Ray Tracing Controller
The `RayTracingController.cs` script manages ray tracing parameters and binds UI controls to shader properties.

### Performance Monitor
The `PerformanceMonitor.cs` script monitors frame rate and can adjust quality settings based on performance.

## Requirements

- Unity 2021.2 or later
- Universal Render Pipeline (URP) package
- DirectX 12 compatible hardware with ray tracing support (NVIDIA RTX, AMD RX 6000 series, Intel Arc)

## Setup Instructions

1. Create a new Unity project with URP template
2. Import the files from this repository
3. Configure the URP asset to enable ray tracing features
4. Open the `RayTracingScene.unity` scene
5. Ensure ray tracing is enabled in the project settings

## Limitations

- URP has more limited ray tracing capabilities compared to HDRP
- Requires compatible hardware for ray tracing features
- Performance may vary significantly based on scene complexity