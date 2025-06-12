using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // Required for UnityEvent
using TMPro; // Required for TextMeshPro

public class WaveEnemySpawner : MonoBehaviour
{
    public List<GameObject> enemyTemplates; // List of enemy prefabs to spawn
    public List<Transform> spawnPoints; // List of spawn points for enemies
    public List<EnemyPath> paths; // List of paths for enemies to choose from
    public float spawnInterval = 2f; // Time between spawns within a wave
    public float waveInterval = 5f; // Time between waves (after all enemies are destroyed)
    public List<Wave> waves; // List of waves with customizable enemy counts

    [Header("UI Settings")]
    public TextMeshProUGUI waveInfoText; // Reference to the TextMeshPro UI element
    public string waveInfoFormat = "Wave: {0}/{1}\nEnemies Left: {2}"; // Editable format for wave info
    public string allWavesCompletedText = "All waves completed!"; // Editable text for when all waves are done

    [Header("Events")]
    public UnityEvent onWaveCleared; // UnityEvent triggered when a normal wave is cleared
    public UnityEvent onFinalWaveCleared; // UnityEvent triggered when the final wave is cleared

    private int currentWaveIndex = 0; // Current wave index
    private List<GameObject> activeEnemies = new List<GameObject>(); // Track active enemies
    private bool isWaveActive = false; // Track if a wave is currently active

    [System.Serializable]
    public class Wave
    {
        public int enemyCount; // Number of enemies in this wave
    }

    private void Start()
    {
        if (enemyTemplates != null && enemyTemplates.Count > 0)
        {
            foreach (var enemy in enemyTemplates)
            {
                enemy.SetActive(false); // Disable all template enemies initially
            }
        }
        else
        {
            Debug.LogError("Enemy templates are not assigned.");
        }

        // Deactivate the TextMeshPro UI at the start
        if (waveInfoText != null)
        {
            waveInfoText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Wave info TextMeshPro UI is not assigned.");
        }
    }

    // Public method to start the wave spawning process
    public void StartWave()
    {
        if (!isWaveActive && currentWaveIndex < waves.Count)
        {
            StartCoroutine(WaveManager());
        }
        else if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("All waves completed. No more waves to start.");
        }
    }

    // Public method to reset the wave system to the first wave
    public void ResetWaves()
    {
        StopAllCoroutines(); // Stop any ongoing coroutines
        currentWaveIndex = 0; // Reset to the first wave
        activeEnemies.Clear(); // Clear the list of active enemies
        isWaveActive = false; // Reset the wave state

        // Destroy all remaining enemies in the scene
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }

        Debug.Log("Waves have been reset to the first wave.");
    }

    private IEnumerator WaveManager()
    {
        isWaveActive = true;

        while (currentWaveIndex < waves.Count)
        {
            if (waveInfoText != null)
            {
                waveInfoText.gameObject.SetActive(true);
            }

            Wave currentWave = waves[currentWaveIndex];
            UpdateWaveInfoUI();
            yield return StartCoroutine(SpawnWave(currentWave.enemyCount));
            yield return new WaitUntil(() => AreAllEnemiesDestroyed());

            if (currentWaveIndex < waves.Count - 1)
            {
                onWaveCleared.Invoke();
            }
            else
            {
                onFinalWaveCleared.Invoke();
            }
            UpdateWaveInfoUI();
            yield return new WaitForSeconds(waveInterval);
            currentWaveIndex++;
        }

        // All waves completed
        UpdateWaveInfoUI(allWavesCompletedText);
        yield return new WaitForSeconds(2f);
        if (waveInfoText != null)
        {
            waveInfoText.gameObject.SetActive(false);
        }

        isWaveActive = false;
    }

    private IEnumerator SpawnWave(int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            if (enemyTemplates.Count > 0 && spawnPoints.Count > 0 && paths.Count > 0)
            {
                GameObject enemyTemplate = enemyTemplates[Random.Range(0, enemyTemplates.Count)];
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
                GameObject enemy = Instantiate(enemyTemplate, spawnPoint.position, spawnPoint.rotation);
                enemy.SetActive(true);
                activeEnemies.Add(enemy); // Add to the active enemies list
                int randomIndex = Random.Range(0, paths.Count);
                EnemyPath assignedPath = paths[randomIndex];
                EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.pathObject = assignedPath.gameObject;
                }
                UpdateWaveInfoUI();
            }
            yield return new WaitForSeconds(spawnInterval); // Wait before spawning the next enemy
        }
    }

    private bool AreAllEnemiesDestroyed()
    {
        // Remove null (destroyed) enemies from the list
        activeEnemies.RemoveAll(enemy => enemy == null);

        // Update the UI after an enemy is destroyed
        UpdateWaveInfoUI();

        // Return true if there are no active enemies left
        return activeEnemies.Count == 0;
    }

    private void UpdateWaveInfoUI(string customMessage = null)
    {
        if (waveInfoText != null)
        {
            if (customMessage != null)
            {
                // Display a custom message (e.g., "All waves completed!")
                waveInfoText.text = customMessage;
            }
            else
            {
                // Display the current wave and enemies left using the editable format
                waveInfoText.text = string.Format(waveInfoFormat, currentWaveIndex + 1, waves.Count, activeEnemies.Count);
            }
        }
        else
        {
            Debug.LogWarning("Wave info TextMeshPro UI is not assigned.");
        }
    }
}