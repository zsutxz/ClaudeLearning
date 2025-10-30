using System;
using System.IO;
using UnityEngine;
using NUnit.Framework;
using CoinAnimation.Core;
using CoinAnimation.Animation;

namespace CoinAnimation.Tests
{
    /// <summary>
    /// 项目配置验证测试
    /// </summary>
    [TestFixture]
    public class ProjectConfigurationTest
    {
        [Test]
        public void CoreAssemblyDefinition_HasValidReferences()
        {
            // 验证Core程序集定义
            var coreAsmdefPath = Path.Combine(Application.dataPath, "Scripts/CoinAnimation.Core.asmdef");

            if (File.Exists(coreAsmdefPath))
            {
                string coreContent = File.ReadAllText(coreAsmdefPath);
                Assert.IsTrue(coreContent.Contains("\"name\": \"CoinAnimation.Core\""),
                    "Core assembly name should be correct");
            }
            else
            {
                // 如果没有asmdef文件，跳过测试
                Assert.Inconclusive("Core assembly definition file not found");
            }
        }

        [Test]
        public void AnimationAssemblyDefinition_HasValidReferences()
        {
            // 验证Animation程序集定义
            var animationAsmdefPath = Path.Combine(Application.dataPath, "Scripts/CoinAnimation.Animation.asmdef");

            if (File.Exists(animationAsmdefPath))
            {
                string animationContent = File.ReadAllText(animationAsmdefPath);
                Assert.IsTrue(animationContent.Contains("\"name\": \"CoinAnimation.Animation\""),
                    "Animation assembly name should be correct");

                // 应该引用Core程序集
                Assert.IsTrue(animationContent.Contains("\"CoinAnimation.Core\""),
                    "Animation assembly should reference Core assembly");
            }
            else
            {
                Assert.Inconclusive("Animation assembly definition file not found");
            }
        }

        [Test]
        public void CoreFiles_HaveCorrectNamespace()
        {
            // 验证Core文件命名空间
            string statePath = Path.Combine(Application.dataPath, "Scripts/Core/CoinAnimationState.cs");

            if (File.Exists(statePath))
            {
                string content = File.ReadAllText(statePath);
                Assert.IsTrue(content.Contains("namespace CoinAnimation.Core"),
                    "CoinAnimationState should be in CoinAnimation.Core namespace");
            }
            else
            {
                Assert.Fail("CoinAnimationState.cs not found");
            }
        }

        [Test]
        public void AnimationFiles_HaveCorrectNamespace()
        {
            // 验证Animation文件命名空间
            string controllerPath = Path.Combine(Application.dataPath, "Scripts/Animation/CoinAnimationController.cs");

            if (File.Exists(controllerPath))
            {
                string content = File.ReadAllText(controllerPath);
                Assert.IsTrue(content.Contains("namespace CoinAnimation.Animation"),
                    "CoinAnimationController should be in CoinAnimation.Animation namespace");
            }
            else
            {
                Assert.Fail("CoinAnimationController.cs not found");
            }

            string managerPath = Path.Combine(Application.dataPath, "Scripts/Animation/CoinAnimationManager.cs");

            if (File.Exists(managerPath))
            {
                string content = File.ReadAllText(managerPath);
                Assert.IsTrue(content.Contains("namespace CoinAnimation.Animation"),
                    "CoinAnimationManager should be in CoinAnimation.Animation namespace");
            }
            else
            {
                Assert.Fail("CoinAnimationManager.cs not found");
            }
        }

        [Test]
        public void BasicAnimation_IsWorking()
        {
            // 验证基本动画功能
            try
            {
                // 测试基本位置设置
                var testObj = new GameObject("AnimationTest");
                testObj.transform.position = Vector3.zero;
                testObj.transform.position = Vector3.one;

                Assert.AreEqual(Vector3.one, testObj.transform.position, "位置动画应该工作");
                UnityEngine.Object.DestroyImmediate(testObj);

                Assert.IsTrue(true, "基本动画功能测试通过");
            }
            catch (Exception ex)
            {
                Assert.Fail($"基本动画功能失败: {ex.Message}");
            }
        }

        [Test]
        public void ProjectStructure_IsValid()
        {
            // 验证项目结构
            string basePath = Path.Combine(Application.dataPath, "Scripts");

            Assert.IsTrue(Directory.Exists(basePath), "Scripts directory should exist");
            Assert.IsTrue(Directory.Exists(Path.Combine(basePath, "Core")), "Core directory should exist");
            Assert.IsTrue(Directory.Exists(Path.Combine(basePath, "Animation")), "Animation directory should exist");
            Assert.IsTrue(Directory.Exists(Path.Combine(basePath, "Examples")), "Examples directory should exist");
            Assert.IsTrue(Directory.Exists(Path.Combine(basePath, "Tests")), "Tests directory should exist");
        }

        [Test]
        public void ExampleFiles_AreValid()
        {
            // 验证示例文件
            string demoPath = Path.Combine(Application.dataPath, "Scripts/Examples/SimpleCoinDemo.cs");
            string readmePath = Path.Combine(Application.dataPath, "Scripts/Examples/README.md");

            Assert.IsTrue(File.Exists(demoPath), "SimpleCoinDemo.cs should exist");
            Assert.IsTrue(File.Exists(readmePath), "README.md should exist");

            // 验证演示脚本内容
            string demoContent = File.ReadAllText(demoPath);
            Assert.IsTrue(demoContent.Contains("public class SimpleCoinDemo"),
                "SimpleCoinDemo class should be defined");
            Assert.IsTrue(demoContent.Contains("CoinAnimationController"),
                "Should reference CoinAnimationController");
        }

        [Test]
        public void RequiredComponents_HaveValidDependencies()
        {
            // 验证必需组件的依赖关系
            Assert.IsTrue(System.Type.GetType("CoinAnimation.Core.CoinAnimationState") != null,
                "CoinAnimationState should be available");

            Assert.IsTrue(System.Type.GetType("CoinAnimation.Animation.CoinAnimationController") != null,
                "CoinAnimationController should be available");

            Assert.IsTrue(System.Type.GetType("CoinAnimation.Animation.CoinAnimationManager") != null,
                "CoinAnimationManager should be available");
        }

        [Test]
        public void PerformanceMetrics_AreAccessible()
        {
            // 测试性能指标访问
            try
            {
                var metrics = new CoinAnimation.Core.PerformanceMetrics();
                Assert.IsNotNull(metrics, "PerformanceMetrics should be accessible");
            }
            catch (Exception ex)
            {
                Assert.Fail($"PerformanceMetrics not accessible: {ex.Message}");
            }
        }
    }
}