using UnityEngine;
using UnityEngine.Events;
using TMPro; // For TextMeshPro support
using System.Collections.Generic; // For List

public class ValueBasedIncrement : MonoBehaviour
{
    [System.Serializable]
    public class ValueRequirement
    {
        public int targetValue; // The target value for this requirement
        public int currentValue; // The current value for this requirement
        public TextMeshPro valueDisplay; // The 3D TextMeshPro display for this value
        public bool wasCorrect; // Track if the value was previously correct
    }

    [Header("Puzzle Settings")]
    public List<ValueRequirement> valueRequirements = new List<ValueRequirement>(); // List of value requirements

    [Header("Increment/Decrement Settings")]
    public List<int> incrementOptions = new List<int> { 1 }; // List of increment amounts
    public List<int> decrementOptions = new List<int> { 1 }; // List of decrement amounts
    private int currentIncrementIndex = 0; // Index of the currently selected increment amount
    private int currentDecrementIndex = 0; // Index of the currently selected decrement amount

    [Header("Range Limits (Optional)")]
    public bool useRangeLimits = false; // Toggle range limits on/off
    public int minValue = 0; // Minimum value (if range limits are enabled)
    public int maxValue = 10; // Maximum value (if range limits are enabled)

    [Header("Failure Behavior")]
    public bool resetOnFailure = true; // Whether to reset values on failure

    [Header("Color Feedback (Optional)")]
    public bool enableColorFeedback = false; // Toggle color feedback on/off
    public Color correctColor = Color.green; // Color for correct values
    public Color incorrectColor = Color.white; // Color for incorrect values

    [Header("Automatic Success Event (Optional)")]
    public bool enableAutomaticSuccess = false; // Toggle automatic success event on/off
    public UnityEvent onAutomaticSuccess; // Triggered when all values match their targets automatically

    [Header("On Correct Then Changed Event (Optional)")]
    public bool enableOnCorrectThenChanged = false; // Toggle this feature on/off
    public UnityEvent onCorrectThenChanged; // Triggered when a correct value is changed to incorrect

    [Header("Submit Button (Optional)")]
    public bool enableSubmitButton = true; // Toggle submit button functionality on/off
    public UnityEvent onSuccess; // Triggered when all values match their targets (via submit)
    public UnityEvent onFailure; // Triggered when any value is incorrect (via submit)

    [Header("Sound Effects (Optional)")]
    public bool enableSoundEffects = false; // Toggle sound effects on/off
    public AudioClip incrementSound; // Sound for incrementing values
    public AudioClip decrementSound; // Sound for decrementing values
    public AudioClip successSound; // Sound for successful submission
    public AudioClip failureSound; // Sound for failed submission
    public AudioClip resetSound; // Sound for resetting values
    public AudioClip hintSound; // Sound for using a hint
    private AudioSource audioSource; // AudioSource component

    [Header("Hints System (Optional)")]
    public bool enableHints = false; // Toggle hints system on/off
    public int maxHints = 3; // Maximum number of hints allowed
    private int hintsUsed = 0; // Number of hints used

    [Header("Hint Visual Feedback (Optional)")]
    public bool enableHintVisualFeedback = false; // Toggle visual feedback for hints
    public Color hintHighlightColor = Color.yellow; // Color to highlight the corrected value
    public float hintHighlightDuration = 1f; // Duration of the highlight effect
    private Color originalColor; // Original color of the TextMeshPro display

    [Header("Progressive Hints (Optional)")]
    public bool enableProgressiveHints = false; // Toggle progressive hints on/off
    public string[] hintMessages; // Messages to display for progressive hints

    [Header("Debugging Tools (Optional)")]
    public bool debugMode = false; // Toggle debug mode on/off

    [Header("Events")]
    public UnityEvent onReset; // Triggered when the values are reset

    private void Start()
    {
        // Initialize AudioSource if sound effects are enabled
        if (enableSoundEffects)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Store the original color of the TextMeshPro display
        if (enableHintVisualFeedback && valueRequirements.Count > 0 && valueRequirements[0].valueDisplay != null)
        {
            originalColor = valueRequirements[0].valueDisplay.color;
        }

        // Initialize color feedback for all displays
        if (enableColorFeedback)
        {
            for (int i = 0; i < valueRequirements.Count; i++)
            {
                UpdateDisplayColor(i);
            }
        }

        // Initialize wasCorrect for all requirements
        foreach (var requirement in valueRequirements)
        {
            requirement.wasCorrect = (requirement.currentValue == requirement.targetValue);
        }
    }

