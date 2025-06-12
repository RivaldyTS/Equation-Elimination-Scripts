using UnityEngine;
using UnityEngine.Events; // Required for UnityEvent
using System.Collections.Generic; // Required for List<>

public class WeaponPickupInteraction : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private string weaponName; // Name of the weapon to pick up (e.g., "Pistol", "Rifle")
    [SerializeField] private List<string> targetTags = new List<string> { "Player" }; // List of tags that can activate the pickup
    [SerializeField] private bool useTrigger = true; // Use trigger for detection
    [SerializeField] private bool autoPickup = false; // Automatically pick up the weapon when in range

    [Header("Events")]
    public UnityEvent onPickup; // Event triggered when the weapon is picked up

    private bool isPlayerNear = false; // Track if the player is near the weapon

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object has one of the allowed tags
        if (useTrigger && targetTags.Contains(other.tag))
        {
            isPlayerNear = true;
            if (autoPickup)
            {
                PickupWeapon(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the collided object has one of the allowed tags
        if (useTrigger && targetTags.Contains(other.tag))
        {
            isPlayerNear = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if collision should be used and if the collided object has one of the allowed tags
        if (!useTrigger && targetTags.Contains(collision.gameObject.tag))
        {
            isPlayerNear = true;
            if (autoPickup)
            {
                PickupWeapon(collision.gameObject);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Check if the collided object has one of the allowed tags
        if (!useTrigger && targetTags.Contains(collision.gameObject.tag))
        {
            isPlayerNear = false;
        }
    }

    void Update()
    {
        // Allow the player to pick up the weapon manually (E key)
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            PickupWeapon(GameObject.FindGameObjectWithTag("Player"));
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
}