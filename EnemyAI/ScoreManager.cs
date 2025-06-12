using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance; // Singleton pattern for easy access

    public int score = 0; // Current score
    public int highScore = 0; // High score
    public TextMeshProUGUI scoreText; // Reference to TextMeshProUGUI for current score
    public TextMeshProUGUI highScoreText; // Reference to TextMeshProUGUI for high score
    public TextMeshProUGUI comboText; // UI text for combo display
    public TextMeshProUGUI announcementText; // UI text for streak announcements
    public Image overlayImage; // Reference to your UI Image
    public Image comboProgressBar; // UI Image for combo timer
    public TextMeshProUGUI comboTimeLabel; // Label for combo progress
    public float fadeDuration = 0.7f; // How long the fade in/out takes for overlay
    public AudioClip scoreIncreaseSound; // Audio clip for score increase
    public float soundVolume = 1f; // Volume control in Inspector
    public float comboWindow = 6f; // Tweakable combo duration, default 6 seconds

    private float fadeTimer;
    private bool isFading;
    private int comboCount = 0; // Combo counter
    private float comboTimer = 0f; // Time since last score for combo
    private float multiplierTimer = 0f; // Time since last score for multiplier
    private int multiplier = 1; // Current multiplier
    private float announcementTimer = 0f; // Timer for announcement display
    private const float MULTIPLIER_WINDOW = 5f; // 5 seconds to increase multiplier
    private const float ANNOUNCEMENT_DURATION = 1.5f; // Announcement display time

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateHighScoreUI();

        if (overlayImage != null)
        {
            overlayImage.gameObject.SetActive(false);
            overlayImage.color = new Color(overlayImage.color.r, overlayImage.color.g, overlayImage.color.b, 0f);
        }

        UpdateComboUI();
        if (announcementText != null)
        {
            announcementText.text = "";
        }
        if (comboProgressBar != null)
        {
            comboProgressBar.fillAmount = 0f;
            comboProgressBar.gameObject.SetActive(false);
        }
        if (comboTimeLabel != null)
        {
            comboTimeLabel.text = "Waktu";
            comboTimeLabel.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Faint gray
            comboTimeLabel.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Handle overlay fading
        if (isFading && overlayImage != null)
        {
            fadeTimer += Time.deltaTime;
            float alpha;

            if (fadeTimer <= fadeDuration / 2)
            {
                alpha = Mathf.Lerp(0f, 1f, fadeTimer / (fadeDuration / 2));
            }
            else
            {
                alpha = Mathf.Lerp(1f, 0f, (fadeTimer - fadeDuration / 2) / (fadeDuration / 2));
            }

            overlayImage.color = new Color(overlayImage.color.r, overlayImage.color.g, overlayImage.color.b, alpha);

            if (fadeTimer >= fadeDuration)
            {
                isFading = false;
                overlayImage.gameObject.SetActive(false);
            }
        }

        // Handle combo and multiplier timers
        if (comboCount > 0)
        {
            comboTimer += Time.deltaTime;
            if (comboProgressBar != null)
            {
                float fill = 1f - (comboTimer / comboWindow); // Use tweakable comboWindow
                comboProgressBar.fillAmount = fill;
                comboProgressBar.color = fill > 0.5f ? Color.green : (fill > 0.16f ? Color.yellow : Color.red);
            }
            if (comboTimer >= comboWindow) // Use tweakable comboWindow
            {
                EndCombo();
            }
        }

        if (multiplier > 1)
        {
            multiplierTimer += Time.deltaTime;
            if (multiplierTimer >= MULTIPLIER_WINDOW)
            {
                multiplier = 1;
                UpdateScoreUI();
            }
        }

        // Handle announcement fading
        if (announcementTimer > 0)
        {
            announcementTimer -= Time.deltaTime;
            if (announcementTimer <= 0 && announcementText != null)
            {
                announcementText.text = "";
            }
        }
    }

    public void AddScore(int points)
    {
        comboCount++;
        comboTimer = 0f;
        UpdateComboUI();

        if (comboTimer < MULTIPLIER_WINDOW)
        {
            multiplier = Mathf.Min(comboCount, 5);
            multiplierTimer = 0f;
        }

        int pointsWithMultiplier = points * multiplier;
        score += pointsWithMultiplier;
        Debug.Log("Added " + pointsWithMultiplier + " points. Current score: " + score + " (x" + multiplier + ")");
        UpdateScoreUI();

        // Trigger overlay
        if (overlayImage != null && !isFading)
        {
            overlayImage.gameObject.SetActive(true);
            isFading = true;
            fadeTimer = 0f;
        }

        // Play sound with adjusted pitch
        if (scoreIncreaseSound != null)
        {
            PlayTemporarySound(scoreIncreaseSound, soundVolume, 1f + (comboCount - 1) * 0.1f);
        }

        // Streak Announcements
        if (announcementText != null)
        {
            if (comboCount == 5)
            {
                announcementText.text = "Kombo Hebat!";
                announcementTimer = ANNOUNCEMENT_DURATION;
            }
            else if (comboCount == 10)
            {
                announcementText.text = "Kombo Legendaris!";
                announcementTimer = ANNOUNCEMENT_DURATION;
            }
            else if (multiplier == 3)
            {
                announcementText.text = "Pengganda Luar Biasa!";
                announcementTimer = ANNOUNCEMENT_DURATION;
            }
            else if (multiplier == 5)
            {
                announcementText.text = "Pengganda Maksimal!";
                announcementTimer = ANNOUNCEMENT_DURATION;
            }
        }

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            UpdateHighScoreUI();
        }
    }

    private void PlayTemporarySound(AudioClip clip, float volume, float pitch)
    {
        GameObject tempAudio = new GameObject("TempScoreSound");
        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = Mathf.Min(volume, 1f);
        audioSource.pitch = Mathf.Clamp(pitch, 1f, 2f);
        audioSource.Play();
        Destroy(tempAudio, clip.length);
    }

    private void EndCombo()
    {
        if (comboCount > 1)
        {
            int bonus = comboCount * 10;
            score += bonus;
            Debug.Log("Combo Ended! Bonus: " + bonus);
            UpdateScoreUI();

            if (overlayImage != null)
            {
                SpawnScoreBurst(bonus);
            }
        }
        comboCount = 0;
        UpdateComboUI();
    }

    private void SpawnScoreBurst(int bonus)
    {
        int burstCount = Mathf.Min(comboCount, 5);
        string[] phrases = { "Bonus: ", "Hadiah: ", "Ekstra: " };
        Color[] colors = { Color.green, Color.yellow, new Color(1f, 0.5f, 0f) }; // Green, Yellow, Orange

        for (int i = 0; i < burstCount; i++)
        {
            GameObject burstTextObj = new GameObject("ScoreBurst");
            burstTextObj.transform.SetParent(overlayImage.transform.parent, false);
            TextMeshProUGUI burstText = burstTextObj.AddComponent<TextMeshProUGUI>();
            burstText.text = phrases[i % phrases.Length] + bonus;
            burstText.fontSize = 30;
            burstText.color = colors[i % colors.Length];
            burstText.alignment = TextAlignmentOptions.Center;
            burstText.transform.Rotate(0f, 0f, Random.Range(-15f, 15f));

            RectTransform rect = burstTextObj.GetComponent<RectTransform>();
            float angle = i * (360f / burstCount) + Random.Range(-20f, 20f);
            Vector2 offset = Quaternion.Euler(0, 0, angle) * Vector2.up * 50f;
            rect.anchoredPosition = overlayImage.rectTransform.anchoredPosition + offset;

            StartCoroutine(AnimateBurst(burstTextObj, angle));
        }
    }

    private System.Collections.IEnumerator AnimateBurst(GameObject burstObj, float angle)
    {
        TextMeshProUGUI text = burstObj.GetComponent<TextMeshProUGUI>();
        RectTransform rect = burstObj.GetComponent<RectTransform>();
        float duration = 1.5f;
        float timer = 0f;
        Vector2 startPos = rect.anchoredPosition;
        Color startColor = text.color;
        Vector3 startScale = Vector3.one;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            float scale = timer < 0.3f ? Mathf.Lerp(1f, 1.2f, timer / 0.3f) : Mathf.Lerp(1.2f, 0.8f, (timer - 0.3f) / (duration - 0.3f));
            rect.localScale = startScale * scale;

            float xOffset = Mathf.Sin(t * Mathf.PI * 2f) * 20f;
            float yOffset = 50f * t;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
            rect.anchoredPosition = startPos + (direction * yOffset) + (Vector2.right * xOffset);

            text.color = Color.Lerp(startColor, Color.clear, t);
            yield return null;
        }

        Destroy(burstObj);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Skor: " + score + (multiplier > 1 ? " x" + multiplier : "");
        }
    }

    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text = "Skor Tertinggi: " + highScore;
        }
    }

    private void UpdateComboUI()
    {
        if (comboText != null)
        {
            comboText.text = comboCount > 0 ? "Kombo: " + comboCount : "";
        }
        if (comboProgressBar != null)
        {
            comboProgressBar.gameObject.SetActive(comboCount > 0);
        }
        if (comboTimeLabel != null)
        {
            comboTimeLabel.gameObject.SetActive(comboCount > 0);
        }
    }

    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey("HighScore");
        highScore = 0;
        UpdateHighScoreUI();
    }
}