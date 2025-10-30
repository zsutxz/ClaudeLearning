using NUnit.Framework;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoinAnimation.Tests
{
    /// <summary>
    /// Tests for project configuration and settings
    /// Validates Unity project setup meets all acceptance criteria
    /// </summary>
    public class ProjectConfigurationTest
    {
        [Test]
        public void ScriptingBackend_IsConfiguredCorrectly()
        {
            // Arrange
#if UNITY_EDITOR
            var scriptingBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone);
            
            // Act & Assert
            Assert.IsTrue(scriptingBackend == ScriptingImplementation.IL2CPP || 
                         scriptingBackend == ScriptingImplementation.Mono2x,
                $"Scripting backend {scriptingBackend} should be configured for optimal performance");
#endif
        }
        
        [Test]
        public void ApiCompatibilityLevel_IsConfiguredCorrectly()
        {
            // Arrange
#if UNITY_EDITOR
            var apiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone);
            
            // Act & Assert
            Assert.IsTrue(apiCompatibilityLevel == ApiCompatibilityLevel.NET_Unity_4_8 ||
                         apiCompatibilityLevel == ApiCompatibilityLevel.NET_Standard_2_0,
                $"API compatibility level {apiCompatibilityLevel} should be configured for compatibility");
#endif
        }
        
        [Test]
        public void ProjectStructure_FollowsNamingConventions()
        {
            // Arrange
            var projectRoot = Path.Combine(Application.dataPath, "..");
            
            // Act & Assert
            Assert.IsTrue(Directory.Exists(Path.Combine(projectRoot, "Project")),
                "Project directory structure should follow Unity conventions");
            Assert.IsTrue(Directory.Exists(Path.Combine(projectRoot, "Project/Assets")),
                "Assets directory should exist within Project folder");
        }
        
        [Test]
        public void AssemblyDefinitions_HaveCorrectReferences()
        {
            // Arrange
            var coreAsmdefPath = Path.Combine(Application.dataPath, "Scripts/Core/CoinAnimation.Core.asmdef");
            
            // Act
            var coreAsmdefContent = File.ReadAllText(coreAsmdefPath);
            
            // Assert
            Assert.IsTrue(coreAsmdefContent.Contains("\"name\": \"CoinAnimation.Core\""),
                "Core assembly definition should have correct name");
            Assert.IsTrue(coreAsmdefContent.Contains("\"rootNamespace\": \"CoinAnimation.Core\""),
                "Core assembly definition should have correct namespace");
        }
        
        [Test]
        public void AnimationAssemblyDefinition_HasRequiredReferences()
        {
            // Arrange
            var animationAsmdefPath = Path.Combine(Application.dataPath, "Scripts/Animation/CoinAnimation.Animation.asmdef");
            
            // Act
            var animationAsmdefContent = File.ReadAllText(animationAsmdefPath);
            
            // Assert
            Assert.IsTrue(animationAsmdefContent.Contains("CoinAnimation.Core"),
                "Animation assembly should reference Core assembly");
            Assert.IsTrue(animationAsmdefContent.Contains("Unity.Mathematics"),
                "Animation assembly should reference Unity.Mathematics");
        }
        
        [Test]
        public void PhysicsAssemblyDefinition_HasRequiredReferences()
        {
            // Arrange
            var physicsAsmdefPath = Path.Combine(Application.dataPath, "Scripts/Physics/CoinAnimation.Physics.asmdef");
            
            // Act
            var physicsAsmdefContent = File.ReadAllText(physicsAsmdefPath);
            
            // Assert
            Assert.IsTrue(physicsAsmdefContent.Contains("CoinAnimation.Core"),
                "Physics assembly should reference Core assembly");
            Assert.IsTrue(physicsAsmdefContent.Contains("Unity.Mathematics"),
                "Physics assembly should reference Unity.Mathematics");
        }
        
        [Test]
        public void TestAssemblyDefinition_HasTestReferences()
        {
            // Arrange
            var testAsmdefPath = Path.Combine(Application.dataPath, "Scripts/Tests/CoinAnimation.Tests.asmdef");
            
            // Act
            var testAsmdefContent = File.ReadAllText(testAsmdefPath);
            
            // Assert
            Assert.IsTrue(testAsmdefContent.Contains("Unity.TestRunner"),
                "Test assembly should reference Unity Test Runner");
            Assert.IsTrue(testAsmdefContent.Contains("UNITY_INCLUDE_TESTS"),
                "Test assembly should have UNITY_INCLUDE_TESTS define constraint");
        }
        
        [Test]
        public void PackageManifest_HasCorrectStructure()
        {
            // Arrange
            var manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            var manifestContent = File.ReadAllText(manifestPath);
            
            // Act & Assert
            Assert.IsTrue(manifestContent.Contains("\"dependencies\""),
                "Manifest should have dependencies section");
            Assert.IsTrue(manifestContent.Contains("\"scopedRegistries\""),
                "Manifest should have scoped registries for OpenUPM");
        }
        
        [Test]
        public void EnvironmentValidator_ImplementsCorrectFunctionality()
        {
            // Arrange
            var validatorPath = Path.Combine(Application.dataPath, "Scripts/Core/UnityEnvironmentValidator.cs");
            var validatorContent = File.ReadAllText(validatorPath);
            
            // Act & Assert
            Assert.IsTrue(validatorContent.Contains("ValidateUnityVersion"),
                "Validator should implement Unity version validation");
            Assert.IsTrue(validatorContent.Contains("ValidateURPInstallation"),
                "Validator should implement URP installation validation");
            Assert.IsTrue(validatorContent.Contains("ValidateDOTweenIntegration"),
                "Validator should implement DOTween integration validation");
            Assert.IsTrue(validatorContent.Contains("ValidateProjectStructure"),
                "Validator should implement project structure validation");
        }
        
        [Test]
        public void CoreInterfaceDefinitions_HaveCorrectSignatures()
        {
            // Arrange
            var managerInterfacePath = Path.Combine(Application.dataPath, "Scripts/Core/ICoinAnimationManager.cs");
            var managerInterfaceContent = File.ReadAllText(managerInterfacePath);
            
            // Act & Assert
            Assert.IsTrue(managerInterfaceContent.Contains("void Initialize"),
                "ICoinAnimationManager should have Initialize method");
            Assert.IsTrue(managerInterfaceContent.Contains("Guid StartCoinAnimation"),
                "ICoinAnimationManager should have StartCoinAnimation method");
            Assert.IsTrue(managerInterfaceContent.Contains("PerformanceMetrics GetPerformanceMetrics"),
                "ICoinAnimationManager should have GetPerformanceMetrics method");
        }
        
        [Test]
        public void ObjectPoolInterface_HasCorrectMethods()
        {
            // Arrange
            var poolInterfacePath = Path.Combine(Application.dataPath, "Scripts/Core/ICoinObjectPool.cs");
            var poolInterfaceContent = File.ReadAllText(poolInterfacePath);
            
            // Act & Assert
            Assert.IsTrue(poolInterfaceContent.Contains("GameObject GetCoin"),
                "ICoinObjectPool should have GetCoin method");
            Assert.IsTrue(poolInterfaceContent.Contains("void ReturnCoin"),
                "ICoinObjectPool should have ReturnCoin method");
            Assert.IsTrue(poolInterfaceContent.Contains("void PreWarmPool"),
                "ICoinObjectPool should have PreWarmPool method");
        }
        
        [Test]
        public void MagneticControllerInterface_HasCorrectMethods()
        {
            // Arrange
            var magneticInterfacePath = Path.Combine(Application.dataPath, "Scripts/Physics/IMagneticCollectionController.cs");
            var magneticInterfaceContent = File.ReadAllText(magneticInterfacePath);
            
            // Act & Assert
            Assert.IsTrue(magneticInterfaceContent.Contains("void InitializeMagneticField"),
                "IMagneticCollectionController should have InitializeMagneticField method");
            Assert.IsTrue(magneticInterfaceContent.Contains("void ApplyMagneticForce"),
                "IMagneticCollectionController should have ApplyMagneticForce method");
            Assert.IsTrue(magneticInterfaceContent.Contains("Vector3[] GenerateSpiralPath"),
                "IMagneticCollectionController should have GenerateSpiralPath method");
        }
    }
}