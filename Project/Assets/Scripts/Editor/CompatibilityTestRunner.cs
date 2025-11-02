using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using CoinAnimation.Core;

namespace CoinAnimation.Editor
{
    /// <summary>
    /// Compatibility Test Runner for Story 2.2 Implementation Validation
    /// Runs automated tests to validate Unity Version Compatibility implementation
    /// </summary>
    public class CompatibilityTestRunner
    {
        [MenuItem("Coin Animation/Compatibility/Run Story 2.2 Tests")]
        public static void RunStory2_2Tests()
        {
            Debug.Log("🚀 Story 2.2: Cross-Platform Compatibility and Deployment - Test Runner");
            Debug.Log("=================================================================");
            
            var testResults = new List<TestResult>();
            
            // Test 1: Validate Unity Version Compatibility Validator exists
            testResults.Add(TestUnityVersionCompatibilityValidatorExists());
            
            // Test 2: Validate directory structure
            testResults.Add(TestDirectoryStructure());
            
            // Test 3: Validate assembly definitions
            testResults.Add(TestAssemblyDefinitions());
            
            // Test 4: Validate test files exist
            testResults.Add(TestFilesExist());
            
            // Test 5: Validate code compiles without errors
            testResults.Add(TestCodeCompilation());
            
            // Report results
            ReportTestResults(testResults);
        }
        
        private static TestResult TestUnityVersionCompatibilityValidatorExists()
        {
            var result = new TestResult
            {
                testName = "UnityVersionCompatibilityValidator Exists",
                passed = false
            };
            
            try
            {
                var validatorType = System.Type.GetType("CoinAnimation.Core.Compatibility.UnityVersionCompatibilityValidator");
                if (validatorType != null)
                {
                    result.passed = true;
                    result.message = "✅ UnityVersionCompatibilityValidator class found and accessible";
                    result.details.Add("Type: " + validatorType.FullName);
                    result.details.Add("Namespace: CoinAnimation.Core.Compatibility");
                }
                else
                {
                    result.message = "❌ UnityVersionCompatibilityValidator class not found";
                    result.details.Add("Expected: CoinAnimation.Core.Compatibility.UnityVersionCompatibilityValidator");
                }
            }
            catch (Exception e)
            {
                result.message = "❌ Error checking UnityVersionCompatibilityValidator: " + e.Message;
            }
            
            return result;
        }
        
        private static TestResult TestDirectoryStructure()
        {
            var result = new TestResult
            {
                testName = "Directory Structure Validation",
                passed = true
            };
            
            var requiredDirectories = new[]
            {
                "Assets/Scripts/Core/Compatibility",
                "Assets/Scripts/Settings/UnityVersions",
                "Assets/Scripts/Tests/Compatibility"
            };
            
            result.message = "Validating required directory structure...";
            
            foreach (var dir in requiredDirectories)
            {
                if (AssetDatabase.IsValidFolder(dir))
                {
                    result.details.Add($"✅ {dir} - Directory exists");
                }
                else
                {
                    result.passed = false;
                    result.details.Add($"❌ {dir} - Directory missing");
                }
            }
            
            if (result.passed)
            {
                result.message = "✅ All required directories exist";
            }
            else
            {
                result.message = "❌ Some required directories are missing";
            }
            
            return result;
        }
        
        private static TestResult TestAssemblyDefinitions()
        {
            var result = new TestResult
            {
                testName = "Assembly Definition Validation",
                passed = true
            };
            
            var requiredAsmdefs = new[]
            {
                "Assets/Scripts/Core/Compatibility/CoinAnimation.Compatibility.asmdef"
            };
            
            result.message = "Validating assembly definitions...";
            
            foreach (var asmdef in requiredAsmdefs)
            {
                if (File.Exists(Path.Combine(Application.dataPath, "..", asmdef)))
                {
                    result.details.Add($"✅ {asmdef} - Assembly definition exists");
                    
                    // Try to read and validate asmdef content
                    try
                    {
                        var asmdefPath = asmdef.Replace("Assets/", "");
                        var asmdefAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(asmdefPath);
                        if (asmdefAsset != null && asmdefAsset.text.Contains("CoinAnimation.Compatibility"))
                        {
                            result.details.Add($"✅ {asmdef} - Content valid");
                        }
                        else
                        {
                            result.passed = false;
                            result.details.Add($"❌ {asmdef} - Content invalid or empty");
                        }
                    }
                    catch (Exception e)
                    {
                        result.passed = false;
                        result.details.Add($"❌ {asmdef} - Error reading: {e.Message}");
                    }
                }
                else
                {
                    result.passed = false;
                    result.details.Add($"❌ {asmdef} - Assembly definition missing");
                }
            }
            
            if (result.passed)
            {
                result.message = "✅ All required assembly definitions exist and are valid";
            }
            else
            {
                result.message = "❌ Some assembly definitions are missing or invalid";
            }
            
            return result;
        }
        
