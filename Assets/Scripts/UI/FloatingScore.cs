using Core;
using UnityEngine;

namespace UI
{
    public class FloatingScore : MonoBehaviour
    {
        [Header("Animation Settings")] public float floatSpeed = 2.0f;
        public float fadeDuration = 0.75f;
        public Color defaultColor = new(1f, 0.92f, 0.23f, 1f); // Arcade Neon Yellow

        private Color _currentColor;
        private float _elapsed;
        private MeshRenderer _meshRenderer;
        private TextMesh _textMesh;

        private void Awake()
        {
            _textMesh = GetComponent<TextMesh>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _currentColor = defaultColor;

            if (_meshRenderer) _meshRenderer.sortingOrder = 30; // Above lasers, enemies, and player
            if (_textMesh) _textMesh.color = _currentColor;
        }

        private void Update()
        {
            transform.Translate(Vector3.up * (floatSpeed * Time.deltaTime), Space.World);
            _elapsed += Time.deltaTime;

            var alpha = Mathf.Clamp01(1f - _elapsed / fadeDuration);
            if (_textMesh)
            {
                var c = _currentColor;
                c.a = alpha;
                _textMesh.color = c;
            }

            if (_elapsed >= fadeDuration) ObjectPoolManager.Despawn(gameObject);
        }

        private void OnEnable()
        {
            _elapsed = 0f;
        }

        private void SetText(string text, Color? color = null)
        {
            _currentColor = color ?? defaultColor;
            if (!_textMesh) return;
            _textMesh.text = text;
            _textMesh.color = _currentColor;
        }

        public static FloatingScore Spawn(GameObject prefab, Vector3 position, int score, Color? color = null)
        {
            if (!prefab) return null;
            var obj = ObjectPoolManager.Spawn(prefab, position, Quaternion.identity);
            var fs = obj ? obj.GetComponent<FloatingScore>() : null;
            if (fs) fs.SetText("+" + score, color);
            return fs;
        }

        public static FloatingScore SpawnText(GameObject prefab, Vector3 position, string text, Color? color = null)
        {
            if (!prefab) return null;
            var obj = ObjectPoolManager.Spawn(prefab, position, Quaternion.identity);
            var fs = obj ? obj.GetComponent<FloatingScore>() : null;
            if (fs) fs.SetText(text, color);
            return fs;
        }
    }
}