using System;
using Core;
using Enemies;
using UnityEngine;

namespace Combat
{
    public class ShockwaveEffect : MonoBehaviour
    {
        [Header("Expansion Settings")] public float maxRadius = 14f;
        public float duration = 0.55f;
        public int bossDamage = 8;

        [Header("Visual")] public SpriteRenderer spriteRenderer;
        public Color shockwaveColor = new(0.3f, 0.9f, 1f, 0.9f);

        // Pre-allocated buffer — avoids array allocation every frame from OverlapCircleAll
        private static readonly Collider2D[] OverlapBuffer = new Collider2D[32];
        private static readonly int[] HitIds = new int[32]; // track by instance ID, not reference
        private int _hitCount;

        private float _timer;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _hitCount = 0;
            _timer = 0f;
        }

        private void Start()
        {
            transform.localScale = Vector3.zero;
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.45f, 0.35f);
        }

        [Obsolete("Obsolete")]
        private void Update()
        {
            _timer += Time.deltaTime;
            var progress = Mathf.Clamp01(_timer / duration);

            // Expand outward
            var currentRadius = Mathf.Lerp(0.5f, maxRadius, Mathf.Sin(progress * Mathf.PI * 0.5f));
            transform.localScale = new Vector3(currentRadius, currentRadius, 1f);

            // Fade alpha — read-modify-write on struct (no new Color allocation)
            if (spriteRenderer)
            {
                var c = shockwaveColor;
                c.a = Mathf.Lerp(0.85f, 0f, progress * progress);
                spriteRenderer.color = c;
            }

            // NonAlloc overlap — reuses the static buffer, zero GC
            var hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, currentRadius * 0.5f, OverlapBuffer);
            for (var i = 0; i < hitCount; i++)
            {
                var col = OverlapBuffer[i];
                if (!col) continue;

                // Deduplicate by instance ID (no HashSet allocation per frame)
                var id = col.GetInstanceID();
                var alreadyHit = false;
                for (var j = 0; j < _hitCount; j++)
                    if (HitIds[j] == id) { alreadyHit = true; break; }
                if (alreadyHit) continue;

                // Enemy
                var enemy = col.GetComponent<Enemy>();
                if (enemy) { RecordHit(id); enemy.TakeHit(); continue; }

                // Boss
                var boss = col.GetComponent<BossController>();
                if (boss) { RecordHit(id); boss.TakeHit(bossDamage); continue; }

                // Splitting Meteor
                var meteor = col.GetComponent<SplittingMeteor>();
                if (meteor) { RecordHit(id); meteor.TakeHitWithDamage(bossDamage); continue; }

                // Enemy Laser
                var laser = col.GetComponent<Laser>();
                if (laser && laser.isEnemyLaser) { RecordHit(id); ObjectPoolManager.Despawn(laser.gameObject); }
            }

            if (_timer >= duration) Destroy(gameObject);
        }

        private void RecordHit(int id)
        {
            if (_hitCount < HitIds.Length)
                HitIds[_hitCount++] = id;
        }
    }
}