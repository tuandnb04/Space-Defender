using System.Linq;
using Core;
using Enemies;
using Player;
using UnityEngine;

namespace Combat
{
    public class Laser : MonoBehaviour
    {
        [Header("Laser Settings")] public float speed = 13f;
        public float topBoundaryOffset = 1.0f;
        public bool isEnemyLaser;
        public int damage = 1;
        public bool isFeverLaser;

        // Cached once per scene — Camera.main is a string lookup
        private static Camera _mainCam;
        private static float _cachedTopY = 6f;
        private static float _cachedBottomY = -6f;

        private bool _hasGrazed;
        private SpriteRenderer _spriteRenderer;

        // Cache player reference per-laser lifetime — avoids PlayerController.Instance lookup every frame
        private static PlayerController _cachedPlayer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _hasGrazed = false;

            // Refresh camera bounds once per enable (not per-Update)
            if (_mainCam == null) _mainCam = Camera.main;
            if (_mainCam != null)
            {
                _cachedTopY = _mainCam.orthographicSize + topBoundaryOffset;
                _cachedBottomY = -_mainCam.orthographicSize - topBoundaryOffset;
            }

            // Flip sprite based on direction
            if (_spriteRenderer != null) _spriteRenderer.flipY = isEnemyLaser;

            // Balanced projectile speeds
            if (isEnemyLaser)
            {
                if (speed is > 11f and < 14.5f) speed = 8.5f;
            }
            else
            {
                if (speed < 14f) speed = 15f;
            }
        }

        private void Update()
        {
            var dir = isEnemyLaser ? -transform.up : transform.up;
            transform.Translate(dir * (speed * Time.deltaTime), Space.World);

            // Graze check: use cached player reference — no per-frame singleton lookup
            if (isEnemyLaser && !_hasGrazed)
            {
                // Refresh cache only when null (player died or scene change)
                if (_cachedPlayer == null) _cachedPlayer = PlayerController.Instance;
                var player = _cachedPlayer;
                if (player != null && !player.IsInvulnerable)
                {
                    var sqrDist = ((Vector2)transform.position - (Vector2)player.CockpitPosition).sqrMagnitude;
                    if (sqrDist is <= 0.7225f and > 0.0324f)
                    {
                        _hasGrazed = true;
                        player.RegisterGraze(transform.position);
                    }
                }
            }

            var posX = transform.position.x;
            var posY = transform.position.y;
            if (isEnemyLaser)
            {
                if (posY < _cachedBottomY || Mathf.Abs(posX) > 10f) Despawn();
            }
            else
            {
                if (posY > _cachedTopY || Mathf.Abs(posX) > 10f) Despawn();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Hit(collision.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Hit(collision.gameObject);
        }

        private void Hit(GameObject target)
        {
            if (isEnemyLaser)
            {
                // Enemy laser ONLY damages the player
                var player = target.GetComponentInParent<PlayerController>();
                if (player == null) return;
                player.TakeDamage();
                Despawn();
                return;
            }

            // Player laser — figure out what we hit first (only one GetComponentInParent branch executes)
            var enemy = target.GetComponentInParent<Enemy>();
            SplittingMeteor meteor = null;
            BossController boss = null;

            if (enemy == null)
            {
                meteor = target.GetComponentInParent<SplittingMeteor>();
                if (meteor == null)
                    boss = target.GetComponentInParent<BossController>();
            }

            // Nothing hittable
            if (enemy == null && meteor == null && boss == null) return;

            // Point-blank bonus — use sqrMagnitude (no sqrt)
            var finalDamage = damage;
            var isPointBlank = false;
            if (_cachedPlayer == null) _cachedPlayer = PlayerController.Instance;
            if (_cachedPlayer != null)
            {
                var dx = _cachedPlayer.transform.position.x - transform.position.x;
                var dy = _cachedPlayer.transform.position.y - transform.position.y;
                if (dx * dx + dy * dy < 6.76f) // 2.6^2
                {
                    finalDamage = Mathf.RoundToInt(damage * 1.8f);
                    isPointBlank = true;
                }
            }

            // Shared hit effects
            HitSparkEffect.SpawnSpark(transform.position, new Color(1f, 0.9f, 0.25f));
            var hasTesla = PerkManager.Instance != null && PerkManager.Instance.HasPerk(PerkType.TeslaArc);

            if (enemy != null)
            {
                if (hasTesla) TriggerTeslaArc(transform.position, enemy.gameObject);
                enemy.TakeHitWithDamage(finalDamage, transform.position, isPointBlank);
            }
            else if (meteor != null)
            {
                if (hasTesla) TriggerTeslaArc(transform.position, meteor.gameObject);
                meteor.TakeHitWithDamage(finalDamage);
            }
            else
            {
                if (hasTesla)
                    if (boss != null)
                        TriggerTeslaArc(transform.position, boss.gameObject);
                if (boss != null) boss.TakeHit(finalDamage);
            }

            Despawn();
        }

        private void TriggerTeslaArc(Vector3 hitPos, GameObject primaryTarget)
        {
            const float radiusSqr = 2.6f * 2.6f;
            Enemy target1 = null;
            Enemy target2 = null;

            // No LINQ — simple loop over ActiveEnemies list (zero allocations)
            var enemies = Enemy.ActiveEnemies;
            foreach (var e in from e in enemies where e != null && e.gameObject != primaryTarget && !e.IsDead let sqrDist = ((Vector2)e.transform.position - (Vector2)hitPos).sqrMagnitude where !(sqrDist > radiusSqr) select e)
            {
                if (target1 == null) target1 = e;
                else { target2 = e; break; }
            }

            var arcDamage = Mathf.Max(1, damage / 2);
            if (target1 != null && !target1.IsDead)
            {
                target1.TakeHitWithDamage(arcDamage);
                HitSparkEffect.SpawnSpark(target1.transform.position, new Color(0.2f, 0.9f, 1f));
            }

            if (target2 == null || target2.IsDead) return;
            target2.TakeHitWithDamage(arcDamage);
            HitSparkEffect.SpawnSpark(target2.transform.position, new Color(0.2f, 0.9f, 1f));
        }

        // Invalidate cached player when player dies or respawns
        public static void InvalidatePlayerCache() => _cachedPlayer = null;

        private void Despawn()
        {
            ObjectPoolManager.Despawn(gameObject);
        }
    }
}