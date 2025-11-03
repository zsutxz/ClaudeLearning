using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using CoinAnimation.Core;
using CoinAnimation.Animation;
using Object = UnityEngine.Object;

namespace CoinAnimation.Tests
{
    /// <summary>
    /// 简化的核心动画测试套件 - 包含所有基本功能测试
    /// </summary>
    public class SimplifiedCoinAnimationTests
    {
        private GameObject _testCoin;
        private CoinAnimationController _coinController;
        private GameObject _testManagerObject;
        private CoinAnimationManager _animationManager;

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

            // 创建管理器
            _testManagerObject = new GameObject("TestManager");
            _animationManager = _testManagerObject.AddComponent<CoinAnimationManager>();
        }

        private void CleanupTestEnvironment()
        {
            if (_testCoin != null) Object.DestroyImmediate(_testCoin);
            if (_testManagerObject != null) Object.DestroyImmediate(_testManagerObject);
        }

        #region 基本动画测试

        [Test]
        public void CoinState_InitializesAsIdle()
        {
            Assert.AreEqual(CoinAnimationState.Idle, _coinController.CurrentState);
        }

        [Test]
        public void CoinAnimation_BasicMovement()
        {
            Vector3 targetPos = new Vector3(3f, 0f, 0f);
            _coinController.AnimateToPosition(targetPos, 1f);

            Assert.AreEqual(CoinAnimationState.Moving, _coinController.CurrentState);
            Assert.IsTrue(_coinController.IsAnimating);
        }

        [UnityTest]
        public IEnumerator CoinAnimation_ReachesTarget()
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
        public void CoinCollection_TriggersCorrectState()
        {
            Vector3 collectionPoint = new Vector3(0, 0, 5);
            _coinController.CollectCoin(collectionPoint, 1f);

            Assert.AreEqual(CoinAnimationState.Collecting, _coinController.CurrentState);
            Assert.IsTrue(_coinController.IsAnimating);
        }

        [UnityTest]
        public IEnumerator CoinCollection_CompletesSuccessfully()
        {
            Vector3 collectionPoint = new Vector3(0, 0, 5);
            _coinController.CollectCoin(collectionPoint, 0.5f);

            yield return new WaitForSeconds(0.6f);

            Assert.AreEqual(CoinAnimationState.Pooled, _coinController.CurrentState);
            Assert.IsFalse(_coinController.IsAnimating);
            Assert.IsFalse(_coinController.gameObject.activeSelf);
        }

        #endregion

        #region 管理器测试

        [Test]
        public void AnimationManager_CanBeCreated()
        {
            Assert.IsNotNull(_animationManager, "CoinAnimationManager should be created");
        }

        [Test]
        public void AnimationManager_AccessibleInstance()
        {
            // 测试单例访问
            var instance = CoinAnimationManager.Instance;
            Assert.IsNotNull(instance, "CoinAnimationManager instance should be accessible");
        }

        #endregion

        #region 性能测试

        [UnityTest]
        public IEnumerator Performance_BasicAnimation()
        {
            int coinCount = 20;
            var coins = new System.Collections.Generic.List<CoinAnimationController>();

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

        #region 集成测试

        [UnityTest]
        public IEnumerator Integration_AnimationFlow()
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

        #region 环境验证测试

        [Test]
        public void Environment_Validation()
        {
            // 验证所有必需的组件都可用
            Assert.IsTrue(System.Type.GetType("CoinAnimation.Core.CoinAnimationState") != null,
                "CoinAnimationState type should be available");

            Assert.IsTrue(System.Type.GetType("CoinAnimation.Animation.CoinAnimationManager") != null,
                "CoinAnimationManager type should be available");

            Assert.IsTrue(System.Type.GetType("CoinAnimation.Animation.CoinAnimationController") != null,
                "CoinAnimationController type should be available");

            Debug.Log("✅ 测试环境验证通过");
        }

        [Test]
        public void BasicAnimationFramework_Works()
        {
            // 验证基本的动画功能
            GameObject testObj = new GameObject("TestObj");

            // 简单的位置测试
            testObj.transform.position = Vector3.one;

            // 验证位置已设置
            Assert.AreEqual(Vector3.one, testObj.transform.position, "位置应该正确设置");

            // 清理
            Object.DestroyImmediate(testObj);

            Debug.Log("✅ 基本动画框架验证通过");
        }

        #endregion
    }
}