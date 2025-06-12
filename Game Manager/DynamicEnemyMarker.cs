using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DynamicEnemyMarker : MonoBehaviour
{
    [SerializeField]
    private float detectionRange = 5f; // Inner range to detect player and enemies
    
    [SerializeField]
    private float hysteresisBuffer = 1f; // Extra buffer distance to prevent jitter
    
    [SerializeField]
    private GameObject player; // Reference to the player GameObject
    
    [SerializeField]
    private TextMeshProUGUI textMeshPro; // Reference to TextMeshProUGUI component for main message
    
    [SerializeField]
    private AudioSource victoryAudio; // AudioSource to play when all enemies are destroyed
    
    [SerializeField]
    private float enemyCheckInterval = 1f; // How often to check for new enemies (seconds)
    
    [SerializeField]
    private float screenPadding = 20f; // Padding from screen edges to keep indicators fully visible
    
    [SerializeField]
    private float smoothSpeed = 10f; // Speed of smoothing for UI element movement (higher = faster)
    
    [Header("Enemy Indicator Prefabs")]
    [SerializeField]
    private Image knobImagePrefab; // Prefab for the knob Image (assign a UI Image prefab)
    
    [SerializeField]
    private TextMeshProUGUI arrowDistanceTextPrefab; // Prefab for the distance TextMeshProUGUI
    
    [SerializeField]
    private Transform uiCanvas; // Reference to the Canvas transform to parent UI elements
    
    [SerializeField]
    private Vector2 knobOffset = new Vector2(0, 0f); // Offset in screen space (pixels), directly above enemy
    
    [Header("Distance Indicators")]
    [SerializeField]
    private bool showArrowDistance = true; // Toggle to show distance
    
    [SerializeField]
    private string arrowDistanceFormat = "{0} m"; // Format for distance text (e.g., "5.0 m")
    
    [SerializeField]
    private Vector2 arrowDistanceOffset = new Vector2(0, -50f); // Offset in screen space, below enemy
    
    private float outerRange => detectionRange + hysteresisBuffer; // Calculated outer range
    private bool hasEnteredRange = false; // Track if player is in range
    private float lastEnemyCheckTime = 0f; // Track last time we checked for enemies
    private List<GameObject> enemies = new List<GameObject>(); // List of detected enemies within range
    private Dictionary<GameObject, (Image knob, TextMeshProUGUI distanceText)> enemyIndicators = new Dictionary<GameObject, (Image knob, TextMeshProUGUI distanceText)>(); // Map enemies to their indicators

    void Start()
    {
        // Ensure main text is initially inactive
        if (textMeshPro != null)
        {
            textMeshPro.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null || Camera.main == null) return;

        // Periodically check for new enemies within range
        if (Time.time - lastEnemyCheckTime >= enemyCheckInterval)
        {
            CheckForNewEnemies();
            lastEnemyCheckTime = Time.time;
        }

        // Always clean up destroyed enemies
        CleanUpDestroyedEnemies();

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // Check if player enters the inner range
        if (distanceToPlayer <= detectionRange && !hasEnteredRange)
        {
            // Activate main text and set initial message
            if (textMeshPro != null)
            {
                textMeshPro.gameObject.SetActive(true);
                textMeshPro.text = "Kalahkan semua musuhnya!"; // "Defeat all the enemies!"
            }
            hasEnteredRange = true;
        }
        // Check if player is still within outer range and handle enemy destruction
        else if (hasEnteredRange && distanceToPlayer <= outerRange)
        {
            UpdateIndicators(); // Update knob and distance text positions for each enemy

            // Check if all enemies are destroyed
            if (AreAllEnemiesDead())
            {
                if (textMeshPro != null)
                {
                    textMeshPro.text = "Semua musuh selesai, lanjutkan!"; // "All enemies finished, continue!"
                }
                if (victoryAudio != null && !victoryAudio.isPlaying)
                {
                    victoryAudio.Play(); // Play the victory audio
                }
                HideAllIndicators(); // Hide all indicators
            }
        }
        // Player has exited the outer range or all enemies are dead
        else if (hasEnteredRange && (distanceToPlayer > outerRange || AreAllEnemiesDead()))
        {
            // Deactivate text and indicators
            if (textMeshPro != null)
            {
                textMeshPro.gameObject.SetActive(false);
            }
            HideAllIndicators();
            hasEnteredRange = false;
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
        }
    }

    // Check for new enemies tagged "Enemy" within the detection range
    private void CheckForNewEnemies()
    {
        GameObject[] taggedEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in taggedEnemies)
        {
            // Only consider enemies within the detection range
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy <= detectionRange && !enemies.Contains(enemy) && !enemyIndicators.ContainsKey(enemy))
            {
                enemies.Add(enemy);
                AssignIndicatorsToEnemy(enemy);
            }
        }

        // Remove enemies that are now outside the range
        List<GameObject> enemiesToRemove = new List<GameObject>();
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue; // Skip null enemies (handled by CleanUpDestroyedEnemies)
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy > outerRange)
            {
                enemiesToRemove.Add(enemy);
                if (enemyIndicators.ContainsKey(enemy))
                {
                    var indicators = enemyIndicators[enemy];
                    if (indicators.knob != null) Destroy(indicators.knob.gameObject);
                    if (indicators.distanceText != null) Destroy(indicators.distanceText.gameObject);
                    enemyIndicators.Remove(enemy);
                }
            }
        }
        enemies.RemoveAll(enemy => enemiesToRemove.Contains(enemy));
    }

    // Assign UI indicators to a newly detected enemy
    private void AssignIndicatorsToEnemy(GameObject enemy)
    {
        if (uiCanvas == null || knobImagePrefab == null || arrowDistanceTextPrefab == null) return;

        // Instantiate knob Image
        Image knob = Instantiate(knobImagePrefab, uiCanvas);
        knob.gameObject.SetActive(false);

        // Instantiate distance TextMeshProUGUI
        TextMeshProUGUI distanceText = Instantiate(arrowDistanceTextPrefab, uiCanvas);
        distanceText.gameObject.SetActive(false);

        // Map the enemy to its indicators
        enemyIndicators[enemy] = (knob, distanceText);
    }

    // Check if all enemies are destroyed
    private bool AreAllEnemiesDead()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                return false; // If any enemy still exists, return false
            }
        }
        return true; // All enemies are destroyed
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
                if (knob != null) Destroy(knob.gameObject);
                if (distanceText != null) Destroy(distanceText.gameObject);
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

    // Update knob and distance text positions for each enemy, clamping to screen with smoothing
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

            // Calculate target positions
            Vector3 knobTargetPos = clampedScreenPos + (Vector3)knobOffset;
            Vector3 distanceTextTargetPos = clampedScreenPos + (Vector3)arrowDistanceOffset;

            // Smoothly move the UI elements to their target positions
            knob.gameObject.SetActive(true);
            knob.transform.position = Vector3.Lerp(knob.transform.position, knobTargetPos, smoothSpeed * Time.deltaTime);

            if (showArrowDistance)
            {
                distanceText.gameObject.SetActive(true);
                distanceText.text = string.Format(arrowDistanceFormat, distance.ToString("F1"));
                distanceText.transform.position = Vector3.Lerp(distanceText.transform.position, distanceTextTargetPos, smoothSpeed * Time.deltaTime);
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
}