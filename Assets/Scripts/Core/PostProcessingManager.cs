using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Core
{
    public class PostProcessingManager : MonoBehaviour
    {
        private static PostProcessingManager _instance;

        [Header("Volume & Overrides")] public Volume volume;

        private Bloom _bloom;
        private ChromaticAberration _chromaticAberration;
        private Coroutine _feverBloomCoroutine;

        private Coroutine _glitchCoroutine;
        private bool _isLowHealthPulsing;
        private Vignette _vignette;

        public static PostProcessingManager Instance
        {
            get
            {
                if (!_instance) _instance = FindAnyObjectByType<PostProcessingManager>(FindObjectsInactive.Include);
                return _instance;
            }
            private set => _instance = value;
        }

        private void Awake()
        {
            Instance = this;
            EnsureVolumeAndOverrides();
        }

        private void Start()
        {
            EnsureVolumeAndOverrides();
        }

        private void Update()
        {
            // Low health heartbeat vignette pulsation
            if (!_isLowHealthPulsing || _vignette == null) return;
            var pulse = 0.30f + 0.10f * Mathf.Sin(Time.time * 6.5f);
            _vignette.intensity.value = pulse;
            _vignette.color.value = new Color(0.85f, 0.1f, 0.15f);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        private void EnsureVolumeAndOverrides()
        {
            if (volume == null)
            {
                volume = FindAnyObjectByType<Volume>();
                if (volume == null)
                {
                    var volObj = new GameObject("GlobalVolume_Dynamic");
                    volume = volObj.AddComponent<Volume>();
                    volume.isGlobal = true;
                    volume.priority = 10;
                }
            }

            if (volume.profile == null) return;
            volume.profile.TryGet(out _bloom);
            volume.profile.TryGet(out _vignette);
            volume.profile.TryGet(out _chromaticAberration);
        }

        public void UpdateHealthVignette(int lives, int maxLives = 3)
        {
            if (_vignette == null) return;

            if (lives is <= 1 and > 0)
            {
                _isLowHealthPulsing = true;
            }
            else
            {
                _isLowHealthPulsing = false;
                _vignette.intensity.value = 0.20f;
                _vignette.color.value = Color.black;
            }
        }

        public void TriggerEmpShockwaveGlitch(float peakIntensity = 0.85f, float duration = 0.5f)
        {
            if (_glitchCoroutine != null) StopCoroutine(_glitchCoroutine);
            _glitchCoroutine = StartCoroutine(ChromaticGlitchRoutine(peakIntensity, duration));
        }

        public void TriggerBossFinisherGlitch()
        {
            if (_glitchCoroutine != null) StopCoroutine(_glitchCoroutine);
            _glitchCoroutine = StartCoroutine(ChromaticGlitchRoutine(1.0f, 1.2f));

            if (_bloom == null) return;
            if (_feverBloomCoroutine != null) StopCoroutine(_feverBloomCoroutine);
            _feverBloomCoroutine = StartCoroutine(BloomFlashRoutine(2.5f, 0.8f));
        }

        public void SetFeverBloom(bool isFever)
        {
            if (_bloom == null) return;
            if (_feverBloomCoroutine != null) StopCoroutine(_feverBloomCoroutine);
            _feverBloomCoroutine =
                StartCoroutine(isFever ? BloomTransitionRoutine(2.2f, 0.3f) : BloomTransitionRoutine(1.45f, 0.5f));
        }

        private IEnumerator ChromaticGlitchRoutine(float targetIntensity, float duration)
        {
            if (_chromaticAberration == null) yield break;

            _chromaticAberration.intensity.value = targetIntensity;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = elapsed / duration;
                _chromaticAberration.intensity.value = Mathf.Lerp(targetIntensity, 0f, t);
                yield return null;
            }

            _chromaticAberration.intensity.value = 0f;
            _glitchCoroutine = null;
        }

        private IEnumerator BloomFlashRoutine(float peakBloom, float duration)
        {
            if (_bloom == null) yield break;
            _bloom.intensity.value = peakBloom;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _bloom.intensity.value = Mathf.Lerp(peakBloom, 1.45f, elapsed / duration);
                yield return null;
            }

            _bloom.intensity.value = 1.45f;
        }

        private IEnumerator BloomTransitionRoutine(float targetBloom, float duration)
        {
            if (_bloom == null) yield break;
            var start = _bloom.intensity.value;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _bloom.intensity.value = Mathf.Lerp(start, targetBloom, elapsed / duration);
                yield return null;
            }

            _bloom.intensity.value = targetBloom;
            _feverBloomCoroutine = null;
        }
    }
}