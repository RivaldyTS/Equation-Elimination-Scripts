using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    public int scoreValue = 10; // Points to add when this object is destroyed

    private static bool isSceneResetting = false; // Static flag to track scene reset

    // Call this method when resetting the scene
    public static void SetSceneResetting(bool resetting)
    {
        isSceneResetting = resetting;
    }

    private void OnDestroy()
    {
        // Only add score if the object is not being destroyed due to a scene reset
        if (!isSceneResetting)
        {
            // Find the ScoreManager in the scene and add the score
            ScoreManager scoreManager = FindAnyObjectByType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddScore(scoreValue);
            }
        }
    }
}