using UnityEngine;
using System.Collections.Generic;

public class RespawnManager : MonoBehaviour
{
    public string targetTag = "Generator"; // Tag to identify the target (e.g., generator)
    public float maxDistance = 5f; // Max allowed distance from the target
    public string objectTag = "Battery"; // Tag to identify objects to manage

    private Transform target; // The target (e.g., generator)
    private Dictionary<GameObject, Vector3> trackedObjects; // Dictionary to store objects and their original positions

    private void Start()
    {
        // Find the target GameObject by tag
        GameObject targetObject = GameObject.FindWithTag(targetTag);
        if (targetObject != null)
        {
            target = targetObject.transform;
        }
        else
        {
            Debug.LogError("No GameObject found with tag: " + targetTag);
        }

        // Initialize the dictionary to store tracked objects and their original positions
        trackedObjects = new Dictionary<GameObject, Vector3>();

        // Find all initial objects with the specified tag and add them to the dictionary
        GameObject[] initialObjects = GameObject.FindGameObjectsWithTag(objectTag);
        foreach (GameObject obj in initialObjects)
        {
            trackedObjects[obj] = obj.transform.position;
        }
    }

    private void Update()
    {
        // If the target is not found, stop here
        if (target == null) return;

        // Check each tracked object's distance from the target
        foreach (var obj in new List<GameObject>(trackedObjects.Keys))
        {
            if (obj != null)
            {
                float distance = Vector3.Distance(obj.transform.position, target.position);

                // If the object is too far, respawn it
                if (distance > maxDistance)
                {
                    Respawn(obj);
                }
            }
            else
            {
                // If the object is destroyed, remove it from the dictionary
                trackedObjects.Remove(obj);
            }
        }
    }

    // Add a new object to the tracking system
    public void AddObject(GameObject newObject)
    {
        if (!trackedObjects.ContainsKey(newObject))
        {
            trackedObjects[newObject] = newObject.transform.position;
        }
    }

    // Respawn a specific object
    private void Respawn(GameObject obj)
    {
        if (trackedObjects.ContainsKey(obj))
        {
            // Reset the object to its original position
            obj.transform.position = trackedObjects[obj];

            // Optional: Add feedback (e.g., sound, particle effect, or debug log)
            Debug.Log(obj.name + " respawned at original position.");
        }
    }

    // Draw Gizmos in the Editor
    private void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // Set the Gizmo color
            Gizmos.color = Color.red;

            // Draw a wireframe sphere around the target to represent the max distance
            Gizmos.DrawWireSphere(target.position, maxDistance);
        }
    }
}