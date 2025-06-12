using UnityEngine;
using UnityEngine.Rendering;

public class VolumeFader : MonoBehaviour
{
    public Volume globalVolume;
    public float fadeInDuration = 0.5f;
    public float holdDuration = 3f;
    public float fadeOutDuration = 1f;
    private float timer = 0f;
    private bool isFading = false;
    private enum FadeState { Idle, FadeIn, Hold, FadeOut }
    private FadeState state = FadeState.Idle;

    void Start()
    {
        if (globalVolume != null)
        {
            globalVolume.gameObject.SetActive(true);
            globalVolume.weight = 0f;
            Debug.Log("Global Volume activated and set to weight 0 at scene start.");
        }
        else
        {
            Debug.LogError("Global Volume not assigned in VolumeFader!");
        }
    }

    void Update()
    {
        if (!isFading) return;

        timer += Time.deltaTime;

        switch (state)
        {
            case FadeState.FadeIn:
                float fadeInProgress = Mathf.Clamp01(timer / fadeInDuration);
                globalVolume.weight = Mathf.Lerp(0f, 1f, fadeInProgress);
                if (timer >= fadeInDuration)
                {
                    globalVolume.weight = 1f; // Force to 1
                    timer = 0f;
                    state = FadeState.Hold;
                }
                break;

            case FadeState.Hold:
                globalVolume.weight = 1f; // Ensure it stays at 1
                if (timer >= holdDuration)
                {
                    timer = 0f;
                    state = FadeState.FadeOut;
                }
                break;

            case FadeState.FadeOut:
                float fadeOutProgress = Mathf.Clamp01(timer / fadeOutDuration);
                globalVolume.weight = Mathf.Lerp(1f, 0f, fadeOutProgress);
                if (timer >= fadeOutDuration)
                {
                    globalVolume.weight = 0f; // Force to 0
                    isFading = false;
                    state = FadeState.Idle;
                }
                break;
        }
    }

    public void TriggerHorrorEffect()
    {
        if (!isFading)
        {
            isFading = true;
            timer = 0f;
            state = FadeState.FadeIn;
            Debug.Log("Horror effect triggered!");
        }
        else
        {
            Debug.Log("Horror effect already active, extending hold.");
            timer = 0f;
            state = FadeState.Hold;
        }
    }
}