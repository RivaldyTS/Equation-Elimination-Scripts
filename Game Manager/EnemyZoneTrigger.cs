using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI; // Required for Image
using System.Collections.Generic;

public class EnemyZoneTrigger : MonoBehaviour
{
    [SerializeField]
    private float detectionRange = 5f; // Inner range to detect player
    
    [SerializeField]
    private float hysteresisBuffer = 1f; // Extra buffer distance to prevent jitter
    
    [SerializeField]
    private GameObject player; // Reference to the player GameObject
    
    [SerializeField]
    private List<GameObject> enemies = new List<GameObject>(); // List of enemy GameObjects
    
    [SerializeField]
    private TextMeshProUGUI textMeshPro; // Reference to TextMeshProUGUI component for main message
    
    [SerializeField]
    private AudioSource victoryAudio; // AudioSource to play when all enemies are destroyed
    
    [SerializeField]
    private UnityEvent onPlayerEnterRange; // Event when player enters range
    
    [SerializeField]
    private UnityEvent onAllEnemiesDestroyed; // Event when all enemies are destroyed
    
    [Header("Enemy Indicators")]
    [SerializeField]
    private List<Image> knobImages = new List<Image>(); // List of Images for enemy indicators
    
    [SerializeField]
    private Vector2 knobOffset = new Vector2(0, 0f); // Offset in screen space (pixels), directly above enemy
    
    [Header("Distance Indicators")]
    [SerializeField]
    private List<TextMeshProUGUI> arrowDistanceTexts = new List<TextMeshProUGUI>(); // List of TextMeshProUGUI for distance displays
    
    [SerializeField]
    private bool showArrowDistance = true; // Toggle to show distance
    
    [SerializeField]
    private string arrowDistanceFormat = "{0} m"; // Format for distance text (e.g., "5.0 m")
    
    [SerializeField]
    private Vector2 arrowDistanceOffset = new Vector2(0, -50f); // Offset in screen space, below enemy
    
    [SerializeField]
    private float screenPadding = 20f; // Padding from screen edges to keep indicators fully visible
    
    private float outerRange => detectionRange + hysteresisBuffer; // Calculated outer range
    private bool hasTriggeredEnterEvent = false; // Track enter event
    private bool hasTriggeredDestroyEvent = false; // Track destroy event
    private bool hasEnteredRange = false; // Track if player is in range
    private Dictionary<GameObject, (Image knob, TextMeshProUGUI distanceText)> enemyIndicators = new Dictionary<GameObject, (Image knob, TextMeshProUGUI distanceText)>(); // Map enemies to their indicators

    void Start()
    {
        // Ensure main text is initially inactive
        if (textMeshPro != null)
        {
            textMeshPro.gameObject.SetActive(false);
        }

        // Ensure all indicators are initially inactive
        foreach (var knob in knobImages)
        {
            if (knob != null)
            {
                knob.gameObject.SetActive(false);
            }
        }
        foreach (var distanceText in arrowDistanceTexts)
        {
            if (distanceText != null)
            {
                distanceText.gameObject.SetActive(false);
            }
        }

        // Map enemies to their indicators (if lists match in size)
        for (int i = 0; i < enemies.Count && i < knobImages.Count && i < arrowDistanceTexts.Count; i++)
        {
            if (enemies[i] != null && knobImages[i] != null && arrowDistanceTexts[i] != null)
            {
                enemyIndicators[enemies[i]] = (knobImages[i], arrowDistanceTexts[i]);
            }
        }
    }

    void Update()
    {
        if (player == null || Camera.main == null) return;
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        CleanUpDestroyedEnemies();
        if (distanceToPlayer <= detectionRange && !hasEnteredRange)
        {
            if (textMeshPro != null)
            {
                textMeshPro.gameObject.SetActive(true);
                textMeshPro.text = "Kalahkan semua musuhnya!";
            }
            TriggerEnterEvent();
            hasEnteredRange = true;
        }
        else if (hasEnteredRange && distanceToPlayer <= outerRange)
        {
            UpdateIndicators();
            if (AreAllEnemiesDead())
            {
                if (textMeshPro != null)
                {
                    textMeshPro.text = "Semua musuh selesai, lanjutkan!";
                }
                TriggerDestroyEvent();
                HideAllIndicators();
            }
        }
        else if (hasEnteredRange && (distanceToPlayer > outerRange || AreAllEnemiesDead()))
        {
            if (textMeshPro != null)
            {
                textMeshPro.gameObject.SetActive(false);
            }
            HideAllIndicators();
            hasEnteredRange = false;
            hasTriggeredEnterEvent = false;
        }
    }

