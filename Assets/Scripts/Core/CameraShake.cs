using System.Collections;
using UnityEngine;

namespace Core
{
    public class CameraShake : MonoBehaviour
    {
        private Vector3 _originalPos;
        private Coroutine _shakeCoroutine;
        public static CameraShake Instance { get; private set; }

        // Cache the pref value — PlayerPrefs is registry I/O; reading it on every Shake() call is wasteful.
        // Shake() is called on every hit, explosion, and dash.
        private static bool _screenShakeEnabled = true;
        private static bool _prefCached;

        public static bool ScreenShakeEnabled
        {
            get
            {
                if (_prefCached) return _screenShakeEnabled;
                _screenShakeEnabled = PlayerPrefs.GetInt("SD_SCREEN_SHAKE", 1) == 1;
                _prefCached = true;
                return _screenShakeEnabled;
            }
            set
            {
                _screenShakeEnabled = value;
                _prefCached = true;
                PlayerPrefs.SetInt("SD_SCREEN_SHAKE", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private void Awake()
        {
            Instance = this;
            _originalPos = transform.localPosition;
            // Pre-warm cache at startup
            _screenShakeEnabled = PlayerPrefs.GetInt("SD_SCREEN_SHAKE", 1) == 1;
            _prefCached = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
            _prefCached = false;
        }

        public void Shake(float duration = 0.18f, float magnitude = 0.12f)
        {
            if (!_screenShakeEnabled) return; // direct field access — no property call overhead
            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                var x = Random.Range(-1f, 1f) * magnitude;
                var y = Random.Range(-1f, 1f) * magnitude;
                transform.localPosition = new Vector3(_originalPos.x + x, _originalPos.y + y, _originalPos.z);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            transform.localPosition = _originalPos;
            _shakeCoroutine = null;
        }
    }
}