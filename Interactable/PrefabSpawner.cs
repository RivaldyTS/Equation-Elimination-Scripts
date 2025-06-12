using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn; // The prefab to spawn
    public Transform spawnLocation; // The location where the prefab will spawn
    public GameObject spawnParticleEffect; // The particle effect to play when spawning
    public AudioSource spawnAudioSource; // Optional AudioSource for spawn sound
    public float minSpawnTime = 2f; // Minimum time between spawns
    public float maxSpawnTime = 5f; // Maximum time between spawns

    private GameObject spawnedObject; // Reference to the spawned object
    private float timer; // Timer to track spawn intervals

    void Start()
    {
        // Initialize the timer with a random value between min and max
        timer = Random.Range(minSpawnTime, maxSpawnTime);
    }

    void Update()
    {
        if (spawnedObject == null)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                SpawnObject();
                timer = Random.Range(minSpawnTime, maxSpawnTime);
            }
        }
    }

    void SpawnObject()
    {
        spawnedObject = Instantiate(prefabToSpawn, spawnLocation.position, spawnLocation.rotation);
        if (spawnParticleEffect != null)
        {
            GameObject particleInstance =
            Instantiate(spawnParticleEffect, spawnLocation.position, spawnLocation.rotation);
            Destroy(particleInstance, 2f);
        }
        if (spawnAudioSource != null)
        {
            spawnAudioSource.Play();
        }
    }
}