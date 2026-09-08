using UnityEngine;

namespace SpaceDefender
{
    public class Enemy : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float speed = 3.5f;
        public float rotationSpeed = 0f;
        public float bottomBoundaryOffset = 1.5f;

        [Header("Game Play")]
        public int scoreValue = 10;
        public GameObject explosionPrefab;
        public GameObject floatingScorePrefab;
        public GameObject[] powerUpPrefabs;
        public float dropChance = 0.25f;

        [Header("Shooting Settings")]
        public bool canShoot = false;
        public GameObject enemyLaserPrefab;
        public float minShootDelay = 1.2f;
        public float maxShootDelay = 2.5f;

        private float bottomY = -6f;
        private bool isDead = false;
        private float nextShootTime = 0f;

        private void Start()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                bottomY = -cam.orthographicSize - bottomBoundaryOffset;
            }

            speed *= Random.Range(0.9f, 1.2f);

            if (canShoot)
            {
                nextShootTime = Time.time + Random.Range(minShootDelay, maxShootDelay);
            }
        }

        private void Update()
        {
            // Move downwards
            transform.Translate(Vector3.down * (speed * Time.deltaTime), Space.World);

            // Optional rotation (especially nice for meteors)
            if (Mathf.Abs(rotationSpeed) > 0.01f)
            {
                transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            }

            // Enemy shooting
            if (canShoot && enemyLaserPrefab != null && Time.time >= nextShootTime)
            {
                nextShootTime = Time.time + Random.Range(minShootDelay, maxShootDelay);
                ShootLaser();
            }

            // Destroy when out of screen bounds at bottom
            if (transform.position.y < bottomY)
            {
                Destroy(gameObject);
            }
        }

        private void ShootLaser()
        {
            if (isDead) return;
            if (GameManager.Instance != null && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver)) return;

            Vector3 spawnPos = transform.position + Vector3.down * 0.5f;
            Instantiate(enemyLaserPrefab, spawnPos, Quaternion.identity);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyShoot();
            }
        }

        public void TakeHit()
        {
            if (isDead) return;
            isDead = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.12f, 0.08f);
            }

            if (floatingScorePrefab != null)
            {
                FloatingScore.Spawn(floatingScorePrefab, transform.position, scoreValue);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayExplosion();
            }

            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            // Power-up drop chance
            if (powerUpPrefabs != null && powerUpPrefabs.Length > 0 && Random.value <= dropChance)
            {
                int pIdx = Random.Range(0, powerUpPrefabs.Length);
                if (powerUpPrefabs[pIdx] != null)
                {
                    Instantiate(powerUpPrefabs[pIdx], transform.position, Quaternion.identity);
                }
            }

            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Hit(collision.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Hit(collision.gameObject);
        }

        private void Hit(GameObject target)
        {
            PlayerController player = target.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(1);
                TakeHit();
            }
        }
    }
}
