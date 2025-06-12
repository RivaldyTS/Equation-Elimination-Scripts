using UnityEngine;

public class WeaponPickupInteractable : Interactable
{
    [Header("Weapon Settings")]
    public string weaponName; // Name of the weapon to pick up
    public WeaponSwitching weaponSwitching; // Reference to the WeaponSwitching script

    [Header("Pickup Settings")]
    public bool useTrigger = true; // Whether to use trigger-based pickup
    public GameObject weaponPrefab; // Prefab to spawn when dropping the weapon (optional)

    private void OnTriggerEnter(Collider other)
    {
        if (useTrigger && other.CompareTag("Player")) // Trigger-based pickup
        {
            PickupWeapon();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!useTrigger && collision.gameObject.CompareTag("Player")) // Collision-based pickup
        {
            PickupWeapon();
        }
    }

    protected override void Interact() // Match the access modifier (protected)
    {
        PickupWeapon(); // Interaction-based pickup
    }

    private void PickupWeapon()
    {
        if (weaponSwitching != null)
        {
            weaponSwitching.PickupWeapon(weaponName); // Add the weapon to the player's inventory
            onPickup.Invoke(); // Trigger the pickup event

            // Optionally destroy the pickup object
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("WeaponSwitching reference is not set.");
        }
    }
}