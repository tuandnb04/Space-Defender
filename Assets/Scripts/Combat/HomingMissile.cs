using Core;
using Enemies;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Combat
{
    public class HomingMissile : MonoBehaviour
    {
        private static readonly Collider2D[] OverlapBuffer = new Collider2D[16];

        [Header("Missile Stats")] public float initialSpeed = 5f;
        public float maxSpeed = 14f;
        public float acceleration = 12f;
        public float rotateSpeed = 260f;
        public float lifeTime = 4.0f;
        public int directDamage = 3;
        public float blastRadius = 1.4f;
        public int blastDamage = 2;
        public GameObject explosionPrefab;

        private float _currentSpeed;
        private float _spawnTime;
        private Transform _target;

        private void Update()
        {
            if (Time.time - _spawnTime > lifeTime)
            {
                Explode();
                return;
            }

            if (!_target || !_target.gameObject.activeInHierarchy) FindTarget();

            _currentSpeed = Mathf.Min(_currentSpeed + acceleration * Time.deltaTime, maxSpeed);

            if (_target)
            {
                var dir = (Vector2)_target.position - (Vector2)transform.position;
                dir.Normalize();
                var targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                var currentAngle = transform.eulerAngles.z;
                var nextAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotateSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0f, 0f, nextAngle);
            }

            transform.Translate(Vector3.up * (_currentSpeed * Time.deltaTime), Space.Self);
        }

        private void OnEnable()
        {
            _spawnTime = Time.time;
            _currentSpeed = initialSpeed;
            FindTarget();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<Enemy>() != null ||
                other.GetComponentInParent<BossController>() != null) Explode();
        }

        private void FindTarget()
        {
            _target = null;
            var minDistSqr = float.MaxValue;
            var myPos = transform.position;

            var boss = BossController.ActiveBoss;
            if (boss != null && boss.gameObject.activeInHierarchy)
            {
                var dSqr = (myPos - boss.transform.position).sqrMagnitude;
                minDistSqr = dSqr;
                _target = boss.transform;
            }

            foreach (var e in Enemy.ActiveEnemies)
            {
                if (!e || !e.gameObject.activeInHierarchy) continue;
                var dSqr = (myPos - e.transform.position).sqrMagnitude;
                if (!(dSqr < minDistSqr)) continue;
                minDistSqr = dSqr;
                _target = e.transform;
            }
        }

        private void Explode()
        {
            var hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, blastRadius, OverlapBuffer);
            for (var i = 0; i < hitCount; i++)
            {
                var col = OverlapBuffer[i];
                var enemy = col.GetComponentInParent<Enemy>();
                if (enemy)
                {
                    enemy.TakeHitWithDamage(blastDamage);
                    continue;
                }

                var boss = col.GetComponentInParent<BossController>();
                if (boss) boss.TakeHit(blastDamage);
            }

            if (PerkManager.Instance != null && PerkManager.Instance.HasPerk(PerkType.ClusterRockets))
                TriggerClusterWarheads();

            if (explosionPrefab) Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            if (AudioManager.Instance) AudioManager.Instance.PlayExplosion();

            if (CameraShake.Instance) CameraShake.Instance.Shake(0.12f, 0.09f);

            ObjectPoolManager.Despawn(gameObject);
        }

        private void TriggerClusterWarheads()
        {
            for (var i = 0; i < 3; i++)
            {
                var offset = (Vector3)Random.insideUnitCircle * 0.8f;
                var pos = transform.position + offset;
                HitSparkEffect.SpawnSpark(pos, new Color(1f, 0.85f, 0.15f));

                var count = Physics2D.OverlapCircleNonAlloc(pos, 0.85f, OverlapBuffer);
                for (var j = 0; j < count; j++)
                {
                    var col = OverlapBuffer[j];
                    var enemy = col.GetComponentInParent<Enemy>();
                    if (enemy)
                    {
                        enemy.TakeHitWithDamage(1);
                        continue;
                    }

                    var boss = col.GetComponentInParent<BossController>();
                    if (boss) boss.TakeHit();
                }
            }
        }
    }
}