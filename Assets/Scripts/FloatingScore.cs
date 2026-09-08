using UnityEngine;

public class FloatingScore : MonoBehaviour
{
    [Header("Animation Settings")]
    public float floatSpeed = 2.0f;
    public float fadeDuration = 0.75f;
    public Color defaultColor = new Color(1f, 0.92f, 0.23f, 1f); // Arcade Neon Yellow

    private TextMesh _textMesh;
    private MeshRenderer _meshRenderer;
    private float _elapsed;
    private Color _currentColor;

    private void Awake()
    {
        _textMesh = GetComponent<TextMesh>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _currentColor = defaultColor;

        if (_meshRenderer)
        {
            _meshRenderer.sortingOrder = 30; // Above lasers, enemies, and player
        }

        if (_textMesh)
        {
            _textMesh.color = _currentColor;
        }
    }

    private void SetText(string text, Color? color = null)
    {
        if (color.HasValue)
        {
            _currentColor = color.Value;
        }

        if (_textMesh == null) return;
        _textMesh.text = text;
        _textMesh.color = _currentColor;
    }

    private void Update()
    {
        transform.Translate(Vector3.up * (floatSpeed * Time.deltaTime), Space.World);
        _elapsed += Time.deltaTime;

        var alpha = Mathf.Clamp01(1f - (_elapsed / fadeDuration));
        if (_textMesh)
        {
            var c = _currentColor;
            c.a = alpha;
            _textMesh.color = c;
        }

        if (_elapsed >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public static FloatingScore Spawn(GameObject prefab, Vector3 position, int score, Color? color = null)
    {
        if (!prefab) return null;
        var obj = Instantiate(prefab, position, Quaternion.identity);
        var fs = obj.GetComponent<FloatingScore>();
        if (fs)
        {
            fs.SetText("+" + score, color);
        }
        return fs;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public static FloatingScore SpawnText(GameObject prefab, Vector3 position, string text, Color? color = null)
    {
        if (!prefab) return null;
        var obj = Instantiate(prefab, position, Quaternion.identity);
        var fs = obj.GetComponent<FloatingScore>();
        if (fs)
        {
            fs.SetText(text, color);
        }
        return fs;
    }
}