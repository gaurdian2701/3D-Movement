using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerIKController playerIKController;
    [SerializeField] private Animator animator;
    [SerializeField] private float normalSpeed;
    [SerializeField] private float upwardsForce;
    [SerializeField] private float downwardsForce;
    [SerializeField] private float fallingForce;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Camera cam;

    private PlayerStateMachine stateMachine;
    private Vector3 hangPointSurfaceNormal;
    private Vector2 inputData;
    private float currentUpwardsForce;
    private float jumpInput;
    private PlayerStates currentPlayerState;
    private const float groundSphereCastDistance = 0.9f;
    private const float groundSphereCastRadius = 0.3f;
    private const float horizontalGrabOffset = 0.2f;
    private const float verticalGrabOffset = 2.3f;

    public bool isAirborne {  get; private set; }
    public bool isJumping {  get; private set; }
    public bool isHanging {  get; private set; }
    public Vector3 jumpVector { get; private set; }

    public const string speedParam = "Speed";
    public const string hangSpeedParam = "HangSpeed";
    public const string isAboveGroundParam = "IsAboveGround";
    public const string isHangingParam = "IsHanging";
    public const string groundMask = "Ground";

    private void Awake()
    {
        stateMachine = new PlayerStateMachine(this, playerIKController);
        jumpInput = 0f;
        currentUpwardsForce = 0f;
        jumpVector = Vector3.zero;
        Application.targetFrameRate = 60;
    }

    public Animator GetAnimator() => animator;
    public float GetNormalSpeed() => normalSpeed;
    public float GetRotationSpeed() => rotationSpeed;
    public string GetSpeedParam() => speedParam;
    public string GetHangSpeedParam() => hangSpeedParam;
    public string GetHangParam() => isHangingParam;
    public string GetGroundMask() => groundMask;
    public CharacterController GetCharacterController() => characterController;
    public void SetJumpingFlag(bool flag) => isJumping = flag;
    public void SetHangingFlag(bool flag) => isHanging = flag;
    public void SetCurrentPlayerState(PlayerStates state) => currentPlayerState = state;

    private void Update()
    {
        if (currentPlayerState != PlayerStates.HANGING)
        {
            if (currentUpwardsForce < 0f)
                CheckForLedge();
            CheckForFall();
        }
        stateMachine.UpdateStateMachine(inputData);
    }
    public void MovePlayer(InputAction.CallbackContext context) => inputData = context.ReadValue<Vector2>();
    public void Jump(InputAction.CallbackContext context)
    {
        jumpInput = context.ReadValue<float>();
        if (jumpInput == 1 && !isAirborne)
            StartCoroutine(DoJump(PlayerAirborneStates.JUMPING));
    }
    private void SetJumpFlags(PlayerAirborneStates playerState)
    {
        if (playerState == PlayerAirborneStates.FALLING)
            isJumping = false;
        else
            isJumping = true;

        isHanging = false;
        animator.SetBool(isHangingParam, false);
        animator.SetBool(isAboveGroundParam, true);
    }
    private void CheckForFall()
    {
        if (isHanging)
        {
            isAirborne = false;
            return;
        }
        isAirborne = !IsGrounded();
        animator.SetBool(isAboveGroundParam, isAirborne);
        if (isAirborne && !isJumping)
            StartCoroutine(DoJump(PlayerAirborneStates.FALLING));
    }
    private bool IsGrounded()
    {
        RaycastHit hitInfo;
        Vector3 center = characterController.bounds.center;
        return Physics.SphereCast(center, groundSphereCastRadius, -Vector3.up, out hitInfo, groundSphereCastDistance, LayerMask.GetMask(groundMask));
    }
    public Vector3 GetCamForward()
    {
        Vector3 forward = cam.transform.forward;
        forward.y = 0f;
        return forward;
    }

    public Vector3 GetCamRight()
    {
        Vector3 right = cam.transform.right;
        right.y = 0f;
        return right;
    }
 
    private void SetJumpVector(Vector3 jVector) => jumpVector = jVector;

    private IEnumerator DoJump(PlayerAirborneStates playerState)
    {
        if(currentPlayerState != PlayerStates.WALKING)
            stateMachine.ChangeState(PlayerStates.WALKING);

        SetJumpFlags(playerState);
        switch (playerState)
        {
            case PlayerAirborneStates.JUMPING:
                currentUpwardsForce = upwardsForce;
                SetJumpVector(new Vector3(0f, currentUpwardsForce, 0f));
                while (!isAirborne)
                    yield return null;
                while (isAirborne)
                {
                    currentUpwardsForce = currentUpwardsForce > -fallingForce ? currentUpwardsForce - downwardsForce : -fallingForce;
                    SetJumpVector(new Vector3(0f, currentUpwardsForce, 0f));
                    yield return null;
                }
                break;

            default:
            case PlayerAirborneStates.FALLING:
                while (isAirborne)
                {
                    SetJumpVector(new Vector3(0f, -fallingForce, 0f));
                    yield return null;
                }
                break;
        }
        SetJumpVector(Vector3.zero);
        isJumping = false;
    }

    private void CheckForLedge()
    {
        RaycastHit verticalHitInfo;
        RaycastHit horizontalHitInfo;
        Vector3 verticalRayOrigin = transform.position;
        Vector3 horizontalRayOrigin = transform.position;
        horizontalRayOrigin.y += characterController.height - 0.5f;
        verticalRayOrigin.y += characterController.height;
        verticalRayOrigin += transform.forward;
        bool verticalCheck = Physics.Raycast(verticalRayOrigin, -Vector3.up, out verticalHitInfo, 0.5f, LayerMask.GetMask(groundMask));
        bool horizontalCheck = Physics.Raycast(horizontalRayOrigin, transform.forward, out horizontalHitInfo, 0.8f, LayerMask.GetMask(groundMask));

        if(verticalCheck && horizontalCheck)
        {
            Vector3 verticalPoint = verticalHitInfo.point;
            hangPointSurfaceNormal = horizontalHitInfo.normal;
            Vector3 horizontalPoint = horizontalHitInfo.point;
            transform.forward = -hangPointSurfaceNormal;

            //I use the normal of the raycast to determine the axis along which I should apply the offset
            //For e.g. if player grabs ledge with forward vector along the x-axis, I would have to apply the offset along the x-axis
            //I would use the sign of the normal to decide whether to add or subtract the offset accordingly.
            //Same goes for the z-axis.
            transform.position = new Vector3(horizontalPoint.x + horizontalGrabOffset * Mathf.Sign(hangPointSurfaceNormal.x), 
                verticalPoint.y - verticalGrabOffset, 
                horizontalPoint.z + horizontalGrabOffset * Mathf.Sign(hangPointSurfaceNormal.z));
            stateMachine.ChangeState(PlayerStates.HANGING);
        }
    }
}

