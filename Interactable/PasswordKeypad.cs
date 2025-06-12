using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PasswordKeypad : MonoBehaviour
{
    public string correctPassword = "123123"; // Set the correct password
    private string inputPassword = ""; // Stores the current input

    public TextMeshPro displayText; // Reference to the 3D TextMeshPro object
    public UnityEvent onCorrectPassword; // UnityEvent for correct password
    public UnityEvent onIncorrectPassword; // UnityEvent for incorrect password

    public bool hideInput = true; // Toggle to hide input with asterisks

    [Header("Audio Settings")]
    [SerializeField] private AudioClip correctInputSound; // Sound for correct digit
    [SerializeField] private AudioClip incorrectInputSound; // Sound for incorrect digit
    [SerializeField] private AudioClip allCorrectSound; // Sound for complete correct password
    [SerializeField] private AudioClip failedSound; // Sound for complete incorrect password
    [SerializeField] private float volume = 1f; // Volume control (0 to 1)

    // Call this method from outside scripts to input a number
    public void InputNumber(int number)
    {
        if (number < 1 || number > 3)
        {
            Debug.LogWarning("Invalid input. Only numbers 1, 2, and 3 are allowed.");
            return;
        }

        // Append the number to the input password
        inputPassword += number.ToString();

        // Play sound based on whether this digit is correct
        char inputChar = inputPassword[inputPassword.Length - 1];
        char correctChar = correctPassword.Length > inputPassword.Length - 1 ? correctPassword[inputPassword.Length - 1] : '\0';
        bool isCorrect = inputChar == correctChar;
        PlaySound(isCorrect ? correctInputSound : incorrectInputSound);

        // Update the display text with color feedback
        UpdateDisplay();

        // Check if the input matches the correct password
        if (inputPassword.Length == correctPassword.Length)
        {
            if (inputPassword == correctPassword)
            {
                Debug.Log("Correct Password!");
                onCorrectPassword.Invoke();
                PlaySound(allCorrectSound);
                ResetInput();
            }
            else
            {
                Debug.Log("Incorrect Password!");
                onIncorrectPassword.Invoke();
                PlaySound(failedSound);
                ResetInput();
            }
        }
    }

    // Update the 3D TextMeshPro display with color feedback
    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            string displayedText = "";

            for (int i = 0; i < inputPassword.Length; i++)
            {
                char inputChar = inputPassword[i];
                char correctChar = correctPassword.Length > i ? correctPassword[i] : '\0';

                // Determine if the character is correct
                bool isCorrect = inputChar == correctChar;

                // Wrap the character in a color tag
                string colorTag = isCorrect ? "<color=green>" : "<color=red>";
                displayedText += $"{colorTag}{inputChar}</color>";
            }

            // If hiding input, replace characters with asterisks but keep the colors
            if (hideInput)
            {
                displayedText = displayedText.Replace("1", "*").Replace("2", "*").Replace("3", "*");
            }

            displayText.text = displayedText;
        }
    }

    // Reset the input password (can be called from outside scripts)
    public void ResetInput()
    {
        inputPassword = "";
        UpdateDisplay();
    }

    // Toggle whether to hide the input or not
    public void SetHideInput(bool hide)
    {
        hideInput = hide;
        UpdateDisplay(); // Update the display immediately after changing the setting
    }

    // Method to spawn AudioSource and play sound
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            // Create a new GameObject with an AudioSource
            GameObject soundObject = new GameObject("KeypadSound");
            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            
            // Configure AudioSource
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
            
            // Play the sound
            audioSource.Play();
            
            // Destroy the object after the clip finishes
            Destroy(soundObject, clip.length);
        }
    }
}