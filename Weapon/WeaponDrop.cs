using UnityEngine;
using UnityEngine.Events; // Required for UnityEvent
using System.Collections.Generic;

public class WeaponDrop : MonoBehaviour
{
    public WeaponSwitching weaponSwitching; // Reference to the WeaponSwitching script

    [Header("Weapon Drop Settings")]
    [SerializeField] private KeyCode dropKey = KeyCode.G; // Configurable drop key
    [SerializeField] private float throwForce = 10f; // Force applied to the thrown weapon
    [SerializeField] private float spawnDistance = 1.5f; // Distance in front of the camera to spawn the weapon

    [Header("Weapon Prefab Mapping")]
    [SerializeField] private WeaponPrefabMapping[] weaponPrefabMappings; // Map weapon names to prefabs

    [Header("Camera Reference")]
    [SerializeField] private Camera playerCamera; // Configurable camera reference

    private Dictionary<string, WeaponPrefabMapping> weaponPrefabDictionary;

    [System.Serializable]
    public class WeaponPrefabMapping
    {
        public string weaponName; // Name of the weapon
        public GameObject weaponPrefab; // Prefab to spawn for this weapon
        public UnityEvent onWeaponDropped; // UnityEvent to trigger when this weapon is dropped
    }

    private void Start()
    {
        // Initialize the dictionary for faster lookups
        weaponPrefabDictionary = new Dictionary<string, WeaponPrefabMapping>();
        foreach (var mapping in weaponPrefabMappings)
        {
            if (!weaponPrefabDictionary.ContainsKey(mapping.weaponName))
            {
                weaponPrefabDictionary.Add(mapping.weaponName, mapping);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(dropKey)) // Use the configured drop key
        {
            DropCurrentWeapon();
        }
    }

    private void DropCurrentWeapon()
    {
        // Check if weaponSwitching is assigned
        if (weaponSwitching == null)
        {
            Debug.LogError("WeaponSwitching reference is not set.");
            return;
        }

        // Check if playerCamera is assigned
        if (playerCamera == null)
        {
            Debug.LogError("Player camera reference is not set.");
            return;
        }

        // Get the currently equipped weapon
        Transform currentWeapon = weaponSwitching.GetCurrentWeaponTransform();
        if (currentWeapon == null)
        {
            Debug.LogWarning("No weapon is currently equipped.");
            return;
        }

        // Get the name of the currently equipped weapon
        string weaponName = currentWeapon.name;
        Debug.Log($"Dropping weapon: {weaponName}");

        // Find the corresponding mapping for the weapon
        if (!weaponPrefabDictionary.TryGetValue(weaponName, out WeaponPrefabMapping mapping))
        {
            Debug.LogWarning($"No prefab found for weapon: {weaponName}");
            return;
        }

        // Remove the weapon from the player's inventory
        weaponSwitching.DropWeapon(weaponName);

        // Spawn the weapon prefab in front of the player's camera
        Vector3 spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * spawnDistance;
        Quaternion spawnRotation = playerCamera.transform.rotation;

        GameObject thrownWeapon = Instantiate(mapping.weaponPrefab, spawnPosition, spawnRotation);

        // Add force to the thrown weapon
        if (thrownWeapon.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero; // Reset velocity
            rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Weapon prefab does not have a Rigidbody component.");
        }

        // Trigger the UnityEvent for this weapon
        mapping.onWeaponDropped.Invoke();
    }
}