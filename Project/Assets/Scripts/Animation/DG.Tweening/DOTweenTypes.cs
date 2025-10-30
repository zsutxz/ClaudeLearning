using System;
using UnityEngine;

namespace DG.Tweening
{
    // Mock DOTween types to allow compilation
    // These should be replaced with actual DOTween package when properly installed

    public enum Ease
    {
        Linear,
        InSine,
        OutSine,
        InOutSine,
        InQuad,
        OutQuad,
        InOutQuad,
        InBack,
        OutBack,
        InOutBack,
        OutBounce,
        OutCirc
    }

    public enum UpdateType
    {
        Normal,
        Late,
        Fixed,
        Manual
    }

    public enum RotateMode
    {
        Fast,
        FastBeyond360,
        WorldAxisAdd,
        LocalAxisAdd
    }

    public enum PathType
    {
        Linear,
        CatmullRom
    }

    public enum LoopType
    {
        Restart,
        Yoyo,
        Incremental
    }

    public class Tween
    {
        public bool active = true;
        public bool playing = false;

        public Tween SetEase(Ease ease) { return this; }
        public Tween SetLoops(int loops, LoopType loopType = LoopType.Restart) { return this; }
        public Tween SetUpdate(UpdateType updateType) { return this; }
        public Tween From(bool from = true) { return this; }
        public Tween From(float fromValue) { return this; }
        public Tween OnComplete(Action callback) { return this; }
        public Tween OnUpdate(Action callback) { return this; }
        public Tween OnStart(Action callback) { return this; }
        public Tween Kill(bool complete = false) { return this; }
        public bool IsPlaying() { return playing; }
        public bool IsActive() { return active; }
        public void Complete() { }
        public void Pause() { playing = false; }
        public void Play() { playing = true; }

        // Additional properties and methods for compatibility
        public float Delay(float delay) { return delay; }
        public float Duration() { return 1f; }
        public float ElapsedPercentage() { return 1f; }
        public Tween SetId(object id) { return this; }
        public int Loops() { return 1; }
    }

    public class Sequence : Tween
    {
        public static Sequence Create() { return new Sequence(); }
        public Sequence Append(Tween tween) { return this; }
        public Sequence Join(Tween tween) { return this; }
        public Sequence Insert(float atPosition, Tween tween) { return this; }
        public Sequence AppendInterval(float interval) { return this; }
        public Sequence Prepend(Tween tween) { return this; }
        public Sequence PrependInterval(float interval) { return this; }
    }

    public static class DOTween
    {
        public static Sequence Sequence() { return new Sequence(); }
        public static float timeScale = 1f;
        public static float globalTimeScale = 1f;
        public static bool useSmoothDeltaTime = false;
        public static bool safeMode = false;
        public static string Version = "1.2.705";
        public static LogBehaviour logBehaviour = LogBehaviour.ErrorsOnly;
        public static AutoPlay defaultAutoPlay = AutoPlay.All;

        public static void SetTweensCapacity(int capacity, int recycleCapacity) { }
        public static void CompleteAll(bool withCallbacks = true) { }
        public static void KillAll(bool complete = false) { }
        public static void PauseAll() { }
        public static void PlayAll() { }
        public static bool IsTweening(object targetOrId = null) { return false; }

        public static event Action<Tween> OnTweenCreated;
        public static event Action<Tween> OnTweenComplete;

        // Static convenience methods
        public static Tweener To(DOGetter<object> getter, DOSetter<object> setter, object endValue, float duration)
        {
            return new Tweener();
        }
    }

    // Delegate types for To method
    public delegate T DOGetter<T>();
    public delegate void DOSetter<T>(T value);

    public class Tweener : Tween
    {
        public Tweener SetEase(Ease ease) { return this; }
        public Tweener SetLoops(int loops, LoopType loopType = LoopType.Restart) { return this; }
        public Tweener SetUpdate(UpdateType updateType) { return this; }
        public Tweener From(bool from = true) { return this; }
        public Tweener OnComplete(Action callback) { return this; }
        public Tweener OnUpdate(Action callback) { return this; }
        public Tweener Kill(bool complete = false) { return this; }
    }

    public static class ShortcutExtensions
    {
        public static Tween DOMove(this Transform target, Vector3 endValue, float duration, bool snapping = false)
        {
            // Mock implementation - just move immediately
            if (Application.isPlaying)
                target.position = endValue;
            return new Tween();
        }

