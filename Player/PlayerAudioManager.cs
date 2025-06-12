using UnityEngine;

public class JumpSoundPlayer : MonoBehaviour
{
    public AudioSource jumpAudioSource; // Reference to the jump AudioSource
    public AudioClip[] jumpSounds; // Array of jump sounds

    // Play a random jump sound
    public void PlayRandomJumpSound()
    {
        if (jumpSounds != null && jumpSounds.Length > 0 && jumpAudioSource != null)
        {
            // Randomly select a jump sound
            int randomIndex = Random.Range(0, jumpSounds.Length);
            jumpAudioSource.PlayOneShot(jumpSounds[randomIndex]); // Play jump sound once
        }
    }
}