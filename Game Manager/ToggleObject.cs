using UnityEngine;

public class ToggleObject : MonoBehaviour
{
    // Reference to the GameObject you want to toggle
    public GameObject objectToToggle;

    private bool isObjectActive = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            isObjectActive = !isObjectActive;
            objectToToggle.SetActive(isObjectActive);
        }
    }
}