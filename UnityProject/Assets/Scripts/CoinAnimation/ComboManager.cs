using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class ComboManager : MonoBehaviour
{
    [Header("Combo Settings")]
    [Tooltip("Time window in seconds for consecutive coin collections to count as combo")]
    public float comboTimeWindow = 2.0f;

    [Header("Combo Levels")]
    [Tooltip("Bronze level: minimum combo count")]
    public int bronzeLevel = 2;
    [Tooltip("Silver level: minimum combo count")]
    public int silverLevel = 5;
    [Tooltip("Gold level: minimum combo count")]
    public int goldLevel = 10;
    [Tooltip("Platinum level: minimum combo count")]
    public int platinumLevel = 20;

    [Header("Events")]
    public UnityEvent<int> onComboIncreased;
    public UnityEvent<ComboLevel> onComboLevelChanged;
    public UnityEvent onComboReset;

    public enum ComboLevel
    {
        None,
        Bronze,
        Silver,
        Gold,
        Platinum
    }

    private static ComboManager instance;
    private int currentCombo = 0;
    private float lastCoinCollectionTime = 0f;
    private Coroutine comboResetCoroutine;

    public static ComboManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject comboManagerObject = new GameObject("ComboManager");
                instance = comboManagerObject.AddComponent<ComboManager>();
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Registers a coin collection and updates the combo counter if within time window
    /// </summary>
    public void RegisterCoinCollection()
    {
        float currentTime = Time.time;

        // Check if this collection is within the combo time window
        if (currentTime - lastCoinCollectionTime <= comboTimeWindow)
        {
            // Increment combo
            currentCombo++;

            // Cancel any existing reset coroutine
            if (comboResetCoroutine != null)
            {
                StopCoroutine(comboResetCoroutine);
            }

            // Start new reset coroutine
            comboResetCoroutine = StartCoroutine(ResetComboAfterDelay(comboTimeWindow));

            // Notify listeners
            onComboIncreased?.Invoke(currentCombo);

            // Check if combo level changed
            ComboLevel newLevel = GetComboLevel(currentCombo);
            ComboLevel previousLevel = GetComboLevel(currentCombo - 1);

            if (newLevel != previousLevel)
            {
                onComboLevelChanged?.Invoke(newLevel);
            }
        }
        else
        {
            // Start a new combo
            currentCombo = 1;

            // Cancel any existing reset coroutine
            if (comboResetCoroutine != null)
            {
                StopCoroutine(comboResetCoroutine);
            }

            // Start new reset coroutine
            comboResetCoroutine = StartCoroutine(ResetComboAfterDelay(comboTimeWindow));

            // Notify listeners for the first coin in a new combo
            onComboIncreased?.Invoke(currentCombo);
        }

        // Update last collection time
        lastCoinCollectionTime = currentTime;
    }

    /// <summary>
    /// Coroutine to reset combo after specified delay
    /// </summary>
    /// <param name="delay">Delay in seconds before resetting combo</param>
    private IEnumerator ResetComboAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentCombo > 0)
        {
            currentCombo = 0;
            onComboReset?.Invoke();
        }

        comboResetCoroutine = null;
    }

    /// <summary>
    /// Gets the current combo count
    /// </summary>
    /// <returns>Current combo count</returns>
    public int GetCurrentCombo()
    {
        return currentCombo;
    }

    /// <summary>
    /// Gets the current combo level based on combo count
    /// </summary>
    /// <param name="comboCount">Combo count to evaluate</param>
    /// <returns>Combo level</returns>
    public ComboLevel GetComboLevel(int comboCount)
    {
        if (comboCount >= platinumLevel)
            return ComboLevel.Platinum;
        else if (comboCount >= goldLevel)
            return ComboLevel.Gold;
        else if (comboCount >= silverLevel)
            return ComboLevel.Silver;
        else if (comboCount >= bronzeLevel)
            return ComboLevel.Bronze;
        else
            return ComboLevel.None;
    }

    /// <summary>
    /// Manually resets the combo
    /// </summary>
    public void ResetCombo()
    {
        currentCombo = 0;
        lastCoinCollectionTime = 0f;

        if (comboResetCoroutine != null)
        {
            StopCoroutine(comboResetCoroutine);
            comboResetCoroutine = null;
        }

        onComboReset?.Invoke();
    }

    /// <summary>
    /// Gets the remaining time before combo resets
    /// </summary>
    /// <returns>Remaining time in seconds, or 0 if no active combo</returns>
    public float GetRemainingComboTime()
    {
        if (currentCombo > 0)
        {
            float elapsedTime = Time.time - lastCoinCollectionTime;
            return Mathf.Max(0f, comboTimeWindow - elapsedTime);
        }
        return 0f;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}