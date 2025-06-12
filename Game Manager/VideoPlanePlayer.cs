using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;

public class VideoPlanePlayer : MonoBehaviour
{
    public GameObject videoPlane; // The 3D plane to display the video
    public VideoPlayer videoPlayer; // Video Player component on the plane
    public VideoClip[] videoClips; // Array of video clips to play

    // Unity Events for play and stop
    public UnityEvent OnPlay; // Triggered when a video starts playing
    public UnityEvent OnStop; // Triggered when a video stops

    private int currentVideoIndex = 0; // Tracks the current video in the array
    private bool isPlaying = false;

    void Start()
    {
        // Ensure the plane is initially hidden
        if (videoPlane != null)
        {
            videoPlane.SetActive(false);
        }
    }

    // Public method to play a specific video by index
    public void PlayVideo(int videoIndex)
    {
        if (videoPlane != null && videoPlayer != null && videoClips.Length > 0 && videoIndex >= 0 && videoIndex < videoClips.Length)
        {
            currentVideoIndex = videoIndex; // Set the starting index
            ActivateVideoPlane();
            PlayCurrentVideo();
            OnPlay?.Invoke(); // Trigger the OnPlay event
        }
    }

    // Public method to play the next video
    public void NextVideo()
    {
        if (isPlaying && videoClips.Length > 0)
        {
            currentVideoIndex = (currentVideoIndex + 1) % videoClips.Length; // Wrap around to first video if at end
            PlayCurrentVideo();
        }
    }

    // Public method to play the previous video
    public void PreviousVideo()
    {
        if (isPlaying && videoClips.Length > 0)
        {
            currentVideoIndex = (currentVideoIndex - 1 + videoClips.Length) % videoClips.Length; // Wrap around to last video if at start
            PlayCurrentVideo();
        }
    }

    // Public method to stop the video
    public void StopVideo()
    {
        if (videoPlane != null && videoPlayer != null)
        {
            videoPlayer.Stop(); // Stop playback
            videoPlane.SetActive(false); // Hide the plane
            isPlaying = false;
            OnStop?.Invoke(); // Trigger the OnStop event
        }
    }

    // Helper method to activate the video plane
    private void ActivateVideoPlane()
    {
        videoPlane.SetActive(true); // Show the plane
        isPlaying = true;
    }

    // Helper method to play the current video
    private void PlayCurrentVideo()
    {
        videoPlayer.Stop(); // Stop any current playback
        videoPlayer.clip = videoClips[currentVideoIndex]; // Set the current clip
        videoPlayer.Play(); // Start playback
    }
}