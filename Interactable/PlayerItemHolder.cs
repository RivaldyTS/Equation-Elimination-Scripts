using UnityEngine;

public class PlayerItemHolder : MonoBehaviour
{
    [SerializeField] private float currentItemPower = 0f; // Power value of the currently held item

    public bool HasItem()
    {
        return currentItemPower > 0f;
    }

    public float GetItemPower()
    {
        return currentItemPower;
    }

    public void RemoveItem()
    {
        currentItemPower = 0f;
    }

    public void PickupItem(float powerValue)
    {
        currentItemPower = powerValue;
    }
}