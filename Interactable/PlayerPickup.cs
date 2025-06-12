using UnityEngine;
using UnityEngine.Events;

public class PlayerPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private KeyCode pickupKey = KeyCode.E; // Key to pick up/drop the object
    [SerializeField] private KeyCode throwKey = KeyCode.Q; // Key to throw the object
    [SerializeField] private float pickupRange = 3f; // Maximum distance to pick up the object
    [SerializeField] private Transform holdPosition; // Position where the object will be held
    [SerializeField] private string pickupTag = "Pickup"; // Tag of objects that can be picked up
    [SerializeField] private float throwForce = 10f; // Force applied when throwing the object
    [SerializeField] private float spinTorque = 100f; // Torque applied to make the object spin

    [Header("Jiggle Settings")]
    [SerializeField] private float jiggleIntensity = 0.1f; // How much the object jiggles
    [SerializeField] private float jiggleSpeed = 5f; // How fast the object jiggles

    [Header("Audio Settings")]
    [SerializeField] private AudioClip pickupSound; // Sound to play when picking up an object
    [SerializeField] private AudioClip dropSound; // Sound to play when dropping an object
    [SerializeField] private AudioClip throwSound; // Sound to play when throwing an object
    [SerializeField] private AudioSource audioSource; // AudioSource to play the sounds

    [Header("Events")]
    public UnityEvent onPickup; // Event triggered when an item is picked up
    public UnityEvent onDrop; // Event triggered when an item is dropped
    public UnityEvent onThrow; // Event triggered when an item is thrown

    private GameObject heldObject; // The object currently being held
    private PickupObject heldPickupObject; // The PickupObject component of the held object
    private bool isHolding = false; // Track if the player is holding an object
    private Vector3 originalHoldPosition; // Original position of the hold position

    void Start()
    {
        // Store the original hold position
        originalHoldPosition = holdPosition.localPosition;

        // Ensure there's an AudioSource component
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Check for pickup/drop input
        if (Input.GetKeyDown(pickupKey))
        {
            if (isHolding)
            {
                DropObject(); // Drop the object if already holding one
            }
            else
            {
                TryPickupObject(); // Try to pick up an object if not holding one
            }
        }

        // Check for throw input
        if (isHolding && Input.GetKeyDown(throwKey))
        {
            ThrowObject(); // Throw the object if holding one
        }

        // If holding an object, move it to the hold position and apply jiggle
        if (isHolding)
        {
            ApplyJiggleEffect();
        }
    }

    private void TryPickupObject()
    {
        // Perform a raycast to check for pickable objects
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            // Check if the hit object has the correct tag and a PickupObject component
            if (hit.collider.CompareTag(pickupTag))
            {
                PickupObject pickupObject = hit.collider.GetComponent<PickupObject>();
                if (pickupObject != null)
                {
                    PickupObject(hit.collider.gameObject, pickupObject);
                }
            }
        }
    }

    private void PickupObject(GameObject objectToPickup, PickupObject pickupObject)
    {
        // Set the object as the held object
        heldObject = objectToPickup;
        heldPickupObject = pickupObject;

        // Disable physics and parenting to the hold position
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        heldObject.transform.SetParent(holdPosition);

        // Play the pickup sound
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        isHolding = true;
        Debug.Log("Picked up: " + heldObject.name + " (Value: " + heldPickupObject.Value + ")");

        // Trigger the pickup event
        onPickup.Invoke();
    }

    // Public method to drop the held object
    public void DropObject()
    {
        if (!isHolding) return; // Exit if not holding an object

        // Re-enable physics and unparent the object
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        heldObject.transform.SetParent(null);

        // Play the drop sound
        if (dropSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dropSound);
        }

        // Reset held object references
        heldObject = null;
        heldPickupObject = null;

        isHolding = false;
        Debug.Log("Dropped object.");

        // Trigger the drop event
        onDrop.Invoke();
    }

    // Public method to throw the held object
    public void ThrowObject()
    {
        if (!isHolding) return; // Exit if not holding an object

        // Re-enable physics and unparent the object
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        heldObject.transform.SetParent(null);

        // Apply force to throw the object
        Vector3 throwDirection = Camera.main.transform.forward;
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

        // Apply torque to make the object spin
        Vector3 randomTorque = new Vector3(
            Random.Range(-spinTorque, spinTorque),
            Random.Range(-spinTorque, spinTorque),
            Random.Range(-spinTorque, spinTorque)
        );
        rb.AddTorque(randomTorque, ForceMode.Impulse);

        // Play the throw sound
        if (throwSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(throwSound);
        }
        // Reset held object references
        heldObject = null;
        heldPickupObject = null;
        isHolding = false;
        // Trigger the throw event
        onThrow.Invoke();
    }

    // Apply jiggle effect to the held object
    private void ApplyJiggleEffect()
    {
        if (heldObject == null) return;

        // Calculate jiggle offset using a sine wave
        float jiggleOffset = Mathf.Sin(Time.time * jiggleSpeed) * jiggleIntensity;

        // Apply the jiggle effect to the hold position
        holdPosition.localPosition = originalHoldPosition + new Vector3(0, jiggleOffset, 0);

        // Move the held object to the jiggled hold position
        heldObject.transform.position = holdPosition.position;
        heldObject.transform.rotation = holdPosition.rotation;
    }

    // Public method to get the held object
    public GameObject GetHeldObject()
    {
        return isHolding ? heldObject : null;
    }

    // Public method to get the value of the held object
    public int GetHeldObjectValue()
    {
        return isHolding ? heldPickupObject.Value : 0;
    }

    // Optional: Draw the pickup range in the editor for debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }

    internal int GetHeldObjectHiddenValue()
    {
        throw new System.NotImplementedException();
    }
}