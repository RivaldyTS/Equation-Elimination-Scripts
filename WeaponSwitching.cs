using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class WeaponSwitching : MonoBehaviour
{
    public int selectedWeapon = -1; // -1 means no weapon is selected

    [Header("Weapon Switching Settings")]
    [SerializeField] private float switchCooldown = 0.5f; // Cooldown between weapon switches
    private float lastSwitchTime; // Track the last time a weapon was switched

    private List<int> obtainedWeapons = new List<int>(); // Track which weapons have been obtained

    [System.Serializable]
    public class WeaponEvent
    {
        public string weaponName; // Name of the weapon
        public UnityEvent onSwitchToWeapon; // Event triggered when switching to this weapon
    }

    [Header("No Weapon Event")]
    public UnityEvent onNoWeapon; // Event triggered when no weapons are left

    [Header("Weapon Events")]
    [SerializeField] private List<WeaponEvent> weaponEvents = new List<WeaponEvent>(); // List of weapon-specific events

    void Start()
    {
        // Ensure all weapons are disabled at the start
        foreach (Transform weapon in transform)
        {
            weapon.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Check if the cooldown is active
        if (Time.time < lastSwitchTime + switchCooldown)
        {
            return;
        }

        int previousSelectedWeapon = selectedWeapon;

        // Mouse Scroll Up
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            SelectNextWeapon();
        }
        // Mouse Scroll Down
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            SelectPreviousWeapon();
        }

        // Number Key Selection (Only switches if the weapon is obtained)
        for (int i = 0; i < transform.childCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && obtainedWeapons.Contains(i))
            {
                selectedWeapon = i;
            }
        }

        // If weapon selection changed, update active weapon
        if (previousSelectedWeapon != selectedWeapon)
        {
            SelectWeapon();
            lastSwitchTime = Time.time; // Start the cooldown
        }
    }

    private void SelectNextWeapon()
    {
        if (obtainedWeapons.Count == 0) return; // No weapons obtained

        int startIndex = obtainedWeapons.IndexOf(selectedWeapon);
        int nextIndex = (startIndex + 1) % obtainedWeapons.Count;

        selectedWeapon = obtainedWeapons[nextIndex];
    }

    private void SelectPreviousWeapon()
    {
        if (obtainedWeapons.Count == 0) return; // No weapons obtained

        int startIndex = obtainedWeapons.IndexOf(selectedWeapon);
        int previousIndex = (startIndex - 1 + obtainedWeapons.Count) % obtainedWeapons.Count;

        selectedWeapon = obtainedWeapons[previousIndex];
    }

    void SelectWeapon()
    {
        // Deactivate all weapons
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        // Activate the selected weapon (if a weapon is selected)
        if (selectedWeapon != -1)
        {
            Transform selectedWeaponTransform = transform.GetChild(selectedWeapon);
            selectedWeaponTransform.gameObject.SetActive(true);

            // Trigger the weapon-specific event
            TriggerWeaponEvent(selectedWeaponTransform.name);
        }

        Debug.Log("Selected Weapon: " + (selectedWeapon == -1 ? "None" : transform.GetChild(selectedWeapon).name));
    }

    private void TriggerWeaponEvent(string weaponName)
    {
        // Find the weapon event matching the weapon name
        foreach (var weaponEvent in weaponEvents)
        {
            if (weaponEvent.weaponName == weaponName)
            {
                weaponEvent.onSwitchToWeapon.Invoke(); // Trigger the event
                break;
            }
        }
    }

    // Call this when the player picks up a weapon
    public void PickupWeapon(string weaponName)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name == weaponName)
            {
                if (!obtainedWeapons.Contains(i))
                {
                    obtainedWeapons.Add(i); // Add the weapon to the obtained list
                }

                selectedWeapon = i; // Set the weapon as the active one
                SelectWeapon();
                break;
            }
        }
    }

    public Transform GetCurrentWeaponTransform()
{
    if (selectedWeapon != -1)
    {
        return transform.GetChild(selectedWeapon);
    }
    return null; // No weapon is equipped
}

     // Modify the DropWeapon method to trigger the event
    public void DropWeapon(string weaponName)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name == weaponName)
            {
                obtainedWeapons.Remove(i); // Remove the weapon from the obtained list

                // If the dropped weapon was selected, switch to the next available weapon
                if (selectedWeapon == i)
                {
                    if (obtainedWeapons.Count > 0)
                    {
                        selectedWeapon = obtainedWeapons[0]; // Switch to the first obtained weapon
                    }
                    else
                    {
                        selectedWeapon = -1; // No weapons left
                        onNoWeapon.Invoke(); // Trigger the no weapon event
                    }
                    SelectWeapon();
                }
                break;
            }
        }
    }
}