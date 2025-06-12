using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Portal : MonoBehaviour
{
    [Header("Transfer Settings")]
    [SerializeField] private List<GameObject> specificObjects; // List of specific objects to transfer
    [SerializeField] private string transferTag = "Player"; // Tag of objects to transfer
    [SerializeField] private LayerMask transferLayer; // Layer of objects to transfer
    [SerializeField] private Transform destination; // Destination transform for teleportation
    [SerializeField] private List<Transform> randomDestinations; // List of possible destinations for randomization
    [SerializeField] private float cooldown = 2f; // Cooldown period between transfers
    [SerializeField] private float chargingTime = 1.5f; // Delay before teleportation

    [Header("Effects")]
    [SerializeField] private ParticleSystem portalParticles; // Particle effect for the portal
    [SerializeField] private Transform portalParticleSpawnPoint; // Where portal particles spawn
    [SerializeField] private AudioClip portalSound; // Sound effect for the portal
    [SerializeField] private Animator portalAnimator; // Animator for portal animations
    [SerializeField] private ParticleSystem destinationParticles; // Particle effect at the destination
    [SerializeField] private Transform destinationParticleSpawnPoint; // Where destination particles spawn

    [Header("Image Overlay")]
    [SerializeField] private Image imageOverlay; // Image UI for fade effect
    [SerializeField] private float fadeDuration = 0.5f; // Duration of fade in/out

    [Header("Optional Settings")]
    [SerializeField] private bool enableCooldownFeedback = true; // Enable cooldown feedback
    [SerializeField] private ParticleSystem cooldownParticles; // Particle effect for cooldown
    [SerializeField] private Transform cooldownParticleSpawnPoint; // Where cooldown particles spawn
    [SerializeField] private AudioClip cooldownSound; // Sound effect for cooldown
    [SerializeField] private bool preserveVelocity = false; // Preserve velocity for Rigidbody objects

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true; // Enable/disable gizmo visualization
    [SerializeField] private Color gizmoColor = Color.cyan; // Gizmo color
    [SerializeField] private float gizmoRadius = 1f; // Gizmo radius

    private float lastTransferTime; // Track the last transfer time
    private bool isCharging = false; // Track if the portal is charging
    private GameObject objectToTransfer; // Store the object to transfer during charging

    void Start()
    {
        // Initialize the last transfer time
        lastTransferTime = -cooldown;

        // Ensure the image overlay is initially inactive
        if (imageOverlay != null)
        {
            Color transparentColor = imageOverlay.color;
            transparentColor.a = 0;
            imageOverlay.color = transparentColor;
            imageOverlay.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object can be transferred and the portal is not already charging
        if (CanTransfer(other.gameObject) && !isCharging)
        {
            if (Time.time >= lastTransferTime + cooldown)
            {
                // Start the charging process
                isCharging = true;
                objectToTransfer = other.gameObject;
                StartCoroutine(ChargeAndTeleport());
            }
            else if (enableCooldownFeedback)
            {
                PlayCooldownFeedback();
            }
        }
    }

    private bool CanTransfer(GameObject obj)
    {
        // Check if the object is in the specific objects list, has the correct tag, or is on the correct layer
        return specificObjects.Contains(obj) || obj.CompareTag(transferTag) || transferLayer == (transferLayer | (1 << obj.layer));
    }

    private IEnumerator ChargeAndTeleport()
    {
        // Step 1: Portal Charging Up
        Debug.Log("Portal charging up...");
        PlayEffects(); // Play charging effects (particles, sound, etc.)

        // Activate and fade in the image overlay
        if (imageOverlay != null)
        {
            imageOverlay.gameObject.SetActive(true);
            yield return StartCoroutine(FadeImageOverlay(0, 1, fadeDuration)); // Fade in
        }

        // Wait for the charging time
        yield return new WaitForSeconds(chargingTime);

        // Step 2: Teleport the object
        TransferObject(objectToTransfer);

        // Step 3: Post-Teleport Effects
        PlayDestinationEffects();

        // Fade out and deactivate the image overlay
        if (imageOverlay != null)
        {
            yield return StartCoroutine(FadeImageOverlay(1, 0, fadeDuration)); // Fade out
            imageOverlay.gameObject.SetActive(false);
        }

        // Reset charging state
        isCharging = false;
    }

    private void TransferObject(GameObject obj)
    {
        // Teleport the object to the destination
        if (randomDestinations != null && randomDestinations.Count > 0)
        {
            // Randomize the destination if randomDestinations is provided
            Transform randomDestination = randomDestinations[Random.Range(0, randomDestinations.Count)];
            Teleport(obj, randomDestination);
        }
        else if (destination != null)
        {
            // Use the fixed destination if no random destinations are provided
            Teleport(obj, destination);
        }
        else
        {
            Debug.LogWarning("No destination set for the portal!");
            return;
        }

        // Update the last transfer time
        lastTransferTime = Time.time;

        Debug.Log("Transferred: " + obj.name);
    }

    private void Teleport(GameObject obj, Transform destinationTransform)
    {
        // Handle Rigidbody velocity preservation
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        Vector3 velocity = Vector3.zero;
        if (preserveVelocity && rb != null)
        {
            velocity = rb.linearVelocity;
        }

        // Handle player teleportation (if the object has a CharacterController)
        CharacterController characterController = obj.GetComponent<CharacterController>();
        if (characterController != null)
        {
            // Disable the CharacterController temporarily to update the position
            characterController.enabled = false;
            obj.transform.position = destinationTransform.position;
            obj.transform.rotation = destinationTransform.rotation;
            characterController.enabled = true;
        }
        else
        {
            // For non-player objects, update the position and rotation directly
            obj.transform.position = destinationTransform.position;
            obj.transform.rotation = destinationTransform.rotation;
        }

        // Restore Rigidbody velocity if enabled
        if (preserveVelocity && rb != null)
        {
            rb.linearVelocity = velocity;
        }
    }

    private void PlayEffects()
    {
        // Play portal particle effect at the specified spawn point
        if (portalParticles != null && portalParticleSpawnPoint != null)
        {
            ParticleSystem portalParticleInstance = Instantiate(portalParticles, portalParticleSpawnPoint.position, portalParticleSpawnPoint.rotation);
            Destroy(portalParticleInstance.gameObject, portalParticleInstance.main.duration); // Destroy after duration
        }

        // Play sound effect
        if (portalSound != null)
        {
            AudioSource.PlayClipAtPoint(portalSound, transform.position);
        }

        // Trigger portal animation
        if (portalAnimator != null)
        {
            portalAnimator.SetTrigger("Activate");
        }
    }

    private void PlayDestinationEffects()
    {
        // Play destination particle effect at the specified spawn point
        if (destinationParticles != null && destinationParticleSpawnPoint != null)
        {
            ParticleSystem destinationParticleInstance = Instantiate(destinationParticles, destinationParticleSpawnPoint.position, destinationParticleSpawnPoint.rotation);
            Destroy(destinationParticleInstance.gameObject, destinationParticleInstance.main.duration); // Destroy after duration
        }
    }

    private void PlayCooldownFeedback()
    {
        // Play cooldown particle effect at the specified spawn point
        if (cooldownParticles != null && cooldownParticleSpawnPoint != null)
        {
            ParticleSystem cooldownParticleInstance = Instantiate(cooldownParticles, cooldownParticleSpawnPoint.position, cooldownParticleSpawnPoint.rotation);
            Destroy(cooldownParticleInstance.gameObject, cooldownParticleInstance.main.duration); // Destroy after duration
        }

        // Play cooldown sound effect
        if (cooldownSound != null)
        {
            AudioSource.PlayClipAtPoint(cooldownSound, transform.position);
        }

        Debug.Log("Portal is on cooldown!");
    }

    private IEnumerator FadeImageOverlay(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color newColor = imageOverlay.color;
            newColor.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            imageOverlay.color = newColor;
            yield return null;
        }
        // Ensure the final alpha is set
        Color finalColor = imageOverlay.color;
        finalColor.a = endAlpha;
        imageOverlay.color = finalColor;
    }

    // Draw the portal's trigger area in the editor for debugging
    private void OnDrawGizmosSelected()
    {
        if (showGizmos)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        }
    }
}