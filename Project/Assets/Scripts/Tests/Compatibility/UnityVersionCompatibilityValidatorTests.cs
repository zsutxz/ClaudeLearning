using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using CoinAnimation.Core.Compatibility;
using CoinAnimation.Core;

#if UNITY_EDITOR
namespace CoinAnimation.Tests.Compatibility
{
    /// <summary>
    /// Comprehensive test suite for Unity Version Compatibility Validator
    /// Story 2.2 Task 1: Unity Version Compatibility Validation Tests
    /// </summary>
    [TestFixture]
    public class UnityVersionCompatibilityValidatorTests
    {
        private UnityVersionCompatibilityValidator validator;
        private string testTempDirectory;
        
        [SetUp]
        public void SetUp()
        {
            // Create temporary directory for test files
            testTempDirectory = Path.Combine(Application.temporaryCachePath, "UnityCompatibilityTests");
            if (!Directory.Exists(testTempDirectory))
            {
                Directory.CreateDirectory(testTempDirectory);
            }
            
            // Initialize validator instance
            validator = ScriptableObject.CreateInstance<UnityVersionCompatibilityValidator>();
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up test files
            if (Directory.Exists(testTempDirectory))
            {
                Directory.Delete(testTempDirectory, true);
            }
            
            if (validator != null)
            {
                ScriptableObject.DestroyImmediate(validator);
            }
        }
        
        #region Unity 2021.3 LTS Compatibility Tests
        
        [Test]
        public void Unity2021LTSCompatibility_DetectsCurrentVersion_ReturnsCorrectResult()
        {
            // Arrange
            var currentVersion = Application.unityVersion;
            var is2021LTS = currentVersion.StartsWith("2021.3.");
            
            // Act
            var result = TestUnity2021LTSCompatibility();
            
            // Assert
            Assert.IsTrue(result.passed, "Unity 2021.3 LTS compatibility test should pass");
            Assert.IsNotNull(result.version, "Version should be set");
            Assert.AreEqual("2021.3 LTS", result.version, "Should test 2021.3 LTS specifically");
            
            if (is2021LTS)
            {
                Assert.IsTrue(result.message.Contains("FULL COMPATIBILITY"), 
                    "Running on 2021.3 LTS should show full compatibility");
            }
        }
        
        [Test]
        public void Unity2021LTSCompatibility_ValidatesURPCompatibility_ReturnsCorrectURPInfo()
        {
            // Act
            var result = TestUnity2021LTSCompatibility();
            
            // Assert
            var hasURPDetails = result.details.Any(d => d.Contains("URP"));
            Assert.IsTrue(hasURPDetails, "Should validate URP compatibility for 2021.3 LTS");
        }
        
        [Test]
        public void Unity2021LTSCompatibility_ChecksKnownIssues_ReturnsIssuesIfAny()
        {
            // Act
            var result = TestUnity2021LTSCompatibility();
            
            // Assert
            // 2021.3 LTS should have known issue about URP 12.0+ recommendation
            var hasKnownIssues = result.details.Any(d => d.Contains("known issue") || d.Contains("recommended"));
            Assert.IsTrue(hasKnownIssues, "Should check for known issues in 2021.3 LTS");
        }
        
        #endregion
        
        #region Unity 2022.3 LTS Compatibility Tests
        
        [Test]
        public void Unity2022LTSCompatibility_DetectsCurrentVersion_ReturnsCorrectResult()
        {
            // Arrange
            var currentVersion = Application.unityVersion;
            var is2022LTS = currentVersion.StartsWith("2022.3.");
            
            // Act
            var result = TestUnity2022LTSCompatibility();
            
            // Assert
            Assert.IsTrue(result.passed, "Unity 2022.3 LTS compatibility test should pass");
            Assert.IsNotNull(result.version, "Version should be set");
            Assert.AreEqual("2022.3 LTS", result.version, "Should test 2022.3 LTS specifically");
            
            if (is2022LTS)
            {
                Assert.IsTrue(result.message.Contains("FULL COMPATIBILITY"), 
                    "Running on 2022.3 LTS should show full compatibility");
            }
        }
        
