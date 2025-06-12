using UnityEngine;
using UnityEngine.Events; // For UnityEvent
using UnityEngine.UI; // For Image (progress bar)
using TMPro; // For TextMeshPro and TextMeshProUGUI

public class Timer3DAndUIWithEvents : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float startTime = 60f; // Default timer duration (1 minute)

    [Header("3D TextMeshPro Settings")]
    [SerializeField] private TextMeshPro timer3DText; // Assign a 3D TextMeshPro object in Inspector

    [Header("UI TextMeshPro Settings")]
    [SerializeField] private TextMeshProUGUI timerUIText; // Assign a UI TextMeshProUGUI object in Inspector

    [Header("Progress Bar Settings")]
    [SerializeField] private Image progressBar; // Assign a UI Image for the progress bar in Inspector

    [Header("Timer Events")]
    public UnityEvent OnTimerComplete; // Triggered when the timer reaches 0
    public UnityEvent OnStartTimer; // Triggered when the timer starts
    public UnityEvent OnInterruptTimer; // Triggered when the timer is interrupted
    public UnityEvent OnResetTimer; // Triggered when the timer is reset
    public UnityEvent OnContinueTimer; // Triggered when the timer continues
    public UnityEvent OnTimerHalfway; // Triggered when timer reaches halfway

    [Header("Audio Settings")]
    [SerializeField] private AudioClip startSound; // Assign in Inspector
    [SerializeField] private AudioClip completeSound; // Assign in Inspector
    [SerializeField] private AudioClip interruptSound; // Assign in Inspector
    [SerializeField] private AudioClip resetSound; // Assign in Inspector
    [SerializeField] private AudioClip continueSound; // Assign in Inspector
    [SerializeField] private AudioClip halfwaySound; // Assign in Inspector
    [SerializeField] private float volume = 1f; // Volume control (0 to 1)

    private float currentTime;
    private bool isTimerActive = false;
    private bool hasHalfwayTriggered = false;

    void Start()
    {
        currentTime = startTime;
        UpdateTimerDisplay();
        UpdateProgressBar();
        hasHalfwayTriggered = false;
    }

    void Update()
    {
        if (isTimerActive)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                UpdateTimerDisplay();
                UpdateProgressBar();

                if (!hasHalfwayTriggered && currentTime <= startTime / 2f)
                {
                    OnTimerHalfway.Invoke();
                    PlaySound(halfwaySound);
                    hasHalfwayTriggered = true;
                    Debug.Log("Timer has reached halfway!");
                }
            }
            else
            {
                currentTime = 0;
                isTimerActive = false;
                OnTimerComplete.Invoke();
                PlaySound(completeSound);
                Debug.Log("Timer has finished!");
            }
        }
    }

    public void StartTimer()
    {
        isTimerActive = true;
        hasHalfwayTriggered = false;
        OnStartTimer.Invoke();
        PlaySound(startSound);
    }

    public void InterruptTimer()
    {
        isTimerActive = false;
        OnInterruptTimer.Invoke();
        PlaySound(interruptSound);
    }

    public void ResetTimer()
    {
        currentTime = startTime;
        isTimerActive = false;
        hasHalfwayTriggered = false;
        UpdateTimerDisplay();
        UpdateProgressBar();
        OnResetTimer.Invoke();
        PlaySound(resetSound);
    }

    public void ContinueTimer()
    {
        isTimerActive = true;
        OnContinueTimer.Invoke();
        PlaySound(continueSound);
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        string timeText = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (timer3DText != null)
        {
            timer3DText.text = timeText;
        }

        if (timerUIText != null)
        {
            timerUIText.text = timeText;
        }
    }

    private void UpdateProgressBar()
    {
        if (progressBar != null)
        {
            float progress = 1 - (currentTime / startTime);
            progressBar.fillAmount = progress;
        }
    }

    // Method to spawn AudioSource and play sound
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            // Create a new GameObject with an AudioSource
            GameObject soundObject = new GameObject("TimerSound");
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

    public bool IsTimerActive => isTimerActive;
}