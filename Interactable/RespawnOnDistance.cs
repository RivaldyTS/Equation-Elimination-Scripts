using UnityEngine;

public class RespawnOnDistance : MonoBehaviour
{
    public Transform target; // The target (e.g., generator) to check distance from
    public float maxDistance = 5f; // Max allowed distance from the target
    private Vector3 originalPosition; // Original position to respawn at

    private void Start()
    {
        // Store the original position of the object
        originalPosition = transform.position;
    }

    private void Update()
    {
        // Check the distance between this object and the target
        if (Vector3.Distance(transform.position, target.position) > maxDistance)
        {
            Respawn();
        }
    }

    // Public method to respawn the object
    public void Respawn()
    {
        // Reset the object to its original position
        transform.position = originalPosition;

        // Optional: Add feedback (e.g., sound, particle effect, or debug log)
        Debug.Log(gameObject.name + " respawned at original position.");
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