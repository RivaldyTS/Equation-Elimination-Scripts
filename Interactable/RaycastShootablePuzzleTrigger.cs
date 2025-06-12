using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;

public class RaycastShootablePuzzleTrigger : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private int requiredHits = 1; // Number of hits required to trigger the event
    [SerializeField] private float resetTimerDuration = 5f; // Time (in seconds) before resetting after no progress
    [SerializeField] private bool permanentCompletion = false; // If true, the puzzle cannot be triggered again after completion

    [Header("Conditional Activation")]
    [SerializeField] private bool requireActiveGameObject = false; // Enable to require a specific GameObject to be active
    [SerializeField] private GameObject requiredGameObject; // The GameObject that must be active to trigger the puzzle

    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI timerText; // TextMeshPro UI element to display the timer
    [SerializeField] private TextMeshProUGUI successText; // TextMeshPro UI element to display the success message
    [SerializeField] private TextMeshProUGUI resetText; // TextMeshPro UI element to display the reset message
    [SerializeField] private string successMessage = "Puzzle Complete!"; // Customizable success message
    [SerializeField] private string resetMessage = "Puzzle Reset!"; // Customizable reset message
    [SerializeField] private float messageDuration = 5f; // Duration (in seconds) to display the success/reset message
    [SerializeField] private float fadeOutDuration = 1f; // Duration (in seconds) for the fade-out effect

    [Header("Events")]
    [SerializeField] private UnityEvent onFirstHit; // Event triggered on the first successful hit
    [SerializeField] private UnityEvent onTriggered; // Event triggered when the required hits are reached
    [SerializeField] private UnityEvent onReset; // Event triggered when the hit count is reset

    private int currentHits = 0; // Track the number of hits
    private bool hasHitOnce = false; // Track if the player has hit at least once
    private float timeSinceLastHit; // Track the time since the last hit
    private bool isPuzzleComplete = false; // Track if the puzzle is permanently complete

    void Start()
    {
        // Deactivate all UI elements at the start
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (successText != null) successText.gameObject.SetActive(false);
        if (resetText != null) resetText.gameObject.SetActive(false);
    }

    void Update()
    {
        // If the puzzle is permanently complete, do nothing
        if (isPuzzleComplete && permanentCompletion)
        {
            return;
        }

        // If the player has hit at least once, start the timer
        if (hasHitOnce && currentHits < requiredHits)
        {
            timeSinceLastHit += Time.deltaTime;

            // Update the timer text
            if (timerText != null)
            {
                timerText.text = $"Time Left: {Mathf.Max(0, resetTimerDuration - timeSinceLastHit):F1}s";
            }

            // If the timer exceeds the reset duration, reset the hits
            if (timeSinceLastHit >= resetTimerDuration)
            {
                ResetHits();
            }
        }
    }

    // Public method to handle hits from the gun's raycast
    public void HandlePuzzleHit()
    {
        // If the puzzle is permanently complete, do nothing
        if (isPuzzleComplete && permanentCompletion)
        {
            return;
        }

        // Check if the required GameObject is active (if enabled)
        if (requireActiveGameObject && (requiredGameObject == null || !requiredGameObject.activeSelf))
        {
            Debug.Log("Puzzle cannot be triggered: Required GameObject is not active.");
            return;
        }

        // Handle the hit logic
        HandleRaycastHit();
    }

    // Method to call when the object is hit by a raycast
    private void HandleRaycastHit()
    {
        currentHits++;
        timeSinceLastHit = 0f;
        if (currentHits == 1 && timerText != null)
        {
            timerText.gameObject.SetActive(true);
        }
        if (currentHits == 1)
        {
            onFirstHit.Invoke();
            hasHitOnce = true;
        }
        if (currentHits >= requiredHits)
        {
            onTriggered.Invoke();
            if (successText != null)
            {
                successText.text = successMessage;
                successText.color = new Color(successText.color.r, successText.color.g, successText.color.b, 1f);
                successText.gameObject.SetActive(true);
                StartCoroutine(FadeOutText(successText));
            }
            if (timerText != null)
            {
                timerText.gameObject.SetActive(false);
            }
            if (permanentCompletion)
            {
                isPuzzleComplete = true;
            }
            if (!permanentCompletion)
            {
                currentHits = 0;
                hasHitOnce = false;
            }
        }
    }

    public void ResetHits()
    {
        currentHits = 0; // Reset the hit count
        onReset.Invoke(); // Trigger the reset event
        Debug.Log("Hits Reset!");
        hasHitOnce = false; // Reset the first hit flag
        timeSinceLastHit = 0f; // Reset the timer

        // Deactivate the timer text
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

        // Display the reset message
        if (resetText != null)
        {
            resetText.text = resetMessage;
            resetText.color = new Color(resetText.color.r, resetText.color.g, resetText.color.b, 1f); // Reset opacity
            resetText.gameObject.SetActive(true);
            StartCoroutine(FadeOutText(resetText));
        }
    }

    // Coroutine to fade out a TextMeshProUGUI element
    private IEnumerator FadeOutText(TextMeshProUGUI textElement)
    {
        // Wait for the message duration
        yield return new WaitForSeconds(messageDuration);

        // Fade out the text
        float elapsedTime = 0f;
        Color startColor = textElement.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            textElement.color = Color.Lerp(startColor, endColor, elapsedTime / fadeOutDuration);
            yield return null;
        }

        // Deactivate the text
        textElement.gameObject.SetActive(false);
    }
}