        public static Tween DOMoveX(this Transform target, float endValue, float duration, bool snapping = false)
        {
            if (Application.isPlaying)
                target.position = new Vector3(endValue, target.position.y, target.position.z);
            return new Tween();
        }

        public static Tween DOLocalRotate(this Transform target, Vector3 endValue, float duration, RotateMode mode = RotateMode.Fast)
        {
            if (Application.isPlaying)
                target.localRotation = Quaternion.Euler(endValue);
            return new Tween();
        }

        public static Tween DOScale(this Transform target, Vector3 endValue, float duration)
        {
            if (Application.isPlaying)
                target.localScale = endValue;
            return new Tween();
        }

        public static Tween DOLocalPath(this Transform target, Vector3[] path, float duration, PathType pathType = PathType.Linear, PathMode pathMode = PathMode.Full3D, int resolution = 10, bool gizmoColor = false)
        {
            // Mock path implementation - just move to last point
            if (Application.isPlaying && path.Length > 0)
                target.position = path[path.Length - 1];
            return new Tween();
        }

        public static Tween SetEase(this Tween tween, Ease ease) { return tween; }
        public static Tween SetLookAt(this Tween tween, float lookAhead, Vector3 forwardDirection) { return tween; }
        public static Tween SetLookAt(this Tween tween, float lookAhead) { return tween; }
        public static Tween From(this Tween tween, Vector3 fromValue = default) { return tween; }
        public static Tween OnComplete(this Tween tween, Action callback) { return tween; }
        public static Tween Join(this Sequence sequence, Tween tween) { return sequence; }
        public static Tween Append(this Sequence sequence, Tween tween) { return sequence; }
        public static Tween OnComplete(this Sequence sequence, Action callback) { return sequence; }
        public static Tween DOFade(this Transform target, float endValue, float duration)
        {
            // Mock implementation for UI elements
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup != null && Application.isPlaying)
                canvasGroup.alpha = endValue;
            return new Tween();
        }

        public static Tween DOKill(this Transform target, bool complete = false)
        {
            // Mock implementation - nothing to kill in mock version
            return new Tween();
        }

        public static Tween DOShakePosition(this Transform target, float duration, Vector3 strength, int vibrato = 10, float randomness = 90f, bool ignoreTimeScale = true, bool fadeOut = true)
        {
            // Mock shake implementation - just apply a small random offset
            if (Application.isPlaying)
            {
                Vector3 randomOffset = new Vector3(
                    UnityEngine.Random.Range(-strength.x, strength.x),
                    UnityEngine.Random.Range(-strength.y, strength.y),
                    UnityEngine.Random.Range(-strength.z, strength.z)
                ) * 0.1f;
                target.position += randomOffset;
            }
            return new Tween();
        }

        public static Tweener To<T>(DOGetter<T> getter, DOSetter<T> setter, T endValue, float duration)
        {
            // Mock generic To method implementation
            return new Tweener();
        }

        public static Tweener To(DOGetter<object> getter, DOSetter<object> setter, object endValue, float duration)
        {
            // Mock To method implementation
            return new Tweener();
        }

        // Transform extension methods
        public static Tweener DOLocalMoveY(this Transform target, float endValue, float duration, bool snapping = false)
        {
            Vector3 currentPos = target.localPosition;
            return To(
                () => target.localPosition.y,
                y => target.localPosition = new Vector3(currentPos.x, y, currentPos.z),
                endValue, duration
            );
        }

        public static Tweener DOPath(this Transform target, Vector3[] path, float duration, PathType pathType = PathType.Linear, PathMode pathMode = PathMode.Full3D, int pathResolution = 10, bool gizmoColor = false)
        {
            // Mock DOPath implementation - just move to the last position in the path
            Vector3 targetPos = path.Length > 0 ? path[path.Length - 1] : target.position;
            return (Tweener)DOMove(target, targetPos, duration);
        }

        public static Tweener DORotate(this Transform target, Vector3 endValue, float duration, RotateMode rotateMode = RotateMode.Fast)
        {
            return To(
                () => target.eulerAngles,
                (Vector3 value) => target.eulerAngles = value,
                endValue, duration
            );
        }
    }

    public enum PathMode
    {
        Ignore,
        Full3D,
        TopDown2D,
        Sidescroller2D
    }

    public enum LogBehaviour
    {
        Default,
        ErrorsOnly,
        Verbose
    }

    public enum AutoPlay
    {
        All,
        None,
        AutoPlayTweeners
    }
}