using UnityEngine;

public class EmergencyLightEffect : MonoBehaviour
{
    public Material material; // Assign your material in the Inspector
    public Color emissionColor = Color.red; // Emission color (e.g., red for emergency light)
    public float minEmissionIntensity = 0.5f; // Minimum emission intensity
    public float maxEmissionIntensity = 2.0f; // Maximum emission intensity
    public float pulseSpeed = 1.0f; // Speed of the pulse effect

    private void Start()
    {
        if (material == null)
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                material = rend.material;
            }
        }

        // Enable emission on the material
        material.EnableKeyword("_EMISSION");
    }

    private void Update()
    {
        float emissionIntensity = Mathf.Lerp(minEmissionIntensity, maxEmissionIntensity, Mathf.PingPong(Time.time * pulseSpeed, 1.0f));
        Color finalEmissionColor = emissionColor * Mathf.LinearToGammaSpace(emissionIntensity);
        material.SetColor("_EmissionColor", finalEmissionColor);
        DynamicGI.SetEmissive(GetComponent<Renderer>(), finalEmissionColor);
    }
}