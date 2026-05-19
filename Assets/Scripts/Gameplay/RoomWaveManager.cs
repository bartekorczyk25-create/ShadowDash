using System.Collections;
using System.Collections.Generic;
using ShadowDash.Enemies;
using UnityEngine;

namespace ShadowDash.Gameplay
{
    public sealed class RoomWaveManager : MonoBehaviour
    {
        [SerializeField] private EnemyHealth enemyPrefab = null;
        [SerializeField] private Transform[] spawnPoints = new Transform[0];
        [SerializeField] private int enemiesPerWave = 5;
        [SerializeField] private float timeBetweenSpawns = 0.65f;
        [SerializeField] private float startDelay = 1f;

        private readonly List<EnemyHealth> spawnedEnemies = new List<EnemyHealth>();
        private bool spawningComplete;
        private bool roomCleared;

        private void Start()
        {
            StartCoroutine(SpawnWave());
        }

        private void Update()
        {
            if (!spawningComplete || roomCleared)
            {
                return;
            }

            spawnedEnemies.RemoveAll(enemy => enemy == null || !enemy.IsAlive);

            if (spawnedEnemies.Count == 0)
            {
                roomCleared = true;
                Debug.Log("ROOM CLEAR");
            }
        }

        private IEnumerator SpawnWave()
        {
            yield return new WaitForSeconds(startDelay);

            if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("RoomWaveManager is missing an enemy prefab or spawn points.");
                spawningComplete = true;
                yield break;
            }

            for (int i = 0; i < enemiesPerWave; i++)
            {
                Transform spawnPoint = spawnPoints[i % spawnPoints.Length];
                EnemyHealth enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                spawnedEnemies.Add(enemy);

                yield return new WaitForSeconds(timeBetweenSpawns);
            }

            spawningComplete = true;
        }
    }
}
