using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonShooterInput : MonoBehaviour
{
    public event EventHandler OnJump;
    public event EventHandler OnAimStarted;
    public event EventHandler OnAimStopped;
    public event EventHandler OnShootStarted;
    public event EventHandler OnShootStopped;

    private ThirdPersonShooterInputAsset thirdPersonShooterInputAsset;
    private bool isSprinting;
    private bool isAiming;
    private bool isShooting;

    private void Awake()
    {
        thirdPersonShooterInputAsset = new ThirdPersonShooterInputAsset();
        thirdPersonShooterInputAsset.Player.Enable();
        thirdPersonShooterInputAsset.Player.Jump.performed += Jump_performed;
        thirdPersonShooterInputAsset.Player.Sprint.started += Sprint_started;
        thirdPersonShooterInputAsset.Player.Sprint.canceled += Sprint_canceled;
        thirdPersonShooterInputAsset.Player.Aim.started += Aim_started;
        thirdPersonShooterInputAsset.Player.Aim.canceled += Aim_canceled;
        thirdPersonShooterInputAsset.Player.Shoot.started += Shoot_started;
        thirdPersonShooterInputAsset.Player.Shoot.canceled += Shoot_canceled;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Shoot_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnShootStopped?.Invoke(this, EventArgs.Empty);
        isShooting = false;
    }

    private void Shoot_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnShootStarted?.Invoke(this, EventArgs.Empty);
        isShooting = true;
    }

    private void Aim_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnAimStopped?.Invoke(this, EventArgs.Empty);
        isAiming = false;
    }

    private void Aim_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnAimStarted?.Invoke(this, EventArgs.Empty);
        isAiming = true;
    }

    private void Sprint_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSprinting = false;
    }

    private void Sprint_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSprinting = true;
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnJump?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetLookVector()
    {
        return thirdPersonShooterInputAsset.Player.Look.ReadValue<Vector2>();
    }

    public Vector2 GetMoveVector()
    {
        return thirdPersonShooterInputAsset.Player.Move.ReadValue<Vector2>();
    }

    public bool IsSprinting()
    {
        return isSprinting;
    }

    public bool IsAiming()
    {
        return isAiming;
    }

    public bool IsShooting()
    {
        return isShooting;
    }
}