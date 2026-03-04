using UnityEngine;

namespace Gomoku
{
    /// <summary>
    /// 音效管理器
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip placePieceClip;
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip loseClip;
        [SerializeField] private AudioClip drawClip;
        [SerializeField] private AudioClip buttonClickClip;

        [Header("Settings")]
        [SerializeField] private float volume = 0.8f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
        }

        public void PlayPlacePiece()
        {
            PlayClip(placePieceClip);
        }

        public void PlayWin()
        {
            PlayClip(winClip);
        }

        public void PlayLose()
        {
            PlayClip(loseClip);
        }

        public void PlayDraw()
        {
            PlayClip(drawClip);
        }

        public void PlayButtonClick()
        {
            PlayClip(buttonClickClip);
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }

        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
        }

        public void Mute(bool mute)
        {
            if (sfxSource != null)
            {
                sfxSource.mute = mute;
            }
        }
    }
}
