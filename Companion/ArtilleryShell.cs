using UnityEngine;

public class ArtilleryShell : MonoBehaviour
{
    [Header("Destruction Settings")]
public float destructionDelay = 0.5f; // Delay before destroying the shell

    [Header("Explosion Settings")]
    public float explosionRadius = 5f; // Radius of the explosion
    public float cameraShakeRadius = 10f; // Radius for camera shake
    public float explosionForce = 10f; // Force applied to nearby objects
    public float damage = 50f; // Damage applied to enemies
    public GameObject[] explosionEffects; // Array of explosion particle effects

    [Header("Camera Shake Settings")]
    public float shakeDuration = 0.5f; // Duration of the camera shake
    public float shakeMagnitude = 0.2f; // Intensity of the camera shake

    [Header("Audio Settings")]
    public AudioSource audioSource; // AudioSource component for playing sounds
    public AudioClip explosionSound; // Sound when the shell explodes

    void Start()
    {
        // Ensure an AudioSource component is assigned or added
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    void Explode()
{
    // Play all explosion effects
    if (explosionEffects != null && explosionEffects.Length > 0)
    {
        foreach (GameObject effect in explosionEffects)
        {
            if (effect != null)
            {
                Instantiate(effect, transform.position, Quaternion.identity);
            }
        }
    }

    // Play explosion sound using the AudioSource
    if (audioSource != null && explosionSound != null)
    {
        audioSource.PlayOneShot(explosionSound);
    }

    // Apply damage or force to nearby objects
    Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
    foreach (Collider collider in colliders)
    {
        // Apply damage to enemies
        Target target = collider.GetComponent<Target>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }

        // Apply explosion force to rigidbodies
        Rigidbody rb = collider.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
        }
    }

    // Check if the player is within the camera shake radius
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player != null)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= cameraShakeRadius)
        {
            // Trigger camera shake
            CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);
        }
    }

    // Disable the shell's renderer and collider to make it "invisible"
    GetComponent<Renderer>().enabled = false;
    GetComponent<Collider>().enabled = false;

    // Add the DelayedDestroy script to the shell and set the delay
    DelayedDestroy destroyScript = gameObject.AddComponent<DelayedDestroy>();
    destroyScript.delay = destructionDelay; // Use the configurable delay
}
}