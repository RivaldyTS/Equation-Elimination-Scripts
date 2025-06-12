using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class EnemyDeathFadeInOut : MonoBehaviour
{
    [Header("Overlay Settings")]
    public GameObject overlayObject; // Assign the overlay GameObject in the Inspector
    public float fadeDuration = 2f; // Duration of each fade (in and out)

    [Header("Time Scale Settings")]
    public bool adjustTimeScale = false; // Enable time scale adjustment
    public float slowTimeScale = 0.5f; // Time scale during fade-in

    [Header("Events")]
    public UnityEvent onFadeComplete; // Event to notify when the entire fade sequence is complete

    private Image overlayImage;
    private float startTime;
    private Color initialColor;
    private bool isFading = false;
    private bool isFadeInPhase = true; // Tracks whether the current phase is fade-in or fade-out

    void Start()
    {
        if (overlayObject == null)
        {
            Debug.LogError("Overlay GameObject is not assigned!");
            return;
        }

        // Get the Image component from the overlay GameObject
        overlayImage = overlayObject.GetComponent<Image>();
        if (overlayImage == null)
        {
            Debug.LogError("No Image component found on the overlay GameObject!");
            return;
        }

        // Initialize the overlay color
        initialColor = overlayImage.color;

        // Ensure the overlay is fully transparent at the start
        overlayImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        overlayObject.SetActive(false); // Deactivate the overlay initially
    }

    // Method to trigger the fade-in and fade-out sequence
    public void OnEnemyDeath()
    {
        // Activate the overlay GameObject
        overlayObject.SetActive(true);

        // Start the fade-in phase
        isFading = true;
        isFadeInPhase = true;
        startTime = Time.time;

        // Adjust time scale if enabled
        if (adjustTimeScale)
        {
            Time.timeScale = slowTimeScale;
        }
    }

    void Update()
    {
        if (!isFading) return;

        // Calculate the elapsed time
        float elapsedTime = Time.time - startTime;

        // Fade the overlay based on the current phase
        if (isFadeInPhase)
        {
            // Fade-in phase: 0% to 100%
            if (elapsedTime < fadeDuration)
            {
                float progress = elapsedTime / fadeDuration;
                float alpha = Mathf.Lerp(0f, 1f, progress);
                overlayImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            }
            else
            {
                // Ensure the overlay is fully visible
                overlayImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, 1f);

                // Transition to the fade-out phase
                isFadeInPhase = false;
                startTime = Time.time; // Reset the timer for the fade-out phase
            }
        }
        else
        {
            // Fade-out phase: 100% to 0%
            if (elapsedTime < fadeDuration)
            {
                float progress = elapsedTime / fadeDuration;
                float alpha = Mathf.Lerp(1f, 0f, progress);
                overlayImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);

                // Adjust time scale back to normal during fade-out
                if (adjustTimeScale)
                {
                    Time.timeScale = Mathf.Lerp(slowTimeScale, 1f, progress);
                }
            }
            else
            {
                // Ensure the overlay is fully transparent
                overlayImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);

                // Reset time scale to normal
                if (adjustTimeScale)
                {
                    Time.timeScale = 1f;
                }

                // Deactivate the overlay GameObject
                overlayObject.SetActive(false);

                // Notify listeners that the entire fade sequence is complete
                onFadeComplete.Invoke();
                isFading = false;
            }
        }
    }
}