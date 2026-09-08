using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SpaceDefender
{
    public class EnemySpawner : MonoBehaviour
    {
        private static EnemySpawner _instance;

        [Header("Prefabs")]
        public GameObject[] enemyPrefabs;
        public GameObject bossPrefab;

        [Header("Spawn Position")]
        public float horizontalPadding = 0.8f;
        public float spawnYOffset = 1.0f;

        [Header("Wave Configuration")]
        public int currentWave = 1;
        public int baseEnemiesPerWave = 6;
        public int enemyIncreasePerWave = 3;
        public int bossWaveInterval = 4; // Boss on Wave 4, 8, 12...

        private float _maxX;
        private float _minX;
        private float _spawnY;

        private Coroutine _waveCoroutine;
        private int _totalWaveEnemies;
        private int _enemiesSpawnedThisWave;
        private int _livingEnemies;
        private bool _isBossActive;
        private bool _isWaveIntermission;
        private bool _isClearing;

        public static EnemySpawner Instance
        {
            get
            {
                if (!_instance) _instance = FindAnyObjectByType<EnemySpawner>(FindObjectsInactive.Include);
                return _instance;
            }
            private set => _instance = value;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            CalculateSpawnBounds();
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

        public void StartWaveSequence()
        {
            ClearAllEnemies();
            currentWave = 1;
            if (_waveCoroutine != null) StopCoroutine(_waveCoroutine);
            _waveCoroutine = StartCoroutine(MasterWaveRoutine());
        }

        private IEnumerator MasterWaveRoutine()
        {
            yield return new WaitForSeconds(0.4f);

            while (true)
            {
                // Wait while game is not running or paused
                while (GameManager.Instance && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver))
                {
                    yield return new WaitForSeconds(0.2f);
                }

                _isClearing = false;
                _isWaveIntermission = false;

                if (UIManager.Instance) UIManager.Instance.UpdateWave(currentWave);

                var isBossWave = (currentWave % bossWaveInterval == 0);

                if (isBossWave && bossPrefab != null)
                {
                    // =================== BOSS WAVE ===================
                    _isBossActive = true;

                    // Warning Alert
                    if (AudioManager.Instance) AudioManager.Instance.PlayBossWarning();
                    if (UIManager.Instance) UIManager.Instance.ShowBossWarning();

                    yield return new WaitForSeconds(2.5f);

                    // Spawn Boss
                    var spawnPos = new Vector3(0f, _spawnY, 0f);
                    Instantiate(bossPrefab, spawnPos, Quaternion.identity);

                    // Wait until boss is defeated
                    while (_isBossActive)
                    {
                        yield return new WaitForSeconds(0.3f);
                    }

                    // Boss defeated -> wave clear!
                }
                else
                {
                    // =================== REGULAR WAVE ===================
                    _totalWaveEnemies = baseEnemiesPerWave + (currentWave - 1) * enemyIncreasePerWave;
                    _enemiesSpawnedThisWave = 0;
                    _livingEnemies = 0;

                    // Wave Start Announcement
                    if (UIManager.Instance)
                    {
                        UIManager.Instance.ShowWaveBanner($"WAVE {currentWave}", "ENGAGE HOSTILE FLEET", Color.cyan);
                    }

                    yield return new WaitForSeconds(1.2f);

                    // Spawn fleet
                    while (_enemiesSpawnedThisWave < _totalWaveEnemies)
                    {
                        if (GameManager.Instance && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver))
                        {
                            yield return new WaitForSeconds(0.25f);
                            continue;
                        }

                        SpawnRandomEnemy();
                        _enemiesSpawnedThisWave++;
                        _livingEnemies++;

                        // Delay scales with wave difficulty
                        var baseMinDelay = Mathf.Max(0.6f, 1.2f - (currentWave * 0.05f));
                        var baseMaxDelay = Mathf.Max(1.0f, 1.9f - (currentWave * 0.07f));
                        var delay = Random.Range(baseMinDelay, baseMaxDelay);
                        yield return new WaitForSeconds(delay);
                    }

                    // Wait until all living enemies in wave are eliminated
                    while (_livingEnemies > 0)
                    {
                        yield return new WaitForSeconds(0.2f);
                    }

                    // Wave completed!
                }

                yield return StartCoroutine(WaveClearRoutine());
            }
        }

        private IEnumerator WaveClearRoutine()
        {
            _isWaveIntermission = true;

            var bonusScore = currentWave * 50;
            if (GameManager.Instance) GameManager.Instance.AwardWaveBonus(currentWave, bonusScore);

            if (AudioManager.Instance) AudioManager.Instance.PlayWaveClear();

            if (UIManager.Instance)
            {
                UIManager.Instance.ShowWaveBanner($"WAVE {currentWave} CLEARED!", $"+{bonusScore} BONUS SCORE", new Color(0.2f, 1f, 0.4f), 2.2f);
            }

            // 2.5s intermission for powerup collection and player recovery
            yield return new WaitForSeconds(2.5f);

            currentWave++;
            _isWaveIntermission = false;
        }

        public void OnEnemyRemoved()
        {
            if (_isClearing || _isBossActive || _isWaveIntermission) return;
            _livingEnemies = Mathf.Max(0, _livingEnemies - 1);
        }

        public void OnBossDefeated()
        {
            _isBossActive = false;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void SpawnRandomEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

            var index = Random.Range(0, enemyPrefabs.Length);
            var prefab = enemyPrefabs[index];
            if (!prefab) return;

            var randomX = Random.Range(_minX, _maxX);
            var spawnPos = new Vector3(randomX, _spawnY, 0f);
            var enemyObj = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Slightly increase speed with wave
            var enemyComp = enemyObj.GetComponent<Enemy>();
            if (!enemyComp) return;
            var speedMultiplier = 1f + Mathf.Min((currentWave - 1) * 0.06f, 0.5f);
            enemyComp.speed *= speedMultiplier;
        }

        public void ClearAllEnemies()
        {
            _isClearing = true;
            _isBossActive = false;
            _isWaveIntermission = false;
            _livingEnemies = 0;
            _enemiesSpawnedThisWave = 0;

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