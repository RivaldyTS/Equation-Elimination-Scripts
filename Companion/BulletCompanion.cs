using System.Collections;
using UnityEngine;

public class BulletCompanion : MonoBehaviour
{
    public float speed = 10f; // Speed of the bullet
    public float damage = 10f; // Damage dealt by the bullet
    public float lifetime = 3f; // Lifetime of the bullet
    public ParticleSystem hitEffect; // Particle effect to spawn on hit
    private Vector3 direction; // Direction to move in

    void Start()
    {
        Destroy(gameObject, lifetime); // Destroy the bullet after its lifetime expires
    }

    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection.normalized; // Set the direction to move
    }

    void Update()
    {
        // Move the bullet in the set direction
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Target"))
        {
            Target target = other.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
            if (other.CompareTag("Target"))
            {
                AimLabTarget aimLabTarget = other.GetComponent<AimLabTarget>();
                if (aimLabTarget != null)
                {
                    aimLabTarget.OnHit();
                }
            }
            ActivateOutline(other.gameObject);
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }

    void ActivateOutline(GameObject target)
    {
        Outline outline = target.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = true;
            outline.OutlineColor = Color.red;
            outline.OutlineWidth = 5f;
            StartCoroutine(DisableOutlineAfterDelay(outline, 2f));
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
}
}