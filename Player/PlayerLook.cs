using UnityEngine;
using TMPro;

public class PlayerLook : MonoBehaviour
{
    public Camera cam;
    private float xRotation = 0f;
    private float currentLean = 0f; // The current lean angle
    private Vector3 leanOffset = Vector3.zero; // The current lean position offset

    [Header("Sensitivity Settings")]
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;
    public float sensitivityStep = 5f; // Step size for increasing/decreasing sensitivity
    public float minSensitivity = 5f;  // Minimum sensitivity
    public float maxSensitivity = 100f; // Maximum sensitivity

    [Header("Leaning Settings")]
    public float leanAngle = 15f; // The angle to which the player can lean
    public float leanSpeed = 5f; // The speed of leaning
    public Vector3 leanLeftOffset = new Vector3(-0.5f, 0f, 0f); // Offset when leaning left (X, Y, Z)
    public Vector3 leanRightOffset = new Vector3(0.5f, 0f, 0f); // Offset when leaning right (X, Y, Z)

    [Header("Input Settings")]
    public KeyCode leanLeftKey = KeyCode.LeftShift; // Key for leaning left
    public KeyCode leanRightKey = KeyCode.C; // Key for leaning right
    public KeyCode increaseSensitivityKey = KeyCode.Equals; // Key for increasing sensitivity
    public KeyCode decreaseSensitivityKey = KeyCode.Minus; // Key for decreasing sensitivity

    [Header("UI Settings")]
    public TextMeshProUGUI sensitivityText; // TextMeshPro UI for sensitivity feedback
    public TextMeshProUGUI leanLeftText; // TextMeshPro UI for leaning left feedback
    public TextMeshProUGUI leanRightText; // TextMeshPro UI for leaning right feedback

    [Header("Bobbing Settings")]
    public float baseBobbingSpeed = 0.18f; // Base speed of the bobbing effect
    public float bobbingAmount = 0.2f; // Amount of bobbing
    public float midpoint = 1.7f; // Midpoint of the bobbing effect
    public float sprintBobbingMultiplier = 1.5f; // Bobbing speed multiplier when sprinting
    public float crouchBobbingMultiplier = 0.5f; // Bobbing speed multiplier when crouching

    private Vector3 originalLocalPosition;
    private float sensitivityTextFadeDuration = 2f; // Duration for sensitivity text to fade out
    private float sensitivityTextFadeTimer = 0f; // Timer for fading out sensitivity text
    private bool isSensitivityTextVisible = false; // Track if sensitivity text is visible
    private float timer = 0f; // Timer for the bobbing effect
    private PlayerMotor playerMotor; // Reference to PlayerMotor component
    private float currentBobbingSpeed; // Track the current bobbing speed

    void Start()
    {
        // Store the camera's original local position
        originalLocalPosition = cam.transform.localPosition;
        Debug.Log("Original Local Position: " + originalLocalPosition);

        // Get PlayerMotor component
        playerMotor = GetComponent<PlayerMotor>();

        // Initialize bobbing speed to base value
        currentBobbingSpeed = baseBobbingSpeed;
        Debug.Log("Initial Bobbing Speed: " + currentBobbingSpeed);

        // Initialize UI texts
        if (sensitivityText != null)
        {
            sensitivityText.gameObject.SetActive(false);
            sensitivityText.alpha = 1f; // Reset opacity
        }
        if (leanLeftText != null)
        {
            leanLeftText.gameObject.SetActive(false);
        }
        if (leanRightText != null)
        {
            leanRightText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        HandleSensitivityAdjustment();
        HandleLeaning();
        UpdateSensitivityText();
        HandleBobbing();
    }

    public void ProcessLook(Vector2 input, bool isScoped)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        // Adjust sensitivity based on scope state
        float sensitivityMultiplier = isScoped ? 0.5f : 1f;
        float adjustedXSensitivity = xSensitivity * sensitivityMultiplier;
        float adjustedYSensitivity = ySensitivity * sensitivityMultiplier;

        // Calculate camera rotation for looking up and down
        xRotation -= (mouseY * Time.deltaTime) * adjustedYSensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        // Apply camera transform
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, currentLean);

