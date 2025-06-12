using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    void Update()
    {
        // Check if the right shift key is pressed
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            GoToMenuScene();
        }
    }

    // Function to load the Classic scene
    public void GoToClassicScene()
    {
        Debug.Log("Classic Button Clicked: Attempting to load Classic scene.");
        SceneManager.LoadScene("Classic");
    }

    // Function to load the Endless scene
    public void GoToEndlessScene()
    {
        Debug.Log("Endless Button Clicked: Attempting to load Endless scene.");
        SceneManager.LoadScene("Endless");
    }

    // Function to load the Menu scene
    public void GoToMenuScene()
    {
        Debug.Log("Menu Button Clicked: Attempting to load Menu scene.");
        SceneManager.LoadScene("Menu");
    }

    // Function to exit the application
    public void ExitGame()
    {
        Debug.Log("Exit Button Clicked: Attempting to exit the game.");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
