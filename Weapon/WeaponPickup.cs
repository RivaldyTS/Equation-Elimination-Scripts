using UnityEngine;
using UnityEngine.Events; // Required for UnityEvent

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Settings")]
    public string weaponName; // Name of the weapon to pick up

    [Header("Pickup Settings")]
    [SerializeField] private float pickupRadius = 1.5f; // Radius for picking up the weapon
    [SerializeField] private string playerTag = "Player"; // Tag for the player
    [SerializeField] private bool useTrigger = true; // Use trigger for pickup (optional)
    [SerializeField] private bool autoPickup = false; // Optional: Automatically pick up the weapon when in range

    [Header("Auto-Assign Settings")]
    [SerializeField] private string weaponSwitchingTag = "Player"; // Tag to find the WeaponSwitching script
    [SerializeField] private string pickupPromptTag = "PickupPrompt"; // Tag to find the pickup prompt UI

    [Header("Events")]
    public UnityEvent OnPickup; // UnityEvent that triggers when the weapon is picked up

    private WeaponSwitching weaponSwitching; // Reference to the WeaponSwitching script
    private GameObject pickupPrompt; // UI prompt to show when the player is near the weapon
    private bool isPlayerNear = false; // Track if the player is near the weapon

    void Start()
    {
        // Automatically assign the weaponSwitching reference
        GameObject player = GameObject.FindGameObjectWithTag(weaponSwitchingTag);
        if (player != null)
        {
            weaponSwitching = player.GetComponent<WeaponSwitching>();
            if (weaponSwitching == null)
            {
                Debug.LogWarning($"No WeaponSwitching script found on object with tag: {weaponSwitchingTag}");
            }
        }
        else
        {
            Debug.LogWarning($"No GameObject found with tag: {weaponSwitchingTag}");
        }

        // Automatically assign the pickupPrompt reference
        pickupPrompt = GameObject.FindGameObjectWithTag(pickupPromptTag);
        if (pickupPrompt == null)
        {
            Debug.LogWarning($"No pickup prompt found with tag: {pickupPromptTag}");
        }
    }

    void Update()
    {
        if (useTrigger)
        {
            // Check if the player is near the weapon (trigger-based)
            Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRadius);
            isPlayerNear = false;

            foreach (var collider in colliders)
            {
                if (collider.CompareTag(playerTag))
                {
                    isPlayerNear = true;
                    break;
                }
            }

            // Show or hide the pickup prompt
            if (pickupPrompt != null)
            {
                pickupPrompt.SetActive(isPlayerNear);
            }

            // Allow the player to pick up the weapon manually (E key)
            if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
            {
                PickupWeapon();
            }

            // Optional: Automatically pick up the weapon when in range
            if (autoPickup && isPlayerNear)
            {
                PickupWeapon();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!useTrigger && collision.gameObject.CompareTag(playerTag))
        {
            // Pick up the weapon on collision (non-trigger)
            PickupWeapon();
        }
    }

    private void PickupWeapon()
    {
        if (weaponSwitching != null)
        {
            // Add the weapon to the player's inventory
            weaponSwitching.PickupWeapon(weaponName);

            // Trigger the OnPickup event
            OnPickup.Invoke();

            // Destroy the dropped weapon object
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("WeaponSwitching reference is not set.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the pickup radius in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}