using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;

public class AimLabSystem : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private float spawnRange = 10f;
    [SerializeField] private float targetLifetime = 2f;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private bool spawnTargetsAutomatically = true;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private int maxTargets = 10;

    [Header("Score Events")]
    [SerializeField] private int scoreEventInterval = 10;
    [SerializeField] private UnityEvent onScoreEvent;

    [Header("Timer Events")]
    [SerializeField] private UnityEvent onTimerFinish;
    [SerializeField] private UnityEvent onTimerReset;

    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private string scorePrefix = "Score: ";
    [SerializeField] private TextMeshProUGUI accuracyText;
    [SerializeField] private string accuracyPrefix = "Accuracy: ";
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private string timerPrefix = "Time: ";
    [SerializeField] private TextMeshProUGUI missedTargetsText;
    [SerializeField] private string missedTargetsPrefix = "Missed Targets: ";

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip scoreIncreaseSound;
    [SerializeField] private AudioClip missedTargetSound;

    [Header("Particle Effect Settings")]
    [SerializeField] private ParticleSystem targetSpawnParticle;

    private int totalShots = 0;
    private int totalHits = 0;
    private int totalMissedTargets = 0;
    private float gameTimer;
    private bool isGameActive = false;
    private bool isTimerRunning = false;
    private bool isSpawning = false;

    void Start()
    {
        UpdateUI();
        // StartTimer(); // Uncomment for auto-start
    }

    void Update()
    {
        if (isTimerRunning)
        {
            gameTimer -= Time.deltaTime;
            UpdateUI();

            if (gameTimer <= 0)
            {
                StopTimer();
                onTimerFinish.Invoke();
            }
        }

        // Detect missed shots with mouse click
        if (isGameActive && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit) || hit.collider.tag != "Target")
            {
                RegisterMiss();
            }
        }
    }

    public void StartTimer()
    {
        if (!isTimerRunning)
        {
            isTimerRunning = true;
            isGameActive = true;
            gameTimer = gameDuration;
            Debug.Log("Timer started!");

            if (spawnTargetsAutomatically && !isSpawning)
            {
                StartCoroutine(SpawnTargets());
            }
        }
    }

    public void StopTimer()
    {
        if (isTimerRunning)
        {
            isTimerRunning = false;
            isGameActive = false;
            Debug.Log("Timer stopped!");
        }
    }

    public void ResetTimer()
    {
        gameTimer = gameDuration;
        isTimerRunning = false;
        isGameActive = false;
        totalShots = 0;
        totalHits = 0;
        totalMissedTargets = 0;
        UpdateUI();
        onTimerReset.Invoke();
        Debug.Log("Timer and scores reset!");
    }

    private IEnumerator SpawnTargets()
    {
        isSpawning = true;
        while (isGameActive)
        {
            if (GameObject.FindGameObjectsWithTag("Target").Length < maxTargets)
            {
                Vector3 randomDirection = Random.insideUnitSphere * spawnRange;
                randomDirection.y = 0;
                Vector3 spawnPosition = transform.position + randomDirection;
                GameObject target = Instantiate(targetPrefab, spawnPosition, Quaternion.identity);
                Destroy(target, targetLifetime);
                if (targetSpawnParticle != null)
                {
                    Instantiate(targetSpawnParticle, spawnPosition, Quaternion.identity);
                }
                AimLabTarget aimLabTarget = target.AddComponent<AimLabTarget>();
                aimLabTarget.Initialize(this);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
        isSpawning = false;
    }

    public void RegisterHit()
    {
        if (!isGameActive) return;
        Debug.Log("RegisterHit called! TotalHits: " + totalHits + ", TotalShots: " + totalShots);
        totalHits++;
        totalShots++;
        UpdateUI();

        if (totalHits % scoreEventInterval == 0)
        {
            onScoreEvent.Invoke();
        }

        PlayScoreIncreaseSound();
    }

    public void RegisterMiss()
    {
        if (!isTimerRunning) return;
        Debug.Log("RegisterMiss called! TotalShots: " + totalShots);
        totalShots++;
        UpdateUI();
    }

    public void RegisterMissedTarget()
    {
        if (!isTimerRunning) return;
        totalMissedTargets++;
        UpdateUI();
        PlayMissedTargetSound();
    }

    private float GetAccuracy()
    {
        if (totalShots == 0) return 0f;
        return (float)totalHits / totalShots * 100f;
    }

    private void UpdateUI()
    {
        Debug.Log("UpdateUI called! Accuracy: " + GetAccuracy());
        if (scoreText != null) scoreText.text = scorePrefix + totalHits;
        if (accuracyText != null)
        {
            Debug.Log("AccuracyText is assigned!");
            accuracyText.text = accuracyPrefix + GetAccuracy().ToString("F1") + "%";
        }
        else
        {
            Debug.LogWarning("AccuracyText is null!");
        }
        if (timerText != null) timerText.text = timerPrefix + Mathf.Max(0, gameTimer).ToString("F1") + "s";
        if (missedTargetsText != null) missedTargetsText.text = missedTargetsPrefix + totalMissedTargets;
    }

    private void PlayScoreIncreaseSound()
    {
        if (audioSource != null && scoreIncreaseSound != null)
        {
            audioSource.PlayOneShot(scoreIncreaseSound);
        }
        else
        {
            Debug.LogWarning("AudioSource or scoreIncreaseSound is not assigned.");
        }
    }

    private void PlayMissedTargetSound()
    {
        if (audioSource != null && missedTargetSound != null)
        {
            audioSource.PlayOneShot(missedTargetSound);
        }
        else
        {
            Debug.LogWarning("AudioSource or missedTargetSound is not assigned.");
        }
    }
}