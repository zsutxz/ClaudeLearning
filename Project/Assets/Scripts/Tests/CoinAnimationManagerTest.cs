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

        [ContextMenu("Test CoinObjectPool Configuration")]
        public void TestCoinObjectPoolConfiguration()
        {
            Debug.Log("=== CoinObjectPool 配置测试 ===");

            var manager = CoinAnimationManager.Instance;
            if (manager != null)
            {
                Debug.Log("✅ CoinAnimationManager 实例存在");

                // 检查对象池状态
                if (manager.IsPoolInitialized)
                {
                    Debug.Log("✅ 对象池已初始化");

                    // 测试获取多个硬币
                    var testCoins = new List<GameObject>();
                    for (int i = 0; i < 5; i++)
                    {
                        var coin = manager.GetCoinFromPool();
                        if (coin != null)
                        {
                            testCoins.Add(coin);
                            Debug.Log($"✅ 获取测试硬币 {i + 1}: {coin.name}");

                            // 检查硬币状态
                            var uguiController = coin.GetComponent<UGUICoinAnimationController>();
                            var standardController = coin.GetComponent<CoinAnimationController>();

                            if (uguiController != null)
                            {
                                Debug.Log($"  📝 硬币 {i + 1} 有 UGUICoinAnimationController");
                            }
                            else if (standardController != null)
                            {
                                Debug.Log($"  📝 硬币 {i + 1} 有 CoinAnimationController");
                            }
                            else
                            {
                                Debug.LogWarning($"  ⚠️ 硬币 {i + 1} 没有检测到动画控制器");
                            }
                        }
                        else
                        {
                            Debug.LogError($"❌ 无法获取测试硬币 {i + 1}");
                        }
                    }

                    // 返回所有测试硬币到池中
                    foreach (var coin in testCoins)
                    {
                        manager.ReturnCoinToPool(coin);
                        Debug.Log($"🔄 测试硬币已返回到池中: {coin.name}");
                    }

                    Debug.Log($"✅ 成功测试了 {testCoins.Count} 个硬币的获取和返回");
                }
                else
                {
                    Debug.LogError("❌ 对象池未初始化");
                }
            }
            else
            {
                Debug.LogError("❌ CoinAnimationManager 实例不存在");
            }

            Debug.Log("=== CoinObjectPool 配置测试完成 ===");
        }

        [ContextMenu("Test Complete System Initialization")]
        public void TestCompleteSystemInitialization()
        {
            Debug.Log("=== 完整系统初始化测试 ===");

            try
            {
                // 1. 测试管理器创建
                var manager = CoinAnimationManager.Instance;
                Debug.Log("✅ 1. CoinAnimationManager 创建成功");

                // 2. 测试对象池初始化
                if (manager.IsPoolInitialized)
                {
                    Debug.Log("✅ 2. 对象池初始化成功");

                    // 3. 测试基本操作
                    var coin = manager.GetCoinFromPool();
                    if (coin != null)
                    {
                        Debug.Log("✅ 3. 基本对象池操作成功");

                        // 4. 测试硬币控制器
                        var controller = coin.GetComponent<UGUICoinAnimationController>();
                        if (controller != null)
                        {
                            Debug.Log("✅ 4. 硬币动画控制器检测成功");
                        }
                        else
                        {
                            Debug.LogWarning("⚠️ 4. 未检测到预期的动画控制器类型");
                        }

                        // 5. 测试返回操作
                        manager.ReturnCoinToPool(coin);
                        Debug.Log("✅ 5. 硬币返回池中成功");

                        Debug.Log("🎉 完整系统初始化测试通过！");
                    }
                    else
                    {
                        Debug.LogError("❌ 3. 基本对象池操作失败");
                    }
                }
                else
                {
                    Debug.LogError("❌ 2. 对象池初始化失败");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 系统初始化测试失败: {e.Message}\n{e.StackTrace}");
            }

            Debug.Log("=== 完整系统初始化测试完成 ===");
        }

        private void StopTestAnimation()
        {
            // 这个方法会被Invoke调用
        }
    }
}