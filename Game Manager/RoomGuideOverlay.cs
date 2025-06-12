using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RoomGuideOverlay : MonoBehaviour
{
    [SerializeField]
    private float detectionRange = 10f; // Range from this GameObject to detect player and show all rooms
    
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
    
    [Header("Room Indicator Prefabs")]
    [SerializeField]
    private Image arrowImagePrefab; // Prefab for the arrow Image (points to room)
    
    [SerializeField]
    private TextMeshProUGUI distanceTextPrefab; // Prefab for distance text
    
    [SerializeField]
    private TextMeshProUGUI roomNameTextPrefab; // Prefab for room name text
    
    [Header("Offsets")]
    [SerializeField]
    private Vector2 arrowOffset = new Vector2(0, 50f); // Offset for arrow
    
    [SerializeField]
    private Vector2 distanceOffset = new Vector2(0, -50f); // Offset for distance
    
    [SerializeField]
    private Vector2 roomNameOffset = new Vector2(0, -80f); // Offset for room name
    
    [Header("Display Settings")]
    [SerializeField]
    private string distanceFormat = "{0} m"; // Format for distance text
    
    [SerializeField]
    private Color defaultArrowColor = Color.white; // Default color if no RoomInfo is present
    
    [SerializeField]
    private float roomCheckInterval = 1f; // How often to check for new active rooms (seconds)

    private float outerRange => detectionRange + hysteresisBuffer; // Calculated outer range
    private List<GameObject> rooms = new List<GameObject>(); // List of all detected rooms
    private Dictionary<GameObject, (Image arrow, TextMeshProUGUI distanceText, TextMeshProUGUI roomNameText)> roomIndicators = new Dictionary<GameObject, (Image, TextMeshProUGUI, TextMeshProUGUI)>(); // Maps each room to its indicators
    private bool isPlayerInRange = false; // Track if player is within detection range
    private float lastRoomCheckTime = 0f; // Track last time we checked for new rooms

    void Start()
    {
        // Find all active rooms tagged "Room" at startup
        GameObject[] taggedRooms = GameObject.FindGameObjectsWithTag("Room");
        foreach (GameObject room in taggedRooms)
        {
            if (room.activeInHierarchy) // Only add active rooms
            {
                rooms.Add(room);
                AssignIndicatorsToRoom(room);
            }
        }

        // Warn if no active rooms are found
        if (rooms.Count == 0)
        {
            Debug.LogWarning("No active rooms found with tag 'Room'. Please tag your room GameObjects and ensure they are enabled.");
        }
    }

    void OnEnable()
    {
        // When enabled, check if player is in range and update indicators
        if (player != null && Camera.main != null)
        {
            CheckPlayerRange();
            UpdateIndicators();
        }
    }

    void OnDisable()
    {
        // When disabled, hide all indicators immediately
        foreach (var pair in roomIndicators)
        {
            if (pair.Value.arrow != null)
            {
                pair.Value.arrow.gameObject.SetActive(false);
            }
            if (pair.Value.distanceText != null)
            {
                pair.Value.distanceText.gameObject.SetActive(false);
            }
            if (pair.Value.roomNameText != null)
            {
                pair.Value.roomNameText.gameObject.SetActive(false);
            }
        }
        isPlayerInRange = false; // Reset range flag
    }

    void Update()
    {
        if (player == null || Camera.main == null) return;

        // Periodically check for newly active rooms
        if (Time.time - lastRoomCheckTime >= roomCheckInterval)
        {
            CheckForNewRooms();
            lastRoomCheckTime = Time.time;
        }

        // Check if player is within range of this GameObject
        CheckPlayerRange();

        // Update indicators for all rooms
        UpdateIndicators();

        // Clean up any destroyed or disabled rooms
        CleanUpInactiveRooms();
    }

    // Check if player is within detection range of this GameObject
    private void CheckPlayerRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= outerRange)
        {
            isPlayerInRange = true;
        }
        else
        {
            isPlayerInRange = false;
        }
    }

    // Check for newly active rooms tagged "Room"
    private void CheckForNewRooms()
    {
        GameObject[] taggedRooms = GameObject.FindGameObjectsWithTag("Room");
        foreach (GameObject room in taggedRooms)
        {
            if (room.activeInHierarchy && !rooms.Contains(room))
            {
                rooms.Add(room);
                AssignIndicatorsToRoom(room);
            }
        }
    }

    // Assign UI indicators (arrow, distance, room name) to a specific room
    private void AssignIndicatorsToRoom(GameObject room)
    {
        if (uiCanvas == null || arrowImagePrefab == null || distanceTextPrefab == null || roomNameTextPrefab == null)
        {
            Debug.LogError("Missing UI prefabs or Canvas reference!");
            return;
        }

        // Instantiate arrow Image
        Image arrow = Instantiate(arrowImagePrefab, uiCanvas);
        arrow.gameObject.SetActive(false);

        // Set arrow color based on RoomInfo or default
        RoomInfo roomInfo = room.GetComponent<RoomInfo>();
        arrow.color = roomInfo != null && roomInfo.arrowColor != Color.clear 
            ? roomInfo.arrowColor 
            : defaultArrowColor;

        // Instantiate distance TextMeshProUGUI
        TextMeshProUGUI distanceText = Instantiate(distanceTextPrefab, uiCanvas);
        distanceText.gameObject.SetActive(false);

        // Instantiate room name TextMeshProUGUI
        TextMeshProUGUI roomNameText = Instantiate(roomNameTextPrefab, uiCanvas);
        roomNameText.gameObject.SetActive(false);

        // Get the room name from RoomInfo component or fallback to GameObject name
        string roomName = roomInfo != null && !string.IsNullOrEmpty(roomInfo.roomName) 
            ? roomInfo.roomName 
            : room.name;
        roomNameText.text = roomName;

        // Map the room to its indicators
        roomIndicators[room] = (arrow, distanceText, roomNameText);
    }

    // Update arrow, distance, and room name positions for each room
    private void UpdateIndicators()
    {
        foreach (var pair in roomIndicators)
        {
            GameObject room = pair.Key;
            Image arrow = pair.Value.arrow;
            TextMeshProUGUI distanceText = pair.Value.distanceText;
            TextMeshProUGUI roomNameText = pair.Value.roomNameText;

            if (room == null || !room.activeInHierarchy || arrow == null || distanceText == null || roomNameText == null) continue;

            // If player is not in range, hide all indicators
            if (!isPlayerInRange)
            {
                arrow.gameObject.SetActive(false);
                distanceText.gameObject.SetActive(false);
                roomNameText.gameObject.SetActive(false);
                continue;
            }

            // Calculate screen position and distance from player to room
            Vector3 roomScreenPos = Camera.main.WorldToScreenPoint(room.transform.position);
            float distance = Vector3.Distance(player.transform.position, room.transform.position);

            // Flip position if behind camera
            if (roomScreenPos.z < 0)
            {
                roomScreenPos *= -1;
            }

            // Clamp to screen boundaries
            float clampedX = Mathf.Clamp(roomScreenPos.x, screenPadding, Screen.width - screenPadding);
            float clampedY = Mathf.Clamp(roomScreenPos.y, screenPadding, Screen.height - screenPadding);
            Vector3 clampedScreenPos = new Vector3(clampedX, clampedY, roomScreenPos.z);

            // Calculate target positions
            Vector3 arrowTargetPos = clampedScreenPos + (Vector3)arrowOffset;
            Vector3 distanceTargetPos = clampedScreenPos + (Vector3)distanceOffset;
            Vector3 roomNameTargetPos = clampedScreenPos + (Vector3)roomNameOffset;

            // Show and smoothly move UI elements
            arrow.gameObject.SetActive(true);
            arrow.transform.position = Vector3.Lerp(arrow.transform.position, arrowTargetPos, smoothSpeed * Time.deltaTime);

            // Rotate arrow to point toward the room
            Vector3 direction = (room.transform.position - player.transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // Adjust for arrow sprite
            arrow.transform.rotation = Quaternion.Euler(0, 0, angle);

            distanceText.gameObject.SetActive(true);
            distanceText.text = string.Format(distanceFormat, distance.ToString("F1"));
            distanceText.transform.position = Vector3.Lerp(distanceText.transform.position, distanceTargetPos, smoothSpeed * Time.deltaTime);

            roomNameText.gameObject.SetActive(true);
            roomNameText.transform.position = Vector3.Lerp(roomNameText.transform.position, roomNameTargetPos, smoothSpeed * Time.deltaTime);
        }
    }

    // Clean up destroyed or disabled rooms
    private void CleanUpInactiveRooms()
    {
        List<GameObject> roomsToRemove = new List<GameObject>();

        foreach (var pair in roomIndicators)
        {
            GameObject room = pair.Key;
            Image arrow = pair.Value.arrow;
            TextMeshProUGUI distanceText = pair.Value.distanceText;
            TextMeshProUGUI roomNameText = pair.Value.roomNameText;

            // Check if room is null or disabled
            if (room == null || !room.activeInHierarchy)
            {
                if (arrow != null) Destroy(arrow.gameObject);
                if (distanceText != null) Destroy(distanceText.gameObject);
                if (roomNameText != null) Destroy(roomNameText.gameObject);
                roomsToRemove.Add(room);
            }
        }

        foreach (var room in roomsToRemove)
        {
            roomIndicators.Remove(room);
            rooms.Remove(room);
        }
    }

    // Visualize ranges in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // Inner range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, outerRange); // Outer buffer range
    }
}

// Component for custom room names and arrow colors
public class RoomInfo : MonoBehaviour
{
    public string roomName = "Unnamed Room"; // Set this in the Inspector for each room
    public Color arrowColor = Color.white; // Set this in the Inspector for each room’s arrow
}