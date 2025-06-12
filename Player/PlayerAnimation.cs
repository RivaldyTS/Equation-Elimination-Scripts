using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        // Get the Animator component from the child GameObject
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Reset all animation parameters
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsJumping", false);
        animator.SetBool("WalkForward", false);
        animator.SetBool("WalkBack", false);
        animator.SetBool("WalkFrontLeft", false);
        animator.SetBool("WalkFrontRight", false);
        animator.SetBool("WalkBackLeft", false);
        animator.SetBool("WalkBackRight", false);

        // Check for movement input
        if (Input.GetKey(KeyCode.W))
        {
            if (Input.GetKey(KeyCode.A))
            {
                // Walk Front Left
                animator.SetBool("WalkFrontLeft", true);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                // Walk Front Right
                animator.SetBool("WalkFrontRight", true);
            }
            else
            {
                // Walk Forward
                animator.SetBool("WalkForward", true);
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.A))
            {
                // Walk Back Left
                animator.SetBool("WalkBackLeft", true);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                // Walk Back Right
                animator.SetBool("WalkBackRight", true);
            }
            else
            {
                // Walk Back
                animator.SetBool("WalkBack", true);
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            // Walk Left
            animator.SetBool("WalkFrontLeft", true);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // Walk Right
            animator.SetBool("WalkFrontRight", true);
        }

        // Handle jumping
        if (Input.GetKey(KeyCode.Space))
        {
            animator.SetBool("IsJumping", true);
        }

        // Set IsWalking if any movement key is pressed
        if (Input.GetKey(KeyCode.W))
        {
            animator.SetBool("IsWalking", true);
        }
    }
}