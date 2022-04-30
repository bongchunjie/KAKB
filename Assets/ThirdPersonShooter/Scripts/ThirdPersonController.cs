using UnityEngine;

public class ThirdPersonController : MonoBehaviour 
{
    [Header("Player")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float rotationSmoothTime;
    [SerializeField] private float speedChangeRate;
    [SerializeField] private float lookSensitivity;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float gravity;
    [SerializeField] private float jumpTimeout;
    [SerializeField] private float fallTimeout;

    [Header("IsGrounded")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private float groundedOffset;
    [SerializeField] private float groundedRadius;
    [SerializeField] private LayerMask groundLayers;

    [Header("Cinemachine")]
    [SerializeField] private GameObject cinemachineCameraTarget;
    [SerializeField] private float topClamp = 70.0f;
    [SerializeField] private float bottomClamp = -30.0f;
    [SerializeField] private float cameraAngleOverride = 0.0f;
    [SerializeField] private bool lockCameraPosition = false;
     
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    private float speed;
    private float animationBlend;
    private float targetRotation = 0.0f;
    private float rotationVelocity;
    private float verticalVelocity;
    private float terminalVelocity = 53.0f;
    private bool jump;

    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;

    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private Animator animator;
    private CharacterController controller;
    private ThirdPersonShooterInput thirdPersonShooterInput;
    private Transform mainCameraTransform;

    private const float threshold = 0.01f;

    private bool hasAnimator;
    private bool rotateOnMove;

    private void Awake() {
        if (mainCameraTransform == null) {
            mainCameraTransform = Camera.main.transform;
        }

        thirdPersonShooterInput = GetComponent<ThirdPersonShooterInput>();
        thirdPersonShooterInput.OnJump += ThirdPersonShooterInput_OnJump;
    }

    private void ThirdPersonShooterInput_OnJump(object sender, System.EventArgs e) {
        jump = true;
    }

    private void Start() {
        hasAnimator = TryGetComponent(out animator);
        controller = GetComponent<CharacterController>();

        AssignAnimationIDs();

        jumpTimeoutDelta = jumpTimeout;
        fallTimeoutDelta = fallTimeout;
    }

    private void Update() {
        hasAnimator = TryGetComponent(out animator);

        JumpAndGravity();
        GroundedCheck();
        Move();
    }

    private void LateUpdate() {
        CameraRotation();
    }

    private void AssignAnimationIDs() {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck() {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

        if (hasAnimator) {
            animator.SetBool(_animIDGrounded, isGrounded);
        }
    }

    private void CameraRotation() {
        if (thirdPersonShooterInput.GetLookVector().sqrMagnitude >= threshold && !lockCameraPosition) {
            cinemachineTargetYaw += thirdPersonShooterInput.GetLookVector().x * Time.deltaTime * lookSensitivity;
            cinemachineTargetPitch += thirdPersonShooterInput.GetLookVector().y * Time.deltaTime * lookSensitivity;
        }
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + cameraAngleOverride, cinemachineTargetYaw, 0.0f);
    }

    private void Move() {
        float targetSpeed = thirdPersonShooterInput.IsSprinting() ? sprintSpeed : moveSpeed;

        if (thirdPersonShooterInput.GetMoveVector() == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = 1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset) {
            speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);

            speed = Mathf.Round(speed * 1000f) / 1000f;
        } else {
            speed = targetSpeed;
        }
        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);

        Vector2 inputMoveVector = thirdPersonShooterInput.GetMoveVector();
        Vector3 inputDirection = new Vector3(inputMoveVector.x, 0.0f, inputMoveVector.y).normalized;

        if (inputMoveVector != Vector2.zero) {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCameraTransform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);

            if (rotateOnMove) {
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

        controller.Move(targetDirection.normalized * (speed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

        if (hasAnimator) {
            animator.SetFloat(_animIDSpeed, animationBlend, .1f, Time.deltaTime);
            animator.SetFloat(_animIDMotionSpeed, inputMagnitude, .1f, Time.deltaTime);
        }
    }

    private void JumpAndGravity() {
        if (isGrounded) {
            fallTimeoutDelta = fallTimeout;

            if (hasAnimator) {
                animator.SetBool(_animIDJump, false);
                animator.SetBool(_animIDFreeFall, false);
            }

            if (verticalVelocity < 0.0f) {
                verticalVelocity = -2f;
            }

            if (jump && jumpTimeoutDelta <= 0.0f) {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (hasAnimator) {
                    animator.SetBool(_animIDJump, true);
                }
            }

            if (jumpTimeoutDelta >= 0.0f) {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        } else {
            jumpTimeoutDelta = jumpTimeout;

            if (fallTimeoutDelta >= 0.0f) {
                fallTimeoutDelta -= Time.deltaTime;
            } else {
                if (hasAnimator) {
                    animator.SetBool(_animIDFreeFall, true);
                }
            }
            jump = false;
        }

        if (verticalVelocity < terminalVelocity) {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax) {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected() {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (isGrounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z), groundedRadius);
    }

    public void SetLookSensitivity(float lookSensitivity) {
        this.lookSensitivity = lookSensitivity;
    }

    public void SetRotateOnMove(bool rotateOnMove) {
        this.rotateOnMove = rotateOnMove;
    }
}