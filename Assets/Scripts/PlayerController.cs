using System.Collections;
using UnityEngine;

namespace SpaceDefender
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 9f;
        public float padding = 0.6f;

        [Header("Health & Lives")]
        public int maxLives = 3;
        public int currentLives = 3;
        public float invulnerableDuration = 1.5f;

        [Header("Shooting")]
        public GameObject laserPrefab;
        public Transform firePoint;
        public float fireRate = 0.22f;

        [Header("Power-ups & Shields")]
        public GameObject shieldVisual;
        public GameObject floatingScorePrefab;

        [Header("Effects")]
        public GameObject explosionPrefab;

        [Header("Demo / Test Mode")]
        public bool autoFireForDemo = false;

        private float minX;
        private float maxX;
        private float nextFireTime = 0f;
        private bool isDead = false;
        private bool isInvulnerable = false;
        private bool hasShield = false;
        private float tripleShotTimer = 0f;
        private SpriteRenderer spriteRenderer;
        private Vector3 startPosition;

        public bool HasShield => hasShield;
        public float TripleShotTimer => tripleShotTimer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            startPosition = transform.position;
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(false);
            }
        }

        private void Start()
        {
            CalculateScreenBounds();
            currentLives = maxLives;
        }

        public void ResetPlayer()
        {
            isDead = false;
            isInvulnerable = false;
            hasShield = false;
            tripleShotTimer = 0f;
            currentLives = maxLives;
            transform.position = startPosition;

            if (shieldVisual != null)
            {
                shieldVisual.SetActive(false);
            }

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateLives(currentLives);
            }
        }

        private void CalculateScreenBounds()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                float halfWidth = cam.orthographicSize * cam.aspect;
                minX = -halfWidth + padding;
                maxX = halfWidth - padding;
            }
            else
            {
                minX = -4.5f;
                maxX = 4.5f;
            }
        }

        private void Update()
        {
            if (isDead) return;
            if (GameManager.Instance != null && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) return;

            if (tripleShotTimer > 0f)
            {
                tripleShotTimer -= Time.deltaTime;
            }

            HandleMovement();
            HandleShooting();
        }

        private void HandleMovement()
        {
            float horizontal = 0f;

#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
            }
            else
#endif
            {
                try
                {
                    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
                    if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
                }
                catch (System.InvalidOperationException) { }
            }

            Vector3 pos = transform.position;
            pos.x += horizontal * moveSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            transform.position = pos;
        }

        private void HandleShooting()
        {
            bool shoot = false;

#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (keyboard != null)
            {
                if (keyboard.spaceKey.isPressed) shoot = true;
                if (keyboard.tKey.wasPressedThisFrame) autoFireForDemo = !autoFireForDemo;
            }
            if (mouse != null && mouse.leftButton.isPressed)
            {
                shoot = true;
            }
#else
            if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
            {
                shoot = true;
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                autoFireForDemo = !autoFireForDemo;
            }
#endif

            if (autoFireForDemo) shoot = true;

            if (shoot && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                Shoot();
            }
        }

        private void Shoot()
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.6f;

            if (laserPrefab != null)
            {
                if (tripleShotTimer > 0f)
                {
                    // Center laser
                    Instantiate(laserPrefab, spawnPos, Quaternion.identity);
                    // Left laser tilted 14 degrees
                    Instantiate(laserPrefab, spawnPos + Vector3.left * 0.25f, Quaternion.Euler(0, 0, 14f));
                    // Right laser tilted -14 degrees
                    Instantiate(laserPrefab, spawnPos + Vector3.right * 0.25f, Quaternion.Euler(0, 0, -14f));
                }
                else
                {
                    Instantiate(laserPrefab, spawnPos, Quaternion.identity);
                }
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShoot();
            }
        }

        public void ApplyPowerUp(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.TripleShot:
                    tripleShotTimer = 10f;
                    if (floatingScorePrefab != null)
                    {
                        FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.8f, "TRIPLE SHOT!", new Color(1f, 0.9f, 0.1f));
                    }
                    break;

                case PowerUpType.Shield:
                    hasShield = true;
                    if (shieldVisual != null)
                    {
                        shieldVisual.SetActive(true);
                    }
                    if (floatingScorePrefab != null)
                    {
                        FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.8f, "SHIELD ACTIVE!", new Color(0.2f, 0.8f, 1f));
                    }
                    break;

                case PowerUpType.Health:
                    if (currentLives < maxLives)
                    {
                        currentLives++;
                        if (UIManager.Instance != null)
                        {
                            UIManager.Instance.UpdateLives(currentLives);
                        }
                    }
                    if (floatingScorePrefab != null)
                    {
                        FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.8f, "+1 LIFE!", new Color(0.3f, 1f, 0.4f));
                    }
                    break;
            }
        }

        public void TakeDamage(int damage = 1)
        {
            if (isDead || isInvulnerable) return;

            // Absorb hit with shield if active
            if (hasShield)
            {
                hasShield = false;
                if (shieldVisual != null)
                {
                    shieldVisual.SetActive(false);
                }

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayShieldDown();
                }

                if (CameraShake.Instance != null)
                {
                    CameraShake.Instance.Shake(0.2f, 0.15f);
                }

                if (floatingScorePrefab != null)
                {
                    FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.8f, "SHIELD BROKEN!", Color.cyan);
                }

                StartCoroutine(InvulnerabilityFlash(0.6f));
                return;
            }

            currentLives -= damage;

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.35f, 0.25f);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShieldDown();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateLives(currentLives);
            }

            if (currentLives <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvulnerabilityFlash(invulnerableDuration));
            }
        }

        private IEnumerator InvulnerabilityFlash(float duration)
        {
            isInvulnerable = true;
            float elapsed = 0f;
            float interval = 0.12f;

            while (elapsed < duration)
            {
                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = (c.a == 1f) ? 0.3f : 1f;
                    spriteRenderer.color = c;
                }
                yield return new WaitForSeconds(interval);
                elapsed += interval;
            }

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }
            isInvulnerable = false;
        }

        public void Die()
        {
            if (isDead) return;
            isDead = true;

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.5f, 0.35f);
            }

            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }

            gameObject.SetActive(false);
        }
    }
}
