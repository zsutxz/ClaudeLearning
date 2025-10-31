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
    /// UGUI金币动画控制器测试套件
    /// 验证UGUI动画系统的正确性和性能
    /// </summary>
    public class UGUICoinAnimationTests
    {
        private GameObject _testCanvas;
        private GameObject _testCoin;
        private UGUICoinAnimationController _controller;
        private Image _image;

        [SetUp]
        public void Setup()
        {
            // 创建测试Canvas
            _testCanvas = new GameObject("TestCanvas");
            Canvas canvas = _testCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = _testCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // 创建测试金币
            _testCoin = new GameObject("TestCoin");
            _testCoin.transform.SetParent(_testCanvas.transform, false);

            // 添加组件
            _image = _testCoin.AddComponent<Image>();
            _controller = _testCoin.AddComponent<UGUICoinAnimationController>();

            // 设置基础属性
            _image.color = Color.yellow;
            _testCoin.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        }

        [TearDown]
        public void TearDown()
        {
            if (_controller != null)
            {
                _controller.StopCurrentAnimation();
            }

            Object.DestroyImmediate(_testCoin);
            Object.DestroyImmediate(_testCanvas);
        }

        #region Initialization Tests

        [Test]
        public void UGUICoinAnimationController_Initialization_HasCorrectInitialState()
        {
            // Assert
            Assert.AreEqual(CoinAnimationState.Idle, _controller.CurrentState);
            Assert.IsFalse(_controller.IsAnimating);
            Assert.IsNotNull(_controller.RectTransform);
        }

        [Test]
        public void UGUICoinAnimationController_Initialization_HasRequiredComponents()
        {
            // Assert
            Assert.IsNotNull(_controller.RectTransform);
            Assert.IsNotNull(_image);
            Assert.IsNotNull(_controller.GetComponent<RectTransform>());
        }

        #endregion

        #region State Management Tests

        [Test]
        public void SetState_ValidState_UpdatesCurrentState()
        {
            // Arrange
            var stateChangedEventFired = false;
            CoinAnimationState newState = CoinAnimationState.Moving;
            CoinAnimationState receivedState = CoinAnimationState.Idle;

            _controller.OnStateChanged += (sender, args) =>
            {
                stateChangedEventFired = true;
                receivedState = args.CurrentState;
            };

            // Act
            _controller.SetState(newState);

            // Assert
            Assert.AreEqual(newState, _controller.CurrentState);
            Assert.IsTrue(stateChangedEventFired);
            Assert.AreEqual(newState, receivedState);
        }

        [Test]
        public void SetState_SameState_DoesNotTriggerEvent()
        {
            // Arrange
            var stateChangedEventFired = false;
            _controller.OnStateChanged += (sender, args) => stateChangedEventFired = true;

            // Act
            _controller.SetState(CoinAnimationState.Idle);

            // Assert
            Assert.IsFalse(stateChangedEventFired);
        }

        #endregion

        #region Animation Tests

        [UnityTest]
        public IEnumerator AnimateToPosition_ValidTarget_MovesToTargetPosition()
        {
            // Arrange
            Vector2 startPosition = new Vector2(0, 0);
            Vector2 targetPosition = new Vector2(200, 100);
            _controller.RectTransform.anchoredPosition = startPosition;

            // Act
            _controller.AnimateToPosition(targetPosition, 1f);

            // Assert - 状态应该立即改变
            Assert.AreEqual(CoinAnimationState.Moving, _controller.CurrentState);
            Assert.IsTrue(_controller.IsAnimating);

            // 等待动画完成
            yield return new WaitForSeconds(1.1f);

            // Assert - 最终位置应该接近目标位置
            Vector2 finalPosition = _controller.RectTransform.anchoredPosition;
            Assert.AreEqual(CoinAnimationState.Idle, _controller.CurrentState);
            Assert.IsFalse(_controller.IsAnimating);
            Assert.IsTrue(Vector2.Distance(finalPosition, targetPosition) < 0.01f);
        }

        [UnityTest]
        public IEnumerator CollectCoin_ValidCollectionPosition_PerformsMultiPhaseAnimation()
        {
            // Arrange
            Vector2 startPosition = new Vector2(100, 100);
            Vector2 collectionPosition = new Vector2(0, 0);
            _controller.RectTransform.anchoredPosition = startPosition;
            _controller.RectTransform.localScale = Vector3.one;

            // Act
            _controller.CollectCoin(collectionPosition, 2f);

            // Assert - 状态应该立即改变
            Assert.AreEqual(CoinAnimationState.Collecting, _controller.CurrentState);
            Assert.IsTrue(_controller.IsAnimating);

            // 等待动画完成
            yield return new WaitForSeconds(2.5f);

            // Assert - 金币应该被停用并进入池化状态
            Assert.AreEqual(CoinAnimationState.Pooled, _controller.CurrentState);
            Assert.IsFalse(_controller.gameObject.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator AnimateToWorldPosition_ValidWorldPosition_ConvertsAndAnimatesCorrectly()
        {
            // Arrange
            Vector2 startPosition = Vector2.zero;
            Vector3 worldTargetPosition = new Vector3(100, 50, 0);
            _controller.RectTransform.anchoredPosition = startPosition;

            // 创建测试相机
            GameObject cameraObject = new GameObject("TestCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";

            // Act
            _controller.AnimateToWorldPosition(worldTargetPosition, 1f);

            // Assert
            Assert.AreEqual(CoinAnimationState.Moving, _controller.CurrentState);
            Assert.IsTrue(_controller.IsAnimating);

            // 等待动画完成
            yield return new WaitForSeconds(1.1f);

            Assert.AreEqual(CoinAnimationState.Idle, _controller.CurrentState);
            Assert.IsFalse(_controller.IsAnimating);

            // Cleanup
            Object.DestroyImmediate(cameraObject);
        }

        #endregion

        #region Utility Methods Tests

        [Test]
        public void StopCurrentAnimation_DuringAnimation_StopsAnimationAndResetsState()
        {
            // Arrange
            _controller.AnimateToPosition(new Vector2(100, 100), 2f);
            Assert.IsTrue(_controller.IsAnimating);

            // Act
            _controller.StopCurrentAnimation();

            // Assert
            Assert.IsFalse(_controller.IsAnimating);
        }

        [Test]
        public void PauseAnimations_DuringActiveAnimation_SetsPausedState()
        {
            // Arrange
            _controller.AnimateToPosition(new Vector2(100, 100), 2f);

            // Act
            _controller.PauseAnimations();

            // Assert
            Assert.AreEqual(CoinAnimationState.Paused, _controller.CurrentState);
        }

        [Test]
        public void ResumeAnimations_FromPausedState_SetsMovingState()
        {
            // Arrange
            _controller.AnimateToPosition(new Vector2(100, 100), 2f);
            _controller.PauseAnimations();

            // Act
            _controller.ResumeAnimations();

            // Assert
            Assert.AreEqual(CoinAnimationState.Moving, _controller.CurrentState);
        }

        [Test]
        public void ResetCoin_AfterAnimation_ResetsToOriginalState()
        {
            // Arrange
            Vector3 originalScale = _controller.RectTransform.localScale;
            _controller.RectTransform.localScale = Vector3.one * 2f;

            // Act
            _controller.ResetCoin();

            // Assert
            Assert.AreEqual(CoinAnimationState.Idle, _controller.CurrentState);
            Assert.AreEqual(originalScale, _controller.RectTransform.localScale);
        }

        #endregion

        #region Performance Tests

        [UnityTest]
        public IEnumerator MultipleCoins_ConcurrentAnimation_MaintainsPerformance()
        {
            // Arrange
            int coinCount = 20;
            UGUICoinAnimationController[] controllers = new UGUICoinAnimationController[coinCount];
            float startTime = Time.realtimeSinceStartup;

            // 创建多个金币
            for (int i = 0; i < coinCount; i++)
            {
                GameObject coin = new GameObject($"TestCoin{i}");
                coin.transform.SetParent(_testCanvas.transform, false);

                Image image = coin.AddComponent<Image>();
                image.color = Color.yellow;
                coin.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

                controllers[i] = coin.AddComponent<UGUICoinAnimationController>();

                // 启动动画
                Vector2 target = new Vector2(Random.Range(-200, 200), Random.Range(-100, 100));
                controllers[i].AnimateToPosition(target, 1f);
            }

            // 等待所有动画完成
            yield return new WaitForSeconds(1.5f);

            float endTime = Time.realtimeSinceStartup;
            float animationTime = endTime - startTime;

            // Assert
            Assert.Less(animationTime, 2.0f, "20 coins animation should complete within 2 seconds");

            // 清理
            for (int i = 0; i < coinCount; i++)
            {
                if (controllers[i] != null)
                {
                    Object.DestroyImmediate(controllers[i].gameObject);
                }
            }
        }

        #endregion

        #region Edge Cases Tests

        [Test]
        public void AnimateToPosition_ZeroDuration_StillChangesState()
        {
            // Arrange
            Vector2 targetPosition = new Vector2(100, 100);

            // Act
            _controller.AnimateToPosition(targetPosition, 0f);

            // Assert
            Assert.AreEqual(CoinAnimationState.Moving, _controller.CurrentState);
        }

        [Test]
        public void CollectCoin_WithoutCanvas_StillWorks()
        {
            // Arrange - 移除Canvas
            Object.DestroyImmediate(_testCanvas);

            // Act & Assert - 不应该抛出异常
            Assert.DoesNotThrow(() => _controller.CollectCoin(Vector2.zero, 1f));
        }

        [UnityTest]
        public IEnumerator MultipleAnimationCalls_SecondCallStopsFirstAnimation()
        {
            // Arrange
            Vector2 firstTarget = new Vector2(100, 100);
            Vector2 secondTarget = new Vector2(-100, -100);

            // Act
            _controller.AnimateToPosition(firstTarget, 2f);
            yield return new WaitForSeconds(0.5f); // 让第一个动画运行一段时间

            _controller.AnimateToPosition(secondTarget, 1f);

            // Assert
            yield return new WaitForSeconds(1.5f);
            Vector2 finalPosition = _controller.RectTransform.anchoredPosition;

            // 最终位置应该接近第二个目标位置
            Assert.IsTrue(Vector2.Distance(finalPosition, secondTarget) < 0.01f);
        }

        #endregion
    }

    /// <summary>
    /// UGUI金币动画集成测试
    /// 验证与现有系统的集成
    /// </summary>
    public class UGUICoinIntegrationTests
    {
        private GameObject _testCanvas;
        private GameObject _coinManager;

        [SetUp]
        public void Setup()
        {
            // 创建测试环境
            _testCanvas = new GameObject("TestCanvas");
            Canvas canvas = _testCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            _coinManager = new GameObject("CoinAnimationManager");
            _coinManager.AddComponent<CoinAnimationManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testCanvas);
            Object.DestroyImmediate(_coinManager);
        }

        [UnityTest]
        public IEnumerator UGUICoinWithAnimationManager_Integration_WorksCorrectly()
        {
            // Arrange
            GameObject coinObject = new GameObject("TestCoin");
            coinObject.transform.SetParent(_testCanvas.transform, false);

            coinObject.AddComponent<Image>();
            UGUICoinAnimationController controller = coinObject.AddComponent<UGUICoinAnimationController>();

            // Act
            yield return new WaitForSeconds(0.1f); // 等待Start()调用

            // Assert
            Assert.IsNotNull(controller);
            Assert.AreNotEqual(-1, controller.CoinId);
            Assert.AreEqual(CoinAnimationState.Idle, controller.CurrentState);

            // Cleanup
            Object.DestroyImmediate(coinObject);
        }
    }
}