using UnityEngine;
using TMPro;
using System.Collections;

public class CompanionRobot : MonoBehaviour
{
    public float detectionRange = 10f; // Range to detect enemies
    public GameObject bulletPrefab; // Prefab for the bullet
    public Transform firePoint1; // First fire point
    public Transform firePoint2; // Second fire point
    public float fireRate = 1f; // Rate of fire
    private float nextFireTime = 0f;

    // Hover effect settings
    public Vector3 positionOffset = new Vector3(1f, 0.5f, 1f); // Offset from the camera
    public float hoverSpeed = 2f; // Speed of the hover effect
    public float hoverHeight = 0.1f; // Height of the hover effect

    // Audio settings
    public AudioSource audioSource; // Reference to the AudioSource component

    // Visual effects
    public ParticleSystem muzzleflashPrefab; // Prefab for the muzzle flash ParticleSystem
    public Transform muzzleTransform1; // Transform for the first muzzle flash position
    public Transform muzzleTransform2; // Transform for the second muzzle flash position

    // Dialogue system
    //public TextMeshProUGUI dialogueText; // Reference to the UI text for dialogue
    //public string safeMessage = "Area is safe."; // Message when no enemies are detected
    //public string enemyDetectedMessage = "Enemy detected, at {0} o'clock."; // Message when an enemy is detected
    //public float dialogueDelay = 5f; // Delay before updating the dialogue text (configurable in Inspector)

    // Debug settings
    public bool showDebugRay = false; // Toggle for debug ray visualization

    // Target locking
    private Transform lockedEnemy; // The currently locked enemy

    // Rotation settings
    public float rotationSpeed = 5f; // Speed of rotating towards the enemy (configurable in Inspector)

    // Toggle for alternating fire points
    private bool useFirstFirePoint = true; // Start with the first fire point

    void Update()
    {
        ApplyHoverEffect();

        // If no enemy is locked or the locked enemy is no longer valid, find a new target
        if (lockedEnemy == null || !IsEnemyVisible(lockedEnemy))
        {
            FindAndLockClosestEnemy();
        }

        // If an enemy is locked, rotate and shoot at it
        if (lockedEnemy != null)
        {
            RotateAndShootAtLockedEnemy();
        }
        //else
        //{
            // If no enemies are detected, display the safe message
            //dialogueText.text = safeMessage;
        //}
    }

    void ApplyHoverEffect()
    {
        // Calculate the hover effect
        float hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;

        // Apply the position offset and hover effect
        transform.localPosition = positionOffset + new Vector3(0, hoverOffset, 0);
    }

    void FindAndLockClosestEnemy()
    {
        // Find all enemies within detection range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange);
        Transform closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                // Check if the enemy is visible
                if (IsEnemyVisible(hitCollider.transform))
                {
                    // Calculate the distance to the enemy
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);

                    // Check if this enemy is closer than the previous closest
                    if (distance < closestDistance)
                    {
                        closestEnemy = hitCollider.transform;
                        closestDistance = distance;
                    }
                }
            }
        }

        // Lock onto the closest enemy
        lockedEnemy = closestEnemy;
    }

    void RotateAndShootAtLockedEnemy()
    {
        // Rotate to face the locked enemy
        Vector3 directionToEnemy = (lockedEnemy.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Shoot at the locked enemy
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;

            // Update dialogue text after shooting
            //StartCoroutine(UpdateDialogueWithDelay(lockedEnemy));
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
                return true; // Enemy is visible
            }
        }

        return false; // Enemy is blocked by an obstacle
    }

    void Shoot()
    {
        if (bulletPrefab && firePoint1 && firePoint2)
        {
            // Determine which fire point to use
            Transform currentFirePoint = useFirstFirePoint ? firePoint1 : firePoint2;
            Transform currentMuzzleTransform = useFirstFirePoint ? muzzleTransform1 : muzzleTransform2;
            // Instantiate the bullet at the current fire point
            GameObject bullet = Instantiate(bulletPrefab, currentFirePoint.position, currentFirePoint.rotation);
            BulletCompanion bulletScript = bullet.GetComponent<BulletCompanion>();
            if (bulletScript)
            {
                bulletScript.SetDirection(transform.forward);
            }
            // Play shooting sound
            if (audioSource && audioSource.clip)
            {
                audioSource.Play();
            }
            // Instantiate and play the muzzle flash at the current muzzle transform
            if (muzzleflashPrefab != null && currentMuzzleTransform != null)
            {
                ParticleSystem muzzleflash = Instantiate
                (muzzleflashPrefab, currentMuzzleTransform.position, currentMuzzleTransform.rotation);
                muzzleflash.Play();
                Destroy(muzzleflash.gameObject, muzzleflash.main.duration);
            }
            // Toggle the fire point for the next shot
            useFirstFirePoint = !useFirstFirePoint;
        }
    }

    //IEnumerator UpdateDialogueWithDelay(Transform enemy)
    //{
        // Calculate the enemy's position relative to the player
        //Vector3 directionToEnemy = enemy.position - transform.position;
        //float angle = Vector3.SignedAngle(transform.forward, directionToEnemy, Vector3.up);

        // Convert the angle to a clock position (e.g., 12 o'clock, 3 o'clock)
        //int clockPosition = Mathf.RoundToInt((angle + 180) / 30) % 12 + 1;

        // Update the dialogue text
        //dialogueText.text = string.Format(enemyDetectedMessage, clockPosition);

        // Wait for the specified delay
        //yield return new WaitForSeconds(dialogueDelay);

        // Reset the dialogue text to the safe message
        //dialogueText.text = safeMessage;
    //}
}