using UnityEngine;
using CoinAnimation.Animation;

namespace CoinAnimation.Examples
{
    /// <summary>
    /// 最简单的金币演示
    /// 展示基础金币动画的使用
    /// </summary>
    public class BasicCoinDemo : MonoBehaviour
    {
        [Header("演示设置")]
        [SerializeField] private SimpleCoinManager coinManager;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform targetPoint;
        [SerializeField] private Transform collectPoint;

        private void Update()
        {
            // 简单的按键控制
            if (Input.GetKeyDown(KeyCode.M))
            {
                DemoMoveAnimation();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                DemoCollectAnimation();
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                coinManager.ClearAllCoins();
            }
        }

        /// <summary>
        /// 演示移动动画
        /// </summary>
        [ContextMenu("演示移动动画")]
        public void DemoMoveAnimation()
        {
            if (coinManager != null && spawnPoint != null && targetPoint != null)
            {
                coinManager.CreateCoinAnimation(spawnPoint.position, targetPoint.position);
            }
        }

        /// <summary>
        /// 演示收集动画
        /// </summary>
        [ContextMenu("演示收集动画")]
        public void DemoCollectAnimation()
        {
            if (coinManager != null && spawnPoint != null && collectPoint != null)
            {
                coinManager.CreateCollectionAnimation(spawnPoint.position, collectPoint.position);
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 200, 150));
            GUILayout.Label("金币动画演示", GUI.skin.box);

            if (GUILayout.Button("移动动画 (M)"))
            {
                DemoMoveAnimation();
            }

            if (GUILayout.Button("收集动画 (C)"))
            {
                DemoCollectAnimation();
            }

            if (GUILayout.Button("清理金币 (X)"))
            {
                coinManager.ClearAllCoins();
            }

            GUILayout.EndArea();
        }
    }
}