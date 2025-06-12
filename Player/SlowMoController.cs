using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class SlowMoController : MonoBehaviour
{
    [Header("Slow Motion Settings")]
    [SerializeField] private float slowMoTimeScale = 0.5f; // Time scale during slow motion
    [SerializeField] private float slowMoDrainRate = 10f; // How fast slow-mo drains
    [SerializeField] private float slowMoRegenRate = 5f; // How fast slow-mo regenerates
    [SerializeField] private float maxSlowMoValue = 100f; // Max slow-mo value
    private float currentSlowMoValue; // Current slow-mo value

    [Header("Input Settings")]
    [SerializeField] private KeyCode slowMoKey = KeyCode.Tab; // Key to activate slow-mo

    [Header("Audio Settings")]
    [SerializeField] private AudioClip slowMoStartSound; // Sound when slow-mo starts
    [SerializeField] private AudioClip slowMoEndSound; // Sound when slow-mo ends
    private AudioSource audioSource;

    [Header("Overlay Settings")]
    [SerializeField] private Image slowMoOverlay; // Image overlay for slow-mo effect
    [SerializeField] private float overlayFadeSpeed = 2f; // Speed of overlay fade in/out
    [SerializeField] private Color overlayColor = new Color(0, 0, 0, 0.5f); // Overlay color

    [Header("Slow Mo Bar Settings")]
    [SerializeField] private Image slowMoBar; // UI Image for slow-mo bar
    [SerializeField] private Color barFullColor = Color.green; // Color when bar is full
    [SerializeField] private Color barEmptyColor = Color.red; // Color when bar is empty

    [Header("Post-Processing Settings")]
    [SerializeField] private float bloomIntensityMultiplier = 2f; // Bloom intensity multiplier during slow-mo
    [SerializeField] private float vignetteIntensityMultiplier = 2f; // Vignette intensity multiplier during slow-mo
    [SerializeField] private float contrastMultiplier = 2f; // Contrast intensity multiplier during slow-mo
    [SerializeField] private float postProcessFadeSpeed = 2f; // Speed of post-processing fade in/out
    private PostProcessVolume globalVolume;
    private Bloom bloomLayer;
    private Vignette vignetteLayer;
    private ColorGrading colorGradingLayer;
    private float defaultBloomIntensity;
    private float defaultVignetteIntensity;
    private float defaultContrast;
    private float targetContrast;
    private float currentContrast;

    private bool isSlowMoActive = false;

    [System.Obsolete]
    private void Start()
    {
        // Initialize slow-mo value
        currentSlowMoValue = maxSlowMoValue;

        // Get or add AudioSource component
        if (!TryGetComponent(out audioSource))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Initialize overlay
        if (slowMoOverlay != null)
        {
            slowMoOverlay.color = Color.clear;
            slowMoOverlay.gameObject.SetActive(false);
        }

        // Initialize slow-mo bar
        if (slowMoBar != null)
        {
            UpdateSlowMoBar();
        }

        // Initialize post-processing effects
        InitializePostProcessing();
    }

    [System.Obsolete]
    private void InitializePostProcessing()
    {
        // Find or create the global volume
        globalVolume = FindObjectOfType<PostProcessVolume>();
        if (globalVolume == null)
        {
            // Create a new global volume if none exists
            GameObject volumeGO = new GameObject("Global Volume");
            globalVolume = volumeGO.AddComponent<PostProcessVolume>();
            globalVolume.isGlobal = true;

            // Create a new profile
            globalVolume.profile = ScriptableObject.CreateInstance<PostProcessProfile>();
        }

        // Ensure Bloom, Vignette, and Color Grading effects exist
        if (!globalVolume.profile.TryGetSettings(out bloomLayer))
        {
            bloomLayer = globalVolume.profile.AddSettings<Bloom>();
            bloomLayer.enabled.Override(true);
        }

        if (!globalVolume.profile.TryGetSettings(out vignetteLayer))
        {
            vignetteLayer = globalVolume.profile.AddSettings<Vignette>();
            vignetteLayer.enabled.Override(true);
        }

        if (!globalVolume.profile.TryGetSettings(out colorGradingLayer))
        {
            colorGradingLayer = globalVolume.profile.AddSettings<ColorGrading>();
            colorGradingLayer.enabled.Override(true);
        }

        // Store default values
        defaultBloomIntensity = bloomLayer.intensity.value;
        defaultVignetteIntensity = vignetteLayer.intensity.value;
        defaultContrast = colorGradingLayer.contrast.value;
        currentContrast = defaultContrast;
        targetContrast = defaultContrast;
    }

    private void Update()
    {
        HandleSlowMoInput();
        UpdateSlowMoValue();
        UpdateOverlay();
        UpdateSlowMoBar();
        UpdatePostProcessing();
    }

    private void HandleSlowMoInput()
    {
        if (Input.GetKeyDown(slowMoKey))
        {
            if (!isSlowMoActive && currentSlowMoValue > 0)
            {
                StartSlowMo();
            }
            else if (isSlowMoActive)
            {
                StopSlowMo();
            }
        }

        // Automatically stop slow-mo if the value drains to 0
        if (isSlowMoActive && currentSlowMoValue <= 0)
        {
            StopSlowMo();
        }
    }

    private void StartSlowMo()
    {
        isSlowMoActive = true;
        Time.timeScale = slowMoTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Play start sound
        if (slowMoStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(slowMoStartSound);
        }

        // Activate and fade in overlay
        if (slowMoOverlay != null)
        {
            slowMoOverlay.gameObject.SetActive(true);
            slowMoOverlay.color = Color.clear;
        }

        // Set target contrast for slow-mo
        targetContrast = defaultContrast * contrastMultiplier;
    }

    private void StopSlowMo()
    {
        isSlowMoActive = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // Play end sound
        if (slowMoEndSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(slowMoEndSound);
        }

        // Fade out and deactivate overlay
        if (slowMoOverlay != null)
        {
            slowMoOverlay.color = overlayColor;
        }

        // Reset target contrast
        targetContrast = defaultContrast;
    }

    private void UpdateSlowMoValue()
    {
        if (isSlowMoActive)
        {
            currentSlowMoValue -= slowMoDrainRate * Time.unscaledDeltaTime;
            currentSlowMoValue = Mathf.Clamp(currentSlowMoValue, 0, maxSlowMoValue);
        }
        else
        {
            currentSlowMoValue += slowMoRegenRate * Time.unscaledDeltaTime;
            currentSlowMoValue = Mathf.Clamp(currentSlowMoValue, 0, maxSlowMoValue);
        }
    }

    private void UpdateOverlay()
    {
        if (slowMoOverlay == null) return;

        if (isSlowMoActive)
        {
            slowMoOverlay.color = Color.Lerp(slowMoOverlay.color, overlayColor, overlayFadeSpeed * Time.unscaledDeltaTime);
        }
        else
        {
            slowMoOverlay.color = Color.Lerp(slowMoOverlay.color, Color.clear, overlayFadeSpeed * Time.unscaledDeltaTime);

            if (slowMoOverlay.color.a <= 0.01f)
            {
                slowMoOverlay.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateSlowMoBar()
    {
        if (slowMoBar == null) return;

        slowMoBar.fillAmount = currentSlowMoValue / maxSlowMoValue;
        slowMoBar.color = Color.Lerp(barEmptyColor, barFullColor, slowMoBar.fillAmount);
    }

    private void UpdatePostProcessing()
    {
        if (globalVolume == null || bloomLayer == null || vignetteLayer == null || colorGradingLayer == null) return;

        // Smoothly interpolate contrast
        currentContrast = Mathf.Lerp(currentContrast, targetContrast, postProcessFadeSpeed * Time.unscaledDeltaTime);
        colorGradingLayer.contrast.value = currentContrast;

        if (isSlowMoActive)
        {
            // Increase bloom intensity and vignette intensity
            bloomLayer.intensity.value = defaultBloomIntensity * bloomIntensityMultiplier;
            vignetteLayer.intensity.value = defaultVignetteIntensity * vignetteIntensityMultiplier;
        }
        else
        {
            // Reset bloom intensity and vignette intensity
            bloomLayer.intensity.value = defaultBloomIntensity;
            vignetteLayer.intensity.value = defaultVignetteIntensity;
        }
    }
}