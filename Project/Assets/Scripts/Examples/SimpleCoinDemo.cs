using System.Collections;
using UnityEngine;
using CoinAnimation.Core;
using CoinAnimation.Animation;
using CoinAnimation.Physics;

namespace CoinAnimation.Examples
{
    /// <summary>
    /// 简单的金币演示脚本
    /// </summary>
    public class SimpleCoinDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform collectionPoint;
        [SerializeField] private int coinCount = 10;

        private void Start()
        {
            StartCoroutine(DemoSequence());
        }

        private IEnumerator DemoSequence()
        {
            // 等待1秒
            yield return new WaitForSeconds(1f);

            // 创建多个金币
            for (int i = 0; i < coinCount; i++)
            {
                CreateCoin();
                yield return new WaitForSeconds(0.2f);
            }

            // 等待2秒后创建磁性收集器
            yield return new WaitForSeconds(2f);
            CreateMagneticCollector();

            Debug.Log("简单金币演示完成！");
        }

        private void CreateCoin()
        {
            if (coinPrefab == null || spawnPoint == null) return;

            GameObject coin = Instantiate(coinPrefab, spawnPoint.position, Random.rotation);
            coin.SetActive(true);

            // 添加金币控制器（如果预制体没有）
            var controller = coin.GetComponent<CoinAnimationController>();
            if (controller == null)
            {
                controller = coin.AddComponent<CoinAnimationController>();
            }

            // 给金币一个随机的小推力
            Rigidbody rb = coin.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(new Vector3(
                    Random.Range(-2f, 2f),
                    Random.Range(1f, 3f),
                    Random.Range(-2f, 2f)
                ), ForceMode.Impulse);
            }
        }

        private void CreateMagneticCollector()
        {
            if (collectionPoint == null) return;

            GameObject magneticObj = new GameObject("MagneticCollector");
            magneticObj.transform.position = collectionPoint.position;

            var magneticController = magneticObj.AddComponent<MagneticCollectionController>();
            magneticController.SetCollectionPoint(collectionPoint);

            Debug.Log("磁性收集器已创建");
        }

        private void OnDrawGizmos()
        {
            if (spawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
            }

            if (collectionPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(collectionPoint.position, 0.5f);
            }
        }
    }
}