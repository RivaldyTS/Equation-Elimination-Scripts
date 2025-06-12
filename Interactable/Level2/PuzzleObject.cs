using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections.Generic;

public class PuzzleObject : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private int requiredKeyValue; // The specific hidden value required for this puzzle
    [SerializeField] private bool destroyHeldObject = true;

    [Header("Player Reference")]
    [SerializeField] private Transform player;

    [Header("UI Settings")]
    [SerializeField] private TMP_Text feedbackText; // UI text to display feedback (e.g., "Access Granted")
    [SerializeField] private TMP_Text apparentValueText; // UI text to display the apparent value of the submitted key

    [Header("Text Customization")]
    [SerializeField] private string submittedTextFormat = "Submitted: {0}"; // Format for the apparent value text

    [Header("Events")]
    public UnityEvent OnSubmit; // Triggered when a key is submitted
    public UnityEvent OnWrongValue; // Triggered when a key with the wrong value is submitted

    [Header("Value-Specific Events")]
    [SerializeField] private List<ValueEventPair> valueSpecificEvents; // List of value-event pairs

    private bool isSolved = false; // Track if this puzzle is solved
    private bool hasWrongKey = false; // Track if a wrong key has been submitted
    private bool isPlayerInRange = false;

    // Dictionary to map values to their corresponding UnityEvents
    private Dictionary<int, UnityEvent> valueEventMap;

    // Event to notify the PuzzleManager when this puzzle is solved
    public UnityEvent onPuzzleSolved;

    [System.Serializable]
    public class ValueEventPair
    {
        public int value; // The value of the key
        public UnityEvent unityEvent; // The event to trigger for this value
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
    }

    void Update()
    {
        // Check if the player is in range and holding an object
        if (isPlayerInRange && !isSolved)
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
                // Automatically submit the held object
                SubmitHeldObject(playerPickup);
            }
        }
    }

    private void SubmitHeldObject(PlayerPickup playerPickup)
    {
        // Get the PickupObject component from the held object
        PickupObject pickupObject = playerPickup.GetHeldObject().GetComponent<PickupObject>();
        if (pickupObject != null)
        {
            // Get the hidden and apparent values of the held object
            int heldObjectHiddenValue = pickupObject.HiddenValue;
            string heldObjectApparentValue = pickupObject.ApparentValue;

            // Display the apparent value using the custom format
            if (apparentValueText != null)
            {
                apparentValueText.text = string.Format(submittedTextFormat, heldObjectApparentValue);
                apparentValueText.gameObject.SetActive(true);
            }

            // Trigger the OnSubmit event
            OnSubmit.Invoke();

            // Check if there's a value-specific event for this value
            if (valueEventMap.ContainsKey(heldObjectHiddenValue))
            {
                valueEventMap[heldObjectHiddenValue].Invoke(); // Trigger the value-specific event
                Debug.Log($"Value-specific event triggered for value {heldObjectHiddenValue}");
            }

            // Check if the held object's hidden value matches the required key value
            if (heldObjectHiddenValue == requiredKeyValue)
            {
                // Puzzle solved
                isSolved = true;
                hasWrongKey = false; // Reset wrong key state
                onPuzzleSolved.Invoke(); // Notify the PuzzleManager
                Debug.Log("Puzzle Solved: Correct key placed.");
            }
            else
            {
                // Incorrect key placed
                hasWrongKey = true; // Set wrong key state
                OnWrongValue.Invoke();
                Debug.Log("Access Denied: Incorrect key placed.");
            }

            // Destroy the held object if configured to do so
            if (destroyHeldObject)
            {
                Destroy(playerPickup.GetHeldObject());
            }

            // Drop the object (this will reset the player's held object)
            playerPickup.DropObject();
        }
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

    public bool IsSolved()
    {
        return isSolved;
    }

    public bool HasWrongKey()
    {
        return hasWrongKey;
    }

    public void ResetPuzzle()
    {
        isSolved = false; // Reset the solved state
        hasWrongKey = false; // Reset the wrong key state
        if (apparentValueText != null)
        {
            apparentValueText.gameObject.SetActive(false); // Hide the apparent value text
        }
        Debug.Log("Puzzle Reset: Ready to accept keys again.");
    }

    // Method to update the submitted text format dynamically
    public void SetSubmittedTextFormat(string format)
    {
        submittedTextFormat = format;
    }
}