using UnityEngine;

public class Bullet : MonoBehaviour
{
    public ParticleSystem hitEffect; // Reference to the particle effect prefab

    private void OnTriggerEnter(Collider other) 
    {
        // Check if the collided object has the "Player" tag
        if (other.CompareTag("Player"))
        {
            Debug.Log("Hit Player");
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(2);
            }
        }

        // Spawn the particle effect at the collision point
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, transform.rotation);
        }

        Destroy(gameObject); // Destroy the bullet after triggering
    }
}