using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    [Header("Puzzle Objects")]
    [SerializeField] private PuzzleObject puzzle1; // Reference to the first puzzle object
    [SerializeField] private PuzzleObject puzzle2; // Reference to the second puzzle object

    [Header("UI Settings")]
    [SerializeField] private TMP_Text feedbackText; // UI text to display feedback (e.g., "Access Granted")

    [Header("Text Customization")]
    [SerializeField] private string accessGrantedText = "Access Granted"; // Text for when access is granted
    [SerializeField] private string accessDeniedText = "Access Denied"; // Text for when access is denied
    [SerializeField] private string puzzleResetText = "Puzzle Reset"; // Text for when the puzzle is reset

    [Header("Events")]
    [SerializeField] private UnityEvent onBothPuzzlesSolved; // Triggered when both puzzles are solved
    [SerializeField] private UnityEvent onAccessDenied; // Triggered when access is denied
    [SerializeField] private UnityEvent onPuzzleReset; // Triggered when the puzzle is reset
    [SerializeField] private float delayBeforeSolvedEvent = 1f; // Delay before triggering the solved event
    [SerializeField] private float delayBeforeDeniedEvent = 0.5f; // Delay before triggering the denied event
    [SerializeField] private float delayBeforeResetEvent = 0.5f; // Delay before triggering the reset event

    private bool isPuzzle1Solved = false; // Track if Puzzle 1 is solved
    private bool isPuzzle2Solved = false; // Track if Puzzle 2 is solved
    private bool isPuzzle1Wrong = false; // Track if Puzzle 1 has a wrong key
    private bool isPuzzle2Wrong = false; // Track if Puzzle 2 has a wrong key

    void Start()
    {
        // Hide the feedback text initially
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }

        // Subscribe to the puzzle solved events
        puzzle1.onPuzzleSolved.AddListener(OnPuzzleSolved);
        puzzle2.onPuzzleSolved.AddListener(OnPuzzleSolved);

        // Subscribe to the wrong value events
        puzzle1.OnWrongValue.AddListener(OnWrongValueSubmitted);
        puzzle2.OnWrongValue.AddListener(OnWrongValueSubmitted);
    }

    private void OnPuzzleSolved()
    {
        // Update the solved state of the puzzles
        isPuzzle1Solved = puzzle1.IsSolved();
        isPuzzle2Solved = puzzle2.IsSolved();

        // Check if both puzzles have been interacted with (either solved or wrong)
        if ((isPuzzle1Solved || isPuzzle1Wrong) && (isPuzzle2Solved || isPuzzle2Wrong))
        {
            // Both puzzles have been interacted with, so check the result
            CheckPuzzleResult();
        }
    }

    private void OnWrongValueSubmitted()
    {
        // Update the wrong key state of the puzzles
        isPuzzle1Wrong = puzzle1.HasWrongKey();
        isPuzzle2Wrong = puzzle2.HasWrongKey();

        // Check if both puzzles have been interacted with (either solved or wrong)
        if ((isPuzzle1Solved || isPuzzle1Wrong) && (isPuzzle2Solved || isPuzzle2Wrong))
        {
            // Both puzzles have been interacted with, so check the result
            CheckPuzzleResult();
        }
    }

    private void CheckPuzzleResult()
    {
        // Check if both puzzles are solved
        if (isPuzzle1Solved && isPuzzle2Solved)
        {
            // Both puzzles are solved
            StartCoroutine(DelayBeforeEvent
            (onBothPuzzlesSolved, delayBeforeSolvedEvent, accessGrantedText, Color.green));
        }
        else if (isPuzzle1Wrong || isPuzzle2Wrong)
        {
            // At least one puzzle has a wrong key
            StartCoroutine(DelayBeforeEvent
            (onAccessDenied, delayBeforeDeniedEvent, accessDeniedText, Color.red));

            // Reset the puzzle after denying access
            StartCoroutine(DelayBeforeEvent
            (onPuzzleReset, delayBeforeResetEvent, puzzleResetText, Color.yellow));
            ResetPuzzle();
        }
    }

    private IEnumerator DelayBeforeEvent(UnityEvent unityEvent, float delay, string message, Color color)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Update the feedback text
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
            feedbackText.gameObject.SetActive(true);
        }

        // Trigger the event
        unityEvent.Invoke();
        Debug.Log(message);
    }

    // Public method to reset the puzzle from outside scripts
    public void ResetPuzzle()
    {
        // Reset both puzzles
        puzzle1.ResetPuzzle();
        puzzle2.ResetPuzzle();

        // Reset the solved and wrong key states
        isPuzzle1Solved = false;
        isPuzzle2Solved = false;
        isPuzzle1Wrong = false;
        isPuzzle2Wrong = false;

        Debug.Log("Puzzle Reset: Ready to accept keys again.");
    }

    // Method to update the access granted text dynamically
    public void SetAccessGrantedText(string text)
    {
        accessGrantedText = text;
    }

    // Method to update the access denied text dynamically
    public void SetAccessDeniedText(string text)
    {
        accessDeniedText = text;
    }

    // Method to update the puzzle reset text dynamically
    public void SetPuzzleResetText(string text)
    {
        puzzleResetText = text;
    }
}