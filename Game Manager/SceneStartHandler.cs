using UnityEngine;

public class SceneStartHandler : MonoBehaviour
{
    private void Start()
    {
        // Reset the scene resetting flag
        DestructibleObject.SetSceneResetting(false);
    }
}