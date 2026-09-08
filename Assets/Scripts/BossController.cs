using System.Collections;
using SpaceDefender;
using UnityEngine;
using UnityEngine.Serialization;

public class BossController : MonoBehaviour
{
    [FormerlySerializedAs("maxHP")] [Header("Boss Stats")]
    public int maxHp = 20;

    [FormerlySerializedAs("currentHP")] public int currentHp = 20;
    public int scoreValue = 100;
    public string bossName = "RED UFO MOTHERSHIP";

    [Header("Movement")] public float entrySpeed = 2.0f;

    public float targetY = 2.8f;
    public float patrolSpeed = 1.8f;
    public float patrolAmplitude = 2.5f;

    [Header("Shooting")] public GameObject enemyLaserPrefab;

    public float attackInterval = 1.4f;
    public Transform leftFirePoint;
    public Transform rightFirePoint;

    [Header("Prefabs & Effects")] public GameObject explosionPrefab;

    public GameObject floatingScorePrefab;
    public GameObject[] dropPowerUpPrefabs;
    private Coroutine _flashCoroutine;
    private bool _isDead;

    private bool _isEntering = true;
    private float _nextAttackTime;
    private Color _normalColor = Color.white;
    private float _patrolTimer;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null) _normalColor = _spriteRenderer.color;
    }

    private void Start()
    {
        currentHp = maxHp;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowBossBar(true, bossName);
            UIManager.Instance.UpdateBossHp(currentHp, maxHp);
        }

        _nextAttackTime = Time.time + 1.8f; // Delay initial volley while entering
    }

    private void Update()
    {
        if (_isDead) return;
        if (GameManager.Instance && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver ||
                                     GameManager.Instance.IsPaused)) return;

        if (_isEntering)
        {
            // Smoothly lower into playfield
            var pos = transform.position;
            pos.y = Mathf.MoveTowards(pos.y, targetY, entrySpeed * Time.deltaTime);
            transform.position = pos;

            if (Mathf.Abs(pos.y - targetY) < 0.05f) _isEntering = false;
            return;
        }

        // Horizontal patrol sway
        _patrolTimer += Time.deltaTime;
        var newX = Mathf.Sin(_patrolTimer * patrolSpeed) * patrolAmplitude;
        transform.position = new Vector3(newX, targetY, transform.position.z);

        // Attack cycle
        if (!(Time.time >= _nextAttackTime)) return;
        _nextAttackTime = Time.time + attackInterval;
        FireVolley();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HitPlayer(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HitPlayer(collision.gameObject);
    }

    private void FireVolley()
    {
        if (_isDead || !enemyLaserPrefab) return;

        var leftPos = leftFirePoint ? leftFirePoint.position : transform.position + new Vector3(-0.6f, -0.4f, 0f);
        var rightPos = rightFirePoint ? rightFirePoint.position : transform.position + new Vector3(0.6f, -0.4f, 0f);

        Instantiate(enemyLaserPrefab, leftPos, Quaternion.identity);
        Instantiate(enemyLaserPrefab, rightPos, Quaternion.identity);

        // Occasional center laser when HP is below 50%
        if (currentHp <= maxHp / 2)
        {
            var centerPos = transform.position + new Vector3(0f, -0.5f, 0f);
            Instantiate(enemyLaserPrefab, centerPos, Quaternion.identity);
        }

        if (AudioManager.Instance) AudioManager.Instance.PlayEnemyShoot();
    }

    public void TakeHit(int damage = 1)
    {
        if (_isDead) return;

        currentHp -= damage;

        if (UIManager.Instance) UIManager.Instance.UpdateBossHp(currentHp, maxHp);

        if (CameraShake.Instance) CameraShake.Instance.Shake(0.1f, 0.07f);

        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(HitFlashRoutine());

        if (currentHp <= 0) Die();
    }

    private IEnumerator HitFlashRoutine()
    {
        if (_spriteRenderer) _spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 1f);
        yield return new WaitForSeconds(0.08f);
        if (_spriteRenderer) _spriteRenderer.color = _normalColor;
        _flashCoroutine = null;
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (UIManager.Instance) UIManager.Instance.ShowBossBar(false);

        var finalScore = scoreValue;
        if (ComboManager.Instance)
            finalScore = ComboManager.Instance.RegisterKill(scoreValue, transform.position, floatingScorePrefab);

        if (GameManager.Instance) GameManager.Instance.AddScore(finalScore);

        if (AchievementManager.Instance) AchievementManager.Instance.UnlockAchievement("BOSS_SLAYER");

        // Reward player with +1 Bomb
        var player = FindAnyObjectByType<PlayerController>();
        if (player) player.AddBomb();

        if (CameraShake.Instance) CameraShake.Instance.Shake(0.5f, 0.35f);

        if (AudioManager.Instance) AudioManager.Instance.PlayExplosion();

        // Spawn floating text if not already spawned by combo manager
        if (floatingScorePrefab && ComboManager.Instance == null)
            FloatingScore.SpawnText(floatingScorePrefab, transform.position, "BOSS DEFEATED! +" + scoreValue,
                new Color(1f, 0.85f, 0.1f, 1f));

        // Spawn explosions
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Instantiate(explosionPrefab, transform.position + new Vector3(-0.6f, 0.3f, 0f), Quaternion.identity);
            Instantiate(explosionPrefab, transform.position + new Vector3(0.6f, -0.2f, 0f), Quaternion.identity);
        }

        // Guaranteed power-up drop
        if (dropPowerUpPrefabs is { Length: > 0 })
        {
            var randomIndex = Random.Range(0, dropPowerUpPrefabs.Length);
            var pUpPrefab = dropPowerUpPrefabs[randomIndex];
            if (pUpPrefab) Instantiate(pUpPrefab, transform.position, Quaternion.identity);
        }

        // Notify EnemySpawner that boss is defeated
        var spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner) spawner.OnBossDefeated();

        Destroy(gameObject);
    }

    private static void HitPlayer(GameObject target)
    {
        var player = target.GetComponent<PlayerController>();
        if (player) player.TakeDamage();
    }
}