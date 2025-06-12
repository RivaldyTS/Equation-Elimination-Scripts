using UnityEngine;
using UnityEngine.Events;

public class TimerScript : MonoBehaviour
{
    [SerializeField] private float duration = 5f; // Timer duration in seconds
    [SerializeField] private UnityEvent onTimerComplete; // Event to trigger when timer finishes
    
    private float timeRemaining;
    private bool isTimerRunning = false;

    // Called when the GameObject is enabled
    private void OnEnable()
    {
        StartTimer();
    }

    // Called when the GameObject is disabled
    private void OnDisable()
    {
        StopTimer();
    }

    public void StartTimer()
    {
        timeRemaining = duration;
        isTimerRunning = true;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                isTimerRunning = false;
                onTimerComplete?.Invoke(); // Trigger the UnityEvent
            }
        }
    }

    // Optional: Get remaining time (useful for UI)
    public float GetTimeRemaining()
    {
        return Mathf.Max(0, timeRemaining);
    }
}