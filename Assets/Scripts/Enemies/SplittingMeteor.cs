using Combat;
using Core;
using Environment;
using Player;
using UnityEngine;

namespace Enemies
{
    public enum MeteorSizeTier
    {
        Big,
        Medium,
        Small
    }

    public class SplittingMeteor : MonoBehaviour
    {
        [Header("Tier & Stats")] public MeteorSizeTier sizeTier = MeteorSizeTier.Big;

        public int maxHp = 3;
        public int currentHp = 3;
        public float fallSpeed = 2.5f;
        public float rotationSpeed = 45f;
        public int scoreValue = 15;

        [Header("Prefabs for Splitting")] public GameObject mediumMeteorPrefab;

        public GameObject smallMeteorPrefab;
        public GameObject starPrefab;
        public GameObject explosionPrefab;
        private float _bottomY = -6f;
        private HitFlashEffect _flashEffect;
        private bool _isDead;

        private Vector2 _moveDirection = Vector2.down;

        private void Awake()
        {
            _flashEffect = GetComponent<HitFlashEffect>();
            if (!_flashEffect) _flashEffect = gameObject.AddComponent<HitFlashEffect>();
        }

        private void Start()
        {
            currentHp = maxHp;
            var cam = Camera.main;
            if (cam) _bottomY = -cam.orthographicSize - 1.5f;

            if (EnemySpawner.Instance) EnemySpawner.Instance.RegisterEnemy(gameObject);
        }

        private void Update()
        {
            if (_isDead) return;

            transform.Translate(_moveDirection * (fallSpeed * Time.deltaTime), Space.World);
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            if (transform.position.y < _bottomY || Mathf.Abs(transform.position.x) > 10f) Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (EnemySpawner.Instance) EnemySpawner.Instance.UnregisterEnemy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            var player = other.gameObject.GetComponentInParent<PlayerController>();
            if (!player) return;
            player.TakeDamage();
            Die();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (!player) return;
            player.TakeDamage();
            Die();
        }

        private void InitializeChild(Vector2 direction, float speedBonus)
        {
            _moveDirection = direction.normalized;
            fallSpeed += speedBonus;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void TakeHitWithDamage(int damage)
        {
            if (_isDead) return;

            currentHp -= damage;
            if (_flashEffect) _flashEffect.Flash(0.08f, Color.white);

            if (currentHp <= 0) Die();
        }

        public void TakeHit()
        {
            TakeHitWithDamage(1);
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            if (GameManager.Instance) GameManager.Instance.AddScore(scoreValue);

            HitstopManager.TriggerHitstop(0.03f, 0.05f);
            if (CameraShake.Instance) CameraShake.Instance.Shake(0.1f, 0.07f);

            if (explosionPrefab) Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            if (AudioManager.Instance) AudioManager.Instance.PlayExplosion();

            // Split according to tier
            Split();

            // Overload gauge gain
            if (PlayerController.Instance) PlayerController.Instance.AddOverload(3f);

            // Meteor debris sparks
            HitSparkEffect.SpawnSpark(transform.position, new Color(0.68f, 0.62f, 0.55f));
            HitSparkEffect.SpawnSpark(transform.position + Vector3.up * 0.1f, new Color(0.85f, 0.75f, 0.5f));

            Destroy(gameObject);
        }

        private void Split()
        {
            switch (sizeTier)
            {
                case MeteorSizeTier.Big when mediumMeteorPrefab:
                {
                    SpawnChildMeteor(mediumMeteorPrefab, new Vector2(-0.7f, -1f));
                    SpawnChildMeteor(mediumMeteorPrefab, new Vector2(0.7f, -1f));
                    break;
                }
                case MeteorSizeTier.Medium when smallMeteorPrefab:
                {
                    SpawnChildMeteor(smallMeteorPrefab, new Vector2(-1.1f, -0.8f));
                    SpawnChildMeteor(smallMeteorPrefab, new Vector2(1.1f, -0.8f));
                    break;
                }
                case MeteorSizeTier.Small:
                {
                    // Drops star!
                    if (starPrefab)
                    {
                        var starObj = ObjectPoolManager.Spawn(starPrefab, transform.position, Quaternion.identity);
                        var star = starObj ? starObj.GetComponent<StarPickup>() : null;
                        if (star) star.AttractToPlayerImmediately();
                    }

                    break;
                }
            }
        }

        private void SpawnChildMeteor(GameObject prefab, Vector2 direction)
        {
            var spawnPos = transform.position + (Vector3)(direction.normalized * 0.35f);
            var obj = Instantiate(prefab, spawnPos, Quaternion.identity);
            var meteor = obj.GetComponent<SplittingMeteor>();
            if (meteor) meteor.InitializeChild(direction, 0.8f);
        }
    }
}