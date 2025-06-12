using UnityEngine;
using TMPro;

public class PickupPromptUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI promptText; // Reference to the TextMeshPro UI element

    [Header("Player Settings")]
    [SerializeField] private Camera playerCamera; // Reference to the player's camera
    [SerializeField] private float maxDistance = 5f; // Maximum distance to detect pickable objects

    private void Update()
    {
        // Perform a raycast from the player's camera
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            // Check if the hit object has the PickupObject component
            PickupObject pickupObject = hit.collider.GetComponent<PickupObject>();
            if (pickupObject != null)
            {
                // Display the prompt message
                promptText.text = pickupObject.PromptMessage;
                promptText.gameObject.SetActive(true); // Enable the UI element
            }
            else
            {
                // Hide the prompt message if no pickable object is detected
                promptText.gameObject.SetActive(false);
            }
        }
        else
        {
            // Hide the prompt message if nothing is hit
            promptText.gameObject.SetActive(false);
        }
    }
}