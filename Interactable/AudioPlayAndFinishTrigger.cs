using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class AudioEventPair
{
    public AudioSource audioSource; // Reference to the AudioSource component
    public UnityEvent onAudioStarted; // Unity Event to trigger when audio starts playing
    public UnityEvent onAudioFinished; // Unity Event to trigger when audio finishes playing
}

public class AudioPlayAndFinishTrigger : MonoBehaviour // Inherits from MonoBehaviour
{
    public AudioEventPair[] audioEventPairs; // Array of AudioSource and UnityEvent pairs

    private bool[] wasPlayingArray; // Tracks whether each audio source was playing in the previous frame

    void Start()
    {
        wasPlayingArray = new bool[audioEventPairs.Length];
    }

    void Update()
    {
        for (int i = 0; i < audioEventPairs.Length; i++)
        {
            bool isPlaying = audioEventPairs[i].audioSource.isPlaying;
            if (isPlaying && !wasPlayingArray[i])
            {
                wasPlayingArray[i] = true;
                audioEventPairs[i].onAudioStarted.Invoke();
            }
            else if (!isPlaying && wasPlayingArray[i])
            {
                wasPlayingArray[i] = false;
                audioEventPairs[i].onAudioFinished.Invoke();
            }
        }
    }
}