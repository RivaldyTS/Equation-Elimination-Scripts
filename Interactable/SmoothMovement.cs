using UnityEngine;
using UnityEngine.Events;

public class SmoothMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 targetPosition; // Target position to move to
    [SerializeField] private float speed = 2f; // Speed of movement

    [Header("Auto Start Settings")]
    [SerializeField] private bool autoStart = false; // Should the object move automatically after resetting?
    [SerializeField] private float startDelay = 0f; // Delay before starting movement (in seconds)

    [Header("Auto Reset Settings")]
    [SerializeField] private bool autoReset = false; // Should the object reset automatically after moving to the target?
    [SerializeField] private float resetDelay = 2f; // Delay before resetting (in seconds)

    [Header("Events")]
    public UnityEvent onMovementStart; // Event triggered when movement starts
    public UnityEvent onMovementComplete; // Event triggered when movement finishes

    private Vector3 _startPosition;
    private bool _isMoving = false;
    private bool _isMovingToTarget = true; // True = moving to target, False = moving to start
    private float _resetTimer = 0f; // Timer for auto-reset
    private float _startTimer = 0f; // Timer for auto-start

    private void Start()
    {
        _startPosition = transform.position; // Save the initial position

        if (autoStart)
        {
            StartAutoStartTimer(); // Start the auto-start timer
        }
    }

    private void Update()
    {
        if (_startTimer > 0f)
        {
            _startTimer -= Time.deltaTime;
            if (_startTimer <= 0f)
            {
                StartMovement();
            }
        }

        if (_isMoving)
        {
            Vector3 currentTarget = _isMovingToTarget ? targetPosition : _startPosition;
            transform.position = Vector3.Lerp(transform.position, currentTarget, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, currentTarget) < 0.01f)
            {
                transform.position = currentTarget;
                _isMoving = false; // Stop moving
                if (_isMovingToTarget && autoReset)
                {
                    _resetTimer = resetDelay;
                }
                onMovementComplete.Invoke();
            }
        }

        if (_resetTimer > 0f)
        {
            _resetTimer -= Time.deltaTime;
            if (_resetTimer <= 0f)
            {
                ResetPosition();
            }
        }
    }
    public void StartMovement()
    {
        if (!_isMoving)
        {
            _isMoving = true;
            _isMovingToTarget = true;
            onMovementStart.Invoke();
        }
    }

    // Call this method to smoothly reset the object to its initial position
    public void ResetPosition()
    {
        if (!_isMoving)
        {
            _isMoving = true;
            _isMovingToTarget = false; // Move back to the start position
            onMovementStart.Invoke(); // Trigger the start event

            // If auto-start is enabled, start the auto-start timer after resetting
            if (autoStart)
            {
                StartAutoStartTimer();
            }
        }
    }

    // Start the auto-start timer
    private void StartAutoStartTimer()
    {
        _startTimer = startDelay; // Set the timer to the start delay
    }

    // Optional: Add a method to check if the object is at the start position
    public bool IsAtStartPosition()
    {
        return Vector3.Distance(transform.position, _startPosition) < 0.01f;
    }

    // Optional: Add a method to check if the object is at the target position
    public bool IsAtTargetPosition()
    {
        return Vector3.Distance(transform.position, targetPosition) < 0.01f;
    }
}