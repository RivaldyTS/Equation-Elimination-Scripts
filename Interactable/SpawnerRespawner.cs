using UnityEngine;
using System.Collections;

public class PuzzleRespawner : MonoBehaviour
{
    [Header("Puzzle Respawner Settings")]
    [SerializeField] private GameObject[] puzzleObjects; // Array of GameObjects to respawn
    [SerializeField] private float respawnDelay = 0.5f; // Delay before respawning

    private Vector3[] originalPositions; // Store original positions
    private Quaternion[] originalRotations; // Store original rotations
    private GameObject[] objectPrefabs; // Store original prefabs

    void Start()
    {
        // Initialize arrays to store original data
        originalPositions = new Vector3[puzzleObjects.Length];
        originalRotations = new Quaternion[puzzleObjects.Length];
        objectPrefabs = new GameObject[puzzleObjects.Length];

        // Save the original positions, rotations, and prefabs
        for (int i = 0; i < puzzleObjects.Length; i++)
        {
            if (puzzleObjects[i] != null)
            {
                originalPositions[i] = puzzleObjects[i].transform.position;
                originalRotations[i] = puzzleObjects[i].transform.rotation;
                objectPrefabs[i] = puzzleObjects[i]; // Store the prefab or reference
            }
        }
    }

    // Public method to trigger the respawn process
    public void RespawnPuzzleObjects()
    {
        StartCoroutine(RespawnObjectsWithDelay());
    }

    // Coroutine to handle the respawn process with a delay
    private IEnumerator RespawnObjectsWithDelay()
    {
        // Destroy all selected puzzle objects
        for (int i = 0; i < puzzleObjects.Length; i++)
        {
            if (puzzleObjects[i] != null)
            {
                Destroy(puzzleObjects[i]); // Destroy the existing object
            }
        }

        // Wait for the specified delay
        yield return new WaitForSeconds(respawnDelay);

        // Respawn all puzzle objects at their original positions and rotations
        for (int i = 0; i < puzzleObjects.Length; i++)
        {
            if (objectPrefabs[i] != null)
            {
                puzzleObjects[i] = Instantiate(objectPrefabs[i], originalPositions[i], originalRotations[i]); // Respawn the object
            }
        }

        Debug.Log("Puzzle objects destroyed and respawned!");
    }
}