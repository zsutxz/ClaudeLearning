using NUnit.Framework;
using UnityEngine;
using System.IO;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoinAnimation.Tests
{
    /// <summary>
    /// Tests for Unity Environment Setup and Configuration (Task 1)
    /// Validates AC1: Unity 2021.3 LTS+ compatibility, project structure, and configuration
    /// </summary>
    public class UnityEnvironmentValidatorTest
    {
        [Test]
        public void UnityVersion_IsCompatibleWithRequirements()
        {
            // Arrange
            var unityVersion = Application.unityVersion;
            var versionParts = unityVersion.Split('.');
            var majorVersion = int.Parse(versionParts[0]);
            var minorVersion = versionParts.Length > 1 ? int.Parse(versionParts[1]) : 0;
            
            // Act & Assert
            if (majorVersion > 2021)
            {
                Assert.Pass($"Unity {unityVersion} is compatible (newer than 2021.3 LTS)");
            }
            else if (majorVersion == 2021)
            {
                Assert.IsTrue(minorVersion >= 3, 
                    $"Unity {unityVersion} is not compatible. Requires 2021.3 LTS or later");
                Assert.Pass($"Unity {unityVersion} is compatible (2021.3 LTS or later)");
            }
            else
            {
                Assert.Fail($"Unity {unityVersion} is not compatible. Requires 2021.3 LTS or later");
            }
        }
        
        [Test]
        public void RequiredDirectoryStructure_Exists()
        {
            // Arrange
            var requiredDirectories = new[]
            {
                "Assets/Scripts/Core",
                "Assets/Scripts/Animation",
                "Assets/Scripts/Physics", 
                "Assets/Scripts/Tests",
                "Assets/Scripts/Settings"
            };
            
            // Act & Assert
            foreach (var directory in requiredDirectories)
            {
                Assert.IsTrue(Directory.Exists(Path.Combine(Application.dataPath, "..", directory)),
                    $"Required directory '{directory}' does not exist");
            }
        }
        
        [Test]
        public void AssemblyDefinitionFiles_ArePresent()
        {
            // Arrange
            var requiredAsmdefs = new[]
            {
                "Assets/Scripts/Core/CoinAnimation.Core.asmdef",
                "Assets/Scripts/Animation/CoinAnimation.Animation.asmdef",
                "Assets/Scripts/Physics/CoinAnimation.Physics.asmdef",
                "Assets/Scripts/Tests/CoinAnimation.Tests.asmdef",
                "Assets/Scripts/Settings/CoinAnimation.Settings.asmdef"
            };
            
            // Act & Assert
            foreach (var asmdef in requiredAsmdefs)
            {
                Assert.IsTrue(File.Exists(Path.Combine(Application.dataPath, "..", asmdef)),
                    $"Assembly definition file '{asmdef}' does not exist");
            }
        }
        
        [Test]
        public void ManifestJson_ContainsRequiredPackages()
        {
            // Arrange
            var manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            Assert.IsTrue(File.Exists(manifestPath), "manifest.json does not exist");
            
            var manifestContent = File.ReadAllText(manifestPath);
            
            // Act & Assert
            Assert.IsTrue(manifestContent.Contains("com.unity.render-pipelines.universal"),
                "manifest.json missing URP package dependency");
            Assert.IsTrue(manifestContent.Contains("com.unity.test-framework"),
                "manifest.json missing Test Framework package dependency");
            Assert.IsTrue(manifestContent.Contains("com.unity.textmeshpro"),
                "manifest.json missing TextMeshPro package dependency");
        }
        
        [Test]
        public void ProjectSettings_AreConfigured()
        {
            // Arrange
            var projectSettingsPath = Path.Combine(Application.dataPath, "../ProjectSettings/ProjectSettings.asset");
            
            // Act & Assert
            Assert.IsTrue(File.Exists(projectSettingsPath), "ProjectSettings.asset does not exist");
            
            var settingsContent = File.ReadAllText(projectSettingsPath);
            Assert.IsTrue(settingsContent.Contains("iPhoneStrippingLevel: 0"),
                "Project settings not configured for optimal performance");
        }
        
        [Test]
        public void QualitySettings_AreConfigured()
        {
            // Arrange
            var qualitySettingsPath = Path.Combine(Application.dataPath, "../ProjectSettings/QualitySettings.asset");
            
            // Act & Assert
            Assert.IsTrue(File.Exists(qualitySettingsPath), "QualitySettings.asset does not exist");
            
            var qualityContent = File.ReadAllText(qualitySettingsPath);
            Assert.IsTrue(qualityContent.Contains("m_CurrentQuality: 2"),
                "Quality settings not configured to Medium (default optimal setting)");
        }
        
        [Test]
        public void CoreInterfaces_AreDefined()
        {
            // Arrange
            var requiredInterfaces = new[]
            {
                "Assets/Scripts/Core/ICoinAnimationManager.cs",
                "Assets/Scripts/Core/ICoinObjectPool.cs",
                "Assets/Scripts/Physics/IMagneticCollectionController.cs"
            };
            
            // Act & Assert
            foreach (var interfaceFile in requiredInterfaces)
            {
                Assert.IsTrue(File.Exists(Path.Combine(Application.dataPath, interfaceFile)),
                    $"Required interface file '{interfaceFile}' does not exist");
            }
        }
        
        [Test]
        public void EnvironmentValidator_CanBeInstantiated()
        {
            // Arrange & Act
            var validatorGO = new GameObject("TestValidator");
            
            // Assert
            Assert.IsNotNull(validatorGO, "Environment validator GameObject should be created");
            
            // Cleanup
           UnityEngine.Object.DestroyImmediate(validatorGO);
        }
        
        [Test]
        public void CoinAnimationConfiguration_HasRequiredProperties()
        {
            // Arrange
            var config = new CoinAnimation.Core.CoinAnimationConfiguration();
            
            // Act & Assert
            Assert.IsTrue(config.maxConcurrentAnimations > 0, "maxConcurrentAnimations should be positive");
            Assert.IsTrue(config.targetFrameRate > 0, "targetFrameRate should be positive");
            Assert.IsTrue(config.defaultAnimationDuration > 0, "defaultAnimationDuration should be positive");
        }
        
        [Test]
        public void PerformanceMetrics_HasRequiredFields()
        {
            // Arrange
            var metrics = new CoinAnimation.Core.PerformanceMetrics();
            
            // Act & Assert
            Assert.IsTrue(metrics.timestamp != default(DateTime), "timestamp should be set");
        }
        
        [Test]
        public void MagneticFieldConfiguration_HasValidDefaults()
        {
            // Arrange
            var config = new CoinAnimation.Physics.MagneticFieldConfiguration();
            
            // Act & Assert
            Assert.IsTrue(config.fieldStrength > 0, "fieldStrength should be positive by default");
            Assert.IsTrue(config.fieldRadius > 0, "fieldRadius should be positive by default");
            Assert.IsTrue(config.maxConcurrentCoins > 0, "maxConcurrentCoins should be positive by default");
        }
    }
}