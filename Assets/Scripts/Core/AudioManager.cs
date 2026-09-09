using System.Collections;
using System.Linq;
using UnityEngine;

namespace Core
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;

        private static readonly float[] ScalePitchRatios =
        {
            1.0f,       // Do (C)
            9f / 8f,    // Re (D) = 1.125f
            5f / 4f,    // Mi (E) = 1.25f
            4f / 3f,    // Fa (F) = 1.333f
            3f / 2f,    // Sol (G) = 1.5f
            5f / 3f,    // La (A) = 1.667f
            15f / 8f,   // Si (B) = 1.875f
            2.0f        // Do High (C)
        };

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
        public AudioClip menuAmbientClip;
        public AudioClip starChimeClip;
        public AudioClip dashClip;
        public AudioClip feverClip;
        public AudioClip missileClip;

        [Header("Audio Sources")] public AudioSource sfxSource;
        public AudioSource bgmSource;
        public AudioSource starSource;

        [Header("Volume & Settings")] public float bgmVolume = 0.45f;
        public float sfxVolume = 0.85f;
        public bool isMuted;

        // BGM generation state flags — prevents "play before ready" freeze
        private bool _bgmReady;
        private bool _menuBgmReady;
        private bool _pendingBattleBGM;
        private bool _pendingMenuBGM;

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

            bgmVolume = PlayerPrefs.GetFloat("SD_BGM_VOL", 0.45f);
            sfxVolume = PlayerPrefs.GetFloat("SD_SFX_VOL", 0.85f);
            isMuted = PlayerPrefs.GetInt("SD_MUTED", 0) == 1;

            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = isMuted ? 0f : sfxVolume;

            if (starSource == null) starSource = gameObject.AddComponent<AudioSource>();
            starSource.playOnAwake = false;
            starSource.volume = isMuted ? 0f : sfxVolume;

            if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = isMuted ? 0f : bgmVolume;
        }

        private void Start()
        {
            // Generate chime + warning alarm synchronously (tiny clips, < 2000 samples each — OK)
            if (starChimeClip == null) starChimeClip = GenerateProceduralStarChime();

            // Generate heavy BGM asynchronously to avoid main thread freeze
            var startBattle = GameManager.Instance != null && GameManager.Instance.IsGameStarted;
            StartCoroutine(startBattle
                ? GenerateBattleBGMAsync(playWhenReady: true)
                : GenerateMenuBGMAsync(playWhenReady: true));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        // ─────────────────────── SFX API ───────────────────────

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
            else if (sfxSource && explosionClip)
                sfxSource.PlayOneShot(explosionClip, 1.0f);
        }

        public void PlayCombo()
        {
            if (comboClip != null && sfxSource != null)
                sfxSource.PlayOneShot(comboClip, 0.9f);
            else if (powerUpClip != null && sfxSource != null)
                sfxSource.PlayOneShot(powerUpClip, 0.6f);
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
            if (bossWarningClip == null) bossWarningClip = GenerateProceduralWarningAlarm();
            if (bossWarningClip != null && sfxSource != null)
                sfxSource.PlayOneShot(bossWarningClip, 1.0f);
        }

        public void PlayStarPickup(int scaleStep = 0)
        {
            if (starChimeClip == null) starChimeClip = GenerateProceduralStarChime();
            if (starSource == null || starChimeClip == null) return;
            var pitchIndex = Mathf.Clamp(scaleStep, 0, ScalePitchRatios.Length - 1);
            starSource.pitch = ScalePitchRatios[pitchIndex];
            starSource.PlayOneShot(starChimeClip, 0.9f);
        }

        public void PlayDash()
        {
            if (dashClip && sfxSource)
            {
                sfxSource.PlayOneShot(dashClip, 0.85f);
            }
            else if (sfxSource && shootClip)
            {
                sfxSource.pitch = 0.6f;
                sfxSource.PlayOneShot(shootClip, 0.7f);
                sfxSource.pitch = 1f;
            }
        }

        public void PlayFeverActivate()
        {
            if (feverClip && sfxSource)
                sfxSource.PlayOneShot(feverClip, 1.0f);
            else if (waveClearClip && sfxSource)
                sfxSource.PlayOneShot(waveClearClip, 1.0f);
        }

        public void PlayMissileLaunch()
        {
            if (missileClip && sfxSource)
            {
                sfxSource.PlayOneShot(missileClip, 0.8f);
            }
            else if (shootClip && sfxSource)
            {
                sfxSource.pitch = 1.4f;
                sfxSource.PlayOneShot(shootClip, 0.65f);
                sfxSource.pitch = 1f;
            }
        }

        public void PlayGraze()
        {
            if (!starSource) return;
            if (!starChimeClip) starChimeClip = GenerateProceduralStarChime();
            starSource.pitch = 2.4f;
            starSource.PlayOneShot(starChimeClip, 0.4f);
        }

        // ─────────────────────── BGM API ───────────────────────

        public void PlayMenuAmbientBGM()
        {
            if (_menuBgmReady && menuAmbientClip != null)
            {
                if (bgmSource.clip == menuAmbientClip && bgmSource.isPlaying) return;
                bgmSource.clip = menuAmbientClip;
                bgmSource.pitch = 1.0f;
                bgmSource.Play();
            }
            else
            {
                // Generation not done yet — queue up and start if not already running
                _pendingMenuBGM = true;
                _pendingBattleBGM = false;
                if (!_menuBgmReady)
                    StartCoroutine(GenerateMenuBGMAsync(playWhenReady: true));
            }
        }

        public void PlayBattleBGMWithDrop()
        {
            if (_bgmReady && bgmClip != null)
            {
                bgmSource.clip = bgmClip;
                bgmSource.pitch = 1.0f;
                bgmSource.time = 0f;
                bgmSource.Play();
            }
            else
            {
                _pendingBattleBGM = true;
                _pendingMenuBGM = false;
                if (!_bgmReady)
                    StartCoroutine(GenerateBattleBGMAsync(playWhenReady: true));
            }
        }

        public void PlayBGM() => PlayBattleBGMWithDrop();

        public void StopBGM()
        {
            if (bgmSource != null && bgmSource.isPlaying) bgmSource.Stop();
        }

        public void SetMusicFeverMode(bool isFever)
        {
            if (bgmSource != null) bgmSource.pitch = isFever ? 1.06f : 1.0f;
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

        // ─────────── Async BGM Generators (chunked, non-blocking) ───────────

        private const int ChunkSize = 4096; // samples processed per frame

        private IEnumerator GenerateBattleBGMAsync(bool playWhenReady)
        {
            const int sampleRate = 44100;
            const float bpm = 132f;
            const float beatDuration = 60f / bpm;
            const int totalBeats = 32;
            const float duration = totalBeats * beatDuration;
            var totalSamples = Mathf.FloorToInt(sampleRate * duration);
            var samples = new float[totalSamples];

            const float twoPi = Mathf.PI * 2f;
            const float sixteenthDuration = beatDuration * 0.25f;

            var rootPitches = new[] { 73.42f, 87.31f, 65.41f, 110.00f };
            var leadScales = new[]
            {
                new[] { 293.66f, 349.23f, 440.00f, 523.25f },
                new[] { 349.23f, 440.00f, 523.25f, 698.46f },
                new[] { 261.63f, 329.63f, 392.00f, 523.25f },
                new[] { 220.00f, 261.63f, 329.63f, 440.00f }
            };

            var noiseSeed = 1337;
            var processed = 0;

            while (processed < totalSamples)
            {
                var end = Mathf.Min(processed + ChunkSize, totalSamples);
                for (var i = processed; i < end; i++)
                {
                    var t = (float)i / sampleRate;
                    var beatTime = t % beatDuration;
                    var beatIndex = Mathf.FloorToInt(t / beatDuration);
                    var barIndex = beatIndex / 4 % 8;
                    var chordIdx = barIndex / 2 % rootPitches.Length;

                    var currentRoot = rootPitches[chordIdx];
                    var currentLeadNotes = leadScales[chordIdx];
                    var mixedVal = 0f;

                    // Kick
                    var kickEnv = Mathf.Exp(-beatTime * 14f);
                    var kickPitch = 45f + 90f * Mathf.Exp(-beatTime * 28f);
                    mixedVal += Mathf.Sin(beatTime * kickPitch * twoPi) * kickEnv * 0.35f;

                    // Snare
                    if (beatIndex % 2 == 1)
                    {
                        var snareEnv = Mathf.Exp(-beatTime * 18f);
                        noiseSeed = noiseSeed * 1664525 + 1013904223;
                        var whiteNoise = (noiseSeed & 0x7FFF) / 32767f * 2f - 1f;
                        var snareTone = Mathf.Sin(beatTime * 180f * twoPi) * 0.4f;
                        mixedVal += (whiteNoise * 0.6f + snareTone) * snareEnv * 0.22f;
                    }

                    // Hi-Hat
                    var sixteenthTime = t % sixteenthDuration;
                    var sixteenthIdx = Mathf.FloorToInt(t / sixteenthDuration) % 4;
                    var hatAccent = sixteenthIdx == 2 ? 1.3f : 0.85f;
                    var hatEnv = Mathf.Exp(-sixteenthTime * 45f);
                    noiseSeed = noiseSeed * 1664525 + 1013904223;
                    var hatNoise = (noiseSeed & 0x7FFF) / 32767f * 2f - 1f;
                    mixedVal += hatNoise * hatEnv * 0.08f * hatAccent;

                    // Bass
                    var bassOctave = sixteenthIdx % 2 == 1 ? 2.0f : 1.0f;
                    var bassFreq = currentRoot * bassOctave;
                    var bassEnv = Mathf.Exp(-sixteenthTime * 9f);
                    var bassSaw = (2f * (t * bassFreq % 1f) - 1f) * 0.18f;
                    var bassSub = Mathf.Sin(t * currentRoot * twoPi) * 0.16f;
                    mixedVal += (bassSaw + bassSub) * bassEnv;

                    // Arp Lead
                    var arpIdx = sixteenthIdx % currentLeadNotes.Length;
                    var arpFreq = currentLeadNotes[arpIdx];
                    var arpEnv = Mathf.Exp(-sixteenthTime * 6f);
                    var arpTone = (Mathf.Sin(t * arpFreq * twoPi) +
                                   0.4f * Mathf.Sin(t * arpFreq * 2f * twoPi) +
                                   0.2f * Mathf.Sin(t * arpFreq * 3f * twoPi)) * 0.11f;
                    mixedVal += arpTone * arpEnv;

                    // Pad
                    var padLfo = 0.5f + 0.5f * Mathf.Sin(t * 0.8f * twoPi);
                    var pad = (Mathf.Sin(t * currentRoot * 2f * twoPi) +
                               Mathf.Sin(t * currentRoot * 3f * twoPi) * 0.5f) * 0.05f * padLfo;
                    mixedVal += pad;

                    samples[i] = Mathf.Clamp(mixedVal * 0.85f, -0.95f, 0.95f);
                }

                processed = end;
                yield return null; // yield every chunk — no frame freeze
            }

            bgmClip = AudioClip.Create("SpaceDefender_SynthwaveBGM", totalSamples, 1, sampleRate, false);
            bgmClip.SetData(samples, 0);
            _bgmReady = true;

            switch (playWhenReady)
            {
                case true when _pendingBattleBGM:
                    _pendingBattleBGM = false;
                    bgmSource.clip = bgmClip;
                    bgmSource.pitch = 1.0f;
                    bgmSource.time = 0f;
                    bgmSource.Play();
                    break;
                case true when !_pendingMenuBGM:
                    bgmSource.clip = bgmClip;
                    bgmSource.pitch = 1.0f;
                    bgmSource.time = 0f;
                    bgmSource.Play();
                    break;
            }
        }

        private IEnumerator GenerateMenuBGMAsync(bool playWhenReady)
        {
            const int sampleRate = 44100;
            const float bpm = 66f;
            const float beatDuration = 60f / bpm;
            const int totalBeats = 16;
            const float duration = totalBeats * beatDuration;
            var totalSamples = Mathf.FloorToInt(sampleRate * duration);
            var samples = new float[totalSamples];

            const float twoPi = Mathf.PI * 2f;

            var rootPitches = new[] { 73.42f, 87.31f, 65.41f, 110.00f };
            var chordPitches = new[]
            {
                new[] { 146.83f, 220.00f, 293.66f, 349.23f },
                new[] { 174.61f, 220.00f, 261.63f, 349.23f },
                new[] { 130.81f, 196.00f, 261.63f, 329.63f },
                new[] { 110.00f, 164.81f, 220.00f, 261.63f }
            };

            var processed = 0;
            while (processed < totalSamples)
            {
                var end = Mathf.Min(processed + ChunkSize, totalSamples);
                for (var i = processed; i < end; i++)
                {
                    var t = (float)i / sampleRate;
                    var beatTime = t % beatDuration;
                    var beatIndex = Mathf.FloorToInt(t / beatDuration);
                    var chordIdx = beatIndex / 4 % rootPitches.Length;

                    var currentRoot = rootPitches[chordIdx];
                    var chord = chordPitches[chordIdx];
                    var mixedVal = 0f;

                    // Bass
                    mixedVal += Mathf.Sin(t * currentRoot * twoPi) * 0.22f;

                    // Pad
                    var padLfo = 0.5f + 0.5f * Mathf.Sin(t * 0.35f * twoPi);
                    var padChorus = Mathf.Sin(t * 0.8f * twoPi) * 0.02f;
                    mixedVal += chord.Sum(freq => Mathf.Sin(t * (freq + padChorus) * twoPi) * 0.06f * padLfo);

                    // Chimes
                    if (beatIndex % 2 == 0)
                    {
                        var chimeEnv = Mathf.Exp(-beatTime * 3.2f);
                        var chimeFreq = chord[beatIndex % chord.Length] * 2f;
                        var chime = (Mathf.Sin(t * chimeFreq * twoPi) * 0.5f +
                                     Mathf.Sin(t * chimeFreq * 2f * twoPi) * 0.25f) * chimeEnv * 0.09f;
                        mixedVal += chime;
                    }

                    // Drift
                    mixedVal += Mathf.Sin(t * 55f * twoPi) * 0.03f * (0.6f + 0.4f * Mathf.Sin(t * 0.15f * twoPi));

                    samples[i] = Mathf.Clamp(mixedVal * 0.8f, -0.9f, 0.9f);
                }

                processed = end;
                yield return null;
            }

            menuAmbientClip = AudioClip.Create("SpaceDefender_MenuAmbientBGM", totalSamples, 1, sampleRate, false);
            menuAmbientClip.SetData(samples, 0);
            _menuBgmReady = true;

            switch (playWhenReady)
            {
                case true when _pendingMenuBGM:
                {
                    _pendingMenuBGM = false;
                    if (bgmSource.clip == menuAmbientClip && bgmSource.isPlaying) yield break;
                    bgmSource.clip = menuAmbientClip;
                    bgmSource.pitch = 1.0f;
                    bgmSource.Play();
                    break;
                }
                case true when !_pendingBattleBGM:
                {
                    if (bgmSource.clip == menuAmbientClip && bgmSource.isPlaying) yield break;
                    bgmSource.clip = menuAmbientClip;
                    bgmSource.pitch = 1.0f;
                    bgmSource.Play();
                    break;
                }
            }
        }

        // ─────────────── Small sync generators (fast, OK on main thread) ───────────────

        private static AudioClip GenerateProceduralStarChime()
        {
            const int sampleRate = 44100;
            const float duration = 0.35f;
            var totalSamples = Mathf.FloorToInt(sampleRate * duration);
            var samples = new float[totalSamples];
            const float twoPi = Mathf.PI * 2f;

            for (var i = 0; i < totalSamples; i++)
            {
                var t = (float)i / sampleRate;
                var env = Mathf.Exp(-t * 9.5f);
                var wave = Mathf.Sin(t * 523.25f * twoPi) * 0.5f +
                           Mathf.Sin(t * 1046.5f * twoPi) * 0.3f +
                           Mathf.Sin(t * 1569.75f * twoPi) * 0.15f;
                samples[i] = Mathf.Clamp(wave * env * 0.8f, -1f, 1f);
            }

            var clip = AudioClip.Create("StarChime", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
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
                var pulse = t * 3.5f % 1.0f;
                var freq = (int)(t * 3.5f) % 2 == 0 ? 880f : 660f;
                var env = Mathf.Pow(Mathf.Clamp01(1f - pulse), 1.5f);
                var val = Mathf.Sin(t * freq * twoPi) * env * 0.45f;
                samples[i] = Mathf.Clamp(val, -1f, 1f);
            }

            var clip = AudioClip.Create("BossWarningAlarm", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}