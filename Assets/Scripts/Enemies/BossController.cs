using System.Collections;
using Combat;
using Core;
using Environment;
using Player;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemies
{
    public class BossController : MonoBehaviour
    {
        [FormerlySerializedAs("maxHP")] [Header("Boss Stats")]
        public int maxHp = 45;

        [FormerlySerializedAs("currentHP")] public int currentHp = 45;
        public int scoreValue = 250;
        public string bossName = "RED UFO MOTHERSHIP";
        public int variantIndex; // 0 = Red, 1 = Blue, 2 = Green, 3 = Yellow

        [Header("Variant Sprites")] public Sprite[] bossVariantSprites;

        [Header("Movement")] public float entrySpeed = 2.0f;
        public float targetY = 2.8f;
        public float patrolSpeed = 1.8f;
        public float patrolAmplitude = 2.5f;

        [Header("Shooting & Phases")] public GameObject enemyLaserPrefab;
        public float attackInterval = 1.2f;
        public Transform leftFirePoint;
        public Transform rightFirePoint;
        public GameObject shieldVisual;
        public GameObject dronePrefab;

        [Header("Prefabs & Effects")] public GameObject starPrefab;
        public GameObject explosionPrefab;
        public GameObject floatingScorePrefab;
        public GameObject[] dropPowerUpPrefabs;

        private int _activeDrones;
        private Collider2D _collider;
        private HitFlashEffect _flashEffect;
        private float _flowerAngleOffset;
        private bool _hasEnteredPhase3;
        private bool _isDead;
        private bool _isEntering = true;
        private bool _isShieldActive;
        private float _nextAttackTime;
        private float _patrolTimer;
        private int _phase2TickCount;
        private int _phase3Cycle;
        private int _shieldHitsRemaining = 15;
        private float _spiralAngle;
        private SpriteRenderer _spriteRenderer;
        private bool _sweepDirectionLeftToRight;
        private Coroutine _sweepingBeamCoroutine;

        public static BossController ActiveBoss { get; private set; }

        private int CurrentPhase
        {
            get
            {
                var pct = (float)currentHp / maxHp;
                return pct switch
                {
                    > 0.66f => 1,
                    > 0.33f => 2,
                    _ => 3
                };
            }
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            _flashEffect = GetComponent<HitFlashEffect>();
            if (_flashEffect == null) _flashEffect = gameObject.AddComponent<HitFlashEffect>();

            if (shieldVisual != null) shieldVisual.SetActive(false);
        }

        private void Start()
        {
            currentHp = maxHp;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowBossBar(true, bossName);
                UIManager.Instance.UpdateBossHp(currentHp, maxHp);
            }

            _nextAttackTime = Time.time + 1.8f;
        }

        private void Update()
        {
            if (_isDead) return;
            if (GameManager.Instance && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver ||
                                         GameManager.Instance.IsPaused)) return;

            if (_isEntering)
            {
                var pos = transform.position;
                pos.y = Mathf.MoveTowards(pos.y, targetY, entrySpeed * Time.deltaTime);
                transform.position = pos;

                if (Mathf.Abs(pos.y - targetY) < 0.05f) _isEntering = false;
                return;
            }

            // Movement behavior by phase
            _patrolTimer += Time.deltaTime;
            var speedMultiplier = CurrentPhase == 2 ? 1.4f : 1.0f;
            var newX = Mathf.Sin(_patrolTimer * patrolSpeed * speedMultiplier) * patrolAmplitude;
            transform.position = new Vector3(newX, targetY, transform.position.z);

            // Check Phase 3 transition
            if (CurrentPhase == 3 && !_hasEnteredPhase3) EnterPhase3();

            // Attack patterns
            if (!(Time.time >= _nextAttackTime)) return;
            switch (CurrentPhase)
            {
                case 1:
                    _nextAttackTime = Time.time + attackInterval;
                    FirePhase1Volley();
                    break;
                case 2:
                    _phase2TickCount++;
                    if (_phase2TickCount >= 10)
                    {
                        _phase2TickCount = 0;
                        _nextAttackTime = Time.time + 0.45f;
                        FireDanmakuFlowerNova();
                    }
                    else
                    {
                        _nextAttackTime = Time.time + (variantIndex == 2 || variantIndex == 3 ? 0.14f : 0.18f);
                        FirePhase2Spiral();
                    }

                    break;
                case 3:
                    _phase3Cycle = (_phase3Cycle + 1) % 3;
                    switch (_phase3Cycle)
                    {
                        case 0:
                            _nextAttackTime = Time.time + attackInterval * 0.95f;
                            FirePhase3Volley();
                            break;
                        case 1:
                            _nextAttackTime = Time.time + attackInterval * 1.50f;
                            if (_sweepingBeamCoroutine != null) StopCoroutine(_sweepingBeamCoroutine);
                            _sweepingBeamCoroutine = StartCoroutine(SweepingLaserRoutine());
                            break;
                        case 2:
                            _nextAttackTime = Time.time + attackInterval * 0.90f;
                            FireDanmakuFlowerNova();
                            break;
                    }

                    break;
            }
        }

        private void OnEnable()
        {
            ActiveBoss = this;
        }

        private void OnDisable()
        {
            if (ActiveBoss == this) ActiveBoss = null;
        }

        public void ConfigureBossVariant(int variant, int wave)
        {
            variantIndex = Mathf.Clamp(variant, 0, 3);

            // Update Sprite if available
            if (bossVariantSprites != null && variantIndex < bossVariantSprites.Length &&
                bossVariantSprites[variantIndex] != null)
                if (_spriteRenderer != null)
                    _spriteRenderer.sprite = bossVariantSprites[variantIndex];

            switch (variantIndex)
            {
                case 0: // Wave 4: Red UFO Mothership
                    bossName = "RED UFO MOTHERSHIP";
                    maxHp = 45 + Mathf.Max(0, (wave - 4) * 5);
                    scoreValue = 250;
                    attackInterval = 1.2f;
                    patrolSpeed = 1.8f;
                    patrolAmplitude = 2.4f;
                    _shieldHitsRemaining = 15;
                    break;

                case 1: // Wave 8: Blue UFO Titan (Tanky, Heavy Spread)
                    bossName = "BLUE UFO TITAN";
                    maxHp = 70 + Mathf.Max(0, (wave - 8) * 7);
                    scoreValue = 400;
                    attackInterval = 1.4f;
                    patrolSpeed = 1.3f;
                    patrolAmplitude = 2.8f;
                    _shieldHitsRemaining = 22;
                    break;

                case 2: // Wave 12: Green Hive Queen (Agile, Fast Spiral Danmaku)
                    bossName = "GREEN HIVE QUEEN";
                    maxHp = 95 + Mathf.Max(0, (wave - 12) * 8);
                    scoreValue = 600;
                    attackInterval = 1.0f;
                    patrolSpeed = 2.2f;
                    patrolAmplitude = 2.5f;
                    _shieldHitsRemaining = 18;
                    break;

                case 3: // Wave 16: Golden UFO Emperor (Supreme Boss, Dual Counter-Spiral)
                    bossName = "GOLDEN UFO EMPEROR";
                    maxHp = 135 + Mathf.Max(0, (wave - 16) * 10);
                    scoreValue = 1000;
                    attackInterval = 0.85f;
                    patrolSpeed = 2.0f;
                    patrolAmplitude = 2.6f;
                    _shieldHitsRemaining = 28;
                    break;
            }

            currentHp = maxHp;
            if (UIManager.Instance == null) return;
            UIManager.Instance.ShowBossBar(true, bossName);
            UIManager.Instance.UpdateBossHp(currentHp, maxHp);
        }

        private void FirePhase1Volley()
        {
            if (_isDead || !enemyLaserPrefab) return;

            var leftPos = leftFirePoint ? leftFirePoint.position : transform.position + new Vector3(-0.6f, -0.4f, 0f);
            var rightPos = rightFirePoint ? rightFirePoint.position : transform.position + new Vector3(0.6f, -0.4f, 0f);

            // Wing sniper lasers straight down
            SpawnLaser(leftPos, Quaternion.identity);
            SpawnLaser(rightPos, Quaternion.identity);

            switch (variantIndex)
            {
                // Blue Titan: Heavy 4-way spread
                case 1:
                {
                    for (var angle = -30f; angle <= 30f; angle += 20f)
                        SpawnLaser(transform.position + Vector3.down * 0.4f, Quaternion.Euler(0f, 0f, angle));
                    break;
                }
                // Golden Emperor: 5-way spread
                case 3:
                {
                    for (var angle = -40f; angle <= 40f; angle += 20f)
                        SpawnLaser(transform.position + Vector3.down * 0.4f, Quaternion.Euler(0f, 0f, angle));
                    break;
                }
                // Red / Green standard spread
                default:
                    SpawnLaser(transform.position + Vector3.down * 0.4f, Quaternion.identity);
                    SpawnLaser(transform.position + Vector3.down * 0.4f, Quaternion.Euler(0f, 0f, 35f));
                    SpawnLaser(transform.position + Vector3.down * 0.4f, Quaternion.Euler(0f, 0f, -35f));
                    break;
            }

            if (AudioManager.Instance) AudioManager.Instance.PlayEnemyShoot();
        }

        private void FirePhase2Spiral()
        {
            if (_isDead || !enemyLaserPrefab) return;

            // Spiral Danmaku rotation
            var step = variantIndex switch
            {
                2 => 26f, // Green Hive: faster spiral
                3 => 30f, // Golden Emperor: dual counter-spiral
                _ => 22f // Standard 22 deg
            };

            _spiralAngle += step;
            if (_spiralAngle >= 360f) _spiralAngle -= 360f;

            switch (variantIndex)
            {
                // Golden Emperor: Counter-rotating dual spirals
                case 3:
                    SpawnLaser(transform.position, Quaternion.Euler(0f, 0f, _spiralAngle));
                    SpawnLaser(transform.position, Quaternion.Euler(0f, 0f, _spiralAngle + 180f));
                    SpawnLaser(transform.position, Quaternion.Euler(0f, 0f, -_spiralAngle));
                    SpawnLaser(transform.position, Quaternion.Euler(0f, 0f, -_spiralAngle + 180f));
                    break;
                // Green Hive Queen: 3-way spiral
                case 2:
                    SpawnLaser(transform.position, Quaternion.Euler(0f, 0f, _spiralAngle));
                    SpawnLaser(transform.position, Quaternion.Euler(0f, 0f, _spiralAngle + 120f));
                    SpawnLaser(transform.position, Quaternion.Euler(0f, 0f, _spiralAngle + 240f));
                    break;
                // Red Mothership & Blue Titan: 2-way spiral
                default:
                    SpawnLaser(transform.position, Quaternion.Euler(0f, 0f, _spiralAngle));
                    SpawnLaser(transform.position, Quaternion.Euler(0f, 0f, _spiralAngle + 180f));
                    break;
            }

            if (AudioManager.Instance && Random.value < 0.35f)
                AudioManager.Instance.PlayEnemyShoot();
        }

        private void EnterPhase3()
        {
            _hasEnteredPhase3 = true;
            _isShieldActive = true;

            if (shieldVisual != null) shieldVisual.SetActive(true);

            if (floatingScorePrefab != null)
                FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 1.2f,
                    $"{bossName} ENRAGED!", Color.red);

            if (CameraShake.Instance) CameraShake.Instance.Shake(0.35f, 0.2f);

            // Summon escort drones (Emperor summons 4, others summon 2)
            var droneCount = variantIndex == 3 ? 4 : 2;
            if (droneCount == 4)
            {
                SpawnDrone(transform.position + new Vector3(-2.0f, -0.4f, 0f));
                SpawnDrone(transform.position + new Vector3(2.0f, -0.4f, 0f));
                SpawnDrone(transform.position + new Vector3(-1.2f, -1.0f, 0f));
                SpawnDrone(transform.position + new Vector3(1.2f, -1.0f, 0f));
            }
            else
            {
                SpawnDrone(transform.position + new Vector3(-1.8f, -0.5f, 0f));
                SpawnDrone(transform.position + new Vector3(1.8f, -0.5f, 0f));
            }
        }

        private void SpawnDrone(Vector3 spawnPos)
        {
            if (!dronePrefab) return;
            var drone = Instantiate(dronePrefab, spawnPos, Quaternion.identity);
            _activeDrones++;
            var enemyComp = drone.GetComponent<Enemy>();
            if (!enemyComp) return;
            enemyComp.maxHp = variantIndex >= 2 ? 4 : 3;
            enemyComp.currentHp = enemyComp.maxHp;
            enemyComp.OnDeathCallback += OnDroneDestroyed;
        }

        private void OnDroneDestroyed()
        {
            _activeDrones = Mathf.Max(0, _activeDrones - 1);
            if (_activeDrones <= 0) BreakShield();
        }

        private void BreakShield()
        {
            if (!_isShieldActive) return;
            _isShieldActive = false;
            _activeDrones = 0;
            if (shieldVisual != null) shieldVisual.SetActive(false);
            if (floatingScorePrefab != null)
                FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 1.0f, "SHIELD BROKEN!",
                    Color.cyan);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayShieldDown();
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.25f, 0.18f);
        }

        private void FirePhase3Volley()
        {
            if (_isDead || !enemyLaserPrefab) return;

            // Spread barrage
            var count = variantIndex == 3 ? 7 : 5;
            var maxAngle = variantIndex == 3 ? 50f : 40f;
            var step = maxAngle * 2f / (count - 1);

            for (var angle = -maxAngle; angle <= maxAngle + 0.1f; angle += step)
                SpawnLaser(transform.position + Vector3.down * 0.5f, Quaternion.Euler(0f, 0f, angle));

            if (AudioManager.Instance) AudioManager.Instance.PlayEnemyShoot();
        }

        private void FireDanmakuFlowerNova()
        {
            if (_isDead || !enemyLaserPrefab) return;

            _flowerAngleOffset += 18f;
            const int petals = 12;
            const float angleStep = 360f / petals;

            var bossColor = variantIndex switch
            {
                1 => new Color(0.3f, 0.7f, 1f), // Blue Titan
                2 => new Color(0.2f, 1f, 0.4f), // Green Hive
                3 => new Color(1f, 0.85f, 0.2f), // Golden Emperor
                _ => new Color(1f, 0.3f, 0.3f) // Red Mothership
            };

            // Ring 1: Inner rotating flower petals
            for (var i = 0; i < petals; i++)
            {
                var rot = Quaternion.Euler(0f, 0f, i * angleStep + _flowerAngleOffset);
                SpawnLaser(transform.position, rot, 0.85f, bossColor);
            }

            // Ring 2: Interleaved fast outer petals
            for (var i = 0; i < petals; i++)
            {
                var rot = Quaternion.Euler(0f, 0f, i * angleStep + angleStep * 0.5f + _flowerAngleOffset);
                SpawnLaser(transform.position, rot, 1.25f, Color.white);
            }

            if (AudioManager.Instance) AudioManager.Instance.PlayEnemyShoot();
            if (CameraShake.Instance) CameraShake.Instance.Shake(0.12f, 0.08f);
        }

        private IEnumerator SweepingLaserRoutine()
        {
            if (_isDead || !enemyLaserPrefab) yield break;

            // Telegraph charge warning
            if (_flashEffect) _flashEffect.Flash(0.35f, Color.red);
            if (AudioManager.Instance) AudioManager.Instance.PlayBossWarning();
            if (CameraShake.Instance) CameraShake.Instance.Shake(0.18f, 0.25f);

            if (floatingScorePrefab != null)
                FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 1.1f,
                    "DASH THROUGH BEAM!", new Color(1f, 0.3f, 0.2f));

            yield return new WaitForSeconds(0.45f);
            if (_isDead) yield break;

            _sweepDirectionLeftToRight = !_sweepDirectionLeftToRight;
            var startAngle = _sweepDirectionLeftToRight ? -48f : 48f;
            var endAngle = _sweepDirectionLeftToRight ? 48f : -48f;
            const int beamShots = 18;
            const float shotInterval = 0.042f;

            for (var i = 0; i < beamShots; i++)
            {
                if (_isDead) yield break;
                var t = (float)i / (beamShots - 1);
                var angle = Mathf.Lerp(startAngle, endAngle, t);
                var rot = Quaternion.Euler(0f, 0f, angle);

                // Double dense projectile stream forming a sweeping beam wall
                SpawnLaser(transform.position + Vector3.down * 0.4f, rot, 1.3f, new Color(1f, 0.25f, 0.15f));
                SpawnLaser(transform.position + Vector3.down * 0.5f, rot, 1.0f, Color.yellow);

                if (i % 3 == 0 && AudioManager.Instance)
                    AudioManager.Instance.PlayEnemyShoot();

                yield return new WaitForSeconds(shotInterval);
            }

            _sweepingBeamCoroutine = null;
        }

        private void SpawnLaser(Vector3 pos, Quaternion rot, float speedMultiplier = 1f, Color? laserColor = null)
        {
            var laserObj = ObjectPoolManager.Spawn(enemyLaserPrefab, pos, rot);
            var laser = laserObj ? laserObj.GetComponent<Laser>() : null;
            if (!laser) return;
            laser.isEnemyLaser = true;
            if (!Mathf.Approximately(speedMultiplier, 1f)) laser.speed *= speedMultiplier;
            if (!laserColor.HasValue) return;
            var sr = laser.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = laserColor.Value;
        }

        public void TakeHit(int damage = 1)
        {
            if (_isDead) return;

            // If shield is active, EMP bomb shatters it immediately, or hits shatter it
            if (_isShieldActive)
            {
                if (damage >= 8)
                {
                    BreakShield();
                    return;
                }

                _shieldHitsRemaining -= damage;
                if (_shieldHitsRemaining <= 0 || _activeDrones <= 0)
                {
                    BreakShield();
                    return;
                }

                if (CameraShake.Instance) CameraShake.Instance.Shake(0.05f, 0.03f);
                if (_flashEffect) _flashEffect.Flash(0.06f, Color.cyan);
                return;
            }

            currentHp -= damage;

            if (UIManager.Instance) UIManager.Instance.UpdateBossHp(currentHp, maxHp);

            if (CameraShake.Instance) CameraShake.Instance.Shake(0.08f, 0.06f);

            if (_flashEffect) _flashEffect.Flash(0.08f, Color.white);

            if (currentHp <= 0) Die();
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            if (_collider != null) _collider.enabled = false;
            if (shieldVisual) shieldVisual.SetActive(false);
            if (UIManager.Instance) UIManager.Instance.ShowBossBar(false);

            StartCoroutine(CinematicBossDeathRoutine());
        }

        private IEnumerator CinematicBossDeathRoutine()
        {
            // 1. Slo-mo Bullet Time Finisher (0.15x for intense visceral climax)
            Time.timeScale = 0.15f;

            if (PostProcessingManager.Instance != null)
                PostProcessingManager.Instance.TriggerBossFinisherGlitch();

            // 2. Multi-stage staggered chain explosions across the UFO hull
            var offsets = new[]
            {
                Vector3.zero,
                new Vector3(-0.7f, 0.35f, 0f),
                new Vector3(0.75f, -0.25f, 0f),
                new Vector3(-0.5f, -0.45f, 0f),
                new Vector3(0.45f, 0.45f, 0f)
            };

            foreach (var offset in offsets)
            {
                if (explosionPrefab != null)
                    Instantiate(explosionPrefab, transform.position + offset, Quaternion.identity);

                HitSparkEffect.SpawnSpark(transform.position + offset, new Color(1f, 0.85f, 0.2f));

                if (AudioManager.Instance) AudioManager.Instance.PlayExplosion();
                if (CameraShake.Instance) CameraShake.Instance.Shake(0.35f, 0.18f);
                if (_flashEffect) _flashEffect.Flash(0.06f, Color.white);

                yield return new WaitForSecondsRealtime(0.09f);
            }

            // 3. Final Massive Core Blast
            if (explosionPrefab != null)
            {
                var finalBlast = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                finalBlast.transform.localScale = Vector3.one * 1.8f;
            }

            HitSparkEffect.SpawnSpark(transform.position, Color.white);
            HitSparkEffect.SpawnSpark(transform.position + Vector3.up * 0.2f, new Color(1f, 0.9f, 0.3f));

            if (CameraShake.Instance) CameraShake.Instance.Shake(0.65f, 0.4f);
            if (AudioManager.Instance) AudioManager.Instance.PlayExplosion();

            // 4. Rewards: Score, Stars, Bomb, Overload
            var finalScore = scoreValue;
            if (ComboManager.Instance)
                finalScore = ComboManager.Instance.RegisterKill(scoreValue, transform.position, floatingScorePrefab);

            if (GameManager.Instance) GameManager.Instance.AddScore(finalScore);
            if (AchievementManager.Instance) AchievementManager.Instance.UnlockAchievement("BOSS_SLAYER");

            var player = FindAnyObjectByType<PlayerController>();
            if (player)
            {
                player.AddBomb();
                player.AddOverload(50f);
            }

            // Rain of 12 Stars flowing to player
            if (starPrefab != null)
                for (var i = 0; i < 12; i++)
                {
                    var offset = (Vector3)Random.insideUnitCircle * 1.3f;
                    var starObj = ObjectPoolManager.Spawn(starPrefab, transform.position + offset, Quaternion.identity);
                    var star = starObj ? starObj.GetComponent<StarPickup>() : null;
                    if (star != null) star.AttractToPlayerImmediately();
                }

            // Guaranteed power-up drop
            if (dropPowerUpPrefabs is { Length: > 0 })
            {
                var randomIndex = Random.Range(0, dropPowerUpPrefabs.Length);
                var pUpPrefab = dropPowerUpPrefabs[randomIndex];
                if (pUpPrefab) Instantiate(pUpPrefab, transform.position, Quaternion.identity);
            }

            // 5. Restore Normal TimeScale
            Time.timeScale = 1.0f;

            // 6. Notify Spawner
            var spawner = FindAnyObjectByType<EnemySpawner>();
            if (spawner) spawner.OnBossDefeated();

            // 7. Trigger Rogue-lite In-Run Tech Perk Selection Modal!
            if (UIManager.Instance != null) UIManager.Instance.ShowPerkModal();

            Destroy(gameObject);
        }
    }
}