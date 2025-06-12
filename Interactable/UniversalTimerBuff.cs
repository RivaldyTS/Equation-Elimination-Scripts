using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UniversalTimerBuff : MonoBehaviour
{
    [Header("Buff Settings")]
    [SerializeField] private bool enableSpeedBuff = true; // Enable/disable speed buff
    [SerializeField] private float speedMultiplier = 1.5f; // Speed multiplier during the buff
    [SerializeField] private string speedBuffName = "Speed Buff"; // Customizable name for speed buff
    [SerializeField] private bool enableHealthRegen = false; // Enable/disable health regeneration
    [SerializeField] private float healthRegenRate = 5f; // Health regenerated per second
    [SerializeField] private string healthRegenName = "HP Generation"; // Customizable name for health regen
    [SerializeField] private float buffDuration = 5f; // Duration of the buff in seconds

    [Header("Trigger Settings")]
    [SerializeField] private string triggerTag = "Player"; // Tag of the object that triggers the buff
    [SerializeField] private LayerMask triggerLayer; // Layer of objects that can trigger the buff

    [Header("UI Settings")]
    [SerializeField] private GameObject buffTextObject; // GameObject containing the TextMeshPro UI
    [SerializeField] private TMP_Text buffText; // TextMeshPro UI to display active buffs
    [SerializeField] private GameObject buffIconPrefab; // Prefab for buff icons (optional)
    [SerializeField] private Transform buffIconParent; // Parent transform for buff icons (optional)

    [Header("Cooldown Settings")]
    [SerializeField] private bool enableCooldown = true; // Enable/disable cooldown
    [SerializeField] private float cooldownDuration = 1f; // Cooldown duration in seconds

    private bool isBuffActive = false; // Track if the buff is currently active
    private float buffEndTime; // Track when the buff ends
    private float cooldownEndTime; // Track when the cooldown ends

    // Original values to restore after the buff ends
    private float originalSpeed;
    private MonoBehaviour targetScript; // Reference to the script that controls movement (e.g., PlayerController)
    private PlayerHealth targetHealth; // Reference to the PlayerHealth component (if health regen is enabled)

    // Active buffs
    private Dictionary<string, float> activeBuffs = new Dictionary<string, float>(); // Tracks buff names and their end times

    void Start()
    {
        // Initialize UI
        if (buffTextObject != null)
        {
            buffTextObject.SetActive(false); // Hide the buff text initially
        }
    }

    void Update()
    {
        // Check if the buff is active and if it's time to end it
        if (isBuffActive && Time.time >= buffEndTime)
        {
            EndBuff();
        }

        // Apply health regeneration (if enabled and buff is active)
        if (isBuffActive && enableHealthRegen && targetHealth != null)
        {
            targetHealth.RestoreHealth(healthRegenRate * Time.deltaTime); // Heal over time
        }

        // Update the UI
        UpdateBuffUI();
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object can trigger the buff and if cooldown is active
        if (CanTriggerBuff(other.gameObject) && !IsOnCooldown())
        {
            Debug.Log("Buff triggered by: " + other.gameObject.name);
            ApplyBuff(other.gameObject);
        }
    }

    // Public method to apply the buff externally
    public void ApplyBuff(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning("Target is null. Cannot apply buff.");
            return;
        }

        // Check if the buff is already active or on cooldown
        if (isBuffActive || IsOnCooldown())
        {
            Debug.Log("Buff is already active or on cooldown.");
            return;
        }

        // Get the target script (e.g., PlayerController)
        targetScript = target.GetComponent<MonoBehaviour>();
        targetHealth = target.GetComponent<PlayerHealth>();

        // Apply the speed buff (if enabled)
        if (enableSpeedBuff && targetScript != null)
        {
            // Example: Assuming the target script has a "speed" field
            var speedField = targetScript.GetType().GetField("speed");
            if (speedField != null)
            {
                // Save the original speed
                originalSpeed = (float)speedField.GetValue(targetScript);

                // Apply the speed multiplier
                float newSpeed = originalSpeed * speedMultiplier;
                speedField.SetValue(targetScript, newSpeed);

                activeBuffs.Add(speedBuffName, Time.time + buffDuration); // Track buff end time
                Debug.Log("Speed buff applied. New speed: " + newSpeed);
            }
            else
            {
                Debug.LogWarning("Target script does not have a 'speed' field.");
            }
        }

        // Apply health regeneration (if enabled)
        if (enableHealthRegen && targetHealth != null)
        {
            activeBuffs.Add(healthRegenName, Time.time + buffDuration); // Track buff end time
            Debug.Log("Health regen applied.");
        }

        // Set the buff as active
        isBuffActive = true;
        buffEndTime = Time.time + buffDuration;

        // Activate the buff text UI
        if (buffTextObject != null)
        {
            buffTextObject.SetActive(true);
        }

        // Start the cooldown
        if (enableCooldown)
        {
            cooldownEndTime = Time.time + cooldownDuration;
        }

        Debug.Log("Buff applied. Duration: " + buffDuration + " seconds.");
    }

    private bool CanTriggerBuff(GameObject obj)
    {
        // Check if the object is on the correct layer or has the correct tag
        return triggerLayer == (triggerLayer | (1 << obj.layer)) || obj.CompareTag(triggerTag);
    }

    private bool IsOnCooldown()
    {
        // Check if the cooldown is active
        if (enableCooldown && Time.time < cooldownEndTime)
        {
            Debug.Log("Buff is on cooldown.");
            return true;
        }
        return false;
    }

    private void EndBuff()
    {
        // Restore the original speed (if the speed buff was applied)
        if (enableSpeedBuff && targetScript != null)
        {
            var speedField = targetScript.GetType().GetField("speed");
            if (speedField != null)
            {
                speedField.SetValue(targetScript, originalSpeed);
                activeBuffs.Remove(speedBuffName);
                Debug.Log("Speed buff ended. Original speed restored: " + originalSpeed);
            }
        }

        // End health regeneration (if enabled)
        if (enableHealthRegen && targetHealth != null)
        {
            activeBuffs.Remove(healthRegenName);
            Debug.Log("Health regen ended.");
        }

        // Reset the buff state
        isBuffActive = false;
        targetScript = null;
        targetHealth = null;

        // Deactivate the buff text UI if no buffs are active
        if (buffTextObject != null && activeBuffs.Count == 0)
        {
            buffTextObject.SetActive(false);
        }

        Debug.Log("Buff ended.");
    }

    private void UpdateBuffUI()
    {
        // Update the TextMeshPro UI with active buffs and their remaining durations
        if (buffText != null)
        {
            buffText.text = "Active Buffs:\n";
            foreach (var buff in activeBuffs)
            {
                float remainingTime = buff.Value - Time.time;
                if (remainingTime > 0)
                {
                    buffText.text += $"- {buff.Key}: {remainingTime.ToString("F1")}s\n";
                }
            }
        }

        // Update buff icons (optional)
        if (buffIconPrefab != null && buffIconParent != null)
        {
            // Clear existing icons
            foreach (Transform child in buffIconParent)
            {
                Destroy(child.gameObject);
            }

            // Create new icons for active buffs
            foreach (var buff in activeBuffs)
            {
                GameObject icon = Instantiate(buffIconPrefab, buffIconParent);
                icon.GetComponentInChildren<TMP_Text>().text = $"{buff.Key}\n{buff.Value - Time.time:F1}s";
            }
        }
    }
}