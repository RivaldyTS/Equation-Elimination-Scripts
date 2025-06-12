using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject enemyTemplate; // Reference to the enemy object in the scene
    public Transform spawnPoint;
    public List<Path> paths; // List of paths for enemies to choose from
    public float spawnInterval = 5f;
    public int maxEnemies = 10; // Maximum number of enemies to be present in the scene
    public int minEnemies = 5;  // Minimum number of enemies before resuming spawn

    private bool canSpawn = true;

    private void Start()
    {
        if (enemyTemplate != null)
        {
            enemyTemplate.SetActive(false); // Disable the template enemy initially
            Debug.Log("Starting Spawner Coroutine");
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
                Debug.Log("Active enemy count: " + activeEnemyCount);

                if (activeEnemyCount < maxEnemies)
                {
                    // Spawn the enemy by enabling the template and moving it to the spawn point
                    GameObject enemy = Instantiate(enemyTemplate, spawnPoint.position, spawnPoint.rotation);
                    enemy.SetActive(true);
                    Debug.Log("Enemy spawned at " + spawnPoint.position);

                    // Randomly assign a path to the enemy
                    int randomIndex = Random.Range(0, paths.Count);
                    Path assignedPath = paths[randomIndex];

                    // Check if the Enemy component is attached
                    Enemy enemyComponent = enemy.GetComponent<Enemy>();
                    if (enemyComponent != null)
                    {
                        enemyComponent.SetPath(assignedPath);
                        Debug.Log("Assigned Path: " + assignedPath.name);
                    }
                    else
                    {
                        Debug.LogError("Enemy component not found on the spawned enemy.");
                    }

                    // Pause spawn if the active enemy count reaches maxEnemies
                    if (activeEnemyCount >= maxEnemies)
                    {
                        canSpawn = false;
                        Debug.Log("Pausing spawn, active enemy count: " + activeEnemyCount);
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
                    Debug.Log("Resuming spawn, active enemy count: " + activeEnemyCount);
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
