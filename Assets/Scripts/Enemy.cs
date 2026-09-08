using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3.5f;
    public float rotationSpeed;
    public float bottomBoundaryOffset = 1.5f;

    [Header("Game Play")]
    public int scoreValue = 10;
    public GameObject explosionPrefab;
    public GameObject floatingScorePrefab;
    public GameObject[] powerUpPrefabs;
    public float dropChance = 0.25f;

    [Header("Shooting Settings")]
    public bool canShoot;
    public GameObject enemyLaserPrefab;
    public float minShootDelay = 1.2f;
    public float maxShootDelay = 2.5f;

    private float _bottomY = -6f;
    private bool _isDead;
    private float _nextShootTime;

    private void Start()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            _bottomY = -cam.orthographicSize - bottomBoundaryOffset;
        }

        speed *= Random.Range(0.9f, 1.2f);

        if (canShoot)
        {
            _nextShootTime = Time.time + Random.Range(minShootDelay, maxShootDelay);
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
        if (canShoot && enemyLaserPrefab && Time.time >= _nextShootTime)
        {
            _nextShootTime = Time.time + Random.Range(minShootDelay, maxShootDelay);
            ShootLaser();
        }

        // Destroy when out of screen bounds at bottom
        if (transform.position.y < _bottomY)
        {
            Destroy(gameObject);
        }
    }

    private void ShootLaser()
    {
        if (_isDead) return;
        if (GameManager.Instance && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver)) return;

        var spawnPos = transform.position + Vector3.down * 0.5f;
        Instantiate(enemyLaserPrefab, spawnPos, Quaternion.identity);

        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlayEnemyShoot();
        }
    }

    public void TakeHit()
    {
        if (_isDead) return;
        _isDead = true;

        var finalScore = scoreValue;
        if (ComboManager.Instance)
        {
            finalScore = ComboManager.Instance.RegisterKill(scoreValue, transform.position, floatingScorePrefab);
        }
        else if (floatingScorePrefab)
        {
            FloatingScore.Spawn(floatingScorePrefab, transform.position, scoreValue);
        }

        if (GameManager.Instance)
        {
            GameManager.Instance.AddScore(finalScore);
        }

        if (AchievementManager.Instance)
        {
            AchievementManager.Instance.UnlockAchievement("FIRST_BLOOD");
        }

        if (CameraShake.Instance)
        {
            CameraShake.Instance.Shake(0.12f, 0.08f);
        }

        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlayExplosion();
        }

        if (explosionPrefab)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Power-up drop chance
        if (powerUpPrefabs is { Length: > 0 } && Random.value <= dropChance)
        {
            var pIdx = Random.Range(0, powerUpPrefabs.Length);
            if (powerUpPrefabs[pIdx])
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
        var player = target.GetComponent<PlayerController>();
        if (player == null) return;
        player.TakeDamage();
        TakeHit();
    }
}