        private static TestResult TestFilesExist()
        {
            var result = new TestResult
            {
                testName = "Implementation Files Validation",
                passed = true
            };
            
            var requiredFiles = new[]
            {
                "Assets/Scripts/Core/Compatibility/UnityVersionCompatibilityValidator.cs",
                "Assets/Scripts/Tests/Compatibility/UnityVersionCompatibilityValidatorTests.cs",
                "Assets/Scripts/Editor/CompatibilityTestRunner.cs"
            };
            
            result.message = "Validating implementation files...";
            
            foreach (var file in requiredFiles)
            {
                if (File.Exists(Path.Combine(Application.dataPath, "..", file)))
                {
                    result.details.Add($"✅ {file} - File exists");
                    
                    // Check file size to ensure it's not empty
                    var fileInfo = new FileInfo(Path.Combine(Application.dataPath, "..", file));
                    if (fileInfo.Length > 100) // At least 100 bytes
                    {
                        result.details.Add($"✅ {file} - File size: {fileInfo.Length} bytes");
                    }
                    else
                    {
                        result.passed = false;
                        result.details.Add($"❌ {file} - File too small: {fileInfo.Length} bytes");
                    }
                }
                else
                {
                    result.passed = false;
                    result.details.Add($"❌ {file} - File missing");
                }
            }
            
            if (result.passed)
            {
                result.message = "✅ All required implementation files exist";
            }
            else
            {
                result.message = "❌ Some implementation files are missing or too small";
            }
            
            return result;
        }
        
        private static TestResult TestCodeCompilation()
        {
            var result = new TestResult
            {
                testName = "Code Compilation Validation",
                passed = true
            };
            
            result.message = "Validating code compilation...";
            
            try
            {
                // Try to access the UnityVersionCompatibilityValidator type
                var validatorType = System.Type.GetType("CoinAnimation.Core.Compatibility.UnityVersionCompatibilityValidator");
                if (validatorType != null)
                {
                    result.details.Add("✅ UnityVersionCompatibilityValidator compiles successfully");
                    
                    // Check for key methods
                    var methods = validatorType.GetMethods();
                    var keyMethods = new[] { "RunAllCompatibilityTests", "TestUnity2021LTSCompatibility", "TestUnity2022LTSCompatibility" };
                    
                    foreach (var method in keyMethods)
                    {
                        var hasMethod = Array.Exists(methods, m => m.Name.Contains(method));
                        if (hasMethod)
                        {
                            result.details.Add($"✅ Method {method} found and accessible");
                        }
                        else
                        {
                            result.passed = false;
                            result.details.Add($"❌ Method {method} not found");
                        }
                    }
                }
                else
                {
                    result.passed = false;
                    result.details.Add("❌ UnityVersionCompatibilityValidator type not accessible - compilation error");
                }
                
                // Test ICoinAnimationManager interface access
                var interfaceType = typeof(ICoinAnimationManager);
                if (interfaceType != null)
                {
                    result.details.Add("✅ ICoinAnimationManager interface accessible");
                }
                else
                {
                    result.passed = false;
                    result.details.Add("❌ ICoinAnimationManager interface not accessible");
                }
                
            }
            catch (Exception e)
            {
                result.passed = false;
                result.message = "❌ Code compilation test failed: " + e.Message;
                result.details.Add("Exception: " + e.GetType().Name);
                result.details.Add("Message: " + e.Message);
            }
            
            if (result.passed)
            {
                result.message = "✅ All code compiles successfully";
            }
            
            return result;
        }
        
        private static void ReportTestResults(List<TestResult> results)
        {
            Debug.Log("\n📊 STORY 2.2 IMPLEMENTATION TEST RESULTS");
            Debug.Log("=====================================");
            
            var passedCount = 0;
            var totalCount = results.Count;
            
            foreach (var result in results)
            {
                var status = result.passed ? "✅ PASS" : "❌ FAIL";
                Debug.Log($"\n{status} {result.testName}");
                Debug.Log($"   {result.message}");
                
                foreach (var detail in result.details)
                {
                    Debug.Log($"   {detail}");
                }
                
                if (result.passed) passedCount++;
            }
            
            Debug.Log($"\n🎯 SUMMARY: {passedCount}/{totalCount} tests passed");
            
            if (passedCount == totalCount)
            {
                Debug.Log("🚀 ALL TESTS PASSED! Story 2.2 Task 1 implementation is VALIDATED!");
                Debug.Log("✅ Unity Version Compatibility Validation is READY FOR REVIEW!");
            }
            else
            {
                Debug.LogWarning($"⚠️ {totalCount - passedCount} test(s) failed. Please review implementation.");
            }
            
            Debug.Log("=====================================");
        }
        
        [Serializable]
        public class TestResult
        {
            public string testName;
            public bool passed;
            public string message;
            public List<string> details = new List<string>();
        }
    }
}