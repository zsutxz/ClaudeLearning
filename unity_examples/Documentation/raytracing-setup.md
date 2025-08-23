# Unity URP Ray Tracing Setup Guide

## Overview
This guide explains how to set up and use the Ray Tracing example in Unity's Universal Render Pipeline (URP).

## Hardware Requirements
- NVIDIA RTX series GPU (20xx, 30xx, 40xx) or equivalent AMD GPU with ray tracing support
- DirectX 12 or Vulkan graphics API

## Software Requirements
- Unity 2021.2 or newer
- Universal Render Pipeline (URP) 12.0 or newer
- Windows 10 version 1809 or newer (for DXR support)

## Setup Instructions

### 1. Configure URP for Ray Tracing
1. In your URP Asset:
   - Enable "Use SRP Batcher"
   - Set "Renderer Type" to "Forward Renderer" or "Universal Renderer"
   - Enable "Enable Ray Tracing" option (if available)

### 2. Create a Scene with Ray Tracing
1. Create a new scene or open an existing one
2. Ensure you have:
   - A Main Camera with Camera component
   - At least one Directional Light
   - Some geometry (spheres, cubes, etc.) for ray tracing to interact with

### 3. Add Ray Tracing Controller
1. Select your Main Camera in the Hierarchy
2. Add the `RayTracingController` component:
   - In the Inspector, click "Add Component"
   - Search for "RayTracingController" and add it
3. Assign the required references:
   - Drag the `RayTracingExample` shader to the "Ray Tracing Shader" field
   - The Directional Light should be auto-assigned, but you can change it if needed

### 4. Configure Ray Tracing Settings
In the RayTracingController component, you can adjust:
- **Ray Count**: Number of rays per pixel (higher = better quality, slower performance)
- **Enable Denoiser**: Enable temporal denoising for smoother results
- **Max Depth**: Maximum ray bounce depth

## Usage Tips

### Performance Optimization
- Start with Ray Count = 1 for real-time performance
- Use simpler geometry for better ray tracing performance
- Ensure your scene has proper lighting setup

### Troubleshooting
- **No ray tracing effect visible**: Check that your GPU supports ray tracing
- **Performance issues**: Reduce ray count or disable denoiser
- **Black screen**: Ensure you have a Directional Light in the scene
- **Shader compilation errors**: Verify URP version compatibility

## Known Limitations
- Ray tracing is not supported on all platforms
- Performance can be significantly lower than rasterization
- May not work in Unity's Editor on some hardware configurations

## Example Scene
The package includes an example scene demonstrating:
- Basic ray tracing setup
- Simple lighting and materials
- Performance considerations

## API Notes
The RayTracingController script provides:
- Automatic hardware support detection
- Acceleration structure management
- Render texture handling
- Temporal denoising (basic implementation)

## References
- [Unity URP Ray Tracing Documentation](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/manual/ray-tracing/getting-started.html)
- [Unity Core RP Ray Tracing](https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@latest/manual/raytracing.html)