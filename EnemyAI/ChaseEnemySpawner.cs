using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseEnemySpawner : MonoBehaviour
{
    public GameObject enemyTemplate; // Reference to the enemy prefab
    public Transform spawnPoint; // Where enemies will spawn
    public List<EnemyPath> paths; // List of paths for enemies to choose from
    public float spawnInterval = 5f; // Time between spawns
    public int maxEnemies = 10; // Maximum number of enemies in the scene
    public int minEnemies = 5; // Minimum number of enemies before resuming spawn

    private bool canSpawn = true;

    private void Start()
    {
        if (enemyTemplate != null)
        {
            enemyTemplate.SetActive(false); // Disable the template enemy initially
            StartCoroutine(SpawnEnemy());
        }
        else
        {
            Debug.LogError("Enemy template is not assigned.");
        }
    }

    private IEnumerator SpawnEnemy()
    {
        while (true)
        {
            if (canSpawn)
            {
                // Check the number of active enemies
                int activeEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

                if (activeEnemyCount < maxEnemies)
                {
                    // Spawn the enemy
                    GameObject enemy = Instantiate(enemyTemplate, spawnPoint.position, spawnPoint.rotation);
                    enemy.SetActive(true);

                    // Randomly assign a path to the enemy
                    if (paths.Count > 0)
                    {
                        int randomIndex = Random.Range(0, paths.Count);
                        EnemyPath assignedPath = paths[randomIndex];

                        // Assign the path to the enemy
                        ChasingEnemyAI enemyAI = enemy.GetComponent<ChasingEnemyAI>();
                        if (enemyAI != null)
                        {
                            enemyAI.pathObject = assignedPath.gameObject;
                        }
                        else
                        {
                            Debug.LogError("ChasingEnemyAI component not found on the spawned enemy.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("No paths assigned to the spawner.");
                    }

                    // Pause spawn if the active enemy count reaches maxEnemies
                    if (activeEnemyCount + 1 >= maxEnemies)
                    {
                        canSpawn = false;
                    }
                }
            }
            else
            {
                // Check again if we need to resume spawning
                int activeEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
                if (activeEnemyCount < minEnemies)
                {
                    canSpawn = true;
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}