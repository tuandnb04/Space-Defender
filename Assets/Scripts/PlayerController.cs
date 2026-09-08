using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private static PlayerController _instance;
    public static PlayerController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
            }
            return _instance;
        }
        private set => _instance = value;
    }

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

    [Header("Bombs & EMP Shockwave")]
    public GameObject shockwavePrefab;
    public int maxBombs = 3;
    public int currentBombs = 2;

    [Header("Ship Customization")]
    public Sprite[] shipSprites;

    [Header("Power-ups & Shields")]
    public GameObject shieldVisual;
    public GameObject floatingScorePrefab;

    [Header("Effects")]
    public GameObject explosionPrefab;

    [Header("Demo / Test Mode")]
    public bool autoFireForDemo;

    private float _minX;
    private float _maxX;
    private float _nextFireTime;
    private bool _isDead;
    private bool _isInvulnerable;
    private SpriteRenderer _spriteRenderer;
    private Vector3 _startPosition;

    private bool HasShield { get; set; }

    private float TripleShotTimer { get; set; }

    private void Awake()
    {
        Instance = this;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _startPosition = transform.position;
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
        _isDead = false;
        _isInvulnerable = false;
        TripleShotTimer = 0f;
        currentLives = maxLives;
        transform.position = _startPosition;

        var selectedShip = PlayerPrefs.GetInt("SD_SELECTED_SHIP", 0);
        ApplyShipConfig(selectedShip);

        if (_spriteRenderer != null)
        {
            var c = _spriteRenderer.color;
            c.a = 1f;
            _spriteRenderer.color = c;
        }

        if (UIManager.Instance == null) return;
        UIManager.Instance.UpdateLives(currentLives);
        UIManager.Instance.UpdateBombs(currentBombs);
    }

    private void ApplyShipConfig(int shipIndex)
    {
        shipIndex = Mathf.Clamp(shipIndex, 0, 3);

        // 0: Blue Vanguard (Balanced)
        // 1: Orange Interceptor (Fast)
        // 2: Green Striker (Rapid Fire)
        // 3: Red Dreadnought (Armored + 3 Bombs)
        switch (shipIndex)
        {
            case 0:
                moveSpeed = 9.5f;
                fireRate = 0.22f;
                maxBombs = 2;
                currentBombs = 2;
                HasShield = false;
                break;
            case 1:
                moveSpeed = 12.0f;
                fireRate = 0.22f;
                maxBombs = 1;
                currentBombs = 1;
                HasShield = false;
                break;
            case 2:
                moveSpeed = 9.0f;
                fireRate = 0.16f;
                maxBombs = 2;
                currentBombs = 2;
                HasShield = false;
                break;
            case 3:
                moveSpeed = 8.2f;
                fireRate = 0.24f;
                maxBombs = 3;
                currentBombs = 3;
                HasShield = true;
                break;
        }

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(HasShield);
        }

        if (shipSprites == null || shipIndex >= shipSprites.Length || shipSprites[shipIndex] == null) return;
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = shipSprites[shipIndex];
        }
    }

    private void CalculateScreenBounds()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            var halfWidth = cam.orthographicSize * cam.aspect;
            _minX = -halfWidth + padding;
            _maxX = halfWidth - padding;
        }
        else
        {
            _minX = -4.5f;
            _maxX = 4.5f;
        }
    }

    private void Update()
    {
        if (_isDead) return;
        if (GameManager.Instance && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) return;

        if (TripleShotTimer > 0f)
        {
            TripleShotTimer -= Time.deltaTime;
        }

        HandleMovement();
        HandleShooting();
        HandleBombInput();
    }

    private void HandleBombInput()
    {
        var bombPressed = false;
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard != null && keyboard.bKey.wasPressedThisFrame) bombPressed = true;
        if (mouse != null && mouse.rightButton.wasPressedThisFrame) bombPressed = true;
#else
            if (Input.GetKeyDown(KeyCode.B) || Input.GetMouseButtonDown(1)) bombPressed = true;