    // Increase the value for a specific requirement using the current increment amount
    public void IncreaseValue(int requirementIndex)
    {
        if (requirementIndex >= 0 && requirementIndex < valueRequirements.Count)
        {
            var requirement = valueRequirements[requirementIndex];
            bool wasCorrectBefore = (requirement.currentValue == requirement.targetValue);

            int incrementAmount = incrementOptions[currentIncrementIndex];
            requirement.currentValue += incrementAmount;
            if (useRangeLimits)
            {
                requirement.currentValue = Mathf.Clamp(requirement.currentValue, minValue, maxValue);
            }
            UpdateDisplay(requirementIndex);

            // Play increment sound if enabled
            if (enableSoundEffects && incrementSound != null)
            {
                audioSource.PlayOneShot(incrementSound);
            }

            // Check for automatic success
            CheckForAutomaticSuccess();

            // Check if a correct value was changed to incorrect
            if (enableOnCorrectThenChanged && wasCorrectBefore && requirement.currentValue != requirement.targetValue)
            {
                onCorrectThenChanged.Invoke();
            }

            // Update wasCorrect
            requirement.wasCorrect = (requirement.currentValue == requirement.targetValue);
        }
        else
        {
            Debug.LogError("Invalid requirement index: " + requirementIndex);
        }
    }

    // Decrease the value for a specific requirement using the current decrement amount
    public void DecreaseValue(int requirementIndex)
    {
        if (requirementIndex >= 0 && requirementIndex < valueRequirements.Count)
        {
            var requirement = valueRequirements[requirementIndex];
            bool wasCorrectBefore = (requirement.currentValue == requirement.targetValue);

            int decrementAmount = decrementOptions[currentDecrementIndex];
            requirement.currentValue -= decrementAmount;
            if (useRangeLimits)
            {
                requirement.currentValue = Mathf.Clamp(requirement.currentValue, minValue, maxValue);
            }
            UpdateDisplay(requirementIndex);

            // Play decrement sound if enabled
            if (enableSoundEffects && decrementSound != null)
            {
                audioSource.PlayOneShot(decrementSound);
            }

            // Check for automatic success
            CheckForAutomaticSuccess();

            // Check if a correct value was changed to incorrect
            if (enableOnCorrectThenChanged && wasCorrectBefore && requirement.currentValue != requirement.targetValue)
            {
                onCorrectThenChanged.Invoke();
            }

            // Update wasCorrect
            requirement.wasCorrect = (requirement.currentValue == requirement.targetValue);
        }
        else
        {
            Debug.LogError("Invalid requirement index: " + requirementIndex);
        }
    }

    // Cycle to the next increment amount
    public void CycleIncrementAmount()
    {
        currentIncrementIndex = (currentIncrementIndex + 1) % incrementOptions.Count;
        Debug.Log("Current Increment Amount: " + incrementOptions[currentIncrementIndex]);
    }

    // Cycle to the next decrement amount
    public void CycleDecrementAmount()
    {
        currentDecrementIndex = (currentDecrementIndex + 1) % decrementOptions.Count;
        Debug.Log("Current Decrement Amount: " + decrementOptions[currentDecrementIndex]);
    }

    // Submit all values for evaluation
    public void Submit()
    {
        if (!enableSubmitButton) return; // Skip if submit button is disabled

        bool allValuesMatch = true;

        // Check if all current values match their target values
        foreach (var requirement in valueRequirements)
        {
            if (requirement.currentValue != requirement.targetValue)
            {
                allValuesMatch = false;
                break;
            }
        }

        if (allValuesMatch)
        {
            onSuccess.Invoke(); // Trigger success event

            // Play success sound if enabled
            if (enableSoundEffects && successSound != null)
            {
                audioSource.PlayOneShot(successSound);
            }
        }
        else
        {
            onFailure.Invoke(); // Trigger failure event

            // Play failure sound if enabled
            if (enableSoundEffects && failureSound != null)
            {
                audioSource.PlayOneShot(failureSound);
            }

            if (resetOnFailure) // Reset values only if resetOnFailure is true
            {
                ResetValues();
            }
        }
    }

