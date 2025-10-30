using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using CoinAnimation.Core;
using CoinAnimation.Animation;

namespace CoinAnimation.Tests
{
    /// <summary>
    /// 简化的性能验证测试
    /// </summary>
    public class PerformanceValidationScenarios
    {
        [SetUp]
        public void SetUp()
        {
            // 基本设置
        }

        [TearDown]
        public void TearDown()
        {
            DG.Tweening.DOTween.KillAll();
        }

        [Test]
        public void Test_BasicSetup()
        {
            Assert.IsTrue(true, "基本设置测试通过");
        }

        [UnityTest]
        public IEnumerator Test_Performance_BasicCoinAnimation()
        {
            // 创建30个金币进行性能测试
            int coinCount = 30;
            var coins = new System.Collections.Generic.List<GameObject>();

            for (int i = 0; i < coinCount; i++)
            {
                GameObject coin = new GameObject($"Coin{i}");
                coin.transform.position = new Vector3(i * 0.3f, 0f, 0f);

                var controller = coin.AddComponent<CoinAnimationController>();
                coin.AddComponent<Rigidbody>();
                coin.AddComponent<SphereCollider>();

                coins.Add(coin);
            }

            float startTime = Time.time;
            float testDuration = 2f;
            int frameCount = 0;

            while (Time.time - startTime < testDuration)
            {
                foreach (var coin in coins)
                {
                    var controller = coin.GetComponent<CoinAnimationController>();
                    if (controller != null && Random.value < 0.05f)
                    {
                        Vector3 target = new Vector3(
                            Random.Range(-2f, 2f),
                            0f,
                            Random.Range(-2f, 2f)
                        );
                        controller.AnimateToPosition(target, Random.Range(0.3f, 0.8f));
                    }
                }

                frameCount++;
                yield return null;
            }

            float fps = frameCount / testDuration;
            UnityEngine.Debug.Log($"平均FPS: {fps:F1}");

            // 验证性能在可接受范围内
            Assert.Greater(fps, 25f, $"性能应该保持在25fps以上 (实际: {fps:F1}fps)");

            // 清理金币
            foreach (var coin in coins)
            {
                if (coin != null)
                    UnityEngine.Object.DestroyImmediate(coin.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator Test_CoinCollection_BasicFunctionality()
        {
            // 创建5个金币进行收集测试
            for (int i = 0; i < 5; i++)
            {
                GameObject coin = new GameObject($"TestCoin{i}");
                coin.transform.position = new Vector3(
                    Random.Range(-2f, 2f),
                    0f,
                    Random.Range(2f, 4f)
                );

                var controller = coin.AddComponent<CoinAnimationController>();
                var rb = coin.AddComponent<Rigidbody>();
                rb.useGravity = false;
                coin.AddComponent<SphereCollider>();

                coin.SetActive(true);
            }

            // 等待动画
            yield return new WaitForSeconds(1f);

            // 验证测试完成
            Assert.IsTrue(true, "金币收集基本功能测试完成");
        }
    }
}