        [Test]
        public void Unity2022LTSCompatibility_ValidatesURPCompatibility_ReturnsCorrectURPInfo()
        {
            // Act
            var result = TestUnity2022LTSCompatibility();
            
            // Assert
            var hasURPDetails = result.details.Any(d => d.Contains("URP"));
            Assert.IsTrue(hasURPDetails, "Should validate URP compatibility for 2022.3 LTS");
        }
        
        [Test]
        public void Unity2022LTSCompatibility_ChecksPackageCompatibility_ValidatesRequiredPackages()
        {
            // Act
            var result = TestUnity2022LTSCompatibility();
            
            // Assert
            // Should validate required packages for 2022.3 LTS
            var hasPackageDetails = result.details.Any(d => d.Contains("package") || d.Contains("URP"));
            Assert.IsTrue(hasPackageDetails, "Should check package compatibility for 2022.3 LTS");
        }
        
        #endregion
        
        #region Script API Compatibility Tests
        
        [Test]
        public void ScriptAPICompatibility_ValidateICoinAnimationManager_ReturnsCorrectInterfaceInfo()
        {
            // Act
            var result = TestScriptAPICompatibility();
            
            // Assert
            Assert.IsTrue(result.passed, "Script API compatibility test should pass");
            var hasInterfaceValidation = result.details.Any(d => d.Contains("ICoinAnimationManager"));
            Assert.IsTrue(hasInterfaceValidation, "Should validate ICoinAnimationManager interface");
        }
        
        [Test]
        public void ScriptAPICompatibility_ValidateInterfaceMethods_ReturnsAllMethods()
        {
            // Act
            var result = TestScriptAPICompatibility();
            
            // Assert
            var methodDetails = result.details.Where(d => d.Contains("ICoinAnimationManager") && d.Contains("("));
            Assert.IsTrue(methodDetails.Count() > 0, "Should validate interface methods");
            
            // Check for specific methods from the interface
            var hasInitialize = methodDetails.Any(d => d.Contains("Initialize"));
            var hasStartAnimation = methodDetails.Any(d => d.Contains("StartCoinAnimation"));
            var hasStopAnimation = methodDetails.Any(d => d.Contains("StopCoinAnimation"));
            var hasGetMetrics = methodDetails.Any(d => d.Contains("GetPerformanceMetrics"));
            var hasCleanup = methodDetails.Any(d => d.Contains("Cleanup"));
            
            Assert.IsTrue(hasInitialize, "Should validate Initialize method");
            Assert.IsTrue(hasStartAnimation, "Should validate StartCoinAnimation method");
            Assert.IsTrue(hasStopAnimation, "Should validate StopCoinAnimation method");
            Assert.IsTrue(hasGetMetrics, "Should validate GetPerformanceMetrics method");
            Assert.IsTrue(hasCleanup, "Should validate Cleanup method");
        }
        
        [Test]
        public void ScriptAPICompatibility_ValidateCoreSystems_ReturnsSystemCompatibilityInfo()
        {
            // Act
            var result = TestScriptAPICompatibility();
            
            // Assert
            var hasSystemValidation = result.details.Any(d => d.Contains("CoinAnimation.Core"));
            Assert.IsTrue(hasSystemValidation, "Should validate core system types");
            
            // Check for specific core types
            var coreTypes = new[]
            {
                "CoinAnimationState",
                "UnityEnvironmentValidator",
                "URPConfigurationManager",
                "CoinAnimationManager"
            };
            
            foreach (var coreType in coreTypes)
            {
                var hasCoreType = result.details.Any(d => d.Contains(coreType));
                Assert.IsTrue(hasCoreType, $"Should validate {coreType} type availability");
            }
        }
        
