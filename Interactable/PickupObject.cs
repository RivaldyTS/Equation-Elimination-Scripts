using UnityEngine;

public class PickupObject : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int value = 1;
    [SerializeField] private int hiddenValue = 1;
    [SerializeField] private string apparentValue = "1/1";
    [SerializeField] private string pickupTag = "Pickup";
    [SerializeField] private string promptMessage = "Press [E] to pick up";

    public int Value => value;
    public int HiddenValue => hiddenValue;
    public string ApparentValue => apparentValue;
    public string PromptMessage => promptMessage;

    private void OnValidate()
    {
        // Ensure the value and hiddenValue are at least 1
        value = Mathf.Max(1, value);
        hiddenValue = Mathf.Max(1, hiddenValue);
    }
}