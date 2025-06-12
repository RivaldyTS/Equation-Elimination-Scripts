using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro; // Required for TextMeshPro

public class MassButton : MonoBehaviour
{
    [Header("Button Settings")]
    [TagSelector]
    [SerializeField] private List<string> targetTags = new List<string> { "Ball" }; // List of tags that can activate the button
    [SerializeField] private bool useTrigger = true; // Use trigger for detection
    [SerializeField] private bool stayActivated = false; // If true, button stays activated after being pressed

    [Header("Mass Requirements")]
    [SerializeField] private List<float> acceptableMasses = new List<float> { 6f }; // List of acceptable masses
    [SerializeField] private float massTolerance = 0.1f; // Tolerance for mass comparison

    [Header("Activation Settings")]
    [SerializeField] private UnityEvent onActivate; // Event triggered when the button is activated
    [SerializeField] private UnityEvent onDeactivate; // Event triggered when the button is deactivated (if not stayActivated)
    [SerializeField] private UnityEvent onWrongMass; // Event triggered when the button is interacted with but the mass is wrong

    [Header("TextMeshPro Display (Optional)")]
    [SerializeField] private bool enableTextDisplay = false; // Enable/disable text display
    [SerializeField] private TextMeshPro stateText; // Reference to the TextMeshPro component
    [SerializeField] private string activeText = "Active"; // Text to display when activated
    [SerializeField] private string requiredMassText = "Mass: 6.0 (±0.1)"; // Editable text for required mass
    [SerializeField] private string wrongMassText = "Wrong Mass"; // Base text for wrong mass detection

    [Header("Audio Settings")]
    [SerializeField] private AudioClip activateSound; // Sound when button is activated
    [SerializeField] private AudioClip deactivateSound; // Sound when button is deactivated
    [SerializeField] private AudioClip wrongMassSound; // Sound when wrong mass is detected
    [SerializeField] private float volume = 1f; // Volume control (0 to 1)

    private bool isActivated = false; // Track whether the button is currently activated

    private void Start()
    {
        // Initialize text display if enabled
        if (enableTextDisplay && stateText != null)
        {
            UpdateText(requiredMassText); // Show required mass text initially
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (useTrigger && targetTags.Contains(other.tag))
        {
            if (CheckMass(other.attachedRigidbody))
            {
                ActivateButton();
            }
            else
            {
                onWrongMass.Invoke();
                PlaySound(wrongMassSound);
                UpdateText($"{wrongMassText}\n{requiredMassText}"); // Show both wrong mass and required mass
                Debug.Log("Wrong mass! Button not activated.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (useTrigger && targetTags.Contains(other.tag) && !stayActivated)
        {
            DeactivateButton();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!useTrigger && targetTags.Contains(collision.gameObject.tag))
        {
            if (CheckMass(collision.rigidbody))
            {
                ActivateButton();
            }
            else
            {
                onWrongMass.Invoke();
                PlaySound(wrongMassSound);
                UpdateText($"{wrongMassText}\n{requiredMassText}"); // Show both wrong mass and required mass
                Debug.Log("Wrong mass! Button not activated.");
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!useTrigger && targetTags.Contains(collision.gameObject.tag) && !stayActivated)
        {
            DeactivateButton();
        }
    }

    private bool CheckMass(Rigidbody rb)
    {
        if (rb == null) return false;

        foreach (float mass in acceptableMasses)
        {
            if (Mathf.Abs(rb.mass - mass) <= massTolerance)
            {
                return true;
            }
        }
        return false;
    }

    private void ActivateButton()
    {
        if (!isActivated)
        {
            isActivated = true;
            onActivate.Invoke();
            PlaySound(activateSound);
            UpdateText(activeText);
            Debug.Log("Button Activated!");
        }
    }

    private void DeactivateButton()
    {
        if (isActivated)
        {
            isActivated = false;
            onDeactivate.Invoke();
            PlaySound(deactivateSound);
            UpdateText(requiredMassText); // Show required mass text when deactivated
            Debug.Log("Button Deactivated!");
        }
    }

    private void UpdateText(string text)
    {
        if (enableTextDisplay && stateText != null)
        {
            stateText.text = text;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            GameObject soundObject = new GameObject("ButtonSound");
            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            
            audioSource.Play();
            Destroy(soundObject, clip.length);
        }
    }
}