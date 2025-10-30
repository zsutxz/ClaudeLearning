// Disable URP features since URP package is not installed
#define URP_NOT_AVAILABLE

using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if !URP_NOT_AVAILABLE && URP_12_0_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif
using CoinAnimation.Core;

namespace CoinAnimation.Tests
{
    /// <summary>
    /// Tests for URP Installation and Configuration (Task 2)
    /// Validates AC2: Universal Render Pipeline (URP) properly installed and configured
    /// </summary>
    public class URPConfigurationTest
    {
        private GameObject testGameObject;
        private URPConfigurationManager urpManager;
        
        [SetUp]
        public void Setup()
        {
            testGameObject = new GameObject("TestURPManager");
            urpManager = testGameObject.AddComponent<URPConfigurationManager>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
        }
        
        [Test]
        public void URPAssets_ArePresent()
        {
            // Arrange
            var requiredAssets = new[]
            {
                "Assets/Scripts/Settings/UniversalRenderPipelineAsset.asset",
                "Assets/Scripts/Settings/ForwardRenderer.asset",
                "Assets/Scripts/Settings/URPSettings_LowQuality.asset",
                "Assets/Scripts/Settings/URPSettings_MediumQuality.asset",
                "Assets/Scripts/Settings/URPSettings_HighQuality.asset"
            };
            
            // Act & Assert
            foreach (var assetPath in requiredAssets)
            {
                Assert.IsTrue(File.Exists(Path.Combine(Application.dataPath, "../", assetPath)),
                    $"Required URP asset '{assetPath}' does not exist");
            }
        }
        
        [Test]
        public void GraphicsSettings_HasURPConfigured()
        {
            // Arrange
            var graphicsSettingsPath = Path.Combine(Application.dataPath, "../ProjectSettings/GraphicsSettings.asset");
            
            // Act
            Assert.IsTrue(File.Exists(graphicsSettingsPath), "GraphicsSettings.asset does not exist");
            
            var graphicsContent = File.ReadAllText(graphicsSettingsPath);
            
            // Assert
            Assert.IsTrue(graphicsContent.Contains("UniversalRenderPipelineAsset"),
                "GraphicsSettings should reference UniversalRenderPipelineAsset");
            Assert.IsTrue(graphicsContent.Contains("Assets/Scripts/Settings/UniversalRenderPipelineAsset.asset"),
                "GraphicsSettings should reference the URP asset path");
        }
        
        [Test]
        public void URPManager_CanBeInstantiated()
        {
            // Arrange & Act
            var manager = testGameObject.GetComponent<URPConfigurationManager>();
            
            // Assert
            Assert.IsNotNull(manager, "URPConfigurationManager should be instantiatable");
        }
        
        [Test]
        public void URPManager_ValidateInstallation()
        {
            // Arrange
            var manager = testGameObject.GetComponent<URPConfigurationManager>();
            
            // Act
            var validationResult = manager.ValidateURPInstallation();
            
            // Assert
            Assert.IsNotNull(validationResult, "Validation result should not be null");
            Assert.IsTrue(validationResult.isURPInstalled || GraphicsSettings.renderPipelineAsset != null,
                "URP should be installed or configured");
        }
        
        [Test]
        public void URPManager_CanGetConfigurationInfo()
        {
            // Arrange
            var manager = testGameObject.GetComponent<URPConfigurationManager>();
            
            // Act
            var config = manager.GetCurrentConfiguration();
            
            // Assert
            Assert.IsNotNull(config, "Configuration info should not be null");
            Assert.IsTrue(config.renderScale > 0, "Render scale should be positive");
            Assert.IsTrue(config.msaaSampleCount >= 1, "MSAA sample count should be at least 1");
            Assert.IsTrue(config.shadowDistance >= 0, "Shadow distance should be non-negative");
        }
        
        [Test]
        public void URPManager_CanSetQualityLevel()
        {
            // Arrange
            var manager = testGameObject.GetComponent<URPConfigurationManager>();
            
            // Act & Assert
            Assert.DoesNotThrow(() => manager.SetQualityLevel(URPConfigurationManager.RenderQuality.Low),
                "Setting Low quality should not throw exception");
            Assert.DoesNotThrow(() => manager.SetQualityLevel(URPConfigurationManager.RenderQuality.Medium),
                "Setting Medium quality should not throw exception");
            Assert.DoesNotThrow(() => manager.SetQualityLevel(URPConfigurationManager.RenderQuality.High),
                "Setting High quality should not throw exception");
        }
        
