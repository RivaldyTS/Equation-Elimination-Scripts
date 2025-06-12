using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class LinkedPuzzleSystem : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private int requiredKeyValue1 = 1; // Required key value for Puzzle 1
    [SerializeField] private int requiredKeyValue2 = 3; // Required key value for Puzzle 2
    [SerializeField] private bool destroyHeldObject = true;

    [Header("UI Settings")]
    [SerializeField] private TMP_Text feedbackText; // UI text to display feedback (e.g., "Access Granted")
    [SerializeField] private float uiDisplayRange = 5f;

    [Header("Events")]
    [SerializeField] private UnityEvent onBothPuzzlesSolved; // Triggered when both puzzles are solved
    [SerializeField] private UnityEvent onPuzzleReset; // Triggered when the puzzles are reset

    [Header("Player Reference")]
    [SerializeField] private Transform player;

    private bool isPuzzle1Solved = false; // Track if Puzzle 1 is solved
    private bool isPuzzle2Solved = false; // Track if Puzzle 2 is solved
    private bool isPlayerInRange = false;

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
        // Get the PickupObject component from the held object
        PickupObject pickupObject = playerPickup.GetHeldObject().GetComponent<PickupObject>();
        if (pickupObject != null)
        {
            // Get the value of the held object
            int heldObjectValue = pickupObject.Value; // Use the value from PickupObject

            // Check which puzzle the key belongs to
            if (heldObjectValue == requiredKeyValue1 && !isPuzzle1Solved)
            {
                // Puzzle 1 solved
                isPuzzle1Solved = true;
                Debug.Log("Puzzle 1 Solved: Correct key placed.");
            }
            else if (heldObjectValue == requiredKeyValue2 && !isPuzzle2Solved)
            {
                // Puzzle 2 solved
                isPuzzle2Solved = true;
                Debug.Log("Puzzle 2 Solved: Correct key placed.");
            }
            else
            {
                // Incorrect key placed
                Debug.Log("Access Denied: Incorrect key placed.");
            }

            // Destroy the held object if configured to do so
            if (destroyHeldObject)
            {
                Destroy(playerPickup.GetHeldObject());
            }

            // Drop the object (this will reset the player's held object)
            playerPickup.DropObject();

            // Check if both puzzles are solved
            if (isPuzzle1Solved && isPuzzle2Solved)
            {
                // Both puzzles solved
                if (feedbackText != null)
                {
                    feedbackText.text = "Access Granted";
                    feedbackText.color = Color.green;
                }
                onBothPuzzlesSolved.Invoke();
                Debug.Log("Both Puzzles Solved: Access Granted!");
            }
            else if (feedbackText != null)
            {
                feedbackText.text = "Access Denied";
                feedbackText.color = Color.red;
            }
        }
    }

    private void ResetPuzzles()
    {
        // Reset both puzzles
        isPuzzle1Solved = false;
        isPuzzle2Solved = false;
        onPuzzleReset.Invoke();
        Debug.Log("Puzzles Reset: Ready to accept keys again.");
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