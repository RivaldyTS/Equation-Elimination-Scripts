using UnityEngine;
using UnityEngine.Events;

public class InsideOutsideTrigger : MonoBehaviour
{
    [Header("Settings")]
    public Transform player; // Assign the player's Transform here
    public float triggerDistance = 5f; // The distance defining inside/outside
    public float hysteresis = 0.5f; // Buffer zone to prevent jitter
    
    [Header("Events")]
    public UnityEvent onEnterInside; // Triggers once when player enters the range
    public UnityEvent onExitToOutside; // Triggers once when player exits the range

    [Header("Audio")]
    public AudioClip insideClip; // Sound to play when entering inside
    public AudioClip outsideClip; // Sound to play when entering outside

    [Header("Debug")]
    public Color rangeColor = Color.green; // Color of the trigger sphere in editor

    private bool isPlayerInside = false; // Tracks current state

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);
        
        float enterThreshold = triggerDistance;
        float exitThreshold = triggerDistance + hysteresis;

        if (distance <= enterThreshold && !isPlayerInside)
        {
            isPlayerInside = true;
            onEnterInside.Invoke();
            
            if (insideClip != null)
            {
                PlayTemporarySound(insideClip);
            }
        }
        else if (distance > exitThreshold && isPlayerInside)
        {
            isPlayerInside = false;
            onExitToOutside.Invoke();
            
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