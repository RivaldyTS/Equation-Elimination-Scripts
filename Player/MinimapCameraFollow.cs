using UnityEngine;
using UnityEngine.UI; // For RawImage and Image

public class MinimapCameraFollow : MonoBehaviour
{
    public Transform player; // Drag your player object here in the Inspector
    public float height = 20f; // How high the camera sits above the player (used if dynamic height is disabled)
    public RenderTexture minimapRenderTexture; // Assign your Render Texture here in the Inspector
    public bool rotateWithPlayer = true; // Toggle to enable/disable rotation with player
    public float positionSmoothSpeed = 5f; // Smoothing speed for position
    public float rotationSmoothSpeed = 5f; // Smoothing speed for rotation

    [Header("Minimap Visibility")]
    public KeyCode toggleMinimapKey = KeyCode.M; // Default key to toggle minimap visibility (M)
    private bool isMinimapVisible = true; // Tracks minimap visibility state
    public Image minimapBackgroundImage; // Assign your UI Image for the minimap background here in the Inspector

    [Header("Dynamic Height Adjustment")]
    public bool enableDynamicHeight = false; // Toggle to enable/disable dynamic height adjustment
    public float heightOffset = 20f; // Desired height above the terrain
    public float heightSmoothSpeed = 5f; // Smoothing speed for height adjustment
    public LayerMask terrainLayerMask; // Layers to check for terrain

    [Header("Minimap Zoom Settings")]
    public KeyCode zoomInKey = KeyCode.LeftBracket; // Default key for zooming in ([)
    public KeyCode zoomOutKey = KeyCode.RightBracket; // Default key for zooming out (])
    public KeyCode resetZoomKey = KeyCode.Backslash; // Default key for resetting zoom (\)
    public float defaultSize = 13.6f; // Default orthographic size
    public float minSize = 5f; // Minimum orthographic size (zoomed in)
    public float maxSize = 50f; // Maximum orthographic size (zoomed out)
    public float sizeStep = 1f; // How much the size changes per key press
    public float zoomSmoothSpeed = 5f; // Smoothing speed for zoom

    [Header("Auto-Zoom Settings")]
    public bool enableAutoZoom = false; // Toggle to enable/disable auto-zoom
    public float checkRadius = 10f; // Radius to check for nearby objects
    public float crampedThreshold = 5f; // Distance threshold for "cramped" (zoom in)
    public float wideThreshold = 15f; // Distance threshold for "wide" (zoom out)
    public float crampedZoomSize = 8f; // Zoom size when cramped
    public float wideZoomSize = 20f; // Zoom size when wide
    public LayerMask autoZoomLayerMask; // Layers to check for auto-zoom
    public float autoZoomCheckInterval = 0.1f; // How often to check for auto-zoom (seconds)

    [Header("Fog of War Settings")]
    public bool enableFogOfWar = false; // Toggle to enable/disable fog of war
    public RawImage minimapRawImage; // The UI Raw Image displaying the minimap
    public int fogTextureSize = 256; // Size of the fog texture (must match Render Texture size)
    public float revealRadius = 30f; // Radius around the player to reveal
    public float fogUpdateInterval = 0.1f; // How often to update the fog (seconds)
    public float mapWorldSize = 100f; // The world size the minimap covers (for mapping world to texture coords)
    public Vector2 mapWorldCenter = Vector2.zero; // The center of the game world in world coordinates

    [Header("Fog of War Debug Settings")]
    public bool debugDrawRevealArea = true; // Toggle to draw the reveal area in the Scene view
    [SerializeField] private Vector2 debugPlayerWorldPos; // Player's world position (X, Z)
    [SerializeField] private Vector2 debugOffsetPos; // Player's position offset by mapWorldCenter
    [SerializeField] private Vector2 debugNormalizedPos; // Normalized position (0-1)
    [SerializeField] private Vector2 debugTexturePos; // Texture position (0-fogTextureSize)
    [SerializeField] private float debugRevealRadiusPixels; // Reveal radius in pixels
    [SerializeField] private Vector2 debugRevealXRange; // X range of the reveal area
    [SerializeField] private Vector2 debugRevealYRange; // Y range of the reveal area

    private Camera minimapCamera;
    private Transform myTransform; // Cached transform for performance
    private float currentSize; // Current orthographic size of the camera
    private float targetSize; // Target size for smooth zooming
    private float autoZoomTimer; // Timer for auto-zoom checks
    private float currentHeight; // Current height of the camera
    private Texture2D fogTexture; // Texture for the fog of war
    private Material fogMaterial; // Material with the fog shader
    private float fogUpdateTimer; // Timer for fog updates

