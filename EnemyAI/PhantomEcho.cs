using UnityEngine;

public class PhantomEcho : MonoBehaviour
{
    private Transform player;
    private Vector3 targetPosition;
    private float lifetime;
    private float speed = 5f;
    private Vector3 targetOffset;
    [SerializeField] private ParticleSystem spawnEffect;
    private VolumeFader volumeFader;

    void Start()
    {
        volumeFader = Object.FindFirstObjectByType<VolumeFader>(); // Updated from FindObjectOfType
        if (volumeFader == null)
        {
            Debug.LogError("No VolumeFader found in the scene!");
        }
    }

    public void RushPlayer(Transform playerTransform, Vector3 initialTarget, float duration, Vector3 offset)
    {
        player = playerTransform;
        targetPosition = initialTarget;
        lifetime = duration;
        targetOffset = offset;
        Destroy(gameObject, lifetime);

        if (spawnEffect != null)
        {
            ParticleSystem effect = Instantiate(spawnEffect, transform.position, Quaternion.identity);
            if (effect != null)
            {
                effect.Play();
                Debug.Log($"Phantom spawned effect at {transform.position}, duration: {effect.main.duration}");
                Debug.DrawRay(transform.position, Vector3.up * 2f, Color.red, 2f);
                Destroy(effect.gameObject, 1f);
            }
        }
    }

    void Update()
    {
        if (player != null)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Vector3 adjustedTarget = player.position + directionToPlayer * 5f + targetOffset;
            Vector3 direction = (adjustedTarget - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            Vector3 lookDirection = (player.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && volumeFader != null)
        {
            volumeFader.TriggerHorrorEffect();
            Destroy(gameObject);
        }
    }
}