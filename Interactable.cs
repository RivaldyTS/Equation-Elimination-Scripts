using UnityEngine;
using UnityEngine.Events;

public abstract class Interactable : MonoBehaviour
{
    public bool useEvents;
    [SerializeField]
    public string promptMessage;

    public UnityEvent onPickup;

    public virtual string OnLook()
    {
        return promptMessage;
    }

    public void BaseInteract()
    {
        if (useEvents)
            GetComponent<InteractionEvent>().onInteract.Invoke();
        Interact();
    }

    protected virtual void Interact()
    {
        // Base interact logic (can be overridden)
    }
}