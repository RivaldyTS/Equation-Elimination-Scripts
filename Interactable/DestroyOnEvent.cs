using UnityEngine;

public class DestroyOnEvent : MonoBehaviour
{
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