    void Start()
    {
        // Cache the transform
        myTransform = transform;

        // Get the Camera component
        minimapCamera = GetComponent<Camera>();
        if (minimapCamera == null)
        {
            //Debug.LogError("MinimapCameraFollow requires a Camera component on this GameObject!");
            enabled = false; // Disable the script to prevent errors
            return;
        }

        // Assign the Render Texture to the camera
        if (minimapRenderTexture != null)
        {
            minimapCamera.targetTexture = minimapRenderTexture;
        }
        else
        {
            //Debug.LogError("Minimap Render Texture is not assigned!");
        }

        // Set the initial size
        currentSize = defaultSize;
        targetSize = defaultSize;
        minimapCamera.orthographicSize = currentSize;

        // Set the initial height
        currentHeight = height;
        myTransform.position = new Vector3(player.position.x, currentHeight, player.position.z);

        // Ensure the minimap and its background are visible at start
        minimapCamera.enabled = isMinimapVisible;
        if (minimapBackgroundImage != null) minimapBackgroundImage.enabled = isMinimapVisible;

        // Initialize Fog of War
        if (enableFogOfWar)
        {
            InitializeFogOfWar();
        }
    }

    void InitializeFogOfWar()
    {
        if (minimapRawImage == null)
        {
            //Debug.LogError("Minimap Raw Image is not assigned for Fog of War!");
            enableFogOfWar = false;
            return;
        }

        // Create the fog texture
        fogTexture = new Texture2D(fogTextureSize, fogTextureSize, TextureFormat.RGBA32, false);
        Color[] colors = new Color[fogTextureSize * fogTextureSize];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.black; // Fully fogged (black)
        }
        fogTexture.SetPixels(colors);
        fogTexture.Apply();

        // Create a material with the fog shader
        fogMaterial = new Material(Shader.Find("Custom/MinimapFogOfWar"));
        if (fogMaterial == null)
        {
            //Debug.LogError("MinimapFogOfWar shader not found! Disabling Fog of War.");
            enableFogOfWar = false;
            return;
        }

        fogMaterial.SetTexture("_MainTex", minimapRenderTexture);
        fogMaterial.SetTexture("_FogTex", fogTexture);
        minimapRawImage.material = fogMaterial;

