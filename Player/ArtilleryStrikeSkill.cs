using UnityEngine;
using TMPro;

public class ArtilleryStrikeSkill : MonoBehaviour
{
    [Header("Artillery Strike Settings")]
    public GameObject artilleryShellPrefab; // Assign the artillery shell prefab in the Inspector
    public int numberOfShells = 10; // Number of shells to rain down
    public float shellInterval = 0.5f; // Time between each shell
    public float strikeDelay = 2f; // Delay before shells start raining down
    public float cooldownDuration = 30f; // Cooldown time in seconds
    public float aoeRadius = 10f; // Radius of the artillery strike area
    public float shellSpawnHeight = 50f; // Height at which shells are spawned
    public LayerMask groundLayer; // Layer mask for detecting where to strike

    [Header("Skill Button")]
    public KeyCode skillButton = KeyCode.G; // Customizable skill activation key

    [Header("UI Settings")]
    public TextMeshProUGUI cooldownText; // Assign the TextMeshProUGUI component for the cooldown UI
    public string readyText = "Artillery Strike Ready, Press G"; // Text when skill is ready
    public Color readyColor = Color.green; // Color when skill is ready
    public Color cooldownColor = Color.white; // Color during cooldown

    [Header("Sound Effects")]
    public AudioSource audioSource; // Assign an AudioSource component in the Inspector
    public AudioClip strikeRequestSound; // Sound when skill is cast ("Requesting Air Strike")
    public AudioClip strikeInboundSound; // Sound when strike delay is done ("Air Strike Inbound")
    public AudioClip cooldownStartSound; // Sound when cooldown starts
    public AudioClip cooldownEndSound; // Sound when cooldown ends

    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;

    void Start()
    {
        // Initialize the cooldown text immediately
        UpdateCooldownUI();
    }

    void Update()
    {
        // Handle cooldown
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                UpdateCooldownUI();
            }
            else
            {
                UpdateCooldownUI();
            }
        }

        // Activate artillery strike when the skill button is pressed and not on cooldown
        if (Input.GetKeyDown(skillButton) && !isOnCooldown)
        {
            MarkLocationForStrike();
        }
    }

    void MarkLocationForStrike()
    {
        // Raycast to find the target location
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // Play the "Requesting Air Strike" sound
            PlaySound(strikeRequestSound);

            // Start the artillery strike at the hit point with AoE
            StartCoroutine(ArtilleryStrike(hit.point));

            // Start cooldown
            isOnCooldown = true;
            cooldownTimer = cooldownDuration;
            UpdateCooldownUI();

            // Play cooldown start sound
            PlaySound(cooldownStartSound);
        }
        else
        {
            Debug.LogWarning("No valid ground location found for artillery strike.");
        }
    }

    private System.Collections.IEnumerator ArtilleryStrike(Vector3 targetPosition)
    {
        // Wait for the strike delay
        yield return new WaitForSeconds(strikeDelay);

        // Play the "Air Strike Inbound" sound
        PlaySound(strikeInboundSound);

        // Rain down shells within the AoE radius
        for (int i = 0; i < numberOfShells; i++)
        {
            // Calculate a random position within the AoE radius
            Vector2 randomCircle = Random.insideUnitCircle * aoeRadius;
            Vector3 shellPosition = targetPosition + new Vector3(randomCircle.x, shellSpawnHeight, randomCircle.y);

            // Spawn the shell
            GameObject shell = Instantiate(artilleryShellPrefab, shellPosition, Quaternion.identity);

            // Add slight horizontal movement to the shell
            Rigidbody shellRb = shell.GetComponent<Rigidbody>();
            if (shellRb != null)
            {
                Vector3 drift = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
                shellRb.linearVelocity = Vector3.down * 20f + drift; // Add downward velocity and slight drift
            }

            // Wait for the next shell
            yield return new WaitForSeconds(shellInterval);
        }
    }

    void UpdateCooldownUI()
    {
        if (cooldownText != null)
        {
            if (isOnCooldown)
            {
                // Display cooldown time
                cooldownText.text = $"Jeda Waktu Serangan Artileri: {Mathf.CeilToInt(cooldownTimer)} Seconds";
                cooldownText.color = cooldownColor;
            }
            else
            {
                // Display ready text
                cooldownText.text = readyText;
                cooldownText.color = readyColor;

                // Trigger text animation
                StartCoroutine(AnimateText(cooldownText));

                // Play cooldown end sound
                PlaySound(cooldownEndSound);
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

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Draw Gizmos in the Scene view
    private void OnDrawGizmosSelected()
    {
        // Set the color of the Gizmo
        Gizmos.color = Color.red;

        // Draw a wireframe sphere to represent the AoE radius
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}