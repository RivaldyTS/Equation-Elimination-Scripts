using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using TMPro;
using UnityEngine.Events;

public class PauseMenu : MonoBehaviour
{
    [System.Serializable]
    public struct GuideImage
    {
        public Sprite image; // The image sprite
        public string text;  // The corresponding text for the image
    }

    public GameObject pauseMenuUI; // Pause menu panel
    public GameObject guideMenuUI; // Existing guide menu panel (first guide)
    public GameObject pickSceneMenuUI; // Pick Scene menu panel
    public GameObject videoGuideUI; // Video guide panel
    public GameObject imageGuideUI; // New image guide panel (second guide, parent of image, text, and buttons)
    
    // CanvasGroup for fading ImageGuideUI
    private CanvasGroup imageGuideCanvasGroup;

    // Existing guide (first guide)
    public Image guideImage; // Image component for the first guide
    public Sprite[] guideSprites; // Array of images for the first guide
    private int currentGuideIndex = 0; // Current image index for the first guide

    // New image guide (second guide) - First set of images
    public Image imageGuideImage; // First Image component for the second guide
    public GuideImage[] imageGuideSet1; // Array of GuideImage for the first set (A, B, C)
    private int currentImageGuideIndex = 0; // Current image index for the first set
    public Button nextImageGuideButton; // Next button for the first set
    public Button previousImageGuideButton; // Previous button for the first set
    public Button showFirstSetButton; // Button to show the first set

    // New image guide (second guide) - Second set of images
    public Image imageGuideImage2; // Second Image component for the second guide
    public GuideImage[] imageGuideSet2; // Array of GuideImage for the second set (D, E, F)
    private int currentImageGuideIndex2 = 0; // Current image index for the second set
    public Button nextImageGuideButton2; // Next button for the second set
    public Button previousImageGuideButton2; // Previous button for the second set
    public Button showSecondSetButton; // Button to show the second set

    // TextMeshProUGUI for displaying the text of the current image
    public TextMeshProUGUI imageGuideTextDisplay; // Text component to display the current image's text

    private bool isFirstSetActive = true; // Track which set is currently active

    public bool isPaused = false;

    // Video Player components
    public VideoPlayer videoPlayer; // Video Player component on the videoGuideUI
    public Button videoExitButton; // Exit button for video canvas
    public Button imageGuideExitButton; // Exit button for the second guide canvas
    public VideoClip[] videoGuides; // Array of video clips assignable in Inspector
    public UnityEvent OnVideoPlay; // Event when video starts
    public UnityEvent OnVideoStop; // Event when video stops

