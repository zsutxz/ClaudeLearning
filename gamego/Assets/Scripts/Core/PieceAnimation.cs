using UnityEngine;

namespace Gomoku
{
    /// <summary>
    /// 棋子动画控制器
    /// </summary>
    public class PieceAnimation : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float dropHeight = 2f;
        [SerializeField] private float dropDuration = 0.3f;
        [SerializeField] private float bounceScale = 1.2f;
        [SerializeField] private float bounceDuration = 0.15f;
        [SerializeField] private AnimationCurve dropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Vector3 _targetScale;
        private bool _isAnimating;

        private void Awake()
        {
            _targetScale = transform.localScale;
        }

        private void Start()
        {
            PlayDropAnimation();
        }

        /// <summary>
        /// 播放下落动画
        /// </summary>
        public void PlayDropAnimation()
        {
            if (_isAnimating) return;
            StartCoroutine(DropCoroutine());
        }

        private System.Collections.IEnumerator DropCoroutine()
        {
            _isAnimating = true;

            Vector3 startPos = transform.position + Vector3.up * dropHeight;
            Vector3 endPos = transform.position;
            float elapsed = 0f;

            transform.position = startPos;

            // 下落动画
            while (elapsed < dropDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dropDuration;
                transform.position = Vector3.Lerp(startPos, endPos, dropCurve.Evaluate(t));
                yield return null;
            }

            transform.position = endPos;

            // 弹跳缩放动画
            elapsed = 0f;
            while (elapsed < bounceDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / bounceDuration;
                float scale = Mathf.Lerp(bounceScale, 1f, t);
                transform.localScale = _targetScale * scale;
                yield return null;
            }

            transform.localScale = _targetScale;
            _isAnimating = false;
        }

        /// <summary>
        /// 播放获胜闪烁动画
        /// </summary>
        public void PlayWinAnimation()
        {
            StartCoroutine(WinCoroutine());
        }

        private System.Collections.IEnumerator WinCoroutine()
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer == null) yield break;

            Material material = renderer.material;
            Color originalColor = material.color;
            Color flashColor = Color.yellow;

            int flashCount = 3;
            float flashDuration = 0.2f;

            for (int i = 0; i < flashCount; i++)
            {
                float elapsed = 0f;
                while (elapsed < flashDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / flashDuration;
                    material.color = Color.Lerp(originalColor, flashColor, Mathf.Sin(t * Mathf.PI));
                    yield return null;
                }
            }

            material.color = originalColor;
        }
    }
}
