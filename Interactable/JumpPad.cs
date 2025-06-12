using System.Collections;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [Header("Jump Pad Settings")]
    [SerializeField] private float playerLaunchForce = 15f; // Force applied to launch the player
    [SerializeField] private float objectLaunchForce = 10f; // Force applied to launch other objects
    [SerializeField] private Vector3 launchDirection = Vector3.up; // Direction of the launch
    [SerializeField] private float cooldown = 1f; // Cooldown period between launches
    [SerializeField] private bool randomizeForce = false; // Randomize the launch force
    [SerializeField] private float minForce = 5f; // Minimum launch force (if randomizeForce is true)
    [SerializeField] private float maxForce = 20f; // Maximum launch force (if randomizeForce is true)
    [SerializeField] private LayerMask launchLayer; // Layer of objects that can be launched
    [SerializeField] private string launchTag = "Player"; // Tag of objects that can be launched

    [Header("Effects")]
    [SerializeField] private ParticleSystem jumpPadParticles; // Particle effect for the jump pad
    [SerializeField] private AudioClip jumpPadSound; // Sound effect for the jump pad
    [SerializeField] private Animator jumpPadAnimator; // Animator for jump pad animations

    private float lastLaunchTime; // Track the last launch time

    void Start()
    {
        // Initialize the last launch time
        lastLaunchTime = -cooldown;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered with: " + other.gameObject.name);

        // Check if the object can be launched
        if (CanLaunch(other.gameObject) && Time.time >= lastLaunchTime + cooldown)
        {
            Debug.Log("Object can be launched: " + other.gameObject.name);
            LaunchObject(other.gameObject);
        }
        else
        {
            Debug.Log("Object cannot be launched or cooldown is active.");
        }
    }

    private bool CanLaunch(GameObject obj)
    {
        // Check if the object is on the correct layer or has the correct tag
        bool canLaunch = launchLayer == (launchLayer | (1 << obj.layer)) || obj.CompareTag(launchTag);
        Debug.Log("Can launch " + obj.name + "? " + canLaunch);
        return canLaunch;
    }

    private void LaunchObject(GameObject obj)
    {
        Debug.Log("Attempting to launch: " + obj.name);

        // Play jump pad effects
        PlayEffects();

        // Calculate the launch force
        float force = GetLaunchForce(obj);
        Debug.Log("Launch force: " + force);

        // Apply the launch force to the object
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        CharacterController characterController = obj.GetComponent<CharacterController>();

        if (rb != null)
        {
            Debug.Log("Rigidbody found. Applying force.");

            // Temporarily disable the CharacterController (if it exists)
            if (characterController != null)
            {
                Debug.Log("CharacterController found. Disabling it temporarily.");
                characterController.enabled = false;
            }

            // Apply the launch force
            rb.linearVelocity = Vector3.zero; // Reset the object's velocity
            rb.AddForce(launchDirection.normalized * force, ForceMode.Impulse);

            // Re-enable the CharacterController after a short delay
            if (characterController != null)
            {
                StartCoroutine(ReenableCharacterController(characterController));
            }
        }
        else
        {
            Debug.LogWarning("No Rigidbody found on " + obj.name);
        }

        // Update the last launch time
        lastLaunchTime = Time.time;

        Debug.Log("Launched: " + obj.name + " with force: " + force);
    }

    private float GetLaunchForce(GameObject obj)
    {
        // Use playerLaunchForce if the object has the launchTag, otherwise use objectLaunchForce
        if (obj.CompareTag(launchTag))
        {
            return randomizeForce ? Random.Range(minForce, maxForce) : playerLaunchForce;
        }
        else
        {
            return randomizeForce ? Random.Range(minForce, maxForce) : objectLaunchForce;
        }
    }

    private IEnumerator ReenableCharacterController(CharacterController characterController)
    {
        // Wait for a short delay before re-enabling the CharacterController
        yield return new WaitForSeconds(0.5f); // Adjust the delay as needed
        characterController.enabled = true;
        Debug.Log("CharacterController re-enabled.");
    }

    private void PlayEffects()
    {
        // Play particle effect
        if (jumpPadParticles != null)
        {
            Debug.Log("Playing particle effect.");
            jumpPadParticles.Play();
        }

        // Play sound effect
        if (jumpPadSound != null)
        {
            Debug.Log("Playing sound effect.");
            AudioSource.PlayClipAtPoint(jumpPadSound, transform.position);
        }

        // Trigger jump pad animation
        if (jumpPadAnimator != null)
        {
            Debug.Log("Triggering jump pad animation.");
            jumpPadAnimator.SetTrigger("Activate");
        }
    }

    // Optional: Draw the launch direction in the editor for debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + launchDirection.normalized * 2f); // Adjust length as needed
    }
}