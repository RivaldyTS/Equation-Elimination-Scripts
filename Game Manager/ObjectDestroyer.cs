using UnityEngine;

public class ObjectDestroyer : MonoBehaviour
{
    [Header("Destruction Settings")]
    [SerializeField] private bool useTag = true;           // Toggle between tag or layer in Inspector
    [SerializeField] private string targetTag = "Enemy";   // Tag to target, editable in Inspector
    [SerializeField] [Range(0, 31)] private int targetLayer = 0; // Layer to target, editable in Inspector
    [SerializeField] private bool useBothTagAndLayer = false; // Option to require both tag AND layer match

    // Single public method for external scripts to call
    public void DestroyTargetedObjects()
    {
        if (useBothTagAndLayer)
        {
            DestroyByTagAndLayer();
        }
        else if (useTag)
        {
            DestroyByTag();
        }
        else
        {
            DestroyByLayer();
        }
    }

    private void DestroyByTag()
    {
        GameObject[] objectsToDestroy = GameObject.FindGameObjectsWithTag(targetTag);
        foreach (GameObject obj in objectsToDestroy)
        {
            Destroy(obj);
        }
    }

    private void DestroyByLayer()
    {
        if (targetLayer < 0 || targetLayer > 31)
        {
            Debug.LogError("Invalid layer number");
            return;
        }

        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int destroyedCount = 0;
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == targetLayer)
            {
                Destroy(obj);
                destroyedCount++;
            }
        }
    }

    private void DestroyByTagAndLayer()
    {
        if (targetLayer < 0 || targetLayer > 31)
        {
            Debug.LogError("Invalid layer number.");
            return;
        }

        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(targetTag);
        int destroyedCount = 0;

        foreach (GameObject obj in taggedObjects)
        {
            if (obj.layer == targetLayer)
            {
                Destroy(obj);
                destroyedCount++;
            }
        }
        
        Debug.Log($"Destroyed {destroyedCount} objects with tag: {targetTag} on layer: {LayerMask.LayerToName(targetLayer)}");
    }
}