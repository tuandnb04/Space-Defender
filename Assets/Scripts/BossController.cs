using System.Collections;
using UnityEngine;

namespace SpaceDefender
{
    public class BossController : MonoBehaviour
    {
        [Header("Boss Stats")]
        public int maxHP = 20;
        public int currentHP = 20;
        public int scoreValue = 100;
        public string bossName = "RED UFO MOTHERSHIP";

        [Header("Movement")]
        public float entrySpeed = 2.0f;
        public float targetY = 2.8f;
        public float patrolSpeed = 1.8f;
        public float patrolAmplitude = 2.5f;

        [Header("Shooting")]
        public GameObject enemyLaserPrefab;
        public float attackInterval = 1.4f;
        public Transform leftFirePoint;
        public Transform rightFirePoint;

        [Header("Prefabs & Effects")]
        public GameObject explosionPrefab;
        public GameObject floatingScorePrefab;
        public GameObject[] dropPowerUpPrefabs;

        private bool isEntering = true;
        private bool isDead = false;
        private float nextAttackTime = 0f;
        private float patrolTimer = 0f;
        private SpriteRenderer spriteRenderer;
        private Color normalColor = Color.white;
        private Coroutine flashCoroutine;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                normalColor = spriteRenderer.color;
            }
        }

        private void Start()
        {
            currentHP = maxHP;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowBossBar(true, bossName);
                UIManager.Instance.UpdateBossHP(currentHP, maxHP);
            }

            nextAttackTime = Time.time + 1.8f; // Delay initial volley while entering
        }

        private void Update()
        {
            if (isDead) return;
            if (GameManager.Instance != null && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) return;

            if (isEntering)
            {
                // Smoothly lower into playfield
                Vector3 pos = transform.position;
                pos.y = Mathf.MoveTowards(pos.y, targetY, entrySpeed * Time.deltaTime);
                transform.position = pos;

                if (Mathf.Abs(pos.y - targetY) < 0.05f)
                {
                    isEntering = false;
                }
                return;
            }

            // Horizontal patrol sway
            patrolTimer += Time.deltaTime;
            float newX = Mathf.Sin(patrolTimer * patrolSpeed) * patrolAmplitude;
            transform.position = new Vector3(newX, targetY, transform.position.z);

            // Attack cycle
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackInterval;
                FireVolley();
            }
        }

        private void FireVolley()
        {
            if (isDead || enemyLaserPrefab == null) return;

            Vector3 leftPos = leftFirePoint != null ? leftFirePoint.position : transform.position + new Vector3(-0.6f, -0.4f, 0f);
            Vector3 rightPos = rightFirePoint != null ? rightFirePoint.position : transform.position + new Vector3(0.6f, -0.4f, 0f);

            Instantiate(enemyLaserPrefab, leftPos, Quaternion.identity);
            Instantiate(enemyLaserPrefab, rightPos, Quaternion.identity);

            // Occasional center laser when HP is below 50%
            if (currentHP <= maxHP / 2)
            {
                Vector3 centerPos = transform.position + new Vector3(0f, -0.5f, 0f);
                Instantiate(enemyLaserPrefab, centerPos, Quaternion.identity);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyShoot();
            }
        }

        public void TakeHit(int damage = 1)
        {
            if (isDead) return;

            currentHP -= damage;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateBossHP(currentHP, maxHP);
            }

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.1f, 0.07f);
            }

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(HitFlashRoutine());

            if (currentHP <= 0)
            {
                Die();
            }
        }

        private IEnumerator HitFlashRoutine()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 1f);
            }
            yield return new WaitForSeconds(0.08f);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
            flashCoroutine = null;
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowBossBar(false);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.5f, 0.35f);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayExplosion();
            }

            // Spawn floating text
            if (floatingScorePrefab != null)
            {
                FloatingScore.SpawnText(floatingScorePrefab, transform.position, "BOSS DEFEATED! +" + scoreValue, new Color(1f, 0.85f, 0.1f, 1f));
            }

            // Spawn explosions
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Instantiate(explosionPrefab, transform.position + new Vector3(-0.6f, 0.3f, 0f), Quaternion.identity);
                Instantiate(explosionPrefab, transform.position + new Vector3(0.6f, -0.2f, 0f), Quaternion.identity);
            }

            // Guaranteed power-up drop
            if (dropPowerUpPrefabs != null && dropPowerUpPrefabs.Length > 0)
            {
                int randomIndex = Random.Range(0, dropPowerUpPrefabs.Length);
                GameObject pUpPrefab = dropPowerUpPrefabs[randomIndex];
                if (pUpPrefab != null)
                {
                    Instantiate(pUpPrefab, transform.position, Quaternion.identity);
                }
            }

            // Notify EnemySpawner that boss is defeated
            EnemySpawner spawner = Object.FindAnyObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.OnBossDefeated();
            }

            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            HitPlayer(collision.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HitPlayer(collision.gameObject);
        }

        private void HitPlayer(GameObject target)
        {
            PlayerController player = target.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(1);
            }
        }
    }
}
