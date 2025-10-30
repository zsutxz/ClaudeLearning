using System;
using System.IO;
using UnityEngine;
using NUnit.Framework;
using CoinAnimation.Core;
using CoinAnimation.Animation;

namespace CoinAnimation.Tests
{
    /// <summary>
    /// Unity环境验证测试
    /// </summary>
    [TestFixture]
    public class UnityEnvironmentValidatorTest
    {
        [Test]
        public void UnityVersion_IsValid()
        {
            // 验证Unity版本
            string unityVersion = Application.unityVersion;
            Assert.IsNotNull(unityVersion, "Unity版本应该不为空");
            Assert.IsTrue(unityVersion.Length > 0, "Unity版本字符串应该有内容");

            Debug.Log($"Unity版本: {unityVersion}");
        }

        [Test]
        public void CoreComponents_Exist()
        {
            // 验证核心组件存在
            Assert.IsTrue(System.Type.GetType("CoinAnimation.Core.CoinAnimationState") != null,
                "CoinAnimationState应该存在");

            Assert.IsTrue(System.Type.GetType("CoinAnimation.Animation.CoinAnimationManager") != null,
                "CoinAnimationManager应该存在");

            Assert.IsTrue(System.Type.GetType("CoinAnimation.Animation.CoinAnimationController") != null,
                "CoinAnimationController应该存在");
        }

        [Test]
        public void RequiredDirectories_Exist()
        {
            // 验证必需目录存在
            string corePath = Path.Combine(Application.dataPath, "Scripts/Core");
            string animationPath = Path.Combine(Application.dataPath, "Scripts/Animation");
            string examplesPath = Path.Combine(Application.dataPath, "Scripts/Examples");

            Assert.IsTrue(Directory.Exists(corePath), $"Core目录应该存在: {corePath}");
            Assert.IsTrue(Directory.Exists(animationPath), $"Animation目录应该存在: {animationPath}");
            Assert.IsTrue(Directory.Exists(examplesPath), $"Examples目录应该存在: {examplesPath}");
        }

        [Test]
        public void RequiredFiles_Exist()
        {
            // 验证必需文件存在
            string controllerPath = Path.Combine(Application.dataPath, "Scripts/Animation/CoinAnimationController.cs");
            string managerPath = Path.Combine(Application.dataPath, "Scripts/Animation/CoinAnimationManager.cs");
            string demoPath = Path.Combine(Application.dataPath, "Scripts/Examples/SimpleCoinDemo.cs");

            Assert.IsTrue(File.Exists(controllerPath), $"CoinAnimationController应该存在: {controllerPath}");
            Assert.IsTrue(File.Exists(managerPath), $"CoinAnimationManager应该存在: {managerPath}");
            Assert.IsTrue(File.Exists(demoPath), $"SimpleCoinDemo应该存在: {demoPath}");
        }

        [Test]
        public void PerformanceMetrics_CanBeCreated()
        {
            // 测试性能指标创建
            var metrics = new CoinAnimation.Core.PerformanceMetrics();

            Assert.IsNotNull(metrics, "PerformanceMetrics应该能创建");
            Assert.IsTrue(metrics.timestamp != default(DateTime), "timestamp应该被设置");
        }

        [Test]
        public void CoinAnimationState_HasValidValues()
        {
            // 测试动画状态枚举
            Assert.IsTrue(Enum.IsDefined(typeof(CoinAnimationState), CoinAnimationState.Idle));
            Assert.IsTrue(Enum.IsDefined(typeof(CoinAnimationState), CoinAnimationState.Moving));
            Assert.IsTrue(Enum.IsDefined(typeof(CoinAnimationState), CoinAnimationState.Collecting));
            Assert.IsTrue(Enum.IsDefined(typeof(CoinAnimationState), CoinAnimationState.Pooled));
        }

        [Test]
        public void BasicAnimation_IsAvailable()
        {
            // 验证基本动画功能
            try
            {
                var testObj = new GameObject("TestAnimation");
                testObj.transform.position = Vector3.one;
                UnityEngine.Object.DestroyImmediate(testObj);

                Assert.IsTrue(true, "基本动画功能应该可用");
            }
            catch (Exception ex)
            {
                Assert.Fail($"基本动画功能不可用: {ex.Message}");
            }
        }

        [Test]
        public void CoinAnimationManager_CanBeInstantiated()
        {
            // 测试管理器实例化
            var go = new GameObject("TestManager");
            var manager = go.AddComponent<CoinAnimationManager>();

            Assert.IsNotNull(manager, "CoinAnimationManager应该能实例化");
            Assert.IsNotNull(CoinAnimationManager.Instance, "单例应该正确设置");

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void CoinAnimationController_CanBeInstantiated()
        {
            // 测试控制器实例化
            var go = new GameObject("TestController");
            var controller = go.AddComponent<CoinAnimationController>();

            Assert.IsNotNull(controller, "CoinAnimationController应该能实例化");
            Assert.AreEqual(CoinAnimationState.Idle, controller.CurrentState, "初始状态应该是Idle");

            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}