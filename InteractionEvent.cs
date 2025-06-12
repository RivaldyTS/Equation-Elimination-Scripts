using UnityEngine.Events;
using UnityEngine;

public class InteractionEvent : MonoBehaviour
{
    public UnityEvent onInteract;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Interact()
    {
        onInteract.Invoke(); // Trigger the UnityEvent
    }
}
