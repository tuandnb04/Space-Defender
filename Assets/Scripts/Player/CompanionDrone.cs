using Combat;
using Core;
using Enemies;
using UnityEngine;

namespace Player
{
    public class CompanionDrone : MonoBehaviour
    {
        [Header("Orbit Settings")] public float orbitRadius = 1.35f;
        public float orbitSpeed = 150f;
        public float angleOffset;

        [Header("Weapon Settings")] public float fireRate = 0.38f;
        public GameObject plasmaPrefab;
        public float searchRadius = 8.5f;

        private float _currentAngle;
        private float _nextFireTime;

        // Cache player reference — avoids singleton lookup every Update frame
        private PlayerController _player;

        private void Awake()
        {
            _currentAngle = angleOffset;
        }

        private void Start()
        {
            _nextFireTime = Time.time + 0.3f;
            _player = PlayerController.Instance;
        }

        private void Update()
        {
            // Refresh cache only if lost (e.g. player died and respawned)
            if (_player == null) _player = PlayerController.Instance;
            if (_player == null || _player.currentLives <= 0)
            {
                Destroy(gameObject);
                return;
            }

            // Orbit around player
            _currentAngle += orbitSpeed * Time.deltaTime;
            if (_currentAngle >= 360f) _currentAngle -= 360f;

            var rad = _currentAngle * Mathf.Deg2Rad;
            var offset = new Vector3(Mathf.Cos(rad) * orbitRadius, Mathf.Sin(rad) * (orbitRadius * 0.75f), 0f);
            transform.position = _player.transform.position + offset;

            // Auto-aim and fire at nearest threat
            if (!(Time.time >= _nextFireTime)) return;
            _nextFireTime = Time.time + fireRate;
            FireAtNearestTarget();
        }

        private void FireAtNearestTarget()
        {
            Transform bestTarget = null;
            var minDistanceSqr = searchRadius * searchRadius;
            var myPos = transform.position;

            // Check Boss via static reference (zero lookup cost)
            var boss = BossController.ActiveBoss;
            if (boss != null && boss.gameObject.activeInHierarchy)
            {
                var dSqr = (myPos - boss.transform.position).sqrMagnitude;
                if (dSqr < minDistanceSqr)
                {
                    minDistanceSqr = dSqr;
                    bestTarget = boss.transform;
                }
            }

            // Check Enemies from ActiveEnemies static list (zero GC allocation)
            var enemies = Enemy.ActiveEnemies;
            foreach (var e in enemies)
            {
                if (!e || !e.gameObject.activeInHierarchy || e.IsDead) continue;
                var dSqr = (myPos - e.transform.position).sqrMagnitude;
                if (!(dSqr < minDistanceSqr)) continue;
                minDistanceSqr = dSqr;
                bestTarget = e.transform;
            }

            if (bestTarget == null) return;

            var dir = (bestTarget.position - transform.position).normalized;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            var rot = Quaternion.Euler(0f, 0f, angle);

            // Lazy-cache laser prefab from player
            if (plasmaPrefab == null && _player != null)
                plasmaPrefab = _player.laserPrefab;

            if (plasmaPrefab != null)
            {
                var boltObj = ObjectPoolManager.Spawn(plasmaPrefab, transform.position, rot);
                var laser = boltObj ? boltObj.GetComponent<Laser>() : null;
                if (laser != null)
                {
                    laser.isEnemyLaser = false;
                    laser.damage = 1;
                    laser.speed = 15f;
                    var sr = laser.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = new Color(0.2f, 1f, 0.7f);
                }
            }

            // Tilt drone towards firing direction
            transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
        }
    }
}