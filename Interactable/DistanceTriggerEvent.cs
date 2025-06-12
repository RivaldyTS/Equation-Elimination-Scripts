using UnityEngine;
using UnityEngine.Events;

public class DistanceTriggerEvent : MonoBehaviour
{
    [Header("Settings")]
    public Transform player; // Assign the player's Transform here
    public float triggerDistance = 5f; // The distance at which the event triggers
    public UnityEvent onPlayerApproach; // UnityEvent to trigger when the player is close

    private bool hasTriggered = false; // To ensure the event only triggers once

    void Update()
    {
        if (player == null)
        {
            return;
        }

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= triggerDistance && !hasTriggered)
        {
            onPlayerApproach.Invoke();
            hasTriggered = true;
        }
    }

    // Optional: Reset the trigger when needed
    public void ResetTrigger()
    {
        hasTriggered = false;
        Debug.Log("Trigger has been reset for " + gameObject.name);
    }

    // Visualize the trigger radius in the editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}