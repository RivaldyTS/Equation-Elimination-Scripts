using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void ResetScene()
    {
        // Notify DestructibleObject that the scene is resetting
        DestructibleObject.SetSceneResetting(true);

        // Reset the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}