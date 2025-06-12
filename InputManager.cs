using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    public PlayerInput.OnfootActions onFoot;

    private PlayerMotor motor;
    private PlayerLook look;

    void Awake() 
    {
        playerInput = new PlayerInput();
        onFoot = playerInput.Onfoot;
        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();
        onFoot.Jump.performed += ctx => motor.Jump();
    }

    private void FixedUpdate() 
    {
        motor.ProcessMovement(onFoot.Movement.ReadValue<Vector2>());
    }

    private void LateUpdate() 
    {
        look.ProcessLook(onFoot.Look.ReadValue<Vector2>(), false);
    }

    private void OnEnable() 
    {
        onFoot.Enable();
    }

    private void OnDisable() 
    {
        onFoot.Disable();
    }
}
