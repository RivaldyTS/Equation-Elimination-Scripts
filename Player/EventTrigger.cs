using UnityEngine;
using UnityEngine.Events;

public class EventTrigger : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode keyToPress = KeyCode.Space; // The key to press to trigger the event
    public bool requireActiveGameObject = true; // Whether to check for an active GameObject
    public GameObject requiredGameObject; // The GameObject that must be active to trigger the event

    [Header("Event")]
    public UnityEvent onKeyPress; // The UnityEvent to trigger

    void Update()
    {
        // Check if the required GameObject is active (if enabled)
        if (requireActiveGameObject && (requiredGameObject == null || !requiredGameObject.activeSelf))
        {
            //Debug.Log("Event cannot be triggered: Required GameObject is not active.");
            return;
        }

        // Check if the key is pressed
        if (Input.GetKeyDown(keyToPress))
        {
            onKeyPress.Invoke(); // Trigger the UnityEvent
        }
    }
}