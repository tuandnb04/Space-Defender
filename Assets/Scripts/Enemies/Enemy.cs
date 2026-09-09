using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combat;
using Core;
using Environment;
using Player;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies
{
    public enum EnemyBehaviorType
    {
        Standard,
        SinusoidalKamikaze,
        Sniper
    }

    public class Enemy : MonoBehaviour
    {
        public static readonly List<Enemy> ActiveEnemies = new(32);
        private static int _chainReactionDepth;

        // Cached once per scene — avoids expensive Camera.main string lookup every Start()
        private static Camera _mainCam;
        [Header("Enemy Stats")] public int maxHp = 1;

        public int currentHp = 1;
        public EnemyBehaviorType behaviorType = EnemyBehaviorType.Standard;

        [Header("Movement Settings")] public float speed = 3.5f;

        public float rotationSpeed;
        public float bottomBoundaryOffset = 1.5f;

        [Header("Sinusoidal / Kamikaze Settings")]
        public float sineFrequency = 3.0f;

        public float sineAmplitude = 1.5f;
        public float diveYThreshold = 1.2f;
        public float diveSpeed = 7.5f;

        [Header("Sniper Settings")] public float sniperHoldY = 3.0f;

        public float sniperAimDuration = 0.9f;
        public LineRenderer aimLine;

        [Header("Game Play & Rewards")] public int scoreValue = 10;

        public GameObject starPrefab;
        public int starDropCount = 2;
        public GameObject explosionPrefab;
        public GameObject floatingScorePrefab;
        public GameObject[] powerUpPrefabs;
        public float dropChance = 0.25f;
        public float chainExplosionRadius = 1.4f;
        public int chainDamage = 2;

        [Header("Shooting Settings")] public bool canShoot;

        public GameObject enemyLaserPrefab;
        public float minShootDelay = 1.2f;
        public float maxShootDelay = 2.5f;
        public Action OnDeathCallback;

        private float _bottomY = -6f;
        private Vector3 _diveTargetDir;
        private HitFlashEffect _flashEffect;
        private bool _isAimingSniper;
        private bool _isDiving;
        private float _nextShootTime;
        private Vector3 _spawnOrigin;
        private float _spawnTime;
        public bool IsDead { get; private set; }

        private void Awake()
        {
            _flashEffect = GetComponent<HitFlashEffect>();
            if (_flashEffect == null) _flashEffect = gameObject.AddComponent<HitFlashEffect>();
        }

        private void Start()
        {
            // Start is only called on first activation; full state reset happens in OnEnable for pool reuse.
            ResetState();
        }

        private void ResetState()
        {
            IsDead = false;
            currentHp = maxHp;
            _spawnTime = Time.time;
            _spawnOrigin = transform.position;
            _isDiving = false;
            _isAimingSniper = false;

            // Use cached camera — Camera.main is a slow string lookup
            if (_mainCam == null) _mainCam = Camera.main;
            if (_mainCam != null) _bottomY = -_mainCam.orthographicSize - bottomBoundaryOffset;

            speed *= Random.Range(0.9f, 1.2f);

            if (canShoot) _nextShootTime = Time.time + Random.Range(minShootDelay, maxShootDelay);
        }

        private void Update()
        {
            if (IsDead) return;

            switch (behaviorType)
            {
                case EnemyBehaviorType.Standard:
                    UpdateStandardMovement();
                    break;
                case EnemyBehaviorType.SinusoidalKamikaze:
                    UpdateKamikazeMovement();
                    break;
                case EnemyBehaviorType.Sniper:
                    UpdateSniperMovement();
                    break;
                default:
                    UpdateStandardMovement();
                    break;
            }

            // Optional rotation (especially nice for meteors)
            if (Mathf.Abs(rotationSpeed) > 0.01f) transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            // Standard enemy shooting
            if (behaviorType != EnemyBehaviorType.Sniper && canShoot && enemyLaserPrefab && Time.time >= _nextShootTime)
            {
                _nextShootTime = Time.time + Random.Range(minShootDelay, maxShootDelay);
                ShootLaser();
            }

            // Destroy when out of screen bounds at bottom
            if (transform.position.y < _bottomY) Destroy(gameObject);
        }

        private void OnEnable()
        {
            if (!ActiveEnemies.Contains(this)) ActiveEnemies.Add(this);

            // Reset state when recycled from pool (OnEnable is called on every pool reuse)
            // IsDead guard prevents double-reset on fresh Instantiate (Start runs after OnEnable)
            if (IsDead)
            {
                IsDead = false;
                ResetState();
            }

            if (EnemySpawner.Instance != null) EnemySpawner.Instance.RegisterEnemy(gameObject);
        }

        private void OnDisable()
        {
            ActiveEnemies.Remove(this);
        }

        private void OnDestroy()
        {
            OnDeathCallback?.Invoke();
            OnDeathCallback = null;
            if (aimLine != null) Destroy(aimLine);
            if (EnemySpawner.Instance == null) return;
            EnemySpawner.Instance.UnregisterEnemy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Hit(collision.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Hit(collision.gameObject);
        }

        private void UpdateStandardMovement()
        {
            transform.Translate(Vector3.down * (speed * Time.deltaTime), Space.World);
        }

        private void UpdateKamikazeMovement()
        {
            if (_isDiving)
            {
                transform.Translate(_diveTargetDir * (diveSpeed * Time.deltaTime), Space.World);
                return;
            }

            var elapsed = Time.time - _spawnTime;
            var horizontalOffset = Mathf.Sin(elapsed * sineFrequency) * sineAmplitude;
            var currentY = transform.position.y - speed * Time.deltaTime;
            transform.position = new Vector3(_spawnOrigin.x + horizontalOffset, currentY, transform.position.z);

            if (!(transform.position.y <= diveYThreshold)) return;
            _isDiving = true;
            if (_flashEffect) _flashEffect.Flash(0.14f, new Color(1f, 0.7f, 0.1f));
            diveSpeed = 6.5f;
            var player = PlayerController.Instance;
            if (player)
            {
                _diveTargetDir = (player.CockpitPosition - transform.position).normalized;
                var angle = Mathf.Atan2(_diveTargetDir.y, _diveTargetDir.x) * Mathf.Rad2Deg + 90f;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            else
            {
                _diveTargetDir = Vector3.down;
            }
        }

        private void UpdateSniperMovement()
        {
            if (transform.position.y > sniperHoldY)
            {
                transform.Translate(Vector3.down * (speed * Time.deltaTime), Space.World);
                return;
            }

            // Horizontal sway at top
            var sway = Mathf.Sin(Time.time * 1.5f) * 1.8f;
            transform.position = new Vector3(sway, sniperHoldY, transform.position.z);

            if (!_isAimingSniper && Time.time >= _nextShootTime) StartCoroutine(SniperAimRoutine());
        }

        private IEnumerator SniperAimRoutine()
        {
            _isAimingSniper = true;
            var player = PlayerController.Instance;
            var aimElapsed = 0f;

            if (!aimLine)
            {
                aimLine = gameObject.AddComponent<LineRenderer>();
                aimLine.material = new Material(Shader.Find("Sprites/Default"));
                aimLine.startColor = new Color(1f, 0.2f, 0.2f, 0.75f);
                aimLine.endColor = new Color(1f, 0.2f, 0.2f, 0.15f);
                aimLine.positionCount = 2;
                aimLine.useWorldSpace = true;
                aimLine.sortingOrder = 20;
            }

            aimLine.startWidth = 0.05f;
            aimLine.endWidth = 0.05f;
            aimLine.enabled = true;

            while (aimElapsed < sniperAimDuration && !IsDead)
            {
                if (player && aimLine)
                {
                    aimLine.SetPosition(0, transform.position);
                    aimLine.SetPosition(1, player.CockpitPosition);
                }

                aimElapsed += Time.deltaTime;
                yield return null;
            }

            if (aimLine) aimLine.enabled = false;

            if (!IsDead && enemyLaserPrefab && player)
            {
                var dir = (player.CockpitPosition - transform.position).normalized;
                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
                var laserObj = ObjectPoolManager.Spawn(enemyLaserPrefab, transform.position + dir * 0.4f,
                    Quaternion.Euler(0f, 0f, angle));
                var laser = laserObj ? laserObj.GetComponent<Laser>() : null;
                if (laser)
                {
                    laser.isEnemyLaser = true;
                    laser.speed = 16f;
                }

                if (AudioManager.Instance) AudioManager.Instance.PlayEnemyShoot();
            }

            _isAimingSniper = false;
            _nextShootTime = Time.time + Random.Range(2.0f, 3.2f);
        }

        private void ShootLaser()
        {
            if (IsDead) return;
            if (!GameManager.IsActive) return;

            var spawnPos = transform.position + Vector3.down * 0.5f;
            var laserObj = ObjectPoolManager.Spawn(enemyLaserPrefab, spawnPos, Quaternion.identity);
            var laser = laserObj ? laserObj.GetComponent<Laser>() : null;
            if (laser)
            {
                laser.isEnemyLaser = true;
                laser.speed = 8.5f;
            }

            if (AudioManager.Instance) AudioManager.Instance.PlayEnemyShoot();
        }

        public void TakeHit()
        {
            TakeHitWithDamage(1);
        }

        public void TakeHitWithDamage(int damage, Vector3? hitPos = null, bool isPointBlank = false)
        {
            if (IsDead) return;

            currentHp -= damage;

            if (_flashEffect) _flashEffect.Flash(0.08f, isPointBlank ? new Color(1f, 0.4f, 0.2f) : Color.white);

            if (isPointBlank && floatingScorePrefab)
                FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.5f,
                    "CRITICAL POINT-BLANK!", new Color(1f, 0.3f, 0.2f));

            if (currentHp <= 0) Die(isPointBlank);
        }

        private void Die(bool isPointBlank = false)
        {
            if (IsDead) return;
            IsDead = true;

            if (aimLine) aimLine.enabled = false;

            var finalScore = scoreValue * (isPointBlank ? 2 : 1);
            if (ComboManager.Instance)
                finalScore = ComboManager.Instance.RegisterKill(finalScore, transform.position, floatingScorePrefab);
            else if (floatingScorePrefab) FloatingScore.Spawn(floatingScorePrefab, transform.position, finalScore);

            if (GameManager.Instance) GameManager.Instance.AddScore(finalScore);

            if (AchievementManager.Instance) AchievementManager.Instance.UnlockAchievement("FIRST_BLOOD");

            // Core Feel: Hitstop (0.04s) & Screen Shake
            HitstopManager.TriggerHitstop();
            if (CameraShake.Instance) CameraShake.Instance.Shake(0.14f, 0.1f);

            if (AudioManager.Instance) AudioManager.Instance.PlayExplosion();

            if (explosionPrefab) Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            // Dopamine Loop: Overload Gauge +5%
            if (PlayerController.Instance) PlayerController.Instance.AddOverload(5f);

            // Drop Stars (musical scale)
            DropStars(isPointBlank);

            // Chain Reaction Explosions!
            TriggerChainReaction();

            // Smart Power-up Drop
            TrySmartDropPowerUp();

            // Unregister before despawn so counter is accurate
            if (EnemySpawner.Instance) EnemySpawner.Instance.UnregisterEnemy(gameObject);

            // Return to pool instead of destroying — zero GC allocation
            ObjectPoolManager.Despawn(gameObject);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void TriggerChainReaction()
        {
            if (_chainReactionDepth >= 3) return;
            _chainReactionDepth++;

            try
            {
                var radiusSqr = chainExplosionRadius * chainExplosionRadius;
                var myPos = transform.position;

                // Snapshot candidates without enumerating ActiveEnemies directly during damage phase
                var candidates = new List<Enemy>(8);
                candidates.AddRange(ActiveEnemies.Where(other => other != null && other != this && !other.IsDead)
                    .Where(other => ((Vector2)other.transform.position - (Vector2)myPos).sqrMagnitude <= radiusSqr));

                foreach (var target in candidates.Where(target => target != null && !target.IsDead))
                    target.TakeHitWithDamage(chainDamage);
            }
            finally
            {
                _chainReactionDepth--;
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void DropStars(bool vacuumDirectly = false)
        {
            var count = IsPointBlankDrop(vacuumDirectly) ? starDropCount * 2 : starDropCount;
            for (var i = 0; i < count; i++)
            {
                var offset = (Vector3)Random.insideUnitCircle * 0.4f;
                if (!starPrefab) continue;
                var starObj = ObjectPoolManager.Spawn(starPrefab, transform.position + offset, Quaternion.identity);
                var star = starObj ? starObj.GetComponent<StarPickup>() : null;
                if (star && vacuumDirectly) star.AttractToPlayerImmediately();
            }
        }

        private static bool IsPointBlankDrop(bool pointBlank)
        {
            return pointBlank;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void TrySmartDropPowerUp()
        {
            if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;

            if (PowerUp.ActivePowerUpCount >= 3) return;

            var player = PlayerController.Instance;
            var finalChance = dropChance;

            if (player)
            {
                // Low health: boost drop chance for survival
                if (player.currentLives <= 1)
                    finalChance = Mathf.Min(finalChance * 1.6f, 0.45f);

                // Low weapon: boost drop chance to get powerup loop started
                if (player.weaponLevel <= 2)
                    finalChance = Mathf.Min(finalChance * 1.35f, 0.40f);
            }

            if (Random.value > finalChance) return;

            var candidates = new List<GameObject>();
            foreach (var p in powerUpPrefabs)
            {
                if (!p) continue;
                var comp = p.GetComponent<PowerUp>();
                if (!comp) continue;

                if (player)
                {
                    switch (comp.powerUpType)
                    {
                        // Smart filtering: avoid dropping powerups the player currently does not need
                        case PowerUpType.Shield when player.HasShield:
                        case PowerUpType.Health when player.currentLives >= player.maxLives:
                            continue;
                    }

                    if (player.weaponLevel >= 5 && comp.powerUpType is PowerUpType.PowerCore or PowerUpType.TripleShot) continue;
                }

                candidates.Add(p);
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
}