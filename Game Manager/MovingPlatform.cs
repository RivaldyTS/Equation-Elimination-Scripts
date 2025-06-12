using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveOffset = new Vector3(5f, 0f, 0f); // Distance and direction to move
    [SerializeField] private float speed = 2f; // Movement speed
    [SerializeField] private float pauseDuration = 1f; // Time to pause at each end

    [Header("Start Settings")]
    [SerializeField] private bool startMoving = true; // Start moving immediately?
    [SerializeField] private float startDelay = 0f; // Initial delay before moving

    private Vector3 startPosition; // Initial position
    private Vector3 targetPosition; // Current target position
    private bool isMoving = false; // Movement state
    private bool movingForward = true; // Direction flag
    private float pauseTimer = 0f; // Timer for pausing
    private float startTimer = 0f; // Timer for initial delay

    private void Start()
    {
        // Store initial position and calculate first target
        startPosition = transform.position;
        targetPosition = startPosition + moveOffset;
        
        if (startMoving)
        {
            startTimer = startDelay;
        }
    }

    private void Update()
    {
        // Handle initial start delay
        if (startTimer > 0f)
        {
            startTimer -= Time.deltaTime;
            if (startTimer <= 0f)
            {
                StartMoving();
            }
            return;
        }

        // Handle pause timer
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                StartMoving();
            }
            return;
        }

        // Handle movement
        if (isMoving)
        {
            // Smoothly move towards target
            transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);

            // Check if close enough to target
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition; // Snap to target
                isMoving = false;
                
                // Set up for next movement
                movingForward = !movingForward;
                targetPosition = movingForward ? startPosition + moveOffset : startPosition;
                pauseTimer = pauseDuration; // Start pause timer
            }
        }
    }

    private void StartMoving()
    {
        isMoving = true;
    }

    // Optional: Reset to initial position immediately
    public void ResetPlatform()
    {
        transform.position = startPosition;
        movingForward = true;
        targetPosition = startPosition + moveOffset;
        isMoving = false;
        pauseTimer = 0f;
        startTimer = startDelay;
    }

    // Optional: Visualize the path in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 from = transform.position;
        Vector3 to = transform.position + moveOffset;
        
        Gizmos.DrawLine(from, to);
        Gizmos.DrawSphere(from, 0.1f);
        Gizmos.DrawSphere(to, 0.1f);
    }
}