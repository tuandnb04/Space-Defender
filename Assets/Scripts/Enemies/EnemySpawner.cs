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
        public int bossWaveInterval = 4;

        // ─── Lightweight alive-enemy counter (zero GC, zero HashSet overhead) ───
        // Incremented by RegisterEnemy, decremented by UnregisterEnemy.
        // No RemoveWhere(), no LINQ, no allocations.

        private readonly List<GameObject> _shipPrefabs = new();
        private int _enemiesSpawnedThisWave;
        private bool _isBossActive;
        private BossController _activeBoss; // direct reference — no FindAnyObjectByType in loops

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

        // Simple, alloc-free alive count
        private int ActiveLivingEnemyCount { get; set; }

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
                    if (p && !p.name.ToLower().Contains("meteor"))
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
            if (enemyObj != null) ActiveLivingEnemyCount++;
        }

        public void UnregisterEnemy(GameObject enemyObj)
        {
            if (enemyObj && ActiveLivingEnemyCount > 0) ActiveLivingEnemyCount--;
        }

        // Legacy compatibility shim — no-op now that we use a counter
        public void OnEnemyRemoved() { }

        public void OnBossDefeated()
        {
            _isBossActive = false;
            _activeBoss = null;
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
                while (GameManager.Instance && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver))
                    yield return new WaitForSeconds(0.2f);

                if (UIManager.Instance) UIManager.Instance.UpdateWave(currentWave);

                var isBossWave = currentWave % bossWaveInterval == 0;

                if (isBossWave && bossPrefab)
                {
                    // =================== BOSS WAVE ===================
                    _isBossActive = true;

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

                    var spawnPos = new Vector3(0f, _spawnY, 0f);
                    var bossObj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
                    _activeBoss = bossObj.GetComponent<BossController>();
                    if (_activeBoss) _activeBoss.ConfigureBossVariant(variant, currentWave);

                    // Wait using the direct reference — no FindAnyObjectByType scan every 0.25s
                    while (_isBossActive)
                    {
                        if (!_activeBoss)
                        {
                            // Boss was destroyed (e.g. game reset) — treat as defeated
                            _isBossActive = false;
                            break;
                        }
                        yield return new WaitForSeconds(0.25f);
                    }
                }
                else
                {
                    // =================== REGULAR WAVE ===================
                    _totalWaveEnemies = baseEnemiesPerWave + (currentWave - 1) * enemyIncreasePerWave;
                    _enemiesSpawnedThisWave = 0;
                    ActiveLivingEnemyCount = 0;

                    if (UIManager.Instance)
                        UIManager.Instance.ShowWaveBanner($"WAVE {currentWave}", "ENGAGE HOSTILE FLEET", Color.cyan, 1.5f);

                    yield return new WaitForSeconds(0.35f);

                    while (_enemiesSpawnedThisWave < _totalWaveEnemies)
                    {
                        if (GameManager.Instance && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver))
                        {
                            yield return new WaitForSeconds(0.25f);
                            continue;
                        }

                        // Screen Density Cap
                        while (ActiveLivingEnemyCount >= 4)
                        {
                            yield return new WaitForSeconds(0.4f);
                            if (GameManager.Instance && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver))
                                break;
                        }

                        if (ActiveLivingEnemyCount == 0)
                        {
                            yield return new WaitForSeconds(0.2f);
                        }
                        else
                        {
                            var minDelay = Mathf.Max(0.40f, 0.75f - currentWave * 0.03f);
                            var maxDelay = Mathf.Max(0.60f, 1.05f - currentWave * 0.04f);
                            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

                            if (GameManager.Instance && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver))
                                continue;
                        }

                        SpawnWaveUnitOrSquad();

                        yield return null;
                    }

                    // Wait for remaining enemies with 4.5s safety timeout
                    var waitElapsed = 0f;
                    while (ActiveLivingEnemyCount > 0 && waitElapsed < 4.5f)
                    {
                        yield return new WaitForSeconds(0.15f);
                        waitElapsed += 0.15f;
                    }

                    ActiveLivingEnemyCount = 0;
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
            yield return new WaitForSeconds(1.2f);
            currentWave++;
        }

        private void SpawnWaveUnitOrSquad()
        {
            var remaining = _totalWaveEnemies - _enemiesSpawnedThisWave;

            // Occasional Splitting Meteor (~20%)
            if (splittingMeteorPrefab && Random.value < 0.20f)
            {
                var meteorX = Random.Range(_minX, _maxX);
                Instantiate(splittingMeteorPrefab, new Vector3(meteorX, _spawnY, 0f), Quaternion.identity);
                _enemiesSpawnedThisWave++;
                return;
            }

            switch (remaining)
            {
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

            // Use ObjectPool instead of Instantiate — dramatically reduces GC allocations
            var enemyObj = ObjectPoolManager.Spawn(prefab, pos, Quaternion.identity);
            if (!enemyObj) return;

            var enemyComp = enemyObj.GetComponent<Enemy>();
            if (!enemyComp) { ObjectPoolManager.Despawn(enemyObj); return; }

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
            _activeBoss = null;
            _enemiesSpawnedThisWave = 0;
            ActiveLivingEnemyCount = 0;

            if (_waveCoroutine != null)
            {
                StopCoroutine(_waveCoroutine);
                _waveCoroutine = null;
            }

            var activeEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude);
            foreach (var enemy in activeEnemies)
                if (enemy != null) ObjectPoolManager.Despawn(enemy.gameObject);

            var activeBosses = FindObjectsByType<BossController>(FindObjectsInactive.Exclude);
            foreach (var boss in activeBosses)
                if (boss != null) Destroy(boss.gameObject);

            var activeMeteors = FindObjectsByType<SplittingMeteor>(FindObjectsInactive.Exclude);
            foreach (var meteor in activeMeteors)
                if (meteor != null) Destroy(meteor.gameObject);

            var activePowerUps = FindObjectsByType<PowerUp>(FindObjectsInactive.Exclude);
            foreach (var pup in activePowerUps)
                if (pup != null) Destroy(pup.gameObject);

            var lasers = FindObjectsByType<Laser>(FindObjectsInactive.Exclude);
            foreach (var laser in lasers)
                if (laser != null) ObjectPoolManager.Despawn(laser.gameObject);

            if (UIManager.Instance == null) return;
            UIManager.Instance.ShowBossBar(false);
            UIManager.Instance.HideWaveBanner();
        }
    }
}