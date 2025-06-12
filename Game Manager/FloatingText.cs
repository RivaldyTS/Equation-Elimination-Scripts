using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float fadeInTime = 0.2f;
    public float stayTime = 0.5f;
    public float fadeOutTime = 0.5f;
    public float maxScale = 2f; // Reduced from 3f to 2f for smaller size
    public Vector2 offsetRange = new Vector2(3f, 3f);

    [Header("Distance Scaling")]
    public float minDistance = 5f;
    public float maxDistance = 20f;
    public float minScaleMultiplier = 0.6f;
    public float maxScaleMultiplier = 1.2f; // Reduced from 1.8f to 1.2f for smaller size at close range
    public float minFontSize = 18f;
    public float maxFontSize = 24f; // Reduced from 36f to 24f for smaller text at close range
    public Vector2 minOffsetRange = new Vector2(0.3f, 0.3f);

    private TextMeshProUGUI textMesh;
    private Camera mainCamera;
    private float elapsedTime = 0f;
    private float randomZRotation;
    private Vector3 worldPosition;
    private Vector2 randomOffset;
    private float distanceToCamera;
    private float adjustedMaxScale;
    private float adjustedFontSize;

    public void SetText(TextMeshProUGUI textComponent)
    {
        textMesh = textComponent;
        if (textMesh == null)
        {
            Debug.LogError("No TextMeshProUGUI given to " + gameObject.name + "!");
            Destroy(gameObject);
            return;
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
            Destroy(gameObject);
            return;
        }

        worldPosition = Camera.main.ScreenToWorldPoint(transform.position);
        distanceToCamera = Vector3.Distance(mainCamera.transform.position, worldPosition);

        float clampedDistance = Mathf.Clamp(distanceToCamera, minDistance, maxDistance);
        float distanceFactor = 1f - ((clampedDistance - minDistance) / (maxDistance - minDistance));

        float offsetX = Mathf.Lerp(minOffsetRange.x, offsetRange.x, distanceFactor);
        float offsetY = Mathf.Lerp(minOffsetRange.y, offsetRange.y, distanceFactor);
        Vector2 adjustedOffsetRange = new Vector2(offsetX, offsetY);
        randomOffset = new Vector2(
            Random.Range(-adjustedOffsetRange.x, adjustedOffsetRange.x),
            Random.Range(-adjustedOffsetRange.y, adjustedOffsetRange.y)
        );

        worldPosition.x += randomOffset.x;
        worldPosition.y += randomOffset.y;
        worldPosition.y += 0.5f;

        float scaleMultiplier = Mathf.Lerp(minScaleMultiplier, maxScaleMultiplier, distanceFactor);
        adjustedMaxScale = maxScale * scaleMultiplier;

        adjustedFontSize = Mathf.Lerp(minFontSize, maxFontSize, distanceFactor);
        textMesh.fontSize = adjustedFontSize;

        Debug.Log($"Animating text '{textMesh.text}' on {gameObject.name}. Distance: {distanceToCamera}, Offset: {randomOffset}, Scale: {adjustedMaxScale}, FontSize: {adjustedFontSize}");
    }

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        randomZRotation = Random.Range(-15f, 15f);
        transform.Rotate(0, 0, randomZRotation);
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        float totalTime = fadeInTime + stayTime + fadeOutTime;
        float t = elapsedTime / totalTime;

        worldPosition.y += moveSpeed * Time.deltaTime;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        transform.position = screenPos;

        float scale;
        if (elapsedTime < fadeInTime)
        {
            scale = Mathf.Lerp(0f, adjustedMaxScale, elapsedTime / fadeInTime);
        }
        else if (elapsedTime < fadeInTime + stayTime)
        {
            scale = adjustedMaxScale;
        }
        else
        {
            float fadeOutProgress = (elapsedTime - (fadeInTime + stayTime)) / fadeOutTime;
            scale = Mathf.Lerp(adjustedMaxScale, 0f, fadeOutProgress);
        }
        transform.localScale = Vector3.one * scale;

        if (textMesh != null)
        {
            Color color;
            if (elapsedTime < fadeInTime)
            {
                color = Color.Lerp(Color.red, Color.white, elapsedTime / fadeInTime);
            }
            else
            {
                color = Color.white;
            }

            float alpha;
            if (elapsedTime < fadeInTime)
            {
                alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInTime);
            }
            else if (elapsedTime < fadeInTime + stayTime)
            {
                alpha = 1f;
            }
            else
            {
                float fadeOutProgress = (elapsedTime - (fadeInTime + stayTime)) / fadeOutTime;
                alpha = Mathf.Lerp(1f, 0f, fadeOutProgress);
            }
            color.a = alpha;
            textMesh.color = color;
        }

        if (elapsedTime >= totalTime)
        {
            Destroy(gameObject);
        }
    }
}