using UnityEngine;
using UnityEngine.Events;

public class OnActiveEventTrigger : MonoBehaviour
{
    public GameObject targetObject; // Assign the GameObject in the Inspector
    public UnityEvent onObjectActiveEvent; // UnityEvent to trigger when the object is active

    void Update()
    {
        // Check if the target GameObject is active in the hierarchy
        if (targetObject != null && targetObject.activeInHierarchy)
        {
            // Trigger the UnityEvent
            onObjectActiveEvent.Invoke();
        }
    }
}