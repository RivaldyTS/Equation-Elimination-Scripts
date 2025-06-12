using UnityEngine;
using TMPro; // Required for TextMeshPro

public class FaceCameraTextMeshPro : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Find the main camera in the scene
        mainCamera = Camera.main;

        // If no main camera is found, log an error
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found in the scene!");
        }
    }

    void Update()
    {
        // Ensure the text always faces the camera
        FaceCamera();
    }

    void FaceCamera()
    {
        if (mainCamera != null)
        {
            // Calculate the direction from the text to the camera
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;

            // Zero out the y-component to prevent tilting
            directionToCamera.y = 0;

            // Rotate the text to face the camera
            if (directionToCamera != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(-directionToCamera);
            }
        }
    }
}