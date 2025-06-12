using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuicideDrone : MonoBehaviour
{
    [Header("Drone Settings")]
    public float normalSpeed = 10f; // Normal speed of the drone
    public float floatingSpeed = 2f; // Floating speed of the drone
    public float damage = 10f; // Damage dealt by the drone
    public float curveIntensity = 1f; // How much the drone curves towards the target
    public float detectionRange = 50f; // Range to detect enemies
    public float lifetime = 10f; // Lifetime of the drone (to prevent it from existing forever)

    [Header("Explosion Settings")]
    public float explosionRange = 5f; // Range of the explosion
    public float explosionDamage = 50f; // Damage dealt by the explosion
    public List<GameObject> explosionPrefabs; // List of explosion prefabs to spawn

    [Header("Fire Particle Effect")]
    public ParticleSystem fireParticleEffect; // ParticleSystem for the fire effect
    public Transform fireParticlePosition; // Empty GameObject for fire particle position and rotation
    public float fireParticleInterval = 0.05f; // Interval for playing the fire particle effect
    private float fireParticleTimer = 0f;

    [Header("Outline Settings")]
    public Color outlineColor = Color.red; // Color of the outline effect
    public float outlineWidth = 5f; // Width of the outline effect
    public float outlineDuration = 2f; // Duration of the outline effect

    [Header("Target Locking")]
    public float rotationSpeed = 5f; // Speed of rotating towards the enemy
    public bool showDebugRay = false; // Toggle for debug ray visualization

    [Header("Audio Settings")]
    public AudioSource audioSource; // AudioSource component for playing sounds
    public AudioClip activationSound; // Sound played when the drone is activated
    public AudioClip normalSpeedSound; // Sound played when the drone switches to normal speed
    public AudioClip explosionSound; // Sound played when the drone explodes

    private Transform target; // Target enemy
    private Rigidbody rb;
    private bool isEnemyVisible = false; // Whether an enemy is visible
    private bool isSpeedNormal = false; // Whether the drone is at normal speed
    private bool hasPlayedNormalSpeedSound = false; // Whether the normal speed sound has been played

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Disable "Is Kinematic" when the script starts
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        // Play activation sound
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }

        Destroy(gameObject, lifetime); // Destroy the drone after its lifetime expires

        // Initialize the fire particle effect
        if (fireParticleEffect != null)
        {
            fireParticleEffect.Stop(); // Ensure the effect is stopped initially
        }
    }

    void Update()
    {
        // If no target is locked or the target is no longer visible, find a new target
        if (target == null || !IsEnemyVisible(target))
        {
            FindAndLockClosestEnemy();
        }

        // Move the drone based on whether an enemy is visible
        MoveDrone();

        // Handle the fire particle effect
        HandleFireParticleEffect();
    }

    void FindAndLockClosestEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange);
        Transform closestEnemy = null;
        float closestDistance = float.MaxValue;
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                if (IsEnemyVisible(hitCollider.transform))
                {
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestEnemy = hitCollider.transform;
                        closestDistance = distance;
                    }
                }
            }
        }

        // Lock onto the closest enemy
        target = closestEnemy;

        // If an enemy is found, set speed to normal
        if (target != null && !isSpeedNormal)
        {
            isSpeedNormal = true;

            // Stop activation sound and play normal speed sound
            if (audioSource != null)
            {
                audioSource.Stop(); // Stop the activation sound
                if (normalSpeedSound != null && !hasPlayedNormalSpeedSound)
                {
                    audioSource.PlayOneShot(normalSpeedSound);
                    hasPlayedNormalSpeedSound = true; // Ensure the sound is only played once
                }
            }
        }
    }

    void MoveDrone()
    {
        if (isSpeedNormal)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            Vector3 curveDirection = Vector3.Slerp(transform.forward, directionToTarget, curveIntensity * Time.deltaTime);
            rb.linearVelocity = curveDirection * normalSpeed;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            rb.linearVelocity = transform.forward * floatingSpeed;
        }
    }

    bool IsEnemyVisible(Transform enemy)
    {
        // Calculate the direction to the enemy
        Vector3 directionToEnemy = (enemy.position - transform.position).normalized;

        // Perform a raycast to check for obstacles
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToEnemy, out hit, detectionRange))
        {
            // Debug visualization
            if (showDebugRay)
            {
                Debug.DrawRay(transform.position, directionToEnemy * detectionRange, Color.red);
            }

            // Check if the raycast hit the enemy
            if (hit.collider.CompareTag("Enemy"))
            {
                isEnemyVisible = true; // Enemy is visible
                return true;
            }
        }

        isEnemyVisible = false; // Enemy is blocked by an obstacle
        return false;
    }

    void HandleFireParticleEffect()
    {
        if (fireParticleEffect != null && fireParticlePosition != null)
        {
            fireParticleTimer += Time.deltaTime;
            if (fireParticleTimer >= fireParticleInterval)
            {
                // Position and rotate the fire particle effect
                fireParticleEffect.transform.position = fireParticlePosition.position;
                fireParticleEffect.transform.rotation = fireParticlePosition.rotation;

                // Play the fire particle effect
                fireParticleEffect.Play();

                // Reset the timer
                fireParticleTimer = 0f;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        TriggerExplosion();
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // Play explosion sound
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
        Destroy(gameObject, explosionSound != null ? explosionSound.length : 1f);
    }

    void TriggerExplosion()
    {
        if (explosionPrefabs != null && explosionPrefabs.Count > 0)
        {
            foreach (var prefab in explosionPrefabs)
            {
                if (prefab != null)
                {
                    Instantiate(prefab, transform.position, Quaternion.identity);
                }
            }
        }
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                Target target = hitCollider.GetComponent<Target>();
                if (target != null)
                {
                    target.TakeDamage(explosionDamage);
                }
            }
        }
    }

    void ActivateOutline(GameObject enemy)
    {
        // Get the Outline component from the enemy
        Outline outline = enemy.GetComponent<Outline>();
        if (outline != null)
        {
            // Enable the outline (if it's disabled)
            outline.enabled = true;

            // Set outline properties
            outline.OutlineColor = outlineColor;
            outline.OutlineWidth = outlineWidth;

            // Disable the outline after a delay
            StartCoroutine(DisableOutlineAfterDelay(outline, outlineDuration));
        }
    }

    IEnumerator DisableOutlineAfterDelay(Outline outline, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Disable the outline after the delay
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw the explosion range in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}