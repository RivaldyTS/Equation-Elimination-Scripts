using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PressureButton : MonoBehaviour
{
    [Header("Button Settings")]
    [TagSelector] // Add the TagSelector attribute here
    [SerializeField] private List<string> targetTags = new List<string> { "Ball" }; // List of tags that can activate the button
    [SerializeField] private bool useTrigger = true; // Use trigger for detection (recommended for puzzles)
    [SerializeField] private bool stayActivated = false; // If true, button stays activated after being pressed

    [Header("Activation Settings")]
    [SerializeField] private UnityEvent onActivate; // Event triggered when the button is activated
    [SerializeField] private UnityEvent onDeactivate; // Event triggered when the button is deactivated (if not stayActivated)

    private bool isActivated = false; // Track whether the button is currently activated

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object has one of the allowed tags
        if (useTrigger && targetTags.Contains(other.tag))
        {
            ActivateButton();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the collided object has one of the allowed tags and the button should deactivate
        if (useTrigger && targetTags.Contains(other.tag) && !stayActivated)
        {
            DeactivateButton();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!useTrigger && targetTags.Contains(collision.gameObject.tag))
        {
            ActivateButton();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!useTrigger && targetTags.Contains(collision.gameObject.tag) && !stayActivated)
        {
            DeactivateButton();
        }
    }

    private void ActivateButton()
    {
        if (!isActivated)
        {
            isActivated = true;
            onActivate.Invoke(); // Trigger the activation event
            Debug.Log("Button Activated!");
        }
    }

    private void DeactivateButton()
    {
        if (isActivated)
        {
            isActivated = false;
            onDeactivate.Invoke(); // Trigger the deactivation event
            Debug.Log("Button Deactivated!");
        }
    }
}