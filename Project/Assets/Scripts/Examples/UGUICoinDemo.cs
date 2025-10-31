using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CoinAnimation.Animation;
using CoinAnimation.Core;

namespace CoinAnimation.Examples
{
    /// <summary>
    /// UGUI金币动画演示脚本
    /// 展示UGUI金币动画系统的各种功能
    /// </summary>
    public class UGUICoinDemo : MonoBehaviour
    {
        [Header("UGUI Coin Prefab")]
        [SerializeField] private GameObject uguiCoinPrefab;

        [Header("Animation Targets")]
        [SerializeField] private Transform coinSpawnPoint;
        [SerializeField] private Transform targetPoint;
        [SerializeField] private Transform collectionPoint;

        [Header("Demo Settings")]
        [SerializeField] private int numberOfCoins = 10;
        [SerializeField] private float spawnInterval = 0.2f;
        [SerializeField] private float animationDuration = 2f;
        [SerializeField] private bool autoStartDemo = true;

        [Header("UI References")]
        [SerializeField] private Button spawnButton;
        [SerializeField] private Button collectAllButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Text statusText;

        private GameObject[] _spawnedCoins;
        private UGUICoinAnimationController[] _coinControllers;
        private int _activeCoins = 0;
        private bool _demoRunning = false;

        private void Start()
        {
            InitializeDemo();
            if (autoStartDemo)
            {
                StartCoroutine(AutoDemoCoroutine());
            }
        }

        /// <summary>
        /// 初始化演示
        /// </summary>
        private void InitializeDemo()
        {
            // 初始化数组
            _spawnedCoins = new GameObject[numberOfCoins];
            _coinControllers = new UGUICoinAnimationController[numberOfCoins];

            // 设置UI按钮事件
            if (spawnButton != null)
                spawnButton.onClick.AddListener(SpawnCoins);

            if (collectAllButton != null)
                collectAllButton.onClick.AddListener(CollectAllCoins);

            if (resetButton != null)
                resetButton.onClick.AddListener(ResetDemo);

            // 更新状态文本
            UpdateStatusText("UGUI Coin Demo Ready - Press Spawn or wait for auto-demo");
        }

        /// <summary>
        /// 自动演示协程
        /// </summary>
        private IEnumerator AutoDemoCoroutine()
        {
            yield return new WaitForSeconds(1f);

            UpdateStatusText("Starting UGUI Coin Auto-Demo...");

            // 生成金币
            SpawnCoins();
            yield return new WaitForSeconds(numberOfCoins * spawnInterval + 1f);

            // 移动金币到目标点
            MoveCoinsToTarget();
            yield return new WaitForSeconds(animationDuration + 0.5f);

            // 收集金币
            CollectAllCoins();
            yield return new WaitForSeconds(animationDuration + 1f);

            UpdateStatusText("UGUI Coin Demo Complete! All animations working correctly.");
        }

        /// <summary>
        /// 生成金币
        /// </summary>
        public void SpawnCoins()
        {
            if (_demoRunning) return;
            _demoRunning = true;

            StartCoroutine(SpawnCoinsCoroutine());
        }

        private IEnumerator SpawnCoinsCoroutine()
        {
            _activeCoins = 0;
            UpdateStatusText($"Spawning {numberOfCoins} UGUI coins...");

            for (int i = 0; i < numberOfCoins; i++)
            {
                if (uguiCoinPrefab != null)
                {
                    // 在Canvas中生成金币
                    Canvas canvas = FindObjectOfType<Canvas>();
                    if (canvas != null)
                    {
                        GameObject coin = Instantiate(uguiCoinPrefab, canvas.transform);
                        _spawnedCoins[i] = coin;

                        // 获取动画控制器
                        UGUICoinAnimationController controller = coin.GetComponent<UGUICoinAnimationController>();
                        _coinControllers[i] = controller;

                        if (controller != null)
                        {
                            // 设置初始位置
                            if (coinSpawnPoint != null)
                            {
                                Vector2 spawnPosition = GetScreenPointToCanvasPosition(coinSpawnPoint.position);
                                controller.RectTransform.anchoredPosition = spawnPosition;
                            }
                            else
                            {
                                // 默认位置
                                controller.RectTransform.anchoredPosition = new Vector2(-200 + i * 30, 100);
                            }

                            // 设置随机大小以增加视觉多样性
                            float randomScale = 0.8f + Random.value * 0.4f;
                            controller.RectTransform.localScale = Vector3.one * randomScale;

                            // 添加状态变化监听
                            controller.OnStateChanged += OnCoinStateChanged;

                            _activeCoins++;
                        }
                    }
                }

                yield return new WaitForSeconds(spawnInterval);
            }

            UpdateStatusText($"Spawned {_activeCoins} UGUI coins successfully!");
            _demoRunning = false;
        }

