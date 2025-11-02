using UnityEngine;
using CoinAnimation.Animation;

namespace CoinAnimation.Tests
{
    /// <summary>
    /// 测试CoinAnimationManager预制体检测修复
    /// </summary>
    public class CoinAnimationManagerTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestOnStart = true;
        [SerializeField] private GameObject testPrefab;

        private void Start()
        {
            if (runTestOnStart)
            {
                TestPrefabDetection();
            }
        }

        [ContextMenu("Test Prefab Detection")]
        public void TestPrefabDetection()
        {
            Debug.Log("=== CoinAnimationManager 预制体检测测试 ===");

            // 测试管理器初始化
            var manager = CoinAnimationManager.Instance;

            if (manager != null)
            {
                Debug.Log("✅ CoinAnimationManager 实例创建成功");

                // 测试对象池初始化
                if (manager.IsPoolInitialized)
                {
                    Debug.Log("✅ 对象池初始化成功");

                    // 测试从池中获取硬币
                    GameObject coin = manager.GetCoinFromPool();
                    if (coin != null)
                    {
                        Debug.Log($"✅ 成功从池中获取硬币: {coin.name}");

                        // 检查控制器类型
                        var controller = coin.GetComponentInChildren<UGUICoinAnimationController>();
                        if (controller != null)
                        {
                            Debug.Log("✅ 检测到 UGUICoinAnimationController");
                        }

                        var standardController = coin.GetComponentInChildren<CoinAnimationController>();
                        if (standardController != null)
                        {
                            Debug.Log("✅ 检测到 CoinAnimationController");
                        }

                        // 返回硬币到池中
                        manager.ReturnCoinToPool(coin);
                        Debug.Log("✅ 硬币已返回到池中");
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ 无法从池中获取硬币");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ 对象池未初始化");
                }
            }
            else
            {
                Debug.LogError("❌ CoinAnimationManager 实例创建失败");
            }

            Debug.Log("=== 测试完成 ===");
        }

        [ContextMenu("Test Animation Session")]
        public void TestAnimationSession()
        {
            Debug.Log("=== 动画会话测试 ===");

            var manager = CoinAnimationManager.Instance;
            if (manager != null)
            {
                // 创建测试目标
                GameObject target = new GameObject("TestTarget");
                target.transform.position = Vector3.zero;

                try
                {
                    // 测试启动动画会话
                    System.Guid sessionId = manager.StartCoinAnimation(target.transform, 3);
                    Debug.Log($"✅ 动画会话启动成功: {sessionId}");

                    // 等待几秒后停止
                    Invoke(nameof(StopTestAnimation), 3f);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"❌ 动画会话测试失败: {e.Message}");
                }
                finally
                {
                    Destroy(target, 5f);
                }
            }

            Debug.Log("=== 动画会话测试完成 ===");
        }

        private void StopTestAnimation()
        {
            // 这个方法会被Invoke调用
        }
    }
}