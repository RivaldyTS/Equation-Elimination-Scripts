using UnityEngine;
using TMPro;

public class HPInfoText : MonoBehaviour
{
    public float fadeInTime = 0.2f;  // Fast fade-in
    public float stayTime = 2f;      // Time to stay visible
    public float fadeOutTime = 0.5f; // Fade-out time

    private TextMeshProUGUI textMesh;
    private float elapsedTime = 0f;

    public void SetText(TextMeshProUGUI textComponent, string enemyName, float currentHP, float maxHP)
    {
        textMesh = textComponent;
        if (textMesh == null)
        {
            Debug.LogError("No TextMeshProUGUI given to " + gameObject.name + "!");
            return;
        }
        // HP (name, "enemy HP: 120/200")
        textMesh.text = $"{enemyName} HP: {Mathf.FloorToInt(currentHP)}/{Mathf.FloorToInt(maxHP)}";
        RectTransform rectTransform = textMesh.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(2, -352);
        // Reset the animation timer
        elapsedTime = 0f;
        Color color = textMesh.color;
        color.a = 0f;
        textMesh.color = color;
    }

    void Start()
    {
        // Remove "(Clone)" from GameObject name if present
        string cleanName = gameObject.name;
        if (cleanName.EndsWith("(Clone)"))
        {
            cleanName = cleanName.Substring(0, cleanName.Length - "(Clone)".Length).Trim();
        }
        gameObject.name = string.IsNullOrEmpty(cleanName) ? "HPText" : cleanName;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        float totalTime = fadeInTime + stayTime + fadeOutTime;
        float t = elapsedTime / totalTime;

        if (textMesh != null)
        {
            Color color = textMesh.color;

            // Opacity: 0 → 1 during fade-in, hold at 1, then 1 → 0 during fade-out
            float alpha;
            if (elapsedTime < fadeInTime)
            {
                alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInTime);
            }
            else if (elapsedTime < fadeInTime + stayTime)
            {
                alpha = 1f; // Fully visible
            }
            else
            {
                float fadeOutProgress = (elapsedTime - (fadeInTime + stayTime)) / fadeOutTime;
                alpha = Mathf.Lerp(1f, 0f, fadeOutProgress);
            }
            color.a = alpha;
            textMesh.color = color;

            // Debug the name to confirm it's correct during runtime
            if (elapsedTime < fadeInTime)
            {
                Debug.Log($"HPInfoText GameObject name during runtime: {gameObject.name}");
            }
        }
    }
}