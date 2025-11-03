using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using NUnit.Framework;
using CoinAnimation.Animation;
using CoinAnimation.Core;

namespace CoinAnimation.Tests
{
    /// <summary>
    /// 简化的UGUI动画测试
    /// </summary>
    public class SimplifiedUGUITests
    {
        private GameObject _testCanvas;
        private GameObject _testCoin;
        private UGUICoinAnimationController _controller;

        [SetUp]
        public void Setup()
        {
            // 创建测试Canvas
            _testCanvas = new GameObject("TestCanvas");
            Canvas canvas = _testCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // 创建测试金币
            _testCoin = new GameObject("TestCoin");
            _testCoin.transform.SetParent(_testCanvas.transform, false);

            // 添加组件
            _testCoin.AddComponent<Image>();
            _controller = _testCoin.AddComponent<UGUICoinAnimationController>();

            // 设置基础属性
            _testCoin.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        }

        [TearDown]
        public void TearDown()
        {
            if (_testCoin != null) Object.DestroyImmediate(_testCoin);
            if (_testCanvas != null) Object.DestroyImmediate(_testCanvas);
        }

        [Test]
        public void UGUICoinController_InitializesCorrectly()
        {
            Assert.IsNotNull(_controller, "UGUI控制器应该创建成功");
            Assert.AreEqual(CoinAnimationState.Idle, _controller.CurrentState, "初始状态应该是Idle");
        }

        [UnityTest]
        public IEnumerator UGUICoinAnimation_BasicMovement()
        {
            Vector3 targetPos = new Vector3(100f, 0f, 0f);
            _controller.AnimateToPosition(targetPos, 0.5f);

            Assert.AreEqual(CoinAnimationState.Moving, _controller.CurrentState);

            yield return new WaitForSeconds(0.6f);

            Assert.AreEqual(CoinAnimationState.Idle, _controller.CurrentState);
        }

        [UnityTest]
        public IEnumerator UGUICoinCollection_WorksCorrectly()
        {
            Vector3 collectionPoint = new Vector3(0f, 100f, 0f);
            _controller.CollectCoin(collectionPoint, 0.5f);

            Assert.AreEqual(CoinAnimationState.Collecting, _controller.CurrentState);

            yield return new WaitForSeconds(0.6f);

            Assert.AreEqual(CoinAnimationState.Pooled, _controller.CurrentState);
            Assert.IsFalse(_testCoin.activeSelf, "收集后金币应该被禁用");
        }
    }
}