        //Debug.Log("Fog of War initialized successfully.");
    }

    void UpdateFogOfWar()
    {
        if (!enableFogOfWar || player == null) return;

        // Map the player's world position to texture coordinates
        debugPlayerWorldPos = new Vector2(player.position.x, player.position.z);
        debugOffsetPos = debugPlayerWorldPos - mapWorldCenter; // Offset by the world center
        debugNormalizedPos = (debugOffsetPos + Vector2.one * mapWorldSize * 0.5f) / mapWorldSize; // Normalize to 0-1
        debugTexturePos = debugNormalizedPos * fogTextureSize; // Scale to texture size

        //Debug.Log($"Player world pos: {debugPlayerWorldPos}, Offset pos: {debugOffsetPos}, Normalized pos: {debugNormalizedPos}, Texture pos: {debugTexturePos}");

        // Reveal the fog around the player's position
        debugRevealRadiusPixels = revealRadius / mapWorldSize * fogTextureSize;
        int revealRadiusPixels = Mathf.RoundToInt(debugRevealRadiusPixels);
        int xMin = Mathf.Max(0, Mathf.RoundToInt(debugTexturePos.x - revealRadiusPixels));
        int xMax = Mathf.Min(fogTextureSize - 1, Mathf.RoundToInt(debugTexturePos.x + revealRadiusPixels));
        int yMin = Mathf.Max(0, Mathf.RoundToInt(debugTexturePos.y - revealRadiusPixels));
        int yMax = Mathf.Min(fogTextureSize - 1, Mathf.RoundToInt(debugTexturePos.y + revealRadiusPixels));

        debugRevealXRange = new Vector2(xMin, xMax);
        debugRevealYRange = new Vector2(yMin, yMax);

        //Debug.Log($"Reveal radius (pixels): {debugRevealRadiusPixels}, X range: {xMin}-{xMax}, Y range: {yMin}-{yMax}");

        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), debugTexturePos);
                if (distance <= revealRadiusPixels)
                {
                    float alpha = Mathf.Clamp01(distance / revealRadiusPixels); // Fade effect
                    Color currentColor = fogTexture.GetPixel(x, y);
                    currentColor.a = Mathf.Min(currentColor.a, alpha); // Reveal by reducing alpha
                    fogTexture.SetPixel(x, y, currentColor);
                }
            }
        }

        fogTexture.Apply();
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Toggle minimap visibility
        if (Input.GetKeyDown(toggleMinimapKey))
        {
            isMinimapVisible = !isMinimapVisible;
            minimapCamera.enabled = isMinimapVisible;
            if (minimapRawImage != null) minimapRawImage.enabled = isMinimapVisible;
            if (minimapBackgroundImage != null) minimapBackgroundImage.enabled = isMinimapVisible; // Toggle the background image
        }

        if (!isMinimapVisible) return; // Skip updates if minimap is not visible

        // Handle manual zoom input
        if (Input.GetKeyDown(zoomInKey))
        {
            targetSize -= sizeStep; // Zoom in (decrease size)
            targetSize = Mathf.Clamp(targetSize, minSize, maxSize); // Keep within bounds
        }
        if (Input.GetKeyDown(zoomOutKey))
        {
            targetSize += sizeStep; // Zoom out (increase size)
            targetSize = Mathf.Clamp(targetSize, minSize, maxSize); // Keep within bounds
        }
        if (Input.GetKeyDown(resetZoomKey))
        {
            targetSize = defaultSize; // Reset to default size
        }

        // Auto-zoom logic
        if (enableAutoZoom)
        {
            autoZoomTimer += Time.deltaTime;
            if (autoZoomTimer >= autoZoomCheckInterval)
            {
                // Check for nearby objects in a sphere around the player
                Collider[] nearbyObjects = Physics.OverlapSphere(player.position, checkRadius, autoZoomLayerMask);
                float closestDistance = wideThreshold; // Default to wide threshold

                // Find the closest object
                foreach (Collider obj in nearbyObjects)
                {
                    if (obj.transform == player) continue; // Skip the player itself

                    // Calculate horizontal distance (ignore Y)
                    Vector3 objPos = obj.transform.position;
                    Vector3 playerPos = player.position;
                    Vector3 horizontalDiff = new Vector3(objPos.x - playerPos.x, 0, objPos.z - playerPos.z);
                    float distance = horizontalDiff.magnitude;

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                    }
                }

                // Adjust target size based on closest distance
                if (closestDistance <= crampedThreshold)
                {
                    targetSize = crampedZoomSize; // Zoom in for cramped areas
                }
                else if (closestDistance >= wideThreshold)
                {
                    targetSize = wideZoomSize; // Zoom out for wide areas
                }
                else
                {
                    // Interpolate between cramped and wide zoom sizes
                    float t = (closestDistance - crampedThreshold) / (wideThreshold - crampedThreshold);
                    targetSize = Mathf.Lerp(crampedZoomSize, wideZoomSize, t);
                }

                targetSize = Mathf.Clamp(targetSize, minSize, maxSize); // Keep within bounds
                autoZoomTimer = 0f; // Reset timer
            }
        }

        // Update Fog of War
        if (enableFogOfWar)
        {
            fogUpdateTimer += Time.deltaTime;
            if (fogUpdateTimer >= fogUpdateInterval)
            {
                UpdateFogOfWar();
                fogUpdateTimer = 0f;
            }
        }

        // Smoothly transition to the target size
        currentSize = Mathf.Lerp(currentSize, targetSize, zoomSmoothSpeed * Time.deltaTime);
        minimapCamera.orthographicSize = currentSize;

        // Dynamic height adjustment
        float targetHeight;
        if (enableDynamicHeight)
        {
            // Raycast downward to find the terrain height
            RaycastHit hit;
            if (Physics.Raycast(player.position, Vector3.down, out hit, 1000f, terrainLayerMask))
            {
                targetHeight = hit.point.y + heightOffset; // Set height above the terrain
            }
            else
            {
                targetHeight = height; // Fallback to default height if no terrain is found
            }
        }
        else
        {
            targetHeight = height; // Use fixed height
        }

        // Smoothly adjust the height
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, heightSmoothSpeed * Time.deltaTime);

        // Target position (follow player's X and Z, use dynamic or fixed Y)
        Vector3 targetPosition = new Vector3(player.position.x, currentHeight, player.position.z);
        myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, positionSmoothSpeed * Time.deltaTime);

        // Calculate the player's Y rotation more reliably
        Vector3 playerForward = player.forward;
        playerForward.y = 0; // Ignore vertical component
        float targetYRotation = Mathf.Atan2(playerForward.x, playerForward.z) * Mathf.Rad2Deg;

        // Target rotation (90 on X to look down, player's Y rotation, 0 on Z)
        Quaternion targetRotation;
        if (rotateWithPlayer)
        {
            targetRotation = Quaternion.Euler(90f, targetYRotation, 0f);
        }
        else
        {
            targetRotation = Quaternion.Euler(90f, 0f, 0f); // Static rotation (up is north)
        }

        // Smoothly rotate the camera
        myTransform.rotation = Quaternion.Slerp(myTransform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        if (!enableFogOfWar || player == null || !debugDrawRevealArea) return;

        // Draw the reveal area in the Scene view
        Gizmos.color = Color.green;
        Vector3 playerPos = new Vector3(player.position.x, 0, player.position.z); // Ignore Y for visualization
        Gizmos.DrawWireSphere(playerPos, revealRadius);
    }

    void OnDestroy()
    {
        // Clean up the fog texture and material
        if (fogTexture != null)
        {
            Destroy(fogTexture);
        }
        if (fogMaterial != null)
        {
            Destroy(fogMaterial);
        }
    }
}