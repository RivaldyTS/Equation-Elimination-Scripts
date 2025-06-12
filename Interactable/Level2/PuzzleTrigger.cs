using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections.Generic;

public class PuzzleTrigger : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private List<KeyData> keys; // List of keys with hidden and apparent values
    [SerializeField] private bool destroyHeldObject = true;

    [Header("UI Settings")]
    [SerializeField] private TMP_Text feedbackText; // UI text to display feedback (e.g., "Access Granted")
    [SerializeField] private float uiDisplayRange = 5f;

    [Header("Events")]
    [SerializeField] private UnityEvent onAccessGranted;
    [SerializeField] private UnityEvent onAccessDenied;
    [SerializeField] private UnityEvent onReset;

    [Header("Player Reference")]
    [SerializeField] private Transform player;

    private List<int> placedKeys = new List<int>(); // Track placed keys by their hidden values
    private bool isPlayerInRange = false;

    [System.Serializable]
    public class KeyData
    {
        public int hiddenValue; // Hidden Key Variable (H)
        public string apparentValue; // Apparent Value (A) as a fraction string
    }

    void Start()
    {
        // Hide the feedback text initially
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
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
        if (feedbackText != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            bool isInRange = distanceToPlayer <= uiDisplayRange;
            feedbackText.gameObject.SetActive(isInRange);
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
                // Automatically submit the held object
                SubmitHeldObject(playerPickup);
            }
        }
    }

    private void SubmitHeldObject(PlayerPickup playerPickup)
    {
        // Get the hidden value of the held object
        int heldObjectHiddenValue = playerPickup.GetHeldObjectHiddenValue();
        if (heldObjectHiddenValue > 0)
        {
            // Check if the key is already placed
            if (!placedKeys.Contains(heldObjectHiddenValue))
            {
                placedKeys.Add(heldObjectHiddenValue); // Add the key to the placed keys list
                ValidateKeys(); // Validate the placed keys
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

    private void ValidateKeys()
    {
        // Check if both required keys (1 and 3) are placed
        bool hasKey1 = placedKeys.Contains(1);
        bool hasKey3 = placedKeys.Contains(3);

        if (hasKey1 && hasKey3)
        {
            // Access Granted
            if (feedbackText != null)
            {
                feedbackText.text = "Access Granted";
                feedbackText.color = Color.green;
            }
            onAccessGranted.Invoke();
            Debug.Log("Access Granted: Required keys (1 and 3) are placed.");
        }
        else
        {
            // Access Denied
            if (feedbackText != null)
            {
                feedbackText.text = "Access Denied";
                feedbackText.color = Color.red;
            }
            onAccessDenied.Invoke();
            Debug.Log("Access Denied: Required keys (1 and 3) are not placed.");
        }
    }

    private void ResetPuzzle()
    {
        placedKeys.Clear(); // Clear the list of placed keys
        onReset.Invoke(); // Trigger the reset event
        Debug.Log("Puzzle Reset: All keys removed.");
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