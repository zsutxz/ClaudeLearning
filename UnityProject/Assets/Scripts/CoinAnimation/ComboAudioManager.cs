using UnityEngine;
using System.Collections;

public class ComboAudioManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public float pitchIncreasePerLevel = 0.1f;
    public float volumeIncreasePerLevel = 0.05f;

    [Header("Combo Level Audio Clips")]
    public AudioClip bronzeSound;
    public AudioClip silverSound;
    public AudioClip goldSound;
    public AudioClip platinumSound;

    [Header("Special Combo Sounds")]
    public AudioClip milestoneSound; // For combo milestones like 10, 20, 50, etc.

    private AudioSource audioSource;

    void Awake()
    {
        // Create audio source component
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        // Subscribe to combo events
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.onComboIncreased.AddListener(OnComboIncreased);
            ComboManager.Instance.onComboLevelChanged.AddListener(OnComboLevelChanged);
        }
    }

    /// <summary>
    /// Called when combo is increased
    /// </summary>
    /// <param name="comboCount">Current combo count</param>
    private void OnComboIncreased(int comboCount)
    {
        if (comboCount > 1) // Only play sounds for combos of 2 or more
        {
            PlayComboSound(comboCount);
        }
    }

    /// <summary>
    /// Called when combo level changes
    /// </summary>
    /// <param name="comboLevel">New combo level</param>
    private void OnComboLevelChanged(ComboManager.ComboLevel comboLevel)
    {
        // Play special sound when reaching a new combo level
        PlayLevelUpSound(comboLevel);
    }

    /// <summary>
    /// Plays the appropriate combo sound based on combo count
    /// </summary>
    /// <param name="comboCount">Current combo count</param>
    private void PlayComboSound(int comboCount)
    {
        ComboManager.ComboLevel level = ComboManager.Instance.GetComboLevel(comboCount);
        AudioClip clip = GetAudioClipForLevel(level);

        if (clip != null && audioSource != null)
        {
            // Adjust pitch and volume based on combo level
            audioSource.pitch = 1.0f + (int)level * pitchIncreasePerLevel;
            audioSource.volume = 1.0f + (int)level * volumeIncreasePerLevel;

            // Play the sound
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Plays special sound when reaching a new combo level
    /// </summary>
    /// <param name="comboLevel">New combo level</param>
    private void PlayLevelUpSound(ComboManager.ComboLevel comboLevel)
    {
        if (milestoneSound != null && audioSource != null)
        {
            // Play milestone sound with special effects
            audioSource.pitch = 1.0f + (int)comboLevel * pitchIncreasePerLevel * 2;
            audioSource.volume = 1.0f + (int)comboLevel * volumeIncreasePerLevel * 1.5f;
            audioSource.PlayOneShot(milestoneSound);
        }
    }

    /// <summary>
    /// Gets the appropriate audio clip for the given level
    /// </summary>
    /// <param name="level">Combo level</param>
    /// <returns>Audio clip for the level</returns>
    private AudioClip GetAudioClipForLevel(ComboManager.ComboLevel level)
    {
        switch (level)
        {
            case ComboManager.ComboLevel.Bronze:
                return bronzeSound;
            case ComboManager.ComboLevel.Silver:
                return silverSound;
            case ComboManager.ComboLevel.Gold:
                return goldSound;
            case ComboManager.ComboLevel.Platinum:
                return platinumSound;
            default:
                return bronzeSound; // Default to bronze sound
        }
    }

    /// <summary>
    /// Plays a sound with specific pitch and volume
    /// </summary>
    /// <param name="clip">Audio clip to play</param>
    /// <param name="pitch">Pitch multiplier</param>
    /// <param name="volume">Volume level</param>
    public void PlaySoundWithVariation(AudioClip clip, float pitch = 1.0f, float volume = 1.0f)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.pitch = pitch;
            audioSource.volume = volume;
            audioSource.PlayOneShot(clip);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.onComboIncreased.RemoveListener(OnComboIncreased);
            ComboManager.Instance.onComboLevelChanged.RemoveListener(OnComboLevelChanged);
        }
    }
}