    // Reset all values to 0
    public void ResetValues()
    {
        for (int i = 0; i < valueRequirements.Count; i++)
        {
            valueRequirements[i].currentValue = 0;
            UpdateDisplay(i);
        }
        onReset.Invoke(); // Trigger reset event

        // Play reset sound if enabled
        if (enableSoundEffects && resetSound != null)
        {
            audioSource.PlayOneShot(resetSound);
        }
    }

    // Provide a hint to the player
    public void ProvideHint()
    {
        if (enableHints && hintsUsed < maxHints)
        {
            foreach (var requirement in valueRequirements)
            {
                if (requirement.currentValue != requirement.targetValue)
                {
                    // Correct the value
                    requirement.currentValue = requirement.targetValue;
                    UpdateDisplay(valueRequirements.IndexOf(requirement));

                    // Play hint sound if enabled
                    if (enableSoundEffects && hintSound != null)
                    {
                        audioSource.PlayOneShot(hintSound);
                    }

                    // Show visual feedback if enabled
                    if (enableHintVisualFeedback && requirement.valueDisplay != null)
                    {
                        StartCoroutine(HighlightDisplay(requirement.valueDisplay));
                    }

                    // Display progressive hint message if enabled
                    if (enableProgressiveHints && hintMessages != null && hintMessages.Length > hintsUsed)
                    {
                        Debug.Log("Hint: " + hintMessages[hintsUsed]);
                    }

                    hintsUsed++;
                    Debug.Log($"Hint used! ({hintsUsed}/{maxHints})");
                    break;
                }
            }

            // Check for automatic success after using a hint
            CheckForAutomaticSuccess();
        }
        else
        {
            Debug.Log("No hints remaining or hints system disabled.");
        }
    }

    // Highlight the TextMeshPro display for a duration
    private System.Collections.IEnumerator HighlightDisplay(TextMeshPro display)
    {
        display.color = hintHighlightColor;
        yield return new WaitForSeconds(hintHighlightDuration);
        display.color = enableColorFeedback ? (display.text == display.GetComponent<ValueRequirement>().targetValue.ToString() ? correctColor : incorrectColor) : originalColor;
    }

    // Update the TextMeshPro display for a specific requirement
    private void UpdateDisplay(int requirementIndex)
    {
        if (requirementIndex >= 0 && requirementIndex < valueRequirements.Count)
        {
            var requirement = valueRequirements[requirementIndex];
            if (requirement.valueDisplay != null)
            {
                requirement.valueDisplay.text = requirement.currentValue.ToString();
                if (enableColorFeedback)
                {
                    UpdateDisplayColor(requirementIndex);
                }
            }
        }
    }

    // Update the color of the TextMeshPro display based on correctness
    private void UpdateDisplayColor(int requirementIndex)
    {
        var requirement = valueRequirements[requirementIndex];
        if (requirement.valueDisplay != null)
        {
            if (requirement.currentValue == requirement.targetValue)
            {
                requirement.valueDisplay.color = correctColor;
            }
            else
            {
                requirement.valueDisplay.color = incorrectColor;
            }
        }
    }

    // Check if all values match their targets and trigger automatic success event
    private void CheckForAutomaticSuccess()
    {
        if (enableAutomaticSuccess)
        {
            bool allValuesMatch = true;

            foreach (var requirement in valueRequirements)
            {
                if (requirement.currentValue != requirement.targetValue)
                {
                    allValuesMatch = false;
                    break;
                }
            }

            if (allValuesMatch)
            {
                onAutomaticSuccess.Invoke(); // Trigger automatic success event

                // Play success sound if enabled
                if (enableSoundEffects && successSound != null)
                {
                    audioSource.PlayOneShot(successSound);
                }
            }
        }
    }

    // Debugging tools
    private void OnGUI()
    {
        if (debugMode)
        {
            GUILayout.Label("Debug Mode");
            for (int i = 0; i < valueRequirements.Count; i++)
            {
                GUILayout.Label($"Requirement {i}: Current = {valueRequirements[i].currentValue}, Target = {valueRequirements[i].targetValue}");
            }
            GUILayout.Label($"Hints Used: {hintsUsed}/{maxHints}");
        }
    }
}