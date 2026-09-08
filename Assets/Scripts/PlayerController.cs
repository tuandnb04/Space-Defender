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

        [Header("Effects")]
        public GameObject explosionPrefab;

        [Header("Demo / Test Mode")]
        public bool autoFireForDemo = false;

        private float minX;
        private float maxX;
        private float nextFireTime = 0f;
        private bool isDead = false;
        private bool isInvulnerable = false;
        private SpriteRenderer spriteRenderer;
        private Vector3 startPosition;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            startPosition = transform.position;
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
            currentLives = maxLives;
            transform.position = startPosition;

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
                Instantiate(laserPrefab, spawnPos, Quaternion.identity);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShoot();
            }
        }

        public void TakeDamage(int damage = 1)
        {
            if (isDead || isInvulnerable) return;

            currentLives -= damage;

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
                StartCoroutine(InvulnerabilityFlash());
            }
        }

        private IEnumerator InvulnerabilityFlash()
        {
            isInvulnerable = true;
            float elapsed = 0f;
            float interval = 0.12f;

            while (elapsed < invulnerableDuration)
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
