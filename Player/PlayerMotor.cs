using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;
    public float speed = 5f; // Base walking speed
    public float sprintMultiplier = 1.5f; // Speed multiplier when sprinting
    public float gravity = -9.8f;
    public float jumpHeight = 3f; // Normal jump height
    public float jumpBoostMultiplier = 1.5f; // Speed boost multiplier when jumping
    public float airControl = 0.5f; // Control over movement while in the air
    public float crouchHeight = 1f; // Height of the player when crouching
    public float crouchSpeedMultiplier = 0.5f; // Speed multiplier when crouching
    public float standingHeight = 2f; // Normal height of the player

    private Vector3 lastMoveDirection; // Stores the last movement direction for jump boost
    private bool isCrouching = false; // Tracks if the player is crouching
    private bool isSprinting = false; // Tracks if the player is sprinting

    // Public getters for PlayerLook to access
    public bool IsSprinting => isSprinting;
    public bool IsCrouching => isCrouching;

    // Audio
    public AudioSource walkAudioSource; // AudioSource for walking sounds
    public AudioSource crouchAudioSource; // AudioSource for crouch sound
    public JumpSoundPlayer jumpSoundPlayer; // Reference to the JumpSoundPlayer script

    public AudioClip[] walkSounds; // Array of walking sounds
    public AudioClip crouchSound; // Sound for crouching

    [Header("Audio Settings")]
    public float minPitch = 0.8f; // Minimum pitch for slow movement
    public float maxPitch = 1.2f; // Maximum pitch for fast movement
    public float minInterval = 0.4f; // Minimum time between footsteps
    public float maxInterval = 0.7f; // Maximum time between footsteps

    private bool isWalking = false; // Tracks if the player is walking
    private float nextStepTime = 0f; // Tracks when the next footstep should play

    void Start()
    {
        controller = GetComponent<CharacterController>();
        standingHeight = controller.height;
    }

    void Update()
    {
        // Check if the player is grounded
        isGrounded = controller.isGrounded;

        // Get player input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector2 input = new Vector2(moveX, moveZ);

        // Handle sprint input
        isSprinting = Input.GetKey(KeyCode.LeftShift) && isGrounded && !isCrouching;

        // Handle crouch input
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (!isCrouching)
            {
                Crouch();
            }
        }
        else
        {
            if (isCrouching)
            {
                StandUp();
            }
        }

        // Process movement
        ProcessMovement(input);

        // Handle jump input
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        // Reset horizontal velocity when grounded
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // Ensures the player stays grounded
            playerVelocity.x = 0f; // Reset horizontal velocity
            playerVelocity.z = 0f; // Reset horizontal velocity
        }

        // Play walking sound if moving
        if (input.magnitude > 0 && isGrounded && !isCrouching)
        {
            if (!isWalking)
            {
                isWalking = true;
                nextStepTime = Time.time + GetStepInterval(input.magnitude); // Set initial step time
            }

            // Play footsteps at intervals
            if (Time.time >= nextStepTime)
            {
                PlayRandomWalkSound(input.magnitude);
                nextStepTime = Time.time + GetStepInterval(input.magnitude); // Set next step time
            }
        }
        else
        {
            isWalking = false;
            StopWalkSound();
        }
    }

    public void ProcessMovement(Vector2 input)
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;

        // Transform move direction relative to the player's forward direction
        moveDirection = transform.TransformDirection(moveDirection);

        // Store the last movement direction for jump boost
        if (moveDirection.magnitude > 0)
        {
            lastMoveDirection = moveDirection.normalized; // Normalize to ensure consistent boost
        }

        // Apply air control if not grounded
        if (!isGrounded)
        {
            moveDirection *= airControl;
        }

        // Calculate current speed with multipliers
        float currentSpeed = speed;
        if (isCrouching)
        {
            currentSpeed *= crouchSpeedMultiplier;
        }
        else if (isSprinting)
        {
            currentSpeed *= sprintMultiplier;
            // Adjust footstep timing when sprinting
            maxInterval = 0.4f; // Faster footsteps when sprinting
            minInterval = 0.3f;
        }
        else
        {
            // Reset footstep timing to normal when not sprinting
            maxInterval = 0.7f;
            minInterval = 0.4f;
        }

        // Move the player
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Apply gravity
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void Jump()
    {
        if (isGrounded && !isCrouching) // Prevent jumping while crouching
        {
            // Apply normal jump
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // Apply jump boost in the last movement direction
            if (lastMoveDirection.magnitude > 0)
            {
                float jumpSpeed = speed * jumpBoostMultiplier;
                if (isSprinting) // Increase jump boost when sprinting
                {
                    jumpSpeed *= sprintMultiplier;
                }
                playerVelocity.x = lastMoveDirection.x * jumpSpeed;
                playerVelocity.z = lastMoveDirection.z * jumpSpeed;
            }

            // Play jump sound
            if (jumpSoundPlayer != null)
            {
                jumpSoundPlayer.PlayRandomJumpSound();
            }
        }
    }

    private void Crouch()
    {
        isCrouching = true;
        controller.height = crouchHeight; // Reduce the controller's height
        controller.center = new Vector3(0, crouchHeight / 2, 0); // Adjust the center to match the new height

        // Play crouch sound
        if (crouchAudioSource != null && crouchSound != null)
        {
            crouchAudioSource.PlayOneShot(crouchSound); // Play crouch sound once
        }
    }

    private void StandUp()
    {
        // Check if there's enough space to stand up
        if (!Physics.Raycast(transform.position, Vector3.up, standingHeight))
        {
            isCrouching = false;
            controller.height = standingHeight; // Restore the controller's height
            controller.center = Vector3.zero; // Reset the center to (0, 0, 0) for standing height
        }
    }

    // Audio Methods
    private void PlayRandomWalkSound(float movementMagnitude)
    {
        if (walkSounds != null && walkSounds.Length > 0 && walkAudioSource != null)
        {
            // Randomly select a walking sound
            int randomIndex = Random.Range(0, walkSounds.Length);
            walkAudioSource.clip = walkSounds[randomIndex];

            // Adjust pitch based on movement speed
            float pitchMultiplier = isSprinting ? 1.3f : 1f; // Higher pitch when sprinting
            walkAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, movementMagnitude) * pitchMultiplier;

            // Play the sound
            walkAudioSource.Play();
        }
    }

    private void StopWalkSound()
    {
        if (walkAudioSource != null && walkAudioSource.isPlaying)
        {
            walkAudioSource.Stop();
        }
    }

    private float GetStepInterval(float movementMagnitude)
    {
        // Calculate interval based on movement speed (faster movement = shorter interval)
        return Mathf.Lerp(maxInterval, minInterval, movementMagnitude);
    }
}