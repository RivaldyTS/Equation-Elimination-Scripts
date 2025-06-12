using UnityEngine;
using UnityEngine.Events;

public class ToggleDistanceTrigger : MonoBehaviour
{
    [Header("Settings")]
    public Transform player; // Assign the player's Transform here
    public float triggerDistance = 5f; // The distance at which events trigger
    public float hysteresis = 0.5f; // Buffer zone to prevent jitter at boundary
    
    [Header("Events")]
    public UnityEvent onFirstState; // Triggers on first, third, fifth, etc. approaches
    public UnityEvent onSecondState; // Triggers on second, fourth, sixth, etc. approaches

    [Header("Audio")]
    public AudioClip enterClip; // Sound to play when entering
    public AudioClip exitClip; // Sound to play when exiting

    [Header("Debug")]
    public Color rangeColor = Color.green; // Color of the trigger sphere in editor

    private bool isFirstState = true; // Tracks which state we're in
    private bool isPlayerInside = false; // Tracks current player presence
    private bool hasToggledThisEntry = false; // Prevents multiple toggles per entry

    void Update()
    {
        // Calculate the distance between the player and this object
        float distance = Vector3.Distance(player.position, transform.position);
        
        // Define enter and exit thresholds with hysteresis
        float enterThreshold = triggerDistance;
        float exitThreshold = triggerDistance + hysteresis;
        
        // Player enters the trigger zone
        if (distance <= enterThreshold && !isPlayerInside)
        {
            isPlayerInside = true;
            hasToggledThisEntry = false; // Reset toggle flag
            
            // Play enter sound
            if (enterClip != null)
            {
                PlayTemporarySound(enterClip);
            }

            // Toggle state only once per entry
            if (!hasToggledThisEntry)
            {
                if (isFirstState)
                {
                    onFirstState.Invoke();
                }
                else
                {
                    onSecondState.Invoke();
                }
                isFirstState = !isFirstState; // Toggle for next entry
                hasToggledThisEntry = true; // Mark as toggled
            }
        }
        // Player leaves the trigger zone (only when beyond exit threshold)
        else if (distance > exitThreshold && isPlayerInside)
        {
            isPlayerInside = false;
            hasToggledThisEntry = false; // Reset for next entry
            
            // Play exit sound
            if (exitClip != null)
            {
                PlayTemporarySound(exitClip);
            }
        }
    }

    private void PlayTemporarySound(AudioClip clip)
    {
        // Create a temporary GameObject for the sound
        GameObject tempAudio = new GameObject("TempAudio_" + clip.name);
        tempAudio.transform.position = transform.position; // Position at trigger location
        
        // Add AudioSource and configure it
        AudioSource source = tempAudio.AddComponent<AudioSource>();
        source.clip = clip;
        source.playOnAwake = false;
        
        // Play the sound
        source.Play();
        
        // Destroy the GameObject after the clip finishes
        Destroy(tempAudio, clip.length);
    }

    // Draws the trigger range in the editor
    void OnDrawGizmos()
    {
        Gizmos.color = rangeColor;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
        // Optionally draw the exit threshold
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerDistance + hysteresis);
    }
}