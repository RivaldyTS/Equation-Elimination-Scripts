using UnityEngine;
using UnityEngine.Events;

public class LongStayZoneTrigger : MonoBehaviour
{
    [Header("Settings")]
    public Transform player; // Assign the player's Transform here
    public float triggerDistance = 5f; // The distance defining inside/outside
    public float requiredStayTime = 3f; // Time in seconds to trigger long stay event
    public float hysteresis = 0.5f; // Buffer zone to prevent jitter
    
    [Header("Events")]
    public UnityEvent onEnterInside; // Triggers once when player enters the range
    public UnityEvent onLongStayInside; // Triggers when player stays inside for required time
    public UnityEvent onExitToOutside; // Triggers once when player exits the range

    [Header("Audio")]
    public AudioClip insideClip; // Sound to play when entering inside
    public AudioClip longStayClip; // Sound to play when stay duration is met
    public AudioClip outsideClip; // Sound to play when entering outside

    [Header("Debug")]
    public Color rangeColor = Color.green; // Color of the trigger sphere in editor

    private bool isPlayerInside = false; // Tracks current state
    private float timeInside = 0f; // Tracks time spent inside
    private bool hasTriggeredLongStay = false; // Prevents repeated long stay triggers

    void Update()
    {
        // Calculate the distance between the player and this object
        float distance = Vector3.Distance(player.position, transform.position);
        
        // Define enter and exit thresholds with hysteresis
        float enterThreshold = triggerDistance;
        float exitThreshold = triggerDistance + hysteresis;

        // Player enters the inside zone
        if (distance <= enterThreshold && !isPlayerInside)
        {
            isPlayerInside = true;
            timeInside = 0f; // Reset timer on entry
            hasTriggeredLongStay = false; // Reset long stay trigger
            onEnterInside.Invoke(); // Trigger entry event
            
            if (insideClip != null)
            {
                PlayTemporarySound(insideClip);
            }
        }
        // Player is inside and we track time
        else if (distance <= enterThreshold && isPlayerInside)
        {
            timeInside += Time.deltaTime;
            
            // Check if player has stayed long enough and hasn't triggered yet
            if (timeInside >= requiredStayTime && !hasTriggeredLongStay)
            {
                hasTriggeredLongStay = true;
                onLongStayInside.Invoke(); // Trigger long stay event
                
                if (longStayClip != null)
                {
                    PlayTemporarySound(longStayClip);
                }
            }
        }
        // Player exits to the outside zone
        else if (distance > exitThreshold && isPlayerInside)
        {
            isPlayerInside = false;
            timeInside = 0f;
            hasTriggeredLongStay = false;
            onExitToOutside.Invoke(); // Trigger exit event
            
            if (outsideClip != null)
            {
                PlayTemporarySound(outsideClip);
            }
        }
    }

    private void PlayTemporarySound(AudioClip clip)
    {
        GameObject tempAudio = new GameObject("TempAudio_" + clip.name);
        tempAudio.transform.position = transform.position;
        AudioSource source = tempAudio.AddComponent<AudioSource>();
        source.clip = clip;
        source.playOnAwake = false;
        source.Play();
        Destroy(tempAudio, clip.length);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = rangeColor;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
        // Draw the exit threshold for debugging
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerDistance + hysteresis);
    }
}