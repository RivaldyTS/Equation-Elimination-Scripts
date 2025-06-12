using UnityEngine;
using UnityEngine.Events;

public class SmoothRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private Vector3 targetRotation; // Target rotation to rotate to (in Euler angles)
    [SerializeField] private float speed = 2f; // Speed of rotation

    [Header("Auto Start Settings")]
    [SerializeField] private bool autoStart = false; // Should the object rotate automatically after resetting?
    [SerializeField] private float startDelay = 0f; // Delay before starting rotation (in seconds)

    [Header("Auto Reset Settings")]
    [SerializeField] private bool autoReset = false; // Should the object reset automatically after rotating to the target?
    [SerializeField] private float resetDelay = 2f; // Delay before resetting (in seconds)

    [Header("Events")]
    public UnityEvent onRotationStart; // Event triggered when rotation starts
    public UnityEvent onRotationComplete; // Event triggered when rotation finishes

    private Quaternion _startRotation;
    private bool _isRotating = false;
    private bool _isRotatingToTarget = true; // True = rotating to target, False = rotating to start
    private float _resetTimer = 0f; // Timer for auto-reset
    private float _startTimer = 0f; // Timer for auto-start

    private void Start()
    {
        _startRotation = transform.rotation; // Save the initial rotation

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
                StartRotation();
            }
        }

        if (_isRotating)
        {
            Quaternion currentTarget = _isRotatingToTarget ? Quaternion.Euler(targetRotation) : _startRotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, currentTarget, speed * Time.deltaTime);
            if (Quaternion.Angle(transform.rotation, currentTarget) < 0.1f)
            {
                transform.rotation = currentTarget;
                _isRotating = false;
                if (_isRotatingToTarget && autoReset)
                {
                    _resetTimer = resetDelay;
                }
                onRotationComplete.Invoke();
            }
        }

        if (_resetTimer > 0f)
        {
            _resetTimer -= Time.deltaTime;
            if (_resetTimer <= 0f)
            {
                ResetRotation();
            }
        }
    }

    public void StartRotation()
    {
        if (!_isRotating)
        {
            _isRotating = true;
            _isRotatingToTarget = true;
            onRotationStart.Invoke();
        }
    }

    // Call this method to smoothly reset the object to its initial rotation
    public void ResetRotation()
    {
        if (!_isRotating)
        {
            _isRotating = true;
            _isRotatingToTarget = false; // Rotate back to the start rotation
            onRotationStart.Invoke(); // Trigger the start event

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

    // Optional: Add a method to check if the object is at the start rotation
    public bool IsAtStartRotation()
    {
        return Quaternion.Angle(transform.rotation, _startRotation) < 0.1f;
    }

    // Optional: Add a method to check if the object is at the target rotation
    public bool IsAtTargetRotation()
    {
        return Quaternion.Angle(transform.rotation, Quaternion.Euler(targetRotation)) < 0.1f;
    }
}