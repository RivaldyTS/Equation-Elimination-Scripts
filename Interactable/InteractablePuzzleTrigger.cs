using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections.Generic;

public class InteractablePuzzleTrigger : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private int requiredValue = 5;
    [SerializeField] private bool destroyHeldObject = true;

    [Header("UI Settings")]
    [SerializeField] private TMP_Text valueText; // UI text to display the current value
    [SerializeField] private TMP_Text submitText; // UI text to display the "Submit" notification
    [SerializeField] private float uiDisplayRange = 5f;

    [Header("Events")]
    [SerializeField] private UnityEvent onPartialProgress;
    [SerializeField] private UnityEvent onGoalMet;
    [SerializeField] private UnityEvent onReset;

    [Header("Value-Specific Events")]
    [SerializeField] private List<ValueEventPair> valueSpecificEvents;

    [Header("Player Reference")]
    [SerializeField] private Transform player;

    private int currentValue = 0;
    private bool isPlayerInRange = false; // Track if the player is in range

    // Dictionary to store value-specific events
    private Dictionary<int, UnityEvent> valueEventMap;

    [System.Serializable]
    public class ValueEventPair
    {
        public int value;
        public UnityEvent unityEvent;
    }

    void Start()
    {
        // Initialize the dictionary
        valueEventMap = new Dictionary<int, UnityEvent>();
        foreach (var pair in valueSpecificEvents)
        {
            if (!valueEventMap.ContainsKey(pair.value))
            {
                valueEventMap.Add(pair.value, pair.unityEvent);
            }
            else
            {
                Debug.LogWarning($"Duplicate value {pair.value} found in value-specific events. Skipping.");
            }
        }

        // Hide the UI text initially
        if (valueText != null)
        {
            valueText.gameObject.SetActive(false);
        }

        // Hide the submit text initially
        if (submitText != null)
        {
            submitText.gameObject.SetActive(false);
        }

        // Debug warning if player is not assigned
        if (player == null)
        {
            Debug.LogWarning("Player reference is not assigned in the inspector.");
        }
    }

    void Update()
    {
        // Check if the player is within UI display range
        if (valueText != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            bool isInRange = distanceToPlayer <= uiDisplayRange;
            valueText.gameObject.SetActive(isInRange);
            valueText.text = "Nilai yang dimasuki: " + currentValue + " / " + "";
        }

        // Check if the player is in range and holding an object
        if (isPlayerInRange)
        {
            // Get the PlayerPickup component from the player
            PlayerPickup playerPickup = player.GetComponent<PlayerPickup>();
            if (playerPickup == null)
            {
                Debug.LogWarning("PlayerPickup component not found on the player.");
                return;
            }

            // Check if the player is holding an object
            GameObject heldObject = playerPickup.GetHeldObject();
            if (heldObject != null)
            {
                // Show the "Submit" notification
                if (submitText != null)
                {
                    submitText.gameObject.SetActive(true);
                    submitText.text = "Submit";
                }

                // Automatically submit the held object
                SubmitHeldObject(playerPickup);
            }
            else if (submitText != null)
            {
                submitText.gameObject.SetActive(false);
            }
        }
        else if (submitText != null)
        {
            submitText.gameObject.SetActive(false);
        }
    }

    private void SubmitHeldObject(PlayerPickup playerPickup)
    {
        // Get the value of the held object
        int heldObjectValue = playerPickup.GetHeldObjectValue();
        if (heldObjectValue > 0)
        {
            // Deposit the value into the puzzle
            DepositValue(heldObjectValue);

            // Destroy the held object if configured to do so
            if (destroyHeldObject)
            {
                Destroy(playerPickup.GetHeldObject());
            }

            // Drop the object (this will reset the player's held object)
            playerPickup.DropObject();

            // Hide the submit text after submission
            if (submitText != null)
            {
                submitText.gameObject.SetActive(false);
            }
        }
    }

    private void DepositValue(int value)
    {
        int newValue = currentValue + value; // Calculate the new value

        // Check if the new value exceeds the required value
        if (newValue > requiredValue)
        {
            ResetPuzzle(); // Reset the puzzle if the value exceeds the goal
            return; // Exit the method to prevent further processing
        }

        currentValue = newValue; // Update the current value

        // Check if the goal value is met
        if (currentValue == requiredValue)
        {
            onGoalMet.Invoke(); // Trigger the goal met event
            Debug.Log("Puzzle Solved: Goal value reached!");
        }
        else if (currentValue < requiredValue)
        {
            onPartialProgress.Invoke(); // Trigger the partial progress event
            Debug.Log("Partial Progress: Current value = " + currentValue);
        }

        // Check if there's an event for the current value
        if (valueEventMap.ContainsKey(currentValue))
        {
            valueEventMap[currentValue].Invoke(); // Trigger the value-specific event
            Debug.Log($"Value-specific event triggered for value {currentValue}");
        }
    }

    private void ResetPuzzle()
    {
        currentValue = 0; // Reset the current value
        onReset.Invoke(); // Trigger the reset event
        Debug.Log("Puzzle Reset: Value exceeded the goal!");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        if (other.transform == player)
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the player exited the trigger
        if (other.transform == player)
        {
            isPlayerInRange = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, uiDisplayRange);
    }
}