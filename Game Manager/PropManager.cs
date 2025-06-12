using UnityEngine;

public class PropManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Distance Settings")]
    [SerializeField] private float appearDistance = 20f;
    [SerializeField] private float disappearDistance = 25f;
    [SerializeField, Range(0.1f, 2f)] private float checkInterval = 0.5f;

    [Header("Layer and Tag Settings")]
    [SerializeField] private LayerMask propLayer = ~0; // Layers to include
    [SerializeField] private string[] tagsToCull = { "Prop" }; // Array of tags to manage

    private GameObject[] managedObjects; // Objects to cull (based on tags and layers)
    private float sqrAppearDistance;
    private float sqrDisappearDistance;

    void Start()
    {
        // Validate player
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("PropManager: No Player found with 'Player' tag!", this);
                enabled = false;
                return;
            }
        }

        // Validate tags
        if (tagsToCull == null || tagsToCull.Length == 0)
        {
            Debug.LogWarning("PropManager: No tags specified in tagsToCull! Defaulting to 'Prop'.", this);
            tagsToCull = new string[] { "Prop" };
        }

        // Find all objects matching the tags and layers
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        managedObjects = System.Array.FindAll(allObjects, obj =>
        {
            foreach (string tag in tagsToCull)
            {
                if (obj.CompareTag(tag) && ((1 << obj.layer) & propLayer) != 0)
                    return true;
            }
            return false;
        });

        if (managedObjects.Length == 0)
        {
            Debug.LogWarning("PropManager: No objects found with specified tags and layers!", this);
        }

        // Pre-calculate squared distances
        sqrAppearDistance = appearDistance * appearDistance;
        sqrDisappearDistance = disappearDistance * disappearDistance;

        // Start periodic checking
        InvokeRepeating(nameof(CheckDistances), 0f, checkInterval);
    }

    void CheckDistances()
    {
        foreach (GameObject obj in managedObjects)
        {
            if (obj == null) continue;

            float sqrDistance = (player.position - obj.transform.position).sqrMagnitude;
            bool isActive = obj.activeSelf;

            if (isActive && sqrDistance >= sqrDisappearDistance)
            {
                obj.SetActive(false);
            }
            else if (!isActive && sqrDistance <= sqrAppearDistance)
            {
                obj.SetActive(true);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (managedObjects == null) return;
        foreach (GameObject obj in managedObjects)
        {
            if (obj == null) continue;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(obj.transform.position, appearDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(obj.transform.position, disappearDistance);
        }
    }
#endif
}