using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTimer : MonoBehaviour
{
    public float delay = 2f; // Time in seconds before switching scenes
    public string nextSceneName; // Name of the next scene to load

    void Start()
    {
        StartCoroutine(WaitAndChangeScene());
    }

    System.Collections.IEnumerator WaitAndChangeScene()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(nextSceneName);
    }
}