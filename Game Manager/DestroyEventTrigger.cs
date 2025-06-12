using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class MultiDestroyEventTrigger : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> targetObjects = new List<GameObject>(); // List of GameObjects to monitor
    
    [SerializeField]
    private UnityEvent onAllDestroyedEvent; // Event to trigger when all objects are destroyed
    
    private bool hasTriggered = false; // Prevent multiple triggers

    void Update()
    {
        if (!hasTriggered && AreAllDestroyed())
        {
            TriggerEvent();
        }
    }

    // Check if all target objects are destroyed
    private bool AreAllDestroyed()
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null) // If any object still exists, return false
            {
                return false;
            }
        }
        return true; // All objects are null (destroyed)
    }

    // Method to add a target object programmatically
    public void AddTarget(GameObject target)
    {
        if (!targetObjects.Contains(target))
        {
            targetObjects.Add(target);
        }
    }

    // Method to remove a target object programmatically
    public void RemoveTarget(GameObject target)
    {
        targetObjects.Remove(target);
    }

    private void TriggerEvent()
    {
        if (!hasTriggered)
        {
            onAllDestroyedEvent?.Invoke(); // Trigger the UnityEvent
            hasTriggered = true; // Ensure it only triggers once
        }
    }

    // Optional: Reset the trigger state
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}