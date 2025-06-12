using UnityEngine;
using TMPro;

public class PlayerSkill : MonoBehaviour
{
    [Header("Turret Skill Settings")]
    public GameObject turretSkillPrefab; // Assign the turret prefab in the Inspector
    public Transform turretSkillSpawnPoint; // Assign the turret spawn point in the Inspector
    public float turretCooldownDuration = 60f; // Cooldown time in seconds for turret skill

    [Header("Suicide Drone Skill Settings")]
    public GameObject suicideDronePrefab; // Assign the suicide drone prefab in the Inspector
    public Transform suicideDroneSpawnPoint; // Assign the suicide drone spawn point in the Inspector
    public float suicideDroneCooldownDuration = 60f; // Cooldown time in seconds for suicide drone skill

    [Header("UI Settings")]
    public TextMeshProUGUI turretCooldownText; // Assign the TextMeshProUGUI component for turret skill
    public TextMeshProUGUI suicideDroneCooldownText; // Assign the TextMeshProUGUI component for suicide drone skill
    public string turretReadyText = "Turret Skill Ready, Press T"; // Text when turret skill is ready
    public string suicideDroneReadyText = "Suicide Drone Ready, Press F"; // Text when suicide drone skill is ready
    public Color readyColor = Color.green; // Color when skill is ready
    public Color cooldownColor = Color.white; // Color during cooldown

    [Header("Sound Effects")]
    public AudioSource audioSource; // Assign an AudioSource component in the Inspector
    public AudioClip cooldownStartSound; // Sound when cooldown starts
    public AudioClip cooldownEndSound; // Sound when cooldown ends

    private float turretCooldownTimer = 0f;
    private float suicideDroneCooldownTimer = 0f;
    private bool isTurretOnCooldown = false;
    private bool isSuicideDroneOnCooldown = false;

    void Start()
    {
        // Initialize the UI text
        UpdateTurretCooldownUI();
        UpdateSuicideDroneCooldownUI();
    }

    void Update()
    {
        // Handle turret skill cooldown
        if (isTurretOnCooldown)
        {
            turretCooldownTimer -= Time.deltaTime;
            if (turretCooldownTimer <= 0f)
            {
                isTurretOnCooldown = false;
                UpdateTurretCooldownUI();
            }
            else
            {
                UpdateTurretCooldownUI();
            }
        }

        // Handle suicide drone skill cooldown
        if (isSuicideDroneOnCooldown)
        {
            suicideDroneCooldownTimer -= Time.deltaTime;
            if (suicideDroneCooldownTimer <= 0f)
            {
                isSuicideDroneOnCooldown = false;
                UpdateSuicideDroneCooldownUI();
            }
            else
            {
                UpdateSuicideDroneCooldownUI();
            }
        }

        // Activate turret skill when 'T' is pressed and not on cooldown
        if (Input.GetKeyDown(KeyCode.T) && !isTurretOnCooldown)
        {
            ActivateTurretSkill();
        }

        // Activate suicide drone skill when 'F' is pressed and not on cooldown
        if (Input.GetKeyDown(KeyCode.F) && !isSuicideDroneOnCooldown)
        {
            ActivateSuicideDroneSkill();
        }
    }

    void ActivateTurretSkill()
    {
        if (turretSkillPrefab != null && turretSkillSpawnPoint != null)
        {
            GameObject spawnedSkill = Instantiate(turretSkillPrefab, turretSkillSpawnPoint.position, turretSkillSpawnPoint.rotation);
            SkillObject skillObject = spawnedSkill.GetComponent<SkillObject>();
            if (skillObject != null)
            {
                skillObject.enabled = true;
            }
            isTurretOnCooldown = true;
            turretCooldownTimer = turretCooldownDuration;
            UpdateTurretCooldownUI();
            if (audioSource != null && cooldownStartSound != null)
            {
                audioSource.PlayOneShot(cooldownStartSound);
            }
        }
    }

    void ActivateSuicideDroneSkill()
{
    if (suicideDronePrefab != null && suicideDroneSpawnPoint != null)
    {
        GameObject spawnedDrone = Instantiate(suicideDronePrefab, suicideDroneSpawnPoint.position, suicideDroneSpawnPoint.rotation);
        spawnedDrone.SetActive(true);
        SuicideDrone suicideDrone = spawnedDrone.GetComponent<SuicideDrone>();
        if (suicideDrone != null)
        {
            suicideDrone.enabled = true;
        }
        isSuicideDroneOnCooldown = true;
        suicideDroneCooldownTimer = suicideDroneCooldownDuration;
        UpdateSuicideDroneCooldownUI();
        if (audioSource != null && cooldownStartSound != null)
        {
            audioSource.PlayOneShot(cooldownStartSound);
        }
    }
}

    void UpdateTurretCooldownUI()
    {
        if (turretCooldownText != null)
        {
            if (isTurretOnCooldown)
            {
                // Display cooldown time
                turretCooldownText.text = $"Jeda Waktu Menara Senjata: {Mathf.CeilToInt(turretCooldownTimer)} Seconds";
                turretCooldownText.color = cooldownColor;
            }
            else
            {
                // Display ready text
                turretCooldownText.text = turretReadyText;
                turretCooldownText.color = readyColor;

                // Trigger text animation
                StartCoroutine(AnimateText(turretCooldownText));

                // Play cooldown end sound
                if (audioSource != null && cooldownEndSound != null)
                {
                    audioSource.PlayOneShot(cooldownEndSound);
                }
            }
        }
    }

    void UpdateSuicideDroneCooldownUI()
    {
        if (suicideDroneCooldownText != null)
        {
            if (isSuicideDroneOnCooldown)
            {
                // Display cooldown time
                suicideDroneCooldownText.text = $"Jeda Waktu Misil: {Mathf.CeilToInt(suicideDroneCooldownTimer)} Seconds";
                suicideDroneCooldownText.color = cooldownColor;
            }
            else
            {
                // Display ready text
                suicideDroneCooldownText.text = suicideDroneReadyText;
                suicideDroneCooldownText.color = readyColor;

                // Trigger text animation
                StartCoroutine(AnimateText(suicideDroneCooldownText));

                // Play cooldown end sound
                if (audioSource != null && cooldownEndSound != null)
                {
                    audioSource.PlayOneShot(cooldownEndSound);
                }
            }
        }
    }

    private System.Collections.IEnumerator AnimateText(TextMeshProUGUI text)
    {
        // Example animation: Scale up and down
        float duration = 0.5f;
        float elapsedTime = 0f;
        Vector3 originalScale = text.transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        // Fade-in effect
        Color originalColor = text.color;
        text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // Start transparent

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            text.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, elapsedTime / duration); // Fade in
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            text.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / duration);
            yield return null;
        }

        // Color pulse effect
        float pulseDuration = 1f;
        elapsedTime = 0f;
        while (elapsedTime < pulseDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.PingPong(elapsedTime, 0.5f) / 0.5f; // PingPong between 0 and 1
            text.color = Color.Lerp(readyColor, Color.yellow, t); // Pulse between green and yellow
            yield return null;
        }

        // Reset to ready color
        text.color = readyColor;
    }
}