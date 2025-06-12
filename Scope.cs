using System.Collections;
using UnityEngine;

public class Scope : MonoBehaviour
{
    public Animator animator;
    public GameObject scopeOverlay;
    public GameObject weaponCamera;
    public Camera mainCamera;

    public float scopedFOV = 15f;
    private float normalFOV;

    public bool isScoped = false;
    private bool isZooming = false; // Flag to indicate a zoom transition is in progress
    public float zoomDuration = 0.2f; // Duration of the zoom transition in seconds
    public float rolloffBase = 10000f; // Controls the steepness of the logarithmic rolloff
    public float minLogFactor = 0.01f; // Ensures the transition starts with at least 1% progress

    [Header("Camera Recoil Settings")]
    public float cameraRecoilAmount = 0.1f; // Amount of camera recoil (position)
    public float cameraRecoilSpeed = 2f; // Speed at which the camera returns to its original position
    public float scopedFovKickAmount = 2f; // FOV kick when shooting while scoped
    public float fovReturnSpeed = 5f; // Speed at which FOV returns to scopedFOV

    private Vector3 originalCameraPosition;
    private Vector3 recoilCameraPosition;

    private Gun gun; // Reference to the Gun script

    void Start()
    {
        // Set initial FOV
        normalFOV = mainCamera.fieldOfView;

        // Make sure the overlay is initially disabled
        scopeOverlay.SetActive(false);

        // Store original camera position
        originalCameraPosition = mainCamera.transform.localPosition;

        // Ensure weaponCamera is initially active
        if (weaponCamera != null)
        {
            weaponCamera.SetActive(true);
        }

        // Find the Gun script on one of the child weapons
        gun = GetComponentInChildren<Gun>();
    }

    void Update()
    {
        // Prevent scoping while reloading
        if (gun != null && gun.IsReloading())
        {
            return;
        }

        if (Input.GetButtonDown("Fire2"))
        {
            isScoped = !isScoped;
            animator.SetBool("Scoped", isScoped);

            if (isScoped)
            {
                StartCoroutine(ZoomIn());
            }
            else
            {
                StartCoroutine(ZoomOut());
            }
        }

        // Apply recoil to the camera if scoped
        if (isScoped && Input.GetButton("Fire1"))
        {
            ApplyCameraRecoil();
        }

        // Return camera to its original position smoothly
        mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition, originalCameraPosition, Time.deltaTime * cameraRecoilSpeed);

        // Smoothly return FOV to scopedFOV when scoped (and not zooming)
        if (isScoped && !isZooming && mainCamera.fieldOfView != scopedFOV)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, scopedFOV, Time.deltaTime * fovReturnSpeed);
            if (Mathf.Abs(mainCamera.fieldOfView - scopedFOV) < 0.1f)
            {
                mainCamera.fieldOfView = scopedFOV; // Snap to avoid floating point drift
            }
        }
    }

    // (fast at start, slow at end)
    private float LogarithmicRolloff(float t)
    {
        // Ensure t is between 0 and 1
        t = Mathf.Clamp01(t);
        // Use a logarithmic curve: minLogFactor + (1 - minLogFactor) * log(1 + t * (b - 1)) / log(b)
        // This creates a curve that starts fast and slows down as t approaches 1
        float scaled = 1f + t * (rolloffBase - 1f);
        float logValue = Mathf.Log(scaled) / Mathf.Log(rolloffBase);
        return minLogFactor + (1f - minLogFactor) * logValue;
    }

    public IEnumerator ZoomIn()
    {
        isZooming = true;

        // Wait for the animator transition (if any)
        yield return new WaitForSeconds(0.15f);

        float elapsedTime = 0f;
        float startFOV = mainCamera.fieldOfView;
        float targetFOV = scopedFOV;

        // Smoothly transition the FOV with true logarithmic rolloff
        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / zoomDuration;
            float logT = LogarithmicRolloff(t); // Apply true logarithmic rolloff
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, logT);
            Debug.Log($"ZoomIn - t: {t:F3}, logT: {logT:F3}, FOV: {mainCamera.fieldOfView:F1}");
            yield return null;
        }

        // Ensure the final FOV is exactly scopedFOV
        mainCamera.fieldOfView = scopedFOV;

        // Enable the scope overlay and disable the weapon camera
        scopeOverlay.SetActive(true);
        if (weaponCamera != null)
        {
            weaponCamera.SetActive(false);
        }

        isZooming = false;
    }

    public IEnumerator ZoomOut()
    {
        isZooming = true;

        float elapsedTime = 0f;
        float startFOV = mainCamera.fieldOfView;
        float targetFOV = normalFOV;

        // Smoothly transition the FOV with true logarithmic rolloff
        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / zoomDuration;
            float logT = LogarithmicRolloff(t); // Apply true logarithmic rolloff
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, logT);
            Debug.Log($"ZoomOut - t: {t:F3}, logT: {logT:F3}, FOV: {mainCamera.fieldOfView:F1}");
            yield return null;
        }

        // Ensure the final FOV is exactly normalFOV
        mainCamera.fieldOfView = normalFOV;

        // Disable the scope overlay and enable the weapon camera
        scopeOverlay.SetActive(false);
        if (weaponCamera != null)
        {
            weaponCamera.SetActive(true);
        }

        isZooming = false;
    }

    void ApplyCameraRecoil()
    {
        // Apply positional recoil (X and Y)
        recoilCameraPosition = new Vector3(
            Random.Range(-cameraRecoilAmount, cameraRecoilAmount),
            Random.Range(-cameraRecoilAmount, cameraRecoilAmount),
            0);

        mainCamera.transform.localPosition -= recoilCameraPosition;

        // Apply FOV kick when scoped
        mainCamera.fieldOfView = scopedFOV + scopedFovKickAmount;
        Debug.Log("Scoped FOV kicked to: " + mainCamera.fieldOfView);
    }

    // Public method to check if a zoom transition is in progress
    public bool IsZooming()
    {
        return isZooming;
    }
}