using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("Shake Settings")]
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.2f;

    private float shakeTimeRemaining = 0f;
    private Vector3 shakeOffset = Vector3.zero;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (shakeTimeRemaining > 0)
        {
            // Apply random shake offset
            shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeTimeRemaining -= Time.deltaTime;
        }
        else
        {
            // Reset shake offset when done
            shakeOffset = Vector3.zero;
        }
    }

    // Public method to get the current shake offset
    public Vector3 GetShakeOffset()
    {
        return shakeOffset;
    }

    // Trigger the shake
    public void Shake(float duration, float magnitude)
    {
        shakeTimeRemaining = duration;
        shakeMagnitude = magnitude;
    }

    // Overload for default settings
    public void Shake()
    {
        Shake(shakeDuration, shakeMagnitude);
    }
}