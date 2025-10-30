using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
namespace CoinAnimation.Core
{
    /// <summary>
    /// Unity Environment Setup Validator
    /// Validates that all required Unity environment configurations are properly set up
    /// </summary>
    public class UnityEnvironmentValidator : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<ValidationResult> validationResults = new List<ValidationResult>();
        
        [MenuItem("Coin Animation/Validate Environment")]
        public static void ShowWindow()
        {
            GetWindow<UnityEnvironmentValidator>("Environment Validator");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Coin Animation Environment Validator", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Run Validation"))
            {
                RunValidation();
            }
            
            EditorGUILayout.Space();
            
            if (validationResults.Count > 0)
            {
                EditorGUILayout.LabelField("Validation Results:", EditorStyles.boldLabel);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                foreach (var result in validationResults)
                {
                    var originalColor = GUI.color;
                    GUI.color = result.passed ? Color.green : Color.red;
                    
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"✓ {result.testName}", result.passed ? EditorStyles.boldLabel : EditorStyles.label);
                    EditorGUILayout.LabelField(result.message, EditorStyles.wordWrappedLabel);
                    EditorGUILayout.EndVertical();
                    
                    GUI.color = originalColor;
                }
                
                EditorGUILayout.EndScrollView();
            }
        }
        
        private void RunValidation()
        {
            validationResults.Clear();
            
            // AC1: Unity Version Compatibility
            ValidateUnityVersion();
            
            // AC2: URP Installation and Configuration
            ValidateURPInstallation();
            
            // AC3: DOTween Integration
            ValidateDOTweenIntegration();
            
            // AC4: Project Structure
            ValidateProjectStructure();
            
            // AC5: Package Manager Configuration
            ValidatePackageConfiguration();
            
            // AC6: Build Settings Optimization
            ValidateBuildSettings();
            
            // AC7: Development Environment Validation
            ValidateDevelopmentEnvironment();
        }
        
        private void ValidateUnityVersion()
        {
            var unityVersion = Application.unityVersion;
            var isCompatible = unityVersion.StartsWith("2021.3.") || 
                             unityVersion.StartsWith("2022.3.") || 
                             unityVersion.StartsWith("2023.");
            
            validationResults.Add(new ValidationResult
            {
                testName = "Unity Version Compatibility (AC1)",
                passed = isCompatible,
                message = isCompatible ? 
                    $"Unity {unityVersion} is compatible (2021.3 LTS+)" : 
                    $"Unity {unityVersion} is not compatible. Requires 2021.3 LTS or later"
            });
        }
        
        private void ValidateURPInstallation()
        {
            var urpInstalled = false;
            try
            {
                var urpAsset = GraphicsSettings.renderPipelineAsset;
                urpInstalled = urpAsset != null && urpAsset.GetType().Name.Contains("UniversalRenderPipelineAsset");
            }
            catch
            {
                urpInstalled = false;
            }
            
            validationResults.Add(new ValidationResult
            {
                testName = "URP Installation (AC2)",
                passed = urpInstalled,
                message = urpInstalled ? 
                    "Universal Render Pipeline is properly installed and configured" : 
                    "Universal Render Pipeline is not installed or configured"
            });
        }
        
        private void ValidateDOTweenIntegration()
        {
            var dotweenInstalled = false;
            try
            {
                var dotweenType = Type.GetType("DG.Tweening.DOTween, DOTween");
                dotweenInstalled = dotweenType != null;
            }
            catch
            {
                dotweenInstalled = false;
            }
            
            validationResults.Add(new ValidationResult
            {
                testName = "DOTween Integration (AC3)",
                passed = dotweenInstalled,
                message = dotweenInstalled ? 
                    "DOTween is properly integrated and accessible" : 
                    "DOTween is not installed or integrated"
            });
        }
        
        private void ValidateProjectStructure()
        {
            var requiredDirectories = new[]
            {
                "Assets/Scripts/Core",
                "Assets/Scripts/Animation", 
                "Assets/Scripts/Physics",
                "Assets/Scripts/Tests",
                "Assets/Scripts/Settings"
            };
            
            var allDirectoriesExist = true;
            var missingDirectories = new List<string>();
            
            foreach (var dir in requiredDirectories)
            {
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    allDirectoriesExist = false;
                    missingDirectories.Add(dir);
                }
            }
            
            validationResults.Add(new ValidationResult
            {
                testName = "Project Structure (AC4)",
                passed = allDirectoriesExist,
                message = allDirectoriesExist ? 
                    "All required directory structures are present" : 
                    $"Missing directories: {string.Join(", ", missingDirectories)}"
            });
        }
        
        private void ValidatePackageConfiguration()
        {
            var manifestPath = "Packages/manifest.json";
            var manifestExists = System.IO.File.Exists(manifestPath);
            
            if (!manifestExists)
            {
                validationResults.Add(new ValidationResult
                {
                    testName = "Package Manager Configuration (AC5)",
                    passed = false,
                    message = "manifest.json file not found"
                });
                return;
            }
            
            var manifestContent = System.IO.File.ReadAllText(manifestPath);
            var hasURP = manifestContent.Contains("com.unity.render-pipelines.universal");
            var hasDOTween = manifestContent.Contains("com.unityDOTween");
            var hasTestFramework = manifestContent.Contains("com.unity.test-framework");
            
            var allPackagesConfigured = hasURP && hasDOTween && hasTestFramework;
            
            validationResults.Add(new ValidationResult
            {
                testName = "Package Manager Configuration (AC5)",
                passed = allPackagesConfigured,
                message = allPackagesConfigured ? 
                    "All required packages are configured in manifest.json" : 
                    $"Missing packages - URP: {hasURP}, DOTween: {hasDOTween}, Test Framework: {hasTestFramework}"
            });
        }
        
        private void ValidateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            var hasValidScenes = scenes.Length > 0;

            validationResults.Add(new ValidationResult
            {
                testName = "Build Settings Optimization (AC6)",
                passed = hasValidScenes,
                message = hasValidScenes ?
                    $"Build settings configured with {scenes.Length} scene(s)" :
                    "No scenes configured in build settings"
            });
        }
        
        private void ValidateDevelopmentEnvironment()
        {
            var scriptingBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone);
            var apiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone);
            
            var isOptimal = scriptingBackend == ScriptingImplementation.IL2CPP && 
                           apiCompatibilityLevel == ApiCompatibilityLevel.NET_Unity_4_8;
            
            validationResults.Add(new ValidationResult
            {
                testName = "Development Environment Validation (AC7)",
                passed = isOptimal,
                message = isOptimal ? 
                    $"Development environment optimized - Backend: {scriptingBackend}, API: {apiCompatibilityLevel}" : 
                    $"Suboptimal configuration - Backend: {scriptingBackend}, API: {apiCompatibilityLevel}"
            });
        }
    }
    
    [Serializable]
    public class ValidationResult
    {
        public string testName;
        public bool passed;
        public string message;
    }
}
#endif