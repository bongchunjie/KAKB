using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.InputSystem;

public class CameraSwitch : MonoBehaviour
{
    public Camera FPSCam;
    public Camera TPSCam;

    private ThirdPersonShooterController thirdPersonShooterController;
    private StarterAssetsInputs starterAssetsInputs;

    private void Awake()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        thirdPersonShooterController = GetComponent<ThirdPersonShooterController>();
    }

    private void Update()
    {
        if (starterAssetsInputs.first)
        {
            FPSCam.enabled = true;
            TPSCam.enabled = false;
            thirdPersonShooterController.enabled = false;
        }
        if (starterAssetsInputs.third)
        {
            FPSCam.enabled = false;
            TPSCam.enabled = true;
            thirdPersonShooterController.enabled = true;
        }
    }
}
