using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class ComboEffectsManager : MonoBehaviour
{
    [Header("Effect Settings")]
    public float effectDuration = 2.0f;
    public float screenShakeDuration = 0.5f;
    public float screenShakeStrength = 0.1f;
    public int particlePoolSize = 10;

    [Header("Combo Level Effects")]
    public ComboEffect bronzeEffect;
    public ComboEffect silverEffect;
    public ComboEffect goldEffect;
    public ComboEffect platinumEffect;

    [Header("UI References")]
    public RectTransform comboText;
    public UnityEngine.UI.Text comboCountText;

    // Particle system pools for better performance
    private Dictionary<ComboManager.ComboLevel, Queue<ParticleSystem>> particlePools;
    private Dictionary<ComboManager.ComboLevel, ComboEffect> effectSettings;

    private Camera mainCamera;
    private Coroutine screenShakeCoroutine;

    [System.Serializable]
    public class ComboEffect
    {
        public ParticleSystem particleEffectPrefab;
        public Color textColor = Color.white;
        public float textScaleMultiplier = 1.0f;
        public float screenShakeMultiplier = 1.0f;
    }

    void Awake()
    {
        mainCamera = Camera.main;
        InitializeParticlePools();
        InitializeEffectSettings();
    }

    void Start()
    {
        // Subscribe to combo events
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.onComboIncreased.AddListener(OnComboIncreased);
            ComboManager.Instance.onComboLevelChanged.AddListener(OnComboLevelChanged);
            ComboManager.Instance.onComboReset.AddListener(OnComboReset);
        }
    }

    /// <summary>
    /// Initializes particle pools for better performance
    /// </summary>
    private void InitializeParticlePools()
    {
        particlePools = new Dictionary<ComboManager.ComboLevel, Queue<ParticleSystem>>();

        // Create pools for each combo level
        CreateParticlePool(ComboManager.ComboLevel.Bronze, bronzeEffect.particleEffectPrefab);
        CreateParticlePool(ComboManager.ComboLevel.Silver, silverEffect.particleEffectPrefab);
        CreateParticlePool(ComboManager.ComboLevel.Gold, goldEffect.particleEffectPrefab);
        CreateParticlePool(ComboManager.ComboLevel.Platinum, platinumEffect.particleEffectPrefab);
    }

    /// <summary>
    /// Creates a particle pool for a specific combo level
    /// </summary>
    /// <param name="level">Combo level</param>
    /// <param name="prefab">Particle system prefab</param>
    private void CreateParticlePool(ComboManager.ComboLevel level, ParticleSystem prefab)
    {
        if (prefab == null) return;

        Queue<ParticleSystem> pool = new Queue<ParticleSystem>();
        particlePools[level] = pool;

        for (int i = 0; i < particlePoolSize; i++)
        {
            ParticleSystem particle = Instantiate(prefab, transform);
            particle.gameObject.SetActive(false);
            pool.Enqueue(particle);
        }
    }

    /// <summary>
    /// Initializes effect settings dictionary for quick lookup
    /// </summary>
    private void InitializeEffectSettings()
    {
        effectSettings = new Dictionary<ComboManager.ComboLevel, ComboEffect>
        {
            { ComboManager.ComboLevel.Bronze, bronzeEffect },
            { ComboManager.ComboLevel.Silver, silverEffect },
            { ComboManager.ComboLevel.Gold, goldEffect },
            { ComboManager.ComboLevel.Platinum, platinumEffect }
        };
    }

    /// <summary>
    /// Called when combo is increased
    /// </summary>
    /// <param name="comboCount">Current combo count</param>
    private void OnComboIncreased(int comboCount)
    {
        if (comboCount > 1) // Only show effects for combos of 2 or more
        {
            PlayComboEffect(comboCount);
            PlayScreenShake(comboCount);
            UpdateComboUI(comboCount);
        }
    }

    /// <summary>
    /// Called when combo level changes
    /// </summary>
    /// <param name="comboLevel">New combo level</param>
    private void OnComboLevelChanged(ComboManager.ComboLevel comboLevel)
    {
        // Additional effects when reaching a new combo level
        switch (comboLevel)
        {
            case ComboManager.ComboLevel.Bronze:
                // Bronze level effect
                break;
            case ComboManager.ComboLevel.Silver:
                // Silver level effect
                break;
            case ComboManager.ComboLevel.Gold:
                // Gold level effect
                break;
            case ComboManager.ComboLevel.Platinum:
                // Platinum level effect
                break;
        }
    }

    /// <summary>
    /// Called when combo is reset
    /// </summary>
    private void OnComboReset()
    {
        HideComboUI();
    }

    /// <summary>
    /// Plays the appropriate combo effect based on combo count
    /// </summary>
    /// <param name="comboCount">Current combo count</param>
    private void PlayComboEffect(int comboCount)
    {
        ComboManager.ComboLevel level = ComboManager.Instance.GetComboLevel(comboCount);
        ComboEffect effect = GetComboEffectForLevel(level);

        // Get particle system from pool
        ParticleSystem particleEffect = GetParticleFromPool(level);

        if (particleEffect != null)
        {
            // Position the effect at the center of the screen or at a specific world position
            particleEffect.transform.position = GetEffectPosition();
            particleEffect.gameObject.SetActive(true);

            // Play the particle effect
            if (!particleEffect.isPlaying)
            {
                particleEffect.Play();
            }

            // Stop the effect after duration and return to pool
            StartCoroutine(StopEffectAfterDelay(particleEffect, effectDuration, level));
        }

        // Additional visual effects
        AnimateComboText(comboCount, effect);
    }

    /// <summary>
    /// Gets a particle system from the pool for the specified level
    /// </summary>
    /// <param name="level">Combo level</param>
    /// <returns>Particle system from pool, or null if none available</returns>
    private ParticleSystem GetParticleFromPool(ComboManager.ComboLevel level)
    {
        if (particlePools.TryGetValue(level, out Queue<ParticleSystem> pool) && pool.Count > 0)
        {
            return pool.Dequeue();
        }
        return null;
    }

    /// <summary>
    /// Returns a particle system to the pool
    /// </summary>
    /// <param name="particleSystem">Particle system to return</param>
    /// <param name="level">Combo level of the particle system</param>
    private void ReturnParticleToPool(ParticleSystem particleSystem, ComboManager.ComboLevel level)
    {
        if (particlePools.TryGetValue(level, out Queue<ParticleSystem> pool))
        {
            particleSystem.Stop();
            particleSystem.gameObject.SetActive(false);
            pool.Enqueue(particleSystem);
        }
        else
        {
            // If no pool exists, destroy the particle system
            Destroy(particleSystem.gameObject);
        }
    }

    /// <summary>
    /// Plays screen shake effect that intensifies with combo level
    /// </summary>
    /// <param name="comboCount">Current combo count</param>
    private void PlayScreenShake(int comboCount)
    {
        ComboManager.ComboLevel level = ComboManager.Instance.GetComboLevel(comboCount);
        ComboEffect effect = GetComboEffectForLevel(level);

        // Cancel any existing screen shake
        if (screenShakeCoroutine != null)
        {
            StopCoroutine(screenShakeCoroutine);
        }

        // Start new screen shake with intensity based on combo level
        float shakeStrength = screenShakeStrength * effect.screenShakeMultiplier;
        screenShakeCoroutine = StartCoroutine(ScreenShake(screenShakeDuration, shakeStrength));
    }

    /// <summary>
    /// Coroutine to shake the screen
    /// </summary>
    /// <param name="duration">Duration of shake</param>
    /// <param name="strength">Strength of shake</param>
    private IEnumerator ScreenShake(float duration, float strength)
    {
        if (mainCamera == null) yield break;

        Vector3 originalPosition = mainCamera.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percentComplete = elapsed / duration;
            float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);

            // Generate random values for shake
            float x = Random.value * 2.0f - 1.0f;
            float y = Random.value * 2.0f - 1.0f;
            x *= strength * damper;
            y *= strength * damper;

            mainCamera.transform.localPosition = new Vector3(x, y, originalPosition.z);

            yield return null;
        }

        mainCamera.transform.localPosition = originalPosition;
        screenShakeCoroutine = null;
    }

    /// <summary>
    /// Updates the combo UI with the current combo count
    /// </summary>
    /// <param name="comboCount">Current combo count</param>
    private void UpdateComboUI(int comboCount)
    {
        if (comboText != null && comboCountText != null)
        {
            comboText.gameObject.SetActive(true);
            comboCountText.text = "x" + comboCount;
        }
    }

    /// <summary>
    /// Hides the combo UI
    /// </summary>
    private void HideComboUI()
    {
        if (comboText != null)
        {
            comboText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Animates the combo text with scaling and color effects
    /// </summary>
    /// <param name="comboCount">Current combo count</param>
    /// <param name="effect">Combo effect settings</param>
    private void AnimateComboText(int comboCount, ComboEffect effect)
    {
        if (comboText != null && comboCountText != null)
        {
            // Update text color
            comboCountText.color = effect.textColor;

            // Animate scale
            comboText.DOScale(comboText.localScale * effect.textScaleMultiplier, 0.2f)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => comboText.DOScale(Vector3.one, 0.1f));
        }
    }

    /// <summary>
    /// Gets the appropriate combo effect for the given level
    /// </summary>
    /// <param name="level">Combo level</param>
    /// <returns>Combo effect settings</returns>
    private ComboEffect GetComboEffectForLevel(ComboManager.ComboLevel level)
    {
        if (effectSettings.TryGetValue(level, out ComboEffect effect))
        {
            return effect;
        }
        return bronzeEffect; // Default to bronze effect
    }

    /// <summary>
    /// Gets the position where effects should be played
    /// </summary>
    /// <returns>Position for effects</returns>
    private Vector3 GetEffectPosition()
    {
        // Default to center of screen
        if (mainCamera != null)
        {
            return mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Coroutine to stop particle effect after delay and return to pool
    /// </summary>
    /// <param name="particleSystem">Particle system to stop</param>
    /// <param name="delay">Delay in seconds</param>
    /// <param name="level">Combo level of the particle system</param>
    private IEnumerator StopEffectAfterDelay(ParticleSystem particleSystem, float delay, ComboManager.ComboLevel level)
    {
        yield return new WaitForSeconds(delay);
        if (particleSystem != null)
        {
            ReturnParticleToPool(particleSystem, level);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.onComboIncreased.RemoveListener(OnComboIncreased);
            ComboManager.Instance.onComboLevelChanged.RemoveListener(OnComboLevelChanged);
            ComboManager.Instance.onComboReset.RemoveListener(OnComboReset);
        }
    }
}