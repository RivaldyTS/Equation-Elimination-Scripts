using UnityEngine;

public class Keypad : Interactable
{
    [SerializeField] private GameObject door;
    [SerializeField] private AudioClip interactSound; // Assigned in the Inspector

    private bool doorOpen;
    private AudioSource audioSource; // Reference to the AudioSource component

    private void Start()
    {
        // Get the AudioSource component from the same GameObject
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // If AudioSource is not found, add it dynamically
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set AudioSource settings (adjust as needed)
        audioSource.playOnAwake = false; // Ensure it doesn't play automatically on awake
        audioSource.spatialBlend = 0f; // 2D sound
    }

    protected override void Interact()
    {
        // Toggle the door state
        doorOpen = !doorOpen;
        door.GetComponent<Animator>().SetBool("IsOpen", doorOpen);

        // Play interaction sound
        if (interactSound != null)
        {
            audioSource.PlayOneShot(interactSound); // Play the interactSound AudioClip once
        }
    }
}
