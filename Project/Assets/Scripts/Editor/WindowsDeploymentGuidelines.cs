using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace CoinAnimation.Editor
{
    /// <summary>
    /// Windows Platform Deployment Guidelines Generator
    /// Story 2.2 Task 2.4: Create platform deployment guidelines
    /// Comprehensive Windows deployment guidelines and best practices
    /// </summary>
    public class WindowsDeploymentGuidelines : EditorWindow
    {
        private Vector2 scrollPosition;
        private WindowsDeploymentConfig deploymentConfig = new WindowsDeploymentConfig();
        
        [MenuItem("Coin Animation/Compatibility/Windows Deployment Guidelines")]
        public static void ShowWindow()
        {
            GetWindow<WindowsDeploymentGuidelines>("Windows Deployment Guidelines");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Windows Platform Deployment Guidelines", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Story 2.2 Task 2.4: Platform Deployment Guidelines", EditorStyles.miniLabel);
            EditorGUILayout.Space();
            
            // Configuration section
            EditorGUILayout.LabelField("Deployment Configuration:", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                deploymentConfig.targetUnityVersion = EditorGUILayout.TextField("Target Unity Version", deploymentConfig.targetUnityVersion);
                deploymentConfig.targetWindowsVersion = EditorGUILayout.TextField("Target Windows Version", deploymentConfig.targetWindowsVersion);
                deploymentConfig.buildArchitecture = (BuildArchitecture)EditorGUILayout.EnumPopup("Build Architecture", deploymentConfig.buildArchitecture);
                deploymentConfig.scriptingBackend = (ScriptingImplementation)EditorGUILayout.EnumPopup("Scripting Backend", deploymentConfig.scriptingBackend);
                deploymentConfig.compressionLevel = (BuildCompression)EditorGUILayout.EnumPopup("Compression Level", deploymentConfig.compressionLevel);
            }
            
            EditorGUILayout.Space();
            
            // Generation buttons
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Generate Guidelines", GUILayout.Height(30)))
                {
                    GenerateDeploymentGuidelines();
                }
                
                if (GUILayout.Button("Export to File", GUILayout.Width(120), GUILayout.Height(30)))
                {
                    ExportGuidelinesToFile();
                }
                
                if (GUILayout.Button("Validate Configuration", GUILayout.Width(140), GUILayout.Height(30)))
                {
                    ValidateDeploymentConfiguration();
                }
            }
            
            EditorGUILayout.Space();
            
            // Guidelines preview
            if (!string.IsNullOrEmpty(deploymentConfig.generatedGuidelines))
            {
                EditorGUILayout.LabelField("Generated Guidelines:", EditorStyles.boldLabel);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));
                EditorGUILayout.TextArea(deploymentConfig.generatedGuidelines, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }
        
        private void GenerateDeploymentGuidelines()
        {
            var guidelines = new System.Text.StringBuilder();
            
            guidelines.AppendLine("# Coin Animation System - Windows Platform Deployment Guidelines");
            guidelines.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            guidelines.AppendLine($"Unity Version: {deploymentConfig.targetUnityVersion}");
            guidelines.AppendLine($"Target Platform: Windows {deploymentConfig.targetWindowsVersion}");
            guidelines.AppendLine("=================================================================");
            guidelines.AppendLine();
            
            // 1. Prerequisites
            guidelines.AppendLine("## 1. Prerequisites");
            guidelines.AppendLine();
            guidelines.AppendLine("### System Requirements");
            guidelines.AppendLine("- **Operating System**: Windows 10 (Version 1903+) or Windows 11");
            guidelines.AppendLine("- **Unity Editor**: " + deploymentConfig.targetUnityVersion + " or later");
            guidelines.AppendLine("- **.NET Framework**: 4.7.2 or later (for IL2CPP builds)");
            guidelines.AppendLine("- **DirectX**: Version 11 or later");
            guidelines.AppendLine("- **Graphics**: DirectX 11/12 compatible GPU");
            guidelines.AppendLine("- **Memory**: Minimum 4GB RAM (8GB+ recommended)");
            guidelines.AppendLine("- **Storage**: 500MB free space for build");
            guidelines.AppendLine();
            
            // 2. Unity Build Configuration
            guidelines.AppendLine("## 2. Unity Build Configuration");
            guidelines.AppendLine();
            guidelines.AppendLine("### Build Settings");
            guidelines.AppendLine($"- **Target Platform**: Windows");
            guidelines.AppendLine($"- **Architecture**: {deploymentConfig.buildArchitecture}");
            guidelines.AppendLine($"- **Scripting Backend**: {deploymentConfig.scriptingBackend}");
            guidelines.AppendLine($"- **Compression**: {deploymentConfig.compressionLevel}");
            guidelines.AppendLine();
            
            guidelines.AppendLine("### Recommended Settings");
            guidelines.AppendLine("- **Api Compatibility Level**: .NET Standard 2.1");
            guidelines.AppendLine("- **Script Runtime Version**: .NET 4.x Equivalent");
            guidelines.AppendLine("- **Autoconnect Profiler**: Enabled");
            guidelines.AppendLine("- **Development Build**: Only for testing (disable for production)");
            guidelines.AppendLine("- **Script Debugging**: Only for development builds");
            guidelines.AppendLine();
            
            // 3. URP Configuration for Windows
            guidelines.AppendLine("## 3. URP Configuration for Windows");
            guidelines.AppendLine();
            guidelines.AppendLine("### Universal Render Pipeline Settings");
            guidelines.AppendLine("- **Renderer**: UniversalRenderPipelineAsset");
            guidelines.AppendLine("- **Camera Renderer**: UniversalAdditionalCameraData");
            guidelines.AppendLine("- **Render Scale**: 1.0 (or adjust for performance)");
            guidelines.AppendLine("- **Depth Texture**: Enabled (for particle effects)");
            guidelines.AppendLine("- **Opaque Texture**: Disabled (unless needed)");
            guidelines.AppendLine();
            
            guidelines.AppendLine("### Quality Settings");
            guidelines.AppendLine("- **Texture Quality**: High (adjust based on target hardware)");
            guidelines.AppendLine("- **Anisotropic Filtering**: Enable");
            guidelines.AppendLine("- **Anti-Aliasing**: 2x or 4x (based on performance)");
            guidelines.AppendLine("- **Soft Particles**: Enabled");
            guidelines.AppendLine("- **VSync**: Enabled (for smooth 60fps)");
            guidelines.AppendLine();
            
            // 4. Performance Optimization
            guidelines.AppendLine("## 4. Performance Optimization");
            guidelines.AppendLine();
            guidelines.AppendLine("### Coin Animation System Settings");
            guidelines.AppendLine("- **Max Concurrent Coins**: 50-100 (adjust based on target hardware)");
            guidelines.AppendLine("- **Target Frame Rate**: 60 FPS");
            guidelines.AppendLine("- **Object Pool Size**: 2x max concurrent coins");
            guidelines.AppendLine("- **Memory Limit**: 200MB (monitor during testing)");
            guidelines.AppendLine("- **Garbage Collection**: Minimize allocations during animation");
            guidelines.AppendLine();
            
            guidelines.AppendLine("### Windows-Specific Optimizations");
            guidelines.AppendLine("- **Process Priority**: Normal (adjust if needed)");
            guidelines.AppendLine("- **Thread Priority**: Normal for main thread, Above Normal for workers");
            guidelines.AppendLine("- **GPU Skinning**: Enabled (for character-based coins)");
            guidelines.AppendLine("- **Instancing**: Enabled for similar coin objects");
            guidelines.AppendLine("- **Occlusion Culling**: Enable if applicable");
            guidelines.AppendLine();
            
            // 5. Testing and Validation
            guidelines.AppendLine("## 5. Testing and Validation");
            guidelines.AppendLine();
            guidelines.AppendLine("### Performance Testing Checklist");
            guidelines.AppendLine("- [ ] Baseline performance measured (FPS, memory, CPU)");
            guidelines.AppendLine("- [ ] 60fps target achieved with target coin count");
            guidelines.AppendLine("- [ ] Memory usage stays within limits during extended use");
            guidelines.AppendLine("- [ ] No memory leaks detected (1+ hour testing)");
            guidelines.AppendLine("- [ ] Garbage collection spikes minimized");
            guidelines.AppendLine("- [ ] Performance consistent across different hardware");
            guidelines.AppendLine();
            
            guidelines.AppendLine("### Compatibility Testing");
            guidelines.AppendLine("- [ ] Windows 10 (Version 1903+) compatibility verified");
            guidelines.AppendLine("- [ ] Windows 11 compatibility verified");
            guidelines.AppendLine("- [ ] DirectX 11/12 compatibility verified");
            guidelines.AppendLine("- [ ] URP pipeline compatibility verified");
            guidelines.AppendLine("- [ ] Different GPU manufacturers tested (NVIDIA, AMD, Intel)");
            guidelines.AppendLine();
            
            // 6. Deployment Steps
            guidelines.AppendLine("## 6. Deployment Steps");
            guidelines.AppendLine();
            guidelines.AppendLine("### Pre-Build Checklist");
            guidelines.AppendLine("1. Clean the project (Assets → Clean)");
            guidelines.AppendLine("2. Resolve all compiler errors and warnings");
            guidelines.AppendLine("3. Run all tests (Window → General → Test Runner)");
            guidelines.AppendLine("4. Optimize assets (compressed textures, audio settings)");
            guidelines.AppendLine("5. Update version number in Player Settings");
            guidelines.AppendLine("6. Configure build settings as outlined above");
            guidelines.AppendLine();
            
            guidelines.AppendLine("### Build Process");
            guidelines.AppendLine("1. File → Build Settings");
            guidelines.AppendLine("2. Select Windows platform");
            guidelines.AppendLine("3. Configure architecture and scripting backend");
            guidelines.AppendLine("4. Click 'Build' or 'Build And Run'");
            guidelines.AppendLine("5. Choose output location");
            guidelines.AppendLine("6. Wait for build completion");
            guidelines.AppendLine();
            
            guidelines.AppendLine("### Post-Build Validation");
            guidelines.AppendLine("1. Launch the built application");
            guidelines.AppendLine("2. Verify all coin animations work correctly");
            guidelines.AppendLine("3. Test performance on target hardware");
            guidelines.AppendLine("4. Check for Windows-specific issues (fonts, paths, permissions)");
            guidelines.AppendLine("5. Validate URP rendering quality");
            guidelines.AppendLine("6. Test error handling and recovery");
            guidelines.AppendLine();
            
            // 7. Troubleshooting
            guidelines.AppendLine("## 7. Troubleshooting Common Issues");
            guidelines.AppendLine();
            guidelines.AppendLine("### Performance Issues");
            guidelines.AppendLine("- **Low FPS**: Reduce coin count, disable VSync, adjust quality settings");
            guidelines.AppendLine("- **Memory Leaks**: Check for unreleased resources, use Unity Profiler");
            guidelines.AppendLine("- **Stuttering**: Optimize garbage collection, use object pooling");
            guidelines.AppendLine("- **High CPU Usage**: Optimize animation calculations, reduce update frequency");
            guidelines.AppendLine();
            
            guidelines.AppendLine("### Rendering Issues");
            guidelines.AppendLine("- **Visual Artifacts**: Check URP configuration, shader compatibility");
            guidelines.AppendLine("- **Missing Effects**: Verify URP renderer feature list");
            guidelines.AppendLine("- **Lighting Issues**: Check URP lighting settings");
            guidelines.AppendLine("- **Particle Problems**: Verify URP particle system support");
            guidelines.AppendLine();
            
            guidelines.AppendLine("### Platform-Specific Issues");
            guidelines.AppendLine("- **Font Problems**: Include fonts in build, check character encoding");
            guidelines.AppendLine("- **Path Issues**: Use Path.Combine, avoid hardcoded paths");
            guidelines.AppendLine("- **Permission Errors**: Check application manifest if required");
            guidelines.AppendLine("- **DLL Dependencies**: Verify all required DLLs are included");
            guidelines.AppendLine();
            
            // 8. Best Practices
            guidelines.AppendLine("## 8. Best Practices");
            guidelines.AppendLine();
            guidelines.AppendLine("### Development");
            guidelines.AppendLine("- Use Windows-specific APIs judiciously");
            guidelines.AppendLine("- Test on multiple Windows versions and hardware configurations");
            guidelines.AppendLine("- Profile memory usage regularly during development");
            guidelines.AppendLine("- Use conditional compilation for platform-specific code");
            guidelines.AppendLine("- Implement proper error handling for Windows APIs");
            guidelines.AppendLine();
            
            guidelines.AppendLine("### Performance");
            guidelines.AppendLine("- Monitor frame times, not just FPS");
            guidelines.AppendLine("- Use Unity Profiler to identify bottlenecks");
            guidelines.AppendLine("- Implement adaptive quality based on hardware capabilities");
            guidelines.AppendLine("- Cache frequently accessed resources");
            guidelines.AppendLine("- Minimize runtime allocations during animations");
            guidelines.AppendLine();
            
            guidelines.AppendLine("### Deployment");
            guidelines.AppendLine("- Create installation documentation");
            guidelines.AppendLine("- Test installation process on clean systems");
            guidelines.AppendLine("- Include system requirements in documentation");
            guidelines.AppendLine("- Provide troubleshooting guide for common issues");
            guidelines.AppendLine("- Consider creating installer for professional deployment");
            guidelines.AppendLine();
            
            // 9. Monitoring and Maintenance
            guidelines.AppendLine("## 9. Monitoring and Maintenance");
            guidelines.AppendLine();
            guidelines.AppendLine("### Runtime Monitoring");
            guidelines.AppendLine("- Implement FPS monitoring and alerting");
            guidelines.AppendLine("- Track memory usage patterns");
            guidelines.AppendLine("- Monitor object pool efficiency");
            guidelines.AppendLine("- Log performance metrics for analysis");
            guidelines.AppendLine("- Implement error reporting system");
            guidelines.AppendLine();
            
            guidelines.AppendLine("### Maintenance Checklist");
            guidelines.AppendLine("- [ ] Regular performance regression testing");
            guidelines.AppendLine("- [ ] Memory leak detection and resolution");
            guidelines.AppendLine("- [ ] Compatibility testing with new Unity versions");
            guidelines.AppendLine("- [ ] Windows update compatibility verification");
            guidelines.AppendLine("- [ ] User feedback collection and analysis");
            guidelines.AppendLine("- [ ] Performance optimization based on usage data");
            guidelines.AppendLine();
            
            // 10. Appendix
            guidelines.AppendLine("## 10. Appendix");
            guidelines.AppendLine();
            guidelines.AppendLine("### Useful Unity Console Commands");
            guidelines.AppendLine("- `Application.targetFrameRate = 60;` - Set target frame rate");
            guidelines.AppendLine("- `QualitySettings.vSyncCount = 1;` - Enable VSync");
            guidelines.AppendLine("- `SystemInfo.graphicsMemorySize` - Get GPU memory info");
            guidelines.AppendLine("- `GC.Collect();` - Force garbage collection");
            guidelines.AppendLine();
            
            guidelines.AppendLine("### Performance Benchmarks");
            guidelines.AppendLine("- **Target Hardware**: Intel i5, 8GB RAM, GTX 960 or equivalent");
            guidelines.AppendLine("- **Expected Performance**: 60fps with 50 concurrent coins");
            guidelines.AppendLine("- **Memory Target**: <200MB peak usage");
            guidelines.AppendLine("- **CPU Usage**: <70% during peak animation");
            guidelines.AppendLine();
            
            guidelines.AppendLine("### Contact and Support");
            guidelines.AppendLine("- For technical issues: Use Unity Forum or GitHub Issues");
            guidelines.AppendLine("- For performance optimization: Unity Performance Optimization Guide");
            guidelines.AppendLine("- For Windows-specific issues: Microsoft Developer Documentation");
            guidelines.AppendLine();
            
            guidelines.AppendLine("---");
            guidelines.AppendLine("Generated by Coin Animation System Windows Deployment Guidelines");
            guidelines.AppendLine($"Last Updated: {DateTime.Now:yyyy-MM-dd}");
            
            deploymentConfig.generatedGuidelines = guidelines.ToString();
            
            Debug.Log("[WindowsDeploymentGuidelines] Guidelines generated successfully");
        }
        
        private void ExportGuidelinesToFile()
        {
            if (string.IsNullOrEmpty(deploymentConfig.generatedGuidelines))
            {
                EditorUtility.DisplayDialog("No Guidelines", "Please generate guidelines first", "OK");
                return;
            }
            
            var fileName = $"Windows_Deployment_Guidelines_{DateTime.Now:yyyyMMdd_HHmmss}.md";
            var filePath = EditorUtility.SaveFilePanel("Save Guidelines", "", fileName, "md");
            
            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    File.WriteAllText(filePath, deploymentConfig.generatedGuidelines);
                    EditorUtility.DisplayDialog("Success", $"Guidelines saved to:\n{filePath}", "OK");
                    Debug.Log($"[WindowsDeploymentGuidelines] Guidelines exported to: {filePath}");
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("Error", $"Failed to save guidelines:\n{e.Message}", "OK");
                    Debug.LogError($"[WindowsDeploymentGuidelines] Export failed: {e.Message}");
                }
            }
        }
        
        private void ValidateDeploymentConfiguration()
        {
            var issues = new List<string>();
            var warnings = new List<string>();
            
            // Validate Unity version
            if (string.IsNullOrEmpty(deploymentConfig.targetUnityVersion))
            {
                issues.Add("Target Unity version is required");
            }
            else if (!deploymentConfig.targetUnityVersion.Contains("2021.3") && 
                     !deploymentConfig.targetUnityVersion.Contains("2022.3") &&
                     !deploymentConfig.targetUnityVersion.Contains("2023"))
            {
                warnings.Add("Unity version should be 2021.3 LTS or later for best compatibility");
            }
            
            // Validate Windows version
            if (string.IsNullOrEmpty(deploymentConfig.targetWindowsVersion))
            {
                issues.Add("Target Windows version is required");
            }
            
            // Validate architecture
            if (deploymentConfig.buildArchitecture == BuildArchitecture.x86)
            {
                warnings.Add("x86 architecture is not recommended for modern systems");
            }
            
            // Validate scripting backend
            if (deploymentConfig.scriptingBackend == ScriptingImplementation.Mono2x)
            {
                warnings.Add("Mono2x scripting backend is deprecated, consider IL2CPP");
            }
            
            // Show results
            if (issues.Count == 0 && warnings.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation Complete", "✅ Configuration is valid!", "OK");
            }
            else
            {
                var message = "Configuration validation results:\n\n";
                
                if (issues.Count > 0)
                {
                    message += "🔴 Issues that must be fixed:\n";
                    foreach (var issue in issues)
                    {
                        message += $"• {issue}\n";
                    }
                    message += "\n";
                }
                
                if (warnings.Count > 0)
                {
                    message += "🟡 Warnings (recommended to fix):\n";
                    foreach (var warning in warnings)
                    {
                        message += $"• {warning}\n";
                    }
                }
                
                EditorUtility.DisplayDialog("Validation Results", message, "OK");
            }
        }
    }
    
    [Serializable]
    public class WindowsDeploymentConfig
    {
        [Header("Target Configuration")]
        public string targetUnityVersion = "2022.3.0f1";
        public string targetWindowsVersion = "Windows 10/11";
        
        [Header("Build Settings")]
        public BuildArchitecture buildArchitecture = BuildArchitecture.x86_64;
        public ScriptingImplementation scriptingBackend = ScriptingImplementation.IL2CPP;
        public BuildCompression compressionLevel = BuildCompression.High;
        
        [Header("Generated Content")]
        [TextArea(10, 50)]
        public string generatedGuidelines;
    }
    
    public enum BuildArchitecture
    {
        x86,
        x86_64
    }
    
    public enum BuildCompression
    {
        None,
        Low,
        Medium,
        High
    }
}