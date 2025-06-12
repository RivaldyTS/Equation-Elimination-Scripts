using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class RangeDisplayWithDynamicTextActivation : MonoBehaviour
{
    [Header("Player and Range Settings")]
    public Transform player;
    public float range = 10f;

    [Header("Main Text Settings")]
    public TextMeshProUGUI distanceText;
    public string textFormat = "This text can be edited: {0}m, the range";
    public bool enableFlicker = true;
    public float flickerDuration = 1f;

    [Header("Additional Text Settings")]
    public TextMeshProUGUI additionalText;
    public bool showAdditionalText = true;
    public string additionalTextFormat = "This game object is nearby";

    [Header("Proximity Text Settings")]
    public TextMeshProUGUI proximityText;
    public bool enableProximityText = true;
    public string proximityTextFormat = "You are very close!";
    public float proximityRange = 5f;

    [Header("Fade Settings")]
    public float fadeOutDuration = 1f;

    [Header("Directional Guide Settings")]
    public RectTransform guideArrow;
    public float guideEdgeOffset = 50f;
    public bool enableArrowScaling = true;
    public float minArrowSize = 0.5f;
    public float maxArrowSize = 2f;

    [Header("Enhanced Arrow Settings")]
    [Tooltip("Smooth arrow movement to new positions")]
    public bool enableArrowSmoothing = true;
    [Range(0.05f, 0.5f)]
    public float smoothTime = 0.1f;
    [Tooltip("Fade arrow opacity based on distance")]
    public bool enableArrowFade = false;
    [Range(0f, 1f)]
    public float minArrowAlpha = 0.3f;
    [Tooltip("Pulse arrow size for attention")]
    public bool enableArrowPulse = false;
    [Range(0.5f, 5f)]
    public float pulseSpeed = 2f;
    [Range(0f, 0.5f)]
    public float pulseAmplitude = 0.2f;
    [Tooltip("Place arrow in world space instead of screen space")]
    public bool useWorldSpaceArrow = false;
    [Range(1f, 5f)]
    public float worldSpaceDistanceFromPlayer = 5f;
    [Tooltip("Control how often the arrow updates (0 = every frame)")]
    public float arrowUpdateFrequency = 0f;

    [Header("Arrow Distance Indicator")]
    public TextMeshProUGUI arrowDistanceText;
    public bool showArrowDistance = false;
    public string arrowDistanceFormat = "{0}m";
    public Vector2 arrowDistanceOffset = new Vector2(20f, 20f);

    [Header("Camera Zoom Compensation")]
    public bool enableZoomCompensation = false;
    [Range(0.1f, 2f)]
    public float zoomScaleFactor = 1f;

    [Header("Height Indicator")]
    public bool enableHeightIndicator = false;
    public RectTransform heightIndicator;
    public float heightThreshold = 2f;
    public Vector2 heightOffset = new Vector2(0f, 30f);

    [Header("Behind Indicator")]
    public bool enableBehindIndicator = false;
    public RectTransform behindIndicator;
    public Vector2 behindOffset = new Vector2(0f, -20f);
    [Range(0f, 180f)]
    public float behindAngleThreshold = 90f;
    public Color behindColor = Color.yellow;

    [Header("Occlusion Handling")]
    public bool enableOcclusionHandling = false;
    public Color occludedColor = new Color(1f, 1f, 1f, 0.5f);

    [Header("Arrow Color Settings")]
    public Color farColor = Color.red;
    public Color closeColor = Color.green;

    [Header("Audio Feedback Settings")]
    public AudioSource audioSource;
    public AudioClip enterRangeSound;
    public AudioClip exitRangeSound;
    public AudioClip enterProximitySound;
    public AudioClip exitProximitySound;

    [Header("Line Renderer Settings")]
    public bool enableLineRenderer = false;
    public LineRenderer lineRenderer;
    public Color lineStartColor = Color.white;
    public Color lineEndColor = Color.yellow;

    [Header("Text Animation Settings")]
    public bool enableTextAnimation = false;
    public float textAnimationScale = 1.2f;
    public float textAnimationSpeed = 2f;

    [Header("Debug Mode")]
    public bool debugMode = false;

    [Header("Game Events")]
    public UnityEvent onEnterRange;
    public UnityEvent onExitRange;
    public UnityEvent onEnterProximity;
    public UnityEvent onExitProximity;

    [Header("Outline Highlight Settings")]
    public bool enableOutlineHighlight = false;
    public Outline outline;

    [Header("Time-Based Proximity Alerts")]
    public bool enableProximityAlert = false;
    public float proximityAlertTime = 5f;
    public UnityEvent onProximityAlert;

    private float proximityAlertTimer = 0f;
    private bool isInRange = false;
    private bool isInProximityRange = false;
    private float flickerTimer = 0f;
    private float fadeOutTimer = 0f;
    private Color originalColor;
    private Color additionalTextOriginalColor;
    private Color proximityTextOriginalColor;
    private Vector3 arrowVelocity = Vector3.zero;
    private float arrowUpdateTimer = 0f;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (player == null || guideArrow == null) return;

        if (distanceText != null)
        {
            originalColor = distanceText.color;
            distanceText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            distanceText.gameObject.SetActive(false);
        }

        if (additionalText != null)
        {
            additionalTextOriginalColor = additionalText.color;
            additionalText.color = new Color(additionalTextOriginalColor.r, additionalTextOriginalColor.g, additionalTextOriginalColor.b, 0f);
            additionalText.gameObject.SetActive(false);
        }

        if (proximityText != null)
        {
            proximityTextOriginalColor = proximityText.color;
            proximityText.color = new Color(proximityTextOriginalColor.r, proximityTextOriginalColor.g, proximityTextOriginalColor.b, 0f);
            proximityText.gameObject.SetActive(false);
        }

        if (guideArrow != null)
        {
            guideArrow.gameObject.SetActive(false);
        }

        if (arrowDistanceText != null)
        {
            arrowDistanceText.gameObject.SetActive(false);
        }

        if (heightIndicator != null)
        {
            heightIndicator.gameObject.SetActive(false);
        }

        if (behindIndicator != null)
        {
            behindIndicator.gameObject.SetActive(false);
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (enableLineRenderer && lineRenderer != null)
        {
            lineRenderer.startColor = lineStartColor;
            lineRenderer.endColor = lineEndColor;
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }

        if (enableOutlineHighlight && outline != null)
        {
            outline.enabled = false;
        }
    }

    private void Update()
    {
        if (player == null || guideArrow == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        arrowUpdateTimer += Time.deltaTime;
        if (arrowUpdateFrequency <= 0f || arrowUpdateTimer >= arrowUpdateFrequency)
        {
            arrowUpdateTimer = 0f;

            if (distance <= range)
            {
                if (!isInRange)
                {
                    isInRange = true;
                    flickerTimer = 0f;
                    fadeOutTimer = 0f;

                    if (enterRangeSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(enterRangeSound);
                    }

                    onEnterRange.Invoke();

                    if (distanceText != null)
                    {
                        distanceText.gameObject.SetActive(true);
                        distanceText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
                    }

                    if (showAdditionalText && additionalText != null)
                    {
                        additionalText.gameObject.SetActive(true);
                        additionalText.color = new Color
                        (additionalTextOriginalColor.r, additionalTextOriginalColor.g, additionalTextOriginalColor.b, 1f);
                    }

                    if (guideArrow != null)
                    {
                        guideArrow.gameObject.SetActive(true);
                    }

                    if (enableLineRenderer && lineRenderer != null)
                    {
                        lineRenderer.enabled = true;
                    }

                    if (enableOutlineHighlight && outline != null)
                    {
                        outline.enabled = true;
                    }
                }

                if (enableProximityText && distance <= proximityRange)
                {
                    if (!isInProximityRange)
                    {
                        isInProximityRange = true;

                        if (enterProximitySound != null && audioSource != null)
                        {
                            audioSource.PlayOneShot(enterProximitySound);
                        }

                        onEnterProximity.Invoke();

                        if (proximityText != null)
                        {
                            proximityText.gameObject.SetActive(true);
                            proximityText.text = proximityTextFormat;
                            proximityText.color = new Color
                            (proximityTextOriginalColor.r, proximityTextOriginalColor.g, proximityTextOriginalColor.b, 1f);
                        }

                        if (distanceText != null)
                        {
                            distanceText.gameObject.SetActive(false);
                        }
                        if (guideArrow != null)
                        {
                            guideArrow.gameObject.SetActive(false);
                        }
                    }

                    if (enableProximityAlert)
                    {
                        proximityAlertTimer += Time.deltaTime;
                        if (proximityAlertTimer >= proximityAlertTime)
                        {
                            onProximityAlert.Invoke();
                            proximityAlertTimer = 0f;
                        }
                    }
                }
                else
                {
                    if (isInProximityRange)
                    {
                        isInProximityRange = false;
                        proximityAlertTimer = 0f;

                        if (exitProximitySound != null && audioSource != null)
                        {
                            audioSource.PlayOneShot(exitProximitySound);
                        }

                        onExitProximity.Invoke();

                        if (proximityText != null)
                        {
                            proximityText.gameObject.SetActive(false);
                        }

                        if (distanceText != null)
                        {
                            distanceText.gameObject.SetActive(true);
                            distanceText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
                        }
                        if (guideArrow != null)
                        {
                            guideArrow.gameObject.SetActive(true);
                        }
                    }

                    if (distanceText != null)
                    {
                        distanceText.text = string.Format(textFormat, distance.ToString("F2"));
                    }

                    if (showAdditionalText && additionalText != null)
                    {
                        additionalText.text = additionalTextFormat;
                    }

                    if (enableFlicker && flickerTimer < flickerDuration)
                    {
                        flickerTimer += Time.deltaTime;
                        float alpha = Mathf.PingPong(Time.time * 10f, 1f);
                        if (distanceText != null)
                        {
                            distanceText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                        }
                        if (showAdditionalText && additionalText != null)
                        {
                            additionalText.color = new Color
                            (additionalTextOriginalColor.r, additionalTextOriginalColor.g, additionalTextOriginalColor.b, alpha);
                        }
                    }
                    else
                    {
                        if (distanceText != null)
                        {
                            distanceText.color = new Color
                            (originalColor.r, originalColor.g, originalColor.b, 1f);
                        }
                        if (showAdditionalText && additionalText != null)
                        {
                            additionalText.color = new Color
                            (additionalTextOriginalColor.r, additionalTextOriginalColor.g, additionalTextOriginalColor.b, 1f);
                        }
                    }

                    if (enableTextAnimation && distanceText != null)
                    {
                        float scale = Mathf.PingPong(Time.time * textAnimationSpeed, textAnimationScale - 1f) + 1f;
                        distanceText.transform.localScale = new Vector3(scale, scale, 1f);
                    }
                }
            }
            else
            {
                if (isInRange)
                {
                    isInRange = false;
                    fadeOutTimer = 0f;

                    if (exitRangeSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(exitRangeSound);
                    }

                    onExitRange.Invoke();
                }

                if (fadeOutTimer < fadeOutDuration)
                {
                    fadeOutTimer += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, fadeOutTimer / fadeOutDuration);
                    if (distanceText != null)
                    {
                        distanceText.color = new Color
                        (originalColor.r, originalColor.g, originalColor.b, alpha);
                    }
                    if (showAdditionalText && additionalText != null)
                    {
                        additionalText.color = new Color
                        (additionalTextOriginalColor.r, additionalTextOriginalColor.g, additionalTextOriginalColor.b, alpha);
                    }
                    if (proximityText != null)
                    {
                        proximityText.color = new Color
                        (proximityTextOriginalColor.r, proximityTextOriginalColor.g, proximityTextOriginalColor.b, alpha);
                    }
                }
                else
                {
                    if (distanceText != null)
                    {
                        distanceText.gameObject.SetActive(false);
                    }
                    if (showAdditionalText && additionalText != null)
                    {
                        additionalText.gameObject.SetActive(false);
                    }
                    if (proximityText != null)
                    {
                        proximityText.gameObject.SetActive(false);
                    }

                    if (guideArrow != null)
                    {
                        guideArrow.gameObject.SetActive(false);
                    }

                    if (enableLineRenderer && lineRenderer != null)
                    {
                        lineRenderer.enabled = false;
                    }

                    if (enableOutlineHighlight && outline != null)
                    {
                        outline.enabled = false;
                    }
                }
            }

            if (guideArrow != null && guideArrow.gameObject.activeSelf)
            {
                UpdateGuideArrow(distance);
            }

            if (enableLineRenderer && lineRenderer != null && lineRenderer.enabled)
            {
                lineRenderer.SetPosition(0, player.position);
                lineRenderer.SetPosition(1, transform.position);
            }

            if (debugMode)
            {
                Debug.Log($"Distance: {distance}, In Range: {isInRange}, In Proximity Range: {isInProximityRange}");
            }
        }
    }

    private void UpdateGuideArrow(float distance)
    {
        Vector3 screenPos;
        if (useWorldSpaceArrow)
        {
            Vector3 direction = (transform.position - player.position).normalized;
            Vector3 arrowPos = player.position + direction * worldSpaceDistanceFromPlayer;
            screenPos = mainCamera.WorldToScreenPoint(arrowPos);
        }
        else
        {
            screenPos = mainCamera.WorldToScreenPoint(transform.position);
        }

        Vector3 playerScreenPos = mainCamera.WorldToScreenPoint(player.position);
        bool isBehind = screenPos.z < 0;

        // Check if target is behind player
        Vector3 directionToTarget = (transform.position - player.position).normalized;
        Vector3 playerForward = player.forward;
        float dot = Vector3.Dot(directionToTarget, playerForward);
        bool isTargetBehind = enableBehindIndicator && dot < Mathf.Cos(behindAngleThreshold * Mathf.Deg2Rad);

        if (isBehind)
        {
            screenPos *= -1;
        }

        Vector3 targetPos = screenPos;
        if (!useWorldSpaceArrow)
        {
            // Original clamping behavior
            targetPos.x = Mathf.Clamp(targetPos.x, guideEdgeOffset, Screen.width - guideEdgeOffset);
            targetPos.y = Mathf.Clamp(targetPos.y, guideEdgeOffset, Screen.height - guideEdgeOffset);
        }

        if (enableArrowSmoothing)
        {
            guideArrow.position = Vector3.SmoothDamp
            (guideArrow.position, targetPos, ref arrowVelocity, smoothTime);
        }
        else
        {
            guideArrow.position = targetPos;
        }

        if (useWorldSpaceArrow)
        {
            Vector3 direction = (transform.position - player.position).normalized;
            direction.y = 0;
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            guideArrow.rotation = Quaternion.Euler(0, 0, -angle);
        }
        else
        {
            Vector3 direction = (transform.position - player.position).normalized;
            direction.y = 0;
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            guideArrow.rotation = Quaternion.Euler(0, 0, -angle);
        }

        float baseScale = enableArrowScaling ? Mathf.Lerp(maxArrowSize, minArrowSize, distance / range) : 1f;
        if (enableZoomCompensation)
        {
            float zoomFactor = mainCamera.orthographic ? 
                mainCamera.orthographicSize / 5f : 
                60f / mainCamera.fieldOfView;
            baseScale *= zoomScaleFactor * zoomFactor;
        }
        if (enableArrowPulse)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
            float finalScale = baseScale + (pulse * baseScale);
            guideArrow.localScale = new Vector3(finalScale, finalScale, 1f);
        }
        else
        {
            guideArrow.localScale = new Vector3(baseScale, baseScale, 1f);
        }

        if (guideArrow.GetComponent<Image>() != null)
        {
            Image arrowImage = guideArrow.GetComponent<Image>();
            float t = Mathf.InverseLerp(range, proximityRange, distance);
            Color baseColor = Color.Lerp(farColor, closeColor, t);
            float alpha = enableArrowFade ? Mathf.Lerp(minArrowAlpha, 1f, 1f - (distance / range)) : 1f;
            if (enableOcclusionHandling && Physics.Linecast(player.position, transform.position))
            {
                baseColor = occludedColor;
            }
            else if (isTargetBehind)
            {
                baseColor = behindColor;
            }
            arrowImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        }

        if (showArrowDistance && arrowDistanceText != null)
        {
            arrowDistanceText.gameObject.SetActive(true);
            arrowDistanceText.text = string.Format(arrowDistanceFormat, distance.ToString("F1"));
            arrowDistanceText.transform.position = guideArrow.position + (Vector3)arrowDistanceOffset;
        }

        if (enableHeightIndicator && heightIndicator != null)
        {
            float heightDiff = transform.position.y - player.position.y;
            if (Mathf.Abs(heightDiff) > heightThreshold)
            {
                heightIndicator.gameObject.SetActive(true);
                heightIndicator.position = guideArrow.position + (Vector3)heightOffset;
                heightIndicator.rotation = Quaternion.Euler(0, 0, heightDiff > 0 ? 0 : 180);
            }
            else
            {
                heightIndicator.gameObject.SetActive(false);
            }
        }

        if (enableBehindIndicator && behindIndicator != null)
        {
            if (isTargetBehind)
            {
                behindIndicator.gameObject.SetActive(true);
                behindIndicator.position = guideArrow.position + (Vector3)behindOffset;
            }
            else
            {
                behindIndicator.gameObject.SetActive(false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);

        if (enableProximityText)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, proximityRange);
        }
    }

    private void OnDisable()
    {
        if (distanceText != null)
        {
            distanceText.gameObject.SetActive(false);
        }

        if (showAdditionalText && additionalText != null)
        {
            additionalText.gameObject.SetActive(false);
        }

        if (proximityText != null)
        {
            proximityText.gameObject.SetActive(false);
        }

        if (guideArrow != null)
        {
            guideArrow.gameObject.SetActive(false);
        }

        if (arrowDistanceText != null)
        {
            arrowDistanceText.gameObject.SetActive(false);
        }

        if (heightIndicator != null)
        {
            heightIndicator.gameObject.SetActive(false);
        }

        if (behindIndicator != null)
        {
            behindIndicator.gameObject.SetActive(false);
        }

        if (enableLineRenderer && lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        isInRange = false;
        isInProximityRange = false;
        fadeOutTimer = 0f;
        flickerTimer = 0f;
        proximityAlertTimer = 0f;

        if (isInRange)
        {
            onExitRange.Invoke();
        }
        if (isInProximityRange)
        {
            onExitProximity.Invoke();
        }
    }
}