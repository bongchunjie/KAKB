using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private FirstPersonInput firstPersonInput;
    public FirstPersonInput.PlayerActions player;

    private PlayerMotor motor;
    private PlayerLook look;

    void Awake()
    {
        firstPersonInput = new FirstPersonInput();
        player = firstPersonInput.Player;

        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();

        player.Jump.performed += ctx => motor.Jump();
    }

    void FixedUpdate()
    {
        motor.ProcessMove(player.Movement.ReadValue<Vector2>());
    }

    private void LateUpdate()
    {
        look.ProcessLook(player.Look.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        player.Enable();
    }

    private void OnDisable()
    {
        player.Disable();
    }
}