    // Deactivate text and indicators when the script or GameObject is disabled
    void OnDisable()
    {
        if (textMeshPro != null && hasEnteredRange && !AreAllEnemiesDead())
        {
            textMeshPro.gameObject.SetActive(false);
            HideAllIndicators();
            hasEnteredRange = false;
            hasTriggeredEnterEvent = false; // Reset enter event for next activation
        }
    }

    private bool AreAllEnemiesDead()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                return false;
            }
        }
        return true;
    }

    private void TriggerEnterEvent()
    {
        if (!hasTriggeredEnterEvent)
        {
            onPlayerEnterRange?.Invoke();
            hasTriggeredEnterEvent = true;
        }
    }

    private void TriggerDestroyEvent()
    {
        if (!hasTriggeredDestroyEvent)
        {
            onAllEnemiesDestroyed?.Invoke(); // Trigger the destroy event
            if (victoryAudio != null && !victoryAudio.isPlaying)
            {
                victoryAudio.Play(); // Play the victory audio
            }
            hasTriggeredDestroyEvent = true;
        }
    }

    // Check for destroyed enemies and clean up their indicators
    private void CleanUpDestroyedEnemies()
    {
        List<GameObject> enemiesToRemove = new List<GameObject>();

        foreach (var pair in enemyIndicators)
        {
            GameObject enemy = pair.Key;
            Image knob = pair.Value.knob;
            TextMeshProUGUI distanceText = pair.Value.distanceText;

            if (enemy == null || knob == null || distanceText == null)
            {
                if (knob != null) knob.gameObject.SetActive(false);
                if (distanceText != null) distanceText.gameObject.SetActive(false);
                enemiesToRemove.Add(enemy);
            }
        }

        foreach (var enemy in enemiesToRemove)
        {
            enemyIndicators.Remove(enemy);
        }

        // Update the enemies list to remove null entries
        enemies.RemoveAll(enemy => enemy == null);
    }

    // Update knob and distance text positions for each enemy, clamping to screen
    private void UpdateIndicators()
    {
        if (AreAllEnemiesDead()) return;

        foreach (var pair in enemyIndicators)
        {
            GameObject enemy = pair.Key;
            Image knob = pair.Value.knob;
            TextMeshProUGUI distanceText = pair.Value.distanceText;

            if (enemy == null || knob == null || distanceText == null) continue;

            // Calculate position and distance
            Vector3 enemyScreenPos = Camera.main.WorldToScreenPoint(enemy.transform.position);
            float distance = Vector3.Distance(player.transform.position, enemy.transform.position);

            // If the enemy is behind the camera (negative Z), adjust position to screen edge
            if (enemyScreenPos.z < 0)
            {
                enemyScreenPos *= -1; // Flip the position to the opposite side of the screen
            }

            // Clamp the position to the screen boundaries with padding
            float clampedX = Mathf.Clamp(enemyScreenPos.x, screenPadding, Screen.width - screenPadding);
            float clampedY = Mathf.Clamp(enemyScreenPos.y, screenPadding, Screen.height - screenPadding);
            Vector3 clampedScreenPos = new Vector3(clampedX, clampedY, enemyScreenPos.z);

            // Update knob position
            knob.gameObject.SetActive(true);
            knob.transform.position = clampedScreenPos + (Vector3)knobOffset;

            // Update distance text position
            if (showArrowDistance)
            {
                distanceText.gameObject.SetActive(true);
                distanceText.text = string.Format(arrowDistanceFormat, distance.ToString("F1"));
                distanceText.transform.position = clampedScreenPos + (Vector3)arrowDistanceOffset;
            }
        }
    }

    // Hide all knobs and distance texts
    private void HideAllIndicators()
    {
        foreach (var pair in enemyIndicators)
        {
            if (pair.Value.knob != null)
            {
                pair.Value.knob.gameObject.SetActive(false);
            }
            if (pair.Value.distanceText != null)
            {
                pair.Value.distanceText.gameObject.SetActive(false);
            }
        }
    }

    // Optional: Visualize the ranges in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // Inner range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, outerRange); // Outer buffer range
    }

    // Method to add an enemy programmatically
    public void AddEnemy(GameObject enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
    }
}