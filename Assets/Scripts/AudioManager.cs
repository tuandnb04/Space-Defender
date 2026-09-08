using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    [Header("Audio Clips")] public AudioClip shootClip;

    public AudioClip enemyShootClip;
    public AudioClip explosionClip;
    public AudioClip shieldDownClip;
    public AudioClip powerUpClip;
    public AudioClip gameOverClip;
    public AudioClip buttonClickClip;
    public AudioClip empBombClip;
    public AudioClip comboClip;
    public AudioClip waveClearClip;
    public AudioClip bossWarningClip;
    public AudioClip bgmClip;

    [Header("Audio Sources")] public AudioSource sfxSource;

    public AudioSource bgmSource;

    [Header("Volume & Settings")] public float bgmVolume = 0.45f;

    public float sfxVolume = 0.85f;
    public bool isMuted;

    public static AudioManager Instance
    {
        get
        {
            if (!_instance) _instance = FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            return _instance;
        }
        private set => _instance = value;
    }

    private void Awake()
    {
        Instance = this;

        // Load audio preferences
        bgmVolume = PlayerPrefs.GetFloat("SD_BGM_VOL", 0.45f);
        sfxVolume = PlayerPrefs.GetFloat("SD_SFX_VOL", 0.85f);
        isMuted = PlayerPrefs.GetInt("SD_MUTED", 0) == 1;

        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = isMuted ? 0f : sfxVolume;

        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = isMuted ? 0f : bgmVolume;
    }

    private void Start()
    {
        if (bgmClip == null) bgmClip = GenerateProceduralSpaceBGM();

        PlayBGM();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    public void PlayShoot()
    {
        if (shootClip && sfxSource) sfxSource.PlayOneShot(shootClip, 0.75f);
    }

    public void PlayEnemyShoot()
    {
        if (enemyShootClip && sfxSource) sfxSource.PlayOneShot(enemyShootClip, 0.65f);
    }

    public void PlayExplosion()
    {
        if (explosionClip && sfxSource) sfxSource.PlayOneShot(explosionClip, 0.9f);
    }

    public void PlayShieldDown()
    {
        if (shieldDownClip != null && sfxSource != null) sfxSource.PlayOneShot(shieldDownClip, 0.95f);
    }

    public void PlayPowerUp()
    {
        if (powerUpClip && sfxSource) sfxSource.PlayOneShot(powerUpClip, 1.0f);
    }

    public void PlayGameOver()
    {
        if (gameOverClip != null && sfxSource != null) sfxSource.PlayOneShot(gameOverClip, 1.0f);
    }

    public void PlayButtonClick()
    {
        if (buttonClickClip != null && sfxSource != null) sfxSource.PlayOneShot(buttonClickClip, 0.8f);
    }

    public void PlayEmpBomb()
    {
        if (empBombClip && sfxSource)
            sfxSource.PlayOneShot(empBombClip, 1.0f);
        else if (sfxSource)
            // Fallback to explosion or twoTone
            if (explosionClip)
                sfxSource.PlayOneShot(explosionClip, 1.0f);
    }

    public void PlayCombo()
    {
        if (comboClip != null && sfxSource != null)
            sfxSource.PlayOneShot(comboClip, 0.9f);
        else if (powerUpClip != null && sfxSource != null) sfxSource.PlayOneShot(powerUpClip, 0.6f);
    }

    public void PlayWaveClear()
    {
        if (waveClearClip != null && sfxSource != null)
            sfxSource.PlayOneShot(waveClearClip, 1.0f);
        else if (powerUpClip != null && sfxSource != null)
            sfxSource.PlayOneShot(powerUpClip, 1.0f);
    }

    public void PlayBossWarning()
    {
        if (bossWarningClip == null)
            bossWarningClip = GenerateProceduralWarningAlarm();

        if (bossWarningClip != null && sfxSource != null)
            sfxSource.PlayOneShot(bossWarningClip, 1.0f);
    }

    private static AudioClip GenerateProceduralWarningAlarm()
    {
        const int sampleRate = 44100;
        const float duration = 2.2f;
        var totalSamples = Mathf.FloorToInt(sampleRate * duration);
        var samples = new float[totalSamples];
        const float twoPi = Mathf.PI * 2f;

        for (var i = 0; i < totalSamples; i++)
        {
            var t = (float)i / sampleRate;
            // 4 alternating pulses between 880Hz and 660Hz with sharp envelope
            var pulse = (t * 3.5f) % 1.0f;
            var freq = ((int)(t * 3.5f) % 2 == 0) ? 880f : 660f;
            var env = Mathf.Pow(Mathf.Clamp01(1f - pulse), 1.5f);
            var val = Mathf.Sin(t * freq * twoPi) * env * 0.45f;
            samples[i] = Mathf.Clamp(val, -1f, 1f);
        }

        var clip = AudioClip.Create("BossWarningAlarm", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SD_BGM_VOL", bgmVolume);
        PlayerPrefs.Save();

        if (bgmSource != null) bgmSource.volume = isMuted ? 0f : bgmVolume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SD_SFX_VOL", sfxVolume);
        PlayerPrefs.Save();

        if (sfxSource != null) sfxSource.volume = isMuted ? 0f : sfxVolume;
    }

    public void SetMute(bool muted)
    {
        isMuted = muted;
        PlayerPrefs.SetInt("SD_MUTED", isMuted ? 1 : 0);
        PlayerPrefs.Save();

        if (bgmSource != null) bgmSource.volume = isMuted ? 0f : bgmVolume;
        if (sfxSource != null) sfxSource.volume = isMuted ? 0f : sfxVolume;
    }

    private void PlayBGM()
    {
        if (bgmSource == null || bgmClip == null) return;
        bgmSource.clip = bgmClip;
        if (!bgmSource.isPlaying) bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying) bgmSource.Stop();
    }

    private static AudioClip GenerateProceduralSpaceBGM()
    {
        const int sampleRate = 44100;
        const float duration = 12f; // 12-second seamless loop
        var totalSamples = Mathf.FloorToInt(sampleRate * duration);
        var samples = new float[totalSamples];

        // Harmonic chord frequencies for space ambience: D minor (D3, F3, A3, C4)
        var freqs = new[] { 146.83f, 174.61f, 220.00f, 261.63f, 293.66f, 349.23f };
        const float twoPi = Mathf.PI * 2f;

        for (var i = 0; i < totalSamples; i++)
        {
            var t = (float)i / sampleRate;
            var envelope = Mathf.Sin(t / duration * Mathf.PI); // Smooth looping fade

            var val = 0f;
            // Bass drone
            val += 0.25f * Mathf.Sin(t * 73.42f * twoPi);
            // Pad chords
            for (var c = 0; c < freqs.Length; c++)
            {
                var lfo = 0.5f + 0.5f * Mathf.Sin(t * (0.2f + c * 0.15f) * twoPi);
                val += 0.08f * lfo * Mathf.Sin(t * freqs[c] * twoPi);
            }

            // Subtle arpeggiator pulse
            var arpIndex = Mathf.Floor(t * 4f) % freqs.Length;
            var arpEnv = Mathf.Exp(-(t * 4f % 1f) * 4f);
            val += 0.12f * arpEnv * Mathf.Sin(t * freqs[(int)arpIndex] * 2f * twoPi);

            samples[i] = Mathf.Clamp(val * envelope * 0.7f, -1f, 1f);
        }

        var clip = AudioClip.Create("SpaceDefender_BGM", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}