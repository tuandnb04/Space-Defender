using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public class HitSparkEffect : MonoBehaviour
    {
        private const int PoolInitialSize = 36;
        private static readonly Queue<HitSparkInstance> SparkPool = new(PoolInitialSize);
        private static Transform _poolRoot;
        private static Sprite _cachedSparkSprite;

        private static void EnsurePool()
        {
            if (_poolRoot != null) return;

            var rootGo = new GameObject("HitSparkPool_Root");
            _poolRoot = rootGo.transform;
            DontDestroyOnLoad(rootGo);

            var sprite = GetSparkSprite();

            for (var i = 0; i < PoolInitialSize; i++)
            {
                var go = new GameObject($"HitSpark_{i}");
                go.transform.SetParent(_poolRoot);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = 25;

                var spark = go.AddComponent<HitSparkInstance>();
                go.SetActive(false);
                SparkPool.Enqueue(spark);
            }
        }

        public static void SpawnSpark(Vector3 position, Color? color = null)
        {
            EnsurePool();

            var sparkColor = color ?? new Color(1f, 0.9f, 0.3f); // Golden electric spark

            // Spawn 4 micro spark fragments from preallocated pool
            for (var i = 0; i < 4; i++)
            {
                HitSparkInstance spark = null;
                while (SparkPool.Count > 0)
                {
                    var candidate = SparkPool.Dequeue();
                    if (candidate == null) continue;
                    spark = candidate;
                    break;
                }

                if (spark == null)
                {
                    // Fallback expand pool if depleted
                    var go = new GameObject("HitSpark_Dynamic");
                    go.transform.SetParent(_poolRoot);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = GetSparkSprite();
                    sr.sortingOrder = 25;
                    spark = go.AddComponent<HitSparkInstance>();
                }

                var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var speed = Random.Range(3.5f, 7.0f);
                var scale = Random.Range(0.08f, 0.14f);

                spark.transform.position = position;
                spark.transform.localScale = Vector3.one * scale;
                spark.gameObject.SetActive(true);
                spark.Initialize(dir, speed, sparkColor, 0.14f);
            }
        }

        public static void ReturnToPool(HitSparkInstance spark)
        {
            if (spark == null) return;
            spark.gameObject.SetActive(false);
            SparkPool.Enqueue(spark);
        }

        private static Sprite GetSparkSprite()
        {
            if (_cachedSparkSprite != null) return _cachedSparkSprite;

            // Create a crisp diamond spark texture programmatically
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            for (var y = 0; y < 8; y++)
            for (var x = 0; x < 8; x++)
            {
                var dist = Mathf.Abs(x - 3.5f) + Mathf.Abs(y - 3.5f);
                tex.SetPixel(x, y, dist <= 3.5f ? Color.white : Color.clear);
            }

            tex.Apply();

            _cachedSparkSprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 16f);
            return _cachedSparkSprite;
        }
    }

    public class HitSparkInstance : MonoBehaviour
    {
        private Vector2 _direction;
        private float _duration;
        private float _elapsed;
        private float _speed;
        private SpriteRenderer _sr;
        private Color _startColor;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            transform.position += (Vector3)(_direction * (_speed * Time.deltaTime));
            _speed *= 0.88f; // Decelerate

            if (_sr != null)
            {
                var alpha = Mathf.Lerp(1f, 0f, _elapsed / _duration);
                _sr.color = new Color(_startColor.r, _startColor.g, _startColor.b, alpha);
            }

            if (_elapsed >= _duration) HitSparkEffect.ReturnToPool(this);
        }

        public void Initialize(Vector2 dir, float spd, Color col, float dur)
        {
            _direction = dir;
            _speed = spd;
            _startColor = col;
            _duration = dur;
            _elapsed = 0f;
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _sr.color = col;
        }
    }
}