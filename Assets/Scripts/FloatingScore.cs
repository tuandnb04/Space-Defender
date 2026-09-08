using UnityEngine;

namespace SpaceDefender
{
    public class FloatingScore : MonoBehaviour
    {
        [Header("Animation Settings")]
        public float floatSpeed = 2.0f;
        public float fadeDuration = 0.75f;
        public Color defaultColor = new Color(1f, 0.92f, 0.23f, 1f); // Arcade Neon Yellow

        private TextMesh textMesh;
        private MeshRenderer meshRenderer;
        private float elapsed = 0f;
        private Color currentColor;

        private void Awake()
        {
            textMesh = GetComponent<TextMesh>();
            meshRenderer = GetComponent<MeshRenderer>();
            currentColor = defaultColor;

            if (meshRenderer != null)
            {
                meshRenderer.sortingOrder = 30; // Above lasers, enemies, and player
            }

            if (textMesh != null)
            {
                textMesh.color = currentColor;
            }
        }

        public void SetText(string text, Color? color = null)
        {
            if (color.HasValue)
            {
                currentColor = color.Value;
            }

            if (textMesh != null)
            {
                textMesh.text = text;
                textMesh.color = currentColor;
            }
        }

        private void Update()
        {
            transform.Translate(Vector3.up * (floatSpeed * Time.deltaTime), Space.World);
            elapsed += Time.deltaTime;

            float alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            if (textMesh != null)
            {
                Color c = currentColor;
                c.a = alpha;
                textMesh.color = c;
            }

            if (elapsed >= fadeDuration)
            {
                Destroy(gameObject);
            }
        }

        public static FloatingScore Spawn(GameObject prefab, Vector3 position, int score, Color? color = null)
        {
            if (prefab == null) return null;
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            FloatingScore fs = obj.GetComponent<FloatingScore>();
            if (fs != null)
            {
                fs.SetText("+" + score, color);
            }
            return fs;
        }

        public static FloatingScore SpawnText(GameObject prefab, Vector3 position, string text, Color? color = null)
        {
            if (prefab == null) return null;
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            FloatingScore fs = obj.GetComponent<FloatingScore>();
            if (fs != null)
            {
                fs.SetText(text, color);
            }
            return fs;
        }
    }
}
