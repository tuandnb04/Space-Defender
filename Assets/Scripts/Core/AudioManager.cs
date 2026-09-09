using System.Linq;
using UnityEngine;

namespace Core
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;

        private static readonly float[] ScalePitchRatios =
        {
            1.0f, // Do (C)
            9f / 8f, // Re (D) = 1.125f
            5f / 4f, // Mi (E) = 1.25f
            4f / 3f, // Fa (F) = 1.333f
            3f / 2f, // Sol (G) = 1.5f
            5f / 3f, // La (A) = 1.667f
            15f / 8f, // Si (B) = 1.875f
            2.0f // Do High (C)
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
            if (bgmClip == null) bgmClip = GenerateProceduralSpaceBGM();
            if (menuAmbientClip == null) menuAmbientClip = GenerateProceduralMenuAmbientBGM();

            if (GameManager.Instance != null && GameManager.Instance.IsGameStarted)
                PlayBattleBGMWithDrop();
            else
                PlayMenuAmbientBGM();
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
            else if (sfxSource)
            {
                if (!shootClip) return;
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
                var env = Mathf.Exp(-t * 9.5f); // Crisp bell-like decay
                // Fundamental 523.25Hz (Middle C) + 2nd & 3rd harmonic bell chime
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
                // 4 alternating pulses between 880Hz and 660Hz with sharp envelope
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

        public void PlayMenuAmbientBGM()
        {
            if (menuAmbientClip == null) menuAmbientClip = GenerateProceduralMenuAmbientBGM();
            if (bgmSource == null || menuAmbientClip == null) return;

            if (bgmSource.clip == menuAmbientClip && bgmSource.isPlaying) return;

            bgmSource.clip = menuAmbientClip;
            bgmSource.pitch = 1.0f;
            bgmSource.Play();
        }

        public void PlayBattleBGMWithDrop()
        {
            if (bgmClip == null) bgmClip = GenerateProceduralSpaceBGM();
            if (bgmSource == null || bgmClip == null) return;

            bgmSource.clip = bgmClip;
            bgmSource.pitch = 1.0f;
            bgmSource.time = 0f; // Instant drop on beat 1 kick!
            bgmSource.Play();
        }

        public void PlayBGM()
        {
            PlayBattleBGMWithDrop();
        }

        public void StopBGM()
        {
            if (bgmSource != null && bgmSource.isPlaying) bgmSource.Stop();
        }

        private static AudioClip GenerateProceduralMenuAmbientBGM()
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
                new[] { 146.83f, 220.00f, 293.66f, 349.23f }, // Dm
                new[] { 174.61f, 220.00f, 261.63f, 349.23f }, // F
                new[] { 130.81f, 196.00f, 261.63f, 329.63f }, // C
                new[] { 110.00f, 164.81f, 220.00f, 261.63f } // Am
            };

            for (var i = 0; i < totalSamples; i++)
            {
                var t = (float)i / sampleRate;
                var beatTime = t % beatDuration;
                var beatIndex = Mathf.FloorToInt(t / beatDuration);
                var chordIdx = beatIndex / 4 % rootPitches.Length;

                var currentRoot = rootPitches[chordIdx];
                var chord = chordPitches[chordIdx];

                var mixedVal = 0f;

                // 1. Warm Analog Sub-Bass Hum
                var bass = Mathf.Sin(t * currentRoot * twoPi) * 0.22f;
                mixedVal += bass;

                // 2. Slow Detuned Ambient Pad Chords
                var padLfo = 0.5f + 0.5f * Mathf.Sin(t * 0.35f * twoPi);
                var padChorus = Mathf.Sin(t * 0.8f * twoPi) * 0.02f;
                mixedVal += chord.Select(freq => Mathf.Sin(t * (freq + padChorus) * twoPi) * 0.06f)
                    .Select(padTone => padTone * padLfo).Sum();

                // 3. Starlight Chimes on even beats
                if (beatIndex % 2 == 0)
                {
                    var chimeEnv = Mathf.Exp(-beatTime * 3.2f);
                    var chimeFreq = chord[beatIndex % chord.Length] * 2f;
                    var chime = (Mathf.Sin(t * chimeFreq * twoPi) * 0.5f +
                                 Mathf.Sin(t * chimeFreq * 2f * twoPi) * 0.25f) * chimeEnv * 0.09f;
                    mixedVal += chime;
                }

                // 4. Low Cosmic Drift Shimmer
                var drift = Mathf.Sin(t * 55f * twoPi) * 0.03f * (0.6f + 0.4f * Mathf.Sin(t * 0.15f * twoPi));
                mixedVal += drift;

                samples[i] = Mathf.Clamp(mixedVal * 0.8f, -0.9f, 0.9f);
            }

            var clip = AudioClip.Create("SpaceDefender_MenuAmbientBGM", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public void SetMusicFeverMode(bool isFever)
        {
            if (bgmSource != null)
                bgmSource.pitch = isFever ? 1.06f : 1.0f;
        }

        private static AudioClip GenerateProceduralSpaceBGM()
        {
            const int sampleRate = 44100;
            const float bpm = 132f;
            const float beatDuration = 60f / bpm; // ~0.4545s
            const int totalBeats = 32; // 8 bars (4/4)
            const float duration = totalBeats * beatDuration; // ~14.545s seamless loop
            var totalSamples = Mathf.FloorToInt(sampleRate * duration);
            var samples = new float[totalSamples];

            const float twoPi = Mathf.PI * 2f;
            const float sixteenthDuration = beatDuration * 0.25f;

            // 8-Bar Chord Progression: Dm (2 bars) -> F (2 bars) -> C (2 bars) -> Am (2 bars)
            var rootPitches = new[]
            {
                73.42f, // D2 (bars 1-2)
                87.31f, // F2 (bars 3-4)
                65.41f, // C2 (bars 5-6)
                110.00f // A2 (bars 7-8)
            };

            // Lead Arpeggio Scale per chord
            var leadScales = new[]
            {
                new[] { 293.66f, 349.23f, 440.00f, 523.25f }, // Dm: D4, F4, A4, C5
                new[] { 349.23f, 440.00f, 523.25f, 698.46f }, // F:  F4, A4, C5, F5
                new[] { 261.63f, 329.63f, 392.00f, 523.25f }, // C:  C4, E4, G4, C5
                new[] { 220.00f, 261.63f, 329.63f, 440.00f } // Am: A3, C4, E4, A4
            };

            // Deterministic pseudo-random seed for crisp hi-hat & snare noise
            var noiseSeed = 1337;

            for (var i = 0; i < totalSamples; i++)
            {
                var t = (float)i / sampleRate;
                var beatTime = t % beatDuration;
                var beatIndex = Mathf.FloorToInt(t / beatDuration);
                var barIndex = beatIndex / 4 % 8;
                var chordIdx = barIndex / 2 % rootPitches.Length;

                var currentRoot = rootPitches[chordIdx];
                var currentLeadNotes = leadScales[chordIdx];

                var mixedVal = 0f;

                // 1. Heavy Synthwave 4-on-the-Floor Kick Drum
                var kickEnv = Mathf.Exp(-beatTime * 14f);
                var kickPitch = 45f + 90f * Mathf.Exp(-beatTime * 28f);
                var kick = Mathf.Sin(beatTime * kickPitch * twoPi) * kickEnv * 0.35f;
                mixedVal += kick;

                // 2. Punchy Cyberpunk Snare on Beats 2 & 4 (even beatIndex in 0-indexed mod 2)
                if (beatIndex % 2 == 1)
                {
                    var snareEnv = Mathf.Exp(-beatTime * 18f);
                    noiseSeed = noiseSeed * 1664525 + 1013904223;
                    var whiteNoise = (noiseSeed & 0x7FFF) / 32767f * 2f - 1f;
                    var snareTone = Mathf.Sin(beatTime * 180f * twoPi) * 0.4f;
                    var snare = (whiteNoise * 0.6f + snareTone) * snareEnv * 0.22f;
                    mixedVal += snare;
                }

                // 3. Sizzling 16th-note Hi-Hat Groove
                var sixteenthTime = t % sixteenthDuration;
                var sixteenthIdx = Mathf.FloorToInt(t / sixteenthDuration) % 4;
                var hatAccent = sixteenthIdx == 2 ? 1.3f : 0.85f; // Accent off-beats
                var hatEnv = Mathf.Exp(-sixteenthTime * 45f);
                noiseSeed = noiseSeed * 1664525 + 1013904223;
                var hatNoise = (noiseSeed & 0x7FFF) / 32767f * 2f - 1f;
                var hiHat = hatNoise * hatEnv * 0.08f * hatAccent;
                mixedVal += hiHat;

                // 4. Rolling 16th Synthwave Bassline (Analog Sawtooth + Sub-bass)
                var bassOctave = sixteenthIdx % 2 == 1 ? 2.0f : 1.0f; // Octave bounce
                var bassFreq = currentRoot * bassOctave;
                var bassEnv = Mathf.Exp(-sixteenthTime * 9f);
                var bassSaw = (2f * (t * bassFreq % 1f) - 1f) * 0.18f;
                var bassSub = Mathf.Sin(t * currentRoot * twoPi) * 0.16f;
                mixedVal += (bassSaw + bassSub) * bassEnv;

                // 5. Arpeggiated Cyberpunk Lead Synth
                var arpIdx = sixteenthIdx % currentLeadNotes.Length;
                var arpFreq = currentLeadNotes[arpIdx];
                var arpEnv = Mathf.Exp(-sixteenthTime * 6f);
                // Rich multi-harmonic pulse wave
                var arpTone = (Mathf.Sin(t * arpFreq * twoPi) +
                               0.4f * Mathf.Sin(t * arpFreq * 2f * twoPi) +
                               0.2f * Mathf.Sin(t * arpFreq * 3f * twoPi)) * 0.11f;
                mixedVal += arpTone * arpEnv;

                // 6. Warm Stereo Pad Shimmer
                var padLfo = 0.5f + 0.5f * Mathf.Sin(t * 0.8f * twoPi);
                var pad = (Mathf.Sin(t * currentRoot * 2f * twoPi) +
                           Mathf.Sin(t * currentRoot * 3f * twoPi) * 0.5f) * 0.05f * padLfo;
                mixedVal += pad;

                // Master limiter / soft clipper
                samples[i] = Mathf.Clamp(mixedVal * 0.85f, -0.95f, 0.95f);
            }

            var clip = AudioClip.Create("SpaceDefender_SynthwaveBGM", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}