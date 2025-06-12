using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    public enum CheckpointType { Permanent, Temporary, OneTime }

    [Header("Checkpoint Settings")]
    public float activationRange = 5f;
    public Color gizmoColor = Color.green;

    [Header("Checkpoint Type")]
    public CheckpointType checkpointType = CheckpointType.Permanent;

    [Header("Indicators")]
    public GameObject activeIndicator;
    public GameObject inactiveIndicator;

    [Header("UI Notification")]
    public TextMeshProUGUI checkpointText;
    public string notificationText = "Checkpoint Reached!";
    public float textDisplayTime = 2f;
    public float fadeDuration = 1f;
    public bool changeTextColor = true;
    public Color highlightColor = Color.green;
    public float colorChangeDuration = 1f;

    [Header("Progress Bar")]
    public Image activationProgressBar;
    public TextMeshProUGUI progressBarText;
    public float activationTime = 2f;
    private float activationProgress = 0f;

    [Header("Progress Bar Animation")]
    public bool useProgressBarAnimation = true;
    public float animationSpeed = 1f;

    [Header("Progress Sound")]
    public bool useProgressSound = true;
    public AudioClip progressSound;
    private AudioSource progressAudioSource;

    [Header("Range Indicator")]
    public bool useRangeIndicator = true;
    public LineRenderer rangeIndicator;
    public int circleSegments = 32;

    [Header("Progress Completion Effects")]
    public bool useProgressCompletionEffects = true;
    public ParticleSystem progressCompleteParticles;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip activationSound;

    [Header("Particle Effects")]
    public ParticleSystem activationParticles;

    [Header("Animation")]
    public Animator checkpointAnimator;
    public string activationTrigger = "Activate";

    [Header("Events")]
    public UnityEvent onProgressStart;
    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    [Header("Respawn Overlay")]
    public Image respawnOverlay;
    public Color overlayColor = Color.black;
    public float overlayFadeDuration = 1f;
    public bool useRespawnOverlay = true;

    [Header("Time Slowdown (Bullet Time)")]
    public bool useTimeSlowdown = true;
    public float slowdownFactor = 0.5f;
    public float slowdownDuration = 1f;
    public bool useSlowdownFade = true;
    public float fadeBackDuration = 1f;

    [Header("Respawn Particle Effects")]
    public bool useRespawnParticles = true;
    public ParticleSystem respawnParticles;
    public GameObject respawnParticleSpawnPoint;

    [Header("Respawn Sound Effects")]
    public bool useRespawnSound = true;
    public AudioSource respawnAudioSource;
    public AudioClip respawnSound;

    [Header("Respawn Point")]
    public GameObject respawnPoint;

    private static Vector3 lastCheckpointPosition;
    private static Quaternion lastCheckpointRotation;
    private static bool hasCheckpoint = false;
    private static Checkpoint activeCheckpoint;

    private Coroutine textCoroutine;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }

    private void Start()
    {
        if (activeIndicator != null) activeIndicator.SetActive(false);
        if (inactiveIndicator != null) inactiveIndicator.SetActive(true);

        if (checkpointText != null)
        {
            checkpointText.gameObject.SetActive(false);
            checkpointText.alpha = 0f;
        }

        if (respawnOverlay != null)
        {
            respawnOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
            respawnOverlay.gameObject.SetActive(false);
        }

        if (activationProgressBar != null)
        {
            activationProgressBar.fillAmount = 0f;
            activationProgressBar.gameObject.SetActive(false);
        }

        if (progressBarText != null)
        {
            progressBarText.gameObject.SetActive(false);
        }

        if (useProgressSound)
        {
            progressAudioSource = gameObject.AddComponent<AudioSource>();
            progressAudioSource.clip = progressSound;
            progressAudioSource.loop = true;
        }

        if (useRangeIndicator && rangeIndicator != null)
        {
            rangeIndicator.positionCount = circleSegments + 1;
            rangeIndicator.useWorldSpace = false;

            float angle = 0f;
            for (int i = 0; i <= circleSegments; i++)
            {
                float x = Mathf.Sin(Mathf.Deg2Rad * angle) * activationRange;
                float z = Mathf.Cos(Mathf.Deg2Rad * angle) * activationRange;
                rangeIndicator.SetPosition(i, new Vector3(x, 0f, z));
                angle += 360f / circleSegments;
            }
        }
    }

    private void Update()
    {
        if (PlayerHealth.Instance != null && Vector3.Distance(transform.position, PlayerHealth.Instance.transform.position) <= activationRange)
        {
            if (activationProgressBar != null && !activationProgressBar.gameObject.activeSelf)
            {
                activationProgressBar.gameObject.SetActive(true);
                if (progressBarText != null) progressBarText.gameObject.SetActive(true);
                onProgressStart.Invoke();

                if (useProgressSound && !progressAudioSource.isPlaying)
                {
                    progressAudioSource.Play();
                }
            }

            activationProgress += Time.deltaTime / activationTime;
            activationProgress = Mathf.Clamp01(activationProgress);

            if (activationProgressBar != null)
            {
                if (useProgressBarAnimation)
                {
                    activationProgressBar.fillAmount = Mathf.PingPong(Time.time * animationSpeed, 1f) * activationProgress;
                }
                else
                {
                    activationProgressBar.fillAmount = activationProgress;
                }
            }

            if (activationProgress >= 1f)
            {
                ActivateCheckpoint();
                DeactivateProgressBar();
            }
        }
        else
        {
            activationProgress = 0f;
            DeactivateProgressBar();
        }
    }

    private void DeactivateProgressBar()
    {
        if (activationProgressBar != null)
        {
            activationProgressBar.fillAmount = 0f;
            activationProgressBar.gameObject.SetActive(false);
        }

        if (progressBarText != null)
        {
            progressBarText.gameObject.SetActive(false);
        }

        if (useProgressSound && progressAudioSource.isPlaying)
        {
            progressAudioSource.Stop();
        }
    }

    public void ActivateCheckpoint()
    {
        if (activeCheckpoint == this) return;
        if (activeCheckpoint != null && activeCheckpoint.checkpointType == CheckpointType.Temporary)
        {
            activeCheckpoint.DeactivateCheckpoint();
        }
        if (respawnPoint != null)
        {
            lastCheckpointPosition = respawnPoint.transform.position;
            lastCheckpointRotation = respawnPoint.transform.rotation;
        }
        else
        {
            lastCheckpointPosition = transform.position;
            lastCheckpointRotation = Quaternion.identity;
        }
        hasCheckpoint = true;
        activeCheckpoint = this;
        if (checkpointType == CheckpointType.OneTime)
        {
            DeactivateCheckpoint();
        }
        if (activeIndicator != null) activeIndicator.SetActive(true);
        if (inactiveIndicator != null) inactiveIndicator.SetActive(false);

        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        if (activationParticles != null)
        {
            activationParticles.Play();
        }
        if (checkpointAnimator != null)
        {
            checkpointAnimator.SetTrigger(activationTrigger);
        }

        if (useProgressCompletionEffects && progressCompleteParticles != null)
        {
            progressCompleteParticles.Play();
        }
        onActivate.Invoke();
        ShowCheckpointText();
    }

    private void DeactivateCheckpoint()
    {
        if (activeIndicator != null) activeIndicator.SetActive(false);
        if (inactiveIndicator != null) inactiveIndicator.SetActive(true);
        onDeactivate.Invoke();
    }

    private void ShowCheckpointText()
    {
        if (checkpointText != null)
        {
            checkpointText.text = notificationText;
            checkpointText.alpha = 1f;
            checkpointText.gameObject.SetActive(true);

            if (textCoroutine != null)
            {
                StopCoroutine(textCoroutine);
            }

            textCoroutine = StartCoroutine(HandleTextEffects());
        }
    }

    private IEnumerator HandleTextEffects()
    {
        if (changeTextColor)
        {
            Color originalColor = checkpointText.color;
            checkpointText.color = highlightColor;

            yield return new WaitForSeconds(colorChangeDuration);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                checkpointText.color = Color.Lerp(highlightColor, originalColor, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            checkpointText.color = originalColor;
        }

        yield return new WaitForSeconds(textDisplayTime - colorChangeDuration);

        float fadeElapsed = 0f;
        while (fadeElapsed < fadeDuration)
        {
            checkpointText.alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);
            fadeElapsed += Time.deltaTime;
            yield return null;
        }

        checkpointText.alpha = 0f;
        checkpointText.gameObject.SetActive(false);
    }

    private IEnumerator HandleRespawnOverlay()
    {
        if (!useRespawnOverlay || respawnOverlay == null) yield break;

        respawnOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 1f);
        respawnOverlay.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < overlayFadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / overlayFadeDuration);
            respawnOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        respawnOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
        respawnOverlay.gameObject.SetActive(false);
    }

    private IEnumerator SlowTime()
    {
        if (!useTimeSlowdown) yield break;

        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(slowdownDuration);

        if (useSlowdownFade)
        {
            float elapsed = 0f;
            float initialTimeScale = Time.timeScale;

            while (elapsed < fadeBackDuration)
            {
                Time.timeScale = Mathf.Lerp(initialTimeScale, 1f, elapsed / fadeBackDuration);
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    private void PlayRespawnParticles()
    {
        if (useRespawnParticles && respawnParticles != null && respawnParticleSpawnPoint != null)
        {
            respawnParticles.transform.position = respawnParticleSpawnPoint.transform.position;
            respawnParticles.transform.rotation = respawnParticleSpawnPoint.transform.rotation;
            respawnParticles.Play();
        }
    }

    private void PlayRespawnSound()
    {
        if (useRespawnSound && respawnAudioSource != null && respawnSound != null)
        {
            respawnAudioSource.PlayOneShot(respawnSound);
        }
    }

    public static void RespawnPlayer()
    {
        if (hasCheckpoint)
        {
            PlayerHealth.Instance.GetComponent<CharacterController>().enabled = false;
            PlayerHealth.Instance.transform.position = lastCheckpointPosition;
            PlayerHealth.Instance.transform.rotation = lastCheckpointRotation;
            PlayerHealth.Instance.GetComponent<CharacterController>().enabled = true;
            PlayerHealth.Instance.RestoreHealth(PlayerHealth.Instance.maxHealth);

            if (activeCheckpoint != null)
            {
                activeCheckpoint.StartCoroutine(activeCheckpoint.HandleRespawnOverlay());
                activeCheckpoint.StartCoroutine(activeCheckpoint.SlowTime());
                activeCheckpoint.PlayRespawnParticles();
                activeCheckpoint.PlayRespawnSound();
            }
        }
        else
        {
            PlayerHealth.Instance.ResetGame();
        }
    }

    public static bool HasCheckpoint()
    {
        return hasCheckpoint;
    }
}