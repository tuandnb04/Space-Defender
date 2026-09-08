using UnityEngine;

namespace SpaceDefender
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Clips")]
        public AudioClip shootClip;
        public AudioClip enemyShootClip;
        public AudioClip explosionClip;
        public AudioClip shieldDownClip;
        public AudioClip gameOverClip;
        public AudioClip buttonClickClip;
        public AudioClip bgmClip;

        [Header("Audio Sources")]
        public AudioSource sfxSource;
        public AudioSource bgmSource;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        private void Awake()
        {
            Instance = this;

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
            sfxSource.playOnAwake = false;

            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
            }
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = 0.45f;
        }

        private void Start()
        {
            if (bgmClip == null)
            {
                bgmClip = GenerateProceduralSpaceBGM();
            }

            PlayBGM();
        }

        public void PlayShoot()
        {
            if (shootClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(shootClip, 0.75f);
            }
        }

        public void PlayEnemyShoot()
        {
            if (enemyShootClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(enemyShootClip, 0.65f);
            }
        }

        public void PlayExplosion()
        {
            if (explosionClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(explosionClip, 0.9f);
            }
        }

        public void PlayShieldDown()
        {
            if (shieldDownClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(shieldDownClip, 0.95f);
            }
        }

        public void PlayGameOver()
        {
            if (gameOverClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(gameOverClip, 1.0f);
            }
        }

        public void PlayButtonClick()
        {
            if (buttonClickClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(buttonClickClip, 0.8f);
            }
        }

        public void PlayBGM()
        {
            if (bgmSource != null && bgmClip != null)
            {
                bgmSource.clip = bgmClip;
                if (!bgmSource.isPlaying)
                {
                    bgmSource.Play();
                }
            }
        }

        public void StopBGM()
        {
            if (bgmSource != null && bgmSource.isPlaying)
            {
                bgmSource.Stop();
            }
        }

        private AudioClip GenerateProceduralSpaceBGM()
        {
            int sampleRate = 44100;
            float duration = 12f; // 12-second seamless loop
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            // Harmonic chord frequencies for space ambience: D minor (D3, F3, A3, C4)
            float[] freqs = new float[] { 146.83f, 174.61f, 220.00f, 261.63f, 293.66f, 349.23f };
            float twoPi = Mathf.PI * 2f;

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Sin(t / duration * Mathf.PI); // Smooth looping fade

                float val = 0f;
                // Bass drone
                val += 0.25f * Mathf.Sin(t * 73.42f * twoPi);
                // Pad chords
                for (int c = 0; c < freqs.Length; c++)
                {
                    float lfo = 0.5f + 0.5f * Mathf.Sin(t * (0.2f + c * 0.15f) * twoPi);
                    val += 0.08f * lfo * Mathf.Sin(t * freqs[c] * twoPi);
                }
                // Subtle arpeggiator pulse
                float arpIndex = Mathf.Floor(t * 4f) % freqs.Length;
                float arpEnv = Mathf.Exp(-((t * 4f) % 1f) * 4f);
                val += 0.12f * arpEnv * Mathf.Sin(t * freqs[(int)arpIndex] * 2f * twoPi);

                samples[i] = Mathf.Clamp(val * envelope * 0.7f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("SpaceDefender_BGM", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
