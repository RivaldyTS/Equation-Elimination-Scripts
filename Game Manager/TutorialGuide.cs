using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using UnityEngine.Events;

public class TutorialGuide : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup overlayGroup;         // The UI overlay group (TutorialCanvas)
    public Image overlayPanel;               // The overlay panel inside the canvas
    public TextMeshProUGUI welcomeTextFirst; // First welcome text
    public TextMeshProUGUI welcomeTextSecond;// Second welcome text
    public TextMeshProUGUI instructionText;  // Text for instructions
    public TextMeshProUGUI notificationText; // Popup text (e.g., "Bagus sekali!")
    public TextMeshProUGUI progressText;     // Progress indicator (e.g., "1/8")
    public Slider progressBar;               // Visual progress bar
    public Image keyIcon;                    // Icon showing the key/mouse to press
    public TextMeshProUGUI skipText;         // Text indicating Enter to skip
    public TextMeshProUGUI completionText;   // "Selesai!" text on completion
    public ParticleSystem completionParticles; // Celebration effect

    [Header("Welcome Settings")]
    [SerializeField] private string[] welcomeMessagesFirst = new string[] {
        "Selamat datang di petualangan baru!",
        "Keren sekali!"
    };
    [SerializeField] private string[] welcomeMessagesSecond = new string[] {
        "Kami akan membantu kamu memulai",
        "Ayo mulai!"
    };
    [SerializeField] private float welcomeTypeSpeed = 0.05f;    // Speed of typewriter effect (seconds per character)
    [SerializeField] private float welcomeDisplayTime = 2f;     // How long both texts stay fully visible
    [SerializeField] private float welcomeFadeOutDuration = 1f; // Duration for fade out

    [Header("Events")]
    public UnityEvent onWelcomeComplete;       // Event triggered after welcome, before tutorial

    [Header("Settings")]
    [SerializeField] private bool skipTutorial = false;         // Option to skip tutorial
    [SerializeField] private float fadeDuration = 0.5f;         // Duration of fade animation
    [SerializeField] private float notificationDuration = 1f;   // How long popup shows
    [SerializeField] private float textPulseSpeed = 5f;         // Speed of text pulse animation
    [SerializeField] private float textPulseAmount = 0.1f;      // Size of text pulse
    [SerializeField] private bool requireMultiplePresses = false; // Toggle for repeat presses (non-WASD)
    [SerializeField, Range(1, 5)] private int pressesRequired = 3; // Number of presses if enabled
    [SerializeField] private float inactivityTimeout = 10f;     // Seconds before reminder
    [SerializeField] private float holdDuration = 1f;           // Time to hold WASD keys
    [SerializeField] private float tutorialTimeScale = 0.5f;    // Game speed during tutorial (half speed)

    [Header("Audio")]
    [SerializeField] private AudioClip successSound;            // Sound for correct key press
    [SerializeField, Range(0f, 1f)] private float soundVolume = 0.8f; // Volume control

    [Header("Visuals")]
    public Sprite[] keySprites;                                 // Sprites for keys/clicks
    [SerializeField] private Color activeKeyColor = Color.green; // Color when key is active
    [SerializeField] private Color inactiveKeyColor = Color.white; // Default key color

    [Header("Player Control")]
    public MonoBehaviour playerController;                      // Player movement script to disable/enable

    [Header("Custom Instructions")]
    [SerializeField] private TutorialStep[] tutorialSteps = new TutorialStep[] {
        new TutorialStep { instruction = "Tahan W untuk jalan ke depan", key = KeyCode.W },
        new TutorialStep { instruction = "Tahan A untuk jalan ke kiri", key = KeyCode.A },
        new TutorialStep { instruction = "Tahan S untuk jalan ke belakang", key = KeyCode.S },
        new TutorialStep { instruction = "Tahan D untuk jalan ke kanan", key = KeyCode.D },
        new TutorialStep { instruction = "Tekan Spasi untuk lompat", key = KeyCode.Space },
        new TutorialStep { instruction = "Tekan Ctrl untuk jongkok", key = KeyCode.LeftControl },
        new TutorialStep { instruction = "Klik kiri untuk menyerang", key = KeyCode.Mouse0 },
        new TutorialStep { instruction = "Klik kanan 2 kali untuk membidik dan melepas", key = KeyCode.Mouse1 }
    };

    [Serializable]
    public class TutorialStep
    {
        public string instruction;
        public KeyCode key;
    }

    private int currentStep = 0; // Tracks current tutorial step
    private int pressCount = 0;  // Tracks presses for multiple-press or right-click mode
    private float holdTimer = 0f; // Tracks how long WASD is held
    private float inactivityTimer; // Tracks time since last input
    private float originalTimeScale; // Store original time scale to restore later
    private string[] encouragementMessages = {
        "Bagus sekali!",
        "Hebat!",
        "Keren!",
        "Pintar!",
        "Luar biasa!"
    };

    void Start()
    {
        if (skipTutorial || tutorialSteps.Length == 0)
        {
            EndTutorial();
            return;
        }

        // Initialize UI and time scale
        originalTimeScale = Time.timeScale;
        Time.timeScale = tutorialTimeScale;
        overlayGroup.alpha = 1f; // Canvas starts fully visible
        overlayPanel.color = new Color(overlayPanel.color.r, overlayPanel.color.g, overlayPanel.color.b, 1f); // Panel fully opaque
        welcomeTextFirst.alpha = 0f; // First welcome text starts invisible
        welcomeTextSecond.alpha = 0f; // Second welcome text starts invisible
        instructionText.alpha = 0f; // Instruction text starts invisible
        notificationText.gameObject.SetActive(false);
        completionText.gameObject.SetActive(false);
        
        if (skipText != null)
        {
            skipText.text = "Tekan Enter untuk skip";
            skipText.gameObject.SetActive(false); // Hidden during welcome
        }

        if (playerController != null) playerController.enabled = false;
        progressBar.maxValue = tutorialSteps.Length;
        progressBar.value = 1;
        progressBar.gameObject.SetActive(false); // Hidden during welcome
        progressText.gameObject.SetActive(false); // Hidden during welcome
        if (keyIcon != null) keyIcon.gameObject.SetActive(false); // Hidden during welcome

        inactivityTimer = inactivityTimeout;
        StartCoroutine(WelcomeSequence());
    }

    IEnumerator WelcomeSequence()
    {
        // Determine the maximum number of pairs to display
        int maxPairs = Mathf.Max(welcomeMessagesFirst.Length, welcomeMessagesSecond.Length);

        for (int i = 0; i < maxPairs; i++)
        {
            // Get messages for this pair (empty string if index exceeds array length)
            string firstMessage = i < welcomeMessagesFirst.Length ? welcomeMessagesFirst[i] : "";
            string secondMessage = i < welcomeMessagesSecond.Length ? welcomeMessagesSecond[i] : "";

            // Typewriter effect for first text
            welcomeTextFirst.text = "";
            foreach (char c in firstMessage)
            {
                welcomeTextFirst.text += c;
                welcomeTextFirst.alpha = 1f; // Fully visible during typing
                yield return new WaitForSeconds(welcomeTypeSpeed);
            }

            // Typewriter effect for second text
            welcomeTextSecond.text = "";
            foreach (char c in secondMessage)
            {
                welcomeTextSecond.text += c;
                welcomeTextSecond.alpha = 1f; // Fully visible during typing
                yield return new WaitForSeconds(welcomeTypeSpeed);
            }

            // Wait with both texts fully visible
            yield return new WaitForSeconds(welcomeDisplayTime);

            // Fade out both texts simultaneously
            float elapsedTime = 0f;
            while (elapsedTime < welcomeFadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / welcomeFadeOutDuration);
                welcomeTextFirst.alpha = alpha;
                welcomeTextSecond.alpha = alpha;
                yield return null;
            }
            welcomeTextFirst.alpha = 0f;
            welcomeTextSecond.alpha = 0f;
        }

        // Reduce overlay panel opacity to 85%
        float fadeTime = 0f;
        while (fadeTime < fadeDuration)
        {
            fadeTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0.85f, fadeTime / fadeDuration);
            overlayPanel.color = new Color(overlayPanel.color.r, overlayPanel.color.g, overlayPanel.color.b, alpha);
            yield return null;
        }
        overlayPanel.color = new Color(overlayPanel.color.r, overlayPanel.color.g, overlayPanel.color.b, 0.85f);

        // Trigger Unity Event
        onWelcomeComplete?.Invoke();

        // Start tutorial
        welcomeTextFirst.gameObject.SetActive(false); // Hide first welcome text
        welcomeTextSecond.gameObject.SetActive(false); // Hide second welcome text
        if (skipText != null) skipText.gameObject.SetActive(true);
        progressBar.gameObject.SetActive(true);
        progressText.gameObject.SetActive(true);
        if (keyIcon != null) keyIcon.gameObject.SetActive(true);
        UpdateInstructionText();
        StartCoroutine(FadeInText()); // Fade in the first instruction
    }

    IEnumerator FadeInText()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            instructionText.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }
        instructionText.alpha = 1f;
    }

    void Update()
    {
        if (skipTutorial || currentStep >= tutorialSteps.Length) return;

        // Pulse animation for instruction text only
        if (instructionText.alpha > 0f)
        {
            float scale = 1f + Mathf.Sin(Time.time * textPulseSpeed) * textPulseAmount;
            instructionText.transform.localScale = new Vector3(scale, scale, 1f);
        }

        // Check for Enter key to skip
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Time.timeScale = originalTimeScale;
            StartCoroutine(EndTutorialWithFade());
            return;
        }

        // Inactivity reminder
        inactivityTimer -= Time.deltaTime;
        if (inactivityTimer <= 0)
        {
            notificationText.text = "Ayo, tekan atau tahan tombolnya!";
            notificationText.gameObject.SetActive(true);
            Invoke("HideNotification", 1f);
            inactivityTimer = inactivityTimeout;
        }

        // Handle input based on current step
        if (currentStep < tutorialSteps.Length)
        {
            if (currentStep < 4) // WASD steps (hold)
            {
                if (Input.GetKey(tutorialSteps[currentStep].key))
                {
                    holdTimer += Time.deltaTime;
                    inactivityTimer = inactivityTimeout;
                    notificationText.text = $"Tahan {holdDuration - holdTimer:F1} detik lagi!";
                    notificationText.gameObject.SetActive(true);

                    if (holdTimer >= holdDuration)
                    {
                        StartCoroutine(ShowNotification());
                        holdTimer = 0f;
                        MoveToNextStep();
                    }
                }
                else
                {
                    holdTimer = Mathf.Max(0f, holdTimer - Time.deltaTime);
                    HideNotification();
                }
            }
            else if (currentStep == tutorialSteps.Length - 1) // Right-click step (2 clicks)
            {
                if (Input.GetKeyDown(tutorialSteps[currentStep].key))
                {
                    inactivityTimer = inactivityTimeout;
                    pressCount++;
                    if (pressCount == 1)
                    {
                        notificationText.text = "Sekarang klik kanan lagi untuk melepas!";
                        notificationText.gameObject.SetActive(true);
                        Invoke("HideNotification", 1f);
                    }
                    else if (pressCount >= 2)
                    {
                        StartCoroutine(ShowNotification());
                        pressCount = 0;
                        MoveToNextStep();
                    }
                }
            }
            else // Regular key steps (Space, Ctrl, Left Click)
            {
                if (Input.GetKeyDown(tutorialSteps[currentStep].key))
                {
                    inactivityTimer = inactivityTimeout;
                    if (requireMultiplePresses)
                    {
                        pressCount++;
                        if (pressCount >= pressesRequired)
                        {
                            StartCoroutine(ShowNotification());
                            pressCount = 0;
                            MoveToNextStep();
                        }
                        else
                        {
                            notificationText.text = $"{pressesRequired - pressCount} lagi!";
                            notificationText.gameObject.SetActive(true);
                            Invoke("HideNotification", 0.5f);
                        }
                    }
                    else
                    {
                        StartCoroutine(ShowNotification());
                        MoveToNextStep();
                    }
                }
            }
        }
    }

    void MoveToNextStep()
    {
        currentStep++;
        if (currentStep < tutorialSteps.Length)
        {
            UpdateInstructionText();
            StartCoroutine(FadeInText());
        }
        else
        {
            Time.timeScale = originalTimeScale;
            StartCoroutine(EndTutorialWithFade());
        }
    }

    void UpdateInstructionText()
    {
        instructionText.text = tutorialSteps[currentStep].instruction;
        instructionText.alpha = 0f; // Reset alpha for fade in
        progressText.text = $"{currentStep + 1}/{tutorialSteps.Length}";
        progressBar.value = currentStep + 1;
        if (keyIcon != null && keySprites.Length > currentStep)
        {
            keyIcon.sprite = keySprites[currentStep];
            keyIcon.color = activeKeyColor;
        }
    }

    IEnumerator ShowNotification()
    {
        notificationText.text = encouragementMessages[UnityEngine.Random.Range(0, encouragementMessages.Length)];
        notificationText.gameObject.SetActive(true);

        if (successSound != null)
        {
            GameObject tempAudio = new GameObject("TempAudio");
            tempAudio.transform.SetParent(transform);
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = successSound;
            tempSource.volume = soundVolume;
            tempSource.Play();
            Destroy(tempAudio, successSound.length);
        }

        yield return new WaitForSeconds(notificationDuration);
        notificationText.gameObject.SetActive(false);
    }

    void HideNotification()
    {
        notificationText.gameObject.SetActive(false);
    }

    IEnumerator EndTutorialWithFade()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            overlayGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }
        StartCoroutine(ShowCompletion());
    }

    IEnumerator ShowCompletion()
    {
        completionText.text = "Selesai! Ayo main!";
        completionText.gameObject.SetActive(true);
        if (completionParticles != null) completionParticles.Play();
        yield return new WaitForSeconds(2f);
        EndTutorial();
    }

    void EndTutorial()
    {
        overlayGroup.gameObject.SetActive(false);
        completionText.gameObject.SetActive(false);
        if (playerController != null) playerController.enabled = true;
        if (keyIcon != null) keyIcon.color = inactiveKeyColor;
        if (skipText != null) skipText.gameObject.SetActive(false);
    }
}