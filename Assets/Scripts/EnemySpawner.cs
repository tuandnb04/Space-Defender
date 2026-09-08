using System.Collections;
using UnityEngine;

namespace SpaceDefender
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject[] enemyPrefabs;
        public GameObject bossPrefab;

        [Header("Spawn Timing")]
        public float minSpawnDelay = 0.8f;
        public float maxSpawnDelay = 1.6f;

        [Header("Difficulty Scaling")]
        public float maxDifficultyScore = 250f;

        [Header("Boss Settings")]
        public int bossScoreThreshold = 80;
        public int bossScoreInterval = 120;

        [Header("Spawn Position")]
        public float horizontalPadding = 0.8f;
        public float spawnYOffset = 1.0f;

        private float minX;
        private float maxX;
        private float spawnY;
        private Coroutine spawnCoroutine;
        private bool isBossActive = false;
        private int nextBossScore = 80;

        public bool IsBossActive => isBossActive;

        private void Start()
        {
            nextBossScore = bossScoreThreshold;
            CalculateSpawnBounds();
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }

        private void CalculateSpawnBounds()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                float halfWidth = cam.orthographicSize * cam.aspect;
                minX = -halfWidth + horizontalPadding;
                maxX = halfWidth - horizontalPadding;
                spawnY = cam.orthographicSize + spawnYOffset;
            }
            else
            {
                minX = -4f;
                maxX = 4f;
                spawnY = 6f;
            }
        }

        private IEnumerator SpawnRoutine()
        {
            yield return new WaitForSeconds(0.3f);

            while (true)
            {
                if (GameManager.Instance != null)
                {
                    if (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver)
                    {
                        yield return new WaitForSeconds(0.25f);
                        continue;
                    }

                    // Check boss spawning milestone
                    if (bossPrefab != null && !isBossActive && GameManager.Instance.Score >= nextBossScore)
                    {
                        SpawnBoss();
                        yield return new WaitForSeconds(2.0f);
                        continue;
                    }
                }

                // If boss is active, slow down regular enemy spawns to create a duel atmosphere
                if (isBossActive)
                {
                    yield return new WaitForSeconds(2.5f);
                    continue;
                }

                SpawnRandomEnemy();

                // Dynamic difficulty calculation based on current score
                float difficulty = GameManager.Instance != null ? Mathf.Clamp01(GameManager.Instance.Score / maxDifficultyScore) : 0f;
                float scaledMin = Mathf.Lerp(minSpawnDelay, minSpawnDelay * 0.55f, difficulty);
                float scaledMax = Mathf.Lerp(maxSpawnDelay, maxSpawnDelay * 0.65f, difficulty);

                float delay = Random.Range(scaledMin, scaledMax);
                yield return new WaitForSeconds(delay);
            }
        }

        private void SpawnBoss()
        {
            isBossActive = true;
            Vector3 spawnPos = new Vector3(0f, spawnY, 0f);
            Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        }

        public void OnBossDefeated()
        {
            isBossActive = false;
            int currentScore = GameManager.Instance != null ? GameManager.Instance.Score : nextBossScore;
            nextBossScore = currentScore + bossScoreInterval;
        }

        private void SpawnRandomEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

            int index = Random.Range(0, enemyPrefabs.Length);
            GameObject prefab = enemyPrefabs[index];

            if (prefab != null)
            {
                float randomX = Random.Range(minX, maxX);
                Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);
                GameObject enemyObj = Instantiate(prefab, spawnPos, Quaternion.identity);

                // Slightly increase speed with difficulty
                if (GameManager.Instance != null)
                {
                    float diff = Mathf.Clamp01(GameManager.Instance.Score / maxDifficultyScore);
                    Enemy enemyComp = enemyObj.GetComponent<Enemy>();
                    if (enemyComp != null)
                    {
                        enemyComp.speed *= (1f + diff * 0.35f);
                    }
                }
            }
        }

        public void ClearAllEnemies()
        {
            isBossActive = false;
            nextBossScore = bossScoreThreshold;

            Enemy[] activeEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null) Destroy(enemy.gameObject);
            }

            BossController[] activeBosses = FindObjectsByType<BossController>(FindObjectsSortMode.None);
            foreach (var boss in activeBosses)
            {
                if (boss != null) Destroy(boss.gameObject);
            }

            PowerUp[] activePowerUps = FindObjectsByType<PowerUp>(FindObjectsSortMode.None);
            foreach (var pup in activePowerUps)
            {
                if (pup != null) Destroy(pup.gameObject);
            }

            EnemyLaser[] activeLasers = FindObjectsByType<EnemyLaser>(FindObjectsSortMode.None);
            foreach (var laser in activeLasers)
            {
                if (laser != null) Destroy(laser.gameObject);
            }

            Laser[] playerLasers = FindObjectsByType<Laser>(FindObjectsSortMode.None);
            foreach (var laser in playerLasers)
            {
                if (laser != null) Destroy(laser.gameObject);
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowBossBar(false);
            }
        }
    }
}
