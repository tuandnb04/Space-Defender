using System.Collections;
using UnityEngine;

namespace SpaceDefender
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject[] enemyPrefabs;

        [Header("Spawn Timing")]
        public float minSpawnDelay = 0.8f;
        public float maxSpawnDelay = 1.6f;

        [Header("Spawn Position")]
        public float horizontalPadding = 0.8f;
        public float spawnYOffset = 1.0f;

        private float minX;
        private float maxX;
        private float spawnY;
        private Coroutine spawnCoroutine;

        private void Start()
        {
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
                }

                SpawnRandomEnemy();

                float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
                yield return new WaitForSeconds(delay);
            }
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
                Instantiate(prefab, spawnPos, Quaternion.identity);
            }
        }

        public void ClearAllEnemies()
        {
            Enemy[] activeEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null) Destroy(enemy.gameObject);
            }

            EnemyLaser[] activeLasers = FindObjectsByType<EnemyLaser>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var laser in activeLasers)
            {
                if (laser != null) Destroy(laser.gameObject);
            }

            Laser[] playerLasers = FindObjectsByType<Laser>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var laser in playerLasers)
            {
                if (laser != null) Destroy(laser.gameObject);
            }
        }
    }
}
