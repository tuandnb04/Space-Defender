using System.Collections;
using UnityEngine;

namespace SpaceDefender
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Prefabs")] public GameObject[] enemyPrefabs;

        public GameObject bossPrefab;

        [Header("Spawn Timing")] public float minSpawnDelay = 0.8f;

        public float maxSpawnDelay = 1.6f;

        [Header("Difficulty Scaling")] public float maxDifficultyScore = 250f;

        [Header("Boss Settings")] public int bossScoreThreshold = 80;

        public int bossScoreInterval = 120;

        [Header("Spawn Position")] public float horizontalPadding = 0.8f;

        public float spawnYOffset = 1.0f;
        private float _maxX;

        private float _minX;
        private int _nextBossScore = 80;
        private float _spawnY;

        private bool IsBossActive { get; set; }

        private void Start()
        {
            _nextBossScore = bossScoreThreshold;
            CalculateSpawnBounds();
            StartCoroutine(SpawnRoutine());
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

        private IEnumerator SpawnRoutine()
        {
            yield return new WaitForSeconds(0.3f);

            while (true)
            {
                if (GameManager.Instance)
                {
                    if (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver)
                    {
                        yield return new WaitForSeconds(0.25f);
                        continue;
                    }

                    // Check boss spawning milestone
                    if (bossPrefab && !IsBossActive && GameManager.Instance.Score >= _nextBossScore)
                    {
                        SpawnBoss();
                        yield return new WaitForSeconds(2.0f);
                        continue;
                    }
                }

                // If boss is active, slow down regular enemy spawns to create a duel atmosphere
                if (IsBossActive)
                {
                    yield return new WaitForSeconds(2.5f);
                    continue;
                }

                SpawnRandomEnemy();

                // Dynamic difficulty calculation based on current score
                var difficulty = GameManager.Instance
                    ? Mathf.Clamp01(GameManager.Instance.Score / maxDifficultyScore)
                    : 0f;
                var scaledMin = Mathf.Lerp(minSpawnDelay, minSpawnDelay * 0.55f, difficulty);
                var scaledMax = Mathf.Lerp(maxSpawnDelay, maxSpawnDelay * 0.65f, difficulty);

                var delay = Random.Range(scaledMin, scaledMax);
                yield return new WaitForSeconds(delay);
            }
        }

        private void SpawnBoss()
        {
            IsBossActive = true;
            var spawnPos = new Vector3(0f, _spawnY, 0f);
            Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        }

        public void OnBossDefeated()
        {
            IsBossActive = false;
            var currentScore = GameManager.Instance ? GameManager.Instance.Score : _nextBossScore;
            _nextBossScore = currentScore + bossScoreInterval;
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

            // Slightly increase speed with difficulty
            if (!GameManager.Instance) return;
            var diff = Mathf.Clamp01(GameManager.Instance.Score / maxDifficultyScore);
            var enemyComp = enemyObj.GetComponent<Enemy>();
            if (enemyComp) enemyComp.speed *= 1f + diff * 0.35f;
        }

        public void ClearAllEnemies()
        {
            IsBossActive = false;
            _nextBossScore = bossScoreThreshold;

            var activeEnemies = FindObjectsByType<Enemy>();
            foreach (var enemy in activeEnemies)
                if (enemy != null)
                    Destroy(enemy.gameObject);

            var activeBosses = FindObjectsByType<BossController>();
            foreach (var boss in activeBosses)
                if (boss != null)
                    Destroy(boss.gameObject);

            var activePowerUps = FindObjectsByType<PowerUp>();
            foreach (var pup in activePowerUps)
                if (pup != null)
                    Destroy(pup.gameObject);

            var playerLasers = FindObjectsByType<Laser>();
            foreach (var laser in playerLasers)
                if (laser)
                    Destroy(laser.gameObject);

            if (UIManager.Instance != null) UIManager.Instance.ShowBossBar(false);
        }
    }
}