using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using CoinAnimation.Core;
using CoinAnimation.Animation;

namespace CoinAnimation.Tests
{
    /// <summary>
    /// 简化的金币动画测试套件
    /// </summary>
    public class CoinAnimationTestSuite
    {
        #region Test Setup

        private GameObject _testCoin;
        private CoinAnimationController _coinController;

        [SetUp]
        public void SetUp()
        {
            CreateTestEnvironment();
        }

        [TearDown]
        public void TearDown()
        {
            CleanupTestEnvironment();
        }

        private void CreateTestEnvironment()
        {
            // 创建测试金币
            _testCoin = new GameObject("TestCoin");
            _testCoin.transform.position = Vector3.zero;
            _coinController = _testCoin.AddComponent<CoinAnimationController>();

            // 添加必要组件
            Rigidbody rb = _testCoin.AddComponent<Rigidbody>();
            rb.useGravity = false;
            _testCoin.AddComponent<SphereCollider>();
        }

        private void CleanupTestEnvironment()
        {
            if (_testCoin != null) Object.DestroyImmediate(_testCoin);
        }

        #endregion

        #region Basic Tests

        [Test]
        public void Test_CoinState_InitializesAsIdle()
        {
            Assert.AreEqual(CoinAnimationState.Idle, _coinController.CurrentState);
        }

        [Test]
        public void Test_CoinAnimation_BasicMovement()
        {
            Vector3 targetPos = new Vector3(3f, 0f, 0f);
            _coinController.AnimateToPosition(targetPos, 1f);

            Assert.AreEqual(CoinAnimationState.Moving, _coinController.CurrentState);
            Assert.IsTrue(_coinController.IsAnimating);
        }

        [UnityTest]
        public IEnumerator Test_CoinAnimation_ReachesTarget()
        {
            Vector3 targetPos = new Vector3(3f, 0f, 0f);
            _coinController.AnimateToPosition(targetPos, 0.5f);

            yield return new WaitForSeconds(0.6f);

            Assert.AreEqual(CoinAnimationState.Idle, _coinController.CurrentState);
            Assert.IsFalse(_coinController.IsAnimating);

            float distance = Vector3.Distance(_coinController.transform.position, targetPos);
            Assert.Less(distance, 0.1f);
        }

        [Test]
        public void Test_CoinCollection_TriggersCorrectState()
        {
            Vector3 collectionPoint = new Vector3(0, 0, 5);
            _coinController.CollectCoin(collectionPoint, 1f);

            Assert.AreEqual(CoinAnimationState.Collecting, _coinController.CurrentState);
            Assert.IsTrue(_coinController.IsAnimating);
        }

        [UnityTest]
        public IEnumerator Test_CoinCollection_CompletesSuccessfully()
        {
            Vector3 collectionPoint = new Vector3(0, 0, 5);
            _coinController.CollectCoin(collectionPoint, 0.5f);

            yield return new WaitForSeconds(0.6f);

            Assert.AreEqual(CoinAnimationState.Pooled, _coinController.CurrentState);
            Assert.IsFalse(_coinController.IsAnimating);
            Assert.IsFalse(_coinController.gameObject.activeSelf);
        }

        #endregion

        #region Performance Tests

        [UnityTest]
        public IEnumerator Test_Performance_BasicAnimation()
        {
            int coinCount = 20;
            var coins = new List<CoinAnimationController>();

            // 创建多个金币
            for (int i = 0; i < coinCount; i++)
            {
                GameObject coin = new GameObject($"Coin{i}");
                coin.transform.position = new Vector3(i * 0.5f, 0f, 0f);

                var controller = coin.AddComponent<CoinAnimationController>();
                coin.AddComponent<Rigidbody>();
                coin.AddComponent<SphereCollider>();

                coins.Add(controller);
            }

            // 启动动画
            float startTime = Time.time;
            float testDuration = 1f;
            int frameCount = 0;

            while (Time.time - startTime < testDuration)
            {
                foreach (var coin in coins)
                {
                    if (Random.value < 0.1f)
                    {
                        Vector3 target = new Vector3(
                            Random.Range(-3f, 3f),
                            0f,
                            Random.Range(-3f, 3f)
                        );
                        coin.AnimateToPosition(target, Random.Range(0.5f, 1f));
                    }
                }

                frameCount++;
                yield return null;
            }

            float fps = frameCount / testDuration;
            Assert.Greater(fps, 30f, $"Performance should be above 30fps (achieved: {fps:F1}fps)");

            // 清理
            foreach (var coin in coins)
            {
                if (coin != null && coin.gameObject != null)
                    Object.DestroyImmediate(coin.gameObject);
            }
        }

        #endregion

        #region Integration Tests

        [UnityTest]
        public IEnumerator Test_Integration_AnimationFlow()
        {
            // 测试完整动画流程：idle -> moving -> collecting -> pooled

            Assert.AreEqual(CoinAnimationState.Idle, _coinController.CurrentState);

            // 移动
            Vector3 moveTarget = new Vector3(2f, 0f, 0f);
            _coinController.AnimateToPosition(moveTarget, 0.5f);

            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(CoinAnimationState.Moving, _coinController.CurrentState);

            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(CoinAnimationState.Idle, _coinController.CurrentState);

            // 收集
            Vector3 collectTarget = new Vector3(2f, 0f, 2f);
            _coinController.CollectCoin(collectTarget, 0.5f);

            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(CoinAnimationState.Collecting, _coinController.CurrentState);

            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(CoinAnimationState.Pooled, _coinController.CurrentState);
        }

        #endregion
    }
}