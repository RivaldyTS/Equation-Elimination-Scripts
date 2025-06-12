using UnityEngine;

public class AimLabTarget : MonoBehaviour
{
    private AimLabSystem aimLabSystem;
    private bool wasHit = false;

    public void Initialize(AimLabSystem system)
    {
        aimLabSystem = system;
        Debug.Log("AimLabTarget initialized with AimLabSystem: " + (aimLabSystem != null));
    }

    public void OnHit()
    {
        if (aimLabSystem != null && !wasHit)
        {
            Debug.Log("Target hit! Object: " + gameObject.name + ", Notifying AimLabSystem.");
            aimLabSystem.RegisterHit();
            wasHit = true;
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Debug.Log("Target destroyed: " + gameObject.name);
        if (aimLabSystem != null && !wasHit)
        {
            Debug.Log("Notifying AimLabSystem of missed target.");
            aimLabSystem.RegisterMissedTarget();
        }
    }
}