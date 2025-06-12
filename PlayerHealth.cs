using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    private float health;
    private float lerpTimer;
    private float durationTimer;
    private float timeSinceLastDamage;
    private bool isInvincible = false;
    private bool isLowHealth = false;
    private Vector3 healthBarOriginalPosition;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public bool useHealthRegeneration = false;
    public float regenRate = 1f;
    public float regenDelay = 5f;

    [Header("Health Bar")]
    public float chipSpeed = 2f;
    public Image frontHealthBar;
    public Image backHealthBar;
    public bool useHealthBarSmoothing = false;
    public float backBarDelay = 0.5f;
    public bool useHealthBarShake = false;
    public float shakeIntensity = 5f;
    public float shakeDuration = 0.2f;

    [Header("Damage Overlay")]
    public Image overlay;
    public Color overlayColor = Color.red;
    public float maxOverlayAlpha = 0.5f;
    public float duration = 1f;
    public float fadeSpeed = 1f;

    [Header("Invincibility")]
    public bool useInvincibility = false;
    public float invincibilityDuration = 1f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip damageSound;
    public AudioClip healSound;
    public bool useLowHealthSound = false;
    public AudioClip lowHealthSound;
    public float lowHealthThreshold = 30f;

    [Header("Debugging")]
    public bool debugMode = false;
    public float debugDamageAmount = 10f;
    public float debugHealAmount = 10f;

    [Header("Damage over Time (DoT)")]
    public bool useDoT = false; // Enable/disable DoT
    public string dotTriggerTag = "DoTEnemy"; // Tag of the object that triggers DoT
    public float dotDamagePerSecond = 5f; // Damage per second
    public float dotInterval = 0.5f; // Interval between damage ticks
    public float dotDuration = 3f; // Default duration of the DoT effect
    private bool isTakingDoT = false; // Track if the player is currently taking DoT damage
    public static PlayerHealth Instance; // Singleton pattern for easy access

    [Header("Health Text")]
    public TextMeshProUGUI healthText; // Assign a TextMeshProUGUI component to display health

    // Events
    public static event Action OnPlayerDeath;
    public static event Action<float> OnHealthChanged;
    public static event Action<float> OnDamageTaken;
    public static event Action<float> OnHealthRestored;

    void Start()
{
    Instance = this; // Set the singleton instance
    health = maxHealth;
    overlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0);
    healthBarOriginalPosition = frontHealthBar.rectTransform.localPosition;

    // Initialize health text
    if (healthText != null)
    {
        healthText.text = $"HP: {health}";
    }
}

    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();

        // Damage Overlay
        if (overlay.color.a > 0)
        {
            if (health < lowHealthThreshold)
            {
                float healthFraction = health / lowHealthThreshold;
                overlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, maxOverlayAlpha * (1 - healthFraction));
                return;
            }

            durationTimer += Time.deltaTime;
            if (durationTimer > duration)
            {
                float tempAlpha = overlay.color.a;
                tempAlpha -= Time.deltaTime * fadeSpeed;
                overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, tempAlpha);
            }
        }

        // Health Regeneration
        if (useHealthRegeneration && health < maxHealth && !isInvincible)
        {
            timeSinceLastDamage += Time.deltaTime;
            if (timeSinceLastDamage > regenDelay)
            {
                health += regenRate * Time.deltaTime;
                OnHealthChanged?.Invoke(health);
                UpdateHealthText();
            }
        }

        // Low Health Sound
        if (useLowHealthSound)
        {
            if (health <= lowHealthThreshold && !isLowHealth)
            {
                isLowHealth = true;
                if (audioSource != null && lowHealthSound != null)
                {
                    audioSource.PlayOneShot(lowHealthSound);
                }
            }
            else if (health > lowHealthThreshold && isLowHealth)
            {
                isLowHealth = false;
            }
        }

        // Debugging
        if (debugMode)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                TakeDamage(debugDamageAmount);
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                RestoreHealth(debugHealAmount);
            }
        }
    }

    public void UpdateHealthUI()
    {
        float hfraction = health / maxHealth;
        frontHealthBar.fillAmount = hfraction;

        if (useHealthBarSmoothing)
        {
            if (backHealthBar.fillAmount > hfraction)
            {
                timeSinceLastDamage += Time.deltaTime;
                if (timeSinceLastDamage > backBarDelay)
                {
                    backHealthBar.color = Color.red;
                    lerpTimer += Time.deltaTime;
                    float percentComplete = lerpTimer / chipSpeed;
                    backHealthBar.fillAmount = Mathf.Lerp(backHealthBar.fillAmount, hfraction, percentComplete);
                }
            }
            else if (backHealthBar.fillAmount < hfraction)
            {
                backHealthBar.color = Color.green;
                lerpTimer += Time.deltaTime;
                float percentComplete = lerpTimer / chipSpeed;
                backHealthBar.fillAmount = Mathf.Lerp(backHealthBar.fillAmount, hfraction, percentComplete);
            }
        }
        else
        {
            backHealthBar.fillAmount = hfraction;
        }

        // Update health text
        UpdateHealthText();
    }

    public void TakeDamage(float damage)
{
    if (useInvincibility && isInvincible) return;

    health -= damage;
    OnDamageTaken?.Invoke(damage);
    OnHealthChanged?.Invoke(health);
    UpdateHealthText();

    if (useInvincibility)
    {
        isInvincible = true;
        Invoke(nameof(ResetInvincibility), invincibilityDuration);
    }

    if (useHealthBarShake)
    {
        StartCoroutine(ShakeHealthBar());
    }

    timeSinceLastDamage = 0; // Reset regeneration delay
    durationTimer = 0;
    overlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, maxOverlayAlpha);

    if (health <= 0)
    {
        // Stop DoT if it's active
        if (isTakingDoT)
        {
            StopDoT();
        }

        // Trigger death logic
        OnPlayerDeath?.Invoke();
        ResetGame();
    }
    else
    {
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
    }
}

    public void RestoreHealth(float healAmount)
    {
        health += healAmount;
        OnHealthRestored?.Invoke(healAmount);
        OnHealthChanged?.Invoke(health);
        UpdateHealthText();

        if (audioSource != null && healSound != null)
        {
            audioSource.PlayOneShot(healSound);
        }
    }

    private IEnumerator ShakeHealthBar()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * shakeIntensity;
            float y = UnityEngine.Random.Range(-1f, 1f) * shakeIntensity;
            frontHealthBar.rectTransform.localPosition = healthBarOriginalPosition + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        frontHealthBar.rectTransform.localPosition = healthBarOriginalPosition;
    }

    private void ResetInvincibility()
    {
        isInvincible = false;
    }

    public void ResetGame()
{
    if (Checkpoint.HasCheckpoint())
    {
        // Respawn at the last checkpoint
        Checkpoint.RespawnPlayer();
    }
    else
    {
        // No checkpoint reached, reset the scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}

    // Damage over Time (DoT) Logic
    private void OnCollisionEnter(Collision collision)
    {
        if (useDoT && collision.gameObject.CompareTag(dotTriggerTag))
        {
            StartDoT(dotDuration); // Use default duration
        }
    }

    // Public method to start DoT with optional duration
    public void StartDoT(float duration = 0)
    {
        if (!isTakingDoT)
        {
            StartCoroutine(ApplyDoT(duration));
        }
    }

    // Public method to stop DoT
    public void StopDoT()
    {
        isTakingDoT = false;
        StopAllCoroutines(); // Stop all running DoT coroutines
    }

    private IEnumerator ApplyDoT(float duration)
    {
        isTakingDoT = true;
        float elapsed = 0f;

        while (isTakingDoT && (duration <= 0 || elapsed < duration))
        {
            TakeDamage(dotDamagePerSecond * dotInterval); // Apply damage per interval
            elapsed += dotInterval;
            yield return new WaitForSeconds(dotInterval); // Wait for the next damage tick
        }

        isTakingDoT = false;
    }

    // Update health text
    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {Mathf.Round(health)}"; // Round health for readability
        }
    }
}