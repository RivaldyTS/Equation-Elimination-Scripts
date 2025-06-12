using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;

public class WeaponInventory : MonoBehaviour
{
    [System.Serializable]
    public class Weapon
    {
        public string weaponName;
        public GameObject weaponObject;
        public GameObject weaponImageUI;
        public GameObject ammoImageUI;
        public TextMeshProUGUI inventorySlotText;
        public UnityEvent onEquip;
        public GameObject weaponPrefab;
        public AudioClip pickupSound; // Sound to play when weapon is picked up
        public AudioClip switchSound; // Sound to play when weapon is switched
    }

    [Header("Weapon Settings")]
    [SerializeField] private List<Weapon> weapons = new List<Weapon>();
    [SerializeField] private int maxWeapons = 5;

    [Header("Drop Settings")]
    [SerializeField] private float throwForce = 5f;
    [SerializeField] private Transform throwPosition;

    [Header("Animation Settings")]
    [SerializeField] private string switchTriggerName = "Switch";
    [SerializeField] private string inspectTriggerName = "Inspect";
    private Animator weaponHolderAnimator;
    private bool isInspecting = false;

    [Header("Audio Settings")]
    [SerializeField] private float pickupVolume = 2f; // Volume for pickup sound
    [SerializeField] private float switchVolume = 2f; // Volume for switch sound
    private AudioSource audioSource; // Self-spawned AudioSource

    private List<Weapon> obtainedWeapons = new List<Weapon>();
    private int selectedWeaponIndex = -1;

    void Start()
    {
        weaponHolderAnimator = GetComponent<Animator>();
        if (weaponHolderAnimator == null)
        {
            Debug.LogError("Animator component not found on WeaponInventory GameObject!");
        }

        // Always spawn an AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        foreach (var weapon in weapons)
        {
            weapon.weaponObject.SetActive(false);
            if (weapon.weaponImageUI != null) weapon.weaponImageUI.SetActive(false);
            if (weapon.ammoImageUI != null) weapon.ammoImageUI.SetActive(false);
            if (weapon.inventorySlotText != null) weapon.inventorySlotText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        HandleWeaponSwitching();
        HandleWeaponInspection();

        if (Input.GetKeyDown(KeyCode.G))
        {
            DropWeapon();
        }
    }

    private void HandleWeaponSwitching()
    {
        if (obtainedWeapons.Count == 0) return;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            SelectNextWeapon();
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            SelectPreviousWeapon();
        }

        for (int i = 0; i < obtainedWeapons.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectWeapon(i);
            }
        }
    }

    private void HandleWeaponInspection()
    {
        if (obtainedWeapons.Count == 0 || selectedWeaponIndex == -1 || isInspecting) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartWeaponInspect();
        }
    }

    private void StartWeaponInspect()
    {
        if (weaponHolderAnimator != null)
        {
            isInspecting = true;
            weaponHolderAnimator.SetTrigger(inspectTriggerName);
            StartCoroutine(ResetInspectAfterDelay(weaponHolderAnimator.GetCurrentAnimatorStateInfo(0).length));
        }
    }

    private System.Collections.IEnumerator ResetInspectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isInspecting = false;
    }

    private void SelectNextWeapon()
    {
        selectedWeaponIndex = (selectedWeaponIndex + 1) % obtainedWeapons.Count;
        EquipWeapon(selectedWeaponIndex);
    }

    private void SelectPreviousWeapon()
    {
        selectedWeaponIndex = (selectedWeaponIndex - 1 + obtainedWeapons.Count) % obtainedWeapons.Count;
        EquipWeapon(selectedWeaponIndex);
    }

    private void SelectWeapon(int index)
    {
        if (index >= 0 && index < obtainedWeapons.Count)
        {
            selectedWeaponIndex = index;
            EquipWeapon(selectedWeaponIndex);
        }
    }

    private void EquipWeapon(int index)
    {
        foreach (var weapon in obtainedWeapons)
        {
            weapon.weaponObject.SetActive(false);
            if (weapon.ammoImageUI != null) weapon.ammoImageUI.SetActive(false);
        }

        if (index != -1)
        {
            Weapon selectedWeapon = obtainedWeapons[index];
            selectedWeapon.weaponObject.SetActive(true);
            if (selectedWeapon.ammoImageUI != null) selectedWeapon.ammoImageUI.SetActive(true);

            if (weaponHolderAnimator != null)
            {
                weaponHolderAnimator.SetTrigger(switchTriggerName);
            }

            selectedWeapon.onEquip.Invoke();

            // Play switch sound with custom volume
            if (selectedWeapon.switchSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(selectedWeapon.switchSound, switchVolume);
            }
        }
    }

    public void AddWeapon(string weaponName)
    {
        if (obtainedWeapons.Count >= maxWeapons)
        {
            Debug.LogWarning("Inventory is full. Cannot pick up more weapons.");
            return;
        }

        Weapon weaponToAdd = weapons.Find(weapon => weapon.weaponName == weaponName);
        if (weaponToAdd != null && !obtainedWeapons.Contains(weaponToAdd))
        {
            obtainedWeapons.Add(weaponToAdd);
            selectedWeaponIndex = obtainedWeapons.Count - 1;
            EquipWeapon(selectedWeaponIndex);

            if (weaponToAdd.weaponImageUI != null) weaponToAdd.weaponImageUI.SetActive(true);
            if (weaponToAdd.inventorySlotText != null)
            {
                weaponToAdd.inventorySlotText.text = $"Slot {selectedWeaponIndex + 1}";
                weaponToAdd.inventorySlotText.gameObject.SetActive(true);
            }

            // Play pickup sound with custom volume
            if (weaponToAdd.pickupSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(weaponToAdd.pickupSound, pickupVolume);
            }
        }
    }

    public void DropWeapon()
    {
        if (selectedWeaponIndex == -1) return;

        Weapon droppedWeapon = obtainedWeapons[selectedWeaponIndex];

        droppedWeapon.weaponObject.SetActive(false);
        if (droppedWeapon.weaponImageUI != null) droppedWeapon.weaponImageUI.SetActive(false);
        if (droppedWeapon.ammoImageUI != null) droppedWeapon.ammoImageUI.SetActive(false);
        if (droppedWeapon.inventorySlotText != null) droppedWeapon.inventorySlotText.gameObject.SetActive(false);

        obtainedWeapons.RemoveAt(selectedWeaponIndex);

        if (droppedWeapon.weaponPrefab != null && throwPosition != null)
        {
            GameObject droppedWeaponObject = Instantiate
            (droppedWeapon.weaponPrefab, throwPosition.position, throwPosition.rotation);
            if (droppedWeaponObject.TryGetComponent(out Rigidbody rb))
            {
                rb.AddForce(throwPosition.forward * throwForce, ForceMode.Impulse);
            }
        }

        if (obtainedWeapons.Count > 0)
        {
            selectedWeaponIndex = Mathf.Clamp(selectedWeaponIndex, 0, obtainedWeapons.Count - 1);
            EquipWeapon(selectedWeaponIndex);
        }
        else
        {
            selectedWeaponIndex = -1;
        }
    }

    public void GetPistol() => AddWeapon("Pistol");
    public void GetRifle() => AddWeapon("Rifle");
    public void GetSniper() => AddWeapon("Sniper");
    public void GetShotgun() => AddWeapon("Shotgun");
    public void GetSMG() => AddWeapon("SMG");
}