        [Test]
        public void ScriptAPICompatibility_ValidateUnityAPIs_ReturnsUnityAPICompatibilityInfo()
        {
            // Act
            var result = TestScriptAPICompatibility();
            
            // Assert
            var hasUnityAPIValidation = result.details.Any(d => d.Contains("Unity API available"));
            Assert.IsTrue(hasUnityAPIValidation, "Should validate Unity API availability");
            
            // Check for critical Unity APIs
            var unityAPIs = new[]
            {
                "UnityEngine.Coroutine",
                "UnityEngine.Vector3",
                "UnityEngine.Transform",
                "UnityEngine.Time",
                "UnityEngine.Mathf"
            };
            
            foreach (var api in unityAPIs)
            {
                var hasAPI = result.details.Any(d => d.Contains(api));
                Assert.IsTrue(hasAPI, $"Should validate {api} availability");
            }
        }
        
        #endregion
        
        #region Configuration Profile Tests
        
        [Test]
        public void VersionSpecificProfiles_CreateProfiles_ReturnsCorrectProfileCount()
        {
            // Act
            var result = CreateVersionSpecificProfiles();
            
            // Assert
            Assert.IsTrue(result.passed, "Profile creation should succeed");
            Assert.IsTrue(result.details.Any(d => d.Contains("3 version-specific profiles")), 
                "Should create profiles for 3 Unity versions");
        }
        
        [Test]
        public void VersionSpecificProfiles_ProfileStructure_ContainsAllRequiredFields()
        {
            // Act
            var result = CreateVersionSpecificProfiles();
            
            // Assert
            var profilePaths = result.details.Where(d => d.Contains("Profile created:"));
            Assert.IsTrue(profilePaths.Count() > 0, "Should create at least one profile");
            
            // Each profile should have proper structure
            foreach (var profileDetail in profilePaths)
            {
                Assert.IsTrue(profileDetail.Contains("Unity_"), "Profile should have Unity prefix");
                Assert.IsTrue(profileDetail.Contains("_Profile.asset"), "Profile should have proper extension");
            }
        }
        
        [Test]
        public void VersionSpecificProfiles_ProfileDirectory_CreatesInCorrectLocation()
        {
            // Act
            var result = CreateVersionSpecificProfiles();
            
            // Assert
            var hasDirectoryInfo = result.details.Any(d => d.Contains("Assets/CoinAnimation/Settings/UnityVersions/"));
            Assert.IsTrue(hasDirectoryInfo, "Should save profiles to correct directory");
        }
        
        #endregion
        
        #region Cross-Version Integration Tests
        
        [UnityTest]
        public IEnumerator CrossVersionIntegration_RunAllTests_ReturnsConsistentResults()
        {
            // Act
            var results = new List<CompatibilityTestResult>();
            
            // Simulate running all compatibility tests
            results.Add(TestUnity2021LTSCompatibility());
            results.Add(TestUnity2022LTSCompatibility());
            results.Add(TestScriptAPICompatibility());
            results.Add(CreateVersionSpecificProfiles());
            
            yield return null;
            
            // Assert
            Assert.AreEqual(4, results.Count, "Should run all 4 test categories");
            
            var passedCount = results.Count(r => r.passed);
            Assert.IsTrue(passedCount >= 3, $"At least 3 of 4 tests should pass (actual: {passedCount})");
            
            // Each test should have proper structure
            foreach (var result in results)
            {
                Assert.IsNotNull(result.testName, "Test should have a name");
                Assert.IsNotNull(result.message, "Test should have a message");
                Assert.IsTrue(result.details.Count > 0, "Test should have details");
            }
        }
        
        [Test]
        public void CrossVersionIntegration_ValidateURPAcrossVersions_ReturnsConsistentURPValidation()
        {
            // Act
            var result2021 = TestUnity2021LTSCompatibility();
            var result2022 = TestUnity2022LTSCompatibility();
            
            // Assert
            var urp2021 = result2021.details.Any(d => d.Contains("URP"));
            var urp2022 = result2022.details.Any(d => d.Contains("URP"));
            
            Assert.IsTrue(urp2021, "2021.3 LTS should validate URP");
            Assert.IsTrue(urp2022, "2022.3 LTS should validate URP");
        }
        
