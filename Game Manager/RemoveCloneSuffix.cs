using UnityEngine;

public class RemoveCloneSuffix : MonoBehaviour
{
    private float timer = 0f;
    private const float INTERVAL = 0.5f; // Check every 0.5 seconds

    void Update()
    {
        // Increment timer based on time passed since last frame
        timer += Time.deltaTime;

        // Check if 0.5 seconds have passed
        if (timer >= INTERVAL)
        {
            #pragma warning disable CS0618 // Disable obsolete warning for FindObjectsOfType
            // Find all GameObjects in the scene
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            #pragma warning restore CS0618 // Restore warning after this block

            // Loop through each GameObject
            foreach (GameObject obj in allObjects)
            {
                // Check if the name contains "(Clone)"
                if (obj.name.Contains("(Clone)"))
                {
                    // Remove "(Clone)" from the name
                    obj.name = obj.name.Replace("(Clone)", "").Trim();
                }
            }

            // Reset timer
            timer = 0f;
        }
    }
}