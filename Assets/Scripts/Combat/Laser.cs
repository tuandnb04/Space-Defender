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
        private float _bottomY = -6f;
        private bool _hasGrazed;
        private SpriteRenderer _spriteRenderer;

        private float _topY = 6f;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (isEnemyLaser && _spriteRenderer != null) _spriteRenderer.flipY = true;
        }

        private void Update()
        {
            var dir = isEnemyLaser ? -transform.up : transform.up;
            transform.Translate(dir * (speed * Time.deltaTime), Space.World);

            // Check Graze on player for enemy lasers
            if (isEnemyLaser && !_hasGrazed)
            {
                var player = PlayerController.Instance;
                if (player && !player.IsInvulnerable)
                {
                    var sqrDist = ((Vector2)transform.position - (Vector2)player.CockpitPosition).sqrMagnitude;
                    if (sqrDist is <= 0.7225f and > 0.0324f) // 0.85^2 and 0.18^2
                    {
                        _hasGrazed = true;
                        player.RegisterGraze(transform.position);
                    }
                }
            }

            if (isEnemyLaser)
            {
                if (transform.position.y < _bottomY || Mathf.Abs(transform.position.x) > 10f) Despawn();
            }
            else
            {
                if (transform.position.y > _topY || Mathf.Abs(transform.position.x) > 10f) Despawn();
            }
        }

        private void OnEnable()
        {
            _hasGrazed = false;
            var cam = Camera.main;
            if (cam != null)
            {
                _topY = cam.orthographicSize + topBoundaryOffset;
                _bottomY = -cam.orthographicSize - topBoundaryOffset;
            }

            // Balanced projectile speeds: Player lasers fast (15f), standard enemy lasers readable (8.5f)
            if (isEnemyLaser)
            {
                if (speed is > 11f and < 14.5f) speed = 8.5f;
            }
            else
            {
                if (speed < 14f) speed = 15f;
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

            // Player laser damages Enemy and Boss
            var playerCtrl = PlayerController.Instance;
            var finalDamage = damage;
            var isPointBlank = false;

            if (playerCtrl != null)
            {
                var distToPlayer = Vector2.Distance(playerCtrl.transform.position, transform.position);
                // Point-blank bonus: closer than 2.6 units -> 1.8x damage
                if (distToPlayer < 2.6f)
                {
                    finalDamage = Mathf.RoundToInt(damage * 1.8f);
                    isPointBlank = true;
                }
            }

            var enemy = target.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                HitSparkEffect.SpawnSpark(transform.position, new Color(1f, 0.9f, 0.25f));
                if (PerkManager.Instance != null && PerkManager.Instance.HasPerk(PerkType.TeslaArc))
                    TriggerTeslaArc(transform.position, enemy.gameObject);
                enemy.TakeHitWithDamage(finalDamage, transform.position, isPointBlank);
                Despawn();
                return;
            }

            var meteor = target.GetComponentInParent<SplittingMeteor>();
            if (meteor != null)
            {
                HitSparkEffect.SpawnSpark(transform.position, new Color(1f, 0.9f, 0.25f));
                if (PerkManager.Instance != null && PerkManager.Instance.HasPerk(PerkType.TeslaArc))
                    TriggerTeslaArc(transform.position, meteor.gameObject);
                meteor.TakeHitWithDamage(finalDamage);
                Despawn();
                return;
            }

            var boss = target.GetComponentInParent<BossController>();
            if (boss == null) return;
            HitSparkEffect.SpawnSpark(transform.position, new Color(1f, 0.9f, 0.25f));
            if (PerkManager.Instance != null && PerkManager.Instance.HasPerk(PerkType.TeslaArc))
                TriggerTeslaArc(transform.position, boss.gameObject);
            boss.TakeHit(finalDamage);
            Despawn();
        }

        private void TriggerTeslaArc(Vector3 hitPos, GameObject primaryTarget)
        {
            const float radiusSqr = 2.6f * 2.6f;
            Enemy target1 = null;
            Enemy target2 = null;

            foreach (var enemy in from enemy in Enemy.ActiveEnemies
                     where enemy != null && enemy.gameObject != primaryTarget && !enemy.IsDead
                     let sqrDist = ((Vector2)enemy.transform.position - (Vector2)hitPos).sqrMagnitude
                     where sqrDist <= radiusSqr
                     select enemy)
                if (target1 == null)
                {
                    target1 = enemy;
                }
                else
                {
                    target2 = enemy;
                    break;
                }

            if (target1 != null && !target1.IsDead)
            {
                target1.TakeHitWithDamage(Mathf.Max(1, damage / 2));
                HitSparkEffect.SpawnSpark(target1.transform.position, new Color(0.2f, 0.9f, 1f));
            }

            if (target2 == null || target2.IsDead) return;
            target2.TakeHitWithDamage(Mathf.Max(1, damage / 2));
            HitSparkEffect.SpawnSpark(target2.transform.position, new Color(0.2f, 0.9f, 1f));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void Despawn()
        {
            ObjectPoolManager.Despawn(gameObject);
        }
    }
}