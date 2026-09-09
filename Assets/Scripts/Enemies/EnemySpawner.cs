using System.Collections;
using System.Collections.Generic;
using Combat;
using Core;
using Environment;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        private static EnemySpawner _instance;

        [Header("Prefabs")] public GameObject[] enemyPrefabs;

        public GameObject bossPrefab;
        public GameObject splittingMeteorPrefab;

        [Header("Spawn Position")] public float horizontalPadding = 0.8f;

        public float spawnYOffset = 1.0f;

        [Header("Wave Configuration")] public int currentWave = 1;

        public int baseEnemiesPerWave = 6;
        public int enemyIncreasePerWave = 3;
        public int bossWaveInterval = 4; // Boss on Wave 4, 8, 12...

        // Robust dynamic enemy tracking (Zero Leaks, Zero Hangs)
        private readonly HashSet<GameObject> _activeLivingEnemies = new();
        private readonly List<GameObject> _shipPrefabs = new();
        private int _enemiesSpawnedThisWave;
        private bool _isBossActive;

        private float _maxX;
        private float _minX;
        private float _spawnY;
        private int _totalWaveEnemies;

        private Coroutine _waveCoroutine;

        public static EnemySpawner Instance
        {
            get
            {
                if (!_instance) _instance = FindAnyObjectByType<EnemySpawner>(FindObjectsInactive.Include);
                return _instance;
            }
            private set => _instance = value;
        }

        private int ActiveLivingEnemyCount
        {
            get
            {
                _activeLivingEnemies.RemoveWhere(e => e == null);
                return _activeLivingEnemies.Count;
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            CalculateSpawnBounds();
            CacheShipPrefabs();
        }

        private void CacheShipPrefabs()
        {
            _shipPrefabs.Clear();
            if (enemyPrefabs != null)
                foreach (var p in enemyPrefabs)
                    if (p != null && !p.name.ToLower().Contains("meteor"))
                        _shipPrefabs.Add(p);

            if (_shipPrefabs.Count == 0 && enemyPrefabs is { Length: > 0 })
                _shipPrefabs.AddRange(enemyPrefabs);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        private void CalculateSpawnBounds()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                var halfWidth = cam.orthographicSize * cam.aspect;
                _minX = -halfWidth + horizontalPadding;
                _maxX = halfWidth - horizontalPadding;
                _spawnY = cam.orthographicSize + spawnYOffset;
            }
            else
            {
                _minX = -4f;
                _maxX = 4f;
                _spawnY = 6f;
            }
        }

        public void RegisterEnemy(GameObject enemyObj)
        {
            if (enemyObj != null) _activeLivingEnemies.Add(enemyObj);
        }

        public void UnregisterEnemy(GameObject enemyObj)
        {
            if (enemyObj != null) _activeLivingEnemies.Remove(enemyObj);
        }

        public void OnEnemyRemoved()
        {
            _activeLivingEnemies.RemoveWhere(e => e == null);
        }

        public void OnBossDefeated()
        {
            _isBossActive = false;
        }

        public void StartWaveSequence()
        {
            ClearAllEnemies();
            currentWave = 1;
            if (_waveCoroutine != null) StopCoroutine(_waveCoroutine);
            _waveCoroutine = StartCoroutine(MasterWaveRoutine());
        }

        private IEnumerator MasterWaveRoutine()
        {
            yield return new WaitForSeconds(0.3f);

            while (true)
            {
                // Wait while game is not running or paused
                while (GameManager.Instance && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver))
                    yield return new WaitForSeconds(0.2f);

                if (UIManager.Instance) UIManager.Instance.UpdateWave(currentWave);

                var isBossWave = currentWave % bossWaveInterval == 0;

                if (isBossWave && bossPrefab)
                {
                    // =================== BOSS WAVE ===================
                    _isBossActive = true;

                    // Warning Alert with Boss Variant Name
                    var bossTier = Mathf.Max(1, currentWave / bossWaveInterval);
                    var variant = (bossTier - 1) % 4;
                    var variantNames = new[]
                    {
                        "RED UFO MOTHERSHIP",
                        "BLUE UFO TITAN",
                        "GREEN HIVE QUEEN",
                        "GOLDEN UFO EMPEROR"
                    };

                    if (AudioManager.Instance) AudioManager.Instance.PlayBossWarning();
                    if (UIManager.Instance) UIManager.Instance.ShowBossWarning(variantNames[variant]);

                    yield return new WaitForSeconds(2.0f);

                    // Spawn Boss with wave tier scaling & variant
                    var spawnPos = new Vector3(0f, _spawnY, 0f);
                    var bossObj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
                    var bossComp = bossObj.GetComponent<BossController>();
                    if (bossComp != null) bossComp.ConfigureBossVariant(variant, currentWave);

                    // Wait until boss is defeated with failsafe assertion
                    while (_isBossActive)
                    {
                        var bossExists = FindAnyObjectByType<BossController>(FindObjectsInactive.Exclude) != null;
                        if (!bossExists)
                        {
                            _isBossActive = false;
                            break;
                        }

                        yield return new WaitForSeconds(0.25f);
                    }

                    // Boss defeated -> wave clear!
                }
                else
                {
                    // =================== REGULAR WAVE ===================
                    _totalWaveEnemies = baseEnemiesPerWave + (currentWave - 1) * enemyIncreasePerWave;
                    _enemiesSpawnedThisWave = 0;
                    _activeLivingEnemies.Clear();

                    // Wave Start Announcement (1.5s display, but spawns begin after 0.35s)
                    if (UIManager.Instance)
                        UIManager.Instance.ShowWaveBanner($"WAVE {currentWave}", "ENGAGE HOSTILE FLEET", Color.cyan,
                            1.5f);

                    yield return new WaitForSeconds(0.35f);

                    // Spawn fleet with dynamic pacing & squad formations
                    while (_enemiesSpawnedThisWave < _totalWaveEnemies)
                    {
                        if (GameManager.Instance &&
                            (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver))
                        {
                            yield return new WaitForSeconds(0.25f);
                            continue;
                        }

                        // Screen Density Cap: if 4 or more enemies are on screen, give player breathing room
                        while (ActiveLivingEnemyCount >= 4)
                        {
                            yield return new WaitForSeconds(0.4f);
                            if (GameManager.Instance &&
                                (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver))
                                break;
                        }

                        // Dynamic Pacing:
                        // If no active enemies are alive on screen, micro-breath 0.2s then spawn!
                        if (ActiveLivingEnemyCount == 0)
                        {
                            yield return new WaitForSeconds(0.2f);
                            SpawnWaveUnitOrSquad();
                        }
                        else
                        {
                            // Snappy combat delay: 0.40s - 0.85s
                            var minDelay = Mathf.Max(0.40f, 0.75f - currentWave * 0.03f);
                            var maxDelay = Mathf.Max(0.60f, 1.05f - currentWave * 0.04f);
                            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

                            if (GameManager.Instance &&
                                (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver))
                                continue;

                            SpawnWaveUnitOrSquad();
                        }

                        yield return null;
                    }

                    // Wait until all living enemies in wave are eliminated, with 4.5s max safety timeout!
                    var waitElapsed = 0f;
                    while (ActiveLivingEnemyCount > 0 && waitElapsed < 4.5f)
                    {
                        yield return new WaitForSeconds(0.15f);
                        waitElapsed += 0.15f;
                    }

                    // Clear any stray off-screen references
                    _activeLivingEnemies.Clear();

                    // Wave completed!
                }

                yield return StartCoroutine(WaveClearRoutine());
            }
        }

        private IEnumerator WaveClearRoutine()
        {
            var bonusScore = currentWave * 50;
            if (GameManager.Instance) GameManager.Instance.AwardWaveBonus(currentWave, bonusScore);

            if (AudioManager.Instance) AudioManager.Instance.PlayWaveClear();

            if (UIManager.Instance)
                UIManager.Instance.ShowWaveBanner($"WAVE {currentWave} CLEARED!", $"+{bonusScore} BONUS SCORE",
                    new Color(0.2f, 1f, 0.4f), 1.4f);

            // Snappy 1.2s intermission for item collection and pacing
            yield return new WaitForSeconds(1.2f);

            currentWave++;
        }

        private void SpawnWaveUnitOrSquad()
        {
            var remaining = _totalWaveEnemies - _enemiesSpawnedThisWave;

            // Occasional Splitting Meteor (approx 20% of waves)
            if (splittingMeteorPrefab != null && Random.value < 0.20f)
            {
                var meteorX = Random.Range(_minX, _maxX);
                Instantiate(splittingMeteorPrefab, new Vector3(meteorX, _spawnY, 0f), Quaternion.identity);
                _enemiesSpawnedThisWave++;
                return;
            }

            switch (remaining)
            {
                // Duo flank squad (Wave 2+, remaining >= 2)
                case >= 2 when currentWave >= 2 && Random.value < 0.35f:
                {
                    var leftX = Random.Range(_minX, -0.6f);
                    var rightX = Random.Range(0.6f, _maxX);
                    var isKamikazeDuo = currentWave >= 3 && Random.value < 0.45f;
                    var beh = isKamikazeDuo ? (EnemyBehaviorType?)EnemyBehaviorType.SinusoidalKamikaze : null;

                    SpawnSingleEnemy(new Vector3(leftX, _spawnY, 0f), beh);
                    SpawnSingleEnemy(new Vector3(rightX, _spawnY, 0f), beh);
                    _enemiesSpawnedThisWave += 2;
                    return;
                }
                // V-Formation (Wave 3+, remaining >= 3)
                case >= 3 when currentWave >= 3 && Random.value < 0.30f:
                {
                    var centerX = Random.Range(_minX + 1.2f, _maxX - 1.2f);
                    SpawnSingleEnemy(new Vector3(centerX, _spawnY, 0f));
                    SpawnSingleEnemy(new Vector3(centerX - 1.1f, _spawnY + 0.6f, 0f));
                    SpawnSingleEnemy(new Vector3(centerX + 1.1f, _spawnY + 0.6f, 0f));
                    _enemiesSpawnedThisWave += 3;
                    return;
                }
            }

            // Standard single enemy
            var randomX = Random.Range(_minX, _maxX);
            SpawnSingleEnemy(new Vector3(randomX, _spawnY, 0f));
            _enemiesSpawnedThisWave++;
        }

        private void SpawnSingleEnemy(Vector3 pos, EnemyBehaviorType? overrideBehavior = null)
        {
            if (_shipPrefabs.Count == 0) CacheShipPrefabs();
            if (_shipPrefabs.Count == 0) return;

            var index = Random.Range(0, _shipPrefabs.Count);
            var prefab = _shipPrefabs[index];
            if (!prefab) return;

            var enemyObj = Instantiate(prefab, pos, Quaternion.identity);
            var enemyComp = enemyObj.GetComponent<Enemy>();
            if (!enemyComp) return;

            // Progressive speed scaling
            var speedMultiplier = 1f + Mathf.Min((currentWave - 1) * 0.05f, 0.45f);
            enemyComp.speed *= speedMultiplier;

            if (overrideBehavior.HasValue)
            {
                enemyComp.behaviorType = overrideBehavior.Value;
                switch (overrideBehavior.Value)
                {
                    case EnemyBehaviorType.SinusoidalKamikaze:
                        enemyComp.maxHp = 2;
                        enemyComp.currentHp = 2;
                        break;
                    case EnemyBehaviorType.Sniper:
                        enemyComp.maxHp = 3;
                        enemyComp.currentHp = 3;
                        break;
                    case EnemyBehaviorType.Standard:
                        break;
                }
            }
            else
            {
                switch (currentWave)
                {
                    case >= 2 when Random.value < 0.32f:
                        enemyComp.behaviorType = EnemyBehaviorType.SinusoidalKamikaze;
                        enemyComp.maxHp = 2;
                        enemyComp.currentHp = 2;
                        break;
                    case >= 3 when Random.value < 0.28f:
                        enemyComp.behaviorType = EnemyBehaviorType.Sniper;
                        enemyComp.maxHp = 3;
                        enemyComp.currentHp = 3;
                        break;
                }
            }
        }

        public void ClearAllEnemies()
        {
            _isBossActive = false;
            _enemiesSpawnedThisWave = 0;
            _activeLivingEnemies.Clear();

            if (_waveCoroutine != null)
            {
                StopCoroutine(_waveCoroutine);
                _waveCoroutine = null;
            }

            var activeEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude);
            foreach (var enemy in activeEnemies)
                if (enemy != null)
                    Destroy(enemy.gameObject);

            var activeBosses = FindObjectsByType<BossController>(FindObjectsInactive.Exclude);
            foreach (var boss in activeBosses)
                if (boss != null)
                    Destroy(boss.gameObject);

            var activeMeteors = FindObjectsByType<SplittingMeteor>(FindObjectsInactive.Exclude);
            foreach (var meteor in activeMeteors)
                if (meteor != null)
                    Destroy(meteor.gameObject);

            var activePowerUps = FindObjectsByType<PowerUp>(FindObjectsInactive.Exclude);
            foreach (var pup in activePowerUps)
                if (pup != null)
                    Destroy(pup.gameObject);

            var lasers = FindObjectsByType<Laser>(FindObjectsInactive.Exclude);
            foreach (var laser in lasers)
                if (laser != null)
                    Destroy(laser.gameObject);

            if (UIManager.Instance == null) return;
            UIManager.Instance.ShowBossBar(false);
            UIManager.Instance.HideWaveBanner();
        }
    }
}