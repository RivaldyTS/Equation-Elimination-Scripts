using UnityEngine;
using UnityEngine.Events;
using TMPro; // For TextMeshPro support
using System.Collections.Generic; // For List

public class ValueBasedPuzzle : MonoBehaviour
{
    [System.Serializable]
    public class ValueRequirement
    {
        public int targetValue; // The target value for this requirement
        public int currentValue; // The current value for this requirement
        public TextMeshPro valueDisplay; // The 3D TextMeshPro display for this value
    }

    [Header("Puzzle Settings")]
    public List<ValueRequirement> valueRequirements = new List<ValueRequirement>(); // List of value requirements

    [Header("Increment/Decrement Settings")]
    public int incrementAmount = 1; // Amount to increase the value by
    public int decrementAmount = 1; // Amount to decrease the value by

    [Header("Range Limits (Optional)")]
    public bool useRangeLimits = false; // Toggle range limits on/off
    public int minValue = 0; // Minimum value (if range limits are enabled)
    public int maxValue = 10; // Maximum value (if range limits are enabled)

    [Header("Events")]
    public UnityEvent onSuccess; // Triggered when all values match their targets
    public UnityEvent onFailure; // Triggered when any value is incorrect
    public UnityEvent onReset; // Triggered when the values are reset

    // Increase the value for a specific requirement
    public void IncreaseValue(int requirementIndex)
    {
        if (requirementIndex >= 0 && requirementIndex < valueRequirements.Count)
        {
            valueRequirements[requirementIndex].currentValue += incrementAmount;
            if (useRangeLimits)
            {
                valueRequirements[requirementIndex].currentValue = Mathf.Clamp(valueRequirements[requirementIndex].currentValue, minValue, maxValue);
            }
            UpdateDisplay(requirementIndex);
        }
        else
        {
            Debug.LogError("Invalid requirement index: " + requirementIndex);
        }
    }

    // Decrease the value for a specific requirement
    public void DecreaseValue(int requirementIndex)
    {
        if (requirementIndex >= 0 && requirementIndex < valueRequirements.Count)
        {
            valueRequirements[requirementIndex].currentValue -= decrementAmount;
            if (useRangeLimits)
            {
                valueRequirements[requirementIndex].currentValue = Mathf.Clamp(valueRequirements[requirementIndex].currentValue, minValue, maxValue);
            }
            UpdateDisplay(requirementIndex);
        }
        else
        {
            Debug.LogError("Invalid requirement index: " + requirementIndex);
        }
    }

    // Submit all values for evaluation
    public void Submit()
    {
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
        }
        else
        {
            onFailure.Invoke(); // Trigger failure event
            ResetValues(); // Reset all values on failure
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
            }
        }
    }
}