        // Rotate player to look left and right
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * adjustedXSensitivity);
    }

    private void HandleSensitivityAdjustment()
    {
        if (Input.GetKeyDown(decreaseSensitivityKey))
        {
            DecreaseSensitivity();
        }
        if (Input.GetKeyDown(increaseSensitivityKey))
        {
            IncreaseSensitivity();
        }
    }

    private void IncreaseSensitivity()
    {
        xSensitivity = Mathf.Clamp(xSensitivity + sensitivityStep, minSensitivity, maxSensitivity);
        ySensitivity = Mathf.Clamp(ySensitivity + sensitivityStep, minSensitivity, maxSensitivity);
        Debug.Log("Increased sensitivity: X=" + xSensitivity + ", Y=" + ySensitivity);

        // Show sensitivity text
        ShowSensitivityText();
    }

    private void DecreaseSensitivity()
    {
        xSensitivity = Mathf.Clamp(xSensitivity - sensitivityStep, minSensitivity, maxSensitivity);
        ySensitivity = Mathf.Clamp(ySensitivity - sensitivityStep, minSensitivity, maxSensitivity);
        Debug.Log("Decreased sensitivity: X=" + xSensitivity + ", Y=" + ySensitivity);

        // Show sensitivity text
        ShowSensitivityText();
    }

    private void ShowSensitivityText()
    {
        if (sensitivityText != null)
        {
            sensitivityText.text = $"Sensitivity: X={xSensitivity}, Y={ySensitivity}";
            sensitivityText.gameObject.SetActive(true);
            sensitivityText.alpha = 1f; // Reset opacity
            isSensitivityTextVisible = true;
            sensitivityTextFadeTimer = 0f; // Reset fade timer
        }
    }

    private void UpdateSensitivityText()
    {
        if (isSensitivityTextVisible && sensitivityText != null)
        {
            sensitivityTextFadeTimer += Time.deltaTime;
            if (sensitivityTextFadeTimer >= sensitivityTextFadeDuration)
            {
                // Fade out text
                sensitivityText.alpha -= Time.deltaTime;
                if (sensitivityText.alpha <= 0f)
                {
                    sensitivityText.gameObject.SetActive(false);
                    isSensitivityTextVisible = false;
                }
            }
        }
    }

    private void HandleLeaning()
    {
        float targetLean = 0f;
        Vector3 targetLeanOffset = Vector3.zero;

        // Determine the target lean angle and position based on input
        if (Input.GetKey(leanLeftKey))
        {
            targetLean = leanAngle; // Lean left
            targetLeanOffset = leanLeftOffset; // Use custom lean left offset

            // Show left lean text and hide right lean text
            if (leanLeftText != null)
            {
                leanLeftText.gameObject.SetActive(true);
            }
            if (leanRightText != null)
            {
                leanRightText.gameObject.SetActive(false);
            }
        }
        else if (Input.GetKey(leanRightKey))
        {
            targetLean = -leanAngle; // Lean right
            targetLeanOffset = leanRightOffset; // Use custom lean right offset

            // Show right lean text and hide left lean text
            if (leanRightText != null)
            {
                leanRightText.gameObject.SetActive(true);
            }
            if (leanLeftText != null)
            {
                leanLeftText.gameObject.SetActive(false);
            }
        }
        else
        {
            // No leaning, hide both texts
            if (leanLeftText != null)
            {
                leanLeftText.gameObject.SetActive(false);
            }
            if (leanRightText != null)
            {
                leanRightText.gameObject.SetActive(false);
            }
        }

        // Smoothly interpolate to the target lean angle and position
        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);
        leanOffset = Vector3.Lerp(leanOffset, targetLeanOffset, Time.deltaTime * leanSpeed);

        // Apply the lean to the camera's local rotation (Z-axis rotation)
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, currentLean);

        // Apply the lean offset to the camera's local position
        cam.transform.localPosition = originalLocalPosition + leanOffset;
    }

    private void HandleBobbing()
    {
        float waveslice = 0.0f;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Update bobbing speed based on PlayerMotor state
        if (playerMotor != null)
        {
            if (playerMotor.IsSprinting)
            {
                currentBobbingSpeed = baseBobbingSpeed * sprintBobbingMultiplier;
            }
            else if (playerMotor.IsCrouching)
            {
                currentBobbingSpeed = baseBobbingSpeed * crouchBobbingMultiplier;
            }
            else
            {
                currentBobbingSpeed = baseBobbingSpeed; // Default to base speed when neither sprinting nor crouching
            }
        }
        else
        {
            currentBobbingSpeed = baseBobbingSpeed; // Fallback if PlayerMotor is missing
        }
        
        if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
        {
            timer = 0.0f;
        }
        else
        {
            waveslice = Mathf.Sin(timer);
            timer = timer + currentBobbingSpeed;
            if (timer > Mathf.PI * 2)
            {
                timer = timer - (Mathf.PI * 2);
            }
        }

        if (waveslice != 0)
        {
            float translateChange = waveslice * bobbingAmount;
            float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
            translateChange = totalAxes * translateChange;
            Vector3 localPosition = cam.transform.localPosition;
            localPosition.y = midpoint + translateChange;
            cam.transform.localPosition = localPosition;
        }
        else
        {
            Vector3 localPosition = cam.transform.localPosition;
            localPosition.y = midpoint;
            cam.transform.localPosition = localPosition;
        }
    }
}