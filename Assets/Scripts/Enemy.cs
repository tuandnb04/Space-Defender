using System;
using System.Collections.Generic;
using SpaceDefender;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")] public float speed = 3.5f;

    public float rotationSpeed;
    public float bottomBoundaryOffset = 1.5f;

    [Header("Game Play")] public int scoreValue = 10;

    public GameObject explosionPrefab;
    public GameObject floatingScorePrefab;
    public GameObject[] powerUpPrefabs;
    public float dropChance = 0.25f;

    [Header("Shooting Settings")] public bool canShoot;

    public GameObject enemyLaserPrefab;
    public float minShootDelay = 1.2f;
    public float maxShootDelay = 2.5f;

    private float _bottomY = -6f;
    private bool _isDead;
    private float _nextShootTime;

    private void Start()
    {
        var cam = Camera.main;
        if (cam != null) _bottomY = -cam.orthographicSize - bottomBoundaryOffset;

        speed *= Random.Range(0.9f, 1.2f);

        if (canShoot) _nextShootTime = Time.time + Random.Range(minShootDelay, maxShootDelay);
    }

    private void Update()
    {
        // Move downwards
        transform.Translate(Vector3.down * (speed * Time.deltaTime), Space.World);

        // Optional rotation (especially nice for meteors)
        if (Mathf.Abs(rotationSpeed) > 0.01f) transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Enemy shooting
        if (canShoot && enemyLaserPrefab && Time.time >= _nextShootTime)
        {
            _nextShootTime = Time.time + Random.Range(minShootDelay, maxShootDelay);
            ShootLaser();
        }

        // Destroy when out of screen bounds at bottom
        if (transform.position.y < _bottomY) Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Hit(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Hit(collision.gameObject);
    }

    private void ShootLaser()
    {
        if (_isDead) return;
        if (GameManager.Instance && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver ||
                                     GameManager.Instance.IsPaused)) return;

        var spawnPos = transform.position + Vector3.down * 0.5f;
        Instantiate(enemyLaserPrefab, spawnPos, Quaternion.identity);

        if (AudioManager.Instance) AudioManager.Instance.PlayEnemyShoot();
    }

    public void TakeHit()
    {
        if (_isDead) return;
        _isDead = true;

        var finalScore = scoreValue;
        if (ComboManager.Instance)
            finalScore = ComboManager.Instance.RegisterKill(scoreValue, transform.position, floatingScorePrefab);
        else if (floatingScorePrefab) FloatingScore.Spawn(floatingScorePrefab, transform.position, scoreValue);

        if (GameManager.Instance) GameManager.Instance.AddScore(finalScore);

        if (AchievementManager.Instance) AchievementManager.Instance.UnlockAchievement("FIRST_BLOOD");

        if (CameraShake.Instance) CameraShake.Instance.Shake(0.12f, 0.08f);

        if (AudioManager.Instance) AudioManager.Instance.PlayExplosion();

        if (explosionPrefab) Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // Smart Power-up Drop
        TrySmartDropPowerUp();

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (EnemySpawner.Instance != null)
            EnemySpawner.Instance.OnEnemyRemoved();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void TrySmartDropPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;

        // Limit active power-ups on screen to 2 to prevent clutter
        var activePowerUps = FindObjectsByType<PowerUp>(FindObjectsInactive.Exclude);
        if (activePowerUps is { Length: >= 2 }) return;

        var player = PlayerController.Instance;
        var finalChance = dropChance;

        // In danger (1 heart) -> boost drop chance to help comeback
        if (player && player.currentLives <= 1)
            finalChance = Mathf.Min(finalChance * 1.6f, 0.45f);

        if (Random.value > finalChance) return;

        var candidates = new List<GameObject>();
        foreach (var p in powerUpPrefabs)
        {
            if (!p) continue;
            var comp = p.GetComponent<PowerUp>();
            if (!comp) continue;

            if (player)
            {
                // Don't drop Shield if player already has shield active
                if (comp.powerUpType == PowerUpType.Shield && player.HasShield)
                    continue;

                // Don't drop Health if player is already at full health
                if (comp.powerUpType == PowerUpType.Health && player.currentLives >= player.maxLives)
                    continue;
            }

            candidates.Add(p);
        }

        // If player is at 1 heart and Health candidate exists, give Health high priority
        if (player && player.currentLives <= 1)
        {
            var healthPrefab = candidates.Find(c => c.GetComponent<PowerUp>()?.powerUpType == PowerUpType.Health);
            if (healthPrefab && Random.value < 0.65f)
            {
                Instantiate(healthPrefab, transform.position, Quaternion.identity);
                return;
            }
        }

        if (candidates.Count > 0)
        {
            var chosen = candidates[Random.Range(0, candidates.Count)];
            if (chosen) Instantiate(chosen, transform.position, Quaternion.identity);
        }
        else if (powerUpPrefabs.Length > 0)
        {
            var chosen = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
            if (chosen) Instantiate(chosen, transform.position, Quaternion.identity);
        }
    }

    private void Hit(GameObject target)
    {
        var player = target.GetComponent<PlayerController>();
        if (player == null) return;
        player.TakeDamage();
        TakeHit();
    }
}