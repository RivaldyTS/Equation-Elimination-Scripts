using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    public enum LeverState
    {
        Neutral, // Lever has not been interacted with yet
        Active,  // Lever is activated
        Inactive // Lever is deactivated
    }

    [Header("Lever Settings")]
    [SerializeField] private LeverState currentState = LeverState.Neutral; // Current state of the lever
    public float toggleCooldown = 1f; // Cooldown between toggles in seconds
    private float lastToggleTime = 0f; // Track the last time the lever was toggled

    [Header("Events")]
    public UnityEvent onFirstActivate; // Event triggered the first time the lever is activated
    public UnityEvent onActivate;      // Event triggered when the lever is activated
    public UnityEvent onDeactivate;   // Event triggered when the lever is deactivated

    /// <summary>
    /// Toggles the lever's state and triggers the appropriate event, with a cooldown.
    /// </summary>
    public void ToggleLever()
    {
        // Proceed with toggling the lever
        switch (currentState)
        {
            case LeverState.Neutral:
                // First interaction: Activate the lever
                currentState = LeverState.Active;
                onFirstActivate.Invoke(); // Trigger the "first activate" event
                onActivate.Invoke(); // Also trigger the regular "activate" event
                Debug.Log("Lever activated for the first time!");
                break;

            case LeverState.Active:
                // Toggle to inactive
                currentState = LeverState.Inactive;
                onDeactivate.Invoke(); // Trigger the "deactivate" event
                Debug.Log("Lever deactivated!");
                break;

            case LeverState.Inactive:
                // Toggle to active
                currentState = LeverState.Active;
                onActivate.Invoke(); // Trigger the "activate" event
                Debug.Log("Lever activated!");
                break;
        }
        // Update the last toggle time
        lastToggleTime = Time.time;
    }

    /// <summary>
    /// Sets the lever's state explicitly.
    /// </summary>
    /// <param name="active">True to activate, false to deactivate.</param>
    public void SetLeverState(bool active)
    {
        // Check cooldown for explicit state changes as well
        if (Time.time - lastToggleTime < toggleCooldown)
        {
            Debug.Log("Please wait before changing the lever state again!");
            return;
        }

        if (currentState == LeverState.Neutral)
        {
            // First interaction: Activate the lever
            currentState = active ? LeverState.Active : LeverState.Inactive;
            onFirstActivate.Invoke(); // Trigger the "first activate" event
            Debug.Log("Lever activated for the first time!");
        }

        if (active && currentState != LeverState.Active)
        {
            currentState = LeverState.Active;
            onActivate.Invoke(); // Trigger the "activate" event
            Debug.Log("Lever activated!");
        }
        else if (!active && currentState != LeverState.Inactive)
        {
            currentState = LeverState.Inactive;
            onDeactivate.Invoke(); // Trigger the "deactivate" event
            Debug.Log("Lever deactivated!");
        }

        // Update the last toggle time
        lastToggleTime = Time.time;
    }

    /// <summary>
    /// Returns the current state of the lever.
    /// </summary>
    /// <returns>The current state as LeverState.</returns>
    public LeverState GetLeverState()
    {
        return currentState;
    }
}