        /// <summary>
        /// 移动金币到目标点
        /// </summary>
        public void MoveCoinsToTarget()
        {
            UpdateStatusText("Moving UGUI coins to target position...");

            for (int i = 0; i < _activeCoins; i++)
            {
                if (_coinControllers[i] != null)
                {
                    Vector2 targetPosition;

                    if (targetPoint != null)
                    {
                        targetPosition = GetScreenPointToCanvasPosition(targetPoint.position);
                    }
                    else
                    {
                        // 默认目标位置
                        targetPosition = new Vector2(-100 + i * 25, -50);
                    }

                    // 添加随机延迟以创造波浪效果
                    float delay = i * 0.05f;
                    StartCoroutine(DelayedMoveToPosition(_coinControllers[i], targetPosition, delay));
                }
            }
        }

        private IEnumerator DelayedMoveToPosition(UGUICoinAnimationController controller, Vector2 targetPosition, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (controller != null && controller.gameObject.activeInHierarchy)
            {
                controller.AnimateToPosition(targetPosition, animationDuration);
            }
        }

        /// <summary>
        /// 收集所有金币
        /// </summary>
        public void CollectAllCoins()
        {
            UpdateStatusText("Collecting UGUI coins...");

            Vector2 collectionPosition;

            if (collectionPoint != null)
            {
                collectionPosition = GetScreenPointToCanvasPosition(collectionPoint.position);
            }
            else
            {
                // 默认收集位置
                collectionPosition = Vector2.zero;
            }

            for (int i = 0; i < _activeCoins; i++)
            {
                if (_coinControllers[i] != null)
                {
                    // 添加随机延迟以创造连锁收集效果
                    float delay = i * 0.1f;
                    StartCoroutine(DelayedCollectCoin(_coinControllers[i], collectionPosition, delay));
                }
            }
        }

        private IEnumerator DelayedCollectCoin(UGUICoinAnimationController controller, Vector2 collectionPosition, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (controller != null && controller.gameObject.activeInHierarchy)
            {
                controller.CollectCoin(collectionPosition, animationDuration);
            }
        }

        /// <summary>
        /// 重置演示
        /// </summary>
        public void ResetDemo()
        {
            UpdateStatusText("Resetting UGUI Coin Demo...");

            // 停止所有协程
            StopAllCoroutines();

            // 销毁生成的金币
            for (int i = 0; i < _spawnedCoins.Length; i++)
            {
                if (_spawnedCoins[i] != null)
                {
                    if (_coinControllers[i] != null)
                    {
                        _coinControllers[i].OnStateChanged -= OnCoinStateChanged;
                    }
                    DestroyImmediate(_spawnedCoins[i]);
                    _spawnedCoins[i] = null;
                    _coinControllers[i] = null;
                }
            }

            _activeCoins = 0;
            _demoRunning = false;

            UpdateStatusText("UGUI Coin Demo Reset Complete");
        }

        /// <summary>
        /// 金币状态变化回调
        /// </summary>
        private void OnCoinStateChanged(object sender, CoinAnimationEventArgs e)
        {
            // 可以在这里添加状态变化时的视觉效果
            Debug.Log($"UGUI Coin state changed: {e.PreviousState} -> {e.CurrentState}");
        }

        /// <summary>
        /// 将世界坐标转换为Canvas坐标
        /// </summary>
        private Vector2 GetScreenPointToCanvasPosition(Vector3 worldPosition)
        {
            Camera camera = Camera.main;
            if (camera == null) camera = Camera.current;

            if (camera != null)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldPosition);
                Canvas canvas = FindObjectOfType<Canvas>();

                if (canvas != null && canvas.transform as RectTransform != null)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvas.transform as RectTransform,
                        screenPoint,
                        camera,
                        out Vector2 localPoint
                    );
                    return localPoint;
                }
            }

            return Vector2.zero;
        }

        /// <summary>
        /// 更新状态文本
        /// </summary>
        private void UpdateStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            Debug.Log($"[UGUICoinDemo] {message}");
        }

        private void OnDestroy()
        {
            // 清理事件监听
            for (int i = 0; i < _coinControllers.Length; i++)
            {
                if (_coinControllers[i] != null)
                {
                    _coinControllers[i].OnStateChanged -= OnCoinStateChanged;
                }
            }
        }

        #region Inspector Helper Methods

        /// <summary>
        /// 在Inspector中快速测试单个金币动画
        /// </summary>
        [ContextMenu("Test Single Coin Animation")]
        private void TestSingleCoinAnimation()
        {
            if (uguiCoinPrefab != null)
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    GameObject coin = Instantiate(uguiCoinPrefab, canvas.transform);
                    UGUICoinAnimationController controller = coin.GetComponent<UGUICoinAnimationController>();

                    if (controller != null)
                    {
                        controller.RectTransform.anchoredPosition = Vector2.zero;

                        // 测试移动动画
                        controller.AnimateToPosition(new Vector2(200, 100), 2f);

                        // 2秒后测试收集动画
                        StartCoroutine(TestCollectAnimation(controller));
                    }
                }
            }
        }

        private IEnumerator TestCollectAnimation(UGUICoinAnimationController controller)
        {
            yield return new WaitForSeconds(2.5f);

            if (controller != null && controller.gameObject.activeInHierarchy)
            {
                controller.CollectCoin(Vector2.zero, 1.5f);
            }
        }

        #endregion
    }
}