#endif
        if (bombPressed)
        {
            UseBomb();
        }
    }

    public void UseBomb()
    {
        if (_isDead || currentBombs <= 0) return;
        if (GameManager.Instance && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) return;

        currentBombs--;

        if (UIManager.Instance)
        {
            UIManager.Instance.UpdateBombs(currentBombs);
        }

        if (shockwavePrefab)
        {
            Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
        }

        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlayEmpBomb();
        }

        if (AchievementManager.Instance)
        {
            AchievementManager.Instance.UnlockAchievement("NUKE_HERO");
        }

        if (floatingScorePrefab)
        {
            FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.8f, "EMP SHOCKWAVE!", new Color(0.3f, 0.9f, 1f));
        }
    }

    public void AddBomb(int amount = 1)
    {
        currentBombs = Mathf.Min(currentBombs + amount, maxBombs);
        if (UIManager.Instance)
        {
            UIManager.Instance.UpdateBombs(currentBombs);
        }
    }

    private void HandleMovement()
    {
        var horizontal = 0f;

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
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
            catch (InvalidOperationException) { }
        }

        var pos = transform.position;
        pos.x += horizontal * moveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, _minX, _maxX);
        transform.position = pos;
    }

    private void HandleShooting()
    {
        var shoot = false;

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
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

        if (!shoot || !(Time.time >= _nextFireTime)) return;
        _nextFireTime = Time.time + fireRate;
        Shoot();
    }

    private void Shoot()
    {
        var spawnPos = firePoint ? firePoint.position : transform.position + Vector3.up * 0.6f;

        if (laserPrefab)
        {
            if (TripleShotTimer > 0f)
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

        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlayShoot();
        }
    }

    public void ApplyPowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.TripleShot:
                TripleShotTimer = 10f;
                if (floatingScorePrefab != null)
                {
                    FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.8f, "TRIPLE SHOT!", new Color(1f, 0.9f, 0.1f));
                }
                if (AchievementManager.Instance != null)
                {
                    AchievementManager.Instance.UnlockAchievement("TRIPLE_POWER");
                }
                break;

            case PowerUpType.Shield:
                HasShield = true;
                if (shieldVisual != null)
                {
                    shieldVisual.SetActive(true);
                }
                if (floatingScorePrefab != null)
                {
                    FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.8f, "SHIELD ACTIVE!", new Color(0.2f, 0.8f, 1f));
                }
                if (AchievementManager.Instance != null)
                {
                    AchievementManager.Instance.UnlockAchievement("SHIELD_UP");
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
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (_isDead || _isInvulnerable) return;

        // Absorb hit with shield if active
        if (HasShield)
        {
            HasShield = false;
            if (shieldVisual)
            {
                shieldVisual.SetActive(false);
            }

            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlayShieldDown();
            }

            if (CameraShake.Instance)
            {
                CameraShake.Instance.Shake(0.2f, 0.15f);
            }

            if (floatingScorePrefab)
            {
                FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.8f, "SHIELD BROKEN!", Color.cyan);
            }

            StartCoroutine(InvulnerabilityFlash(0.6f));
            return;
        }

        currentLives -= damage;

        if (CameraShake.Instance)
        {
            CameraShake.Instance.Shake(0.35f, 0.25f);
        }

        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlayShieldDown();
        }

        if (UIManager.Instance)
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
        _isInvulnerable = true;
        var elapsed = 0f;
        const float interval = 0.12f;

        while (elapsed < duration)
        {
            if (_spriteRenderer)
            {
                var c = _spriteRenderer.color;
                c.a = (Mathf.Approximately(c.a, 1f)) ? 0.3f : 1f;
                _spriteRenderer.color = c;
            }
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        if (_spriteRenderer)
        {
            var c = _spriteRenderer.color;
            c.a = 1f;
            _spriteRenderer.color = c;
        }
        _isInvulnerable = false;
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (CameraShake.Instance)
        {
            CameraShake.Instance.Shake(0.5f, 0.35f);
        }

        if (explosionPrefab)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (GameManager.Instance)
        {
            GameManager.Instance.GameOver();
        }

        gameObject.SetActive(false);
    }
}