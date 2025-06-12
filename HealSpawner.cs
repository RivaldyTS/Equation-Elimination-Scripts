using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealSpawner : MonoBehaviour
{
    public GameObject healItem; // The heal item game object already placed in the scene
    public Transform spawnPoint; // Where to spawn the heal items
    public float spawnInterval = 10f; // Time between spawns
    public int maxHealItems = 10; // Maximum number of heal items in the scene
    public int minHealItems = 5;  // Minimum number of heal items before resuming spawn

    private bool canSpawn = true;

    private void Start()
    {
        if (healItem != null)
        {
            healItem.SetActive(false); // Ensure the original item is inactive
            Debug.Log("Starting HealSpawner Coroutine");
            StartCoroutine(SpawnHealItem());
        }
        else
        {
            Debug.LogError("Heal item is not assigned.");
        }
    }

    private IEnumerator SpawnHealItem()
    {
        while (true)
        {
            // Count the number of active heal items
            int activeHealItemCount = GameObject.FindGameObjectsWithTag("HealItem").Length;
            Debug.Log("Active heal item count: " + activeHealItemCount);

            if (canSpawn)
            {
                if (activeHealItemCount < maxHealItems)
                {
                    // Instantiate the heal item
                    GameObject healItemInstance = Instantiate(healItem, spawnPoint.position, spawnPoint.rotation);
                    healItemInstance.SetActive(true);
                    Debug.Log("Heal item spawned at " + spawnPoint.position);

                    // Pause spawn if the active heal item count reaches maxHealItems
                    if (activeHealItemCount >= maxHealItems)
                    {
                        canSpawn = false;
                        Debug.Log("Pausing spawn, active heal item count: " + activeHealItemCount);
                    }
                }
            }
            else
            {
                // Check again if we need to resume spawning
                if (activeHealItemCount < minHealItems)
                {
                    canSpawn = true;
                    Debug.Log("Resuming spawn, active heal item count: " + activeHealItemCount);
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
