using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DialogueEntry
{
    public AudioSource audioSource; // Reference to the AudioSource component
    public string titleName; // Name to display as the speaker
    public string dialogueText; // Dialogue text to display
}

public class SubtitleDialogueManager : MonoBehaviour
{
    [Header("UI Components")]
    public Canvas canvas; // Canvas to parent the text container
    public RectTransform textContainer; // Parent RectTransform for spawning texts
    public TextMeshProUGUI titleTextPrefab; // Prefab for the title name text (TMP)
    public TextMeshProUGUI dialogueTextPrefab; // Prefab for the dialogue text (TMP)

    [Header("Dialogue Settings")]
    public DialogueEntry[] dialogueEntries; // Array of audio sources, titles, and dialogues
    [Header("Position Offsets")]
    public Vector2 titleOffset; // Offset for title text position
    public Vector2 dialogueOffset; // Offset for dialogue text position
    [Header("Animation Durations")]
    public float fadeInDuration = 1f; // Duration of fade-in animation (seconds)
    public float fadeOutDuration = 1f; // Duration of fade-out animation (seconds)
    [Tooltip("Seconds per character for typewriter effect. Smaller = faster typing. Overridden to sync with audio unless disabled.")]
    public float writingSpeed = 0.05f; // Seconds per character for typewriter effect
    [Tooltip("Disable to use writingSpeed directly instead of syncing with audio")]
    public bool syncWritingWithAudio = true; // Toggle to sync typing with audio duration
    [Header("Audio Settings")]
    [Tooltip("Volume scale for other AudioSources when dialogue audio is playing (0 to 1).")]
    public float otherAudioVolumeScale = 0.2f; // Volume scale for non-dialogue audio (e.g., 0.2 = 20%)

    private TextMeshProUGUI[] spawnedTitleTexts; // Array to store spawned title text instances
    private TextMeshProUGUI[] spawnedDialogueTexts; // Array to store spawned dialogue text instances
    private bool[] isDialogueActive; // Tracks whether a dialogue sequence is active
    private bool[] wasPausedByTimeScale; // Tracks which audio sources were paused due to time scale
    private Coroutine[] dialogueCoroutines; // Tracks active dialogue coroutines
    private Coroutine[] fadeOutCoroutines; // Tracks active fade-out coroutines
    private Dictionary<AudioSource, float> originalVolumes; // Stores original volumes of non-dialogue AudioSources

    void Start()
    {
        // Validate canvas and textContainer
        if (canvas == null)
        {
            Debug.LogError("Canvas is not assigned! Please assign a Canvas in the Inspector.");
            return;
        }
        if (textContainer == null)
        {
            Debug.LogWarning("TextContainer is not assigned! Defaulting to Canvas root.");
            textContainer = canvas.GetComponent<RectTransform>();
        }

        // Initialize arrays
        spawnedTitleTexts = new TextMeshProUGUI[dialogueEntries.Length];
        spawnedDialogueTexts = new TextMeshProUGUI[dialogueEntries.Length];
        isDialogueActive = new bool[dialogueEntries.Length];
        wasPausedByTimeScale = new bool[dialogueEntries.Length];
        dialogueCoroutines = new Coroutine[dialogueEntries.Length];
        fadeOutCoroutines = new Coroutine[dialogueEntries.Length];

        // Initialize volume management
        originalVolumes = new Dictionary<AudioSource, float>();
        AudioSource[] allAudioSources = Resources.FindObjectsOfTypeAll<AudioSource>();
        HashSet<AudioSource> dialogueAudioSources = new HashSet<AudioSource>();
        foreach (var entry in dialogueEntries)
        {
            if (entry.audioSource != null)
                dialogueAudioSources.Add(entry.audioSource);
        }
        foreach (var audioSource in allAudioSources)
        {
            if (!dialogueAudioSources.Contains(audioSource))
            {
                originalVolumes[audioSource] = audioSource.volume;
            }
        }
    }

    void Update()
    {
        if (canvas == null) return;
        bool isGamePaused = Time.timeScale == 0f;
        bool anyDialoguePlaying = false;
        for (int i = 0; i < dialogueEntries.Length; i++)
        {
            if (dialogueEntries[i].audioSource == null || dialogueEntries[i].audioSource.clip == null) continue;
            AudioSource audio = dialogueEntries[i].audioSource;
            bool isPlaying = audio.isPlaying;

            // pause/resume
            if (isPlaying && isGamePaused && !wasPausedByTimeScale[i])
            {
                audio.Pause();
                wasPausedByTimeScale[i] = true;
            }
            else if (!isGamePaused && wasPausedByTimeScale[i])
            {
                audio.UnPause();
                wasPausedByTimeScale[i] = false;
            }
            // Start dialogue
            if (isPlaying && !isDialogueActive[i])
            {
                isDialogueActive[i] = true;
                if (dialogueCoroutines[i] == null)
                {
                    dialogueCoroutines[i] = StartCoroutine(PlayDialogueSequence(i));
                }
            }
            else if (!isPlaying && isDialogueActive[i] && !wasPausedByTimeScale[i])
            {
                if (audio.time >= audio.clip.length - 0.01f || (audio.time == 0f && !audio.isPlaying))
                {
                    isDialogueActive[i] = false;
                    if (fadeOutCoroutines[i] == null)
                    {
                        fadeOutCoroutines[i] = StartCoroutine(FadeOutSequence(i));
                    }
                }
            }
            if (isPlaying)
            {
                anyDialoguePlaying = true;
            }
        }
        // Adjust volumes of other AudioSources
        foreach (var kvp in originalVolumes)
        {
            AudioSource audioSource = kvp.Key;
            float originalVolume = kvp.Value;
            audioSource.volume = anyDialoguePlaying ? originalVolume * otherAudioVolumeScale : originalVolume;
        }
    }

