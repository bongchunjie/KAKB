using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ThirdPersonShooter : MonoBehaviour 
{
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] private CinemachineVirtualCamera playerZoomCamera;
    [SerializeField] private GameObject cinemachineCameraTarget;
    [SerializeField] private Transform rifleTransform;
    [SerializeField] private Transform rifleNormalRefTransform;
    [SerializeField] private Transform rifleAimRefTransform;
    [SerializeField] private DisableAfterTime particleDisableAfterTime;
    [SerializeField] private DisableAfterTime particleLightDisableAfterTime;
    [SerializeField] private Transform pfBulletProjectileRaycast;
    [SerializeField] private Transform shootPointTransform;
    [SerializeField] private Transform pfSound;
    [SerializeField] private List<AudioClip> shootAudioClipList;
    [SerializeField] private List<AudioClip> rifleShootAudioClipList;
    [SerializeField] private Transform mouseWorldPositionTransform;
    [SerializeField] private LayerMask aimColliderLayerMask;

    private ThirdPersonController thirdPersonController;
    private ThirdPersonShooterInput thirdPersonShooterInput;
    private CinemachineImpulseSource cinemachineImpulseSource;
    private bool isShooting;
    private float shootTimer;
    private float shootTimerMax = .1f;

    private void Awake() {
        thirdPersonController = GetComponent<ThirdPersonController>();
        thirdPersonShooterInput = GetComponent<ThirdPersonShooterInput>();
        cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();

        playerZoomCamera.gameObject.SetActive(false);

        thirdPersonShooterInput.OnAimStarted += OnAimStarted;
        thirdPersonShooterInput.OnAimStopped += OnAimStopped;
        thirdPersonShooterInput.OnShootStarted += OnShootStarted;
        thirdPersonShooterInput.OnShootStopped += OnShootStopped;
    }

    private void Update() {
        HandleMouseWorldPosition();
        HandleShooting();
        HandleAiming();
    }

    private void HandleMouseWorldPosition() {
        mouseWorldPositionTransform.position = Vector3.Lerp(mouseWorldPositionTransform.position, GetMouseWorldPosition(), Time.deltaTime * 20f);
    }

    private Vector3 GetMouseWorldPosition() {
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, aimColliderLayerMask)) {
            return raycastHit.point;
        } else {
            return Vector3.zero;
        }
    }

    private void Shoot() {
        cinemachineImpulseSource.GenerateImpulse();

        Vector3 targetPosition = GetMouseWorldPosition();
        Vector3 aimDir = (targetPosition - shootPointTransform.position).normalized;
        Transform bulletProjectileRaycastTransform =
            Instantiate(pfBulletProjectileRaycast, shootPointTransform.position, Quaternion.LookRotation(aimDir, Vector3.up));

        bulletProjectileRaycastTransform.GetComponent<BulletProjectileRaycast>().Setup(targetPosition);

        Transform soundTransform = Instantiate(pfSound, transform.position, Quaternion.identity);
        soundTransform.GetComponent<AudioSource>().clip = rifleShootAudioClipList[Random.Range(0, rifleShootAudioClipList.Count)];
        soundTransform.GetComponent<AudioSource>().Play();
        Destroy(soundTransform.gameObject, 1f);

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f)) {
            if (raycastHit.collider.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbody)) {
                rigidbody.AddExplosionForce(1000f, targetPosition, 5f);
            }
        }
    }

    private void HandleShooting() {
        if (isShooting) {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f) {
                shootTimer += shootTimerMax + Random.Range(0f, shootTimerMax * .25f);

                if (thirdPersonShooterInput.IsAiming()) {
                    Shoot();
                }
            }
        }
    }

    private void OnShootStopped(object sender, System.EventArgs e) {
        isShooting = false;
    }

    private void OnShootStarted(object sender, System.EventArgs e) {
        isShooting = true;
        shootTimer = shootTimerMax;

        if (thirdPersonShooterInput.IsAiming()) {
            Shoot();
        }
    }

    private void OnAimStopped(object sender, System.EventArgs e) {
        playerZoomCamera.gameObject.SetActive(false);
        thirdPersonController.SetLookSensitivity(normalSensitivity);
        thirdPersonController.SetRotateOnMove(true);
    }

    private void OnAimStarted(object sender, System.EventArgs e) {
        playerZoomCamera.gameObject.SetActive(true);
        thirdPersonController.SetLookSensitivity(aimSensitivity);
        thirdPersonController.SetRotateOnMove(false);
    }

    private void HandleAiming() {
        if (thirdPersonShooterInput.IsAiming()) {

            Vector3 worldAimTarget = GetMouseWorldPosition();
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);

            if (rifleTransform != null) rifleTransform.localEulerAngles = rifleAimRefTransform.localEulerAngles;
        } else {

            if (rifleTransform != null) rifleTransform.localEulerAngles = rifleNormalRefTransform.localEulerAngles;
        }
    }
}