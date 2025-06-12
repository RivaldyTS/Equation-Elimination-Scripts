using UnityEngine;

public class DelayedDestroy : MonoBehaviour
{
    public float delay = 0.5f; // Time delay before destruction

    void Start()
    {
        // Schedule the destruction of this object after the specified delay
        Destroy(gameObject, delay);
    }
}