    IEnumerator PlayDialogueSequence(int index)
    {
        if (titleTextPrefab == null || dialogueTextPrefab == null)
        {
            Debug.LogWarning($"Dialogue entry {index} is missing TextMeshProUGUI prefabs!");
            yield break;
        }
        if (spawnedTitleTexts[index] == null)
        {
            spawnedTitleTexts[index] = Instantiate(titleTextPrefab, textContainer);
            spawnedTitleTexts[index].rectTransform.anchoredPosition = titleOffset;
            spawnedTitleTexts[index].text = dialogueEntries[index].titleName;
            spawnedTitleTexts[index].color = new Color(1f, 1f, 1f, 0f);
            spawnedTitleTexts[index].gameObject.SetActive(true);
        }

        if (spawnedDialogueTexts[index] == null)
        {
            spawnedDialogueTexts[index] = Instantiate(dialogueTextPrefab, textContainer);
            spawnedDialogueTexts[index].rectTransform.anchoredPosition = dialogueOffset;
            spawnedDialogueTexts[index].text = "";
            spawnedDialogueTexts[index].color = new Color(1f, 1f, 1f, 0f);
            spawnedDialogueTexts[index].gameObject.SetActive(true);
        }
        yield return StartCoroutine(FadeText(spawnedTitleTexts[index], true, fadeInDuration));
        float charDelay = writingSpeed;
        if (syncWritingWithAudio && dialogueEntries[index].audioSource.clip != null)
        {
            float audioLength = dialogueEntries[index].audioSource.clip.length;
            float remainingTime = audioLength - fadeInDuration;
            charDelay = remainingTime / dialogueEntries[index].dialogueText.Length;
        }
        yield return StartCoroutine(TypewriterEffect(spawnedDialogueTexts[index], dialogueEntries[index].dialogueText, charDelay));

        dialogueCoroutines[index] = null;
    }

    IEnumerator FadeOutSequence(int index)
    {
        if (spawnedTitleTexts[index] == null || spawnedDialogueTexts[index] == null) yield break;

        // Fade out both title and dialogue texts
        yield return StartCoroutine(FadeText(spawnedTitleTexts[index], false, fadeOutDuration));
        yield return StartCoroutine(FadeText(spawnedDialogueTexts[index], false, fadeOutDuration));

        // Destroy texts to save memory
        if (spawnedTitleTexts[index] != null) Destroy(spawnedTitleTexts[index].gameObject);
        if (spawnedDialogueTexts[index] != null) Destroy(spawnedDialogueTexts[index].gameObject);
        spawnedTitleTexts[index] = null;
        spawnedDialogueTexts[index] = null;

        fadeOutCoroutines[index] = null; // Clear coroutine reference
    }

    IEnumerator FadeText(TextMeshProUGUI text, bool fadeIn, float duration)
    {
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; // Use scaled time to pause with Time.timeScale
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            yield return null;
        }
        text.color = new Color(text.color.r, text.color.g, text.color.b, endAlpha);
    }

    IEnumerator TypewriterEffect(TextMeshProUGUI text, string fullText, float charDelay)
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1f); // Ensure visible
        text.text = "";
        foreach (char c in fullText)
        {
            text.text += c;
            yield return new WaitForSeconds(charDelay); // Use scaled time to pause with Time.timeScale
        }
    }

    void OnDestroy()
    {
        // Clean up any remaining spawned texts and coroutines
        for (int i = 0; i < dialogueEntries.Length; i++)
        {
            if (dialogueCoroutines[i] != null) StopCoroutine(dialogueCoroutines[i]);
            if (fadeOutCoroutines[i] != null) StopCoroutine(fadeOutCoroutines[i]);
            if (spawnedTitleTexts[i] != null) Destroy(spawnedTitleTexts[i].gameObject);
            if (spawnedDialogueTexts[i] != null) Destroy(spawnedDialogueTexts[i].gameObject);
        }

        // Restore original volumes
        foreach (var kvp in originalVolumes)
        {
            if (kvp.Key != null)
            {
                kvp.Key.volume = kvp.Value;
            }
        }
    }
}