    // Editable scaling values
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f); // Scale on hover
    public Vector3 clickScale = new Vector3(0.9f, 0.9f, 0.9f); // Scale on click
    public Vector3 normalScale = new Vector3(1f, 1f, 1f); // Normal scale of the button
    public float animationDuration = 0.2f; // Duration of the animation

    // Fade duration for ImageGuideUI
    public float fadeDuration = 0.5f; // Duration for fade in/out

    // Audio
    public AudioSource menuPopAudio; // Sound when a menu pops out
    public AudioSource buttonClickAudio; // Sound when a button is clicked
    public AudioSource buttonHoverAudio; // Sound when a button is hovered
    public AudioSource sliderHoldAudio; // Sound when the slider knob is held
    public AudioSource sliderReleaseAudio; // Sound when the slider knob is released

    // Volume control
    public Slider volumeSlider; // Slider for adjusting volume
    public TextMeshProUGUI volumeText; // Text to display volume percentage

    private bool isSliderHeld = false; // Track if the slider knob is being held

    void Start()
    {
        // Set the normal scale to the button's initial scale
        normalScale = pauseMenuUI.transform.GetChild(0).localScale; // Assuming the first child is a button

        // Get or add CanvasGroup to ImageGuideUI
        imageGuideCanvasGroup = imageGuideUI.GetComponent<CanvasGroup>();
        if (imageGuideCanvasGroup == null)
        {
            imageGuideCanvasGroup = imageGuideUI.AddComponent<CanvasGroup>();
        }
        imageGuideCanvasGroup.alpha = 0f; // Start with UI invisible

        // Ensure audio plays even when the game is paused
        if (menuPopAudio != null) menuPopAudio.ignoreListenerPause = true;
        if (buttonClickAudio != null) buttonClickAudio.ignoreListenerPause = true;
        if (buttonHoverAudio != null) buttonHoverAudio.ignoreListenerPause = true;
        if (sliderHoldAudio != null) sliderHoldAudio.ignoreListenerPause = true;
        if (sliderReleaseAudio != null) sliderReleaseAudio.ignoreListenerPause = true;

        // Load saved volume settings
        LoadVolume();

        // Set up slider events
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetGlobalVolume);
            volumeSlider.onValueChanged.AddListener(UpdateVolumeText);

            // Add events for slider knob hold and release
            var sliderEventTrigger = volumeSlider.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            // Pointer Down (Hold)
            var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => OnSliderHold());
            sliderEventTrigger.triggers.Add(pointerDown);

            // Pointer Up (Release)
            var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => OnSliderRelease());
            sliderEventTrigger.triggers.Add(pointerUp);
        }

        // Set up video exit button
        if (videoExitButton != null)
        {
            videoExitButton.onClick.AddListener(CloseVideoGuide);
        }

        // Set up image guide exit button
        if (imageGuideExitButton != null)
        {
            imageGuideExitButton.onClick.AddListener(CloseImageGuide);
        }

        // Set up next/previous buttons for the first set
        if (nextImageGuideButton != null)
        {
            nextImageGuideButton.onClick.AddListener(NextImageGuideImage);
        }
        if (previousImageGuideButton != null)
        {
            previousImageGuideButton.onClick.AddListener(PreviousImageGuideImage);
        }

        // Set up next/previous buttons for the second set
        if (nextImageGuideButton2 != null)
        {
            nextImageGuideButton2.onClick.AddListener(NextImageGuideImage2);
        }
        if (previousImageGuideButton2 != null)
        {
            previousImageGuideButton2.onClick.AddListener(PreviousImageGuideImage2);
        }

        // Set up buttons to switch between sets
        if (showFirstSetButton != null)
        {
            showFirstSetButton.onClick.AddListener(ShowFirstSet);
        }
        if (showSecondSetButton != null)
        {
            showSecondSetButton.onClick.AddListener(ShowSecondSet);
        }

        // Ensure video and image guide canvases are initially hidden
        if (videoGuideUI != null)
        {
            videoGuideUI.SetActive(false);
        }
        if (imageGuideUI != null)
        {
            imageGuideUI.SetActive(false);
        }

        // Lock and hide the cursor when the game starts
        LockCursor();
    }

    void OnEnable()
    {
        // Handle cursor visibility when the scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Unsubscribe from the event to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Lock and hide the cursor when a new scene is loaded
        LockCursor();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true); // Show the pause menu
        Time.timeScale = 0f; // Freeze the game
        isPaused = true;
        UnlockCursor(); // Unlock and show the cursor
        if (menuPopAudio != null)
        {
            menuPopAudio.Play();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // Hide pause menu
        guideMenuUI.SetActive(false); // Hide first guide menu (if active)
        pickSceneMenuUI.SetActive(false); // Hide pick scene menu (if active)
        videoGuideUI.SetActive(false); // Hide video guide (if active)
        imageGuideUI.SetActive(false); // Hide second guide menu (if active)
        Time.timeScale = 1f; // Unfreeze the game
        isPaused = false;
        LockCursor(); // hide the cursor
    }

    // Quit the game
    public void QuitGame()
    {
        Application.Quit();
    }

    // Open the first guide menu
    public void OpenGuide()
    {
        pauseMenuUI.SetActive(false); // Hide the pause menu
        guideMenuUI.SetActive(true); // Show the first guide menu
        currentGuideIndex = 0; // Reset to the first image
        UpdateGuideImage(); // Display the first image
        Time.timeScale = 0f; // Freeze the game
        UnlockCursor(); // Show the cursor

        // Play menu pop sound
        if (menuPopAudio != null)
        {
            menuPopAudio.Play();
        }
    }

    // Open the second guide menu (image guide) with the first set active
    public void OpenImageGuide(int imageIndex)
    {
        if (imageGuideUI != null && imageGuideSet1.Length > 0 && imageIndex >= 0 && imageIndex < imageGuideSet1.Length && imageGuideSet2.Length > 0 && imageIndex < imageGuideSet2.Length)
        {
            pauseMenuUI.SetActive(false); // Hide the pause menu
            imageGuideUI.SetActive(true); // Show the second guide menu
            currentImageGuideIndex = imageIndex; // Set the starting index for the first set
            currentImageGuideIndex2 = imageIndex; // Set the starting index for the second set

            // Start with the first set active
            isFirstSetActive = true;
            ShowFirstSet();

            Time.timeScale = 0f; // Freeze the game
            UnlockCursor(); // Show the cursor

            // Fade in the ImageGuideUI
            imageGuideCanvasGroup.alpha = 0f; // Start from invisible
            LeanTween.alphaCanvas(imageGuideCanvasGroup, 1f, fadeDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setIgnoreTimeScale(true); // Ignore Time.timeScale

            // Play menu pop sound
            if (menuPopAudio != null)
            {
                menuPopAudio.Play();
            }
        }
        else
        {
            Debug.LogWarning("Invalid image index or setup incomplete for image guide!");
        }
    }

    // Open the second guide menu with the first set active (callable from outside)
    public void OpenImageGuideFirstSet(int imageIndex)
    {
        if (imageGuideUI != null && imageGuideSet1.Length > 0 && imageIndex >= 0 && imageIndex < imageGuideSet1.Length)
        {
            pauseMenuUI.SetActive(false); // Hide the pause menu
            imageGuideUI.SetActive(true); // Show the second guide menu
            currentImageGuideIndex = imageIndex; // Set the starting index for the first set
            currentImageGuideIndex2 = 0; // Reset the second set index

            // Show the first set
            isFirstSetActive = true;
            ShowFirstSet();

            Time.timeScale = 0f; // Freeze the game
            UnlockCursor(); // Show the cursor

            // Fade in the ImageGuideUI
            imageGuideCanvasGroup.alpha = 0f; // Start from invisible
            LeanTween.alphaCanvas(imageGuideCanvasGroup, 1f, fadeDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setIgnoreTimeScale(true); // Ignore Time.timeScale

            // Play menu pop sound
            if (menuPopAudio != null)
            {
                menuPopAudio.Play();
            }
        }
        else
        {
            Debug.LogWarning("Invalid image index or setup incomplete for first image guide set!");
        }
    }

    // Open the second guide menu with the second set active (callable from outside)
    public void OpenImageGuideSecondSet(int imageIndex)
    {
        if (imageGuideUI != null && imageGuideSet2.Length > 0 && imageIndex >= 0 && imageIndex < imageGuideSet2.Length)
        {
            pauseMenuUI.SetActive(false); // Hide the pause menu
            imageGuideUI.SetActive(true); // Show the second guide menu
            currentImageGuideIndex = 0; // Reset the first set index
            currentImageGuideIndex2 = imageIndex; // Set the starting index for the second set

            // Show the second set
            isFirstSetActive = false;
            ShowSecondSet();

            Time.timeScale = 0f; // Freeze the game
            UnlockCursor(); // Show the cursor

            // Fade in the ImageGuideUI
            imageGuideCanvasGroup.alpha = 0f; // Start from invisible
            LeanTween.alphaCanvas(imageGuideCanvasGroup, 1f, fadeDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setIgnoreTimeScale(true); // Ignore Time.timeScale

            // Play menu pop sound
            if (menuPopAudio != null)
            {
                menuPopAudio.Play();
            }
        }
        else
        {
            Debug.LogWarning("Invalid image index or setup incomplete for second image guide set!");
        }
    }

    // Open the pick scene menu
    public void OpenPickSceneMenu()
    {
        pauseMenuUI.SetActive(false); // Hide the pause menu
        pickSceneMenuUI.SetActive(true); // Show the pick scene menu
        Time.timeScale = 0f; // Freeze the game
        UnlockCursor(); // Show the cursor

        // Play menu pop sound
        if (menuPopAudio != null)
        {
            menuPopAudio.Play();
        }
    }

    // Go back to the pause menu from the guide, image guide, or pick scene menu
    public void BackToPauseMenu()
    {
        guideMenuUI.SetActive(false); // Hide the first guide menu
        if (imageGuideUI.activeSelf)
        {
            // Fade out the ImageGuideUI before hiding
            LeanTween.alphaCanvas(imageGuideCanvasGroup, 0f, fadeDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setIgnoreTimeScale(true)
                .setOnComplete(() =>
                {
                    imageGuideUI.SetActive(false);
                });
        }
        pickSceneMenuUI.SetActive(false); // Hide the pick scene menu
        pauseMenuUI.SetActive(true); // Show the pause menu
        Time.timeScale = 0f; // Ensure the game remains frozen
        UnlockCursor(); // Ensure the cursor is visible for the pause menu

        // Play menu pop sound
        if (menuPopAudio != null)
        {
            menuPopAudio.Play();
        }
    }

    // Show the next image for the first guide
    public void NextGuideImage()
    {
        if (currentGuideIndex < guideSprites.Length - 1)
        {
            currentGuideIndex++;
            UpdateGuideImage();
        }
    }

    // Show the previous image for the first guide
    public void PreviousGuideImage()
    {
        if (currentGuideIndex > 0)
        {
            currentGuideIndex--;
            UpdateGuideImage();
        }
    }

    // Update the image display for the first guide
    private void UpdateGuideImage()
    {
        guideImage.sprite = guideSprites[currentGuideIndex];
    }

    // Show the next image for the second guide (first set: A, B, C)
    public void NextImageGuideImage()
    {
        if (currentImageGuideIndex < imageGuideSet1.Length - 1)
        {
            currentImageGuideIndex++;
            UpdateImageGuideImage();
        }
    }

    // Show the previous image for the second guide (first set: A, B, C)
    public void PreviousImageGuideImage()
    {
        if (currentImageGuideIndex > 0)
        {
            currentImageGuideIndex--;
            UpdateImageGuideImage();
        }
    }

    // Update the image display for the second guide (first set: A, B, C)
    private void UpdateImageGuideImage()
    {
        if (imageGuideImage != null && currentImageGuideIndex < imageGuideSet1.Length)
        {
            imageGuideImage.sprite = imageGuideSet1[currentImageGuideIndex].image;
            // Update the text display
            if (imageGuideTextDisplay != null)
            {
                imageGuideTextDisplay.text = imageGuideSet1[currentImageGuideIndex].text;
            }
        }
    }

    // Show the next image for the second guide (second set: D, E, F)
    public void NextImageGuideImage2()
    {
        if (currentImageGuideIndex2 < imageGuideSet2.Length - 1)
        {
            currentImageGuideIndex2++;
            UpdateImageGuideImage2();
        }
    }

    // Show the previous image for the second guide (second set: D, E, F)
    public void PreviousImageGuideImage2()
    {
        if (currentImageGuideIndex2 > 0)
        {
            currentImageGuideIndex2--;
            UpdateImageGuideImage2();
        }
    }

    // Update the image display for the second guide (second set: D, E, F)
    private void UpdateImageGuideImage2()
    {
        if (imageGuideImage2 != null && currentImageGuideIndex2 < imageGuideSet2.Length)
        {
            imageGuideImage2.sprite = imageGuideSet2[currentImageGuideIndex2].image;
            // Update the text display
            if (imageGuideTextDisplay != null)
            {
                imageGuideTextDisplay.text = imageGuideSet2[currentImageGuideIndex2].text;
            }
        }
    }

    // Show the first set of images (A, B, C) and hide the second set
    public void ShowFirstSet()
    {
        isFirstSetActive = true;
        if (imageGuideImage != null) imageGuideImage.gameObject.SetActive(true);
        if (imageGuideImage2 != null) imageGuideImage2.gameObject.SetActive(false);
        if (nextImageGuideButton != null) nextImageGuideButton.gameObject.SetActive(true);
        if (previousImageGuideButton != null) previousImageGuideButton.gameObject.SetActive(true);
        if (nextImageGuideButton2 != null) nextImageGuideButton2.gameObject.SetActive(false);
        if (previousImageGuideButton2 != null) previousImageGuideButton2.gameObject.SetActive(false);
        UpdateImageGuideImage(); // Ensure the correct image and text are displayed
    }

    // Show the second set of images (D, E, F) and hide the first set
    public void ShowSecondSet()
    {
        isFirstSetActive = false;
        if (imageGuideImage != null) imageGuideImage.gameObject.SetActive(false);
        if (imageGuideImage2 != null) imageGuideImage2.gameObject.SetActive(true);
        if (nextImageGuideButton != null) nextImageGuideButton.gameObject.SetActive(false);
        if (previousImageGuideButton != null) previousImageGuideButton.gameObject.SetActive(false);
        if (nextImageGuideButton2 != null) nextImageGuideButton2.gameObject.SetActive(true);
        if (previousImageGuideButton2 != null) previousImageGuideButton2.gameObject.SetActive(true);
        UpdateImageGuideImage2(); // Ensure the correct image and text are displayed
    }

    // Load a scene by name
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f; // Unfreeze the game
        SceneManager.LoadScene(sceneName); // Load the specified scene
    }

    // Public method to play video guide by index - callable from other scripts
    public void PlayVideoGuide(int videoIndex)
    {
        if (videoGuideUI != null && videoPlayer != null && videoIndex >= 0 && imageGuideSet2.Length > 0 && videoIndex < videoGuides.Length)
        {
            videoGuideUI.SetActive(true);
            Time.timeScale = 0f; // Freeze the game
            UnlockCursor(); // Show cursor
            
            videoPlayer.clip = videoGuides[videoIndex]; // Set the video clip from array
            videoPlayer.Play(); // Start playing the video

            // Play menu pop sound
            if (menuPopAudio != null)
            {
                menuPopAudio.Play();
            }

            OnVideoPlay?.Invoke(); // Trigger the OnVideoPlay event
        }
        else
        {
            Debug.LogWarning("Invalid video index or setup incomplete!");
        }
    }

    // Close the video guide
    private void CloseVideoGuide()
    {
        if (videoGuideUI != null && videoPlayer != null)
        {
            videoPlayer.Stop(); // Stop the video
            videoGuideUI.SetActive(false);
            Time.timeScale = 1f; // Resume the game
            LockCursor(); // Hide cursor

            // Play button click sound
            if (buttonClickAudio != null)
            {
                buttonClickAudio.Play();
            }

            OnVideoStop?.Invoke(); // Trigger the OnVideoStop event
        }
    }

    // Close the second guide (image guide)
    private void CloseImageGuide()
    {
        if (imageGuideUI != null)
        {
            // Fade out the ImageGuideUI before hiding
            LeanTween.alphaCanvas(imageGuideCanvasGroup, 0f, fadeDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setIgnoreTimeScale(true)
                .setOnComplete(() =>
                {
                    imageGuideUI.SetActive(false);
                    Time.timeScale = 1f; // Resume the game
                    LockCursor(); // Hide cursor

                    // Play button click sound
                    if (buttonClickAudio != null)
                    {
                        buttonClickAudio.Play();
                    }
                });
        }
    }

    // Button hover animation
    public void OnButtonHover(Button button)
    {
        LeanTween.scale(button.gameObject, hoverScale, animationDuration)
            .setEase(LeanTweenType.easeOutBack)
            .setIgnoreTimeScale(true); // Ignore Time.timeScale

        // Play button hover sound
        if (buttonHoverAudio != null)
        {
            buttonHoverAudio.Play();
        }
    }

    // Button hover exit animation
    public void OnButtonHoverExit(Button button)
    {
        LeanTween.scale(button.gameObject, normalScale, animationDuration)
            .setEase(LeanTweenType.easeOutBack)
            .setIgnoreTimeScale(true); // Ignore Time.timeScale
    }

    // Button click animation
    public void OnButtonClick(Button button)
    {
        LeanTween.scale(button.gameObject, clickScale, animationDuration * 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setIgnoreTimeScale(true) // Ignore Time.timeScale
            .setOnComplete(() =>
            {
                LeanTween.scale(button.gameObject, normalScale, animationDuration * 0.5f)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setIgnoreTimeScale(true); // Ignore Time.timeScale
            });

        // Play button click sound
        if (buttonClickAudio != null)
        {
            buttonClickAudio.Play();
        }
    }

    // Set global volume
    public void SetGlobalVolume(float volume)
    {
        // Update all AudioSources in the scene using the new method
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource audioSource in allAudioSources)
        {
            audioSource.volume = volume;
        }

        // Save the volume setting
        PlayerPrefs.SetFloat("GlobalVolume", volume);
        PlayerPrefs.Save();
    }

    // Update the volume text
    private void UpdateVolumeText(float volume)
    {
        if (volumeText != null)
        {
            volumeText.text = $"Volume: {Mathf.RoundToInt(volume * 100)}%";
        }
    }

    // Load saved volume settings
    private void LoadVolume()
    {
        // Load the saved volume (default to 0.5 if not set)
        float savedVolume = PlayerPrefs.GetFloat("GlobalVolume", 0.5f);

        // Set the slider value and update the volume
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            SetGlobalVolume(savedVolume);
            UpdateVolumeText(savedVolume);
        }
    }

    // Slider knob held
    private void OnSliderHold()
    {
        isSliderHeld = true;

        // Play slider hold sound
        if (sliderHoldAudio != null)
        {
            sliderHoldAudio.Play();
        }
    }

    // Slider knob released
    private void OnSliderRelease()
    {
        if (isSliderHeld)
        {
            isSliderHeld = false;

            // Play slider release sound
            if (sliderReleaseAudio != null)
            {
                sliderReleaseAudio.Play();
            }
        }
    }

    // Lock and hide the cursor
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
        Cursor.visible = false; // Hide the cursor
    }

    // Unlock and show the cursor
    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true; // Show the cursor
    }
}