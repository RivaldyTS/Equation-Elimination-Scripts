using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class FadeOverlayAndTime : MonoBehaviour
{
    [Header("Overlay Settings")]
    public GameObject overlayObject; // Assign the overlay GameObject in the Inspector
    public float fadeDuration = 2f; // Duration of the fade effect

    [Header("Start Scene Text Settings")]
    public bool useStartText = true; // Toggle to enable/disable start scene text
    public List<TextMeshProUGUI> startTextElements; // List of TextMeshProUGUI components for start scene
    public float startTextFadeInDuration = 1f; // Duration for start text fade-in
    public float startTextStayDuration = 1f; // Duration for start text to stay visible
    public float startTextFadeOutDuration = 1f; // Duration for start text fade-out

    [Header("Reset Scene Text Settings")]
    public bool useResetText = true; // Toggle to enable/disable reset scene text
    public List<TextMeshProUGUI> resetTextElements; // List of TextMeshProUGUI components for reset scene
    public float resetTextFadeInDuration = 1f; // Duration for reset text fade-in
    public float resetTextStayDuration = 1f; // Duration for reset text to stay visible
    public float resetTextFadeOutDuration = 1f; // Duration for reset text fade-out

    [Header("Audio Settings")]
    public AudioSource audioSource; // Assign an AudioSource in the Inspector

    [Header("Events")]
    public UnityEvent onOverlayComplete; // UnityEvent triggered when overlay fade completes

    private Image overlayImage;
    private float startTime;
    private Color initialOverlayColor;
    private List<Color> initialTextColors;
    private int currentTextIndex = 0;
    private bool isFading = false;
    private bool isReset = false; // Track if the scene is reset
    private bool hasTriggeredCompletion = false; // Prevent multiple triggers

    void Start()
    {
        if (overlayObject == null)
        {
            Debug.LogError("Overlay GameObject is not assigned!");
            return;
        }

        // Activate the overlay GameObject
        overlayObject.SetActive(true);

        // Get the Image component from the overlay GameObject
        overlayImage = overlayObject.GetComponent<Image>();
        if (overlayImage == null)
        {
            Debug.LogError("No Image component found on the overlay GameObject!");
            return;
        }

        // Validate audio setup
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource is not assigned. Audio won't play on overlay completion.");
        }
        else if (audioSource.clip == null)
        {
            Debug.LogWarning("AudioSource has no AudioClip assigned. Audio won't play.");
        }
        else
        {
            Debug.Log("AudioSource is set up with clip: " + audioSource.clip.name);
        }

        // Initialize overlay fade
        initialOverlayColor = overlayImage.color;
        startTime = Time.time;
        isFading = true;

        // Set initial time scale to 0.5 (slow motion)
        Time.timeScale = 0.5f;

        // Check if this is a reset
        if (SceneManager.GetActiveScene().name == "YourSceneName" && isReset)
        {
            InitializeText(resetTextElements, resetTextFadeInDuration, resetTextStayDuration, resetTextFadeOutDuration);
        }
        else
        {
            InitializeText(startTextElements, startTextFadeInDuration, startTextStayDuration, startTextFadeOutDuration);
        }
    }

    void InitializeText(List<TextMeshProUGUI> textElements, float fadeInDuration, float stayDuration, float fadeOutDuration)
    {
        if (useStartText && textElements != null && textElements.Count > 0)
        {
            initialTextColors = new List<Color>();
            foreach (var textElement in textElements)
            {
                if (textElement != null)
                {
                    textElement.gameObject.SetActive(true);
                    initialTextColors.Add(textElement.color);
                    textElement.color = new Color(textElement.color.r, textElement.color.g, textElement.color.b, 0f); // Start with transparent text
                }
            }
        }
    }

    void Update()
    {
        if (!isFading) return;

        // Calculate the elapsed time
        float elapsedTime = Time.time - startTime;

        // Handle text fade-in, stay, and fade-out
        List<TextMeshProUGUI> currentTextElements = isReset ? resetTextElements : startTextElements;
        float fadeInDuration = isReset ? resetTextFadeInDuration : startTextFadeInDuration;
        float stayDuration = isReset ? resetTextStayDuration : startTextStayDuration;
        float fadeOutDuration = isReset ? resetTextFadeOutDuration : startTextFadeOutDuration;

        if (currentTextElements != null && currentTextElements.Count > 0 && currentTextIndex < currentTextElements.Count)
        {
            float textStartTime = currentTextIndex * (fadeInDuration + stayDuration + fadeOutDuration);
            float textEndTime = textStartTime + fadeInDuration + stayDuration + fadeOutDuration;

            if (elapsedTime < textStartTime + fadeInDuration)
            {
                // Fade in current text
                float textAlpha = Mathf.Lerp(0f, 1f, (elapsedTime - textStartTime) / fadeInDuration);
                currentTextElements[currentTextIndex].color = new Color(initialTextColors[currentTextIndex].r, initialTextColors[currentTextIndex].g, initialTextColors[currentTextIndex].b, textAlpha);
            }
            else if (elapsedTime < textStartTime + fadeInDuration + stayDuration)
            {
                // Keep current text fully visible
                currentTextElements[currentTextIndex].color = new Color(initialTextColors[currentTextIndex].r, initialTextColors[currentTextIndex].g, initialTextColors[currentTextIndex].b, 1f);
            }
            else if (elapsedTime < textEndTime)
            {
                // Fade out current text
                float textAlpha = Mathf.Lerp(1f, 0f, (elapsedTime - textStartTime - fadeInDuration - stayDuration) / fadeOutDuration);
                currentTextElements[currentTextIndex].color = new Color(initialTextColors[currentTextIndex].r, initialTextColors[currentTextIndex].g, initialTextColors[currentTextIndex].b, textAlpha);

                // If this is the final text, fade out the overlay as well
                if (currentTextIndex == currentTextElements.Count - 1)
                {
                    float overlayAlpha = Mathf.Lerp(1f, 0f, (elapsedTime - textStartTime - fadeInDuration - stayDuration) / fadeOutDuration);
                    overlayImage.color = new Color(initialOverlayColor.r, initialOverlayColor.g, initialOverlayColor.b, overlayAlpha);

                    // Adjust time scale (0.5 to 1)
                    float timeScale = Mathf.Lerp(0.5f, 1f, (elapsedTime - textStartTime - fadeInDuration - stayDuration) / fadeOutDuration);
                    Time.timeScale = timeScale;
                }
            }
            else
            {
                // Move to the next text
                currentTextIndex++;

                // If all texts are done, deactivate everything and trigger audio/event
                if (currentTextIndex >= currentTextElements.Count)
                {
                    overlayObject.SetActive(false);
                    foreach (var textElement in currentTextElements)
                    {
                        if (textElement != null)
                        {
                            textElement.gameObject.SetActive(false);
                        }
                    }
                    isFading = false;

                    // Trigger audio and event when fade sequence completes
                    if (!hasTriggeredCompletion)
                    {
                        hasTriggeredCompletion = true;
                        if (audioSource != null)
                        {
                            Debug.Log("Playing AudioSource...");
                            audioSource.Play();
                        }
                        else
                        {
                            Debug.LogWarning("No AudioSource assigned to play.");
                        }
                        onOverlayComplete?.Invoke();
                        Debug.Log("Overlay fade complete. UnityEvent triggered.");
                    }
                }
            }
        }
    }

    // Call this method to reset the scene
    public void ResetScene()
    {
        isReset = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}