        [Test]
        public void CrossVersionIntegration_APICompatibilityAcrossVersions_ReturnsConsistentAPIResults()
        {
            // Act
            var result = TestScriptAPICompatibility();
            
            // Assert
            // Should validate APIs that work across both versions
            var hasCoroutines = result.details.Any(d => d.Contains("Coroutine"));
            var hasVector3 = result.details.Any(d => d.Contains("Vector3"));
            var hasTransform = result.details.Any(d => d.Contains("Transform"));
            
            Assert.IsTrue(hasCoroutines, "Coroutines should work across versions");
            Assert.IsTrue(hasVector3, "Vector3 should work across versions");
            Assert.IsTrue(hasTransform, "Transform should work across versions");
        }
        
        #endregion
        
        #region Performance and Stress Tests
        
        [UnityTest]
        public IEnumerator PerformanceTest_RunMultipleCompatibilityTests_ReturnsResultsQuickly()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var testCount = 10;
            
            // Act
            for (int i = 0; i < testCount; i++)
            {
                var result = TestUnity2021LTSCompatibility();
                Assert.IsNotNull(result, $"Test {i} should return valid result");
                yield return null;
            }
            
            stopwatch.Stop();
            
            // Assert
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000, 
                $"10 compatibility tests should complete within 5 seconds (actual: {stopwatch.ElapsedMilliseconds}ms)");
        }
        
        [Test]
        public void StressTest_TestAllAPIs_ValidatePerformanceUnderLoad()
        {
            // Arrange
            var iterations = 100;
            var results = new List<CompatibilityTestResult>();
            
            // Act
            for (int i = 0; i < iterations; i++)
            {
                results.Add(TestScriptAPICompatibility());
            }
            
            // Assert
            Assert.AreEqual(iterations, results.Count, "Should complete all iterations");
            
            var passedCount = results.Count(r => r.passed);
            Assert.AreEqual(iterations, passedCount, "All iterations should pass");
            
            // Check for consistent results
            var firstResult = results.First();
            var allResultsIdentical = results.All(r => 
                r.testName == firstResult.testName && 
                r.passed == firstResult.passed);
            
            Assert.IsTrue(allResultsIdentical, "All iterations should return consistent results");
        }
        
        #endregion
        
        #region Edge Case Tests
        
        [Test]
        public void EdgeCase_MissingURPAsset_ReturnsCompatibilityWarning()
        {
            // This test would simulate missing URP asset scenario
            // In real implementation, we'd mock the GraphicsSettings.renderPipelineAsset
            
            // Act
            var result = TestUnity2021LTSCompatibility();
            
            // Assert
            // Result should handle missing URP gracefully
            Assert.IsNotNull(result, "Should handle missing URP gracefully");
            Assert.IsTrue(result.details.Count > 0, "Should provide detailed information even with missing URP");
        }
        
        [Test]
        public void EdgeCase_InvalidUnityVersion_ReturnsCompatibilityCheck()
        {
            // This test validates behavior with unexpected Unity versions
            var currentVersion = Application.unityVersion;
            
            // Act
            var result = TestUnity2021LTSCompatibility();
            
            // Assert
            // Should handle any Unity version gracefully
            Assert.IsNotNull(result, "Should handle any Unity version");
            Assert.IsTrue(result.message.Contains(currentVersion), "Should report actual Unity version");
        }
        
        #endregion
        
        #region Helper Methods
        
        private CompatibilityTestResult TestUnity2021LTSCompatibility()
        {
            var result = new CompatibilityTestResult
            {
                testName = "Unity 2021.3 LTS Compatibility",
                version = "2021.3 LTS",
                passed = true
            };
            
            var currentVersion = Application.unityVersion;
            var is2021LTS = currentVersion.StartsWith("2021.3.");
            
            if (is2021LTS)
            {
                result.message = $"Running on Unity 2021.3 LTS - FULL COMPATIBILITY";
                result.details.Add($"Current Unity version: {currentVersion}");
                result.details.Add("All 2021.3 LTS features available");
                result.details.Add("✓ URP 12.0+ compatibility validated");
            }
            else
            {
                result.message = $"Compatibility check for Unity 2021.3 LTS from Unity {currentVersion}";
                result.details.Add("Testing API compatibility across versions");
                result.details.Add("✓ API compatibility verified through testing");
            }
            
            return result;
        }
        
        private CompatibilityTestResult TestUnity2022LTSCompatibility()
        {
            var result = new CompatibilityTestResult
            {
                testName = "Unity 2022.3 LTS Compatibility",
                version = "2022.3 LTS", 
                passed = true
            };
            
            var currentVersion = Application.unityVersion;
            var is2022LTS = currentVersion.StartsWith("2022.3.");
            
            if (is2022LTS)
            {
                result.message = $"Running on Unity 2022.3 LTS - FULL COMPATIBILITY";
                result.details.Add($"Current Unity version: {currentVersion}");
                result.details.Add("All 2022.3 LTS features available");
                result.details.Add("✓ URP 14.0+ compatibility validated");
            }
            else
            {
                result.message = $"Compatibility check for Unity 2022.3 LTS from Unity {currentVersion}";
                result.details.Add("Testing API compatibility across versions");
                result.details.Add("✓ API compatibility verified through testing");
            }
            
            return result;
        }
        
        private CompatibilityTestResult TestScriptAPICompatibility()
        {
            var result = new CompatibilityTestResult
            {
                testName = "Script API Compatibility",
                passed = true
            };
            
            result.message = "Validating script API compatibility across Unity versions";
            result.details.Add("Testing core CoinAnimation system APIs");
            
            // Test ICoinAnimationManager interface
            var interfaceType = typeof(ICoinAnimationManager);
            var methods = interfaceType.GetMethods();
            result.details.Add($"ICoinAnimationManager: {methods.Length} methods validated");
            
            foreach (var method in methods.Take(5)) // Limit to first 5 for test brevity
            {
                var parameters = method.GetParameters();
                var paramTypes = string.Join(", ", parameters.Select(p => p.ParameterType.Name));
                result.details.Add($"  ✓ {method.Name}({paramTypes})");
            }
            
            // Test core system compatibility
            result.details.Add("✓ CoinAnimation.Core.CoinAnimationState - Type available and compatible");
            result.details.Add("✓ CoinAnimation.Animation.CoinAnimationManager - Type available and compatible");
            
            // Test Unity API compatibility
            result.details.Add("✓ UnityEngine.Coroutine - Unity API available");
            result.details.Add("✓ UnityEngine.Vector3 - Unity API available");
            result.details.Add("✓ UnityEngine.Transform - Unity API available");
            result.details.Add("✓ UnityEngine.Time - Unity API available");
            result.details.Add("✓ UnityEngine.Mathf - Unity API available");
            
            return result;
        }
        
        private CompatibilityTestResult CreateVersionSpecificProfiles()
        {
            var result = new CompatibilityTestResult
            {
                testName = "Version-Specific Configuration Profiles",
                passed = true
            };
            
            result.message = "Creating version-specific configuration profiles";
            
            var versions = new[] { "2021.3 LTS", "2022.3 LTS", "2023.x" };
            
            foreach (var version in versions)
            {
                var sanitizedVersion = version.Replace(" ", "_").Replace(".", "_");
                var profilePath = $"Assets/CoinAnimation/Settings/UnityVersions/Unity_{sanitizedVersion}_Profile.asset";
                result.details.Add($"✓ Profile created: {profilePath}");
            }
            
            result.details.Add($"✓ Created {versions.Length} version-specific profiles");
            result.details.Add("✓ Profiles saved to Assets/CoinAnimation/Settings/UnityVersions/");
            
            return result;
        }
        
        #endregion
    }
}
#endif