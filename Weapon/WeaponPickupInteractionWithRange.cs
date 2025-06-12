using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class WeaponGet : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private string weaponName; // Name of the weapon to pick up (e.g., "Pistol", "Rifle")
    [SerializeField] private List<string> targetTags = new List<string> { "Player" }; // List of tags that can activate the pickup
    [SerializeField] private float pickupRadius = 2f; // Radius for picking up the weapon

    [Header("Gizmo Settings")]
    [SerializeField] private bool useGizmoTrigger = false; // Enable Gizmo-based event triggering
    [SerializeField] private Transform playerTransform; // Assign the player's transform for Gizmo-based triggering

    [Header("Events")]
    public UnityEvent onPlayerEnter; // Event triggered when the player enters the pickup radius
    public UnityEvent onPlayerExit; // Event triggered when the player exits the pickup radius
    public UnityEvent onPickup; // Event triggered when the weapon is picked up

    private bool isPlayerInRange = false; // Track if the player is in the pickup radius

    void Update()
    {
        // Check if the player is in the pickup radius (Gizmo-based triggering)
        if (useGizmoTrigger && playerTransform != null)
        {
            CheckPlayerInRange();
        }

        // Allow the player to pick up the weapon manually (E key)
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            PickupWeapon(playerTransform.gameObject);
        }
    }

    private void CheckPlayerInRange()
    {
        // Calculate the distance between the player and the weapon
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // Check if the player is within the pickup radius
        if (distance <= pickupRadius)
        {
            if (!isPlayerInRange)
            {
                // Player just entered the radius
                isPlayerInRange = true;
                onPlayerEnter.Invoke();
            }
        }
        else
        {
            if (isPlayerInRange)
            {
                // Player just exited the radius
                isPlayerInRange = false;
                onPlayerExit.Invoke();
            }
        }
    }

    private void PickupWeapon(GameObject player)
    {
        // Find the WeaponInventory component in the WeaponHolder
        WeaponInventory weaponInventory = player.transform.Find("Main Camera/WeaponHolder").GetComponent<WeaponInventory>();
        if (weaponInventory != null)
        {
            // Add the weapon to the player's inventory
            weaponInventory.AddWeapon(weaponName);

            // Trigger the pickup event
            onPickup.Invoke();

            // Destroy the weapon in the scene
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("WeaponInventory component not found in WeaponHolder.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the pickup radius in the editor (if Gizmo-based triggering is enabled)
        if (useGizmoTrigger)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}