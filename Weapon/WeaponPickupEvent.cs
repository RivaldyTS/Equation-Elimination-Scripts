using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class WeaponPickupEvent : MonoBehaviour
{
    [Header("Weapon Settings")]
    public string weaponName; // Name of the weapon to pick up

    [Header("Pickup Settings")]
    [SerializeField] private float pickupRadius = 1.5f; // Radius for picking up the weapon
    [SerializeField, TagSelector] private string playerTag = "Player"; // Tag for the player
    [SerializeField] private bool useTrigger = true; // Use trigger for pickup (optional)
    [SerializeField] private bool autoPickup = false; // Optional: Automatically pick up the weapon when in range

    [Header("Events")]
    public UnityEvent OnPickup; // UnityEvent that triggers when the weapon is picked up

    private WeaponSwitching weaponSwitching; // Reference to the WeaponSwitching script
    private GameObject pickupPrompt; // UI prompt to show when the player is near the weapon
    private bool isPlayerNear = false; // Track if the player is near the weapon

    void Start()
    {
        InitializeReferences();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeReferences();
    }

    void Update()
    {
        if (useTrigger)
        {
            CheckPlayerProximity();

            // Allow the player to pick up the weapon manually (E key)
            if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
            {
                TryPickupWeapon();
            }

            // Optional: Automatically pick up the weapon when in range
            if (autoPickup && isPlayerNear)
            {
                TryPickupWeapon();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!useTrigger && collision.gameObject.CompareTag(playerTag))
        {
            // Pick up the weapon on collision (non-trigger)
            TryPickupWeapon();
        }
    }

    /// <summary>
    /// Initializes references to the player's WeaponSwitching script and the pickup prompt UI.
    /// </summary>
    private void InitializeReferences()
    {
        // Find the player object using the playerTag
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            // Get the WeaponSwitching component from the player
            weaponSwitching = player.GetComponent<WeaponSwitching>();
            if (weaponSwitching == null)
            {
                Debug.LogWarning($"No WeaponSwitching script found on the player object with tag: {playerTag}");
            }
        }
        else
        {
            Debug.LogWarning($"No GameObject found with tag: {playerTag}");
        }

        // Find the pickup prompt UI (if needed)
        pickupPrompt = GameObject.FindGameObjectWithTag("PickupPrompt");
        if (pickupPrompt == null)
        {
            Debug.LogWarning($"No pickup prompt found with tag: PickupPrompt");
        }
    }

    /// <summary>
    /// Checks if the player is within the pickup radius.
    /// </summary>
    private void CheckPlayerProximity()
    {
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
    }

    /// <summary>
    /// Attempts to pick up the weapon. Can be called externally.
    /// </summary>
    public void TryPickupWeapon()
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
            Debug.LogWarning("Cannot pick up weapon: WeaponSwitching reference is not set.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the pickup radius in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}