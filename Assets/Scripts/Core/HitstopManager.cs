using System.Collections;
using UnityEngine;

namespace Core
{
    public class HitstopManager : MonoBehaviour
    {
        private static HitstopManager _instance;
        private Coroutine _hitstopCoroutine;
        private float _preHitstopTimeScale = 1f;

        private static HitstopManager Instance
        {
            get
            {
                if (_instance) return _instance;
                _instance = FindAnyObjectByType<HitstopManager>(FindObjectsInactive.Include);
                if (_instance) return _instance;
                var go = new GameObject("HitstopManager");
                _instance = go.AddComponent<HitstopManager>();
                return _instance;
            }
            set => _instance = value;
        }

        private void Awake()
        {
            if (_instance == null)
                Instance = this;
            else if (_instance != this)
                Destroy(gameObject);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _instance = null;
        }

        public static void TriggerHitstop(float duration = 0.04f, float slowScale = 0.02f)
        {
            if (Instance) Instance.Hitstop(duration, slowScale);
        }

        private void Hitstop(float duration = 0.04f, float slowScale = 0.02f)
        {
            // Don't freeze if game is paused or game over
            if (GameManager.Instance && (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver))
                return;

            if (_hitstopCoroutine != null)
                StopCoroutine(_hitstopCoroutine);

            _hitstopCoroutine = StartCoroutine(HitstopRoutine(duration, slowScale));
        }

        private IEnumerator HitstopRoutine(float duration, float slowScale)
        {
            _preHitstopTimeScale = Time.timeScale > 0.05f ? Time.timeScale : 1f;
            Time.timeScale = slowScale;

            yield return new WaitForSecondsRealtime(duration);

            // Restore timescale only if not paused in the meantime
            if (GameManager.Instance && !GameManager.Instance.IsPaused) Time.timeScale = _preHitstopTimeScale;

            _hitstopCoroutine = null;
        }
    }
}