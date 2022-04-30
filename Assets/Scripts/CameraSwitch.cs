using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class CameraSwitch : MonoBehaviour
{
    public GameObject cam;
    public GameObject body;
    public Camera FirstPersonCam;
    public Camera ThirdPersonCam;

    private ThirdPersonController thirdPersonController;
    private ThirdPersonShooter thirdPersonShooter;
    private StarterAssetsInputs starterAssetsInputs;
    private InputManager inputManager;
    private PlayerMotor playerMotor;
    private PlayerLook playerLook;

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        thirdPersonShooter = GetComponent<ThirdPersonShooter>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        inputManager = GetComponent<InputManager>();
        playerMotor = GetComponent<PlayerMotor>();  
        playerLook = GetComponent<PlayerLook>();
    }

    private void Update()
    {
        if (starterAssetsInputs.first)
        {
            cam.SetActive(true);
            body.SetActive(false);
            FirstPersonCam.enabled = true;
            ThirdPersonCam.enabled = false;
            thirdPersonController.enabled = false;
            thirdPersonShooter.enabled = false;
            inputManager.enabled = true;
            playerMotor.enabled = true;
            playerLook.enabled = true;
        }
        if (starterAssetsInputs.third)
        {
            body.SetActive(true);
            FirstPersonCam.enabled = false;
            ThirdPersonCam.enabled = true;
            thirdPersonController.enabled = true;
            thirdPersonShooter.enabled = true;
            inputManager.enabled = false;
            playerMotor.enabled = false;
            playerLook.enabled = false;
        }
    }
}
