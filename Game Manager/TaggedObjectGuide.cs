using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TaggedObjectGuide : MonoBehaviour
{
    [SerializeField]
    private string targetTag = ""; // Optional tag to filter objects, leave blank for no tag filter
    
    [SerializeField]
    private LayerMask targetLayers = ~0; // Optional layer filter, default to all layers
    
    [SerializeField]
    private float detectionRange = 10f; // Range to detect objects and show indicators
    
    [SerializeField]
    private float hysteresisBuffer = 2f; // Buffer to prevent jitter
    
    [SerializeField]
    private GameObject player; // Reference to the player GameObject
    
    [SerializeField]
    private Transform uiCanvas; // Reference to the Canvas transform
    
    [SerializeField]
    private float screenPadding = 20f; // Padding from screen edges
    
    [SerializeField]
    private float smoothSpeed = 10f; // Smoothing speed for UI movement
    
    [SerializeField]
    private float checkInterval = 1f; // How often to check for new objects (seconds)
    
    [Header("Indicator Prefabs")]
    [SerializeField]
    private Image arrowImagePrefab; // Prefab for the arrow Image
    
    [SerializeField]
    private TextMeshProUGUI distanceTextPrefab; // Prefab for distance text
    
    [SerializeField]
    private TextMeshProUGUI nameTextPrefab; // Prefab for name text
    
    [Header("Offsets")]
    [SerializeField]
    private Vector2 arrowOffset = new Vector2(0, 50f); // Offset for arrow
    
    [SerializeField]
    private Vector2 distanceOffset = new Vector2(0, -50f); // Offset for distance
    
    [SerializeField]
    private Vector2 nameOffset = new Vector2(0, -80f); // Offset for name
    
    [Header("Display Settings")]
    [SerializeField]
    private string distanceFormat = "{0} m"; // Format for distance text
    
    [SerializeField]
    private Color defaultArrowColor = Color.white; // Default arrow color if no ObjectInfo
    
    [SerializeField]
    private string defaultName = "Unknown"; // Default name if no ObjectInfo
    
    [SerializeField]
    private bool debugMode = false; // Toggle debug logging

    private float outerRange => detectionRange + hysteresisBuffer; // Calculated outer range
    private List<GameObject> detectedObjects = new List<GameObject>(); // List of detected objects
    private Dictionary<GameObject, (Image arrow, TextMeshProUGUI distanceText, TextMeshProUGUI nameText)> indicators = new Dictionary<GameObject, (Image, TextMeshProUGUI, TextMeshProUGUI)>(); // Maps objects to indicators
    private bool isPlayerInRange = false; // Track if player is within range
    private float lastCheckTime = 0f; // Track last object check

    void Start()
    {
        CheckForObjects(); // Initial detection
    }

    void OnEnable()
    {
        if (player != null && Camera.main != null)
        {
            CheckPlayerRange();
            UpdateIndicators();
        }
    }

    void OnDisable()
    {
        foreach (var pair in indicators)
        {
            if (pair.Value.arrow != null) pair.Value.arrow.gameObject.SetActive(false);
            if (pair.Value.distanceText != null) pair.Value.distanceText.gameObject.SetActive(false);
            if (pair.Value.nameText != null) pair.Value.nameText.gameObject.SetActive(false);
        }
        isPlayerInRange = false;
    }

    void Update()
    {
        if (player == null || Camera.main == null) return;

        if (Time.time - lastCheckTime >= checkInterval)
        {
            CheckForObjects();
            lastCheckTime = Time.time;
        }

        CheckPlayerRange();
        UpdateIndicators();
        CleanUpInactiveObjects();
    }

    private void CheckPlayerRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        isPlayerInRange = distanceToPlayer <= outerRange;
    }

    private void CheckForObjects()
    {
        // Clear objects that are no longer in range or inactive
        List<GameObject> objectsToRemove = new List<GameObject>();
        foreach (GameObject obj in detectedObjects)
        {
            if (obj == null || !obj.activeInHierarchy || Vector3.Distance(transform.position, obj.transform.position) > outerRange)
            {
                objectsToRemove.Add(obj);
            }
        }
        foreach (GameObject obj in objectsToRemove)
        {
            detectedObjects.Remove(obj);
            if (indicators.ContainsKey(obj))
            {
                var indicator = indicators[obj];
                if (indicator.arrow != null) Destroy(indicator.arrow.gameObject);
                if (indicator.distanceText != null) Destroy(indicator.distanceText.gameObject);
                if (indicator.nameText != null) Destroy(indicator.nameText.gameObject);
                indicators.Remove(obj);
            }
        }

        // Find new objects within range
        GameObject[] allObjects = string.IsNullOrEmpty(targetTag) ? Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None) :
        GameObject.FindGameObjectsWithTag(targetTag);
        foreach (GameObject obj in allObjects)
        {
            if (!obj.activeInHierarchy || detectedObjects.Contains(obj)) continue;

            float distanceToObject = Vector3.Distance(transform.position, obj.transform.position);
            if (distanceToObject > outerRange) continue; // Skip if outside range

            // Check layer if filter is applied
            if (targetLayers.value != ~0 && ((1 << obj.layer) & targetLayers.value) == 0) continue;

            detectedObjects.Add(obj);
            AssignIndicatorsToObject(obj);
            if (debugMode) Debug.Log($"Detected {obj.name} at {obj.transform.position}");
        }
    }

    private void AssignIndicatorsToObject(GameObject obj)
    {
        if (uiCanvas == null || arrowImagePrefab == null || distanceTextPrefab == null || nameTextPrefab == null)
        {
            Debug.LogError("Missing UI prefabs or Canvas reference!");
            return;
        }

        Image arrow = Instantiate(arrowImagePrefab, uiCanvas);
        arrow.gameObject.SetActive(false);

        TextMeshProUGUI distanceText = Instantiate(distanceTextPrefab, uiCanvas);
        distanceText.gameObject.SetActive(false);

        TextMeshProUGUI nameText = Instantiate(nameTextPrefab, uiCanvas);
        nameText.gameObject.SetActive(false);

        ObjectInfo info = obj.GetComponent<ObjectInfo>();
        arrow.color = info != null && info.arrowColor != Color.clear ? info.arrowColor : defaultArrowColor;
        
        // Remove "(Clone)" from GameObject name if present
        string cleanName = obj.name;
        if (cleanName.EndsWith("(Clone)"))
        {
            cleanName = cleanName.Substring(0, cleanName.Length - "(Clone)".Length).Trim();
        }
        nameText.text = info != null && !string.IsNullOrEmpty(info.objectName) ? info.objectName :
        (string.IsNullOrEmpty(cleanName) ? defaultName : cleanName);

        indicators[obj] = (arrow, distanceText, nameText);
    }

    private void UpdateIndicators()
    {
        foreach (var pair in indicators)
        {
            GameObject obj = pair.Key;
            Image arrow = pair.Value.arrow;
            TextMeshProUGUI distanceText = pair.Value.distanceText;
            TextMeshProUGUI nameText = pair.Value.nameText;

            if (obj == null || !obj.activeInHierarchy || arrow == null || distanceText == null || nameText == null) continue;

            if (!isPlayerInRange)
            {
                arrow.gameObject.SetActive(false);
                distanceText.gameObject.SetActive(false);
                nameText.gameObject.SetActive(false);
                continue;
            }

            Vector3 screenPos = Camera.main.WorldToScreenPoint(obj.transform.position);
            float distance = Vector3.Distance(player.transform.position, obj.transform.position);

            if (screenPos.z < 0) screenPos *= -1;

            float clampedX = Mathf.Clamp(screenPos.x, screenPadding, Screen.width - screenPadding);
            float clampedY = Mathf.Clamp(screenPos.y, screenPadding, Screen.height - screenPadding);
            Vector3 clampedScreenPos = new Vector3(clampedX, clampedY, screenPos.z);

            Vector3 arrowTargetPos = clampedScreenPos + (Vector3)arrowOffset;
            Vector3 distanceTargetPos = clampedScreenPos + (Vector3)distanceOffset;
            Vector3 nameTargetPos = clampedScreenPos + (Vector3)nameOffset;

            arrow.gameObject.SetActive(true);
            arrow.transform.position = Vector3.Lerp(arrow.transform.position, arrowTargetPos, smoothSpeed * Time.deltaTime);
            Vector3 direction = (obj.transform.position - player.transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            arrow.transform.rotation = Quaternion.Euler(0, 0, angle);

            distanceText.gameObject.SetActive(true);
            distanceText.text = string.Format(distanceFormat, distance.ToString("F1"));
            distanceText.transform.position = Vector3.Lerp(distanceText.transform.position, distanceTargetPos, smoothSpeed * Time.deltaTime);

            nameText.gameObject.SetActive(true);
            nameText.transform.position = Vector3.Lerp(nameText.transform.position, nameTargetPos, smoothSpeed * Time.deltaTime);
        }
    }

    private void CleanUpInactiveObjects()
    {
        List<GameObject> objectsToRemove = new List<GameObject>();

        foreach (var pair in indicators)
        {
            GameObject obj = pair.Key;
            Image arrow = pair.Value.arrow;
            TextMeshProUGUI distanceText = pair.Value.distanceText;
            TextMeshProUGUI nameText = pair.Value.nameText;

            if (obj == null || !obj.activeInHierarchy || Vector3.Distance(transform.position, obj.transform.position) > outerRange)
            {
                if (arrow != null) Destroy(arrow.gameObject);
                if (distanceText != null) Destroy(distanceText.gameObject);
                if (nameText != null) Destroy(nameText.gameObject);
                objectsToRemove.Add(obj);
            }
        }

        foreach (var obj in objectsToRemove)
        {
            indicators.Remove(obj);
            detectedObjects.Remove(obj);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, outerRange);
    }
}

public class ObjectInfo : MonoBehaviour
{
    public string objectName = ""; // Custom name, optional
    public Color arrowColor = Color.white; // Custom arrow color, optional
}