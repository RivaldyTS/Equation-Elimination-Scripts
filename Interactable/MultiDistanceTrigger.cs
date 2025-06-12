using UnityEngine;
using UnityEngine.Events;

public class MultiDistanceTrigger : MonoBehaviour
{
    [Header("Settings")]
    public Transform player; // Assign the player's Transform here
    public float triggerDistance = 5f; // The distance at which events trigger
    
    [Header("Events")]
    public UnityEvent onFirstApproach; // Triggers on first entry
    public UnityEvent onSubsequentApproach; // Triggers on re-entry after leaving

    [Header("Debug")]
    public Color rangeColor = Color.green; // Color of the trigger sphere in editor

    private bool hasFirstTriggered = false; // Tracks first entry
    private bool isPlayerInside = false; // Tracks current player presence

    void Update()
    {
        // Calculate the distance between the player and this object
        float distance = Vector3.Distance(player.position, transform.position);
        bool isCurrentlyInRange = distance <= triggerDistance;

        // Player enters the trigger zone
        if (isCurrentlyInRange && !isPlayerInside)
        {
            if (!hasFirstTriggered)
            {
                // First time entering
                onFirstApproach.Invoke();
                hasFirstTriggered = true;
            }
            else
            {
                // Subsequent entries after leaving
                onSubsequentApproach.Invoke();
            }
            isPlayerInside = true;
        }
        // Player leaves the trigger zone
        else if (!isCurrentlyInRange && isPlayerInside)
        {
            isPlayerInside = false;
        }
    }

    // Draws the trigger range in the editor
    void OnDrawGizmos()
    {
        Gizmos.color = rangeColor;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}