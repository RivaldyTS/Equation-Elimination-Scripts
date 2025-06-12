using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // Required for UnityEvent

public class RandomParticlePlayer : MonoBehaviour
{
    public ParticleSystem[] particleEffects; // Array to hold your particle effects
    public float minInterval = 2f; // Minimum time interval between particle effects
    public float maxInterval = 5f; // Maximum time interval between particle effects
    public UnityEvent onParticlePlay; // UnityEvent to trigger when a particle plays

    private int currentIndex = -1;
    private float timer = 0f;
    private float nextInterval = 0f;

    void Start()
    {
        if (particleEffects.Length == 0)
        {
            Debug.LogError("No particle effects assigned!");
            enabled = false; // Disable the script if no particle effects are assigned
        }

        // Set the initial random interval
        nextInterval = GetRandomInterval();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= nextInterval)
        {
            PlayRandomParticle();
            timer = 0f;
            nextInterval = GetRandomInterval();
        }
    }

    void PlayRandomParticle()
    {
        // Stop the currently playing particle
        if (currentIndex != -1)
        {
            particleEffects[currentIndex].Stop();
        }
        int randomIndex = Random.Range(0, particleEffects.Length);
        particleEffects[randomIndex].Play();
        currentIndex = randomIndex;
        onParticlePlay.Invoke();
    }

    float GetRandomInterval()
    {
        return Random.Range(minInterval, maxInterval);
    }
}