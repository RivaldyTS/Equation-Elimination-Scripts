using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class MaterialTextureTrigger : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private List<GameObject> targetObjects; // List of GameObjects to check for material/texture
    [SerializeField] private Material targetMaterial; // The material to check for
    [SerializeField] private Texture targetTexture; // The texture to check for (optional)

    [Header("Trigger Settings")]
    [SerializeField] private bool checkMaterial = true; // Check for material
    [SerializeField] private bool checkTexture = false; // Check for texture
    [SerializeField] private bool continuousCheck = false; // Continuously check for the material/texture

    [Header("Events")]
    [SerializeField] private UnityEvent onAllConditionsMet; // Event triggered when all conditions are met
    [SerializeField] private UnityEvent onAnyConditionNotMet; // Event triggered when any condition is not met

    [Header("Audio Settings")]
    [SerializeField] private AudioClip conditionsMetSound; // Sound when all conditions are met
    [SerializeField] private AudioClip conditionsNotMetSound; // Sound when any condition is not met
    [SerializeField] private float volume = 1f; // Volume control (0 to 1)

    private List<Renderer> targetRenderers = new List<Renderer>(); // Renderer components of the target objects
    private bool allConditionsMet = false; // Track whether all conditions are currently met

    void Start()
    {
        // Initialize the list of renderers
        foreach (var targetObject in targetObjects)
        {
            if (targetObject != null)
            {
                var renderer = targetObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    targetRenderers.Add(renderer);
                }
                else
                {
                    Debug.LogError($"Target object {targetObject.name} does not have a Renderer component!");
                }
            }
            else
            {
                Debug.LogError("A target object in the list is not assigned!");
            }
        }

        CheckAllConditions(); // Initial check
    }

    void Update()
    {
        if (continuousCheck)
        {
            CheckAllConditions(); // Continuously check for the conditions
        }
    }

    private void CheckAllConditions()
    {
        bool newAllConditionsMet = true;
        // Check each target object's material/texture
        foreach (var renderer in targetRenderers)
        {
            bool conditionMet = false;

            // Check for material
            if (checkMaterial && renderer.sharedMaterial == targetMaterial)
            {
                conditionMet = true;
            }

            // Check for texture
            if (checkTexture && renderer.sharedMaterial.mainTexture == targetTexture)
            {
                conditionMet = true;
            }

            // If any object fails the condition, set newAllConditionsMet to false
            if (!conditionMet)
            {
                newAllConditionsMet = false;
                break;
            }
        }
        // Trigger events and play sound if the condition has changed
        if (newAllConditionsMet != allConditionsMet)
        {
            allConditionsMet = newAllConditionsMet;

            if (allConditionsMet)
            {
                onAllConditionsMet.Invoke();
                PlaySound(conditionsMetSound);
            }
            else
            {
                onAnyConditionNotMet.Invoke();
                PlaySound(conditionsNotMetSound);
            }
        }
    }

    // Method to spawn AudioSource and play sound
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            // Create a new GameObject with an AudioSource
            GameObject soundObject = new GameObject("MaterialTriggerSound");
            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            
            // Configure AudioSource
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
            
            // Play the sound
            audioSource.Play();
            
            // Destroy the object after the clip finishes
            Destroy(soundObject, clip.length);
        }
    }
}