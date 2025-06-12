using UnityEngine;

public class EmergencyDirectionalLight : MonoBehaviour
{
    public Light directionalLight; // Assign the directional light in the Inspector
    public Color lightColor = Color.red; // Color of the emergency light
    public float minIntensity = 0.5f; // Minimum light intensity
    public float maxIntensity = 2.0f; // Maximum light intensity
    public float pulseSpeed = 1.0f; // Speed of the pulse effect

    private void Start()
    {
        if (directionalLight == null)
        {
            directionalLight = GetComponent<Light>();
            if (directionalLight == null)
            {
                Debug.LogError("No directional light found!");
                return;
            }
        }
        directionalLight.color = lightColor;
    }

    private void Update()
    {
        // Calculate the light intensity using sine wave
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PingPong(Time.time * pulseSpeed, 1.0f));
        directionalLight.intensity = intensity;
    }
}