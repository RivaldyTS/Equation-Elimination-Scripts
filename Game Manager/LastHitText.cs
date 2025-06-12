using UnityEngine;
using TMPro;

public class LastHitText : MonoBehaviour
{
    public float popInTime = 0.2f;   // Time for the initial pop-in
    public float pulseTime = 0.3f;   // Time for the pulse animation (scale down and back up)
    public float riseTime = 0.5f;    // Time for the rise and fade-out
    public float baseScaleMultiplier = 1.5f; // Base scale multiplier for the text
    public float pulseScaleDip = 0.8f;   // How much the text scales down during the pulse (relative to scaleMultiplier)
    public float riseDistance = 1f;      // How far the text rises (in world units)
    public Color startColor = Color.yellow; // Starting color (gold-like)
    public Color endColor = Color.white;    // Color to fade to

    private TextMeshProUGUI textMesh; // Single TextMeshProUGUI for the whole word
    private float elapsedTime = 0f;
    private Vector3 startWorldPosition; // Starting position in world space
    private Camera mainCamera;
    private float scaleMultiplier; // Adjusted scale multiplier based on word length

    // Static fields to manage the single shared last hit text instance
    private static TextMeshProUGUI sharedTextInstance;
    private static LastHitText sharedTextComponent;

    // Array of possible last hit texts
    private readonly string[] lastHitTexts = new string[]
    {
        "Mati",      // "Dead" in Indonesian
        "Tewas",     // "Killed" in Indonesian
        "Hancur",    // "Destroyed" in Indonesian
        "Lenyap",    // "Vanished" in Indonesian
        "Cooked"     // Slang for thoroughly defeated
    };

    public static void UpdateSharedText(TextMeshProUGUI textPrefab, Transform uiCanvas, Vector3 worldPosition)
    {
        // If the shared instance doesn't exist, create it by instantiating the prefab
        if (sharedTextInstance == null)
        {
            sharedTextInstance = Instantiate(textPrefab, uiCanvas);
            sharedTextInstance.gameObject.name = "SharedLastHitText";
            sharedTextComponent = sharedTextInstance.gameObject.AddComponent<LastHitText>();
        }

        // Update the shared instance
        sharedTextComponent.SetTextInternal(worldPosition);
    }

    private void SetTextInternal(Vector3 worldPosition)
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
            Destroy(gameObject);
            return;
        }
        textMesh = sharedTextInstance;
        gameObject.transform.SetParent(textMesh.transform.parent);

        string selectedText = lastHitTexts[Random.Range(0, lastHitTexts.Length)];
        textMesh.text = selectedText;

        scaleMultiplier = baseScaleMultiplier * Mathf.Lerp(1f, 0.6f, (selectedText.Length - 3f) / (9f - 3f));
        textMesh.fontSize = textMesh.fontSize / baseScaleMultiplier * scaleMultiplier;
        textMesh.color = startColor;

        startWorldPosition = worldPosition;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(startWorldPosition);
        textMesh.transform.position = screenPos;

        elapsedTime = 0f;
        textMesh.transform.localScale = Vector3.zero;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        float totalTime = popInTime + pulseTime + riseTime;

        if (textMesh == null) return;

        // Pop-in phase: Scale up with a slight overshoot
        float scale;
        if (elapsedTime < popInTime)
        {
            float t = elapsedTime / popInTime;
            scale = Mathf.Lerp(0f, scaleMultiplier * 1.2f, t); // Overshoot to 120%
            textMesh.transform.localScale = Vector3.one * scale;
        }
        // Pulse phase: Scale down and back up
        else if (elapsedTime < popInTime + pulseTime)
        {
            float pulseProgress = (elapsedTime - popInTime) / pulseTime;
            // Use a sine wave to create a smooth pulse effect (scale dips then returns)
            float pulseScale = Mathf.Sin(pulseProgress * Mathf.PI) * (scaleMultiplier - scaleMultiplier * pulseScaleDip) + scaleMultiplier * pulseScaleDip;
            scale = pulseScale;
            textMesh.transform.localScale = Vector3.one * scale;
        }
        // Rise and fade phase
        else
        {
            float riseProgress = (elapsedTime - (popInTime + pulseTime)) / riseTime;
            scale = scaleMultiplier; // Keep scale constant during rise
            textMesh.transform.localScale = Vector3.one * scale;

            // Move the text upward
            Vector3 newWorldPos = startWorldPosition + Vector3.up * (riseDistance * riseProgress);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(newWorldPos);
            textMesh.transform.position = screenPos;

            // Color transition and alpha fade
            Color color = Color.Lerp(startColor, endColor, riseProgress);
            color.a = Mathf.Lerp(1f, 0f, riseProgress);
            textMesh.color = color;
        }

        // Hide the text after the animation is complete (but don't destroy the shared instance)
        if (elapsedTime >= totalTime)
        {
            textMesh.transform.localScale = Vector3.zero;
            Color color = textMesh.color;
            color.a = 0f;
            textMesh.color = color;
            Destroy(gameObject); // Destroy the component, but not the shared text instance
        }
    }
}