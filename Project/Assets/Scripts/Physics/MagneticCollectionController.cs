using System.Collections.Generic;
using UnityEngine;
using CoinAnimation.Core;
using CoinAnimation.Animation;

namespace CoinAnimation.Physics
{
    /// <summary>
    /// 简化的磁性收集控制器
    /// </summary>
    public class MagneticCollectionController : MonoBehaviour
    {
        [Header("Magnetic Settings")]
        [SerializeField] private float magneticRadius = 5f;
        [SerializeField] private float magneticForce = 10f;
        [SerializeField] private Transform collectionPoint;

        private List<CoinAnimationController> _nearbyCoins = new List<CoinAnimationController>();

        private void Update()
        {
            FindNearbyCoins();
            ApplyMagneticForce();
        }

        private void FindNearbyCoins()
        {
            _nearbyCoins.Clear();

            if (CoinAnimationManager.Instance == null) return;

            Collider[] hitColliders = UnityEngine.Physics.OverlapSphere(collectionPoint.position, magneticRadius);
            foreach (var collider in hitColliders)
            {
                var coin = collider.GetComponent<CoinAnimationController>();
                if (coin != null && coin.CurrentState != CoinAnimationState.Pooled)
                {
                    _nearbyCoins.Add(coin);
                }
            }
        }

        private void ApplyMagneticForce()
        {
            foreach (var coin in _nearbyCoins)
            {
                if (coin.CurrentState == CoinAnimationState.Collecting) continue;

                float distance = Vector3.Distance(coin.transform.position, collectionPoint.position);

                if (distance < magneticRadius)
                {
                    float force = 1f - (distance / magneticRadius);

                    if (distance < 0.5f)
                    {
                        coin.CollectCoin(collectionPoint.position, 0.5f);
                    }
                    else
                    {
                        Vector3 targetPos = Vector3.Lerp(coin.transform.position, collectionPoint.position, force * 0.1f);
                        coin.AnimateToPosition(targetPos, 0.016f);
                    }
                }
            }
        }

        public void SetCollectionPoint(Transform point)
        {
            collectionPoint = point;
        }

        private void OnDrawGizmosSelected()
        {
            if (collectionPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(collectionPoint.position, magneticRadius);
            }
        }
    }
}