        [Test]
        public void LowQualityAsset_HasCorrectSettings()
        {
            // Arrange
            var lowQualityAssetPath = Path.Combine(Application.dataPath, "../Assets/Scripts/Settings/URPSettings_LowQuality.asset");
            
            // Act
            Assert.IsTrue(File.Exists(lowQualityAssetPath), "Low quality URP asset should exist");
            
            var assetContent = File.ReadAllText(lowQualityAssetPath);
            
            // Assert
            Assert.IsTrue(assetContent.Contains("m_RenderScale: 0.5"),
                "Low quality should have 0.5 render scale");
            Assert.IsTrue(assetContent.Contains("m_SupportsHDR: 0"),
                "Low quality should not support HDR");
            Assert.IsTrue(assetContent.Contains("m_MSAA: 1"),
                "Low quality should have no MSAA");
            Assert.IsTrue(assetContent.Contains("m_AnyShadowsSupported: 0"),
                "Low quality should not support shadows");
        }
        
        [Test]
        public void MediumQualityAsset_HasCorrectSettings()
        {
            // Arrange
            var mediumQualityAssetPath = Path.Combine(Application.dataPath, "../Assets/Scripts/Settings/URPSettings_MediumQuality.asset");
            
            // Act
            Assert.IsTrue(File.Exists(mediumQualityAssetPath), "Medium quality URP asset should exist");
            
            var assetContent = File.ReadAllText(mediumQualityAssetPath);
            
            // Assert
            Assert.IsTrue(assetContent.Contains("m_RenderScale: 0.75"),
                "Medium quality should have 0.75 render scale");
            Assert.IsTrue(assetContent.Contains("m_SupportsHDR: 1"),
                "Medium quality should support HDR");
            Assert.IsTrue(assetContent.Contains("m_MSAA: 1"),
                "Medium quality should have no MSAA");
            Assert.IsTrue(assetContent.Contains("m_AnyShadowsSupported: 1"),
                "Medium quality should support shadows");
        }
        
        [Test]
        public void HighQualityAsset_HasCorrectSettings()
        {
            // Arrange
            var highQualityAssetPath = Path.Combine(Application.dataPath, "../Assets/Scripts/Settings/URPSettings_HighQuality.asset");
            
            // Act
            Assert.IsTrue(File.Exists(highQualityAssetPath), "High quality URP asset should exist");
            
            var assetContent = File.ReadAllText(highQualityAssetPath);
            
            // Assert
            Assert.IsTrue(assetContent.Contains("m_RenderScale: 1"),
                "High quality should have 1.0 render scale");
            Assert.IsTrue(assetContent.Contains("m_SupportsHDR: 1"),
                "High quality should support HDR");
            Assert.IsTrue(assetContent.Contains("m_MSAA: 2"),
                "High quality should have 2x MSAA");
            Assert.IsTrue(assetContent.Contains("m_AnyShadowsSupported: 1"),
                "High quality should support shadows");
        }
        
        [Test]
        public void PerformanceMonitor_CanGetMetrics()
        {
            // Arrange
            var performanceMonitor = testGameObject.AddComponent<PerformanceMonitor>();
            
            // Act
            var metrics = performanceMonitor.GetCurrentMetrics();
            
            // Assert
            Assert.IsNotNull(metrics, "Performance metrics should not be null");
            Assert.IsTrue(metrics.currentFrameRate > 0, "Current frame rate should be positive");
            Assert.IsTrue(metrics.memoryUsage >= 0, "Memory usage should be non-negative");
        }
        
        [Test]
        public void ForwardRendererAsset_HasCorrectConfiguration()
        {
            // Arrange
            var rendererPath = Path.Combine(Application.dataPath, "../Assets/Scripts/Settings/ForwardRenderer.asset");
            
            // Act
            Assert.IsTrue(File.Exists(rendererPath), "Forward renderer asset should exist");
            
            var rendererContent = File.ReadAllText(rendererPath);
            
            // Assert
            Assert.IsTrue(rendererContent.Contains("ForwardRenderer"),
                "Asset should be configured as ForwardRenderer");
            Assert.IsTrue(rendererContent.Contains("m_UseNativeRenderPass: 0"),
                "Native render pass should be disabled for compatibility");
        }
        
        [Test]
        public void URPPackage_IsInManifest()
        {
            // Arrange
            var manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            
            // Act
            Assert.IsTrue(File.Exists(manifestPath), "manifest.json should exist");
            
            var manifestContent = File.ReadAllText(manifestPath);
            
            // Assert
            Assert.IsTrue(manifestContent.Contains("com.unity.render-pipelines.universal"),
                "URP package should be in manifest.json");
        }
        
        [Test]
        public void QualitySettings_HaveURPConfiguration()
        {
            // Arrange
            var qualitySettingsPath = Path.Combine(Application.dataPath, "../ProjectSettings/QualitySettings.asset");
            
            // Act
            Assert.IsTrue(File.Exists(qualitySettingsPath), "QualitySettings.asset should exist");
            
            var qualityContent = File.ReadAllText(qualitySettingsPath);
            
            // Assert
            Assert.IsTrue(qualityContent.Contains("m_CurrentQuality: 2"),
                "Current quality should be set to Medium (index 2)");
        }
    }
}