using UnityEngine;

public class SkillObject : MonoBehaviour
{
    [Header("Timer Settings")]
    public float lifetime = 10f; // Time before the object fades and is destroyed

    [Header("Particle Effects")]
    public ParticleSystem startParticleEffect; // Particle effect to spawn when the timer starts
    public ParticleSystem fadeParticleEffect; // Particle effect to spawn before destruction

    private float timer;
    private Renderer objectRenderer;
    private bool isFading = false;

    void Start()
    {
        // Only run this logic if the object is not the prefab itself
        if (gameObject.scene.IsValid()) // Check if the object is in a scene (not a prefab asset)
        {
            timer = lifetime;
            objectRenderer = GetComponent<Renderer>();

            if (objectRenderer == null)
            {
                Debug.LogWarning("No Renderer component found on the skill object.");
            }

            // Spawn the start particle effect when the timer starts
            if (startParticleEffect != null)
            {
                Instantiate(startParticleEffect, transform.position, transform.rotation);
            }
        }
    }

    void Update()
    {
        // Only run this logic if the object is not the prefab itself
        if (gameObject.scene.IsValid())
        {
            // Countdown the timer
            timer -= Time.deltaTime;

            if (timer <= 0f && !isFading)
            {
                StartCoroutine(FadeAndDestroy());
            }
        }
    }

    private System.Collections.IEnumerator FadeAndDestroy()
    {
        isFading = true;
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        Color initialColor = objectRenderer.material.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            objectRenderer.material.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }
        if (fadeParticleEffect != null)
        {
            Instantiate(fadeParticleEffect, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }
}