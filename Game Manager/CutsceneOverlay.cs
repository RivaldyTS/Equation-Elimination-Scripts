using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;

public class CutsceneOverlay : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneFrame
    {
        public Sprite sprite; // The image to display
        public AudioClip transitionInSound; // Sound to play when fading in
        public AudioClip transitionOutSound; // Sound to play when fading out
        [TextArea] public string narrationText; // Unique text to display with this frame
    }

    [Header("Overlay Settings")]
    public GameObject overlayObject; // Assign the overlay GameObject (black background) in the Inspector
    public float overlayFadeInDuration = 1f; // Duration for overlay fade-in
    public float overlayFadeOutDuration = 1f; // Duration for overlay fade-out (if not skipped)
    public bool skipOverlayFadeOut = false; // Option to skip overlay fade-out after last image

    [Header("Cutscene Image Settings")]
    public Image cutsceneImage; // Single UI Image component for displaying sprites
    public List<CutsceneFrame> cutsceneFrames; // List of frames with sprites, audio, and text
    public float imageFadeInDuration = 1f; // Duration for each image fade-in
    public float imageStayDuration = 1f; // Duration for each image to stay visible (before text fades out)
    public float imageFadeOutDuration = 1f; // Duration for each image fade-out

    [Header("Text Settings")]
    public TextMeshProUGUI narrationTextElement; // Single TextMeshProUGUI for all frame texts
    public float textFadeInDuration = 0.5f; // Duration for text fade-in and typewriter effect
    public float textStayDuration = 1f; // Duration for text to stay visible
    public float textFadeOutDuration = 0.5f; // Duration for text fade-out

    [Header("Vignette Settings")]
    [Range(0f, 1f)] public float vignetteRadius = 0.5f; // Controls the size of the vignette effect
    [Range(0f, 1f)] public float vignetteSoftness = 0.5f; // Controls the softness of the vignette edges

    [Header("Audio Settings")]
    public AudioSource audioSource; // AudioSource for transition sounds and end sound
    public AudioClip endSound; // Optional sound to play when cutscene ends

    [Header("Events")]
    public UnityEvent onCutsceneComplete; // UnityEvent triggered when cutscene finishes

    private Image overlayImage;
    private float startTime;
    private Color initialOverlayColor;
    private Color initialImageColor;
    private Color initialTextColor;
    private int currentFrameIndex = 0;
    private bool isPlaying = false;
    private bool hasTriggeredCompletion = false;
    private List<AudioSource> sceneAudioSources = new List<AudioSource>();
    private Material vignetteMaterial;

    private enum CutscenePhase { OverlayFadeIn, ImageSequence, OverlayFadeOut }
    private CutscenePhase currentPhase = CutscenePhase.OverlayFadeIn;

    void Awake()
    {
        if (overlayObject == null)
        {
            Debug.LogError("Overlay GameObject is not assigned!");
            return;
        }

        overlayImage = overlayObject.GetComponent<Image>();
        if (overlayImage == null)
        {
            Debug.LogError("No Image component found on the overlay GameObject!");
            return;
        }

        if (cutsceneImage == null)
        {
            Debug.LogError("Cutscene Image is not assigned!");
            return;
        }

        if (cutsceneFrames == null || cutsceneFrames.Count == 0)
        {
            Debug.LogError("No cutscene frames assigned!");
            return;
        }

        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource not assigned. Transition sounds won't play.");
        }

        if (narrationTextElement != null)
        {
            initialTextColor = narrationTextElement.color;
            narrationTextElement.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Narration Text Element not assigned. Text overlays will be skipped.");
        }

        vignetteMaterial = new Material(Shader.Find("UI/Vignette"));
        if (vignetteMaterial == null)
        {
            Debug.LogError("Vignette shader not found! Please ensure 'UI/Vignette' shader exists.");
        }
        else
        {
            vignetteMaterial.SetFloat("_VignetteRadius", vignetteRadius);
            vignetteMaterial.SetFloat("_VignetteSoftness", vignetteSoftness);
        }

        overlayObject.SetActive(false);
        cutsceneImage.gameObject.SetActive(false);
        initialOverlayColor = overlayImage.color;
        initialImageColor = cutsceneImage.color;
    }

    public void PlayCutscene()
    {
        if (isPlaying)
        {
            Debug.LogWarning("Cutscene is already playing!");
            return;
        }

        overlayObject.SetActive(true);
        overlayImage.color = new Color(initialOverlayColor.r, initialOverlayColor.g, initialOverlayColor.b, 0f);
        cutsceneImage.gameObject.SetActive(true);
        cutsceneImage.sprite = cutsceneFrames[0].sprite;
        cutsceneImage.color = new Color(initialImageColor.r, initialImageColor.g, initialImageColor.b, 0f);

        if (vignetteMaterial != null)
        {
            cutsceneImage.material = vignetteMaterial;
        }

        if (narrationTextElement != null)
        {
            narrationTextElement.gameObject.SetActive(true);
            narrationTextElement.text = ""; // Start with empty text for typewriter effect
            narrationTextElement.color = new Color(initialTextColor.r, initialTextColor.g, initialTextColor.b, 1f);
        }

        Time.timeScale = 0f;
        sceneAudioSources.Clear();
        foreach (var audio in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            if (audio != audioSource)
            {
                sceneAudioSources.Add(audio);
                if (audio.isPlaying)
                {
                    audio.Pause();
                }
            }
        }

        startTime = Time.unscaledTime;
        currentFrameIndex = 0;
        currentPhase = CutscenePhase.OverlayFadeIn;
        isPlaying = true;
        hasTriggeredCompletion = false;
    }

    void Update()
    {
        if (!isPlaying) return;

        float elapsedTime = Time.unscaledTime - startTime;

        switch (currentPhase)
        {
            case CutscenePhase.OverlayFadeIn:
                if (elapsedTime < overlayFadeInDuration)
                {
                    float alpha = Mathf.Lerp(0f, 1f, elapsedTime / overlayFadeInDuration);
                    overlayImage.color = new Color(initialOverlayColor.r, initialOverlayColor.g, initialOverlayColor.b, alpha);
                }
                else
                {
                    overlayImage.color = new Color(initialOverlayColor.r, initialOverlayColor.g, initialOverlayColor.b, 1f);
                    currentPhase = CutscenePhase.ImageSequence;
                    startTime = Time.unscaledTime;
                }
                break;

            case CutscenePhase.ImageSequence:
                if (currentFrameIndex < cutsceneFrames.Count)
                {
                    float frameDuration = imageFadeInDuration + textFadeInDuration + textStayDuration + textFadeOutDuration + imageFadeOutDuration;
                    float frameStartTime = currentFrameIndex * frameDuration;
                    float frameEndTime = frameStartTime + frameDuration;

                    // Image Fade In
                    if (elapsedTime < frameStartTime + imageFadeInDuration)
                    {
                        float alpha = Mathf.Lerp(0f, 1f, (elapsedTime - frameStartTime) / imageFadeInDuration);
                        cutsceneImage.color = new Color(initialImageColor.r, initialImageColor.g, initialImageColor.b, alpha);

                        if (audioSource != null && cutsceneFrames[currentFrameIndex].transitionInSound != null && !audioSource.isPlaying)
                        {
                            audioSource.clip = cutsceneFrames[currentFrameIndex].transitionInSound;
                            audioSource.volume = alpha;
                            audioSource.Play();
                        }
                    }
                    // Image Fully Visible, Text Fade In with Typewriter Effect
                    else if (elapsedTime < frameStartTime + imageFadeInDuration + textFadeInDuration)
                    {
                        cutsceneImage.color = new Color(initialImageColor.r, initialImageColor.g, initialImageColor.b, 1f);
                        float textTime = elapsedTime - (frameStartTime + imageFadeInDuration);
                        float progress = textTime / textFadeInDuration;
                        if (narrationTextElement != null && !string.IsNullOrEmpty(cutsceneFrames[currentFrameIndex].narrationText))
                        {
                            int totalChars = cutsceneFrames[currentFrameIndex].narrationText.Length;
                            int visibleChars = Mathf.FloorToInt(progress * totalChars);
                            visibleChars = Mathf.Clamp(visibleChars, 0, totalChars);
                            narrationTextElement.text = cutsceneFrames[currentFrameIndex].narrationText.Substring(0, visibleChars);
                            narrationTextElement.color = new Color(initialTextColor.r, initialTextColor.g, initialTextColor.b, 1f);
                        }
                        if (audioSource != null && audioSource.isPlaying)
                        {
                            audioSource.volume = 1f;
                        }
                    }
                    // Image and Text Fully Visible
                    else if (elapsedTime < frameStartTime + imageFadeInDuration + textFadeInDuration + textStayDuration)
                    {
                        cutsceneImage.color = new Color(initialImageColor.r, initialImageColor.g, initialImageColor.b, 1f);
                        if (narrationTextElement != null && !string.IsNullOrEmpty(cutsceneFrames[currentFrameIndex].narrationText))
                        {
                            narrationTextElement.text = cutsceneFrames[currentFrameIndex].narrationText;
                            narrationTextElement.color = new Color(initialTextColor.r, initialTextColor.g, initialTextColor.b, 1f);
                        }
                        if (audioSource != null && audioSource.isPlaying)
                        {
                            audioSource.volume = 1f;
                        }
                    }
                    // Text Fade Out
                    else if (elapsedTime < frameStartTime + imageFadeInDuration + textFadeInDuration + textStayDuration + textFadeOutDuration)
                    {
                        cutsceneImage.color = new Color(initialImageColor.r, initialImageColor.g, initialImageColor.b, 1f);
                        float textOutTime = elapsedTime - (frameStartTime + imageFadeInDuration + textFadeInDuration + textStayDuration);
                        float alpha = Mathf.Lerp(1f, 0f, textOutTime / textFadeOutDuration);
                        if (narrationTextElement != null)
                        {
                            narrationTextElement.color = new Color(initialTextColor.r, initialTextColor.g, initialTextColor.b, alpha);
                        }
                        if (audioSource != null && cutsceneFrames[currentFrameIndex].transitionOutSound != null && !audioSource.isPlaying)
                        {
                            audioSource.clip = cutsceneFrames[currentFrameIndex].transitionOutSound;
                            audioSource.volume = alpha;
                            audioSource.Play();
                        }
                        else if (audioSource != null && audioSource.isPlaying)
                        {
                            audioSource.volume = alpha;
                        }
                    }
                    // Image Fade Out
                    else if (elapsedTime < frameEndTime)
                    {
                        float imageOutTime = elapsedTime - (frameStartTime + imageFadeInDuration + textFadeInDuration + textStayDuration + textFadeOutDuration);
                        float alpha = Mathf.Lerp(1f, 0f, imageOutTime / imageFadeOutDuration);
                        cutsceneImage.color = new Color(initialImageColor.r, initialImageColor.g, initialImageColor.b, alpha);
                        if (narrationTextElement != null)
                        {
                            narrationTextElement.color = new Color(initialTextColor.r, initialTextColor.g, initialTextColor.b, 0f);
                        }
                        if (audioSource != null && audioSource.isPlaying)
                        {
                            audioSource.volume = alpha;
                        }
                    }
                    else
                    {
                        currentFrameIndex++;
                        if (currentFrameIndex < cutsceneFrames.Count)
                        {
                            cutsceneImage.sprite = cutsceneFrames[currentFrameIndex].sprite;
                            cutsceneImage.color = new Color(initialImageColor.r, initialImageColor.g, initialImageColor.b, 0f);
                            if (narrationTextElement != null)
                            {
                                narrationTextElement.text = "";
                                narrationTextElement.color = new Color(initialTextColor.r, initialTextColor.g, initialTextColor.b, 1f);
                            }
                            if (audioSource != null)
                            {
                                audioSource.Stop();
                            }
                        }
                        else
                        {
                            if (skipOverlayFadeOut)
                            {
                                // Skip fade-out, keep overlay on, and end immediately
                                overlayObject.SetActive(true); // Ensure overlay stays on
                                overlayImage.color = new Color(initialOverlayColor.r, initialOverlayColor.g, initialOverlayColor.b, 1f);
                                cutsceneImage.gameObject.SetActive(false);
                                cutsceneImage.material = null;
                                isPlaying = false;

                                Time.timeScale = 1f;
                                foreach (var audio in sceneAudioSources)
                                {
                                    if (audio != null && !audio.isPlaying)
                                    {
                                        audio.UnPause();
                                    }
                                }

                                if (!hasTriggeredCompletion)
                                {
                                    hasTriggeredCompletion = true;
                                    if (audioSource != null && endSound != null)
                                    {
                                        audioSource.clip = endSound;
                                        audioSource.volume = 1f;
                                        audioSource.Play();
                                    }
                                    onCutsceneComplete?.Invoke();
                                    Debug.Log("Cutscene complete (no fade-out). UnityEvent triggered.");
                                }
                            }
                            else
                            {
                                // Proceed to normal fade-out phase
                                currentPhase = CutscenePhase.OverlayFadeOut;
                                startTime = Time.unscaledTime;
                            }
                            if (audioSource != null)
                            {
                                audioSource.Stop();
                            }
                            if (narrationTextElement != null)
                            {
                                narrationTextElement.gameObject.SetActive(false);
                            }
                        }
                    }
                }
                break;

            case CutscenePhase.OverlayFadeOut:
                if (elapsedTime < overlayFadeOutDuration)
                {
                    float alpha = Mathf.Lerp(1f, 0f, elapsedTime / overlayFadeOutDuration);
                    overlayImage.color = new Color(initialOverlayColor.r, initialOverlayColor.g, initialOverlayColor.b, alpha);
                }
                else
                {
                    overlayObject.SetActive(false);
                    cutsceneImage.gameObject.SetActive(false);
                    cutsceneImage.material = null;
                    isPlaying = false;

                    Time.timeScale = 1f;
                    foreach (var audio in sceneAudioSources)
                    {
                        if (audio != null && !audio.isPlaying)
                        {
                            audio.UnPause();
                        }
                    }

                    if (!hasTriggeredCompletion)
                    {
                        hasTriggeredCompletion = true;
                        if (audioSource != null && endSound != null)
                        {
                            audioSource.clip = endSound;
                            audioSource.volume = 1f;
                            audioSource.Play();
                        }
                        onCutsceneComplete?.Invoke();
                        Debug.Log("Cutscene complete. UnityEvent triggered.");
                    }
                }
                break;
        }
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
        foreach (var audio in sceneAudioSources)
        {
            if (audio != null && !audio.isPlaying)
            {
                audio.UnPause();
            }
        }
        if (cutsceneImage != null)
        {
            cutsceneImage.material = null;
        }
        if (narrationTextElement != null)
        {
            narrationTextElement.gameObject.SetActive(false);
        }
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    void OnDestroy()
    {
        if (vignetteMaterial != null)
        {
            Destroy(vignetteMaterial);
        }
    }
}