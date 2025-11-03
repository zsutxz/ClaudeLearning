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
        /// 飞行动画到目标位置（带抛物线轨迹）
        /// </summary>
        public void FlyTo(Vector3 targetPosition, float duration = 1.5f)
        {
            StopAnimation();
            _currentAnimation = StartCoroutine(FlyToTarget(targetPosition, duration));
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
        /// 飞行动画协程（抛物线轨迹）
        /// </summary>
        private IEnumerator FlyToTarget(Vector3 target, float duration)
        {
            Vector3 start = transform.position;
            float elapsed = 0f;

            // 计算抛物线高度（中点抬高）
            Vector3 midPoint = (start + target) / 2f;
            float arcHeight = Vector3.Distance(start, target) * 0.3f; // 弧线高度为距离的30%
            midPoint.y += arcHeight;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 使用贝塞尔曲线计算位置
                Vector3 a = Vector3.Lerp(start, midPoint, t);
                Vector3 b = Vector3.Lerp(midPoint, target, t);
                Vector3 position = Vector3.Lerp(a, b, t);

                // 添加上下浮动效果
                float floatOffset = Mathf.Sin(t * Mathf.PI * 2f) * 0.1f;
                position.y += floatOffset;

                transform.position = position;

                // 飞行时的旋转效果（更快更活泼）
                transform.Rotate(0, 720f * Time.deltaTime, 0);

                // 轻微的缩放脉冲效果
                float scale = 1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.05f;
                transform.localScale = Vector3.one * scale;

                yield return null;
            }

            transform.position = target;
            transform.localScale = Vector3.one;
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