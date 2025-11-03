using System.Collections;
using UnityEngine;

namespace CoinAnimation.Animation
{
    /// <summary>
    /// 基础金币动画 - 最简单的实现
    /// 只包含移动和收集两个核心功能
    /// </summary>
    public class BasicCoinAnimation : MonoBehaviour
    {
        private Coroutine _currentAnimation;

        /// <summary>
        /// 移动到指定位置
        /// </summary>
        public void MoveTo(Vector3 targetPosition, float duration = 1f)
        {
            StopAnimation();
            _currentAnimation = StartCoroutine(MoveToTarget(targetPosition, duration));
        }

        /// <summary>
        /// 收集金币到目标点
        /// </summary>
        public void Collect(Vector3 collectPoint, float duration = 0.5f)
        {
            StopAnimation();
            _currentAnimation = StartCoroutine(CollectAnimation(collectPoint, duration));
        }

        /// <summary>
        /// 停止当前动画
        /// </summary>
        public void StopAnimation()
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
                _currentAnimation = null;
            }
        }

        /// <summary>
        /// 重置金币
        /// </summary>
        public void Reset()
        {
            StopAnimation();
            transform.localScale = Vector3.one;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 移动动画协程
        /// </summary>
        private IEnumerator MoveToTarget(Vector3 target, float duration)
        {
            Vector3 start = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t * (3f - 2f * t); // 简单缓动

                transform.position = Vector3.Lerp(start, target, t);
                transform.Rotate(0, 360f * Time.deltaTime, 0);

                yield return null;
            }

            transform.position = target;
            _currentAnimation = null;
        }

        /// <summary>
        /// 收集动画协程
        /// </summary>
        private IEnumerator CollectAnimation(Vector3 collectPoint, float duration)
        {
            Vector3 start = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 移动到收集点
                transform.position = Vector3.Lerp(start, collectPoint, t);
                // 旋转
                transform.Rotate(0, 360f * Time.deltaTime, 0);
                // 缩放效果
                transform.localScale = Vector3.one * (1f + t);

                yield return null;
            }

            // 收集完成，隐藏金币
            gameObject.SetActive(false);
            transform.localScale = Vector3.one;
            _currentAnimation = null;
        }
    }
}