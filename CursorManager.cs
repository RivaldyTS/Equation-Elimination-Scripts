using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private bool isCursorVisible = false;

    void Start()
    {
        // Initially lock the cursor and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Toggle cursor visibility and lock state when Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isCursorVisible)
            {
                // Hide and lock the cursor
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                // Show and unlock the cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // Toggle the cursor visibility flag
            isCursorVisible = !isCursorVisible;
        }

        // Exit the game when the player presses the 'P' key
        if (Input.GetKeyDown(KeyCode.P))
        {
            ExitGame();
        }
    }

    public void ExitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop playing the scene in the editor
        #endif
    }
}
